import React from "react";
import ReactDOM from "react-dom";
import "./index.css";
import Embed from "./Components/Embed";

// get all elements with the teams-embed-entityid attribute and bootstrap app to it
let scriptElements = document.querySelectorAll("[teams-embed-client-id]");
let embedElements = document.querySelectorAll("[teams-embed-entityid]");

if (scriptElements.length === 1)
{
    const clientId = scriptElements[0].getAttribute("teams-embed-client-id");
    const acsEndpoint = scriptElements[0].getAttribute("teams-embed-acs-endpoint");
    const acsAccountName = scriptElements[0].getAttribute("teams-embed-acs-account-name");
    embedElements.forEach((element:Element, index: number) => {
        console.log("bootstrap react");
        ReactDOM.render(
            <div className="wrapper" style={{display: "flex", height: "100%", width: "100%"}}>
                <Embed 
                    entityId={element.getAttribute("teams-embed-entityid")} 
                    chatTitle={element.getAttribute("teams-embed-chat-title")} 
                    clientId={clientId}
                    acsEndpoint={acsEndpoint}
                    acsAccountName={acsAccountName} />
            </div>,
            element
        );
    });
}

class teamsEmbeddedChat {
    public static renderEmbed(element:Element, entityId:string, chatTitle?:string) {
        // TODO...make this global
    }
}