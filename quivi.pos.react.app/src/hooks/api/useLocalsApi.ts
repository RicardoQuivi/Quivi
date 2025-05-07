import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { GetLocalsRequest } from "./Dtos/locals/GetLocalsRequest";
import { GetLocalsResponse } from "./Dtos/locals/GetLocalsResponse";

export const useLocalsApi = (token: string) => {
    const httpClient = useHttpClient();

    const get = async (_: GetLocalsRequest) => {
        let url = `${import.meta.env.VITE_API_URL}api/locations`;
        return httpClient.httpGet<GetLocalsResponse>(url, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient, token]);

    return state;
}