import { useTranslation } from "react-i18next";
import { Language } from "../../../hooks/api/Dtos/Language";
import { useMemo } from "react";

interface Props {
    readonly language: Language;
    readonly onChange?: (value: Language) => any;
}
export const LanguageSelector = (props: Props) => {
    const { t } = useTranslation();

    const languages = useMemo(() => [
        {
            key: Language.Portuguese,
            name: "Português",
            onClick: () => props.onChange?.(Language.Portuguese),
        },
        {
            key: Language.English,
            name: "English",
            onClick: () => props.onChange?.(Language.English),
        }
    ], [t, props.onChange]);

    return (
        <nav className="flex overflow-x-auto rounded-lg bg-gray-100 p-1 dark:bg-gray-900 [&::-webkit-scrollbar]:h-1.5 [&::-webkit-scrollbar-track]:bg-white dark:[&::-webkit-scrollbar-track]:bg-transparent [&::-webkit-scrollbar-thumb]:rounded-full [&::-webkit-scrollbar-thumb]:bg-gray-200 dark:[&::-webkit-scrollbar-thumb]:bg-gray-600">
            {
                languages.map((l) => (
                    <button
                        key={l.key}
                        onClick={l.onClick}
                        className={`inline-flex items-center rounded-md px-3 py-2 text-sm font-medium transition-colors duration-200 ease-in-out ${
                            props.language === l.key
                            ? "bg-white text-gray-900 shadow-theme-xs dark:bg-white/[0.03] dark:text-white"
                            : "bg-transparent text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
                        }`}
                    >
                        {l.name}
                    </button>
                ))
            }
        </nav>
    )
}