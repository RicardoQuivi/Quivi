import { createContext, ReactNode, useContext, useEffect, useMemo } from "react";
import { ICartSession } from "../../hooks/pos/session/ICartSession";
import { useChannelsQuery } from "../../hooks/queries/implementations/useChannelsQuery";
import { useStoredState } from "../../hooks/useStoredState";
import useBrowserStorage, { BrowserStorageType } from "../../hooks/useBrowserStorage";
import { Employee } from "../../hooks/api/Dtos/employees/Employee";
import { useLoggedEmployee } from "./LoggedEmployeeContextProvider";
import { useCartSession } from "../../hooks/pos/session/useCartSession";
import { ChannelPermissions, useAllowedActions } from "../../hooks/pos/useAllowedActions";
import { QueryResult } from "../../hooks/queries/QueryResult";

interface PosSessionContextType {
    readonly signOut: () => void;
    readonly employee: Employee;
    readonly token: string;
    readonly cartSession: ICartSession;
    readonly permissions: QueryResult<ChannelPermissions>;
    readonly changeToSession: (channelId: string) => void;
}
const PosSessionContext = createContext<PosSessionContextType | undefined>(undefined);

export const PosSessionContextProvider = ({ children }: { children: ReactNode }) => {
    const employeeContext = useLoggedEmployee();
    
    const searchParamsHook = useBrowserStorage(BrowserStorageType.UrlParam);
    const [channelId, setChannelId] = useStoredState<string | undefined>("channelId", undefined, searchParamsHook);

    const defaultChannelWithSessionQuery = useChannelsQuery({
        ids: !channelId ? undefined : [channelId],
        allowsSessionsOnly: true,
        page: 0,
    })

    const cartSession = useCartSession(channelId);
    const permissions = useAllowedActions(!channelId ? undefined : channelId);
    
    useEffect(() => {
        if(defaultChannelWithSessionQuery.data.length == 0) {
            return;
        }
        setChannelId(defaultChannelWithSessionQuery.data[0].id);
    }, [defaultChannelWithSessionQuery.data])

    const state = useMemo(() => ({
        ...employeeContext,
        changeToSession: setChannelId,
        cartSession: cartSession,
        permissions: permissions.data.channelId != channelId ? {
            data: {
                ...permissions.data,
                channelId: channelId ?? "",
            },
            isFirstLoading: true,
            isLoading: true,
        } : permissions,
        channelId,
    }), [channelId, employeeContext, cartSession, permissions])
    
    return (
        <PosSessionContext.Provider value={state}>
            {children}
        </PosSessionContext.Provider>
    );
}

export const usePosSession = (): PosSessionContextType => {
    const context = useContext(PosSessionContext);
    if (!context) {
        throw new Error('usePosSession must be used within a PosContextProvider');
    }
    return context;
}