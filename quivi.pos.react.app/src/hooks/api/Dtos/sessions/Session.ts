import { SessionItem } from "./SessionItem";

export interface Session {
    readonly id: string;
    readonly isOpen: boolean;
    readonly channelId: string;
    readonly employeeId?: string;
    readonly items: SessionItem[];
    readonly closedDate?: string;
    readonly isDeleted: boolean;
}