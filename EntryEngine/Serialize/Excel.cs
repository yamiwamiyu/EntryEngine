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
            public PropertyInfo Property;
            public FieldInfo Field;
            public bool Special;
        }

		//public bool IsSkipTitleRow = true;

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
        //public override object ReadObject(Type type)
        //{
        //    bool isArray = type.IsArray;
        //    Type arrayType = type;
        //    if (!isArray)
        //        arrayType = type.MakeArrayType();
        //    var time = DateTime.Now.Ticks;
        //    var readNode = ReadToNode(type, isArray ? -1 : 1);
        //    _LOG.Debug("Read node end: {0}", DateTime.Now.Ticks - time);
        //    var read = XmlReader.ReadObject(arrayType, readNode, Setting);
        //    _LOG.Debug("Read object end: {0}", DateTime.Now.Ticks - time);
        //    if (isArray)
        //        return read;
        //    else
        //        return ((Array)read).GetValue(0);
        //}
        public override object ReadObject(Type type)
        {
            bool isArray = type.IsArray;
            if (isArray)
                type = type.GetElementType();

            //if (Setting.AutoType)
            //    type = Type.GetType(ReadType());

            List<object> objects = null;
            List<string> keys = PeekGridColumnKey(true);
            ColumnProperty[] columns = new ColumnProperty[keys.Count];
            var fields = Setting.GetFields(type);
            var properties = Setting.GetProperties(type);
            for (int i = 0; i < keys.Count; i++)
            {
                ColumnProperty column = new ColumnProperty();
                var field = fields.FirstOrDefault(f => f.Name == keys[i]);
                if (field != null)
                {
                    column.Field = field;
                    column.Special = field.FieldType.IsCustomType();
                }
                else
                {
                    var property = properties.FirstOrDefault(p => p.Name == keys[i]);
                    if (property == null)
                        throw new KeyNotFoundException(string.Format("缺少CSV列{0}[长度:{1}]", keys[i], keys[i].Length));
                    column.Property = property;
                    column.Special = property.PropertyType.IsCustomType();
                }
                columns[i] = column;
            }

            // 用于后面动态创建objects时计算一个合适的capcity
            int start = pos;
            while (pos < len)
            {
                if (PeekIsNullRow())
                {
                    EatLine();
                    continue;
                }

                object obj;
                if (type.IsStatic())
                    obj = null;
                else
                    obj = Activator.CreateInstance(type);

                for (int i = 0; i < keys.Count; i++)
                {
                    string name = keys[i];
                    string text = Decode(ReadGrid);
                    if (columns[i].Special)
                    {
                        JsonReader reader = new JsonReader(text);
                        reader.Setting = this.Setting;
                        if (columns[i].Field == null)
                            columns[i].Property.SetValue(obj, reader.ReadObject(columns[i].Property.PropertyType), null);
                        else
                            columns[i].Field.SetValue(obj, reader.ReadObject(columns[i].Field.FieldType));
                    }
                    else
                    {
                        if (columns[i].Field == null)
                            columns[i].Property.SetValue(obj, ReadValue(columns[i].Property.PropertyType, text), null);
                        else
                            columns[i].Field.SetValue(obj, ReadValue(columns[i].Field.FieldType, text));
                    }
                }

                if (obj != null)
                {
                    if (objects == null)
                        objects = new List<object>((int)(len * 1.2f / (pos - start)));

                    objects.Add(obj);
                    if (!isArray)
                        return obj;
                }
            }

            if (isArray)
            {
                int count = 0;
                if (objects != null)
                    count = objects.Count;
                Array array = Array.CreateInstance(type, count);
                for (int i = 0; i < count; i++)
                    array.SetValue(objects[i], i);
                return array;
            }
            else
                return objects[0];
        }
        private XmlNode ReadToNode(Type type, int row)
        {
            if (str == null)
                throw new ArgumentNullException("read string can not be null");

            //if (Setting.AutoType)
            //    type = Type.GetType(ReadType());

            List<int> customType = new List<int>();
            if (type != null)
            {
                if (type.IsArray)
                    type = type.GetElementType();
                int index = 0;
                Setting.Serialize(type, null,
                    v =>
                    {
                        if (v.Type.IsCustomType())
                            customType.Add(index);
                        index++;
                    });
            }

            List<XmlNode> rows = new List<XmlNode>();
            List<string> keys = PeekGridColumnKey(true);
            while (pos < len && (row < 0 || rows.Count < row))
            {
                if (PeekIsNullRow())
                {
                    EatLine();
                    continue;
                }

                XmlNode array = new XmlNode();
                array.Name = _XML.ARRAY_NODE;
                if (PeekIsNull())
                {
                    EatLine();
                    array.Value = null;
                }
                else
                {
                    for (int i = 0; i < keys.Count; i++)
                    {
                        XmlNode node = new XmlNode();
                        node.Name = keys[i];
                        node.Value = Decode(ReadGrid);
                        if (customType.Contains(i))
                        {
                            JsonReader reader = new JsonReader(node.Value);
                            reader.Setting = this.Setting;
                            node.AddRange(reader.ReadToNode().ToArray());
                        }
                        else
                        {
                            node.Value = _XML.Escape(node.Value);
                        }
                        array.Add(node);
                    }
                }
                rows.Add(array);
            }

            return XmlNode.CreateRoot(rows);
        }
		public override XmlNode ReadToNode()
		{
            return ReadToNode(null, -1);
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
            return Deserialize(buffer, type, SerializeSetting.DefaultSetting);
        }
        public static object Deserialize(string buffer, Type type, SerializeSetting setting)
        {
            if (string.IsNullOrEmpty(buffer))
                return null;
            if (type == null)
                throw new ArgumentNullException();
            JsonReader reader = new JsonReader(buffer);
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
