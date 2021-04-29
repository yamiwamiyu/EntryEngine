#if CLIENT

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EntryEngine.UI;

namespace EntryEngine
{
    /// <summary>二维向量</summary>
    [AReflexible]public struct VECTOR2 : IEquatable<VECTOR2>
    {
        public float X;
        public float Y;
		public static readonly VECTOR2 NaN = new VECTOR2(float.NaN, float.NaN);
        private static VECTOR2 _zero = new VECTOR2();
		private static VECTOR2 _one = new VECTOR2(1, 1);
        private static VECTOR2 _half = new VECTOR2(0.5f, 0.5f);
		private static VECTOR2 _unitX = new VECTOR2(1, 0);
		private static VECTOR2 _unitY = new VECTOR2(0, 1);
		public static VECTOR2 Zero
		{
			get { return _zero; }
		}
		public static VECTOR2 One
		{
			get { return _one; }
		}
        public static VECTOR2 Half
        {
            get { return _half; }
        }
		public static VECTOR2 UnitX
		{
			get { return _unitX; }
		}
		public static VECTOR2 UnitY
		{
			get { return _unitY; }
		}
        public VECTOR2(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }
        public VECTOR2(float value)
        {
            this.Y = value;
            this.X = value;
        }
        public override string ToString()
        {
            return string.Format("X:{0} Y:{1}", X, Y);
        }
        public bool Equals(ref VECTOR2 other)
        {
            return this.X == other.X && this.Y == other.Y;
        }
        public bool Equals(VECTOR2 other)
        {
            return this.X == other.X && this.Y == other.Y;
        }
        public override bool Equals(object obj)
        {
            bool result = false;
            if (obj is VECTOR2)
            {
                result = this.Equals((VECTOR2)obj);
            }
            return result;
        }
        public override int GetHashCode()
        {
            return this.X.GetHashCode() + this.Y.GetHashCode();
        }
		public bool IsNaN()
		{
			return float.IsNaN(X) || float.IsNaN(Y);
		}
        public float Length()
        {
            return (float)Math.Sqrt(LengthSquared());
        }
        public float LengthSquared()
        {
            return this.X * this.X + this.Y * this.Y;
        }
        public void Normalize()
        {
            float num2 = 1f / (float)Math.Sqrt(this.X * this.X + this.Y * this.Y);
            this.X *= num2;
            this.Y *= num2;
        }
        public void Transform(ref MATRIX matrix)
        {
            X = X * matrix.M11 + Y * matrix.M21 + matrix.M41;
            Y = X * matrix.M12 + Y * matrix.M22 + matrix.M42;
        }
        public void Add(float x, float y)
        {
            X += x;
            Y += y;
        }
        public void Add(VECTOR2 vector)
        {
            Add(vector.X, vector.Y);
        }
        public void Minus(float x, float y)
        {
            X -= x;
            Y -= y;
        }
        public void Minus(VECTOR2 vector)
        {
            Minus(vector.X, vector.Y);
        }
        public void Multiple(float x, float y)
        {
            X *= x;
            Y *= y;
        }
        public void Multiple(VECTOR2 vector)
        {
            Multiple(vector.X, vector.Y);
        }
        public void Multiple(float scale)
        {
            this.X *= scale;
            this.Y *= scale;
        }
        public void Divide(float x, float y)
        {
            X /= x;
            Y /= y;
        }
        public void Divide(VECTOR2 vector)
        {
            Divide(vector.X, vector.Y);
        }
        public void Set(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }
		public void ToRoundInt()
		{
			X = _MATH.Round(X);
			Y = _MATH.Round(Y);
		}
		public static VECTOR2 Transform(VECTOR2 position, MATRIX2x3 matrix)
		{
			float x = position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M31;
			float y = position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M32;
			VECTOR2 result;
			result.X = x;
			result.Y = y;
			return result;
		}
		public static void Transform(ref VECTOR2 vector, ref MATRIX2x3 matrix)
		{
			float x = vector.X * matrix.M11 + vector.Y * matrix.M21 + matrix.M31;
			float y = vector.X * matrix.M12 + vector.Y * matrix.M22 + matrix.M32;
			vector.X = x;
			vector.Y = y;
		}
        public static void Transform(ref float x, ref float y, ref MATRIX2x3 matrix)
        {
            float x1 = x * matrix.M11 + y * matrix.M21 + matrix.M31;
            float y1 = x * matrix.M12 + y * matrix.M22 + matrix.M32;
            x = x1;
            y = y1;
        }
		public static void Transform(ref VECTOR2 position, ref MATRIX2x3 matrix, out VECTOR2 result)
		{
			float x = position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M31;
			float y = position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M32;
			result.X = x;
			result.Y = y;
		}
		public static void Transform(VECTOR2[] sourceArray, ref MATRIX2x3 matrix, VECTOR2[] destinationArray)
		{
			for (int i = 0; i < sourceArray.Length; i++)
			{
				float x = sourceArray[i].X;
				float y = sourceArray[i].Y;
				destinationArray[i].X = x * matrix.M11 + y * matrix.M21 + matrix.M31;
				destinationArray[i].Y = x * matrix.M12 + y * matrix.M22 + matrix.M32;
			}
		}
		public static void Transform(VECTOR2[] sourceArray, int sourceIndex, ref MATRIX2x3 matrix, VECTOR2[] destinationArray, int destinationIndex, int length)
		{
			while (length > 0)
			{
				float x = sourceArray[sourceIndex].X;
				float y = sourceArray[sourceIndex].Y;
				destinationArray[destinationIndex].X = x * matrix.M11 + y * matrix.M21 + matrix.M31;
				destinationArray[destinationIndex].Y = x * matrix.M12 + y * matrix.M22 + matrix.M32;
				sourceIndex++;
				destinationIndex++;
				length--;
			}
		}
		public static float Distance(VECTOR2 value1, VECTOR2 value2)
		{
			float result;
			Distance(ref value1, ref value2, out result);
			return result;
		}
		public static void Distance(ref VECTOR2 value1, ref VECTOR2 value2, out float result)
		{
			float num = value1.X - value2.X;
			float num2 = value1.Y - value2.Y;
			float num3 = num * num + num2 * num2;
            result = (float)Math.Sqrt(num3);
		}
        public static bool IsInDistance(ref VECTOR2 v1, ref VECTOR2 v2, float farest)
        {
            float num1 = v2.Y - v1.Y;
            float num2 = v2.X - v1.X;

            if (num1 < 0)
                num1 = -num1;
            if (num2 < 0)
                num2 = -num2;

            if (num1 + num2 <= farest)
                return false;

            //float num3 = num1 * _MATH.DIVIDE_2048 * num1 + num2 * _MATH.DIVIDE_2048 * num2;
            //return num3 <= farest * _MATH.DIVIDE_2048 * farest;

            float num3 = num1 * num1 + num2 * num2;
            return num3 <= farest * farest;
        }
        public static bool IsInDistance(float x1, float y1, float x2, float y2, float farest)
        {
            float num1 = y2 - y1;
            float num2 = x2 - x1;

            if (num1 < 0)
                num1 = -num1;
            if (num2 < 0)
                num2 = -num2;

            if (num1 + num2 <= farest)
                return false;

            float num3 = num1 * _MATH.DIVIDE_2048 * num1 + num2 * _MATH.DIVIDE_2048 * num2;
            return num3 <= farest * _MATH.DIVIDE_2048 * farest;
        }
		public static float DistanceSquared(VECTOR2 value1, VECTOR2 value2)
		{
			float result;
			DistanceSquared(ref value1, ref value2, out result);
			return result;
		}
		public static void DistanceSquared(ref VECTOR2 value1, ref VECTOR2 value2, out float result)
		{
			float num = value1.X - value2.X;
			float num2 = value1.Y - value2.Y;
			result = num * num + num2 * num2;
		}
		public static float Dot(VECTOR2 value1, VECTOR2 value2)
		{
			return value1.X * value2.X + value1.Y * value2.Y;
		}
		public static void Dot(ref VECTOR2 value1, ref VECTOR2 value2, out float result)
		{
			result = value1.X * value2.X + value1.Y * value2.Y;
		}
		public static VECTOR2 Normalize(VECTOR2 value)
		{
			VECTOR2 result;
			Normalize(ref value, out result);
			return result;
		}
		public static void Normalize(ref VECTOR2 value, out VECTOR2 result)
		{
			float num = value.X * value.X + value.Y * value.Y;
            float num2 = 1f / (float)Math.Sqrt(num);
			result.X = value.X * num2;
			result.Y = value.Y * num2;
		}
		public static VECTOR2 Min(VECTOR2 value1, VECTOR2 value2)
		{
			VECTOR2 result;
			Min(ref value1, ref value2, out result);
			return result;
		}
		public static void Min(ref VECTOR2 value1, ref VECTOR2 value2, out VECTOR2 result)
		{
			result.X = ((value1.X < value2.X) ? value1.X : value2.X);
			result.Y = ((value1.Y < value2.Y) ? value1.Y : value2.Y);
		}
		public static VECTOR2 Max(VECTOR2 value1, VECTOR2 value2)
		{
			VECTOR2 result;
			Max(ref value1, ref value2, out result);
			return result;
		}
		public static void Max(ref VECTOR2 value1, ref VECTOR2 value2, out VECTOR2 result)
		{
			result.X = ((value1.X > value2.X) ? value1.X : value2.X);
			result.Y = ((value1.Y > value2.Y) ? value1.Y : value2.Y);
		}
		public static VECTOR2 Clamp(VECTOR2 value, VECTOR2 min, VECTOR2 max)
		{
			VECTOR2 result;
			Clamp(ref value, ref min, ref max, out result);
			return result;
		}
		public static void Clamp(ref VECTOR2 value, ref VECTOR2 min, ref VECTOR2 max, out VECTOR2 result)
		{
			float num = value.X;
			num = ((num > max.X) ? max.X : num);
			num = ((num < min.X) ? min.X : num);
			float num2 = value.Y;
			num2 = ((num2 > max.Y) ? max.Y : num2);
			num2 = ((num2 < min.Y) ? min.Y : num2);
			result.X = num;
			result.Y = num2;
		}
		public static VECTOR2 Lerp(VECTOR2 value1, VECTOR2 value2, float amount)
		{
			VECTOR2 result;
			Lerp(ref value1, ref value2, amount, out result);
			return result;
		}
		public static void Lerp(ref VECTOR2 value1, ref VECTOR2 value2, float amount, out VECTOR2 result)
		{
			result.X = value1.X + (value2.X - value1.X) * amount;
			result.Y = value1.Y + (value2.Y - value1.Y) * amount;
		}
		public static float Towards(VECTOR2 current, VECTOR2 target, float angle)
		{
			float result;
			Towards(ref current, ref target, angle, out result);
			return result;
		}
		/// <summary>
		/// 朝向目标点
		/// </summary>
		/// <param name="current">当前坐标</param>
		/// <param name="target">目标坐标</param>
		/// <param name="angle">当前方向</param>
		/// <param name="direction">朝向目标角度最近的角度（带方向）</param>
		public static void Towards(ref VECTOR2 current, ref VECTOR2 target, float angle, out float direction)
		{
			float include;
			Degree(ref current, ref target, out include);
			direction = _MATH.Closewise(angle, include);
		}
		public static VECTOR2 Barycentric(VECTOR2 value1, VECTOR2 value2, VECTOR2 value3, float u, float v)
		{
			VECTOR2 result;
			Barycentric(ref value1, ref value2, ref value3, u, v, out result);
			return result;
		}
		/// <summary>
		/// 质心 / 重心：三条中线的交点
		/// </summary>
		/// <param name="u">value1 -> value2</param>
		/// <param name="v">value1 -> value3</param>
		public static void Barycentric(ref VECTOR2 value1, ref VECTOR2 value2, ref VECTOR2 value3, float u, float v, out VECTOR2 result)
		{
			result.X = value1.X + u * (value2.X - value1.X) + v * (value3.X - value1.X);
			result.Y = value1.Y + u * (value2.Y - value1.Y) + v * (value3.Y - value1.Y);
		}
		public static void Barycentric(float x, float y, float x1, float y1, float x2, float y2, float x3, float y3, out float u, out float v)
		{
			if (x3 == x1)
			{
				if (x2 != x1)
				{
					u = (x - x1) / (x2 - x1);
					// 点1和点3共点
					if (y3 == y1)
						v = 0;
					else
						v = (y - y1 + (y1 - y2) * u) / (y3 - y1);
				}
				else
				{
					// 4点x相等
					u = 0;
					// 点1和点2共点
					if (y2 == y1)
						v = 1;
					else
						v = (y - y1) / (y2 - y1);
				}
			}
			else if (y3 == y1)
			{
				if (y2 != y1)
				{
					u = (y - y1) / (y2 - y1);
					v = (x - x1 + (x1 - x2) * u) / (x3 - x1);
				}
				else
				{
					// 4点y相等
					v = 0;
					// 点1和点2共点
					if (x2 == x1)
						u = 1;
					else
						u = (x - x1) / (x2 - x1);
				}
			}
			else
			{
				float d31 = 1 / (x3 - x1);
				u = (y - y1 + (x * y1 - x1 * y1 - x * y3 + x1 * y3) * d31)
					/ (y2 - y1 + (x2 * y1 - x1 * y1 - x2 * y3 + x1 * y3) * d31);
				v = (x - x1 - (x2 - x1) * u) * d31;
			}
		}
		/// <summary>
		/// 质心权重 value1: w = 1 - u - v
		/// </summary>
		/// <param name="u">value2</param>
		/// <param name="v">value3</param>
		public static void Barycentric(ref VECTOR2 value, ref VECTOR2 value1, ref VECTOR2 value2, ref VECTOR2 value3, out float u, out float v)
		{
			if (value3.X == value1.X)
			{
				if (value2.X != value1.X)
				{
					u = (value.X - value1.X) / (value2.X - value1.X);
					// 点1和点3共点
					if (value3.Y == value1.Y)
						v = 0;
					else
						v = (value.Y - value1.Y + (value1.Y - value2.Y) * u) / (value3.Y - value1.Y);
				}
				else
				{
					// 4点value.X相等
					u = 0;
					// 点1和点2共点
					if (value2.Y == value1.Y)
						v = 1;
					else
						v = (value.Y - value1.Y) / (value2.Y - value1.Y);
				}
			}
			else if (value3.Y == value1.Y)
			{
				if (value2.Y != value1.Y)
				{
					u = (value.Y - value1.Y) / (value2.Y - value1.Y);
					v = (value.X - value1.X + (value1.X - value2.X) * u) / (value3.X - value1.X);
				}
				else
				{
					// 4点value.Y相等
					v = 0;
					// 点1和点2共点
					if (value2.X == value1.X)
						u = 1;
					else
						u = (value.X - value1.X) / (value2.X - value1.X);
				}
			}
			else
			{
				//value.X = (1 - u - v) * value1.X + u * value2.X + v * value3.X;
				//value.Y = (1 - u - v) * value1.Y + u * value2.Y + v * value3.Y;

				//value.X = value1.X - u * value1.X - v * value1.X + u * value2.X + v * value3.X;
				//value.X = value1.X + (value2.X - value1.X) * u + (value3.X - value1.X) * v;
				//v = (value.X - value1.X - (value2.X - value1.X) * u) / (value3.X - value1.X);
				float d31 = 1 / (value3.X - value1.X);
				//v = value.X * d31 - value1.X * d31 - value2.X * u * d31 + value1.X * u * d31;

				//value.Y = value1.Y - u * value1.Y - value.X * value1.Y * d31 + value1.X * value1.Y * d31 + u * value2.X * value1.Y * d31 - u * value1.X * value1.Y * d31
				//    + u * value2.Y
				//    + value.X * value3.Y * d31 - value1.X * value3.Y * d31 - u * value2.X * value3.Y * d31 + u * value1.X * value3.Y * d31;

				//value.Y = value1.Y - value.X * value1.Y * d31 + value1.X * value1.Y * d31 + value.X * value3.Y * d31 - value1.X * value3.Y * d31
				//    + (-value1.Y + value2.X * value1.Y * d31 - value1.X * value1.Y * d31 + value2.Y - value2.X * value3.Y * d31 + value1.X * value3.Y * d31) * u;

				// u 结果及优化
				//u = (value.Y - value1.Y + value.X * value1.Y * d31 - value1.X * value1.Y * d31 - value.X * value3.Y * d31 + value1.X * value3.Y * d31)
				//    / (-value1.Y + value2.X * value1.Y * d31 - value1.X * value1.Y * d31 + value2.Y - value2.X * value3.Y * d31 + value1.X * value3.Y * d31);
				u = (value.Y - value1.Y + (value.X * value1.Y - value1.X * value1.Y - value.X * value3.Y + value1.X * value3.Y) * d31)
					/ (value2.Y - value1.Y + (value2.X * value1.Y - value1.X * value1.Y - value2.X * value3.Y + value1.X * value3.Y) * d31);

				v = (value.X - value1.X - (value2.X - value1.X) * u) * d31;
			}
		}
		public static VECTOR2 RotateTo(VECTOR2 center, VECTOR2 p, float radian)
		{
			VECTOR2 result;
			RotateTo(ref center, ref p, radian, out result);
			return result;
		}
		public static void RotateTo(ref VECTOR2 center, ref VECTOR2 p, float radian, out VECTOR2 result)
		{
			float radius;
			Distance(ref center, ref p, out radius);
			CIRCLE.ParametricEquation(ref center, radius, radian, out result);
		}
		public static VECTOR2 Rotate(ref VECTOR2 center, ref VECTOR2 p, float radian)
		{
			VECTOR2 result;
			Rotate(ref center, ref p, radian, out result);
			return result;
		}
		public static void Rotate(ref VECTOR2 center, ref VECTOR2 p, float radian, out VECTOR2 result)
		{
			float radius;
			Distance(ref center, ref p, out radius);
			float rotate;
			Radian(ref center, ref p, out rotate);
			radian += rotate;
			CIRCLE.ParametricEquation(ref center, radius, radian, out result);
		}
		public static float Radian(VECTOR2 refference, VECTOR2 target)
		{
			float result;
			Radian(ref refference, ref target, out result);
			return result;
		}
		public static void Radian(ref VECTOR2 refference, ref VECTOR2 target, out float radian)
		{
            radian = (float)Math.Atan2(target.Y - refference.Y, target.X - refference.X);
		}
		public static float Degree(VECTOR2 refference, VECTOR2 target)
		{
			float result;
			Degree(ref refference, ref target, out result);
			return result;
		}
		public static void Degree(ref VECTOR2 refference, ref VECTOR2 target, out float degree)
		{
            degree = _MATH.ToDegree((float)Math.Atan2(target.Y - refference.Y, target.X - refference.X));
		}
		public static VECTOR2 Negate(VECTOR2 value)
		{
			VECTOR2 result;
			result.X = -value.X;
			result.Y = -value.Y;
			return result;
		}
		public static void Negate(ref VECTOR2 value, out VECTOR2 result)
		{
			result.X = -value.X;
			result.Y = -value.Y;
		}
		public static VECTOR2 Add(VECTOR2 value1, VECTOR2 value2)
		{
			VECTOR2 result;
			result.X = value1.X + value2.X;
			result.Y = value1.Y + value2.Y;
			return result;
		}
        public static VECTOR2 Add(ref VECTOR2 value1, ref VECTOR2 value2)
        {
            VECTOR2 result;
            result.X = value1.X + value2.X;
            result.Y = value1.Y + value2.Y;
            return result;
        }
		public static void Add(ref VECTOR2 value1, ref VECTOR2 value2, out VECTOR2 result)
		{
			result.X = value1.X + value2.X;
			result.Y = value1.Y + value2.Y;
		}
		public static VECTOR2 Subtract(VECTOR2 value1, VECTOR2 value2)
		{
			VECTOR2 result;
			result.X = value1.X - value2.X;
			result.Y = value1.Y - value2.Y;
			return result;
		}
        public static VECTOR2 Subtract(ref VECTOR2 value1, ref VECTOR2 value2)
        {
            VECTOR2 result;
            result.X = value1.X - value2.X;
            result.Y = value1.Y - value2.Y;
            return result;
        }
		public static void Subtract(ref VECTOR2 value1, ref VECTOR2 value2, out VECTOR2 result)
		{
			result.X = value1.X - value2.X;
			result.Y = value1.Y - value2.Y;
		}
		public static VECTOR2 Multiply(VECTOR2 value1, VECTOR2 value2)
		{
			VECTOR2 result;
			result.X = value1.X * value2.X;
			result.Y = value1.Y * value2.Y;
			return result;
		}
        public static VECTOR2 Multiply(ref VECTOR2 value1, ref VECTOR2 value2)
        {
            VECTOR2 result;
            result.X = value1.X * value2.X;
            result.Y = value1.Y * value2.Y;
            return result;
        }
		public static void Multiply(ref VECTOR2 value1, ref VECTOR2 value2, out VECTOR2 result)
		{
			result.X = value1.X * value2.X;
			result.Y = value1.Y * value2.Y;
		}
		public static VECTOR2 Multiply(VECTOR2 value1, float scaleFactor)
		{
			VECTOR2 result;
			result.X = value1.X * scaleFactor;
			result.Y = value1.Y * scaleFactor;
			return result;
		}
        public static VECTOR2 Multiply(ref VECTOR2 value1, float scaleFactor)
        {
            VECTOR2 result;
            result.X = value1.X * scaleFactor;
            result.Y = value1.Y * scaleFactor;
            return result;
        }
		public static void Multiply(ref VECTOR2 value1, float scaleFactor, out VECTOR2 result)
		{
			result.X = value1.X * scaleFactor;
			result.Y = value1.Y * scaleFactor;
		}
		public static VECTOR2 Divide(VECTOR2 value1, VECTOR2 value2)
		{
			VECTOR2 result;
			result.X = value1.X / value2.X;
			result.Y = value1.Y / value2.Y;
			return result;
		}
        public static VECTOR2 Divide(ref VECTOR2 value1, ref VECTOR2 value2)
        {
            VECTOR2 result;
            result.X = value1.X / value2.X;
            result.Y = value1.Y / value2.Y;
            return result;
        }
		public static void Divide(ref VECTOR2 value1, ref VECTOR2 value2, out VECTOR2 result)
		{
			result.X = value1.X / value2.X;
			result.Y = value1.Y / value2.Y;
		}
		public static VECTOR2 Divide(VECTOR2 value1, float divider)
		{
			float num = 1f / divider;
			VECTOR2 result;
			result.X = value1.X * num;
			result.Y = value1.Y * num;
			return result;
		}
        public static VECTOR2 Divide(ref VECTOR2 value1, float divider)
        {
            float num = 1f / divider;
            VECTOR2 result;
            result.X = value1.X * num;
            result.Y = value1.Y * num;
            return result;
        }
		public static void Divide(ref VECTOR2 value1, float divider, out VECTOR2 result)
		{
			float num = 1f / divider;
			result.X = value1.X * num;
			result.Y = value1.Y * num;
		}
        public static bool operator ==(VECTOR2 value1, VECTOR2 value2)
        {
            return value1.X == value2.X && value1.Y == value2.Y;
        }
        public static bool operator !=(VECTOR2 value1, VECTOR2 value2)
        {
            return value1.X != value2.X || value1.Y != value2.Y;
        }
        public static VECTOR2 operator -(VECTOR2 value)
        {
            VECTOR2 result;
            result.X = -value.X;
            result.Y = -value.Y;
            return result;
        }
        public static VECTOR2 operator +(VECTOR2 value1, VECTOR2 value2)
        {
            VECTOR2 result;
            result.X = value1.X + value2.X;
            result.Y = value1.Y + value2.Y;
            return result;
        }
        public static VECTOR2 operator -(VECTOR2 value1, VECTOR2 value2)
        {
            VECTOR2 result;
            result.X = value1.X - value2.X;
            result.Y = value1.Y - value2.Y;
            return result;
        }
        public static VECTOR2 operator *(VECTOR2 value1, VECTOR2 value2)
        {
            VECTOR2 result;
            result.X = value1.X * value2.X;
            result.Y = value1.Y * value2.Y;
            return result;
        }
        public static VECTOR2 operator *(VECTOR2 value, float scaleFactor)
        {
            VECTOR2 result;
            result.X = value.X * scaleFactor;
            result.Y = value.Y * scaleFactor;
            return result;
        }
        public static VECTOR2 operator *(float scaleFactor, VECTOR2 value)
        {
            VECTOR2 result;
            result.X = value.X * scaleFactor;
            result.Y = value.Y * scaleFactor;
            return result;
        }
        public static VECTOR2 operator /(VECTOR2 value1, VECTOR2 value2)
        {
            VECTOR2 result;
            result.X = value1.X / value2.X;
            result.Y = value1.Y / value2.Y;
            return result;
        }
        public static VECTOR2 operator /(VECTOR2 value1, float divider)
        {
            VECTOR2 result;
            Divide(ref value1, divider, out result);
            return result;
        }
    }
    [Code(ECode.LessUseful)]
    public struct POINT : IEquatable<POINT>
    {
        public int X;
        public int Y;
        private static POINT _zero = new POINT();
        private static POINT _one = new POINT(1, 1);
        private static POINT _unitX = new POINT(1, 0);
        private static POINT _unitY = new POINT(0, 1);
        public static POINT Zero
        {
            get { return _zero; }
        }
        public static POINT One
        {
            get { return _one; }
        }
        public static POINT UnitX
        {
            get { return _unitX; }
        }
        public static POINT UnitY
        {
            get { return _unitY; }
        }
        public POINT(int value)
        {
            this.X = value;
            this.Y = value;
        }
        public POINT(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
        public override string ToString()
        {
            return string.Format("X:{0} Y:{1}", X, Y);
        }
        public override bool Equals(object obj)
        {
            bool result = false;
            if (obj is POINT)
                result = Equals((POINT)obj);
            return result;
        }
        public bool Equals(POINT other)
        {
            return this.X == other.X && this.Y == other.Y;
        }
        public override int GetHashCode()
        {
            return this.X.GetHashCode() + this.Y.GetHashCode();
        }
        public float Length()
        {
            return (float)Math.Sqrt(LengthSquared());
        }
        public int LengthSquared()
        {
            return this.X * this.X + this.Y * this.Y;
        }
        public VECTOR2 ToVector()
        {
            return new VECTOR2(X, Y);
        }
        //public static bool operator ==(POINT value1, POINT value2)
        //{
        //    return value1.X == value2.X && value1.Y == value2.Y;
        //}
        //public static bool operator !=(POINT value1, POINT value2)
        //{
        //    return value1.X != value2.X || value1.Y != value2.Y;
        //}
        //public static POINT operator +(POINT value1, POINT value2)
        //{
        //    value1.X += value2.X;
        //    value1.Y += value2.Y;
        //    return value1;
        //}
        //public static POINT operator -(POINT value1, POINT value2)
        //{
        //    value1.X -= value2.X;
        //    value1.Y -= value2.Y;
        //    return value1;
        //}
        //public static POINT operator *(POINT value1, POINT value2)
        //{
        //    value1.X *= value2.X;
        //    value1.Y *= value2.Y;
        //    return value1;
        //}
        //public static POINT operator *(POINT value1, int value2)
        //{
        //    value1.X *= value2;
        //    value1.Y *= value2;
        //    return value1;
        //}
        //public static POINT operator *(int value2, POINT value1)
        //{
        //    value1.X *= value2;
        //    value1.Y *= value2;
        //    return value1;
        //}
        //public static POINT operator /(POINT value1, POINT value2)
        //{
        //    value1.X /= value2.X;
        //    value1.Y /= value2.Y;
        //    return value1;
        //}
        //public static POINT operator /(POINT value1, int value2)
        //{
        //    value1.X /= value2;
        //    value1.Y /= value2;
        //    return value1;
        //}
    }
    /// <summary>三维向量</summary>
    public struct VECTOR3 : IEquatable<VECTOR3>
    {
        public float X;
        public float Y;
        public float Z;
        private static VECTOR3 _zero = default(VECTOR3);
        private static VECTOR3 _one = new VECTOR3(1f, 1f, 1f);
        private static VECTOR3 _unitX = new VECTOR3(1f, 0f, 0f);
        private static VECTOR3 _unitY = new VECTOR3(0f, 1f, 0f);
        private static VECTOR3 _unitZ = new VECTOR3(0f, 0f, 1f);
        private static VECTOR3 _up = new VECTOR3(0f, 1f, 0f);
        private static VECTOR3 _down = new VECTOR3(0f, -1f, 0f);
        private static VECTOR3 _right = new VECTOR3(1f, 0f, 0f);
        private static VECTOR3 _left = new VECTOR3(-1f, 0f, 0f);
        private static VECTOR3 _forward = new VECTOR3(0f, 0f, -1f);
        private static VECTOR3 _backward = new VECTOR3(0f, 0f, 1f);
        public static VECTOR3 Zero
        {
            get
            {
                return VECTOR3._zero;
            }
        }
        public static VECTOR3 One
        {
            get
            {
                return VECTOR3._one;
            }
        }
        public static VECTOR3 UnitX
        {
            get
            {
                return VECTOR3._unitX;
            }
        }
        public static VECTOR3 UnitY
        {
            get
            {
                return VECTOR3._unitY;
            }
        }
        public static VECTOR3 UnitZ
        {
            get
            {
                return VECTOR3._unitZ;
            }
        }
        public static VECTOR3 Up
        {
            get
            {
                return VECTOR3._up;
            }
        }
        public static VECTOR3 Down
        {
            get
            {
                return VECTOR3._down;
            }
        }
        public static VECTOR3 Right
        {
            get
            {
                return VECTOR3._right;
            }
        }
        public static VECTOR3 Left
        {
            get
            {
                return VECTOR3._left;
            }
        }
        public static VECTOR3 Forward
        {
            get
            {
                return VECTOR3._forward;
            }
        }
        public static VECTOR3 Backward
        {
            get
            {
                return VECTOR3._backward;
            }
        }
		public VECTOR2 XY
		{
			get { return new VECTOR2(X, Y); }
			set
			{
				X = value.X;
				Y = value.Y;
			}
		}
        public VECTOR3(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
        public VECTOR3(float value)
        {
            this.Z = value;
            this.Y = value;
            this.X = value;
        }
        public VECTOR3(VECTOR2 value, float z)
        {
            this.X = value.X;
            this.Y = value.Y;
            this.Z = z;
        }
        public override string ToString()
        {
            return string.Format("{{X:{0} Y:{1} Z:{2}}}", X, Y, Z);
        }
        public bool Equals(VECTOR3 other)
        {
            return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
        }
        public override bool Equals(object obj)
        {
            bool result = false;
            if (obj is VECTOR3)
            {
                result = this.Equals((VECTOR3)obj);
            }
            return result;
        }
        public override int GetHashCode()
        {
            return this.X.GetHashCode() + this.Y.GetHashCode() + this.Z.GetHashCode();
        }
        public float Length()
        {
            float num = this.X * this.X + this.Y * this.Y + this.Z * this.Z;
            return (float)Math.Sqrt((double)num);
        }
        public float LengthSquared()
        {
            return this.X * this.X + this.Y * this.Y + this.Z * this.Z;
        }
        public static float Distance(VECTOR3 value1, VECTOR3 value2)
        {
            float num = value1.X - value2.X;
            float num2 = value1.Y - value2.Y;
            float num3 = value1.Z - value2.Z;
            float num4 = num * num + num2 * num2 + num3 * num3;
            return (float)Math.Sqrt((double)num4);
        }
        public static void Distance(ref VECTOR3 value1, ref VECTOR3 value2, out float result)
        {
            float num = value1.X - value2.X;
            float num2 = value1.Y - value2.Y;
            float num3 = value1.Z - value2.Z;
            float num4 = num * num + num2 * num2 + num3 * num3;
            result = (float)Math.Sqrt((double)num4);
        }
        public static float DistanceSquared(VECTOR3 value1, VECTOR3 value2)
        {
            float num = value1.X - value2.X;
            float num2 = value1.Y - value2.Y;
            float num3 = value1.Z - value2.Z;
            return num * num + num2 * num2 + num3 * num3;
        }
        public static void DistanceSquared(ref VECTOR3 value1, ref VECTOR3 value2, out float result)
        {
            float num = value1.X - value2.X;
            float num2 = value1.Y - value2.Y;
            float num3 = value1.Z - value2.Z;
            result = num * num + num2 * num2 + num3 * num3;
        }
        public static float Dot(VECTOR3 vector1, VECTOR3 vector2)
        {
            return vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z;
        }
        public static void Dot(ref VECTOR3 vector1, ref VECTOR3 vector2, out float result)
        {
            result = vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z;
        }
        public void Normalize()
        {
            float num = this.X * this.X + this.Y * this.Y + this.Z * this.Z;
            float num2 = 1f / (float)Math.Sqrt((double)num);
            this.X *= num2;
            this.Y *= num2;
            this.Z *= num2;
        }
        public static VECTOR3 Normalize(VECTOR3 value)
        {
            float num = value.X * value.X + value.Y * value.Y + value.Z * value.Z;
            float num2 = 1f / (float)Math.Sqrt((double)num);
            VECTOR3 result;
            result.X = value.X * num2;
            result.Y = value.Y * num2;
            result.Z = value.Z * num2;
            return result;
        }
        public static void Normalize(ref VECTOR3 value, out VECTOR3 result)
        {
            float num = value.X * value.X + value.Y * value.Y + value.Z * value.Z;
            float num2 = 1f / (float)Math.Sqrt((double)num);
            result.X = value.X * num2;
            result.Y = value.Y * num2;
            result.Z = value.Z * num2;
        }
        public static VECTOR3 Cross(VECTOR3 vector1, VECTOR3 vector2)
        {
            VECTOR3 result;
            result.X = vector1.Y * vector2.Z - vector1.Z * vector2.Y;
            result.Y = vector1.Z * vector2.X - vector1.X * vector2.Z;
            result.Z = vector1.X * vector2.Y - vector1.Y * vector2.X;
            return result;
        }
        public static void Cross(ref VECTOR3 vector1, ref VECTOR3 vector2, out VECTOR3 result)
        {
            float x = vector1.Y * vector2.Z - vector1.Z * vector2.Y;
            float y = vector1.Z * vector2.X - vector1.X * vector2.Z;
            float z = vector1.X * vector2.Y - vector1.Y * vector2.X;
            result.X = x;
            result.Y = y;
            result.Z = z;
        }
        public static VECTOR3 Reflect(VECTOR3 vector, VECTOR3 normal)
        {
            float num = vector.X * normal.X + vector.Y * normal.Y + vector.Z * normal.Z;
            VECTOR3 result;
            result.X = vector.X - 2f * num * normal.X;
            result.Y = vector.Y - 2f * num * normal.Y;
            result.Z = vector.Z - 2f * num * normal.Z;
            return result;
        }
        public static void Reflect(ref VECTOR3 vector, ref VECTOR3 normal, out VECTOR3 result)
        {
            float num = vector.X * normal.X + vector.Y * normal.Y + vector.Z * normal.Z;
            result.X = vector.X - 2f * num * normal.X;
            result.Y = vector.Y - 2f * num * normal.Y;
            result.Z = vector.Z - 2f * num * normal.Z;
        }
        public static VECTOR3 Min(VECTOR3 value1, VECTOR3 value2)
        {
            VECTOR3 result;
            result.X = ((value1.X < value2.X) ? value1.X : value2.X);
            result.Y = ((value1.Y < value2.Y) ? value1.Y : value2.Y);
            result.Z = ((value1.Z < value2.Z) ? value1.Z : value2.Z);
            return result;
        }
        public static void Min(ref VECTOR3 value1, ref VECTOR3 value2, out VECTOR3 result)
        {
            result.X = ((value1.X < value2.X) ? value1.X : value2.X);
            result.Y = ((value1.Y < value2.Y) ? value1.Y : value2.Y);
            result.Z = ((value1.Z < value2.Z) ? value1.Z : value2.Z);
        }
        public static VECTOR3 Max(VECTOR3 value1, VECTOR3 value2)
        {
            VECTOR3 result;
            result.X = ((value1.X > value2.X) ? value1.X : value2.X);
            result.Y = ((value1.Y > value2.Y) ? value1.Y : value2.Y);
            result.Z = ((value1.Z > value2.Z) ? value1.Z : value2.Z);
            return result;
        }
        public static void Max(ref VECTOR3 value1, ref VECTOR3 value2, out VECTOR3 result)
        {
            result.X = ((value1.X > value2.X) ? value1.X : value2.X);
            result.Y = ((value1.Y > value2.Y) ? value1.Y : value2.Y);
            result.Z = ((value1.Z > value2.Z) ? value1.Z : value2.Z);
        }
        public static VECTOR3 Clamp(VECTOR3 value1, VECTOR3 min, VECTOR3 max)
        {
            float num = value1.X;
            num = ((num > max.X) ? max.X : num);
            num = ((num < min.X) ? min.X : num);
            float num2 = value1.Y;
            num2 = ((num2 > max.Y) ? max.Y : num2);
            num2 = ((num2 < min.Y) ? min.Y : num2);
            float num3 = value1.Z;
            num3 = ((num3 > max.Z) ? max.Z : num3);
            num3 = ((num3 < min.Z) ? min.Z : num3);
            VECTOR3 result;
            result.X = num;
            result.Y = num2;
            result.Z = num3;
            return result;
        }
        public static void Clamp(ref VECTOR3 value1, ref VECTOR3 min, ref VECTOR3 max, out VECTOR3 result)
        {
            float num = value1.X;
            num = ((num > max.X) ? max.X : num);
            num = ((num < min.X) ? min.X : num);
            float num2 = value1.Y;
            num2 = ((num2 > max.Y) ? max.Y : num2);
            num2 = ((num2 < min.Y) ? min.Y : num2);
            float num3 = value1.Z;
            num3 = ((num3 > max.Z) ? max.Z : num3);
            num3 = ((num3 < min.Z) ? min.Z : num3);
            result.X = num;
            result.Y = num2;
            result.Z = num3;
        }
        public static VECTOR3 Lerp(VECTOR3 value1, VECTOR3 value2, float amount)
        {
            VECTOR3 result;
            result.X = value1.X + (value2.X - value1.X) * amount;
            result.Y = value1.Y + (value2.Y - value1.Y) * amount;
            result.Z = value1.Z + (value2.Z - value1.Z) * amount;
            return result;
        }
        public static void Lerp(ref VECTOR3 value1, ref VECTOR3 value2, float amount, out VECTOR3 result)
        {
            result.X = value1.X + (value2.X - value1.X) * amount;
            result.Y = value1.Y + (value2.Y - value1.Y) * amount;
            result.Z = value1.Z + (value2.Z - value1.Z) * amount;
        }
        public static VECTOR3 Barycentric(VECTOR3 value1, VECTOR3 value2, VECTOR3 value3, float amount1, float amount2)
        {
            VECTOR3 result;
            result.X = value1.X + amount1 * (value2.X - value1.X) + amount2 * (value3.X - value1.X);
            result.Y = value1.Y + amount1 * (value2.Y - value1.Y) + amount2 * (value3.Y - value1.Y);
            result.Z = value1.Z + amount1 * (value2.Z - value1.Z) + amount2 * (value3.Z - value1.Z);
            return result;
        }
        public static void Barycentric(ref VECTOR3 value1, ref VECTOR3 value2, ref VECTOR3 value3, float amount1, float amount2, out VECTOR3 result)
        {
            result.X = value1.X + amount1 * (value2.X - value1.X) + amount2 * (value3.X - value1.X);
            result.Y = value1.Y + amount1 * (value2.Y - value1.Y) + amount2 * (value3.Y - value1.Y);
            result.Z = value1.Z + amount1 * (value2.Z - value1.Z) + amount2 * (value3.Z - value1.Z);
        }
        public static VECTOR3 SmoothStep(VECTOR3 value1, VECTOR3 value2, float amount)
        {
            amount = ((amount > 1f) ? 1f : ((amount < 0f) ? 0f : amount));
            amount = amount * amount * (3f - 2f * amount);
            VECTOR3 result;
            result.X = value1.X + (value2.X - value1.X) * amount;
            result.Y = value1.Y + (value2.Y - value1.Y) * amount;
            result.Z = value1.Z + (value2.Z - value1.Z) * amount;
            return result;
        }
        public static void SmoothStep(ref VECTOR3 value1, ref VECTOR3 value2, float amount, out VECTOR3 result)
        {
            amount = ((amount > 1f) ? 1f : ((amount < 0f) ? 0f : amount));
            amount = amount * amount * (3f - 2f * amount);
            result.X = value1.X + (value2.X - value1.X) * amount;
            result.Y = value1.Y + (value2.Y - value1.Y) * amount;
            result.Z = value1.Z + (value2.Z - value1.Z) * amount;
        }
        public static VECTOR3 CatmullRom(VECTOR3 value1, VECTOR3 value2, VECTOR3 value3, VECTOR3 value4, float amount)
        {
            float num = amount * amount;
            float num2 = amount * num;
            VECTOR3 result;
            result.X = 0.5f * (2f * value2.X + (-value1.X + value3.X) * amount + (2f * value1.X - 5f * value2.X + 4f * value3.X - value4.X) * num + (-value1.X + 3f * value2.X - 3f * value3.X + value4.X) * num2);
            result.Y = 0.5f * (2f * value2.Y + (-value1.Y + value3.Y) * amount + (2f * value1.Y - 5f * value2.Y + 4f * value3.Y - value4.Y) * num + (-value1.Y + 3f * value2.Y - 3f * value3.Y + value4.Y) * num2);
            result.Z = 0.5f * (2f * value2.Z + (-value1.Z + value3.Z) * amount + (2f * value1.Z - 5f * value2.Z + 4f * value3.Z - value4.Z) * num + (-value1.Z + 3f * value2.Z - 3f * value3.Z + value4.Z) * num2);
            return result;
        }
        public static void CatmullRom(ref VECTOR3 value1, ref VECTOR3 value2, ref VECTOR3 value3, ref VECTOR3 value4, float amount, out VECTOR3 result)
        {
            float num = amount * amount;
            float num2 = amount * num;
            result.X = 0.5f * (2f * value2.X + (-value1.X + value3.X) * amount + (2f * value1.X - 5f * value2.X + 4f * value3.X - value4.X) * num + (-value1.X + 3f * value2.X - 3f * value3.X + value4.X) * num2);
            result.Y = 0.5f * (2f * value2.Y + (-value1.Y + value3.Y) * amount + (2f * value1.Y - 5f * value2.Y + 4f * value3.Y - value4.Y) * num + (-value1.Y + 3f * value2.Y - 3f * value3.Y + value4.Y) * num2);
            result.Z = 0.5f * (2f * value2.Z + (-value1.Z + value3.Z) * amount + (2f * value1.Z - 5f * value2.Z + 4f * value3.Z - value4.Z) * num + (-value1.Z + 3f * value2.Z - 3f * value3.Z + value4.Z) * num2);
        }
        public static VECTOR3 Hermite(VECTOR3 value1, VECTOR3 tangent1, VECTOR3 value2, VECTOR3 tangent2, float amount)
        {
            float num = amount * amount;
            float num2 = amount * num;
            float num3 = 2f * num2 - 3f * num + 1f;
            float num4 = -2f * num2 + 3f * num;
            float num5 = num2 - 2f * num + amount;
            float num6 = num2 - num;
            VECTOR3 result;
            result.X = value1.X * num3 + value2.X * num4 + tangent1.X * num5 + tangent2.X * num6;
            result.Y = value1.Y * num3 + value2.Y * num4 + tangent1.Y * num5 + tangent2.Y * num6;
            result.Z = value1.Z * num3 + value2.Z * num4 + tangent1.Z * num5 + tangent2.Z * num6;
            return result;
        }
        public static void Hermite(ref VECTOR3 value1, ref VECTOR3 tangent1, ref VECTOR3 value2, ref VECTOR3 tangent2, float amount, out VECTOR3 result)
        {
            float num = amount * amount;
            float num2 = amount * num;
            float num3 = 2f * num2 - 3f * num + 1f;
            float num4 = -2f * num2 + 3f * num;
            float num5 = num2 - 2f * num + amount;
            float num6 = num2 - num;
            result.X = value1.X * num3 + value2.X * num4 + tangent1.X * num5 + tangent2.X * num6;
            result.Y = value1.Y * num3 + value2.Y * num4 + tangent1.Y * num5 + tangent2.Y * num6;
            result.Z = value1.Z * num3 + value2.Z * num4 + tangent1.Z * num5 + tangent2.Z * num6;
        }
        public static VECTOR3 Transform(VECTOR3 position, MATRIX matrix)
        {
            float x = position.X * matrix.M11 + position.Y * matrix.M21 + position.Z * matrix.M31 + matrix.M41;
            float y = position.X * matrix.M12 + position.Y * matrix.M22 + position.Z * matrix.M32 + matrix.M42;
            float z = position.X * matrix.M13 + position.Y * matrix.M23 + position.Z * matrix.M33 + matrix.M43;
            VECTOR3 result;
            result.X = x;
            result.Y = y;
            result.Z = z;
            return result;
        }
        public static void Transform(ref VECTOR3 position, ref MATRIX matrix, out VECTOR3 result)
        {
            float x = position.X * matrix.M11 + position.Y * matrix.M21 + position.Z * matrix.M31 + matrix.M41;
            float y = position.X * matrix.M12 + position.Y * matrix.M22 + position.Z * matrix.M32 + matrix.M42;
            float z = position.X * matrix.M13 + position.Y * matrix.M23 + position.Z * matrix.M33 + matrix.M43;
            result.X = x;
            result.Y = y;
            result.Z = z;
        }
        public static VECTOR3 TransformNormal(VECTOR3 normal, MATRIX matrix)
        {
            float x = normal.X * matrix.M11 + normal.Y * matrix.M21 + normal.Z * matrix.M31;
            float y = normal.X * matrix.M12 + normal.Y * matrix.M22 + normal.Z * matrix.M32;
            float z = normal.X * matrix.M13 + normal.Y * matrix.M23 + normal.Z * matrix.M33;
            VECTOR3 result;
            result.X = x;
            result.Y = y;
            result.Z = z;
            return result;
        }
        public static void TransformNormal(ref VECTOR3 normal, ref MATRIX matrix, out VECTOR3 result)
        {
            float x = normal.X * matrix.M11 + normal.Y * matrix.M21 + normal.Z * matrix.M31;
            float y = normal.X * matrix.M12 + normal.Y * matrix.M22 + normal.Z * matrix.M32;
            float z = normal.X * matrix.M13 + normal.Y * matrix.M23 + normal.Z * matrix.M33;
            result.X = x;
            result.Y = y;
            result.Z = z;
        }
        public static VECTOR3 Negate(VECTOR3 value)
        {
            VECTOR3 result;
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
            return result;
        }
        public static void Negate(ref VECTOR3 value, out VECTOR3 result)
        {
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
        }
        public static VECTOR3 Add(VECTOR3 value1, VECTOR3 value2)
        {
            VECTOR3 result;
            result.X = value1.X + value2.X;
            result.Y = value1.Y + value2.Y;
            result.Z = value1.Z + value2.Z;
            return result;
        }
        public static void Add(ref VECTOR3 value1, ref VECTOR3 value2, out VECTOR3 result)
        {
            result.X = value1.X + value2.X;
            result.Y = value1.Y + value2.Y;
            result.Z = value1.Z + value2.Z;
        }
        public static VECTOR3 Subtract(VECTOR3 value1, VECTOR3 value2)
        {
            VECTOR3 result;
            result.X = value1.X - value2.X;
            result.Y = value1.Y - value2.Y;
            result.Z = value1.Z - value2.Z;
            return result;
        }
        public static void Subtract(ref VECTOR3 value1, ref VECTOR3 value2, out VECTOR3 result)
        {
            result.X = value1.X - value2.X;
            result.Y = value1.Y - value2.Y;
            result.Z = value1.Z - value2.Z;
        }
        public static VECTOR3 Multiply(VECTOR3 value1, VECTOR3 value2)
        {
            VECTOR3 result;
            result.X = value1.X * value2.X;
            result.Y = value1.Y * value2.Y;
            result.Z = value1.Z * value2.Z;
            return result;
        }
        public static void Multiply(ref VECTOR3 value1, ref VECTOR3 value2, out VECTOR3 result)
        {
            result.X = value1.X * value2.X;
            result.Y = value1.Y * value2.Y;
            result.Z = value1.Z * value2.Z;
        }
        public static VECTOR3 Multiply(VECTOR3 value1, float scaleFactor)
        {
            VECTOR3 result;
            result.X = value1.X * scaleFactor;
            result.Y = value1.Y * scaleFactor;
            result.Z = value1.Z * scaleFactor;
            return result;
        }
        public static void Multiply(ref VECTOR3 value1, float scaleFactor, out VECTOR3 result)
        {
            result.X = value1.X * scaleFactor;
            result.Y = value1.Y * scaleFactor;
            result.Z = value1.Z * scaleFactor;
        }
        public static VECTOR3 Divide(VECTOR3 value1, VECTOR3 value2)
        {
            VECTOR3 result;
            result.X = value1.X / value2.X;
            result.Y = value1.Y / value2.Y;
            result.Z = value1.Z / value2.Z;
            return result;
        }
        public static void Divide(ref VECTOR3 value1, ref VECTOR3 value2, out VECTOR3 result)
        {
            result.X = value1.X / value2.X;
            result.Y = value1.Y / value2.Y;
            result.Z = value1.Z / value2.Z;
        }
        public static VECTOR3 Divide(VECTOR3 value1, float value2)
        {
            float num = 1f / value2;
            VECTOR3 result;
            result.X = value1.X * num;
            result.Y = value1.Y * num;
            result.Z = value1.Z * num;
            return result;
        }
        public static void Divide(ref VECTOR3 value1, float value2, out VECTOR3 result)
        {
            float num = 1f / value2;
            result.X = value1.X * num;
            result.Y = value1.Y * num;
            result.Z = value1.Z * num;
        }
        public static VECTOR3 operator -(VECTOR3 value)
        {
            VECTOR3 result;
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
            return result;
        }
        public static bool operator ==(VECTOR3 value1, VECTOR3 value2)
        {
            return value1.X == value2.X && value1.Y == value2.Y && value1.Z == value2.Z;
        }
        public static bool operator !=(VECTOR3 value1, VECTOR3 value2)
        {
            return !(value1 == value2);
        }
        public static VECTOR3 operator +(VECTOR3 value1, VECTOR3 value2)
        {
            VECTOR3 result;
            result.X = value1.X + value2.X;
            result.Y = value1.Y + value2.Y;
            result.Z = value1.Z + value2.Z;
            return result;
        }
        public static VECTOR3 operator -(VECTOR3 value1, VECTOR3 value2)
        {
            VECTOR3 result;
            result.X = value1.X - value2.X;
            result.Y = value1.Y - value2.Y;
            result.Z = value1.Z - value2.Z;
            return result;
        }
        public static VECTOR3 operator *(VECTOR3 value1, VECTOR3 value2)
        {
            VECTOR3 result;
            result.X = value1.X * value2.X;
            result.Y = value1.Y * value2.Y;
            result.Z = value1.Z * value2.Z;
            return result;
        }
        public static VECTOR3 operator *(VECTOR3 value, float scaleFactor)
        {
            VECTOR3 result;
            result.X = value.X * scaleFactor;
            result.Y = value.Y * scaleFactor;
            result.Z = value.Z * scaleFactor;
            return result;
        }
        public static VECTOR3 operator *(float scaleFactor, VECTOR3 value)
        {
            VECTOR3 result;
            result.X = value.X * scaleFactor;
            result.Y = value.Y * scaleFactor;
            result.Z = value.Z * scaleFactor;
            return result;
        }
        public static VECTOR3 operator /(VECTOR3 value1, VECTOR3 value2)
        {
            VECTOR3 result;
            result.X = value1.X / value2.X;
            result.Y = value1.Y / value2.Y;
            result.Z = value1.Z / value2.Z;
            return result;
        }
        public static VECTOR3 operator /(VECTOR3 value, float divider)
        {
            float num = 1f / divider;
            VECTOR3 result;
            result.X = value.X * num;
            result.Y = value.Y * num;
            result.Z = value.Z * num;
            return result;
        }
    }
    public struct VECTOR4 : IEquatable<VECTOR4>
    {
        public float X;
        public float Y;
        public float Z;
        public float W;
        private static VECTOR4 _zero = default(VECTOR4);
        private static VECTOR4 _one = new VECTOR4(1f, 1f, 1f, 1f);
        private static VECTOR4 _unitX = new VECTOR4(1f, 0f, 0f, 0f);
        private static VECTOR4 _unitY = new VECTOR4(0f, 1f, 0f, 0f);
        private static VECTOR4 _unitZ = new VECTOR4(0f, 0f, 1f, 0f);
        private static VECTOR4 _unitW = new VECTOR4(0f, 0f, 0f, 1f);
        public static VECTOR4 Zero
        {
            get
            {
                return VECTOR4._zero;
            }
        }
        public static VECTOR4 One
        {
            get
            {
                return VECTOR4._one;
            }
        }
        public static VECTOR4 UnitX
        {
            get
            {
                return VECTOR4._unitX;
            }
        }
        public static VECTOR4 UnitY
        {
            get
            {
                return VECTOR4._unitY;
            }
        }
        public static VECTOR4 UnitZ
        {
            get
            {
                return VECTOR4._unitZ;
            }
        }
        public static VECTOR4 UnitW
        {
            get
            {
                return VECTOR4._unitW;
            }
        }
        public VECTOR4(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }
        public VECTOR4(VECTOR2 value, float z, float w)
        {
            this.X = value.X;
            this.Y = value.Y;
            this.Z = z;
            this.W = w;
        }
        public VECTOR4(VECTOR3 value, float w)
        {
            this.X = value.X;
            this.Y = value.Y;
            this.Z = value.Z;
            this.W = w;
        }
        public VECTOR4(float value)
        {
            this.W = value;
            this.Z = value;
            this.Y = value;
            this.X = value;
        }
        public override string ToString()
        {
            return string.Format("X:{0} Y:{1} Z:{2} W:{3}", new object[]
			{
				this.X.ToString(),
				this.Y.ToString(),
				this.Z.ToString(),
				this.W.ToString()
			});
        }
        public bool Equals(VECTOR4 other)
        {
            return this.X == other.X && this.Y == other.Y && this.Z == other.Z && this.W == other.W;
        }
        public override bool Equals(object obj)
        {
            bool result = false;
            if (obj is VECTOR4)
            {
                result = this.Equals((VECTOR4)obj);
            }
            return result;
        }
        public override int GetHashCode()
        {
            return this.X.GetHashCode() + this.Y.GetHashCode() + this.Z.GetHashCode() + this.W.GetHashCode();
        }
        public float Length()
        {
            float num = this.X * this.X + this.Y * this.Y + this.Z * this.Z + this.W * this.W;
            return (float)Math.Sqrt((double)num);
        }
        public float LengthSquared()
        {
            return this.X * this.X + this.Y * this.Y + this.Z * this.Z + this.W * this.W;
        }
        public static float Distance(VECTOR4 value1, VECTOR4 value2)
        {
            float num = value1.X - value2.X;
            float num2 = value1.Y - value2.Y;
            float num3 = value1.Z - value2.Z;
            float num4 = value1.W - value2.W;
            float num5 = num * num + num2 * num2 + num3 * num3 + num4 * num4;
            return (float)Math.Sqrt((double)num5);
        }
        public static void Distance(ref VECTOR4 value1, ref VECTOR4 value2, out float result)
        {
            float num = value1.X - value2.X;
            float num2 = value1.Y - value2.Y;
            float num3 = value1.Z - value2.Z;
            float num4 = value1.W - value2.W;
            float num5 = num * num + num2 * num2 + num3 * num3 + num4 * num4;
            result = (float)Math.Sqrt((double)num5);
        }
        public static float DistanceSquared(VECTOR4 value1, VECTOR4 value2)
        {
            float num = value1.X - value2.X;
            float num2 = value1.Y - value2.Y;
            float num3 = value1.Z - value2.Z;
            float num4 = value1.W - value2.W;
            return num * num + num2 * num2 + num3 * num3 + num4 * num4;
        }
        public static void DistanceSquared(ref VECTOR4 value1, ref VECTOR4 value2, out float result)
        {
            float num = value1.X - value2.X;
            float num2 = value1.Y - value2.Y;
            float num3 = value1.Z - value2.Z;
            float num4 = value1.W - value2.W;
            result = num * num + num2 * num2 + num3 * num3 + num4 * num4;
        }
        public static float Dot(VECTOR4 vector1, VECTOR4 vector2)
        {
            return vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z + vector1.W * vector2.W;
        }
        public static void Dot(ref VECTOR4 vector1, ref VECTOR4 vector2, out float result)
        {
            result = vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z + vector1.W * vector2.W;
        }
        public void Normalize()
        {
            float num = this.X * this.X + this.Y * this.Y + this.Z * this.Z + this.W * this.W;
            float num2 = 1f / (float)Math.Sqrt((double)num);
            this.X *= num2;
            this.Y *= num2;
            this.Z *= num2;
            this.W *= num2;
        }
        public static VECTOR4 Normalize(VECTOR4 vector)
        {
            float num = vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z + vector.W * vector.W;
            float num2 = 1f / (float)Math.Sqrt((double)num);
            VECTOR4 result;
            result.X = vector.X * num2;
            result.Y = vector.Y * num2;
            result.Z = vector.Z * num2;
            result.W = vector.W * num2;
            return result;
        }
        public static void Normalize(ref VECTOR4 vector, out VECTOR4 result)
        {
            float num = vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z + vector.W * vector.W;
            float num2 = 1f / (float)Math.Sqrt((double)num);
            result.X = vector.X * num2;
            result.Y = vector.Y * num2;
            result.Z = vector.Z * num2;
            result.W = vector.W * num2;
        }
        public static VECTOR4 Min(VECTOR4 value1, VECTOR4 value2)
        {
            VECTOR4 result;
            result.X = ((value1.X < value2.X) ? value1.X : value2.X);
            result.Y = ((value1.Y < value2.Y) ? value1.Y : value2.Y);
            result.Z = ((value1.Z < value2.Z) ? value1.Z : value2.Z);
            result.W = ((value1.W < value2.W) ? value1.W : value2.W);
            return result;
        }
        public static void Min(ref VECTOR4 value1, ref VECTOR4 value2, out VECTOR4 result)
        {
            result.X = ((value1.X < value2.X) ? value1.X : value2.X);
            result.Y = ((value1.Y < value2.Y) ? value1.Y : value2.Y);
            result.Z = ((value1.Z < value2.Z) ? value1.Z : value2.Z);
            result.W = ((value1.W < value2.W) ? value1.W : value2.W);
        }
        public static VECTOR4 Max(VECTOR4 value1, VECTOR4 value2)
        {
            VECTOR4 result;
            result.X = ((value1.X > value2.X) ? value1.X : value2.X);
            result.Y = ((value1.Y > value2.Y) ? value1.Y : value2.Y);
            result.Z = ((value1.Z > value2.Z) ? value1.Z : value2.Z);
            result.W = ((value1.W > value2.W) ? value1.W : value2.W);
            return result;
        }
        public static void Max(ref VECTOR4 value1, ref VECTOR4 value2, out VECTOR4 result)
        {
            result.X = ((value1.X > value2.X) ? value1.X : value2.X);
            result.Y = ((value1.Y > value2.Y) ? value1.Y : value2.Y);
            result.Z = ((value1.Z > value2.Z) ? value1.Z : value2.Z);
            result.W = ((value1.W > value2.W) ? value1.W : value2.W);
        }
        public static VECTOR4 Clamp(VECTOR4 value1, VECTOR4 min, VECTOR4 max)
        {
            float num = value1.X;
            num = ((num > max.X) ? max.X : num);
            num = ((num < min.X) ? min.X : num);
            float num2 = value1.Y;
            num2 = ((num2 > max.Y) ? max.Y : num2);
            num2 = ((num2 < min.Y) ? min.Y : num2);
            float num3 = value1.Z;
            num3 = ((num3 > max.Z) ? max.Z : num3);
            num3 = ((num3 < min.Z) ? min.Z : num3);
            float num4 = value1.W;
            num4 = ((num4 > max.W) ? max.W : num4);
            num4 = ((num4 < min.W) ? min.W : num4);
            VECTOR4 result;
            result.X = num;
            result.Y = num2;
            result.Z = num3;
            result.W = num4;
            return result;
        }
        public static void Clamp(ref VECTOR4 value1, ref VECTOR4 min, ref VECTOR4 max, out VECTOR4 result)
        {
            float num = value1.X;
            num = ((num > max.X) ? max.X : num);
            num = ((num < min.X) ? min.X : num);
            float num2 = value1.Y;
            num2 = ((num2 > max.Y) ? max.Y : num2);
            num2 = ((num2 < min.Y) ? min.Y : num2);
            float num3 = value1.Z;
            num3 = ((num3 > max.Z) ? max.Z : num3);
            num3 = ((num3 < min.Z) ? min.Z : num3);
            float num4 = value1.W;
            num4 = ((num4 > max.W) ? max.W : num4);
            num4 = ((num4 < min.W) ? min.W : num4);
            result.X = num;
            result.Y = num2;
            result.Z = num3;
            result.W = num4;
        }
        public static VECTOR4 Lerp(VECTOR4 value1, VECTOR4 value2, float amount)
        {
            VECTOR4 result;
            result.X = value1.X + (value2.X - value1.X) * amount;
            result.Y = value1.Y + (value2.Y - value1.Y) * amount;
            result.Z = value1.Z + (value2.Z - value1.Z) * amount;
            result.W = value1.W + (value2.W - value1.W) * amount;
            return result;
        }
        public static void Lerp(ref VECTOR4 value1, ref VECTOR4 value2, float amount, out VECTOR4 result)
        {
            result.X = value1.X + (value2.X - value1.X) * amount;
            result.Y = value1.Y + (value2.Y - value1.Y) * amount;
            result.Z = value1.Z + (value2.Z - value1.Z) * amount;
            result.W = value1.W + (value2.W - value1.W) * amount;
        }
        public static VECTOR4 Barycentric(VECTOR4 value1, VECTOR4 value2, VECTOR4 value3, float amount1, float amount2)
        {
            VECTOR4 result;
            result.X = value1.X + amount1 * (value2.X - value1.X) + amount2 * (value3.X - value1.X);
            result.Y = value1.Y + amount1 * (value2.Y - value1.Y) + amount2 * (value3.Y - value1.Y);
            result.Z = value1.Z + amount1 * (value2.Z - value1.Z) + amount2 * (value3.Z - value1.Z);
            result.W = value1.W + amount1 * (value2.W - value1.W) + amount2 * (value3.W - value1.W);
            return result;
        }
        public static void Barycentric(ref VECTOR4 value1, ref VECTOR4 value2, ref VECTOR4 value3, float amount1, float amount2, out VECTOR4 result)
        {
            result.X = value1.X + amount1 * (value2.X - value1.X) + amount2 * (value3.X - value1.X);
            result.Y = value1.Y + amount1 * (value2.Y - value1.Y) + amount2 * (value3.Y - value1.Y);
            result.Z = value1.Z + amount1 * (value2.Z - value1.Z) + amount2 * (value3.Z - value1.Z);
            result.W = value1.W + amount1 * (value2.W - value1.W) + amount2 * (value3.W - value1.W);
        }
        public static VECTOR4 SmoothStep(VECTOR4 value1, VECTOR4 value2, float amount)
        {
            amount = ((amount > 1f) ? 1f : ((amount < 0f) ? 0f : amount));
            amount = amount * amount * (3f - 2f * amount);
            VECTOR4 result;
            result.X = value1.X + (value2.X - value1.X) * amount;
            result.Y = value1.Y + (value2.Y - value1.Y) * amount;
            result.Z = value1.Z + (value2.Z - value1.Z) * amount;
            result.W = value1.W + (value2.W - value1.W) * amount;
            return result;
        }
        public static void SmoothStep(ref VECTOR4 value1, ref VECTOR4 value2, float amount, out VECTOR4 result)
        {
            amount = ((amount > 1f) ? 1f : ((amount < 0f) ? 0f : amount));
            amount = amount * amount * (3f - 2f * amount);
            result.X = value1.X + (value2.X - value1.X) * amount;
            result.Y = value1.Y + (value2.Y - value1.Y) * amount;
            result.Z = value1.Z + (value2.Z - value1.Z) * amount;
            result.W = value1.W + (value2.W - value1.W) * amount;
        }
        public static VECTOR4 CatmullRom(VECTOR4 value1, VECTOR4 value2, VECTOR4 value3, VECTOR4 value4, float amount)
        {
            float num = amount * amount;
            float num2 = amount * num;
            VECTOR4 result;
            result.X = 0.5f * (2f * value2.X + (-value1.X + value3.X) * amount + (2f * value1.X - 5f * value2.X + 4f * value3.X - value4.X) * num + (-value1.X + 3f * value2.X - 3f * value3.X + value4.X) * num2);
            result.Y = 0.5f * (2f * value2.Y + (-value1.Y + value3.Y) * amount + (2f * value1.Y - 5f * value2.Y + 4f * value3.Y - value4.Y) * num + (-value1.Y + 3f * value2.Y - 3f * value3.Y + value4.Y) * num2);
            result.Z = 0.5f * (2f * value2.Z + (-value1.Z + value3.Z) * amount + (2f * value1.Z - 5f * value2.Z + 4f * value3.Z - value4.Z) * num + (-value1.Z + 3f * value2.Z - 3f * value3.Z + value4.Z) * num2);
            result.W = 0.5f * (2f * value2.W + (-value1.W + value3.W) * amount + (2f * value1.W - 5f * value2.W + 4f * value3.W - value4.W) * num + (-value1.W + 3f * value2.W - 3f * value3.W + value4.W) * num2);
            return result;
        }
        public static void CatmullRom(ref VECTOR4 value1, ref VECTOR4 value2, ref VECTOR4 value3, ref VECTOR4 value4, float amount, out VECTOR4 result)
        {
            float num = amount * amount;
            float num2 = amount * num;
            result.X = 0.5f * (2f * value2.X + (-value1.X + value3.X) * amount + (2f * value1.X - 5f * value2.X + 4f * value3.X - value4.X) * num + (-value1.X + 3f * value2.X - 3f * value3.X + value4.X) * num2);
            result.Y = 0.5f * (2f * value2.Y + (-value1.Y + value3.Y) * amount + (2f * value1.Y - 5f * value2.Y + 4f * value3.Y - value4.Y) * num + (-value1.Y + 3f * value2.Y - 3f * value3.Y + value4.Y) * num2);
            result.Z = 0.5f * (2f * value2.Z + (-value1.Z + value3.Z) * amount + (2f * value1.Z - 5f * value2.Z + 4f * value3.Z - value4.Z) * num + (-value1.Z + 3f * value2.Z - 3f * value3.Z + value4.Z) * num2);
            result.W = 0.5f * (2f * value2.W + (-value1.W + value3.W) * amount + (2f * value1.W - 5f * value2.W + 4f * value3.W - value4.W) * num + (-value1.W + 3f * value2.W - 3f * value3.W + value4.W) * num2);
        }
        public static VECTOR4 Hermite(VECTOR4 value1, VECTOR4 tangent1, VECTOR4 value2, VECTOR4 tangent2, float amount)
        {
            float num = amount * amount;
            float num2 = amount * num;
            float num3 = 2f * num2 - 3f * num + 1f;
            float num4 = -2f * num2 + 3f * num;
            float num5 = num2 - 2f * num + amount;
            float num6 = num2 - num;
            VECTOR4 result;
            result.X = value1.X * num3 + value2.X * num4 + tangent1.X * num5 + tangent2.X * num6;
            result.Y = value1.Y * num3 + value2.Y * num4 + tangent1.Y * num5 + tangent2.Y * num6;
            result.Z = value1.Z * num3 + value2.Z * num4 + tangent1.Z * num5 + tangent2.Z * num6;
            result.W = value1.W * num3 + value2.W * num4 + tangent1.W * num5 + tangent2.W * num6;
            return result;
        }
        public static void Hermite(ref VECTOR4 value1, ref VECTOR4 tangent1, ref VECTOR4 value2, ref VECTOR4 tangent2, float amount, out VECTOR4 result)
        {
            float num = amount * amount;
            float num2 = amount * num;
            float num3 = 2f * num2 - 3f * num + 1f;
            float num4 = -2f * num2 + 3f * num;
            float num5 = num2 - 2f * num + amount;
            float num6 = num2 - num;
            result.X = value1.X * num3 + value2.X * num4 + tangent1.X * num5 + tangent2.X * num6;
            result.Y = value1.Y * num3 + value2.Y * num4 + tangent1.Y * num5 + tangent2.Y * num6;
            result.Z = value1.Z * num3 + value2.Z * num4 + tangent1.Z * num5 + tangent2.Z * num6;
            result.W = value1.W * num3 + value2.W * num4 + tangent1.W * num5 + tangent2.W * num6;
        }
        public static VECTOR4 Transform(VECTOR2 position, MATRIX matrix)
        {
            float x = position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M41;
            float y = position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M42;
            float z = position.X * matrix.M13 + position.Y * matrix.M23 + matrix.M43;
            float w = position.X * matrix.M14 + position.Y * matrix.M24 + matrix.M44;
            VECTOR4 result;
            result.X = x;
            result.Y = y;
            result.Z = z;
            result.W = w;
            return result;
        }
        public static void Transform(ref VECTOR2 position, ref MATRIX matrix, out VECTOR4 result)
        {
            float x = position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M41;
            float y = position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M42;
            float z = position.X * matrix.M13 + position.Y * matrix.M23 + matrix.M43;
            float w = position.X * matrix.M14 + position.Y * matrix.M24 + matrix.M44;
            result.X = x;
            result.Y = y;
            result.Z = z;
            result.W = w;
        }
        public static VECTOR4 Transform(VECTOR3 position, MATRIX matrix)
        {
            float x = position.X * matrix.M11 + position.Y * matrix.M21 + position.Z * matrix.M31 + matrix.M41;
            float y = position.X * matrix.M12 + position.Y * matrix.M22 + position.Z * matrix.M32 + matrix.M42;
            float z = position.X * matrix.M13 + position.Y * matrix.M23 + position.Z * matrix.M33 + matrix.M43;
            float w = position.X * matrix.M14 + position.Y * matrix.M24 + position.Z * matrix.M34 + matrix.M44;
            VECTOR4 result;
            result.X = x;
            result.Y = y;
            result.Z = z;
            result.W = w;
            return result;
        }
        public static void Transform(ref VECTOR3 position, ref MATRIX matrix, out VECTOR4 result)
        {
            float x = position.X * matrix.M11 + position.Y * matrix.M21 + position.Z * matrix.M31 + matrix.M41;
            float y = position.X * matrix.M12 + position.Y * matrix.M22 + position.Z * matrix.M32 + matrix.M42;
            float z = position.X * matrix.M13 + position.Y * matrix.M23 + position.Z * matrix.M33 + matrix.M43;
            float w = position.X * matrix.M14 + position.Y * matrix.M24 + position.Z * matrix.M34 + matrix.M44;
            result.X = x;
            result.Y = y;
            result.Z = z;
            result.W = w;
        }
        public static VECTOR4 Transform(VECTOR4 vector, MATRIX matrix)
        {
            float x = vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31 + vector.W * matrix.M41;
            float y = vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32 + vector.W * matrix.M42;
            float z = vector.X * matrix.M13 + vector.Y * matrix.M23 + vector.Z * matrix.M33 + vector.W * matrix.M43;
            float w = vector.X * matrix.M14 + vector.Y * matrix.M24 + vector.Z * matrix.M34 + vector.W * matrix.M44;
            VECTOR4 result;
            result.X = x;
            result.Y = y;
            result.Z = z;
            result.W = w;
            return result;
        }
        public static void Transform(ref VECTOR4 vector, ref MATRIX matrix, out VECTOR4 result)
        {
            float x = vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31 + vector.W * matrix.M41;
            float y = vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32 + vector.W * matrix.M42;
            float z = vector.X * matrix.M13 + vector.Y * matrix.M23 + vector.Z * matrix.M33 + vector.W * matrix.M43;
            float w = vector.X * matrix.M14 + vector.Y * matrix.M24 + vector.Z * matrix.M34 + vector.W * matrix.M44;
            result.X = x;
            result.Y = y;
            result.Z = z;
            result.W = w;
        }
        public static void Transform(VECTOR4[] sourceArray, ref MATRIX matrix, VECTOR4[] destinationArray)
        {
            if (sourceArray == null)
            {
                throw new ArgumentNullException("sourceArray");
            }
            if (destinationArray == null)
            {
                throw new ArgumentNullException("destinationArray");
            }
            for (int i = 0; i < sourceArray.Length; i++)
            {
                float x = sourceArray[i].X;
                float y = sourceArray[i].Y;
                float z = sourceArray[i].Z;
                float w = sourceArray[i].W;
                destinationArray[i].X = x * matrix.M11 + y * matrix.M21 + z * matrix.M31 + w * matrix.M41;
                destinationArray[i].Y = x * matrix.M12 + y * matrix.M22 + z * matrix.M32 + w * matrix.M42;
                destinationArray[i].Z = x * matrix.M13 + y * matrix.M23 + z * matrix.M33 + w * matrix.M43;
                destinationArray[i].W = x * matrix.M14 + y * matrix.M24 + z * matrix.M34 + w * matrix.M44;
            }
        }
        public static void Transform(VECTOR4[] sourceArray, int sourceIndex, ref MATRIX matrix, VECTOR4[] destinationArray, int destinationIndex, int length)
        {
            if (sourceArray == null)
            {
                throw new ArgumentNullException("sourceArray");
            }
            if (destinationArray == null)
            {
                throw new ArgumentNullException("destinationArray");
            }
            while (length > 0)
            {
                float x = sourceArray[sourceIndex].X;
                float y = sourceArray[sourceIndex].Y;
                float z = sourceArray[sourceIndex].Z;
                float w = sourceArray[sourceIndex].W;
                destinationArray[destinationIndex].X = x * matrix.M11 + y * matrix.M21 + z * matrix.M31 + w * matrix.M41;
                destinationArray[destinationIndex].Y = x * matrix.M12 + y * matrix.M22 + z * matrix.M32 + w * matrix.M42;
                destinationArray[destinationIndex].Z = x * matrix.M13 + y * matrix.M23 + z * matrix.M33 + w * matrix.M43;
                destinationArray[destinationIndex].W = x * matrix.M14 + y * matrix.M24 + z * matrix.M34 + w * matrix.M44;
                sourceIndex++;
                destinationIndex++;
                length--;
            }
        }
        public static VECTOR4 Negate(VECTOR4 value)
        {
            VECTOR4 result;
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
            result.W = -value.W;
            return result;
        }
        public static void Negate(ref VECTOR4 value, out VECTOR4 result)
        {
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
            result.W = -value.W;
        }
        public static VECTOR4 Add(VECTOR4 value1, VECTOR4 value2)
        {
            VECTOR4 result;
            result.X = value1.X + value2.X;
            result.Y = value1.Y + value2.Y;
            result.Z = value1.Z + value2.Z;
            result.W = value1.W + value2.W;
            return result;
        }
        public static void Add(ref VECTOR4 value1, ref VECTOR4 value2, out VECTOR4 result)
        {
            result.X = value1.X + value2.X;
            result.Y = value1.Y + value2.Y;
            result.Z = value1.Z + value2.Z;
            result.W = value1.W + value2.W;
        }
        public static VECTOR4 Subtract(VECTOR4 value1, VECTOR4 value2)
        {
            VECTOR4 result;
            result.X = value1.X - value2.X;
            result.Y = value1.Y - value2.Y;
            result.Z = value1.Z - value2.Z;
            result.W = value1.W - value2.W;
            return result;
        }
        public static void Subtract(ref VECTOR4 value1, ref VECTOR4 value2, out VECTOR4 result)
        {
            result.X = value1.X - value2.X;
            result.Y = value1.Y - value2.Y;
            result.Z = value1.Z - value2.Z;
            result.W = value1.W - value2.W;
        }
        public static VECTOR4 Multiply(VECTOR4 value1, VECTOR4 value2)
        {
            VECTOR4 result;
            result.X = value1.X * value2.X;
            result.Y = value1.Y * value2.Y;
            result.Z = value1.Z * value2.Z;
            result.W = value1.W * value2.W;
            return result;
        }
        public static void Multiply(ref VECTOR4 value1, ref VECTOR4 value2, out VECTOR4 result)
        {
            result.X = value1.X * value2.X;
            result.Y = value1.Y * value2.Y;
            result.Z = value1.Z * value2.Z;
            result.W = value1.W * value2.W;
        }
        public static VECTOR4 Multiply(VECTOR4 value1, float scaleFactor)
        {
            VECTOR4 result;
            result.X = value1.X * scaleFactor;
            result.Y = value1.Y * scaleFactor;
            result.Z = value1.Z * scaleFactor;
            result.W = value1.W * scaleFactor;
            return result;
        }
        public static void Multiply(ref VECTOR4 value1, float scaleFactor, out VECTOR4 result)
        {
            result.X = value1.X * scaleFactor;
            result.Y = value1.Y * scaleFactor;
            result.Z = value1.Z * scaleFactor;
            result.W = value1.W * scaleFactor;
        }
        public static VECTOR4 Divide(VECTOR4 value1, VECTOR4 value2)
        {
            VECTOR4 result;
            result.X = value1.X / value2.X;
            result.Y = value1.Y / value2.Y;
            result.Z = value1.Z / value2.Z;
            result.W = value1.W / value2.W;
            return result;
        }
        public static void Divide(ref VECTOR4 value1, ref VECTOR4 value2, out VECTOR4 result)
        {
            result.X = value1.X / value2.X;
            result.Y = value1.Y / value2.Y;
            result.Z = value1.Z / value2.Z;
            result.W = value1.W / value2.W;
        }
        public static VECTOR4 Divide(VECTOR4 value1, float divider)
        {
            float num = 1f / divider;
            VECTOR4 result;
            result.X = value1.X * num;
            result.Y = value1.Y * num;
            result.Z = value1.Z * num;
            result.W = value1.W * num;
            return result;
        }
        public static void Divide(ref VECTOR4 value1, float divider, out VECTOR4 result)
        {
            float num = 1f / divider;
            result.X = value1.X * num;
            result.Y = value1.Y * num;
            result.Z = value1.Z * num;
            result.W = value1.W * num;
        }
        public static VECTOR4 operator -(VECTOR4 value)
        {
            VECTOR4 result;
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
            result.W = -value.W;
            return result;
        }
        public static bool operator ==(VECTOR4 value1, VECTOR4 value2)
        {
            return value1.X == value2.X && value1.Y == value2.Y && value1.Z == value2.Z && value1.W == value2.W;
        }
        public static bool operator !=(VECTOR4 value1, VECTOR4 value2)
        {
            return value1.X != value2.X || value1.Y != value2.Y || value1.Z != value2.Z || value1.W != value2.W;
        }
        public static VECTOR4 operator +(VECTOR4 value1, VECTOR4 value2)
        {
            VECTOR4 result;
            result.X = value1.X + value2.X;
            result.Y = value1.Y + value2.Y;
            result.Z = value1.Z + value2.Z;
            result.W = value1.W + value2.W;
            return result;
        }
        public static VECTOR4 operator -(VECTOR4 value1, VECTOR4 value2)
        {
            VECTOR4 result;
            result.X = value1.X - value2.X;
            result.Y = value1.Y - value2.Y;
            result.Z = value1.Z - value2.Z;
            result.W = value1.W - value2.W;
            return result;
        }
        public static VECTOR4 operator *(VECTOR4 value1, VECTOR4 value2)
        {
            VECTOR4 result;
            result.X = value1.X * value2.X;
            result.Y = value1.Y * value2.Y;
            result.Z = value1.Z * value2.Z;
            result.W = value1.W * value2.W;
            return result;
        }
        public static VECTOR4 operator *(VECTOR4 value1, float scaleFactor)
        {
            VECTOR4 result;
            result.X = value1.X * scaleFactor;
            result.Y = value1.Y * scaleFactor;
            result.Z = value1.Z * scaleFactor;
            result.W = value1.W * scaleFactor;
            return result;
        }
        public static VECTOR4 operator *(float scaleFactor, VECTOR4 value1)
        {
            VECTOR4 result;
            result.X = value1.X * scaleFactor;
            result.Y = value1.Y * scaleFactor;
            result.Z = value1.Z * scaleFactor;
            result.W = value1.W * scaleFactor;
            return result;
        }
        public static VECTOR4 operator /(VECTOR4 value1, VECTOR4 value2)
        {
            VECTOR4 result;
            result.X = value1.X / value2.X;
            result.Y = value1.Y / value2.Y;
            result.Z = value1.Z / value2.Z;
            result.W = value1.W / value2.W;
            return result;
        }
        public static VECTOR4 operator /(VECTOR4 value1, float divider)
        {
            float num = 1f / divider;
            VECTOR4 result;
            result.X = value1.X * num;
            result.Y = value1.Y * num;
            result.Z = value1.Z * num;
            result.W = value1.W * num;
            return result;
        }
    }
    public struct Quaternion : IEquatable<Quaternion>
    {
        public float X;
        public float Y;
        public float Z;
        public float W;
        private static Quaternion _identity = new Quaternion(0f, 0f, 0f, 1f);
        public static Quaternion Identity
        {
            get
            {
                return Quaternion._identity;
            }
        }
        public Quaternion(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }
        public Quaternion(VECTOR3 vectorPart, float scalarPart)
        {
            this.X = vectorPart.X;
            this.Y = vectorPart.Y;
            this.Z = vectorPart.Z;
            this.W = scalarPart;
        }
        public override string ToString()
        {
            return string.Format("{{X:{0} Y:{1} Z:{2} W:{3}}}", new object[]
			{
				this.X.ToString(),
				this.Y.ToString(),
				this.Z.ToString(),
				this.W.ToString()
			});
        }
        public bool Equals(Quaternion other)
        {
            return this.X == other.X && this.Y == other.Y && this.Z == other.Z && this.W == other.W;
        }
        public override bool Equals(object obj)
        {
            bool result = false;
            if (obj is Quaternion)
            {
                result = this.Equals((Quaternion)obj);
            }
            return result;
        }
        public override int GetHashCode()
        {
            return this.X.GetHashCode() + this.Y.GetHashCode() + this.Z.GetHashCode() + this.W.GetHashCode();
        }
        public float LengthSquared()
        {
            return this.X * this.X + this.Y * this.Y + this.Z * this.Z + this.W * this.W;
        }
        public float Length()
        {
            float num = this.X * this.X + this.Y * this.Y + this.Z * this.Z + this.W * this.W;
            return (float)Math.Sqrt((double)num);
        }
        public void Normalize()
        {
            float num = this.X * this.X + this.Y * this.Y + this.Z * this.Z + this.W * this.W;
            float num2 = 1f / (float)Math.Sqrt((double)num);
            this.X *= num2;
            this.Y *= num2;
            this.Z *= num2;
            this.W *= num2;
        }
        public static Quaternion Normalize(Quaternion quaternion)
        {
            float num = quaternion.X * quaternion.X + quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z + quaternion.W * quaternion.W;
            float num2 = 1f / (float)Math.Sqrt((double)num);
            Quaternion result;
            result.X = quaternion.X * num2;
            result.Y = quaternion.Y * num2;
            result.Z = quaternion.Z * num2;
            result.W = quaternion.W * num2;
            return result;
        }
        public static void Normalize(ref Quaternion quaternion, out Quaternion result)
        {
            float num = quaternion.X * quaternion.X + quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z + quaternion.W * quaternion.W;
            float num2 = 1f / (float)Math.Sqrt((double)num);
            result.X = quaternion.X * num2;
            result.Y = quaternion.Y * num2;
            result.Z = quaternion.Z * num2;
            result.W = quaternion.W * num2;
        }
        public void Conjugate()
        {
            this.X = -this.X;
            this.Y = -this.Y;
            this.Z = -this.Z;
        }
        public static Quaternion Conjugate(Quaternion value)
        {
            Quaternion result;
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
            result.W = value.W;
            return result;
        }
        public static void Conjugate(ref Quaternion value, out Quaternion result)
        {
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
            result.W = value.W;
        }
        public static Quaternion Inverse(Quaternion quaternion)
        {
            float num = quaternion.X * quaternion.X + quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z + quaternion.W * quaternion.W;
            float num2 = 1f / num;
            Quaternion result;
            result.X = -quaternion.X * num2;
            result.Y = -quaternion.Y * num2;
            result.Z = -quaternion.Z * num2;
            result.W = quaternion.W * num2;
            return result;
        }
        public static void Inverse(ref Quaternion quaternion, out Quaternion result)
        {
            float num = quaternion.X * quaternion.X + quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z + quaternion.W * quaternion.W;
            float num2 = 1f / num;
            result.X = -quaternion.X * num2;
            result.Y = -quaternion.Y * num2;
            result.Z = -quaternion.Z * num2;
            result.W = quaternion.W * num2;
        }
        public static Quaternion CreateFromAxisAngle(VECTOR3 axis, float angle)
        {
            float num = angle * 0.5f;
            float num2 = (float)Math.Sin((double)num);
            float w = (float)Math.Cos((double)num);
            Quaternion result;
            result.X = axis.X * num2;
            result.Y = axis.Y * num2;
            result.Z = axis.Z * num2;
            result.W = w;
            return result;
        }
        public static void CreateFromAxisAngle(ref VECTOR3 axis, float angle, out Quaternion result)
        {
            float num = angle * 0.5f;
            float num2 = (float)Math.Sin((double)num);
            float w = (float)Math.Cos((double)num);
            result.X = axis.X * num2;
            result.Y = axis.Y * num2;
            result.Z = axis.Z * num2;
            result.W = w;
        }
        public static Quaternion CreateFromYawPitchRoll(float yaw, float pitch, float roll)
        {
            float num = roll * 0.5f;
            float num2 = (float)Math.Sin((double)num);
            float num3 = (float)Math.Cos((double)num);
            float num4 = pitch * 0.5f;
            float num5 = (float)Math.Sin((double)num4);
            float num6 = (float)Math.Cos((double)num4);
            float num7 = yaw * 0.5f;
            float num8 = (float)Math.Sin((double)num7);
            float num9 = (float)Math.Cos((double)num7);
            Quaternion result;
            result.X = num9 * num5 * num3 + num8 * num6 * num2;
            result.Y = num8 * num6 * num3 - num9 * num5 * num2;
            result.Z = num9 * num6 * num2 - num8 * num5 * num3;
            result.W = num9 * num6 * num3 + num8 * num5 * num2;
            return result;
        }
        public static void CreateFromYawPitchRoll(float yaw, float pitch, float roll, out Quaternion result)
        {
            float num = roll * 0.5f;
            float num2 = (float)Math.Sin((double)num);
            float num3 = (float)Math.Cos((double)num);
            float num4 = pitch * 0.5f;
            float num5 = (float)Math.Sin((double)num4);
            float num6 = (float)Math.Cos((double)num4);
            float num7 = yaw * 0.5f;
            float num8 = (float)Math.Sin((double)num7);
            float num9 = (float)Math.Cos((double)num7);
            result.X = num9 * num5 * num3 + num8 * num6 * num2;
            result.Y = num8 * num6 * num3 - num9 * num5 * num2;
            result.Z = num9 * num6 * num2 - num8 * num5 * num3;
            result.W = num9 * num6 * num3 + num8 * num5 * num2;
        }
        public static Quaternion CreateFromRotationMatrix(MATRIX matrix)
        {
            float num = matrix.M11 + matrix.M22 + matrix.M33;
            Quaternion result = default(Quaternion);
            if (num > 0f)
            {
                float num2 = (float)Math.Sqrt((double)(num + 1f));
                result.W = num2 * 0.5f;
                num2 = 0.5f / num2;
                result.X = (matrix.M23 - matrix.M32) * num2;
                result.Y = (matrix.M31 - matrix.M13) * num2;
                result.Z = (matrix.M12 - matrix.M21) * num2;
            }
            else
            {
                if (matrix.M11 >= matrix.M22 && matrix.M11 >= matrix.M33)
                {
                    float num3 = (float)Math.Sqrt((double)(1f + matrix.M11 - matrix.M22 - matrix.M33));
                    float num4 = 0.5f / num3;
                    result.X = 0.5f * num3;
                    result.Y = (matrix.M12 + matrix.M21) * num4;
                    result.Z = (matrix.M13 + matrix.M31) * num4;
                    result.W = (matrix.M23 - matrix.M32) * num4;
                }
                else
                {
                    if (matrix.M22 > matrix.M33)
                    {
                        float num5 = (float)Math.Sqrt((double)(1f + matrix.M22 - matrix.M11 - matrix.M33));
                        float num6 = 0.5f / num5;
                        result.X = (matrix.M21 + matrix.M12) * num6;
                        result.Y = 0.5f * num5;
                        result.Z = (matrix.M32 + matrix.M23) * num6;
                        result.W = (matrix.M31 - matrix.M13) * num6;
                    }
                    else
                    {
                        float num7 = (float)Math.Sqrt((double)(1f + matrix.M33 - matrix.M11 - matrix.M22));
                        float num8 = 0.5f / num7;
                        result.X = (matrix.M31 + matrix.M13) * num8;
                        result.Y = (matrix.M32 + matrix.M23) * num8;
                        result.Z = 0.5f * num7;
                        result.W = (matrix.M12 - matrix.M21) * num8;
                    }
                }
            }
            return result;
        }
        public static void CreateFromRotationMatrix(ref MATRIX matrix, out Quaternion result)
        {
            float num = matrix.M11 + matrix.M22 + matrix.M33;
            if (num > 0f)
            {
                float num2 = (float)Math.Sqrt((double)(num + 1f));
                result.W = num2 * 0.5f;
                num2 = 0.5f / num2;
                result.X = (matrix.M23 - matrix.M32) * num2;
                result.Y = (matrix.M31 - matrix.M13) * num2;
                result.Z = (matrix.M12 - matrix.M21) * num2;
                return;
            }
            if (matrix.M11 >= matrix.M22 && matrix.M11 >= matrix.M33)
            {
                float num3 = (float)Math.Sqrt((double)(1f + matrix.M11 - matrix.M22 - matrix.M33));
                float num4 = 0.5f / num3;
                result.X = 0.5f * num3;
                result.Y = (matrix.M12 + matrix.M21) * num4;
                result.Z = (matrix.M13 + matrix.M31) * num4;
                result.W = (matrix.M23 - matrix.M32) * num4;
                return;
            }
            if (matrix.M22 > matrix.M33)
            {
                float num5 = (float)Math.Sqrt((double)(1f + matrix.M22 - matrix.M11 - matrix.M33));
                float num6 = 0.5f / num5;
                result.X = (matrix.M21 + matrix.M12) * num6;
                result.Y = 0.5f * num5;
                result.Z = (matrix.M32 + matrix.M23) * num6;
                result.W = (matrix.M31 - matrix.M13) * num6;
                return;
            }
            float num7 = (float)Math.Sqrt((double)(1f + matrix.M33 - matrix.M11 - matrix.M22));
            float num8 = 0.5f / num7;
            result.X = (matrix.M31 + matrix.M13) * num8;
            result.Y = (matrix.M32 + matrix.M23) * num8;
            result.Z = 0.5f * num7;
            result.W = (matrix.M12 - matrix.M21) * num8;
        }
        public static float Dot(Quaternion quaternion1, Quaternion quaternion2)
        {
            return quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y + quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W;
        }
        public static void Dot(ref Quaternion quaternion1, ref Quaternion quaternion2, out float result)
        {
            result = quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y + quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W;
        }
        public static Quaternion Slerp(Quaternion quaternion1, Quaternion quaternion2, float amount)
        {
            float num = quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y + quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W;
            bool flag = false;
            if (num < 0f)
            {
                flag = true;
                num = -num;
            }
            float num2;
            float num3;
            if (num > 0.999999f)
            {
                num2 = 1f - amount;
                num3 = (flag ? (-amount) : amount);
            }
            else
            {
                float num4 = (float)Math.Acos((double)num);
                float num5 = (float)(1.0 / Math.Sin((double)num4));
                num2 = (float)Math.Sin((double)((1f - amount) * num4)) * num5;
                //num3 = (flag ? ((float)(-(float)Math.Sin((double)(amount * num4))) * num5) : ((float)Math.Sin((double)(amount * num4)) * num5));
                num3 = (flag ? -((float)Math.Sin((double)(amount * num4)) * num5) : ((float)Math.Sin((double)(amount * num4)) * num5));
            }
            Quaternion result;
            result.X = num2 * quaternion1.X + num3 * quaternion2.X;
            result.Y = num2 * quaternion1.Y + num3 * quaternion2.Y;
            result.Z = num2 * quaternion1.Z + num3 * quaternion2.Z;
            result.W = num2 * quaternion1.W + num3 * quaternion2.W;
            return result;
        }
        public static void Slerp(ref Quaternion quaternion1, ref Quaternion quaternion2, float amount, out Quaternion result)
        {
            float num = quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y + quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W;
            bool flag = false;
            if (num < 0f)
            {
                flag = true;
                num = -num;
            }
            float num2;
            float num3;
            if (num > 0.999999f)
            {
                num2 = 1f - amount;
                num3 = (flag ? (-amount) : amount);
            }
            else
            {
                float num4 = (float)Math.Acos((double)num);
                float num5 = (float)(1.0 / Math.Sin((double)num4));
                num2 = (float)Math.Sin((double)((1f - amount) * num4)) * num5;
                //num3 = (flag ? ((float)(-(float)Math.Sin((double)(amount * num4))) * num5) : ((float)Math.Sin((double)(amount * num4)) * num5));
                num3 = (flag ? -((float)Math.Sin((double)(amount * num4)) * num5) : ((float)Math.Sin((double)(amount * num4)) * num5));
            }
            result.X = num2 * quaternion1.X + num3 * quaternion2.X;
            result.Y = num2 * quaternion1.Y + num3 * quaternion2.Y;
            result.Z = num2 * quaternion1.Z + num3 * quaternion2.Z;
            result.W = num2 * quaternion1.W + num3 * quaternion2.W;
        }
        public static Quaternion Lerp(Quaternion quaternion1, Quaternion quaternion2, float amount)
        {
            float num = 1f - amount;
            Quaternion result = default(Quaternion);
            float num2 = quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y + quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W;
            if (num2 >= 0f)
            {
                result.X = num * quaternion1.X + amount * quaternion2.X;
                result.Y = num * quaternion1.Y + amount * quaternion2.Y;
                result.Z = num * quaternion1.Z + amount * quaternion2.Z;
                result.W = num * quaternion1.W + amount * quaternion2.W;
            }
            else
            {
                result.X = num * quaternion1.X - amount * quaternion2.X;
                result.Y = num * quaternion1.Y - amount * quaternion2.Y;
                result.Z = num * quaternion1.Z - amount * quaternion2.Z;
                result.W = num * quaternion1.W - amount * quaternion2.W;
            }
            float num3 = result.X * result.X + result.Y * result.Y + result.Z * result.Z + result.W * result.W;
            float num4 = 1f / (float)Math.Sqrt((double)num3);
            result.X *= num4;
            result.Y *= num4;
            result.Z *= num4;
            result.W *= num4;
            return result;
        }
        public static void Lerp(ref Quaternion quaternion1, ref Quaternion quaternion2, float amount, out Quaternion result)
        {
            float num = 1f - amount;
            float num2 = quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y + quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W;
            if (num2 >= 0f)
            {
                result.X = num * quaternion1.X + amount * quaternion2.X;
                result.Y = num * quaternion1.Y + amount * quaternion2.Y;
                result.Z = num * quaternion1.Z + amount * quaternion2.Z;
                result.W = num * quaternion1.W + amount * quaternion2.W;
            }
            else
            {
                result.X = num * quaternion1.X - amount * quaternion2.X;
                result.Y = num * quaternion1.Y - amount * quaternion2.Y;
                result.Z = num * quaternion1.Z - amount * quaternion2.Z;
                result.W = num * quaternion1.W - amount * quaternion2.W;
            }
            float num3 = result.X * result.X + result.Y * result.Y + result.Z * result.Z + result.W * result.W;
            float num4 = 1f / (float)Math.Sqrt((double)num3);
            result.X *= num4;
            result.Y *= num4;
            result.Z *= num4;
            result.W *= num4;
        }
        public static Quaternion Concatenate(Quaternion value1, Quaternion value2)
        {
            float x = value2.X;
            float y = value2.Y;
            float z = value2.Z;
            float w = value2.W;
            float x2 = value1.X;
            float y2 = value1.Y;
            float z2 = value1.Z;
            float w2 = value1.W;
            float num = y * z2 - z * y2;
            float num2 = z * x2 - x * z2;
            float num3 = x * y2 - y * x2;
            float num4 = x * x2 + y * y2 + z * z2;
            Quaternion result;
            result.X = x * w2 + x2 * w + num;
            result.Y = y * w2 + y2 * w + num2;
            result.Z = z * w2 + z2 * w + num3;
            result.W = w * w2 - num4;
            return result;
        }
        public static void Concatenate(ref Quaternion value1, ref Quaternion value2, out Quaternion result)
        {
            float x = value2.X;
            float y = value2.Y;
            float z = value2.Z;
            float w = value2.W;
            float x2 = value1.X;
            float y2 = value1.Y;
            float z2 = value1.Z;
            float w2 = value1.W;
            float num = y * z2 - z * y2;
            float num2 = z * x2 - x * z2;
            float num3 = x * y2 - y * x2;
            float num4 = x * x2 + y * y2 + z * z2;
            result.X = x * w2 + x2 * w + num;
            result.Y = y * w2 + y2 * w + num2;
            result.Z = z * w2 + z2 * w + num3;
            result.W = w * w2 - num4;
        }
        public static Quaternion Negate(Quaternion quaternion)
        {
            Quaternion result;
            result.X = -quaternion.X;
            result.Y = -quaternion.Y;
            result.Z = -quaternion.Z;
            result.W = -quaternion.W;
            return result;
        }
        public static void Negate(ref Quaternion quaternion, out Quaternion result)
        {
            result.X = -quaternion.X;
            result.Y = -quaternion.Y;
            result.Z = -quaternion.Z;
            result.W = -quaternion.W;
        }
        public static Quaternion Add(Quaternion quaternion1, Quaternion quaternion2)
        {
            Quaternion result;
            result.X = quaternion1.X + quaternion2.X;
            result.Y = quaternion1.Y + quaternion2.Y;
            result.Z = quaternion1.Z + quaternion2.Z;
            result.W = quaternion1.W + quaternion2.W;
            return result;
        }
        public static void Add(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
        {
            result.X = quaternion1.X + quaternion2.X;
            result.Y = quaternion1.Y + quaternion2.Y;
            result.Z = quaternion1.Z + quaternion2.Z;
            result.W = quaternion1.W + quaternion2.W;
        }
        public static Quaternion Subtract(Quaternion quaternion1, Quaternion quaternion2)
        {
            Quaternion result;
            result.X = quaternion1.X - quaternion2.X;
            result.Y = quaternion1.Y - quaternion2.Y;
            result.Z = quaternion1.Z - quaternion2.Z;
            result.W = quaternion1.W - quaternion2.W;
            return result;
        }
        public static void Subtract(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
        {
            result.X = quaternion1.X - quaternion2.X;
            result.Y = quaternion1.Y - quaternion2.Y;
            result.Z = quaternion1.Z - quaternion2.Z;
            result.W = quaternion1.W - quaternion2.W;
        }
        public static Quaternion Multiply(Quaternion quaternion1, Quaternion quaternion2)
        {
            float x = quaternion1.X;
            float y = quaternion1.Y;
            float z = quaternion1.Z;
            float w = quaternion1.W;
            float x2 = quaternion2.X;
            float y2 = quaternion2.Y;
            float z2 = quaternion2.Z;
            float w2 = quaternion2.W;
            float num = y * z2 - z * y2;
            float num2 = z * x2 - x * z2;
            float num3 = x * y2 - y * x2;
            float num4 = x * x2 + y * y2 + z * z2;
            Quaternion result;
            result.X = x * w2 + x2 * w + num;
            result.Y = y * w2 + y2 * w + num2;
            result.Z = z * w2 + z2 * w + num3;
            result.W = w * w2 - num4;
            return result;
        }
        public static void Multiply(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
        {
            float x = quaternion1.X;
            float y = quaternion1.Y;
            float z = quaternion1.Z;
            float w = quaternion1.W;
            float x2 = quaternion2.X;
            float y2 = quaternion2.Y;
            float z2 = quaternion2.Z;
            float w2 = quaternion2.W;
            float num = y * z2 - z * y2;
            float num2 = z * x2 - x * z2;
            float num3 = x * y2 - y * x2;
            float num4 = x * x2 + y * y2 + z * z2;
            result.X = x * w2 + x2 * w + num;
            result.Y = y * w2 + y2 * w + num2;
            result.Z = z * w2 + z2 * w + num3;
            result.W = w * w2 - num4;
        }
        public static Quaternion Multiply(Quaternion quaternion1, float scaleFactor)
        {
            Quaternion result;
            result.X = quaternion1.X * scaleFactor;
            result.Y = quaternion1.Y * scaleFactor;
            result.Z = quaternion1.Z * scaleFactor;
            result.W = quaternion1.W * scaleFactor;
            return result;
        }
        public static void Multiply(ref Quaternion quaternion1, float scaleFactor, out Quaternion result)
        {
            result.X = quaternion1.X * scaleFactor;
            result.Y = quaternion1.Y * scaleFactor;
            result.Z = quaternion1.Z * scaleFactor;
            result.W = quaternion1.W * scaleFactor;
        }
        public static Quaternion Divide(Quaternion quaternion1, Quaternion quaternion2)
        {
            float x = quaternion1.X;
            float y = quaternion1.Y;
            float z = quaternion1.Z;
            float w = quaternion1.W;
            float num = quaternion2.X * quaternion2.X + quaternion2.Y * quaternion2.Y + quaternion2.Z * quaternion2.Z + quaternion2.W * quaternion2.W;
            float num2 = 1f / num;
            float num3 = -quaternion2.X * num2;
            float num4 = -quaternion2.Y * num2;
            float num5 = -quaternion2.Z * num2;
            float num6 = quaternion2.W * num2;
            float num7 = y * num5 - z * num4;
            float num8 = z * num3 - x * num5;
            float num9 = x * num4 - y * num3;
            float num10 = x * num3 + y * num4 + z * num5;
            Quaternion result;
            result.X = x * num6 + num3 * w + num7;
            result.Y = y * num6 + num4 * w + num8;
            result.Z = z * num6 + num5 * w + num9;
            result.W = w * num6 - num10;
            return result;
        }
        public static void Divide(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
        {
            float x = quaternion1.X;
            float y = quaternion1.Y;
            float z = quaternion1.Z;
            float w = quaternion1.W;
            float num = quaternion2.X * quaternion2.X + quaternion2.Y * quaternion2.Y + quaternion2.Z * quaternion2.Z + quaternion2.W * quaternion2.W;
            float num2 = 1f / num;
            float num3 = -quaternion2.X * num2;
            float num4 = -quaternion2.Y * num2;
            float num5 = -quaternion2.Z * num2;
            float num6 = quaternion2.W * num2;
            float num7 = y * num5 - z * num4;
            float num8 = z * num3 - x * num5;
            float num9 = x * num4 - y * num3;
            float num10 = x * num3 + y * num4 + z * num5;
            result.X = x * num6 + num3 * w + num7;
            result.Y = y * num6 + num4 * w + num8;
            result.Z = z * num6 + num5 * w + num9;
            result.W = w * num6 - num10;
        }
        public static Quaternion operator -(Quaternion quaternion)
        {
            Quaternion result;
            result.X = -quaternion.X;
            result.Y = -quaternion.Y;
            result.Z = -quaternion.Z;
            result.W = -quaternion.W;
            return result;
        }
        public static bool operator ==(Quaternion quaternion1, Quaternion quaternion2)
        {
            return quaternion1.X == quaternion2.X && quaternion1.Y == quaternion2.Y && quaternion1.Z == quaternion2.Z && quaternion1.W == quaternion2.W;
        }
        public static bool operator !=(Quaternion quaternion1, Quaternion quaternion2)
        {
            return quaternion1.X != quaternion2.X || quaternion1.Y != quaternion2.Y || quaternion1.Z != quaternion2.Z || quaternion1.W != quaternion2.W;
        }
        public static Quaternion operator +(Quaternion quaternion1, Quaternion quaternion2)
        {
            Quaternion result;
            result.X = quaternion1.X + quaternion2.X;
            result.Y = quaternion1.Y + quaternion2.Y;
            result.Z = quaternion1.Z + quaternion2.Z;
            result.W = quaternion1.W + quaternion2.W;
            return result;
        }
        public static Quaternion operator -(Quaternion quaternion1, Quaternion quaternion2)
        {
            Quaternion result;
            result.X = quaternion1.X - quaternion2.X;
            result.Y = quaternion1.Y - quaternion2.Y;
            result.Z = quaternion1.Z - quaternion2.Z;
            result.W = quaternion1.W - quaternion2.W;
            return result;
        }
        public static Quaternion operator *(Quaternion quaternion1, Quaternion quaternion2)
        {
            float x = quaternion1.X;
            float y = quaternion1.Y;
            float z = quaternion1.Z;
            float w = quaternion1.W;
            float x2 = quaternion2.X;
            float y2 = quaternion2.Y;
            float z2 = quaternion2.Z;
            float w2 = quaternion2.W;
            float num = y * z2 - z * y2;
            float num2 = z * x2 - x * z2;
            float num3 = x * y2 - y * x2;
            float num4 = x * x2 + y * y2 + z * z2;
            Quaternion result;
            result.X = x * w2 + x2 * w + num;
            result.Y = y * w2 + y2 * w + num2;
            result.Z = z * w2 + z2 * w + num3;
            result.W = w * w2 - num4;
            return result;
        }
        public static Quaternion operator *(Quaternion quaternion1, float scaleFactor)
        {
            Quaternion result;
            result.X = quaternion1.X * scaleFactor;
            result.Y = quaternion1.Y * scaleFactor;
            result.Z = quaternion1.Z * scaleFactor;
            result.W = quaternion1.W * scaleFactor;
            return result;
        }
        public static Quaternion operator /(Quaternion quaternion1, Quaternion quaternion2)
        {
            float x = quaternion1.X;
            float y = quaternion1.Y;
            float z = quaternion1.Z;
            float w = quaternion1.W;
            float num = quaternion2.X * quaternion2.X + quaternion2.Y * quaternion2.Y + quaternion2.Z * quaternion2.Z + quaternion2.W * quaternion2.W;
            float num2 = 1f / num;
            float num3 = -quaternion2.X * num2;
            float num4 = -quaternion2.Y * num2;
            float num5 = -quaternion2.Z * num2;
            float num6 = quaternion2.W * num2;
            float num7 = y * num5 - z * num4;
            float num8 = z * num3 - x * num5;
            float num9 = x * num4 - y * num3;
            float num10 = x * num3 + y * num4 + z * num5;
            Quaternion result;
            result.X = x * num6 + num3 * w + num7;
            result.Y = y * num6 + num4 * w + num8;
            result.Z = z * num6 + num5 * w + num9;
            result.W = w * num6 - num10;
            return result;
        }
    }
    /// <summary>三维矩阵变换</summary>
    public struct MATRIX : IEquatable<MATRIX>
    {
        private struct CanonicalBasis
        {
            public VECTOR3 Row0;
            public VECTOR3 Row1;
            public VECTOR3 Row2;
        }
        public float M11;
        public float M12;
        public float M13;
        public float M14;
        public float M21;
        public float M22;
        public float M23;
        public float M24;
        public float M31;
        public float M32;
        public float M33;
        public float M34;
        public float M41;
        public float M42;
        public float M43;
        public float M44;
        private static MATRIX _identity = new MATRIX(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f);
        public static MATRIX Identity
        {
            get
            {
                return MATRIX._identity;
            }
        }
        public VECTOR3 Up
        {
            get
            {
                VECTOR3 result;
                result.X = this.M21;
                result.Y = this.M22;
                result.Z = this.M23;
                return result;
            }
            set
            {
                this.M21 = value.X;
                this.M22 = value.Y;
                this.M23 = value.Z;
            }
        }
        public VECTOR3 Down
        {
            get
            {
                VECTOR3 result;
                result.X = -this.M21;
                result.Y = -this.M22;
                result.Z = -this.M23;
                return result;
            }
            set
            {
                this.M21 = -value.X;
                this.M22 = -value.Y;
                this.M23 = -value.Z;
            }
        }
        public VECTOR3 Right
        {
            get
            {
                VECTOR3 result;
                result.X = this.M11;
                result.Y = this.M12;
                result.Z = this.M13;
                return result;
            }
            set
            {
                this.M11 = value.X;
                this.M12 = value.Y;
                this.M13 = value.Z;
            }
        }
        public VECTOR3 Left
        {
            get
            {
                VECTOR3 result;
                result.X = -this.M11;
                result.Y = -this.M12;
                result.Z = -this.M13;
                return result;
            }
            set
            {
                this.M11 = -value.X;
                this.M12 = -value.Y;
                this.M13 = -value.Z;
            }
        }
        public VECTOR3 Forward
        {
            get
            {
                VECTOR3 result;
                result.X = -this.M31;
                result.Y = -this.M32;
                result.Z = -this.M33;
                return result;
            }
            set
            {
                this.M31 = -value.X;
                this.M32 = -value.Y;
                this.M33 = -value.Z;
            }
        }
        public VECTOR3 Backward
        {
            get
            {
                VECTOR3 result;
                result.X = this.M31;
                result.Y = this.M32;
                result.Z = this.M33;
                return result;
            }
            set
            {
                this.M31 = value.X;
                this.M32 = value.Y;
                this.M33 = value.Z;
            }
        }
        public VECTOR3 Translation
        {
            get
            {
                VECTOR3 result;
                result.X = this.M41;
                result.Y = this.M42;
                result.Z = this.M43;
                return result;
            }
            set
            {
                this.M41 = value.X;
                this.M42 = value.Y;
                this.M43 = value.Z;
            }
        }
        public VECTOR3 Scale
        {
            get
            {
                VECTOR3 result;
                result.X = this.M11;
                result.Y = this.M22;
                result.Z = this.M33;
                return result;
            }
            set
            {
                this.M11 = value.X;
                this.M22 = value.Y;
                this.M33 = value.Z;
            }
        }
        public MATRIX(float m11, float m12, float m13, float m14, float m21, float m22, float m23, float m24, float m31, float m32, float m33, float m34, float m41, float m42, float m43, float m44)
        {
            this.M11 = m11;
            this.M12 = m12;
            this.M13 = m13;
            this.M14 = m14;
            this.M21 = m21;
            this.M22 = m22;
            this.M23 = m23;
            this.M24 = m24;
            this.M31 = m31;
            this.M32 = m32;
            this.M33 = m33;
            this.M34 = m34;
            this.M41 = m41;
            this.M42 = m42;
            this.M43 = m43;
            this.M44 = m44;
        }
        public static MATRIX CreateBillboard(VECTOR3 objectPosition, VECTOR3 cameraPosition, VECTOR3 cameraUpVector, VECTOR3? cameraForwardVector)
        {
            VECTOR3 vector;
            vector.X = objectPosition.X - cameraPosition.X;
            vector.Y = objectPosition.Y - cameraPosition.Y;
            vector.Z = objectPosition.Z - cameraPosition.Z;
            float num = vector.LengthSquared();
            if (num < 0.0001f)
            {
                vector = (cameraForwardVector.HasValue ? (-cameraForwardVector.Value) : VECTOR3.Forward);
            }
            else
            {
                VECTOR3.Multiply(ref vector, 1f / (float)Math.Sqrt((double)num), out vector);
            }
            VECTOR3 vector2;
            VECTOR3.Cross(ref cameraUpVector, ref vector, out vector2);
            vector2.Normalize();
            VECTOR3 vector3;
            VECTOR3.Cross(ref vector, ref vector2, out vector3);
            MATRIX result;
            result.M11 = vector2.X;
            result.M12 = vector2.Y;
            result.M13 = vector2.Z;
            result.M14 = 0f;
            result.M21 = vector3.X;
            result.M22 = vector3.Y;
            result.M23 = vector3.Z;
            result.M24 = 0f;
            result.M31 = vector.X;
            result.M32 = vector.Y;
            result.M33 = vector.Z;
            result.M34 = 0f;
            result.M41 = objectPosition.X;
            result.M42 = objectPosition.Y;
            result.M43 = objectPosition.Z;
            result.M44 = 1f;
            return result;
        }
        public static void CreateBillboard(ref VECTOR3 objectPosition, ref VECTOR3 cameraPosition, ref VECTOR3 cameraUpVector, VECTOR3? cameraForwardVector, out MATRIX result)
        {
            VECTOR3 vector;
            vector.X = objectPosition.X - cameraPosition.X;
            vector.Y = objectPosition.Y - cameraPosition.Y;
            vector.Z = objectPosition.Z - cameraPosition.Z;
            float num = vector.LengthSquared();
            if (num < 0.0001f)
            {
                vector = (cameraForwardVector.HasValue ? (-cameraForwardVector.Value) : VECTOR3.Forward);
            }
            else
            {
                VECTOR3.Multiply(ref vector, 1f / (float)Math.Sqrt((double)num), out vector);
            }
            VECTOR3 vector2;
            VECTOR3.Cross(ref cameraUpVector, ref vector, out vector2);
            vector2.Normalize();
            VECTOR3 vector3;
            VECTOR3.Cross(ref vector, ref vector2, out vector3);
            result.M11 = vector2.X;
            result.M12 = vector2.Y;
            result.M13 = vector2.Z;
            result.M14 = 0f;
            result.M21 = vector3.X;
            result.M22 = vector3.Y;
            result.M23 = vector3.Z;
            result.M24 = 0f;
            result.M31 = vector.X;
            result.M32 = vector.Y;
            result.M33 = vector.Z;
            result.M34 = 0f;
            result.M41 = objectPosition.X;
            result.M42 = objectPosition.Y;
            result.M43 = objectPosition.Z;
            result.M44 = 1f;
        }
        public static MATRIX CreateConstrainedBillboard(VECTOR3 objectPosition, VECTOR3 cameraPosition, VECTOR3 rotateAxis, VECTOR3? cameraForwardVector, VECTOR3? objectForwardVector)
        {
            VECTOR3 vector;
            vector.X = objectPosition.X - cameraPosition.X;
            vector.Y = objectPosition.Y - cameraPosition.Y;
            vector.Z = objectPosition.Z - cameraPosition.Z;
            float num = vector.LengthSquared();
            if (num < 0.0001f)
            {
                vector = (cameraForwardVector.HasValue ? (-cameraForwardVector.Value) : VECTOR3.Forward);
            }
            else
            {
                VECTOR3.Multiply(ref vector, 1f / (float)Math.Sqrt((double)num), out vector);
            }
            VECTOR3 vector2 = rotateAxis;
            float value;
            VECTOR3.Dot(ref rotateAxis, ref vector, out value);
            VECTOR3 vector3;
            VECTOR3 vector4;
            if (Math.Abs(value) > 0.998254657f)
            {
                if (objectForwardVector.HasValue)
                {
                    vector3 = objectForwardVector.Value;
                    VECTOR3.Dot(ref rotateAxis, ref vector3, out value);
                    if (Math.Abs(value) > 0.998254657f)
                    {
                        value = rotateAxis.X * VECTOR3.Forward.X + rotateAxis.Y * VECTOR3.Forward.Y + rotateAxis.Z * VECTOR3.Forward.Z;
                        vector3 = ((Math.Abs(value) > 0.998254657f) ? VECTOR3.Right : VECTOR3.Forward);
                    }
                }
                else
                {
                    value = rotateAxis.X * VECTOR3.Forward.X + rotateAxis.Y * VECTOR3.Forward.Y + rotateAxis.Z * VECTOR3.Forward.Z;
                    vector3 = ((Math.Abs(value) > 0.998254657f) ? VECTOR3.Right : VECTOR3.Forward);
                }
                VECTOR3.Cross(ref rotateAxis, ref vector3, out vector4);
                vector4.Normalize();
                VECTOR3.Cross(ref vector4, ref rotateAxis, out vector3);
                vector3.Normalize();
            }
            else
            {
                VECTOR3.Cross(ref rotateAxis, ref vector, out vector4);
                vector4.Normalize();
                VECTOR3.Cross(ref vector4, ref vector2, out vector3);
                vector3.Normalize();
            }
            MATRIX result;
            result.M11 = vector4.X;
            result.M12 = vector4.Y;
            result.M13 = vector4.Z;
            result.M14 = 0f;
            result.M21 = vector2.X;
            result.M22 = vector2.Y;
            result.M23 = vector2.Z;
            result.M24 = 0f;
            result.M31 = vector3.X;
            result.M32 = vector3.Y;
            result.M33 = vector3.Z;
            result.M34 = 0f;
            result.M41 = objectPosition.X;
            result.M42 = objectPosition.Y;
            result.M43 = objectPosition.Z;
            result.M44 = 1f;
            return result;
        }
        public static void CreateConstrainedBillboard(ref VECTOR3 objectPosition, ref VECTOR3 cameraPosition, ref VECTOR3 rotateAxis, VECTOR3? cameraForwardVector, VECTOR3? objectForwardVector, out MATRIX result)
        {
            VECTOR3 vector;
            vector.X = objectPosition.X - cameraPosition.X;
            vector.Y = objectPosition.Y - cameraPosition.Y;
            vector.Z = objectPosition.Z - cameraPosition.Z;
            float num = vector.LengthSquared();
            if (num < 0.0001f)
            {
                vector = (cameraForwardVector.HasValue ? (-cameraForwardVector.Value) : VECTOR3.Forward);
            }
            else
            {
                VECTOR3.Multiply(ref vector, 1f / (float)Math.Sqrt((double)num), out vector);
            }
            VECTOR3 vector2 = rotateAxis;
            float value;
            VECTOR3.Dot(ref rotateAxis, ref vector, out value);
            VECTOR3 vector3;
            VECTOR3 vector4;
            if (Math.Abs(value) > 0.998254657f)
            {
                if (objectForwardVector.HasValue)
                {
                    vector3 = objectForwardVector.Value;
                    VECTOR3.Dot(ref rotateAxis, ref vector3, out value);
                    if (Math.Abs(value) > 0.998254657f)
                    {
                        value = rotateAxis.X * VECTOR3.Forward.X + rotateAxis.Y * VECTOR3.Forward.Y + rotateAxis.Z * VECTOR3.Forward.Z;
                        vector3 = ((Math.Abs(value) > 0.998254657f) ? VECTOR3.Right : VECTOR3.Forward);
                    }
                }
                else
                {
                    value = rotateAxis.X * VECTOR3.Forward.X + rotateAxis.Y * VECTOR3.Forward.Y + rotateAxis.Z * VECTOR3.Forward.Z;
                    vector3 = ((Math.Abs(value) > 0.998254657f) ? VECTOR3.Right : VECTOR3.Forward);
                }
                VECTOR3.Cross(ref rotateAxis, ref vector3, out vector4);
                vector4.Normalize();
                VECTOR3.Cross(ref vector4, ref rotateAxis, out vector3);
                vector3.Normalize();
            }
            else
            {
                VECTOR3.Cross(ref rotateAxis, ref vector, out vector4);
                vector4.Normalize();
                VECTOR3.Cross(ref vector4, ref vector2, out vector3);
                vector3.Normalize();
            }
            result.M11 = vector4.X;
            result.M12 = vector4.Y;
            result.M13 = vector4.Z;
            result.M14 = 0f;
            result.M21 = vector2.X;
            result.M22 = vector2.Y;
            result.M23 = vector2.Z;
            result.M24 = 0f;
            result.M31 = vector3.X;
            result.M32 = vector3.Y;
            result.M33 = vector3.Z;
            result.M34 = 0f;
            result.M41 = objectPosition.X;
            result.M42 = objectPosition.Y;
            result.M43 = objectPosition.Z;
            result.M44 = 1f;
        }
        public static MATRIX CreateTranslation(VECTOR3 position)
        {
            MATRIX result;
            result.M11 = 1f;
            result.M12 = 0f;
            result.M13 = 0f;
            result.M14 = 0f;
            result.M21 = 0f;
            result.M22 = 1f;
            result.M23 = 0f;
            result.M24 = 0f;
            result.M31 = 0f;
            result.M32 = 0f;
            result.M33 = 1f;
            result.M34 = 0f;
            result.M41 = position.X;
            result.M42 = position.Y;
            result.M43 = position.Z;
            result.M44 = 1f;
            return result;
        }
        public static void CreateTranslation(ref VECTOR3 position, out MATRIX result)
        {
            result.M11 = 1f;
            result.M12 = 0f;
            result.M13 = 0f;
            result.M14 = 0f;
            result.M21 = 0f;
            result.M22 = 1f;
            result.M23 = 0f;
            result.M24 = 0f;
            result.M31 = 0f;
            result.M32 = 0f;
            result.M33 = 1f;
            result.M34 = 0f;
            result.M41 = position.X;
            result.M42 = position.Y;
            result.M43 = position.Z;
            result.M44 = 1f;
        }
        public static MATRIX CreateTranslation(float xPosition, float yPosition, float zPosition)
        {
            MATRIX result;
            result.M11 = 1f;
            result.M12 = 0f;
            result.M13 = 0f;
            result.M14 = 0f;
            result.M21 = 0f;
            result.M22 = 1f;
            result.M23 = 0f;
            result.M24 = 0f;
            result.M31 = 0f;
            result.M32 = 0f;
            result.M33 = 1f;
            result.M34 = 0f;
            result.M41 = xPosition;
            result.M42 = yPosition;
            result.M43 = zPosition;
            result.M44 = 1f;
            return result;
        }
        public static void CreateTranslation(float xPosition, float yPosition, float zPosition, out MATRIX result)
        {
            result.M11 = 1f;
            result.M12 = 0f;
            result.M13 = 0f;
            result.M14 = 0f;
            result.M21 = 0f;
            result.M22 = 1f;
            result.M23 = 0f;
            result.M24 = 0f;
            result.M31 = 0f;
            result.M32 = 0f;
            result.M33 = 1f;
            result.M34 = 0f;
            result.M41 = xPosition;
            result.M42 = yPosition;
            result.M43 = zPosition;
            result.M44 = 1f;
        }
        public static MATRIX CreateScale(float xScale, float yScale, float zScale)
        {
            MATRIX result;
            result.M11 = xScale;
            result.M12 = 0f;
            result.M13 = 0f;
            result.M14 = 0f;
            result.M21 = 0f;
            result.M22 = yScale;
            result.M23 = 0f;
            result.M24 = 0f;
            result.M31 = 0f;
            result.M32 = 0f;
            result.M33 = zScale;
            result.M34 = 0f;
            result.M41 = 0f;
            result.M42 = 0f;
            result.M43 = 0f;
            result.M44 = 1f;
            return result;
        }
        public static void CreateScale(float xScale, float yScale, float zScale, out MATRIX result)
        {
            result.M11 = xScale;
            result.M12 = 0f;
            result.M13 = 0f;
            result.M14 = 0f;
            result.M21 = 0f;
            result.M22 = yScale;
            result.M23 = 0f;
            result.M24 = 0f;
            result.M31 = 0f;
            result.M32 = 0f;
            result.M33 = zScale;
            result.M34 = 0f;
            result.M41 = 0f;
            result.M42 = 0f;
            result.M43 = 0f;
            result.M44 = 1f;
        }
        public static MATRIX CreateScale(VECTOR3 scales)
        {
            float x = scales.X;
            float y = scales.Y;
            float z = scales.Z;
            MATRIX result;
            result.M11 = x;
            result.M12 = 0f;
            result.M13 = 0f;
            result.M14 = 0f;
            result.M21 = 0f;
            result.M22 = y;
            result.M23 = 0f;
            result.M24 = 0f;
            result.M31 = 0f;
            result.M32 = 0f;
            result.M33 = z;
            result.M34 = 0f;
            result.M41 = 0f;
            result.M42 = 0f;
            result.M43 = 0f;
            result.M44 = 1f;
            return result;
        }
        public static void CreateScale(ref VECTOR3 scales, out MATRIX result)
        {
            float x = scales.X;
            float y = scales.Y;
            float z = scales.Z;
            result.M11 = x;
            result.M12 = 0f;
            result.M13 = 0f;
            result.M14 = 0f;
            result.M21 = 0f;
            result.M22 = y;
            result.M23 = 0f;
            result.M24 = 0f;
            result.M31 = 0f;
            result.M32 = 0f;
            result.M33 = z;
            result.M34 = 0f;
            result.M41 = 0f;
            result.M42 = 0f;
            result.M43 = 0f;
            result.M44 = 1f;
        }
        public static MATRIX CreateScale(float scale)
        {
            MATRIX result;
            result.M11 = scale;
            result.M12 = 0f;
            result.M13 = 0f;
            result.M14 = 0f;
            result.M21 = 0f;
            result.M22 = scale;
            result.M23 = 0f;
            result.M24 = 0f;
            result.M31 = 0f;
            result.M32 = 0f;
            result.M33 = scale;
            result.M34 = 0f;
            result.M41 = 0f;
            result.M42 = 0f;
            result.M43 = 0f;
            result.M44 = 1f;
            return result;
        }
        public static void CreateScale(float scale, out MATRIX result)
        {
            result.M11 = scale;
            result.M12 = 0f;
            result.M13 = 0f;
            result.M14 = 0f;
            result.M21 = 0f;
            result.M22 = scale;
            result.M23 = 0f;
            result.M24 = 0f;
            result.M31 = 0f;
            result.M32 = 0f;
            result.M33 = scale;
            result.M34 = 0f;
            result.M41 = 0f;
            result.M42 = 0f;
            result.M43 = 0f;
            result.M44 = 1f;
        }
        public static MATRIX CreateRotationX(float radians)
        {
            float num = (float)Math.Cos((double)radians);
            float num2 = (float)Math.Sin((double)radians);
            MATRIX result;
            result.M11 = 1f;
            result.M12 = 0f;
            result.M13 = 0f;
            result.M14 = 0f;
            result.M21 = 0f;
            result.M22 = num;
            result.M23 = num2;
            result.M24 = 0f;
            result.M31 = 0f;
            result.M32 = -num2;
            result.M33 = num;
            result.M34 = 0f;
            result.M41 = 0f;
            result.M42 = 0f;
            result.M43 = 0f;
            result.M44 = 1f;
            return result;
        }
        public static void CreateRotationX(float radians, out MATRIX result)
        {
            float num = (float)Math.Cos((double)radians);
            float num2 = (float)Math.Sin((double)radians);
            result.M11 = 1f;
            result.M12 = 0f;
            result.M13 = 0f;
            result.M14 = 0f;
            result.M21 = 0f;
            result.M22 = num;
            result.M23 = num2;
            result.M24 = 0f;
            result.M31 = 0f;
            result.M32 = -num2;
            result.M33 = num;
            result.M34 = 0f;
            result.M41 = 0f;
            result.M42 = 0f;
            result.M43 = 0f;
            result.M44 = 1f;
        }
        public static MATRIX CreateRotationY(float radians)
        {
            float num = (float)Math.Cos((double)radians);
            float num2 = (float)Math.Sin((double)radians);
            MATRIX result;
            result.M11 = num;
            result.M12 = 0f;
            result.M13 = -num2;
            result.M14 = 0f;
            result.M21 = 0f;
            result.M22 = 1f;
            result.M23 = 0f;
            result.M24 = 0f;
            result.M31 = num2;
            result.M32 = 0f;
            result.M33 = num;
            result.M34 = 0f;
            result.M41 = 0f;
            result.M42 = 0f;
            result.M43 = 0f;
            result.M44 = 1f;
            return result;
        }
        public static void CreateRotationY(float radians, out MATRIX result)
        {
            float num = (float)Math.Cos((double)radians);
            float num2 = (float)Math.Sin((double)radians);
            result.M11 = num;
            result.M12 = 0f;
            result.M13 = -num2;
            result.M14 = 0f;
            result.M21 = 0f;
            result.M22 = 1f;
            result.M23 = 0f;
            result.M24 = 0f;
            result.M31 = num2;
            result.M32 = 0f;
            result.M33 = num;
            result.M34 = 0f;
            result.M41 = 0f;
            result.M42 = 0f;
            result.M43 = 0f;
            result.M44 = 1f;
        }
        public static MATRIX CreateRotationZ(float radians)
        {
            float num = (float)Math.Cos((double)radians);
            float num2 = (float)Math.Sin((double)radians);
            MATRIX result;
            result.M11 = num;
            result.M12 = num2;
            result.M13 = 0f;
            result.M14 = 0f;
            result.M21 = -num2;
            result.M22 = num;
            result.M23 = 0f;
            result.M24 = 0f;
            result.M31 = 0f;
            result.M32 = 0f;
            result.M33 = 1f;
            result.M34 = 0f;
            result.M41 = 0f;
            result.M42 = 0f;
            result.M43 = 0f;
            result.M44 = 1f;
            return result;
        }
        public static void CreateRotationZ(float radians, out MATRIX result)
        {
            float num = (float)Math.Cos((double)radians);
            float num2 = (float)Math.Sin((double)radians);
            result.M11 = num;
            result.M12 = num2;
            result.M13 = 0f;
            result.M14 = 0f;
            result.M21 = -num2;
            result.M22 = num;
            result.M23 = 0f;
            result.M24 = 0f;
            result.M31 = 0f;
            result.M32 = 0f;
            result.M33 = 1f;
            result.M34 = 0f;
            result.M41 = 0f;
            result.M42 = 0f;
            result.M43 = 0f;
            result.M44 = 1f;
        }
        public static MATRIX CreateRotationPivot(VECTOR2 pivot, float radian)
        {
            return MATRIX.CreateTranslation(-pivot.X, -pivot.Y, 0) *
                MATRIX.CreateRotationZ(radian) *
                MATRIX.CreateTranslation(pivot.X, pivot.Y, 0);
        }
        public static MATRIX CreateFromAxisAngle(VECTOR3 axis, float angle)
        {
            float x = axis.X;
            float y = axis.Y;
            float z = axis.Z;
            float num = (float)Math.Sin((double)angle);
            float num2 = (float)Math.Cos((double)angle);
            float num3 = x * x;
            float num4 = y * y;
            float num5 = z * z;
            float num6 = x * y;
            float num7 = x * z;
            float num8 = y * z;
            MATRIX result;
            result.M11 = num3 + num2 * (1f - num3);
            result.M12 = num6 - num2 * num6 + num * z;
            result.M13 = num7 - num2 * num7 - num * y;
            result.M14 = 0f;
            result.M21 = num6 - num2 * num6 - num * z;
            result.M22 = num4 + num2 * (1f - num4);
            result.M23 = num8 - num2 * num8 + num * x;
            result.M24 = 0f;
            result.M31 = num7 - num2 * num7 + num * y;
            result.M32 = num8 - num2 * num8 - num * x;
            result.M33 = num5 + num2 * (1f - num5);
            result.M34 = 0f;
            result.M41 = 0f;
            result.M42 = 0f;
            result.M43 = 0f;
            result.M44 = 1f;
            return result;
        }
        public static void CreateFromAxisAngle(ref VECTOR3 axis, float angle, out MATRIX result)
        {
            float x = axis.X;
            float y = axis.Y;
            float z = axis.Z;
            float num = (float)Math.Sin((double)angle);
            float num2 = (float)Math.Cos((double)angle);
            float num3 = x * x;
            float num4 = y * y;
            float num5 = z * z;
            float num6 = x * y;
            float num7 = x * z;
            float num8 = y * z;
            result.M11 = num3 + num2 * (1f - num3);
            result.M12 = num6 - num2 * num6 + num * z;
            result.M13 = num7 - num2 * num7 - num * y;
            result.M14 = 0f;
            result.M21 = num6 - num2 * num6 - num * z;
            result.M22 = num4 + num2 * (1f - num4);
            result.M23 = num8 - num2 * num8 + num * x;
            result.M24 = 0f;
            result.M31 = num7 - num2 * num7 + num * y;
            result.M32 = num8 - num2 * num8 - num * x;
            result.M33 = num5 + num2 * (1f - num5);
            result.M34 = 0f;
            result.M41 = 0f;
            result.M42 = 0f;
            result.M43 = 0f;
            result.M44 = 1f;
        }
        public static MATRIX CreatePerspectiveFieldOfView(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance)
        {
            if (fieldOfView <= 0f || fieldOfView >= 3.14159274f)
            {
                throw new ArgumentOutOfRangeException("fieldOfView");
            }
            if (nearPlaneDistance <= 0f)
            {
                throw new ArgumentOutOfRangeException("nearPlaneDistance");
            }
            if (farPlaneDistance <= 0f)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (nearPlaneDistance >= farPlaneDistance)
            {
                throw new ArgumentOutOfRangeException("nearPlaneDistance");
            }
            float num = 1f / (float)Math.Tan((double)(fieldOfView * 0.5f));
            float m = num / aspectRatio;
            MATRIX result;
            result.M11 = m;
            result.M12 = (result.M13 = (result.M14 = 0f));
            result.M22 = num;
            result.M21 = (result.M23 = (result.M24 = 0f));
            result.M31 = (result.M32 = 0f);
            result.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
            result.M34 = -1f;
            result.M41 = (result.M42 = (result.M44 = 0f));
            result.M43 = nearPlaneDistance * farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
            return result;
        }
        public static void CreatePerspectiveFieldOfView(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance, out MATRIX result)
        {
            if (fieldOfView <= 0f || fieldOfView >= 3.14159274f)
            {
                throw new ArgumentOutOfRangeException("fieldOfView");
            }
            if (nearPlaneDistance <= 0f)
            {
                throw new ArgumentOutOfRangeException("nearPlaneDistance");
            }
            if (farPlaneDistance <= 0f)
            {
                throw new ArgumentOutOfRangeException("farPlaneDistance");
            }
            if (nearPlaneDistance >= farPlaneDistance)
            {
                throw new ArgumentOutOfRangeException("nearPlaneDistance");
            }
            float num = 1f / (float)Math.Tan((double)(fieldOfView * 0.5f));
            float m = num / aspectRatio;
            result.M11 = m;
            result.M12 = (result.M13 = (result.M14 = 0f));
            result.M22 = num;
            result.M21 = (result.M23 = (result.M24 = 0f));
            result.M31 = (result.M32 = 0f);
            result.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
            result.M34 = -1f;
            result.M41 = (result.M42 = (result.M44 = 0f));
            result.M43 = nearPlaneDistance * farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
        }
        public static MATRIX CreatePerspective(float width, float height, float nearPlaneDistance, float farPlaneDistance)
        {
            if (nearPlaneDistance <= 0f)
            {
                throw new ArgumentOutOfRangeException("nearPlaneDistance");
            }
            if (farPlaneDistance <= 0f)
            {
                throw new ArgumentOutOfRangeException("farPlaneDistance");
            }
            if (nearPlaneDistance >= farPlaneDistance)
            {
                throw new ArgumentOutOfRangeException("nearPlaneDistance");
            }
            MATRIX result;
            result.M11 = 2f * nearPlaneDistance / width;
            result.M12 = (result.M13 = (result.M14 = 0f));
            result.M22 = 2f * nearPlaneDistance / height;
            result.M21 = (result.M23 = (result.M24 = 0f));
            result.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
            result.M31 = (result.M32 = 0f);
            result.M34 = -1f;
            result.M41 = (result.M42 = (result.M44 = 0f));
            result.M43 = nearPlaneDistance * farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
            return result;
        }
        public static void CreatePerspective(float width, float height, float nearPlaneDistance, float farPlaneDistance, out MATRIX result)
        {
            if (nearPlaneDistance <= 0f)
            {
                throw new ArgumentOutOfRangeException("nearPlaneDistance");
            }
            if (farPlaneDistance <= 0f)
            {
                throw new ArgumentOutOfRangeException("farPlaneDistance");
            }
            if (nearPlaneDistance >= farPlaneDistance)
            {
                throw new ArgumentOutOfRangeException("nearPlaneDistance");
            }
            result.M11 = 2f * nearPlaneDistance / width;
            result.M12 = (result.M13 = (result.M14 = 0f));
            result.M22 = 2f * nearPlaneDistance / height;
            result.M21 = (result.M23 = (result.M24 = 0f));
            result.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
            result.M31 = (result.M32 = 0f);
            result.M34 = -1f;
            result.M41 = (result.M42 = (result.M44 = 0f));
            result.M43 = nearPlaneDistance * farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
        }
        public static MATRIX CreatePerspectiveOffCenter(float left, float right, float bottom, float top, float nearPlaneDistance, float farPlaneDistance)
        {
            if (nearPlaneDistance <= 0f)
            {
                throw new ArgumentOutOfRangeException("nearPlaneDistance");
            }
            if (farPlaneDistance <= 0f)
            {
                throw new ArgumentOutOfRangeException("farPlaneDistance");
            }
            if (nearPlaneDistance >= farPlaneDistance)
            {
                throw new ArgumentOutOfRangeException("nearPlaneDistance");
            }
            MATRIX result;
            result.M11 = 2f * nearPlaneDistance / (right - left);
            result.M12 = (result.M13 = (result.M14 = 0f));
            result.M22 = 2f * nearPlaneDistance / (top - bottom);
            result.M21 = (result.M23 = (result.M24 = 0f));
            result.M31 = (left + right) / (right - left);
            result.M32 = (top + bottom) / (top - bottom);
            result.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
            result.M34 = -1f;
            result.M43 = nearPlaneDistance * farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
            result.M41 = (result.M42 = (result.M44 = 0f));
            return result;
        }
        public static void CreatePerspectiveOffCenter(float left, float right, float bottom, float top, float nearPlaneDistance, float farPlaneDistance, out MATRIX result)
        {
            if (nearPlaneDistance <= 0f)
            {
                throw new ArgumentOutOfRangeException("nearPlaneDistance");
            }
            if (farPlaneDistance <= 0f)
            {
                throw new ArgumentOutOfRangeException("farPlaneDistance");
            }
            if (nearPlaneDistance >= farPlaneDistance)
            {
                throw new ArgumentOutOfRangeException("nearPlaneDistance");
            }
            result.M11 = 2f * nearPlaneDistance / (right - left);
            result.M12 = (result.M13 = (result.M14 = 0f));
            result.M22 = 2f * nearPlaneDistance / (top - bottom);
            result.M21 = (result.M23 = (result.M24 = 0f));
            result.M31 = (left + right) / (right - left);
            result.M32 = (top + bottom) / (top - bottom);
            result.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
            result.M34 = -1f;
            result.M43 = nearPlaneDistance * farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
            result.M41 = (result.M42 = (result.M44 = 0f));
        }
        public static MATRIX CreateOrthographic(float width, float height, float zNearPlane, float zFarPlane)
        {
            MATRIX result;
            result.M11 = 2f / width;
            result.M12 = (result.M13 = (result.M14 = 0f));
            result.M22 = 2f / height;
            result.M21 = (result.M23 = (result.M24 = 0f));
            result.M33 = 1f / (zNearPlane - zFarPlane);
            result.M31 = (result.M32 = (result.M34 = 0f));
            result.M41 = (result.M42 = 0f);
            result.M43 = zNearPlane / (zNearPlane - zFarPlane);
            result.M44 = 1f;
            return result;
        }
        public static void CreateOrthographic(float width, float height, float zNearPlane, float zFarPlane, out MATRIX result)
        {
            result.M11 = 2f / width;
            result.M12 = (result.M13 = (result.M14 = 0f));
            result.M22 = 2f / height;
            result.M21 = (result.M23 = (result.M24 = 0f));
            result.M33 = 1f / (zNearPlane - zFarPlane);
            result.M31 = (result.M32 = (result.M34 = 0f));
            result.M41 = (result.M42 = 0f);
            result.M43 = zNearPlane / (zNearPlane - zFarPlane);
            result.M44 = 1f;
        }
        public static MATRIX CreateOrthographicOffCenter(float left, float right, float bottom, float top, float zNearPlane, float zFarPlane)
        {
            MATRIX result;
            result.M11 = 2f / (right - left);
            result.M12 = (result.M13 = (result.M14 = 0f));
            result.M22 = 2f / (top - bottom);
            result.M21 = (result.M23 = (result.M24 = 0f));
            result.M33 = 1f / (zNearPlane - zFarPlane);
            result.M31 = (result.M32 = (result.M34 = 0f));
            result.M41 = (left + right) / (left - right);
            result.M42 = (top + bottom) / (bottom - top);
            result.M43 = zNearPlane / (zNearPlane - zFarPlane);
            result.M44 = 1f;
            return result;
        }
        public static void CreateOrthographicOffCenter(float left, float right, float bottom, float top, float zNearPlane, float zFarPlane, out MATRIX result)
        {
            result.M11 = 2f / (right - left);
            result.M12 = (result.M13 = (result.M14 = 0f));
            result.M22 = 2f / (top - bottom);
            result.M21 = (result.M23 = (result.M24 = 0f));
            result.M33 = 1f / (zNearPlane - zFarPlane);
            result.M31 = (result.M32 = (result.M34 = 0f));
            result.M41 = (left + right) / (left - right);
            result.M42 = (top + bottom) / (bottom - top);
            result.M43 = zNearPlane / (zNearPlane - zFarPlane);
            result.M44 = 1f;
        }
        public static MATRIX CreateLookAt(VECTOR3 cameraPosition, VECTOR3 cameraTarget, VECTOR3 cameraUpVector)
        {
            VECTOR3 vector = VECTOR3.Normalize(cameraPosition - cameraTarget);
            VECTOR3 vector2 = VECTOR3.Normalize(VECTOR3.Cross(cameraUpVector, vector));
            VECTOR3 vector3 = VECTOR3.Cross(vector, vector2);
            MATRIX result;
            result.M11 = vector2.X;
            result.M12 = vector3.X;
            result.M13 = vector.X;
            result.M14 = 0f;
            result.M21 = vector2.Y;
            result.M22 = vector3.Y;
            result.M23 = vector.Y;
            result.M24 = 0f;
            result.M31 = vector2.Z;
            result.M32 = vector3.Z;
            result.M33 = vector.Z;
            result.M34 = 0f;
            result.M41 = -VECTOR3.Dot(vector2, cameraPosition);
            result.M42 = -VECTOR3.Dot(vector3, cameraPosition);
            result.M43 = -VECTOR3.Dot(vector, cameraPosition);
            result.M44 = 1f;
            return result;
        }
        public static void CreateLookAt(ref VECTOR3 cameraPosition, ref VECTOR3 cameraTarget, ref VECTOR3 cameraUpVector, out MATRIX result)
        {
            VECTOR3 vector = VECTOR3.Normalize(cameraPosition - cameraTarget);
            VECTOR3 vector2 = VECTOR3.Normalize(VECTOR3.Cross(cameraUpVector, vector));
            VECTOR3 vector3 = VECTOR3.Cross(vector, vector2);
            result.M11 = vector2.X;
            result.M12 = vector3.X;
            result.M13 = vector.X;
            result.M14 = 0f;
            result.M21 = vector2.Y;
            result.M22 = vector3.Y;
            result.M23 = vector.Y;
            result.M24 = 0f;
            result.M31 = vector2.Z;
            result.M32 = vector3.Z;
            result.M33 = vector.Z;
            result.M34 = 0f;
            result.M41 = -VECTOR3.Dot(vector2, cameraPosition);
            result.M42 = -VECTOR3.Dot(vector3, cameraPosition);
            result.M43 = -VECTOR3.Dot(vector, cameraPosition);
            result.M44 = 1f;
        }
        public static MATRIX CreateWorld(VECTOR3 position, VECTOR3 forward, VECTOR3 up)
        {
            VECTOR3 vector = VECTOR3.Normalize(-forward);
            VECTOR3 vector2 = VECTOR3.Normalize(VECTOR3.Cross(up, vector));
            VECTOR3 vector3 = VECTOR3.Cross(vector, vector2);
            MATRIX result;
            result.M11 = vector2.X;
            result.M12 = vector2.Y;
            result.M13 = vector2.Z;
            result.M14 = 0f;
            result.M21 = vector3.X;
            result.M22 = vector3.Y;
            result.M23 = vector3.Z;
            result.M24 = 0f;
            result.M31 = vector.X;
            result.M32 = vector.Y;
            result.M33 = vector.Z;
            result.M34 = 0f;
            result.M41 = position.X;
            result.M42 = position.Y;
            result.M43 = position.Z;
            result.M44 = 1f;
            return result;
        }
        public static void CreateWorld(ref VECTOR3 position, ref VECTOR3 forward, ref VECTOR3 up, out MATRIX result)
        {
            VECTOR3 vector = VECTOR3.Normalize(-forward);
            VECTOR3 vector2 = VECTOR3.Normalize(VECTOR3.Cross(up, vector));
            VECTOR3 vector3 = VECTOR3.Cross(vector, vector2);
            result.M11 = vector2.X;
            result.M12 = vector2.Y;
            result.M13 = vector2.Z;
            result.M14 = 0f;
            result.M21 = vector3.X;
            result.M22 = vector3.Y;
            result.M23 = vector3.Z;
            result.M24 = 0f;
            result.M31 = vector.X;
            result.M32 = vector.Y;
            result.M33 = vector.Z;
            result.M34 = 0f;
            result.M41 = position.X;
            result.M42 = position.Y;
            result.M43 = position.Z;
            result.M44 = 1f;
        }
        public static MATRIX CreateFromQuaternion(Quaternion quaternion)
        {
            float num = quaternion.X * quaternion.X;
            float num2 = quaternion.Y * quaternion.Y;
            float num3 = quaternion.Z * quaternion.Z;
            float num4 = quaternion.X * quaternion.Y;
            float num5 = quaternion.Z * quaternion.W;
            float num6 = quaternion.Z * quaternion.X;
            float num7 = quaternion.Y * quaternion.W;
            float num8 = quaternion.Y * quaternion.Z;
            float num9 = quaternion.X * quaternion.W;
            MATRIX result;
            result.M11 = 1f - 2f * (num2 + num3);
            result.M12 = 2f * (num4 + num5);
            result.M13 = 2f * (num6 - num7);
            result.M14 = 0f;
            result.M21 = 2f * (num4 - num5);
            result.M22 = 1f - 2f * (num3 + num);
            result.M23 = 2f * (num8 + num9);
            result.M24 = 0f;
            result.M31 = 2f * (num6 + num7);
            result.M32 = 2f * (num8 - num9);
            result.M33 = 1f - 2f * (num2 + num);
            result.M34 = 0f;
            result.M41 = 0f;
            result.M42 = 0f;
            result.M43 = 0f;
            result.M44 = 1f;
            return result;
        }
        public static void CreateFromQuaternion(ref Quaternion quaternion, out MATRIX result)
        {
            float num = quaternion.X * quaternion.X;
            float num2 = quaternion.Y * quaternion.Y;
            float num3 = quaternion.Z * quaternion.Z;
            float num4 = quaternion.X * quaternion.Y;
            float num5 = quaternion.Z * quaternion.W;
            float num6 = quaternion.Z * quaternion.X;
            float num7 = quaternion.Y * quaternion.W;
            float num8 = quaternion.Y * quaternion.Z;
            float num9 = quaternion.X * quaternion.W;
            result.M11 = 1f - 2f * (num2 + num3);
            result.M12 = 2f * (num4 + num5);
            result.M13 = 2f * (num6 - num7);
            result.M14 = 0f;
            result.M21 = 2f * (num4 - num5);
            result.M22 = 1f - 2f * (num3 + num);
            result.M23 = 2f * (num8 + num9);
            result.M24 = 0f;
            result.M31 = 2f * (num6 + num7);
            result.M32 = 2f * (num8 - num9);
            result.M33 = 1f - 2f * (num2 + num);
            result.M34 = 0f;
            result.M41 = 0f;
            result.M42 = 0f;
            result.M43 = 0f;
            result.M44 = 1f;
        }
        public static MATRIX CreateFromYawPitchRoll(float yaw, float pitch, float roll)
        {
            Quaternion quaternion;
            Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll, out quaternion);
            MATRIX result;
            MATRIX.CreateFromQuaternion(ref quaternion, out result);
            return result;
        }
        public static void CreateFromYawPitchRoll(float yaw, float pitch, float roll, out MATRIX result)
        {
            Quaternion quaternion;
            Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll, out quaternion);
            MATRIX.CreateFromQuaternion(ref quaternion, out result);
        }
        //public static EEMatrix CreateShadow(EEVector3 lightDirection, Plane plane)
        //{
        //    Plane plane2;
        //    Plane.Normalize(ref plane, out plane2);
        //    float num = plane2.Normal.X * lightDirection.X + plane2.Normal.Y * lightDirection.Y + plane2.Normal.Z * lightDirection.Z;
        //    float num2 = -plane2.Normal.X;
        //    float num3 = -plane2.Normal.Y;
        //    float num4 = -plane2.Normal.Z;
        //    float num5 = -plane2.D;
        //    EEMatrix result;
        //    result.M11 = num2 * lightDirection.X + num;
        //    result.M21 = num3 * lightDirection.X;
        //    result.M31 = num4 * lightDirection.X;
        //    result.M41 = num5 * lightDirection.X;
        //    result.M12 = num2 * lightDirection.Y;
        //    result.M22 = num3 * lightDirection.Y + num;
        //    result.M32 = num4 * lightDirection.Y;
        //    result.M42 = num5 * lightDirection.Y;
        //    result.M13 = num2 * lightDirection.Z;
        //    result.M23 = num3 * lightDirection.Z;
        //    result.M33 = num4 * lightDirection.Z + num;
        //    result.M43 = num5 * lightDirection.Z;
        //    result.M14 = 0f;
        //    result.M24 = 0f;
        //    result.M34 = 0f;
        //    result.M44 = num;
        //    return result;
        //}
        //public static void CreateShadow(ref EEVector3 lightDirection, ref Plane plane, out EEMatrix result)
        //{
        //    Plane plane2;
        //    Plane.Normalize(ref plane, out plane2);
        //    float num = plane2.Normal.X * lightDirection.X + plane2.Normal.Y * lightDirection.Y + plane2.Normal.Z * lightDirection.Z;
        //    float num2 = -plane2.Normal.X;
        //    float num3 = -plane2.Normal.Y;
        //    float num4 = -plane2.Normal.Z;
        //    float num5 = -plane2.D;
        //    result.M11 = num2 * lightDirection.X + num;
        //    result.M21 = num3 * lightDirection.X;
        //    result.M31 = num4 * lightDirection.X;
        //    result.M41 = num5 * lightDirection.X;
        //    result.M12 = num2 * lightDirection.Y;
        //    result.M22 = num3 * lightDirection.Y + num;
        //    result.M32 = num4 * lightDirection.Y;
        //    result.M42 = num5 * lightDirection.Y;
        //    result.M13 = num2 * lightDirection.Z;
        //    result.M23 = num3 * lightDirection.Z;
        //    result.M33 = num4 * lightDirection.Z + num;
        //    result.M43 = num5 * lightDirection.Z;
        //    result.M14 = 0f;
        //    result.M24 = 0f;
        //    result.M34 = 0f;
        //    result.M44 = num;
        //}
        //public static EEMatrix CreateReflection(Plane value)
        //{
        //    value.Normalize();
        //    float x = value.Normal.X;
        //    float y = value.Normal.Y;
        //    float z = value.Normal.Z;
        //    float num = -2f * x;
        //    float num2 = -2f * y;
        //    float num3 = -2f * z;
        //    EEMatrix result;
        //    result.M11 = num * x + 1f;
        //    result.M12 = num2 * x;
        //    result.M13 = num3 * x;
        //    result.M14 = 0f;
        //    result.M21 = num * y;
        //    result.M22 = num2 * y + 1f;
        //    result.M23 = num3 * y;
        //    result.M24 = 0f;
        //    result.M31 = num * z;
        //    result.M32 = num2 * z;
        //    result.M33 = num3 * z + 1f;
        //    result.M34 = 0f;
        //    result.M41 = num * value.D;
        //    result.M42 = num2 * value.D;
        //    result.M43 = num3 * value.D;
        //    result.M44 = 1f;
        //    return result;
        //}
        //public static void CreateReflection(ref Plane value, out EEMatrix result)
        //{
        //    Plane plane;
        //    Plane.Normalize(ref value, out plane);
        //    value.Normalize();
        //    float x = plane.Normal.X;
        //    float y = plane.Normal.Y;
        //    float z = plane.Normal.Z;
        //    float num = -2f * x;
        //    float num2 = -2f * y;
        //    float num3 = -2f * z;
        //    result.M11 = num * x + 1f;
        //    result.M12 = num2 * x;
        //    result.M13 = num3 * x;
        //    result.M14 = 0f;
        //    result.M21 = num * y;
        //    result.M22 = num2 * y + 1f;
        //    result.M23 = num3 * y;
        //    result.M24 = 0f;
        //    result.M31 = num * z;
        //    result.M32 = num2 * z;
        //    result.M33 = num3 * z + 1f;
        //    result.M34 = 0f;
        //    result.M41 = num * plane.D;
        //    result.M42 = num2 * plane.D;
        //    result.M43 = num3 * plane.D;
        //    result.M44 = 1f;
        //}
        //public unsafe bool Decompose(out EEVector3 scale, out Quaternion rotation, out EEVector3 translation)
        //{
        //    bool result = true;
        //    fixed (float* ptr = &scale.X)
        //    {
        //        EEMatrix.VectorBasis vectorBasis;
        //        EEVector3** ptr2 = (EEVector3**)(&vectorBasis);
        //        EEMatrix identity = EEMatrix.Identity;
        //        EEMatrix.CanonicalBasis canonicalBasis = default(EEMatrix.CanonicalBasis);
        //        EEVector3* ptr3 = &canonicalBasis.Row0;
        //        canonicalBasis.Row0 = new EEVector3(1f, 0f, 0f);
        //        canonicalBasis.Row1 = new EEVector3(0f, 1f, 0f);
        //        canonicalBasis.Row2 = new EEVector3(0f, 0f, 1f);
        //        translation.X = this.M41;
        //        translation.Y = this.M42;
        //        translation.Z = this.M43;
        //        *(IntPtr*)ptr2 = &identity.M11;
        //        *(IntPtr*)(ptr2 + (IntPtr)sizeof(EEVector3*) / sizeof(EEVector3*)) = &identity.M21;
        //        *(IntPtr*)(ptr2 + (IntPtr)2 * (IntPtr)sizeof(EEVector3*) / sizeof(EEVector3*)) = &identity.M31;
        //        *(*(IntPtr*)ptr2) = new EEVector3(this.M11, this.M12, this.M13);
        //        *(*(IntPtr*)(ptr2 + (IntPtr)sizeof(EEVector3*) / sizeof(EEVector3*))) = new EEVector3(this.M21, this.M22, this.M23);
        //        *(*(IntPtr*)(ptr2 + (IntPtr)2 * (IntPtr)sizeof(EEVector3*) / sizeof(EEVector3*))) = new EEVector3(this.M31, this.M32, this.M33);
        //        scale.X = ((IntPtr*)ptr2)->Length();
        //        scale.Y = ((IntPtr*)(ptr2 + (IntPtr)sizeof(EEVector3*) / sizeof(EEVector3*)))->Length();
        //        scale.Z = ((IntPtr*)(ptr2 + (IntPtr)2 * (IntPtr)sizeof(EEVector3*) / sizeof(EEVector3*)))->Length();
        //        float num = *ptr;
        //        float num2 = ptr[(IntPtr)4 / 4];
        //        float num3 = ptr[(IntPtr)8 / 4];
        //        uint num4;
        //        uint num5;
        //        uint num6;
        //        if (num < num2)
        //        {
        //            if (num2 < num3)
        //            {
        //                num4 = 2u;
        //                num5 = 1u;
        //                num6 = 0u;
        //            }
        //            else
        //            {
        //                num4 = 1u;
        //                if (num < num3)
        //                {
        //                    num5 = 2u;
        //                    num6 = 0u;
        //                }
        //                else
        //                {
        //                    num5 = 0u;
        //                    num6 = 2u;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (num < num3)
        //            {
        //                num4 = 2u;
        //                num5 = 0u;
        //                num6 = 1u;
        //            }
        //            else
        //            {
        //                num4 = 0u;
        //                if (num2 < num3)
        //                {
        //                    num5 = 2u;
        //                    num6 = 1u;
        //                }
        //                else
        //                {
        //                    num5 = 1u;
        //                    num6 = 2u;
        //                }
        //            }
        //        }
        //        if (ptr[(IntPtr)((UIntPtr)num4 * 4) / 4] < 0.0001f)
        //        {
        //            *(*(IntPtr*)(ptr2 + (IntPtr)((ulong)num4 * (ulong)((long)sizeof(EEVector3*))) / sizeof(EEVector3*))) = ptr3[(IntPtr)((ulong)num4 * (ulong)((long)sizeof(EEVector3))) / sizeof(EEVector3)];
        //        }
        //        ((IntPtr*)(ptr2 + (IntPtr)((ulong)num4 * (ulong)((long)sizeof(EEVector3*))) / sizeof(EEVector3*)))->Normalize();
        //        if (ptr[(IntPtr)((UIntPtr)num5 * 4) / 4] < 0.0001f)
        //        {
        //            float num7 = Math.Abs(((IntPtr*)(ptr2 + (IntPtr)((ulong)num4 * (ulong)((long)sizeof(EEVector3*))) / sizeof(EEVector3*)))->X);
        //            float num8 = Math.Abs(((IntPtr*)(ptr2 + (IntPtr)((ulong)num4 * (ulong)((long)sizeof(EEVector3*))) / sizeof(EEVector3*)))->Y);
        //            float num9 = Math.Abs(((IntPtr*)(ptr2 + (IntPtr)((ulong)num4 * (ulong)((long)sizeof(EEVector3*))) / sizeof(EEVector3*)))->Z);
        //            uint num10;
        //            if (num7 < num8)
        //            {
        //                if (num8 < num9)
        //                {
        //                    num10 = 0u;
        //                }
        //                else
        //                {
        //                    if (num7 < num9)
        //                    {
        //                        num10 = 0u;
        //                    }
        //                    else
        //                    {
        //                        num10 = 2u;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                if (num7 < num9)
        //                {
        //                    num10 = 1u;
        //                }
        //                else
        //                {
        //                    if (num8 < num9)
        //                    {
        //                        num10 = 1u;
        //                    }
        //                    else
        //                    {
        //                        num10 = 2u;
        //                    }
        //                }
        //            }
        //            EEVector3.Cross(*(IntPtr*)(ptr2 + (IntPtr)((ulong)num5 * (ulong)((long)sizeof(EEVector3*))) / sizeof(EEVector3*)), *(IntPtr*)(ptr2 + (IntPtr)((ulong)num4 * (ulong)((long)sizeof(EEVector3*))) / sizeof(EEVector3*)), out ptr3[(IntPtr)((ulong)num10 * (ulong)((long)sizeof(EEVector3))) / sizeof(EEVector3)]);
        //        }
        //        ((IntPtr*)(ptr2 + (IntPtr)((ulong)num5 * (ulong)((long)sizeof(EEVector3*))) / sizeof(EEVector3*)))->Normalize();
        //        if (ptr[(IntPtr)((UIntPtr)num6 * 4) / 4] < 0.0001f)
        //        {
        //            EEVector3.Cross(*(IntPtr*)(ptr2 + (IntPtr)((ulong)num6 * (ulong)((long)sizeof(EEVector3*))) / sizeof(EEVector3*)), *(IntPtr*)(ptr2 + (IntPtr)((ulong)num4 * (ulong)((long)sizeof(EEVector3*))) / sizeof(EEVector3*)), *(IntPtr*)(ptr2 + (IntPtr)((ulong)num5 * (ulong)((long)sizeof(EEVector3*))) / sizeof(EEVector3*)));
        //        }
        //        ((IntPtr*)(ptr2 + (IntPtr)((ulong)num6 * (ulong)((long)sizeof(EEVector3*))) / sizeof(EEVector3*)))->Normalize();
        //        float num11 = identity.Determinant();
        //        if (num11 < 0f)
        //        {
        //            ptr[(IntPtr)((UIntPtr)num4 * 4) / 4] = -(*(ptr + (IntPtr)((UIntPtr)num4 * 4) / 4));
        //            *(*(IntPtr*)(ptr2 + (IntPtr)((ulong)num4 * (ulong)((long)sizeof(EEVector3*))) / sizeof(EEVector3*))) = -(*(*(IntPtr*)(ptr2 + (IntPtr)((ulong)num4 * (ulong)((long)sizeof(EEVector3*))) / sizeof(EEVector3*))));
        //            num11 = -num11;
        //        }
        //        num11 -= 1f;
        //        num11 *= num11;
        //        if (0.0001f < num11)
        //        {
        //            rotation = Quaternion.Identity;
        //            result = false;
        //        }
        //        else
        //        {
        //            Quaternion.CreateFromRotationEEMatrix(ref identity, out rotation);
        //        }
        //    }
        //    return result;
        //}
        //public static EEMatrix Transform(EEMatrix value, Quaternion rotation)
        //{
        //    float num = rotation.X + rotation.X;
        //    float num2 = rotation.Y + rotation.Y;
        //    float num3 = rotation.Z + rotation.Z;
        //    float num4 = rotation.W * num;
        //    float num5 = rotation.W * num2;
        //    float num6 = rotation.W * num3;
        //    float num7 = rotation.X * num;
        //    float num8 = rotation.X * num2;
        //    float num9 = rotation.X * num3;
        //    float num10 = rotation.Y * num2;
        //    float num11 = rotation.Y * num3;
        //    float num12 = rotation.Z * num3;
        //    float num13 = 1f - num10 - num12;
        //    float num14 = num8 - num6;
        //    float num15 = num9 + num5;
        //    float num16 = num8 + num6;
        //    float num17 = 1f - num7 - num12;
        //    float num18 = num11 - num4;
        //    float num19 = num9 - num5;
        //    float num20 = num11 + num4;
        //    float num21 = 1f - num7 - num10;
        //    EEMatrix result;
        //    result.M11 = value.M11 * num13 + value.M12 * num14 + value.M13 * num15;
        //    result.M12 = value.M11 * num16 + value.M12 * num17 + value.M13 * num18;
        //    result.M13 = value.M11 * num19 + value.M12 * num20 + value.M13 * num21;
        //    result.M14 = value.M14;
        //    result.M21 = value.M21 * num13 + value.M22 * num14 + value.M23 * num15;
        //    result.M22 = value.M21 * num16 + value.M22 * num17 + value.M23 * num18;
        //    result.M23 = value.M21 * num19 + value.M22 * num20 + value.M23 * num21;
        //    result.M24 = value.M24;
        //    result.M31 = value.M31 * num13 + value.M32 * num14 + value.M33 * num15;
        //    result.M32 = value.M31 * num16 + value.M32 * num17 + value.M33 * num18;
        //    result.M33 = value.M31 * num19 + value.M32 * num20 + value.M33 * num21;
        //    result.M34 = value.M34;
        //    result.M41 = value.M41 * num13 + value.M42 * num14 + value.M43 * num15;
        //    result.M42 = value.M41 * num16 + value.M42 * num17 + value.M43 * num18;
        //    result.M43 = value.M41 * num19 + value.M42 * num20 + value.M43 * num21;
        //    result.M44 = value.M44;
        //    return result;
        //}
        //public static void Transform(ref EEMatrix value, ref Quaternion rotation, out EEMatrix result)
        //{
        //    float num = rotation.X + rotation.X;
        //    float num2 = rotation.Y + rotation.Y;
        //    float num3 = rotation.Z + rotation.Z;
        //    float num4 = rotation.W * num;
        //    float num5 = rotation.W * num2;
        //    float num6 = rotation.W * num3;
        //    float num7 = rotation.X * num;
        //    float num8 = rotation.X * num2;
        //    float num9 = rotation.X * num3;
        //    float num10 = rotation.Y * num2;
        //    float num11 = rotation.Y * num3;
        //    float num12 = rotation.Z * num3;
        //    float num13 = 1f - num10 - num12;
        //    float num14 = num8 - num6;
        //    float num15 = num9 + num5;
        //    float num16 = num8 + num6;
        //    float num17 = 1f - num7 - num12;
        //    float num18 = num11 - num4;
        //    float num19 = num9 - num5;
        //    float num20 = num11 + num4;
        //    float num21 = 1f - num7 - num10;
        //    float m = value.M11 * num13 + value.M12 * num14 + value.M13 * num15;
        //    float m2 = value.M11 * num16 + value.M12 * num17 + value.M13 * num18;
        //    float m3 = value.M11 * num19 + value.M12 * num20 + value.M13 * num21;
        //    float m4 = value.M14;
        //    float m5 = value.M21 * num13 + value.M22 * num14 + value.M23 * num15;
        //    float m6 = value.M21 * num16 + value.M22 * num17 + value.M23 * num18;
        //    float m7 = value.M21 * num19 + value.M22 * num20 + value.M23 * num21;
        //    float m8 = value.M24;
        //    float m9 = value.M31 * num13 + value.M32 * num14 + value.M33 * num15;
        //    float m10 = value.M31 * num16 + value.M32 * num17 + value.M33 * num18;
        //    float m11 = value.M31 * num19 + value.M32 * num20 + value.M33 * num21;
        //    float m12 = value.M34;
        //    float m13 = value.M41 * num13 + value.M42 * num14 + value.M43 * num15;
        //    float m14 = value.M41 * num16 + value.M42 * num17 + value.M43 * num18;
        //    float m15 = value.M41 * num19 + value.M42 * num20 + value.M43 * num21;
        //    float m16 = value.M44;
        //    result.M11 = m;
        //    result.M12 = m2;
        //    result.M13 = m3;
        //    result.M14 = m4;
        //    result.M21 = m5;
        //    result.M22 = m6;
        //    result.M23 = m7;
        //    result.M24 = m8;
        //    result.M31 = m9;
        //    result.M32 = m10;
        //    result.M33 = m11;
        //    result.M34 = m12;
        //    result.M41 = m13;
        //    result.M42 = m14;
        //    result.M43 = m15;
        //    result.M44 = m16;
        //}
        public override string ToString()
        {
            return string.Format("{{M11:{0} M12:{1} M13:{2} M14:{3}}} {{M11:{4} M12:{5} M13:{6} M14:{7}}} {{M11:{8} M12:{9} M13:{10} M14:{11}}} {{M11:{12} M12:{13} M13:{14} M14:{15}}}"
                , M11, M12, M13, M14, M21, M22, M23, M24, M31, M32, M33, M34, M41, M42, M43, M44);
        }
        public bool Equals(MATRIX other)
        {
            return this.M11 == other.M11 && this.M22 == other.M22 && this.M33 == other.M33 && this.M44 == other.M44 && this.M12 == other.M12 && this.M13 == other.M13 && this.M14 == other.M14 && this.M21 == other.M21 && this.M23 == other.M23 && this.M24 == other.M24 && this.M31 == other.M31 && this.M32 == other.M32 && this.M34 == other.M34 && this.M41 == other.M41 && this.M42 == other.M42 && this.M43 == other.M43;
        }
        public override bool Equals(object obj)
        {
            bool result = false;
            if (obj is MATRIX)
            {
                result = this.Equals((MATRIX)obj);
            }
            return result;
        }
        public override int GetHashCode()
        {
            return this.M11.GetHashCode() + this.M12.GetHashCode() + this.M13.GetHashCode() + this.M14.GetHashCode() + this.M21.GetHashCode() + this.M22.GetHashCode() + this.M23.GetHashCode() + this.M24.GetHashCode() + this.M31.GetHashCode() + this.M32.GetHashCode() + this.M33.GetHashCode() + this.M34.GetHashCode() + this.M41.GetHashCode() + this.M42.GetHashCode() + this.M43.GetHashCode() + this.M44.GetHashCode();
        }
        public static MATRIX Transpose(MATRIX matrix)
        {
            MATRIX result;
            result.M11 = matrix.M11;
            result.M12 = matrix.M21;
            result.M13 = matrix.M31;
            result.M14 = matrix.M41;
            result.M21 = matrix.M12;
            result.M22 = matrix.M22;
            result.M23 = matrix.M32;
            result.M24 = matrix.M42;
            result.M31 = matrix.M13;
            result.M32 = matrix.M23;
            result.M33 = matrix.M33;
            result.M34 = matrix.M43;
            result.M41 = matrix.M14;
            result.M42 = matrix.M24;
            result.M43 = matrix.M34;
            result.M44 = matrix.M44;
            return result;
        }
        public static void Transpose(ref MATRIX matrix, out MATRIX result)
        {
            float m = matrix.M11;
            float m2 = matrix.M12;
            float m3 = matrix.M13;
            float m4 = matrix.M14;
            float m5 = matrix.M21;
            float m6 = matrix.M22;
            float m7 = matrix.M23;
            float m8 = matrix.M24;
            float m9 = matrix.M31;
            float m10 = matrix.M32;
            float m11 = matrix.M33;
            float m12 = matrix.M34;
            float m13 = matrix.M41;
            float m14 = matrix.M42;
            float m15 = matrix.M43;
            float m16 = matrix.M44;
            result.M11 = m;
            result.M12 = m5;
            result.M13 = m9;
            result.M14 = m13;
            result.M21 = m2;
            result.M22 = m6;
            result.M23 = m10;
            result.M24 = m14;
            result.M31 = m3;
            result.M32 = m7;
            result.M33 = m11;
            result.M34 = m15;
            result.M41 = m4;
            result.M42 = m8;
            result.M43 = m12;
            result.M44 = m16;
        }
        public float Determinant()
        {
            float m = this.M11;
            float m2 = this.M12;
            float m3 = this.M13;
            float m4 = this.M14;
            float m5 = this.M21;
            float m6 = this.M22;
            float m7 = this.M23;
            float m8 = this.M24;
            float m9 = this.M31;
            float m10 = this.M32;
            float m11 = this.M33;
            float m12 = this.M34;
            float m13 = this.M41;
            float m14 = this.M42;
            float m15 = this.M43;
            float m16 = this.M44;
            float num = m11 * m16 - m12 * m15;
            float num2 = m10 * m16 - m12 * m14;
            float num3 = m10 * m15 - m11 * m14;
            float num4 = m9 * m16 - m12 * m13;
            float num5 = m9 * m15 - m11 * m13;
            float num6 = m9 * m14 - m10 * m13;
            return m * (m6 * num - m7 * num2 + m8 * num3) - m2 * (m5 * num - m7 * num4 + m8 * num5) + m3 * (m5 * num2 - m6 * num4 + m8 * num6) - m4 * (m5 * num3 - m6 * num5 + m7 * num6);
        }
        public MATRIX Inverse()
        {
            return Invert(this);
        }
        public static MATRIX Invert(MATRIX matrix)
        {
            float m = matrix.M11;
            float m2 = matrix.M12;
            float m3 = matrix.M13;
            float m4 = matrix.M14;
            float m5 = matrix.M21;
            float m6 = matrix.M22;
            float m7 = matrix.M23;
            float m8 = matrix.M24;
            float m9 = matrix.M31;
            float m10 = matrix.M32;
            float m11 = matrix.M33;
            float m12 = matrix.M34;
            float m13 = matrix.M41;
            float m14 = matrix.M42;
            float m15 = matrix.M43;
            float m16 = matrix.M44;
            float num = m11 * m16 - m12 * m15;
            float num2 = m10 * m16 - m12 * m14;
            float num3 = m10 * m15 - m11 * m14;
            float num4 = m9 * m16 - m12 * m13;
            float num5 = m9 * m15 - m11 * m13;
            float num6 = m9 * m14 - m10 * m13;
            float num7 = m6 * num - m7 * num2 + m8 * num3;
            float num8 = -(m5 * num - m7 * num4 + m8 * num5);
            float num9 = m5 * num2 - m6 * num4 + m8 * num6;
            float num10 = -(m5 * num3 - m6 * num5 + m7 * num6);
            float num11 = 1f / (m * num7 + m2 * num8 + m3 * num9 + m4 * num10);
            MATRIX result;
            result.M11 = num7 * num11;
            result.M21 = num8 * num11;
            result.M31 = num9 * num11;
            result.M41 = num10 * num11;
            result.M12 = -(m2 * num - m3 * num2 + m4 * num3) * num11;
            result.M22 = (m * num - m3 * num4 + m4 * num5) * num11;
            result.M32 = -(m * num2 - m2 * num4 + m4 * num6) * num11;
            result.M42 = (m * num3 - m2 * num5 + m3 * num6) * num11;
            float num12 = m7 * m16 - m8 * m15;
            float num13 = m6 * m16 - m8 * m14;
            float num14 = m6 * m15 - m7 * m14;
            float num15 = m5 * m16 - m8 * m13;
            float num16 = m5 * m15 - m7 * m13;
            float num17 = m5 * m14 - m6 * m13;
            result.M13 = (m2 * num12 - m3 * num13 + m4 * num14) * num11;
            result.M23 = -(m * num12 - m3 * num15 + m4 * num16) * num11;
            result.M33 = (m * num13 - m2 * num15 + m4 * num17) * num11;
            result.M43 = -(m * num14 - m2 * num16 + m3 * num17) * num11;
            float num18 = m7 * m12 - m8 * m11;
            float num19 = m6 * m12 - m8 * m10;
            float num20 = m6 * m11 - m7 * m10;
            float num21 = m5 * m12 - m8 * m9;
            float num22 = m5 * m11 - m7 * m9;
            float num23 = m5 * m10 - m6 * m9;
            result.M14 = -(m2 * num18 - m3 * num19 + m4 * num20) * num11;
            result.M24 = (m * num18 - m3 * num21 + m4 * num22) * num11;
            result.M34 = -(m * num19 - m2 * num21 + m4 * num23) * num11;
            result.M44 = (m * num20 - m2 * num22 + m3 * num23) * num11;
            return result;
        }
        public static void Invert(ref MATRIX matrix, out MATRIX result)
        {
            float m = matrix.M11;
            float m2 = matrix.M12;
            float m3 = matrix.M13;
            float m4 = matrix.M14;
            float m5 = matrix.M21;
            float m6 = matrix.M22;
            float m7 = matrix.M23;
            float m8 = matrix.M24;
            float m9 = matrix.M31;
            float m10 = matrix.M32;
            float m11 = matrix.M33;
            float m12 = matrix.M34;
            float m13 = matrix.M41;
            float m14 = matrix.M42;
            float m15 = matrix.M43;
            float m16 = matrix.M44;
            float num = m11 * m16 - m12 * m15;
            float num2 = m10 * m16 - m12 * m14;
            float num3 = m10 * m15 - m11 * m14;
            float num4 = m9 * m16 - m12 * m13;
            float num5 = m9 * m15 - m11 * m13;
            float num6 = m9 * m14 - m10 * m13;
            float num7 = m6 * num - m7 * num2 + m8 * num3;
            float num8 = -(m5 * num - m7 * num4 + m8 * num5);
            float num9 = m5 * num2 - m6 * num4 + m8 * num6;
            float num10 = -(m5 * num3 - m6 * num5 + m7 * num6);
            float num11 = 1f / (m * num7 + m2 * num8 + m3 * num9 + m4 * num10);
            result.M11 = num7 * num11;
            result.M21 = num8 * num11;
            result.M31 = num9 * num11;
            result.M41 = num10 * num11;
            result.M12 = -(m2 * num - m3 * num2 + m4 * num3) * num11;
            result.M22 = (m * num - m3 * num4 + m4 * num5) * num11;
            result.M32 = -(m * num2 - m2 * num4 + m4 * num6) * num11;
            result.M42 = (m * num3 - m2 * num5 + m3 * num6) * num11;
            float num12 = m7 * m16 - m8 * m15;
            float num13 = m6 * m16 - m8 * m14;
            float num14 = m6 * m15 - m7 * m14;
            float num15 = m5 * m16 - m8 * m13;
            float num16 = m5 * m15 - m7 * m13;
            float num17 = m5 * m14 - m6 * m13;
            result.M13 = (m2 * num12 - m3 * num13 + m4 * num14) * num11;
            result.M23 = -(m * num12 - m3 * num15 + m4 * num16) * num11;
            result.M33 = (m * num13 - m2 * num15 + m4 * num17) * num11;
            result.M43 = -(m * num14 - m2 * num16 + m3 * num17) * num11;
            float num18 = m7 * m12 - m8 * m11;
            float num19 = m6 * m12 - m8 * m10;
            float num20 = m6 * m11 - m7 * m10;
            float num21 = m5 * m12 - m8 * m9;
            float num22 = m5 * m11 - m7 * m9;
            float num23 = m5 * m10 - m6 * m9;
            result.M14 = -(m2 * num18 - m3 * num19 + m4 * num20) * num11;
            result.M24 = (m * num18 - m3 * num21 + m4 * num22) * num11;
            result.M34 = -(m * num19 - m2 * num21 + m4 * num23) * num11;
            result.M44 = (m * num20 - m2 * num22 + m3 * num23) * num11;
        }
        public static MATRIX Lerp(MATRIX matrix1, MATRIX matrix2, float amount)
        {
            MATRIX result;
            result.M11 = matrix1.M11 + (matrix2.M11 - matrix1.M11) * amount;
            result.M12 = matrix1.M12 + (matrix2.M12 - matrix1.M12) * amount;
            result.M13 = matrix1.M13 + (matrix2.M13 - matrix1.M13) * amount;
            result.M14 = matrix1.M14 + (matrix2.M14 - matrix1.M14) * amount;
            result.M21 = matrix1.M21 + (matrix2.M21 - matrix1.M21) * amount;
            result.M22 = matrix1.M22 + (matrix2.M22 - matrix1.M22) * amount;
            result.M23 = matrix1.M23 + (matrix2.M23 - matrix1.M23) * amount;
            result.M24 = matrix1.M24 + (matrix2.M24 - matrix1.M24) * amount;
            result.M31 = matrix1.M31 + (matrix2.M31 - matrix1.M31) * amount;
            result.M32 = matrix1.M32 + (matrix2.M32 - matrix1.M32) * amount;
            result.M33 = matrix1.M33 + (matrix2.M33 - matrix1.M33) * amount;
            result.M34 = matrix1.M34 + (matrix2.M34 - matrix1.M34) * amount;
            result.M41 = matrix1.M41 + (matrix2.M41 - matrix1.M41) * amount;
            result.M42 = matrix1.M42 + (matrix2.M42 - matrix1.M42) * amount;
            result.M43 = matrix1.M43 + (matrix2.M43 - matrix1.M43) * amount;
            result.M44 = matrix1.M44 + (matrix2.M44 - matrix1.M44) * amount;
            return result;
        }
        public static void Lerp(ref MATRIX matrix1, ref MATRIX matrix2, float amount, out MATRIX result)
        {
            result.M11 = matrix1.M11 + (matrix2.M11 - matrix1.M11) * amount;
            result.M12 = matrix1.M12 + (matrix2.M12 - matrix1.M12) * amount;
            result.M13 = matrix1.M13 + (matrix2.M13 - matrix1.M13) * amount;
            result.M14 = matrix1.M14 + (matrix2.M14 - matrix1.M14) * amount;
            result.M21 = matrix1.M21 + (matrix2.M21 - matrix1.M21) * amount;
            result.M22 = matrix1.M22 + (matrix2.M22 - matrix1.M22) * amount;
            result.M23 = matrix1.M23 + (matrix2.M23 - matrix1.M23) * amount;
            result.M24 = matrix1.M24 + (matrix2.M24 - matrix1.M24) * amount;
            result.M31 = matrix1.M31 + (matrix2.M31 - matrix1.M31) * amount;
            result.M32 = matrix1.M32 + (matrix2.M32 - matrix1.M32) * amount;
            result.M33 = matrix1.M33 + (matrix2.M33 - matrix1.M33) * amount;
            result.M34 = matrix1.M34 + (matrix2.M34 - matrix1.M34) * amount;
            result.M41 = matrix1.M41 + (matrix2.M41 - matrix1.M41) * amount;
            result.M42 = matrix1.M42 + (matrix2.M42 - matrix1.M42) * amount;
            result.M43 = matrix1.M43 + (matrix2.M43 - matrix1.M43) * amount;
            result.M44 = matrix1.M44 + (matrix2.M44 - matrix1.M44) * amount;
        }
        public static MATRIX Negate(MATRIX matrix)
        {
            MATRIX result;
            result.M11 = -matrix.M11;
            result.M12 = -matrix.M12;
            result.M13 = -matrix.M13;
            result.M14 = -matrix.M14;
            result.M21 = -matrix.M21;
            result.M22 = -matrix.M22;
            result.M23 = -matrix.M23;
            result.M24 = -matrix.M24;
            result.M31 = -matrix.M31;
            result.M32 = -matrix.M32;
            result.M33 = -matrix.M33;
            result.M34 = -matrix.M34;
            result.M41 = -matrix.M41;
            result.M42 = -matrix.M42;
            result.M43 = -matrix.M43;
            result.M44 = -matrix.M44;
            return result;
        }
        public static void Negate(ref MATRIX matrix, out MATRIX result)
        {
            result.M11 = -matrix.M11;
            result.M12 = -matrix.M12;
            result.M13 = -matrix.M13;
            result.M14 = -matrix.M14;
            result.M21 = -matrix.M21;
            result.M22 = -matrix.M22;
            result.M23 = -matrix.M23;
            result.M24 = -matrix.M24;
            result.M31 = -matrix.M31;
            result.M32 = -matrix.M32;
            result.M33 = -matrix.M33;
            result.M34 = -matrix.M34;
            result.M41 = -matrix.M41;
            result.M42 = -matrix.M42;
            result.M43 = -matrix.M43;
            result.M44 = -matrix.M44;
        }
        public static MATRIX Add(MATRIX matrix1, MATRIX matrix2)
        {
            MATRIX result;
            result.M11 = matrix1.M11 + matrix2.M11;
            result.M12 = matrix1.M12 + matrix2.M12;
            result.M13 = matrix1.M13 + matrix2.M13;
            result.M14 = matrix1.M14 + matrix2.M14;
            result.M21 = matrix1.M21 + matrix2.M21;
            result.M22 = matrix1.M22 + matrix2.M22;
            result.M23 = matrix1.M23 + matrix2.M23;
            result.M24 = matrix1.M24 + matrix2.M24;
            result.M31 = matrix1.M31 + matrix2.M31;
            result.M32 = matrix1.M32 + matrix2.M32;
            result.M33 = matrix1.M33 + matrix2.M33;
            result.M34 = matrix1.M34 + matrix2.M34;
            result.M41 = matrix1.M41 + matrix2.M41;
            result.M42 = matrix1.M42 + matrix2.M42;
            result.M43 = matrix1.M43 + matrix2.M43;
            result.M44 = matrix1.M44 + matrix2.M44;
            return result;
        }
        public static void Add(ref MATRIX matrix1, ref MATRIX matrix2, out MATRIX result)
        {
            result.M11 = matrix1.M11 + matrix2.M11;
            result.M12 = matrix1.M12 + matrix2.M12;
            result.M13 = matrix1.M13 + matrix2.M13;
            result.M14 = matrix1.M14 + matrix2.M14;
            result.M21 = matrix1.M21 + matrix2.M21;
            result.M22 = matrix1.M22 + matrix2.M22;
            result.M23 = matrix1.M23 + matrix2.M23;
            result.M24 = matrix1.M24 + matrix2.M24;
            result.M31 = matrix1.M31 + matrix2.M31;
            result.M32 = matrix1.M32 + matrix2.M32;
            result.M33 = matrix1.M33 + matrix2.M33;
            result.M34 = matrix1.M34 + matrix2.M34;
            result.M41 = matrix1.M41 + matrix2.M41;
            result.M42 = matrix1.M42 + matrix2.M42;
            result.M43 = matrix1.M43 + matrix2.M43;
            result.M44 = matrix1.M44 + matrix2.M44;
        }
        public static MATRIX Subtract(MATRIX matrix1, MATRIX matrix2)
        {
            MATRIX result;
            result.M11 = matrix1.M11 - matrix2.M11;
            result.M12 = matrix1.M12 - matrix2.M12;
            result.M13 = matrix1.M13 - matrix2.M13;
            result.M14 = matrix1.M14 - matrix2.M14;
            result.M21 = matrix1.M21 - matrix2.M21;
            result.M22 = matrix1.M22 - matrix2.M22;
            result.M23 = matrix1.M23 - matrix2.M23;
            result.M24 = matrix1.M24 - matrix2.M24;
            result.M31 = matrix1.M31 - matrix2.M31;
            result.M32 = matrix1.M32 - matrix2.M32;
            result.M33 = matrix1.M33 - matrix2.M33;
            result.M34 = matrix1.M34 - matrix2.M34;
            result.M41 = matrix1.M41 - matrix2.M41;
            result.M42 = matrix1.M42 - matrix2.M42;
            result.M43 = matrix1.M43 - matrix2.M43;
            result.M44 = matrix1.M44 - matrix2.M44;
            return result;
        }
        public static void Subtract(ref MATRIX matrix1, ref MATRIX matrix2, out MATRIX result)
        {
            result.M11 = matrix1.M11 - matrix2.M11;
            result.M12 = matrix1.M12 - matrix2.M12;
            result.M13 = matrix1.M13 - matrix2.M13;
            result.M14 = matrix1.M14 - matrix2.M14;
            result.M21 = matrix1.M21 - matrix2.M21;
            result.M22 = matrix1.M22 - matrix2.M22;
            result.M23 = matrix1.M23 - matrix2.M23;
            result.M24 = matrix1.M24 - matrix2.M24;
            result.M31 = matrix1.M31 - matrix2.M31;
            result.M32 = matrix1.M32 - matrix2.M32;
            result.M33 = matrix1.M33 - matrix2.M33;
            result.M34 = matrix1.M34 - matrix2.M34;
            result.M41 = matrix1.M41 - matrix2.M41;
            result.M42 = matrix1.M42 - matrix2.M42;
            result.M43 = matrix1.M43 - matrix2.M43;
            result.M44 = matrix1.M44 - matrix2.M44;
        }
        public static MATRIX Multiply(MATRIX matrix1, MATRIX matrix2)
        {
            MATRIX result;
            result.M11 = matrix1.M11 * matrix2.M11 + matrix1.M12 * matrix2.M21 + matrix1.M13 * matrix2.M31 + matrix1.M14 * matrix2.M41;
            result.M12 = matrix1.M11 * matrix2.M12 + matrix1.M12 * matrix2.M22 + matrix1.M13 * matrix2.M32 + matrix1.M14 * matrix2.M42;
            result.M13 = matrix1.M11 * matrix2.M13 + matrix1.M12 * matrix2.M23 + matrix1.M13 * matrix2.M33 + matrix1.M14 * matrix2.M43;
            result.M14 = matrix1.M11 * matrix2.M14 + matrix1.M12 * matrix2.M24 + matrix1.M13 * matrix2.M34 + matrix1.M14 * matrix2.M44;
            result.M21 = matrix1.M21 * matrix2.M11 + matrix1.M22 * matrix2.M21 + matrix1.M23 * matrix2.M31 + matrix1.M24 * matrix2.M41;
            result.M22 = matrix1.M21 * matrix2.M12 + matrix1.M22 * matrix2.M22 + matrix1.M23 * matrix2.M32 + matrix1.M24 * matrix2.M42;
            result.M23 = matrix1.M21 * matrix2.M13 + matrix1.M22 * matrix2.M23 + matrix1.M23 * matrix2.M33 + matrix1.M24 * matrix2.M43;
            result.M24 = matrix1.M21 * matrix2.M14 + matrix1.M22 * matrix2.M24 + matrix1.M23 * matrix2.M34 + matrix1.M24 * matrix2.M44;
            result.M31 = matrix1.M31 * matrix2.M11 + matrix1.M32 * matrix2.M21 + matrix1.M33 * matrix2.M31 + matrix1.M34 * matrix2.M41;
            result.M32 = matrix1.M31 * matrix2.M12 + matrix1.M32 * matrix2.M22 + matrix1.M33 * matrix2.M32 + matrix1.M34 * matrix2.M42;
            result.M33 = matrix1.M31 * matrix2.M13 + matrix1.M32 * matrix2.M23 + matrix1.M33 * matrix2.M33 + matrix1.M34 * matrix2.M43;
            result.M34 = matrix1.M31 * matrix2.M14 + matrix1.M32 * matrix2.M24 + matrix1.M33 * matrix2.M34 + matrix1.M34 * matrix2.M44;
            result.M41 = matrix1.M41 * matrix2.M11 + matrix1.M42 * matrix2.M21 + matrix1.M43 * matrix2.M31 + matrix1.M44 * matrix2.M41;
            result.M42 = matrix1.M41 * matrix2.M12 + matrix1.M42 * matrix2.M22 + matrix1.M43 * matrix2.M32 + matrix1.M44 * matrix2.M42;
            result.M43 = matrix1.M41 * matrix2.M13 + matrix1.M42 * matrix2.M23 + matrix1.M43 * matrix2.M33 + matrix1.M44 * matrix2.M43;
            result.M44 = matrix1.M41 * matrix2.M14 + matrix1.M42 * matrix2.M24 + matrix1.M43 * matrix2.M34 + matrix1.M44 * matrix2.M44;
            return result;
        }
        public static void Multiply(ref MATRIX matrix1, ref MATRIX matrix2, out MATRIX result)
        {
            float m = matrix1.M11 * matrix2.M11 + matrix1.M12 * matrix2.M21 + matrix1.M13 * matrix2.M31 + matrix1.M14 * matrix2.M41;
            float m2 = matrix1.M11 * matrix2.M12 + matrix1.M12 * matrix2.M22 + matrix1.M13 * matrix2.M32 + matrix1.M14 * matrix2.M42;
            float m3 = matrix1.M11 * matrix2.M13 + matrix1.M12 * matrix2.M23 + matrix1.M13 * matrix2.M33 + matrix1.M14 * matrix2.M43;
            float m4 = matrix1.M11 * matrix2.M14 + matrix1.M12 * matrix2.M24 + matrix1.M13 * matrix2.M34 + matrix1.M14 * matrix2.M44;
            float m5 = matrix1.M21 * matrix2.M11 + matrix1.M22 * matrix2.M21 + matrix1.M23 * matrix2.M31 + matrix1.M24 * matrix2.M41;
            float m6 = matrix1.M21 * matrix2.M12 + matrix1.M22 * matrix2.M22 + matrix1.M23 * matrix2.M32 + matrix1.M24 * matrix2.M42;
            float m7 = matrix1.M21 * matrix2.M13 + matrix1.M22 * matrix2.M23 + matrix1.M23 * matrix2.M33 + matrix1.M24 * matrix2.M43;
            float m8 = matrix1.M21 * matrix2.M14 + matrix1.M22 * matrix2.M24 + matrix1.M23 * matrix2.M34 + matrix1.M24 * matrix2.M44;
            float m9 = matrix1.M31 * matrix2.M11 + matrix1.M32 * matrix2.M21 + matrix1.M33 * matrix2.M31 + matrix1.M34 * matrix2.M41;
            float m10 = matrix1.M31 * matrix2.M12 + matrix1.M32 * matrix2.M22 + matrix1.M33 * matrix2.M32 + matrix1.M34 * matrix2.M42;
            float m11 = matrix1.M31 * matrix2.M13 + matrix1.M32 * matrix2.M23 + matrix1.M33 * matrix2.M33 + matrix1.M34 * matrix2.M43;
            float m12 = matrix1.M31 * matrix2.M14 + matrix1.M32 * matrix2.M24 + matrix1.M33 * matrix2.M34 + matrix1.M34 * matrix2.M44;
            float m13 = matrix1.M41 * matrix2.M11 + matrix1.M42 * matrix2.M21 + matrix1.M43 * matrix2.M31 + matrix1.M44 * matrix2.M41;
            float m14 = matrix1.M41 * matrix2.M12 + matrix1.M42 * matrix2.M22 + matrix1.M43 * matrix2.M32 + matrix1.M44 * matrix2.M42;
            float m15 = matrix1.M41 * matrix2.M13 + matrix1.M42 * matrix2.M23 + matrix1.M43 * matrix2.M33 + matrix1.M44 * matrix2.M43;
            float m16 = matrix1.M41 * matrix2.M14 + matrix1.M42 * matrix2.M24 + matrix1.M43 * matrix2.M34 + matrix1.M44 * matrix2.M44;
            result.M11 = m;
            result.M12 = m2;
            result.M13 = m3;
            result.M14 = m4;
            result.M21 = m5;
            result.M22 = m6;
            result.M23 = m7;
            result.M24 = m8;
            result.M31 = m9;
            result.M32 = m10;
            result.M33 = m11;
            result.M34 = m12;
            result.M41 = m13;
            result.M42 = m14;
            result.M43 = m15;
            result.M44 = m16;
        }
        public static MATRIX Multiply(MATRIX matrix1, float scaleFactor)
        {
            MATRIX result;
            result.M11 = matrix1.M11 * scaleFactor;
            result.M12 = matrix1.M12 * scaleFactor;
            result.M13 = matrix1.M13 * scaleFactor;
            result.M14 = matrix1.M14 * scaleFactor;
            result.M21 = matrix1.M21 * scaleFactor;
            result.M22 = matrix1.M22 * scaleFactor;
            result.M23 = matrix1.M23 * scaleFactor;
            result.M24 = matrix1.M24 * scaleFactor;
            result.M31 = matrix1.M31 * scaleFactor;
            result.M32 = matrix1.M32 * scaleFactor;
            result.M33 = matrix1.M33 * scaleFactor;
            result.M34 = matrix1.M34 * scaleFactor;
            result.M41 = matrix1.M41 * scaleFactor;
            result.M42 = matrix1.M42 * scaleFactor;
            result.M43 = matrix1.M43 * scaleFactor;
            result.M44 = matrix1.M44 * scaleFactor;
            return result;
        }
        public static void Multiply(ref MATRIX matrix1, float scaleFactor, out MATRIX result)
        {
            result.M11 = matrix1.M11 * scaleFactor;
            result.M12 = matrix1.M12 * scaleFactor;
            result.M13 = matrix1.M13 * scaleFactor;
            result.M14 = matrix1.M14 * scaleFactor;
            result.M21 = matrix1.M21 * scaleFactor;
            result.M22 = matrix1.M22 * scaleFactor;
            result.M23 = matrix1.M23 * scaleFactor;
            result.M24 = matrix1.M24 * scaleFactor;
            result.M31 = matrix1.M31 * scaleFactor;
            result.M32 = matrix1.M32 * scaleFactor;
            result.M33 = matrix1.M33 * scaleFactor;
            result.M34 = matrix1.M34 * scaleFactor;
            result.M41 = matrix1.M41 * scaleFactor;
            result.M42 = matrix1.M42 * scaleFactor;
            result.M43 = matrix1.M43 * scaleFactor;
            result.M44 = matrix1.M44 * scaleFactor;
        }
        public static MATRIX Divide(MATRIX matrix1, MATRIX matrix2)
        {
            MATRIX result;
            result.M11 = matrix1.M11 / matrix2.M11;
            result.M12 = matrix1.M12 / matrix2.M12;
            result.M13 = matrix1.M13 / matrix2.M13;
            result.M14 = matrix1.M14 / matrix2.M14;
            result.M21 = matrix1.M21 / matrix2.M21;
            result.M22 = matrix1.M22 / matrix2.M22;
            result.M23 = matrix1.M23 / matrix2.M23;
            result.M24 = matrix1.M24 / matrix2.M24;
            result.M31 = matrix1.M31 / matrix2.M31;
            result.M32 = matrix1.M32 / matrix2.M32;
            result.M33 = matrix1.M33 / matrix2.M33;
            result.M34 = matrix1.M34 / matrix2.M34;
            result.M41 = matrix1.M41 / matrix2.M41;
            result.M42 = matrix1.M42 / matrix2.M42;
            result.M43 = matrix1.M43 / matrix2.M43;
            result.M44 = matrix1.M44 / matrix2.M44;
            return result;
        }
        public static void Divide(ref MATRIX matrix1, ref MATRIX matrix2, out MATRIX result)
        {
            result.M11 = matrix1.M11 / matrix2.M11;
            result.M12 = matrix1.M12 / matrix2.M12;
            result.M13 = matrix1.M13 / matrix2.M13;
            result.M14 = matrix1.M14 / matrix2.M14;
            result.M21 = matrix1.M21 / matrix2.M21;
            result.M22 = matrix1.M22 / matrix2.M22;
            result.M23 = matrix1.M23 / matrix2.M23;
            result.M24 = matrix1.M24 / matrix2.M24;
            result.M31 = matrix1.M31 / matrix2.M31;
            result.M32 = matrix1.M32 / matrix2.M32;
            result.M33 = matrix1.M33 / matrix2.M33;
            result.M34 = matrix1.M34 / matrix2.M34;
            result.M41 = matrix1.M41 / matrix2.M41;
            result.M42 = matrix1.M42 / matrix2.M42;
            result.M43 = matrix1.M43 / matrix2.M43;
            result.M44 = matrix1.M44 / matrix2.M44;
        }
        public static MATRIX Divide(MATRIX matrix1, float divider)
        {
            float num = 1f / divider;
            MATRIX result;
            result.M11 = matrix1.M11 * num;
            result.M12 = matrix1.M12 * num;
            result.M13 = matrix1.M13 * num;
            result.M14 = matrix1.M14 * num;
            result.M21 = matrix1.M21 * num;
            result.M22 = matrix1.M22 * num;
            result.M23 = matrix1.M23 * num;
            result.M24 = matrix1.M24 * num;
            result.M31 = matrix1.M31 * num;
            result.M32 = matrix1.M32 * num;
            result.M33 = matrix1.M33 * num;
            result.M34 = matrix1.M34 * num;
            result.M41 = matrix1.M41 * num;
            result.M42 = matrix1.M42 * num;
            result.M43 = matrix1.M43 * num;
            result.M44 = matrix1.M44 * num;
            return result;
        }
        public static void Divide(ref MATRIX matrix1, float divider, out MATRIX result)
        {
            float num = 1f / divider;
            result.M11 = matrix1.M11 * num;
            result.M12 = matrix1.M12 * num;
            result.M13 = matrix1.M13 * num;
            result.M14 = matrix1.M14 * num;
            result.M21 = matrix1.M21 * num;
            result.M22 = matrix1.M22 * num;
            result.M23 = matrix1.M23 * num;
            result.M24 = matrix1.M24 * num;
            result.M31 = matrix1.M31 * num;
            result.M32 = matrix1.M32 * num;
            result.M33 = matrix1.M33 * num;
            result.M34 = matrix1.M34 * num;
            result.M41 = matrix1.M41 * num;
            result.M42 = matrix1.M42 * num;
            result.M43 = matrix1.M43 * num;
            result.M44 = matrix1.M44 * num;
        }
        public static MATRIX operator -(MATRIX matrix1)
        {
            MATRIX result;
            result.M11 = -matrix1.M11;
            result.M12 = -matrix1.M12;
            result.M13 = -matrix1.M13;
            result.M14 = -matrix1.M14;
            result.M21 = -matrix1.M21;
            result.M22 = -matrix1.M22;
            result.M23 = -matrix1.M23;
            result.M24 = -matrix1.M24;
            result.M31 = -matrix1.M31;
            result.M32 = -matrix1.M32;
            result.M33 = -matrix1.M33;
            result.M34 = -matrix1.M34;
            result.M41 = -matrix1.M41;
            result.M42 = -matrix1.M42;
            result.M43 = -matrix1.M43;
            result.M44 = -matrix1.M44;
            return result;
        }
        public static bool operator ==(MATRIX matrix1, MATRIX matrix2)
        {
            return matrix1.M11 == matrix2.M11 && matrix1.M22 == matrix2.M22 && matrix1.M33 == matrix2.M33 && matrix1.M44 == matrix2.M44 && matrix1.M12 == matrix2.M12 && matrix1.M13 == matrix2.M13 && matrix1.M14 == matrix2.M14 && matrix1.M21 == matrix2.M21 && matrix1.M23 == matrix2.M23 && matrix1.M24 == matrix2.M24 && matrix1.M31 == matrix2.M31 && matrix1.M32 == matrix2.M32 && matrix1.M34 == matrix2.M34 && matrix1.M41 == matrix2.M41 && matrix1.M42 == matrix2.M42 && matrix1.M43 == matrix2.M43;
        }
        public static bool operator !=(MATRIX matrix1, MATRIX matrix2)
        {
            return matrix1.M11 != matrix2.M11 || matrix1.M12 != matrix2.M12 || matrix1.M13 != matrix2.M13 || matrix1.M14 != matrix2.M14 || matrix1.M21 != matrix2.M21 || matrix1.M22 != matrix2.M22 || matrix1.M23 != matrix2.M23 || matrix1.M24 != matrix2.M24 || matrix1.M31 != matrix2.M31 || matrix1.M32 != matrix2.M32 || matrix1.M33 != matrix2.M33 || matrix1.M34 != matrix2.M34 || matrix1.M41 != matrix2.M41 || matrix1.M42 != matrix2.M42 || matrix1.M43 != matrix2.M43 || matrix1.M44 != matrix2.M44;
        }
        public static MATRIX operator +(MATRIX matrix1, MATRIX matrix2)
        {
            MATRIX result;
            result.M11 = matrix1.M11 + matrix2.M11;
            result.M12 = matrix1.M12 + matrix2.M12;
            result.M13 = matrix1.M13 + matrix2.M13;
            result.M14 = matrix1.M14 + matrix2.M14;
            result.M21 = matrix1.M21 + matrix2.M21;
            result.M22 = matrix1.M22 + matrix2.M22;
            result.M23 = matrix1.M23 + matrix2.M23;
            result.M24 = matrix1.M24 + matrix2.M24;
            result.M31 = matrix1.M31 + matrix2.M31;
            result.M32 = matrix1.M32 + matrix2.M32;
            result.M33 = matrix1.M33 + matrix2.M33;
            result.M34 = matrix1.M34 + matrix2.M34;
            result.M41 = matrix1.M41 + matrix2.M41;
            result.M42 = matrix1.M42 + matrix2.M42;
            result.M43 = matrix1.M43 + matrix2.M43;
            result.M44 = matrix1.M44 + matrix2.M44;
            return result;
        }
        public static MATRIX operator -(MATRIX matrix1, MATRIX matrix2)
        {
            MATRIX result;
            result.M11 = matrix1.M11 - matrix2.M11;
            result.M12 = matrix1.M12 - matrix2.M12;
            result.M13 = matrix1.M13 - matrix2.M13;
            result.M14 = matrix1.M14 - matrix2.M14;
            result.M21 = matrix1.M21 - matrix2.M21;
            result.M22 = matrix1.M22 - matrix2.M22;
            result.M23 = matrix1.M23 - matrix2.M23;
            result.M24 = matrix1.M24 - matrix2.M24;
            result.M31 = matrix1.M31 - matrix2.M31;
            result.M32 = matrix1.M32 - matrix2.M32;
            result.M33 = matrix1.M33 - matrix2.M33;
            result.M34 = matrix1.M34 - matrix2.M34;
            result.M41 = matrix1.M41 - matrix2.M41;
            result.M42 = matrix1.M42 - matrix2.M42;
            result.M43 = matrix1.M43 - matrix2.M43;
            result.M44 = matrix1.M44 - matrix2.M44;
            return result;
        }
        public static MATRIX operator *(MATRIX matrix1, MATRIX matrix2)
        {
            MATRIX result;
            result.M11 = matrix1.M11 * matrix2.M11 + matrix1.M12 * matrix2.M21 + matrix1.M13 * matrix2.M31 + matrix1.M14 * matrix2.M41;
            result.M12 = matrix1.M11 * matrix2.M12 + matrix1.M12 * matrix2.M22 + matrix1.M13 * matrix2.M32 + matrix1.M14 * matrix2.M42;
            result.M13 = matrix1.M11 * matrix2.M13 + matrix1.M12 * matrix2.M23 + matrix1.M13 * matrix2.M33 + matrix1.M14 * matrix2.M43;
            result.M14 = matrix1.M11 * matrix2.M14 + matrix1.M12 * matrix2.M24 + matrix1.M13 * matrix2.M34 + matrix1.M14 * matrix2.M44;
            result.M21 = matrix1.M21 * matrix2.M11 + matrix1.M22 * matrix2.M21 + matrix1.M23 * matrix2.M31 + matrix1.M24 * matrix2.M41;
            result.M22 = matrix1.M21 * matrix2.M12 + matrix1.M22 * matrix2.M22 + matrix1.M23 * matrix2.M32 + matrix1.M24 * matrix2.M42;
            result.M23 = matrix1.M21 * matrix2.M13 + matrix1.M22 * matrix2.M23 + matrix1.M23 * matrix2.M33 + matrix1.M24 * matrix2.M43;
            result.M24 = matrix1.M21 * matrix2.M14 + matrix1.M22 * matrix2.M24 + matrix1.M23 * matrix2.M34 + matrix1.M24 * matrix2.M44;
            result.M31 = matrix1.M31 * matrix2.M11 + matrix1.M32 * matrix2.M21 + matrix1.M33 * matrix2.M31 + matrix1.M34 * matrix2.M41;
            result.M32 = matrix1.M31 * matrix2.M12 + matrix1.M32 * matrix2.M22 + matrix1.M33 * matrix2.M32 + matrix1.M34 * matrix2.M42;
            result.M33 = matrix1.M31 * matrix2.M13 + matrix1.M32 * matrix2.M23 + matrix1.M33 * matrix2.M33 + matrix1.M34 * matrix2.M43;
            result.M34 = matrix1.M31 * matrix2.M14 + matrix1.M32 * matrix2.M24 + matrix1.M33 * matrix2.M34 + matrix1.M34 * matrix2.M44;
            result.M41 = matrix1.M41 * matrix2.M11 + matrix1.M42 * matrix2.M21 + matrix1.M43 * matrix2.M31 + matrix1.M44 * matrix2.M41;
            result.M42 = matrix1.M41 * matrix2.M12 + matrix1.M42 * matrix2.M22 + matrix1.M43 * matrix2.M32 + matrix1.M44 * matrix2.M42;
            result.M43 = matrix1.M41 * matrix2.M13 + matrix1.M42 * matrix2.M23 + matrix1.M43 * matrix2.M33 + matrix1.M44 * matrix2.M43;
            result.M44 = matrix1.M41 * matrix2.M14 + matrix1.M42 * matrix2.M24 + matrix1.M43 * matrix2.M34 + matrix1.M44 * matrix2.M44;
            return result;
        }
        public static MATRIX operator *(MATRIX matrix, float scaleFactor)
        {
            MATRIX result;
            result.M11 = matrix.M11 * scaleFactor;
            result.M12 = matrix.M12 * scaleFactor;
            result.M13 = matrix.M13 * scaleFactor;
            result.M14 = matrix.M14 * scaleFactor;
            result.M21 = matrix.M21 * scaleFactor;
            result.M22 = matrix.M22 * scaleFactor;
            result.M23 = matrix.M23 * scaleFactor;
            result.M24 = matrix.M24 * scaleFactor;
            result.M31 = matrix.M31 * scaleFactor;
            result.M32 = matrix.M32 * scaleFactor;
            result.M33 = matrix.M33 * scaleFactor;
            result.M34 = matrix.M34 * scaleFactor;
            result.M41 = matrix.M41 * scaleFactor;
            result.M42 = matrix.M42 * scaleFactor;
            result.M43 = matrix.M43 * scaleFactor;
            result.M44 = matrix.M44 * scaleFactor;
            return result;
        }
        public static MATRIX operator *(float scaleFactor, MATRIX matrix)
        {
            MATRIX result;
            result.M11 = matrix.M11 * scaleFactor;
            result.M12 = matrix.M12 * scaleFactor;
            result.M13 = matrix.M13 * scaleFactor;
            result.M14 = matrix.M14 * scaleFactor;
            result.M21 = matrix.M21 * scaleFactor;
            result.M22 = matrix.M22 * scaleFactor;
            result.M23 = matrix.M23 * scaleFactor;
            result.M24 = matrix.M24 * scaleFactor;
            result.M31 = matrix.M31 * scaleFactor;
            result.M32 = matrix.M32 * scaleFactor;
            result.M33 = matrix.M33 * scaleFactor;
            result.M34 = matrix.M34 * scaleFactor;
            result.M41 = matrix.M41 * scaleFactor;
            result.M42 = matrix.M42 * scaleFactor;
            result.M43 = matrix.M43 * scaleFactor;
            result.M44 = matrix.M44 * scaleFactor;
            return result;
        }
        public static MATRIX operator /(MATRIX matrix1, MATRIX matrix2)
        {
            MATRIX result;
            result.M11 = matrix1.M11 / matrix2.M11;
            result.M12 = matrix1.M12 / matrix2.M12;
            result.M13 = matrix1.M13 / matrix2.M13;
            result.M14 = matrix1.M14 / matrix2.M14;
            result.M21 = matrix1.M21 / matrix2.M21;
            result.M22 = matrix1.M22 / matrix2.M22;
            result.M23 = matrix1.M23 / matrix2.M23;
            result.M24 = matrix1.M24 / matrix2.M24;
            result.M31 = matrix1.M31 / matrix2.M31;
            result.M32 = matrix1.M32 / matrix2.M32;
            result.M33 = matrix1.M33 / matrix2.M33;
            result.M34 = matrix1.M34 / matrix2.M34;
            result.M41 = matrix1.M41 / matrix2.M41;
            result.M42 = matrix1.M42 / matrix2.M42;
            result.M43 = matrix1.M43 / matrix2.M43;
            result.M44 = matrix1.M44 / matrix2.M44;
            return result;
        }
        public static MATRIX operator /(MATRIX matrix1, float divider)
        {
            float num = 1f / divider;
            MATRIX result;
            result.M11 = matrix1.M11 * num;
            result.M12 = matrix1.M12 * num;
            result.M13 = matrix1.M13 * num;
            result.M14 = matrix1.M14 * num;
            result.M21 = matrix1.M21 * num;
            result.M22 = matrix1.M22 * num;
            result.M23 = matrix1.M23 * num;
            result.M24 = matrix1.M24 * num;
            result.M31 = matrix1.M31 * num;
            result.M32 = matrix1.M32 * num;
            result.M33 = matrix1.M33 * num;
            result.M34 = matrix1.M34 * num;
            result.M41 = matrix1.M41 * num;
            result.M42 = matrix1.M42 * num;
            result.M43 = matrix1.M43 * num;
            result.M44 = matrix1.M44 * num;
            return result;
        }
    }
    /// <summary>二维矩阵变换</summary>
    public struct MATRIX2x3 : IEquatable<MATRIX2x3>
    {
		public static byte ACCURACY = 3;
        private static MATRIX2x3 _identity = new MATRIX2x3(1f, 0f, 0f, 1f, 0f, 0f);
        public static MATRIX2x3 Identity
        {
            get
            {
                return MATRIX2x3._identity;
            }
        }

        public float M11;
        public float M12;
        public float M21;
        public float M22;
        public float M31;
        public float M32;
        
        public VECTOR2 Up
        {
            get
            {
                VECTOR2 result;
                result.X = this.M21;
                result.Y = this.M22;
                return result;
            }
            set
            {
                this.M21 = value.X;
                this.M22 = value.Y;
            }
        }
        public VECTOR2 Down
        {
            get
            {
                VECTOR2 result;
                result.X = -this.M21;
                result.Y = -this.M22;
                return result;
            }
            set
            {
                this.M21 = -value.X;
                this.M22 = -value.Y;
            }
        }
        public VECTOR2 Right
        {
            get
            {
                VECTOR2 result;
                result.X = this.M11;
                result.Y = this.M12;
                return result;
            }
            set
            {
                this.M11 = value.X;
                this.M12 = value.Y;
            }
        }
        public VECTOR2 Left
        {
            get
            {
                VECTOR2 result;
                result.X = -this.M11;
                result.Y = -this.M12;
                return result;
            }
            set
            {
                this.M11 = -value.X;
                this.M12 = -value.Y;
            }
        }
        public VECTOR2 Translation
        {
            get
            {
                VECTOR2 result;
                result.X = this.M31;
                result.Y = this.M32;
                return result;
            }
            set
            {
                this.M31 = value.X;
                this.M32 = value.Y;
            }
        }
        public VECTOR2 Scale
        {
            get
            {
                VECTOR2 result;
                result.X = this.M11;
                result.Y = this.M22;
                return result;
            }
            set
            {
                this.M11 = value.X;
                this.M22 = value.Y;
            }
        }

        public MATRIX2x3(float m11, float m12, float m21, float m22, float m31, float m32)
        {
            this.M11 = m11;
            this.M12 = m12;
            this.M21 = m21;
            this.M22 = m22;
            this.M31 = m31;
            this.M32 = m32;
        }

		public bool IsIdentity()
		{
			return M11 == 1 && M12 == 0 && M21 == 0 && M22 == 1 && M31 == 0 && M32 == 0;
		}
        //public void Trim()
        //{
        //    M11 = (float)Math.Round(M11, ACCURACY);
        //    M12 = (float)Math.Round(M12, ACCURACY);
        //    M21 = (float)Math.Round(M21, ACCURACY);
        //    M22 = (float)Math.Round(M22, ACCURACY);
        //    M31 = (float)Math.Round(M31, ACCURACY);
        //    M32 = (float)Math.Round(M32, ACCURACY);
        //}
        public float Determinant()
        {
            return M11 * M22 - M21 * M12;
        }
        public bool Equals(MATRIX2x3 other)
        {
            return this.M11 == other.M11 && this.M12 == other.M12 &&
                this.M21 == other.M21 && this.M22 == other.M22 &&
                this.M31 == other.M31 && this.M32 == other.M32;
        }
        public override bool Equals(object obj)
        {
            bool result = false;
            if (obj is MATRIX2x3)
            {
                result = this.Equals((MATRIX2x3)obj);
            }
            return result;
        }
        public override int GetHashCode()
        {
            return this.M11.GetHashCode() + this.M12.GetHashCode() + this.M21.GetHashCode() + this.M22.GetHashCode() + this.M31.GetHashCode() + this.M32.GetHashCode();
        }
        public override string ToString()
        {
            return string.Format("M11:{0} M12:{1} M21:{2} M22:{3} M31:{4} M32:{5}", M11, M12, M21, M22, M31, M32);
        }

		public static MATRIX2x3 CreateTransform(float radians, ref VECTOR2 scale, ref VECTOR2 translation)
		{
			return CreateTransform(radians, scale.X, scale.Y, translation.X, translation.Y);
		}
		public static MATRIX2x3 CreateTransform(float radians, float scaleX, float scaleY, float offsetX, float offsetY)
		{
			MATRIX2x3 result;
			CreateTransform(radians, scaleX, scaleY, offsetX, offsetY, out result);
			return result;
		}
		public static void CreateTransform(float radians, float scaleX, float scaleY, float offsetX, float offsetY, out MATRIX2x3 result)
		{
            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);
			result.M11 = cos * scaleX;
			result.M12 = sin * scaleY;
			result.M21 = -sin * scaleX;
			result.M22 = cos * scaleY;
			result.M31 = offsetX;
			result.M32 = offsetY;
		}
		public static MATRIX2x3 CreateTransform(float radians, VECTOR2 pivot, VECTOR2 scale, VECTOR2 offset)
		{
			MATRIX2x3 result;
			CreateTransform(radians, pivot.X, pivot.Y, scale.X, scale.Y, offset.X, offset.Y, out result);
			return result;
		}
		public static MATRIX2x3 CreateTransform(float radians, float pivotX, float pivotY, float scaleX, float scaleY, float offsetX, float offsetY)
		{
			MATRIX2x3 result;
			CreateTransform(radians, pivotX, pivotY, scaleX, scaleY, offsetX, offsetY, out result);
			return result;
		}
		public static void CreateTransform(float radians, ref VECTOR2 pivot, ref VECTOR2 scale, ref VECTOR2 offset, out MATRIX2x3 result)
		{
			CreateTransform(radians, pivot.X, pivot.Y, scale.X, scale.Y, offset.X, offset.Y, out result);
		}
		public static void CreateTransform(float radians, float pivotX, float pivotY, float scaleX, float scaleY, float offsetX, float offsetY, out MATRIX2x3 result)
		{
			result = MATRIX2x3.CreateTranslation(-pivotX, -pivotY)
				* MATRIX2x3.CreateScale(scaleX, scaleY)
				* MATRIX2x3.CreateRotation(radians)
                * MATRIX2x3.CreateTranslation(offsetX, offsetY)
                //* Matrix2x3.CreateTranslation(pivotX, pivotY)
				;
		}
        public static MATRIX2x3 CreateTranslation(float x, float y)
        {
            MATRIX2x3 result;
            result.M11 = 1;
            result.M12 = 0;
            result.M21 = 0;
            result.M22 = 1;
            result.M31 = x;
            result.M32 = y;
            return result;
        }
		public static void CreateTranslation(float x, float y, out MATRIX2x3 result)
		{
			result.M11 = 1;
			result.M12 = 0;
			result.M21 = 0;
			result.M22 = 1;
			result.M31 = x;
			result.M32 = y;
		}
        public static MATRIX2x3 CreateScale(float x, float y)
        {
            MATRIX2x3 result;
            result.M11 = x;
            result.M12 = 0;
            result.M21 = 0;
            result.M22 = y;
            result.M31 = 0;
            result.M32 = 0;
            return result;
        }
        public static void CreateScale(float x, float y, out MATRIX2x3 result)
        {
            result.M11 = x;
            result.M12 = 0;
            result.M21 = 0;
            result.M22 = y;
            result.M31 = 0;
            result.M32 = 0;
        }
        public static MATRIX2x3 CreateRotation(float radians)
        {
            MATRIX2x3 result;
            CreateRotation(radians, out result);
            return result;
        }
        public static MATRIX2x3 CreateRotationA(float angle)
        {
            MATRIX2x3 result;
            CreateRotation(_MATH.ToRadian(angle), out result);
            return result;
        }
		public static MATRIX2x3 CreateRotation(float radians, float pivotX, float pivotY)
		{
			MATRIX2x3 result;
			CreateRotation(radians, pivotX, pivotY, out result);
			return result;
		}
		public static MATRIX2x3 CreateRotationA(float angle, float pivotX, float pivotY)
		{
			MATRIX2x3 result;
			CreateRotation(_MATH.ToRadian(angle), pivotX, pivotY, out result);
			return result;
		}
        public static MATRIX2x3 CreateRotationStay(float radians, float pivotX, float pivotY)
        {
            MATRIX2x3 result;
            CreateRotationStay(radians, pivotX, pivotY, out result);
            return result;
        }
        public static MATRIX2x3 CreateRotationAStay(float angle, float pivotX, float pivotY)
        {
            MATRIX2x3 result;
            CreateRotationStay(_MATH.ToRadian(angle), pivotX, pivotY, out result);
            return result;
        }
        public static void CreateRotation(float radians, out MATRIX2x3 result)
        {
            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);
            result.M11 = cos;
            result.M12 = sin;
            result.M21 = -sin;
            result.M22 = cos;
            result.M31 = 0;
            result.M32 = 0;
        }
        public static void CreateRotationA(float angle, out MATRIX2x3 result)
        {
            CreateRotation(_MATH.ToRadian(angle), out result);
        }
		public static void CreateRotation(float radians, float pivotX, float pivotY, out MATRIX2x3 result)
		{
			float m31 = -pivotX;
			float m32 = -pivotY;
            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);
			result.M11 = cos;
			result.M12 = sin;
			result.M21 = -sin;
			result.M22 = cos;
			result.M31 = m31 * result.M11 + m32 * result.M21;
			result.M32 = m31 * result.M12 + m32 * result.M22;
		}
		public static void CreateRotationA(float angle, float pivotX, float pivotY, out MATRIX2x3 result)
		{
			CreateRotation(_MATH.ToRadian(angle), pivotX, pivotY, out result);
		}
        public static void CreateRotationStay(float radians, float pivotX, float pivotY, out MATRIX2x3 result)
        {
            CreateRotation(radians, pivotX, pivotY, out result);
            result.M31 += pivotX;
            result.M32 += pivotY;
        }
        public static void CreateRotationAStay(float angle, float pivotX, float pivotY, out MATRIX2x3 result)
        {
            CreateRotationAStay(_MATH.ToRadian(angle), pivotX, pivotY, out result);
        }
        public static void Transpose(ref MATRIX2x3 matrix, out MATRIX2x3 result)
        {
            result.M11 = matrix.M11;
            result.M12 = matrix.M21;
            result.M21 = matrix.M12;
            result.M22 = matrix.M22;
            result.M31 = matrix.M31;
            result.M32 = matrix.M32;
        }
        public static MATRIX2x3 Invert(MATRIX2x3 matrix)
        {
            MATRIX2x3 result;
            Invert(ref matrix, out result);
            return result;
        }
        public static void Invert(ref MATRIX2x3 matrix, out MATRIX2x3 result)
        {
            float m11 = matrix.M11;
            float m12 = matrix.M12;
            float m21 = matrix.M21;
            float m22 = matrix.M22;
            float m31 = matrix.M31;
            float m32 = matrix.M32;

            float d = 1 / (m11 * m22 - m21 * m12);

            result.M11 = m22 * d;
            result.M12 = -m12 * d;
            result.M21 = -m21 * d;
            result.M22 = m11 * d;
            result.M31 = (m21 * m32 - m22 * m31) * d;
            result.M32 = -(m11 * m32 - m12 * m31) * d;
        }
        public static void Lerp(ref MATRIX2x3 matrix1, ref MATRIX2x3 matrix2, float amount, out MATRIX2x3 result)
        {
            result.M11 = matrix1.M11 + (matrix2.M11 - matrix1.M11) * amount;
            result.M12 = matrix1.M12 + (matrix2.M12 - matrix1.M12) * amount;
            result.M21 = matrix1.M21 + (matrix2.M21 - matrix1.M21) * amount;
            result.M22 = matrix1.M22 + (matrix2.M22 - matrix1.M22) * amount;
            result.M31 = matrix1.M31 + (matrix2.M31 - matrix1.M31) * amount;
            result.M32 = matrix1.M32 + (matrix2.M32 - matrix1.M32) * amount;
        }
        public static void Negate(ref MATRIX2x3 matrix, out MATRIX2x3 result)
        {
            result.M11 = -matrix.M11;
            result.M12 = -matrix.M12;
            result.M21 = -matrix.M21;
            result.M22 = -matrix.M22;
            result.M31 = -matrix.M31;
            result.M32 = -matrix.M32;
        }
        public static void Add(ref MATRIX2x3 matrix1, ref MATRIX2x3 matrix2, out MATRIX2x3 result)
        {
            result.M11 = matrix1.M11 + matrix2.M11;
            result.M12 = matrix1.M12 + matrix2.M12;
            result.M21 = matrix1.M21 + matrix2.M21;
            result.M22 = matrix1.M22 + matrix2.M22;
            result.M31 = matrix1.M31 + matrix2.M31;
            result.M32 = matrix1.M32 + matrix2.M32;
        }
        public static void Subtract(ref MATRIX2x3 matrix1, ref MATRIX2x3 matrix2, out MATRIX2x3 result)
        {
            result.M11 = matrix1.M11 - matrix2.M11;
            result.M12 = matrix1.M12 - matrix2.M12;
            result.M21 = matrix1.M21 - matrix2.M21;
            result.M22 = matrix1.M22 - matrix2.M22;
            result.M31 = matrix1.M31 - matrix2.M31;
            result.M32 = matrix1.M32 - matrix2.M32;
        }
        public static void Multiply(ref MATRIX2x3 matrix1, ref MATRIX2x3 matrix2, out MATRIX2x3 result)
        {
            float m11 = matrix1.M11 * matrix2.M11 + matrix1.M12 * matrix2.M21;
            float m12 = matrix1.M11 * matrix2.M12 + matrix1.M12 * matrix2.M22;
            float m21 = matrix1.M21 * matrix2.M11 + matrix1.M22 * matrix2.M21;
            float m22 = matrix1.M21 * matrix2.M12 + matrix1.M22 * matrix2.M22;
            float m31 = matrix1.M31 * matrix2.M11 + matrix1.M32 * matrix2.M21 + matrix2.M31;
            float m32 = matrix1.M31 * matrix2.M12 + matrix1.M32 * matrix2.M22 + matrix2.M32;
            result.M11 = m11;
            result.M12 = m12;
            result.M21 = m21;
            result.M22 = m22;
            result.M31 = m31;
            result.M32 = m32;
        }
        public static void Multiply(ref MATRIX2x3 matrix1, float scaleFactor, out MATRIX2x3 result)
        {
            result.M11 = matrix1.M11 * scaleFactor;
            result.M12 = matrix1.M12 * scaleFactor;
            result.M21 = matrix1.M21 * scaleFactor;
            result.M22 = matrix1.M22 * scaleFactor;
            result.M31 = matrix1.M31 * scaleFactor;
            result.M32 = matrix1.M32 * scaleFactor;
        }
        public static void Divide(ref MATRIX2x3 matrix1, ref MATRIX2x3 matrix2, out MATRIX2x3 result)
        {
            result.M11 = matrix1.M11 / matrix2.M11;
            result.M12 = matrix1.M12 / matrix2.M12;
            result.M21 = matrix1.M21 / matrix2.M21;
            result.M22 = matrix1.M22 / matrix2.M22;
            result.M31 = matrix1.M31 / matrix2.M31;
            result.M32 = matrix1.M32 / matrix2.M32;
        }
        public static void Divide(ref MATRIX2x3 matrix1, float divider, out MATRIX2x3 result)
        {
            float scaleFactor = 1f / divider;
            result.M11 = matrix1.M11 * scaleFactor;
            result.M12 = matrix1.M11 * scaleFactor;
            result.M21 = matrix1.M21 * scaleFactor;
            result.M22 = matrix1.M21 * scaleFactor;
            result.M31 = matrix1.M31 * scaleFactor;
            result.M32 = matrix1.M31 * scaleFactor;
        }
        public static MATRIX2x3 operator -(MATRIX2x3 matrix1)
        {
            MATRIX2x3 result;
            Negate(ref matrix1, out result);
            return result;
        }
        public static bool operator ==(MATRIX2x3 matrix1, MATRIX2x3 matrix2)
        {
            return matrix1.M11 == matrix2.M11 && matrix1.M12 == matrix2.M12 &&
                matrix1.M21 == matrix2.M21 && matrix1.M22 == matrix2.M22 &&
                matrix1.M31 == matrix2.M31 && matrix1.M32 == matrix2.M32;
        }
        public static bool operator !=(MATRIX2x3 matrix1, MATRIX2x3 matrix2)
        {
            return matrix1.M11 != matrix2.M11 || matrix1.M12 != matrix2.M12 &&
                matrix1.M21 != matrix2.M21 || matrix1.M22 != matrix2.M22 &&
                matrix1.M31 != matrix2.M31 || matrix1.M32 != matrix2.M32;
        }
        public static MATRIX2x3 operator +(MATRIX2x3 matrix1, MATRIX2x3 matrix2)
        {
            MATRIX2x3 result;
            Add(ref matrix1, ref matrix2, out result);
            return result;
        }
        public static MATRIX2x3 operator -(MATRIX2x3 matrix1, MATRIX2x3 matrix2)
        {
            MATRIX2x3 result;
            Subtract(ref matrix1, ref matrix2, out result);
            return result;
        }
        public static MATRIX2x3 operator *(MATRIX2x3 matrix1, MATRIX2x3 matrix2)
        {
            MATRIX2x3 result;
            Multiply(ref matrix1, ref matrix2, out result);
            return result;
        }
        public static MATRIX2x3 operator *(MATRIX2x3 matrix1, float scaleFactor)
        {
            MATRIX2x3 result;
            Multiply(ref matrix1, scaleFactor, out result);
            return result;
        }
        public static MATRIX2x3 operator *(float scaleFactor, MATRIX2x3 matrix1)
        {
            MATRIX2x3 result;
            Multiply(ref matrix1, scaleFactor, out result);
            return result;
        }
        public static MATRIX2x3 operator /(MATRIX2x3 matrix1, MATRIX2x3 matrix2)
        {
            MATRIX2x3 result;
            Divide(ref matrix1, ref matrix2, out result);
            return result;
        }
        public static MATRIX2x3 operator /(MATRIX2x3 matrix1, float divider)
        {
            MATRIX2x3 result;
            Divide(ref matrix1, divider, out result);
            return result;
        }
        public static explicit operator MATRIX(MATRIX2x3 matrix)
        {
            MATRIX result;
            result.M11 = matrix.M11;
            result.M12 = matrix.M12;
            result.M13 = 0;
            result.M14 = 0;
            result.M21 = matrix.M21;
            result.M22 = matrix.M22;
            result.M23 = 0;
            result.M24 = 0;
            result.M31 = 0;
            result.M32 = 0;
            result.M33 = 1;
            result.M34 = 0;
            result.M41 = matrix.M31;
            result.M42 = matrix.M32;
            result.M43 = 0;
            result.M44 = 1;
            return result;
        }
		public static explicit operator MATRIX2x3(MATRIX matrix)
        {
            MATRIX2x3 result;
            result.M11 = matrix.M11;
            result.M12 = matrix.M12;
            result.M21 = matrix.M21;
            result.M22 = matrix.M22;
            result.M31 = matrix.M41;
            result.M32 = matrix.M42;
            return result;
        }
    }
    /// <summary>Xna3.1的颜色是BGRA</summary>
	[AReflexible]public struct COLOR : IEquatable<COLOR>
    {
        public const float BYTE_TO_FLOAT = 1f / 255f;
        public static COLOR Default = COLOR.White;
        public byte B;
        public byte G;
        public byte R;
        public byte A;
        //public byte R;
        //public byte G;
        //public byte B;
        //public byte A;

		public uint PackedValue
        {
            get { return PackUtility(R, G, B, A); }
        }
        public static COLOR TransparentBlack
        {
            get
            {
                return new COLOR(0u);
            }
        }
        public static COLOR TransparentWhite
        {
            get
            {
                return new COLOR(16777215u);
            }
        }
        public static COLOR AliceBlue
        {
            get
            {
                return new COLOR(4293982463u);
            }
        }
        public static COLOR AntiqueWhite
        {
            get
            {
                return new COLOR(4294634455u);
            }
        }
        public static COLOR Aqua
        {
            get
            {
                return new COLOR(4278255615u);
            }
        }
        public static COLOR Aquamarine
        {
            get
            {
                return new COLOR(4286578644u);
            }
        }
        public static COLOR Azure
        {
            get
            {
                return new COLOR(4293984255u);
            }
        }
        public static COLOR Beige
        {
            get
            {
                return new COLOR(4294309340u);
            }
        }
        public static COLOR Bisque
        {
            get
            {
                return new COLOR(4294960324u);
            }
        }
        public static COLOR Black
        {
            get
            {
                return new COLOR(4278190080u);
            }
        }
        public static COLOR BlanchedAlmond
        {
            get
            {
                return new COLOR(4294962125u);
            }
        }
        public static COLOR Blue
        {
            get
            {
                return new COLOR(4278190335u);
            }
        }
        public static COLOR BlueViolet
        {
            get
            {
                return new COLOR(4287245282u);
            }
        }
        public static COLOR Brown
        {
            get
            {
                return new COLOR(4289014314u);
            }
        }
        public static COLOR BurlyWood
        {
            get
            {
                return new COLOR(4292786311u);
            }
        }
        public static COLOR CadetBlue
        {
            get
            {
                return new COLOR(4284456608u);
            }
        }
        public static COLOR Chartreuse
        {
            get
            {
                return new COLOR(4286578432u);
            }
        }
        public static COLOR Chocolate
        {
            get
            {
                return new COLOR(4291979550u);
            }
        }
        public static COLOR Coral
        {
            get
            {
                return new COLOR(4294934352u);
            }
        }
        public static COLOR CornflowerBlue
        {
            get
            {
                return new COLOR(4284782061u);
            }
        }
        public static COLOR Cornsilk
        {
            get
            {
                return new COLOR(4294965468u);
            }
        }
        public static COLOR Crimson
        {
            get
            {
                return new COLOR(4292613180u);
            }
        }
        public static COLOR Cyan
        {
            get
            {
                return new COLOR(4278255615u);
            }
        }
        public static COLOR DarkBlue
        {
            get
            {
                return new COLOR(4278190219u);
            }
        }
        public static COLOR DarkCyan
        {
            get
            {
                return new COLOR(4278225803u);
            }
        }
        public static COLOR DarkGoldenrod
        {
            get
            {
                return new COLOR(4290283019u);
            }
        }
        public static COLOR DarkGray
        {
            get
            {
                return new COLOR(4289309097u);
            }
        }
        public static COLOR DarkGreen
        {
            get
            {
                return new COLOR(4278215680u);
            }
        }
        public static COLOR DarkKhaki
        {
            get
            {
                return new COLOR(4290623339u);
            }
        }
        public static COLOR DarkMagenta
        {
            get
            {
                return new COLOR(4287299723u);
            }
        }
        public static COLOR DarkOliveGreen
        {
            get
            {
                return new COLOR(4283788079u);
            }
        }
        public static COLOR DarkOrange
        {
            get
            {
                return new COLOR(4294937600u);
            }
        }
        public static COLOR DarkOrchid
        {
            get
            {
                return new COLOR(4288230092u);
            }
        }
        public static COLOR DarkRed
        {
            get
            {
                return new COLOR(4287299584u);
            }
        }
        public static COLOR DarkSalmon
        {
            get
            {
                return new COLOR(4293498490u);
            }
        }
        public static COLOR DarkSeaGreen
        {
            get
            {
                return new COLOR(4287609995u);
            }
        }
        public static COLOR DarkSlateBlue
        {
            get
            {
                return new COLOR(4282924427u);
            }
        }
        public static COLOR DarkSlateGray
        {
            get
            {
                return new COLOR(4281290575u);
            }
        }
        public static COLOR DarkTurquoise
        {
            get
            {
                return new COLOR(4278243025u);
            }
        }
        public static COLOR DarkViolet
        {
            get
            {
                return new COLOR(4287889619u);
            }
        }
        public static COLOR DeepPink
        {
            get
            {
                return new COLOR(4294907027u);
            }
        }
        public static COLOR DeepSkyBlue
        {
            get
            {
                return new COLOR(4278239231u);
            }
        }
        public static COLOR DimGray
        {
            get
            {
                return new COLOR(4285098345u);
            }
        }
        public static COLOR DodgerBlue
        {
            get
            {
                return new COLOR(4280193279u);
            }
        }
        public static COLOR Firebrick
        {
            get
            {
                return new COLOR(4289864226u);
            }
        }
        public static COLOR FloralWhite
        {
            get
            {
                return new COLOR(4294966000u);
            }
        }
        public static COLOR ForestGreen
        {
            get
            {
                return new COLOR(4280453922u);
            }
        }
        public static COLOR Fuchsia
        {
            get
            {
                return new COLOR(4294902015u);
            }
        }
        public static COLOR Gainsboro
        {
            get
            {
                return new COLOR(4292664540u);
            }
        }
        public static COLOR GhostWhite
        {
            get
            {
                return new COLOR(4294506751u);
            }
        }
        public static COLOR Gold
        {
            get
            {
                return new COLOR(4294956800u);
            }
        }
        public static COLOR Goldenrod
        {
            get
            {
                return new COLOR(4292519200u);
            }
        }
        public static COLOR Gray
        {
            get
            {
                return new COLOR(4286611584u);
            }
        }
        public static COLOR Green
        {
            get
            {
                return new COLOR(4278222848u);
            }
        }
        public static COLOR GreenYellow
        {
            get
            {
                return new COLOR(4289593135u);
            }
        }
        public static COLOR Honeydew
        {
            get
            {
                return new COLOR(4293984240u);
            }
        }
        public static COLOR HotPink
        {
            get
            {
                return new COLOR(4294928820u);
            }
        }
        public static COLOR IndianRed
        {
            get
            {
                return new COLOR(4291648604u);
            }
        }
        public static COLOR Indigo
        {
            get
            {
                return new COLOR(4283105410u);
            }
        }
        public static COLOR Ivory
        {
            get
            {
                return new COLOR(4294967280u);
            }
        }
        public static COLOR Khaki
        {
            get
            {
                return new COLOR(4293977740u);
            }
        }
        public static COLOR Lavender
        {
            get
            {
                return new COLOR(4293322490u);
            }
        }
        public static COLOR LavenderBlush
        {
            get
            {
                return new COLOR(4294963445u);
            }
        }
        public static COLOR LawnGreen
        {
            get
            {
                return new COLOR(4286381056u);
            }
        }
        public static COLOR LemonChiffon
        {
            get
            {
                return new COLOR(4294965965u);
            }
        }
        public static COLOR LightBlue
        {
            get
            {
                return new COLOR(4289583334u);
            }
        }
        public static COLOR LightCoral
        {
            get
            {
                return new COLOR(4293951616u);
            }
        }
        public static COLOR LightCyan
        {
            get
            {
                return new COLOR(4292935679u);
            }
        }
        public static COLOR LightGoldenrodYellow
        {
            get
            {
                return new COLOR(4294638290u);
            }
        }
        public static COLOR LightGreen
        {
            get
            {
                return new COLOR(4287688336u);
            }
        }
        public static COLOR LightGray
        {
            get
            {
                return new COLOR(4292072403u);
            }
        }
        public static COLOR LightPink
        {
            get
            {
                return new COLOR(4294948545u);
            }
        }
        public static COLOR LightSalmon
        {
            get
            {
                return new COLOR(4294942842u);
            }
        }
        public static COLOR LightSeaGreen
        {
            get
            {
                return new COLOR(4280332970u);
            }
        }
        public static COLOR LightSkyBlue
        {
            get
            {
                return new COLOR(4287090426u);
            }
        }
        public static COLOR LightSlateGray
        {
            get
            {
                return new COLOR(4286023833u);
            }
        }
        public static COLOR LightSteelBlue
        {
            get
            {
                return new COLOR(4289774814u);
            }
        }
        public static COLOR LightYellow
        {
            get
            {
                return new COLOR(4294967264u);
            }
        }
        public static COLOR Lime
        {
            get
            {
                return new COLOR(4278255360u);
            }
        }
        public static COLOR LimeGreen
        {
            get
            {
                return new COLOR(4281519410u);
            }
        }
        public static COLOR Linen
        {
            get
            {
                return new COLOR(4294635750u);
            }
        }
        public static COLOR Magenta
        {
            get
            {
                return new COLOR(4294902015u);
            }
        }
        public static COLOR Maroon
        {
            get
            {
                return new COLOR(4286578688u);
            }
        }
        public static COLOR MediumAquamarine
        {
            get
            {
                return new COLOR(4284927402u);
            }
        }
        public static COLOR MediumBlue
        {
            get
            {
                return new COLOR(4278190285u);
            }
        }
        public static COLOR MediumOrchid
        {
            get
            {
                return new COLOR(4290401747u);
            }
        }
        public static COLOR MediumPurple
        {
            get
            {
                return new COLOR(4287852763u);
            }
        }
        public static COLOR MediumSeaGreen
        {
            get
            {
                return new COLOR(4282168177u);
            }
        }
        public static COLOR MediumSlateBlue
        {
            get
            {
                return new COLOR(4286277870u);
            }
        }
        public static COLOR MediumSpringGreen
        {
            get
            {
                return new COLOR(4278254234u);
            }
        }
        public static COLOR MediumTurquoise
        {
            get
            {
                return new COLOR(4282962380u);
            }
        }
        public static COLOR MediumVioletRed
        {
            get
            {
                return new COLOR(4291237253u);
            }
        }
        public static COLOR MidnightBlue
        {
            get
            {
                return new COLOR(4279834992u);
            }
        }
        public static COLOR MintCream
        {
            get
            {
                return new COLOR(4294311930u);
            }
        }
        public static COLOR MistyRose
        {
            get
            {
                return new COLOR(4294960353u);
            }
        }
        public static COLOR Moccasin
        {
            get
            {
                return new COLOR(4294960309u);
            }
        }
        public static COLOR NavajoWhite
        {
            get
            {
                return new COLOR(4294958765u);
            }
        }
        public static COLOR Navy
        {
            get
            {
                return new COLOR(4278190208u);
            }
        }
        public static COLOR OldLace
        {
            get
            {
                return new COLOR(4294833638u);
            }
        }
        public static COLOR Olive
        {
            get
            {
                return new COLOR(4286611456u);
            }
        }
        public static COLOR OliveDrab
        {
            get
            {
                return new COLOR(4285238819u);
            }
        }
        public static COLOR Orange
        {
            get
            {
                return new COLOR(4294944000u);
            }
        }
        public static COLOR OrangeRed
        {
            get
            {
                return new COLOR(4294919424u);
            }
        }
        public static COLOR Orchid
        {
            get
            {
                return new COLOR(4292505814u);
            }
        }
        public static COLOR PaleGoldenrod
        {
            get
            {
                return new COLOR(4293847210u);
            }
        }
        public static COLOR PaleGreen
        {
            get
            {
                return new COLOR(4288215960u);
            }
        }
        public static COLOR PaleTurquoise
        {
            get
            {
                return new COLOR(4289720046u);
            }
        }
        public static COLOR PaleVioletRed
        {
            get
            {
                return new COLOR(4292571283u);
            }
        }
        public static COLOR PapayaWhip
        {
            get
            {
                return new COLOR(4294963157u);
            }
        }
        public static COLOR PeachPuff
        {
            get
            {
                return new COLOR(4294957753u);
            }
        }
        public static COLOR Peru
        {
            get
            {
                return new COLOR(4291659071u);
            }
        }
        public static COLOR Pink
        {
            get
            {
                return new COLOR(4294951115u);
            }
        }
        public static COLOR Plum
        {
            get
            {
                return new COLOR(4292714717u);
            }
        }
        public static COLOR PowderBlue
        {
            get
            {
                return new COLOR(4289781990u);
            }
        }
        public static COLOR Purple
        {
            get
            {
                return new COLOR(4286578816u);
            }
        }
        public static COLOR Red
        {
            get
            {
                return new COLOR(4294901760u);
            }
        }
        public static COLOR RosyBrown
        {
            get
            {
                return new COLOR(4290547599u);
            }
        }
        public static COLOR RoyalBlue
        {
            get
            {
                return new COLOR(4282477025u);
            }
        }
        public static COLOR SaddleBrown
        {
            get
            {
                return new COLOR(4287317267u);
            }
        }
        public static COLOR Salmon
        {
            get
            {
                return new COLOR(4294606962u);
            }
        }
        public static COLOR SandyBrown
        {
            get
            {
                return new COLOR(4294222944u);
            }
        }
        public static COLOR SeaGreen
        {
            get
            {
                return new COLOR(4281240407u);
            }
        }
        public static COLOR SeaShell
        {
            get
            {
                return new COLOR(4294964718u);
            }
        }
        public static COLOR Sienna
        {
            get
            {
                return new COLOR(4288696877u);
            }
        }
        public static COLOR Silver
        {
            get
            {
                return new COLOR(4290822336u);
            }
        }
        public static COLOR SkyBlue
        {
            get
            {
                return new COLOR(4287090411u);
            }
        }
        public static COLOR SlateBlue
        {
            get
            {
                return new COLOR(4285160141u);
            }
        }
        public static COLOR SlateGray
        {
            get
            {
                return new COLOR(4285563024u);
            }
        }
        public static COLOR Snow
        {
            get
            {
                return new COLOR(4294966010u);
            }
        }
        public static COLOR SpringGreen
        {
            get
            {
                return new COLOR(4278255487u);
            }
        }
        public static COLOR SteelBlue
        {
            get
            {
                return new COLOR(4282811060u);
            }
        }
        public static COLOR Tan
        {
            get
            {
                return new COLOR(4291998860u);
            }
        }
        public static COLOR Teal
        {
            get
            {
                return new COLOR(4278222976u);
            }
        }
        public static COLOR Thistle
        {
            get
            {
                return new COLOR(4292394968u);
            }
        }
        public static COLOR Tomato
        {
            get
            {
                return new COLOR(4294927175u);
            }
        }
        public static COLOR Turquoise
        {
            get
            {
                return new COLOR(4282441936u);
            }
        }
        public static COLOR Violet
        {
            get
            {
                return new COLOR(4293821166u);
            }
        }
        public static COLOR Wheat
        {
            get
            {
                return new COLOR(4294303411u);
            }
        }
        public static COLOR White
        {
            get
            {
                return new COLOR(4294967295u);
            }
        }
        public static COLOR WhiteSmoke
        {
            get
            {
                return new COLOR(4294309365u);
            }
        }
        public static COLOR Yellow
        {
            get
            {
                return new COLOR(4294967040u);
            }
        }
        public static COLOR YellowGreen
        {
            get
            {
                return new COLOR(4288335154u);
            }
        }
        public COLOR(uint packedValue)
        {
			this.R = (byte)(packedValue >> 16);
			this.G = (byte)(packedValue >> 8);
			this.B = (byte)packedValue;
			this.A = (byte)(packedValue >> 24);
        }
        public COLOR(byte r, byte g, byte b, byte a)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }
        public COLOR(float r, float g, float b, float a)
        {
            this.R = _MATH.InByte(r * byte.MaxValue);
            this.G = _MATH.InByte(g * byte.MaxValue);
            this.B = _MATH.InByte(b * byte.MaxValue);
            this.A = _MATH.InByte(a * byte.MaxValue);
        }
        public COLOR(COLOR rgb, byte a)
        {
            this.R = rgb.R;
            this.G = rgb.G;
            this.B = rgb.B;
            this.A = a;
        }
		public COLOR(byte r, byte g, byte b) : this(r, g, b, byte.MaxValue) { }
        public byte[] ToRGBA()
        {
            return new byte[] { R, G, B, A };
        }
        public string ToRGBAComma()
        {
            return string.Format("{0},{1},{2},{3}", new object[] { R, G, B, A });
        }
        public override string ToString()
        {
            return string.Format("R:{0} G:{1} B:{2} A:{3}", new object[]
			{
				this.R,
				this.G,
				this.B,
				this.A
			});
        }
        public override int GetHashCode()
        {
            return this.PackedValue.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return obj is COLOR && this.Equals((COLOR)obj);
        }
        public bool Equals(COLOR other)
        {
            return this.R == other.R &&
                this.G == other.G &&
                this.B == other.B &&
                this.A == other.A;
        }
		public bool Equals(ref COLOR other)
		{
			return this.R == other.R &&
				   this.G == other.G &&
				   this.B == other.B &&
				   this.A == other.A;
		}
		public static bool operator ==(COLOR a, COLOR b)
		{
			return a.Equals(b);
		}
		public static bool operator !=(COLOR a, COLOR b)
		{
			return !a.Equals(b);
		}

        public static byte[] Convert(COLOR[] colors)
        {
			int count = colors.Length;
			byte[] buffer = new byte[count * 4];
            for (int i = 0, index = 0; i < count; i++, index = i * 4)
            {
                buffer[index + 0] = colors[i].R;
                buffer[index + 1] = colors[i].G;
                buffer[index + 2] = colors[i].B;
                buffer[index + 3] = colors[i].A;
            }
			return buffer;
        }
        public static COLOR[] Convert(byte[] bytes)
        {
            return Convert(bytes, 0, bytes.Length);
        }
        public static COLOR[] Convert(byte[] bytes, int start)
        {
            return Convert(bytes, start, bytes.Length - start);
        }
        public static COLOR[] Convert(byte[] bytes, int start, int length)
        {
            if (start < 0 || start + length > bytes.Length)
                throw new ArgumentOutOfRangeException();

            if (length % 4 != 0)
                throw new ArgumentException();

            length /= 4;
            COLOR[] colors = new COLOR[length];

            for (int i = 0, index = 0; i < length; i++, index = i * 4)
            {
                colors[i].R = bytes[index + 0];
                colors[i].G = bytes[index + 1];
                colors[i].B = bytes[index + 2];
                colors[i].A = bytes[index + 3];
            }

            return colors;
        }
        public static uint PackUtility(byte r, byte g, byte b, byte a)
        {
            uint num = (uint)(r << 16);
            uint num2 = (uint)(g << 8);
            uint num3 = (uint)(a << 24);
            return num | num2 | b | num3;
        }
        public static byte GetR(uint packedValue)
        {
            return (byte)(packedValue >> 16);
        }
        public static byte GetG(uint packedValue)
        {
            return (byte)(packedValue >> 8);
        }
        public static byte GetB(uint packedValue)
        {
            return (byte)packedValue;
        }
        public static byte GetA(uint packedValue)
        {
            return (byte)(packedValue >> 24);
        }
        public static COLOR ColorFromPackedValue(uint packedValue)
        {
            return new COLOR(packedValue);
        }
        /// <summary>
        /// 颜色的反转色 255 - value
        /// </summary>
        /// <param name="color">要反转的颜色</param>
        /// <returns>反转颜色值后的颜色</returns>
        public static COLOR ColorInverse(COLOR color)
        {
            return new COLOR(
                byte.MaxValue - color.R,
                byte.MaxValue - color.G,
                byte.MaxValue - color.B,
                color.A);
        }
        /// <summary>
        /// 正常叠加模式
        /// </summary>
        /// <param name="value">混合色</param>
        /// <param name="alpha">混合色透明度</param>
        /// <param name="background">背景色</param>
        /// <returns>结果色</returns>
        public static byte ColorNormal(byte value, byte alpha, byte background)
        {
            //return (byte)(value * alpha / byte.MaxValue + background - background * alpha / byte.MaxValue);
            return (byte)(value * alpha / 255 + background - background * alpha / 255);
        }
        /// <summary>
        /// 正常模式的逆运算
        /// </summary>
        /// <param name="value">结果色</param>
        /// <param name="alpha">混合色透明度</param>
        /// <param name="background">背景色</param>
        /// <returns>混合色</returns>
        public static byte ColorNormalEx(byte value, byte alpha, byte background)
        {
            //return (byte)((value - background + background * alpha / byte.MaxValue) * byte.MaxValue / alpha);
            return (byte)((value - background + background * alpha / 255) * 255 / alpha);
        }
        /// <summary>
        /// 滤色
        /// </summary>
        /// <param name="value">源颜色(上层颜色)</param>
        /// <param name="filter">过滤色(下层颜色)</param>
        /// <returns>滤色后的值</returns>
        public static byte ColorScreen(byte value, byte filter)
        {
            return (byte)(255 - (255 - value) * (255 - filter) / 255);
        }
        /// <summary>
        /// 清除指定颜色
        /// </summary>
        /// <param name="rgb">当前颜色</param>
        /// <param name="clear">要清除的颜色</param>
        /// <returns>清除颜色后的颜色</returns>
        public static COLOR ClearColor(COLOR rgb, COLOR clear)
        {
            byte alpha = ColorAlpha(ref rgb);
            if (alpha != 0)
            {
                // 正常模式的逆运算
                rgb.R = ColorNormalEx(rgb.R, alpha, clear.R);
                rgb.G = ColorNormalEx(rgb.G, alpha, clear.G);
                rgb.B = ColorNormalEx(rgb.B, alpha, clear.B);
            }

            return rgb;
        }
        public static COLOR ClearBlack(COLOR rgb)
        {
            return ClearColor(rgb, COLOR.Black);
        }
        public static COLOR ClearWhite(COLOR rgb)
        {
            return ClearColor(rgb, COLOR.White);
        }
        /// <summary>
        /// 使用滤色通过颜色的RGB计算Alpha
        /// </summary>
        /// <param name="rgb">RGB颜色</param>
        /// <returns>颜色的Alpha</returns>
        public static byte ColorAlpha(COLOR rgb)
        {
            byte alpha;
            alpha = ColorScreen(rgb.R, rgb.G);
            alpha = ColorScreen(alpha, rgb.B);
            return alpha;
        }
        public static byte ColorAlpha(ref COLOR rgb)
        {
            byte alpha = ColorAlpha(rgb);
            rgb.A = alpha;
            return alpha;
        }
        public static byte ColorCenter(byte value)
        {
            return (byte)(value >= 128 ? value - 127 : value);
        }
        public static byte ColorAlpha(byte value)
        {
            return (byte)(value >= 128 ? 255 - value : value);
        }
        public static byte ColorGray(byte rgb, byte alpha)
        {
            // 255 - (255 - rgb) * x / 255 = y
            // 255 - (255 - y) * x = alpha
            if (rgb == 255) return 0;
            return (byte)Math.Pow((255 - alpha) * 255 / (255 - rgb), 0.5f);
        }
		public static void Lerp(ref COLOR value1, ref COLOR value2, float amount, out COLOR result)
		{
			result.R = (byte)(value1.R + (value2.R - value1.R) * amount);
			result.G = (byte)(value1.G + (value2.G - value1.G) * amount);
			result.B = (byte)(value1.B + (value2.B - value1.B) * amount);
			result.A = (byte)(value1.A + (value2.A - value1.A) * amount);
		}
    }
    //public enum ELine
    //{
    //    Cross,
    //    Overlap,
    //    Parallel,
    //}
    public struct LINE
    {
        public float A;
        public float B;
        public float C;

        public float K
        {
            get { return -A / B; }
            set { A = value * -B; }
        }
        public float Dx
        {
            get { return -C / A; }
            //set { if (value != 0) A = -C / value; }
        }
        public float Dy
        {
            get { return -C / B; }
            //set { if (value != 0) B = -C / value; }
        }
        public float Angle
        {
            get { return _MATH.ToDegree(Radian); }
            set { Radian = _MATH.ToRadian(value); }
        }
        public float Radian
        {
            get
            {
                bool a = A == 0;
                bool b = B == 0;
                if (a && b)
                    return float.NaN;
                else if (a)
                    return B > 0 ? _MATH.PI : 0;
                else if (b)
                    return A > 0 ? _MATH.PI_OVER_2 : -_MATH.PI_OVER_2;
                else
                    return (float)Math.Atan2(A, -B);
            }
            set { K = (float)Math.Tan(value); }
        }
        public bool IsLine
        {
            get { return A != 0 || B != 0; }
        }
        public bool IsConstantX
        {
            get { return A != 0 && B == 0; }
        }
        public bool IsConstantY
        {
            get { return B != 0 && A == 0; }
        }
        public bool IsClockwise
        {
            get
            {
                if (A == 0)
                    if (B > 0)
                        return true;
                    else
                        return false;
                else
                    if (A > 0)
                        return true;
                    else
                        return false;
            }
        }

        /// <summary>一般式 ax + by + c = 0</summary>
        public LINE(float a, float b, float c)
        {
            this.A = a;
            this.B = b;
            this.C = c;
        }

        public float Value(float x, float y)
        {
            return A * x + B * y + C;
        }
        public float Value(VECTOR2 point)
        {
            return Value(point.X, point.Y);
        }
        public float Value(ref VECTOR2 point)
        {
            return Value(point.X, point.Y);
        }
        public float X(float y)
        {
            if (A == 0)
                if (B == 0)
                    return 0;
                else
                    return Dy;
            else
                return -(B * y + C) / A;
        }
        public float Y(float x)
        {
            if (B == 0)
                if (A == 0)
                    return 0;
                else
                    return Dx;
            else
                return -(A * x + C) / B;
        }
        /// <summary>重合 A1/A2=B1/B2=C1/C2</summary>
        public bool IsOverlap(LINE line)
        {
            float value = A / line.A;
            return _MATH.IsNear(B / line.B, value) &&
                ((C == 0 && line.C == 0) || _MATH.IsNear(C / line.C, value));
        }
        /// <summary>平行 A1/A2=B1/B2≠C1/C2</summary>
        public bool IsParallel(LINE line)
        {
            float value = A / line.A;
            return _MATH.IsNear(B / line.B, value) &&
                ((C != 0 || line.C != 0) && !_MATH.IsNear(C / line.C, value));
        }
        public bool IsParallelOrOverlap(LINE line)
        {
            return _MATH.IsNear(A * line.B, B * line.A);
        }
        /// <summary>垂直 A1A2+B1B2=0</summary>
        public bool IsVertical(LINE line)
        {
            return _MATH.IsNear(A * line.A, -B * line.B);
        }
        public bool IsIntersects(LINE line)
        {
            VECTOR2 cross;
            return Intersects(ref this, ref line, out cross);
        }
        public bool Intersects(LINE line, out VECTOR2 cross)
        {
            return Intersects(ref this, ref line, out cross);
        }
        public VECTOR2 Intersection(LINE line)
        {
            VECTOR2 cross;
            Intersects(ref this, ref line, out cross);
            return cross;
        }
        public LINE Vertical(VECTOR2 point)
        {
            if (A == 0)
                return new LINE(1, 0, -point.X);
            return LinePointK(B / A, point);
        }
        /// <summary>点到直线的距离 d = |Ax0+By0+C| / √(A^2+B^2)</summary>
        public float Distance(VECTOR2 point)
        {
            return _MATH.Abs(Value(point.X, point.Y)) / (float)Math.Sqrt(A * A + B * B);
        }
        /// <summary>距离符号代表在直线的顺时针还是逆时针一侧，不过这和直线的方向有关</summary>
        public float DistanceWithSign(VECTOR2 point)
        {
            return Value(point.X, point.Y) / (float)Math.Sqrt(A * A + B * B);
        }
        /// <summary>平行线之间的距离 d = |C1-C2| / √(A^2+B^2)</summary>
        public float Distance(LINE line)
        {
            if (IsParallel(line))
                if (line.IsConstantX)
                    return _MATH.Distance(Dx, line.Dx);
                else if (line.IsConstantY)
                    return _MATH.Distance(Dy, line.Dy);
                else
                    return Distance(new VECTOR2(0, line.Y(0)));
            else
                return -1;
        }
        /// <summary>一点关于直线对称的点 [x0-2A(Ax0+By0+C)/(A^2+B^2) , y0-2B(Ax0+By0+C)/(A^2+B^2)]</summary>
        public VECTOR2 Symmetrical(VECTOR2 point)
        {
            float value = 2 * Value(point.X, point.Y) / (A * A + B * B);
            VECTOR2 result;
            result.X = point.X - A * value;
            result.Y = point.Y - B * value;
            return result;
        }
        /// <summary>使直线方向相反</summary>
        public void Reverse()
        {
            A = -A;
            B = -B;
            C = -C;
        }
        public override string ToString()
        {
            return string.Format("{0}x + {1}y + {2} = 0", A, B, C);
        }
        public override int GetHashCode()
        {
            return A.GetHashCode() + B.GetHashCode() + C.GetHashCode();
        }

        public static LINE ConstantX(float x)
        {
            return new LINE(-1, 0, x);
        }
        public static LINE ConstantY(float y)
        {
            return new LINE(0, -1, y);
        }
        /// <summary>点斜式 y - y0 = tan(angle)(x - x0)</summary>
        public static LINE LinePointAngle(float angle, VECTOR2 target)
        {
            if (_MATH.Range(angle, 180) == 90)
            {
                LINE line;
                line.A = -1;
                line.B = 0;
                line.C = target.X;
                return line;
            }
            return LinePointK((float)Math.Tan(_MATH.ToRadian(angle)), target);
        }
        /// <summary>点斜式 y - y0 = k(x - x0)</summary>
        public static LINE LinePointK(float k, VECTOR2 target)
        {
            LINE line;
            line.A = k;
            line.B = -1;
            line.C = -k * target.X + target.Y;
            return line;
        }
        /// <summary>两点式 (y - y1) / (y2 - y1) = (x - x1) / (x2 - x1)</summary>
        public static LINE LineMultiPoint(VECTOR2 p1, VECTOR2 p2)
        {
            LINE line;
            line.A = p2.Y - p1.Y;
            line.B = p1.X - p2.X;
            line.C = -p1.X * (p2.Y - p1.Y) + p1.Y * (p2.X - p1.X);
            return line;
        }
        /// <summary>截距式 x / a + y / b = 1</summary>
        public static LINE LineIntercept(float a, float b)
        {
            LINE line;
            line.A = 1 / a;
            line.B = 1 / b;
            line.C = -1;
            return line;
        }
        /// <summary>斜截式 y = kx + b</summary>
        public static LINE LineInterceptY(float k, float b)
        {
            LINE line;
            line.A = k;
            line.B = -1;
            line.C = b;
            return line;
        }
        public static void ParallelOrOverlap(ref LINE line1, ref LINE line2, out bool parallel, out bool overlap)
        {
            float value = line1.A / line2.A;
            if (_MATH.IsNear(line1.B / line2.B, value))
            {
                overlap = (line1.C == 0 && line2.C == 0) || _MATH.IsNear(line1.C / line2.C, value);
                parallel = !overlap;
            }
            else
            {
                parallel = false;
                overlap = false;
            }
        }
        public static bool Intersects(ref LINE line1, ref LINE line2, out VECTOR2 cross)
        {
            cross = VECTOR2.NaN;
            if (!line1.IsLine || !line2.IsLine)
                return false;

            bool parallel, overlap;
            ParallelOrOverlap(ref line1, ref line2, out parallel, out overlap);
            if (overlap)
            {
                cross = new VECTOR2(float.PositiveInfinity, float.PositiveInfinity);
                return true;
            }
            else if (parallel)
                return false;

            if (line1.IsConstantX)
            {
                cross.X = line1.X(0);
                cross.Y = line2.Y(cross.X);
            }
            else if (line2.IsConstantX)
            {
                cross.X = line2.X(0);
                cross.Y = line1.Y(cross.X);
            }
            else if (line1.IsConstantY)
            {
                cross.Y = line1.Y(0);
                cross.X = line2.X(cross.Y);
            }
            else if (line2.IsConstantY)
            {
                cross.Y = line2.Y(0);
                cross.X = line1.X(cross.Y);
            }
            else
            {
                // 两直线方程联立求解
                cross.X = -(line1.C * line2.B - line2.C * line1.B) / (line1.A * line2.B - line2.A * line1.B);
                cross.Y = line1.Y(cross.X);
            }
            return true;
        }
    }
    public interface IShape<T>
    {
        VECTOR2 Center { get; set; }
        RECT Bound { get; }

        bool Contains(float x, float y);
        void Offset(float x, float y);
        bool Intersects(T target);
    }
	[AReflexible]public struct RECT : IEquatable<RECT>, IShape<RECT>
    {
		private static VECTOR2[] _boundingBox = new VECTOR2[4];
		private static RECT _empty = new RECT(0, 0, 0, 0);
		public static RECT Empty
		{
			get { return _empty; }
		}

        public float X;
        public float Y;
        public float Width;
        public float Height;
        public float Right
        {
            get
            {
                return this.X + this.Width;
            }
        }
        public float Bottom
        {
            get
            {
                return this.Y + this.Height;
            }
        }
        public VECTOR2 Center
        {
            get
            {
                return new VECTOR2(this.X + this.Width / 2.0f, this.Y + this.Height / 2.0f);
            }
            set
            {
                VECTOR2 moved = VECTOR2.Subtract(value, Center);
                Offset(moved.X, moved.Y);
            }
        }
        public VECTOR2 Location
        {
            get { return new VECTOR2(X, Y); }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }
        public VECTOR2 Size
        {
            get { return new VECTOR2(Width, Height); }
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }
        public RECT Bound
        {
            get { return this; }
        }
        public bool IsEmpty
        {
            get
            {
                return this.Width == 0 && this.Height == 0 && this.X == 0 && this.Y == 0;
            }
        }
        public RECT(float x, float y, float width, float height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }
        public RECT(VECTOR2 location, VECTOR2 size) :
            this(location.X, location.Y, size.X, size.Y) { }
        public void Offset(float offsetX, float offsetY)
        {
            this.X += offsetX;
            this.Y += offsetY;
        }
        public void Inflate(float horizontalAmount, float verticalAmount)
        {
            this.X -= horizontalAmount;
            this.Y -= verticalAmount;
            this.Width += horizontalAmount * 2;
            this.Height += verticalAmount * 2;
        }
        public bool Contains(float x, float y)
        {
            return this.X <= x && x < this.X + this.Width && this.Y <= y && y < this.Y + this.Height;
        }
        public bool Contains(VECTOR2 value)
        {
            return this.X <= value.X && value.X < this.X + this.Width && this.Y <= value.Y && value.Y < this.Y + this.Height;
        }
        public bool Contains(RECT value)
        {
            return this.X <= value.X && value.X + value.Width <= this.X + this.Width && this.Y <= value.Y && value.Y + value.Height <= this.Y + this.Height;
        }
		public bool ContainsLocation(float x, float y)
		{
			return this.X <= x && this.Y <= y && x < this.Width && y < this.Height;
		}
        /// <summary>倍数缩放</summary>
        /// <param name="pivot">缩放锚点</param>
        /// <param name="x">横轴缩放倍数</param>
        /// <param name="y">纵轴缩放倍数</param>
        public void ScaleMultiple(EPivot pivot, float x, float y)
        {
            Scale(pivot, Width * x, Height * y);
        }
        /// <summary>量缩放</summary>
        /// <param name="pivot">缩放锚点</param>
        /// <param name="x">横轴缩放量</param>
        /// <param name="y">纵轴缩放量</param>
        public void Scale(EPivot pivot, float x, float y)
        {
            X -= x * UIElement.PivotX(pivot) * 0.5f;
            Y -= y * UIElement.PivotY(pivot) * 0.5f;
            Width += x;
            Height += y;
        }
		public RECT ToLocation()
		{
			RECT result;
			result.X = this.X;
			result.Y = this.Y;
			result.Width = this.Right;
			result.Height = this.Height;
			return result;
		}
        public bool Intersects(RECT value)
        {
            return value.X < this.X + this.Width && this.X < value.X + value.Width && value.Y < this.Y + this.Height && this.Y < value.Y + value.Height;
        }
        public bool Intersects(ref RECT value)
        {
            return value.X < this.X + this.Width && this.X < value.X + value.Width && value.Y < this.Y + this.Height && this.Y < value.Y + value.Height;
        }
        public bool Equals(RECT other)
        {
            return this.X == other.X && this.Y == other.Y && this.Width == other.Width && this.Height == other.Height;
        }
        public override bool Equals(object obj)
        {
            bool result = false;
            if (obj is RECT)
            {
                result = this.Equals((RECT)obj);
            }
            return result;
        }
        public override string ToString()
        {
            return string.Format("X:{0} Y:{1} Width:{2} Height:{3}", X, Y, Width, Height);
        }
        public override int GetHashCode()
        {
            return this.X.GetHashCode() + this.Y.GetHashCode() + this.Width.GetHashCode() + this.Height.GetHashCode();
        }
		public static RECT Intersect(RECT value1, RECT value2)
		{
			RECT result;
			Intersect(ref value1, ref value2, out result);
			return result;
		}
		public static void Intersect(ref RECT value1, ref RECT value2, out RECT result)
		{
			float num = value1.X + value1.Width;
			float num2 = value2.X + value2.Width;
			float num3 = value1.Y + value1.Height;
			float num4 = value2.Y + value2.Height;
			float num5 = (value1.X > value2.X) ? value1.X : value2.X;
			float num6 = (value1.Y > value2.Y) ? value1.Y : value2.Y;
			float num7 = (num < num2) ? num : num2;
			float num8 = (num3 < num4) ? num3 : num4;
			if (num7 > num5 && num8 > num6)
			{
				result.X = num5;
				result.Y = num6;
				result.Width = num7 - num5;
				result.Height = num8 - num6;
				return;
			}
			result.X = 0;
			result.Y = 0;
			result.Width = 0;
			result.Height = 0;
		}
		public static RECT Union(RECT value1, RECT value2)
		{
			RECT result;
            Union(ref value1, ref value2, out result);
			return result;
		}
        public static void Union(ref RECT value1, ref RECT value2, out RECT result)
        {
            float num = value1.X + value1.Width;
            float num2 = value2.X + value2.Width;
            float num3 = value1.Y + value1.Height;
            float num4 = value2.Y + value2.Height;
            float num5 = (value1.X < value2.X) ? value1.X : value2.X;
            float num6 = (value1.Y < value2.Y) ? value1.Y : value2.Y;
            float num7 = (num > num2) ? num : num2;
            float num8 = (num3 > num4) ? num3 : num4;
            result.X = num5;
            result.Y = num6;
            result.Width = num7 - num5;
            result.Height = num8 - num6;
        }
        public static RECT ScreenToCartesian(RECT source, VECTOR2 screen)
        {
            RECT result;
            ScreenToCartesian(ref source, ref screen, out result);
            return result;
        }
        public static void ScreenToCartesian(ref RECT source, ref VECTOR2 screen, out RECT result)
        {
            result.X = source.X;
            result.Y = screen.Y - source.Height - source.Y;
            result.Width = source.Width;
            result.Height = source.Height;
        }
        public static void Transform(ref RECT source, ref MATRIX2x3 matrix, out RECT result)
        {
            float x = source.X * matrix.M11 + source.Y * matrix.M21 + matrix.M31;
            float y = source.X * matrix.M12 + source.Y * matrix.M22 + matrix.M32;
            float w = source.Width * matrix.M11 + source.Height * matrix.M21;
            float h = source.Width * matrix.M12 + source.Height * matrix.M22;
            result.X = x;
            result.Y = y;
            result.Width = w;
            result.Height = h;
        }
        public static void Transform(ref RECT source, ref MATRIX2x3 matrix)
        {
            Transform(ref source, ref matrix, out source);
        }
        public static void CreateBoundingBox(ref RECT box, ref MATRIX2x3 transform, out RECT result)
        {
            float right = box.Right;
            float bottom = box.Bottom;
            _boundingBox[0] = new VECTOR2(box.X, box.Y);
            _boundingBox[1] = new VECTOR2(right, box.Y);
            _boundingBox[2] = new VECTOR2(right, bottom);
            _boundingBox[3] = new VECTOR2(box.X, bottom);
            VECTOR2.Transform(_boundingBox, ref transform, _boundingBox);
            CreateBoundingBox(out result, _boundingBox);
        }
		public static RECT CreateBoundingBox(RECT box, MATRIX2x3 transform)
		{
            CreateBoundingBox(ref box, ref transform, out box);
            return box;
		}
		public static RECT CreateBoundingBox(params VECTOR2[] points)
		{
			RECT result;
			CreateBoundingBox(out result, points);
			return result;
		}
		public static void CreateBoundingBox(out RECT result, params VECTOR2[] points)
		{
            result.X = float.PositiveInfinity;
            result.Y = float.PositiveInfinity;
            result.Width = float.NegativeInfinity;
            result.Height = float.NegativeInfinity;

			bool flag = true;
			int count = points.Length;
            for (int i = 0; i < count; i++)
            {
                VECTOR2 p = points[i];
                if (flag)
                {
                    result.X = p.X;
                    result.Width = p.X;
                    result.Y = p.Y;
                    result.Height = p.Y;
                    flag = false;
                }
                else
                {
                    if (p.X < result.X)
                    {
                        result.X = p.X;
                    }
                    else if (p.X > result.Width)
                    {
                        result.Width = p.X;
                    }

                    if (p.Y < result.Y)
                    {
                        result.Y = p.Y;
                    }
                    else if (p.Y > result.Height)
                    {
                        result.Height = p.Y;
                    }
                }
            }

			if (!flag)
			{
				result.Width -= result.X;
				result.Height -= result.Y;
			}
		}
		public static RECT CreateRectangle(VECTOR2 p1, VECTOR2 p2)
		{
			RECT target;
			target.X = Math.Min(p1.X, p2.X);
			target.Y = Math.Min(p1.Y, p2.Y);
			target.Width = Math.Abs(p2.X - p1.X);
			target.Height = Math.Abs(p2.Y - p1.Y);
			return target;
		}
        public static void CreateRectangle(ref VECTOR2 p1, ref VECTOR2 p2, out RECT target)
        {
            target.X = Math.Min(p1.X, p2.X);
            target.Y = Math.Min(p1.Y, p2.Y);
            target.Width = Math.Abs(p2.X - p1.X);
            target.Height = Math.Abs(p2.Y - p1.Y);
        }
		public static bool operator ==(RECT value1, RECT value2)
		{
			return value1.X == value2.X &&
				value1.Y == value2.Y &&
				value1.Width == value2.Width &&
				value1.Height == value2.Height;
		}
		public static bool operator !=(RECT value1, RECT value2)
		{
			return !(value1 == value2);
		}
        public static RECT operator +(RECT rect, VECTOR2 v)
        {
			rect.Offset(v.X, v.Y);
			return rect;
        }
        public static RECT operator -(RECT rect, VECTOR2 v)
        {
			rect.Offset(-v.X, -v.Y);
			return rect;
        }
        public static RECT operator +(RECT rect)
        {
            return new RECT(rect.X, rect.Y, rect.Width, rect.Height);
        }
        public static RECT operator *(RECT rect, VECTOR2 v)
        {
            return new RECT(rect.X * v.X, rect.Y * v.Y, rect.Width * v.X, rect.Height * v.Y);
        }
        public static RECT operator *(VECTOR2 v, RECT rect)
        {
            return rect * v;
        }
        public static RECT operator *(RECT rect, float v)
        {
            return new RECT(rect.X * v, rect.Y * v, rect.Width * v, rect.Height * v);
        }
        public static RECT operator *(float v, RECT rect)
        {
            return rect * v;
        }
        public static RECT operator /(RECT rect, VECTOR2 v)
        {
            return new RECT(rect.X / v.X, rect.Y / v.Y, rect.Width / v.X, rect.Height / v.Y);
        }
        public static RECT operator /(RECT rect, float v)
        {
            return new RECT(rect.X / v, rect.Y / v, rect.Width / v, rect.Height / v);
        }
    }
    [AReflexible]public struct CIRCLE : IShape<CIRCLE>, IEquatable<CIRCLE>
    {
        // fields
        public float R;
        public VECTOR2 C;

        // properties
        public float A
        {
            get { return C.X; }
            set { C.X = value; }
        }
        public float B
        {
            get { return C.Y; }
            set { C.Y = value; }
        }
        public float D
        {
			get { return R * 2; }
        }
        public VECTOR2 Center
        {
            get { return C; }
            set { C = value; }
        }
        public RECT Bound
        {
            get
            {
                float d = D;
                return new RECT(C.X - R, C.Y - R, d, d);
            }
        }

        // contructors
        public CIRCLE(float r)
        {
            this.R = r;
			this.C = new VECTOR2();
        }
        public CIRCLE(float r, VECTOR2 c)
        {
            this.R = r;
            this.C = c;
        }

        // methods
        // 圆的方程(x - a)^2 + (y - b)^2 = r^2
        public void X(float y, out float x1, out float x2)
        {
            float temp = y - C.Y;
            temp = R * R - temp * temp;
            temp = (float)Math.Sqrt(temp);
            x1 = temp + C.X;
            x2 = -temp + C.X;
        }
        public void Y(float x, out float y1, out float y2)
        {
            float temp = x - C.X;
            temp = R * R - temp * temp;
            temp = (float)Math.Sqrt(temp);
            y1 = temp + C.Y;
            y2 = -temp + C.Y;
        }
        /// <summary>过一点得出圆的切线</summary>
        [Code(ECode.BUG)]
        public void TangentLine(float x, float y, out LINE line1, out LINE line2)
        {
            float dx = x - C.X;
            float dy = y - C.Y;
            dx *= dx;
            dy *= dy;
            float r = R * R;
            if (dx + dy < r)
            {
                line1 = new LINE();
                line2 = new LINE();
                return;
            }
            else if (dx + dy == r)
            {
                if (y == C.Y)
                    line1 = LINE.ConstantX(x);
                else if (x == C.X)
                    line1 = LINE.ConstantY(y);
                else
                    line1 = LINE.LinePointK((y - C.Y) / (x - C.X) + _MATH.PI_OVER_2, new VECTOR2(x, y));
                line2 = line1;
                // 保证圆在line1到line2的夹角内
                if (line1.IsClockwise)
                    line1.Reverse();
                else
                    line2.Reverse();
            }
            else
            {
                VECTOR2 target = new VECTOR2(x, y);
                float angle;
                VECTOR2.Degree(ref target, ref C, out angle);

                line1 = new LINE();
                line2 = new LINE();
                // 圆的切线方程(x-a)(x0-a)+(y-b)(y0-b)=r^2
                if (y == C.Y + R || y == C.Y - R)
                {
                    if (x < C.X == y < C.Y)
                        line1 = LINE.ConstantY(y);
                    else
                        line2 = LINE.ConstantY(y);
                }

                if (x == C.X + R || x == C.X - R)
                {
                    if (x < C.X == y < C.Y)
                        line2 = LINE.ConstantX(x);
                    else
                    {
                        line1 = LINE.ConstantX(x);
                        line1.Reverse();
                    }
                }

                if (!line1.IsLine || !line2.IsLine)
                {
                    // 点斜式(y0 - y) = k(x0 - x)
                    // 一般式kx0 - y0 - kx + y = 0
                    // 点到直线的距离 d = r = |Ax0+By0+C| / √(A^2+B^2)
                    // R = |kC.X - C.y - kx + y| / √(k^2+1)
                    // R^2 = (kC.x-C.y-kx+y)^2 / (k^2+1)
                    // R^2 * k^2 + R^2 = ((C.x-x)k + (y-C.y))^2
                    float n1 = C.X - x;
                    float n2 = y - C.Y;
                    // n1^2 * k^2 + 2 * (n1*k) * n2 + n2^2 - R^2 * k^2 - R^2 = 0
                    // (n1^2 - R^2)k^2 + 2*n1*n2*k + n2^2-R^2 = 0
                    float a = n1 * n1 - r;
                    float b = 2 * n1 * n2;
                    float c = n2 * n2 - r;
                    // k = (-b+-√b^2-4ac) / 2a
                    float temp = (float)Math.Sqrt(b * b - 4 * a * c);
                    if (!line2.IsLine)
                    {
                        float k2;
                        if (a == 0)
                            k2 = -c / b;
                        else
                            k2 = (-b + temp) / (2 * a);
                        line2 = LINE.LinePointK(k2, new VECTOR2(x, y));
                        if (line1.IsConstantY)
                            line2.Reverse();
                    }
                    if (!line1.IsLine)
                    {
                        float k1;
                        if (a == 0)
                            k1 = -c / b;
                        else
                            k1 = (-b - temp) / (2 * a);
                        line1 = LINE.LinePointK(k1, new VECTOR2(x, y));
                        if (line2.IsConstantY)
                            line1.Reverse();
                    }
                }

                if (y <= C.Y)
                    line2.Reverse();
                else
                    line1.Reverse();
            }
        }
        public float OneTangentLineAngleDifference(float x, float y)
        {
            VECTOR2 target = new VECTOR2(x, y);
            return OneTangentLineRadianDifference(ref target);
        }
        public float OneTangentLineRadianDifference(ref VECTOR2 target)
        {
            float dx = target.X - C.X;
            float dy = target.Y - C.Y;
            dx *= dx;
            dy *= dy;
            float r = R * R;
            if (dx + dy < r)
                return float.NaN;
            else if (dx + dy == r)
                return _MATH.PI_2;
            else
            {
                float distance;
                VECTOR2.Distance(ref target, ref C, out distance);
                return (float)Math.Asin(R / distance);
            }
        }
        public bool Contains(float x, float y)
        {
            float num1 = x - C.X;
            float num2 = y - C.Y;
            float num3 = R;
            return num1 * num1 + num2 * num2 < num3 * num3;
        }
        public bool Contains(VECTOR2 target)
        {
            float num1 = target.X - C.X;
            float num2 = target.Y - C.Y;
            float num3 = R;
            return num1 * num1 + num2 * num2 < num3 * num3;
        }
        public bool Contains(CIRCLE target)
        {
            float num1 = target.C.X - C.X;
            float num2 = target.C.Y - C.Y;
            float num3 = target.R - this.R;
            return num1 * num1 + num2 * num2 < num3 * num3;
        }
        public bool Intersects(CIRCLE target)
        {
            float num1 = target.C.X - C.X;
            float num2 = target.C.Y - C.Y;
            float num3 = target.R + this.R;
            return num1 * num1 + num2 * num2 < num3 * num3;
        }
        public void Offset(float x, float y)
        {
            C.X += x;
            C.Y += y;
        }
		public VECTOR2 ParametricEquation(float radian)
		{
			VECTOR2 result;
			ParametricEquation(ref C, R, radian, out result);
			return result;
		}

        // override
        public override string ToString()
        {
            return "R = " + R + ", Center = " + C;
        }

        // clone
        public bool Equals(CIRCLE other)
        {
            return R == other.R && C.Equals(ref other.C);
        }

		public static void ParametricEquation(ref CIRCLE circle, float radian, out VECTOR2 result)
		{
			ParametricEquation(ref circle.C, circle.R, radian, out result);
		}
		public static void ParametricEquation(float radius, float radian, out VECTOR2 result)
		{
            result.X = (float)Math.Cos(radian) * radius;
            result.Y = (float)Math.Sin(radian) * radius;
		}
		public static void ParametricEquation(ref VECTOR2 center, float radius, float radian, out VECTOR2 result)
		{
			float x = center.X;
			float y = center.Y;
			ParametricEquation(radius, radian, out result);
			result.X += x;
			result.Y += y;
		}
    }
    // 实现IShape<ShapeList<T>>将会导致2个Intersects指向不明
    public class ShapeList<T> : PoolStack<T>, IShape<T> where T : IShape<T>
    {
        public VECTOR2 Center
        {
            get
            {
                VECTOR2 center = VECTOR2.Zero;
                if (size == 0)
                    return center;
                for (int i = 0; i < size; i++)
                    center.Add(items[i].Center);
                return VECTOR2.Divide(ref center, size);
            }
            set
            {
                VECTOR2 move = VECTOR2.Subtract(value, Center);
                Offset(move.X, move.Y);
            }
        }
        public RECT Bound
        {
            get
            {
                if (Count == 0)
                    return new RECT();
                RECT first = items[0].Bound;
                if (Count == 1)
                    return first;
                VECTOR2 c = first.Center;
                RECT bound = new RECT(c.X, c.Y, c.X, c.Y);
                for (int i = 0; i < Count; i++)
                {
                    T scope = items[i];
                    RECT temp = scope.Bound;
                    if (temp.X < bound.X)
                        bound.X = temp.X;
                    if (temp.Y < bound.Y)
                        bound.Y = temp.Y;
                    if (temp.Right > bound.Width)
                        bound.Width = temp.Right;
                    if (temp.Bottom > bound.Height)
                        bound.Height = temp.Bottom;
                }
                return new RECT(bound.X, bound.Y, bound.Width - bound.X, bound.Height - bound.Y);
            }
        }
        public T Main
        {
            get
            {
                if (Count == 0)
                    return default(T);
                else
                    return items[0];
            }
        }

        public bool Contains(float x, float y)
        {
            for (int i = 0; i < Count; i++)
            {
                if (items[i].Contains(x, y))
                {
                    return true;
                }
            }
            return false;
        }
        public void Offset(float x, float y)
        {
            for (int i = 0; i < Count; i++)
            {
                items[i].Offset(x, y);
            }
        }
        public bool Intersects(T target)
        {
            for (int i = 0; i < Count; i++)
            {
                if (items[i].Intersects(target))
                {
                    return true;
                }
            }
            return false;
        }
        public bool Intersects(ShapeList<T> target)
        {
            for (int i = 0; i < Count; i++)
            {
                for (int j = 0; j < target.Count; j++)
                {
                    if (items[i].Intersects(target.items[j]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
    public class CircleList : ShapeList<CIRCLE>
    {
        public void Rotate(CIRCLE circle, float radian)
        {
            Rotate(circle.C, radian);
        }
        public void Rotate(VECTOR2 vector, float radian)
        {
            for (int i = 0; i < Count; i++)
            {
                VECTOR2.Rotate(ref vector, ref items[i].C, radian, out items[i].C);
            }
        }
    }
    
    public abstract class Graph<T> : IEnumerable<T> where T : Graph<T>
    {
        private List<T> nodes = new List<T>();
        private List<T> entire;
        /// <summary>是否为有向图。此属性针对添加和删除的父节点，而非被添加和被删除的子节点</summary>
        public bool Digraph;

        //public List<T> Nodes
        //{
        //    get { return nodes; }
        //}
        public T First
        {
            get { return nodes.Count > 0 ? nodes[0] : null; }
        }
        public T Last
        {
            get { return nodes.Count > 0 ? nodes[nodes.Count - 1] : null; }
        }
        public T this[int index]
        {
            get { return nodes[index]; }
        }
        public int Count
        {
            get { return nodes.Count; }
        }

        protected virtual bool Add(T node)
        {
            return true;
        }
        public void AddChild(T node)
        {
            if (node == null)
                throw new ArgumentNullException("graph node");

            if (Add(node) && !nodes.Contains(node))
            {
                nodes.Add(node);
                if (!Digraph)
                    node.nodes.Add((T)this);
                ClearEntireGraphBuffer();
                Added(node);
            }
        }
        protected virtual void Added(T node)
        {
        }
        public void AddChild(IEnumerable<T> nodes)
        {
            foreach (var node in nodes)
                AddChild(node);
        }
        public bool Remove(T node)
        {
            bool result = nodes.Remove(node);
            if (result)
            {
                if (!Digraph && this is T)
                    node.nodes.Remove((T)this);
                ClearEntireGraphBuffer();
            }
            return result;
        }
        public void RemoveAt(int index)
        {
            T node = nodes[index];
            nodes.RemoveAt(index);
            if (!Digraph && this is T)
                node.nodes.Remove((T)this);
            ClearEntireGraphBuffer();
        }
        public void Clear()
        {
            foreach (var link in nodes)
                link.nodes.Remove((T)this);
            nodes.Clear();
            ClearEntireGraphBuffer();
        }
        private void ClearEntireGraphBuffer()
        {
            if (entire != null)
            {
                Queue<T> clone = new Queue<T>(entire);
                while (clone.Count > 0)
                    clone.Dequeue().entire = null;
            }
        }
        public List<T> GetEntireGraph()
        {
            if (entire != null)
                return entire;

            entire = new List<T>();
            HashSet<T> set = new HashSet<T>();
            AddGraphNode(entire, set, (T)this);
            entire.AddRange(set);
            return entire;
        }
        private static void AddGraphNode(List<T> entire, HashSet<T> set, T node)
        {
            node.entire = entire;
            if (set.Add(node))
                foreach (var item in node)
                    AddGraphNode(entire, set, item);
        }
        public IEnumerator<T> GetEnumerator()
        {
            return nodes.GetEnumerator();
        }
        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static GraphPath<T> Navigate(T start, T target)
        {
            return Navigate(start, null, target);
        }
        public static GraphPath<T> Navigate(T start, Func<T, bool> visitable, T target)
        {
            return Navigate(start, -1, visitable, target);
        }
        /// <summary>
        /// 寻路
        /// </summary>
        /// <param name="start">起始节点</param>
        /// <param name="breadth">最远距离</param>
        /// <param name="target">目标节点</param>
        /// <returns>寻路路径</returns>
        public static GraphPath<T> Navigate(T start, int breadth, Func<T, bool> visitable, T target)
        {
            if (start == target)
                return new GraphPath<T>(start, 0);
            var path = GPS(start, breadth, visitable, p => p.Node == target);
            GraphPath<T> result;
            path.TryGetValue(target, out result);
            return result;
        }
        /// <summary>
        /// 广度优先循环图节点
        /// </summary>
        /// <param name="start">开始节点</param>
        /// <param name="breadth">广度，-1则循环整个图</param>
        /// <param name="visitable">节点是否可以访问</param>
        /// <param name="onNewNode">加入新节点的回调事件，返回true则中断循环</param>
        /// <returns>可访问的图节点</returns>
        /// <remarks>效率: EntryEngine.Timer=5000节点/20ms</remarks>
        public static Dictionary<T, GraphPath<T>> GPS(T start, int breadth, Func<T, bool> visitable, Func<GraphPath<T>, bool> onNewNode)
        {
            bool find = false;
            Dictionary<T, GraphPath<T>> path = new Dictionary<T, GraphPath<T>>();

            int d = 0;
            path.Add(start, new GraphPath<T>(start, d));

            int capcity = breadth < 0 ? start.entire.Count : 4 * breadth;
            Queue<T> visit = new Queue<T>(capcity);
            visit.Enqueue(start);
            Queue<T> next = new Queue<T>(capcity);
            GraphPath<T> __path;
            while (visit.Count > 0)
            {
                d++;
                if (breadth >= 0 && d > breadth)
                    break;

                while (visit.Count > 0)
                {
                    var current = visit.Dequeue();
                    foreach (var node in current.nodes)
                    {
                        if ((visitable == null || visitable(node)))
                        {
                            if (path.TryGetValue(node, out __path))
                            {
                                if (d == __path.Distance)
                                {
                                    __path.Previous.Add(path[current]);
                                }
                            }
                            else
                            {
                                __path = new GraphPath<T>(node, d);
                                __path.Previous.Add(path[current]);
                                path.Add(node, __path);
                                if (onNewNode != null && onNewNode(__path))
                                    find = true;
                                next.Enqueue(node);
                            }
                        }
                    }
                }

                if (find)
                    break;

                var swap = visit;
                visit = next;
                next = swap;
            }

            return path;
        }
        public static HashSet<T> Broadcast(T start, int breadth)
        {
            return Broadcast(start, breadth, null);
        }
        /// <summary>
        /// 获得节点周围指定广度的节点
        /// </summary>
        /// <param name="start">开始节点</param>
        /// <param name="breadth">广度</param>
        /// <param name="visitable">节点是否可以访问</param>
        /// <returns>指定节点广度周围的节点（包含指定节点）</returns>
        public static HashSet<T> Broadcast(T start, int breadth, Func<T, bool> visitable)
        {
            HashSet<T> location = new HashSet<T>();
            location.Add(start);

            int d = 0;
            // key = root, value = link nodes
            HashSet<T> nodes = new HashSet<T>(start);
            while (nodes.Count > 0)
            {
                d++;
                if (breadth >= 0 && d > breadth)
                    break;

                HashSet<T> next = new HashSet<T>();
                foreach (var road in nodes)
                {
                    if ((visitable == null || visitable(road)) && location.Add(road))
                    {
                        foreach (var node in road)
                        {
                            if (!location.Contains(node))
                            {
                                next.Add(node);
                            }
                        }
                    }
                }
                nodes.Clear();
                nodes = next;
            }

            return location;
        }
        public static void BuildGraph4444(T[] grids, int cols)
        {
            int count = grids.Length;
            if (count % cols != 0)
                throw new ArgumentOutOfRangeException("grid count don't match the map cols.");
            int rows = count / cols;
            List<T> graph = new List<T>(grids);

            T grid;
            int x, y;
            bool l, r, t, b;
            for (int i = 0; i < count; i++)
            {
                grid = grids[i];
                x = i % cols;
                y = i / cols;
                l = x == 0;
                r = x == cols - 1;
                t = y == 0;
                b = y == rows - 1;

                if (!l)
                    grid.AddChild(grids[i - 1]);
                if (!r)
                    grid.AddChild(grids[i + 1]);
                if (!t)
                    grid.AddChild(grids[i - cols]);
                if (!b)
                    grid.AddChild(grids[i + cols]);

                grid.entire = graph;
            }
        }
        //public static void BuildGraph4343(T[] grids, int cols)
        //{
        //    int count = grids.Length;
        //    int rows = (2 * count) / (2 * cols - 1);
        //    if (rows * cols - rows / 2 != count)
        //        throw new ArgumentOutOfRangeException("grid count don't match the map cols.");
        //    List<T> graph = new List<T>(grids);

        //    T grid;
        //    int x, y;
        //    bool l, r, t, b, even;
        //    for (int i = 0; i < count; i++)
        //    {
        //        grid = grids[i];
        //        // x, y error!
        //        x = i % cols;
        //        y = i / cols;
        //        l = x == 0;
        //        r = x == cols - 1 - y % 2;
        //        t = y == 0;
        //        b = y == rows - 1;
        //        even = y % 2 == 1;

        //        if (!t)
        //        {
        //            if (!l || even)
        //                grid.AddChild(grids[i - cols]);
        //            if (!r || even)
        //                grid.AddChild(grids[i - cols + 1]);
        //        }
        //        if (!b)
        //        {
        //            if (!l || even)
        //                grid.AddChild(grids[i + cols - 1]);
        //            if (!r || even)
        //                grid.AddChild(grids[i + cols]);
        //        }

        //        grid.entire = graph;
        //    }
        //}
    }
    public class GraphPath<T> where T : Graph<T>
    {
        public static Func<List<GraphPath<T>>, GraphPath<T>> PathSelector;

        public int Distance
        {
            get;
            private set;
        }
        public T Node
        {
            get;
            private set;
        }
        public List<GraphPath<T>> Previous
        {
            get;
            private set;
        }

        internal GraphPath(T node, int distance)
        {
            //this = new GraphPath<T>();
            this.Node = node;
            this.Distance = distance;
            this.Previous = new List<GraphPath<T>>();
        }

        public IEnumerable<T> BuildPath()
        {
            return BuildPath(PathSelector);
        }
        public IEnumerable<T> BuildPath(Func<List<GraphPath<T>>, GraphPath<T>> pathSelector)
        {
            Stack<T> path = new Stack<T>();
            AddNode(path, this, pathSelector);
            return path;
        }
        private static void AddNode(Stack<T> path, GraphPath<T> node, Func<List<GraphPath<T>>, GraphPath<T>> pathSelector)
        {
            path.Push(node.Node);
            if (node.Previous.Count > 0)
                if (pathSelector == null)
                    AddNode(path, node.Previous[0], null);
                else
                    AddNode(path, pathSelector(node.Previous), pathSelector);
        }
    }
    public class GraphMap4444<T> where T : Graph<T>
    {
        public float Size = 100;

        public int Col
        {
            get;
            protected set;
        }
        public int Row
        {
            get;
            protected set;
        }
        public T[] Grids
        {
            get;
            protected set;
        }
        public int GridCount
        {
            get { return Grids.Length; }
        }
        public T this[int index]
        {
            get { return Grids[index]; }
        }
        public T this[int x, int y]
        {
            get { return Grids[ToIndex(x, y)]; }
        }
        public T this[float x, float y]
        {
            get { return Grids[ToIndex(x, y)]; }
        }

        public void Build(T[] grids, int col)
        {
            int count = grids.Length;
            if (count % col != 0)
                throw new ArgumentOutOfRangeException("Grids count must equal (row * col).");
            this.Col = col;
            this.Row = count / col;
            this.Grids = grids;
            Graph<T>.BuildGraph4444(grids, col);
            OnBuild();
        }
        protected virtual void OnBuild()
        {
        }
        public int ToIndex(int x, int y)
        {
            return y * Col + x;
        }
        public int ToIndex(float x, float y)
        {
            return ToIndex((int)(x / Size), (int)(y / Size));
        }
        public void ToPosition(int index, out int x, out int y)
        {
            x = index % Col;
            y = index / Col;
        }
        public int ToPosition(float position)
        {
            return (int)(position / Size);
        }
        public void ToPosition(int index, out float x, out float y)
        {
            x = (index % Col) * Size;
            y = (index / Col) * Size;
        }
        public void ToPosition(ref float x, ref float y)
        {
            x = (int)(x / Size) * Size;
            y = (int)(x / Size) * Size;
        }
        public void ToPositionCenter(int index, out float x, out float y)
        {
            ToPosition(index, out x, out y);
            x += Size * 0.5f;
            y += Size * 0.5f;
        }
        public void ToPositionCenter(ref float x, ref float y)
        {
            x = (int)(x / Size) * Size + Size * 0.5f;
            y = (int)(x / Size) * Size + Size * 0.5f;
        }
        public void ToPositionCenter(int x, int y, out float px, out float py)
        {
            px = x;
            py = y;
            ToPositionCenter(ref px, ref py);
        }
        public IEnumerable<T> GetGridInArea(RECT area)
        {
            int x, y, w, h;
            x = ToPosition(_MATH.Nature(area.X));
            y = ToPosition(_MATH.Nature(area.Y));
            w = ToPosition(_MATH.Min(area.Right, Col)) - x;
            h = ToPosition(_MATH.Min(area.Bottom, Row)) - y;
            return Grids.GetArray(x, y, w, h, Col);
        }
    }

    public enum EDirection4
    {
        Right = 0,
        Down = 1,
        Left = 2,
        Up = 3
    }
    public enum EDirection8
    {
        Right = 0,
        RightDown = 1,
        Down = 2,
        DownLeft = 3,
        Left = 4,
        LeftUp = 5,
        Up = 6,
        UpRight = 7
    }
}

#endif