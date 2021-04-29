using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntryEngine
{
	public static partial class _RANDOM
	{
		[ADefaultValue(typeof(RandomDotNet))]
		public abstract class Random
		{
            /// <summary>当前随机种子</summary>
			public abstract int Seed { get; }
            /// <summary>当前已随机的次数</summary>
			public abstract int Count { get; }
            /// <summary>重置随机</summary>
			/// <param name="seed">随机种子</param>
			public abstract void ResetRandom(int seed);
            /// <summary>随机0~int.MaxValue之间的数</summary>
			/// <returns>0 ~ int.MaxValue</returns>
			public abstract int Next();
            /// <summary>随机0~1之间的数</summary>
			/// <returns>0 ~ 1</returns>
			public abstract double NextDouble();

			protected Random()
			{
			}
			[ADeviceNew]
			public Random(int seed)
			{
				ResetRandom(seed);
			}

            /// <summary>随机0~max之间的数，包含0，不包含max</summary>
			public int Next(int max)
			{
				return (int)Next((double)max);
			}
            /// <summary>随机min~max之间的数，包含min，不包含max</summary>
			public int Next(int min, int max)
			{
				return (int)Next((double)min, (double)max);
			}
            /// <summary>随机0~max之间的数，包含0，不包含max</summary>
			public float Next(float max)
			{
				return (float)Next((double)max);
			}
            /// <summary>随机min~max之间的数，包含min，不包含max</summary>
			public float Next(float min, float max)
			{
				return (float)Next((double)min, (double)max);
			}
            /// <summary>随机0~max之间的数，包含0，不包含max</summary>
			public double Next(double max)
			{
				return NextDouble() * max;
			}
            /// <summary>随机min~max之间的数，包含min，不包含max</summary>
			public double Next(double min, double max)
			{
				return min + Next(max - min);
			}
            /// <summary>随机-1或1</summary>
			public int NextSign()
			{
				return Next() % 2 == 0 ? 1 : -1;
			}
            /// <summary>随机true或false</summary>
			public bool NextBool()
			{
				return Next() % 2 == 0;
			}
            /// <summary>随机0~360°，单位弧度</summary>
			public float NextRadian()
			{
				return Next(0, _MATH.PI_2);
			}
            /// <summary>随机0~360°，单位角度</summary>
			public float NextAngle()
			{
				return Next(0, 360);
			}
            /// <summary>随机0~255</summary>
			public byte NextByte()
			{
				return (byte)Next(byte.MinValue, byte.MaxValue + 1);
			}
		}
	}
	public sealed class RandomDotNet : _RANDOM.Random
	{
		private int seed;
		private int count;
		private Random random = new Random();

		public override int Seed
		{
			get { return seed; }
		}
		public override int Count
		{
			get { return count; }
		}

		public RandomDotNet()
			: base(new Random().Next())
		{
		}
		public RandomDotNet(int seed)
			: base(seed)
		{
		}

		public override void ResetRandom(int seed)
		{
			this.seed = seed;
			this.count = 0;
			random = new Random(seed);
		}
		public override int Next()
		{
			count++;
			return random.Next();
		}
		public override double NextDouble()
		{
			count++;
			return random.NextDouble();
		}
	}
}
