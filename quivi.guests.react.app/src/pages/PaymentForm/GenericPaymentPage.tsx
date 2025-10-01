import { useTranslation } from "react-i18next";
import { Page } from "../../layout/Page"
import { ButtonsSection } from "../../layout/ButtonsSection";
import { useEffect, useState } from "react";
import Dialog from "../../components/Shared/Dialog";
import type { Transaction } from "../../hooks/api/Dtos/transactions/Transaction";
import { TransactionStatus } from "../../hooks/api/Dtos/transactions/TransactionStatus";
import { CloseIcon, ExpiredIcon } from "../../icons";
import { useChannelContext } from "../../context/AppContextProvider";
import { useNavigate } from "react-router";

export interface GenericPaymentPageProps {
    readonly transaction: Transaction;
    readonly nextTransaction?: Transaction;
    readonly children: React.ReactNode;
    readonly footer?: React.ReactNode;
    readonly onSuccess?: (c: Transaction) => any;
    readonly onFail?: () => any;
}
export const GenericPaymentPage = ({
    transaction,
    nextTransaction,
    children,
    footer,
    onSuccess,
    onFail,
}: GenericPaymentPageProps) => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const channelContext = useChannelContext();
    const [showPaymentNoteModal, setShowPaymentNoteModal] = useState(() => channelContext.features.showPaymentNote);
    
    useEffect(() => {
        if(transaction.topUpData == undefined) {
            return;
        }

        if(nextTransaction != undefined) {
            if(nextTransaction.status == TransactionStatus.Success) {
                onSuccess?.(nextTransaction);
                return;
            }

            if(nextTransaction.status == TransactionStatus.Failed || transaction.status == TransactionStatus.Failed) {
                onFail?.();
                return;
            }
        } else {
            if(transaction.status == TransactionStatus.Success) {
                onSuccess?.(transaction);
                return;
            }

            if(transaction.status == TransactionStatus.Failed) {
                onFail?.();
                return;
            }
        } 
    }, [transaction, nextTransaction])

    const getFooter = (c: Transaction) => {
        if(c.status == TransactionStatus.Expired) {
            return <ButtonsSection>
                <button type="button" className="primary-button" onClick={() => navigate(-1)}>
                    {t("paymentMethods.ok")}
                </button>
                {undefined}
            </ButtonsSection>
        }

        return footer;
    }

    return <Page
        title={t("pay.title")}
        headerProps={{
            ordering: {
                hideCart: true,
            }
        }}
        footer={getFooter(transaction)}
    >
        {
            transaction.status == TransactionStatus.Expired
            ?
                <div style={{marginTop: "2rem", display: "flex", flexDirection: "column", alignItems: "center", textAlign: "center"}}>
                    <ExpiredIcon style={{marginBottom: "2rem", width: "100%", height: "auto", maxWidth: "180px"}}/>
                    <p style={{marginBottom: "2rem"}}>{t("paymentMethods.expired")}</p>
                </div>
            :
                <section
                    className="pay"
                    style={{
                        marginBottom: 0,
                        height: "100%",
                        display: "flex",
                        flexDirection: "column",
                    }}
                >
                    {children}
                </section>
        }
        <Dialog isOpen={showPaymentNoteModal && transaction.status == TransactionStatus.Processing} onClose={() => setShowPaymentNoteModal(false)}>
            <div className="container" style={{ paddingTop: "1.75rem", paddingBottom: "1.75rem" }}>
                <div className="modal__header">
                    <h3>{t("paymentMethods.warning")}</h3>
                    <div className="close-icon" onClick={() => setShowPaymentNoteModal(false)}>
                        <CloseIcon />
                    </div>
                </div>
                <hr/>
                <p className="mb-5">
                    {t("paymentMethods.merchantCustomPaymentNotes")}
                </p>
                <br/>                
                <ButtonsSection>
                    <button type="button" className="primary-button" onClick={() => setShowPaymentNoteModal(false)}>
                        {t("paymentMethods.continue")}
                    </button>
                    {undefined}
                </ButtonsSection>
            </div>
        </Dialog>
    </Page>
}