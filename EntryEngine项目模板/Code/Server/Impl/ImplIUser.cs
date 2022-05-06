using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine;

namespace Server.Impl
{
    class ImplIUser : ImplUserBase<T_USER>, _IUser
    {
        /// <summary>需要更新的T_PLAYER的数据字段</summary>
        public HashSet<ET_USER> Updates = new HashSet<ET_USER>();
        /// <summary>需要更新的T_USER的字段，调用Save时统一保存</summary>
        public void Update(params ET_USER[] updates)
        {
            foreach (var item in updates)
                Updates.Add(item);
        }
        protected override void OnSave(StringBuilder builder, List<object> objs)
        {
            if (Updates.Count > 0)
            {
                _DB._T_USER.GetUpdateSQL(User, null, builder, objs, Updates.ToArray());
                Updates.Clear();
            }
        }

        void _IUser.UserInfo(CBIUser_UserInfo callback)
        {
            // 同步部分数据
            callback.Callback(User);
        }
        void _IUser.UserModifyPassword(string opass, string npass, CBIUser_UserModifyPassword callback)
        {
            UserModifyPassword(opass, npass);
            callback.Callback(true);
        }
        void _IUser.UserModifyPhone(string phone, string code, CBIUser_UserModifyPhone callback)
        {
            UserModifyPhone(phone, code);
            callback.Callback(true);
        }
        void _IUser.UserExitLogin(CBIUser_UserExitLogin callback)
        {
            UserExitLogin();
            callback.Callback(true);
        }
    }
}
