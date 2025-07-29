export interface CreateQuiviViaFacturalusaPosIntegrationRequest {
    readonly accessToken: string;
    readonly skipInvoice: boolean;
    readonly includeTipInInvoice: boolean;
    readonly invoicePrefix?: string;
}