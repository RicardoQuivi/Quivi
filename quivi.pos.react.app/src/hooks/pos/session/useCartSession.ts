import { useEffect, useMemo, useState } from "react";
import { ICartSession, MenuItemWithExtras } from "./ICartSession";
import { BaseSessionItem, SessionItem } from "../../api/Dtos/sessions/SessionItem";
import { MenuItem } from "../../api/Dtos/menuitems/MenuItem";
import { BaseCreateOrderItem, CreateOrder, CreateOrderItem } from "../../api/Dtos/orders/CreateOrdersRequest";
import { Session } from "../../api/Dtos/sessions/Session";
import { useOrdersApi } from "../../api/useOrdersApi";
import { useTranslation } from "react-i18next";
import { useToast } from "../../../context/ToastProvider";
import { useSessionsQuery } from "../../queries/implementations/useSessionsQuery";
import BigNumber from "bignumber.js";
import { useBackgroundJobsQuery } from "../../queries/implementations/useBackgroundJobsQuery";
import { JobState } from "../../api/Dtos/backgroundjobs/JobState";
import { BackgroundJob } from "../../api/Dtos/backgroundjobs/BackgroundJob";

interface ItemToSync {
    readonly channelId: string;
    readonly item: MenuItem | SessionItem;
    readonly patch: CreateOrderItem;
    processingOperation?: {
        jobId: string;
    }
    synced: boolean;
}
export const useCartSession = (channelId: string | undefined): ICartSession => {
    const { t } = useTranslation();
    const ordersApi = useOrdersApi();
    const toast = useToast();
  
    const sessionsQuery = useSessionsQuery(!channelId ? undefined : {
        channelIds: [channelId],
        page: 0,
        isOpen: true,
    });

    const [state, setState] = useState(() => ({
        session: getClosedSession(channelId, new Date().toISOString()),
        operationsToSync: [] as ItemToSync[],
        pendingJobIds: [] as string[],
    }))
    const [outOfSyncTimeout, setOutOfSyncTimeout] = useState<() => Promise<any>>();

    const jobQuery = useBackgroundJobsQuery(state.pendingJobIds.length == 0 ? undefined : state.pendingJobIds);

    const updateSession = (channelId: string | undefined, data: Session | null) => setState(s => {
        let session = s.session;
        let operationsToSync = s.operationsToSync;

        if (data == null) {
            session = getClosedSession(channelId, new Date().toISOString());
        } else if (data.isOpen == false) {
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
            const aux = operationsToSync.filter(o => o.synced == false);

            if(operationsToSync.length != aux.length) {
                operationsToSync = aux;
            }
        }

        return {
            session: session,
            operationsToSync: operationsToSync,
            pendingJobIds: s.pendingJobIds,
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
        
        let extras: BaseCreateOrderItem[] | undefined = undefined;
        if(isMenuItem && 'selectedOptions' in item) {
            extras = [];
            for(const [, selections] of item.selectedOptions) {
                for(const selection of selections) {
                    extras.push({
                        menuItemId: selection.menuItemId,
                        price: selection.price,
                        quantity: 1,
                    })
                }
            }
        } else if('extras' in item && item.extras.length > 0) {
            extras = [];
            for(const extra of item.extras) {
                extras.push({
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
            pendingJobIds: s.pendingJobIds,
            operationsToSync: operationsToSync,
        }
    })

    const removeItem = (item: SessionItem, quantity: number, discount?: number) => setState(s => {
        if(channelId == undefined) {
            return s;
        }

        const discountApplied = discount ?? 0;
        const idToFind = item.menuItemId;

        const extras = [] as BaseSessionItem[];
        for(const extra of item.extras) {
            extras.push({
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
            pendingJobIds: s.pendingJobIds,
            operationsToSync: operationsToSync,
        }
    })

    const applyDiscount = (item: SessionItem, quantity: number, discount: number, priceOverride?: number) => setState(s => {
        if(channelId == undefined) {
            return s;
        }

        const idToFind = item.menuItemId;
        const extras = [] as BaseSessionItem[];
        for(const extra of item.extras) {
            extras.push({
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
            synced: false,
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
            synced: false,
        }];

        return {
            session: s.session,
            pendingJobIds: s.pendingJobIds,
            operationsToSync: operationsToSync,
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
                synced: false,
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
                synced: false,
            })
        }

        return {
            session: s.session,
            pendingJobIds: s.pendingJobIds,
            operationsToSync: operationsToSync,
        }
    });

    const forceSync = async () => {
        if (!outOfSyncTimeout) {
            return;
        }

        await outOfSyncTimeout();
        setOutOfSyncTimeout(undefined);
    }

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
                
                const jobId = response.data;
                setState(s => {
                    const operationsToSync: ItemToSync[] = [];
                    for(const item of s.operationsToSync) {
                        if(pendingOperations.find(i => i == item) != undefined) {
                            if(jobId == undefined) {
                                continue;
                            }
                            
                            item.processingOperation = {
                                jobId: jobId,
                            };
                        }
                        operationsToSync.push(item);
                    }

                    let pendingJobIds = jobId != undefined ? [...s.pendingJobIds, jobId] : s.pendingJobIds;

                    return {
                        session: s.session,
                        operationsToSync: operationsToSync,
                        pendingJobIds: pendingJobIds,
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
                        pendingJobIds: s.pendingJobIds,
                        operationsToSync: result,
                    };
                });
                toast.error(t("employeeAccessDenied"))
            }
        });
    }, [state.operationsToSync, ordersApi])

    useEffect(() => setState(s => {
        let hasChanges = false;

        let operationsToSync = s.operationsToSync;
        let pendingJobIds = s.pendingJobIds;

        const completedJobs = getCompletedJobsSet(jobQuery.data);
        if(completedJobs.size > 0) {
            operationsToSync = [];
            for(const item of s.operationsToSync) {
                if(item.processingOperation != undefined) {
                    const jobId = item.processingOperation.jobId;
                    const job = completedJobs.has(jobId);
                    if(job != undefined && item.synced == false) {
                        item.synced = true;
                        hasChanges = true;
                    }
                }
                operationsToSync.push(item);
            }

            if(s.pendingJobIds.length > 0) {
                for(const jobId of completedJobs) {
                    pendingJobIds = pendingJobIds.filter(j => j != jobId);
                }
                hasChanges = true;
            }
        }

        if(hasChanges == false) {
            return s;
        }

        return {
            session: s.session,
            operationsToSync: operationsToSync,
            pendingJobIds: pendingJobIds,
        }
    }), [jobQuery.data])
    
    useEffect(() => {
        updateSession(channelId, sessionsQuery.data.length > 0 ? sessionsQuery.data[0] : null);
        // const timeout = setTimeout(() => updateSession(channelId, sessionsQuery.data.length > 0 ? sessionsQuery.data[0] : null), 1000);
        // return () => clearTimeout(timeout);
    }, [channelId, sessionsQuery.data])

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
        isSyncing: sessionsQuery.isFirstLoading || state.operationsToSync.filter(o => o.synced == false).length > 0 || state.pendingJobIds.length > 0,
        addItem: addItem,
        removeItem: removeItem,
        applyDiscount: applyDiscount,
        transferSession: transferSession,
        forceSync: forceSync,
        sessionId: state.session.id,
    }), [channelId, sessionsQuery.isFirstLoading, state.operationsToSync, state.pendingJobIds, state.session.id, items])

    return result;
}

const getCompletedJobsSet = (data: BackgroundJob[]) => {
    const set = new Set<string>();
    for(const job of data) {
        if(job.state == JobState.Completed) {
            set.add(job.id);
        }
    }
    return set;
}

const getClosedSession = (channelId: string | undefined, closeDate: string): Session => ({
    id: "",
    items: [],
    isOpen: false,
    closedDate: closeDate,
    channelId: channelId ?? "",
    isDeleted: false,
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
        //const hasDigitalExtras = isMenuItem && 'extras' in item;
        //const hasSessionExtras = !isMenuItem && item.extras.length > 0;

        const extras: BaseSessionItem[] = [];
        // if(hasDigitalExtras) {
        //     for(const [, selection] of item.extras) {
        //         for(const extra of selection) {
        //             extras.push({
        //                 id: `${extra.menuItem.id}-${1}-${extra.price}-${discountToApply}`,
        //                 menuItemId: extra.menuItem.id,
        //                 isPaid: false,
        //                 lastModified: "",
        //                 price: extra.price,
        //                 originalPrice: extra.price,
        //                 quantity: 1,
        //                 discountPercentage: discountToApply,
        //             })
        //         }
        //     }
        // } else if(hasSessionExtras) {
        //     for(const modifier of item.extras ?? []) {
        //         extras.push({
        //             id: `${modifier.menuItemId}-${1}-${modifier.price}-${discountToApply}`,
        //             menuItemId: `${modifier.menuItemId}-${1}-${modifier.price}-${discountToApply}`,
        //             isPaid: false,
        //             price: modifier.price,
        //             originalPrice: modifier.price,
        //             quantity: modifier.quantity,
        //             discountPercentage: discountToApply,
        //             lastModified: "",
        //         })
        //     }
        // }

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

const isSameItem = (existingItem: SessionItem, item: SessionItem | MenuItem, discount?: number) => {
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

    // if(isMenuItem && 'extras' in item && item.extras.size > 0) {
    //     for(const [, selection] of item.extras) {
    //         for(const extra of selection) {
    //             const extraPrice = extra.price ?? extra.menuItem.price ?? 0;
    //             const key = getKey(extra.menuItem.id, discountToApply, extraPrice)

    //             const foundQuantity = itemsQuantityMap.get(key);
    //             if(foundQuantity == undefined) {
    //                 return false;
    //             }

    //             if(foundQuantity > 1) {
    //                 itemsQuantityMap.set(key, foundQuantity - 1);
    //             } else {
    //                 itemsQuantityMap.delete(key);
    //             }
    //         }
    //     }
    // } else if('extras' in item && item.extras != undefined && item.extras.length > 0) {
    //     for(const extra of item.extras) {
    //         const key = getKey(extra.menuItemId, extra.discountPercentage, extra.originalPrice)

    //         const foundQuantity = itemsQuantityMap.get(key);
    //         if(foundQuantity == undefined) {
    //             return false;
    //         }

    //         if(foundQuantity - extra.quantity > 0) {
    //             itemsQuantityMap.set(key, foundQuantity - extra.quantity);
    //         } else {
    //             itemsQuantityMap.delete(key);
    //         }
    //     }
    // }
    
    return itemsQuantityMap.size == 0;
}