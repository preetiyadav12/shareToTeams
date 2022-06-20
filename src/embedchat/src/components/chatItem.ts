import { Message } from "src/models/message";

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
        (<HTMLElement>dom.querySelector(".teams-embed-chat-item")).classList.add(message.sender.id);
        (<HTMLImageElement>dom.querySelector(".teams-embed-avatar-image")).src = message.sender.photo;
        (<HTMLElement>dom.querySelector(".teams-embed-chat-message-author")).innerText = message.sender.displayName;
        (<HTMLElement>dom.querySelector(".teams-embed-chat-message-timestamp")).innerText = message.createdOn.toLocaleString();
        (<HTMLElement>dom.querySelector(".teams-embed-chat-message-content")).innerHTML = message.message;

        if (isMe) {
            (<HTMLElement>dom.querySelector(".teams-embed-chat-item")).classList.add("right");
            (<HTMLElement>dom.querySelector(".teams-embed-avatar-container")).remove();
        }
        
        this.appendChild(dom);
    }
}

customElements.define("chat-item", ChatItem);