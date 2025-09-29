import { UpsertAcquirerConfigurationRequest } from "./UpsertAcquirerConfigurationRequest";

export interface UpsertPaybyrdTerminalAcquirerConfigurationRequest extends UpsertAcquirerConfigurationRequest {
    readonly terminalId: string;
    readonly apiKey: string;
}