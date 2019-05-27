using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;
using LauncherManager;
using LauncherManagerProtocol;
using System.Linq;
using System;

public partial class S账号管理菜单 : UIScene
{
    private Pool<S账号管理信息> scenes = new Pool<S账号管理信息>();
    private IEnumerable<Maintainer> managed;
    private int sort;

    internal IEnumerable<Manager> Managed
    {
        get { return managed.SelectMany(m => m.Managers); }
    }
    internal IEnumerable<S账号管理信息> Selected
    {
        get
        {
            foreach (var item in scenes)
                if (item.CB单选.Checked)
                    yield return item;
        }
    }

    public S账号管理菜单()
    {
        Initialize();
        B关闭.Clicked += new DUpdate<UIElement>(B关闭_Clicked);
        B全选.Clicked += new DUpdate<UIElement>(B全选_Clicked);
        B同名.Clicked += new DUpdate<UIElement>(B同名_Clicked);
        B同平台.Clicked += new DUpdate<UIElement>(B同平台_Clicked);
        B新建.Clicked += new DUpdate<UIElement>(B新建_Clicked);
        B删除.Clicked += new DUpdate<UIElement>(B删除_Clicked);
        L平台.Clicked += new DUpdate<UIElement>(L平台_Clicked);
        L账号.Clicked += new DUpdate<UIElement>(L账号_Clicked);
        L权限.Clicked += new DUpdate<UIElement>(L权限_Clicked);
        PhaseEnding += new System.Action<UIScene>(S账号管理菜单_PhaseEnding);
    }

    void L权限_Clicked(UIElement sender, Entry e)
    {
        var array = P账号管理信息面板.Select(s => new { Manager = ((S账号管理信息)s).Manager, User = ((S账号管理信息)s).User }).ToArray();
        if (Sort(3))
            Array.Sort(array, (s1, s2) => s1.User.Security.CompareTo(s2.User.Security));
        else
            Array.Sort(array, (s1, s2) => s2.User.Security.CompareTo(s1.User.Security));

        var array2 = P账号管理信息面板.Select(s => (S账号管理信息)s).ToArray();
        for (int i = 0; i < array.Length; i++)
        {
            array2[i].Manager = array[i].Manager;
            array2[i].User = array[i].User;
        }
    }
    void L账号_Clicked(UIElement sender, Entry e)
    {
        var array = P账号管理信息面板.Select(s => new { Manager = ((S账号管理信息)s).Manager, User = ((S账号管理信息)s).User }).ToArray();
        if (Sort(1))
            Array.Sort(array, (s1, s2) => s1.User.Name.CompareTo(s2.User.Name));
        else
            Array.Sort(array, (s1, s2) => s2.User.Name.CompareTo(s1.User.Name));

        var array2 = P账号管理信息面板.Select(s => (S账号管理信息)s).ToArray();
        for (int i = 0; i < array.Length; i++)
        {
            array2[i].Manager = array[i].Manager;
            array2[i].User = array[i].User;
        }
    }
    void L平台_Clicked(UIElement sender, Entry e)
    {
        var array = P账号管理信息面板.Select(s => new { Manager = ((S账号管理信息)s).Manager, User = ((S账号管理信息)s).User }).ToArray();
        if (Sort(2))
            Array.Sort(array, (s1, s2) => s1.Manager.Platform.CompareTo(s2.Manager.Platform));
        else
            Array.Sort(array, (s1, s2) => s2.Manager.Platform.CompareTo(s1.Manager.Platform));

        var array2 = P账号管理信息面板.Select(s => (S账号管理信息)s).ToArray();
        for (int i = 0; i < array.Length; i++)
        {
            array2[i].Manager = array[i].Manager;
            array2[i].User = array[i].User;
        }
    }
    void B删除_Clicked(UIElement sender, Entry e)
    {
        if (!scenes.Any(s => s.CB单选.Checked))
        {
            S确认对话框.Hint("必须选中至少一项");
            return;
        }

        S确认对话框.Confirm("确认要删除选中的项吗？",
            (result) =>
            {
                if (!result)
                    return;
                foreach (var item in Selected)
                    item.Manager.Proxy.DeleteManager(item.User.Name);
            });
    }
    void B新建_Clicked(UIElement sender, Entry e)
    {
        Entry.ShowDialogScene<S新建管理账号菜单>(EState.Dialog).Managed = managed;
    }
    void B同平台_Clicked(UIElement sender, Entry e)
    {
        var selected = Selected.Select(s => s.Manager.Platform).Distinct().ToArray();
        foreach (var item in scenes)
            if (!item.CB单选.Checked && selected.Contains(item.Manager.Platform))
                item.CB单选.Checked = true;
    }
    void B同名_Clicked(UIElement sender, Entry e)
    {
        var selected = Selected.Select(s => s.User.Name).Distinct().ToArray();
        foreach (var item in scenes)
            if (!item.CB单选.Checked && selected.Contains(item.User.Name))
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
    void B关闭_Clicked(UIElement sender, Entry e)
    {
        Close(true);
    }

    bool Sort(byte type)
    {
        if (_MATH.Abs(sort) == type)
        {
            sort = -sort;
            return sort > 0;
        }
        else
        {
            sort = type;
            return true;
        }
    }
    void S账号管理菜单_PhaseEnding(UIScene obj)
    {
        Close();
    }
    void Close()
    {
        foreach (var scene in scenes)
            scene.Close(true);
        scenes.ClearToFree();
        P账号管理信息面板.Clear();
    }
    internal void Show(IEnumerable<Maintainer> managed)
    {
        Close();

        this.managed = managed;
        
        float x = TB账号管理信息1.X;
        float y = TB账号管理信息1.Y;
        float space = TB账号管理信息2.Y - y;
        int index = 0;
        foreach (var manager in managed)
        {
            if (manager.Managers != null)
            {
                foreach (var user in manager.Managers)
                {
                    var scene = scenes.Allot();
                    if (scene == null)
                    {
                        scene = new S账号管理信息();
                        scenes.Add(scene);
                    }

                    scene.X = x;
                    scene.Y = y + index * space;
                    scene.Manager = manager;
                    scene.User = user;

                    P账号管理信息面板.Add(scene);
                    Entry.ShowDialogScene(scene, EState.Break);

                    index++;
                }
            }
        }

        IManagerAgent._OnGetManagers.Callback = Refresh;
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
        return null;
    }
}
