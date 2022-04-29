using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;
using LauncherManager;
using System;
using System.Linq;
using LauncherProtocolStructure;
using LauncherManagerProtocol;

public partial class S服务器管理 : UIScene
{
    internal IEnumerable<Maintainer> Managed
    {
        get
        {
            if (Maintainer.Maintainers.Count == 0)
                yield break;

            if (Maintainer.Maintainers.Count == 1)
                yield return Maintainer.Maintainers[0];
            else
            {
                if (DD用户名.DropDownList.SelectedIndex == 0)
                    foreach (var item in Maintainer.Maintainers)
                        yield return item;
                else
                    yield return Maintainer.Maintainers[DD用户名.DropDownList.SelectedIndex - 1];
            }
        }
    }
    internal IEnumerable<Server> ManagedServers
    {
        get { return Managed.SelectMany(s => s.Servers); }
    }
    internal IEnumerable<Service> ManagedServices
    {
        get { return Managed.SelectMany(s => s.Services); }
    }
    public bool NoRefresh;

    public S服务器管理()
    {
        Initialize();

        B中断连接.Clicked += new DUpdate<UIElement>(B中断连接_Clicked);
        DD用户名.DropDownList.SelectedIndexChanged += new Action<Selectable>(DropDownList_SelectedIndexChanged);
        B账号管理.Clicked += new DUpdate<UIElement>(B账号管理_Clicked);
        B服务管理.Clicked += new DUpdate<UIElement>(B服务管理_Clicked);
        CB服务.CheckedChanged += new DUpdate<Button>(CB服务_CheckedChanged);
        CB服务器.CheckedChanged += new DUpdate<Button>(CB服务器_CheckedChanged);
        B显示日志.Clicked += new DUpdate<UIElement>(B显示日志_Clicked);
        B更新管理器.Clicked += new DUpdate<UIElement>(B更新管理器_Clicked);
    }

    void B更新管理器_Clicked(UIElement sender, Entry e)
    {
        S确认对话框.Confirm("更新管理器将关闭所有平台的管理器及其所有服务！是否确认此操作？", (result) =>
            {
                if (!result)
                    return;
                int count = Maintainer.Maintainers.Count;
                foreach (var item in Maintainer.Maintainers)
                    item.Proxy.UpdateManager();
                bool flag = true;
                S确认对话框.Wait("正在等待服务器操作", () =>
                    {
                        if (flag)
                        {
                            // 开始有管理器关闭进行更新
                            if (Maintainer.Maintainers.Where(m => m.Agent != null).Count() < count)
                                flag = false;
                        }
                        else
                        {
                            // 所有管理器更新重启并且重连完成
                            if (Maintainer.Maintainers.Where(m => m.Agent != null).Count() == count)
                                return true;
                        }
                        return false;
                    });
            });
    }
    void B显示日志_Clicked(UIElement sender, Entry e)
    {
        var scene = Entry.GetScene<SManagerLog>();
        if (scene == null || scene.Entry == null)
            scene = Entry.ShowDialogScene<SManagerLog>(EState.None);
        else
            if (scene.FixedShow)
                scene.Close(false);
        scene.FixedShow = !scene.FixedShow;
    }
    void CB服务器_CheckedChanged(Button sender, Entry e)
    {
        if (sender.Checked)
        {
            if (P服务器管理面板.ChildCount > 0)
            {
                ((UIScene)P服务器管理面板[0]).Close(true);
                P服务器管理面板.Clear();
            }
            var scene = Entry.ShowDialogScene<S服务器面板>(EState.None);
            scene.Show(Managed);
            P服务器管理面板.Add(scene);
        }
    }
    void CB服务_CheckedChanged(Button sender, Entry e)
    {
        if (sender.Checked)
        {
            if (P服务器管理面板.ChildCount > 0)
            {
                ((UIScene)P服务器管理面板[0]).Close(true);
                P服务器管理面板.Clear();
            }
            var scene = Entry.ShowDialogScene<S服务管理面板>(EState.None);
            scene.Show(Managed);
            P服务器管理面板.Add(scene);
        }
    }
    void B服务管理_Clicked(UIElement sender, Entry e)
    {
        e.ShowDialogScene<S服务类型管理>(EState.Dialog).Show(Managed);
    }
    void B账号管理_Clicked(UIElement sender, Entry e)
    {
        e.ShowDialogScene<S账号管理菜单>(EState.Dialog).Show(Managed);
    }
    void DropDownList_SelectedIndexChanged(Selectable obj)
    {
        if (CB服务器.Checked)
        {
            Entry.ShowDialogScene<S服务器面板>(EState.None).Show(Managed);
        }
        else if (CB服务.Checked)
        {
            Entry.ShowDialogScene<S服务管理面板>(EState.None).Show(Managed);
        }

        bool security = Managed.All(m => m.Security >= ESecurity.Manager);
        B账号管理.Visible = security;
        B更新管理器.Visible = security;

        security = Managed.All(m => m.Security >= ESecurity.Maintainer);
        B服务管理.Visible = security;
    }
    void B中断连接_Clicked(UIElement sender, Entry e)
    {
        if (DD用户名.DropDownList.ItemCount > 1)
        {
            if (DD用户名.DropDownList.SelectedIndex == 0)
            {
                // 关闭全部
                foreach (var item in Maintainer.Maintainers)
                    item.Logout();
                e.ShowMainScene<S登陆菜单>();
            }
            else
            {
                DD用户名.DropDownList.RemoveCurrent();
                // 0是全部
                int index = DD用户名.DropDownList.SelectedIndex + 1;
                Maintainer.Maintainers[index].Logout();
                // 剩下最后一个，清除全部选项
                if (Maintainer.Maintainers.Count == 1)
                    DD用户名.DropDownList.RemoveItem(0);
            }
        }
        else
        {
            // 关闭最后一个平台，退出到登录菜单
            Maintainer.Maintainers[0].Logout();
            e.ShowMainScene<S登陆菜单>();
        }
    }

    void RefreshPlatform()
    {
        DD用户名.DropDownList.Clear();
        DD用户名.DropDownText.Eventable = false;
        if (Maintainer.Maintainers.Count > 1)
        {
            var newItem = ___B用户名1();
            newItem.Text = "全部";
            newItem.Width = DD用户名.DropDownList.Width;
            DD用户名.DropDownList.Add(newItem);
        }
        foreach (var item in Maintainer.Maintainers)
        {
            var newItem = ___B用户名1();
            newItem.Text = item.Name + "-" + item.Platform;
            newItem.Width = DD用户名.DropDownList.Width;
            DD用户名.DropDownList.Add(newItem);
        }
    }
    
    private IEnumerable<ICoroutine> MyLoading()
    {
        if (!NoRefresh)
        {
            RefreshPlatform();

            // 等待获取服务器列表
            bool result = false;
            S确认对话框.Wait("正在连接服务器", () => result);
            foreach (var item in Maintainer.Maintainers)
                item.Proxy.GetServers();
            while (!Maintainer.Maintainers.TrueForAll(m => m.Servers != null && m.ServiceTypes != null))
                yield return null;
            result = true;

            if (DD用户名.DropDownList.SelectedIndex == -1)
                DD用户名.DropDownList.SelectedIndex = 0;
        }
        NoRefresh = false;

        CB服务.Checked = false;
        CB服务.Checked = true;
    }
}
