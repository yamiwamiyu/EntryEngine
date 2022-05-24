#if CLIENT

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EntryEngine.UI;

namespace EntryEngine
{
    public interface IMobile
    {
        float X { get; set; }
        float Y { get; set; }
    }
    public class MOBILE : IMobile
    {
        public VECTOR2 P;
        public float X
        {
            get { return P.X; }
            set { P.X = value; }
        }
        public float Y
        {
            get { return P.Y; }
            set { P.Y = value; }
        }
        public MOBILE() { }
        public MOBILE(float x, float y) { this.P.X = x; this.P.Y = y; }
        public MOBILE(VECTOR2 p) { this.P = p; }

        /// <summary>from根据速度朝to匀速移动</summary>
        /// <param name="speed">移动速度，单位像素/秒</param>
        public static IEnumerable<ICoroutine> UniformMoveBySpeed(IMobile from, IMobile to, float speed)
        {
            while (true)
            {
                float y = to.Y - from.Y;
                float x = to.X - from.X;
                float radian = (float)Math.Atan2(y, x);
                float distance = (float)Math.Sqrt(x * x + y * y);

                float moved = speed * GameTime.Time.ElapsedSecond;
                bool over = distance <= moved;
                if (over)
                    moved = distance;
                from.X += (float)Math.Cos(radian) * moved;
                from.Y += (float)Math.Sin(radian) * moved;
                if (over)
                    break;
                yield return null;
            }
        }
        /// <summary>from根据时间朝to匀速移动</summary>
        /// <param name="second">移动时间，单位秒，移动速度将自动计算</param>
        public static IEnumerable<ICoroutine> UniformMoveByTime(IMobile from, IMobile to, float second)
        {
            if (second == 0)
                throw new ArgumentException("移动时间必须不为0");
            double x = from.X - to.X;
            double y = from.Y - to.Y;
            float distance = (float)Math.Sqrt(x * x + y * y);
            return UniformMoveBySpeed(from, to, distance / second);
        }
        /// <summary>from根据速度朝to加速靠近，减速停下</summary>
        /// <param name="second1">加速时间，单位秒，超过加速时间将开始减速最终停下，加速度将自动计算</param>
        /// <param name="second2">移动总时间，单位秒，加速度将自动计算</param>
        public static IEnumerable<ICoroutine> CloseToBySpeed(IMobile from, IMobile to, float second1, float second2)
        {
            VECTOR2 origin = new VECTOR2(from.X, from.Y);
            VECTOR2 current;
            double x = to.X - from.X;
            double y = to.Y - from.Y;
            // 两点距离
            float distance = (float)Math.Sqrt(x * x + y * y);
            // 移动方向
            float radian = (float)Math.Atan2(y, x);
            // v0 * t + 1/2 * a * t * t
            // 加速运动占总时间的百分比
            float percent = second1 / second2;
            float d = distance * percent;
            // 根据[加速运动移动距离]，[加速运动时间]计算出加速度
            float a = d / (0.5f * second1 * second1);
            // 经过加速或减速阶段的时间
            float time = 0;
            while (time < second1)
            {
                float elapsed = GameTime.Time.ElapsedSecond;
                if (time + elapsed > second1)
                {
                    elapsed = second1 - time;
                }
                time += elapsed;
                CIRCLE.ParametricEquation(ref origin, 0.5f * a * time * time, radian, out current);
                from.X = current.X;
                from.Y = current.Y;
                yield return null;
            }
            // 减速运动时间
            second2 = second2 - second1;
            percent = 1 - percent;
            // 当前移动速度
            float v0 = a * second1;
            // 根据[减速运动移动距离]，[减速运动时间]计算出加速度
            a = (distance * percent - v0 * second2) / (0.5f * second2 * second2);
            time = 0;
            while (time < second2)
            {
                float elapsed = GameTime.Time.ElapsedSecond;
                if (time + elapsed >= second2)
                {
                    elapsed = second2 - time;
                }
                time += elapsed;
                CIRCLE.ParametricEquation(ref origin, d + v0 * time + 0.5f * a * time * time, radian, out current);
                from.X = current.X;
                from.Y = current.Y;
                yield return null;
            }
        }
    }
    /// <summary>二维向量</summary>
    [AReflexible]public struct VECTOR2 : IEquatable<VECTOR2>, IMobile
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
                VECTOR2 other = (VECTOR2)obj;
                result = this.X == other.X && this.Y == other.Y;
            }
            return result;
        }
        public override int GetHashCode()
        {
            return this.X.GetHashCode() + this.Y.GetHashCode();
        }
        public bool IsZero()
        {
            return X == 0 && Y == 0;
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
        /// <summary>两点间的距离，数字太大时，计算精度不够可能会引发其它BUG</summary>
		public static void Distance(ref VECTOR2 value1, ref VECTOR2 value2, out float result)
		{
			float num = value1.X - value2.X;
			float num2 = value1.Y - value2.Y;
			float num3 = num * num + num2 * num2;
            result = (float)Math.Sqrt(num3);
		}
		public static float DistanceSquared(VECTOR2 value1, VECTOR2 value2)
		{
			float result;
			DistanceSquared(ref value1, ref value2, out result);
			return result;
		}
        /// <summary>两点间距离的平方，开根就是两点间距离，可用于简单比较距离远近，但数字太大时，计算精度不够可能会引发其它BUG</summary>
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
        /// <summary>x1 * x2 + y1 * y2</summary>
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
        /// <summary>将点设定在0~1的范围内，1 / sqrt(x * x + y * y)</summary>
		public static void Normalize(ref VECTOR2 value, out VECTOR2 result)
		{
			float num = value.X * value.X + value.Y * value.Y;
            float num2 = 1f / (float)Math.Sqrt(num);
			result.X = value.X * num2;
			result.Y = value.Y * num2;
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
        /// <summary>力的合成</summary>
        /// <param name="v1">分力1</param>
        /// <param name="v2">分力2</param>
        /// <param name="v3">合力</param>
        public static void Combine(ref VECTOR2 v1, ref VECTOR2 v2, out VECTOR2 v3)
        {
            float a1 = (float)Math.Atan2(v1.Y, v1.X) * _MATH.R2D;
            float a2 = (float)Math.Atan2(v2.Y, v2.X) * _MATH.R2D;

            float speed1 = v1.Length();
            float speed2 = v2.Length();

            // 夹角
            float a = _MATH.Closewise(a2, a1);
            float r = a * _MATH.D2R;
            float cos = (float)Math.Cos(r);
            float sin = (float)Math.Sin(r);

            float direction = a1;
            float speed;
            if (a != 0)
            {
                if (speed1 != 0)
                    direction = a2 + (float)Math.Atan2(speed1 * sin, speed2 + speed1 * cos) * _MATH.R2D;
                else
                    direction = a2;
            }
            speed = (float)Math.Sqrt(speed1 * speed1 + speed2 * speed2 + 2 * speed1 * speed2 * cos);

            float radian = direction * _MATH.D2R;
            v3.X = (float)Math.Cos(radian) * speed;
            v3.Y = (float)Math.Sin(radian) * speed;
        }
        /// <summary>万有引力</summary>
        /// <param name="p">受力点坐标</param>
        /// <param name="power">受力点原始运动方向和速度</param>
        /// <param name="target">引力点</param>
        /// <param name="force">引力大小</param>
        /// <param name="v3">被万有引力吸引后的运动方向和速度</param>
        public static void Gravitation(ref VECTOR2 p, ref VECTOR2 power, ref VECTOR2 target, float force, out VECTOR2 v3)
        {
            float radian = (float)Math.Atan2(target.Y - p.Y, target.X - p.X);
            float angle = radian * _MATH.R2D;

            float speed1 = power.Length();

            // 夹角
            float a1 = (float)Math.Atan2(power.Y, power.X) * _MATH.R2D;
            float a = _MATH.Closewise(angle, a1);
            float r = a * _MATH.D2R;
            float cos = (float)Math.Cos(r);
            float sin = (float)Math.Sin(r);

            float direction = a1;
            float speed;
            if (a != 0)
            {
                if (speed1 != 0)
                    direction = angle + (float)Math.Atan2(speed1 * sin, force + speed1 * cos) * _MATH.R2D;
                else
                    direction = angle;
            }
            speed = (float)Math.Sqrt(speed1 * speed1 + force * force + 2 * speed1 * force * cos);

            radian = direction * _MATH.D2R;
            v3.X = (float)Math.Cos(radian) * speed;
            v3.Y = (float)Math.Sin(radian) * speed;
        }
		public static VECTOR2 Lerp(VECTOR2 value1, VECTOR2 value2, float amount)
		{
			VECTOR2 result;
			Lerp(ref value1, ref value2, amount, out result);
			return result;
		}
        /// <summary>点1到点2的线段上的一点</summary>
        /// <param name="amount">线段上的百分比</param>
		public static void Lerp(ref VECTOR2 value1, ref VECTOR2 value2, float amount, out VECTOR2 result)
		{
			result.X = value1.X + (value2.X - value1.X) * amount;
			result.Y = value1.Y + (value2.Y - value1.Y) * amount;
		}
        /// <summary>质心 / 重心：三条中线的交点</summary>
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
        /// <summary>质心权重 value1: w = 1 - u - v</summary>
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
		public static void Negate(ref VECTOR2 value, out VECTOR2 result)
		{
			result.X = -value.X;
			result.Y = -value.Y;
		}
		public static void Add(ref VECTOR2 value1, ref VECTOR2 value2, out VECTOR2 result)
		{
			result.X = value1.X + value2.X;
			result.Y = value1.Y + value2.Y;
		}
		public static void Subtract(ref VECTOR2 value1, ref VECTOR2 value2, out VECTOR2 result)
		{
			result.X = value1.X - value2.X;
			result.Y = value1.Y - value2.Y;
		}
		public static void Multiply(ref VECTOR2 value1, ref VECTOR2 value2, out VECTOR2 result)
		{
			result.X = value1.X * value2.X;
			result.Y = value1.Y * value2.Y;
		}
		public static void Multiply(ref VECTOR2 value1, float scaleFactor, out VECTOR2 result)
		{
			result.X = value1.X * scaleFactor;
			result.Y = value1.Y * scaleFactor;
		}
		public static void Divide(ref VECTOR2 value1, ref VECTOR2 value2, out VECTOR2 result)
		{
			result.X = value1.X / value2.X;
			result.Y = value1.Y / value2.Y;
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
            float num = 1f / divider;
            return new VECTOR2(value1.X * num, value1.Y * num);
        }


        float IMobile.X
        {
            get { return this.X; }
            set { this.X = value; }
        }
        float IMobile.Y
        {
            get { return this.Y; }
            set { this.Y = value; }
        }
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
        public static void Add(ref VECTOR3 value1, ref VECTOR3 value2, out VECTOR3 result)
        {
            result.X = value1.X + value2.X;
            result.Y = value1.Y + value2.Y;
            result.Z = value1.Z + value2.Z;
        }
        public static void Subtract(ref VECTOR3 value1, ref VECTOR3 value2, out VECTOR3 result)
        {
            result.X = value1.X - value2.X;
            result.Y = value1.Y - value2.Y;
            result.Z = value1.Z - value2.Z;
        }
        public static void Multiply(ref VECTOR3 value1, ref VECTOR3 value2, out VECTOR3 result)
        {
            result.X = value1.X * value2.X;
            result.Y = value1.Y * value2.Y;
            result.Z = value1.Z * value2.Z;
        }
        public static void Multiply(ref VECTOR3 value1, float scaleFactor, out VECTOR3 result)
        {
            result.X = value1.X * scaleFactor;
            result.Y = value1.Y * scaleFactor;
            result.Z = value1.Z * scaleFactor;
        }
        public static void Divide(ref VECTOR3 value1, ref VECTOR3 value2, out VECTOR3 result)
        {
            result.X = value1.X / value2.X;
            result.Y = value1.Y / value2.Y;
            result.Z = value1.Z / value2.Z;
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
        public VECTOR4(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
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
    }
    /// <summary>三维矩阵变换</summary>
    public struct MATRIX : IEquatable<MATRIX>
    {
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
        /// <summary>转置矩阵</summary>
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
        public void Inverse()
        {
            Invert(ref this, out this);
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
            Negate(ref matrix1, out matrix1);
            return matrix1;
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
            Add(ref matrix1, ref matrix2, out result);
            return result;
        }
        public static MATRIX operator -(MATRIX matrix1, MATRIX matrix2)
        {
            MATRIX result;
            Subtract(ref matrix1, ref matrix2, out result);
            return result;
        }
        public static MATRIX operator *(MATRIX matrix1, MATRIX matrix2)
        {
            MATRIX result;
            Multiply(ref matrix1, ref matrix2, out result);
            return result;
        }
        public static MATRIX operator *(MATRIX matrix, float scaleFactor)
        {
            MATRIX result;
            Multiply(ref matrix, scaleFactor, out result);
            return result;
        }
        public static MATRIX operator *(float scaleFactor, MATRIX matrix)
        {
            MATRIX result;
            Multiply(ref matrix, scaleFactor, out result);
            return result;
        }
        public static MATRIX operator /(MATRIX matrix1, MATRIX matrix2)
        {
            MATRIX result;
            Divide(ref matrix1, ref matrix2, out result);
            return result;
        }
        public static MATRIX operator /(MATRIX matrix1, float divider)
        {
            MATRIX result;
            Divide(ref matrix1, divider, out result);
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
                //* MATRIX2x3.CreateTranslation(pivotX, pivotY)
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
        public static COLOR Black
        {
            get
            {
                return new COLOR(4278190080u);
            }
        }
        public static COLOR Blue
        {
            get
            {
                return new COLOR(4278190335u);
            }
        }
        public static COLOR CornflowerBlue
        {
            get
            {
                return new COLOR(4284782061u);
            }
        }
        public static COLOR DeepSkyBlue
        {
            get
            {
                return new COLOR(4278239231u);
            }
        }
        public static COLOR Gold
        {
            get
            {
                return new COLOR(4294956800u);
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
        public static COLOR LawnGreen
        {
            get
            {
                return new COLOR(4286381056u);
            }
        }
        public static COLOR LightGray
        {
            get
            {
                return new COLOR(4292072403u);
            }
        }
        public static COLOR Lime
        {
            get
            {
                return new COLOR(4278255360u);
            }
        }
        public static COLOR Orange
        {
            get
            {
                return new COLOR(4294944000u);
            }
        }
        public static COLOR Pink
        {
            get
            {
                return new COLOR(4294951115u);
            }
        }
        public static COLOR Red
        {
            get
            {
                return new COLOR(4294901760u);
            }
        }
        public static COLOR Silver
        {
            get
            {
                return new COLOR(4290822336u);
            }
        }
        public static COLOR White
        {
            get
            {
                return new COLOR(4294967295u);
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
        public VECTOR4 ToFloat()
        {
            return new VECTOR4(
                R * BYTE_TO_FLOAT,
                G * BYTE_TO_FLOAT,
                B * BYTE_TO_FLOAT,
                A * BYTE_TO_FLOAT);
        }
        public byte[] ToRGBA()
        {
            return new byte[] { R, G, B, A };
        }
        /// <summary>R,G,B,A</summary>
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
                return new VECTOR2(this.X + this.Width * 0.5f, this.Y + this.Height * 0.5f);
            }
            set
            {
                this.X += value.X - (this.X + this.Width * 0.5f);
                this.Y += value.Y - (this.Y + this.Height * 0.5f);
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
            //return value.X < this.X + this.Width && this.X < value.X + value.Width && value.Y < this.Y + this.Height && this.Y < value.Y + value.Height;
            return Intersects(ref value);
        }
        public bool Intersects(ref RECT value)
        {
            
            float num1 = this.X + this.Width;
            float num2 = value.X + value.Width;
            float num3 = this.Y + this.Height;
            float num4 = value.Y + value.Height;
            bool bool1 = value.X < num1;
            bool bool2 = this.X < num2;
            bool bool3 = value.Y < num3;
            bool bool4 = this.Y < num4;
            /* hack: 以下测试代码会导致bool4为false的情况下bool5返回true
             * RECT rect1 = new RECT()
            {
                X = -4900,
                Y = 350,
                Width = 150,
                Height = 100,
            };
            RECT rect2 = new RECT()
            {
                X = -4916.54248f,
                Y = 168.000015f,
                Width = 60,
                Height = 182,
            };
            bool s = rect1.Intersects(ref rect2);
             */
            //bool bool5 = value.X < this.X + this.Width && this.X < value.X + value.Width && value.Y < this.Y + this.Height && this.Y < value.Y + value.Height;
            //return bool5;
            return bool1 && bool2 && bool3 && bool4;
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
            return R == other.R && C.X == other.C.X && C.Y == other.C.Y;
        }

		public static void ParametricEquation(ref CIRCLE circle, float radian, out VECTOR2 result)
		{
            float x = circle.C.X;
            float y = circle.C.Y;
            result.X = (float)Math.Cos(radian) * circle.R;
            result.Y = (float)Math.Sin(radian) * circle.R;
            result.X += x;
            result.Y += y;
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
            result.X = (float)Math.Cos(radian) * radius;
            result.Y = (float)Math.Sin(radian) * radius;
			result.X += x;
			result.Y += y;
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
        /// <summary>广度优先循环图节点</summary>
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

    public enum EDirection4 : byte
    {
        Right = 0,
        Down = 1,
        Left = 2,
        Up = 3
    }
    public enum EDirection8 : byte
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