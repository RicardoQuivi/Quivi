import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router";
import PageMeta from "../../../components/common/PageMeta";
import PageBreadcrumb from "../../../components/common/PageBreadCrumb";
import ComponentCard from "../../../components/common/ComponentCard";
import { useState } from "react";
import { DeleteEntityModal } from "../../../components/modals/DeleteEntityModal";
import { Entity } from "../../../hooks/EntitiesName";
import Button from "../../../components/ui/button/Button";
import { KeyIcon, PencilIcon, PlusIcon, TrashBinIcon } from "../../../icons";
import { QueryPagination } from "../../../components/pagination/QueryPagination";
import { Divider } from "../../../components/dividers/Divider";
import { useEmployeeMutator } from "../../../hooks/mutators/useEmployeeMutator";
import { Employee } from "../../../hooks/api/Dtos/employees/Employee";
import { useEmployeesQuery } from "../../../hooks/queries/implementations/useEmployeesQuery";
import { ResetEmployeePinModal } from "./ResetEmployeePinModal";
import { ResponsiveTable } from "../../../components/tables/ResponsiveTable";

export const EmployeesPage = () => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const mutator = useEmployeeMutator();

    const [state, setState] = useState({
        page: 0,
        pageSize: 25,
        deleteEntity: undefined as Employee | undefined,
        resetEntity: undefined as Employee | undefined,
    })
    const employeesQuery = useEmployeesQuery({
        page: state.page,
        pageSize: state.pageSize,
    })

    return <>
        <PageMeta
            title={t("pages.employees.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb pageTitle={t("pages.employees.title")} />

        <ComponentCard title={t("pages.employees.title")}>
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
                                name: t("common.entities.employee")
                            })
                        }
                    </Button>
                </div>
                <div className="max-w-full overflow-x-auto">
                    <ResponsiveTable
                        isLoading={employeesQuery.isFirstLoading}
                        name={{
                            key: "name",
                            render: (d) => d.name,
                            label: t("common.name"),
                        }}
                        actions={[
                            {
                                render: d => d.hasPinCode == true ? <KeyIcon className="size-5" /> : undefined,
                                key: "resetPin",
                                label: t("pages.employees.resetPin"),
                                onClick: d => setState(s => ({ ...s, resetEntity: d})),
                            },
                            {
                                render: () => <PencilIcon className="size-5" />,
                                key: "edit",
                                label: t("common.edit"),
                                onClick: d => navigate(`/settings/employees/${d.id}/edit`),
                            },
                            {
                                render: () => <TrashBinIcon className="size-5" />,
                                key: "delete",
                                label: t("common.delete"),
                                onClick: d => setState(s => ({ ...s, deleteEntity: d}))
                            },
                        ]}
                        data={employeesQuery.data}
                        getKey={d => d.id}
                    />
                    <Divider />
                    <QueryPagination
                        query={employeesQuery}
                        onPageIndexChange={p => setState(s => ({ ...s, page: p }))}
                        pageSize={state.pageSize}
                    />
                </div>
            </div>
        </ComponentCard>
        <DeleteEntityModal
            model={state.deleteEntity}
            entity={Entity.Employees}
            action={s => mutator.delete(s)}
            getName={s => s.name}
            onClose={() => setState(s => ({ ...s, deleteEntity: undefined}))}
        />
        <ResetEmployeePinModal
            model={state.resetEntity}
            onClose={() => setState(s => ({ ...s, resetEntity: undefined}))}
        />
    </>
}