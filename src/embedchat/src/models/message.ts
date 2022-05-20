import { Person } from "./person";

export interface Message {
    id: string;
    message: string;
    sender: Person;
    threadId: string;
    type: string;
    version: string;
    createdOn: Date;
}
  