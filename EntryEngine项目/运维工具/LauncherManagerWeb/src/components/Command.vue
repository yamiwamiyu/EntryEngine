<template>
  <van-dialog width="15rem"
    v-model:show="commandShow"
    title="命令"
    :before-close="commandClose"
    show-cancel-button
  >
    <template #title>
      <span v-if="commands.length == 0">命令</span>
      <select ref="select" v-if="commands.length > 0" @change="selectCommand">
        <option class="optitle">选择命令</option>
        <option class="op" v-for="(item, index) in commands" :value="item">{{item}}</option>
      </select>
      <!--<van-dropdown-menu v-if="commands.length > 0" active-color="#1989fa">
        <van-dropdown-item ref="dd" id="scmd" title="选择命令" @change="selectCommand" :options="getCommandsList()" />
      </van-dropdown-menu>-->
    </template>
    <van-form label-width="4rem"
      @submit="onSubmitCommand"
      ref="commandFrom"
      validate-trigger="onSubmit"
    >
      <van-field
        v-model="commandFrom.LaunchCommand"
        name="LaunchCommand"
        label="执行命令"
        type="textarea"
        rows="10"
        placeholder="输入命令"
        :rules="[{ required: true, message: '请输入命令' }]"
      />
    </van-form>
  </van-dialog>
</template>

<script>
export default {
  name: "CommandComponent",
  components: {},
  props: {
    chooseList: Array,
    show: {
      type: Boolean,
      default: false,
    },
  },
  data() {
    return {
      commandShow: false,
      commandFrom: {
        LaunchCommand: "",
      },
      commands: [],
    };
  },
  computed: {},
  watch: {
    show: function (val) {
      this.commandShow = val;
      this.commandFrom.LaunchCommand = "";
      if (val) {
        this.getCommands()
      }
    },
  },
  methods: {
    commandClose(action, done) {
      if (action == "confirm") {
        return new Promise((resolve) => {
          this.$IMBSProxy.CallCommand(
            this.chooseList,
            this.commandFrom.LaunchCommand,
            (res) => {
              if (res) {
                this.$Toast("操作成功");
                this.$emit("submit");
                this.$emit("close");
              }
            }
          ).then(() => resolve(true)).catch(() => resolve(false))
        })
      } else {
        this.$emit("close");
      }
      return true;
    },

    getCommands() {
      this.$IMBSProxy.GetCommands(this.chooseList[0], ret => this.commands = ret);
    },

    getCommandsList() {
      console.log("getCommandsList", this.$refs.dd)
      return this.commands;
    },

    selectCommand() {
      const select = this.$refs.select;
      console.log("selectCommand", select.options[select.selectedIndex])
      if (select.selectedIndex > 0) {
        this.commandFrom.LaunchCommand += select.options[select.selectedIndex].value + "\r\n";
        select.selectedIndex = 0;
      }
    }
  },
  mounted() { },
};
</script>

<style scoped lang='scss'>
.container {
  display: flex;

  select {
    /*text-align: center;*/

    .optitle {
      text-align: center;
      color: red,
    }

    .op {

    }
  }
}
</style>