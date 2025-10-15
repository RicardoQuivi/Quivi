import { PagedRequest } from "../PagedRequest";
import { ProductSalesSortBy } from "./ProductSalesSortBy";
import { SalesPeriod } from "./SalesPeriod";

export interface GetProductSalesRequest extends PagedRequest {
    readonly adminView?: boolean;
    readonly sortBy: ProductSalesSortBy;
    readonly period?: SalesPeriod;
    readonly from?: string;
    readonly to?: string;
}