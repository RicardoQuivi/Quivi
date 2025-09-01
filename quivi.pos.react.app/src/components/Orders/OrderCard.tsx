import { ReactNode, useMemo } from "react"
import { Alert, AlertTitle, Card, CardActionArea, CardActions, CardContent, CardHeader, Divider, keyframes, Paper, Skeleton, styled, Tooltip, Typography } from "@mui/material";
import { useTranslation } from "react-i18next";
import { Order } from "../../hooks/api/Dtos/orders/Order";
import { useDateHelper } from "../../helpers/dateHelper";
import { useOrderHelper } from "../../helpers/useOrderHelper";
import { useNow } from "../../hooks/useNow";
import { useChannelsQuery } from "../../hooks/queries/implementations/useChannelsQuery";
import { ConfigurableField, ConfigurableFieldType } from "../../hooks/api/Dtos/configurablefields/ConfigurableField";
import { useConfigurableFieldsQuery } from "../../hooks/queries/implementations/useConfigurableFieldsQuery";
import { OrderState } from "../../hooks/api/Dtos/orders/OrderState";
import { CardItemDetails } from "./CardItemDetails";
import ConfirmButton from "../Buttons/ConfirmButton";
import { CashCoinIcon, NotificationIcon, TakeAwayIcon } from "../../icons";
import { useMenuItemsQuery } from "../../hooks/queries/implementations/useMenuItemsQuery";
import { MenuItem } from "../../hooks/api/Dtos/menuitems/MenuItem";
import { useChannelProfilesQuery } from "../../hooks/queries/implementations/useChannelProfilesQuery";

const fadeIn = keyframes`
  from {
    opacity: 0;
  }
`;

const FadingTypography = styled(Typography)`
  animation: ${fadeIn} 1s infinite alternate;
`;

const StyledAlert = styled(Alert)(() => ({
    "& .MuiAlert-icon": {
        display: "flex",
        alignContent: "center",
        flexWrap: "wrap",

        marginTop: 0,
        marginBottom: 0,
        paddingTop: 0,
        paddingBottom: 0,
    },

    "& .MuiAlert-message": {
        padding: 0,
        flex: "1 1 auto"
    }
}))
interface OrderCardProps {
    readonly order?: Order;
    readonly onCardClicked?: (order: Order) => any;
    readonly onNextStateClicked?: (o: Order) => any;
    readonly onCompleteClicked?: (o: Order) => any;
}
export const OrderCard = (props: OrderCardProps) => {
    const { t, i18n } = useTranslation();
    const dateHelper = useDateHelper();
    const orderHelper = useOrderHelper();
    const now = useNow(1000);

    const channelQuery = useChannelsQuery(props.order == undefined ? undefined :{
        ids: [props.order.channelId],
        includeDeleted: true,
        page: 0,
        pageSize: 1,
    });
    const channel = useMemo(() => {
        if(channelQuery.data.length == 0) {
            return undefined;
        }
        return channelQuery.data[0];
    }, [channelQuery.data])

    const profileQuery = useChannelProfilesQuery(channel == undefined ? undefined :{
        ids: [channel.channelProfileId],
        page: 0,
        pageSize: 1,
    });

    const profile = useMemo(() => {
        if(profileQuery.data.length == 0) {
            return undefined;
        }
        return profileQuery.data[0];
    }, [profileQuery.data])

    const fieldsQuery = useConfigurableFieldsQuery(props.order == undefined  || props.order.fields.length == 0 ? undefined : {
        ids: props.order.fields.map(f => f.id),
        page: 0,
    })
    const fieldsMap = useMemo(() => {
        const map = new Map<string, ConfigurableField>();
        for(const f of fieldsQuery.data) {
            map.set(f.id, f);
        }
        return map;
    }, [fieldsQuery.data])

    const menuItemIds = useMemo(() => {
        const result = new Set<string>();
        if(props.order == undefined) {
            return Array.from(result.values());
        }

        for(const item of props.order.items) {
            result.add(item.menuItemId);
            for(const extra of item.extras) {
                result.add(extra.menuItemId);
            }
        }
        return Array.from(result.values());
    }, [props.order])

    const menuItemsQuery = useMenuItemsQuery(menuItemIds.length == 0 ? undefined : {
        ids: menuItemIds,
        page: 0,
    })
    const menuItemsMap = useMemo(() => {
        const result = new Map<string, MenuItem>();
        for(const item of menuItemsQuery.data) {
            result.set(item.id, item);
        }
        return result;
    }, [menuItemsQuery.data])

    const isDelayed = props.order == undefined ? false : orderHelper.isOrderDelayed(props.order.lastModified);

    const getTitle = () => {
        if(props.order == undefined) {
            return <Skeleton animation="wave" />
        }

        const additionalInfo: ReactNode[] = [];
        if(props.order.fields.length > 0) {
            additionalInfo.push(<Tooltip title={t("WebDashboard.AdditionalNotes")} key="notification">
                <NotificationIcon />
            </Tooltip>)
        }
        if(props.order.isTakeAway) {
            additionalInfo.push(<Tooltip title={t("Resources.Takeaway")} key="takeaway">
                <TakeAwayIcon />
            </Tooltip>)
        }
        if(props.order.items.every(i => i.isPaid)) {
            additionalInfo.push(<Tooltip title={t("WebDashboard.PrePaid")} key="prepaid">
                <CashCoinIcon />
            </Tooltip>)
        }

        return <div style={{display: "flex", flexDirection: "row", width: "100%"}}>
            <b style={{flex: "0 0 auto"}}>{props.order.sequenceNumber}</b>
            <div style={{flex: "1 1 auto", display: "flex", justifyContent: "flex-end", gap: "0.5rem"}}>{additionalInfo}</div>
        </div>;
    }

    const getSubTitle = () => {
        if(profile == undefined || channel == undefined) {
            return <Skeleton animation="wave" />
        }
        return `${profile.name} ${channel.name}`;
    }

    const getTimeBadge = () => {
        if(props.order == undefined) {
            return <Skeleton animation="wave" />
        }
        
        const order = props.order;
        if([OrderState.Scheduled, OrderState.ScheduledRequested].includes(order.state) && order.scheduledTo != undefined) {
            const date = dateHelper.toDate(order.scheduledTo);
            const day = date.getDate();

            const time = new Intl.DateTimeFormat(i18n.language, {
                hour: '2-digit',
                minute: '2-digit',
                hour12: false, // use 24-hour format
            }).format(date);

            let month = new Intl.DateTimeFormat(i18n.language, {
                month: 'long',
            }).format(date);
            month = `${month[0].toUpperCase()}${month.substring(1)}`;

            return (
                <StyledAlert variant="outlined" severity="info">
                    <time className="timeago">
                        {t("WebDashboard.PickUpAt", { day: day, month: month, time: time})}
                    </time>
                </StyledAlert>
            )
        }
        
        return (
            <StyledAlert variant="outlined" severity={isDelayed ? "warning": "info"}>
                {
                    isDelayed
                    ?
                    <FadingTypography variant="subtitle1">
                        {dateHelper.getTimeAgo(now, order.lastModified)}
                    </FadingTypography>
                    :
                    <Typography variant="subtitle1">
                        {dateHelper.getTimeAgo(now, order.lastModified)}
                    </Typography>
                }
            </StyledAlert>
        )
    }

    const showNextStateButton = (order: Order | undefined, state: OrderState) => {
        if(order == undefined) {
            return false;
        }
        
        switch (state) {
            case OrderState.Scheduled:
                return order.state == OrderState.ScheduledRequested;
            case OrderState.Processing:
                return [OrderState.PendingApproval, OrderState.Accepted, OrderState.Scheduled].includes(order.state);
            case OrderState.Completed:
                return [OrderState.PendingApproval, OrderState.Accepted, OrderState.Scheduled].includes(order.state) == false;
            default:
                return false;
        }
    }

    return <Paper elevation={16} style={{height: "100%"}}>
        <Card onClick={() => props.order != undefined && props.onCardClicked?.(props.order)} sx={{
            border: isDelayed ? "2px solid #d26806" : undefined,
            height: "100%",
            display: "flex",
            flexDirection: "column",
            cursor: "pointer",
        }}>
            <CardActionArea onClick={() => props.order != undefined && props.onCardClicked?.(props.order)} style={{flex: "0 0 auto"}}>
                <CardHeader
                    title={getTitle()}
                    subheader={getSubTitle()}
                    slotProps={{
                        title: {
                            fontSize: "1rem"
                        }
                    }}
                />
            </CardActionArea>
            <Divider style={{flex: "0 0 auto"}}/>
            <CardContent sx={{ paddingTop: "0.5rem", paddingBottom: "0 !important"}} style={{flex: "1 1 auto"}}>
                <CardItemDetails 
                    items={props.order == undefined ? undefined : props.order.items} 

                    getId={item => item.id}
                    getSubItems={item => item.extras.map(e => ({
                        ...e,
                        id: "",
                        i: e.menuItemId,
                        isPaid: item.isPaid,
                        discountPercentage: item.discountPercentage,
                        lastModified: item.lastModified,
                        extras: [],
                    }))}
                    getQuantity={item => item.quantity}
                    renderName={item => {
                        const menuItem = menuItemsMap.get(item.menuItemId);
                        if(menuItem == undefined) {
                            return <Skeleton animation="wave" />
                        }
                        return menuItem.name;
                    }}
                />
            </CardContent>
            {
                props.order?.fields.map(f => {
                    const configurableField = fieldsMap.get(f.id);
                    return (
                        <CardContent key={f.id} sx={{ marginTop: "0.5rem", paddingTop: "0", paddingBottom: "0.5rem !important"}} >
                            <StyledAlert variant="outlined" severity="info">
                                <AlertTitle sx={{textAlign: "left"}}>{configurableField == undefined ? <Skeleton animation="wave" width="100%" /> : configurableField.name}</AlertTitle>
                                {
                                    configurableField == undefined
                                    ? 
                                        <Skeleton animation="wave" width="100%" />
                                    :
                                    (
                                        configurableField.type == ConfigurableFieldType.Check
                                        ?
                                        t(`${["", "0", "false"].includes(f.value.toLowerCase()) ? "no" : "yes"}`)
                                        :
                                        f.value
                                    )
                                }
                            </StyledAlert>
                        </CardContent>
                    );
                })
            }
            <CardContent sx={{ marginTop: "0.5rem", paddingTop: "0", paddingBottom: "0.5rem !important"}} style={{flex: "0 0 auto"}}>
                {getTimeBadge()}
            </CardContent>
            <CardActions disableSpacing style={{flex: "0 0 auto"}}>
                {
                    showNextStateButton(props.order, OrderState.Processing) &&
                    <ConfirmButton primaryButton onAction={() => props.order != undefined && props.onNextStateClicked?.(props.order)} confirmText={`${t("confirm")}?`} style={{width: "100%"}}>
                        {
                            props.order?.state == OrderState.Scheduled
                            ?
                                t("sendToInProgress")
                            :
                                t("accept")
                        }
                    </ConfirmButton>
                }
                {
                    showNextStateButton(props.order, OrderState.Completed) &&
                    <ConfirmButton onAction={() => props.order != undefined && props.onCompleteClicked?.(props.order)} confirmText={`${t("confirm")}?`} style={{width: "100%"}}>
                        {t("ready")}
                    </ConfirmButton>
                }
                {
                    showNextStateButton(props.order, OrderState.Scheduled) &&
                    <ConfirmButton primaryButton onAction={() => props.order != undefined && props.onNextStateClicked?.(props.order)} confirmText={`${t("confirm")}?`} style={{width: "100%"}}>
                        {t("accept")}
                    </ConfirmButton>
                }
            </CardActions>
        </Card>
    </Paper>
}