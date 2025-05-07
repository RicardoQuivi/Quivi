export class UploadHandler<T> {
    private getPromise: () => Promise<T>;
    private promise: Promise<T> | undefined;
    private callbacks: Set<(url: T) => any>;

    constructor(getPromise: () => Promise<T>) {
        this.getPromise = getPromise;
        this.promise = undefined;
        this.callbacks = new Set<(url: T) => any>();
    }

    public onDone(callback: (url: T) => any) {
        this.callbacks.add(callback);
    }

    public getUrl(): Promise<T> {
        if(this.promise == undefined) {
            this.promise = this.getPromise().then(result => {
                this.callbacks.forEach(s => s(result));
                return result;
            });
        }

        return this.promise;
    }
}