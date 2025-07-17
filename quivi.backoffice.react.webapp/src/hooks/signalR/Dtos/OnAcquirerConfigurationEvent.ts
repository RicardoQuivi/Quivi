import { Operation } from "./Operation";

export interface OnAcquirerConfigurationEvent {
    readonly operation: Operation;
    readonly id: string;
    readonly merchantId: string;
}