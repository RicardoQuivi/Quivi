import { FileExtension } from "./FileExtension";

export interface UploadImageRequest {
    readonly extension: FileExtension;
    readonly name: string;
    readonly base64Data: string;
}