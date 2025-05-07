import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";

export const useUserApi = () => {
    const httpClient = useHttpClient();

    const register = async (email: string, password?: string) => {
        await httpClient.httpPost(`${import.meta.env.VITE_API_URL}api/users`, {
            email: email,
            password: password,
        })
    }

    const confirm = async (email: string, code: string) => {
        await httpClient.httpPost(`${import.meta.env.VITE_API_URL}api/users/confirm`, {
            email: email,
            code: code,
        })
    }

    const forgotPassword = async (email: string) => {
        await httpClient.httpPost(`${import.meta.env.VITE_API_URL}api/users/password/forgot`, {
            email: email,
        })
    }

    const recoverPassword = async (email: string, code: string, password: string) => {
        await httpClient.httpPost(`${import.meta.env.VITE_API_URL}api/users/password/reset`, {
            email: email,
            code: code,
            password: password,
        })
    }

    const state = useMemo(() => ({
        register,
        confirm,
        forgotPassword,
        recoverPassword,
    }), [httpClient]);

    return state;
}