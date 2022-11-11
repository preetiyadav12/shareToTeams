import { Waiting } from "../components/waiting";
import { AppSettings } from "../config/appSettings";
import { AuthInfo } from "../models";

export class AuthUtil {
  public static graphDefaultScope = "https://graph.microsoft.com/.default";
  public static async acquireToken(element: Element, resource: string, config: AppSettings, waiting: Waiting): Promise<AuthInfo | undefined> {
    // first try silent auth
    return new Promise((resolve, reject) => {
      this.acquireTokenSilent(element, resource, config).then((authInfo: AuthInfo) => {
        if (authInfo) resolve(authInfo);
        else {
          // requires an interactive login
          waiting.hide();
          let popupRef = window.open(
            `https://${config.publicHost}/auth.html?mode=interactive&resource=${encodeURIComponent(resource)}&client_id=${config.clientId}&host_uri=${config.publicHost}&tenant=${config.tenant}`,
            "Share to teams",
            "width=700,height=700,toolbar=yes",
          );

          // listen for response
          window.addEventListener("message", (e) => {
            (popupRef as Window).close();
            popupRef = null;
            resolve(e.data);
          });
        }
      });
    });
  }

  private static async acquireTokenSilent(element: Element, resource: string, config: AppSettings): Promise<AuthInfo> {
    return new Promise((resolve) => {
      const loginframe = document.createElement("iframe");
      loginframe.setAttribute(
        "src",
        `https://${config.publicHost}/auth.html?resource=${encodeURIComponent(resource)}&client_id=${config.clientId}&host_uri=${config.publicHost}&tenant=${config.tenant}`,
      );
      loginframe.style.display = "none";
      element.append(loginframe);
      window.addEventListener("message", (event) => {
        // TODO: enable line below...commented for easier token flush in testing
        //element.removeChild(loginframe);
        resolve(event.data);
      });
    });
  }
}
