import { useMemo } from "react";
import { GetPreparationGroupsRequest } from "./Dtos/preparationgroups/GetPreparationGroupsRequest";
import { GetPreparationGroupsResponse } from "./Dtos/preparationgroups/GetPreparationGroupsResponse";
import { CommitPreparationGroupRequest } from "./Dtos/preparationgroups/CommitPreparationGroupRequest";
import { CommitPreparationGroupResponse } from "./Dtos/preparationgroups/CommitPreparationGroupResponse";
import { PatchPreparationGroupRequest } from "./Dtos/preparationgroups/PatchPreparationGroupRequest";
import { PatchPreparationGroupResponse } from "./Dtos/preparationgroups/PatchPreparationGroupResponse";
import { PrintCommitedPreparationGroupRequest } from "./Dtos/preparationgroups/PrintCommitedPreparationGroupRequest";
import { PrintCommitedPreparationGroupResponse } from "./Dtos/preparationgroups/PrintCommitedPreparationGroupResponse";
import { useEmployeeHttpClient } from "../../context/employee/EmployeeContextProvider";

export const usePreparationGroupsApi = () => {
    const httpClient = useEmployeeHttpClient();

    const get = async (request: GetPreparationGroupsRequest) => {
        const queryParams = new URLSearchParams();
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }
        queryParams.set("page", request.page.toString());

        request.sessionIds?.forEach((id, i) => queryParams.set(`sessionIds[${i}]`, id));

        if(request.locationId != undefined) {
            queryParams.set('locationId', request.locationId);
        }
        if(request.isCommited != undefined) {
            queryParams.set('IsCommited', request.isCommited ? "true" : "false")
        }

        const url = new URL(`api/preparationGroups?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.get<GetPreparationGroupsResponse>(url, {});
    }

    const commit = async (request: CommitPreparationGroupRequest) => {
        const url = new URL(`api/preparationGroups/${request.id}`, import.meta.env.VITE_API_URL).toString();
        return await httpClient.put<CommitPreparationGroupResponse>(url, request, {});
    }

    const patch = async (request: PatchPreparationGroupRequest): Promise<PatchPreparationGroupResponse> => {
        const url = new URL(`api/preparationGroups/${request.id}`, import.meta.env.VITE_API_URL).toString();
        return await httpClient.patch<PatchPreparationGroupResponse>(url, request, {});
    }

    const print = async (request: PrintCommitedPreparationGroupRequest): Promise<PrintCommitedPreparationGroupResponse> => {
        const url = new URL(`api/preparationGroups/${request.id}/print`, import.meta.env.VITE_API_URL).toString();
        return await httpClient.post<PatchPreparationGroupResponse>(url, request, {});
    }

    const state = useMemo(() => ({
        get,
        commit,
        patch,
        print,
    }), [httpClient]);

    return state;
}