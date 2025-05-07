import { SessionItem } from "../sessions/SessionItem";

export interface CreateTransactionRequest {
    readonly channelId: string;
    readonly customChargeMethodId: string;
    readonly locationId?: string;
    readonly email?: string;
    readonly vatNumber?: string;
    readonly observations?: string;
    readonly total: number;
    readonly tip: number;
    readonly items?: SessionItem[];
}