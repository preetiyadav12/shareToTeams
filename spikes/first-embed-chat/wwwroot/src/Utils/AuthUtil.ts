import { configUtil } from "./ConfigUtil";

export class authUtil {
    public static async acquireToken(element:Element) : Promise<any> {
        // first try silent auth
        return new Promise((resolve, reject) => {
            this.acquireTokenSilent(element).then((token) => {
                if (token)
                    resolve(token);
                else {
                    // requires an interactive login
                    var btn = document.createElement("button");
                    btn.innerText = "Sign-in";
                    btn.addEventListener("click", () => {
                        var popupRef = window.open(`https://${configUtil.HOST_DOMAIN}/auth.html?mode=interactive`, "Teams Embed", "width=700,height=700,toolbar=yes");
                        window.addEventListener("message", (e) => {
                            element.removeChild(btn); //TODO: check if successful
                            popupRef.close();
                            popupRef = null;
                            resolve(e.data);
                        });
                        let t = setInterval(() => {
                            if (popupRef && popupRef.closed) {
                                clearInterval(t);
                                resolve(null);
                            }
                        }, 500);
                    });
                    element.append(btn);
                }
            });
        });
    }

    private static async acquireTokenSilent(element:Element) : Promise<any> {
        return new Promise((resolve, reject) => {
            let loginframe = document.createElement("iframe");
            loginframe.setAttribute("src", `https://${configUtil.HOST_DOMAIN}/auth.html`);
            loginframe.style.display = "none";
            element.append(loginframe);
            window.addEventListener("message", (event) => {
                //element.removeChild(loginframe);
                resolve(event.data);
            });
        });
    }
}