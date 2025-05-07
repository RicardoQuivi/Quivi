import { createContext, useContext, useState, ReactNode, useEffect } from 'react';
import { jwtDecode } from 'jwt-decode';
import { AuthenticationError, useAuthApi } from '../hooks/api/useAuthApi';

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

interface AuthContextType {
    readonly isAuth: boolean;
    readonly merchantId: string | undefined;
    readonly subMerchantId: string | undefined;
    readonly signIn: (token: string) => Promise<void>;
    readonly token: string | undefined;
    readonly isAdmin: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
    const authApi = useAuthApi();

    const [state, setState] = useState(getState());
    const [expiresAt, setExpiresAt] = useState<number>();

    const signIn = async (token: string) => {
        const response = await authApi.tokenExchange({
            subjectToken: token,
        });

        saveTokens({
            accessToken: response.access_token,
            refreshToken: response.refresh_token,
        });
        setState(getState());
    }

    const refreshJwt = async (refreshToken: string, retry: boolean) => {
        try {
            console.debug("Refreshing JWT!")
            const response = await authApi.jwtRefresh({
                refreshToken: refreshToken,
            });
            console.debug("Refreshing JWT response:", response)

            saveTokens({
                accessToken: response.access_token,
                refreshToken: response.refresh_token,
            });
            setState(getState());
        } catch (e) {
            if(e instanceof AuthenticationError) {
                saveTokens(undefined);
                setState(getState());
                return;
            }

            if(retry) {
                setTimeout(() => refreshJwt(refreshToken, retry), 1000);
            }
        }
    }

    useEffect(() => {
        if(state == undefined) {
            setExpiresAt(undefined);
            return;
        }

        const tokenData = state;
        setExpiresAt(tokenData.expire * 1000);
    }, [state]);

    useEffect(() => {
        if(expiresAt == undefined) {
            return;
        }

        const refreshToken = state?.refreshToken;
        if(refreshToken == undefined) {
            setExpiresAt(undefined);
            return;
        }

        const timeToExpire = expiresAt - Date.now();
        if(timeToExpire <= 0) {
            saveTokens({
                accessToken: undefined,
                refreshToken: refreshToken,
            });
            setState(getState());
        }

        const delay = timeToExpire - 15000;
        if (delay <= 0) {
            refreshJwt(refreshToken, true);
            return;
        }

        const timeout = setTimeout(() => refreshJwt(refreshToken, false), delay);
        return () => clearTimeout(timeout);
    }, [expiresAt])
    
    return (
        <AuthContext.Provider value={{
            isAuth: state != undefined,
            token: state?.accessToken,
            signIn,
            merchantId: state?.merchantId,
            subMerchantId: state?.subMerchantId,
            isAdmin: state?.isAdmin ?? false,
        }}>
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
    readonly role: string[];
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
        this.isAdmin = decoded.role.find(p => ["Admin", "SuperAdmin"].includes(p)) != undefined;
    }
}