using EntryEditor;
using EntryEngine;
using EntryEngine.Serialize;
using EntryEngine.UI;
using System.IO;
using System.Collections.Generic;

namespace EditorUI
{
    public partial class EditorUI
    {
        private MenuStrip ms;

        private void InitializeMenuStrip()
        {
			TEXTURE icon = new PATCH(TEXTURE.Pixel, RECT.Empty, new COLOR(180, 220, 220, 255), PATCH.NullColor);
			TEXTURE sub = new PIECE(EditorContent.Load<TEXTURE>("IconArrow.png"), new RECT(0, 0, 24, 24));

            ms = new MenuStrip();
            ms.SortZ = int.MaxValue;
            ms.Width = this.Width;
            //strip.Height = 28;
            ms.Anchor = EAnchor.Top | EAnchor.Left | EAnchor.Right;
			ms.Background = GetNinePatch(new COLOR(222, 222, 222, 255), new COLOR(180, 200, 222, 255));
			
            MenuStrip strip2 = new ContextMenuStrip();
            strip2.Background = ms.Background;


            MenuStripItem item = new MenuStripItem();
            item.Text = "文件";
            item.UIText.Padding.X = 10;
            item.MenuStripChild = strip2;
            ms.AddItem(item);

            MenuStripItem item2 = new MenuStripItem();
            item2.Text = "新建项目";
            strip2.AddItem(item2);

            item2 = new MenuStripItem();
            item2.Text = "更换翻译表";
            strip2.AddItem(item2);

            item2 = new MenuStripItem();
            item2.Text = "新建场景";
            strip2.AddItem(item2);

            item2 = new MenuStripItem();
            item2.Text = "打开";
            strip2.AddItem(item2);

            item2 = new MenuStripItem();
            item2.Text = "保存";
            strip2.AddItem(item2);

            item2 = new MenuStripItem();
            item2.Text = "退出";
            strip2.AddItem(item2);



            strip2 = new ContextMenuStrip();
            strip2.Background = ms.Background;

            item = new MenuStripItem();
            item.Text = "编辑";
            item.UIText.Padding.X = 10;
            item.MenuStripChild = strip2;
            ms.AddItem(item);

            item2 = new MenuStripItem();
            item2.Text = "撤销";
            strip2.AddItem(item2);

            item2 = new MenuStripItem();
            item2.Text = "重做";
            strip2.AddItem(item2);

            item2 = new MenuStripItem();
            item2.Text = "剪切";
            strip2.AddItem(item2);

            item2 = new MenuStripItem();
            item2.Text = "复制";
            strip2.AddItem(item2);

            item2 = new MenuStripItem();
            item2.Text = "粘贴";
            strip2.AddItem(item2);

            item2 = new MenuStripItem();
            item2.Text = "删除";
            strip2.AddItem(item2);


            strip2 = new ContextMenuStrip();
            strip2.Background = ms.Background;

            item = new MenuStripItem();
            item.Text = "设置";
            item.UIText.Padding.X = 10;
            item.MenuStripChild = strip2;
            ms.AddItem(item);

            item2 = new MenuStripItem();
            item2.Text = "指定构建工具";
            strip2.AddItem(item2);

            item2 = new MenuStripItem();
            item2.Text = "背景色";
            strip2.AddItem(item2);

            item2 = new MenuStripItem();
            item2.Text = "重置视口尺寸";
            strip2.AddItem(item2);

			item2 = new MenuStripItem();
			item2.Text = "重新生成预设代码";
			strip2.AddItem(item2);

            item2 = new MenuStripItem();
            item2.Text = "全部显示";
            strip2.AddItem(item2);

            item2 = new MenuStripItem();
            item2.Text = "删除不可见";
            strip2.AddItem(item2);

            item2 = new MenuStripItem();
            item2.Text = "取消翻译表";
            strip2.AddItem(item2);

            foreach (MenuStripItem i in ms.Items)
            {
                //i.TextShader = new TextShader();
                i.UIText.FontColor = COLOR.Black;
                //i.UIText.Scale = new EEVector2(0.74f);
                i.UIText.TextAlignment = EPivot.TopLeft;
                foreach (MenuStripItem item3 in i.MenuStripChild.Items)
                {
                    item3.UIText.TextAlignment = EPivot.TopLeft;
                    item3.UIText.FontColor = COLOR.Black;
                    //item3.TextShader = new TextShader();
                    //item3.UIText.Scale = i.Scale;
                    //item3.Icon = icon;
                    item3.IconHasSubItem = sub;
                }
            }

            ms.RefreshItem();
            ms.Height = ms.ContentSize.Y;

            ms[0].MenuStripChild[0].Clicked += NewProject;
            ms[0].MenuStripChild[1].Clicked += SelectTranslation;
            ms[0].MenuStripChild[2].Clicked += NewScene;
            ms[0].MenuStripChild[3].Clicked += OpenProject;
            ms[0].MenuStripChild[4].Clicked += SaveView;
            ms[0].MenuStripChild[5].Clicked += ExitApplication;

            ms[2].MenuStripChild[0].Clicked += SelectEntryBuilder;
            ms[2].MenuStripChild[1].Clicked += SelectBGColor;
            ms[2].MenuStripChild[2].Clicked += ResetViewportSize;
			ms[2].MenuStripChild[3].Clicked += ResavePreviewCode;
            ms[2].MenuStripChild[4].Clicked += AllVisible;
            ms[2].MenuStripChild[5].Clicked += DeleteInvisible;
            ms[2].MenuStripChild[6].Clicked += ClearTranslation;
            this.Add(ms);
        }

        private void NewProject(UIElement sender, Entry e)
        {
            //SaveView();

            string file = UtilityEditor.SaveFile(SUFFIX_PROJECT);
            if (file != null)
            {
                // dispose current project, build a new project
                CloseProject();

                string contentDirectory = UtilityEditor.OpenFolder("请选择项目资源目录", Path.GetDirectoryName(file));
                if (string.IsNullOrEmpty(contentDirectory))
                {
                    UtilityEditor.Message("必须选择项目资源目录");
                    return;
                }
                //contentDirectory = _IO.RelativePath(contentDirectory, file);
                contentDirectory += "\\";

                string dir = Path.GetDirectoryName(file);
                string name = Path.GetFileNameWithoutExtension(file);
                dir = Path.Combine(dir, name);
                if (Directory.Exists(dir))
                {
                    UtilityEditor.Message("Sys0002");
                    return;
                }
                dir = _IO.DirectoryWithEnding(dir);
                Directory.CreateDirectory(dir);
                file = Path.Combine(dir, Path.GetFileName(file));

                Project = new Project();
                Project.Name = Path.ChangeExtension(file, SUFFIX_PROJECT);
                Project.ContentDirectory = _IO.RelativePath(contentDirectory, dir);
                CreateDefaultProjectDirectory(dir);
                SaveProject();
                OnOpenProject();

                NewScene(sender, e);
            }
        }
        private void SelectTranslation(UIElement sender, Entry e)
        {
            if (Project == null)
                return;

            string file = UtilityEditor.SaveFile("csv");
            if (file == null)
                return;

            //Project.TranslateTable = Utility.RelativePath(file, DirectoryProject);
            if (!File.Exists(file))
                CSVWriter.WriteTable(StringTable.DefaultLanguageTable(), file);
            Project.TranslateTable = _IO.RelativePath(file, DirectoryProject);
        }
        private void OpenProject(UIElement sender, Entry e)
        {
            //SaveView();

            string file = UtilityEditor.OpenFile(SUFFIX_PROJECT);
            if (file != null)
            {
                // dispose current project, load and open the selected project
                CloseProject();

                Project = OpenProject(file);
                OnOpenProject();
            }
        }
        private void SaveView(UIElement sender, Entry e)
        {
            SaveView();
        }
        private void NewScene(UIElement sender, Entry e)
        {
            UIScene scene = (UIScene)BuildNewElement(typeof(UIScene));
            Project.Document = new Document(null);
            BuildView(scene);
        }
        private void ExitApplication(UIElement sender, Entry e)
        {
            Entry.Exit();
        }

        private void SelectEntryBuilder(UIElement sender, Entry e)
        {
            string path = UtilityEditor.OpenFile("exe");
            if (path == null)
                return;
            _LOG.Debug("Directory Editor: {0}", DirectoryEditor);
            C.EntryBuilder = _IO.RelativePath(path, DirectoryEditor);
        }
        private void SelectBGColor(UIElement sender, Entry e)
        {
			var color = UtilityEditor.SelectColor(C.ViewportBGColor);
			if (color.HasValue)
			{
				C.ViewportBGColor = color.Value;
				pvc.Background = GetNinePatch(COLOR.Gray, C.ViewportBGColor);
			}
        }
        private void ResetViewportSize(UIElement sender, Entry e)
        {
            if (view == null)
                return;
            // fixed: 停靠改变尺寸
            pv.Remove(EditingScene);
            ResetViewport();
            pv.Add(EditingScene);
        }
		private void ResavePreviewCode(UIElement sender, Entry e)
		{
            //if (string.IsNullOrEmpty(Project.TranslateTable) ||
            //        string.IsNullOrEmpty(C.EntryBuilder))
            //{
            //    UtilityEditor.Message("请先配置翻译文件和工具");
            //    return;
            //}

            if (string.IsNullOrEmpty(C.EntryBuilder))
            {
                UtilityEditor.Message("请先配置生成工具");
                return;
            }

            string dir = DIR_PREVIEW;
			if (Directory.Exists(dir))
			{
				string[] scenes = Directory.GetFiles(dir, "*." + SUFFIX_ELEMENT, SearchOption.AllDirectories);
				foreach (var item in scenes)
				{
					UIElement load = LoadUI(item);
                    if (!(load is UIScene)) continue;
					Widget widget = SetElement(load);
                    //byte[] content = SaveUI(widget);
                    //File.WriteAllBytes(widget.FilePath, content);

                    GeneratorUICode.Generate(widget);
					_LOG.Debug("重新生成{0}完成！", item);
				}
			}
		}
        private void AllVisible(UIElement sender, Entry e)
        {
            if (EditingScene == null)
                return;

            EditingScene.ForeachParentPriority(null, ui => 
                {
                    ui.Visible = true;
                    if (ui.IsAutoWidth && ui.IsAutoHeight)
                    {
                        var csize = ui.ContentSize;
                        if (csize.X == 0 || csize.Y == 0)
                        {
                            ui.Width = 10;
                            ui.Height = 10;
                            _LOG.Debug("UI:{0}取消自动尺寸");
                            return;
                        }
                        TextureBox tb = ui as TextureBox;
                        if (tb != null)
                        {
                            if (tb.Texture == null)
                            {
                                tb.Width = 10;
                                tb.Height = 10;
                            }
                        }
                        else
                        {
                            Button button = ui as Button;
                            if (button != null)
                            {
                                if (string.IsNullOrEmpty(button.Text) &&
                                    (button.SourceNormal == null ||
                                    (button.SourceClicked == null && button.Checked)))
                                {
                                    button.Width = 10;
                                    button.Height = 10;
                                }
                            }
                        }
                    }
                });
        }
        private void DeleteInvisible(UIElement sender, Entry e)
        {
            if (EditingScene == null)
                return;
            List<UIElement> invisibles = new List<UIElement>();
            EditingScene.ForeachParentPriority(null, ui =>
            {
                if (!ui.Visible || ui.Width == 0 || ui.Height == 0)
                    invisibles.Add(ui);
            });
            foreach (var item in invisibles)
            {
                if (item.Parent != null)
                {
                    item.Parent.Remove(item);
                    _LOG.Debug("删除不可见控件:{0}", item.Name);
                }
            }
        }
        private void ClearTranslation(UIElement sender, Entry e)
        {
            Project.TranslateTable = null;
        }
    }
}
