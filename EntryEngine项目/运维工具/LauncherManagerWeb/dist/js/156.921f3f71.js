"use strict";(self["webpackChunkclient"]=self["webpackChunkclient"]||[]).push([[156],{9959:function(t,e,o){o.d(e,{Z:function(){return w}});var s=o(3396),i=o(9242),l=o(7139);const n=t=>((0,s.dD)("data-v-af82bad6"),t=t(),(0,s.Cn)(),t),a={class:"float-btn"},h={class:"btn-list"},r=n((()=>(0,s._)("span",null,"置顶",-1))),u=[r],c=["onClick"],d=n((()=>(0,s._)("span",null,"触底",-1))),p=[d];function m(t,e,o,n,r,d){const m=(0,s.up)("van-icon");return(0,s.wg)(),(0,s.iD)("div",a,[(0,s._)("div",h,[(0,s.wy)(((0,s.wg)(),(0,s.iD)("div",{class:"btn",key:"float-btn-top",onClick:e[0]||(e[0]=(0,i.iM)((t=>d.choose("goTop")),["stop"]))},u)),[[i.F8,o.goTop]]),((0,s.wg)(!0),(0,s.iD)(s.HY,null,(0,s.Ko)(d.showList,((t,e)=>((0,s.wg)(),(0,s.iD)("div",{class:"btn",key:"float-btn-"+e,onClick:(0,i.iM)((o=>d.choose(e,t)),["stop"])},[(0,s._)("span",null,(0,l.zw)(t),1)],8,c)))),128)),(0,s.wy)(((0,s.wg)(),(0,s.iD)("div",{class:"btn",key:"float-btn-bottom",onClick:e[1]||(e[1]=(0,i.iM)((t=>d.choose("goBottom")),["stop"]))},p)),[[i.F8,o.goBottom]])]),(0,s.wy)(((0,s.wg)(),(0,s.iD)("div",{class:"btn",key:"float-btn-more",onClick:e[2]||(e[2]=(0,i.iM)((t=>r.showMore=!r.showMore),["stop"]))},[(0,s.Wm)(m,{name:r.showMore?"arrow-up":"arrow-down"},null,8,["name"])])),[[i.F8,d.hidden]])])}var f={components:{},props:{list:Array,showNum:{type:Number,default:3},goTop:Boolean,goBottom:Boolean},data(){return{showMore:!1}},computed:{hidden(){return this.list&&this.list.length>this.showNum},showList(){let t=[],e=this.list.length;!this.showMore&&this.showNum<e&&(e=this.showNum);for(let o=0;o<e;o++)t.push(this.list[o]);return t}},methods:{choose(t,e){this.$emit("choose",t,e)}},mounted(){}},g=o(89);const y=(0,g.Z)(f,[["render",m],["__scopeId","data-v-af82bad6"]]);var w=y},8107:function(t,e,o){o.d(e,{Z:function(){return d}});var s=o(3396),i=o(9242),l=o(7139);const n=["id"],a=["onClick"];function h(t,e,o,h,r,u){const c=(0,s.up)("van-field"),d=(0,s.up)("van-icon");return(0,s.wg)(),(0,s.iD)("div",{class:"input-select",id:"inputSelect"+u.id,key:"input-select-"+u.id,onClick:e[3]||(e[3]=(0,i.iM)((()=>{}),["stop"]))},[(0,s.Wm)(c,{modelValue:r.inputValue,"onUpdate:modelValue":e[0]||(e[0]=t=>r.inputValue=t),name:o.name,label:o.label,placeholder:o.placeholder,rules:o.rules,onFocus:e[1]||(e[1]=t=>u.inputFocus()),onBlur:e[2]||(e[2]=t=>u.inputBlur())},null,8,["modelValue","name","label","placeholder","rules"]),(0,s.wy)((0,s._)("div",{class:"list",style:(0,l.j5)("top"==o.listPosition?"bottom: "+r.inputSelectHeight+"px;":null)},[(0,s._)("ul",null,[((0,s.wg)(!0),(0,s.iD)(s.HY,null,(0,s.Ko)(u.searchList,((t,e)=>((0,s.wg)(),(0,s.iD)("li",{key:"input-select-"+u.id+"-list-"+ +e,onClick:(0,i.iM)((t=>u.select(e)),["stop"])},[(0,s._)("span",null,(0,l.zw)(o.listKey?t[o.listKey]:t),1),(0,s.wy)((0,s.Wm)(d,{name:"cross",onClick:(0,i.iM)((t=>u.del(e)),["stop"])},null,8,["onClick"]),[[i.F8,1!=o.inputType]])],8,a)))),128))])],4),[[i.F8,r.showList]])],8,n)}var r={name:"InputSelect",components:{},props:{value:{type:String},inputType:{type:Number,default:0,validator:function(t){return[0,1].includes(t)}},search:{type:Boolean,default:!0},name:{type:String},label:{type:String},placeholder:{type:String},rules:{type:Array},list:{type:Array},listKey:String,bindKey:String,listPosition:{type:String,default:"bottom",validator:function(t){return["bottom","top"].includes(t)}}},data(){return{inputValue:"",showList:!1,showListEnable:!0,bindValue:"",inputSelectHeight:44,isSelect:!1}},computed:{id(){return this._uid},searchList(){if(this.search){let t=this.list.filter((t=>{const e=this.listKey?t[this.listKey]:t;return!!e&&-1!=e.indexOf(this.inputValue)}));return t}return this.list}},watch:{value:function(t){!this.isSelect&&(this.inputValue=t),this.isSelect=!1},inputValue:function(t){this.$emit("update:model-value",this.bindKey?this.bindValue:t)}},methods:{inputFocus(){document.body.click(),this.setInputSelectHeight(),this.showList=!0},inputBlur(){1==this.inputType&&(this.inputValue="")},del(t){console.log("del",t),this.$emit("del",t)},select(t){this.setInputSelectHeight(),this.showList=!1,this.bindValue=this.bindKey?this.list[t][this.bindKey].toString():this.list[t],this.inputValue=this.listKey?this.list[t][this.listKey]:this.list[t],this.isSelect=!0},setInputSelectHeight(){this.$nextTick().then((()=>{setTimeout((()=>{this.inputSelectHeight=document.getElementById("inputSelect"+this.id).clientHeight}),500)}))}},mounted(){this.$nextTick((()=>{document.body.addEventListener("click",(()=>{this.showList=!1}))}))}},u=o(89);const c=(0,u.Z)(r,[["render",h],["__scopeId","data-v-6c2d99e8"]]);var d=c},97:function(t,e,o){o.d(e,{Z:function(){return g}});var s=o(3396),i=o(9242),l=o(7139);const n={class:"table"},a={align:"left"},h={key:0,class:"selector"},r=["onClick"],u=["onClick"],c=["innerHTML"];function d(t,e,o,d,p,m){const f=(0,s.up)("van-checkbox");return(0,s.wg)(),(0,s.iD)("table",n,[(0,s._)("thead",null,[(0,s._)("tr",a,[o.choose?((0,s.wg)(),(0,s.iD)("th",h)):(0,s.kq)("",!0),((0,s.wg)(!0),(0,s.iD)(s.HY,null,(0,s.Ko)(m.thead,((t,e)=>((0,s.wg)(),(0,s.iD)("th",{key:"thead-"+e,onClick:(0,i.iM)((t=>m.clickTile(e)),["stop"]),style:(0,l.j5)(o.theadStyle?o.theadStyle[e]:null)},(0,l.zw)(t),13,r)))),128))])]),(0,s._)("tbody",null,[((0,s.wg)(!0),(0,s.iD)(s.HY,null,(0,s.Ko)(o.list,((t,n)=>((0,s.wg)(),(0,s.iD)("tr",{align:"left",valign:"top",key:"tbody-"+n,onClick:(0,i.iM)((e=>m.clickRow(t)),["stop"])},[o.choose?((0,s.wg)(),(0,s.iD)("td",{key:0,onClick:e[0]||(e[0]=(0,i.iM)((()=>{}),["stop"])),style:{padding:"0"}},[(0,s.Wm)(f,{style:{"justify-content":"center",padding:"10px 5px"},modelValue:p.chooseValues[n],"onUpdate:modelValue":t=>p.chooseValues[n]=t,onClick:(0,i.iM)((t=>m.onChoose(n)),["stop"])},null,8,["modelValue","onUpdate:modelValue","onClick"])])):(0,s.kq)("",!0),((0,s.wg)(!0),(0,s.iD)(s.HY,null,(0,s.Ko)(m.tbody,((e,i)=>((0,s.wg)(),(0,s.iD)("td",{key:"tbody-td-"+i,innerHTML:t[e],style:(0,l.j5)(o.tbodyStyle?o.tbodyStyle[i]:null)},null,12,c)))),128))],8,u)))),128))])])}var p={components:{},props:{list:Array,option:Object,choose:{type:Number,default:0,validator:function(t){return[0,1,2].includes(t)}},theadStyle:Object,tbodyStyle:Object},data(){return{chooseValues:[],chooseList:[],nextChoose:null}},computed:{thead(){let t=[];for(const e in this.option)t.push(e);return t},tbody(){let t=[];for(const e in this.option)t.push(this.option[e]);return t},selected(){let t=[];for(let e=0;e<this.chooseValues.length;e++)this.chooseValues[e]&&t.push(this.list[e]);return t}},watch:{list:{handler:function(t){console.log("table",t)},deep:!0}},methods:{clickRow(t){this.$emit("clickRow",t)},onChoose(t){console.log("点击选择",t,this.chooseValues),1==this.choose&&(console.log("单选",this.nextChoose),this.nextChoose!=t&&(null!=this.nextChoose&&(this.chooseValues[this.nextChoose]=!1),this.nextChoose=t)),this.chooseList=[];for(const e in this.chooseValues)this.chooseValues[e]&&this.chooseList.push(e);this.$emit("chooseClick",this.chooseList),this.$forceUpdate()},initChooseValues(){let t=[];if(this.choose&&this.list.length)for(let e=0;e<this.list.length;e++,t.push(!1));this.chooseValues=t},clearChoose(){if(this.choose&&this.list.length)for(let t=0;t<this.list.length;t++)this.chooseValues[t]=!1;this.chooseList=[],this.$forceUpdate()},verifyChoose(t){return new Promise(((e,o)=>{this.chooseList.length?e():(this.$Toast(t||"请选择数据"),o())}))},clickTile(t){this.$emit("clickTile",t)}},mounted(){this.initChooseValues(),console.log("props---\x3e",this.$props)}},m=o(89);const f=(0,m.Z)(p,[["render",d],["__scopeId","data-v-73baa144"]]);var g=f},3156:function(t,e,o){o.r(e),o.d(e,{default:function(){return d}});var s=o(3396);const i={class:"container"};function l(t,e,o,l,n,a){const h=(0,s.up)("van-nav-bar"),r=(0,s.up)("Table"),u=(0,s.up)("FloatBtn"),c=(0,s.up)("van-field"),d=(0,s.up)("input-select"),p=(0,s.up)("van-form"),m=(0,s.up)("van-dialog");return(0,s.wg)(),(0,s.iD)("div",i,[(0,s.Wm)(h,{title:"账号管理","left-arrow":"",onClickLeft:e[0]||(e[0]=e=>t.$router.go(-1)),style:{width:"100%"}}),(0,s.Wm)(r,{ref:"table",list:n.list,option:{"账号":"Name","密码":"Password","角色":"role"},choose:2},null,8,["list"]),(0,s.Wm)(u,{list:["添加","删除"],onChoose:e[1]||(e[1]=t=>a.chooseFloatBtn(t))}),(0,s.Wm)(m,{show:n.dialogShow,"onUpdate:show":e[5]||(e[5]=t=>n.dialogShow=t),title:"添加","before-close":a.dialogClose,"show-cancel-button":""},{default:(0,s.w5)((()=>[n.dialogShow?((0,s.wg)(),(0,s.j4)(p,{key:0,onSubmit:t.onSubmit,ref:"from"},{default:(0,s.w5)((()=>[(0,s.Wm)(c,{modelValue:n.from.Name,"onUpdate:modelValue":e[2]||(e[2]=t=>n.from.Name=t),name:"Name",label:"账号",rules:[{required:!0,message:"请填写账号"}]},null,8,["modelValue"]),(0,s.Wm)(c,{modelValue:n.from.Password,"onUpdate:modelValue":e[3]||(e[3]=t=>n.from.Password=t),name:"Password",label:"密码",rules:[{required:!0,message:"请填写密码"}]},null,8,["modelValue"]),(0,s.Wm)(d,{modelValue:n.from.Security,"onUpdate:modelValue":e[4]||(e[4]=t=>n.from.Security=t),name:"Security",label:"角色",onInput:a.test,rules:[{required:!0,message:"请选择角色"}],inputType:1,list:n.role,listPosition:"top",search:!1},null,8,["modelValue","onInput","list"])])),_:1},8,["onSubmit"])):(0,s.kq)("",!0)])),_:1},8,["show","before-close"])])}var n=o(97),a=o(9959),h=o(8107),r={components:{Table:n.Z,FloatBtn:a.Z,InputSelect:h.Z},data(){return{list:[],role:["程序员","运维","管理员"],dialogShow:!1,from:{Name:"",Password:"",Security:""}}},methods:{test(t){this.from.Security=t},getManagers(){this.$IMBSProxy.GetManagers((t=>{for(const e in t)t[e].role=this.role[t[e].Security];this.list=t}))},chooseFloatBtn(t){switch(t){case 0:this.from=JSON.parse(JSON.stringify(this.$options.data().from)),this.dialogShow=!0;break;case 1:console.log(this.$refs.table.selected),this.$refs.table.verifyChoose().then((()=>{this.$Dialog.confirm({message:"是否确定删除"}).then((()=>{let t=this.$refs.table.chooseList.length;for(let e=0;e<t;e++)this.$IMBSProxy.DeleteManager(this.$refs.table.selected[e].Name,(e=>{t--,0==t&&(this.$Toast("删除成功"),this.getManagers())}))})).catch((()=>{}))})).catch((()=>{}));break;default:break}},dialogClose(t){return"confirm"!=t||new Promise((t=>{this.$IMBSProxy.NewManager({Name:this.from.Name,Password:this.from.Password,Security:-1!=this.role.indexOf(this.from.Security)?this.role.indexOf(this.from.Security):0},(t=>{t&&(this.$Toast("添加成功"),this.dialogShow=!1,this.getManagers(),this.$refs.table.clearChoose())})).then((()=>t(!0))).catch((()=>t(!1)))}))}},mounted(){this.getManagers()}},u=o(89);const c=(0,u.Z)(r,[["render",l],["__scopeId","data-v-7594bd71"]]);var d=c}}]);