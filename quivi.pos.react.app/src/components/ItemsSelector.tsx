import React, { useEffect, useMemo, useState } from "react"
import { Box, ButtonBase, Divider, ImageList, ImageListItem, ImageListItemBar, Paper, Skeleton, Tab, Tabs, styled, useMediaQuery, useTheme } from "@mui/material"
import { useTranslation } from "react-i18next";
import { PaginationFooter } from "./Pagination/PaginationFooter";
import { MenuItem } from "../hooks/api/Dtos/menuitems/MenuItem";
import { MenuCategory } from "../hooks/api/Dtos/menucategories/MenuCategory";
import { useMenuCategoriesQuery } from "../hooks/queries/implementations/useMenuCategoriesQuery";
import { useMenuItemsQuery } from "../hooks/queries/implementations/useMenuItemsQuery";
import { useGenerateImage } from "../hooks/useGenerateImage";

const ListItem = (props: {
    readonly image?: string;
    readonly name?: string;
    readonly price?: number;
    readonly isMobile: boolean;
    readonly onClick?: () => any;
}) => {
    const truncateName = (original: string) => original.length > 84 ? `${original.substring(0, 84)}...` : original;

    const generatedImageUrl = useGenerateImage({
        name: props.image == undefined ?  props.name : undefined,
        width: 400,
        height: 300,
    })

    const imageUrl = useMemo(() => {
        if(props.image == undefined) {
            return generatedImageUrl;
        }
        return props.image.replace("_full.", "_thumbnail.");
    }, [props.image, generatedImageUrl])

    return (
        <ButtonBase onClick={props.onClick}>
            <ImageListItem
                sx={{
                    cursor: "pointer",
                    height: "100%",
                    width: "100%",
                }}
            >
                {
                    imageUrl == undefined
                    ?
                    <Skeleton variant="rounded" height={200} animation="wave"/>
                    :
                    <img src={imageUrl} alt={props.name} loading="lazy" style={{ aspectRatio: "4/3"}}/>
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

interface Props {
    readonly search: string; 
    readonly onItemSelect: (item: MenuItem) => any;
    readonly selectedCategoryId?: string;
    readonly onCategoryChanged: (category: MenuCategory | undefined) => any;
}
export const ItemsSelector: React.FC<Props> = ({
    search,
    onItemSelect,
    selectedCategoryId,
    onCategoryChanged,
}) => {
    const { t } = useTranslation();

    const theme = useTheme();
    const xs = useMediaQuery(theme.breakpoints.only('xs'));
    const sm = useMediaQuery(theme.breakpoints.only('sm'));
    const isMobile = useMediaQuery(theme.breakpoints.down('lg'));

    const categoriesQuery = useMenuCategoriesQuery({
        hasItems: true,
        page: 0,
    });

    const categoriesMap = useMemo(() => categoriesQuery.data.reduce((r, c) => {
        r.set(c.id, c);
        return r;
    }, new Map<string, MenuCategory>()), [categoriesQuery.data])

    const [currentPage, setCurrentPage] = useState<number>(0);
    const [_selectedItemWithModifier, setSelectedItemWithModifier] = useState<MenuItem>();

    const selectedCategory = useMemo(() => {
        if(selectedCategoryId == undefined) {
            return undefined;
        }

        return categoriesMap.get(selectedCategoryId);
    }, [selectedCategoryId, categoriesQuery.isFirstLoading]);

    const menuItemsQuery = useMenuItemsQuery({
        search: search,
        menuCategoryId: selectedCategory == undefined ? undefined : selectedCategory.id,
        page: currentPage,
        pageSize: 64,
    });

    useEffect(() => setCurrentPage(0), [selectedCategory])    
    useEffect(() => {
        if (categoriesQuery.isFirstLoading || !categoriesQuery.data.length || !search?.length) {
            return;
        }
        
        // Select category "All" each time user search anything
        onCategoryChanged(undefined);
    }, [search]);

    const onCategorySelected = (_: React.SyntheticEvent, newValue: string) => {
        if(categoriesQuery.isFirstLoading) {
            return;
        }

        const newSelectedCategory = !!newValue ? categoriesMap.get(newValue) : undefined;
        onCategoryChanged(newSelectedCategory);
    }

    const onItemClicked = (item: MenuItem) => {
        const modifiers = item.modifierGroups ?? [];
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
            <Tabs 
                variant="scrollable" 
                allowScrollButtonsMobile 
                scrollButtons="auto"
                value={categoriesQuery.isFirstLoading ? false : selectedCategory?.id ?? ""} 
                onChange={onCategorySelected}
                sx={{
                    flex: "0 0 auto",

                    paddingTop: 0,
                    paddingBottom: 0,
                    minHeight: {
                        xs: 35,
                        sm: 48,
                    },

                    "& .MuiTab-root": {
                        minHeight: {
                            xs: 35,
                            sm: 48,
                        },
                        paddingTop: {
                            xs: "0.25rem",
                            sm: "0.5rem",
                        },
                        paddingBottom: {
                            xs: "0.25rem",
                            sm: "0.5rem",
                        },
                    },

                    "& > .MuiTabScrollButton-horizontal:first-of-type": {
                        marginLeft: "-1rem",
                        "&.Mui-disabled": {
                            marginLeft: "-3rem",
                        },
                    },

                    "& > .MuiTabScrollButton-horizontal:last-of-type": {
                        marginRight: "-1rem",
                        "&.Mui-disabled": {
                            marginRight: "-3rem",
                        },
                    },
                }}
                
            >
            {
                categoriesQuery.isFirstLoading
                ?
                [1, 2, 3, 4, 5].map(i => <Tab
                                            label={<Skeleton animation="wave" width="100%" />}
                                            value={i == 0 ? "" : `Loading-${i}`}
                                            key={`Loading-${i}`}
                                        />)
                :
                [
                    <Tab label={t("all")} value="" key="All" />,
                    ...categoriesQuery.data.map(a => <Tab label={a.name} value={a.id} key={a.id} />), 
                ]
            }
            </Tabs>

            <Box
                sx={{
                    flex: "1 1 auto",
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
                                    price={item.price}
                                    onClick={() => onItemClicked(item)}
                                />
                            )
                    }
                </ImageList>
            </Box>
            {
                menuItemsQuery.totalPages > 0 && 
                <Box
                    sx={{
                        flex: "0 0 auto",
                    }}
                >
                    <Divider />
                    <PaginationFooter currentPage={currentPage} numberOfPages={menuItemsQuery.totalPages} onPageChanged={setCurrentPage} />
                </Box>
            }
            {/* <ItemWithModifiersSelectorModal onSelect={onItemSelect} item={selectedItemWithModifier} onClose={() => setSelectedItemWithModifier(undefined)} /> */}
        </Paper>
    )
}