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
            if (Event.current.type == EventType.repaint)
                Entry.Draw();
        }
    }
}
