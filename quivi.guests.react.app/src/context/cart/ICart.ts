import type { Order } from "../../hooks/api/Dtos/orders/Order";
import type { ICartItem } from "./ICartItem";
import type { ISchedulerChanger } from "./ISchedulerChanger";
import type { IBaseItem } from "./item";

export interface ICart {
    readonly isInitializing: boolean;
    readonly items: ICartItem[];
    readonly total: number;
    readonly totalItems: number;
    readonly scheduledDate: Date | undefined;
    readonly fields: Record<string, string>;

    addItem(item: IBaseItem | ICartItem): void;
    updateItem(oldItem: ICartItem, newItem: ICartItem): void;
    removeItem(item: IBaseItem, allQuantity?: boolean): void;
    getQuantityInCart(item: IBaseItem | ICartItem, exact: boolean): number;
    editFields(fieldsMap: Record<string, string>): void;
    submit(payLater?: boolean): Promise<Order>;
    setScheduleDate(date?: Date): Promise<ISchedulerChanger>
}