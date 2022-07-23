const httpsBase = 'C:/Users/elite/AppData/Roaming/ASP.NET/https/';

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
        cert: httpsBase + 'fantasy-critic-client-app.pem',
        key: httpsBase + 'fantasy-critic-client-app.key'
      }
    },
    proxy: 'https://localhost:44391/'
  }
}