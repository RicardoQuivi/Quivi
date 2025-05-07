export enum EmployeeRestriction {
    RemoveItems,
    ApplyDiscounts,
    OnlyOwnTransactions,
    OnlyTransactionsOfLast24Hours,
    TransferingItems,
    SessionsAccess,
    Refunds,
}

export interface Employee {
    readonly id: string;
    readonly name: string;
    readonly hasPinCode: boolean;
    readonly inactivityLogoutTimeout?: string;
    readonly restrictions: EmployeeRestriction[];
}