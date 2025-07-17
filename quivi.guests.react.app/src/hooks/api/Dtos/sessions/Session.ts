import type { SessionItem } from "./SessionItem";

export interface Session {
    readonly id: string;
    readonly channelId: string;
    readonly items: SessionItem[];
}