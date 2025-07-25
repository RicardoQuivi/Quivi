import React, { useEffect, useMemo, useState } from "react"
import { Box, Button, ButtonGroup, Card, CardActionArea, CardActions, CardHeader, Chip, Divider, Grid, IconButton, Paper, Skeleton, SxProps, Theme, Tooltip, useMediaQuery, useTheme } from "@mui/material";
import { Trans, useTranslation } from "react-i18next";
import { Channel } from "../hooks/api/Dtos/channels/Channel";
import { useStoredState } from "../hooks/useStoredState";
import useBrowserStorage, { BrowserStorageType } from "../hooks/useBrowserStorage";
import { useChannelsQuery } from "../hooks/queries/implementations/useChannelsQuery";
import { usePosIntegrationsQuery } from "../hooks/queries/implementations/usePosIntegrationsQuery";
import { PosIntegration } from "../hooks/api/Dtos/posintegrations/PosIntegration";
import { useChannelProfilesQuery } from "../hooks/queries/implementations/useChannelProfilesQuery";
import { ChannelProfile } from "../hooks/api/Dtos/channelProfiles/ChannelProfile";
import CurrencySpan from "./Currency/CurrencySpan";
import { CloseIcon, GridIcon, LayersIcon, QrCodeIcon, SwitchIcon } from "../icons";
import { PaginationFooter } from "./Pagination/PaginationFooter";
import { useEmployeesQuery } from "../hooks/queries/implementations/useEmployeesQuery";
import { useAllowedActions } from "../hooks/pos/useAllowedActions";
import { EmployeeAvatar } from "./Employees/EmployeeAvatar";
import CustomModal, { ModalSize } from "./Modals/CustomModal";
import LoadingButton from "./Buttons/LoadingButton";
import { useSessionsQuery } from "../hooks/queries/implementations/useSessionsQuery";
import { Session } from "../hooks/api/Dtos/sessions/Session";
import { useCartSession } from "../hooks/pos/session/useCartSession";
import { usePosSession } from "../context/pos/PosSessionContextProvider";
import { Items } from "../helpers/itemsHelpers";

enum QrCodesViewType {
    Grid,
    Map,
}

interface Props {
    readonly search: string;
    readonly onChannelClicked: (channel: Channel) => any;
    readonly onTransferSessionClicked: (channel: Channel) => any;
}
export const ChannelsOverview: React.FC<Props> = ({
    search,
    onChannelClicked,
    onTransferSessionClicked,
}) => {
    const theme = useTheme();
    const xs = useMediaQuery(theme.breakpoints.only('xs'));
    const posContext = usePosSession();
    const browserStorage = useBrowserStorage(BrowserStorageType.LocalStorage);
    const searchParamsStorage = useBrowserStorage(BrowserStorageType.UrlParam);

    const canTransferSession = posContext.cartSession.isSyncing == false;
    const [channelProfileId, setChannelProfileId] = useStoredState<string | undefined>("channelProfileId", browserStorage.getItem("channelProfileId"), searchParamsStorage);
    const [channelsViewType, setQrCodesViewType] = useStoredState<QrCodesViewType | undefined>("channelView", !xs ? (browserStorage.getItem("channelView") ?? QrCodesViewType.Grid) : QrCodesViewType.Grid, searchParamsStorage);
    const [currentPage, setCurrentPage] = useState(0);
    const channelProfilesQuery = useChannelProfilesQuery({
        allowsSessionsOnly: true,
        page: 0,
    });
    const channelsQuery = useChannelsQuery(channelProfileId == undefined ? undefined : {
        page: currentPage, 
        pageSize: 48,
        includePageRanges: true, 
        search: search, 
        allowsSessionsOnly: true,
        channelProfileId: channelProfileId,
    });

    const sessionsQuery = useSessionsQuery(channelsQuery.data.length == 0 ? undefined : {
        channelIds: channelsQuery.data.map(q => q.id),
        page: 0,
        isOpen: true,
    });
    const integrationsQuery = usePosIntegrationsQuery(channelsQuery.isFirstLoading ? undefined : {
        page: 0,
    });

    const [sessionToClose, setSessionToClose] = useState<{ channel: Channel, profile: ChannelProfile }>();

    const profilesMap = useMemo(() => channelProfilesQuery.data.reduce((r, s) => {
        r.set(s.id, s);
        return r;
    }, new Map<string, ChannelProfile>()), [channelProfilesQuery.data])

    const sessionsMap = useMemo(() => sessionsQuery.data.reduce((r, s) => {
        r.set(s.channelId, s);
        return r;
    }, new Map<string, Session>()), [sessionsQuery.data])
    
    const integrationsMap = useMemo(() => integrationsQuery.data.reduce((r, s) => {
        r.set(s.id, s);
        return r;
    }, new Map<string, PosIntegration>()), [integrationsQuery.data])
    
    useEffect(() => {
        if (xs)
            setQrCodesViewType(QrCodesViewType.Grid);
        else
            setQrCodesViewType(browserStorage.getItem<QrCodesViewType>("channelView") ?? QrCodesViewType.Grid);
    }, [xs]);

    useEffect(() => {
        if (!channelProfileId) {
            return;
        }
        browserStorage.setItem("channelProfileId", channelProfileId);
    }, [channelProfileId]);

    useEffect(() => {
        if (channelsViewType == undefined || xs) {
            return;
        }
        browserStorage.setItem("qrCodeView", channelsViewType);
    }, [channelsViewType]);

    const getUnpaidTotal = (session: Session) => Items.getTotalPrice(session.items.filter(e => !e.isPaid));

    useEffect(() => setCurrentPage(0), [])

    useEffect(() => {
        if(profilesMap.size == 0) {
            setChannelProfileId(undefined);
            return;
        }

        const profile = channelProfileId == undefined ? profilesMap.values().next().value : profilesMap.get(channelProfileId);        
        setChannelProfileId(profile?.id)
    }, [profilesMap]);

    return (
        <Box sx={{height: "100%", overflow: "hidden", display: "flex", flexDirection: "column"}}>
            <Grid container width="100%" spacing={2}>
                {
                    !xs &&
                    <Grid size="auto">
                        <ButtonGroup variant="outlined" sx={{alignSelf: "center", justifySelf: "flex-end"}}>
                            <Button variant={channelsViewType == QrCodesViewType.Grid ? "contained" : "outlined"} onClick={() => setQrCodesViewType(QrCodesViewType.Grid)}>
                                <GridIcon />
                            </Button>
                            <Button variant={channelsViewType == QrCodesViewType.Map ? "contained" : "outlined"} onClick={() => setQrCodesViewType(QrCodesViewType.Map)}>
                                <LayersIcon />
                            </Button>
                        </ButtonGroup>
                    </Grid>
                }
                {
                    channelsViewType == QrCodesViewType.Grid &&
                    !channelProfilesQuery.isFirstLoading &&
                    channelProfilesQuery.data.map(profile => 
                        <Grid size="grow" key={profile.id}>
                            <Chip
                                sx={{width: "100%"}}
                                label={profile.name}
                                color="primary"
                                variant={channelProfileId == profile.id ? "filled" : "outlined"}
                                onClick={() => setChannelProfileId(profile.id)}
                            />
                        </Grid>
                    )
                }
            </Grid>
            {
                channelsViewType == QrCodesViewType.Grid &&
                <>
                    <Box style={{flex: "1 1 auto", overflow: "auto", padding: "0.5rem 0.5rem", width: "100%", display: "flex", flexDirection: "column", flexWrap: "wrap", alignContent: "center"}}>
                        <Grid container spacing={2} sx={{width: "100%"}}>
                            {
                                channelsQuery.isFirstLoading == false
                                ?
                                channelsQuery.data.map(channel => {
                                    const session = sessionsMap.get(channel.id);
                                    const profile = profilesMap.get(channel.channelProfileId);
                                    const integration = profile == undefined ? undefined : integrationsMap.get(profile.posIntegrationId);
                                    return <Grid size={{xs: 6, sm: 3, md: 2}} key={channel.id}>
                                        <ChannelCard
                                            cardProps={{ backgroundColor: session?.isOpen == true ? "rgba(255, 0, 0, 0.1)" : "rgba(0, 255, 0, 0.1)" }}
                                            subtitle={
                                                sessionsQuery.isLoading
                                                ?
                                                    <Skeleton animation="wave"/>
                                                :
                                                    <CurrencySpan value={session == undefined ? 0 : getUnpaidTotal(session)} />
                                            }
                                            employeeId={session?.isOpen == true ? session.employeeId : undefined}
                                            disableTransfer={!canTransferSession}
                                            channel={channel}
                                            channelProfile={profile}
                                            session={session}
                                            integration={integration}

                                            onCardClicked={() => onChannelClicked(channel)}
                                            onQrCodeUrlClicked={() => window.open(channel.url, "_blank")}
                                            onTransferQrCodeSessionClicked={sessionsQuery.isLoading ? undefined : () => onTransferSessionClicked(channel)}
                                            onCloseSessionClicked={session == undefined || profile == undefined ? undefined : () => setSessionToClose({
                                                channel: channel,
                                                profile: profile,
                                            })}
                                        />
                                    </Grid>
                                })
                                :
                                    [1, 2, 3, 4, 5, 6].map(i => <Grid size={{xs: 4, sm: 3, md: 2}} key={i}>
                                        <ChannelCard subtitle={<Skeleton animation="wave" />} />
                                    </Grid>)
                            }
                        </Grid>
                    </Box>
                    {
                        channelsQuery.totalPages > 1 &&
                        <Box style={{flex: "0 0 auto"}}>
                            <Divider />
                            <PaginationFooter currentPage={currentPage} onPageChanged={setCurrentPage} numberOfPages={channelsQuery.totalPages} />
                        </Box>
                    }
                </>
            }
            {/* {
                channelsViewType == QrCodesViewType.Map &&
                <Paper sx={{width: "100%", height: "82vh", mt: 2}}>
                    <QrCodeSessionsMap 
                        style={{width: "100%", height: "82vh"}}
                        isReadonly
                        showStatusColor
                        onItemClick={onQrCodeSessionClicked} 
                    />
                </Paper>
            } */}
            <CloseSessionModal
                isOpen={!!sessionToClose}
                channel={sessionToClose?.channel}
                channelProfile={sessionToClose?.profile}
                onClose={() => setSessionToClose(undefined)}
            />
        </Box>
    )
}

const ChannelCard = (props: { 
    readonly cardProps?: SxProps<Theme>,
    readonly subtitle: React.ReactNode,
    readonly employeeId?: string,
    readonly disableTransfer?: boolean,
    readonly channel?: Channel;
    readonly channelProfile?: ChannelProfile;
    readonly session?: Session, 
    readonly integration?: PosIntegration,
    readonly onCardClicked?: () => any,
    readonly onQrCodeUrlClicked?: () => any,
    readonly onTransferQrCodeSessionClicked?: () => any,
    readonly onCloseSessionClicked?: () => any;
}) => {
    const { t } = useTranslation();

    const sessionEmployeesQuery = useEmployeesQuery(props.employeeId == undefined ? undefined : {
        ids: [props.employeeId],
        page: 1,
    })
    const sessionEmployee = sessionEmployeesQuery.data.length > 0 ? sessionEmployeesQuery.data[0] : undefined;

    const permissionsQuery = useAllowedActions(props.channel?.id);

    return (
        <Paper elevation={16}>
            <Card sx={props.cardProps}>
                <CardActionArea onClick={props.onCardClicked}>
                    <Grid
                        container
                    >
                        <Grid size="grow">
                            <CardHeader
                                title={props.channel == undefined || props.channelProfile == undefined 
                                    ? 
                                        <Skeleton animation="wave" />
                                    : 
                                    `${props.channelProfile.name} ${props.channel.name}`
                                }
                                subheader={props.subtitle}
                                slotProps={{
                                    title: {
                                        fontSize: "1rem",
                                    },
                                }}
                            />
                        </Grid>

                        {
                            sessionEmployee != undefined &&
                            <Grid size="auto">
                                <Box sx={{marginRight: "0.25rem"}} height="100%" display="flex" alignItems="center">
                                    <Tooltip title={sessionEmployee.name} style={{ float: "right" }}>
                                        <EmployeeAvatar employee={sessionEmployee} />
                                    </Tooltip>
                                </Box>
                            </Grid>
                        }
                    </Grid>
                </CardActionArea>
                <Divider />
                {
                    <CardActions sx={{display: "flex", flexDirection: "row", alignItems: "center", justifyContent: "space-between"}}>
                        <Tooltip title={t("openLink")}>
                            <IconButton size="large" onClick={props.onQrCodeUrlClicked}>
                                <QrCodeIcon style={{height: "16", aspectRatio: 1}} />
                            </IconButton>
                        </Tooltip>
                        {
                            permissionsQuery.data.allowsAddingItems != false &&
                            <>
                                {
                                    permissionsQuery.data.allowsRemovingItems == true &&
                                    props.session?.isOpen !== false &&
                                    props.onCloseSessionClicked != undefined &&
                                    <Tooltip title={t("closeSession")}>
                                        <IconButton size="large" onClick={props.onCloseSessionClicked}>
                                            <CloseIcon style={{height: "16", aspectRatio: 1}} />
                                        </IconButton>
                                    </Tooltip>
                                }
                                <Tooltip title={t("transferSession")}>
                                    <IconButton size="large" disabled={!!props.disableTransfer} onClick={props.onTransferQrCodeSessionClicked}>
                                        <SwitchIcon style={{height: "16", aspectRatio: 1}}/>
                                    </IconButton>
                                </Tooltip>
                            </>
                        }
                    </CardActions>
                }
            </Card>
        </Paper>
    );
}

interface CloseSessionModalState {
    readonly isOpen: boolean;
    readonly channel: Channel;
}

interface CloseSessionModalProps {
    readonly isOpen: boolean;
    readonly channel?: Channel;
    readonly channelProfile?: ChannelProfile;
    readonly onClose: () => any;
}
const CloseSessionModal = (props: CloseSessionModalProps) => {
    const { t } = useTranslation();
    
    const [state, setState] = useState<CloseSessionModalState>();
    const session = useCartSession(state?.channel.id)
    const [isLoading, setIsLoading] = useState(false);

    useEffect(() => {
        if(props.isOpen) {
            if(props.channel != undefined) {
                setState({
                    isOpen: true,
                    channel: props.channel,
                })
                return;
            }
            props.onClose();
            return;
        }

        setState(s => {
            if(s == undefined) {
                return s;
            }

            return {
                ...s,
                isOpen: false,
            }
        })
    }, [props.isOpen, props.channel])

    useEffect(() => {
        if(isLoading == false) {
            return;
        }

        session?.forceSync().then(() => {
            setIsLoading(false);
            props.onClose();
        })
    }, [isLoading])

    if(state == undefined) {
        return <></>
    }

    return (
        <CustomModal
            {...props}
            isOpen={props.isOpen}
            title={t("closeSession")}
            onClose={props.onClose}
            size={ModalSize.Default}
            footer={
                <Grid container sx={{width: "100%", margin: "1rem 0.25rem"}} spacing={1}>
                    <Grid size="grow">
                        <LoadingButton isLoading={false} onClick={props.onClose} style={{width: "100%"}}>
                            {t("close")}
                        </LoadingButton>
                    </Grid>
                    <Grid size="grow">
                        <LoadingButton
                            isLoading={isLoading || session == undefined}
                            primaryButton 
                            onClick={async () => {
                                if(session == undefined) {
                                    return;
                                }

                                for(const item of session.items) {
                                    session.removeItem(item, item.quantity);
                                }
                                setIsLoading(true);
                            }}
                            style={{width: "100%"}}
                        >
                            {t("confirm")}
                        </LoadingButton>
                    </Grid>
                </Grid>
            }
        >
            <Trans
                t={t}
                i18nKey="closeSessionQuestion"
                shouldUnescape={true}
                values={{
                    name: `${props.channelProfile?.name} ${props.channel?.name}`
                }}
                components={{
                    bold: <b/>,
                }}
            />
    </CustomModal>
    )
}