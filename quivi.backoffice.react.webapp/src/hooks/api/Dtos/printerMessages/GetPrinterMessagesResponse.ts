import { PagedResponse } from "../PagedResponse";
import { PrinterMessage } from "./PrinterMessage";

export interface GetPrinterMessagesResponse extends PagedResponse<PrinterMessage> {
}