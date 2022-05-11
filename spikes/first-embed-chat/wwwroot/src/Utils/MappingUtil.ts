import { Mapping } from "../Models/Mapping";
import { configUtil } from "./ConfigUtil";

export class mappingUtil {
    public static getMapping = async (entityId: string, idToken: string) => {
        var resp = await fetch(`https://${configUtil.HOST_DOMAIN}/api/mapping/${entityId}`, {
            method: "GET",
            headers: new Headers({
                "Authorization": "Bearer " + idToken
            }),
        });

        if (!resp.ok)
            return null;
        
        return await resp.json();
    };

    public static updateMapping = async (mapping: Mapping, idToken: string) => {
        var resp = await fetch(`https://${configUtil.HOST_DOMAIN}/api/mapping`, {
            method: "PATCH",
            body: JSON.stringify(mapping),
            headers: new Headers({
                "Authorization": "Bearer " + idToken,
                "accept": "application/json",
                "content-type": "application/json"
            }),
        });

        return resp.ok;
    };
}