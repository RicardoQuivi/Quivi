import type { OrderItem } from "../../hooks/api/Dtos/orders/orderItem";

export interface ISchedulerChanger {
    readonly date: Date | undefined;
    readonly unavailableItems: OrderItem[];
    confirm(): Promise<void>;
}