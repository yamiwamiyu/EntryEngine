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
  height: 100vh;
  width: 100vw;
  overflow: hidden;
}
</style>
