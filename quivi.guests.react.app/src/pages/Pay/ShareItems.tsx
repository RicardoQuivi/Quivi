import { useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import BigNumber from "bignumber.js";
import type { SessionItem } from "../../hooks/api/Dtos/sessions/SessionItem";
import { Formatter } from "../../helpers/formatter";
import { useChannelContext } from "../../context/AppContextProvider";
import { useSessionsQuery } from "../../hooks/queries/implementations/useSessionsQuery";
import { useBrowserStorageService } from "../../hooks/useBrowserStorageService";
import { Calculations } from "../../helpers/calculations";
import { ItemsHelper } from "../../helpers/ItemsHelper";
import { QuantitySelector } from "../../components/Quantity/QuantitySelector";
import { Box, Skeleton } from "@mui/material";
import { useMenuItemsQuery } from "../../hooks/queries/implementations/useMenuItemsQuery";
import type { MenuItem } from "../../hooks/api/Dtos/menuItems/MenuItem";

interface ItemToPay {
    readonly sessionItem: SessionItem;
    inputIds: string[];
    pendingQuantity: number;
    splittedQuantity: number;
    isPaid: boolean;
}

interface Props {
    readonly onChangeAmount: (userBillAmount: number) => void;
    readonly onChangeItems: (selectedItems: SessionItem[]) => void;
    readonly sessionPending: number | null;
}

export const ShareItems: React.FC<Props> = ({ 
    onChangeAmount, 
    onChangeItems,
    sessionPending,
}) => {
    const { t } = useTranslation();

    const browserStorageService = useBrowserStorageService();
    const channelContext = useChannelContext();
    const sessionQuery = useSessionsQuery({
        channelId: channelContext.channelId,
    });
    
    const [isInitialized, setIsInitialized] = useState(false);
    const [pendingItems, setPendingItems] = useState<ItemToPay[]>([]);
    const [splittedBill, setSplittedBill] = useState<number>(0);
    
    let [paymentDivision, setPaymentDivision] = useState(() => browserStorageService.getPaymentDivision() ?? {
        divideEvenly: {
            payForPeople: 1,
            peopleAtTheTable: 1,
        },
        selectedItems: [],
    })

    const itemsIds = useMemo(() => {
        const result = new Set<string>();
        for(const item of sessionQuery.data?.items ?? []) {
            result.add(item.menuItemId);
            for(const extra of item.extras) {
                result.add(extra.menuItemId);
            }
        }
        return Array.from(result);
    }, [sessionQuery.data])

    const menuItemsQuery = useMenuItemsQuery(itemsIds.length == 0 ? undefined : {
        channelId: channelContext.channelId,
        ids: itemsIds,
        ignoreCalendarAvailability: true,
        page: 0,
    })
    const menuItemsMap = useMemo(() => {
        const result = new Map<string, MenuItem>();
        for(const item of menuItemsQuery.data) {
            result.set(item.id, item);
        }
        return result;
    }, [menuItemsQuery.data])

    const isOverPaying = () => Formatter.floatify(splittedBill) > (sessionPending ?? 0);

    const updateSelectedItemsStored = (items: ItemToPay[]) => {
        let storeItems: string[] = [...paymentDivision.selectedItems];
        items.forEach(item => {
            storeItems = storeItems.filter(s => !item.inputIds.some(inputId => inputId == s));
            if (item.splittedQuantity > 0) {
                for (let index = 0; index < item.splittedQuantity; index++) {
                    storeItems.push(item.inputIds[index]);
                }
            }
        });
        setPaymentDivision(p => ({
            ...p,
            selectedItems: storeItems,
        }))
    }

    const handleChange = (itemId: string, qty: number) => {
        const item = pendingItems.find(x => x.sessionItem.id == itemId);
        if (item) {
            item.splittedQuantity = qty;
        }
        setPendingItems([...pendingItems]);
        updateSelectedItemsStored(pendingItems);
    }

    useEffect(() => browserStorageService.savePaymentDivision(paymentDivision), [paymentDivision])
    useEffect(() => {
        if (isInitialized) {
            return;
        }

        if(sessionQuery.isFirstLoading) {
            return;
        }

        if(sessionQuery.data == undefined) {
            return;
        }

        if(pendingItems.length > 0) {
            return;
        }

        const items: ItemToPay[] = [];
        sessionQuery.data.items.forEach(sessionItem => {
            let item = items.find(x => x.sessionItem === sessionItem);
            if (item) {
                item.isPaid &&= sessionItem.isPaid;
                item.pendingQuantity += sessionItem.quantity;
            }
            else {
                item = {
                    sessionItem: sessionItem,
                    inputIds: [],
                    isPaid: sessionItem.isPaid,
                    pendingQuantity: sessionItem.quantity,
                    splittedQuantity: 0,
                };
                items.push(item);
            }

            item.inputIds = [];
            for (let index = 0; index < item.pendingQuantity; ++index) {
                item.inputIds.push(`${item.sessionItem}-${index}`);
            }
        });
        setPendingItems(items);
        setIsInitialized(true);
    }, [sessionQuery.data, sessionQuery.isFirstLoading]);

    useEffect(() => {
        if (!isInitialized) {
            return;
        }

        if (!paymentDivision.selectedItems.length) {
            return;
        }

        const totalSelected = pendingItems.reduce((result, item) => result + item.splittedQuantity, 0);
        if (totalSelected > 0) {
            return;
        }

        paymentDivision.selectedItems.forEach(s => {
            const item = pendingItems.find(x => x.inputIds.some(inputId => inputId == s));
            if (item) {
                item.splittedQuantity++;
            }
        });
        setPendingItems([...pendingItems]);
    }, [paymentDivision.selectedItems, isInitialized]);

    useEffect(() => {
        if (!isInitialized)
            return;

        onChangeItems(pendingItems
            .filter(item => item.splittedQuantity > 0)
            .map(item => ({
                ...item.sessionItem,
                quantity: item.splittedQuantity,
            })));
    }, [pendingItems]);

    useEffect(() => {
        if (!isInitialized)
            return;

        onChangeAmount(Formatter.floatify(splittedBill));
    }, [splittedBill]);

    useEffect(() => {
        const bill = ItemsHelper.getItemsPriceGeneric(pendingItems, item => getPrice(item.sessionItem), item => item.splittedQuantity);
        setSplittedBill(bill);
    }, [pendingItems]);

    const getPrice = (item: SessionItem) => {
        const modifiersTotal = item.extras.reduce((r, m) => r.plus(BigNumber(m.price).multipliedBy(m.quantity)), BigNumber(0)) ?? BigNumber(0);
        return BigNumber(item.price).plus(modifiersTotal).toNumber();
    }

    //#endregion

    return (
        <div className={"mb-10"}>
            <h2 className="mb-4">{t("pay.shareItemsTitle")}</h2>
            {
                pendingItems.filter(item => !item.isPaid).map(item => (
                    <SelectableTableItem
                        name={menuItemsMap.get(item.sessionItem.menuItemId)?.name}
                        price={getPrice(item.sessionItem)}
                        maxQuantity={item.pendingQuantity}
                        quantity={item.splittedQuantity}
                        isPaid={item.isPaid}
                        onChange={qty => handleChange(item.sessionItem.id, qty)}
                        key={item.sessionItem.id}
                    />
                ))
            }

            <p className="small ta-r mt-7">
                {t("pay.billAmount")}&nbsp;
                <span className="semi-bold">{Formatter.price(sessionPending ?? 0, "€")}</span>
            </p>

            {
                isOverPaying() &&
                <div className="alert alert--error mt-4">
                    <p>{t("pay.overPayAlert")}</p>
                </div>
            }

            <div className={"total-container mt-2"}>
                <h2 className="mb-1">{t("pay.itemsTotal")}</h2>
                <h1>{Formatter.price(splittedBill, "€")}</h1>
            </div>
        </div>
    )
}

interface SelectableTableItemProps {
    readonly name?: string,
    readonly price: number,
    readonly maxQuantity: number,
    readonly quantity: number,
    readonly isPaid: boolean,
    readonly onChange: (addedQty: number) => void,
}

const SelectableTableItem = ({ name, price, maxQuantity, quantity, isPaid, onChange }: SelectableTableItemProps) => {
    const { i18n } = useTranslation();

    const onIncrement = () => {
        if (quantity === maxQuantity) {
            return;
        }

        const remainingQty = maxQuantity - quantity; 
        const newQty = quantity + Math.min(1, remainingQty);
        onChange(newQty);
    }

    const onDecrement = () => {
        if (quantity === 0) {
            return;
        }

        const newQty = quantity - Math.min(1, quantity);
        onChange(newQty);
    }

    return (
        <Box
            sx={{
                display: "flex",
                gap: "10px",
                alignItems: "center",
            }} 
            className={`checkbox-tableitem ${isPaid ? "is-paid" : ""}`}
        >
            <span style={{width: "4rem"}}>({Formatter.number(Formatter.floatify(maxQuantity), i18n.language)} x)</span>
            <Box
                sx={{
                    display: "flex", 
                    flexDirection: "column", 
                    alignItems: "flex-start",
                }}
            >
                {
                    name == undefined
                    ?
                    <Skeleton animation="wave" />
                    :
                    <span>{name}</span>
                }
                <span style={{fontSize: "0.8rem"}}>{Formatter.price(Calculations.roundUp(price), "€")}</span>
            </Box>
            <Box sx={{flex: "1 1 auto"}}></Box>
            {
                !isPaid &&
                <Box>
                    <QuantitySelector
                        quantity={Formatter.floatify(quantity)} 
                        onDecrement={onDecrement} 
                        onIncrement={onIncrement}
                        shouldCollapse={false} 
                        alwaysOpened={true}
                        incrementDisabled={maxQuantity == quantity}
                        decrementDisabled={quantity == 0} />
                </Box>
            }
        </Box>
    );
}