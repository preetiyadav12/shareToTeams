import { Person } from "src/models/person";

const template = document.createElement("template");
template.innerHTML = `
    <div class="teams-embed-people-item">
        <img class="teams-embed-people-item-img" src="" />
        <div class="teams-embed-people-item-name"></div>
    </div>`;

export class PeopleItem extends HTMLElement {
    constructor(person: Person, index: number, onSelectedEvent: any)  {
        super();

        const dom = <HTMLElement>template.content.cloneNode(true);
        (<HTMLImageElement>dom.querySelector(".teams-embed-people-item-img")).src = person.photo;
        (<HTMLElement>dom.querySelector(".teams-embed-people-item-name")).innerText = person.displayName;
        if (onSelectedEvent) {
            (<HTMLElement>dom.querySelector(".teams-embed-people-item")).addEventListener("click", onSelectedEvent);
        }
        this.appendChild(dom);
    }
}

customElements.define("people-item", PeopleItem);