import type { CreateOrderItem } from "./CreateOrderRequest";

export interface UpdateOrderRequest {
    readonly id: string;
    readonly items: CreateOrderItem[];
    readonly fields?: Record<string, string>;
}