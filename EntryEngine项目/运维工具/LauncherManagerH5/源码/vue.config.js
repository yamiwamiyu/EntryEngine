module.exports = {
  publicPath: './',
  outputDir: 'dist',
  css: {
    loaderOptions: {
      sass: {
        prependData: `
          @import "~@/assets/styles/variables.scss";
          @import "~@/assets/styles/globals.scss";
        `,
      }
    }
  }
}