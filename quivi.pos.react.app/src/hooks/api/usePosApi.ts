import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { PrintConsumerBillRequest } from "./Dtos/pos/PrintConsumerBillRequest";
import { OpenCashDrawerRequest } from "./Dtos/pos/OpenCashDrawerRequest";

export const usePosApi = (token: string) => {
    const httpClient = useHttpClient();

    const openCashDrawer = async (request: OpenCashDrawerRequest) => {
        const url = new URL(`api/pos/cashDrawer`, import.meta.env.VITE_API_URL).toString();
        httpClient.httpPost(url, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const printBill = async (request: PrintConsumerBillRequest) => {
        const url = new URL(`api/pos/bill`, import.meta.env.VITE_API_URL).toString();
        httpClient.httpPost(url, request, {
            "Authorization": `Bearer ${token}`,
        });
    }

    const state = useMemo(() => ({
        openCashDrawer,
        printBill,
    }), [httpClient, token]);

    return state;
}