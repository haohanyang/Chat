import { fakerSV as faker } from "@faker-js/faker"
import User from "./models/user"
import { DirectChat, DirectChatInfo } from "./models/chat"
import { Space, SpaceInfo } from "./models/space"
import { Message } from "./models/message"
import { ChatType } from "./models/requests"

export default class Database {
    user: User
    friends: Map<string, User> = new Map()
    directChats: Map<number, DirectChatInfo> = new Map()
    spaces: Map<number, SpaceInfo> = new Map()

    constructor() {
        this.user = Database.randomUser()
        const friends = Array.from({ length: 30 }, () => Database.randomUser())
        for (let i = 0; i < friends.length; i++) {
            this.friends.set(friends[i].id, friends[i])
        }

        for (let i = 0; i < 30 / 2; i++) {
            const friend = friends[i]
            const chat: DirectChat = {
                __typename: "DirectChat",
                id: Database.randomNumber(),
                updated: faker.date.past().toISOString(),
                user1: this.user.id < friend.id ? this.user : friend,
                user2: this.user.id < friend.id ? friend : this.user,
            }
            const messages: Message[] = []
            for (let j = 0; j < 30; j++) {
                const author = faker.number.int({ min: 0, max: 1 }) % 2 === 0 ? this.user : friend
                const message: Message = {
                    __typename: "DirectMessage",
                    id: Database.randomNumber(),
                    author: author,
                    content: faker.lorem.paragraph(),
                    timeStamp: faker.date.past().toISOString(),
                    chatId: chat.id,
                    type: ChatType.Direct
                }

                if (faker.number.int({ min: 0, max: 4 }) === 0) {
                    message.content = "image.jpg"
                    const imageUrl = faker.image.url()
                    message.attachment = {
                        __typename: "DirectMessageAttachment",
                        blobName: "",
                        containerName: "",
                        uri: imageUrl,
                    }
                    message.attachmentUri = imageUrl
                }
                messages.push(message)
            }

            this.directChats.set(chat.id, {
                chat: chat,
                messages: messages
            })
        }

        for (let i = 0; i < 10; i++) {
            const memberIds: Set<string> = new Set([this.user.id])
            const members: User[] = [this.user]

            const space: Space = {
                __typename: "Space",
                id: Database.randomNumber(),
                name: faker.company.name(),
                updated: faker.date.past().toISOString(),
                description: faker.lorem.paragraph(),
            }

            for (let j = 0; j < 10; j++) {
                const member = friends[faker.number.int({ min: 0, max: friends.length - 1 })]
                if (!memberIds.has(member.id)) {
                    memberIds.add(member.id)
                    members.push(member)
                }
            }

            const messages: Message[] = []
            for (let j = 0; j < 30; j++) {
                const author = members[faker.number.int({ min: 0, max: members.length - 1 })]

                const message: Message = {
                    __typename: "SpaceMessage",
                    id: Database.randomNumber(),
                    author: author,
                    content: faker.lorem.paragraph(),
                    timeStamp: faker.date.past().toISOString(),
                    chatId: space.id,
                    type: ChatType.Group
                }

                if (faker.number.int({ min: 0, max: 4 }) === 0) {
                    const imageUrl = faker.image.url()
                    message.content = "image.jpg"
                    message.attachment = {
                        __typename: "SpaceMessageAttachment",
                        blobName: "",
                        containerName: "",
                        uri: imageUrl,
                    }
                    message.attachmentUri = imageUrl
                }

                messages.push(message)
            }
            this.spaces.set(space.id, {
                space: space,
                messages: messages,
                members: members,
            })
        }
    }

    getDirectChat(user1Id: string, user2Id: string): DirectChatInfo | null {
        for (const info of this.directChats.values()) {
            if (info.chat.user1.id === user1Id && info.chat.user2.id === user2Id) {
                return info
            }
        }
        return null
    }

    static randomUser(): User {
        return {
            __typename: "User",
            id: faker.string.uuid(),
            userName: faker.internet.userName(),
            firstName: faker.person.firstName(),
            lastName: faker.person.lastName(),
            avatar: faker.image.avatar(),
            bio: faker.lorem.paragraph(),
        }
    }

    static randomNumber(): number {
        return faker.number.int({ min: 0, max: 1000000 })
    }
}