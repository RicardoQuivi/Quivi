import { useMemo } from "react";
import { GetReviewsRequest } from "./Dtos/reviews/GetReviewsRequest";
import { GetReviewsResponse } from "./Dtos/reviews/GetReviewsResponse";
import { useAuthenticatedHttpClient } from "../../context/AuthContext";

export const useReviewsApi = () => {
    const client = useAuthenticatedHttpClient();

    const get = (request: GetReviewsRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }
        request.ids?.forEach((id, index) => queryParams.set(`ids[${index}]`, id));

        const url = new URL(`api/reviews?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return client.get<GetReviewsResponse>(url, {});
    }

    const state = useMemo(() => ({
        get,
    }), [client]);

    return state;
}