import { useMemo } from "react";
import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import type { GetSessionRequest } from "../../api/Dtos/sessions/GetSessionRequest";
import type { Session } from "../../api/Dtos/sessions/Session";
import { useSessionsApi } from "../../api/useSessionsApi";
import type { QueryResult } from "../QueryResult";
import { ItemsHelper } from "../../../helpers/ItemsHelper";
import BigNumber from "bignumber.js";

interface ExtendedSession extends Session {
    readonly total: number;
    readonly unpaid: number;
    readonly paid: number;
    readonly discount: number;
}
export const useSessionsQuery = (request: GetSessionRequest | undefined): QueryResult<ExtendedSession | undefined> => {
    const api = useSessionsApi();

    const query = useQueryable({
        queryName: "useSessionsQuery",
        entityType: getEntityType(Entity.Sessions),
        getId: (item: Session) => item.id,
        request: request,
        query: async request => {
            const response = await api.get(request);
            return {
                data: response.data == undefined ? [] : [response.data],
            }
        },

        refreshOnAnyUpdate: true,
        canUseOptimizedResponse: _ => false,
        getResponseFromEntities: (e) => ({
            data: e,
            page: 0,
            totalPages: 1,
            totalItems: 1,
        }),
    })
    
    const result = useMemo(() => ({
        isFirstLoading: query.isFirstLoading,
        isLoading: query.isLoading,
        data: query.data.length == 0 ? undefined : getSession(query.data[0]),
    }), [query])
    
    return result;
}

const getSession = (session: Session): ExtendedSession => {
    let total = new BigNumber(0);
    let unpaid = new BigNumber(0);
    let paid = new BigNumber(0);
    let discount = new BigNumber(0);
    session.items.forEach((item) => {
        let itemAbsoluteDiscount = ItemsHelper.appliedDiscountTotal(item);
        let itemTotal = new BigNumber(item.price).multipliedBy(new BigNumber(item.quantity));

        if(item.extras.length > 0) {
            let modifiersTotal = item.extras.reduce((r, m) => r.plus(new BigNumber(m.price).multipliedBy(new BigNumber(m.quantity))), new BigNumber(0));
            itemTotal = itemTotal.plus(modifiersTotal.multipliedBy(item.quantity));
        }

        total = total.plus(itemTotal);
        switch(item.isPaid)
        {
            case true: paid = paid.plus(itemTotal); break;
            case false: unpaid = unpaid.plus(itemTotal); break;
        }

        discount = discount.plus(new BigNumber(item.quantity).multipliedBy(itemAbsoluteDiscount));
    });

    return {
        ...session,
        total: total.toNumber(),
        unpaid: unpaid.toNumber(),
        paid: paid.toNumber(),
        discount: discount.toNumber(),
    }
}