import { Person } from "src/models/person";

const template = document.createElement("template");
template.innerHTML = `
    <div class="teams-embed-peoplepicker-selection-outer">
        <div class="teams-embed-peoplepicker-selection">
            <img class="teams-embed-peoplepicker-selection-img" src="" />
            <div class="teams-embed-peoplepicker-selection-name"></div>
            <div class="teams-embed-peoplepicker-selection-remove">
                <svg role="presentation" focusable="false" class="app-svg icons-close" viewBox="-6 -6 32 32">
                    <g class="icons-default-fill"><path class="icons-unfilled" d="M4.08859 4.21569L4.14645 4.14645C4.32001 3.97288 4.58944 3.9536 4.78431 4.08859L4.85355 4.14645L10 9.293L15.1464 4.14645C15.32 3.97288 15.5894 3.9536 15.7843 4.08859L15.8536 4.14645C16.0271 4.32001 16.0464 4.58944 15.9114 4.78431L15.8536 4.85355L10.707 10L15.8536 15.1464C16.0271 15.32 16.0464 15.5894 15.9114 15.7843L15.8536 15.8536C15.68 16.0271 15.4106 16.0464 15.2157 15.9114L15.1464 15.8536L10 10.707L4.85355 15.8536C4.67999 16.0271 4.41056 16.0464 4.21569 15.9114L4.14645 15.8536C3.97288 15.68 3.9536 15.4106 4.08859 15.2157L4.14645 15.1464L9.293 10L4.14645 4.85355C3.97288 4.67999 3.9536 4.41056 4.08859 4.21569L4.14645 4.14645L4.08859 4.21569Z"></path><path class="icons-filled" d="M3.89705 4.05379L3.96967 3.96967C4.23594 3.7034 4.6526 3.6792 4.94621 3.89705L5.03033 3.96967L10 8.939L14.9697 3.96967C15.2359 3.7034 15.6526 3.6792 15.9462 3.89705L16.0303 3.96967C16.2966 4.23594 16.3208 4.6526 16.1029 4.94621L16.0303 5.03033L11.061 10L16.0303 14.9697C16.2966 15.2359 16.3208 15.6526 16.1029 15.9462L16.0303 16.0303C15.7641 16.2966 15.3474 16.3208 15.0538 16.1029L14.9697 16.0303L10 11.061L5.03033 16.0303C4.76406 16.2966 4.3474 16.3208 4.05379 16.1029L3.96967 16.0303C3.7034 15.7641 3.6792 15.3474 3.89705 15.0538L3.96967 14.9697L8.939 10L3.96967 5.03033C3.7034 4.76406 3.6792 4.3474 3.89705 4.05379L3.96967 3.96967L3.89705 4.05379Z"></path></g>
                </svg>
            </div>
        </div>
    </div>`;

export class PeoplePickerSelection extends HTMLElement {
    constructor(person:Person, index:number, onRemove:any)  {
        super();
        
        const dom = <HTMLElement>template.content.cloneNode(true);
        (<HTMLImageElement>dom.querySelector(".teams-embed-peoplepicker-selection-img")).src = person.photo;
        (<HTMLElement>dom.querySelector(".teams-embed-peoplepicker-selection-name")).innerText = person.displayName;
        if (onRemove)
            (<HTMLElement>dom.querySelector(".teams-embed-peoplepicker-selection-remove")).addEventListener("click", onRemove);
        this.appendChild(dom);
    }
}

customElements.define("people-picker-selection", PeoplePickerSelection);