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

  public getMapping = async (entityId: string): Promise<EntityState | undefined> => {
    if (entityId.length === 0) {
      throw new Error("Entity Id cannot be emtpy!");
    }

    const entityRequest: EntityState = {
      entityId,
    };

    const requestOptions = {
      method: "POST",
      headers: {
        Authorization: `Bearer ${this.idToken}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify(entityRequest),
    };

    const resp = await fetch(`${this.config.apiBaseUrl}/api/entity/mapping`, requestOptions);

    if (!resp.ok) return undefined;

    return await resp.json();
  };

  public updateMapping = async (entity: EntityState): Promise<boolean> => {
    const requestOptions = {
      method: "POST",
      headers: new Headers({
        Authorization: `Bearer ${this.idToken}`,
        "Content-Type": "application/json",
      }),
      body: JSON.stringify(entity),
    };

    const resp = await fetch(`${this.config.apiBaseUrl}/api/entity/mapping/update`, requestOptions);

    return resp.ok;
  };
}
