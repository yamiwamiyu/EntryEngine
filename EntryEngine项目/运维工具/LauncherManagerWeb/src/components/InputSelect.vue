<template>
  <div
    class="input-select"
    :id="'inputSelect' + id"
    :key="'input-select-' + id"
    @click.stop
  >
    <van-field
      v-model="inputValue"
      :name="name"
      :label="label"
      :placeholder="placeholder"
      :rules="rules"
      @focus="inputFocus()"
      @blur="inputBlur()"
    />
    <div
      class="list"
      :style="
        listPosition == 'top' ? 'bottom: ' + inputSelectHeight + 'px;' : null
      "
      v-show="showList"
    >
      <ul>
        <li
          v-for="(item, index) in searchList"
          :key="'input-select-' + id + '-list-' + +index"
          @click.stop="select(index)"
        >
          <span>{{ listKey ? item[listKey] : item }}</span>
          <van-icon
            v-show="inputType != 1"
            name="cross"
            @click.stop="del(index)"
          />
        </li>
      </ul>
    </div>
  </div>
</template>

<script>
export default {
  name: "InputSelect",
  components: {},
  props: {
    value: {
      type: String,
    },
    // 输入模式（0: 可以输入、选择、删除选项， 1：可选择、不可删除选项）
    inputType: {
      type: Number,
      default: 0,
      validator: function (value) {
        return [0, 1].includes(value);
      },
    },
    search: {
      type: Boolean,
      default: true,
    },
    name: {
      type: String,
    },
    label: {
      type: String,
    },
    placeholder: {
      type: String,
    },
    rules: {
      type: Array,
    },
    list: {
      type: Array,
    },
    listKey: String,
    bindKey: String,
    listPosition: {
      type: String,
      default: "bottom",
      validator: function (value) {
        return ["bottom", "top"].includes(value);
      },
    },
  },
  data() {
    return {
      inputValue: "",
      showList: false,
      showListEnable: true,
      bindValue: "",
      inputSelectHeight: 44,
      isSelect: false,
    };
  },
  computed: {
    id() {
      return this._uid;
    },
    searchList() {
      if (this.search) {
        let list = this.list.filter((item) => {
          const v = this.listKey ? item[this.listKey] : item;
          return v ? v.indexOf(this.inputValue) != -1 : false;
        });
        return list;
      } else {
        return this.list;
      }
    },
  },
  watch: {
    value: function (val) {
      !this.isSelect && (this.inputValue = val);
      this.isSelect = false;
    },
    inputValue: function (val) {
      this.$emit("update:model-value", this.bindKey ? this.bindValue : val);
    },
  },
  methods: {
    inputFocus() {
      document.body.click();
      this.setInputSelectHeight();
      this.showList = true;
    },
    inputBlur() {
      if (this.inputType == 1) {
        this.inputValue = "";
      }
    },
    del(index) {
      console.log("del", index);
      this.$emit("del", index);
      // if (this.inputValue == this.list[index]) {
      //   this.inputValue = "";
      // }
    },
    select(index) {
      this.setInputSelectHeight();
      this.showList = false;
      this.bindValue = this.bindKey
        ? this.list[index][this.bindKey].toString()
        : this.list[index];
      this.inputValue = this.listKey
        ? this.list[index][this.listKey]
        : this.list[index];
      this.isSelect = true;
    },
    setInputSelectHeight() {
      this.$nextTick().then(() => {
        setTimeout(() => {
          this.inputSelectHeight = document.getElementById(
            "inputSelect" + this.id
          ).clientHeight;
        }, 500);
      });
    },
  },
  mounted() {
    this.$nextTick(() => {
      document.body.addEventListener("click", () => {
        this.showList = false;
      });
    });
  },
};
</script>

<style scoped lang='scss'>
.input-select {
  position: relative;
  height: auto;
  width: 100%;

  .list {
    display: flex;
    position: absolute;
    z-index: 1;
    left: 0;
    width: 100%;
    background: white;
    box-shadow: 0 3px 5px #ccc;

    ul {
      list-style: none;
      max-height: 120px;
      overflow-y: auto;
      width: 100%;

      li {
        padding: 10px 16px;
        display: flex;
        justify-content: space-between;
        cursor: pointer;
      }
    }
  }
}
</style>