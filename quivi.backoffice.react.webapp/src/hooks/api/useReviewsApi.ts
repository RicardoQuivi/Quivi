import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { GetReviewsRequest } from "./Dtos/reviews/GetReviewsRequest";
import { GetReviewsResponse } from "./Dtos/reviews/GetReviewsResponse";

export const useReviewsApi = (token?: string) => {
    const httpClient = useHttpClient();
 
    const get = (request: GetReviewsRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }
        request.ids?.forEach((id, index) => queryParams.set(`ids[${index}]`, id));

        const url = new URL(`api/reviews?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.httpGet<GetReviewsResponse>(url, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const state = useMemo(() => ({
        get,
    }), [httpClient, token]);

    return state;
}