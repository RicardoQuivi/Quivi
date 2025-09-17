import { useState } from "react";
import { useTranslation } from 'react-i18next';
import { useQuiviTheme } from "../hooks/theme/useQuiviTheme";
import { useLocation, useNavigate } from "react-router";
import { useAuth } from "../context/AuthContext";
import { CloseIcon, QuiviFullIcon } from "../icons";
import { Modal } from "../components/Shared/Modal";

//TODO: Add navigations bellow
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
        //appNavigation.goTo(urlBuilder => urlBuilder.auth.LoginUrl());
    }

    const goToAccount = () => {
        props.onClose();
        //appNavigation.goTo(urlBuilder => urlBuilder.profile.ProfileUrl());
    }

    const goToRegister = () => {
        props.onClose();
        //appNavigation.goTo(urlBuilder => urlBuilder.auth.RegisterUrl());
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
        <Modal 
            onClose={() => setLogoutModalIsOpen(false)}
            isOpen={logoutModalIsOpen}
        >
            <button type="button" className="secondary-button mb-4" onClick={logOut}>{t("expandedMenu.logout")}</button>
            <button type="button" className="clear-button" onClick={() => setLogoutModalIsOpen(false)}>{t("cancel")}</button>
        </Modal>
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
                            <CloseIcon />
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
                            <button type="button" onClick={() => setLogoutModalIsOpen(true)}>{t("expandedMenu.logout")}</button>
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