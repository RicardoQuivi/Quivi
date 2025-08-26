import type { DataResponse } from "../DataResponse";
import type { OrderField } from "./OrderField";

export interface GetOrderFieldsResponse extends DataResponse<OrderField[]> {
}