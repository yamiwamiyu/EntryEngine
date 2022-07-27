const { defineConfig } = require('@vue/cli-service')
module.exports = defineConfig({
    transpileDependencies: true,
    lintOnSave: false,
	// 发布时不生成.map文件
	productionSourceMap: false,
	// 发布后可以将网站配置到IIS的一个文件夹里，方便一个IIS网站跑多个项目
	publicPath: "./",

	css: {
        loaderOptions: {
			// postcss-pxtorem：css中使用的尺寸单位为px，编译后会自动转换成rem（需要下载postcss-pxtorem插件，注意不支持行内样式）
            postcss: {
                postcssOptions: {
                    plugins: [
                        require('postcss-pxtorem')({
                            rootValue: 32,
                            propList: ["*"],
                        })
                    ]
                }
            },

            // 全局加载scss
			// sass: {
			// 	additionalData: `@import "@/assets/global.scss";`
			// },
        }
    },
})
