<template>
  <el-container>
    <el-header>
      <div class="label" >
        游戏名称：
        <el-select v-model="where.gameName" placeholder="请选择">
          <el-option
              v-for="(item,index) in gameNames"
              :key="index"
              :label="item"
              :value="item"
          />
        </el-select>
      </div>
      <div class="label" v-show="where.gameName">
        渠道：
        <el-select v-model="where.channel" placeholder="请选择">
          <el-option
              v-for="(item,index) in channels"
              :key="index"
              :label="item"
              :value="item"
          />
        </el-select>
      </div>
      <div class="label" v-show="where.gameName">
        分析类型：
        <el-select v-model="where.analysis" placeholder="请选择">
          <el-option
              v-for="(item,index) in analysisLabel"
              :key="index"
              :label="item"
              :value="item"
          />
        </el-select>
      </div>
      <div class="label" v-show="where.gameName">
        日期:
        <el-date-picker
            v-model="where.time"
            type="daterange"
            start-placeholder="开始日期"
            end-placeholder="结束日期"
            :default-time="defaultTime2"
        >
        </el-date-picker>
      </div>
      <el-button type="primary" icon="el-icon-search" @click="query">搜索</el-button>
      <el-button icon="el-icon-refresh-left" @click="ref"> 重置</el-button>
    </el-header>
    <el-main>
      <div class="title">
        <strong>{{where.analysis}}</strong>
      </div>
      <div id="analysisEcharts" style="width: 100%; height: 400px;"/>
    </el-main>
  </el-container>
</template>

<script>
import * as echarts from "echarts";
export default {
  name: "Analysis",
  data(){
    this.myChart = null;
    return {
      where:{
        gameName:null,
        channel:null,
        analysis:null,
        time:[null,null],
      },
      gameNames:[],
      channels:[],
      analysisLabel:[],
      defaultTime2: [
        new Date(2000, 1, 1, 0, 0, 0),
        new Date(2000, 2, 1, 23, 59, 59),
      ],
    }
  },
  mounted() {
    this.init()
    this.query()
  },
  watch:{
    "where.gameName"(nval,oval){
      console.log(nval);
      if(nval!=null){
        this.$IP5.GetChannel(nval,(res)=>{
          this.channels=res
          if(res!=null && res.length > 0)
            this.where.channel = res[0]
        })
        this.$IP5.GetAnalysisLabel(nval,(res)=>{
          this.analysisLabel=res
          if(res!=null && res.length > 0)
            this.where.analysis = res[0]
        })
      }
    }
  },
  methods:{
    init(){
      this.$IP5.GetAnalysisGame((res)=>{
        this.gameNames=res
        if(res!=null && res.length > 0)
          this.where.gameName = this.gameNames[0]
      })
    },
    query(){
      //游戏名为空则不访问接口
      if(!this.where.gameName)
        return
      let start , end
      if(this.where.time == null){
        start = null
        end = null
      }else {
        start = this.$Tool.GMT(this.where.time[0])
        end = this.$Tool.GMT(this.where.time[1])
      }

      this.$IP5.GetAnalysis(this.where.gameName,this.where.channel,this.where.analysis,start,end,res=>{
        let xAxisData = [],series = []
        res.forEach(item=>{
          xAxisData.push(item.Name)
          series.push(item.Count)
        })

        this.myChart = echarts.init(document.getElementById('analysisEcharts'));
        // 指定图表的配置项和数据
        let option = {
          xAxis: {
            type: 'category',
            data: xAxisData
          },
          yAxis: {
            type: 'value'
          },
          series: [
            {
              data: series,
              type: 'bar'
            }
          ],
          tooltip: {
            axisPointer: {
              type: 'shadow'
            },
            show: true,
            trigger: 'axis',
            formatter: (params) => {//提示框自定义
              return params[0].name + "：<br>" +params[0].value + '<br>';
            }
          },
        };
        // 使用刚指定的配置项和数据显示图表。
        this.myChart.setOption(option);
      })

    },
    ref(){
      Object.assign(this.where, this.$options.data().where)
      this.init()
    }
  },
  beforeUnmount() {
    console.log('beforeUnmount')
    if (!this.myChart) {
      return
    }
    // window.removeEventListener('resize', this.__resizeHandler)
    this.myChart.dispose()
    this.myChart = null
  },
}
</script>

<style scoped lang="scss">
.el-header{
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  height: auto !important;
  .label{
    margin: 5px;
  }
}
.el-main{
  .title{
    display: flex;
  }
}
</style>