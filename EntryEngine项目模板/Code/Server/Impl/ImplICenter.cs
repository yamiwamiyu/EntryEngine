using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine;

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

        void _ICenter.UserInfo(CBICenter_UserInfo callback)
        {
            // 同步部分数据
            callback.Callback(User);
        }
        void _ICenter.UserModifyPassword(string opass, string npass, CBICenter_UserModifyPassword callback)
        {
            UserModifyPassword(opass, npass);
            callback.Callback(true);
        }
        void _ICenter.UserModifyPhone(string phone, string code, CBICenter_UserModifyPhone callback)
        {
            UserModifyPhone(phone, code);
            callback.Callback(true);
        }
        void _ICenter.UserExitLogin(CBICenter_UserExitLogin callback)
        {
            UserExitLogin();
            callback.Callback(true);
        }
    }
}
