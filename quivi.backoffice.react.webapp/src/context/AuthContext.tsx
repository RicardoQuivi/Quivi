import { createContext, useContext, useState, ReactNode, useMemo } from 'react';
import { jwtDecode } from 'jwt-decode';
import { AuthenticationError, useAuthApi } from '../hooks/api/useAuthApi';
import { useTranslation } from 'react-i18next';
import { useToast } from '../layout/ToastProvider';
import { HttpClient, HttpHelper } from '../utilities/httpClient';
import { UnauthorizedException } from '../hooks/api/exceptions/UnauthorizedException';
import { AuthResponse } from '../hooks/api/Dtos/auth/AuthResponse';

const TOKENS = "tokens";
interface Tokens {
    readonly accessToken: string | undefined;
    readonly refreshToken: string;
}

const saveTokens = (token: Tokens | undefined) => {
    if(token == undefined) {
        localStorage.removeItem(TOKENS);
        return;
    }
    localStorage.setItem(TOKENS, JSON.stringify(token));
}

const getTokens = (): Tokens | undefined => {
    const value = localStorage.getItem(TOKENS);
    if(value == null) {
        return undefined;
    }
    
    const tokens = JSON.parse(value) as Tokens;
    return tokens;
}

const getState = () => {
    const tokens = getTokens();

    let data = undefined as TokenData | undefined;
    if(tokens?.accessToken != undefined) {
        data = new TokenData(tokens.accessToken, tokens.refreshToken);
    }
    return data;
}


let refreshPromise: {
    promise: Promise<AuthResponse>,
    refreshToken: string,
} | undefined = undefined;

interface UserDetails {
    readonly email: string;
    readonly name: string;
    readonly isAdmin: boolean;
    readonly merchantId: string | undefined;
    readonly subMerchantId: string | undefined;
    readonly merchantActivated: boolean;
    readonly token: string;
}

interface AuthContextType {
    readonly signIn: (email: string, password: string) => Promise<void>;
    readonly signOut: () => void;
    readonly switchMerchant: (merchantId: string) => Promise<void>;
    readonly user: UserDetails | undefined;
    readonly client: HttpClient;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
    const authApi = useAuthApi();
    const { t } = useTranslation();
    const toast = useToast();

    const [state, setState] = useState<TokenData | undefined>(getState);

    const signIn = async (email: string, password: string) => {
        try {
            const response = await authApi.jwtAuth({
                email: email,
                password: password,
            });

            saveTokens({
                accessToken: response.access_token,
                refreshToken: response.refresh_token,
            });
            setState(getState);
        } catch (e) {
            if(e instanceof AuthenticationError) {
                toast.error(t("common.apiErrors.InvalidCredentials"), {
                    title: null,
                });
            }
            throw e;
        }
    }

    const signOut = () => {
        saveTokens(undefined);
        setState(getState);
    }

    const switchMerchant = async (merchantId: string) => {
        if(state?.refreshToken == undefined) {
            throw new UnauthorizedException(); 
        }

        try {
            const response = await authApi.jwtRefresh({
                merchantId: merchantId,
                refreshToken: state.refreshToken,
            });
            saveTokens({
                accessToken: response.access_token,
                refreshToken: response.refresh_token,
            });
            setState(getState);
        } catch (e) {
            if(e instanceof AuthenticationError) {
                saveTokens(undefined);
                setState(getState);
                return;
            }
            throw e;
        }
    }
    

    const httpClient = useMemo<HttpClient>(() => {
        const refreshToken = async (refreshToken: string, merchantId?: string) => {
            const promise = await navigator.locks.request("AuthContextProvider.RefreshToken.Lock", () => {
                if(state == undefined) {
                    throw new AuthenticationError("No refresh token");
                }

                if (refreshPromise != undefined && refreshPromise.refreshToken == refreshToken) {
                    return refreshPromise.promise;
                }
                
                console.debug("Merchant refreshing JWT for token: ", refreshToken);
                const promise = authApi.jwtRefresh({
                    merchantId: merchantId,
                    refreshToken: refreshToken,
                }).then(r => {
                    saveTokens({
                        accessToken: r.access_token,
                        refreshToken: r.refresh_token,
                    });
                    setState(getState);
                    return r;
                });
                refreshPromise = {
                    promise: promise,
                    refreshToken: refreshToken,
                }
                return promise;
            });
            return await promise;
        }

        const guard = async <TResponse = void>(call: (token: string) => Promise<TResponse>) => {
            if(state == undefined) {
                throw new UnauthorizedException(); 
            }

            try {
                return await call(state.accessToken);
            } catch (e) {
                if(e instanceof UnauthorizedException) {
                    try {                       
                        const refreshResponse = await refreshToken(state.refreshToken, state.subMerchantId ?? state.merchantId);
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
            get: async <TResponse = void>(url: string, headers?: HeadersInit) => await guard(token => HttpHelper.Client.get<TResponse>(url, {
                ...(headers ?? {}),
                "Authorization": `Bearer ${token}`,
            })),
            post: async <TResponse = void>(url: string, requestData?: any, headers?: HeadersInit) => await guard(token => HttpHelper.Client.post<TResponse>(url, requestData, {
                ...(headers ?? {}),
                "Authorization": `Bearer ${token}`,
            })),
            put: async <TResponse = void>(url: string, requestData?: any, headers?: HeadersInit) => await guard(token => HttpHelper.Client.put<TResponse>(url, requestData, {
                ...(headers ?? {}),
                "Authorization": `Bearer ${token}`,
            })),
            patch: async <TResponse = void>(url: string, requestData?: any, headers?: HeadersInit) => await guard(token => HttpHelper.Client.patch<TResponse>(url, requestData, {
                ...(headers ?? {}),
                "Authorization": `Bearer ${token}`,
            })),
            delete: async <TResponse = void>(url: string, requestData?: any, headers?: HeadersInit) => await guard(token => HttpHelper.Client.delete<TResponse>(url, requestData, {
                ...(headers ?? {}),
                "Authorization": `Bearer ${token}`,
            })),
        }
    }, [state?.accessToken, state?.refreshToken])

    return (
        <AuthContext.Provider value={{
            signIn,
            signOut,
            switchMerchant,
            user: state == undefined ? undefined :  {
                name: state.email,
                email: state.email,
                isAdmin: state.isAdmin,
                merchantId: state.merchantId,
                subMerchantId: state.subMerchantId,
                merchantActivated: state.isActivated == true,
                token: state.accessToken,
            },
            client: httpClient,
        }}
        >
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = (): AuthContextType => {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error('useAuth must be used within a AuthProvider');
    }
    return context;
};

export const useAuthenticatedUser = (): UserDetails => {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error('useAuthenticatedUser must be used within a AuthProvider');
    }

    if(context.user == undefined) {
        throw new Error('useAuthenticatedUser can only be used when user is authenticated');
    }
    return context.user;
};

export const useAuthenticatedHttpClient = (): HttpClient => {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error('useAuthenticatedHttpClient must be used within a AuthProvider');
    }
    return context.client;
}

interface DecodedToken {
    readonly aud: string;
    readonly exp: number;
    readonly email: string;
    readonly sub: string;
    readonly iss: string;
    readonly jti: string;
    readonly merchant_id?: string;
    readonly sub_merchant_id?: string;
    readonly activated_at?: number;
    readonly role?: string[];
}
class TokenData {
    public readonly accessToken: string;
    public readonly refreshToken: string;
    
    public readonly jwtId: string;
    public readonly audience: string;
    public readonly issuer: string;
    public readonly expire: number;
    public readonly userId: string;
    public readonly email: string;
    public readonly merchantId?: string;
    public readonly subMerchantId?: string;
    public readonly isAdmin: boolean;
    public readonly isActivated: boolean;

    constructor(accessToken: string, refreshToken: string) {
        this.accessToken = accessToken;
        this.refreshToken = refreshToken;

        const decoded = jwtDecode<DecodedToken>(accessToken);

        this.jwtId = decoded.jti;
        this.audience = decoded.aud as string;
        this.expire = decoded.exp;
        this.userId = decoded.sub;
        this.email = decoded.email;
        this.issuer = decoded.iss;
        this.merchantId = decoded.merchant_id;
        this.subMerchantId = decoded.sub_merchant_id;
        this.isActivated = decoded.activated_at != undefined;
        this.isAdmin = decoded.role?.find(p => ["Admin", "SuperAdmin"].includes(p)) != undefined;
    }
}