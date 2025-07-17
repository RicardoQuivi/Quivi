export interface OrderItem extends BaseOrderItem {
    readonly modifiers?: OrderItemModifierGroup[];
}

export interface OrderItemModifierGroup {
    readonly id: string;
    readonly selectedOptions: BaseOrderItem[];
}

export interface BaseOrderItem {
    readonly id: string;
    readonly name: string;
    readonly amount: number;
    readonly quantity: number;
}
