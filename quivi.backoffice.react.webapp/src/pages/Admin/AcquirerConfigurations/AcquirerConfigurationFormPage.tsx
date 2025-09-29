import { useTranslation } from "react-i18next";
import { useNavigate, useParams } from "react-router";
import { useToast } from "../../../layout/ToastProvider";
import { useMemo } from "react";
import PageMeta from "../../../components/common/PageMeta";
import PageBreadcrumb from "../../../components/common/PageBreadCrumb";
import ComponentCard from "../../../components/common/ComponentCard";
import { useAcquirerConfigurationMutator } from "../../../hooks/mutators/useAcquirerConfigurationMutator";
import { useAcquirerConfigurationsQuery } from "../../../hooks/queries/implementations/useAcquirerConfigurationsQuery";
import { AcquirerConfigurationForm, AcquirerConfigurationFormState } from "./AcquirerConfigurationForm";
import { ChargePartner } from "../../../hooks/api/Dtos/acquirerconfigurations/ChargePartner";
import { ChargeMethod } from "../../../hooks/api/Dtos/ChargeMethod";

export const AcquirerConfigurationFormPage = () => {
    const { id } = useParams();
    const { t } = useTranslation();
    const navigate = useNavigate();
    const toast = useToast();
    const mutator = useAcquirerConfigurationMutator();

    const title = t(`common.operations.${id == undefined ? "new" : "edit"}`, {
        name: t("common.entities.acquirerConfiguration")
    });

    const acquirersQuery = useAcquirerConfigurationsQuery(id == undefined ? undefined : {
        ids: [ id ],
        page: 0,
    })

    const acquirer = useMemo(() => {
        if(id == undefined) {
            return undefined;
        }
        if(acquirersQuery.data.length == 0) {
            return undefined;
        }
        return acquirersQuery.data[0];
    }, [id, acquirersQuery.data])

    const submit = async (state: AcquirerConfigurationFormState) => {
        if(state.partner == ChargePartner.Quivi) {
            if(state.method == ChargeMethod.Cash) {
                await mutator.upsertCash({
                    isActive: state.isActive,
                })
            }
        } else if(state.partner == ChargePartner.Paybyrd) {
            const data = state.states[ChargePartner.Paybyrd][state.method];
            switch(state.method)
            {
                case ChargeMethod.CreditCard:
                case ChargeMethod.MbWay:
                    await mutator.upsertPaybyrd({
                        method: state.method,
                        apiKey: data.apiKey,
                        isActive: state.isActive,
                    });
                    break;
                case ChargeMethod.PaymentTerminal:
                    if('terminalId' in data == false ) {
                        throw new Error('This should never happen.');
                    }

                    await mutator.upsertPaybyrdTerminal({
                        apiKey: data.apiKey,
                        terminalId: data.terminalId,
                        isActive: state.isActive,
                    });
                    break;
            }
        } else {
            throw new Error();
        }

        if(acquirer == undefined) {
            toast.success(t("common.operations.success.new"));
        } else {
            toast.success(t("common.operations.success.edit"));
        }
        navigate("/admin/acquirerConfigurations")
    }
    
    return <>
        <PageMeta
            title={t("pages.acquirerConfigurations.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb
            pageTitle={t("pages.acquirerConfigurations.title")}
            breadcrumbs={[
                {
                    title: t("pages.acquirerConfigurations.title"),
                    to: "/admin/acquirerConfigurations",
                }
            ]}
            breadcrumb={title}
        />

        <ComponentCard title={title}>
            <AcquirerConfigurationForm
                model={acquirer}
                onSubmit={submit}
                submitText={t(`common.operations.save`, {
                    name: t("common.entities.acquirerConfiguration")
                })}
            />
        </ComponentCard>
    </>
}