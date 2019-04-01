using System.Collections.Generic;
using System;
using EntryEngine.Serialize;
using System.IO;
using EntryEngine;

namespace EntryEditor
{
	public static class EditorConfig
	{
		public static ProjectData LastOpenProject;
		public static ProjectData[] RecentProjects;
		public static int OperationLogCount = 50;

		private const string CONFIG_FILE = "editor.cfg";
		public static void Save()
		{
			XmlWriter writer = new XmlWriter();
			writer.Setting.Static = true;
			writer.WriteObject(null, typeof(EditorConfig));
			Utility.WriteAllText(CONFIG_FILE, writer.Result);
		}
		public static void Load()
		{
			if (File.Exists(CONFIG_FILE))
			{
				XmlReader reader = new XmlReader(Utility.ReadAllText(CONFIG_FILE));
				reader.Setting.Static = true;
				reader.ReadObject(typeof(EditorConfig));

				if (RecentProjects != null)
				{
					RecentProjects.SortQuit(true, s => Utility.ToUnixTimestamp(s.LastCloseTime));
				}
			}
		}
	}
	public class ProjectData
	{
		public string ProjectPath;
		public string ProjectEditorName;
		public DateTime LastCloseTime;
	}
}
