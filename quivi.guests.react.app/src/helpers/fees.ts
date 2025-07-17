import type { Fee } from "../hooks/api/Dtos/merchants/Fee";
import { FeeUnit } from "../hooks/api/Dtos/merchants/FeeUnit";
import type { ChargeMethod } from "../hooks/api/Dtos/ChargeMethod";

export class Fees {
    static getAmountWithFee = (fees: Fee[], amount: number, method: ChargeMethod) => {
        const fee = fees.find(f => f.chargeMethod == method);
        if(fee == undefined) {
            return 0;
        }

        switch(fee.unit)
        {
            case FeeUnit.Absolute: return fee.value;
            case FeeUnit.Percentage: return amount * (100 + fee.value) / 100.0;
            case FeeUnit.PercentageFraction: return amount * (1 + fee.value);
        }
    }
}