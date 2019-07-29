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
		object Instance { get; }
		Type Type { get; }
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
			MethodInfo set = property.GetSetMethod();
			if (set == null || set.IsAbstract)
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
			// get set
			if (!property.CanRead)
				return true;
			// index
			if (property.GetIndexParameters().Length > 0)
				return true;
			// abstract
			MethodInfo set = property.GetGetMethod();
			if (set == null || set.IsAbstract)
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
	public abstract class Serializable
	{
        public const string ABSTRACT_TYPE = "#";
		public SerializeSetting Setting = SerializeSetting.DefaultSetting;
        /// <summary>
        /// <para>true: value.GetType() != type时，则将序列化value.GetType()的类型名以便反序列化</para>
        /// <para>false: 数据库自动生成的数据表子类型可能直接用于传输，此时序列化类型名将加大传输数据量</para>
        /// </summary>
        public static bool RecognitionChildType = true;

		public static bool IsAbstractType(object value, ref Type type)
		{
			Type temp;
			if (type.IsNullable(out temp))
			{
				type = temp;
				return false;
			}
			if (RecognitionChildType && value != null)
			{
				temp = value.GetType();
				if (temp != type)
				{
					type = temp;
					return true;
				}
			}
			return (type.IsAbstract && !type.IsSealed);
		}
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
			if (type.IsEnum)
			{
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
				|| value is double)
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
				if (type.IsGenericType)
				{
					Type[] temp = type.GetGenericArguments();
					childType = temp[0];
				}
				else
				{
					childType = type.GetElementType();
                    if (childType == null)
                        throw new NotSupportedException("不支持非泛型非数组的IEnumerable类型");
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
			//WriteNumber(value.Ticks);
			WriteString(value.ToString());
		}
		protected void WriteDateTime(DateTime value)
		{
			//WriteNumber(Utility.ToUnixTimestamp(value));
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
        //protected object ReadValue(Type type, string value)
        //{
        //    if (value == null)
        //    {
        //        return null;
        //    }

        //    Type nullable;
        //    if (type.IsValueType && type.IsNullable(out nullable))
        //    {
        //        // Nullable<struct>
        //        if (string.IsNullOrEmpty(value))
        //        {
        //            return null;
        //        }
        //        else
        //        {
        //            type = nullable;
        //        }
        //    }

        //    if (type.IsEnum)
        //    {
        //        if (string.IsNullOrEmpty(value))
        //            return Enum.GetValues(type).GetValue(0);
        //        return Enum.Parse(type, value);
        //        //Type underlying = Enum.GetUnderlyingType(type);
        //        //try
        //        //{
        //        //    return Convert.ChangeType(Convert.ChangeType(value, underlying), type);
        //        //}
        //        //catch
        //        //{
        //        //    return Enum.Parse(type, value);
        //        //}
        //    }
        //    else if (type == typeof(char))
        //    {
        //        if (string.IsNullOrEmpty(value))
        //            return default(char);
        //        return UnEscapeString(value)[0];
        //    }
        //    else if (type == typeof(string))
        //    {
        //        return UnEscapeString(value);
        //    }
        //    else if (type == typeof(bool))
        //    {
        //        if (string.IsNullOrEmpty(value))
        //            return default(bool);
        //        if (value == "1")
        //            return true;
        //        else if (value == "0")
        //            return false;
        //        else
        //            return bool.Parse(value);
        //    }
        //    else if (type == typeof(sbyte))
        //    {
        //        if (string.IsNullOrEmpty(value))
        //            return default(sbyte);
        //        return sbyte.Parse(value);
        //    }
        //    else if (type == typeof(byte))
        //    {
        //        if (string.IsNullOrEmpty(value))
        //            return default(byte);
        //        return byte.Parse(value);
        //    }
        //    else if (type == typeof(short))
        //    {
        //        if (string.IsNullOrEmpty(value))
        //            return default(short);
        //        return short.Parse(value);
        //    }
        //    else if (type == typeof(ushort))
        //    {
        //        if (string.IsNullOrEmpty(value))
        //            return default(ushort);
        //        return ushort.Parse(value);
        //    }
        //    else if (type == typeof(int))
        //    {
        //        if (string.IsNullOrEmpty(value))
        //            return default(int);
        //        return int.Parse(value);
        //    }
        //    else if (type == typeof(uint))
        //    {
        //        if (string.IsNullOrEmpty(value))
        //            return default(uint);
        //        return uint.Parse(value);
        //    }
        //    else if (type == typeof(float))
        //    {
        //        if (string.IsNullOrEmpty(value))
        //            return default(float);
        //        return float.Parse(value);
        //    }
        //    else if (type == typeof(long))
        //    {
        //        if (string.IsNullOrEmpty(value))
        //            return default(long);
        //        return long.Parse(value);
        //    }
        //    else if (type == typeof(ulong))
        //    {
        //        if (string.IsNullOrEmpty(value))
        //            return default(ulong);
        //        return ulong.Parse(value);
        //    }
        //    else if (type == typeof(double))
        //    {
        //        if (string.IsNullOrEmpty(value))
        //            return default(double);
        //        return double.Parse(value);
        //    }
        //    else if (type == typeof(DateTime))
        //    {
        //        if (string.IsNullOrEmpty(value))
        //            return default(DateTime);
        //        //return Utility.ToUnixTime(int.Parse(value));
        //        return DateTime.Parse(value);
        //    }
        //    else if (type == typeof(TimeSpan))
        //    {
        //        if (string.IsNullOrEmpty(value))
        //            return default(TimeSpan);
        //        //return new TimeSpan(long.Parse(value));
        //        return TimeSpan.Parse(value);
        //    }
        //    throw new NotSupportedException("不支持的数据类型");
        //}
        protected object ReadValue(Type type, string value, out bool read)
        {
            read = true;

            if (value == null)
            {
                return null;
            }

            Type nullable;
            if (type.IsValueType && type.IsNullable(out nullable))
            {
                // Nullable<struct>
                if (string.IsNullOrEmpty(value))
                {
                    return null;
                }
                else
                {
                    type = nullable;
                }
            }

            if (type.IsArray)
            {
            }

            if (type.IsEnum)
            {
                if (string.IsNullOrEmpty(value))
                    return Enum.GetValues(type).GetValue(0);
                return Enum.Parse(type, value);
                //Type underlying = Enum.GetUnderlyingType(type);
                //try
                //{
                //    return Convert.ChangeType(Convert.ChangeType(value, underlying), type);
                //}
                //catch
                //{
                //    return Enum.Parse(type, value);
                //}
            }
            else if (type == typeof(char))
            {
                if (string.IsNullOrEmpty(value))
                    return default(char);
                return UnEscapeString(value)[0];
            }
            else if (type == typeof(string))
            {
                return UnEscapeString(value);
            }
            else if (type == typeof(bool))
            {
                if (string.IsNullOrEmpty(value))
                    return default(bool);
                if (value == "1")
                    return true;
                else if (value == "0")
                    return false;
                else
                    return bool.Parse(value);
            }
            else if (type == typeof(sbyte))
            {
                if (string.IsNullOrEmpty(value))
                    return default(sbyte);
                return sbyte.Parse(value);
            }
            else if (type == typeof(byte))
            {
                if (string.IsNullOrEmpty(value))
                    return default(byte);
                return byte.Parse(value);
            }
            else if (type == typeof(short))
            {
                if (string.IsNullOrEmpty(value))
                    return default(short);
                return short.Parse(value);
            }
            else if (type == typeof(ushort))
            {
                if (string.IsNullOrEmpty(value))
                    return default(ushort);
                return ushort.Parse(value);
            }
            else if (type == typeof(int))
            {
                if (string.IsNullOrEmpty(value))
                    return default(int);
                return int.Parse(value);
            }
            else if (type == typeof(uint))
            {
                if (string.IsNullOrEmpty(value))
                    return default(uint);
                return uint.Parse(value);
            }
            else if (type == typeof(float))
            {
                if (string.IsNullOrEmpty(value))
                    return default(float);
                return float.Parse(value);
            }
            else if (type == typeof(long))
            {
                if (string.IsNullOrEmpty(value))
                    return default(long);
                return long.Parse(value);
            }
            else if (type == typeof(ulong))
            {
                if (string.IsNullOrEmpty(value))
                    return default(ulong);
                return ulong.Parse(value);
            }
            else if (type == typeof(double))
            {
                if (string.IsNullOrEmpty(value))
                    return default(double);
                return double.Parse(value);
            }
            else if (type == typeof(DateTime))
            {
                if (string.IsNullOrEmpty(value))
                    return default(DateTime);
                long timestamp;
                if (long.TryParse(value, out timestamp))
                    return Utility.ToTime(timestamp);
                return DateTime.Parse(value);
            }
            else if (type == typeof(TimeSpan))
            {
                if (string.IsNullOrEmpty(value))
                    return default(TimeSpan);
                long timestamp;
                if (long.TryParse(value, out timestamp))
                    return TimeSpan.FromMilliseconds(timestamp);
                return TimeSpan.Parse(value);
            }

            read = false;
            return null;
            //throw new NotSupportedException("不支持的数据类型");
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
            string word = ReadNextString();
            if (string.IsNullOrEmpty(word) || word == "null")
            {
                return null;
            }
            else
            {
                return ReadObject(nullableType);
            }
        }
        protected virtual object ReadEnum(Type type, Type underlyingType)
        {
            if (string.IsNullOrEmpty(PeekNextString()))
            {
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
            return _SERIALIZE.ParseNumber(type, ReadNextString());
            //char c = PeekChar;
            //switch (c)
            //{
            //    case '0':
            //    case '1':
            //    case '2':
            //    case '3':
            //    case '4':
            //    case '5':
            //    case '6':
            //    case '7':
            //    case '8':
            //    case '9':
            //    case '-':
            //    case '.':
            //        return _SERIALIZE.ParseNumber(type, ReadNextString());

            //    default: throw new FormatException("错误的数字格式");
            //}
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
    public class StringStreamReader
    {
        public string WHITE_SPACE = " \t\n\r";
		public string WORD_BREAK;

        public string str;
		protected int pos;
		protected int len;

		public bool IsEnd
		{
			get { return pos >= len; }
		}
		public char PeekChar
		{
			get { return Peek(); }
		}
		public string PeekNextWord
		{
			get { return PeekNext(WORD_BREAK); }
		}
		public string PeekNextLine
		{
			get { return PeekNext("\n"); }
		}
		public string PeekNextEnd
		{
			get { return PeekNext(";"); }
		}
        public string Tail
        {
            get { return str.Substring(pos); }
        }
        public int Pos
        {
            get { return pos; }
            set
            {
                if (value < 0 || value >= len)
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

        public void SetContent(string content, int startIndex)
        {
            if (content == null)
                throw new ArgumentNullException("content");

            if (startIndex >= content.Length || startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex");

            this.str = content;
            this.len = content.Length;
            this.pos = startIndex;
        }
        public char Peek()
		{
			return str[pos];
		}
        public char Read()
		{
			return str[pos++];
		}
		public string ReadLine()
		{
            return Next("\n");
		}
        public string EatLine()
        {
            return Eat("\n");
        }
        public string Eat(string filter)
		{
            //EatWhitespace();
			int start = pos;
			string next = Next(filter);
			if (pos < len)
				pos++;
			return next;
		}
        public string Next(string filter)
        {
            return Next(filter, true);
        }
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
			while (filter.IndexOf(PeekChar) == -1)
				if (++pos == len)
					break;

			return str.Substring(start, pos - start);
		}
        public string NextWord()
        {
            return Next(WORD_BREAK);
        }
        public string PeekNext(string filter)
		{
            return PeekNext(filter, true);
		}
        public string PeekNext(string filter, bool eatWhitespace)
        {
            int start = pos;
            string next = Next(filter, eatWhitespace);
            pos = start;
            return next;
        }
        public void EatWhitespace()
        {
            EatWhitespace(null);
        }
        public void EatWhitespace(string filter)
		{
            if (IsEnd)
                return;
			if (filter == null)
				filter = string.Empty;
			while (!filter.Contains(PeekChar) && WHITE_SPACE.IndexOf(PeekChar) != -1)
				if (++pos == len)
					break;
		}


        public char GetChar(int pos)
        {
            return str[pos];
        }
        public int NextPosition(string filter)
        {
            return NextPosition(filter, 0);
        }
        public int NextPosition(string filter, int skipIndex)
        {
            int p = pos + skipIndex;
            while (filter.IndexOf(str[p]) == -1)
                if (++p == len)
                    break;
            return p;
        }
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
        public string NextToSign(string sign)
        {
            return ToSign(sign, false, false);
        }
        public string NextAfterSign(string sign)
        {
            return ToSign(sign, true, true);
        }
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
        public void EatToSign(string sign)
        {
            int index = str.IndexOf(sign, pos);
            if (index != -1)
                pos = index;
        }
        public void EatAfterSign(string sign)
        {
            int index = str.IndexOf(sign, pos);
            if (index != -1)
                pos = index + sign.Length;
        }
        public bool EatAfterSignIfIs(string sign)
        {
            bool result = IsNextSign(sign);
            if (result)
                pos += sign.Length;
            return result;
        }
    }

	[AttributeUsage(AttributeTargets.Property)]
	[AReflexible]public class ANonSerializedP : Attribute
	{
        public static readonly string Name = typeof(ANonSerializedP).Name;
	}
	[AttributeUsage(AttributeTargets.All)]
	public class ASummary : Attribute
	{
		public string Note
		{
			get;
			private set;
		}
		public ASummary(string note)
		{
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
        private static Dictionary<char, char> ESC = new Dictionary<char, char>();

        static _SERIALIZE()
        {
            ESC.Add('t', '\t');
            ESC.Add('r', '\r');
            ESC.Add('n', '\n');
            ESC.Add('0', '\0');
        }

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
        public static char GetEscapeChar(char c)
        {
            char ec;
            if (ESC.TryGetValue(c, out ec))
                return ec;
            else
                return c;
        }
        public static char GetUnescapeChar(char c, out bool unescaped)
        {
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
            try { type = Type.GetType(name); }
            catch { }
            //Type type = Type.GetType(name);
#if DEBUG
            if (type == null)
            {
                // 缓存
                int index = name.IndexOf('[', 0);
                if (index == -1)
                {
                    string[] names = name.Split(',');
                    names[1] = names[1].Trim();
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    var ass = assemblies.FirstOrDefault(a => a.GetName().Name == names[1]);
                    if (ass == null)
                        return null;
                    // mono: 若没有类型则肯定抛出TypeLoadException
                    type = ass.GetType(names[0]);
                }
                else
                {
                    string typeName = name.Substring(0, index);
                    // 追加最后的程序集信息
                    int lastIndex = name.LastIndexOf(',');
                    typeName += name.Substring(lastIndex);
                    type = LoadSimpleAQName(typeName);
                    int index2 = typeName.IndexOf('`', 0);
                    if (index2 != -1)
                    {
                        int typeParameterCount = int.Parse(name.Substring(index2 + 1, index - index2 - 1));
                        Type[] typeArguments = new Type[typeParameterCount];
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
                            typeArguments[tai++] = LoadSimpleAQName(typeName);
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
                        type = type.MakeGenericType(typeArguments);
                    }
                    // name[index] == '[' 用于处理 System.Int32[], mscorlib
                    while (index < name.Length && name[index] == '[')
                    {
                        type = type.MakeArrayType();
                        index += 2;
                    }
                }
            }
#endif
            return type;
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
            else if (type == typeof(decimal))
            {
                if (string.IsNullOrEmpty(value)) return (decimal)0;
                return decimal.Parse(value);
            }
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
