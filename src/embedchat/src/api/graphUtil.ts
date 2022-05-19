import { Person } from "src/models/person";

export class GraphUtil {
    public static searchPeople = async (token: string, search: string) => {
        const resp = await fetch(`https://graph.microsoft.com/v1.0/me/people?$search=${search}`, {
            method: "GET",
            headers: new Headers({
                Authorization: "Bearer " + token,
            }),
        });
  
        if (!resp.ok) {
            return [];
        }
        const json = await resp.json();
        return json.value;
    };

    public static getOnlineMeeting = async (token: string, meetingId: string, userId: string) => {
        const resp = await fetch(`https://graph.microsoft.com/v1.0/users/${userId}/onlineMeetings/${meetingId}`, {
            method: "GET",
            headers: new Headers({
                Authorization: "Bearer " + token
            }),
        });
  
        if (!resp.ok) {
            return [];
        }
        const json = await resp.json();
        return json.value;
    };

    public static updateOnlineMeeting = async (token: string, meetingId: string, userId: string, participants:Person[]) => {
        const attendees:any[] = participants.map((p:Person, i:number) => {
            return {
                identity: {
                    "@odata.type": "#microsoft.graph.identitySet"
                },
                upn: p.userPrincipalName,
                role: "attendee"
            };
        });
        const payload = {
            participants: {
                attendees: attendees
            }
        }
        const resp = await fetch(`https://graph.microsoft.com/v1.0/users/${userId}/onlineMeetings/${meetingId}`, {
            method: "PATCH",
            headers: new Headers({
                Authorization: "Bearer " + token,
                ContentType: "application/json",
                Accept: "application/json"
            }),
            body: JSON.stringify(payload),
        });
  
        return resp.ok;
    };
}

  