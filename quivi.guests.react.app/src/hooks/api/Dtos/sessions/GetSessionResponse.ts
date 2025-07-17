import type { DataResponse } from "../DataResponse";
import type { Session } from "./Session";

export interface GetSessionResponse extends DataResponse<Session | undefined> {

}