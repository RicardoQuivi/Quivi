import { useTranslation } from "react-i18next";
import PageMeta from "../../components/common/PageMeta";
import AuthLayout from "./AuthPageLayout";
import { useMemo, useState } from "react";
import Button from "../../components/ui/button/Button";
import { Link } from "react-router";
import { ChevronLeftIcon } from "../../icons";
import { useUserApi } from "../../hooks/api/useUserApi";
import { CheckYourInbox } from "../../components/auth/CheckYourInbox";
import * as yup from 'yup';
import { useQuiviForm } from "../../hooks/api/exceptions/useQuiviForm";
import { useToast } from "../../layout/ToastProvider";
import { TextField } from "../../components/inputs/TextField";
import { Spinner } from "../../components/spinners/Spinner";

export const ForgotPasswordPage = () => {
    const { t } = useTranslation();
    const userApi = useUserApi();

    const [email, setEmail] = useState<string>("");
    const [recovered, setRecovered] = useState(false);

    const recover = async () => {
        await userApi.forgotPassword(email);
        setRecovered(true);
    }

    return <>
        <PageMeta
            title={t("pages.forgotPassword.title")}
            description={t("quivi.product.description")}
        />
        <AuthLayout>
            <div className="flex flex-col flex-1">
                <div className="w-full max-w-md mx-auto mb-5 sm:pt-10">
                    <Link
                        to="/signin"
                        className="inline-flex items-center text-sm text-gray-500 transition-colors hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300"
                    >
                        <ChevronLeftIcon className="size-5" />
                        {t("pages.forgotPassword.backToSignIn")}
                    </Link>
                </div>
                <div className="flex flex-col justify-center flex-1 w-full max-w-md mx-auto">
                    {
                        recovered == false
                        ?
                        <RecoverForm
                            email={email}
                            onEmailChanged={setEmail}
                            onRecover={recover}
                        />
                        :
                        <CheckYourInbox
                            title={t("pages.forgotPassword.emailSent")}
                            description={t("pages.forgotPassword.emailSentDescription")}
                            buttonText={t("pages.forgotPassword.resend")}
                            onResend={recover}
                        />
                    }
                </div>
            </div>
        </AuthLayout>
    </>
}

const recoverPasswordSchema = yup.object({
    email: yup.string().email().required(),
});
interface RecoverFormProps {
    readonly email: string;
    readonly onEmailChanged: (email: string) => any;
    readonly onRecover: () => Promise<any>;
}
const RecoverForm = (props: RecoverFormProps) => {
    const { t } = useTranslation();
    const toast = useToast();
    
    const state = useMemo(() => ({
        email: props.email,
    }), [props.email])
    const form = useQuiviForm(state, recoverPasswordSchema);
    
    const recover = () => form.submit(async () => {
        await props.onRecover();
    }, () => toast.error(t("common.operations.failure.generic")));

    return <>
        <div className="mb-5 sm:mb-8">
            <h1 className="mb-2 font-semibold text-gray-800 text-title-sm dark:text-white/90 sm:text-title-md">
                {t("pages.forgotPassword.forgotPassword")}
            </h1>
            <p className="text-sm text-gray-500 dark:text-gray-400">
                {t("pages.forgotPassword.forgotPasswordDescription")}
            </p>
        </div>
        <>
            <form
                onSubmit={async e => {
                    e.preventDefault();
                    await recover();
                }}
            >
                <div className="space-y-6">
                    <TextField
                        label={t("common.email")}
                        name="email"
                        type="email"
                        autoComplete="username"
                        value={props.email}
                        onChange={props.onEmailChanged}
                        errorMessage={form.touchedErrors.get("email")?.message}
                    />
                    <Button 
                        size="md" 
                        variant="primary"
                        type="submit"
                        disabled={form.isSubmitting}
                        className="w-full"
                    >
                        {
                            form.isSubmitting
                            ?
                            <Spinner />
                            :
                            t("pages.forgotPassword.sendEmail")
                        }
                    </Button>
                </div>
            </form>

            <div className="mt-5">
                <p className="text-sm font-normal text-center text-gray-700 dark:text-gray-400 sm:text-start">
                    {t("pages.forgotPassword.rememberedYourPassword")}&nbsp;
                    <Link
                        to="/signin"
                        className="text-brand-500 hover:text-brand-600 dark:text-brand-400"
                    >
                        {t("pages.forgotPassword.signIn")}
                    </Link>
                </p>
            </div>
        </>
    </>
}