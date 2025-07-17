import type { Fee } from "./Fee";

export interface Merchant {
    readonly id: string;
    readonly name: string;
    readonly logoUrl: string;
    readonly surchargeFeesMayApply: boolean;
    readonly freePayment: boolean;
    readonly itemSelectionPayment: boolean;
    readonly splitBillPayment: boolean;
    readonly enforceTip: boolean;
    readonly allowsIgnoreBill: boolean;
    readonly showPaymentNotes: boolean;
    readonly fees: Fee[];
    readonly inactive: boolean;
}