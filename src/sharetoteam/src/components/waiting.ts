const template = document.createElement("template");
template.innerHTML = `
    <div class="share-to-teams-waiting" style="display: none;">
        <div class="share-to-teams-waiting-overlay"></div>
        <div class="share-to-teams-waiting-indicator x72">
            <div class="share-to-teams-waiting-indicator-img x72"></div>
        </div>
    </div>`;

export class Waiting extends HTMLElement {
    constructor()  {
        super();
        this.render();
    }

    show = () => {
        (<HTMLElement>this.querySelector(".share-to-teams-waiting")).style.display = "block";
    };

    hide = () => {
        (<HTMLElement>this.querySelector(".share-to-teams-waiting")).style.display = "none";
    };

    render = () => {
        const dom = <HTMLElement>template.content.cloneNode(true);
        this.appendChild(dom);
    }
}

customElements.define("waiting-indicator", Waiting);