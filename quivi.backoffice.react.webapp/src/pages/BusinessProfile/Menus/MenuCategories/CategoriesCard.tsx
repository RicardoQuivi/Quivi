import { useEffect, useState } from "react";
import ComponentCard from "../../../../components/common/ComponentCard";
import { DraggingContainer, InjectedElementProps } from "../../../../components/draggable/DraggingContainer";
import { DeleteEntityModal } from "../../../../components/modals/DeleteEntityModal";
import Button from "../../../../components/ui/button/Button";
import { Skeleton } from "../../../../components/ui/skeleton/Skeleton";
import { Entity } from "../../../../hooks/EntitiesName";
import { HandleIcon, PencilIcon, PlusIcon, TrashBinIcon } from "../../../../icons";
import { MenuCategory } from "../../../../hooks/api/Dtos/menuCategories/MenuCategory";
import { useMenuCategoriesQuery } from "../../../../hooks/queries/implementations/useMenuCategoriesQuery";
import { useMenuCategoryMutator } from "../../../../hooks/mutators/useMenuCategoryMutator";
import { useToast } from "../../../../layout/ToastProvider";
import { useNavigate } from "react-router";
import { useTranslation } from "react-i18next";

interface CategoriesCardProps {
    readonly categoryId: string | undefined | null;
    readonly onCategoryChanged: (category: string | undefined | null) => any;
}
export const CategoriesCard = (props: CategoriesCardProps) => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const toast = useToast(); 
    const mutator = useMenuCategoryMutator();
    const menuCategoriesQuery = useMenuCategoriesQuery({
        page: 0,
    });

    const [menuCategoryToDelete, setMenuCategoryToDelete] = useState<MenuCategory>();

    const onOrderChanged = async (items: MenuCategory[]) => {
        setItems(items);
        await mutator.sort(items);
        toast.success(t("common.operations.success.edit"));
    }

    const [items, setItems] = useState<MenuCategory[]>(() => menuCategoriesQuery.data.sort((a, b) => a.sortIndex - b.sortIndex));

    useEffect(() => setItems(menuCategoriesQuery.data.sort((a, b) => a.sortIndex - b.sortIndex)), [menuCategoriesQuery.data])

    return (
    <ComponentCard
        title={t("common.entities.menuCategories")}
        description={
            <Button
                size="md"
                variant="primary"
                startIcon={<PlusIcon />}
                onClick={() => navigate("/businessProfile/menumanagement/categories/add")}
                className="mt-4 w-full"
            >
                {
                    t("common.operations.new", {
                        name: t("common.entities.menuCategory")
                    })
                }
            </Button>
        }
        className="size-full"
    >
        <div className="overflow-x-auto pb-2 width-full [&::-webkit-scrollbar-thumb]:rounded-full [&::-webkit-scrollbar-thumb]:bg-gray-100 dark:[&::-webkit-scrollbar-thumb]:bg-gray-600 [&::-webkit-scrollbar-track]:bg-white dark:[&::-webkit-scrollbar-track]:bg-transparent [&::-webkit-scrollbar]:h-1.5">
            <nav className="flex flex-col w-full sm:flex-col sm:space-y-2">
                <button
                    className={`inline-flex items-center rounded-lg px-3 py-2 text-sm font-medium transition-colors duration-200 ease-in-out sm:p-3 ${
                        props.categoryId === undefined
                        ? "text-brand-500 dark:bg-brand-400/20 dark:text-brand-400 bg-brand-50"
                        : "bg-transparent text-gray-500 border-transparent hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
                    }`}
                    onClick={() => props.onCategoryChanged(undefined)}
                >
                    {t("pages.menuManagement.all")}
                </button>
                <button
                    className={`inline-flex items-center rounded-lg px-3 py-2.5 text-sm font-medium transition-colors duration-200 ease-in-out sm:p-3 ${
                        props.categoryId === null
                        ? "text-brand-500 dark:bg-brand-400/20 dark:text-brand-400 bg-brand-50"
                        : "bg-transparent text-gray-500 border-transparent hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
                    }`}
                    onClick={() => props.onCategoryChanged(null)}
                >
                    {t("pages.menuManagement.noCategory")}
                </button>

                {
                    menuCategoriesQuery.isFirstLoading
                    ?
                    [1, 2, 3, 4, 5].map(c => (
                        <button
                            key={`Loading-${c}`}
                            className={`inline-flex items-center rounded-lg px-3 py-2.5 text-sm font-medium transition-colors duration-200 ease-in-out sm:p-3 ${"bg-transparent text-gray-500 border-transparent hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"}`}
                            disabled
                        >
                            <Skeleton className="w-full" />
                        </button>
                    ))
                    :
                    <DraggingContainer 
                        items={items}
                        onOrderChanged={onOrderChanged}
                        render={(c, injectedProps: InjectedElementProps<HTMLButtonElement>) => (<button
                            {...injectedProps}
                            key={c.id}
                            className={`w-full inline-flex items-center rounded-lg px-3 py-2.5 text-sm font-medium transition-colors duration-200 ease-in-out sm:p-3 ${
                                props.categoryId == c.id
                                ? "text-brand-500 dark:bg-brand-400/20 dark:text-brand-400 bg-brand-50"
                                : "bg-transparent text-gray-500 border-transparent hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
                            }`}
                            onClick={() => props.onCategoryChanged(c.id)}
                        >
                            <div className="grid grid-cols-[auto_1fr_auto_auto] gap-2 w-full">
                                <div className="cursor-grab">
                                    <HandleIcon className="size-5 m-1" />
                                </div>
                                <div className="text-start flex items-center">
                                    {c.name}
                                </div>
                                <div
                                    className={`flex items-center justify-center text-gray-500 transition-colors border border-gray-200 rounded-lg max-w-10 hover:bg-gray-100 hover:text-gray-700 dark:border-gray-800 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-white`}
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        navigate(`/businessProfile/menumanagement/categories/${c.id}/edit`);
                                    }}
                                >
                                    <PencilIcon className="size-5 m-1" />
                                </div>
                                <div
                                    className={`flex items-center justify-center text-gray-500 transition-colors border border-gray-200 rounded-lg max-w-10 hover:bg-gray-100 hover:text-gray-700 dark:border-gray-800 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-white`}
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        setMenuCategoryToDelete(c);
                                    }}
                                >
                                    <TrashBinIcon className="size-5 m-1" />
                                </div>
                            </div>
                        </button>
                        )}                      
                    />
                }
            </nav>
        </div>
        <DeleteEntityModal
            model={menuCategoryToDelete}
            entity={Entity.MenuCategories}
            getName={m => m.name}
            action={async m => {
                await mutator.delete(m);
                if(m.id == props.categoryId) {
                    props.onCategoryChanged(undefined);
                }
            }}
            onClose={() => setMenuCategoryToDelete(undefined)}
        />
    </ComponentCard>
    )
}