import { FileExtension } from "../../hooks/api/Dtos/fileStorage/FileExtension";

export const getMimeType = (extension: FileExtension): string => {
    switch(extension)
    {
        case FileExtension.JPEG: return "image/jpeg";
        case FileExtension.JPG: return "image/jpeg";
        case FileExtension.PNG: return "image/png";
        case FileExtension.PDF: return "application/pdf";
    }
}

export const getName = (extension: FileExtension): string => {
    switch(extension)
    {
        case FileExtension.JPEG: return "JPEG";
        case FileExtension.JPG: return "JPG";
        case FileExtension.PNG: return "PNG";
        case FileExtension.PDF: return "PDF";
    }
}

export const getExtension = (extension: FileExtension): string => {
    switch(extension)
    {
        case FileExtension.JPEG: return ".jpeg";
        case FileExtension.JPG: return ".jpg";
        case FileExtension.PNG: return ".png";
        case FileExtension.PDF: return ".pdf";
    }
}