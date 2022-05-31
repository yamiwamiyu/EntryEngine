const { defineConfig } = require('@vue/cli-service')
module.exports = defineConfig({
  transpileDependencies: true,
  lintOnSave: false,
  productionSourceMap: false,
  publicPath: "./",

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
        //scss: {
        //  prependData: `
        //  @import "~@/assets/scss/style.scss";`,
        //},
      },

      // È«¾Ö¼ÓÔØscss
      sass: {
        additionalData: `@import "@/assets/scss/style.scss";`
      }
    }
  },
})

// npm -v 8.0.0
// node -v v16.13.0
// vue -V @vue-cli 5.0.4