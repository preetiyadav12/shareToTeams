# Creating Embedded Chat React App with Typescript and ESLint with Webpack 5

Our project is based in the following folders in a root folder:

- *build*: This is where all the artifacts from the build output.
- *src*: This holds our source code.

>Note that a node_modules folder will also be created as we start to install the
>project’s dependencies.

## Typescript Configuration File: tsconfig.json

We are only going to use TypeScript in our project for type checking. We use
Babel to do the code transpilation. So, the compiler options in our
`tsconfig.json` are focused on type checking, not code transpilation.

Here’s an explanation of the settings we have used:

- lib: The standard typing to be included in the type checking process. In our
  case, we have chosen to use the types for the browser’s DOM and the latest
  version of ECMAScript.
- allowJs: Whether to allow JavaScript files to be compiled.
- allowSyntheticDefaultImports: This allows default imports from modules with no
  default export in the type checking process.
- skipLibCheck: Whether to skip type checking of all the type declaration files
  (*.d.ts).
- esModuleInterop: This enables compatibility with Babel.
- strict: This sets the level of type checking to very high. When this is true,
  the project is said to be running in strict mode.
- forceConsistentCasingInFileNames: Ensures that the casing of referenced file
  names is consistent during the type checking process.
- moduleResolution: How module dependencies get resolved, which is node for our
  project.
- resolveJsonModule: This allows modules to be in .json files which are useful
  for configuration files.
- noEmit: Whether to suppress TypeScript generating code during the compilation
  process. This is true in our project because Babel will be generating the
  JavaScript code.
- jsx: Whether to support JSX in .tsx files.
- include: These are the files and folders for TypeScript to check. In our
  project, we have specified all the files in the src folder.

## Babel

Our project uses Babel to convert our React and TypeScript code to JavaScript.
Here’s an explanation of the packages we have just installed:

- @babel/core: As the name suggests, this is the core Babel library.
- @babel/preset-env: This is a collection of plugins that allow us to use the
  latest JavaScript features but still target browsers that don’t support them.
- @babel/preset-react: This is a collection of plugins that enable Babel to
  transform React code into JavaScript.
- @babel/preset-typescript: This is a plugin that enables Babel to transform
  TypeScript code into JavaScript.
- @babel/plugin-transform-runtime and @babel/runtime: These are plugins that
  allow us to use the async and await JavaScript features.

## Linting

We use ESLint in our project. ESLint can help us find problematic coding
patterns or code that doesn’t adhere to specific style guidelines.

Below is an explanation of the packages that we just installed:

- eslint: This is the core ESLint library.
- eslint-plugin-react: This contains some standard linting rules for React code.
- eslint-plugin-react-hooks: This includes some linting rules for React hooks
  code.
- @typescript-eslint/parser: This allows TypeScript code to be linted.
- @typescript-eslint/eslint-plugin: This contains some standard linting rules
  for TypeScript code.

>Note: ESLint can be configured in a .eslintrc.json file in the project root.

We have configured ESLint to use the TypeScript parser, and the standard React
and TypeScript rules as a base set of rules. We’ve explicitly added the two
React hooks rules and suppressed the `react/prop-types` rule because prop types
aren’t relevant in React with TypeScript projects. We have also told ESLint to
detect the version of React we are using.

## Webpack

Webpack is a popular tool that we use to create performant bundles containing
our app’s JavaScript code.
>Note: TypeScript types are included in the webpack package, so we don’t need to
>install them separately.

Webpack has a web server that we use during development. TypeScript types aren’t
included in the `webpack-dev-server` package, which is why we installed the
`@types/webpack-dev-server` package.

We also need a Webpack plugin, `babel-loader`, to allow Babel to transpile the
React and TypeScript code into JavaScript and a Webpack plugin,
`html-webpack-plugin`, which will generate the HTML.

## Configuring Development

We've added two configuration files for Webpack: one for development and one for
production. A development configuration file `webpack.dev.config.ts` can be
found in the project’s root directory.

Here are the critical bits in this configuration file:

- The `mode` field tells Webpack whether the app needs to be bundled for
  production or development. We are configuring Webpack for development, so we
  have set this to `development`. Webpack will automatically set
  `process.env.NODE_ENV` to `development` which means we get the React
  development tools included in the bundle.
- The `output.public` field tells Webpack what the root path is in the app. This
  is important for deep linking in the dev server to work properly.
- The `entry` field tells Webpack where to start looking for modules to bundle.
  In our project, this is `index.tsx`.
- The `module` field tells Webpack how different modules will be treated. Our
  project is telling Webpack to use the `babel-loader` plugin to process files
  with `.js`, `.ts`, and `.tsx` extensions.
- The `resolve.extensions` field tells Webpack what file types to look for in
  which order during module resolution. We need to tell it to look for
  TypeScript files as well as JavaScript files.
- The `HtmlWebpackPlugin` creates the HTML file. We have told this to use our
  index.html in the src folder as the template.
- The `HotModuleReplacementPlugin` and `devServer.hot` allow modules to be
  updated while an application is running, without a full reload.
- The `devtool` field tells Webpack to use full inline source maps. This allows
  us to debug the original code before transpilation.
- The `devServer` field configures the Webpack development server. We tell
  Webpack that the root of the webserver is the `build` folder, and to serve
  files on port `4000`. `historyApiFallback` is necessary for deep links to work
  in multi-page apps. We are also telling Webpack to open the browser after the
  server has been started.

## Configuring Production

The Webpack configuration for production is a little different - we want files
to be bundled in the file system optimized for production without any dev stuff.
This is similar to the development configuration with the following differences:

- We’ve specified the mode to be production. Webpack will automatically set
  `process.env.NODE_ENV` to `production` which means we won’t get the React
  development tools included in the bundle.
- The `output` field tells Webpack where to bundle our code. In our project,
  this is the `build` folder. We have used the `name` token to allow Webpack to
  name the files if our app is code split. We have used the `contenthash` token
  so that the bundle file name changes when its content changes, which will bust
  the browser cache.
- The `CleanWebpackPlugin` plugin will clear out the build folder at the start
  of the bundling process.

## Type Checking

We use a package called `fork-ts-checker-webpack-plugin` to enable the Webpack
process to type check the code. This means that Webpack will inform us of any
type errors. We have used the `async` flag in the webpack's plugin configuration
to tell Webpack to wait for the type checking process to finish before it emits
any code.

## Running App in Development

You can run the `yarn start` script to start our app in development mode. The
script starts the Webpack development server. We have used the `config` option
in the `package.json` file to reference the development configuration file.

## Building App for Production

You can run the `yarn build` script to start the Webpack bundling process. We
have used the `config` option in the `build` script in `package.json` file to
reference the production configuration file. If we look at the JavaScript file,
we will see it is minified. Webpack uses its `TerserWebpackPlugin` out of the
box in production mode to minify code. The JavaScript bundle contains all the
code from our app as well as the code from the react and react-dom packages.

If we look at the html file, we will see all the spaces have been removed. If we
look closely, we will see a script element referencing our JavaScript file which
the `HtmlWebpackPlugin` did for us.
