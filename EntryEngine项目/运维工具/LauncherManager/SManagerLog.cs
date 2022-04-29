using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;

public partial class SManagerLog : UIScene
{
    const byte FADE = 10;
    const byte MASK = 160;
    TIME time = new TIME(3000);

    Pool<Label> pool = new Pool<Label>();
    Queue<Label> queue = new Queue<Label>();
    public bool FixedShow;

    public SManagerLog()
    {
        Initialize();
    }
    
    private IEnumerable<ICoroutine> MyLoading()
    {
        Background = TEXTURE.Pixel;
        return null;
    }
    /// <summary>
    /// 有出现过一只卡在Showing阶段，Color.A加不上去的BUG
    /// </summary>
    [Code(ECode.BUG)]
    protected override IEnumerable<ICoroutine> Showing()
    {
        Color.A = 0;
        while (Color.A < MASK)
        {
            Color.A = (byte)_MATH.Min(Color.A + FADE, MASK);
            yield return null;
        }
        time.Reset();
    }
    protected override IEnumerable<ICoroutine> Ending()
    {
        Color.A = MASK;
        while (Color.A > 0)
        {
            Color.A = (byte)_MATH.Nature(Color.A - FADE);
            yield return null;
        }
    }
    protected override void InternalEvent(Entry e)
    {
        base.InternalEvent(e);

        if (!FixedShow)
        {
            time.Update(e.GameTime);
            if (time.IsEnd)
                Close(false);
        }
    }

    public static void Log(string name, Record record)
    {
        var scene = Entry.Instance.ShowDialogScene<SManagerLog>(EState.None);

        var label = scene.pool.Allot();
        if (label == null)
        {
            label = scene.___LLog();
            label.Height = 0;
            scene.pool.Add(label);
        }

        label.Text = string.Format("[{0}{1}] {2}", 
            record.Time.ToString(Utility.DATETIME_FORMAT), 
            (string.IsNullOrEmpty(name) ? "" : string.Format(" [{0}] ", name)), 
            record.ToString());
        if (record.Level > 7)
            label.UIText.FontColor = S日志信息.LevelColor[4];
        else
            label.UIText.FontColor = S日志信息.LevelColor[record.Level % 4];
        scene.queue.Enqueue(label);
        scene.Add(label);

        int count = 0;
        float y = 0;
        var temp = scene.queue.ToArray();
        temp.Reverse();
        foreach (var item in temp)
        {
            item.Y = y;
            y += item.TextContentSize.Y;
            count++;
            if (y > scene.Height)
                break;
        }
        while (count < scene.queue.Count)
            scene.Remove(scene.queue.Dequeue());
    }
}
