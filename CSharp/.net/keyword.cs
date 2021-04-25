using __System;
using __System.Collections.Generic;
using __System.Linq;
using __System.Text;

[AInvariant]public partial class @object
{
    //return this.GetType().ToString();
    [ASystemAPI][AInvariant]public extern virtual string ToString();
    [AInvariant]public virtual bool Equals(object obj)
    {
        return this == obj;
    }
    public static bool Equals(object objA, object objB)
    {
        return objA == objB || (objA != null && objB != null && objA.Equals(objB));
    }
    public static bool ReferenceEquals(object objA, object objB)
    {
        return objA == objB;
    }
    [ASystemAPI][AInvariant]public extern virtual int GetHashCode();
    [ASystemAPI]public extern Type GetType();
}
[AInvariant]public struct @bool
{
    public static bool Parse(string value)
    {
        bool result = false;
        if (!bool.TryParse(value, out result))
            throw new FormatException("Format_BadBoolean");
        return result;
    }
    public static bool TryParse(string value, out bool result)
    {
        result = false;
        if (value == null)
        {
            return false;
        }
        if ("True" == value || "true" == value)
        {
            result = true;
            return true;
        }
        if ("False" == value || "false" == value)
        {
            result = false;
            return true;
        }
        return false;
    }
}
[AInvariant]public struct @sbyte
{
    public const sbyte MaxValue = 127;
    public const sbyte MinValue = -128;
    public static sbyte Parse(string s)
    {
        sbyte result;
        if (!TryParse(s, out result))
            throw new FormatException("Format_BadNumber");
        return result;
    }
    public static bool TryParse(string s, out sbyte result)
    {
        long r;
        bool flag = long.TryParse(s, out r);
        result = (sbyte)r;
        return flag;
    }
}
[AInvariant]public struct @byte
{
    public const byte MaxValue = 255;
    public const byte MinValue = 0;
    public static byte Parse(string s)
    {
        byte result;
        if (!TryParse(s, out result))
            throw new FormatException("Format_BadNumber");
        return result;
    }
    public static bool TryParse(string s, out byte result)
    {
        ulong r;
        bool flag = ulong.TryParse(s, out r);
        result = (byte)r;
        return flag;
    }
}
[AInvariant]public struct @short
{
    public const short MaxValue = 32767;
    public const short MinValue = -32768;
    public static short Parse(string s)
    {
        short result;
        if (!TryParse(s, out result))
            throw new FormatException("Format_BadNumber");
        return result;
    }
    public static bool TryParse(string s, out short result)
    {
        long r;
        bool flag = long.TryParse(s, out r);
        result = (short)r;
        return flag;
    }
}
[AInvariant]public struct @ushort
{
    public const ushort MaxValue = 65535;
    public const ushort MinValue = 0;
    public static ushort Parse(string s)
    {
        ushort result;
        if (!TryParse(s, out result))
            throw new FormatException("Format_BadNumber");
        return result;
    }
    public static bool TryParse(string s, out ushort result)
    {
        ulong r;
        bool flag = ulong.TryParse(s, out r);
        result = (ushort)r;
        return flag;
    }
}
[AInvariant]public struct @char
{
    public const char MaxValue = '￿';
    public const char MinValue = '\0';
    public static char Parse(string s)
    {
        if (s == null)
        {
            throw new ArgumentNullException("s");
        }
        if (s.Length != 1)
        {
            throw new FormatException("Format_NeedSingleChar");
        }
        return s[0];
    }
    public static bool TryParse(string s, out char result)
    {
        result = '\0';
        if (s == null)
        {
            return false;
        }
        if (s.Length != 1)
        {
            return false;
        }
        result = s[0];
        return true;
    }
    public static bool IsWhiteSpace(char c)
    {
        return c == ' ' || (c >= '\t' && c <= '\r') || c == '\u00a0' || c == '\u0085';
    }
}
[AInvariant]public struct @int
{
    public const int MaxValue = 2147483647;
    public const int MinValue = -2147483648;
    public static int Parse(string s)
    {
        int result;
        if (!TryParse(s, out result))
            throw new FormatException("Format_BadNumber");
        return result;
    }
    public static bool TryParse(string s, out int result)
    {
        long r;
        bool flag = long.TryParse(s, out r);
        result = (int)r;
        return flag;
    }
}
[AInvariant]public struct @uint
{
    public const uint MaxValue = 4294967295u;
    public const uint MinValue = 0;
    public static uint Parse(string s)
    {
        uint result;
        if (!TryParse(s, out result))
            throw new FormatException("Format_BadNumber");
        return result;
    }
    public static bool TryParse(string s, out uint result)
    {
        ulong r;
        bool flag = ulong.TryParse(s, out r);
        result = (uint)r;
        return flag;
    }
}
[AInvariant]public partial struct @float
{
    public const float Epsilon = 1.4013e-045f;
    public const float MaxValue = 3.40282e+038f;
    public const float MinValue = -3.40282e+038f;
    public const float NaN = 0.0f / 0.0f;
    public const float NegativeInfinity = -1.0f / 0.0f;
    public const float PositiveInfinity = 1.0f / 0.0f;
    [ASystemAPI]public static bool IsNaN(float f)
    {
        return f == NaN;
    }
    public static float Parse(string s)
    {
        float result;
        if (!TryParse(s, out result))
            throw new FormatException("Format_BadNumber");
        return result;
    }
    public static bool TryParse(string s, out float result)
    {
        double r;
        bool flag = double.TryParse(s, out r);
        result = (float)r;
        return flag;
    }
}
[AInvariant]public struct @long
{
    public const long MaxValue = 9223372036854775807L;
    public const long MinValue = -9223372036854775808L;
    public static long Parse(string s)
    {
        long result;
        if (!TryParse(s, out result))
            throw new FormatException("Format_BadNumber");
        return result;
    }
    public static bool TryParse(string s, out long result)
    {
        result = 0;
        bool minusFlag = false;
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (i == 0 && c == '-') minusFlag = true;
            else if (c >= '0' && c <= '9') result = result * 10 + (c - '0');
            else return false;
        }
        if (minusFlag) result = -result;
        return true;
    }
}
[AInvariant]public struct @ulong
{
    public const ulong MaxValue = 18446744073709551615uL;
    public const ulong MinValue = 0;
    public static ulong Parse(string s)
    {
        ulong result;
        if (!TryParse(s, out result))
            throw new FormatException("Format_BadNumber");
        return result;
    }
    public static bool TryParse(string s, out ulong result)
    {
        result = 0;
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (c >= '0' && c <= '9') result = result * 10 + (byte)(c - '0');
            else return false;
        }
        return true;
    }
}
[AInvariant]public partial struct @double
{
    public static double Epsilon = 4.94066e-324;
    public static double MaxValue = 1.79769e+308;
    public static double MinValue = -1.79769e+308;
    public static double NaN;
    public static double NegativeInfinity;
    public static double PositiveInfinity;
    [ASystemAPI]public static bool IsNaN(double f)
    {
        return f == NaN;
    }
    public static double Parse(string s)
    {
        double result;
        if (!TryParse(s, out result))
            throw new FormatException("Format_BadNumber");
        return result;
    }
    public static bool TryParse(string s, out double result)
    {
        result = 0;
        bool minusFlag = false;
        double dcm = 0;
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (i == 0 && c == '-') minusFlag = true;
            else if (c == '.')
            {
                if (dcm != 0)
                    return false;
                dcm = 1;
            }
            else if (c >= '0' && c <= '9')
            {
                if (dcm != 0)
                    result += (dcm *= 0.1) * (c - '0');
                else
                    result = result * 10 + (c - '0');
            }
            else return false;
        }
        if (minusFlag) result = -result;
        return true;
    }
}
//public struct @decimal
//{
//    public const decimal MaxValue = 79228162514264337593543950335m;
//    public const decimal MinValue = -79228162514264337593543950335m;
//    public static decimal Parse(string s)
//    {
//        throw new Exception();
//    }
//    public static bool TryParse(string s, out decimal result)
//    {
//        throw new Exception();
//    }
//}
[AInvariant]public partial class @string : IEnumerable<char>
{
    public static readonly string Empty = "";

#if !DEBUG
    [AInvariant]public extern @string(char[] chars);
    [AInvariant]public extern @string(char[] chars, int start, int length);
#endif

    /* Length将会生成到JS的String类型的prototype里
     * 方法内部调用系统API的实现函数时，实现的系统函数返回this.length
     * 此时this默认是String类型的对象，所以可以成功返回值
     */
    [ASystemAPI]public int Length
    {
        get { throw new NotImplementedException(); }
    }
    [ASystemAPI]public char this[int index]
    {
        get { throw new NotImplementedException(); }
    }
    public int CompareTo(string strB)
    {
#if !DEBUG
        return CompareOrdinal(this, strB);
#else
        throw new NotImplementedException();
#endif
    }
    public bool Contains(char value)
    {
        return IndexOf(value) >= 0;
    }
    public bool Contains(string value)
    {
        return IndexOf(value, 0) >= 0;
    }
    public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
    {
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                destination[destinationIndex + i] = this[sourceIndex + i];
            }
        }
    }
    public bool EndsWith(string value)
    {
#if !DEBUG
        if (value.Length > this.Length)
            return false;
        if (this == value)
            return true;
        if (value.Length == 0)
            return true;
        for (int i = this.Length - 1, j = value.Length - 1; j >= 0; i--, j--)
            if (this[i] != value[j])
                return false;
#endif
        return true;
    }
    public override bool Equals(object obj)
    {
        if (this == obj) return true;
        if (obj == null) return false;
        string text = obj.ToString();
        if (this.Length != text.Length) return false;
        for (int i = 1, len = this.Length; i < len; i++)
            if (this[i] != text[i])
                return false;
        return true;
    }
    public override int GetHashCode()
    {
        int num = 5381;
        int num2 = num;
        int num3;
        int index = 0;
        while (index < this.Length)
        {
            num3 = (int)this[index];
            num = ((num << 5) + num ^ num3);
            if (index + 1 == this.Length)
                break;
            num3 = (int)this[index + 1];
            num2 = ((num2 << 5) + num2 ^ num3);
            index += 2;
        }
        return (num + num2 * 1566083941) & int.MaxValue;
    }
    public int IndexOf(char value)
    {
        return IndexOf(value, 0);
    }
    [AInvariant]public int IndexOf(char value, int startIndex)
    {
        for (int i = startIndex, n = this.Length; i < n; i++)
            if (this[i] == value)
                return i;
        return -1;
    }
    public int IndexOf(string value)
    {
        return IndexOf(value, 0);
    }
    public int IndexOf(string value, int startIndex)
    {
        if (value == null)
            return -1;
        int len2 = value.Length;
        if (len2 == 0)
            return -1;
        if (startIndex + len2 > this.Length)
            return -1;
        int end = this.Length - len2;
        while (startIndex <= end)
        {
            if (this[startIndex] == value[0])
            {
                int i;
                for (i = 1; i < len2; i++)
                    if (value[i] != this[startIndex + i])
                        break;
                if (i == len2)
                    return startIndex;
            }
            startIndex++;
        }
        return -1;
    }
    public string Insert(int index, string value)
    {
#if !DEBUG
        if (value == null || value.Length == 0)
            return this;
#endif
        var len1 = this.Length;
        var len2 = value.Length;
        char[] chars = new char[len1 + len2];
        this.CopyTo(0, chars, 0, len1);
        value.CopyTo(0, chars, len1, len2);
        return new string(chars);
    }
    public int LastIndexOf(char value)
    {
        for (int i = this.Length - 1; i >= 0; i--)
            if (this[i] == value)
                return i;
        return -1;
    }
    public int LastIndexOf(string value)
    {
        if (value == null)
            return -1;
        int len2 = value.Length;
        if (len2 == 0)
            return -1;
        if (len2 > this.Length)
            return -1;
        int startIndex = this.Length - value.Length;
        while (startIndex >= 0)
        {
            if (this[startIndex] == value[0])
            {
                int i;
                for (i = 1; i < len2; i++)
                    if (value[i] != this[startIndex + i])
                        break;
                if (i == len2)
                    return startIndex;
            }
            startIndex--;
        }
        return -1;
    }
    public string Remove(int startIndex, int count)
    {
        if (startIndex == 0 && count == this.Length)
            return Empty;
        char[] chars = new char[this.Length - count];
        this.CopyTo(0, chars, 0, startIndex);
        int index = startIndex + count;
        this.CopyTo(index, chars, startIndex, this.Length - index);
        return new string(chars);
    }
    public string Remove(int startIndex)
    {
        return this.Substring(0, startIndex);
    }
    public string Replace(char oldChar, char newChar)
    {
        char[] chars = this.ToCharArray();
        for (int i = 0, n = chars.Length; i < n; i++)
            if (chars[i] == oldChar)
                chars[i] = newChar;
        return new string(chars);
    }
    public string Replace(string oldValue, string newValue)
    {
#if !DEBUG
        if (IsNullOrEmpty(oldValue)) return this;
        if (newValue == null) newValue = Empty;
        int len = oldValue.Length;
        if (len > this.Length) return this;
        int len2 = newValue.Length;
        StringBuilder builder = StringBuilderCache.Acquire(len2 <= len ? this.Length : (len2 - len) * 8);
        char[] newValueChars = newValue.ToCharArray();
        int i = 0;
        for (int n = this.Length - oldValue.Length; i <= n; i++)
        {
            if (this[i] == oldValue[0])
            {
                int j;
                for (j = 1; j < len; j++)
                    if (oldValue[j] != this[i + j])
                        break;
                if (j == len)
                {
                    builder.Append(newValueChars);
                    i += len - 1;
                    continue;
                }
            }
            builder.Append(this[i]);
        }
        for (; i < this.Length; i++)
            builder.Append(this[i]);
        return StringBuilderCache.GetStringAndRelease(builder);
#else
        throw new NotImplementedException();
#endif
    }
    [AInvariant]public string[] Split(params char[] separator)
    {
#if !DEBUG
        int[] sepList = new int[this.Length];
        int num = this.MakeSeparatorList(separator, sepList);
        if (num == 0)
        {
            return new string[] { this };
        }
        //if (flag)
        //{
        //    return this.InternalSplitOmitEmptyEntries(sepList, null, num, count);
        //}
        return this.InternalSplitKeepEmptyEntries(sepList, null, num);
#else
        throw new NotImplementedException();
#endif
    }
    public string[] Split(char[] separator, StringSplitOptions options)
    {
#if !DEBUG
        var result = Split(separator);
        if (options == StringSplitOptions.RemoveEmptyEntries)
        {
            int emptyCount = 0;
            for (int i = 0; i < result.Length; i++)
                if (IsNullOrEmpty(result[i]))
                    emptyCount++;
            if (emptyCount > 0)
            {
                string[] result2 = new string[result.Length - emptyCount];
                int offset = emptyCount;
                for (int i = 0; i < result.Length; i++)
                {
                    if (IsNullOrEmpty(result[i]))
                    {
                        emptyCount--;
                        continue;
                    }
                    result2[i + offset - emptyCount] = result[i];
                }
                result = result2;
            }
        }
        return result;
#else
        throw new NotImplementedException();
#endif
    }
    private int MakeSeparatorList(char[] separator, int[] sepList)
    {
        int num = 0;
        int num3 = sepList.Length;
        int num4 = separator.Length;
        char[] chars = this.ToCharArray();
        for (int i = 0, n = this.Length; i < n && num < num3; i++)
        {
            for (int j = 0; j < num4; j++)
            {
                if (chars[i] == separator[j])
                {
                    sepList[num++] = i;
                    break;
                }
            }
        }
        return num;
    }
    //private string[] InternalSplitOmitEmptyEntries(int[] sepList, int[] lengthList, int numReplaces, int count)
    //{
    //    int num = (numReplaces < count) ? (numReplaces + 1) : count;
    //    string[] array = new string[num];
    //    int num2 = 0;
    //    int num3 = 0;
    //    int i = 0;
    //    while (i < numReplaces && num2 < this.Length)
    //    {
    //        if (sepList[i] - num2 > 0)
    //        {
    //            array[num3++] = this.Substring(num2, sepList[i] - num2);
    //        }
    //        num2 = sepList[i] + ((lengthList == null) ? 1 : lengthList[i]);
    //        if (num3 == count - 1)
    //        {
    //            while (i < numReplaces - 1)
    //            {
    //                if (num2 != sepList[++i])
    //                {
    //                    break;
    //                }
    //                num2 += ((lengthList == null) ? 1 : lengthList[i]);
    //            }
    //            break;
    //        }
    //        i++;
    //    }
    //    if (num2 < this.Length)
    //    {
    //        array[num3++] = this.Substring(num2);
    //    }
    //    string[] array2 = array;
    //    if (num3 != num)
    //    {
    //        array2 = new string[num3];
    //        for (int j = 0; j < num3; j++)
    //        {
    //            array2[j] = array[j];
    //        }
    //    }
    //    return array2;
    //}
    private string[] InternalSplitKeepEmptyEntries(int[] sepList, int[] lengthList, int numReplaces)
    {
        int num = 0;
        int num2 = 0;
        int num3 = numReplaces;
        string[] array = new string[num3 + 1];
        int num4 = 0;
        while (num4 < num3 && num < this.Length)
        {
            array[num2++] = this.Substring(num, sepList[num4] - num);
            num = sepList[num4] + ((lengthList == null) ? 1 : lengthList[num4]);
            num4++;
        }
        if (num < this.Length && num3 >= 0)
        {
            array[num2] = this.Substring(num);
        }
        else
        {
            if (num2 == num3)
            {
                array[num2] = string.Empty;
            }
        }
        return array;
    }
    public bool StartsWith(string value)
    {
#if !DEBUG
        if (value.Length > this.Length)
            return false;
        if (this == value)
            return true;
        if (value.Length == 0)
            return true;
        for (int i = 0; i < value.Length; i++)
            if (this[i] != value[i])
                return false;
#endif
        return true;
    }
    public string Substring(int startIndex)
    {
        return Substring(startIndex, Length - startIndex);
    }
    [AInvariant]public string Substring(int startIndex, int length)
    {
#if !DEBUG
        if (length == 0)
        {
            return string.Empty;
        }
        if (startIndex == 0 && length == this.Length)
        {
            return this;
        }
        char[] result = new char[length];
        this.CopyTo(startIndex, result, 0, length);
        return new string(result);
#else
        throw new NotImplementedException();
#endif
    }
    public char[] ToCharArray()
    {
        return ToCharArray(0, this.Length);
    }
    public char[] ToCharArray(int startIndex, int length)
    {
        char[] array = new char[length];
        if (length > 0)
        {
            for (int i = startIndex, n = startIndex + length; i < n; i++)
            {
                array[i] = this[i];
            }
        }
        return array;
    }
    public string Trim()
    {
#if !DEBUG
        int num = this.Length - 1;
        int num2 = 0;
        //if (trimType != 1)
        {
            num2 = 0;
            while (num2 < this.Length && char.IsWhiteSpace(this[num2]))
            {
                num2++;
            }
        }
        //if (trimType != 0)
        {
            num = this.Length - 1;
            while (num >= num2 && char.IsWhiteSpace(this[num]))
            {
                num--;
            }
        }

        int result = num - num2 + 1;
        if (result == this.Length)
        {
            return this;
        }
        if (result == 0)
        {
            return string.Empty;
        }

        return this.Substring(num2, result);
#else
        throw new NotImplementedException();
#endif
    }
#if !DEBUG
    public override string ToString()
    {
        return this;
    }
#endif

    public static bool IsNullOrEmpty(string value)
    {
        return value == null || value.Length == 0;
    }
    public static string Format(string format, params object[] args)
    {
        return StringBuilderCache.GetStringAndRelease(StringBuilderCache.Acquire(format.Length + args.Length * 8).AppendFormatHelper(format, new ParamsArray(args)));
    }
    public static string Join(string separator, string[] values)
    {
        if (values == null)
        {
            throw new ArgumentNullException("values");
        }
        if (values.Length == 0 || values[0] == null)
        {
            return string.Empty;
        }
        if (separator == null)
        {
            separator = string.Empty;
        }
        StringBuilder stringBuilder = StringBuilderCache.Acquire(16);
        string text = values[0];
        if (text != null)
        {
            stringBuilder.Append(text);
        }
        for (int i = 1; i < values.Length; i++)
        {
            stringBuilder.Append(separator);
            if (values[i] != null)
            {
                text = values[i];
                if (text != null)
                {
                    stringBuilder.Append(text);
                }
            }
        }
        return StringBuilderCache.GetStringAndRelease(stringBuilder);
    }
    public static int CompareOrdinal(string strA, string strB)
    {
        if (strA == strB)
            return 0;
        if (strA == null)
            return -1;
        if (strB == null)
            return 1;
        if (strA.Length == strB.Length && strA.Length == 0)
            return 0;
        if (strA[0] - strB[0] != '\0')
            return (int)(strA[0] - strB[0]);
        int len = strA.Length < strB.Length ? strA.Length : strB.Length;
        for (int i = 1; i < len; i++)
        {
            int v = (strA[i] | 97) - (strB[i] | 97);
            if (v == 0)
                continue;
            else if (v > 0)
                return -1;
            else
                return 1;
        }
        return strA.Length - strB.Length;
    }
    /// <summary>用于取代new string(char[] chars)，毕竟构造函数比较难通用化，由new string(chars)表达式转换成调用此函数String.Create(chars)</summary>
    [ASystemAPI][AInvariant][ANonOptimize]internal extern static string Create(char[] chars);

    struct Enumerator : IEnumerator<char>
    {
        private string value;
        private int index;
        public Enumerator(string str)
        {
            this.value = str;
            this.index = -1;
        }
        public char Current
        {
            get { return value[index]; }
        }
        public bool MoveNext()
        {
            index++;
            return index < value.Length;
        }
        public void Reset()
        {
            index = -1;
        }
        public void Dispose()
        {
            value = null;
            index = 0;
        }
    }
    public IEnumerator<char> GetEnumerator()
    {
#if !DEBUG
        return new Enumerator(this);
#else
        throw new NotImplementedException();
#endif
        
    }
}