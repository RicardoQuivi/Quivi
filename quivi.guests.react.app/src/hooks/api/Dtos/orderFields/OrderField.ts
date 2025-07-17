import type { OrderFieldType } from "./OrderFieldType";

export interface OrderField {
    readonly id: string;
    readonly name: string;
    readonly isRequired: boolean;
    readonly defaultValue?: string;
    readonly type: OrderFieldType;
}