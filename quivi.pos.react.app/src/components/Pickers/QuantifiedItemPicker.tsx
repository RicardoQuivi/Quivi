import React from "react";
import { GenericItemPicker } from "./GenericItemPicker"
import { Box } from "@mui/material";
import { useTranslation } from "react-i18next";
import { Currency } from "../../helpers/currencyHelper";

export interface QuantifiedItem<T> {
    readonly item: T;
    //If positive number, then items selected. If negative, then items unselected
    readonly quantity: number;
}

export interface SelectableItem<T> {
    readonly item: T;
    readonly quantity: number;
}

interface Props<T> {
    readonly unselectedItems: QuantifiedItem<T>[];
    readonly selectedItems: QuantifiedItem<T>[];
    readonly getItemName: (item: T) => React.ReactNode;
    readonly getItemKey: (item: T) => string;
    readonly getItemStyle?: (item: T) => React.CSSProperties | undefined;
    readonly unselectedLabel?: React.ReactNode;
    readonly selectedLabel?: React.ReactNode;
    readonly onChanged: (selectedItems: SelectableItem<T>[]) => any;
    readonly selectButtonsDisable?: boolean;
    readonly unselectButtonsDisable?: boolean;
    readonly maxHeight?: string;
}

export const QuantifiedItemPicker = <T,>({
    unselectedItems,
    selectedItems,
    getItemName,
    getItemKey,
    getItemStyle,
    onChanged,
    unselectedLabel,
    selectedLabel,
    selectButtonsDisable,
    unselectButtonsDisable,
    maxHeight,
}: Props<T>) => {
    const { i18n } = useTranslation();
    
    const onItemsSelected = (itemsThatWereSelected: QuantifiedItem<T>[]) => {
        const changedEvents : SelectableItem<T>[] = [];

        for(const unselectedItem of unselectedItems) {
            const item = itemsThatWereSelected.find(j => getItemKey(unselectedItem.item) == getItemKey(j.item));

            const wasPicked = item != undefined;
            if(wasPicked) {
                let selectedItem = selectedItems.find(i => getItemKey(i.item) == getItemKey(item.item));
                const itemWasAlreadyInSelection = selectedItem != undefined
                if(itemWasAlreadyInSelection == false) {
                    selectedItem = {
                        item: unselectedItem.item,
                        quantity: 0,
                    }
                }
                
                changedEvents.push({
                    item: unselectedItem.item,
                    quantity: item.quantity,
                })
                if(unselectedItem.quantity == 0) {
                    continue;
                }
            }
        }

        onChanged(changedEvents);
    }

    const onItemsUnselected = (itemsThatWereSelected: QuantifiedItem<T>[]) => {
        const changedEvents : SelectableItem<T>[] = [];

        const newSelectedItemFromList: QuantifiedItem<T>[] = [];
        const newUnselectedItemFromList: QuantifiedItem<T>[] = [...unselectedItems];
        for(const selectedItem of selectedItems.map(i => ({...i}))){
            const item = itemsThatWereSelected.find(j => getItemKey(selectedItem.item) == getItemKey(j.item));

            const wasPicked = item != undefined;
            if(wasPicked) {
                let unselectedItem = newUnselectedItemFromList.find(i => getItemKey(i.item) == getItemKey(item.item));
                if(unselectedItem == undefined) {
                    unselectedItem = {
                        item: selectedItem.item,
                        quantity: 0,
                    }
                    newUnselectedItemFromList.push(unselectedItem);
                }

                changedEvents.push({
                    item: unselectedItem.item,
                    quantity: -item.quantity,
                })
                if(selectedItem.quantity == 0) {
                    continue;
                }

            }

            newSelectedItemFromList.push(selectedItem);
        }

        onChanged(changedEvents);
    }

    return (
        <GenericItemPicker
            maxHeight={maxHeight}
            unselectedItems={unselectedItems}
            selectedItems={selectedItems}
            getItemName={(item) => 
                <Box sx={{display: "flex", gap: 1, alignItems: "center"}}>
                    <b>{Currency.toDecimalFormat({value: item.quantity, culture: i18n.language, maxDecimalPlaces: 2})} x</b> {getItemName(item.item)}
                </Box>}
            getItemKey={(item) => getItemKey(item.item)}
            getItemStyle={(item) => getItemStyle?.(item.item)}
            onItemsAdded={(items, isAll) => onItemsSelected(items.map(item => ({
                item: item.item,
                quantity: isAll ? item.quantity : (item.quantity >= 1 ? 1 : item.quantity),
            })))}
            onItemsRemoved={(items, isAll) => onItemsUnselected(items.map(item => ({
                item: item.item,
                quantity: isAll ? item.quantity : (item.quantity >= 1 ? 1 : item.quantity),
            })))}
            unselectedLabel={unselectedLabel}
            selectedLabel={selectedLabel}
            selectButtonsDisable={selectButtonsDisable}
            unselectButtonsDisable={unselectButtonsDisable}
        />
    )
}