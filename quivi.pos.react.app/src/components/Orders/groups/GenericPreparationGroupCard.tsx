import React, { useEffect, useMemo, useState } from "react"
import { Alert, Box, Card, CardActionArea, CardActions, CardContent, CardHeader, Checkbox, Chip, Divider, Grid, keyframes, Paper, Skeleton, styled, Typography } from "@mui/material";
import { useTranslation } from "react-i18next";
import { BasePreparationGroupItem, PreparationGroup, PreparationGroupItem } from "../../../hooks/api/Dtos/preparationgroups/PreparationGroup";
import { Order } from "../../../hooks/api/Dtos/orders/Order";
import { Local } from "../../../hooks/api/Dtos/locals/Local";
import { useChannelsQuery } from "../../../hooks/queries/implementations/useChannelsQuery";
import { useMenuItemsQuery } from "../../../hooks/queries/implementations/useMenuItemsQuery";
import { useNow } from "../../../hooks/useNow";
import { TimeUnit, useDateHelper } from "../../../helpers/dateHelper";
import { useOrdersQuery } from "../../../hooks/queries/implementations/useOrdersQuery";
import { CardItemDetails } from "../CardItemDetails";
import { useChannelProfilesQuery } from "../../../hooks/queries/implementations/useChannelProfilesQuery";
import { CollectionFunctions } from "../../../helpers/collectionsHelper";
import { SortDirection } from "../../../hooks/api/Dtos/SortableRequest";

const getCheckedItems = (itemsMap: Map<string, BasePreparationGroupItem> | undefined, checkedMap: Record<string, boolean>, previous: {
    checks: Record<string, boolean>,
    changes: Record<string, boolean>,
}) => {
    if(itemsMap == undefined) {
        return {
            checks: {},
            changes: {},
        };
    }
    
    let checksResult = undefined as Record<string, boolean> | undefined;
    const keysToDelete = new Set(Object.keys(previous.checks));
    for(const [itemId] of itemsMap) {
        const isChecked = checkedMap[itemId] ?? false;

        if(itemId in previous.checks) {
            if(previous.checks[itemId] != isChecked) {
                if(checksResult == undefined) {
                    checksResult = {
                        ...previous.checks,
                    }
                }
                checksResult[itemId] = isChecked;
            }
            keysToDelete.delete(itemId);
            continue;
        }

        if(isChecked) {
            if(checksResult == undefined) {
                checksResult = {
                    ...previous.checks,
                }
            }
            checksResult[itemId] = isChecked;
            continue;
        }

        previous.checks[itemId] = isChecked;
    }

    for(const key of keysToDelete) {
        if(checksResult == undefined) {
            checksResult = {
                ...previous.checks,
            }
        }
        delete checksResult[key];
    }


    let changesResult = undefined as Record<string, boolean> | undefined;
    for(const key in previous.changes) {
        const item = itemsMap.get(key);
        if(item != undefined) {
            if(key in previous.changes && previous.changes[key] == checkedMap[key]) {
                if(changesResult == undefined) {
                    changesResult = {
                        ...previous.changes,
                    };
                }
                delete changesResult[key];
            }
            continue;
        }
        
        if(key in previous.changes) {
            if(changesResult == undefined) {
                changesResult = {
                    ...previous.changes,
                };
            }
            delete changesResult[key];
        }
    }

    if(checksResult == undefined && changesResult == undefined) {
        return previous;
    }

    return {
        checks: checksResult ?? previous.checks,
        changes: changesResult ?? previous.changes,
    };
}

interface GenericPreparationGroupCardProps {
    readonly group?: PreparationGroup;
    readonly locationId: string | undefined;
    readonly onOrderClicked?: (order: Order) => any;
    readonly onCardClicked?: (group: PreparationGroup) => any;
    readonly isCheckedMap: Record<string, boolean>;
    readonly locationsMap: Map<string, Local>;
    readonly onItemsChecked?: (group: PreparationGroup, changes: Record<string, boolean>, selections: Record<string, boolean>) => any;
    readonly headerAction?: React.ReactNode;
    readonly footerButtons?: React.ReactNode[];
}
export const GenericPreparationGroupCard = (props: GenericPreparationGroupCardProps) => {
    const { t } = useTranslation();

    const channelsQuery = useChannelsQuery(props.group == undefined ? undefined : {
        sessionIds: [props.group.sessionId],
        page: 0,
        pageSize: 1,
        includeDeleted: true,
    })
    const channel = useMemo(() => channelsQuery.data.length == 0 ? undefined : channelsQuery.data[0], [channelsQuery.data])

    const profilesQuery = useChannelProfilesQuery(channel == undefined ? undefined : {
        ids: [channel.channelProfileId],
        page: 0,
        pageSize: 1,
    })
    const profile = useMemo(() => profilesQuery.data.length == 0 ? undefined : profilesQuery.data[0], [profilesQuery.data])

    const ordersQuery = useOrdersQuery(props.group == undefined || props.group.orderIds.length == 0 ? undefined : {
        ids: props.group.orderIds,
        page: 0,
        sortDirection: SortDirection.Asc,
    })
    const ordersMap = useMemo(() => CollectionFunctions.toMap(ordersQuery.data, o => o.id), [ordersQuery.data]);

    const ids = useMemo(() => {
        const set = new Set<string>();
        for(const it of props.group?.items ?? []) {
            set.add(it.menuItemId);
            for(const e of it.extras) {
                set.add(e.menuItemId);
            }
        }
        return Array.from(set);
    }, [props.group]);

    const menuItemsQuery = useMenuItemsQuery(ids.length == 0 ? undefined : {
        ids: ids,
        includeDeleted: true,
        page: 0,
    })
    const menuItemsMap = useMemo(() => CollectionFunctions.toMap(menuItemsQuery.data, i => i.id), [menuItemsQuery.data])

    const groupItemsMap = useMemo(() => props.group?.items.reduce((r, item) => {
        r.set(item.id, item);
        for(const extra of item.extras ?? []) {
            r.set(extra.id, extra);
        }
        return r;
    }, new Map<string, BasePreparationGroupItem>()), [props.group]);
    
    const itemsPerLocation = useMemo(() => {
        const result = new Map<string | undefined, (BasePreparationGroupItem | PreparationGroupItem)[]>();

        if(props.group == undefined) {
            result.set(undefined, []);
            return result;
        }

        const group = props.group;
        for(const item of group.items) {
            const locations = (item.extras ?? []).reduce((r, it) => {
                r.add(it.locationId)
                return r;
            }, new Set<string | undefined>());

            locations.add(item.locationId);

            for(const location of locations) {
                const list = result.get(location) ?? [];
                list.push(item)
                result.set(location, list);
            }
        }
        return result;
    }, [props.group, props.locationsMap, menuItemsMap])

    const [checkedItems, setCheckedItems] = useState(getCheckedItems(groupItemsMap, props.isCheckedMap, {
        changes: {},
        checks: props.isCheckedMap,
    }))
    const [isDelayed, setIsDelayed] = useState(false);

    useEffect(() => setCheckedItems(c => getCheckedItems(groupItemsMap, props.isCheckedMap, c)), [groupItemsMap, props.isCheckedMap])

    useEffect(() => {
        if(props.group == undefined) {
            return;
        }

        props.onItemsChecked?.(props.group, checkedItems.changes, checkedItems.checks);
    }, [checkedItems])

    const onItemClicked = (item: BasePreparationGroupItem | PreparationGroupItem) => setCheckedItems(c => {
        const getChanges = (s: Record<string, boolean>, defaultChecked: boolean) => {
            const isChecked = item.id in s ? !s[item.id] : !(defaultChecked);
            
            if(defaultChecked == isChecked) {
                if(item.id in s) {
                    delete s[item.id];
                    return {
                        ...s,
                    }
                }
                return undefined;
            }
            s[item.id] = isChecked;
            return {
                ...s,
            }
        }

        const changes = getChanges(c.changes, props.isCheckedMap[item.id]);
        return {
            checks: {
                ...c.checks,
                [item.id]: !c.checks[item.id],
            },
            changes: changes ?? c.changes,
        }
    });
    
    const getTitle = () => {
        return <Box
            sx={{
                display: "flex",
                flexDirection: "row",
                width: "100%",
            }}
        >
            {
                channel == undefined  || profile == undefined
                ?
                <Skeleton animation="wave" sx={{width: "100%"}} />
                :
                <Typography variant="body1" fontWeight="bold" flex="0 0 auto">{profile.name} {channel.name}</Typography>
            }
        </Box>;
    }
    
    const getItemsForLocation = (localId: string | undefined) => {
        const items = itemsPerLocation.get(localId);
        const filteredItems = items?.filter(i => i.remainingQuantity != 0 || ('extras' in i && i.extras.find(e => e.locationId == localId && e.remainingQuantity != 0) != undefined));
        if(filteredItems?.length == 0) {
            return undefined;
        }

        const location = localId == undefined ? undefined : props.locationsMap.get(localId);
        return <CardItemDetails
                    key={localId}
                    hideQuantityAndActionsIfZero
                    
                    items={filteredItems} 
                    header={localId == undefined 
                            ? 
                                <b>{t("ordersTab.noLocal")}</b>
                            :
                            (
                                location == undefined
                                ?
                                <Skeleton animation="wave" width={50} />
                                :
                                <b>{location.name}</b>
                            )
                    }

                    getId={c => c.id}
                    getSubItems={c => 'extras' in c ? c.extras.filter(e => e.locationId == localId) : []}
                    getQuantity={c => props.group?.isCommited == true? c.quantity : c.remainingQuantity}
                    renderName={c => {
                        const digitalItem = menuItemsMap.get(c.menuItemId);
                        if(digitalItem == undefined) {
                            return <Skeleton animation="wave" width={50} />
                        }
                        return digitalItem.name;
                    }}
                    onItemClicked={onItemClicked}
                    renderExtraAction={(item) => <Checkbox edge="end" checked={checkedItems.checks[item.id] ?? false} />}
        />
    }

    const getItems = () => {
        if(props.group == undefined) {
            return;
        }

        const aux = [] as React.ReactNode[];
        if(props.locationId != undefined) {
            aux.push(getItemsForLocation(props.locationId))
            return aux;
        }

        for(const entry of itemsPerLocation) {
            aux.push(getItemsForLocation(entry[0]))
        }

        return aux;
    }

    const getCardFooter = () => {
        if(props.footerButtons == undefined || props.footerButtons.length == 0) {
            return undefined;
        }

        return (
            <Grid container spacing={2} width="100%">
                { props.footerButtons.map((b, index) => (<Grid size={12} key={index}> {b} </Grid>)) }
            </Grid>
        )
    }

    return <Paper
        elevation={16}
        sx={{
            height: "100%",
            maxWidth: "100%",
        }}
    >
        <Card
            sx={{
                borderStyle: isDelayed ? "solid" : undefined,
                borderColor: t => isDelayed ? t.palette.warning.main : undefined,
                borderWidth: isDelayed ? "2px" : undefined,
                height: "100%",
                display: "flex",
                flexDirection: "column",
            }}
        >
            <CardActionArea 
                onClick={() => props.group != undefined && props.onCardClicked?.(props.group)} 
                sx={{
                    flex: "0 0 auto",
                    cursor: "pointer",
                }}
            >
                <CardHeader 
                    title={getTitle()} 
                    slotProps={{
                        title: {
                            fontSize: "1rem",
                        },
                    }}
                    action={props.headerAction}
                />
            </CardActionArea>
            <Divider sx={{flex: "0 0 auto"}} />
            <CardContent
                sx={{
                    paddingTop: "0.5rem",
                    paddingBottom: "0 !important",
                    flex: "1 1 auto",
                }}
            >
                {getItems()}
            </CardContent>
            <CardContent
                sx={{
                    marginTop: "0.5rem",
                    paddingTop: "0",
                    paddingBottom: "0.5rem !important",
                    flex: "0 0 auto",
                }}
            >
                <TimeBadge 
                    date={props.group?.createdDate}
                    onDelayedChange={setIsDelayed}
                />
            </CardContent>
            {
                props.group != undefined &&
                <CardContent>
                    <Divider sx={{mb: "0.5rem"}}>{t("ordersTab.associatedOrders")}</Divider>
                    <Grid
                        container
                        spacing={1}
                        wrap="wrap"
                        sx={{
                            overflowX: "auto",
                        }}
                    >
                    {
                        props.group.orderIds.map(id => <Grid size="auto" key={id} justifyContent="center" display="flex">
                            <OrderBadge order={ordersMap.get(id)} onOrderClicked={props.onOrderClicked} />
                        </Grid>)
                    }
                    </Grid>
                </CardContent>
            }
            <CardActions disableSpacing style={{flex: "0 0 auto", gap: 1}}>
                {getCardFooter()}
            </CardActions>
        </Card>
    </Paper>
}

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
    }
}))

interface TimeBadgeProps {
    readonly date?: string | Date;
    readonly onDelayedChange: (isDelayed: boolean) => any;
}
const TimeBadge = (props: TimeBadgeProps) => {
    const dateHelper = useDateHelper();
    const now = useNow(1000);

    const isDelayed = useMemo(() => props.date == undefined ? false : dateHelper.getTimeTaken(props.date, now, TimeUnit.Minutes) > 15, [props.date, now])

    useEffect(() => props.onDelayedChange(isDelayed), [isDelayed])
    
    if(props.date == undefined) {
        return <Skeleton animation="wave" />
    }

    return (
        <StyledAlert
            variant="outlined"
            severity={isDelayed ? "warning": "info"}
        >
        {
            isDelayed
            ?
            <FadingTypography variant="subtitle1">
                {dateHelper.getTimeAgo(now, props.date)}
            </FadingTypography>
            :
            <Typography variant="subtitle1">
                {dateHelper.getTimeAgo(now, props.date)}
            </Typography>
        }
        </StyledAlert>
    )
}

interface OrderSectionProps {
    readonly order?: Order;
    readonly onOrderClicked?: (order: Order) => any;
}
const OrderBadge = (props: OrderSectionProps) => {
    return <Chip
        color="primary"
        variant="filled"
        onClick={(evt) => {
            if(props.order == undefined) {
                return;
            }

            evt.stopPropagation(); 
            props.onOrderClicked?.(props.order); 
        }}
        label={props.order == undefined ? <Skeleton animation="wave" width={50}/> : props.order.sequenceNumber}
    />
}