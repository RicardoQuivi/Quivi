import { useMemo } from "react";
import { GetMenuCategoriesRequest } from "./Dtos/menucategories/GetMenuCategoriesRequest";
import { GetMenuCategoriesResponse } from "./Dtos/menucategories/GetMenuCategoriesResponse";
import { useEmployeeHttpClient } from "../../context/employee/EmployeeContextProvider";

export const useMenuCategoriesApi = () => {
    const httpClient = useEmployeeHttpClient();

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
        
        if(request.search != undefined) {
            queryParams.set("search", request.search);
        }

        const url = new URL(`api/menucategories?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.get<GetMenuCategoriesResponse>(url, {});
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient]);

    return state;
}