import { useEffect, useMemo } from "react";
import React from "react";
import type { Order } from "../../hooks/api/Dtos/orders/Order";
import { useBrowserStorageService } from "../../hooks/useBrowserStorageService";
import { useTranslation } from "react-i18next";
import { useTransactionsQuery } from "../../hooks/queries/implementations/useTransactionsQuery";
import type { ReceiptLine } from "../../components/Receipt/ReceiptLine";
import { OrderState } from "../../hooks/api/Dtos/orders/OrderState";
import Receipt from "../../components/Receipt/Receipt";
import { Alert, AlertTitle } from "@mui/material";
import { OrdersHelper } from "../../helpers/ordersHelper";

interface Props {
    readonly order: Order;
}
export const OrderAndPayCanceled: React.FC<Props> = ({ 
    order,
 }) => {
    const browserStorageService = useBrowserStorageService();
    const { t } = useTranslation();
    const transactionQuery = useTransactionsQuery({
        orderId: order.id,
        page: 0,
        pageSize: 1,
    })
    const transaction = useMemo(() => transactionQuery.data.length == 0 ? undefined : transactionQuery.data[0], [transactionQuery.data]);

    useEffect(() => {
        browserStorageService.savePaymentDivision(null);
        browserStorageService.savePaymentDetails(null);
    }, [])

    const mappedItems = useMemo<ReceiptLine[]>(() => order.items.map(item => ({
        id: item.id,
        discount: 0,
        isStroke: false,
        name: item.name,
        amount: item.amount,
        quantity: item.quantity,
    })), [order.items]);
    
    const mappedSubTotals = useMemo(() => order.extraCosts.map(item => ({
        amount: item.amount,
        name: t(`extraCost.${item.type}`),
    })), [order.extraCosts, t]);

    const orderTotal = useMemo(() => OrdersHelper.getTotal(order), [order]);

    const cancelationReason = useMemo(() => {
        if(!order.changes) {
            return "";
        }
        const rejected = order.changes.filter(l => l.state == OrderState.Rejected);
        if(rejected.length == 0) {
            return "";
        }
        return rejected[0].note ?? "";
    }, [order]);

    const refund = transaction?.refundData?.refund ?? 0;
    return <>
        <div style={{display: "flow-root"}}>
            <h2 className="mb-4" style={{float: "left"}}>{t("orderAndPayResult.yourOrder")}</h2>
            <h4 className="mb-4" style={{float: "right"}}>{order.id}</h4>
        </div>
        <Receipt
            items={mappedItems}
            subTotals={mappedSubTotals}
            total={{
                amount: orderTotal,
                name: t("cart.totalPrice"),
            }}
        />
        <div className="mb-8 mt-5">
            <Alert severity="error">
                <AlertTitle><strong>{t("orderAndPayResult.orderCanceled")}</strong></AlertTitle>
                {
                    refund == 0
                    ?
                        t("orderAndPayResult.orderCanceledDescription")
                    :
                        t("orderAndPayResult.orderCanceledAndRefundedDescription")
                }
                {
                    !!cancelationReason &&
                    <>
                        <br/>
                        <br/>
                        {t("orderAndPayResult.orderCanceledMessage")}
                        <br/>
                        <br/>
                        {cancelationReason}
                    </>
                }
            </Alert>
        </div>
    </>;
}