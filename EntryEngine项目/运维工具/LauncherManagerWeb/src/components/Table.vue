<template>
  <table class="table">
    <thead>
      <tr align="left">
        <th v-if="choose" class="selector"></th>
        <th
          v-for="(item, index) in thead"
          :key="'thead-' + index"
          @click.stop="clickTile(index)"
          :style="theadStyle ? theadStyle[index] : null"
        >
          {{ item }}
        </th>
      </tr>
    </thead>
    <tbody>
      <tr
        align="left"
        valign="top"
        v-for="(item, index) in list"
        :key="'tbody-' + index"
        @click.stop="clickRow(item)"
      >
        <td v-if="choose" @click.stop style="padding: 0">
          <van-checkbox
            style="justify-content: center; padding: 10px 5px"
            v-model="chooseValues[index]"
            @click.stop="onChoose(index)"
          ></van-checkbox>
        </td>
        <td
          v-for="(v, k) in tbody"
          :key="'tbody-td-' + k"
          v-html="item[v]"
          :style="tbodyStyle ? tbodyStyle[k] : null"
        ></td>
      </tr>
    </tbody>
  </table>
</template>

<script>
export default {
  components: {},
  props: {
    list: Array,
    // :option="{ 列名: list索引 }"
    option: Object,
    // 是否可选(0: 不可选，1：单选，2：多选)
    choose: {
      type: Number,
      default: 0,
      validator: function (value) {
        return [0, 1, 2].includes(value);
      },
    },
    theadStyle: Object,
    tbodyStyle: Object,
  },
  data() {
    return {
      // bool[]，是否选中
      chooseValues: [],
      // int[]，选中的索引
      chooseList: [],
      nextChoose: null,
    };
  },
  computed: {
    thead() {
      let list = [];
      for (const key in this.option) {
        list.push(key);
      }
      return list;
    },
    tbody() {
      let list = [];
      for (const key in this.option) {
        list.push(this.option[key]);
      }
      return list;
    },
    selected() {
      let result = [];
      for (let i = 0; i < this.chooseValues.length; i++)
        if (this.chooseValues[i])
          result.push(this.list[i]);
      return result;
    },
  },
  watch: {
    list: {
      handler: function (val) {
        console.log("table", val);
      },
      deep: true,
    },
  },
  methods: {
    // 点击行
    clickRow(row) {
      this.$emit("clickRow", row);
    },
    // 点击选择
    onChoose(index) {
      console.log("点击选择", index, this.chooseValues);
      if (this.choose == 1) {
        console.log("单选", this.nextChoose);
        if (this.nextChoose != index) {
          if (this.nextChoose != null) {
            this.chooseValues[this.nextChoose] = false;
          }
          this.nextChoose = index;
        }
      }
      this.chooseList = [];
      for (const key in this.chooseValues) {
        if (this.chooseValues[key]) {
          this.chooseList.push(key);
        }
      }
      this.$emit("chooseClick", this.chooseList);
      this.$forceUpdate();
    },
    // 初始化选项
    initChooseValues() {
      let v = [];
      if (this.choose && this.list.length) {
        for (let i = 0; i < this.list.length; i++, v.push(false));
      }
      this.chooseValues = v;
    },
    // 清空所有选择
    clearChoose() {
      if (this.choose && this.list.length) {
        for (let i = 0; i < this.list.length; i++) {
          this.chooseValues[i] = false;
        }
      }
      this.chooseList = [];
      this.$forceUpdate();
    },
    // 是否已选择验证
    verifyChoose(errMsg) {
      return new Promise((resolve, reject) => {
        if (this.chooseList.length) {
          resolve();
        } else {
          this.$Toast(errMsg ? errMsg : "请选择数据");
          reject();
        }
      });
    },
    // 点击名称
    clickTile(index) {
      this.$emit("clickTile", index);
    },
  },
  mounted() {
    this.initChooseValues();
    console.log("props--->", this.$props);
  },
};
</script>

<style scoped lang='scss'>
  .table {
    width: 100%;
    height: auto;
    overflow: auto;
    border: none;
    border-collapse: collapse;
    font-size: 24px;
    line-height: 32px;

    tr {
      border-bottom: 1px solid #ccc;

      td,
      th {
        border: none;
        padding: 10px 5px;
      }
    }

    .selector {
      width: 70px;
    }
  }
</style>