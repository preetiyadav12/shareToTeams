

export class GraphUtil {

  public static postInChannel = async(token: string) => { 

    fetch(`https://graph.microsoft.com/beta/teams/e69b0f5a-3351-4e47-bad8-038d0ec1fd8f/channels/19:CE8D6NVUnrFD9d6ipTzONeaBMXgH4VxSh8rPoN_Bix81@thread.tacv2/messages`, {
      method: "POST",
      body: JSON.stringify({ //adpative card 
        "body": {
          "contentType": "html",
          "content": "<attachment id=\"4465B062-EE1C-4E0F-B944-3B7AF61EAF40\"></attachment>"
        },
        "attachments": [
          {
            "id": "4465B062-EE1C-4E0F-B944-3B7AF61EAF40",
            "contentType": "application/vnd.microsoft.card.adaptive",
            "content": "{\n        \"type\": \"AdaptiveCard\",\n        \"$schema\": \"http://adaptivecards.io/schemas/adaptive-card.json\",\n        \"version\": \"1.3\",\n        \"body\": [\n          {\n            \"type\": \"TextBlock\",\n            \"size\": \"Large\",\n            \"weight\": \"Bolder\",\n            \"text\": \"ViewSonic VP2756-2K 27 Inch\",\n            \"wrap\": true\n          }\n        ],\n        \"actions\": [\n          {\n            \"type\": \"Action.OpenUrl\",\n            \"title\": \"View\",\n            \"url\": \"http://localhost:3000/products/62ba6ef9689d0263fb0bd8bc\"\n          }\n        ]\n      }"
          }
        ]
      }),
      headers: {
          "accept": "application/json",
          "content-type": "application/json",
          "Authorization": token
      }
    }).then((r:any) => {
        if (r.ok)
            return r.json();
    }).then((r:any) => {
        window.close();
    });
    }
}
