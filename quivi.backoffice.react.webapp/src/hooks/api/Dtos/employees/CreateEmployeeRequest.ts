import { EmployeeRestriction } from "./Employee";

export interface CreateEmployeeRequest {
    readonly name: string;
    readonly inactivityLogoutTimeout?: string;
    readonly restrictions: EmployeeRestriction[];
}