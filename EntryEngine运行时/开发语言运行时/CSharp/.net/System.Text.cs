using System;

namespace __System.Text
{
    public sealed class StringBuilder
    {
        internal char[] m_ChunkChars;
        internal StringBuilder m_ChunkPrevious;
        internal int m_ChunkLength;
        internal int m_ChunkOffset;
        internal int m_MaxCapacity;
        internal const int DefaultCapacity = 16;
        public int Capacity
        {
            get
            {
                return this.m_ChunkChars.Length + this.m_ChunkOffset;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("ArgumentOutOfRange_NegativeCapacity");
                }
                if (value > this.MaxCapacity)
                {
                    throw new ArgumentOutOfRangeException("ArgumentOutOfRange_Capacity");
                }
                if (value < this.Length)
                {
                    throw new ArgumentOutOfRangeException("ArgumentOutOfRange_SmallCapacity");
                }
                if (this.Capacity != value)
                {
                    char[] array = new char[value - this.m_ChunkOffset];
                    Array.Copy(this.m_ChunkChars, array, this.m_ChunkLength);
                    this.m_ChunkChars = array;
                }
            }
        }
        public int MaxCapacity
        {
            get
            {
                return this.m_MaxCapacity;
            }
        }
        public int Length
        {
            get
            {
                return this.m_ChunkOffset + this.m_ChunkLength;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("ArgumentOutOfRange_NegativeLength");
                }
                if (value > this.MaxCapacity)
                {
                    throw new ArgumentOutOfRangeException("ArgumentOutOfRange_SmallCapacity");
                }
                int capacity = this.Capacity;
                if (value == 0 && this.m_ChunkPrevious == null)
                {
                    this.m_ChunkLength = 0;
                    this.m_ChunkOffset = 0;
                    return;
                }
                int num = value - this.Length;
                if (num > 0)
                {
                    this.Append('\0', num);
                    return;
                }
                StringBuilder stringBuilder = this.FindChunkForIndex(value);
                if (stringBuilder != this)
                {
                    char[] array = new char[capacity - stringBuilder.m_ChunkOffset];
                    Array.Copy(stringBuilder.m_ChunkChars, array, stringBuilder.m_ChunkLength);
                    this.m_ChunkChars = array;
                    this.m_ChunkPrevious = stringBuilder.m_ChunkPrevious;
                    this.m_ChunkOffset = stringBuilder.m_ChunkOffset;
                }
                this.m_ChunkLength = value - stringBuilder.m_ChunkOffset;
            }
        }
        public char this[int index]
        {
            get
            {
                StringBuilder stringBuilder = this;
                int num;
                while (true)
                {
                    num = index - stringBuilder.m_ChunkOffset;
                    if (num >= 0)
                    {
                        break;
                    }
                    stringBuilder = stringBuilder.m_ChunkPrevious;
                    if (stringBuilder == null)
                    {
                        throw new IndexOutOfRangeException();
                    }
                }
                if (num >= stringBuilder.m_ChunkLength)
                {
                    throw new IndexOutOfRangeException();
                }
                return stringBuilder.m_ChunkChars[num];
            }
            set
            {
                StringBuilder stringBuilder = this;
                int num;
                while (true)
                {
                    num = index - stringBuilder.m_ChunkOffset;
                    if (num >= 0)
                    {
                        break;
                    }
                    stringBuilder = stringBuilder.m_ChunkPrevious;
                    if (stringBuilder == null)
                    {
                        throw new ArgumentOutOfRangeException("ArgumentOutOfRange_Index");
                    }
                }
                if (num >= stringBuilder.m_ChunkLength)
                {
                    throw new ArgumentOutOfRangeException("ArgumentOutOfRange_Index");
                }
                stringBuilder.m_ChunkChars[num] = value;
            }
        }
        public StringBuilder()
            : this(16)
        {
        }
        public StringBuilder(int capacity)
            : this(string.Empty, capacity)
        {
        }
        public StringBuilder(string value)
            : this(value, 16)
        {
        }
        public StringBuilder(string value, int capacity)
        {
            this.m_MaxCapacity = 2147483647;
            if (capacity == 0)
            {
                capacity = 16;
            }
            this.m_ChunkChars = new char[capacity];
            int length = value == null ? 0 : value.Length;
            this.m_ChunkLength = length;
            for (int i = 0; i < length; i++)
                this.m_ChunkChars[i] = value[i];
        }
        private void VerifyClassInvariant()
        {
            StringBuilder stringBuilder = this;
            int arg_08_0 = this.m_MaxCapacity;
            while (true)
            {
                StringBuilder chunkPrevious = stringBuilder.m_ChunkPrevious;
                if (chunkPrevious == null)
                {
                    break;
                }
                stringBuilder = chunkPrevious;
            }
        }
        public int EnsureCapacity(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException("ArgumentOutOfRange_NegativeCapacity");
            }
            if (this.Capacity < capacity)
            {
                this.Capacity = capacity;
            }
            return this.Capacity;
        }
        public override string ToString()
        {
            if (this.Length == 0)
            {
                return string.Empty;
            }
            char[] text = new char[this.Length];
            StringBuilder stringBuilder = this;
            while (true)
            {
                if (stringBuilder.m_ChunkLength > 0)
                {
                    char[] chunkChars = stringBuilder.m_ChunkChars;
                    int chunkOffset = stringBuilder.m_ChunkOffset;
                    int chunkLength = stringBuilder.m_ChunkLength;
                    if ((ulong)(chunkLength + chunkOffset) > (ulong)((long)text.Length) || chunkLength > chunkChars.Length)
                    {
                        break;
                    }
                    Array.Copy(chunkChars, 0, text, chunkOffset, chunkLength);
                }
                stringBuilder = stringBuilder.m_ChunkPrevious;
                if (stringBuilder == null)
                {
                    return new string(text);
                }
            }
            throw new ArgumentOutOfRangeException("ArgumentOutOfRange_Index");
        }
        public StringBuilder Clear()
        {
            this.Length = 0;
            return this;
        }
        public StringBuilder Append(char value, int repeatCount)
        {
            if (repeatCount < 0)
            {
                throw new ArgumentOutOfRangeException("ArgumentOutOfRange_NegativeCount");
            }
            if (repeatCount == 0)
            {
                return this;
            }
            int num = this.m_ChunkLength;
            while (repeatCount > 0)
            {
                if (num < this.m_ChunkChars.Length)
                {
                    this.m_ChunkChars[num++] = value;
                    repeatCount--;
                }
                else
                {
                    this.m_ChunkLength = num;
                    this.ExpandByABlock(repeatCount);
                    num = 0;
                }
            }
            this.m_ChunkLength = num;
            return this;
        }
        public StringBuilder Append(string value)
        {
            return Append(value.ToCharArray());
        }
        public StringBuilder Append(char[] value, int index, int valueCount)
        {
            if (valueCount < 0)
            {
                throw new ArgumentOutOfRangeException("ArgumentOutOfRange_NegativeCount");
            }
            int num = valueCount + this.m_ChunkLength;
            if (num <= this.m_ChunkChars.Length)
            {
                Array.Copy(value, index, this.m_ChunkChars, this.m_ChunkLength, valueCount);
                this.m_ChunkLength = num;
            }
            else
            {
                int num2 = this.m_ChunkChars.Length - this.m_ChunkLength;
                if (num2 > 0)
                {
                    Array.Copy(value, index, this.m_ChunkChars, this.m_ChunkLength, num2);
                    this.m_ChunkLength = this.m_ChunkChars.Length;
                }
                int num3 = valueCount - num2;
                this.ExpandByABlock(num3);
                Array.Copy(value, index + num2, this.m_ChunkChars, 0, num3);
                this.m_ChunkLength = num3;
            }
            return this;
        }
        public StringBuilder AppendLine()
        {
            return this.Append(Environment.NewLine);
        }
        public StringBuilder AppendLine(string value)
        {
            this.Append(value);
            return this.Append(Environment.NewLine);
        }
        public StringBuilder Insert(int index, string value, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("ArgumentOutOfRange_NeedNonNegNum");
            }
            int length = this.Length;
            if (index > length)
            {
                throw new ArgumentOutOfRangeException("ArgumentOutOfRange_Index");
            }
            if (value == null || value.Length == 0 || count == 0)
            {
                return this;
            }
            long num = (long)value.Length * (long)count;
            if (num > (long)(this.MaxCapacity - this.Length))
            {
                throw new OutOfMemoryException();
            }
            StringBuilder stringBuilder;
            int num2;
            this.MakeRoom(index, (int)num, out stringBuilder, out num2, false);
            while (count > 0)
            {
                this.ReplaceInPlaceAtChunk(ref stringBuilder, ref num2, value.ToCharArray(), 0, value.Length);
                count--;
            }
            return this;
        }
        public StringBuilder Remove(int startIndex, int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("ArgumentOutOfRange_NegativeLength");
            }
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException("ArgumentOutOfRange_StartIndex");
            }
            if (length > this.Length - startIndex)
            {
                throw new ArgumentOutOfRangeException("ArgumentOutOfRange_Index");
            }
            if (this.Length == length && startIndex == 0)
            {
                this.Length = 0;
                return this;
            }
            if (length > 0)
            {
                StringBuilder stringBuilder;
                int num;
                this.Remove(startIndex, length, out stringBuilder, out num);
            }
            return this;
        }
        public StringBuilder Append(char value)
        {
            if (this.m_ChunkLength < this.m_ChunkChars.Length)
            {
                char[] arg_28_0 = this.m_ChunkChars;
                int chunkLength = this.m_ChunkLength;
                this.m_ChunkLength = chunkLength + 1;
                arg_28_0[chunkLength] = value;
            }
            else
            {
                this.Append(value, 1);
            }
            return this;
        }
        public StringBuilder Append(object value)
        {
            if (value == null)
            {
                return this;
            }
            return this.Append(value.ToString());
        }
        public StringBuilder Append(char[] value)
        {
            if (value != null && value.Length != 0)
            {
                char[] chunkChars = this.m_ChunkChars;
                int chunkLength = this.m_ChunkLength;
                int length = value.Length;
                int num = chunkLength + length;
                if (num < chunkChars.Length)
                {
                    if (length <= 2)
                    {
                        if (length > 0)
                        {
                            chunkChars[chunkLength] = value[0];
                        }
                        if (length > 1)
                        {
                            chunkChars[chunkLength + 1] = value[1];
                        }
                    }
                    else
                    {
                        Array.Copy(value, 0, chunkChars, chunkLength, value.Length);
                    }
                    this.m_ChunkLength = num;
                }
                else
                {
                    Append(value, 0, value.Length);
                }
            }
            return this;
        }
        public StringBuilder Insert(int index, string value)
        {
            if (index > this.Length)
            {
                throw new ArgumentOutOfRangeException("ArgumentOutOfRange_Index");
            }
            if (value != null)
            {
                this.Insert(index, value.ToCharArray(), 0, value.Length);
            }
            return this;
        }
        private void Insert(int index, char[] value, int destIndex, int valueCount)
        {
            if (index > this.Length)
            {
                throw new ArgumentOutOfRangeException("ArgumentOutOfRange_Index");
            }
            if (valueCount > 0)
            {
                StringBuilder stringBuilder;
                int num;
                this.MakeRoom(index, valueCount, out stringBuilder, out num, false);
                this.ReplaceInPlaceAtChunk(ref stringBuilder, ref num, value, destIndex, valueCount);
            }
        }
        public StringBuilder AppendFormat(string format, object arg0)
        {
            return this.AppendFormatHelper(format, new ParamsArray(arg0));
        }
        public StringBuilder AppendFormat(string format, object arg0, object arg1)
        {
            return this.AppendFormatHelper(format, new ParamsArray(arg0, arg1));
        }
        public StringBuilder AppendFormat(string format, object arg0, object arg1, object arg2)
        {
            return this.AppendFormatHelper(format, new ParamsArray(arg0, arg1, arg2));
        }
        public StringBuilder AppendFormat(string format, params object[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException((format == null) ? "format" : "args");
            }
            return this.AppendFormatHelper(format, new ParamsArray(args));
        }
        private static void FormatError()
        {
            throw new FormatException("Format_InvalidString");
        }
        internal StringBuilder AppendFormatHelper(string format, ParamsArray args)
        {
            if (format == null)
            {
                throw new ArgumentNullException("format");
            }
            int num = 0;
            int length = format.Length;
            char c = '\0';
            while (true)
            {
                if (num < length)
                {
                    c = format[num];
                    num++;
                    bool IL_8D_Flag = false;
                    if (c == '}')
                    {
                        if (num < length && format[num] == '}')
                        {
                            num++;
                        }
                        else
                        {
                            StringBuilder.FormatError();
                        }
                    }
                    if (c == '{')
                    {
                        if (num >= length || format[num] != '{')
                        {
                            num--;
                            IL_8D_Flag = true;
                        }
                        else
                        {
                            num++;
                        }
                    }
                    if (!IL_8D_Flag)
                    {
                        this.Append(c);
                        continue;
                    }
                }
                if (num == length)
                {
                    return this;
                }
                num++;
                if (num == length || (c = format[num]) < '0' || c > '9')
                {
                    StringBuilder.FormatError();
                }
                int num2 = 0;
                do
                {
                    num2 = num2 * 10 + (int)c - 48;
                    num++;
                    if (num == length)
                    {
                        StringBuilder.FormatError();
                    }
                    c = format[num];
                }
                while (c >= '0' && c <= '9' && num2 < 1000000);
                if (num2 >= args.Length)
                {
                    break;
                }
                while (num < length && (c = format[num]) == ' ')
                {
                    num++;
                }
                bool flag = false;
                int num3 = 0;
                if (c == ',')
                {
                    num++;
                    while (num < length && format[num] == ' ')
                    {
                        num++;
                    }
                    if (num == length)
                    {
                        StringBuilder.FormatError();
                    }
                    c = format[num];
                    if (c == '-')
                    {
                        flag = true;
                        num++;
                        if (num == length)
                        {
                            StringBuilder.FormatError();
                        }
                        c = format[num];
                    }
                    if (c < '0' || c > '9')
                    {
                        StringBuilder.FormatError();
                    }
                    do
                    {
                        num3 = num3 * 10 + (int)c - 48;
                        num++;
                        if (num == length)
                        {
                            StringBuilder.FormatError();
                        }
                        c = format[num];
                        if (c < '0' || c > '9')
                        {
                            break;
                        }
                    }
                    while (num3 < 1000000);
                }
                while (num < length && (c = format[num]) == ' ')
                {
                    num++;
                }
                object obj = args[num2];
                StringBuilder stringBuilder = null;
                if (c == ':')
                {
                    num++;
                    while (true)
                    {
                        if (num == length)
                        {
                            StringBuilder.FormatError();
                        }
                        c = format[num];
                        num++;
                        if (c == '{')
                        {
                            if (num < length && format[num] == '{')
                            {
                                num++;
                            }
                            else
                            {
                                StringBuilder.FormatError();
                            }
                        }
                        else
                        {
                            if (c == '}')
                            {
                                if (num >= length || format[num] != '}')
                                {
                                    break;
                                }
                                num++;
                            }
                        }
                        if (stringBuilder == null)
                        {
                            stringBuilder = new StringBuilder();
                        }
                        stringBuilder.Append(c);
                    }
                    num--;
                }
                if (c != '}')
                {
                    StringBuilder.FormatError();
                }
                num++;
                string text = null;
                string text2 = null;
                if (obj != null)
                {
                    text2 = obj.ToString();
                }
                if (text2 == null)
                {
                    text2 = string.Empty;
                }
                int num4 = num3 - text2.Length;
                if (!flag && num4 > 0)
                {
                    this.Append(' ', num4);
                }
                this.Append(text2);
                if (flag && num4 > 0)
                {
                    this.Append(' ', num4);
                }
            }
            throw new FormatException("Format_IndexOutOfRange");
        }
        public StringBuilder Replace(string oldValue, string newValue)
        {
            return this.Replace(oldValue, newValue, 0, this.Length);
        }
        public StringBuilder Replace(string oldValue, string newValue, int startIndex, int count)
        {
            int length = this.Length;
            if (startIndex > length)
            {
                throw new ArgumentOutOfRangeException("ArgumentOutOfRange_Index");
            }
            if (count < 0 || startIndex > length - count)
            {
                throw new ArgumentOutOfRangeException("ArgumentOutOfRange_Index");
            }
            if (oldValue == null)
            {
                throw new ArgumentNullException("oldValue");
            }
            if (oldValue.Length == 0)
            {
                throw new ArgumentException("Argument_EmptyName");
            }
            if (newValue == null)
            {
                newValue = "";
            }
            int arg_7C_0 = newValue.Length;
            int arg_83_0 = oldValue.Length;
            int[] array = null;
            int num = 0;
            StringBuilder stringBuilder = this.FindChunkForIndex(startIndex);
            int num2 = startIndex - stringBuilder.m_ChunkOffset;
            while (count > 0)
            {
                if (this.StartsWith(stringBuilder, num2, count, oldValue))
                {
                    if (array == null)
                    {
                        array = new int[5];
                    }
                    else
                    {
                        if (num >= array.Length)
                        {
                            int[] array2 = new int[array.Length * 3 / 2 + 4];
                            Array.Copy(array, array2, array.Length);
                            array = array2;
                        }
                    }
                    array[num++] = num2;
                    num2 += oldValue.Length;
                    count -= oldValue.Length;
                }
                else
                {
                    num2++;
                    count--;
                }
                if (num2 >= stringBuilder.m_ChunkLength || count == 0)
                {
                    int num3 = num2 + stringBuilder.m_ChunkOffset;
                    this.ReplaceAllInChunk(array, num, stringBuilder, oldValue.Length, newValue);
                    num3 += (newValue.Length - oldValue.Length) * num;
                    num = 0;
                    stringBuilder = this.FindChunkForIndex(num3);
                    num2 = num3 - stringBuilder.m_ChunkOffset;
                }
            }
            return this;
        }
        private void ReplaceAllInChunk(int[] replacements, int replacementsCount, StringBuilder sourceChunk, int removeCount, string value)
        {
            if (replacementsCount <= 0)
            {
                return;
            }
            char[] array = value.ToCharArray();
            int num = (value.Length - removeCount) * replacementsCount;
            StringBuilder stringBuilder = sourceChunk;
            int num2 = replacements[0];
            if (num > 0)
            {
                this.MakeRoom(stringBuilder.m_ChunkOffset + num2, num, out stringBuilder, out num2, true);
            }
            int num3 = 0;
            while (true)
            {
                this.ReplaceInPlaceAtChunk(ref stringBuilder, ref num2, array, 0, value.Length);
                int num4 = replacements[num3] + removeCount;
                num3++;
                if (num3 >= replacementsCount)
                {
                    break;
                }
                int num5 = replacements[num3];
                if (num != 0)
                {
                    this.ReplaceInPlaceAtChunk(ref stringBuilder, ref num2, sourceChunk.m_ChunkChars, num4, num5 - num4);
                }
                else
                {
                    num2 += num5 - num4;
                }
            }
            if (num < 0)
            {
                this.Remove(stringBuilder.m_ChunkOffset + num2, -num, out stringBuilder, out num2);
            }
        }
        private void ReplaceInPlaceAtChunk(ref StringBuilder chunk, ref int indexInChunk, char[] value, int index, int count)
        {
            if (count != 0)
            {
                while (true)
                {
                    int temp = chunk.m_ChunkLength - indexInChunk;
                    int num = temp < count ? temp : count;
                    Array.Copy(value, index, chunk.m_ChunkChars, indexInChunk, num);
                    indexInChunk += num;
                    if (indexInChunk >= chunk.m_ChunkLength)
                    {
                        chunk = this.Next(chunk);
                        indexInChunk = 0;
                    }
                    count -= num;
                    if (count == 0)
                    {
                        break;
                    }
                    index++;
                }
            }
        }
        private bool StartsWith(StringBuilder chunk, int indexInChunk, int count, string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (count == 0)
                {
                    return false;
                }
                if (indexInChunk >= chunk.m_ChunkLength)
                {
                    chunk = this.Next(chunk);
                    if (chunk == null)
                    {
                        return false;
                    }
                    indexInChunk = 0;
                }
                if (value[i] != chunk.m_ChunkChars[indexInChunk])
                {
                    return false;
                }
                indexInChunk++;
                count--;
            }
            return true;
        }
        private StringBuilder Next(StringBuilder chunk)
        {
            if (chunk == this)
            {
                return null;
            }
            return this.FindChunkForIndex(chunk.m_ChunkOffset + chunk.m_ChunkLength);
        }
        public bool Equals(StringBuilder sb)
        {
            if (sb == null)
            {
                return false;
            }
            if (this.Capacity != sb.Capacity || this.MaxCapacity != sb.MaxCapacity || this.Length != sb.Length)
            {
                return false;
            }
            if (sb == this)
            {
                return true;
            }
            StringBuilder stringBuilder = this;
            int i = stringBuilder.m_ChunkLength;
            StringBuilder stringBuilder2 = sb;
            int j = stringBuilder2.m_ChunkLength;
            while (true)
            {
                i--;
                j--;

                while (i < 0)
                {
                    stringBuilder = stringBuilder.m_ChunkPrevious;
                    if (stringBuilder != null)
                    {
                        i = stringBuilder.m_ChunkLength + i;
                    }
                }

                while (j < 0)
                {
                    stringBuilder2 = stringBuilder2.m_ChunkPrevious;
                    if (stringBuilder2 == null)
                    {
                        break;
                    }
                    j = stringBuilder2.m_ChunkLength + j;
                }
                if (i < 0)
                {
                    return j < 0;
                }
                if (j < 0)
                {
                    return false;
                }
                if (stringBuilder.m_ChunkChars[i] != stringBuilder2.m_ChunkChars[j])
                {
                    return false;
                }
            }
        }
        public StringBuilder Replace(char oldChar, char newChar)
        {
            return this.Replace(oldChar, newChar, 0, this.Length);
        }
        public StringBuilder Replace(char oldChar, char newChar, int startIndex, int count)
        {
            int length = this.Length;
            if (startIndex > length)
            {
                throw new ArgumentOutOfRangeException("ArgumentOutOfRange_Index");
            }
            if (count < 0 || startIndex > length - count)
            {
                throw new ArgumentOutOfRangeException("ArgumentOutOfRange_Index");
            }
            int num = startIndex + count;
            StringBuilder stringBuilder = this;
            while (true)
            {
                int num2 = num - stringBuilder.m_ChunkOffset;
                int num3 = startIndex - stringBuilder.m_ChunkOffset;
                if (num2 >= 0)
                {
                    int i = num3 < 0 ? 0 : num3;
                    int num4 = stringBuilder.m_ChunkLength < num2 ? num2 : stringBuilder.m_ChunkLength;
                    while (i < num4)
                    {
                        if (stringBuilder.m_ChunkChars[i] == oldChar)
                        {
                            stringBuilder.m_ChunkChars[i] = newChar;
                        }
                        i++;
                    }
                }
                if (num3 >= 0)
                {
                    break;
                }
                stringBuilder = stringBuilder.m_ChunkPrevious;
            }
            return this;
        }
        private StringBuilder FindChunkForIndex(int index)
        {
            StringBuilder stringBuilder = this;
            while (stringBuilder.m_ChunkOffset > index)
            {
                stringBuilder = stringBuilder.m_ChunkPrevious;
            }
            return stringBuilder;
        }
        private void ExpandByABlock(int minBlockCharCount)
        {
            if (minBlockCharCount + this.Length > this.m_MaxCapacity)
            {
                throw new ArgumentOutOfRangeException("ArgumentOutOfRange_SmallCapacity");
            }
            int num = (this.Length < 8000 ? this.Length : 8000);
            num = minBlockCharCount > num ? minBlockCharCount : num;
            this.m_ChunkPrevious = new StringBuilder(this);
            this.m_ChunkOffset += this.m_ChunkLength;
            this.m_ChunkLength = 0;
            if (this.m_ChunkOffset + num < num)
            {
                this.m_ChunkChars = null;
                throw new OutOfMemoryException();
            }
            this.m_ChunkChars = new char[num];
        }
        private StringBuilder(StringBuilder from)
        {
            this.m_ChunkLength = from.m_ChunkLength;
            this.m_ChunkOffset = from.m_ChunkOffset;
            this.m_ChunkChars = from.m_ChunkChars;
            this.m_ChunkPrevious = from.m_ChunkPrevious;
            this.m_MaxCapacity = from.m_MaxCapacity;
        }
        private void MakeRoom(int index, int count, out StringBuilder chunk, out int indexInChunk, bool doneMoveFollowingChars)
        {
            if (count + this.Length > this.m_MaxCapacity)
            {
                throw new ArgumentOutOfRangeException("ArgumentOutOfRange_SmallCapacity");
            }
            chunk = this;
            while (chunk.m_ChunkOffset > index)
            {
                chunk.m_ChunkOffset += count;
                chunk = chunk.m_ChunkPrevious;
            }
            indexInChunk = index - chunk.m_ChunkOffset;
            if (!doneMoveFollowingChars && chunk.m_ChunkLength <= 32 && chunk.m_ChunkChars.Length - chunk.m_ChunkLength >= count)
            {
                int i = chunk.m_ChunkLength;
                while (i > indexInChunk)
                {
                    i--;
                    chunk.m_ChunkChars[i + count] = chunk.m_ChunkChars[i];
                }
                chunk.m_ChunkLength += count;
                return;
            }
            StringBuilder stringBuilder = new StringBuilder(count > 16 ? count : 16, chunk.m_MaxCapacity, chunk.m_ChunkPrevious);
            stringBuilder.m_ChunkLength = count;
            int num = count < indexInChunk ? count : indexInChunk;
            if (num > 0)
            {
                Array.Copy(chunk.m_ChunkChars, 0, stringBuilder.m_ChunkChars, 0, num);
                int num2 = indexInChunk - num;
                if (num2 >= 0)
                {
                    Array.Copy(chunk.m_ChunkChars, num, chunk.m_ChunkChars, 0, num2);
                    indexInChunk = num2;
                }
            }
            chunk.m_ChunkPrevious = stringBuilder;
            chunk.m_ChunkOffset += count;
            if (num < count)
            {
                chunk = stringBuilder;
                indexInChunk = num;
            }
        }
        private StringBuilder(int size, int maxCapacity, StringBuilder previousBlock)
        {
            this.m_ChunkChars = new char[size];
            this.m_MaxCapacity = maxCapacity;
            this.m_ChunkPrevious = previousBlock;
            if (previousBlock != null)
            {
                this.m_ChunkOffset = previousBlock.m_ChunkOffset + previousBlock.m_ChunkLength;
            }
        }
        private void Remove(int startIndex, int count, out StringBuilder chunk, out int indexInChunk)
        {
            int num = startIndex + count;
            chunk = this;
            StringBuilder stringBuilder = null;
            int num2 = 0;
            while (true)
            {
                if (num - chunk.m_ChunkOffset >= 0)
                {
                    if (stringBuilder == null)
                    {
                        stringBuilder = chunk;
                        num2 = num - stringBuilder.m_ChunkOffset;
                    }
                    if (startIndex - chunk.m_ChunkOffset >= 0)
                    {
                        break;
                    }
                }
                else
                {
                    chunk.m_ChunkOffset -= count;
                }
                chunk = chunk.m_ChunkPrevious;
            }
            indexInChunk = startIndex - chunk.m_ChunkOffset;
            int num3 = indexInChunk;
            int count2 = stringBuilder.m_ChunkLength - num2;
            if (stringBuilder != chunk)
            {
                num3 = 0;
                chunk.m_ChunkLength = indexInChunk;
                stringBuilder.m_ChunkPrevious = chunk;
                stringBuilder.m_ChunkOffset = chunk.m_ChunkOffset + chunk.m_ChunkLength;
                if (indexInChunk == 0)
                {
                    stringBuilder.m_ChunkPrevious = chunk.m_ChunkPrevious;
                    chunk = stringBuilder;
                }
            }
            stringBuilder.m_ChunkLength -= num2 - num3;
            if (num3 != num2)
            {
                Array.Copy(stringBuilder.m_ChunkChars, num2, stringBuilder.m_ChunkChars, num3, count2);
            }
        }
    }
    internal static class StringBuilderCache
    {
        private const int MAX_BUILDER_SIZE = 360;
        private static StringBuilder CachedInstance;
        public static StringBuilder Acquire(int capacity)
        {
            if (capacity <= 360)
            {
                StringBuilder cachedInstance = StringBuilderCache.CachedInstance;
                if (cachedInstance != null && capacity <= cachedInstance.Capacity)
                {
                    StringBuilderCache.CachedInstance = null;
                    cachedInstance.Clear();
                    return cachedInstance;
                }
            }
            return new StringBuilder(capacity);
        }
        public static void Release(StringBuilder sb)
        {
            if (sb.Capacity <= 360)
            {
                StringBuilderCache.CachedInstance = sb;
            }
        }
        public static string GetStringAndRelease(StringBuilder sb)
        {
            string arg_0C_0 = sb.ToString();
            StringBuilderCache.Release(sb);
            return arg_0C_0;
        }
    }
    public abstract class Encoding
    {
        protected static readonly byte[] EmptyPreamble = new byte[0];
        private static Encoding defaultEncoding;
        private static Encoding unicodeEncoding;
        private static Encoding bigEndianUnicode;
        private static Encoding utf8Encoding;
        private static Encoding asciiEncoding;
        public virtual string EncodingName
        {
            get
            {
                return string.Empty;
            }
        }
        public virtual bool IsSingleByte
        {
            get
            {
                return false;
            }
        }
        //public static Encoding ASCII
        //{
        //    get
        //    {
        //        if (Encoding.asciiEncoding == null)
        //        {
        //            Encoding.asciiEncoding = new ASCIIEncoding();
        //        }
        //        return Encoding.asciiEncoding;
        //    }
        //}
        public static Encoding Default
        {
            get
            {
                if (Encoding.defaultEncoding == null)
                {
                    Encoding.defaultEncoding = Encoding.CreateDefaultEncoding();
                }
                return Encoding.defaultEncoding;
            }
        }
        //public static Encoding Unicode
        //{
        //    get
        //    {
        //        if (Encoding.unicodeEncoding == null)
        //        {
        //            Encoding.unicodeEncoding = new UnicodeEncoding(false, true);
        //        }
        //        return Encoding.unicodeEncoding;
        //    }
        //}
        //public static Encoding BigEndianUnicode
        //{
        //    get
        //    {
        //        if (Encoding.bigEndianUnicode == null)
        //        {
        //            Encoding.bigEndianUnicode = new UnicodeEncoding(true, true);
        //        }
        //        return Encoding.bigEndianUnicode;
        //    }
        //}
        public static Encoding UTF8
        {
            get
            {
                if (Encoding.utf8Encoding == null)
                {
                    Encoding.utf8Encoding = new UTF8Encoding(true);
                }
                return Encoding.utf8Encoding;
            }
        }
        public static byte[] Convert(Encoding srcEncoding, Encoding dstEncoding, byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            return Encoding.Convert(srcEncoding, dstEncoding, bytes, 0, bytes.Length);
        }
        public static byte[] Convert(Encoding srcEncoding, Encoding dstEncoding, byte[] bytes, int index, int count)
        {
            if (srcEncoding == null || dstEncoding == null)
            {
                throw new ArgumentNullException((srcEncoding == null) ? "srcEncoding" : "dstEncoding");
            }
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            return dstEncoding.GetBytes(srcEncoding.GetChars(bytes, index, count));
        }
        public virtual byte[] GetPreamble()
        {
            return EmptyPreamble;
        }
        public virtual int GetByteCount(char[] chars)
        {
            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }
            return this.GetByteCount(chars, 0, chars.Length);
        }
        public virtual int GetByteCount(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            char[] array = s.ToCharArray();
            return this.GetByteCount(array, 0, array.Length);
        }
        public abstract int GetByteCount(char[] chars, int index, int count);
        public virtual byte[] GetBytes(char[] chars)
        {
            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }
            return this.GetBytes(chars, 0, chars.Length);
        }
        public virtual byte[] GetBytes(char[] chars, int index, int count)
        {
            byte[] array = new byte[this.GetByteCount(chars, index, count)];
            this.GetBytes(chars, index, count, array, 0);
            return array;
        }
        public abstract int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex);
        public virtual byte[] GetBytes(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            byte[] array = new byte[this.GetByteCount(s)];
            this.GetBytes(s, 0, s.Length, array, 0);
            return array;
        }
        public virtual int GetBytes(string s, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            return this.GetBytes(s.ToCharArray(), charIndex, charCount, bytes, byteIndex);
        }
        public virtual int GetCharCount(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            return this.GetCharCount(bytes, 0, bytes.Length);
        }
        public abstract int GetCharCount(byte[] bytes, int index, int count);
        public virtual char[] GetChars(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            return this.GetChars(bytes, 0, bytes.Length);
        }
        public virtual char[] GetChars(byte[] bytes, int index, int count)
        {
            char[] array = new char[this.GetCharCount(bytes, index, count)];
            this.GetChars(bytes, index, count, array, 0);
            return array;
        }
        public abstract int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex);
        private static Encoding CreateDefaultEncoding()
        {
            return UTF8;
        }
        public abstract int GetMaxByteCount(int charCount);
        public abstract int GetMaxCharCount(int byteCount);
        public virtual string GetString(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            return this.GetString(bytes, 0, bytes.Length);
        }
        public virtual string GetString(byte[] bytes, int index, int count)
        {
            return new string(this.GetChars(bytes, index, count));
        }
    }
    public class UTF8Encoding : Encoding
    {
        private bool emitUTF8Identifier;
        public UTF8Encoding() : this(false) { }
        public UTF8Encoding(bool encoderShouldEmitUTF8Identifier)
        {
            this.emitUTF8Identifier = encoderShouldEmitUTF8Identifier;
        }
        public override int GetByteCount(char[] chars, int index, int count)
        {
            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }
            if (index < 0 || count < 0)
            {
                throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count");
            }
            if (chars.Length - index < count)
            {
                throw new ArgumentOutOfRangeException("chars");
            }
            if (chars.Length == 0)
            {
                return 0;
            }

            int num = 0;
            for (int i = index, n = index + count; i < n; i++)
            {
                var charCode = chars[i];
                if (charCode < 128)
                    num++;
                else if (charCode < 2048)
                    num += 2;
                else
                    num += 3;
            }
            return num;
        }
        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            if (chars == null || bytes == null)
            {
                throw new ArgumentNullException((chars == null) ? "chars" : "bytes");
            }
            if (charIndex < 0 || charCount < 0)
            {
                throw new ArgumentOutOfRangeException((charIndex < 0) ? "charIndex" : "charCount");
            }
            if (chars.Length - charIndex < charCount)
            {
                throw new ArgumentOutOfRangeException("chars");
            }
            if (byteIndex < 0 || byteIndex > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("byteIndex");
            }
            if (chars.Length == 0)
            {
                return 0;
            }

            int offset = byteIndex;
            for (int i = charIndex, n = charIndex + charCount; i < n; i++)
            {
                var charCode = chars[i];
                if (charCode < 128)
                {
                    bytes[offset++] = (byte)charCode;
                }
                else if (charCode < 2048)
                {
                    bytes[offset++] = (byte)((charCode >> 6) | 192);
                    bytes[offset++] = (byte)((charCode & 63) | 128);
                }
                else
                {
                    bytes[offset++] = (byte)((charCode >> 12) | 224);
                    bytes[offset++] = (byte)(((charCode >> 6) & 63) | 128);
                    bytes[offset++] = (byte)((charCode & 63) | 128);
                }
            }
            return offset - byteIndex;
        }
        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (index < 0 || count < 0)
            {
                throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count");
            }
            if (bytes.Length - index < count)
            {
                throw new ArgumentOutOfRangeException("bytes");
            }
            if (bytes.Length == 0)
            {
                return 0;
            }

            int result = 0;
            for (int i = index, n = index + count; i < n; i++)
            {
                int num = bytes[i];
                if (num > 127)
                {
                    if ((num & 32) == 0)
                        i++;
                    else
                        i += 2;
                }
                result++;
            }
            return result;
        }
        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            if (bytes == null || chars == null)
            {
                throw new ArgumentNullException((bytes == null) ? "bytes" : "chars");
            }
            if (byteIndex < 0 || byteCount < 0)
            {
                throw new ArgumentOutOfRangeException((byteIndex < 0) ? "byteIndex" : "byteCount");
            }
            if (bytes.Length - byteIndex < byteCount)
            {
                throw new ArgumentOutOfRangeException("bytes");
            }
            if (charIndex < 0 || charIndex > chars.Length)
            {
                throw new ArgumentOutOfRangeException("charIndex");
            }
            if (bytes.Length == 0)
            {
                return 0;
            }

            int offset = charIndex;
            int n = byteIndex + byteCount;
            int o1 = n - 2, o2 = n - 3;
            for (int i = byteIndex; i < n; i++)
            {
                int num = bytes[i];
                if (num > 127)
                {
                    // 因为流读取方式可能一次不读完，连续读取多次才将数据读取完毕，所以这里不抛出异常
                    if ((num & 32) == 0)
                    {
                        if (i > o1)
                            //throw new IndexOutOfRangeException("bytes");
                            break;
                        i++;
                        num = ((num & 31) << 6) | (bytes[i] & 63);
                    }
                    else
                    {
                        if (i > o2)
                            break;
                            //throw new IndexOutOfRangeException("bytes");
                        if ((num & 16) != 0 || (bytes[i + 1] & 64) != 0)
                            break;
                            //throw new ArgumentException("Unknow character");
                        num = ((num & 15) << 12) | ((bytes[i + 1] & 63) << 6) | (bytes[i + 2] & 63);
                        i += 2;
                    }
                }
                chars[offset++] = (char)num;
            }
            return offset - charIndex;
        }
        public override int GetMaxByteCount(int charCount)
        {
            if (charCount < 0)
            {
                throw new ArgumentOutOfRangeException("charCount");
            }
            return (charCount + 1) * 3;
        }
        public override int GetMaxCharCount(int byteCount)
        {
            if (byteCount < 0)
            {
                throw new ArgumentOutOfRangeException("byteCount");
            }
            return byteCount + 1;
        }
        public override byte[] GetPreamble()
        {
            if (this.emitUTF8Identifier)
            {
                return new byte[]
				{
					239,
					187,
					191
				};
            }
            return base.GetPreamble();
        }
        public override bool Equals(object value)
        {
            UTF8Encoding uTF8Encoding = value as UTF8Encoding;
            return uTF8Encoding != null && (this.emitUTF8Identifier == uTF8Encoding.emitUTF8Identifier);
        }
        public override int GetHashCode()
        {
            return 65001 + (this.emitUTF8Identifier ? 1 : 0);
        }
    }
}
