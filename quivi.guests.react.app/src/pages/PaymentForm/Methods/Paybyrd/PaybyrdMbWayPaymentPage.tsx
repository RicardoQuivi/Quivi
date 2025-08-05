import { useTranslation } from "react-i18next";
import { useMemo } from "react";
import { asPaybyrdMbWayAdditionalData, type PaybyrdMbWayAdditionalData, type Transaction } from "../../../../hooks/api/Dtos/transactions/Transaction";
import { GenericPaymentPage } from "../../GenericPaymentPage";
import { PaymentResume } from "../../PaymentResume";
import { ChargeMethod } from "../../../../hooks/api/Dtos/ChargeMethod";
import { useLocation } from "react-router";
import { useQuiviTheme, type IColor } from "../../../../hooks/theme/useQuiviTheme";

interface Props {
    readonly transaction: Transaction;
    readonly nextTransaction?: Transaction;
    readonly onSuccess?: (transaction: Transaction) => void | Promise<void>;
    readonly onFail?: () => void | Promise<void>;
}
export const PaybyrdMbWayPaymentPage : React.FC<Props> = ({
    transaction,
    nextTransaction,
    onSuccess,
    onFail,
}) => {
    const { i18n } = useTranslation();
    const location = useLocation();
    const quiviTheme = useQuiviTheme();

    const additionalData = useMemo(() => asPaybyrdMbWayAdditionalData(transaction.additionalData), [transaction])
    const source = useMemo(() => getSource(additionalData, `${window.location.origin}${location.pathname}`, i18n.language, quiviTheme.primaryColor), [
        additionalData, 
        window.location.origin,
        location.pathname,
        i18n.language,
        quiviTheme.primaryColor,
    ])

    if(transaction.method != ChargeMethod.MbWay) {
        return <></>
    }

    if(source == undefined) {
        return <></>
    } 
    
    return (
        <GenericPaymentPage 
            transaction={transaction}
            nextTransaction={nextTransaction}
            onFail={onFail}
            onSuccess={onSuccess}
        >
            <PaymentResume transaction={transaction} nextTransaction={nextTransaction} />
            <iframe
                key={source}
                src={source}
                style={{
                    width: "100%",
                    height: "100%",
                    border: "none",
                }}
            />
        </GenericPaymentPage>
    )
}

const getSource = (data: PaybyrdMbWayAdditionalData | undefined, redirectUrl: string, locale: string, color: IColor) => {
    if(data == undefined) {
        return undefined;
    }

    let configData = {
        redirectUrl: redirectUrl,
        orderId: data.OrderId,
        checkoutKey: data.CheckoutKey,
        locale: locale,
        autoRedirect: false,
        compact: true,
        defaultPaymentMethod: 'mbway',
        theme: {
			backgroundColor: '#0000FF',
			formBackgroundColor: '#FFFFFF',
			primaryColor: color.hex,
			textColor: '#000000',
            effectsBackgroundColor: '#00FF00',
		},
    };
    const configs = btoa(JSON.stringify(configData));

    let baseUrl = !!import.meta.env.VITE_IS_PRODUCTION ? "https://checkout.paybyrd.com" : "https://checkoutsandbox.paybyrd.com";
    return `${baseUrl}/#/payment?checkoutKey=${data.CheckoutKey}&orderId=${data.OrderId}&configs=${configs}`
}