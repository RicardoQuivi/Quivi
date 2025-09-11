import { useTranslation } from "react-i18next";
import { useLocation, useNavigate, useParams } from "react-router";
import { useMemo } from "react";
import PageMeta from "../../../../components/common/PageMeta";
import PageBreadcrumb from "../../../../components/common/PageBreadCrumb";
import ComponentCard from "../../../../components/common/ComponentCard";
import { ModifierGroupForm } from "./ModifierGroupForm";
import { useToast } from "../../../../layout/ToastProvider";
import { useModifierGroupMutator } from "../../../../hooks/mutators/useModifierGroupMutator";
import { useModifierGroupsQuery } from "../../../../hooks/queries/implementations/useModifierGroupsQuery";
import { ModifierGroupFormState } from "./ModifierGroupBaseForm";

export const ModifierGroupFormPage = () => {
    const { id } = useParams();
    const { t } = useTranslation();
    const navigate = useNavigate();
    const toast = useToast();
    const mutator = useModifierGroupMutator();
    const { search } = useLocation();
    const params = new URLSearchParams(search);

    const title = t(`common.operations.${id == undefined ? "new" : "edit"}`, {
        name: t("common.entities.modifierGroup")
    });

    const modifierQuery = useModifierGroupsQuery(id == undefined ? undefined : {
        ids: [ id ],
        page: 0,
    })

    const item = useMemo(() => {
        if(id == undefined) {
            return undefined;
        }
        if(modifierQuery.data.length == 0) {
            return undefined;
        }
        return modifierQuery.data[0];
    }, [id, modifierQuery.data])

    const submit = async (state: ModifierGroupFormState) => {
        if(item == undefined) {
            await mutator.create({
                name: state.name,
                minSelection: state.minSelection,
                maxSelection: state.maxSelection,
                items: state.items,
                translations: state.translations,
            })
            toast.success(t("common.operations.success.new"));
        } else {
            await mutator.patch(item, {
                name: state.name,
                minSelection: state.minSelection,
                maxSelection: state.maxSelection,
                items: state.items,
                translations: state.translations,
            })
            toast.success(t("common.operations.success.edit"));
        }
        navigate("/businessProfile/menumanagement?tab=Modifiers")
    }

    return <>
        <PageMeta
            title={t("pages.modifierGroups.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb
            pageTitle={t("pages.modifierGroups.title")}
            breadcrumbs={[
                {
                    title: t("pages.menuManagement.title"),
                    to: "/businessProfile/menumanagement?tab=Modifiers",
                }
            ]}
            breadcrumb={title}
        />

        <ComponentCard title={title}>
            <ModifierGroupForm
                model={item}
                onSubmit={submit}
                submitText={t(`common.operations.save`, {
                    name: t("common.entities.modifierGroup")
                })}
                categoryId={params.get("categoryId") ?? undefined}
            />
        </ComponentCard>
    </>
}