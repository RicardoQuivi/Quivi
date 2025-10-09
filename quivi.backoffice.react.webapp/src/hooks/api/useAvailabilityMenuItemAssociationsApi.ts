import { useMemo } from "react";
import { useAuthenticatedHttpClient } from "../../context/AuthContext";
import { GetAvailabilityMenuItemAssociationsRequest } from "./Dtos/availabilityMenuItemAssociations/GetAvailabilityMenuItemAssociationsRequest";
import { GetAvailabilityMenuItemAssociationsResponse } from "./Dtos/availabilityMenuItemAssociations/GetAvailabilityMenuItemAssociationsResponse";
import { UpdateAvailabilityMenuItemAssociationsRequest } from "./Dtos/availabilityMenuItemAssociations/UpdateAvailabilityMenuItemAssociationsRequest";


export const useAvailabilityMenuItemAssociationsApi = () => {   
    const client = useAuthenticatedHttpClient();

    const get = (request: GetAvailabilityMenuItemAssociationsRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }
        
        request.availabilityIds?.forEach((id, index) => queryParams.set(`availabilityIds[${index}]`, id));
        request.menuItemIds?.forEach((id, index) => queryParams.set(`menuItemIds[${index}]`, id));

        const url = new URL(`api/AvailabilityMenuItemAssociations?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return client.get<GetAvailabilityMenuItemAssociationsResponse>(url, {});
    }

    const patch = (request: UpdateAvailabilityMenuItemAssociationsRequest) => {
        if(request.availabilityId != undefined) {
            const url = new URL(`api/Availabilities/${request.availabilityId}/MenuItemAssociations`, import.meta.env.VITE_API_URL).toString();
            return client.patch<GetAvailabilityMenuItemAssociationsResponse>(url, {
                associations: request.associations
            }, {});
        }

        if(request.menuItemId != undefined) {
            const url = new URL(`api/MenuItems/${request.menuItemId}/AvailabilityAssociations`, import.meta.env.VITE_API_URL).toString();
            return client.patch<GetAvailabilityMenuItemAssociationsResponse>(url, {
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