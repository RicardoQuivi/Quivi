import type { OrderExtraCost } from "./OrderExtraCost";
import type { OrderItem } from "./OrderItem";
import type { OrderState } from "./OrderState";
import type { OrderType } from "./OrderType";

export interface OrderChangeLog {
    readonly state: OrderState;
    readonly note: string;
}

export interface OrderFieldValue {
    readonly id: string;
    readonly value: string;
}

export interface Order {
    readonly id: string;
    readonly sequenceNumber: string;
    readonly scheduledTo?: string;
    readonly merchantId: string;
    readonly channelId: string;
    readonly state: OrderState;
    readonly type: OrderType;
    readonly items: OrderItem[];
    readonly lastModified: string;
    readonly extraCosts: OrderExtraCost[];
    readonly changes: OrderChangeLog[];
    readonly fields: OrderFieldValue[];
}