import path from "path";
import { HotModuleReplacementPlugin } from "webpack";
import CopyPlugin from "copy-webpack-plugin";
import HtmlWebpackPlugin from "html-webpack-plugin";
import ForkTsCheckerWebpackPlugin from "fork-ts-checker-webpack-plugin";
import ESLintPlugin from "eslint-webpack-plugin";
import * as dotenv from "dotenv";

dotenv.config();

if (!process.env.CDN_ENDPOINT_FQN) {
  throw Error("Azure CDN Endpoint Environment Variable is not found");
}

const PACKAGE_NAME = "MSTeamsExt";
const output_dir = "dist";
const port = 4000;
const authPath = `https://${process.env.CDN_ENDPOINT_FQN?.replace(/["]/g, '')}`;

module.exports = {
  mode: "production",
  context: __dirname,
  entry: {
    shareToTeams: [path.resolve(__dirname, "src", "index.ts")],
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
      publicPath: authPath,
      minify: false,
      inject: "body",
      cache: true,
    }),
    new CopyPlugin({
      patterns: [
        {
          from: "src/shareToTeams.css",
        },
      ],
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
