import { useMemo } from "react";
import { useAuthenticatedHttpClient } from "../../context/AuthContext";
import { GetAvailabilitiesRequest } from "./Dtos/availabilities/GetAvailabilitiesRequest";
import { GetAvailabilitiesResponse } from "./Dtos/availabilities/GetAvailabilitiesResponse";
import { CreateAvailabilityRequest } from "./Dtos/availabilities/CreateAvailabilityRequest";
import { CreateAvailabilityResponse } from "./Dtos/availabilities/CreateAvailabilityResponse";
import { PatchAvailabilityResponse } from "./Dtos/availabilities/PatchAvailabilityResponse";
import { DeleteAvailabilityRequest } from "./Dtos/availabilities/DeleteAvailabilityRequest";
import { DeleteAvailabilityResponse } from "./Dtos/availabilities/DeleteAvailabilityResponse";
import { PatchAvailabilityRequest } from "./Dtos/availabilities/PatchAvailabilityRequest";

export const useAvailabilitiesApi = () => {
    const client = useAuthenticatedHttpClient();

    const get = (request: GetAvailabilitiesRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }
        request.ids?.forEach((id, index) => queryParams.set(`ids[${index}]`, id));

        const url = new URL(`api/availabilities?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return client.get<GetAvailabilitiesResponse>(url, {});
    }

    const create = (request: CreateAvailabilityRequest) => {
        const url = new URL(`api/availabilities`, import.meta.env.VITE_API_URL).toString();
        return client.post<CreateAvailabilityResponse>(url, request, {});
    }

    const patch = (request: PatchAvailabilityRequest) => {
        const url = new URL(`api/availabilities/${request.id}`, import.meta.env.VITE_API_URL).toString();
        return client.patch<PatchAvailabilityResponse>(url, request, {});
    }

    const deleteAvailability = (request: DeleteAvailabilityRequest) => {
        const url = new URL(`api/availabilities/${request.id}`, import.meta.env.VITE_API_URL).toString();
        return client.delete<DeleteAvailabilityResponse>(url, request, {});
    }

    const state = useMemo(() => ({
        get,
        create,
        patch,
        delete: deleteAvailability,
    }), [client]);

    return state;
}