import { useAppContext } from "../../context/AppContextProvider";
import { useQuiviTheme } from "../../hooks/theme/useQuiviTheme";
import { Link, useNavigate } from "react-router";
import { ArrowLeftIcon, HomeIcon } from "../../icons";
import { NavActions } from "./NavActions";

interface Props {
    readonly title?: string;
    readonly hideCart?: boolean;
    readonly hideOrder?: boolean;
}
export const TableNav: React.FC<Props> = ({
    title,
    hideCart,
    hideOrder,
}) => {
    const theme = useQuiviTheme();
    const navigate = useNavigate();
    const appContext = useAppContext();
    
    return (
        <div className="container" style={{height: "fit-content"}}>
            <div className="page-title">
                {
                    title
                    ?
                        <div className="page-title__content" onClick={() => navigate(-1)} style={{ cursor: "pointer" }}>
                            <div className="nav__menu">
                                <ArrowLeftIcon width="60%" height="60%" />
                            </div>
                            <h2>{title}</h2>
                        </div>
                    :
                    <Link to={!appContext?.channelId ? "/user/home" : `/c/${appContext.channelId}`}>
                        <div className="nav__menu nav__menu--not-auth">
                            <HomeIcon fill={theme.primaryColor.hex} width="60%" height="60%" />
                        </div>
                    </Link>
                }
                <NavActions hideCart={hideCart} hideOrder={hideOrder}/>
            </div>
        </div>
    );
}