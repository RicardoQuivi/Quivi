import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { useTranslation } from "react-i18next";
import type { GetReviewRequest } from "./Dtos/reviews/GetReviewRequest";
import type { GetReviewResponse } from "./Dtos/reviews/GetReviewResponse";
import type { PatchReviewRequest } from "./Dtos/reviews/PatchReviewRequest";
import type { PatchReviewResponse } from "./Dtos/reviews/PatchReviewResponse";

export const useReviewsApi = () => {
    const httpClient = useHttpClient();
    const { i18n } = useTranslation();

    const get = (request: GetReviewRequest) => {
        const url = new URL(`api/reviews/${request.transactionId}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.get<GetReviewResponse>(url, {
            'Accept-Language': i18n.language,
        });
    }

    const patch = (request: PatchReviewRequest) => {
        const url = new URL(`api/reviews/${request.transactionId}`, import.meta.env.VITE_API_URL).toString();
        return httpClient.patch<PatchReviewResponse>(url, request, {
            'Accept-Language': i18n.language,
        });
    }

    const state = useMemo(() => ({
        get,
        patch,
    }), [httpClient, i18n, i18n.language]);

    return state;
}