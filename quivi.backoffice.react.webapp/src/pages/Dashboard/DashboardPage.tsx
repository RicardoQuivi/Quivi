import { useTranslation } from "react-i18next"
import PageMeta from "../../components/common/PageMeta"
import { Onboarding } from "./Onboarding/Onboard";

export const DashboardPage = () => {
    const { t } = useTranslation();

    return <>
        <PageMeta
            title={t("pages.dashboard.title")}
            description={t("quivi.product.description")}
        />
        
        <div className="grid grid-cols-12 gap-4 md:gap-6">
            <div className="col-span-12">
                <Onboarding />
            </div>
      </div>
    </>
}