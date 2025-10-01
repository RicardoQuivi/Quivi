import { useTranslation } from 'react-i18next';
import { useEffect, useState } from 'react';
import { Page } from '../../layout/Page';
import { useUserApi } from '../../hooks/api/useUserApi';
import { useNavigate, useSearchParams } from 'react-router';
import { LoadingAnimation } from '../../components/LoadingAnimation/LoadingAnimation';
import { ApiException } from '../../hooks/api/exceptions/ApiException';
import LoadingButton from '../../components/Buttons/LoadingButton';
import { useQuiviTheme } from '../../hooks/theme/useQuiviTheme';

const getCooldownEnd = () => new Date(new Date().getTime() + 30000)
enum ConfirmState {
    Confirming,
    Confirmed,
    Expired,
}
export const ConfirmEmailPage = () => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const userApi = useUserApi();

    const [confirmState, setConfirmState] = useState<ConfirmState>(ConfirmState.Confirming);
    const [searchParams] = useSearchParams();
    const [state, setState] = useState({
        email: searchParams.get('email') ?? undefined as string | undefined,
        code: searchParams.get('code') ?? undefined as string | undefined,
    })

    const [cooldownEnd, setCooldownEnd] = useState<Date | undefined>(getCooldownEnd);
    const [isSubmitting, setIsSubmitting] = useState(false);

    const onResend = async () => {
        if(state.email == undefined) {
            return;
        }

        try {
            setIsSubmitting(true);
            await userApi.confirm(state.email, "");
        }catch (e) {
            if(e instanceof ApiException) {
                setCooldownEnd(getCooldownEnd());
            }
        } finally {
            setIsSubmitting(false);
        }
    }

    useEffect(() => setState({
        email: searchParams.get('email') ?? undefined as string | undefined,
        code: searchParams.get('code') ?? undefined as string | undefined,
    }), [searchParams])

    useEffect(() => {
        if(cooldownEnd == undefined) {
            return;
        }

        const awaitTime = cooldownEnd.getTime() - new Date().getTime();
        if(awaitTime <= 0) {
            setCooldownEnd(undefined);
            return;
        }
        const timeout = setTimeout(() => setCooldownEnd(undefined), awaitTime);
        return () => clearTimeout(timeout);
    }, [cooldownEnd])

    const getFooter = (confirmState: ConfirmState): React.ReactNode => {
        switch(confirmState){
            case ConfirmState.Confirming:
                return undefined;
            case ConfirmState.Confirmed:
                return <LoadingButton
                    isLoading={false}
                    onClick={() => navigate("/user/login")}
                >
                    {t("confirmEmail.login")}
                </LoadingButton>
            case ConfirmState.Expired:
                return <LoadingButton
                    isLoading={isSubmitting}
                    onClick={onResend}
                    disabled={cooldownEnd != undefined || isSubmitting}
                >
                    {
                        cooldownEnd == undefined
                        ?
                        t("confirmEmail.resendConfirmation")
                        :
                        t("confirmEmail.resendAwait")
                    }
                </LoadingButton>
        }
    }

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
                return <TokenExpired />
        }
    }

    return <Page
        title={t("confirmEmail.title")}
        footer={getFooter(confirmState)}
    >
        { getBody(confirmState) }
    </Page>
};

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
            const timeout = setTimeout(props.onExpired, 3000);
            return () => clearTimeout(timeout);
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

    return <div className="authentication__header">
        <div className="container">
            <div className="flex flex-fd-c flex-ai-c mb-8">
                <LoadingAnimation />
            </div>
            <h1>{t("confirmEmail.confirmingYourEmail")}</h1>
            <p>{t("confirmEmail.confirmingYourEmailDescription")}</p>
        </div>
    </div>
}

const TokenExpired = () => {
    const { t } = useTranslation();

    return <div className="authentication__header">
        <div className="container">
            <div className="flex flex-fd-c flex-ai-c mb-8">
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
            <h1>{t("confirmEmail.tokenExpired")}</h1>
            <p>{t("confirmEmail.tokenExpiredDescription")}</p>
        </div>
    </div>
}

const EmailConfirmed = () => {
    const { t } = useTranslation();
    const theme = useQuiviTheme();

    return <div className="authentication__header">
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
            <h1>{t("confirmEmail.confirmed")}</h1>
            <p>{t("confirmEmail.confirmedDescription")}</p>
        </div>
    </div>
}