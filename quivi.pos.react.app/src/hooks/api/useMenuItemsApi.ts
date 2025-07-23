import { useMemo } from "react";
import { GetMenuItemsRequest } from "./Dtos/menuitems/GetMenuItemsRequest";
import { GetMenuItemsResponse } from "./Dtos/menuitems/GetMenuItemsResponse";
import { useEmployeeHttpClient } from "../../context/employee/EmployeeContextProvider";

export const useMenuItemsApi = () => {
    const httpClient = useEmployeeHttpClient();

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
        return httpClient.get<GetMenuItemsResponse>(url, {});
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient]);

    return state;
}