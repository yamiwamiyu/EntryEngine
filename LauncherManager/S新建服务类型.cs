using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;
using LauncherProtocolStructure;
using LauncherManager;
using System.Linq;

public partial class S新建服务类型 : UIScene
{
    public ServiceType Type;

    bool New
    {
        get { return Type == null; }
    }

    public S新建服务类型()
    {
        Initialize();
        this.Background = TEXTURE.Pixel;
        this.Color = new COLOR(244, 244, 244, 244);
        B取消.Clicked += new DUpdate<UIElement>(B取消_Clicked);
        B确定.Clicked += new DUpdate<UIElement>(B确定_Clicked);
    }

    void B确定_Clicked(UIElement sender, Entry e)
    {
        if (string.IsNullOrEmpty(TB名字.Text))
        {
            S确认对话框.Hint("账号名不能为空");
            return;
        }

        if (string.IsNullOrEmpty(TBSVN目录.Text)
            || string.IsNullOrEmpty(TBSVN账号.Text)
            || string.IsNullOrEmpty(TBSVN密码.Text))
        {
            S确认对话框.Hint("SVN信息不能为空");
            return;
        }

        if (!TBSVN目录.Text.EndsWith("/"))
        {
            S确认对话框.Hint("SVN路径必须以'/'结尾");
            return;
        }

        //if (string.IsNullOrEmpty(TB启动文件.Text))
        //{
        //    S确认对话框.Hint("运行程序不能为空");
        //    return;
        //}

        ServiceType type = new ServiceType();
        type.Name = TB名字.Text;
        type.SVNPath = TBSVN目录.Text;
        type.SVNUser = TBSVN账号.Text;
        type.SVNPassword = TBSVN密码.Text;
        type.Exe = TB启动文件.Text;
        type.LaunchCommand = TB启动命令.Text;
        if (New)
            S确认对话框.Wait("正在等待服务器操作", NewServiceType(type));
        else
            S确认对话框.Wait("正在等待服务器操作", ModifyServiceType(Type.Name, type));

        Close(true);
    }
    void B取消_Clicked(UIElement sender, Entry e)
    {
        Close(true);
    }

    private IEnumerable<ICoroutine> ModifyServiceType(string name, ServiceType type)
    {
        foreach (var item in _DATA.Managed)
        {
            bool result = false;
            _LOG.Debug("平台[{0}]正在创建服务类型", item.Platform);

            // 等待处理完毕回调
            yield return item.Proxy.ModifyServiceType(name, type, r => result = r);

            _LOG.Debug("平台[{0}]创建服务类型完成[{1}]", item.Platform, result);
        }
    }
    private IEnumerable<ICoroutine> NewServiceType(ServiceType type)
    {
        foreach (var item in _DATA.Managed)
        {
            bool result = false;
            _LOG.Debug("平台[{0}]正在创建服务类型", item.Platform);

            // 等待处理完毕回调
            yield return item.Proxy.NewServiceType(type, r => result = r);

            _LOG.Debug("平台[{0}]创建服务类型完成[{1}]", item.Platform, result);
        }
    }
    private IEnumerable<ICoroutine> MyLoading()
    {
        if (New)
        {
            TB名字.Readonly = false;

            TB名字.Text = string.Empty;
            TBSVN目录.Text = string.Empty;
            TBSVN账号.Text = string.Empty;
            TBSVN密码.Text = string.Empty;
            TB启动文件.Text = string.Empty;
            TB启动命令.Text = string.Empty;
        }
        else
        {
            TB名字.Readonly = true;

            TB名字.Text = Type.Name;
            TBSVN目录.Text = Type.SVNPath;
            TBSVN账号.Text = Type.SVNUser;
            TBSVN密码.Text = Type.SVNPassword;
            TB启动文件.Text = Type.Exe;
            TB启动命令.Text = Type.LaunchCommand;
        }
        return null;
    }
}
