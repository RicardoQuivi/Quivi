import { useTranslation } from "react-i18next";
import { useMemo, useState } from "react";
import BigNumber from "bignumber.js";
import type { Transaction } from "../../hooks/api/Dtos/transactions/Transaction";
import withStyles from "@mui/styles/withStyles";
import { IconButton, Tooltip } from "@mui/material";
import { useChannelContext } from "../../context/AppContextProvider";
import { useQuiviTheme } from "../../hooks/theme/useQuiviTheme";
import { useWallet } from "../../hooks/useWallet";
import { ChargeMethod } from "../../hooks/api/Dtos/ChargeMethod";
import { ChargeMethodIcon } from "../../icons/ChargeMethodIcon";
import { Formatter } from "../../helpers/formatter";
import { InfoIcon } from "../../icons";
import { TermsAndConditionsDialog } from "./TermsAndConditionsDialog";
import { useBrowserStorageService } from "../../hooks/useBrowserStorageService";
import { Navigate } from "react-router";

export interface Props {
    readonly transaction: Transaction;
    readonly nextTransaction?: Transaction;
}

const CustomTooltip = withStyles(() => ({
    tooltip: {
      fontSize: "0.9rem",
    },
}))(Tooltip);

export const PaymentResume = ({
    transaction,
    nextTransaction,
}: Props) => {
    const browserStorageService = useBrowserStorageService();

    const { t } = useTranslation();
    const channelContext = useChannelContext();
    const theme = useQuiviTheme();
    const wallet = useWallet();
    
    const [isOpen, setIsOpen] = useState(false);
    const [serviceFeeTooltipOpen, setServiceFeeTooltipOpen] = useState(false);

    const {
        surcharge,
        amount,
        total,
     } = useMemo(() => {
        const amount = getAmount(nextTransaction ?? transaction);
        const surcharge = getServiceFee(nextTransaction ?? transaction);

        return {
            amount: amount,
            surcharge: surcharge,
            total: amount + surcharge,
        }
    }, [nextTransaction, transaction])


    const walletDiscount = nextTransaction != null ? getTotal(nextTransaction) - getTotal(transaction) : 0;
    const paymentDetails = browserStorageService.getPaymentDetails();


    if(paymentDetails == null) {
        return <Navigate to={`/c/${channelContext.channelId}`} replace />
    }

    const applyDiscount = async (_discount: number) => {
        //TODO: Implement discount via wallet

        throw new Error("Not implemented");
        // const payAtTheTableData = paymentDetails.additionalData as PayAtTheTableData;
        // const orderAndPayData = paymentDetails.additionalData as OrderAndPayData;
        // const response = await web10api.payment.CreateTopUp({
        //     amount: BigNumber(getTotal(transaction)).minus(discount).toNumber(),
        //     chargeMethod: transaction.method,
        //     continueWithCharge: {
        //         merchantId: channelContext.merchantId,
        //         tableId: channelContext.channelId,
        //         amount: transaction.payment,
        //         tip: paymentDetails.tip > 0 ? paymentDetails.tip : null,
        //         email: paymentDetails.email,
        //         vatNumber: paymentDetails.vatNumber,
        //         orderAndPayData: isOrderAndPay(paymentDetails.additionalData) ? {
        //             orderId: orderAndPayData.orderId,
        //         } : null,
        //         payAtTheTableData: isPayAtTheTable(paymentDetails.additionalData) ? {
        //             items: payAtTheTableData.items,
        //         } : null,
        //     }
        // })

        // const chargeId = response.data.id;
        // if(isOrderAndPay(paymentDetails.additionalData)) {
        //     navigate(`/pay/orders/${orderAndPayData.orderId}/${chargeId}`)
        // } else {
        //     navigate(`/pay/session/${chargeId}`)
        // } 
    }

    return (
        <>
            <div className="purchase-summary">
                <h3 className="merchant-name">
                    {!!transaction.paymentAdditionalData && channelContext.merchantName}
                    {!!transaction.topUpData && t("walletTopUp.wallet")}
                </h3>
                <div className="purchase-summary__wrapper">
                    <div className="purchase-summary__row">
                        <p style={{whiteSpace: "nowrap"}}>{t(`paymentMethods.methods.${ChargeMethod[transaction.method].toLowerCase()}`)}</p>
                        <div className="purchase-info">
                            <ChargeMethodIcon
                                chargeMethod={transaction.method}
                                color={theme.primaryColor.hex}
                                style={{
                                    height: "24px",
                                    width: "auto",
                                }}
                                height="auto"
                                width="auto"
                            />
                        </div>
                    </div>
                    {
                        surcharge > 0 &&
                        <>
                            <div className="purchase-summary__row">
                                <p>{t("paymentMethods.amount")}</p>
                                <p className="purchase-info">{Formatter.price(amount, "€")}</p>
                            </div>
                            <div className="purchase-summary__row">
                                <p style={{display: "flex", alignItems: "center"}}>
                                    {t("paymentMethods.serviceFee")} 
                                    &nbsp;
                                    <CustomTooltip title={t("paymentMethods.serviceFeeInfo")} 
                                        placement="top"
                                        open={serviceFeeTooltipOpen}
                                        disableFocusListener
                                        disableHoverListener
                                        disableTouchListener
                                        onMouseLeave={() => setServiceFeeTooltipOpen(false)}
                                        onMouseEnter={() => setServiceFeeTooltipOpen(true)}
                                        onTouchEnd={() => setServiceFeeTooltipOpen(s => !s)}>
                                        <IconButton >
                                            <InfoIcon />
                                        </IconButton>
                                    </CustomTooltip>
                                </p>
                                <p className="purchase-info">{Formatter.price(surcharge, "€")}</p>
                            </div>
                        </>
                    }
                    {
                        walletDiscount > 0 &&
                        <div className="purchase-summary__row">
                            <p>{t("paymentMethods.walletDiscount")}</p>
                            <p className="purchase-amount">{Formatter.price(-walletDiscount, "€")}</p>
                        </div>
                    }
                    <div className="purchase-summary__row">
                        <p>{t("paymentMethods.total")}</p>
                        <p className="purchase-amount">{Formatter.price(BigNumber(total).minus(walletDiscount).toNumber(), "€")}</p>
                    </div>
                </div>
            </div>
            <div>
                {
                    transaction.method !== ChargeMethod.Wallet && transaction.topUpData == null &&
                    wallet.isAvailable &&
                    0 < wallet.balance && wallet.balance < total &&
                    walletDiscount === 0 &&
                    <div className="purchase-summary__row">
                        <p style={{ fontSize: "0.8rem", cursor: "pointer", textDecoration: "underline", marginBottom: "1rem", marginLeft: "auto", marginRight: "0"}} onClick={() => applyDiscount(wallet.balance)}>
                            {t("paymentMethods.applyWalletDiscount", { amount: Formatter.price(wallet.balance, "€") })}
                        </p>
                    </div>
                }
            </div>
            {
                surcharge > 0 &&
                <>
                    <p style={{marginTop: "-10px", fontSize: "0.8rem", cursor: "pointer"}} onClick={() => setIsOpen(true)}>{t("paymentMethods.termsAndConditions")}</p>
                    <br/>
                    <TermsAndConditionsDialog isOpen={isOpen} onClose={() => setIsOpen(false)} />
                </>
            }
        </>
    )
}

const getAmount = (c: Transaction) =>  c.payment + c.tip;
const getServiceFee = (c: Transaction) => c.surcharge;
const getTotal = (c: Transaction) => getAmount(c) + getServiceFee(c);