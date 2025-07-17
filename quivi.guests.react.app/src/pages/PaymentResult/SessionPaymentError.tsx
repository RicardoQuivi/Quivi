import { useTranslation } from "react-i18next";
import { ErrorIcon } from "../../icons";
import { Link } from "react-router";
import { useChannelContext } from "../../context/AppContextProvider";

export const SessionPaymentError = (props: { message: string }) => {
    const { t } = useTranslation();
    const channelContext = useChannelContext();

    return <>
        <div className="result__image">
            <ErrorIcon />
        </div>
        <h2 className="mb-3 ta-c">{t("paymentResult.errorTitle")}</h2>
        <p className="ta-c">
            {props.message}
        </p>
        {
            <Link to={`/c/${channelContext.channelId}`} className="secondary-button smaller mt-10">
                {t("paymentResult.home")}
            </Link>
        }
    </>
}