export enum ValidationErrorCode {
    Required = "Required",
    InvalidEmail = "InvalidEmail",
    InvalidPassword = "InvalidPassword",
    Expired = "Expired",
    InvalidCredentials = "InvalidCredentials",
}

export interface InvalidModelResponse {
    readonly property: any;
    readonly errorMessage: string;
    readonly errorCode: ValidationErrorCode;
    readonly context?: any
}