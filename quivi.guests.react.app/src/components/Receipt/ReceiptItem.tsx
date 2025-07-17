import { Chip, Skeleton, Typography } from "@mui/material";
import { ItemsHelper } from "../../helpers/ItemsHelper";
import { useQuiviTheme } from "../../hooks/theme/useQuiviTheme";
import type { BaseReceiptLine, ReceiptLine } from "./ReceiptLine";
import { Formatter } from "../../helpers/formatter";
import { Calculations } from "../../helpers/calculations";
import { InfoIcon } from "../../icons";

interface ReceiptItemLineProps {
    readonly item: BaseReceiptLine | undefined;
    readonly isSubItem: boolean;
}
const ReceiptItemLine: React.FC<ReceiptItemLineProps> = ({ 
    item,
    isSubItem
}) => {
    const theme = useQuiviTheme();

    const isStroke = item != undefined && item.isStroke; 
    const itemFractionPrice = item == undefined ? undefined : (item.quantity ?? 1) * item.amount;
    const formattedQuantity = item == undefined ? undefined : Number((item.quantity ?? 1).toFixed(2)).toString().replace(".", ",");
    const unitPrice = item == undefined ? 1 : (item.discount ? ItemsHelper.originalUnitPrice(item.amount, item.discount) : item.amount);

    return (
        <div className={`table-item ${isStroke ? "is-paid" : ""}`}>
            <Typography variant="subtitle1" component="span" className="quantity">
                {
                    formattedQuantity != undefined
                    ?
                        !isSubItem && item?.quantity != undefined && formattedQuantity.toString()
                    :
                        <Skeleton variant="text" animation="wave" height="1.5rem" width="70%"/>
                }
            </Typography>
            <Typography variant="subtitle1" component="span">
                {
                    item != undefined 
                    ?
                    <>
                        {item.name}
                        {
                            item.info != undefined &&
                            <>
                                &nbsp;&nbsp;
                                <Chip label={<span>{item.info}</span>} variant="outlined" size="small" avatar={<>&nbsp;<InfoIcon color={theme.primaryColor.hex} /></>}/>
                            </>
                        }
                    </>
                    :
                        <Skeleton variant="text" animation="wave" height="1.5rem" width="70%"/>
                }
            </Typography >
            {
                item != undefined && (item.discount || (item.quantity != 1 && (item.quantity ?? 1) > 0 && unitPrice != itemFractionPrice))
                ?
                    <Typography variant="subtitle1" component="span" className="unit price">{Formatter.price(Calculations.roundUp(unitPrice), "€")}</Typography>
                :
                    <Typography variant="subtitle1" component="span" className="unit price" />
            }
            {
                itemFractionPrice == undefined
                ?
                    <Skeleton variant="text" animation="wave" height="1.5rem" width="70%"/>
                :
                (
                    itemFractionPrice != 0 &&
                    <Typography variant="subtitle1" component="span" className="fraction price">{Formatter.price(itemFractionPrice, "€")}</Typography>
                )
            }
        </div>
    );
}

interface ReceiptLineProps {
    readonly item: ReceiptLine | undefined;
}
const ReceiptItem: React.FC<ReceiptLineProps> = ({ item }) => {
    return <>
        <ReceiptItemLine item={item} isSubItem={false}/>
        {
            item != undefined &&
            item.subItems != undefined &&
            item.subItems.map((s, index) => {
                const result = [];
                for(let i = 0; i < (s.quantity ?? 1); ++i) {
                    const aux = {...s, quantity: item.quantity, isStroke: item.isStroke }
                    result.push(<ReceiptItemLine item={aux} isSubItem={true} key={`${index}-${i}`}/>);
                }
                return result;
            }).reduce((r, items) => [...r, ...items], [])
        }
    </>
};
export default ReceiptItem;