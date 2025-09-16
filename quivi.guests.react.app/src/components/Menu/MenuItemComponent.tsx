import { useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { Avatar, Box, Chip, Grid, ImageList, ImageListItem, Skeleton, Typography } from "@mui/material";
import type { IItem } from "../../context/cart/item";
import type { ICartItem } from "../../context/cart/ICartItem";
import { useCart } from "../../context/OrderingContextProvider";
import { useChannelContext } from "../../context/AppContextProvider";
import { ItemsHelper } from "../../helpers/ItemsHelper";
import { Formatter } from "../../helpers/formatter";
import { QuantitySelector } from "../Quantity/QuantitySelector";


interface Props {
    readonly menuItem: IItem | ICartItem | null;
    readonly disableQuickCart?: boolean;
    readonly quickCartAlwaysOpened?: boolean;
    readonly onItemSelected?: () => any;
    readonly exactItemMatch?: boolean;
}

export const MenuItemComponent: React.FC<Props> = (props: Props) => {
    const cart = useCart();
    const { t } = useTranslation();

    const channelContext = useChannelContext();
    const merchantLogo = channelContext.logoUrl;
    const [itemQty, setItemQty] = useState(0);
    const [isPhotoLoaded, setIsPhotoLoaded] = useState(false);

    //#region User Actions
    const decrementQty = (item: IItem | ICartItem) => { 
        if(itemQty > 0) {
            cart.removeItem(item);
            setItemQty(prev => prev - 1);
        }
    }

    const incrementQty = (item: IItem | ICartItem) => {
        const isCartItem = 'quantity' in item;
        if(!isCartItem && 'modifiers' in item && item.modifiers.length > 0) {
            props.onItemSelected && props.onItemSelected();
            return;
        }
        cart.addItem({
            ...item,
            quantity: 1,
        });
        setItemQty(prev => prev + 1);
    }
    //#endregion

    //#region Effects
    useEffect(() => {
        if(props.menuItem == null) {
            setItemQty(0);
            return;
        }

        const qty = cart?.getQuantityInCart(props.menuItem!, props.exactItemMatch ?? false) || 0;
        setItemQty(qty);
    }, [cart, props.menuItem]);
    //#endregion

    const hasModifiers = () => {
        if(props.menuItem == null) {
            return false;
        }

        if('modifiers' in props.menuItem) {
            return !!props.menuItem.modifiers && props.menuItem.modifiers.length > 0;
        }
        
        return false;
    }

    const selectedModifierOptions = useMemo(() => {
        if(props.menuItem == null) {
            return [];
        }

        if('modifiers' in props.menuItem == false) {
            return [];
        }
        
        const result = [] as ICartItem[]
        for(const s of props.menuItem.modifiers ?? []) {
            if('selectedOptions' in s) {
                for(const item of s.selectedOptions) {
                    result.push(item);
                }
            }
        }

        return result;
    }, [props.menuItem])

    const loading = isPhotoLoaded == false || props.menuItem == null;
    return <>
        <Box
            sx={{
                cursor: "pointer",
                backgroundColor: "#F1F3F1",
                borderRadius: "5px",
                display: "flex",
                flexDirection: "row",
                alignItems: "center",
                boxShadow: "0 0px 8px 0 rgba(0, 0, 0, 0.1), 0 0px 20px 0 rgba(0, 0, 0, 0.19)",
                opacity: props.menuItem?.isAvailable != true ? "0.6" : "1",
            }}
            onClick={props.onItemSelected}
        >
            <Box
                sx={{
                    height: "100%",
                    width: "95px",
                    borderRadius: "5px 0 0 5px",
                    objectFit: "cover",
                    marginLeft: "0",
                    aspectRatio: 1,
                }}
            >
                {
                    loading &&
                    <Skeleton
                        variant="rectangular"
                        animation="pulse"
                        height="100%"
                        width="100%"
                    />
                }
                {
                    props.menuItem != null &&
                    <img
                        src={!props.menuItem.imageUrl ? merchantLogo : props.menuItem.imageUrl}
                        style={{
                            display: !loading ? "unset" : "none",
                            height: "100%",
                            width: "100%",
                        }}
                        onLoad={() => setIsPhotoLoaded(true)}
                    />
                }
            </Box>

            <Box
                sx={{
                    display: "flex",
                    flexDirection: "column",
                    width: "calc(100% - 95px)",
                    padding: "0 10px",
                    paddingLeft: "20px",
                }}
            >
                <Grid container spacing={0}>
                    <Grid 
                        size={12}
                        display="flex"
                        fontSize="1rem"
                    >
                        <Typography
                            variant="body2"
                            sx={{
                                overflow: "hidden",
                                textOverflow: "ellipsis",
                                whiteSpace: "nowrap",
                                width: "100%"
                            }}
                            gutterBottom
                        >
                        {
                            loading
                            ?
                            <Skeleton variant="text" animation="wave" height="1.5rem" width="70%" />
                            :
                            props.menuItem.name
                        }
                        </Typography>
                    </Grid>
                    <Grid
                        size={12}
                        sx={{
                            borderColor: "inherit",
                            color: "inherit",
                            maxWidth: "100%",

                            '& .MuiImageList-root::-webkit-scrollbar': {
                                display: 'none',
                                msOverflowStyle: "none",
                                scrollbarWidth: "none",
                            },

                            '& .MuiImageList-root': {
                                flexWrap: "nowrap",
                                transform: "translateZ(0)",
                                display: "flex",
                                margin: 0,
                                
                                '& .MuiImageListItem-root': {
                                    height: "auto !important",
                                    width: "auto !important",

                                    '& .MuiImageListItem-item': {
                                        height: "auto",
                                    }
                                },
                            },

                            '& .MuiChip-outlined': {
                                borderColor: "inherit",
                            },

                            '& .MuiChip-root': {
                                color: "inherit",

                                '& .MuiChip-label': {
                                    color: "inherit",
                                }
                            },
                        }}
                    >
                    {
                        loading == false && 
                        selectedModifierOptions.length > 0 &&
                        hasModifiers() &&
                        <ImageList>
                            {
                                selectedModifierOptions.map((o, index) => 
                                    <ImageListItem key={`${o.id}-${index}`}>
                                        <Chip label={<span>{o.quantity > 1 && <b>{o.quantity} x </b>}{o.name}</span>} variant="outlined" size="small" avatar={!o.imageUrl ? undefined : <Avatar src={o.imageUrl}/>}/>
                                    </ImageListItem>
                                )
                            }
                        </ImageList>
                    }
                    </Grid>
                    <Grid size={12}>
                        <Box
                            sx={{
                                display: "flex",
                                flexDirection: 'row',
                                justifyContent: "space-between",
                                paddingTop: "6px",
                            }}
                        >
                            {
                                loading
                                ?
                                <Skeleton variant="text" animation="wave" height="1.5rem" width="20%" />
                                :
                                <>
                                    <span style={{ fontWeight: 600 }}>{Formatter.price(ItemsHelper.getItemPrice(props.menuItem), "€")}</span>
                                    {
                                        props.menuItem != null &&
                                        <>
                                            {
                                                props.menuItem.isAvailable == false
                                                ? 
                                                    <span>{t("digitalMenu.unavailable")}</span>
                                                :
                                                    (
                                                        props.disableQuickCart != true &&
                                                        <QuantitySelector quantity={itemQty} 
                                                            onDecrement={() => decrementQty(props.menuItem!)} 
                                                            onIncrement={() => incrementQty(props.menuItem!)}
                                                            alwaysOpened={props.quickCartAlwaysOpened ?? false}
                                                            shouldCollapse={(props.quickCartAlwaysOpened ?? false) ? false : itemQty == 0} 
                                                        />
                                                    )
                                            }
                                        </>
                                    }
                                </>
                            }
                        </Box>
                    </Grid>
                </Grid>
            </Box>
        </Box>
    </>
}