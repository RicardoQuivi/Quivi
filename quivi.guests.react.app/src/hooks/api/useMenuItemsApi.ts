import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { useTranslation } from "react-i18next";
import type { GetMenuItemsRequest } from "./Dtos/menuItems/GetMenuItemsRequest";
import type { GetMenuItemsResponse } from "./Dtos/menuItems/GetMenuItemsResponse";

export const useMenuItemsApi = () => {
    const httpClient = useHttpClient();
    const { i18n } = useTranslation();

    const get = (request: GetMenuItemsRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }

        queryParams.set("channelId", request.channelId);
        request.ids?.forEach((id, i) => queryParams.set(`ids[${i}]`, id));

        if(request.menuItemCategoryId != undefined) {
            queryParams.set("menuItemCategoryId", request.menuItemCategoryId);
        }
        if(request.ignoreCalendarAvailability == true) {
            queryParams.set("ignoreCalendarAvailability", "true");
        }
        if(request.atDate != undefined) {
            queryParams.set("atDate", request.atDate.toISOString());
        }
        
        const url = new URL(`api/menuItems?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpGet<GetMenuItemsResponse>(url, {
            'Accept-Language': i18n.language,
        });
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient, i18n, i18n.language]);

    return state;
}