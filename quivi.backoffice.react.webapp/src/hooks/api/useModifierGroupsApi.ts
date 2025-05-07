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

        let url = `${import.meta.env.VITE_API_URL}api/modifiergroups?${queryParams}`;
        return httpClient.httpGet<GetModifierGroupsResponse>(url, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const create = (request: CreateModifierGroupRequest) => {
        return httpClient.httpPost<CreateModifierGroupResponse>(`${import.meta.env.VITE_API_URL}api/modifiergroups`, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const patch = (request: PatchModifierGroupRequest) => {
        return httpClient.httpPatch<PatchModifierGroupResponse>(`${import.meta.env.VITE_API_URL}api/modifiergroups/${request.id}`, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const deleteItem = (request: DeleteModifierGroupRequest) => {
        return httpClient.httpDelete<DeleteModifierGroupResponse>(`${import.meta.env.VITE_API_URL}api/modifiergroups/${request.id}`, request, {
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