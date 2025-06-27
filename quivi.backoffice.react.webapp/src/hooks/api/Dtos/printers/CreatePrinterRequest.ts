import { NotificationType } from "../notifications/NotificationType";

export interface CreatePrinterRequest {
    readonly name: string;
    readonly address: string;
    readonly printerWorkerId: string;
    readonly locationId?: string;
    readonly notifications: NotificationType[];
}