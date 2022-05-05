<template>
  <el-container class="container">
    <el-header>
      <el-row>
        <titleUI :keywords="form" @onQuery="onQuery()" @res="res()"></titleUI>
      </el-row>
    </el-header>
    <el-main class="main">
      <el-row>
        <el-breadcrumb separator="/" v-if="navList.length > 0">
          <el-breadcrumb-item class="all-game" @click="gameIDunll"
            >全部</el-breadcrumb-item
          >
          <el-breadcrumb-item v-for="(item, index) in navList" :key="'game' + index">{{
            item
          }}</el-breadcrumb-item>
        </el-breadcrumb>
      </el-row>
      <el-row class="row-bg-title" justify="space-between">
        <el-col :span="6" class="el-col-L">
          <b>数据列表</b>
        </el-col>
      </el-row>

      <el-table
        v-if="!gameIsClick"
        :data="tableData"
        border
        style="width: 100%"
        class="_el-thead-radio"
      >
        <el-table-column prop="GameName" label="游戏名称" width="180">
          <template #default="scope">
            <el-button @click="tabList(scope.row.GameName)" type="text" size="small">
              {{ scope.row.GameName }}
            </el-button>
          </template>
        </el-table-column>
        <el-table-column prop="ActiveCount" label="活跃玩家数" width="180">
        </el-table-column>
        <el-table-column prop="RegistCount" label="新增玩家数" width="180">
        </el-table-column>
        <el-table-column 
          prop="RetainedRate"
          v-for="(item, key, index) in RetainedRate"
          :key="'RetainedRate' + index"
          :label="item"
          >
          <template #default="scope">
            {{scope.row.RetainedRate[key]==-1?"-": scope.row.RetainedRate[key] +"%"}}
          </template>
        </el-table-column>
      </el-table>
      <el-table
        v-if="gameIsClick"
        :data="tableData"
        border
        style="width: 100%"
        class="_el-thead-radio"
      >
        <el-table-column prop="Time" label="日期" width="180"> </el-table-column>
        <el-table-column prop="ActiveCount" label="活跃玩家数" width="180">
        </el-table-column>
        <el-table-column prop="RegistCount" label="新增玩家数" width="180">
        </el-table-column>
        
        <el-table-column 
          prop="RetainedRate"
          v-for="(item, key, index) in RetainedRate"
          :key="'RetainedRate' + index"
          :label="item"
          >
          <template #default="scope">
            {{scope.row.RetainedRate[key]==-1?"-":  scope.row.RetainedRate[key] +"%"}}
          </template>
        </el-table-column>
      </el-table>
    </el-main>
    <el-footer class="_el-pagination" style="height: auto">
      <div class="block">
        <el-pagination
          @current-change="clickPages"
          :current-page="page.index + 1"
          :page-size="page.size"
          layout="total,  prev, pager, next, jumper"
          :total="page.total"
          :close-on-click-modal="false"
        ></el-pagination>
      </div>
    </el-footer>
  </el-container>
</template>

<script>
import titleUI from "@/components/titleUI.vue";

export default {
  name: "GameManager",
  components: {
    titleUI,
  },
  data() {
    return {
      form: {
        region: "",
        channel: "",
        date: [new Date(new Date().toLocaleDateString()).getTime(), Date.now()],
      },
      tableData: [],
      page: {
        index: 0,
        total: 0,
        size: 15,
      },
      navList: [],
      // 当前游戏名字
      gameList: [],
      // 游戏详情切换
      gameIsClick: false,
      // 点击查看的游戏
      gameName: '',
      RetainedRate: ["次日留存","2日留存","3日留存","4日留存","5日留存","6日留存","7日留存","14日留存","30日留存"]
    };
  },
  watch:{
  },
  mounted() {
    // this.onQuery();
  },
  methods: {
    initData() {
      this.$IP5.GetRetained(
        this.page.index,
        this.form.region,
        this.form.channel,
        this.form.date && this.form.date !=null ? this.$Tool.GMT(this.form.date[0]) : "",
        this.form.date && this.form.date !=null ? this.$Tool.GMT(this.form.date[1]) : "",
        (res) => {
          this.page.total = res.Count;
          this.page.size = res.PageSize;
          this.page.index = res.Page;
          this.tableData = res.Models;
          console.log(this.tableData);
        }
      );
    },
    initData2(){
      this.$IP5.GetRetained2(
        this.page.index,
        this.gameName,
        this.form.channel,
        this.form.date && this.form.date !=null ? this.$Tool.GMT(this.form.date[0]) : "",
        this.form.date && this.form.date !=null ? this.$Tool.GMT(this.form.date[1]) : "",
        (res) => {
          this.page.total = res.Count;
          this.page.size = res.PageSize;
          this.page.index = res.Page;
          this.tableData = res.Models;
          this.tableData.forEach(item=>{
            item.Time = item.Time.split(" ")[0];
          })
        }
      );
    },
    onQuery() {
      
      if(this.form.region == "全部"){
        this.gameIDunll();
        return;
      }
      if(this.navList.length>0){
        this.tabList(this.form.region);
        return;
      }
      this.initData();
    },
    res() {
      Object.assign(this.form, this.$options.data().form);
      Object.assign(this.page, this.$options.data().page);
      this.navList = [];
      this.gameIsClick = false;
      this.tableData = [];
    },
    clickPages(pageIndex) {
      this.page.index = pageIndex - 1;
      if(this.navList.length>0){
        this.initData2();
      } else {
        this.initData();
      }
    },
    // 点击游戏到二级游戏统计表
    tabList(gameName) {
      this.navList = [];
      this.gameIsClick = true;
      this.gameName = gameName;
      Object.assign(this.page, this.$options.data().page);
      this.initData2();
      this.navList.push(gameName);
    },
    // 回到全部游戏的数据
    gameIDunll() {
      this.gameIsClick = false;
      this.navList = [];
      Object.assign(this.page, this.$options.data().page);
      this.initData();
    },
  },
};
</script>

<style scoped lang="scss">
.container {
  display: flex;
  .el-header {
    width: 100%;
    display: flex;
    height: auto;
    line-height: 0;
    .el-form-item {
      float: left;
      margin: 0 15px 10px 4px;
      .el-form-item__content * {
        font-size: 12px;
      }
    }
  }
  .el-main {
    .row-bg-title {
      margin-bottom: 20px;
      .el-col-L {
        text-align: left;
      }
    }
  }
  .all-game {
    font-weight: bold;
    font-size: 14px;
    cursor: pointer;
    /deep/ .el-breadcrumb__inner {
      color: #409eff;
    }
  }
  .row-bg-title {
    margin: 10px 0;
  }
}
</style>

