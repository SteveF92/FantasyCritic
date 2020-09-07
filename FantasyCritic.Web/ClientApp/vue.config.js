let config = {
  lintOnSave: false,
};

if (process.env.NODE_ENV === "client") {
  config = {
    ...config,
    devServer: {
      proxy: {
        // Route the api calls to the acutal site
        "/api/**": {
          target: "https://www.fantasycritic.games",
          secure: false,
          changeOrigin: true,
        },
      },
    },
  };
}

module.exports = config;
