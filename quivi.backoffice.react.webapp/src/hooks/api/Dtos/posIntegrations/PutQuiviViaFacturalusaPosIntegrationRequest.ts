export interface PutQuiviViaFacturalusaPosIntegrationRequest {
    readonly id: string;
    readonly accessToken: string;
    readonly skipInvoice: boolean;
    readonly includeTipInInvoice: boolean;
    readonly invoicePrefix?: string;
}