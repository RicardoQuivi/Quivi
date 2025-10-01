import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { useTranslation } from "react-i18next";
import type { GetMenuCategoriesRequest } from "./Dtos/menuCategories/GetMenuCategoriesRequest";
import type { GetMenuCategoriesResponse } from "./Dtos/menuCategories/GetMenuCategoriesResponse";

export const useMenuCategoriesApi = () => {
    const httpClient = useHttpClient();
    const { i18n } = useTranslation();

    const get = (request: GetMenuCategoriesRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }

        queryParams.set("channelId", request.channelId);

        if(request.atDate != undefined) {
            queryParams.set("atDate", request.atDate.toISOString());
        }
        
        const url = new URL(`api/menuCategories?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.get<GetMenuCategoriesResponse>(url, {
            'Accept-Language': i18n.language,
        });
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient, i18n, i18n.language]);

    return state;
}