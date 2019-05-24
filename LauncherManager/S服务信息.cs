using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;
using LauncherProtocolStructure;
using LauncherManager;

public partial class S服务信息 : UIScene
{
    public Service Service;

    public S服务信息()
    {
        Initialize();
    }

    private IEnumerable<ICoroutine> MyLoading()
    {
        L名字.Text = Service.Name;
        return null;
    }
    protected override void InternalUpdate(Entry e)
    {
        L服务版本.Text = string.Format("{0} - {1}", Service.Type,
            (Service.Revision < 0 ? -Service.Revision + _TABLE._TextByID[Text.ETextID.HotFix].Content : Service.Revision.ToString()));
        var maintainer = Maintainer.Find(Service);
        L服务器端口版本.Text = string.Format("{2} : {0} - {1}", maintainer.Link.EndPoint.ToString(), Service.RevisionOnServer, maintainer.Platform);
        L时间.Text = Service.LastStatusTime;
        L服务状态.Text = Service.Status.ToString();
    }
    protected override void InternalEvent(Entry e)
    {
        base.InternalEvent(e);

        if (IsHover && __INPUT.PointerComboClick.IsDoubleClick && !CB单选.IsHover)
        {
            Entry.Instance.ShowMainScene<S日志>().Show(Service);
        }
    }
}
