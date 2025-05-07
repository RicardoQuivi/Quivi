import { useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Chip, Grid, InputAdornment, Paper, Skeleton, Stack, TextField } from '@mui/material';
import { SessionItem } from '../../hooks/api/Dtos/sessions/SessionItem';
import CustomModal, { ModalSize } from './CustomModal';
import LoadingButton from '../Buttons/LoadingButton';
import { Currency } from '../../helpers/currencyHelper';
import { QuantifiedItem, QuantifiedItemPicker, SelectableItem } from '../Pickers/QuantifiedItemPicker';
import { useMenuItemsQuery } from '../../hooks/queries/implementations/useMenuItemsQuery';
import { MenuItem } from '../../hooks/api/Dtos/menuitems/MenuItem';

interface Props {
    readonly isOpen: boolean;
    readonly onClose: () => any;
    readonly items: SessionItem[];
    readonly onApplyDiscount: (discountedItems: DiscountedItem<SessionItem>[]) => any | Promise<any>;
}

interface InternalDiscountedItem<T>{
    item: T;
    quantity: number;
    discount: number;
}

export interface DiscountedItem<T> {
    readonly item: T;
    readonly newDiscount: number;
    readonly quantityToApply: number;
}

export const DiscountsModal = ({
    isOpen,
    onClose,
    items,
    onApplyDiscount,
}: Props) => {
    const defaultDiscount = 10;

    const { t, i18n } = useTranslation();
    
    const [itemsMap, setItemsMap] = useState(new Map<SessionItem, InternalDiscountedItem<SessionItem>[]>()); //One item can lead to multiple rows. Example: 0 Discount, 10% discount, 50% discount (3 rows)
    const [innerUnselectedItems, setInnerUnselectedItems] = useState<QuantifiedItem<InternalDiscountedItem<SessionItem>>[]>([]);
    const [innerSelectedItems, setInnerSelectedItems] = useState<QuantifiedItem<InternalDiscountedItem<SessionItem>>[]>([]);
    const [discountsResult, setDiscountsResult] = useState<DiscountedItem<SessionItem>[]>([]);
    
    const [discount, setDiscount] = useState<number>(defaultDiscount);
    const [isSending, setIsSending] = useState(false);

    const itemsQuery = useMenuItemsQuery(items.length == 0 ? undefined : {
        ids: items.map(i => i.menuItemId),
        page: 0,
    })
    const itemsLookup = useMemo(() => {
        const result = new Map<string, MenuItem>();
        for(const item of itemsQuery.data) {
            result.set(item.id, item);
        }
        return result;
    }, [itemsQuery.data])

    useEffect(() => {
        const result = new Map<SessionItem, InternalDiscountedItem<SessionItem>[]>();
        for(const item of items.map(i => i as SessionItem)) {
            if(item.isPaid) {
                continue;
            }

            result.set(item, [{
                item: item,
                quantity: item.quantity,
                discount: item.discountPercentage,
            }]);
        }
        setItemsMap(result);
    }, [items])

    useEffect(() => {
        const selectedItemsResult: QuantifiedItem<InternalDiscountedItem<SessionItem>>[] = [];
        const unselectedItemsResult: QuantifiedItem<InternalDiscountedItem<SessionItem>>[] = [];
        for(const originalItem of items) {
            if(originalItem.isPaid) {
                continue;
            }

            const relatedItems = itemsMap.get(originalItem);
            if(!relatedItems) {
                return;
            }
            for(const relatedItem of relatedItems) {
                if(relatedItem.discount == 0) {
                    unselectedItemsResult.push({
                        item: {
                            item: originalItem,
                            discount: 0,
                            quantity: relatedItem.quantity,
                        },
                        quantity: relatedItem.quantity,
                    });
                } else {
                    selectedItemsResult.push({
                        item: {
                            item: originalItem,
                            discount: relatedItem.discount,
                            quantity: relatedItem.quantity,
                        },
                        quantity: relatedItem.quantity,
                    });
                }
            }
        }
        setInnerSelectedItems(selectedItemsResult);
        setInnerUnselectedItems(unselectedItemsResult);
    }, [itemsMap])

    useEffect(() => {
        const result: DiscountedItem<SessionItem>[] = [];
        for(const item of items) {
            if(item.isPaid) {
                continue;
            }

            const relatedItems = itemsMap.get(item)
            if(!relatedItems) {
                return;
            }
            for(const newItem of relatedItems) {
                const hasDiscount = newItem.discount != 0;
                if(item.discountPercentage == 0 && hasDiscount) {
                    result.push({
                        item: newItem.item,
                        newDiscount: newItem.discount,
                        quantityToApply: newItem.quantity,
                    })
                    continue;
                }

                if(item.discountPercentage > 0 && !hasDiscount) {
                    result.push({
                        item: newItem.item,
                        newDiscount: newItem.discount,
                        quantityToApply: newItem.quantity,
                    })
                    continue;
                }
            }
        }
        setDiscountsResult(result);
    }, [itemsMap])

    useEffect(() => setDiscount(defaultDiscount), [isOpen])

    const onConfirm = async () => {
        setIsSending(true);

        const closeAction = () => {
            setIsSending(false);
            onClose();
        }

        const r = onApplyDiscount(discountsResult);
        if(r != undefined && 'then' in r) {
            r.then(() => closeAction());
            return;
        }
        closeAction();
    }

    const onItemsChanged = (changedItems: SelectableItem<InternalDiscountedItem<SessionItem>>[]) => {
        const map = new Map<SessionItem, InternalDiscountedItem<SessionItem>[]>(itemsMap);
        for(const evt of changedItems) {
            const itemResult: InternalDiscountedItem<SessionItem>[] = [];
            const currentItems = itemsMap.get(evt.item.item)!;

            const addedDiscount = evt.quantity > 0;

            let itemWithoutDiscountReference: InternalDiscountedItem<SessionItem> | undefined = currentItems.find(c => c.discount == 0);
            if(itemWithoutDiscountReference == undefined) {
                itemWithoutDiscountReference = {
                    discount: 0,
                    item: evt.item.item,
                    quantity: 0,
                }
            }

            itemWithoutDiscountReference.quantity = itemWithoutDiscountReference.quantity - evt.quantity;
            if(itemWithoutDiscountReference.quantity > 0) {
                itemResult.push(itemWithoutDiscountReference)
            }

            let itemWithDiscountReference: InternalDiscountedItem<SessionItem> | undefined = undefined;
            for(const previousItem of currentItems) {
                if(previousItem.discount == 0) {
                    continue;
                }

                if(previousItem.discount != (addedDiscount ? discount : evt.item.discount)) {
                    itemResult.push(previousItem);
                    continue;
                }

                itemWithDiscountReference = previousItem;
            }

            if(itemWithDiscountReference == undefined) {
                itemWithDiscountReference = {
                    discount: discount,
                    item: evt.item.item,
                    quantity: 0,
                }
            }
            itemWithDiscountReference.quantity = itemWithDiscountReference.quantity + evt.quantity;
            if(itemWithDiscountReference.quantity > 0) {
                itemResult.push(itemWithDiscountReference)
            }

            map.set(evt.item.item, itemResult);
        }
        setItemsMap(map);
    }

    return (
        <CustomModal 
            isOpen={isOpen} 
            title={t("discounts")}
            size={ModalSize.ExtraLarge}
            onClose={onClose}
            disableCloseOutsideModal
            footer={
                <Grid container sx={{width: "100%", margin: "1rem 0.25rem"}} spacing={1}>
                    <Grid size="grow">
                        <LoadingButton isLoading={false} onClick={onClose} style={{width: "100%"}}>
                            {t("close")}
                        </LoadingButton>
                    </Grid>
                    <Grid size="grow">
                        <LoadingButton isLoading={isSending} disabled={discountsResult.length == 0} onClick={onConfirm} primaryButton style={{width: "100%"}}>
                            {t("confirm")}
                        </LoadingButton>
                    </Grid>
                </Grid>
            }
        >
            <Grid container rowSpacing={3} justifyContent="center">
                <Grid size={12}>
                    <QuantifiedItemPicker
                        unselectedItems={innerUnselectedItems}
                        selectedItems={innerSelectedItems}
                        getItemName={(item) => {
                            const menuItem = itemsLookup.get(item.item.menuItemId);
                            return (
                            <Stack
                                direction="row"
                                flex={1}
                                gap={2}
                            >
                                {
                                    menuItem == undefined
                                    ?
                                    <Skeleton animation="wave" sx={{flexGrow: "1" }}/>
                                    :
                                    menuItem.name
                                }
                                {
                                    item.discount > 0 &&
                                    <Chip size="small" label={`${Currency.toDecimalFormat({culture: i18n.language, value: item.discount, maxDecimalPlaces: 2})} %`} color="success" variant="outlined" />
                                }
                            </Stack>
                            )
                        }}
                        getItemKey={(item) => `${item.item.id}-${item.discount}`}
                        unselectedLabel={t("itemsWithNoDiscount")}
                        selectedLabel={t("discountedItems")}
                        selectButtonsDisable={discount == 0}
                        onChanged={(changedItems) => onItemsChanged(changedItems)}
                    />
                </Grid>
                <Grid size={{xs: 4}} sx={{mt: 3}}>
                    <Paper elevation={2}>
                        <TextField
                            label={t("discount")}
                            variant="standard"
                            type="number"
                            slotProps={{
                                htmlInput: {
                                    min: 0,
                                    max: 100,
                                    step: "0.01",
                                    style: {
                                        textAlign: 'center'
                                    }
                                },
                                input: {
                                    style: {
                                        fontSize: "2rem",
                                        fontWeight: "bold",
                                    },
                                    endAdornment: <InputAdornment position="start">
                                        <p style={{fontSize: "2rem", fontWeight: "bold", marginBottom: 0}}>%</p>
                                    </InputAdornment>,
                                }
                            }}
                            autoFocus
                            value={discount}
                            onChange={(e) => setDiscount(parseFloat(e.target.value))}
                            sx={{width: "100%"}}
                        />
                    </Paper>
                </Grid>
            </Grid>
        </CustomModal>
    )
}