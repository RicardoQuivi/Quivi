export interface Printer {
    readonly id: string;
    readonly name: string;
    readonly printConsumerInvoice: boolean;
    readonly printConsumerBill: boolean;
    readonly sendOrdersToToKitchen: boolean;
    readonly canOpenCashDrawer: boolean;
    readonly canPrintCloseDayTotals: boolean;
}