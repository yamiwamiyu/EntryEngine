using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntryEngine
{
    static partial class _RANDOM
    {
        public static Random _Random = new EntryEngine.RandomDotNet();
        
        public static int Seed
        {
            get
            {
                #if EntryBuilder
                throw new System.NotImplementedException();
                #else
                return _Random.Seed;
                #endif
            }
        }
        public static int Count
        {
            get
            {
                #if EntryBuilder
                throw new System.NotImplementedException();
                #else
                return _Random.Count;
                #endif
            }
        }
        public static void ResetRandom(int seed)
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            _Random.ResetRandom(seed);
            #endif
        }
        public static int Next()
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            return _Random.Next();
            #endif
        }
        public static double NextDouble()
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            return _Random.NextDouble();
            #endif
        }
        public static int Next(int max)
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            return _Random.Next(max);
            #endif
        }
        public static int Next(int min, int max)
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            return _Random.Next(min, max);
            #endif
        }
        public static float Next(float max)
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            return _Random.Next(max);
            #endif
        }
        public static float Next(float min, float max)
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            return _Random.Next(min, max);
            #endif
        }
        public static double Next(double max)
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            return _Random.Next(max);
            #endif
        }
        public static double Next(double min, double max)
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            return _Random.Next(min, max);
            #endif
        }
        public static int NextSign()
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            return _Random.NextSign();
            #endif
        }
        public static bool NextBool()
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            return _Random.NextBool();
            #endif
        }
        public static float NextRadian()
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            return _Random.NextRadian();
            #endif
        }
        public static float NextAngle()
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            return _Random.NextAngle();
            #endif
        }
        public static byte NextByte()
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            return _Random.NextByte();
            #endif
        }
    }
}
