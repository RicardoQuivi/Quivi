import { useMemo } from "react";
import { HttpHelper } from "../../utilities/httpClient";

export const useHttpClient = () => {
    const state = useMemo(() => HttpHelper.Client, []);
    return state;
};