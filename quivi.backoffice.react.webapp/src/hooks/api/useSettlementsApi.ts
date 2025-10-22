import { useMemo } from "react";
import { useAuthenticatedHttpClient } from "../../context/AuthContext";
import { GetSettlementsRequest } from "./Dtos/settlements/GetSettlementsRequest";
import { GetSettlementsResponse } from "./Dtos/settlements/GetSettlementsResponse";
import { GetSettlementDetailsRequest } from "./Dtos/settlements/GetSettlementDetailsRequest";
import { GetSettlementDetailsResponse } from "./Dtos/settlements/GetSettlementDetailsResponse";
import { ChargeMethod } from "./Dtos/ChargeMethod";

export const useSettlementsApi = () => {
    const client = useAuthenticatedHttpClient();

    const get = async (request: GetSettlementsRequest): Promise<GetSettlementsResponse> => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }
        
        if(request.ids != null) {
            request.ids.forEach((id, i) => queryParams.set(`id[${i}]`, id))
        }

        if(request.chargeMethod != undefined) {
            queryParams.set("chargeMethod", ChargeMethod[request.chargeMethod]);
        }

        const url = new URL(`api/settlements?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return client.get<GetSettlementsResponse>(url, {});
    }

    const getDetails = async (request: GetSettlementDetailsRequest): Promise<GetSettlementDetailsResponse> => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }

        if(request.chargeMethod != undefined) {
            queryParams.set("chargeMethod", ChargeMethod[request.chargeMethod]);
        }
        
        const url = new URL(`api/settlements/${request.settlementId}/details?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return client.get<GetSettlementDetailsResponse>(url, {});
    }

    const state = useMemo(() => ({
        get,
        getDetails,
    }), [client]);

    return state;
}