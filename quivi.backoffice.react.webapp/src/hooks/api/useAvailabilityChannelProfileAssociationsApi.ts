import { useMemo } from "react";
import { useAuthenticatedHttpClient } from "../../context/AuthContext";
import { GetAvailabilityChannelProfileAssociationsRequest } from "./Dtos/availabilityChannelProfileAssociations/GetAvailabilityChannelProfileAssociationsRequest";
import { GetAvailabilityChannelProfileAssociationsResponse } from "./Dtos/availabilityChannelProfileAssociations/GetAvailabilityChannelProfileAssociationsResponse";
import { UpdateAvailabilityChannelProfileAssociationsRequest } from "./Dtos/availabilityChannelProfileAssociations/UpdateAvailabilityChannelProfileAssociationsRequest";

export const useAvailabilityChannelProfileAssociationsApi = () => {   
    const client = useAuthenticatedHttpClient();

    const get = (request: GetAvailabilityChannelProfileAssociationsRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }
        
        request.availabilityIds?.forEach((id, index) => queryParams.set(`availabilityIds[${index}]`, id));
        request.channelProfileIds?.forEach((id, index) => queryParams.set(`channelProfileIds[${index}]`, id));

        const url = new URL(`api/AvailabilityChannelProfileAssociations?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return client.get<GetAvailabilityChannelProfileAssociationsResponse>(url, {});
    }

    const patch = (request: UpdateAvailabilityChannelProfileAssociationsRequest) => {
        if(request.availabilityId != undefined) {
            const url = new URL(`api/Availabilities/${request.availabilityId}/ChannelProfileAssociations`, import.meta.env.VITE_API_URL).toString();
            return client.patch<GetAvailabilityChannelProfileAssociationsResponse>(url, {
                associations: request.associations
            }, {});
        }

        if(request.channelProfileId != undefined) {
            const url = new URL(`api/ChannelProfiles/${request.channelProfileId}/AvailabilityAssociations`, import.meta.env.VITE_API_URL).toString();
            return client.patch<GetAvailabilityChannelProfileAssociationsResponse>(url, {
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