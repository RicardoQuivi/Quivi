import { useEffect, useMemo, useState } from "react"
import { Alert, AlertTitle, Box, Divider, FormControl, Grid, IconButton, InputLabel, keyframes, MenuItem as MUIMenuItem, OutlinedInput, Select, SelectChangeEvent, Skeleton, Stack, styled, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, TextField, Tooltip, Typography } from "@mui/material";
import { Trans, useTranslation } from "react-i18next";
import { useChannelsQuery } from "../../hooks/queries/implementations/useChannelsQuery";
import { useEmployeesQuery } from "../../hooks/queries/implementations/useEmployeesQuery";
import { useOrdersQuery } from "../../hooks/queries/implementations/useOrdersQuery";
import { SortDirection } from "../../hooks/api/Dtos/SortableRequest";
import { OrderState } from "../../hooks/api/Dtos/orders/OrderState";
import LoadingButton from "../Buttons/LoadingButton";
import { CloseIcon } from "../../icons";
import { Order } from "../../hooks/api/Dtos/orders/Order";
import CustomModal, { ModalSize } from "../Modals/CustomModal";
import { useToast } from "../../context/ToastProvider";
import CurrencySpan from "../Currency/CurrencySpan";
import { ConfigurableField, ConfigurableFieldType } from "../../hooks/api/Dtos/configurablefields/ConfigurableField";
import ConfirmButton from "../Buttons/ConfirmButton";
import { ApiException } from "../../hooks/api/exceptions/ApiException";
import { useNow } from "../../hooks/useNow";
import { MenuItem } from "../../hooks/api/Dtos/menuitems/MenuItem";
import { useChannelProfilesQuery } from "../../hooks/queries/implementations/useChannelProfilesQuery";
import { useConfigurableFieldsQuery } from "../../hooks/queries/implementations/useConfigurableFieldsQuery";
import { useTransactionsQuery } from "../../hooks/queries/implementations/useTransactionsQuery";
import { useOrderHelper } from "../../helpers/useOrderHelper";
import { useMenuItemsQuery } from "../../hooks/queries/implementations/useMenuItemsQuery";
import { useDateHelper } from "../../helpers/dateHelper";
import { useOrderMutator } from "../../hooks/mutators/useOrderMutator";
import { useTransactionMutator } from "../../hooks/mutators/useTransactionMutator";
import { Transaction } from "../../hooks/api/Dtos/transactions/Transaction";
import { useMenuItemMutator } from "../../hooks/mutators/useMenuItemMutator";
import { InvalidModelResponse } from "../../hooks/api/exceptions/InvalidModelResponse";
import ValidationMessage from "../Validations/ValidationMessage";
import { useActionAwaiter } from "../../hooks/useActionAwaiter";

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
        flex: 1,
    }
}))

enum CancelReasonType {
    Other = 0,
    OutOfStock = 1,
}

interface Aux {
    readonly id: string;
    readonly menuItemId: string;
    readonly quantity: number;
    readonly isSubItem: boolean;
    readonly price: number;
}
interface OrderDetailModalProps {
    readonly orderId?: string;
    readonly onClose: () => any;
    readonly alreadyRedeemed: boolean;
}
export const OrderDetailModal = (props: OrderDetailModalProps) => {
    const { t } = useTranslation();
    const orderMutator = useOrderMutator();
    const toast = useToast();
    const awaiter = useActionAwaiter();

    const [helpActive, setHelpActive] = useState(false);
    const ordersQuery = useOrdersQuery(props.orderId == undefined ? undefined : {
        ids: [props.orderId],
        page: 0,
        sortDirection: SortDirection.Asc,
    })
    const order = useMemo(() => ordersQuery.data.length > 0 ? ordersQuery.data[0] : undefined, [ordersQuery.data]);

    const transactionsQuery = useTransactionsQuery(order != undefined ? {
        orderIds: [order.id],
        page: 0,
    } : undefined)
    const transaction = useMemo(() => transactionsQuery.data.length == 0 ? undefined : transactionsQuery.data[0], [transactionsQuery.data]);

    const channelQuery = useChannelsQuery(order == undefined ? undefined : {
        page: 0,
        pageSize: 1,
        allowsSessionsOnly: false,
        ids: [order.channelId],
        includeDeleted: true,
    });
    const channel = useMemo(() => channelQuery.data.length > 0 ? channelQuery.data[0] : undefined, [channelQuery.data]);

    const profileQuery = useChannelProfilesQuery(channel == undefined ? undefined : {
        ids: [channel.channelProfileId],
        page: 0,
        pageSize: 1,
    });
    const profile = useMemo(() => profileQuery.data.length > 0 ? profileQuery.data[0] : undefined, [profileQuery.data]);

    const employeeQuery = useEmployeesQuery(order != undefined && order.employeeId != undefined ? {
        ids: [order.employeeId],
        includeDeleted: true,
        page: 0,
        pageSize: 1,
    } : undefined)
    const employee = useMemo(() => employeeQuery.data.length > 0 ? employeeQuery.data[0] : undefined, [employeeQuery.data]);

    useEffect(() => setHelpActive(false), [props.orderId]);
    
    const getTitle = () => {
        if(order == undefined) {
            return <></>
        }

        return <Box
            sx={{
                display: "flex",
                flexDirection: "row",
                justifyContent: "space-between",
            }}
        >
            <Stack
                direction="column"
                alignItems="center"
            >
                <Typography variant="h4">
                    {order.sequenceNumber}
                </Typography>
                <Typography variant="subtitle1">
                    {order.id}
                </Typography>
            </Stack>
            <Stack
                direction="column"
                alignItems="center"
            >
                {
                    channel == undefined || profile == undefined
                    ?
                    <Skeleton animation="wave" />
                    :
                    <Typography variant="h4">
                        {profile.name} {channel.name}
                    </Typography>
                }
                {
                    order.employeeId != undefined &&
                    <Typography variant="subtitle1">
                        {t("viaPos")} ({employee == undefined ? <Skeleton animation="wave" /> : employee.name})
                    </Typography>
                }
            </Stack>

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
                {
                    order.employeeId == undefined &&
                    [OrderState.Completed, OrderState.Rejected].includes(order.state) == false &&
                    <LoadingButton
                        isLoading={false}
                        onClick={() => setHelpActive(h => !h)}
                    >
                        {t(helpActive ? "back" : "ordersTab.help")}
                    </LoadingButton>
                }
                <IconButton
                    onClick={props.onClose}
                    sx={{
                        cursor: "pointer",
                    }}
                >
                    <CloseIcon />
                </IconButton>
            </Box>
        </Box>
    }
   
    const onOrderNextStateClicked = async (o: Order, complete: boolean) => {
        try {
            const jobId = await orderMutator.process(o, {
                completeOrder: complete,
            });
            await awaiter.job(jobId);
            props.onClose();
        } catch {
            toast.error(t('unexpectedErrorHasOccurred'));
        }
    }

    const onOrderCanceled = () => {
        if(transaction == undefined) {
            props.onClose();
            return;
        }
        setHelpActive(false);
    }

    return <CustomModal
        isOpen={order != undefined}
        title={getTitle()}
        onClose={props.onClose}
        size={ModalSize.Large}
        hideClose
    >
        {
            order == undefined || transactionsQuery.isFirstLoading == true
            ?
            <OrderDetailPlaceholder />
            :
            (
                helpActive == false
                ?
                <OrderDetail
                    order={order}
                    transaction={transaction}
                    alreadyRedeemed={props.alreadyRedeemed}
                    onOrderNextStateClicked={o => onOrderNextStateClicked(o, false)} 
                    onOrderCompleteClicked={o => onOrderNextStateClicked(o, true)}
                    onOrderRefund={props.onClose}
                />
                :
                <OrderHelp
                    order={order}
                    onCompleted={onOrderCanceled}
                />
            )
        }
    </CustomModal>
}

const OrderDetailPlaceholder = () => {
    return <>
        <TableContainer>
            <Table
                sx={{
                    width: "100%",
                }}
                size="medium"
            >
                <TableHead>
                    <TableRow
                        sx={{ 
                            display: "flex", 
                            flexDirection: "row",
                            justifyContent: "space-between",

                            "& .MuiTableCell-root": {
                                fontSize: "1rem",
                            },

                            "& th": {
                                flex: 1,
                                fontWeight: "bold",
                            }
                        }}
                    >
                        <TableCell align="left"><Skeleton animation="wave" /></TableCell>
                        <TableCell align="right"><Skeleton animation="wave" /></TableCell>
                    </TableRow>
                </TableHead>
                <TableBody>
                    {
                        [1, 2, 3, 4, 5].map(i => <TableRow
                            key={`Loading-${i}`}
                            sx={{ 
                                display: "flex", 
                                flexDirection: "row", 
                                '&:last-child td, &:last-child th': { 
                                    border: 0,
                                },
                                "& .MuiTableCell-root": {
                                    fontSize: "1rem",
                                }
                            }}
                        >
                            <TableCell
                                component="th"
                                scope="row"
                                sx={{
                                    flex: "0 0 auto",
                                }}
                            >
                                <Skeleton animation="wave" sx={{ minWidth: "3rem"}} />
                            </TableCell>
                            <TableCell
                                align="left"
                                sx={{
                                    flex: 1,
                                }}
                            >
                                <Skeleton animation="wave" />
                            </TableCell>
                            <TableCell
                                align="right"
                                sx={{
                                    flex: 0,
                                }}
                            >
                                <Skeleton animation="wave" sx={{ minWidth: "3rem"}} />
                            </TableCell>
                        </TableRow>
                        )
                    }
                    <TableRow
                        sx={{
                            display: "flex", 
                            flexDirection: "row", 
                            '& td, & th': { 
                                border: "unset",
                            },
                            "& .MuiTableCell-root": {
                                fontSize: "1rem",
                            }
                        }}
                    >
                        <TableCell sx={{flex: 1}}><Skeleton animation="wave" /></TableCell>
                        <TableCell sx={{flex: 0}} align="right"><Skeleton animation="wave"  sx={{ minWidth: "3rem"}} /></TableCell>
                    </TableRow>
                </TableBody>
            </Table>
        </TableContainer>
        <Skeleton animation="wave" />
        <Divider />
        <Grid 
            container
            sx={{
                marginTop: "1rem",
            }}
            spacing={2}
        >
            <Grid size="grow">
                <LoadingButton 
                    style={{
                        width: "100%",
                    }}
                    disabled
                >
                    <Skeleton animation="wave" width="100%" />
                </LoadingButton>
            </Grid>
            <Grid
                size="grow"
            >
                <LoadingButton
                    style={{
                        width: "100%",
                    }}
                    disabled
                >
                    <Skeleton animation="wave" width="100%" />
                </LoadingButton>
            </Grid>
        </Grid>
    </>
}
interface OrderDetailProps {
    readonly order: Order;
    readonly transaction?: Transaction
    readonly alreadyRedeemed: boolean;
    readonly onOrderNextStateClicked: (order: Order) => Promise<void>;
    readonly onOrderCompleteClicked: (order: Order) => Promise<void>;
    readonly onOrderRefund: (order: Order) => Promise<void>;
}
const OrderDetail = ({
    order,
    alreadyRedeemed,
    onOrderNextStateClicked,
    onOrderCompleteClicked,
    transaction,
}: OrderDetailProps) => {
    const { t } = useTranslation();
    const dateHelper = useDateHelper();
    const now = useNow(1000);
    const helper = useOrderHelper();
    const transactionMutator = useTransactionMutator();

    const isDelayed = helper.isOrderDelayed(order.lastModified);
    const fieldIds = useMemo(() => {
        const set = new Set<string>();
        for(const f of order.fields) {
            set.add(f.id);
        }
        return Array.from(set.keys());
    }, [order.fields]);
    const fieldsQuery = useConfigurableFieldsQuery(order.fields.length == 0 ? undefined : {
        ids: fieldIds,
        page: 0,
    })
    const fieldsMap = useMemo(() => {
        const map = new Map<string, ConfigurableField>();
        for(const f of fieldsQuery.data) {
            map.set(f.id, f);
        }
        return map;
    }, [fieldsQuery.data])
    
    const getTimeBadge = (order: Order) => {
        if([OrderState.Scheduled, OrderState.ScheduledRequested].includes(order.state) && order.scheduledTo != undefined) {
            const date = dateHelper.toDate(order.scheduledTo);
            const day = date.getDate();
            let month = dateHelper.toLocalString(date, "MMMM");
            month = `${month[0].toUpperCase()}${month.substring(1)}`;
            const time = dateHelper.toLocalString(date, "HH:mm");
            return (
                <StyledAlert variant="outlined" severity="info">
                    <Typography variant="body1" component="time">
                        {t("ordersTab.pickUpAt", { day: day, month: month, time: time})}
                    </Typography>
                </StyledAlert>
            )
        }
        
        return (
            <StyledAlert variant="outlined" severity={isDelayed ? "warning": "info"}>
                {
                    isDelayed
                    ?
                    <FadingTypography variant="subtitle1">
                        {dateHelper.getTimeAgo(now, order.lastModified)}
                    </FadingTypography>
                    :
                    <Typography variant="subtitle1">
                        {dateHelper.getTimeAgo(now, order.lastModified)}
                    </Typography>
                }
            </StyledAlert>
        )
    }

    const {
        itemsList,
        menuItemIds,
     } = useMemo(() => {
        if(order == undefined) {
            return {
                itemsList: [],
                menuItemIds: [],
            }
        }

        const result = [] as Aux[];
        const ids = new Set<string>();
        for(const i of order.items) {
            ids.add(i.menuItemId);

            result.push({
                id: i.id,
                menuItemId: i.menuItemId,
                quantity: i.quantity,
                isSubItem: false,
                price: i.price,
            });

            let index = 0;
            for(const m of i.extras) {
                ids.add(m.menuItemId);
                result.push({
                    id: `${i.id}-${index}`,
                    menuItemId: m.menuItemId,
                    quantity: m.quantity * i.quantity,
                    isSubItem: true, 
                    price: m.price,
                })
                ++index;
            }
        }

        return {
            itemsList: result,
            menuItemIds: Array.from(ids),
        }
    }, [order.items])

    const menuItemsQuery = useMenuItemsQuery(menuItemIds.length == 0 ? undefined : {
        ids: menuItemIds,
        page: 0,
        includeDeleted: true,
    })
    const menuItemsMap = useMemo(() => {
        const map = new Map<string, MenuItem>();
        for(const item of menuItemsQuery.data) {
            map.set(item.id, item);
        }
        return map;
    }, [menuItemsQuery.data])

    console.log(transaction)
    return <>
        <TableContainer>
            <Table
                sx={{
                    width: "100%",
                }}
                size="medium"
            >
                <TableHead>
                    <TableRow
                        sx={{ 
                            display: "flex", 
                            flexDirection: "row",
                            justifyContent: "space-between",

                            "& .MuiTableCell-root": {
                                fontSize: "1rem",
                            },

                            "& th": {
                                flex: 1,
                                fontWeight: "bold",
                            }
                        }}
                    >
                        <TableCell align="left">{t("name")}</TableCell>
                        <TableCell align="right">{t("price")}</TableCell>
                    </TableRow>
                </TableHead>
                <TableBody>
                    {
                        order != undefined &&
                        <>
                            {
                                itemsList.map(i => {
                                    const menuItem = menuItemsMap.get(i.menuItemId);
                                    return <TableRow
                                        key={i.id}
                                        sx={{ 
                                            display: "flex", 
                                            flexDirection: "row", 
                                            '&:last-child td, &:last-child th': { 
                                                border: 0,
                                            },
                                            "& .MuiTableCell-root": {
                                                fontSize: "1rem",
                                            }
                                        }}
                                    >
                                        <TableCell
                                            component="th"
                                            scope="row"
                                            sx={{
                                                flex: "0 0 auto",
                                                
                                            }}
                                        >
                                            <Typography
                                                variant={i.isSubItem ? "body2" : "body1"}
                                                gutterBottom
                                                sx={{
                                                    fontWeight: i.isSubItem ? undefined : "bold",
                                                }}
                                            >
                                                {i.quantity}
                                            </Typography>
                                        </TableCell>
                                        <TableCell
                                            align="left"
                                            sx={{
                                                flex: 1,
                                            }}
                                        >
                                            {
                                                menuItem == undefined
                                                ?
                                                <Skeleton animation="wave" />
                                                :
                                                <Typography
                                                    variant={i.isSubItem ? "body2" : "body1"}
                                                    gutterBottom
                                                    sx={{
                                                        paddingLeft: "2rem",
                                                    }}
                                                >
                                                    {menuItem.name}
                                                </Typography>
                                            }
                                        </TableCell>
                                        <TableCell
                                            align="right"
                                            sx={{
                                                flex: 0,
                                            }}
                                        >
                                            <CurrencySpan value={i.price} />
                                        </TableCell>
                                    </TableRow>
                                })
                            }
                            <TableRow
                                sx={{
                                    display: "flex", 
                                    flexDirection: "row", 
                                    '& td, & th': { 
                                        border: "unset",
                                    },
                                    "& .MuiTableCell-root": {
                                        fontSize: "1rem",
                                    }
                                }}
                            >
                                <TableCell sx={{flex: 1, fontWeight: "bold"}}>{t("total")}</TableCell>
                                <TableCell sx={{flex: 0}} align="right">
                                    <CurrencySpan value={helper.getTotal(order)} />
                                </TableCell>
                            </TableRow>
                        </>
                    }
                </TableBody>
            </Table>
        </TableContainer>
        {
            order.fields.map(f => {
                const configurableField = fieldsMap.get(f.id);
                return (
                    <StyledAlert
                        variant="outlined"
                        severity="info"
                        key={f.id}
                        sx={{
                            marginBottom: "0.5rem",
                        }}
                    >
                        <AlertTitle
                            sx={{
                                textAlign: "left",
                            }}
                        >
                            {configurableField == undefined ? <Skeleton animation="wave" width="100%" /> : configurableField.name}
                        </AlertTitle>
                        {
                            configurableField == undefined
                            ? 
                                <Skeleton animation="wave" width="100%" />
                            :
                            (
                                configurableField.type == ConfigurableFieldType.Check
                                ?
                                t(`${["", "0", "false"].includes(f.value.toLowerCase()) ? "no" : "yes"}`)
                                :
                                f.value
                            )
                        }
                    </StyledAlert>
                );
            })
        }
        {
            [OrderState.Completed, OrderState.Rejected].includes(order.state) == false &&
            getTimeBadge(order)
        }
        {
            order.state == OrderState.Rejected &&
            <>
                <StyledAlert variant="filled" severity="error">
                    {t("ordersTab.canceled")}
                </StyledAlert>
                {
                    transaction != undefined &&
                    transaction.refundedAmount == 0 &&
                    <Grid
                        container
                        sx={{
                            marginTop: "1rem",
                        }}
                        spacing={2}
                    >
                        <Grid size={12}>
                            <ConfirmButton
                                style={{
                                    width: "100%",
                                }}
                                onAction={async () => {
                                    await transactionMutator.refund(transaction, {
                                        amount: transaction.payment + transaction.tip,
                                        ignoreAcquireRefundErrors: true,
                                    })
                                }}
                                confirmText={`${t("confirm")}?`}
                            >
                                {t("ordersTab.refund")}
                            </ConfirmButton>
                        </Grid>
                    </Grid>
                }
            </>
        }
        {
            order.state == OrderState.Completed && alreadyRedeemed &&
            <StyledAlert variant="filled" severity="warning">
                <Trans
                    t={t}
                    i18nKey="ordersTab.orderCompletedWarning"
                    values={{
                        timeAgo: dateHelper.getTimeAgo(order.lastModified, now),
                    }}
                    components={{
                        time: <Typography variant="body1" component="time" sx={{ textTransform: "lowercase" }} />
                    }}
                />
            </StyledAlert>
        }
        <Divider />
        {
            order.state == OrderState.PendingApproval &&
            <Grid 
                container
                sx={{
                    marginTop: "1rem",
                }}
                spacing={2}
            >
                <Grid size="grow">
                    <ConfirmButton 
                        style={{
                            width: "100%",
                        }}
                        onAction={async () => {
                            if(order == undefined) {
                                return;
                            } 
                            await onOrderCompleteClicked(order);
                        }}
                        confirmText={`${t("confirm")}?`}
                    >
                        {t("ready")}
                    </ConfirmButton>
                </Grid>
                <Grid
                    size="grow"
                >
                    <ConfirmButton
                        style={{
                            width: "100%",
                        }}
                        onAction={async () => {
                            if(order == undefined) {
                                return;
                            } 
                            await onOrderNextStateClicked(order);
                        }} 
                        confirmText={`${t("confirm")}?`}
                        primaryButton
                    >
                        {t("accept")}
                    </ConfirmButton>
                </Grid>
            </Grid>
        }
    </>
}

interface OrderHelpProps {
    readonly order: Order;
    readonly onCompleted: () => any;
}
const OrderHelp = ({
    order,
    onCompleted,
}: OrderHelpProps) => {
    const { t } = useTranslation();
    const toast = useToast();
    const orderMutator = useOrderMutator();
    const itemMutator = useMenuItemMutator();
    const awaiter = useActionAwaiter();

    const actionOptions = [{
        id: 0,
        title: t("ordersTab.cancel"),
    }]
    const cancelOptions = [
        {
            id: CancelReasonType.OutOfStock,
            title: t("ordersTab.outOfStock"),
        },
        {
            id: CancelReasonType.Other,
            title: t("ordersTab.other"),
        },
    ]
   
    const menuItemIds = useMemo(() => {
        const set = new Set<string>();
        for(const item of order.items) {
            set.add(item.menuItemId);
            for(const m of item.extras) {
                set.add(m.menuItemId);
            }
        }
        return Array.from(set.keys());
    }, [order.items])
    const menuItemsQuery = useMenuItemsQuery(menuItemIds.length == 0 ? undefined : {
        ids: menuItemIds,
        page: 0,
        includeDeleted: true,
    });

    const [state, setState] = useState({
        selectedAction: actionOptions[0],
        selectedCancelReason: cancelOptions[0],
        reason: "",
        isSubmitting: false,
        apiErrors: [] as InvalidModelResponse[],
    })

    const handleActionChange = (event: SelectChangeEvent<typeof state.selectedAction>) => {
        const {
          target: { value },
        } = event;

        if( typeof value === 'string' ) {
            return;
        }

        if( typeof value === 'number' ) {
            const selected = actionOptions.find(a => a.id == value);
            if(selected != undefined) {
                setState(s => ({...s, selectedAction: selected }));
            }
            return;
        }
        setState(s => ({...s, selectedAction: value }));
    }
      
    const handleCancelChange = (event: SelectChangeEvent<typeof state.selectedCancelReason>) => {
        const {
          target: { value },
        } = event;

        if( typeof value === 'string' ) {
            return;
        }

        if( typeof value === 'number' ) {
            const selected = cancelOptions.find(a => a.id == value);
            if(selected != undefined) {
                setState(s => ({...s, selectedCancelReason: selected }));
            }
            return;
        }
        setState(s => ({...s, selectedCancelReason: value }));
    }

    const onToggleStock = async (item: MenuItem, newStock: boolean) => {
        try {
            if(newStock) {
                await itemMutator.updateStock([item], []);
            } else {
                await itemMutator.updateStock([], [item]);
            }
        } catch (error) {
            toast.error(t('unexpectedErrorHasOccurred'));
        }
    }

    const declineOrder = async () => {
        try {
            setState(s => ({...s, isSubmitting: true}))
            const jobId = await orderMutator.decline(order, {
                reason: state.reason,
            })
            await awaiter.job(jobId);
            setState(s => ({...s, apiErrors: [], isSubmitting: false}));
            onCompleted();
        } catch (e) {
            if(e instanceof ApiException) {
                const apiErrors = e.errors;
                setState(s => ({...s, apiErrors: apiErrors, isSubmitting: false}));
            } else {
                setState(s => ({...s, apiErrors: [], isSubmitting: false}));
                toast.error(t('unexpectedErrorHasOccurred'));
                throw e;
            }
        }
    }

    return (
    <Grid container spacing={2}>
        <Grid size={12}>
            <FormControl
                sx={{
                    width: "100%",
                }}
            >
                <InputLabel>{t("action")}</InputLabel>
                <Select
                    value={state.selectedAction}
                    renderValue={(selected) => selected.title}
                    onChange={handleActionChange}
                    input={<OutlinedInput label={t("action")} />}
                    label={t("action")}
                    fullWidth
                    MenuProps={{
                        sx: {
                            zIndex: 9999999, // Ensure this is higher than the modal's zIndex
                        },
                    }}
                >
                    {actionOptions.map((a) => <MUIMenuItem key={a.id} value={a.id}>{a.title}</MUIMenuItem>)}
                </Select>
            </FormControl>
        </Grid>
        <Grid size={12}>
            <FormControl
                sx={{
                    width: "100%",
                }}
            >
                <InputLabel>{t("ordersTab.cancelReason")}</InputLabel>
                <Select
                    value={state.selectedCancelReason}
                    renderValue={s => s.title}
                    onChange={handleCancelChange}
                    input={<OutlinedInput label={t("ordersTab.cancelReason")} />}
                    fullWidth
                    label={t("ordersTab.cancelReason")}
                    inputProps={{
                        style: {
                            zIndex: 9999999, // Ensure this is higher than the modal's zIndex
                        }
                    }}
                    MenuProps={{
                        sx: {
                            zIndex: 9999999, // Ensure this is higher than the modal's zIndex
                        },
                    }}
                >
                    {cancelOptions.map((a) => <MUIMenuItem key={a.id} value={a.id}>{a.title}</MUIMenuItem>)}
                </Select>
            </FormControl>
        </Grid>

        {
            state.selectedCancelReason.id == CancelReasonType.OutOfStock && 
            <>
                <Grid size={12}>
                    <Alert variant="outlined" severity="warning" title={t("warning")!}>
                        {t("ordersTab.cancelAlert")}
                    </Alert>
                </Grid>

                <Grid size={12}>
                    <TableContainer sx={{ width: "100%" }}>
                        <Table sx={{ width: "100%" }} size="medium">
                            <TableHead>
                                <TableRow 
                                    sx={{ 
                                        width: "100%",
                                        "& .MuiTableCell-root": {
                                            fontSize: "1rem",
                                        },

                                        "& th": {
                                            fontWeight: "bold",
                                        }
                                    }}
                                >
                                    <TableCell align="left">{t("name")}</TableCell>
                                    <TableCell align="right">{t("ordersTab.availability")}</TableCell>
                                </TableRow>
                            </TableHead>
                            <TableBody>
                                {
                                    menuItemsQuery.isFirstLoading
                                    ?
                                        [1, 2, 3, 4, 5].map(i => (
                                            <TableRow
                                                key={`Loading-${i}`}
                                                sx={{
                                                    width: "100%",
                                                    '&:last-child td, &:last-child th': { 
                                                        border: 0 
                                                    },
                                                    "& .MuiTableCell-root": {
                                                        fontSize: "1rem",
                                                    }
                                                }}
                                            >
                                                <TableCell align="left">
                                                    <Skeleton animation="wave" />
                                                </TableCell>
                                                <TableCell align="right">
                                                    <Skeleton animation="wave" />
                                                </TableCell>
                                            </TableRow>
                                        ))
                                    :
                                    menuItemsQuery.data.map((i) => (
                                        <TableRow
                                            key={i.id}
                                            sx={{
                                                width: "100%",
                                                '&:last-child td, &:last-child th': { 
                                                    border: 0 
                                                },
                                                "& .MuiTableCell-root": {
                                                    fontSize: "1rem",
                                                }
                                            }}
                                        >
                                            <TableCell align="left">
                                                {i.name}
                                            </TableCell>
                                            <TableCell align="right">
                                                <Tooltip title={t("ordersTab.doubleClickToToggleStatus")}>
                                                    <ConfirmButton
                                                        primaryButton={false}
                                                        style={{padding: "5px 17px"}} 
                                                        className={i.hasStock ? "paid-btn" : "cancelled-btn"} 
                                                        overrideClassName={true} 
                                                        confirmText={`${i.hasStock ? t("ordersTab.toggleToUnavailable") : t("ordersTab.toggleToAvailable")}?`} 
                                                        onAction={() => onToggleStock(i, !i.hasStock)}
                                                        timeoutMillis={3000}
                                                    >
                                                        {i.hasStock ? t("available") : t("notAvailable")}
                                                    </ConfirmButton>
                                                </Tooltip>
                                            </TableCell>
                                        </TableRow>
                                    ))
                                }
                            </TableBody>
                        </Table>
                    </TableContainer>
                </Grid>
            </>
        }
        {
            state.selectedCancelReason.id == CancelReasonType.Other &&
            <Grid size={12}>
                <TextField
                    label={t("description")}
                    fullWidth
                    value={state.reason}
                    onChange={(event) => setState(s => ({...s, reason: event.target.value}))}
                    error={state.apiErrors.some(a => a.property == "Reason")}
                />
                <ValidationMessage errorMessages={state.apiErrors} propertyPath="reason" />
            </Grid>
        }
        <Grid size={12}>
            <LoadingButton
                style={{
                    width: "100%",
                }}
                primaryButton={false}
                isLoading={state.isSubmitting}
                onClick={declineOrder}
            >
                {t("ordersTab.cancel")}
            </LoadingButton>
        </Grid>
    </Grid>
    )
}