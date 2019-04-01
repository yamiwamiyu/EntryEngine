using __System.Collections.Generic;
using __System.Text;
using System;

namespace __System.Linq
{
#if !DEBUG
    public static class Enumerable
	{
        public static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            foreach (TSource current in source)
            {
                if (!predicate(current))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool Any<TSource>(this IEnumerable<TSource> source)
        {
            IEnumerator<TSource> enumerator = source.GetEnumerator();
            bool result = enumerator.MoveNext();
            enumerator.Dispose();
            return result;
        }
        public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            foreach (TSource current in source)
            {
                if (predicate(current))
                {
                    return true;
                }
            }
            return false;
        }
        public static IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            foreach (TSource current in first)
            {
                yield return current;
            }
            foreach (TSource current2 in second)
            {
                yield return current2;
            }
            yield break;
        }
        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value)
        {
            return source.Contains(value, null);
        }
        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource> comparer)
        {
            if (comparer == null)
            {
                comparer = EqualityComparer<TSource>.Default;
            }
            foreach (TSource current in source)
            {
                if (comparer.Equals(current, value))
                {
                    return true;
                }
            }
            return false;
        }
        public static int Count<TSource>(this IEnumerable<TSource> source)
        {
            ICollection<TSource> collection = source as ICollection<TSource>;
            if (collection != null)
            {
                return collection.Count;
            }
            //ICollection collection2 = source as ICollection;
            //if (collection2 != null)
            //{
            //    return collection2.Count;
            //}
            int num = 0;
            IEnumerator<TSource> enumerator = source.GetEnumerator();
            while (enumerator.MoveNext())
                num++;
            return num;
        }
        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            int num = 0;
            foreach (TSource current in source)
            {
                if (predicate(current))
                {
                    num++;
                }
            }
            return num;
        }
        public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source)
        {
            return Enumerable.DistinctIterator<TSource>(source, null);
        }
        public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer)
        {
            return Enumerable.DistinctIterator<TSource>(source, comparer);
        }
        private static IEnumerable<TSource> DistinctIterator<TSource>(IEnumerable<TSource> source, IEqualityComparer<TSource> comparer)
        {
            Set<TSource> set = new Set<TSource>(comparer);
            foreach (TSource current in source)
            {
                if (set.Add(current))
                {
                    yield return current;
                }
            }
            yield break;
        }
        public static TSource First<TSource>(this IEnumerable<TSource> source)
        {
            IList<TSource> list = source as IList<TSource>;
            if (list != null)
            {
                if (list.Count > 0)
                {
                    return list[0];
                }
            }
            else
            {
                IEnumerator<TSource> enumerator = source.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    return enumerator.Current;
                }
            }
            throw new InvalidOperationException("No Elements");
        }
        public static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            foreach (TSource current in source)
            {
                if (predicate(current))
                {
                    return current;
                }
            }
            throw new InvalidOperationException("No Elements");
        }
        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            IList<TSource> list = source as IList<TSource>;
            if (list != null)
            {
                if (list.Count > 0)
                {
                    return list[0];
                }
            }
            else
            {
                IEnumerator<TSource> enumerator = source.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    return enumerator.Current;
                }
            }
            return default(TSource);
        }
        public static TSource Last<TSource>(this IEnumerable<TSource> source)
        {
            IList<TSource> list = source as IList<TSource>;
            if (list != null)
            {
                int count = list.Count;
                if (count > 0)
                {
                    return list[count - 1];
                }
            }
            else
            {
                IEnumerator<TSource> enumerator = source.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    TSource current;
                    do
                    {
                        current = enumerator.Current;
                    }
                    while (enumerator.MoveNext());
                    return current;
                }
            }
            throw new InvalidOperationException("No Elements");
        }
        public static TSource Last<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            TSource result = default(TSource);
            bool flag = false;
            foreach (TSource current in source)
            {
                if (predicate(current))
                {
                    result = current;
                    flag = true;
                }
            }
            if (flag)
            {
                return result;
            }
            throw new InvalidOperationException("No Elements");
        }
        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            foreach (TSource current in source)
            {
                if (predicate(current))
                {
                    return current;
                }
            }
            return default(TSource);
        }
        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            IList<TSource> list = source as IList<TSource>;
            if (list != null)
            {
                int count = list.Count;
                if (count > 0)
                {
                    return list[count - 1];
                }
            }
            else
            {
                IEnumerator<TSource> enumerator = source.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    TSource current;
                    do
                    {
                        current = enumerator.Current;
                    }
                    while (enumerator.MoveNext());
                    return current;
                }
            }
            return default(TSource);
        }
        public static IEnumerable<int> Range(int start, int count)
        {
            for (int i = start, n = start + count; i < n; i++)
                yield return i;
        }
        public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            foreach (var item in source)
                yield return selector(item);
        }
        public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            foreach (TSource current in source)
            {
                foreach (TResult current2 in selector(current))
                {
                    yield return current2;
                }
            }
            yield break;
        }
        public static int Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
        {
            int value = 0;
            foreach (var v in source.Select(selector))
                value += v;
            return value;
        }
        public static float Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
        {
            float value = 0;
            foreach (var v in source.Select(selector))
                value += v;
            return value;
        }
        public static IEnumerable<TSource> Skip<TSource>(this IEnumerable<TSource> source, int count)
        {
            IEnumerator<TSource> enumerator = source.GetEnumerator();
            while (count > 0 && enumerator.MoveNext())
            {
                count--;
            }
            if (count <= 0)
            {
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
            }
            yield break;
        }
        public static IEnumerable<TSource> Take<TSource>(this IEnumerable<TSource> source, int count)
        {
            if (count > 0)
            {
                foreach (TSource current in source)
                {
                    yield return current;
                    if (--count == 0)
                    {
                        break;
                    }
                }
            }
            yield break;
        }
        public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source)
        {
            TSource[] array = null;
            int num = 0;
            ICollection<TSource> collection = source as ICollection<TSource>;
            if (collection != null)
            {
                num = collection.Count;
                if (num > 0)
                {
                    array = new TSource[num];
                    collection.CopyTo(array, 0);
                }
            }
            else
            {
                foreach (TSource current in source)
                {
                    if (array == null)
                    {
                        array = new TSource[4];
                    }
                    else
                    {
                        if (array.Length == num)
                        {
                            TSource[] array2 = new TSource[num * 2];
                            Array.Copy(array, 0, array2, 0, num);
                            array = array2;
                        }
                    }
                    array[num] = current;
                    num++;
                }
            }
            if (array == null)
                array = new TSource[0];
            else
            {
                if (num < array.Length)
                {
                    TSource[] array2 = new TSource[num];
                    Array.Copy(array, array2, num);
                    array = array2;
                }
            }
            return array;
        }
        public static List<TSource> ToList<TSource>(this IEnumerable<TSource> source)
        {
            return new List<TSource>(source);
        }
        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.ToDictionary(keySelector, null);
        }
        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            Dictionary<TKey, TSource> dictionary = new Dictionary<TKey, TSource>(comparer);
            foreach (TSource current in source)
            {
                dictionary.Add(keySelector(current), current);
            }
            return dictionary;
        }
        public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            foreach (var item in source)
                if (predicate(item))
                    yield return item;
        }
	}
    internal class Set<TElement>
    {
        internal struct Slot
        {
            internal int hashCode;
            internal TElement value;
            internal int next;
        }
        private int[] buckets;
        private Set<TElement>.Slot[] slots;
        private int count;
        private int freeList;
        private IEqualityComparer<TElement> comparer;
        public Set()
            : this(null)
        {
        }
        public Set(IEqualityComparer<TElement> comparer)
        {
            if (comparer == null)
            {
                comparer = EqualityComparer<TElement>.Default;
            }
            this.comparer = comparer;
            this.buckets = new int[7];
            this.slots = new Set<TElement>.Slot[7];
            this.freeList = -1;
        }
        public bool Add(TElement value)
        {
            return !this.Find(value, true);
        }
        public bool Contains(TElement value)
        {
            return this.Find(value, false);
        }
        public bool Remove(TElement value)
        {
            int num = this.InternalGetHashCode(value);
            int num2 = num % this.buckets.Length;
            int num3 = -1;
            for (int i = this.buckets[num2] - 1; i >= 0; i = this.slots[i].next)
            {
                if (this.slots[i].hashCode == num && this.comparer.Equals(this.slots[i].value, value))
                {
                    if (num3 < 0)
                    {
                        this.buckets[num2] = this.slots[i].next + 1;
                    }
                    else
                    {
                        this.slots[num3].next = this.slots[i].next;
                    }
                    this.slots[i].hashCode = -1;
                    this.slots[i].value = default(TElement);
                    this.slots[i].next = this.freeList;
                    this.freeList = i;
                    return true;
                }
                num3 = i;
            }
            return false;
        }
        private bool Find(TElement value, bool add)
        {
            int num = this.InternalGetHashCode(value);
            for (int i = this.buckets[num % this.buckets.Length] - 1; i >= 0; i = this.slots[i].next)
            {
                if (this.slots[i].hashCode == num && this.comparer.Equals(this.slots[i].value, value))
                {
                    return true;
                }
            }
            if (add)
            {
                int num2;
                if (this.freeList >= 0)
                {
                    num2 = this.freeList;
                    this.freeList = this.slots[num2].next;
                }
                else
                {
                    if (this.count == this.slots.Length)
                    {
                        this.Resize();
                    }
                    num2 = this.count;
                    this.count++;
                }
                int num3 = num % this.buckets.Length;
                this.slots[num2].hashCode = num;
                this.slots[num2].value = value;
                this.slots[num2].next = this.buckets[num3] - 1;
                this.buckets[num3] = num2 + 1;
            }
            return false;
        }
        private void Resize()
        {
            int num = this.count * 2 + 1;
            int[] array = new int[num];
            Set<TElement>.Slot[] array2 = new Set<TElement>.Slot[num];
            Array.Copy(this.slots, 0, array2, 0, this.count);
            for (int i = 0; i < this.count; i++)
            {
                int num2 = array2[i].hashCode % num;
                array2[i].next = array[num2] - 1;
                array[num2] = i + 1;
            }
            this.buckets = array;
            this.slots = array2;
        }
        internal int InternalGetHashCode(TElement value)
        {
            if (value != null)
            {
                return this.comparer.GetHashCode(value) & 2147483647;
            }
            return 0;
        }
    }
#endif
}
