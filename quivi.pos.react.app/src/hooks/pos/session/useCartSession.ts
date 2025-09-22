import { useEffect, useMemo, useState } from "react";
import { ICartSession, MenuItemWithExtras } from "./ICartSession";
import { SessionExtraItem, SessionItem } from "../../api/Dtos/sessions/SessionItem";
import { MenuItem } from "../../api/Dtos/menuitems/MenuItem";
import { CreateOrder, CreateOrderExtraItem, CreateOrderItem } from "../../api/Dtos/orders/CreateOrdersRequest";
import { Session } from "../../api/Dtos/sessions/Session";
import { useOrdersApi } from "../../api/useOrdersApi";
import { useTranslation } from "react-i18next";
import { useToast } from "../../../context/ToastProvider";
import { useSessionsQuery } from "../../queries/implementations/useSessionsQuery";
import BigNumber from "bignumber.js";
import { useOrdersQuery } from "../../queries/implementations/useOrdersQuery";
import { SortDirection } from "../../api/Dtos/SortableRequest";
import { Order } from "../../api/Dtos/orders/Order";

interface ItemToSync {
    readonly channelId: string;
    readonly item: MenuItem | MenuItemWithExtras | SessionItem;
    readonly patch: CreateOrderItem;
    readonly processingOperation?: {
        readonly orderIds: string[];
    }
}
export const useCartSession = (channelId: string | undefined): ICartSession => {
    const { t } = useTranslation();
    const ordersApi = useOrdersApi();
    const toast = useToast();
  
    const sessionsQuery = useSessionsQuery(!channelId ? undefined : {
        channelIds: [channelId],
        page: 0,
    });

    const currentSession = useMemo(() => {
        if(sessionsQuery.data.length == 0) {
            return getClosedSession(channelId, new Date().toISOString());
        }
        return sessionsQuery.data[0];
    }, [sessionsQuery.data, channelId]);

    const [state, setState] = useState(() => ({
        session: currentSession,
        operationsToSync: [] as ItemToSync[],
        pendingOrderIds: [] as string[],
    }))
    const [outOfSyncTimeout, setOutOfSyncTimeout] = useState<() => Promise<any>>();

    const pendingOrdersQuery = useOrdersQuery(state.pendingOrderIds.length == 0 ? undefined : {
        ids: state.pendingOrderIds,
        page: 0,
        sortDirection: SortDirection.Asc
    });

    const updateSession = (data: Session) => setState(s => {
        let session = s.session;
        let operationsToSync = s.operationsToSync;

        if (data.isOpen == false) {
            session = {
                ...data,
                items: [],
            };
        } else {
            const items = data.items.map(item => ({
                ...item,
            }));
            items.reverse();

            session = {
                ...data,
                items: items,
            };
        }

        if(operationsToSync.length > 0) {
            const syncedOrders = new Set(data.orderIds);
            let newOperationsToSync = [];
            for(const operation of operationsToSync) {
                if(operation.processingOperation == undefined) {
                    newOperationsToSync.push(operation);
                    continue;
                }

                for(const order of operation.processingOperation.orderIds) {
                    if(syncedOrders.has(order) == false) {
                        newOperationsToSync.push(operation);
                        continue;
                    }
                }
            }
            operationsToSync = newOperationsToSync;
        }

        return {
            session: session,
            operationsToSync: operationsToSync,
            pendingOrderIds: s.pendingOrderIds,
        }
    })

    const addItem = (item: MenuItem | MenuItemWithExtras | SessionItem, quantity: number) => setState(s => {
        if(channelId == undefined) {
            return s;
        }

        const isMenuItem = 'menuItemId' in item == false;
        const idToFind = isMenuItem ? item.id : item.menuItemId;
        const discountToApply = !isMenuItem ? item.discountPercentage : 0;
        const price = isMenuItem ? item.price : item.originalPrice;
        
        let extras: CreateOrderExtraItem[] | undefined = undefined;
        if(isMenuItem && 'selectedOptions' in item) {
            extras = [];
            for(const [modifierGroupId, selections] of item.selectedOptions) {
                for(const selection of selections) {
                    extras.push({
                        modifierGroupId: modifierGroupId,
                        menuItemId: selection.menuItemId,
                        price: selection.price,
                        quantity: 1,
                    })
                }
            }
        } else if('extras' in item) {
            extras = [];
            for(const extra of item.extras) {
                extras.push({
                    modifierGroupId: extra.modifierGroupId,
                    menuItemId: extra.menuItemId,
                    quantity: extra.quantity,
                    price: extra.originalPrice,
                })
            }
        }

        const operationsToSync = [...s.operationsToSync, {
            channelId: channelId,
            item: item,
            patch: {
                menuItemId: idToFind,
                quantity: quantity,
                price: price,
                discount: discountToApply,
                extras: extras ?? [],
            } as CreateOrderItem,
            synced: false,
        }];

        return {
            session: s.session,
            operationsToSync: operationsToSync,
            pendingOrderIds: s.pendingOrderIds,
        }
    })

    const removeItem = (item: SessionItem, quantity: number, discount?: number) => setState(s => {
        if(channelId == undefined) {
            return s;
        }

        const discountApplied = discount ?? 0;
        const idToFind = item.menuItemId;

        const extras = [] as SessionExtraItem[];
        for(const extra of item.extras) {
            extras.push({
                modifierGroupId: extra.modifierGroupId,
                originalPrice: extra.originalPrice,
                menuItemId: extra.menuItemId,
                quantity: extra.quantity,
                price: extra.originalPrice,
            })
        }
        
        const operationsToSync = [...s.operationsToSync, {
            channelId: channelId,
            item: item,
            patch: {
                menuItemId: idToFind,
                quantity: -1 * quantity,
                price: item.originalPrice,
                discount: discountApplied,
                extras: extras,
            } as CreateOrderItem,
            synced: false,
        }];

        return {
            session: s.session,
            operationsToSync: operationsToSync,
            pendingOrderIds: s.pendingOrderIds,
        }
    })

    const applyDiscount = (item: SessionItem, quantity: number, discount: number, priceOverride?: number) => setState(s => {
        if(channelId == undefined) {
            return s;
        }

        const idToFind = item.menuItemId;
        const extras = [] as SessionExtraItem[];
        for(const extra of item.extras) {
            extras.push({
                modifierGroupId: extra.modifierGroupId,
                originalPrice: extra.originalPrice,
                menuItemId: extra.menuItemId,
                quantity: extra.quantity,
                price: extra.originalPrice,
            })
        }

        const originalPrice = priceOverride ?? new BigNumber(item.price).times(100).dividedBy(new BigNumber(100).minus(item.discountPercentage)).toNumber();
        const operationsToSync = [...s.operationsToSync, {
            channelId: channelId,
            item: {
                ...item,
                price: new BigNumber(originalPrice).minus(new BigNumber(originalPrice).times(discount).dividedBy(100)).toNumber(),
                originalPrice: originalPrice,
                modifiers: item.extras.map(m => {
                    const originalExtraPrice = m.originalPrice;
                    return {
                        ...m,
                        price: new BigNumber(originalExtraPrice).minus(new BigNumber(originalExtraPrice).times(discount).dividedBy(100)).toNumber(),
                        appliedDiscountPercentage: discount,
                    }
                }),
                appliedDiscountPercentage: discount,
            },
            patch: {
                menuItemId: idToFind,
                quantity: quantity,
                discount: discount,
                price: originalPrice,
                extras: extras?.map(m => {
                    return {
                        ...m,
                        price: m.price,
                    };
                }),
            } as CreateOrderItem,
        },
        {
            channelId: channelId,
            item: item,
            patch: {
                menuItemId: idToFind,
                quantity: -1 * quantity,
                discount: item.discountPercentage,
                extras: extras,
                price: item.originalPrice,
            } as CreateOrderItem,
        }];

        return {
            session: s.session,
            operationsToSync: operationsToSync,
            pendingOrderIds: s.pendingOrderIds,
        }
    })

    const transferSession = (toChannelId: string, transferItems?: Map<SessionItem, number>) => setState(s => {
        if(channelId == undefined) {
            return s;
        }

        const operationsToSync = [...s.operationsToSync];
        const currentItems = getItems(s.session.items, s.operationsToSync);
        for(const item of currentItems.map(c => c as SessionItem)) {
            const quantityToTransfer = transferItems == undefined ? item.quantity : (transferItems.get(item) ?? 0);
            if(quantityToTransfer == 0) {
                continue;
            }

            const extras = item.extras.map(extra => ({
                modifierGroupId: extra.modifierGroupId,
                menuItemId: extra.menuItemId,
                quantity: extra.quantity,
                price: extra.originalPrice,
            }));
            operationsToSync.push({
                channelId: toChannelId,
                item: item,
                patch: {
                    menuItemId: item.menuItemId,
                    discount: item.discountPercentage,
                    quantity: quantityToTransfer,
                    extras: extras,
                    price: item.originalPrice,
                } as CreateOrderItem,
            })
            operationsToSync.push({
                channelId: channelId,
                item: item,
                patch: {
                    menuItemId: item.menuItemId,
                    discount: item.discountPercentage,
                    quantity: -quantityToTransfer,
                    extras: extras,
                    price: item.originalPrice,
                },
            })
        }

        return {
            session: s.session,
            operationsToSync: operationsToSync,
            pendingOrderIds: s.pendingOrderIds,
        }
    });

    const forceSync = async () => {
        if (outOfSyncTimeout == undefined) {
            return;
        }

        await outOfSyncTimeout();
        setOutOfSyncTimeout(undefined);
    }

    useEffect(() => setState(s => {
        let hasChanges = false;

        let pendingOrderIds = s.pendingOrderIds;
        const assignedOrders = getAssignedOrdersSet(pendingOrdersQuery.data);
        if(assignedOrders.size > 0 && s.pendingOrderIds.length > 0) {
            const pendingOrderIdsSet = new Set(s.pendingOrderIds);
            for(const orderId of assignedOrders) {
                hasChanges ||= pendingOrderIdsSet.delete(orderId);
            }

            if(hasChanges) {
                pendingOrderIds = Array.from(pendingOrderIdsSet);
            }
        }

        if(hasChanges == false) {
            return s;
        }

        return {
            session: s.session,
            operationsToSync: s.operationsToSync,
            pendingOrderIds: pendingOrderIds,
        }
    }), [pendingOrdersQuery.data])

    useEffect(() => {
        const pendingOperations = state.operationsToSync.filter(o => o.processingOperation == undefined)
        if(pendingOperations.length == 0) {
            setOutOfSyncTimeout(undefined);
            return;
        }

        setOutOfSyncTimeout(() => async () => {
            try {
                const map = new Map<string, CreateOrderItem[]>();
                for(const item of pendingOperations) {
                    let aux = map.get(item.channelId);
                    if(aux == undefined) {
                        aux = [];
                        map.set(item.channelId, aux);
                    }
                    aux.push(item.patch);
                }
                
                const createOrders = [] as CreateOrder[];
                for(const [key, items] of map) {
                    createOrders.push({
                        channelId: key,
                        items: items,
                    })
                }

                const response = await ordersApi.post({
                    orders: createOrders,
                })
                const jobId = response.jobId;
                setState(s => {
                    const operationsToSync: ItemToSync[] = [];
                    let pendingOrderIds = s.pendingOrderIds;

                    let otherChannelOrderIds = [] as string[];
                    for(const item of s.operationsToSync) {
                        if(pendingOperations.find(i => i == item) != undefined) {
                            if(jobId == undefined) {
                                continue;
                            }

                            let thisChannelOrderIds = [] as string[];
                            for(const o of response.data) {
                                if(o.channelId == s.session.channelId) {
                                    thisChannelOrderIds.push(o.id);
                                    continue;
                                }
                                otherChannelOrderIds.push(o.id);
                            }
                            
                            operationsToSync.push({
                                ...item,
                                processingOperation: {
                                    orderIds: thisChannelOrderIds,
                                },
                            });
                            continue;
                        }
                        operationsToSync.push(item);
                    }

                    if(otherChannelOrderIds.length > 0) {
                        pendingOrderIds = [...pendingOrderIds, ...otherChannelOrderIds];
                    }

                    return {
                        session: s.session,
                        operationsToSync: operationsToSync,
                        pendingOrderIds: pendingOrderIds,
                    };
                });
            } catch (e) {
                setState(s => {
                    const result: ItemToSync[] = [];
                    for(const item of s.operationsToSync) {
                        if(pendingOperations.find(i => i == item) != undefined) {
                            continue;
                        }
                        result.push(item);
                    }
                    return {
                        session: s.session,
                        operationsToSync: result,
                        pendingOrderIds: s.pendingOrderIds,
                    };
                });
                toast.error(t("employeeAccessDenied"))
            }
        });
    }, [state.operationsToSync, ordersApi])
    
    useEffect(() => updateSession(currentSession), [currentSession])

    useEffect(() => {
        if(outOfSyncTimeout == undefined) {
            return;
        }

        const timeout = setTimeout(() => {
            outOfSyncTimeout();
            setOutOfSyncTimeout(undefined);
        }, 1000);
        return () => clearTimeout(timeout);
    }, [outOfSyncTimeout])

    const items = useMemo(() => getItems(state.session.items, state.operationsToSync), [state.session.items, state.operationsToSync]);

    const result = useMemo(() => ({
        channelId: channelId ?? "",
        items: items,
        isSyncing: sessionsQuery.isFirstLoading || state.operationsToSync.length > 0 || state.pendingOrderIds.length > 0,
        addItem: addItem,
        removeItem: removeItem,
        applyDiscount: applyDiscount,
        transferSession: transferSession,
        forceSync: forceSync,
        sessionId: state.session.id,
    }), [channelId, sessionsQuery.isFirstLoading, state.operationsToSync, state.pendingOrderIds, state.session.id, items, outOfSyncTimeout])

    return result;
}

const getAssignedOrdersSet = (data: Order[]) => {
    const set = new Set<string>();
    for(const order of data) {
        if(order.sessionId != undefined) {
            set.add(order.id);
        }
    }
    return set;
}

const getClosedSession = (channelId: string | undefined, closeDate: string): Session => ({
    id: "",
    items: [],
    isOpen: false,
    startDate: closeDate,
    closedDate: closeDate,
    channelId: channelId ?? "",
    isDeleted: false,
    orderIds: [],
})

const getItems = (items: SessionItem[], itemsToSync: ItemToSync[]): SessionItem[] => {
    const result: SessionItem[] = [...items];
    const syncArray = [...itemsToSync];
    for(let itemToSync = syncArray.shift(); itemToSync != undefined; itemToSync = syncArray.shift()) {
        let isUnmatched = true;

        for(let i = 0; i < result.length; ++i) {
            const sessionItem = result[i];
            if(sessionItem.isPaid) {
                continue;
            }

            if(!isSameItem(sessionItem, itemToSync.item, itemToSync.patch.discount)) {
                continue;
            }

            isUnmatched = false;
            let newSessionItem: SessionItem = {
                ...sessionItem,
                quantity: sessionItem.quantity + itemToSync.patch.quantity,
            }

            result[i] = newSessionItem;
        }

        if(isUnmatched == false) {
            continue;
        }
        
        const item = itemToSync.item;
        const isMenuItem = 'menuItemId' in item == false;
        const idToFind = isMenuItem ? item.id : item.menuItemId;
        const discountToApply = itemToSync.patch.discount ?? (!isMenuItem ? item.discountPercentage : 0);

        const extras: SessionExtraItem[] = [];
        if(isMenuItem && 'selectedOptions' in item) {
            for(const [modifierGroupId, selection] of item.selectedOptions) {
                for(const extra of selection) {
                    extras.push({
                        modifierGroupId: modifierGroupId,
                        menuItemId: extra.menuItemId,
                        price: extra.price,
                        originalPrice: extra.price,
                        quantity: 1,
                    })
                }
            }
        } else if(!isMenuItem) {
            for(const modifier of item.extras) {
                extras.push({
                    modifierGroupId: modifier.modifierGroupId,
                    menuItemId: modifier.menuItemId,
                    quantity: modifier.quantity,
                    price: modifier.price,
                    originalPrice: modifier.originalPrice,
                })
            }
        }

        result.unshift({
            id: [`${idToFind}-${false}-${item.price ?? 0}`, ...(extras.map(m => `${m.menuItemId}-${m.quantity}-${m.price}-${m.originalPrice}`) ?? [])].join('-'),
            menuItemId: idToFind,
            isPaid: false,
            lastModified: "",
            extras: extras,
            price: item.price ?? 0,
            originalPrice: item.price ?? 0,
            quantity: itemToSync.patch.quantity,
            discountPercentage: discountToApply,
        });
    }
    return result.filter(p => p.quantity > 0);
}

const isSameItem = (existingItem: SessionItem, item: SessionItem | MenuItem | MenuItemWithExtras, discount?: number) => {
    const isMenuItem = 'menuItemId' in item == false;
    const idToFind = isMenuItem ? item.id : item.menuItemId;
    const discountToApply = discount ?? (!isMenuItem ? item.discountPercentage : 0);
    const originalPrice = isMenuItem ? item.price : item.originalPrice;

    const getKey = (digitalMenuItemId: string, discountPercentage: number, price: number) => `${digitalMenuItemId}-${discountPercentage}-${price}`;
    const baseIsSame = getKey(existingItem.menuItemId, existingItem.discountPercentage, existingItem.originalPrice) == 
                        getKey(idToFind, discountToApply, originalPrice);

    if(baseIsSame == false) {
        return false;
    }
    
    const itemsQuantityMap = existingItem.extras.reduce((maps, e) => {
        maps.set(getKey(e.menuItemId, existingItem.discountPercentage, e.originalPrice), e.quantity);
        return maps;
    }, new Map<string, number>()) ?? new Map<string, number>();

    if(isMenuItem && 'selectedOptions' in item) {
        for(const [, selection] of item.selectedOptions) {
            for(const extra of selection) {
                const key = getKey(extra.menuItemId, discountToApply, extra.price)

                const foundQuantity = itemsQuantityMap.get(key);
                if(foundQuantity == undefined) {
                    return false;
                }

                if(foundQuantity > 1) {
                    itemsQuantityMap.set(key, foundQuantity - 1);
                } else {
                    itemsQuantityMap.delete(key);
                }
            }
        }
    } else if('extras' in item) {
        for(const extra of item.extras) {
            const key = getKey(extra.menuItemId, item.discountPercentage, extra.originalPrice)

            const foundQuantity = itemsQuantityMap.get(key);
            if(foundQuantity == undefined) {
                return false;
            }

            if(foundQuantity - extra.quantity > 0) {
                itemsQuantityMap.set(key, foundQuantity - extra.quantity);
            } else {
                itemsQuantityMap.delete(key);
            }
        }
    }
    
    return itemsQuantityMap.size == 0;
}