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
      <div class="label" >
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
      <div class="label">
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
        <strong>均值<span>{{tableData.TimeAvg}}</span>分钟</strong>
      </div>
      <div id="lineCountEcharts" style="width: 100%; height: 400px;"/>
    </el-main>
  </el-container>
</template>

<script>
import * as echarts from "echarts";

export default {
  name: "OnLineCount",

  data(){
    this.myChart = null;
    return {
      where:{
        gameName:'全部',
        channel:'全部',
        time:[null,null],
      },
      gameNames:[],
      channels:[],
      defaultTime2: [
        new Date(2000, 1, 1, 0, 0, 0),
        new Date(2000, 2, 1, 23, 59, 59),
      ],
      tableData:{
        TimeAvg: 0,
        Info: [],
        NowCount: 0,
        NowTime: null,
      }

    }
  },
  mounted() {
    this.init()
    this.query()
  },
  methods:{
    init(){
      this.$IP5.GetGameName((res)=>{
        this.gameNames=res
      })
      this.$IP5.GetChannel(this.where.gameName,(res)=>{
        this.channels=res
      })
    },
    query(){
      let start , end
      if(this.where.time == null){
        start = null
        end = null
      }else {
        start = this.$Tool.GMT(this.where.time[0])
        end = this.$Tool.GMT(this.where.time[1])
      }

      this.$IP5.GameTime(this.where.gameName,this.where.channel,start,end,(res)=>{
        this.tableData = res
        let categoryData = [],valueData = []
        res.GameTimes.forEach(item=>{
          categoryData.push(item.Time)
          valueData.push(item.Count)
        })
        this.myChart = echarts.init(document.getElementById('lineCountEcharts'));
        // 指定图表的配置项和数据
        let option = {
          toolbox: {
            feature: {
              dataZoom: {
                yAxisIndex: false
              },
              saveAsImage: {
                pixelRatio: 2
              }
            }
          },
          tooltip: {
            axisPointer: {
              type: 'shadow'
            },
            trigger: 'axis',
            formatter: (params) => {//提示框自定义
              return params[0].name + "：<br>" +params[0].value + '次<br>';
            }
          },
          grid: {
            bottom: 90
          },
          dataZoom: [
            {
              type: 'inside'
            },
            {
              type: 'slider'
            }
          ],
          xAxis: {
            data: categoryData,
            silent: false,
            splitLine: {
              show: false
            },
            splitArea: {
              show: false
            }
          },
          yAxis: {
            splitArea: {
              show: false
            }
          },
          series: [
            {
              type: 'bar',
              data: valueData,
              // Set `large` for large data amount
              large: true
            }
          ]
        };
        // 使用刚指定的配置项和数据显示图表。
        this.myChart.setOption(option);
      })
    },
    ref(){
      Object.assign(this.where, this.$options.data().where);
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
      justify-content: space-between;
      span{
        font-size: 30px;
        font-weight: normal;
      }
    }
  }
</style>