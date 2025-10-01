import { useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { Alert, LinearProgress } from "@mui/material";
import { Link } from "react-router";
import { useBrowserStorageService } from "../../hooks/useBrowserStorageService";
import type { Transaction } from "../../hooks/api/Dtos/transactions/Transaction";
import { useReviewsQuery } from "../../hooks/queries/implementations/useReviewsQuery";
import { useAuth } from "../../context/AuthContext";
import { CheckIcon, DownloadIcon } from "../../icons";
import { useTransactionInvoicesQuery } from "../../hooks/queries/implementations/useTransactionInvoicesQuery";
import { useChannelContext } from "../../context/AppContextProvider";
import ActionButton from "../../components/Buttons/ActionButton";
import { Formatter } from "../../helpers/formatter";
import { LoadingAnimation } from "../../components/LoadingAnimation/LoadingAnimation";
import { Files } from "../../helpers/files";
import { useSessionsQuery } from "../../hooks/queries/implementations/useSessionsQuery";
import { Calculations } from "../../helpers/calculations";
import { SyncStatus } from "../../hooks/api/Dtos/transactions/SyncStatus";
import { usePostCheckoutLinksQuery } from "../../hooks/queries/implementations/usePostCheckoutLinksQuery";
import { ExternalLink } from "./ExternalLink";
import { Review } from "./Review";

interface Props{
    readonly transaction: Transaction,
}
export const SessionPaymentSuccess: React.FC<Props> = ({ 
    transaction, 
}) => {
    const channelContext = useChannelContext();
    const browserStorageService = useBrowserStorageService();
    const { t } = useTranslation();
    const auth = useAuth();
    
    const reviewQuery = useReviewsQuery(transaction?.id);
    const review = useMemo(() => reviewQuery.data, [reviewQuery.data]);

    const sessionQuery = useSessionsQuery({
        channelId: channelContext.channelId,
    });
    const session = useMemo(() => sessionQuery.data == undefined ? undefined : sessionQuery.data, [sessionQuery.data]);
    const sessionRemainder = useMemo(() => Calculations.roundUp(session?.unpaid ?? 0), [session]);

    const [state, setState] = useState({
        reviewStars: 0,
        downloadInvoice: false,
        isInvoiceTimedOut: false,
    })
    const checkoutLinksQuery = usePostCheckoutLinksQuery({
        merchantId: channelContext.merchantId,
    });
    const invoicesQuery = useTransactionInvoicesQuery(transaction.id);

    const downloadInvoices = () => invoicesQuery.data.forEach(d => Files.saveFileFromURL(d.downloadUrl, d.name));

    useEffect(() => {
        browserStorageService.savePaymentDivision(null);
        browserStorageService.savePaymentDetails(null);
    }, [])
    
    useEffect(() => {
        if(invoicesQuery.data.length == 0) {
            return;
        }

        setState(s => ({...s, isInvoiceTimedOut: false}));
    }, [ invoicesQuery.data])

    useEffect(() => {
        if(channelContext.features.payAtTheTable.allowsInvoiceDownloads == false) {
            return;
        }
        
        if(transaction.syncStatus != SyncStatus.Synced) {
            return;
        }

        const timeout = setTimeout(() => {
            setState(s => ({...s, isInvoiceTimedOut: true}));
        }, 30000);
        return () => clearTimeout(timeout);
    }, [channelContext.features.payAtTheTable.allowsInvoiceDownloads, transaction])

    return (
        <>
            <>
                <div className="mb-8">
                    {
                        transaction.syncStatus == SyncStatus.Syncing &&
                        <div className="flex flex-fd-c flex-ai-c flex-jc-c mt-5">
                            <LoadingAnimation />
                            <p className="mt-8">{t("paymentResult.verifyingPayment")}</p>
                        </div>
                    }
                    {
                        transaction.syncStatus == SyncStatus.Failed &&
                        <>
                            <Alert variant="outlined" severity="success">
                                {t("paymentResult.paymentCompleted")}
                            </Alert>

                            <Alert variant="outlined" severity="warning">
                                {t("paymentResult.syncFailed")}
                            </Alert>
                        </>
                    }
                    {
                        transaction.syncStatus == SyncStatus.Synced && 
                        <>
                            <Alert variant="outlined" severity="success">
                                {t("paymentResult.paymentCompleted")}
                            </Alert>
                            {
                                sessionRemainder > 0 &&
                                <Alert variant="outlined" severity="info" sx={{mt: "0.75rem"}}>
                                    {t("paymentResult.paymentCompletedDescription", { pendingAmount: `${Formatter.price(sessionRemainder, "€")}` })}
                                </Alert>
                            }
                            {
                                invoicesQuery.data.length > 0
                                ?
                                <ActionButton onClick={downloadInvoices} primaryButton={false} style={{ marginTop: "0.75rem", marginBottom: "0.75rem" }}>
                                    <DownloadIcon />
                                    <span style={{marginLeft: "10px"}}>{t("paymentResult.downloadInvoice")}</span>
                                </ActionButton>
                                :
                                (
                                    channelContext.features.payAtTheTable.allowsInvoiceDownloads &&
                                    (
                                        state.isInvoiceTimedOut
                                        ?
                                        <Alert variant="outlined" severity="warning" sx={{mt: "0.75rem",}}>
                                            {t("paymentResult.noInvoiceAvailable")}
                                        </Alert>
                                        :
                                        <Alert variant="outlined" severity="info" sx={{mt: "0.75rem", "& .MuiAlert-message":{ flex: "1 1 auto"}}}>
                                            {t("paymentResult.gettingInvoice")}
                                            <LinearProgress color="inherit" style={{width: "100%"}}/>
                                        </Alert>
                                    )
                                )
                            }
                        </>
                    }
                </div>
                {
                    reviewQuery.isFirstLoading == false &&
                    (
                        review == undefined
                        ?
                        <Review transactionId={transaction.id} />
                        :
                        (
                            state.reviewStars === 5 && checkoutLinksQuery.data.length > 0 ?
                                <div className="external-links__container">
                                    <Alert variant="standard" severity="success" icon={<CheckIcon />}>
                                        {t("paymentResult.reviewSent")}
                                    </Alert>
                                    <h4>{t("paymentResult.externalLinksTitle")}</h4>
                                    <div className="external-links__grid">
                                    {
                                        checkoutLinksQuery.data.map(item => <ExternalLink key={item.id} name={item.url} url={item.url} logoUrl={item.logoUrl} />)
                                    }
                                    </div>
                                </div>
                            :
                                <div className="flex flex-fd-c flex-ai-c mt-6">
                                    <h2 className="mb-3 mt-5 ta-c">{t("paymentResult.reviewSent")}</h2>
                                    <p className="ta-c">{t("paymentResult.reviewThanks")}</p>
                                    {
                                        auth.user == undefined &&
                                        <Link to={`/c/${channelContext.channelId}`} className="secondary-button mt-6">{t("paymentResult.home")}</Link>
                                    }
                                </div>
                        )
                    )
                }
            </>
            {   
                auth.user != undefined &&
                review != undefined && 
                <div className="container">
                    <Link to={`/c/${channelContext.channelId}`} className="secondary-button mb-4">{t("paymentResult.home")}</Link>
                    <Link to="/user/home" className="secondary-button">{t("paymentResult.seeAccount")}</Link>
                </div>
            }
        </>
    );
}