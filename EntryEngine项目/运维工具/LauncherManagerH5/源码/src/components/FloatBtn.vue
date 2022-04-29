<template>
  <div class="float-btn">
    <div
      class="btn-list"
      :style="'height: ' + (hidden && !showMore ? 60 * showNum + 'px' : 'auto')"
    >
      <div
        class="btn"
        :key="'float-btn-top'"
        @click.stop="choose('goTop')"
        v-show="goTop"
      >
        <span>置顶</span>
      </div>
      <div
        class="btn"
        v-for="(item, index) in list"
        :key="'float-btn-' + index"
        @click.stop="choose(index)"
      >
        <span>{{ item }}</span>
      </div>
      <div
        class="btn"
        :key="'float-btn-bottom'"
        @click.stop="choose('goBottom')"
        v-show="goBottom"
      >
        <span>触底</span>
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
    goTop: Boolean,
    goBottom: Boolean,
  },
  data() {
    return {
      showMore: false,
    };
  },
  computed: {
    hidden() {
      return this.list && this.list.length > this.showNum;
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
    right: 0;
    display: flex;
    flex-direction: column;
    overflow: hidden;
    justify-content: flex-end;
    height: 100vh;
    bottom: 60px;

    .btn-list {
      display: flex;
      flex-direction: column;
      overflow: hidden;
    }

    .btn {
      margin: 10px;
      border-radius: 50%;
      background: rgba(0, 0, 0, 0.5);
      width: 40px;
      height: 40px;
      line-height: 40px;
      text-align: center;
      font-size: 12px;
      color: white;
      cursor: pointer;
    }
  }
}
</style>