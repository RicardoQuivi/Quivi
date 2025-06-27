import { NotificationType } from "../notifications/NotificationType";

export interface Printer {
    readonly id: string;
    readonly address: string;
    readonly name: string;
    readonly printerWorkerId: string;
    readonly locationId?: string;
    readonly notifications: NotificationType[];
}