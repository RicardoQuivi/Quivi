import { useMemo } from "react";
import { Channel } from "../api/Dtos/channels/Channel";
import { CreateChannelsRequest } from "../api/Dtos/channels/CreateChannelRequest";
import { useChannelsApi } from "../api/useChannelsApi";
import { Entity, getEntityType } from "../EntitiesName";
import { useMutator } from "./useMutator";

interface PatchRequest {
    readonly channelProfileId?: string;
    readonly name?: string;
}
export const useChannelMutator = () => {
    const api = useChannelsApi();

    const createMutator = useMutator({
        entityType: getEntityType(Entity.Channels),
        getKey: (e: Channel) => e.id,
        updateCall: async (request: CreateChannelsRequest) => {
            const response = await api.create(request);
            return response.data;
        }
    })

    const editMutator = useMutator({
        entityType: getEntityType(Entity.Channels),
        getKey: (e: Channel) => e.id,
        updateCall: async (request: PatchRequest, entities: Channel[]) => {
            const response = await api.patch({
                ids: entities.map(e => e.id),
                ...request,
            });
            return response.data;
        }
    })

    const deleteMutator = useMutator({
        entityType: getEntityType(Entity.Channels),
        getKey: (e: Channel) => e.id,
        updateCall: async (_: {}, entities: Channel[]) => {
            const result = [] as Channel[];
            await api.delete({
                ids: entities.map(e => e.id),
            });
            return result;
        }
    })

    const result = useMemo(() => ({
        create: async (request: CreateChannelsRequest) => {
            const result = await createMutator.mutate([], request);
            return result.response;
        },
        patch: (e: Channel[], patch: PatchRequest) => editMutator.mutate(e, patch),
        delete: (e: Channel[]) => deleteMutator.mutate(e, {}),
    }), [api]);

    return result;
}