import tool from '@/tool'
import { createStore } from 'vuex'

let initState={
    userInfo: {},
    hasLogin:false,
}

// 创建一个新的 store 实例
const store = createStore({
    state () {
        return tool.copy(initState)
    },
    mutations: {
        login(state, provider) {
            Object.assign(state, tool.copy(initState))
            state.hasLogin = true;
            state.userInfo = provider;
        },
        logout(state) {
            Object.assign(state, tool.copy(initState));
            state.hasLogin = false;
            localStorage.removeItem('token')
        },
    },
    getters: {
        /**
         * 用户信息
         * @param state
         * @returns {function(): {}}
         */
        getUserInfo: (state) => () => {
            return state.userInfo
        },
        /**
         * 登录状态
         * @param state
         * @returns {function(): boolean}
         */
        getLogin: (state) => () => {
            return state.hasLogin
        },
    },
    actions: {
    },
    modules: {}
})
export default store

