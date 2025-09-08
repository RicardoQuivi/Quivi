import React, { useEffect, useMemo, useState } from "react"
import { Alert, Box, Checkbox, Divider, Grid, IconButton, keyframes, Skeleton, Stack, styled, TextField, Typography } from "@mui/material";
import { useTranslation } from "react-i18next";
import { BasePreparationGroupItem, PreparationGroup, PreparationGroupItem } from "../../../hooks/api/Dtos/preparationgroups/PreparationGroup";
import { Local } from "../../../hooks/api/Dtos/locals/Local";
import { useChannelsQuery } from "../../../hooks/queries/implementations/useChannelsQuery";
import { useChannelProfilesQuery } from "../../../hooks/queries/implementations/useChannelProfilesQuery";
import { CloseIcon } from "../../../icons";
import CustomModal, { ModalSize } from "../../Modals/CustomModal";
import { MenuItem } from "../../../hooks/api/Dtos/menuitems/MenuItem";
import { useNow } from "../../../hooks/useNow";
import { useToast } from "../../../context/ToastProvider";
import { useMenuItemsQuery } from "../../../hooks/queries/implementations/useMenuItemsQuery";
import { CardItemDetails } from "../CardItemDetails";
import { SingleSelect } from "../../Inputs/SingleSelect";
import { ActionButton } from "../../Buttons/ActionButton";
import { useDateHelper } from "../../../helpers/dateHelper";
import { useOrderHelper } from "../../../helpers/useOrderHelper";
import { usePreparationGroupMutator } from "../../../hooks/mutators/usePreparationGroupMutator";
import { useActionAwaiter } from "../../../hooks/useActionAwaiter";

const fadeIn = keyframes`
    from {
        opacity: 0;
    }
`;

const FadingTypography = styled(Typography)`
    animation: ${fadeIn} 1s infinite alternate;
`;
const StyledAlert = styled(Alert)(() => ({
    marginTop: "0.5rem",
    marginBottom: "0.5rem",

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
    },
}))

interface GroupDetailModalProps {
    readonly group?: PreparationGroup;
    readonly note?: string;
    readonly onClose: () => any;
    readonly localsMap: Map<string, Local>;
    readonly currentLocalId?: string | undefined;
    readonly checkedItems: Record<string, boolean>;
    readonly onCheckedItemsChanged: (checkedItems: Record<string, boolean>) => any;
}
export const PreparationGroupDetailModal = (props: GroupDetailModalProps) => {  
    const channelQuery = useChannelsQuery(props.group == undefined ? undefined : {
        sessionIds: [props.group.sessionId],
        page: 0,
        pageSize: 1,
        includeDeleted: true,
    })

    const channel = useMemo(() => {
        if(channelQuery.data.length == 0) {
            return undefined;
        }
        return channelQuery.data[0];
    }, [channelQuery.data])

    const profileQuery = useChannelProfilesQuery(channel == undefined ? undefined : {
        ids: [channel.channelProfileId],
        page: 0,
        pageSize: 1,
    })

    const profile = useMemo(() => {
        if(profileQuery.data.length == 0) {
            return undefined;
        }
        return profileQuery.data[0];
    }, [profileQuery.data])

    const getTitle = () => {
        if(props.group == undefined) {
            return <></>
        }

        return <Box
            sx={{
                display: "flex",
                flexDirection: "row",
                justifyContent: "space-between",
            }}
        >
            <Box
                sx={{
                    display: "flex",
                    flexDirection: "column",
                    justifyContent: "center",
                    alignContent: "center",
                    flex: 1,
                    flexWrap: "wrap",
                }}
            >
                <Typography
                    variant="h5"
                    gutterBottom
                    sx={{
                        fontWeight: "bold",
                    }}
                >
                {
                    channel == undefined || profile == undefined
                    ?
                    <Skeleton animation="wave" sx={{width: "90%", height: "100%"}} />
                    :
                    `${profile.name} ${channel.name}`
                }
                </Typography>
            </Box>

            <Box
                sx={{
                    display: "flex",
                    flexWrap: "wrap",
                    flexDirection: "row",
                    justifyContent: "center",
                    alignContent: "center",
                    gap: 4,
                }}
            >
                <IconButton onClick={props.onClose}>
                    <CloseIcon />
                </IconButton>
            </Box>
        </Box>
    }

    return <CustomModal isOpen={props.group != undefined} title={getTitle()} onClose={props.onClose} size={ModalSize.Large} hideClose>
        {
            props.group != undefined &&
            <GroupDetail
                group={props.group}
                note={props.note}
                onSubmit={props.onClose}
                localsMap={props.localsMap}
                locationId={props.currentLocalId}
                checkedItems={props.checkedItems}
                onCheckedItemsChanged={props.onCheckedItemsChanged}
            />
        }
    </CustomModal>
}

interface GroupDetailProps {
    readonly group: PreparationGroup;
    readonly note?: string;
    readonly onSubmit: () => any;
    readonly localsMap: Map<string, Local>;
    readonly locationId?: string;
    readonly checkedItems: Record<string, boolean>;
    readonly onCheckedItemsChanged: (checkedItems: Record<string, boolean>) => any;
}
const GroupDetail = ({
    group,
    note,
    onSubmit,
    localsMap,
    locationId,
    checkedItems,
    onCheckedItemsChanged,
}: GroupDetailProps) => {
    const { t } = useTranslation();
    const dateHelper = useDateHelper();
    const now = useNow(1000);
    const helper = useOrderHelper();
    const toast = useToast();
    const awaiter = useActionAwaiter();
    const mutator = usePreparationGroupMutator();
    
    const [selectedLocation, setSelectedLocation] = useState<Local>();
    const [innerNote, setInnerNote] = useState(note);

    const ids = useMemo(() => group.items.reduce((r, it) => {
        r.push(it.menuItemId);
        it.extras?.forEach(e => r.push(e.menuItemId));
        return r;
    }, [] as string[]), [group]);

    const menuItemsQuery = useMenuItemsQuery(ids.length == 0 ? undefined :  {
        ids: ids,
        includeDeleted: true,
        page: 0,
    })

    const menuItemsMap = useMemo(() => menuItemsQuery.data.reduce((r, i) => {
        r.set(i.id, i);
        return r;
    }, new Map<string, MenuItem>()), [menuItemsQuery.data])
    
    const itemsPerLocal = useMemo(() => {
        const result = new Map<string | undefined, (BasePreparationGroupItem | PreparationGroupItem)[]>();

        if(group == undefined) {
            result.set(undefined, []);
            return result;
        }

        for(const item of group.items) {
            const locations = (item.extras ?? []).reduce((r, it) => {
                r.add(it.locationId)
                return r;
            }, new Set<string | undefined>());

            locations.add(item.locationId ?? undefined);
            for(const location of locations) {
                const list = result.get(item.locationId ?? undefined) ?? [];
                list.push(item)
                result.set(location, list);
            }
        }
        return result;
    }, [group, localsMap, menuItemsMap])
    
    const totalItemChecked = useMemo(() => {
        let result = 0;
        for(const key in checkedItems) {
            if(checkedItems[key]) {
                ++result;
            }
        }
        return result;
    }, [checkedItems])

    useEffect(() => {
        if(locationId == undefined) {
            setSelectedLocation(undefined);

            const location = localsMap.values().next().value;
            if(location != undefined) {
                setSelectedLocation(location);
            }
            return;
        }

        const location = localsMap.get(locationId);
        if(location != undefined) {
            setSelectedLocation(location);
            return;
        }
    }, [localsMap, locationId])
   
    const commitPreparationGroup = async (asPrepared: boolean) => {
        try {
            const jobId = await mutator.commit(group, {
                note: innerNote,
                locationId: selectedLocation?.id ?? locationId,
                itemsToCommit: group.items.reduce((r, item) => {
                    if(checkedItems[item.id]) {
                        r[item.id] = item.remainingQuantity;
                    }
                    for(const extra of item.extras) {
                        if(checkedItems[extra.id]) {
                            r[extra.id] = extra.remainingQuantity;
                        }
                    }
                    return r;
                }, {} as Record<string, number>),
                isPrepared: asPrepared,
            })

            await awaiter.job(jobId);
            onSubmit();
        } catch {
            toast.error(t('unexpectedErrorHasOccurred'));
        }
    }

    const completePreparationGroup = async () => {
        try {
            await mutator.patch(group, {
                items: group.items.reduce((r, item) => {
                    if(checkedItems[item.id]) {
                        r[item.id] = item.remainingQuantity;
                    }
                    for(const extra of item.extras) {
                        if(checkedItems[extra.id]) {
                            r[extra.id] = extra.remainingQuantity;
                        }
                    }
                    return r;
                }, {} as Record<string, number>),
            })
            onSubmit();
        } catch {
            toast.error(t('unexpectedErrorHasOccurred'));
        }
    }

    const printPreparationGroup = async () => {
        try {
            await mutator.print(group, {
                locationId: locationId,
            })
        } catch {
            toast.error(t('Resources.UnexpectedErrorHasOccurred'));
        }
    }

    const getActions = () => {
        if(group.isCommited == true)
        {
            return (
                <Grid container spacing={2}>
                    <Grid size={{xs: 12, sm: 6}}>
                        <ActionButton
                            disabled={totalItemChecked != Object.keys(checkedItems).length}
                            style={{width: "100%", height: "100%"}}
                            onAction={() => printPreparationGroup()}
                        >
                            {t("print")}
                        </ActionButton>
                    </Grid>
                    <Grid size={{xs: 12, sm: 6}}>
                        <ActionButton
                            disabled={totalItemChecked == 0}
                            style={{width: "100%", height: "100%"}} 
                            primaryButton
                            onAction={() => completePreparationGroup()}
                        >
                            {t("ready")}
                        </ActionButton>
                    </Grid>
                </Grid>
                )
        }

        return (
            <Grid container spacing={2}>
                <Grid size={{xs: 12, sm: 6}}>
                    <ActionButton 
                        style={{width: "100%", height: "100%"}} 
                        onAction={() => commitPreparationGroup(true)}
                    >
                        {t("ready")}
                    </ActionButton>
                </Grid>
                <Grid size={{xs: 12, sm: 6}}>
                    <ActionButton 
                        style={{width: "100%", height: "100%"}}
                        primaryButton
                        onAction={() => commitPreparationGroup(false)}
                    >
                        { t("ordersTab.sendToPreparation")}
                    </ActionButton>
                </Grid>
            </Grid>
            )
    }

    const onItemClicked = (item: BasePreparationGroupItem | PreparationGroupItem) => onCheckedItemsChanged({
        ...checkedItems,
        [item.id]: checkedItems[item.id] == undefined ? true : !checkedItems[item.id],
    });

    const getItemsForLocation = (localId: string | undefined) => {
        const items = itemsPerLocal.get(localId);
        const filteredItems = items?.filter(i => i.remainingQuantity != 0 || ('extras' in i && i.extras.find(e => e.locationId == localId && e.remainingQuantity != 0) != undefined));
        if(filteredItems?.length == 0) {
            return undefined;
        }

        const local = localId == undefined ? undefined : localsMap.get(localId);
        return <CardItemDetails
                    key={localId ?? "No Location"}
                    hideQuantityAndActionsIfZero
                    
                    items={filteredItems} 
                    header={localId == undefined 
                            ?
                            (
                                selectedLocation == undefined
                                ?
                                <b>{t("ordersTab.noLocal")}</b>
                                :
                                <b>{selectedLocation.name}</b>
                            )
                            :
                            (
                                local == undefined
                                ?
                                <Skeleton animation="wave" width={50} />
                                :
                                <b>{local.name}</b>
                            )
                    }

                    getId={c => c.id}
                    getSubItems={c => 'extras' in c ? c.extras.filter(e => e.locationId == localId) : []}
                    getQuantity={c => c.remainingQuantity}
                    renderName={c => {
                        const digitalItem = menuItemsMap.get(c.menuItemId);
                        if(digitalItem == undefined) {
                            return <Skeleton animation="wave" width={50} />
                        }
                        return digitalItem.name;
                    }}
                    onItemClicked={onItemClicked}
                    renderExtraAction={(item) => <Checkbox edge="end" checked={checkedItems[item.id] ?? false} />}
        />
    }

    const getItems = () => {
        if(group == undefined) {
            return;
        }

        const aux = [] as React.ReactNode[];
        for(const entry of itemsPerLocal) {
            aux.push(getItemsForLocation(entry[0]))
        }

        return aux;
    }

    const isDelayed = group.lastModified != undefined && helper.isOrderDelayed(group.lastModified);
    const notes = [note].filter(o => !!o);

    return <Stack direction="column" spacing={2}>
        {
            itemsPerLocal.has(undefined) &&
            locationId == undefined &&
            selectedLocation != undefined &&
            <>
                <Divider>
                    <Typography variant="overline">{t("local")}</Typography>
                </Divider>
                <SingleSelect
                    label={t("local")}
                    value={selectedLocation} 
                    options={Array.from(localsMap.values())}
                    getId={l => l.id}
                    render={l => l.name}
                    onChange={setSelectedLocation}
                />
                <Divider>
                    <Typography variant="overline">{t("items")}</Typography>
                </Divider>
            </>
        }
        {getItems()}
        <StyledAlert variant="outlined" severity={isDelayed ? "warning": "info"}>
            {
                isDelayed
                ?
                <FadingTypography variant="subtitle1">
                    {dateHelper.getTimeAgo(now, group.lastModified)}
                </FadingTypography>
                :
                <Typography variant="subtitle1">
                    {dateHelper.getTimeAgo(now, group.lastModified)}
                </Typography>
            }
        </StyledAlert>
        {
            (notes.length > 0 || group.isCommited == false) &&
            <>
                <Divider>{t("observations")}</Divider>
                {
                    group.isCommited == false &&
                    <TextField label={t("observations")} multiline rows={4} value={innerNote} onChange={(e) => setInnerNote(e.target.value)} sx={{mt: "1rem", mb: "1rem", width: "100%"}}/>
                }
                { notes.map(o => <Typography mt={2} variant="subtitle1">{o}</Typography>) }
            </>
        }

        <Divider />
        {getActions()}
    </Stack>
}