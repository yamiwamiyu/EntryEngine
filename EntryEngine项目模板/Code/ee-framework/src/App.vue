<template>
  <router-view/>
</template>

<script type="text/javascript">

export default {
  name: "App",
  data() {
    return {
       //a: 1,
    }
  },
  props: {
    //disabled: Boolean,
    //id: {
    //  type: Number,
    //  default: 0
    //},
  },
  // 计算属性，有依赖其它属性时，且其它属性没有发生变化，计算属性将不会重新计算而使用缓存
  computed: {
    // 仅读取
    //aDouble: function () { return this.a * 2 },
    // 读取和设置
    //aPlus: {
    //  get: function () { return this.a + 1 },
    //  set: function (v) { this.a = v - 1 }
    //}
  },
  methods: {
    //plus: function () { this.a++ },
  },
  watch: {
    //a: function (val, oldVal) { },
  },
  mounted() {
  },
}
</script>

<style lang="scss" scoped>
</style>