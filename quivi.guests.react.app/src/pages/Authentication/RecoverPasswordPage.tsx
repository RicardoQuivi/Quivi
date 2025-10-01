import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import * as Yup from "yup";
import { useFormik } from 'formik';
import { useNavigate, useSearchParams } from 'react-router';
import { Page } from '../../layout/Page';
import { useUserApi } from '../../hooks/api/useUserApi';
import { EyeIcon, EyeOffIcon } from '../../icons';
import { Alert, Stack } from '@mui/material';
import { ButtonsSection } from '../../layout/ButtonsSection';
import LoadingButton from '../../components/Buttons/LoadingButton';
import { toast } from 'react-toastify';

export const RecoverPasswordPage = () => {
    const { t } = useTranslation();
    const [searchParams] = useSearchParams();
    const userApi = useUserApi();
    const navigate = useNavigate();

    const [newPasswordIsShowing, setNewPasswordIsShowing] = useState(false);
    const [confirmPasswordIsShowing, setConfirmPasswordIsShowing] = useState(false);
    const [isLoading, setIsLoading] = useState(false);

    const formik = useFormik({
        initialValues: {
            newPassword: "",
            confirmPassword: "",
        },
        validationSchema: Yup.object({
            newPassword: Yup.string()
                .min(6, t("form.passwordValidation"))
                .required(t("form.requiredField")),
            confirmPassword: Yup.string()
                .oneOf([Yup.ref("newPassword")], t("form.samePasswordValidation"))
                .required(t("form.requiredField"))
        }),
        onSubmit: async (values) => {
            try {
                setIsLoading(true);
                const email = searchParams.get('email');
                const code = searchParams.get('code');

                if(email == null) {
                    return;
                }

                if(code == null) {
                    return;
                }

                await userApi.recoverPassword(email, code, values.newPassword);
                navigate("/user/login")
                toast.info(t("resetPassword.successDescription"));
            } catch(e) {
                toast.error(t("unexpectedError"));
            } finally {
                setIsLoading(false);
            }
        }
    });

    return <Page
        title={t("resetPassword.title")}
        footer={<ButtonsSection>
            <LoadingButton
                isLoading={isLoading}
                type="submit"
                form="recover-form"
                disabled={formik.isValid == false}
            >
                {t("resetPassword.confirm")}
            </LoadingButton>
            {undefined}
        </ButtonsSection>}
    >
        <div className="container">
            <h1>{t("resetPassword.title")}</h1>
            <p>{t("resetPassword.description")}</p>
        </div>
        <div className="container">
            <form onSubmit={formik.handleSubmit} id="recover-form">
                <Stack
                    sx={{
                        "& label": {
                            margin: 0,
                        },

                        "& .MuiStack-root": {
                            margin: 0,
                        },

                        "& .MuiAlert-root": {
                            paddingTop: 0,
                            paddingBottom: 0,
                        }
                    }}
                    gap={2}
                >
                    <Stack className="form-group password-field" direction="column" gap={1} marginBottom="0.5rem">
                        <label htmlFor="password">{t("form.newPassword")}</label>
                        <input
                            type={newPasswordIsShowing ? "text" : "password"}
                            id="newPassword"
                            name="newPassword"
                            placeholder={t("form.newPasswordPlaceholder")}
                            onChange={formik.handleChange}
                            onBlur={formik.handleBlur}
                            value={formik.values.newPassword}
                        />
                        <div className="eye-toggle" onClick={() => setNewPasswordIsShowing(prevState => !prevState)}>
                        {
                            formik.values.newPassword.length > 0 &&
                            (newPasswordIsShowing ? <EyeOffIcon /> : <EyeIcon />)
                        }
                        </div>
                        {
                            formik.touched.newPassword &&
                            formik.errors.newPassword &&
                            <Alert variant="outlined" severity="warning" icon={false}>
                                {formik.errors.newPassword}
                            </Alert>
                        }
                    </Stack>
                    <Stack className="form-group password-field" direction="column" gap={1} marginBottom="0.5rem">
                        <label htmlFor="password">{t("form.confirmNewPassword")}</label>
                        <input
                            type={confirmPasswordIsShowing ? "text" : "password"}
                            id="confirmPassword"
                            name="confirmPassword"
                            placeholder={t("form.confirmNewPassword")}
                            onChange={formik.handleChange}
                            onBlur={formik.handleBlur}
                            value={formik.values.confirmPassword}
                        />
                        <div className="eye-toggle" onClick={() => setConfirmPasswordIsShowing(prevState => !prevState)}>
                        {
                            formik.values.confirmPassword.length > 0 &&
                            (confirmPasswordIsShowing ? <EyeOffIcon /> : <EyeIcon />)
                        }
                        </div>
                        {
                            formik.touched.confirmPassword &&
                            formik.errors.confirmPassword &&
                            <Alert variant="outlined" severity="warning" icon={false}>
                                {formik.errors.confirmPassword}
                            </Alert>
                        }
                    </Stack>
                </Stack>
            </form>
        </div>
    </Page>
};
