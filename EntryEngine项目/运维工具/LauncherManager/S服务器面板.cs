using System.Collections.Generic;
using System.Linq;
using EntryEngine;
using EntryEngine.UI;
using LauncherManager;
using LauncherManagerProtocol;
using LauncherProtocolStructure;

public partial class S服务器面板 : UIScene
{
    private Pool<S服务器信息> scenes = new Pool<S服务器信息>();
    private IEnumerable<Maintainer> managed;

    internal IEnumerable<Server> Managed
    {
        get { return managed.SelectMany(m => m.Servers); }
    }
    internal IEnumerable<Server> Selected
    {
        get
        {
            foreach (var item in scenes)
                if (item.CB单选.Checked)
                    yield return item.Server;
        }
    }

    public S服务器面板()
    {
        Initialize();
        this.PhaseEnding += new System.Action<UIScene>(S服务器面板_PhaseEnding);
        B数据统计.Clicked += new DUpdate<UIElement>(B数据统计_Clicked);
        B全选.Clicked += new DUpdate<UIElement>(B全选_Clicked);
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
    void B数据统计_Clicked(UIElement sender, Entry e)
    {
    }
    void S服务器面板_PhaseEnding(UIScene obj)
    {
        Close();
    }
    void Close()
    {
        foreach (var scene in scenes)
            scene.Close(true);
        scenes.ClearToFree();
        P服务器信息面板.Clear();
    }
    internal void Show(IEnumerable<Maintainer> managed)
    {
        Close();

        this.managed = managed;

        float x = TB服务器单选1.X;
        float y = TB服务器单选1.Y;
        float space = TB服务器单选2.Y - y;
        int index = 0;
        foreach (var server in Managed)
        {
            var scene = scenes.Allot();
            if (scene == null)
            {
                scene = new S服务器信息();
                scenes.Add(scene);
            }

            scene.X = x;
            scene.Y = y + index * space;
            scene.Show(server);

            P服务器信息面板.Add(scene);
            Entry.ShowDialogScene(scene, EState.Break);

            //scene.CB单选.Location = scene.CB单选.ConvertLocalToOther(VECTOR2.Zero, P服务器信息面板);
            //P服务器信息面板.Add(scene.CB单选);

            index++;
        }
    }
    
    private IEnumerable<ICoroutine> MyLoading()
    {
        B数据统计.Visible = managed.All(m => m.Security == ESecurity.Administrator);
        return null;
    }
}
