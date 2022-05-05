<template>
  <el-form class="keywords" ref="keywords" :model="keywords">

    <el-form-item label="游戏名称：" v-if="form.region !== undefined && !exclusive.includes('region')"
>
      <el-select v-model="form.region" @change="getAccessList()">
        <el-option
          v-for="(item, index) in gameList"
          :key="'gameList' + index"
          :label="item"
          :value="item"
        ></el-option>
      </el-select>
    </el-form-item>


    <el-form-item label="渠道：" v-if="form.channel !== undefined && !exclusive.includes('channel')"
>
      <el-select v-model="form.channel" placeholder="请选择渠道">
        <el-option
          v-for="(item, index) in accessList"
          :key="'accessList' + index"
          :label="item"
          :value="item"
        ></el-option>
      </el-select> 
    </el-form-item>
    <!--inputName-->
    <div v-for="(item, index) in inputName"
        :key="item.name + index">
      <el-form-item
        :label-width="item.nameWitdth ? item.nameWitdth : '100px'"
        :label="(labels[item.name] ? labels[item.name] : item.label) + '：'"
        v-if="form[item.name] !== undefined && !exclusive.includes(item.name)"
      >
        <el-input      
          v-model="form[item.name]"
          clearable
        ></el-input>
      </el-form-item>
    </div>

    <slot></slot>

    <el-form-item label="日期：" v-if="form.date !== undefined && !exclusive.includes('date')">
      <el-date-picker
        v-model="form.date"
        type="daterange"
        range-separator="至"
        start-placeholder="开始日期"
        end-placeholder="结束日期"
      >
      </el-date-picker>
    </el-form-item>
    <el-form-item label-width="10px">
      <el-button
        @click="onQuery()"
        type="primary"
        style="
          height: 32px;
          color: white;
          border: 0px;
          font-weight: bold;
          margin-left: 2px;
          border-radius: 6px;
        "
        >搜索</el-button
      >
      <el-button
        type="primary"
        style="height: 32px; font-weight: bold; border-radius: 6px"
        @click="res()"
        >重置</el-button
      >
    </el-form-item>
  </el-form>
</template>

<script>
    /**
     * keywords:校验参数,当没有这个参数时将会隐藏输入框
     * labels:自定义label标签
     * exclusive:排除的字段名数组
     * updateWhere(val) 当参数改变时回调,val查询条件对象
     * onQuery() 点击查询时调用
     * res() 重置时调用
     *
     * 插槽写法
     <el-form-item
      :label-width="item.nameWitdth ? item.nameWitdth : '100px'"
      v-for="(item,index) in inputName"
      :key="item.name+index"
      :label="(labels[item.name]?labels[item.name]:item.label)+'：'"
      v-if="form[item.name]!==undefined&&!exclusive.includes(item.name)"
      >
      <el-input v-model="form[item.name]" clearable></el-input>
      </el-form-item>
      */
    export default {
        name: "Playe",
        props: {
            /*校验参数,当没有这个参数时将会隐藏输入框*/
            keywords: {
                type: [Object],
                default: () => {
                    return {};
                },
            },
            /*自定义labels名称*/
            labels: {
                type: [Object],
                default: () => {
                    return {};
                },
            },
            /*排除的字段名数组*/
            exclusive: {
                type: [Array],
                default: () => {
                    return [];
                },
            },
        },
        data() {
            return {
                form: {},
                /*原始的数据*/
                // cform: {},
                /*
                 * 账号 account
                 * */
                inputName: [
                  {
                    name: "account",
                    label: "账号",
                    nameWitdth: "60px",
                  },
                ],
                // 游戏名称数据
                gameList: [],
                // 渠道数据 
                accessList: [],
            };
        },
        watch: {

            /*当监听到改变时调用父类*/
            form: {
                deep: true, //深度监听设置为 true
                handler: function(val, oldV) {
                    this.$emit("updateWhere", val);
                    this.$emit("update:keywords", val);
                },
            },
        },
        mounted() {
            this.form = this.keywords;
            this.init();
        },
        methods: {
            init() {
              if(this.form.region != undefined)
                this.getGameList();
            },
            /*点击查询*/
            onQuery() {
                this.$emit("onQuery");
                console.log(this.form,this.cform);
            },
            /*点击重置*/
            res() {
              if(this.form.region != undefined)
                this.getGameList();
              this.$emit("res");
            },
            // 获取游戏名称   传入regionType =1获取不包含全部的游戏名
            getGameList() {
              if(!this.form.regionType){
                this.$IP5.GetGameName((res) => {
                  this.gameList = res;
                  this.form.region = res[0];
                  this.getAccessList();
                });
              } else {
                this.$IP5.GetAnalysisGame((res) => {
                  this.gameList = res;
                  this.form.region = res[0];
                  this.getAccessList();
                });
              }
            },
            // 获取渠道
            getAccessList() {
              if(this.form.channel == null || this.form.channel == undefined)
                return;
              this.$IP5.GetChannel(this.form.region, (res) => {
                  this.accessList = res;
                  this.form.channel = res[0];
              });
            },
        },
    };
</script>
<style lang="scss">
    .keywords {
        margin: 0;
        display: flex;
        flex-wrap: wrap;
        .el-form-item {
            float: left;
            margin: 0 15px 10px 4px;
            .el-form-item__content * {
                font-size: 12px;
            }
        }
    }
</style>