import { alpha, AppBar,  Chip, IconButton, InputBase, ListItemIcon, ListItemText, Menu, MenuItem, styled, Toolbar, Tooltip, useMediaQuery, useTheme } from "@mui/material";
import { useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useLocalsQuery } from "../hooks/queries/implementations/useLocalsQuery";
import useDebouncedState from "../hooks/useDebouncedState";
import { EmployeeAvatar } from "./Avatars/EmployeeAvatar";
import { MenuIcon, SearchIcon, SwapIcon } from "../icons";
import { ChangeLocalModal } from "./Locals/ChangeLocalModal";
import { usePosSession } from "../context/pos/PosSessionContextProvider";

const Search = styled('div')(({ theme }) => ({
    position: 'relative',
    borderRadius: theme.shape.borderRadius,
    backgroundColor: alpha(theme.palette.common.white, 0.15),
    margin: 0,
    width: '100%',
    
    '&:hover': {
        backgroundColor: alpha(theme.palette.common.white, 0.25),
    },

    [theme.breakpoints.up('sm')]: {
        marginLeft: theme.spacing(3),
        width: 'auto',
    },
}));
const SearchIconWrapper = styled('div')(({ theme }) => ({
    padding: theme.spacing(0, 2),
    height: '100%',
    position: 'absolute',
    pointerEvents: 'none',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
}));

interface Props {
    readonly search: string;
    readonly onSearchChanged: (s: string) => any;
    readonly onNotificationClicked: () => any;
    readonly localId?: string;
    readonly onNewLocalSelected: (l: string | undefined) => any;
    readonly onReadQrCodeButtonClicked: () => any;
}
export const PosAppBar = (props: Props) => {
    const { t } = useTranslation();
    const theme = useTheme();
    const xs = useMediaQuery(theme.breakpoints.only('xs'));
    const posContext = usePosSession();
    const locationsQuery = useLocalsQuery({})

    const [searchTxt, setSearchTxt, debouncedSearchText, setDebouncedSearchText] = useDebouncedState<string>("", 500);
    const [state, setState] = useState({
        isExitingPoSMode: false,
        localModalOpen: false,
        menuAnchor: undefined as (HTMLElement | undefined),
    })

    useEffect(() => props.onSearchChanged(debouncedSearchText), [debouncedSearchText])
    useEffect(() => setDebouncedSearchText(props.search), [props.search])

    const local = useMemo(() => props.localId == undefined ? undefined : locationsQuery.data.find(l => l.id == props.localId), [props.localId, locationsQuery.data])
    
    const renderOptionsDesktop = () => {
        return <>
            <Tooltip title={posContext.employee.name}>
                <IconButton size="large" edge="start" onClick={posContext.signOut} sx={{ margin: "0 0.25rem", marginLeft: 0, paddingLeft: 0, }}>
                    <EmployeeAvatar employee={posContext.employee} />
                </IconButton>
            </Tooltip>
            {
                locationsQuery.isFirstLoading == false &&
                locationsQuery.data.length > 0 &&
                <Tooltip
                    title={t("changeLocal")}
                    sx={{
                        backgroundColor: "transparent",
                    }}
                >
                    <Chip
                        avatar={(<SwapIcon />)} 
                        label={local?.name ?? t("allLocals")}
                        variant="filled"
                        color="default"
                        sx={{
                            color: t => t.typography.button,
                            height: 40,
                            cursor: "pointer",
                            margin: "0 0.25rem",

                            "& svg": {
                                marginLeft: 5,
                                cursor: "pointer",
                                height: "100%",
                                color: t => t.typography.button,
                            }
                        }}
                        onClick={() => setState(s => ({...s, localModalOpen: true, }))}
                    />
                </Tooltip>
            }
        </>
    }

    const renderOptionsMobile = () => {
        return <>
            <IconButton size="large" color="inherit" sx={{paddingLeft: 0}} onClick={(e) => setState(s => ({...s, menuAnchor: e.currentTarget}))}>
                <MenuIcon style={{paddingRight: "0.25rem", height: 24, width: "auto", color: "white"}}/>
            </IconButton>
            <Menu
                anchorEl={state.menuAnchor}
                open={state.menuAnchor != undefined}
                onClose={() => setState(s => ({...s, menuAnchor: undefined}))}
            >
                <MenuItem
                    onClick={async () => {
                        await posContext.signOut();
                        setState(s => ({...s, menuAnchor: undefined}))
                    }}
                    sx={{
                        gap: 1,
                    }}
                >
                    <ListItemIcon>
                        <EmployeeAvatar employee={posContext.employee} />
                    </ListItemIcon>
                    <ListItemText sx={{marginLeft: "0.25rem"}}>
                        {posContext.employee!.name}
                    </ListItemText>
                </MenuItem>
                {
                    locationsQuery.isFirstLoading == false &&
                    locationsQuery.data.length > 0 &&
                    <MenuItem onClick={() => setState(s => ({...s, localModalOpen: true, menuAnchor: undefined}))}>
                        <ListItemIcon
                            sx={{
                                "& svg": {
                                    cursor: "pointer",
                                    height: "100%",
                                }
                            }}
                        >
                            <SwapIcon />
                        </ListItemIcon>
                        <ListItemText sx={{marginLeft: "0.25rem"}}>
                            {local?.name ?? t("allLocals")}
                        </ListItemText>
                    </MenuItem>
                }
                {/* <MenuItem 
                    onClick={() => {
                        props.onReadQrCodeButtonClicked(); 
                        setState(s => ({...s, menuAnchor: undefined}))
                    }}
                >
                    <ListItemIcon>
                        <QrCodeIcon />
                    </ListItemIcon>
                    <ListItemText sx={{marginLeft: "0.25rem"}}>
                        {t("verify")}
                    </ListItemText>
                </MenuItem> */}
            </Menu>
        </>
    }

    const renderOptions = () =>  xs ? renderOptionsMobile() : renderOptionsDesktop();
    
    return <>
        <AppBar position="relative">
            <Toolbar variant="regular">
                { renderOptions() }
                <Search
                    sx={{
                        flex: 1,
                    }}
                >
                    <SearchIconWrapper>
                        <SearchIcon />
                    </SearchIconWrapper>
                    <InputBase
                        placeholder={t("search")!}
                        value={searchTxt}
                        onChange={(v) => setSearchTxt(v.target.value)}
                        sx={{
                            color: 'inherit',
                            width: "100%",
                            
                            '& .MuiInputBase-input': {
                                padding: theme.spacing(1, 1, 1, 0),

                                // vertical padding + font size from searchIcon
                                paddingLeft: `calc(1em + ${theme.spacing(4)})`,
                                transition: theme.transitions.create('width'),
                                width: '100%',

                                [theme.breakpoints.up('md')]: {
                                    width: '20ch',
                                },
                            },
                        }}
                    />
                </Search>
            </Toolbar>
        </AppBar>
        <ChangeLocalModal
            isOpen={state.localModalOpen}
            selectedLocal={local}
            onClose={l => {
                setState(s => ({...s, localModalOpen: false}))
                props.onNewLocalSelected(l?.id);
            }}
        />
    </>
}