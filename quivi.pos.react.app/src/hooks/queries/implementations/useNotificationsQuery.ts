import { useMemo } from "react";
import { NotificationMessage } from "../../api/Dtos/notifications/NotificationMessage";
import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { GetNotificationMessagesRequest } from "../../api/Dtos/notifications/GetNotificationMessagesRequest";

export const useNotificationsQuery = (request: GetNotificationMessagesRequest | undefined) => {
    const queryResult = useQueryable({
        queryName: "useNotificationsQuery",
        entityType: getEntityType(Entity.Notifications),
        request: request == undefined ? undefined : {
            ...request,
        },
        getId: (e: NotificationMessage) => e.id,
        query: async _ => ({
            data: [] as NotificationMessage[],
            page: 0,
            totalPages: 0,
            totalItems: 0,
        }),

        refreshOnAnyUpdate: true, 
    })

    const result = useMemo(() => ({
        isFirstLoading: queryResult.isFirstLoading,
        isLoading: queryResult.isLoading,
        data: queryResult.data,
        page: queryResult.response?.page ?? 0,
        totalPages: queryResult.response?.totalPages ?? 0,
        totalItems: queryResult.response?.totalItems ?? 0,
    }), [queryResult])

    return result;
}