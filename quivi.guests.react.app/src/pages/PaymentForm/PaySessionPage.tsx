import { useTranslation } from "react-i18next";
import { toast } from "react-toastify";
import { PaymentPage } from "./PaymentPage";
import { ErrorIcon } from "../../icons";
import { useNavigate } from "react-router";
import type { Transaction } from "../../hooks/api/Dtos/transactions/Transaction";
import { useChannelContext } from "../../context/AppContextProvider";

export const PaySessionPage : React.FC = () => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const channelContext = useChannelContext();

    const onFail = () => {
        navigate(`/c/${channelContext.channelId}/session/pay/methods`, {
            replace: true,
        })
        toast.info(t("paymentMethods.paymentFailed"), {
            icon: <ErrorIcon />,
        });
    }

    const onSuccess = async (c: Transaction) => navigate(`/c/${channelContext.channelId}/session/pay/${c.id}/complete`);

    return <PaymentPage onFail={onFail} onSuccess={onSuccess} />
}