import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { GetPreparationGroupsRequest } from "./Dtos/preparationgroups/GetPreparationGroupsRequest";
import { GetPreparationGroupsResponse } from "./Dtos/preparationgroups/GetPreparationGroupsResponse";
import { CommitPreparationGroupRequest } from "./Dtos/preparationgroups/CommitPreparationGroupRequest";
import { CommitPreparationGroupResponse } from "./Dtos/preparationgroups/CommitPreparationGroupResponse";
import { PatchPreparationGroupRequest } from "./Dtos/preparationgroups/PatchPreparationGroupRequest";
import { PatchPreparationGroupResponse } from "./Dtos/preparationgroups/PatchPreparationGroupResponse";
import { PrintCommitedPreparationGroupRequest } from "./Dtos/preparationgroups/PrintCommitedPreparationGroupRequest";
import { PrintCommitedPreparationGroupResponse } from "./Dtos/preparationgroups/PrintCommitedPreparationGroupResponse";

export const usePreparationGroupsApi = (token: string) => {
    const httpClient = useHttpClient();

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

        let url = `${import.meta.env.VITE_API_URL}api/preparationGroups?${queryParams}`;
        return httpClient.httpGet<GetPreparationGroupsResponse>(url, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const commit = async (request: CommitPreparationGroupRequest) => {
        return await httpClient.httpPut<CommitPreparationGroupResponse>(`${import.meta.env.VITE_API_URL}api/preparationGroups/${request.id}`, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const patch = async (request: PatchPreparationGroupRequest): Promise<PatchPreparationGroupResponse> => {
        return await httpClient.httpPatch<PatchPreparationGroupResponse>(`${import.meta.env.VITE_API_URL}api/preparationGroups/${request.id}`, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const print = async (request: PrintCommitedPreparationGroupRequest): Promise<PrintCommitedPreparationGroupResponse> => {
        return await httpClient.httpPost<PatchPreparationGroupResponse>(`${import.meta.env.VITE_API_URL}api/preparationGroups/${request.id}/print`, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const state = useMemo(() => ({
        get,
        commit,
        patch,
        print,
    }), [httpClient, token]);

    return state;
}