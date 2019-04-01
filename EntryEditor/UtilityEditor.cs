using System;
using System.Collections.Generic;
using EntryEngine;
using EntryEngine.Serialize;
using System.IO;

namespace EntryEditor
{
	public static class UtilityEditor
	{
		private static Dictionary<string, string> strings = new Dictionary<string, string>();

		public static void LoadStringTable()
		{
			if (File.Exists("StringTable.csv"))
			{
				CSVReader reader = new CSVReader(Utility.ReadAllText("StringTable.csv"));
				StringTable table = reader.ReadTable();
				for (int i = 0; i < table.ColumnCount; i++)
				{
					strings[table[0, i]] = table[1, i];
				}
			}
		}
		public static string GetString(string key)
		{
			string msg;
			if (strings.TryGetValue(key, out msg))
			{
				return msg;
			}
			throw new ArgumentException();
		}
	}
}
