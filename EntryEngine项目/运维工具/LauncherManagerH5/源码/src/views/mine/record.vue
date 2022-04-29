<template>
  <div class="container">
    <van-nav-bar
      title="日志"
      left-arrow
      @click-left="$router.go(-1)"
      style="width: 100%"
    />
  </div>
</template>

<script>
export default {
  name: "Record",
  data() {
    return {};
  },
  methods: {},
};
</script>

<style lang="scss" scoped>
.container {
  display: flex;
  height: 100%;
  align-items: center;
  flex-direction: column;
}
</style>
