<template>
  <div class="container">
    <van-pull-refresh v-model="isLoading" @refresh="getServers(true)">
      <Table
        ref="table"
        :list="filterServices"
        :option="{ 名称: 'newName', 服务版本: 'info', 状态: 'state' }"
        :theadStyle="{ 1: 'cursor: pointer' }"
        :tbodyStyle="{
          0: 'cursor: pointer',
          1: 'cursor: pointer',
          2: 'cursor: pointer',
        }"
        :choose="2"
        @chooseClick="chooseClick"
        @clickTile="clickTile"
        @clickRow="clickRow"
        :rowPointer="true"
      ></Table>
    </van-pull-refresh>
    <!-- 添加 -->
    <van-dialog
      v-model="dialog.show"
      :title="dialog.title"
      :before-close="dialogClose"
      show-cancel-button
    >
      <van-form @submit="onSubmit" ref="from" validate-trigger="onSubmit">
        <van-field
          v-model="from.Name"
          name="Name"
          label="名称"
          :rules="[{ required: true, message: '请填写名称' }]"
        />
        <input-select
          v-model="from.Server"
          name="Server"
          label="服务器"
          :rules="[{ required: true, message: '请选择服务器' }]"
          :inputType="1"
          :list="list"
          listKey="NickName"
          bindKey="ID"
        ></input-select>
        <input-select
          v-model="from.ServerType"
          name="ServerType"
          label="服务类型"
          :rules="[{ required: true, message: '请选择服务类型' }]"
          :inputType="1"
          :list="serverTypeList"
          listKey="Name"
        ></input-select>
        <van-field
          v-model="from.LaunchCommand"
          name="LaunchCommand"
          label="执行命令"
          type="textarea"
        />
      </van-form>
    </van-dialog>
    <!-- 修改 -->
    <van-dialog
      v-model="changeShow"
      title="修改"
      :before-close="changeClose"
      show-cancel-button
    >
      <van-cell-group>
        <van-cell
          title="名称"
          title-style="max-width: 5em;margin-right: 12px;color: #969799;"
          value-class="change-service-label"
        >
          <template>
            <p
              style="padding: 0; margin: 0"
              v-for="(item, index) in chooseListNewName"
              :key="'change-service-' + index"
            >
              {{ item }}
            </p>
          </template>
        </van-cell>
      </van-cell-group>
      <van-form
        @submit="onSubmitChange"
        ref="changeFrom"
        validate-trigger="onSubmit"
        label-width="5em"
      >
        <van-field
          v-model="changeFrom.LaunchCommand"
          name="LaunchCommand"
          label="执行命令"
          type="textarea"
          placeholder="输入命令"
          :rules="[{ required: true, message: '请输入命令' }]"
        />
      </van-form>
    </van-dialog>
    <!-- 命令 -->
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
          placeholder="输入命令"
          :rules="[{ required: true, message: '请输入命令' }]"
        />
      </van-form>
    </van-dialog>
    <FloatBtn
      :list="['更新', '停止', '启动', '命令', '添加', '修改', '删除']"
      @choose="chooseFloatBtn"
    ></FloatBtn>
    <van-action-sheet
      v-model="actionsShow"
      :actions="actionsList"
      @select="onSelect"
    />
    <Nav></Nav>
  </div>
</template>

<script>
import Nav from "@/components/Nav";
import FloatBtn from "@/components/FloatBtn";
import Table from "@/components/Table";
import InputSelect from "@/components/InputSelect";
import storage from "../store/storage";

export default {
  name: "Services",
  components: {
    Nav,
    FloatBtn,
    Table,
    InputSelect,
  },
  data() {
    return {
      list: [],
      services: [],
      isLoading: false,
      dialog: {
        title: "添加",
        show: false,
        type: "add",
      },
      from: {
        Name: "",
        Server: "",
        ServerType: "",
        LaunchCommand: "",
      },
      serverTypeList: [],
      chooseList: [],
      chooseListNewName: [],
      commandShow: false,
      commandFrom: {
        LaunchCommand: "",
      },
      changeShow: false,
      changeFrom: {
        LaunchCommand: "",
      },
      actionsShow: false,
      actionsList: [{ name: "全部" }],
      chooseType: "",
    };
  },
  computed: {
    filterServices() {
      let list = this.services.filter((item) => {
        return item.Type == this.chooseType || this.chooseType == "全部";
      });
      return list;
    },
  },
  methods: {
    getServers(refresh) {
      this.$IMBSProxy.GetServers((res) => {
        console.log("list", res);
        if (refresh) {
          this.isLoading = false;
          this.services = [];
          this.actionsList = [];
        }

        this.list = res;
        for (const item of res) {
          for (const key in item.Services) {
            let i = item.Services[key];
            item.Services[key].newName = `${i.Name} (${item.NickName})`;

            item.Services[key].info = `${i.Type} <span style="color:${
              i.Revision == i.RevisionOnServer ? "green" : "red"
            }">(${i.Revision} / ${i.RevisionOnServer})</span>`;

            const s = {
              0: {
                name: "停止",
                color: "red",
              },
              1: {
                name: "启动中",
                color: "orange",
              },
              2: {
                name: "运行",
                color: "green",
              },
            };
            item.Services[key].state = `<span style="color:${
              s[i.Status].color
            }">${s[i.Status].name}</span>`;
          }
          this.services = this.services.concat(item.Services);
        }
      });
    },
    chooseFloatBtn(index) {
      console.log("chooseFloatBtn", index);
      switch (index) {
        // 更新
        case 0:
          this.$refs.table
            .verifyChoose()
            .then(() => {
              this.$IMBSProxy.UpdateService(this.chooseList, (res) => {
                if (res) {
                  this.$Toast("更新成功");
                  // this.$refs.table.clearChoose();
                }
              });
            })
            .catch(() => {});
          break;

        // 停止
        case 1:
          this.$refs.table
            .verifyChoose()
            .then(() => {
              this.$Dialog
                .confirm({
                  message: "是否停止所选服务",
                })
                .then(() => {
                  this.$IMBSProxy.StopService(this.chooseList, (res) => {
                    if (res) {
                      this.$Toast("停止成功");
                      // this.$refs.table.clearChoose();
                    }
                  });
                })
                .catch(() => {});
            })
            .catch(() => {});
          break;

        // 启动
        case 2:
          this.$refs.table
            .verifyChoose()
            .then(() => {
              this.$IMBSProxy.LaunchService(this.chooseList, (res) => {
                if (res) {
                  this.$Toast("启动成功");
                  // this.$refs.table.clearChoose();
                }
              });
            })
            .catch(() => {});
          break;

        // 命令
        case 3:
          this.$refs.table
            .verifyChoose()
            .then(() => {
              this.commandFrom = JSON.parse(
                JSON.stringify(this.$options.data().commandFrom)
              );
              this.commandShow = true;
            })
            .catch(() => {});
          break;

        // 添加
        case 4:
          this.from = JSON.parse(JSON.stringify(this.$options.data().from));
          this.dialog.show = true;
          break;

        // 修改
        case 5:
          this.$refs.table
            .verifyChoose()
            .then(() => {
              this.changeFrom = JSON.parse(
                JSON.stringify(this.$options.data().changeFrom)
              );
              this.changeShow = true;
            })
            .catch(() => {});
          break;

        // 删除
        case 6:
          this.$refs.table
            .verifyChoose()
            .then(() => {
              this.$Dialog
                .confirm({
                  message: "是否确定删除",
                })
                .then(() => {
                  this.$IMBSProxy.DeleteService(this.chooseList, (res) => {
                    if (res) {
                      this.$Toast("删除成功");
                      // this.$refs.table.clearChoose();
                      this.getServers(true);
                    }
                  });
                })
                .catch(() => {});
            })
            .catch(() => {});
          break;

        default:
          break;
      }
    },
    // 添加 ---------------------------------------
    onSubmit() {
      console.log("submit", this.from);
      this.$IMBSProxy.NewService(
        this.from.Server,
        this.from.ServerType,
        this.from.Name,
        this.from.LaunchCommand,
        (res) => {
          if (res) {
            this.$Toast(this.dialog.title + "成功");
            this.dialog.show = false;
            this.getServers(true);
            // this.$refs.table.clearChoose();
          }
        }
      );
    },
    dialogClose(action, done) {
      if (action == "confirm") {
        this.$refs.from.submit();
        done(false);
      } else {
        done();
      }
    },
    // 修改 --------------------------------
    onSubmitChange() {
      console.log("onSubmitChange", this.changeFrom);
      this.$IMBSProxy.SetServiceLaunchCommand(
        this.chooseList,
        this.changeFrom.LaunchCommand,
        (res) => {
          if (res) {
            this.$Toast("操作成功");
            this.changeShow = false;
            this.getServers(true);
            // this.$refs.table.clearChoose();
          }
        }
      );
    },
    changeClose(action, done) {
      if (action == "confirm") {
        this.$refs.changeFrom.submit();
        done(false);
      } else {
        done();
      }
    },
    // 命令 -----------------------------------
    onSubmitCommand() {
      console.log("onSubmitCommand", this.commandFrom);
      this.$IMBSProxy.CallCommand(
        this.chooseList,
        this.commandFrom.LaunchCommand,
        (res) => {
          if (res) {
            this.$Toast("操作成功");
            this.commandShow = false;
            this.getServers(true);
            // this.$refs.table.clearChoose();
          }
        }
      );
    },
    commandClose(action, done) {
      if (action == "confirm") {
        this.$refs.commandFrom.submit();
        done(false);
      } else {
        done();
      }
    },
    // 获取服务器类型
    getServiceType() {
      this.$IMBSProxy.GetServiceType((res) => {
        this.serverTypeList = res;
        for (const item of res) {
          this.actionsList.push({ name: item.Name });
        }
        this.setChooseTypeColor();
      });
    },
    // 选择列表
    chooseClick(val) {
      console.log("chooseClick", val);
      this.chooseList = [];
      this.chooseListNewName = [];
      for (const item of val) {
        this.chooseList.push(this.filterServices[item].Name);
        this.chooseListNewName.push(this.filterServices[item].newName);
      }
    },
    // 筛选
    clickTile(val) {
      console.log("clickTile", val);
      if (val == 1) this.actionsShow = true;
    },
    onSelect(item) {
      console.log("onSelect", item);
      storage.serviceTypeSet(item.name);
      this.chooseType = item.name;
      this.setChooseTypeColor();
      this.actionsShow = false;
    },
    setChooseTypeColor(name = this.chooseType) {
      console.log("setChooseTypeColor--->", name);
      for (const key in this.actionsList) {
        if (this.actionsList[key].name == name) {
          this.actionsList[key].color = "#1989fa";
        } else {
          this.actionsList[key].color = undefined;
        }
      }
    },
    clickRow(row) {
      console.log("clickRow---->", row.Name);
      // this.$router.push(`/serviceLog/${}`);
      this.$router.push({ path: "/serviceLog", query: { name: row.Name } });
    },
  },
  mounted() {
    this.getServers();
    this.getServiceType();
    this.chooseType = storage.serviceTypeGet()
      ? storage.serviceTypeGet()
      : "全部";
  },
};
</script>

<style lang="scss" scoped>
/deep/ .van-pull-refresh {
  height: 100%;
}
.change-service-label {
  text-align: left;
}
</style>
