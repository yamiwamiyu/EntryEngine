namespace EntryEngine.DragonBone.DBCore
{
    /// <summary>
    /// - Play animation interface. (Both Armature and Wordclock implement the interface)
    /// Any instance that implements the interface can be added to the Worldclock instance and advance time by Worldclock instance uniformly.
    /// </summary>
    /// <see cref="DragonBones.WorldClock"/>
    /// <see cref="DragonBones.Armature"/>
    /// <version>DragonBones 3.0</version>
    /// <language>en_US</language>

    /// <summary>
    /// - 播放动画接口。 (Armature 和 WordClock 都实现了该接口)
    /// 任何实现了此接口的实例都可以添加到 WorldClock 实例中，由 WorldClock 实例统一更新时间。
    /// </summary>
    /// <see cref="DragonBones.WorldClock"/>
    /// <see cref="DragonBones.Armature"/>
    /// <version>DragonBones 3.0</version>
    /// <language>zh_CN</language>
    public interface IAnimatable
    {
        /// <summary>
        /// - Advance time.
        /// </summary>
        /// <param name="passedTime">- Passed time. (In seconds)</param>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>

        /// <summary>
        /// - 更新时间。
        /// </summary>
        /// <param name="passedTime">- 前进的时间。 （以秒为单位）</param>
        /// <version>DragonBones 3.0</version>
        /// <language>zh_CN</language>
        void AdvanceTime(float passedTime);
        WorldClock clock
        {
            get;
            set;
        }
    }
}