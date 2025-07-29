import { useMemo } from "react";
import { Entity, getEntityType } from "../EntitiesName";
import { useMutator } from "./useMutator";
import { usePosIntegrationsApi } from "../api/usePosIntegrationsApi";
import { CreateQuiviViaFacturalusaPosIntegrationRequest } from "../api/Dtos/posIntegrations/CreateQuiviViaFacturalusaPosIntegrationRequest";
import { PosIntegration } from "../api/Dtos/posIntegrations/PosIntegration";

export const usePosIntegrationMutator = () => {
    const api = usePosIntegrationsApi();
    
    const createQuivi = useMutator({
        entityType: getEntityType(Entity.PosIntegrations),
        getKey: (e: PosIntegration) => e.id,
        updateCall: async (request: CreateQuiviViaFacturalusaPosIntegrationRequest) => {
            const response = await api.createQuiviViaFacturaLusa(request);
            return [response.data];
        }
    })

    const updateQuivi = useMutator({
        entityType: getEntityType(Entity.PosIntegrations),
        getKey: (e: PosIntegration) => e.id,
        updateCall: async (request: CreateQuiviViaFacturalusaPosIntegrationRequest, entities: PosIntegration[]) => {
            const entity = entities[0];
            const response = await api.putQuiviViaFacturaLusa({
                ...request,
                id: entity.id,
            });
            return [response.data];
        }
    })

    const result = useMemo(() => ({
        createQuivi: async (request: CreateQuiviViaFacturalusaPosIntegrationRequest) => {
            const result = await createQuivi.mutate([], request);
            return result.response;
        },
        updateQuivi: async (integration: PosIntegration, request: CreateQuiviViaFacturalusaPosIntegrationRequest) => {
            const result = await updateQuivi.mutate([integration], request);
            return result.response;
        },
    }), [api]);

    return result;
}