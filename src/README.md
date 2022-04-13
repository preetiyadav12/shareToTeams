# Microsoft Teams Embedded Chat
The Microsoft Teams Embedded Chat solution allows you to embed a Microsoft Teams group chat into websites based on a unique identifier for that application (ex: Purchase Order Number, Customer ID, Ticket Number, etc). It uses Azure Communication Services to retrieve messages and subscribe to chat notifications and Microsoft Graph to post new messages, add participants, and display profile information of participants.

## Architecture
TODO

## Getting Started

1. Provision **Azure Communication Services** and note the **endpoint** and **connection string** information

1. Provision an Azure AD application with the **Microsoft Graph delegated permissions** for:

    - Chat.ReadWrite
    - email
    - offline_access
    - openid
    - profile
    - User.Read
    - User.ReadBasic.All

1. Update the Azure AD app registration to have a **single-page application** with a redirect pointing to `auth.html` at the root of your host domain (ex: https://localhost:5001/auth.html or https://somedomain.azurewebsites.net/auth.html)

1. Install client dependencies by running `npm install`

1. Create a `appsettings.development.json` file and populate it with the appropriate settings (see `appsettings.json` for example)

1. Create a `.env.dev` and `.env.prod` files and populate it with the appropriate settings (see `.env` for example)

1. Build the client-side bundle by running `npm run build`

## Using the Embedded Chat
Using the embedded chat in an existing application is a simple two step process...add a script reference to the embed script and bootstrap the chat to a DOM element.

1. Add script reference to the embed script

```
<script src="https://myhostdomain/dist/teamsembeddedchat.min.js"></script>
```

1. Bootstrapping the embedded chat to a DOM element can be done in two ways:

    - Via element attributes
    ```
    <div teams-embed-entity-id="000342" teams-embed-chat-topic="Purchase Order 000342"></div>
    ```

    - Via script
    ```
    chat = new teamsEmbeddedChat();
    chat.renderEmbed(document.getElementById("embed"), "0000342", "Purchase Order 0000342");
    ```