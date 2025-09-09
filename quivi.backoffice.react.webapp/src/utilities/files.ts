import saveAs from "file-saver";

export class Files {
    private static toByteArray = (b64Data: string) => {
        const sliceSize = 512;
        const byteCharacters = atob(b64Data);
        const byteArrays = [];

        for (let offset = 0; offset < byteCharacters.length; offset += sliceSize) {
            const slice = byteCharacters.slice(offset, offset + sliceSize);
            const byteNumbers = new Array(slice.length);

            for (let i = 0; i < slice.length; ++i) {
                byteNumbers[i] = slice.charCodeAt(i);
            }
            
            var byteArray = new Uint8Array(byteNumbers);
            byteArrays.push(byteArray);
        }
        return byteArrays;
    }

    static base64ToBlob = (b64Data: string, contentType: string) => {
        const byteArrays = this.toByteArray(b64Data);
        const blob = new Blob(byteArrays, { type: contentType });
        return blob;
    }

    static saveBase64File = (b64Data: string, filename: string, contentType: string): void => {
        const blob = this.base64ToBlob(b64Data, contentType);
        saveAs(blob, filename);
    }

    static saveFileFromURL = (url: string, filename: string): void => {
        saveAs(url, filename);
    }
}