using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace EntryEngine.Serialize
{
	public class JsonWriter : StringWriter
	{
        public bool Encode = false;

        public override void WriteObject(object value, Type type)
        {
            if (value is IDictionary)
            {
                bool first = true;
                builder.Append('{');
#if HTML5
                var e = ((IDictionary)value);
                var keys = e.Keys;
                foreach (var item in keys)
                {
                    if (!first)
                        builder.Append(',');
                    WriteObject(item);
                    builder.Append(':');
                    this.WriteObject(e[item]);
                    first = false;
                }
#else
                var e = ((IDictionary)value).GetEnumerator();
                while (e.MoveNext())
                {
                    if (!first)
                        builder.Append(',');
                    //WriteString(e.Key);
                    WriteObject(e.Key);
                    builder.Append(':');
                    this.WriteObject(e.Value);
                    first = false;
                }
#endif
                builder.Append('}');
            }
            else
                base.WriteObject(value, type);
        }
		public void WriteDictionary(Dictionary<string, object> obj)
		{
			bool first = true;
			builder.Append('{');
			foreach (var e in obj)
			{
				if (!first)
					builder.Append(',');
				WriteString(e.Key);
				builder.Append(':');
				this.WriteObject(e.Value);
                first = false;
			}
			builder.Append('}');
		}
        public void WriteStringDictionary(Dictionary<string, string> obj)
        {
            bool first = true;
            builder.Append('{');
            foreach (var e in obj)
            {
                if (!first)
                    builder.Append(',');
                WriteString(e.Key);
                builder.Append(':');
                WriteString(e.Value);
                first = false;
            }
            builder.Append('}');
        }
		public override void WriteTable(StringTable table)
		{
            WriteArray(table.ToDictionaryArray(), WriteStringDictionary);
		}
		private void WriteArray<T>(T[] array, Action<T> write)
		{
			builder.Append('[');
            for (int i = 0, n = array.Length - 1; i <= n; i++)
            {
                write(array[i]);
                if (i != n)
                    builder.Append(',');
            }
			builder.Append(']');
		}
		protected override void WriteArray(IEnumerable value, Type type)
		{
			builder.Append('[');

			bool first = true;

			foreach (object obj in value)
			{
				if (!first)
				{
					builder.Append(',');
				}

				WriteObject(obj, type);

				first = false;
			}

			builder.Append(']');
		}
		protected override void WriteString(object value)
		{
			builder.Append('\"');

			char[] charArray = value.ToString().ToCharArray();
			foreach (var c in charArray)
			{
				switch (c)
				{
					case '"':
						builder.Append("\\\"");
						break;
					case '\\':
						builder.Append("\\\\");
						break;
					case '\b':
						builder.Append("\\b");
						break;
					case '\f':
						builder.Append("\\f");
						break;
					case '\n':
						builder.Append("\\n");
						break;
					case '\r':
						builder.Append("\\r");
						break;
					case '\t':
						builder.Append("\\t");
						break;
					default:
                        ushort codepoint = (ushort)c;
						if ((codepoint >= 32) && (codepoint <= 126))
						{
							builder.Append(c);
						}
						else
						{
                            if (Encode)
                            {
                                builder.Append("\\u");
                                builder.Append(ConvertString(codepoint));
                            }
                            else
                            {
                                builder.Append(c);
                            }
						}
						break;
				}
			}

			builder.Append('\"');
		}
		protected override void WriteClassObject(object value, Type type)
		{
			builder.Append('{');

			bool abs = Setting.IsAbstractType(value, ref type);
			if (abs)
			{
				type = value.GetType();
                WriteString(ABSTRACT_TYPE + type.SimpleAQName());
				builder.Append(':');
				builder.Append('{');
			}

			bool first = true;
			Setting.Serialize(type, value,
				field =>
				{
					if (!first)
					{
						builder.Append(',');
					}

					WriteString(field.VariableName);
					builder.Append(':');

					WriteObject(field.GetValue(), field.Type);

					first = false;
				});

			if (abs)
			{
				builder.Append('}');
			}
			builder.Append('}');
		}

        private static char[] string4 = new char[4];
        public static string ConvertString(ushort value)
        {
            string4[3] = IntToChar(value & 15);
            string4[2] = IntToChar((value >> 4) & 15);
            string4[1] = IntToChar((value >> 8) & 15);
            string4[0] = IntToChar(value >> 12);
            return new string(string4);
        }
        internal static char IntToChar(int value)
        {
            return (char)(value > 9 ? 'A' + (value - 10) : '0' + value);
        }
        [Obsolete("To use JsonWriter.Serialize")]
        public static string WriteJson(object value)
        {
            JsonWriter writer = new JsonWriter();
            writer.WriteObject(value);
            return writer.Result;
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
            if (value == null && type == null)
                throw new ArgumentNullException();
            if (value != null && type == null)
                type = value.GetType();
            JsonWriter writer = new JsonWriter();
            writer.Setting = setting;
            writer.WriteObject(value, type);
            return writer.Result;
        }
	}
    public class JsonReader : StringReader
    {
        enum EJson
        {
            NONE,
            /// <summary>new object push</summary>
            CURLY_OPEN,
            /// <summary>new object pop</summary>
            CURLY_CLOSE,
            /// <summary>'[' array object push</summary>
            SQUARED_OPEN,
            /// <summary>']' array object pop</summary>
            SQUARED_CLOSE,
            /// <summary>',' array object split</summary>
            COMMA,
            /// <summary>Value</summary>
            VALUE,
        }

        private EJson PeekJson
        {
            get
            {
                EJson token = NextJson;
                if (token != EJson.NONE)
                    pos--;
                return token;
            }
        }
        private EJson NextJson
        {
            get
            {
                EatWhitespace();

                if (Peek() == -1)
                    return EJson.NONE;

                char c = NextChar;
                switch (c)
                {
                    case '{':
                        return EJson.CURLY_OPEN;
                    case '}':
                        return EJson.CURLY_CLOSE;
                    case '[':
                        return EJson.SQUARED_OPEN;
                    case ']':
                        return EJson.SQUARED_CLOSE;
                    case ',':
                        return EJson.COMMA;
                    case ':':
                        return EJson.VALUE;
                    default:
                        return EJson.VALUE;
                }
            }
        }

        public JsonReader()
            : this(null)
        {
        }
        public JsonReader(string content)
            : base(content)
        {
            WORD_BREAK = " \t\n\r{}[],:\"";
        }

        public override object ReadObject(Type type)
        {
            if (type.Is(typeof(IDictionary)))
            {
                var types = type.GetGenericArguments();
                var dictionary = (IDictionary)Activator.CreateInstance(type);
                // ditch opening brace
                Read();

                while (true)
                {
                    switch (PeekJson)
                    {
                        case EJson.NONE:
                            return null;
                        case EJson.COMMA:
                            Read();
                            continue;
                        case EJson.CURLY_CLOSE:
                            Read();
                            return dictionary;
                        default:
                            object key = ReadObject(types[0]);
                            // :
                            Read();
                            // value
                            dictionary[key] = ReadObject(types[1]);
                            break;
                    }
                }
            }
            else
                return base.ReadObject(type);
        }
        protected override string ReadString()
        {
            if (PeekChar != '"')
            {
                if (ReadNextString() == "null") return null;
                throw new FormatException("字符串开头不包含'\"'");
            }
            Read();

            StringBuilder builder = new StringBuilder(64);
            while (true)
            {
                if (pos >= len) throw new FormatException("字符串结尾不包含'\"'");
                char c = NextChar;
                switch (c)
                {
                    case '"': return builder.ToString();

                    case '\\':
                        if (pos >= len)
                        {
                            builder.Append(c);
                            return builder.ToString();
                        }

                        // 转义
                        c = NextChar;
                        switch (c)
                        {
                            case '"':
                            case '\\':
                            case '/':
                                builder.Append(c);
                                break;
                            case 'b': builder.Append('\b'); break;
                            case 'f': builder.Append('\f'); break;
                            case 'n': builder.Append('\n'); break;
                            case 'r': builder.Append('\r'); break;
                            case 't': builder.Append('\t'); break;
                            case 'u':
                                // 4个字符和结尾引号
                                if (pos + 5 >= len) throw new FormatException("\\u字符串必须后面有4个字符");
                                builder.Append(ConvertString(NextChar, NextChar, NextChar, NextChar));
                                break;
                        }
                        break;

                    default: builder.Append(c); break;
                }
            }
        }
        protected override object ReadArray(Type type, Type elementType)
        {
            if (PeekChar != '[')
            {
                if (ReadNextString() == "null") return null;
                throw new FormatException("数组缺少'['");
            }
            Read();
            EatWhitespace();

            // byte[]数组加速
            if (type == typeof(byte[]))
            {
                List<byte> bytes = new List<byte>((len - pos) >> 2 + 16);
                int value;
                bool __continue = true;
                while (__continue)
                {
                    value = 0;
                    while (true)
                    {
                        if (pos >= len) throw new FormatException("数组缺少']'");
                        char c = NextChar;
                        if (c == ',' || c == ']')
                        {
                            bytes.Add((byte)value);
                            if (c == ']') __continue = false;
                            break;
                        }
                        else if (c >= '0' && c <= '9')
                        {
                            value = value * 10 + c - '0';
                        }
                        else
                            throw new FormatException("byte[]数值间不能带有其它字符");
                    }
                }
                return bytes.ToArray();
            }

            return base.ReadArray(type, elementType);
        }
        protected override void ReadArray(Type elementType, List<object> list)
        {
            while (true)
            {
                EatWhitespace();
                if (pos >= len) throw new FormatException("数组缺少']'");
                if (PeekChar == ']')
                {
                    Read();
                    break;
                }
                else if (PeekChar == ',') Read();
                list.Add(ReadObject(elementType));
            }
        }
        protected override object ReadClassObject(Type type)
        {
            EatWhitespace();

            bool isStatic = type.IsStatic();

            if (PeekChar != '{') // }
            {
                if (ReadNextString() == "null") return null;
                throw new FormatException("对象缺少'{'"); // }
            }
            Read();
            EatWhitespace();

            bool isAbstract = PeekChar == '\"' && pos < len && str[pos + 1] == ABSTRACT_CHAR;
            if (isAbstract)
            {
                // #类型SimpleName
                string absType = ReadString();
                type = _SERIALIZE.LoadSimpleAQName(absType.Substring(ABSTRACT_TYPE.Length));

                EatWhitespace();
                if (PeekChar != ':')
                    throw new FormatException("Key后缺少符号':'");
                Read();
                EatWhitespace();
                if (PeekChar != '{') // }
                {
                    if (ReadNextString() == "null") return null;
                    throw new FormatException("对象缺少'{'"); // }
                }
                Read();
            }

            object obj;
            if (isStatic) obj = null;
            else obj = Activator.CreateInstance(type);
            // {
            if (PeekChar == '}')
            {
                // 空对象
                Read();
                return obj;
            }
            Dictionary<string, IVariable> variables = Setting.GetVariablesDic(type, obj);

            while (true)
            {
                EatWhitespace();
                if (pos >= len)
                    // {
                    throw new FormatException("对象缺少'}'");
                string key = ReadString();
                EatWhitespace();
                if (PeekChar != ':')
                    throw new FormatException("Key后缺少符号':'");
                Read();
                EatWhitespace();

                object value = null;
                IVariable variable;
                if (variables.TryGetValue(key, out variable))
                {
                    value = ReadObject(variable.Type);
                    variable.SetValue(value);
                }
                else
                {
                    // 消耗掉值
                    ReadValue();
                }

                EatWhitespace();
                // 读取完一个对象
                // {
                if (PeekChar == '}')
                {
                    Read();
                    break;
                }
                // 读完一个属性
                else if (PeekChar == ',')
                {
                    Read();
                }
                else
                {
                    // 可能是key: null,
                    if (value != null)
                        // {
                        throw new FormatException("对象缺少'}' 或 属性间缺少','");
                }
            }

            if (isAbstract)
            {
                EatWhitespace();
                // { {
                if (PeekChar != '}')
                    throw new FormatException("抽象类型实例后缺少'}'");
                Read();
            }

            return obj;
        }

        public object ReadValue()
        {
            return ReadByToken(PeekJson);
        }
        private object ReadByToken(EJson token)
        {
            switch (token)
            {
                case EJson.VALUE:
                    char c = PeekChar;
                    switch (c)
                    {
                        case '\"':
                            return ReadString();

                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                        case '-':
                            return ReadNumber(NextWord);

                        default:
                            string word = NextWord;
                            switch (word)
                            {
                                case "true": return true;
                                case "false": return false;
                                case "null": return null;
                                default: throw new NotImplementedException();
                            }
                    }
                case EJson.CURLY_OPEN:
                    return ReadDictionary();
                case EJson.SQUARED_OPEN:
                    return ReadArray();
                default:
                    return null;
            }
        }
        private object ReadNumber(string number)
        {
            if (number.Contains('.'))
            {
                double parsedDouble;
                double.TryParse(number, out parsedDouble);
                return parsedDouble;
            }
            else
            {
                long parsedLong;
                long.TryParse(number, out parsedLong);
                return parsedLong;
            }
        }
        private List<object> ReadArray()
        {
            List<object> array = new List<object>();

            // [
            Read();

            while (true)
            {
                var nextToken = PeekJson;
                switch (nextToken)
                {
                    case EJson.COMMA:
                        Read();
                        continue;

                    case EJson.SQUARED_CLOSE:
                        Read();
                        return array;

                    default:
                        object value = ReadByToken(nextToken);
                        array.Add(value);
                        break;
                }
            }
        }
        public Dictionary<string, object> ReadDictionary()
        {
            Dictionary<string, object> table = new Dictionary<string, object>();

            // ditch opening brace
            Read();

            while (true)
            {
                switch (PeekJson)
                {
                    case EJson.NONE:
                        return null;
                    case EJson.COMMA:
                        Read();
                        continue;
                    case EJson.CURLY_CLOSE:
                        Read();
                        return table;
                    default:
                        // name
                        string name = ReadString();

                        // :
                        Read();

                        // value
                        table[name] = ReadValue();
                        break;
                }
            }
        }
        public override StringTable ReadTable()
        {
            List<Dictionary<string, string>> table = new List<Dictionary<string, string>>();

            var parsing = true;
            while (parsing)
            {
                EJson nextToken = PeekJson;

                switch (nextToken)
                {
                    case EJson.COMMA:
                        Read();
                        continue;

                    case EJson.SQUARED_CLOSE:
                        parsing = false;
                        break;

                    default:
                        Dictionary<string, string> dic = new Dictionary<string, string>();

                        // ditch opening brace
                        Read();

                        bool readDic = true;
                        while (readDic)
                        {
                            switch (NextJson)
                            {
                                case EJson.NONE:
                                    return null;
                                case EJson.COMMA:
                                    continue;
                                case EJson.CURLY_CLOSE:
                                    readDic = false;
                                    break;
                                default:
                                    // name
                                    string name = ReadString();
                                    if (name == null)
                                    {
                                        return null;
                                    }

                                    // :
                                    if (NextJson != EJson.VALUE)
                                    {
                                        return null;
                                    }

                                    // value
                                    char c = PeekChar;
                                    switch (c)
                                    {
                                        case '\"':
                                            dic[name] = ReadString();
                                            break;

                                        default:
                                            string word = NextWord;
                                            if (word == _XML.NULL || word == _XML.NULL2)
                                                dic[name] = null;
                                            else
                                                dic[name] = string.Format("\"{0}\"", word);
                                            break;
                                    }
                                    break;
                            }
                        }

                        table.Add(dic);
                        break;
                }
            }

            return new StringTable(table);
        }

        [Obsolete("To use JsonReader.Deserialize")]
        public static T ReadJson<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
                return default(T);
            else
                return new JsonReader(json).ReadObject<T>();
        }
        [Obsolete("To use JsonReader.Deserialize")]
        public static void ReadJson<T>(string json, out T value)
        {
            value = ReadJson<T>(json);
        }

        private static char ConvertString(char c1, char c2, char c3, char c4)
        {
            return (char)((CharToInt(c1) << 12) | (CharToInt(c2) << 8) | (CharToInt(c3) << 4) | CharToInt(c4));
        }
        internal static char CharToInt(char value)
        {
            return (char)(value <= '9' ? value - '0' :
                (value >= 'a' ? value - 'a' : value - 'A') + 10);
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
        public static JsonObject Deserialize(string buffer)
        {
            return new JsonObject(new JsonReader(buffer).ReadValue());
        }
    }
    public struct JsonObject
    {
        public object Value;

        public JsonObject this[string key]
        {
            get { return new JsonObject(((Dictionary<string, object>)Value)[key]); }
        }
        public JsonObject this[int arrayIndex]
        {
            get { return new JsonObject(((List<object>)Value)[arrayIndex]); }
        }

        public JsonObject(object value)
        {
            this.Value = value;
        }

        public bool IsObject { get { return Value is Dictionary<string, object>; } }
        public bool IsArray { get { return Value is List<object>; } }
        public bool IsLong { get { return Value is long; } }
        public bool IsDouble { get { return Value is double; } }
        public bool IsString { get { return Value is string; } }
        public bool IsNull { get { return Value == null; } }
        public bool IsBoolean { get { return Value is bool; } }
        public bool IsDateTime
        {
            get
            {
                string v = Value as string;
                if (v != null)
                {
                    if (v.IndexOf('-') == -1 && v.IndexOf(':') == -1)
                        return false;
                    DateTime datetime;
                    return DateTime.TryParse(v, out datetime);
                }
                return false;
            }
        }

        //public T GetValue<T>()
        //{
        //    return (T)Value;
        //}
        public byte GetByte()
        {
#if HTML5
            return (byte)(Value == null ? 0 : Value);
#else
            return Convert.ToByte(Value == null ? 0 : Value);
#endif
        }
        public sbyte GetSByte()
        {
#if HTML5
            return (sbyte)(Value == null ? 0 : Value);
#else
            return Convert.ToSByte(Value == null ? 0 : Value);
#endif
        }
        public bool GetBool()
        {
            return (bool)(Value == null ? false : Value);
        }
        public char GetChar()
        {
            return (char)(Value == null ? 0 : Value);
        }
        public ushort GetUInt16()
        {
#if HTML5
            return (ushort)(Value == null ? 0 : Value);
#else
            return Convert.ToUInt16(Value == null ? 0 : Value);
#endif
        }
        public short GetInt16()
        {
#if HTML5
            return (short)(Value == null ? 0 : Value);
#else
            return Convert.ToInt16(Value == null ? 0 : Value);
#endif
        }
        public uint GetUInt32()
        {
#if HTML5
            return (uint)(Value == null ? 0 : Value);
#else
            return Convert.ToUInt32(Value == null ? 0 : Value);
#endif
        }
        public int GetInt32()
        {
#if HTML5
            return (int)(Value == null ? 0 : Value);
#else
            return Convert.ToInt32(Value == null ? 0 : Value);
#endif
        }
        public float GetSingle()
        {
#if HTML5
            return (float)(Value == null ? 0 : Value);
#else
            return Convert.ToSingle(Value == null ? 0 : Value);
#endif
        }
        public ulong GetUInt64()
        {
#if HTML5
            return (ulong)(Value == null ? 0 : Value);
#else
            return Convert.ToUInt64(Value == null ? 0 : Value);
#endif
        }
        public long GetInt64()
        {
#if HTML5
            return (long)(Value == null ? 0 : Value);
#else
            return Convert.ToInt64(Value == null ? 0 : Value);
#endif
        }
        public string GetString()
        {
            return (string)Value;
        }
        public T[] GetArray<T>()
        {
            List<object> array = (List<object>)Value;
            T[] result = new T[array.Count];
            for (int i = 0; i < result.Length; i++)
                result[i] = (T)array[i];
            return result;
        }
        public DateTime GetDateTime()
        {
            return DateTime.Parse((string)Value);
        }
        public DateTime GetDateTimeFromTimeStamp()
        {
            return Utility.ToTime(GetInt64());
        }
        public DateTime GetDateTimeFromUnixTimeStamp()
        {
            return Utility.ToUnixTime(GetInt32());
        }

        class JsonClass : IEquatable<JsonClass>
        {
            public string ClassName;
            public List<JsonClassField> Fields = new List<JsonClassField>();

            public bool Equals(JsonClass other)
            {
                int equals = 0;
                foreach (var mine in Fields)
                {
                    foreach (var their in other.Fields)
                    {
                        if (mine.Type == their.Type &&
                            mine.ArrayDimension == their.ArrayDimension &&
                            mine.Key == their.Key &&
                            (mine.Type != JsonFieldType.Class || mine.InnerObject.Equals(their.InnerObject)))
                        {
                            equals++;
                        }
                    }
                }
                float eq = equals;
                return (eq / Fields.Count) * (eq / other.Fields.Count) >= 0.666f;
            }
            public override bool Equals(object obj)
            {
                var other = obj as JsonClass;
                if (other == null) return false;
                return Equals(other);
            }
        }
        enum JsonFieldType
        {
            Null,
            Class,
            String,
            Double,
            Long,
            Boolean,
            DateTime,
        }
        class JsonClassField : IEquatable<JsonClassField>
        {
            public string Key;
            public JsonFieldType Type;
            public JsonClass InnerObject;
            public int ArrayDimension;

            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("\"{0}\":", Key);
                for (int i = 0; i < ArrayDimension; i++)
                    builder.Append('[');
                switch (Type)
                {
                    case JsonFieldType.Class:
                        builder.Append("{ }");
                        break;
                    case JsonFieldType.Double:
                    case JsonFieldType.Long:
                    case JsonFieldType.Boolean:
                        builder.Append("Value");
                        break;

                    case JsonFieldType.String:
                    case JsonFieldType.DateTime:
                        builder.Append("\"Value\"");
                        break;
                }
                for (int i = 0; i < ArrayDimension; i++)
                    builder.Append(']');
                return builder.ToString();
            }
            public bool Equals(JsonClassField other)
            {
                return Key == other.Key;
            }
            public override bool Equals(object obj)
            {
                var other = obj as JsonClassField;
                if (other == null) return false;
                return Equals(other);
            }
            public override int GetHashCode()
            {
                return Key.GetHashCode();
            }
        }

        /// <summary>转换成类型代码，免得自己写实体类</summary>
        public string ToClassCode(string rootClassName, bool isProperty)
        {
            JsonObject obj = this;
            while (obj.IsArray)
                obj = obj[0];

            JsonClass _class = new JsonClass();
            _class.ClassName = rootClassName;
            JsonObjectVisit(_class, obj);

            StringBuilder builder = new StringBuilder();
            ToClassCode(isProperty, _class, builder);
            return builder.ToString();
        }
        static void ToClassCode(bool isProperty, JsonClass _class, StringBuilder builder)
        {
            builder.AppendLine("public class {0}", _class.ClassName);
            builder.AppendBlock(() =>
            {
                foreach (var item in _class.Fields)
                {
                    if (item.Type == JsonFieldType.Class)
                        ToClassCode(isProperty, item.InnerObject, builder);
                    builder.Append("public ");
                    switch (item.Type)
                    {
                        case JsonFieldType.Class: builder.Append(item.InnerObject.ClassName); break;
                        case JsonFieldType.String: builder.Append("string"); break;
                        case JsonFieldType.Double: builder.Append("double"); break;
                        case JsonFieldType.Long: builder.Append("long"); break;
                        case JsonFieldType.Boolean: builder.Append("bool"); break;
                        case JsonFieldType.DateTime: builder.Append("DateTime"); break;
                    }
                    for (int i = 0; i < item.ArrayDimension; i++)
                        builder.Append("[]");
                    builder.Append(' ');
                    builder.Append(item.Key);
                    if (isProperty)
                        builder.AppendLine(" { get; set; }");
                    else
                        builder.AppendLine(";");
                }
            });
        }
        static void JsonObjectVisit(JsonClass _class, JsonObject jo)
        {
            if (!jo.IsObject) return;

            int exists;
            JsonObject next = new JsonObject();
            List<object> array;
            Dictionary<string, object> obj = (Dictionary<string, object>)jo.Value;
            foreach (var item in obj)
            {
                if (!_SERIALIZE.IsVariableName(item.Key))
                {
                    _LOG.Warning("忽略字段[{0}]：字段名不合法", item.Key);
                    continue;
                }

                JsonClassField field = new JsonClassField();
                field.Key = item.Key;

                next.Value = item.Value;

                // 已有字段
                exists = _class.Fields.IndexOf(field);
                if (exists != -1)
                {
                    // 两个相同类型，前一个解析为long，后面其实还有double，总体采用double类型
                    if (_class.Fields[exists].Type == JsonFieldType.Long && next.IsDouble)
                        _class.Fields[exists].Type = JsonFieldType.Double;
                    else if (_class.Fields[exists].Type == JsonFieldType.Class)
                        JsonObjectVisit(_class.Fields[exists].InnerObject, next);
                    continue;
                }
                _class.Fields.Add(field);

                while (true)
                {
                    if (next.IsObject)
                    {
                        field.Type = JsonFieldType.Class;
                        field.InnerObject = new JsonClass();
                        field.InnerObject.ClassName = "__" + field.Key;
                        if (_class.ClassName == field.InnerObject.ClassName) field.InnerObject.ClassName += "2";
                        JsonObjectVisit(field.InnerObject, new JsonObject(item.Value));
                        if (field.InnerObject.Fields.Count == 0)
                        {
                            _LOG.Warning("忽略字段[{0}]：类型没有字段", item.Key);
                            _class.Fields.Remove(field);
                            break;
                        }
                    }
                    else if (next.IsArray)
                    {
                        while (true)
                        {
                            field.ArrayDimension++;
                            array = (List<object>)next.Value;
                            if (array.Count == 0)
                            {
                                // 未知数组类型，则忽略掉该字段
                                _LOG.Warning("忽略字段[{0}]：数组长度为0，未知数组类型", item.Key);
                                field.Type = JsonFieldType.Class;
                                _class.Fields.Remove(field);
                                break;
                            }
                            next.Value = array[0];
                            if (next.IsArray)
                                continue;
                            else if (next.IsObject)
                            {
                                field.Type = JsonFieldType.Class;
                                field.InnerObject = new JsonClass();
                                // 字段名与外包类型同名
                                field.InnerObject.ClassName = "__" + field.Key;
                                if (_class.ClassName == field.InnerObject.ClassName) field.InnerObject.ClassName += "2";
                                // 数组里的类型字段保持一致
                                for (int i = 0; i < array.Count; i++)
                                    JsonObjectVisit(field.InnerObject, new JsonObject(array[i]));
                            }
                            break;
                        }
                        // object类型数组
                        if (field.Type == JsonFieldType.Class)
                            break;
                        else
                            // 标准类型数组
                            continue;
                    }
                    else if (next.IsBoolean)
                        field.Type = JsonFieldType.Boolean;
                    else if (next.IsLong)
                        field.Type = JsonFieldType.Long;
                    else if (next.IsDouble)
                        field.Type = JsonFieldType.Double;
                    else if (next.IsDateTime)
                        field.Type = JsonFieldType.DateTime;
                    else
                        // string | null
                        field.Type = JsonFieldType.String;

                    break;
                }
            }
        }
        public override string ToString()
        {
            if (Value == null) return string.Empty;
            else if (Value is List<object>) return string.Format("List[{0}]", ((List<object>)Value).Count);
            else if (Value is Dictionary<string, object>) return "Object";
            else return Value.ToString();
        }
    }
}
