import type { MenuCategory } from "../../hooks/api/Dtos/menuCategories/MenuCategory";
import type { MenuItem } from "../../hooks/api/Dtos/menuItems/MenuItem";
import { useEffect, useState } from "react";
import { useChannelContext } from "../../context/AppContextProvider";
import { useMenuItemsQuery } from "../../hooks/queries/implementations/useMenuItemsQuery";
import { TextDivider } from "../../components/Shared/TextDivider";
import { Grid } from "@mui/material";
import { MenuItemComponent } from "../../components/Menu/MenuItemComponent";

interface MenuCategoriesProps  {
    readonly category: MenuCategory;
    readonly atTimestamp?: number,
    readonly onItemSelect: (item: MenuItem) => void;
    readonly hideDivider?: boolean;
    readonly shouldLoad: boolean;
}
export const MenuCategorySection = ({
    category,
    atTimestamp,
    onItemSelect,
    hideDivider,
    shouldLoad,
}: MenuCategoriesProps) => {
    const [internalShouldLoad, setInternalShouldLoad] = useState(shouldLoad)

    const channelContext = useChannelContext();
    const orderingFeatures = channelContext.features.ordering;
    const atDate = atTimestamp == undefined ? undefined : new Date(atTimestamp);
    const itemsQuery = useMenuItemsQuery(internalShouldLoad == false ? undefined : {
        channelId: channelContext.channelId,
        menuItemCategoryId: category.id,
        atDate: atDate,
        page: 0,
    })

    useEffect(() => setInternalShouldLoad(s => s || shouldLoad), [shouldLoad]);

    return <>
        {
            hideDivider != true &&
            <TextDivider style={{margin: "1rem 0"}}>
                <b>{category.name}</b>
            </TextDivider>
        }
        <Grid container spacing={2} style={{maxWidth: "100%", margin: 0}}>
            {
                internalShouldLoad == false || itemsQuery.isFirstLoading
                ?
                Array(12).fill(0).map((_,i) => i).map((_, i) => 
                    <Grid key={`loading-${i}`} size={{ xs: 12, sm: 12, md: 6 , lg: 6, xl: 6}}>
                        <MenuItemComponent disableQuickCart={orderingFeatures.isActive == false} menuItem={null}/>
                    </Grid>)
                :
                itemsQuery.data.map(item =>
                    <Grid key={item.id} size={{ xs: 12, sm: 12, md: 6 , lg: 6, xl: 6}}>
                        <MenuItemComponent disableQuickCart={orderingFeatures.isActive == false} menuItem={item} onItemSelected={() => onItemSelect(item)}/>
                    </Grid>
                )
            }
        </Grid>
    </>
}