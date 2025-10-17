import { ExportFileType } from "../ExportFileType";

interface ExportSalesLabels {
    readonly date: string;
    readonly transactionId: string;
    readonly invoice: string;
    readonly id: string;
    readonly method: string;
    readonly menuId: string;
    readonly item: string;
    readonly unitPrice: string;
    readonly quantity: string;
    readonly total: string;
}
export interface ExportSalesRequest {
    readonly type: ExportFileType;
    readonly labels: ExportSalesLabels;
    readonly from?: string;
    readonly to?: string;
}