import { Mapping } from "./Mapping";

export interface InitResponse {
    mapping: Mapping;
    acsToken: string;
}