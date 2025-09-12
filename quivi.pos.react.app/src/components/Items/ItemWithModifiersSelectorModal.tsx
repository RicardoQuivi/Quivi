import { useEffect, useMemo, useState } from "react";
import { Trans, useTranslation } from "react-i18next";
import { Alert, Button as MUIButton, ImageList, Step, StepLabel, Stepper,  useMediaQuery, useTheme, Typography, ButtonGroup, Grid } from "@mui/material";
import BigNumber from "bignumber.js";
import { MenuItem, ModifierGroup, ModifierGroupOption } from "../../hooks/api/Dtos/menuitems/MenuItem";
import CustomModal, { ModalSize } from "../Modals/CustomModal";
import LoadingButton from "../Buttons/LoadingButton";
import CurrencySpan from "../Currency/CurrencySpan";
import { useMenuItemsQuery } from "../../hooks/queries/implementations/useMenuItemsQuery";
import { CollectionFunctions } from "../../helpers/collectionsHelper";
import { ItemCard } from "./ItemCard";
import { MenuItemWithExtras } from "../../hooks/pos/session/ICartSession";

interface Props {
    readonly item?: MenuItem;
    readonly onSelect: (item: MenuItemWithExtras) => any;
    readonly onClose?: () => any;
}
export const ItemWithModifiersSelectorModal = (props: Props) => {
    const { t } = useTranslation();
    const theme = useTheme();
    const xs = useMediaQuery(theme.breakpoints.only('xs'));
    const sm = useMediaQuery(theme.breakpoints.only('sm'));
    const md = useMediaQuery(theme.breakpoints.only('md'));

    const [state, setState] = useState(() => ({
        isSubmitting: false,
        currentModifierIndex: 0,
    }))
    const [modifiersSelectionMap, setModifiersSelectionMap] = useState(() => new Map<string, ModifierGroupOption[]>());
    const modifiers = props.item?.modifierGroups ?? [];

    const itemIds = useMemo(() => {
        if(props.item == undefined) {
            return [];
        }

        const set = new Set<string>();
        for(const group of props.item.modifierGroups) {
            for(const option of group.options) {
                set.add(option.menuItemId);
            }
        }
        return Array.from(set);
    }, [props.item])

    const itemsQuery = useMenuItemsQuery(itemIds.length == 0 ? undefined : {
        ids: itemIds,
        page: 0,
        includeDeleted: true,
    })
    const itemsMap = useMemo(() => CollectionFunctions.toMap(itemsQuery.data, i => i.id), [itemsQuery.data]);

    useEffect(() => setState(() => ({
        isOpen: props.item != undefined,
        isSubmitting: false,
        currentModifierIndex: 0,
    })), [props.item])
    
    useEffect(() => {
        if(props.item == undefined) {
            return;
        }

        const map = new Map<string, ModifierGroupOption[]>();
        for(const m of props.item.modifierGroups) {
            map.set(m.id, []);
        }
        setModifiersSelectionMap(map);
    }, [props.item])

    const canSubmit = useMemo(() => {
        const modifiers = props.item?.modifierGroups ?? [];
        return modifiers.every((_, i) => canGoForth(i, modifiers, modifiersSelectionMap));
    }, [props.item, modifiersSelectionMap]);

    const price = useMemo(() => {
        let price = new BigNumber(props.item?.price ?? 0);

        for(const selections of modifiersSelectionMap.values()) {
            for(const item of selections) {
                price = price.plus(item.price);
            }
        }

        return price.toNumber();
    }, [props.item, modifiersSelectionMap])

    const onItemAdded = (modifierId: string, m: ModifierGroupOption) => {
        const selectionTotal = (modifiersSelectionMap.get(modifierId)?.length ?? 0)
        const max = props.item?.modifierGroups.find(m => m.id == modifierId)!.maxSelection;
        
        const map = new Map(modifiersSelectionMap);
        if(max == 1 && selectionTotal == 1) {
            map.set(modifierId, [m]);
        } else {
            const entry = map.get(modifierId)!;
            entry.push(m);
        }
        setModifiersSelectionMap(map);
    }

    const onItemRemoved = (modifierId: string, m: ModifierGroupOption) => {
        const map = new Map(modifiersSelectionMap);

        const newEntry: ModifierGroupOption[] = [];
        const entry = map.get(modifierId)!;
        let removed = false;
        for(let i = 0; i < entry.length; ++i) {
            if(removed == false && m.id == entry[i].id) {
                removed = true;
                continue;
            }

            newEntry.push(entry[i]);
        }
        map.set(modifierId, newEntry);
        setModifiersSelectionMap(map);
    }

    const submit = async () => {       
        const r = props.onSelect({
            ...props.item!,
            selectedOptions: modifiersSelectionMap,
        });
        if(r instanceof Promise) {
            setState(s => ({ ...s, isSubmitting: true}));
            await r;
            setState(s => ({ ...s, isSubmitting: false}));
            props.onClose?.()
            return;
        } else {
            props.onClose?.();
        }
    }

    return (
    <CustomModal 
        isOpen={props.item != undefined}
        title={t("itemsTab.itemConfigurator", {name: props.item?.name ?? ""})}
        size={xs || sm ? ModalSize.FullScreen : ModalSize.ExtraLarge}
        onClose={props.onClose}
        disableCloseOutsideModal={true}
        footer={
            <Grid
                container
                sx={{
                    width: "100%",
                    margin: "1rem 0.25rem",
                }}
                spacing={1}
            >
                <Grid size="grow">
                {
                    state.currentModifierIndex == 0
                    ?
                    <LoadingButton
                        isLoading={false}
                        onClick={props.onClose}
                        style={{
                            width: "100%",
                        }}
                    >
                        {t("cancel")}
                    </LoadingButton>
                    :
                    <LoadingButton
                        isLoading={false}
                        onClick={() => setState(s => ({ ...s, currentModifierIndex: s.currentModifierIndex - 1 }))}
                        style={{
                            width: "100%",
                        }}
                    >
                        {t("back")}
                    </LoadingButton>
                }
                </Grid>
                <Grid size="grow">
                {
                    modifiers.length - 1 == state.currentModifierIndex
                    ?
                    <LoadingButton
                        isLoading={state.isSubmitting}
                        primaryButton
                        onClick={submit}
                        disabled={canSubmit == false}
                        style={{
                            width: "100%",
                        }}
                    >
                        <Trans
                            t={t}
                            i18nKey="itemsTab.addConfiguredItem"
                            shouldUnescape={true}
                            components={{
                                price: <CurrencySpan value={price}/>,
                            }}
                        />
                    </LoadingButton>
                    :
                    <LoadingButton 
                        isLoading={false}
                        primaryButton
                        onClick={() => setState(s => ({ ...s, currentModifierIndex: s.currentModifierIndex + 1 }))}
                        disabled={modifiers.length == 0 || canGoForth(state.currentModifierIndex, modifiers, modifiersSelectionMap) == false}
                        style={{width: "100%"}}
                    >
                        {t("next")}
                    </LoadingButton>
                }
                </Grid>
            </Grid>
        }
    >
    {
        modifiers.length > 0 &&
        <>
            <Stepper
                activeStep={state.currentModifierIndex}
                alternativeLabel
            >
            {
                modifiers.map(g => (
                    <Step key={g.id}>
                        <StepLabel
                            optional={g.minSelection == 0 ? <Typography variant="caption">{t("itemsTab.optionalItem")}</Typography> : undefined}
                        >
                            {g.name}
                        </StepLabel>
                    </Step>
                ))
            }
            </Stepper>
            <Alert severity="info">
                {
                    modifiers[state.currentModifierIndex].minSelection != modifiers[state.currentModifierIndex].maxSelection &&
                    t("itemsTab.selectionRange", { 
                        min: modifiers[state.currentModifierIndex].minSelection ?? 0, 
                        max: modifiers[state.currentModifierIndex].maxSelection ?? 100, 
                    })
                }
                {
                    modifiers[state.currentModifierIndex].minSelection == modifiers[state.currentModifierIndex].maxSelection &&
                    t("itemsTab.selectExactly", { 
                        caption: `${modifiers[state.currentModifierIndex].minSelection ?? 0} ${(modifiers[state.currentModifierIndex].minSelection == 1 ? t("item") : t("items")).toLowerCase()}`,
                    })
                }
            </Alert>
            <ImageList
                sx={{
                    mt: "1rem",
                }}
                cols={xs || sm ? 2 : (md ? 3 : 6)}
            >
                {
                    modifiers[state.currentModifierIndex].options.map(m => {
                        const currentModifierGroup = modifiers[state.currentModifierIndex];
                        const selectedQuantity = modifiersSelectionMap.get(currentModifierGroup.id)?.filter(f => f.id == m.id).length ?? 0;
                        const mayAdd = canAdd(state.currentModifierIndex, modifiers, modifiersSelectionMap) || (selectedQuantity != 1 && currentModifierGroup.maxSelection == 1);
                        const item = itemsMap.get(m.menuItemId);

                        return (
                        <ItemCard
                            key={m.id}
                            image={item?.imageUrl}
                            name={item?.name}
                            price={m.price ?? 0}
                            onClick={mayAdd ? () => onItemAdded(modifiers[state.currentModifierIndex].id, m) : undefined}
                            selectedQuantity={selectedQuantity}
                        >
                            <ButtonGroup
                                sx={{
                                    mt: "0.25rem",
                                }}
                            >
                                <MUIButton
                                    sx={{
                                        width: "50%",
                                    }}
                                    variant="outlined"
                                    onClick={(e) => {
                                        if(selectedQuantity > 0) {
                                            onItemRemoved(modifiers[state.currentModifierIndex].id, m);
                                        }
                                        e.stopPropagation();
                                    }}
                                >
                                    -
                                </MUIButton>
                                <MUIButton
                                    sx={{
                                        width: "50%",
                                    }}
                                    variant="contained"
                                    onClick={(e) => {
                                        if(mayAdd) {
                                            onItemAdded(modifiers[state.currentModifierIndex].id, m);
                                        }
                                        e.stopPropagation();
                                    }}
                                >
                                    +
                                </MUIButton>
                            </ButtonGroup>
                        </ItemCard>)
                    })
                }
            </ImageList>
        </>
    }
    </CustomModal>
    )
}

const canAdd = (index: number, modifiers: ModifierGroup[], modifiersSelectionMap:  Map<string, ModifierGroupOption[]>) => {
    const currentGroup = modifiers[index];
    const max = currentGroup.maxSelection ?? 0;
    const selectionTotal = (modifiersSelectionMap.get(currentGroup.id)?.length ?? 0)
    return selectionTotal < max;
}

const canGoForth = (index: number, modifiers: ModifierGroup[], modifiersSelectionMap:  Map<string, ModifierGroupOption[]>) => {
    const currentGroup = modifiers[index];
    const min = currentGroup.minSelection ?? 0;
    const max = currentGroup.maxSelection ?? 0;
    const selectionTotal = (modifiersSelectionMap.get(currentGroup.id)?.length ?? 0)
    return min <= selectionTotal && selectionTotal <= max;
}