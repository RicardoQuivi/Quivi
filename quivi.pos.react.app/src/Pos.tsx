import { Box, Grid, useMediaQuery, useTheme } from "@mui/material";
import { useTranslation } from "react-i18next";
import { useToast } from "./context/ToastProvider";
import useNoSleepMonitor from "./hooks/useNoSleepMonitor";
import { PosAppBar } from "./components/PosAppBar";
import { useEffect, useState } from "react";
import { ActiveTab, PosTabs } from "./components/PosTabs";
import useBrowserStorage, { BrowserStorageType } from "./hooks/useBrowserStorage";
import { useStoredState } from "./hooks/useStoredState";
import { ChannelsOverview } from "./components/ChannelsOverview";
import { EmployeeRestriction } from "./hooks/api/Dtos/employees/Employee";
import { Channel } from "./hooks/api/Dtos/channels/Channel";
import { ItemsSelector } from "./components/ItemsSelector";
import { MenuItem } from "./hooks/api/Dtos/menuitems/MenuItem";
import { usePosSession } from "./context/pos/PosSessionContextProvider";
import { NewOrderAudioPlayer } from "./components/Orders/NewOrderAudioPlayer";
import { SessionViewer } from "./components/Sessions/SessionViewer";
import { useConfigurableFieldsQuery } from "./hooks/queries/implementations/useConfigurableFieldsQuery";
import { EditItemPriceModel } from "./components/Sessions/EditSessionItemModal";
import { TransferSessionModal } from "./components/Sessions/TransferSessionModal";
import { OrdersOverview } from "./components/Orders/OrdersOverview";
import { SessionAdditionalFieldsModal } from "./components/SessionAdditionalFieldsModal";
import { useSessionAdditionalInformationsQuery } from "./hooks/queries/implementations/useSessionAdditionalInformationsQuery";

export const Pos = () => {
    const theme = useTheme();
    const pos = usePosSession();

    const isMobile = useMediaQuery(theme.breakpoints.only('xs'));
    
    const { t } = useTranslation();
    const toast = useToast();

    const [selectedCategoryId, setSelectedCategoryId] = useState<string>();
    const [transferChannel, setTransferChannel] = useState<Channel>();

    const { } = useNoSleepMonitor(true);

    const [searchTxt, setSearchTxt] = useState("");
    const [readQrCodeOpen, setReadQrCodeOpen] = useState(false);
    const [additionalInfoModalOpen, setAdditionalInfoModalOpen] = useState(false);

    const searchParamsHook = useBrowserStorage(BrowserStorageType.UrlParam);
    const [activeTab, setActiveTab] = useStoredState<ActiveTab>("tab", ActiveTab.ChannelSelection, searchParamsHook);
    const [localId, setLocalId] = useStoredState<string | undefined>("localId", undefined, searchParamsHook);
    const [orderId, setOrderId] = useStoredState<string | undefined>("orderId", undefined, searchParamsHook);

    const configurableFieldsQuery = useConfigurableFieldsQuery(!pos.cartSession.channelId ? undefined : {
        channelIds: [pos.cartSession.channelId],
        forPosSessions: true,
        page: 0,
    });
    const sessionAdditionalInfoQuery = useSessionAdditionalInformationsQuery(!pos.cartSession.sessionId || pos.cartSession.closedAt != undefined ? undefined : {
        sessionId: pos.cartSession.sessionId,
    })

    const hasChannelsWithSessions = pos.permissions.data.canViewSessions == true;
    
    const onTransferSessionClicked = (item: Channel) => {
        if(pos.employee.restrictions.find(p => p == EmployeeRestriction.TransferingItems) != undefined) {
            toast.error(t("employeeAccessDenied"))
            return;
        }

        setTransferChannel(item);
    }

    const onEditItemPrice = (item: EditItemPriceModel) => pos.cartSession.applyDiscount(item.item, item.quantityToApply, item.newDiscount, item.priceOverride);

    const onChannelClicked = (channelId: string, goToTab: boolean) => {
        pos.changeToSession(channelId);
        if(goToTab) {
            setActiveTab(ActiveTab.ItemSelection);
        }
    }

    const onItemAdded = async (item: MenuItem) => {
        setSearchTxt("");
        pos.cartSession.addItem(item, 1);
    }

    useEffect(() => console.log(configurableFieldsQuery.data), [configurableFieldsQuery.data])
    useEffect(() => console.log(pos.cartSession.channelId ), [pos.cartSession.channelId])

    useEffect(() => {
        if(!pos.cartSession.sessionId) {
            return;
        }

        if(pos.cartSession.closedAt != undefined) {
            return;
        }

        if(sessionAdditionalInfoQuery.isLoading || sessionAdditionalInfoQuery.data.length > 0) {
            return;
        }

        if(configurableFieldsQuery.isFirstLoading || configurableFieldsQuery.data.length == 0) {
            return;
        }

        setAdditionalInfoModalOpen(true);
    }, [pos.cartSession, sessionAdditionalInfoQuery, configurableFieldsQuery]);

    return (
        <Box style={{height: "100dvh", width: "100dvw", overflow: "hidden", display: "flex", flexDirection: "column"}}>
            <Box style={{flex: "0 0 auto"}}>
                <PosAppBar
                    search={searchTxt}
                    onSearchChanged={setSearchTxt}
                    onNotificationClicked={() => setActiveTab(ActiveTab.Notifications)}
                    localId={localId}
                    onNewLocalSelected={setLocalId}
                    onReadQrCodeButtonClicked={() => setReadQrCodeOpen(true)}
                />
            </Box>
            <Grid container sx={{ overflow: "hidden", flex: "1 1 auto"}}>
                <Grid size={{xs: 12, sm: hasChannelsWithSessions ? 8 : 12 }} style={{height: "100%", display: "flex", flexDirection: "column", padding: isMobile ? 0 : "1rem"}}>
                    <Box style={{flex: "0 0 auto"}}>
                        <PosTabs
                            hasChannelsWithSessions={hasChannelsWithSessions}
                            isMobile={isMobile}
                            onTabIndexChanged={setActiveTab}
                            tab={activeTab}
                            localId={localId}
                        />
                    </Box>
                    <Box style={{flex: "1 1 auto", marginTop: isMobile ? 0 : "1rem", order: isMobile ? -1 : 0}}>
                        {
                            hasChannelsWithSessions && activeTab == ActiveTab.ItemSelection &&
                            <ItemsSelector
                                search={searchTxt}
                                onItemSelect={onItemAdded}
                                selectedCategoryId={selectedCategoryId}
                                onCategoryChanged={(category) => setSelectedCategoryId(category?.id)}
                            />
                        }
                        {
                            hasChannelsWithSessions && activeTab == ActiveTab.ChannelSelection &&
                            <ChannelsOverview
                                search={searchTxt}
                                onChannelClicked={q => onChannelClicked(q.id, true)}
                                onTransferSessionClicked={onTransferSessionClicked}
                            />
                        }
                        {
                            activeTab == ActiveTab.Ordering &&
                            <OrdersOverview
                                onOrderSelected={setOrderId}
                                selectedOrderId={orderId}
                                localId={localId}
                                readQrCodeOpen={readQrCodeOpen}
                                onReadQrCodeClosed={() => setReadQrCodeOpen(false)}
                                onOrderUpdated={(id) => onChannelClicked(id, false)}
                            />
                        }
                        {/*
                        {
                            activeTab == ActiveTab.Notifications &&
                            <NotificationCenter />
                        }
                        */}
                        
                        {
                            hasChannelsWithSessions && isMobile && activeTab == ActiveTab.SessionOverview &&
                            <Box style={{padding: "0.5rem", height: "100%"}}>
                                <SessionViewer
                                    onTransferSessionClicked={onTransferSessionClicked}
                                    onEditItem={onEditItemPrice}
                                    onSessionAdditionalInfoClicked={configurableFieldsQuery.data.length == 0 ? undefined : () => setAdditionalInfoModalOpen(true)}
                                    localId={localId}
                                />
                            </Box>
                        }
                    </Box>
                </Grid>
                {
                    hasChannelsWithSessions && isMobile == false &&
                    <Grid size={{xs: 12, sm: 4}} sx={{padding: "1rem", height: "100%"}}>
                        <SessionViewer
                            onTransferSessionClicked={onTransferSessionClicked}
                            onEditItem={onEditItemPrice}
                            onSessionAdditionalInfoClicked={configurableFieldsQuery.data.length == 0 ? undefined : () => setAdditionalInfoModalOpen(true)}
                            localId={localId}
                        />
                    </Grid>
                }
            </Grid>
            <TransferSessionModal
                currentChannel={transferChannel}
                onClose={() => setTransferChannel(undefined)}
            />
            <SessionAdditionalFieldsModal
                fields={configurableFieldsQuery.data}
                additionalInfo={sessionAdditionalInfoQuery.data}
                isOpen={additionalInfoModalOpen}
                onClose={() => setAdditionalInfoModalOpen(false)}
            />
            <NewOrderAudioPlayer />
            {/* <PrinterFailureManager /> */}
        </Box>
    )
}