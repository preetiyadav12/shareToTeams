import { ChatInfoRequest } from "src/models/chat-info-request";
import { AppSettings } from "../config/appSettings";
import { EntityState } from "../models/entity-state";

export class EntityApi {
  private readonly config: AppSettings;
  private readonly idToken: string;

  constructor(config: AppSettings, idToken: string) {
    this.config = config;
    this.idToken = idToken;

    console.log(`API Base Url: ${this.config.apiBaseUrl}`);
  }

  public getMapping = async (chatInfoRequest: ChatInfoRequest): Promise<EntityState | undefined> => {
    if (!chatInfoRequest) {
      throw new Error("Entity Id cannot be emtpy!");
    }

    const requestOptions = {
      method: "POST",
      headers: {
        Authorization: `Bearer ${this.idToken}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify(chatInfoRequest),
    };

    const resp = await fetch(`${this.config.apiBaseUrl}/api/entity`, requestOptions);

    console.log(resp);
    if (resp.ok) {
      const data = (await resp.json()) as EntityState;
      if (data.isSuccess) {
        return data;
      } else {
        return;
      }
    } else {
      if (resp.status === 404)
        // Entity is not found
        return undefined;
    }

    return await resp.json();
  };

  public createChat = async (chatInfoRequest: ChatInfoRequest): Promise<boolean> => {
    const requestOptions = {
      method: "POST",
      headers: new Headers({
        Authorization: `Bearer ${this.idToken}`,
        "Content-Type": "application/json",
      }),
      body: JSON.stringify(chatInfoRequest),
    };

    const resp = await fetch(`${this.config.apiBaseUrl}/api/entity/create`, requestOptions);

    return resp.ok;
  };
}
