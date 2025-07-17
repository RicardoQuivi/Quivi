import { useMemo } from "react";
import { Trans, useTranslation } from "react-i18next";
import { useQuiviTheme, type IColor } from "../../hooks/theme/useQuiviTheme";
import { makeStyles } from "@mui/styles";
import { AvatarGroup, Chip, Skeleton, type Theme } from "@mui/material";
import type { Order } from "../../hooks/api/Dtos/orders/Order";
import type { MenuItem } from "../../hooks/api/Dtos/menuItems/MenuItem";
import { useMenuItemsQuery } from "../../hooks/queries/implementations/useMenuItemsQuery";
import { useChannelContext } from "../../context/AppContextProvider";
import { OrderState } from "../../hooks/api/Dtos/orders/OrderState";
import { AvatarImage } from "../../components/Avatars/AvatarImage";
import { Formatter } from "../../helpers/formatter";

interface StyleProps {
    readonly primarycolor: IColor,
    readonly padding?: string;
}

const useStyles = makeStyles<Theme, StyleProps>(() => ({
    container: {
        width: "100%",
        display: "flex",
        flexDirection: "row",
        alignItems: "center",
    },
    photo: {
        height: "100%",
        minHeight: "50px",
        objectFit: "contain",
        borderRadius: props => props.padding ? "5px" : "5px 0 0 5px",
        marginLeft: props => props.padding ? props.padding : "0",
    },
    infoContainer: {
        display: "flex",
        flexDirection: "column",
        padding: "10px",
        paddingLeft: "20px",
        flex: "1 1 auto",
    },
    descriptionContainer: {
        display: "flex",
        flexDirection: "column",
        fontWeight: 600,
        alignItems: "center",
    },
    nameContainer: {
        display: "inline-flex",
        fontSize: "1rem",
        fontWeight: 400,
    },
    nameTxt: {
        overflow: "hidden",
        textOverflow: "ellipsis",
        whiteSpace: "nowrap",
    },
    dateTxt: {
        fontWeight: 400,
        fontSize: "0.85rem",
        color: "#9D9F9E"
    },
    price: {
        height: "100%",
        marginLeft: "1rem",
        marginRight: props => props.padding ? props.padding : "0",
        fontWeight: 600,
        color: "black",
        textAlign: "center",
    },
    unavailable: {
        fontSize: "small",
    }
}));

interface Props {
    readonly model?: Order;
}

const getItemsMap = (items: MenuItem[]): Map<string, MenuItem> => {
    const result = new Map<string, MenuItem>();
    for(const item of items) {
        result.set(item.id, item);
    }
    return result;
}

export const OrderRow: React.FC<Props> = (props: Props) => {
    const { t } = useTranslation();
    const channelContext = useChannelContext();

    const theme = useQuiviTheme();
    const classes = useStyles({ primarycolor: theme.primaryColor });

    const menuItemsQuery = useMenuItemsQuery(props.model == undefined ? undefined : {
        ids: props.model.items.map(i => i.id),
        ignoreCalendarAvailability: true,
        channelId: channelContext.channelId,
        page: 0,
    })
    const itemsMap = useMemo(() => getItemsMap(menuItemsQuery.data), [menuItemsQuery.data])

    const getTotal = (order: Order): number => {
        let total = 0;
        order.items.forEach(item => {
            const modifiersPrices = (item.modifiers ?? []).map(m => m.selectedOptions)
                                                    .reduce((r, o) => [...r, ...o], [])
                                                    .reduce((r, o) => r + o.amount * o.quantity, 0);
            total += (item.amount + modifiersPrices) * item.quantity;
        });
        order.extraCosts?.forEach(item => total += item.amount);
        return total;
    }

    const getTotalItems = (order: Order): number => order.items.reduce((r, item) => r + item.quantity, 0);
    const getDescription = (order: Order) => {
        const totalItems = getTotalItems(order);
        return <p className={classes.nameTxt}>
            <Trans
                t={t}
                i18nKey={totalItems == 1 ? "orders.singleItem" : "orders.totalItems"}
                values={{
                    items: getTotalItems(order)
                }}
                components={{
                    bold:  <b />,
                }}
            />
        </p>
    }
    const getOrderState = (order: Order) => {
        if(order.state == OrderState.Requested) {
            return t("orderAndPayResult.reviewing");
        }
        if(order.state == OrderState.Completed) {
            return t("orderAndPayResult.completed");
        }
        if(order.state == OrderState.Rejected) {
            return t("orderAndPayResult.orderCanceled")
        }
        return t("orderAndPayResult.preparing");
    }

    const orderState = props.model == undefined ? undefined : getOrderState(props.model)
    return <>
        <div className={classes.container}>
            <div className={classes.descriptionContainer}>
                {
                    props.model == undefined
                    ?
                    <Skeleton variant="text" animation="wave" height="0.75rem" width="20%" />
                    :
                    <span>{t("orders.order")} {props.model.sequenceNumber}</span>
                }
                <AvatarGroup className={classes.photo} >
                    {
                        props.model == undefined
                        ?
                        []
                        :
                        props.model.items.map(item => {
                            const digitalItem = itemsMap.get(item.id);
                            return <AvatarImage key={item.id} src={digitalItem?.imageUrl} name={item.name} />
                        })
                    }
                </AvatarGroup>
            </div>
            <div className={classes.infoContainer}>
                <div className={classes.nameContainer}>
                    {
                        props.model == null
                        ?
                            <Skeleton variant="text" animation="wave" height="1.5rem" width="70%" />
                        :
                            getDescription(props.model)

                    } 
                </div>
                <p className={classes.dateTxt}>
                    {
                        props.model == null
                        ?
                            <Skeleton variant="text" animation="wave" height="1.5rem" width="20%" />
                        :
                            <>
                                {/* TODO: Add time ago */}
                                {/* <Moment local locale="pt" format="DD MMM">{props.model.lastModified}</Moment>
                                &nbsp;|&nbsp;
                                <Moment local format="HH:mm">{props.model.lastModified}</Moment> */}
                            </>
                    }
                </p>
            </div>
            {

                props.model != undefined && orderState != undefined && 
                <Chip 
                    variant="outlined" 
                    label={orderState} 
                    style={{
                        borderColor: theme.primaryColor.hex,
                        color: theme.primaryColor.hex,
                    }} 
                />
            }
            <p className={classes.price}>
            {
                props.model == null
                ?
                    <Skeleton variant="text" animation="wave" height="1.5rem" width="80px" />
                :
                    <span>{Formatter.price(getTotal(props.model), "€")}</span>
            }
            </p>
        </div>
    </>;
}