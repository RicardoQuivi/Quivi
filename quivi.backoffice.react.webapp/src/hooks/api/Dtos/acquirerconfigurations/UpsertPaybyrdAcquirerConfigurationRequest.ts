import { ChargeMethod } from "../ChargeMethod";
import { UpsertAcquirerConfigurationRequest } from "./UpsertAcquirerConfigurationRequest";

export interface UpsertPaybyrdAcquirerConfigurationRequest extends UpsertAcquirerConfigurationRequest {
    readonly method: ChargeMethod;
    readonly apiKey: string;
}