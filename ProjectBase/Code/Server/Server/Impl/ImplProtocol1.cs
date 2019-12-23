using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine;

namespace Server.Impl
{
    class ImplProtocol1 : _Protocol1
    {
        void _Protocol1.Login(string name, string password, CBProtocol1_Login callback)
        {
            var player = _DB._T_PLAYER.Select(null, "WHERE Name=@p0", name);
            bool isRegister = player == null;
            if (isRegister)
            {
                _LOG.Debug("创建角色：{0}", name);
                player = new T_PLAYER();
                player.Name = name;
                player.Password = password;
                player.RegisterDate = DateTime.Now;
                player.Platform = "账号密码";
                player.LastLoginTime = player.RegisterDate;
                player.Token = Guid.NewGuid().ToString();
                player.ID = _DB._T_PLAYER.Insert(player);
            }
            else
            {
                if (player.Password != password)
                    throw new InvalidOperationException("密码错误");
                _LOG.Debug("玩家{0}登录", name);
                player.LastLoginTime = DateTime.Now;
                player.Token = Guid.NewGuid().ToString();
                _DB._T_PLAYER.Update(player, null,
                    ET_PLAYER.LastLoginTime,
                    ET_PLAYER.Token);
            }
            _DB._T_OPLog.Insert(new T_OPLog()
            {
                Operation = "登录",
                Way = isRegister ? "0" : "1",
                PID = player.ID,
                Time = DateTime.Now,
            });
            callback.Callback(player);
        }
    }
}
