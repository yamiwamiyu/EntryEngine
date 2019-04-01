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
                //if (Project == null)
					value = Environment.CurrentDirectory;
                //else
                //    value = Path.GetFullPath(Path.GetDirectoryName(Project.Name));
                return _IO.DirectoryWithEnding(value);
			}
			protected set
			{
                //if (Project == null)
					Environment.CurrentDirectory = value;
                //else
                //    Project.Name = Path.GetFullPath(value);
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

            EditorVariable.WIDTH = 150;
            EditorVariable.HEIGHT = 21;
            EditorVariable.CONTENT = Content;
            EditorVariable.GENERATOR.Setting = GetSetting();
            EditorVariable.GENERATOR.Generate = OnGenerateUIElement;
            EditorVariable.GENERATOR.OnGenerated += GENERATOR_OnGenerated;
            EditorCommon.Initialize();

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
                if (string.IsNullOrEmpty(C.EntryBuilder))
                {
                    UtilityEditor.Message("请先配置生成工具");
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

            string targetFile = Path.Combine(DirectoryEditor, Project.Name);
            File.WriteAllText(targetFile, node.OutterXml);
        }
        private void OnOpenProject()
        {
            string fullProjectName = Path.GetFullPath(Project.Name);
            string fullDir = Path.GetDirectoryName(fullProjectName);
            Environment.CurrentDirectory = fullDir;
            contentPreview.IODevice.RootDirectory = fullDir;
            Content.RootDirectory = Project.ContentDirectory;

            Project.Name = _IO.RelativePath(fullProjectName, _IO.DirectoryWithEnding(DirectoryEditor));

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
            if (WritingProperty == null || value == WritingProperty)
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
        public static UIFilter FILTER = new UIFilter();
        public static byte[] SaveUI(Widget target, bool reff = false)
        {
            ByteRefWriter writer = new ByteRefWriter(GetSetting());
            writer.OnSerialize += TEXTURE.Serializer;
            writer.OnSerialize += EntryEngine.Content.Serializer;
            //writer.OnSerialize += UISerializer;
            writer.Write(target.Target.Instance.GetType().SimpleAQName());
            InternalSaveUI(target, writer, reff);
            return writer.GetBuffer();
        }
        public static void InternalSaveUI(Widget target, ByteRefWriter writer, bool reff = false)
        {
            // refference
            bool writeRef = reff && !target.IsSaveAs;
            writer.Write(writeRef ? target.FilePath : null);

            UIElement element = target.Target.Instance as UIElement;
            //writer.Write(element.GetType().SimpleAQName());

            if (!writeRef)
            {
                /* 放在属性前加载子控件，停靠的子控件在父控件设置宽高属性时将会变形 */
                writer.Write(element.ChildCount);
                for (int i = 0; i < element.ChildCount; i++)
                    InternalSaveUI((Widget)element[i].Tag, writer, true);
            }

            writer.OnSerialize += UISerializer;
            WritingProperty = element;
            element.ResetContentSize();
            writer.WriteObject(element, typeof(UIElement));
            WritingProperty = null;
            writer.OnSerialize -= UISerializer;
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
            var target = LoadUI(File.ReadAllBytes(file), true);
            (target.Tag as Widget).FilePath = file;
            return target;
        }
        public static UIElement LoadUI(byte[] buffer, bool reff = false)
        {
            var reader = GetReader(buffer);
            string type;
            reader.Read(out type);
            UIElement target = InternalLoadUI(reader, reff);
            return target;
        }
        public static Type LoadElementType(string file)
        {
            ByteRefReader reader = new ByteRefReader(File.ReadAllBytes(file));
            string key;
            reader.Read(out key);
            return _SERIALIZE.LoadSimpleAQName(key);
        }
        public static UIElement InternalLoadUI(ByteRefReader reader, bool reff)
        {
            string file;
            reader.Read(out file);
            UIElement reference = null;
            if (reff && !string.IsNullOrEmpty(file) && File.Exists(file))
            {
                try
                {
                    reference = LoadUI((File.ReadAllBytes(file)), false);
                    reference.Tag = file;
                }
                catch (Exception ex)
                {
                    _LOG.Error("Load file:{0} error:{1}!", file, ex.Message);
                }
            }

            // childs
            UIElement[] childs = null;
            if (reference == null)
            {
                int count;
                reader.Read(out count);
                if (count > 0)
                {
                    childs = new UIElement[count];
                    for (int i = 0; i < count; i++)
                        childs[i] = InternalLoadUI(reader, true);
                }
            }

            var deserializer = UIDeserializer(childs);
            reader.OnDeserialize += deserializer;
            UIElement instance = reader.ReadObject<UIElement>();
            if (reference == null)
            {
                if (childs != null)
                    instance.AddRange(childs);
            }
            else
            {
                reference.X = instance.X;
                reference.Y = instance.Y;
                reference.Pivot = instance.Pivot;
                reference.Anchor = instance.Anchor;
                instance = reference;
            }
            reader.OnDeserialize -= deserializer;

            // editor should be built by instance
            Widget view = SetElement(instance);

            return instance;
        }

        public static EditorVariable OnGenerateUIElement(IVariable variable)
        {
            if (variable.MemberInfo != null)
            {
                ElementLib generatingLib = null;
                if (variable.MemberInfo.ReflectedType != null)
                    generatingLib = GetElementLib(variable.MemberInfo.ReflectedType);
                if (generatingLib != null)
                {
                    var generating = generatingLib.Variables.FirstOrDefault(v => v.VariableName == variable.VariableName);
                    if (generating != null && !string.IsNullOrEmpty(generating.EditorType))
                        return (EditorVariable)UtilityEditor.GetDllType(generating.EditorType).GetConstructor(Type.EmptyTypes).Invoke(new object[0]);
                }
            }
            if (variable.Type.Is(typeof(UIElement)))
            {
                return new EditorUIElement();
            }
            return null;
        }
        private static void GENERATOR_OnGenerated(IVariable variable, EditorVariable ev)
        {
            if (variable.MemberInfo == null)
                return;
            EditorCommon editor = new EditorCommon(ev);
            editor.Text = variable.VariableName;
            ElementLib generatingLib = null;
            if (variable.MemberInfo.ReflectedType != null)
            {
                generatingLib = GetElementLib(variable.MemberInfo.ReflectedType);
            }
            if (generatingLib != null)
            {
                var generating = generatingLib.Variables.FirstOrDefault(v => v.VariableName == variable.VariableName);
                if (generating != null)
                {
                    if (!generating.Properties.IsEmpty())
                    {
                        foreach (var property in generating.Properties)
                        {
                            VariableObject temp = new VariableObject(ev, property.Key);
                            temp.SetValue(Convert.ChangeType(property.Value, temp.Type));
                        }
                    }

                    if (!string.IsNullOrEmpty(generating.NickName))
                        editor.Text = generating.NickName;
                }
            }
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
                        EditorVariable editor = EditorVariable.GENERATOR.GenerateEditor(variable);
                        // Set variable
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
            if (e.INPUT.Pointer.IsClick(0) && SelectedElement == null)
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
                spriteBatch.Draw(patchViewportFocusBorder, focus.ViewClip, GRAPHICS.NullSource, C.ColorFocusBorder);

                //string text = focus.Name;
                //if (string.IsNullOrEmpty(text))
                //{
                //    text = focus.GetType().Name;
                //}
                //spriteBatch.Draw(FONT.Default, text, e.INPUT.Pointer.Position, COLOR.Red);
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
                spriteBatch.Draw(TEXTURE.Pixel, line, GRAPHICS.NullSource, C.ColorAlignLine);
                // 坐标显示
                spriteBatch.Draw(FONT.Default, result.ToString(), location, COLOR.Black);
                foreach (UIElement align in item.AlignTargets)
                {
                    spriteBatch.Draw(patchViewportAlign, align.ViewClip, GRAPHICS.NullSource, C.ColorAlignLine);
                }
            }

            if (CurrentEditMode != null)
            {
                spriteBatch.Draw(TEXTURE.Pixel, Entry.GRAPHICS.FullGraphicsArea, GRAPHICS.NullSource, CurrentEditMode.BackColor);
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
            EditorVariable editor = EditorVariable.GENERATOR.GenerateEditor(new VariableValue(Instance));
            //editor.Variable = new VariableValue(Instance);
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
                    InternalAddVariable(new VariableObject(Instance, v.VariableName), EditorVariable.GENERATOR.GenerateEditor(v), auto);
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
			if (target.Any(e => e.Variable.VariableName == variable.VariableName))
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
                editor = EditorVariable.GENERATOR.GenerateEditor(variable);
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

    public class EditorCommon : Label
    {
        public static COLOR BGBorderColor = new COLOR(160, 160, 160, 222);
        public static COLOR BGBodyColor = new COLOR(96, 96, 96, 255);
        public static PATCH PatchLabel;
        public static float WIDTH = 150;

        public static void Initialize()
        {
            PatchLabel = PATCH.GetNinePatch(COLOR.TransparentBlack, BGBorderColor, 1);
            PatchLabel.Left = 0;
            PatchLabel.Top = 0;
            PatchLabel.Bottom = PatchLabel.Height;

            //EditorVariable.FONT.FontSize = 14;
        }

        public EditorCommon(EditorVariable editor)
        {
            editor.Background = PATCH.GetNinePatch(BGBodyColor, BGBorderColor, 1);
            //this.Hover += HoverArea;
            //this.UnHover += UnHoverArea;
            SourceNormal = PatchLabel;
            Font = EditorVariable.FONT;
            Width = WIDTH;
            UIText.Padding.X = 12;

            //editor.OnSetVariable += new Action<IVariable>(editor_OnSetVariable);
            foreach (var item in editor)
                if (!(item is EditorVariable))
                    item.X += WIDTH;
            editor.Add(this);
        }

        //private static void HoverArea(UIElement sender, Entry e)
        //{
        //    ((Panel)sender).Background.Color = BGBorderHoverColor;
        //}
        //private static void UnHoverArea(UIElement sender, Entry e)
        //{
        //    ((Panel)sender).Background.Color = BGBorderColor;
        //}
    }

	// Editor
    public class EditorUIElement : EditorVariable
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
            button.Width = WIDTH;
            button.Height = HEIGHT;
            button.SourceNormal = EditorUI.GetNinePatchBodyColor();
            button.UIText.FontColor = COLOR.Black;
            button.DefaultText.FontColor = new COLOR(32, 32, 32, 128);
            button.Clicked += new DUpdate<UIElement>(button_Clicked);
            button.Hover += new DUpdate<UIElement>(CancelBind_RightClicked);
            Add(button);
        }

        void button_Clicked(UIElement sender, Entry e)
        {
            selector.Start();
        }
        private void SelectUIElement(UIElement target)
        {
            if (target != null)
            {
                // 将坐标转换到父类中的坐标
                var parent = target.Parent;
                VECTOR2 pos = target.Location;
                VariableValue = target;
                if (target.Parent != parent)
                {
                    target.Location = parent.ConvertLocalToOther(pos, target.Parent);
                }
            }
        }
        void CancelBind_RightClicked(UIElement sender, Entry e)
        {
            if (e.INPUT.Pointer.IsClick(1))
            {
                if (UtilityEditor.Confirm("", "是否要解除绑定？"))
                {
                    VariableValue = null;
                }
            }
        }
        protected override void SetValue()
        {
            if (selector == null)
                selector = new EditSelectElement(SelectUIElement, Variable.Type);

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
    /// <summary>UIElement.Clip可以点击Width和Height设置成自动尺寸</summary>
    public class EditorClip : EditorAllObject
    {
        public EditorClip()
            : base(new Type[] { typeof(RECT) })
        {
            this.OnCreateValueEditor += new Action<EditorVariable>(EditorClip_OnCreateValueEditor);
            this.OnGetValue += new Func<object, object>(EditorClip_OnGetValue);
        }

        object EditorClip_OnGetValue(object arg)
        {
            var ins = ((UIElement)Variable.Instance);
            RECT rect = (RECT)arg;
            if (ins.IsAutoWidth)
                rect.Width = 0;
            if (ins.IsAutoHeight)
                rect.Height = 0;
            return rect;
        }
        void EditorClip_OnCreateValueEditor(EditorVariable obj)
        {
            EditorVariable[] editors = ((EditorObject)ValueEditor).Editors.ToArray();
            EditorVariable editor = editors[2];
            editor.Hover -= SetWidthAuto;
            editor.Hover += SetWidthAuto;
            editor.OnGetValue -= GetWidth;
            editor.OnGetValue += GetWidth;
            editor = editors[3];
            editor.Hover -= SetHeightAuto;
            editor.Hover += SetHeightAuto;
            editor.OnGetValue -= GetHeight;
            editor.OnGetValue += GetHeight;
        }

        object GetWidth(object arg)
        {
            if (((UIElement)Variable.Instance).IsAutoWidth)
                return 0;
            else
                return arg;
        }
        object GetHeight(object arg)
        {
            if (((UIElement)Variable.Instance).IsAutoHeight)
                return 0;
            else
                return arg;
        }
        void SetWidthAuto(UIElement sender, Entry e)
        {
            if (e.INPUT.Pointer.IsClick(1))
            {
                RECT current = (RECT)VariableValue;
                current.Width = 0;
                VariableValue = current;
            }
        }
        void SetHeightAuto(UIElement sender, Entry e)
        {
            if (e.INPUT.Pointer.IsClick(1))
            {
                RECT current = (RECT)VariableValue;
                current.Height = 0;
                VariableValue = current;
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
    internal static class GeneratorUICode
	{
		#region constant

		private const string TEMPLATE = "Template.txt";
		private const string TEMPLATE_DESIGN = "Template.design.txt";
		private const string TEMPLATE_CODE =
@"using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;

public partial class {0} : UIScene
{{
	public {0}()
	{{
		Initialize();
	}}

	private IEnumerable<ICoroutine> MyLoading()
    {{
        return null;
    }}
}}";
		private const string TEMPLATE_CODE_DESIGN =
@"using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;

public partial class {0}
{{
	{1}

	private void Initialize()
	{{
		{2}
		this.PhaseShowing += Show;
	}}
	protected override IEnumerable<ICoroutine> Loading()
	{{
		ICoroutine async;
		{3}
        var __loading = MyLoading();
        if (__loading != null)
            foreach (var item in __loading)
                yield return item;
	}}
	private void Show(EntryEngine.UI.UIScene __scene)
	{{
		{4}
	}}
}}";
		private static Dictionary<Type, object> Default = new Dictionary<Type, object>();

		#endregion

        private const string ASYNC_VAR = "___c";
        private const string ASYNC = "___async";
        private static StringTable Language;
        public static SerializeSetting SETTING = SerializeSetting.DefaultSetting;

        public static void Generate(Widget target)
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
            builder3.AppendLine("ICoroutine {0};", ASYNC);
            // show code
            StringBuilder builder4 = new StringBuilder();

            List<EditorVariable> editors = target.Target.GetEditors().ToList();
            UIScene scene = target.Target.Instance as UIScene;

            if (EditorUI.Instance.Project.HasTranslateTable)
            {
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
                        //.Where(t => t != null && !(t is EditorNumber) && t.Variable.VariableName != "Name" && t.Translate))
                        .Where(t => t != null && !(t is EditorNumber) && t.Variable.VariableName != "Name" && IsTranslate(t.VariableStringValue)))
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
                Language = table;
            }

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
                        //if (EditorUI.HasPreElement(e.Name))
                        //{
                        //    if (name.EndsWith("]"))
                        //    {
                        //        builder3.AppendLine("{0} = new {1}();", name, e.Name);
                        //        builder3.AppendLine("Entry.ShowDialogScene({0}, EState.None);", name);
                        //    }
                        //    else
                        //    {
                        //        //builder3.AppendLine("{0} = Entry.ShowDialogScene<{1}>(EState.None);", name, e.Name);
                        //        builder3.AppendLine("Entry.ShowDialogScene({0}, EState.None);", name);
                        //    }
                        //    builder3.AppendLine("{0}.X = {1}f;", name, e.X);
                        //    builder3.AppendLine("{0}.Y = {1}f;", name, e.Y);
                        //    builder3.AppendLine("{1}.Add({0});", name, BuildInstance(elements, e.Parent));
                        //}
                        //else
                        //{
                        //    builder3.AppendLine("Entry.ShowDialogScene({0}, EState.None);", name);
                        //}
                        if (EditorUI.HasPreElement(e.Name))
                        {
                            if (name.EndsWith("]"))
                            {
                                builder2.AppendLine("{0} = new {1}();", name, e.Name);
                                builder3.AppendLine("Entry.ShowDialogScene({0}, EState.None);", name);
                            }
                            else
                            {
                                //builder3.AppendLine("{0} = Entry.ShowDialogScene<{1}>(EState.None);", name, e.Name);
                                builder3.AppendLine("Entry.ShowDialogScene({0}, EState.None);", name);
                            }
                            builder2.AppendLine("{0}.X = {1}f;", name, e.X);
                            builder2.AppendLine("{0}.Y = {1}f;", name, e.Y);
                            builder2.AppendLine("{1}.Add({0});", name, BuildInstance(elements, e.Parent));
                        }
                        else
                        {
                            builder3.AppendLine("Entry.ShowDialogScene({0}, EState.None);", name);
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
                                generating = e;
                                builder1.AppendLine("private {0} ___{1}()", typeName, name);
                                builder1.AppendLine("{");
                            }
                            builder1.AppendLine("var {0} = new {1}();", name, typeName);
                        }
                        // instance的子对象加载资源也应该同步
                        if (generated2)
                            instance = "  " + instance;

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

                            Dictionary<EGenerateTime, string> generate = GenerateCode(instance, item);
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

                        //if (generated && !generated2)
                        //    generating = e;

                        if (e != scene && !e.Name.StartsWith(" ") && !generated)
                            builder2.AppendLine("{0}.Add({1});", BuildInstance(elements, e.Parent), instance);
                        if (generated2)
                            builder1.AppendLine("{0}.Add({1});", BuildInstance(elements, e.Parent), instance);
                    });

            if (generating != null)
            {
                builder1.AppendLine("return {0};", generating.Name.Trim());
                builder1.AppendLine("}");
            }

            string result = _RH.Indent(string.Format(builder.ToString(), className));
            string result_auto = _RH.Indent(string.Format(builderDesign.ToString(), className, builder1.ToString(), builder2.ToString(), builder3.ToString(), builder4.ToString()));

            string cs = Path.Combine(EditorUI.DIR_UI, className + EditorUI.SUFFIX_CS);
            if (!File.Exists(cs))
                File.WriteAllText(cs, result, Encoding.UTF8);

            cs = Path.Combine(EditorUI.DIR_UI, className + EditorUI.SUFFIX_DESIGN_CS);
            if (!Directory.Exists(EditorUI.DIR_UI))
                Directory.CreateDirectory(EditorUI.DIR_UI);
            File.WriteAllText(cs, result_auto, Encoding.UTF8);

            Language = null;
        }
        private static Dictionary<string, UIElement[]> elements;
		private static bool CheckTemplate(string className, string template, ref StringBuilder builder)
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

        private static string BuildInstance(UIElement target)
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
        private static void CombineCode(Dictionary<EGenerateTime, string> codes, Dictionary<EGenerateTime, string> generate)
        {
            foreach (var item in generate)
                CombineCode(codes, item.Key, item.Value);
        }
        private static void CombineCode(Dictionary<EGenerateTime, string> codes, EGenerateTime gt, string code)
        {
            string result;
            codes.TryGetValue(gt, out result);
            if (result != null && !result.EndsWith("\r\n"))
                result += "\r\n";
            result += code;
            codes[gt] = result;
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
        private static Dictionary<EGenerateTime, string> GenerateCode(string instance, EditorVariable editor)
        {
            Dictionary<EGenerateTime, string> codes = new Dictionary<EGenerateTime,string>();
            var variable = editor.Variable;
            var value = editor.VariableValue;
            var type = variable.Type;

            if (value is UIElement)
            {
                UIElement e = (UIElement)value;
                codes[EGenerateTime.Initialize] = string.Format("{0}.{1} = {2};", instance, variable.VariableName, BuildInstance(e));
            }
            else if (value is Content)
            {
                StringBuilder builder = new StringBuilder();
                if (value == null || (value is FONT && ((FONT)value).IsDefault))
                {
                }
                else if (value == TEXTURE.Pixel)
                {
                    builder.Append("{0}.{1} = TEXTURE.Pixel;", instance, variable.VariableName);
                }
                else if (value is PATCH && ((PATCH)value).IsDefaultPatch)
                {
                    PATCH e = (PATCH)value;
                    COLOR border = e.ColorBorder;
                    byte bold = (byte)e.Anchor.X;
                    builder.Append("{5}.{6} = PATCH.GetNinePatch(PATCH.NullColor, new COLOR({0}, {1}, {2}, {3}), {4});",
                        border.R, border.G, border.B, border.A,
                        bold, instance, variable.VariableName);
                }
                else
                {
                    if (instance.StartsWith("  "))
                    {
                        //builder.Append("LoadAsync(Content.LoadAsync<{0}>(@{1}, {4} => {2}.{3} = {4}));", variable.Type.CodeName(), _RH.CodeValue(editor.VariableStringValue, true), instance, variable.VariableName, ASYNC_VAR);
                        builder.Append("LoadAsync(Content.LoadAsync<{0}>(@\"{1}\", {4} => {2}.{3} = {4}));", variable.Type.CodeName(), editor.VariableStringValue, instance, variable.VariableName, ASYNC_VAR);
                    }
                    else
                    {
                        //builder.AppendLine("{5} = LoadAsync(Content.LoadAsync<{0}>(@{1}, {4} => {2}.{3} = {4}));", variable.Type.CodeName(), _RH.CodeValue(editor.VariableStringValue, true), instance, variable.VariableName, ASYNC_VAR, ASYNC);
                        builder.AppendLine("{5} = LoadAsync(Content.LoadAsync<{0}>(@\"{1}\", {4} => {2}.{3} = {4}));", variable.Type.CodeName(), editor.VariableStringValue, instance, variable.VariableName, ASYNC_VAR, ASYNC);
                        builder.Append("if ({0} != null && !{0}.IsEnd) yield return {0};", ASYNC);
                    }
                }
                codes[EGenerateTime.Load] = builder.ToString();
            }
            else if (value is string)
            {
                string e = (string)value;
                if (IsTranslate(e))
                {
                    // {2} need translate
                    int row = Language.GetRowIndex(2, e);
                    if (row != -1)
                    {
                        string id = Language[1, row];
                        codes[EGenerateTime.Show] = string.Format("{0}.{1} = _LANGUAGE.GetString(\"{2}\");", instance, variable.VariableName, id);
                        return codes;
                    }
                }
                codes[EGenerateTime.Initialize] = string.Format("{0}.{1} = {2};", instance, variable.VariableName, _RH.CodeValue(value, true, SETTING));
            }
            //else if (editor is EditorClip)
            //{
            //    EditorClip e = (EditorClip)editor;
            //}
            else if (editor is EditorAllObject)
            {
                EditorAllObject e = (EditorAllObject)editor;
                if (e.SelectedIndex != 0)
                    codes[EGenerateTime.Initialize] = string.Format("{0}.{1} = new {2}();", instance, variable.VariableName, value.GetType().CodeName());
                if (e.ValueEditor != null)
                    GeneratorUICode.CombineCode(codes, GenerateCode(instance, e.ValueEditor));
                else
                    CombineCode(codes, EGenerateTime.Initialize, string.Format("{0}.{1} = {2};", instance, variable.VariableName, _RH.CodeValue(value, true, SETTING)));
            }
            else if (editor is EditorObject)
            {
                EditorObject e = (EditorObject)editor;
                if (!variable.Type.IsValueType || variable.MemberInfo.MemberType == MemberTypes.Field)
                {
                    instance = string.Format("{0}.{1}", instance, variable.VariableName);
                    if (variable.Instance == null || value == null)
                    {
                        codes[EGenerateTime.Initialize] = instance + "= null;";
                    }
                    else
                    {
                        foreach (EditorVariable item in e.Editors)
                            GeneratorUICode.CombineCode(codes, GenerateCode(instance, item));
                    }
                }
                else
                {
                    StringBuilder builder = new StringBuilder();
                    string org = instance;
                    instance = string.Format("{0}.{1}", instance, variable.VariableName);
                    instance = instance.Replace('.', '_');
                    instance = instance.Replace('[', '_');
                    instance = instance.Replace(']', '_');
                    builder.AppendLine("{0} {1} = new {0}();", variable.Type.CodeName(), instance);
                    foreach (EditorVariable item in e.Editors)
                    {
                        foreach (var generate in GenerateCode(instance, item))
                        {
                            if (generate.Key == EGenerateTime.Initialize)
                            {
                                builder.AppendLine(generate.Value);
                            }
                            else
                            {
                                string result;
                                codes.TryGetValue(generate.Key, out result);
                                result += generate.Value;
                                codes[generate.Key] = result;
                            }
                        }
                    }
                    builder.AppendLine("{0}.{1} = {2};", org, variable.VariableName, instance);

                    codes[EGenerateTime.Initialize] = builder.ToString();
                }
            }
            else
            {
                codes[EGenerateTime.Initialize] = string.Format("{0}.{1} = {2};", instance, variable.VariableName, _RH.CodeValue(value, true, SETTING));
            }
            return codes;
        }
        private static bool IsTranslate(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            if (!EditorUI.Instance.Project.HasTranslateTable)
                return false;

            bool result = true;
            double temp;
            result = !double.TryParse(value, out temp);

            if (value.StartsWith("#") || value.StartsWith("_"))
                result = false;

            return result;
        }
	}

    public class UIFilter : ISerializeFilter
    {
        private static ISerializeFilter _default = new SerializeValidatorDefault();

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
