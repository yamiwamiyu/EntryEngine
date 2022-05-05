using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine.Network;

/// <summary>后台管理系统需要登录针对某个用户的服务接口</summary>
[ProtocolStub(2, null)]
public interface ICenter
{
    void GetUserInfo(Action<T_CENTER_USER> callback);
    /// <summary>修改个人密码</summary>
    /// <param name="oldPassword">旧密码</param>
    /// <param name="newPassword">新密码</param>
    void ChangePassword(string oldPassword, string newPassword, Action<bool> callback);

    /// <summary>获取游戏名</summary>
    void GetGameName(Action<List<string>> callback);
    /// <summary>获取某一游戏的所有渠道</summary>
    /// <param name="gameName">游戏名</param>
    void GetChannel(string gameName, Action<List<string>> callback);
    /// <summary>获取不包含“全部”的游戏名</summary>
    void GetAnalysisGame(Action<List<string>> callback);

    ///// <summary>上传文件</summary>
    ///// <param name="file">文件</param>
    //void UploadFile(FileUpload file, Action<string> callback);

    //#region 基础分析
    ///// <summary>获取基础分析标签</summary>
    ///// <param name="gameName">游戏名</param>
    //void GetAnalysisLabel(string gameName, Action<List<string>> callback);
    ///// <summary>获取基础分析数据</summary>
    ///// <param name="gameName">游戏名</param>
    ///// <param name="channel">渠道</param>
    ///// <param name="label">分析标签</param>
    ///// <param name="startTime">开始时间</param>
    ///// <param name="endTime">结束时间</param>
    //void GetAnalysis(string gameName, string channel, string label, DateTime startTime, DateTime endTime, Action<List<RetAnalysis>> callback);
    //#endregion

    //#region 留存分析
    ///// <summary>第一层留存分析</summary>
    ///// <param name="page">分页号</param>
    ///// <param name="gameName">游戏名</param>
    ///// <param name="channel">渠道</param>
    ///// <param name="startTime">开始时间</param>
    ///// <param name="endTime">结束时间</param>
    //void GetRetained(int page, string gameName, string channel, DateTime startTime, DateTime endTime, Action<PagedModel<RetRetained>> callback);
    ///// <summary>第二层留存分析</summary>
    ///// <param name="page">分页号</param>
    ///// <param name="gameName">游戏名</param>
    ///// <param name="channel">渠道</param>
    ///// <param name="startTime">开始时间</param>
    ///// <param name="endTime">结束时间</param>
    //void GetRetained2(int page, string gameName, string channel, DateTime startTime, DateTime endTime, Action<PagedModel<RetRetained>> callback);
    //#endregion

    //#region 在线人数
    ///// <summary>获取在线人数</summary>
    ///// <param name="gameName">游戏名</param>
    ///// <param name="channel">渠道</param>
    ///// <param name="unit">单位</param>
    ///// <param name="startTime">开始时间</param>
    ///// <param name="endTime">结束时间</param>
    //void OnlineCount(string gameName, string channel, RetOnlineUnit unit, DateTime startTime, DateTime endTime, Action<RetOnline> callback);
    //#endregion

    //#region 游戏时长
    ///// <summary>获取游戏时长</summary>
    ///// <param name="gameName">游戏名</param>
    ///// <param name="channel">渠道</param>
    ///// <param name="startTime">起始时间</param>
    ///// <param name="endTime">结束时间</param>
    //void GameTime(string gameName, string channel, DateTime startTime, DateTime endTime, Action<RetGameTime> callback);
    //#endregion

    //#region 游戏统计
    ///// <summary>第一层游戏统计</summary>
    ///// <param name="page">分页号</param>
    ///// <param name="gameName">游戏名</param>
    ///// <param name="channel">渠道</param>
    ///// <param name="startTime">开始时间</param>
    ///// <param name="endTime">结束时间</param>
    //void GetGameData(int page, string gameName, string channel, DateTime startTime, DateTime endTime, Action<RetGameData> callback);
    ///// <summary>第二层游戏统计</summary>
    ///// <param name="page">分页号</param>
    ///// <param name="gameName">游戏名</param>
    ///// <param name="channel">渠道</param>
    ///// <param name="startTime">开始时间</param>
    ///// <param name="endTime">结束时间</param>
    //void GetGameData2(int page, string gameName, string channel, DateTime startTime, DateTime endTime, Action<PagedModel<GameDataItem>> callback);
    //#endregion

    //#region 系统设置
    ///// <summary>获取平台账号列表</summary>
    ///// <param name="page">分页号</param>
    ///// <param name="account">账号筛选</param>
    //void GetAccountList(int page, string account, Action<PagedModel<RetAccount>> callback);
    ///// <summary>绑定游戏</summary>
    ///// <param name="identityID">绑定的账号ID</param>
    ///// <param name="gameNames">绑定的游戏名数组</param>
    //void BindGame(int identityID, string[] gameNames, Action<bool> callback);
    ///// <summary>修改平台账号信息</summary>
    ///// <param name="identityID">账号ID，为0添加</param>
    ///// <param name="account">账号</param>
    ///// <param name="password">密码</param>
    ///// <param name="type">账号类型</param>
    ///// <param name="nickName">昵称</param>
    //void ModifyAccount(int identityID, string account, string password, AccountType type, string nickName, Action<bool> callback);
    ///// <summary>删除平台账号</summary>
    ///// <param name="identityID">账号ID</param>
    //void DeleteAccount(int identityID, Action<bool> callback);
    ///// <summary>改变账号状态</summary>
    ///// <param name="identityID">账号ID</param>
    //void ChangeAccountState(int identityID, Action<bool> callback);
    //#endregion
}
