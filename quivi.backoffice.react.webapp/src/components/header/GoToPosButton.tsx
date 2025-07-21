import { Trans, useTranslation } from "react-i18next";
import { Placement, Tooltip } from "../ui/tooltip/Tooltip";
import { Modal, ModalSize } from "../ui/modal";
import { useState } from "react";
import { ModalButtonsFooter } from "../ui/modal/ModalButtonsFooter";
import { useAuth, useAuthenticatedUser } from "../../context/AuthContext";

export const GoToPosButton: React.FC = () => {
    const { t } = useTranslation();
    const [isOpen, setIsOpen] = useState(false);
    const auth = useAuth();
    const user = useAuthenticatedUser();

    const open = (keepSession: boolean) => {
        try {
            if(keepSession == false) {
                auth.signOut();
            }

            const queryParams = new URLSearchParams();
            queryParams.set("subjectToken", user.token);
            let url = `${import.meta.env.VITE_POS_APP_URL}signIn?${queryParams}`;
            window.open(url, keepSession ? '_blank' : '_self', 'noopener,noreferrer');
        } finally {
            setIsOpen(false);
        }
    }

    return <>
        <Tooltip message={t("appHeader.pos.openMessage")} placement={Placement.Bottom}>
            <button
                onClick={() => setIsOpen(true)}
                className="relative flex items-center justify-center text-gray-500 transition-colors bg-white border border-gray-200 rounded-full hover:text-dark-900 h-11 hover:bg-gray-100 hover:text-gray-700 dark:border-gray-800 dark:bg-gray-900 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-white px-3"
            >
                {t("appHeader.pos.open")}
            </button>
        </Tooltip>
        <Modal
            isOpen={isOpen}
            onClose={() => setIsOpen(false)} 
            title={t("appHeader.pos.open")}
            footer={(
                <ModalButtonsFooter 
                    primaryButton={{
                        content: t("appHeader.pos.goAndLogout"),
                        onClick: () => open(false),
                    }}
                    secondaryButton={{
                        content: t("appHeader.pos.goAndKeepSession"),
                        onClick: () => open(true),
                    }}
                />
            )}
            size={ModalSize.Medium}
        >
            <p
                className="dark:text-white"
            >
                <Trans
                    t={t}
                    i18nKey="appHeader.pos.openDescription"
                    shouldUnescape={true}
                    components={{
                        b: <b/>,
                        br: <><br/><br/></>,
                    }}
                />
            </p>
        </Modal>
    </>
};
