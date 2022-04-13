const webpack = require("webpack");
const Dotenv = require("dotenv-webpack");
const path = require("path");

var config = {
    mode: "development",
    context: path.resolve(__dirname),
    resolve: {
        extensions: [".ts", ".js", ".json"]
    },

    module: {
        rules: [
            {
                test: /\.ts?$/,
                loader: "awesome-typescript-loader"
            },
            // All output ".js" files will have any sourcemaps re-processed by "source-map-loader".
            {
                enforce: "pre",
                test: /\.js$/,
                loader: "source-map-loader"
            }
        ]
    }
}

// Return Array of Configurations
module.exports = (env) => [
    Object.assign({}, config, {
        entry: "./wwwroot/src/Embed.ts",
        output: {
            filename: "teamsembeddedchat.min.js",
            path: path.resolve(__dirname, "wwwroot/dist"),
            publicPath: "/wwwroot/dist/",
            library: "",
            libraryTarget: "umd",
            umdNamedDefine: true
        },
        plugins: [
            new Dotenv({
                path: `./.env.${env}`
            })
        ]
    }), 
    Object.assign({}, config, {
        entry: "./wwwroot/src/Auth.ts",
        output: {
            filename: "auth.min.js",
            path: path.resolve(__dirname, "wwwroot/dist"),
            publicPath: "/wwwroot/dist/",
            library: "",
            libraryTarget: "umd",
            umdNamedDefine: true
        },
        plugins: [
            new Dotenv({
                path: `./.env.${env}`
            })
        ]
    })
];