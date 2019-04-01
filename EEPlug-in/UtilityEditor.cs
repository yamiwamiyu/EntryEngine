using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using EntryEngine;
using EntryEngine.Serialize;
using EntryEngine.UI;
using Graphics = System.Drawing.Graphics;

namespace EntryEditor
{
	public static class UtilityEditor
	{
        public static SceneEditorEntry LoadExpandEditor(SceneEditorEntry entry)
		{
			Assembly[] assemblies = UtilityEditor.LoadPlugins();
			List<Type> editorTypes = new List<Type>();
			for (int i = 0; i < assemblies.Length; i++)
			{
				editorTypes.AddRange(assemblies[i].GetTypes(typeof(SceneEditorEntry)));
			}
			if (entry == null && editorTypes.Count == 0)
			{
				throw new ArgumentNullException("no editor");
			}
			if (entry == null)
			{
				entry = (SceneEditorEntry)Activator.CreateInstance(editorTypes[0]);
			}
			Environment.CurrentDirectory = Path.GetFullPath(string.Format("{0}\\{1}\\", PLUG_ING, entry.GetType().Name));
			entry.DirectoryEditor = _IO.DirectoryWithEnding(Environment.CurrentDirectory);
            //Entry.Instance.ShowMainScene(entry, true);
            Entry.Instance.ShowMainScene(entry);
            return entry;
		}

		public const string PLUG_ING = "Plug-ins";
		public static string StartDirectory;
		private static Assembly[] plugins;
        //private static Assembly[] myDll;
		public static Assembly[] LoadPlugins()
		{
			if (plugins == null)
			{
				string[] files = Directory.GetFiles(PLUG_ING, "*.dll", SearchOption.AllDirectories);
				int count = files.Length;
				plugins = new Assembly[count];
				for (int i = 0; i < count; i++)
				{
					try
					{
						files[i] = Path.GetFullPath(files[i]);
						plugins[i] = Assembly.LoadFile(files[i]);
					}
					catch (Exception ex)
					{
						_LOG.Error("load plug-in {0} error! msg={1}", files[i], ex.Message);
					}
				}
			}
			return plugins;
		}
		public static IEnumerable<Assembly> OriginalAssemblies()
		{
			if (plugins == null)
			{
				return LoadedAssemblies();
			}
			else
			{
				return LoadedAssemblies().Where(a => !plugins.Contains(a)).ToArray();
			}
		}
        public static IEnumerable<Assembly> LoadedAssemblies()
		{
            //if (myDll != null)
            //    return myDll;
            const string DLL = ".dll";
            const string XNA = "Microsoft.Xna";
            // 从程序启动的文件夹中加载的所有dll
            //myDll = 
            return
                AppDomain.CurrentDomain.GetAssemblies().Where(a =>
                // load from byte[] will not have location 
                (string.IsNullOrEmpty(a.Location) ||
                (a.Location.StartsWith(StartDirectory) &&
                a.Location.EndsWith(DLL))) &&
                !a.GetAssemblyName().StartsWith(XNA));
            //return myDll;
		}

		public static string GetAssemblyName(this Assembly assembly)
		{
			return assembly.FullName.Substring(0, assembly.FullName.IndexOf(','));
		}
		public static string BuildDllType(this Type type)
		{
			return type.Assembly.GetAssemblyName() + "." + type.FullName;
		}
		public static Type GetDllType(string dllAndType)
		{
			// path\Dll.Namespace.Class
			int index = dllAndType.LastIndexOf('\\') + 1;
			string dll = dllAndType.Substring(0, index);
			// Dll.Namespace.Class
			dllAndType = dllAndType.Substring(index);
			string[] assemblyInfo = dllAndType.Split('.');
			dll = Path.GetFullPath(dll + assemblyInfo[0] + ".dll");
			Assembly assembly = LoadedAssemblies().FirstOrDefault(a => a.GetAssemblyName() == assemblyInfo[0]);
			if (assembly == null)
			{
				assembly = Assembly.LoadFile(dll);
				if (assembly == null)
				{
					Console.WriteLine("no library: {0}", dll);
					return null;
				}
			}
			string type = dllAndType.Substring(dllAndType.IndexOf('.') + 1);
			Type result = assembly.GetType(type);
			if (result == null)
				Console.WriteLine("no type: {0}", type);
			return result;
		}
		public static T GetDllTypeInstance<T>(string dllAndType) where T : class
		{
			Type type = GetDllType(dllAndType);
			if (type == null)
				return null;
			T instance = Activator.CreateInstance(type) as T;
			if (instance == null)
				Console.WriteLine("can not create instance of {0}", type.FullName);
			return instance;
		}
		public static string[] GetFiles(string dirOrFile, string pattern = null)
		{
			if (pattern == null || Path.GetFileName(dirOrFile).Contains('.'))
			{
				return new string[] { dirOrFile };
			}
			else
			{
				return Directory.GetFiles(dirOrFile, pattern, SearchOption.AllDirectories);
			}
		}
        public static string GetProjectRelativePath(string path, string directory = "")
        {
            string root = Path.Combine(SceneEditorEntry.Instance.DirectoryProject, directory);
            path = Path.GetFullPath(path);
            path = _IO.RelativePathForward(path, root);
            if (path == null)
            {
                UtilityEditor.Message("", string.Format("必须使用项目中{0}目录中的文件", root));
            }
            return path;
        }

		public static Form Window;
		public static Action<string[]> DragFiles;
        public static Action OnExit;
		public static SaveFileDialog Saver = new SaveFileDialog()
		{
			Title = string.Empty,
			RestoreDirectory = true,
		};
		public static OpenFileDialog Loader = new OpenFileDialog()
		{
			Title = string.Empty,
			Multiselect = false,
			RestoreDirectory = true,
			AddExtension = false,
		};
        public static FolderBrowserDialog Folder = new FolderBrowserDialog();
		public static ColorDialog ColorPicker = new ColorDialog()
		{
			AllowFullOpen = true,
			AnyColor = true,
			FullOpen = true,
		};
		private static Bitmap pixel = new Bitmap(1, 1);
		private static Graphics graphics = Graphics.FromImage(pixel);
		public static bool Confirm(string title, string msg)
		{
			return MessageBox.Show(msg, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
		}
		public static bool Confirm(string msg)
		{
            return Confirm(string.Empty, msg);
		}
		public static void Message(string title, string msg)
		{
			MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
		public static void Message(string msg)
		{
            Message(string.Empty, msg);
		}
		private static bool FileDialog(FileDialog dialog)
		{
			if (string.IsNullOrEmpty(dialog.InitialDirectory))
			{
				dialog.InitialDirectory = "";
			}
			else
			{
				try
				{
					dialog.InitialDirectory = Path.GetFullPath(dialog.InitialDirectory);
				}
				catch (Exception)
				{
					dialog.InitialDirectory = "";
				}
			}
			//dialog.FileName = null;
			return dialog.ShowDialog() == DialogResult.OK;
		}
		public static bool OpenFile(ref string name, params string[] suffix)
		{
            string directory = string.IsNullOrEmpty(name) ? null : Path.GetDirectoryName(name);
            if (!string.IsNullOrEmpty(directory))
            {
                Loader.InitialDirectory = Path.GetFullPath(directory);
                name = Path.GetFileName(name);
            }
            if (!Directory.Exists(Loader.InitialDirectory))
                Loader.InitialDirectory = Environment.CurrentDirectory;
            // 包含错误字符的文字会导致看不见文件
            if (name != null && !(name.Contains('*') || name.Contains('|') || name.Contains('/') || name.Contains('\\') || name.Contains('<') || name.Contains('>') || name.Contains(':') || name.Contains('?') || name.Contains('\"')))
			    Loader.FileName = name;
			Loader.Filter = GetFilters(suffix);
			if (FileDialog(Loader))
			{
				name = Loader.FileName;
			}
			else
			{
				name = null;
			}
			return name != null;
		}
		public static string OpenFile(params string[] suffix)
		{
			string name = null;
			OpenFile(ref name, suffix);
			return name;
		}
		public static string[] OpenFiles(params string[] suffix)
		{
			Loader.Filter = GetFilters(suffix);
			Loader.Multiselect = true;
			Loader.FileName = null;

			if (FileDialog(Loader))
			{
				return Loader.FileNames;
			}
			else
			{
				return new string[0];
			}
		}
        public static string OpenFolder(string title, string selected)
        {
            string _title = Folder.Description;
            Folder.Description = title;
            if (string.IsNullOrEmpty(selected))
                selected = Environment.CurrentDirectory;
            Folder.SelectedPath = selected;

            string result;
            if (Folder.ShowDialog() == DialogResult.OK)
                result = Folder.SelectedPath;
            else
                result = null;

            Folder.Description = _title;
            return result;
        }
		public static bool SaveFile(ref string name, string suffix)
		{
            string directory = Path.GetDirectoryName(name);
            if (!string.IsNullOrEmpty(directory))
            {
                Saver.InitialDirectory = Path.GetFullPath(directory);
                name = Path.GetFileName(name);
            }
            if (!Directory.Exists(Saver.InitialDirectory))
                Saver.InitialDirectory = Environment.CurrentDirectory;
			Saver.FileName = name;
			Saver.Filter = GetFilter(suffix);
			if (FileDialog(Saver))
			{
				name = Saver.FileName;
			}
			else
			{
				name = null;
			}
			return name != null;
		}
		public static string SaveFile(string suffix)
		{
			string name = null;
			SaveFile(ref name, suffix);
			return name;
		}
		public static string GetFilters(string[] suffix)
		{
			string filter = GetFilter(suffix);      // 全部
			if (suffix.Length > 1)
			{
				filter += "|\r\n" + _GetFilters(suffix);       // 各类型
			}
			return filter;
		}
		private static string _GetFilters(params string[] suffix)
		{
			if (suffix.Length == 0)
				return "(*.*)| *.*";

			StringBuilder filter = new StringBuilder();
			for (int i = 0; i < suffix.Length; i++)
			{
				filter.AppendLine(string.Format("(*.{0})|*.{0}|", suffix[i]));
			}
			filter.Remove(filter.Length - 4, 4);        // 移除最后一个 "空格" "|" 和 "\r\n"
			return filter.ToString();
		}
		public static string GetFilter(params string[] suffix)
		{
            if (suffix.Length == 0)
                return "(*.*)|*.*";
            else
                for (int i = 0; i < suffix.Length; i++)
                    if (string.IsNullOrEmpty(suffix[i]))
                        suffix[i] = "*";

			StringBuilder filter = new StringBuilder();
			filter.Append("(");
			for (int i = 0; i < suffix.Length; i++)
			{
				filter.Append(string.Format("*.{0};", suffix[i]));
			}
			filter.Remove(filter.Length - 1, 1);    // 移除最后一个";"
			filter.Append(")|");                 // 添加分割线
			for (int i = 0; i < suffix.Length; i++)
			{
				filter.Append(string.Format("*.{0};", suffix[i]));
			}
			filter.Remove(filter.Length - 1, 1);    // 移除最后一个";"
			return filter.ToString();
		}
		public static Color GUIColor(this COLOR color)
		{
			return Color.FromArgb(color.A, color.R, color.G, color.B);
		}
		public static COLOR GUIColor(this Color color)
		{
			return new COLOR(color.R, color.G, color.B, color.A);
		}
		public static COLOR? SelectColor(COLOR? defaultColor)
		{
			if (defaultColor.HasValue)
			{
				COLOR color = defaultColor.Value;
				ColorPicker.Color = Color.FromArgb(color.A, color.R, color.G, color.B);
			}
			if (ColorPicker.ShowDialog() == DialogResult.OK)
			{
				return new COLOR(ColorPicker.Color.R, ColorPicker.Color.G, ColorPicker.Color.B, ColorPicker.Color.A);
			}
			else
			{
                return null;
			}
		}
		public static COLOR SelectColorFromScreen(int x, int y)
		{
			graphics.CopyFromScreen(x, y, 0, 0, pixel.Size, CopyPixelOperation.SourceCopy);
			graphics.Save();
			return pixel.GetPixel(0, 0).GUIColor();
		}
	}
}
