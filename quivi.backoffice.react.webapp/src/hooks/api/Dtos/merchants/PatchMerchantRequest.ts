import { ChargeMethod } from "../ChargeMethod";
import { FeeUnit } from "./FeeUnit";

export interface PatchMerchantRequest {
    readonly id: string;
    readonly name?: string;
    readonly iban?: string;
    readonly vatNumber?: string;
    readonly vatRate?: string;
    readonly postalCode?: string;
    readonly acceptTermsAndConditions?: boolean;

    readonly transactionFee?: number;
    readonly transactionFeeUnit?: FeeUnit;

    readonly surchargeFee?: number;
    readonly surchargeFeeUnit?: FeeUnit;

    readonly surchargefees?: Record<ChargeMethod, PatchSurcharge>;

    readonly isDemo?: boolean;
    readonly inactive?: boolean;
}

export interface PatchSurcharge {
    readonly fee?: number;
    readonly unit?: FeeUnit;
}