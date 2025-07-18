import { SynchronizationState } from "./Transaction";

export interface ATransactionsRequest {
    readonly fromDate?: string;
    readonly toDate?: string;
    readonly syncState?: SynchronizationState;
    readonly overPayed?: boolean;
    readonly hasDiscounts?: boolean;
    readonly hasReviewComment?: boolean;
    readonly search?: string;
    readonly adminView?: boolean;
    readonly customChargeMethodId?: string;
    readonly quiviPaymentsOnly?: boolean;
    readonly hasRefunds?: boolean;
}