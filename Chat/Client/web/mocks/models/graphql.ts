import { DirectChat } from "./chat"
import { Message } from "./message"
import { Space } from "./space"
import User from "./user"

export interface GetChatsResponse {
    user: {
        __typename: string,
        directChats1: DirectChat[],
        directChats2: DirectChat[],
        spaceMemberships: {
            __typename: string,
            space: Space,
            id: number,
        }[],
        id: string,
    }
}

export interface GetDirectMessages {
    directMessages: {
        __typename: string,
        edges: {
            __typename: string,
            node: Message,
            cursor: string,
        }[],
        totalCount: number
    }
}

export interface GetSpaceMessages {
    spaceMessages: {
        __typename: string,
        edges: {
            __typename: string,
            node: Message,
            cursor: string,
        }[],
        totalCount: number
    }
}

export interface GetFriends {
    users: User[]
}

export interface GetDirectChat {
    directChat: DirectChat
}

export interface GetSpace {
    space: {
        __typename: string,
        id: number,
        name: string,
        description: string,
        updated: string,
        memberships: {
            __typename: string,
            id: number,
            member: User,
        }[]
    }
}

export interface AddDirectChat {
    addDirectChat: {
        __typename: string,
        directChat: DirectChat
    }
}

export interface AddSpace {
    addSpace: {
        __typename: string,
        space: Space
    }
}