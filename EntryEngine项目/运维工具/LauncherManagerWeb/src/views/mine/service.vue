<template>
  <div class="container">
    <van-nav-bar
      title="服务管理"
      left-arrow
      @click-left="$router.go(-1)"
      style="width: 100%"
    />
    <Table
      ref="table"
      :list="list"
      :option="{ 名称: 'Name', SVN目录: 'SVNPath' }"
      :choose="1"
      @chooseClick="chooseClick"
    ></Table>
    <FloatBtn :list="floatBtnList" @choose="chooseFloatBtn($event)"></FloatBtn>

    <!-- 添加修改 -->
    <van-dialog
      v-model:show="dialog.show"
      :title="dialog.title"
      :before-close="dialogClose"
      show-cancel-button
      width="15rem"
    >
      <van-form @submit="onSubmit" ref="from" label-width="3.5rem">
        <van-field
          v-for="(item, index) in from"
          :key="'field-' + index"
          v-model="item.value"
          :name="item.key"
          :label="item.title"
          :rules="item.rules"
          :disabled="item.disabled"
          :type="item.type"
        />
      </van-form>
    </van-dialog>
  </div>
</template>

<script>
import Table from "@/components/Table";
import FloatBtn from "@/components/FloatBtn";
export default {
  components: {
    Table,
    FloatBtn,
  },
  data() {
    return {
      list: [],
      dialog: {
        title: "添加",
        show: false,
      },
      from: [
        {
          title: "名称",
          key: "Name",
          value: "",
          disabled: false,
          rules: [{ required: true, message: "请填写名称" }],
        },
        {
          title: "SVN目录",
          key: "SVNPath",
          value: "",
          disabled: false,
          rules: [{ required: true, message: "请填写SVN目录" }],
        },
        {
          title: "SVN账号",
          key: "SVNUser",
          value: "",
          disabled: false,
          rules: [{ required: true, message: "请填写SVN账号" }],
        },
        {
          title: "SVN密码",
          key: "SVNPassword",
          value: "",
          disabled: false,
          rules: [{ required: true, message: "请填写SVN密码" }],
        },
        {
          title: "启动文件",
          key: "Exe",
          value: "",
          disabled: false,
        },
        {
          title: "启动命令",
          key: "LaunchCommand",
          value: "",
          disabled: false,
          type: "textarea",
        },
      ],
      selectRow: {},
    };
  },
  computed: {
    floatBtnList() {
      return [this.dialog.title, "删除"];
    },
  },
  methods: {
    getServiceType() {
      this.$IMBSProxy.GetServiceType((res) => {
        this.list = res;
      });
    },
    chooseFloatBtn(index) {
      switch (index) {
        case 0:
          console.log(this.dialog.type);
          if (this.dialog.title == "添加") {
            // 添加
            this.from = JSON.parse(JSON.stringify(this.$options.data().from));
          } else {
            // 修改
            for (const key in this.from) {
              this.from[key].value = this.selectRow[this.from[key].key];
              if (key < 4) this.from[key].disabled = true;
              JSON.parse(JSON.stringify(this.from));
            }
          }
          this.dialog.show = true;
          break;

        //删除
        case 1:
          this.$refs.table
            .verifyChoose()
            .then(() => {
              this.$Dialog
                .confirm({
                  message: "是否确定删除",
                })
                .then(() => {
                  this.$IMBSProxy.DeleteServiceType(
                    this.selectRow.Name,
                    (res) => {
                      if (res) {
                        this.$Toast("删除成功");
                        this.getServiceType();
                        this.$refs.table.clearChoose();
                        this.reInit();
                      }
                    }
                  );
                })
                .catch(() => {});
            })
            .catch(() => {});
          break;

        default:
          break;
      }
    },
    dialogClose(action) {
      if (action == "confirm") {
        let data = this.$refs.from.getValues();
        return new Promise((resolve) => {
          this.$IMBSProxy.ModifyServiceType(data,
            (res) => {
              if (res) {
                this.$Toast(this.dialog.title + "成功");
                this.dialog.show = false;
                this.getServiceType();
                this.$refs.table.clearChoose();
                this.reInit();
              }
            }).then(() => resolve(true)).catch(() => resolve(false))
        })
      }
      return true;
    },
    chooseClick(val) {
      if (val.length) {
        this.selectRow = this.list[val[0]];
        this.dialog.title = "修改";
      } else {
        this.reInit();
      }
    },
    reInit() {
      this.selectRow = null;
      this.dialog.title = "添加";
    },
  },
  mounted() {
    this.getServiceType();
  },
};
</script>

<style lang="scss" scoped>
  tbodyStyle1 {
  }
</style>
