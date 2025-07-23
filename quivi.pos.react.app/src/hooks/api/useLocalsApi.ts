import { useMemo } from "react";
import { GetLocalsRequest } from "./Dtos/locals/GetLocalsRequest";
import { GetLocalsResponse } from "./Dtos/locals/GetLocalsResponse";
import { useEmployeeHttpClient } from "../../context/employee/EmployeeContextProvider";

export const useLocalsApi = () => {
    const httpClient = useEmployeeHttpClient();

    const get = async (_: GetLocalsRequest) => {
        const url = new URL(`api/locations`, import.meta.env.VITE_API_URL).toString();
        return httpClient.get<GetLocalsResponse>(url, {});
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient]);

    return state;
}