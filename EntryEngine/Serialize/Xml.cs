using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EntryEngine.Serialize
{
	public static class _XML
	{
		const string ESCAPE_START = "&#";
		const string ESCAPE_END = ";";
		public static EEKeyValuePair<string, string>[] ESCAPE =
			new EEKeyValuePair<string, string>[]
			{
				new EEKeyValuePair<string, string>("&", "&#38;"),
				new EEKeyValuePair<string, string>("<", "&#60;"),
				new EEKeyValuePair<string, string>(">", "&#62;"),
				new EEKeyValuePair<string, string>("'", "&#39;"),
				new EEKeyValuePair<string, string>("\"", "&#34;"),
				new EEKeyValuePair<string, string>("\\", "&#92;"),
			};
		public static string ARRAY_NODE = "e";
		public static string INDENT_SPACE = "\t";
		public static string NULL = "null";
        public static string NULL2 = "NULL";
		public static string ROOT = "root";

		public static XmlNode BuildRoot(IEnumerable<XmlNode> nodes)
		{
			XmlNode root = new XmlNode();
			root.Name = ROOT;
			root.AddRange(nodes);
			if (root.ChildCount == 0)
				root.Value = null;
			return root;
		}

		public static string Escape(string str)
		{
			if (str == null)
				return null;
			foreach (EEKeyValuePair<string, string> escape in _XML.ESCAPE)
			{
				str = str.Replace(escape.Key, escape.Value);
			}
			return str;
		}
		public static string Escape(string str, params char[] escape)
		{
			if (str == null)
				return null;
			foreach (char c in escape)
			{
				str = str.Replace(c.ToString(), string.Format("{0}{1}{2}", ESCAPE_START, (int)c, ESCAPE_END));
			}
			return str;
		}
		public static string UnEscape(string str)
		{
			if (str == null)
				return null;
			foreach (EEKeyValuePair<string, string> escape in _XML.ESCAPE)
			{
				str = str.Replace(escape.Value, escape.Key);
			}
			return str;
		}
		public static string UnEscapeAscii(string str)
		{
			if (str == null)
				return null;
			int num = 0;
			while (true)
			{
				int num0 = str.IndexOf(ESCAPE_START, num);
				if (num0 == -1)
					return str;

				int num1 = str.IndexOf(ESCAPE_END, num0);
				if (num1 == -1)
					return str;

				int ascii;
				if (int.TryParse(str.Substring(num0 + 2, num1 - num0 - 2), out ascii))
				{
					str = str.Replace(str.Substring(num0, num1 - num0 + 1), ((char)ascii).ToString());
					num = num0 + 1;
				}
			}
		}
	}
	public class XmlNode : Tree<XmlNode>
	{
		private string name;
		private Dictionary<string, string> attributes = new Dictionary<string, string>();
		public string Value = string.Empty;

		public string Name
		{
			get { return name; }
			set
			{
				if (!string.IsNullOrEmpty(value.Trim()))
				{
					this.name = value;
				}
			}
		}
		public XmlNode this[string name]
		{
			get
			{
				foreach (XmlNode node in Childs)
					if (node.name == name)
						return node;
				return null;
			}
		}
		public Dictionary<string, string> Attributes
		{
			get { return attributes; }
		}
		public string OutterText
		{
			get { return BuildXml(this, false); }
		}
		public string OutterXml
		{
			get { return BuildXml(this, true); }
		}

		public XmlNode()
		{
		}
		public XmlNode(string name)
		{
			this.name = name;
		}
		public XmlNode(string name, string value)
		{
			this.name = name;
			this.Value = value;
		}
		public XmlNode(string name, string value, IEnumerable<XmlNode> nodes)
		{
			this.name = name;
			this.Value = value;
			AddRange(nodes);
		}

		public XmlNode Add(string name)
		{
			XmlNode node = new XmlNode();
			node.Name = name;
            Add(node);
			return node;
		}
        public XmlNode Add(string name, string value)
        {
            XmlNode node = new XmlNode();
            node.Name = name;
            node.Value = value;
            Add(node);
            return node;
        }

        private static string BuildXml(XmlNode node)
        {
            return BuildXml(node, false);
        }
		private static string BuildXml(XmlNode node, bool escape)
		{
            StringBuilder builder = new StringBuilder();
            BuildXml(builder, node, escape);
            return builder.ToString();
		}
        private static void BuildXml(StringBuilder text, XmlNode node, bool escape)
        {
            string space = string.Empty;
            int level = node.Depth;
            // space
            for (int i = 0; i < level; i++)
                space += _XML.INDENT_SPACE;
            text.Append(space);
            // start of node
            text.Append("<{0}", node.name);
            // attributes
            foreach (string key in node.attributes.Keys)
                text.Append(" {0}=\"{1}\"", key, node.attributes[key]);
            // end of node
            text.Append(">");
            // child nodes
            if (node.Childs.Count > 0)
            {
                text.AppendLine();
                foreach (XmlNode child in node.Childs)
                {
                    BuildXml(text, child, escape);
                    text.AppendLine();
                }
                text.Append(space);
            }
            else
            {
                if (node.Value == null)
                    text.Append(_XML.NULL);
                else
                    if (escape)
                        text.Append(_XML.Escape(node.Value));
                    else
                        // inner text
                        text.Append(node.Value);
            }
            // end
            text.Append("</{0}>", node.name);
        }
		public static XmlNode WriteObject(object value)
		{
			return WriteObject(value, SerializeSetting.DefaultSetting);
		}
		public static XmlNode WriteObject(object value, SerializeSetting setting)
		{
			return WriteObject(value, value.GetType(), setting);
		}
		public static XmlNode WriteObject(object value, Type type, SerializeSetting setting)
		{
			return WriteValue(_XML.ROOT, value, type, setting);
		}
		private static XmlNode WriteValue(string name, object value, Type type, SerializeSetting setting)
		{
			XmlNode node = new XmlNode();
			node.name = name;
			if (value == null)
			{
				node.Value = null;
			}
			else if (type.IsEnum)
			{
				node.Value = ((int)value).ToString();
			}
			else if (value is char || value is string
				|| value is bool
				|| value is bool
				|| value is sbyte
				|| value is byte
				|| value is short
				|| value is ushort
				|| value is int
				|| value is uint
				|| value is float
				|| value is long
				|| value is ulong
				|| value is double)
			{
				node.Value = value.ToString();
			}
			else if (value is TimeSpan)
			{
				node.Value = ((TimeSpan)value).Ticks.ToString();
			}
			else if (value is DateTime)
			{
				node.Value = Utility.ToUnixTimestamp((DateTime)value).ToString();
			}
			else if (value is Array)
			{
				Array array = (Array)value;
				Type elementType = type.GetElementType();
				foreach (object item in array)
				{
					node.Add(WriteValue(_XML.ARRAY_NODE, item, elementType, setting));
				}
			}
			else
			{
				setting.SerializeField(type,
				field =>
				{
					node.Add(WriteValue(field.Name, field.GetValue(value), field.FieldType, setting));
				});
				if (setting.Property)
				{
					setting.SerializeProperty(type,
						property =>
						{
							node.Add(WriteValue(property.Name, property.GetValue(value, _SERIALIZE.EmptyObjects), property.PropertyType, setting));
						});
				}
			}
			return node;
		}
		public static XmlNode CreateRoot()
		{
			return new XmlNode(_XML.ROOT);
		}
		public static XmlNode CreateRoot(IEnumerable<XmlNode> roots)
		{
			XmlNode root = CreateRoot();
			root.AddRange(roots);
			return root;
		}
	}
	public class XmlWriter : StringWriter
	{
        public const string SPECIAL = "#";
		public bool MultipleRoot;

		public override void WriteTable(StringTable table)
		{
			WriteNode(_XML.ROOT);

            string column;
            for (int i = 0; i < table.RowCount; i++)
            {
                for (int j = 0; j < table.ColumnCount; j++)
                {
                    WriteNode(_XML.ARRAY_NODE);

                    column = table.GetColumn(i);
                    WriteNode(column);
                    WriteString(table[i, j]);
                    WriteNodeClose(column);

                    WriteNodeClose(_XML.ARRAY_NODE);
                }
            }

			WriteNodeClose(_XML.ROOT);
		}
		public void WriteNode(string name)
		{
			builder.Append(string.Format("<{0}>", name));
		}
		public void WriteNodeClose(string name)
		{
			builder.Append(string.Format("</{0}>", name));
		}
		public void WriteNode(string name, string value)
		{
			WriteNode(name);
			WriteString(_XML.Escape(value));
			WriteNodeClose(name);
		}
		public override void WriteObject(object value, Type type)
		{
			bool isFirst = IsFirst && builder.Length == 0;
			bool single = !MultipleRoot;
			if (single)
			{
				if (isFirst)
					WriteNode(_XML.ROOT);
			}
			base.WriteObject(value, type);
			if (single)
			{
				if (isFirst)
					WriteNodeClose(_XML.ROOT);
			}
		}
		protected override void WriteString(object value)
		{
			builder.Append(_XML.Escape(value.ToString()));
		}
		protected override void WriteArray(IEnumerable value, Type type)
		{
			foreach (object obj in value)
			{
				if (type == null)
					type = obj.GetType();
				WriteNode(_XML.ARRAY_NODE);
				WriteObject(obj, type);
				WriteNodeClose(_XML.ARRAY_NODE);
			}
		}
		protected override void WriteClassObject(object value, Type type)
		{
			bool specialType = IsAbstractType(value, ref type);

			if (specialType)
			{
				type = value.GetType();
                WriteNode(SPECIAL + type.SimpleAQName());
			}
			Setting.Serialize(type, value,
				variable =>
				{
					WriteNode(variable.VariableName);
					WriteObject(variable.GetValue(), variable.Type);
					WriteNodeClose(variable.VariableName);
				});
			if (specialType)
			{
                WriteNodeClose(SPECIAL + type.SimpleAQName());
			}
		}

        public static string Serialize(object value)
        {
            if (value == null)
                return null;
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
            XmlWriter writer = new XmlWriter();
            writer.Setting = setting;
            writer.WriteObject(value, type);
            return writer.Result;
        }
	}
	public class XmlReader : StringReader
	{
		enum EXml
		{
			NONE,
			OPEN,
			CLOSE,
			VALUE,
		}

		private EXml NextXml
		{
			get
			{
				const string STRUCT_BREAK = " \t\n\r>";

				string word;
				while (true)
				{
					word = PeekNext(STRUCT_BREAK);
					if (word == null)
					{
						return EXml.NONE;
					}
					else if (word == string.Empty)
					{
						word = PeekChar.ToString();
						if (string.IsNullOrEmpty(word))
						{
							return EXml.NONE;
						}
					}
					if (word.StartsWith("<?") ||
						word.StartsWith("<!--"))
					{
						EatEnd();
					}
					else if (word.StartsWith("</"))
					{
						EatEnd();
						return EXml.CLOSE;
					}
					else if (word.Contains(">"))
					{
						EatEnd();
					}
					else if (word.StartsWith("<"))
					{
						EatOpen();
						return EXml.OPEN;
					}
					else
					{
						return EXml.VALUE;
					}
				}
			}
		}

		public XmlReader() : this(null)
		{
		}
		public XmlReader(string content) : base(content)
		{
			WORD_BREAK = " \t\n\r<>";
		}

		public override StringTable ReadTable()
		{
			XmlNode root = ReadToNode();
			StringTable table = new StringTable();

			foreach (XmlNode value in root.First)
			{
				table.AddColumn(value.Name);
			}

			foreach (XmlNode array in root)
			{
				foreach (XmlNode value in array)
				{
					table.AddValue(value.Value);
				}
			}

			return table;
		}
		public List<XmlNode> ReadToNodes()
		{
            List<XmlNode> roots = new List<XmlNode>();
            while (!End)
                roots.Add(ReadToNode());
            return roots;
		}
		public override XmlNode ReadToNode()
		{
			if (str == null)
				throw new ArgumentNullException("read string can not be null");

            //if (Setting.AutoType)
            //    ReadType();

			Stack<XmlNode> nodes = new Stack<XmlNode>();
			XmlNode node = null;
			XmlNode parent = null;
			while (pos < len)
			{
				EXml token = NextXml;
				if (token == EXml.NONE)
					break;

				switch (token)
				{
					case EXml.OPEN:
						node = new XmlNode();
						node.Name = Next(">", false);

						if (parent == null)
						{
							parent = node;
						}
						else
						{
							if (nodes.Count > 0)
							{
								parent = nodes.Peek();
								parent.Add(node);
							}
						}

						nodes.Push(node);
						break;

					case EXml.CLOSE:
						if (nodes.Count > 0)
							node = nodes.Pop();
						else
							throw new NotImplementedException("can not have multiple root node");
						break;

					case EXml.VALUE:
						string word = Next("<", false);
                        if (node == null && parent == null && nodes.Count == 0 && End)
                        {
                            // value only
                            node = new XmlNode();
                        }
						if (word == _XML.NULL || word == _XML.NULL2)
							node.Value = null;
						else
							node.Value = _XML.UnEscape(word);
						break;

					default:
						break;
				}
			}
			return node;
		}
		public override object ReadObject(Type type)
		{
			//return ReadValue(type, EEXml.BuildRoot(ReadToNodes()));
			return ReadValue(type, ReadToNode());
		}
		private object ReadValue(Type type, XmlNode node)
		{
			if (node.Value == null)
			{
				return null;
			}

			Type nullable;
            if (type.IsValueType && type.IsNullable(out nullable))
            {
                // Nullable<struct>
                if (string.IsNullOrEmpty(node.Value) && node.ChildCount == 0)
                {
                    return null;
                }
                else
                {
					type = nullable;
                }
            }

			if (type.IsEnum)
			{
                if (string.IsNullOrEmpty(node.Value))
                    return 0;
                Type underlying = Enum.GetUnderlyingType(type);
                try
                {
                    return Convert.ChangeType(Convert.ChangeType(node.Value, underlying), type);
                }
                catch
                {
                    return Enum.Parse(type, node.Value);
                }
			}
			else if (type == typeof(char))
			{
                if (string.IsNullOrEmpty(node.Value))
                    return default(char);
				return _XML.UnEscape(node.Value)[0];
			}
			else if (type == typeof(string))
			{
				return _XML.UnEscape(node.Value);
			}
			else if (type == typeof(bool))
			{
                if (string.IsNullOrEmpty(node.Value))
                    return default(bool);
				if (node.Value == "1")
					return true;
				else if (node.Value == "0")
					return false;
				else
					return bool.Parse(node.Value);
			}
			else if (type == typeof(sbyte))
			{
                if (string.IsNullOrEmpty(node.Value))
                    return default(sbyte);
				return sbyte.Parse(node.Value);
			}
			else if (type == typeof(byte))
			{
                if (string.IsNullOrEmpty(node.Value))
                    return default(byte);
				return byte.Parse(node.Value);
			}
			else if (type == typeof(short))
			{
                if (string.IsNullOrEmpty(node.Value))
                    return default(short);
				return short.Parse(node.Value);
			}
			else if (type == typeof(ushort))
			{
                if (string.IsNullOrEmpty(node.Value))
                    return default(ushort);
				return ushort.Parse(node.Value);
			}
			else if (type == typeof(int))
			{
                if (string.IsNullOrEmpty(node.Value))
                    return default(int);
				return int.Parse(node.Value);
			}
			else if (type == typeof(uint))
			{
                if (string.IsNullOrEmpty(node.Value))
                    return default(uint);
				return uint.Parse(node.Value);
			}
			else if (type == typeof(float))
			{
                if (string.IsNullOrEmpty(node.Value))
                    return default(float);
				return float.Parse(node.Value);
			}
			else if (type == typeof(long))
			{
                if (string.IsNullOrEmpty(node.Value))
                    return default(long);
				return long.Parse(node.Value);
			}
			else if (type == typeof(ulong))
			{
                if (string.IsNullOrEmpty(node.Value))
                    return default(ulong);
				return ulong.Parse(node.Value);
			}
			else if (type == typeof(double))
			{
                if (string.IsNullOrEmpty(node.Value))
                    return default(double);
				return double.Parse(node.Value);
			}
			else if (type == typeof(DateTime))
			{
                if (string.IsNullOrEmpty(node.Value))
                    return default(DateTime);
                int timestamp;
                if (int.TryParse(node.Value, out timestamp))
                    return Utility.ToTime(timestamp);
				//return Utility.ToUnixTime(int.Parse(node.Value));
				return DateTime.Parse(node.Value);
			}
			else if (type == typeof(TimeSpan))
			{
                if (string.IsNullOrEmpty(node.Value))
                    return default(TimeSpan);
				//return new TimeSpan(long.Parse(node.Value));
				return TimeSpan.Parse(node.Value);
			}
            else if (type.IsArray || type.Is(typeof(IList)))
			{
				Type elementType = type.GetElementType();
                if (elementType == null)
                    elementType = type.GetGenericArguments()[0];
				Array array = Array.CreateInstance(elementType, node.ChildCount);
				for (int i = 0; i < node.ChildCount; i++)
					array.SetValue(ReadValue(elementType, node[i]), i);
                if (type.IsArray)
                    return array;
                IList list = Activator.CreateInstance(type) as IList;
                for (int i = 0; i < array.Length; i++)
                    list.Add(array.GetValue(i));
                return list;
			}
			else
			{
				object obj;
                //if (IsAbstractType(null, ref type))
                //{
                //    node = node.First;
                //    type = Type.GetType(node.Name);
                //}
                // 单纯的接口实现某方法而没有其它字段属性时，First.ChildCount是0
                //if (node.ChildCount == 1 && node.First.ChildCount > 0 && node.First.Name.StartsWith(XmlWriter.SPECIAL))
                if (node.ChildCount == 1 && node.First.Name.StartsWith(XmlWriter.SPECIAL))
                {
                    node = node.First;
                    type = _SERIALIZE.LoadSimpleAQName(node.Name.Substring(XmlWriter.SPECIAL.Length));
                }
				if (type.IsStatic())
				{
					obj = null;
				}
				else
				{
                    obj = Activator.CreateInstance(type);
					if (obj == null)
						throw new NotImplementedException(string.Format("can not create instance of {0}", type.FullName));
				}

                var properties = Setting.GetProperties(type);
                var fields = Setting.GetFields(type);
                for (int i = 0; i < node.ChildCount; i++)
                {
                    XmlNode child = node[i];

                    FieldInfo field = fields.FirstOrDefault(f => f.Name == child.Name);
                    if (field == null)
                    {
                        if (properties == null)
                            continue;
                        PropertyInfo property = properties.FirstOrDefault(p => p.Name == child.Name);
                        if (property != null)
                            property.SetValue(obj, ReadValue(property.PropertyType, child), null);
                    }
                    else
                    {
                        field.SetValue(obj, ReadValue(field.FieldType, child));
                    }
                }

				return obj;
			}
		}
		private void EatOpen()
		{
			Eat("<");
		}
		private void EatEnd()
		{
			Eat(">");
		}

		public static object ReadObject(Type type, XmlNode root, SerializeSetting setting)
		{
			XmlReader reader = new XmlReader();
			reader.Setting = setting;
			return reader.ReadValue(type, root);
		}
		public static object ReadObject(Type type, List<XmlNode> roots, SerializeSetting setting)
		{
			XmlReader reader = new XmlReader();
			reader.Setting = setting;
			return reader.ReadValue(type, XmlNode.CreateRoot(roots));
		}
		public static T ReadObject<T>(XmlNode root, SerializeSetting setting)
		{
			return (T)ReadObject(typeof(T), root, setting);
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
            XmlReader reader = new XmlReader(buffer);
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
