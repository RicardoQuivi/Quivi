import { useAppContext } from "../../context/AppContextProvider";
import { useQuiviTheme } from "../../hooks/theme/useQuiviTheme";
import { Link, useNavigate, useNavigationType } from "react-router";
import { ArrowLeftIcon, HomeIcon } from "../../icons";
import { NavActions } from "./NavActions";
import { useMemo } from "react";
import { Box } from "@mui/material";

interface Props {
    readonly title?: string;
    readonly hideCart?: boolean;
    readonly hideOrder?: boolean;
    readonly hideFlag?: boolean;
}
export const TableNav: React.FC<Props> = ({
    title,
    hideCart,
    hideOrder,
    hideFlag,
}) => {
    const theme = useQuiviTheme();
    const navigate = useNavigate();
    const appContext = useAppContext();
    const navType = useNavigationType();

    const canGoBack = useMemo(() => navType === "PUSH", [navType])

    return (
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
                            className="page-title__content"
                            onClick={() => {
                                if(canGoBack) {
                                    navigate(-1);
                                    return;
                                }
                                navigate(!appContext?.channelId ? "/user/home" : `/c/${appContext.channelId}`);
                            }} 
                            sx={{
                                cursor: canGoBack ? "pointer" : undefined,
                            }}
                        >
                            <Box className="nav__menu">
                                <ArrowLeftIcon width="60%" height="60%" />
                            </Box>
                            <h2>{title}</h2>
                        </Box>
                    :
                    <Link to={!appContext?.channelId ? "/user/home" : `/c/${appContext.channelId}`}>
                        <Box className="nav__menu nav__menu--not-auth">
                            <HomeIcon fill={theme.primaryColor.hex} width="60%" height="60%" />
                        </Box>
                    </Link>
                }
                <NavActions
                    hideCart={hideCart}
                    hideOrder={hideOrder}
                    hideFlag={hideFlag}
                />
            </Box>
        </Box>
    );
}