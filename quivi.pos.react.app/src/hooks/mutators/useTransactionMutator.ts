import { useMemo } from "react";
import { Entity, getEntityType } from "../EntitiesName";
import { useMutator } from "./useMutator";
import { useTransactionsApi } from "../api/useTransactionsApi";
import { Transaction } from "../api/Dtos/transactions/Transaction";
import { CreateTransactionRequest } from "../api/Dtos/transactions/CreateTransactionRequest";

interface RefundRequest {
    readonly amount: number;
    readonly ignoreAcquireRefundErrors: boolean;
    readonly isCancellation?: boolean;
    readonly refundReason?: string;
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

    const refundMutator = useMutator({
        entityType: getEntityType(Entity.Transactions),
        getKey: (e: Transaction) => e.id,
        updateCall: async (request: RefundRequest, entities: Transaction[]) => {
            const result = [] as Transaction[];
            for(const e of entities) {
                const response = await api.refund({
                    ...request,
                    id: e.id,
                });
                result.push(response.data)
            }
            return result;
        }
    })

    const result = useMemo(() => ({
        create: async (request: CreateTransactionRequest) => {
            const result = await createMutator.mutate([], request);
            return result.response[0];
        },
        refund: async (transaction: Transaction, request: RefundRequest) => {
            const result = await refundMutator.mutate([transaction], request);
            return result.response[0];
        },
    }), [api]);

    return result;
}