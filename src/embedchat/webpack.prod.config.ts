import path from "path";
import { HotModuleReplacementPlugin } from "webpack";
import HtmlWebpackPlugin from "html-webpack-plugin";
import ForkTsCheckerWebpackPlugin from "fork-ts-checker-webpack-plugin";
import ESLintPlugin from "eslint-webpack-plugin";
import * as dotenv from "dotenv";

dotenv.config();

const PACKAGE_NAME = "MSTeamsExt";
const output_dir = "dist";
const embedChatUrl = `https://${process.env.AZURE_STORAGE_ACCOUNTNAME}.blob.core.windows.net/${process.env.CONTAINER}`;
const port = 4000;

module.exports = {
  mode: "production",
  context: __dirname,
  entry: {
    embedChat: [path.resolve(__dirname, "src", "index.ts")],
    auth: [path.resolve(__dirname, "src", "auth.ts")],
  },
  output: {
    filename: "[name].js",
    path: path.resolve(__dirname, output_dir),
    publicPath: output_dir,
    libraryTarget: "umd",
    library: [PACKAGE_NAME, "[name]"],
    umdNamedDefine: true,
    globalObject: "this",
  },
  optimization: {
    minimize: true,
  },
  module: {
    rules: [
      {
        test: /\.(ts|js)x?$/i,
        exclude: /node_modules/,
        use: {
          loader: "babel-loader",
          options: {
            presets: ["@babel/preset-env", "@babel/preset-typescript"],
          },
        },
      },
      {
        test: /\.s[ac]ss$/i,
        use: ["style-loader", "css-loader", "sass-loader"],
      },
      // All output ".js" files will have any sourcemaps re-processed by "source-map-loader".
      {
        enforce: "pre",
        test: /\.js$/,
        loader: "source-map-loader",
      },
    ],
  },
  resolve: {
    extensions: [".ts", ".js", ".json"],
  },
  plugins: [
    new HotModuleReplacementPlugin(),
    new ForkTsCheckerWebpackPlugin({
      async: false,
    }),
    new ESLintPlugin({
      extensions: ["js", "jsx", "ts", "tsx"],
    }),
    new HtmlWebpackPlugin({
      template: path.resolve(__dirname, "src", "auth.html"),
      filename: "auth.html",
      chunks: ["auth"],
      publicPath: `${embedChatUrl}`,
      minify: false,
      inject: "body",
      cache: true,
    }),
  ],
  devtool: "inline-source-map",
  devServer: {
    contentBase: path.join(__dirname, output_dir),
    port: `${port}`,
    historyApiFallback: true,
    hot: true,
  },
};
