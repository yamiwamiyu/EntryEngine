<template>
  <el-container class="container">
    <el-header>
      <el-row>
        <titleUI :keywords="form" @onQuery="onQuery()" @res="res()">
          <el-form-item label="类型：" label-width="80px">
              <el-input v-model="form.verificationType"></el-input>
          </el-form-item>
          <el-form-item label="ID：" label-width="80px">
              <el-input v-model="form.verificationID"></el-input>
          </el-form-item>
          <el-form-item label="获奖码：" label-width="120px">
              <el-input v-model="form.winningCode"></el-input>
          </el-form-item>
        </titleUI>
      </el-row>
    </el-header>
    <el-main class="main">
      <el-row>
        <el-breadcrumb separator="/" v-if="navList.length > 0">
          <el-breadcrumb-item class="all-game" @click="gameIDunll"
            >全部</el-breadcrumb-item
          >
          <el-breadcrumb-item
            v-for="(item, index) in navList"
            :key="'game' + index"
            >{{ item }}</el-breadcrumb-item
          >
        </el-breadcrumb>
      </el-row>
      <el-row class="row-bg-title" justify="space-between">
        <el-col :span="6" class="el-col-L">
          <b>数据列表</b>
        </el-col>

        <el-col :span="12" class="el-col-R">
          <el-button
            :disabled="aData != null && aData.Type == 1"
            type="primary"
            @click="verification()"
            >核銷</el-button
          >
        </el-col>
      </el-row>

      <el-table
        :data="tableData"
        border
        style="width: 100%"
        class="_el-thead-radio"
        highlight-current-row
        @current-change="handleCurrentChange"
      >
        <el-table-column prop="Code" label="获奖码">
          <template #default="scope">
            <el-button
              @click="tabList(scope.row.GameName)"
              type="text"
              size="small"
            >
              {{ scope.row.GameName }}
            </el-button>
          </template>
        </el-table-column>
        <el-table-column prop="GameName" label="游戏名称">
        </el-table-column>
        <el-table-column prop="Type" label="类型">
        </el-table-column>
        <el-table-column prop="ID" label="ID">
        </el-table-column>
        <el-table-column prop="RewardName" label="道具名称">
        </el-table-column>
        <el-table-column prop="DeviceID" label="设备号">
        </el-table-column>
        <el-table-column prop="Operator" label="操作人">
        </el-table-column>
        <el-table-column prop="Time" label="核销时间">
        </el-table-column>
        <el-table-column prop="IsVerify" label="是否核销">
          <template #default="scope">
            {{scope.row.IsVerify ? "已核销":"未核销"}}
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
  <!-- 核销 -->
  <el-dialog title="核销" v-model="isVerification" width="30%" center>
    <el-form
      :model="verificationForm"
      :rules="verificationRuleForm"
      ref="verificationForm"
      label-width="100px"
      class="demo-BindRuleForm"
    >
      <el-form-item label="获奖码：" prop="verification">
        <el-input v-model="verificationForm.verification" @blur="verifyGameTaskInfo()"></el-input>
      </el-form-item>
    </el-form>
    <div class="hexiao" v-if="verifyGameTaskInfoList!=''">
        <p :class="verificationClass ? 'verificationOk':'verificationNo'">未核销</p>
        <p>游戏名称：{{verifyGameTaskInfoList.GameName}}</p>
        <p>类型：{{verifyGameTaskInfoList.Type}}</p>
        <p>ID：{{verifyGameTaskInfoList.ID}}</p>
        <p>道具名称：68元礼包{{verifyGameTaskInfoList.RewardName}}</p>
        <p>设备号：{{verifyGameTaskInfoList.DeviceID}}</p>
        <p>完成时间：{{verifyGameTaskInfoList.CompletionTime}}</p>
    </div>
    <template #footer>
      <span class="dialog-footer">
        <el-button v-if="!verificationClass" type="primary" @click="verificationSubmit()">确 定</el-button>
        <el-button v-if="!verificationClass" @click="isVerification = false">取 消</el-button>
        <el-button v-if="verificationClass" type="primary" @click="isVerification = false">关闭</el-button>
      </span>
    </template>
  </el-dialog>
</template>

<script>
import titleUI from "@/components/titleUI.vue";
import { ElMessage } from 'element-plus'

export default {
  name: "GameTask",
  components: {
    titleUI,
  },
  data() {
    return {
      form: {
        region: "全部",        
        verificationType: "全部",
        verificationID: "",
        winningCode: "",        
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
      // 点击查看的游戏
      gameName: "",
      isVerification:false,
      verificationRuleForm:{
        verification: [
          { required: true, message: '！无效的获奖码', trigger: 'blur' },
        ],
      },
      verificationForm:{
          verification:""
      },
      verificationClass: false,
      // 焦点数据
      verifyGameTaskInfoList: "",
    };
  },
  watch: {},
  mounted() {
      this.initData();
  },
  methods: {
    initData() {
      this.$IP5.GetGameTaskRecordList(
        this.page.index,
        this.form.region,        
        this.form.verificationType,
        this.form.verificationID,
        this.form.winningCode,
        this.form.date && this.form.date !=null ? this.$Tool.GMT(this.form.date[0]) : "",
        this.form.date && this.form.date !=null ? this.$Tool.GMT(this.form.date[1]) : "",
        (res) => {
          this.page.total = res.Count;
          this.page.size = res.PageSize;
          this.page.index = res.Page;
          this.tableData = res.Models;
        }
      );
    },
    onQuery() {
      Object.assign(this.page, this.$options.data().page);
      this.initData();
    },
    res() {
      Object.assign(this.form, this.$options.data().form);
      Object.assign(this.page, this.$options.data().page);
      this.tableData = [];
    },
    clickPages(pageIndex) {
      this.page.index = pageIndex - 1;
      this.initData();
    },
    verification(){
        this.isVerification = true;
    },
    verificationSubmit(){
      this.$IP5.VerifyGameTask(
        this.verificationForm.verification,
        (res) => {
          console.log(res);
          this.verificationClass = true;
        }
      );
    },
    // 失去焦点查询
    verifyGameTaskInfo(){
      console.log(this.verificationForm.verification);
      this.$IP5.VerifyGameTaskInfo(
        this.verificationForm.verification,
        (res) => {
          console.log(res);
          this.verificationClass = true;
        }
      );
    }
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
  .el-col-R {
    text-align: right;
  }
}
.hexiao {
    padding-left: 100px;
    p{
        color:#AAAAAA;
        font-size: 14px;
    }
    .verificationNo {
        color: #70B603;
    }
    .verificationOk {
        color: #D9001B;
    }
}
</style>

