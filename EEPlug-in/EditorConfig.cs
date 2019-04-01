using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EntryEngine;
using EntryEngine.UI;
using EntryEngine.Xna;

namespace EntryEditor
{
	public abstract class SceneEditorEntry : UIScene
	{
		protected static ContentManager EditorContent;
		private static SceneEditorEntry instance;

		public static SceneEditorEntry Instance
		{
			get { return instance; }
		}

		public string DirectoryEditor
		{
			get;
			internal set;
		}
		public abstract string DirectoryProject
		{
			get;
			protected set;
		}

		public SceneEditorEntry()
		{
			instance = this;
		}

        //public abstract void LoadProject(string project);
        public void ChangeEditor(SceneEditorEntry newEditor)
        {
        }
	}
	public class MenuStrip : Panel
	{
		public event Action<MenuStripItem> ItemAdded;
		public event Action<MenuStripItem> ItemRemoved;
		public event DUpdate<MenuStrip> Opening;
		public event DUpdate<MenuStrip> Closing;
		private bool isOpening;
		private bool layoutVertical;
		public bool IsEnterOpen = true;
		public bool IsAutoOnOff = true;

		public MenuStripItem ParentItem
		{
			get { return Parent as MenuStripItem; }
		}
		public IEnumerable<MenuStripItem> Items
		{
			get
			{
				foreach (UIElement child in Childs)
				{
					MenuStripItem item = child as MenuStripItem;
					if (item != null)
					{
						yield return item;
					}
				}
			}
		}
		public VECTOR2 ItemSize
		{
			get
			{
				VECTOR2 size = new VECTOR2();
				foreach (MenuStripItem item in Items)
				{
					VECTOR2 temp = item.ContentSize;
					if (size.X < temp.X)
					{
						size.X = temp.X;
					}
					if (size.Y < temp.X)
					{
						size.Y = temp.Y;
					}
				}
				return size;
			}
		}
		public bool IsOpening
		{
			get { return isOpening; }
			protected internal set
			{
				if (isOpening == value)
				{
					return;
				}
				if (value)
				{
					OnOpening();
				}
				else
				{
					OnClosing();
				}
			}
		}
		public bool IsTop
		{
			get { return ParentItem == null; }
		}
		public bool LayoutVertical
		{
			get { return layoutVertical; }
			set
			{
				if (layoutVertical == value)
					return;
				layoutVertical = value;
				ItemChanged();
			}
		}
		public bool ChildHover
		{
			get
			{
				foreach (MenuStripItem item in Items)
				{
					if (item.IsHover)
					{
						return true;
					}
					MenuStrip strip = item.MenuStripChild;
					if (strip != null && strip.ChildHover)
					{
						return true;
					}
				}
				return false;
			}
		}
		public override VECTOR2 ContentSize
		{
			get
			{
				RECT clip = RECT.Empty;
				foreach (MenuStripItem child in Items)
				{
					RECT temp = child.InParentClip;
					if (IsAutoClip && child.MenuStripChild != null && child.MenuStripChild.isOpening)
					{
						temp = RECT.Union(temp, child.MenuStripChild.ChildClip);
					}
					clip.X = _MATH.Min(temp.X, clip.X);
					clip.Y = _MATH.Min(temp.Y, clip.Y);
					clip.Width = _MATH.Max(temp.Right, clip.Width);
					clip.Height = _MATH.Max(temp.Bottom, clip.Height);
				}
				clip.Width = clip.Width - clip.X;
				clip.Height = clip.Height - clip.Y;
				return clip.Size;
			}
		}
		protected override bool NeedBeginEnd
		{
			get
			{
				return false;
			}
		}
		public new MenuStripItem this[int index]
		{
			get
			{
				int i = 0;
				foreach (UIElement child in Childs)
				{
					MenuStripItem item = child as MenuStripItem;
					if (item != null && i++ == index)
					{
						return item;
					}
				}
				return null;
			}
		}
		public MenuStripItem this[string name]
		{
			get
			{
				foreach (UIElement child in Childs)
				{
					MenuStripItem item = child as MenuStripItem;
					if (item != null && item.Name == name)
					{
						return item;
					}
				}
				return null;
			}
		}

		public MenuStrip()
		{
			this.IsClip = false;
		}

		public void AddItem(MenuStripItem item)
		{
			if (item == null)
			{
				throw new ArgumentNullException();
			}

			base.Add(item);
			ItemChanged();

			if (ItemAdded != null)
			{
				ItemAdded(item);
			}
		}
		public void RemoveItem(MenuStripItem item)
		{
			base.Remove(item);
			ItemChanged();

			if (ItemRemoved != null)
			{
				ItemRemoved(item);
			}
		}
		public void RemoveItem(int index)
		{
			MenuStripItem[] items = Items.ToArray();
			if (index >= 0 && index < items.Length)
			{
				RemoveItem(items[index]);
			}
		}
		public void RefreshItem()
		{
			foreach (MenuStripItem item in Items)
			{
				if (item.MenuStripChild != null)
				{
					item.MenuStripChild.RefreshItem();
				}
			}
			ItemChanged();
		}
		protected virtual void ItemChanged()
		{
			VECTOR2 size = ItemSize;
			int index = 0;
			if (LayoutVertical)
			{
				foreach (MenuStripItem item in Items)
				{
					item.X = 0;
					item.Y = size.Y * index++;
					item.Width = size.X;
					item.Height = size.Y;
				}
			}
			else
			{
				foreach (MenuStripItem item in Items)
				{
					item.X = size.X * index++;
					item.Y = 0;
					item.Width = size.X;
					item.Height = size.Y;
				}
			}
		}
		protected void OnOpening()
		{
			isOpening = true;
			if (Opening != null)
			{
				Opening(this, Entry.Instance);
			}
		}
		internal void OnClosing()
		{
			isOpening = false;
			if (IsAutoOnOff)
			{
				foreach (MenuStripItem item in Items)
				{
					item.IsOpening = false;
				}
			}
			if (Closing != null)
			{
				Closing(this, Entry.Instance);
			}
		}
		protected override void InternalEvent(Entry e)
		{
			if (IsTop && IsOpening && IsAutoOnOff)
			{
                //if (Handled)
                //{
                //    OnClosing();
                //}
                //else
				{
					bool click = e.INPUT.Pointer.IsRelease(0) || e.INPUT.Pointer.IsRelease(1);
                    if (click && !ChildHover)
					{
						OnClosing();
					}
				}
			}
		}

		public static void VisibleChangeTreeNodeMode(MenuStrip sender, Entry e)
		{
			if (!sender.IsTop)
			{
				sender.Visible = sender.IsOpening;
			}
			MenuStripItem parentItem = sender.ParentItem;
			if (parentItem != null)
			{
				MenuStrip parentStrip = parentItem.MenuStripParent;
				sender.X = 20;
				sender.Y = parentItem.Height;

				float drop = sender.Height * (sender.Visible ? 1 : -1);
				TreeNodeMove(parentItem, drop);
			}
		}
		private static void TreeNodeMove(MenuStripItem item, float drop)
		{
			bool flag = false;
			foreach (MenuStripItem parentBrother in item.Brothers)
			{
				if (flag)
				{
					parentBrother.Y += drop;
				}
				else
				{
					if (parentBrother == item)
					{
						flag = true;
					}
				}
			}

			if (item.MenuStripParent.ParentItem != null)
			{
				TreeNodeMove(item.MenuStripParent.ParentItem, drop);
			}
		}
	}
	public class ContextMenuStrip : MenuStrip
	{
		public ContextMenuStrip()
		{
			this.Visible = false;
			this.IsOpening = false;
			this.IsClip = false;
			this.LayoutVertical = true;
			this.Opening += VisibleChangeContextNodeMode;
			this.Closing += VisibleChangeContextNodeMode;
		}
		protected override void OnAddedBy(UIElement parent, int index)
		{
            base.OnAddedBy(parent, index);
			parent.Hover += RightClick;
		}
        protected override void OnRemovedBy(UIElement parent)
		{
            base.OnRemovedBy(parent);
			parent.Hover -= RightClick;
		}
		private void RightClick(UIElement sender, Entry e)
		{
			if (e.INPUT.Pointer.IsRelease(1))
			{
				OnOpening();
			}
		}
		public void VisibleChangeContextNodeMode(MenuStrip sender, Entry e)
		{
			Visible = sender.IsOpening;
			if (Visible)
			{
				MenuStripItem parentItem = ParentItem;
				if (parentItem == null)
				{
					Location = Parent.ConvertGraphicsToLocal(e.INPUT.Pointer.Position);
				}
				else
				{
					MenuStrip parentStrip = parentItem.MenuStripParent;
					if (parentStrip.LayoutVertical)
					{
						X = parentStrip.Width;
						Y = 0;
					}
					else
					{
						X = 0;
						Y = parentStrip.Height;
					}
				}
			}
		}
	}
	public class MenuStripItem : Label
	{
		public TEXTURE Icon;
		public TEXTURE IconHasSubItem;

		public MenuStrip MenuStripChild
		{
			get
			{
				foreach (UIElement item in Childs)
				{
					return item as MenuStrip;
				}
				return null;
			}
			set
			{
				MenuStrip child = MenuStripChild;
				if (child == value)
					return;

				if (child != null)
				{
					Remove(0);
				}

				if (value != null)
				{
					value.Visible = false;
					AddChildFirst(value);
				}
			}
		}
		public MenuStrip MenuStripParent
		{
			get { return Parent as MenuStrip; }
		}
		public IEnumerable<MenuStripItem> Brothers
		{
			get
			{
				return MenuStripParent.Items;
			}
		}
		public IEnumerable<MenuStripItem> Items
		{
			get
			{
				return MenuStripChild.Items;
			}
		}
		public bool IsTop
		{
			get { return MenuStripParent.IsTop; }
		}
		public bool IsOpening
		{
			get
			{
				MenuStrip child = MenuStripChild;
				return child != null && child.IsOpening;
			}
			set
			{
				MenuStrip child = MenuStripChild;
				if (child != null)
				{
					child.IsOpening = value;
				}
			}
		}
		public bool NeedLayout
		{
			get
			{
                if (MenuStripParent == null)
                    return false;

				foreach (MenuStripItem item in Brothers)
				{
					if (item.Icon != null || item.IconHasSubItem != null)
					{
						return true;
					}
				}
				return !IsTop && Icon == null && IconHasSubItem == null;
			}
		}
		public override VECTOR2 ContentSize
		{
			get
			{
				VECTOR2 size = base.ContentSize;
				if (NeedLayout)
				{
					size.X += size.Y * 2;
				}
				return size;
			}
		}

		public MenuStripItem()
		{
			this.Clicked += ClickedOpen;
			this.Enter += EnterOpen;
		}

		protected override void InternalDraw(GRAPHICS spriteBatch, Entry e)
		{
			bool needLayout = NeedLayout;
			VECTOR2 temp = UIText.Padding;
			if (needLayout)
			{
				UIText.Padding += new VECTOR2(Height * 2, 0);
			}
			base.InternalDraw(spriteBatch, e);
			if (needLayout)
			{
				UIText.Padding = temp;
			}

			if (Icon != null)
			{
				spriteBatch.Draw(Icon, new RECT(ViewClip.X, ViewClip.Y, Height, Height), GRAPHICS.NullSource, Color);
			}

			if (IconHasSubItem != null && MenuStripChild != null)
			{
				spriteBatch.Draw(IconHasSubItem, new RECT(ViewClip.X + Width - Height, ViewClip.Y, Height, Height), GRAPHICS.NullSource, Color);
			}
		}
		private void ClickedOpen(UIElement sender, Entry e)
		{
			MenuStrip menuStrip = MenuStripChild;
			if (menuStrip != null)
			{
				menuStrip.IsOpening = !menuStrip.IsOpening;
				if (IsTop)
				{
					MenuStripParent.IsOpening = menuStrip.IsOpening;
				}
			}
            else
            {
                menuStrip = MenuStripParent;
                menuStrip.IsOpening = !menuStrip.IsOpening;
                //Handled = true;
            }
		}
		private void EnterOpen(UIElement sender, Entry e)
		{
			if (MenuStripParent.IsEnterOpen)
			{
				MenuStripItem opened = Brothers.FirstOrDefault(i => i.IsOpening);
				if (opened != null && opened != this)
				{
					opened.IsOpening = false;
					IsOpening = true;
				}
			}
		}
	}

	public class ConfigEditor
	{
		public const string CONFIG_FILE = "eee.cfg";

		public ProjectData LastOpenProject;
		public ProjectData[] RecentProjects = new ProjectData[0];
		public int OperationLogCount = 50;
		public RECT WindowClip = new RECT(0, 0, 960, 540);
		public bool WindowMax;
		public COLOR BGColor = COLOR.CornflowerBlue;

		public void RefreshRecentProject()
		{
			if (RecentProjects != null)
			{
				RecentProjects = RecentProjects.Where(p => File.Exists(p.ProjectPath)).ToArray();
				RecentProjects.SortQuit(true, s => Utility.ToUnixTimestamp(s.LastCloseTime));
			}
		}
		public void OpenProject(ProjectData project)
		{
			if (project == null)
				return;
			project.LastCloseTime = DateTime.Now;
			LastOpenProject = project;
			int index = RecentProjects.IndexOf(p => p.ProjectPath == project.ProjectPath);
			if (index == -1)
				RecentProjects = RecentProjects.Add(project);
			else
				RecentProjects[index] = project;
			RefreshRecentProject();
		}
	}
	public class ProjectData
	{
		public string ProjectPath;
		public string ProjectEditorName;
		public DateTime LastCloseTime;
	}
	public class DirSystem
	{
		public string File;
		public DirSystem[] Dir;

		public string[] Directories
		{
			get
			{
				if (IsFile)
				{
					return Path.GetDirectoryName(File).Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
				}
				else
				{
					return File.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
				}
			}
		}
		public bool IsFile
		{
			get { return Dir == null; }
		}

		public DirSystem()
		{
		}
		public DirSystem(string directory)
		{
			File = directory;
			Dir = new DirSystem[0];
		}

		public bool Add(string file)
		{
			if (IsFile)
				return false;
			if (file.StartsWith(File))
			{
				if (Dir == null || Dir.FirstOrDefault(d => d.File == file) == null)
				{
					Dir = Dir.Add(new DirSystem() { File = file });
					return true;
				}
			}
			return false;
		}
		public void Sort()
		{
			if (IsFile)
				return;
			Dir.SortQuit(true, f => (f.IsFile ? 0 : 1000) + f.File[0]);
		}
		public void RefreshDirectory()
		{
			if (IsFile)
				return;
			Dir = ReadDirectoryRelative(File, string.Empty);
		}
		public override string ToString()
		{
			return File;
		}
		public override int GetHashCode()
		{
			return File.GetHashCode();
		}

		public static DirSystem[] ReadDirectory()
		{
			return ReadDirectory(Environment.CurrentDirectory);
		}
		public static DirSystem[] ReadDirectory(string directory)
		{
			DirSystem temp = new DirSystem(directory);
			ReadDirectoryFromDir(ref temp);
			return temp.Dir;
		}
		public static DirSystem[] ReadDirectoryRelative()
		{
			return ReadDirectory(Environment.CurrentDirectory);
		}
		public static DirSystem[] ReadDirectoryRelative(string directory, string root = null)
		{
			DirSystem[] dirs = ReadDirectory(directory);
			dirs = RelativeDirectory(dirs, root);
			return dirs;
		}
		private static void ReadDirectoryFromDir(ref DirSystem dir)
		{
			List<DirSystem> all = new List<DirSystem>();

			string[] dirs = Directory.GetDirectories(dir.File);
			for (int i = 0; i < dirs.Length; i++)
			{
				DirSystem temp = new DirSystem();
				temp.File = dirs[i];
				ReadDirectoryFromDir(ref temp);
				all.Add(temp);
			}

			string[] files = Directory.GetFiles(dir.File);
			for (int i = 0; i < files.Length; i++)
			{
				DirSystem temp = new DirSystem();
				temp.File = files[i];
				all.Add(temp);
			}

			dir.Dir = all.ToArray();
		}
		public static DirSystem[] RelativeDirectory(DirSystem[] dirs, string root = null)
		{
			if (dirs.Length == 0)
				return dirs;

			if (root == null)
			{
				string sample = dirs[0].File;
				root = Path.GetDirectoryName(sample);
			}
			for (int i = 0; i < dirs.Length; i++)
			{
				RelativeDirectory(ref dirs[i], ref root);
			}

			return dirs;
		}
		private static void RelativeDirectory(ref DirSystem dir, ref string root)
		{
			if (dir.File.StartsWith(root))
			{
				dir.File = dir.File.Remove(0, root.Length);
				if (dir.File.StartsWith("/") || dir.File.StartsWith("\\"))
				{
					dir.File = dir.File.Remove(0, 1);
				}
			}

			if (dir.Dir != null)
			{
				for (int i = 0; i < dir.Dir.Length; i++)
				{
					RelativeDirectory(ref dir.Dir[i], ref root);
				}
			}
		}
	}
}
