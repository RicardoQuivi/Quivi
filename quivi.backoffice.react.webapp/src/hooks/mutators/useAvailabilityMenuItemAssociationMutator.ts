import { useMemo } from "react";
import { Entity, getEntityType } from "../EntitiesName";
import { useMutator } from "./useMutator";
import { useAvailabilityMenuItemAssociationsApi } from "../api/useAvailabilityMenuItemAssociationsApi";
import { AvailabilityMenuItemAssociation } from "../api/Dtos/availabilityMenuItemAssociations/AvailabilityMenuItemAssociation";
import { Availability } from "../api/Dtos/availabilities/Availability";
import { UpdateAvailabilityMenuItemAssociation, UpdateAvailabilityMenuItemAssociationsRequest } from "../api/Dtos/availabilityMenuItemAssociations/UpdateAvailabilityMenuItemAssociationsRequest";
import { MenuItem } from "../api/Dtos/menuItems/MenuItem";

interface PatchMutator {
    readonly associations: UpdateAvailabilityMenuItemAssociation[];
}
export const useAvailabilityMenuItemAssociationMutator = () => {
    const api = useAvailabilityMenuItemAssociationsApi();
    
    const updateMutator = useMutator({
        entityType: getEntityType(Entity.AvailabilityMenuItemAssociations),
        getKey: (e: AvailabilityMenuItemAssociation) => `${e.availabilityId}-${e.menuItemId}`,
        updateCall: async (request: UpdateAvailabilityMenuItemAssociationsRequest) => {
            const response = await api.patch(request);
            return response.data;
        }
    })

    const result = useMemo(() => ({
        patchAvailability: async (e: Availability, associations: AvailabilityMenuItemAssociation[], mutator: PatchMutator) => {
            const result = await updateMutator.mutate(associations, {
                ...mutator,
                availabilityId: e.id,
            });
            return result.response[0];
        },
        patchMenuItem: async (e: MenuItem, associations: AvailabilityMenuItemAssociation[], mutator: PatchMutator) => {
            const result = await updateMutator.mutate(associations, {
                ...mutator,
                menuItemId: e.id,
            });
            return result.response[0];
        },
    }), [api]);

    return result;
}