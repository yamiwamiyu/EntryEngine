const { defineConfig } = require('@vue/cli-service')
module.exports = defineConfig({
  transpileDependencies: true,
  lintOnSave: false,
  productionSourceMap: false,

  css: {
    loaderOptions: {
      postcss: {
        postcssOptions: {
          plugins: [
            require('postcss-pxtorem')({
              rootValue: 32,
              propList: ["*"],
            })
          ]
        },
        //sass: {
        //  prependData: `
        //  @import "~@/assets/styles/variables.scss";
        //  @import "~@/assets/styles/globals.scss";`,
        //},
      },
    }
  },
})

// npm -v 8.0.0
// node -v v16.13.0
// vue -V @vue-cli 5.0.4