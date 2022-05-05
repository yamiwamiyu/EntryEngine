using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine.Network;
using EntryEngine;

namespace Server.Impl
{
    public static class _Cache
    {
        public class C_Register
        {
            /// <summary>注册的ID</summary>
            public int ID;
            /// <summary>游戏名</summary>
            public string GameName;
            /// <summary>设备ID</summary>
            public string DeviceID;
            /// <summary>渠道号</summary>
            public string Channel;
            /// <summary>注册时间</summary>
            public DateTime RegisterTime;

            public static C_Register change(T_Register register)
            {
                return new C_Register()
                {
                    ID = register.ID,
                    GameName = register.GameName,
                    DeviceID = register.DeviceID,
                    Channel = register.Channel,
                    RegisterTime = register.RegisterTime
                };
            }
        }

        public class C_Login
        {
            /// <summary>登录ID</summary>
            public int ID;
            /// <summary>账号</summary>
            public C_Register Account;
            /// <summary>登录时间</summary>
            public DateTime LoginTime;
            /// <summary>最后在线时间</summary>
            public DateTime LastOnlineTime;

            public static C_Login change(T_Login login)
            {
                var result = new C_Login()
                {
                    ID = login.ID,
                    LoginTime = login.LoginTime,
                    LastOnlineTime = login.LastOnlineTime
                };
                C_Register account;
                if (!RegisterByID.TryGetValue(login.RegisterID, out account)) return null;
                result.Account = account;
                return result;
            }
            public class C_Login2
            {
                /// <summary>登录ID</summary>
                public int ID;
                /// <summary>账号ID</summary>
                public int RegisterID;
                /// <summary>登录时间</summary>
                public DateTime LoginTime;
                /// <summary>最后在线时间</summary>
                public DateTime LastOnlineTime;

                public C_Login change()
                {
                    C_Login result = new C_Login()
                    {
                        ID = ID,
                        LoginTime = LoginTime,
                        LastOnlineTime = LastOnlineTime
                    };
                    C_Register account;
                    if (!RegisterByID.TryGetValue(RegisterID, out account)) return null;
                    result.Account = account;
                    return result;
                }
            }
        }

        public class C_Analysis
        {
            /// <summary>注册ID</summary>
            public C_Register Account;
            /// <summary>事件页签</summary>
            public string Label;
            /// <summary>事件名称</summary>
            public string Name;
            /// <summary>事件排序，页签内排序</summary>
            public int OrderID;
            /// <summary>事件发生次数</summary>
            public int Count;
            /// <summary>事件发生时间</summary>
            public DateTime Time;

            public static C_Analysis change(T_Analysis analysis)
            {
                C_Analysis result = new C_Analysis()
                {
                    Label = analysis.Label,
                    Name = analysis.Name,
                    OrderID = analysis.OrderID,
                    Count = analysis.Count,
                    Time = analysis.Time
                };
                C_Register account;
                if (!RegisterByID.TryGetValue(analysis.RegisterID, out account)) return null;
                result.Account = account;
                return result;
            }

            public class C_Analysis2
            {
                /// <summary>注册ID</summary>
                public int RegisterID;
                /// <summary>事件页签</summary>
                public string Label;
                /// <summary>事件名称</summary>
                public string Name;
                /// <summary>事件排序，页签内排序</summary>
                public int OrderID;
                /// <summary>事件发生次数</summary>
                public int Count;
                /// <summary>事件发生时间</summary>
                public DateTime Time;

                public C_Analysis change()
                {
                    if (string.IsNullOrWhiteSpace(Label) || string.IsNullOrWhiteSpace(Name))
                        return null;
                    C_Analysis result = new C_Analysis()
                    {
                        Label = Label,
                        Name = Name,
                        OrderID = OrderID,
                        Count = Count,
                        Time = Time
                    };
                    C_Register account;
                    if (!RegisterByID.TryGetValue(RegisterID, out account)) return null;
                    result.Account = account;
                    return result;
                }
            }
        }

        public class C_Online : T_Online
        {
            public C_Online Clone()
            {
                return new C_Online()
                {
                    Time = Time,
                    GameName = GameName,
                    Channel = Channel,
                    Quarter0 = Quarter0,
                    Quarter1 = Quarter1,
                    Quarter2 = Quarter2,
                    Quarter3 = Quarter3,
                    Quarter4 = Quarter4,
                    Quarter5 = Quarter5,
                    Quarter6 = Quarter6,
                    Quarter7 = Quarter7,
                    Quarter8 = Quarter8,
                    Quarter9 = Quarter9,
                    Quarter10 = Quarter10,
                    Quarter11 = Quarter11,
                    Quarter12 = Quarter12,
                    Quarter13 = Quarter13,
                    Quarter14 = Quarter14,
                    Quarter15 = Quarter15,
                    Quarter16 = Quarter16,
                    Quarter17 = Quarter17,
                    Quarter18 = Quarter18,
                    Quarter19 = Quarter19,
                    Quarter20 = Quarter20,
                    Quarter21 = Quarter21,
                    Quarter22 = Quarter22,
                    Quarter23 = Quarter23,
                    Quarter24 = Quarter24,
                    Quarter25 = Quarter25,
                    Quarter26 = Quarter26,
                    Quarter27 = Quarter27,
                    Quarter28 = Quarter28,
                    Quarter29 = Quarter29,
                    Quarter30 = Quarter30,
                    Quarter31 = Quarter31,
                    Quarter32 = Quarter32,
                    Quarter33 = Quarter33,
                    Quarter34 = Quarter34,
                    Quarter35 = Quarter35,
                    Quarter36 = Quarter36,
                    Quarter37 = Quarter37,
                    Quarter38 = Quarter38,
                    Quarter39 = Quarter39,
                    Quarter40 = Quarter40,
                    Quarter41 = Quarter41,
                    Quarter42 = Quarter42,
                    Quarter43 = Quarter43,
                    Quarter44 = Quarter44,
                    Quarter45 = Quarter45,
                    Quarter46 = Quarter46,
                    Quarter47 = Quarter47,
                    Quarter48 = Quarter48,
                    Quarter49 = Quarter49,
                    Quarter50 = Quarter50,
                    Quarter51 = Quarter51,
                    Quarter52 = Quarter52,
                    Quarter53 = Quarter53,
                    Quarter54 = Quarter54,
                    Quarter55 = Quarter55,
                    Quarter56 = Quarter56,
                    Quarter57 = Quarter57,
                    Quarter58 = Quarter58,
                    Quarter59 = Quarter59,
                    Quarter60 = Quarter60,
                    Quarter61 = Quarter61,
                    Quarter62 = Quarter62,
                    Quarter63 = Quarter63,
                    Quarter64 = Quarter64,
                    Quarter65 = Quarter65,
                    Quarter66 = Quarter66,
                    Quarter67 = Quarter67,
                    Quarter68 = Quarter68,
                    Quarter69 = Quarter69,
                    Quarter70 = Quarter70,
                    Quarter71 = Quarter71,
                    Quarter72 = Quarter72,
                    Quarter73 = Quarter73,
                    Quarter74 = Quarter74,
                    Quarter75 = Quarter75,
                    Quarter76 = Quarter76,
                    Quarter77 = Quarter77,
                    Quarter78 = Quarter78,
                    Quarter79 = Quarter79,
                    Quarter80 = Quarter80,
                    Quarter81 = Quarter81,
                    Quarter82 = Quarter82,
                    Quarter83 = Quarter83,
                    Quarter84 = Quarter84,
                    Quarter85 = Quarter85,
                    Quarter86 = Quarter86,
                    Quarter87 = Quarter87,
                    Quarter88 = Quarter88,
                    Quarter89 = Quarter89,
                    Quarter90 = Quarter90,
                    Quarter91 = Quarter91,
                    Quarter92 = Quarter92,
                    Quarter93 = Quarter93,
                    Quarter94 = Quarter94,
                    Quarter95 = Quarter95
                };
            }
        }

        public static List<C_Register> Register = new List<C_Register>();
        public static Dictionary<DateTime, List<C_Register>> RegisterByDate = new Dictionary<DateTime, List<C_Register>>();
        public static Dictionary<int, C_Register> RegisterByID = new Dictionary<int, C_Register>();
        static DateTime LastRegisterTime = new DateTime(0);

        public static List<C_Login> Login = new List<C_Login>();
        public static Dictionary<DateTime, List<C_Login>> LoginByDate = new Dictionary<DateTime, List<C_Login>>();
        public static Dictionary<int, C_Login> LoginByID = new Dictionary<int, C_Login>();
        static int LastLoginID = 0;

        public static HashSet<string> AnalysisID = new HashSet<string>();
        public static List<C_Analysis> Analysis = new List<C_Analysis>();
        public static Dictionary<int, List<C_Analysis>> AnalysisByRegisterID = new Dictionary<int, List<C_Analysis>>();
        static DateTime LastAnalysisTime = new DateTime(0);

        public static List<C_Online> Online = new List<C_Online>();
        public static Dictionary<DateTime, List<C_Online>> OnlineByDate = new Dictionary<DateTime, List<C_Online>>();
        static DateTime LastOnlineTime = new DateTime(0);

        public static HashSet<string> GameNames = new HashSet<string>();
        public static Dictionary<string, HashSet<string>> LabelByGameName = new Dictionary<string, HashSet<string>>();
        public static Dictionary<string, HashSet<string>> ChannelByGameName = new Dictionary<string, HashSet<string>>();

        public static DateTime startTime = DateTime.Today.AddMonths(-1);


        public static bool IsRefreshing { get; private set; }

        public static void ClearCache()
        {
            IsRefreshing = true;

            Register.Clear();
            RegisterByDate.Clear();
            RegisterByID.Clear();
            LastRegisterTime = new DateTime(0);

            Login.Clear();
            LoginByDate.Clear();
            LoginByID.Clear();
            LastLoginID = 0;

            AnalysisID.Clear();
            Analysis.Clear();
            AnalysisByRegisterID.Clear();
            LastAnalysisTime = new DateTime(0);

            Online.Clear();
            OnlineByDate.Clear();
            LastOnlineTime = new DateTime(0);

            GameNames.Clear();
            LabelByGameName.Clear();
            ChannelByGameName.Clear();

            IsRefreshing = false;
        }

        public static void RefreshCache(Action onComplete)
        {
            if (IsRefreshing)
            {
                _LOG.Warning("缓存正在刷新，不能重复刷新");
                return;
            }

            IsRefreshing = true;
            AsyncThread.QueueUserWorkItem(() =>
            {
                try
                {
                    RefreshRegister();
                    RefreshLogin();
                    RefreshAnalysis();
                    RefreshOnline();
                }
                catch (Exception ex)
                {
                    _LOG.Error(ex, "更新缓存异常");
                    IsRefreshing = false;
                }
                finally
                {
                    if (!IsRefreshing)
                        ClearCache();
                    IsRefreshing = false;
                    if (onComplete != null)
                        onComplete();
                }
            });
        }

        static DateTime HandleStartTime(DateTime time)
        {
            return time >= startTime ? time : startTime;
        }

        static void RefreshRegister()
        {
            var list = _DB._DAO.SelectObjects<C_Register>("SELECT * FROM t_register WHERE RegisterTime > @p0 ORDER BY ID", HandleStartTime(LastRegisterTime));
            if (list.Count == 0) return;
            foreach (var item in list)
            {
                if (RegisterByID.ContainsKey(item.ID)) continue;
                Register.Add(item);
                List<C_Register> temp;
                if (!RegisterByDate.TryGetValue(item.RegisterTime.Date, out temp))
                {
                    temp = new List<C_Register>();
                    RegisterByDate.Add(item.RegisterTime.Date, temp);
                }
                temp.Add(item);
                RegisterByID.Add(item.ID, item);
                GameNames.Add(item.GameName);

                HashSet<string> channels;
                if (!ChannelByGameName.TryGetValue(item.GameName, out channels))
                {
                    channels = new HashSet<string>();
                    ChannelByGameName.Add(item.GameName, channels);
                }
                channels.Add(item.Channel);
            }
            LastRegisterTime = list.Last().RegisterTime;
        }

        static void RefreshLogin()
        {
            var list = _DB._DAO.SelectObjects<C_Login.C_Login2>("SELECT * FROM t_login WHERE ID > @p0 ORDER BY ID", LastLoginID);
            if (list.Count == 0) return;
            foreach (var item in list)
            {
                if (LoginByID.ContainsKey(item.ID)) continue;
                var change = item.change();
                if (change == null) continue;
                Login.Add(change);
                LoginByID.Add(change.ID, change);
                List<C_Login> temp;
                if (!LoginByDate.TryGetValue(change.LoginTime.Date, out temp))
                {
                    temp = new List<C_Login>();
                    LoginByDate.Add(change.LoginTime.Date, temp);
                }
                temp.Add(change);
            }
            LastLoginID = list.Last().ID;
        }

        static void RefreshAnalysis()
        {
            var list = _DB._DAO.SelectObjects<C_Analysis.C_Analysis2>("SELECT * FROM t_analysis WHERE `Time` > @p0 ORDER BY `Time`", HandleStartTime(LastAnalysisTime));
            if (list.Count == 0) return;
            foreach (var item in list)
            {
                if (!AnalysisID.Add(item.RegisterID + item.Label + item.Name)) continue;
                var change = item.change();
                if (change == null) continue;
                Analysis.Add(change);

                HashSet<string> temp;
                if (!LabelByGameName.TryGetValue(change.Account.GameName, out temp))
                {
                    temp = new HashSet<string>();
                    LabelByGameName.Add(change.Account.GameName, temp);
                }
                temp.Add(change.Label);

                List<C_Analysis> analyses;
                if (!AnalysisByRegisterID.TryGetValue(item.RegisterID, out analyses))
                {
                    analyses = new List<C_Analysis>();
                    AnalysisByRegisterID.Add(item.RegisterID, analyses);
                }
                analyses.Add(change);
            }
            LastAnalysisTime = list.Last().Time;
        }

        static void RefreshOnline()
        {
            var list = _DB._DAO.SelectObjects<C_Online>("SELECT * FROM t_online WHERE `Time` > @p0 ORDER BY `Time`", HandleStartTime(LastOnlineTime));
            if (list.Count == 0) return;
            foreach (var item in list)
            {
                Online.Add(item);
                List<C_Online> onlines;
                if (!OnlineByDate.TryGetValue(item.Time, out onlines))
                {
                    onlines = new List<C_Online>();
                    OnlineByDate.Add(item.Time, onlines);
                }
                onlines.Add(item);
            }
            LastOnlineTime = list.Last().Time;
        }
    }
}
