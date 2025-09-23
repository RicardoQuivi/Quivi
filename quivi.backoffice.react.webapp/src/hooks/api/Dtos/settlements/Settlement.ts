export interface Settlement {
    readonly id: string;
    readonly date: string;
    readonly amount: number;
    readonly tip: number;
    readonly services?: number;
}