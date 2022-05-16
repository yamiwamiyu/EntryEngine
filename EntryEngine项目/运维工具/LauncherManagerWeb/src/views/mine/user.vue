<template>
  <div class="container">
    <van-nav-bar
      title="账号管理"
      left-arrow
      @click-left="$router.go(-1)"
      style="width: 100%"
    />
    <Table
      ref="table"
      :list="list"
      :option="{ 账号: 'Name', 密码: 'Password', 角色: 'role' }"
      :choose="2"
    ></Table>
    <FloatBtn
      :list="['添加', '删除']"
      @choose="chooseFloatBtn($event)"
    ></FloatBtn>
    <!-- 添加 -->
    <van-dialog
      v-model:show="dialogShow"
      title="添加"
      :before-close="dialogClose"
      show-cancel-button
    >
      <van-form @submit="onSubmit" ref="from" v-if="dialogShow">
        <van-field
          v-model="from.Name"
          name="Name"
          label="账号"
          :rules="[{ required: true, message: '请填写账号' }]"
        />
        <van-field
          v-model="from.Password"
          name="Password"
          label="密码"
          :rules="[{ required: true, message: '请填写密码' }]"
        />
        <input-select
          v-model="from.Security"
          name="Security"
          label="角色"
          @input ="test"
          :rules="[{ required: true, message: '请选择角色' }]"
          :inputType="1"
          :list="role"
          listPosition="top"
          :search="false"
        ></input-select>
      </van-form>
    </van-dialog>
  </div>
</template>

<script>
import Table from "@/components/Table";
import FloatBtn from "@/components/FloatBtn";
import InputSelect from "@/components/InputSelect";
export default {
  components: {
    Table,
    FloatBtn,
    InputSelect,
  },
  data() {
    return {
      list: [],
      role: [ "程序员", "运维", "管理员"],
      dialogShow: false,
      from: {
        Name: "",
        Password: "",
        Security: "",
      },
    };
  },
  methods: {
    test(index) {
      this.from.Security = index;
    },
    getManagers() {
      this.$IMBSProxy.GetManagers((res) => {
        for (const key in res) {
          res[key].role = this.role[res[key].Security];
        }
        this.list = res;
      });
    },
    chooseFloatBtn(index) {
      switch (index) {
        // 添加
        case 0:
          this.from = JSON.parse(JSON.stringify(this.$options.data().from));
          this.dialogShow = true;
          break;
        // 删除
        case 1:
          console.log(this.$refs.table.selected)
          this.$refs.table
            .verifyChoose()
            .then(() => {
              this.$Dialog
                .confirm({
                  message: "是否确定删除",
                })
                .then(() => {
                  let d = this.$refs.table.chooseList.length;
                  for (let i = 0; i < d; i++) {
                    this.$IMBSProxy.DeleteManager(
                      this.$refs.table.selected[i].Name,
                      (res) => {
                        d--;
                        if (d == 0) {
                          this.$Toast("删除成功");
                          this.getManagers();
                        }
                      }
                    );
                  }
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
        return new Promise(resolve => {
          this.$IMBSProxy.NewManager(
            {
              Name: this.from.Name,
              Password: this.from.Password,
              Security:
                this.role.indexOf(this.from.Security) != -1
                  ? this.role.indexOf(this.from.Security)
                  : 0,
            },
            (res) => {
              if (res) {
                this.$Toast("添加成功");
                this.dialogShow = false;
                this.getManagers();
                this.$refs.table.clearChoose();
              }
            }
          ).then(() => resolve(true)).catch(() => resolve(false))
        })
      }
      return true;
    },
  },
  mounted() {
    this.getManagers();
  },
};
</script>

<style lang="scss" scoped>
.container {
  display: flex;
  height: 100%;
  align-items: center;
  flex-direction: column;
}
</style>
