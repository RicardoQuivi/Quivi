import { useMemo } from "react";
import { GetModifierGroupsRequest } from "./Dtos/modifierGroups/GetModifierGroupsRequest";
import { GetModifierGroupsResponse } from "./Dtos/modifierGroups/GetModifierGroupsResponse";
import { CreateModifierGroupResponse } from "./Dtos/modifierGroups/CreateModifierGroupResponse";
import { CreateModifierGroupRequest } from "./Dtos/modifierGroups/CreateModifierGroupRequest";
import { PatchModifierGroupResponse } from "./Dtos/modifierGroups/PatchModifierGroupResponse";
import { PatchModifierGroupRequest } from "./Dtos/modifierGroups/PatchModifierGroupRequest";
import { DeleteModifierGroupResponse } from "./Dtos/modifierGroups/DeleteModifierGroupResponse";
import { DeleteModifierGroupRequest } from "./Dtos/modifierGroups/DeleteModifierGroupRequest";
import { useAuthenticatedHttpClient } from "../../context/AuthContext";

export const useModifierGroupsApi = () => {
    const client = useAuthenticatedHttpClient();
    
    const get = (request: GetModifierGroupsRequest) => {
        const queryParams = new URLSearchParams();
        
        request.ids?.forEach((id, index) => queryParams.set(`ids[${index}]`, id));
        request.menuItemIds?.forEach((id, index) => queryParams.set(`menuItemIds[${index}]`, id));

        const url = new URL(`api/modifiergroups?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return client.get<GetModifierGroupsResponse>(url, {});
    }

    const create = (request: CreateModifierGroupRequest) => {
        const url = new URL(`api/modifiergroups`, import.meta.env.VITE_API_URL).toString();
        return client.post<CreateModifierGroupResponse>(url, request, {});
    }

    const patch = (request: PatchModifierGroupRequest) => {
        const url = new URL(`api/modifiergroups/${request.id}`, import.meta.env.VITE_API_URL).toString();
        return client.patch<PatchModifierGroupResponse>(url, request, {});
    }

    const deleteItem = (request: DeleteModifierGroupRequest) => {
        const url = new URL(`api/modifiergroups/${request.id}`, import.meta.env.VITE_API_URL).toString();
        return client.delete<DeleteModifierGroupResponse>(url, request, {});
    }

    const state = useMemo(() => ({
        get,
        create,
        patch,
        delete: deleteItem,
    }), [client]);

    return state;
}