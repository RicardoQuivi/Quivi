import { useTranslation } from "react-i18next";
import { useChannelContext } from "../../context/AppContextProvider";
import { Link } from "react-router";
import { Page } from "../../layout/Page";
import Footer from "../../layout/Footer";
import { ButtonsSection } from "../../layout/ButtonsSection";
import { Alert, Grid } from "@mui/material";
import { MethodsIcon, SecureIcon } from "../../icons";
import { MenuSelector } from "../../components/Menu/MenuSelector";
import MerchantHeader from "../../layout/MerchantHeader";
import { useAuth } from "../../context/AuthContext";

export const ChannelProfilePage = () => {
    const { t } = useTranslation();
    const channelContext = useChannelContext();
    const auth = useAuth();

    const getFooter = () => {
        if(channelContext.inactive)  {
            return <Footer />;
        }

        return <>
            <ButtonsSection>
                <MenuSelector /> 
                {
                    channelContext.features.freePayments.isActive &&
                    <Link className={`${channelContext.features.freePayments.isTipOnly ? "secondary-button" : "primary-button"} w-100`} to="/pay/FreePayment">
                    {
                        channelContext.features.freePayments.isTipOnly
                        ?
                        t("home.sendTip")
                        :
                        t("home.sendPayment")
                    }
                    </Link>
                }
                {
                    channelContext.features.allowsSessions == true &&
                    <Link className="primary-button w-100" to={`/c/${channelContext.channelId}/session/summary`}>
                        {channelContext.features.payAtTheTable.isActive ? `🤩 ${t("home.payBill")}` : t("home.seeBill")}
                    </Link>
                }
            </ButtonsSection>
            <Grid container spacing={2} style={{justifyContent: "center"}}>
                <Grid size={{xs: 12 }}>
                    <div className='home__secure'>
                        <SecureIcon height={16} width={16} />
                        <span>{t("home.secure")}</span>
                        <div className="home__methods" style={{ height: "1.5rem" }}>
                            <MethodsIcon style={{ height: "100%", aspectRatio: "452 / 62" }} />
                        </div>
                    </div>
                </Grid>
            </Grid>
            <Footer />
        </>
    }

    return <Page footer={getFooter()}>
        <MerchantHeader logo={channelContext.logoUrl} username={auth.user?.username}/>
        {
            channelContext.inactive &&
            <Alert severity="warning">{t("home.inactiveMerchantWarn")}</Alert>
        }
    </Page>
}