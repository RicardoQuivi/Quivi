import { ApiException } from "../hooks/api/exceptions/ApiException";
import { InvalidModelResponse } from "../hooks/api/exceptions/InvalidModelResponse";
import { UnauthorizedException } from "../hooks/api/exceptions/UnauthorizedException";

export interface HttpClient {
    readonly get: <TResponse = void>(url: string, headers?: HeadersInit) => Promise<TResponse>;
    readonly post: <TResponse = void>(url: string, requestData?: any, headers?: HeadersInit) => Promise<TResponse>;
    readonly put: <TResponse = void>(url: string, requestData: any, headers?: HeadersInit) => Promise<TResponse>;
    readonly patch: <TResponse = void>(url: string, requestData: any, headers?: HeadersInit) => Promise<TResponse>;
    readonly delete: <TResponse = void>(url: string, requestData?: any, headers?: HeadersInit) => Promise<TResponse>;
}

const parseResponse = async <TResponse = void>(httpResponse: Response): Promise<TResponse> => {
    if (httpResponse.status === 204) {
        // 204 No Content
        return undefined as TResponse;
    }

    if(httpResponse.status == 401) {
        throw new UnauthorizedException();
    }

    if([200, 400].includes(httpResponse.status)) {
        const text = await httpResponse.text();
        const jsonResponse = JSON.parse(text);

        if (httpResponse.ok) {
            if (!text) {
                return undefined as TResponse; // Empty body
            }

            return jsonResponse as TResponse;
        }
        
        const errors = jsonResponse as InvalidModelResponse[];
        if (!!errors && httpResponse.status === 400) {
            throw new ApiException(errors);
        }
    }
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

const client: HttpClient = {
    get: httpGet,
    post: httpPost,
    put: httpPut,
    patch: httpPatch,
    delete: httpDelete,
};

export class HttpHelper {
    static Client = client;
}