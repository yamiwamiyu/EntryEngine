import P5 from '@/api/Protocol5Proxy';
import store from '@/store';
import Login from '@/views/Login';
import { ElMessage } from 'element-plus';
import { createRouter, createWebHistory } from 'vue-router';

const history = createWebHistory()
const index = () =>
    import ('@/views/Index')
const routes = [{
        path: '/login',
        name: 'login',
        component: Login
    },
    {
        path: '/index',
        name: 'Index',
        component: index,
        children: [{
                path: 'analysis',
                name: 'analysis',
                component: () =>
                    import ('@/views/Analysis')
            },
            {
                path: 'onLineCount',
                name: 'onLineCount',
                component: () =>
                    import ('@/views/OnLineCount')
            },
            {
                path: 'gameTime',
                name: 'gameTime',
                component: () =>
                    import ('@/views/GameTime')
            },
            {
                path: 'gameManager',
                name: 'gameManager',
                component: () =>
                    import ('@/views/GameManager')
            },
            {
                path: 'systemSetting',
                name: 'systemSetting',
                component: () =>
                    import ('@/views/SystemSetting')
            },
            {
                path: 'keep',
                name: 'keep',
                component: () =>
                    import ('@/views/Keep')
            },
            {
                path: 'gameTask',
                name: 'gameTask',
                component: () =>
                    import ('@/views/GameTask')
            },
            {
                path: 'gameVerfication',
                name: 'gameVerfication',
                component: () =>
                    import ('@/views/GameVerfication')
            }

        ]
    },

]
const router = createRouter({
    history, // 路由模式
    routes
})
router.beforeEach((to, from, next) => {
    // 如果去游戏测试页，无需登录
    if (to.path.slice(0, 10) == '/gameTest/') {
        next();
        return;
    } else if (to.path == '/') { // 跳转到登录页
        next({ path: '/login' })
        return
    }

    let token = localStorage.getItem('token')
    let urls = ['/login']
    console.log(to.path)
        // next();
        // return;
    if (urls.includes(to.path))
        next();
    else if (store.getters.getLogin())
        next();
    else {
        if (token != undefined && token != '') {
            P5.GetUserInfo((res) => {
                store.commit('login', res)
                next()
            })
        } else {
            ElMessage({
                message: '请先登录',
                type: 'error'
            })
            next({
                path: '/login'
            })
        }
    }
})
export default router;