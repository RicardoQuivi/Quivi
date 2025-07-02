export interface CreatePrinterMessageRequest {
    readonly printerId: string;
    readonly text?: string;
    readonly pingOnly: boolean;
}