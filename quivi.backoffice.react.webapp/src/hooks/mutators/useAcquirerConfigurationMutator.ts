import { useMemo } from "react";
import { useAuth } from "../../context/AuthContext";
import { Entity, getEntityType } from "../EntitiesName";
import { useMutator } from "./useMutator";
import { useAcquirerConfigurationsApi } from "../api/useAcquirerConfigurationsApi";
import { UpsertCashAcquirerConfigurationRequest } from "../api/Dtos/acquirerconfigurations/UpsertCashAcquirerConfigurationRequest";
import { AcquirerConfiguration } from "../api/Dtos/acquirerconfigurations/AcquirerConfiguration";

export const useAcquirerConfigurationMutator = () => {
    const auth = useAuth();
    const api = useAcquirerConfigurationsApi(auth.token);
    
    const upsertCash = useMutator({
        entityType: getEntityType(Entity.Locals),
        getKey: (e: AcquirerConfiguration) => e.id,
        updateCall: async (request: UpsertCashAcquirerConfigurationRequest) => {
            const response = await api.upsertCash(request);
            return [response.data];
        }
    })

    const result = useMemo(() => ({
        upsertCash: async (request: UpsertCashAcquirerConfigurationRequest) => {
            const result = await upsertCash.mutate([], request);
            return result.response;
        },
    }), [auth, api]);

    return result;
}