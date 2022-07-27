import { createApp } from 'vue'
import { createRouter, createWebHistory } from 'vue-router'
import App from './App.vue';

// router
const routes = [
    { path: '/', component: () => import('./components/index') },
]

let vue = createApp(App)
// 设置通用数据
//vue.config.globalProperties.$IMBSProxy = IMBSProxy;
vue
  // Vuex
  //.use(store)
  // router
  .use(
    createRouter({
        history: createWebHistory(process.env.BASE_URL),
        routes
    })
  )
  .mount('#app')