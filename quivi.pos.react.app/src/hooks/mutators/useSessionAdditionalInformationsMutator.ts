import { useMemo } from "react";
import { Entity, getEntityType } from "../EntitiesName";
import { useMutator } from "./useMutator";
import { useSessionAdditionalInformationsApi } from "../api/useSessionAdditionalInformationsApi";
import { SessionAdditionalInformation } from "../api/Dtos/sessionAdditionalInformations/SessionAdditionalInformation";
import { UpsertSessionAdditionalInformationsRequest } from "../api/Dtos/sessionAdditionalInformations/UpsertSessionAdditionalInformationsRequest";

export const useSessionAdditionalInformationsMutator = () => {
    const api = useSessionAdditionalInformationsApi();

    const upsertMutator = useMutator({
        entityType: getEntityType(Entity.SessionAdditionalInformations),
        getKey: (e: SessionAdditionalInformation) => `${e.orderId}/${e.id}`,
        updateCall: async (request: UpsertSessionAdditionalInformationsRequest) => {
            const response = await api.post(request);
            return response.data;
        }
    })

    const result = useMemo(() => ({
        upsert: async (sessionId: string, fields: Record<string, string | number | boolean>) => {
            const entities = [] as SessionAdditionalInformation[];
            for(const id in fields) {
                entities.push({
                    id: id,
                    orderId: "",
                    value: fields[id].toString(),
                })
            }
            const parsedFields = {} as Record<string, string>;
            for(const key of Object.keys(fields)) {
                parsedFields[key] = fields[key].toString();
            }
            return upsertMutator.mutate(entities, {
                sessionId: sessionId,
                fields: parsedFields,
            });
        },
    }), [api]);

    return result;
}