const path = require('path')
const webpack = require('webpack')
const bundleOutputDir = './wwwroot/dist'
const VueLoaderPlugin = require('vue-loader/lib/plugin')
const HtmlWebpackPlugin = require('html-webpack-plugin');

module.exports = () => {
  console.log('Building for \x1b[33m%s\x1b[0m development')

  let configObject = {
    mode: 'development',
    stats: { modules: false },
    entry: { 'main': './ClientApp/boot-app.js' },
    devtool: 'eval-source-map',
    devServer: {
      contentBase: path.join(__dirname, 'wwwroot'),
      host: 'localhost',
      publicPath: '/',
      historyApiFallback: true,
      open: true,
      hot: true,
      inline: true,
      proxy: {
        // Route the api calls to the acutal site
        '/api/**': {
          target: 'https://www.fantasycritic.games',
          secure: false,
          changeOrigin: true
        }
      }
    },
    resolve: {
      extensions: ['.js', '.vue'],
      alias: {
        'vue$': 'vue/dist/vue',
        'components': path.resolve(__dirname, './ClientApp/components'),
        'views': path.resolve(__dirname, './ClientApp/views'),
        'utils': path.resolve(__dirname, './ClientApp/utils'),
        'api': path.resolve(__dirname, './ClientApp/store/api')
      }
    },
    output: {
      path: path.join(__dirname, bundleOutputDir),
      filename: '[name].js',
      publicPath: '/dist/'
    },
    module: {
      rules: [
        {
          test: /\.vue$/,
          include: /ClientApp/,
          loader: 'vue-loader'
        },
        { test: /\.js$/, include: /ClientApp/, use: 'babel-loader' },
        { test: /\.(png|jpg|jpeg|gif|svg)$/, use: 'url-loader?limit=25000' },
        {
          test: /\.css$/,
          use: ['style-loader', 'css-loader']
        },
        {
          test: /\.scss$/,
          use: ['style-loader', 'css-loader', 'sass-loader']
        }
      ]
    },
    plugins: [
      new VueLoaderPlugin(),
      new webpack.DllReferencePlugin({
        context: __dirname,
        manifest: require('./wwwroot/dist/vendor-manifest.json')
      }),
      new webpack.SourceMapDevToolPlugin({
        filename: '[file].map', // Remove this line if you prefer inline source maps
        moduleFilenameTemplate:
          path.relative(bundleOutputDir,
            '[resourcePath]') // Point sourcemap entries to the original file locations on disk
      }),
      new HtmlWebpackPlugin({
        template: './ClientApp/index.ejs',
        minify: { collapseWhitespace: true }
      })
    ]
  };

  return [configObject];
}
