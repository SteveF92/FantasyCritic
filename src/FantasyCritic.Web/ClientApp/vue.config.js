const { defineConfig } = require("@vue/cli-service");
module.exports = defineConfig({
  transpileDependencies: true,
  outputDir: "..\\wwwroot\\ClientApp",
  filenameHashing: false,
  publicPath: "/ClientApp"
});
