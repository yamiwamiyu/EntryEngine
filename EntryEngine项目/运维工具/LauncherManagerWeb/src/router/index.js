/*import VueRouter from 'vue-router'*/
import { createRouter, createWebHashHistory } from 'vue-router'
import store from '../store'

const routes = [{
    path: '/',
    redirect: '/login'
  },
  {
    path: '/login',
    name: 'Login',
    component: () => import('../views/login.vue')
  },
  {
    path: '/home',
    name: 'Home',
    component: () => import('../views/home.vue')
  },
  {
    path: '/services',
    name: 'Services',
    component: () => import('../views/services.vue')
  },
  {
    path: '/serviceLog',
    name: 'ServiceLog',
    component: () => import('../views/serviceLog.vue')
  },
  {
    path: '/mine',
    name: 'Mine',
    component: () => import('../views/mine/index.vue'),
  },
  {
    path: '/mineRecord',
    name: 'Record',
    component: () => import('../views/mine/record.vue')
  },
  {
    path: '/mineService',
    name: 'Service',
    component: () => import('../views/mine/service.vue')
  },
  {
    path: '/mineUser',
    name: 'User',
    component: () => import('../views/mine/user.vue')
  },
  {
    path: "/:pathMatch(.*)*",
    name: '404',
    component: () => import('../views/404.vue'),
  }
]

const router = createRouter({
  // mode: 'history',
  base: process.env.BASE_URL,
  history: createWebHashHistory(process.env.BASE_URL),
  routes
})

router.beforeEach((to, from, next) => {
  if (to.name !== 'Login' && !store.state.token) {
    //store.state.token = localStorage.getItem("token")
    if (!store.state.token)
      next('/login')
  }
  next()
})
console.log(router);
export default router