using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;
using LauncherProtocolStructure;
using LauncherManager;

public partial class S服务类型管理信息 : UIScene
{
    public ServiceType Type;

    public S服务类型管理信息()
    {
        Initialize();
        B修改.Clicked += new DUpdate<UIElement>(B修改_Clicked);
        B删除.Clicked += new DUpdate<UIElement>(B删除_Clicked);
    }

    void B删除_Clicked(UIElement sender, Entry e)
    {
        S确认对话框.Confirm("确认要删除选中的项吗？",
            (result) =>
            {
                if (result)
                {
                    foreach (var item in Maintainer.Maintainers)
                        item.Proxy.DeleteServiceType(Type.Name);
                }
            });
    }
    void B修改_Clicked(UIElement sender, Entry e)
    {
        Entry.Instance.ShowDialogScene<S新建服务类型>().Type = Type;
    }
    
    private IEnumerable<ICoroutine> MyLoading()
    {
        L服务类型名字.Text = Type.Name;
        L服务地址.Text = Type.SVNPath;
        return null;
    }
}
