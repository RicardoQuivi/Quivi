import { DataResponse } from "../DataResponse";

export interface ExportSalesResponse extends DataResponse<string> {
    readonly name: string;
}