import { Message } from "src/models/message";
import { Person } from "src/models/person";

const template = document.createElement("template");
template.innerHTML = `
    <li class="teams-embed-chat-item">
        <div class="teams-embed-avatar-container">
            <div class="teams-embed-avatar">
                <img class="teams-embed-avatar-image" src="">
            </div>
        </div>
        <div class="teams-embed-chat-item-message">
            <div class="teams-embed-chat-message">
                <div class="teams-embed-chat-message-header">
                    <span class="teams-embed-chat-message-author"></span>
                    <span class="teams-embed-chat-message-timestamp"></span>
                </div>
                <div class="teams-embed-chat-message-content"></div>
            </div>
        </div>
    </li>`;

export class ChatItem extends HTMLElement {
    constructor(message: Message, isMe: boolean)  {
        super();
        const dom = <HTMLElement>template.content.cloneNode(true);
        if (isMe)
            (<HTMLElement>this.querySelector(".teams-embed-chat-item")).className = "teams-embed-chat-item right";
        this.appendChild(dom);
        this.refresh(message);
    }

    refresh = (message:Message) => {
        (<HTMLImageElement>this.querySelector(".teams-embed-avatar-image")).src = message.sender.photo;
        (<HTMLElement>this.querySelector(".teams-embed-chat-message-author")).innerText = message.sender.displayName;
        (<HTMLElement>this.querySelector(".teams-embed-chat-message-timestamp")).innerText = message.createdOn.toLocaleString();
        (<HTMLElement>this.querySelector(".teams-embed-chat-message-content")).innerHTML = message.message;
    };
}

customElements.define("chat-item", ChatItem);