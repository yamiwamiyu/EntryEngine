<template>
  <van-dialog
    v-model="commandShow"
    title="命令"
    :before-close="commandClose"
    show-cancel-button
  >
    <van-form
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
    };
  },
  computed: {},
  watch: {
    show: function (val) {
      this.commandShow = val;
      this.commandFrom.LaunchCommand = "";
    },
  },
  methods: {
    commandClose(action, done) {
      if (action == "confirm") {
        this.$refs.commandFrom.submit();
        done(false);
      } else {
        done();
        this.$emit("close");
      }
    },
    onSubmitCommand() {
      console.log("onSubmitCommand===》", this.commandFrom);
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
      );
    },
  },
  mounted() {},
};
</script>

<style scoped lang='scss'>
.container {
  display: flex;
}
</style>