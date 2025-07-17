import { useEffect } from "react";
import { useTranslation } from "react-i18next";
import { Formatter } from "../../helpers/formatter";

interface Props {
    readonly loadUserBill: (userBill: number) => void;
    readonly sessionPending: number | null;
}

export const PayTotal: React.FC<Props> = ({ 
    loadUserBill,
    sessionPending,
}) => {
    const { t } = useTranslation();
    
    useEffect(() => {
        if(sessionPending == null) {
            return;
        }

        loadUserBill(sessionPending);
    }, [sessionPending]);

    return (
        <div className={"total-container"}>
            <h2 className="mb-1">{t("pay.payTotal")}</h2>
            <h1>{Formatter.price(sessionPending ?? 0, "€")}</h1>
        </div>
    )
}