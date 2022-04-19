import { Mapping } from "../Models/Mapping";

export class ApiUtil {

    public static getMapping = async (entityId: string, idToken: string) => {
        const resp = await fetch(`/api/mapping/${entityId}`, {
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
        const resp = await fetch(`/api/mapping`, {
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