import { useEffect, useState } from 'react';
import { useTranslation } from "react-i18next";
import { QuantitySelector } from "../../components/Quantity/QuantitySelector";
import { ButtonsSection } from "../../layout/ButtonsSection";
import { useQuiviTheme } from '../../hooks/theme/useQuiviTheme';
import type { IBaseItem, IItem, IItemModifierGroup } from '../../context/cart/item';
import type { ICartItem, ICartModifier } from '../../context/cart/ICartItem';
import { AvatarImage } from '../../components/Avatars/AvatarImage';
import { Box, ButtonBase, Checkbox, FormControl, FormGroup, FormHelperText, FormLabel, Grid, ListItemText } from '@mui/material';
import { Formatter } from '../../helpers/formatter';
import { SquareButton } from '../../components/Buttons/SquareButton';

interface ModifierOptionProps {
    readonly modifier: IItemModifierGroup | ICartModifier;
    readonly onChange: (isValid: boolean, ids: { [index: string]: number; }) => any | void;
    readonly alwaysShowAvatar?: boolean;
}
const ModifierOption = (props: ModifierOptionProps) => {
    const theme = useQuiviTheme();
    const { t } = useTranslation();

    const [minAmountError, setMinAmountError] = useState(false);
    const [maxAmountError, setMaxAmountError] = useState(false);
    const [selectedOptions, setSelectedOptions] = useState<string[]>(() => {
        if('selectedOptions' in props.modifier) {
            const r = props.modifier.selectedOptions.reduce((r, v) => {
                for(let i = 0; i < v.quantity; ++i)
                    r.push(v.id);
                return r;
            }, [] as string[])
            return r;
        }
        return [];
    });
    const hasError = minAmountError || maxAmountError;

    useEffect(() => {
        setMinAmountError(props.modifier.minSelection > selectedOptions.length);
        setMaxAmountError(props.modifier.maxSelection < selectedOptions.length);
    }, [selectedOptions])

    const handleChange = (selectedValue: string, checked: boolean) => {
        let newOptions = [];

        if(props.modifier.maxSelection == 1 && checked) {
            newOptions.push(selectedValue);
        } else {
            const filteredSelections = selectedOptions.filter(s => s == selectedValue);
            const currentNumberOfSelections = filteredSelections.length;
            const desiredNumberOfSelections = currentNumberOfSelections + (checked ? 1 : -1);
            newOptions = selectedOptions.filter(s => s != selectedValue);
            for(let i = 0; i < desiredNumberOfSelections; ++i) {
                newOptions.push(selectedValue);
            }
        } 

        setSelectedOptions(newOptions);
    };

    useEffect(() => {
        const hasError = minAmountError || maxAmountError;
        const result: { [index: string]: number; } = {};
        for(let id of selectedOptions) {
            if(id in result)
                result[id]+= 1;
            else
                result[id] = 1;
        }
        props.onChange(!hasError, result);
    }, [minAmountError, maxAmountError, selectedOptions])

    const getErrorMessage = () => {
        if(minAmountError == false && maxAmountError == false) {
            return "";
        }

        if(props.modifier.minSelection == props.modifier.maxSelection) {
            if(props.modifier.minSelection == 1) {
                return t("digitalMenu.modifierValidation.selectOne");
            }
            return t("digitalMenu.modifierValidation.selectExactly", { number: props.modifier.minSelection });
        }

        return t("digitalMenu.modifierValidation.selectRange", { minimum: props.modifier.minSelection, maximum: props.modifier.maxSelection });
    }

    const getAvatar = (item: IBaseItem) => {
        const hasPhoto = !!item.imageUrl;
        if(hasPhoto || props.alwaysShowAvatar == true) {
            return <AvatarImage src={item.imageUrl} name={item.name} style={{
                alignSelf: "center",
                marginLeft: "0.5rem",
            }}/>
        }

        return undefined;
    }

    return <>
        <FormLabel sx={{color: "#585858", display: "inline-block"}}>
            {props.modifier.name}
        </FormLabel>
        {
            hasError &&
            <>
                &nbsp;
                <FormHelperText sx={{color: "#585858", display: "inline-block"}}>({getErrorMessage()})</FormHelperText>
            </>
        }
        <FormControl
            required
            error={hasError}
            component="fieldset"
            sx={{
                overflow: "hidden",
                color: '#585858 !important',

                '& .MuiFormLabel-root': {
                    color: '#585858',
                },

                '& .MuiFormLabel-asterisk': {
                    color: '#585858'
                },


                '& .Mui-focused': {
                    color: "inherit",
                    fontWeight: "bold",
                },

                '& .MuiListItem-root': {
                    paddingLeft: 0,
                    paddingTop: 0,
                    paddingBottom: 0,
                },

                '& .MuiFormHelperText-root': {
                    marginTop: "0.5rem",
                    marginBottom: "1rem",
                    color: '#585858'
                },

                '& .MuiIconButton-root': {
                    '& .MuiSvgIcon-root': {
                        fill: theme.primaryColor.hex,
                    }
                },

                '& .Mui-Error': {
                    color: '#585858'
                }
            }}
        >
            <FormGroup
                sx={{
                    marginTop: "1rem",
                    padding: "0 0.5rem",
                }}
            >
                <Grid 
                    container
                    spacing={1}
                    sx={{
                        margin: 0,
                    }}
                >
                    {
                        props.modifier.options.map(m => (
                            <Grid size={{xs: 12, sm: 12, md: 6, lg: 6, xl:4 }} key={m.id}>
                                <ButtonBase 
                                    onClick={() => props.modifier.maxSelection == 1 && handleChange(m.id, !selectedOptions.includes(m.id))} 
                                    sx={{
                                        display: "flex", 
                                        flexDirection: "row", 
                                        width: "100%", 
                                        justifyContent: "space-between",
                                        boxShadow: "rgba(0, 0, 0, 0.05) 0px 0px 0px 1px",
                                        height: "100%",
                                    }}
                                >
                                    { getAvatar(m) }
                                    <ListItemText
                                        sx={{
                                            flexGrow: 1,
                                            padding: "0 0.5rem",
                                            alignSelf: "center",
                                        }}
                                        primary={m.name}
                                        secondary={m.price > 0 ? `+ ${Formatter.price(m.price, "€")}` : undefined}
                                    />
                                    <Box
                                        sx={{
                                            height: "100%", 
                                            alignSelf: "center", 
                                            marginRight: "1rem",
                                            display: "flex",
                                            flexWrap: "wrap",
                                            alignContent: "center",

                                            "& .Mui-checked.MuiCheckbox-root": {
                                                color: theme.primaryColor.hex,
                                            }
                                        }}
                                    >
                                        {
                                            props.modifier.maxSelection == 1
                                            ?
                                                <Checkbox
                                                    edge="end"
                                                    onChange={(e) => handleChange(e.target.value, e.target.checked)}
                                                    checked={selectedOptions.includes(m.id)}
                                                    value={m.id}
                                                />
                                            :
                                                <ButtonBase
                                                    sx={{
                                                        padding: "9px 0px",
                                                    }}
                                                >
                                                    <QuantitySelector 
                                                        quantity={selectedOptions.filter(s => s == m.id).length}
                                                        onDecrement={() => handleChange(m.id, false)}
                                                        onIncrement={() => handleChange(m.id, true)}
                                                        shouldCollapse={false}
                                                        alwaysOpened={true}
                                                        pixelSize={24}
                                                    />
                                                </ButtonBase>
                                        }
                                    </Box>
                                </ButtonBase>
                            </Grid>
                        ))
                    }
                </Grid>
            </FormGroup>
        </FormControl>
    </>
}

interface Props {
    readonly item: IItem | ICartItem;
    readonly onModifiersChanged: (isValid: boolean, selectionDictionary: { [index: string]: ICartItem[]; }) => any;
    readonly onAddToCart: () => any;
    readonly className?: string;
}
export const MenuItemSelector = ({
    item,
    onModifiersChanged,
    onAddToCart,
    className,
}: Props) => {  
    const { t } = useTranslation();
    const theme = useQuiviTheme();
    
    const [activeModifierIndex, setActiveModifierIndex] = useState(0);
    const [selectedModifiers] = useState<Map<IItemModifierGroup, ICartItem[]>>(new Map<IItemModifierGroup, ICartItem[]>());
    const [validModifiers, setValidModifiers] = useState(() => {
        const result: { [index: string]: boolean; } = {};

        for(let k of item.modifiers ?? []) {
            result[k.id] = true;
        }

        return result;
    });

    useEffect(() => {
        if((item.modifiers ?? []).length == 0) {
            return;
        }

        const currentModifier = item.modifiers[activeModifierIndex];
        const isValid = validModifiers[currentModifier.id];
        if(isValid == false) {
            return;
        }

        if(currentModifier.minSelection == 1 && currentModifier.maxSelection == 1) {
            setActiveModifierIndex(p => p + (p + 1 < item.modifiers.length ? 1 : 0));
        }
    }, [validModifiers])

    useEffect(() => {
        const result: { [index: string]: boolean; } = {};

        for(const k of item.modifiers ?? []) {
            result[k.id] = true;
        }

        setActiveModifierIndex(0);
        setValidModifiers(result);
    }, [item])

    const updateSelectedModifiers = (modifierGroupId: string, isValid: boolean, ids: { [index: string]: number; }) => {
        const dictionary = new Map<string, IBaseItem>();

        const selectedModifierGroup = item.modifiers.find(v => v.id == modifierGroupId)!;
        for (let o of selectedModifierGroup.options) {
            dictionary.set(o.id, o);
        }
        const modifiers = Object.keys(ids).map(id => ({
            ...dictionary.get(id)!,
            quantity: ids[id],
            modifiers: [],
        }));
        selectedModifiers.set(selectedModifierGroup, modifiers);

        const result: { [index: string]: ICartItem[]; } = {};
        for(let k of selectedModifiers.keys()) {
            result[k.id] = selectedModifiers.get(k)!;
        }

        validModifiers[modifierGroupId] = isValid;
        setValidModifiers({ ...validModifiers });

        let allAreValid = selectionsAreValid();
        onModifiersChanged(allAreValid, result);
    }

    const selectionsAreValid = (): boolean => {
        for(let v of Object.keys(validModifiers)) {
            if(validModifiers[v] == false) {
                return false;
            }
        }
        return true;
    }

    const totalModifiers = (item.modifiers ?? []).length;
    return <div className={className} style={{
        height: "100%",
        position: "relative",
        display: "flex",
        flexDirection: "column",
    }}>
        {
            totalModifiers > 0 && item.modifiers.map((m) => 
                <div key={m.id} style={{marginTop: "1rem"}}>
                    <ModifierOption modifier={m} 
                                    onChange={(isValid, ids) => updateSelectedModifiers(m.id, isValid, ids)}
                                    alwaysShowAvatar={m.options.findIndex(o => !!o.imageUrl) != -1}
                                  
                    />  
                </div>
            )
        }
        <Box
            sx={{
                position: "sticky",
                bottom: 0,
                paddingBottom: "20px",
                background: "linear-gradient(0deg, rgba(255, 255, 255, 1) 0%, rgba(255, 255, 255, 1) 70%, rgba(255, 255, 255, 0) 100%)",

                flexGrow: 1,
                display: "flex",
                flexWrap: "wrap",
                alignContent: "flex-End",
            }}
        >
            <ButtonsSection transparent>
                <SquareButton
                    disabled={selectionsAreValid() == false} 
                    onClick={onAddToCart} 
                    color={theme.primaryColor} 
                    showShadow
                    style={{
                        color: "white",
                        border: 0,
                        padding: "1rem",
                        fontSize: "1.2rem",
                        fontWeight: 500,
                        width: "100%",
                        marginTop: "1rem",
                    }}
                >
                    {
                        'quantity' in item
                        ?
                            t("digitalMenu.updateItem")
                        :
                            t("digitalMenu.addItem")
                    }
                </SquareButton>
                {undefined}
            </ButtonsSection>
        </Box>
    </div>
}