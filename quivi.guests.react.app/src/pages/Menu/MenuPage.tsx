import React, { useCallback, useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { Page } from "../../layout/Page";
import { ButtonsSection } from "../../layout/ButtonsSection";
import { Box, Grid, Stack, Typography } from "@mui/material";
import { makeStyles } from '@mui/styles';
import type { MenuCategory } from "../../hooks/api/Dtos/menuCategories/MenuCategory";
import { PageMode, usePageMode } from "../../hooks/usePageMode";
import { useQuiviTheme } from "../../hooks/theme/useQuiviTheme";
import { useCart } from "../../context/OrderingContextProvider";
import { useMenuCategoriesQuery } from "../../hooks/queries/implementations/useMenuCategoriesQuery";
import { useChannelContext } from "../../context/AppContextProvider";
import type { IItem } from "../../context/cart/item";
import { SquareButton } from "../../components/Buttons/SquareButton";
import { LoadingContainer } from "../../components/LoadingAnimation/LoadingContainer";
import TabOptions from "../../components/Shared/TabOptions";
import { AvatarImage } from "../../components/Avatars/AvatarImage";
import { MenuCategorySection } from "./MenuCategorySection";
import { MenuItemDetailDialog } from "./MenuItemDetailDialog";
import { useNavigate } from "react-router";

const useStyles = makeStyles({
    checkoutBtn: {
        color: "white",
        border: 0,
        padding: "1rem",
        fontSize: "1.2rem",
        fontWeight: 500,
        width: "100%",
    },
    categoriesHeader: {
        overflowY: "hidden",
        position: "sticky",
        top: -1,
        zIndex: 3,
        height: "fit-content",
        maxHeight: "100dvh",

        "&::-webkit-scrollbar": {
            display: "none",
        },

        "& .MuiTabs-root": {
            overflowY: "auto",
            maxHeight: "100dvh",
        },

        "& .MuiTab-root": {
            "& .MuiTypography-root": {
                fontFamily: "unset",
                fontWeight: 500,
                color: "rgba(0, 0, 0, 0.54)",
                letterSpacing: "0.02857em",
            },

            "&.Mui-selected": {
                "& .MuiTypography-root": {
                    fontWeight: "bold",
                    color: "rgb(0, 0, 0)",
                },
            }
        }
    },
    categoriesHeaderPinned: {
        "&.kiosk": {
            "& .MuiPaper-root": {
                padding: "0.5rem 0",
            },
        },
        "&.mobile": {
            height: "64px", //Hardcoded to avoid issue where when on the fix boundary, the element would flicker
            
            "& .MuiPaper-root": {
                padding: "0.5rem 0.75rem",

                position: "fixed",
                top: 0,
                left: 0,
                right: 0,
            },
        }
    },
});

const getSelectedCategory = (sortedCategories: MenuCategory[], map: Map<string, boolean>, pickedCategoryId?: string) => {
    let selectedCategory: MenuCategory | undefined = undefined;
    for(const category of sortedCategories) {
        if(map.get(category.id) == true) {
            if(category.id == pickedCategoryId) {
                selectedCategory = category;
                break;
            }

            if(selectedCategory == undefined) {
                selectedCategory = category;
            }
        }
    }

    return selectedCategory;
}

interface Props {
    readonly atTimestamp?: number,
}
export const MenuPage: React.FC<Props> = ({
    atTimestamp,
}) => {
    const { t } = useTranslation();

    const channelContext = useChannelContext();
    const pageMode = usePageMode();
    const theme = useQuiviTheme();
    const classes = useStyles({ primarycolor: theme.primaryColor});

    const cartSession = useCart();
    
    const navigate = useNavigate();
    const orderingFeatures = channelContext.features.ordering;
    
    const categoriesHeaderRef = useRef<HTMLDivElement>(null);
    const [categoriesHeaderPinned, setCategoriesHeaderPinned] = useState(false);

    const [categorySections] = useState({
        idsMap: new Map<string, HTMLDivElement>(),
        elementsMap: new Map<Element, string>(),
    })

    const [tabSettings, setTabSettings] = useState({
        isScrollingTo: undefined as (HTMLDivElement | undefined),
        pickedCategoryId: undefined as (string | undefined),
        categoriesVisibilityMap: new Map<string, boolean>(),
        currentCategory: undefined as (MenuCategory | undefined),
    });
    const [selectedItem, setSelectedItem] = useState<IItem>();
    const containerRef = useRef<HTMLDivElement>(null);

    const atDate = atTimestamp == undefined ? undefined : new Date(atTimestamp);
    const headerOffset = 50 + (categoriesHeaderPinned ? 10 : 0);

    const categoriesQuery = useMenuCategoriesQuery({
        channelId: channelContext.channelId,
        page: 0,
        atDate: atDate,
    })
    
    const goToAnchor = (t: MenuCategory) => {
        const sectionElem = categorySections.idsMap.get(t.id);
        if (sectionElem == null) {
            return;
        }
        setTabSettings(p => ({
            ...p,
            pickedCategoryId: t.id,
            isScrollingTo: sectionElem,
        }));
    }

    useEffect(() => {
        if(categoriesQuery.isLoading == true) {
            return;
        }

        if(categoriesQuery.data.length != 0) {
            return;
        }

        navigate(`/c/${channelContext.channelId}`, {
            replace: true,
        });
    }, [categoriesQuery.data])

    useEffect(() => {
        if(tabSettings.isScrollingTo == undefined) {
            if(tabSettings.pickedCategoryId == undefined) {
                return;
            }

            const scrollListener = () => {
                setTabSettings(p => ({
                    ...p,
                    isScrollingTo: undefined,
                    pickedCategoryId: undefined,
                }));
                window.removeEventListener("scrollend", scrollListener)
            }
            window.addEventListener("scroll", scrollListener)
            return () => window.removeEventListener("scroll", scrollListener);
        }

        const endScrollListener = () => {
            setTabSettings(p => ({
                ...p,
                isScrollingTo: undefined,
            }));
            window.removeEventListener("scrollend", endScrollListener)
        }
        window.addEventListener("scrollend", endScrollListener)
        window.scrollTo({
            top: tabSettings.isScrollingTo.offsetTop - headerOffset,
            behavior: 'smooth',
        });
        return () => window.removeEventListener("scrollend", endScrollListener);
    }, [tabSettings.isScrollingTo])

    useEffect(() => {
        if(categoriesQuery.isFirstLoading) {
            return;
        }

        if(categoriesQuery.data.length == 0) {
            return;
        }

        goToAnchor(categoriesQuery.data[0])
    }, [categoriesQuery.isFirstLoading, categoriesQuery.data])

    useEffect(() => {
        let selectedCategory = getSelectedCategory(categoriesQuery.data, tabSettings.categoriesVisibilityMap, tabSettings.pickedCategoryId)
        if(selectedCategory == undefined) {
            return;
        }

        setTabSettings(s => ({...s, currentCategory: selectedCategory}));
    }, [tabSettings.pickedCategoryId])

    useEffect(() => {
        if(categoriesHeaderRef.current == null) {
            return;
        }
        const element = categoriesHeaderRef.current;
        const observer = new IntersectionObserver( 
            ([e]) => setCategoriesHeaderPinned(e.intersectionRatio < 1),
            { threshold: [1] }
        );
        
        observer.observe(element);
        return () => observer.unobserve(element);
    }, [categoriesHeaderRef.current])

    useEffect(() => {
        const container = containerRef.current;
        if(container == null) {
            return;
        }

        const observer = new IntersectionObserver((entries) => setTabSettings(p => {
            const map = new Map<string, boolean>(p.categoriesVisibilityMap);
            for(const entry of entries) {
                const categoryId = categorySections.elementsMap.get(entry.target)!
                map.set(categoryId, entry.isIntersecting);
            }
            return {
                ...p,
                categoriesVisibilityMap: map,
                currentCategory: getSelectedCategory(categoriesQuery.data, map, p.pickedCategoryId) ?? p.currentCategory,
            };
        }), {
            threshold: 0,
        });
    
        for(const marker of container.children) {
            observer.observe(marker);
        }
    
        return () => {
            for(const marker of container.children) {
                observer.unobserve(marker);
            }
            observer.disconnect();
        }
    }, [containerRef, containerRef.current, categoriesQuery.data])

    const getFooter = () => {
        if(cartSession.totalItems == 0) {
            return;
        }

        if(orderingFeatures.isActive == false) {
            return;
        }

        return <ButtonsSection transparent>
            {
                orderingFeatures.isActive &&
                <SquareButton className={classes.checkoutBtn} color={theme.primaryColor} showShadow={true} onClick={() => navigate(`/c/${channelContext.channelId}/cart`)}>
                    {t("digitalMenu.checkout", {items: cartSession.totalItems})}
                </SquareButton>
            }
            { undefined }
        </ButtonsSection>
    }

    const renderTab = useCallback((t: MenuCategory) => {
        return <Stack
            sx={{
                width: pageMode == PageMode.Kiosk ? "100%" : undefined,
            }}
            direction="column"
            gap={1}
        >
            {
                pageMode == PageMode.Kiosk &&
                <Box
                    sx={{
                        margin: "0.5rem",
                        width: "100%",
                        "& .MuiAvatar-root": {
                            width: "100%",
                            height: "unset",
                            borderRadius: "5px 0 0 5px ",
                            filter: "drop-shadow(0.35rem 0.35rem 0.4rem rgba(0, 0, 0, 0.5))",
                            aspectRatio: t.imageUrl == undefined ? "1" : undefined,
                        }
                    }}
                >
                    <AvatarImage
                        src={t.imageUrl}
                        name={t.name}
                        style={{aspectRatio: t.imageUrl == undefined ? "1" : undefined}}
                    />
                </Box>
            }
            <Typography noWrap variant="body2">{t.name}</Typography>
        </Stack>
    }, [pageMode]);

    return <Page title="Menu" footer={getFooter()}>
        {
            categoriesQuery.isFirstLoading
            ?
                <LoadingContainer />
            :
            <Grid
                container
                spacing={pageMode == PageMode.Kiosk ? 6 : 0}
                sx={{
                    display: "flex", 
                    flexGrow: 1,
                    flexDirection: pageMode == PageMode.Kiosk ? "row" : "column"
                }}
            >
                <Grid
                    size={{
                        xs: 12,
                        sm: 12,
                        md: 2,
                        lg: 2,
                        xl: 2,
                    }}
                    className={`${classes.categoriesHeader} ${pageMode == PageMode.Kiosk ? "kiosk" : "mobile"} ${categoriesHeaderPinned ? classes.categoriesHeaderPinned : ""}`}
                    ref={categoriesHeaderRef}
                    sx={{
                        paddingTop: pageMode == PageMode.Kiosk ? 0 : undefined,
                        paddingBottom: pageMode == PageMode.Kiosk ? 0 : undefined,
                    }}
                >
                    <TabOptions 
                        orientation={pageMode == PageMode.Kiosk ? "vertical" : "horizontal"}
                        tabs={categoriesQuery.data} 
                        selectedTab={tabSettings.currentCategory} 
                        onTabSelected={goToAnchor}
                        getKey={getCategoryId}
                        getValue={renderTab}
                    />
                </Grid>
                <Grid
                    size={{
                        xs: 12,
                        sm: 12,
                        md: 10,
                        lg: 10,
                        xl: 10,
                    }}
                    ref={containerRef}
                    sx={{
                        flex: 1,
                        flexGrow: 1,

                        '&:last-of-type': {
                            marginBottom: '1.5rem',
                        },
                    }}
                >
                    {
                        categoriesQuery.data.map((c) => (
                            <div
                                key={c.id}
                                ref={el => {
                                    categorySections.idsMap.set(c.id, el!);
                                    categorySections.elementsMap.set(el!, c.id)
                                }}
                            >
                                <MenuCategorySection
                                    category={c}
                                    onItemSelect={(item) => setSelectedItem({...item, imageUrl: item.imageUrl ?? channelContext.logoUrl})}
                                    atTimestamp={atTimestamp}
                                    shouldLoad={tabSettings.categoriesVisibilityMap.get(c.id) == true}
                                />
                            </div>
                        ))
                    }
                </Grid>
            </Grid>
        }
        <MenuItemDetailDialog menuItem={selectedItem ?? null} onClose={() => setSelectedItem(undefined)} />
    </Page>
}

const getCategoryId = (t: MenuCategory) => t.id;