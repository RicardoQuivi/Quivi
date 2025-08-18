import { useState } from "react";
import { useTranslation } from "react-i18next";
import { toast } from "react-toastify";
import { Page } from "../../layout/Page";
import { useBrowserStorageService } from "../../hooks/useBrowserStorageService";
import { useQuiviTheme } from "../../hooks/theme/useQuiviTheme";
import { isOrderAndPay, isPayAtTheTable, type OrderAndPayData, type PayAtTheTableData } from "../Pay/paymentTypes";
import { Navigate, useNavigate } from "react-router";
import { useChannelContext, useCurrentMerchant } from "../../context/AppContextProvider";
import { InfoIcon, QuiviFullIcon } from "../../icons";
import { Formatter } from "../../helpers/formatter";
import { Grid, Tooltip } from "@mui/material";
import { LoadingAnimation } from "../../components/LoadingAnimation/LoadingAnimation";
import { ChargeMethod } from "../../hooks/api/Dtos/ChargeMethod";
import { Fees } from "../../helpers/fees";
import { ChargeMethodIcon } from "../../icons/ChargeMethodIcon";
import { usePaymentMethodsQuery } from "../../hooks/queries/implementations/usePaymentMethodsQuery";
import type { PaymentMethod } from "../../hooks/api/Dtos/paymentmethods/PaymentMethod";
import { useSessionsQuery } from "../../hooks/queries/implementations/useSessionsQuery";
import { Calculations } from "../../helpers/calculations";
import { useWallet } from "../../hooks/useWallet";
import { useTransactionMutator } from "../../hooks/mutators/useTransactionMutator";

interface Props {
    readonly isFreePayment?: boolean
}
export const PaymentMethodsPage = (props: Props) => {
    const browserStorageService = useBrowserStorageService();
    const { t } = useTranslation();
    const channelContext = useChannelContext();
    const merchant = useCurrentMerchant();

    const navigate = useNavigate();

    const theme = useQuiviTheme();
    const wallet = useWallet();

    const sessionQuery = useSessionsQuery({
        channelId: channelContext.channelId,
    })
    const paymentMethodsQuery = usePaymentMethodsQuery({
        channelId: channelContext.channelId,
        page: 0,
        pageSize: undefined,
    })

    const transactionMutator = useTransactionMutator();

    const [walletTooltipOpen, setWalletTooltipOpen] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);

    const paymentDetails = browserStorageService.getPaymentDetails();
    if(paymentDetails == null) {
        return <Navigate to={`/c/${channelContext.channelId}`} replace />
    }

    if(paymentDetails.total == 0) {
        return <Navigate to={`/c/${channelContext.channelId}/session/summary`} replace />
    }
    
    const paymentDivision = browserStorageService.getPaymentDivision() ?? {
        divideEvenly: {
            payForPeople: 1,
            peopleAtTheTable: 1,
        },
        selectedItems: [],
    };
    
    const submitPaymentDetails = async (paymentMethod: PaymentMethod) => {
        if (props.isFreePayment != true && isPayAtTheTable(paymentDetails.additionalData) && isAmountPayable(sessionQuery.data?.unpaid ?? 0, paymentDetails.amount) == false) {
            navigate(`/c/${channelContext.channelId}/session/summary`, {
                replace: true,
            });
            toast.info(t("pay.sessionHasUpdates"), {
                icon: <InfoIcon color={theme.primaryColor.hex} />,
            });
            return;
        }

        browserStorageService.savePaymentDetails(paymentDetails);
        browserStorageService.savePaymentDivision(paymentDivision);

        const payAtTheTableData = paymentDetails.additionalData as PayAtTheTableData;
        const orderAndPayData = paymentDetails.additionalData as OrderAndPayData;
        setIsSubmitting(true);
        try {
            const charge = await transactionMutator.create({
                channelId: channelContext.channelId,
                amount: paymentDetails.amount,
                tip: paymentDetails.tip > 0 ? paymentDetails.tip : 0,
                email: paymentDetails.email,
                vatNumber: paymentDetails.vatNumber,
                merchantAcquirerConfigurationId: paymentMethod.id,
                orderAndPayData: isOrderAndPay(paymentDetails.additionalData) ? {
                    orderId: orderAndPayData.orderId,
                } : undefined,
                payAtTheTableData: props.isFreePayment == false && isPayAtTheTable(paymentDetails.additionalData) ? {
                    items: payAtTheTableData.items,
                } : undefined,
            });
            const chargeId = charge.id;

            if(isOrderAndPay(paymentDetails.additionalData)) {
                const orderAndPayData = paymentDetails.additionalData as OrderAndPayData;
                navigate(`/c/${channelContext.channelId}/pay/orders/${orderAndPayData.orderId}/${chargeId}`);
            } else {
                navigate(`/c/${channelContext.channelId}/session/pay/${chargeId}`)
            }
        } catch (err) {
            navigate(-2);
        }
    }

    const isPaymentMethodAvailable = (paymentMethods: PaymentMethod[], m: ChargeMethod, generate: (payment: PaymentMethod) => React.ReactNode): React.ReactNode | undefined => {
        const found = paymentMethods.find(p => p.method == m);
        if(found == undefined) {
            return undefined;
        }

        if(m == ChargeMethod.Wallet) {
            if(wallet.isAvailable == false) {
                return generate(found);
            }

            const totalAmount = Fees.getAmountWithFee(merchant.fees, paymentDetails.total, m);
            if(wallet.balance >= totalAmount){
                return generate(found);
            }

            return undefined;
        }

        return generate(found);
    }

    const getWalletButton = (paymentMethod: PaymentMethod) => {
        const button = <button
            type="button"
            id={ChargeMethod.Wallet.toString()}
            onClick={() => {
                if (wallet.isAvailable) {
                    submitPaymentDetails(paymentMethod)
                    return;
                }

                navigate("/user/login");
            }}
            style={{
                opacity: wallet.isAvailable ? "1": "0.6", 
                display: "flex",
                flexDirection: "column",
                overflowY: "hidden",
            }}
        >
            <QuiviFullIcon color={theme.primaryColor.hex} style={{
                flexGrow: 1,
                maxHeight: "3rem",
                maxWidth: "80%",
            }}/>
            <span>
                <b>{t("userHome.balance")}: </b>{Formatter.price(wallet.balance, "€")}
            </span>
        </button>;

        if(wallet.isAvailable) {
            return button;
        }

        return (
            <Tooltip
                title={t("paymentMethods.walletUnavailable")} 
                placement="top"
                open={walletTooltipOpen}
                disableFocusListener
                disableHoverListener
                disableTouchListener
                onMouseLeave={() => setWalletTooltipOpen(false)}
                onMouseEnter={() => setWalletTooltipOpen(true)}
                onTouchEnd={() => setWalletTooltipOpen(s => !s)}>
                    {button}
            </Tooltip>
        )
    }

    return <Page title={t("pay.title")}>
        <section className="pay" style={{marginBottom: 0, height: "100%", display: "flex", flexDirection: "column"}}>
            <div className="purchase-summary">
                <h3 className="merchant-name">{t("paymentMethods.payTo")} {channelContext.merchantName}</h3>
                <div className="purchase-summary__wrapper">
                    <div className="purchase-summary__row">
                        <p>{t("paymentMethods.amount")}</p>
                        <p className="purchase-amount">{Formatter.price(paymentDetails.total, "€")}</p>
                    </div>
                </div>
            </div>
            {
                paymentMethodsQuery.isFirstLoading || sessionQuery.isFirstLoading || isSubmitting
                ?
                <div className="loader-container">
                    <LoadingAnimation />
                </div>
                :
                <div className="method mb-6" style={{flexGrow: 1, display: "flex", flexDirection: "column"}}>
                    <label className="mb-4">{t("paymentMethods.choosePaymentMethod")}</label>
                    <div style={{display: "flex"}}>
                        <Grid container spacing={1} sx={{justifyContent: "center", width: "100%"}}>
                            {
                                isPaymentMethodAvailable(paymentMethodsQuery.data, ChargeMethod.PaymentTerminal, p => (
                                    <Grid size={{ xs: 6, sm: 4, md: 3, lg: 2, xl: 2 }} className="method--option">
                                        <button
                                            type="button"
                                            id={ChargeMethod.PaymentTerminal.toString()}
                                            onClick={() => submitPaymentDetails(p)}
                                        >
                                            <ChargeMethodIcon chargeMethod={ChargeMethod.PaymentTerminal} />
                                        </button>
                                    </Grid>
                                ))
                            }
                            {
                                isPaymentMethodAvailable(paymentMethodsQuery.data, ChargeMethod.MbWay, p => (
                                    <Grid size={{ xs: 6, sm: 4, md: 3, lg: 2, xl: 2 }} className="method--option">
                                        <button
                                            type="button"
                                            id={ChargeMethod.MbWay.toString()}
                                            onClick={() => submitPaymentDetails(p)}
                                        >
                                            <ChargeMethodIcon chargeMethod={ChargeMethod.MbWay} />
                                        </button>
                                    </Grid>
                                ))
                            }
                            {
                                isPaymentMethodAvailable(paymentMethodsQuery.data, ChargeMethod.CreditCard, p => (
                                    <Grid size={{ xs: 6, sm: 4, md: 3, lg: 2, xl: 2 }} className="method--option">
                                        <button
                                            type="button"
                                            id={ChargeMethod.CreditCard.toString()}
                                            onClick={() => submitPaymentDetails(p)}
                                        >
                                            <ChargeMethodIcon chargeMethod={ChargeMethod.CreditCard} />
                                        </button>
                                    </Grid>
                                ))
                            }
                            {
                                isPaymentMethodAvailable(paymentMethodsQuery.data, ChargeMethod.TicketRestaurantMobile, p => (
                                    <Grid size={{ xs: 6, sm: 4, md: 3, lg: 2, xl: 2 }} className="method--option">
                                        <button
                                            type="button"
                                            id={ChargeMethod.TicketRestaurantMobile.toString()}
                                            onClick={() => submitPaymentDetails(p)}
                                        >
                                            <ChargeMethodIcon chargeMethod={ChargeMethod.TicketRestaurantMobile} />
                                        </button>
                                    </Grid>
                                ))
                            }
                            {
                                isPaymentMethodAvailable(paymentMethodsQuery.data, ChargeMethod.Wallet, p => (
                                    <Grid size={{ xs: 6, sm: 4, md: 3, lg: 2, xl: 2 }} className="method--option">
                                        {getWalletButton(p)}
                                    </Grid>
                                ))
                            }                                
                            {
                                isPaymentMethodAvailable(paymentMethodsQuery.data, ChargeMethod.Cash, p => (
                                    <Grid size={{ xs: 6, sm: 4, md: 3, lg: 2, xl: 2 }} className="method--option">
                                        <button
                                            type="button"
                                            id={ChargeMethod.Cash.toString()}
                                            onClick={() => submitPaymentDetails(p)}
                                        >
                                            <ChargeMethodIcon chargeMethod={ChargeMethod.Cash} color={theme.primaryColor.hex} />
                                        </button>
                                    </Grid>
                                ))
                            }
                        </Grid>
                    </div>
                </div>
            }
        </section>
    </Page>
}

const isAmountPayable = (unpaidAmount: number, amount: number): boolean => {
    return Calculations.roundUp(unpaidAmount) >= Calculations.roundUp(amount);
}