import { useTranslation } from "react-i18next";
import { useLocation, useNavigate, useParams } from "react-router";
import { useMenuItemsQuery } from "../../../../hooks/queries/implementations/useMenuItemsQuery";
import { useMemo } from "react";
import PageMeta from "../../../../components/common/PageMeta";
import PageBreadcrumb from "../../../../components/common/PageBreadCrumb";
import ComponentCard from "../../../../components/common/ComponentCard";
import { MenuItemForm, MenuItemFormState } from "./MenuItemForm";
import { useToast } from "../../../../layout/ToastProvider";
import { useMenuItemMutator } from "../../../../hooks/mutators/useMenuItemMutator";

export const MenuItemFormPage = () => {
    const { id } = useParams();
    const { t } = useTranslation();
    const navigate = useNavigate();
    const toast = useToast();
    const mutator = useMenuItemMutator();
    const { search } = useLocation();
    const params = new URLSearchParams(search);

    const title = t(`common.operations.${id == undefined ? "new" : "edit"}`, {
        name: t("common.entities.menuItem")
    });

    const itemsQuery = useMenuItemsQuery(id == undefined ? undefined : {
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

    const submit = async (state: MenuItemFormState) => {
        if(item == undefined) {
            await mutator.create({
                name: state.name,
                description: state.description,
                imageUrl: state.imageUrl,
                price: state.price,
                priceType: state.priceType,
                vatRate: state.vatRate,
                locationId: state.locationId,
                translations: state.translations,
                menuCategoryIds: state.menuCategoryIds,
            })
            toast.success(t("common.operations.success.new"));
        } else {
            await mutator.patch(item, {
                name: state.name,
                description: state.description,
                price: state.price,
                priceType: state.priceType,
                vatRate: state.vatRate,
                locationId: state.locationId,
                imageUrl: state.imageUrl,
                translations: state.translations,
                menuCategoryIds: state.menuCategoryIds,
            })
            toast.success(t("common.operations.success.edit"));
        }
        navigate("/businessProfile/menuManagement")
    }

    return <>
        <PageMeta
            title={t("pages.menuItems.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb
            pageTitle={t("pages.menuItems.title")}
            breadcrumbs={[
                {
                    title: t("pages.menuManagement.title"),
                    to: "/businessProfile/menumanagement",
                }
            ]}
            breadcrumb={title}
        />

        <ComponentCard title={title}>
            <MenuItemForm
                model={item}
                onSubmit={submit}
                submitText={t(`common.operations.save`, {
                    name: t("common.entities.menuItem")
                })}
                categoryId={params.get("categoryId") ?? undefined}
            />
        </ComponentCard>
    </>
}