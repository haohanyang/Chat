import { ChatType } from "./requests"
import User from "./user"

export interface Attachment {
    __typename: string
    containerName: string
    blobName: string
    uri: string
}

export interface Message {
    __typename: string
    id: number
    author: User
    content: string
    timeStamp: string
    type: ChatType
    attachment?: Attachment
    attachmentUri?: string
    chatId: number
}