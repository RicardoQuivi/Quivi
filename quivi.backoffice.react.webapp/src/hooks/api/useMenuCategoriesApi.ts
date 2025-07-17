import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { GetMenuCategoriesRequest } from "./Dtos/menuCategories/GetMenuCategoriesRequest";
import { GetMenuCategoriesResponse } from "./Dtos/menuCategories/GetMenuCategoriesResponse";
import { CreateMenuCategoryRequest } from "./Dtos/menuCategories/CreateMenuCategoryRequest";
import { CreateMenuCategoryResponse } from "./Dtos/menuCategories/CreateMenuCategoryResponse";
import { PatchMenuCategoryRequest } from "./Dtos/menuCategories/PatchMenuCategoryRequest";
import { PatchMenuCategoryResponse } from "./Dtos/menuCategories/PatchMenuCategoryResponse";
import { DeleteMenuCategoryRequest } from "./Dtos/menuCategories/DeleteMenuCategoryRequest";
import { DeleteMenuCategoryResponse } from "./Dtos/menuCategories/DeleteMenuCategoryResponse";
import { SortMenuCategoriesResponse } from "./Dtos/menuCategories/SortMenuCategoriesResponse";
import { SortMenuCategoriesRequest } from "./Dtos/menuCategories/SortMenuCategoriesRequest";

export const useMenuCategoriesApi = (token?: string) => {
    const httpClient = useHttpClient();
 
    const get = (request: GetMenuCategoriesRequest) => {
        const queryParams = new URLSearchParams();
        request.ids?.forEach((id, index) => queryParams.set(`ids[${index}]`, id));

        const url = new URL(`api/menucategories?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpGet<GetMenuCategoriesResponse>(url, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const create = (request: CreateMenuCategoryRequest) => {
        const url = new URL(`api/menucategories`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpPost<CreateMenuCategoryResponse>(url, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const patch = (request: PatchMenuCategoryRequest) => {
        const url = new URL(`api/menucategories/${request.id}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpPatch<PatchMenuCategoryResponse>(url, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const deleteCategory = (request: DeleteMenuCategoryRequest) => {
        const url = new URL(`api/menucategories/${request.id}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpDelete<DeleteMenuCategoryResponse>(url, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const sort = (request: SortMenuCategoriesRequest) => {
        const url = new URL(`api/menucategories/sort`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpPut<SortMenuCategoriesResponse>(url, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const state = useMemo(() => ({
        get,
        create,
        patch,
        delete: deleteCategory,
        sort,
    }), [httpClient, token]);

    return state;
}