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

        void _ICenter.GetUserInfo(CBICenter_GetUserInfo callback)
        {
            // 同步部分数据
            callback.Callback(User);
        }


        void _ICenter.ChangePassword(string oldPassword, string newPassword, CBICenter_ChangePassword callback)
        {
            throw new NotImplementedException();
        }

        void _ICenter.GetGameName(CBICenter_GetGameName callback)
        {
            throw new NotImplementedException();
        }
        void _ICenter.GetChannel(string gameName, CBICenter_GetChannel callback)
        {
            throw new NotImplementedException();
        }
        void _ICenter.GetAnalysisGame(CBICenter_GetAnalysisGame callback)
        {
            throw new NotImplementedException();
        }
    }
}
