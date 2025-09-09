import { useTranslation } from "react-i18next";
import PageMeta from "../../components/common/PageMeta"
import { Link, useNavigate, useSearchParams } from "react-router";
import { ChevronLeftIcon } from "../../icons";
import React, { useEffect, useState } from "react";
import { useUserApi } from "../../hooks/api/useUserApi";
import { PuffLoader } from "react-spinners";
import { ApiException } from "../../hooks/api/exceptions/ApiException";
import Button from "../../components/ui/button/Button";
import AuthLayout from "./AuthPageLayout";
import { Spinner } from "../../components/spinners/Spinner";

enum ConfirmState {
    Confirming,
    Confirmed,
    Expired,
}
export const ConfirmEmailPage = () => {
    const { t } = useTranslation();
    
    const [confirmState, setConfirmState] = useState<ConfirmState>(ConfirmState.Confirming);
    const [searchParams] = useSearchParams();
    const [state, setState] = useState({
        email: undefined as string | undefined,
        code: undefined as string | undefined,
    })

    useEffect(() => {
        const email = searchParams.get('email');
        const code = searchParams.get('code');
        if(email == null) {
            return;
        }

        if(code == null) {
            return;
        }

        setState({
            email: email,
            code: code,
        })
    }, [searchParams])

    const getBody = (confirmState: ConfirmState): React.ReactNode => {
        switch(confirmState){
            case ConfirmState.Confirming: 
                return <ConfirmingEmail
                    email={state.email}
                    code={state.code}
                    onConfirmed={() => setConfirmState(ConfirmState.Confirmed)}
                    onExpired={() => setConfirmState(ConfirmState.Expired)}
                />
            case ConfirmState.Confirmed:
                return <EmailConfirmed />;
            case ConfirmState.Expired:
                return <TokenExpired
                    email={state.email}
                 />
        }
    }
    return <>
        <PageMeta
            title={t("pages.signUpConfirm.title")}
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
                        {t("pages.signUpConfirm.backToSignIn")}
                    </Link>
                </div>
                <div className="flex flex-col justify-center flex-1 w-full max-w-md mx-auto">
                    { getBody(confirmState) }
                </div>
            </div>
        </AuthLayout>
    </>
}

interface ConfirmingEmailProps {
    readonly email: string | undefined;
    readonly code: string | undefined;
    readonly onConfirmed: () => any;
    readonly onExpired: () => any;
}
const ConfirmingEmail = (props: ConfirmingEmailProps) => {
    const { t } = useTranslation();
    const userApi = useUserApi();

    const [shouldSubmit, setShouldSubmit] = useState(false);
    const [submitting, setSubmitting] = useState(false);

    useEffect(() => {
        const timeout = setTimeout(() => setShouldSubmit(true), 1000);
        return () => clearTimeout(timeout);
    }, [props.email, props.code])

    useEffect(() => {
        if(shouldSubmit == false) {
            return;
        }

        if(submitting == true) {
            return;
        }

        if(props.email == undefined || props.code == undefined) {
            return;
        }

        confirmEmail(props.email, props.code);
    }, [shouldSubmit, submitting])

    const confirmEmail = async (email: string, code: string) =>{
        setSubmitting(true);
        try {
            await userApi.confirm(email, code);
            props.onConfirmed();
        } catch (e) {
            if(e instanceof ApiException) {
                props.onExpired();
            }
        }
    }

    return <>
        <div className="mb-5 sm:mb-8">
            <h1 className="mb-2 font-semibold text-gray-800 text-title-sm dark:text-white/90 sm:text-title-md">
                {t("pages.signUpConfirm.confirmingYourEmail")}
            </h1>
            <div className="relative flex items-center justify-center z-1 mt-10 mb-10">
                <div className="flex flex-col items-center max-w-xs">
                    <PuffLoader
                        size="150px"
                        color="var(--color-brand-800)"
                    />
                </div>
            </div>
            <p className="text-sm text-gray-500 dark:text-gray-400">
                {t("pages.signUpConfirm.confirmingYourEmailDescription")}
            </p>
        </div>
    </>
}

interface TokenExpiredProps {
    readonly email: string | undefined;
}
const TokenExpired = (props: TokenExpiredProps) => {
    const { t } = useTranslation();
    const userApi = useUserApi();

    const [isSubmitting, setIsSubmitting] = useState(false);

    const submit = async () => {
        if(props.email == undefined) {
            return;
        }

        try {
            setIsSubmitting(true);
            await userApi.register(props.email);
            //Implement loader after submitting
        } finally {
            setIsSubmitting(false);
        }
    }

    return <>
        <div className="mb-5 sm:mb-8">
            <h1 className="mb-2 font-semibold text-gray-800 text-title-sm dark:text-white/90 sm:text-title-md">
                {t("pages.signUpConfirm.tokenExpired")}
            </h1>
            <div className="relative flex items-center justify-center z-1 mt-10 mb-10">
                <div className="flex flex-col items-center max-w-xs">
                    <svg xmlns="http://www.w3.org/2000/svg" width="116" height="86" viewBox="0 0 116 86" fill="none">
                        <path d="M3.66351 81.915C3.66351 82.121 2.91014 82.4006 1.87615 82.5477C0.842153 82.6948 0.0149732 82.5477 0.000201892 82.4153C-0.0145695 82.2828 0.783101 81.915 1.77278 81.7826C2.76246 81.6501 3.6192 81.6796 3.66351 81.915Z" fill="#263238"/>
                        <path d="M16.8246 76.0293C16.9428 76.1912 15.7315 77.3242 13.9885 78.3541C12.9805 79.086 11.8003 79.5472 10.5616 79.6931C10.4729 79.4871 11.891 78.6926 13.5897 77.692C14.5724 76.9699 15.6644 76.4086 16.8246 76.0293Z" fill="#263238"/>
                        <path d="M26.3523 65.2292C26.1023 66.4437 25.5427 67.5737 24.7274 68.5105C23.5605 70.1732 22.3197 71.262 22.1572 71.1296C22.6497 70.018 23.307 68.9863 24.1071 68.069C25.2444 66.4652 26.1603 65.1262 26.3523 65.2292Z" fill="#263238"/>
                        <path d="M24.5354 68.4217C23.3133 68.6892 22.0391 68.5919 20.8722 68.1421C19.6336 67.9327 18.4864 67.3587 17.5781 66.4941C18.7759 66.6656 19.9503 66.9717 21.0789 67.4064C22.2682 67.6038 23.4289 67.9447 24.5354 68.4217V68.4217Z" fill="#263238"/>
                        <path d="M38.5831 65.4795C37.6826 66.3204 36.578 66.9137 35.3778 67.2011C34.2525 67.739 33.0017 67.9628 31.7588 67.8485C32.8066 67.2381 33.9353 66.7775 35.1119 66.4801C36.2135 65.979 37.3831 65.6419 38.5831 65.4795V65.4795Z" fill="#263238"/>
                        <path d="M46.3756 85.7376L47.7945 85.8463C53.6453 86.3105 59.5785 85.7183 64.9533 84.1338C70.328 82.5492 74.9401 80.0325 78.2915 76.8554C79.1539 76.0201 79.9207 75.1442 80.5857 74.2348C82.9365 70.911 83.9982 67.2583 83.6768 63.6009C83.3553 59.9436 81.6605 56.3949 78.743 53.27C75.2603 49.6048 70.0177 46.4287 68.8107 42.2019C67.226 36.701 72.9569 31.6168 78.3468 27.3236C83.7368 23.0304 89.4492 17.9342 87.8276 12.4333C86.8141 8.99152 82.9905 6.12335 78.5587 4.12469C66.839 -1.1769 50.7152 -1.12255 38.0188 2.88684C25.3225 6.89624 15.8048 14.3595 9.57642 22.638C0.860347 34.2193 -1.89452 48.0348 4.43523 60.383C10.765 72.7313 26.8612 83.1895 46.3756 85.7376Z" fill="#F8F8F8"/>
                        <path d="M50.6367 22.7928L57.4759 31.5919L59.8393 46.0707L65.0093 42.451L70.1792 50.1171L116 6.88672L50.6367 22.7928Z" fill="var(--color-brand-800)"/>
                        <path opacity="0.3" d="M64.9355 42.5399L62.6016 44.129L60.9473 36.0215L64.9355 42.5399Z" fill="black"/>
                        <path opacity="0.3" d="M57.4756 31.5919L116 6.88672L61.3014 35.1969L59.839 46.0707L57.4756 31.5919Z" fill="black"/>
                        <path d="M116 6.87207C115.791 7.04505 115.563 7.1932 115.32 7.3135L113.311 8.44649L105.822 12.4782L80.9178 25.5297L60.9913 35.8297L61.1242 35.4029C63.6353 39.9496 65.9101 43.5988 67.535 46.1149C68.3179 47.3215 68.9383 48.2926 69.4405 49.0577C69.6886 49.3852 69.8968 49.7408 70.0609 50.1172C69.7675 49.8261 69.5052 49.5055 69.278 49.1607C68.7905 48.528 68.0963 47.5716 67.2395 46.3356C65.5113 43.8636 63.1479 40.2586 60.5629 35.7267L60.4004 35.4471L60.6958 35.2852L80.5633 24.8823L105.497 12.0368L113.105 8.22578L115.187 7.2105C115.438 7.05445 115.712 6.94014 116 6.87207V6.87207Z" fill="#263238"/>
                        <path d="M116 6.87241C116.088 7.06369 102.971 12.7581 86.7082 19.7179C70.445 26.6777 57.1655 32.0484 57.1655 31.8572C57.1655 31.6659 70.1938 25.9715 86.4571 19.0116C102.72 12.0518 115.926 6.69584 116 6.87241Z" fill="#263238"/>
                        <path d="M59.839 46.0705C59.6349 44.3019 59.6846 42.5134 59.9866 40.7587C60.0449 38.9834 60.3532 37.225 60.9025 35.5352C61.1063 37.2989 61.0566 39.0825 60.7548 40.8323C60.706 42.6134 60.3975 44.3779 59.839 46.0705V46.0705Z" fill="#263238"/>
                        <path d="M29.8239 51.3682C30.2946 52.5252 30.4078 53.7958 30.1489 55.0173C30.136 56.2662 29.7672 57.4858 29.0854 58.534C29.0221 57.3295 29.1215 56.1219 29.3808 54.9437C29.3883 53.7387 29.537 52.5388 29.8239 51.3682V51.3682Z" fill="#263238"/>
                        <path d="M21.0351 41.4216C22.2915 41.166 23.5984 41.4032 24.6836 42.0838C26.6039 43.0402 27.4606 44.6146 27.3129 44.6882C27.1652 44.7617 26.0573 43.614 24.3586 42.7606C22.6599 41.9072 21.0055 41.6571 21.0351 41.4216Z" fill="#263238"/>
                        <path d="M9.51367 48.4406C9.57359 47.1704 10.0992 45.9662 10.9908 45.0563C11.6956 43.9911 12.7854 43.2375 14.0337 42.9521C14.1519 43.1434 12.8077 44.041 11.5964 45.5419C10.3852 47.0427 9.72047 48.5141 9.51367 48.4406Z" fill="#263238"/>
                        <path d="M11.8914 62.0654C10.8259 61.3636 10.0255 60.3281 9.61668 59.1226C8.98385 58.0331 8.73429 56.7644 8.90764 55.5176C9.49049 56.5737 9.98456 57.6762 10.3848 58.8136C11.1233 60.6234 12.0835 61.9036 11.8914 62.0654Z" fill="#263238"/>
                        <path d="M50.3706 57.1064C50.5184 57.2536 49.3662 58.416 47.8004 59.6814C46.9363 60.5514 45.9001 61.2328 44.7575 61.6826C44.6246 61.506 45.7768 60.3436 47.3278 59.0929C48.1954 58.2309 49.2309 57.5549 50.3706 57.1064V57.1064Z" fill="#263238"/>
                        <path d="M57.5793 48.9254C57.7713 49.0137 57.5792 49.8524 56.9145 50.7058C56.2498 51.5592 55.6442 52.0742 55.4374 51.9418C55.2306 51.8094 55.6737 51.0737 56.2498 50.2644C56.8259 49.4551 57.3872 48.8371 57.5793 48.9254Z" fill="#263238"/>
                    </svg>
                </div>
            </div>
            <p className="text-sm text-gray-500 dark:text-gray-400">
                {t("pages.signUpConfirm.tokenExpiredDescription")}
            </p>
        </div>
        <Button
            size="md" 
            variant="primary"
            className="w-full"
            onClick={submit}
            disabled={isSubmitting}
        >
            {
                isSubmitting
                ?
                <Spinner />
                :
                t("pages.signUpConfirm.resendConfirmation")
            }
        </Button>
    </>
}

const EmailConfirmed = () => {
    const { t } = useTranslation();
    const navigate = useNavigate();

    return <>
        <div className="mb-5 sm:mb-8">
            <h1 className="mb-2 font-semibold text-gray-800 text-title-sm dark:text-white/90 sm:text-title-md">
                {t("pages.signUpConfirm.emailConfirmed")}
            </h1>
            <div className="relative flex items-center justify-center z-1 mt-10 mb-10">
                <div className="flex flex-col items-center max-w-xs">
                    <svg
                        xmlns="http://www.w3.org/2000/svg"
                        width="126"
                        height="86"
                        viewBox="0 0 126 86"
                        fill="none"
                    >
                        <path d="M124.704 46.2579C124.603 45.7264 124.496 45.201 124.385 44.6817C121.598 31.5189 115.984 17.9254 105.977 8.63603C101.641 4.61908 96.3984 1.54909 90.5565 0.443279C83.4245 -0.903851 76.0096 0.96258 69.4557 4.10894C62.9018 7.2553 57.0203 11.5991 50.9472 15.5886C47.0252 18.1637 42.878 20.6441 38.2501 21.3925C32.7095 22.2876 27.041 20.6075 21.4516 21.142C12.6888 21.979 4.96649 28.5406 1.80821 36.7913C-1.35007 45.0421 -0.206034 54.6187 3.95632 62.4082C8.11868 70.1977 15.062 76.2644 22.9243 80.263C33.8018 85.8104 46.657 87.5455 58.493 84.5396C64.274 83.0734 69.8543 80.5074 75.8057 80.4005C83.1872 80.2691 90.1062 83.9348 97.4086 84.9489C104.273 85.899 111.593 84.338 117.055 80.0614C127.184 72.1191 126.898 57.7009 124.704 46.2579Z" fill="#F8F8F8"/>
                        <path d="M108.239 15H24.9463C22.6356 15 20.7623 16.8807 20.7623 19.2006V66.7833C20.7623 69.1032 22.6356 70.9839 24.9463 70.9839H108.239C110.55 70.9839 112.423 69.1032 112.423 66.7833V19.2006C112.423 16.8807 110.55 15 108.239 15Z" fill="var(--color-brand-800)" stroke="#263238" strokeWidth="1.29468" strokeLinecap="round" strokeLinejoin="round"/>
                        <path d="M70.2528 41.5945L112.423 20.1737V19.1938C112.423 18.0797 111.982 17.0113 111.198 16.2235C110.413 15.4357 109.349 14.9932 108.239 14.9932H24.9463C23.8367 14.9932 22.7725 15.4357 21.9878 16.2235C21.2031 17.0113 20.7623 18.0797 20.7623 19.1938V20.3538L62.1647 41.6882C63.4274 42.3046 64.8158 42.617 66.2198 42.6008C67.6238 42.5845 69.0046 42.24 70.2528 41.5945V41.5945Z" fill="white" stroke="#263238" strokeWidth="1.29468" strokeLinecap="round" strokeLinejoin="round"/>
                        <path d="M111.892 33.6549C119.684 32.3795 124.97 25.0043 123.7 17.1819C122.43 9.35954 115.084 4.05212 107.292 5.32748C99.5005 6.60283 94.214 13.978 95.4843 21.8004C96.7547 29.6228 104.101 34.9302 111.892 33.6549Z" fill="#FFE0D6"/>
                        <path d="M118.218 13.0928L106.502 24.8545L101.177 19.5083" stroke="var(--color-brand-800)" strokeWidth="2.94184" strokeLinecap="round" strokeLinejoin="round"/>
                    </svg>
                </div>
            </div>
            <p className="text-sm text-gray-500 dark:text-gray-400">
                {t("pages.signUpConfirm.emailConfirmedDescription")}
            </p>
        </div>
        <Button
            size="md" 
            variant="primary"
            className="w-full"
            onClick={() => navigate("/signIn")}
        >
            {t("pages.signUpConfirm.signIn")}
        </Button>
    </>
}