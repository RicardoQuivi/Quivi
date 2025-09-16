import { useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import Dialog from "../../components/Shared/Dialog";
import BigNumber from "bignumber.js";
import { motion } from "framer-motion";
import { Box, Divider, IconButton, type Theme } from "@mui/material";
import { useQuiviTheme, type IColor } from "../../hooks/theme/useQuiviTheme";
import type { IItem, IItemModifierGroup } from "../../context/cart/item";
import type { ICartItem, ICartModifier } from "../../context/cart/ICartItem";
import { PageMode, usePageMode } from "../../hooks/usePageMode";
import { useCart } from "../../context/OrderingContextProvider";
import { ItemsHelper } from "../../helpers/ItemsHelper";
import { CloseIcon, DashCircleIcon, PlusCircleIcon } from "../../icons";
import { Formatter } from "../../helpers/formatter";
import { useSwipeable } from "react-swipeable";
import { useChannelContext } from "../../context/AppContextProvider";
import { TextSection } from "../../components/TextSection";
import { MenuItemSelector } from "./MenuItemSelector";
import makeStyles from "@mui/styles/makeStyles";

interface StyleProps {
    readonly primarycolor: IColor;
    readonly photo?: string;
}
const useStyles = makeStyles<Theme, StyleProps>((theme) => {
    const shadowSize = "2vh";
    const photoSize = "18vh";

    return ({
        dialog: {
            backgroundColor: "transparent", 
            boxShadow: "unset",

            [theme.breakpoints.down("sm")]: {
                "&.fullview": {
                    top: 0,
                },
            },
        },
        headerContainer: {
            position: "relative",
        },
        headerPivotContainer: {
            display: "flex",
            flexDirection: "column",

            [theme.breakpoints.down("sm")]: {
                "&.fullview": {
                    flexDirection: "row",
                    justifyContent: "space-evenly",
                },
            }
        },
        photoContainer: {
            width: '100%',
            height: `calc(${photoSize} + ${shadowSize})`,
            paddingTop: shadowSize,
            zIndex: 1,

            [theme.breakpoints.down("sm")]: {
                "&.fullview": {
                    height: "unset",
                    width: "unset",
                    margin: "0 1rem",
                }
            }
        },
        photo: {
            width: photoSize,
            filter: `drop-shadow(0px 4px ${shadowSize} black)`,
            height: photoSize,
            margin: "0 auto",
            borderRadius: "16px",
            backgroundRepeat: "no-repeat",
            backgroundSize: "cover",
            backgroundPosition: "center",
            willChange: "filter",
            backgroundImage: props => props.photo == undefined ? undefined : `url(${props.photo})`,

            [theme.breakpoints.down("sm")]: {
                "&.fullview": {
                    paddingTop: 0,
                    height: 50,
                    width: 50,
                },
            }
        },
        transparentOverhead: {
            position: "absolute", 
            top: 0,
            width: "100%", 
            height: `calc(${photoSize} / 2 + ${shadowSize})`,
            backgroundColor: "transparent",
            zIndex: 1,
        },
        whiteOverhead: {
            position: "absolute", 
            top: `calc(${photoSize} / 2 + ${shadowSize})`,
            bottom: 0,
            width: "100%", 
            borderRadius: "15px 15px 0 0",
            backgroundColor: "white",
            zIndex: -1,

            [theme.breakpoints.down("sm")]: {
                "&.fullview": {
                    top: 0,
                },
            }
        },
        contentBackground: {
            backgroundColor: "white",
            display: "flex",
            overflow: "hidden auto",
            flexGrow: 1,
        },
        contentContainer: {
            padding: "0 20px",
        },
        name: {
            paddingTop: "1.5rem",
            fontWeight: 500,
            fontSize: "1.5rem",
            textAlign: "center",
            position: "relative",
            zIndex: 1,
        },
        description: {
            textAlign: "center",
        },
        priceInfo: {
            display: "flex",
            flexDirection: "row",
            alignItems: "flex-start",
            justifyContent: "space-evenly"
        },
        priceTxt: {
            padding: "8px",
            fontSize: "1.5rem",
            fontWeight: 500,
        },
        quantityComponent: {
            display: "inline-flex",
            alignItems: "center",
        },
        quantityTxt: {
            margin: "0 10px",
            fontSize: "1.5rem",
            fontWeight: 500,
        },
        quantityBtn: {
            width: "30px",
            height: "30px",
            fill: props => props.primarycolor.hex,
        },
        addToCartBtn: {
            color: "white",
            border: 0,
            padding: "1rem",
            fontSize: "1.2rem",
            fontWeight: 500,
            width: "100%",
            marginTop: "1rem",
        },
        totalPriceTxt: {
            marginLeft: "12px",
        },
        closeBtn: {
            visibility: "collapse",
            position: "absolute",
            top: 20,
            right: 20,

            [theme.breakpoints.down("sm")]: {
                "&.fullview": {
                    visibility: "visible",
                    position: "unset",
                    top: undefined,
                    right: undefined,
                    marginTop: "1rem",
                    marginRight: "1rem",
                },
            }
        }
    })
});

interface Props {
    readonly menuItem: IItem | ICartItem | null;
    readonly onClose: () => void;
}
export const MenuItemDetailDialog = (props: Props) => {
    const theme = useQuiviTheme();

    const classes = useStyles({ 
        primarycolor: theme.primaryColor,
        photo: props.menuItem?.imageUrl,
    });
    const { t } = useTranslation();
    
    const channelContext = useChannelContext();
    const cartService = useCart();
    const pageMode = usePageMode();

    const handlers = useSwipeable({
        onSwipedUp: () => setIsSticky(true),
        onSwipedDown: () => {
            if(isSticky) {
                setIsSticky(false);
                return;
            }
            setIsOpen(false);
        },
        swipeDuration: 150,
        preventScrollOnSwipe: false,
        trackMouse: true,
    });
    const [isSticky, setIsSticky] = useState(false);
    const [itemsQty, setItemsQty] = useState(1);
    const [selectedModifiers, setSelectedModifiers] = useState<{
        [index: string]: ICartItem[];
    }>({});

    const availableModifiers = useMemo(() => {
        const map = new Map<string, IItemModifierGroup>();
        for(let item of props.menuItem?.modifiers ?? []) {
            map.set(item.id, item);
        }
        return map;
    }, [props.menuItem]);

    const [isOpen, setIsOpen] = useState(props.menuItem != null);
    
    const allowToOrder = useMemo(() => {
        const isAvailable = props.menuItem?.isAvailable == true;
        return props.menuItem != null && channelContext.features.ordering.isActive && isAvailable;
    }, [props.menuItem, channelContext.features.ordering]);

    const itemsQtyTotal = useMemo(() => {
        let extraPrice = 0;
        
        for(const k of Object.keys(selectedModifiers)) {
            for(const m of selectedModifiers[k]) {
                extraPrice += ItemsHelper.getItemPrice(m);
            }
        }

        const itemPrice = props.menuItem?.price ?? 0;
        return BigNumber(itemsQty).multipliedBy(BigNumber(itemPrice).plus(extraPrice)).toNumber();
    }, [props.menuItem, itemsQty, selectedModifiers]);

    //#region Effects
    useEffect(() => setIsOpen(props.menuItem != null), [props.menuItem]);
    useEffect(() => setItemsQty(props.menuItem != null && 'quantity' in props.menuItem ? props.menuItem.quantity : 1), [props.menuItem])
    useEffect(() => setSelectedModifiers(s => props.menuItem == null ? {} : s), [props.menuItem]);
    useEffect(() => setIsSticky(false), [isOpen])
    //#endregion

    const addToCart = () => {
        if(props.menuItem == null) {
            return;
        }

        const modifiers: ICartModifier[] = [];
        for(const groupId of Object.keys(selectedModifiers)) {
            modifiers.push({
                ...availableModifiers.get(groupId)!,
                selectedOptions: selectedModifiers[groupId],
            });
        }

        const item: ICartItem = {
            id: props.menuItem.id,
            name: props.menuItem.name,
            description: props.menuItem.description,
            price: props.menuItem.price,
            priceType: props.menuItem.priceType,
            imageUrl: props.menuItem.imageUrl,
            isAvailable: props.menuItem.isAvailable,
            quantity: itemsQty,
            modifiers: modifiers,
        };

        const isCartItem = 'quantity' in props.menuItem;
        if(isCartItem) {
            cartService.updateItem(props.menuItem, item);
        } else {
            cartService.addItem(item);
        }
        setIsOpen(false);
    }

    const duration = 0.3;
    return (
        <Dialog
            isOpen={isOpen}
            className={`${classes.dialog} ${isSticky ? "fullview" : ""}`}
            showCloseButton={!isSticky || pageMode == PageMode.Kiosk}
            onClose={props.onClose}
            style={{
                backgroundColor: "transparent",
                boxShadow: "unset",
                ...(isSticky ? {top: 0} : {})
            }}
        >
            <Box 
                className={classes.headerContainer}
                {...handlers}
            >
                <motion.div
                    layout
                    initial={false}
                    transition={{
                        duration: duration,
                    }}
                    className={`${classes.headerPivotContainer} ${isSticky ? "fullview" : ""}`}
                >
                    <motion.div
                        layout
                        initial={false}
                        transition={{
                            duration: duration,
                        }}
                        className={`${classes.photoContainer} ${isSticky ? "fullview" : ""}`}
                    >
                        <motion.div layout initial={false} transition={{duration: duration}} className={`${classes.photo} ${isSticky ? "fullview" : ""}`} />
                    </motion.div>
                    <motion.p
                        layout
                        initial={false}
                        transition={{
                            duration: duration,
                        }}
                        className={classes.name}
                    >
                        {props.menuItem?.name}
                    </motion.p>
                    <motion.div
                        layout
                        initial={false}
                        transition={{
                            duration: duration,
                        }}
                        className={`${classes.closeBtn} ${isSticky ? "fullview" : ""}`}
                    >
                        <IconButton aria-label="close" onClick={() => setIsOpen(false)}>
                            <CloseIcon />
                        </IconButton>
                    </motion.div>
                </motion.div>
                {
                    isSticky == false &&
                    <TextSection 
                        element={<p className={classes.description} />} 
                        text={props.menuItem?.description} 
                        maxLenght={60}
                        toggle={c => <b style={{cursor: "pointer"}}>{t(c ? "readMore" : "readLess")}</b>}
                    />
                }
                <Divider sx={{ width: "100%", margin: "0.75rem auto"}}/>
                <Box className={classes.priceInfo}>
                    <p className={classes.priceTxt}>{Formatter.price(itemsQtyTotal, "€")}</p>
                    {
                        allowToOrder &&
                        <Box className={classes.quantityContainer}>
                            <Box className={classes.quantityComponent}>
                                <Box sx={{opacity: itemsQty > 1 ? 1 : 0.5}}>
                                    <IconButton disabled={itemsQty == 1} onClick={() => setItemsQty(prev => Math.max(prev - 1, 1))} title={t("digitalMenu.decrementQty")}>
                                        <DashCircleIcon className={classes.quantityBtn} />
                                    </IconButton>
                                </Box>
                                <span className={classes.quantityTxt}>{itemsQty}</span>
                                <IconButton onClick={() => setItemsQty(prev => prev + 1)} title={t("digitalMenu.incrementQty")}>
                                    <PlusCircleIcon className={classes.quantityBtn} />
                                </IconButton>
                            </Box>
                        </Box>
                    }
                </Box>

                <Box onClick={() => setIsOpen(false)} className={classes.transparentOverhead} />
                <Box className={`${classes.whiteOverhead} ${isSticky ? "fullview" : ""}`} sx={{transition: `top ${duration}s`}} />
            </Box>
            <Box
                className={classes.contentBackground}
                onScroll={(el) => {
                    const target = el.currentTarget;
                    setIsSticky(p => {
                        if(p == false) {
                            return target.scrollTop > 0;
                        }

                        if(target.scrollHeight == target.clientHeight) {
                            return true;
                        }
                        
                        return target.scrollTop > 0;
                    });
                }}
            >
                <Box className={`container ${classes.contentContainer}`}>
                {
                    allowToOrder && props.menuItem != null &&
                    <MenuItemSelector item={props.menuItem} onModifiersChanged={(_, m) => setSelectedModifiers(m)} onAddToCart={addToCart}/>
                }
                </Box>
            </Box>
        </Dialog>
    );
}