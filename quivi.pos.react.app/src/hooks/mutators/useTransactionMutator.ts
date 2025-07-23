import { useMemo } from "react";
import { Entity, getEntityType } from "../EntitiesName";
import { useMutator } from "./useMutator";
import { useTransactionsApi } from "../api/useTransactionsApi";
import { Transaction } from "../api/Dtos/transactions/Transaction";
import { CreateTransactionRequest } from "../api/Dtos/transactions/CreateTransactionRequest";

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

    const result = useMemo(() => ({
        create: async (request: CreateTransactionRequest) => {
            const result = await createMutator.mutate([], request);
            return result.response[0];
        },
    }), [api]);

    return result;
}