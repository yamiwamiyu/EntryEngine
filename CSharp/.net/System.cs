using System;
using __System.Reflection;
using __System.Text;
using __System.Collections.Generic;
using EntryBuilder.CodeAnalysis.Semantics;
using __System.Collections;

namespace __System
{
    public interface IDisposable
    {
        [AInvariant]void Dispose();
    }
    public delegate int Comparison<T>(T x, T y);
    public interface IComparable<T>
    {
        int CompareTo(T other);
    }
    public interface IEquatable<T>
    {
        bool Equals(T other);
    }

    internal struct ParamsArray
    {
        private static readonly object[] oneArgArray = new object[1];
        private static readonly object[] twoArgArray = new object[2];
        private static readonly object[] threeArgArray = new object[3];
        private readonly object arg0;
        private readonly object arg1;
        private readonly object arg2;
        private readonly object[] args;
        public int Length
        {
            get
            {
                return this.args.Length;
            }
        }
        public object this[int index]
        {
            get
            {
                if (index != 0)
                {
                    return this.GetAtSlow(index);
                }
                return this.arg0;
            }
        }
        public ParamsArray(object arg0)
        {
            this.arg0 = arg0;
            this.arg1 = null;
            this.arg2 = null;
            this.args = ParamsArray.oneArgArray;
        }
        public ParamsArray(object arg0, object arg1)
        {
            this.arg0 = arg0;
            this.arg1 = arg1;
            this.arg2 = null;
            this.args = ParamsArray.twoArgArray;
        }
        public ParamsArray(object arg0, object arg1, object arg2)
        {
            this.arg0 = arg0;
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.args = ParamsArray.threeArgArray;
        }
        public ParamsArray(object[] args)
        {
            int num = args.Length;
            this.arg0 = ((num > 0) ? args[0] : null);
            this.arg1 = ((num > 1) ? args[1] : null);
            this.arg2 = ((num > 2) ? args[2] : null);
            this.args = args;
        }
        private object GetAtSlow(int index)
        {
            if (index == 1)
            {
                return this.arg1;
            }
            if (index == 2)
            {
                return this.arg2;
            }
            return this.args[index];
        }
    }
    public class Uri
    {
        private string path;
        public string AbsolutePath { get { return path; } }
        public Uri(string uriStr)
        {
            this.path = uriStr;
        }
    }
    public interface IAsyncResult
    {
        bool IsCompleted { get; }
        object AsyncState { get; }
        bool CompletedSynchronously { get; }
    }
    public delegate void AsyncCallback(IAsyncResult ar);

    /// <summary>精确到2^52，单位ms</summary>
    public struct TimeSpan
    {
        public static readonly TimeSpan Zero = new TimeSpan(0);
        public static readonly TimeSpan MinValue = new TimeSpan(4503599627370495L);
        public static readonly TimeSpan MaxValue = new TimeSpan(-4503599627370496L);
        internal long _ticks;
        public long Ticks
        {
            get
            {
                return this._ticks;
            }
        }
        public int Days
        {
            get
            {
                return (int)(this._ticks / 86400000);
            }
        }
        public int Hours
        {
            get
            {
                return (int)(this._ticks / 3600000 % 24);
            }
        }
        public int Milliseconds
        {
            get
            {
                return (int)(this._ticks % 1000);
            }
        }
        public int Minutes
        {
            get
            {
                return (int)(this._ticks / 60000 % 60);
            }
        }
        public int Seconds
        {
            get
            {
                return (int)(this._ticks / 1000 % 60);
            }
        }
        public double TotalDays
        {
            get
            {
                return (double)this._ticks * 1.1574074074074074E-7;
            }
        }
        public double TotalHours
        {
            get
            {
                return (double)this._ticks * 2.7777777777777777E-6;
            }
        }
        public double TotalMilliseconds
        {
            get
            {
                return (double)this._ticks;
            }
        }
        public double TotalMinutes
        {
            get
            {
                return (double)this._ticks * 1.6666666666666667E-04;
            }
        }
        public double TotalSeconds
        {
            get
            {
                return (double)this._ticks * 1E-03;
            }
        }
        public TimeSpan(long ticks)
        {
            this._ticks = ticks;
        }
        public TimeSpan(int hours, int minutes, int seconds)
        {
            this._ticks = TimeSpan.TimeToTicks(hours, minutes, seconds);
        }
        public TimeSpan(int days, int hours, int minutes, int seconds) : this(days, hours, minutes, seconds, 0)
        {
        }
        public TimeSpan(int days, int hours, int minutes, int seconds, int milliseconds)
        {
            long num = ((long)days * 3600L * 24L + (long)hours * 3600L + (long)minutes * 60L + (long)seconds) * 1000L + (long)milliseconds;
            if (num > 4503599627370495L || num < -4503599627370496L)
            {
                throw new OverflowException("Overflow_TimeSpanTooLong");
            }
            this._ticks = num;
        }
        public TimeSpan Add(TimeSpan ts)
        {
            return new TimeSpan(this._ticks + ts._ticks);
        }
        public static int Compare(TimeSpan t1, TimeSpan t2)
        {
            if (t1._ticks > t2._ticks)
            {
                return 1;
            }
            if (t1._ticks < t2._ticks)
            {
                return -1;
            }
            return 0;
        }
        public static TimeSpan FromDays(double value)
        {
            return TimeSpan.Interval(value, 86400000);
        }
        public override bool Equals(object value)
        {
            return value is TimeSpan && this._ticks == ((TimeSpan)value)._ticks;
        }
        public bool Equals(TimeSpan obj)
        {
            return this._ticks == obj._ticks;
        }
        public static bool Equals(TimeSpan t1, TimeSpan t2)
        {
            return t1._ticks == t2._ticks;
        }
        public override int GetHashCode()
        {
            return (int)this._ticks ^ (int)(this._ticks >> 32);
        }
        public static TimeSpan FromHours(double value)
        {
            return TimeSpan.Interval(value, 3600000);
        }
        private static TimeSpan Interval(double value, int scale)
        {
            if (double.IsNaN(value))
            {
                throw new Exception("Arg_CannotBeNaN");
            }
            double num = value * (double)scale + ((value >= 0.0) ? 0.5 : -0.5);
            return new TimeSpan((int)num);
        }
        public static TimeSpan FromMilliseconds(double value)
        {
            return new TimeSpan((long)value);
        }
        public static TimeSpan FromMinutes(double value)
        {
            return TimeSpan.Interval(value, 60000);
        }
        public static TimeSpan FromSeconds(double value)
        {
            return TimeSpan.Interval(value, 1000);
        }
        public TimeSpan Subtract(TimeSpan ts)
        {
            return new TimeSpan(this._ticks - ts._ticks);
        }
        public static TimeSpan FromTicks(long value)
        {
            return new TimeSpan(value);
        }
        internal static int TimeToTicks(int hour, int minute, int second)
        {
            return (hour * 3600 + minute * 60 + second) * 1000;
        }
        public static TimeSpan Parse(string s)
        {
            TimeSpan result;
            if (!TryParse(s, out result))
                throw new FormatException("Format_BadTimeSpan");
            return result;
        }
        public static bool TryParse(string s, out TimeSpan result)
        {
            result._ticks = 0;
            int days = 0, hours = 0, minutes = 0, seconds = 0;
            int digit = 0;
            int index = 0;
            bool minusFlag = false;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '.')
                {
                    if (index == 0)
                        days = digit;
                    else if (index == 3)
                        seconds = digit;
                    else
                        return false;
                    digit = 0;
                    index++;
                }
                else if (c == '-')
                {
                    if (digit != 0 || minusFlag)
                        return false;
                    minusFlag = true;
                }
                else if (c == ':')
                {
                    if (index == 0)
                        index++;
                    if (index == 1)
                        hours = digit;
                    else if (index == 2)
                        minutes = digit;
                    else
                        return false;
                    digit = 0;
                    index++;
                }
                else if (c >= '0' && c <= '9')
                {
                    digit = digit * 10 + (c - '0');
                }
                else
                    return false;
            }
            int milliseconds = digit;
            if (minusFlag)
            {
                days = -days;
                hours = -hours;
                minutes = -minutes;
                seconds = -seconds;
                milliseconds = -milliseconds;
            }
            result = new TimeSpan(days, hours, minutes, seconds, milliseconds);
            return true;
        }
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            int days = this.Days;
            int hours = this.Hours;
            int minutes = this.Minutes;
            int seconds = this.Seconds;
            int milliseconds = this.Milliseconds;
            if (this._ticks < 0)
            {
                builder.Append('-');
                if (days < 0) days = -days;
                if (hours < 0) hours = -hours;
                if (minutes < 0) minutes = -minutes;
                if (seconds < 0) seconds = -seconds;
                if (milliseconds < 0) milliseconds = -milliseconds;
            }
            if (days != 0)
            {
                builder.Append(days);
                builder.Append('.');
            }
            if (hours < 10)
                builder.Append('0');
            builder.Append(hours);
            builder.Append(':');
            if (minutes < 10)
                builder.Append('0');
            builder.Append(minutes);
            builder.Append(':');
            if (seconds < 10)
                builder.Append('0');
            builder.Append(seconds);
            if (milliseconds > 0)
            {
                builder.Append('.');
                builder.Append(milliseconds);
            }
            return builder.ToString();
        }
        public static TimeSpan operator -(TimeSpan t)
        {
            if (t._ticks == TimeSpan.MinValue._ticks)
            {
                throw new Exception("Overflow_NegateTwosCompNum");
            }
            return new TimeSpan(-t._ticks);
        }
        public static TimeSpan operator -(TimeSpan t1, TimeSpan t2)
        {
            return t1.Subtract(t2);
        }
        public static TimeSpan operator +(TimeSpan t)
        {
            return t;
        }
        public static TimeSpan operator +(TimeSpan t1, TimeSpan t2)
        {
            return t1.Add(t2);
        }
        public static bool operator ==(TimeSpan t1, TimeSpan t2)
        {
            return t1._ticks == t2._ticks;
        }
        public static bool operator !=(TimeSpan t1, TimeSpan t2)
        {
            return t1._ticks != t2._ticks;
        }
        public static bool operator <(TimeSpan t1, TimeSpan t2)
        {
            return t1._ticks < t2._ticks;
        }
        public static bool operator <=(TimeSpan t1, TimeSpan t2)
        {
            return t1._ticks <= t2._ticks;
        }
        public static bool operator >(TimeSpan t1, TimeSpan t2)
        {
            return t1._ticks > t2._ticks;
        }
        public static bool operator >=(TimeSpan t1, TimeSpan t2)
        {
            return t1._ticks >= t2._ticks;
        }
    }
    public enum DateTimeKind
    {
        Unspecified,
        Utc,
        Local
    }
    /// <summary>2^50 - 1 = 1125899906842623，还有两位记做Kind，JS的Number精确整数到2^53</summary>
    public partial struct DateTime
    {
        // 1970年1月1日的毫秒数
        public const long TIMESPAN_OFFSET = 62135596800000L;
        public const long KIND_1 = 1125899906842624L;
        public const long KIND_2 = 2251799813685248L;
        public const long KIND_3 = 3377699720527872L;
        private static readonly long[] KINDS = new long[] { 0L, KIND_1, KIND_2, KIND_3 };
        public static readonly DateTime MinValue = new DateTime(0L, DateTimeKind.Unspecified);
        public static readonly DateTime MaxValue = new DateTime(1125899906842623L, DateTimeKind.Unspecified);
        public static readonly int[] DaysToMonth365 = new int[] { 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334, 365 };
        public static readonly int[] DaysToMonth366 = new int[] { 0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 335, 366 };
        //private DateTimeKind kind; 
        private long dateData;
        internal long InternalTicks
        {
            get
            {
                return dateData - KINDS[dateData / 1125899906842623L];
                //switch (v)
                //{
                //    case 0: return dateData;
                //    case 1: return dateData - KIND_1;
                //    case 2: return dateData - KIND_2;
                //    case 3: return dateData - KIND_3;
                //}
                //throw new NotImplementedException();
                //return this.dateData & 1125899906842623L;
            }
        }
        private ulong InternalKind
        {
            get
            {
                //return (ulong)(dateData >> 50);
                return (ulong)(dateData / 1125899906842623L);
            }
        }
        public DateTimeKind Kind
        {
            //get { return (DateTimeKind)(dateData >> 50); }
            get { return (DateTimeKind)(dateData / 1125899906842623L); }
        }
        public DateTime Date
        {
            get
            {
                long expr_06 = this.InternalTicks;
                //return new DateTime((ulong)(expr_06 - expr_06 % 86400000L | (long)this.InternalKind));
                return new DateTime((expr_06 - expr_06 % 86400000L), Kind);
            }
        }
        public int Day
        {
            get
            {
                return this.GetDatePart(3);
            }
        }
        public DayOfWeek DayOfWeek
        {
            get
            {
                return (DayOfWeek)((this.InternalTicks / 86400000L + 1L) % 7L);
            }
        }
        public int DayOfYear
        {
            get
            {
                return this.GetDatePart(1);
            }
        }
        public int Hour
        {
            get
            {
                return (int)(this.InternalTicks / 3600000L % 24L);
            }
        }
        public int Millisecond
        {
            get
            {
                return (int)(this.InternalTicks % 1000L);
            }
        }
        public int Minute
        {
            get
            {
                return (int)(this.InternalTicks / 60000L % 60L);
            }
        }
        public int Month
        {
            get
            {
                return this.GetDatePart(2);
            }
        }
        [ASystemAPI]private static long DateTimeNow { get { throw new NotImplementedException(); } }
        [ASystemAPI]private static long DateTimeUtcNow { get { throw new NotImplementedException(); } }
        public static DateTime Now
        {
            get
            {
                return new DateTime(DateTimeNow, DateTimeKind.Local);
            }
        }
        public static DateTime UtcNow
        {
            get
            {
                return new DateTime(DateTimeUtcNow, DateTimeKind.Local);
            }
        }
        public int Second
        {
            get
            {
                // 除以1000任然超过32位的最大值，所以先%60000保证数字在32位以内
                //return (int)(this.InternalTicks / 1000L % 60L);
                return (int)((this.InternalTicks % 60000) / 1000L % 60L);
            }
        }
        /// <summary>精确到ms</summary>
        public long Ticks
        {
            get
            {
                return this.InternalTicks;
            }
        }
        public TimeSpan TimeOfDay
        {
            get
            {
                return new TimeSpan(this.InternalTicks % 86400000L);
            }
        }
        public static DateTime Today
        {
            get
            {
                return DateTime.Now.Date;
            }
        }
        public int Year
        {
            get
            {
                return this.GetDatePart(0);
            }
        }
        public DateTime(long ticks)
        {
            //if (ticks < 0L || ticks > 1125899906842623L)
            //{
            //    throw new Exception("ArgumentOutOfRange_DateTimeBadTicks");
            //}
            //this.kind = DateTimeKind.Unspecified;
            this.dateData = ticks;
        }
        public DateTime(long ticks, DateTimeKind kind)
        {
            if (ticks < 0L || ticks > 1125899906842623L)
            {
                throw new ArgumentOutOfRangeException("ticks");
            }
            if (kind < DateTimeKind.Unspecified || kind > DateTimeKind.Local)
            {
                throw new ArgumentException("Argument_InvalidDateTimeKind");
            }
            //this.kind = kind;
            this.dateData = ticks + KINDS[(int)kind];
        }
        private DateTime(ulong dateData)
        {
            //this.kind = DateTimeKind.Unspecified;
            this.dateData = (long)dateData;
        }
        public DateTime(int year, int month, int day)
        {
            //this.kind = DateTimeKind.Unspecified;
            this.dateData = DateTime.DateToTicks(year, month, day);
        }
        public DateTime(int year, int month, int day, int hour, int minute, int second)
        {
            //this.kind = DateTimeKind.Unspecified;
            this.dateData = (DateTime.DateToTicks(year, month, day) + DateTime.TimeToTicks(hour, minute, second));
        }
        public DateTime(int year, int month, int day, int hour, int minute, int second, int millisecond)
        {
            if (millisecond < 0 || millisecond >= 1000)
            {
                throw new Exception("ArgumentOutOfRange_Range");
            }
            long num = DateTime.DateToTicks(year, month, day) + DateTime.TimeToTicks(hour, minute, second);
            //num += (long)millisecond * 10000L;
            num += (long)millisecond;
            if (num < 0L || num > 1125899906842623L)
            {
                throw new Exception("Arg_DateTimeRange");
            }
            //this.kind = DateTimeKind.Unspecified;
            this.dateData = num;
        }
        public DateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, DateTimeKind kind)
        {
            if (millisecond < 0 || millisecond >= 1000)
            {
                throw new Exception("ArgumentOutOfRange_Range");
            }
            long num = DateTime.DateToTicks(year, month, day) + DateTime.TimeToTicks(hour, minute, second);
            //num += (long)millisecond * 10000L;
            num += (long)millisecond;
            if (num < 0L || num > 1125899906842623L)
            {
                throw new Exception("Arg_DateTimeRange");
            }
            //this.kind = kind;
            //this.dateData = num;
            this.dateData = (num | (long)kind << 50);
        }
        public DateTime Add(TimeSpan value)
        {
            return this.AddTicks(value._ticks);
        }
        private DateTime Add(double value, int scale)
        {
            long num = (long)(value * (double)scale + ((value >= 0.0) ? 0.5 : -0.5));
            return this.AddTicks(num);
        }
        public DateTime AddDays(double value)
        {
            return this.Add(value, 86400000);
        }
        public DateTime AddHours(double value)
        {
            return this.Add(value, 3600000);
        }
        public DateTime AddMilliseconds(double value)
        {
            return this.Add(value, 1);
        }
        public DateTime AddMinutes(double value)
        {
            return this.Add(value, 60000);
        }
        public DateTime AddMonths(int months)
        {
            if (months < -120000 || months > 120000)
            {
                throw new Exception("ArgumentOutOfRange_DateTimeBadMonths");
            }
            int num = this.GetDatePart(0);
            int num2 = this.GetDatePart(2);
            int num3 = this.GetDatePart(3);
            int num4 = num2 - 1 + months;
            if (num4 >= 0)
            {
                num2 = num4 % 12 + 1;
                num += num4 / 12;
            }
            else
            {
                num2 = 12 + (num4 + 1) % 12;
                num += (num4 - 11) / 12;
            }
            if (num < 1 || num > 9999)
            {
                throw new Exception("ArgumentOutOfRange_DateArithmetic");
            }
            int num5 = DateTime.DaysInMonth(num, num2);
            if (num3 > num5)
            {
                num3 = num5;
            }
            return new DateTime((ulong)(DateTime.DateToTicks(num, num2, num3) + this.InternalTicks % 86400000L | (long)this.InternalKind));
        }
        public DateTime AddSeconds(double value)
        {
            return this.Add(value, 1000);
        }
        public DateTime AddTicks(long value)
        {
            long internalTicks = this.InternalTicks;
            if (value > 1125899906842623L - internalTicks || value < 0L - internalTicks)
            {
                throw new Exception("ArgumentOutOfRange_DateArithmetic");
            }
            return new DateTime((ulong)(internalTicks + value | (long)this.InternalKind));
        }
        public DateTime AddYears(int value)
        {
            return this.AddMonths(value * 12);
        }
        public static int Compare(DateTime t1, DateTime t2)
        {
            long internalTicks = t1.InternalTicks;
            long internalTicks2 = t2.InternalTicks;
            if (internalTicks > internalTicks2)
            {
                return 1;
            }
            if (internalTicks < internalTicks2)
            {
                return -1;
            }
            return 0;
        }
        public int CompareTo(object value)
        {
            if (value == null)
            {
                return 1;
            }
            if (!(value is DateTime))
            {
                throw new Exception("Arg_MustBeDateTime");
            }
            long internalTicks = ((DateTime)value).InternalTicks;
            long internalTicks2 = this.InternalTicks;
            if (internalTicks2 > internalTicks)
            {
                return 1;
            }
            if (internalTicks2 < internalTicks)
            {
                return -1;
            }
            return 0;
        }
        public int CompareTo(DateTime value)
        {
            long internalTicks = value.InternalTicks;
            long internalTicks2 = this.InternalTicks;
            if (internalTicks2 > internalTicks)
            {
                return 1;
            }
            if (internalTicks2 < internalTicks)
            {
                return -1;
            }
            return 0;
        }
        private static long DateToTicks(int year, int month, int day)
        {
            if (year >= 1 && year <= 9999 && month >= 1 && month <= 12)
            {
                int[] array = DateTime.IsLeapYear(year) ? DateTime.DaysToMonth366 : DateTime.DaysToMonth365;
                if (day >= 1 && day <= array[month] - array[month - 1])
                {
                    int num = year - 1;
                    return (long)(num * 365 + num / 4 - num / 100 + num / 400 + array[month - 1] + day - 1) * 86400000L;
                }
            }
            throw new Exception("ArgumentOutOfRange_BadYearMonthDay");
        }
        private static long TimeToTicks(int hour, int minute, int second)
        {
            if (hour >= 0 && hour < 24 && minute >= 0 && minute < 60 && second >= 0 && second < 60)
            {
                return TimeSpan.TimeToTicks(hour, minute, second);
            }
            throw new Exception("ArgumentOutOfRange_BadHourMinuteSecond");
        }
        public static int DaysInMonth(int year, int month)
        {
            int[] array = DateTime.IsLeapYear(year) ? DateTime.DaysToMonth366 : DateTime.DaysToMonth365;
            return array[month] - array[month - 1];
        }
        public override bool Equals(object value)
        {
            return value is DateTime && this.InternalTicks == ((DateTime)value).InternalTicks;
        }
        public bool Equals(DateTime value)
        {
            return this.InternalTicks == value.InternalTicks;
        }
        public static bool Equals(DateTime t1, DateTime t2)
        {
            return t1.InternalTicks == t2.InternalTicks;
        }
        private int GetDatePart(int part)
        {
            int i = (int)(this.InternalTicks / 86400000L);
            int num = i / 146097;
            i -= num * 146097;
            int num2 = i / 36524;
            if (num2 == 4)
            {
                num2 = 3;
            }
            i -= num2 * 36524;
            int num3 = i / 1461;
            i -= num3 * 1461;
            int num4 = i / 365;
            if (num4 == 4)
            {
                num4 = 3;
            }
            if (part == 0)
            {
                return num * 400 + num2 * 100 + num3 * 4 + num4 + 1;
            }
            i -= num4 * 365;
            if (part == 1)
            {
                return i + 1;
            }
            int[] array = (num4 == 3 && (num3 != 24 || num2 == 3)) ? DateTime.DaysToMonth366 : DateTime.DaysToMonth365;
            int num5 = i >> 6;
            while (i >= array[num5])
            {
                num5++;
            }
            if (part == 2)
            {
                return num5;
            }
            return i - array[num5 - 1] + 1;
        }
        public override int GetHashCode()
        {
            long internalTicks = this.InternalTicks;
            return (int)internalTicks ^ (int)(internalTicks >> 32);
        }
        public static bool IsLeapYear(int year)
        {
            return year % 4 == 0 && (year % 100 != 0 || year % 400 == 0);
        }
        public static DateTime Parse(string s)
        {
            DateTime time;
            if (!TryParse(s, out time))
                throw new FormatException("Format_BadDateTime");
            return time;
        }
        public TimeSpan Subtract(DateTime value)
        {
            return new TimeSpan(this.InternalTicks - value.InternalTicks);
        }
        public DateTime Subtract(TimeSpan value)
        {
            long internalTicks = this.InternalTicks;
            long ticks = value._ticks;
            return new DateTime((ulong)(internalTicks - ticks | (long)this.InternalKind));
        }
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            int year = this.Year;
            int month = this.Month;
            int day = this.Day;
            int hour = this.Hour;
            int minute = this.Minute;
            int second = this.Second;
            builder.Append(year);
            builder.Append('/');
            builder.Append(month);
            builder.Append('/');
            builder.Append(day);
            builder.Append(' ');
            builder.Append(hour);
            builder.Append(':');
            builder.Append(minute);
            builder.Append(':');
            builder.Append(second);
            return builder.ToString();
        }
        public string ToString(string format)
        {
            int year = this.Year;
            int month = this.Month;
            int day = this.Day;
            int hour = this.Hour;
            int minute = this.Minute;
            int second = this.Second;
            StringBuilder builder = new StringBuilder();
            char tc = '\0';
            int count = 0;
            for (int i = 0, n = format.Length; i <= n; i++)
            {
                char c = i == n ? tc : format[i];
                if (i < n && (c == 'y' || c == 'M' || c == 'd' || c == 'h' || c == 'H' || c == 'm' || c == 's'))
                {
                    if (tc == c)
                    {
                        count++;
                        continue;
                    }
                }
                else
                {
                    if (tc == 'y')
                    {
                        string yearStr = year.ToString();
                        if (count > yearStr.Length)
                        {
                            for (int j = count - yearStr.Length; j > 0; j--)
                                builder.Append('0');
                            builder.Append(yearStr);
                        }
                        else
                            builder.Append(yearStr);
                    }
                    else if (tc == 'M')
                    {
                        if (count > 1 && month < 10)
                            builder.Append('0');
                        builder.Append(month);
                    }
                    else if (tc == 'd')
                    {
                        if (count > 1 && day < 10)
                            builder.Append('0');
                        builder.Append(day);
                    }
                    else if (tc == 'h')
                    {
                        int h2 = hour == 12 ? 12 : hour % 12;
                        if (count > 1 && h2 < 10)
                            builder.Append('0');
                        builder.Append(h2);
                    }
                    else if (tc == 'H')
                    {
                        if (count > 1 && hour < 10)
                            builder.Append('0');
                        builder.Append(hour);
                    }
                    else if (tc == 'm')
                    {
                        if (count > 1 && minute < 10)
                            builder.Append('0');
                        builder.Append(minute);
                    }
                    else if (tc == 's')
                    {
                        if (count > 1 && second < 10)
                            builder.Append('0');
                        builder.Append(second);
                    }

                    if (i < n)
                        builder.Append(c);
                }
                tc = c;
                count = 1;
            }
            return builder.ToString();
        }
        public static bool TryParse(string s, out DateTime result)
        {
            //result.kind = DateTimeKind.Unspecified;
            int year = 0;
            int month = 0;
            int day = 0;
            int hour = 0;
            int minute = 0;
            int second = 0;
            int index = 0;
            int digit = 0;
            bool sep = false;
            result.dateData = 0;
            char c = '0';
            for (int i = 0, n = s.Length; i <= n; i++)
            {
                if (i < n)
                    c = s[i];
                if (i != n && c >= '0' && c <= '9')
                {
                    digit = digit * 10 + (c - '0');
                    sep = false;
                }
                else
                {
                    if (sep) return false;
                    index++;
                    switch (index)
                    {
                        case 1: year = digit; break;
                        case 2: month = digit; break;
                        case 3: day = digit; break;
                        case 4: hour = digit; break;
                        case 5: minute = digit; break;
                        case 6: second = digit; break;
                        default: return false;
                    }
                    sep = true;
                    digit = 0;
                    if (index < 3)
                    {
                        if (c == '/' || c == '-')
                            continue;
                    }
                    else if (index == 3)
                    {
                        if (c == ' ')
                            continue;
                    }
                    else
                    {
                        if (c == ':')
                            continue;
                    }
                    if (i < n)
                        return false;
                }
            }
            result = new DateTime(year, month, day, hour, minute, second);
            return true;
        }
        public static DateTime operator +(DateTime d, TimeSpan t)
        {
            long internalTicks = d.InternalTicks;
            long ticks = t._ticks;
            return new DateTime((ulong)(internalTicks + ticks | (long)d.InternalKind));
        }
        public static DateTime operator -(DateTime d, TimeSpan t)
        {
            long internalTicks = d.InternalTicks;
            long ticks = t._ticks;
            return new DateTime((ulong)(internalTicks - ticks | (long)d.InternalKind));
        }
        public static TimeSpan operator -(DateTime d1, DateTime d2)
        {
            return new TimeSpan(d1.InternalTicks - d2.InternalTicks);
        }
        public static bool operator ==(DateTime d1, DateTime d2)
        {
            return d1.InternalTicks == d2.InternalTicks;
        }
        public static bool operator !=(DateTime d1, DateTime d2)
        {
            return d1.InternalTicks != d2.InternalTicks;
        }
        public static bool operator <(DateTime t1, DateTime t2)
        {
            return t1.InternalTicks < t2.InternalTicks;
        }
        public static bool operator <=(DateTime t1, DateTime t2)
        {
            return t1.InternalTicks <= t2.InternalTicks;
        }
        public static bool operator >(DateTime t1, DateTime t2)
        {
            return t1.InternalTicks > t2.InternalTicks;
        }
        public static bool operator >=(DateTime t1, DateTime t2)
        {
            return t1.InternalTicks >= t2.InternalTicks;
        }
    }
    public enum DayOfWeek
    {
        Sunday = 0,
        Monday = 1,
        Tuesday = 2,
        Wednesday = 3,
        Thursday = 4,
        Friday = 5,
        Saturday = 6,
    }
    //public class Random
    //{
    //    private int inext;
    //    private int inextp;
    //    private int[] SeedArray = new int[56];
    //    public Random()
    //        : this((int)DateTime.Now.Ticks)
    //    {
    //    }
    //    public Random(int Seed)
    //    {
    //        int num = (Seed == -2147483648) ? 2147483647 : (Seed < 0 ? -Seed : Seed);
    //        int num2 = 161803398 - num;
    //        this.SeedArray[55] = num2;
    //        int num3 = 1;
    //        for (int i = 1; i < 55; i++)
    //        {
    //            int num4 = 21 * i % 55;
    //            this.SeedArray[num4] = num3;
    //            num3 = num2 - num3;
    //            if (num3 < 0)
    //            {
    //                num3 += 2147483647;
    //            }
    //            num2 = this.SeedArray[num4];
    //        }
    //        for (int j = 1; j < 5; j++)
    //        {
    //            for (int k = 1; k < 56; k++)
    //            {
    //                this.SeedArray[k] -= this.SeedArray[1 + (k + 30) % 55];
    //                if (this.SeedArray[k] < 0)
    //                {
    //                    this.SeedArray[k] += 2147483647;
    //                }
    //            }
    //        }
    //        this.inext = 0;
    //        this.inextp = 21;
    //        Seed = 1;
    //    }
    //    protected virtual double Sample()
    //    {
    //        return (double)this.InternalSample() * 4.6566128752457969E-10;
    //    }
    //    private int InternalSample()
    //    {
    //        int num = this.inext;
    //        int num2 = this.inextp;
    //        if (++num >= 56)
    //        {
    //            num = 1;
    //        }
    //        if (++num2 >= 56)
    //        {
    //            num2 = 1;
    //        }
    //        int num3 = this.SeedArray[num] - this.SeedArray[num2];
    //        if (num3 == 2147483647)
    //        {
    //            num3--;
    //        }
    //        if (num3 < 0)
    //        {
    //            num3 += 2147483647;
    //        }
    //        this.SeedArray[num] = num3;
    //        this.inext = num;
    //        this.inextp = num2;
    //        return num3;
    //    }
    //    public virtual int Next()
    //    {
    //        return this.InternalSample();
    //    }
    //    private double GetSampleForLargeRange()
    //    {
    //        int num = this.InternalSample();
    //        if (this.InternalSample() % 2 == 0)
    //        {
    //            num = -num;
    //        }
    //        return ((double)num + 2147483646.0) / 4294967293.0;
    //    }
    //    public virtual int Next(int minValue, int maxValue)
    //    {
    //        long num = (long)maxValue - (long)minValue;
    //        if (num <= 2147483647L)
    //        {
    //            return (int)(this.Sample() * (double)num) + minValue;
    //        }
    //        return (int)((long)(this.GetSampleForLargeRange() * (double)num) + (long)minValue);
    //    }
    //    public virtual int Next(int maxValue)
    //    {
    //        return (int)(this.Sample() * (double)maxValue);
    //    }
    //    public virtual double NextDouble()
    //    {
    //        return this.Sample();
    //    }
    //    public virtual void NextBytes(byte[] buffer)
    //    {
    //        if (buffer == null)
    //        {
    //            throw new Exception("buffer");
    //        }
    //        for (int i = 0; i < buffer.Length; i++)
    //        {
    //            buffer[i] = (byte)(this.InternalSample() % 256);
    //        }
    //    }
    //}
    public class Random
    {
        private int seed;
        public Random() : this((int)DateTime.Now.Ticks) { }
        public Random(int Seed) { this.seed = Seed; }
        protected virtual double Sample()
        {
            return (double)this.InternalSample() * 4.6566128752457969E-10;
        }
        private int InternalSample()
        {
            seed = (seed * 1194211693 + 12345) & int.MaxValue;
            return seed;
        }
        public virtual int Next()
        {
            return this.InternalSample();
        }
        public virtual int Next(int minValue, int maxValue)
        {
            long num = (long)maxValue - (long)minValue;
            return (int)(this.Sample() * (double)num) + minValue;
        }
        public virtual int Next(int maxValue)
        {
            return (int)(this.Sample() * (double)maxValue);
        }
        public virtual double NextDouble()
        {
            return this.Sample();
        }
        public virtual void NextBytes(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new Exception("buffer");
            }
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)(this.InternalSample() % 256);
            }
        }
    }

    
    public static class Nullable
    {
        public static int Compare<T>(Nullable<T> n1, Nullable<T> n2) where T : struct
        {
            if (n1.HasValue)
            {
                if (n2.HasValue)
                {
                    return Comparer<T>.Default.Compare(n1.value, n2.value);
                }
                return 1;
            }
            else
            {
                if (n2.HasValue)
                {
                    return -1;
                }
                return 0;
            }
        }
        public static bool Equals<T>(Nullable<T> n1, Nullable<T> n2) where T : struct
        {
            if (n1.HasValue)
            {
                return n2.HasValue && EqualityComparer<T>.Default.Equals(n1.value, n2.value);
            }
            return !n2.HasValue;
        }
        public static Type GetUnderlyingType(Type nullableType)
        {
            if (nullableType == null)
            {
                throw new ArgumentNullException("nullableType");
            }
            Type result = null;
#if !DEBUG
            //if (nullableType.IsGenericType && !nullableType.IsGenericTypeDefinition && nullableType.GetGenericTypeDefinition() == typeof(Nullable<>))
            if (nullableType.IsGenericType && !nullableType.IsGenericTypeDefinition && nullableType.Name == "Nullable")
            {
                result = nullableType.GetGenericArguments()[0];
            }
#endif
            return result;
        }
    }
    [AInvariant]public struct Nullable<T> where T : struct
    {
        private bool hasValue;
        internal T value;
        public bool HasValue
        {
            get
            {
                return this.hasValue;
            }
        }
        public T Value
        {
            get
            {
                if (!this.hasValue)
                {
                    throw new Exception();
                }
                return this.value;
            }
        }
        public Nullable(T value)
        {
            this.value = value;
            this.hasValue = true;
        }
        public T GetValueOrDefault()
        {
            return this.value;
        }
        public T GetValueOrDefault(T defaultValue)
        {
            if (!this.hasValue)
            {
                return defaultValue;
            }
            return this.value;
        }
        public override bool Equals(object other)
        {
            if (!this.hasValue)
            {
                return other == null;
            }
            return other != null && this.value.Equals(other);
        }
        public override int GetHashCode()
        {
            if (!this.hasValue)
            {
                return 0;
            }
            return this.value.GetHashCode();
        }
        public override string ToString()
        {
            if (!this.hasValue)
            {
                return "";
            }
            return this.value.ToString();
        }
        public static explicit operator Nullable<T>(T value)
        {
            return new Nullable<T>(value);
        }
        public static explicit operator T(Nullable<T> value)
        {
            return value.Value;
        }
    }
#if !DEBUG
    public abstract partial class Array : IList<object>, ICollection<object>, IEnumerable<object>
    {
        struct Enumerator : IEnumerator<object>, IDisposable
        {
            Array array;
            int index;
            public Enumerator(Array array)
            {
                this.array = array;
                this.index = -1;
            }
            public object Current
            {
                get { return array.GetValue(index); }
            }
            public bool MoveNext()
            {
                index++;
                return index <= array.Length - 1;
            }
            public void Reset()
            {
                index = -1;
            }
            public void Dispose()
            {
                array = null;
                index = -1;
            }
        }

        [ASystemAPI]public extern int Length { get; }
        int ICollection<object>.Count { get { return this.Length; } }
        [ASystemAPI]public object this[int index]
        {
            get { return this[index]; }
            set { this[index] = value; }
        }

        public Array(int length) { }

        public void CopyTo(object[] array, int index)
        {
            Array.Copy(this, 0, array, index, this.Length);
        }
        public object GetValue(int index)
        {
            return this[index];
        }
        public void SetValue(object value, int index)
        {
            this[index] = value;
        }
        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            return new Enumerator(this);
        }
        //int IList<object>.Add(object value)
        //{
        //    throw new NotImplementedException();
        //}
        //bool IList<object>.Contains(object value)
        //{
        //    return IndexOf(this, value) != -1;
        //}
        //void IList<object>.Clear()
        //{
        //    //Clear(this, 0, this.Length);
        //}
        //int IList<object>.IndexOf(object value)
        //{
        //    return IndexOf(this, value);
        //}
        //void IList<object>.Insert(int index, object value)
        //{
        //    throw new NotImplementedException();
        //}
        //void IList<object>.Remove(object value)
        //{
        //    throw new NotImplementedException();
        //}
        void IList<object>.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        //[AInvariant]public static void Clear(Array array, int index, int length)
        //{
        //    object value = _R.Default(array.GetType().GetElementType());
        //    for (int i = index, n = index + length; i < n; i++)
        //        array.SetValue(value, i);
        //}
        [AInvariant]public static void Clear<T>(T[] array, int index, int length)
        {
            for (int i = index, n = index + length; i < n; i++)
                array[i] = default(T);
        }
        public static void Copy(Array sourceArray, Array destinationArray, int length)
        {
            Copy(sourceArray, 0, destinationArray, 0, length);
        }
        public static void Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length)
        {
            bool same = sourceArray == destinationArray;
            if (!same || sourceIndex >= destinationIndex || sourceIndex + length <= destinationIndex)
            {
                for (int i = 0; i < length; i++)
                {
                    destinationArray[destinationIndex + i] = sourceArray[sourceIndex + i];
                }
            }
            else
            {
                for (int i = length - 1; i >= 0; i--)
                {
                    destinationArray[destinationIndex + i] = sourceArray[sourceIndex + i];
                }
            }
        }
        [AInvariant]public static Array CreateInstance(Type elementType, int length)
        {
            // BUG: 尚未测试正确性
            return (Array)elementType.MakeArrayType().GetConstructors()[0].Invoke(new object[1] { length });
        }
        public static int IndexOf<T>(T[] array, T value)
        {
            return Array.IndexOf<T>(array, value, 0, array.Length);
        }
        public static int IndexOf<T>(T[] array, T value, int startIndex)
        {
            return Array.IndexOf<T>(array, value, startIndex, array.Length - startIndex);
        }
        public static int IndexOf<T>(T[] array, T value, int startIndex, int count)
        {
            return EqualityComparer<T>.Default.IndexOf(array, value, startIndex, count);
        }
        public static int IndexOf(Array array, object value)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            return Array.IndexOf(array, value, 0, array.Length);
        }
        public static int IndexOf(Array array, object value, int startIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            return Array.IndexOf(array, value, startIndex, array.Length - startIndex);
        }
        public static int IndexOf(Array array, object value, int startIndex, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            int num = startIndex + count;
            for (int k = startIndex; k < num; k++)
            {
                object value2 = array.GetValue(k);
                if (value2 == null)
                {
                    if (value == null)
                    {
                        return k;
                    }
                }
                else
                {
                    if (value2.Equals(value))
                    {
                        return k;
                    }
                }
            }
            return -1;
        }
        public static void Resize<T>(ref T[] array, int newSize)
        {
            T[] array2 = array;
            if (array2 == null)
            {
                array = new T[newSize];
                return;
            }
            if (array2.Length != newSize)
            {
                T[] array3 = new T[newSize];
                Array.Copy(array2, 0, array3, 0, (array2.Length > newSize) ? newSize : array2.Length);
                array = array3;
            }
        }
    }
#endif
    public abstract class Enum : ValueType
    {
        public static Type GetUnderlyingType(Type enumType)
        {
            return enumType.GetEnumUnderlyingType();
        }
        public static Array GetValues(Type enumType)
        {
            throw new Exception();
        }
        public static object Parse(Type enumType, string value)
        {
            throw new Exception();
        }
    }
    /// <summary>delegate默认先继承MulticastDelegate</summary>
    public abstract partial class Delegate
    {
        public virtual Delegate[] GetInvocationList()
        {
            return new Delegate[]
            {
                this
            };
        }
        [ASystemAPI][AInvariant]public abstract object Invoke(params object[] objs);
        public static Delegate operator +(Delegate d1, Delegate d2)
        {
            MulticastDelegate mul = d1 as MulticastDelegate;
            if (mul == null)
                mul = new MulticastDelegate(d1);
            mul._invokationList.Add(d2);
            return mul;
        }
        public static Delegate operator -(Delegate d1, Delegate d2)
        {
            MulticastDelegate mul = d1 as MulticastDelegate;
            if (mul == null)
                return d1;
            mul._invokationList.Remove(d2);
            if (mul._invokationList.Count == 0)
                return null;
            return mul;
        }
        //public static bool operator ==(Delegate d1, Delegate d2)
        //{
        //    if (d1 == null)
        //    {
        //        return d2 == null;
        //    }
        //    return d1.Equals(d2);
        //}
        //public static bool operator !=(Delegate d1, Delegate d2)
        //{
        //    if (d1 == null)
        //    {
        //        return d2 != null;
        //    }
        //    return !d1.Equals(d2);
        //}
    }
    [AInvariant][ANonOptimize]public class MulticastDelegate : Delegate
    {
        internal List<Delegate> _invokationList = new List<Delegate>();
        [AInvariant]internal MulticastDelegate(Delegate d)
        {
            if (d != null)
                _invokationList.Add(d);
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            MulticastDelegate mul = obj as MulticastDelegate;
            if (mul != null)
            {
                if (this._invokationList.Count != mul._invokationList.Count)
                    return false;
                for (int i = 0; i < _invokationList.Count; i++)
                    if (!_invokationList[i].Equals(mul._invokationList[i]))
                        return false;
                //return object.Equals(mul, this);
                return true;
            }
            Delegate del = obj as Delegate;
            if (del == null)
                return false;
            return _invokationList.Count == 1 && _invokationList[0].Equals(del);
        }
        public override int GetHashCode()
        {
            return _invokationList.GetHashCode();
        }
        public override Delegate[] GetInvocationList()
        {
            return _invokationList.ToArray();
        }
        [ANonOptimize][AInvariant]public override object Invoke(object[] objs)
        {
            object result = null;
            for (int i = 0; i < _invokationList.Count; i++)
                result = _invokationList[i].Invoke(objs);
            return result;
        }
        [ANonOptimize][AInvariant]internal object InvokeAll(Func<Delegate, object> func)
        {
            object result = null;
            for (int i = 0; i < _invokationList.Count; i++)
                result = func(_invokationList[i]);
            return result;
        }
    }

    public class Attribute
#if DEBUG
: System.Attribute
#endif
    {
    }
    public class Flags : Attribute
    {
    }
    public class Serializable : Attribute
    {
    }
    public class AttributeUsage : Attribute
    {
        public AttributeUsage(AttributeTargets validOn) { }
    }
    public sealed class ParamArrayAttribute : Attribute
    {
        public ParamArrayAttribute(AttributeTargets validOn) { }
    }
    public enum AttributeTargets
    {
        Assembly = 1,
        Module = 2,
        Class = 4,
        Struct = 8,
        Enum = 16,
        Constructor = 32,
        Method = 64,
        Property = 128,
        Field = 256,
        Event = 512,
        Interface = 1024,
        Parameter = 2048,
        Delegate = 4096,
        ReturnValue = 8192,
        GenericParameter = 16384,
        All = 32767,
    }
    public sealed class Obsolete : Attribute
    {
        public Obsolete(){}
        public Obsolete(string message){}
        public Obsolete(string message, bool error) { }
    }
    [AReflexible]public sealed class NonSerialized : Attribute { }

    public partial class Exception
#if DEBUG
 : System.Exception
#endif
    {
        public Exception() { }
        public Exception(string message)
            : this(message, null)
        {
        }
        public Exception(string message, Exception innerException)
        {
            this.Message = message;
            this.InnerException = innerException;
        }

        public Exception InnerException { get; private set; }
        public virtual string Message { get; private set; }
        [ASystemAPI]public extern string StackTrace { get; }
    }
    public class NotImplementedException : Exception
    {
        public NotImplementedException() { }
        public NotImplementedException(string msg) : base(msg) { }
    }
    public class InvalidOperationException : Exception
    {
        public InvalidOperationException() { }
        public InvalidOperationException(string msg) : base(msg) { }
    }
    public class ArgumentException : Exception
    {
        public ArgumentException() { }
        public ArgumentException(string msg) : base(msg) { }
    }
    public class ArgumentNullException : Exception
    {
        public ArgumentNullException() { }
        public ArgumentNullException(string msg) : base(msg) { }
    }
    public class ArgumentOutOfRangeException : Exception
    {
        public ArgumentOutOfRangeException() { }
        public ArgumentOutOfRangeException(string msg) : base(msg) { }
    }
    public class IndexOutOfRangeException : Exception
    {
        public IndexOutOfRangeException() { }
        public IndexOutOfRangeException(string msg) : base(msg) { }
    }
    public class OverflowException : Exception
    {
        public OverflowException() { }
        public OverflowException(string msg) : base(msg) { }
    }
    public class OutOfMemoryException : Exception
    {
        public OutOfMemoryException() { }
        public OutOfMemoryException(string msg) : base(msg) { }
    }
    public class InvalidCastException : Exception
    {
        public InvalidCastException() { }
        public InvalidCastException(string msg) : base(msg) { }
    }
    public class FormatException : Exception
    {
        public FormatException() { }
        public FormatException(string msg) : base(msg) { }
    }
    public class NotSupportedException : Exception
    {
        public NotSupportedException() { }
        public NotSupportedException(string msg) : base(msg) { }
    }

    public delegate void Action();
    public delegate void Action<T>(T arg1);
    public delegate void Action<T1, T2>(T1 arg1, T2 arg2);
    public delegate void Action<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);
    public delegate void Action<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    public delegate T Func<T>();
    public delegate T Func<T1, T>(T1 t1);
    public delegate T Func<T1, T2, T>(T1 t1, T2 t2);
    public delegate T Func<T1, T2, T3, T>(T1 t1, T2 t2, T3 t3);
    public delegate T Func<T1, T2, T3, T4, T>(T1 t1, T2 t2, T3 t3, T4 t4);
    public delegate bool Predicate<T>(T obj);

    public enum StringSplitOptions
    {
        None = 0,
        RemoveEmptyEntries = 1,
    }

    [ASystemAPI]public static partial class Math
    {
        public static double Sqrt(double value) { throw new NotImplementedException(); }
        public static double Acos(double value) { throw new NotImplementedException(); }
        public static double Asin(double value) { throw new NotImplementedException(); }
        public static double Atan(double value) { throw new NotImplementedException(); }
        public static double Atan2(double y, double x) { throw new NotImplementedException(); }
        public static double Tan(double value) { throw new NotImplementedException(); }
        public static double Sin(double value) { throw new NotImplementedException(); }
        public static double Cos(double value) { throw new NotImplementedException(); }
        public static double Pow(double x, double y) { throw new NotImplementedException(); }
    }
    public static class Environment
    {
        private static string newLine = "\r\n";
        public static string NewLine
        {
            get { return newLine; }
        }
        public static string CurrentDirectory { get { return ""; } }
        public static void Exit(int code) { }
    }

    #region Type & Reflection

    public abstract class Type : MemberInfo
    {
        public static readonly Type[] EmptyTypes = new Type[0];

        protected Type() { }

        public abstract Assembly Assembly { get; }
        public string AssemblyQualifiedName { get { return string.Format("{0}, {1}", FullName, Assembly.FullName); } }
        public abstract Type BaseType { get; }
        public bool ContainsGenericParameters { get { return TypeParametersCount > 0; } }
        public abstract Type ReflectedType { get; }
        protected abstract int TypeParametersCount { get; }
        [AInvariant]public virtual string FullName
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                var ns = Namespace;
                if (!string.IsNullOrEmpty(ns))
                {
                    builder.Append(ns);
                    builder.Append(".");
                }
                int count = 0;
                Type parent = ReflectedType;
                for (; parent != null; count++)
                    parent = parent.ReflectedType;
                // 栈方式追加内部类的外层类
                for (int i = count; i > 0; i--)
                {
                    parent = this;
                    for (int j = 0; j < i; j++)
                        parent = parent.ReflectedType;
                    builder.Append(parent.Name);
                    builder.Append("+");
                }
                builder.Append(Name);
                if (IsGenericType)
                {
                    builder.Append("^");
                    if (IsGenericTypeDefinition)
                    {
                        // 泛型个数
                        builder.Append(this.TypeParametersCount);
                    }
                    else
                    {
                        // 泛型参数
                        var gtd = GetGenericTypeDefinition();
                        builder.Append(this.TypeParametersCount);
                        var typeArguments = GetGenericArguments();
                        builder.Append("[");
                        for (int i = 0, n = typeArguments.Length - 1; i <= n; i++)
                        {
                            var argument = typeArguments[i];
                            builder.Append("[");
                            builder.Append(argument.AssemblyQualifiedName);
                            builder.Append("]");
                            if (i != n)
                                builder.Append(", ");
                        }
                        builder.Append("]");
                    }
                }
                if (IsArray)
                {
                    Type element = this;
                    while (element != null && element.IsArray)
                    {
                        builder.Append("[]");
                        element = element.GetElementType();
                    }
                }
                return builder.ToString();
            }
        }
        public abstract int GenericParameterPosition { get; }
        public abstract bool HasElementType { get; }
        public abstract bool IsAbstract { get; }
        public abstract bool IsArray { get; }
        public abstract bool IsByRef { get; }
        public abstract bool IsClass { get; }
        public abstract bool IsEnum { get; }
        public abstract bool IsGenericParameter { get; }
        public abstract bool IsGenericType { get; }
        public abstract bool IsGenericTypeDefinition { get; }
        public abstract bool IsInterface { get; }
        public abstract bool IsNested { get; }
        public abstract bool IsNestedAssembly { get; }
        public abstract bool IsNestedFamily { get; }
        public abstract bool IsNestedFamORAssem { get; }
        public abstract bool IsNestedPrivate { get; }
        public abstract bool IsNestedPublic { get; }
        public abstract bool IsNotPublic { get; }
        public virtual bool IsNotSerialized
        {
            get
            {
#if !DEBUG
                return GetCustomAttributes(typeof(NonSerialized), true).Length > 0;
#else
                throw new NotImplementedException();
#endif
            }
        }
        public virtual bool IsPrimitive
        {
            get
            {
                string name = this.Name;
                return name == "System.Boolean"
                    || name == "System.Byte" || name == "System.SByte"
                    || name == "System.Char" || name == "System.UInt16" || name == "System.Int16"
                    || name == "System.UInt32" || name == "System.Int32" || name == "System.Single"
                    || name == "System.UInt64" || name == "System.Int64" || name == "System.Double";
            }
        }
        public abstract bool IsPublic { get; }
        public abstract bool IsSealed { get; }
        public abstract bool IsSpecialName { get; }
        public abstract bool IsValueType { get; }
        public abstract bool IsVisible { get; }
        public abstract string Namespace { get; }

        //public ConstructorInfo GetConstructor(Type[] types)
        //{
        //    var members = type.Members;
        //    for (int i = 0; i < members.Count; i++)
        //    {
        //        var member = members[i];
        //        if (member.IsConstructor && )
        //            return new ConstructorInfo(member);
        //    }
        //    return null;
        //}
        public ConstructorInfo[] GetConstructors()
        {
            return GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        }
        public abstract ConstructorInfo[] GetConstructors(BindingFlags bindingAttr);
        public abstract Type GetElementType();
        //public virtual string GetEnumName(object value)
        //{
        //    return null;
        //}
        //public virtual string[] GetEnumNames()
        //{
        //    return null;
        //}
        public abstract Type GetEnumUnderlyingType();
        //public virtual Array GetEnumValues()
        //{
        //    return null;
        //}
        public FieldInfo GetField(string name)
        {
            return GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
        }
        public abstract FieldInfo GetField(string name, BindingFlags bindingAttr);
        public FieldInfo[] GetFields()
        {
            return GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
        }
        public abstract FieldInfo[] GetFields(BindingFlags bindingAttr);
        public abstract Type[] GetGenericArguments();
        public abstract Type[] GetGenericParameterConstraints();
        public abstract Type GetGenericTypeDefinition();
        public abstract Type GetInterface(string name);
        public abstract Type[] GetInterfaces();
        public MethodInfo GetMethod(string name)
        {
            return GetMethod(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
        }
        public abstract MethodInfo GetMethod(string name, BindingFlags bindingAttr);
        //public MethodInfo GetMethod(string name, Type[] types)
        //{
        //    return null;
        //}
        public MethodInfo[] GetMethods()
        {
            return GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
        }
        public abstract MethodInfo[] GetMethods(BindingFlags bindingAttr);
        public Type GetNestedType(string name)
        {
            return GetNestedType(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
        }
        public abstract Type GetNestedType(string name, BindingFlags bindingAttr);
        public Type[] GetNestedTypes()
        {
            return GetNestedTypes(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
        }
        public abstract Type[] GetNestedTypes(BindingFlags bindingAttr);
        public PropertyInfo[] GetProperties()
        {
            return GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
        }
        public abstract PropertyInfo[] GetProperties(BindingFlags bindingAttr);
        public PropertyInfo GetProperty(string name)
        {
            return GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
        }
        public abstract PropertyInfo GetProperty(string name, BindingFlags bindingAttr);
        //public PropertyInfo GetProperty(string name, Type[] types)
        //{
        //    return null;
        //}
        public override string ToString()
        {
            return FullName;
        }
        public abstract bool IsSubclassOf(Type c);
        [ANonOptimize][AInvariant]public abstract bool IsAssignableFrom(Type c);

        [ASystemAPI]public extern static Type GetType(string typeName);

        [AInvariant]public abstract Type MakeArrayType();
        public abstract Type MakeByRefType();
        public abstract Type MakeGenericType(params Type[] typeArguments);
    }
    [AInvariant]public partial class Activator
    {
        [ASystemAPI]private static extern object CreateDefault(Type type);
        public static object CreateInstance(Type type)
        {
            var constructors = type.GetConstructors();
            bool hasConstructorFlag = false;
            for (int i = 0; i < constructors.Length; i++)
            {
                var constructor = constructors[i];
                if (constructor.IsStatic || !constructor.IsPublic)
                    continue;
                hasConstructorFlag = true;
                if (constructor.GetParameters().Length == 0)
                    return constructor.Invoke(MemberInfo._Empty);
            }
            if (!hasConstructorFlag)
            {
                // 应调用默认构造函数构造对象
                object obj = CreateDefault(type);
                if (obj != null)
                    return obj;
            }
            return null;
        }
        public static T CreateInstance<T>()
        {
#if !DEBUG
            object obj = CreateInstance(typeof(T));
            if (obj != null)
                return (T)obj;
            return default(T);
#else
            throw new NotImplementedException();
#endif
        }
    }
    public static partial class Convert
    {
        //internal static readonly RuntimeType[] ConvertTypes = new RuntimeType[]
        //{
        //    //(RuntimeType)typeof(Empty),
        //    null,
        //    (RuntimeType)typeof(object),
        //    (RuntimeType)typeof(DBNull),
        //    (RuntimeType)typeof(bool),
        //    (RuntimeType)typeof(char),
        //    (RuntimeType)typeof(sbyte),
        //    (RuntimeType)typeof(byte),
        //    (RuntimeType)typeof(short),
        //    (RuntimeType)typeof(ushort),
        //    (RuntimeType)typeof(int),
        //    (RuntimeType)typeof(uint),
        //    (RuntimeType)typeof(long),
        //    (RuntimeType)typeof(ulong),
        //    (RuntimeType)typeof(float),
        //    (RuntimeType)typeof(double),
        //    (RuntimeType)typeof(decimal),
        //    (RuntimeType)typeof(DateTime),
        //    (RuntimeType)typeof(object),
        //    (RuntimeType)typeof(string)
        //};

        [ASystemAPI]public static object ChangeType(object value, Type conversionType)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>copy from EntryEngine.Serialize.Binary</summary>
    public static class BitConverter
    {
        const int B23 = 8388607;
        const long B52 = 4503599627370495L;
        const double B23D = 1.0 / B23;
        const double B52D = 1.0 / B52;
        private static int GetBitCount(long value)
        {
            if (value == 0)
                return 0;
            int count = 1;
            while ((value >>= 1) != 0)
                count++;
            return count;
        }

        public static byte[] GetBytes(bool value)
        {
            byte[] bytes = new byte[1];
            bytes[0] = (byte)(value ? 1 : 0);
            return bytes;
        }
        public static byte[] GetBytes(sbyte value)
        {
            byte[] bytes = new byte[1];
            bytes[0] = (byte)(value + 127);
            return bytes;
        }
        public static byte[] GetBytes(byte value)
        {
            byte[] bytes = new byte[1];
            bytes[0] = value;
            return bytes;
        }
        public static byte[] GetBytes(short value)
        {
            byte[] bytes = new byte[2];
            bytes[0] = (byte)(value & 255);
            bytes[1] = (byte)(value >> 8);
            return bytes;
        }
        public static byte[] GetBytes(ushort value)
        {
            byte[] bytes = new byte[2];
            bytes[0] = (byte)(value & 255);
            bytes[1] = (byte)(value >> 8);
            return bytes;
        }
        public static byte[] GetBytes(char value)
        {
            return GetBytes((ushort)value);
        }
        public static byte[] GetBytes(int value)
        {
            byte[] bytes = new byte[4];
            bytes[0] = (byte)(value);
            bytes[1] = (byte)(value >> 8);
            bytes[2] = (byte)(value >> 16);
            bytes[3] = (byte)(value >> 24);
            return bytes;
        }
        public static byte[] GetBytes(uint value)
        {
            byte[] bytes = new byte[4];
            bytes[0] = (byte)(value);
            bytes[1] = (byte)(value >> 8);
            bytes[2] = (byte)(value >> 16);
            bytes[3] = (byte)(value >> 24);
            return bytes;
        }
        public static byte[] GetBytes(float value)
        {
            byte[] bytes = new byte[4];
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
            return GetBytes(v);
        }
        public static byte[] GetBytes(long value)
        {
            byte[] bytes = new byte[8];
            bytes[0] = (byte)(value & 255);
            bytes[1] = (byte)((value >> 8) & 255);
            bytes[2] = (byte)((value >> 16) & 255);
            bytes[3] = (byte)((value >> 24) & 255);
            bytes[4] = (byte)((value >> 32) & 255);
            bytes[5] = (byte)((value >> 40) & 255);
            bytes[6] = (byte)((value >> 48) & 255);
            bytes[7] = (byte)((value >> 56));
            return bytes;
        }
        public static byte[] GetBytes(ulong value)
        {
            byte[] bytes = new byte[8];
            bytes[0] = (byte)(value & 255);
            bytes[1] = (byte)((value >> 8) & 255);
            bytes[2] = (byte)((value >> 16) & 255);
            bytes[3] = (byte)((value >> 24) & 255);
            bytes[4] = (byte)((value >> 32) & 255);
            bytes[5] = (byte)((value >> 40) & 255);
            bytes[6] = (byte)((value >> 48) & 255);
            bytes[7] = (byte)((value >> 56));
            return bytes;
        }
        public static byte[] GetBytes(double value)
        {
            byte[] bytes = new byte[8];
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
            return GetBytes(v);
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
            return (bytes[index] | bytes[index + 1] << 8 | bytes[index + 2] << 16 | bytes[index + 3] << 24);
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
                Math.Pow(2, ((b4 & 127) << 1 | b3 >> 7) - 127));
        }
        public static long ToInt64(byte[] bytes, int index)
        {
            return (bytes[index] | bytes[index + 1] << 8 | bytes[index + 1] << 16 | bytes[index + 1] << 24
                | (long)(bytes[index + 4] << 32 | bytes[index + 5] << 40 | bytes[index + 6] << 48 | bytes[index + 7] << 56));
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

    [ANonOptimize][AInvariant]internal sealed class SimpleType : RuntimeType
    {
        [AInvariant]private string name;
        [ANonOptimize][AInvariant]public SimpleType(string name) : base(_R.AllocType("System.Object")) { this.name = name; }
        public override string Name { get { return this.name; } }
        public override bool Equals(object obj)
        {
            return obj is SimpleType && ((SimpleType)obj).name == this.name;
        }
        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
    }
    [ANonOptimize][AInvariant]internal class RuntimeType : Type
    {
        [AInvariant]internal CSharpType type;

        protected RuntimeType() { }
        [AInvariant]internal RuntimeType(CSharpType type)
        {
            this.type = type;
        }

        public override Assembly Assembly { get { return _R.AllocAssembly(type); } }
        public override string Name { get { return type.Name.Name; } }
        public override string FullName { get { return CSharpType.GetRuntimeTypeName(type); } }
        public override Type BaseType { get { return _R.FromType(type.BaseClass); } }
        protected override int TypeParametersCount { get { return type.TypeParametersCount; } }
        public override Type DeclaringType { get { return _R.FromType(type.DefiningType); } }
        public override Type ReflectedType { get { return _R.FromType(type.ContainingType); } }
        public override int GenericParameterPosition { get { return type.TypeParameterPosition; } }
        public override bool HasElementType { get { return type.ElementType != null; } }
        public override bool IsAbstract { get { return type.IsAbstract; } }
        public override bool IsArray { get { return type.IsArray; } }
        public override bool IsByRef { get { return type.IsPointer; } }
        public override bool IsClass { get { return type.IsClass; } }
        public override bool IsEnum { get { return type.IsEnum; } }
        public override bool IsGenericParameter { get { return type.IsTypeParameter; } }
        public override bool IsGenericType { get { return type.IsConstructed || ContainsGenericParameters; } }
        public override bool IsGenericTypeDefinition { get { return type.DefiningType == null && ContainsGenericParameters; } }
        public override bool IsInterface { get { return type.IsInterface; } }
        public override bool IsNested { get { return type.DefiningType != null; } }
        public override bool IsNestedAssembly { get { return IsNested && type.IsInternal; } }
        public override bool IsNestedFamily { get { return IsNested && type.IsProtected; } }
        public override bool IsNestedFamORAssem { get { return IsNested && type.IsProtectedOrInternal; } }
        public override bool IsNestedPrivate { get { return IsNested && type.IsPrivate; } }
        public override bool IsNestedPublic { get { return IsNested && type.IsPublic; } }
        public override bool IsNotPublic { get { return !type.IsPublic; } }
        public override bool IsPublic { get { return type.IsPublic; } }
        public override bool IsSealed { get { return type.IsSealed; } }
        public override bool IsSpecialName { get { return false; } }
        public override bool IsValueType { get { return type.IsValueType; } }
        public override bool IsVisible { get { return true; } }
        public override string Namespace { get { return type.ContainingNamespace == null ? null : type.ContainingNamespace.ToString(); } }

        //public ConstructorInfo GetConstructor(Type[] types)
        //{
        //    var members = type.Members;
        //    for (int i = 0; i < members.Count; i++)
        //    {
        //        var member = members[i];
        //        if (member.IsConstructor && )
        //            return new ConstructorInfo(member);
        //    }
        //    return null;
        //}
        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            List<RuntimeConstructorInfo> list = new List<RuntimeConstructorInfo>(4);
            var members = type.Members;
            for (int i = 0; i < members.Count; i++)
            {
                var member = members[i];
                if (member.IsConstructor && TestBindingFlags(bindingAttr, member))
                    list.Add(new RuntimeConstructorInfo(member));
            }
            return list.ToArray();
        }
        public override Type GetElementType()
        {
            if (!HasElementType)
                return null;
            return _R.FromType(type.ElementType);
        }
        //public virtual string GetEnumName(object value)
        //{
        //    return null;
        //}
        //public virtual string[] GetEnumNames()
        //{
        //    return null;
        //}
        public override Type GetEnumUnderlyingType()
        {
            if (!type.IsEnum)
                return null;
            return _R.FromType(type.UnderlyingType);
        }
        //public virtual Array GetEnumValues()
        //{
        //    return null;
        //}
        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            var members = type.Members;
            for (int i = 0; i < members.Count; i++)
            {
                var member = members[i];
                if (member.IsField && member.Name.Name == name && TestBindingFlags(bindingAttr, member))
                    return new RuntimeFieldInfo(member);
            }
            return null;
        }
        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            List<RuntimeFieldInfo> list = new List<RuntimeFieldInfo>(16);
            var members = type.Members;
            for (int i = 0; i < members.Count; i++)
            {
                var member = members[i];
                if (member.IsField && TestBindingFlags(bindingAttr, member))
                    list.Add(new RuntimeFieldInfo(member));
            }
            return list.ToArray();
        }
        public override Type[] GetGenericArguments()
        {
            if (type.IsConstructed)
            {
                var arguments = type.TypeArguments;
                RuntimeType[] result = new RuntimeType[arguments.Count];
                for (int i = 0; i < result.Length; i++)
                    result[i] = _R.FromType(arguments[i]);
                return result;
            }
            return RuntimeType.EmptyTypes;
        }
        public override Type[] GetGenericParameterConstraints()
        {
            var constraints = type.TypeConstraints;
            if (constraints.Count == 0)
                return RuntimeType.EmptyTypes;
            RuntimeType[] types = new RuntimeType[constraints.Count];
            for (int i = 0; i < types.Length; i++)
                types[i] = _R.FromType(constraints[i]);
            return types;
        }
        public override Type GetGenericTypeDefinition()
        {
            if (type.IsConstructed)
                return _R.FromType(type.DefiningType);
            return null;
        }
        public override Type GetInterface(string name)
        {
            var interfaces = CSharpType.GetAllBaseInterfaces(type.BaseInterfaces);
            if (interfaces.Count == 0)
                return null;
            for (int i = 0; i < interfaces.Count; i++)
                if (interfaces[i].Name.Name == name)
                    return _R.FromType(interfaces[i]);
            return null;
        }
        public override Type[] GetInterfaces()
        {
            var interfaces = CSharpType.GetAllBaseInterfaces(type.BaseInterfaces);
            if (interfaces.Count == 0)
                return EmptyTypes;
            RuntimeType[] types = new RuntimeType[interfaces.Count];
            for (int i = 0; i < interfaces.Count; i++)
                types[i] = _R.FromType(interfaces[i]);
            return types;
        }
        public override MethodInfo GetMethod(string name, BindingFlags bindingAttr)
        {
            //CSharpMember matched = null;
            var members = type.Members;
            for (int i = 0; i < members.Count; i++)
            {
                var member = members[i];
                if (member.IsMethod && member.Name.Name == name && TestBindingFlags(bindingAttr, member))
                {
                    //if (matched != null)
                    //    throw new Reflection.AmbiguousMatchException();
                    //matched = member;
                    return new RuntimeMethodInfo(member);
                }
            }
            return null;
        }
        //public MethodInfo GetMethod(string name, Type[] types)
        //{
        //    return null;
        //}
        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            List<RuntimeMethodInfo> list = new List<RuntimeMethodInfo>(16);
            var members = type.Members;
            for (int i = 0; i < members.Count; i++)
            {
                var member = members[i];
                if (member.IsMethod && TestBindingFlags(bindingAttr, member))
                    list.Add(new RuntimeMethodInfo(member));
            }
            return list.ToArray();
        }
        public override Type GetNestedType(string name, BindingFlags bindingAttr)
        {
            var nested = type.NestedTypes;
            for (int i = 0; i < nested.Count; i++)
            {
                var t = nested[i];
                if (t.Name.Name == name && TestBindingFlags(bindingAttr, t))
                    return _R.FromType(t);
            }
            return null;
        }
        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
        {
            List<RuntimeType> list = new List<RuntimeType>(4);
            var nested = type.NestedTypes;
            for (int i = 0; i < nested.Count; i++)
                if (TestBindingFlags(bindingAttr, nested[i]))
                    list.Add(_R.FromType(nested[i]));
            return list.ToArray();
        }
        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            List<RuntimePropertyInfo> list = new List<RuntimePropertyInfo>(16);
            var members = type.Members;
            for (int i = 0; i < members.Count; i++)
            {
                var member = members[i];
                if (member.IsProperty && TestBindingFlags(bindingAttr, member))
                    list.Add(new RuntimePropertyInfo(member));
            }
            return list.ToArray();
        }
        public override PropertyInfo GetProperty(string name, BindingFlags bindingAttr)
        {
            //CSharpMember matched = null;
            var members = type.Members;
            for (int i = 0; i < members.Count; i++)
            {
                var member = members[i];
                if (member.IsProperty && member.Name.Name == name && TestBindingFlags(bindingAttr, member))
                {
                    //if (matched != null)
                    //    throw new Reflection.AmbiguousMatchException();
                    //matched = member;
                    return new RuntimePropertyInfo(member);
                }
            }
            return null;
        }
        //public PropertyInfo GetProperty(string name, Type[] types)
        //{
        //    return null;
        //}
        public override bool IsAssignableFrom(Type c)
        {
            return type.IsAssignableFrom(((RuntimeType)c).type);
        }
        public override bool IsSubclassOf(Type c)
        {
            return type.IsSubclassOf(((RuntimeType)c).type);
        }

        internal static bool TestBindingFlags(BindingFlags flags, CSharpMember member)
        {
            bool result = false;
            result |= (flags & BindingFlags.Instance) != BindingFlags.Default && !member.IsStatic;
            result |= (flags & BindingFlags.Static) != BindingFlags.Default && member.IsStatic;
            result |= (flags & BindingFlags.Public) != BindingFlags.Default && member.IsPublic;
            result |= (flags & BindingFlags.NonPublic) != BindingFlags.Default && !member.IsPublic;
            return result;
        }
        internal static bool TestBindingFlags(BindingFlags flags, CSharpType type)
        {
            bool result = false;
            result |= (flags & BindingFlags.Instance) != BindingFlags.Default && !type.IsStatic;
            result |= (flags & BindingFlags.Static) != BindingFlags.Default && type.IsStatic;
            result |= (flags & BindingFlags.Public) != BindingFlags.Default && type.IsPublic;
            result |= (flags & BindingFlags.NonPublic) != BindingFlags.Default && !type.IsPublic;
            return result;
        }
        //internal static bool TestType(Type[] needs, Type[] types)
        //{
        //}

        public override Type MakeArrayType()
        {
            return new RuntimeType(CSharpType.CreateArray(1, type));
        }
        public override Type MakeByRefType()
        {
            // BUG: maybe
            return new RuntimeType(CSharpType.CreatePointer(type));
            //return _R.GetType(_R.AllocType("System.Ref^1")).MakeGenericType(this);
        }
        public override Type MakeGenericType(params Type[] typeArguments)
        {
            var typeParameters = type.TypeParameters;
            int count = typeParameters.Count;
            if (count != typeArguments.Length)
                return null;
            var ta = new CSharpType[count];
            for (int i = 0; i < count; i++)
                ta[i] = ((RuntimeType)typeArguments[i]).type;
            return new RuntimeType(CSharpType.CreateConstructedType(type, type.ContainingType, ta));
        }
        public override bool Equals(object obj)
        {
            RuntimeType target = (RuntimeType)obj;
            if (target == null)
                return false;
            return type.Equals(target.type);
        }
        public override int GetHashCode()
        {
            return type.GetHashCode();
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            List<object> list = new List<object>();
            foreach (var item in type.Attributes)
                if (inherit || item.DefiningObject == type)
                    list.Add(item);
            return list.ToArray();
        }
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            CSharpType attType = ((RuntimeType)attributeType).type;
            List<object> list = new List<object>();
            foreach (var item in type.Attributes)
                if (item.Type == attType && (inherit || item.DefiningObject == type))
                    list.Add(item);
            return list.ToArray();
        }
    }

    #endregion
}
