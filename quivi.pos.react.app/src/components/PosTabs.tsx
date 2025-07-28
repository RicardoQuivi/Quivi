import { Badge, BottomNavigation, BottomNavigationAction, Grid, Paper, Tab, Tabs } from "@mui/material";
import { useTranslation } from "react-i18next";
import { useChannelsQuery } from "../hooks/queries/implementations/useChannelsQuery";
import { CartIcon, ListIcon, OrdersIcon, QrCodeIcon } from "../icons";
import { SortDirection } from "../hooks/api/Dtos/SortableRequest";
import { useOrdersQuery } from "../hooks/queries/implementations/useOrdersQuery";
import { OrderState } from "../hooks/api/Dtos/orders/OrderState";
import { usePreparationGroupsQuery } from "../hooks/queries/implementations/usePreparationGroupsQuery";
import { usePosSession } from "../context/pos/PosSessionContextProvider";
import { useEffect } from "react";
import { useAllowedActions } from "../hooks/pos/useAllowedActions";

const availableTabs = ["items", "channels", "orders"]

export enum ActiveTab {
    ItemSelection,
    ChannelSelection,
    Ordering,
    SessionOverview,
    Notifications,
}

interface Props {
    readonly isMobile: boolean;
    readonly tab: ActiveTab;
    readonly onTabIndexChanged: (tab: ActiveTab) => any;
    readonly hasChannelsWithSessions: boolean;
    readonly localId: string | undefined;
}
export const PosTabs = (props: Props) => {
    const { t } = useTranslation();
    const pos = usePosSession();

    const channelQuery = useChannelsQuery({
        ids: [pos.cartSession.channelId],
        page: 0,
        pageSize: 1,
        allowsSessionsOnly: false,
        includeDeleted: true,
    })

    const pendingApprovalOrdersQuery = useOrdersQuery({
        page: 0,
        pageSize: 0,
        states: [OrderState.PendingApproval],
        sortDirection: SortDirection.Desc,
    });
    const preparationGroupsQuery = usePreparationGroupsQuery({
        locationId: props.localId,
        page: 0,
        pageSize: 0,
    });

    const totalPendingOrders = pendingApprovalOrdersQuery.totalItems + preparationGroupsQuery.totalItems;
    if(pos.permissions.data.canViewSessions === false) {
        return <></>
    }
    
    if(props.isMobile == false) {
        return <>
            <Grid container gap={2}>
                <Grid size="grow" style={{flex: "1 1 auto"}}>
                    <Paper elevation={16}>
                        <Tabs
                            value={
                                    props.tab == ActiveTab.Notifications
                                    ?
                                    false
                                    :
                                    (
                                        [ActiveTab.ItemSelection, ActiveTab.ChannelSelection].includes(props.tab)
                                        ? 
                                        (
                                            props.hasChannelsWithSessions == false
                                            ?
                                            false
                                            :
                                            (
                                                pos.permissions.data.allowsAddingItems === false && props.tab == ActiveTab.ItemSelection
                                                ?
                                                false
                                                :
                                                props.tab
                                            )
                                        ) 
                                        :
                                        props.tab
                                    )
                                }
                            onChange={(_, value: number) => props.onTabIndexChanged(value)}
                            indicatorColor="primary"
                            textColor="primary"
                            variant="fullWidth"
                        >
                            {
                                props.hasChannelsWithSessions &&
                                [
                                    ...
                                    (
                                        pos.permissions.data.allowsAddingItems === true
                                        ?
                                            [<Tab key={"add-items"} label={<span>{t(availableTabs[0])}</span>} value={ActiveTab.ItemSelection} /> ]
                                        :
                                            []
                                    ),
                                    <Tab key={"qrcode-selection"} label={<span>{t(availableTabs[1])}</span>} value={ActiveTab.ChannelSelection} />
                                ]
                            }

                            <Tab value={ActiveTab.Ordering}
                                label={<Badge badgeContent={totalPendingOrders} color="primary" variant="standard" overlap="rectangular">
                                        <span>{t(availableTabs[2])}</span>
                                    </Badge>}
                            />
                        </Tabs>
                    </Paper>
                </Grid>
            </Grid>
            <TabValidator channelId={pos.cartSession.channelId} tab={props.tab} onTabCanProceed={props.onTabIndexChanged} />
        </>
    }

    return <>
        <BottomNavigation 
            sx={{ 
                width: "100%", 
                flex: "0 0 auto", 
                bgcolor: "#FF3F01",
                height: "auto",
                padding: "0.75rem 0",

                "& .MuiButtonBase-root": {
                    color: "rgba(255, 255, 255, 0.7)",
                    padding: 0,
                },
                
                "& .MuiButtonBase-root.Mui-selected": {
                    color: "rgba(255, 255, 255, 1)",
                    fontWeight: 800,

                    "& .MuiBottomNavigationAction-label": {
                        fontSize: "0.75rem"
                    }
                }
            }}
            showLabels
            value={props.tab}
            onChange={(_, v) => props.onTabIndexChanged(v as ActiveTab)}
        >
            {
                pos.permissions.data.allowsAddingItems === true &&
                <BottomNavigationAction label={t(availableTabs[0])} value={ActiveTab.ItemSelection} icon={<ListIcon />} />
            }
            <BottomNavigationAction label={t(availableTabs[1])} value={ActiveTab.ChannelSelection} icon={<QrCodeIcon />} />
            <BottomNavigationAction label={t(availableTabs[2])} value={ActiveTab.Ordering} icon={<Badge badgeContent={totalPendingOrders} color="primary" variant="standard" overlap="rectangular" sx={{
                                                                                                    "& .MuiBadge-badge": {
                                                                                                        backgroundColor: '#6c757d',
                                                                                                        color: '#fff',
                                                                                                    }
                                                                                                }}>
                                                                                                    <OrdersIcon />
                                                                                                </Badge>} 
                                                            />
            {
                channelQuery.data.length > 0 &&
                <BottomNavigationAction label={channelQuery.data[0].name} 
                                        value={ActiveTab.SessionOverview}
                                        icon={<Badge badgeContent={pos.cartSession.items.reduce((r, c) => r + c.quantity, 0)} color="primary" variant="standard" overlap="rectangular" sx={{
                                                    "& .MuiBadge-badge": {
                                                        backgroundColor: '#6c757d',
                                                        color: '#fff',
                                                    }
                                                }}>
                                                    <CartIcon />
                                                </Badge>}
                />
            }
        </BottomNavigation>
        <TabValidator channelId={pos.cartSession.channelId} tab={props.tab} onTabCanProceed={props.onTabIndexChanged} />
    </>
}

interface TabValidatorProps {
    readonly channelId: string;
    readonly tab: ActiveTab;
    readonly onTabCanProceed: (t: ActiveTab) => void;
}
const TabValidator = (props: TabValidatorProps) => {
    const permissionsQuery = useAllowedActions(!props.channelId ? undefined : props.channelId);

    const prepaidOrderingChannelsQuery = useChannelsQuery({
        pageSize: 0,
        page: 0,
        allowsPrePaidOrderingOnly: true,
    })
    const postpaidOrderingChannelsQuery = useChannelsQuery({
        pageSize: 0,
        page: 0,
        allowsPostPaidOrderingOnly: true,
    })

    useEffect(() => {
        if(permissionsQuery.isFirstLoading) {
            return;
        }
        
        if(prepaidOrderingChannelsQuery.isFirstLoading) {
            return;
        }

        if(postpaidOrderingChannelsQuery.isFirstLoading) {
            return;
        }

        const permissions = permissionsQuery.data;
        const hasSessions = permissions.canViewSessions;
        const hasOrdering = prepaidOrderingChannelsQuery.totalItems > 0 || postpaidOrderingChannelsQuery.totalItems > 0;
        
        let newTab = props.tab;
        if(hasSessions == false && [ActiveTab.ItemSelection, ActiveTab.SessionOverview, ActiveTab.ChannelSelection].includes(newTab)) {
            newTab = hasOrdering ? ActiveTab.Ordering : ActiveTab.Notifications;
        } else if(newTab == ActiveTab.ItemSelection && permissions.allowsAddingItems == false) {
            newTab = ActiveTab.ChannelSelection;
        }
        
        props.onTabCanProceed(newTab);
    }, [props.channelId, props.tab, props.onTabCanProceed])

    return <></>
}