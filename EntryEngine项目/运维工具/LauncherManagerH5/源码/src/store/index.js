import Vue from 'vue'
import Vuex from 'vuex'
Vue.use(Vuex)
import IMBSProxy from '../services/IMBSProxy.js';
import storage from './storage';
import '../tool'

export default new Vuex.Store({
  state: {
    /**登录token */
    token: null,
    /**服务日志查询条件快照 */
    logSearch: {
      startTime: new Date(new Date().getTime() - 86400000 * 3).format("yyyy-MM-dd hh:mm"),
      endTime: null,
      content: "",
      param: "",
      type: ['1', '2', '3'],
    }
  },
  getters: {},
  mutations: {
    /**登录 */
    login(state, data) {
      console.log('登录', data);

      // 持久化存储
      storage.serviceAdd(data.services);
      storage.userAdd(data.username);
      storage.tokenSet(data.token);

      state.token = data.token;
      IMBSProxy.onSend = function (req) {
        console.log('AccessToken--->');
        req.setRequestHeader("AccessToken", data.token);
      };
    },
    /**登出 */
    logout(state) {
      state.token = null;
      localStorage.setItem('token', null);
      console.log('登出', state.token);
    },
    setLogSearch(state, data) {
      state.logSearch = data
    }
  },
  actions: {}
})