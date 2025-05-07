import { FileExtension } from "./FileExtension";

export interface UploadFileRequest {
    readonly extension: FileExtension;
    readonly base64Data: string;
}