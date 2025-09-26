import { Box, Grid, useMediaQuery } from "@mui/material";
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
import { ItemsSelector } from "./components/Items/ItemsSelector";
import { usePosSession } from "./context/pos/PosSessionContextProvider";
import { SessionViewer } from "./components/Sessions/SessionViewer";
import { useConfigurableFieldsQuery } from "./hooks/queries/implementations/useConfigurableFieldsQuery";
import { EditItemPriceModel } from "./components/Sessions/EditSessionItemModal";
import { TransferSessionModal } from "./components/Sessions/TransferSessionModal";
import { OrdersOverview } from "./components/Orders/OrdersOverview";
import { SessionAdditionalFieldsModal } from "./components/SessionAdditionalFieldsModal";
import { useSessionAdditionalInformationsQuery } from "./hooks/queries/implementations/useSessionAdditionalInformationsQuery";
import { useNewOrderAudio } from "./hooks/useNewOrderAudio";

export const Pos = () => {
    useNewOrderAudio();
    useNoSleepMonitor(true);

    const xs = useMediaQuery(t => t.breakpoints.only('xs'));
    const pos = usePosSession();

    const { t } = useTranslation();
    const toast = useToast();

    const [selectedCategoryId, setSelectedCategoryId] = useState<string | undefined | null>(undefined);
    const [transferChannel, setTransferChannel] = useState<Channel>();

    const [searchTxt, setSearchTxt] = useState("");
    const [readQrCodeOpen, setReadQrCodeOpen] = useState(false);
    const [additionalInfoModalOpen, setAdditionalInfoModalOpen] = useState(false);

    const searchParamsHook = useBrowserStorage(BrowserStorageType.UrlParam);
    const [activeTab, setActiveTab] = useStoredState<ActiveTab>("tab", () => ActiveTab.ChannelSelection, searchParamsHook);
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
    }, [
        pos.cartSession.sessionId, pos.cartSession.closedAt,
        sessionAdditionalInfoQuery.isLoading, sessionAdditionalInfoQuery.data.length, 
        configurableFieldsQuery.isFirstLoading, configurableFieldsQuery.data.length,
    ]);

    return (
        <Box 
            sx={{
                height: "100dvh",
                width: "100dvw",
                overflow: "hidden",
                display: "flex",
                flexDirection: "column",
                background: p => p.palette.primary.light,
            }}
        >
            <Box 
                sx={{
                    flex: 0,
                }}
            >
                <PosAppBar
                    search={searchTxt}
                    onSearchChanged={setSearchTxt}
                    onNotificationClicked={() => setActiveTab(ActiveTab.Notifications)}
                    localId={localId}
                    onNewLocalSelected={setLocalId}
                    onReadQrCodeButtonClicked={() => setReadQrCodeOpen(true)}
                />
            </Box>

            <Grid 
                container
                sx={{
                    overflow: "hidden",
                    flex: 1,
                    padding: {
                        xs: 0,
                        sm: "1rem",
                    }
                }}
                spacing={2}
            >
                {
                    hasChannelsWithSessions &&
                    xs == false &&
                    <Grid
                        size={{
                            xs: 12,
                            sm: 4,
                        }}
                        sx={{
                            height: "100%",
                        }}
                    >
                        <SessionViewer
                            onTransferSessionClicked={onTransferSessionClicked}
                            onEditItem={onEditItemPrice}
                            onSessionAdditionalInfoClicked={configurableFieldsQuery.data.length == 0 ? undefined : () => setAdditionalInfoModalOpen(true)}
                            localId={localId}
                        />
                    </Grid>
                }

                <Grid 
                    size={{
                        xs: 12, 
                        sm: hasChannelsWithSessions ? 8 : 12
                    }}
                    sx={{
                        height: "100%",
                        display: "flex",
                        flexDirection: "column",
                    }}
                >
                    <PosTabs
                        hasChannelsWithSessions={hasChannelsWithSessions}
                        onTabIndexChanged={setActiveTab}
                        tab={activeTab}
                        localId={localId}
                        sx={{
                            flex: 0,
                        }}
                    />

                    <Box
                        sx={{
                            flex: 1,
                            marginTop: {
                                xs: 0,
                                sm: "1rem",
                            },
                            order: {
                                xs: -1,
                                sm: 0,
                            },
                            padding: {
                                xs: "0.5rem 0.5rem 0rem 0.5rem",
                                sm: 0,
                            },
                            overflowY: "hidden",
                        }}
                    >
                        {
                            hasChannelsWithSessions && activeTab == ActiveTab.ItemSelection &&
                            <ItemsSelector
                                search={searchTxt}
                                onItemSelect={item => {
                                    setSearchTxt("");
                                    pos.cartSession.addItem(item, 1);
                                }}
                                selectedCategoryId={selectedCategoryId}
                                onCategoryChanged={(category) => {
                                    if(category !== null)  {
                                        setSearchTxt("");
                                    }
                                    setSelectedCategoryId(() => {
                                        if(category === null) {
                                            return null;
                                        }

                                        if(category === undefined) {
                                            return undefined;
                                        }
                                        
                                        return category.id;
                                    });
                                }}
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
                        
                        {
                            hasChannelsWithSessions &&
                            activeTab == ActiveTab.SessionOverview &&
                            <Box
                                sx={{
                                    height: "100%",
                                    display: {
                                        xs: "block",
                                        sm: "hidden"
                                    }
                                }}
                            >
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
            {/* <PrinterFailureManager /> */}
        </Box>
    )
}