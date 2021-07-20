using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntryEngine
{
	public static partial class _MATH
	{
		static _MATH()
		{
			DIVIDE_BY_2048 = new float[2049];
			for (int i = 0; i <= 2048; i++)
			{
				DIVIDE_BY_2048[i] = 2048.0f / i;
			}
			DIVIDE_BY_1 = new float[2049];
			for (int i = 0; i <= 2048; i++)
			{
				DIVIDE_BY_1[i] = 1.0f / i;
			}
		}

		public const float PI = 3.141593f;
		public const float PI_OVER_2 = PI / 2;
		public const float PI_OVER_4 = PI / 4;
		public const float PI_2 = PI * 2;
		public static int MinValue;
		public static int MaxValue;
		public static int MaxSize = 2048;
		public static float FloatError = 0.0005f;
		public readonly static float[] DIVIDE_BY_2048;
        /// <summary>1 ~ 2048的倒数(reciprocal)</summary>
		public readonly static float[] DIVIDE_BY_1;
        public readonly static ushort[] POW_2_SIZE = { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048 };
		public const float DIVIDE_2048 = 1f / 2048f;
        /// <summary>角度乘以变量转换成弧度</summary>
        public const float D2R = 0.0174532924f;
        /// <summary>弧度乘以变量转换成角度</summary>
        public const float R2D = 57.2957764f;

		public static float Abs(float value)
		{
			return value < 0 ? -value : value;
		}
		public static int Abs(int value)
		{
			return (int)Abs((float)value);
		}
		public static void Floor(ref float value1, ref float value2)
		{
			// (int)(0.1 - -0.9) = 0
			// 0.1 - -0.9 = (int)1.0 = 1
			float dis = value2 - value1;
			dis = (int)dis;
			value1 = Round(value1);
			value2 = value1 + dis;
		}
		public static void Floor(float value1, float value2, out int result1, out int result2)
		{
			Floor(ref value1, ref value2);
			result1 = (int)value1;
			result2 = (int)value2;
		}
        /// <summary>小于浮点数的最大整数</summary>
        public static int Floor(float value)
        {
            int v = (int)value;
            if (v != value && value < 0)
                v--;
            return v;
        }
        /// <summary>大于浮点数的最小整数</summary>
        public static int Ceiling(float value)
        {
            int v = (int)value;
            if (v != value && value > 0)
                v++;
            return v;
        }
        /// <summary>四舍五入的整数</summary>
		public static int Round(float value)
		{
			if (value >= 0)
				value += 0.5f;
			else
				value -= 0.5f;
			return (int)value;
		}
		public static float Max(float x, float y)
		{
			return x < y ? y : x;
		}
		public static int Max(int x, int y)
		{
            return x < y ? y : x;
		}
		public static float Min(float x, float y)
		{
			return x > y ? y : x;
		}
		public static int Min(int x, int y)
		{
            return x > y ? y : x;
		}
		public static int Sign(float value)
		{
			if (value == 0)
				return 0;
			else if (value > 0)
				return 1;
			else
				return -1;
		}
		public static float ToDegree(float value)
		{
            return value * R2D;
		}
		public static float ToRadian(float value)
		{
            return value * D2R;
		}
		public static float Distance(float x, float y)
		{
			return Abs(x - y);
		}
		public static int Distance(int x, int y)
		{
			return Abs(x - y);
		}
		/// <summary>
		/// 取两个数之间的值
		/// </summary>
		/// <param name="value">值</param>
		/// <param name="min">最小值</param>
		/// <param name="max">最大值</param>
		/// <returns>最小值与最大值之间的值</returns>
		public static float Clamp(float value, float min, float max)
		{
			value = ((value > max) ? max : value);
			value = ((value < min) ? min : value);
			return value;
		}
		public static int Clamp(int value, int min, int max)
		{
			return (int)Clamp((float)value, min, max);
		}
		/// <summary>
		/// 0 ~ 1
		/// </summary>
		public static float InOne(float value)
		{
			return Clamp(value, 0, 1);
		}
		/// <summary>
		/// -1 ~ 1
		/// </summary>
		public static float InTwo(float value)
		{
			return Clamp(value, -1, 1);
		}
		/// <summary>
		/// 大于等于1
		/// </summary>
		public static float Positive(float value)
		{
			return Max(value, 1);
		}
		public static int Positive(int value)
		{
			return Max(value, 1);
		}
		/// <summary>
		/// 小于等于0
		/// </summary>
		public static float Negative(float value)
		{
			return Min(value, 0);
		}
		public static int Negative(int value)
		{
			return Min(value, 0);
		}
		/// <summary>
		/// 自然数
		/// </summary>
		public static float Nature(float value)
		{
			return Max(value, 0);
		}
		public static int Nature(int value)
		{
			return Max(value, 0);
		}
		/// <summary>
		/// 区间：通过MinValue和MaxValue自定义区间
		/// </summary>
		public static float Interval(float value)
		{
			return Clamp(value, MinValue, MaxValue);
		}
		public static int Interval(int value)
		{
			return Clamp(value, MinValue, MaxValue);
		}
		/// <summary>
		/// 0 ~ 255
		/// </summary>
		public static byte InByte(float value)
		{
			return (byte)Clamp(value, 0, 255);
		}
		/// <summary>
		/// 倍数值：倍数20，取值则是20的倍数，如-20, 0, 20, 40
		/// </summary>
		/// <param name="multiple">倍数值</param>
		/// <returns>最接近倍数值倍数的值</returns>
		public static float Multiple(float value, float multiple)
		{
            int sign = Sign(value);
            value = Abs(value);
            multiple = Abs(multiple);
			return ((int)(value / multiple) +
				(value % multiple >= multiple * 0.5f ? 1 : 0))
				* multiple * sign;
		}
		public static int Multiple(int value, int multiple)
		{
			return (int)Multiple((float)value, multiple);
		}
		/// <summary>
		/// 近似值：值与近似值的差值小于差值则取近似值，否则取值
		/// </summary>
		/// <param name="near">近似值</param>
		/// <param name="diffrence">差值</param>
		/// <returns>近似值或者值</returns>
		public static float Near(float value, float near, float diffrence)
		{
			return Distance(value, near) <= diffrence ? near : value;
		}
		public static int Near(int value, int near, int diffrence)
		{
			return (int)Near((float)value, near, diffrence);
		}
		public static float Near(float value, float near)
		{
			return Near(value, near, FloatError);
		}
		public static bool IsNear(float value, float near)
		{
			return Near(value, near) == near;
		}
		/// <summary>
		/// 靠近目标值：100 -> 1，速度5，到5之前，return -5，然后return -4
		/// </summary>
		/// <param name="value">当前值</param>
		/// <param name="target">要靠近的目标值</param>
		/// <param name="speed">靠近速度（无论方向）</param>
		/// <returns>靠近速度（带方向）</returns>
		public static float CloseToSpeed(float value, float target, float speed)
		{
			int direction = Sign(target - value);
			speed = Abs(speed);
			float diff = Distance(target, value);
			if (speed > diff)
				speed = diff;
			return speed * direction;
		}
		public static int CloseToSpeed(int value, int target, int speed)
		{
			return (int)CloseToSpeed((float)value, (float)target, (float)speed);
		}
		/// <summary>
		/// 靠近目标值：100 -> 1，速度5，return 95,90...5,1
		/// </summary>
		/// <param name="value">当前值</param>
		/// <param name="target">要靠近的目标值</param>
		/// <param name="speed">靠近速度（无论方向）</param>
		/// <returns>靠近后的值</returns>
		public static float CloseTo(float value, float target, float speed)
		{
			int direction = Sign(target - value);
			speed = Abs(speed);
			float diff = Distance(target, value);
			if (speed > diff)
				return target;
			else
				return value + speed * direction;
		}
		public static int CloseTo(int value, int target, int speed)
		{
			return (int)CloseTo((float)value, (float)target, (float)speed);
		}
		public static float Lerp(float value1, float value2, float amount)
		{
			return value1 + (value2 - value1) * amount;
		}
		public static int Lerp(int value1, int value2, float amount)
		{
			return (int)Lerp((float)value1, (float)value2, amount);
		}
		/// <summary>
		/// 靠近目标角度
		/// </summary>
		/// <param name="value">当前角度</param>
		/// <param name="target">目标角度</param>
		/// <param name="speed">靠近速度（无论方向）</param>
		/// <returns>靠近后的角度</returns>
		public static float CloseToAngle(float value, float target, float speed)
		{
			float diff = Closewise(value, target);
			int sign = Sign(diff);
			speed = Abs(speed);
			if (speed > Abs(diff))
				return target;
			else
				return value + speed * sign;
		}
		public static int CloseToAngle(int value, int target, int speed)
		{
			return (int)CloseToAngle((float)value, (float)target, (float)speed);
		}
		/// <summary>
		/// 将值转换为min ~ max区间内的值
		/// </summary>
		/// <param name="min">区间最小值</param>
		/// <param name="max">区间最大值</param>
		/// <returns>min ~ max</returns>
		public static float Range(float value, float min, float max)
		{
			return Range(value - min, max - min) + min;
		}
		public static int Range(int value, int min, int max)
		{
			return (int)Range((float)value, min, max);
		}
		/// <summary>
		/// 将值转换为0 ~ max区间内的值（不包含max）
		/// </summary>
		/// <param name="max">区间最大值</param>
		/// <returns>0 ~ max</returns>
		public static float Range(float value, float max)
		{
			value %= max;
			value += max;
			value %= max;
			return value;
		}
		public static int Range(int value, int max)
		{
			return (int)Range((float)value, max);
		}
		public static float RangeAngle(float value)
		{
			return Range(value, 360);
		}
		public static float RangeRadian(float value)
		{
			return Range(value, PI_2);
		}
		/// <summary>
		/// 值在区间内的比例
		/// </summary>
		/// <param name="min">区间最小值</param>
		/// <param name="max">区间最大值</param>
		/// <returns>值在区间内的比例</returns>
		public static float RangeRate(float value, float min, float max)
		{
			return (value - min) / (max - min);
		}
		/// <summary>
		/// 百分比在区间内对应的值：比例小于0或大于1会溢出区间
		/// </summary>
		/// <param name="rate">比例</param>
		/// <param name="min">区间最小值</param>
		/// <param name="max">区间最大值</param>
		/// <returns>百分比在区间内对应的值</returns>
		public static float RangeValue(float rate, float min, float max)
		{
			return rate * (max - min) + min;
		}
		public static int Reverse(int value)
		{
			// reverse '54321' to '12345'
			int len = value.ToString().Length;
            int pow = (int)Math.Pow(10, len);
			int temp = value;
			value = 0;
			int rpow = 1;
			for (int i = 0; i < len; i++)
			{
				value += (temp % pow) / (pow / 10) * rpow;
				pow /= 10;
				rpow *= 10;
			}
			return value;
		}
		/// <summary>
		/// 将值缩放到规格范围内
		/// </summary>
		/// <param name="norm">规格</param>
		/// <param name="x">缩放x</param>
		/// <param name="y">缩放y</param>
		/// <returns>缩放规格</returns>
		public static float Nomalize(float norm, ref float x, ref float y)
		{
			float factor = NomalizeFactor(norm, x, y);
			x *= factor;
			y *= factor;
			return factor;
		}
		/// <summary>
		/// 规格为1
		/// </summary>
		public static float Nomalize(ref float x, ref float y)
		{
			return Nomalize(1, ref x, ref y);
		}
		/// <summary>
		/// 将值缩放到规格范围内
		/// </summary>
		/// <param name="norm">规格</param>
		/// <param name="values">要缩放的值</param>
		/// <returns>缩放规格</returns>
		public static float Nomalize(float norm, float[] values)
		{
			float factor = NomalizeFactor(norm, values);
			for (int i = 0; i < values.Length; i++)
				values[i] = values[i] * factor;
			return factor;
		}
		/// <summary>
		/// 计算缩放规格
		/// </summary>
		/// <param name="norm">规格</param>
		/// <returns>缩放规格</returns>
		public static float NomalizeFactor(float norm, params float[] values)
		{
            return norm / (float)Math.Sqrt(Dot(values));
		}
		/// <summary>
		/// a * a + b * b + c * c......
		/// </summary>
		/// <returns>a * a + b * b + c * c......</returns>
		public static float Dot(params float[] values)
		{
			float value = 0;
			foreach (float v in values)
				value += v * v;
			return value;
		}
		/// <summary>
		/// 矩形外接圆半径
		/// </summary>
		/// <returns>外接圆半径</returns>
		public static float CircumcircleRadius(float width, float height)
		{
			return TheThird(width, height) / 2;
		}
		/// <summary>
		/// Sqrt(a * a + b * b)
		/// </summary>
		/// <returns>Sqrt(a * a + b * b)</returns>
		public static float TheThird(float a, float b)
		{
            return (float)Math.Sqrt(Dot(a, b));
		}
		/// <summary>
		/// 斐波那契数列
		/// </summary>
		/// <param name="index">数列索引</param>
		/// <returns>数列中的项</returns>
		public static int Fibonacci(int index)
		{
			int first = 0;
			int second = 1;
			for (int i = 0; i < index; i++)
			{
				first += second;
				second = first - second;
			}
			return first;
		}
		/// <summary>
		/// 斐波那契数列逆运算
		/// </summary>
		/// <param name="value">数列值</param>
		/// <returns>数列中的索引</returns>
		public static int FibonacciInverse(int value)
		{
			int first = 0;
			int second = 1;
			for (int i = 0; i < int.MaxValue; i++)
			{
				first += second;
				second = first - second;
				if (second >= value)
				{
					return i;
				}
			}
			return -1;
		}
		/// <summary>
		/// 斐波那契数列作为经验值的等级计算
		/// </summary>
		/// <param name="level">要达到的等级</param>
		/// <param name="exp">达到等级需要的经验</param>
		/// <param name="currentExp">当前经验</param>
		/// <returns>当前等级</returns>
		public static int FibonacciLevel(int level, int exp, int currentExp)
		{
			int lv = _MATH.FibonacciInverse(exp);
			float expRate = _MATH.Fibonacci(lv - 1) * 1.0f / exp;
			return _MATH.FibonacciInverse((int)(currentExp * expRate)) * level / lv;
		}
		/// <summary>
		/// 等差数列
		/// </summary>
		/// <param name="a0">等差数列的第一项</param>
		/// <param name="d">差值</param>
		/// <param name="n">项索引</param>
		/// <returns>第n项</returns>
		public static float Arithmetic(float a0, float d, int n)
		{
			return a0 + d * n;
		}
		public static int Arithmetic(int a0, int d, int n)
		{
			return (int)Arithmetic(a0, d, n);
		}
		/// <summary>
		/// 等比数列
		/// </summary>
		/// <param name="a0">等比数列第一项</param>
		/// <param name="q">比值</param>
		/// <param name="n">项索引</param>
		/// <returns>第n项</returns>
		public static float Geometric(float a0, float q, int n)
		{
			float an = a0;
			for (int i = 0; i < n; i++)
			{
				an *= q;
			}
			return an;
		}
		public static int Geometric(int a0, int q, int n)
		{
			return (int)Geometric(a0, q, n);
		}
		/// <summary>
		/// <para>排列</para>
		/// <para>有count个数，总共能组成多少个factor位数</para>
		/// <para>设count=9,factor=3,a93=9*8*7=504</para>
		/// </summary>
		/// <param name="count">总数</param>
		/// <param name="factor">位数</param>
		/// <returns>总共的排列数</returns>
		public static ulong Arrangement(int count, int factor)
		{
			if (factor == 0 || count <= 0 || count < factor)
				return 0;

			uint temp = (uint)count;
			ulong total = temp;
			for (uint i = 1; i < factor; i++)
				total *= (temp - i);
			return total;
		}
		/// <summary>
		/// <para>组合</para>
		/// <para>有count个人，factor人为一组，总共能组成多少个不同组合</para>
		/// <para>设count=9,factor=3,c93=(9*8*7)/(3*2*1)=84</para>
		/// </summary>
		/// <param name="count">总数</param>
		/// <param name="factor">组数</param>
		/// <returns>总共的组合数</returns>
		public static ulong Combination(int count, int factor)
		{
			ulong a = Arrangement(factor, factor);
			if (a == 0)
				return 0;
			else
				return Arrangement(count, factor) / a;
		}
		/// <summary>
		/// <para>组合</para>
		/// <para>有count个人，factor人为一组，总共能组成多少个不同组合</para>
		/// <para>设count=9,factor=3,c93=(9*8*7)/(3*2*1)=84</para>
		/// </summary>
		/// <param name="array">要组合的数组</param>
		/// <param name="factor">组数</param>
		/// <returns>所有的组合结果</returns>
		public static IEnumerable<T[]> Combination<T>(this IList<T> array, int factor)
		{
			T[] result = new T[factor];
			foreach (var item in CombinationRight(result, 0, array, 0))
				yield return item.ToArray();
		}
		/// <summary>
		/// 将组合视为树队列，每个节点不断向右
		/// 父节点向右一次，子节点都需要向右到最后
		/// </summary>
		/// <param name="result">组合结果</param>
		/// <param name="index">队列进入组合的索引</param>
		/// <param name="array">队列</param>
		/// <param name="queue">队列索引</param>
		/// <returns>所有的组合结果</returns>
		private static IEnumerable<T[]> CombinationRight<T>(T[] result, int index, IList<T> array, int queue)
		{
			for (; queue < array.Count; queue++)
			{
				result[index] = array[queue];
				if (index == result.Length - 1)
					yield return result;
				else
					foreach (var item in CombinationRight(result, index + 1, array, queue + 1))
						yield return result;
			}
		}
		/// <summary>
		/// 数组的全排列
		/// </summary>
		/// <param name="array">需要得出全排列的数组</param>
		/// <returns>全排列项</returns>
		public static IEnumerable<T[]> Arrangement<T>(IList<T> array)
		{
			return Arrangement(array, array.Count);
		}
		/// <summary>
		/// <para>排列</para>
		/// <para>有count个数，总共能组成多少个factor位数</para>
		/// <para>设count=9,factor=3,a93=9*8*7=504</para>
		/// </summary>
		/// <param name="array">要排列的数组</param>
		/// <param name="factor">位数</param>
		/// <returns>所有的排列结果</returns>
		public static IEnumerable<T[]> Arrangement<T>(IList<T> array, int factor)
		{
			var temp = new T[factor];
			foreach (var item in CombinationRight(temp, 0, array, 0))
				//foreach (var item in Combination(array, factor))
				foreach (var result in ArrangementRight(item, 0))
					yield return result.ToArray();
		}
		/// <summary>
		/// 要排列的每个元素向后面的每个元素交换得出结果
		/// 每个靠前的元素交换一次，后面的元素要重复次步骤
		/// </summary>
		/// <param name="array">待排列的数组</param>
		/// <param name="startIndex">交换的索引</param>
		/// <returns>数组的全排列</returns>
		private static IEnumerable<T[]> ArrangementRight<T>(T[] array, int startIndex)
		{
			int last = array.Length - 1;
			if (startIndex == last)
			{
				yield return array;
			}
			else
			{
				for (int i = startIndex; i <= last; i++)
				{
					Utility.Swap(ref array[startIndex], ref array[i]);
					foreach (var result in ArrangementRight(array, startIndex + 1))
						yield return result;
					Utility.Swap(ref array[startIndex], ref array[i]);
				}
			}
		}
        /// <summary>求一组正数的最大公约数</summary>
        /// <returns>最小为1</returns>
        public static int MaxDivisor(IEnumerable<int> values)
        {
            int min = values.Min();
            // 最小数的质因数的乘积的排列组合就是有可能的公约数
            List<int> prime1 = new List<int>();
            List<int> prime2 = new List<int>();
            prime1.Add(min);
            for (int i = 2, e = (int)Math.Sqrt(min); i <= e; i++)
            {
                if (min % i == 0)
                {
                    prime2.Add(i);
                    prime1.Add(min / i);
                }
            }
            for (int i = 0; i < prime1.Count; i++)
            {
                int value = prime1[i];
                bool flag = true;
                foreach (var item in values)
                {
                    if (item % value != 0)
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                    return value;
            }
            for (int i = prime2.Count - 1; i >= 0; i--)
            {
                int value = prime2[i];
                bool flag = true;
                foreach (var item in values)
                {
                    if (item % value != 0)
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                    return value;
            }
            return 1;
        }

		/// <summary>
		/// 关于一个指定角度对称
		/// </summary>
		/// <param name="value">角度</param>
		/// <param name="angle">对称角</param>
		/// <returns>对称的角度</returns>
		public static float Symmetry(float value, float angle)
		{
			return angle + (angle - value);
		}
		/// <summary>
		/// 相对于Y轴的对称角
		/// </summary>
		/// <param name="value">角度</param>
		/// <returns>对称的角度</returns>
		public static float SymmetryY(float value)
		{
			return 180 - value;
		}
		/// <summary>
		/// 相对于原点的对称角
		/// </summary>
		/// <param name="value">角度</param>
		/// <returns>对称的角度</returns>
		public static float SymmetryC(float value)
		{
			return 180 + value;
		}
		/// <summary>
		/// 相对于XY轴的对称角
		/// </summary>
		/// <param name="value">角度</param>
		/// <returns>对称的角度</returns>
		public static float SymmetryXY(float value)
		{
			return SymmetryY(-value);
		}
		//public static float SymmetryR(float value, float radian)
		//{
		//    return radian + (radian - value);
		//}
		//public static float SymmetryYR(float radian)
		//{
		//    return PI - radian;
		//}
		//public static float SymmetryCR(float radian)
		//{
		//    return PI + radian;
		//}
		//public static float SymmetryXYR(float radian)
		//{
		//    return SymmetryYR(-radian);
		//}
		public static bool Is90t270(float angle)
		{
			float temp = RangeAngle(angle);
			return temp >= 90 && temp < 270;
		}
		public static int ClosewiseSign(float value, float target)
		{
			return Sign(Closewise(value, target));
		}
		/// <summary>
		/// 角度1到角度2顺时针还是逆时针比较近
		/// </summary>
		/// <param name="value">当前角</param>
		/// <param name="target">目标角</param>
		/// <returns>角度差（带方向）</returns>
		public static float Closewise(float value, float target)
		{
            //value %= 360;
            //target %= 360;
            //float diff = (target - value) % 360;
            float diff = target - value;
            if (diff > 0)
            {
                if (diff > 180)
                    diff -= 360;
            }
            else
            {
                if (diff < -180)
                    diff += 360;
            }
			return diff;
        }
        /// <summary>
        /// 角度1到角度2顺时针还是逆时针比较近(角度范围必须是[-180~180])
        /// </summary>
        /// <param name="a1">当前角</param>
        /// <param name="a2">目标角</param>
        /// <returns>角度差（不带方向）</returns>
        public static float AngleDifference(float a1, float a2)
        {
            float diff = a2 - a1;
            if (diff > 180)
            {
                return 360 + a1 - a2;
            }
            else if (diff < -180)
            {
                return 360 + a2 - a1;
            }
            else
            {
                return diff < 0 ? -diff : diff;
            }
        }


        /// <summary>权重变化</summary>
        /// <param name="weight">当前待改变的权重</param>
        /// <param name="total">总权重</param>
        /// <param name="multiple">权重变为原来的倍数</param>
        /// <returns>权重的变化量</returns>
        public static float WeightVary(float weight, float total, float multiple)
        {
            /* 设增量为x
             * (w + x) / (t + x) = m * w / t
             * wt + xt = mwt + mwx
             * x(t - mw) = mwt - wt
             * x = (m - 1) * (w * t) / (t - m * w)
             */
            if (weight * multiple >= total)
            {
                return float.PositiveInfinity;
            }
            return ((multiple - 1) * weight * total) / (total - weight * multiple);
        }
        /// <summary>批量权重变化，例如750,125,125三个权重，希望125,125的两个权重翻倍</summary>
        /// <param name="weights">待改变的权重(125,125)</param>
        /// <param name="total">总权重(1000)</param>
        /// <param name="multiple">权重变为原来的倍数(2)</param>
        /// <returns>每个权重的变化量(250,250)，即最终权重应该变为(750,125+250,125+250)->(750,375,375)</returns>
        public static float[] WeightVary(float[] weights, float total, float multiple)
        {
            /* 将每个待变化的权重增加的权重和视为整体p
             * 就可同样带入单个计算的公式内得出p
             * 再根据每个待变化的权重在[[这些待变化的权重]中的权重]可得出p的占比
             * 设p = x1 + x2
             * (w1 + p) / (t + p) = m * w1 / t
             * (w2 + p) / (t + p) = m * w2 / t
             * => (w1 + p) / (w2 + p) = w1 / w2
             * m * (w1 + w2) / t = (w1 + w2 + p) / (t + p)
             * 设s = w1 + w2
             * m * s / t = (s + p) / (t + p)
             * ms + msp/t = s + p
             * (ms/t - 1)p = s - ms
             * p = (s - ms) / (ms / t - 1)
             */
            float sum = 0;
            int len = weights.Length;
            for (int i = 0; i < len; i++)
                sum += weights[i];
            // 结果权重
            float result = sum * multiple;
            // 权重乘以倍数后不能超过总权重
            if (result >= total)
                return null;
            float p = (sum - result) / (result / total - 1);
            float[] varies = new float[len];
            float _sum = 1 / sum;
            for (int i = 0; i < len; i++)
                varies[i] = weights[i] * _sum * p;
            return varies;
        }
    }
}
