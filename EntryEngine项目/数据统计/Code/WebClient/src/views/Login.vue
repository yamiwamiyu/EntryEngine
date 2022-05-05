<template>
  <el-container class="container">
    <el-header
      class="header"
      height="100px;"
    >
      <h1 class="logo">
        <img src="../assets/logo.png" />
      </h1>
      <img src="../assets/imgs/login/slogan.png" />
    </el-header>
    <el-main class="main">
      <div class="form">
        <img src="../assets/logo.png" />
        <div style="padding:20px 0">欢迎来到1996游戏平台</div>
        <el-collapse-transition>
          <div v-show="modelUser">
            <el-input
              v-model="username"
              placeholder="用户名"
              clearable
              class="margin"
            />
            <el-input
              @keyup.enter="login()"
              v-model="password"
              placeholder="密码"
              show-password
              clearable
              class="margin"
            />
          </div>
        </el-collapse-transition>
        <el-collapse-transition>
          <div v-show="!modelUser">
            <el-input
              v-model="phone"
              placeholder="手机号"
              clearable
              class="margin"
            />
            <el-input
              @keyup.enter="login()"
              v-model="code"
              placeholder="验证码"
              clearable
              class="margin"
              style="width: 77%"
            />
            <el-image
              style="width: 20%; height: 20px"
              src="https://timgsa.baidu.com/timg?image&quality=80&size=b9999_10000&sec=1581489570955&di=9d04482c14b84ad940e8e3bcaae52cd4&imgtype=0&src=http%3A%2F%2Fi0.hdslb.com%2Fbfs%2Farticle%2F0a10e76210f5c315da85010803f3d3b26fc8d2c4.png"
              fit="scale-down"
            ></el-image>
          </div>
        </el-collapse-transition>
        <el-divider v-if="false">
          <el-button
            type="text"
            @click="modelUser=!modelUser"
          >切换{{modelUser?'手机':'用户名'}}登录</el-button>
        </el-divider>
        <div class="submit">
          <el-button
            type="primary"
            @click="login"
            class="margin submit-btn"
          >登录</el-button>
        </div>
      </div>
    </el-main>
    <el-footer class="copyright">
      <span>经营许可证：桂B2-20200032</span>
      <a
        target="_blank"
        href="https://beian.miit.gov.cn/#/Integrated/index"
      >桂ICP备18004203号-1</a>
      <a
        target="_blank"
        href="http://www.beian.gov.cn/portal/registerSystemInfo?recordcode=45030202000100"
      >
        <img src="../assets/imgs/copyright.png" />
        <span>桂公网安备 45030202000100号</span>
      </a>桂网文{2018} 10356-049号
    </el-footer>
  </el-container>
</template>

<script>
export default {
  name: "Login",
  data() {
    return {
      username: "",
      password: "",
      phone: "",
      code: "",
      modelUser: true,
    };
  },
  mounted() {
    this.$store.commit("logout");
  },
  methods: {
    login() {
      this.$IP4.Login(this.username,this.password,(res)=>{
        localStorage.setItem("token", res);
        this.$router.push("index")
      })
    },
  },
};
</script>

<style scoped lang="scss">
.container {
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  height: 100vh;
  width: 100vw;

  .header {
    display: flex;
    justify-content: space-between;
    flex-direction: row;
    flex-wrap: nowrap;
    align-items: center;

    height: 80px;
    width: 100%;
    max-width: 1200px;

    .logo {
      color: $color-title-font;
      object-fit: scale-down;
      height: 30px;
      width: 150px;
      display: flex;
      flex-direction: row;
      flex-wrap: nowrap;
      align-items: center;
      img {
        height: 40px;
      }
    }
  }

  .main {
    background: $color-background;
    display: flex;
    flex: 1;
    justify-content: center;
    align-items: center;

    .form {
      padding: 20px 60px;
      box-shadow: 1px 1px 5px $color-background;
      border-radius: 5px;
      width: 300px;
      background: white;

      .margin {
        margin-top: 10px;
        margin-bottom: 10px;
      }

      .submit {
        text-align: center;

        .submit-btn {
          width: 100%;
        }
      }
    }
  }

  .copyright {
    height: 60px;
    width: 100%;
    max-width: 1200px;
    text-align: center;
    padding: 0;
    a {
      padding-right: 10px;
    }
    img {
      display: inline-block;
    }
  }
}
</style>
