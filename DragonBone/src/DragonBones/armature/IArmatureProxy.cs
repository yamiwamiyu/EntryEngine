namespace DragonBones
{
    public enum EEventType
    {
        /// <summary>动画开始播放</summary>
        START,
        /// <summary>动画循环播放完成一次</summary>
        LOOP_COMPLETE,
        /// <summary>动画播放完成</summary>
        COMPLETE,
        /// <summary>动画淡入开始</summary>
        FADE_IN,
        /// <summary>动画淡入完成</summary>
        FADE_IN_COMPLETE,
        /// <summary>动画淡出开始</summary>
        FADE_OUT,
        /// <summary>动画淡出完成</summary>
        FADE_OUT_COMPLETE,
        /// <summary>动画帧事件</summary>
        FRAME_EVENT,
        /// <summary>动画帧声音事件</summary>
        SOUND_EVENT,
    }
    /// <summary>
    /// - The armature proxy interface, the docking engine needs to implement it concretely.
    /// </summary>
    /// <see cref="DragonBones.Armature"/>
    /// <version>DragonBones 5.0</version>
    /// <language>en_US</language>

    /// <summary>
    /// - 骨架代理接口，对接的引擎需要对其进行具体实现。
    /// </summary>
    /// <see cref="DragonBones.Armature"/>
    /// <version>DragonBones 5.0</version>
    /// <language>zh_CN</language>
    public class IArmatureProxy : IEventDispatcher<EventObject>
    {
        /// <summary>骨架信息</summary>
        public virtual Armature Armature { get; protected set; }
        /// <summary>动作动画</summary>
        public virtual Animation Animation { get { return Armature.animation; } }

        protected internal virtual void DBInit(Armature armature) { this.Armature = armature; }
        protected internal virtual void DBClear() { }
        protected internal virtual void DBUpdate() { }
        protected internal virtual void Dispose(bool disposeProxy) { }
    }
}
