using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EntryEditor;
using EntryEngine;
using EntryEngine.Serialize;
using EntryEngine.UI;
using System.Text;

namespace EditorUI
{
    [Obsolete("to use of TabPage")]
    public class Collapsable : Button
    {
        public event DUpdate<Collapsable> BeforeExpand;
        public event DUpdate<Collapsable> AfterExpand;
        public event DUpdate<Collapsable> BeforeCollapse;
        public event DUpdate<Collapsable> AfterCollapse;

        public Panel Panel
        {
            get;
            set;
        }
        public override bool Checked
        {
            get { return Panel.Visible; }
            set
            {
                if (value != Checked)
                {
                    if (value)
                    {
                        Expand();
                    }
                    else
                    {
                        Collapse();
                    }
                    base.Checked = value;
                }
            }
        }

        public Collapsable()
        {
            Panel = new Panel();
            //Panel.IsClip = false;
            Panel.ScrollOrientation = EScrollOrientation.None;
            Panel.DragMode = EDragMode.Drag;
            Panel.Width = 200;
            Panel.Height = 100;
            Add(Panel);

            Width = 100;
            Height = 24;
            Panel.Y = Height;

            UIText.TextAlignment = EPivot.MiddleLeft;
        }

        public void Expand()
        {
            if (Checked)
                return;
            if (BeforeExpand != null)
                BeforeExpand(this, Entry.Instance);
            Panel.Visible = true;
            if (AfterExpand != null)
                AfterExpand(this, Entry.Instance);
        }
        public void Collapse()
        {
            if (!Checked)
                return;
            if (BeforeCollapse != null)
                BeforeCollapse(this, Entry.Instance);
            Panel.Visible = false;
            if (AfterCollapse != null)
                AfterCollapse(this, Entry.Instance);
        }
        protected override bool OnCheckedChanging()
        {
            return !Checked;
        }
        public override IEnumerator<UIElement> GetEnumerator()
        {
            foreach (var item in Childs.ToArray())
                if (item != Panel)
                    yield return item;
        }
    }
    public partial class EditorUI
    {
        private const int PREVIEW_WIDTH = 140;
        private const int PREVIEW_HEIGHT = 44;
        private const string FILE_ELEMENTS = "Elements.xml";
        private static Dictionary<Type, ElementLib> libs;
        private static ElementLib LibUIElement;
        // 所有预设控件的名字
        private static string[] preElements;
        public static ElementLib GetElementLib(Type type)
        {
            ElementLib lib;
            libs.TryGetValue(type, out lib);
            return lib;
        }
        public static bool HasPreElement(string name)
        {
            return preElements.Any(p => p == name);
        }

        private Panel pec;
        private Panel pe;
        private Collapsable collapse;
        private UIElement selectedPreviewPanel;
        private UIElement selectedElement;
        private TEXTURE selectedPreview;

        public UIElement SelectedElement
        {
            get { return selectedElement; }
            set
            {
                if (value == selectedElement)
                    return;
                selectedElement = value;
                if (selectedPreview != null)
                {
                    selectedPreview.Dispose();
                    selectedPreview = null;
                }
                if (selectedElement != null && value.Tag is Widget)
                {
                    Widget widget = (Widget)value.Tag;
                    string preview = widget.FilePath;
                    preview = Path.ChangeExtension(preview, SUFFIX_TEXTURE);
                    if (File.Exists(preview))
                        selectedPreview = contentPreview.Load<TEXTURE>("PREVIEW", preview);
                }
                selectedPreviewPanel = null;
            }
        }

        private void InitializeElement()
        {
            pec = new Panel();
			pec.SortZ = 10;
            pec.Name = "PanelElementListContainer";
            pec.Anchor = EAnchor.Top | EAnchor.Bottom | EAnchor.Left;
            pec.X = 0;
            pec.Y = ms.Height;
            pec.Width = 200;
            pec.Height = Entry.GRAPHICS.ScreenSize.Y - ms.Height;
            pec.Pivot = EPivot.TopLeft;
            pec.Background = GetNinePatchBodyColor();
            //panelElementListContainer.BackgroundInner = GetNinePatchBodyColor();
            //panelElementListContainer.BackgroundCrossScrollBar = GetNinePatchTitleColor();
            //pec.ScrollOrientation = EScrollOrientation.VerticalAuto | EScrollOrientation.HorizontalAuto;
            //var scroll = GetScrollBar(true);
            //scroll.Pivot = EPivot.TopRight;
            //scroll.Anchor = EAnchor.Top | EAnchor.Right | EAnchor.Bottom;
            //scroll.X = pec.Width;
            //scroll.Height = pec.Height;
            //pec.ScrollBarVertical = scroll;
            //pec.Add(scroll);
            //pec.ScrollBarHorizontal = GetScrollBar(false);
            //pec.Add(pec.ScrollBarHorizontal);
            pec.DragMode = EDragMode.Drag;
            pec.Hover += new DUpdate<UIElement>(pec_Hover);
            this.Add(pec);

            pe = new Panel();
            pe.Width = pec.Width;
            pec.Add(pe);

            collapse = new Collapsable();
            collapse.Checked = false;
            //collapse.Panel.Width = 0;

            collapse.Panel.Name = "PanelElementList";
            //collapse.Panel.Anchor = EAnchor.Top | EAnchor.Bottom | EAnchor.Left;
            collapse.Panel.Pivot = EPivot.TopLeft;

            XmlReader reader = new XmlReader(File.ReadAllText(Path.GetFullPath(FILE_ELEMENTS)));
            ElementLib[] widgets = reader.ReadObject<ElementLib[]>();

            libs = widgets.ToDictionary(l => UtilityEditor.GetDllType(l.Type));
            for (int i = 0; i < widgets.Length; i++)
            {
                if (string.IsNullOrEmpty(widgets[i].Name))
                    continue;
                Panel panel = BuildPreElement(UtilityEditor.GetDllType(widgets[i].Type));
                panel.Y = panel.Height * collapse.Panel.ChildCount;
                collapse.Panel.Add(panel);
            }
            LibUIElement = libs.FirstOrDefault(l => l.Key.FullName == typeof(UIElement).FullName).Value;
            //LibUIElement = libs[typeof(UIElement)];
            
            collapse.Width = pec.Width;
            collapse.Height = 24;
            collapse.UIText.FontColor = COLOR.Black;
            //box.UIText.Scale = new EEVector2(0.74f);
            collapse.SourceNormal = GetNinePatchTitleColor();
            collapse.Text = "系统控件";
            collapse.UIText.TextAlignment = EPivot.MiddleLeft;
            collapse.UIText.Padding.X = 20;
            collapse.CheckedChanged += LayoutElementList;

            collapse.Panel.IsClip = false;
            collapse.Panel.Y = collapse.Height;
            collapse.Panel.Height = 0;

            //panelElementListContainer.Width += panelElementListContainer.ScrollBarVertical.Width;
            pe.Add(collapse);
            pe.Add(collapse.Panel);

            pec.Keyboard += KeyboardEventForElementList;
            pec.DrawBeforeEnd += DrawSelectedElement;
        }

        private Panel BuildPreElement(Type type, string file = null)
        {
            Panel panel = new Panel();
            panel.Width = pec.Width;
            panel.Height = PREVIEW_HEIGHT;
            panel.DragMode = EDragMode.None;
            panel.Background = GetNinePatchBodyColor();

            if (file != null && File.Exists(file))
            {
                panel.Tag = file;

                string previewFile = string.Format("{0}\\{1}.{2}.{3}",
                    DIR_PREVIEW,
                    Path.GetFileNameWithoutExtension(file),
                    type.Name,
                    SUFFIX_TEXTURE);
                if (File.Exists(previewFile))
                {
                    TextureBox preview = new TextureBox();
                    preview.X = 60;
                    preview.Width = PREVIEW_WIDTH;
                    preview.Height = PREVIEW_HEIGHT;
                    preview.DisplayMode = EViewport.Adapt;
                    preview.Texture = contentPreview.Load<TEXTURE>(previewFile);
                    panel.Add(preview);
                }
            }
            else
            {
                panel.Tag = type;
            }
            if (type == typeof(UIScene))
                panel.Hover += new DUpdate<UIElement>(panel_Hover);
            else
                panel.Clicked += SelectPreviewElement;

            ElementLib widget = GetElementLib(type);
            if (!string.IsNullOrEmpty(widget.Icon))
            {
                TextureBox icon = new TextureBox();
                icon.X = 30;
                icon.Y = 22;
                icon.Pivot = EPivot.MiddleCenter;
                icon.DisplayMode = EViewport.Adapt;
                icon.Texture = EditorContent.Load<TEXTURE>(widget.Icon);
                panel.Add(icon);
            }

            Label label = new Label();
            label.X = 60;
            label.Y = 5;
            label.UIText.FontColor = COLOR.Black;
            label.UIText.FontSize -= 4;
            panel.Add(label);
            label.Text = file == null ? widget.Name : Path.GetFileNameWithoutExtension(file);

            return panel;
        }

        private void LoadPreElements()
        {
            pe.Clear();
            pe.Add(collapse);
            pe.Add(collapse.Panel);

            List<string> temps = new List<string>();
            string[] directories = Directory.GetDirectories(DIR_PREVIEW, "*.*", SearchOption.TopDirectoryOnly);
            foreach (string directory in directories)
            {
                string[] files = Directory.GetFiles(directory, "*." + SUFFIX_ELEMENT, SearchOption.AllDirectories);
                if (files.Length == 0)
                    continue;

                Collapsable box = new Collapsable();
                box.Panel.Width = 0;
                box.Panel.Height = 0;
                box.Checked = false;
                box.Width = pec.Width;
                box.Height = 24;
                box.UIText.FontColor = COLOR.Black;
                box.SourceNormal = GetNinePatchTitleColor();
                box.Text = directory;
                box.UIText.TextAlignment = EPivot.MiddleLeft;
                box.UIText.Padding.X = 20;
                box.CheckedChanged += LayoutElementList;

                //box.Panel.Anchor = EAnchor.Top | EAnchor.Bottom | EAnchor.Left;
                box.Panel.Pivot = EPivot.TopLeft;
                //box.Panel.IsClip = false;
                //box.Panel.DragMode = EDragMode.Drag;

                pe.Add(box);
                pe.Add(box.Panel);

                for (int i = 0; i < files.Length; i++)
                {
                    Panel panel = BuildPreElement(LoadElementType(files[i]), files[i]);
                    panel.Y = panel.Height * box.Panel.ChildCount;
                    box.Panel.Add(panel);

                    temps.Add(Path.GetFileNameWithoutExtension(files[i]));
                }
            }
            LayoutElementList(null, null);

            preElements = temps.ToArray();
        }
        private UIElement BuildNewElement(Type type)
        {
            UIElement target;
            string preFile = string.Format("{0}\\{1}.{2}", DIR_PREVIEW, type.BuildDllType(), SUFFIX_ELEMENT);
            if (File.Exists(preFile))
            {
                target = LoadUI(preFile);
            }
            else
            {
                target = (UIElement)type.GetConstructor(new Type[0]).Invoke(new object[0]);
                // set default value
                ElementLib lib;
                if (libs.TryGetValue(type, out lib))
                {
                    if (!lib.Variables.IsEmpty())
                    {
                        foreach (var item in lib.Variables)
                        {
                            if (!string.IsNullOrEmpty(item.Value))
                            {
                                VariableObject variable = new VariableObject(target, item.VariableName);
                                XmlReader reader = new XmlReader(item.Value);
                                object value = reader.ReadObject(variable.Type);
                                variable.SetValue(value);
                            }
                        }
                    }
                }

                // build element has child like 'combobox'
                for (int i = 0; i < target.ChildCount; i++)
                    BuildNewElement(target[i].GetType());
            }

            BuildElementName(target);
            return target;
        }
        private void BuildElementName(UIElement element)
        {
            if (string.IsNullOrEmpty(element.Name))
                element.Name = new string(element.GetType().Name.Where(c => c >= 65 && c <= 91).ToArray());
            element.Name += DateTime.Now.ToString("yyMMddHHmmss");
        }
        private string FindPreviewFile(string preview)
        {
            string target = Path.GetFileName(preview);
			string previewDir = _IO.RelativePath(preview, _IO.DirectoryWithEnding(DIR_PREVIEW));
            if (previewDir != null)
            {
                // preview\uiname.uitype.png -> preview\uitype\uiname.ui
                if (previewDir == target)
                {
                    string[] data = target.Split('.');
                    if (data.Length != 3)
                        return null;
                    target = string.Format("{0}\\{1}.{2}", data[1], data[0], SUFFIX_ELEMENT);
                }
                else
                    target = Path.ChangeExtension(previewDir, SUFFIX_ELEMENT);
            }
            return string.Format("{0}\\{1}", DIR_PREVIEW, target);
        }
        private void SaveElement(UIElement element)
        {
            (element.Tag as Widget).Save();
        }

        private void pec_Hover(UIElement sender, Entry e)
        {
            if (e.INPUT.Mouse.ScrollWheelValue != 0)
            {
                pec.OffsetY += 30 * e.INPUT.Mouse.ScrollWheelValue;
            }
        }
        private void panel_Hover(UIElement sender, Entry e)
        {
            bool trigger = false;
            if (!Editing)
            {
                if (e.INPUT.Pointer.ComboClick.IsDoubleClick)
                {
                    trigger = true;
                }
            }
            // 中键
            if (e.INPUT.Pointer.IsClick(2))
            {
                trigger = true;
            }

            if (trigger)
            {
                string file = sender.Tag.ToString();
                BuildView(LoadUI(file) as UIScene);
                SelectedElement = null;
                Handle();
            }
            else
            {
                if (e.INPUT.Pointer.IsTap(0))
                {
                    SelectPreviewElement(sender, e);
                }
            }
        }
        private void LayoutElementList(Button sender, Entry e)
        {
            float y = 0;
            foreach (var child in pe)
            {
                if (!child.Visible)
                    continue;
                child.Y = y;
                y += child.Height;
                //if (child.Checked)
                //    y += child.ChildClip.Height;
            }
            SelectedElement = null;
        }
        private void SelectPreviewElement(UIElement sender, Entry e)
        {
            if (view == null)
                return;

            UIElement selected;
            if (sender.Tag is string)
            {
                string file = sender.Tag.ToString();
                selected = LoadUI(file);
            }
            else
                selected = BuildNewElement((Type)sender.Tag);

            SelectedElement = selected;
            selectedPreviewPanel = sender;
        }
        private void KeyboardEventForElementList(UIElement sender, Entry e)
        {
            if (SelectedElement != null && SelectedElement.Tag is Widget)
            {
                // 删除预设
                if (e.INPUT.Keyboard.IsClick(PCKeys.Delete))
                {
                    var widget = SelectedElement.Tag as Widget;
                    string file = widget.FilePath;
                    if (UtilityEditor.Confirm("确认删除", string.Format("是否确定要删除{0}？", file)))
                    {
                        string filename = Path.GetFileNameWithoutExtension(file);
                        Type type = SelectedElement.GetType();

                        string preview1 = Path.ChangeExtension(file, SUFFIX_TEXTURE);
                        string preview2 = string.Format("{0}\\{1}.{2}.{3}", DIR_PREVIEW,
                            filename, type.Name, SUFFIX_TEXTURE);

                        if (type == typeof(UIScene))
                        {
                            string cs = string.Format("{0}\\{1}{2}", DIR_UI, filename, SUFFIX_CS);
                            string csDesign = string.Format("{0}\\{1}{2}", DIR_UI, filename, SUFFIX_DESIGN_CS);
                            string csv = Path.ChangeExtension(file, "csv");

                            if (File.Exists(csv))
                            {
                                // cancel ui translate
                                CSVReader reader = new CSVReader(File.ReadAllText(csv, Encoding.UTF8));
                                var table = reader.ReadTable();
                                table.Clear(2);

                                CSVWriter writer = new CSVWriter();
                                writer.WriteTable(table);
                                File.WriteAllText(csv, writer.Result, Encoding.UTF8);

                                TranslateUI(csv);
                            }

                            File.Delete(cs);
                            File.Delete(csDesign);
                            File.Delete(csv);
                        }

                        contentPreview.Dispose(preview1);
                        contentPreview.Dispose(preview2);

                        File.Delete(file);
                        File.Delete(preview1);
                        File.Delete(preview2);

                        //UIElement.FindAllParentPriority(pec, null, u => u.Remove(selectedPreviewPanel));
                        SelectedElement = null;
                        LoadPreElements();
                    }
                }
            }

            // 刷新列表
            if (e.INPUT.Keyboard.IsClick(PCKeys.F5))
                LoadPreElements();
        }
        private void DrawSelectedElement(UIElement sender, GRAPHICS g, Entry e)
        {
            if (selectedPreviewPanel != null)
                g.Draw(TEXTURE.Pixel, selectedPreviewPanel.ViewClip, GRAPHICS.NullSource, C.PreSelectedColor);
        }
    }
    public class ElementLib
    {
        public class ElementVariable
        {
            public string VariableName;
            public string NickName;
            public string EditorType;
            public string Value;
            public EEKeyValuePair<string, string>[] Properties;
        }

        public string Icon;
        public string Name;
        public string Type;
        public ElementVariable[] Variables;
    }
}
