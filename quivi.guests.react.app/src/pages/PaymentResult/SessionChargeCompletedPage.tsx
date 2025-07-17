import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { Page } from "../../layout/Page";
import { LoadingAnimation } from "../../components/LoadingAnimation/LoadingAnimation";
import { Navigate, useParams } from "react-router";
import { useChannelContext } from "../../context/AppContextProvider";
import { useTransactionsQuery } from "../../hooks/queries/implementations/useTransactionsQuery";
import { TransactionStatus } from "../../hooks/api/Dtos/transactions/TransactionStatus";
import { SessionPaymentSuccess } from "./SessionPaymentSuccess";
import { SessionPaymentError } from "./SessionPaymentError";

export const SessionChargeCompletedPage = () => {
    const { chargeId } = useParams<{chargeId: string}>(); 
    const channelContext = useChannelContext();
    
    if(chargeId == undefined) {
        return <Navigate to={`/c/${channelContext.channelId}`} replace />
    }

    const { t } = useTranslation();
    const transactionsQuery = useTransactionsQuery({
        id: chargeId,
        page: 0,
        pageSize: 1,
    })
    const transaction = useMemo(() => transactionsQuery.data.length == 0 ? undefined : transactionsQuery.data[0], [transactionsQuery.data]);

    return <Page headerProps={{hideCart: true}}>
    {
        transactionsQuery.isFirstLoading
        ?
        <div className="loader-container">
            <LoadingAnimation />
        </div>
        :
        (
            transaction != undefined && transaction.status == TransactionStatus.Success
            ?
                <SessionPaymentSuccess transaction={transaction} />
            :
                <SessionPaymentError message={t("paymentMethods.paymentFailed")} />
        )
    }
    </Page>
}