<template>
  <div class="float-btn">
    <div class="btn-list" >
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
        v-for="(item, index) in showList"
        :key="'float-btn-' + index"
        @click.stop="choose(index, item)"
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
      <van-icon :name="showMore ? 'arrow-up' : 'arrow-down'" />
    </div>
  </div>
</template>

<script>
export default {
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
    showList() {
      let result = [];
      let min = this.list.length;
      if (!this.showMore && this.showNum < min)
        min = this.showNum;
      for (let i = 0; i < min; i++)
        result.push(this.list[i]);
      return result;
    },
  },
  methods: {
    choose(index, item) {
      this.$emit("choose", index, item);
    },
  },
  mounted() {},
};
</script>

<style lang="scss" scoped>
.container {
  .float-btn {
    position: absolute;
    right: 0;
    display: flex;
    flex-direction: column;
    overflow: hidden;
    justify-content: flex-end;
    /*height: 100vh;*/
    bottom: 98px;

    .btn-list {
      display: flex;
      flex-direction: column;
      overflow: hidden;
    }

    .btn {
      margin: 10px;
      border-radius: 50%;
      background: rgba(0, 0, 0, 0.5);
      width: 60px;
      height: 60px;
      line-height: 60px;
      text-align: center;
      font-size: 14px;
      color: white;
      cursor: pointer;
    }
  }
}
</style>