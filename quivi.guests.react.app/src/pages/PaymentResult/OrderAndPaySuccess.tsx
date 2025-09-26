import { useEffect, useMemo } from "react";
import { useTranslation } from "react-i18next";
import React from "react";
import QRCode from "react-qr-code";
import { format } from "date-fns";
import { useQuiviTheme } from "../../hooks/theme/useQuiviTheme";
import type { Order } from "../../hooks/api/Dtos/orders/Order";
import { Alert, Box, Step, StepLabel, Stepper } from "@mui/material";
import { useBrowserStorageService } from "../../hooks/useBrowserStorageService";
import { useChannelContext } from "../../context/AppContextProvider";
import { useAuth } from "../../context/AuthContext";
import { OrderState } from "../../hooks/api/Dtos/orders/OrderState";
import type { ReceiptLine } from "../../components/Receipt/ReceiptLine";
import type { ReceiptSubTotalLine } from "../../components/Receipt/ReceiptSubTotalLine";
import Receipt from "../../components/Receipt/Receipt";
import ActionButton from "../../components/Buttons/ActionButton";
import { DownloadIcon } from "../../icons";
import { Files } from "../../helpers/files";
import { Link } from "react-router";
import { useTransactionsQuery } from "../../hooks/queries/implementations/useTransactionsQuery";
import { useTransactionInvoicesQuery } from "../../hooks/queries/implementations/useTransactionInvoicesQuery";
import { useReviewsQuery } from "../../hooks/queries/implementations/useReviewsQuery";
import { usePostCheckoutMessagesQuery } from "../../hooks/queries/implementations/usePostCheckoutMessagesQuery";
import { Review } from "./Review";
import { OrdersHelper } from "../../helpers/ordersHelper";

interface Props {
    readonly order: Order,
}
export const OrderAndPaySuccess: React.FC<Props> = ({ 
    order, 
}) => {
    const theme = useQuiviTheme();

    const browserStorageService = useBrowserStorageService();
    const channelContext = useChannelContext();
    const { t } = useTranslation();

    const auth = useAuth();
    const features = channelContext.features;

    const transactionQuery = useTransactionsQuery({
        orderId: order.id,
        page: 0,
    })
    const transaction = useMemo(() => transactionQuery.data.length == 0 ? undefined : transactionQuery.data[0], [transactionQuery.data]);

    const reviewQuery = useReviewsQuery(transaction?.id);
    const review = useMemo(() => reviewQuery.data, [reviewQuery.data]);
    
    const checkoutMessagesQuery = usePostCheckoutMessagesQuery({
        merchantId: order.merchantId,
    })
    const invoicesQuery = useTransactionInvoicesQuery(transaction?.id);

    const getSteps = () => {
        if(order.scheduledTo != undefined) {
            return [t("orderAndPayResult.received"), t("orderAndPayResult.reviewing"), t("orderAndPayResult.scheduled"), t("orderAndPayResult.preparing"), t("orderAndPayResult.completed")];
        }
        return [t("orderAndPayResult.received"), t("orderAndPayResult.reviewing"), t("orderAndPayResult.preparing"), t("orderAndPayResult.completed")];
    }

    useEffect(() => {
        browserStorageService.savePaymentDivision(null);
        browserStorageService.savePaymentDetails(null);
    }, [])

    const getActiveStep = () => {
        if(order.scheduledTo == undefined) {
            switch(order.state)
            {
                case OrderState.Draft: return -1;
                case OrderState.Requested: return 1;
                case OrderState.Processing: return 2;
                case OrderState.Completed: return -1;
            }
        }

        switch(order.state)
        {
            case OrderState.Draft: return -1;
            case OrderState.Requested: return 1;
            case OrderState.Scheduled: return -1;
            case OrderState.Processing: return 3;
            case OrderState.Completed: return -1;
        }
        return -1;
    }

    const isStepCompleted = (index: number): boolean => {
        if(order.scheduledTo == undefined) {
            switch(order.state)
            {
                case OrderState.Draft: return index == 0;
                case OrderState.Requested: return index == 0;
                case OrderState.Processing: return index <= 1;
                case OrderState.Completed: return true;
            }
        }

        switch(order.state)
        {
            case OrderState.Draft: return index == 0;
            case OrderState.Requested: return index == 0;
            case OrderState.Scheduled: return index <= 2;
            case OrderState.Processing: return index <= 2;
            case OrderState.Completed: return true;
        }
        return false;
    }

    const getMessage = () => {
        switch(order.state)
        {
            case OrderState.Draft: return t("orderAndPayResult.awaitingPayment");
            case OrderState.Requested: 
            {
                if(order.scheduledTo == undefined) {
                    return t("orderAndPayResult.awaitingAcceptance");
                }

                const date = new Date(order.scheduledTo!);
                const day = date.getDate();
                const month = t(`calendar.months.${date.getMonth() + 1}`);
                const time = format(date, "HH:mm");

                return t("orderAndPayResult.awaitingScheduleAcceptance", { day: day, month: month, time: time});
            }
            case OrderState.Processing: return t("orderAndPayResult.orderIsProcessing");
            case OrderState.Completed: return t("orderAndPayResult.orderCompleted");
            case OrderState.Scheduled: 
                const date = new Date(order.scheduledTo!);
                const day = date.getDate();
                const month = t(`calendar.months.${date.getMonth() + 1}`);
                const time = format(date, "HH:mm");
                return t("orderAndPayResult.awaitingSchedule", { day: day, month: month, time: time});
        }
        return "";
    }

    const total = useMemo(() => OrdersHelper.getTotal(order), [order]);

    const receiptLines = useMemo(() => {
        const lines: ReceiptLine[] = [];

        for(const item of order.items) {
            lines.push({
                id: item.id,
                discount: 0,
                isStroke: false,
                name: item.name,
                amount: item.amount,
                quantity: item.quantity,
                subItems: item.modifiers?.map(s => s.selectedOptions).reduce((r, s) => [...r, ...s], []).map(s => ({
                    id: s.id,
                    discount: 0,
                    isStroke: false,
                    name: s.name,
                    amount: s.amount,
                    quantity: s.quantity,
                }))
            });
        }
        return lines
    }, [order.items]);

    const subTotals = useMemo(() => {
        const subTotals: ReceiptSubTotalLine[] = [];
        for(const item of order.extraCosts) {
            subTotals.push({
                amount: item.amount,
                name: t(`extraCost.${item.type}`),
            })
        }

        return subTotals
    }, [order.extraCosts]);

    const downloadInvoices = () => invoicesQuery.data.forEach(d => Files.saveFileFromURL(d.downloadUrl, d.name));

    return (
        <>
            <div style={{display: "flow-root"}}>
                <h2 className="mb-4" style={{float: "left"}}>{t("orderAndPayResult.yourOrder")}</h2>
            </div>
            <div className="mb-6">
                <Receipt
                    items={receiptLines}
                    subTotals={subTotals}
                    total={{
                        amount: total,
                        name: t("cart.totalPrice"),
                    }}
                />
            </div>
            {
                features.physicalKiosk == false &&
                <>
                {
                    features.ordering.allowsTracking &&
                    <div className="mb-8">
                        <Box sx={{ width: '100%' }}>
                            <Stepper
                                activeStep={getActiveStep()}
                                alternativeLabel
                                sx={{
                                    paddingTop: 0,

                                    "& .MuiStepIcon-root.Mui-active": { 
                                        color: theme.primaryColor.hex,
                                        '@keyframes blink': {
                                            '0%, 100%': { 
                                                color: 'white',
                                            },
                                            '50%': {
                                                color: theme.primaryColor.hex,
                                            },
                                        },
                                        animation: 'blink 1.5s infinite',
                                    },

                                    "& .MuiStepIcon-root.Mui-completed": { 
                                        color: theme.primaryColor.hex,
                                    },
                                }}
                            >
                                {
                                    getSteps().map((label, index) => 
                                        <Step key={label} completed={isStepCompleted(index)}>
                                            <StepLabel>{label}</StepLabel>
                                        </Step>
                                    )
                                }
                            </Stepper>
                            <div className="flex flex-fd-c flex-ai-c mt-5">
                                <p className="ta-c">{getMessage()}</p>
                                {order.state !== OrderState.Completed && <p className="ta-c">{t("orderAndPayResult.screenWillBeKeptUpdated")}</p>}
                            </div>
                        </Box>
                    </div>
                }
                {
                    order.state == OrderState.Completed && transaction != undefined &&
                    <div className="mb-8">
                        {
                            invoicesQuery.data.length > 0 && features.ordering.invoiceIsDownloadable &&
                            <ActionButton onClick={downloadInvoices} primaryButton={false} style={{ marginTop: "20px", marginBottom: "20px" }}>
                                <DownloadIcon />
                                <span style={{marginLeft: "10px"}}>{t("paymentResult.downloadInvoice")}</span>
                            </ActionButton>
                        }
                        {
                            !reviewQuery.isFirstLoading &&
                            (
                                review != undefined
                                ?
                                <Box
                                    className="flex flex-fd-c flex-ai-c mt-6"
                                >
                                    <h2 className="mb-3 mt-5 ta-c">{t("paymentResult.reviewSent")}</h2>
                                    <p className="ta-c">{t("paymentResult.reviewThanks")}</p>
                                    {
                                        auth.user == undefined &&
                                        <Link to={`/c/${order.channelId}`} className="secondary-button mt-6">{t("paymentResult.home")}</Link>
                                    }
                                </Box>
                                :
                                <Review transactionId={transaction.id} />
                            )
                        }
                    </div>
                }
                </>
            }
            {
                checkoutMessagesQuery.data.map(d => (
                    <Alert severity="info" title={d.title} key={`${d.title}-${d.message}`}>
                        <p style={{ fontSize:"1.5rem" }}>{d.message}</p>
                    </Alert>
                ))
            }
            <div className="mt-4 mb-4 pl-8" style={{display: "flex", justifyContent: "center", flexDirection: "column", alignItems: "center"}}>
                <h1 className="mb-2">{order.sequenceNumber}</h1>
                <QRCode
                    size={256}
                    style={{ height: "auto", maxWidth: "350px", paddingLeft: "2rem", paddingRight: "2rem" }}
                    value={order.id}
                    viewBox={`0 0 256 256`}
                />
                <h5 className="mt-2">{order.id}</h5>
            </div>
        </>
    );
}