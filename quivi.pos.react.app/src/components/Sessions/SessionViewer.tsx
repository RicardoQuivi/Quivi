import React, { useEffect, useMemo, useState } from "react"
import { BottomNavigation, BottomNavigationAction, Box, Button, ButtonBase, ButtonGroup, Card, CardHeader, Chip, CircularProgress, Collapse, Divider, IconButton, List, ListItem, ListItemAvatar, ListItemText, Paper, Skeleton, Stack, Tooltip, Typography, styled } from "@mui/material"
import { useTranslation } from "react-i18next";
import { usePosSession } from "../../context/pos/PosSessionContextProvider";
import { useChannelsQuery } from "../../hooks/queries/implementations/useChannelsQuery";
import { useOrdersQuery } from "../../hooks/queries/implementations/useOrdersQuery";
import { OrderState } from "../../hooks/api/Dtos/orders/OrderState";
import { SortDirection } from "../../hooks/api/Dtos/SortableRequest";
import CurrencySpan from "../Currency/CurrencySpan";
import { Channel } from "../../hooks/api/Dtos/channels/Channel";
import { CheckIcon, CollapseIcon, ExclamationIcon, ExpandIcon, InfoIcon, MinusIcon, PencilIcon, PlusIcon, PlusMinusIcon, SwitchIcon, UncheckIcon } from "../../icons";
import { Order } from "../../hooks/api/Dtos/orders/Order";
import { useChannelProfilesQuery } from "../../hooks/queries/implementations/useChannelProfilesQuery";
import { useNow } from "../../hooks/useNow";
import { useDateHelper } from "../../helpers/dateHelper";
import { Currency } from "../../helpers/currencyHelper";
import { SessionButtons } from "./SessionButtons";
import { BaseSessionItem, SessionItem } from "../../hooks/api/Dtos/sessions/SessionItem";
import { useMenuItemsQuery } from "../../hooks/queries/implementations/useMenuItemsQuery";
import { MenuItem } from "../../hooks/api/Dtos/menuitems/MenuItem";
import { PickItemQuantityModal } from "./PickItemQuantityModal";
import { EditItemPriceModel, EditSessionItemModal } from "./EditSessionItemModal";
import { Items } from "../../helpers/itemsHelpers";
import { useToast } from "../../context/ToastProvider";
import { useOrderMutator } from "../../hooks/mutators/useOrderMutator";
import { useActionAwaiter } from "../../hooks/useActionAwaiter";
import { CollectionFunctions } from "../../helpers/collectionsHelper";

const StyleBottomNavigationAction = styled(BottomNavigationAction)(({ }) => ({
    transition: "background-color 0.5s ease",
    borderRadius: "20px",
    fontSize: "1.5rem",
    
    "& .MuiBottomNavigationAction-label": {
        fontSize: "0.9rem",
    },

    "&.Mui-selected .MuiBottomNavigationAction-label": {
        fontSize: "0.9rem",
    },

    "&.active": {
        backgroundColor: "rgba(25, 118, 210, 0.08);",
    }
}));

const recentThreshold = 2500;
const getItemsState = (allItems: SessionItem[], stringToDate: (input: string) => Date) => {
    const now = new Date();
    const paidItems: SessionItem[] = [];
    const unpaidItems: SessionItem[] = [];
    const recentlyPaidChangedItems = new Set<string>();
    const recentlyUnpaidChangedItems = new Set<string>();
    let nextUpdateAt: number | undefined = undefined;

    for(const item of allItems) {
        let aux = item.isPaid ? paidItems : unpaidItems;
        aux.push(item);

        const itemChangedAt = stringToDate(item.lastModified).getTime();
        const threshold = itemChangedAt + recentThreshold;
        if(itemChangedAt + recentThreshold > now.getTime()) {
            let aux2 = item.isPaid ? recentlyPaidChangedItems : recentlyUnpaidChangedItems;
            aux2.add(item.id);
            
            if(nextUpdateAt == undefined || threshold < nextUpdateAt) {
                nextUpdateAt = threshold;
            }
        }
    }

    return {
        allItems: allItems,
        paidItems: paidItems,
        unpaidItems: unpaidItems,
        recentlyPaidChangedItems: recentlyPaidChangedItems,
        recentlyUnpaidChangedItems: recentlyUnpaidChangedItems,
        nextUpdateAt: nextUpdateAt,
    }
}

interface Props {
    readonly onTransferSessionClicked: (channel: Channel) => any;
    readonly onEditItem: (item: EditItemPriceModel) => any;
    readonly onSessionAdditionalInfoClicked?: () => any;
    readonly localId: string | undefined;
}
export const SessionViewer: React.FC<Props> = ({
    onTransferSessionClicked,
    onEditItem,
    onSessionAdditionalInfoClicked,
    localId,
}) => {
    const { t } = useTranslation();
    const dateHelper = useDateHelper();
    const pos = usePosSession();

    const [editItem, setEditItem] = useState<SessionItem>();
    const [pickItemQuantity, setPickItemQuantity] = useState<SessionItem>();

    const [itemsState, setItemsState] = useState(getItemsState(pos.cartSession.items, dateHelper.toDate));
    const channelsQuery = useChannelsQuery(!pos.cartSession.channelId ? undefined : {
        ids: [pos.cartSession.channelId],
        allowsSessionsOnly: true,
        includeDeleted: true,
        page: 0,
        pageSize: 1,
    })
    const channel = useMemo(() => channelsQuery.data.length == 0 ? undefined : channelsQuery.data[0], [channelsQuery.data]);
    
    const profilesQuery = useChannelProfilesQuery(channel == undefined ? undefined :{
        ids: [channel.channelProfileId],
        page: 0,
    });
    const profile = useMemo(() => profilesQuery.data.length == 0 ? undefined : profilesQuery.data[0], [profilesQuery.data]);


    const pendingOrdersQuery = useOrdersQuery(!pos.cartSession.channelId ? undefined : {
        channelIds: [pos.cartSession.channelId],
        states: [OrderState.PendingApproval],
        sortDirection: SortDirection.Desc,
        page: 0,
    })

    const itemsIds = useMemo(() => {
        const set = new Set<string>();
        for(const item of pos.cartSession.items) {
            set.add(item.menuItemId);
            for(const e of item.extras) {
                set.add(e.menuItemId);
            }
        }

        for(const order of pendingOrdersQuery.data) {
            for(const item of order.items) {
                set.add(item.menuItemId);
                for(const e of item.extras) {
                    set.add(e.menuItemId);
                }
            }
        }

        return Array.from(set.values());
    }, [pos.cartSession.items, pendingOrdersQuery.data])

    const itemsQuery = useMenuItemsQuery(itemsIds.length == 0 ? undefined : {
        ids: itemsIds,
        page: 0,
    })
    const itemsMap = useMemo(() => CollectionFunctions.toMap(itemsQuery.data, item => item.id), [itemsQuery.data]);
    
    const [itemStatusFilter, setItemStatusFilter] = useState<boolean | undefined>(false);

    useEffect(() => setItemsState(p => p.allItems == pos.cartSession.items ? p : getItemsState(pos.cartSession.items, dateHelper.toDate)), [pos.cartSession]);
    useEffect(() => {
        if(itemsState.nextUpdateAt == undefined) {
            return;
        }

        const now = new Date().getTime();
        const awaitTime = itemsState.nextUpdateAt - now;
        if(awaitTime > 0) {
            const timeout = setTimeout(() => setItemsState(p => getItemsState(p.allItems, dateHelper.toDate)), awaitTime);
            return () => clearTimeout(timeout);
        } else {
            setItemsState(p => getItemsState(p.allItems, dateHelper.toDate))
        }
    }, [itemsState])

    const changeQuantity = (item: SessionItem, newQuantity: number) => {
        const change = newQuantity - item.quantity;
        if(change > 0) {
            pos.cartSession.addItem(item, change)
        } else {
            pos.cartSession.removeItem(item, -1 * change, item.discountPercentage);
        }
    }

    const canAddItems = pos.permissions.data.allowsAddingItems == true;
    const canRemoveItems = pos.permissions.data.allowsRemovingItems == true;
    const canApplyDiscounts = pos.permissions.data.applyDiscounts == true;
    const allowsInvoiceEscPosPrinting = pos.permissions.data.allowsInvoiceEscPosPrinting == true;

    const sessionClosedRecently = pos.cartSession.closedAt != null &&  pos.cartSession.closedAt.getTime() + recentThreshold > new Date().getTime();

    return <Box
        sx={{
            display: "flex",
            flexDirection: "column",
            height: "100%"
        }}
    >
        <Paper
            elevation={16}
            sx={{
                flex: 1,
                marginBottom: {
                    xs: 0,
                    sm: "1rem",
                },
                display: "flex", flexDirection: "column",
                overflow: "hidden auto",
            }}
        >
            <Box
                sx={{
                    height: "100%",
                    display: "flex",
                    flexDirection: "column",
                }}
            >
                {
                    channel == undefined || profile == undefined
                    ?
                    <Skeleton animation="wave" sx={{ width: "96%", alignSelf: "center" }}/>
                    :
                    <Box
                        sx={{
                            display: "flex",
                            flexDirection: "row",
                            justifyContent: "space-between",
                            alignItems: "center",
                            height: "48px",
                        }}
                    >
                        {
                            onSessionAdditionalInfoClicked != undefined &&
                            !!pos.cartSession.sessionId &&
                            pos.cartSession.closedAt == undefined &&
                            <Tooltip title={t("sessionInformation")}>
                                <IconButton size="large" onClick={onSessionAdditionalInfoClicked}>
                                    <InfoIcon height={18} width={18} />
                                </IconButton>
                            </Tooltip>
                        }
                        <Typography variant="h6" sx={{textAlign: "center", width: "100%"}}>
                            {profile.name} {channel.name}
                        </Typography>
                        {
                            canAddItems && canRemoveItems &&
                            <Tooltip title={t("transferSession")}>
                                <IconButton size="large" disabled={!!pos.cartSession.isSyncing} onClick={() => onTransferSessionClicked(channelsQuery.data[0])}>
                                    <SwitchIcon height={18} width={18} />
                                </IconButton>
                            </Tooltip>
                        }
                    </Box>
                }
                <Divider variant="fullWidth" sx={{flex: "0 0 auto"}}/>
                <Box
                    sx={{
                        flex: "1 1 auto",
                        overflow: "hidden auto",
                    }}
                >
                    {
                        pos.cartSession.items.length == 0 && pendingOrdersQuery.data.length == 0
                        ?
                        <Box
                            sx={{
                                height: "100%",
                                display: "flex",
                                flexDirection: "column",
                                alignItems: "center",
                                justifyContent: "center",
                            }}
                        >
                            <p style={{ textAlign: "center", fontSize: "16px" }}>{ t("session.empty")}</p>
                        </Box>
                        :
                        <List sx={{ bgcolor: 'background.paper'}}>
                            {
                                itemStatusFilter == false &&
                                pendingOrdersQuery.data.map(item =>
                                    <OrderItemComponent 
                                        key={item.id}
                                        order={item}
                                        itemsMap={itemsMap}
                                    />
                                )
                            }
                            {
                                 pos.cartSession.items.filter(item => itemStatusFilter == undefined || item.isPaid == itemStatusFilter).map((item, index) => (
                                    <SessionItemComponent
                                        key={item.id}
                                        showBackground={index % 2 == 1}
                                        item={item}
                                        itemStatusFilter={itemStatusFilter}
                                        canAdd={canAddItems && (canApplyDiscounts == true || item.discountPercentage == 0)}
                                        canRemove={canRemoveItems}
                                        onClickEdit={canAddItems && canRemoveItems && canApplyDiscounts ? setEditItem : undefined}
                                        onPickQuantity={canAddItems && (canApplyDiscounts == true || item.discountPercentage == 0) || canRemoveItems ? setPickItemQuantity : undefined}
                                        recentlyChanged={itemsState.recentlyPaidChangedItems.has(item.id) || itemsState.recentlyUnpaidChangedItems.has(item.id)}
                                        itemsMap={itemsMap}
                                    />
                                ))
                            }
                        </List>
                    }
                </Box>
                <BottomNavigation
                    sx={{
                        mt: "1rem",
                        mb: "0.5rem",
                    }}
                    showLabels
                    value={itemStatusFilter}
                    onChange={(_, newValue) => setItemStatusFilter(newValue)}
                >
                    <StyleBottomNavigationAction
                        value={false}
                        label={t("unpaid")}
                        className={itemsState.recentlyUnpaidChangedItems.size > 0 || sessionClosedRecently ? "active" : ""}
                        icon={<CurrencySpan value={Items.getTotalPrice(itemsState.unpaidItems)} />}
                    />
                    <StyleBottomNavigationAction 
                        value={true}
                        label={t("paid")}
                        className={itemsState.recentlyPaidChangedItems.size > 0 || sessionClosedRecently ? "active" : ""}
                        icon={<CurrencySpan value={Items.getTotalPrice(itemsState.paidItems)} />}
                    />
                    <StyleBottomNavigationAction
                        value={null}
                        label={t("total")}
                        className={itemsState.recentlyPaidChangedItems.size > 0 || itemsState.recentlyUnpaidChangedItems.size > 0 || sessionClosedRecently ? "active" : ""}
                        icon={<CurrencySpan value={Items.getTotalPrice(itemsState.allItems)} />}
                    />
                </BottomNavigation>
            </Box>
            <EditSessionItemModal
                item={editItem}
                onClose={() => setEditItem(undefined)}
                onSubmit={onEditItem}
            />
            <PickItemQuantityModal
                item={pickItemQuantity}
                canAdd={canAddItems}
                canApplyDiscounts={canApplyDiscounts}
                canRemove={canRemoveItems}
                onClose={() => setPickItemQuantity(undefined)}
                onSubmit={changeQuantity}
                itemsMap={itemsMap}
            />
        </Paper>

        <SessionButtons
            canPay={pos.permissions.data.allowsPayments}
            canAddItems={canAddItems}
            canRemoveItems={canRemoveItems}
            canApplyDiscounts={canApplyDiscounts}
            allowsInvoiceEscPosPrinting={allowsInvoiceEscPosPrinting}
            localId={localId}
        />
    </Box>;
}

const SessionItemComponent = (props : {
    readonly item: SessionItem | BaseSessionItem;
    readonly recentlyChanged: boolean;
    readonly itemStatusFilter?: boolean;
    readonly isModifier?: boolean;
    readonly showBackground?: boolean;
    readonly canAdd?: boolean;
    readonly canRemove?: boolean;
    readonly onPickQuantity?: (item: SessionItem) => any;
    readonly showQuantityBadge?: (quantity: number) => boolean;
    readonly onClickEdit?: (item: SessionItem) => void;
    readonly itemsMap: Map<string, MenuItem>;
}) => {
    const { t, i18n } = useTranslation();

    const [isOpen, setIsOpen] = useState(false);

    const isItemDisabled = (): boolean => props.itemStatusFilter == undefined && ('isPaid' in props.item && props.item.isPaid);
    const secondaryActions = (): React.ReactNode => {
        const buttons: React.ReactNode[] = [];

        if(props.itemStatusFilter == false) {
            if('extras' in props.item) {
                const item = props.item as SessionItem;
                if(props.onPickQuantity != undefined) {
                    const action = props.onPickQuantity;
                    let icon = undefined as undefined | React.ReactNode;
                    if(props.canRemove == true && props.canAdd == true) {
                        icon = <PlusMinusIcon height={20} width={20} />
                    } else if (props.canRemove == true) {
                        icon = <MinusIcon height={20} width={20} />;
                    } else if (props.canAdd == true) {
                        icon = <PlusIcon height={20} width={20} />;
                    }

                    if(icon != undefined) {
                        buttons.push(<Button
                                key={"change-quantity-button"}
                                sx={{height: "32px"}}
                                aria-label="reduce"
                                onClick={() => action(item)}
                            >
                            {icon}
                        </Button>);
                    }
                }
            }
        }
        if(buttons.length == 0) {
            return undefined;
        }

        return <ButtonGroup 
            variant="outlined"

        >
            { buttons }
        </ButtonGroup>
    }

    const modifiers = ("extras" in props.item ? props.item.extras : []) ?? [];
    const getPrice = () => {
        if(isOpen || modifiers.length == 0) {
            return props.item.price;
        }

        return Items.getPrice(props.item);
    }
    
    const item = props.item?.menuItemId != undefined ? props.item as BaseSessionItem : undefined;
    const name = item == undefined ? undefined : props.itemsMap.get(item?.menuItemId)?.name;
    return <>
        <ListItem
            sx={{
                opacity: isItemDisabled() ? 0.6 : 1,

                transition: "background-color 0.5s ease",
                borderRadius: 0,
                padding: "0 0 0 1rem",

                "& .MuiListItemSecondaryAction-root": {
                    position: "unset",
                    top: "unset",
                    right: 0,
                    transform: "unset",
                    marginRight: "0.5rem"
                },

                "& .MuiListItemAvatar-root": {
                    minWidth: "34px",
                },

                "& .MuiListItemText-primary": {
                    fontSize: "0.8rem",
                },

                "&.active": {
                    backgroundColor: "rgba(25, 118, 210, 0.08);",
                },
            }}
            className={props.recentlyChanged ? "active" : ""}
            style={{
                width: "100%", 
                transition: "background-color 0.5s ease",
                cursor: modifiers.length > 0 ? "pointer" : undefined,
                backgroundColor: !!props.showBackground && props.recentlyChanged == false ? "ghostwhite" : undefined,

                display: "flex",
                flexDirection: "row",
                flexWrap: "wrap",
                alignContent: "center",
                justifyContent: "center",
                alignItems: "center",
                gap: "0.75rem"
            }}
            secondaryAction={secondaryActions()}>
            <ListItemAvatar>
                {
                    props.showQuantityBadge?.(props.item.quantity) != false &&
                    <Chip label={Currency.toDecimalFormat({culture: i18n.language, value: props.item.quantity, maxDecimalPlaces: 2})} size="small" />
                }
            </ListItemAvatar>
            <ListItemText 
                primary={
                <Stack
                    direction="row"
                    spacing={2}
                >
                    <Box
                        sx={{ flexGrow: 1 }}
                    >
                        {

                            name == undefined
                            ?
                            <Skeleton animation="wave" width="100%" />
                            :
                            name
                        }
                    </Box>

                    {
                        modifiers.length > 0 &&
                        <>
                            &nbsp;
                            <Chip size="small" label={t("modifiers")} color="success" variant="outlined" onClick={() => modifiers.length > 0 && setIsOpen(p => !p)} />
                        </>
                    }
                </Stack>} 
                secondary={
                    <ButtonBase 
                        sx={{borderRadius: 2, fontSize: "0.8rem"}}
                        onClick={() => {
                            if(props.item == undefined) {
                                return;
                            }
                            if('extras' in props.item == false) {
                                return;
                            }
                            props.onClickEdit?.(props.item);
                        }}
                    >
                        <CurrencySpan value={getPrice()} />
                        {
                            'discountPercentage' in props.item && props.item.discountPercentage > 0 &&
                            <>
                                &nbsp;
                                <Chip size="small" label={`${Currency.toDecimalFormat({culture: i18n.language, value: props.item.discountPercentage, maxDecimalPlaces: 2})} %`} color="success" variant="outlined" />
                            </>
                        }
                        {
                            !props.isModifier && props.onClickEdit != undefined && item != undefined &&
                            <PencilIcon width={18} height={18} style={{marginLeft: "0.5rem"}} />
                        }
                    </ButtonBase>
                }
            />
        </ListItem>
        {
            modifiers.length > 0 &&
            <Collapse in={isOpen} timeout="auto" unmountOnExit>
                <List component="div" disablePadding sx={{ pl: 4 }}>
                    {
                        modifiers.map((m, i) => (
                        <SessionItemComponent
                            key={i}
                            item={m}
                            isModifier={true}
                            showQuantityBadge={q => q > 1}
                            recentlyChanged={props.recentlyChanged}
                            itemsMap={props.itemsMap}
                        />
                        ))
                    }
                </List>
            </Collapse>
        }
    </>
}

const OrderItemComponent = (props : {
    readonly order: Order;
    readonly itemsMap: Map<string, MenuItem>;
}) => {
    const { t } = useTranslation();
    const dateHelper = useDateHelper();
    const now = useNow(1000);
    const toast = useToast();
    const orderMutator = useOrderMutator();
    const awaiter = useActionAwaiter();

    const [isOpen, setIsOpen] = useState(false);
    const [isLoading, setIsLoading] = useState(false);

    const acceptOrder = async () => {
        setIsLoading(true);
        try {
            const jobId = await orderMutator.process(props.order, {})
            await awaiter.job(jobId);
        } catch {
            toast.error(t('unexpectedErrorHasOccurred'));
        } finally {
            setIsLoading(false);
        }
    }

    const declineOrder = async () => {
        setIsLoading(true);
        try {
            const jobId = await orderMutator.decline(props.order, {})
            await awaiter.job(jobId);
        } catch {
            toast.error(t('unexpectedErrorHasOccurred'));
        } finally {
            setIsLoading(false);
        }
    }

    const secondaryActions = (): React.ReactNode => {
        const buttons: React.ReactNode[] = [];

        let variant: 'text' | 'outlined' | 'contained' = "outlined";
        variant = "text";

        buttons.push(<Button
            key={"expand-button"}
            sx={{height: "32px"}}
            aria-label="reduce"
            onClick={() => setIsOpen(p => !p)}
            >
            {
                isOpen
                ?
                <ExpandIcon height={16} width={16} onClick={() => setIsOpen(p => !p)} fill="black"/>
                :
                <CollapseIcon height={16} width={16} onClick={() => setIsOpen(p => !p)} fill="black"/>
            }
        </Button>)

        if(isLoading == false) {
            buttons.push(<Button
                key={"accept-button"}
                sx={{height: "32px"}}
                aria-label="reduce"
                onClick={acceptOrder}
                >
                <CheckIcon height={16} width={16} aria-hidden="true" fill="green" />
            </Button>);

            buttons.push(<Button
                key={"decline-button"}
                sx={{height: "32px"}}
                aria-label="reduce"
                onClick={declineOrder}
                >
                <UncheckIcon height={16} width={16} aria-hidden="true" fill="red" />
            </Button>);
        } else {
            buttons.push(<Box sx={{ display: 'flex' }} key="loading">
                <CircularProgress />
            </Box>)
        }

        return <ButtonGroup variant={variant}>
            { buttons }
        </ButtonGroup>
    }

    return (
    <Card sx={{ margin: "1rem", cursor: "pointer", transition: "background-color 0.5s ease",}}>
        <CardHeader avatar={
                <Tooltip title={t("pendingApproval")}>
                    <Chip size="small" variant="outlined" color="info" label={<>
                            <ExclamationIcon height={16} width={16} aria-hidden="true" fill="#0288d1" />
                            {dateHelper.getTimeAgo(now, props.order.lastModified)}
                        </>}/>
                </Tooltip>
            }
            action={secondaryActions()}
            onClick={() => setIsOpen(s => !s)}
            title={<b>{t("order")} {props.order.sequenceNumber}</b>} 
            subheader={<CurrencySpan value={Items.getTotalPrice(props.order.items)} />}
        />
        <Collapse in={isOpen} timeout="auto">
            <List component="div" disablePadding>
            { 
                props.order.items.map((item, index) => (
                    <SessionItemComponent 
                        key={item.id}
                        showBackground={index % 2 == 0}
                        item={item}
                        itemStatusFilter={true}
                        recentlyChanged={false}
                        itemsMap={props.itemsMap}
                    />
                )) 
            }
            </List>
        </Collapse>
    </Card>)
}