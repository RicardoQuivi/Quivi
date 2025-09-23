import { useTranslation } from "react-i18next";
import PageMeta from "../../components/common/PageMeta";
import AuthLayout from "./AuthPageLayout";
import { useEffect, useState } from "react";
import Button from "../../components/ui/button/Button";
import { GoogleIcon, XIcon } from "../../icons";
import { Link, useNavigate } from "react-router";
import { useAuth } from "../../context/AuthContext";
import { AuthenticationError } from "../../hooks/api/useAuthApi";
import { TextField } from "../../components/inputs/TextField";
import { PasswordField } from "../../components/inputs/PasswordField";
import { Spinner } from "../../components/spinners/Spinner";

export const SignInPage = () => {
    const { t } = useTranslation();
    const auth = useAuth();
    const navigate = useNavigate();

    const [isSubmitting, setIsSubmitting] = useState(false);
    const [state, setState] = useState({
        email: "",
        password: "",
    })

    const [credentialsError, setCredentialsError] = useState(false);

    useEffect(() => setCredentialsError(false), [state]);

    const login = async () => {
        try {
            setIsSubmitting(true);
            await auth.signIn(state.email, state.password);
            navigate("/");
        } catch (e) {
            if (e instanceof AuthenticationError) {
                setCredentialsError(true);
            }
        } finally {
            setIsSubmitting(false);
        }
    }

    return <>
        <PageMeta
            title={t("pages.signIn.title")}
            description={t("quivi.product.description")}
        />
        <AuthLayout>
            <div className="flex flex-col flex-1">
                <div className="flex flex-col justify-center flex-1 w-full max-w-md mx-auto">
                    <div>
                        <div className="mb-5 sm:mb-8">
                            <h1 className="mb-2 font-semibold text-gray-800 text-title-sm dark:text-white/90 sm:text-title-md">
                                {t("pages.signIn.signIn")}
                            </h1>
                            <p className="text-sm text-gray-500 dark:text-gray-400">
                                {t("pages.signIn.signInDescription")}
                            </p>
                        </div>
                        <div>
                            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 sm:gap-5">
                                <Button
                                    size="md"
                                    variant="outline"
                                    disabled
                                    startIcon={<GoogleIcon />}
                                >
                                    {t("pages.signIn.with", { provider: "Google" })}
                                </Button>
                                <Button
                                    size="md"
                                    variant="outline"
                                    disabled
                                    startIcon={<XIcon />}
                                >
                                    {t("pages.signIn.with", { provider: "X" })}
                                </Button>
                            </div>
                            <div className="relative py-3 sm:py-5">
                                <div className="absolute inset-0 flex items-center">
                                    <div className="w-full border-t border-gray-200 dark:border-gray-800" />
                                </div>
                                <div className="relative flex justify-center text-sm">
                                    <span className="p-2 text-gray-400 bg-white dark:bg-gray-900 sm:px-5 sm:py-2">
                                        {t("common.or")}
                                    </span>
                                </div>
                            </div>
                            <form
                                onSubmit={async e => {
                                    e.preventDefault();
                                    await login();
                                }}
                            >
                                <div className="space-y-6">
                                    <TextField
                                        value={state.email}
                                        type="email"
                                        name="email"
                                        label={t("common.email")}
                                        placeholder={t("common.emailPlaceholder")}
                                        onChange={v => setState(s => ({ ...s, email: v }))}
                                        autoComplete="username"
                                    />
                                    <PasswordField
                                        value={state.password}
                                        name="password"
                                        label={t("common.password")}
                                        placeholder={t("common.passwordPlaceholder")}
                                        onChange={v => setState(s => ({ ...s, password: v }))}
                                        errorMessage={credentialsError ? t("common.apiErrors.InvalidCredentials") : undefined}
                                        autoComplete="current-password"
                                    />
                                    <div className="flex items-center justify-between">
                                        <div className="flex items-center gap-3" />
                                        <Link
                                            to="/forgotPassword"
                                            className="text-sm text-brand-500 hover:text-brand-600 dark:text-brand-400"
                                        >
                                            {t("pages.signIn.forgotPassword")}
                                        </Link>
                                    </div>
                                    <Button
                                        size="md"
                                        variant="primary"
                                        type="submit"
                                        disabled={isSubmitting || !state.email || !state.password}
                                        className="w-full"
                                    >
                                        {isSubmitting ? <Spinner /> : t("pages.signIn.signIn")}
                                    </Button>
                                </div>
                            </form>

                            <div className="mt-5">
                                <p className="text-sm font-normal text-center text-gray-700 dark:text-gray-400 sm:text-start">
                                    {t("pages.signIn.dontHaveAnAccount")}&nbsp;
                                    <Link
                                        to="/signup"
                                        className="text-brand-500 hover:text-brand-600 dark:text-brand-400"
                                    >
                                        {t("pages.signIn.signUp")}
                                    </Link>
                                </p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </AuthLayout>
    </>
}