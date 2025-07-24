import { FeeUnit } from "./FeeUnit";

export interface MerchantFee {
    readonly fee: number;
    readonly unit: FeeUnit;
}