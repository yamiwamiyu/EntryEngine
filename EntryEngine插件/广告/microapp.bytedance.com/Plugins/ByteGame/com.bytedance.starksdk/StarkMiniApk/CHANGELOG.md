# Ci Buildlog
[Jenkins activity ↗](http://10.94.85.240:8080/blue/organizations/jenkins/com-bytedance-starkminiapk-unity/activity)

# Stark MiniApk 引导包 for Unity Changelog

## [1.0.36] - 2020-10-20
- 调整Editor菜单名称：MiniApk引导包

## [1.0.35] - 2020-10-20
- 适配StarkMini 1.1.5 dll的接口

## [1.0.32] - 2020-10-10
- 更新Gradle插件版本，打包服务迁移到打包机上。

## [1.0.31] - 2020-10-10
- 修正小包已安装但版本不匹配时，CanUseAdAPI判断为false的问题。

## [1.0.30] - 2020-10-10
- 移除apk中androidx的引入。

## [1.0.29] - 2020-09-30
- Gradle script 增加参数：UC 小游戏 版本类型 `GAME_VER_TYPE`
- MiniApkTool 工具增加新参数: `UcGameVerType`，增加：Revert 按钮、是否有修改提示、支持Ctrl S快捷键。
- UnionAd `InstallApp`接口，如果已安装小包、且最新版本匹配，自动Toast提示"已添加桌面入口"。

## [1.0.28] - 2020-09-30
- 微调注释

## [1.0.27] - 2020-09-29
- 增加：封装的穿山甲广告类：`class UnionAdAppManager`

## [1.0.25] - 2020-09-29
- 调整：Runtime 命名: `namespace StarkMiniApk`, `class MiniApkApi`
- 调整：简化 `MiniApkApi.Init` 参数.

## [1.0.19] - 2020-09-29
- 新增：Editor设置 MiniApkSetting 工具
    - 菜单项：***ByteGame/StarkMiniApk/Open MiniApkTool***
- 新增：运行时读取 MiniApkSetting 接口
    - `MiniApkSetting.InitLoad()`
    - `MiniApkSetting.Get()`

