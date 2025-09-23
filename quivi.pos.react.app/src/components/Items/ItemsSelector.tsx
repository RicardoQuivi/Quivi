import React, { useEffect, useMemo, useState } from "react"
import { Box, Breadcrumbs, ButtonBase, Divider, ImageList, ImageListItem, ImageListItemBar, Link, Paper, Skeleton, Typography, useMediaQuery, useTheme } from "@mui/material"
import { useTranslation } from "react-i18next";
import { PaginationFooter } from "../Pagination/PaginationFooter";
import { MenuItem } from "../../hooks/api/Dtos/menuitems/MenuItem";
import { MenuCategory } from "../../hooks/api/Dtos/menucategories/MenuCategory";
import { useMenuCategoriesQuery } from "../../hooks/queries/implementations/useMenuCategoriesQuery";
import { useMenuItemsQuery } from "../../hooks/queries/implementations/useMenuItemsQuery";
import { useGenerateImage } from "../../hooks/useGenerateImage";
import { ItemWithModifiersSelectorModal } from "./ItemWithModifiersSelectorModal";
import { MenuItemWithExtras } from "../../hooks/pos/session/ICartSession";
import { HomeIcon } from "../../icons";

interface Props {
    readonly search: string; 
    readonly onItemSelect: (item: MenuItem | MenuItemWithExtras) => any;
    readonly selectedCategoryId: string | undefined | null;
    readonly onCategoryChanged: (category: MenuCategory | undefined | null) => any;
}
export const ItemsSelector: React.FC<Props> = ({
    search,
    onItemSelect,
    selectedCategoryId,
    onCategoryChanged,
}) => {
    const { t } = useTranslation();

    const selectedCategoryQuery = useMenuCategoriesQuery(selectedCategoryId == undefined ? undefined : {
        ids: [selectedCategoryId],
        hasItems: true,
        page: 0,
    });
    const selectedCategory = useMemo(() => {
        if(selectedCategoryId == undefined) {
            return undefined;
        }

        return selectedCategoryQuery.data.length == 0 ? undefined : selectedCategoryQuery.data[0];
    }, [selectedCategoryId, selectedCategoryQuery.isFirstLoading]);

    const [currentPage, setCurrentPage] = useState<number>(0);
    const [selectedItemWithModifier, setSelectedItemWithModifier] = useState<MenuItem>();

    useEffect(() => setCurrentPage(0), [selectedCategoryId])
    useEffect(() => {
        if (!search?.length) {
            return;
        }
        
        if(selectedCategoryId === undefined) {
            return;
        }

        // Select category "All" each time user search anything
        onCategoryChanged(undefined);
    }, [search]);

    const onItemClicked = (item: MenuItem) => {
        const modifiers = item.modifierGroups;
        if(modifiers.length == 0) {
            onItemSelect(item);
            return;
        }
        setSelectedItemWithModifier(item);
    }

    return (
        <Paper
            elevation={16}
            sx={{
                px: "1rem",
                pb: 0,
                height: "100%",
                display: "flex",
                flexDirection: "column",
                overflow: "hidden",

                '& > *': {
                    marginBottom: 1,
                },

                '& > *:nth-last-of-type(-n+2)': {
                    marginBottom: 0,
                }
            }}
        >

            <Breadcrumbs
                separator={<Typography variant="h6">/</Typography>}
                sx={{
                    flex: 0,
                    paddingY: "1.5rem",
                }}
            >
                <ButtonBase>
                    <Link underline="none" color="inherit" onClick={() => onCategoryChanged(undefined)} sx={{ cursor: "pointer"}}>
                        <Typography
                            variant="h6"
                            sx={{
                                display: "inline-flex",
                                alignItems: "center",
                                gap: 1,
                                fontWeight: selectedCategoryId === undefined ? "bold" : undefined,
                            }}
                        >
                                <HomeIcon />
                                {t("itemsTab.categories")}
                        </Typography>
                    </Link>
                </ButtonBase>
                {
                    selectedCategoryId !== undefined &&
                    <ButtonBase>
                        <Typography
                            variant="h6"
                            sx={{
                                display: "inline-flex",
                                alignItems: "center",
                                gap: 1,
                                fontWeight: "bold",
                            }}
                        >
                        {
                            selectedCategoryId === null
                            ?
                            t("itemsTab.allCategories")
                            :
                            (
                                selectedCategory == undefined
                                ?
                                <Skeleton animation="wave" width="100" />
                                :
                                selectedCategory.name
                            )
                        }
                        </Typography>
                    </ButtonBase>
                }
            </Breadcrumbs>
            {
                selectedCategoryId === undefined
                ?
                <CategorySelector
                    search={search}
                    currentPage={currentPage}
                    onPageChange={setCurrentPage}
                    onCategoryClicked={onCategoryChanged}
                />
                :
                <ItemSelector
                    menuCategoryId={selectedCategory?.id}
                    search={search}
                    onItemClicked={onItemClicked}
                    currentPage={currentPage}
                    onPageChange={setCurrentPage}
                />
            }
            <ItemWithModifiersSelectorModal
                onSelect={onItemSelect}
                item={selectedItemWithModifier}
                onClose={() => setSelectedItemWithModifier(undefined)}
            />
        </Paper>
    )
}


interface CategorySelectorProps {
    readonly search: string;
    readonly currentPage: number;
    readonly onPageChange: (p: number) => any;
    readonly onCategoryClicked: (m: MenuCategory | null) => any;
}
const CategorySelector = (props: CategorySelectorProps) => {
    const { t } = useTranslation();
    const theme = useTheme();
    const xs = useMediaQuery(theme.breakpoints.only('xs'));
    const sm = useMediaQuery(theme.breakpoints.only('sm'));
    const isMobile = useMediaQuery(theme.breakpoints.down('lg'));

    const categoriesQuery = useMenuCategoriesQuery({
        page: props.currentPage,
        pageSize: 64,
    });

    return <>
        <Box
            sx={{
                flex: 1,
                overflow: "auto",
                paddingBottom: "0.5rem",
            }}
        >
            <ImageList
                sx={{
                    mt: 0,
                    mb: 0,
                }}
                cols={xs ? 2 : (sm ? 3 : isMobile ? 4 : 6)}
            >
                <ListItem
                    isMobile={isMobile}
                    name={t("itemsTab.allCategories")}
                    onClick={() => props.onCategoryClicked(null)}
                />
                {
                    categoriesQuery.isFirstLoading
                    ?
                        [1, 2, 3, 4, 5, 6].map(i => <ListItem isMobile={isMobile} key={`Loading-${i}`}/>)
                    :
                        categoriesQuery.data.map((item) => 
                            <ListItem
                                key={item.id}
                                isMobile={isMobile}
                                image={item.imageUrl}
                                name={item.name}
                                onClick={() => props.onCategoryClicked(item)}
                            />
                        )
                }
            </ImageList>
        </Box>
        {
            categoriesQuery.totalPages > 0 && 
            <Box
                sx={{
                    flex: "0 0 auto",
                }}
            >
                <Divider />
                <PaginationFooter currentPage={props.currentPage} numberOfPages={categoriesQuery.totalPages} onPageChanged={props.onPageChange} />
            </Box>
        }
    </>
}

interface ItemSelectorProps {
    readonly menuCategoryId?: string;
    readonly search: string;
    readonly currentPage: number;
    readonly onPageChange: (p: number) => any;
    readonly onItemClicked: (m: MenuItem) => any;
}
const ItemSelector = (props: ItemSelectorProps) => {
    const theme = useTheme();
    const xs = useMediaQuery(theme.breakpoints.only('xs'));
    const sm = useMediaQuery(theme.breakpoints.only('sm'));
    const isMobile = useMediaQuery(theme.breakpoints.down('lg'));

    const menuItemsQuery = useMenuItemsQuery({
        search: props.search,
        menuCategoryId: props.menuCategoryId,
        page: props.currentPage,
        pageSize: 64,
    });

    return <>
        <Box
            sx={{
                flex: 1,
                overflow: "auto",
                paddingBottom: "0.5rem",
            }}
        >
            <ImageList
                sx={{
                    mt: 0,
                    mb: 0,
                }}
                cols={xs ? 2 : (sm ? 3 : isMobile ? 4 : 6)}
            >
                {
                    menuItemsQuery.isFirstLoading
                    ?
                        [1, 2, 3, 4, 5, 6].map(i => <ListItem isMobile={isMobile} key={`Loading-${i}`}/>)
                    :
                        menuItemsQuery.data.map((item) =>
                            <ListItem
                                key={item.id}
                                isMobile={isMobile}
                                image={item.imageUrl}
                                name={item.name}
                                onClick={() => props.onItemClicked(item)}
                            />
                        )
                }
            </ImageList>
        </Box>
        {
            menuItemsQuery.totalPages > 0 && 
            <Box
                sx={{
                    flex: 0,
                }}
            >
                <Divider />
                <PaginationFooter currentPage={props.currentPage} numberOfPages={menuItemsQuery.totalPages} onPageChanged={props.onPageChange} />
            </Box>
        }
    </>
}

const ListItem = (props: {
    readonly image?: string;
    readonly name?: string;
    readonly isMobile: boolean;
    readonly onClick?: () => any;
}) => {
    const width = 400;
    const height = 300;

    const truncateName = (original: string) => original.length > 84 ? `${original.substring(0, 84)}...` : original;

    const generatedImageUrl = useGenerateImage({
        name: props.image == undefined ?  props.name : undefined,
        width: width,
        height: height,
    })

    const imageUrl = useMemo(() => {
        if(props.image == undefined) {
            return generatedImageUrl;
        }
        return props.image;
    }, [props.image, generatedImageUrl])

    return (
        <ButtonBase
            onClick={props.onClick}
        >
            <ImageListItem
                sx={{
                    cursor: "pointer",
                    height: "100%",
                    width: "100%",

                    "& .MuiImageListItemBar-root, & img, & .MuiSkeleton-root": {
                        borderRadius: "0.5rem",
                    },
                }}
            >
                {
                    imageUrl == undefined
                    ?
                    <Box
                        sx={{
                            aspectRatio: "4/3",
                        }}
                    >
                        <Skeleton
                            variant="rounded"
                            width="100%"
                            height="100%"
                            animation="wave"
                        />
                    </Box>
                    :
                    <img
                        src={imageUrl}
                        alt={props.name}
                        loading="lazy"
                        style={{ 
                            aspectRatio: "4/3",
                        }}
                    />
                }
                <ImageListItemBar
                    title={props.name == undefined ? <Skeleton animation="wave" /> : truncateName(props.name)}
                    sx={{
                        height: "100%",
                        userSelect: "none",

                        "& .MuiImageListItemBar-title": {
                            textWrap: "wrap",
                            fontSize: !props.isMobile ? "1rem" : "0.85rem",
                            lineHeight: !props.isMobile ? "24px" : "16px",
                            textAlign: "center",
                        },

                        "& .MuiImageListItemBar-titleWrap": {
                            backgroundColor: "rgba(0,0,0,0.4)",
                        },

                        "& .MuiImageListItemBar-subtitle": {
                            position: "absolute",
                            bottom: "0.5rem",
                            right: "0.5rem",
                        }
                    }}
                />
            </ImageListItem>
        </ButtonBase>
    )
}