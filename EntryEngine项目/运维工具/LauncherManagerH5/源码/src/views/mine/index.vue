<template>
  <div class="container">
    <img class="photo" alt="photo" src="../../assets/logo.png" />
    <van-cell-group>
      <!-- <van-cell title="显示日志" is-link url="#/mineRecord" /> -->
      <van-cell title="服务管理" is-link url="#/mineService" />
      <van-cell title="账号管理" is-link url="#/mineUser" />
      <van-cell title="退出" is-link @click="logout()" />
    </van-cell-group>
    <Nav></Nav>
  </div>
</template>

<script>
import Nav from "@/components/Nav";

export default {
  name: "Mine",
  components: {
    Nav,
  },
  methods: {
    logout() {
      this.$store.commit("logout");
      this.$router.push("/");
    },
  },
};
</script>

<style lang="scss" scoped>
.container {
  .photo {
    display: block;
    width: 100px;
    height: 100px;
    margin: 30px auto;
    padding: 0;
    background: white;
    border-radius: 50%;
    object-fit: cover;
  }
}
</style>