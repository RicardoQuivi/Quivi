import { useTranslation } from "react-i18next";
import { CountryIcon } from "../../icons/CountryIcon";
import { Modal, ModalSize } from "../ui/modal/Modal";
import { useState } from "react";

const languages = [
    {
        id: "en",
        name: "English",
    },
    {
        id: "pt",
        name: "Português",
    }
]

export const LanguageButton: React.FC = () => {
    const { t, i18n } = useTranslation();
    const [isOpen, setIsOpen] = useState(false);

    return <>
        <button
            onClick={() => setIsOpen(true)}
            className="relative flex items-center justify-center text-gray-500 transition-colors bg-white border border-gray-200 rounded-full hover:text-dark-900 h-11 w-11 hover:bg-gray-100 hover:text-gray-700 dark:border-gray-800 dark:bg-gray-900 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-white"
        >
            <CountryIcon
                language={i18n.language}
                className="w-auto h-full rounded-full"
            />
        </button>
        <Modal
            isOpen={isOpen}
            onClose={() => setIsOpen(false)}
            size={ModalSize.Small}
            title={t("appHeader.changeLanguage")}
        >
            <div className="grid grid-cols-1 md:grid-cols-2 gap-2 mb-8">
                {
                    languages.map((l) => (
                        <div
                            key={l.id}
                            onClick={() => {
                                i18n.changeLanguage(l.id);
                                setIsOpen(false);
                            }}
                            className="flex items-center cursor-pointer"
                        >
                            <CountryIcon
                                language={l.id}
                                className="w-16 h-12 rounded-full"
                            />
                            <div className="ml-3 flex items-center">
                                <p className="dark:text-white">{l.name}</p>
                            </div>
                        </div>
                    ))
                }
            </div>
        </Modal>
    </>;
};

export const FloatingLanguageButton = () => {
    const { t, i18n } = useTranslation();
    const [isOpen, setIsOpen] = useState(false);

    return <>
        <button
            onClick={() => setIsOpen(true)}
            className="relative flex items-center justify-center text-gray-500 transition-colors border border-gray-200 rounded-full hover:text-dark-900 size-14 hover:border-gray-100 hover:text-gray-700 dark:border-gray-800 dark:bg-gray-900 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-white"
        >
            <CountryIcon
                language={i18n.language}
                className="w-auto h-full rounded-full"
            />
        </button>
        <Modal
            isOpen={isOpen}
            onClose={() => setIsOpen(false)}
            size={ModalSize.Small}
            title={t("appHeader.changeLanguage")}
        >
            <div className="grid grid-cols-1 md:grid-cols-2 gap-2 mb-8">
                {
                    languages.map((l) => (
                        <div
                            key={l.id}
                            onClick={() => {
                                i18n.changeLanguage(l.id);
                                setIsOpen(false);
                            }}
                            className="flex items-center cursor-pointer"
                        >
                            <CountryIcon
                                language={l.id}
                                className="w-16 h-12 rounded-full"
                            />
                            <div className="ml-3 flex items-center">
                                <p className="dark:text-white">{l.name}</p>
                            </div>
                        </div>
                    ))
                }
            </div>
        </Modal>
    </>;
}
