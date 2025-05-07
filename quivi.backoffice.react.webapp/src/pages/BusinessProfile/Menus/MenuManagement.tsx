import { useTranslation } from "react-i18next"
import PageMeta from "../../../components/common/PageMeta";
import PageBreadcrumb from "../../../components/common/PageBreadCrumb";
import { useNavigate, useParams, useSearchParams } from "react-router";
import { CategoriesCard } from "./MenuCategories/CategoriesCard";
import { MenuItemsCard } from "./MenuItems/MenuItemsCard";
import { ModifierGroupsCard } from "./ModifierGroups/ModifierGroupsCard";

interface Props {
    readonly categories?: "All" | "None";
}
enum Tabs {
    Items = "Items",
    Modifiers = "Modifiers",
}
export const MenuManagementPage = (props: Props) => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const { categoryId } = useParams<string>();
    const [searchParams, setSearchParams] = useSearchParams();
    const tab = searchParams.get("tab") ?? Tabs.Items;

    const selectedCategory = categoryId != undefined ? categoryId : (props.categories == "All" ? undefined : null );
    
    const changeCategory = (c: string | undefined | null) => {
        if(c === undefined) {
            navigate(`/businessProfile/menumanagement`);
            return;
        }

        if(c === null) {
            navigate(`/businessProfile/menumanagement/categories/none/`)
            return;
        }

        navigate(`/businessProfile/menumanagement/categories/${c}/`)
    }

    return <>
        <PageMeta
            title={t("pages.menuManagement.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb
            pageTitle={t("pages.menuManagement.title")}
            breadcrumb={t("pages.menuManagement.title")}
        />

        <div className="grid grid-cols-12 gap-4">
            <div className="col-span-12">
                <nav className="flex overflow-x-auto rounded-lg bg-gray-100 p-1 dark:bg-gray-900 [&::-webkit-scrollbar]:h-1.5 [&::-webkit-scrollbar-track]:bg-white dark:[&::-webkit-scrollbar-track]:bg-transparent [&::-webkit-scrollbar-thumb]:rounded-full [&::-webkit-scrollbar-thumb]:bg-gray-200 dark:[&::-webkit-scrollbar-thumb]:bg-gray-600">
                    <button
                        onClick={() => setSearchParams(s => ({
                            ...s,
                           tab: Tabs.Items, 
                        }))}
                        className={`inline-flex items-center rounded-md px-3 py-2 text-sm font-medium transition-colors duration-200 ease-in-out ${
                            tab === Tabs.Items
                            ? "bg-white text-gray-900 shadow-theme-xs dark:bg-white/[0.03] dark:text-white"
                            : "bg-transparent text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
                        }`}
                    >
                        {t("pages.menuManagement.itemsAndCategories")}
                    </button>

                    <button
                        onClick={() => setSearchParams(s => ({
                            ...s,
                           tab: Tabs.Modifiers, 
                        }))}
                        className={`inline-flex items-center rounded-md px-3 py-2 text-sm font-medium transition-colors duration-200 ease-in-out ${
                            tab === Tabs.Modifiers
                            ? "bg-white text-gray-900 shadow-theme-xs dark:bg-white/[0.03] dark:text-white"
                            : "bg-transparent text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
                        }`}
                    >
                        {t("pages.menuManagement.modifiers")}
                    </button>
                </nav>
            </div>

            {
                tab == Tabs.Items
                ?
                <>
                    <div className="col-span-12 lg:col-span-4 h-full">
                        <CategoriesCard
                            categoryId={selectedCategory}
                            onCategoryChanged={changeCategory}
                        />
                    </div>

                    <div className="col-span-12 lg:col-span-8 h-full">
                        <MenuItemsCard
                            categoryId={selectedCategory}
                        />
                    </div>
                </>
                :
                <div className="col-span-12">
                    <ModifierGroupsCard />
                </div>
            }
        </div>
    </>
}