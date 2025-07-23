import { useMemo } from "react";
import { GetBackgroundJobResponse } from "./Dtos/backgroundjobs/GetBackgroundJobResponse";
import { GetBackgroundJobRequest } from "./Dtos/backgroundjobs/GetBackgroundJobRequest";
import { useEmployeeHttpClient } from "../../context/employee/EmployeeContextProvider";

export const useBackgroundJobsApi = () => {
    const httpClient = useEmployeeHttpClient();

    const get = async (request: GetBackgroundJobRequest) => {
        const queryParams = new URLSearchParams();
        request.ids.filter(id => !!id).forEach((id, i) => queryParams.set(`ids[${i}]`, id));
        const url = new URL(`api/backgroundJobs?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.get<GetBackgroundJobResponse>(url, {});
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient]);

    return state;
}