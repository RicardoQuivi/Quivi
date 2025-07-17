import { useMemo } from "react";
import { ApiException } from "./exceptions/ApiException";
import type { InvalidModelResponse } from "./exceptions/InvalidModelResponse";

export const useHttpClient = () => {

    const parseResponse = async <TResponse = void>(httpResponse: Response): Promise<TResponse> => {
        if (httpResponse.status === 204) {
            // 204 No Content
            return undefined as TResponse;
        }

        const text = await httpResponse.text();

        if (!text) {
            // Empty body
            return undefined as TResponse;
        }

        const jsonResponse = JSON.parse(text);
        if (httpResponse.ok)
            return jsonResponse as TResponse;
        
        const errors = jsonResponse as InvalidModelResponse[];
        if (!!errors && httpResponse.status === 400)
            throw new ApiException(errors);
        else
            throw new Error(`HttpStatus: ${httpResponse.status} - ${httpResponse.statusText}`);
    }

    const httpGet = async <TResponse = void,>(url: string, headers?: HeadersInit): Promise<TResponse> => {
        const httpResponse = await fetch(url, {
            method: "GET",
            headers: {
                'Content-Type': 'application/json',
                ...headers,
            }
        })

        return await parseResponse<TResponse>(httpResponse);
    }

    const httpPost = async <TResponse = void,>(url: string, requestData?: any, headers?: HeadersInit): Promise<TResponse> => {
        const httpRequest: RequestInit = {
            method: "POST",
            headers: {
                'Content-Type': 'application/json',
                ...headers,
            },
        };
        
        if (requestData)
            httpRequest.body = JSON.stringify(requestData);

        const httpResponse = await fetch(url, httpRequest);
        return await parseResponse<TResponse>(httpResponse);
    }

    const httpPut = async <TResponse = void,>(url: string, requestData: any, headers?: HeadersInit): Promise<TResponse> => {
        const httpResponse = await fetch(url, {
            method: "PUT",
            headers: {
                'Content-Type': 'application/json',
                ...headers,
            },
            body: JSON.stringify(requestData),
        });
        return await parseResponse<TResponse>(httpResponse);
    }

    const httpPatch = async <TResponse = void,>(url: string, requestData: any, headers?: HeadersInit): Promise<TResponse> => {
        const httpResponse = await fetch(url, {
            method: "PATCH",
            headers: {
                'Content-Type': 'application/json',
                ...headers,
            },
            body: JSON.stringify(requestData),
        });
        return await parseResponse<TResponse>(httpResponse);
    }

    const httpDelete = async <TResponse = void,>(url: string, requestData?: any, headers?: HeadersInit): Promise<TResponse> => {
        const httpRequest: RequestInit = {
            method: "DELETE",
            headers: {
                'Content-Type': 'application/json',
                ...headers,
            },
        };
        
        if (requestData)
            httpRequest.body = JSON.stringify(requestData);

        const httpResponse = await fetch(url, httpRequest);
        return await parseResponse<TResponse>(httpResponse);
    }

    const state = useMemo(() => ({
        httpGet,
        httpPost,
        httpPut,
        httpPatch,
        httpDelete,
    }), []);
    
    return state;
};