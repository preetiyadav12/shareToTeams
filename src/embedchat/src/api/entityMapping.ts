import { AppSettings } from "../config/appSettings";
import { ChatInfoRequest, EntityState } from "../models";

export class EntityApi {
  private readonly config: AppSettings;
  private readonly idToken: string;

  constructor(config: AppSettings, idToken: string) {
    this.config = config;
    this.idToken = idToken;

    console.log(`API Base Url: ${this.config.apiBaseUrl}`);
  }

  public getMapping = async (chatInfoRequest: ChatInfoRequest): Promise<EntityState | any> => {
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
      if (resp.status === 200) {
        const data = (await resp.json()) as EntityState;
        return data;
      } else if (resp.status === 500) {
        const data = await resp.json();
        console.log(data.Content);
        return null;
      }
    } else {
      if (resp.status === 404) {
        // Entity is not found
        console.log(`No entity mapping found for this entity: ${chatInfoRequest.entityId}`);
        return null;
      }
      throw new Error(resp.statusText);
    }
  };

  public createChat = async (chatInfoRequest: ChatInfoRequest): Promise<EntityState | null> => {
    const requestOptions = {
      method: "POST",
      headers: new Headers({
        Authorization: `Bearer ${this.idToken}`,
        "Content-Type": "application/json",
      }),
      body: JSON.stringify(chatInfoRequest),
    };

    try {
      const resp = await fetch(`${this.config.apiBaseUrl}/api/entity/create`, requestOptions);

      if (resp.ok) {
        return await resp.json();
      }
    } catch (error) {
      const err = error as Error;
      if (err) {
        console.log(err.message);
      }
      throw error;
    }

    return null;
  };
}
