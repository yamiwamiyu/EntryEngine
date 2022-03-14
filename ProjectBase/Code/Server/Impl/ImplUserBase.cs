using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using EntryEngine.Network;
using EntryEngine;

namespace Server.Impl
{
    public enum ELoginWay : byte
    {
        Token = 0,
        手机号 = 1,
        忘记密码 = 2,
        密码 = 3,
        其它 = 255,
    }
    /// <summary>基于用户的接口实现基类</summary>
    /// <typeparam name="T">用户类型</typeparam>
    public class ImplUserBase<T> where T : T_UserBase, new()
    {
        internal static Dictionary<string, T> cacheByToken = new Dictionary<string, T>();
        internal static Dictionary<int, T> cacheByID = new Dictionary<int, T>();
        internal static Dictionary<long, T> cacheByPhone = new Dictionary<long, T>();

        public static I Create<I>(int pid) where I : ImplUserBase<T>, new()
        {
            I result = new I();

            // 缓存
            lock (cacheByToken)
                cacheByID.TryGetValue(pid, out result.User);
            bool hasCache = result.User != null;
            if (!hasCache)
                // 从数据库登录
                result.User = result.LoadFromDatabaseByID(pid);

            string.Format("未找到ID为{0}的用户", pid).Check(result.User == null);

            result.LoginFromDatabase(result.User);
            result.User.MaskData();
            return result;
        }

        public T User;
        public StringBuilder SaveBuilder = new StringBuilder();
        /// <summary></summary>
        public List<object> SaveValues = new List<object>();

        protected virtual void OnSave(StringBuilder builder, List<object> objs) { }
        /// <summary>批量执行非查询语句</summary>
        /// <param name="onSQL"></param>
        /// <param name="async">是否异步保存</param>
        public void Save(Action<StringBuilder, List<object>> onSQL, bool async)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(SaveBuilder);
            List<object> objs = new List<object>(SaveValues);

            SaveBuilder.Clear();
            SaveValues.Clear();

            OnSave(builder, objs);

            if (onSQL != null)
                onSQL(builder, objs);

            if (builder.Length > 0)
            {
                if (async)
                    _DB._DAO.ExecuteAsync(db => db.ExecuteNonQuery(builder.ToString(), objs.ToArray()));
                else
                    _DB._DAO.ExecuteNonQuery(builder.ToString(), objs.ToArray());
            }
        }
        /// <summary>批量执行非查询语句</summary>
        public void Save()
        {
            Save(null, false);
        }

        public void OP(string op, string way, int sign, int statistic, string detail)
        {
            T_OPLog log = new T_OPLog();
            log.PID = User.ID;
            log.Time = DateTime.Now;
            log.Operation = op;
            log.Way = way;
            log.Sign = sign;
            log.Statistic = statistic;
            log.Detail = detail;
            //_DB._T_OPLog.Insert(log);
            _DB._T_OPLog.GetInsertSQL(log, SaveBuilder, SaveValues);
        }
        public void OP(string op, string way, int sign, int statistic)
        {
            OP(op, way, sign, statistic, null);
        }
        public void OP(string op, string way, int sign)
        {
            OP(op, way, sign, 0, null);
        }
        public void OP(string op, string way)
        {
            OP(op, way, 0, 0, null);
        }
        public void OP(string op, int sign)
        {
            OP(op, null, sign, 0, null);
        }

        public void CheckToken(HttpListenerContext context)
        {
            string token = context.Request.Headers["AccessToken"];
            if (string.IsNullOrEmpty(token))
                throw new HttpException(100, "没有登录!");

            // 做分布式使用多进程时，请不要使用以下缓存机制

            // 缓存
            lock (cacheByToken)
            {
                cacheByToken.TryGetValue(token, out User);
            }
            bool hasCache = User != null;
            if (!hasCache)
            {
                // 从数据库登录
                User = LoadFromDatabaseByToken(token);
                if (User != null)
                {
                    LoginFromDatabase(User);
                    User.MaskData();
                }
            }
            // 检查登录
            if (User != null)
            {
                var now = GameTime.Time.CurrentFrame;
                var elapsed = (now - User.LastLoginTime).TotalMinutes;
                // Token过期时间
                if (elapsed < T_UserBase.TOKEN_EXPIRE_MINUTES)
                {
                    // 刷新Token过期时间到数据库
                    if (elapsed > T_UserBase.TOKEN_EXPIRE_MINUTES * 0.7f)
                    {
                        User.LastLoginTime = now;
                        User.LastRefreshLoginTime = now;
                        OnUpdateLastLoginTime();
                    }
                    return;
                }
                else
                {
                    _LOG.Debug("用户:{0}缓存过期", User.ID);
                    lock (cacheByToken)
                    {
                        cacheByToken.Remove(token);
                    }
                }
            }
            throw new HttpException(100, "没有登录!");
        }

        /// <summary>首次从数据库加载出来T_USER对象时调用</summary>
        private void LoginFromDatabase(T player)
        {
            lock (cacheByPhone)
            {
                cacheByPhone[player.Phone] = player;
            }
            lock (cacheByID)
            {
                cacheByID[player.ID] = player;
            }
            if (!string.IsNullOrEmpty(player.Token))
            {
                lock (cacheByToken)
                {
                    cacheByToken[player.Token] = player;
                }
            }

            OnLoginFromDatabase(player);

            player.LastRefreshLoginTime = player.LastLoginTime;
        }
        /// <summary>从数据库将用户数据拉出来时，可以初始化一些内存数据</summary>
        protected virtual void OnLoginFromDatabase(T user)
        {
        }
        /// <summary>通过ID从数据库加载用户</summary>
        protected virtual T LoadFromDatabaseByID(int id)
        {
            return _DB._DAO.SelectObject<T>(string.Format("SELECT * FROM `{0}` WHERE ID = @p0", typeof(T).Name), id);
        }
        /// <summary>通过Token从数据库加载用户</summary>
        protected virtual T LoadFromDatabaseByToken(string token)
        {
            return _DB._DAO.SelectObject<T>(string.Format("SELECT * FROM `{0}` WHERE Token = @p0", typeof(T).Name), token);
        }
        /// <summary>通过手机号从数据库加载用户</summary>
        protected virtual T LoadFromDatabaseByPhone(long phone)
        {
            return _DB._DAO.SelectObject<T>(string.Format("SELECT * FROM `{0}` WHERE Phone = @p0", typeof(T).Name), phone);
        }
        /// <summary>通过用户名从数据库加载用户</summary>
        protected virtual T LoadFromDatabaseByAccount(string account)
        {
            return _DB._DAO.SelectObject<T>(string.Format("SELECT * FROM `{0}` WHERE Account = @p0 OR Phone = @p0", typeof(T).Name), account);
        }
        /// <summary>更新Token过期时间，默认为数据库语句更新</summary>
        protected virtual void OnUpdateLastLoginTime()
        {
            _DB._DAO.ExecuteNonQuery(string.Format("UPDATE `{0}` SET LastLoginTime = @p1 WHERE ID = @p0", typeof(T).Name), User.ID, User.LastLoginTime);
        }
        /// <summary>更新Token，默认为数据库语句更新</summary>
        protected virtual void OnUpdateToken()
        {
            _DB._DAO.ExecuteNonQuery(string.Format("UPDATE `{0}` SET Token = @p1 WHERE ID = @p0", typeof(T).Name), User.ID, User.Token);
        }

        protected virtual void InternalRegister(T user, ELoginWay way)
        {
        }
        protected virtual void InternalLogin(T user, ELoginWay way)
        {
        }
        /// <summary>账号登录或注册</summary>
        /// <param name="onNewUser">注册时，写入玩家信息，例如游戏名，手机号等</param>
        /// <param name="onRegister">玩家已经成功注册并登录后回调</param>
        /// <param name="loginWay">登录途径，例如手机，Token，微信等</param>
        protected T InternalLogin(T user, Action<T> onNewUser, Action<T> onRegister, ELoginWay loginWay)
        {
            // 没有角色则自动创建角色
            bool isRegister = user == null;
            if (user == null)
            {
                user = new T();
            }
            this.User = user;
            if (isRegister)
            {
                if (onNewUser != null)
                    onNewUser(user);
                user.RegisterTime = DateTime.Now;
                user.LastLoginTime = user.RegisterTime;
                user.Token = Guid.NewGuid().ToString();
                user.ID = OnInsert(user);
            }
            else
            {
                user.LastLoginTime = DateTime.Now;
                OnUpdateLastLoginTime();

                if (loginWay != ELoginWay.Token)
                {
                    if (user.Token != null)
                    {
                        lock (cacheByToken)
                        {
                            cacheByToken.Remove(user.Token);
                        }
                    }

                    user.Token = Guid.NewGuid().ToString("n");
                    OnUpdateToken();
                }
            }

            LoginFromDatabase(user);

            // 注册账号
            if (isRegister)
            {
                if (onRegister != null)
                    onRegister(User);
            }

            OP("登录", null, (int)loginWay, isRegister ? 1 : 0);
            Save();

            user.MaskData();
            return user;
        }
        /// <summary>插入一个用户，返回用户ID</summary>
        protected virtual int OnInsert(T user)
        {
            return _DB._DAO.SelectValue<int>(
string.Format(@"INSERT `{0}`(RegisterTime, Token, Password, Phone, LastLoginTime) VALUES(@p0, @p1, @p2, @p3, @p4)
SELECT LAST_INSERT_ID();", typeof(T).Name), 
                         user.RegisterTime,
                         user.Token,
                         user.Password,
                         user.Phone,
                         user.LastLoginTime);
        }
        /// <summary>手机号登录 / 注册</summary>
        public T LoginByPhone(string telphone, string password, Action<T> onNewUser, Action<T> onRegister)
        {
            long phone = long.Parse(telphone);

            T player = null;
            // 翻缓存
            lock (cacheByPhone)
            {
                cacheByPhone.TryGetValue(phone, out player);
            }
            // 缓存没有则找数据库
            if (player == null)
            {
                player = LoadFromDatabaseByPhone(phone);
            }
            return InternalLogin(player, p =>
            {
                _LOG.Info("手机号创建用户：{0}", telphone);
                p.Phone = phone;
                p.Password = password;
                if (onNewUser != null)
                    onNewUser(p);
            }, onRegister, ELoginWay.手机号);
        }
        /// <summary>忘记密码：手机号登录，重设密码</summary>
        public T ForgetPassword(string telphone, string password)
        {
            long phone = long.Parse(telphone);

            T player = null;
            // 翻缓存
            lock (cacheByPhone)
            {
                cacheByPhone.TryGetValue(phone, out player);
            }
            // 缓存没有则找数据库
            if (player == null)
            {
                player = LoadFromDatabaseByPhone(phone);
            }
            "账号不存在".Check(player == null);
            player.Password = password;
            return InternalLogin(player, null, null, ELoginWay.忘记密码);
        }
        /// <summary>Token登录</summary>
        public T LoginByToken(string token)
        {
            // 缓存
            lock (cacheByToken)
            {
                cacheByToken.TryGetValue(token, out User);
            }
            bool hasCache = User != null;
            if (!hasCache)
            {
                User = LoadFromDatabaseByToken(token);
                "Token已过期".Check(User == null || User.TokenExpired);
            }
            return InternalLogin(User, null, null, ELoginWay.Token);
        }
        /// <summary>账号密码：登录 | 注册</summary>
        /// <param name="isLoginOnly">true：仅登录，没有账号则抛出一场 / false: 没有账号则注册</param>
        public T LoginByPassword(string account, string password, bool isLoginOnly)
        {
            User = LoadFromDatabaseByAccount(account);
            "不存在此用户".Check(isLoginOnly && User == null);
            if (User == null)
            {
                // 注册
                return InternalLogin(User, 
                    (u) =>
                    {
                        u.Account = account;
                        u.Password = password;
                    }, null, ELoginWay.密码);
            }
            else
            {
                // 登录
                "密码不正确".Check(User.Password != password);
                return InternalLogin(User, null, null, ELoginWay.密码);
            }
        }
    }
}
