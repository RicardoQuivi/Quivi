import { useMemo } from "react";
import { PrintConsumerBillRequest } from "./Dtos/pos/PrintConsumerBillRequest";
import { OpenCashDrawerRequest } from "./Dtos/pos/OpenCashDrawerRequest";
import { useEmployeeHttpClient } from "../../context/employee/EmployeeContextProvider";
import { EndOfDayClosingRequest } from "./Dtos/pos/EndOfDayClosingRequest";

export const usePosApi = () => {
    const httpClient = useEmployeeHttpClient();

    const openCashDrawer = async (request: OpenCashDrawerRequest) => {
        const url = new URL(`api/pos/cashDrawer`, import.meta.env.VITE_API_URL).toString();
        await httpClient.post(url, request, {});
    }

    const endOfDayClosing = async (request: EndOfDayClosingRequest) => {
        const url = new URL(`api/pos/endOfDayClosing`, import.meta.env.VITE_API_URL).toString();
        await httpClient.post(url, request, {});
    }

    const printBill = async (request: PrintConsumerBillRequest) => {
        const url = new URL(`api/pos/bill`, import.meta.env.VITE_API_URL).toString();
        await httpClient.post(url, request, {});
    }

    const state = useMemo(() => ({
        openCashDrawer,
        endOfDayClosing,
        printBill,
    }), [httpClient]);

    return state;
}