import { hideLoading, showLoading } from './api/jk';
import _IP4 from './api/IServiceProxy.js'; //不需要登录的接口
import _IP5 from './api/ICenterProxy.js'; //需要登录的接口
import router from './router';
import store from "./store";
import tool from './tool';
import element from 'element-plus';
import 'element-plus/dist/index.css';
import locale from 'element-plus/lib/locale/lang/zh-cn';
import { createApp } from 'vue';
import App from './App.vue';


// 设置 rem 函数
function FixWindowRem() {
  // 文字基准大小（单位px）
  const BaseFontSize = 32
  // 应用设计基准宽高
  const Width = 1920
  const Height = 1080
  var scale;
  // 屏幕宽高
  var width = document.documentElement.clientWidth || document.body.clientWidth;
  var height = document.documentElement.clientHeight || document.body.clientHeight;
  if ((width / height) < (Width / Height)) {
    // 竖屏
    scale = width / Width
  } else {
    // 横屏
    scale = height / Height
  }
  // 设置页面根节点字体大小
  document.getElementsByTagName('html').item(0).style.fontSize = BaseFontSize * scale + 'px'
}
// 第一次初始化设置
FixWindowRem()
// 改变窗口大小时重新设置
window.onresize = FixWindowRem


const app = createApp(App)

app.config.globalProperties.$IP4 = _IP4;
app.config.globalProperties.$IP5 = _IP5;
app.config.globalProperties.$ShowLoading = showLoading;
app.config.globalProperties.$EndLoading = hideLoading;
app.config.globalProperties.$Tool = tool;

// 注入路由
app.use(router)
app.use(store)
app.use(element, { locale })
//挂载实例
app.mount('#app')