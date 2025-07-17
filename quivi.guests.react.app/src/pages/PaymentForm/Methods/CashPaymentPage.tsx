import { useTranslation } from "react-i18next";
import { useState } from "react";
import LoadingButton from "../../../components/Buttons/LoadingButton";
import { ButtonsSection } from "../../../layout/ButtonsSection";
import type { Transaction } from "../../../hooks/api/Dtos/transactions/Transaction";
import { ChargeMethod } from "../../../hooks/api/Dtos/ChargeMethod";
import { GenericPaymentPage } from "../GenericPaymentPage";
import { PaymentResume } from "../PaymentResume";
import { useTransactionMutator } from "../../../hooks/mutators/useTransactionMutator";

interface Props {
    readonly transaction: Transaction;
    readonly nextTransaction?: Transaction;
    readonly onSuccess?: (transaction: Transaction) => void | Promise<void>;
    readonly onFail?: () => void | Promise<void>;
}
export const CashPaymentPage = ({
    transaction,
    nextTransaction,
    onFail,
    onSuccess,
}: Props) => {
    const { t } = useTranslation(); 
    
    const transactionMutator = useTransactionMutator();
    const [isLoading, setIsLoading] = useState(false);

    const process = async () => {
        setIsLoading(true);

        try {
            const result = await transactionMutator.processCash(transaction);
            onSuccess?.(result);
        } catch {
            setIsLoading(false);
            onFail?.();
        }
    }

    const getFooter = () => {
        return <>
            <label style={{ marginBottom: "24px", textAlign: "center" }}>{t("paymentMethods.confirmPaymentQuestion")}</label>
            <ButtonsSection>
                <LoadingButton onClick={process} isLoading={isLoading}>
                    {t("paymentMethods.confirm")} 
                </LoadingButton>
                {undefined}
            </ButtonsSection>
        </>
    }

    if(transaction.method != ChargeMethod.Cash)
        return <></>

    return (
        <GenericPaymentPage footer={getFooter()} transaction={transaction} nextTransaction={nextTransaction} onFail={onFail} onSuccess={onSuccess}>
            <PaymentResume transaction={transaction} nextTransaction={nextTransaction} />
        </GenericPaymentPage>
    )
}