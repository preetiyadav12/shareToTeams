import { GraphUtil } from "../api/graphUtil";
import { AuthInfo } from "src/models";
import { Person } from "src/models/person";
import { PeopleItem } from "./peopleItem";
import { PhotoUtil } from "src/api/photoUtil";

const template = document.createElement("template");
template.innerHTML = `
    <div class="teams-embed-input-mention-container" style="display: none;">

        <div class="teams-embed-mention-suggestions" style="">

            <div class="teams-embed-mention-waiting">
                <div class="teams-embed-waiting-indicator x24">
                    <div class="teams-embed-waiting-indicator-img x24"></div>
                </div>
            </div>
            <div class="teams-embed-mention-noresults" style="display: none;">
                We didn't find any matches
            </div>
        </div>
    </div>`;

//TODO: needs exclude list for existing members
export class PeoplePickerMention extends HTMLElement {
    private authInfo:AuthInfo;
    private searchResults:Person[];
    private selections:Person[];
    private photoUtil:PhotoUtil;
    private input: HTMLElement;
    constructor(authInfo:AuthInfo, photoUtil:PhotoUtil, elm: HTMLElement) {
        super();
        this.authInfo = authInfo;
        this.photoUtil = photoUtil;
        this.searchResults = [];
        this.selections = [];
        this.input = elm;
        this.render();
    }

    getSelections = () => {
        return this.selections;
    };

    personSelected = (selectedIndex:number) => {
        console.log(selectedIndex);

        const input = (<HTMLElement>document.querySelector(".teams-embed-footer-input"));
        const suggestionContainer = (<HTMLElement>document.querySelector(".teams-embed-mention-suggestions"));
        //const inputOuter = (<HTMLElement>document.querySelector(".teams-embed-peoplepicker-input"));
        
        input.innerText = "";
        suggestionContainer.style.display = "none";
        const selection:Person = this.searchResults[selectedIndex];
        this.selections.push(selection);
        this.searchResults = [];

        //const insertIndex = this.selections.length - 1;
        // picker.insertBefore(
        //     new PeoplePickerSelection(selection, insertIndex, this.personRemoved.bind(this, selection)), inputOuter);
    };

    // personRemoved = (selection:Person) => {
    //     const picker = (<HTMLElement>document.querySelector(".teams-embed-peoplepicker-input-wrapper"));
    //     const index = this.selections.indexOf(selection);
    //     this.selections.splice(index, 1);
    //     picker.removeChild(picker.children[index]);
    // };

    render = () => {
        //<at id=\"0\">Brennen Cage</at>
        const dom = <HTMLElement>template.content.cloneNode(true);
        const input = (<HTMLElement>document.querySelector(".teams-embed-footer-input"));
        //TODO: keydown for ENTER and TAB
        // const input = this.querySelector(".teams-embed-footer-input");
        // if (!input) return;
        input.addEventListener("keyup", async (evt: any) => {
            // get the suggestions pane DOM elements
            const suggestionContainer = (<HTMLElement>document.querySelector(".teams-embed-mention-suggestions"));
            const waiting = (<HTMLElement>document.querySelector(".teams-embed-mention-waiting"));
            const noresults = (<HTMLElement>document.querySelector(".teams-embed-mention-noresults"));
            //const input = (<HTMLElement>document.querySelector(".teams-embed-peoplepicker-input-ctrl"));
            
            // check what to do
            if (evt.key == "Escape" || evt.key == "TAB") {
                // close the suggestions
                this.searchResults = [];
                suggestionContainer.style.display = "none";
                waiting.style.display = "none";
                noresults.style.display = "none";
                this.input.innerText = "";
            } 
            else if (this.input.innerText.length > 1) {
                // display suggestions container and waiting indicator
                suggestionContainer.style.display = "block";
                waiting.style.display = "block";
                
                // call graph to get matches
                const results = await GraphUtil.searchPeople(this.authInfo.accessToken, this.input.innerText.replace('@', ''));

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
                        const person:Person = { id: obj.id, displayName: obj.displayName, userPrincipalName: obj.userPrincipalName, photo: this.photoUtil.emptyPic };
                        this.searchResults.push(person);
                        const peopleItem = new PeopleItem(person, i, this.personSelected.bind(this, i));
                        suggestionContainer.insertBefore(peopleItem, waiting);
                        this.photoUtil.getGraphPhotoAsync(this.authInfo.accessToken, person.id).then((pic:string) => {
                            person.photo = pic;
                            peopleItem.refresh(person);
                        });
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

customElements.define("people-picker-mention", PeoplePickerMention);