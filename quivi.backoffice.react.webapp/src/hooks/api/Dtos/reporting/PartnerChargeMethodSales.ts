import { ChargePartner } from "../acquirerconfigurations/ChargePartner";
import { ChargeMethod } from "../ChargeMethod";

export interface PartnerChargeMethodSales {
    readonly chargePartner: ChargePartner;
    readonly chargeMethod: ChargeMethod;

    readonly from: string;
    readonly to: string;

    readonly total: number;
    readonly totalSuccess: number;
    readonly totalFailed: number;
    readonly totalProcessing: number;
}