import React, { useEffect, useMemo, useState } from "react"
import { useTranslation } from "react-i18next";
import { Transaction } from "../../hooks/api/Dtos/transactions/Transaction";
import { PaginationFooter } from "../Pagination/PaginationFooter";
import CurrencySpan from "../Currency/CurrencySpan";
import { ResponsiveTable } from "../Tables/ResponsiveTable";
import DecimalSpan from "../Currency/DecimalSpan";
import { BaseTransactionItem } from "../../hooks/api/Dtos/transactionItems/TransactionItem";
import { useTransactionItemsQuery } from "../../hooks/queries/implementations/useTransactionItemsQuery";
import { Button, Stack } from "@mui/material";

interface Props {
    readonly transaction: Transaction;
    readonly canLoadItems: boolean;
}

export const TransactionItemsPage: React.FC<Props> = (props) => {
    const { t } = useTranslation();

    const [loadItems, setLoadItems] = useState(props.canLoadItems);
    const [page, setPage] = useState(0);
    const [expandedItems, setExpandedItems] = useState<Set<string>>(() => new Set<string>());

    const itemsQuery = useTransactionItemsQuery(loadItems == false ? undefined : {
        transactionId: props.transaction.id,
        page: page,
        pageSize: 10,
    });

    const toggleExpandedItem = (id: string) => setExpandedItems(expandedList => {
        const result = new Set<string>(expandedList);
        if (result.has(id)) {
            result.delete(id);
            return result;
        }
        result.add(id);
        return result;
    });

    useEffect(() => setLoadItems(p => p || props.canLoadItems), [props.canLoadItems])
    
    const mappedItems = useMemo(() => itemsQuery.data.map(item => ({
        item: item,
        isExpandable: item.modifiers.length > 0,
        isChildren: false,
    })), [itemsQuery.data])

    return <>
        <ResponsiveTable
            isLoading={itemsQuery.isFirstLoading}
            data={mappedItems}
            columns={[
                {
                    key: "name",
                    label: t("name"),
                    render: d => <Stack
                        direction="row"
                        gap={2}
                        alignItems="center"
                    >
                        {
                            (d.isChildren == false || d.item.quantity != 1) &&
                            <span className="badge badge-secondary">
                                {formatQuantity(d.item.quantity)} x
                            </span>
                        }
                        <span>
                            {d.item.name} {d.isExpandable && <strong>{expandedItems.has(d.item.id) ? " ( - )" : " ( + )"}</strong>}
                        </span>
                    </Stack>
                },
                {
                    key: "unit-price",
                    label: t("paymentHistory.unitPrice"),
                    render: d => <CurrencySpan value={d.item.originalPrice + (!expandedItems.has(d.item.id) ? getModifiersTotal(1, d.item.modifiers) : 0)} />
                },
                {
                    key: "discount",
                    label: t("discount"),
                    render: d => <Stack
                        direction="row"
                        gap={2}
                        alignItems="center"
                    >
                        <CurrencySpan value={d.item.originalPrice - d.item.price} />
                        {
                            d.item.appliedDiscountPercentage > 0 &&
                            <Button
                                variant="contained"
                                color="info"
                                sx={{
                                    paddingY: "3px",
                                    paddingX: "8px",
                                    minWidth: "unset",
                                    height: "unset",
                                }}
                            >
                                <DecimalSpan value={d.item.appliedDiscountPercentage} /> %
                            </Button>
                        }
                    </Stack>
                },
                {
                    key: "total",
                    label: t("total"),
                    render: d => <CurrencySpan value={d.item.quantity * d.item.price + (!expandedItems.has(d.item.id) ? getModifiersTotal(d.item.quantity, d.item.modifiers) : 0)} />
                }
            ]}
            getKey={d => d.item.id}
            onRowClick={d => d.isExpandable && toggleExpandedItem(d.item.id)}
            getChildren={d => d.item.modifiers.map(m => ({
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

const getModifiersTotal = (itemQty: number, modifiers: BaseTransactionItem[]) => modifiers.reduce((r, m) => r + itemQty * m.quantity * m.price, 0) ?? 0;
const formatQuantity = (value: number) => value.toFixed(2).replace('.00', '');