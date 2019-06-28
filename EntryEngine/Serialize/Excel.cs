using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EntryEngine.Serialize
{
	// csv file default encoding is GB2312
	public class CSVWriter : StringWriter
	{
		public static Encoding CSVEncoding = Encoding.UTF8;
        //private HashSet<string> fields = new HashSet<string>();
        private int columnCount;
        private int cursor;

		public override void WriteTable(StringTable table)
		{
			int col = table.ColumnCount;
			int index = 0;
			foreach (string column in table.Columns)
			{
				if (column == null)
					throw new ArgumentNullException("columnName");
				WriteString(column);
				if (++index != col)
					WriteSeperator();
			}

            //for (int i = 1; i < CSVReader.TITLE_ROW_COUNT; i++)
            //    WriteLine();

			index = 0;
			while (index < table.RowCount)
			{
				WriteLine();
				for (int i = 0; i < col; i++)
				{
					WriteString(table[i, index]);
					if (i != col - 1)
						WriteSeperator();
				}
				index++;
			}
		}
		protected override bool WriteNull(Type type)
		{
            if (type.IsValueType)
            {
                WriteString("");
                return true;
            }
			if (type.IsClass)
				return false;
			else
				return base.WriteNull(type);
		}
		protected override void WriteBool(bool value)
		{
			builder.Append(value ? 1 : 0);
		}
		protected override void WriteString(object value)
		{
			if (value == null)
			{
				builder.Append(_XML.NULL);
				return;
			}

			string str = value.ToString();

            bool special = str.Contains('\r') || str.Contains('\n') || str.Contains('\"') || str.Contains(',');
			if (special)
			{
				builder.Append('\"');
				builder.Append(Encode(str));
				builder.Append('\"');
			}
			else
			{
				builder.Append(str);
			}
		}
        public void WriteValue(object value)
        {
            WriteString(value);
            if (cursor++ < columnCount - 1)
            {
                WriteSeperator();
            }
            else
            {
                cursor = 0;
                WriteLine();
            }
        }
		protected override void WriteArray(System.Collections.IEnumerable value, Type type)
		{
			if (columnCount == 0)
				base.WriteArray(value, type);
			else
				WriteCustomValue(value, type);
		}
        protected override void WriteClassObject(object value, Type type)
        {
            bool first = columnCount == 0;

            if (first)
            {
                if (this.type.IsArray && this.type.GetElementType() == type)
                {
                    type = this.type.GetElementType();
                    this.type = type;
                }
                Setting.Serialize(type, value, field => { columnCount++; });
            }

            if (columnCount == 0)
                return;

            if (first)
            {
                // field name
                Setting.Serialize(type, value,
                    field =>
                    {
                        WriteValue(field.VariableName);
                    });
                // field type
                if (CSVReader.TITLE_ROW_COUNT > 1)
                {
                    //Setting.Serialize(type, value,
                    //    field =>
                    //    {
                    //        WriteValue(field.Type.CodeName());
                    //    });
                    WriteEmptyRow();
                }
                // field description
                if (CSVReader.TITLE_ROW_COUNT > 2)
                {
                    //Setting.Serialize(type, value,
                    //    field =>
                    //    {
                    //        var desc = field.MemberInfo.GetAttribute<ASummary>();
                    //        WriteValue(desc == null ? string.Empty : desc.Note);
                    //    });
                    WriteEmptyRow();
                }
                // other empty row
                for (int i = 3; i < CSVReader.TITLE_ROW_COUNT; i++)
                    WriteEmptyRow();
            }
            else
            {
                if (type != this.type || cursor != 0)
                {
                    if (value != null)
                    {
						WriteCustomValue(value, type);
                    }
                    return;
                }
                WriteLine();
            }

            if (value == null)
                return;

            //int count = 0;
            cursor = 0;
            Setting.Serialize(type, value,
                field =>
                {
                    cursor++;
                    WriteObject(field.GetValue(), field.Type);
                    //if (count++ < columnCount - 1)
                    if (cursor < columnCount)
                        WriteSeperator();
                });
            cursor = 0;
        }
		private void WriteCustomValue(object value, Type type)
		{
			JsonWriter writer = new JsonWriter();
			writer.Setting = this.Setting;
			//writer.MultipleRoot = true;
			writer.WriteObject(value, type);
			WriteString(writer.Result);
		}
		private void WriteEmptyRow()
		{
			int count = columnCount - 1;
			for (int i = 0; i < count; i++)
				WriteSeperator();
			WriteLine();
		}
		private void WriteSeperator()
		{
			builder.Append(',');
		}
		private void WriteLine()
		{
            // AppendLine: mono is only '\n'
            // string.Format("{0}\r\n", true)也不能使用\r\n，否则会导致整个程序卡死的报错
			builder.Append("\r\n");
		}

        public static void WriteTable(StringTable table, string file)
        {
            CSVWriter writer = new CSVWriter();
            writer.WriteTable(table);
			_IO.WriteText(file, writer.Result, CSVWriter.CSVEncoding);
        }
        public static string Encode(string str)
        {
            str = str.Replace("\"", "\"\"");
            str = str.Replace("\r\n", "\n");
            str = str.Replace("\r", "\n");
            return str;
        }
        public static string Serialize(object value)
        {
            if (value == null)
                return string.Empty;
            return Serialize(value, value.GetType(), SerializeSetting.DefaultSetting);
        }
        public static string Serialize(object value, Type type)
        {
            return Serialize(value, type, SerializeSetting.DefaultSetting);
        }
        public static string Serialize(object value, Type type, SerializeSetting setting)
        {
            if (value == null || type == null)
                throw new ArgumentNullException();
            if (value != null && type == null)
                type = value.GetType();
            CSVWriter writer = new CSVWriter();
            writer.Setting = setting;
            writer.WriteObject(value, type);
            return writer.Result;
        }
	}
	public class CSVReader : StringReader
	{
		public const byte TITLE_ROW_COUNT = 3;
        private static char[] SPLIT = new char[] { ',' };

        private class ColumnProperty
        {
            public string Name;
            public PropertyInfo Property;
            public FieldInfo Field;
            public bool Special;
        }

		//public bool IsSkipTitleRow = true;
        ColumnProperty[] columns;

		public CSVReader() : this(null)
		{
		}
		//public CSVReader(string content) : this(content, true)
		//{
		//}
		//public CSVReader(bool skipTitle) : this(null, skipTitle)
		//{
		//}
		//public CSVReader(string content, bool skipTitle) : base(content)
		//{
		//    WHITE_SPACE = "";
		//    WORD_BREAK = "\",\r";
		//    this.IsSkipTitleRow = skipTitle;
		//}
		public CSVReader(string content) : base(content)
		{
            WHITE_SPACE = "";
			WORD_BREAK = "\",\r";
		}

		private string PeekGrid
		{
			get
			{
				int temp = pos;
				string value = ReadGrid;
				pos = temp;
				return value;
			}
		}
		private string ReadGrid
		{
			get
			{
				char c = PeekChar;

                // 最后一列空字符串会变成null
                if (c == '\r')
                    return string.Empty;

                if (End)
                    return string.Empty;

                if (c == ',')
                {
                    Read();
                    return string.Empty;
                }

				string value;
				if (PeekChar == '\"')
				{
					Read();

					int temp = pos;
					while (true)
					{
						int index = str.IndexOf('\"', temp);
						int index2 = str.IndexOf("\"\"", index);
						if (index == index2)
						{
							temp = index2 + 2;
						}
						else
						{
							temp = index;
							break;
						}
					}

					value = str.Substring(pos, temp - pos);
					pos = temp + 1;
				}
				else
				{
					value = NextWord;
				}

                if (PeekChar == ',')
                {
                    Read();
                }
				if (value == _XML.NULL || value == _XML.NULL2)
				{
					return null;
				}
				else
				{
					return value;
				}
			}
		}

        public StringTable ReadTable(int rows)
        {
            StringTable table = new StringTable();

            //table.Columns.AddRange(PeekGridColumnKey());
            foreach (var column in PeekGridColumnKey(false))
                table.AddColumn(column);

            while (pos < len)
            {
                if (rows >= 0 && table.RowCount >= rows)
                    break;

                if (PeekIsNullRow())
                {
                    EatLine();
                    continue;
                }

                for (int i = 0; i < table.ColumnCount; i++)
                {
                    if (i == 0 && PeekChar == ',')
                    {
                        table.AddValue(string.Empty);
                        Read();
                    }
                    else
                        table.AddValue(Decode(ReadGrid));
                }
            }

            return table;
        }
		public override StringTable ReadTable()
		{
            return ReadTable(-1);
		}
        protected override string ReadNextString()
        {
            return Decode(ReadGrid);
        }
        //protected override void ReadArray(Type elementType, List<object> list)
        //{
        //    while (pos < len)
        //    {
        //        list.Add(ReadObject(elementType));
        //    }
        //}
        protected override object ReadClassObject(Type type)
        {
            if (columns == null)
            {
                List<string> keys = PeekGridColumnKey(true);
                columns = new ColumnProperty[keys.Count];
                var fields = Setting.GetFieldsDic(type);
                var properties = Setting.GetPropertiesDic(type);
                for (int i = 0; i < keys.Count; i++)
                {
                    ColumnProperty column = new ColumnProperty();
                    column.Name = keys[i];
                    FieldInfo field;
                    if (fields.TryGetValue(keys[i], out field))
                    {
                        column.Field = field;
                        column.Special = field.FieldType.IsCustomType();
                    }
                    else
                    {
                        PropertyInfo property;
                        if (!properties.TryGetValue(keys[i], out property))
                            throw new KeyNotFoundException(string.Format("缺少CSV列{0}[长度:{1}]", keys[i], keys[i].Length));
                        column.Property = property;
                        column.Special = property.PropertyType.IsCustomType();
                    }
                    columns[i] = column;
                }
            }
            //else
            //{
            //    JsonReader reader = new JsonReader(ReadNextString());
            //    reader.Setting = this.Setting;
            //    return reader.ReadObject(type);
            //}

            object obj;
            if (type.IsStatic())
                obj = null;
            else
                obj = Activator.CreateInstance(type);

            for (int i = 0; i < columns.Length; i++)
            {
                string name = columns[i].Name;
                //string text = Decode(ReadGrid);
                if (columns[i].Special)
                {
                    JsonReader reader = new JsonReader(ReadNextString());
                    reader.Setting = this.Setting;
                    if (columns[i].Field == null)
                        columns[i].Property.SetValue(obj, reader.ReadObject(columns[i].Property.PropertyType), null);
                    else
                        columns[i].Field.SetValue(obj, reader.ReadObject(columns[i].Field.FieldType));
                }
                else
                {
                    if (columns[i].Field == null)
                        columns[i].Property.SetValue(obj, ReadObject(columns[i].Property.PropertyType), null);
                    else
                        columns[i].Field.SetValue(obj, ReadObject(columns[i].Field.FieldType));
                }
            }

            EatLine();

            return obj;
        }
		private List<string> PeekGridColumnKey(bool skipTitle)
		{
			List<string> keys = new List<string>();
			while (pos < len)
			{
                if (PeekChar == ',')
                {
                    keys.Add(string.Empty);
                    Read();
                }
                else
                {
                    string value = ReadGrid;
                    keys.Add(value);
                    if (PeekChar == '\r')
                        break;
                }
			}
			if (skipTitle)
				for (int i = 0; i < TITLE_ROW_COUNT; i++)
					EatLine();
			return keys;
		}
		private bool PeekIsNull()
		{
			return string.IsNullOrEmpty(PeekNext("\r"));
		}
		private bool PeekIsNullRow()
		{
			if ((pos == 0 || (pos > 1 && str[pos - 1] == '\n' && str[pos - 2] == '\r'))
				&& PeekIsNull())
				return false;
            return PeekNext("\r").Split(SPLIT, StringSplitOptions.RemoveEmptyEntries).Length == 0;
		}
		private void EatLine()
		{
			int index = str.IndexOf("\r\n", pos);
			if (index == -1)
				pos = len;
			else
				pos = index + 2;
		}

        public static string Decode(string str)
        {
            if (str == null)
                return null;
            str = str.Replace("\"\"", "\"");
            //str = str.Replace("\n", "\r\n");
            return str;
        }
        public static object Deserialize(string buffer, Type type)
        {
            return Deserialize(buffer, type, SerializeSetting.DefaultSerializeAll);
        }
        public static object Deserialize(string buffer, Type type, SerializeSetting setting)
        {
            if (string.IsNullOrEmpty(buffer))
                return null;
            if (type == null)
                throw new ArgumentNullException();
            CSVReader reader = new CSVReader(buffer);
            reader.Setting = setting;
            return reader.ReadObject(type);
        }
        public static T Deserialize<T>(string buffer)
        {
            return (T)Deserialize(buffer, typeof(T));
        }
        public static T Deserialize<T>(string buffer, SerializeSetting setting)
        {
            return (T)Deserialize(buffer, typeof(T), setting);
        }
	}
}
