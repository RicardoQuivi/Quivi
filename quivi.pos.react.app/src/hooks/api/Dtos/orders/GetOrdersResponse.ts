import { PagedResponse } from "../PagedResponse";
import { Order } from "./Order";

export interface GetOrdersResponse extends PagedResponse<Order> {
    
}