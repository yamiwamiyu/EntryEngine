<template>
  <div class="container">
    <van-nav-bar title="登录" style="width: 100%" />
    <van-form @submit="onSubmit" class="form">
      <input-select
        v-model="form.username"
        name="username"
        label="用户名"
        placeholder="用户名"
        :rules="[{ required: true, message: '请填写用户名' }]"
        :list="user"
        :search="false"
        @del="userDel($event)"
      ></input-select>
      <van-field
        v-model="form.password"
        type="password"
        name="password"
        label="密码"
        placeholder="密码"
        :rules="[{ required: true, message: '请填写密码' }]"
      />
      <input-select
        v-model="form.serve"
        name="serve"
        label="地址"
        placeholder="服务器、端口"
        :rules="[{ required: true, message: '请填写地址' }]"
        :list="services"
        :search="false"
        @del="serveDel($event)"
      ></input-select>
      <div style="margin-top: 30px; width: 100%;">
        <van-button round block type="primary" native-type="submit">
          登录
        </van-button>
      </div>
    </van-form>
  </div>
</template>

<script>
import InputSelect from "@/components/InputSelect";
import storage from "@/store/storage";
export default {
  components: {
    InputSelect,
  },
  data() {
    return {
      form: {
        username: "",
        password: "",
        serve: "",
      },
      services: [],
      user: [],
    };
  },
  methods: {
    onSubmit(form) {
      console.log("submit", form);
      this.$IMBSProxy.url = `http://${form.serve}/`;
      this.$IMBSProxy.Connect(form.username, form.password, (res) => {
        console.log("---->", res);
        this.$store.commit("login", {
          token: res,
          services: form.serve,
          username: form.username,
        });
        this.$router.push("/services");

        localStorage.setItem("token", res);
      });
    },
    userDel(index) {
      console.log("userDel", index);
      storage.userDel(index);
      this.init();
    },
    serveDel(index) {
      console.log("serveDel", index);
      this.services = storage.serviceDel(index);
      this.init();
    },
    init() {
      this.services = storage.serviceGet();
      this.user = storage.userGet();
      if (this.user.length) {
        this.form.username = this.user[this.user.length - 1];
      }
      if (this.services.length) {
        this.form.serve = this.services[this.services.length - 1];
      }
      console.log("init", this.form);
    },
  },
  mounted() {
    this.init();
  },
};
</script>

<style lang="scss" scoped>
.container {
  display: flex;
  height: 100%;
  align-items: center;
  flex-direction: column;

  .form {
    margin-top: -50px;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    flex: 1;
  }
}
</style>
