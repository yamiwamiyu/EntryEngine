using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;
using System;
using LauncherManager;
using System.Linq;

public partial class S登陆菜单 : UIScene
{
    private User user;
    private Pool<S登陆平台信息> platforms = new Pool<S登陆平台信息>();

    bool HasPlatformSelected
    {
        get
        {
            foreach (var item in platforms)
                if (item.CB单选.Checked)
                    return true;
            return false;
        }
    }

    public S登陆菜单()
    {
        Initialize();

        Background = TEXTURE.Pixel;

        DD用户名.DropDownList.SelectedIndexChanged += DD用户名_SelectedIndexChanged;
        B连接服务端.Clicked += new DUpdate<UIElement>(B连接服务端_Clicked);
        B用户名删除.Clicked += new DUpdate<UIElement>(B用户名删除_Clicked);
        B全选.Clicked += new DUpdate<UIElement>(B全选_Clicked);
        B一键连接.Clicked += new DUpdate<UIElement>(B一键连接_Clicked);
        B一键删除.Clicked += new DUpdate<UIElement>(B一键删除_Clicked);
    }

    void DD用户名_SelectedIndexChanged(Selectable sender)
    {
        if (sender.SelectedIndex != -1)
        {
            string name = DD用户名.Text;
            user = _SAVE.Users.FirstOrDefault(u => u.Name == name);
        }
        else
        {
            user = null;
        }
        RefreshPlatforms();
    }
    void B一键删除_Clicked(UIElement sender, Entry e)
    {
        if (!HasPlatformSelected)
        {
            S确认对话框.Hint("必须选中至少一项");
            return;
        }

        S确认对话框.Confirm("确认要删除选中的项吗？", (result) =>
        {
            if (result)
            {
                foreach (var item in platforms)
                    if (item.CB单选.Checked)
                        user.Managed.Remove((Platform)item.CB单选.Tag);

                RefreshPlatforms();
                _SAVE.Save();
            }
        });
    }
    void B一键连接_Clicked(UIElement sender, Entry e)
    {
        if (!HasPlatformSelected)
        {
            S确认对话框.Hint("必须选中至少一项");
            return;
        }

        string name = DD用户名.Text;
        if (string.IsNullOrEmpty(name))
        {
            S确认对话框.Hint("账号名不能为空");
            return;
        }

        string password = TB密码.Text;
        if (string.IsNullOrEmpty(password))
        {
            S确认对话框.Hint("密码不能为空");
            return;
        }

        // 连接所有选择的平台
        foreach (var item in platforms)
        {
            if (!item.CB单选.Checked)
                continue;

            Platform platform = item.CB单选.Tag as Platform;
            Maintainer.Connect(name, password, platform.IP, platform.Port);
        }

        // 等待连接完成
        S确认对话框.Wait("正在等待服务器操作", WaitConnect());
    }
    void B全选_Clicked(UIElement sender, Entry e)
    {
        bool result = false;
        foreach (var item in platforms)
        {
            if (!item.CB单选.Checked)
            {
                result = true;
                break;
            }
        }

        foreach (var item in platforms)
            item.CB单选.Checked = result;
    }
    void B用户名删除_Clicked(UIElement sender, Entry e)
    {
        var user = _SAVE.Users.FirstOrDefault(u => u.Name == DD用户名.Text);
        if (user != null)
        {
            S确认对话框.Confirm("确认要删除选中的项吗？", (result) =>
            {
                if (!result)
                    return;
                _SAVE.Users.Remove(user);
                _SAVE.Save();
                DD用户名.DropDownList.RemoveCurrent();
                if (DD用户名.DropDownList.SelectedIndex == -1)
                {
                    this.user = null;
                    RefreshPlatforms();
                }
            });
        }
    }
    void B连接服务端_Clicked(UIElement sender, Entry e)
    {
        string name = DD用户名.Text;
        if (string.IsNullOrEmpty(name))
        {
            S确认对话框.Hint("账号名不能为空");
            return;
        }

        string password = TB密码.Text;
        if (string.IsNullOrEmpty(password))
        {
            S确认对话框.Hint("密码不能为空");
            return;
        }

        if (string.IsNullOrEmpty(TB服务端.Text))
        {
            S确认对话框.Hint("终端入口不能为空");
            return;
        }

        try
        {
            string[] param = TB服务端.Text.Split(':');
            string ip = param[0];
            ushort port = ushort.Parse(param[1]);

            var maintainer = Maintainer.Connect(name, password, ip, port);
            // 最长等待连接
            TIME time = new TIME(5000);
            S确认对话框.Wait("正在连接服务器", () => 
                {
                    if (maintainer.Connected)
                    {
                        bool save = false;
                        // 连接成功
                        user = _SAVE.Users.FirstOrDefault(u => u.Name == name);
                        if (user == null)
                        {
                            user = new User();
                            user.Name = name;
                            _SAVE.Users.Add(user);
                            save = true;

                            DD用户名.DropDownList.AddItem(name);
                        }

                        if (!user.Managed.Any(p => p.IP == ip && p.Port == port))
                        {
                            Platform platform = new Platform();
                            platform.IP = ip;
                            platform.Port = port;
                            platform.Name = maintainer.Platform;
                            user.Managed.Add(platform);
                            save = true;
                            RefreshPlatforms();
                        }

                        if (save)
                        {
                            _SAVE.Save();
                        }
                        _LOG.Info("{0}连接平台服务器{1}成功", name, ip);

                        // 切换至服务器管理
                        Entry.ShowMainScene<S服务器管理>();
                        return true;
                    }
                    else
                    {
                        if (maintainer.IsDisposed)
                        {
                            S确认对话框.Hint("用户名或密码错误");
                        }
                        if (time.IsEnd)
                        {
                            maintainer.Logout();
                            S确认对话框.Hint("连接服务器{0}失败", TB服务端.Text);
                        }

                        time.Update(Entry.Instance.GameTime);
                        return false;
                    }
                });
        }
        catch (Exception ex)
        {
        }
    }

    void RefreshUsers()
    {
        DD用户名.DropDownList.Clear();
        foreach (var item in _SAVE.Users)
        {
            var newItem = ___B用户名1();
            newItem.Text = item.Name;
            newItem.Width = DD用户名.DropDownList.Width;
            DD用户名.DropDownList.Add(newItem);
        }
    }
    void RefreshPlatforms()
    {
        Panel parent = P登陆信息面板;
        parent.Clear();

        foreach (var item in platforms)
            item.Close(true);
        platforms.ClearToFree();

        if (user != null)
        {
            float x = TB登陆平台信息1.X;
            float y = TB登陆平台信息1.Y;
            float space = TB登陆平台信息2.Y - TB登陆平台信息1.Y;
            for (int i = 0; i < user.Managed.Count; i++)
            {
                var scene = platforms.Allot();
                if (scene == null)
                {
                    scene = new S登陆平台信息();
                    platforms.Add(scene);
                }

                scene.X = x;
                scene.Y = y + space * i;
                scene.L平台信息.Text = user.Managed[i].FullName;
                scene.CB单选.Tag = user.Managed[i];
                scene.CB单选.Checked = true;
                parent.Add(scene);

                Entry.ShowDialogScene(scene);
            }
        }
    }
    IEnumerable<ICoroutine> WaitConnect()
    {
        TIMER timer = TIMER.StartNew();

        while (true)
        {
            if (Maintainer.Maintainers.Count == 0)
            {
                S确认对话框.Hint("连接服务器{0}失败", string.Empty);
                yield break;
            }

            bool result = Maintainer.Maintainers.TrueForAll(m => m.Connected);
            if (result)
            {
                break;
            }
            else
            {
                if (timer.ElapsedNow.TotalSeconds >= 10)
                {
                    foreach (var item in Maintainer.Maintainers.ToArray())
                        if (!item.Connected)
                            item.Logout();
                    continue;
                }
                yield return null;
            }
        }

        // 切换至服务器管理
        Entry.ShowMainScene<S服务器管理>();
    }
    
    private IEnumerable<ICoroutine> MyLoading()
    {
        RefreshUsers();
        TB密码.Text = null;
        if (DD用户名.DropDownList.SelectedIndex == -1 && _SAVE.Users.Count > 0)
            DD用户名.DropDownList.SelectedIndex = 0;

        return null;
    }

    protected override void InternalEvent(Entry e)
    {
        base.InternalEvent(e);

        if (e.INPUT.Keyboard != null && e.INPUT.Keyboard.IsClick(PCKeys.Enter))
        {
            if (string.IsNullOrEmpty(TB服务端.Text))
                B一键连接_Clicked(B一键连接, e);
            else
                B连接服务端_Clicked(B连接服务端, e);
        }
    }
}
