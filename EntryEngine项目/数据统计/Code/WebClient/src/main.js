import { hideLoading, showLoading } from '@/api/jk';
import _IP4 from '@/api/Protocol4Proxy.js'; //不需要登录的接口
import _IP5 from '@/api/Protocol5Proxy.js'; //需要登录的接口
import router from '@/router';
import store from "@/store";
import tool from '@/tool';
import element from 'element-plus';
import 'element-plus/dist/index.css';
import locale from 'element-plus/lib/locale/lang/zh-cn';
import { createApp } from 'vue';
import App from './App.vue';



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