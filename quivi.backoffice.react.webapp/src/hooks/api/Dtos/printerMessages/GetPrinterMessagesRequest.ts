import { PagedRequest } from "../PagedRequest";

export interface GetPrinterMessagesRequest extends PagedRequest {
    readonly printerId: string;
}