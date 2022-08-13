const { env } = require("process");
const path = require('path');

const baseFolder =
  process.env.APPDATA !== undefined && process.env.APPDATA !== ''
    ? `${process.env.APPDATA}/ASP.NET/https`
    : `${process.env.HOME}/.aspnet/https`;

const certificateArg = process.argv.map(arg => arg.match(/--name=(?<value>.+)/i)).filter(Boolean)[0];
const certificateName = certificateArg ? certificateArg.groups.value : process.env.npm_package_name;

let target = env.ASPNETCORE_HTTPS_PORT
  ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}`
  : env.ASPNETCORE_URLS
  ? env.ASPNETCORE_URLS.split(";")[0]
  : "http://localhost:44391";

if (process.env.CLIENT_MODE) {
  target = "https://www.fantasycritic.games";
}

module.exports = {
  chainWebpack: config => {
    config.module
      .rule('vue')
      .use('vue-loader')
        .tap(options => {
          options.transformAssetUrls = {
            video: ['src', 'poster'],
            source: 'src',
            img: 'src',
            image: 'xlink:href',
            'b-avatar': 'src',
            'b-img': 'src',
            'b-img-lazy': ['src', 'blank-src'],
            'b-card': 'img-src',
            'b-card-img': 'src',
            'b-card-img-lazy': ['src', 'blank-src'],
            'b-carousel-slide': 'img-src',
            'b-embed': 'src'
          };
          return options;
        })
  },
  devServer: {
    server: {
      type: 'https',
      options: {
        cert: path.join(baseFolder, `${certificateName}.pem`),
        key: path.join(baseFolder, `${certificateName}.key`)
      }
    },
    proxy: {
      "^/api": {
        target: target,
        changeOrigin: true,
      },
      "^/Account": {
        target: target,
        changeOrigin: true,
      },
      "^/updatehub": {
        target: target,
        ws: true,
        changeOrigin: true,
      },
      "^/.well-known": {
        target: target,
        changeOrigin: true,
      },
      "^/css": {
        target: target,
        changeOrigin: true,
      },
      "^/img": {
        target: target,
        changeOrigin: true,
      },
      "^/js": {
        target: target,
        changeOrigin: true,
      },
      "^/lib": {
        target: target,
        changeOrigin: true,
      },
    },
    client: {
      webSocketURL: 'auto://localhost:44477/ws',
    },
    allowedHosts: 'all'
  }
}