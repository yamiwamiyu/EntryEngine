<template>
  <el-container class="container">
    <el-header>
      <el-row>
        <titleUI :keywords="form" @onQuery="onQuery()" @res="res()"></titleUI>
      </el-row>
    </el-header>
    <el-main class="main">
      <el-row class="row-bg-title" justify="space-between">
        <el-col :span="6" class="el-col-L">
          <b>数据列表</b>
        </el-col>
        <el-col :span="12" class="el-col-R">
          <el-button :disabled="aData != null && aData.Type ==1" type="primary" @click="Bind()">绑定游戏</el-button>
          <el-button type="primary" @click="Add()">添加</el-button>
          <el-button type="primary" @click="Modify()">修改</el-button>
          <el-button type="primary" @click="Delete()">删除</el-button>
        </el-col>
      </el-row>
      <el-table 
      class="_el-thead-radio"
      :data="tableData" 
      border 
      style="width: 100%"
      highlight-current-row
      @current-change="handleCurrentChange">
        <el-table-column prop="Account" label="账号" width="180">
        </el-table-column>
        <el-table-column prop="Nickname" label="昵称" width="180">
        </el-table-column>
        <el-table-column prop="Type" label="账号属性"> 
          <template #default="scope">
            <div>{{scope.row.Type==1?"平台账号":"普通账号"}}</div>
          </template>
        </el-table-column>
        <el-table-column prop="State" label="状态" width="180">
          <template #default="scope">
            禁用 <el-switch :disabled="$store.state.userInfo.ID==scope.row.ID" v-model="scope.row.State" @change="beforeChange(scope.row.ID)"></el-switch> 正常
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
  <!-- 绑定 -->
  <el-dialog title="绑定游戏" v-model="isBind" width="50%" center>
    
    <el-form-item>
      <el-checkbox-group v-model="bindType">
        <el-checkbox  
          v-for="(item, index) in gameList"
          :key="'gameList' + index"
          :label="item.text"
          :checked="item.checked"
          :value="item"></el-checkbox>
      </el-checkbox-group>
    </el-form-item>
    <template #footer>
      <span class="dialog-footer">
        <el-button type="primary" @click="bind()">确 定</el-button>
        <el-button @click="isBind = false">取 消</el-button>
      </span>
    </template>
  </el-dialog>
  <!-- 添加 -->
  <el-dialog title="添加" v-model="isAdd" width="30%" center>
    <el-form
      :model="addRuleForm"
      :rules="addRules"
      ref="addRuleForm"
      label-width="100px"
      class="demo-BindRuleForm"
    >
      <el-form-item label="账号" prop="account">
        <el-input v-model="addRuleForm.account"></el-input>
      </el-form-item>
      <el-form-item label="密码" prop="password">
        <el-input
          placeholder="请输入密码"
          v-model="addRuleForm.password"
          show-password
        ></el-input>
      </el-form-item>
                
      <el-form-item label="账号属性" prop="type">
        <el-select v-model="addRuleForm.type" placeholder="请选择活动区域">
          <el-option v-for="(item,index) in platformType"
                :key="'platformType'+index" :label="item.text" :value="item.type"></el-option>
        </el-select>
      </el-form-item>
      <el-form-item label="昵称">
        <el-input v-model="addRuleForm.name"></el-input>
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
  <el-dialog title="修改" v-model="isModify" width="30%" center>
    <el-form
      :model="updateRuleForm"
      ref="updateRuleForm"
      label-width="100px"
      class="demo-BindRuleForm"
    >
      <el-form-item label="账号">
        <el-input v-model="updateRuleForm.account" 
          disabled="true"></el-input>
      </el-form-item><el-form-item label="密码">
        <el-input
          placeholder="请输入密码"
          v-model="updateRuleForm.password"
          show-password
        ></el-input>
      </el-form-item>
                
      <el-form-item label="账号属性" prop="type">
        <el-select v-model="updateRuleForm.type" placeholder="请选择活动区域">
          <el-option v-for="(item,index) in platformType"
                :key="'platformType'+index" :label="item.text" :value="item.type"></el-option>
        </el-select>
      </el-form-item>
      <el-form-item label="昵称">
        <el-input v-model="updateRuleForm.name"></el-input>
      </el-form-item>
    </el-form>
    <template #footer>
      <span class="dialog-footer">
        <el-button type="primary" @click="unpdatSubmit()">确 定</el-button>
        <el-button @click="isModify = false">取 消</el-button>
      </span>
    </template>
  </el-dialog>
  <!-- 删除 -->
  <el-dialog title="删除" v-model="isDelete" width="30%" center>
    <el-row>
      <el-col>
        确定要删除账号吗？
      </el-col>
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
import titleUI from "@/components/titleUI.vue";

export default {
  name: "SystemSetting",
  components: {
    titleUI,
  }, 
  data() {
    return {
      tableData: [
      ],
      page: {
        index: 0,
        total: 0,
        size: 15,
      },
      centerDialogVisible: true,
      // 搜索的账号
      form: {
        account: ''
      },
      // 弹窗控制
      isBind: false, 
      isAdd: false, 
      isModify: false,
      isDelete: false,
      BindRuleForm: {
        account: '',
        password:'',
        type: '',
        name: '',
      },
      // 添加的数据
      addRuleForm: {
        ID: 0,
        account: '',
        password:'',
        type: '',
        name: '',
      },
      addRules: {
        account: [
          { required: true, message: '请输入活动名称', trigger: 'blur' },
        ],
        password: [
          { required: true, message: '请输入密码', trigger: 'blur' },
        ],
        type: [
          { required: true, message: '请选择账号属性', trigger: 'change' },
        ],
      },
      // 修改的数据
      updateRuleForm: {
        ID: 0,
        account: '',
        password:'',
        type: '',
        name: '',
      },
      // 绑定游戏列表
      bindType:[],
      // 当前选中数据
      aData: null,
      userInfo:'',
      platformType: [{type:1,text:"平台账号"},{type:2,text:"普通账号"}],
      // 绑定游戏列表
      gameList: [],
    };
  },
  mounted() {
    this.initData();
  },
  methods: {
    // 查找
    onQuery() {
      Object.assign(this.page, this.$options.data().page);
      this.initData();
    },
    // 重置查找
    res() {
      this.form.account = "";
      Object.assign(this.page, this.$options.data().page);
      this.initData();
    },
    clickPages(pageIndex) {
      this.page.index = pageIndex - 1;
      this.initData();
    },
    // 打开绑定窗口
    Bind() {
      if(this.aData != null && this.aData != undefined){  
        this.bindType = [];
        this.gameList = [];
        this.isBind = true;
        this.getGameName();
      } else {
        ElMessage.error('请选中一条数据后再操作!');
      }
    },
    // 打开添加窗口
    Add() {
      Object.assign(this.addRuleForm, this.$options.data().addRuleForm);
      this.isAdd  = true;
    },
    // 打开修改窗口
    Modify() {
      console.log(this.aData);
      if(this.aData != null && this.aData != undefined){      
        Object.assign(this.addRuleForm, this.$options.data().addRuleForm);
        this.isModify  = true;
        this.updateRuleForm.ID = this.aData.ID;
        this.updateRuleForm.account = this.aData.Account;
        this.updateRuleForm.type = this.aData.Type;
        this.updateRuleForm.name = this.aData.Nickname;
        console.log(this.updateRuleForm);
      } else {
        ElMessage.error('请选中一条数据后再操作!');
      }
    },
    // 打开删除窗口
    Delete() {
      if(this.aData != null && this.aData != undefined){
        this.isDelete = true;
      } else {
        ElMessage.error('请选中一条数据后再操作!');
      }
    },
    // 当前选中的某一条数据
    handleCurrentChange(val) {
      this.aData = val;
      console.log(val);
    },
    // 初始化的数据
    initData() {
      this.$IP5.GetAccountList(this.page.index, this.form.account, res=>{
        this.page.total = res.Count;
        this.page.size = res.PageSize;
        this.page.index = res.Page;
        this.tableData = res.Models;
        console.log(res);
      });
    },
    // 添加保存
    submit() {
      this.$IP5.ModifyAccount(
        this.addRuleForm.ID,
        this.addRuleForm.account,
        this.addRuleForm.password,
        this.addRuleForm.type,
        this.addRuleForm.name,
        res=>{
          this.initData();
          ElMessage.success({
            message: '保存成功',
            type: 'success',
          });
          this.isAdd = false;
        });
    },
    // 绑定游戏
    bind(){
      console.log(this.bindType);
      this.$IP5.BindGame(this.aData.ID, this.bindType,res=>{
        this.initData();
        ElMessage.success({
          message: '绑定成功',
          type: 'success',
        });
        this.isBind = false;
      });
    },
    // 切换账号状态
    beforeChange(id){
      this.$IP5.ChangeAccountState(id, res=>{
        if(res.errMsg){
          console.log("123");
        }
        console.log(res);
      });
    },
    // 修改数据
    unpdatSubmit(){
      this.$IP5.ModifyAccount(
        this.updateRuleForm.ID,
        this.updateRuleForm.account,
        this.updateRuleForm.password,
        this.updateRuleForm.type,
        this.updateRuleForm.name,
        res=>{
          this.initData();
          ElMessage.success({
            message: '修改成功',
            type: 'success',
          });
          this.isModify = false;
      });
    },
    // 删除此条数据
    delSubmit() {
      this.$IP5.DeleteAccount(
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
    // 获取绑定游戏列表
    getGameName(){
      this.$IP5.GetGameName((res) => {
        // res.forEach(element => {
          res.filter((n) =>{
              if(n != "全部"){
                if(this.aData.ManagerGames.indexOf(n) != -1){
                  this.gameList.push({text:n,checked: true});
                } else {
                  this.gameList.push({text:n,checked: false});
                }
              }
            });
            
        // });
      });
    }
  },
};
</script>

<style scoped lang="scss">
.container {
  display: flex;
  .row-bg-title {
    margin-bottom: 15px;
  }
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
  .el-col-L {
    text-align: left;
  }
  .el-col-R {
    text-align: right;
  }
}
</style>
