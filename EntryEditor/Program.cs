using System;
using System.Collections.Generic;
using System.Windows.Forms;
using EntryEngine;
using EntryEngine.Xna;
using EntryEngine.Serialize;
using System.IO;

namespace EntryEditor
{
	class Program
	{
        static string Editor;
		static IEnumerable<ICoroutine> Initialize()
		{
            //_LOG._Logger = new EntryEngine.Cmdline.Logger();
			// 编辑器翻译
            //FONT.Default = Entry.Instance.NewFONT("Consolas", 16.8f);
            FONT.Default = Entry.Instance.NewFONT("黑体", 16f);

			// 编辑器环境配置
            Console.WriteLine("加载配置文件");
			Config<ConfigEditor>.Load(ConfigEditor.CONFIG_FILE);
            Console.WriteLine("加载编辑器插件");
			UtilityEditor.LoadPlugins();

			XnaGate.Gate.BGColor = Config<ConfigEditor>.Setting.BGColor.GetColor();
			XnaGate.Gate.Window.AllowUserResizing = true;
			UtilityEditor.Window = (Form)Form.FromHandle(XnaGate.Gate.Window.Handle);
            UtilityEditor.Window.FormClosed += (sender, e) =>
                {
                    if (UtilityEditor.OnExit != null)
                    {
                        UtilityEditor.OnExit();
                    }
                };
			UtilityEditor.Window.Resize += (sender, e) =>
				{
					Entry.Instance.GRAPHICS.ScreenSize = new VECTOR2(
						XnaGate.Gate.Window.ClientBounds.Width,
						XnaGate.Gate.Window.ClientBounds.Height);
                    Entry.Instance.GRAPHICS.GraphicsSize = Entry.Instance.GRAPHICS.ScreenSize;
                    if (SceneEditorEntry.Instance != null)
					{
                        SceneEditorEntry.Instance.Size = Entry.Instance.GRAPHICS.ScreenSize;
					}
                    Config<ConfigEditor>.Setting.WindowClip.Size = Entry.Instance.GRAPHICS.GraphicsSize;
					Config<ConfigEditor>.Setting.WindowMax = UtilityEditor.Window.WindowState == FormWindowState.Maximized;
				};
			UtilityEditor.Window.LocationChanged += (sender, e) =>
				{
					Config<ConfigEditor>.Setting.WindowClip.Location = new VECTOR2(
						XnaGate.Gate.Window.ClientBounds.X,
						XnaGate.Gate.Window.ClientBounds.Y);
				};

			UtilityEditor.Window.AllowDrop = true;
			UtilityEditor.Window.DragEnter += (sender, e) =>
				{
					if (e.Data.GetDataPresent(DataFormats.FileDrop))
						e.Effect = DragDropEffects.Link;
					else
						e.Effect = DragDropEffects.None;
				};
			UtilityEditor.Window.DragDrop += (sender, e) =>
				{
					if (UtilityEditor.DragFiles != null)
					{
						string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
						UtilityEditor.DragFiles(files);
					}
				};

            Entry.Instance.GRAPHICS.ViewportMode = EViewport.None;
            if (Config<ConfigEditor>.Setting.WindowMax)
                UtilityEditor.Window.WindowState = FormWindowState.Maximized;
            else
                Entry.Instance.GRAPHICS.ScreenSize = Config<ConfigEditor>.Setting.WindowClip.Size;
            Entry.Instance.GRAPHICS.GraphicsSize = Entry.Instance.GRAPHICS.ScreenSize;

            Console.WriteLine("正在打开编辑器");
			// 扩展编辑器
			SceneEditorEntry entry = null;
            if (!string.IsNullOrEmpty(Editor))
                entry = UtilityEditor.GetDllTypeInstance<SceneEditorEntry>(Editor);
			else if (Config<ConfigEditor>.Setting.LastOpenProject != null)
				entry = UtilityEditor.GetDllTypeInstance<SceneEditorEntry>(Config<ConfigEditor>.Setting.LastOpenProject.ProjectEditorName);

            entry = UtilityEditor.LoadExpandEditor(entry);
            if (entry != null)
                entry.Size = Entry.Instance.GRAPHICS.ScreenSize;

            if (!string.IsNullOrEmpty(Config<ConfigEditor>.Setting.Font))
                FONT.Default = Entry.Instance.NewFONT(Config<ConfigEditor>.Setting.Font, 16f);

            yield break;
		}
		[STAThread]
		static void Main(string[] args)
		{
            //args = new string[] { "EditorParticle" };
            //args = new string[] { "EditorPicture" };
            if (args.Length > 0)
                Editor = args[0];

			UtilityEditor.StartDirectory = Environment.CurrentDirectory;

            try
            {
                using (XnaGate gate = new XnaGate())
                {
					//gate.OnCreateEntry += () => new EntryCPU();
                    gate.OnInitialize += entry =>
                        {
                            entry.OnNewContentManager += (content) =>
                            {
                                content.AddPipeline(new EntryEngine.DragonBone.DBCore.PipelineDragonBones());
                            };
                        };
                    gate.OnInitialized += entry =>
                        {
                            entry.SetCoroutine(Program.Initialize());
                        };
                    gate.Run();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ReadKey();
            }

			// save editor configuration
			Environment.CurrentDirectory = UtilityEditor.StartDirectory;
			Config<ConfigEditor>.Save(ConfigEditor.CONFIG_FILE);
		}
	}
}
