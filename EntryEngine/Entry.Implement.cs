#if CLIENT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntryEngine
{
    partial class Entry
    {
        public static EntryEngine.AUDIO _AUDIO
        {
            get;
            set;
        }
        public EntryEngine.AUDIO AUDIO { get { return _AUDIO; } }
        public static EntryEngine.ContentManager _ContentManager
        {
            get;
            set;
        }
        public EntryEngine.ContentManager ContentManager { get { return _ContentManager; } }
        public event Action<EntryEngine.ContentManager> OnNewContentManager;
        public EntryEngine.ContentManager NewContentManager()
        {
            var __device = InternalNewContentManager();
            if (OnNewContentManager != null) OnNewContentManager(__device);
            return __device;
        }
        protected virtual EntryEngine.ContentManager InternalNewContentManager()
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            throw new System.NotImplementedException();
            #endif
        }
        public static EntryEngine.FONT _FONT
        {
            get { return FONT.Default; }
            set { FONT.Default = value; }
        }
        public EntryEngine.FONT FONT { get { return _FONT; } }
        public event Action<EntryEngine.FONT> OnNewFONT;
        public EntryEngine.FONT NewFONT(string name, float fontSize)
        {
            var __device = InternalNewFONT(name, fontSize);
            if (OnNewFONT != null) OnNewFONT(__device);
            return __device;
        }
        protected virtual EntryEngine.FONT InternalNewFONT(string name, float fontSize)
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            throw new System.NotImplementedException();
            #endif
        }
        public static EntryEngine.GRAPHICS _GRAPHICS
        {
            get;
            set;
        }
        public EntryEngine.GRAPHICS GRAPHICS { get { return _GRAPHICS; } }
        public static EntryEngine.INPUT _INPUT
        {
            get;
            set;
        }
        public EntryEngine.INPUT INPUT { get { return _INPUT; } }
        public static EntryEngine._IO.iO _iO
        {
            get { return _IO._iO; }
            set { _IO._iO = value; }
        }
        public EntryEngine._IO.iO iO { get { return _iO; } }
        public event Action<EntryEngine._IO.iO> OnNewiO;
        public EntryEngine._IO.iO NewiO(string root)
        {
            var __device = InternalNewiO(root);
            if (OnNewiO != null) OnNewiO(__device);
            return __device;
        }
        protected virtual EntryEngine._IO.iO InternalNewiO(string root)
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            return new EntryEngine._IO.iO(root);
            #endif
        }
        public static EntryEngine.IPlatform _IPlatform
        {
            get;
            set;
        }
        public EntryEngine.IPlatform IPlatform { get { return _IPlatform; } }
        public static EntryEngine.TEXTURE _TEXTURE
        {
            get;
            set;
        }
        public EntryEngine.TEXTURE TEXTURE { get { return _TEXTURE; } }
        public event Action<EntryEngine.TEXTURE> OnNewTEXTURE;
        public EntryEngine.TEXTURE NewTEXTURE(int width, int height)
        {
            var __device = InternalNewTEXTURE(width, height);
            if (OnNewTEXTURE != null) OnNewTEXTURE(__device);
            return __device;
        }
        protected virtual EntryEngine.TEXTURE InternalNewTEXTURE(int width, int height)
        {
            #if EntryBuilder
            throw new System.NotImplementedException();
            #else
            throw new System.NotImplementedException();
            #endif
        }
        
        public void Initialize()
        {
            EntryEngine.AUDIO AUDIO;
            EntryEngine.ContentManager ContentManager;
            EntryEngine.FONT FONT;
            EntryEngine.GRAPHICS GRAPHICS;
            EntryEngine.INPUT INPUT;
            EntryEngine._IO.iO iO;
            EntryEngine.IPlatform IPlatform;
            EntryEngine.TEXTURE TEXTURE;
            Initialize(out AUDIO, out ContentManager, out FONT, out GRAPHICS, out INPUT, out iO, out IPlatform, out TEXTURE);
            if (AUDIO != null) _AUDIO = AUDIO;
            if (ContentManager != null) _ContentManager = ContentManager;
            if (FONT != null) _FONT = FONT;
            if (GRAPHICS != null) _GRAPHICS = GRAPHICS;
            if (INPUT != null) _INPUT = INPUT;
            if (iO != null) _iO = iO;
            if (IPlatform != null) _IPlatform = IPlatform;
            if (TEXTURE != null) _TEXTURE = TEXTURE;
            OnInitialized();
        }
        protected abstract void Initialize(
        out EntryEngine.AUDIO AUDIO,
        out EntryEngine.ContentManager ContentManager,
        out EntryEngine.FONT FONT,
        out EntryEngine.GRAPHICS GRAPHICS,
        out EntryEngine.INPUT INPUT,
        out EntryEngine._IO.iO iO,
        out EntryEngine.IPlatform IPlatform,
        out EntryEngine.TEXTURE TEXTURE);
    }
}

#endif
