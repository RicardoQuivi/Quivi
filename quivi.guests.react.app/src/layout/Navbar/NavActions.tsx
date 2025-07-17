import { useLocation, useNavigate } from "react-router";
import { Trans, useTranslation } from "react-i18next";
import { useMemo, useState, type JSX } from "react";
import ReactDOMServer from 'react-dom/server';
import {  useChannelContext } from "../../context/AppContextProvider";
import { useQuiviTheme } from "../../hooks/theme/useQuiviTheme";
import { styled } from "@mui/material/styles";
import Badge, { type BadgeProps } from "@mui/material/Badge";
import Grid from "@mui/material/Grid";
import { useAuth } from "../../context/AuthContext";
import { CountryIcon } from "../../icons/CountryIcon";
import { CartIcon, CloseIcon, ProfileIcon, SettingsIcon } from "../../icons";
import Dialog from "../../components/Shared/Dialog";
import { ButtonsSection } from "../ButtonsSection";
import LoadingButton from "../../components/Buttons/LoadingButton";
import { useCart, useOrdersContext } from "../../context/OrderingContextProvider";
import { OrderState } from "../../hooks/api/Dtos/orders/OrderState";
import { Stack } from "@mui/material";
import { ExpandedMenu } from "../ExpandedMenu";

interface StyledBadgeProps extends BadgeProps {
    primarycolor: string
}

const StyledBadge = styled(Badge)((props: StyledBadgeProps) => ({
    "& .MuiBadge-colorPrimary": {
        backgroundColor: props.primarycolor,
    },
}));

const languages = [
    {
        id: "en",
        name: "English",
    },
    {
        id: "pt",
        name: "Português",
    }
]

const svgToDataUrl = (svgElement: JSX.Element) => {
    const svgString = ReactDOMServer.renderToStaticMarkup(svgElement);
    const encodedData = encodeURIComponent(svgString);
    return `url("data:image/svg+xml,${encodedData}")`;
};

interface Props {
    readonly hideOrder?: boolean;
    readonly hideCart?: boolean
}
export const NavActions: React.FC<Props> = ({
    hideOrder,
    hideCart,
}) => {
    const theme = useQuiviTheme();
    const location = useLocation();
    const navigate = useNavigate();
    const cart = useCart();
    const { t, i18n } = useTranslation();

    const channelContext = useChannelContext();
    const auth = useAuth();
    const features = channelContext.features;
    const isUserAccount = location.pathname.includes("/user")
    const userInitial = auth.user?.username.slice(0, 1).toUpperCase();
    const isAuth = auth.user != undefined;

    const ordersContext = useOrdersContext();

    const [languageDialogOpen, setLanguageDialogOpen] = useState(false);
    const [menuOpen, setMenuOpen] = useState(false);
    const currentFlagBackground = useMemo(() => svgToDataUrl(<CountryIcon language={i18n.language}/>), [i18n.language])

    const onOrderClick = () => {
        if(ordersContext.data.length == 1) {
            const order = ordersContext.data[0];
            navigate(`/c/${order.channelId}/orders/${order.id}/track`)
            return;
        }
        navigate(`/c/${channelContext.channelId}/orders`);
    };

    const onCartClick = () => navigate(`/c/${channelContext.channelId}/cart`);

    const shouldDisplayOrder = () => {
        if(hideOrder == true) {
            return false;
        }

        if(features.physicalKiosk == true) {
            return false;
        }

        if(ordersContext.data.length == 1) {
            const order = ordersContext.data[0];
            if(order.state == OrderState.Draft) {
                return false;
            }

            if(order.state == OrderState.Completed) {
                return false;
            }

            if(order.state == OrderState.Rejected) {
                return false;
            }
        }

        return ordersContext.data.length > 0;
    }

    return <>
        <Stack direction="row" spacing={1}>
             {
                shouldDisplayOrder() &&
                <button type="button" className="nav__menu nav__menu--live" style={{width: "auto", paddingLeft: "15px", paddingRight: "15px"}} onClick={onOrderClick}>
                    <StyledBadge color="primary" overlap="rectangular" primarycolor={theme.primaryColor.hex}>
                        <Trans
                            t={t}
                            i18nKey={ordersContext.data.length == 1 ? "navbar.seeOrder" : "navbar.seeOrders"}
                            values={{
                                count: ordersContext.data.length
                            }}
                            components={{
                                b:  <b style={{marginRight: "0.25rem"}}/>,
                            }}
                        />
                    </StyledBadge>
                </button>
            }
            {
                features.ordering.isActive && cart.totalItems > 0 && hideCart != true &&
                <button type="button" className="nav__menu nav__menu--not-auth" onClick={onCartClick}>
                    <StyledBadge
                        badgeContent={cart.totalItems}
                        color="primary"
                        overlap="rectangular"
                        primarycolor={theme.primaryColor.hex}
                        sx={{
                            width: "100%",
                            height: "100%",
                            display: "flex",
                            alignContent: "center",
                            justifyContent: "center",
                            flexWrap: "wrap"
                        }}
                    >
                        <CartIcon fill={theme.primaryColor.hex} width="60%" height="60%" />
                    </StyledBadge>
                </button>
            }
            <button type="button" className="nav__menu nav__menu--not-auth" onClick={() => setLanguageDialogOpen(true)} style={{
                backgroundImage: currentFlagBackground,
                backgroundSize: 'cover',
                backgroundRepeat: 'no-repeat',
                backgroundPosition: 'center'
            }}/>
            {
                features.physicalKiosk == false &&
                <>
                    {
                        !isAuth 
                        ?
                            <button type="button" className="nav__menu nav__menu--not-auth" onClick={() => setMenuOpen(true)}>
                                <ProfileIcon stroke={theme.primaryColor.hex} width="60%" height="60%" />
                            </button>
                        :
                        <>
                            {
                                isUserAccount 
                                ?
                                    <button type="button" className="nav__menu nav__menu--auth settings-icon" onClick={() => setMenuOpen(true)}>
                                        <SettingsIcon />
                                    </button>
                                :
                                    <button type="button" className="nav__menu nav__menu--auth" onClick={() => setMenuOpen(true)}>
                                        <span>{userInitial}</span>
                                    </button>
                            }
                        </>
                    }
                </>
            }
        </Stack>

        <Dialog isOpen={languageDialogOpen} onClose={() => setLanguageDialogOpen(false)}>
            <div className="container">
                <div className="modal__header mb-5" style={{alignItems: "baseline"}}>
                    <h3>{t("settings.chooseLanguage")}</h3>
                    <div className="close-icon" onClick={() => setLanguageDialogOpen(false)}>
                        <CloseIcon />
                    </div>
                </div>
                <Grid container spacing={2} className="mb-8" style={{}}>
                    {
                        languages.map(l =>
                            <Grid
                                key={l.id}
                                size={{ xs: 12, sm: 12, md: 6, lg: 6, xl: 6 }}
                                onClick={() => { 
                                    i18n.changeLanguage(l.id);
                                    setLanguageDialogOpen(false);
                                }}
                                style={{
                                    display: "flex",
                                    flexDirection: "row",
                                    justifyContent: "flex-start",
                                    cursor: "pointer",
                                }}
                            >
                                <CountryIcon
                                    language={l.id}
                                    style={{
                                        height: "48px",
                                        width: "64px",
                                        borderRadius: "3rem",
                                    }}
                                />
                                <div className="ml-3" style={{display: "flex", alignContent: "center", flexWrap: "wrap"}}>
                                    <p>{l.name}</p>
                                </div>
                            </Grid>
                        )
                    }
                </Grid>
                <ButtonsSection>
                    <LoadingButton isLoading={false} primaryButton={false} onClick={() => setLanguageDialogOpen(false)}>
                        {t("cancel")}
                    </LoadingButton>
                    {undefined}
                </ButtonsSection>
            </div>
        </Dialog>
        <ExpandedMenu isOpen={menuOpen} onClose={() => setMenuOpen(false)} />
    </>;
}