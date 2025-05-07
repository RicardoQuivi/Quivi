import { useEffect, useMemo, useState } from "react";
import { ICartSession } from "./ICartSession";
import { BaseSessionItem, SessionItem } from "../../api/Dtos/sessions/SessionItem";
import { MenuItem } from "../../api/Dtos/menuitems/MenuItem";
import { BaseCreateOrderItem, CreateOrder, CreateOrderItem } from "../../api/Dtos/orders/CreateOrdersRequest";
import { Session } from "../../api/Dtos/sessions/Session";
import { useLoggedEmployee } from "../../../context/pos/LoggedEmployeeContextProvider";
import { useOrdersApi } from "../../api/useOrdersApi";
import { useTranslation } from "react-i18next";
import { useToast } from "../../../context/ToastProvider";
import { useBackgroundJobsQuery } from "../../queries/implementations/useBackgroundJobsQuery";
import { JobState } from "../../api/Dtos/backgroundjobs/JobState";
import { useSessionsQuery } from "../../queries/implementations/useSessionsQuery";

interface ItemToSync {
    readonly channelId: string;
    readonly item: MenuItem | SessionItem;
    readonly patch: CreateOrderItem;
    jobId?: string;
    synced: boolean;
}

export const useCartSession = (channelId: string | undefined): ICartSession => {
    const { t } = useTranslation();
    const employeeContext = useLoggedEmployee();
    const ordersApi = useOrdersApi(employeeContext.token);
    const toast = useToast();
  
    const sessionsQuery = useSessionsQuery(!channelId ? undefined : {
        channelIds: [channelId],
        page: 0,
        isOpen: true,
    });

    const [currentSession, setCurrentSession] = useState<Session>(() => getClosedSession(channelId, new Date().toISOString()));
    const [operationsToSync, setOperationsToSync] = useState<ItemToSync[]>(() => [])
    const [outOfSyncTimeout, setOutOfSyncTimeout] = useState<() => Promise<any>>();
    const [pendingJobIds, setPendingJobIds] = useState<string[]>(() => []);

    const jobQuery = useBackgroundJobsQuery(pendingJobIds.length == 0 ? undefined : pendingJobIds);
    
    const updateSession = async (channelId: string | undefined, data: Session | null) => {
        if (data == null) {
            setCurrentSession(() => getClosedSession(channelId, new Date().toISOString()))
        } else if (data.isOpen == false) {
            setCurrentSession({
                ...data,
                items: [],
            });
        } else {
            const items = data.items.map(item => ({
                ...item,
            }));
            items.reverse();

            setCurrentSession({
                ...data,
                items: items,
            });
        }

        if(operationsToSync.length > 0){
            setOperationsToSync(p => p.filter(o => o.synced == false && o.jobId == undefined));
        }
    }

    const addItem = (item: MenuItem | SessionItem, quantity: number) => setOperationsToSync(p => {
        if(channelId == undefined) {
            return p;
        }

        const isDigitalMenuItem = 'menuItemId' in item == false;
        const idToFind = isDigitalMenuItem ? item.id : item.menuItemId;
        const discountToApply = !isDigitalMenuItem ? item.discountPercentage : 0;
        const price = isDigitalMenuItem ? item.price : item.originalPrice;
        
        let extras: BaseCreateOrderItem[] | undefined = undefined;
        // if(isDigitalMenuItem && 'extras' in item) {
        //     extras = [];
        //     for(const [, selection] of item.extras) {
        //         selection.map(s => ({
        //             itemId: s.menuItem.id,
        //             quantity: 1,
        //             price: s.price,
        //         })).forEach(s => extras?.push(s));
        //     }
        // } else if('extras' in item && item.extras != undefined && item.extras.length > 0) {
        //     extras = [];
        //     for(const extra of item.extras) {
        //         extras.push({
        //             menuItemId: extra.menuItemId,
        //             quantity: extra.quantity,
        //             price: extra.originalPrice,
        //         })
        //     }
        // }

        return [...p, {
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
    })

    const removeItem = (item: SessionItem, quantity: number, discount?: number) => setOperationsToSync(p => {
        if(channelId == undefined) {
            return p;
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
        return [...p, {
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
    })

    const applyDiscount = (item: SessionItem, quantity: number, discount: number, priceOverride?: number) => setOperationsToSync(p => {
        if(channelId == undefined) {
            return p;
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
        return [...p, {
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
        }]
    })

    const transferSession = (toChannelId: string, transferItems?: Map<SessionItem, number>) => setOperationsToSync(p => {
        if(channelId == undefined) {
            return p;
        }

        const result = [...p];
        const currentItems = getItems(currentSession.items, p);
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
            result.push({
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
            result.push({
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
        return result;
    });

    const forceSync = async () => {
        if (!outOfSyncTimeout)
            return;
        
        await outOfSyncTimeout();
        setOutOfSyncTimeout(undefined);
    }

    useEffect(() => {
        const pendingOperations = operationsToSync.filter(o => o.jobId == undefined)
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
                setOperationsToSync(p => {
                    const result: ItemToSync[] = [];
                    for(const item of p) {
                        if(pendingOperations.find(i => i == item) != undefined) {
                            if(jobId == undefined) {
                                continue;
                            }
                            
                            item.jobId = jobId;
                        }
                        result.push(item);
                    }
                    return result;
                });
                if(jobId != undefined) {
                    setPendingJobIds(p => [...p, jobId]);
                }                
            } catch (e) {
                setOperationsToSync(p => {
                    const result: ItemToSync[] = [];
                    for(const item of p) {
                        if(pendingOperations.find(i => i == item) != undefined) {
                            continue;
                        }
                        result.push(item);
                    }
                    return result;
                });
                toast.error(t("employeeAccessDenied"))
            }
        });
    }, [operationsToSync, ordersApi])

    useEffect(() => {
        const completedJobs = jobQuery.data.filter(j => j != undefined && j.state == JobState.Completed);
        if(completedJobs.length > 0) {
            setOperationsToSync(p => {
                const result: ItemToSync[] = [];
                for(const item of p) {    
                    if(item.jobId != undefined) {
                        const job = completedJobs.find(j => j.id == item.jobId);
                        if(job != undefined) {
                            item.synced = true;
                        }
                    }
                    result.push(item);
                }
                return result;
            });
        }

        if(jobQuery.isLoading || pendingJobIds.length == 0) {
            return;
        }

        setPendingJobIds(p => {
            for(const job of jobQuery.data) {
                if(job.state == JobState.Completed) {
                    p = p.filter(j => j != job.id);
                }
            }
            return p;
        })
    }, [jobQuery.data, jobQuery.isLoading])
    
    useEffect(() => {
        if(sessionsQuery.isFirstLoading == true) {
            return;
        }

        updateSession(channelId, sessionsQuery.data?.length > 0 ? sessionsQuery.data[0] : null);
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

    const state = useMemo(() => ({
        channelId: channelId ?? "",
        items: getItems(currentSession.items, operationsToSync),
        isSyncing: sessionsQuery.isFirstLoading || operationsToSync.filter(o => o.synced == false).length > 0 || pendingJobIds.length > 0,
        addItem: addItem,
        removeItem: removeItem,
        applyDiscount: applyDiscount,
        transferSession: transferSession,
        forceSync: forceSync,
    }), [channelId, currentSession, operationsToSync, pendingJobIds])

    return state;
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

const isSamePatch = (patch1: BaseSessionItem | SessionItem, patch2: BaseSessionItem | SessionItem) => {
    if(patch1.menuItemId != patch2.menuItemId) {
        return false;
    }

    if(patch1.price != patch2.price) {
        return false;
    }

    const patch1Discount = 'discountPercentage' in patch1 ? patch1.discountPercentage : null;
    const patch2Discount = 'discountPercentage' in patch2 ? patch2.discountPercentage : null;
    if(patch1Discount != patch2Discount) {
        return false;
    }

    const patch1Extras = 'extras' in patch1 ? patch1.extras : undefined;
    const patch2Extras = 'extras' in patch2 ? patch2.extras : undefined;
    if(patch1Extras != undefined && patch2Extras != undefined) {
        let patch1ExtrasMap = patch1Extras.reduce((maps, e) => {
            maps.set(e.menuItemId, e);
            return maps;
        }, new Map<string, BaseSessionItem>());

        for(const e2 of patch2Extras) {
            const e1 = patch1ExtrasMap.get(e2.menuItemId);
            if(e1 == undefined) {
                return false;
            }

            if(isSamePatch(e2, e1) == false) {
                return false;
            }
            patch1ExtrasMap.delete(e2.menuItemId);
        }

        if(patch1ExtrasMap.size != 0) {
            return false;
        }
    }
    return true;
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