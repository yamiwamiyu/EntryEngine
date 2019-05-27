using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;
using LauncherProtocolStructure;
using LauncherManager;
using System.Linq;
using System;

public partial class S新开服菜单 : UIScene
{
    private IEnumerable<Maintainer> managed;
    private Service service;

    internal IEnumerable<Server> Servers
    {
        get
        {
            return managed.SelectMany(m => m.Servers);
        }
    }
    //internal Server SelectedServer
    //{
    //}
    public bool New
    {
        get
        {
            return service == null;
        }
    }

    public S新开服菜单()
    {
        Initialize();

        B确定.Clicked += new DUpdate<UIElement>(B确定_Clicked);
        B取消.Clicked += new DUpdate<UIElement>(B取消_Clicked);
        DD服务器.DropDownList.SelectedIndexChanged += new Action<Selectable>(ChangeServer);
        DD服务类型.DropDownList.SelectedIndexChanged += new Action<Selectable>(ChangeServiceType);
    }

    void ChangeServiceType(Selectable obj)
    {
        if (obj.SelectedIndex == -1)
        {
            TB命令.Text = null;
        }
        else
        {
            var selected = obj.Selected;
            var serviceType = selected.Tag as ServiceType;
            TB命令.Text = serviceType.LaunchCommand;
        }
    }
    void ChangeServer(Selectable obj)
    {
        if (obj.SelectedIndex == -1)
            return;
        var selected = obj.Selected;
        var server = selected.Tag as Server;
        var managed = Maintainer.Find(server);
        foreach (var item in managed.ServiceTypes)
        {
            var newItem = DD服务类型.DropDownList.AddItem(item.Name);
            newItem.UIText.FontColor = COLOR.Black;
            newItem.UIText.TextAlignment = EPivot.MiddleCenter;
            newItem.Tag = item;
        }
    }
    void B取消_Clicked(UIElement sender, Entry e)
    {
        Close(true);
    }
    void B确定_Clicked(UIElement sender, Entry e)
    {
        if (New)
        {
            if (string.IsNullOrEmpty(TB名字.Text))
            {
                S确认对话框.Hint("账号名不能为空");
                return;
            }

            if (DD服务器.DropDownList.SelectedIndex == -1)
            {
                S确认对话框.Hint("必须选择服务器");
                return;
            }

            if (DD服务类型.DropDownList.SelectedIndex == -1)
            {
                S确认对话框.Hint("必须选择服务类型");
                return;
            }

            var server = DD服务器.DropDownList.Selected.Tag as Server;
            var maintainer = Maintainer.Find(server);
            var serviceType = DD服务类型.Text;

            maintainer.Proxy.NewService(server.ID, serviceType, TB名字.Text, TB命令.Text);
        }
        else
        {
            if (service.LaunchCommand != TB命令.Text)
            {
                var maintainer = Maintainer.Find(service);
                maintainer.Proxy.SetServiceLaunchCommand(service.Name, TB命令.Text);
            }
        }
        Close(true);
    }

    internal void Show(IEnumerable<Maintainer> managed, Service service)
    {
        this.managed = managed;
        this.service = service;

        DD服务器.DropDownList.Clear();
        DD服务类型.DropDownList.Clear();
        bool _new = New;
        if (_new)
        {
            foreach (var item in managed)
            {
                foreach (var server in item.Servers)
                {
                    var newItem = DD服务器.DropDownList.AddItem(string.Format("{0} - {1}", item.Platform, server.Name));
                    newItem.Tag = server;
                    newItem.UIText.FontColor = COLOR.Black;
                }
            }
            DD服务器.DropDownList.SelectedIndex = -1;
            DD服务类型.DropDownList.SelectedIndex = -1;
            TB名字.Text = null;
            TB命令.Text = null;

            TB名字.UIText.FontColor = COLOR.Black;
            DD服务器.UIText.FontColor = COLOR.Black;
            DD服务类型.UIText.FontColor = COLOR.Black;
        }
        else
        {
            DD服务器.Text = Maintainer.Find(service).Platform;
            DD服务类型.Text = service.Type;
            TB名字.Text = service.Name;
            TB命令.Text = service.LaunchCommand;

            TB名字.UIText.FontColor = COLOR.Gray;
            DD服务器.UIText.FontColor = COLOR.Gray;
            DD服务类型.UIText.FontColor = COLOR.Gray;
        }

        DD服务器.Eventable = _new;
        DD服务类型.Eventable = _new;
        TB名字.Readonly = !_new;
    }
    private IEnumerable<ICoroutine> MyLoading()
    {
        return null;
    }
}
