import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useFormik } from "formik";
import * as Yup from "yup";
import { Page } from '../../layout/Page';
import { Alert, Stack } from '@mui/material';
import { useLocation, useNavigate } from 'react-router';
import { nifValidation } from '../../helpers/inputValidators';
import LoadingButton from '../../components/Buttons/LoadingButton';
import { useUserApi } from '../../hooks/api/useUserApi';
import { EyeIcon, EyeOffIcon } from '../../icons';
import { useQuiviTheme } from '../../hooks/theme/useQuiviTheme';
import { isPossiblePhoneNumber } from 'react-phone-number-input';
import PhoneInput from 'react-phone-number-input';
import 'react-phone-number-input/style.css'
import { ButtonsSection } from '../../layout/ButtonsSection';

const RegisterPage = () => {
    const theme = useQuiviTheme();
    const { t } = useTranslation();
    const location = useLocation();
    const api = useUserApi();
    const navigate = useNavigate();

    const [registrationEmail, setRegistrationEmail] = useState<string>();
    const [passwordIsShowing, setPasswordIsShowing] = useState(false);
    const [isLoading, setIsLoading] = useState(false);

    const email = new URLSearchParams(location.search).get("email");

    const formik = useFormik({
        initialValues: {
            name: "",
            vat: "",
            email: email ?? "",
            password: "",
            phoneNumber: "",
        },
        validationSchema: () => Yup.object({
            name: Yup.string().required(t("form.requiredField")),
            vat: Yup.string().max(9, t("form.vatCharValidation"))
                            .test("vat-validation", t("form.vatValidation"), (value) => {
                                if (value) {
                                    return nifValidation(value);
                                } else return true;
                            }),
            email: Yup.string().email(t("form.emailValidation")).required(t("form.requiredField")),
            password: Yup.string()
                .min(6, t("form.passwordValidation"))
                .required(t("form.requiredField")),
            phoneNumber: Yup.string()
                .test("phoneNumber-validation", t("form.phoneNumberValidation"),
                    function (value) {
                        if (value) {
                            return isPossiblePhoneNumber(value);
                        } else return true;
                    })
        }),
        onSubmit: async (values) => {
            setIsLoading(true);
            await api.register({
                vatNumber: values.vat,
                name: values.name,
                email: values.email,
                password: values.password,
                phoneNumber: values.phoneNumber,
            });
            setRegistrationEmail(values.email)
            setIsLoading(false);
        }
    })

    return <Page
        title={t("register.title")}
        headerProps={{
            ordering: false,
        }}
        footer={registrationEmail != undefined ? undefined : (
            <ButtonsSection>
                <LoadingButton
                    isLoading={false}
                    primaryButton={false}
                    onClick={() => navigate("/user/login")}
                >
                    {t("register.alreadyHasAccount")} {t("register.login")}
                </LoadingButton>
                <LoadingButton
                    isLoading={isLoading}
                    type="submit"
                    form="register-form"
                    disabled={formik.isValid == false}
                >
                    {t("register.register")}
                </LoadingButton>
            </ButtonsSection>
        )}
    >
        {
            registrationEmail == undefined
            ?
            <>
                <div className="container">
                    <h1>{t("register.title")}</h1>
                    <p>{t("register.description")}</p>
                </div>
                <div className="container">
                    <form onSubmit={formik.handleSubmit} id="register-form">
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
                                <label htmlFor="name">{t("form.name")}<span className="required-sign"> *</span></label>
                                <input
                                    type="text"
                                    id="name"
                                    name="name"
                                    placeholder={t("form.namePlaceholder")}
                                    onChange={formik.handleChange}
                                    onBlur={formik.handleBlur}
                                    value={formik.values.name}
                                />
                                {
                                    formik.touched.name &&
                                    formik.errors.name &&
                                    <Alert variant="outlined" severity="warning" icon={false}>
                                        {formik.errors.name}
                                    </Alert>
                                }
                            </Stack>
                            <Stack className="form-group" direction="column" gap={1}>
                                <label htmlFor="vat">{t("form.vat")}</label>
                                <input
                                    type="tel"
                                    id="vat"
                                    name="vat"
                                    placeholder={t("form.vatPlaceholder")}
                                    onChange={formik.handleChange}
                                    onBlur={formik.handleBlur}
                                    value={formik.values.vat}
                                />
                                {
                                    formik.touched.vat &&
                                    formik.errors.vat &&
                                    <Alert variant="outlined" severity="warning" icon={false}>
                                        {formik.errors.vat}
                                    </Alert>
                                }
                            </Stack>
                            <Stack className="form-group" direction="column" gap={1}>
                                <label htmlFor="phoneNumber">{t("form.phoneNumber")}</label>
                                <PhoneInput
                                    className="input--phonenumber"
                                    name="phoneNumber"
                                    id="phoneNumber"
                                    placeholder={t("form.phoneNumberPlaceholder")}
                                    defaultCountry="PT"
                                    value={formik.values.phoneNumber}
                                    onChange={e => formik.setFieldValue("phoneNumber", e)}
                                    onBlur={formik.handleBlur("phoneNumber")}
                                />
                                {
                                    formik.touched.phoneNumber &&
                                    formik.errors.phoneNumber &&
                                    <Alert variant="outlined" severity="warning">
                                        {formik.errors.phoneNumber}
                                    </Alert>
                                }
                            </Stack>
                            <Stack className="form-group" direction="column" gap={1}>
                                <label htmlFor="email">{t("form.email")}<span className="required-sign"> *</span></label>
                                <input
                                    type="email"
                                    id="email"
                                    name="email"
                                    disabled={email != null}
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
                                <label htmlFor="password">{t("form.password")}<span className="required-sign"> *</span></label>
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
                                        (
                                            passwordIsShowing
                                            ?
                                            <EyeOffIcon />
                                            :
                                            <EyeIcon />
                                        )
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
                        </Stack>
                    </form>
                </div>
            </>
            :
            <div className="authentication__header">
                <div className="container">
                    <div className="flex flex-fd-c flex-ai-c mb-8">
                        <svg width="174" height="154" viewBox="0 0 174 154" fill="none" xmlns="http://www.w3.org/2000/svg">
                            <path d="M80.8699 153.53L83.3977 153.725C93.8211 154.556 104.391 153.496 113.966 150.658C123.542 147.821 131.758 143.314 137.729 137.625C139.265 136.129 140.631 134.561 141.816 132.932C146.004 126.98 147.895 120.439 147.323 113.89C146.75 107.341 143.731 100.986 138.533 95.3905C132.329 88.8272 122.989 83.1397 120.839 75.5708C118.015 65.7205 128.225 56.6162 137.827 48.9284C147.43 41.2406 157.607 32.1147 154.718 22.2643C152.912 16.1011 146.1 10.9651 138.205 7.38607C117.326 -2.10746 88.601 -2.01015 65.9821 5.16946C43.3633 12.3491 26.4073 25.7136 15.3113 40.5377C-0.216609 61.2764 -5.12448 86.0159 6.15213 108.128C17.4287 130.24 46.1045 148.967 80.8699 153.53Z" fill="#FFF5F2" />
                            <path d="M87.3097 49.4031L95.6983 61.2273L97.8771 80.1124L104.865 75.7614L111.142 86.0123L173.777 33.0239L87.3097 49.4031Z" fill={theme.primaryColor.hex} />
                            <path opacity="0.3" d="M105.316 75.6613L102.313 77.4761L100.738 67.3867L105.316 75.6613Z" fill="black" />
                            <path opacity="0.3" d="M95.8963 60.5259L173.981 32.1702L100.671 65.4582L98.0707 79.5002L95.8963 60.5259Z" fill="black" />
                            <path d="M23.8655 128.971C24.0716 129.205 21.959 130.845 18.9188 132.337C17.1606 133.396 15.1021 134.064 12.9414 134.275C12.7869 133.977 15.2603 132.826 18.2232 131.378C19.9371 130.332 21.8418 129.52 23.8655 128.971Z" fill="#4F4F4F" />
                            <path d="M39.7993 111.298C39.3843 113.113 38.4552 114.801 37.1017 116.201C35.1642 118.685 33.1042 120.312 32.8345 120.114C33.6522 118.453 34.7434 116.912 36.0717 115.541C37.96 113.145 39.4805 111.144 39.7993 111.298Z" fill="#4F4F4F" />
                            <path d="M36.8143 116.406C34.7169 116.748 32.5303 116.623 30.5276 116.049C28.4021 115.781 26.4332 115.047 24.8745 113.942C26.9301 114.162 28.9455 114.553 30.8824 115.108C32.9234 115.36 34.9154 115.796 36.8143 116.406V116.406Z" fill="#4F4F4F" />
                            <path d="M62.6839 112.174C61.1083 113.414 59.1757 114.289 57.0759 114.712C55.1071 115.506 52.9187 115.836 50.7441 115.667C52.5774 114.767 54.5522 114.088 56.6106 113.649C58.5381 112.91 60.5843 112.413 62.6839 112.174V112.174Z" fill="#4F4F4F" />
                            <path d="M47.0062 89.1885C47.7753 90.9014 47.9603 92.7825 47.5372 94.5909C47.5161 96.4398 46.9134 98.2454 45.7992 99.7972C45.6959 98.0139 45.8582 96.2262 46.282 94.4819C46.2942 92.698 46.5373 90.9215 47.0062 89.1885V89.1885Z" fill="#4F4F4F" />
                            <path d="M30.8449 73.4333C33.0295 73.031 35.3021 73.4043 37.1889 74.4754C40.5279 75.9807 42.0176 78.4586 41.7607 78.5744C41.5039 78.6902 39.5776 76.8839 36.6239 75.5407C33.6702 74.1975 30.7935 73.8038 30.8449 73.4333Z" fill="#4F4F4F" />
                            <path d="M10.9448 83.8804C11.0502 82.0399 11.9742 80.2951 13.5419 78.9766C14.7811 77.4332 16.6972 76.3412 18.8919 75.9277C19.0997 76.2049 16.7363 77.5055 14.6067 79.6802C12.4771 81.8549 11.3084 83.987 10.9448 83.8804Z" fill="#4F4F4F" />
                            <path d="M13.8891 105.985C12.158 104.943 10.8577 103.405 10.1936 101.615C9.16553 99.9967 8.76011 98.1124 9.04173 96.2607C9.9886 97.8293 10.7912 99.4666 11.4414 101.156C12.6413 103.844 14.2011 105.745 13.8891 105.985Z" fill="#4F4F4F" />
                            <path d="M83.5557 98.9131C83.8165 99.1405 81.7825 100.937 79.0185 102.893C77.4931 104.237 75.6638 105.29 73.6468 105.986C73.4122 105.713 75.4461 103.916 78.1841 101.983C79.7157 100.651 81.5438 99.6061 83.5557 98.9131V98.9131Z" fill="#4F4F4F" />
                            <path d="M96.3889 85.6645C96.7264 85.7927 96.3889 87.01 95.2205 88.2487C94.0521 89.4874 92.9875 90.2349 92.624 90.0426C92.2605 89.8504 93.0394 88.7826 94.052 87.608C95.0646 86.4334 96.0514 85.5364 96.3889 85.6645Z" fill="#4F4F4F" />
                        </svg>
                    </div>
                    <h1>{t("register.confirmationPending")}</h1>
                    <p>{t("register.confirmationPendingDescription")}</p>
                </div>
            </div>
        }
    </Page>
};

export default RegisterPage;