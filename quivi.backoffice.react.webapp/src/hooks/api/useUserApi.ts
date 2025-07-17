import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";

export const useUserApi = () => {
    const httpClient = useHttpClient();

    const register = async (email: string, password?: string) => {
        const url = new URL(`api/users`, import.meta.env.VITE_API_URL).toString();
        await httpClient.httpPost(url, {
            email: email,
            password: password,
        })
    }

    const confirm = async (email: string, code: string) => {
        const url = new URL(`api/users/confirm`, import.meta.env.VITE_API_URL).toString();
        await httpClient.httpPost(url, {
            email: email,
            code: code,
        })
    }

    const forgotPassword = async (email: string) => {
        const url = new URL(`api/users/password/forgot`, import.meta.env.VITE_API_URL).toString();
        await httpClient.httpPost(url, {
            email: email,
        })
    }

    const recoverPassword = async (email: string, code: string, password: string) => {
        const url = new URL(`api/users/password/reset`, import.meta.env.VITE_API_URL).toString();
        await httpClient.httpPost(url, {
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