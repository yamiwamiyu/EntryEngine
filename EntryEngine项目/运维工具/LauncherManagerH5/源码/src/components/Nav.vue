<template>
  <van-tabbar route>
    <van-tabbar-item replace to="/home" icon="desktop-o"> 服务器 </van-tabbar-item>
    <van-tabbar-item replace to="/services" icon="bars"> 服务 </van-tabbar-item>
    <van-tabbar-item replace to="/mine" icon="contact"> 我的 </van-tabbar-item>
  </van-tabbar>
</template>

<script>
export default {
  name: "Nav",
  components: {},
  props: {},
  data() {
    return {};
  },
  computed: {},
  watch: {},
  methods: {},
  mounted() {},
};
</script>

<style scoped lang='scss'>
</style>