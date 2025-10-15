import { useTranslation } from "react-i18next";
import { ProductSalesSortBy } from "../../../hooks/api/Dtos/reporting/ProductSalesSortBy";
import { useNow } from "../../../hooks/useNow";
import { ComparisonChartWidget } from "./ComparisonChartWidget"
import { SalesPeriod } from "../../../hooks/api/Dtos/reporting/SalesPeriod";
import { useMemo, useState } from "react";
import { useChargeMethodSalesQuery } from "../../../hooks/queries/implementations/useChargeMethodSalesQuery";
import { useCustomChargeMethodsQuery } from "../../../hooks/queries/implementations/useCustomChargeMethodsQuery";
import { Collections } from "../../../utilities/Collectionts";
import { CurrencyHelper } from "../../../utilities/currencyHelper";

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
export const ChargeMethodsComparisonChartWidget = (props: Props) => {
    const now = useNow(1000 * 60);
    const { t, i18n } = useTranslation();

    const [period, setPeriod] = useState<SalesPeriod | undefined>(SalesPeriod.Daily)

    const stats = useMemo(() => {
        const now = new Date(); 
        return getRange(now, period);
    }, [now, period]);

    const chargeSalesQuery = useChargeMethodSalesQuery({
        sortBy: props.by,
        from: stats.from?.toISOString(),
        to: stats.to?.toISOString(),
        page: 0,
    })

    const customChargeMethodIds = useMemo(() => Collections.uniqueIds(chargeSalesQuery.data.filter(d => d.customChargeMethodId != undefined), d => d.customChargeMethodId!), [chargeSalesQuery.data])
    const customChargeMethodsQuery = useCustomChargeMethodsQuery(customChargeMethodIds.length == 0 ? undefined : {
        ids: customChargeMethodIds,
        page: 0,
    })
    const customChargeMethodsMap = useMemo(() => Collections.toMap(customChargeMethodsQuery.data, d => d.id), [customChargeMethodsQuery.data])
    
    return <ComparisonChartWidget
        period={period}
        onPeriodChange={setPeriod}
        title={props.by == ProductSalesSortBy.SoldQuantity ? t("widgets.compareChargeMethods.perQuantityTitle") : t("widgets.compareChargeMethods.perBillingTitle")}
        data={chargeSalesQuery.data}
        getName={d => {
            if(d.customChargeMethodId == undefined) {
                return "Quivi"
            }

            const customChargeMethod = customChargeMethodsMap.get(d.customChargeMethodId);
            if(customChargeMethod == undefined) {
                return t("common.loading");
            }

            return customChargeMethod.name;
        }}
        getValue={props.by == ProductSalesSortBy.SoldQuantity ? t => t.totalInvoices : t => t.totalBilledAmount}
        formatter={props.by == ProductSalesSortBy.SoldQuantity ? t => t.toFixed(0) : t => CurrencyHelper.format({
            value: t,
            culture: i18n.language,
        })}
    />
}