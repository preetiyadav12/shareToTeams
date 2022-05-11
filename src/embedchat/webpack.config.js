const path = require("path");

const PACKAGE_NAME = "MSTeamsExtensions";
const output_dir = "dist";

const config = {
  context: __dirname,
  entry: {
    app: [path.resolve(__dirname, "src", "index.ts")],
  },
  output: {
    filename: "embedchat.js",
    path: path.resolve(__dirname, output_dir),
    publicPath: output_dir,
    libraryTarget: "umd",
    library: PACKAGE_NAME,
    umdNamedDefine: true,
    globalObject: "this",
  },
  module: {
    rules: [
      {
        test: /\.ts?$/,
        loader: "ts-loader",
        exclude: /node_modules/,
      },
      {
        test: /\.s[ac]ss$/i,
        use: ["style-loader", "css-loader", "sass-loader"],
      },
    ],
  },
  resolve: {
    extensions: [".ts", ".js", ".json"],
  },
  devtool: "inline-source-map",
  devServer: {
    contentBase: path.join(__dirname, output_dir),
  },
};

module.exports = (env, argv) => {
  return config;
};
