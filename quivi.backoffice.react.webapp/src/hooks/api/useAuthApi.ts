import { useMemo } from "react";
import { AuthRequest } from "./Dtos/auth/AuthRequest";
import { AuthResponse } from "./Dtos/auth/AuthResponse";
import { AuthRefreshRequest } from "./Dtos/auth/AuthRefreshRequest";
import { AuthIntrospectResponse } from "./Dtos/auth/AuthIntrospectResponse";

export class AuthenticationError extends Error {

}

export const useAuthApi = () => {
    const post = async <TResponse>(url: string, request: Record<string, string>, onError: (statusCode: number) => void) => {
        const httpResponse = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: new URLSearchParams(request),
        });
       
        if (httpResponse.status === 204) {
            return undefined as TResponse;
        }
        
        if (httpResponse.ok == false) {
            if(httpResponse.status != 200) {
                onError(httpResponse.status);
                return undefined as TResponse;
            }

            throw new Error(`HttpStatus: ${httpResponse.status} - ${httpResponse.statusText}`);
        }

        const text = await httpResponse.text();
        if (!text) {
            return undefined as TResponse;
        }

        const jsonResponse = JSON.parse(text);
        return jsonResponse as TResponse;
    }

    const getUser = async (token: string): Promise<AuthIntrospectResponse> => {
        return await post<AuthIntrospectResponse>(`${import.meta.env.VITE_AUTH_URL}connect/introspect`, {
            client_id: "backoffice",
            token: token,
        }, (s) => {
            if(s == 400) {
                throw new AuthenticationError();
            }
        });
    }

    const jwtAuth = async (request: AuthRequest): Promise<AuthResponse> => {
        const url = new URL(`connect/token`, import.meta.env.VITE_AUTH_URL).toString();
        return await post<AuthResponse>(url, {
            client_id: "backoffice",
            grant_type: "password",
            username: request.email,
            password: request.password,
        }, (s) => {
            if(s == 400) {
                throw new AuthenticationError();
            }
        });
    }

    const jwtRefresh = async (request: AuthRefreshRequest): Promise<AuthResponse> => {
        const rawRequest: Record<string, string> = {
            client_id: "backoffice",
            grant_type: "refresh_token",
            refresh_token: request.refreshToken,
        }
        if(request.merchantId != undefined) {
            rawRequest["merchant_id"] = request.merchantId;
        }
        const url = new URL(`connect/token`, import.meta.env.VITE_AUTH_URL).toString();
        return await post<AuthResponse>(url, rawRequest, (s) => {
            if(s == 400) {
                throw new AuthenticationError();
            }
        });
    }

    const state = useMemo(() => ({
        getUser,
        jwtAuth,
        jwtRefresh,
    }), []);

    return state;
}