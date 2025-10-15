import { PagedRequest } from "../PagedRequest";
import { SalesPeriod } from "./SalesPeriod";

export interface GetSalesRequest extends PagedRequest {
    readonly adminView?: boolean;
    readonly period?: SalesPeriod;
    readonly from?: string;
    readonly to?: string;
}