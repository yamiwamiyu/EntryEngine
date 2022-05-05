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
            type="primary"
            @click="Bind()"
            >配置客服</el-button
          >
          <el-button type="primary" @click="Add()">添加</el-button>
          <el-button type="primary" @click="Modify()">修改</el-button>
          <el-button type="primary" @click="Delete()">删除</el-button>
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
        <el-table-column prop="GameName" label="游戏名称"> </el-table-column>
        <el-table-column prop="Channel" label="渠道"> </el-table-column>
        <el-table-column prop="TaskType" label="任务类型"> </el-table-column>
        <el-table-column prop="Condition2" label="达成条件"> </el-table-column>
        <el-table-column prop="RecordCount" label="领取次数"> </el-table-column>
        <el-table-column prop="RewardImageAccessURL" label="道具图片">
          <template #default="scope">
            <el-image
              style="width: 50px; height: 50px"
              :src="scope.row.RewardImageAccessURL"
              fit="fit"
            ></el-image>
          </template>
        </el-table-column>
        <el-table-column
          :show-overflow-tooltip="true"
          prop="Description"
          label="描述"
        >
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

  <!-- 配置客服 -->
  <el-dialog title="配置客服" v-model="isBind" width="50%" center>
    <el-form
      :model="service"
      ref="service"
      label-width="100px"
      class="demo-BindRuleForm"
    >
      <el-form-item label="客服图片">
        <upload-file
          :init="service.imageUrlFull"
          @get-result="uploadFile"
          :key="service.imageUrlFull"
        ></upload-file>
      </el-form-item>
      <el-form-item label="说明：">
        <el-input
          v-model="service.textarea"
          :rows="3"
          type="textarea"
          placeholder="请输入"
        />
      </el-form-item>
    </el-form>
    <template #footer>
      <span class="dialog-footer">
        <el-button type="primary" @click="bind()">确 定</el-button>
        <el-button @click="isBind = false">取 消</el-button>
      </span>
    </template>
  </el-dialog>
  <!-- 添加 -->
  <el-dialog :title="updateType==0?'添加游戏任务':'修改游戏任务'" v-model="isAdd" width="30%" center>
    <el-form
      :model="addRuleForm"
      :rules="addRules"
      ref="addRuleForm"
      label-width="100px"
      class="demo-BindRuleForm"
    >
      <el-form-item  label="游戏名称：" prop="GameName">
        <el-input :disabled="updateType==1" v-model="addRuleForm.gameName" placeholder="请输入游戏名称" />
      </el-form-item>

      <el-form-item label="渠道" prop="channel">
        <el-checkbox :indeterminate="isIndeterminate" v-model="checkAll" @change="handleCheckAllChange">全选</el-checkbox>
        <div style="margin: 15px 0;"></div>
        <el-checkbox-group v-model="addRuleForm.channel" @change="handleCheckedCitiesChange">
          <el-checkbox v-for="city in cities" :label="city" :key="city">{{city}}</el-checkbox>
        </el-checkbox-group>
        
      </el-form-item>

      <el-form-item label="任务类型" prop="taskType">
        <el-input :disabled="updateType==1" v-model="addRuleForm.taskType" placeholder="请输入任务类型">
          <template #append
            >
            <el-tooltip
                class="item"
                effect="dark"
                placement="top"
            >
                <template #content> 
                须知：字段“任务类型”、“达成条件”需组合使用。<br />
                填写完成联系开发人员对接游戏，研发通过后才<br />
                可正式投入使用。<br />
                填写示例：<br />
                任务类型，可输入文字、数字，输入“关卡”；<br />
                达成条件，只能输入纯数字，输入“10”，下拉选择“等于“。<br />
                表示当通过关卡的第10关时，可领取奖励。<br />
                </template>
                <el-button>!</el-button>
            </el-tooltip>
          </template>
        </el-input>
      </el-form-item>

      <el-form-item label="达成条件" prop="condition2">
        <el-input :disabled="updateType==1" v-model="addRuleForm.condition2" placeholder="请输入达成条件" >
            <template #prepend>
                <el-select :disabled="updateType==1" v-model="addRuleForm.condition1" style="width: 110px">
                    <el-option label="≥" value=">="></el-option>
                    <el-option label="≤" value="<="></el-option>
                    <el-option label="＝" value="="></el-option>
                </el-select>
            </template>
        </el-input>
      </el-form-item>

      <el-form-item label="道具名称" prop="rewardName">
        <el-input v-model="addRuleForm.rewardName" placeholder="请输入道具名称" />
      </el-form-item>

      <el-form-item label="道具图片">
        <upload-file
          :init="addRuleForm.imageUrl"
          @get-result="addUploadFile"
          :key="addRuleForm.imageUrl"
        ></upload-file>
      </el-form-item>

      <el-form-item label="描述">
        <el-input
          v-model="addRuleForm.description"
          placeholder="请输入"
          :rows="3"
          type="textarea"
        />
      </el-form-item>
    </el-form>
    <template #footer>
      <span class="dialog-footer">
        <el-button type="primary" @click="submit()">确 定</el-button>
        <el-button @click="isAdd = false">取 消</el-button>
      </span>
    </template>
  </el-dialog>
  <!-- 修改 -->
  <el-dialog title="修改游戏任务" v-model="isModify" width="30%" center>
    <template #footer>
      <span class="dialog-footer">
        <el-button type="primary" @click="unpdatSubmit()">确 定</el-button>
        <el-button @click="isModify = false">取 消</el-button>
      </span>
    </template>
  </el-dialog>
  <!-- 删除 -->
  <el-dialog title="删除任务" v-model="isDelete" width="30%" center>
    <el-row>
      <el-col> 确定要删除任务吗？ </el-col>
    </el-row>
    <template #footer>
      <span class="dialog-footer">
        <el-button type="primary" @click="delSubmit()">确 定</el-button>
        <el-button @click="isDelete = false">取 消</el-button>
      </span>
    </template>
  </el-dialog>
</template>

<script>
import { ElMessage } from 'element-plus'
import UploadFile from "@/components/UploadFile";
import titleUI from "@/components/titleUI.vue";

export default {
  name: "GameTask",
  components: {
    titleUI,
    "upload-file": UploadFile,
  },
  data() {
    return {
      form: {
        region: "",
        regionType: 1,
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
      // 弹窗控制
      isBind: false,
      isAdd: false,
      isModify: false,
      isDelete: false,
      service: {
        textarea: "",
        imageUrl: "",
        imageUrlFull: "",
      },
      // 添加修改切换 0-添加  1-修改
      updateType: 0,
      // 添加任务的渠道
      addRuleForm: {
        ID: 0,
        gameName: '', 
        taskType: '', 
        condition1: '>=', 
        condition2: '', 
        rewardName: '', 
        rewardImage: '', 
        description: '',
        // 默认图片
        imageUrl: '',
        // 渠道选择
        channel: [],
      },
      checkAll: false,
      cities: [],
      isIndeterminate: true,
      addRules: {
        gameName: [
          { required: true, message: '请输入活动名称', trigger: 'blur' },
        ],
        channel: [
          { required: true, message: '请输入渠道', trigger: 'change' },
        ],
        taskType: [
          { required: true, message: '请输入任务类型', trigger: 'blur' },
        ],
        condition2: [
          { required: true, message: '请输入达成条件', trigger: 'blur' },
        ],
        rewardName: [
          { required: true, message: '请输入道具名称', trigger: 'blur' },
        ],
      },
    };
  },
  watch: {},
  mounted() {
    this.$IP5.GetAnalysisGame((res) => {
      this.form.region = res[0];
      this.initData();
    });
  },
  methods: {
    initData() {
      this.$IP5.GetGameTaskList(this.page.index, this.form.region, res=>{
        this.page.total = res.Count;
        this.page.size = res.PageSize;
        this.page.index = res.Page;
        let arr = [];
        res.Models.forEach(item=>{
          arr.push(item["#Server._Cache+C_GameTask, Server"])
        })
        this.tableData = arr;
        this.$IP5.GetChannel(this.form.region, res=>{
          var newArr = res.slice(1);
          this.cities = newArr;
        });
      });
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

    // 打开绑定窗口
    Bind() {
      Object.assign(this.service, this.$options.data().service);
      this.isBind = true;
      this.$IP5.GetCustomerService(
        this.form.region,
        (res)=>{
          console.log(res);
          this.service.imageUrlFull = res.QRCodeAccessURL;
          this.service.imageUrl = res.QRCode;
          this.service.textarea = res.Description;
      });
    },
    // 打开添加窗口
    Add() {
      Object.assign(this.addRuleForm, this.$options.data().addRuleForm);
      Object.assign(this.checkAll, this.$options.data().checkAll);
      this.isIndeterminate = true;
      this.isAdd = true;
      this.updateType = 0;
    },
    // 打开修改窗口
    Modify() {
      if(this.aData != null && this.aData != undefined){ 
        
        this.isAdd = true;      
        // this.addRuleForm = this.aData;
        this.updateType = 1;
        this.addRuleForm.ID =  this.aData.ID;
        this.addRuleForm.gameName = this.aData.GameName; 
        this.addRuleForm.taskType=this.aData.TaskType; 
        this.addRuleForm.condition1=this.aData.Condition1; 
        this.addRuleForm.condition2=this.aData.Condition2; 
        this.addRuleForm.rewardName=this.aData.RewardName; 
        this.addRuleForm.description=this.aData.Description;
        // 默认图片
        this.addRuleForm.imageUrl=this.aData.RewardImageAccessURL;
        
        this.addRuleForm.rewardImage = this.aData.RewardImage;
        // 渠道选择
        if (this.aData.Channel[0] == "全部"){
          this.checkAll = true;
          this.isIndeterminate = false;
          this.addRuleForm.channel = this.cities;
        } else {
          this.checkAll = false;
          this.isIndeterminate = true;
          this.addRuleForm.channel = this.aData.Channel;
        }
        console.log(this.addRuleForm.channel);
      } else {
        ElMessage.error('请选中一条数据后再操作!');
      }
    },
    // 打开删除窗口
    Delete() {
      this.isDelete = true;
    },
    // 当前选中的某一条数据
    handleCurrentChange(val) {
      this.aData = val;
    },
    // 绑定客服
    bind() {
      this.$IP5.SetCustomerService(
        this.form.region,
        this.service.imageUrl,
        this.service.textarea,
        (res)=>{
          console.log(res);
          this.isBind = false;
      });
    },
    // 添加保存
    submit() {
      this.$IP5.ModifyGameTask(
        this.addRuleForm.ID,
        this.addRuleForm.gameName, 
        this.checkAll ? ["全部"] : this.addRuleForm.channel, 
        this.addRuleForm.taskType, 
        this.addRuleForm.condition1, 
        this.addRuleForm.condition2, 
        this.addRuleForm.rewardName, 
        this.addRuleForm.rewardImage,  
        this.addRuleForm.description,
        // 默认图片
        (res)=>{
          console.log(res);
          this.isAdd = false;
          this.initData();
      });
    },
    // 修改数据
    unpdatSubmit() {},
    // 删除此条数据
    delSubmit() {
      this.$IP5.DeleteGameTask(
        this.aData.ID,
        res=>{
          this.initData();
          ElMessage.success({
            message: '已删除',
            type: 'success',
          });
          this.isDelete = false;
      });
    },

    // 获取上传文件
    uploadFile(fileName) {
      console.log("获取上传文件", fileName);
      this.service.imageUrl = fileName;
    },
    // 添加数据的上传图片
    addUploadFile(fileName) {
      console.log("获取上传文件", fileName);
      this.addRuleForm.rewardImage = fileName;
    },
    handleCheckAllChange(val) {
      this.addRuleForm.channel = val ? this.cities : [];
      this.isIndeterminate = false;
    },
    handleCheckedCitiesChange(value) {
      console.log(value);
      let checkedCount = value.length;
      this.checkAll = checkedCount === this.cities.length;
      this.isIndeterminate = checkedCount > 0 && checkedCount < this.cities.length;
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
  
  .el-col-R {
    text-align: right;
  }
}
/deep/ .el-icon-zoom-in {
  display: none;
}
</style>

