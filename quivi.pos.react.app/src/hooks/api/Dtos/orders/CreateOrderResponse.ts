import { DataResponse } from "../DataResponse";
import { Order } from "./Order";

export interface CreateOrdersResponse extends DataResponse<Order[]> {
    readonly jobId?: string;
}