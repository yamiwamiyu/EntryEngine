using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;
using LauncherManagerProtocol;

public partial class S日志信息 : UIScene
{
    public static COLOR[] LevelColor =
    {
        COLOR.Gray,
        COLOR.Black,
        COLOR.Orange,
        COLOR.Red,
        COLOR.CornflowerBlue,
    };
    public LogRecord Record;
    public int Index;

    public S日志信息()
    {
        Initialize();
        L日志内容.Height = 0;
        L日志内容.CanSelect = true;
    }
    
    private IEnumerable<ICoroutine> MyLoading()
    {
        //L时间.Text = Record.Record.Time.ToString("yyyy-MM-dd HH:mm:ss");
        //L日志内容.Text = Record.Record.ToString();
        //if (Record.Count > 1)
        //    L整合条数.Text = Record.Count.ToString();
        //else
        //    L整合条数.Text = null;

        //if (Record.Record.Level > 3)
        //    L日志内容.UIText.FontColor = LevelColor[4];
        //else
        //    L日志内容.UIText.FontColor = LevelColor[Record.Record.Level];
        return null;
    }
}
