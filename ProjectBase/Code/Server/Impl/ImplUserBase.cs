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
        // hack: 做分布式使用多进程时，请不要缓存机制
        /// <summary>使用缓存：加速访问，数据库改用户数据无法自动刷新，多进程时无法同步</summary>
        internal static bool UseCache = true;
        internal static Dictionary<string, T> cacheByToken = new Dictionary<string, T>();
        internal static Dictionary<int, T> cacheByID = new Dictionary<int, T>();
        internal static Dictionary<long, T> cacheByPhone = new Dictionary<long, T>();
        internal static Dictionary<string, T> cacheByAccount = new Dictionary<string, T>();
        public static bool ClearUserCache(int id)
        {
            T user;
            lock (cacheByID)
                cacheByID.TryGetValue(id, out user);
            if (user != null)
                return ClearUserCache(user);
            return false;
        }
        public static bool ClearUserCache(T user)
        {
            bool flag = false;
            lock (cacheByID)
                if (cacheByID.TryGetValue(user.ID, out user))
                    flag |= cacheByID.Remove(user.ID);
            if (user != null)
            {
                lock (cacheByPhone)
                    flag |= cacheByPhone.Remove(user.Phone);
                if (!string.IsNullOrEmpty(user.Token))
                    lock (cacheByToken)
                        flag |= cacheByToken.Remove(user.Token);
                if (!string.IsNullOrEmpty(user.Account))
                    lock (cacheByAccount)
                        flag |= cacheByAccount.Remove(user.Account);
            }
            return flag;
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

        public void CheckToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new HttpException(100, "没有登录!");

            // 缓存
            InitializeByToken(token);
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
                    // Token过期
                    lock (cacheByToken)
                        cacheByToken.Remove(token);
                }
            }
            throw new HttpException(100, "没有登录!");
        }

        /// <summary>首次从数据库加载出来T_USER对象时调用</summary>
        private void LoginFromDatabase(T user)
        {
            this.User = user;

            if (UseCache)
            {
                if (user.Phone != 0)
                    lock (cacheByPhone)
                        cacheByPhone[user.Phone] = user;
                if (user.ID != 0)
                    lock (cacheByID)
                        cacheByID[user.ID] = user;
                if (!string.IsNullOrEmpty(user.Token))
                    lock (cacheByToken)
                        cacheByToken[user.Token] = user;
                if (!string.IsNullOrEmpty(user.Account))
                    lock (cacheByAccount)
                        cacheByAccount[user.Account] = user;
            }

            OnLoginFromDatabase(user);

            user.LastRefreshLoginTime = user.LastLoginTime;
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
            long phone = 0;
            if (account.Length == 11)
                long.TryParse(account, out phone);
            return _DB._DAO.SelectObject<T>(string.Format("SELECT * FROM `{0}` WHERE Account = @p0 OR (@p1 <> 0 AND Phone = @p1)", typeof(T).Name), account, phone);
        }
        /// <summary>更新Token过期时间，默认为数据库语句更新</summary>
        protected virtual void OnUpdateLastLoginTime()
        {
            SaveBuilder.AppendFormat("UPDATE `{0}` SET LastLoginTime = @p{2} WHERE ID = @p{1}", typeof(T).Name, SaveValues.Count, SaveValues.Count + 1);
            SaveValues.Add(User.ID);
            SaveValues.Add(User.LastLoginTime);
            //_DB._DAO.ExecuteNonQuery(string.Format("UPDATE `{0}` SET LastLoginTime = @p1 WHERE ID = @p0", typeof(T).Name), User.ID, User.LastLoginTime);
        }
        /// <summary>更新Token，默认为数据库语句更新</summary>
        protected virtual void OnUpdateToken()
        {
            SaveBuilder.AppendFormat("UPDATE `{0}` SET LastLoginTime = @p{2} WHERE ID = @p{1}", typeof(T).Name, SaveValues.Count, SaveValues.Count + 1);
            SaveValues.Add(User.ID);
            SaveValues.Add(User.Token);
            //_DB._DAO.ExecuteNonQuery(string.Format("UPDATE `{0}` SET Token = @p1 WHERE ID = @p0", typeof(T).Name), User.ID, User.Token);
        }

        /// <summary>注册一个账号</summary>
        public virtual void Register(T user, ELoginWay way)
        {
            user.RegisterTime = DateTime.Now;
            user.LastLoginTime = user.RegisterTime;
            user.Token = Guid.NewGuid().ToString();
            user.ID = OnInsert(user);

            LoginFromDatabase(user);

            OP("登录", typeof(T).Name, (int)way, 1);
            Save();

            user.MaskData();
        }
        /// <summary>插入一个用户，返回用户ID</summary>
        protected virtual int OnInsert(T user)
        {
            return _DB._DAO.SelectValue<int>(
string.Format(@"INSERT `{0}`(RegisterTime, Token, Account, Password, Phone, LastLoginTime) VALUES(@p0, @p1, @p2, @p3, @p4, @p5);
SELECT LAST_INSERT_ID();", typeof(T).Name),
                         user.RegisterTime,
                         user.Token,
                         user.Account,
                         user.Password,
                         user.Phone,
                         user.LastLoginTime);
        }
        /// <summary>登录一个账号</summary>
        public virtual void Login(T user, ELoginWay way)
        {
            user.LastLoginTime = DateTime.Now;
            OnUpdateLastLoginTime();

            if (way != ELoginWay.Token)
            {
                if (user.Token != null)
                    lock (cacheByToken)
                        cacheByToken.Remove(user.Token);

                user.Token = Guid.NewGuid().ToString("n");
                OnUpdateToken();
            }

            LoginFromDatabase(user);

            OP("登录", typeof(T).Name, (int)way, 0);
            Save();

            user.MaskData();
        }

        // 根据不同参数加载用户
        public T InitializeByID(int id)
        {
            // 缓存
            if (UseCache)
                lock (cacheByID)
                    cacheByID.TryGetValue(id, out User);
            if (User == null)
                // 从数据库加载
                User = LoadFromDatabaseByID(id);
            if (User != null)
            {
                LoginFromDatabase(User);
                User.MaskData();
            }
            return User;
        }
        public T InitializeByPhone(long phone)
        {
            // 缓存
            if (UseCache)
                lock (cacheByPhone)
                    cacheByPhone.TryGetValue(phone, out User);
            if (User == null)
                // 从数据库加载
                User = LoadFromDatabaseByPhone(phone);
            if (User != null)
            {
                LoginFromDatabase(User);
                User.MaskData();
            }
            return User;
        }
        public T InitializeByToken(string token)
        {
            // 缓存
            if (UseCache)
                lock (cacheByToken)
                    cacheByToken.TryGetValue(token, out User);
            if (User == null)
                // 从数据库加载
                User = LoadFromDatabaseByToken(token);
            if (User != null)
            {
                LoginFromDatabase(User);
                User.MaskData();
            }
            return User;
        }
        public T InitializeByAccount(string account)
        {
            long phone;
            if (UseCache)
                if (long.TryParse(account, out phone))
                    // 缓存
                    lock (cacheByPhone)
                        cacheByPhone.TryGetValue(phone, out User);
            if (UseCache)
                if (User == null)
                    // 缓存
                    lock (cacheByAccount)
                        cacheByAccount.TryGetValue(account, out User);
            if (User == null)
                // 从数据库加载
                User = LoadFromDatabaseByAccount(account);
            if (User != null)
            {
                LoginFromDatabase(User);
                User.MaskData();
            }
            return User;
        }
    }
}
