import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { GetMenuItemsRequest } from "./Dtos/menuItems/GetMenuItemsRequest";
import { GetMenuItemsResponse } from "./Dtos/menuItems/GetMenuItemsResponse";
import { CreateMenuItemResponse } from "./Dtos/menuItems/CreateMenuItemResponse";
import { CreateMenuItemRequest } from "./Dtos/menuItems/CreateMenuItemRequest";
import { PatchMenuItemResponse } from "./Dtos/menuItems/PatchMenuItemResponse";
import { PatchMenuItemRequest } from "./Dtos/menuItems/PatchMenuItemRequest";
import { DeleteMenuItemResponse } from "./Dtos/menuItems/DeleteMenuItemResponse";
import { DeleteMenuItemRequest } from "./Dtos/menuItems/DeleteMenuItemRequest";

export const useMenuItemsApi = (token?: string) => {
    const httpClient = useHttpClient();
 
    const get = (request: GetMenuItemsRequest) => {
        const queryParams = new URLSearchParams();
        
        request.ids?.forEach((id, index) => queryParams.set(`ids[${index}]`, id));
        if(request.itemCategoryId != undefined) {
            queryParams.set(`itemCategoryId`, request.itemCategoryId)
        }
        if(request.search != undefined) {
            queryParams.set(`search`, request.search)
        }

        if(request.hasCategory != undefined) {
            queryParams.set(`hasCategory`, request.hasCategory ? "true" : "false")
        }

        let url = `${import.meta.env.VITE_API_URL}api/menuitems?${queryParams}`;
        return httpClient.httpGet<GetMenuItemsResponse>(url, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const create = (request: CreateMenuItemRequest) => {
        return httpClient.httpPost<CreateMenuItemResponse>(`${import.meta.env.VITE_API_URL}api/menuitems`, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const patch = (request: PatchMenuItemRequest) => {
        return httpClient.httpPatch<PatchMenuItemResponse>(`${import.meta.env.VITE_API_URL}api/menuitems/${request.id}`, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const deleteItem = (request: DeleteMenuItemRequest) => {
        return httpClient.httpDelete<DeleteMenuItemResponse>(`${import.meta.env.VITE_API_URL}api/menuitems/${request.id}`, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const state = useMemo(() => ({
        get,
        create,
        patch,
        delete: deleteItem,
    }), [httpClient, token]);

    return state;
}