import { useCallback, useEffect, useRef, useState } from "react";
import { Link, useLocation } from "react-router";
import { useSidebar } from "../context/SidebarContext";
import { ChevronDownIcon, GearIcon, GridIcon, HorizontaLDots, ListIcon, QuiviFullIcon, QuiviIcon, UserIcon } from "../icons";
import { useTranslation } from "react-i18next";
import { useAuthenticatedUser } from "../context/AuthContext";

interface User {
    readonly isAdmin: boolean;
    readonly merchantId?: string;
    readonly subMerchantId?: string;
    readonly merchantActivated: boolean;
}

interface NavItem {
    readonly name: string;
    readonly icon: React.ReactNode;
    readonly path?: string;
    readonly show: (user: User) => boolean;
    readonly subItems?: { 
        readonly name: string; 
        readonly path: string;
        readonly show: (user: User) => boolean;
    }[];
};

const items: NavItem[] = [
    {
        icon: <GridIcon />,
        name: "sidebar.home",
        path: "/",
        show: () => true,
    },
    {
        icon: <UserIcon />,
        name: "sidebar.administration.",
        show: (u) => u.isAdmin,
        subItems: [
            { 
                name: "sidebar.administration.integrations",
                path: "/admin/integrations",
                show: (u) => u.isAdmin && u.subMerchantId != undefined,
            },
            { 
                name: "sidebar.administration.acquirerConfigurations",
                path: "/admin/acquirerConfigurations",
                show: (u) => u.isAdmin && u.subMerchantId != undefined,
            },
            { 
                name: "sidebar.administration.transactions",
                path: "/admin/transactions",
                show: (u) => u.isAdmin,
            },
        ],
    },
    {
        icon: <></>,
        name: "sidebar.businessProfile.",
        show: (u) => u.merchantActivated == true,
        subItems: [
            { 
                name: "sidebar.businessProfile.basicInfo",
                path: "/businessProfile/merchant",
                show: (u) => u.merchantActivated == true && u.subMerchantId != undefined,
            },
            { 
                name: "sidebar.businessProfile.menuManagement",
                path: "/businessProfile/menuManagement",
                show: (u) => u.merchantActivated == true && u.subMerchantId != undefined,
            },
            { 
                name: "sidebar.businessProfile.channelProfiles",
                path: "/businessProfile/channels/profiles",
                show: (u) => u.merchantActivated == true && u.subMerchantId != undefined,
            },
            { 
                name: "sidebar.businessProfile.channels",
                path: "/businessProfile/channels",
                show: (u) => u.merchantActivated == true && u.subMerchantId != undefined,
            },
        ],
    },
    {
        icon: <></>,
        name: "sidebar.transactions",
        path: "/transactions",
        show: (u) => u.merchantActivated == true,
    },
    {
        icon: <></>,
        name: "sidebar.payouts",
        path: "/payouts",
        show: (u) => u.merchantActivated == true,
    },
    {
        icon: <ListIcon />,
        name: "sidebar.invoicing",
        path: "/invoicing",
        show: (u) => u.merchantActivated == true,
    },
    {
        icon: <></>,
        name: "sidebar.reports.",
        show: (u) => u.merchantActivated == true,
        subItems: [
            { 
                name: "sidebar.reports.dailySales",
                path: "/reports/dailySales",
                show: (u) => u.merchantActivated == true,
            },
        ],
    },
    {
        icon: <GearIcon />,
        name: "sidebar.settings.",
        show: (u) => u.merchantActivated == true,
        subItems: [
            { 
                name: "sidebar.settings.locals",
                path: "/settings/locals",
                show: (u) => u.merchantActivated == true && u.subMerchantId != undefined,
            },
            { 
                name: "sidebar.settings.chargeMethods",
                path: "/settings/chargemethods",
                show: (u) => u.merchantActivated == true && u.subMerchantId != undefined,
            },
            { 
                name: "sidebar.settings.employees",
                path: "/settings/employees",
                show: (u) => u.merchantActivated == true && u.subMerchantId != undefined,
            },
            { 
                name: "sidebar.settings.printers",
                path: "/settings/printersmanagement",
                show: (u) => u.merchantActivated == true && u.subMerchantId != undefined,
            },
        ],
    },
]

const AppSidebar = () => {
    const { t } = useTranslation();
    const user = useAuthenticatedUser();

    const { isExpanded, isMobileOpen, isHovered, setIsHovered } = useSidebar();
    const location = useLocation();

    const [openSubmenu, setOpenSubmenu] = useState<{
        index: number;
    } | null>(null);
    const [subMenuHeight, setSubMenuHeight] = useState<Record<string, number>>({});
    const subMenuRefs = useRef<Record<string, HTMLDivElement | null>>({});

    const isActive = useCallback((path: string) => location.pathname.startsWith(path), [location.pathname]);

    useEffect(() => {
        let submenuMatched = false;
        items.forEach((nav, index) => {
            if (nav.subItems) {
                nav.subItems.forEach((subItem) => {
                    if (isActive(subItem.path)) {
                        setOpenSubmenu({
                            index,
                        });
                        submenuMatched = true;
                    }
                });
            }
        });

        if (!submenuMatched) {
            setOpenSubmenu(null);
        }
    }, [location, isActive]);

    useEffect(() => {
        if (openSubmenu !== null) {
            const key = `${openSubmenu.index}`;
            if (subMenuRefs.current[key]) {
                setSubMenuHeight((prevHeights) => ({
                    ...prevHeights,
                    [key]: subMenuRefs.current[key]?.scrollHeight || 0,
                }));
            }
        }
    }, [openSubmenu]);

    const handleSubmenuToggle = (index: number) => {
        setOpenSubmenu((prevOpenSubmenu) => {
            if (
                prevOpenSubmenu &&
                prevOpenSubmenu.index === index
            ) {
                return null;
            }
            return { index };
        });
    };

    const renderMenuItems = (items: NavItem[]) => (
        <ul className="flex flex-col gap-4">
            {
                items.filter(i => i.show(user) == true).map((nav, index) => {
                    const subItems = nav.subItems?.filter(s => s.show(user));
                    return <li key={nav.name}>
                        {
                            subItems 
                            ?
                            <button
                                onClick={() => handleSubmenuToggle(index)}
                                className={`menu-item group ${openSubmenu?.index === index
                                        ? "menu-item-active"
                                        : "menu-item-inactive"
                                    } cursor-pointer ${!isExpanded && !isHovered
                                        ? "lg:justify-center"
                                        : "lg:justify-start"
                                    }`}
                            >
                                <span
                                    className={`menu-item-icon-size ${openSubmenu?.index === index
                                            ? "menu-item-icon-active"
                                            : "menu-item-icon-inactive"
                                        }`}
                                >
                                    {nav.icon}
                                </span>
                                {(isExpanded || isHovered || isMobileOpen) && (
                                    <span className="menu-item-text">{t(nav.name)}</span>
                                )}
                                {(isExpanded || isHovered || isMobileOpen) && (
                                    <ChevronDownIcon
                                        className={`ml-auto w-5 h-5 transition-transform duration-200 ${openSubmenu?.index === index
                                                ? "rotate-180 text-brand-500"
                                                : ""
                                            }`}
                                    />
                                )}
                            </button>
                            : 
                            (
                                nav.path && 
                                <Link
                                    to={nav.path}
                                    className={`menu-item group ${isActive(nav.path) ? "menu-item-active" : "menu-item-inactive"
                                        }`}
                                >
                                    <span
                                        className={`menu-item-icon-size ${isActive(nav.path)
                                                ? "menu-item-icon-active"
                                                : "menu-item-icon-inactive"
                                            }`}
                                    >
                                        {nav.icon}
                                    </span>
                                    {(isExpanded || isHovered || isMobileOpen) && (
                                        <span className="menu-item-text">{t(nav.name)}</span>
                                    )}
                                </Link>
                            )
                        }

                        {
                            (subItems && (isExpanded || isHovered || isMobileOpen)) && 
                            <div
                                ref={(el) => {
                                    subMenuRefs.current[`${index}`] = el;
                                }}
                                className="overflow-hidden transition-all duration-300"
                                style={{
                                    height:
                                        openSubmenu?.index === index
                                            ? `${subMenuHeight[`${index}`]}px`
                                            : "0px",
                                }}
                            >
                                <ul className="mt-2 space-y-1 ml-9">
                                    {
                                        subItems.map((subItem) => (
                                            <li key={subItem.name}>
                                                <Link
                                                    to={subItem.path}
                                                    className={`menu-dropdown-item ${isActive(subItem.path)
                                                            ? "menu-dropdown-item-active"
                                                            : "menu-dropdown-item-inactive"
                                                        }`}
                                                >
                                                    {t(subItem.name)}
                                                    <span className="flex items-center gap-1 ml-auto">
                                                    </span>
                                                </Link>
                                            </li>
                                        ))
                                    }
                                </ul>
                            </div>
                        }
                    </li>
                })}
        </ul>
    );

    return (
        <aside
            className={`fixed mt-16 flex flex-col lg:mt-0 top-0 px-5 left-0 bg-white dark:bg-gray-900 dark:border-gray-800 text-gray-900 h-screen transition-all duration-300 ease-in-out z-50 border-r border-gray-200 
                    ${isExpanded || isMobileOpen
                    ?
                        "w-[290px]"
                    : 
                        isHovered
                        ?
                            "w-[290px]"
                        : 
                            "w-[90px]"
                    }
                    ${isMobileOpen ? "translate-x-0" : "-translate-x-full"} lg:translate-x-0`}
            onMouseEnter={() => !isExpanded && setIsHovered(true)}
            onMouseLeave={() => setIsHovered(false)}
        >
            <div
                className={`py-2 flex ${!isExpanded && !isHovered ? "lg:justify-center" : "justify-start"}`}
            >
                <Link to="/" className="w-full flex justify-center">
                {
                    isExpanded || isHovered || isMobileOpen 
                    ?
                    <QuiviFullIcon width="70%" className="h-auto" />
                    :
                    <QuiviIcon width="100%" className="h-auto" />
                }
                </Link>
            </div>
            <div className="flex flex-col overflow-y-auto duration-300 ease-linear no-scrollbar">
                <nav className="mb-6">
                    <div className="flex flex-col gap-4">
                        <>
                            <h2
                                className={`mb-4 text-xs uppercase flex leading-[20px] text-gray-400 ${!isExpanded && !isHovered
                                        ? "lg:justify-center"
                                        : "justify-start"
                                    }`}
                            >
                                {
                                    isExpanded || isHovered || isMobileOpen 
                                    ?
                                        "Menu"
                                    :
                                        <HorizontaLDots className="size-6" />
                                }
                            </h2>
                            {renderMenuItems(items)}
                        </>
                    </div>
                </nav>
            </div>
        </aside>
    );
};
export default AppSidebar;