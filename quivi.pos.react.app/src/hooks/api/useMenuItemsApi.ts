import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { GetMenuItemsRequest } from "./Dtos/menuitems/GetMenuItemsRequest";
import { GetMenuItemsResponse } from "./Dtos/menuitems/GetMenuItemsResponse";

export const useMenuItemsApi = (token: string) => {
    const httpClient = useHttpClient();

    const get = async (request: GetMenuItemsRequest) => {
        const queryParams = new URLSearchParams();
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }

        queryParams.set("page", request.page.toString());
        request.ids?.forEach((id, i) => queryParams.set(`ids[${i}]`, id));

        if(request.menuCategoryId != undefined) {
            queryParams.set("menuCategoryId", request.menuCategoryId);
        }

        if(request.search != undefined) {
            queryParams.set("search", request.search);
        }

        if(request.includeDeleted == true) {
            queryParams.set("includeDeleted", "true");
        }

        const url = new URL(`api/menuitems?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpGet<GetMenuItemsResponse>(url, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient, token]);

    return state;
}