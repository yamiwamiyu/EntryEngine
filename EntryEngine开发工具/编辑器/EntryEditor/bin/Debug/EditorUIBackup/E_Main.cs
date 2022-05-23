using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using EntryEditor;
using EntryEngine;
using EntryEngine.Serialize;
using EntryEngine.UI;
using EntryEngine.Xna;
using BinaryReader = EntryEngine.Serialize.ByteReader;
using BinaryWriter = EntryEngine.Serialize.ByteWriter;

namespace EditorUI
{
    // Gate
    public partial class EditorUI : SceneEditorEntry
    {
        public const string SUFFIX_PROJECT = "eui";
        public const string SUFFIX_TEXTURE = "png";
        public const string SUFFIX_ELEMENT = "ui";
        public const string SUFFIX_CS = ".logic.cs";
        public const string SUFFIX_DESIGN_CS = ".design.cs";
        /// <summary>
        /// 输出的UI代码目录
        /// </summary>
        public const string DIR_UI = "UI";
        /// <summary>
        /// 控件预设文件及预览图目录
        /// </summary>
        public const string DIR_PREVIEW = "Preview";
        public static string[] DIRECTORY =
		{
            DIR_UI,
            DIR_PREVIEW
		};
        public const string FILE_CONSTANT = "C.xml";

        public new static EditorUI Instance
        {
            get { return (EditorUI)SceneEditorEntry.Instance; }
        }

        private ContentManager contentPreview;
        internal Project Project
        {
            get;
            private set;
        }

        public override string DirectoryProject
        {
            get
            {
                string value;
                if (Project == null)
                    value = Environment.CurrentDirectory;
                else
                    value = Path.GetFullPath(Path.GetDirectoryName(Project.Name));
                return _IO.DirectoryWithEnding(value);
            }
            protected set
            {
                if (Project == null)
                    Environment.CurrentDirectory = value;
                else
                    Project.Name = Path.GetFullPath(value);
            }
        }
        public static string DIR_CONTENT
        {
            get { return Instance.Content.RootDirectory; }
        }

        protected override IEnumerable<ICoroutine> Loading()
        {
            if (EditorContent != null)
                EditorContent.Dispose();
            if (contentPreview != null)
                contentPreview.Dispose();
            EditorContent = Entry.NewContentManager();
            EditorContent.RootDirectory = Path.GetFullPath("Content");
            Content = Entry.NewContentManager();
            contentPreview = Entry.NewContentManager();

            InitializeEditorResource();
            InitializeMenuStrip();
            InitializeProperty();
            InitializeElement();
            InitializeViewport();

            UtilityEditor.DragFiles = DragDropFiles;
            // open the last project
            if (Config<ConfigEditor>.Setting.LastOpenProject != null &&
                File.Exists(Config<ConfigEditor>.Setting.LastOpenProject.ProjectPath))
            {
                Project = OpenProject(Config<ConfigEditor>.Setting.LastOpenProject.ProjectPath);
                OnOpenProject();
            }

            pp.Background = null;

            return null;
        }
        private void CreateDefaultProjectDirectory(string root)
        {
            for (int i = 0; i < DIRECTORY.Length; i++)
                Directory.CreateDirectory(Path.Combine(root, DIRECTORY[i]));
        }
        private void SaveView()
        {
            if (view != null)
            {
                if (string.IsNullOrEmpty(Project.TranslateTable) ||
                    string.IsNullOrEmpty(C.EntryBuilder))
                {
                    UtilityEditor.Message("请先配置翻译文件和工具");
                    return;
                }
                view.Save();
            }
        }
        private void SaveProject()
        {
            XmlWriter writer = new XmlWriter();
            writer.Setting.Static = false;
            writer.WriteObject(Project);

            XmlReader reader = new XmlReader(writer.Result);
            XmlNode node = reader.ReadToNode();

            File.WriteAllText(Project.Name, node.OutterXml);
        }
        private void OnOpenProject()
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(Project.Name);
            contentPreview.IODevice.RootDirectory = Environment.CurrentDirectory;

            Content.RootDirectory = _IO.DirectoryWithEnding(Path.GetFullPath(Project.ContentDirectory));

            if (Project.Document != null)
                OpenFile(Project.Document.File);

            ProjectData data = new ProjectData();
            data.ProjectEditorName = GetType().BuildDllType();
            data.ProjectPath = Project.Name;
            Config<ConfigEditor>.Setting.OpenProject(data);
            LoadPreElements();
        }
        private void CloseProject()
        {
            if (Project != null)
            {
                Content.Dispose();
                SaveProject();
                CloseView();
                Project = null;
            }
        }
        private void CloseView()
        {
            if (view != null)
            {
                pvc.ContentScope = VECTOR2.Zero;
                pv.Clear();
                view = null;
            }
        }
        private void OpenFile(string file)
        {
        }
        private Project OpenProject(string file)
        {
            XmlReader reader = new XmlReader(File.ReadAllText(file));
            Project project = reader.ReadObject<Project>();
            if (project.Name != file)
            {
                project.Name = file;
            }
            return project;
        }

        private void DragDropFiles(string[] files)
        {
            VECTOR2 location = Entry.INPUT.Pointer.Position;
            FromFile container = (FromFile)UIElement.FindChildPriority(ppc, ViewFindSkip,
                ui => ui.IsContains(location) && ui is FromFile);

            foreach (string file in files)
            {
                if (container != null)
                {
                    container.DragFile(file);
                    continue;
                }

                string target = FindPreviewFile(file);
                if (target == null)
                {
                    UtilityEditor.Message("文件名错误", string.Format("在{0}目录的文件名格式必须是 文件名.UI类型.{1}", DIR_PREVIEW, SUFFIX_TEXTURE));
                    return;
                }

                try
                {
                    if (!target.EndsWith(SUFFIX_ELEMENT))
                        continue;

                    UIElement ui = LoadUI(target);

                    UIScene scene = ui as UIScene;
                    if (scene != null)
                    {
                        Project.Document = new Document(target);
                        BuildView(scene);
                        break;
                    }
                    else
                    {
                        scene = EditingScene;
                        if (scene == null)
                            continue;
                        Entry.INPUT.Update(Entry.Instance);
                        SetElement(ui, Entry.INPUT.Pointer.Position);
                    }
                }
                catch (Exception ex)
                {
                    _LOG.Error(ex, "LoadUI error! target={0}", target);
                }
            }
        }

        public static Action<UIElement> OnLoadUICompleted;

        private static UIElement WritingProperty;
        public static bool UISerializer(ByteRefWriter writer, object value, Type type)
        {
            if (WritingProperty != null || value == WritingProperty)
                return false;

            bool nil = value == null;
            UIElement element = null;
            if (!nil)
            {
                element = value as UIElement;
                if (element == null)
                    return false;
            }
            else if (!type.Is(typeof(UIElement)))
                return false;

            // 作为其它类型的属性时序列化其在父控件中的索引
            if (element == null)
                writer.Write(-1);
            else
                writer.Write(element.Parent.IndexOf(element));

            return true;
        }
        public static Func<Type, Func<ByteRefReader, object>> UIDeserializer(UIElement[] childs)
        {
            return (type) =>
                {
                    if (type == typeof(UIElement))
                        return null;

                    if (type.Is(typeof(UIElement)))
                    {
                        return (reader) =>
                        {
                            int index;
                            reader.Read(out index);
                            if (index == -1)
                                return null;
                            else
                                return childs[index];
                        };
                    }
                    else
                        return null;
                };
        }
        public static SerializeSetting GetSetting()
        {
            return new SerializeSetting()
            {
                Property = true,
                Filter = FILTER,
            };
        }
        public static SerializeSetting GetSetting(ElementLib lib)
        {
            return new SerializeSetting()
            {
                Property = true,
                Filter = new UIFilter() { Lib = lib },
            };
        }
        public static UIFilter FILTER = new UIFilter();
        public static byte[] SaveUI(Widget target, bool reff = false)
        {
            ByteRefWriter writer = new ByteRefWriter(GetSetting());
            writer.OnSerialize += TEXTURE.Serializer;
            writer.OnSerialize += EntryEngine.Content.Serializer;
            writer.OnSerialize += UISerializer;
            writer.Write(target.Target.Instance.GetType().SimpleAQName());
            InternalSaveUI(target, writer, reff);
            return writer.GetBuffer();
        }
        public static void InternalSaveUI(Widget target, ByteRefWriter writer, bool reff = false)
        {
            // refference
            writer.Write(reff && !target.IsSaveAs ? target.FilePath : null);

            UIElement element = target.Target.Instance as UIElement;
            //writer.Write(element.GetType().SimpleAQName());

            /* 放在属性前加载子控件，停靠的子控件在父控件设置宽高属性时将会变形 */
            writer.OnSerialize -= UISerializer;
            writer.Write(element.ChildCount);
            for (int i = 0; i < element.ChildCount; i++)
                InternalSaveUI((Widget)element[i].Tag, writer, true);

            writer.OnSerialize += UISerializer;
            WritingProperty = element;
            writer.WriteObject(element, typeof(UIElement));
            WritingProperty = null;
        }
        public static ByteRefReader GetReader(byte[] buffer)
        {
            SerializeSetting setting = SerializeSetting.DefaultSetting;
            setting.Property = true;
            ByteRefReader reader = new ByteRefReader(buffer, setting);
            reader.OnDeserialize += TEXTURE.Deserializer(Instance.Content, null);
            reader.OnDeserialize += FONT.Deserializer(Instance.Content, null);
            //reader.OnDeserialize += UIDeserializer;
            return reader;
        }
        public static UIElement LoadUI(string file)
        {
            ByteReader reader = new BinaryReader(File.ReadAllBytes(file));
            UIElement target = InternalLoadUI(reader);
            //target.Tag = file;
            (target.Tag as Widget).FilePath = file;
            return target;
        }
        public static Type LoadElementType(string file)
        {
            ByteReader reader = new BinaryReader(File.ReadAllBytes(file));
            string key;
            reader.Read(out key);
            reader.Read(out key);
            return UtilityEditor.GetDllType(key);
        }
        public static UIElement InternalLoadUI(BinaryReader reader, bool reff = false)
        {
            string file;
            reader.Read(out file);
            UIElement refference = null;
            if (reff && !string.IsNullOrEmpty(file) && File.Exists(file))
            {
                try
                {
                    refference = InternalLoadUI(new BinaryReader(File.ReadAllBytes(file)), false);
                    refference.Tag = file;
                }
                catch (Exception ex)
                {
                    _LOG.Error("Load file:{0} error:{1}!", file, ex.Message);
                }
            }

            string key;
            reader.Read(out key);
            Type type = UtilityEditor.GetDllType(key);

            UIElement instance;
            if (refference == null)
                instance = (UIElement)type.GetConstructor(Type.EmptyTypes).Invoke(new object[0]);
            else
                instance = refference;
            // editor should be built by instance
            Widget view = SetElement(instance);

            // childs
            //int count;
            //reader.Read(out count);
            //for (int i = 0; i < count; i++)
            //{
            //    var child = InternalLoadUI(reader, true);
            //    if (refference == null)
            //        instance.Add(child);
            //}

            // properties
            List<EditorVariable> editors = view.Target.GetEditors().ToList();
            while (true)
            {
                reader.Read(out key);

                if (key == null)
                    break;

                var variable = editors.FirstOrDefault(v => v.Variable.VariableName == key);
                if (variable != null)
                    if (refference != null && !LibUIElement.Variables.Contains(l => l.VariableName == key))
                        variable.LoadValue(null, reader);
                    else
                        //variable.Load(Instance.ContentManager);
                        variable.LoadValue(instance, reader);
            }

            var tempCallback = OnLoadUICompleted;
            OnLoadUICompleted = null;
            // childs
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                var child = InternalLoadUI(reader, true);
                if (refference == null)
                    instance.Add(child);
            }

            if (tempCallback != null)
                tempCallback(instance);

            return instance;
        }

        public static IEditorType BuildPropertyEditor(object instance)
        {
            IEditorType fix;
            Type type = instance.GetType();

            ElementLib lib;
            if (libs.TryGetValue(type, out lib))
            {
                type = UtilityEditor.GetDllType(lib.Type);

                if (lib.Variables.IsEmpty())
                {
                    fix = GetSuitableType(instance, type);
                }
                else
                {
                    EditorBaseType editorType = new EditorBaseType(instance);
                    foreach (var item in lib.Variables)
                    {
                        VariableObject variable = new VariableObject(instance, item.VariableName);

                        EditorVariable editor;

                        if (string.IsNullOrEmpty(item.EditorType))
                            editor = EditorUI.TypeEditorGenerator.GeneratorEditor(variable.Type, variable.GetValue());
                        else
                            editor = (EditorVariable)UtilityEditor.GetDllType(item.EditorType).GetConstructor(Type.EmptyTypes).Invoke(new object[0]);

                        if (!item.Properties.IsEmpty())
                        {
                            foreach (var property in item.Properties)
                            {
                                VariableObject temp = new VariableObject(editor, property.Key);
                                temp.SetValue(Convert.ChangeType(property.Value, temp.Type));
                            }
                        }

                        if (!string.IsNullOrEmpty(item.NickName))
                        {
                            EditorCommon common = editor as EditorCommon;
                            if (common != null)
                            {
                                common.Label.Text = item.NickName;
                            }
                        }

                        editorType.AddVariable(variable, editor);
                    }

                    fix = editorType;
                }

                return fix;
            }

            while (true)
            {
                fix = GetSuitableType(instance, instance.GetType());
                if (fix != null)
                {
                    return fix;
                }
                type = type.BaseType;
            }
        }
        private static IEditorType GetSuitableType(object instance, Type type)
        {
            Type editorType = Type.GetType(string.Format("EditorUI.TypeUIElement`1[[{0}]]", type.AssemblyQualifiedName));
            List<Type> types = UtilityEditor.LoadedAssemblies().GetTypes(editorType);
            if (types.Count > 0)
                editorType = types.Last();
            return (IEditorType)editorType.GetConstructor(new Type[] { type }).Invoke(new object[] { instance });
        }
        public static void TranslateUI(string file)
        {
            _LOG.Debug("EntryBuilder = {0}", _IO.PathCombine(EditorUI.Instance.DirectoryEditor, C.EntryBuilder));
            _LOG.Debug("LANGUAGE = {0}", Path.GetFullPath(EditorUI.Instance.Project.TranslateTable));
            Process process = Process.Start(_IO.PathCombine(EditorUI.Instance.DirectoryEditor, C.EntryBuilder), string.Format("BuildTableTranslate {0} {1}", Path.GetFullPath(EditorUI.Instance.Project.TranslateTable), file));
            process.WaitForExit();
        }

        protected override void InternalEvent(Entry e)
        {
            if (Handled)
                return;

            if (e.INPUT.Pointer.ComboClick.IsDoubleClick)
            {
                // TEST
            }
            //int count = 0;
            //UIElement.ForeachAllChildPriority(this, null, u => count += u.ChildCount);
            //Console.WriteLine("UIElement count: {0}", count);
            // 自定义编辑模式
            if (CurrentEditMode != null)
            {
                if (e.INPUT.Keyboard.IsClick((int)PCKeys.Escape))
                {
                    CurrentEditMode.Cancel();
                    CurrentEditMode = null;
                }
                else if (CurrentEditMode.Edit(this, e))
                {
                    CurrentEditMode = null;
                }
                return;
            }

            if (view == null)
                return;

            bool dragable = e.INPUT.Keyboard.IsPressed((int)PCKeys.Space);
            pvc.DragMode = dragable ? EDragMode.Drag : EDragMode.None;

            if (SelectedElement != null)
            {
                // 取消放置：esc / 右键
                if (e.INPUT.Keyboard.IsClick((int)PCKeys.Escape) ||
                    e.INPUT.Pointer.IsClick(1))
                    SelectedElement = null;

                VECTOR2 position = e.INPUT.Pointer.Position;
                // 放下拖入的新控件
                if (!dragable &&
                    e.INPUT.Pointer.IsRelease(0) &&
                    (!pec.ViewClip.Contains(position) ||
                    // Duplicate控件
                    selectedPreviewPanel == null))
                {
                    if (!pec.ViewClip.Contains(e.INPUT.Pointer.ClickPosition)
                        && !e.INPUT.Pointer.IsTap() && selectedPreviewPanel != null)
                    {
                        // 拖拽指定大小
                        RECT box = RECT.CreateRectangle(position, e.INPUT.Pointer.ClickPosition);
                        position = box.Location + UIElement.CalcPivotPoint(box.Size, SelectedElement.Pivot);
                        SelectedElement.Size = box.Size;
                    }
                    SetElement(SelectedElement, position);
                    SelectedElement = null;
                }
            }

            if (!e.INPUT.Pointer.IsPressed(0))
            {
                // 结束编辑模式
                if (Editing)
                {
                    switch (EditMode)
                    {
                        case EEditMode.Offset:
                            UIElement newParent = (UIElement)UIElement.FindChildPriority(EditingScene, ViewFindSkipSelected, t => t.IsContains(e.INPUT.Pointer.Position));
                            if (newParent != null && newParent != selected.Parent)
                            {
                                SetElementLocation(selected, selected.ConvertLocalToOther(selected.PivotPoint, newParent));
                                newParent.Add(selected);
                            }
                            break;
                    }
                    editScaleMode = null;
                    nears.Clear();
                }
            }

            // 窗口非激活状态跳过操作
            if (!XnaGate.Gate.IsActive)
                return;

            UIElement target = UIElement.FindChildPriority(pvc, ViewFindSkip, t => t.IsContains(e.INPUT.Pointer.Position));
            // 点在了panelViewContainer之外
            if (target == null)
                return;
            // 选择panelViewContainer视为取消选择
            if (target == pvc)
                target = null;

            if (dragable)
                return;

            // 编辑操作
            if (Editing)
            {
                // 最前端绘制
                selected.DrawTopMost(this);

                if (e.INPUT.Pointer.DeltaPosition != VECTOR2.Zero)
                {
                    // 自动对齐
                    AutoAlign(e);
                }
            }

            // 开始编辑
            if (e.INPUT.Pointer.IsClick(0))
            {
                if (e.INPUT.Keyboard.IsPressed(PCKeys.LeftShift) ||
                    e.INPUT.Keyboard.IsPressed(PCKeys.RightShift))
                {
                    // 复制控件
                    if (target != EditingScene)
                    {
                        SelectedElement = CopyElement(target);
                        PhotoElement(target, pre => selectedPreview = pre);
                    }
                }
                else
                {
                    // 选中控件
                    Selected = target;
                    // Ctrl键可以只选中对象，放置误拖动；ComboBox里的Panel会放不回去
                    if (selected != null && selected != EditingScene && !e.INPUT.Keyboard.Ctrl)
                    {
                        // 开始编辑操作
                        editScaleMode = GetScaleMode(selected, e);
                        editStartParam = selected.Clip;
                        editStartPoint = selected.ConvertGraphicsToLocal(e.INPUT.Pointer.Position);
                        if (EditMode == EEditMode.Offset)
                        {
                            SetElementLocation(target, target.ConvertLocalToOther(target.PivotPoint, EditingScene));
                            target.Parent.Remove(target);
                            EditingScene.Add(target);
                        }
                    }
                }
            }

            if (selected != null)
            {
                // 双击：保存
                if (e.INPUT.Pointer.ComboClick.IsDoubleClick)
                    SaveElement(selected);
                // Delete：删除选中控件
                else if (e.INPUT.Keyboard.IsPressed(PCKeys.Delete) &&
                    selected != EditingScene)
                    DeleteSelected();
                // 上下左右移动
                else if (e.INPUT.Keyboard.IsInputKeyPressed(PCKeys.Up))
                    SetElementLocation(selected, selected.Location + new VECTOR2(0, -1));
                else if (e.INPUT.Keyboard.IsInputKeyPressed(PCKeys.Left))
                    SetElementLocation(selected, selected.Location + new VECTOR2(-1, 0));
                else if (e.INPUT.Keyboard.IsInputKeyPressed(PCKeys.Down))
                    SetElementLocation(selected, selected.Location + new VECTOR2(0, 1));
                else if (e.INPUT.Keyboard.IsInputKeyPressed(PCKeys.Right))
                    SetElementLocation(selected, selected.Location + new VECTOR2(1, 0));
            }

            // Ctrl + 快捷键
            if (e.INPUT.Keyboard.IsPressed(PCKeys.LeftControl) ||
                e.INPUT.Keyboard.IsPressed(PCKeys.RightControl))
            {
                if (selected != null && selected != EditingScene)
                {
                    // C：复制
                    if (e.INPUT.Keyboard.IsClick(PCKeys.C))
                        copy = selected;
                    // V：粘贴
                    else if (e.INPUT.Keyboard.IsClick(PCKeys.V) && copy != null)
                        SetElement(CopyElement(copy), e.INPUT.Pointer.Position);
                    // D：删除
                    else if (e.INPUT.Keyboard.IsClick(PCKeys.D))
                        DeleteSelected();
                }

                // Z：撤销
                if (e.INPUT.Keyboard.IsClick(PCKeys.Z))
                    ;
                // Y：重做
                else if (e.INPUT.Keyboard.IsClick(PCKeys.Y))
                    ;
                // S：保存
                else if (e.INPUT.Keyboard.IsClick(PCKeys.S))
                    if (selected == null || selected == EditingScene)
                        SaveView();
                    else
                        SaveElement(selected);
            }

            if (e.INPUT.Keyboard.IsClick(PCKeys.LeftAlt) ||
                e.INPUT.Keyboard.IsClick(PCKeys.RightAlt))
                drawBorder = !drawBorder;
        }
        protected override void InternalUpdate(Entry e)
        {
            // 新建场景按钮样式
            for (int i = 1; i < 3; i++)
            {
                MenuStripItem item = ms[0].MenuStripChild[i];
                if ((Project == null) == item.Eventable)
                {
                    if (Project == null)
                    {
                        item.Eventable = false;
                        item.Color = COLOR.Gray;
                    }
                    else
                    {
                        item.Eventable = true;
                        item.Color = COLOR.Black;
                    }
                }
            }
        }
        protected override void InternalDrawAfter(GRAPHICS spriteBatch, Entry e)
        {
            base.InternalDrawAfter(spriteBatch, e);

            UIElement focus = UIElement.FindElementByPosition(this, e.INPUT.Pointer.Position);
            if (focus != null && focus != this && focus != pvc)
            {
                spriteBatch.Draw(patchViewportFocusBorder, focus.ViewClip, null, C.ColorFocusBorder);

                //string text = focus.Name;
                //if (string.IsNullOrEmpty(text))
                //{
                //    text = focus.GetType().Name;
                //}
                //spriteBatch.Draw(EEFont.Default, text, e.CurrentPosition, EEColor.Red, EEVector2.One);
            }

            foreach (NearAlign item in nears)
            {
                RECT line;
                VECTOR2 location;
                float result;
                if (item.Vertical)
                {
                    line.X = item.Result;
                    line.Y = 0;
                    line.Width = C.LineBold;
                    line.Height = Entry.GRAPHICS.ScreenSize.Y;

                    location.X = line.X;
                    location.Y = line.Height / 3;

                    result = selected.ConvertGraphicsToLocalView(new VECTOR2(item.Result, 0)).X;
                }
                else
                {
                    line.X = 0;
                    line.Y = item.Result;
                    line.Width = Entry.GRAPHICS.ScreenSize.X;
                    line.Height = C.LineBold;

                    location.X = line.Width / 3;
                    location.Y = line.Y;

                    result = selected.ConvertGraphicsToLocalView(new VECTOR2(0, item.Result)).Y;
                }
                spriteBatch.Draw(TEXTURE.Pixel, line, null, C.ColorAlignLine);
                // 坐标显示
                spriteBatch.Draw(FONT.Default, result.ToString(), location, COLOR.Black);
                foreach (UIElement align in item.AlignTargets)
                {
                    spriteBatch.Draw(patchViewportAlign, align.ViewClip, null, C.ColorAlignLine);
                }
            }

            if (CurrentEditMode != null)
            {
                spriteBatch.Draw(TEXTURE.Pixel, Entry.GRAPHICS.FullGraphicsArea, null, CurrentEditMode.BackColor);
            }
        }
    }
    public class Widget
    {
        public string FilePath;
        public IEditorType Target;

        public bool IsSaveAs
        {
            get
            {
                return string.IsNullOrEmpty(FilePath) ||
                    ((UIElement)Target.Instance).Name != Path.GetFileNameWithoutExtension(FilePath);
            }
        }

        public virtual bool Save()
        {
            UIElement instance = (UIElement)Target.Instance;

            if (IsSaveAs)
            {
                ElementLib lib = EditorUI.GetElementLib(instance.GetType());
                if (lib == null)
                {
                    UtilityEditor.Message("", "不能保存没有注册的类型");
                    return false;
                }
                string name = Path.Combine(EditorUI.DIR_PREVIEW, lib.Name);
                if (!Directory.Exists(name))
                    Directory.CreateDirectory(name);
                name = Path.Combine(name, instance.Name);
                if (!UtilityEditor.SaveFile(ref name, EditorUI.SUFFIX_ELEMENT))
                    return false;
                FilePath = name;
            }

            // save uielement to data
            byte[] content = EditorUI.SaveUI(this);
            File.WriteAllBytes(FilePath, content);

            // save preview image
            string preview = UtilityEditor.GetProjectRelativePath(FilePath, EditorUI.DIR_PREVIEW);
            preview = Path.ChangeExtension(preview, ".png");
            string directory = Path.GetDirectoryName(preview);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            EditorUI.PhotoElement(instance, preview);

            return true;
        }
    }

    public abstract class EditMode
    {
        private static COLOR DefaultBackColor = new COLOR(33, 33, 222, 64);

        protected internal virtual COLOR BackColor
        {
            get { return DefaultBackColor; }
        }

        /// <summary>
        /// 在编辑器内进行自定义编辑操作
        /// </summary>
        /// <returns>是否操作完成</returns>
        protected internal abstract bool Edit(EditorUI editor, Entry e);
        protected internal abstract void Cancel();
        public void Start()
        {
            EditorUI.Instance.CurrentEditMode = this;
        }
    }
    public class EditSelectElement : EditMode
    {
        private Action<UIElement> callback;
        private Type targetType;
        public EditSelectElement(Action<UIElement> callback, Type type)
        {
            this.callback = callback;
            this.targetType = type;
        }
        protected internal override bool Edit(EditorUI editor, Entry e)
        {
            if (e.INPUT.Pointer.IsClick(1))
            {
                Cancel();
                return true;
            }
            if (e.INPUT.Pointer.IsClick(0))
            {
                callback(UIElement.FindChildPriority(editor.EditingScene, UIElement.FindSkipUnhover, ui => ui.IsContains(e.INPUT.Pointer.Position) && ui.GetType().Is(targetType)));
                return true;
            }
            else
            {
                return false;
            }
        }
        protected internal override void Cancel()
        {
            callback(null);
        }
    }

    // Type
    public interface IEditorType
    {
        object Instance { get; set; }
        IEnumerable<EditorVariable> GetEditors();
    }
    public abstract class EditorType : IEditorType
    {
        private object instance;

        public virtual object Instance
        {
            get { return instance; }
            set { instance = value; }
        }
        public Type Type
        {
            get { return Instance.GetType(); }
        }

        public virtual IEnumerable<EditorVariable> GetEditors()
        {
            EditorVariable editor = EditorUI.TypeEditorGenerator.GeneratorEditor(Type, null);
            editor.Variable = new VariableValue(Instance);
            yield return editor;
        }
    }
    public class EditorBaseType : EditorType
    {
        private List<EditorVariable> variables;
        private List<EditorVariable> auto;

        public override object Instance
        {
            get
            {
                return base.Instance;
            }
            set
            {
                base.Instance = value;
                if (variables != null || auto != null)
                {
                    foreach (var item in GetEditors())
                    {
                        item.Variable = new VariableObject(value, item.Variable.VariableName);
                    }
                }
            }
        }

        public EditorBaseType()
        {
        }
        public EditorBaseType(object target)
        {
            Instance = target;
        }

        private void AutoBuildType()
        {
            auto = new List<EditorVariable>();

            SerializeSetting setting = SerializeSetting.DefaultSetting;
            setting.Property = true;
            setting.Static = Instance == null;
            setting.Serialize(Type, Instance, v =>
                {
                    InternalAddVariable(new VariableObject(Instance, v.VariableName), EditorUI.TypeEditorGenerator.GeneratorEditor(v.Type, v.GetValue()), auto);
                });
        }
        private void InternalAddVariable(VariableObject variable, EditorVariable editor, List<EditorVariable> target)
        {
            if (variable == null)
            {
                throw new ArgumentNullException("variable");
            }
            if (editor == null)
            {
                throw new NotImplementedException(string.Format("can not find the editor by type={0} field={1}", variable.Type.Name, variable.VariableName));
            }
            if (target.Contains(e => e.Variable.VariableName == variable.VariableName))
            {
                throw new NotImplementedException(string.Format("instance variable repeated! name={0}", variable.VariableName));
            }
            editor.Variable = variable;
            target.Add(editor);
        }
        public void AddVariable(string name)
        {
            AddVariable(name, null);
        }
        public void AddVariable(string name, EditorVariable editor)
        {
            AddVariable(new VariableObject(Instance, name), editor);
        }
        public void AddVariable(VariableObject variable, EditorVariable editor)
        {
            if (variable == null)
            {
                throw new ArgumentNullException("variable");
            }
            if (variables == null)
            {
                variables = new List<EditorVariable>();
                if (auto != null)
                {
                    auto.Clear();
                    auto = null;
                }
            }
            if (editor == null)
            {
                editor = EditorUI.TypeEditorGenerator.GeneratorEditor(variable.Type, variable.Instance);
            }
            InternalAddVariable(variable, editor, variables);
        }
        public sealed override IEnumerable<EditorVariable> GetEditors()
        {
            if (variables == null)
            {
                if (auto == null)
                {
                    AutoBuildType();
                }
                return auto;
            }
            return variables;
        }
    }

    // Editor
    public abstract class EditorVariable : Panel
    {
        private IVariable variable;
        private object valueMonitor;
        private ContentManager content;
        public event DUpdate<EditorVariable> ValueChanged;
        public virtual EGenerateTime GenerateTime
        {
            get { return EGenerateTime.Initialize; }
        }

        public IVariable Variable
        {
            get { return variable; }
            set
            {
                if (variable == value)
                    return;
                variable = value;
                SetVariable(value);
                if (value == null || value.Instance == null)
                    return;
                SetValue();
            }
        }
        public virtual object VariableValue
        {
            get
            {
                if (variable == null || variable.Instance == null)
                    return null;
                return variable.GetValue();
            }
            set
            {
                if (variable == null || object.Equals(VariableValue, value))
                    //if (variable == null || VariableValue.Equals(value))
                    return;
                variable.SetValue(value);
                SetValue();
                DoValueChanged();
            }
        }
        public virtual string VariableStringValue
        {
            get
            {
                object value = VariableValue;
                if (value == null)
                    //return string.Empty;
                    return null;
                else
                    return value.ToString();
            }
        }
        protected ContentManager Content
        {
            get { return content; }
        }

        protected void DoValueChanged()
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, Entry.Instance);
            }
        }
        internal protected virtual Dictionary<EGenerateTime, string> GenerateCode(string instance)
        {
            Dictionary<EGenerateTime, string> codes = new Dictionary<EGenerateTime, string>();
            codes[GenerateTime] = string.Format("{0}.{1} = {2};", instance, variable.VariableName, _SERIALIZE.CodeValue(VariableValue, true, SerializeSetting.DefaultSetting));
            return codes;
        }
        internal protected virtual void SaveValue(BinaryWriter writer)
        {
            writer.WriteObject(VariableValue, Variable.Type);
        }
        internal protected virtual void LoadValue(object instance, BinaryReader reader)
        {
            object value = reader.ReadObject(Variable.Type);
            if (instance != null)
                VariableValue = value;
        }
        internal void Load(ContentManager content)
        {
            if (content == null)
                throw new ArgumentNullException("content");
            InternalLoad(content);
            //SetValue();
        }
        protected virtual void InternalLoad(ContentManager content)
        {
            this.content = content;
        }
        protected virtual void SetVariable(IVariable variable)
        {
        }
        protected abstract void SetValue();
        protected override void InternalUpdate(Entry e)
        {
            CheckValueChanged();
        }
        public void CheckValueChanged()
        {
            object value = VariableValue;
            if (!object.Equals(valueMonitor, value))
            {
                SetValue();
                valueMonitor = value;
            }
        }
    }
    public abstract class EditorCommon : EditorVariable
    {
        public Label Label = new Label();

        public EPivot LayoutMode
        {
            get { return Label.UIText.TextAlignment; }
            set { Label.UIText.TextAlignment = value; }
        }
        protected virtual float Seperator
        {
            get { return 150; }
        }
        protected float EditableWidth
        {
            get { return 150; }
        }
        protected float EditableHeight
        {
            get { return Label.ContentSize.Y; }
        }

        public EditorCommon()
        {
            this.Background = EditorUI.GetNinePatch(BGBorderColor, BGBodyColor);
            //this.Hover += HoverArea;
            //this.UnHover += UnHoverArea;

            var texture = EditorUI.GetNinePatch(BGBorderColor, COLOR.TransparentBlack);
            texture.Left = 0;
            texture.Top = 0;
            texture.Bottom = texture.Height;
            Label.SourceNormal = texture;
            Label.Width = Seperator;
            Add(Label);
        }

        protected override void SetValue()
        {
            if (Variable != null)
            {
                if (string.IsNullOrEmpty(Label.Text))
                {
                    Label.Text = Variable.VariableName;
                }
                Label.UIText.Padding.X = EditableHeight * 2 + 2;
            }
        }

        public static COLOR BGBorderColor = new COLOR(160, 160, 160, 222);
        //public static EEColor BGBorderHoverColor = EEColor.Silver;
        public static COLOR BGBodyColor = new COLOR(96, 96, 96, 255);

        //private static void HoverArea(UIElement sender, Entry e)
        //{
        //    ((Panel)sender).Background.Color = BGBorderHoverColor;
        //}
        //private static void UnHoverArea(UIElement sender, Entry e)
        //{
        //    ((Panel)sender).Background.Color = BGBorderColor;
        //}
        public static CheckBox BuildNullable()
        {
            CheckBox checkBox = new CheckBox();
            checkBox.SourceNormal = EditorUI.GetNinePatch(COLOR.Gold, COLOR.TransparentBlack);
            checkBox.SourceClicked = EditorUI.GetNinePatch(COLOR.TransparentBlack, COLOR.GreenYellow);
            return checkBox;
        }
    }
    public class EditorNullable : EditorCommon
    {
        private EditorVariable valueEditor;
        private bool loaded;
        private CheckBox valueCheckBox;

        public bool IsNull
        {
            get { return VariableValue == null; }
        }
        public Type ValueType
        {
            get { return Nullable.GetUnderlyingType(Variable.Type); }
        }

        public EditorNullable(EditorVariable valueEditor)
        {
            this.valueEditor = valueEditor;

            valueCheckBox = EditorCommon.BuildNullable();
            valueCheckBox.CheckedChanged += SetValue;
            valueCheckBox.Height = EditableHeight;
            valueCheckBox.Width = EditableHeight;
            valueCheckBox.X = Seperator;
            valueCheckBox.Pivot = EPivot.TopRight;
            Insert(valueCheckBox, 0);

            valueEditor.Background = null;
            valueEditor.ValueChanged += ResetValue;
            Insert(valueEditor, 0);
        }

        protected override void SetValue()
        {
            base.SetValue();
            bool hasValue = !IsNull;
            if (hasValue)
            {
                if (!loaded && Content != null)
                {
                    valueEditor.Load(Content);
                    loaded = true;
                }
                valueEditor.Visible = true;
                valueEditor.Variable = new VariableValue(VariableValue);
            }
            else
            {
                valueEditor.Visible = false;
            }
            valueCheckBox.Checked = hasValue;
        }
        private void SetValue(Button sender, Entry e)
        {
            if (sender.Checked)
            {
                if (IsNull)
                {
                    VariableValue = Activator.CreateInstance(ValueType);
                }
            }
            else
            {
                VariableValue = null;
            }
        }
        private void ResetValue(EditorVariable sender, Entry e)
        {
            if (valueCheckBox.Checked)
            {
                VariableValue = sender.VariableValue;
            }
        }

        public static COLOR NullColor = new COLOR(160, 160, 160, 255);
    }
    public class EditorObject : EditorVariable
    {
        public IEnumerable<EditorVariable> Editors
        {
            get
            {
                //foreach (UIElement child in Childs)
                for (int i = 0; i < Childs.Count; i++)
                {
                    var child = Childs[i];
                    EditorVariable editor = child as EditorVariable;
                    if (editor != null)
                    {
                        yield return editor;
                    }
                }
            }
        }

        protected internal override Dictionary<EGenerateTime, string> GenerateCode(string instance)
        {
            Dictionary<EGenerateTime, string> codes = new Dictionary<EGenerateTime, string>();

            if (!Variable.Type.IsValueType || Variable.MemberInfo.MemberType == MemberTypes.Field)
            {
                instance = string.Format("{0}.{1}", instance, Variable.VariableName);
                if (Variable.Instance == null || VariableValue == null)
                {
                    codes[GenerateTime] = instance + "= null;";
                }
                else
                {
                    foreach (EditorVariable item in Editors)
                    {
                        GeneratorUICode.CombineCode(codes, item.GenerateCode(instance));
                    }
                }
            }
            else
            {
                StringBuilder builder = new StringBuilder();
                string org = instance;
                instance = string.Format("{0}.{1}", instance, Variable.VariableName);
                instance = instance.Replace('.', '_');
                instance = instance.Replace('[', '_');
                instance = instance.Replace(']', '_');
                builder.AppendLine("{0} {1} = new {0}();", Variable.Type.CodeName(), instance);
                foreach (EditorVariable item in Editors)
                {
                    foreach (var generate in item.GenerateCode(instance))
                    {
                        if (generate.Key == GenerateTime)
                        {
                            builder.AppendLine(generate.Value);
                        }
                        else
                        {
                            string result = null;
                            if (codes.ContainsKey(generate.Key))
                                result = codes[generate.Key];
                            result += generate.Value;
                            codes[generate.Key] = result;
                        }
                    }
                }
                builder.AppendLine("{0}.{1} = {2};", org, Variable.VariableName, instance);

                codes[GenerateTime] = builder.ToString();
            }

            return codes;
        }
        protected internal override void SaveValue(BinaryWriter writer)
        {
            foreach (EditorVariable item in Editors)
            {
                item.SaveValue(writer);
            }
        }
        protected internal override void LoadValue(object instance, BinaryReader reader)
        {
            foreach (EditorVariable item in Editors)
            {
                item.LoadValue(instance, reader);
            }
        }
        protected override void InternalLoad(ContentManager content)
        {
            foreach (EditorVariable item in Editors)
            {
                item.Load(content);
                item.ValueChanged -= ResetValue;
                item.ValueChanged += ResetValue;
            }
            SetValue();
        }
        protected override void SetValue()
        {
            ResetLayout(VECTOR2.NaN);
            //foreach (EditorVariable editor in Editors)
            //{
            //    EditorCommon common = editor as EditorCommon;
            //    if (common != null)
            //    {
            //        common.Label.Scale = new EEVector2(0.84f);
            //    }
            //}
        }
        private void ResetValue(EditorVariable sender, Entry e)
        {
            if (Variable.Type.IsValueType)
            {
                VariableValue = sender.Variable.Instance;
            }
        }
        private void ResetLayout(VECTOR2 size)
        {
            float y = 0;
            foreach (EditorVariable item in Editors)
            {
                item.ContentSizeChanged -= ResetLayout;
                item.ContentSizeChanged += ResetLayout;

                item.Y = y;
                y += item.ContentSize.Y;
                if (size.IsNaN() && VariableValue != null)
                {
                    item.Variable = new VariableObject(VariableValue, item.Variable.VariableName);
                }
            }
        }
    }
    public class EditorAllObject : EditorComboBox
    {
        private EditorVariable valueEditor;
        private bool singleType;
        private bool initNull;
        private CheckBox collapse;

        internal EditorVariable ValueEditor
        {
            get { return valueEditor; }
            set
            {
                valueEditor = value;
                Add(valueEditor);
            }
        }
        public bool SingleType
        {
            get { return singleType; }
        }

        public EditorAllObject(IList<Type> types)
            : base((Array)null)
        {
            collapse = new CheckBox();
            var texture = EditorUI.GetNinePatch(COLOR.Yellow, COLOR.TransparentBlack);

            texture.Left = 2;
            texture.Top = 2;
            texture.Right = texture.Width;
            texture.Bottom = texture.Height;
            collapse.SourceNormal = texture;

            texture = EditorUI.GetNinePatch(COLOR.Yellow, COLOR.TransparentBlack);
            texture.Left = 0;
            texture.Top = 0;
            texture.Right = texture.Width - 2;
            texture.Bottom = texture.Width - 2;
            collapse.SourceClicked = texture;

            collapse.Width = EditableHeight;
            collapse.Height = EditableHeight;
            collapse.CheckedOverlayNormal = true;
            collapse.CheckedChanged += CollapseSwitch;
            AddChildFirst(collapse);

            object[] array = types.Select(t => Activator.CreateInstance(t)).ToArray();
            if (array.Length == 1)
            {
                object target = array[0];
                Type type = target.GetType();
                singleType = type.IsValueType;
            }

            if (!singleType)
            {
                array = array.Insert(0, (object)null);
            }

            SetArray(array);
        }

        private void Show()
        {
            if (valueEditor != null)
            {
                valueEditor.Visible = collapse.Checked;
            }
        }
        private void CollapseSwitch(Button sender, Entry e)
        {
            Show();
        }
        private void SetVariableValue()
        {
            for (int i = 0; i < array.Length; i++)
            {
                object obj = array.GetValue(i);
                if (obj != null && obj.GetType() == Variable.Type && VariableValue != null)
                {
                    array.SetValue(VariableValue, i);
                    break;
                }
            }
        }
        protected override void SelectedIndexChanged(Selectable sender)
        {
            if (Variable.Type.IsValueType)
            {
                SetVariableValue();
            }
            base.SelectedIndexChanged(sender);
        }
        protected override string ShowValue(object value)
        {
            //if (singleType)
            //    return value.ToString();
            if (value == null)
                return "null";
            Type type = value.GetType();
            ElementLib lib = EditorUI.GetElementLib(type);
            if (lib == null)
            {
                return type.Name;
            }
            else
            {
                return lib.Name;
            }
        }
        protected override int Select(Array array, object value)
        {
            if (singleType || value == null)
                return 0;
            for (int i = 1; i < array.Length; i++)
            {
                object item = array.GetValue(i);
                if (item.GetType() == value.GetType())
                {
                    return i;
                }
            }
            return 0;
        }
        protected override void InternalLoad(ContentManager content)
        {
            base.InternalLoad(content);
            if (valueEditor != null)
                valueEditor.Load(content);
        }
        protected internal override Dictionary<EGenerateTime, string> GenerateCode(string instance)
        {
            Dictionary<EGenerateTime, string> codes = new Dictionary<EGenerateTime, string>();

            StringBuilder builder = new StringBuilder();
            //if (initNull && SelectedIndex != 0)
            if (SelectedIndex != 0)
                codes[GenerateTime] = string.Format("{0}.{1} = new {2}();", instance, Variable.VariableName, VariableValue.GetType().CodeName());
            if (valueEditor != null)
                GeneratorUICode.CombineCode(codes, valueEditor.GenerateCode(instance));
            else
                GeneratorUICode.CombineCode(codes, base.GenerateCode(instance));

            return codes;
        }
        protected internal override void SaveValue(BinaryWriter writer)
        {
            if (!singleType)
                writer.Write(SelectedIndex);
            if (valueEditor != null)
                valueEditor.SaveValue(writer);
            else
                base.SaveValue(writer);
        }
        protected internal override void LoadValue(object instance, BinaryReader reader)
        {
            if (!singleType)
            {
                int index;
                reader.Read(out index);
                comboBox.DropDownList.SelectedIndex = index;
            }
            if (valueEditor != null)
                valueEditor.LoadValue(instance, reader);
            else
                base.LoadValue(instance, reader);
            SetVariableValue();
        }
        protected override void SetValue()
        {
            bool first = SelectedIndex == -1;
            if (first)
            {
                SetVariableValue();
            }
            if (!singleType && SelectedIndex == 0)
            {
                initNull = true;
            }
            base.SetValue();
            bool changed;
            bool null1 = valueEditor == null;
            bool null2 = SelectedItem == null;
            if (null1 == null2 && null1)
            {
                changed = false;
            }
            else if (null1 != null2)
            {
                changed = true;
            }
            else
            {
                changed = Variable.Type != SelectedItem.GetType();
            }
            if (valueEditor != null && changed)
            {
                Remove(valueEditor);
            }
            if (changed)
            {
                if (VariableValue == null)
                {
                    valueEditor = null;
                }
                else
                {
                    valueEditor = EditorUI.TypeEditorGenerator.GeneratorEditor(SelectedItem.GetType(), VariableValue);
                    if (valueEditor is EditorAllObject)
                    {
                        valueEditor = ((EditorAllObject)valueEditor).valueEditor;
                    }
                    Add(valueEditor);
                }
            }

            if (valueEditor != null)
            {
                valueEditor.Y = EditableHeight;
                valueEditor.Variable = Variable;
                if (Content != null)
                {
                    valueEditor.Load(Content);
                }
                Show();

                valueEditor.ValueChanged -= ThrowValue;
                valueEditor.ValueChanged += ThrowValue;
            }
            if (singleType)
                comboBox.Text = VariableValue.ToString();

            //collapse.Checked = VariableValue != null;
        }
        private void ThrowValue(EditorVariable sender, Entry e)
        {
            if (Variable.Instance.GetType().IsValueType)
            {
                EditorObject parent = this.Parent as EditorObject;
                if (parent != null)
                {
                    parent.VariableValue = Variable.Instance;
                }
            }
        }
    }
    public class EditorTextBox : EditorCommon
    {
        internal static StringTable Language;

        private TextBox textBox = new TextBox();

        /// <summary>
        /// False: 空 / 数字 / #或_开头
        /// </summary>
        public bool Translate
        {
            get
            {
                string value = VariableStringValue;

                if (string.IsNullOrEmpty(value))
                    return false;

                bool result = true;
                double temp;
                result = !double.TryParse(value, out temp);

                if (value.StartsWith("#") || value.StartsWith("_"))
                    result = false;

                return result;
            }
        }
        protected TextBox TextBox
        {
            get { return textBox; }
        }

        public EditorTextBox()
        {
            textBox.X = Seperator;
            textBox.Width = EditableWidth;
            textBox.Height = EditableHeight;
            textBox.SourceNormal = EditorUI.GetNinePatch(COLOR.Gray, COLOR.White);
            textBox.UIText.Padding = new VECTOR2(10, 4);
            textBox.UIText.FontColor = COLOR.Black;
            Add(textBox);

            InternalInit();
        }

        protected virtual void InternalInit()
        {
            TextBox.TextChanged += TextChangedModifyValue;
        }
        protected virtual void TextChangedModifyValue(Label sender, Entry e)
        {
            VariableValue = sender.Text;
        }
        protected override void SetValue()
        {
            base.SetValue();
            textBox.Text = VariableStringValue;
        }
        protected internal override Dictionary<EGenerateTime, string> GenerateCode(string instance)
        {
            if (Translate)
            {
                Dictionary<EGenerateTime, string> codes = new Dictionary<EGenerateTime, string>();
                // {2} need translate
                int row = Language.GetRowIndex(2, VariableStringValue);
                if (row != -1)
                {
                    string id = Language[1, row];
                    codes[EGenerateTime.Show] = string.Format("{0}.{1} = _LANGUAGE.GetString(\"{2}\");", instance, Variable.VariableName, id);
                    //Utility.CodeValue(VariableValue, true, SerializeSetting.DefaultSetting));
                    return codes;
                }
            }
            return base.GenerateCode(instance);
        }
    }
    public class EditorNumber : EditorTextBox
    {
        private decimal minValue;
        private decimal maxValue;
        /// <summary>
        /// y+1px时对应的Value变化
        /// </summary>
        public decimal DragStep = 1;

        public decimal MinValue
        {
            get { return minValue; }
            set
            {
                minValue = value;

                if (minValue > maxValue)
                    maxValue = minValue;

                if (minValue > Value)
                    Value = minValue;
            }
        }
        public decimal MaxValue
        {
            get { return maxValue; }
            set
            {
                maxValue = value;

                if (maxValue < minValue)
                    minValue = maxValue;

                if (maxValue < Value)
                    Value = maxValue;
            }
        }
        public decimal Value
        {
            get
            {
                object value = VariableValue;
                if (value == null)
                    return 0;
                else
                    return Convert.ToDecimal(value);
            }
            set
            {
                value = value > maxValue ? maxValue : value;
                value = value < minValue ? minValue : value;
                VariableValue = Convert.ChangeType(value, Variable.Type);
            }
        }

        public EditorNumber()
        {
            InternalInit();
        }
        public EditorNumber(IVariable variable)
            : this()
        {
            this.Variable = variable;
        }
        public EditorNumber(decimal step)
            : this()
        {
            this.DragStep = step;
        }
        public EditorNumber(IVariable variable, decimal step)
            : this()
        {
            this.DragStep = step;
            this.Variable = variable;
        }
        public EditorNumber(IVariable variable, decimal min, decimal max, decimal step)
        {
            this.DragStep = step;
            this.MinValue = min;
            this.MaxValue = max;
            this.Variable = variable;
            InternalInit();
        }

        protected override void InternalInit()
        {
            base.InternalInit();
            this.TextBox.Drag += DragModifyValue;
        }
        protected void SetDefaultScope()
        {
            Type type = Variable.Type;
            try
            {
                if (type == typeof(float) || type == typeof(double))
                {
                    MinValue = decimal.MinValue;
                    MaxValue = decimal.MaxValue;
                }
                else
                {
                    FieldInfo const1 = type.GetField("MaxValue");
                    FieldInfo const2 = type.GetField("MinValue");

                    MaxValue = Convert.ToDecimal(const1.GetValue(null));
                    MinValue = Convert.ToDecimal(const2.GetValue(null));
                }
            }
            catch (Exception ex)
            {
                _LOG.Error("typeof({0}) get MaxValue and MinValue error! msg={1}", type.Name, ex.Message);
            }
        }
        protected void DragModifyValue(UIElement sender, Entry e)
        {
            VECTOR2 moved = e.INPUT.Pointer.DeltaPosition;
            if (moved != VECTOR2.Zero && !moved.IsNaN())
            {
                TextBox.SetFocus(false);
                Value += (decimal)-moved.Y * DragStep;
            }
        }
        protected override void TextChangedModifyValue(Label sender, Entry e)
        {
            decimal value;
            if (decimal.TryParse(sender.Text, out value))
            {
                Value = value;
            }
            else
            {
                SetValue();
            }
        }
        protected override void SetValue()
        {
            if (Variable != null)
            {
                if (minValue == 0 && maxValue == 0)
                {
                    SetDefaultScope();
                }
            }

            base.SetValue();
        }
    }
    public class EditorComboBox : EditorCommon
    {
        protected DropDown comboBox = new DropDown();
        protected Array array;

        public int SelectedIndex
        {
            get
            {
                object value = VariableValue;
                for (int i = 0; i < array.Length; i++)
                {
                    if (object.Equals(array.GetValue(i), value))
                    {
                        return i;
                    }
                }
                return -1;
            }
        }
        public object SelectedItem
        {
            get
            {
                if (SelectedIndex == -1)
                    return null;
                else
                    return array.GetValue(SelectedIndex);
            }
        }

        public EditorComboBox(Array array)
        {
            comboBox.SetDefaultControl(EditableWidth, EditableHeight);
            comboBox.X = Seperator;
            comboBox.DropDownText.SourceNormal = EditorUI.GetNinePatch(COLOR.Gray, COLOR.Black);
            comboBox.DropDownText.UIText.Padding = new VECTOR2(10, 0);
            comboBox.DropDownText.UIText.TextAlignment = EPivot.MiddleLeft;
            comboBox.Color = COLOR.Black;
            //comboBox.ScrollBar = EditorUI.GetScrollBar(true);
            comboBox.DropDownList.SelectedIndexChanged += SelectedIndexChanged;
            comboBox.DropDownList.Background = EditorUI.GetNinePatch(COLOR.Gray, COLOR.Black);
            Add(comboBox);

            if (array != null)
            {
                SetArray(array);
            }
        }
        public EditorComboBox(Type enumType)
            : this(Enum.GetValues(enumType))
        {
        }

        protected void SetArray(Array array)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            this.array = array;
            comboBox.Clear();
            for (int i = 0; i < array.Length; i++)
            {
                var item = comboBox.DropDownList.AddItem(ShowValue(array.GetValue(i)));
            }
            comboBox.DropDownList.Height = comboBox.DropDownList.ChildClip.Height;
            //comboBox.DisplayItemCount = array.Length;
        }
        protected virtual string ShowValue(object value)
        {
            return value.ToString();
        }
        protected virtual void SelectedIndexChanged(Selectable sender)
        {
            VariableValue = array.GetValue(sender.SelectedIndex);
        }
        protected override void SetValue()
        {
            base.SetValue();
            comboBox.DropDownList.SelectedIndex = Select(array, VariableValue);
        }
        protected virtual int Select(Array array, object value)
        {
            return Array.IndexOf(array, value);
        }
    }
    public class EditorAsset : EditorCommon
    {
        private const string ASYNC_VAR = "___c";
        public const string ASYNC = "___async";
        private const string __PIXEL = "#pixel";
        private const string __PATCH = "#patch";
        private Label path = new Label();
        private Panel patch = new Panel();
        private TextureBox cborder = new TextureBox();
        private NumberBox alpha = new NumberBox();
        private NumberBox bold = new NumberBox();

        public override EGenerateTime GenerateTime
        {
            get { return EGenerateTime.Load; }
        }
        public override object VariableValue
        {
            get
            {
                return VariableStringValue;
            }
            set
            {
                string _value = value.ToString();
                if (path.Text != _value)
                {
                    path.Text = _value;
                    //DoValueChanged();
                }
                SetValue();
            }
        }
        public override string VariableStringValue
        {
            get
            {
                return path.Text;
            }
        }

        public EditorAsset()
        {
            path.X = Seperator;
            path.Width = EditableWidth;
            path.Height = EditableHeight;
            path.Text = string.Empty;
            //path.TextChanged += new DUpdate<Label>(label_TextChanged);
            path.Clicked += new DUpdate<UIElement>(label_Clicked);
            Add(path);

            Label.Clicked += new DUpdate<UIElement>(Label_Clicked);

            cborder.X = path.X;
            cborder.Width = 50;
            cborder.Height = EditableHeight;
            cborder.Texture = TEXTURE.Pixel;
            cborder.Clicked += new DUpdate<UIElement>(cborder_Clicked);

            alpha.X = path.X + 50;
            alpha.Width = 40;
            alpha.Height = EditableHeight;
            alpha.MinValue = 0;
            alpha.MaxValue = 255;
            alpha.DefaultText.TextAlignment = EPivot.MiddleCenter;
            alpha.DefaultText.Text = "A";
            alpha.UIText.TextAlignment = EPivot.MiddleCenter;
            alpha.ValueChanged += new DUpdate<NumberBox>(alpha_ValueChanged);

            bold.X = path.X + 100;
            bold.Width = 20;
            bold.Height = EditableHeight;
            bold.MinValue = 1;
            bold.MaxValue = 3;
            bold.UIText.TextAlignment = EPivot.MiddleCenter;
            bold.ValueChanged += new DUpdate<NumberBox>(bold_ValueChanged);

            patch.Add(cborder);
            patch.Add(alpha);
            patch.Add(bold);
            Add(patch);
        }

        void bold_ValueChanged(NumberBox sender, Entry e)
        {
            COLOR color;
            byte bold;
            ParsePatch(out color, out bold);
            SetPatch(ref color, (byte)this.bold.Value);
        }
        void alpha_ValueChanged(NumberBox sender, Entry e)
        {
            COLOR color;
            byte bold;
            ParsePatch(out color, out bold);
            color.A = (byte)alpha.Value;
            SetPatch(ref color, bold);
        }
        void cborder_Clicked(UIElement sender, Entry e)
        {
            COLOR color;
            byte bold;
            ParsePatch(out color, out bold);
            COLOR? result = UtilityEditor.SelectColor(color);
            if (result.HasValue)
            {
                byte a = color.A;
                color = result.Value;
                color.A = a;
                SetPatch(ref color, bold);
            }
        }
        void SetPatch(ref COLOR color, byte bold)
        {
            VariableValue = string.Format("{0}{1},{2},{3},{4},{5}", __PATCH, color.R, color.G, color.B, color.A, bold);
        }
        void Label_Clicked(UIElement sender, Entry e)
        {
            if (Variable.Type == typeof(TEXTURE))
            {
                if (string.IsNullOrEmpty(VariableStringValue))
                {
                    VariableValue = __PIXEL;
                }
                else if (VariableStringValue == __PIXEL)
                {
                    if (UtilityEditor.Confirm("", "是否要图片换成默认的九宫格？"))
                        VariableValue = __PATCH + "0,0,0,255,2";
                }
                else
                {
                    if (UtilityEditor.Confirm("", "是否要清空图片？"))
                    {
                        Variable.SetValue(null);
                        VariableValue = string.Empty;
                    }
                }
                return;
            }
            else if (Variable.Type == typeof(FONT))
            {
                if (UtilityEditor.Confirm("", "是否要使用默认字体？"))
                {
                    //Variable.SetValue(FONT.Default);
                    VariableValue = string.Empty;
                }
            }
        }
        //void label_TextChanged(Label sender, Entry e)
        //{
        //    SetValue();
        //}
        void label_Clicked(UIElement sender, Entry e)
        {
            string file = EditorUI.DIR_CONTENT + "/";
            if (!string.IsNullOrEmpty(VariableStringValue))
                file += VariableStringValue;
            if (UtilityEditor.OpenFile(ref file, Content.LoadableContentFileSuffix.ToArray()))
            {
                file = _IO.RelativePathForward(file, EditorUI.DIR_CONTENT);
                //file = UtilityEditor.GetProjectRelativePath(file, EditorUI.DIR_CONTENT + "/");
                if (file != null)
                {
                    VariableValue = file;
                    //path.Text = file;
                }
            }
        }
        //void ParsePatch(out COLOR? body, out COLOR? border, out byte bold)
        //{
        //    string text = VariableStringValue.Substring(__PATCH.Length);
        //    string[] data = text.Split(',');
        //    if (data[3] == "-1")
        //        body = null;
        //    else
        //        body = new COLOR(byte.Parse(data[0]), byte.Parse(data[1]), byte.Parse(data[2]), byte.Parse(data[3]));
        //    if (data[7] == "-1")
        //        border = null;
        //    else
        //        border = new COLOR(byte.Parse(data[4]), byte.Parse(data[5]), byte.Parse(data[6]), byte.Parse(data[7]));
        //    bold = byte.Parse(data[8]);
        //}
        //PATCH ParsePatch()
        //{
        //    COLOR? body, border;
        //    byte bold;
        //    ParsePatch(out body, out border, out bold);
        //    return PATCH.GetNinePatch(body, border, bold);
        //}
        void ParsePatch(out COLOR border, out byte bold)
        {
            string text = VariableStringValue.Substring(__PATCH.Length);
            string[] data = text.Split(',');
            border = new COLOR(byte.Parse(data[0]), byte.Parse(data[1]), byte.Parse(data[2]), byte.Parse(data[3]));
            bold = byte.Parse(data[4]);
        }
        COLOR ParseColor()
        {
            string text = VariableStringValue.Substring(__PATCH.Length);
            string[] data = text.Split(',');
            return new COLOR(byte.Parse(data[0]), byte.Parse(data[1]), byte.Parse(data[2]), byte.Parse(data[3]));
        }
        byte ParseBold()
        {
            string text = VariableStringValue.Substring(VariableStringValue.LastIndexOf(',') + 1);
            return byte.Parse(text);
        }
        PATCH ParsePatch()
        {
            COLOR border;
            byte bold;
            ParsePatch(out border, out bold);
            return PATCH.GetNinePatch(null, border, bold);
        }
        protected override void SetValue()
        {
            path.Visible = true;
            patch.Visible = false;
            base.SetValue();
            if (string.IsNullOrEmpty(path.Text))
            {
                if (Variable.Type == typeof(FONT))
                {
                    Variable.SetValue(FONT.Default);
                }
                //if (Variable.GetValue() == null)
                else
                {
                    Variable.SetValue(null);
                }
            }
            else if (VariableStringValue == __PIXEL)
            {
                Variable.SetValue(TEXTURE.Pixel);
            }
            else if (VariableStringValue.StartsWith(__PATCH))
            {
                patch.Visible = true;
                path.Visible = false;
                COLOR border;
                byte _bold;
                ParsePatch(out border, out _bold);
                cborder.Color = border;
                alpha.Value = border.A;
                bold.Value = _bold;
                Variable.SetValue(PATCH.GetNinePatch(null, border, _bold));
            }
            else
            {
                Variable.SetValue(Content.Load(VariableStringValue));
            }
        }
        protected internal override Dictionary<EGenerateTime, string> GenerateCode(string instance)
        {
            Dictionary<EGenerateTime, string> codes = new Dictionary<EGenerateTime, string>();
            string result;

            if (string.IsNullOrEmpty(path.Text))
            {
                result = string.Empty;
            }
            else if (path.Text == __PIXEL)
            {
                result = string.Format("{0}.{1} = TEXTURE.Pixel;", instance, Variable.VariableName);
            }
            else if (VariableStringValue.StartsWith(__PATCH))
            {
                //COLOR? body, border;
                //byte bold;
                //ParsePatch(out body, out border, out bold);
                //result = string.Format("{2}.{3} = PATCH.GetNinePatch(null, {0}, {1});",
                //    border.HasValue ? string.Format("new COLOR({0}, {1}, {2}, {3})", border.Value.R, border.Value.G, border.Value.B, border.Value.A) : "null",
                //    bold, instance, Variable.VariableName);
                COLOR border;
                byte bold;
                ParsePatch(out border, out bold);
                result = string.Format("{5}.{6} = PATCH.GetNinePatch(null, new COLOR({0}, {1}, {2}, {3}), {4});",
                    border.R, border.G, border.B, border.A,
                    bold, instance, Variable.VariableName);
            }
            else
            {
                switch (GenerateTime)
                {
                    case EGenerateTime.Initialize:
                        result = string.Format("{0}.{1} = Content.Load<{2}>({3});", instance, Variable.VariableName, Variable.Type.CodeName(), _SERIALIZE.CodeValue(VariableStringValue, true));
                        break;
                    case EGenerateTime.Load:
                        if (instance.StartsWith("  "))
                        {
                            result = string.Format("LoadAsync(Content.LoadAsync<{0}>(@{1}, {4} => {2}.{3} = {4}));", Variable.Type.CodeName(), _SERIALIZE.CodeValue(VariableStringValue, true), instance, Variable.VariableName, ASYNC_VAR);
                        }
                        else
                        {
                            result = string.Format("{5} = LoadAsync(Content.LoadAsync<{0}>(@{1}, {4} => {2}.{3} = {4}));", Variable.Type.CodeName(), _SERIALIZE.CodeValue(VariableStringValue, true), instance, Variable.VariableName, ASYNC_VAR, ASYNC);
                            result += string.Format("\r\nif ({0} != null) yield return {0};", ASYNC);
                        }
                        break;
                    default:
                        throw new ArgumentException("GenerateTime");
                }
            }
            codes[GenerateTime] = result;
            return codes;
        }
        protected internal override void SaveValue(BinaryWriter writer)
        {
            writer.Write(VariableStringValue);
        }
        protected internal override void LoadValue(object instance, BinaryReader reader)
        {
            string path;
            reader.Read(out path);
            if (instance != null)
                VariableValue = path;
        }
    }
    public class EditorCheckBox : EditorCommon
    {
        private CheckBox checkBox = new CheckBox();

        public EditorCheckBox()
        {
            checkBox.X = Seperator;
            checkBox.Width = EditableHeight;
            checkBox.Height = checkBox.Width;
            checkBox.SourceNormal = EditorUI.GetNinePatch(COLOR.Gold, COLOR.TransparentBlack);
            checkBox.SourceClicked = EditorUI.GetNinePatch(COLOR.TransparentBlack, COLOR.GreenYellow);
            checkBox.CheckedChanged += checkBox_CheckedChanged;
            Add(checkBox);
        }

        void checkBox_CheckedChanged(Button sender, Entry e)
        {
            VariableValue = sender.Checked;
        }
        protected override void SetValue()
        {
            base.SetValue();
            checkBox.Checked = (bool)VariableValue;
        }
    }
    public class EditorCheckBoxMultiple : EditorCommon
    {
        private Array array;

        public EditorCheckBoxMultiple(Type enumType)
        {
            Type utype = Enum.GetUnderlyingType(enumType);
            array = Enum.GetValues(enumType);
            for (int i = 0; i < array.Length; i++)
            {
                int value = Convert.ToInt32(array.GetValue(i));

                CheckBox checkBox = new CheckBox();
                checkBox.X = Seperator;
                checkBox.Y = EditableHeight * i;
                checkBox.Width = EditableHeight;
                checkBox.Height = checkBox.Width;
                checkBox.SourceNormal = PATCH.GetNinePatch(COLOR.TransparentBlack, COLOR.Black, 1);
                checkBox.SourceClicked = PATCH.GetNinePatch(COLOR.LawnGreen, COLOR.TransparentBlack, 1);
                checkBox.CheckedChanged += (sender, e) =>
                {
                    if (sender.Checked)
                    {
                        VariableValue = Convert.ChangeType(Convert.ToInt32(VariableValue) | value, utype);
                    }
                    else
                    {
                        VariableValue = Convert.ChangeType(Utility.EnumRemove(Convert.ToInt32(VariableValue), value), utype);
                    }
                };
                Add(checkBox);

                checkBox.Text = array.GetValue(i).ToString();
            }
        }
        protected override void SetValue()
        {
            base.SetValue();
            int index = 0;
            int value = Convert.ToInt32(VariableValue);
            foreach (var item in Childs)
            {
                CheckBox target = item as CheckBox;
                if (target == null)
                    continue;
                bool result = Utility.EnumContains(value, Convert.ToInt32(array.GetValue(index)));
                if (target.Checked != result)
                    target.Checked = result;
                index++;
            }
        }
    }
    public class EditorColor : EditorCommon
    {
        private TextureBox box;
        private NumberBox alpha;

        public EditorColor()
        {
            const float ALPHA_SIZE = 30;

            box = new TextureBox();
            box.X = Seperator;
            box.Width = EditableWidth - ALPHA_SIZE;
            box.Height = EditableHeight;
            box.Clicked += box_Clicked;
            box.Texture = TEXTURE.Pixel;
            box.DisplayMode = EViewport.Strength;
            Add(box);

            alpha = new NumberBox();
            alpha.MinValue = 0;
            alpha.MaxValue = 255;
            alpha.X = box.X + box.Width;
            alpha.Width = ALPHA_SIZE;
            alpha.Height = EditableHeight;
            alpha.DefaultText.TextAlignment = EPivot.TopCenter;
            alpha.DefaultText.Text = "A";
            alpha.UIText.TextAlignment = EPivot.TopCenter;
            alpha.ValueChanged += alpha_ValueChanged;
            Add(alpha);
        }

        void alpha_ValueChanged(Label sender, Entry e)
        {
            COLOR color = (COLOR)VariableValue;
            color.A = (byte)alpha.Value;
            VariableValue = color;
        }
        void box_Clicked(UIElement sender, Entry e)
        {
            COLOR? color = UtilityEditor.SelectColor(box.Color);
            if (color.HasValue)
            {
                COLOR value = color.Value;
                value.A = (byte)alpha.Value;
                VariableValue = value;
            }
        }
        protected override void SetValue()
        {
            base.SetValue();
            if (VariableValue != null)
            {
                //box.Color = (EEColor)VariableValue;
                box.Color = (COLOR)VariableValue;
            }
            else
            {
                //box.Color = EEColor.Default;
                box.Color = COLOR.Default;
            }
            //if (box.Color.A == 0)
            //{
            //    box.Color.A = 255;
            //}
            alpha.Value = box.Color.A;
        }
    }
    public class EditorUIElement : EditorCommon
    {
        private TextBox button;
        private EditSelectElement selector;

        public override string VariableStringValue
        {
            get { return button.Text; }
        }

        public EditorUIElement()
        {
            button = new TextBox();
            button.X = Seperator;
            button.Width = EditableWidth;
            button.Height = EditableHeight;
            button.SourceNormal = EditorUI.GetNinePatchBodyColor();
            button.UIText.FontColor = COLOR.Black;
            button.DefaultText.FontColor = new COLOR(32, 32, 32, 128);
            button.Clicked += new DUpdate<UIElement>(button_Clicked);
            Add(button);

            Label.Clicked += new DUpdate<UIElement>(Label_Clicked);
        }

        void button_Clicked(UIElement sender, Entry e)
        {
            selector.Start();
        }
        private void SelectUIElement(UIElement target)
        {
            if (target != null)
                VariableValue = target;
        }
        void Label_Clicked(UIElement sender, Entry e)
        {
            if (UtilityEditor.Confirm("", "是否要解除绑定？"))
            {
                VariableValue = null;
            }
        }
        protected override void SetValue()
        {
            if (selector == null)
            {
                selector = new EditSelectElement(SelectUIElement, Variable.Type);
            }

            base.SetValue();

            if (VariableValue == null)
            {
                button.Text = null;
                ElementLib lib = EditorUI.GetElementLib(Variable.Type);
                if (lib == null)
                    button.DefaultText.Text = "请选择" + Variable.Type.Name;
                else
                    button.DefaultText.Text = "请选择" + lib.Name;
            }
            else
            {
                UIElement element = VariableValue as UIElement;
                if (string.IsNullOrEmpty(element.Name))
                    button.Text = element.GetType().Name;
                else
                    button.Text = element.Name;
            }
        }
        protected internal override void SaveValue(BinaryWriter writer)
        {
            // write index
            if (VariableValue == null)
            {
                writer.Write(-1);
            }
            else
            {
                var target = (UIElement)VariableValue;
                UIElement parent = (UIElement)Variable.Instance;
                for (int i = 0; i < parent.ChildCount; i++)
                {
                    if (parent[i] == target)
                    {
                        writer.Write(i);
                        return;
                    }
                }
                throw new ArgumentNullException("没有在父控件中找到合适的项");
            }
        }
        protected internal override void LoadValue(object instance, BinaryReader reader)
        {
            int index;
            reader.Read(out index);
            EditorUI.OnLoadUICompleted += (e) =>
            {
                _LOG.Debug("OnLoadUICompleted: {0}", index);
                if (index == -1)
                    VariableValue = null;
                else
                    VariableValue = ((UIElement)Variable.Instance)[index];
            };
        }
        protected internal override Dictionary<EGenerateTime, string> GenerateCode(string instance)
        {
            var element = ((UIElement)VariableValue);
            Dictionary<EGenerateTime, string> codes = new Dictionary<EGenerateTime, string>();
            codes[GenerateTime] = string.Format("{0}.{1} = {2};", instance, Variable.VariableName, GeneratorUICode.BuildInstance(element));
            return codes;
        }
        protected override void InternalUpdate(Entry e)
        {
            base.InternalUpdate(e);
            if (VariableValue != null)
            {
                UIElement value = (UIElement)VariableValue;
                if (value.Parent != Variable.Instance)
                {
                    VariableValue = null;
                }
            }
        }
    }

    /// <summary>
    /// UIElement.Clip可以点击Width和Height设置成自动尺寸
    /// </summary>
    public class EditorClip : EditorAllObject
    {
        public EditorClip()
            : base(new Type[] { typeof(RECT) })
        {
        }
        protected override void SetValue()
        {
            base.SetValue();

            if (ValueEditor != null)
            {
                EditorCommon[] editors = ((EditorObject)ValueEditor).Editors.Select(e => (EditorCommon)e).ToArray();
                if (editors.Length > 0)
                {
                    EditorCommon editor = editors[2];
                    editor.Label.Clicked -= SetWidthAuto;
                    editor.Label.Clicked += SetWidthAuto;

                    editor = editors[3];
                    editor.Label.Clicked -= SetHeightAuto;
                    editor.Label.Clicked += SetHeightAuto;
                }
            }
        }
        private void SetWidthAuto(UIElement sender, Entry e)
        {
            RECT current = (RECT)VariableValue;
            current.Width = 0;
            VariableValue = current;
        }
        private void SetHeightAuto(UIElement sender, Entry e)
        {
            RECT current = (RECT)VariableValue;
            current.Height = 0;
            VariableValue = current;
        }
    }

    public class FromFile : Button
    {
        public event Action<string> OnDragFile;
        public string DefaultFile;
        public string[] Suffix = new string[0];

        public FromFile()
        {
            SourceNormal = EditorUI.GetNinePatch(COLOR.Gray, COLOR.White);
            SourceHover = EditorUI.GetNinePatch(COLOR.Gray, COLOR.Silver);
            Color = COLOR.Black;
            Click += new DUpdate<UIElement>(FromFile_Click);
        }

        void FromFile_Click(UIElement sender, Entry e)
        {
            string file = DefaultFile;
            if (UtilityEditor.OpenFile(ref file, Suffix))
            {
                DragFile(file);
            }
        }
        public virtual void DragFile(string file)
        {
            if (OnDragFile != null)
            {
                OnDragFile(file);
            }
        }
    }

    public interface IGeneratorTypeEditor
    {
        EditorVariable GeneratorEditor(Type type, object obj);
    }
    public class GeneratorDefault : IGeneratorTypeEditor
    {
        private static Dictionary<Type, List<Type>> cache = new Dictionary<Type, List<Type>>();

        public EditorVariable GeneratorEditor(Type type, object obj)
        {
            if (type.IsEnum)
            {
                if (type.HasAttribute<FlagsAttribute>())
                {
                    return new EditorCheckBoxMultiple(type);
                }
                else
                {
                    return new EditorComboBox(type);
                }
            }
            else if (type.IsArray)
            {
                throw new NotImplementedException();
            }
            else if (type == typeof(bool))
            {
                return new EditorCheckBox();
            }
            else if (type.IsNumber())
            {
                return new EditorNumber();
            }
            else if (type == typeof(string))
            {
                return new EditorTextBox();
            }
            else if (type == typeof(COLOR))
            {
                return new EditorColor();
            }
            //else if (type == typeof(TEXTURE))
            //{
            //}
            else if (type == typeof(TEXTURE)
                || type == typeof(FONT)
                || type == typeof(SHADER))
            {
                return new EditorAsset();
            }
            else if (type.IsNullable())
            {
                EditorVariable valueEditor = GeneratorEditor(Nullable.GetUnderlyingType(type), obj);
                return new EditorNullable(valueEditor);
            }
            else if (type != typeof(UIScene) && type.Is(typeof(UIElement)))
            {
                return new EditorUIElement();
            }
            else
            {
                IList<Type> types;
                if (type.IsValueType)
                {
                    types = new Type[] { type };
                }
                else
                {
                    List<Type> temp;
                    if (!cache.TryGetValue(type, out temp))
                    {
                        temp = UtilityEditor.LoadedAssemblies().GetTypes(type);
                        cache[type] = temp;
                    }
                    types = temp;
                }

                EditorAllObject all = new EditorAllObject(types);
                if (!all.SingleType && obj == null)
                    return all;

                ElementLib lib = EditorUI.GetElementLib(type);
                EditorVariable editor = new EditorObject();

                if (lib == null || lib.Variables.IsEmpty())
                {
                    SerializeSetting setting = SerializeSetting.DefaultSetting;
                    setting.Static = false;
                    setting.Property = true;

                    setting.Serialize(type, obj,
                        v =>
                        {
                            EditorVariable editor2 = GeneratorEditor(v.Type, v.GetValue());
                            editor2.Variable = v;
                            editor.Add(editor2);
                        });
                }
                else
                {
                    foreach (var item in lib.Variables)
                    {
                        VariableObject variable = new VariableObject(type, item.VariableName);

                        EditorVariable editor2;

                        if (string.IsNullOrEmpty(item.EditorType))
                            editor2 = GeneratorEditor(variable.Type, obj);
                        else
                            editor2 = (EditorVariable)UtilityEditor.GetDllType(item.EditorType).GetConstructor(Type.EmptyTypes).Invoke(new object[0]);

                        if (!item.Properties.IsEmpty())
                        {
                            foreach (var property in item.Properties)
                            {
                                VariableObject temp = new VariableObject(editor2, property.Key);
                                temp.SetValue(Convert.ChangeType(property.Value, temp.Type));
                            }
                        }

                        if (!string.IsNullOrEmpty(item.NickName))
                        {
                            EditorCommon common = editor2 as EditorCommon;
                            if (common != null)
                            {
                                common.Label.Text = item.NickName;
                            }
                        }

                        editor2.Variable = variable;

                        editor.Add(editor2);
                    }
                }

                all.ValueEditor = editor;
                return all;
            }
        }
    }

    public enum EGenerateTime
    {
        /// <summary>
        /// initialize only time
        /// </summary>
        Initialize,
        /// <summary>
        /// initialize while shown
        /// </summary>
        Load,
        /// <summary>
        /// initialize while show begin
        /// </summary>
        Show,
    }
    internal class GeneratorUICode
    {
        #region constant

        private const string TEMPLATE = "Template.txt";
        private const string TEMPLATE_DESIGN = "Template.design.txt";
        private const string TEMPLATE_CODE =
@"using EntryEngine;
using EntryEngine.UI;

public partial class {0} : UIScene
{{
	public {0}()
	{{
		Initialize();
	}}

    {1}

	protected virtual void LoadBegin()
	{{
	}}
	protected virtual void LoadEnd()
	{{
	}}
}}";
        private const string TEMPLATE_CODE_DESIGN =
@"using EntryEngine;
using EntryEngine.UI;

public partial class {0}
{{
	{1}

	private void Initialize()
	{{
		{2}
	}}
	protected override void Load(ContentManager content)
	{{
		LoadBegin();

		{3}

		LoadEnd();
	}}
}}";
        private static Dictionary<Type, object> Default = new Dictionary<Type, object>();

        #endregion

        public void Generate(Widget target)
        {
            string className = Path.GetFileNameWithoutExtension(target.FilePath);
            StringBuilder builder = new StringBuilder();
            StringBuilder builderDesign = new StringBuilder();

            // use project template / editor template
            if (!CheckTemplate(className, TEMPLATE, ref builder))
                builder.AppendFormat(TEMPLATE_CODE);
            if (!CheckTemplate(className, TEMPLATE_DESIGN, ref builderDesign))
                builderDesign.Append(TEMPLATE_CODE_DESIGN);

            // fields
            StringBuilder builder1 = new StringBuilder();
            // initialize code
            StringBuilder builder2 = new StringBuilder();
            // load code
            StringBuilder builder3 = new StringBuilder();
            builder3.AppendLine("ICoroutine {0};", EditorAsset.ASYNC);
            // show code
            StringBuilder builder4 = new StringBuilder();

            List<EditorVariable> editors = target.Target.GetEditors().ToList();
            UIScene scene = target.Target.Instance as UIScene;

            // translate table
            StringTable table = new StringTable("Source", "ID", "Word");
            table.AddRow("string", "String", "string");
            table.AddRow("源控件", "语言ID", "文字");
            bool flagTranslated = false;
            UIElement.ForParentPriority(scene, null, e =>
            {
                if (e == scene)
                    return;
                Widget widget = e.Tag as Widget;
                foreach (var temp in widget.Target.GetEditors()
                    .SelectMany(t => GetTextEditor(t))
                    .Where(t => t != null && !(t is EditorNumber) && t.Variable.VariableName != "Name" && t.Translate))
                {
                    flagTranslated = true;
                    table.AddRow(e.Name, temp.VariableStringValue, temp.VariableStringValue);
                }
            });
            string csv = Path.Combine(Path.GetDirectoryName(target.FilePath), scene.Name + ".csv");
            bool exists = File.Exists(csv);
            if (flagTranslated)
            {
                // write language table
                CSVWriter writer = new CSVWriter();
                writer.WriteTable(table);
                File.WriteAllText(csv, writer.Result, Encoding.UTF8);

                // translate table
                EditorUI.TranslateUI(csv);

                // reload language table
                CSVReader reader = new CSVReader(File.ReadAllText(csv, Encoding.UTF8));
                table = reader.ReadTable();
            }
            else
            {
                if (exists)
                {
                    _LOG.Debug("删除{0}", csv);
                    File.Delete(csv);
                    // translate table
                    EditorUI.TranslateUI(csv);
                }
            }
            EditorTextBox.Language = table;

            // elements group by name, generate elements in array
            elements = new Dictionary<string, UIElement[]>();
            UIElement.ForParentPriority(scene,
                e =>
                {
                    // 不生成预设场景的控件
                    if (e != scene && e is UIScene && EditorUI.HasPreElement(e.Name))
                    {
                        if (elements.ContainsKey(e.Name))
                        {
                            elements[e.Name] = elements[e.Name].Add(e);
                        }
                        else
                        {
                            elements[e.Name] = new UIElement[] { e };
                        }
                        return true;
                    }
                    return false;
                },
                e =>
                {
                    if (e == scene)
                        return;
                    if (elements.ContainsKey(e.Name))
                    {
                        elements[e.Name] = elements[e.Name].Add(e);
                    }
                    else
                    {
                        elements[e.Name] = new UIElement[] { e };
                    }
                });
            foreach (var item in elements)
            {
                var ee = item.Value[0];
                Type t = item.Value[0].GetType();
                string type;
                if (item.Value[0] is UIScene && EditorUI.HasPreElement(ee.Name))
                    type = ee.Name;
                else
                    type = t.CodeName();
                if (item.Value.Length > 1)
                {
                    // array element
                    builder1.AppendLine("public {0}[] {1} = new {0}[{2}]", type, item.Key, item.Value.Length);
                    builder1.AppendLine("{");
                    foreach (var element in item.Value)
                    {
                        builder1.AppendLine("new {0}(),", type);
                    }
                    builder1.AppendLine("};");
                }
                else
                {
                    //if (t == typeof(UIScene))
                    //    builder1.AppendLine("public {0} {0} = new {0}();", item.Key);
                    //else
                    builder1.AppendLine("public {0} {1} = new {0}();", type, item.Key);
                }
            }

            UIElement.ForParentPriority(scene, null,
                e =>
                {
                    if (e != scene && e is UIScene
                        // 不Add的控件不显示
                        // 若希望场景脱离父控件而显示，则改为两个空格，仅仅生成的控件不显示
                        && !e.Name.StartsWith(" ")
                        // 预设场景里的子场景
                        && elements.ContainsKey(e.Name))
                    {
                        string name = BuildInstance(elements, e);
                        if (EditorUI.HasPreElement(e.Name))
                        {
                            builder3.AppendLine("{0} = Entry.ShowDialogScene<{0}>(EState.None);", name);
                            builder3.AppendLine("{0}.X = {1}f;", name, e.X);
                            builder3.AppendLine("{0}.Y = {1}f;", name, e.Y);
                            builder3.AppendLine("this.Add({0});", name);
                        }
                        else
                        {
                            builder3.AppendLine("Entry.ShowDialogScene({0}, EState.None);", BuildInstance(elements, e));
                        }
                    }
                });
            UIElement generating = null;
            // generate element code by instance of EditorVariable in element's Tag
            UIElement.ForParentPriority(scene,
                    e =>
                    {
                        if (e != scene && e is UIScene)
                        {
                            UIScene childScene = e as UIScene;
                            //builder3.AppendLine("Entry.ShowDialogScene({0});", childScene.Name);
                            // 保存的预设场景不重新生成代码
                            if (EditorUI.HasPreElement(childScene.Name))
                            {
                                //string name = BuildInstance(elements, childScene);
                                //builder2.AppendLine("{0}.X = {1}f;", name, childScene.X);
                                //builder2.AppendLine("{0}.Y = {1}f;", name, childScene.Y);
                                //builder2.AppendLine("this.Add({0});", name);
                                return true;
                            }
                        }
                        return false;
                    },
                    e =>
                    {
                        //if (e != scene && e is UIScene)
                        //{
                        //    UIScene childScene = e as UIScene;
                        //    builder2.AppendLine("this.Add({0});", childScene.Name);
                        //    builder4.AppendLine("Entry.ShowDialogScene({0}, true);", childScene.Name);
                        //    return;
                        //}

                        IEditorType editor = (e.Tag as Widget).Target;
                        if (editor == null)
                            throw new ArgumentNullException("Widget in Tag");

                        // don't generate code while value has not be modified
                        object _default;
                        Type type = editor.Instance.GetType();
                        if (!Default.TryGetValue(type, out _default))
                        {
                            _default = editor.Instance.GetType().GetConstructor(Type.EmptyTypes).Invoke(new object[0]);
                            Default[type] = _default;
                        }

                        // this / entity / entity[index]
                        string instance;
                        UIElement element = editor.Instance as UIElement;
                        instance = BuildInstance(elements, element);

                        bool generated2 = generating != null && generating.Find(u => u == e) != null;
                        if (!generated2 && generating != null)
                        {
                            builder1.AppendLine("return {0};", generating.Name.Trim());
                            builder1.AppendLine("}");
                            generating = null;
                        }
                        bool generated = e.Name.StartsWith("  ");
                        if (generated && generated2)
                            throw new ArgumentException("只能同时构建一个元素");
                        generated |= generated2;
                        if (generated)
                        {
                            string typeName = type.CodeName();
                            string name = e.Name.Trim();
                            if (!generated2)
                            {
                                builder1.AppendLine("private {0} ___{1}()", typeName, name);
                                builder1.AppendLine("{");
                            }
                            builder1.AppendLine("var {0} = new {1}();", name, typeName);
                        }

                        foreach (EditorVariable item in editor.GetEditors())
                        {
                            object value1 = item.Variable.GetValue();
                            IVariable defaultValue = new VariableObject(_default, item.Variable.VariableName);
                            object value2 = defaultValue.GetValue();
                            if (object.Equals(value1, value2))
                                continue;
                            if (value1 == null && value2.Equals(value1))
                                continue;
                            else if (value1.Equals(value2))
                                continue;

                            Dictionary<EGenerateTime, string> generate = item.GenerateCode(instance);
                            foreach (var code in generate)
                            {
                                switch (code.Key)
                                {
                                    case EGenerateTime.Initialize:
                                        if (!generated)
                                            builder2.AppendLine(code.Value);
                                        break;

                                    case EGenerateTime.Load:
                                        builder3.AppendLine(code.Value);
                                        break;

                                    case EGenerateTime.Show:
                                        if (!generated)
                                            builder4.AppendLine(code.Value);
                                        break;

                                    default:
                                        throw new ArgumentException("item.GenerateTime");
                                }
                                if (generated)
                                    builder1.AppendLine(code.Value);
                            }
                        }

                        if (generated && !generated2)
                            generating = e;

                        if (e != scene && !e.Name.StartsWith(" "))
                            builder2.AppendLine("{0}.Add({1});", BuildInstance(elements, e.Parent), instance);
                        if (generated2)
                            builder1.AppendLine("{0}.Add({1});", BuildInstance(elements, e.Parent), instance);
                    });

            if (generating != null)
            {
                builder1.AppendLine("return {0};", generating.Name.Trim());
                builder1.AppendLine("}");
            }

            string result = _SERIALIZE.Indent(string.Format(builder.ToString(), className));
            string result_auto = _SERIALIZE.Indent(string.Format(builderDesign.ToString(), className, builder1.ToString(), builder2.ToString(), builder3.ToString(), builder4.ToString()));

            string cs = Path.Combine(EditorUI.DIR_UI, className + EditorUI.SUFFIX_CS);
            if (!File.Exists(cs))
                File.WriteAllText(cs, result, Encoding.UTF8);

            cs = Path.Combine(EditorUI.DIR_UI, className + EditorUI.SUFFIX_DESIGN_CS);
            if (!Directory.Exists(EditorUI.DIR_UI))
                Directory.CreateDirectory(EditorUI.DIR_UI);
            File.WriteAllText(cs, result_auto, Encoding.UTF8);

            EditorTextBox.Language = null;
        }
        //public Dictionary<EGenerateTime, string> GenerateCode(UIElement target)
        //{
        //    Dictionary<EGenerateTime, string> codes = new Dictionary<EGenerateTime, string>();

        //    IEditorType editor = (target.Tag as Widget).Target;
        //    List<EditorVariable> editors = editor.GetEditors().ToList();

        //    // elements group by name, generate elements in array
        //    Dictionary<string, UIElement[]> elements = new Dictionary<string, UIElement[]>();
        //    UIElement.ForeachAllParentPriority(target, null, e =>
        //    {
        //        if (elements.ContainsKey(e.Name))
        //        {
        //            elements[e.Name] = elements[e.Name].Add(e);
        //        }
        //        else
        //        {
        //            elements[e.Name] = new UIElement[] { e };
        //        }
        //    });
        //    //foreach (var item in elements)
        //    //{
        //    //    builder1.AppendFormatLine("public {0}{2} {1};", item.Value[0].GetType().CodeName(), item.Key, item.Value.Length > 1 ? "[]" : string.Empty);
        //    //}

        //    // generate element code by instance of EditorVariable in element's Tag
        //    UIElement.ForeachAllParentPriority(target, null, e =>
        //    {
        //        editor = (e.Tag as Widget).Target;
        //        if (editor == null)
        //            throw new ArgumentNullException("IEditorType in Tag");

        //        // don't generate code while value has not be modified
        //        object _default;
        //        Type type = editor.Instance.GetType();
        //        if (!Default.TryGetValue(type, out _default))
        //        {
        //            _default = editor.Instance.GetType().GetConstructor(Type.EmptyTypes).Invoke(new object[0]);
        //            Default[type] = _default;
        //        }

        //        // this / entity / entity[index]
        //        string instance;
        //        UIElement element = editor.Instance as UIElement;
        //        UIElement[] items;
        //        if (elements.TryGetValue(element.Name, out items))
        //        {
        //            if (items.Length == 1)
        //            {
        //                instance = element.Name;
        //            }
        //            else
        //            {
        //                int index = items.IndexOf(element);
        //                instance = string.Format("{0}[{1}]", element.Name, index);
        //            }
        //        }
        //        else
        //        {
        //            throw new ArgumentNullException("element.Name");
        //            //instance = "this";
        //        }

        //        foreach (EditorVariable item in editor.GetEditors())
        //        {
        //            object value1 = item.Variable.GetValue();
        //            IVariable defaultValue = new VariableObject(_default, item.Variable.VariableName);
        //            object value2 = defaultValue.GetValue();
        //            if (object.Equals(value1, value2))
        //                continue;
        //            if (value1 == null && value2.Equals(value1))
        //                continue;
        //            else if (value1.Equals(value2))
        //                continue;

        //            CombineCode(codes, item.GenerateCode(instance));
        //        }

        //        if (e != target)
        //        {
        //            Dictionary<EGenerateTime, string> last = new Dictionary<EGenerateTime, string>();
        //            last.Add(EGenerateTime.Initialize, string.Format("{0}.Add({1});", e.Parent == target ? "this" : e.Parent.Name, instance));
        //            CombineCode(codes, last);
        //        }
        //    });

        //    return codes;
        //}
        private static Dictionary<string, UIElement[]> elements;
        private bool CheckTemplate(string className, string template, ref StringBuilder builder)
        {
            // use project template / editor template
            string path = Path.Combine(EditorUI.Instance.DirectoryProject, template);
            if (!File.Exists(path))
            {
                path = Path.Combine(EditorUI.Instance.DirectoryEditor, template);
                if (!File.Exists(path))
                {
                    path = null;
                }
            }

            bool useTemplateSuccess = false;
            if (path != null)
            {
                try
                {
                    builder.AppendLine(File.ReadAllText(path));
                    useTemplateSuccess = true;
                }
                catch (Exception ex)
                {
                    _LOG.Debug("模板文件格式错误！模板文件：{0}\r\n{1}", path, ex.Message);
                }
            }

            return useTemplateSuccess;
        }

        public static string BuildInstance(UIElement target)
        {
            return BuildInstance(elements, target);
        }
        private static string BuildInstance(Dictionary<string, UIElement[]> elements, UIElement element)
        {
            string instance;
            UIElement[] items;
            if (elements.TryGetValue(element.Name, out items))
            {
                if (items.Length == 1)
                {
                    instance = element.Name;
                }
                else
                {
                    int index = items.IndexOf(element);
                    instance = string.Format("{0}[{1}]", element.Name, index);
                }
            }
            else
            {
                instance = "this";
            }

            return instance;
        }
        public static void CombineCode(Dictionary<EGenerateTime, string> codes, Dictionary<EGenerateTime, string> generate)
        {
            foreach (var item in generate)
            {
                string result = null;
                if (codes.ContainsKey(item.Key))
                    result = codes[item.Key];
                if (result != null && !result.EndsWith("\r\n"))
                    result += "\r\n";
                result += item.Value;
                codes[item.Key] = result;
            }
        }
        private static IEnumerable<EditorTextBox> GetTextEditor(EditorVariable editor)
        {
            EditorTextBox result = editor as EditorTextBox;
            if (result != null)
                yield return result;

            EditorAllObject all = editor as EditorAllObject;
            if (all != null && all.ValueEditor != null)
                foreach (var item in GetTextEditor(all.ValueEditor))
                    yield return item;

            EditorObject obj = editor as EditorObject;
            if (obj != null)
                foreach (var item in obj.Editors)
                    foreach (var item2 in GetTextEditor(item))
                        yield return item2;
        }
    }

    public class UIFilter : ISerializeFilter
    {
        private static ISerializeFilter _default = new SerializeValidatorDefault();

        public ElementLib Lib;

        public bool SkipField(FieldInfo field)
        {
            if (field.ReflectedType != null)
            {
                var lib = EditorUI.GetElementLib(field.ReflectedType);
                if (lib != null)
                    return lib.Variables.FirstOrDefault(v => v.VariableName == field.Name) == null;
            }
            return _default.SkipField(field);
        }
        public bool SkipProperty(PropertyInfo property)
        {
            if (property.ReflectedType != null)
            {
                var lib = EditorUI.GetElementLib(property.ReflectedType);
                if (lib != null)
                    return lib.Variables.FirstOrDefault(v => v.VariableName == property.Name) == null;
            }
            return true;
            //return _default.SkipProperty(property);
        }
    }
}
