import { useMemo } from "react";
import { useEmployeeHttpClient } from "../../context/employee/EmployeeContextProvider";
import { GetSessionAdditionalInformationsRequest } from "./Dtos/sessionAdditionalInformations/GetSessionAdditionalInformationsRequest";
import { GetSessionAdditionalInformationsResponse } from "./Dtos/sessionAdditionalInformations/GetSessionAdditionalInformationsResponse";
import { UpsertSessionAdditionalInformationsRequest } from "./Dtos/sessionAdditionalInformations/UpsertSessionAdditionalInformationsRequest";
import { UpsertSessionAdditionalInformationsResponse } from "./Dtos/sessionAdditionalInformations/UpsertSessionAdditionalInformationsResponse";

export const useSessionAdditionalInformationsApi = () => {
    const httpClient = useEmployeeHttpClient();

    const get = async (request: GetSessionAdditionalInformationsRequest) => {
        const url = new URL(`api/sessions/${request.sessionId}/additionalinfo`, import.meta.env.VITE_API_URL).toString();
        return httpClient.get<GetSessionAdditionalInformationsResponse>(url, {});
    }

    const post = async (request: UpsertSessionAdditionalInformationsRequest) => {
        const url = new URL(`api/sessions/${request.sessionId}/additionalinfo`, import.meta.env.VITE_API_URL).toString();
        return httpClient.post<UpsertSessionAdditionalInformationsResponse>(url, request, {});
    }

    const state = useMemo(() => ({
        get,
        post,
    }), [httpClient]);

    return state;
}