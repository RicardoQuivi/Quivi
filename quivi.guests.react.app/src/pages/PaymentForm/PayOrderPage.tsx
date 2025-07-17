import { useTranslation } from "react-i18next";
import { useNavigate, useParams } from "react-router";
import { toast } from "react-toastify";
import { ErrorIcon } from "../../icons";
import { PaymentPage } from "./PaymentPage";
import { useChannelContext } from "../../context/AppContextProvider";

export const PayOrderPage = () => {
    const { orderId } = useParams<{orderId: string}>();
    const channelContext = useChannelContext();

    const { t } = useTranslation();
    const navigate = useNavigate();
    
    const onFail = () => {
        navigate(`/c/${channelContext.channelId}/session/pay/methods`, {
            replace: true,
        })
        toast.info(t("paymentMethods.paymentFailed"), {
            icon: <ErrorIcon />,
        });
    }
    const onSuccess = () => navigate(`/c/${channelContext.channelId}/orders/${orderId}/track`);

    return <PaymentPage
        onFail={onFail}
        onSuccess={onSuccess}
    />
}