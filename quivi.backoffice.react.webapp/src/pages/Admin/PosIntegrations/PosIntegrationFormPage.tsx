import { useTranslation } from "react-i18next";
import { useNavigate, useParams } from "react-router";
import { useToast } from "../../../layout/ToastProvider";
import { useMemo } from "react";
import PageMeta from "../../../components/common/PageMeta";
import PageBreadcrumb from "../../../components/common/PageBreadCrumb";
import ComponentCard from "../../../components/common/ComponentCard";
import { usePosIntegrationsQuery } from "../../../hooks/queries/implementations/usePosIntegrationsQuery";
import { PosIntegrationForm, PosIntegrationFormState, QuiviViaFacturaLusaState } from "./PosIntegrationForm";
import { IntegrationType } from "../../../hooks/api/Dtos/posIntegrations/PosIntegration";
import { usePosIntegrationMutator } from "../../../hooks/mutators/usePosIntegrationMutator";

export const PosIntegrationFormPage = () => {
    const { id } = useParams();
    const { t } = useTranslation();
    const navigate = useNavigate();
    const toast = useToast();
    const mutator = usePosIntegrationMutator();

    const title = t(`common.operations.${id == undefined ? "new" : "edit"}`, {
        name: t("common.entities.posIntegration")
    });

    const integrationQuery = usePosIntegrationsQuery(id == undefined ? undefined : {
        ids: [id],
        page: 0,
    })

    const integration = useMemo(() => {
        if(id == undefined) {
            return undefined;
        }
        if(integrationQuery.data.length == 0) {
            return undefined;
        }
        return integrationQuery.data[0];
    }, [id, integrationQuery.data])

    const submit = async (state: PosIntegrationFormState) => {
        if(integration == undefined) {
            switch(state.type)
            {
                case IntegrationType.QuiviViaFacturalusa:
                {
                    const form = state.states[IntegrationType.QuiviViaFacturalusa] as QuiviViaFacturaLusaState;
                    await mutator.createQuivi({
                        accessToken: form.accessToken,
                        includeTipInInvoice: form.includeTipInInvoice,
                        skipInvoice: form.skipInvoice,
                        invoicePrefix: form.invoicePrefix,
                    })
                    break;
                }
            }
        } else {
            switch(state.type)
            {
                case IntegrationType.QuiviViaFacturalusa:
                {
                    const form = state.states[IntegrationType.QuiviViaFacturalusa] as QuiviViaFacturaLusaState;
                    await mutator.updateQuivi(integration, {
                        accessToken: form.accessToken,
                        includeTipInInvoice: form.includeTipInInvoice,
                        skipInvoice: form.skipInvoice,
                        invoicePrefix: form.invoicePrefix,
                    })
                    break;
                }
            }
        }

        if(integration == undefined) {
            toast.success(t("common.operations.success.new"));
        } else {
            toast.success(t("common.operations.success.edit"));
        }
        navigate("/admin/integrations")
    }
    
    return <>
        <PageMeta
            title={t("pages.posIntegrations.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb
            pageTitle={t("pages.posIntegrations.title")}
            breadcrumbs={[
                {
                    title: t("pages.posIntegrations.title"),
                    to: "/admin/integrations",
                }
            ]}
            breadcrumb={title}
        />

        <ComponentCard title={title}>
            <PosIntegrationForm
                model={integration}
                isLoading={id != undefined && integration == undefined}
                onSubmit={submit}
                submitText={t(`common.operations.save`, {
                    name: t("common.entities.posIntegration")
                })}
            />
        </ComponentCard>
    </>
}