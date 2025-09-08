import { useMemo } from "react";
import { useWebEvents } from "./signalR/useWebEvents"
import { BackgroundJobPromise } from "./signalR/promises/BackgroundJobPromise";
import { useBackgroundJobsApi } from "./api/useBackgroundJobsApi";
import { useTransactionsApi } from "./api/useTransactionsApi";
import { TransactionSyncedPromise } from "./signalR/promises/TransactionSyncedPromise";

export const useActionAwaiter = () => {
    const webEvents = useWebEvents();
    const jobsApi = useBackgroundJobsApi();
    const transactionApi = useTransactionsApi();

    const result = useMemo(() => ({
        job: (jobId: string) => {
            return new BackgroundJobPromise(jobId, webEvents.client, async (jobId) => {
                const response = await jobsApi.get({
                    ids: [jobId],
                });
                return response.data[0].state;
            })
        },
        syncedTransaction: (transactionId: string) => {
            return new TransactionSyncedPromise(transactionId, webEvents.client, async () => {
                const result = await transactionApi.get({
                    ids: [transactionId],
                    page: 0,
                    pageSize: 1,
                });

                if(result.data.length == 0) {
                    return undefined;
                }
                return result.data[0];
            });
        }
    }), [webEvents, jobsApi])

    return result;
}

