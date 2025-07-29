import { useTranslation } from "react-i18next";
import ComponentCard from "../../../../components/common/ComponentCard"
import { useNavigate, useParams } from "react-router";
import PageMeta from "../../../../components/common/PageMeta";
import PageBreadcrumb from "../../../../components/common/PageBreadCrumb";
import { useMenuCategoriesQuery } from "../../../../hooks/queries/implementations/useMenuCategoriesQuery";
import { useMemo } from "react";
import { MenuCategoryForm, MenuCategoryFormState } from "./MenuCategoryForm";
import { useMenuCategoryMutator } from "../../../../hooks/mutators/useMenuCategoryMutator";
import { useToast } from "../../../../layout/ToastProvider";

export const MenuCategoryFormPage = () => {
    const { id } = useParams();
    const { t } = useTranslation();
    const navigate = useNavigate();
    const toast = useToast();
    const mutator = useMenuCategoryMutator();

    const title = t(`common.operations.save`, {
        name: t("common.entities.menuCategory")
    });

    const categoriesQuery = useMenuCategoriesQuery(id == undefined ? undefined : {
        ids: [ id ],
        page: 0,
    })

    const category = useMemo(() => {
        if(id == undefined) {
            return undefined;
        }
        if(categoriesQuery.data.length == 0) {
            return undefined;
        }
        return categoriesQuery.data[0];
    }, [id, categoriesQuery.data])

    const submit = async (state: MenuCategoryFormState) => {
        if(category == undefined) {
            await mutator.create({
                name: state.name,
                imageUrl: state.imageUrl,
                translations: state.translations,
            })
            toast.success(t("common.operations.success.new"));
        } else {
            await mutator.patch(category, {
                name: state.name,
                imageUrl: state.imageUrl,
                translations: state.translations,
            })
            toast.success(t("common.operations.success.edit"));
        }
        navigate("/businessProfile/menuManagement")
    }

    return <>
        <PageMeta
            title={t("pages.menuCategories.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb
            pageTitle={t("pages.menuCategories.title")}
            breadcrumbs={[
                {
                    title: t("pages.menuManagement.title"),
                    to: "/businessProfile/menumanagement",
                }
            ]}
            breadcrumb={title}
        />

        <ComponentCard title={title}>
            <MenuCategoryForm
                model={category}
                onSubmit={submit}
                submitText={title}
            />
        </ComponentCard>
    </>
}