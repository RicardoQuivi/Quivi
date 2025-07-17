import { useTranslation } from "react-i18next";
import type { ReceiptLine } from "./ReceiptLine";
import { ItemsHelper } from "../../helpers/ItemsHelper";
import { Formatter } from "../../helpers/formatter";
import { Calculations } from "../../helpers/calculations";

interface Props {
    readonly item: ReceiptLine;
}
const ReceiptFooter: React.FC<Props> = ({ item }) => {
    const { t } = useTranslation();

    const quantity = item.quantity ?? 1;
    const itemFractionPrice = quantity * item.amount;
    const formattedQuantity = Number(quantity.toFixed(2)).toString().replace(".", ",");
    const formattedDiscount = item.discount.toFixed(2);
    const unitPriceLabel = item.discount ? t("pay.appliedDiscount", { formattedDiscount }) : t('pay.unitPrice');
    const unitPrice = item.discount ? ItemsHelper.originalUnitPrice(item.amount, item.discount) : item.amount;
    return (
        <div className={`table-item ${item.isStroke ? "is-paid" : ""}`}>
            <p className="quantity">{formattedQuantity.toString()}</p>
            <p>{item.name}</p>
            {
                item.discount || (item.quantity !== 1 && quantity > 0)
                ?
                <p className="unit price" title={unitPriceLabel}>{Formatter.price(Calculations.roundUp(unitPrice), "€")}</p>
                :
                <p className="unit price"/>
            }
            <p className="fraction price">
                {Formatter.price(itemFractionPrice, "€")}
            </p>
        </div>
    );
}
export default ReceiptFooter;