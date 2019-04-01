using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace __System.Collections
{
    public interface IEnumerable
    {
        [AInvariant]IEnumerator GetEnumerator();
    }
    public interface IEnumerator
    {
        [AInvariant]object Current { get; }
        [AInvariant]bool MoveNext();
        void Reset();
    }
    public interface ICollection : IEnumerable
    {
        [AInvariant]int Count { get; }
        void CopyTo(Array array, int arrayIndex);
    }
    public interface IDictionary : ICollection, IEnumerable
    {
        object this[object key] { get; set; }
        ICollection Keys { get; }
        ICollection Values { get; }
        bool Contains(object key);
        void Add(object key, object value);
        void Clear();
        void Remove(object key);
    }
    public interface IList : ICollection, IEnumerable
    {
        object this[int index] { get; set; }
        int Add(object value);
        bool Contains(object value);
        void Clear();
        int IndexOf(object value);
        void Insert(int index, object value);
        void Remove(object value);
        void RemoveAt(int index);
    }

    //public interface IEnumerable : IEnumerable<object> { }
    //public interface IEnumerator : IEnumerator<object> { }
    //public interface ICollection : ICollection<object> { }
    //public interface IDictionary : IDictionary<object, object> { }
    //public interface IList : IList<object> { }
}
