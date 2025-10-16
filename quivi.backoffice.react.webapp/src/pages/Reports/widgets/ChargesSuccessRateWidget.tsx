import { useTranslation } from "react-i18next";
import { ChargePartner } from "../../../hooks/api/Dtos/acquirerconfigurations/ChargePartner"
import { ChargeMethod } from "../../../hooks/api/Dtos/ChargeMethod";
import { useNow } from "../../../hooks/useNow";
import { useMemo, useState } from "react";
import { SalesPeriod } from "../../../hooks/api/Dtos/reporting/SalesPeriod";
import { usePartnerChargeMethodSalesQuery } from "../../../hooks/queries/implementations/usePartnerChargeMethodSalesQuery";
import { ComparisonChartWidget } from "./ComparisonChartWidget";

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

interface ChargesSuccessRateWidgetProps {
    readonly partner: ChargePartner;
    readonly method: ChargeMethod;
    readonly adminView?: boolean;
}
export const ChargesSuccessRateWidget = (props: ChargesSuccessRateWidgetProps) => {
    const now = useNow(1000 * 60);
    const { t } = useTranslation();

    const [period, setPeriod] = useState<SalesPeriod | undefined>(SalesPeriod.Daily)

    const stats = useMemo(() => {
        const now = new Date(); 
        return getRange(now, period);
    }, [now, period]);

    const partnerChargeSalesQuery = usePartnerChargeMethodSalesQuery({
        adminView: props.adminView,
        chargeMethods: [props.method],
        chargePartners: [props.partner],
        from: stats.from?.toISOString(),
        to: stats.to?.toISOString(),
        page: 0,
    })
    
    const data = useMemo(() => {
        if(partnerChargeSalesQuery.data.length == 0) {
            return [];
        }

        const data = partnerChargeSalesQuery.data[0];
        return [
            {
                name: t("widgets.chargeSuccessRate.success"),
                value: data.totalSuccess,
            },
            {
                name: t("widgets.chargeSuccessRate.failed"),
                value: data.totalFailed,
            },
            {
                name: t("widgets.chargeSuccessRate.inProgress"),
                value: data.totalProcessing,
            },
        ]
    }, [partnerChargeSalesQuery.data, t])

    return <ComparisonChartWidget
        period={period}
        onPeriodChange={setPeriod}
        title={t("widgets.chargeSuccessRate.title", {
            name: t("widgets.chargeSuccessRate.methodName", {
                method: t(`common.chargeMethod.${ChargeMethod[props.method]}`),
                partner: ChargePartner[props.partner],
            })
        })}
        data={data}
        getName={d => d.name}
        getValue={d => d.value}
        isLoading={partnerChargeSalesQuery.isFirstLoading}
    />
}