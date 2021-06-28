using System.Collections.Generic;
using System;

namespace DragonBones
{
    /// <summary>
    /// - The BaseObject is the base class for all objects in the DragonBones framework.
    /// All BaseObject instances are cached to the object pool to reduce the performance consumption of frequent requests for memory or memory recovery.
    /// </summary>
    /// <version>DragonBones 4.5</version>
    /// <language>en_US</language>

    /// <summary>
    /// - 基础对象，通常 DragonBones 的对象都继承自该类。
    /// 所有基础对象的实例都会缓存到对象池，以减少频繁申请内存或内存回收的性能消耗。
    /// </summary>
    /// <version>DragonBones 4.5</version>
    /// <language>zh_CN</language>
    public abstract class BaseObject
    {
        private static uint _hashCode = 0;
        private static uint _defaultMaxCount = 3000;
        private static readonly Dictionary<System.Type, uint> _maxCountMap = new Dictionary<System.Type, uint>();
        private static readonly Dictionary<System.Type, List<BaseObject>> _poolsMap = new Dictionary<System.Type, List<BaseObject>>();

        private static void _ReturnObject(BaseObject obj)
        {
            var classType = obj.GetType();
            var maxCount = _maxCountMap.ContainsKey(classType) ? _maxCountMap[classType] : _defaultMaxCount;
            var pool = _poolsMap.ContainsKey(classType) ? _poolsMap[classType] : _poolsMap[classType] = new List<BaseObject>();

            if (pool.Count < maxCount)
            {
                if (!pool.Contains(obj))
                {
                    pool.Add(obj);
                }
                else
                {
                    throw new InvalidOperationException("The object is already in the pool.");
                }
            }
            else
            {

            }
        }

        /// <summary>
        /// - Set the maximum cache count of the specify object pool.
        /// </summary>
        /// <param name="objectConstructor">- The specify class. (Set all object pools max cache count if not set)</param>
        /// <param name="maxCount">- Max count.</param>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        /// <summary>
        /// - 设置特定对象池的最大缓存数量。
        /// </summary>
        /// <param name="objectConstructor">- 特定的类。 (不设置则设置所有对象池的最大缓存数量)</param>
        /// <param name="maxCount">- 最大缓存数量。</param>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public static void SetMaxCount(System.Type classType, uint maxCount)
        {
            if (classType != null)
            {
                if (_poolsMap.ContainsKey(classType))
                {
                    var pool = _poolsMap[classType];
                    if (pool.Count > maxCount)
                    {
                        pool.ResizeList((int)maxCount, null);
                    }
                }

                _maxCountMap[classType] = maxCount;
            }
            else
            {
                _defaultMaxCount = maxCount;

                foreach (var key in _poolsMap.Keys)
                {
                    var pool = _poolsMap[key];
                    if (pool.Count > maxCount)
                    {
                        pool.ResizeList((int)maxCount, null);
                    }

                    if (_maxCountMap.ContainsKey(key))
                    {
                        _maxCountMap[key] = maxCount;
                    }
                }
            }
        }

        /// <summary>
        /// - Clear the cached instances of a specify object pool.
        /// </summary>
        /// <param name="objectConstructor">- Specify class. (Clear all cached instances if not set)</param>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        /// <summary>
        /// - 清除特定对象池的缓存实例。
        /// </summary>
        /// <param name="objectConstructor">- 特定的类。 (不设置则清除所有缓存的实例)</param>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public static void ClearPool(System.Type classType)
        {
            if (classType != null)
            {
                if (_poolsMap.ContainsKey(classType))
                {
                    var pool = _poolsMap[classType];
                    if (pool != null)
                    {
                        pool.Clear();
                    }
                }
            }
            else
            {
                foreach (var pair in _poolsMap)
                {
                    var pool = _poolsMap[pair.Key];
                    if (pool != null)
                    {
                        pool.Clear();
                    }
                }
            }
        }
        /// <summary>
        /// - Get an instance of the specify class from object pool.
        /// </summary>
        /// <param name="objectConstructor">- The specify class.</param>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        /// <summary>
        /// - 从对象池中获取特定类的实例。
        /// </summary>
        /// <param name="objectConstructor">- 特定的类。</param>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public static T BorrowObject<T>() where T : BaseObject, new()
        {
            //var type = typeof(T);
            //List<BaseObject> pool;
            //_poolsMap.TryGetValue(type, out pool);
            //if (pool != null && pool.Count > 0)
            //{
            //    var index = pool.Count - 1;
            //    var obj = pool[index];
            //    pool.RemoveAt(index);
            //    return (T)obj;
            //}
            //else
            //{
            //    var obj = new T();
            //    obj._OnClear();
            //    return obj;
            //}
            var obj = new T();
            obj._OnClear();
            return obj;
        }
        /// <summary>
        /// - A unique identification number assigned to the object.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        /// <summary>
        /// - 分配给此实例的唯一标识号。
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public readonly uint hashCode = _hashCode++;

        protected BaseObject()
        {
        }

        /// <private/>
        protected abstract void _OnClear();
        /// <summary>
        /// - Clear the object and return it back to object pool。
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        /// <summary>
        /// - 清除该实例的所有数据并将其返还对象池。
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public void ReturnToPool()
        {
            _OnClear();
            //_ReturnObject(this);
        }

        // public static implicit operator bool(BaseObject exists)
        // {
        //     return exists != null;
        // }
    }
}