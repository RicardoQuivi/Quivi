import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuiviTheme, type IColor } from "../../hooks/theme/useQuiviTheme";
import makeStyles from "@mui/styles/makeStyles";
import { Avatar, Chip, Grid, ImageList, ImageListItem, Skeleton, type Theme } from "@mui/material";
import type { IItem } from "../../context/cart/item";
import type { ICartItem } from "../../context/cart/ICartItem";
import { useCart } from "../../context/OrderingContextProvider";
import { useChannelContext } from "../../context/AppContextProvider";
import { ItemsHelper } from "../../helpers/ItemsHelper";
import { Formatter } from "../../helpers/formatter";
import { QuantitySelector } from "../Quantity/QuantitySelector";

interface StyleProps {
    readonly primarycolor: IColor,
    readonly padding?: string;
}

const useStyles = makeStyles<Theme, StyleProps>(() => ({
    container: {
        cursor: "pointer",
        backgroundColor: "#F1F3F1",
        borderRadius: "5px",
        display: "flex",
        flexDirection: "row",
        alignItems: "center",
        boxShadow: "0 0px 8px 0 rgba(0, 0, 0, 0.1), 0 0px 20px 0 rgba(0, 0, 0, 0.19)",
        opacity: "1",
        
        "&.unavailable": {
            opacity: "0.6",
        }
    },
    photo: {
        height: "100%",
        width: "95px",
        borderRadius: props => props.padding ? "5px" : "5px 0 0 5px",
        objectFit: "cover",
        marginLeft: props => props.padding ? props.padding : "0",
        aspectRatio: 1,
    },
    infoContainer: {
        display: "flex",
        flexDirection: "column",
        width: props => `calc(100% - 95px - ${props.padding || "0px"})`,
        padding: "0 10px",
        paddingLeft: "20px",
    },
    nameContainer: {
        display: "flex",
        fontSize: "1rem",
    },
    badgesContainer: {
        borderColor: "inherit",
        color: "inherit",
        maxWidth: "100%",

        '& .MuiImageList-root::-webkit-scrollbar': {
            display: 'none',
            "-ms-overflow-style": "none",
            scrollbarWidth: "none",
        },

        '& .MuiImageList-root': {
            flexWrap: "nowrap",
            transform: "translateZ(0)",

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
    },
    nameTxt: {
        overflow: "hidden",
        textOverflow: "ellipsis",
        whiteSpace: "nowrap",
    },
    priceContainer: {
        display: "flex",
        flexDirection: 'row',
        justifyContent: "space-between",
        paddingTop: "6px",
    },
    priceTxt: {
        fontWeight: 600,
    },
    qtyRemoveContainer: {
        display: "flex",
        flexDirection: "row",
        alignItems: "center",
        justifyContent: "space-between",
        marginTop: "10px",
    },
    removeTxt: {
        fontSize: "0.85rem",
        textDecoration: "underline",
        cursor: "pointer",
    },
    quantityComponent: {
        display: "inline-flex",
        alignItems: "center",
        margin: "-12px 0",
    },
    quantityTxt: {
        fontSize: "1.3rem",
        fontWeight: 400,
    },
    quantityBtn: {
        width: "26px",
        height: "26px",
        backgroundColor: "#E9F8F5",
        fill: props => props.primarycolor.hex,
    },
    outOfStockLabel: {
        // border: "1px transparent solid",
        // borderRadius: "10px",
        // background: "lightgray",
        // margin: "0",
        // padding: "6px",
    }
}));

interface Props {
    readonly menuItem: IItem | ICartItem | null;
    readonly disableQuickCart?: boolean;
    readonly quickCartAlwaysOpened?: boolean;
    readonly onItemSelected?: () => any;
    readonly exactItemMatch?: boolean;
}

export const MenuItemComponent: React.FC<Props> = (props: Props) => {
    const theme = useQuiviTheme();
    const classes = useStyles({ primarycolor: theme.primaryColor});
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

    const getSelectedModifierOptions = (): ICartItem[] => {
        if(props.menuItem == null) {
            return [];
        }

        if('modifiers' in props.menuItem) {
            return (props.menuItem.modifiers ?? []).map(s => 'selectedOptions' in s ? s.selectedOptions: []).reduce<ICartItem[]>((r, a) => {
                if(!a) {
                    return r;
                }
                return [...r, ...a];
            }, []);
        }
        
        return [];
    }

    const loading = isPhotoLoaded == false || props.menuItem == null;
    
    return <>
        <div className={`${classes.container} ${props.menuItem?.isAvailable != true ? "unavailable" : ""}`} onClick={props.onItemSelected}>
            {
                loading &&
                <Skeleton variant="rectangular" animation="pulse" className={classes.photo} height="95px" width="95px" />
            }
            {
                props.menuItem != null &&
                <img className={classes.photo} src={!props.menuItem.imageUrl ? merchantLogo : props.menuItem.imageUrl} style={{display: !loading ? "unset" : "none"}} onLoad={() => setIsPhotoLoaded(true)} />
            }
            
            <div className={classes.infoContainer}>
                <Grid container spacing={0}>
                    <Grid size={12}>
                        <div className={classes.nameContainer}>
                            {
                                loading
                                ?
                                <Skeleton variant="text" animation="wave" height="1.5rem" width="70%" className={classes.nameTxt} />
                                :
                                <p className={classes.nameTxt} title={props.menuItem.name}>{props.menuItem.name}</p>
                            }
                        </div>
                    </Grid>
                    <Grid size={12}>
                        <div className={classes.badgesContainer}>
                        {
                            loading == false && 
                            hasModifiers() && 
                            <ImageList>
                                {
                                    getSelectedModifierOptions().map((o, index) => 
                                        <ImageListItem key={`${o.id}-${index}`}>
                                            <Chip label={<span>{o.quantity > 1 && <b>{o.quantity} x </b>}{o.name}</span>} variant="outlined" size="small" avatar={!o.imageUrl ? undefined : <Avatar src={o.imageUrl}/>}/>
                                        </ImageListItem>
                                    )
                                }
                            </ImageList>
                        }
                        </div>
                    </Grid>
                    <Grid size={12}>
                        <div className={classes.priceContainer}>
                            {
                                loading
                                ?
                                <Skeleton variant="text" animation="wave" height="1.5rem" width="20%" />
                                :
                                <>
                                    <span className={classes.priceTxt} >{Formatter.price(ItemsHelper.getItemPrice(props.menuItem), "€")}</span>
                                    {
                                        props.menuItem != null &&
                                        <>
                                            {
                                                props.menuItem.isAvailable == false
                                                ? 
                                                    <span className={classes.outOfStockLabel}>{t("digitalMenu.unavailable")}</span>
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
                        </div>
                    </Grid>
                </Grid>
            </div>
        </div>
    </>
}