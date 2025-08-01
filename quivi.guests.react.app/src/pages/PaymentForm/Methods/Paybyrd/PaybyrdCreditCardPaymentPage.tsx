import { useTranslation } from "react-i18next";
import { useEffect, useRef, useState } from "react";
import type { Transaction } from "../../../../hooks/api/Dtos/transactions/Transaction";
import { GenericPaymentPage } from "../../GenericPaymentPage";
import { PaymentResume } from "../../PaymentResume";
import { ButtonsSection } from "../../../../layout/ButtonsSection";
import LoadingButton from "../../../../components/Buttons/LoadingButton";
import { ChargeMethod } from "../../../../hooks/api/Dtos/ChargeMethod";
import Dialog from "../../../../components/Shared/Dialog";
import Paybyrd3DSFrame from "./Paybyrd3DSFrame";
import CardCollect from "@paybyrd/card-collect";
import { useTransactionMutator } from "../../../../hooks/mutators/useTransactionMutator";
import { Box, Skeleton, Stack } from "@mui/material";
import { useLocation } from "react-router";
import { TransactionStatus } from "../../../../hooks/api/Dtos/transactions/TransactionStatus";
import { CircularProgressTracker } from "../../../../components/Progress/CircularProgressTracker";
import type { CardCollectResponse } from "@paybyrd/card-collect/dist/types/types";
import cssString from '../../../../styles/paybyrd/card.collect.css?raw';

interface Props {
    readonly transaction: Transaction;
    readonly nextTransaction?: Transaction;
    readonly onSuccess?: (transaction: Transaction) => void | Promise<void>;
    readonly onFail?: () => void | Promise<void>;
}
export const PaybyrdCreditCardPaymentPage : React.FC<Props> = ({
    transaction,
    nextTransaction,
    onSuccess,
    onFail,
}) => {
    const { t } = useTranslation();
    const transactionMutator = useTransactionMutator();
    const location = useLocation();

    const [threeDSUrl, setThreeDSUrl] = useState("");
    const [isProcessingPayment, setIsProcessingPayment] = useState(false);

    const [cc, setCC] = useState<CardCollectResponse>();
    const hasInitialized = useRef(false);

    useEffect(() => {
        const setup = async () => {
            try {
                if (!hasInitialized.current) {
                    hasInitialized.current = true;
                    const form = await CardCollect({
                        displayErrors: true,
                        validateOnChange: true,
                        displayHelpIcons: true,
                        i18nMessages: {
                            requiredField: t("acquirer.mustSpecifyCardNumber"),
                            invalidCardNumber: t("acquirer.mustSpecifyCardNumber"),
                            invalidExpirationDate: t("acquirer.mustSpecifyExpiryDate"),
                            invalidCVV: t("acquirer.mustSpecifyCVV"),

                            holderName: t("acquirer.cardHolderPlaceholder"),
                            cardNumber: t("acquirer.cardNumberPlaceholder"),
                            expDate: `${t("acquirer.monthPlaceholder")}/${t("acquirer.yearPlaceholder")}`,
                            cvv: t("acquirer.cvvPlaceholder"),
                        },
                        env: !!import.meta.env.VITE_IS_PRODUCTION ? 'production' : 'stage',
                        css: cssString,
                        
                    });
                    setCC(form);
                }
            } catch {
                hasInitialized.current = false;
                await setup();
            }
        };

        setup();
    }, []);


    const submit = async () => {
        if(cc == undefined) {
            return;
        }
        
        try {
            setIsProcessingPayment(true);
            const submissionResult = await cc.cardCollect_submit();
            if(submissionResult.status != 200) {
                throw Error();
            }

            const fullUrl = `${window.location.origin}${location.pathname}#done`;
            const response = await transactionMutator.processPaybyrd(transaction, {
                tokenId: submissionResult.data.tokenId,
                redirectUrl: fullUrl,
            });

            if(!!response.threeDsUrl) {
                setThreeDSUrl(response.threeDsUrl);
            } else {
                onSuccess?.(response.entity);
            }
        } catch (error) {
            setIsProcessingPayment(false);
        }
    }

    const getFooter = () => {
        return <ButtonsSection>
            <LoadingButton
                primaryButton={true}
                onClick={submit}
                style={{marginTop: "1rem"}}
                isLoading={isProcessingPayment || cc == undefined}
            >
                {t("paymentMethods.confirm")}
            </LoadingButton>
            {undefined}
        </ButtonsSection>
    }

    if(transaction.method != ChargeMethod.CreditCard)
        return <></>

    return (
        <GenericPaymentPage 
            transaction={transaction}
            nextTransaction={nextTransaction}
            footer={getFooter()}
            onFail={onFail}
            onSuccess={onSuccess}
        >
            {
                transaction.status != TransactionStatus.Processing
                ?
                <>
                    <PaymentResume transaction={transaction} nextTransaction={nextTransaction} />
                    {
                        <Box
                            className="checkout-payment"
                            sx={{
                                "& iframe": {
                                    height: "unset !important",
                                }
                            }}
                        >
                            <Stack id="cardCollect" direction="column" gap={1}>
                                <div id="cc-holder" className="form-field" data-placeholder="Card Holder">
                                {
                                    cc == undefined &&
                                    <Skeleton width="100%" height={55} />
                                }
                                </div>
                                <div id="cc-number" className="form-field" data-placeholder="Card Number">
                                {
                                    cc == undefined &&
                                    <Skeleton width="100%" height={55} />
                                }
                                </div>
                                <Stack direction="row" gap={1} width="100%">
                                    <div id="cc-expiration-date" className="form-field" data-placeholder="MM/YY" style={{ width: "100%"}}>
                                    {
                                        cc == undefined &&
                                        <Skeleton width="100%" height={55} />
                                    }
                                    </div>
                                    <div id="cc-cvc" className="form-field" data-placeholder="CVV" style={{ width: "100%"}}>
                                    {
                                        cc == undefined &&
                                        <Skeleton width="100%" height={55} />
                                    }
                                    </div>
                                </Stack>
                            </Stack>
                        </Box>
                    }
                </>
                :
                <div>
                    <div className="tutorial__header">
                        <h2>{t("paymentMethods.processingPayment")}</h2>
                        <p>{t("paymentMethods.processingPaymentDescription")}</p>
                    </div>
                    <div className="tutorial__step" style={{marginTop: "0.75rem"}}>
                        <span className="tutorial__step-number">1</span>
                        <p>{t("paymentMethods.openBankApp")}</p>
                    </div>
                    <div className="tutorial__step">
                        <span className="tutorial__step-number">2</span>
                        <p>{t("paymentMethods.authorizeAppPayment")}</p>
                    </div>
                    <div style={{marginTop: "3rem"}}>
                        <div style={{position: "relative", height: "100px", width: "100px", margin: "0 auto"}}>
                            <CircularProgressTracker startDate={new Date(transaction.lastModified)} totalMinutes={0} />
                        </div>
                    </div>

                    <Dialog isOpen={!!threeDSUrl} onClose={() => setThreeDSUrl("")} style={{height: "80vh"}} disableClosing={true}>
                        <Paybyrd3DSFrame url={threeDSUrl}/>
                    </Dialog>
                </div>
            }
        </GenericPaymentPage>
    )
}