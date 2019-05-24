using System.Collections.Generic;
using System.Linq;
using EntryEngine;
using EntryEngine.UI;
using LauncherManager;
using LauncherManagerProtocol;
using LauncherProtocolStructure;

public partial class S服务管理面板 : UIScene
{
    public delegate IEnumerable<T> DYield<T>();

    private Pool<S服务信息> scenes = new Pool<S服务信息>();
    private IEnumerable<Maintainer> managed;

    internal IEnumerable<Service> Managed
    {
        get { return managed.SelectMany(m => m.Services); }
    }
    internal IEnumerable<Service> Selected
    {
        get
        {
            foreach (var item in scenes)
                if (item.CB单选.Checked)
                    yield return item.Service;
        }
    }

    public S服务管理面板()
    {
        Initialize();
        this.PhaseEnding += new System.Action<UIScene>(S服务管理面板_PhaseEnding);

        B全选.Clicked += new DUpdate<UIElement>(B全选_Clicked);
        B同类型.Clicked += new DUpdate<UIElement>(B同类型_Clicked);
        B同服务器.Clicked += new DUpdate<UIElement>(B同服务器_Clicked);
        B新设更改服务.Clicked += new DUpdate<UIElement>(B新设更改服务_Clicked);
        B删除服务.Clicked += new DUpdate<UIElement>(B删除服务_Clicked);
        B更新服务.Clicked += new DUpdate<UIElement>(B更新服务_Clicked);
        B关闭服务.Clicked += new DUpdate<UIElement>(B关闭服务_Clicked);
        B开启服务.Clicked += new DUpdate<UIElement>(B开启服务_Clicked);
        B执行命令.Clicked += new DUpdate<UIElement>(B执行命令_Clicked);
    }

    void B执行命令_Clicked(UIElement sender, Entry e)
    {
        if (!Selected.Any(s => s.Status != EServiceStatus.Stop))
        //if (!Selected.Contains(s => s.Status == EServiceStatus.Starting))
        {
            S确认对话框.Hint(Text.ETextID.ServiceAvailable);
            return;
        }

        S确认对话框.Input((command) =>
        {
            foreach (var service in Selected)
            {
                if (service.Status != EServiceStatus.Stop)
                //if (service.Status == EServiceStatus.Starting)
                {
                    var server = Maintainer.Find(service);
                    server.Proxy.CallCommand(service.Name, command);
                }
            }
        });
    }
    void B开启服务_Clicked(UIElement sender, Entry e)
    {
        foreach (var service in Selected)
        {
            if (service.Status == EServiceStatus.Stop)
            {
                var server = Maintainer.Find(service);
                server.Proxy.Launch(service.Name);
            }
        }
    }
    void B关闭服务_Clicked(UIElement sender, Entry e)
    {
        var selected = Selected.Where(s => s.Status != EServiceStatus.Stop).ToArrayAfterCount();
        if (selected.Length == 0)
        {
            //S确认对话框.Hint(Text.ETextID.SelectionEmpty);
            return;
        }

        S确认对话框.Confirm(Text.ETextID.Close,
            (result) =>
            {
                if (!result)
                    return;
                foreach (var service in Selected)
                {
                    if (service.Status != EServiceStatus.Stop)
                    {
                        var server = Maintainer.Find(service);
                        server.Proxy.Stop(service.Name);
                    }
                }
            }, string.Join(",", selected.Select(s => s.Name).ToArrayAfterCount()));
    }
    void B更新服务_Clicked(UIElement sender, Entry e)
    {
        bool hasSelected = false;
        foreach (var item in scenes)
        {
            if (item.CB单选.Checked)
            {
                hasSelected = true;
                break;
            }
        }
        if (hasSelected)
        {
            //foreach (var item in managed)
            //{
            //    foreach (var service in item.Services)
            //    {
            //        if (service.NeedUpdate)
            //        {
            //            _LOG.Debug("更新服务[{0}] r{1} -> r{2}", service.Name, service.Revision, service.RevisionOnServer);
            //            item.Proxy.Update(service.Name);
            //        }
            //    }
            //}
            foreach (var service in Selected)
            {
                var maintainer = Maintainer.Find(service.Name);
                if (service.NeedUpdate)
                {
                    _LOG.Debug("更新服务[{0}] r{1} -> r{2}", service.Name, service.Revision, service.RevisionOnServer);
                    maintainer.Proxy.Update(service.Name);
                }
            }
        }
        else
        {
            // 没有选中服务则更新所有服务器版本
            foreach (var item in managed)
                item.Proxy.UpdateSVN();
        }
    }
    void B同服务器_Clicked(UIElement sender, Entry e)
    {
        var service = Selected.FirstOrDefault();
        if (service == null)
            return;
        Maintainer maintainer = Maintainer.Find(service);
        foreach (var item in scenes)
        {
            if (!item.CB单选.Checked)
            {
                var temp = Maintainer.Find(item.Service);
                if (maintainer.Name == temp.Name)
                    item.CB单选.Checked = true;
            }
        }
    }
    void B同类型_Clicked(UIElement sender, Entry e)
    {
        var service = Selected.FirstOrDefault();
        if (service == null)
            return;
        foreach (var item in scenes)
            if (!item.CB单选.Checked && item.Service.Type == service.Type)
                item.CB单选.Checked = true;
    }
    void B全选_Clicked(UIElement sender, Entry e)
    {
        bool result = false;
        foreach (var item in scenes)
        {
            if (!item.CB单选.Checked)
            {
                result = true;
                break;
            }
        }

        foreach (var item in scenes)
            item.CB单选.Checked = result;
    }
    void B删除服务_Clicked(UIElement sender, Entry e)
    {
        S确认对话框.Confirm(Text.ETextID.DeleteConfirm,
            (result) =>
            {
                if (!result)
                    return;
                foreach (var service in Selected)
                {
                    if (service.Status == EServiceStatus.Stop)
                    {
                        var server = Maintainer.Find(service);
                        server.Proxy.DeleteService(service.Name);
                    }
                }
            });
    }
    void B新设更改服务_Clicked(UIElement sender, Entry e)
    {
        var selected = Selected.ToArrayAfterCount();
        if (selected.Length == 0)
        {
            Entry.ShowDialogScene<S新开服菜单>().Show(managed, null);
        }
        else
        {
            if (selected.Length > 1)
                S确认对话框.Hint(Text.ETextID.Single);
            else
                Entry.ShowDialogScene<S新开服菜单>().Show(managed, selected[0]);
        }
    }

    void S服务管理面板_PhaseEnding(UIScene obj)
    {
        Close();
    }
    void Close()
    {
        foreach (var scene in scenes)
            scene.Close(true);
        scenes.ClearToFree();
        P服务信息面板.Clear();
    }
    internal void Show(IEnumerable<Maintainer> managed)
    {
        Close();

        this.managed = managed;

        float x = TB服务信息面板.X;
        float y = TB服务信息面板.Y;
        float space = TB服务信息面板2.Y - y;
        int index = 0;
        foreach (var service in Managed)
        {
            var scene = scenes.Allot();
            if (scene == null)
            {
                scene = new S服务信息();
                scenes.Add(scene);
            }

            scene.X = x;
            scene.Y = y + index * space;
            scene.Service = service;

            P服务信息面板.Add(scene);
            Entry.ShowDialogScene(scene, EState.Break);

            index++;
        }

        IManagerAgent._OnGetServices.Callback = Refresh;
    }
    void Refresh()
    {
        if (Entry == null)
            return;
        Close();
        Show(managed);
    }
    
    private IEnumerable<ICoroutine> MyLoading()
    {
        bool security = managed.All(m => m.Security >= ESecurity.Maintainer);
        B全选.Visible = security;
        B新设更改服务.Visible = security;
        B删除服务.Visible = security;
        B更新服务.Visible = security;
        B关闭服务.Visible = security;
        B开启服务.Visible = security;
        B执行命令.Visible = security;
        return null;
    }

    protected override void InternalUpdate(Entry e)
    {
        base.InternalUpdate(e);

        L服务个数.Text = string.Format("{0} / {1}",
            managed.Sum(m => m.Services.Where(s => s.Status != EServiceStatus.Stop).Count()),
            managed.Sum(m => m.Services.Count()));
    }
}
