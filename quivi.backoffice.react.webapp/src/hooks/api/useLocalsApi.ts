import { useMemo } from "react";
import { GetLocalsRequest } from "./Dtos/locals/GetLocalsRequest";
import { GetLocalsResponse } from "./Dtos/locals/GetLocalsResponse";
import { CreateLocalRequest } from "./Dtos/locals/CreateLocalRequest";
import { CreateLocalResponse } from "./Dtos/locals/CreateLocalResponse";
import { PatchLocalRequest } from "./Dtos/locals/PatchLocalRequest";
import { PatchLocalResponse } from "./Dtos/locals/PatchLocalResponse";
import { DeleteLocalRequest } from "./Dtos/locals/DeleteLocalRequest";
import { DeleteLocalResponse } from "./Dtos/locals/DeleteLocalResponse";
import { useAuthenticatedHttpClient } from "../../context/AuthContext";

export const useLocalsApi = () => {
    const client = useAuthenticatedHttpClient();

    const get = (request: GetLocalsRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }
        request.ids?.forEach((id, index) => queryParams.set(`ids[${index}]`, id));

        const url = new URL(`api/locals?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return client.get<GetLocalsResponse>(url, {});
    }

    const create = (request: CreateLocalRequest) => {
        const url = new URL(`api/locals`, import.meta.env.VITE_API_URL).toString();
        return client.post<CreateLocalResponse>(url, request, {});
    }

    const patch = (request: PatchLocalRequest) => {
        const url = new URL(`api/locals/${request.id}`, import.meta.env.VITE_API_URL).toString();
        return client.patch<PatchLocalResponse>(url, request, {});
    }

    const deleteLocal = (request: DeleteLocalRequest) => {
        const url = new URL(`api/locals/${request.id}`, import.meta.env.VITE_API_URL).toString();
        return client.delete<DeleteLocalResponse>(url, request, {});
    }

    const state = useMemo(() => ({
        get,
        create,
        patch,
        delete: deleteLocal,
    }), [client]);

    return state;
}