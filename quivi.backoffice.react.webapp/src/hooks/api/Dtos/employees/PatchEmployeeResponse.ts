import { DataResponse } from "../DataResponse";
import { Employee } from "./Employee";

export interface PatchEmployeeResponse extends DataResponse<Employee | undefined> {
    
}