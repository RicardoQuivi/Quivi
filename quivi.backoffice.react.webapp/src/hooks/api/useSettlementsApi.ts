import { useMemo } from "react";
import { useAuthenticatedHttpClient } from "../../context/AuthContext";
import { GetSettlementsRequest } from "./Dtos/settlements/GetSettlementsRequest";
import { GetSettlementsResponse } from "./Dtos/settlements/GetSettlementsResponse";

function delay(ms: number): Promise<void> {
  return new Promise(resolve => setTimeout(resolve, ms));
}
export const useSettlementsApi = () => {
    const client = useAuthenticatedHttpClient();

    const get = async (request: GetSettlementsRequest): Promise<GetSettlementsResponse> => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }


        await delay(500);
        return {
            data: [],
            page: 0,
            totalItems: 0,
            totalPages: 0,
        }
        // const url = new URL(`api/settlements?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        // return client.get<GetSettlementsResponse>(url, {});
    }

    const state = useMemo(() => ({
        get,
    }), [client]);

    return state;
}