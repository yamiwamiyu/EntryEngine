<template>
  <div class="container">
    <Table
      ref="table"
      :list="list"
      :option="{ IP: 'EndPoint', 名称: 'NickName' }"
    ></Table>
    <Nav></Nav>
    <FloatBtn :list="['更新']" @choose="chooseFloatBtn"></FloatBtn>
  </div>
</template>

<script>
import Nav from "@/components/Nav";
import Table from "@/components/Table";
import FloatBtn from "@/components/FloatBtn";

export default {
  name: "Home",
  components: {
    Nav,
    Table,
    FloatBtn,
  },
  data() {
    return {
      list: [],
    };
  },
  methods: {
    getServers() {
      this.$IMBSProxy.GetServers((res) => {
        console.log("list", res);
        this.list = res;
      });
    },
    chooseFloatBtn(index) {
      console.log("chooseFloatBtn", index);
      this.$IMBSProxy.UpdateServer((res) => {
        if (res) {
          this.$Toast("更新成功");
          this.getServers();
        }
      });
    },
  },
  mounted() {
    this.getServers();
  },
};
</script>
