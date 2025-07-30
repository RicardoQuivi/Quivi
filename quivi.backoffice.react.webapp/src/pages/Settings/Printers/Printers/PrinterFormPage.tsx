import { useTranslation } from "react-i18next";
import { useLocation, useNavigate, useParams } from "react-router";
import { useMemo } from "react";
import PageMeta from "../../../../components/common/PageMeta";
import PageBreadcrumb from "../../../../components/common/PageBreadCrumb";
import ComponentCard from "../../../../components/common/ComponentCard";
import { PrinterForm, PrinterFormState } from "./PrinterForm";
import { useToast } from "../../../../layout/ToastProvider";
import { usePrinterMutator } from "../../../../hooks/mutators/usePrinterMutator";
import { usePrintersQuery } from "../../../../hooks/queries/implementations/usePrintersQuery";

export const PrinterFormPage = () => {
    const { id } = useParams();
    const { t } = useTranslation();
    const navigate = useNavigate();
    const toast = useToast();
    const mutator = usePrinterMutator();
    const { search } = useLocation();
    const params = new URLSearchParams(search);

    const title = t(`common.operations.${id == undefined ? "new" : "edit"}`, {
        name: t("common.entities.printer")
    });

    const itemsQuery = usePrintersQuery(id == undefined ? undefined : {
        ids: [ id ],
        page: 0,
    })

    const item = useMemo(() => {
        if(id == undefined) {
            return undefined;
        }
        if(itemsQuery.data.length == 0) {
            return undefined;
        }
        return itemsQuery.data[0];
    }, [id, itemsQuery.data])

    const submit = async (state: PrinterFormState) => {
        if(item == undefined) {
            await mutator.create({
                name: state.name,
                address: state.address,
                printerWorkerId: state.printerWorkerId,
                locationId: state.locationId,
                notifications: state.notifications,
            })
            toast.success(t("common.operations.success.new"));
        } else {
            await mutator.patch(item, {
                name: state.name,
                address: state.address,
                printerWorkerId: state.printerWorkerId,
                locationId: state.locationId,
                notifications: state.notifications,
            })
            toast.success(t("common.operations.success.edit"));
        }
        navigate("/settings/printersmanagement")
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
                    to: "/settings/printersmanagement",
                }
            ]}
            breadcrumb={title}
        />

        <ComponentCard title={title}>
            <PrinterForm
                model={item}
                onSubmit={submit}
                submitText={t(`common.operations.save`, {
                    name: t("common.entities.printer")
                })}
                printerWorkerId={params.get("printerWorkerId") ?? undefined}
            />
        </ComponentCard>
    </>
}