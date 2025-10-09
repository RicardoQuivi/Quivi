import { useMemo } from "react";
import { useAuthenticatedHttpClient } from "../../context/AuthContext";
import { GetConfigurableFieldAssociationsRequest } from "./Dtos/configurableFieldAssociations/GetConfigurableFieldAssociationsRequest";
import { GetConfigurableFieldAssociationsResponse } from "./Dtos/configurableFieldAssociations/GetConfigurableFieldAssociationsResponse";
import { UpdateConfigurableFieldAssociationsRequest } from "./Dtos/configurableFieldAssociations/UpdateConfigurableFieldAssociationsRequest";

export const useConfigurableFieldAssociationsApi = () => {   
    const client = useAuthenticatedHttpClient();

    const get = (request: GetConfigurableFieldAssociationsRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }
        
        request.channelProfileIds?.forEach((id, index) => queryParams.set(`channelProfileIds[${index}]`, id));
        request.configurableFieldIds?.forEach((id, index) => queryParams.set(`configurableFieldIds[${index}]`, id));

        const url = new URL(`api/ConfigurableFieldAssociations?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return client.get<GetConfigurableFieldAssociationsResponse>(url, {});
    }

    const patch = (request: UpdateConfigurableFieldAssociationsRequest) => {
        if(request.channelProfileId != undefined) {
            const url = new URL(`api/ChannelProfiles/${request.channelProfileId}/associations`, import.meta.env.VITE_API_URL).toString();
            return client.patch<GetConfigurableFieldAssociationsResponse>(url, {
                associations: request.associations
            }, {});
        }

        if(request.configurableFieldId != undefined) {
            const url = new URL(`api/ConfigurableFields/${request.configurableFieldId}/associations`, import.meta.env.VITE_API_URL).toString();
            return client.patch<GetConfigurableFieldAssociationsResponse>(url, {
                associations: request.associations
            }, {});
        }

        throw new Error();
    }

    const state = useMemo(() => ({
        get,
        patch,
    }), [client]);

    return state;
}