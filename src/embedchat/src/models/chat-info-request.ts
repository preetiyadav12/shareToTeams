import { BaseModel } from "./base-model";
import { Person } from "./person";

export interface ChatInfoRequest extends BaseModel {
  entityId: string;
  owner: Person;
  accessToken: string;
  topic: string;
  threadId?: string;
  participants?: Person[];
}
