import { DataResponse } from "../DataResponse";
import { Availability } from "./Availability";

export interface PatchAvailabilityResponse extends DataResponse<Availability | undefined> {
    
}