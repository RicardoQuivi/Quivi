import { ChargePartner } from "../acquirerconfigurations/ChargePartner";
import { ChargeMethod } from "../ChargeMethod";
import { PagedRequest } from "../PagedRequest";
import { SalesPeriod } from "./SalesPeriod";

export interface GetPartnerChargeMethodSalesRequest extends PagedRequest {
    readonly adminView?: boolean;
    readonly chargePartners?: ChargePartner[];
    readonly chargeMethods?: ChargeMethod[];
    readonly period?: SalesPeriod;
    readonly from?: string;
    readonly to?: string;
}