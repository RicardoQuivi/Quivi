import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router";
import { useModifierGroupMutator } from "../../../../hooks/mutators/useModifierGroupMutator";
import { useState } from "react";
import { ModifierGroup } from "../../../../hooks/api/Dtos/modifierGroups/ModifierGroup";
import { useModifierGroupsQuery } from "../../../../hooks/queries/implementations/useModifierGroupsQuery";
import ComponentCard from "../../../../components/common/ComponentCard";
import Button from "../../../../components/ui/button/Button";
import { PencilIcon, PlusIcon, TrashBinIcon } from "../../../../icons";
import ResponsiveTable from "../../../../components/tables/ResponsiveTable";
import { Tooltip } from "../../../../components/ui/tooltip/Tooltip";
import { IconButton } from "../../../../components/ui/button/IconButton";
import { Divider } from "../../../../components/dividers/Divider";
import { QueryPagination } from "../../../../components/pagination/QueryPagination";
import { DeleteEntityModal } from "../../../../components/modals/DeleteEntityModal";
import { Entity } from "../../../../hooks/EntitiesName";

export const ModifierGroupsCard = () => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const mutator = useModifierGroupMutator();

    const [state, setState] = useState({
        page: 0,
        pageSize: 25,
        deleteEntity: undefined as ModifierGroup | undefined,
    })
    const groupsQuery = useModifierGroupsQuery({
        page: state.page,
        pageSize: state.pageSize,
    })

    const rowAction = (evt: React.MouseEvent<HTMLElement, MouseEvent>, action: () => any) => {
        evt.stopPropagation();
        action();
    }

    return <>
        <ComponentCard title={t("pages.modifierGroups.title")}>
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
                                name: t("common.entities.modifierGroup")
                            })
                        }
                    </Button>
                </div>
                <div className="max-w-full overflow-x-auto">
                    <ResponsiveTable
                        isLoading={groupsQuery.isFirstLoading}
                        columns={[
                            {
                                key: "name",
                                label: t("common.name"),
                                render: (d) => d.name,
                            },
                            {
                                label: "",
                                render: d => <>
                                    <Tooltip message={t("common.edit")}>
                                        <IconButton
                                            onClick={(e) => rowAction(e, () => navigate(`/businessProfile/menumanagement/modifiers/${d.id}/edit`))}
                                            className="!text-gray-700 hover:!text-error-500 dark:!text-gray-400 dark:!hover:text-error-500"
                                        >
                                            <PencilIcon className="size-5" />
                                        </IconButton>
                                    </Tooltip>
                                    <Tooltip message={t("common.delete")}>
                                        <IconButton
                                            onClick={() => setState(s => ({ ...s, deleteEntity: d }))}
                                            className="!text-gray-700 hover:!text-error-500 dark:!text-gray-400 dark:!hover:text-error-500"
                                        >
                                            <TrashBinIcon className="size-5" />
                                        </IconButton>
                                    </Tooltip>
                                </>,
                                key: "actions",
                                isActions: true,
                            },
                        ]}
                        data={groupsQuery.data}
                        getKey={d => d.id}
                    />
                    <Divider />
                    <QueryPagination
                        query={groupsQuery}
                        onPageIndexChange={p => setState(s => ({ ...s, page: p }))}
                        pageSize={state.pageSize}
                    />
                </div>
            </div>
        </ComponentCard>
        <DeleteEntityModal
            model={state.deleteEntity}
            entity={Entity.ModifierGroups}
            action={s => mutator.delete(s)}
            getName={s => s.name}
            onClose={() => setState(s => ({ ...s, deleteEntity: undefined}))}
        />
    </>
}