import { useMemo } from "react";
import { Entity, getEntityType } from "../EntitiesName";
import { useMutator } from "./useMutator";
import { ConfigurableField } from "../api/Dtos/configurableFields/ConfigurableField";
import { useConfigurableFieldAssociationsApi } from "../api/useConfigurableFieldAssociationsApi";
import { ConfigurableFieldAssociation } from "../api/Dtos/configurableFieldAssociations/ConfigurableFieldAssociation";
import { UpdateConfigurableFieldAssociation, UpdateConfigurableFieldAssociationsRequest } from "../api/Dtos/configurableFieldAssociations/UpdateConfigurableFieldAssociationsRequest";
import { ChannelProfile } from "../api/Dtos/channelProfiles/ChannelProfile";

interface PatchMutator {
    readonly associations: UpdateConfigurableFieldAssociation[];
}
export const useConfigurableFieldAssociationMutator = () => {
    const api = useConfigurableFieldAssociationsApi();
    
    const updateMutator = useMutator({
        entityType: getEntityType(Entity.ConfigurableFieldAssociations),
        getKey: (e: ConfigurableFieldAssociation) => `${e.channelProfileId}-${e.configurableFieldId}`,
        updateCall: async (request: UpdateConfigurableFieldAssociationsRequest) => {
            const response = await api.patch(request);
            return response.data;
        }
    })

    const result = useMemo(() => ({
        patchConfigurableField: async (e: ConfigurableField, associations: ConfigurableFieldAssociation[], mutator: PatchMutator) => {
            const result = await updateMutator.mutate(associations, {
                ...mutator,
                configurableFieldId: e.id,
            });
            return result.response[0];
        },
        patchChannelProfile: async (e: ChannelProfile, associations: ConfigurableFieldAssociation[], mutator: PatchMutator) => {
            const result = await updateMutator.mutate(associations, {
                ...mutator,
                channelProfileId: e.id,
            });
            return result.response[0];
        },
    }), [api]);

    return result;
}