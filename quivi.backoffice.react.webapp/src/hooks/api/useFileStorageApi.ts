import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { UploadFileRequest } from "./Dtos/fileStorage/UploadFileRequest";
import { UploadFileResponse } from "./Dtos/fileStorage/UploadFileResponse";
import { UploadImageRequest } from "./Dtos/fileStorage/UploadImageRequest";

export const useFileStorageApi = () => {
    const httpClient = useHttpClient();

    const uploadFile = async (request: UploadFileRequest) => {
        const url = new URL(`api/Storage`, import.meta.env.VITE_API_URL).toString();
        return await httpClient.httpPost<UploadFileResponse>(url, request);
    };

    const uploadImage = async (request: UploadImageRequest) => {
        const url = new URL(`api/Storage/image`, import.meta.env.VITE_API_URL).toString();
        return await httpClient.httpPost<UploadFileResponse>(url, request);
    };


    const result = useMemo(() => ({
        uploadFile,
        uploadImage
    }), [httpClient]);

    return result;
}