import { createContext, useContext, useState, ReactNode, useMemo, useEffect, useCallback } from 'react';
import { Employee } from '../../hooks/api/Dtos/employees/Employee';
import { useAuth } from '../AuthContextProvider';
import { AuthenticationError, useAuthApi } from '../../hooks/api/useAuthApi';
import { useEmployeesQuery } from '../../hooks/queries/implementations/useEmployeesQuery';
import { useUserInactivity } from '../../hooks/useUserInactivity';
import { EmployeeTokenData } from './EmployeeToken';
import { HttpClient, HttpHelper } from '../../helpers/httpClient';
import { UnauthorizedException } from '../../hooks/api/exceptions/UnauthorizedException';
import { AuthResponse } from '../../hooks/api/Dtos/auth/AuthResponse';

const EMPLOYEE_TOKENS = "employeeTokens";

interface Tokens {
    readonly accessToken: string | undefined;
    readonly refreshToken: string;
}

const saveTokens = (token: Tokens | undefined) => {
    if(token == undefined) {
        localStorage.removeItem(EMPLOYEE_TOKENS);
        return;
    }
    localStorage.setItem(EMPLOYEE_TOKENS, JSON.stringify(token));
}

const getTokens = (): Tokens | undefined => {
    const value = localStorage.getItem(EMPLOYEE_TOKENS);
    if(value == null) {
        return undefined;
    }
    
    const tokens = JSON.parse(value) as Tokens;
    return tokens;
}

const getState = () => {
    const tokens = getTokens();

    let data = undefined as EmployeeTokenData | undefined;
    if(tokens?.accessToken != undefined) {
        data = new EmployeeTokenData(tokens.accessToken, tokens.refreshToken);
    }
    return data;
}

const getLogoutTimeout = (employee: Employee | undefined, defaultTimeout?: number): number | undefined => {
    if(employee?.inactivityLogoutTimeout == undefined) {
        return defaultTimeout;
    }

    const aux = employee.inactivityLogoutTimeout.split(":");
    if(aux.length == 0) {
        return 0;
    }
    if(aux.length == 1) {
        return (+aux[0]) * 1000;
    }
    if(aux.length == 2) {
        return ((+aux[0])*60+(+aux[1])) * 1000;
    }
    if(aux.length == 3) {
        return ((+aux[0])*60*60+(+aux[1])*60+(+aux[2])) * 1000;
    }
    return ((+aux[0])*60*60*24+(+aux[1])*60*60+(+aux[2])*60+(+aux[3])) * 1000;
}

interface EmployeeContextType {
    readonly login: (employeeId: string, pinCode: string) => Promise<void>;
    readonly signOut: () => void;
    readonly employee: Employee | undefined;
    readonly token: string | undefined;
    readonly client: HttpClient;
}

const EmployeeContext = createContext<EmployeeContextType | undefined>(undefined);

let refreshPromise: {
    promise: Promise<AuthResponse>,
    refreshToken: string,
} | undefined = undefined;

export const EmployeeProvider = ({ children }: { children: ReactNode }) => {
    const auth = useAuth();
    const authApi = useAuthApi();

    const [state, setState] = useState(getState);

    const loggedEmployeeQuery = useEmployeesQuery(state?.employeeId == undefined ? undefined : {
        ids: [state.employeeId],
        page: 0,
        pageSize: 1,
    })

    const loggedEmployee = useMemo(() => {
        if(loggedEmployeeQuery.data.length == 0) {
            return undefined
        }
        return loggedEmployeeQuery.data[0];
    }, [loggedEmployeeQuery.data])

    const activity = useUserInactivity({ timeout: getLogoutTimeout(loggedEmployeeQuery.data.length > 0 ? loggedEmployeeQuery.data[0] : undefined)});

    const signOut = () => {
        saveTokens(undefined);
        setState(getState);
    }
    
    useEffect(() => {
        if(activity.isInactive == false) {
            return;
        }

        signOut();
    }, [activity.isInactive, loggedEmployee])

    const login = (employeeId: string, pinCode: string): Promise<void> => auth.tokenProvider(async token => {
        if(auth.principal == undefined) {
            throw new Error("Cannot login an employee because no merchant is available");
        }

        try {
            const response = await authApi.employeeLogin(token, employeeId, pinCode);

            saveTokens({
                accessToken: response.access_token,
                refreshToken: response.refresh_token,
            });
            setState(getState);
        } catch (e) {
            if(e instanceof UnauthorizedException) {
                throw e;
            }
            if(e instanceof AuthenticationError) {
                throw e;
            }
            
            signOut();
            throw e;
        }
    });
    
    const httpClient = useMemo<HttpClient>(() => {
        const refreshToken = async (refreshToken: string) => {
            const promise = await navigator.locks.request("EmployeeContextProvider.RefreshToken.Lock", () => {
                if(state == undefined) {
                    throw new AuthenticationError("No refresh token");
                }

                if (refreshPromise != undefined && refreshPromise.refreshToken == refreshToken) {
                    return refreshPromise.promise;
                }
                
                console.debug("Employee refreshing JWT for token: ", refreshToken);
                const promise = authApi.jwtRefresh({
                    refreshToken: refreshToken,
                });
                refreshPromise = {
                    promise: promise,
                    refreshToken: refreshToken,
                }
                return promise;
            });
            return await promise;
        }

        const guard = async <TResponse = void>(call: (token: string) => Promise<TResponse>): Promise<TResponse> => {
            if(state == undefined) {
                throw new UnauthorizedException(); 
            }

            try {
                return await call(state.accessToken);
            } catch (e) {
                if(e instanceof UnauthorizedException) {
                    try {
                        const refreshResponse = await refreshToken(state.refreshToken);
                        saveTokens({
                            accessToken: refreshResponse.access_token,
                            refreshToken: refreshResponse.refresh_token,
                        });
                        setState(getState);
                        const response = await call(refreshResponse.access_token);
                        return response;
                    } catch (e2) {
                        if(e2 instanceof AuthenticationError) {
                            saveTokens(undefined);
                            setState(getState);
                            throw e;
                        }
                        throw e2;
                    }
                }
                throw e;
            }
        }
        
        return {
            get: async <TResponse = void>(url: string, headers?: HeadersInit) => guard(token => HttpHelper.Client.get<TResponse>(url, {
                ...(headers ?? {}),
                "Authorization": `Bearer ${token}`,
            })),
            post: async <TResponse = void>(url: string, requestData?: any, headers?: HeadersInit) => guard(token => HttpHelper.Client.post<TResponse>(url, requestData, {
                ...(headers ?? {}),
                "Authorization": `Bearer ${token}`,
            })),
            put: async <TResponse = void>(url: string, requestData?: any, headers?: HeadersInit) => guard(token => HttpHelper.Client.put<TResponse>(url, requestData, {
                ...(headers ?? {}),
                "Authorization": `Bearer ${token}`,
            })),
            patch: async <TResponse = void>(url: string, requestData?: any, headers?: HeadersInit) => guard(token => HttpHelper.Client.patch<TResponse>(url, requestData, {
                ...(headers ?? {}),
                "Authorization": `Bearer ${token}`,
            })),
            delete: async <TResponse = void>(url: string, requestData?: any, headers?: HeadersInit) => guard(token => HttpHelper.Client.delete<TResponse>(url, requestData, {
                ...(headers ?? {}),
                "Authorization": `Bearer ${token}`,
            })),
        };
    }, [state?.accessToken, state?.refreshToken])

    const result = useMemo(() => ({
        token: state?.accessToken,
        login: login,
        signOut: signOut,
        employee: loggedEmployee,
        client: httpClient,
    }), [state, loggedEmployee, auth.principal?.token])
    
    return (
        <EmployeeContext.Provider value={result}>
            {children}
        </EmployeeContext.Provider>
    );
};

export const useEmployeeManager = (): EmployeeContextType => {
    const context = useContext(EmployeeContext);
    if (!context) {
        throw new Error('useEmployeeManager must be used within a EmployeeProvider');
    }
    return context;
}

export const useEmployeeHttpClient = (): HttpClient => {
    const context = useContext(EmployeeContext);
    if (!context) {
        throw new Error('useEmployeeHttpClient must be used within a EmployeeProvider');
    }
    return context.client;
}