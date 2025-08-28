import { ChargeMethod } from "../ChargeMethod";
import { FeeUnit } from "./FeeUnit";
import { MerchantFee } from "./MerchantFee";

export interface Merchant {
    readonly id: string;
    readonly parentId?: string;
    readonly name: string;
    readonly vatNumber: string;
    readonly logoUrl: string;
    readonly setUpFee?: number;

    readonly transactionFee?: number;
    readonly transactionFeeUnit?: FeeUnit;

    readonly surchargeFee?: number;
    readonly surchargeFeeUnit?: FeeUnit;

    readonly surchargeFees: Record<ChargeMethod, MerchantFee>;
}