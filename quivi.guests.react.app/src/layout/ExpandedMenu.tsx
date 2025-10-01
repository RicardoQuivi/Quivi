import { useState } from "react";
import { useTranslation } from 'react-i18next';
import { useQuiviTheme } from "../hooks/theme/useQuiviTheme";
import { useLocation, useNavigate } from "react-router";
import { useAuth } from "../context/AuthContext";
import { CloseIcon, QuiviFullIcon } from "../icons";
import { ButtonsSection } from "./ButtonsSection";
import LoadingButton from "../components/Buttons/LoadingButton";
import Dialog from "../components/Shared/Dialog";
import { Box, Stack } from "@mui/material";

interface Props {
    readonly isOpen: boolean;
    readonly onClose: () => any;
}
export const ExpandedMenu = (props: Props) => {
    const { t } = useTranslation();
    const theme = useQuiviTheme();
    const navigate = useNavigate();

    const location = useLocation();
    const isUserAccount = location.pathname.includes("/user")
    const auth = useAuth();

    const [logoutModalIsOpen, setLogoutModalIsOpen] = useState(false);

    const goHome = () => {
        props.onClose();
        navigate("/");
    }

    const goToLogin = () => {
        props.onClose();
        navigate("/user/login");
    }

    const goToAccount = () => {
        props.onClose();
        navigate("/user/home");
    }

    const goToRegister = () => {
        props.onClose();
        navigate("/user/register");
    }

    const goToSettings = () => {
        props.onClose();
        //appNavigation.goTo(urlBuilder => urlBuilder.profile.SettingsUrl());
    }

    const logOut = () => {
        props.onClose();
        auth.signOut();
        setLogoutModalIsOpen(false);
    }

    return <>
        <Dialog 
            onClose={() => setLogoutModalIsOpen(false)}
            isOpen={logoutModalIsOpen}
        >
            <Stack direction="column" className="container">
                <Box className="modal__header">
                    <h3>{t("expandedMenu.logout")}</h3>
                    <Box className="close-icon" onClick={() => setLogoutModalIsOpen(false)}>
                        <CloseIcon />
                    </Box>
                </Box>
                <p className="mb-5">{t("expandedMenu.logoutConfirmation")}</p>
                <ButtonsSection>
                    <LoadingButton
                        onClick={() => setLogoutModalIsOpen(false)}
                        primaryButton={false}
                    >
                        {t("cancel")}
                    </LoadingButton>
                    <LoadingButton
                        onClick={logOut}
                        primaryButton
                    >
                        {t("expandedMenu.logout")}
                    </LoadingButton>
                </ButtonsSection>
            </Stack>
        </Dialog>
        <div
            className={`expanded-menu ${props.isOpen ? "open" : ""}`}
            style={{
                position: "fixed",
                top: 0,
                bottom: 0,
                left: 0,
                right: 0,
            }}
        >
            <div className="nav">
                <div className="container">
                    <div className="nav__container nav__container--expanded">
                        <button type="button" onClick={goHome} className="nav__logo">
                            <QuiviFullIcon fill={theme.primaryColor.hex} />
                        </button>
                        <button type="button" className="nav__close" onClick={props.onClose}>
                            <CloseIcon height="100%" width="auto" />
                        </button>
                    </div>
                </div>
            </div>
            <div className="expanded-menu__content">
                <div className="container">
                    {
                        auth.user != undefined
                        ?
                        <>
                            {
                                isUserAccount
                                ?
                                    <button type="button" onClick={goToSettings}>{t("expandedMenu.settings")}</button>
                                :
                                    <button type="button" onClick={goToAccount}>{t("expandedMenu.account")}</button>
                            }
                            <button
                                type="button"
                                onClick={() => {
                                    setLogoutModalIsOpen(true);
                                    props.onClose();
                                }}
                            >
                                {t("expandedMenu.logout")}
                            </button>
                        </>
                        :
                        <>
                            <button type="button" onClick={goToLogin}>{t("expandedMenu.login")}</button>
                            <button type="button" onClick={goToRegister}>{t("expandedMenu.register")}</button>
                        </>
                    }
                </div>
            </div>
        </div>
    </>
}