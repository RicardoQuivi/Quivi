import { createContext, useContext, useState, ReactNode, useMemo, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useToast } from '../ToastProvider';
import { Employee } from '../../hooks/api/Dtos/employees/Employee';
import { useAuth } from '../AuthContextProvider';
import { AuthenticationError, useAuthApi } from '../../hooks/api/useAuthApi';
import { useEmployeesQuery } from '../../hooks/queries/implementations/useEmployeesQuery';
import { useUserInactivity } from '../../hooks/useUserInactivity';
import { EmployeeTokenData } from './EmployeeToken';

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

const getLogoutTimeout = (employee: Employee | undefined, defaultTimeout: number): number => {
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
}

const EmployeeContext = createContext<EmployeeContextType | undefined>(undefined);

export const EmployeeProvider = ({ children }: { children: ReactNode }) => {
    const auth = useAuth();
    const authApi = useAuthApi();
    const { t } = useTranslation();
    const toast = useToast();

    const [state, setState] = useState(getState());
    const [expiresAt, setExpiresAt] = useState<number>();

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
    const activity = useUserInactivity({ timeout: getLogoutTimeout(loggedEmployeeQuery.data.length > 0 ? loggedEmployeeQuery.data[0] : undefined, 2 * 60 * 1000)});

    const login = async (employeeId: string, pinCode: string) => {
        if(auth.token == undefined) {
            throw new Error("Cannot login an employee because no merchant is available");
        }

        try {
            const response = await authApi.employeeLogin(auth.token, employeeId, pinCode);

            saveTokens({
                accessToken: response.access_token,
                refreshToken: response.refresh_token,
            });
            setState(getState());
        } catch (e) {
            signOut();
            if(e instanceof AuthenticationError) {
                toast.error(t("common.apiErrors.InvalidCredentials"));
                return;
            }
            throw e;
        }
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

    const signOut = () => {
        saveTokens(undefined);
        setState(getState());
    }
    
    useEffect(() => {
        if(activity.isInactive == false) {
            return;
        }

        signOut();
    }, [activity.isInactive, loggedEmployee])

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

    const result = useMemo(() => ({
        token: state?.accessToken,
        login: login,
        signOut: signOut,
        employee: loggedEmployee,
    }), [state, loggedEmployee, auth.token])
    
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