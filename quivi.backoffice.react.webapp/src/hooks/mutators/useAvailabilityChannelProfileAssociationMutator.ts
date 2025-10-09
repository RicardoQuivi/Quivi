import { useMemo } from "react";
import { Entity, getEntityType } from "../EntitiesName";
import { useMutator } from "./useMutator";
import { Availability } from "../api/Dtos/availabilities/Availability";
import { useAvailabilityChannelProfileAssociationsApi } from "../api/useAvailabilityChannelProfileAssociationsApi";
import { AvailabilityChannelProfileAssociation } from "../api/Dtos/availabilityChannelProfileAssociations/AvailabilityChannelProfileAssociation";
import { UpdateAvailabilityChannelProfileAssociation, UpdateAvailabilityChannelProfileAssociationsRequest } from "../api/Dtos/availabilityChannelProfileAssociations/UpdateAvailabilityChannelProfileAssociationsRequest";
import { ChannelProfile } from "../api/Dtos/channelProfiles/ChannelProfile";

interface PatchMutator {
    readonly associations: UpdateAvailabilityChannelProfileAssociation[];
}
export const useAvailabilityChannelProfileAssociationMutator = () => {
    const api = useAvailabilityChannelProfileAssociationsApi();
    
    const updateMutator = useMutator({
        entityType: getEntityType(Entity.AvailabilityChannelProfileAssociations),
        getKey: (e: AvailabilityChannelProfileAssociation) => `${e.availabilityId}-${e.channelProfileId}`,
        updateCall: async (request: UpdateAvailabilityChannelProfileAssociationsRequest) => {
            const response = await api.patch(request);
            return response.data;
        }
    })

    const result = useMemo(() => ({
        patchAvailability: async (e: Availability, associations: AvailabilityChannelProfileAssociation[], mutator: PatchMutator) => {
            const result = await updateMutator.mutate(associations, {
                ...mutator,
                availabilityId: e.id,
            });
            return result.response[0];
        },
        patchMenuItem: async (e: ChannelProfile, associations: AvailabilityChannelProfileAssociation[], mutator: PatchMutator) => {
            const result = await updateMutator.mutate(associations, {
                ...mutator,
                channelProfileId: e.id,
            });
            return result.response[0];
        },
    }), [api]);

    return result;
}