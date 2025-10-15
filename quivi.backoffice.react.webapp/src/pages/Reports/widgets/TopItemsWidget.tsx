import { useTranslation } from "react-i18next";
import { useNow } from "../../../hooks/useNow";
import { useMemo, useState } from "react";
import { useProductSalesQuery } from "../../../hooks/queries/implementations/useProductSalesQuery";
import { Collections } from "../../../utilities/Collectionts";
import { useMenuItemsQuery } from "../../../hooks/queries/implementations/useMenuItemsQuery";
import { Skeleton } from "../../../components/ui/skeleton/Skeleton";
import { TopTableWidget } from "./TopTableWidget";
import { ProductSalesSortBy } from "../../../hooks/api/Dtos/reporting/ProductSalesSortBy";
import CurrencySpan from "../../../components/currency/CurrencySpan";
import { ProductSales } from "../../../hooks/api/Dtos/reporting/ProductSales";
import { SalesPeriod } from "../../../hooks/api/Dtos/reporting/SalesPeriod";

const getRange = (now: Date, period?: SalesPeriod) => {
    if(period == undefined) {
        return {
            now,
            from: undefined,
            to: undefined,
        }
    }
    const startDate = new Date(now);
    const endDate = new Date(now);

    switch (period) {
        case SalesPeriod.Hourly:
            // Last 24 hours
            startDate.setUTCHours(now.getHours() - 24, 0, 0, 0);
            endDate.setUTCHours(now.getHours() + 1, 0, 0, 0);
            break;

        case SalesPeriod.Daily:
            // Last 31 days
            startDate.setUTCDate(now.getUTCDate() - 31);
            startDate.setUTCHours(0, 0, 0, 0);

            endDate.setUTCDate(now.getUTCDate() + 1);
            endDate.setUTCHours(0, 0, 0, 0);
            break;

        case SalesPeriod.Monthly:
            // Last 12 months
            startDate.setUTCMonth(now.getUTCMonth() - 11, 1);
            startDate.setUTCHours(0, 0, 0, 0);

            endDate.setMonth(now.getUTCMonth() + 1, 1);
            endDate.setUTCHours(0, 0, 0, 0);
            break;
    }

    return {
        now,
        from: startDate,
        to: endDate,
    };
};


interface Props {
    readonly by: ProductSalesSortBy;
}
export const TopItemsWidget = (props: Props) => {
    const now = useNow(1000 * 60);
    const { t } = useTranslation();

    const [period, setPeriod] = useState<SalesPeriod | undefined>(SalesPeriod.Daily)

    const stats = useMemo(() => {
        const now = new Date(); 
        return getRange(now, period);
    }, [now, period]);

    const productSalesQuery = useProductSalesQuery({
        sortBy: props.by,
        from: stats.from?.toISOString(),
        to: stats.to?.toISOString(),
        page: 0,
        pageSize: 10,
    })

    const itemIds = useMemo(() => Collections.uniqueIds(productSalesQuery.data, d => d.menuItemId), [productSalesQuery.data])
    const menuItemsQuery = useMenuItemsQuery(itemIds.length == 0 ? undefined : {
        ids: itemIds,
        page: 0,
    })
    const itemsMap = useMemo(() => Collections.toMap(menuItemsQuery.data, d => d.id), [menuItemsQuery.data])

    const widgetProps = useMemo(() => {
        if(props.by == ProductSalesSortBy.SoldQuantity) {
            return {
                title: t("widgets.topMenuItems.topQuantity"),
                nameLabel: t("common.entities.menuItem"),
                valueLabel: t("widgets.topMenuItems.quantityLabel"),
                getValue: (d: ProductSales) => d.totalSoldQuantity.toFixed(0),
            }
        }

        return {
            title: t("widgets.topMenuItems.topQuantity"),
            nameLabel: t("common.entities.menuItem"),
            valueLabel: t("widgets.topMenuItems.billedLabel"),
            getValue: (d: ProductSales) => <CurrencySpan value={d.totalBilledAmount} />,
        }
    }, [t, props.by])

    return (
        <TopTableWidget
            isLoading={productSalesQuery.isFirstLoading}
            title={widgetProps.title}
            nameLabel={widgetProps.nameLabel}
            valueLabel={widgetProps.valueLabel}
            data={productSalesQuery.data}
            getKey={d => `${d.menuItemId}-${d.from}-${d.to}`}
            getName={d => {
                const menuItem = itemsMap.get(d.menuItemId);
                if(menuItem == undefined) {
                    return <Skeleton />
                }
                return menuItem.name;
            }}
            period={period}
            onPeriodChange={setPeriod}
            getValue={widgetProps.getValue}
        />
    );
}