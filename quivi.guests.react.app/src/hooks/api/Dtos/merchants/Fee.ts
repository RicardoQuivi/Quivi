import type { ChargeMethod } from "../ChargeMethod";
import type { FeeType } from "./FeeType";
import type { FeeUnit } from "./FeeUnit";

export interface Fee {
    readonly feeType: FeeType;
    readonly chargeMethod: ChargeMethod;
    readonly value: number;
    readonly unit: FeeUnit;
}