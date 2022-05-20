import { Person } from "src/models/person";

const template = document.createElement("template");
template.innerHTML = `
    <li class="teams-embed-chat-item">
        <div class="teams-embed-avatar-container">
            <div class="teams-embed-avatar">
                <img class="teams-embed-avatar-image" src="https://fluentsite.z22.web.core.windows.net/0.51.7/public/images/avatar/small/ade.jpg">
            </div>
        </div>
        <div class="teams-embed-chat-item-message">
            <div class="teams-embed-chat-message">
                <div class="teams-embed-chat-message-header">
                    <span class="teams-embed-chat-message-author">Jane Doe</span>
                    <span class="teams-embed-chat-message-timestamp">Yesterday, 4:15 PM</span>
                </div>
                <div class="teams-embed-chat-message-content"></div>
            </div>
        </div>
    </li>`;

export class ChatItem extends HTMLElement {
    constructor(message: string)  {
        super();

        const dom = <HTMLElement>template.content.cloneNode(true);
        (<HTMLElement>dom.querySelector(".teams-embed-chat-message-content")).innerHTML = message;
        this.appendChild(dom);
    }

}

customElements.define("chat-item", ChatItem);