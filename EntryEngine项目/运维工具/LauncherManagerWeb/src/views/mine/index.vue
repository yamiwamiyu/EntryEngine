<template>
  <div class="container">
    <img class="photo" alt="photo" src="../../assets/logo.png" />
    <!--<van-cell-group inset>
      <van-cell title="服务管理" size="Large" icon="cluster-o" is-link url="/mineService" />
      <van-cell title="账号管理" icon="manager-o" is-link url="/mineUser" />
      <van-cell title="退出" icon="close" is-link @click="logout()" />
    </van-cell-group>-->
    <div class="group">
      <router-link :to="'/mineService'" class="cell">
        <van-icon class="van-icon-left" name="cluster-o" size="1rem"></van-icon>服务管理
        <van-icon class="van-icon-right" name="arrow" size="1rem"></van-icon>
      </router-link>
      <van-divider />

      <router-link :to="'/mineUser'" class="cell">
        <van-icon class="van-icon-left" name="manager-o" size="1rem"></van-icon>账号管理
        <van-icon class="van-icon-right" name="arrow" size="1rem"></van-icon>
      </router-link>
      <van-divider></van-divider>

      <router-link :to="'/'" class="cell">
        <van-icon class="van-icon-left" name="close" size="1rem"></van-icon>退出
      </router-link>
    </div>
    <Nav></Nav>
  </div>
</template>

<script>
import Nav from "@/components/Nav";

export default {
  components: {
    Nav,
  },
};
</script>

<style lang="scss" scoped>
  .container {
    .photo {
      display: block;
/*      width: 400px;
      height: 400px;*/
      margin: 30px auto;
      padding: 0;
      background: white;
      border-radius: 10%;
      object-fit: none;
    }

    .group {
      margin-left: auto;
      margin-right: auto;
      margin-top: 30px;
      width: 700px;
      border-radius: 20px;
      background-color: white;

      .cell {
        display: block;
        padding: 20px;
        color: #7d7e80;

        .van-icon-left {
          margin: 30px;
        }

        .van-icon-right {
          margin: 30px;
          float: right;
        }
      }
    }

    .van-divider {
      width: 95%;
      margin-left: auto;
      margin-right: auto;
      margin-top: 0px;
      margin-bottom: 0px;
    }
  }
</style>