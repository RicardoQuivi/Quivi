import type { PagedResponse } from "../PagedResponse";
import type { Order } from "./Order";

export interface GetOrdersResponse extends PagedResponse<Order> {
    
}