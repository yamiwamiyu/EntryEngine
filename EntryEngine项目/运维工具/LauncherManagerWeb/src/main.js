import { createApp } from 'vue'
import App from './App.vue'
import Vant, {
  Toast,
  Notify,
  Dialog
} from 'vant';
import 'vant/lib/index.css';
import store from './store'
import router from './router'
import IMBSProxy from './services/IMBSProxy.js';


// 设置 rem 函数
function FixWindowRem() {
  // 文字基准大小（单位px）
  const BaseFontSize = 32
  // 应用设计基准宽高
  const Width = 750
  const Height = 1334
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

var vue = createApp(App)
vue.config.globalProperties.$Toast = Toast;
vue.config.globalProperties.$Notify = Notify;
vue.config.globalProperties.$Dialog = Dialog;
vue.config.globalProperties.$IMBSProxy = IMBSProxy;
vue
  // ui框架
  .use(Vant)
  // Vuex
  .use(store)
  // router
  .use(router)
  .mount('#app')