export interface PatchMerchantRequest {
    readonly id: string;
    readonly name?: string;
    readonly iban?: string;
    readonly vatNumber?: string;
    readonly vatRate?: string;
    readonly postalCode?: string;
    readonly transactionFee?: number;
    readonly acceptTermsAndConditions?: boolean;
    readonly isDemo?: boolean;
    readonly inactive?: boolean;
}