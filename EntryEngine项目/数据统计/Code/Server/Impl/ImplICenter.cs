using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine;
using EntryEngine.Network;

namespace Server.Impl
{
    class ImplICenter : ImplUserBase<T_CENTER_USER>, _ICenter
    {
        /// <summary>需要更新的T_PLAYER的数据字段</summary>
        public HashSet<ET_CENTER_USER> Updates = new HashSet<ET_CENTER_USER>();
        /// <summary>需要更新的T_USER的字段，调用Save时统一保存</summary>
        public void Update(params ET_CENTER_USER[] updates)
        {
            foreach (var item in updates)
                Updates.Add(item);
        }
        protected override void OnSave(StringBuilder builder, List<object> objs)
        {
            if (Updates.Count > 0)
            {
                _DB._T_CENTER_USER.GetUpdateSQL(User, null, builder, objs, Updates.ToArray());
                Updates.Clear();
            }
        }
        public override void CheckToken(string token)
        {
            base.CheckToken(token);
            "账号已被冻结".Check(!User.State);
        }

        void _ICenter.GetUserInfo(CBICenter_GetUserInfo callback)
        {
            // 同步部分数据
            callback.Callback(User);
        }

        private void CheckGameName(string gameName)
        {
            "必须先选择游戏".Check(string.IsNullOrWhiteSpace(gameName));
            "没有查看此游戏的权限".Check(User.Type == EAccountType.普通账号 && gameName != "全部" && gameName != "暂无游戏" && (User.ManagerGame == null || !User.ManagerGame.Contains(gameName)));
        }
        private void Check(string gameName)
        {
            CheckGameName(gameName);
            "缓存正在刷新，请稍后查询".Check(_Cache.IsRefreshing);
        }
        private List<string> GetGameName()
        {
            var result = new List<string>();
            if (User.Type == EAccountType.平台账号)
                result.AddRange(_Cache.GameNames);
            else
                result.AddRange(User.ManagerGame != null ? User.ManagerGame : new string[0]);
            return result;
        }
        void CheckAccountType()
        {
            "权限不足，非平台账号".Check(User.Type != EAccountType.平台账号);
        }


        void _ICenter.ChangePassword(string oldPassword, string newPassword, CBICenter_ChangePassword callback)
        {
            "原密码错误".Check(oldPassword != User.__Password &&
                // 都为空可以
                !(string.IsNullOrEmpty(User.__Password) && string.IsNullOrEmpty(oldPassword)));
            "新密码不能为空".Check(string.IsNullOrEmpty(newPassword));
            User.Password = newPassword;
            User.__Password = newPassword;
            Update(ET_CENTER_USER.Password);
            OP("修改密码", null, 0, 0, string.Format("{0} -> {1}", oldPassword, newPassword));
            Save();
            User.Password = null;
            callback.Callback(true);
        }

        void _ICenter.GetGameName(CBICenter_GetGameName callback)
        {
            var result = new List<string>();
            result.Add("全部");
            result.AddRange(GetGameName());
            callback.Callback(result);
        }
        void _ICenter.GetChannel(string gameName, CBICenter_GetChannel callback)
        {
            List<string> result = new List<string>();
            CheckGameName(gameName);
            result.Add("全部");
            HashSet<string> channels;
            if (_Cache.ChannelByGameName.TryGetValue(gameName, out channels))
                result.AddRange(channels);
            callback.Callback(result);
        }
        void _ICenter.GetAnalysisGame(CBICenter_GetAnalysisGame callback)
        {
            List<string> result = GetGameName();
            if (result.Count == 0)
                result.Add("暂无游戏");
            callback.Callback(result);
        }


        void _ICenter.GetAnalysisLabel(string gameName, CBICenter_GetAnalysisLabel callback)
        {
            List<string> result = new List<string>();
            CheckGameName(gameName);
            HashSet<string> labels;
            if (_Cache.LabelByGameName.TryGetValue(gameName, out labels))
            {
                result.AddRange(labels);
                for (int i = 0; i < result.Count; i++)
                    for (int j = 0; j < _Cache.Analysis.Count; j++)
                        if (_Cache.Analysis[j].Label == result[i] && _Cache.Analysis[j].Count != 0)
                        {
                            result[i] += "（次）";
                            break;
                        }
                for (int i = 0; i < result.Count; i++)
                    if (!result[i].Contains("（次）"))
                        result[i] += "（人）";
            }
            else
                result.Add("全部");
            callback.Callback(result);
        }
        void _ICenter.GetAnalysis(string gameName, string channel, string label, DateTime startTime, DateTime endTime, CBICenter_GetAnalysis callback)
        {
            if (User.Type == EAccountType.普通账号 && User.ManagerGame.IsEmpty())
            {
                callback.Callback(new List<RetAnalysis>());
                return;
            }
            Check(gameName);
            if (string.IsNullOrWhiteSpace(label))
                label = "全部";
            if (label.EndsWith("（次）") || label.EndsWith("（人）"))
                label = label.Substring(0, label.Length - 3);

            endTime = endTime.HandleEndTime().Date;

            List<RetAnalysis> result = new List<RetAnalysis>();
            for (int i = 0; i < _Cache.Analysis.Count; i++)
            {
                var analysis = _Cache.Analysis[i];
                if ((gameName == "全部" || analysis.Account.GameName == gameName) && (channel == "全部" || analysis.Account.Channel == channel) &&
                    analysis.Label == label && analysis.Time >= startTime && analysis.Time < endTime)
                {
                    var flag = true;
                    for (int j = 0; j < result.Count; j++)
                    {
                        if (analysis.Name == result[j].Name)
                        {
                            if (!result[j].CanRepeat)
                                result[j].Count++;
                            else
                                result[j].Count += analysis.Count;
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                        result.Add(new RetAnalysis() { Name = analysis.Name, Count = (analysis.Count == 0 ? 1 : analysis.Count), OrderID = analysis.OrderID, CanRepeat = (analysis.Count != 0) });
                }
            }
            bool order = false;
            for (int i = 0; i < result.Count; i++)
            {
                if (result[i].OrderID != 0)
                {
                    order = true;
                    break;
                }
            }
            if (order)
                result = result.OrderBy(item => item.OrderID).ToList();
            else
                result = result.OrderBy(item => item.Name).ToList();
            callback.Callback(result);
        }


        private PagedModel<RetRetained> GetGetRetainedBase(int page, string gameName, string channel, DateTime startTime, DateTime endTime)
        {
            int registCount;
            int activeCount = 0;
            int retainedCount = RetRetained.Retained.Length;

            PagedModel<RetRetained> result = new PagedModel<RetRetained>();
            result.Page = page;
            result.PageSize = 15;
            result.Models = new List<RetRetained>();

            if (startTime.Ticks == 0)
                startTime = _Cache.startTime;
            endTime = endTime.HandleEndTime().Date;
            for (int i = (int)(endTime - startTime).TotalDays - 1; i >= 0; i--)
            {
                int[] LoginCount = new int[retainedCount];
                DateTime date = startTime.Date.AddDays(i);
                List<_Cache.C_Register> list;
                HashSet<int> registerID = new HashSet<int>();
                if (_Cache.RegisterByDate.TryGetValue(date, out list))
                {
                    for (int j = 0; j < list.Count; j++)
                    {
                        var register = list[j];
                        if (register.GameName == gameName && (channel == "全部" || register.Channel == channel))
                            registerID.Add(register.ID);
                    }
                }

                registCount = registerID.Count;

                var loginAccountID = new HashSet<int>();
                List<_Cache.C_Login> login;
                if (_Cache.LoginByDate.TryGetValue(date, out login))
                    for (int j = 0; j < login.Count; j++)
                        if (login[j].Account.GameName == gameName && (channel == "全部" || login[j].Account.Channel == channel))
                            loginAccountID.Add(login[j].Account.ID);

                DateTime today = DateTime.Today;
                for (int j = 0; j < retainedCount; j++)
                {
                    DateTime retainedDate = date.AddDays(RetRetained.Retained[j]);
                    if (retainedDate > today)
                    {
                        LoginCount[j] = -1;
                        continue;
                    }
                    HashSet<int> LoginID = new HashSet<int>();
                    if (_Cache.LoginByDate.TryGetValue(retainedDate, out login))
                        for (int k = 0; k < login.Count; k++)
                            if (registerID.Contains(login[k].Account.ID) && LoginID.Add(login[k].Account.ID))
                                LoginCount[j]++;
                }

                RetRetained item = new RetRetained()
                {
                    Time = date,
                    GameName = gameName,
                    ActiveCount = activeCount,
                    RegistCount = registCount,
                    LoginCount = LoginCount,
                    loginAccountID = loginAccountID
                };
                result.Models.Add(item);
            }
            if (page != -1)
            {
                result.Count = result.Models.Count;
                result.Models = result.Models.Skip(result.Page * result.PageSize).Take(result.PageSize).ToList();
            }
            return result;
        }
        void _ICenter.GetRetained(int page, string gameName, string channel, DateTime startTime, DateTime endTime, CBICenter_GetRetained callback)
        {
            Check(gameName);
            List<string> games = new List<string>();
            if (gameName == "全部")
                games.AddRange(GetGameName());
            else
                games.Add(gameName);
            PagedModel<RetRetained> result = new PagedModel<RetRetained>();
            result.Page = page;
            result.PageSize = 15;
            result.Models = new List<RetRetained>();

            // 大于day的统计天数不显示
            foreach (string game in games)
            {
                List<RetRetained> temp = GetGetRetainedBase(-1, game, channel, startTime, endTime).Models;
                RetRetained resultItem = new RetRetained();
                resultItem.GameName = game;
                resultItem.loginAccountID = new HashSet<int>();
                foreach (RetRetained item in temp)
                {
                    foreach (var id in item.loginAccountID)
                        resultItem.loginAccountID.Add(id);
                    resultItem.RegistCount += item.RegistCount;
                    int loginCount = item.LoginCount.Length;
                    if (resultItem.LoginCount == null) resultItem.LoginCount = new int[loginCount];
                    for (int i = 0; i < loginCount; i++)
                        if (item.LoginCount[i] != -1)
                            resultItem.LoginCount[i] += item.LoginCount[i];
                }
                result.Models.Add(resultItem);
            }
            result.Count = result.Models.Count;
            result.Models = result.Models.Skip(result.Page * result.PageSize).Take(result.PageSize).ToList();
            foreach (var item in result.Models)
                item.Calc();
            callback.Callback(result);
        }
        void _ICenter.GetRetained2(int page, string gameName, string channel, DateTime startTime, DateTime endTime, CBICenter_GetRetained2 callback)
        {
            Check(gameName);
            "必须先选择游戏".Check(gameName == "全部");
            var result = GetGetRetainedBase(page, gameName, channel, startTime, endTime);
            foreach (var item in result.Models)
            {
                item.Calc();
            }
            callback.Callback(result);
        }


        void _ICenter.OnlineCount(string gameName, string channel, RetOnlineUnit unit, DateTime startTime, DateTime endTime, CBICenter_OnlineCount callback)
        {
            RetOnline result = new RetOnline();
            if (User.Type == EAccountType.普通账号 && User.ManagerGame.IsEmpty())
            {
                callback.Callback(result);
                return;
            }
            Check(gameName);

            var games = new HashSet<string>();
            if (gameName == "全部")
                foreach (var game in GetGameName())
                    games.Add(game);
            else
                games.Add(gameName);

            if (startTime.Ticks == 0)
                startTime = _Cache.startTime;
            startTime = startTime.Date;
            endTime = endTime.HandleEndTime().Date;

            result.unit = unit;
            // 计算在线数据
            List<_Cache.C_Login> logins;
            List<RetOnlineItem> onlineItems = new List<RetOnlineItem>();
            for (int i = 0, days = (int)(endTime - startTime).TotalDays; i < days; i++)
            {
                if (_Cache.LoginByDate.TryGetValue(startTime.AddDays(i), out logins))
                {
                    for (int j = 0; j < logins.Count; j++)
                    {
                        _Cache.C_Login login = logins[j];
                        if (games.Contains(login.Account.GameName) && (channel == "全部" || login.Account.Channel == channel) &&
                            ((login.LoginTime >= startTime && login.LoginTime < endTime) || (login.LastOnlineTime >= startTime && login.LastOnlineTime < endTime)))
                        {
                            DateTime tempTime = login.LoginTime;
                            while (true)
                            {
                                onlineItems.Add(new RetOnlineItem()
                                {
                                    Time = tempTime,
                                    Count = login.Account.ID
                                });
                                tempTime = tempTime.AddMinutes(15);
                                if (tempTime >= login.LastOnlineTime)
                                {
                                    onlineItems.Add(new RetOnlineItem()
                                    {
                                        Time = login.LastOnlineTime,
                                        Count = login.Account.ID
                                    });
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            result.Calc(onlineItems.OrderBy(i => i.Time).ToList(), startTime, endTime);

            if (_Cache.LoginByDate.TryGetValue(DateTime.Today, out logins))
            {
                HashSet<int> accountID = new HashSet<int>();
                var nowQuarter = T_Online.GetQuarter(result.NowTime);
                for (int i = 0; i < logins.Count; i++)
                {
                    _Cache.C_Login login = logins[i];
                    if (T_Online.GetQuarter(login.LastOnlineTime) != nowQuarter) continue;
                    if (games.Contains(login.Account.GameName) && (channel == "全部" || login.Account.Channel == channel))
                        accountID.Add(login.Account.ID);
                }
                result.NowCount = accountID.Count;
            }
            callback.Callback(result);
        }


        void _ICenter.GameTime(string gameName, string channel, DateTime startTime, DateTime endTime, CBICenter_GameTime callback)
        {
            RetGameTime result = new RetGameTime();
            if (User.Type == EAccountType.普通账号 && User.ManagerGame.IsEmpty())
            {
                callback.Callback(result);
                return;
            }
            Check(gameName);

            var games = new HashSet<string>();
            if (gameName == "全部")
                foreach (var game in GetGameName())
                    games.Add(game);
            else
                games.Add(gameName);

            if (startTime.Ticks == 0)
                startTime = _Cache.startTime;
            startTime = startTime.Date;
            endTime = endTime.HandleEndTime().Date;

            for (int i = 0, days = (int)(endTime - startTime).TotalDays; i < days; i++)
            {
                List<_Cache.C_Login> logins;
                if (!_Cache.LoginByDate.TryGetValue(startTime.AddDays(i), out logins)) continue;
                for (int j = 0; j < logins.Count; j++)
                {
                    _Cache.C_Login login = logins[j];
                    if (games.Contains(login.Account.GameName) && (channel == "全部" || channel == login.Account.Channel))
                    {
                        int time = (int)(login.LastOnlineTime - login.LoginTime).TotalMinutes;
                        if (time == 0) continue;
                        RetGameTimeItem item = null;
                        foreach (RetGameTimeItem temp in result.GameTimes)
                        {
                            if (temp.Time == time)
                            {
                                item = temp;
                                break;
                            }
                        }
                        if (item == null)
                        {
                            item = new RetGameTimeItem() { Time = time, Count = 0 };
                            result.GameTimes.Add(item);
                        }
                        item.Count++;
                    }
                }
            }

            result.GameTimes = result.GameTimes.OrderBy(i => i.Time).ToList();
            result.Calc();
            callback.Callback(result);
        }


        private PagedModel<GameDataItem> GetGameData(int page, string gameName, string channel, DateTime startTime, DateTime endTime)
        {
            endTime = endTime.HandleEndTime();
            DateTime startDate = startTime.Date;
            DateTime endDate = endTime.Date;
            int days = (int)(endDate - startDate).TotalDays;

            PagedModel<GameDataItem> result = new PagedModel<GameDataItem>();
            result.Page = page;
            result.PageSize = 15;
            result.Models = new List<GameDataItem>();
            HashSet<int> loginUser = new HashSet<int>();
            for (int i = 0; i < days; i++)
            {
                GameDataItem item = null;
                DateTime date = startDate.AddDays(i);
                List<_Cache.C_Login> login;
                if (_Cache.LoginByDate.TryGetValue(date, out login))
                {
                    foreach (var loginItem in login)
                    {
                        if (loginItem.Account.GameName == gameName && (channel == "全部" || loginItem.Account.Channel == channel))
                        {
                            foreach (var resultItem in result.Models)
                            {
                                if (resultItem.GameName == gameName && resultItem.Channel == loginItem.Account.Channel)
                                {
                                    item = resultItem;
                                    break;
                                }
                            }

                            if (item == null)
                            {
                                item = new GameDataItem();
                                item.GameName = gameName;
                                item.Channel = loginItem.Account.Channel;
                                item.LoginID = new HashSet<int>();
                                result.Models.Add(item);
                            }
                            item.LoginID.Add(loginItem.ID);
                            if (loginUser.Add(loginItem.Account.ID))
                                item.ActivityCount++;
                            item.AvgOnlineTime += Math.Ceiling((loginItem.LastOnlineTime - loginItem.LoginTime).TotalSeconds);
                        }
                    }
                }

                List<_Cache.C_Register> regist;
                if (_Cache.RegisterByDate.TryGetValue(date, out regist))
                {
                    foreach (var registItem in regist)
                    {
                        if (registItem.GameName == gameName && (channel == "全部" || registItem.Channel == channel))
                        {
                            foreach (var resultItem in result.Models)
                            {
                                if (resultItem.GameName == gameName && resultItem.Channel == registItem.Channel)
                                {
                                    item = resultItem;
                                    break;
                                }
                            }

                            if (item == null)
                            {
                                item = new GameDataItem();
                                item.GameName = gameName;
                                item.Channel = registItem.Channel;
                                result.Models.Add(item);
                            }

                            item.RegistCount++;
                        }
                    }
                }
            }
            if (page != -1)
            {
                result.Count = result.Models.Count;
                result.Models = result.Models.Skip(result.Page * result.PageSize).Take(result.PageSize).ToList();
            }
            return result;
        }
        void _ICenter.GetGameData(int page, string gameName, string channel, DateTime startTime, DateTime endTime, CBICenter_GetGameData callback)
        {
            CheckAccountType();
            Check(gameName);
            List<string> games = new List<string>();
            if (gameName == "全部")
                games.AddRange(GetGameName());
            else
                games.Add(gameName);

            var result = new RetGameData();
            result.Items.Models = new List<GameDataItem>();
            result.Items.Page = page;
            result.Items.PageSize = 15;
            result.Total.LoginID = new HashSet<int>();
            foreach (string game in games)
            {
                var temp = GetGameData(-1, game, channel, startTime, endTime).Models;
                var resultItem = new GameDataItem();
                resultItem.GameName = game;
                resultItem.LoginID = new HashSet<int>();
                if (temp.Count > 0)
                    resultItem.Channel = temp.First().Channel;
                foreach (var item in temp)
                {
                    resultItem.ActivityCount += item.ActivityCount;
                    resultItem.RegistCount += item.RegistCount;
                    resultItem.AvgOnlineTime += item.AvgOnlineTime;
                    foreach (int id in item.LoginID)
                        resultItem.LoginID.Add(id);

                    result.Total.ActivityCount += item.ActivityCount;
                    result.Total.RegistCount += item.RegistCount;
                    result.Total.AvgOnlineTime += item.AvgOnlineTime;
                    foreach (int id in item.LoginID)
                        result.Total.LoginID.Add(id);
                }
                result.Items.Models.Add(resultItem);
            }

            result.Items.Count = result.Items.Models.Count;
            result.Items.Models = result.Items.Models.Skip(result.Items.Page * result.Items.PageSize).Take(result.Items.PageSize).ToList();

            foreach (var item in result.Items.Models)
                item.Calc();
            result.Total.Calc();

            callback.Callback(result);
        }
        void _ICenter.GetGameData2(int page, string gameName, string channel, DateTime startTime, DateTime endTime, CBICenter_GetGameData2 callback)
        {
            CheckAccountType();
            Check(gameName);
            "必须先选择游戏".Check(gameName == "全部");
            var result = GetGameData(page, gameName, channel, startTime, endTime);
            foreach (var item in result.Models)
                item.Calc();
            callback.Callback(result);
        }


        ImplICenter CreateAccount(int id)
        {
            var temp = new ImplICenter();
            temp.InitializeByID(id);
            "账号不存在".Check(temp.User == null);
            return temp;
        }
        void _ICenter.GetAccountList(int page, string account, CBICenter_GetAccountList callback)
        {
            CheckAccountType();
            PagedModel<RetAccount> result = _DB._DAO.SelectPaged<RetAccount>(
@"SELECT ID, Account,
    Name as Nickname, Type,
    ManagerGame, State
FROM T_CENTER_USER
WHERE (@p0 IS NULL OR Account LIKE @p0)", page, 15, account.Like());
            if (result.Models != null)
                foreach (var item in result.Models)
                    item.Calc();
            callback.Callback(result);
        }
        void _ICenter.BindGame(int identityID, string[] gameNames, CBICenter_BindGame callback)
        {
            CheckAccountType();
            var temp = CreateAccount(identityID);
            temp.User.ManagerGame = gameNames;
            temp.Update(ET_CENTER_USER.ManagerGame);
            temp.Save();
            callback.Callback(true);
        }
        void _ICenter.ModifyAccount(int identityID, string account, string password, EAccountType type, string nickName, CBICenter_ModifyAccount callback)
        {
            CheckAccountType();
            "账号已存在".Check(_DB._T_CENTER_USER.Exists2("WHERE ID <> @p0 AND Account=@p1", identityID, account));
            var identity = new T_CENTER_USER()
            {
                ID = identityID,
                Account = account,
                Password = password,
                Type = type,
                Name = nickName,
                RegisterTime = DateTime.Now
            };
            if (identityID != 0)
            {
                if (!string.IsNullOrWhiteSpace(password))
                    Update(ET_CENTER_USER.Password);
                Update(ET_CENTER_USER.Type, ET_CENTER_USER.Name);
            }
            else
                _DB._T_CENTER_USER.Insert(identity);
            callback.Callback(true);
        }
        void _ICenter.DeleteAccount(int identityID, CBICenter_DeleteAccount callback)
        {
            CheckAccountType();
            "不能删除当前登录的账号".Check(identityID == User.ID);
            var temp = CreateAccount(identityID);
            ClearUserCache(temp.User);
            callback.Callback(_DB._T_CENTER_USER.Delete(identityID) > 0);
        }
        void _ICenter.ChangeAccountState(int identityID, CBICenter_ChangeAccountState callback)
        {
            CheckAccountType();
            "不能封禁当前登录的账号".Check(identityID == User.ID);
            var temp = CreateAccount(identityID);
            temp.User.State = !temp.User.State;
            temp.Update(ET_CENTER_USER.State);
            temp.Save();
            callback.Callback(true);
        }
    }
}
