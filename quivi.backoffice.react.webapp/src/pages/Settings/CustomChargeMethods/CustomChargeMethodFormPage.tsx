import { useTranslation } from "react-i18next";
import { useNavigate, useParams } from "react-router";
import { useToast } from "../../../layout/ToastProvider";
import { useMemo } from "react";
import PageMeta from "../../../components/common/PageMeta";
import PageBreadcrumb from "../../../components/common/PageBreadCrumb";
import ComponentCard from "../../../components/common/ComponentCard";
import { useCustomChargeMethodsQuery } from "../../../hooks/queries/implementations/useCustomChargeMethodsQuery";
import { useCustomChargeMethodMutator } from "../../../hooks/mutators/useCustomChargeMethodMutator";
import { CustomChargeMethodForm, CustomChargeMethodFormState } from "./CustomChargeMethodForm";

export const CustomChargeMethodFormPage = () => {
    const { id } = useParams();
    const { t } = useTranslation();
    const navigate = useNavigate();
    const toast = useToast();
    const mutator = useCustomChargeMethodMutator();

    const title = t(`common.operations.${id == undefined ? 'new' : 'edit'}`, {
        name: t("common.entities.customChargeMethod")
    });

    const customChargeMethodsQuery = useCustomChargeMethodsQuery(id == undefined ? undefined : {
        ids: [ id ],
        page: 0,
    })

    const customChargeMethod = useMemo(() => {
        if(id == undefined) {
            return undefined;
        }
        if(customChargeMethodsQuery.data.length == 0) {
            return undefined;
        }
        return customChargeMethodsQuery.data[0];
    }, [id, customChargeMethodsQuery.data])

    const submit = async (state: CustomChargeMethodFormState) => {
        if(customChargeMethod == undefined) {
            await mutator.create({
                name: state.name,
            })
            toast.success(t("common.operations.success.new"));
        } else {
            await mutator.patch(customChargeMethod, {
                name: state.name,
            })
            toast.success(t("common.operations.success.edit"));
        }
        navigate("/settings/chargemethods")
    }
    
    return <>
        <PageMeta
            title={t("pages.customChargeMethods.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb
            pageTitle={t("pages.customChargeMethods.title")}
            breadcrumbs={[
                {
                    title: t("pages.customChargeMethods.title"),
                    to: "/settings/chargeMethods",
                }
            ]}
            breadcrumb={title}
        />

        <ComponentCard title={title}>
            <CustomChargeMethodForm
                model={customChargeMethod}
                onSubmit={submit}
                submitText={title}
            />
        </ComponentCard>
    </>
}