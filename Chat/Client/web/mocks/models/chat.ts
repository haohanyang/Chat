import { Message } from "./message"
import User from "./user"

export interface DirectChatInfo {
    chat: DirectChat
    messages: Message[]
}

export interface DirectChat {
    __typename: string,
    id: number,
    updated: string,
    user1: User,
    user2: User,
}