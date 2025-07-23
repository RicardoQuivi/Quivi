import { useMemo } from "react";
import { HttpHelper } from "../../helpers/httpClient";

export const useHttpClient = () => {
    const state = useMemo(() => HttpHelper.Client, []);
    return state;
};