import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useFormik } from "formik";
import * as Yup from "yup";
import { Link, useNavigate } from 'react-router';
import { EyeIcon, EyeOffIcon } from '../../icons';
import { Page } from '../../layout/Page';
import LoadingButton from '../../components/Buttons/LoadingButton';
import { Alert, Box, Stack } from '@mui/material';
import { ButtonsSection } from '../../layout/ButtonsSection';
import { AuthenticationError } from '../../hooks/api/useAuthApi';
import { useAuth } from '../../context/AuthContext';
import { toast } from "react-toastify";

const LoginPage = () => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const auth = useAuth();
    const [passwordIsShowing, setPasswordIsShowing] = useState(false);
    const [isLoading, setIsLoading] = useState(false);

    const formik = useFormik({
        initialValues: {
            email: "",
            password: "",
        },
        validationSchema: Yup.object({
            email: Yup.string()
                .email(t("form.emailValidation"))
                .required(t("form.requiredField")),
            password: Yup.string()
                .required(t("form.requiredField")),
        }),
        onSubmit: async (values) => {
            try {
                setIsLoading(true);
                debugger;
                await auth.signIn(values.email, values.password);
                navigate("/user/home");
            } catch (e) {
                if (e instanceof AuthenticationError) {
                    toast.error(t("login.errors.invalidCredentials"))
                    return;
                }
                toast.error(t("login.errors.unkownError"))
            } finally {
                setIsLoading(false);
            }
        }
    });

    return <Page
        title={t("login.title")}
        footer={<ButtonsSection>
            <LoadingButton
                isLoading={false}
                primaryButton={false}
            >
                {t("login.noAccount")} {t("login.createAccount")}
            </LoadingButton>
            <LoadingButton
                isLoading={isLoading}
                type="submit"
                form="signin-form"
                disabled={formik.isValid == false}
            >
                {t("login.login")}
            </LoadingButton>
        </ButtonsSection>}
    >
        <Box
            className="container"
            sx={{
                flex: 1,
                display: "flex",
                flexDirection: "column",
                justifyContent: "center",

                "& .MuiAlert-root": {
                    paddingTop: 0,
                    paddingBottom: 0,
                }
            }}
        >
            <form onSubmit={formik.handleSubmit} id="signin-form">
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
                    <Stack className="form-group" direction="column" gap={0.5}>
                        <label htmlFor="email">{t("form.email")}</label>
                        <input
                            type="email"
                            id="email"
                            name="email"
                            placeholder={t("form.emailPlaceholder")}
                            onChange={formik.handleChange}
                            onBlur={formik.handleBlur}
                            value={formik.values.email}
                        />
                        {
                            formik.touched.email &&
                            formik.errors.email &&
                            <Alert variant="outlined" severity="warning" icon={false}>
                                {formik.errors.email}
                            </Alert>
                        }
                    </Stack>
                    <Stack className="form-group password-field" direction="column" gap={1} marginBottom="0.5rem">
                        <label htmlFor="password">{t("form.password")}</label>
                        <input
                            type={passwordIsShowing ? "text" : "password"}
                            id="password"
                            name="password"
                            placeholder={t("form.passwordPlaceholder")}
                            onChange={formik.handleChange}
                            onBlur={formik.handleBlur}
                            value={formik.values.password}
                        />
                        <div className="eye-toggle" onClick={() => setPasswordIsShowing(prevState => !prevState)}>
                        {
                            formik.values.password.length > 0 &&
                            (passwordIsShowing ? <EyeOffIcon /> : <EyeIcon />)
                        }
                        </div>
                        {
                            formik.touched.password &&
                            formik.errors.password &&
                            <Alert variant="outlined" severity="warning" icon={false}>
                                {formik.errors.password}
                            </Alert>
                        }
                    </Stack>
                    <Link className="authentication__forgot" to="/user/forgotpassword">
                        {t("login.forgotPassword")}
                    </Link>
                </Stack>
            </form>
        </Box>
    </Page>
};

export default LoginPage;