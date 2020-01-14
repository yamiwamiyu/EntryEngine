using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine;

namespace Server.Impl
{
    class ImplProtocol1 : _Protocol1
    {
        void _Protocol1.PlayerExists(string name, CBProtocol1_PlayerExists callback)
        {
            callback.Callback(_DB._DAO.ExecuteScalar<bool>("SELECT EXISTS(SELECT ID FROM T_PLAYER WHERE Name=@p0)", name));
        }
        T_PLAYER Login(string name, string password, T_PLAYER player)
        {
            // 没有角色则自动创建角色
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
            player.Password = null;
            return player;
        }
        void _Protocol1.Register(string name, string password, CBProtocol1_Register callback)
        {
            bool exists = _DB._DAO.ExecuteScalar<bool>("SELECT EXISTS(SELECT ID FROM T_PLAYER WHERE Name=@p0)", name);
            if (exists)
                throw new InvalidOperationException("账号已存在");
            callback.Callback(Login(name, password, null));
        }
        void _Protocol1.Login(string name, string password, CBProtocol1_Login callback)
        {
            var player = _DB._T_PLAYER.Select(null, "WHERE Name=@p0", name);
            // 并行时用此接口创建角色会创建多个相同角色
            // 仅方便测试的话，可以不需要这个检测，以自动创建角色或登录角色
            if (player == null)
                throw new InvalidOperationException("账号不存在");
            callback.Callback(Login(name, password, player));
        }
    }
}
