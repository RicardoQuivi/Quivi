import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { GetMenuCategoriesRequest } from "./Dtos/menuCategories/GetMenuCategoriesRequest";
import { GetMenuCategoriesResponse } from "./Dtos/menuCategories/GetMenuCategoriesResponse";
import { CreateMenuCategoryRequest } from "./Dtos/menuCategories/CreateMenuCategoryRequest";
import { CreateMenuCategoryResponse } from "./Dtos/menuCategories/CreateMenuCategoryResponse";
import { PatchMenuCategoryRequest } from "./Dtos/menuCategories/PatchMenuCategoryRequest";
import { PatchMenuCategoryResponse } from "./Dtos/menuCategories/PatchMenuCategoryResponse";
import { DeleteMenuCategoryRequest } from "./Dtos/menuCategories/DeleteMenuCategoryResquest";
import { DeleteMenuCategoryResponse } from "./Dtos/menuCategories/DeleteMenuCategoryResponse";
import { SortMenuCategoriesResponse } from "./Dtos/menuCategories/SortMenuCategoriesResponse";
import { SortMenuCategoriesRequest } from "./Dtos/menuCategories/SortMenuCategoriesRequest";

export const useMenuCategoriesApi = (token?: string) => {
    const httpClient = useHttpClient();
 
    const get = (request: GetMenuCategoriesRequest) => {
        const queryParams = new URLSearchParams();
        request.ids?.forEach((id, index) => queryParams.set(`ids[${index}]`, id));

        let url = `${import.meta.env.VITE_API_URL}api/menucategories?${queryParams}`;
        return httpClient.httpGet<GetMenuCategoriesResponse>(url, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const create = (request: CreateMenuCategoryRequest) => {
        return httpClient.httpPost<CreateMenuCategoryResponse>(`${import.meta.env.VITE_API_URL}api/menucategories`, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const patch = (request: PatchMenuCategoryRequest) => {
        return httpClient.httpPatch<PatchMenuCategoryResponse>(`${import.meta.env.VITE_API_URL}api/menucategories/${request.id}`, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const deleteCategory = (request: DeleteMenuCategoryRequest) => {
        return httpClient.httpDelete<DeleteMenuCategoryResponse>(`${import.meta.env.VITE_API_URL}api/menucategories/${request.id}`, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const sort = (request: SortMenuCategoriesRequest) => {
        return httpClient.httpPut<SortMenuCategoriesResponse>(`${import.meta.env.VITE_API_URL}api/menucategories/sort`, request, {
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