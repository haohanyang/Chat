import { rest, graphql } from "msw"
import Database from "./database"
import { ChatType, LoginRequest, MessageRequest, UploadResult } from "./models/requests"
import { AddDirectChat, AddSpace, GetChatsResponse, GetDirectChat, GetDirectMessages, GetFriends, GetSpace, GetSpaceMessages } from "./models/graphql"
import { DirectChat } from "./models/chat"
import { Space, SpaceInfo } from "./models/space"
import User from "./models/user"
import { Message } from "./models/message"
import moment from "moment"

const db = new Database()

let base64Url = ""

const restHandlers = [
    rest.get("/api/v1/auth", (_req, res, ctx) => {
        if (sessionStorage.getItem("is-authenticated")) {
            return res(
                ctx.status(200),
                ctx.json(db.user)
            )
        } else {
            return res(
                ctx.status(401),
                ctx.text("Not authorized")
            )
        }
    }),
    rest.post("/api/v1/auth/signin", async (req, res, ctx) => {
        const request = await req.json() as LoginRequest
        if (request.userName === "test" && request.password === "test") {
            sessionStorage.setItem("is-authenticated", "true")
            return res(
                ctx.status(200),
                ctx.text("ok")
            )
        } else {
            return res(
                ctx.status(401),
                ctx.text("Invalid credentials")
            )
        }
    }),
    rest.post("/api/v1/auth/signout", async (_req, res, ctx) => {
        if (sessionStorage.getItem("is-authenticated")) {
            sessionStorage.removeItem("is-authenticated")
            return res(
                ctx.status(200),
                ctx.text("Ok"))
        }
        return res(
            ctx.status(401),
            ctx.text("Not authorized"))
    }),
    rest.post("/api/v1/message", async (req, res, ctx) => {
        const request: MessageRequest = await req.json()
        const message: Message = {
            __typename: "Message",
            id: Database.randomNumber(),
            content: request.content,
            author: db.user,
            timeStamp: moment().format("h:mm A, D MMM"),
            chatId: request.chatId,
            type: request.type,
        }
        if (request.attachmentUploadResult) {
            message.content = request.attachmentUploadResult.fileName
            message.attachment = {
                __typename: request.type == ChatType.Direct ? "DirectMessageAttachment" : "SpaceMessageAttachment",
                blobName: "",
                containerName: "",
                uri: base64Url
            }
            message.attachmentUri = base64Url
        }
        if (request.type === ChatType.Direct) {
            const directChat = db.directChats.get(request.chatId)
            if (!directChat) {
                return res(
                    ctx.status(400),
                    ctx.text("Chat not found")
                )
            }
            directChat.messages.push(message)
        } else {
            const space = db.spaces.get(request.chatId)
            if (!space) {
                return res(
                    ctx.status(400),
                    ctx.text("Space not found")
                )
            }
            space.messages.push(message)
        }
        return res(
            ctx.status(201),
            ctx.json(message))
    }),
    rest.post("/api/v1/file", async (req, res, ctx) => {
        base64Url = req.body as string
        const result: UploadResult = {
            blobName: "",
            fileName: "image.png",
            containerName: "",
        }
        return res(ctx.status(201), ctx.json(result))
    })
]



const graphqlHandlers = [
    graphql.query("GetChats", (_req, res, ctx) => {
        const directChats1 = Array.from(db.directChats.values())
            .filter(x => x.chat.user1.id === db.user.id).map(x => x.chat)
        const directChats2 = Array.from(db.directChats.values())
            .filter(x => x.chat.user2.id === db.user.id).map(x => x.chat)
        const spaces = Array.from(db.spaces.values()).map(x => x.space)
        const response: GetChatsResponse = {
            user: {
                __typename: "User",
                directChats1: directChats1,
                directChats2: directChats2,
                spaceMemberships: spaces.map(x => {
                    return {
                        __typename: "SpaceMembership",
                        space: x,
                        id: x.id
                    }
                }),
                id: db.user.id
            }
        }
        return res(ctx.data(response))
    }),
    graphql.query("GetDirectMessages", (req, res, ctx) => {
        const chatId = req.variables.chatId
        if (!chatId) {
            return res(ctx.errors(["ChatId is required"]))
        }
        const chat = db.directChats.get(chatId)
        if (chat) {
            const response: GetDirectMessages = {
                directMessages: {
                    __typename: "DirectMessagesConnection",
                    edges: chat.messages.map(x => {
                        return {
                            __typename: "DirectMessagesEdge",
                            node: x,
                            cursor: Database.randomNumber().toString()
                        }
                    }),
                    totalCount: chat.messages.length
                }
            }
            return res(ctx.data(response))
        } else {
            return res(ctx.data({
                directMessages: null
            }))
        }
    }),
    graphql.query("GetSpaceMessages", (req, res, ctx) => {
        const spaceId = req.variables.spaceId
        if (!spaceId) {
            return res(ctx.errors(["SpaceId is required"]))
        }
        const space = db.spaces.get(spaceId)
        if (space) {
            const response: GetSpaceMessages = {
                spaceMessages: {
                    __typename: "SpaceMessagesConnection",
                    edges: space.messages.map(x => {
                        return {
                            __typename: "SpaceMessagesEdge",
                            node: x,
                            cursor: Database.randomNumber().toString()
                        }
                    }),
                    totalCount: space.messages.length
                }
            }
            return res(ctx.data(response))
        } else {
            return res(ctx.data({
                spaceMessages: null
            }))
        }
    }),
    graphql.query("GetFriends", (_req, res, ctx) => {
        const response: GetFriends = {
            users: Array.from(db.friends.values())
        }
        return res(ctx.data(response))
    }),
    graphql.query("GetSpace", (req, res, ctx) => {
        const id = req.variables.id
        if (!id) {
            return res(ctx.errors(["Id is required"]))
        }

        const spaceInfo = db.spaces.get(id)
        if (!spaceInfo) {
            return res(ctx.data({
                space: null
            }))
        }
        const space = spaceInfo.space
        const response: GetSpace = {
            space: {
                __typename: "Space",
                id: space.id,
                name: space.name,
                description: space.description,
                updated: space.updated,
                memberships: spaceInfo.members.map(x => {
                    return {
                        __typename: "SpaceMembership",
                        id: Database.randomNumber(),
                        member: x,
                    }
                })
            }
        }
        return res(ctx.data(response))
    }),
    graphql.query("GetDirectChat", (req, res, ctx) => {
        const id = req.variables.id
        if (!id) {
            return res(ctx.errors(["Id is required"]))
        }

        const chatInfo = db.directChats.get(id)
        if (!chatInfo) {
            return res(ctx.data({
                directChat: null
            }))
        }
        const response: GetDirectChat = {
            directChat: chatInfo.chat
        }
        return res(ctx.data(response))
    }),

    graphql.mutation("AddDirectChat", (req, res, ctx) => {
        const _user1Id = req.variables.user1Id
        const _user2Id = req.variables.user2Id
        const errors = []

        if (!_user1Id) {
            errors.push("User1Id is required")
        }

        if (!_user2Id) {
            errors.push("User2Id is required")
        }

        if (errors.length > 0) {
            return res(ctx.errors(errors))
        }

        const user1Id = _user1Id < _user2Id ? _user1Id : _user2Id
        const user2Id = _user1Id < _user2Id ? _user2Id : _user1Id
        const user1 = user1Id === db.user.id ? db.user : db.friends.get(user1Id)
        const user2 = user2Id === db.user.id ? db.user : db.friends.get(user2Id)

        if (!user1) {
            errors.push(`User1 with id ${user1Id} not found`)
        }

        if (!user2) {
            errors.push(`User2 with id ${user2Id} not found`)
        }

        if (errors.length > 0) {
            return res(ctx.errors(errors))
        }

        let chatInfo = db.getDirectChat(user1Id, user2Id)
        if (!chatInfo) {
            const newChat: DirectChat = {
                __typename: "DirectChat",
                id: Database.randomNumber(),
                user1: user1!,
                user2: user2!,
                updated: (new Date()).toISOString(),
            }
            chatInfo = {
                chat: newChat,
                messages: []
            }
            db.directChats.set(newChat.id, chatInfo)
        }

        const response: AddDirectChat = {
            addDirectChat: {
                __typename: "AddDirectChatPayload",
                directChat: chatInfo.chat
            }
        }
        return res(ctx.data(response))
    }),
    graphql.mutation("AddSpace", (req, res, ctx) => {
        const errors = []
        const name = req.variables.name as string
        const description = req.variables.description as string
        const memberIds = req.variables.members as string[]
        if (!name) {
            errors.push("Name is required")
        }
        if (!description) {
            errors.push("Description is required")
        }
        if (!memberIds) {
            errors.push("Members is required")
        }
        if (errors.length > 0) {
            return res(ctx.errors(errors))
        }

        const space: Space = {
            __typename: "Space",
            id: Database.randomNumber(),
            name: name,
            description: description,
            updated: (new Date()).toISOString(),
        }

        const members = memberIds
            .map(id => id == db.user.id ? db.user : db.friends.get(id))
            .filter(x => x != null) as User[]

        const spaceInfo: SpaceInfo = {
            space: space,
            members: members,
            messages: []
        }
        db.spaces.set(space.id, spaceInfo)

        const resposne: AddSpace = {
            addSpace: {
                __typename: "AddSpacePayload",
                space: spaceInfo.space
            }
        }
        return res(ctx.data(resposne))
    })
]

export const handlers = [
    ...restHandlers, ...graphqlHandlers
]
