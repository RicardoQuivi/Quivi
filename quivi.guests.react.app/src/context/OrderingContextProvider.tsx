import { createContext, useContext, useEffect, useMemo, useState } from "react";
import type { Order } from "../hooks/api/Dtos/orders/Order";
import type { QueryResult } from "../hooks/queries/QueryResult";
import { useAppContext } from "./AppContextProvider";
import { useBrowserStorageService } from "../hooks/useBrowserStorageService";
import { useOrdersQuery } from "../hooks/queries/implementations/useOrdersQuery";
import type { ICart } from "./cart/ICart";
import { useTranslation } from "react-i18next";
import type { IBaseItem } from "./cart/item";
import type { ICartItem, ICartModifier } from "./cart/ICartItem";
import { OrderState } from "../hooks/api/Dtos/orders/OrderState";
import { ItemsHelper } from "../helpers/ItemsHelper";
import type { OrderItem } from "../hooks/api/Dtos/orders/OrderItem";
import { useOrderMutator } from "../hooks/mutators/useOrderMutator";
import { toast } from "react-toastify";
import { useMenuItemsQuery } from "../hooks/queries/implementations/useMenuItemsQuery";
import type { MenuItem, MenuItemModifierGroup } from "../hooks/api/Dtos/menuItems/MenuItem";
import { useSessionsQuery } from "../hooks/queries/implementations/useSessionsQuery";
import { JobPromise } from "../hooks/signalR/promises/JobPromise";
import { useWebEvents } from "../hooks/signalR/useWebEvents";
import { useJobsApi } from "../hooks/api/useJobsApi";

interface OrderingContextType {
    readonly orders: QueryResult<Order[]>;
    readonly cart: ICart;
}
const OrderingContext = createContext<OrderingContextType | undefined>(undefined);

const mergeData = (queries: QueryResult<Order[]>[]): QueryResult<Order[]> => {
    let result = {
        isLoading: false,
        isFirstLoading: false,
        data: [] as Order[],
    }
    const map = new Map<string, Order>();
    for(const query of queries) {
        result.isFirstLoading ||= query.isFirstLoading;
        result.isLoading ||= query.isLoading;
        for(const order of query.data) {
            map.set(order.id, order);
        }
    }
    result.data = Array.from(map.values())
    return result;
}

interface IModifiableCartItem extends ICartItem {
    quantity: number;
}

export const OrderingContextProvider = (props: {
    readonly children: React.ReactNode;
}) => {
    const { t } = useTranslation();
    const appContext = useAppContext();
    const orderMutator = useOrderMutator();
    const webEvents = useWebEvents();
    const jobsApi = useJobsApi();

    const storage = useBrowserStorageService();
    const storedOrderId = storage.getOrderId();

    const [orderId, setOrderId] = useState(() => storedOrderId);
    const channelId = appContext?.channelId;
    const merchantId = appContext?.merchantId;

    const orderQuery = useOrdersQuery(!orderId || !channelId ? undefined : {
        ids: [orderId],
        page: 0,
        pageSize: 1,
    });
    const order = useMemo(() => orderQuery.data.length == 0 ? undefined : orderQuery.data[0], [orderQuery.data])
    
    const sessionQuery = useSessionsQuery(appContext == undefined ? undefined : {
        channelId: appContext.channelId,
    });
    const sessionOrdersQuery = useOrdersQuery(!channelId || sessionQuery.data == undefined || appContext?.features.allowsSessions != true ? undefined : {
        channelIds: [channelId],
        sessionId: sessionQuery.data.id,
        page: 0,
        pageSize: undefined,
    })

    const pendingAprovalOrdersQuery = useOrdersQuery(!channelId || appContext?.features.physicalKiosk !== false ? undefined : {
        channelIds: [channelId],
        page: 0,
    })

    const orders = useMemo(() => {
        const queries = [] as QueryResult<Order[]>[];
        if(!!orderId && appContext?.features.physicalKiosk != true) {
            queries.push(orderQuery);
        }

        if(channelId != undefined) {
            if(sessionQuery.data != undefined && appContext?.features.allowsSessions == true) {
                queries.push(sessionOrdersQuery);
            }

            if(appContext?.features.physicalKiosk != true) {
                queries.push(pendingAprovalOrdersQuery);
            }
        }

        return mergeData(queries);
    }, [
        orderQuery.data, orderQuery.isFirstLoading, orderQuery.isFirstLoading,
        sessionOrdersQuery.data, sessionOrdersQuery.isFirstLoading, sessionOrdersQuery.isFirstLoading,
        pendingAprovalOrdersQuery.data, pendingAprovalOrdersQuery.isFirstLoading, pendingAprovalOrdersQuery.isFirstLoading,
    ])

    //#region Cart Management
    const [state, setState] = useState({
        orderId: undefined as string | undefined,
        items: [] as ICartItem[],
        fields: {} as Record<string, string>,
        atDate: undefined as Date | undefined,
        outOfSyncTimeout: undefined as undefined | (() => Promise<Order>),
    })

    const addItem = (item: IBaseItem | ICartItem) => setState(s => {
        if(channelId == undefined || merchantId == undefined) {
            return s;
        }

        const existingItem = getExactItem(item, s.items);
        const quantity = 'quantity' in item ? item.quantity : 1;
        const items = [...s.items];

        if (existingItem) {
            existingItem.quantity += quantity;
        } else {
            let modifiers: ICartModifier[] = [];
            if('modifiers' in item) {
                modifiers = item.modifiers;
            }

            const itemToAdd = {
                ...item,
                quantity: quantity,
                modifiers: modifiers,
            }

            items.push(itemToAdd);
        }
        
        let timeoutFunc: () => any;
        if(!orderId || (order != undefined && order.state != OrderState.Draft)) {
            timeoutFunc = async () => {
                const response = await orderMutator.create({
                    merchantId: merchantId,
                    channelId: channelId,
                    items: items.map(x => ({
                        menuItemId: x.id,
                        quantity: x.quantity,
                        modifierGroups: x.modifiers.map(m => ({
                            modifierId: m.id,
                            selectedOptions: m.selectedOptions.map(o => ({
                                menuItemId: o.id,
                                quantity: o.quantity,
                            })),
                        }))
                    })),
                    payLater: false,
                });

                setState(s => ({
                    ...s,
                    orderId: response.id,
                }))
                setOrderId(response.id);
                return response;
            }
        } else {
            timeoutFunc = async () => {
                const response = await orderMutator.update({
                    id: orderId,
                    items: items.map(x => ({
                        menuItemId: x.id,
                        quantity: x.quantity,
                        modifierGroups: (x.modifiers ?? []).map(m => ({
                            modifierId: m.id,
                            selectedOptions: m.selectedOptions.map(o => ({
                                menuItemId: o.id,
                                quantity: o.quantity,
                            })),
                        }))
                    })),
                    fields: s.fields,
                });
                return response;
            };
        }
        return {
            ...s,
            items: items,
            outOfSyncTimeout: timeoutFunc,
        }
    });

    const updateItem = (oldItem: ICartItem, newItem: ICartItem) => setState(s => {
        if(!orderId) {
            return s;
        }

        const existingItem = getExactItem(oldItem, s.items);
        if (existingItem == null) {
            return s;
        }

        const items = [...s.items];
        const index = items.indexOf(existingItem);
        items[index] = newItem;
        
        return {
            ...s,
            items: items,
            outOfSyncTimeout: async () => {
                const response = await await orderMutator.update({
                    id: orderId,
                    items: items.map(x => ({
                        menuItemId: x.id,
                        quantity: x.quantity,
                        modifierGroups: (x.modifiers ?? []).map(m => ({
                            modifierId: m.id,
                            selectedOptions: m.selectedOptions.map(o => ({
                                menuItemId: o.id,
                                quantity: o.quantity,
                            })),
                        }))
                    })),
                    fields: s.fields,
                });
                return response;
            },
        }
    });

    const removeItem = (item: IBaseItem, allQuantity?: boolean) => setState(s => {
        if(!orderId) {
            return s;
        }

        const it = getExactItem(item, s.items);
        const items = [...s.items];
        if(it != null) {
            it.quantity -= allQuantity ? it.quantity : 1;

            if (it.quantity == 0){
                items.splice(items.findIndex(i => it == i), 1);
            }
        }

        return {
            ...s,
            items: items,
            outOfSyncTimeout: async () => {
                const response = await orderMutator.update({
                    id: orderId,
                    items: items.map(x => ({
                        menuItemId: x.id,
                        quantity: x.quantity,
                        modifierGroups: (x.modifiers ?? []).map(m => ({
                            modifierId: m.id,
                            selectedOptions: m.selectedOptions.map(o => ({
                                menuItemId: o.id,
                                quantity: o.quantity,
                            })),
                        }))
                    })),
                    fields: s.fields,
                });
                return response;
            },
        }
    });

    const editFields = (value: Record<string, string>) => setState(s => {
        if(orderId == undefined) {
            return s;
        }

        if(order?.state === OrderState.Draft){
            const timeoutFunc = async () => {
                const response = await orderMutator.update({
                    id: orderId,
                    items: s.items.map(x => ({
                        menuItemId: x.id,
                        quantity: x.quantity,
                        modifierGroups: (x.modifiers ?? []).map(m => ({
                            modifierId: m.id,
                            selectedOptions: m.selectedOptions.map(o => ({
                                menuItemId: o.id,
                                quantity: o.quantity,
                            })),
                        }))
                    })),
                    fields: value,
                });
                return response;
            };

            return {
                ...s,
                fields: value,
                outOfSyncTimeout: timeoutFunc,
            }
        }

        return {
            ...s,
            fields: value,
        }
    });

    const submit = async (payLater?: boolean): Promise<Order> => {
        let _order: Order | null = null;
        let outOfSyncTimeout = state.outOfSyncTimeout;
        let _orderId = state.orderId ?? orderId;
        if(_orderId == undefined) {
            throw new Error("This should never happen");
        }

        let hasChanges = false;
        if(outOfSyncTimeout != undefined) {
            _order = await outOfSyncTimeout();
            hasChanges = true;
        }

        if(payLater == true) {
            try {
                const result = await orderMutator.submit(_orderId);
                _order = result.order;

                const promise = new JobPromise(result.jobId, webEvents.client, async (jobId) => {
                    const response = await jobsApi.get({
                        ids: [jobId],
                    });
                    return response.data[0].state;
                })
                await promise;
                hasChanges = true;
            } catch (e) {
                hasChanges = true;
                throw e;
            }
        } else {
            hasChanges = true;
        }

        if(hasChanges) {
            setState(s => ({
                ...s,
                outOfSyncTimeout: undefined,
                orderId: undefined,
                items: [],
                atDate: undefined,
                fields: {},
            }))
        }
        return (_order ?? order)!;
    }
    //#endregion

    const baseItemIds = useMemo(() => order?.items.map(i => i.id) ?? [], [order]);
    const modifierIds = useMemo(() => order?.items.map(i => [
                                                i.id, 
                                                ...i.modifiers?.map(m => m.selectedOptions.map(opt => opt.id))
                                                                .reduce((r, ids) => [...r, ...ids], []) ?? []
                                            ])
                                    .reduce((r, ids) => [...r, ...ids], []) ?? [], [order]);

    const baseItemsQuery = useMenuItemsQuery(order == undefined || baseItemIds.length == 0 ? undefined : {
        channelId: order.channelId,
        ids: baseItemIds,
        atDate: state.atDate,
        page: 0,
        pageSize: baseItemIds.length,
    })
    const modifierItemsQuery = useMenuItemsQuery(order == undefined || modifierIds.length == 0 ? undefined : {
        channelId: order.channelId,
        ids: modifierIds,
        atDate: state.atDate,
        ignoreCalendarAvailability: state.atDate == undefined,
        page: 0,
        pageSize: modifierIds.length,
    })
    
    useEffect(() => storage.saveOrderId(orderId), [orderId])
    
    useEffect(() => {
        if(order == undefined && orderQuery.isFirstLoading == false) {
            setOrderId(null);
            return;
        }

        if(order == undefined || merchantId == undefined || channelId == undefined) {
            return;
        }

        if(merchantId != order.merchantId || channelId != order.channelId || order.state != OrderState.Draft) {
            setOrderId(null);
            return;
        }

        if(state.items.length > 0) {
            return;
        }
        
        if(baseItemsQuery.isFirstLoading) {
            return;
        }

        if(modifierItemsQuery.isFirstLoading) {
            return;
        }

        const { cartItems, unavailableItems } = getCartItems(order, baseItemsQuery.data, modifierItemsQuery.data);
        for(const item of unavailableItems) {
            toast.warn(t("digitalMenu.availabilityChangedMsg", { items: [item.name].join(", ") }));
        }

        const items = [] as ICartItem[];
        for(const item of cartItems) {
            let modifiers: ICartModifier[] = [];
            if('modifiers' in item) {
                modifiers = item.modifiers;
            }

            const itemToAdd = {
                ...item,
                quantity: item.quantity,
                modifiers: modifiers,
            }

            items.push(itemToAdd);
        }
        const fields = order?.fields.reduce((r, f) => {
            r[f.id] = f.value;
            return r;
        }, {} as Record<string, string>)

        setState(s => ({
            ...s,
            items: items,
            fields: fields,
            orderId: order.id,
        }))
    }, [
        orderQuery.isFirstLoading, order,
        merchantId, 
        channelId,
        baseItemsQuery.data, baseItemsQuery.isFirstLoading,
        modifierItemsQuery.data, modifierItemsQuery.data
    ]);

    useEffect(() => {
        const outOfSyncTimeout = state.outOfSyncTimeout;
        if(!outOfSyncTimeout) {
            return;
        }

        if(!orderId) {
            outOfSyncTimeout();
            setState(s => ({
                ...s,
                outOfSyncTimeout: undefined,
            }))
            return;
        }

        const timeout = setTimeout(() => {
            outOfSyncTimeout();
            setState(s => ({
                ...s,
                outOfSyncTimeout: undefined,
            }))
        }, 5000);
        return () => clearTimeout(timeout);
    }, [state.outOfSyncTimeout])

    const cart = useMemo<ICart>(() => ({
        isInitializing: !orderId ? false : state.orderId != orderId,
        items: state.items,
        total: ItemsHelper.getItemsPrice(state.items),
        totalItems: state.items.reduce((r, i) => r += i.quantity, 0),
        getQuantityInCart: (item: IBaseItem | ICartItem, exact: boolean) => getCartQuantity(item, exact, state.items),
        fields: state.fields,
        addItem: addItem,
        updateItem: updateItem,
        removeItem: removeItem,
        editFields: editFields,
        scheduledDate: state.atDate,
        setScheduleDate: async () => ({
            confirm: async () => {},
            date: undefined,
            unavailableItems: [],
        }),
        submit: submit,
    }), [state.orderId, state.items, state.fields, state.atDate])

    return (
        <OrderingContext.Provider
            value={{
                orders: orders,
                cart: cart,
            }}
        >
            {props.children}
        </OrderingContext.Provider>
    );
}

export const useOrdersContext = (): QueryResult<Order[]> => {
    const context = useContext(OrderingContext);
    if(context === undefined) {
        throw Error("useOrdersContext can only be used inside OrderingContextProvider");
    }
    return context.orders;
}

export const useCart = (): ICart => {
    const context = useContext(OrderingContext);
    if(context === undefined) {
        throw Error("useCart can only be used inside OrderingContextProvider");
    }
    return context.cart;
}

const getExactItem = (item: IBaseItem | ICartItem, items: ICartItem[]): IModifiableCartItem | null => {
    const baseItems = items.filter(it => it.id === item.id);
    const referenceModifiers = 'modifiers' in item ? (item.modifiers ?? []) : [];
    const itemsWithSameNumberOfModifiers = baseItems.filter(it => (it.modifiers?.length ?? 0) == referenceModifiers.length);

    let result = itemsWithSameNumberOfModifiers;
    for(let m of referenceModifiers) {
        result = result.filter(it => it.modifiers.find(mt => {
            if(mt.id != m.id) {
                return false;
            }

            if(m.selectedOptions == null) {
                return true;
            }

            if(mt.selectedOptions.length != m.selectedOptions.length) {
                return false;
            }

            for(let o of m.selectedOptions) {
                if(mt.selectedOptions.find(ot => ot.id == o.id && ot.quantity == o.quantity) == undefined) {
                    return false;
                }
            }

            return true;
        }) != undefined);
    }

    return result.length > 0 ? result[0] : null;
}

const getCartQuantity = (item: IBaseItem | ICartItem, exact: boolean, items: ICartItem[]): number => {
    if(exact) {
        const it = getExactItem(item, items);
        return it?.quantity ?? 0;
    }

    const baseItems = items?.filter(it => it.id === item.id) ?? [];
    return baseItems.reduce((sum, current) => sum + current.quantity, 0);
}

const getCartItems = (order: Order, baseItems: MenuItem[], modifierItems: MenuItem[]) => {
    if(order.items.length == 0) {
        return {
            cartItems: [],
            unavailableItems: [],
        };
    }

    const mappedItems = [...baseItems, ...modifierItems].reduce((map, obj) => {
        map.set(obj.id, obj);
        return map;
    }, new Map<string, MenuItem>());
    
    const cartItems: ICartItem[] = [];
    const unavailableItems: OrderItem[] = [];
    order.items.forEach(orderItem => {
        const apiItem = mappedItems.get(orderItem.id);
        if(apiItem == undefined) {
            unavailableItems.push(orderItem);
            return;
        }

        const modifiersMap = apiItem.modifiers.reduce((r, m) => {
            r.set(m.id, m);
            return r;
        }, new Map<string, MenuItemModifierGroup>())
        
        try {
            const itemToAdd = {
                id: apiItem.id,
                quantity: orderItem.quantity,
                name: apiItem.name,
                description: apiItem.description,
                imageUrl: apiItem.imageUrl,
                price: apiItem.price,
                priceType: apiItem.priceType,
                isAvailable: apiItem.isAvailable,
                modifiers: orderItem.modifiers?.map(m => {
                    const mod = modifiersMap.get(m.id);
                    if(mod == undefined) {
                        throw Error("This item is not available");
                    }
    
                    const selectedOptions = m.selectedOptions.map(opt => {
                        const o = mappedItems.get(opt.id);
                        if(o == undefined) {
                            throw Error("This item is not available");
                        }

                        return {
                            ...o,
                            price: opt.amount,
                            quantity: opt.quantity,
                            modifiers: [],

                            id: o.id,
                            name: o.name,
                            description: o.description,
                            imageUrl: o.imageUrl,
                            priceType: o.priceType,
                            isAvailable: o.isAvailable,
                        };
                    });
                    return {
                        id: mod.id,
                        name: mod.name,
                        minSelection: mod.minSelection,
                        maxSelection: mod.maxSelection,
                        selectedOptions: selectedOptions,
                        options: mod.options,
                    };
                }) ?? [],
            }
            cartItems.push(itemToAdd);
        } catch {
            unavailableItems.push(orderItem);
            return;
        }
    });
    return {
        cartItems: cartItems,
        unavailableItems: unavailableItems,
    }
}