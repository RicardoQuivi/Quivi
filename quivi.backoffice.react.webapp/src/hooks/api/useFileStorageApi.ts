import { useMemo } from "react";
import { useHttpClient } from "./useHttpClient";
import { UploadFileRequest } from "./Dtos/fileStorage/UploadFileRequest";
import { UploadFileResponse } from "./Dtos/fileStorage/UploadFileResponse";
import { UploadImageRequest } from "./Dtos/fileStorage/UploadImageRequest";

export const useFileStorageApi = () => {
    const httpClient = useHttpClient();

    const uploadFile = async (request: UploadFileRequest) => {
        return await httpClient.httpPost<UploadFileResponse>(`${import.meta.env.VITE_API_URL}api/Storage`, request);
    };

    const uploadImage = async (request: UploadImageRequest) => {
        return await httpClient.httpPost<UploadFileResponse>(`${import.meta.env.VITE_API_URL}api/Storage/image`, request);
    };


    const result = useMemo(() => ({
        uploadFile,
        uploadImage
    }), [httpClient]);

    return result;
}