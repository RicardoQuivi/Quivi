import { useEffect, useState } from "react"
import { Avatar, Box, Chip, Grid, } from "@mui/material";
import { useTranslation } from "react-i18next";
import { OrderState } from "../../hooks/api/Dtos/orders/OrderState";
import { useOrdersQuery } from "../../hooks/queries/implementations/useOrdersQuery";
import { usePreparationGroupsQuery } from "../../hooks/queries/implementations/usePreparationGroupsQuery";
import { SortDirection } from "../../hooks/api/Dtos/SortableRequest";
import { Local } from "../../hooks/api/Dtos/locals/Local";
import { PreparationGroupsQueueCards } from "./groups/PreparationGroupsQueueCards";
import { CommitedPreparationGroupsQueueCards } from "./groups/CommitedPreparationGroupsQueueCards";
import { OrdersQueueCards } from "./OrdersQueueCards";
import { OrdersHistory } from "./OrdersHistory";
import { OrderDetailModal } from "./OrderDetailModal";

const newTabStates = [OrderState.PendingApproval, OrderState.ScheduledRequested];
const processingStates = [OrderState.Accepted, OrderState.Processing];
const historyStates = [OrderState.Rejected, OrderState.Completed];
const scheduledStates = [OrderState.Scheduled];
const preparingStates: OrderState[] = [];

interface Props {
    readonly selectedOrderId?: string;
    readonly onOrderSelected: (orderId?: string) => any;
    readonly localId: string | undefined;
    readonly onOrderUpdated: (channelId: string) => any;

    readonly readQrCodeOpen: boolean;
    readonly onReadQrCodeClosed: () => any;
}
export const OrdersOverview = (props: Props) => {
    const { t } = useTranslation();
    
    const [filters, setFilters] = useState({
        states: newTabStates,
        selectedLocation: undefined as Local | undefined,
    })
    const requestedCountQuery = useOrdersQuery({
        page: 0,
        pageSize: 0,
        states: newTabStates,
        sortDirection: SortDirection.Asc,
    });
    const processingCountQuery = usePreparationGroupsQuery({
        locationId: props.localId,
        isCommited: false,
        page: 0,
        pageSize: 0,
    });
    const preparingCountQuery = usePreparationGroupsQuery({
        locationId: props.localId,
        isCommited: true,
        page: 0,
        pageSize: 0,
    });
    const scheduledCountQuery = useOrdersQuery({
        page: 0,
        pageSize: 0,
        states: scheduledStates,
        sortDirection: SortDirection.Asc,
    });
    
    const [selectedOrder, setSelectedOrder] = useState({
        id: undefined as (string | undefined),
        wasAlreadyRedeemed: false,
    });

    useEffect(() => setSelectedOrder(s => {
        if(s.id == props.selectedOrderId) {
            return s;
        }

        return {
            id: props.selectedOrderId,
            wasAlreadyRedeemed: false,
        }
    }), [props.selectedOrderId])

    const _onQrCodeRead = (id: string, wasAlreadyRedeemed: boolean) => {
        props.onReadQrCodeClosed();
        setSelectedOrder({
            id: id,
            wasAlreadyRedeemed: wasAlreadyRedeemed,
        })
        props.onOrderSelected(id);
    }

    const getCardsSection = () => {
        switch(filters.states)
        {
            case historyStates:
                return <OrdersHistory
                            states={filters.states}
                            onOrderSelected={(o) => props.onOrderSelected(o.id)}
                        />;
            case processingStates: 
                return <PreparationGroupsQueueCards
                            locationId={props.localId}
                            onOrderSelected={(o) => props.onOrderSelected(o.id)}
                        />
            case preparingStates: 
                return <CommitedPreparationGroupsQueueCards
                            locationId={props.localId}
                            onOrderSelected={(o) => props.onOrderSelected(o.id)}
                        />
        }
        return <OrdersQueueCards
            states={filters.states}
            onOrderSelected={(o) => props.onOrderSelected(o.id)}
            onOrderUpdated={o => props.onOrderUpdated(o.channelId)}
        />;
    }

    return (
        <Box
            sx={{
                width: "100%",
                height: "100%",
                display: "flex",
                flexDirection: "column",
                overflow: "hidden"
            }}
        >
            <Grid
                container
                spacing={1}
                width="100%"
            >
                <Grid
                    size={{
                        xs: "auto",
                        md: "grow",
                    }}
                >
                    <Chip 
                        sx={{
                            width: "100%",
                            whiteSpace: "normal",
                        }}
                        label={t("ordersTab.new")} 
                        color="primary"
                        variant={filters.states == newTabStates ? "filled" : "outlined"} 
                        onClick={() => setFilters(f => ({...f, states: newTabStates}))} 
                        avatar={requestedCountQuery.isFirstLoading ? undefined : <Avatar>{requestedCountQuery.totalItems}</Avatar>}
                    />
                </Grid>
                <Grid
                    size={{
                        xs: "auto",
                        md: "grow",
                    }}
                >
                    <Chip 
                        sx={{
                            width: "100%",
                            whiteSpace: "normal",
                        }}
                        label={t("ordersTab.inProgress")} 
                        color="primary"
                        variant={filters.states == processingStates ? "filled" : "outlined"} 
                        onClick={() => setFilters(f => ({...f, states: processingStates}))} 
                        avatar={processingCountQuery.isFirstLoading ? undefined : <Avatar>{processingCountQuery.totalItems}</Avatar>}
                    />
                </Grid>
                <Grid
                    size={{
                        xs: "auto",
                        md: "grow",
                    }}
                >
                    <Chip 
                        sx={{
                            width: "100%",
                            whiteSpace: "normal",
                        }}
                        label={t("ordersTab.preparing")} 
                        color="primary"
                        variant={filters.states == preparingStates ? "filled" : "outlined"} 
                        onClick={() => setFilters(f => ({...f, states: preparingStates}))} 
                        avatar={preparingCountQuery.isFirstLoading ? undefined : <Avatar>{preparingCountQuery.totalItems}</Avatar>}
                    />
                </Grid>
                <Grid
                    size={{
                        xs: "auto",
                        md: "grow",
                    }}
                >
                    <Chip 
                        sx={{
                            width: "100%",
                            whiteSpace: "normal",
                        }}
                        label={t("ordersTab.history")} 
                        color="primary"
                        variant={filters.states == historyStates ? "filled" : "outlined"} 
                        onClick={() => setFilters(f => ({...f, states: historyStates}))} 
                    />
                </Grid>
                <Grid
                    size={{
                        xs: "auto",
                        md: "grow",
                    }}
                >
                    <Chip 
                        sx={{
                            width: "100%",
                            whiteSpace: "normal",
                        }}
                        label={t("ordersTab.scheduled")} 
                        color="primary"
                        variant={filters.states == scheduledStates ? "filled" : "outlined"} 
                        onClick={() => setFilters(f => ({...f, states: scheduledStates}))} 
                        avatar={scheduledCountQuery.isFirstLoading ? undefined : <Avatar>{scheduledCountQuery.totalItems}</Avatar>}
                    />
                </Grid>
            </Grid>

            <Box 
                sx={{
                    flex: 1,
                    pt: "1rem",
                    overflow: "hidden"
                }}
            >
                {getCardsSection()}
            </Box>
            {/* <QrCodeReaderModal isOpen={props.readQrCodeOpen} onClose={props.onReadQrCodeClosed} onOrderRead={onQrCodeRead} /> */}
            <OrderDetailModal
                orderId={selectedOrder.id}
                alreadyRedeemed={selectedOrder.wasAlreadyRedeemed}
                onClose={() => props.onOrderSelected(undefined)}
            />
        </Box>
    )
}