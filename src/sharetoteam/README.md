# shareToTeams  Project Starter

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
 - **Environment Variables for React App Sample app** (optional): if you plan on
  running a sample React App with the shareToTeams, you'd be required
  to create a local `.env` file in the same folder where the React App sample
  application is located. You can use the provided `.env.template` file to clone
  it into `.env` or `.env.development` file. Use your settings to fill the
  content of that file prior to building React App sample application.
- **ShareToTeams Project Environment Variables**: for uploading bundle output files
  to Azure Storage Blob, you need to create a local environment variables file
  `.env`, which you can clone from `.env.template` and then fill all required
  fields.

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

**upload** will run after the build and publishing are complete. It will upload
all bundle files that should be listed in the `.env` file to the specified Azure
Storage Blob container.

`"upload" : "upload": "node uploadpkg.js"`

This is how the **scripts** section in package.json looks like:

```json
"scripts": {
    "clean": "rimraf dist",
    "prebuild": "npm run clean",
    "unpublish:local": "yarn unlink",
    "publish:local": "yarn link",
    "npm:local": "run-s unpublish:local publish:local",
    "prepare": "run-s build:dev publish:local upload",
    "prepublishOnly": "yarn test && yarn lint",
    "preversion": "yarn lint",
    "build:dev": "tsc --build ./tsconfig.build.json && webpack --config ./webpack.dev.config.ts",
    "build:deploy": "tsc --build ./tsconfig.build.json && webpack --config ./webpack.prod.config.ts",
    "build": "run-s build:dev npm:local upload",
    "build:prod": "run-s build:deploy npm:local upload",
    "lint": "eslint . --ext .ts",
    "lint:fix": "eslint . --ext .ts --fix",
    "format": "prettier --write \"src/**/*.ts\"",
    "version": "yarn format && git add .",
    "postversion": "git push && git push --tags",
    "test": "jest --config jestconfig.json",
    "upload": "node uploadpkg.js"
}

```

## ✌️ Start Coding in 2 steps

1. Run `yarn install` When running the project build for the very first time,
   you need to build and publish the NPM package locally so it can be used in
   your sample projects. This is done by running **prepare** command as it
   explained earlier.

2. Now, run `yarn link` to publish locally the new `@msteams/sharetoteams` package
   on your machine.

Then later, when you need to re-build your package, just simply use this
command: `yarn build` or `yarn build:prod` for the production version of the
package.

> Note: You can build the package for `development` or `production`
> environments. The `development` version will produce the unoptimized, not
> minimized version of the bundle files, while the `production` version will
> optimize and minize Javascript files in the bundle.

- To build the project for `development` environment, just use this command:
  `yarn build`

- To build the project for `production` environment, use this command: `yarn build:prod`

These commands will build the shareToTeams project and also publish **locally**
the `@msteams/sharetoteams` NPM package.

## Publish shareToTeams package to NPM

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
yarn add @msteams/sharetoteams
# OR
npm install @msteams/sharetoteams
```

- Use it 💪

1.To launch your local client HTML file that references the ShareToTeams scripts, 
  you can either use the `Live Server` or you can use online service
  like `Codepen.io`. Here's example of the local HTML file that references the
  ShareToTeams.js.


  ## 🕵️‍♀️ Troubleshooting

Here go any troubleshooting solutions.

## 🥂 License

[MIT](./LICENSE.md) as always
