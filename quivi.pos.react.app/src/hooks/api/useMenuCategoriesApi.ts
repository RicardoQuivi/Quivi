import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { GetMenuCategoriesRequest } from "./Dtos/menucategories/GetMenuCategoriesRequest";
import { GetMenuCategoriesResponse } from "./Dtos/menucategories/GetMenuCategoriesResponse";

export const useMenuCategoriesApi = (token: string) => {
    const httpClient = useHttpClient();

    const get = async (request: GetMenuCategoriesRequest) => {
        const queryParams = new URLSearchParams();
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }

        queryParams.set("page", request.page.toString());
        request.ids?.forEach((id, i) => queryParams.set(`ids[${i}]`, id));

        if(request.hasItems != undefined) {
            queryParams.set("hasItems", request.hasItems == true ? "true" : "false");
        }
        
        let url = `${import.meta.env.VITE_API_URL}api/menucategories?${queryParams}`;
        return httpClient.httpGet<GetMenuCategoriesResponse>(url, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient, token]);

    return state;
}