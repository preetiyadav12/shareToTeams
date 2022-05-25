const template = document.createElement("template");
template.innerHTML = `<div class="error-container">
<div class="error-descriptor">
</div>
</div>`;

export class ErrorDescriptorDialogue extends HTMLElement {
    private errorText:string;
    constructor(errorText: string)  {
        super();
        this.errorText = errorText;
        this.render();
    }

    render = () => {
        const dom = <HTMLElement>template.content.cloneNode(true);
        (<HTMLElement>this.querySelector(".error-descriptor")).innerText = this.errorText;
        this.appendChild(dom);
    }
}

customElements.define("error-descriptor-dialogue", ErrorDescriptorDialogue);