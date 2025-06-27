import { NotificationType } from "../notifications/NotificationType";

export interface PatchPrinterRequest {
    readonly id: string;
    readonly name?: string;
    readonly address?: string;
    readonly printerWorkerId?: string;
    readonly locationId?: string | null;
    readonly notifications?: NotificationType[];
}