using System;
using UnityEngine;
using EntryEngine;
using EntryEngine.Serialize;

namespace EntryEngine.Unity
{
    public class UnityGate : MonoBehaviour
    {
        public Material GLMaterial;

        public static UnityGate Gate
        {
            get;
            private set;
        }

        public event Func<Entry> OnCreateEntry;
        public event Func<Entry, VECTOR2> OnInitialize;
        public event Action<Entry> OnInitialized;

        public Entry Entry
        {
            get;
            private set;
        }

        protected virtual void Start()
        {
            Debug.Log("初始化Unity入口");
            Gate = this;

            if (OnCreateEntry != null)
                Entry = OnCreateEntry();
            else
                Entry = new EntryUnity();
            if (OnInitialize != null)
                OnInitialize(Entry);
            Entry.Initialize();
            if (OnInitialized != null)
                OnInitialized(Entry);
        }
        protected virtual void Update()
        {
            Entry.Update();
        }
        //protected virtual void OnPostRender()
        protected virtual void OnGUI()
        {
            if (Event.current.type == EventType.Repaint)
                Entry.Draw();
        }
        private void OnApplicationPause(bool pause)
        {
            // 进程缩小后可能不能后台运行，下次切回进程时会一次更新较长时间，应该让时间静止在那一帧的时间
            if (!pause)
            {
                Entry.GameTime.Elapse();
                Entry.GameTime.Still();
            }
        }
    }
}
