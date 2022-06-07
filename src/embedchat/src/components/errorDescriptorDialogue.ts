const template = document.createElement("template");
template.innerHTML = `<div class="error-container">
<div class="error-descriptor">
<span class="error-txt"></span>
<button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarSupportedContent" aria-controls="navbarSupportedContent" aria-expanded="false">
      Close
</button>
</div>
</div>`;

export class ErrorDescriptorDialogue extends HTMLElement {
    constructor()  {
        super();
        this.render();
    }

    printError = (errorText: string) => {
      // var btn = document.createElement("button");
      // btn.innerText = ;
      (<HTMLElement>this.querySelector(".error-txt")).innerText = errorText;
    }

    render = () => {
        const dom = <HTMLElement>template.content.cloneNode(true);
        (<HTMLElement>dom.querySelector(".navbar-toggler")).addEventListener('click', () => {
          (<HTMLElement>this.querySelector(".error-container")).style.display = "none";
        });
        this.appendChild(dom);
    }
}

customElements.define("error-descriptor-dialogue", ErrorDescriptorDialogue);