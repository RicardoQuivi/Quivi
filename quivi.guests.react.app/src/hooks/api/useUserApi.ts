import { useMemo } from "react";
import { HttpHelper } from "../../helpers/httpClient";
import type { RegisterRequest } from "./Dtos/users/RegisterRequest";

    const register = async (request: RegisterRequest) => {
        const url = new URL(`api/users`, import.meta.env.VITE_API_URL).toString();
        await HttpHelper.Client.post(url, request)
    }

    const confirm = async (email: string, code: string) => {
        const url = new URL(`api/users/confirm`, import.meta.env.VITE_API_URL).toString();
        await HttpHelper.Client.post(url, {
            email: email,
            code: code,
        })
    }

    const forgotPassword = async (email: string) => {
        const url = new URL(`api/users/password/forgot`, import.meta.env.VITE_API_URL).toString();
        await HttpHelper.Client.post(url, {
            email: email,
        })
    }

    const recoverPassword = async (email: string, code: string, password: string) => {
        const url = new URL(`api/users/password/reset`, import.meta.env.VITE_API_URL).toString();
        await HttpHelper.Client.post(url, {
            email: email,
            code: code,
            password: password,
        })
    }

export const useUserApi = () => {
    const state = useMemo(() => ({
        register,
        confirm,
        forgotPassword,
        recoverPassword,
    }), []);
    return state;
}