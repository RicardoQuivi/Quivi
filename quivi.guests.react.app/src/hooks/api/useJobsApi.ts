import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { useTranslation } from "react-i18next";
import type { GetJobsRequest } from "./Dtos/jobs/GetJobsRequest";
import type { GetJobsResponse } from "./Dtos/jobs/GetJobsResponse";

export const useJobsApi = () => {
    const httpClient = useHttpClient();
    const { i18n } = useTranslation();

    const get = (request: GetJobsRequest) => {
        const queryParams = new URLSearchParams();
        request.ids.filter(id => !!id).forEach((id, i) => queryParams.set(`ids[${i}]`, id));       
        const url = new URL(`api/jobs?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.get<GetJobsResponse>(url, {
            'Accept-Language': i18n.language,
        });
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient, i18n, i18n.language]);

    return state;
}