import { useLocation, useNavigate } from "react-router";
import { useAuth } from "./context/AuthContextProvider";
import { useEffect } from "react";
import { useEmployeeManager } from "./context/employee/EmployeeContextProvider";
import { useTranslation } from "react-i18next";
import { LoadingAnimation } from "./components/Loadings/LoadingAnimation";

export const SignIn = () => {
    const { i18n } = useTranslation();
    const navigate = useNavigate();
    const location = useLocation();
    const auth = useAuth();
    const employeeManager = useEmployeeManager();

    useEffect(() => {
        const query = new URLSearchParams(location.search);
        const subjectToken = query.get("subjectToken");
        const language = query.get("language");
        if(!!language) {
            i18n.changeLanguage(language);
        }

        if(!!subjectToken) {
            employeeManager.signOut();
            
            auth.signIn(subjectToken).then(() => {
                navigate("/");
            })
            return;
        }

        navigate("/");
    }, [])

    return <LoadingAnimation
        sx={{
            width: {
                xs: "50%",
                sm: "25%",
            }
        }}
    />
}