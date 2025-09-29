import { Trans, useTranslation } from "react-i18next";
import { useNavigate } from "react-router";
import { useMenuItemMutator } from "../../../../hooks/mutators/useMenuItemMutator";
import useDebouncedState from "../../../../hooks/useDebouncedState";
import { MenuItem } from "../../../../hooks/api/Dtos/menuItems/MenuItem";
import { useMemo, useState } from "react";
import { useMenuItemsQuery } from "../../../../hooks/queries/implementations/useMenuItemsQuery";
import { useLocalsQuery } from "../../../../hooks/queries/implementations/useLocalsQuery";
import { useMenuCategoriesQuery } from "../../../../hooks/queries/implementations/useMenuCategoriesQuery";
import ComponentCard from "../../../../components/common/ComponentCard";
import Button from "../../../../components/ui/button/Button";
import { CopyIcon, PencilIcon, PlusIcon, SearchIcon, TrashBinIcon } from "../../../../icons";
import Avatar from "../../../../components/ui/avatar/Avatar";
import Badge from "../../../../components/ui/badge/Badge";
import { Skeleton } from "../../../../components/ui/skeleton/Skeleton";
import CurrencySpan from "../../../../components/currency/CurrencySpan";
import { DeleteEntityModal } from "../../../../components/modals/DeleteEntityModal";
import { Entity } from "../../../../hooks/EntitiesName";
import { ResponsiveTable } from "../../../../components/tables/ResponsiveTable";
import { Collections } from "../../../../utilities/Collectionts";

interface ItemsCardProps {
    readonly categoryId: string | undefined | null;   
}
export const MenuItemsCard = (props: ItemsCardProps) => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const mutator = useMenuItemMutator();

    const [search, setSearch, debouncedSearch, _] = useDebouncedState("", 300);
    const [menuItemToDelete, setMenuItemToDelete] = useState<MenuItem>();

    const menuItemsQuery = useMenuItemsQuery({
        itemCategoryId: props.categoryId ?? undefined,
        hasCategory: props.categoryId === null ? false : undefined,
        search: !!debouncedSearch ? debouncedSearch : undefined,
        page: 0,
    });
    const localIds = useMemo(() => {
        const set = new Set<string>();
        for(const item of menuItemsQuery.data) {
            if(item.locationId == undefined) {
                continue;
            }

            set.add(item.locationId);
        }
        return Array.from(set.values());
    }, [menuItemsQuery.data])

    const localsQuery = useLocalsQuery({
        ids: localIds,
        page: 0,
    })
    const localsMap = useMemo(() => Collections.toMap(localsQuery.data, c => c.id), [localsQuery.data])

    const categoriesQuery = useMenuCategoriesQuery({
        page: 0,
    })
    const categoriesMap = useMemo(() => Collections.toMap(categoriesQuery.data, c => c.id), [categoriesQuery.data])

    return <ComponentCard
        title={t("common.entities.menuItems")}
        className="size-full"
    >
        <div className="flex flex-col gap-2 px-4 py-4 border border-b-0 border-gray-100 dark:border-white/[0.05] rounded-t-xl sm:flex-row sm:items-center sm:justify-between">
            <div className="flex items-center gap-3">
                <Button
                    variant="primary"
                    size="sm"
                    startIcon={<PlusIcon />}
                    onClick={() => navigate(`/businessProfile/menumanagement/items/add?${props.categoryId == undefined ? "" : `categoryId=${props.categoryId}`}`)}
                >
                    {
                        t("common.operations.new", {
                            name: t("common.entities.menuItem")
                        })
                    }
                </Button>
            </div>

            <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
                <div className="relative">
                    <button className="absolute text-gray-500 -translate-y-1/2 left-4 top-1/2 dark:text-gray-400">
                        <SearchIcon />
                    </button>

                    <input
                        type="text"
                        placeholder={t("common.startTypingToSearch")}
                        value={search}
                        onChange={e => setSearch(e.target.value)}

                        className="dark:bg-dark-900 h-11 w-full rounded-lg border border-gray-300 bg-transparent py-2.5 pl-11 pr-4 text-sm text-gray-800 shadow-theme-xs placeholder:text-gray-400 focus:border-brand-300 focus:outline-hidden focus:ring-3 focus:ring-brand-500/10 dark:border-gray-700 dark:bg-gray-900 dark:text-white/90 dark:placeholder:text-white/30 dark:focus:border-brand-800 xl:w-[300px]"
                    />
                </div>
            </div>
        </div>
        <div className="max-w-full overflow-x-auto">
            <ResponsiveTable
                isLoading={menuItemsQuery.isFirstLoading}
                name={{
                    key: "name",
                    label: t("common.name"),
                    render: item => <>
                        <div className="flex items-center gap-3">
                            <div className="aspect-square">
                                <Avatar
                                    src={item.imageUrl}
                                    alt={item.name}
                                    size="large"
                                />
                            </div>
                            <div>
                                <span className="block font-medium text-gray-800 text-theme-sm dark:text-white/90">
                                    {item.name}
                                </span>
                                <span className="block text-gray-500 text-theme-xs dark:text-gray-400">
                                    {item.description}
                                </span>
                            </div>
                        </div>
                    </>
                }}
                columns={[
                    {
                        key: "categories",
                        label: t("common.entities.menuCategories"),
                        render: item => (
                            categoriesQuery.isFirstLoading
                            ?
                            <Skeleton className="w-24"/>
                            :
                            <div className="flex items-center gap-2">
                                {
                                    item.menuCategoryIds.length > 0
                                    ?
                                        item.menuCategoryIds.map(id => {
                                            const category = categoriesMap.get(id);
                                            if(category == undefined) {
                                                return;
                                            }

                                            const isSelectedCategory = id === props.categoryId;
                                            return <div
                                                key={id}
                                                className={isSelectedCategory ? "order-first" : ""}
                                            >
                                                <Badge 
                                                    variant={isSelectedCategory ? "solid" : "light"}
                                                    color={isSelectedCategory ? "primary" : "light"}
                                                >
                                                    {category.name}
                                                </Badge>
                                            </div>
                                        })
                                    :
                                    <span className="block font-medium text-gray-800 text-theme-sm dark:text-white/90">
                                        {t("pages.menuManagement.noCategory")}
                                    </span>
                                }
                            </div>
                        )
                    },
                    {
                        key: "price",
                        label: t("common.price"),
                        render: item => <>
                            <span className="block font-medium text-gray-800 text-theme-sm dark:text-white/90">
                                <CurrencySpan value={item.price} />
                            </span>
                            <span className="block text-gray-500 text-theme-xs dark:text-gray-400">
                                <Trans
                                    t={t}
                                    i18nKey="common.vatRateIncluded"
                                    shouldUnescape={true}
                                    values={{
                                        value: `${item.vatRate.toFixed(0)}%`
                                    }}
                                />
                            </span>
                        </>
                    },
                    {
                        key: "location",
                        label: t("common.entities.local"),
                        render: item => <>
                            {
                                item.locationId == undefined
                                ?
                                t("common.noLocation")
                                :
                                (
                                    localsMap.has(item.locationId)
                                    ?
                                    localsMap.get(item.locationId)!.name
                                    :
                                    <Skeleton className="w-md"/>
                                )
                            }
                        </>
                    },
                ]}
                actions={[
                    {
                        render: () => <CopyIcon className="size-5" />,
                        key: "copy",
                        label: t("pages.menuItems.copy"),
                        onClick: d => navigate(`/businessProfile/menumanagement/items/${d.id}/clone`)
                    },
                    {
                        render: () => <PencilIcon className="size-5" />,
                        key: "edit",
                        label: t("common.edit"),
                        onClick: d => navigate(`/businessProfile/menumanagement/items/${d.id}/edit`)
                    },
                    {
                        render: () => <TrashBinIcon className="size-5" />,
                        key: "delete",
                        label: t("common.delete"),
                        onClick: d => setMenuItemToDelete(d)
                    },
                ]}
                data={menuItemsQuery.data}
                getKey={d => d.id}
            />
        </div>
        <DeleteEntityModal
            model={menuItemToDelete}
            entity={Entity.MenuItems}
            getName={m => m.name}
            action={mutator.delete}
            onClose={() => setMenuItemToDelete(undefined)}
        />
    </ComponentCard>
}