import { useLocation, useNavigate } from "react-router";
import { useAuth } from "./context/AuthContextProvider";
import { useEffect } from "react";
import { useEmployeeManager } from "./context/employee/EmployeeContextProvider";

export const SignIn = () => {
    const navigate = useNavigate();
    const location = useLocation();
    const auth = useAuth();
    const employeeManager = useEmployeeManager();

    useEffect(() => {
        const query = new URLSearchParams(location.search);
        const subjectToken = query.get('subjectToken');
        if(!!subjectToken) {
            employeeManager.signOut();
            
            auth.signIn(subjectToken).then(() => {
                navigate("/");
            })
            return;
        }

        navigate("/");
    }, [])

    return <></>
}