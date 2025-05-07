import { Chip, Grid, Paper, Skeleton, Stack, ToggleButton, ToggleButtonGroup } from "@mui/material";
import React, { useEffect, useMemo, useState } from "react"
import { useTranslation } from "react-i18next";
import { QuantifiedItem, QuantifiedItemPicker, SelectableItem } from "../Pickers/QuantifiedItemPicker";
import { SessionItem } from "../../hooks/api/Dtos/sessions/SessionItem";
import { PaymentAmountType } from "../../hooks/api/Dtos/payments/PaymentAmountType";
import { Currency } from "../../helpers/currencyHelper";
import { Items } from "../../helpers/itemsHelpers";
import DecimalInput from "../Inputs/DecimalInput";
import CurrencySpan from "../Currency/CurrencySpan";
import { useMenuItemsQuery } from "../../hooks/queries/implementations/useMenuItemsQuery";
import { MenuItem } from "../../hooks/api/Dtos/menuitems/MenuItem";

interface InternalQuantifiedItem<T> extends QuantifiedItem<T> {
    quantity: number;
}

interface Props {
    readonly bill: number;
    readonly unpaidItems: SessionItem[];
    readonly partialAmount?: number;
    readonly amountType: PaymentAmountType;
    readonly selectedItems?: QuantifiedItem<SessionItem>[];
    readonly onPayAmountChanged?: (value: number) => void;
    readonly onAmountTypeChanged: (value: PaymentAmountType) => void;
    readonly onIsValidChanged: (isValid: boolean) => void;
    readonly onSelectedItemsChanged?: (items: QuantifiedItem<SessionItem>[]) => void;
}
export const PaymentAmount: React.FC<Props> = (props) => {
    const { t, i18n } = useTranslation();

    const [state, setState] = useState({
        amountTxt: "",
        selectedItemsAmount: 0,
        isValid: true,

        unselectedItems: [] as InternalQuantifiedItem<SessionItem>[],
        selectedItems: [] as InternalQuantifiedItem<SessionItem>[],
    })

    const itemIds = useMemo(() => {
        const set = new Set<string>();

        for(const item of state.selectedItems) {
            set.add(item.item.menuItemId);
            for(const extra of item.item.extras) {
                set.add(extra.menuItemId);
            }
        }

        for(const item of state.unselectedItems) {
            set.add(item.item.menuItemId);
            for(const extra of item.item.extras) {
                set.add(extra.menuItemId);
            }
        }

        return Array.from(set.values());
    }, [state.selectedItems, state.unselectedItems])

    const itemsQuery = useMenuItemsQuery(itemIds.length == 0 ? undefined : {
        ids: itemIds,
        page: 0,
    })
    const itemsMap = useMemo(() => {
        const map = new Map<string, MenuItem>();
        for(const item of itemsQuery.data) {
            map.set(item.id, item);
        }
        return map;
    }, [itemsQuery.data])

    const amountErrorMsg = (valueTxt: string): string | null => {
        const value = Number(valueTxt);

        if (!valueTxt?.length)
            return t("amountErrors.required");
        if (value !== 0 && !value)
            return t("amountErrors.invalid");
        if (value <= 0)
            return props.bill > 0 ? t("amountErrors.minValue", { min: Currency.toCurrencyFormat({value: 0.01, culture: i18n.language }) }) : null;
        if (value > props.bill)
            return t("amountErrors.maxValue", { max: Currency.toCurrencyFormat({value: props.bill, culture: i18n.language}) });
        return null;
    }

    const getPayAmountTypeLabel = (type: PaymentAmountType) => t(`paymentTypes.${PaymentAmountType[type]}`);

    const onSelectedItemsChanged = (changedItems: SelectableItem<SessionItem>[]) => {
        let selectedItemsResult: InternalQuantifiedItem<SessionItem>[] = [...state.selectedItems];
        let unselectedItemsResult: InternalQuantifiedItem<SessionItem>[] = [...state.unselectedItems];

        for(const item of changedItems) {
            let unselectedItem = unselectedItemsResult.find(u => u.item.id == item.item.id);
            let selectedItem = selectedItemsResult.find(u => u.item.id == item.item.id);

            if(selectedItem == undefined) {
                selectedItem = {
                    item: item.item,
                    quantity: 0,
                }
                selectedItemsResult.push(selectedItem);
            }

            if(unselectedItem == undefined) {
                unselectedItem = {
                    item: item.item,
                    quantity: 0,
                }
                unselectedItemsResult.push(unselectedItem);
            }

            unselectedItem.quantity -= item.quantity;
            selectedItem.quantity += item.quantity;
        }

        selectedItemsResult = selectedItemsResult.filter(i => i.quantity > 0);
        unselectedItemsResult = unselectedItemsResult.filter(i => i.quantity > 0);
        setState(s => ({
            ...s,
            selectedItems: selectedItemsResult,
            unselectedItems: unselectedItemsResult,
        }))
        props.onSelectedItemsChanged?.(selectedItemsResult);
    }

    useEffect(() => setState(s => ({...s, amountTxt: props.partialAmount?.toString() ?? props.bill?.toString() ?? ""})), [props.partialAmount, props.bill]);

    useEffect(() => {
        if (!state.amountTxt) {
            return;
        }

        const isValid = !amountErrorMsg(state.amountTxt);
        if (isValid) {
            props.onPayAmountChanged?.(Number(state.amountTxt));
        }
    }, [state.amountTxt]);

    useEffect(() => {
        const selectedItemsResult: InternalQuantifiedItem<SessionItem>[] = [];
        const unselectedItemsResult: InternalQuantifiedItem<SessionItem>[] = [];

        for(const item of props.unpaidItems) {
            let availableQuantity = item.quantity;
            const selectedItem = props.selectedItems?.find(i => i.item.id == item.id);
            if(selectedItem != undefined) {
                selectedItemsResult.push(selectedItem);
                availableQuantity -= selectedItem.quantity;
                
                if(availableQuantity == 0) {
                    continue;
                }
            }

            unselectedItemsResult.push({
                item: item,
                quantity: availableQuantity,
            });
        }

        setState(s => ({
            ...s,
            selectedItems: selectedItemsResult,
            unselectedItems: unselectedItemsResult,
        }))
    }, [props.unpaidItems, props.selectedItems])

    useEffect(() => {
        switch (props.amountType) {
            case PaymentAmountType.Price:
                setState(s => ({...s, isValid: !amountErrorMsg(s.amountTxt)}))
                break
            case PaymentAmountType.ItemsSelection:
                setState(s => ({...s, isValid: !!props.selectedItems?.length}))
                break;
        }        
    }, [props.amountType, state.amountTxt, props.selectedItems]);

    useEffect(() => props.onIsValidChanged(state.isValid), [state.isValid]);

    useEffect(() => {
        const selectedItems = props.selectedItems ?? [];
        const price = Items.getTotalPrice(selectedItems.map(i => ({...i.item, quantity: i.quantity})));
        setState(s => ({...s, selectedItemsAmount: price}))
    }, [props.selectedItems]);

    return (
    <>
        <Grid container rowSpacing={3} justifyContent={"center"}>
            <Grid size={12} sx={{display: "flex", justifyContent: "center"}}>
                <ToggleButtonGroup
                    color="primary"
                    value={props.amountType}
                    exclusive
                    onChange={(_, value) => {
                        if (value == undefined) {
                            return;
                        }
                        props.onAmountTypeChanged(value);
                    }}
                    aria-label="Platform"
                >
                    <ToggleButton value={PaymentAmountType.Price}>{getPayAmountTypeLabel(PaymentAmountType.Price)}</ToggleButton>
                    <ToggleButton value={PaymentAmountType.ItemsSelection}>{getPayAmountTypeLabel(PaymentAmountType.ItemsSelection)}</ToggleButton>
                </ToggleButtonGroup>
            </Grid>
            {
                props.amountType == PaymentAmountType.Price &&
                <Grid size={4} sx={{mt: 2}} alignSelf="center" justifyContent="center" alignItems="center" alignContent="center" textAlign="center">
                    <DecimalInput 
                        textFieldProps={{
                            variant: "standard",
                        }}
                        value={Number(state.amountTxt)} 
                        errorMsg={amountErrorMsg(state.amountTxt)}
                        endAdornment={
                            <p style={{fontSize: "1.5rem", marginBottom: 0, marginTop: 0 }}>€</p>
                        }
                        style={{
                            fontSize: "1.5rem",
                            maxWidth: "200px",
                            textAlign: "center",
                        }}
                        onChange={(newValue) => setState(s => ({...s, amountTxt: newValue.toString()}))} />
                </Grid>
            }
            {
                props.amountType == PaymentAmountType.ItemsSelection &&
                <>
                    <Grid size={12}>
                        <QuantifiedItemPicker 
                            maxHeight="250px"
                            unselectedItems={state.unselectedItems}
                            selectedItems={state.selectedItems}
                            getItemName={(item) => {
                                const menuItem = itemsMap.get(item.menuItemId);

                                return <Stack
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
                                        item.discountPercentage > 0 &&
                                        <Chip size="small" label={`${Currency.toDecimalFormat({culture: i18n.language, value: item.discountPercentage, maxDecimalPlaces: 2})} %`} color="success" variant="outlined" />
                                    }
                                </Stack>
                            }}
                            getItemKey={item => item.id} 
                            onChanged={onSelectedItemsChanged} />
                    </Grid>
                    <Grid size={4} sx={{mt: 2}}>
                        <Paper elevation={2}>
                            <p style={{fontSize: "2rem", fontWeight: "bold", textAlign: "center"}}><CurrencySpan value={state.selectedItemsAmount} /></p>
                        </Paper>
                    </Grid>
                </>
            }
        </Grid>
    </>
    );
}