import { useTranslation } from "react-i18next";
import PageMeta from "../../components/common/PageMeta"
import AuthLayout from "./AuthPageLayout";
import { useUserApi } from "../../hooks/api/useUserApi";
import { useState } from "react";
import { CheckYourInbox } from "../../components/auth/CheckYourInbox";
import { ChevronLeftIcon, GoogleIcon, XIcon } from "../../icons";
import Button from "../../components/ui/button/Button";
import { ClipLoader } from "react-spinners";
import { useQuiviForm } from "../../hooks/api/exceptions/useQuiviForm";
import * as yup from 'yup';
import { Link } from "react-router";
import { useToast } from "../../layout/ToastProvider";
import { TextField } from "../../components/inputs/TextField";
import { PasswordField } from "../../components/inputs/PasswordField";

export const SignUpPage = () => {
    const { t } = useTranslation();
    const userApi = useUserApi();

    const [registrationEmail, setRegistrationEmail] = useState<string>();
    
    const onSubmit = async (email: string, password?: string) => {
        await userApi.register(email, password);
        setRegistrationEmail(email)
    }

    return <>
        <PageMeta
            title={t("pages.signUp.title")}
            description={t("quivi.product.description")}
        />
        <AuthLayout>
            <div className="flex flex-col flex-1 w-full overflow-y-auto lg:w-1/2 no-scrollbar">
                <div className="w-full max-w-md mx-auto mb-5 sm:pt-10">
                    <Link
                        to="/signin"
                        className="inline-flex items-center text-sm text-gray-500 transition-colors hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300"
                    >
                        <ChevronLeftIcon className="size-5" />
                        {t("pages.signUp.backToSignIn")}
                    </Link>
                </div>
                <div className="flex flex-col justify-center flex-1 w-full max-w-md mx-auto">
                    {
                        registrationEmail == undefined
                        ?
                        <SigningUpForm onSubmit={onSubmit} />
                        :
                        <CheckYourInbox
                            title={t("pages.signUp.verifyYourInbox")}
                            description={t("pages.signUp.signUpCompletedDescription")}
                            buttonText={t("pages.signUp.resendConfirmation")}
                            onResend={() => onSubmit(registrationEmail)}
                        />
                    }
                </div>
            </div>
        </AuthLayout>
    </>
}


const signInSchema = yup.object({
    email: yup.string().email().required(),
    password: yup.string().min(6).required(),
});

interface SigningUpFormProps {
    readonly onSubmit: (email: string, password: string) => Promise<any>;
}
const SigningUpForm = (props: SigningUpFormProps) => {
    const { t } = useTranslation();
    const toast = useToast();

    const [state, setState] = useState({
        email: "",
        password: "",
    })
    
    const form = useQuiviForm(state, signInSchema);

    const submit = async () => await form.submit(async () => {
        await props.onSubmit(state.email, state.password);
    }, () => toast.error(t("common.operations.failure.generic")));
    
    return <>
        <div className="mb-5 sm:mb-8">
            <h1 className="mb-2 font-semibold text-gray-800 text-title-sm dark:text-white/90 sm:text-title-md">
                {t("pages.signUp.signUp")}
            </h1>
            <p className="text-sm text-gray-500 dark:text-gray-400">
                {t("pages.signUp.signUpDescription")}
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
                    {t("pages.signUp.with", { provider: "Google" })}
                </Button>
                <Button
                    size="md"
                    variant="outline"
                    disabled
                    startIcon={<XIcon />}
                >
                    {t("pages.signUp.with", { provider: "X" })}
                </Button>
            </div>
            <div className="relative py-3 sm:py-5">
                <div className="absolute inset-0 flex items-center">
                    <div className="w-full border-t border-gray-200 dark:border-gray-800"></div>
                </div>
                <div className="relative flex justify-center text-sm">
                    <span className="p-2 text-gray-400 bg-white dark:bg-gray-900 sm:px-5 sm:py-2">
                        {t("common.or")}
                    </span>
                </div>
            </div>

            <div className="space-y-5">
                <TextField
                    value={state.email}
                    type="email"
                    name="email"
                    label={t("common.email")}
                    placeholder={t("common.emailPlaceholder")}
                    onChange={v => setState(s => ({...s, email: v}))}
                    errorMessage={form.touchedErrors.get("email")?.message}
                />
                <PasswordField
                    value={state.password}
                    label={t("common.password")}
                    placeholder={t("common.passwordPlaceholder")}
                    name="password"
                    onChange={v => setState(s => ({...s, password: v }))}
                    errorMessage={form.touchedErrors.get("password")?.message}
                />
                <Button 
                    size="md" 
                    variant="primary"
                    onClick={submit}
                    disabled={form.isSubmitting || !form.isValid}
                    className="w-full"
                >
                    {
                        form.isSubmitting
                        ?
                        <ClipLoader
                            size={20}
                            cssOverride={{
                                borderColor: "white"
                            }}
                        />
                        :
                        t("pages.signUp.signUp")
                    }
                </Button>
            </div>

            <div className="mt-5">
                <p className="text-sm font-normal text-center text-gray-700 dark:text-gray-400 sm:text-start">
                    {t("pages.signUp.alreadyHaveAnAccount")} &nbsp;
                    <Link
                        to="/signin"
                        className="text-brand-500 hover:text-brand-600 dark:text-brand-400"
                    >
                        {t("pages.signUp.signIn")}
                    </Link>
                </p>
            </div>
        </div>
    </>
}