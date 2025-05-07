import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { GetBackgroundJobResponse } from "./Dtos/backgroundjobs/GetBackgroundJobResponse";
import { GetBackgroundJobRequest } from "./Dtos/backgroundjobs/GetBackgroundJobRequest";

export const useBackgroundJobsApi = (token: string) => {
    const httpClient = useHttpClient();

    const get = async (request: GetBackgroundJobRequest) => {
        const queryParams = new URLSearchParams();
        request.ids.filter(id => !!id).forEach((id, i) => queryParams.set(`ids[${i}]`, id));
        let url = `${import.meta.env.VITE_API_URL}api/backgroundJobs?${queryParams.toString()}`;
        return httpClient.httpGet<GetBackgroundJobResponse>(url, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient, token]);

    return state;
}