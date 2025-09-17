import React from "react";
import GridShape from "../../components/common/GridShape";
import { Link } from "react-router";
import FloatingThemeToggleButton from "../../components/common/FloatingThemeToggleButton";
import { useTranslation } from "react-i18next";
import { FloatingLanguageButton } from "../../components/header/LanguageButton";
import { QuiviFullIcon } from "../../icons";

interface AuthLayoutProps {
    readonly children: React.ReactNode;
}
export default function AuthLayout(props: AuthLayoutProps) {
    const { t } = useTranslation();

    return (
    <div className="relative p-6 bg-white z-1 dark:bg-gray-900 sm:p-0">
        <div className="relative flex flex-col justify-center w-full h-screen lg:flex-row dark:bg-gray-900 sm:p-0">
            {props.children}
            <div className="items-center hidden w-full h-full lg:w-1/2 bg-brand-950 dark:bg-white/5 lg:grid">
                <div className="relative flex items-center justify-center z-1">
                    <GridShape />
                    <div className="flex flex-col items-center max-w-xs">
                        <Link to="/" className="block mb-4">
                            <QuiviFullIcon
                                height="auto"
                                width={231}
                                className="fill-brand-50 dark:fill-brand-700"
                            />
                        </Link>
                        <p className="text-center text-gray-400 dark:text-white/60">
                            {t("quivi.product.description")}
                        </p>
                    </div>
                </div>
            </div>
            <div className="fixed z-50 hidden bottom-6 right-6 sm:block">
                <div className="flex flex-wrap justify-between gap-2">
                    <FloatingLanguageButton />
                    <FloatingThemeToggleButton />
                </div>
            </div>
        </div>
    </div>
    );
}