import { DataResponse } from "./DataResponse";

export interface PagedResponse<TData> extends DataResponse<TData[]> {
    readonly page: number;
    readonly totalPages: number;
    readonly totalItems: number;
}