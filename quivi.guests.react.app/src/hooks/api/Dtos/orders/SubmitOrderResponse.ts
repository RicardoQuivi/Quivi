import type { DataResponse } from "../DataResponse";
import type { Order } from "./Order";

export interface SubmitOrderResponse extends DataResponse<Order> {
    readonly jobId: string;
}