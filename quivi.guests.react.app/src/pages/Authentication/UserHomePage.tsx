import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Page } from '../../layout/Page';
import { LoadingAnimation } from '../../components/LoadingAnimation/LoadingAnimation';

export const UserHomePage = () => {
    const { t } = useTranslation();
    
    const homeEmptyStateImage = `/quivi.svg`;
    const [imageIsLoaded, setImageIsLoaded] = useState(false);
    //const wallet = useWallet();
    // const theme = useQuiviTheme();
    // const [balance, setBalance] = useState<number | null>(wallet.isLoading ? null : wallet.balance);

    // useEffect(() => {
    //     if (wallet.isLoading) {
    //         return;
    //     }

    //     setBalance(wallet.balance);
    // }, [wallet.isLoading, wallet.balance])

    
    return <Page
        title=''
        headerProps={{
            ordering: false,
        }}
    >
        {
            !imageIsLoaded &&
            <section className="user body loading">
                <div>
                    <LoadingAnimation />
                </div>
            </section>
        }
        <section className={!imageIsLoaded ? "invisible" : "user"}>
            {/* <h1>{t("paymentMethods.yourWallet")}</h1>
            <p style={{ verticalAlign: "bottom" }}>
                <span
                    style={{
                        fontSize: "49px",
                        fontWeight: 400,
                        lineHeight: "59px",
                        letterSpacing: "-0.02em",
                        textAlign: "left",
                    }}
                >
                    {
                        balance == null 
                        ?
                            <Skeleton variant="text" animation="wave" height="68px" width="30%" style={{ display: "inline-flex" }} />
                        :
                            Formatter.amount(balance)
                    }
                    &nbsp;
                </span>
                <span
                    style={{
                        fontSize: "30px",
                        fontWeight: 500,
                        lineHeight: "59px",
                        textAlign: "left",
                    }}
                >
                    €
                </span>
            </p>
            <p>
                <span style={{ cursor: "pointer", textDecoration: "underline" }} onClick={() => navigator.goTo(b => b.profile.WalletTopUpUrl())}>{t("paymentMethods.topupHere")}</span>
            </p> */}

            <div className="user__empty">
                <img src={homeEmptyStateImage} alt="Cards" onLoad={() => setImageIsLoaded(true)}/>
            </div>

            <h1>{t("userHome.title")}</h1>
            <p className="mb-4">{t("userHome.soon")}</p>
        </section>
    </Page>
};