import { t } from "i18next";
import { useEffect, useState } from "react";
import { useCart } from "../../context/OrderingContextProvider";
import { useChannelContext } from "../../context/AppContextProvider";
import { Formatter } from "../../helpers/formatter";
import { useNavigate } from "react-router";

interface Props {
    readonly loadUserBill: (userBill: number) => void; 
}
export const PayTotalCart: React.FC<Props> = ({ 
    loadUserBill,
}) => {
    const cartState = useCart();
    const channelContext = useChannelContext();

    const [totalPending] = useState(cartState.total);
    const navigate = useNavigate();

    useEffect(() => {
        if(totalPending == 0) {
            navigate(`/c/${channelContext.channelId}`)
            return;
        }
        
        loadUserBill(totalPending);
    }, []);

    return <div>
        <div className={"total-container"}>
            <h2 className="mb-1">{t("pay.payTotal")}</h2>
            <h1>{Formatter.price(totalPending, "€")}</h1>
        </div>
    </div>
}