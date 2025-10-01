import { Page } from "../../layout/Page";
import { useTranslation } from "react-i18next";
import { useEffect, useMemo } from "react";
import { useParams } from "react-router";
import { useTransactionsQuery } from "../../hooks/queries/implementations/useTransactionsQuery";
import type { Transaction } from "../../hooks/api/Dtos/transactions/Transaction";
import { LoadingAnimation } from "../../components/LoadingAnimation/LoadingAnimation";
import { ChargeMethod } from "../../hooks/api/Dtos/ChargeMethod";
import { TransactionStatus } from "../../hooks/api/Dtos/transactions/TransactionStatus";
import { CashPaymentPage } from "./Methods/CashPaymentPage";
import { PaybyrdCreditCardPaymentPage } from "./Methods/Paybyrd/PaybyrdCreditCardPaymentPage";
import { PaybyrdMbWayPaymentPage } from "./Methods/Paybyrd/PaybyrdMbWayPaymentPage";
import TerminalPaymentPage from "./Methods/TerminalPaymentPage";

interface Props {
    readonly onSuccess?: (c: Transaction) => any;
    readonly onFail?: () => any;
}
export const PaymentPage = (props: Props) => {
    const { chargeId } = useParams<{chargeId: string}>();

    const transactionsQuery = useTransactionsQuery({
        id: chargeId,
        page: 0,
    });
    const transaction = useMemo(() => transactionsQuery.data.length == 0 ? undefined : transactionsQuery.data[0], [transactionsQuery.data]);        
    const nextTransactionQuery = useTransactionsQuery(transaction?.topUpData == undefined ? undefined : {
        id: transaction.topUpData.continuationId,
        page: 0,
        pageSize: 1,
    });
    const nextTransaction = useMemo(() => nextTransactionQuery.data.length == 0 ? undefined : nextTransactionQuery.data[0], [nextTransactionQuery.data])

    const { t } = useTranslation();

    useEffect(() => {
        if(transaction == undefined) {
            return;
        }
        
        if(transaction.status == TransactionStatus.Success) {
            onInternalSuccess(transaction);
        } else if(transaction.status == TransactionStatus.Failed) {
            onInternalFail();
        }
    }, [transaction])

    const onInternalSuccess = (c: Transaction) => {
        if(props.onSuccess == undefined) {
            return;
        }

        if(c.capturedDate != undefined) {
            if(c.topUpData?.continuationId == null) {
                props.onSuccess(c);
            }
        }
    }

    const onInternalFail = () => {
        if(props.onFail == undefined) {
            return;
        }

        if(transaction == undefined) {
            return;
        }

        if(transaction.status == TransactionStatus.Failed) {
            props.onFail();
        }
    }

    if(transaction == undefined) {
        return <Page
            title={t("pay.title")}
            headerProps={{
                ordering: {
                    hideCart: true,
                }
            }}
        >
            <div className="flex flex-fd-c flex-ai-c flex-jc-c mt-10">
                <LoadingAnimation />
            </div>
        </Page>
    }

    if(transaction.method == ChargeMethod.Cash) {
        return <CashPaymentPage
            transaction={transaction}
            nextTransaction={nextTransaction}
            onSuccess={onInternalSuccess}
            onFail={onInternalFail}
        />;
    }

    if(transaction.method == ChargeMethod.MbWay) {
        return <PaybyrdMbWayPaymentPage
            transaction={transaction}
            nextTransaction={nextTransaction}
            onSuccess={onInternalSuccess}
            onFail={onInternalFail}
        />
    }

    if(transaction.method == ChargeMethod.CreditCard) {
         return <PaybyrdCreditCardPaymentPage
            transaction={transaction}
            nextTransaction={nextTransaction}
            onSuccess={onInternalSuccess}
            onFail={onInternalFail}
        />
    }

    if(transaction.method == ChargeMethod.PaymentTerminal) {
         return <TerminalPaymentPage
            transaction={transaction}
            nextTransaction={nextTransaction}
        />
    }

    return <></>
}