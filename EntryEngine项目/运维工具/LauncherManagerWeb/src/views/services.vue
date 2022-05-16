<template>
  <div class="container">
    <van-pull-refresh
      v-model="isLoading"
      @refresh="getServers(true)"
    >
      <Table
        ref="table"
        :list="filterServices"
        :option="{ 名称: 'newName', 服务版本: 'info', 状态: 'state' }"
        :theadStyle="{ 
          0: 'cursor: pointer',
          1: 'cursor: pointer',
          2: 'cursor: pointer',
        }"
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
    <van-dialog width="15rem"
      v-model:show="dialog.show"
      :title="dialog.title"
      :before-close="dialogClose"
      show-cancel-button
    >
      <van-form label-width="4rem"
        ref="from"
      >
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
          @onselect="chooseServiceType"
        ></input-select>
        <van-field
          v-model="from.Exe"
          name="Exe"
          label="启动程序"
        />
        <van-field
          v-model="from.LaunchCommand"
          name="LaunchCommand"
          label="启动命令"
          type="textarea"
          rows="10"
        />
      </van-form>
    </van-dialog>
    <!-- 修改 -->
    <van-dialog width="15rem"
      v-model:show="changeShow"
      title="修改"
      :before-close="changeClose"
      show-cancel-button
    >
      <van-form label-width="4rem"
                ref="changeFrom">
        <van-field v-model="changeFrom.Exe"
                   name="Exe"
                   label="启动程序"
                   placeholder="输入.exe程序"/>
        <van-field v-model="changeFrom.LaunchCommand"
                   name="LaunchCommand"
                   label="启动命令"
                   type="textarea"
                   rows="10"
                   placeholder="输入命令"
                   :rules="[{ required: true, message: '请输入命令' }]" />
      </van-form>
    </van-dialog>
    <!-- 命令 -->
    <Command
      :show="commandShow"
      :chooseList="chooseList"
      @close="commandShow=false"
      @submit="getServers(true)"
    ></Command>
    <FloatBtn
      showNum="5"
      :list="['刷新', '更新', '停止', '启动', '命令', '添加', '修改', '删除']"
      @choose="chooseFloatBtn"
    ></FloatBtn>
    <van-action-sheet
      v-model:show="actionsShow"
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
import Command from "@/components/Command";
import InputSelect from "@/components/InputSelect";
import storage from "../store/storage";

export default {
  components: {
    Nav,
    FloatBtn,
    Table,
    InputSelect,
    Command,
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
        Exe: "",
        LaunchCommand: "",
      },
      serverTypeList: [],
      chooseList: [],
      chooseListNewName: [],
      chooseListIndex: [],
      commandShow: false,
      changeShow: false,
      changeFrom: {
        Exe: "",
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

        for (const key in res) {
          res[key].NickName = res[key].NickName
            ? res[key].NickName
            : res[key].EndPoint;
        }

        this.list = res;
        for (const item of res) {
          for (const key in item.Services) {
            let i = item.Services[key];
            i.newName = `${i.Name} (${item.NickName})`;

            i.info = `${i.Type} <span style="color:${
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
            i.state = `<span style="color:${
              s[i.Status].color
              }">${s[i.Status].name}</span>`;

            let existsKey = -1;
            for (const exists in this.services) {
              if (this.services[exists].newName == i.newName) {
                existsKey = exists;
                break;
              }
            }
            if (existsKey != -1) {
              console.log("修改已有服务", existsKey)
              this.services[existsKey] = i;
            } else {
              this.services.push(i)
            }
          }
        }

        // 强制刷新页面
        console.log("this", this)
      });
    },
    chooseFloatBtn(index, item) {
      console.log("chooseFloatBtn", index, item);
      switch (index) {
        // 刷新
        case 0:
          this.$IMBSProxy.UpdateServer(res => this.getServers());
          break;

        // 更新
        case 1:
          this.$refs.table
            .verifyChoose()
            .then(() => {
              this.$IMBSProxy.UpdateService(this.chooseList, (res) => {
                if (res) {
                  this.$Toast("更新成功");
                  // this.$refs.table.clearChoose();
                  this.getServers(false);
                }
              });
            })
            .catch(() => {});
          break;

        // 停止
        case 2:
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
                      setTimeout(() => this.getServers(false), 500)
                      setTimeout(() => this.getServers(false), 1000)
                      setTimeout(() => this.getServers(false), 2000)
                      setTimeout(() => this.getServers(false), 3000)
                    }
                  });
                })
                .catch(() => {});
            })
            .catch(() => {});
          break;

        // 启动
        case 3:
          this.$refs.table
            .verifyChoose()
            .then(() => {
              this.$IMBSProxy.LaunchService(this.chooseList, (res) => {
                if (res) {
                  this.$Toast("启动成功");
                  // this.$refs.table.clearChoose();
                  setTimeout(() => this.getServers(false), 500)
                  setTimeout(() => this.getServers(false), 1000)
                  setTimeout(() => this.getServers(false), 2000)
                  setTimeout(() => this.getServers(false), 3000)
                }
              });
            })
            .catch(() => {});
          break;

        // 命令
        case 4:
          this.$refs.table
            .verifyChoose()
            .then(() => (this.commandShow = true))
            .catch(() => {});
          break;

        // 添加
        case 5:
          this.from = JSON.parse(JSON.stringify(this.$options.data().from));
          this.dialog.show = true;
          break;

        // 修改
        case 6:
          this.$refs.table
            .verifyChoose()
            .then(() => {
              this.changeFrom = JSON.parse(
                JSON.stringify(this.$options.data().changeFrom)
              );
              let list = [];
              for (const item of this.chooseListIndex) {
                list.push(this.filterServices[item]);
              }
              let resp = list.every((item) => {
                return item.LaunchCommand == list[0].LaunchCommand;
              });
              if (resp) {
                this.changeFrom.LaunchCommand = list[0].LaunchCommand;
              }
              resp = list.every((item) => {
                return item.Exe == list[0].Exe;
              });
              if (resp) {
                this.changeFrom.Exe = list[0].Exe;
              }
              this.changeShow = true;
            })
            .catch(() => {});
          break;

        // 删除
        case 7:
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
      }
    },
    // 添加 ---------------------------------------
    chooseServiceType(value) {
      let current;
      for (const item of this.serverTypeList) {
        if (item.Name == value) {
          current = item;
          break;
        }
      }
      if (current) {
        this.from.Exe = current.Exe;
        this.from.LaunchCommand = current.LaunchCommand;
      }
    },
    dialogClose(action) {
      if (action == "confirm") {
        return new Promise((resolve) => {
          this.$IMBSProxy.NewService(
            this.from.Server,
            this.from.ServerType,
            this.from.Name,
            this.from.Exe,
            this.from.LaunchCommand,
            (res) => {
              this.$Toast(this.dialog.title + "成功");
              this.dialog.show = false;
              this.getServers(true);
              // this.$refs.table.clearChoose();
            }).then(() => resolve(true)).catch(() => resolve(false))
        })
      }
      return true;
    },
    // 修改 --------------------------------
    changeClose(action) {
      if (action == "confirm") {
        return this.$IMBSProxy.SetServiceLaunchCommand(
          this.chooseList,
          this.changeFrom.Exe,
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
      }
      return true;
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
      this.chooseListIndex = val;
      this.chooseList = [];
      this.chooseListNewName = [];
      for (const item of val) {
        this.chooseList.push(this.filterServices[item].Name);
        this.chooseListNewName.push(this.filterServices[item].newName);
      }
      console.log(this.chooseListNewName)
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
::deep .van-pull-refresh {
  height: 100%;
  overflow-y: auto;
}
.change-service-label {
  text-align: left;
}
</style>
