import { useAppContext } from "../../context/AppContextProvider";
import { useQuiviTheme } from "../../hooks/theme/useQuiviTheme";
import { Link, useNavigate, useNavigationType } from "react-router";
import { ArrowLeftIcon, HomeIcon } from "../../icons";
import { NavActions, type NavActionsOrderingProps } from "./NavActions";
import { useMemo } from "react";
import { Box, Divider, Stack, Typography } from "@mui/material";

interface Props {
    readonly title?: string;
    readonly ordering: NavActionsOrderingProps | false;
    readonly hideFlag?: boolean;
}
export const TableNav: React.FC<Props> = ({
    title,
    ordering,
    hideFlag,
}) => {
    const theme = useQuiviTheme();
    const navigate = useNavigate();
    const appContext = useAppContext();
    const navType = useNavigationType();

    const canGoBack = useMemo(() => navType === "PUSH", [navType])

    return <Stack
        direction="column"
        alignItems="center"
    >
        <Box
            className="container"
            sx={{
                height: "fit-content",
            }}
        >
            <Box className="page-title">
                {
                    title
                    ?
                        <Box
                            onClick={() => {
                                if(canGoBack) {
                                    navigate(-1);
                                    return;
                                }
                                navigate(!appContext?.channelId ? "/user/home" : `/c/${appContext.channelId}`);
                            }} 
                            sx={{
                                cursor: canGoBack ? "pointer" : undefined,
                                display: "flex",
                                alignItems: "center",
                            }}
                        >
                            <Box className="nav__menu">
                                <ArrowLeftIcon width="auto" height="50%" />
                            </Box>
                            <Typography variant="h6" component="h2" fontWeight="bold">{title}</Typography>
                        </Box>
                    :
                    <Link to={!appContext?.channelId ? "/user/home" : `/c/${appContext.channelId}`}>
                        <Box className="nav__menu nav__menu--not-auth">
                            <HomeIcon fill={theme.primaryColor.hex} width="60%" height="60%" />
                        </Box>
                    </Link>
                }
                {
                    !!appContext?.channelId &&
                    <NavActions
                        ordering={ordering}
                        hideFlag={hideFlag}
                    />
                }
            </Box>
        </Box>
        <Divider
            sx={{
                width: "90%",
            }}
        />
    </Stack>
}