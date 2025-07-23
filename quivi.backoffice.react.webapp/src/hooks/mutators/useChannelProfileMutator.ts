import { useMemo } from "react";
import { Entity, getEntityType } from "../EntitiesName";
import { useMutator } from "./useMutator";
import { useChannelProfilesApi } from "../api/useChannelProfilesApi";
import { ChannelFeatures, ChannelProfile } from "../api/Dtos/channelProfiles/ChannelProfile";
import { CreateChannelProfileRequest } from "../api/Dtos/channelProfiles/CreateChannelProfileRequest";

interface PatchMutator {
    readonly name?: string;
    readonly minimumPrePaidOrderAmount?: number;
    readonly features?: ChannelFeatures;
    readonly sendToPreparationTimer?: string | undefined | null;
    readonly posIntegrationId?: string | undefined;
}
export const useChannelProfileMutator = () => {
    const api = useChannelProfilesApi();
    
    const createMutator = useMutator({
        entityType: getEntityType(Entity.ChannelProfiles),
        getKey: (e: ChannelProfile) => e.id,
        updateCall: async (request: CreateChannelProfileRequest) => {
            const result = [] as ChannelProfile[];
            const response = await api.create(request);
            result.push(response.data);
            return result;
        }
    })

    const updateMutator = useMutator({
        entityType: getEntityType(Entity.ChannelProfiles),
        getKey: (e: ChannelProfile) => e.id,
        updateCall: async (request: PatchMutator, entities: ChannelProfile[]) => {
            const result = [] as ChannelProfile[];
            
            //NOTE: Current implementation limits entities to an array of a single entry.
            // Nevertheless, the above implementation works even if we would allow several.
            for(const entity of entities) {
                const response = await api.patch({
                    ...request,
                    id: entity.id,
                });
                
                if(response.data != undefined) {
                    result.push(response.data);
                }
            }
            return result;
        }
    })

    const deleteMutator = useMutator({
        entityType: getEntityType(Entity.ChannelProfiles),
        getKey: (e: ChannelProfile) => e.id,
        updateCall: async (_: {}, entities: ChannelProfile[]) => {
            const result = [] as ChannelProfile[];
            
            //NOTE: Current implementation limits entities to an array of a single entry.
            // Nevertheless, the above implementation works even if we would allow several.
            for(const entity of entities) {
                await api.delete({
                    id: entity.id,
                });
            }
            return result;
        }
    })

    const result = useMemo(() => ({
        create: async (request: CreateChannelProfileRequest) => {
            const result = await createMutator.mutate([], request);
            return result.response[0];
        },
        patch: async (e: ChannelProfile, mutator: PatchMutator) => {
            const result = await updateMutator.mutate([e], mutator);
            return result.response[0];
        },
        delete:  (e: ChannelProfile) => deleteMutator.mutate([e], {}),
    }), [api]);

    return result;
}