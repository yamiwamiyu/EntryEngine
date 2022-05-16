<template>
  <div class="nav">
    <router-link v-for="item in nav" :key="item.title" :to="item.url" :class="'item ' + isSelected(item)">
      <van-icon :name="item.icon" size="1.5rem" class="icon" />
      <span class="title">{{item.title}}</span>
    </router-link>
  </div>
</template>

<script>
export default {
  data() {
    return {
      nav: [
        {
          title: "服务",
          url: "/services",
          icon: "desktop-o",
        },
        {
          title: "管理",
          url: "/mine",
          icon: "manager-o",
        },
      ]
    };
  },
    methods: {
      isSelected(item) {
        return location.pathname == item.url ? "selected" : "unselected";
      }
    }
};
</script>

<style scoped lang='scss'>
  .nav {
    background-color: white;
    width: 750px;
    height: 98px;
    position: fixed;
    bottom: 0;
    display: flex;
    justify-content: center;
    align-items: center;

    .item {
      flex: 1;
      height: 100%;
      justify-content: center;
      align-items: center;
      display: flex;

      .title {
        vertical-align: top;
        padding: 20px;
      }
    }

    .unselected {
      color: #7d7e80;
    }

    .unselected:hover {
      color: #4fc08d;
    }

    .selected {
      color: #1989fa;
    }
  }
</style>