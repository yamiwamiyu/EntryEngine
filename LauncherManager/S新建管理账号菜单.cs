using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;
using LauncherProtocolStructure;
using LauncherManagerProtocol;
using LauncherManager;
using System.Linq;

public partial class S新建管理账号菜单 : UIScene
{
    internal IEnumerable<Maintainer> Managed;

    public S新建管理账号菜单()
    {
        Initialize();
        B确定.Clicked += new DUpdate<UIElement>(B确定_Clicked);
        B取消.Clicked += new DUpdate<UIElement>(B取消_Clicked);

        int admin = (int)ESecurity.Administrator;
        for (int i = 0; i < admin; i++)
        {
            var item = DD权限.DropDownList.AddItem(((ESecurity)i).ToString());
            item.UIText.FontColor = COLOR.Black;
        }
    }

    void B取消_Clicked(UIElement sender, Entry e)
    {
        Close(true);
    }
    void B确定_Clicked(UIElement sender, Entry e)
    {
        if (string.IsNullOrEmpty(TB账号.Text))
        {
            S确认对话框.Hint("账号名不能为空");
            return;
        }

        if (string.IsNullOrEmpty(TB密码.Text))
        {
            S确认对话框.Hint("密码不能为空");
            return;
        }

        Manager manager = new Manager();
        manager.Name = TB账号.Text;
        manager.Password = TB密码.Text;
        manager.Security = (ESecurity)DD权限.DropDownList.SelectedIndex;
        foreach (var item in Managed)
            if (item.Managers == null || !item.Managers.Any(m => m.Name == manager.Name))
                item.Proxy.NewManager(manager);

        Close(true);
    }

    private IEnumerable<ICoroutine> MyLoading()
    {
        DD权限.DropDownList.SelectedIndex = 0;
        TB账号.Text = null;
        TB密码.Text = null;
        return null;
    }
}
