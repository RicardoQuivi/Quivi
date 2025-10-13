import { useTranslation } from "react-i18next";
import { useLocation, useNavigate, useParams } from "react-router";
import { useMenuItemsQuery } from "../../../../hooks/queries/implementations/useMenuItemsQuery";
import { useMemo, useState } from "react";
import PageMeta from "../../../../components/common/PageMeta";
import PageBreadcrumb from "../../../../components/common/PageBreadCrumb";
import ComponentCard from "../../../../components/common/ComponentCard";
import { MenuItemForm, MenuItemFormState } from "./MenuItemForm";
import { useToast } from "../../../../layout/ToastProvider";
import { useMenuItemMutator } from "../../../../hooks/mutators/useMenuItemMutator";
import { Modal, ModalSize } from "../../../../components/ui/modal";
import { ModalButtonsFooter } from "../../../../components/ui/modal/ModalButtonsFooter";
import { MenuItem } from "../../../../hooks/api/Dtos/menuItems/MenuItem";

interface Props {
    readonly clone?: boolean;
}
export const MenuItemFormPage = (props: Props) => {
    const { id } = useParams();
    const { t } = useTranslation();
    const navigate = useNavigate();
    const toast = useToast();
    const mutator = useMenuItemMutator();
    const { search } = useLocation();
    const params = new URLSearchParams(search);

    const [cloneItemModalItem, setCloneItemModalItem] = useState<MenuItem>();

    const title = t(`common.operations.${id == undefined || props.clone == true ? "new" : "edit"}`, {
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
        if(item != undefined && props.clone != true) {
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
                modifierGroupIds: state.modifierGroupIds,
            })
            toast.success(t("common.operations.success.edit"));
            navigate("/businessProfile/menuManagement")
        } else {
            const item = await mutator.create({
                name: state.name,
                description: state.description,
                imageUrl: state.imageUrl,
                price: state.price,
                priceType: state.priceType,
                vatRate: state.vatRate,
                locationId: state.locationId,
                translations: state.translations,
                menuCategoryIds: state.menuCategoryIds,
                modifierGroupIds: state.modifierGroupIds,
            })
            toast.success(t("common.operations.success.new"));
            setCloneItemModalItem(item[0]);
        }
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
                isLoading={id != undefined && item == undefined}
            />
        </ComponentCard>

        <Modal
            isOpen={cloneItemModalItem != undefined}
            showCloseButton={false}
            size={ModalSize.Medium}
            title={t("pages.menuItems.copyItemModal.title")}
            footer={(
                <ModalButtonsFooter 
                    primaryButton={{
                        content: t("common.yes"),
                        onClick: () => {
                            if(cloneItemModalItem != undefined) {
                                navigate(`/businessProfile/menumanagement/items/${cloneItemModalItem.id}/clone`);
                                setCloneItemModalItem(undefined);
                            }
                        },
                    }}
                    secondaryButton={{
                        content: t("common.no"),
                        onClick: () => navigate("/businessProfile/menuManagement"),
                    }}
                />
            )}
        >
            <p className="text-sm text-gray-500 dark:text-gray-400">
                {t("pages.menuItems.copyItemModal.description")}
            </p>
        </Modal>
    </>
}