import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router";
import PageMeta from "../../../components/common/PageMeta";
import PageBreadcrumb from "../../../components/common/PageBreadCrumb";
import ComponentCard from "../../../components/common/ComponentCard";
import { useState } from "react";
import { useCustomChargeMethodsQuery } from "../../../hooks/queries/implementations/useCustomChargeMethodsQuery";
import { DeleteEntityModal } from "../../../components/modals/DeleteEntityModal";
import { useCustomChargeMethodMutator } from "../../../hooks/mutators/useCustomChargeMethodMutator";
import { Entity } from "../../../hooks/EntitiesName";
import Button from "../../../components/ui/button/Button";
import { PencilIcon, PlusIcon, TrashBinIcon } from "../../../icons";
import { QueryPagination } from "../../../components/pagination/QueryPagination";
import { Divider } from "../../../components/dividers/Divider";
import { CustomChargeMethod } from "../../../hooks/api/Dtos/customchargemethods/CustomChargeMethod";
import Avatar from "../../../components/ui/avatar/Avatar";
import { ResponsiveTable } from "../../../components/tables/ResponsiveTable";

export const CustomChargeMethodsPage = () => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const mutator = useCustomChargeMethodMutator();

    const [state, setState] = useState({
        page: 0,
        pageSize: 25,
        deleteEntity: undefined as CustomChargeMethod | undefined,
    })
    const chargeMethodsQuery = useCustomChargeMethodsQuery({
        page: state.page,
        pageSize: state.pageSize,
    })

    return <>
        <PageMeta
            title={t("pages.customChargeMethods.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb pageTitle={t("pages.customChargeMethods.title")} />

        <ComponentCard title={t("pages.customChargeMethods.title")}>
            <div className="overflow-hidden rounded-2xl border border-gray-200 bg-white pt-4 dark:border-white/[0.05] dark:bg-white/[0.03]">
                <div className="flex flex-col gap-4 px-6 mb-4 sm:flex-row sm:items-center sm:justify-between">
                    <Button
                        size="md"
                        variant="primary"
                        startIcon={<PlusIcon />}
                        onClick={() => navigate("add")}
                    >
                        {
                            t("common.operations.new", {
                                name: t("common.entities.customChargeMethod")
                            })
                        }
                    </Button>
                </div>
                <div className="max-w-full overflow-x-auto">
                    <ResponsiveTable
                        isLoading={chargeMethodsQuery.isFirstLoading}
                        name={{
                            key: "name",
                            label: t("common.name"),
                            render: d => <>
                                <div className="flex items-center gap-3">
                                    <Avatar
                                        src={d.logoUrl}
                                        alt={d.name}
                                        size="large"
                                    />
                                    <div>
                                        <span className="block font-medium text-gray-800 text-theme-sm dark:text-white/90">
                                            {d.name}
                                        </span>
                                    </div>
                                </div>
                            </>,
                        }}
                        actions={[
                            {
                                render: () => <PencilIcon className="size-5" />,
                                key: "edit",
                                label: t("common.edit"),
                                onClick: d => navigate(`/settings/chargemethods/${d.id}/edit`)
                            },
                            {
                                render: () => <TrashBinIcon className="size-5" />,
                                key: "delete",
                                label: t("common.delete"),
                                onClick: d => setState(s => ({ ...s, deleteEntity: d}))
                            },
                        ]}
                        data={chargeMethodsQuery.data}
                        getKey={d => d.id}
                    />
                    <Divider />
                    <QueryPagination
                        query={chargeMethodsQuery}
                        onPageIndexChange={p => setState(s => ({ ...s, page: p }))}
                        pageSize={state.pageSize}
                    />
                </div>
            </div>
        </ComponentCard>
        <DeleteEntityModal
            model={state.deleteEntity}
            entity={Entity.CustomChargeMethods}
            action={s => mutator.delete(s)}
            getName={s => s.name}
            onClose={() => setState(s => ({ ...s, deleteEntity: undefined}))}
        />
    </>
}