import { useMemo } from "react";
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
import { useAuthenticatedHttpClient } from "../../context/AuthContext";

export const useMenuCategoriesApi = () => {
    const client = useAuthenticatedHttpClient();
    
    const get = (request: GetMenuCategoriesRequest) => {
        const queryParams = new URLSearchParams();
        request.ids?.forEach((id, index) => queryParams.set(`ids[${index}]`, id));

        const url = new URL(`api/menucategories?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return client.get<GetMenuCategoriesResponse>(url, {});
    }

    const create = (request: CreateMenuCategoryRequest) => {
        const url = new URL(`api/menucategories`, import.meta.env.VITE_API_URL).toString();
        return client.post<CreateMenuCategoryResponse>(url, request, {});
    }

    const patch = (request: PatchMenuCategoryRequest) => {
        const url = new URL(`api/menucategories/${request.id}`, import.meta.env.VITE_API_URL).toString();
        return client.patch<PatchMenuCategoryResponse>(url, request, {});
    }

    const deleteCategory = (request: DeleteMenuCategoryRequest) => {
        const url = new URL(`api/menucategories/${request.id}`, import.meta.env.VITE_API_URL).toString();
        return client.delete<DeleteMenuCategoryResponse>(url, request, {});
    }

    const sort = (request: SortMenuCategoriesRequest) => {
        const url = new URL(`api/menucategories/sort`, import.meta.env.VITE_API_URL).toString();
        return client.put<SortMenuCategoriesResponse>(url, request, {});
    }

    const state = useMemo(() => ({
        get,
        create,
        patch,
        delete: deleteCategory,
        sort,
    }), [client]);

    return state;
}