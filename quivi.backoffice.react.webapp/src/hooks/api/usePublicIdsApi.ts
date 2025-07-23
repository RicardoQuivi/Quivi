import { useMemo } from "react";
import { GetPublicIdResponse } from "./Dtos/publicids/GetPublicIdResponse";
import { useAuthenticatedHttpClient } from "../../context/AuthContext";

export const usePublicIdsApi = () => {
    const client = useAuthenticatedHttpClient();
    
    const state = useMemo(() => ({
        get: (id: string) => {            
            const url = new URL(`api/publicIds/${id}`, import.meta.env.VITE_API_URL).toString();
            return client.get<GetPublicIdResponse>(url, {});
        },
    }), [client]);

    return state;
}