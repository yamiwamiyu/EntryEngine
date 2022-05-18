"use strict";(self["webpackChunkclient"]=self["webpackChunkclient"]||[]).push([[513],{5471:function(t,o,e){e.d(o,{Z:function(){return v}});var s=e(3396),n=e(9242),l=e(7139);const i=t=>((0,s.dD)("data-v-d9de0d1c"),t=t(),(0,s.Cn)(),t),c={class:"float-btn"},h={class:"btn-list"},a=i((()=>(0,s._)("span",null,"置顶",-1))),r=[a],u=["onClick"],d=i((()=>(0,s._)("span",null,"触底",-1))),p=[d];function g(t,o,e,i,a,d){const g=(0,s.up)("van-icon");return(0,s.wg)(),(0,s.iD)("div",c,[(0,s._)("div",h,[(0,s.wy)(((0,s.wg)(),(0,s.iD)("div",{class:"btn",key:"float-btn-top",onClick:o[0]||(o[0]=(0,n.iM)((t=>d.choose("goTop")),["stop"]))},r)),[[n.F8,e.goTop]]),((0,s.wg)(!0),(0,s.iD)(s.HY,null,(0,s.Ko)(d.showList,((t,o)=>((0,s.wg)(),(0,s.iD)("div",{class:"btn",key:"float-btn-"+o,onClick:(0,n.iM)((e=>d.choose(o,t)),["stop"])},[(0,s._)("span",null,(0,l.zw)(t),1)],8,u)))),128)),(0,s.wy)(((0,s.wg)(),(0,s.iD)("div",{class:"btn",key:"float-btn-bottom",onClick:o[1]||(o[1]=(0,n.iM)((t=>d.choose("goBottom")),["stop"]))},p)),[[n.F8,e.goBottom]])]),(0,s.wy)(((0,s.wg)(),(0,s.iD)("div",{class:"btn",key:"float-btn-more",onClick:o[2]||(o[2]=(0,n.iM)((t=>a.showMore=!a.showMore),["stop"]))},[(0,s.Wm)(g,{name:a.showMore?"arrow-up":"arrow-down"},null,8,["name"])])),[[n.F8,d.hidden]])])}var m={components:{},props:{list:Array,showNum:{type:Number,default:3},goTop:Boolean,goBottom:Boolean},data(){return{showMore:!1}},computed:{hidden(){return this.list&&this.list.length>this.showNum},showList(){let t=[],o=this.list?this.list.length:0;!this.showMore&&this.showNum<o&&(o=this.showNum);for(let e=0;e<o;e++)t.push(this.list[e]);return t}},methods:{choose(t,o){this.$emit("choose",t,o)}},mounted(){}},w=e(89);const f=(0,w.Z)(m,[["render",g],["__scopeId","data-v-d9de0d1c"]]);var v=f},7934:function(t,o,e){e.d(o,{Z:function(){return u}});var s=e(3396),n=e(7139);const l={class:"nav"},i={class:"title"};function c(t,o,e,c,h,a){const r=(0,s.up)("van-icon"),u=(0,s.up)("router-link");return(0,s.wg)(),(0,s.iD)("div",l,[((0,s.wg)(!0),(0,s.iD)(s.HY,null,(0,s.Ko)(h.nav,(t=>((0,s.wg)(),(0,s.j4)(u,{key:t.title,to:t.url,class:(0,n.C_)("item "+a.isSelected(t))},{default:(0,s.w5)((()=>[(0,s.Wm)(r,{name:t.icon,size:"1.5rem",class:"icon"},null,8,["name"]),(0,s._)("span",i,(0,n.zw)(t.title),1)])),_:2},1032,["to","class"])))),128))])}var h={data(){return{nav:[{title:"服务",url:"/services",icon:"desktop-o"},{title:"管理",url:"/mine",icon:"manager-o"}]}},methods:{isSelected(t){let o=location.hash=="#"+t.url;return o||(o=location.pathname==t.url),o?"selected":"unselected"}}},a=e(89);const r=(0,a.Z)(h,[["render",c],["__scopeId","data-v-d07655d4"]]);var u=r},97:function(t,o,e){e.d(o,{Z:function(){return w}});var s=e(3396),n=e(9242),l=e(7139);const i={class:"table"},c={align:"left"},h={key:0,class:"selector"},a=["onClick"],r=["onClick"],u=["innerHTML"];function d(t,o,e,d,p,g){const m=(0,s.up)("van-checkbox");return(0,s.wg)(),(0,s.iD)("table",i,[(0,s._)("thead",null,[(0,s._)("tr",c,[e.choose?((0,s.wg)(),(0,s.iD)("th",h)):(0,s.kq)("",!0),((0,s.wg)(!0),(0,s.iD)(s.HY,null,(0,s.Ko)(g.thead,((t,o)=>((0,s.wg)(),(0,s.iD)("th",{key:"thead-"+o,onClick:(0,n.iM)((t=>g.clickTile(o)),["stop"]),style:(0,l.j5)(e.theadStyle?e.theadStyle[o]:null)},(0,l.zw)(t),13,a)))),128))])]),(0,s._)("tbody",null,[((0,s.wg)(!0),(0,s.iD)(s.HY,null,(0,s.Ko)(e.list,((t,i)=>((0,s.wg)(),(0,s.iD)("tr",{align:"left",valign:"top",key:"tbody-"+i,onClick:(0,n.iM)((o=>g.clickRow(t)),["stop"])},[e.choose?((0,s.wg)(),(0,s.iD)("td",{key:0,onClick:o[0]||(o[0]=(0,n.iM)((()=>{}),["stop"])),style:{padding:"0"}},[(0,s.Wm)(m,{style:{"justify-content":"center",padding:"10px 5px"},modelValue:p.chooseValues[i],"onUpdate:modelValue":t=>p.chooseValues[i]=t,onClick:(0,n.iM)((t=>g.onChoose(i)),["stop"])},null,8,["modelValue","onUpdate:modelValue","onClick"])])):(0,s.kq)("",!0),((0,s.wg)(!0),(0,s.iD)(s.HY,null,(0,s.Ko)(g.tbody,((o,n)=>((0,s.wg)(),(0,s.iD)("td",{key:"tbody-td-"+n,innerHTML:t[o],style:(0,l.j5)(e.tbodyStyle?e.tbodyStyle[n]:null)},null,12,u)))),128))],8,r)))),128))])])}var p={components:{},props:{list:Array,option:Object,choose:{type:Number,default:0,validator:function(t){return[0,1,2].includes(t)}},theadStyle:Object,tbodyStyle:Object},data(){return{chooseValues:[],chooseList:[],nextChoose:null}},computed:{thead(){let t=[];for(const o in this.option)t.push(o);return t},tbody(){let t=[];for(const o in this.option)t.push(this.option[o]);return t},selected(){let t=[];for(let o=0;o<this.chooseValues.length;o++)this.chooseValues[o]&&t.push(this.list[o]);return t}},watch:{list:{handler:function(t){console.log("table",t)},deep:!0}},methods:{clickRow(t){this.$emit("clickRow",t)},onChoose(t){console.log("点击选择",t,this.chooseValues),1==this.choose&&(console.log("单选",this.nextChoose),this.nextChoose!=t&&(null!=this.nextChoose&&(this.chooseValues[this.nextChoose]=!1),this.nextChoose=t)),this.chooseList=[];for(const o in this.chooseValues)this.chooseValues[o]&&this.chooseList.push(o);this.$emit("chooseClick",this.chooseList),this.$forceUpdate()},initChooseValues(){let t=[];if(this.choose&&this.list.length)for(let o=0;o<this.list.length;o++,t.push(!1));this.chooseValues=t},clearChoose(){if(this.choose&&this.list.length)for(let t=0;t<this.list.length;t++)this.chooseValues[t]=!1;this.chooseList=[],this.$forceUpdate()},verifyChoose(t){return new Promise(((o,e)=>{this.chooseList.length?o():(this.$Toast(t||"请选择数据"),e())}))},clickTile(t){this.$emit("clickTile",t)}},mounted(){this.initChooseValues(),console.log("props---\x3e",this.$props)}},g=e(89);const m=(0,g.Z)(p,[["render",d],["__scopeId","data-v-73baa144"]]);var w=m},5513:function(t,o,e){e.r(o),e.d(o,{default:function(){return d}});var s=e(3396);const n={class:"container"};function l(t,o,e,l,i,c){const h=(0,s.up)("Table"),a=(0,s.up)("Nav"),r=(0,s.up)("FloatBtn");return(0,s.wg)(),(0,s.iD)("div",n,[(0,s.Wm)(h,{ref:"table",list:i.list,option:{IP:"EndPoint","名称":"NickName"}},null,8,["list"]),(0,s.Wm)(a),(0,s.Wm)(r,{list:["更新"],onChoose:c.chooseFloatBtn},null,8,["onChoose"])])}var i=e(7934),c=e(97),h=e(5471),a={components:{Nav:i.Z,Table:c.Z,FloatBtn:h.Z},data(){return{list:[]}},methods:{getServers(){this.$IMBSProxy.GetServers((t=>{console.log("list",t),this.list=t}))},chooseFloatBtn(t){console.log("chooseFloatBtn",t),this.$IMBSProxy.UpdateServer((t=>{t&&(this.$Toast("更新成功"),this.getServers())}))}},mounted(){this.getServers()}},r=e(89);const u=(0,r.Z)(a,[["render",l]]);var d=u}}]);