<template>
  <div class="map">
    <div></div>
    <div id="main" style="width: 800px; height: 400px;"/>
  </div>
</template>
<script>
import * as echarts from 'echarts';
export default {
  data(){
    return {}
  },
  mounted() {
    let data = this.generateData(5e5);
    let myChart = echarts.init(document.getElementById('main'));
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
        trigger: 'axis',
        axisPointer: {
          type: 'shadow'
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
        data: data.categoryData,
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
          data: data.valueData,
          // Set `large` for large data amount
          large: true
        }
      ]
    };
    // 使用刚指定的配置项和数据显示图表。
    myChart.setOption(option);
  },
  methods:{
    generateData() {
      // this.$IP5.OnlineCount()
      let baseValue = Math.random() * 1000;
      let time = +new Date(2011, 0, 1);
      let smallBaseValue;
      function next(idx) {
        smallBaseValue =
            idx % 30 === 0
                ? Math.random() * 700
                : smallBaseValue + Math.random() * 500 - 250;
        baseValue += Math.random() * 20 - 10;
        return Math.max(0, Math.round(baseValue + smallBaseValue) + 3000);
      }
      const categoryData = [];
      const valueData = [];
      for (let i = 0; i < 100; i++) {
        categoryData.push(
            echarts.format.formatTime('yyyy-MM-dd\nhh:mm:ss', time, false)
        );
        valueData.push(next(i).toFixed(2));
        time += 1000;
      }
      return {
        categoryData: categoryData,
        valueData: valueData
      };
    }
  },
  /*setup() {
    //methods
    /!*const echartInit = () =>{
      let myChart = echarts.init(document.getElementById('main'));
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
          trigger: 'axis',
          axisPointer: {
            type: 'shadow'
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
          data: data.categoryData,
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
            data: data.valueData,
            // Set `large` for large data amount
            large: true
          }
        ]
      };
      // 使用刚指定的配置项和数据显示图表。
      myChart.setOption(option);
    }*!/
    //onMounted
    onMounted(()=>{
      echartInit()
    })
    //return
    return {
      echartInit
    };
  }*/
};
</script>