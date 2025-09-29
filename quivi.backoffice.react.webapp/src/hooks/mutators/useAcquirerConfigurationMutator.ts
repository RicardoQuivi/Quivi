import { useMemo } from "react";
import { Entity, getEntityType } from "../EntitiesName";
import { useMutator } from "./useMutator";
import { useAcquirerConfigurationsApi } from "../api/useAcquirerConfigurationsApi";
import { UpsertCashAcquirerConfigurationRequest } from "../api/Dtos/acquirerconfigurations/UpsertCashAcquirerConfigurationRequest";
import { AcquirerConfiguration } from "../api/Dtos/acquirerconfigurations/AcquirerConfiguration";
import { UpsertPaybyrdAcquirerConfigurationRequest } from "../api/Dtos/acquirerconfigurations/UpsertPaybyrdAcquirerConfigurationRequest";
import { UpsertPaybyrdTerminalAcquirerConfigurationRequest } from "../api/Dtos/acquirerconfigurations/UpsertPaybyrdTerminalAcquirerConfigurationRequest";

export const useAcquirerConfigurationMutator = () => {
    const api = useAcquirerConfigurationsApi();
    
    const upsertCash = useMutator({
        entityType: getEntityType(Entity.AcquirerConfigurations),
        getKey: (e: AcquirerConfiguration) => e.id,
        updateCall: async (request: UpsertCashAcquirerConfigurationRequest) => {
            const response = await api.upsertCash(request);
            return [response.data];
        }
    })

    const upsertPaybyrd = useMutator({
        entityType: getEntityType(Entity.AcquirerConfigurations),
        getKey: (e: AcquirerConfiguration) => e.id,
        updateCall: async (request: UpsertPaybyrdAcquirerConfigurationRequest) => {
            const response = await api.upsertPaybyrd(request);
            return [response.data];
        }
    })

    const upsertPaybyrdTerminal = useMutator({
        entityType: getEntityType(Entity.AcquirerConfigurations),
        getKey: (e: AcquirerConfiguration) => e.id,
        updateCall: async (request: UpsertPaybyrdTerminalAcquirerConfigurationRequest) => {
            const response = await api.upsertPaybyrdTerminal(request);
            return [response.data];
        }
    })

    const result = useMemo(() => ({
        upsertCash: async (request: UpsertCashAcquirerConfigurationRequest) => {
            const result = await upsertCash.mutate([], request);
            return result.response;
        },
        upsertPaybyrd: async (request: UpsertPaybyrdAcquirerConfigurationRequest) => {
            const result = await upsertPaybyrd.mutate([], request);
            return result.response;
        },
        upsertPaybyrdTerminal: async (request: UpsertPaybyrdTerminalAcquirerConfigurationRequest) => {
            const result = await upsertPaybyrdTerminal.mutate([], request);
            return result.response;
        },
    }), [api]);

    return result;
}