using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEditor;
using EntryEngine;
using EntryEngine.Serialize;
using System.IO;
using EntryEngine.UI;

#region data structures

class VIEW
{
    public float Angle;
    public float Scale = 1;
    public float OffsetX;
    public float OffsetY;

    public MATRIX2x3 GetTransform()
    {
        return MATRIX2x3.CreateTransform(_MATH.ToRadian(Angle), Scale, Scale, OffsetX, OffsetY);
    }
    public void Reset()
    {
        Angle = 0;
        Scale = 1;
        OffsetX = 0;
        OffsetY = 0;
    }
}

#endregion

public partial class EditorParticle : SceneEditorEntry
{
    const string CONSTANT = "C.xml";
    public static COLOR[] TypeColors =
        {
            new COLOR(0, 0, 255, 255),
            new COLOR(0, 255, 0, 255),
            new COLOR(255, 0, 0, 255),
            new COLOR(0, 255, 255, 255),
            new COLOR(255, 255, 0, 255),
            // 项目内已编辑好的粒子系统
            COLOR.CornflowerBlue,
        };
    const string PREVIEW = "Preview\\";
    const string PARTICLE_FLOW = "pf";

    #region ui class

    class FLOW : UIScene
    {
        static PATCH patchFlow = PATCH.GetNinePatch(new COLOR(244, 244, 244, 128), new COLOR(32, 222, 128, 255), 2);
        static PATCH patch = PATCH.GetNinePatch(new COLOR(64, 64, 64, 255), new COLOR(222, 222, 222, 255), 1);
        static PATCH patchHover = PATCH.GetNinePatch(new COLOR(64, 64, 64, 255), new COLOR(32, 64, 222, 255), 1);
        static PATCH patchClicked = PATCH.GetNinePatch(new COLOR(92, 92, 92, 128), new COLOR(222, 16, 32, 222), 2);
        const string FLOW_NAME = "#FLOW";

        public ParticleEmitter Emitter;

        public FLOW() : this(new ParticleEmitter() { Flow = new List<ParticleStream>() } )
        {
        }
        public FLOW(ParticleEmitter emitter)
        {
            Background = patchFlow;
            //DragMode = EDragMode.Move;
            this.Width = 190;
            this.Height = 0;
            Name = FLOW_NAME;
            //Enable = false;
            this.Emitter = emitter;
            this.IsClip = false;
        }

        public void AddPS(ParticleStream stream)
        {
            AddPS(Childs.Count, stream);
        }
        public void AddPS(int index, ParticleStream stream)
        {
            _single[0] = stream;
            AddPS(index, _single);
        }
        public void AddPS(IList<ParticleStream> streams)
        {
            AddPS(Childs.Count, streams);
        }
        public void AddPS(int index, IList<ParticleStream> streams)
        {
            AddPS(index, streams, true);
        }
        private void AddPS(int index, IList<ParticleStream> streams, bool refresh)
        {
            for (int i = 0; i < streams.Count; i++)
            {
                var stream = streams[i];
                var key = stream.GetType().FullName;
                var item = _TABLE._PF.FirstOrDefault(pf => pf.TypeName.StartsWith(key));

                CheckBox container = new CheckBox();
                container.Name = FLOW_NAME;
                container.IsRadioButton = false;
                //container.CheckedOverlayNormal = true;
                container.SourceNormal = patch;
                container.SourceHover = patchHover;
                container.SourceClicked = patchClicked;
                container.X = 20;
                container.Width = 140;
                container.Height = 30;
                container.UIText.FontColor = COLOR.Red;
                //container.UIText.TextAlignment = EPivot.MiddleLeft;
                container.UIText.Padding.X = -5;
                container.Tag = stream;
                //container.Enable = false;
                container.DrawAfterBegin = DrawLine;

                container.Clicked -= container.OnClicked;

                TextureBox type = new TextureBox();
                type.X = 2;
                type.Y = 2;
                type.Width = 26;
                type.Height = 26;
                type.Texture = TEXTURE.Pixel;
                type.Color = EditorParticle.TypeColors[item.Type];

                Label label = new Label();
                label.X = 32;
                label.Y = 2;
                label.Width = 136;
                label.Height = 26;
                label.UIText.TextAlignment = EPivot.MiddleLeft;
                label.Text = item.Name;

                container.Add(type);
                container.Add(label);
                Insert(container, index++);

                //if (stream.Child == 0)
                //    break;
                //start += stream.Child;

                if (stream.Child > 0)
                {
                    // 新建子流
                    FLOW child = new FLOW(Emitter);
                    child.AddPS(0, streams.Skip(i + 1).Take(stream.Child).ToArray(), false);
                    container.Add(child);
                    i += stream.Child;
                }
            }
            if (refresh)
            {
                RefreshEmitter();
            }
        }
        public void RefreshEmitter()
        {
            UIElement temp = this;
            while (temp.Parent != null && temp.Parent != _this.PViewPF)
                temp = temp.Parent;
            FLOW top = temp as FLOW;
            top.Emitter.Flow.Clear();
            RefreshParticleStream(top);
        }

        static void DrawLine(UIElement sender, GRAPHICS sb, Entry e)
        {
            if (sender.ChildCount > 2)
            {
                var view1 = sender.ViewClip;
                var view2 = sender[2].ViewClip;
                sb.Draw(TEXTURE.Pixel, new RECT(view1.X + view1.Width, view1.Y + view1.Height * 0.5f, view2.X - view1.X, 2), GRAPHICS.NullSource, new COLOR(0, 255, 20, 128));
            }
        }
        static int SetChildPos(UIElement next, int i)
        {
            if (next.ChildCount == 2)
                return 0;
            int current = next.ChildCount;
            int max = 0;
            for (int j = i + 1; j < i + current; j++)
            {
                if (j >= next.Parent.ChildCount)
                    break;
                var c = next.Parent[j];
                if (c.ChildCount > 2)
                {
                    int value = SetChildPos(c, j);
                    if (value > max)
                        max = value;
                }
            }
            if (max >= current)
                current = max + 1;
            else if (max < current - 1)
                current = max + 1;
            return current;
        }
        static byte RefreshParticleStream(FLOW flow)
        {
            byte count = 0;
            for (int i = 0; i < flow.Childs.Count; i++)
            {
                var child = flow.Childs[i];
                child.Y = 30 * i;
                var stream = (ParticleStream)child.Tag;
                ((CheckBox)child).Text = flow.Emitter.Flow.Count.ToString();
                //((CheckBox)child).Text = "255";
                stream.Child = 0;
                flow.Emitter.Flow.Add(stream);

                if (child.ChildCount > 2)
                {
                    FLOW next = (FLOW)child[2];
                    if (next.ChildCount > 255)
                        throw new ArgumentOutOfRangeException("Child's count over the max value.");
                    next.X = child.Parent.Width - child.X + child.Parent.Width * (SetChildPos(child, i) - 1);
                    stream.Child += RefreshParticleStream(next);
                    count += stream.Child;
                }
                else
                    count++;
            }
            return count;
        }
        public static void ForAllPS(Action<CheckBox, ParticleStream> action)
        {
            for (int i = 0; i < _this.PViewPF.ChildCount; i++)
                ForAllPS((FLOW)_this.PViewPF[i], action);
        }
        public static void ForAllPS(FLOW flow, Action<CheckBox, ParticleStream> action)
        {
            for (int i = 0; i < flow.Childs.Count; i++)
            {
                var child = flow.Childs[i];
                var stream = (ParticleStream)child.Tag;
                action((CheckBox)child, stream);
                if (child.ChildCount > 2)
                {
                    FLOW next = (FLOW)child[2];
                    ForAllPS(next, action);
                }
            }
        }
        public static IEnumerable<ParticleStream> AllStream(FLOW flow)
        {
            for (int i = 0; i < flow.Childs.Count; i++)
                foreach (var item in AllStream((CheckBox)flow.Childs[i]))
                    yield return item;
        }
        public static IEnumerable<ParticleStream> AllStream(CheckBox cb)
        {
            yield return (ParticleStream)cb.Tag;
            if (cb.ChildCount > 2)
            {
                FLOW next = (FLOW)cb[2];
                foreach (var item in AllStream(next))
                    yield return item;
            }
        }
        public static IEnumerable<ParticleStream> AllStream(IList<ParticleStream> flow, ParticleStream stream)
        {
            int index = flow.IndexOf(stream);
            if (index == -1)
                yield break;
            for (int i = index; i <= index; i++)
            {
                yield return flow[i];
                if (stream.Child == 0)
                    break;
                index += stream.Child;
            }
        }
    }

    class ODelete : IOperation
    {
        internal UIElement stream;
        internal UIElement parent;
        internal int index;

        public ODelete()
        {
        }
        public ODelete(UIElement stream)
        {
            CheckBox cb = stream as CheckBox;
            if (cb != null && cb.Parent.ChildCount == 1)
                stream = cb.Parent;
            this.stream = stream;
            this.parent = stream.Parent;
            this.index = parent.IndexOf(stream);
        }

        public void Redo()
        {
            parent.Remove(stream);
            if (parent == _this.PViewPF)
                _this.ps.RemoveEmitter(((FLOW)stream).Emitter);
            else
            {
                FLOW flow = stream as FLOW;
                if (flow == null)
                    ((FLOW)parent).RefreshEmitter();
                else
                    ((FLOW)parent.Parent).RefreshEmitter();
            }
        }
        public void Undo()
        {
            parent.Insert(stream, index);
            if (parent == _this.PViewPF)
                _this.ps.AddEmitter(((FLOW)stream).Emitter);
            else
            {
                FLOW flow = stream as FLOW;
                if (flow == null)
                    ((FLOW)parent).RefreshEmitter();
                else
                    ((FLOW)parent.Parent).RefreshEmitter();
            }
        }
    }
    class ONew : IOperation
    {
        public ODelete Delete;
        public ODelete Add;

        public FLOW Flow;
        public int Index;
        public ParticleStream[] Stream;

        public void Redo()
        {
            Flow.AddPS(Index, Stream);
            if (Add != null)
                Add.Undo();
            if (Delete != null)
                Delete.Redo();
        }
        public void Undo()
        {
            for (int i = 0; i < Stream.Length; i++)
            {
                Flow.Remove(Index);
                i += Stream[i].Child;
            }
            if (Delete != null)
                Delete.Undo();
            if (Add != null)
                Add.Redo();
        }
    }

    #endregion


    static EditorParticle _this;
    static ParticleStream[] _single = new ParticleStream[1];
    OperationLog ol = new OperationLog();
    string saved;
    ParticleSystem ps;

    internal static EditMode edit;
    static UIElement selectedUI;
    static bool removeUI;
    static FLOW flow;
    VIEW vflow = new VIEW();
    VIEW vdisplay = new VIEW();
    VECTOR2 psPos;


    public override string DirectoryProject
    {
        get { return _C.Directory; }
        protected set { _C.Directory = value; }
    }

    public EditorParticle()
    {
        _this = this;
        Initialize();

        PViewProperty.DragMode = EDragMode.Drag;
        this.PhaseLoading += (e, content) => content.RootDirectory = "Content\\";
    }

    private IEnumerable<ICoroutine> MyLoading()
    {
        if (File.Exists(CONSTANT))
            new XmlReader(File.ReadAllText(CONSTANT)).ReadStatic(typeof(_C));
        UtilityEditor.OnExit += () =>
        {
            File.WriteAllText(CONSTANT, new XmlWriter().WriteStatic(typeof(_C)));
        };

        Content = Entry.NewContentManager();
        Content.AddPipeline(new PipelineParticle());
        if (!string.IsNullOrEmpty(_C.Directory))
            TBDirectory.Text = _IO.DirectoryWithEnding(Path.GetFullPath(_C.Directory));
        Content.RootDirectory = TBDirectory.Text;
        _LANGUAGE.Load(_IO.ReadText("Content\\LANGUAGE.csv"), "");
        _TABLE.Load("Content\\");

        EditorVariable.WIDTH = 100;
        EditorVariable.HEIGHT = 21;
        EditorVariable.CONTENT = Content;
        EditorVariable.GENERATOR.Generate = Generate;
        EditorVariable.GENERATOR.OnGenerated += GENERATOR_OnGenerated;
        EditorCommon.WIDTH = 80;

        #region public fields

        EditMode.FONT = FONT.Default;

        #endregion

        #region button event
        
        BNew.Clicked += new DUpdate<UIElement>(BNew_Clicked);
        BOpen.Clicked += new DUpdate<UIElement>(BOpen_Clicked);
        BSave.Clicked += new DUpdate<UIElement>(BSave_Clicked);
        BUndo.Clicked += new DUpdate<UIElement>(BUndo_Clicked);
        BRedo.Clicked += new DUpdate<UIElement>(BRedo_Clicked);
        TBDirectory.Clicked += new DUpdate<UIElement>(TBDirectory_Clicked);
        CBPlay.Hover += new DUpdate<UIElement>(CBPlay_Hover);
        BBack.Hover += new DUpdate<UIElement>(BBack_Hover);
        BForward.Hover += new DUpdate<UIElement>(BForward_Hover);
        TBDuration.Blur += new DUpdate<UIElement>(TBDuration_Blur);

        #endregion

        #region panel of particle flow & preview

        flow = new FLOW();
        PViewPF.Add(flow);
        //PViewPF.BackgroundFull = Content.Load<TEXTURE>("New.jpg");
        //PViewPF.Color = new COLOR(32, 64, 64, 255);
        int row = 0;
        foreach (var item in _TABLE._PF)
        {
            SPF spf = new SPF();
            spf.PF = item;
            spf.X = spf.Width * (row / 4);
            spf.Y = spf.Height * (row % 4);
            spf.TBFlowType.Texture = TEXTURE.Pixel;
            spf.TBFlowType.Color = TypeColors[item.Type];
            spf.BFlowName.Text = item.Name;
            this.PViewPreview.Add(spf);
            row++;
        }

        // 预设文件夹内的预设粒子
        string[] previews = Directory.GetFiles(Path.Combine(DirectoryEditor, PREVIEW), "*." + PARTICLE_FLOW, SearchOption.AllDirectories);
        foreach (var file in previews)
        {
            SPF spf = new SPF();
            spf.X = spf.Width * (row / 4);
            spf.Y = spf.Height * (row % 4);
            spf.TBFlowType.Texture = TEXTURE.Pixel;
            spf.Preview = Load(file);
            spf.File = file;
            if (spf.Preview.Length > 1)
                spf.TBFlowType.Color = TypeColors[TypeColors.Length - 2];
            else
            {
                string t = spf.Preview[0].GetType().FullName;
                var item = _TABLE._PF.FirstOrDefault(pf => pf.TypeName.StartsWith(t));
                spf.TBFlowType.Color = TypeColors[item.Type];
            }
            spf.BFlowName.Text = Path.GetFileNameWithoutExtension(file);
            this.PViewPreview.Add(spf);
            row++;
        }

        //if (!string.IsNullOrEmpty(_C.Directory))
        //{
        //    previews = Directory.GetFiles(DirectoryEditor, "*." + PipelineParticle.FILE_TYPE, SearchOption.AllDirectories);
        //    foreach (var file in previews)
        //    {
        //        SPF spf = new SPF();
        //        spf.X = spf.Width * (row / 4);
        //        spf.Y = spf.Height * (row % 4);
        //        spf.TBFlowType.Texture = TEXTURE.Pixel;
        //        spf.File = file;
        //        spf.TBFlowType.Color = TypeColors[TypeColors.Length - 1];
        //        spf.BFlowName.Text = Path.GetFileNameWithoutExtension(file);
        //        this.PViewPreview.Add(spf);
        //        row++;
        //    }
        //}

        //PViewPreview.DragMode = EDragMode.Drag;
        PViewPreview.DragInertia = 1;

        #endregion

        #region panel of particle view


        #endregion

        New();
        SetTip();
        return null;
    }
    
    void TBDuration_Blur(UIElement sender, Entry e)
    {
        string text = TBDuration.Text;

        float duration;
        if (float.TryParse(text, out duration))
        {
            ps.Duration = duration;
        }
        else
        {
            TimeSpan span;
            if (TimeSpan.TryParse(text, out span))
            {
                ps.Duration = (float)span.TotalMilliseconds;
            }
        }

        TBDuration.Text = GetTimeDisplay(ps.Duration);
    }
    void BForward_Hover(UIElement sender, Entry e)
    {
        if (__INPUT.Pointer.ComboClick.IsKeyActive(50))
            ps.SetElapsed(_MATH.Min(ps.Elapsed + e.GameTime.ElapsedSecond, ps.Duration));
    }
    void BBack_Hover(UIElement sender, Entry e)
    {
        if (__INPUT.Pointer.ComboClick.IsKeyActive(50))
            ps.SetElapsed(_MATH.Nature(ps.Elapsed - e.GameTime.ElapsedSecond));
    }
    void CBPlay_Hover(UIElement sender, Entry e)
    {
        if (__INPUT.PointerIsClick(1))
            ps.Reset();
    }
    void TBDirectory_Clicked(UIElement sender, Entry e)
    {
        string dir = _C.Directory;
        dir = UtilityEditor.OpenFolder(null, TBDirectory.Text);
        if (!string.IsNullOrEmpty(dir))
        {
            dir = _IO.DirectoryWithEnding(dir);
            _C.Directory = _IO.RelativePath(dir, DirectoryEditor);
            TBDirectory.Text = dir;
        }
    }
    void BRedo_Clicked(UIElement sender, Entry e)
    {
        Redo();
    }
    void BUndo_Clicked(UIElement sender, Entry e)
    {
        Undo();
    }
    void BSave_Clicked(UIElement sender, Entry e)
    {
        Save();
    }
    void BOpen_Clicked(UIElement sender, Entry e)
    {
        Open();
    }
    void BNew_Clicked(UIElement sender, Entry e)
    {
        New();
    }

    EditorVariable Generate(IVariable variable)
    {
        if (variable.MemberInfo != null)
        {
            if (variable.Type == typeof(VECTOR2))
                return new EditorPoint();
            else if (variable.Type == typeof(RECT))
                return new EditorClip();
        }
        return null;
    }
    void editor_ValueChanged(EditorVariable sender)
    {
        ResetParticleStream();
    }
    void ReLayout(VECTOR2 size)
    {
        float y = 0;
        foreach (var item in PViewProperty)
        {
            item.Y = y;
            y += item.ContentSize.Y;
        }
    }
    void GENERATOR_OnGenerated(IVariable variable, EditorVariable ev)
    {
        if (variable.MemberInfo == null)
            return;
        ev.ValueChanged += editor_ValueChanged;
        ev.ContentSizeChanged += ReLayout;
        EditorCommon editor = new EditorCommon(ev);
        editor.Text = variable.VariableName;
    }

    void Redo()
    {
        ol.Redo();
        ResetParticleStream();
    }
    void Undo()
    {
        ol.Undo();
        ResetParticleStream();
    }
    void Delete()
    {
        if (flow.ChildCount == 0)
        {
            // 删除光标指向的流
            UIElement target = UIElement.FindChildPriority(PViewPF, null, ui => ui != PViewPF && ui.Name != null && ui.IsHover);
            if (target != null)
            {
                ol.Operate(new ODelete(target), true);
            }
            else
            {
                // 删除选中的流
                target = UIElement.FindChildPriority(PViewPF, null, ui => ui != PViewPF && ui.Name != null && ui is CheckBox && ((CheckBox)ui).Checked);
                if (target != null)
                {
                    ol.Operate(new ODelete(target), true);
                    PViewProperty.Clear();
                }
            }
        }
    }
    ByteRefWriter GetWriter()
    {
        //ByteRefWriter writer = new ByteRefWriter(SerializeSetting.DefaultSerializeProperty);
        ByteRefWriter writer = new ByteRefWriter();
        writer.OnSerialize = TEXTURE.Serializer;
        return writer;
    }
    ByteRefReader GetReader(byte[] buffer)
    {
        //ByteRefReader reader = new ByteRefReader(buffer, SerializeSetting.DefaultSerializeProperty);
        ByteRefReader reader = new ByteRefReader(buffer);
        reader.OnDeserialize = TEXTURE.Deserializer(Content, null);
        return reader;
    }
    ParticleStream[] Load(string file)
    {
        return GetReader(_IO.ReadByte(file)).ReadObject<ParticleStream[]>();
    }
    T Copy<T>(T obj)
    {
        var writer = GetWriter();
        writer.Write(obj);
        return GetReader(writer.GetRawBuffer()).ReadObject<T>();
    }
    void Save(object target, string file)
    {
        var writer = GetWriter();
        if (target is ParticleSystem)
        {
            target = new StructureParticleSystem((ParticleSystem)target);
        }
        writer.WriteObject(target, target.GetType());
        _IO.WriteByte(file, writer.GetBuffer());
    }
    void Save(ParticleStream[] stream)
    {
        string file = Path.Combine(DirectoryEditor, PREVIEW);
        if (UtilityEditor.SaveFile(ref file, PARTICLE_FLOW))
            Save(stream, file);
    }
    void Save()
    {
        if (!ol.HasModified)
            return;
        if (string.IsNullOrEmpty(saved))
            SaveAs();
        else
        {
            Save(ps, saved);
            ol.Save();
        }
    }
    void SaveAs()
    {
        if (string.IsNullOrEmpty(_C.Directory))
        {
            UtilityEditor.Message(_TABLE._TEXTByKey[TEXT.ETEXTKey.SaveAs].Value);
            return;
        }

        string file = TBDirectory.Text;
        if (!string.IsNullOrEmpty(saved))
            file += Path.GetFileName(saved);
        if (UtilityEditor.SaveFile(ref file, PipelineParticle.FILE_TYPE))
        {
            Save(ps, file);
            this.saved = file;
            ol.Save();
        }
    }
    void Open()
    {
        string file = TBDirectory.Text;
        if (UtilityEditor.OpenFile(ref file, PipelineParticle.FILE_TYPE))
            Open(file);
    }
    void Open(string file)
    {
        string relative = _IO.RelativePathForward(file, TBDirectory.Text);
        if (relative == null)
        {
            UtilityEditor.Message("必须打开工作目录内的文件");
            return;
        }
        ParticleSystem ps = Content.Load<ParticleSystem>(relative);
        if (this.ps.EmittersCount > 0)
        {
            // 加入粒子系统

        }
        else
        {
            // 打开粒子系统
            OnNew(ps);
            this.saved = file;
        }
        AddParticleSystem(ps, new VECTOR2(100, 100));
    }
    void AddParticleSystem(ParticleSystem ps, VECTOR2 pos)
    {
        var emitters = ps.Emitters;
        for (int i = 0; i < emitters.Length; i++)
        {
            var flow = new FLOW();
            flow.Location = pos;

            ONew op = new ONew();
            op.Flow = flow;
            op.Index = 0;
            op.Stream = emitters[i].Flow.ToArray();
            emitters[i].Flow.Clear();

            ODelete add = new ODelete();
            add.parent = PViewPF;
            add.index = PViewPF.ChildCount;
            add.stream = flow;
            op.Add = add;

            ol.Operate(op, true);
            pos.Y += flow.ChildClip.Bottom + 10;
        }
    }
    void New()
    {
        var ps = new ParticleSystem();
        ps.Duration = 10;
        OnNew(ps);
    }
    void OnNew(ParticleSystem ps)
    {
        PViewPF.Clear();
        PViewPF.Add(flow);
        saved = null;
        this.ps = ps;
        TBDuration.Text = GetTimeDisplay(ps.Duration);
    }
    void SetTip()
    {
        var tip = Entry.ShowDialogScene<Tip>();
        tip.Close(true);

        tip.SetTip(BNew, TEXT.ETEXTKey.New);
        tip.SetTip(BOpen, TEXT.ETEXTKey.Open);
        tip.SetTip(BSave, TEXT.ETEXTKey.Save);
        tip.SetTip(TBDirectory, TEXT.ETEXTKey.Directory);

        tip.SetTip(BUndo, TEXT.ETEXTKey.Undo);
        tip.SetTip(BRedo, TEXT.ETEXTKey.Redo);
        tip.SetTip(BMove, TEXT.ETEXTKey.Move);
        tip.SetTip(BDelete, TEXT.ETEXTKey.Delete);

        tip.SetTip(BHelp, TEXT.ETEXTKey.Help);
        tip.SetTip(TBDuration, TEXT.ETEXTKey.Duration);
        tip.SetTip(CBPlay, TEXT.ETEXTKey.Play);
        tip.SetTip(BBack, TEXT.ETEXTKey.Back);
        tip.SetTip(BForward, TEXT.ETEXTKey.Forward);
        tip.SetTip(LOffset, TEXT.ETEXTKey.Offset);
        tip.SetTip(LScale, TEXT.ETEXTKey.Scale);
        tip.SetTip(LParticleCount, TEXT.ETEXTKey.PCount);
        tip.SetTip(TBTimelineBottom, () =>
            {
                if (ps.Duration <= 0)
                    return "NaN";
                VECTOR2 pos = TBTimelineBottom.ConvertGraphicsToLocal(__INPUT.PointerPosition);
                return GetTimeDisplay(ps.Duration * pos.X / TBTimelineBottom.Width);
            });

        foreach (SPF item in PViewPreview)
        {
            if (item.Preview == null)
                tip.SetTip(item, item.PF.Explain);
            else
                tip.SetTip(item, string.Format(_TABLE._TEXTByKey[TEXT.ETEXTKey.Preview].Value, item.File));
        }

        this.Add(tip);
    }
    static string GetTimeDisplay(float ms)
    {
        //return TimeSpan.FromMilliseconds(ms).ToString();
        var time = TimeSpan.FromSeconds(ms);
        return string.Format("{3:00}:{0:00}:{1:00}.{2:000}", (int)time.Minutes, (int)time.TotalSeconds, (int)time.Milliseconds, (int)time.TotalHours);
    }
    void ResetParticleStream()
    {
        float time = ps.Elapsed;
        ps.Reset();
        ps.SetElapsed(time);
    }
    VECTOR2 GetViewPosition(VECTOR2 graphicsPos)
    {
        var clip = PViewDisplay.Clip;
        MATRIX2x3 matrix = vdisplay.GetTransform() * MATRIX2x3.CreateTranslation(clip.Center.X, clip.Center.Y);
        MATRIX2x3.Invert(ref matrix, out matrix);
        return VECTOR2.Transform(graphicsPos, matrix);
    }

    protected override void InternalEvent(Entry e)
    {
        base.InternalEvent(e);

        #region 快捷键

        if (__INPUT.Keyboard.IsClick(PCKeys.Delete))
        {
            Delete();
        }
        else if (__INPUT.Keyboard.IsClick(PCKeys.H))
        {
            vdisplay.Reset();
            psPos.X = 0;
            psPos.Y = 0;
        }
        else if (__INPUT.Keyboard.IsClick(PCKeys.Space))
        {
            if (__INPUT.Keyboard.Alt)
                ps.Reset();
            else
                CBPlay.Checked = !CBPlay.Checked;
        }
        else if (__INPUT.Keyboard.IsInputKeyPressed(PCKeys.Left))
        {
            ps.SetElapsed(_MATH.Nature(ps.Elapsed - e.GameTime.ElapsedSecond));
        }
        else if (__INPUT.Keyboard.IsInputKeyPressed(PCKeys.Right))
        {
            ps.SetElapsed(_MATH.Min(ps.Elapsed + e.GameTime.ElapsedSecond, ps.Duration));
        }
        if (__INPUT.Keyboard.Ctrl)
        {
            if (__INPUT.Keyboard.IsClick(PCKeys.Q))
            {
                New();
            }
            else if (__INPUT.Keyboard.IsClick(PCKeys.W))
            {
                Open();
            }
            else if (__INPUT.Keyboard.IsClick(PCKeys.S))
            {
                if (__INPUT.Keyboard.Shift)
                {
                    SaveAs();
                }
                else
                {
                    Save();
                }
            }
            else if (__INPUT.Keyboard.IsClick(PCKeys.Z))
            {
                Undo();
            }
            else if (__INPUT.Keyboard.IsClick(PCKeys.Y))
            {
                Redo();
            }
            else if (__INPUT.Keyboard.IsClick(PCKeys.D))
            {
                Delete();
            }
        }

        #endregion

        #region 选择预设或存在的流

        // 拖动预设面板
        if (!PViewDisplay.IsHover && __INPUT.MouseScrollWheelValue != 0)
        {
            PViewPreview.OffsetX += 100 * __INPUT.MouseScrollWheelValue;
        }
        PViewPreview.DragMode = __INPUT.KeyboardCtrl ? EDragMode.Drag : EDragMode.None;

        if (PViewPreview.IsHover)
        {
            if (__INPUT.PointerIsClick(0))
            {
                foreach (SPF item in PViewPreview)
                {
                    if (item.IsHover)
                    {
                        flow.Clear();
                        if (item.Preview != null)
                        {
                            flow.AddPS(Copy(item.Preview));
                        }
                        else
                        {
                            flow.AddPS(item.GetParticleStream());
                        }
                        break;
                    }
                }
            }
        }

        #endregion

        #region 操作粒子流

        // 取消放置或拖拽
        if (__INPUT.PointerIsClick(1))
        {
            flow.Clear();
        }

        flow.Visible = false;
        if (PViewPF.IsHover)
        {
            if (flow.ChildCount > 0)
            {
                if (__INPUT.PointerIsRelease(0))
                {
                    UIElement target = UIElement.FindChildPriority(PViewPF, ui => ui == flow || ui == selectedUI, ui => ui != PViewPF && ui.Name != null && ui.IsHover);
                    ONew operation = new ONew();
                    operation.Stream = flow.Emitter.Flow.ToArray();
                    if (target != null)
                    {
                        // 新建粒子流到目标处
                        if (target is FLOW)
                        {
                            // 添加到流的最后
                            operation.Flow = ((FLOW)target);
                            operation.Index = operation.Flow.ChildCount;
                        }
                        else
                        {
                            VECTOR2 pos = target.ConvertGraphicsToLocal(__INPUT.PointerPosition);
                            if (pos.Y < target.Height * 0.25f)
                            {
                                // 插入到目标前面
                                operation.Flow = ((FLOW)target.Parent);
                                operation.Index = target.Parent.IndexOf(target);
                            }
                            else if (pos.Y > target.Height * 0.75f)
                            {
                                // 插入到目标后面
                                operation.Flow = ((FLOW)target.Parent);
                                operation.Index = target.Parent.IndexOf(target) + 1;
                            }
                            else
                            {
                                if (target.ChildCount > 2)
                                {
                                    // 已经有子流则插入子流末尾
                                    operation.Flow = ((FLOW)target[2]);
                                    operation.Index = operation.Flow.ChildCount;
                                }
                                else
                                {
                                    // 没有子流则新建子流
                                    FLOW _f = (FLOW)target.Parent;

                                    FLOW nf = new FLOW(_f.Emitter);
                                    operation.Flow = nf;
                                    operation.Index = 0;

                                    ODelete add = new ODelete();
                                    add.parent = target;
                                    add.index = target.ChildCount;
                                    add.stream = nf;
                                    operation.Add = add;
                                }
                            }
                        }
                    } // END: target != null
                    else
                    {
                        if (selectedUI != null && selectedUI is FLOW && removeUI && selectedUI.Parent == PViewPF)
                        {
                            // 仅仅是移动了流位置
                            removeUI = false;
                            //selectedUI.Location += __INPUT.MouseClickPositionRelative;
                            selectedUI.Location = flow.Location;
                        }
                        else
                        {
                            var nf = new FLOW();
                            nf.Location = flow.Location;
                            //foreach (var item in flow)
                            //    nf.Add(item);
                            //nf.RefreshEmitter();

                            operation.Flow = nf;
                            operation.Index = 0;

                            ODelete add = new ODelete();
                            add.parent = PViewPF;
                            add.index = PViewPF.ChildCount;
                            add.stream = nf;
                            operation.Add = add;
                        }
                    }
                    if (selectedUI != null && removeUI)
                    {
                        ODelete add = new ODelete(selectedUI);
                        operation.Delete = add;
                    }
                    flow.Clear();
                    flow.Emitter.Flow.Clear();
                    removeUI = false;
                    selectedUI = null;
                    PViewPF.DragMode = EDragMode.Drag;

                    // 仅移动顶级流时不记录日志
                    if (operation.Flow != null)
                        ol.Operate(operation, true);
                } // end of pointer release
                else
                    // 防止放下之后仍然显示一帧
                    flow.Visible = true;
            }
            else
            {
                if (__INPUT.PointerComboClick.IsDoubleClick)
                {
                    // save particle stream
                    UIElement target = UIElement.FindChildPriority(PViewPF, null, ui => ui != PViewPF && ui.Name != null && ui.IsHover);
                    if (target != null)
                    {
                        FLOW _flow = target as FLOW;
                        if (_flow == null)
                        {
                            Save(FLOW.AllStream((CheckBox)target).ToArray());
                        }
                        else
                        {
                            Save(FLOW.AllStream(_flow).ToArray());
                        }
                    }
                }
                else if (__INPUT.PointerIsRelease(0))
                {
                    // select particle stream and show it's property on property panel
                    UIElement target = UIElement.FindChildPriority(PViewPF, null, ui => ui != PViewPF && ui.Name != null && ui.IsHover);
                    if (target != null)
                    {
                        CheckBox cb = target as CheckBox;
                        if (cb != null)
                        {
                            ParticleStream stream = (ParticleStream)cb.Tag;
                            PViewProperty.Clear();
                            SerializeSetting.DefaultSerializeAll.Serialize(stream.GetType(), stream,
                                v =>
                                {
                                    if (v.VariableName == "Skip" || v.VariableName == "Child")
                                        return;
                                    EditorVariable editor = EditorVariable.GENERATOR.GenerateEditor(v);
                                    PViewProperty.Add(editor);
                                });
                            int index = 0;
                            float y = 0;
                            foreach (var item in PViewProperty)
                            {
                                item.Width = PViewProperty.Width;
                                item.Y = y;

                                y += item.ContentSize.Y;
                                index++;
                            }

                            // 选中
                            cb.Checked = true;
                            // 所有对象取消点亮
                            // 若为点亮同一实例的对象也点亮
                            FLOW.ForAllPS((sender, _stream) =>
                            {
                                sender.Checked = _stream == stream;
                            });
                        }
                    }
                }
                else if (__INPUT.PointerComboClick.IsPressedTimeOver(150))
                {
                    // move or copy particle stream
                    UIElement target = UIElement.FindChildPriority(PViewPF, null, ui => ui != PViewPF && ui.Name != null && ui.IsHover);
                    if (target != null)
                    {
                        // Shift: copy particle flow
                        // Ctrl: refference particle flow instance
                        // None: move particle flow instance to other
                        FLOW _flow = target as FLOW;
                        if (_flow == null)
                        {
                            CheckBox cb = target as CheckBox;
                            var streams = FLOW.AllStream(cb).ToList();
                            if (__INPUT.Keyboard.Shift)
                            {
                                for (int i = 0; i < streams.Count; i++)
                                    streams[i] = Copy(streams[i]);
                            }
                            else if (__INPUT.Keyboard.Ctrl)
                            {
                                selectedUI = target;
                            }
                            else
                            {
                                selectedUI = target;
                                removeUI = true;
                            }
                            flow.AddPS(streams);
                        }
                        else
                        {
                            var streams = FLOW.AllStream(_flow).ToList();
                            if (__INPUT.Keyboard.Shift)
                            {
                                for (int i = 0; i < streams.Count; i++)
                                    streams[i] = Copy(streams[i]);
                            }
                            else if (__INPUT.Keyboard.Ctrl)
                            {
                                selectedUI = target;
                            }
                            else
                            {
                                selectedUI = target;
                                removeUI = true;
                            }
                            flow.AddPS(streams);
                        }
                    }
                }
            } // end of flow.ChildCount == 0
        }

        // 防止在上次的位置先显示一帧Flow
        if (flow.ChildCount > 0)
        {
            flow.Location = PViewPF.ConvertGraphicsToLocal(__INPUT.PointerPosition);
            PViewPF.DragMode = EDragMode.None;
        }

        #endregion

        #region 操作粒子系统视图

        if (TBTimelineBottom.IsClick && __INPUT.PointerIsPressed(0))
        {
            VECTOR2 pos = TBTimelineBottom.ConvertGraphicsToLocal(__INPUT.PointerPosition);
            ps.SetElapsed(_MATH.Clamp(pos.X, 0, TBTimelineBottom.Width) / TBTimelineBottom.Width * ps.Duration);
        }

        if (PViewDisplay.IsClick && __INPUT.PointerIsPressed(0) && edit == null)
        {
            var moved = __INPUT.Pointer.DeltaPosition;
            vdisplay.OffsetX += moved.X;
            vdisplay.OffsetY += moved.Y;
        }
        if (PViewDisplay.IsHover)
        {
            vdisplay.Scale = _MATH.Clamp(vdisplay.Scale - 0.1f * __INPUT.Mouse.ScrollWheelValue, 0.25f, 16f);
        }
        if (edit == null && PViewDisplay.IsHover && __INPUT.PointerIsPressed(1))
        {
            psPos = GetViewPosition(__INPUT.PointerPosition);
        }

        #endregion

        if (edit != null)
        {
            if ((!PViewDisplay.IsHover && __INPUT.PointerIsClick(0)) ||
                __INPUT.Keyboard.IsClick(PCKeys.Escape))
                edit.Cancel();
        }
    }
    protected override void InternalUpdate(Entry e)
    {
        base.InternalUpdate(e);

        if (CBPlay.Checked)
        {
            ps.Update(e.GameTime.ElapsedSecond);
            //ps.Update(e.GameTime.Elapsed);
            if (ps.IsEnd)
                ps.Reset();
        }
        else
            ps.Update(0);
        TBTime.Text = GetTimeDisplay(ps.Elapsed);

        if (ps.Duration <= 0)
            TBTimeline.Width = 0.5f;
        else
            TBTimeline.Width = 0.5f + TBTimelineBottom.Width * ps.Elapsed / ps.Duration;

        LOffset.Text = string.Format("{0:000}, {1:000}", vdisplay.OffsetX, vdisplay.OffsetY);
        LScale.Text = vdisplay.Scale.ToString("0.00");
        LParticleCount.Text = ps.Emitters.Sum(pe => pe.Count).ToString();
    }
    protected override void InternalDrawAfter(GRAPHICS spriteBatch, Entry e)
    {
        base.InternalDrawAfter(spriteBatch, e);

        // display
        if (ps != null)
        {
            var clip = PViewDisplay.Clip;
            MATRIX2x3 matrix = vdisplay.GetTransform() * MATRIX2x3.CreateTranslation(clip.Center.X, clip.Center.Y);

            spriteBatch.Begin(matrix, PViewDisplay.ViewClip);
            spriteBatch.Draw(ps, psPos);

            // edit mode
            if (edit != null)
            {
                spriteBatch.Draw(TEXTURE.Pixel, VECTOR2.Zero, GRAPHICS.NullSource, new COLOR(33, 33, 222, 64),
                    0, TEXTURE.Pixel.Center, clip.Size / matrix.Scale, EFlip.None);
                MATRIX2x3.Invert(ref matrix, out matrix);
                if (edit.Edit(ref matrix, spriteBatch, e))
                    edit.Cancel();
            }

            spriteBatch.End();
        }
    }
}
