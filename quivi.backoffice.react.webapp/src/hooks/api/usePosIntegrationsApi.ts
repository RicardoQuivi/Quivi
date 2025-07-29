import { useMemo } from "react";
import { GetPosIntegrationsRequest } from "./Dtos/posIntegrations/GetPosIntegrationsRequest";
import { GetPosIntegrationsResponse } from "./Dtos/posIntegrations/GetPosIntegrationsResponse";
import { useAuthenticatedHttpClient } from "../../context/AuthContext";
import { CreatePosIntegrationResponse } from "./Dtos/posIntegrations/CreatePosIntegrationResponse";
import { CreateQuiviViaFacturalusaPosIntegrationRequest } from "./Dtos/posIntegrations/CreateQuiviViaFacturalusaPosIntegrationRequest";
import { PutQuiviViaFacturalusaPosIntegrationRequest } from "./Dtos/posIntegrations/PutQuiviViaFacturalusaPosIntegrationRequest";

export const usePosIntegrationsApi = () => {
    const client = useAuthenticatedHttpClient();

    const get = (request: GetPosIntegrationsRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }

        if(request.ids != undefined) {
            for(let i = 0; i < request.ids.length; ++i) {
                queryParams.set(`ids[${i}]`, request.ids[i].toString());
            }
        }
        
        if(request.channelId != undefined) {
            queryParams.set("channelId", request.channelId);
        }

        const url = new URL(`api/posintegrations?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return client.get<GetPosIntegrationsResponse>(url, {});
    }

    const createQuiviViaFacturaLusa = (request: CreateQuiviViaFacturalusaPosIntegrationRequest) => {
        const url = new URL(`api/posintegrations/QuiviViaFacturalusa`, import.meta.env.VITE_API_URL).toString();
        return client.post<CreatePosIntegrationResponse>(url, request, {});
    }

    const putQuiviViaFacturaLusa = (request: PutQuiviViaFacturalusaPosIntegrationRequest) => {
        const url = new URL(`api/posintegrations/${request.id}/QuiviViaFacturalusa`, import.meta.env.VITE_API_URL).toString();
        return client.put<CreatePosIntegrationResponse>(url, request, {});
    }

    const state = useMemo(() => ({
        get,
        
        createQuiviViaFacturaLusa,
        putQuiviViaFacturaLusa,
    }), [client]);

    return state;
}