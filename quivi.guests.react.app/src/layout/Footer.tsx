import { useAppContext } from "../context/AppContextProvider";
import { useQuiviTheme } from "../hooks/theme/useQuiviTheme";
import { QuiviIcon } from "../icons";

const Footer = () => {
    const theme = useQuiviTheme();
    const appContext = useAppContext();

    return (
        <div className={`footer ${appContext == undefined ? "footer--not-table" : "footer--at-table"}`}>
            {
                appContext != undefined &&
                <p>{appContext.channelFullName}</p>
            }
            <a href="https://www.quivi.com" className="footer__powered">
                <QuiviIcon fill={theme.primaryColor.hex} height="100%" style={{ height: "50px", width: "auto" }} />
            </a>
        </div>
    );
};
export default Footer;