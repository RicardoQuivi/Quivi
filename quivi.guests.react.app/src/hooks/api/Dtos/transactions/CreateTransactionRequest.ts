import type { SessionItem } from "../sessions/SessionItem";

interface PayAtTheTableData {
    readonly items?: SessionItem[];
}

interface OrderAndPayData {
    readonly orderId: string;
}

export interface CreateTransactionRequest {
    readonly channelId: string;
    readonly amount: number;
    readonly tip: number;
    readonly email?: string;
    readonly vatNumber?: string;
    readonly merchantAcquirerConfigurationId: string;
    readonly payAtTheTableData?: PayAtTheTableData;
    readonly orderAndPayData?: OrderAndPayData
}