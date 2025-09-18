import React, { useEffect, useState } from "react"
import { useTranslation } from "react-i18next";
import { Transaction } from "../../hooks/api/Dtos/transactions/Transaction";
import { PaginationFooter } from "../Pagination/PaginationFooter";
import CurrencySpan from "../Currency/CurrencySpan";
import { ResponsiveTable } from "../Tables/ResponsiveTable";
import DecimalSpan from "../Currency/DecimalSpan";
import { BaseTransactionItem } from "../../hooks/api/Dtos/transactionItems/TransactionItem";
import { useTransactionItemsQuery } from "../../hooks/queries/implementations/useTransactionItemsQuery";

interface Props {
    readonly transaction: Transaction;
    readonly canLoadItems: boolean;
}

export const TransactionItemsPage: React.FC<Props> = (props) => {
    const { t } = useTranslation();

    const [loadItems, setLoadItems] = useState(props.canLoadItems);
    const [page, setPage] = useState(0);
    const [expandedItems, setExpandedItems] = useState<string[]>([]);

    const itemsQuery = useTransactionItemsQuery(loadItems == false ? undefined : {
        transactionId: props.transaction.id,
        page: page,
        pageSize: 10,
    });

    const getModifiersTotal = (itemQty: number, modifiers: BaseTransactionItem[]) => modifiers.reduce((r, m) => r + itemQty * m.quantity * m.price, 0) ?? 0;

    const formatQuantity = (value: number) => value.toFixed(2).replace('.00', '');

    const isExpanded = (id: string) => expandedItems.some(x => x == id);

    const toggleExpandedItem = (id: string) => setExpandedItems(expandedList => {
        if (expandedList.some(x => x == id)) {
            return expandedList.filter(x => x != id);
        }
        return [...expandedList, id];
    });

    useEffect(() => setLoadItems(p => p || props.canLoadItems), [props.canLoadItems])
    
    return <>
        <ResponsiveTable
            isLoading={itemsQuery.isFirstLoading}
            data={itemsQuery.data.map(item => ({
                item: item,
                isExpandable: item.modifiers.length > 0,
                isExpanded: isExpanded(item.id),
                isChildren: false,
            }))}
            columns={[
                {
                    key: "name",
                    label: t("name"),
                    render: d => <>
                        {
                            (d.isChildren == false || d.item.quantity > 1) &&
                            <>
                                <span className="badge badge-secondary">
                                    {formatQuantity(d.item.quantity)} x
                                </span>
                                &nbsp;
                            </>
                        }
                        {d.item.name} {d.isExpandable && <strong>{d.isExpanded ? " ( - )" : " ( + )"}</strong>}
                    </>
                },
                {
                    key: "unit-price",
                    label: t("paymentHistory.unitPrice"),
                    render: d => <CurrencySpan value={d.item.originalPrice + (!d.isExpanded ? getModifiersTotal(1, d.item.modifiers) : 0)} />
                },
                {
                    key: "discount",
                    label: t("discount"),
                    render: d => <>
                        {
                            d.item.appliedDiscountPercentage > 0 &&
                            <>
                                <span className="badge badge-pill badge-success">
                                    <DecimalSpan value={d.item.appliedDiscountPercentage} /> %
                                </span>
                                &nbsp;
                            </>
                        }
                        <CurrencySpan value={d.item.originalPrice - d.item.price} />
                    </>
                },
                {
                    key: "total",
                    label: t("total"),
                    render: d => <CurrencySpan value={d.item.quantity * d.item.price + (!d.isExpanded ? getModifiersTotal(d.item.quantity, d.item.modifiers) : 0)} />
                }
            ]}
            getKey={d => d.item.id}
            onRowClick={d => d.isExpandable && toggleExpandedItem(d.item.id)}
            getChildren={d => (d.item.modifiers ?? []).map(m => ({
                item: {
                    ...m,
                    modifiers: [],
                },
                isChildren: true,
                isExpandable: false,
                isExpanded: false,
            }))}
        />
        <PaginationFooter currentPage={itemsQuery.page} numberOfPages={itemsQuery.totalPages} onPageChanged={setPage} />
    </>
}