const path = require('path')
const webpack = require('webpack')
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const OptimizeCSSPlugin = require('optimize-css-assets-webpack-plugin')
const bundleOutputDir = './wwwroot/dist'
const VueLoaderPlugin = require('vue-loader/lib/plugin')
var CompressionPlugin = require('compression-webpack-plugin');

module.exports = () => {
  console.log('Building for \x1b[33m%s\x1b[0m', process.env.NODE_ENV)

  const isDevBuild = !(process.env.NODE_ENV && process.env.NODE_ENV === 'production')

  const extractCSS = new MiniCssExtractPlugin({
    filename: 'style.css'
  })

  let configObject = {
    mode: (isDevBuild ? 'development' : 'production'),
    stats: { modules: false },
    entry: { 'main': './ClientApp/boot-app.js' },
    devServer: {
      contentBase: path.join(__dirname, 'wwwroot'),
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
      alias: isDevBuild
        ? {
          'vue$': 'vue/dist/vue',
          'components': path.resolve(__dirname, './ClientApp/components'),
          'views': path.resolve(__dirname, './ClientApp/views'),
          'utils': path.resolve(__dirname, './ClientApp/utils'),
          'api': path.resolve(__dirname, './ClientApp/store/api')
        }
        : {
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
      })
    ].concat(isDevBuild
      ? [
        // Plugins that apply in development builds only
        new webpack.SourceMapDevToolPlugin({
          filename: '[file].map', // Remove this line if you prefer inline source maps
          moduleFilenameTemplate:
            path.relative(bundleOutputDir,
              '[resourcePath]') // Point sourcemap entries to the original file locations on disk
        })
      ]
      : [
        extractCSS,
        // Compress extracted CSS.
        new OptimizeCSSPlugin({
          cssProcessorOptions: {
            safe: true
          }
        }),
        new CompressionPlugin({
          algorithm: "gzip",
          threshold: 10240,
        })
      ])
  };

  if (isDevBuild) {
    configObject.devtool = "eval-source-map";
  }

  return [configObject];
}
