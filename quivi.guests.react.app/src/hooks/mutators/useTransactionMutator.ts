import { useMemo } from "react";
import { Entity, getEntityType } from "../EntitiesName";
import { useMutator } from "./useMutator";
import type { Transaction } from "../api/Dtos/transactions/Transaction";
import { useTransactionsApi } from "../api/useTransactionsApi";
import type { CreateTransactionRequest } from "../api/Dtos/transactions/CreateTransactionRequest";
import { ChargeMethod } from "../api/Dtos/ChargeMethod";

export const useTransactionMutator = () => {
    const api = useTransactionsApi();

    const createMutator = useMutator({
        entityType: getEntityType(Entity.Transactions),
        getKey: (e: Transaction) => e.id,
        updateCall: async (request: CreateTransactionRequest) => {
            const response = await api.create(request);
            return [response.data];
        }
    })

    const processCashMutator = useMutator({
        entityType: getEntityType(Entity.Transactions),
        getKey: (e: Transaction) => e.id,
        updateCall: async (_: {}, entities: Transaction[]) => {
            const result = [];
            for(const entity of entities) {
                if(entity.method != ChargeMethod.Cash) {
                    throw new Error();
                }

                const response = await api.processCash(entity.id);
                result.push(response.data);
            }
            return result;
        }
    })

    const result = useMemo(() => ({
        create: async (request: CreateTransactionRequest) => {
            const result = await createMutator.mutate([], request);
            return result.response[0];
        },
        processCash: async (entity: Transaction) => {
            const result = await processCashMutator.mutate([entity], {});
            return result.response[0];
        }
    }), [api]);
    
    return result;
}