<template>
  <el-upload
    list-type="picture-card"
    :class="{'hide-upload': hide}"
    :action="url"
    :headers="headersUpload"
    :file-list="fileList"
    :on-change="handleChange"
    :on-remove="handleRemove"
    :on-success="success"
    :data="parames"
    :disabled="isDisabled"
    :before-upload="onProgress"
    accept="image/*"
  >
    <i class="el-icon-plus"></i>
  </el-upload>
</template>

<script>
/**
  这是一个文件上传公共组件，仅支持单文件上传
*/
export default {
  name: "UploadFile",
  props: {
    /** 初始显示 */
    init: {
      type: String,
      default: ""
    },
    /** 上传api */
    uploadUrl: {
      type: String,
      default: "UploadFile"
    },
    /** 提交参数 */
    parames: {
      type: Object,
      default: null
    },
    /** 显示列表类型 */
    showList: {
      type: String,
      default: "picture-card"
    },
    /** 是否可删除 */
    isDisabled: false,
  },
  data() {
    return {
      hide: true, // 隐藏
      headersUpload: {
        AccessToken: localStorage.getItem("token")
      },
      url: `${this.$IP5.url}5/${this.uploadUrl}`,
      // url: `https://jsonplaceholder.typicode.com/posts/`,
      fileList: []
    };
  },
  methods: {
    handleChange(file) {
      console.log("[UploadFile->Change]", file);
    console.log(this.url);
      this.hide = file.response != "";
    },
    handleRemove(file) {
      console.log("[UploadFile->Remove]", this.$props);
      this.hide = false;
      this.uploadBack("");
      // this.$emit("get-romve", file);
    },
    // 上传成功
    success(response) {
      this.uploadBack(response);
    },
    // 返回上传结果
    uploadBack(result) {
      if (result.errMsg) {
        this.$message.error(result.errMsg);
        return;
      }
      this.$emit("get-result", result);
    },
    // 上传之前限制图片
    onProgress(file) {
      this.$emit("get-onProgress", file);
    },
    // 监听初始化数据
    initFun(){
      if (this.init && !this.init.includes("default.png")) {
      this.fileList.push({
        name: "",
        url: this.init
      });
      this.hide = true;
    } else {
      this.hide = false;
    }
    }
  },
  mounted() {
    this.initFun()
    console.log("[uploadUrl]", this.init, this.fileList);
  },
  watch:{
    init: function(){
      this.initFun()
    }
  }
};
</script>

<style lang="scss" scoped>
</style>
