import { useMemo } from "react";
import { AuthResponse } from "./Dtos/auth/AuthResponse";
import { AuthRefreshRequest } from "./Dtos/auth/AuthRefreshRequest";
import { AuthIntrospectResponse } from "./Dtos/auth/AuthIntrospectResponse";
import { AuthTokenExchangeRequest } from "./Dtos/auth/AuthTokenExchangeRequest";

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
            client_id: "pos",
            token: token,
        }, (s) => {
            if(s == 400) {
                throw new AuthenticationError();
            }
        });
    }

    const jwtRefresh = async (request: AuthRefreshRequest): Promise<AuthResponse> => {
        const rawRequest: Record<string, string> = {
            client_id: "pos",
            grant_type: "refresh_token",
            refresh_token: request.refreshToken,
        }
        
        return await post<AuthResponse>(`${import.meta.env.VITE_AUTH_URL}connect/token`, rawRequest, (s) => {
            if(s == 400) {
                throw new AuthenticationError();
            }
        });
    }

    const tokenExchange = async (request: AuthTokenExchangeRequest): Promise<AuthResponse> => {
        const rawRequest: Record<string, string> = {
            client_id: "pos",
            grant_type: "urn:ietf:params:oauth:grant-type:token-exchange",
            subject_token: request.subjectToken,
            subject_token_type: "urn:ietf:params:oauth:token-type:access_token",
        }

        return await post<AuthResponse>(`${import.meta.env.VITE_AUTH_URL}connect/token`, rawRequest, (s) => {
            if(s == 400) {
                throw new AuthenticationError();
            }
        });
    }

    const employeeLogin  = async (token: string, id: string, pincode: string): Promise<AuthResponse> => {
        const rawRequest: Record<string, string> = {
            client_id: "pos",
            grant_type: "employee",
            subject_token: token,
            username: id,
            password: pincode,
        }

        return await post<AuthResponse>(`${import.meta.env.VITE_AUTH_URL}connect/token`, rawRequest, (s) => {
            if(s == 400) {
                throw new AuthenticationError();
            }
        });
    }

    const state = useMemo(() => ({
        getUser,
        tokenExchange,
        jwtRefresh,
        employeeLogin,
    }), []);

    return state;
}