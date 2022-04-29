using System.Collections.Generic;
using System.Linq;
using EntryEngine;
using EntryEngine.UI;
using LauncherManager;
using LauncherProtocolStructure;

public partial class S服务器信息 : UIScene
{
    public Server Server;
    Maintainer Maintainer;

    public S服务器信息()
    {
        Initialize();
        BUpdate.Clicked += new DUpdate<UIElement>(BUpdate_Clicked);
    }

    void BUpdate_Clicked(UIElement sender, Entry e)
    {
        Maintainer.Find(Server).Proxy.UpdateSVN();
    }

    internal void Show(Server server)
    {
        this.Server = server;
        this.Maintainer = Maintainer.Find(Server);
        BUpdate.Visible = Maintainer.Security != LauncherManagerProtocol.ESecurity.Programmer;
    }
    private IEnumerable<ICoroutine> MyLoading()
    {
        LIP.Text = Server.EndPoint.ToString();
        return null;
    }

    protected override void InternalUpdate(Entry e)
    {
        base.InternalUpdate(e);

        L服务.Text = string.Format("{0} / {1}",
            Server.Services.Where(s => s.Status != EServiceStatus.Stop).Count(),
            Server.Services.Length);

        //if (e.GameTime.TickSecond && e.GameTime.Second % 5 == 0)
        //{
        //    Maintainer.Proxy.GetServerStatusStatistic(Server.ID,
        //        data =>
        //        {
        //            L内存.Text = data.Memory + " MB";
        //            L人数.Text = data.Connections.ToString();
        //            L网络.Text = (data.Network / 1024) + " kb/s";
        //            LCPU.Text = data.Cpu + "%";
        //            L硬盘.Text = (data.Disk / 1024) + " kb/s";
        //        });
        //}
    }
}
