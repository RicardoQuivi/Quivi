import { PagedResponse } from "../PagedResponse";
import { Employee } from "./Employee";

export interface GetEmployeesResponse extends PagedResponse<Employee> {

}