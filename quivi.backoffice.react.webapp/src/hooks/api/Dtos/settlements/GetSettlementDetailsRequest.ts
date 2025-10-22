import { ChargeMethod } from "../ChargeMethod";
import { PagedRequest } from "../PagedRequest";

export interface GetSettlementDetailsRequest extends PagedRequest {
    readonly settlementId: string;
    readonly chargeMethod?: ChargeMethod;
}