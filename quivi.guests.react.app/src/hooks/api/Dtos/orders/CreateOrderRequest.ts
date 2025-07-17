export interface CreateOrderBaseItem {
    readonly menuItemId: string;
    readonly quantity: number;
}

export interface CreateOrderItemModifier {
    readonly modifierId: string;
    readonly selectedOptions: CreateOrderBaseItem[];
}

export interface CreateOrderItem extends CreateOrderBaseItem {
    readonly modifierGroups: CreateOrderItemModifier[];
}

export interface CreateOrderRequest {
    readonly merchantId: string;
    readonly channelId: string;
    readonly payLater: boolean;
    readonly items: CreateOrderItem[];
}