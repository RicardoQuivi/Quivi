import { useMemo } from "react";
import { Entity, getEntityType } from "../EntitiesName";
import { useMutator } from "./useMutator";
import type { Transaction } from "../api/Dtos/transactions/Transaction";
import { useTransactionsApi } from "../api/useTransactionsApi";
import type { CreateTransactionRequest } from "../api/Dtos/transactions/CreateTransactionRequest";
import { ChargeMethod } from "../api/Dtos/ChargeMethod";

interface PaybyrdProcessMutator {
    readonly tokenId: string;
    readonly redirectUrl?: string;
}

interface ExtendedTransaction extends Transaction {
    readonly threeDsUrl?: string | undefined;
}

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

    const processPaybyrdMutator = useMutator({
        entityType: getEntityType(Entity.Transactions),
        getKey: (e: ExtendedTransaction) => e.id,
        updateCall: async (mutator: PaybyrdProcessMutator, entities: ExtendedTransaction[]) => {
            const result = [] as ExtendedTransaction[];
            for(const entity of entities) {
                if(entity.method != ChargeMethod.CreditCard) {
                    throw new Error();
                }

                const response = await api.processPaybyrd(entity.id, {
                    ...mutator,
                    method: entity.method,
                });
                result.push({
                    ...response.data,
                    threeDsUrl: response.threeDsUrl as string | undefined,
                });
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
        },
        processPaybyrd: async (entity: Transaction, data: PaybyrdProcessMutator) => {
            const result = await processPaybyrdMutator.mutate([entity], data);
            const rEntity = result.response[0];
            return {
                entity: rEntity as Transaction,
                threeDsUrl: rEntity.threeDsUrl,
            };
        }
    }), [api]);
    
    return result;
}