# Embedded Chat Project Starter

## ✨ Features

- Creates both NPM and bundled packages providing flexibility for development
  scenarios
- Build is available for both **Development** and **Production** environments.
- Type definitions are automatically generated and shipped with your package.
  - > `types` field in package.json

## Prerequisites

- Visual Studio 2019 or later (to run API from your local machine)
- .NET 6.0 (can be installed with the latest Visual Studio Installer)
- Visual Studio Code
- Git client
- Azure CLI
- [Azurite](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite?toc=%2Fazure%2Fstorage%2Fblobs%2Ftoc.json&tabs=visual-studio)
  - > Note: The Azure Storage Emulator is depreciated. Azurite is automatically
    > installed with Visual Studio 2022, but for earlier versions of Visual
    > Studio, you'd need to install and run it manually
- nvm (to manage your node versions)
- node (version: 14.17.3)
- yarn

### Visual Studio Code Extensions

In order to make your development experience smooth and also to enable some of
the features required to run our project from your local machine, the following
list of Visual Studio Code Extensions is strongly recommended to be installed:

- Azure CLI Tools
- Azure Functions
- Azure Storage
- Azure Terraform
- Azure Tools
- Azurite
- Debugger for Microsoft Edge
- ESLint
- HashiCorp Terraform
- IntelliCode
- JavaSCript Debugger
- Jest
- Live Server
- Live Server Preview
- Material Theme
- Mermaid Editor
- Mermaid Preview
- Microsoft Edge Tools
- npm
- PowerShell
- Prettier - Code formatter
- Prettier Now
- REST Client
- Teams Toolkit
- Terminal
- Typescript Debugger
- YAML
- React Native Tools

## Local Settings

When you clone the repo to your local machine, obviously you'd miss a few
important files containing vital, but sensitive information that you'd need in
order to run this project locally. Here is the list of files you'd need to
re-create on your local machine before you can run the project:

- **API Local Settings**:
  1. Navigate to `/src/api` folder
  2. Clone existing `local.settings.json.template` file into a new file named
     `local.settings.json` file and fill all required values in this file.
- **Environment Variables for React App Sample app** (optional): if you plan on
  running a sample React App with the Embedded Chat control, you'd be required
  to create a local `.env` file in the same folder where the React App sample
  application is located. You can use the provided `.env.template` file to clone
  it into `.env` or `.env.development` file. Use your settings to fill the
  content of that file prior to building React App sample application.

## Use of NPM Scripts in the project

As much as we can we should always strive for automation. We can achieve some of
it by using some of the special scripts in npm: **prepare**, **prepublishOnly**,
**preversion**, **version** and **postversion**.

**prepare** will run both BEFORE the package is packed and published, and on
local `yarn install`. Perfect for running the project build for the very first
time.

`"prepare" : "yarn build:dev"`

**prepublishOnly** will run BEFORE **prepare** and ONLY on `yarn publish`. Here
we will run our **test** and **lint** to make sure we don’t publish bad code:

`"prepublishOnly" : "yarn test && yarn lint"`

**preversion** will run before bumping a new package version. To be extra sure
that we’re not bumping a version with bad code, why not run **lint** here as
well? 😃

`"preversion" : "yarn lint"`

**Version** will run after a new version has been bumped. **git commit** and a
new _version-tag_ will be made every time you bump a new version. This command
will run BEFORE the commit is made. One idea is to run the formatter here and so
no ugly code will pass into the new version:

`"version" : "yarn format && git add ."`

**Postversion** will run after the commit has been made. A perfect place for
pushing the commit as well as the tag.

`"postversion" : "git push && git push --tags"`

This is how the **scripts** section in package.json looks like:

```json
"scripts": {
    "clean": "rimraf dist",
    "prebuild": "npm run clean",
    "unpublish:local": "yarn unlink",
    "publish:local": "yarn link",
    "npm:local": "run-s unpublish:local publish:local",
    "prepare": "yarn build:dev",
    "prepublishOnly": "yarn test && yarn lint",
    "preversion": "yarn lint",
    "build:dev": "tsc --build ./tsconfig.build.json && webpack --config ./webpack.dev.config.ts",
    "build:deploy": "tsc --build ./tsconfig.build.json && webpack --config ./webpack.prod.config.ts",
    "build": "run-s build:dev npm:local",
    "build:prod": "run-s build:deploy npm:local",
    "lint": "eslint . --ext .ts",
    "lint:fix": "eslint . --ext .ts --fix",
    "format": "prettier --write \"src/**/*.ts\"",
    "version": "yarn format && git add .",
    "postversion": "git push && git push --tags",
    "test": "jest --config jestconfig.json"
}

```

## ✌️ Start Coding in 2 steps

1. Run `yarn install` When running the project build for the very first time,
   you need to build and publish the NPM package locally so it can be used in
   your sample projects. This is done by running **prepare** command as it
   explained earlier.

2. Then later, when you need to re-build your package, just simply use this
   command: `yarn build` or `yarn build:prod` for the production version of the
   package.

> Note: You can build the package for `development` or `production`
> environments. The `development` version will produce the unoptimized, not
> minimized version of the bundle files, while the `production` version will
> optimize and minize Javascript files in the bundle.

- To build the project for `development` environment, just use this command:
  `yarn build`

- To build the project for `production` environment, use this command: `yarn build:prod`

These commands will build the Embedded Chat project and also publish **locally**
the `@msteams/embedchat` NPM package.

## Publish Embedded Chat package to NPM

To package it into [NPM](https://www.npmjs.com) **globally**, you need to create
an NPM account.

If you don’t have an account you can do so at [NPM
Signup](https://www.npmjs.com/signup) or run the command: `npm adduser`

If you already have an account, run `npm login` to login to your NPM account.

Alright! Now run publish: `npm publish`

Yes that's it. Happy coding ! 🖖

## 💉 Consumption of published library

- Install it 🤖

```sh
yarn add @msteams/embedchat
# OR
npm install @msteams/embedchat
```

- Use it 💪

```html
<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="UTF-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>MS Teams Embedded Chat Sample</title>
    <link rel="stylesheet" href="./style.css" />
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <!-- <script src="https://msembedchatdevsta.blob.core.windows.net/$web/embedChat.js"></script> -->
    <script src="node_modules/@msteams/embedchat/dist/embedChat.js"></script>
    <script>
      $(document).ready(() => {
        var EmbeddedChat = MSTeamsExt.EmbeddedChat;

        var appSettings = {
          hostDomain: "msembedchatdevsta.blob.core.windows.net/$web",
          apiBaseUrl: "http://localhost:7071",
          clientId: "c7dffc2e-43bb-46c3-9e03-5dfd2116ff88",
          tenant: "8385af7f-4e3e-42b9-a17d-0a43eb16aefd",
          acsEndpoint: "endpoint=https://ms-demos-acs-embchat-dannyg.communication.azure.com/",
        };

        var chatTitleDiv = document.getElementById("title");
        var chatWindowDiv = document.getElementById("embed");
        var logsDiv = document.getElementById("log");

        var embchat = new EmbeddedChat(appSettings);

        var App = function (chat) {
          return `<h1>${chat.PrintName("Danny")}</h1>`;
        };

        var renderChat = (Chat, Root, title, where, what, log) => {
          log.innerHTML = `Entity Id: ${what}`;
          title.innerHTML = Root(Chat);
          Chat.renderEmbed(where, what)
            .then(() => {
              log.innerHTML = "Rendered successfully!";
            })
            .catch((error) => {
              log.innerHTML = error.message;
            });
        };

        renderChat(embchat, App, chatTitleDiv, chatWindowDiv, "0001", logsDiv);
      });
    </script>
  </head>
  <body>
    <h3>👋 Embedded Chat</h3>
    <div id="title"></div>
    <div id="embed" style="height: 400px"></div>
    <div id="log" title="Logs: " style="border: 1px; font-size: large"></div>
  </body>
</html>
```

## 🕵️‍♀️ Troubleshooting

Here go any troubleshooting solutions.

## 🥂 License

[MIT](./LICENSE.md) as always
