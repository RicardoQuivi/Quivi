import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import type { QueryResult } from "../QueryResult";
import { useTransactionsApi } from "../../api/useTransactionsApi";
import type { TransactionInvoice } from "../../api/Dtos/transactions/TransactionInvoice";
import { useEffect } from "react";
import { useInvalidator } from "../../../context/QueryContextProvider";
import { useWebEvents } from "../../signalR/useWebEvents";
import type { TransactionListener } from "../../signalR/TransactionListener";

export const useTransactionInvoicesQuery = (id: string | undefined): QueryResult<TransactionInvoice[]> => {
    const api = useTransactionsApi();
    const invalidator = useInvalidator();
    const webEvents = useWebEvents();

    const query = useQueryable({
        queryName: "useTransactionInvoicesQuery",
        entityType: getEntityType(Entity.TransactionInvoices),
        getId: (item: TransactionInvoice) => item.id,
        request: id == undefined ? undefined : {
            transactionId: id,
        },
        query: r => api.getInvoice(r.transactionId),

        refreshOnAnyUpdate: true,
        canUseOptimizedResponse: _ => false,
        getResponseFromEntities: (e) => ({
            data: e,
            page: 0,
            totalPages: 1,
            totalItems: 1,
        }),   
    })

    useEffect(() => {
        if(id == undefined) {
            return;
        }

        const listener: TransactionListener = {
            transactionId: id,
            onTransactionInvoiceOperation: evt => invalidator.invalidate(Entity.TransactionInvoices, evt.id),
        }
        webEvents.client.addTransactionListener(listener);
        return () => webEvents.client.removeTransactionListener(listener);
    }, [id])
    
    return query;
}