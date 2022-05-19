import { GraphUtil } from "../api/graphUtil";
import { AuthInfo } from "src/models";
import { Person } from "src/models/person";
import { PeopleItem } from "./peopleItem";
import { PeoplePickerSelection } from "./peoplePickerSelection";

const template = document.createElement("template");
template.innerHTML = `
    <div class="teams-embed-peoplepicker">
        <div class="teams-embed-peoplepicker-input">
            <div class="teams-embed-peoplepicker-input-ctrl" contenteditable="true"></div>
        </div>
        <div class="teams-embed-peoplepicker-suggestions" style="display: none">

            <div class="teams-embed-peoplepicker-waiting">
                <div class="teams-embed-waiting-indicator x24">
                    <div class="teams-embed-waiting-indicator-img x24"></div>
                </div>
            </div>
            <div class="teams-embed-peoplepicker-noresults" style="display: none;">
                We didn't find any matches
            </div>
        </div>
    </div>`;

//TODO: needs exclude list for existing members
export class PeoplePicker extends HTMLElement {
    private authInfo:AuthInfo;
    private searchResults:Person[];
    private selections:Person[];
    constructor(authInfo:AuthInfo) {
        super();
        this.authInfo = authInfo;
        this.searchResults = [];
        this.selections = [];
        this.render();
    }

    getSelections = () => {
        return this.selections;
    };

    personSelected = (selectedIndex:number) => {
        console.log(selectedIndex);

        const input = (<HTMLElement>document.querySelector(".teams-embed-peoplepicker-input-ctrl"));
        const suggestionContainer = (<HTMLElement>document.querySelector(".teams-embed-peoplepicker-suggestions"));
        const picker = (<HTMLElement>document.querySelector(".teams-embed-peoplepicker"));
        const inputOuter = (<HTMLElement>document.querySelector(".teams-embed-peoplepicker-input"));
        
        input.innerText = "";
        suggestionContainer.style.display = "none";
        const selection:Person = this.searchResults[selectedIndex];
        this.selections.push(selection);
        this.searchResults = [];

        const insertIndex = this.selections.length - 1;
        picker.insertBefore(
            new PeoplePickerSelection(selection, insertIndex, this.personRemoved.bind(this, selection)), inputOuter);
    };

    personRemoved = (selection:Person) => {
        const picker = (<HTMLElement>document.querySelector(".teams-embed-peoplepicker"));
        const index = this.selections.indexOf(selection);
        this.selections.splice(index, 1);
        picker.removeChild(picker.children[index]);
    };

    render = () => {
        const dom = <HTMLElement>template.content.cloneNode(true);
        //TODO: keydown for ENTER and TAB
        (<HTMLElement>dom.querySelector(".teams-embed-peoplepicker-input-ctrl")).addEventListener("keyup", async (evt:any) => {
            // get the suggestions pane DOM elements
            const suggestionContainer = (<HTMLElement>document.querySelector(".teams-embed-peoplepicker-suggestions"));
            const waiting = (<HTMLElement>document.querySelector(".teams-embed-peoplepicker-waiting"));
            const noresults = (<HTMLElement>document.querySelector(".teams-embed-peoplepicker-noresults"));
            const input = (<HTMLElement>document.querySelector(".teams-embed-peoplepicker-input-ctrl"));
            
            // check what to do
            if (evt.key == "Escape" || evt.key == "TAB") {
                // close the suggestions
                this.searchResults = [];
                suggestionContainer.style.display = "none";
                waiting.style.display = "none";
                noresults.style.display = "none";
                input.innerText = "";
            }
            else if (input.innerText.length > 1) {
                // display suggestions container and waiting indicator
                suggestionContainer.style.display = "block";
                waiting.style.display = "block";
                
                // call graph to get matches
                const results = await GraphUtil.searchPeople(this.authInfo.accessToken, input.innerText);

                // clear any old suggestions
                while (suggestionContainer.children.length > 2) {
                    suggestionContainer.removeChild(suggestionContainer.children[0]);
                }

                // parse the results
                this.searchResults = [];
                const selectedIds = this.selections.map((p:Person, i:number) => {
                    return p.id;
                });
                results.forEach((obj:any, i:number) => {
                    if (selectedIds.indexOf(obj.id) === -1) {
                        const person:Person = { id: obj.id, displayName: obj.displayName, userPrincipalName: obj.userPrincipalName, photo: "https://www.ugx-mods.com/forum/Themes/UGX-Mods/images/default-avatar.png" };
                        this.searchResults.push(person);
                        suggestionContainer.insertBefore(new PeopleItem(person, i, this.personSelected.bind(this, i)), waiting);
                    }
                });

                // show no results if empty
                if (this.searchResults.length === 0) {
                    noresults.style.display = "block";
                }

                // update the UI
                waiting.style.display = "none";
            }
        });
        
        this.appendChild(dom);
    }
}

customElements.define("people-picker", PeoplePicker);