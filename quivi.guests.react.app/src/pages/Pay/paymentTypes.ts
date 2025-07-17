import type { TipsOptionsConfiguration } from "../../helpers/tipsOptions";
import type { SessionItem } from "../../hooks/api/Dtos/sessions/SessionItem";

export interface ExpandedTableItem {
    readonly name: string,
    readonly price: number,
    readonly itemId: string,
    readonly tableItemCount: number,
    readonly isPaid: boolean,
    readonly inputId: string,
}

export enum PaymentType {
    PayAtTheTable = 0,
    OrderAndPay = 1,
}

export interface PayAtTheTableData {
    readonly items: SessionItem[],
}

export interface OrderAndPayData {
    readonly orderId: string;
    readonly scheduledDate: Date | undefined;
}

export type AdditionalData = PayAtTheTableData | OrderAndPayData;

export interface PaymentDetails {
    total: number,
    amount: number,
    tip: number,
    email: string,
    vatNumber: string,
    selectedTip: TipsOptionsConfiguration,
    additionalData: AdditionalData,
}

export interface PaymentDivionDetails {
    readonly selectedItems: string[],
    readonly divideEvenly: {
        readonly peopleAtTheTable: number,
        readonly payForPeople: number,
    }
}

export function orderAndPay(d: AdditionalData): OrderAndPayData | null {
    if (isOrderAndPay(d) == false) {
        return null;
    }
    return d as OrderAndPayData;
}

export function payAtTheTable(d: AdditionalData): PayAtTheTableData | null {
    if (isPayAtTheTable(d) == false) {
        return null;
    }
    return d as PayAtTheTableData;
}

export function isOrderAndPay(d: AdditionalData): boolean {
    return getPropertyName<OrderAndPayData>("orderId") in d;
}

export function isPayAtTheTable(d: AdditionalData): boolean {
    return getPropertyName<PayAtTheTableData>("items") in d;
}

function getPropertyName<T>(name: keyof T) {
    return name;
} 