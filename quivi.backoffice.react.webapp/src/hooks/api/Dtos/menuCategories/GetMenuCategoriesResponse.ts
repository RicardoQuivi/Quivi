import { DataResponse } from "../DataResponse";
import { MenuCategory } from "../menuCategories/MenuCategory";

export interface GetMenuCategoriesResponse extends DataResponse<MenuCategory[]> {

}