<template>
  <el-container class="index">
    <el-header class="header" height="100px;">
      <h1 class="logo">
        <img src="../assets/logo2.png" />
        <i style="padding: 0 16px 0 16px; margin-top:-2px;">|</i>
        <span>游戏平台</span>
      </h1>
      <el-menu
          :default-active="''+navActive"
          class="el-menu-demo"
          mode="horizontal"
          @select="goNav"
      >
          <el-menu-item 
            v-for="(nav, navIndex) in templates"
            :key="'nav-' + navIndex"
            :index="'' + navIndex"
            :style="{'display': $store.state.userInfo.Type==1 || nav.text=='数据分析' ? 'block':'none'}"
              >
            {{nav.text}}
          </el-menu-item>
      </el-menu>
      <div class="manage">
        <div class="user-name">
          欢迎
          <span>{{ $store.getters.getUserInfo().Account }}</span>
        </div>
        <div
            @click="logout"
            class="text_back"
        >退出
        </div>
        <i class="jiangefu">|</i>
        <div
            @click="showUpPwd = !showUpPwd"
            class="text_modify"
        >修改密码
        </div>
      </div>
      <el-dialog
          title="修改密码"
          v-model="showUpPwd"
          width="300px"
      >
        <el-form
            ref="pwdFrom"
            :model="pwdFrom"
            label-width="60px"
            label-position="left"
        >
          <el-form-item label="原密码">
            <el-input v-model="pwdFrom.opwd"></el-input>
          </el-form-item>
          <el-form-item label="新密码">
            <el-input
                v-model="pwdFrom.npwd"
                show-password
            ></el-input>
          </el-form-item>
        </el-form>
        <span
            slot="footer"
            class="dialog-footer"
        >
          <el-button @click="showUpPwd = false">取 消</el-button>
          <el-button
              type="primary"
              @click="updatePwd"
          >确 定</el-button>
        </span>
      </el-dialog>
    </el-header>
    <el-container class="content">
      <el-aside class="aside">
        <el-menu
            :default-active="$route.path"
            class="el-menu-vertical-demo"
            router
            unique-opened
        >
<!--  templates[navActive]通过横向菜单当前选择下标获取纵向菜单          -->
          <el-menu-item
              v-for="(sub, subIndex) in templates[navActive].templates"
              :key="'sub'+subIndex"
              :index="sub.route">
            <template #title>
              <i :class="sub.icon"></i>
              <span :class="{'is-active':sub.route == $route.path}">{{ sub.text }}</span>
            </template>
          </el-menu-item>
        </el-menu>
      </el-aside>

      <el-main class="main">
        <router-view/>
      </el-main>
    </el-container>
  </el-container>
</template>

<script>
import { ElMessage } from 'element-plus'
export default {
  name: "Index",
  data() {
    return {
      // 选择导航 
      navActive: 0,
      defaultActive:"",
      // 选择菜单
      chooseMenu: "",
      templates: [
        {
          icon: "",
          text: "数据分析",
          templates: [
            {
              icon: "",
              text: "基础分析",
              route: "/index/analysis",
            },
            {
              icon: "",
              text: "留存分析",
              route: "/index/keep",
            },
            {
              icon: "",
              text: "在线人数",
              route: "/index/onLineCount",
            },
            {
              icon: "",
              text: "游戏时长",
              route: "/index/gameTime",
            },
          ],
        },
        {
          icon:"",
          text:"游戏管理",
          templates:[
            {
              icon: "",
              text: "游戏统计",
              route: "/index/gameManager",
            },
            //{
            //  icon: "",
            //  text: "游戏任务管理",
            //  route: "/index/gameTask",
            //},
            //{
            //  icon: "",
            //  text: "获奖码核销",
            //  route: "/index/gameVerfication",
            //},
          ],
        },
        {
          icon:"",
          text:"系统设置",
          templates:[
            {
              icon: "",
              text: "用户管理",
              route: "/index/systemSetting",
            },
          ],
        }
      ],
      
      showUpPwd: false,
      pwdFrom: {
        /*旧密码*/
        opwd: "",
        /*新密码*/
        npwd: "",
      },
    }
  },
  watch: {
    showUpPwd(val) {
      if (!val) {
        Object.assign(this.$data.pwdFrom, this.$options.data().pwdFrom);
      }
    },
  },
  computed: {
  },
  mounted() {
    this.goNav(0);
  },
  methods: {

    updatePwd() {
      if (this.pwdFrom.opwd === this.pwdFrom.npwd) {
        ElMessage.error('新密码和旧密码不能相同!');
        return;
      }
      this.$IP5.ChangePassword(
        this.pwdFrom.opwd,
        this.pwdFrom.npwd,
        (res) => {
          this.logout();
        }
      );
    },
    handleSelect(key, keyPath) {
      console.log(key, keyPath)
    },
    goNav(index) {
      console.log(index);
      this.navActive = index;
      this.$router.push(this.templates[index].templates[0].route)
    },
    logout() {
      localStorage.removeItem('token');
      this.$store.commit("logout");
      this.$router.push("/login");
      this.$message.success("已退出登录");
    },
  }

}
</script>

<style lang="scss" scoped>
.is-active{
  color: var(--el-color-primary);
}
.index {
  height: 100vh;
  display: flex;
  flex: 1;
  flex-direction: column;
  justify-content: flex-start;

  .header {
    display: flex;
    flex-direction: row;
    flex-wrap: nowrap;
    align-items: center;
    padding: 0 20px;
    min-width: 1200px;
    justify-content: space-between;
    .el-menu-demo{
      width: 500px;
      border-bottom: solid 0px var(--el-menu-border-color);
    }
    .logo {
      color: $color-title-font;
      object-fit: scale-down;
      height: 30px;
      width: 230px;
      display: flex;
      flex-direction: row;
      flex-wrap: nowrap;
      align-items: center;

      img {
        height: 100%;
      }
    }

    .nav {
      flex: 1;
      display: flex;
      justify-content: center;
      align-items: center;
      flex-direction: row;
      min-width: 800px;
      border: none;

      .nav-item {
        padding: 0;
        margin: 0 34px;
        color: #142338;

        &.is-active {
          border-bottom: 4px solid $color-primary;
          font-weight: bold;
        }
      }
    }

    .manage {
      display: flex;
      flex-direction: row;
      justify-content: flex-end;
      align-items: center;
      width: 300px;
      color: #abacb0;

      & > * {
        padding-left: 10px;
        display: inline-block;
        cursor: pointer;
      }

      .text_back {
        color: #142338;
        font-size: 12px;
      }

      .text_back:hover {
        color: #017bff;
      }

      .text_modify {
        color: #142338;
        font-size: 12px;
      }

      .text_modify:hover {
        color: #017bff;
      }

      .jiangefu {
        font-size: 12px;
        color: #142338;
        margin-top: -1px;
      }

      .user-name {
        cursor: default;
        font-size: 12px;
        margin-right: 10px;

        span {
          color: $color-primary;
          font-size: 12px;
          font-weight: bold;
          margin-left: 7px;
        }
      }
    }
  }

  .content {
    border-top: 1px solid #f8f8f8;
    height: initial;

    .aside {
      border-right: 1px solid #ededed;
      width: 240px !important;

      ::v-deep(.el-menu) {
        border-right: none;
      }

      ::v-deep(.el-menu-item) {
        padding-left: 20px !important;
      }
    }

    .main {
      display: flex;
      height: inherit;
      background: $color-background;
    }
  }
}

.el-scrollbar-main {
  height: 100%;
  width: 100%;
}

.el-scrollbar-main ::v-deep(.el-scrollbar__wrap) {
  width: 110%;
  height: 100%;
}

.active-text-color {
  color: #142338;
  font-weight: bold;

  i {
    color: $color-primary;
    font-weight: normal;
  }
}

.idle-text-color {
  color: #abacb0;
}

.el-form-item__label {
  color: #4e5462;
}
</style>