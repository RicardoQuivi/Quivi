import React, { useEffect, useMemo, useState } from "react"
import { Box, Button, ButtonGroup, Card, CardActionArea, CardActions, CardHeader, Divider, Grid, IconButton, Skeleton, SxProps, Theme, Tooltip, useMediaQuery, useTheme } from "@mui/material";
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
import { CrossIcon, GridIcon, LayersIcon, QrCodeIcon, SwapIcon } from "../icons";
import { PaginationFooter } from "./Pagination/PaginationFooter";
import { useEmployeesQuery } from "../hooks/queries/implementations/useEmployeesQuery";
import { useAllowedActions } from "../hooks/pos/useAllowedActions";
import { EmployeeAvatar } from "./Avatars/EmployeeAvatar";
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

const getUnpaidTotal = (session: Session) => Items.getTotalPrice(session.items.filter(e => !e.isPaid));

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
    const [channelProfileId, setChannelProfileId] = useStoredState<string | undefined>("channelProfileId", () => browserStorage.getItem("channelProfileId"), searchParamsStorage);
    const [channelsViewType, setQrCodesViewType] = useStoredState<QrCodesViewType | undefined>("channelView", () => !xs ? (browserStorage.getItem("channelView") ?? QrCodesViewType.Grid) : QrCodesViewType.Grid, searchParamsStorage);
    const [currentPage, setCurrentPage] = useState(0);
    const [sessionToClose, setSessionToClose] = useState<{ channel: Channel, profile: ChannelProfile }>();

    const channelProfilesQuery = useChannelProfilesQuery({
        allowsSessionsOnly: true,
        page: 0,
    });
    const channelsQuery = useChannelsQuery(channelProfileId == undefined ? undefined : {
        page: currentPage, 
        pageSize: 48,
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
        if (xs) {
            setQrCodesViewType(QrCodesViewType.Grid);
        } else {
            setQrCodesViewType(browserStorage.getItem<QrCodesViewType>("channelView") ?? QrCodesViewType.Grid);
        }
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
        browserStorage.setItem("channelView", channelsViewType);
    }, [channelsViewType]);
    
    useEffect(() => {
        if(profilesMap.size == 0) {
            setChannelProfileId(undefined);
            return;
        }

        const profile = channelProfileId == undefined ? profilesMap.values().next().value : profilesMap.get(channelProfileId);        
        setChannelProfileId(profile?.id)
    }, [profilesMap]);

    return (
        <Box 
            sx={{
                height: "100%",
                overflow: "hidden",
                display: "flex",
                flexDirection: "column"
            }}
        >
            <Grid
                container
                width="100%"
                spacing={1}
            >
                {
                    !xs &&
                    <Grid
                        size="auto"
                    >
                        <ButtonGroup
                            variant="outlined"
                            sx={{
                                alignSelf: "center",
                                justifySelf: "flex-end",
                                height: "100%",
                            }}
                        >
                            <Button
                                variant={channelsViewType == QrCodesViewType.Grid ? "contained" : "outlined"}
                                onClick={() => setQrCodesViewType(QrCodesViewType.Grid)}
                            >
                                <GridIcon />
                            </Button>
                            <Button
                                variant={channelsViewType == QrCodesViewType.Map ? "contained" : "outlined"}
                                onClick={() => setQrCodesViewType(QrCodesViewType.Map)}
                            >
                                <LayersIcon />
                            </Button>
                        </ButtonGroup>
                    </Grid>
                }
                {
                    channelsViewType == QrCodesViewType.Grid &&
                    channelProfilesQuery.data.map(profile =>
                        <Grid size="grow" key={profile.id}>
                            <Button
                                sx={{
                                    width: "100%",
                                }}
                                children={profile.name}
                                variant={channelProfileId == profile.id ? "contained" : "outlined"}
                                onClick={() => setChannelProfileId(profile.id)}
                            />
                        </Grid>
                    )
                }
            </Grid>
            
            {
                channelsViewType == QrCodesViewType.Grid &&
                <>
                    <Box
                        sx={{
                            flex: 1,
                            overflow: "auto",
                            width: "100%",
                            display: "flex",
                            flexDirection: "column",
                            flexWrap: "wrap",
                            alignContent: "center",
                            mt: "1rem",
                        }}
                    >
                        <Grid
                            container
                            spacing={2}
                            sx={{
                                width: "100%",
                                padding: "1rem",
                            }}
                            gap={2}
                        >
                            {
                                channelsQuery.isFirstLoading == false
                                ?
                                channelsQuery.data.map(channel => {
                                    const session = sessionsMap.get(channel.id);
                                    const profile = profilesMap.get(channel.channelProfileId);
                                    const integration = profile == undefined ? undefined : integrationsMap.get(profile.posIntegrationId);
                                    return <Grid
                                        key={channel.id}
                                        size={{
                                            xs: 6,
                                            sm: 4,
                                            md: 3,
                                            lg: 3,
                                            xl: 2,
                                        }}
                                    >
                                        <ChannelCard
                                            cardProps={{
                                                backgroundColor: t => session?.isOpen == true ? t.palette.error.dark : undefined,
                                                borderColor: t => t.palette.error.main,
                                            }}
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
                                            onChannelUrlClicked={() => window.open(channel.url, "_blank")}
                                            onTransferQrCodeSessionClicked={sessionsQuery.isLoading ? undefined : () => onTransferSessionClicked(channel)}
                                            onCloseSessionClicked={session == undefined || profile == undefined ? undefined : () => setSessionToClose({
                                                channel: channel,
                                                profile: profile,
                                            })}
                                        />
                                    </Grid>
                                })
                                :
                                    [1, 2, 3, 4, 5, 6].map(i => <Grid size={{xs: 6, sm: 3, md: 2}} key={i}>
                                        <ChannelCard subtitle={<Skeleton animation="wave" />} />
                                    </Grid>)
                            }
                        </Grid>
                    </Box>
                    {
                        channelsQuery.totalPages > 1 &&
                        <Box
                            sx={{flex: "0 0 auto"}}
                        >
                            <Divider />
                            <PaginationFooter
                                currentPage={currentPage}
                                onPageChanged={setCurrentPage}
                                numberOfPages={channelsQuery.totalPages}
                            />
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
    readonly onChannelUrlClicked?: () => any,
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
        <Card
            sx={{
                ...(props.cardProps ?? {}),
                height: "100%",
                display: "flex",
                flexDirection: "column",
            }}
            elevation={8}
        >
            <CardActionArea
                onClick={props.onCardClicked}
                sx={{
                    flex: 1,
                }}
            >
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
                <CardActions
                    sx={{
                        display: "flex",
                        flexDirection: "row",
                        justifyContent: "space-between",
                        flex: 0,
                    }}
                >
                    <Tooltip title={t("openLink")}>
                        <IconButton
                            size="medium"
                            onClick={props.onChannelUrlClicked}
                        >
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
                                    <IconButton
                                        size="medium"
                                        onClick={props.onCloseSessionClicked}
                                    >
                                        <CrossIcon style={{height: "16", aspectRatio: 1}} />
                                    </IconButton>
                                </Tooltip>
                            }
                            <Tooltip title={t("transferSession")}>
                                <IconButton size="medium" disabled={!!props.disableTransfer} onClick={props.onTransferQrCodeSessionClicked}>
                                    <SwapIcon style={{height: "16", aspectRatio: 1}}/>
                                </IconButton>
                            </Tooltip>
                        </>
                    }
                </CardActions>
            }
        </Card>
    );
}

interface CloseSessionModalState {
    readonly isOpen: boolean;
    readonly channel: Channel;
    readonly channelProfile: ChannelProfile;
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
        if(props.isOpen == false) {
            setState(s => {
                if(s == undefined) {
                    return s;
                }

                return {
                    ...s,
                    isOpen: false,
                }
            })
            return;
        }

        if(props.channel != undefined && props.channelProfile != undefined) {
            setState({
                isOpen: true,
                channel: props.channel,
                channelProfile: props.channelProfile,
            })
            return;
        }

        props.onClose();
    }, [props.isOpen, props.channel, props.channelProfile])

    useEffect(() => {
        if(isLoading == false) {
            return;
        }

        if(session.isSyncing) {
            session.forceSync();
            return;
        }

        setIsLoading(false);
        props.onClose();
    }, [isLoading, session.isSyncing, session.forceSync])

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
                <Grid
                    container
                    sx={{
                        width: "100%",
                        margin: "1rem 0.25rem",
                    }}
                    spacing={1}
                >
                    <Grid size="grow">
                        <LoadingButton
                            isLoading={false}
                            onClick={props.onClose}
                            style={{width: "100%"}}
                            disabled={isLoading}
                        >
                            {t("close")}
                        </LoadingButton>
                    </Grid>
                    <Grid size="grow">
                        <LoadingButton
                            isLoading={isLoading}
                            primaryButton 
                            onClick={() => {
                                for(const item of session.items) {
                                    session.removeItem(item, item.quantity);
                                }
                                setIsLoading(true);
                            }}
                            style={{
                                width: "100%",
                            }}
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
                    name: `${state.channelProfile?.name} ${state.channel?.name}`
                }}
                components={{
                    bold: <b/>,
                }}
            />
    </CustomModal>
    )
}