import { useTranslation } from "react-i18next";
import ComponentCard from "../../../../components/common/ComponentCard"
import { useNavigate, useParams } from "react-router";
import PageMeta from "../../../../components/common/PageMeta";
import PageBreadcrumb from "../../../../components/common/PageBreadCrumb";
import { useMemo } from "react";
import { PrinterWorkerForm, PrinterWorkerFormState } from "./PrinterWorkerForm";
import { useToast } from "../../../../layout/ToastProvider";
import { usePrinterWorkerMutator } from "../../../../hooks/mutators/usePrinterWorkerMutator";
import { usePrinterWorkersQuery } from "../../../../hooks/queries/implementations/usePrinterWorkersQuery";

export const PrinterWorkerFormPage = () => {
    const { id } = useParams();
    const { t } = useTranslation();
    const navigate = useNavigate();
    const toast = useToast();
    const mutator = usePrinterWorkerMutator();

    const title = t(`common.operations.${id == undefined ? 'new' : 'edit'}`, {
        name: t("common.entities.printerWorker")
    });

    const workersQuery = usePrinterWorkersQuery(id == undefined ? undefined : {
        ids: [ id ],
        page: 0,
    })

    const worker = useMemo(() => {
        if(id == undefined) {
            return undefined;
        }
        if(workersQuery.data.length == 0) {
            return undefined;
        }
        return workersQuery.data[0];
    }, [id, workersQuery.data])

    const submit = async (state: PrinterWorkerFormState) => {
        if(worker == undefined) {
            await mutator.create({
                identifier: state.identifier,
                name: state.name,
            })
            toast.success(t("common.operations.success.new"));
        } else {
            await mutator.patch(worker, {
                name: state.name,
            })
            toast.success(t("common.operations.success.edit"));
        }
        navigate("/settings/printers")
    }

    return <>
        <PageMeta
            title={t("pages.printers.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb
            pageTitle={t("pages.printers.title")}
            breadcrumbs={[
                {
                    title: t("pages.printers.title"),
                    to: "/settings/printers",
                }
            ]}
            breadcrumb={title}
        />

        <ComponentCard title={title}>
            <PrinterWorkerForm
                model={worker}
                onSubmit={submit}
                submitText={title}
            />
        </ComponentCard>
    </>
}