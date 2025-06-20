import { useTranslation } from "react-i18next";
import { useNavigate, useParams } from "react-router";
import PageMeta from "../../../components/common/PageMeta";
import PageBreadcrumb from "../../../components/common/PageBreadCrumb";
import { PrinterWorkersCard } from "./PrinterWorkers/PrinterWorkersCard";

export const PrinterPage = () => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const { workerId } = useParams<string>();
   
    const changeWorker = (c: string | undefined) => {
        if(c == undefined) {
            navigate(`/settings/printers`);
            return;
        }
        navigate(`/settings/printers/worker/${c}/`)
    }

    return <>
        <PageMeta
            title={t("pages.printers.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb
            pageTitle={t("pages.printers.title")}
            breadcrumb={t("pages.printers.title")}
        />

        <div className="grid grid-cols-12 gap-4">
            <div className="col-span-12 lg:col-span-4 h-full">
                <PrinterWorkersCard
                    printerWorkerId={workerId}
                    onPrinterWorkerChanged={changeWorker}
                />
            </div>

            <div className="col-span-12 lg:col-span-8 h-full">
                {/* <MenuItemsCard
                    categoryId={selectedCategory}
                /> */}
            </div>
        </div>
    </>
}