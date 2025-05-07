export interface CreateMerchantRequest {
    readonly fiscalName: string;
    readonly vatNumber: string;
    readonly name: string;
    readonly logoUrl: string;
    readonly postalCode: string;
    readonly iban: string;
    readonly ibanProofUrl: string;
}