import saveAs from "file-saver";

export class Files {
    static base64ToBlob = (b64Data: string, contentType: string) => {
        var sliceSize = 512;
        var byteCharacters = atob(b64Data);
        var byteArrays = [];

        for (var offset = 0; offset < byteCharacters.length; offset += sliceSize) {
            var slice = byteCharacters.slice(offset, offset + sliceSize);
            var byteNumbers = new Array(slice.length);

            for (var i = 0; i < slice.length; ++i)
                byteNumbers[i] = slice.charCodeAt(i);
                
            var byteArray = new Uint8Array(byteNumbers);
            byteArrays.push(byteArray);
        }
        const blob = new Blob(byteArrays, { type: contentType });
        return blob;
    }

    static saveBase64File = (b64Data: string, filename: string, contentType: string): void => {
        var sliceSize = 512;
        var byteCharacters = atob(b64Data);
        var byteArrays = [];

        for (var offset = 0; offset < byteCharacters.length; offset += sliceSize) {
            var slice = byteCharacters.slice(offset, offset + sliceSize);
            var byteNumbers = new Array(slice.length);

            for (var i = 0; i < slice.length; ++i)
                byteNumbers[i] = slice.charCodeAt(i);
                
            var byteArray = new Uint8Array(byteNumbers);
            byteArrays.push(byteArray);
        }
        const file = new File(byteArrays, filename, {type: contentType});
        saveAs(file);
    }

    static saveFileFromURL = (url: string, filename: string): void => {
        saveAs(url, filename);
    }
}