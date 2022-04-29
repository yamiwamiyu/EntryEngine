import Vue from 'vue'
import App from './App.vue'
import router from './router'
import store from './store'
import './tool'

import Vant, {
  Toast,
  Notify,
  Dialog
} from 'vant';
import 'vant/lib/index.css';
Vue.prototype.$Toast = Toast;
Vue.prototype.$Notify = Notify;
Vue.prototype.$Dialog = Dialog;
Vue.use(Vant);


import IMBSProxy from './services/IMBSProxy.js';
Vue.prototype.$IMBSProxy = IMBSProxy;


new Vue({
  router,
  store,
  render: h => h(App)
}).$mount('#app')