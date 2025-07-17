import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { GetModifierGroupsRequest } from "./Dtos/modifierGroups/GetModifierGroupsRequest";
import { GetModifierGroupsResponse } from "./Dtos/modifierGroups/GetModifierGroupsResponse";
import { CreateModifierGroupResponse } from "./Dtos/modifierGroups/CreateModifierGroupResponse";
import { CreateModifierGroupRequest } from "./Dtos/modifierGroups/CreateModifierGroupRequest";
import { PatchModifierGroupResponse } from "./Dtos/modifierGroups/PatchModifierGroupResponse";
import { PatchModifierGroupRequest } from "./Dtos/modifierGroups/PatchModifierGroupRequest";
import { DeleteModifierGroupResponse } from "./Dtos/modifierGroups/DeleteModifierGroupResponse";
import { DeleteModifierGroupRequest } from "./Dtos/modifierGroups/DeleteModifierGroupRequest";

export const useModifierGroupsApi = (token?: string) => {
    const httpClient = useHttpClient();
 
    const get = (request: GetModifierGroupsRequest) => {
        const queryParams = new URLSearchParams();
        
        request.ids?.forEach((id, index) => queryParams.set(`ids[${index}]`, id));
        request.menuItemIds?.forEach((id, index) => queryParams.set(`menuItemIds[${index}]`, id));

        const url = new URL(`api/modifiergroups?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpGet<GetModifierGroupsResponse>(url, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const create = (request: CreateModifierGroupRequest) => {
        const url = new URL(`api/modifiergroups`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpPost<CreateModifierGroupResponse>(url, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const patch = (request: PatchModifierGroupRequest) => {
        const url = new URL(`api/modifiergroups/${request.id}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpPatch<PatchModifierGroupResponse>(url, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const deleteItem = (request: DeleteModifierGroupRequest) => {
        const url = new URL(`api/modifiergroups/${request.id}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpDelete<DeleteModifierGroupResponse>(url, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const state = useMemo(() => ({
        get,
        create,
        patch,
        delete: deleteItem,
    }), [httpClient, token]);

    return state;
}