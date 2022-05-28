import { ButtonPage } from "../components/buttonPage";
import { AuthInfo } from "src/models";
import { AppSettings } from "../config/appSettings";
import { Waiting } from "src/components/waiting";

export class AuthUtil {
  public static async acquireToken(
    element: Element,
    config: AppSettings,
    waiting: Waiting,
  ): Promise<AuthInfo | undefined> {
    // first try silent auth
    return new Promise((resolve, reject) => {
      this.acquireTokenSilent(element, config).then((authInfo: AuthInfo) => {
        if (authInfo) resolve(authInfo);
        else {
          // requires an interactive login
          waiting.hide();
          const btn = new ButtonPage("Sign-in to Microsoft Teams", () => {
            // launch popup
            waiting.show();
            let popupRef = window.open(
              `https://${config.hostDomain}/auth.html?mode=interactive&client_id=${config.clientId}&host_uri=${config.hostDomain}&tenant=${config.tenant}`,
              "Teams Embed",
              "width=700,height=700,toolbar=yes",
            );

            // listen for response
            window.addEventListener("message", (e) => {
              element.removeChild(btn); //TODO: check if successful
              (popupRef as Window).close();
              popupRef = null;
              resolve(e.data);
            });

            // set timer in case they close the window
            const timer = setInterval(() => {
              if (popupRef && popupRef.closed) {
                clearInterval(timer);
                reject();
              }
            }, 500);
          });
          element.append(btn);
        }
      });
    });
  }

  private static async acquireTokenSilent(element: Element, config: AppSettings): Promise<AuthInfo> {
    return new Promise((resolve) => {
      const loginframe = document.createElement("iframe");
      loginframe.setAttribute(
        "src",
        `https://${config.hostDomain}/auth.html?client_id=${config.clientId}&host_uri=${config.hostDomain}&tenant=${config.tenant}`,
      );
      loginframe.style.display = "none";
      element.append(loginframe);
      window.addEventListener("message", (event) => {
        //element.removeChild(loginframe);
        resolve(event.data);
      });
    });
  }
}
