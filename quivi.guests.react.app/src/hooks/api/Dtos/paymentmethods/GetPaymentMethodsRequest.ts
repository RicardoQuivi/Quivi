import type { PagedRequest } from "../PagedRequest";

export interface GetPaymentMethodsRequest extends PagedRequest {
    readonly channelId: string;
}