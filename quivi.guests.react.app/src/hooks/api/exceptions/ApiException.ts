import type { InvalidModelResponse } from "./InvalidModelResponse";

export class ApiException {
    constructor(public readonly errors: InvalidModelResponse[]) {

    }
}