using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EntryEngine.Serialize
{
	public interface IWriter
	{
		void WriteObject(object value, Type type);
		object Output();
	}
	public interface IReader
	{
		object ReadObject(Type type);
	}
	public interface ISerializeFilter
	{
		bool SkipField(FieldInfo field);
		bool SkipProperty(PropertyInfo property);
	}
    public interface IVariable
    {
        /// <summary>Value所在实例，基元类型可能是其本身</summary>
		object Instance { get; }
        /// <summary>GetValue的类型</summary>
		Type Type { get; }
        /// <summary>类型中的字段名，基元类型为空字符串</summary>
		string VariableName { get; }
        MemberInfo MemberInfo { get; }
        object GetValue();
        void SetValue(object value);
    }
    public struct VariableValue : IVariable
    {
		private object instance;
        private Type type;
		public object Instance
		{
			get { return instance; }
		}
		public Type Type
		{
            get { return type; }
		}
		public string VariableName
		{
			get { return ""; }
		}
        public MemberInfo MemberInfo
        {
            get { return null; }
        }
        public VariableValue(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            this.instance = null;
            this.type = type;
        }
        public VariableValue(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");
			this.instance = value;
            this.type = value.GetType();
        }
        public object GetValue()
        {
            return instance;
        }
        public void SetValue(object value)
        {
            instance = value;
        }
    }
    /// <summary>表达式变量，例如array[0]</summary>
    public class VariableExpression : IVariable
    {
        private bool hasInstance;
        private object _Instance;
        private Type _Type;
        private string _Key;
        private Func<object, string, object> Getter;
        private Action<object, string, object> Setter;

        public object Instance
        {
            get { return hasInstance ? _Instance : GetValue(); }
        }
        public Type Type
        {
            get { return _Type; }
        }
        public string VariableName
        {
            get { return _Key; }
        }
        public MemberInfo MemberInfo
        {
            get { return null; }
        }
        public object GetValue()
        {
            return Getter(_Instance, _Key);
        }
        public void SetValue(object value)
        {
            Setter(_Instance, _Key, value);
        }

        /// <summary>自定义构造一个表达式变量</summary>
        /// <param name="instance">例如数组，字典</param>
        /// <param name="key">例如数组的索引，字典的Key</param>
        /// <param name="getter">value (instance, key)</param>
        /// <param name="setter">(instance, key, value)</param>
        public VariableExpression(object instance,
            Type type,
            string key,
            Func<object, string, object> getter,
            Action<object, string, object> setter)
        {
            hasInstance = true;
            this._Instance = instance;
            this._Type = type;
            this._Key = key;
            this.Getter = getter;
            this.Setter = setter;
        }
        public VariableExpression(Type type, Func<object> getter, Action<object> setter)
        {
            hasInstance = false;
            this._Key = string.Empty;
            this._Type = type;
            this.Getter = (arg1, arg2) => getter();
            this.Setter = (arg1, arg2, value) => setter(value);
        }
    }
    public class VariableObject : IVariable
    {
        private string variableName;
        internal FieldInfo fieldInfo;
        internal PropertyInfo propertyInfo;
		private Type staticType;
		private object instance;

		public object Instance
		{
			get { return instance; }
			set { instance = value; }
		}
        public Type Type
        {
			get
			{
				if (fieldInfo == null)
					return propertyInfo.PropertyType;
				else
					return fieldInfo.FieldType;
			}
        }
		public Type InstanceType
		{
			get
			{
				Type type;
				if (instance == null)
				{
					if (staticType == null)
					{
						throw new ArgumentNullException("staticType");
					}
					else
					{
						type = staticType;
					}
				}
				else
				{
					type = instance.GetType();
				}
				return type;
			}
		}
        public string VariableName
        {
            get { return variableName; }
            set
            {
                variableName = value;
                if (string.IsNullOrEmpty(value))
                {
                    fieldInfo = null;
                    propertyInfo = null;
                    return;
                }

                SetInfo(out fieldInfo, out propertyInfo);
                if (!HasInfo)
                {
                    throw new ArgumentNullException("variable info can not be null!");
                }
            }
        }
        public MemberInfo MemberInfo
        {
            get
            {
                if (fieldInfo == null)
                    return propertyInfo;
                else
                    return fieldInfo;
            }
        }
        private bool HasInfo
        {
            get { return fieldInfo != null || propertyInfo != null; }
        }

        public VariableObject()
        {
        }
        public VariableObject(object instance, string fieldName)
        {
			this.instance = instance;
            this.VariableName = fieldName;
        }
		public VariableObject(Type type, string fieldName)
		{
			this.staticType = type;
			this.VariableName = fieldName;
		}
        internal VariableObject(object instance, PropertyInfo property)
        {
            this.instance = instance;
            this.propertyInfo = property;
            this.variableName = property.Name;
        }
        internal VariableObject(object instance, FieldInfo field)
        {
            this.instance = instance;
            this.fieldInfo = field;
            this.variableName = field.Name;
        }
        internal VariableObject(Type staticType, PropertyInfo property)
        {
            this.staticType = staticType;
            this.propertyInfo = property;
            this.variableName = property.Name;
        }
        internal VariableObject(Type staticType, FieldInfo field)
        {
            this.staticType = staticType;
            this.fieldInfo = field;
            this.variableName = field.Name;
        }

        protected virtual void SetInfo(out FieldInfo field, out PropertyInfo property)
        {
			Type type = InstanceType;
            field = type.GetField(variableName);
            if (field == null)
            {
                property = type.GetProperty(variableName);
            }
            else
            {
                property = null;
            }
        }
        public object GetValue()
        {
            if (fieldInfo == null)
				return propertyInfo.GetValue(instance, null);
            else
				return fieldInfo.GetValue(instance);
        }
        public void SetValue(object value)
        {
            if (fieldInfo == null)
				propertyInfo.SetValue(instance, value, null);
            else
				fieldInfo.SetValue(instance, value);
        }
    }

	public struct EEKeyValuePair<T, U>
	{
		public T Key;
		public U Value;

		public EEKeyValuePair(T t, U u)
		{
			this.Key = t;
			this.Value = u;
		}
	}
	public class SerializeValidatorDefault : ISerializeFilter
	{
		public bool SkipField(FieldInfo field)
		{
			return (field.IsInitOnly || field.IsNotSerialized);
		}
		public bool SkipProperty(PropertyInfo property)
		{
            // todo: Unity导出的IOS项目，property.CanWrite一直返回false
			// get set
			if (!(property.CanRead && property.CanWrite))
				return true;
			// index
			if (property.GetIndexParameters().Length > 0)
				return true;
			// abstract
			MethodInfo get = property.GetGetMethod();
			if (get == null || get.IsAbstract)
				return true;
			// NonSerialized property
			object[] nonSerialized = property.GetCustomAttributes(true);
			if (nonSerialized.Length > 0 &&
				nonSerialized.Any(IsNonSerializeProperty))
				return true;

			return false;
		}
		public static bool IsNonSerializeProperty(object non)
		{
			return non as ANonSerializedP != null;
		}
	}
	public class SerializeValidatorReadonly : ISerializeFilter
	{
		public bool SkipField(FieldInfo field)
		{
			return (field.IsInitOnly || field.IsNotSerialized);
		}
		public bool SkipProperty(PropertyInfo property)
		{
			// get
			if (!property.CanRead)
				return true;
			// index
			if (property.GetIndexParameters().Length > 0)
				return true;
			// abstract
			MethodInfo get = property.GetGetMethod();
			if (get == null || get.IsAbstract)
				return true;
			// NonSerialized property
			object[] nonSerialized = property.GetCustomAttributes(true);
			if (nonSerialized.Length > 0 &&
				nonSerialized.Any(SerializeValidatorDefault.IsNonSerializeProperty))
				return true;

			return false;
		}
	}
    /// <summary>
    /// 反射GetField(string name)被调用后，接着调用GetFields()
    /// 类型的Field顺序将被打乱，GetField的字段将被提到第一位
    /// 所以应尽量使用此类型的GetProperties和GetFields获取所有成员再使用FirstOrDefault查找
    /// 此时可以使用默认实例SettingSerializeAll
    /// </summary>
    [Code(ECode.Attention)]
	public struct SerializeSetting
	{
		private static SerializeSetting defaultSetting = 
			new SerializeSetting()
			{
				Filter = new SerializeValidatorDefault(),
			};
		/// <summary>
		/// 用于所有序列化的默认设置
		/// </summary>
		public static SerializeSetting DefaultSetting
		{
			get { return defaultSetting; }
			set { defaultSetting = value; }
		}
        public static SerializeSetting DefaultSerializeProperty
        {
            get
            {
                SerializeSetting _default = DefaultSetting;
                _default.Property = true;
                return _default;
            }
        }
        public static SerializeSetting DefaultSerializeStatic
        {
            get
            {
                SerializeSetting _default = DefaultSetting;
                _default.Static = true;
                return _default;
            }
        }
        public static SerializeSetting DefaultSerializeAll
        {
            get
            {
                SerializeSetting _default = DefaultSetting;
                _default.Static = true;
                _default.Property = true;
                return _default;
            }
        }

		/// <summary>
		/// 序列化静态变量，只用于首个类型
		/// <value>True: 静态变量</value>
		/// <value>False: 实例变量</value>
		/// </summary>
		public bool Static;
		/// <summary>
		/// 序列化属性
		/// </summary>
		public bool Property;
        public ESerializeType SerializeType;
		public ISerializeFilter Filter;
        

		public BindingFlags BuildingFlags
		{
			get
			{
				if (Static)
				{
					return BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
				}
				else
				{
					return BindingFlags.Public | BindingFlags.Instance;
				}
			}
		}

        public bool IsAbstractType(object value, ref Type type)
        {
            Type temp;
            if (type.IsNullable(out temp))
            {
                type = temp;
                return false;
            }

            // 不序列化类型，也不修改类型
            if (SerializeType == ESerializeType.None) return false;

            // 修改类型
            if (value != null)
            {
                temp = value.GetType();
                if (temp != type)
                {
                    type = temp;
                    return SerializeType == ESerializeType.SerializeType;
                }
                else
                    return false;
            }

            if (SerializeType == ESerializeType.SerializeType)
                // 对于抽象类和接口，需要序列化类型
                return (type.IsAbstract && !type.IsSealed);
            else
                return false;
        }
		public void SerializeField(Type type, Action<FieldInfo> func)
		{
			FieldInfo[] fields = type.GetFields(BuildingFlags);
			foreach (FieldInfo field in fields)
			{
				if (Filter != null && Filter.SkipField(field))
					continue;
				func(field);
			}
		}
		public void SerializeProperty(Type type, Action<PropertyInfo> func)
		{
			if (!Property)
			{
				throw new InvalidOperationException("setting can not serialize property");
			}

			PropertyInfo[] properties = type.GetProperties(BuildingFlags);
			foreach (PropertyInfo property in properties)
			{
				if (Filter != null && Filter.SkipProperty(property))
					continue;
				func(property);
			}
		}
        /// <summary>使用GetVariables，静态相关的逻辑需要测试</summary>
        [Code(ECode.MayBeReform | ECode.BeNotTest)]
        public void Serialize(Type type, object instance, Action<VariableObject> func)
		{
            List<VariableObject> variables = new List<VariableObject>();

			BindingFlags flag = BuildingFlags;
			bool staticFlag = Static;
			Static = false;

			FieldInfo[] fields = type.GetFields(flag);
			foreach (FieldInfo field in fields)
			{
				if (Filter != null && Filter.SkipField(field))
					continue;
				if (instance == null)
					variables.Add(new VariableObject(type, field.Name));
				else
					variables.Add(new VariableObject(instance, field.Name));
			}

			if (Property)
			{
				PropertyInfo[] properties = type.GetProperties(flag);
				foreach (PropertyInfo property in properties)
				{
					if (Filter != null && Filter.SkipProperty(property))
						continue;
					if (instance == null)
						variables.Add(new VariableObject(type, property.Name));
					else
						variables.Add(new VariableObject(instance, property.Name));
				}
			}

			for (int i = 0; i < variables.Count; i++)
			{
				func(variables[i]);
			}

			Static = staticFlag;
		}
        public IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            if (!Property)
                return null;
            PropertyInfo[] properties = type.GetProperties(BuildingFlags);
            if (Filter == null)
                return properties;
            var filter = Filter;
            return properties.Where(p => !filter.SkipProperty(p));
        }
        public Dictionary<string, PropertyInfo> GetPropertiesDic(Type type)
        {
            Dictionary<string, PropertyInfo> dic = new Dictionary<string, PropertyInfo>();
            if (!Property)
                return dic;
            PropertyInfo[] properties = type.GetProperties(BuildingFlags);
            if (Filter == null)
            {
                foreach (var item in properties)
                    dic.Add(item.Name, item);
                return dic;
            }
            var filter = Filter;
            foreach (var item in properties.Where(p => !filter.SkipProperty(p)))
                dic.Add(item.Name, item);
            return dic;
        }
        public IEnumerable<FieldInfo> GetFields(Type type)
        {
            FieldInfo[] fields = type.GetFields(BuildingFlags);
            if (Filter == null)
                return fields;
            var filter = Filter;
            return fields.Where(f => !filter.SkipField(f));
        }
        public Dictionary<string, FieldInfo> GetFieldsDic(Type type)
        {
            FieldInfo[] fields = type.GetFields(BuildingFlags);
            Dictionary<string, FieldInfo> dic = new Dictionary<string, FieldInfo>();
            if (Filter == null)
            {
                foreach (var item in fields)
                    dic.Add(item.Name, item);
                return dic;
            }
            var filter = Filter;
            foreach (var item in fields.Where(f => !filter.SkipField(f)))
                dic.Add(item.Name, item);
            return dic;
        }
        public IEnumerable<IVariable> GetVariables(Type type, object instance)
        {
            foreach (var field in GetFields(type))
            {
                if (instance == null)
                    yield return new VariableObject(type, field);
                else
                    yield return new VariableObject(instance, field);
            }
            
            if (Property)
            {
                foreach (PropertyInfo property in GetProperties(type))
                {
                    if (instance == null)
                        yield return new VariableObject(type, property);
                    else
                        yield return new VariableObject(instance, property);
                }
            }
        }
        public Dictionary<string, IVariable> GetVariablesDic(Type type, object instance)
        {
            Dictionary<string, IVariable> dic = new Dictionary<string, IVariable>();

            foreach (var field in GetFields(type))
            {
                if (instance == null)
                    dic.Add(field.Name, new VariableObject(type, field));
                else
                    dic.Add(field.Name, new VariableObject(instance, field));
            }

            if (Property)
            {
                foreach (PropertyInfo property in GetProperties(type))
                {
                    if (instance == null)
                        dic.Add(property.Name, new VariableObject(type, property));
                    else
                        dic.Add(property.Name, new VariableObject(instance, property));
                }
            }

            return dic;
        }
	}
    /// <summary>
    /// 序列化时会遇到抽象类型或object类型的字段
    /// 若不记录当时的类型，反序列化时将无法正确还原序列化时的对象
    /// </summary>
    public enum ESerializeType
    {
        /// <summary>
        /// <para>序列化类型</para>
        /// <para>序列化子类型字段</para>
        /// <para>使用场景：字段是父类型，对象是子类型时，例如字段类型是object</para>
        /// <para>多用于前后端都是C#且使用EntryEngine的情况，否则解析不了子类型</para>
        /// </summary>
        SerializeType,
        /// <summary>
        /// <para>不序列化类型</para>
        /// <para>序列化子类型字段</para>
        /// <para>使用场景：字段是父类型，对象是子类型时，例如字段类型是object</para>
        /// <para>多用于后端接口使用，直接将子类型字段发送给前端，不用考虑自己反序列化</para>
        /// </summary>
        Child,
        /// <summary>
        /// <para>不序列化类型</para>
        /// <para>不序列化子类型字段</para>
        /// <para>使用场景：数据库字段为string[]，生成数据库表对应子类型字段类型为string，用Json字符串存储</para>
        /// <para>此时对于后端接口传输时，应用父类型的原始类型string[]做传输</para>
        /// </summary>
        None,
    }
	public abstract class Serializable
	{
        internal const string ABSTRACT_TYPE = "#";
        internal const char ABSTRACT_CHAR = '#';
		public SerializeSetting Setting = SerializeSetting.DefaultSetting;
	}
	public abstract class StringWriter : Serializable, IWriter
	{
		protected StringBuilder builder = new StringBuilder();
		protected Type type;

		protected bool IsFirst
		{
			get { return type == null; }
		}
		public string Result
		{
			get { return builder.ToString(); }
		}

        public void Reset()
        {
            int len = builder.Length;
            if (len > 0)
                builder.Remove(0, len);
        }
		public virtual void WriteTable(StringTable table)
		{
			throw new NotImplementedException();
		}
        public void WriteObject(object value)
        {
            if (value == null)
                return;
            WriteObject(value, value.GetType());
        }
		public virtual void WriteObject(object value, Type type)
		{
			if (IsFirst)
			{
				//IsAbstractType(value, ref type);
				this.type = type;
			}

			if (value == null && (type != this.type || !type.IsStatic()))
			{
				if (WriteNull(type))
				{
					return;
				}
			}
			if (value is Enum)
			{
                if (!type.IsEnum)
                    type = value.GetType();
				WriteEnum(value, type);
			}
			else if (value is bool)
			{
				WriteBool((bool)value);
			}
			else if (value is bool
				|| value is sbyte
				|| value is byte
				|| value is short
				|| value is ushort
				|| value is int
				|| value is uint
				|| value is float
				|| value is long
				|| value is ulong
				|| value is double
#if !HTML5
                || value is decimal
#endif
                )
			{
				WriteNumber(value);
			}
			else if (value is char || value is string)
			{
				WriteString(value.ToString());
			}
			else if (value is TimeSpan)
			{
				WriteTimeSpan((TimeSpan)value);
			}
			else if (value is DateTime)
			{
				WriteDateTime((DateTime)value);
			}
            else if (type.IsArray || value is IEnumerable)
            {
                Type childType;
                if (type.IsArray)
                {
                    childType = type.GetElementType();
                    if (childType == null)
                        throw new NotSupportedException("不支持非泛型非数组的IEnumerable类型");
                }
                else
                {
                    childType = type.GetInterface("IEnumerable`1").GetGenericArguments()[0];
                }
                WriteArray((IEnumerable)value, childType);
            }
            else
            {
                WriteClassObject(value, type);
            }
		}
		protected virtual bool WriteNull(Type type)
		{
			builder.Append(_XML.NULL);
			return true;
		}
		protected void WriteEnum(object value, Type type)
		{
            //builder.Append(Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType())));
            builder.Append(Convert.ChangeType(value, Enum.GetUnderlyingType(type)));
		}
		protected virtual void WriteBool(bool value)
		{
            if (value)
			    builder.Append("true");
            else
                builder.Append("false");
		}
		protected virtual void WriteNumber(object value)
		{
			builder.Append(value.ToString());
		}
		protected virtual void WriteString(object value)
		{
			builder.Append(value.ToString());
		}
		protected void WriteTimeSpan(TimeSpan value)
		{
			WriteNumber(value.Ticks);
		}
		protected void WriteDateTime(DateTime value)
		{
			//WriteNumber(Utility.ToUnixTimestamp(value));
            if (value.Ticks == 0)
                WriteString("");
            else
			    WriteString(value.ToString("yyyy-MM-dd HH:mm:ss"));
		}
		protected virtual void WriteArray(IEnumerable value, Type type)
		{
			foreach (object obj in value)
			{
				WriteObject(obj, type);
			}
		}
		protected abstract void WriteClassObject(object value, Type type);

		public object Output()
		{
            //if (Setting.AutoType)
            //{
            //    WriteType();
            //}
			return builder.ToString();
		}
	}
	public abstract class StringReader : Serializable, IReader
	{
		public string WHITE_SPACE = " \t\n\r";
		public string WORD_BREAK;

		protected string str;
		protected int pos;
		protected int len;

		public bool End
		{
			get { return pos >= len; }
		}
		protected char PeekChar
		{
			get { return (char)Peek(); }
		}
		protected char NextChar
		{
			get { return (char)Read(); }
		}
		protected string NextWord
		{
			get { return Next(WORD_BREAK); }
		}
		protected string PeekNextWord
		{
			get { return PeekNext(WORD_BREAK); }
		}
		protected string NextLine
		{
			get { return Next("\n"); }
		}
		protected string PeekNextLine
		{
			get { return PeekNext("\n"); }
		}
		protected string NextEnd
		{
			get { return Next(";"); }
		}
		protected string PeekNextEnd
		{
			get { return PeekNext(";"); }
		}

		protected StringReader()
		{
            this.Setting = SerializeSetting.DefaultSerializeAll;
		}
		public StringReader(string content) : this()
		{
			Input(content);
		}

        protected virtual string UnEscapeString(string str)
        {
            return str;
        }
        protected virtual string ReadNextString()
        {
            return NextWord;
        }
        protected string PeekNextString()
        {
            int temp = pos;
            string result = ReadNextString();
            pos = temp;
            return result;
        }
        public virtual object ReadObject(Type type)
        {
            if (pos >= len) return null;

            EatWhitespace();

            Type nullable;
            if (type.IsValueType && type.IsNullable(out nullable))
            {
                // Nullable<struct>
                return ReadNullable(type, nullable);
            }

            if (type.IsEnum)
            {
                Type underlying = Enum.GetUnderlyingType(type);
                return ReadEnum(type, underlying);
            }
            else if (type == typeof(char))
            {
                return ReadChar();
            }
            else if (type == typeof(string))
            {
                return ReadString();
            }
            else if (type == typeof(bool))
            {
                return ReadBool();
            }
            else if (type == typeof(sbyte)
                || type == typeof(byte)
                || type == typeof(short)
                || type == typeof(ushort)
                || type == typeof(int)
                || type == typeof(uint)
                || type == typeof(float)
                || type == typeof(long)
                || type == typeof(ulong)
                || type == typeof(double)
                )
            {
                return ReadNumber(type);
            }
            else if (type == typeof(TimeSpan))
            {
                return ReadTimeSpan();
            }
            else if (type == typeof(DateTime))
            {
                return ReadDateTime();
            }

            bool isList = !type.IsArray && type.Is(typeof(IList));
            if (type.IsArray || isList)
            {
                Type elementType;
                if (isList)
                    elementType = type.GetGenericArguments()[0];
                else
                    elementType = type.GetElementType();
                return ReadArray(type, elementType);
            }

            return ReadClassObject(type);
        }
        protected virtual object ReadNullable(Type type, Type nullableType)
        {
            string word = PeekNextString();
            if (string.IsNullOrEmpty(word) || word == "null")
            {
                ReadNextString();
                return null;
            }
            else
            {
                return ReadObject(nullableType);
            }
        }
        protected virtual object ReadEnum(Type type, Type underlyingType)
        {
            string read = PeekNextString();
            if (string.IsNullOrEmpty(read))
            {
                ReadNextString();
                var array = Enum.GetValues(type);
                if (array.Length > 0) return array.GetValue(0);
            }
            return Enum.Parse(type, ReadNumber(underlyingType).ToString());
        }
        protected virtual char ReadChar()
        {
            return ReadString()[0];
        }
        protected virtual string ReadString()
        {
            return ReadNextString();
        }
        protected virtual bool ReadBool()
        {
            if (pos >= len) return false;
            string result = ReadNextString();
            if (string.IsNullOrEmpty(result)) return false;
            if (result.Length == 1)
            {
                char c = result[0];
                if (c == '1') return true;
                else return false;
            }
            else
            {
                return bool.Parse(result);
            }
        }
        protected virtual object ReadNumber(Type type)
        {
            string value = ReadNextString();

            if (string.IsNullOrEmpty(value)) return _SERIALIZE.ParseNumber(type, null);

            char c = value[0];
            switch (c)
            {
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
                case '.':
                    return _SERIALIZE.ParseNumber(type, value);

                default: throw new FormatException("错误的数字格式");
            }
        }
        protected virtual TimeSpan ReadTimeSpan()
        {
            return new TimeSpan((long)ReadNumber(typeof(long)));
        }
        protected virtual DateTime ReadDateTime()
        {
            int temp = pos;
            string strValue = null;
            try
            {
                strValue = ReadString();
            }
            catch
            {
                pos = temp;
                long timestamp = (long)ReadNumber(typeof(long));
                return Utility.ToTime(timestamp);
            }
            if (string.IsNullOrEmpty(strValue))
                return default(DateTime);
            return DateTime.Parse(strValue);
        }
        protected virtual object ReadArray(Type type, Type elementType)
        {
            List<object> objects = new List<object>((len - pos) >> 2 + 16);
            ReadArray(elementType, objects);
            int count = objects.Count;
            if (type.IsArray)
            {
                Array array = Array.CreateInstance(elementType, count);
                for (int i = 0; i < count; i++)
                    array.SetValue(objects[i], i);
                return array;
            }
            else
            {
                IList list = Activator.CreateInstance(type, count) as IList;
                for (int i = 0; i < count; i++)
                    list.Add(objects[i]);
                return list;
            }
        }
        protected virtual void ReadArray(Type elementType, List<object> list)
        {
            while (pos < len)
            {
                list.Add(ReadObject(elementType));
            }
        }
        protected virtual object ReadClassObject(Type type)
        {
            throw new NotImplementedException();
        }
		public virtual StringTable ReadTable()
		{
			throw new NotImplementedException();
		}
		public void Input(object content)
		{
			if (content == null)
				return;
			Input(content.ToString());
		}
		public void Input(string content)
		{
			if (content == null)
				return;
			this.str = content;
			this.len = content.Length;
            this.pos = 0;
		}
        public void Peek(int index)
        {
            if (index < 0 || index >= len)
                throw new ArgumentOutOfRangeException("index");
            this.pos = index;
        }
		protected int Peek()
		{
			if (pos >= len)
				return -1;
			return (int)str[pos];
		}
		protected int Read()
		{
			if (pos >= len)
				return -1;
			return (int)str[pos++];
		}
		protected string ReadLine()
		{
			return NextLine;
		}
		protected string Eat(string filter)
		{
			EatWhitespace();
			int start = pos;
			string next = Next(filter);
			if (pos < len)
				pos++;
			return next;
		}
        protected string Next(string filter)
        {
            return Next(filter, true);
        }
		protected string Next(string filter, bool eatWhitespace)
		{
			if (eatWhitespace)
			{
				if (WHITE_SPACE.Contains(filter))
					EatWhitespace(filter);
				else
					EatWhitespace();
			}

			if (pos == len)
				return null;

			int start = pos;
			while (filter.IndexOf(PeekChar) == -1)
			{
				pos++;
				if (pos == len)
				{
					break;
				}
			}

			return str.Substring(start, pos - start);
		}
		protected string PeekNext(string filter)
		{
			int start = pos;
			string next = Next(filter);
			pos = start;
			return next;
		}
        protected void EatWhitespace()
        {
            EatWhitespace(null);
        }
		protected void EatWhitespace(string filter)
		{
			if (filter == null)
				filter = string.Empty;
			while (!filter.Contains(PeekChar) && WHITE_SPACE.IndexOf(PeekChar) != -1)
			{
				Read();

				if (Peek() == -1)
				{
					break;
				}
			}
		}
	}
    /// <summary>像流一样的形式一点点解析字符串</summary>
    public class StringStreamReader
    {
        /// <summary>被视为空格的字符（字符串里的每一个字符）</summary>
        public string WHITE_SPACE = " \t\n\r";
        /// <summary>被视为单词分隔符的字符（字符串里的每一个字符）</summary>
		public string WORD_BREAK;

        /// <summary>需要解析的字符串</summary>
        public string str;
		protected int pos;
		protected int len;

        /// <summary>当前字符串是否已经解析完毕</summary>
		public bool IsEnd
		{
			get { return pos >= len; }
		}
        /// <summary>获取下一个字符，流不进行</summary>
		public char PeekChar
		{
			get { return Peek(); }
		}
        /// <summary>获取下一个单词，流不进行</summary>
		public string PeekNextWord
		{
			get { return PeekNext(WORD_BREAK); }
		}
        /// <summary>获取下一行字符串，流不进行</summary>
		public string PeekNextLine
		{
			get { return PeekNext("\n"); }
		}
        /// <summary>还未解析的后续字符串</summary>
        public string Tail
        {
            get { return str.Substring(pos); }
        }
        /// <summary>当前解析到的字符串索引位置</summary>
        public int Pos
        {
            get { return pos; }
            set
            {
                if (value < 0 || value > len)
                    throw new ArgumentOutOfRangeException("pos");
                this.pos = value;
            }
        }

        public StringStreamReader()
        {
            SetContent(string.Empty, 0);
        }
		public StringStreamReader(string content) : this(content, 0)
		{
		}
        public StringStreamReader(string content, int startIndex)
        {
            SetContent(content, startIndex);
        }

        /// <summary>设置需要解析的字符串</summary>
        /// <param name="content">字符串</param>
        /// <param name="startIndex">设定默认起始位置</param>
        public void SetContent(string content, int startIndex)
        {
            if (content == null)
                content = string.Empty;

            if (startIndex != 0)
                if (startIndex >= content.Length || startIndex < 0)
                    throw new ArgumentOutOfRangeException("startIndex");

            this.str = content;
            this.len = content.Length;
            this.pos = startIndex;
        }
        /// <summary>读取下一个字符，流不进行</summary>
        public char Peek()
		{
			return str[pos];
		}
        /// <summary>读取一个字符，流进行</summary>
        public char Read()
		{
			return str[pos++];
		}
        /// <summary>读取下一行字符串，流进行</summary>
		public string ReadLine()
		{
            return Next("\n");
		}
        /// <summary>读取下一行字符串，索引停在\n字符后</summary>
        public string EatLine()
        {
            return Eat("\n");
        }
        /// <summary>读取下一个单词，与Next不同是索引停在字符后</summary>
        public string Eat(string filter)
		{
            //EatWhitespace();
			int start = pos;
			string next = Next(filter);
			if (pos < len)
				pos++;
			return next;
		}
        /// <summary>读取下一个单词，索引停在字符处，默认会清空所有空格符</summary>
        /// <param name="filter">遇到此字符串中任意字符时停止</param>
        /// <returns>读取到的下一个单词</returns>
        public string Next(string filter)
        {
            return Next(filter, true);
        }
        /// <summary>读取下一个单词，索引停在字符处</summary>
        /// <param name="filter">遇到此字符串中任意字符时停止</param>
        /// <param name="eatWhitespace">查找单词之前，是否需要清空所有空格符</param>
        /// <returns>读取到的下一个单词</returns>
        public string Next(string filter, bool eatWhitespace)
		{
			if (eatWhitespace)
			{
				if (WHITE_SPACE.Contains(filter))
					EatWhitespace(filter);
				else
					EatWhitespace();
			}

			if (pos == len)
				return null;

			int start = pos;
			while (pos < len && filter.IndexOf(PeekChar) == -1)
				if (++pos == len)
					break;

			return str.Substring(start, pos - start);
		}
        /// <summary>以WORD_BREAK作为filter，读取下一个单词</summary>
        public string NextWord()
        {
            return Next(WORD_BREAK, true);
        }
        /// <summary>参见Next，流不进行</summary>
        public string PeekNext(string filter)
		{
            return PeekNext(filter, true);
		}
        /// <summary>参见Next，流不进行</summary>
        public string PeekNext(string filter, bool eatWhitespace)
        {
            int start = pos;
            string next = Next(filter, eatWhitespace);
            pos = start;
            return next;
        }
        /// <summary>清空WHITE_SPACE指定的空格符</summary>
        public void EatWhitespace()
        {
            EatWhitespace(null);
        }
        /// <summary>清空WHITE_SPACE指定的空格符</summary>
        /// <param name="filter">不被视为空格符的字符</param>
        public void EatWhitespace(string filter)
		{
            if (IsEnd)
                return;
			if (filter == null)
				filter = string.Empty;
            while (pos < len && !filter.Contains(PeekChar) && WHITE_SPACE.IndexOf(PeekChar) != -1)
				if (++pos == len)
					break;
		}

        /// <summary>获取指定索引的字符</summary>
        public char GetChar(int pos)
        {
            return str[pos];
        }
        /// <summary>找到下个单词的位置</summary>
        /// <param name="filter">遇到此字符串中任意字符时停止</param>
        /// <returns>找到的位置</returns>
        public int NextPosition(string filter)
        {
            return NextPosition(filter, 0);
        }
        /// <summary>找到下个单词的位置</summary>
        /// <param name="filter">遇到此字符串中任意字符时停止</param>
        /// <param name="skipIndex">需要跳过前几个字符的个数</param>
        /// <returns>找到的位置</returns>
        public int NextPosition(string filter, int skipIndex)
        {
            int p = pos + skipIndex;
            while (filter.IndexOf(str[p]) == -1)
                if (++p == len)
                    break;
            return p;
        }
        /// <summary>设定索引位置，返回当前位置到索引位置的字符串</summary>
        public string ToPosition(int index)
        {
            if (index < pos)
                return null;
            if (index == pos)
                return string.Empty;
            string result = str.Substring(pos, index - pos);
            pos = index;
            return result;
        }
        public bool IsNext(string filter)
        {
            return IsNext(filter, 0);
        }
        public bool IsNext(string filter, int skipIndex)
        {
            return IsNext(filter, skipIndex, true);
        }
        /// <summary>接下来的字符是否是指定的字符</summary>
        /// <param name="filter">指定的字符</param>
        /// <param name="skipIndex">需要跳过前几个字符的个数</param>
        /// <param name="eatWhiteSpace">查找单词之前，是否需要清空所有空格符</param>
        public bool IsNext(string filter, int skipIndex, bool eatWhiteSpace)
        {
            int temp = pos;
            pos += skipIndex;
            if (eatWhiteSpace)
                EatWhitespace();
            if (IsEnd)
                return false;
            if (skipIndex == 0)
                temp = pos;
            int p = pos;
            pos = temp;
            return filter.Contains(str[p]);
        }
        /// <summary>接下来的字符是否是指定的字符</summary>
        public bool IsNextSign(char c)
        {
            EatWhitespace();
            if (IsEnd)
                return false;
            return PeekChar == c;
        }
        public bool IsNextSign(string word)
        {
            return IsNextSign(word, 0);
        }
        /// <summary>接下来的字符串是否是指定的字符串</summary>
        /// <param name="word">指定的字符串</param>
        /// <param name="skipIndex">需要跳过前几个字符的个数</param>
        public bool IsNextSign(string word, int skipIndex)
        {
            int temp = pos;
            pos += skipIndex;
            EatWhitespace();
            if (IsEnd)
                return false;
            if (skipIndex == 0)
                temp = pos;
            int p = pos;
            pos = temp;
            int n = word.Length;
            if (p + n > len)
                return false;
            for (int i = 0; i < n; i++)
                if (str[p + i] != word[i])
                    return false;
            return true;
        }
        /// <summary>读取下一个段落，直到出现指定单词为止，光标停在单词之前</summary>
        public string NextToSign(string sign)
        {
            return ToSign(sign, false, false);
        }
        /// <summary>读取下一个段落(包含指定单词)，直到出现指定标识单词为止，光标停在单词之后</summary>
        public string NextAfterSign(string sign)
        {
            return ToSign(sign, true, true);
        }
        /// <summary>读取下一个段落(不包含指定单词)，直到出现指定标识单词为止，光标停在单词之后</summary>
        public string NextToSignAfter(string sign)
        {
            return ToSign(sign, false, true);
        }
        private string ToSign(string sign, bool after, bool eat)
        {
            int index = str.IndexOf(sign, pos);
            if (index != -1)
            {
                string result = str.Substring(pos, index - pos + (after ? sign.Length : 0));
                pos = index + (eat ? sign.Length : 0);
                return result;
            }
            return null;
        }
        /// <summary>进行到下个指定单词的位置，光标停在单词之前</summary>
        public void EatToSign(string sign)
        {
            int index = str.IndexOf(sign, pos);
            if (index != -1)
                pos = index;
        }
        /// <summary>进行到下个指定单词的位置，光标停在单词之后</summary>
        public void EatAfterSign(string sign)
        {
            int index = str.IndexOf(sign, pos);
            if (index != -1)
                pos = index + sign.Length;
        }
        /// <summary>若接下来是指定单词，进行到指定单词的位置，光标停在单词之后</summary>
        public bool EatAfterSignIfIs(string sign)
        {
            bool result = IsNextSign(sign);
            if (result)
                pos += sign.Length;
            return result;
        }
        /// <summary>若接下来是指定单词，进行到指定单词的位置，光标停在单词之后</summary>
        public bool EatAfterWordIfIs(string word)
        {
            int temp = pos;
            bool result = NextWord() == word;
            if (!result)
                pos = temp;
            return result;
        }
    }

	[AttributeUsage(AttributeTargets.Property)]
	[AReflexible]public class ANonSerializedP : Attribute
	{
        public static readonly string Name = typeof(ANonSerializedP).Name;
	}
    [AttributeUsage(AttributeTargets.Property)]
    [AReflexible]public class ASerializedP : Attribute
    {
        public static readonly string Name = typeof(ASerializedP).Name;
    }
    /// <summary>编辑器里反射编辑字段时的字段名别名和注释</summary>
	[AttributeUsage(AttributeTargets.All)]
	public class ASummary : Attribute
	{
        /// <summary>字段昵称</summary>
        public string FieldName { get; private set; }
        /// <summary>字段注释</summary>
        public string Note { get; private set; }
		public ASummary(string note)
		{
			this.Note = note;
		}
        public ASummary(string field, string note)
        {
            this.FieldName = field;
            this.Note = note;
        }
	}

	public static class Config<T> where T : class, new()
	{
		private static T settingDefault = new T();
		private static T setting = settingDefault;

		public static T Setting
		{
			get { return setting; }
		}
		public static T SettingDefault
		{
			get { return settingDefault; }
		}

		public static void Save(string file)
		{
			XmlWriter writer = new XmlWriter();
			writer.Setting.Static = false;
			writer.WriteObject(setting);

            XmlReader reader = new XmlReader(writer.Result);
            reader.Setting.Static = false;

			_IO.WriteText(file, reader.ReadToNode().OutterXml);
		}
        public static bool Load(string file)
        {
            if (System.IO.File.Exists(file))
            {
                try
                {
					string content = _IO.ReadText(file);
					XmlReader reader = new XmlReader(content);
                    reader.Setting.Static = false;
                    setting = reader.ReadObject<T>();
                    return true;
                }
                catch (Exception ex)
                {
                    _LOG.Info("Load Config<T> error! msg={0}", ex.Message);
                    setting = settingDefault;
                }
            }
            else
            {
                setting = settingDefault;
                Save(file);
            }
            return false;
        }
	}
    /// <summary>
    /// 1. StringBuilder.AppendLine() { Append(Environment.NewLine); }: .net = \r\n, mono = \n
    /// </summary>
    public static class _SERIALIZE
    {
        public static readonly object[] EmptyObjects = new object[0];
        public static readonly Type ObjectType = typeof(object);
        private static Dictionary<char, char> ESC;
        /// <summary>变量名不能使用的字符</summary>
        public const string VARIABLE_NAME = "/?.>,<;:'\"\\|}]{[=+-)(*&^%$#@!`~\r\n\t ";
        /// <summary>变量名不能使用的C#关键字</summary>
        public static HashSet<string> Keywords;

        public static T ReadObject<T>(this IReader reader)
        {
            return (T)reader.ReadObject(typeof(T));
        }
        public static void Read<T>(this IReader reader, out T result)
        {
            result = (T)reader.ReadObject(typeof(T));
        }
        public static string WriteStatic(this XmlWriter writer, Type staticType)
        {
            if (!staticType.IsStatic())
                throw new ArgumentException("staticType");

            writer.Setting.Static = true;
            writer.WriteObject(null, staticType);

            XmlReader reader = new XmlReader(writer.Result);
            reader.Setting.Static = true;

            return reader.ReadToNode().OutterXml;
        }
        public static void ReadStatic(this XmlReader reader, Type staticType)
        {
            if (!staticType.IsStatic())
                throw new ArgumentException("staticType");

            reader.Setting.Static = true;
            reader.ReadObject(staticType);
        }
        public static byte[] WriteStatic(this ByteWriter writer, Type staticType)
        {
            if (!staticType.IsStatic())
                throw new ArgumentException("staticType");

            writer.Setting.Static = true;
            writer.WriteObject(null, staticType);
            return writer.GetBuffer();
        }
        public static void ReadStatic(this ByteReader reader, Type staticType)
        {
            if (!staticType.IsStatic())
                throw new ArgumentException("staticType");

            reader.Setting.Static = true;
            reader.ReadObject(staticType);
        }
        private static void InitializeESC()
        {
            if (ESC == null)
            {
                ESC = new Dictionary<char, char>();
                ESC.Add('t', '\t');
                ESC.Add('r', '\r');
                ESC.Add('n', '\n');
                ESC.Add('0', '\0');
            }
        }
        public static char GetEscapeChar(char c)
        {
            InitializeESC();
            char ec;
            if (ESC.TryGetValue(c, out ec))
                return ec;
            else
                return c;
        }
        public static char GetUnescapeChar(char c, out bool unescaped)
        {
            InitializeESC();
            unescaped = true;
            foreach (var item in ESC)
                if (item.Value == c)
                    return item.Key;
            unescaped = false;
            return c;
        }
        public static string CodeToString(string codeStr)
        {
            StringBuilder builder = new StringBuilder(codeStr.Length);
            bool esc = false;
            for (int i = 0, n = codeStr.Length; i < n; i++)
            {
                if (esc)
                {
                    builder.Append(GetEscapeChar(codeStr[i]));
                    esc = false;
                }
                else
                {
                    if (codeStr[i] == '\\')
                        esc = true;
                    else
                        builder.Append(codeStr[i]);
                }
            }
            return builder.ToString();
        }
        public static string StringToCode(string str)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('\"');
            foreach (char c in str)
            {
                builder.Append(CharToCodeChar(c));
                //switch (c)
                //{
                //    case '"': builder.Append("\\\""); break;
                //    case '\r': builder.Append("\\r"); break;
                //    case '\n': builder.Append("\\n"); break;
                //    case '\0': builder.Append("\\0"); break;
                //    case '\t': builder.Append("\\t"); break;
                //    case '\\': builder.Append("\\\\"); break;
                //    default: builder.Append(c); break;
                //}
            }
            builder.Append('\"');
            return builder.ToString();
        }
        public static string CharToCodeChar(char c)
        {
            switch (c)
            {
                case '"': return "\\\"";
                case '\r': return "\\r";
                case '\n': return "\\n";
                case '\0': return "\\0";
                case '\t': return "\\t";
                case '\\': return "\\\\";
                default: return c.ToString();
            }
        }
        /// <summary>是否是合法的变量名</summary>
        public static bool IsVariableName(string name)
        {
            if (Keywords == null)
            {
                Keywords = new HashSet<string>(new string[]
                {
                    // 访问修饰符
                    "volatile", "partial", "private", "internal", "protected", "public", "abstract", "virtual", "override", "sealed", "static", "readonly", "const", "new", "partial", "extern", "unsafe", "explicit", "implicit", "event", "operator",
                    // 类型
                    "class", "struct", "interface", "enum", "delegate",
                    // 泛型，参数
                    "in", "out", "ref", "params", "this", "base",
                    // 运算符
                    "typeof", "sizeof", "default", "checked", "unchecked", "as", "is",
                    // 声明
                    "if", "else", "switch", "case", "goto", "for", "foreach", "while", "do", "continue", "break", "return", "yield", "throw", "try", "catch", "finally", "using", "fixed", "lock",
                    // 类型
                    "bool", "true", "false", "sbyte", "byte", "char", "short", "ushort", "int", "uint", "float", "long", "ulong", "double", "decimal", "object", "null", "void",
                    // 其它
                    "namespace", "stackalloc",
                });
            }
            if (string.IsNullOrEmpty(name)) return false;
            // 不能数字开头
            char c = name[0];
            if (c >= '0' && c <= '9') return false;
            foreach (var item in name)
                if (VARIABLE_NAME.IndexOf(item) != -1)
                    return false;
            return !Keywords.Contains(name);
        }
        public static void Append(this StringBuilder builder, string format, params object[] param)
        {
            builder.AppendFormat(format, param);
        }
        public static void AppendLine(this StringBuilder builder, string format, params object[] param)
        {
            builder.AppendFormat(format, param);
            builder.AppendLine();
        }
        public static void AppendBlock(this StringBuilder builder, Action append)
        {
            if (append == null)
                throw new ArgumentNullException("append");
            builder.AppendLine("{");
            append();
            builder.AppendLine("}");
        }
        public static void AppendBlockNonBreakline(this StringBuilder builder, Action append)
        {
            if (append == null)
                throw new ArgumentNullException("append");
            builder.AppendLine("{");
            append();
            builder.Append("}");
        }
        public static void AppendBlockWithComma(this StringBuilder builder, Action append)
        {
            if (append == null)
                throw new ArgumentNullException("append");
            builder.AppendLine("{");
            append();
            builder.AppendLine("},");
        }
        public static void AppendBlockWithEnd(this StringBuilder builder, Action append)
        {
            if (append == null)
                throw new ArgumentNullException("append");
            builder.AppendLine("{");
            append();
            builder.AppendLine("};");
        }
        private static void BuildSimpleAQName(StringBuilder builder, Type type)
        {
            int array = 0;
            while (type.IsArray)
            {
                array++;
                type = type.GetElementType();
            }
            if (!string.IsNullOrEmpty(type.Namespace))
            {
                builder.Append("{0}.", type.Namespace);
            }
            if (type.ReflectedType != null)
            {
                int count = 0;
                Type reflected = type.ReflectedType;
                for (; reflected != null; count++)
                    reflected = reflected.ReflectedType;
                // 栈方式追加内部类的外层类
                for (int i = count; i > 0; i--)
                {
                    reflected = type;
                    for (int j = 0; j < i; j++)
                        reflected = reflected.ReflectedType;
                    builder.Append("{0}+", reflected.Name);
                }
            }
            builder.Append(type.Name);
            if (type.IsGenericType)
            {
                builder.Append('[');
                var generic = type.GetGenericArguments();
                int len = generic.Length - 1;
                for (int i = 0; i <= len; i++)
                {
                    builder.Append('[');
                    BuildSimpleAQName(builder, generic[i]);
                    builder.Append(']');
                    if (i != len)
                        builder.Append(',');
                }
                builder.Append(']');
            }
            for (int i = 0; i < array; i++)
                builder.Append("[]");
            builder.Append(", {0}", type.Assembly.GetName().Name);
        }
        /// <summary>短AQ名在加密方式加载下的程序集中还是未能通过Type.GetType获得类型</summary>
        [Code(ECode.BUG)]public static string SimpleAQName(this Type type)
        {
            StringBuilder builder = new StringBuilder();
            BuildSimpleAQName(builder, type);
            return builder.ToString();
        }
        public static Type LoadSimpleAQName(string name)
        {
            Type type = null;
            try
            {
                type = Type.GetType(name);
#if DEBUG
                if (type == null)
                {
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    type = _SERIALIZE.ParseTypeName<Type>(name,
                        (typeName, assemblyName) =>
                        {
                            var ass = assemblies.FirstOrDefault(a => a.GetName().Name == assemblyName);
                            if (ass == null)
                                return null;
                            // mono: 若没有类型则肯定抛出TypeLoadException
                            return ass.GetType(typeName);
                        },
                        (t1, tArray) =>
                        {
                            return t1.MakeGenericType(tArray);
                        },
                        (t) =>
                        {
                            return t.MakeArrayType();
                        });
                }
#endif
            }
            catch
            {
                type = null;
            }
            return type;
        }
        /// <summary>解析类型全名称</summary>
        /// <param name="name">类型全名称</param>
        /// <param name="onParseType">例如System.Int32, mscorlib，根据类型名System.Int32和程序集名mscorlib构造类型T</param>
        /// <param name="onGeneric">构造泛型类型</param>
        /// <param name="onArray">构造数组类型</param>
        public static T ParseTypeName<T>(string name, 
            Func<string, string, T> onParseType,
            Func<T, T[], T> onGeneric,
            Func<T, T> onArray)
        {
            int index = name.IndexOf('[', 0);
            if (index == -1)
            {
                string[] names = name.Split(',');
                if (names.Length > 1)
                {
                    names[1] = names[1].Trim();
                    return onParseType(names[0], names[1]);
                }
                else
                    return onParseType(names[0], null);
            }
            else
            {
                string typeName = name.Substring(0, index);
                // 追加最后的程序集信息
                int lastIndex = name.LastIndexOf(']');
                lastIndex = name.IndexOf(',', lastIndex);
                string aname = null;
                if (lastIndex != -1)
                    aname = name.Substring(lastIndex + 2);
                T type = onParseType(typeName, aname);
                int index2 = typeName.IndexOf('`', 0);
                if (index2 != -1)
                {
                    int typeParameterCount = int.Parse(name.Substring(index2 + 1, index - index2 - 1));
                    T[] typeArguments = new T[typeParameterCount];
                    int gcount = 1;
                    index += 2;
                    index2 = index;
                    int tai = 0;
                    while (true)
                    {
                        int end = name.IndexOf(']', index2);
                        int start = name.IndexOf('[', index2);
                        if (start != -1 && start < end)
                        {
                            // 有其它的泛型类型
                            gcount++;
                            index2 = start + 1;
                            continue;
                        }
                        if (gcount > 0)
                        {
                            // 其它泛型类型的结束
                            gcount--;
                            if (gcount > 0)
                            {
                                index2 = end + 1;
                                continue;
                            }
                        }
                        typeName = name.Substring(index, end - index);
                        typeArguments[tai++] = ParseTypeName(typeName, onParseType, onGeneric, onArray);
                        if (tai == typeArguments.Length)
                        {
                            // 跳过字符]]
                            index = end + 2;
                            break;
                        }
                        // 跳过字符：],[
                        index = end + 3;
                        index2 = index;
                        gcount = 1;
                    }

                    type = onGeneric(type, typeArguments);
                }
                // name[index] == '[' 用于处理 System.Int32[], mscorlib
                while (index < name.Length && name[index] == '[')
                {
                    type = onArray(type);
                    index += 2;
                }
                return type;
            }
        }
        public static bool IsCustomType(this Type type)
        {
            bool isNullable;
            Type nullable;
            if (IsNullable(type, out nullable))
                isNullable = !IsCustomType(nullable);
            else
                isNullable = false;
            return !type.IsPrimitive && type != typeof(string) && !isNullable && !type.IsEnum;
            //return !type.IsPrimitive && type != typeof(string) && !isNullable && (type.IsClass || type.IsInterface);
            //return type != typeof(string) && !type.IsNullable() && (type.IsClass || !(type.IsInterface || type.IsPrimitive));
        }
        public static bool IsStatic(this Type type)
        {
            return type.IsAbstract && type.IsSealed;
        }
        public static bool IsNumber(this Type type)
        {
            return type == typeof(sbyte)
                || type == typeof(byte)
                || type == typeof(short)
                || type == typeof(ushort)
                || type == typeof(int)
                || type == typeof(uint)
                || type == typeof(float)
                || type == typeof(double)
                || type == typeof(long)
                || type == typeof(ulong)
#if DEBUG
                || type == typeof(decimal);
#else
            ;
#endif
        }
        public static object ParseNumber(this Type type, string value)
        {
            if (type == typeof(sbyte))
            {
                if (string.IsNullOrEmpty(value)) return (sbyte)0;
                return sbyte.Parse(value);
            }
            else if (type == typeof(byte))
            {
                if (string.IsNullOrEmpty(value)) return (byte)0;
                return byte.Parse(value);
            }
            else if (type == typeof(short))
            {
                if (string.IsNullOrEmpty(value)) return (short)0;
                return short.Parse(value);
            }
            else if (type == typeof(ushort))
            {
                if (string.IsNullOrEmpty(value)) return (ushort)0;
                return ushort.Parse(value);
            }
            else if (type == typeof(int))
            {
                if (string.IsNullOrEmpty(value)) return (int)0;
                return int.Parse(value);
            }
            else if (type == typeof(uint))
            {
                if (string.IsNullOrEmpty(value)) return (uint)0;
                return uint.Parse(value);
            }
            else if (type == typeof(float))
            {
                if (string.IsNullOrEmpty(value)) return (float)0;
                return float.Parse(value);
            }
            else if (type == typeof(long))
            {
                if (string.IsNullOrEmpty(value)) return (long)0;
                return long.Parse(value);
            }
            else if (type == typeof(ulong))
            {
                if (string.IsNullOrEmpty(value)) return (ulong)0;
                return ulong.Parse(value);
            }
            else if (type == typeof(double))
            {
                if (string.IsNullOrEmpty(value)) return (double)0;
                return double.Parse(value);
            }
#if !HTML5
            else if (type == typeof(decimal))
            {
                if (string.IsNullOrEmpty(value)) return (decimal)0;
                return decimal.Parse(value);
            }
#endif
            throw new InvalidCastException("错误的数字类型" + type.FullName);
        }
        public static bool IsNullable(this Type type)
        {
            Type nullType;
            return IsNullable(type, out nullType);
        }
        public static bool IsNullable(this Type type, out Type nullable)
        {
            nullable = Nullable.GetUnderlyingType(type);
            return nullable != null && nullable != type;
        }
        public static bool Is(this Type type, Type target)
        {
            if ((type == null && target != null) || (type != null && target == null))
                return false;
            return target.IsAssignableFrom(type);
        }
        public static bool IsDelegate(this Type type)
        {
            return type.Is(typeof(Delegate));
        }
        public static Type[] GetAllInterfaces(this Type type)
        {
            List<Type> types = new List<Type>();
            Type[] interfaces = type.GetInterfaces();
            foreach (Type item in interfaces)
            {
                types.AddRange(item.GetAllInterfaces());
            }
            types.AddRange(interfaces);
            return types.Distinct().ToArray();
        }
    }
}
