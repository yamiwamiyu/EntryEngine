using System.Collections.Generic;
using System;
namespace DragonBones
{
    public delegate void ListenerDelegate<T>(string type, T eventObject);
    /// <summary>
    /// - The event dispatcher interface.
    /// Dragonbones event dispatch usually relies on docking engine to implement, which defines the event method to be implemented when docking the engine.
    /// </summary>
    /// <version>DragonBones 4.5</version>
    /// <language>en_US</language>

    /// <summary>
    /// - 事件派发接口。
    /// DragonBones 的事件派发通常依赖于对接的引擎来实现，该接口定义了对接引擎时需要实现的事件方法。
    /// </summary>
    /// <version>DragonBones 4.5</version>
    /// <language>zh_CN</language>
    public abstract class IEventDispatcher<T>
    {
        protected internal Dictionary<EEventType, List<Action<T>>> events = new Dictionary<EEventType, List<Action<T>>>();

        protected internal virtual bool HasDBEventListener(EEventType type)
        {
            return events.ContainsKey(type);
        }
        protected internal virtual void DispatchDBEvent(EEventType type, T eventObject)
        {
            List<Action<T>> list;
            if (!events.TryGetValue(type, out list))
                return;
            for (int i = 0; i < list.Count; i++)
                list[i](eventObject);
        }
        /// <summary>添加事件</summary>
        public void AddDBEventListener(EEventType type, Action<T> listener)
        {
            List<Action<T>> list;
            if (!events.TryGetValue(type, out list))
            {
                list = new List<Action<T>>(2);
                events.Add(type, list);
            }
            list.Add(listener);
        }
        public void RemoveDBEventListener(EEventType type, Action<T> listener)
        {
            List<Action<T>> list;
            if (events.TryGetValue(type, out list))
            {
                list.Remove(listener);
            }
        }
    }
}
