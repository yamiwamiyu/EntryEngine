using System;
using System.Collections;

namespace __System.Collections.Generic
{
    public interface IEnumerable<out T>
    {
        [AInvariant][ANonOptimize]IEnumerator<T> GetEnumerator();
    }
    public interface IEnumerator<out T> : IDisposable
    {
        [AInvariant]T Current { get; }
        [AInvariant]bool MoveNext();
        void Reset();
    }
    public interface ICollection<T> : IEnumerable<T>
    {
        [AInvariant]int Count { get; }
        void CopyTo(T[] array, int arrayIndex);
    }
    public interface IList<T> : ICollection<T>
    {
        T this[int index] { get; set; }
        //void Add(T item);
        void RemoveAt(int index);
    }

    public interface IDictionary<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>
    {
        TValue this[TKey key]
        {
            get;
            set;
        }
        ICollection<TKey> Keys
        {
            get;
        }
        ICollection<TValue> Values
        {
            get;
        }
        bool ContainsKey(TKey key);
        [AInvariant]void Add(TKey key, TValue value);
        bool Remove(TKey key);
        bool TryGetValue(TKey key, out TValue value);
    }
    public interface IEqualityComparer<in T>
    {
        bool Equals(T x, T y);
        int GetHashCode(T obj);
    }
    public interface IComparer<in T>
    {
        int Compare(T x, T y);
    }

    public abstract class EqualityComparer<T> : IEqualityComparer<T>
    {
#if !DEBUG
        //static Type[] PrimitiveTypes = new Type[]
        //{
        //    typeof(bool), typeof(byte), typeof(sbyte),
        //    typeof(ushort), typeof(short), typeof(char),
        //    typeof(uint), typeof(int), typeof(float),
        //    typeof(ulong), typeof(long), typeof(double),
        //};
#endif
        private static EqualityComparer<T> defaultComparer;
        public static EqualityComparer<T> Default
        {
            get
            {
                EqualityComparer<T> equalityComparer = EqualityComparer<T>.defaultComparer;
                if (equalityComparer == null)
                {
                    equalityComparer = EqualityComparer<T>.CreateComparer();
                    EqualityComparer<T>.defaultComparer = equalityComparer;
                }
                return equalityComparer;
            }
        }
        private static EqualityComparer<T> CreateComparer()
        {
            return new ObjectEqualityComparer<T>();
        }
        public abstract bool Equals(T x, T y);
        public abstract int GetHashCode(T obj);
        internal virtual int IndexOf(T[] array, T value, int startIndex, int count)
        {
            int num = startIndex + count;
            for (int i = startIndex; i < num; i++)
            {
                if (this.Equals(array[i], value))
                {
                    return i;
                }
            }
            return -1;
        }
        internal virtual int LastIndexOf(T[] array, T value, int startIndex, int count)
        {
            int num = startIndex - count + 1;
            for (int i = startIndex; i >= num; i--)
            {
                if (this.Equals(array[i], value))
                {
                    return i;
                }
            }
            return -1;
        }
    }
    internal class BoolComparer : EqualityComparer<bool>
    {
        public override bool Equals(bool x, bool y)
        {
            return x == y;
        }
        public override int GetHashCode(bool obj)
        {
            return obj ? 1 : 0;
        }
        internal override int IndexOf(bool[] array, bool value, int startIndex, int count)
        {
            int num = startIndex + count;
            for (int j = startIndex; j < num; j++)
            {
                if (array[j] == value)
                {
                    return j;
                }
            }
            return -1;
        }
        internal override int LastIndexOf(bool[] array, bool value, int startIndex, int count)
        {
            int num = startIndex - count + 1;
            for (int j = startIndex; j >= num; j--)
            {
                if (array[j] == value)
                {
                    return j;
                }
            }
            return -1;
        }
        public override bool Equals(object obj)
        {
            return obj is ObjectEqualityComparer<BoolComparer>;
        }
        public override int GetHashCode()
        {
            return 0;
        }
    }
    internal class ByteComparer : EqualityComparer<byte>
    {
        public override bool Equals(byte x, byte y)
        {
            return x == y;
        }
        public override int GetHashCode(byte obj)
        {
            return obj;
        }
        internal override int IndexOf(byte[] array, byte value, int startIndex, int count)
        {
            int num = startIndex + count;
            for (int j = startIndex; j < num; j++)
            {
                if (array[j] == value)
                {
                    return j;
                }
            }
            return -1;
        }
        internal override int LastIndexOf(byte[] array, byte value, int startIndex, int count)
        {
            int num = startIndex - count + 1;
            for (int j = startIndex; j >= num; j--)
            {
                if (array[j] == value)
                {
                    return j;
                }
            }
            return -1;
        }
        public override bool Equals(object obj)
        {
            return obj is ObjectEqualityComparer<ByteComparer>;
        }
        public override int GetHashCode()
        {
            return 1;
        }
    }
    internal class SByteComparer : EqualityComparer<sbyte>
    {
        public override bool Equals(sbyte x, sbyte y)
        {
            return x == y;
        }
        public override int GetHashCode(sbyte obj)
        {
            return obj;
        }
        internal override int IndexOf(sbyte[] array, sbyte value, int startIndex, int count)
        {
            int num = startIndex + count;
            for (int j = startIndex; j < num; j++)
            {
                if (array[j] == value)
                {
                    return j;
                }
            }
            return -1;
        }
        internal override int LastIndexOf(sbyte[] array, sbyte value, int startIndex, int count)
        {
            int num = startIndex - count + 1;
            for (int j = startIndex; j >= num; j--)
            {
                if (array[j] == value)
                {
                    return j;
                }
            }
            return -1;
        }
        public override bool Equals(object obj)
        {
            return obj is ObjectEqualityComparer<SByteComparer>;
        }
        public override int GetHashCode()
        {
            return 2;
        }
    }
    internal class ObjectEqualityComparer<T> : EqualityComparer<T>
    {
        public override bool Equals(T x, T y)
        {
            if (x != null)
            {
                //return y != null && x.Equals(y);
                return y != null && x.Equals(y);
            }
            return y == null;
        }
        public override int GetHashCode(T obj)
        {
            if (obj == null)
            {
                return 0;
            }
            return obj.GetHashCode();
        }
        internal override int IndexOf(T[] array, T value, int startIndex, int count)
        {
            int num = startIndex + count;
            if (value == null)
            {
                for (int i = startIndex; i < num; i++)
                {
                    if (array[i] == null)
                    {
                        return i;
                    }
                }
            }
            else
            {
                for (int j = startIndex; j < num; j++)
                {
                    if (array[j] != null && array[j].Equals(value))
                    {
                        return j;
                    }
                }
            }
            return -1;
        }
        internal override int LastIndexOf(T[] array, T value, int startIndex, int count)
        {
            int num = startIndex - count + 1;
            if (value == null)
            {
                for (int i = startIndex; i >= num; i--)
                {
                    if (array[i] == null)
                    {
                        return i;
                    }
                }
            }
            else
            {
                for (int j = startIndex; j >= num; j--)
                {
                    if (array[j] != null && array[j].Equals(value))
                    {
                        return j;
                    }
                }
            }
            return -1;
        }
        public override bool Equals(object obj)
        {
            return obj is ObjectEqualityComparer<T>;
        }
        public override int GetHashCode()
        {
            return GetType().Name.GetHashCode();
        }
    }

    public abstract class Comparer<T> : IComparer<T>
    {
        private static Comparer<T> defaultComparer;
        public static Comparer<T> Default
        {
            get
            {
                Comparer<T> comparer = Comparer<T>.defaultComparer;
                if (comparer == null)
                {
                    comparer = Comparer<T>.CreateComparer();
                    Comparer<T>.defaultComparer = comparer;
                }
                return comparer;
            }
        }
        public static Comparer<T> Create(Comparison<T> comparison)
        {
            return new ComparisonComparer<T>(comparison);
        }
        private static Comparer<T> CreateComparer()
        {
            return new ObjectComparer<T>();
        }
        public abstract int Compare(T x, T y);
    }
    internal class ComparisonComparer<T> : Comparer<T>
    {
        private readonly Comparison<T> _comparison;
        public ComparisonComparer(Comparison<T> comparison)
        {
            this._comparison = comparison;
        }
        public override int Compare(T x, T y)
        {
            return this._comparison(x, y);
        }
    }
    internal class ObjectComparer<T> : Comparer<T>
    {
        public override int Compare(T x, T y)
        {
            if (object.Equals(x, y))
                return 0;
            if (x == null)
                return -1;
            if (y == null)
                return 1;
            IComparable<T> comparable = x as IComparable<T>;
            if (comparable != null)
                return comparable.CompareTo(y);
            throw new Exception();
        }
    }

    public class List<T> : IList<T>
    {
        public struct Enumerator : IEnumerator<T>
        {
            private List<T> list;
            private int index;
            private T current;
            public T Current
            {
                get
                {
                    return this.current;
                }
            }
            internal Enumerator(List<T> list)
            {
                this.list = list;
                this.index = 0;
                this.current = default(T);
            }
            public void Dispose()
            {
            }
            public bool MoveNext()
            {
                List<T> list = this.list;
                if (this.index < list._size)
                {
                    this.current = list._items[this.index];
                    this.index++;
                    return true;
                }
                return this.MoveNextRare();
            }
            private bool MoveNextRare()
            {
                this.index = this.list._size + 1;
                this.current = default(T);
                return false;
            }
            public void Reset()
            {
                this.index = 0;
                this.current = default(T);
            }
        }

        private T[] _items;
        private int _size;
        private static readonly T[] _emptyArray = new T[0];

        public int Capacity
        {
            get
            {
                return _items.Length;
            }
            set
            {
                if (value != _items.Length)
                {
                    if (value > 0)
                    {
                        T[] array = new T[value];
                        if (_size > 0)
                        {
                            Array.Copy(_items, 0, array, 0, _size);
                        }
                        _items = array;
                        return;
                    }
                    _items = _emptyArray;
                }
            }
        }
        public int Count
        {
            get
            {
                return _size;
            }
        }
        public T this[int index]
        {
            get
            {
                return _items[index];
            }
            set
            {
                _items[index] = value;
            }
        }
        public List()
        {
            _items = _emptyArray;
        }
        public List(int capacity)
        {
            if (capacity == 0)
            {
                _items = _emptyArray;
                return;
            }
            _items = new T[capacity];
        }
        public List(IEnumerable<T> collection)
        {
            ICollection<T> collection2 = collection as ICollection<T>;
            if (collection2 == null)
            {
                this._size = 0;
                this._items = List<T>._emptyArray;
                IEnumerator<T> enumerator = collection.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    this.Add(enumerator.Current);
                }
                enumerator.Dispose();
                return;
            }
            int count = collection2.Count;
            if (count == 0)
            {
                this._items = List<T>._emptyArray;
                return;
            }
            this._items = new T[count];
            collection2.CopyTo(this._items, 0);
            this._size = count;
        }
        [AInvariant]public void Add(T item)
        {
            if (_size == _items.Length)
                EnsureCapacity(this._size + 1);
            _items[_size] = item;
            _size++;
        }
        public void AddRange(IEnumerable<T> collection)
        {
            this.InsertRange(this._size, collection);
        }
        public void Clear()
        {
            if (this._size > 0)
            {
                Array.Clear(this._items, 0, this._size);
                this._size = 0;
            }
        }
        public bool Contains(T item)
        {
            return IndexOf(item) != -1;
        }
        private void EnsureCapacity(int size)
        {
            if (this._items.Length < size)
            {
                int num = (this._items.Length == 0) ? 4 : (this._items.Length * 2);
                if (num > 2146435071)
                {
                    num = 2146435071;
                }
                if (num < size)
                {
                    num = size;
                }
                this.Capacity = num;
            }
        }
        public int IndexOf(T item)
        {
            if (item == null)
            {
                for (int i = 0; i < this._size; i++)
                {
                    if (this._items[i] == null)
                    {
                        return i;
                    }
                }
                return -1;
            }
            EqualityComparer<T> _default = EqualityComparer<T>.Default;
            for (int j = 0; j < this._size; j++)
            {
                if (_default.Equals(this._items[j], item))
                {
                    return j;
                }
            }
            return -1;
        }
        public void Insert(int index, T item)
        {
            if (_size == _items.Length)
                EnsureCapacity(this._size + 1);
            if (index < _size)
                Array.Copy(_items, index, _items, index + 1, _size - index);
            _items[index] = item;
            _size++;
        }
        public void InsertRange(int index, IEnumerable<T> collection)
        {
            ICollection<T> collection2 = collection as ICollection<T>;
            if (collection2 != null)
            {
                int count = collection2.Count;
                if (count > 0)
                {
                    this.EnsureCapacity(this._size + count);
                    if (index < this._size)
                    {
                        Array.Copy(this._items, index, this._items, index + count, this._size - index);
                    }
                    if (this == collection2)
                    {
                        Array.Copy(this._items, 0, this._items, index, index);
                        Array.Copy(this._items, index + count, this._items, index * 2, this._size - index);
                    }
                    else
                    {
                        T[] array = new T[count];
                        collection2.CopyTo(array, 0);
                        array.CopyTo(this._items, index);
                    }
                    this._size += count;
                }
            }
            else
            {
                IEnumerator<T> enumerator = collection.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    this.Insert(index++, enumerator.Current);
                }
                enumerator.Dispose();
            }
        }
        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }
        public void RemoveAt(int index)
        {
            _size--;
            if (index < _size)
            {
                Array.Copy(_items, index + 1, _items, index, _size - index);
            }
            _items[_size] = default(T);
        }
        public void RemoveRange(int index, int count)
        {
            if (count > 0)
            {
                this._size -= count;
                if (index < this._size)
                {
                    Array.Copy(this._items, index + count, this._items, index, this._size - index);
                }
                Array.Clear(this._items, this._size, count);
            }
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(_items, 0, array, arrayIndex, _size);
        }
        public T[] ToArray()
        {
            T[] array = new T[this._size];
            Array.Copy(this._items, 0, array, 0, this._size);
            return array;
        }
        public void Sort(Comparison<T> comparison)
        {
            #if !DEBUG
            if (this._size > 0)
            {
                IComparer<T> comparer = new Array.FunctorComparer<T>(comparison);
                Array.Sort(this._items, 0, this._size, comparer);
            }
            #endif
        }
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }
    }
    public class Stack<T> : ICollection<T>
    {
        public struct Enumerator : IEnumerator<T>
        {
            private Stack<T> _stack;
            private int _index;
            private T currentElement;
            public T Current
            {
                get
                {
                    if (this._index == -2)
                    {
                        throw new Exception();
                    }
                    if (this._index == -1)
                    {
                        throw new Exception();
                    }
                    return this.currentElement;
                }
            }
            internal Enumerator(Stack<T> stack)
            {
                this._stack = stack;
                this._index = -2;
                this.currentElement = default(T);
            }
            public void Dispose()
            {
                this._index = -1;
            }
            public bool MoveNext()
            {
                if (this._index == -2)
                {
                    this._index = this._stack._size - 1;
                    bool expr_43 = this._index >= 0;
                    if (expr_43)
                    {
                        this.currentElement = this._stack._array[this._index];
                    }
                    return expr_43;
                }
                if (this._index == -1)
                {
                    return false;
                }
                int num = this._index - 1;
                this._index = num;
                bool expr_85 = num >= 0;
                if (expr_85)
                {
                    this.currentElement = this._stack._array[this._index];
                    return expr_85;
                }
                this.currentElement = default(T);
                return expr_85;
            }
            public void Reset()
            {
                this._index = -2;
                this.currentElement = default(T);
            }
        }

        private T[] _array;
        private int _size;
        static T[] _emptyArray = new T[0];
        public Stack()
        {
            _array = _emptyArray;
        }
        public Stack(int capacity)
        {
            _array = new T[capacity];
        }
        public Stack(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            ICollection<T> collection2 = collection as ICollection<T>;
            if (collection2 != null)
            {
                int count = collection2.Count;
                this._array = new T[count];
                collection2.CopyTo(this._array, 0);
                this._size = count;
                return;
            }
            this._size = 0;
            this._array = new T[4];
            IEnumerator<T> enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                this.Push(enumerator.Current);
            }
            enumerator.Dispose();
        }
        public int Count
        {
            get { return _size; }
        }
        public void Clear()
        {
            Array.Clear(_array, 0, _size);
            _size = 0;
        }
        public T Peek()
        {
            return _array[_size - 1];
        }
        public T Pop()
        {
            T item = _array[--_size];
            _array[_size] = default(T);
            return item;
        }
        public void Push(T item)
        {
            if (_size == _array.Length)
            {
                T[] newArray = new T[(_array.Length == 0) ? 4 : 2 * _array.Length];
                Array.Copy(_array, 0, newArray, 0, _size);
                _array = newArray;
            }
            _array[_size++] = item;
        }
        public T[] ToArray()
        {
            T[] array = new T[this._size];
            for (int i = 0; i < this._size; i++)
                array[i] = this._array[this._size - i - 1];
            return array;
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new Exception();
        }
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }
    }
    public class Queue<T> : ICollection<T>
    {
        public struct Enumerator : IEnumerator<T>
        {
            private Queue<T> _q;
            private int _index;
            private T _currentElement;
            public T Current
            {
                get
                {
                    if (this._index < 0)
                    {
                        throw new Exception();
                    }
                    return this._currentElement;
                }
            }
            internal Enumerator(Queue<T> q)
            {
                this._q = q;
                this._index = -1;
                this._currentElement = default(T);
            }
            public void Dispose()
            {
                this._index = -2;
                this._currentElement = default(T);
            }
            public bool MoveNext()
            {
                if (this._index == -2)
                {
                    return false;
                }
                this._index++;
                if (this._index == this._q._size)
                {
                    this._index = -2;
                    this._currentElement = default(T);
                    return false;
                }
                this._currentElement = this._q.GetElement(this._index);
                return true;
            }
            public void Reset()
            {
                this._index = -1;
                this._currentElement = default(T);
            }
        }

        private T[] _array;
        private int _head;
        private int _tail;
        private int _size;
        private static T[] _emptyArray = new T[0];
        public int Count
        {
            get
            {
                return this._size;
            }
        }
        public Queue()
        {
            this._array = Queue<T>._emptyArray;
        }
        public Queue(int capacity)
        {
            this._array = new T[capacity];
            this._head = 0;
            this._tail = 0;
            this._size = 0;
        }
        public Queue(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            this._array = new T[4];
            this._size = 0;
            IEnumerator<T> enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                this.Enqueue(enumerator.Current);
            }
            enumerator.Dispose();
        }
        public void Clear()
        {
            if (_head < _tail)
            {
                Array.Clear(_array, _head, _size);
            }
            else
            {
                Array.Clear(_array, _head, _array.Length - _head);
                Array.Clear(_array, 0, _tail);
            }
            _head = 0;
            _tail = 0;
            _size = 0;
        }
        public void Enqueue(T item)
        {
            if (_size == _array.Length)
                SetCapacity((_array.Length == 0) ? 4 : (_array.Length * 2));

            _array[_tail] = item;
            _tail = (_tail + 1) % _array.Length;
            _size++;
        }
        public T Dequeue()
        {
            T removed = _array[_head];
            _array[_head] = default(T);
            _head = (_head + 1) % _array.Length;
            _size--;
            return removed;
        }
        public T Peek()
        {
            return _array[_head];
        }
        internal T GetElement(int i)
        {
            return this._array[(this._head + i) % this._array.Length];
        }
        private void SetCapacity(int capacity)
        {
            T[] newarray = new T[capacity];
            if (_size > 0)
            {
                if (_head < _tail)
                {
                    Array.Copy(_array, _head, newarray, 0, _size);
                }
                else
                {
                    Array.Copy(_array, _head, newarray, 0, _array.Length - _head);
                    Array.Copy(_array, 0, newarray, _array.Length - _head, _tail);
                }
            }

            _array = newarray;
            _head = 0;
            _tail = (_size == capacity) ? 0 : _size;
        }
        public T[] ToArray()
        {
            T[] array = new T[this._size];
            if (this._size == 0)
            {
                return array;
            }
            if (this._head < this._tail)
            {
                Array.Copy(this._array, this._head, array, 0, this._size);
            }
            else
            {
                Array.Copy(this._array, this._head, array, 0, this._array.Length - this._head);
                Array.Copy(this._array, 0, array, this._array.Length - this._head, this._tail);
            }
            return array;
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new Exception();
        }
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }
    }

    public sealed class LinkedListNode<T>
    {
        internal LinkedList<T> list;
        internal LinkedListNode<T> next;
        internal LinkedListNode<T> prev;
        internal T item;
        public LinkedList<T> List
        {
            get
            {
                return this.list;
            }
        }
        public LinkedListNode<T> Next
        {
            get
            {
                if (this.next != null && this.next != this.list.head)
                {
                    return this.next;
                }
                return null;
            }
        }
        public LinkedListNode<T> Previous
        {
            get
            {
                if (this.prev != null && this != this.list.head)
                {
                    return this.prev;
                }
                return null;
            }
        }
        public T Value
        {
            get
            {
                return this.item;
            }
            set
            {
                this.item = value;
            }
        }
        internal LinkedListNode(LinkedList<T> list, T value)
        {
            this.list = list;
            this.item = value;
        }
        internal void Invalidate()
        {
            this.list = null;
            this.next = null;
            this.prev = null;
        }
    }
    public class LinkedList<T> : ICollection<T>
    {
        public struct Enumerator : IEnumerator<T>
        {
            private LinkedList<T> list;
            private LinkedListNode<T> node;
            private T current;
            public T Current
            {
                get
                {
                    return this.current;
                }
            }
            internal Enumerator(LinkedList<T> list)
            {
                this.list = list;
                this.node = list.head;
                this.current = default(T);
            }
            public bool MoveNext()
            {
                if (this.node == null)
                {
                    return false;
                }
                this.current = this.node.item;
                this.node = this.node.next;
                if (this.node == this.list.head)
                {
                    this.node = null;
                }
                return true;
            }
            public void Reset()
            {
                this.current = default(T);
                this.node = this.list.head;
            }
            public void Dispose()
            {
            }
        }

        internal LinkedListNode<T> head;
        internal int count;

        public int Count
        {
            get
            {
                return this.count;
            }
        }
        public LinkedListNode<T> First
        {
            get
            {
                return this.head;
            }
        }
        public LinkedListNode<T> Last
        {
            get
            {
                if (this.head != null)
                {
                    return this.head.prev;
                }
                return null;
            }
        }

        public LinkedList()
        {
        }
        public LinkedList(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            foreach (T current in collection)
            {
                this.AddLast(current);
            }
        }
        public LinkedListNode<T> AddAfter(LinkedListNode<T> node, T value)
        {
            this.ValidateNode(node);
            LinkedListNode<T> linkedListNode = new LinkedListNode<T>(node.list, value);
            this.InternalInsertNodeBefore(node.next, linkedListNode);
            return linkedListNode;
        }
        public void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode)
        {
            this.ValidateNode(node);
            this.ValidateNode(newNode);
            this.InternalInsertNodeBefore(node.next, newNode);
            newNode.list = this;
        }
        public LinkedListNode<T> AddBefore(LinkedListNode<T> node, T value)
        {
            this.ValidateNode(node);
            LinkedListNode<T> linkedListNode = new LinkedListNode<T>(node.list, value);
            this.InternalInsertNodeBefore(node, linkedListNode);
            if (node == this.head)
            {
                this.head = linkedListNode;
            }
            return linkedListNode;
        }
        public void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode)
        {
            this.ValidateNode(node);
            this.ValidateNode(newNode);
            this.InternalInsertNodeBefore(node, newNode);
            newNode.list = this;
            if (node == this.head)
            {
                this.head = newNode;
            }
        }
        public LinkedListNode<T> AddFirst(T value)
        {
            LinkedListNode<T> linkedListNode = new LinkedListNode<T>(this, value);
            if (this.head == null)
            {
                this.InternalInsertNodeToEmptyList(linkedListNode);
            }
            else
            {
                this.InternalInsertNodeBefore(this.head, linkedListNode);
                this.head = linkedListNode;
            }
            return linkedListNode;
        }
        public void AddFirst(LinkedListNode<T> node)
        {
            this.ValidateNode(node);
            if (this.head == null)
            {
                this.InternalInsertNodeToEmptyList(node);
            }
            else
            {
                this.InternalInsertNodeBefore(this.head, node);
                this.head = node;
            }
            node.list = this;
        }
        public LinkedListNode<T> AddLast(T value)
        {
            LinkedListNode<T> linkedListNode = new LinkedListNode<T>(this, value);
            if (this.head == null)
                this.InternalInsertNodeToEmptyList(linkedListNode);
            else
                this.InternalInsertNodeBefore(this.head, linkedListNode);
            return linkedListNode;
        }
        public void AddLast(LinkedListNode<T> node)
        {
            this.ValidateNode(node);
            if (this.head == null)
                this.InternalInsertNodeToEmptyList(node);
            else
                this.InternalInsertNodeBefore(this.head, node);
            node.list = this;
        }
        public void Clear()
        {
            LinkedListNode<T> next = this.head;
            while (next != null)
            {
                LinkedListNode<T> linkedListNode = next;
                next = next.Next;
                linkedListNode.Invalidate();
            }
            this.head = null;
            this.count = 0;
        }
        public bool Contains(T value)
        {
            return this.Find(value) != null;
        }
        public void CopyTo(T[] array, int index)
        {
            LinkedListNode<T> next = this.head;
            if (next != null)
            {
                do
                {
                    array[index++] = next.item;
                    next = next.next;
                }
                while (next != this.head);
            }
        }
        public LinkedListNode<T> Find(T value)
        {
            LinkedListNode<T> next = this.head;
            if (next != null)
            {
                if (value != null)
                {
                    while (!Equals(next.item, value))
                    {
                        next = next.next;
                        if (next == this.head)
                        {
                            return null;
                        }
                    }
                    return next;
                }
                while (next.item != null)
                {
                    next = next.next;
                    if (next == this.head)
                    {
                        return null;
                    }
                }
                return next;
            }
            return null;
        }
        public bool Remove(T value)
        {
            LinkedListNode<T> linkedListNode = this.Find(value);
            if (linkedListNode != null)
            {
                this.InternalRemoveNode(linkedListNode);
                return true;
            }
            return false;
        }
        public void Remove(LinkedListNode<T> node)
        {
            this.ValidateNode(node);
            this.InternalRemoveNode(node);
        }
        public void RemoveFirst()
        {
            this.InternalRemoveNode(this.head);
        }
        public void RemoveLast()
        {
            this.InternalRemoveNode(this.head.prev);
        }
        private void InternalInsertNodeToEmptyList(LinkedListNode<T> newNode)
        {
            newNode.next = newNode;
            newNode.prev = newNode;
            this.head = newNode;
            this.count++;
        }
        private void InternalInsertNodeBefore(LinkedListNode<T> node, LinkedListNode<T> newNode)
        {
            newNode.next = node;
            newNode.prev = node.prev;
            node.prev.next = newNode;
            node.prev = newNode;
            this.count++;
        }
        internal void InternalRemoveNode(LinkedListNode<T> node)
        {
            if (node.next == node)
            {
                this.head = null;
            }
            else
            {
                node.next.prev = node.prev;
                node.prev.next = node.next;
                if (this.head == node)
                {
                    this.head = node.next;
                }
            }
            node.Invalidate();
            this.count--;
        }
        internal void ValidateNode(LinkedListNode<T> node)
        {
            if (node == null)
            {
                throw new Exception("node");
            }
            if (node.list != null)
            {
                throw new Exception();
            }
        }
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }
    }

    internal static class HashHelpers
    {
        public const int HashCollisionThreshold = 100;
        public static readonly int[] primes = new int[]
		{
			3,
			7,
			11,
			17,
			23,
			29,
			37,
			47,
			59,
			71,
			89,
			107,
			131,
			163,
			197,
			239,
			293,
			353,
			431,
			521,
			631,
			761,
			919,
			1103,
			1327,
			1597,
			1931,
			2333,
			2801,
			3371,
			4049,
			4861,
			5839,
			7013,
			8419,
			10103,
			12143,
			14591,
			17519,
			21023,
			25229,
			30293,
			36353,
			43627,
			52361,
			62851,
			75431,
			90523,
			108631,
			130363,
			156437,
			187751,
			225307,
			270371,
			324449,
			389357,
			467237,
			560689,
			672827,
			807403,
			968897,
			1162687,
			1395263,
			1674319,
			2009191,
			2411033,
			2893249,
			3471899,
			4166287,
			4999559,
			5999471,
			7199369
		};
        public static bool IsPrime(int candidate)
        {
            if ((candidate & 1) != 0)
            {
                int num = (int)Math.Sqrt((double)candidate);
                for (int i = 3; i <= num; i += 2)
                {
                    if (candidate % i == 0)
                    {
                        return false;
                    }
                }
                return true;
            }
            return candidate == 2;
        }
        public static int GetPrime(int min)
        {
            for (int i = 0; i < HashHelpers.primes.Length; i++)
            {
                int num = HashHelpers.primes[i];
                if (num >= min)
                {
                    return num;
                }
            }
            for (int j = min | 1; j < 2147483647; j += 2)
            {
                if (HashHelpers.IsPrime(j) && (j - 1) % 101 != 0)
                {
                    return j;
                }
            }
            return min;
        }
        public static int GetMinPrime()
        {
            return HashHelpers.primes[0];
        }
        public static int ExpandPrime(int oldSize)
        {
            int num = 2 * oldSize;
            if (num > 2146435069 && 2146435069 > oldSize)
            {
                return 2146435069;
            }
            return HashHelpers.GetPrime(num);
        }
        public static bool IsWellKnownEqualityComparer(object comparer)
        {
            return false;
            //return comparer == null || comparer == EqualityComparer<string>.Default || comparer is IWellKnownStringEqualityComparer;
        }
        public static IEqualityComparer<T> GetRandomizedEqualityComparer<T>(object comparer)
        {
            //if (comparer == EqualityComparer<string>.Default)
            //{
            //    return new RandomizedStringEqualityComparer();
            //}
            //IWellKnownStringEqualityComparer wellKnownStringEqualityComparer = comparer as IWellKnownStringEqualityComparer;
            //if (wellKnownStringEqualityComparer != null)
            //{
            //    return wellKnownStringEqualityComparer.GetRandomizedEqualityComparer();
            //}
            return null;
        }
        public static object GetEqualityComparerForSerialization(object comparer)
        {
            //if (comparer == null)
            //{
            //    return null;
            //}
            //IWellKnownStringEqualityComparer wellKnownStringEqualityComparer = comparer as IWellKnownStringEqualityComparer;
            //if (wellKnownStringEqualityComparer != null)
            //{
            //    return wellKnownStringEqualityComparer.GetEqualityComparerForSerialization();
            //}
            return comparer;
        }
    }
    public class Dictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private struct Entry
        {
            public int hashCode;
            public int next;
            public TKey key;
            public TValue value;
        }
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private Dictionary<TKey, TValue> dictionary;
            private int index;
            private KeyValuePair<TKey, TValue> current;

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    return this.current;
                }
            }
            internal Enumerator(Dictionary<TKey, TValue> dictionary)
            {
                this.dictionary = dictionary;
                this.index = 0;
                this.current = default(KeyValuePair<TKey, TValue>);
            }

            public bool MoveNext()
            {
                while (this.index < this.dictionary.count)
                {
                    if (this.dictionary.entries[this.index].hashCode >= 0)
                    {
                        this.current = new KeyValuePair<TKey, TValue>(this.dictionary.entries[this.index].key, this.dictionary.entries[this.index].value);
                        this.index++;
                        return true;
                    }
                    this.index++;
                }
                this.index = this.dictionary.count + 1;
                this.current = default(KeyValuePair<TKey, TValue>);
                return false;
            }
            public void Dispose()
            {
            }
            public void Reset()
            {
                this.index = 0;
                this.current = default(KeyValuePair<TKey, TValue>);
            }
        }
        public sealed class KeyCollection : ICollection<TKey>
        {
            public struct Enumerator : IEnumerator<TKey>
            {
                private Dictionary<TKey, TValue> dictionary;
                private int index;
                private TKey currentKey;

                public TKey Current
                {
                    get
                    {
                        return this.currentKey;
                    }
                }
                internal Enumerator(Dictionary<TKey, TValue> dictionary)
                {
                    this.dictionary = dictionary;
                    this.index = 0;
                    this.currentKey = default(TKey);
                }
                public void Dispose()
                {
                }
                public bool MoveNext()
                {
                    while (this.index < this.dictionary.count)
                    {
                        if (this.dictionary.entries[this.index].hashCode >= 0)
                        {
                            this.currentKey = this.dictionary.entries[this.index].key;
                            this.index++;
                            return true;
                        }
                        this.index++;
                    }
                    this.index = this.dictionary.count + 1;
                    this.currentKey = default(TKey);
                    return false;
                }
                public void Reset()
                {
                    this.index = 0;
                    this.currentKey = default(TKey);
                }
            }
            private Dictionary<TKey, TValue> dictionary;
            public int Count
            {

                get
                {
                    return this.dictionary.Count;
                }
            }
            public KeyCollection(Dictionary<TKey, TValue> dictionary)
            {
                this.dictionary = dictionary;
            }
            public void CopyTo(TKey[] array, int index)
            {
                int count = this.dictionary.count;
                Dictionary<TKey, TValue>.Entry[] entries = this.dictionary.entries;
                for (int i = 0; i < count; i++)
                {
                    if (entries[i].hashCode >= 0)
                    {
                        array[index++] = entries[i].key;
                    }
                }
            }
            public IEnumerator<TKey> GetEnumerator()
            {
                return new Dictionary<TKey, TValue>.KeyCollection.Enumerator(this.dictionary);
            }
        }
        public sealed class ValueCollection : ICollection<TValue>, IEnumerable<TValue>
        {
            public struct Enumerator : IEnumerator<TValue>
            {
                private Dictionary<TKey, TValue> dictionary;
                private int index;
                private TValue currentValue;

                public TValue Current
                {
                    get
                    {
                        return this.currentValue;
                    }
                }
                internal Enumerator(Dictionary<TKey, TValue> dictionary)
                {
                    this.dictionary = dictionary;
                    this.index = 0;
                    this.currentValue = default(TValue);
                }
                public void Dispose()
                {
                }
                public bool MoveNext()
                {
                    while (this.index < this.dictionary.count)
                    {
                        if (this.dictionary.entries[this.index].hashCode >= 0)
                        {
                            this.currentValue = this.dictionary.entries[this.index].value;
                            this.index++;
                            return true;
                        }
                        this.index++;
                    }
                    this.index = this.dictionary.count + 1;
                    this.currentValue = default(TValue);
                    return false;
                }
                public void Reset()
                {
                    this.index = 0;
                    this.currentValue = default(TValue);
                }
            }
            private Dictionary<TKey, TValue> dictionary;
            public int Count
            {
                get
                {
                    return this.dictionary.Count;
                }
            }
            public ValueCollection(Dictionary<TKey, TValue> dictionary)
            {
                this.dictionary = dictionary;
            }
            public void CopyTo(TValue[] array, int index)
            {
                int count = this.dictionary.count;
                Dictionary<TKey, TValue>.Entry[] entries = this.dictionary.entries;
                for (int i = 0; i < count; i++)
                {
                    if (entries[i].hashCode >= 0)
                    {
                        array[index++] = entries[i].value;
                    }
                }
            }
            public IEnumerator<TValue> GetEnumerator()
            {
                return new Dictionary<TKey, TValue>.ValueCollection.Enumerator(this.dictionary);
            }
        }
        private int[] buckets;
        private Dictionary<TKey, TValue>.Entry[] entries;
        private int count;
        private int freeList;
        private int freeCount;
        private IEqualityComparer<TKey> comparer;
        private Dictionary<TKey, TValue>.KeyCollection keys;
        private Dictionary<TKey, TValue>.ValueCollection values;
        public IEqualityComparer<TKey> Comparer
        {
            get
            {
                return this.comparer;
            }
        }
        public int Count
        {
            get
            {
                return this.count - this.freeCount;
            }
        }
        public Dictionary<TKey, TValue>.KeyCollection Keys
        {
            get
            {
                if (this.keys == null)
                {
                    this.keys = new Dictionary<TKey, TValue>.KeyCollection(this);
                }
                return this.keys;
            }
        }
        public Dictionary<TKey, TValue>.ValueCollection Values
        {
            get
            {
                if (this.values == null)
                {
                    this.values = new Dictionary<TKey, TValue>.ValueCollection(this);
                }
                return this.values;
            }
        }
        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get
            {
                if (this.keys == null)
                {
                    this.keys = new Dictionary<TKey, TValue>.KeyCollection(this);
                }
                return this.keys;
            }
        }
        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get
            {
                if (this.values == null)
                {
                    this.values = new Dictionary<TKey, TValue>.ValueCollection(this);
                }
                return this.values;
            }
        }
        public TValue this[TKey key]
        {
            get
            {
                int num = this.FindEntry(key);
                if (num >= 0)
                {
                    return this.entries[num].value;
                }
                return default(TValue);
            }
            set
            {
                this.Insert(key, value, false);
            }
        }
        public Dictionary()
            : this(0, null)
        {
        }
        public Dictionary(int capacity)
            : this(capacity, null)
        {
        }
        public Dictionary(IEqualityComparer<TKey> comparer)
            : this(0, comparer)
        {
        }
        public Dictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            if (capacity > 0)
            {
                this.Initialize(capacity);
            }
            this.comparer = (comparer == null ? EqualityComparer<TKey>.Default : comparer);
        }
        public Dictionary(Dictionary<TKey, TValue> dictionary)
            : this(dictionary, null)
        {
        }
        public Dictionary(Dictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : this((dictionary != null) ? dictionary.Count : 0, comparer)
        {
            var __enumerator = dictionary.GetEnumerator();
            while (__enumerator.MoveNext())
            {
                var current = __enumerator.Current;
                this.Add(current.Key, current.Value);
            }
        }
        public void Add(TKey key, TValue value)
        {
            this.Insert(key, value, true);
        }
        //void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
        //{
        //    this.Add(keyValuePair.Key, keyValuePair.Value);
        //}
        //bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
        //{
        //    int num = this.FindEntry(keyValuePair.Key);
        //    return num >= 0 && EqualityComparer<TValue>.Default.Equals(this.entries[num].value, keyValuePair.Value);
        //}
        //bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
        //{
        //    int num = this.FindEntry(keyValuePair.Key);
        //    if (num >= 0 && EqualityComparer<TValue>.Default.Equals(this.entries[num].value, keyValuePair.Value))
        //    {
        //        this.Remove(keyValuePair.Key);
        //        return true;
        //    }
        //    return false;
        //}
        public void Clear()
        {
            if (this.count > 0)
            {
                for (int i = 0; i < this.buckets.Length; i++)
                {
                    this.buckets[i] = -1;
                }
                Array.Clear(this.entries, 0, this.count);
                this.freeList = -1;
                this.count = 0;
                this.freeCount = 0;
            }
        }
        public bool ContainsKey(TKey key)
        {
            return this.FindEntry(key) >= 0;
        }
        public bool ContainsValue(TValue value)
        {
            if (value == null)
            {
                for (int i = 0; i < this.count; i++)
                {
                    if (this.entries[i].hashCode >= 0 && this.entries[i].value == null)
                    {
                        return true;
                    }
                }
            }
            else
            {
                EqualityComparer<TValue> _default = EqualityComparer<TValue>.Default;
                for (int j = 0; j < this.count; j++)
                {
                    if (this.entries[j].hashCode >= 0 && _default.Equals(this.entries[j].value, value))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new Dictionary<TKey, TValue>.Enumerator(this);
        }
        private int FindEntry(TKey key)
        {
            if (this.buckets != null)
            {
                int num = this.comparer.GetHashCode(key) & 2147483647;
                for (int i = this.buckets[num % this.buckets.Length]; i >= 0; i = this.entries[i].next)
                {
                    if (this.entries[i].hashCode == num && this.comparer.Equals(this.entries[i].key, key))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
        private void Initialize(int capacity)
        {
            int prime = HashHelpers.GetPrime(capacity);
            this.buckets = new int[prime];
            for (int i = 0; i < prime; i++)
            {
                this.buckets[i] = -1;
            }
            this.entries = new Dictionary<TKey, TValue>.Entry[prime];
            this.freeList = -1;
        }
        private void Insert(TKey key, TValue value, bool add)
        {
            if (this.buckets == null)
            {
                this.Initialize(0);
            }
            int num = this.comparer.GetHashCode(key) & 2147483647;
            int num2 = num % this.buckets.Length;
            int num3 = 0;
            for (int i = this.buckets[num2]; i >= 0; i = this.entries[i].next)
            {
                if (this.entries[i].hashCode == num && this.comparer.Equals(this.entries[i].key, key))
                {
                    if (add)
                    {
                        // 键重复
                        throw new Exception();
                    }
                    this.entries[i].value = value;
                    return;
                }
                num3++;
            }
            int num4;
            if (this.freeCount > 0)
            {
                num4 = this.freeList;
                this.freeList = this.entries[num4].next;
                this.freeCount--;
            }
            else
            {
                if (this.count == this.entries.Length)
                {
                    this.Resize();
                    num2 = num % this.buckets.Length;
                }
                num4 = this.count;
                this.count++;
            }
            this.entries[num4].hashCode = num;
            this.entries[num4].next = this.buckets[num2];
            this.entries[num4].key = key;
            this.entries[num4].value = value;
            this.buckets[num2] = num4;
            if (num3 > 100 && HashHelpers.IsWellKnownEqualityComparer(this.comparer))
            {
                this.comparer = HashHelpers.GetRandomizedEqualityComparer<TKey>(this.comparer);
                this.Resize(this.entries.Length, true);
            }
        }
        private void Resize()
        {
            this.Resize(HashHelpers.ExpandPrime(this.count), false);
        }
        private void Resize(int newSize, bool forceNewHashCodes)
        {
            int[] array = new int[newSize];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = -1;
            }
            Dictionary<TKey, TValue>.Entry[] array2 = new Dictionary<TKey, TValue>.Entry[newSize];
            Array.Copy(this.entries, 0, array2, 0, this.count);
            if (forceNewHashCodes)
            {
                for (int j = 0; j < this.count; j++)
                {
                    if (array2[j].hashCode != -1)
                    {
                        array2[j].hashCode = (this.comparer.GetHashCode(array2[j].key) & 2147483647);
                    }
                }
            }
            for (int k = 0; k < this.count; k++)
            {
                if (array2[k].hashCode >= 0)
                {
                    int num = array2[k].hashCode % newSize;
                    array2[k].next = array[num];
                    array[num] = k;
                }
            }
            this.buckets = array;
            this.entries = array2;
        }
        public bool Remove(TKey key)
        {
            if (this.buckets != null)
            {
                int num = this.comparer.GetHashCode(key) & 2147483647;
                int num2 = num % this.buckets.Length;
                int num3 = -1;
                for (int i = this.buckets[num2]; i >= 0; i = this.entries[i].next)
                {
                    if (this.entries[i].hashCode == num && this.comparer.Equals(this.entries[i].key, key))
                    {
                        if (num3 < 0)
                        {
                            this.buckets[num2] = this.entries[i].next;
                        }
                        else
                        {
                            this.entries[num3].next = this.entries[i].next;
                        }
                        this.entries[i].hashCode = -1;
                        this.entries[i].next = this.freeList;
                        this.entries[i].key = default(TKey);
                        this.entries[i].value = default(TValue);
                        this.freeList = i;
                        this.freeCount++;
                        return true;
                    }
                    num3 = i;
                }
            }
            return false;
        }
        public bool TryGetValue(TKey key, out TValue value)
        {
            int num = this.FindEntry(key);
            if (num >= 0)
            {
                value = this.entries[num].value;
                return true;
            }
            value = default(TValue);
            return false;
        }
        internal TValue GetValueOrDefault(TKey key)
        {
            int num = this.FindEntry(key);
            if (num >= 0)
            {
                return this.entries[num].value;
            }
            return default(TValue);
        }
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            int num = this.count;
            Dictionary<TKey, TValue>.Entry[] array2 = this.entries;
            for (int i = 0; i < num; i++)
            {
                if (array2[i].hashCode >= 0)
                {
                    array[index++] = new KeyValuePair<TKey, TValue>(array2[i].key, array2[i].value);
                }
            }
        }
    }
    public struct KeyValuePair<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;
        public KeyValuePair(TKey key, TValue value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
    public class HashSet<T> : ICollection<T>, IEnumerable<T>
    {
        internal struct Slot
        {
            internal int hashCode;
            internal T value;
            internal int next;
        }
        public struct Enumerator : IEnumerator<T>, IDisposable
        {
            private HashSet<T> set;
            private int index;
            private T current;
            public T Current
            {
                get
                {
                    return this.current;
                }
            }
            internal Enumerator(HashSet<T> set)
            {
                this.set = set;
                this.index = 0;
                this.current = default(T);
            }
            public void Dispose()
            {
            }
            public bool MoveNext()
            {
                while (this.index < this.set.m_lastIndex)
                {
                    if (this.set.m_slots[this.index].hashCode >= 0)
                    {
                        this.current = this.set.m_slots[this.index].value;
                        this.index++;
                        return true;
                    }
                    this.index++;
                }
                this.index = this.set.m_lastIndex + 1;
                this.current = default(T);
                return false;
            }
            public void Reset()
            {
                this.index = 0;
                this.current = default(T);
            }
        }
        private int[] m_buckets;
        private HashSet<T>.Slot[] m_slots;
        private int m_count;
        private int m_lastIndex;
        private int m_freeList;
        private IEqualityComparer<T> m_comparer;
        public int Count
        {
            get
            {
                return this.m_count;
            }
        }
        public IEqualityComparer<T> Comparer
        {
            get
            {
                return this.m_comparer;
            }
        }
        public HashSet()
            : this(EqualityComparer<T>.Default)
        {
        }
        public HashSet(IEqualityComparer<T> comparer)
        {
            if (comparer == null)
            {
                comparer = EqualityComparer<T>.Default;
            }
            this.m_comparer = comparer;
            this.m_lastIndex = 0;
            this.m_count = 0;
            this.m_freeList = -1;
        }
        public HashSet(IEnumerable<T> collection)
            : this(collection, EqualityComparer<T>.Default)
        {
        }
        public HashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
            : this(comparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            int capacity = 0;
            ICollection<T> collection2 = collection as ICollection<T>;
            if (collection2 != null)
            {
                capacity = collection2.Count;
            }
            this.Initialize(capacity);
            this.UnionWith(collection);
            if ((this.m_count == 0 && this.m_slots.Length > HashHelpers.GetMinPrime()) || (this.m_count > 0 && this.m_slots.Length / this.m_count > 3))
            {
                this.TrimExcess();
            }
        }
        public void Clear()
        {
            if (this.m_lastIndex > 0)
            {
                Array.Clear(this.m_slots, 0, this.m_lastIndex);
                Array.Clear(this.m_buckets, 0, this.m_buckets.Length);
                this.m_lastIndex = 0;
                this.m_count = 0;
                this.m_freeList = -1;
            }
        }
        public bool Contains(T item)
        {
            if (this.m_buckets != null)
            {
                int num = this.InternalGetHashCode(item);
                for (int i = this.m_buckets[num % this.m_buckets.Length] - 1; i >= 0; i = this.m_slots[i].next)
                {
                    if (this.m_slots[i].hashCode == num && this.m_comparer.Equals(this.m_slots[i].value, item))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            this.CopyTo(array, arrayIndex, this.m_count);
        }
        public bool Remove(T item)
        {
            if (this.m_buckets != null)
            {
                int num = this.InternalGetHashCode(item);
                int num2 = num % this.m_buckets.Length;
                int num3 = -1;
                for (int i = this.m_buckets[num2] - 1; i >= 0; i = this.m_slots[i].next)
                {
                    if (this.m_slots[i].hashCode == num && this.m_comparer.Equals(this.m_slots[i].value, item))
                    {
                        if (num3 < 0)
                        {
                            this.m_buckets[num2] = this.m_slots[i].next + 1;
                        }
                        else
                        {
                            this.m_slots[num3].next = this.m_slots[i].next;
                        }
                        this.m_slots[i].hashCode = -1;
                        this.m_slots[i].value = default(T);
                        this.m_slots[i].next = this.m_freeList;
                        this.m_count--;
                        if (this.m_count == 0)
                        {
                            this.m_lastIndex = 0;
                            this.m_freeList = -1;
                        }
                        else
                        {
                            this.m_freeList = i;
                        }
                        return true;
                    }
                    num3 = i;
                }
            }
            return false;
        }
        public IEnumerator<T> GetEnumerator()
        {
            return new HashSet<T>.Enumerator(this);
        }
        public bool Add(T item)
        {
            return this.AddIfNotPresent(item);
        }
        public void UnionWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            foreach (T current in other)
            {
                this.AddIfNotPresent(current);
            }
        }
        public void ExceptWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            if (this.m_count == 0)
            {
                return;
            }
            if (other == this)
            {
                this.Clear();
                return;
            }
            foreach (T current in other)
            {
                this.Remove(current);
            }
        }
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            ICollection<T> collection = other as ICollection<T>;
            if (collection != null)
            {
                if (collection.Count == 0)
                {
                    return true;
                }
                HashSet<T> hashSet = other as HashSet<T>;
                if (hashSet != null && HashSet<T>.AreEqualityComparersEqual(this, hashSet) && hashSet.Count > this.m_count)
                {
                    return false;
                }
            }
            return this.ContainsAllElements(other);
        }
        public bool Overlaps(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            if (this.m_count == 0)
            {
                return false;
            }
            foreach (T current in other)
            {
                if (this.Contains(current))
                {
                    return true;
                }
            }
            return false;
        }
        public void CopyTo(T[] array)
        {
            this.CopyTo(array, 0, this.m_count);
        }
        public void CopyTo(T[] array, int arrayIndex, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException("ArgumentOutOfRange_NeedNonNegNum");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("ArgumentOutOfRange_NeedNonNegNum");
            }
            if (arrayIndex > array.Length || count > array.Length - arrayIndex)
            {
                throw new ArgumentException("Arg_ArrayPlusOffTooSmall");
            }
            int num = 0;
            int num2 = 0;
            while (num2 < this.m_lastIndex && num < count)
            {
                if (this.m_slots[num2].hashCode >= 0)
                {
                    array[arrayIndex + num] = this.m_slots[num2].value;
                    num++;
                }
                num2++;
            }
        }
        public int RemoveWhere(Predicate<T> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            int num = 0;
            for (int i = 0; i < this.m_lastIndex; i++)
            {
                if (this.m_slots[i].hashCode >= 0)
                {
                    T value = this.m_slots[i].value;
                    if (match(value) && this.Remove(value))
                    {
                        num++;
                    }
                }
            }
            return num;
        }
        public void TrimExcess()
        {
            if (this.m_count == 0)
            {
                this.m_buckets = null;
                this.m_slots = null;
                return;
            }
            int prime = HashHelpers.GetPrime(this.m_count);
            HashSet<T>.Slot[] array = new HashSet<T>.Slot[prime];
            int[] array2 = new int[prime];
            int num = 0;
            for (int i = 0; i < this.m_lastIndex; i++)
            {
                if (this.m_slots[i].hashCode >= 0)
                {
                    array[num] = this.m_slots[i];
                    int num2 = array[num].hashCode % prime;
                    array[num].next = array2[num2] - 1;
                    array2[num2] = num + 1;
                    num++;
                }
            }
            this.m_lastIndex = num;
            this.m_slots = array;
            this.m_buckets = array2;
            this.m_freeList = -1;
        }
        private void Initialize(int capacity)
        {
            int prime = HashHelpers.GetPrime(capacity);
            this.m_buckets = new int[prime];
            this.m_slots = new HashSet<T>.Slot[prime];
        }
        private void IncreaseCapacity()
        {
            int num = HashHelpers.ExpandPrime(this.m_count);
            if (num <= this.m_count)
            {
                throw new ArgumentException("Arg_HSCapacityOverflow");
            }
            this.SetCapacity(num, false);
        }
        private void SetCapacity(int newSize, bool forceNewHashCodes)
        {
            HashSet<T>.Slot[] array = new HashSet<T>.Slot[newSize];
            if (this.m_slots != null)
            {
                Array.Copy(this.m_slots, 0, array, 0, this.m_lastIndex);
            }
            if (forceNewHashCodes)
            {
                for (int i = 0; i < this.m_lastIndex; i++)
                {
                    if (array[i].hashCode != -1)
                    {
                        array[i].hashCode = this.InternalGetHashCode(array[i].value);
                    }
                }
            }
            int[] array2 = new int[newSize];
            for (int j = 0; j < this.m_lastIndex; j++)
            {
                int num = array[j].hashCode % newSize;
                array[j].next = array2[num] - 1;
                array2[num] = j + 1;
            }
            this.m_slots = array;
            this.m_buckets = array2;
        }
        private bool AddIfNotPresent(T value)
        {
            if (this.m_buckets == null)
            {
                this.Initialize(0);
            }
            int num = this.InternalGetHashCode(value);
            int num2 = num % this.m_buckets.Length;
            int num3 = 0;
            for (int i = this.m_buckets[num % this.m_buckets.Length] - 1; i >= 0; i = this.m_slots[i].next)
            {
                if (this.m_slots[i].hashCode == num && this.m_comparer.Equals(this.m_slots[i].value, value))
                {
                    return false;
                }
                num3++;
            }
            int num4;
            if (this.m_freeList >= 0)
            {
                num4 = this.m_freeList;
                this.m_freeList = this.m_slots[num4].next;
            }
            else
            {
                if (this.m_lastIndex == this.m_slots.Length)
                {
                    this.IncreaseCapacity();
                    num2 = num % this.m_buckets.Length;
                }
                num4 = this.m_lastIndex;
                this.m_lastIndex++;
            }
            this.m_slots[num4].hashCode = num;
            this.m_slots[num4].value = value;
            this.m_slots[num4].next = this.m_buckets[num2] - 1;
            this.m_buckets[num2] = num4 + 1;
            this.m_count++;
            if (num3 > 100 && HashHelpers.IsWellKnownEqualityComparer(this.m_comparer))
            {
                this.m_comparer = (IEqualityComparer<T>)HashHelpers.GetRandomizedEqualityComparer<T>(this.m_comparer);
                this.SetCapacity(this.m_buckets.Length, true);
            }
            return true;
        }
        private bool ContainsAllElements(IEnumerable<T> other)
        {
            foreach (T current in other)
            {
                if (!this.Contains(current))
                {
                    return false;
                }
            }
            return true;
        }
        //internal T[] ToArray()
        //{
        //    T[] array = new T[this.Count];
        //    this.CopyTo(array);
        //    return array;
        //}
        private static bool AreEqualityComparersEqual(HashSet<T> set1, HashSet<T> set2)
        {
            return set1.Comparer == set2.Comparer;
        }
        private int InternalGetHashCode(T item)
        {
            if (item == null)
            {
                return 0;
            }
            return this.m_comparer.GetHashCode(item) & 2147483647;
        }
    }
    public class KeyNotFoundException : Exception
    {
        public KeyNotFoundException() { }
        public KeyNotFoundException(string msg) : base(msg) { }
        public KeyNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}
