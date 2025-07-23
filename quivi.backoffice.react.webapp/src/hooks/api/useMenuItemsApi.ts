import { useMemo } from "react";
import { GetMenuItemsRequest } from "./Dtos/menuItems/GetMenuItemsRequest";
import { GetMenuItemsResponse } from "./Dtos/menuItems/GetMenuItemsResponse";
import { CreateMenuItemResponse } from "./Dtos/menuItems/CreateMenuItemResponse";
import { CreateMenuItemRequest } from "./Dtos/menuItems/CreateMenuItemRequest";
import { PatchMenuItemResponse } from "./Dtos/menuItems/PatchMenuItemResponse";
import { PatchMenuItemRequest } from "./Dtos/menuItems/PatchMenuItemRequest";
import { DeleteMenuItemResponse } from "./Dtos/menuItems/DeleteMenuItemResponse";
import { DeleteMenuItemRequest } from "./Dtos/menuItems/DeleteMenuItemRequest";
import { useAuthenticatedHttpClient } from "../../context/AuthContext";

export const useMenuItemsApi = () => {
    const client = useAuthenticatedHttpClient();

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

        const url = new URL(`api/menuitems?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return client.get<GetMenuItemsResponse>(url, {});
    }

    const create = (request: CreateMenuItemRequest) => {
        const url = new URL(`api/menuitems`, import.meta.env.VITE_API_URL).toString();
        return client.post<CreateMenuItemResponse>(url, request, {});
    }

    const patch = (request: PatchMenuItemRequest) => {
        const url = new URL(`api/menuitems/${request.id}`, import.meta.env.VITE_API_URL).toString();
        return client.patch<PatchMenuItemResponse>(url, request, {});
    }

    const deleteItem = (request: DeleteMenuItemRequest) => {
        const url = new URL(`api/menuitems/${request.id}`, import.meta.env.VITE_API_URL).toString();
        return client.delete<DeleteMenuItemResponse>(url, request, {});
    }

    const state = useMemo(() => ({
        get,
        create,
        patch,
        delete: deleteItem,
    }), [client]);

    return state;
}