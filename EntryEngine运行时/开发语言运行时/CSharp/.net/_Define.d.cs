using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[AInvariant]public struct @void { }

namespace __System
{
    public abstract class ValueType { }
}

#if !DEBUG
[ANonOptimize][AInvariant]abstract class ___Array<T> : Array, IList<T>, IEnumerable<T>
{
    struct Enumerator : IEnumerator<T>
    {
        ___Array<T> array;
        int index;
        public Enumerator(___Array<T> array)
        {
            this.array = array;
            this.index = -1;
        }
        public T Current
        {
            get { return array[index]; }
        }
        public bool MoveNext()
        {
            index++;
            return index >= array.Length - 1;
        }
        public void Reset()
        {
            index = -1;
        }
        public void Dispose()
        {
            array = null;
        }
    }
    public new IEnumerator<T> GetEnumerator()
    {
        return new Enumerator(this);
    }
    //public new T this[int index]
    //{
    //    get { return this[index]; }
    //    set { this[index] = value; }
    //}
    public extern T this[int index] { get; set; }
    public new void RemoveAt(int index)
    {
        throw new NotImplementedException();
    }
    public new void CopyTo(T[] array, int arrayIndex)
    {
        Array.Copy(this, arrayIndex, array, 0, this.Length);
    }
}
#endif

/// <summary>标识此特性的类型或程序集在重构代码或转换语言代码时类型及其成员不进行重命名，[不进行优化无引用成员（暂未实现，优化掉也貌似可以）]</summary>
internal class AInvariant : Attribute { public static readonly string Name = typeof(AInvariant).Name; }
/// <summary>标识此特性则不会被优化掉</summary>
internal class ANonOptimize : Attribute { public static readonly string Name = typeof(ANonOptimize).Name; }
/// <summary>标识此特性的类型在重构代码时生成程序集信息</summary>
internal class AReflexible : Attribute { public static readonly string Name = typeof(AReflexible).Name; }
/// <summary>标识此特性的类型或成员需要在目标语言上实现重写覆盖API</summary>
internal class ASystemAPI : Attribute { public static readonly string Name = typeof(ASystemAPI).Name; }