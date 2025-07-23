import { useMemo } from "react";
import { GetTransactionsRequest } from "./Dtos/transactions/GetTransactionsRequest";
import { GetTransactionsResponse } from "./Dtos/transactions/GetTransactionsResponse";
import { useAuthenticatedHttpClient } from "../../context/AuthContext";

export const useTransactionApi = () => {
    const client = useAuthenticatedHttpClient();

     const get = (request: GetTransactionsRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }
        request.ids?.forEach((id, index) => queryParams.set(`ids[${index}]`, id));

        if(request.fromDate != undefined) {
            queryParams.set(`fromDate`, request.fromDate.toString())
        }

        if(request.toDate != undefined) {
            queryParams.set(`toDate`, request.toDate.toString())
        }

        if(request.hasReviewComment != undefined) {
            queryParams.set(`hasReviewComment`, request.hasReviewComment.toString())
        }

        if(request.syncState != undefined) {
            queryParams.set(`syncState`, request.syncState.toString())
        }

        if(request.overPayed != undefined) {
            queryParams.set(`overPayed`, request.overPayed.toString())
        }

        if (request.hasDiscounts != undefined) {
            queryParams.set("hasDiscounts", request.hasDiscounts.toString());
        }

        if(request.search != undefined) {
            queryParams.set(`search`, request.search.toString())
        }
        
        if(request.adminView != undefined) {
            queryParams.set(`adminView`, request.adminView.toString())
        }

        if(request.customChargeMethodId != undefined) {
            queryParams.set(`customChargeMethodId`, request.customChargeMethodId.toString())
        }

        if(request.quiviPaymentsOnly != undefined) {
            queryParams.set(`quiviPaymentsOnly`, request.quiviPaymentsOnly.toString())
        }

        if (request.hasRefunds != undefined) {
            queryParams.set(`hasRefunds`, request.hasRefunds.toString());
        }

        const url = new URL(`api/transactions?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return client.get<GetTransactionsResponse>(url);
    }

    const state = useMemo(() => ({
        get,
    }), [client]);

    return state;
}