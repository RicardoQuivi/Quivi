export interface CreateOrdersRequest {
    readonly orders: CreateOrder[];
}

export interface CreateOrder {
    readonly channelId: string;
    readonly items: CreateOrderItem[];
}

export interface BaseCreateOrderItem {
    readonly menuItemId: string;
    readonly quantity: number;
    readonly price?: number;
}

export interface CreateOrderItem extends BaseCreateOrderItem {
    readonly discount: number;
    readonly extras: BaseCreateOrderItem[];
}