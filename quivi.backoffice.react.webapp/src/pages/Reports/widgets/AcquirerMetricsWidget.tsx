import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { AnalyticMetricWidget } from "./AnalyticMetricWidget";
import { usePartnerChargeMethodSalesQuery } from "../../../hooks/queries/implementations/usePartnerChargeMethodSalesQuery";
import { ChargeMethod } from "../../../hooks/api/Dtos/ChargeMethod";
import { ChargePartner } from "../../../hooks/api/Dtos/acquirerconfigurations/ChargePartner";

const getImprovement = (current: number, previous: number) => {
    if (previous === 0) {
        return 0;
    }
    return ((current - previous) / previous * 100);
}

interface Period {
    readonly from: Date;
    readonly to: Date;
}
interface State {
    readonly now: Date;
    readonly reference: Period;
    readonly analysis: Period;
}
interface Props {
    readonly adminView?: boolean;
    readonly partner: ChargePartner;
    readonly method: ChargeMethod;
}
export const AcquirerMetricsWidget = (props: Props) => {
    const { t } = useTranslation();
    
    const [stats, setStats] = useState<State>();

    const analysisSales = usePartnerChargeMethodSalesQuery(stats == undefined ? undefined : {
        adminView: props.adminView,
        chargeMethods: [props.method],
        chargePartners: [props.partner],
        from: stats.analysis.from.toISOString(),
        to: stats.analysis.to.toISOString(),
        page: 0,
        pageSize: 1,
    })
    const referenceSales = usePartnerChargeMethodSalesQuery(stats == undefined ? undefined : {
        adminView: props.adminView,
        chargeMethods: [props.method],
        chargePartners: [props.partner],
        from: stats.reference.from.toISOString(),
        to: stats.reference.to.toISOString(),
        page: 0,
        pageSize: 1,
    })

    const data = useMemo(() => {
        const currentMonth = analysisSales.data.length > 0 ? analysisSales.data[0] : {
            total: 0,
            totalSuccess: 0,
            totalFailed: 0,
            totalProcessing: 0,
        }
        const previousMonth = referenceSales.data.length > 0 ? referenceSales.data[0] : {
            total: 0,
            totalSuccess: 0,
            totalFailed: 0,
            totalProcessing: 0,
        }

        const currentMonthTotal = currentMonth.totalSuccess + currentMonth.totalFailed;
        const currentSuccessRate = currentMonthTotal == 0 ? 0 : currentMonth.totalSuccess / currentMonthTotal;

        const previousMonthTotal = previousMonth.totalSuccess + previousMonth.totalFailed;
        const previousSuccessRate = previousMonthTotal == 0 ? 0 : previousMonth.totalSuccess / previousMonthTotal;

        return {
            percentage: getImprovement(currentSuccessRate, previousSuccessRate),
            node: <span>
                { 
                    currentMonthTotal == 0
                    ?
                    t("widgets.acquirerAnalytics.noTransactions")
                    :
                    `${(currentSuccessRate * 100).toFixed(2)} %`
                }
            </span>
        }
    }, [analysisSales.data, referenceSales.data, stats])

    return <AnalyticMetricWidget
        key={getKey(props.partner, props.method)}
        title={t("widgets.acquirerAnalytics.successRate", {
            name: t("widgets.chargeSuccessRate.methodName", {
                method: t(`common.chargeMethod.${ChargeMethod[props.method]}`),
                partner: ChargePartner[props.partner],
            })
        })}
        percentage={data.percentage}
        onPeriodChange={(now, aFrom, aTo, rFrom, rTo) => setStats({
            now: now,
            analysis: {
                from: aFrom,
                to: aTo,
            },
            reference: {
                from: rFrom,
                to: rTo,
            }
        })}
        isLoading={analysisSales.isFirstLoading || referenceSales.isFirstLoading}
    >
        {data.node}
    </AnalyticMetricWidget>
};
const getKey = (partner: ChargePartner, method: ChargeMethod) => `${method}-${partner}`