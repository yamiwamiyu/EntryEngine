<template>
  <div class="container">
    <van-nav-bar
      title="日志"
      left-arrow
      @click-left="$router.go(-1)"
      style="width: 100%"
    >
      <template #right>
        <div style="display: flex;flex-direction: row;align-items: center;">
          <span
            @click="commandShow=true"
            style="padding:0 10px"
          >命令</span>
          <van-icon
            name="filter-o"
            size="18"
            @click="filter = true"
          />
        </div>
      </template>
    </van-nav-bar>
    <van-cell-group class="list">
      <van-cell
        v-for="(item, index) in list"
        :key="'list-item-' + index"
        :title="item.Record.Time"
        :value="item.Count"
        :style="
          'white-space: pre-line;' +
          (item.Record.ContentMore ? 'cursor: pointer;' : '') +
          'color:' +
          ['#ccc', '#000', 'orange', 'red'][item.Record.Level]
        "
        @click="detail(item)"
      >
        <template #label>
          <div v-text="item.Record.ContentShow" />
        </template>
      </van-cell>
    </van-cell-group>
    <!-- 分页 -->
    <div class="page-block">
      <div
        class="page-btn"
        @click="page == 1 ? null : (page = 1)"
        :class="{ 'page-btn-disabled': page == 1 }"
      >
        <van-icon
          name="arrow-left"
          class="icon-1"
        />
        <van-icon
          name="arrow-left"
          class="icon-2"
        />
      </div>
      <van-pagination
        v-model="page"
        :page-count="pageCount"
        :show-page-size="7"
      >
        <template #prev-text>
          <van-icon name="arrow-left" />
        </template>
        <template #next-text>
          <van-icon name="arrow" />
        </template>
        <template #page="{ text }">{{ text }}</template>
      </van-pagination>
      <div
        class="page-btn"
        @click="page == pageCount ? null : (page = pageCount)"
        :class="{ 'page-btn-disabled': page == pageCount }"
      >
        <van-icon
          name="arrow"
          class="icon-1"
          style="right: 50%"
        />
        <van-icon
          name="arrow"
          class="icon-2"
        />
      </div>
    </div>

    <!-- 筛选菜单 -->
    <van-popup
      v-model="filter"
      position="right"
      style="height: 100%; width: 80%"
    >
      <van-form
        @submit="onSubmit"
        ref="from"
        validate-trigger="onSubmit"
        label-width="56"
      >
        <van-field
          v-model="from.startTime"
          @click="
            showTime = true;
            sheetTimeType = 'start';
          "
          name="startTime"
          label="开始时间"
        >
        </van-field>
        <van-field
          v-model="from.endTime"
          @click="
            showTime = true;
            sheetTimeType = 'end';
          "
          name="endTime"
          label="结束时间"
        />
        <van-field
          v-model="from.content"
          name="content"
          label="内容"
        />
        <van-field
          v-model="from.param"
          name="param"
          label="参数"
        />
        <van-field
          name="type"
          label=""
          label-width="0"
        >
          <template #input>
            <van-checkbox-group
              v-model="from.type"
              direction="horizontal"
            >
              <van-checkbox
                class="type-check"
                name="0"
              >调试</van-checkbox>
              <van-checkbox
                class="type-check"
                name="1"
              >信息</van-checkbox>
              <van-checkbox
                class="type-check"
                name="2"
              >警告</van-checkbox>
              <van-checkbox
                class="type-check"
                name="3"
              >错误</van-checkbox>
            </van-checkbox-group>
          </template>
        </van-field>
        <van-row
          gutter="20"
          style="margin: 0 16px"
        >
          <van-col span="12">
            <van-button
              style="margin-top: 30px; flex: 1"
              round
              block
              type="info"
              native-type="submit"
              @click="submitType = 0"
            >
              普通查询
            </van-button>
          </van-col>
          <van-col span="12">
            <van-button
              style="margin-top: 30px; flex: 1"
              round
              block
              type="info"
              native-type="submit"
              @click="submitType = 1"
            >
              分组查询
            </van-button>
          </van-col>
        </van-row>
      </van-form>
    </van-popup>
    <!-- 时间选择 -->
    <van-action-sheet v-model="showTime">
      <van-datetime-picker
        v-model="sheetTime"
        type="datetime"
        @confirm="sureTime"
      />
    </van-action-sheet>
    <!-- 日志详情 -->
    <van-overlay
      :show="overlay"
      @click="overlay = false"
    >
      <div
        class="wrapper"
        @click.stop
      >
        <van-nav-bar
          left-arrow
          @click-left="overlay = false"
          :title="selectRow.Record.Time"
          right-text="关闭"
          @click-right="overlay = false"
          style="width: 100%"
        >
          <template #right>
            <van-icon
              name="cross"
              size="18"
            />
          </template>
        </van-nav-bar>
        <div
          class="block"
          ref="detailBlock"
          id="detailBlock"
        >
          <p id="detail">{{ selectRow.Record.Content }}</p>
        </div>
        <FloatBtn
          :goTop="true"
          :goBottom="true"
          @choose="chooseFloatBtn"
        ></FloatBtn>
      </div>
    </van-overlay>
    <!-- 命令 -->
    <Command
      :show="commandShow"
      :chooseList="[serviceName]"
      @close="commandShow=false"
      @submit="getLog()"
    ></Command>
  </div>
</template>

<script>
import FloatBtn from "@/components/FloatBtn";
import Command from "@/components/Command";
export default {
  name: "ServiceLog",
  components: {
    FloatBtn,
    Command,
  },
  data() {
    return {
      filter: false,
      from: {},
      showTime: false,
      sheetTime: new Date(),
      sheetTimeType: "start" | "end",
      page: 1,
      pageCount: 0,
      list: [],
      // 0：普通查询，1：分组查询
      submitType: 0,
      overlay: false,
      selectRow: { Record: {} },
      commandShow: false,
    };
  },
  computed: {
    serviceName() {
      let name = this.$route.query.name;
      return name;
    },
  },
  watch: {
    page: function (val) {
      console.log("page-->", val);
      this.getLog();
    },
  },
  methods: {
    getLog() {
      let typeList = [];
      for (let item of this.from.type) {
        typeList.push(Number(item));
      }
      console.log("getLog-->", this.submitType);
      const response = (res) => {
        this.list = res.Models;
        for (const key in this.list) {
          let params = this.list[key].Record.Params;
          for (const k in params) {
            let regexp = "{" + k + "}";
            this.list[key].Record.Content = `${this.list[
              key
            ].Record.Content.replace(regexp, params[k])}`;
          }

          if (this.list[key].Record.Content.length > 200) {
            this.list[key].Record.ContentShow =
              this.list[key].Record.Content.slice(0, 200) + " ...";
            this.list[key].Record.ContentMore = true;
          } else {
            this.list[key].Record.ContentShow = this.list[key].Record.Content;
            this.list[key].Record.ContentMore = false;
          }
        }
        console.log("response---->");
        this.pageCount = (((res.Count - 1) / res.PageSize) | 0) + 1;
      };

      if (this.submitType == 0) {
        this.$IMBSProxy.GetLog(
          this.serviceName,
          this.from.startTime ? new Date(this.from.startTime).getTime() : 0,
          this.from.endTime ? new Date(this.from.endTime).getTime() : 0,
          30,
          this.page - 1,
          this.from.content,
          this.from.param,
          typeList,
          response
        );
      } else {
        this.$IMBSProxy.GroupLog(
          this.serviceName,
          this.from.startTime ? new Date(this.from.startTime).getTime() : 0,
          this.from.endTime ? new Date(this.from.endTime).getTime() : 0,
          this.from.content,
          this.from.param,
          typeList,
          response
        );
      }
    },
    onSubmit() {
      console.log("submit", this.from);
      this.$store.commit("setLogSearch", this.from);
      this.getLog();
      this.filter = false;
    },
    sureTime(val) {
      console.log("sureTime", this.sheetTimeType, val);
      if (this.sheetTimeType == "start") {
        this.from.startTime = val.format("yyyy-MM-dd hh:mm");
      } else {
        this.from.endTime = val.format("yyyy-MM-dd hh:mm");
      }
      this.showTime = false;
      this.sheetTime = new Date();
    },
    detail(item) {
      console.log("detail--->", item);
      this.selectRow = item;
      if (item.Record.ContentMore) this.overlay = true;
    },
    chooseFloatBtn(index) {
      console.log("chooseFloatBtn", index);
      switch (index) {
        case "goTop":
          document.getElementById("detailBlock").scrollTop = 0;
          break;

        case "goBottom":
          document.getElementById(
            "detailBlock"
          ).scrollTop = document.getElementById("detail").clientHeight;
          break;

        default:
          break;
      }
    },
  },
  mounted() {
    console.log("logSearch-->", this.$store.state.logSearch);
    this.from = this.$store.state.logSearch;
    this.getLog();
    this.$nextTick().then(() => {
      this.$refs.detailBlock.addEventListener(
        "touchmove",
        (e) => e.stopPropagation(),
        false
      );
    });
  },
};
</script>

<style lang="scss" scoped>
.container {
  display: flex;
  height: 100%;
  align-items: center;
  flex-direction: column;

  .list {
    flex: 1;
    overflow: auto;
    width: 100%;

    /deep/.van-cell__label {
      color: inherit;
    }
    /deep/.van-cell__value {
      color: inherit;
      flex: none;
    }
  }

  .page-block {
    width: 100%;
    display: flex;
    align-items: center;

    .van-pagination {
      flex: 1;
    }

    // /deep/.van-pagination__prev,
    // /deep/.van-pagination__next {
    //   max-width: 50px;
    // }

    .page-btn {
      width: 50px;
      height: 40px;
      line-height: 40px;
      text-align: center;
      font-size: 14px;
      color: #1989fa;
      background-color: #fff;
      cursor: not-allowed;
      position: relative;

      .icon-1 {
        position: absolute;
        top: 50%;
        left: 50%;
        margin-top: -7px;
        margin-left: -9px;
      }
      .icon-2 {
        position: absolute;
        top: 50%;
        left: 20px;
        margin-top: -7px;
        margin-left: -2px;
      }

      &.page-btn-disabled {
        color: #646566;
        background-color: #f7f8fa;
        opacity: 0.5;
      }
    }
  }

  .type-check {
    margin: 0 10px 5px 0;
  }

  .wrapper {
    display: flex;
    flex-direction: column;
    height: 100%;
    background-color: #fff;
    .block {
      padding: 15px;
      flex: 1;
      height: 0;
      overflow-y: auto;
      white-space: pre-line;
      word-break: break-word;
    }
  }

  .ellipsis {
    overflow: hidden;
    text-overflow: ellipsis;
    display: -webkit-box;
    -webkit-line-clamp: 3;
    -webkit-box-orient: vertical;
  }
}
</style>
