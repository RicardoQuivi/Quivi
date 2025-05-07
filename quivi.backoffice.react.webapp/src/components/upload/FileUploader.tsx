import { useFileStorageApi } from "../../hooks/api/useFileStorageApi";
import { FileExtension } from "../../hooks/api/Dtos/fileStorage/FileExtension";
import { ExtensionSettings, FileDropZone } from "./FileDropZone";

interface FileUploaderProps {
    readonly allowedFiles: FileExtension[];
    readonly onUploaded: (name: string, url: string) => any;
}

export const FileUploader = (props: FileUploaderProps) => {
    const fileApi = useFileStorageApi();

    const submitFile = (file: File, allowedExtensions: ExtensionSettings[]) => new Promise<string>((resolve, reject) => {
        const reader = new FileReader()
        reader.onload = async (readerEvt) => {
            try{
                if(readerEvt.target == null) {
                    reject(null);
                    return;
                }

                const base64 = readerEvt.target.result as string;

                const aux = ";base64,";
                const index = base64.indexOf(aux);
                const mimeType = base64.substring(5, index);
                
                const extension = allowedExtensions.find(e => e.mimeType == mimeType)?.fileExtension;
                if(extension == undefined) {
                    throw Error(`${mimeType} is not supported. Please implement me.`);
                }

                const response = await fileApi.uploadFile({
                    base64Data: base64.substring(index + aux.length),
                    extension: extension,
                })

                resolve(response.data);
            } catch (e) {
                reject(e);
            }
        };
        reader.readAsDataURL(file);
    })

    return (
        <FileDropZone
            allowedFiles={props.allowedFiles}
            onFilesDroped={async (files, allowedExtensions) => {
                const promises = [] as Promise<any>[];
                for(const f of files) {
                    promises.push(submitFile(f, allowedExtensions).then(url => {
                        const lastIndex = f.name.lastIndexOf(".");
                        const name = f.name.substring(0, lastIndex);
                        props.onUploaded(name, url);
                    }));
                }
                await Promise.all(promises);
            }}
        />
    );
}