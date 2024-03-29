namespace EntryEngine.DragonBone.DBCore
{
    /// <summary>
    /// - The properties of the object carry basic information about an event,
    /// which are passed as parameter or parameter's parameter to event listeners when an event occurs.
    /// </summary>
    /// <version>DragonBones 4.5</version>
    /// <language>en_US</language>

    /// <summary>
    /// - 事件对象，包含有关事件的基本信息，当发生事件时，该实例将作为参数或参数的参数传递给事件侦听器。
    /// </summary>
    /// <version>DragonBones 4.5</version>
    /// <language>zh_CN</language>
    public class EventObject : BaseObject
    {
        /// <summary>
        /// - Animation start play.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        /// <summary>
        /// - 动画开始播放。
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public const string START = "start";
        /// <summary>
        /// - Animation loop play complete once.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        /// <summary>
        /// - 动画循环播放完成一次。
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public const string LOOP_COMPLETE = "loopComplete";
        /// <summary>
        /// - Animation play complete.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        /// <summary>
        /// - 动画播放完成。
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public const string COMPLETE = "complete";
        /// <summary>
        /// - Animation fade in start.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        /// <summary>
        /// - 动画淡入开始。
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public const string FADE_IN = "fadeIn";
        /// <summary>
        /// - Animation fade in complete.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        /// <summary>
        /// - 动画淡入完成。
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public const string FADE_IN_COMPLETE = "fadeInComplete";
        /// <summary>
        /// - Animation fade out start.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        /// <summary>
        /// - 动画淡出开始。
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public const string FADE_OUT = "fadeOut";
        /// <summary>
        /// - Animation fade out complete.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        /// <summary>
        /// - 动画淡出完成。
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public const string FADE_OUT_COMPLETE = "fadeOutComplete";
        /// <summary>
        /// - Animation frame event.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        /// <summary>
        /// - 动画帧事件。
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public const string FRAME_EVENT = "frameEvent";
        /// <summary>
        /// - Animation frame sound event.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        /// <summary>
        /// - 动画帧声音事件。
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public const string SOUND_EVENT = "soundEvent";

        /// <internal/>
        /// <private/>
        /// <summary>
        /// - The armature that dispatch the event.
        /// </summary>
        /// <see cref="DragonBones.Armature"/>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        /// <summary>
        /// - 发出该事件的骨架。
        /// </summary>
        /// <see cref="DragonBones.Armature"/>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        /// <summary>
        /// - The custom data.
        /// </summary>
        /// <see cref="DragonBones.CustomData"/>
        /// <private/>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>

        /// <summary>
        /// - 自定义数据。
        /// </summary>
        /// <see cref="DragonBones.CustomData"/>
        /// <private/>
        /// <version>DragonBones 5.0</version>
        /// <language>zh_CN</language>
        public static void ActionDataToInstance(ActionData data, EventObject instance, Armature armature)
        {
            if (data.type == ActionType.Play)
            {
                instance.type = EEventType.FRAME_EVENT;
            }
            else
            {
                instance.type = data.type == ActionType.Frame ? EEventType.FRAME_EVENT : EEventType.SOUND_EVENT;
            }

            instance.name = data.name;
            instance.armature = armature;
            instance.actionData = data;
            instance.data = data.data;

            if (data.bone != null)
            {
                instance.bone = armature.GetBone(data.bone.name);
            }

            if (data.slot != null)
            {
                instance.slot = armature.GetSlot(data.slot.name);
            }
        }

        /// <summary>
        /// - If is a frame event, the value is used to describe the time that the event was in the animation timeline. (In seconds)
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        /// <summary>
        /// - 如果是帧事件，此值用来描述该事件在动画时间轴中所处的时间。（以秒为单位）
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public float time;
        /// <summary>
        /// - The event type。
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        /// <summary>
        /// - 事件类型。
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public EEventType type;
        /// <summary>
        /// - The event name. (The frame event name or the frame sound name)
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        /// <summary>
        /// - 事件名称。 (帧事件的名称或帧声音的名称)
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public string name;
        public Armature armature;
        /// <summary>
        /// - The bone that dispatch the event.
        /// </summary>
        /// <see cref="DragonBones.Bone"/>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        /// <summary>
        /// - 发出该事件的骨骼。
        /// </summary>
        /// <see cref="DragonBones.Bone"/>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public Bone bone;
        /// <summary>
        /// - The slot that dispatch the event.
        /// </summary>
        /// <see cref="DragonBones.Slot"/>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        /// <summary>
        /// - 发出该事件的插槽。
        /// </summary>
        /// <see cref="DragonBones.Slot"/>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public Slot slot;
        /// <summary>
        /// - The animation state that dispatch the event.
        /// </summary>
        /// <see cref="DragonBones.AnimationState"/>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        /// <summary>
        /// - 发出该事件的动画状态。
        /// </summary>
        /// <see cref="DragonBones.AnimationState"/>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public AnimationState animationState;
        /// <private/>
        public ActionData actionData;
        public UserData data;

        /// <private/>
        protected override void _OnClear()
        {
            this.time = 0.0f;
            this.type = EEventType.FRAME_EVENT;
            this.name = string.Empty;
            this.armature = null;
            this.bone = null;
            this.slot = null;
            this.animationState = null;
            this.actionData = null;
            this.data = null;
        }
    }
}
