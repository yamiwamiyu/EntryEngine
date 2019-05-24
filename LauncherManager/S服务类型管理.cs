using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;
using LauncherManager;
using LauncherProtocolStructure;
using System.Linq;

public partial class S服务类型管理 : UIScene
{
    private Pool<S服务类型管理信息> scenes = new Pool<S服务类型管理信息>();
    private IEnumerable<Maintainer> managed;

    internal IEnumerable<ServiceType> Managed
    {
        get { return managed.SelectIntersection(m => m.ServiceTypes); }
    }
    //internal IEnumerable<Service> Selected
    //{
    //    get
    //    {
    //        foreach (var item in scenes)
    //            if (item.CB单选.Checked)
    //                yield return item.Service;
    //    }
    //}

    public S服务类型管理()
    {
        Initialize();
        this.PhaseEnding += new System.Action<UIScene>(S服务类型管理_PhaseEnding);

        B关闭.Clicked += new DUpdate<UIElement>(B关闭_Clicked);
        B新建.Clicked += new DUpdate<UIElement>(B新建_Clicked);
    }

    void B新建_Clicked(UIElement sender, Entry e)
    {
        Entry.ShowDialogScene<S新建服务类型>().Type = null;
    }
    void B关闭_Clicked(UIElement sender, Entry e)
    {
        IManagerAgent._OnGetServiceTypes.Callback = null;
        Close(true);
    }

    void S服务类型管理_PhaseEnding(UIScene obj)
    {
        Close();
    }
    void Close()
    {
        foreach (var scene in scenes)
            scene.Close(true);
        scenes.ClearToFree();
        P服务类型信息面板.Clear();
    }
    internal void Show(IEnumerable<Maintainer> managed)
    {
        this.managed = managed;

        float x = TB服务类管理信息.X;
        float y = TB服务类管理信息.Y;
        float space = TB服务类管理信息2.Y - y;
        int index = 0;
        foreach (var service in Managed)
        {
            var scene = scenes.Allot();
            if (scene == null)
            {
                scene = new S服务类型管理信息();
                scenes.Add(scene);
            }

            scene.X = x;
            scene.Y = y + index * space;
            scene.Type = service;

            P服务类型信息面板.Add(scene);
            Entry.ShowDialogScene(scene, EState.Break);

            index++;
        }

        IManagerAgent._OnGetServiceTypes.Callback = Refresh;
    }
    void Refresh()
    {
        Close();
        Show(managed);
    }

    private IEnumerable<ICoroutine> MyLoading()
    {
        return null;
    }
}
