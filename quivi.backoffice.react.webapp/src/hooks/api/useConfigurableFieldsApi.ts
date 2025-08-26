import { useMemo } from "react";
import { useAuthenticatedHttpClient } from "../../context/AuthContext";
import { GetConfigurableFieldRequest } from "./Dtos/configurableFields/GetConfigurableFieldsRequest";
import { GetConfigurableFieldsResponse } from "./Dtos/configurableFields/GetConfigurableFieldsResponse";
import { CreateConfigurableFieldResponse } from "./Dtos/configurableFields/CreateConfigurableFieldResponse";
import { PatchConfigurableFieldResponse } from "./Dtos/configurableFields/PatchConfigurableFieldResponse";
import { DeleteConfigurableFieldResponse } from "./Dtos/configurableFields/DeleteConfigurableFieldResponse";
import { DeleteConfigurableFieldRequest } from "./Dtos/configurableFields/DeleteConfigurableFieldRequest";
import { PatchConfigurableFieldRequest } from "./Dtos/configurableFields/PatchConfigurableFieldRequest";
import { CreateConfigurableFieldRequest } from "./Dtos/configurableFields/CreateConfigurableFieldRequest";

export const useConfigurableFieldsApi = () => {   
    const client = useAuthenticatedHttpClient();

    const get = (request: GetConfigurableFieldRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }
        
        request.ids?.forEach((id, index) => queryParams.set(`ids[${index}]`, id));

        const url = new URL(`api/configurablefields?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return client.get<GetConfigurableFieldsResponse>(url, {});
    }

    const create = (request: CreateConfigurableFieldRequest) => {
        const url = new URL(`api/configurablefields`, import.meta.env.VITE_API_URL).toString();
        return client.post<CreateConfigurableFieldResponse>(url, request, {});
    }

    const patch = (request: PatchConfigurableFieldRequest) => {
        const url = new URL(`api/configurablefields/${request.id}`, import.meta.env.VITE_API_URL).toString();
        return client.patch<PatchConfigurableFieldResponse>(url, request, {});
    }

    const deleteField = (request: DeleteConfigurableFieldRequest) => {
        const url = new URL(`api/configurablefields/${request.id}`, import.meta.env.VITE_API_URL).toString();
        return client.delete<DeleteConfigurableFieldResponse>(url, request, {});
    }

    const state = useMemo(() => ({
        get,
        create,
        patch,
        delete: deleteField,
    }), [client]);

    return state;
}