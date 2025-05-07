import { Operation } from "./Operation";

export interface OnMerchantAssociatedEvent {
    readonly operation: Operation;
    readonly merchantId: string;
    readonly userId: string;
}