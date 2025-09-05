import { useEffect, useMemo, useState } from 'react';
import { Trans, useTranslation } from 'react-i18next';
import { Box } from '@mui/system';
import { Autocomplete, ButtonBase, CircularProgress, FormControl, Grid, Skeleton, TextField } from '@mui/material';
import { Channel } from '../../hooks/api/Dtos/channels/Channel';
import { QuantifiedItem, QuantifiedItemPicker, SelectableItem } from '../Pickers/QuantifiedItemPicker';
import { useSessionsQuery } from '../../hooks/queries/implementations/useSessionsQuery';
import { useChannelsQuery } from '../../hooks/queries/implementations/useChannelsQuery';
import { useToast } from '../../context/ToastProvider';
import { SessionItem } from '../../hooks/api/Dtos/sessions/SessionItem';
import { usePosIntegrationsQuery } from '../../hooks/queries/implementations/usePosIntegrationsQuery';
import CustomModal, { ModalSize } from '../Modals/CustomModal';
import LoadingButton from '../Buttons/LoadingButton';
import HighlightMessage, { MessageType } from '../Messages/HighlightMessage';
import { useChannelProfilesQuery } from '../../hooks/queries/implementations/useChannelProfilesQuery';
import { ChannelProfile } from '../../hooks/api/Dtos/channelProfiles/ChannelProfile';
import { LoadingAnimation } from '../Loadings/LoadingAnimation';
import { RightArrowIcon } from '../../icons';
import { useCartSession } from '../../hooks/pos/session/useCartSession';
import { useMenuItemsQuery } from '../../hooks/queries/implementations/useMenuItemsQuery';
import { MenuItem } from '../../hooks/api/Dtos/menuitems/MenuItem';
import { ICartSession } from '../../hooks/pos/session/ICartSession';

const getItemIds = (...sessions: ICartSession[]) => {
    const ids = new Set<string>();

    for(const s of sessions) {
        for(const item of s.items) {
            ids.add(item.menuItemId);
            for(const extra of item.extras) {
                ids.add(extra.menuItemId);
            }
        }
    }

    return Array.from(ids);
}
const sort = (qrCodes: Channel[]): Channel[] => qrCodes.sort((a, b) => {
    const profileCompare =  a.channelProfileId.localeCompare(b.channelProfileId);
    if(profileCompare != 0) {
        return profileCompare;
    }

    const numberA = parseInt(a.name);
    const numberB = parseInt(b.name);

    if(Number.isNaN(numberA) == false && Number.isNaN(numberB) == false) {
        return numberA - numberB;
    }
    return a.name.localeCompare(b.name);
})

interface InternalQuantifiedItem<T> extends QuantifiedItem<T> {
    quantity: number;
}

interface Props {
    readonly currentChannel: Channel | undefined;
    readonly onClose: () => any;
}
export const TransferSessionModal = ({
    currentChannel,
    onClose,
}: Props) => {
    const { t } = useTranslation();
    const toast = useToast();

    const openedSessionsQuery = useSessionsQuery({
        isOpen: true,
        page: 0,
    })
    const channelsQuery = useChannelsQuery({
        page: 0,
        allowsSessionsOnly: true,
    })

    const [state, setState] = useState({
        isSubmitting: false,
        sourceOptions: {
            selected: undefined as (Channel | undefined),
            data: [] as Channel[]
        },
        targetOptions: {
            selected: undefined as (Channel | undefined),
            data: [] as Channel[]
        },
        unselectedItems: [] as InternalQuantifiedItem<SessionItem>[],
        selectedItems: [] as InternalQuantifiedItem<SessionItem>[],
    })

    const sourceSession = useCartSession(state.sourceOptions.selected?.id);
    const targetSession = useCartSession(state.targetOptions.selected?.id);

    const profilesQuery = useChannelProfilesQuery({
        page: 0,
    })
    const profilesMap = useMemo(() => {
        const result = new Map<string, ChannelProfile>();
        for(const p of profilesQuery.data) {
            result.set(p.id, p);
        }
        return result;
    }, [profilesQuery.data])


    const itemIds = getItemIds(sourceSession, targetSession);
    const itemsQuery = useMenuItemsQuery(itemIds.length == 0 ? undefined : {
        ids: itemIds,
        page: 0,
    })
    const itemsMap = useMemo(() => {
        const map = new Map<string, MenuItem>();
        for(const item of itemsQuery.data) {
            map.set(item.id, item);
        }
        return map;
    }, [itemsQuery.data])

    const sourceChannelProfile = state.sourceOptions.selected?.channelProfileId == undefined ? undefined : profilesMap.get(state.sourceOptions.selected.channelProfileId);
    const integrationSourceQuery = usePosIntegrationsQuery(sourceChannelProfile == undefined ? undefined : {
        ids: [sourceChannelProfile.posIntegrationId],
        page: 0,
    })
    const targetChannelProfile = state.targetOptions.selected?.channelProfileId == undefined ? undefined : profilesMap.get(state.targetOptions.selected.channelProfileId);
    const integrationTargetQuery = usePosIntegrationsQuery(targetChannelProfile == undefined ? undefined : {
        ids: [targetChannelProfile.posIntegrationId],
        page: 0,
    })

    const isLoading = !state.isSubmitting && (openedSessionsQuery.isFirstLoading || channelsQuery.isFirstLoading || (sourceSession.isSyncing && openedSessionsQuery.data.length > 0))

    //#region Helpers and User Actions
    const resetData = () => setState(s => ({
        ...s,
        isSubmitting: false,
        sourceOptions: {
            selected: undefined,
            data: [],
        },
        targetOptions: {
            selected: undefined,
            data: [],
        },
        selectedItems: [],
        unselectedItems: [],
    }))

    const onLoadOptions = (openedChannels: Channel[], closeChannels: Channel[], selectedChannelId: string) => {
        const openedQrCode = openedChannels.find(q => q.id == selectedChannelId);
        if (openedQrCode != undefined) {
            const availableQrCodes = sort([...closeChannels, ...openedChannels.filter(q => q != openedQrCode)]);
            setState(s => ({
                ...s,
                sourceOptions: {
                    data: [openedQrCode],
                    selected: openedQrCode,
                },
                targetOptions: {
                    data: availableQrCodes,
                    selected: availableQrCodes.length > 0 ? availableQrCodes[0] : undefined,
                },
            }))
            return;
        }
        const closedQrCode = closeChannels.find(q => q.id == selectedChannelId);
        if(closedQrCode != undefined) {
            const availableQrCodes = sort(openedChannels);
            setState(s => ({
                ...s,
                sourceOptions: {
                    data: availableQrCodes,
                    selected: availableQrCodes.length > 0 ? availableQrCodes[0] : undefined,
                },
                targetOptions: {
                    data: [closedQrCode],
                    selected: closedQrCode,
                },
            }))
        }
    }

    const getNotAllowedReason = () => {
        const result = [];
        if (state.sourceOptions.data.length == 0) {
            result.push(t("noOpenSessions")!);
        }

        if (state.targetOptions.data.length == 0) {
            result.push(t("noCloseSessions")!);
        }

        const sourceIntegration = integrationSourceQuery.data.length == 0 ? {
            allowsRemovingItemsFromSession: false,
        } : integrationSourceQuery.data[0];
        if(state.sourceOptions.selected != undefined && integrationSourceQuery.isFirstLoading == false && (sourceIntegration.allowsRemovingItemsFromSession == false)) {
            const sourceProfile = profilesMap.get(state.sourceOptions.selected.channelProfileId);
            if(sourceProfile == undefined) {
                result.push(<Skeleton animation="wave" />)
            } else {
                result.push(<Trans
                    t={t}
                    i18nKey="sourceIntegrationDoesNotAllowRemovingItems"
                    shouldUnescape={true}
                    values={{
                        name: `${sourceProfile.name} ${state.sourceOptions.selected.name}`,
                    }}
                    components={{
                        bold: <b/>
                    }}
                />)
            }
        }


        const targetIntegration = integrationTargetQuery.data.length == 0 ? {
            allowsAddingItemsToSession: false,
            allowsOpeningSessions: false,
        } : integrationTargetQuery.data[0];
        if(state.targetOptions.selected != undefined && integrationTargetQuery.isFirstLoading == false) {
            const targetProfile = profilesMap.get(state.targetOptions.selected.channelProfileId);
            
            if(targetProfile == undefined) {
                result.push(<Skeleton animation="wave" />)
            } else {
                if(targetIntegration.allowsAddingItemsToSession == false) {
                    result.push(<Trans
                        t={t}
                        i18nKey="targetIntegrationDoesNotAllowAddingItems"
                        shouldUnescape={true}
                        values={{
                            name: `${targetProfile.name} ${state.targetOptions.selected.name}`,
                        }}
                        components={{
                            bold: <b/>
                        }}
                    />)
                }

                if(targetIntegration.allowsOpeningSessions == false && targetSession.isSyncing == false && targetSession.closedAt != null) {
                    result.push(<Trans
                        t={t}
                        i18nKey="targetIntegrationDoesNotAllowOpeningSessions"
                        shouldUnescape={true}
                        values={{
                            name: `${targetProfile.name} ${state.targetOptions.selected.name}`,
                        }}
                        components={{
                            bold: <b/>
                        }}
                    />)
                }
            }
        }

        return result;
    }

    const getFromSourceItemsLabel = () => {
        if(state.sourceOptions.selected == undefined) {
            return undefined;
        }

        const sourceProfile = profilesMap.get(state.sourceOptions.selected.channelProfileId);
        if(sourceProfile == undefined) {
            return <Skeleton animation="pulse" />
        }

        return t("transferingFromChannel", {
            channel: `${sourceProfile.name} ${state.sourceOptions.selected.name}`,
        });
    }

    const getToTargetItemsLabel = () => {
        if(state.targetOptions.selected == undefined) {
            return undefined;
        } 
        
        const targetProfile = profilesMap.get(state.targetOptions.selected.channelProfileId);
        if(targetProfile == undefined) {
            return <Skeleton animation="pulse" />
        }

        return t("transferingToChannel", {
            channel: `${targetProfile.name} ${state.targetOptions.selected.name}`,
        });
    }

    const onItemsChanged = (changedItems: SelectableItem<SessionItem>[]) => {
        let selectedItemsResult: InternalQuantifiedItem<SessionItem>[] = [...state.selectedItems];
        let unselectedItemsResult: InternalQuantifiedItem<SessionItem>[] = [...state.unselectedItems];

        for(const item of changedItems) {
            let unselectedItem = unselectedItemsResult.find(u => u.item.id == item.item.id);
            let selectedItem = selectedItemsResult.find(u => u.item.id == item.item.id);

            if(selectedItem == undefined) {
                selectedItem = {
                    item: item.item,
                    quantity: 0,
                }
                selectedItemsResult.push(selectedItem);
            }

            if(unselectedItem == undefined) {
                unselectedItem = {
                    item: item.item,
                    quantity: 0,
                }
                unselectedItemsResult.push(unselectedItem);
            }

            unselectedItem.quantity -= item.quantity;
            selectedItem.quantity += item.quantity;
        }

        setState(s => ({
            ...s,
            selectedItems: selectedItemsResult.filter(i => i.quantity > 0 && i.item.isPaid == false),
            unselectedItems: unselectedItemsResult.filter(i => i.quantity > 0 && i.item.isPaid == false),
        }))
    }
    
    const onSubmit = async () => {
        setState(s => ({
            ...s,
            isSubmitting: true
        }));

        try {
            const itemsToTransfer = state.selectedItems.reduce((r, item) => {
                r.set(item.item, item.quantity);
                return r;
            }, new Map<SessionItem, number>());
            sourceSession.transferSession(state.targetOptions.selected!.id, itemsToTransfer);
            await sourceSession.forceSync();
        } catch {
            toast.error(t('unexpectedErrorHasOccurred'));
        }
    }
    //#endregion

    //#region Events
    useEffect(() => {
        if(currentChannel != undefined) {
            return;
        }
        resetData();
    }, [currentChannel]);

    useEffect(() => {
        if (currentChannel == undefined) {
            return;
        }

        if(openedSessionsQuery.isFirstLoading) {
            return;
        }

        if(channelsQuery.isFirstLoading) {
            return;
        }

        const openSessionChannelIds = new Set(openedSessionsQuery.data.map(c => c.channelId));
        const openedQrCodes = [] as Channel[];
        const closedQrCodes = [] as Channel[];
        
        for(const channel of channelsQuery.data) {
            if(openSessionChannelIds.has(channel.id)) {
                openedQrCodes.push(channel);
            } else {
                closedQrCodes.push(channel);
            }
        }
        onLoadOptions(openedQrCodes, closedQrCodes, currentChannel.id);
    }, [currentChannel, openedSessionsQuery.isFirstLoading, channelsQuery.isFirstLoading, openedSessionsQuery.data, channelsQuery.data]);

    useEffect(() => {
        if (!state.isSubmitting) {
            return;
        }

        const isSyncing = sourceSession.isSyncing ?? true;
        if (!isSyncing) {
            onClose();
            toast.success(t("transferSessionSucceeded"));
            setState(s => ({
                ...s,
                isSubmitting: false,
            }))
        }
    }, [sourceSession.isSyncing]);

    useEffect(() => {
        if(sourceSession.isSyncing) {
            return;
        }

        setState(s => ({
            ...s,
            selectedItems: sourceSession.items.filter(item => item.isPaid == false).map(item => ({
                item: item as SessionItem,
                quantity: item.quantity,
            })),
        }))
    }, [sourceSession.items, sourceSession.isSyncing])
    //#endregion

    const notAllowedReason = getNotAllowedReason();
    const isLoadingAnything = isLoading || sourceSession.isSyncing || integrationSourceQuery.isFirstLoading || integrationTargetQuery.isFirstLoading || targetSession.isSyncing;
    return (
        <CustomModal
            size={ModalSize.Large}
            disableCloseOutsideModal
            isOpen={currentChannel != undefined}
            onClose={onClose}
            title={t("transferSession")}
            footer={
                <Grid container sx={{width: "100%", margin: "1rem 0.25rem"}} spacing={1}>
                    <Grid size="grow">
                        <LoadingButton isLoading={false} onClick={onClose} style={{width: "100%"}}>
                            {t("close")}
                        </LoadingButton>
                    </Grid>
                    <Grid size="grow">
                        <LoadingButton
                            isLoading={state.isSubmitting}
                            disabled={
                                isLoadingAnything ||
                                notAllowedReason.length > 0 ||
                                state.selectedItems.length == 0
                            }
                            onClick={onSubmit}
                            primaryButton
                            style={{width: "100%"}}
                        >
                            {t("transfer")}
                        </LoadingButton>
                    </Grid>
                </Grid>
            }
        >
            <Grid container rowSpacing={3} justifyContent="center">
                <Grid size={12}>
                    {
                        state.sourceOptions.data.length > 0 && state.targetOptions.data.length > 0 &&
                        <Grid container>
                            <Grid
                                size={{
                                    xs: 12,
                                    sm: "grow",
                                }}
                            >
                                <Dropdown
                                    label={t("source")}
                                    selectedItem={state.sourceOptions.selected}
                                    items={state.sourceOptions.data}
                                    isLoading={isLoading}
                                    onSelectionChanged={selection => setState(s => ({
                                        ...s,
                                        sourceOptions: {
                                            ...s.sourceOptions,
                                            selected: selection,
                                        }
                                    }))}
                                    channelProfilesMap={profilesMap}
                                />
                            </Grid>
                            <Grid
                                size={{
                                    xs: 12,
                                    sm: "auto",
                                }}
                                alignContent="center"
                                justifyContent="center"
                                display="flex"
                                sx={{
                                    transform: {
                                        xs: "rotate(90deg)",
                                        sm: "none"
                                    }
                                }}
                            >
                                <ButtonBase
                                    disabled
                                    sx={{
                                        margin: "0.5rem",
                                    }}
                                >
                                    <RightArrowIcon height={24} width={24} />
                                </ButtonBase>
                            </Grid>
                            <Grid
                                size={{
                                    xs: 12,
                                    sm: "grow",
                                }}
                            >
                                <Dropdown
                                    label={t("target")}
                                    selectedItem={state.targetOptions.selected}
                                    items={state.targetOptions.data}
                                    isLoading={isLoading}
                                    onSelectionChanged={selection => setState(s => ({
                                        ...s,
                                        targetOptions: {
                                            ...s.targetOptions,
                                            selected: selection,
                                        }
                                    }))}
                                    channelProfilesMap={profilesMap}
                                />
                            </Grid>
                        </Grid>
                    }
                    { notAllowedReason.map((r, i) => <HighlightMessage messageType={MessageType.warning} key={i} children={r} />) }
                </Grid>
                {
                    isLoadingAnything
                    ?
                    <CircularProgress />
                    :
                    (
                        notAllowedReason.length == 0 &&
                        <Grid size={12}>
                            <QuantifiedItemPicker
                                unselectedItems={state.unselectedItems}
                                selectedItems={state.selectedItems}
                                getItemName={(item) => {
                                    const menuItem = itemsMap.get(item.menuItemId);
                                    if(menuItem == undefined) {
                                        return <Skeleton animation="wave" />
                                    }
                                    return menuItem.name;
                                }}
                                getItemKey={(item) => item.id}
                                unselectedLabel={getFromSourceItemsLabel()}
                                selectedLabel={getToTargetItemsLabel()}
                                onChanged={onItemsChanged}
                            />
                        </Grid>
                    )
                }
            </Grid>
        </CustomModal>
    )
}

const Dropdown = (props: {
    readonly isLoading?: boolean;
    readonly label: string;
    readonly selectedItem?: Channel,
    readonly items: Channel[],
    readonly onSelectionChanged: (selected: Channel) => void;
    readonly channelProfilesMap: Map<string, ChannelProfile>;
}) => {
    const { t } = useTranslation();

    const getChannelName = (channel: Channel) => {
        const profile = props.channelProfilesMap.get(channel.channelProfileId);
        if(profile == undefined) {
            return <Skeleton animation="wave" />
        }
        return `${profile.name} ${channel.name}`;
    }

    return (
        <FormControl fullWidth>
        {
            props.isLoading || props.selectedItem == undefined || props.items.length == 0
            ?
            <TextField
                label={props.label}
                value=""
                slotProps={{
                    input: {
                        readOnly: true,
                        disabled: true,
                        startAdornment: (
                            props.isLoading && <Skeleton animation="wave" sx={{width: "100%", height: "100%", m: 0, p: 0}}/>
                        ),
                        inputProps: {
                            style: {
                                width: 0,
                            }
                        }
                    }
                }}
            /> 
            :
            <Autocomplete
                options={props.items}
                groupBy={(channel) => channel.channelProfileId}
                renderValue={getChannelName}
                renderOption={(props, option) => (
                    <li {...props}>
                        {getChannelName(option)}
                    </li>
                )}
                renderInput={(params) => <TextField {...params} label={props.label} />}
                value={props.selectedItem}
                disabled={props.items.length == 1}
                noOptionsText={t("notFound")}
                onChange={(_, v) => v != undefined && props.onSelectionChanged(v)}
            />
        }
        </FormControl>
    );
};