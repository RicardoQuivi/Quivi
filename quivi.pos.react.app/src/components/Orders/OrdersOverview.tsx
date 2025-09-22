import { useEffect, useState } from "react"
import { Badge, Box, Button, Grid, Stack, styled, Typography, useMediaQuery, useTheme, } from "@mui/material";
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

const StyledBadge = styled(Badge)(({ theme }) => ({
    borderRadius: "50%",
    aspectRatio: 1,
    width: "24.5px",
    maxHeight: "24.5px",
    backgroundColor: theme.palette.primary.main,
    color: theme.palette.primary.dark,
    display: 'flex', // Ensures content is centered
    alignItems: 'center', // Centers content vertically
    justifyContent: 'center', // Centers content horizontally
    boxSizing: 'border-box', // Ensures padding/borders don’t affect size
}))

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
    const theme = useTheme();
    const xs = useMediaQuery(theme.breakpoints.only('xs'));

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

    // const onQrCodeRead = (id: string, wasAlreadyRedeemed: boolean) => {
    //     props.onReadQrCodeClosed();
    //     setSelectedOrder({
    //         id: id,
    //         wasAlreadyRedeemed: wasAlreadyRedeemed,
    //     })
    //     props.onOrderSelected(id);
    // }

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
                overflow: "hidden",
            }}
        >
            <Grid
                container
                spacing={1}
                width="100%"
                sx={{
                    "& .MuiButton-root": {
                        width: "100%",
                        whiteSpace: "normal",

                        height: {
                            xs: "100%",
                            sm: undefined,
                        }
                    }
                }}
            >
                <Grid
                    size={{
                        xs: 6,
                        md: "grow",
                    }}
                >
                    <Button
                        size={xs ? "small" : "medium"}
                        variant={filters.states == newTabStates ? "contained" : "outlined"} 
                        onClick={() => setFilters(f => ({...f, states: newTabStates}))} 
                    >
                        <Stack direction="row" gap={1} justifyContent="center">
                            { requestedCountQuery.isFirstLoading == false && <StyledBadge>{requestedCountQuery.totalItems}</StyledBadge>}
                            <Typography variant="button">
                                {t("ordersTab.new")} 
                            </Typography>
                        </Stack>
                    </Button>
                </Grid>
                <Grid
                    size={{
                        xs: 6,
                        md: "grow",
                    }}
                >
                    <Button
                        size={xs ? "small" : "medium"}
                        variant={filters.states == processingStates ? "contained" : "outlined"} 
                        onClick={() => setFilters(f => ({...f, states: processingStates}))} 
                    >
                        <Stack direction="row" gap={1} justifyContent="center">
                            { processingCountQuery.isFirstLoading == false && <StyledBadge>{processingCountQuery.totalItems}</StyledBadge>}
                            <Typography variant="button">
                                {t("ordersTab.inProgress")} 
                            </Typography>
                        </Stack>
                    </Button>
                </Grid>
                <Grid
                    size={{
                        xs: 6,
                        md: "grow",
                    }}
                >
                    <Button
                        size={xs ? "small" : "medium"}
                        variant={filters.states == preparingStates ? "contained" : "outlined"} 
                        onClick={() => setFilters(f => ({...f, states: preparingStates}))} 
                    >
                        <Stack direction="row" gap={1} justifyContent="center">
                            { preparingCountQuery.isFirstLoading == false && <StyledBadge>{preparingCountQuery.totalItems}</StyledBadge>}
                            <Typography variant="button">
                                {t("ordersTab.preparing")} 
                            </Typography>
                        </Stack>
                    </Button>
                </Grid>
                <Grid
                    size={{
                        xs: 6,
                        md: "grow",
                    }}
                >
                    <Button
                        size={xs ? "small" : "medium"}
                        variant={filters.states == historyStates ? "contained" : "outlined"} 
                        onClick={() => setFilters(f => ({...f, states: historyStates}))} 
                    >
                        <Typography variant="button">
                            {t("ordersTab.history")} 
                        </Typography>
                    </Button>
                </Grid>
                <Grid
                    size={{
                        xs: 6,
                        md: "grow",
                    }}
                >
                    <Button
                        size={xs ? "small" : "medium"}
                        variant={filters.states == scheduledStates ? "contained" : "outlined"} 
                        onClick={() => setFilters(f => ({...f, states: scheduledStates}))} 
                    >
                        <Stack direction="row" gap={1} justifyContent="center">
                            { scheduledCountQuery.isFirstLoading == false && <StyledBadge>{scheduledCountQuery.totalItems}</StyledBadge>}
                            <Typography variant="button">
                                {t("ordersTab.scheduled")} 
                            </Typography>
                        </Stack>
                    </Button>
                </Grid>
            </Grid>

            <Box 
                sx={{
                    flex: 1,
                    pt: "1rem",
                    overflow: "hidden",
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