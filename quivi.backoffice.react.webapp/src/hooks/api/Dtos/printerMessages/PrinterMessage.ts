import { PrinterMessageStatus } from "./PrinterMessageStatus";
import { NotificationType } from "../notifications/NotificationType";

export interface PrinterMessage {
    readonly printerId: string;
    readonly messageId: string;
    readonly type: NotificationType;
    readonly statuses: Record<PrinterMessageStatus, string>;
}