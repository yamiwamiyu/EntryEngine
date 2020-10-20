<template>
  <div class="float-btn">
    <div
      class="btn-list"
      :style="'height: ' + (hidden && !showMore ? 60 * showNum + 'px' : 'auto')"
    >
      <div
        class="btn"
        v-for="(item, index) in list"
        :key="'float-btn-' + index"
        @click.stop="choose(index)"
      >
        <span>{{ item }}</span>
      </div>
    </div>
    <div
      class="btn"
      :key="'float-btn-more'"
      @click.stop="showMore = !showMore"
      v-show="hidden"
    >
      <van-icon :name="showMore ? 'arrow-up' : 'arrow'" />
    </div>
  </div>
</template>

<script>
export default {
  name: "ComponentName",
  components: {},
  props: {
    list: Array,
    showNum: {
      type: Number,
      default: 3,
    },
  },
  data() {
    return {
      showMore: false,
    };
  },
  computed: {
    hidden() {
      return this.list.length > this.showNum;
    },
  },
  methods: {
    choose(index) {
      this.$emit("choose", index);
    },
  },
  mounted() {},
};
</script>

<style lang="scss" scoped>
.container {
  .float-btn {
    position: fixed;
    bottom: 60px;
    right: 0;
    display: flex;
    flex-direction: column;
    overflow: hidden;

    .btn-list {
      display: flex;
      flex-direction: column;
      overflow: hidden;
    }

    .btn {
      margin: 10px;
      border-radius: 50%;
      background: white;
      width: 40px;
      height: 40px;
      line-height: 40px;
      text-align: center;
      font-size: 12px;
      color: $theme-color;
      cursor: pointer;
    }
  }
}
</style>