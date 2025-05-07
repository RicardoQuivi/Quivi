import { Accept, useDropzone } from "react-dropzone";
import { useTranslation } from "react-i18next";
import { FileExtension } from "../../hooks/api/Dtos/fileStorage/FileExtension";
import { getExtension, getMimeType, getName } from "./helper";
import { useMemo, useState } from "react";
import { ClimbingBoxLoader } from "react-spinners";
import { UploadIcon } from "../../icons";

const imagesExtensions = new Set<FileExtension>([FileExtension.JPEG, FileExtension.JPG, FileExtension.PNG ]);
interface FileDropZoneProps {
    readonly allowedFiles: FileExtension[];
    readonly onFilesDroped: (files: File[], allowedExtensions: ExtensionSettings[]) => Promise<any>;
}

export interface ExtensionSettings {
    readonly name: string;
    readonly mimeType: string;
    readonly extension: string;
    readonly fileExtension: FileExtension;
}
const getSettingsFromExtensions = (extensions: FileExtension[]): ExtensionSettings[] => {
    let result = [] as ExtensionSettings[];

    for(const e of extensions){
        result.push({
            mimeType: getMimeType(e),
            name: getName(e),
            extension: getExtension(e),
            fileExtension: e,
        })
    }
    return result;
}

export const FileDropZone = (props: FileDropZoneProps) => {
    const { t } = useTranslation();

    const [isSubmitting, setIsSubmitting] = useState(false);
    const allowedExtensions = useMemo(() => getSettingsFromExtensions(props.allowedFiles), [props.allowedFiles]);
    const imagesOnly = useMemo(() => {
        for(const e of props.allowedFiles) {
            if(imagesExtensions.has(e) == false) {
                return false;
            }
        }
        return true;
    }, [props.allowedFiles]);

    const onDrop = async (files: File[]) => {
        setIsSubmitting(true);
        await props.onFilesDroped(files, allowedExtensions);
        setIsSubmitting(false);
    };

    const getAccept = (): Accept => {
        let accept: Accept = {};

        for(const e of allowedExtensions) {
            accept[e.mimeType] = [];
        }
        return accept;
    }
    const { getRootProps, getInputProps, isDragActive } = useDropzone({
        onDrop,
        accept: getAccept(),
    });

    return (
        <div
            className="transition border border-gray-300 border-dashed cursor-pointer dark:hover:border-brand-500 dark:border-gray-700 rounded-xl hover:border-brand-500"
            style={{
                position: "relative",
            }}
        >
            <form
                {...getRootProps()}
                className={`dropzone rounded-xl   border-dashed border-gray-300 p-7 lg:p-10
                    ${
                        isDragActive
                        ? "border-brand-500 bg-gray-100 dark:bg-gray-800"
                        : "border-gray-300 bg-gray-50 dark:border-gray-700 dark:bg-gray-900"
                    }
                    `}
            >
                <input {...getInputProps()} />

                <div className="dz-message flex flex-col items-center m-0!">
                    <div className="mb-[22px] flex justify-center">
                        <div className="flex h-[68px] w-[68px] items-center justify-center rounded-full bg-gray-200 text-gray-700 dark:bg-gray-800 dark:text-gray-400 fill-gray-700 dark:fill-gray-400">
                            <UploadIcon className="w-[50px] h-[50px]"/>
                        </div>
                    </div>

                    {/* Text Content */}
                    <h4 className="mb-3 font-semibold text-gray-800 text-theme-xl dark:text-white/90">
                        {
                            imagesOnly
                            ?
                            t("common.dropImageHere")
                            :
                            t("common.dropFilesHere")
                        }
                    </h4>
                    
                    <span className=" text-center mb-5 block w-full max-w-[290px] text-sm text-gray-700 dark:text-gray-400">
                        {t("common.dragAndDropFiles", {
                            files: allowedExtensions.map(e => e.name).join(", ")
                        })}
                    </span>

                    <span className="font-medium underline text-theme-sm text-brand-500">
                        {
                            imagesOnly
                            ?
                            t("common.browseImage")
                            :
                            t("common.browseFile")
                        }
                    </span>
                </div>
            </form>
            {
                isSubmitting &&
                <div
                    style={{
                        position: "absolute",
                        top: 0,
                        bottom: 0,
                        left: 0,
                        right: 0,
                        backgroundColor: "rgba(0, 0, 0, 0.1)",
                        zIndex: 1,
                        backdropFilter: "blur(4px)",
                        borderRadius: "12px",
                        display: "flex",
                        alignContent: "center",
                        justifyContent: "center",
                        flexWrap: "wrap",
                    }}
                >
                    <ClimbingBoxLoader size="100%" className="text-gray-200 stroke-brand-500 dark:text-gray-800"/>
                </div>
            }
        </div>
    );
}