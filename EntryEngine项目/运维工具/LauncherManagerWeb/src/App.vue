<template>
  <div id="app">
    <router-view />
  </div>
</template>

<script>
export default {
  name: "App",
  mounted() {
    // 错误全局处理
    this.$IMBSProxy.onErrorMsg = (res) => {
      this.$Toast(res.errMsg);
      if (res.errCode == 403) {
        this.$store.commit("logout");
        this.$router.push("/");
      }
    };
    this.$IMBSProxy.onError = () => {
      this.$Notify({ type: "danger", message: "请求服务不存在" });
    };
    if (this.$IMBSProxy.url) {
      this.$store.commit("logout");
      this.$router.push("/");
    }
  },
};
</script>
<style lang="scss" scoped>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  background-color: #f7f8fa;
  /*height: 100vh;
  width: 100vw;*/
  width: 750px;
  height: 100vh;
  position: relative;
  margin-left: auto;
  margin-right: auto;
  overflow: hidden;
}

  ::-webkit-scrollbar {
    width: 5px;
    height: 5px;
  }
  ::-webkit-scrollbar-thumb {
    border-radius: 1em;
    background-color: rgba(50, 50, 50, .3);
  }
  ::-webkit-scrollbar-track {
    border-radius: 1em;
    background-color: rgba(50, 50, 50, .3);
  }
  // 滚动条隐藏时可以滚动，滚动完又隐藏
  .demo::-webkit-scrollbar {
    display: none;
  }
  .demo {
    scrollbar-width: none;
    -ms-overflow-style: none;
    overflow-x: hidden;
    overflow-y: auto;
  }
</style>
