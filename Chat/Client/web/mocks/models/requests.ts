export interface LoginRequest {
    userName: string
    password: string
}

export interface MessageRequest {
    authorId: string
    content: string
    type: ChatType
    chatId: number
    attachmentUploadResult?: UploadResult
}

export enum ChatType {
    Direct = 0,
    Group = 1
}

export interface UploadResult {
    fileName: string
    containerName: string
    blobName: string
}
