import { ChargeMethod } from "../ChargeMethod";
import { PagedRequest } from "../PagedRequest";

export interface GetSettlementsRequest extends PagedRequest {
    readonly ids?: string[];
    readonly chargeMethod?: ChargeMethod;
}