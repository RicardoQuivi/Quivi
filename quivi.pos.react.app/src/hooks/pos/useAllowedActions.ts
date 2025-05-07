import { useEffect, useState } from "react";
import { Employee, EmployeeRestriction } from "../api/Dtos/employees/Employee";
import { useChannelsQuery } from "../queries/implementations/useChannelsQuery";
import { usePosIntegrationsQuery } from "../queries/implementations/usePosIntegrationsQuery";
import { useSessionsQuery } from "../queries/implementations/useSessionsQuery";
import { useLoggedEmployee } from "../../context/pos/LoggedEmployeeContextProvider";
import { useChannelProfilesQuery } from "../queries/implementations/useChannelProfilesQuery";
import { QueryResult } from "../queries/QueryResult";
import { PosIntegration } from "../api/Dtos/posintegrations/PosIntegration";
import { Channel } from "../api/Dtos/channels/Channel";
import { ChannelProfile } from "../api/Dtos/channelProfiles/ChannelProfile";
import { Session } from "../api/Dtos/sessions/Session";

const getDefaultPermissions = (channelId: string | undefined, previousPermission: ChannelPermissions | undefined): ChannelPermissions => ({
    allowsAddingItems: previousPermission?.allowsAddingItems ?? false,
    allowsInvoiceEscPosPrinting: previousPermission?.allowsInvoiceEscPosPrinting ??false,
    allowsPayments: previousPermission?.allowsPayments ??false,
    allowsRemovingItems: previousPermission?.allowsRemovingItems ??false,
    applyDiscounts: previousPermission?.applyDiscounts ??false,
    canViewSessions: previousPermission?.canViewSessions ??false,
    channelId: channelId ?? "",
})

export interface ChannelPermissions {
    readonly channelId: string;
    readonly canViewSessions: boolean;
    readonly allowsPayments: boolean;
    readonly allowsAddingItems: boolean;
    readonly allowsRemovingItems: boolean;
    readonly allowsInvoiceEscPosPrinting: boolean;
    readonly applyDiscounts: boolean;
}
export const useAllowedActions = (channelId: string | undefined): QueryResult<ChannelPermissions> => {
    const posContext = useLoggedEmployee();

    const channelsQuery = useChannelsQuery(channelId == undefined ? undefined :{
        ids: [channelId],
        page: 0,
    })
    const channelProfileQuery = useChannelProfilesQuery(channelsQuery.isLoading || channelsQuery.data.length == 0 ? undefined : {
        ids: [channelsQuery.data[0].channelProfileId],
        page: 0,
    })
    const integrationQuery = usePosIntegrationsQuery(channelProfileQuery.isLoading || channelProfileQuery.data.length == 0 ? undefined : {
        ids: [channelProfileQuery.data[0].posIntegrationId],
        page: 0,
    })
    
    const sessionsQuery = useSessionsQuery(channelId == undefined ? undefined : {
        channelIds: [channelId],
        isOpen: true,
        page: 0,
    })

    const [permissions, setPermissions] = useState<QueryResult<ChannelPermissions>>(getPermissions(undefined, channelId, integrationQuery, channelsQuery, channelProfileQuery, sessionsQuery, posContext.employee));

    useEffect(() => setPermissions(p => getPermissions(p, channelId, integrationQuery, channelsQuery, channelProfileQuery, sessionsQuery, posContext.employee)), [
        channelId,

        integrationQuery.isLoading, 
        integrationQuery.isFirstLoading, 
        integrationQuery.data,

        channelsQuery.isLoading,
        channelsQuery.isFirstLoading,
        channelsQuery.data,
        
        channelProfileQuery.isLoading,
        channelProfileQuery.isFirstLoading,
        channelProfileQuery.data,
        
        sessionsQuery.isLoading,
        sessionsQuery.isFirstLoading,
        sessionsQuery.data,

        posContext.employee,
    ])

    return permissions;
}

const getPermissions = (p: QueryResult<ChannelPermissions | undefined> | undefined, 
                            channelId: string | undefined, 
                            integrationQuery: QueryResult<PosIntegration[]>,
                            channelsQuery: QueryResult<Channel[]>,
                            channelProfileQuery: QueryResult<ChannelProfile[]>,
                            sessionsQuery: QueryResult<Session[]>,
                            employee: Employee): QueryResult<ChannelPermissions> => {
    const integration = integrationQuery.data.length > 0 ? integrationQuery.data[0] : {
        id: channelId ?? undefined,
        isOnline: false,
        allowsPayments: false,
        allowsOpeningSessions: false,
        allowsEscPosInvoices: false,
        allowsAddingItemsToSession: false,
        allowsRemovingItemsFromSession: false,
        applyDiscounts: false,
    }

    if(channelsQuery.isFirstLoading || channelProfileQuery.isFirstLoading || integrationQuery.isFirstLoading) {
        return {
            data: getDefaultPermissions(channelId, p?.data),
            isFirstLoading: true,
            isLoading: true,
        }
    }

    if(channelsQuery.isLoading ||
            channelsQuery.data.length == 0 ||
            channelProfileQuery.isLoading ||
            channelProfileQuery.data.length == 0 || 
            integrationQuery.isLoading ||
            integration.id != channelProfileQuery.data[0].posIntegrationId ||
            sessionsQuery.isLoading) {

        return {
            data: getDefaultPermissions(channelId, p?.data),
            isLoading: true,
            isFirstLoading: p?.isFirstLoading ?? true,
        };
    }
    
    const isSessionOpen = sessionsQuery.data.length != 0 && sessionsQuery.data[0].isOpen;
    let aux = integration.allowsOpeningSessions || isSessionOpen;

    let employeeIsAllowedToRemoveItems = true;
    let canViewSessions = true;
    let canApplyDiscounts = true;
    for(const r of employee.restrictions) {
        if(r == EmployeeRestriction.RemoveItems) {
            employeeIsAllowedToRemoveItems = false;
        }

        if(r == EmployeeRestriction.SessionsAccess) {
            canViewSessions = false;
        }

        if(r == EmployeeRestriction.ApplyDiscounts) {
            canApplyDiscounts = false;
        }

        if(employeeIsAllowedToRemoveItems == false && canViewSessions == false && canApplyDiscounts == false) {
            break;
        }
    }

    return {
        isFirstLoading: false,
        isLoading: false,
        data: {
            channelId: channelsQuery.data[0].id,
            canViewSessions: canViewSessions,
            allowsPayments: integration.isOnline && integration.allowsPayments,
            allowsRemovingItems: integration.isOnline && integration.allowsRemovingItemsFromSession && aux && employeeIsAllowedToRemoveItems,
            allowsAddingItems: integration.isOnline && integration.allowsAddingItemsToSession && aux && canViewSessions,
            allowsInvoiceEscPosPrinting: integration.isOnline && integration.allowsEscPosInvoices,
            applyDiscounts: integration.isOnline && canApplyDiscounts,
        }
    }
}