export interface Review {
    readonly id: string;
    readonly stars: number;
    readonly comment?: string;
}