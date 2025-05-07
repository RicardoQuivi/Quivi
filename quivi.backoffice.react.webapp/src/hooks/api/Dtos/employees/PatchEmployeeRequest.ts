import { EmployeeRestriction } from "./Employee";

export interface PatchEmployeeRequest {
    readonly id: string;
    readonly name?: string;
    readonly inactivityLogoutTimeout?: string | null;
    readonly restrictions?: EmployeeRestriction[];
}