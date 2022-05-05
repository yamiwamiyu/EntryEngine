using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine;

namespace Server.Impl
{
    class ImplIUser : _IUser
    {
        /// <summary>更新在线人数</summary>
        /// <param name="now">当前时间</param>
        void recordOnline(DateTime now, string gameName, string channel)
        {
            var quarter = T_Online.GetQuarter(now);
            var today = now.Date;
            _Cache.C_Online online = null;
            if (!_DB._T_Online.Exists(today, gameName, channel))
            {
                online = new _Cache.C_Online() { Time = today, GameName = gameName, Channel = channel };
                _Cache.Online.Add(online);
                List<_Cache.C_Online> onlines;
                if (!_Cache.OnlineByDate.TryGetValue(today, out onlines))
                {
                    onlines = new List<_Cache.C_Online>();
                    _Cache.OnlineByDate.Add(today, onlines);
                }
                onlines.Add(online);

                _DB._T_Online.Insert(online);
            }
            else
            {
                List<_Cache.C_Online> onlines;
                if (_Cache.OnlineByDate.TryGetValue(today, out onlines))
                {
                    for (int i = 0; i < onlines.Count; i++)
                    {
                        if (onlines[i].GameName == gameName && onlines[i].Channel == channel)
                        {
                            online = onlines[i];
                            break;
                        }
                    }
                }
            }

            if (online != null)
            {
                lock (online)
                {
                    var field = typeof(_Cache.C_Online).GetField("Quarter" + quarter);
                    field.SetValue(online, (int)field.GetValue(online) + 1);
                }
            }

            _DB._DAO.ExecuteNonQuery(string.Format("UPDATE t_online SET Quarter{0}=Quarter{1}+1 WHERE `Time` = @p0 AND GameName=@p1 AND Channel=@p2", quarter, quarter), today, gameName, channel);
        }

        void _IUser.Login(string gameName, string deviceID, string channel, CBIUser_Login callback)
        {
            "游戏名不能为空".Check(string.IsNullOrWhiteSpace(gameName));
            "设备号不能为空".Check(string.IsNullOrWhiteSpace(deviceID));
            channel = string.IsNullOrWhiteSpace(channel) ? "母包" : channel;
            var now = DateTime.Now;
            var today = DateTime.Today;
            int result;
            if (_DB._T_Register.Exists2("WHERE GameName = @p0 AND DeviceID = @p1", gameName, deviceID))
            {
                result = _DB._T_Register.Select(new ET_Register[] { ET_Register.ID }, "WHERE GameName = @p0 AND DeviceID = @p1 AND channel = @p2", gameName, deviceID, channel).ID;
            }
            else
            {
                var target = new T_Register()
                {
                    GameName = gameName,
                    DeviceID = deviceID,
                    Channel = channel,
                    RegisterTime = now,
                };
                result = _DB._T_Register.Insert(target);
                // 更新注册缓存
                var register = _Cache.C_Register.change(target);
                _Cache.Register.Add(register);
                _Cache.RegisterByID.Add(result, register);
                List<_Cache.C_Register> registers;
                if (!_Cache.RegisterByDate.TryGetValue(today, out registers))
                {
                    registers = new List<_Cache.C_Register>();
                    _Cache.RegisterByDate.Add(today, registers);
                }
                registers.Add(register);
            }

            // 更新在线人数
            var lastLoginTime = _DB._DAO.SelectValue<DateTime>("SELECT LastOnlineTime FROM t_login WHERE RegisterID = @p0 ORDER BY ID DESC LIMIT 1", result);
            if (lastLoginTime == null || T_Online.needRecord(lastLoginTime))
                recordOnline(now, gameName, channel);

            var loginTarget = new T_Login()
            {
                RegisterID = result,
                LoginTime = now,
                LastOnlineTime = now
            };
            result = _DB._T_Login.Insert(loginTarget);

            // 添加登录缓存
            var login = _Cache.C_Login.change(loginTarget);
            if (login != null)
            {
                _Cache.Login.Add(login);
                _Cache.LoginByID.Add(result, login);
                List<_Cache.C_Login> logins;
                if (!_Cache.LoginByDate.TryGetValue(today, out logins))
                {
                    logins = new List<_Cache.C_Login>();
                    _Cache.LoginByDate.Add(today, logins);
                }
                logins.Add(login);
            }

            // 添加游戏名缓存
            _Cache.GameNames.Add(gameName);

            // 添加渠道缓存
            HashSet<string> channels;
            if (!_Cache.ChannelByGameName.TryGetValue(gameName, out channels))
            {
                channels = new HashSet<string>();
                _Cache.ChannelByGameName.Add(gameName, channels);
            }
            channels.Add(channel);

            callback.Callback(result);
        }

        void _IUser.Online(int loginID, CBIUser_Online callback)
        {
            var login = _DB._T_Login.Select(loginID, ET_Login.ID, ET_Login.LastOnlineTime, ET_Login.RegisterID);
            if (login != null)
            {
                var register = _DB._T_Register.Select(login.RegisterID, ET_Register.GameName, ET_Register.Channel);
                var now = DateTime.Now;
                // 更新在线人数
                if (T_Online.needRecord(login.LastOnlineTime))
                    recordOnline(now, register.GameName, register.Channel);

                // 更新最后在线时间
                login.LastOnlineTime = now;
                _DB._T_Login.Update(login, null, ET_Login.LastOnlineTime);
                if (_Cache.LoginByID.ContainsKey(loginID))
                    _Cache.LoginByID[loginID].LastOnlineTime = now;
                else
                    _Cache.LoginByID.Add(loginID, _Cache.C_Login.change(login));

                callback.Callback(true);
            }
            else
                callback.Callback(false);
        }

        void _IUser.Analysis(int loginID, List<T_Analysis> analysis, CBIUser_Analysis callback)
        {
            var login = _DB._T_Login.Select(loginID, ET_Login.RegisterID);
            if (login != null && _Cache.RegisterByID.ContainsKey(login.RegisterID))
            {
                var register = _Cache.RegisterByID[login.RegisterID];
                _DB._DAO.ExecuteNonQuery((sql, values) =>
                {
                    if (analysis != null)
                    {
                        foreach (var item in analysis)
                        {
                            if (string.IsNullOrWhiteSpace(item.Label) || string.IsNullOrWhiteSpace(item.Name) || (item.Count != 0 && item.Count != 1)) continue;

                            item.RegisterID = login.RegisterID;
                            _Cache.C_Analysis target = _Cache.C_Analysis.change(item);
                            if (target == null) continue;
                            bool insert = false;
                            bool flag = _Cache.AnalysisID.Add(item.RegisterID + item.Label + item.Name);
                            // 插入事件到缓存
                            if (item.Count == 0)
                            {
                                // 事件只缓存一次
                                if (!flag) continue;
                                insert = true;
                            }
                            else
                            {
                                // 事件可缓存多次
                                List<_Cache.C_Analysis> list;
                                if (!_Cache.AnalysisByRegisterID.TryGetValue(item.RegisterID, out list))
                                {
                                    list = new List<_Cache.C_Analysis>();
                                    _Cache.AnalysisByRegisterID.Add(item.RegisterID, list);
                                }

                                target = null;
                                for (int i = 0; i < list.Count; i++)
                                {
                                    _Cache.C_Analysis listTemp = list[i];
                                    if (listTemp.Label == item.Label && listTemp.Name == item.Name)
                                    {
                                        target = listTemp;
                                        break;
                                    }
                                }
                                if (target != null)
                                    target.Count++;
                                else
                                {
                                    target = _Cache.C_Analysis.change(item);
                                    insert = true;
                                }
                            }
                            item.Count = target.Count;

                            // 刷新事件标签缓存
                            HashSet<string> temp;
                            if (!_Cache.LabelByGameName.TryGetValue(register.GameName, out temp))
                            {
                                temp = new HashSet<string>();
                                _Cache.LabelByGameName.Add(register.GameName, temp);
                            }
                            temp.Add(item.Label);

                            // 插入事件到数据库
                            if (insert)
                            {
                                _DB._T_Analysis.GetInsertSQL(item, sql, values);
                                _Cache.Analysis.Add(target);
                                List<_Cache.C_Analysis> list;
                                if (!_Cache.AnalysisByRegisterID.TryGetValue(item.RegisterID, out list))
                                {
                                    list = new List<_Cache.C_Analysis>();
                                    _Cache.AnalysisByRegisterID.Add(item.RegisterID, list);
                                }
                                list.Add(target);
                            }
                            else
                                _DB._T_Analysis.GetUpdateSQL(item, "WHERE RegisterID=" + item.RegisterID + " AND Label='" + item.Label + "' AND Name='" + item.Name + "'", sql, values, ET_Analysis.Count);
                        }
                    }
                });
                callback.Callback(true);
            }
            else
                callback.Callback(false);
        }
    }
}
