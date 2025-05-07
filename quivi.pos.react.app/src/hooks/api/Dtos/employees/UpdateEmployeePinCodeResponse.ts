import { DataResponse } from "../DataResponse";
import { Employee } from "./Employee";

export interface UpdateEmployeePinCodeResponse extends DataResponse<Employee | undefined> {
}