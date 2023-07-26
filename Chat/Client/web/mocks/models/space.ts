import { Message } from "./message"
import User from "./user"

export interface SpaceInfo {
    space: Space,
    messages: Message[],
    members: User[],
}

export interface Space {
    __typename: string,
    id: number,
    name: string,
    updated: string,
    description: string,
}