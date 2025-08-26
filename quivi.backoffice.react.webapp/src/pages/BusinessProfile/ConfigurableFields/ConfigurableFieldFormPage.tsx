import { useTranslation } from "react-i18next"
import PageMeta from "../../../components/common/PageMeta";
import PageBreadcrumb from "../../../components/common/PageBreadCrumb";
import ComponentCard from "../../../components/common/ComponentCard";
import { useMemo } from "react";
import { useNavigate, useParams } from "react-router";
import { useToast } from "../../../layout/ToastProvider";
import { useConfigurableFieldMutator } from "../../../hooks/mutators/useConfigurableFieldMutator";
import { ConfigurableFieldForm, ConfigurableFieldFormState } from "./ConfigurableFieldForm";
import { useConfigurableFieldsQuery } from "../../../hooks/queries/implementations/useConfigurableFieldsQuery";

export const ConfigurableFieldFormPage = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const { t } = useTranslation();
    const toast = useToast();

    const mutator = useConfigurableFieldMutator();
    
    const configurableFieldQuery = useConfigurableFieldsQuery(id == undefined ? undefined : {
        ids: [id],
        page: 0,
    })

    const configurableField = useMemo(() => {
        if(id == undefined) {
            return undefined;
        }
        if(configurableFieldQuery.data.length == 0) {
            return undefined;
        }
        return configurableFieldQuery.data[0];
    }, [id, configurableFieldQuery.data])

    const submit = async (state: ConfigurableFieldFormState) => {
        if(configurableField == undefined) {
            await mutator.create({
                name: state.name,
                assignedOn: state.assignedOn,
                isAutoFill: state.isAutoFill,
                isRequired: state.isRequired,
                printedOn: state.printedOn,
                type: state.type,
                defaultValue: state.defaultValue,
                translations: state.translations,
            })
            toast.success(t("common.operations.success.new"));
        } else {
            await mutator.patch(configurableField, {
                name: state.name,
                assignedOn: state.assignedOn,
                isAutoFill: state.isAutoFill,
                isRequired: state.isRequired,
                printedOn: state.printedOn,
                type: state.type,
                defaultValue: state.defaultValue,
                translations: state.translations,
            })
            toast.success(t("common.operations.success.edit"));
        }
        navigate("/businessProfile/configurablefields")
    }

    const title = t(`common.operations.${id == undefined ? "new" : "edit"}`, {
        name: t("common.entities.configurableFields")
    });

    return <>
        <PageMeta
            title={t("pages.configurableFields.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb
            pageTitle={t("pages.configurableFields.title")}
            breadcrumbs={[
                {
                    title: t("pages.configurableFields.title"),
                    to: "/businessProfile/configurablefields",
                }
            ]}
            breadcrumb={title}
        />

        <ComponentCard title={title}>
            <ConfigurableFieldForm
                model={configurableField}
                onSubmit={submit}
                submitText={t(`common.operations.save`, {
                    name: t("common.entities.configurableField")
                })}
            />
        </ComponentCard>
    </>
}