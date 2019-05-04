using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Text;

namespace EntryEngine.Serialize
{
	public abstract class Binary : Serializable
    {
        //internal static Type COLLECTION = typeof(ICollection);
        //internal static Type NULLABLE = typeof(Nullable<>);

#if HTML5
        public bool Long52Bit = true;
#else
        /// <summary>Long类型采用52位长度</summary>
        public bool Long52Bit = false;
#endif

        protected byte[] buffer;
        protected int index;
		public byte[] Buffer
		{
			get { return buffer; }
		}
        protected byte[] Previous
        {
            get
            {
                int count = 20;
                count = _MATH.Min(index, count);
                byte[] temp = new byte[count];
                this.buffer.Copy(index - count, temp, 0, count);
                return temp;
            }
        }
        protected byte[] Current
        {
            get
            {
                int count = 20;
                count = _MATH.Min(buffer.Length - index, count);
                byte[] temp = new byte[count];
                this.buffer.Copy(index, temp, 0, count);
                return temp;
            }
        }
        public int Position
        {
            get { return index; }
        }
		public void Reset()
		{
			index = 0;
		}
        public void Seek(int position)
        {
            if (position < 0 || position >= buffer.Length)
                throw new IndexOutOfRangeException();
            this.index = position;
        }
		public void Skip(int bytes)
		{
			index = _MATH.Clamp(index + bytes, 0, buffer.Length - 1);
		}
        public virtual byte[] GetBuffer()
        {
            byte[] flush = new byte[index];
            Utility.Copy(buffer, 0, flush, 0, index);
            return flush;
        }

        protected static Type[] SupportiveTypes =
        {
            typeof(bool),
            typeof(sbyte),
            typeof(byte),
            typeof(short),
            typeof(ushort),
            typeof(char),
            typeof(int),
            typeof(uint),
            typeof(float),
            typeof(long),
            typeof(ulong),
            typeof(double),
            typeof(TimeSpan),
            typeof(DateTime),
            typeof(string),

            typeof(bool[]),
            typeof(sbyte[]),
            typeof(byte[]),
            typeof(short[]),
            typeof(ushort[]),
            typeof(char[]),
            typeof(int[]),
            typeof(uint[]),
            typeof(float[]),
            typeof(long[]),
            typeof(ulong[]),
            typeof(double[]),
            typeof(TimeSpan[]),
            typeof(DateTime[]),
            typeof(string[]),
        };

        const int B23 = 8388607;
        const long B52 = 4503599627370495L;
        const double B23D = 1.0 / B23;
        const double B52D = 1.0 / B52;
        public static int GetBitCount(long value)
        {
            if (value == 0)
                return 0;
            int count = 1;
            while ((value >>= 1) != 0)
                count++;
            return count;
        }

        public static void GetBytes(byte[] bytes, int index, bool value)
        {
            bytes[index] = (byte)(value ? 1 : 0);
        }
        public static void GetBytes(byte[] bytes, int index, sbyte value)
        {
            bytes[index] = (byte)(value + 127);
        }
        public static void GetBytes(byte[] bytes, int index, byte value)
        {
            bytes[index] = value;
        }
        public static void GetBytes(byte[] bytes, int index, short value)
        {
            bytes[index] = (byte)(value & 255);
            bytes[index + 1] = (byte)(value >> 8);
        }
        public static void GetBytes(byte[] bytes, int index, ushort value)
        {
            bytes[index] = (byte)(value & 255);
            bytes[index + 1] = (byte)(value >> 8);
        }
        public static void GetBytes(byte[] bytes, int index, char value)
        {
            GetBytes(bytes, index, (ushort)value);
        }
        public static void GetBytes(byte[] bytes, int index, int value)
        {
            bytes[index] = (byte)(value & 255);
            bytes[index + 1] = (byte)((value >> 8) & 255);
            bytes[index + 2] = (byte)((value >> 16) & 255);
            bytes[index + 3] = (byte)((value >> 24));
        }
        public static void GetBytes(byte[] bytes, int index, uint value)
        {
            bytes[index] = (byte)(value & 255);
            bytes[index + 1] = (byte)((value >> 8) & 255);
            bytes[index + 2] = (byte)((value >> 16) & 255);
            bytes[index + 3] = (byte)((value >> 24));
        }
        public static void GetBytes(byte[] bytes, int index, float value)
        {
            int v = 0;
            int a = (int)value;
            v |= a;
            float b = value - a;
            int offset = GetBitCount(a);
            int temp = offset;
            while (temp < 24)
            {
                b *= 2;
                if (b >= 1)
                {
                    v = (v << 1) | 1;
                    b--;
                }
                else
                    v <<= 1;
                temp++;
            }
            v &= B23;
            v |= (offset + 126) << 23;
            if (value < 0) v |= 1 << 31;
            GetBytes(bytes, index, v);
        }
        public static void GetBytes(byte[] bytes, int index, long value)
        {
            bytes[index] = (byte)(value & 255);
            bytes[index + 1] = (byte)((value >> 8) & 255);
            bytes[index + 2] = (byte)((value >> 16) & 255);
            bytes[index + 3] = (byte)((value >> 24) & 255);
            bytes[index + 4] = (byte)((value >> 32) & 255);
            bytes[index + 5] = (byte)((value >> 40) & 255);
            bytes[index + 6] = (byte)((value >> 48) & 255);
            bytes[index + 7] = (byte)((value >> 56));
        }
        public static void GetBytes(byte[] bytes, int index, ulong value)
        {
            bytes[index] = (byte)(value & 255);
            bytes[index + 1] = (byte)((value >> 8) & 255);
            bytes[index + 2] = (byte)((value >> 16) & 255);
            bytes[index + 3] = (byte)((value >> 24) & 255);
            bytes[index + 4] = (byte)((value >> 32) & 255);
            bytes[index + 5] = (byte)((value >> 40) & 255);
            bytes[index + 6] = (byte)((value >> 48) & 255);
            bytes[index + 7] = (byte)((value >> 56));
        }
        public static void GetBytes(byte[] bytes, int index, double value)
        {
            long v = 0;
            long a = (long)value;
            v |= a;
            double b = value - a;
            int offset = GetBitCount(a);
            int temp = offset;
            while (temp < 53)
            {
                b *= 2;
                if (b >= 1)
                {
                    v = (v << 1) | 1;
                    b--;
                }
                else
                    v <<= 1;
                temp++;
            }
            v &= B52;
            v |= (long)(offset + 1022) << 52;
            if (value < 0) v |= 1 << 63;
            GetBytes(bytes, index, v);
        }

        public static bool ToBoolean(byte[] bytes, int index)
        {
            return bytes[index] == 1 ? true : false;
        }
        public static sbyte ToSByte(byte[] bytes, int index)
        {
            return (sbyte)bytes[index];
        }
        public static byte ToByte(byte[] bytes, int index)
        {
            return bytes[index];
        }
        public static short ToInt16(byte[] bytes, int index)
        {
            return (short)ToUInt16(bytes, index);
        }
        public static ushort ToUInt16(byte[] bytes, int index)
        {
            return (ushort)(bytes[index] | bytes[index + 1] << 8);
        }
        public static char ToChar(byte[] bytes, int index)
        {
            return (char)ToUInt16(bytes, index);
        }
        public static int ToInt32(byte[] bytes, int index)
        {
            return (bytes[index] | bytes[index + 1] << 8 | bytes[index + 1] << 16 | bytes[index + 1] << 24) & int.MaxValue;
        }
        public static uint ToUInt32(byte[] bytes, int index)
        {
            return (uint)ToInt32(bytes, index);
        }
        public static float ToSingle(byte[] bytes, int index)
        {
            //int value = (a4 << 24) | (a3 << 16) | (a2 << 8) | a1;
            //float result = ((value >> 31) == 0 ? 1 : -1) *
            //    // 最后23位
            //    (1 + (value & 8388607) / 8388607.0f)
            //    // 符号位 - 末尾位 中间的8位
            //    * (float)Math.Pow(2, (((value & 2139095040) >> 23) & 255) - 127);
            byte b1 = bytes[index], b2 = bytes[index + 1], b3 = bytes[index + 2], b4 = bytes[index + 3];
            if (b1 == 0 && b2 == 0 && b3 == 0 && b4 == 0)
                return 0;
            return (float)(((b4 >> 7) == 0 ? 1 : -1) *
                (1 + ((b3 & 127) << 16 | b2 << 8 | b1) * B23D) *
                (float)Math.Pow(2, ((b4 & 127) << 1 | b3 >> 7) - 127));
        }
        public static long ToInt64(byte[] bytes, int index)
        {
            return (long)bytes[index] | (long)bytes[index + 1] << 8 | (long)bytes[index + 2] << 16 | (long)bytes[index + 3] << 24
                | (long)bytes[index + 4] << 32 | (long)bytes[index + 5] << 40 | (long)bytes[index + 6] << 48 | (long)bytes[index + 7] << 56;
        }
        public static ulong ToUInt64(byte[] bytes, int index)
        {
            return (ulong)ToInt64(bytes, index);
        }
        public static double ToDouble(byte[] bytes, int index)
        {
            // 参照float
            byte b1 = bytes[index], b2 = bytes[index + 1], b3 = bytes[index + 2], b4 = bytes[index + 3],
                b5 = bytes[index + 4], b6 = bytes[index + 5], b7 = bytes[index + 6], b8 = bytes[index + 7];
            if (b1 == 0 && b2 == 0 && b3 == 0 && b4 == 0 && b5 == 0 && b6 == 0 && b7 == 0 && b8 == 0)
                return 0;
            return (double)(((b8 >> 7) == 0 ? 1 : -1) *
                (1 + ((long)(b7 & 15) << 48 | (long)b6 << 40 | (long)b5 << 32 | b4 << 24 | b3 << 16 | b2 << 8 | b1) * B52D) *
                Math.Pow(2, ((long)(b8 & 127) << 4 | b7 >> 4) - 1023));
        }
    }
    public class ByteWriter : Binary, IWriter
    {
        protected static Dictionary<Type, MethodInfo> genericMethods;

        static ByteWriter()
        {
            // todo: HTML5由于方法不能重名，genericMethods不能正确拿到泛型方法，导致序列化对泛型类型无效
            // 可能的解决方案: 针对泛型集合类进行private所有字段的序列化，里面为数组及一些数字
            Type type = typeof(ByteWriter);
            const string METHOD_NAME = "Write";
            genericMethods = type.GetMethods().Where(m =>
            {
                if (m.IsGenericMethod && m.Name == METHOD_NAME)
                {
                    ParameterInfo[] parameters = m.GetParameters();
                    if (parameters.Length == 1)
                    {
                        ParameterInfo param = parameters[0];
                        if (!param.ParameterType.IsArray && !param.ParameterType.IsGenericParameter)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }).ToDictionary(m => m.GetParameters()[0].ParameterType.GetGenericTypeDefinition());
        }

        public ByteWriter() : this(4) { }
        public ByteWriter(int capacity)
        {
            this.buffer = new byte[capacity];
        }
        public ByteWriter(byte[] buffer)
        {
            this.buffer = buffer;
        }
        public ByteWriter(byte[] buffer, int index)
        {
            this.buffer = buffer;
            this.index = index;
        }

        protected bool WriteSupportiveType(Type type, object value)
        {
            int index = Array.IndexOf(SupportiveTypes, type);
            if (index == -1) return false;
            switch (index)
            {
                case 0: Write((bool)value); break;
                case 1: Write((sbyte)value); break;
                case 2: Write((byte)value); break;
                case 3: Write((short)value); break;
                case 4: Write((ushort)value); break;
                case 5: Write((char)value); break;
                case 6: Write((int)value); break;
                case 7: Write((uint)value); break;
                case 8: Write((float)value); break;
                case 9: Write((long)value); break;
                case 10: Write((ulong)value); break;
                case 11: Write((double)value); break;
                case 12: Write((TimeSpan)value); break;
                case 13: Write((DateTime)value); break;
                case 14: Write((string)value); break;

                case 15: Write((bool[])value); break;
                case 16: Write((sbyte[])value); break;
                case 17: Write((byte[])value); break;
                case 18: Write((short[])value); break;
                case 19: Write((ushort[])value); break;
                case 20: Write((char[])value); break;
                case 21: Write((int[])value); break;
                case 22: Write((uint[])value); break;
                case 23: Write((float[])value); break;
                case 24: Write((long[])value); break;
                case 25: Write((ulong[])value); break;
                case 26: Write((double[])value); break;
                case 27: Write((TimeSpan[])value); break;
                case 28: Write((DateTime[])value); break;
                case 29: Write((string[])value); break;

                default: throw new NotImplementedException();
            }
            return true;
        }
        //protected bool WriteGenericType(Type type, object value)
        //{
        //    if (!type.IsGenericType) return false;
        //    Type typeDefinition = type.GetGenericTypeDefinition();
        //    if (typeDefinition.Equals(NULLABLE))
        //    {
        //        GetType().GetMethod("WriteNullable").MakeGenericMethod(type.GetGenericArguments()).Invoke(this, new object[] { value });
        //    }
        //    else
        //    {
        //        Type[] interfaces = type.GetInterfaces();
        //        for (int i = 0; i < interfaces.Length; i++)
        //        {
        //            if (interfaces[i].Name == COLLECTION.Name)
        //            {
        //                Type childType = null;
        //                if (interfaces[i].IsGenericType)
        //                {
        //                    childType = interfaces[i].GetGenericArguments()[0];
        //                }
        //                WriteCollection((ICollection)value, childType);
        //                break;
        //            }
        //        }
        //    }
        //    return true;
        //}
        public void EnsureCapacity(int count)
        {
            int min = index + count;
            if (buffer.Length < min)
            {
                int num = buffer.Length * 2;
                if (num < min)
                {
                    num = min;
                }

                byte[] clone = new byte[num];
                Utility.Copy(buffer, 0, clone, 0, index);
                this.buffer = clone;
            }
        }
        public object Output()
        {
            return GetBuffer();
        }
        public void WriteObject(object value)
        {
            if (value == null)
                return;
            WriteObject(value, value.GetType());
        }
        public virtual void WriteObject(object value, Type type)
        {
            MethodInfo method;
            if (type.IsGenericType && genericMethods.TryGetValue(type.GetGenericTypeDefinition(), out method))
            {
                method = method.MakeGenericMethod(type.GetGenericArguments());
                method.Invoke(this, new object[] { value });
            }
            else
            {
                if (type.IsEnum)
                {
                    Type underlyingType = Enum.GetUnderlyingType(type);
                    WriteObject(Convert.ChangeType(value, underlyingType), underlyingType);
                }
                else if (WriteSupportiveType(type, value))
                {
                }
                else if (type.IsArray)
                {
                    WriteArray((Array)value, type.GetElementType());
                }
                //else if (type.Equals(COLLECTION))
                //{
                //    WriteCollection((ICollection)value, null);
                //}
                else
                {
                    if (type.IsClass || type.IsInterface)
                    {
                        if (value == null)
                        {
                            Write(false);
                            return;
                        }
                        else if (IsAbstractType(value, ref type))
                        {
                            Write((byte)2);
                            //Write(value.GetType().AssemblyQualifiedName);
                            Write(type.SimpleAQName());
                        }
                        else
                        {
                            Write(true);
                        }
                    }
                    Setting.SerializeField(type,
                        field =>
                        {
                            WriteObject(field.GetValue(value), field.FieldType);
                        });
                    if (Setting.Property)
                    {
                        Setting.SerializeProperty(type,
                            property =>
                            {
                                WriteObject(property.GetValue(value, _SERIALIZE.EmptyObjects), property.PropertyType);
                            });
                    }
                }
            }
        }
        //public void WriteCollection(ICollection collection, Type elementType)
        //{
        //    if (collection == null)
        //    {
        //        Write(-1);
        //    }
        //    else
        //    {
        //        Write(collection.Count);
        //        foreach (var item in collection)
        //        {
        //            if (elementType == null)
        //            {
        //                if (item == null)
        //                    throw new ArgumentException("Collection的元素类型未知的情况下元素不能为null");
        //                elementType = item.GetType();
        //            }
        //            WriteObject(item, item == null ? elementType : item.GetType());
        //        }
        //    }
        //}
        //public void WriteArray(Array array)
        //{
        //}
        public void WriteArray<T>(T[] array)
        {
            if (array == null)
            {
                Write(-1);
            }
            else
            {
                var childType = typeof(T);
                Type type = childType.MakeArrayType();
                if (!WriteSupportiveType(type, array))
                {
                    int count = array.Length;
                    Write(count);
                    for (int i = 0; i < count; i++)
                    {
                        WriteObject(array.GetValue(i), childType);
                    }
                }
            }
        }
        public void WriteArray(Array array, Type type)
        {
            if (array == null)
            {
                Write(-1);
            }
            else
            {
                int count = array.Length;
                Write(count);
                for (int i = 0; i < count; i++)
                {
                    WriteObject(array.GetValue(i), type);
                }
            }
        }
        public void Write<T>(T value)
        {
            WriteObject(value, typeof(T));
        }
        public void Write<T, U>(Dictionary<T, U> value)
        {
            if (value == null)
            {
                Write(-1);
                //WriteArray((Array)null, typeof(EEKeyValuePair<T, U>));
                return;
            }
            EEKeyValuePair<T, U>[] array = new EEKeyValuePair<T, U>[value.Count];
            int index = 0;
            foreach (KeyValuePair<T, U> item in value)
            {
                array[index++] = new EEKeyValuePair<T, U>(item.Key, item.Value);
            }
            WriteArray(array);
        }
        public void Write<T>(List<T> value)
        {
            WriteArray(value == null ? null : value.ToArray());
        }
        public void Write<T>(Queue<T> value)
        {
            WriteArray(value == null ? null : value.ToArray());
        }
        public void Write<T>(Stack<T> value)
        {
            WriteArray(value == null ? null : value.ToArray());
        }
        public void Write<T>(LinkedList<T> value)
        {
            WriteArray(value == null ? null : value.ToArray());
        }
        public void Write<T>(Nullable<T> value) where T : struct
        {
            bool hasValue = value.HasValue;
            Write(hasValue);
            if (hasValue)
            {
                this.WriteObject(value.Value);
            }
        }
        public void Write(bool value)
        {
            Write((byte)(value ? 1 : 0));
        }
        public void Write(sbyte value)
        {
            int length = 1;
            EnsureCapacity(length);
            buffer[index] = (byte)value;
            index += length;
        }
        public void Write(byte value)
        {
            EnsureCapacity(1);
            buffer[index++] = value;
        }
        public void Write(short value)
        {
            int length = 2;
            EnsureCapacity(length);
            buffer[index] = (byte)value;
            buffer[index + 1] = (byte)(value >> 8);
            index += length;
        }
        public void Write(ushort value)
        {
            int length = 2;
            EnsureCapacity(length);
            buffer[index] = (byte)value;
            buffer[index + 1] = (byte)(value >> 8);
            index += length;
        }
        public void Write(char value)
        {
            Write((ushort)value);
        }
        public void Write(int value)
        {
            int length = 4;
            EnsureCapacity(length);
            buffer[index] = (byte)value;
            buffer[index + 1] = (byte)(value >> 8);
            buffer[index + 2] = (byte)(value >> 16);
            buffer[index + 3] = (byte)(value >> 24);
            index += length;
        }
        public void WriteVariableInt(int value)
        {
            if (value > -128 && value < 127)
            {
                this.Write((sbyte)value);
            }
            else
            {
                if (value >= -32768 && value <= 32767)
                {
                    this.Write((byte)127);
                    this.Write((short)value);
                }
                else
                {
                    this.Write((byte)128);
                    Write(value);
                }
            }
        }
        public void Write(uint value)
        {
            int length = 4;
            EnsureCapacity(length);
            buffer[index] = (byte)value;
            buffer[index + 1] = (byte)(value >> 8);
            buffer[index + 2] = (byte)(value >> 16);
            buffer[index + 3] = (byte)(value >> 24);
            index += length;
        }
        public void Write(float value)
        {
            int length = 4;
            EnsureCapacity(length);
            byte[] result = BitConverter.GetBytes(value);
            for (int i = 0; i < length; i++)
                buffer[index + i] = result[i];
            index += length;
        }
        public void Write(long value)
        {
            if (Long52Bit)
            {
                int length = 7;
                EnsureCapacity(length);
                //byte[] result = BitConverter.GetBytes(value);
#if HTML5
                buffer[index] = (byte)value;
                buffer[index + 1] = (byte)((value & 65280) >> 8);
                buffer[index + 2] = (byte)((value & 16711680) >> 16);
                buffer[index + 3] = (byte)((value / 16777216) & 255);
                buffer[index + 4] = (byte)((value / 4294967296L) & 255);
                buffer[index + 5] = (byte)((value / 1099511627776L) & 255);
                buffer[index + 6] = (byte)((value / 281474976710656L) & 15);
#else
                buffer[index] = (byte)value;
                buffer[index + 1] = (byte)(value >> 8);
                buffer[index + 2] = (byte)(value >> 16);
                buffer[index + 3] = (byte)(value >> 24);
                buffer[index + 4] = (byte)(value >> 32);
                buffer[index + 5] = (byte)(value >> 40);
                buffer[index + 6] = (byte)(value >> 48 & 15);
#endif
                index += length;
            }
            else
            {
                int length = 8;
                EnsureCapacity(length);
                //byte[] result = BitConverter.GetBytes(value);
                buffer[index] = (byte)value;
                buffer[index + 1] = (byte)(value >> 8);
                buffer[index + 2] = (byte)(value >> 16);
                buffer[index + 3] = (byte)(value >> 24);
                buffer[index + 4] = (byte)(value >> 32);
                buffer[index + 5] = (byte)(value >> 40);
                buffer[index + 6] = (byte)(value >> 48);
                buffer[index + 7] = (byte)(value >> 56);
                index += length;
            }
        }
        public void Write(ulong value)
        {
            if (Long52Bit)
            {
                int length = 7;
                EnsureCapacity(length);
                //byte[] result = BitConverter.GetBytes(value);
#if HTML5
                buffer[index] = (byte)value;
                buffer[index + 1] = (byte)((value & 65280) >> 8);
                buffer[index + 2] = (byte)((value & 16711680) >> 16);
                buffer[index + 3] = (byte)((value / 16777216) & 255);
                buffer[index + 4] = (byte)((value / 4294967296L) & 255);
                buffer[index + 5] = (byte)((value / 1099511627776L) & 255);
                buffer[index + 6] = (byte)((value / 281474976710656L) & 15);
#else
                buffer[index] = (byte)value;
                buffer[index + 1] = (byte)(value >> 8);
                buffer[index + 2] = (byte)(value >> 16);
                buffer[index + 3] = (byte)(value >> 24);
                buffer[index + 4] = (byte)(value >> 32);
                buffer[index + 5] = (byte)(value >> 40);
                buffer[index + 6] = (byte)(value >> 48 & 15);
#endif
                index += length;
            }
            else
            {
                int length = 8;
                EnsureCapacity(length);
                //byte[] result = BitConverter.GetBytes(value);
                buffer[index] = (byte)value;
                buffer[index + 1] = (byte)(value >> 8);
                buffer[index + 2] = (byte)(value >> 16);
                buffer[index + 3] = (byte)(value >> 24);
                buffer[index + 4] = (byte)(value >> 32);
                buffer[index + 5] = (byte)(value >> 40);
                buffer[index + 6] = (byte)(value >> 48);
                buffer[index + 7] = (byte)(value >> 56);
                index += length;
            }
        }
        public void Write(double value)
        {
            int length = 8;
            EnsureCapacity(length);
            byte[] result = BitConverter.GetBytes(value);
            for (int i = 0; i < length; i++)
                buffer[index + i] = result[i];
            index += length;
        }
        public void Write(TimeSpan value)
        {
            if (Long52Bit)
            {
#if HTML5
                Write(value.Ticks);
#else
                Write(value.Ticks / 10000);
#endif
            }
            else
            {
                Write(value.Ticks);
            }
        }
        public void Write(DateTime value)
        {
            if (Long52Bit)
            {
                int length = 7;
                EnsureCapacity(length);
                //byte[] result = BitConverter.GetBytes(value);

#if HTML5
                long v = value.Ticks;
                buffer[index] = (byte)v;
                buffer[index + 1] = (byte)((v & 65280) >> 8);
                buffer[index + 2] = (byte)((v & 16711680) >> 16);
                buffer[index + 3] = (byte)((v / 16777216) & 255);
                buffer[index + 4] = (byte)((v / 4294967296L) & 255);
                buffer[index + 5] = (byte)((v / 1099511627776L) & 255);
                buffer[index + 6] = (byte)((v / 281474976710656L) & 15);
#else
                long v = value.Ticks / 10000;
                buffer[index] = (byte)v;
                buffer[index + 1] = (byte)(v >> 8);
                buffer[index + 2] = (byte)(v >> 16);
                buffer[index + 3] = (byte)(v >> 24);
                buffer[index + 4] = (byte)(v >> 32);
                buffer[index + 5] = (byte)(v >> 40);
                buffer[index + 6] = (byte)((byte)(v >> 48 & 3) | ((int)value.Kind << 2));
#endif
                index += length;
            }
            else
            {
                Write(value.Ticks);
            }
        }
        public void Write(bool[] array)
        {
            if (array == null)
            {
                this.Write(-1);
            }
            else
            {
                int count = array.Length;
                Write(count);
                EnsureCapacity(count);
                for (int i = 0; i < count; i++)
                    buffer[index++] = (byte)(array[i] ? 1 : 0);
            }
        }
        public void Write(sbyte[] array)
        {
            if (array == null)
            {
                this.Write(-1);
            }
            else
            {
                int count = array.Length;
                Write(count);
                EnsureCapacity(count);
                for (int i = 0; i < count; i++)
                    buffer[index++] = (byte)array[i];
            }
        }
        public void Write(byte[] array)
        {
            if (array == null)
            {
                this.Write(-1);
            }
            else
            {
                int count = array.Length;
                Write(count);
                EnsureCapacity(count);
                //for (int i = 0; i < count; i++)
                //    buffer[index++] = array[i];
                Array.Copy(array, 0, this.buffer, index, array.Length);
                index += array.Length;
            }
        }
        public void WriteBytes(byte[] buffer, int offset, int size)
        {
            EnsureCapacity(size);
            Array.Copy(buffer, offset, this.buffer, index, size);
            index += size;
        }
        public void Write(short[] array)
        {
            if (array == null)
            {
                this.Write(-1);
            }
            else
            {
                int count = array.Length;
                int length = 2;
                Write(count);
                EnsureCapacity(count * length);
                for (int i = 0; i < count; i++)
                {
                    buffer[index++] = (byte)array[i];
                    buffer[index++] = (byte)(array[i] >> 8);
                }
            }
        }
        public void Write(ushort[] array)
        {
            if (array == null)
            {
                this.Write(-1);
            }
            else
            {
                int count = array.Length;
                int length = 2;
                Write(count);
                EnsureCapacity(count * length);
                for (int i = 0; i < count; i++)
                {
                    buffer[index++] = (byte)array[i];
                    buffer[index++] = (byte)(array[i] >> 8);
                }
            }
        }
        public void Write(char[] array)
        {
            if (array == null)
            {
                this.Write(-1);
            }
            else
            {
                int count = array.Length;
                int length = 2;
                Write(count);
                EnsureCapacity(count * length);
                for (int i = 0; i < count; i++)
                {
                    buffer[index++] = (byte)array[i];
                    buffer[index++] = (byte)(array[i] >> 8);
                }
            }
        }
        public void Write(string value)
        {
            if (value == null)
            {
                this.Write(-1);
            }
            else
            {
                Write(value.ToCharArray());
            }
        }
        /*
         * WriteVariableInt
         * 1byte -127 ~ 126
         * 3byte -32768 ~ 32767
         * 5byte 超过short到int最大值的部分
         * 写入时间是写入固定4byte的int的10倍
         */
        public void Write(int[] array)
        {
            if (array == null)
            {
                this.Write(-1);
            }
            else
            {
                int count = array.Length;
                int length = 4;
                Write(count);
                EnsureCapacity(count * length);
                for (int i = 0; i < count; i++)
                {
                    buffer[index++] = (byte)array[i];
                    buffer[index++] = (byte)(array[i] >> 8);
                    buffer[index++] = (byte)(array[i] >> 16);
                    buffer[index++] = (byte)(array[i] >> 24);
                }
            }
        }
        public void WriteVariableInt(int[] array)
        {
            if (array == null)
            {
                this.Write(-1);
            }
            else
            {
                int count = array.Length;
                Write(count);
                for (int i = 0; i < count; i++)
                    WriteVariableInt(array[i]);
            }
        }
        public void Write(uint[] array)
        {
            if (array == null)
            {
                this.Write(-1);
            }
            else
            {
                int count = array.Length;
                int length = 4;
                Write(count);
                EnsureCapacity(count * length);
                for (int i = 0; i < count; i++)
                {
                    buffer[index++] = (byte)array[i];
                    buffer[index++] = (byte)(array[i] >> 8);
                    buffer[index++] = (byte)(array[i] >> 16);
                    buffer[index++] = (byte)(array[i] >> 24);
                }
            }
        }
        public void Write(float[] array)
        {
            if (array == null)
            {
                this.Write(-1);
            }
            else
            {
                int count = array.Length;
                int length = 4;
                Write(count);
                EnsureCapacity(count * length);
                for (int i = 0; i < count; i++)
                {
                    byte[] result = BitConverter.GetBytes(array[i]);
                    for (int j = 0; j < 4; j++)
                        buffer[index++] = result[j];
                }
            }
        }
        public void Write(long[] array)
        {
            if (array == null)
            {
                this.Write(-1);
            }
            else
            {
                int count = array.Length;
                Write(count);
                if (Long52Bit)
                {
                    int length = 7;
                    EnsureCapacity(count * length);
                    for (int i = 0; i < count; i++)
                    {
                        buffer[index++] = (byte)array[i];
                        buffer[index++] = (byte)(array[i] >> 8);
                        buffer[index++] = (byte)(array[i] >> 16);
                        buffer[index++] = (byte)(array[i] >> 24);
                        buffer[index++] = (byte)(array[i] >> 32);
                        buffer[index++] = (byte)(array[i] >> 40);
                        buffer[index++] = (byte)(array[i] >> 48 & 15);
                    }
                }
                else
                {
                    int length = 8;
                    EnsureCapacity(count * length);
                    for (int i = 0; i < count; i++)
                    {
                        buffer[index++] = (byte)array[i];
                        buffer[index++] = (byte)(array[i] >> 8);
                        buffer[index++] = (byte)(array[i] >> 16);
                        buffer[index++] = (byte)(array[i] >> 24);
                        buffer[index++] = (byte)(array[i] >> 32);
                        buffer[index++] = (byte)(array[i] >> 40);
                        buffer[index++] = (byte)(array[i] >> 48);
                        buffer[index++] = (byte)(array[i] >> 56);
                    }
                }
            }
        }
        public void Write(ulong[] array)
        {
            if (array == null)
            {
                this.Write(-1);
            }
            else
            {
                int count = array.Length;
                Write(count);
                if (Long52Bit)
                {
                    int length = 7;
                    EnsureCapacity(count * length);
                    for (int i = 0; i < count; i++)
                    {
                        buffer[index++] = (byte)array[i];
                        buffer[index++] = (byte)(array[i] >> 8);
                        buffer[index++] = (byte)(array[i] >> 16);
                        buffer[index++] = (byte)(array[i] >> 24);
                        buffer[index++] = (byte)(array[i] >> 32);
                        buffer[index++] = (byte)(array[i] >> 40);
                        buffer[index++] = (byte)(array[i] >> 48 & 15);
                    }
                }
                else
                {
                    int length = 8;
                    EnsureCapacity(count * length);
                    for (int i = 0; i < count; i++)
                    {
                        buffer[index++] = (byte)array[i];
                        buffer[index++] = (byte)(array[i] >> 8);
                        buffer[index++] = (byte)(array[i] >> 16);
                        buffer[index++] = (byte)(array[i] >> 24);
                        buffer[index++] = (byte)(array[i] >> 32);
                        buffer[index++] = (byte)(array[i] >> 40);
                        buffer[index++] = (byte)(array[i] >> 48);
                        buffer[index++] = (byte)(array[i] >> 56);
                    }
                }
            }
        }
        public void Write(double[] array)
        {
            if (array == null)
            {
                this.Write(-1);
            }
            else
            {
                int count = array.Length;
                int length = 8;
                Write(count);
                EnsureCapacity(count * length);
                for (int i = 0; i < count; i++)
                {
                    byte[] result = BitConverter.GetBytes(array[i]);
                    for (int j = 0; j < 8; j++)
                        buffer[index++] = result[j];
                }
            }
        }
        public void Write(TimeSpan[] array)
        {
            if (array == null)
            {
                this.Write(-1);
            }
            else
            {
                int count = array.Length;
                Write(count);
                if (Long52Bit)
                {
                    int length = 7;
                    EnsureCapacity(count * length);
                    for (int i = 0; i < count; i++)
                    {
#if HTML5
                        var value = array[i].Ticks;
                        buffer[index] = (byte)value;
                        buffer[index + 1] = (byte)((value & 65280) >> 8);
                        buffer[index + 2] = (byte)((value & 16711680) >> 16);
                        buffer[index + 3] = (byte)((value / 16777216) & 255);
                        buffer[index + 4] = (byte)((value / 4294967296L) & 255);
                        buffer[index + 5] = (byte)((value / 1099511627776L) & 255);
                        buffer[index + 6] = (byte)((value / 281474976710656L) & 15);
#else
                        var value = array[i].Ticks / 10000;
                        buffer[index++] = (byte)value;
                        buffer[index++] = (byte)(value >> 8);
                        buffer[index++] = (byte)(value >> 16);
                        buffer[index++] = (byte)(value >> 24);
                        buffer[index++] = (byte)(value >> 32);
                        buffer[index++] = (byte)(value >> 40);
                        buffer[index++] = (byte)(value >> 48 & 15);
#endif
                    }
                }
                else
                {
                    int length = 8;
                    EnsureCapacity(count * length);
                    for (int i = 0; i < count; i++)
                    {
                        var value = array[i].Ticks;
                        buffer[index++] = (byte)value;
                        buffer[index++] = (byte)(value >> 8);
                        buffer[index++] = (byte)(value >> 16);
                        buffer[index++] = (byte)(value >> 24);
                        buffer[index++] = (byte)(value >> 32);
                        buffer[index++] = (byte)(value >> 40);
                        buffer[index++] = (byte)(value >> 48);
                        buffer[index++] = (byte)(value >> 56);
                    }
                }
            }
        }
        public void Write(DateTime[] array)
        {
            if (array == null)
            {
                this.Write(-1);
            }
            else
            {
                int count = array.Length;
                Write(count);
                if (Long52Bit)
                {
                    int length = 7;
                    EnsureCapacity(count * length);
                    for (int i = 0; i < count; i++)
                    {
#if HTML5
                        var value = array[i].Ticks;
                        buffer[index] = (byte)value;
                        buffer[index + 1] = (byte)((value & 65280) >> 8);
                        buffer[index + 2] = (byte)((value & 16711680) >> 16);
                        buffer[index + 3] = (byte)((value / 16777216) & 255);
                        buffer[index + 4] = (byte)((value / 4294967296L) & 255);
                        buffer[index + 5] = (byte)((value / 1099511627776L) & 255);
                        buffer[index + 6] = (byte)((value / 281474976710656L) & 15);
#else
                        var value = array[i].Ticks / 10000;
                        buffer[index++] = (byte)value;
                        buffer[index++] = (byte)(value >> 8);
                        buffer[index++] = (byte)(value >> 16);
                        buffer[index++] = (byte)(value >> 24);
                        buffer[index++] = (byte)(value >> 32);
                        buffer[index++] = (byte)(value >> 40);
                        buffer[index++] = (byte)((byte)(value >> 48 & 3) | ((int)array[i].Kind << 3));
#endif
                    }
                }
                else
                {
                    int length = 8;
                    EnsureCapacity(count * length);
                    for (int i = 0; i < count; i++)
                    {
                        var value = array[i].Ticks;
                        buffer[index++] = (byte)value;
                        buffer[index++] = (byte)(value >> 8);
                        buffer[index++] = (byte)(value >> 16);
                        buffer[index++] = (byte)(value >> 24);
                        buffer[index++] = (byte)(value >> 32);
                        buffer[index++] = (byte)(value >> 40);
                        buffer[index++] = (byte)(value >> 48);
                        buffer[index++] = (byte)(value >> 56);
                    }
                }
            }
        }
        public void Write(string[] array)
        {
            if (array == null)
            {
                this.Write(-1);
            }
            else
            {
                int count = array.Length;
                Write(count);
                for (int i = 0; i < count; i++)
                    Write(array[i]);
            }
        }

        [Obsolete("To use ByteWriter.Serialize")]
        public static byte[] WriteBinary(object value)
        {
            ByteWriter writer = new ByteWriter();
            writer.WriteObject(value);
            return writer.GetBuffer();
        }
        public static byte[] Serialize(object value)
        {
            if (value == null)
                return null;
            return Serialize(value, value.GetType(), SerializeSetting.DefaultSetting);
        }
        public static byte[] Serialize(object value, Type type)
        {
            return Serialize(value, type, SerializeSetting.DefaultSetting);
        }
        public static byte[] Serialize(object value, Type type, SerializeSetting setting)
        {
            if (value == null || type == null)
                throw new ArgumentNullException();
            ByteWriter writer = new ByteWriter();
            writer.Setting = setting;
            writer.WriteObject(value);
            return writer.GetBuffer();
        }
    }
    public class ByteReader : Binary, IReader
    {
        protected static Dictionary<Type, MethodInfo> genericMethods;
        static ByteReader()
        {
            Type type = typeof(ByteReader);
            const string METHOD_NAME = "Read";
            genericMethods = type.GetMethods().Where(m =>
            {
                if (m.IsGenericMethod && m.Name == METHOD_NAME)
                {
                    ParameterInfo[] parameters = m.GetParameters();
                    if (parameters.Length == 1)
                    {
                        Type param = parameters[0].ParameterType.GetElementType();
                        if (!param.IsArray && !param.IsGenericParameter)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }).ToDictionary(m => m.GetParameters()[0].ParameterType.GetElementType().GetGenericTypeDefinition());
        }

        public bool IsEnd
        {
            get { return Position >= buffer.Length; }
        }

        public ByteReader(byte[] buffer)
        {
            this.buffer = buffer;
        }
        public ByteReader(byte[] buffer, int offset)
        {
            this.buffer = buffer;
            Skip(offset);
        }

        protected object ReadSupportiveType(int index)
        {
            switch (index)
            {
                case 0: bool v0; Read(out v0); return v0;
                case 1: sbyte v1; Read(out v1); return v1;
                case 2: byte v2; Read(out v2); return v2;
                case 3: short v3; Read(out v3); return v3;
                case 4: ushort v4; Read(out v4); return v4;
                case 5: char v5; Read(out v5); return v5;
                case 6: int v6; Read(out v6); return v6;
                case 7: uint v7; Read(out v7); return v7;
                case 8: float v8; Read(out v8); return v8;
                case 9: long v9; Read(out v9); return v9;
                case 10: ulong v10; Read(out v10); return v10;
                case 11: double v11; Read(out v11); return v11;
                case 12: TimeSpan v12; Read(out v12); return v12;
                case 13: DateTime v13; Read(out v13); return v13;
                case 14: string v14; Read(out v14); return v14;

                case 15: bool[] v15; Read(out v15); return v15;
                case 16: sbyte[] v16; Read(out v16); return v16;
                case 17: byte[] v17; Read(out v17); return v17;
                case 18: short[] v18; Read(out v18); return v18;
                case 19: ushort[] v19; Read(out v19); return v19;
                case 20: char[] v20; Read(out v20); return v20;
                case 21: int[] v21; Read(out v21); return v21;
                case 22: uint[] v22; Read(out v22); return v22;
                case 23: float[] v23; Read(out v23); return v23;
                case 24: long[] v24; Read(out v24); return v24;
                case 25: ulong[] v25; Read(out v25); return v25;
                case 26: double[] v26; Read(out v26); return v26;
                case 27: TimeSpan[] v27; Read(out v27); return v27;
                case 28: DateTime[] v28; Read(out v28); return v28;
                case 29: string[] v29; Read(out v29); return v29;

                default: throw new NotImplementedException();
            }
        }
        public void Input(object data)
        {
            this.buffer = (byte[])data;
            this.index = 0;
        }
        public T ReadObject<T>()
        {
            return (T)ReadObject(typeof(T));
        }
        public void ReadTo<T>(T value)
        {
            ReadTo(ref value);
        }
        public void ReadTo<T>(ref T value)
        {
            //if (!hasReadType && Setting.AutoType)
            //    ReadType();

            Type type = typeof(T);
            if (type.IsClass || type.IsInterface)
            {
                byte hasValue;
                Read(out hasValue);
                if (hasValue == 0)
                {
                    value = default(T);
                }
                else if (hasValue == 2)
                {
                    string typeName;
                    Read(out typeName);
                    type = _SERIALIZE.LoadSimpleAQName(typeName);
                }
            }

            object instance = value;
            Setting.SerializeField(type,
                field =>
                {
                    field.SetValue(instance, ReadObject(field.FieldType));
                });
            if (Setting.Property)
            {
                Setting.SerializeProperty(type,
                    property =>
                    {
                        property.SetValue(instance, ReadObject(property.PropertyType), _SERIALIZE.EmptyObjects);
                    });
            }
        }
        public void Read<T>(out T value)
        {
            value = ReadObject<T>();
        }
        public virtual object ReadObject(Type type)
        {
            //if (!hasReadType && Setting.AutoType)
            //    ReadType();

            if (type.IsEnum)
            {
                return ReadObject(Enum.GetUnderlyingType(type));
            }
            else
            {
                int supportive = Array.IndexOf(SupportiveTypes, type);
                if (supportive != -1)
                {
                    return ReadSupportiveType(supportive);
                }
                else if (type.IsArray)
                {
                    Array array;
                    ReadArray(out array, type.GetElementType());
                    return array;
                }
                else
                {
                    // 这样虽然简便，但对于JS方法不能重名，这里不好生成反射，所以改成以上方法
                    //MethodInfo method = GetType().GetMethod("Read", new Type[] { type.MakeByRefType() });
                    //if (method != null)
                    //{
                    //    object[] value = new object[1];
                    //    method.Invoke(this, value);
                    //    return value[0];
                    //}
                    //else
                    MethodInfo method;
                    if (type.IsGenericType && genericMethods.TryGetValue(type.GetGenericTypeDefinition(), out method))
                    {
                        object[] value = new object[1];
                        method = method.MakeGenericMethod(type.GetGenericArguments());
                        method.Invoke(this, value);
                        return value[0];
                    }
                    else
                    {
                        if (type.IsClass || type.IsInterface)
                        {
                            byte hasValue;
                            Read(out hasValue);
                            if (hasValue == 0)
                            {
                                return null;
                            }
                            else if (hasValue == 2)
                            {
                                string typeName;
                                Read(out typeName);
                                Type _type = _SERIALIZE.LoadSimpleAQName(typeName);
                                if (_type != null)
                                    type = _type;
                            }
                        }
                        object value;
                        if (type.IsStatic())
                        {
                            value = null;
                        }
                        else
                        {
                            value = Activator.CreateInstance(type);
                            if (value == null)
                                throw new NotImplementedException(string.Format("can not create instance of {0}", type.FullName));
                        }
                        Setting.SerializeField(type,
                            field =>
                            {
                                field.SetValue(value, ReadObject(field.FieldType));
                            });
                        if (Setting.Property)
                        {
                            Setting.SerializeProperty(type,
                                property =>
                                {
                                    property.SetValue(value, ReadObject(property.PropertyType), _SERIALIZE.EmptyObjects);
                                });
                        }
                        return value;
                    }
                }
            }
        }
        /// <summary>
        /// 元素类型（非数组类型）
        /// </summary>
        /// <param name="array">要赋值的数组</param>
        /// <param name="type">数组元素类型，int[]则传typeof(int)</param>
        public void ReadArray(out Array array, Type type)
        {
            int count;
            Read(out count);
            if (count < 0)
            {
                array = null;
            }
            else
            {
                array = Array.CreateInstance(type, count);
                for (int i = 0; i < count; i++)
                {
                    array.SetValue(ReadObject(type), i);
                }
            }
        }
        public void Read<T>(out T[] array)
        {
            int count;
            Read(out count);
            if (count < 0)
            {
                array = null;
            }
            else
            {
                Type type = typeof(T);
                array = new T[count];
                for (int i = 0; i < count; i++)
                {
                    array[i] = (T)ReadObject(type);
                }
            }
        }
        public void Read<T, U>(out Dictionary<T, U> value)
        {
            Array temp;
            ReadArray(out temp, typeof(EEKeyValuePair<T, U>));
            if (temp == null)
            {
                value = null;
                return;
            }
            EEKeyValuePair<T, U>[] items = (EEKeyValuePair<T, U>[])temp;

            value = new Dictionary<T, U>();
            foreach (EEKeyValuePair<T, U> item in items)
            {
                value.Add(item.Key, item.Value);
            }
        }
        public void Read<T>(out List<T> value)
        {
            T[] array;
            Read<T>(out array);
            if (array == null)
                value = null;
            else
                value = new List<T>(array);
        }
        public void Read<T>(out Queue<T> value)
        {
            T[] array;
            Read<T>(out array);
            if (array == null)
                value = null;
            else
                value = new Queue<T>(array);
        }
        public void Read<T>(out Stack<T> value)
        {
            T[] array;
            Read<T>(out array);
            if (array == null)
                value = null;
            else
                value = new Stack<T>(array);
        }
        public void Read<T>(out LinkedList<T> value)
        {
            T[] array;
            Read<T>(out array);
            if (array == null)
                value = null;
            else
                value = new LinkedList<T>(array);
        }
        public void Read<T>(out Nullable<T> value) where T : struct
        {
            bool hasValue;
            Read(out hasValue);
            if (hasValue)
            {
                value = ReadObject<T>();
            }
            else
            {
                value = null;
            }
        }
        //public void Read<T>(out Nullable<T>[] array) where T : struct
        //{
        //    int count;
        //    Read(out count);
        //    if (count < 0)
        //    {
        //        array = null;
        //    }
        //    else
        //    {
        //        array = new Nullable<T>[count];
        //        for (int i = 0; i < count; i++)
        //        {
        //            Read(out array[i]);
        //        }
        //    }
        //}
        public void Read(out bool value)
        {
            value = buffer[index] != 0;
            index++;
        }
        public void Read(out sbyte value)
        {
            value = (sbyte)buffer[index];
            index++;
        }
        public void Read(out byte value)
        {
            value = buffer[index++];
        }
        public void Read(out short value)
        {
            value = BitConverter.ToInt16(buffer, index);
            index += 2;
        }
        public void Read(out ushort value)
        {
            value = BitConverter.ToUInt16(buffer, index);
            index += 2;
        }
        public void Read(out char value)
        {
            ushort temp;
            Read(out temp);
            value = (char)temp;
        }
        public void Read(out int value)
        {
            value = BitConverter.ToInt32(buffer, index);
            index += 4;

            //fixed (byte* ptr = buffer)
            //{
            //    value = *(int*)&ptr[index];
            //    index += 4;
            //}
        }
        public int ReadVariableInt()
        {
            int value;
            sbyte v;
            Read(out v);
            if (v == -128)
            {
                Read(out value);
            }
            else
            {
                if (v == 127)
                {
                    short i;
                    Read(out i);
                    value = (int)i;
                }
                else
                {
                    value = (int)v;
                }
            }
            return value;
        }
        public void Read(out uint value)
        {
            value = BitConverter.ToUInt32(buffer, index);
            index += 4;
        }
        public void Read(out float value)
        {
            //fixed (byte* ptr = buffer)
            //{
            //    value = *(float*)&ptr[index];
            //    index += 4;
            //}

            value = BitConverter.ToSingle(buffer, index);
            index += 4;
        }
        public void Read(out long value)
        {
            if (Long52Bit)
            {
                //long l0 = buffer[index];
                //long l1 = buffer[index + 1] << 8;
                //long l2 = buffer[index + 2] << 16;
                //long l3 = (long)buffer[index + 3] << 24;
                //long l4 = (long)buffer[index + 4] << 32;
                //long l5 = (long)buffer[index + 5] << 40;
                //long l6 = (long)buffer[index + 6] << 48;
                //value = l0 | l1 | l2 | l3 | l4 | l5 | l6;
#if HTML5
                value = buffer[index] + (buffer[index + 1] << 8) + (buffer[index + 2] << 16) + buffer[index + 3] * 16777216L
                    + buffer[index + 4] * 4294967296L + buffer[index + 5] * 1099511627776L + buffer[index + 6] * 281474976710656L;
#else
                value = (long)buffer[index] | (long)buffer[index + 1] << 8 | (long)buffer[index + 2] << 16 | (long)buffer[index + 3] << 24
                    | (long)buffer[index + 4] << 32 | (long)buffer[index + 5] << 40 | (long)buffer[index + 6] << 48;
#endif
                index += 7;
            }
            else
            {
                value = BitConverter.ToInt64(buffer, index);
                index += 8;
            }
        }
        public void Read(out ulong value)
        {
            if (Long52Bit)
            {
                //long l0 = buffer[index];
                //long l1 = buffer[index + 1] << 8;
                //long l2 = buffer[index + 2] << 16;
                //long l3 = (long)buffer[index + 3] << 24;
                //long l4 = (long)buffer[index + 4] << 32;
                //long l5 = (long)buffer[index + 5] << 40;
                //long l6 = (long)buffer[index + 6] << 48;
                //value = (ulong)(l0 | l1 | l2 | l3 | l4 | l5 | l6);
#if HTML5
                value = (ulong)(buffer[index] + (buffer[index + 1] << 8) + (buffer[index + 2] << 16) + buffer[index + 3] * 16777216L
                    + buffer[index + 4] * 4294967296L + buffer[index + 5] * 1099511627776L + buffer[index + 6] * 281474976710656L);
#else
                value = (ulong)((long)buffer[index] | (long)buffer[index + 1] << 8 | (long)buffer[index + 2] << 16 | (long)buffer[index + 3] << 24
                    | (long)buffer[index + 4] << 32 | (long)buffer[index + 5] << 40 | (long)buffer[index + 6] << 48);
#endif
                index += 7;
            }
            else
            {
                value = BitConverter.ToUInt64(buffer, index);
                index += 8;
            }
        }
        public void Read(out double value)
        {
            //fixed (byte* ptr = buffer)
            //{
            //    value = *(double*)&ptr[index];
            //    index += 8;
            //}

            value = BitConverter.ToDouble(buffer, index);
            index += 8;
        }
        public void Read(out TimeSpan value)
        {
            long ticks;
            Read(out ticks);
            if (Long52Bit)
            {
#if HTML5
                value = new TimeSpan(ticks);
#else
                value = new TimeSpan(ticks * 10000);
#endif
            }
            else
                value = new TimeSpan(ticks);
        }
        public void Read(out DateTime value)
        {
            long ticks;
            Read(out ticks);
            if (Long52Bit)
            {
#if HTML5
                value = new DateTime(ticks);
#else
                value = new DateTime(((ticks & 1125899906842623L) * 10000), (DateTimeKind)(ticks >> 50));
#endif
            }
            else
                value = new DateTime(ticks);
        }
        public void Read(out bool[] array)
        {
            int count;
            Read(out count);
            if (count < 0)
            {
                array = null;
            }
            else
            {

                array = new bool[count];
                for (int i = 0; i < count; i++)
                    array[i] = buffer[index++] != 0;
            }
        }
        public void Read(out sbyte[] array)
        {
            int count;
            Read(out count);
            if (count < 0)
            {
                array = null;
            }
            else
            {
                array = new sbyte[count];
                for (int i = 0; i < count; i++)
                    array[i] = (sbyte)buffer[index++];
            }
        }
        public void Read(out byte[] array)
        {
            int count;
            Read(out count);
            if (count < 0)
            {
                array = null;
            }
            else
            {
                array = new byte[count];
                //for (int i = 0; i < count; i++)
                //    array[i] = buffer[index++];
                Array.Copy(buffer, index, array, 0, count);
                index += count;
            }
        }
        public void Read(out short[] array)
        {
            int count;
            Read(out count);
            if (count < 0)
            {
                array = null;
            }
            else
            {
                array = new short[count];
                for (int i = 0; i < count; i++)
                    Read(out array[i]);
            }
        }
        public void Read(out ushort[] array)
        {
            int count;
            Read(out count);
            if (count < 0)
            {
                array = null;
            }
            else
            {
                array = new ushort[count];
                for (int i = 0; i < count; i++)
                    Read(out array[i]);
            }
        }
        public void Read(out char[] array)
        {
            int count;
            Read(out count);
            if (count < 0)
            {
                array = null;
            }
            else
            {
                array = new char[count];
                for (int i = 0; i < count; i++)
                    Read(out array[i]);
            }
        }
        public void Read(out string value)
        {
            char[] array;
            Read(out array);
            if (array == null)
                value = null;
            else
                value = new string(array);
        }
        public void Read(out int[] array)
        {
            int count;
            Read(out count);
            if (count < 0)
            {
                array = null;
            }
            else
            {
                array = new int[count];
                for (int i = 0; i < count; i++)
                    Read(out array[i]);
            }
        }
        public void ReadVariableInt(out int[] array)
        {
            int count;
            Read(out count);
            if (count < 0)
            {
                array = null;
            }
            else
            {
                array = new int[count];
                for (int i = 0; i < count; i++)
                    array[i] = ReadVariableInt();
            }
        }
        public void Read(out uint[] array)
        {
            int count;
            Read(out count);
            if (count < 0)
            {
                array = null;
            }
            else
            {
                array = new uint[count];
                for (int i = 0; i < count; i++)
                    Read(out array[i]);
            }
        }
        public void Read(out float[] array)
        {
            int count;
            Read(out count);
            if (count < 0)
            {
                array = null;
            }
            else
            {
                array = new float[count];
                for (int i = 0; i < count; i++)
                    Read(out array[i]);
            }
        }
        public void Read(out long[] array)
        {
            int count;
            Read(out count);
            if (count < 0)
            {
                array = null;
            }
            else
            {
                array = new long[count];
                for (int i = 0; i < count; i++)
                    Read(out array[i]);
            }
        }
        public void Read(out ulong[] array)
        {
            int count;
            Read(out count);
            if (count < 0)
            {
                array = null;
            }
            else
            {
                array = new ulong[count];
                for (int i = 0; i < count; i++)
                    Read(out array[i]);
            }
        }
        public void Read(out double[] array)
        {
            int count;
            Read(out count);
            if (count < 0)
            {
                array = null;
            }
            else
            {
                array = new double[count];
                for (int i = 0; i < count; i++)
                    Read(out array[i]);
            }
        }
        public void Read(out TimeSpan[] array)
        {
            int count;
            Read(out count);
            if (count < 0)
            {
                array = null;
            }
            else
            {
                array = new TimeSpan[count];
                for (int i = 0; i < count; i++)
                    Read(out array[i]);
            }
        }
        public void Read(out DateTime[] array)
        {
            int count;
            Read(out count);
            if (count < 0)
            {
                array = null;
            }
            else
            {
                array = new DateTime[count];
                for (int i = 0; i < count; i++)
                    Read(out array[i]);
            }
        }
        public void Read(out string[] array)
        {
            int count;
            Read(out count);
            if (count < 0)
            {
                array = null;
            }
            else
            {
                array = new string[count];
                for (int i = 0; i < count; i++)
                    Read(out array[i]);
            }
        }
        public byte[] PeekBytes(int length)
        {
            byte[] array = new byte[length];
            buffer.Copy(index, array, 0, length);
            return array;
        }
        public byte[] ReadBytes(int length)
        {
            byte[] array = new byte[length];
            buffer.Copy(index, array, 0, length);
            index += length;
            return array;
        }
        public byte[] GetBuffer(int count)
        {
            byte[] flush = new byte[_MATH.Min(count, buffer.Length - index)];
            Utility.Copy(buffer, index, flush, 0, flush.Length);
            return flush;
        }
        public override byte[] GetBuffer()
        {
            return GetBuffer(buffer.Length - index);
        }

        [Obsolete("To use ByteReader.Serialize")]
        public static T ReadBinary<T>(byte[] buffer)
        {
            return new ByteReader(buffer).ReadObject<T>();
        }
        public static object Deserialize(byte[] buffer, Type type)
        {
            return Deserialize(buffer, type, SerializeSetting.DefaultSetting);
        }
        public static object Deserialize(byte[] buffer, Type type, SerializeSetting setting)
        {
            if (buffer == null)
                return null;
            if (type == null)
                throw new ArgumentNullException();
            ByteReader reader = new ByteReader(buffer);
            reader.Setting = setting;
            return reader.ReadObject(type);
        }
        public static T Deserialize<T>(byte[] buffer)
        {
            return (T)Deserialize(buffer, typeof(T));
        }
        public static T Deserialize<T>(byte[] buffer, SerializeSetting setting)
        {
            return (T)Deserialize(buffer, typeof(T), setting);
        }
    }

    /// <summary>支持的引用：对象，对象数组</summary>
	public class ByteRefWriter : IWriter
	{
        class INNER_WRITER : ByteWriter
        {
            public ByteRefWriter PARENT;
            private Dictionary<object, int> objs = new Dictionary<object, int>();
            private Dictionary<Type, int> types = new Dictionary<Type, int>();

            private int WriteRef<T>(T target)
            {
                if (target == null)
                    return -1;

                int refIndex;
                if (objs.TryGetValue(target, out refIndex))
                {
                    return refIndex;
                }
                else
                {
                    objs[target] = index;
                    return -1;
                }
            }
            private void WriteKey(string key, Type type)
            {
                byte length = (byte)key.Length;
                EnsureCapacity(length + 1);
                buffer[index++] = length;
                foreach (var c in key)
                {
                    if (c > 255)
                        throw new NotSupportedException("Key name can use alphabet only.");
                    buffer[index++] = (byte)c;
                }
                int refIndex;
                if (types.TryGetValue(type, out refIndex))
                    Write(refIndex);
                else
                {
                    types[type] = index;
                    Write(-index);
                    // 短AQ名在加密方式加载下的程序集中还是未能通过Type.GetType获得类型
                    byte[] typeName = Encoding.UTF8.GetBytes(type.SimpleAQName());
                    //byte[] typeName = Encoding.UTF8.GetBytes(type.AssemblyQualifiedName);
                    //Write(type.SimpleAQName());
                    Write(typeName);
                }
                //Write(type.AssemblyQualifiedName);
            }
            public override void WriteObject(object value, Type type)
            {
                if (PARENT.onSerialize.Count > 0)
                {
                    for (int i = 0; i < PARENT.onSerialize.Count; i++)
                        if (PARENT.onSerialize[i](PARENT, value, type))
                            return;
                }
                //if (PARENT.OnSerialize != null)
                //{
                //    var list = PARENT.OnSerialize.GetInvocationList();
                //    if (list.Length == 1)
                //    {
                //        if (PARENT.OnSerialize(PARENT, value, type))
                //            return;
                //    }
                //    else
                //    {
                //        for (int i = 0; i < list.Length; i++)
                //            if (((Func<ByteRefWriter, object, Type, bool>)list[i])(PARENT, value, type))
                //                return;
                //    }
                //}
                //if (PARENT.OnSerialize != null && PARENT.OnSerialize(PARENT, value, type))
                //    return;

                MethodInfo method;
                if (type.IsGenericType && genericMethods.TryGetValue(type.GetGenericTypeDefinition(), out method))
                {
                    method = method.MakeGenericMethod(type.GetGenericArguments());
                    method.Invoke(this, new object[] { value });
                }
                else
                {
                    if (type.IsEnum)
                    {
                        WriteObject(value, Enum.GetUnderlyingType(type));
                    }
                    else if (WriteSupportiveType(type, value))
                    {
                    }
                    else if (type.IsArray)
                    {
                        int refIndex = WriteRef(value);
                        if (refIndex == -1)
                        {
                            Write(false);
                            WriteArray((Array)value, type.GetElementType());
                        }
                        else
                        {
                            Write(true);
                            Write(refIndex);
                        }
                    }
                    else
                    {
                        if (type.IsClass || type.IsInterface)
                        {
                            int refIndex = WriteRef(value);
                            if (refIndex == -1)
                            {
                                if (value == null)
                                {
                                    Write(-1);
                                    return;
                                }
                                else if (IsAbstractType(value, ref type))
                                {
                                    if (index == 0)
                                    {
                                        // 防止和引用的0冲突
                                        Write(-3);
                                    }
                                    else
                                    {
                                        Write(-index);
                                    }
                                    Write(type.AssemblyQualifiedName);
                                    //Write(type.SimpleAQName());
                                }
                                else
                                {
                                    Write(index);
                                }
                            }
                            else
                            {
                                Write(refIndex);
                                return;
                            }
                        }
                        else
                        {
                            // 对结构也写入4字节索引，当结构体丢失时可以正确解读丢失结构体类型的数据
                            Write(-2);
                        }
                        Setting.SerializeField(type,
                            field =>
                            {
                                WriteKey(field.Name, field.FieldType);
                                WriteObject(field.GetValue(value), field.FieldType);
                            });
                        if (Setting.Property)
                        {
                            Setting.SerializeProperty(type,
                                property =>
                                {
                                    WriteKey(property.Name, property.PropertyType);
                                    WriteObject(property.GetValue(value, _SERIALIZE.EmptyObjects), property.PropertyType);
                                });
                        }
                        Write((byte)0);
                    }
                }
            }
        }

        /// <summary>自定义类型序列化，若进行了序列化，应该返回true</summary>
        //public Func<ByteRefWriter, object, Type, bool> OnSerialize;
        private List<Func<ByteRefWriter, object, Type, bool>> onSerialize = new List<Func<ByteRefWriter, object, Type, bool>>();
        private INNER_WRITER writer;

        public List<Func<ByteRefWriter, object, Type, bool>> OnSerialize { get { return onSerialize; } }

        public ByteRefWriter()
        {
            writer = new INNER_WRITER();
            writer.Long52Bit = false;
            writer.PARENT = this;
        }
        public ByteRefWriter(SerializeSetting setting) : this()
        {
            writer.Setting = setting;
        }

        public void AddOnSerialize(Func<ByteRefWriter, object, Type, bool> method)
        {
            if (method == null) return;
            if (!onSerialize.Contains(method))
            {
                onSerialize.Add(method);
            }
        }
        public void SetOnSerialize(Func<ByteRefWriter, object, Type, bool> method)
        {
            onSerialize.Clear();
            onSerialize.Add(method);
        }
        public void RemoveOnSerialize(Func<ByteRefWriter, object, Type, bool> method)
        {
            onSerialize.Remove(method);
        }
        public void ClearOnSerialize()
        {
            onSerialize.Clear();
        }

        public void Write<T>(T obj)
        {
            WriteObject(obj, typeof(T));
        }
        public void WriteObject(object value, Type type)
        {
            writer.WriteObject(value, type);
        }
        public byte[] GetRawBuffer()
        {
            return writer.Buffer;
        }
        public byte[] GetRawBuffer(out int count)
        {
            count = writer.Position;
            return writer.Buffer;
        }
        public byte[] GetBuffer()
        {
            return writer.GetBuffer();
        }
        object IWriter.Output()
        {
            return GetBuffer();
        }

        public static byte[] Serialize(object value)
        {
            if (value == null)
                return null;
            return Serialize(value, value.GetType(), SerializeSetting.DefaultSetting, null);
        }
        public static byte[] Serialize(object value, Type type)
        {
            return Serialize(value, type, SerializeSetting.DefaultSetting, null);
        }
        public static byte[] Serialize(object value, Type type, Func<ByteRefWriter, object, Type, bool> onSerialize)
        {
            return Serialize(value, type, SerializeSetting.DefaultSetting, onSerialize);
        }
        public static byte[] Serialize(object value, Type type, SerializeSetting setting)
        {
            return Serialize(value, type, setting, null);
        }
        public static byte[] Serialize(object value, Type type, SerializeSetting setting, Func<ByteRefWriter, object, Type, bool> onSerialize)
        {
            if (value == null || type == null)
                throw new ArgumentNullException();
            if (value != null && type == null)
                type = value.GetType();
            ByteRefWriter writer = new ByteRefWriter();
            writer.AddOnSerialize(onSerialize);
            writer.writer.Setting = setting;
            writer.WriteObject(value, type);
            return writer.GetBuffer();
        }
    }
    public class ByteRefReader : IReader
    {
        class AsyncReadField
        {
            public VariableObject Field;
            public AsyncData<object> Async;
            public Type ValueType;
            public AsyncReadField()
            {
            }
            public AsyncReadField(VariableObject field, AsyncData<object> async)
            {
                this.Field = field;
                this.Async = async;
            }
            public void SetValue()
            {
                try
                {
                    object value = Async.Data;
                    if (ValueType != null && ValueType != Field.Type)
                    {
                        value = Convert.ChangeType(value, Field.Type);
                    }
                    Field.SetValue(value);
                }
                catch (Exception ex)
                {
                    _LOG.Warning("AsyncReadField set value error. Type = {0}, Field = {1}, Ex = {2}", Field.Type.FullName, Field.VariableName, ex.Message);
                }
            }
        }
        class INNER_READER : ByteReader
        {
            internal VariableObject ReadingField;
            internal ByteRefReader PARENT;
            private Dictionary<int, object> objs = new Dictionary<int, object>();
            private Dictionary<int, Type> types = new Dictionary<int, Type>();

            public INNER_READER(byte[] buffer) : base(buffer) { }

            /// <summary>由于读取数组不一样，需要先读取引用，所以封装成内部类，仅允许ReadObject操作</summary>
            public override object ReadObject(Type type)
            {
                if (PARENT.onDeserialize.Count > 0)
                {
                    Func<ByteRefReader, object> _read = null;
                    for (int i = 0; i < PARENT.onDeserialize.Count; i++)
                    {
                        _read = PARENT.onDeserialize[i](type, ReadingField);
                        if (_read != null)
                            return _read(PARENT);
                    }
                }
                //if (PARENT.OnDeserialize != null)
                //{
                //    var list = PARENT.OnDeserialize.GetInvocationList();
                //    Func<ByteRefReader, object> _read = null;
                //    if (list.Length == 1)
                //        _read = PARENT.OnDeserialize(type);
                //    else
                //    {
                //        for (int i = 0; i < list.Length; i++)
                //        {
                //            _read = ((Func<Type, Func<ByteRefReader, object>>)list[i])(type);
                //            if (_read != null)
                //                break;
                //        }
                //    }
                //    // 以上方法主要GetInvocationList费时
                //    //var _read = PARENT.OnDeserialize(type);
                //    if (_read != null)
                //        return _read(PARENT);
                //}

                if (type.IsEnum)
                {
                    return ReadObject(Enum.GetUnderlyingType(type));
                }
                else
                {
                    int supportive = Array.IndexOf(SupportiveTypes, type);
                    if (supportive != -1)
                    {
                        return ReadSupportiveType(supportive);
                    }
                    else if (type.IsArray)
                    {
                        bool hasRef;
                        Read(out hasRef);
                        if (hasRef)
                        {
                            int refIndex;
                            Read(out refIndex);
                            return objs[refIndex];
                        }
                        else
                        {
                            Array array;
                            ReadArray(out array, type.GetElementType());
                            return array;
                        }
                    }
                    else
                    {
                        MethodInfo method;
                        if (type.IsGenericType && genericMethods.TryGetValue(type.GetGenericTypeDefinition(), out method))
                        {
                            // todo: 读取数组和基类不一样，需要先读取一个bool值（未知解决与否）

                            object[] value = new object[1];
                            method = method.MakeGenericMethod(type.GetGenericArguments());
                            method.Invoke(this, value);
                            return value[0];
                        }
                        else
                        {
                            object value;
                            if (ReadObject(out value, ref type))
                                return value;
                            ReadTo(type, value);
                            return value;
                        }
                    }
                }
                //return InternalReadObject(type, null);
            }
            private string ReadKey(out Type type)
            {
                type = null;
                byte length;
                Read(out length);
                if (length == 0)
                    return null;
                char[] chars = new char[length];
                for (int i = 0; i < length; i++)
                    chars[i] = (char)buffer[index++];

                int refIndex;
                Read(out refIndex);
                if (refIndex > 0)
                {
                    if (!types.TryGetValue(refIndex, out type))
                        throw new KeyNotFoundException("Type ref can't be found.");
                }
                else
                {
                    //string typeName;
                    //Read(out typeName);
                    byte[] bytes;
                    Read(out bytes);
                    string typeName = Encoding.UTF8.GetString(bytes);
                    type = _SERIALIZE.LoadSimpleAQName(typeName);
                    types[-refIndex] = type;
                }

                return new string(chars);
            }
            internal bool ReadObject(out object value, ref Type type)
            {
                value = null;
                int index;
                Read(out index);
                if (index == -1)
                {
                    return true;
                }
                else if (index == -2)
                {
                    // struct
                }
                else
                {
                    if (index >= 0)
                    {
                        // index：对象引用
                        if (objs.TryGetValue(index, out value))
                            return true;
                    }
                    else
                    {
                        // -index：特殊特性
                        string typeName;
                        Read(out typeName);
                        type = _SERIALIZE.LoadSimpleAQName(typeName);
                        // TYPE_DELETED: 此类型已经不存在
                    }
                }

                // TYPE_DELETED
                if (type == null)
                {
                    if (index != -1 && index != -2)
                        objs[_MATH.Abs(index)] = value;

                    // 读取完所有遗失类型的数据
                    while (true)
                    {
                        Type valueType;
                        string key = ReadKey(out valueType);
                        if (key == null)
                            break;

                        if (valueType == null)
                        {
                            // 递归读取丢失的类型数据
                            ReadObject(out value, ref type);
                        }
                        else
                        {
                            ReadObject(valueType);
                        }
                    }
                    return true;
                }

                // 类型未丢失
                if (type.IsStatic())
                {
                    value = null;
                }
                else
                {
                    value = Activator.CreateInstance(type);
                    //if (value == null)
                    //    throw new NotImplementedException(string.Format("can not create instance of {0}", type.FullName));
                }

                if (index != -1 && index != -2)
                    objs[_MATH.Abs(index)] = value;

                return false;
            }
            internal void ReadTo(Type type, object value)
            {
                var fields = Setting.GetFields(type);
                var properties = Setting.GetProperties(type);
                while (true)
                {
                    Type valueType;
                    string key = ReadKey(out valueType);
                    if (key == null)
                        break;
                    // TYPE_DELETED
                    if (valueType == null)
                    {
                        object lost;
                        ReadObject(out lost, ref valueType);
                        continue;
                    }

                    FieldInfo field = fields.FirstOrDefault(f => f.Name == key);
                    if (field == null)
                    {
                        object result;
                        if (properties == null)
                        {
                            ReadingField = null;
                            ReadObject(valueType);
                            continue;
                        }
                        PropertyInfo property = properties.FirstOrDefault(p => p.Name == key);
                        if (property != null)
                        {
                            ReadingField = new VariableObject(value, property);
                            result = ReadObject(valueType);
                            if (result is AsyncData<object>)
                            {
                                PARENT.AsyncRead((AsyncData<object>)result);
                                PARENT.async[PARENT.async.Count - 1].ValueType = valueType;
                            }
                            else
                            {
                                try
                                {
                                    if (valueType != property.PropertyType)
                                        result = Convert.ChangeType(result, property.PropertyType);
                                    property.SetValue(value, result, _SERIALIZE.EmptyObjects);
                                }
                                catch (Exception ex)
                                {
                                    // 类型转换失败时不赋值
                                    _LOG.Warning("Property set value error. Type = {0}, Field = {1}, Ex = {2}", property.PropertyType.FullName, property.Name, ex.Message);
                                }
                            }
                        }
                    }
                    else
                    {
                        ReadingField = new VariableObject(value, field);
                        object result = ReadObject(valueType);
                        if (result is AsyncData<object>)
                        {
                            PARENT.AsyncRead((AsyncData<object>)result);
                            PARENT.async[PARENT.async.Count - 1].ValueType = valueType;
                        }
                        else
                        {
                            try
                            {
                                if (valueType != field.FieldType)
                                    result = Convert.ChangeType(result, field.FieldType);
                                field.SetValue(value, result);
                            }
                            catch (Exception ex)
                            {
                                // 类型转换失败时不赋值
                                _LOG.Warning("Field set value error. Type = {0}, Field = {1}, Ex = {2}", field.FieldType.FullName, field.Name, ex.Message);
                            }
                        }
                    }
                }
                ReadingField = null;
            }
        }

        //public Func<Type, Func<ByteRefReader, object>> OnDeserialize;
        private List<AsyncReadField> async = new List<AsyncReadField>();
        private List<Func<Type, VariableObject, Func<ByteRefReader, object>>> onDeserialize = new List<Func<Type, VariableObject, Func<ByteRefReader, object>>>();
        private INNER_READER reader;

        public List<Func<Type, VariableObject, Func<ByteRefReader, object>>> OnDeserialize { get { return onDeserialize; } }

        public ByteRefReader(byte[] buffer)
        {
            reader = new INNER_READER(buffer);
            reader.Long52Bit = false;
            reader.PARENT = this;
        }
        public ByteRefReader(byte[] buffer, SerializeSetting setting): this(buffer)
        {
            reader.Setting = setting;
        }

        public void AsyncRead(AsyncData<object> data)
        {
            if (reader.ReadingField == null) throw new InvalidOperationException("No reading field can't invoke async read.");
            for (int i = 0; i < async.Count; i++)
                if (async[i].Async == data)
                    return;
            this.async.Add(new AsyncReadField(reader.ReadingField, data));
        }
        public bool AsyncIsComplete()
        {
            if (async.Count == 0)
                return true;
            else
            {
                for (int i = async.Count - 1; i >= 0; i--)
                {
                    if (async[i].Async.IsEnd)
                    {
                        if (async[i].Async.IsSuccess)
                            async[i].SetValue();
                        async.RemoveAt(i);
                    }
                }
                return async.Count == 0;
            }
        }

        public void AddOnDeserialize(Func<Type, VariableObject, Func<ByteRefReader, object>> method)
        {
            if (method == null) return;
            if (!onDeserialize.Contains(method))
            {
                onDeserialize.Add(method);
            }
        }
        public void SetOnDeserialize(Func<Type, VariableObject, Func<ByteRefReader, object>> method)
        {
            onDeserialize.Clear();
            onDeserialize.Add(method);
        }
        public void RemoveOnDeserialize(Func<Type, VariableObject, Func<ByteRefReader, object>> method)
        {
            onDeserialize.Remove(method);
        }
        public void ClearOnDeserialize()
        {
            onDeserialize.Clear();
        }

        public object ReadObject(Type type)
        {
            return reader.ReadObject(type);
        }
        public void ReadTo<T>(T value)
        {
            ReadTo(ref value);
        }
        public void ReadTo<T>(ref T value)
        {
            Type type = typeof(T);
            object obj;
            if (reader.ReadObject(out obj, ref type))
                return;
            //reader.Read(out index);
            reader.ReadTo(type, value);
        }

        public static object Deserialize(byte[] buffer, Type type)
        {
            return Deserialize(buffer, type, SerializeSetting.DefaultSerializeProperty, null);
        }
        public static object Deserialize(byte[] buffer, Type type, Func<Type, VariableObject, Func<ByteRefReader, object>> onDeserialize)
        {
            return Deserialize(buffer, type, SerializeSetting.DefaultSerializeProperty, onDeserialize);
        }
        public static object Deserialize(byte[] buffer, Type type, SerializeSetting setting, Func<Type, VariableObject, Func<ByteRefReader, object>> onDeserialize)
        {
            if (buffer == null)
                return null;
            if (type == null)
                throw new ArgumentNullException();
            ByteRefReader reader = new ByteRefReader(buffer);
            reader.AddOnDeserialize(onDeserialize);
            reader.reader.Setting = setting;
            return reader.ReadObject(type);
        }
        public static T Deserialize<T>(byte[] buffer)
        {
            return (T)Deserialize(buffer, typeof(T));
        }
        public static T Deserialize<T>(byte[] buffer, Func<Type, VariableObject, Func<ByteRefReader, object>> onDeserialize)
        {
            return (T)Deserialize(buffer, typeof(T), onDeserialize);
        }
        public static T Deserialize<T>(byte[] buffer, SerializeSetting setting, Func<Type, VariableObject, Func<ByteRefReader, object>> onDeserialize)
        {
            return (T)Deserialize(buffer, typeof(T), setting, onDeserialize);
        }
    }
}
