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
      :tbodyStyle="{
        1: 'width: 200px',
      }"
      :option="{ 名称: 'Name', SVN目录: 'SVNPath' }"
      :choose="1"
      @chooseClick="chooseClick"
    ></Table>
    <FloatBtn :list="floatBtnList" @choose="chooseFloatBtn($event)"></FloatBtn>

    <!-- 添加修改 -->
    <van-dialog
      v-model="dialog.show"
      :title="dialog.title"
      :before-close="dialogClose"
      show-cancel-button
    >
      <van-form @submit="onSubmit" ref="from">
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
  name: "Record",
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
        type: "add",
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
          if (this.dialog.type == "add") {
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
    onSubmit(values) {
      console.log("submit", values);
      this.$IMBSProxy.ModifyServiceType(values, (res) => {
        if (res) {
          this.$Toast(this.dialog.title + "成功");
          this.dialog.show = false;
          this.getServiceType();
          this.$refs.table.clearChoose();
          this.reInit();
        }
      });
    },
    dialogClose(action, done) {
      if (action == "confirm") {
        this.$refs.from.submit();
        done(false);
      } else {
        done();
      }
    },
    chooseClick(val) {
      if (val.length) {
        this.selectRow = this.list[val[0]];
        this.dialog.title = "修改";
        this.dialog.type = "change";
      } else {
        this.reInit();
      }
    },
    reInit() {
      this.dialog.title = "添加";
      this.dialog.type = "add";
    },
  },
  mounted() {
    this.getServiceType();
  },
};
</script>

<style lang="scss" scoped>
</style>
