import React from "react"
import ReceiptItem from "./ReceiptItem"
import type { BaseReceiptLine, ReceiptLine } from "./ReceiptLine";
import type { ReceiptTotalLine } from "./ReceiptTotalLine";
import type { ReceiptSubTotalLine } from "./ReceiptSubTotalLine";
import { Formatter } from "../../helpers/formatter";
import { Skeleton } from "@mui/material";

interface Props {
    readonly header?: string;
    readonly items: ReceiptLine[] | undefined,
    readonly subTotals: ReceiptSubTotalLine[] | undefined,
    readonly total: ReceiptTotalLine,
    readonly style?: React.DetailedHTMLProps<React.StyleHTMLAttributes<HTMLStyleElement>, HTMLStyleElement>,
    readonly className?: string;
    readonly children?: React.ReactNode;
}

const Receipt: React.FC<Props> = ({
    header,
    items,
    subTotals,
    total,
    children,
    style,
    className
}) => {
    const getItemKey = (item: ReceiptLine) => {
        const nameGenerator = (line: BaseReceiptLine) => `${line.name}-${line.amount}-${line.discount}-${line.isStroke}-${line.quantity}`;
        return (item.subItems ?? []).reduce((r, l) => `${r}|${nameGenerator(l)}`, nameGenerator(item));
    }

    return (
        <section className={`summary ${className || ""}`} style={style}>
            <div>
                { header != undefined && <h2 className="mb-4">{header}</h2>}
                { 
                    items != undefined
                    ?
                        items.map((item, index) => <ReceiptItem item={item} key={`${getItemKey(item)}_${index}`}/>) 
                    :
                        [1, 2, 3].map(i => <ReceiptItem item={undefined} key={i}/>)
                }
                <hr />
                <div className="table-totals">
                    {
                        subTotals != undefined && 
                        subTotals.map((item, index) => 
                            <div className="table-totals--subtotal" key={`${item.name}_${index}`}>
                                <p className="title">{item.name}</p>
                                <p className="amount">{Formatter.price(item.amount, "€")}</p>
                            </div>
                        )
                    }
                    <div className="table-totals--total">
                        <h2 className="title">{total.name}</h2>
                        <h2 className="amount">
                            {
                                total.amount != undefined
                                ?
                                    Formatter.price(total.amount, "€")
                                :
                                <Skeleton variant="text" animation="wave" height="1.5rem" width="40px" style={{marginRight: "10px"}} />
                            }
                        </h2>
                    </div>
                </div>
            </div>
            {!!children && children}
        </section>
    )
}
export default Receipt;