import { useTranslation } from "react-i18next";
import { useEffect, useMemo, useState } from "react";
import { Page } from "../../layout/Page";
import { ButtonsSection } from "../../layout/ButtonsSection";
import { Link, Navigate, useNavigate, useParams } from "react-router";
import { useOrdersQuery } from "../../hooks/queries/implementations/useOrdersQuery";
import { useQuiviTheme } from "../../hooks/theme/useQuiviTheme";
import { useChannelContext } from "../../context/AppContextProvider";
import { useAuth } from "../../context/AuthContext";
import { Colors } from "../../helpers/colors";
import { SquareButton } from "../../components/Buttons/SquareButton";
import { LoadingAnimation } from "../../components/LoadingAnimation/LoadingAnimation";
import { OrderState } from "../../hooks/api/Dtos/orders/OrderState";
import { OrderAndPayCanceled } from "./OrderAndPayCanceled";
import { OrderAndPaySuccess } from "./OrderAndPaySuccess";

interface AutoCloseProps {
    readonly startAt: number;
    readonly percentage: number;
}
const getPercentage = (startAt: number, duration: number) => {
    const now = new Date().getTime();
    const ex = now - startAt;

    return 100 - (ex > duration ? 0 : ex * 100 / duration);
}
const autoCloseDuration = 10000;
export const TrackOrderPage = () => {
    const { t } = useTranslation();
    const { orderId } = useParams<{orderId: string}>();
    const channelContext = useChannelContext();
    
    if(orderId == undefined) {
        return <Navigate to={`/c/${channelContext.channelId}`} replace />
    }

    const orderQuery = useOrdersQuery({
        ids: [orderId],
        page: 0,
    });
    const order = useMemo(() => orderQuery.data.length > 0 ? orderQuery.data[0] : undefined, [orderQuery.data])
    const theme = useQuiviTheme();
    const navigate = useNavigate();
    
    const features = channelContext.features;
    const auth = useAuth();

    const [autoCloseState, setAutoCloseState] = useState<AutoCloseProps>();

    useEffect(() => {
        if(autoCloseState == undefined) {
            return;
        }

        if(autoCloseState.percentage == 100) {
            if(order?.channelId != undefined) {
                navigate(`/c/${order.channelId}`);
                return
            }
            navigate("");
            return;
        }

        const interval = setTimeout(() => setAutoCloseState(p => p == undefined ? undefined :({
            ...p,
            percentage: getPercentage(p.startAt, autoCloseDuration),
        })), 10);
        return () => clearTimeout(interval);
    }, [autoCloseState])
    
    useEffect(() => {
        if(order == undefined) {
            return;
        }

        if(features.physicalKiosk == false) {
            return;
        }

        setAutoCloseState({
            percentage: 0,
            startAt: new Date().getTime(),
        })
    }, [order, features])

    const getSecondsLeft = (state: AutoCloseProps) => Math.max((autoCloseDuration - (new Date().getTime() - state.startAt)) / 1000, 0);

    const darkenColor = Colors.shadeColor(theme.primaryColor, 35);

    const getFooter = () => {
        if(order == undefined) {
            return;
        }
        if(autoCloseState == undefined) {
            if(auth.user == undefined) {
                return;
            }
            return <ButtonsSection>
                <Link to={`/c/${order.channelId}`} className="secondary-button mb-4">{t("paymentResult.home")}</Link>
                <Link to="/user/home" className="secondary-button">{t("paymentResult.seeAccount")}</Link>
            </ButtonsSection>;
        }
        
        return <ButtonsSection>
            <SquareButton style={{
                                background: `linear-gradient(90deg, ${darkenColor.hex} 0%, ${darkenColor.hex} ${autoCloseState.percentage.toFixed(2)}%, ${theme.primaryColor.hex} ${autoCloseState.percentage.toFixed(2)}%)`,
                                color: "white",
                            }}
                            color={darkenColor} 
                            showShadow
                            isLoading={false}
                            disabled={true}>
                {t("orderAndPayResult.closing")}... ({getSecondsLeft(autoCloseState).toFixed(0)})
            </SquareButton>
            {undefined}
        </ButtonsSection>
    }

    return <Page 
        title={orderQuery.isFirstLoading == false && orderQuery.data.length > 1 ? t("orderAndPayResult.title") : undefined}
        headerProps={{ hideCart: true, hideOrder: true }} 
        footer={getFooter()}
    >
        { 
            order != undefined
            ?
            (
                order.state != OrderState.Rejected
                ? 
                    <OrderAndPaySuccess order={order} />
                :
                    <OrderAndPayCanceled order={order} /> 
            )
            :
                <div className="loader-container">
                    <LoadingAnimation />
                </div>
        }
    </Page>
}