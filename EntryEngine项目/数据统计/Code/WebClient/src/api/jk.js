import _IP4 from '@/api/Protocol4Proxy.js';//不需要登录的接口
import _IP5 from '@/api/Protocol5Proxy.js';//需要登录的接口
// import config from '../../public/config.js'
import { ElMessage, ElLoading } from 'element-plus'
import router from '@/router'

let loadingCount = 0
let Loading
// console.log(config.config_url);
// let url = config.config_url
let url = config_url

let err = function (res) {
  ElMessage.error(`错误${res.status}  ${res.statusText}`)
}
let errmsg = function (res) {
  if (res.errCode === 100) {
    router.push({ path: '/login' })
    localStorage.setItem('token', '')
    localStorage.removeItem('user')
  }
  ElMessage.error(res.errMsg)
}

let token = function (res) {
  res.timeoutid = window.setTimeout(startLoading(res), 200)
  res.setRequestHeader('AccessToken', localStorage.getItem('token'))
  res.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded;charset=UTF-8')
}
let onCallback = (res) => {
  if (res.timeoutid == undefined) {
    endLoading()
  } else {
    window.clearTimeout(res.timeoutid)
  }
}
const startLoading = (res) => {
  return () => {
    res.timeoutid = undefined
    Loading = ElLoading.service({
      lock: true,
      text: '加载中……',
      background: 'rgba(0,0,0,0.5)'
    })
  }
}

const endLoading = () => {
  Loading.close()
};

export const showLoading = () => {
  if (loadingCount === 0) {
    startLoading()
  }
  loadingCount += 1
};

export const hideLoading = () => {
  if (loadingCount <= 0) {
    return
  }
  loadingCount -= 1
  if (loadingCount === 0) {
    endLoading()
  }
}

/*需要登录*/
_IP5.url = url
_IP5.onErrorMsg = errmsg
_IP5.onError = err
_IP5.onSend = token
_IP5.onCallback = onCallback
/*不需要登录*/
_IP4.url = url
_IP4.onErrorMsg = errmsg
_IP4.onError = err
_IP4.onSend = token
_IP4.onCallback = onCallback

/*let socket = new WebSocket(this.path+this.mytoken)
// 监听socket消息
socket.onmessage = (msg)=>{
  console.log(JSON.parse(msg.data))
}
_iws._IWSProxy.ws = socket*/
