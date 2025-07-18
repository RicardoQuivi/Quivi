import { useState } from "react";
import { useAuth } from "../../context/AuthContext";
import { Tooltip } from "../ui/tooltip/Tooltip";
import { usePublicIdQuery } from "../../hooks/queries/implementations/usePublicIdQuery";
import { Spinner } from "../spinners/Spinner";

interface Props {
    readonly id?: string;
}
export const PublicId = (props: Props) => {
    const auth = useAuth();

    const [shouldLoad, setShouldLoad] = useState(false);
    const publicIdQuery = usePublicIdQuery(shouldLoad == false ? undefined : props.id);
    
    const getMessage = () => {
        if(shouldLoad == false || publicIdQuery.isFirstLoading) {
            return (
            <span
                className="inline-flex items-center gap-2"
            >
                Id:&nbsp;&nbsp;<Spinner className="inline-block" />
            </span>)
        }

        return `Id: ${publicIdQuery.data}`;
    }
    
    return <>
        {
            auth.isAdmin == false
            ?
            props.id
            :
            <Tooltip message={getMessage()} onOpen={() => setShouldLoad(true)}>
                {props.id}
            </Tooltip>    
        }
    </>
}