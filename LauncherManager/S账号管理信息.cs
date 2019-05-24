using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;
using LauncherManager;
using LauncherManagerProtocol;

public partial class S账号管理信息 : UIScene
{
    internal Maintainer Manager;
    public Manager User;

    public S账号管理信息()
    {
        Initialize();
    }
    
    private IEnumerable<ICoroutine> MyLoading()
    {
        return null;
    }

    protected override void InternalUpdate(Entry e)
    {
        base.InternalUpdate(e);

        L账号.Text = User.Name;
        L密码.Text = User.Password;
        L权限.Text = User.Security.ToString();
        L平台.Text = string.Format("{0}-{1}", Manager.Platform, Manager.Link.EndPoint);
    }
}
