#if CLIENT

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EntryEngine.Serialize;
using EntryEngine.UI;

namespace EntryEngine
{
    public abstract partial class Entry : EntryService
    {
        public new static Entry Instance
        {
            get;
            private set;
        }

        private Dictionary<Type, UIScene> cachedScenes = new Dictionary<Type, UIScene>();
        /*
         * 1. Entry更新流程
         * EntryService协程 -> PhaseUpdate -> SceneUpdate -> Draw
         * 2. 场景显示一般被调用在以上任一步骤中
         * 3. PhaseUpdate开始时表示完整的一帧，此时新场景Event,Update,Draw的完整一帧将开始
         * 4. PhaseUpdate开始之后显示新场景，Loading的场景不执行Update和Draw，Showing的场景将执行Update不执行Draw(UIScene.IsDrawable来跳过Draw)
         * 4. PhaseUpdate开始之前显示的场景，将显示完整的一帧
         */
        private EPhase phase = EPhase.Running;
        private LinkedList<UIScene> scenes = new LinkedList<UIScene>();
        public event Action<GRAPHICS, INPUT> OnDrawMouse;
        public event Action<UIScene, EState, bool> OnShowScene;
        /// <summary>每帧经过的时间：true:Platform.FrameRate/false:实际经过时间</summary>
        public bool IsFixedTimeStep;

        public IEnumerable<UIScene> Dialogs
        {
            get
            {
                var current = scenes.Last;
                while (current != null && current != scenes.First)
                {
                    var dialog = current.Value;
                    if (dialog.Parent == null)
                        yield return dialog;
                    current = current.Previous;
                }
            }
        }
        public UIScene Scene
        {
            get
            {
                if (scenes.Count == 0)
                    return null;
                else
                    return scenes.First.Value;
            }
        }
        public UIScene PrevMainScene { get; private set; }

        public Entry()
        {
            if (Entry.Instance == null)
                Entry.Instance = this;
        }

        private void OnInitialized()
        {
            GRAPHICS.GraphicsAdaptScreen();
            INPUT.Update(this);
            INPUT.Update(this);
        }
        protected void SetPIXEL(TEXTURE texture)
        {
            if (TEXTURE._pixel != null)
                throw new InvalidOperationException();
            if (texture != null)
                TEXTURE._pixel = new TEXTURE_SYSTEM(texture);
        }
        protected void SetPATCH(TEXTURE texture)
        {
            if (PATCH._patch != null)
                throw new InvalidOperationException();
            if (texture != null)
                PATCH._patch = new TEXTURE_SYSTEM(texture);
        }
        public void ReleaseUnusableScene()
        {
            PrevMainScene = null;
            Dictionary<Type, UIScene> cache = new Dictionary<Type, UIScene>();
            foreach (var item in scenes)
            {
                Type type = item.GetType();
                cache.Add(type, item);
                cachedScenes.Remove(type);
            }
            foreach (var item in cachedScenes.Values)
                item.Dispose();
            cachedScenes.Clear();
            cachedScenes = cache;
        }
        public IEnumerable<UIScene> GetChildScene(UIScene parent)
        {
            var current = scenes.Last;
            if (current != null && current != scenes.First)
            {
                var child = current.Value;
                if (child.Parent == parent)
                    yield return child;
                current = current.Previous;
            }
        }
        public T GetScene<T>() where T : UIScene
        {
            UIScene scene;
            cachedScenes.TryGetValue(typeof(T), out scene);
            return (T)scene;
        }
        public T GetSceneOrCreate<T>() where T : UIScene, new()
        {
            Type type = typeof(T);
            UIScene scene;
            if (!cachedScenes.TryGetValue(type, out scene))
            {
                scene = new T();
                cachedScenes.Add(type, scene);
            }
            return (T)scene;
        }
        private T CacheScene<T>() where T : UIScene, new()
        {
            Type type = typeof(T);
            UIScene scene;
            if (!cachedScenes.TryGetValue(type, out scene))
            {
                //scene = new T();
                // JS暂不支持上面的写法
                scene = Activator.CreateInstance<T>();
                cachedScenes.Add(type, scene);
            }
            return (T)scene;
        }
        public T ShowMainScene<T>() where T : UIScene, new()
        {
            T scene = CacheScene<T>();
            ShowMainScene(scene);
            return scene;
        }
        public void ShowMainScene(UIScene scene)
        {
            if (scenes.Count > 0)
                scenes.ForFirstToLast((item) => item.OnPhaseEnding());
            InternalShowScene(scene, EState.None, true);
            //phase = EPhase.Ending;
        }
        /// <summary>
        /// 使用过场场景切换场景
        /// </summary>
        /// <typeparam name="T">要切换的主场景类型</typeparam>
        /// <typeparam name="U">过场场景类型</typeparam>
        /// <returns>主场景</returns>
        public T SwitchMainScene<T, U>()
            where T : UIScene, new()
            where U : UIScene, new()
        {
            T main = ShowMainScene<T>();
            ShowDialogScene<U>();
            return main;
        }
        public T ShowDialogScene<T>() where T : UIScene, new()
        {
            return ShowDialogScene<T>(EState.Dialog);
        }
        public T ShowDialogScene<T>(EState dialogState) where T : UIScene, new()
        {
            T scene = CacheScene<T>();
            // new scene loading
            ShowDialogScene(scene, dialogState);
            return (T)scene;
        }
        public void ShowDialogScene(UIScene scene)
        {
            InternalShowScene(scene, EState.None, false);
        }
        public void ShowDialogScene(UIScene scene, EState state)
        {
            InternalShowScene(scene, state, false);
        }
        private void InternalShowScene(UIScene scene, EState dialogState, bool isMain)
        {
            if (scene == null)
                throw new ArgumentNullException("scene");

            //scene.Entry = this;

            if (OnShowScene != null)
                OnShowScene(scene, dialogState, isMain);

            // 已显示的场景重新进入Showing阶段
            if (scenes.Contains(scene))
            {
                scene.State = dialogState;
                scene.Show(this);
                scene.OnPhaseShowing();
                if (phase == EPhase.Running)
                    phase = EPhase.Showing;
                ToFront(scene);
                return;
            }

            // new scene loading
            if (isMain)
            {
                // 缓存前一个主场景
                PrevMainScene = this.Scene;
                scenes.AddFirst(scene);
                phase = EPhase.Ending;
            }
            else
            {
                scenes.AddLast(scene);
                if (phase > EPhase.Loading)
                    phase = EPhase.Loading;
            }
            scene.Show(this);
            scene.OnPhaseLoading();
            scene.State = dialogState;
            UIElement.__HandledElement = scene;
        }
        public void ShowDesktop(EState state)
        {
            foreach (var scene in Dialogs)
                Close(scene, state);
        }
        public void ShowDesktopImmediately(EState state)
        {
            foreach (var scene in Dialogs)
                CloseImmediately(scene, state);
        }
        internal void Close(UIScene scene, EState state)
        {
            scene.State = state;
            scene.OnPhaseEnding();
            // 关闭子场景
            foreach (var dialog in GetChildScene(scene))
            {
                dialog.State = state;
                dialog.OnPhaseEnding();
            }
            phase = EPhase.Ending;
        }
        internal void CloseImmediately(UIScene scene, EState state)
        {
            if (scenes.Remove(scene))
            {
                // 关闭子场景
                foreach (var dialog in GetChildScene(scene))
                    CloseImmediately(dialog, state);
                scene.OnPhaseEnding();
                scene.OnPhaseEnded();
                InternalCloseScene(scene, ref state);
            }
        }
        internal void ToFront(UIScene scene)
        {
            if (Scene == scene)
                return;

            if (scenes.Remove(scene))
                scenes.AddLast(scene);
        }
        internal void ToBack(UIScene scene)
        {
            if (Scene == scene)
                return;

            if (scenes.Remove(scene))
                scenes.AddAfter(scenes.First, scene);
        }
        private void InternalCloseScene(UIScene scene, ref EState state)
        {
            scene.SetPhase(null);
            if (state == EState.Dispose || state == EState.Release)
            {
                scene.Dispose();
                if (state == EState.Release)
                    cachedScenes.Remove(scene.GetType());
            }
            scene.State = state;
        }
        protected override void InternalUpdate()
        {
            UIElement.__PrevHandledElement = UIElement.__HandledElement;
            UIElement.__HandledElement = null;
            if (IPlatform.IsActive)
                InputUpdate();
            if (AUDIO != null)
                AUDIO.Update(GameTime);
            PhaseUpdate();
            SceneUpdate();
        }
        private bool NeedUpdateScene(UIScene scene)
        {
            if (!scene.IsEnable)
                return false;

            if (scene.Phase != EPhase.Loading
                && scene.Phase != EPhase.Preparing
                && scene.Phase != EPhase.Prepared)
            {
                if (scene.Parent == null)
                {
                    return true;
                }
                else
                {
                    var parent = scene.Scene;
                    while (true)
                    {
                        if (parent != null)
                        {
                            if (parent.Entry == null)
                            {
                                if (parent.Parent != null)
                                {
                                    parent = parent.Scene;
                                    continue;
                                }
                            }
                            else
                            {
                                // 场景是其它场景中的部件时需要等待父场景准备完成
                                return NeedUpdateScene(parent);
                            }
                        }
                        scene.Close(true);
                        return false;
                    }
                }
            }
            return false;
        }
        private bool NeedEventScene(UIScene scene)
        {
            if (scene.Phase != EPhase.Running)
            {
                //_LOG.Debug("Entry.Phase is Running, UIScene.Phase is not Running.");
                return false;
            }

            if (!scene.IsEventable)
                return false;

            if (scene.Parent == null)
            {
                return true;
            }
            else
            {
                var parent = scene.Scene;
                //if (parent == null)
                //{
                //    // null -> UIElement -> scene的情况，即场景的父对象并没有存在于场景内，此时属于幽灵场景
                //    throw new InvalidCastException("ghost scene");
                //}
                //if (parent == null || (parent.Parent == null && parent.Entry == null))
                //{
                //    scene.Close(true);
                //}
                //else
                //{
                //    // 场景是其它场景中的部件时需要等待父场景准备完成
                //    // 目前主要测试ghost scene的情况，如果无此情况，则只需要测试phase == Running
                //    return NeedEventScene(parent);
                //}
                while (true)
                {
                    if (parent != null)
                    {
                        if (parent.Entry == null)
                        {
                            if (parent.Parent != null)
                            {
                                parent = parent.Scene;
                                continue;
                            }
                        }
                        else
                        {
                            // 场景是其它场景中的部件时需要等待父场景准备完成
                            return NeedEventScene(parent);
                        }
                    }
                    scene.Close(true);
                    return false;
                }
            }
        }
        private bool NeedDrawScene(UIScene scene)
        {
            scene.IsDrawable = scene.Phase != EPhase.Loading
                // 没有准备且处在准备阶段的菜单不绘制
                && ((scene.Phase != EPhase.Preparing && scene.Phase != EPhase.Prepared) || scene.Phasing != null);

            if (scene.IsDrawable && scene.DrawState && scene.IsVisible)
            {
                if (scene.Parent == null)
                {
                    return true;
                }
                else
                {
                    var parent = scene.Scene;
                    //if (parent == null)
                    //{
                    //    // null -> UIElement -> scene的情况，即场景的父对象并没有存在于场景内，此时属于幽灵场景
                    //    throw new InvalidCastException("ghost scene");
                    //}
                    while (true)
                    {
                        if (parent != null)
                        {
                            if (parent.Entry == null)
                            {
                                if (parent.Parent != null)
                                {
                                    parent = parent.Scene;
                                    continue;
                                }
                            }
                            else
                            {
                                // 场景是其它场景中的部件时需要等待父场景准备完成
                                return NeedDrawScene(parent);
                            }
                        }
                        scene.Close(true);
                        return false;
                    }
                }
            }
            return false;
        }
        private void PhaseEnding()
        {
            bool end = true;

            scenes.ForFirstToLast(item =>
            {
                if (item.Phase == EPhase.Ending)
                {
                    if (item.Phasing == null || item.Phasing.IsEnd)
                    {
                        scenes.Remove(item);
                        item.OnPhaseEnded();
                        InternalCloseScene(item, ref item.State);
                    }
                    else
                        end = false;
                }
            });

            //foreach (var item in scenes.Where(s => s.Phase == EPhase.Ending))
            //    if (item.Phasing != null && !item.Phasing.IsEnd)
            //        end = false;

            if (end)
            {
                phase = EPhase.Loading;
                //scenes.ForFirstToLast(item =>
                //{
                //    if (item.Phase == EPhase.Ending)
                //    {
                //        scenes.Remove(item);
                //        item.OnPhaseEnded();
                //        InternalCloseScene(item, ref item.State);
                //    }
                //});
            }

            PhaseLoading();
        }
        private void PhaseLoading()
        {
            bool end = true;
            foreach (var item in scenes.Where(s => s.Phase == EPhase.Loading))
                if (item.Phasing != null && !item.Phasing.IsEnd)
                    end = false;
                else
                    item.OnPhasePreparing();

            // 等待Ending结束，等待所有Loading结束
            if (end && phase == EPhase.Loading)
            {
                phase = EPhase.Preparing;
                //scenes.ForFirstToLast(item =>
                //{
                //    if (item.Phase == EPhase.Ending)
                //    {
                //        scenes.Remove(item);
                //        item.OnPhaseEnded();
                //        InternalCloseScene(item, ref item.State);
                //    }
                //});
            }

            // 等待Ending结束
            if (phase != EPhase.Ending)
                PhasePreparing();
        }
        private void PhasePreparing()
        {
            bool end = true;
            foreach (var item in scenes.Where(s => s.Phase == EPhase.Preparing))
                if (item.Phasing != null && !item.Phasing.IsEnd)
                    end = false;
                else
                    item.OnPhasePrepared();

            // 等待Loading结束，等待所有Preparing结束，进入Showing
            if (end && phase == EPhase.Preparing)
            {
                phase = EPhase.Showing;
                scenes.ForFirstToLast(item =>
                {
                    if (item.Phase == EPhase.Prepared)
                    {
                        item.OnPhaseShowing();
                    }
                });

                PhaseShowing();
            }
        }
        private void PhaseShowing()
        {
            bool end = true;
            foreach (var item in scenes.Where(s => s.Phase == EPhase.Showing))
                if (item.Phasing != null && !item.Phasing.IsEnd)
                    end = false;
                else
                    item.OnPhaseShown();

            if (phase == EPhase.Showing && end)
                phase = EPhase.Running;
        }
        private void PhaseUpdate()
        {
            // 当ShowScene在协程上就被调用时，防止当前帧的绘制被跳过造成闪屏
            foreach (var item in scenes)
                item.IsDrawable = true;

            switch (phase)
            {
                case EPhase.Ending: PhaseEnding(); break;
                case EPhase.Loading: PhaseLoading(); break;
                case EPhase.Preparing: PhasePreparing(); break;
                case EPhase.Showing: PhaseShowing(); break;
                default: break;
            }

            LinkedListNode<UIScene> node = scenes.Last;
            while (node != null)
            {
                var item = node.Value;
                if (item.Phasing != null && !item.Phasing.IsEnd)
                {
                    try
                    {
                        item.Phasing.Update(GameTime);
                    }
                    catch (Exception ex)
                    {
                        _LOG.Error(ex, "Scene:{0}-{1} Phase:{2}", item.GetType().Name, item.Name, item.Phase.ToString());
                        item.Phasing.Dispose();
                        item.Phasing = null;
                    }
                }
                node = node.Previous;
            }
        }
        private void SceneUpdate()
        {
            bool _event = phase == EPhase.Running;
            if (!_event && Scene != null && Scene.Phase == EPhase.Running)
                _event = true;

            foreach (var item in scenes)
                // 首先更新一下Hover状态，否则子场景更新时，可能父场景没有更新导致Hover失效
                item.UpdateHoverState(INPUT.Pointer);

            var main = this.Scene;
            var node = scenes.Last;
            while (node != null &&
                // 对话框场景中切换主场景时，会导致原来的主场景移动到对话框场景，此时跳过这一帧的主场景更新
                (this.Scene == main || node.Value != main))
            {
                UIScene scene = node.Value;
                node = node.Previous;

                // 更新UI的流布局
                scene.FlowLayout();

                // 场景不能是在其它场景中的部件
                // 否则需要等待父场景加载完成
                //if (scene.Parent == null)
                {
                    if (_event && NeedEventScene(scene))
                    {
                        scene.Event(this);
                        // 幽灵场景被从舞台中移除
                        if (scene.Entry == null)
                            continue;
                    }

                    if (NeedUpdateScene(scene))
                    {
                        scene.Update(this);
                        // 幽灵场景被从舞台中移除
                        if (scene.Entry == null)
                            continue;
                    }

                    switch (scene.State)
                    {
                        case EState.Dialog:
                        case EState.Cover:
                            _event = false;
                            break;

                        case EState.CoverAll:
                        case EState.Block: return;

                        case EState.Break:
                        case EState.Dispose:
                        case EState.Release:
                            // close
                            //if (scene.Phase != EPhase.Ending)
                            if (phase == EPhase.Running && scene.Phase != EPhase.Ending)
                                Close(scene, scene.State);
                            break;
                    }
                }
            }
        }
        protected virtual void InputUpdate()
        {
            INPUT.Update(this);
        }
        protected override void Elapsed(GameTime gameTime)
        {
            if (IsFixedTimeStep)
            {
                TimeSpan time = IPlatform.FrameRate;
                gameTime.ElapsedTime = time;
                gameTime.Elapsed = (float)time.TotalMilliseconds;
                gameTime.ElapsedSecond = (float)time.TotalSeconds;
            }
        }
        public void Draw()
        {
            GRAPHICS.Clear();

            GRAPHICS.Begin(MATRIX2x3.Identity, GRAPHICS.FullGraphicsArea);

            var node = scenes.Last;
            // Cover和CoverAll将对绘制其它场景造成遮挡
            UIScene coverScene = null;
            UIScene coverAll = null;
            while (node != null)
            {
                UIScene scene = node.Value;
                if (scene.State == EState.Cover)
                {
                    coverScene = scene;
                    break;
                }
                else if (scene.State == EState.CoverAll)
                {
                    coverAll = scene;
                    break;
                }
                node = node.Previous;
            }
            if (coverAll != null)
            {
                // CoverAll绘制和Cover场景[及其后面的对话框场景]
                //if (NeedDrawScene(coverAll))
                //    coverAll.Draw(GRAPHICS, this);
                node = scenes.First;
                bool cover = false;
                // Cover绘制主场景和Cover场景[及其后面的对话框场景]
                while (node != null)
                {
                    var scene = node.Value;
                    if (scene == coverAll)
                        cover = true;
                    if (cover && NeedDrawScene(scene))
                        scene.Draw(GRAPHICS, this);
                    node = node.Next;
                }
            }
            else
            {
                node = scenes.First;
                bool cover = coverScene != null;
                // Cover绘制主场景和Cover场景[及其后面的对话框场景]
                while (node != null)
                {
                    var scene = node.Value;
                    if (coverScene != null && cover && scene == coverScene)
                        cover = false;
                    if ((!cover || node == scenes.First) && NeedDrawScene(scene))
                        scene.Draw(GRAPHICS, this);
                    node = node.Next;
                }
            }

            if (OnDrawMouse != null && IPlatform.IsMouseVisible)
                OnDrawMouse(GRAPHICS, INPUT);

            GRAPHICS.End();

            GRAPHICS.Render();
        }
        public override void Dispose()
        {
            PrevMainScene = null;
            foreach (var item in cachedScenes)
                item.Value.Dispose();
            cachedScenes.Clear();
            foreach (var item in scenes)
                item.Dispose();
            scenes.Clear();
        }
    }


    #region Device


    public enum EPlatform
    {
        /// <summary>
        /// PC
        /// </summary>
        Desktop,
        /// <summary>
        /// 移动设备
        /// </summary>
        Mobile,
        /// <summary>
        /// 主机 & 掌机
        /// </summary>
        Console,
        VR,
        Unique,
    }
    [ADevice]
    public interface IPlatform
    {
        /// <summary>平台</summary>
        EPlatform Platform { get; }
        /// <summary>一帧的时间</summary>
        TimeSpan FrameRate { get; set; }
        /// <summary>指针是否可见</summary>
        bool IsMouseVisible { get; set; }
        /// <summary>当前程序是否激活，激活将更新用户操作</summary>
        bool IsActive { get; }
    }


    #endregion


    #region Input


    /*
	 * 鼠标和触屏都应有屏幕坐标和画布坐标
	 */
    [ADevice]
    public class INPUT
    {
        public IPointer Pointer
        {
            get
            {
                if (Touch != null)
                {
                    return Touch;
                }
                else
                {
                    return Mouse;
                }
            }
        }
        public MOUSE Mouse;
        public TOUCH Touch;
        public KEYBOARD Keyboard;
        public InputText InputDevice;

        protected INPUT()
        {
        }
        public INPUT(MOUSE mouse)
        {
            if (mouse == null)
                throw new ArgumentNullException("mouse");
            this.Mouse = mouse;
        }
        public INPUT(MOUSE mouse, KEYBOARD keyboard)
            : this(mouse)
        {
            this.Keyboard = keyboard;
        }
        public INPUT(TOUCH touch)
        {
            if (touch == null)
                throw new ArgumentNullException("touch");
            this.Touch = touch;
        }
        public INPUT(TOUCH touch, KEYBOARD keyboard)
            : this(touch)
        {
            this.Keyboard = keyboard;
        }
        public INPUT(MOUSE mouse,
            TOUCH touch,
            KEYBOARD keyboard,
            InputText device)
        {
            this.Mouse = mouse;
            this.Touch = touch;
            this.Keyboard = keyboard;
            this.InputDevice = device;
        }

        public virtual void Update(Entry entry)
        {
            // 若Keyboard在InputDevice之后调用，会使InputCapture先捕获操作键
            if (Keyboard != null)
                Keyboard.Update(entry);
            if (Mouse != null)
                Mouse.Update(entry);
            if (Touch != null)
                Touch.Update(entry);

            if (InputDevice != null)
            {
                InputDevice.Update(entry);
                if (InputDevice.IsActive && Pointer != null && Pointer.IsPressed(Pointer.DefaultKey))
                {
                    if (InputDevice.Typist is UIElement)
                    {
                        UIElement.__HandledElement = (UIElement)InputDevice.Typist;
                    }
                }
            }
        }
    }
    public enum PCKeys : byte
    {
        A = 65,
        Add = 107,
        Apps = 93,
        Attn = 246,
        B = 66,
        Back = 8,
        BrowserBack = 166,
        BrowserFavorites = 171,
        BrowserForward = 167,
        BrowserHome = 172,
        BrowserRefresh = 168,
        BrowserSearch = 170,
        BrowserStop = 169,
        C = 67,
        CapsLock = 20,
        Crsel = 247,
        D = 68,
        D0 = 48,
        D1,
        D2,
        D3,
        D4,
        D5,
        D6,
        D7,
        D8,
        D9,
        Decimal = 110,
        Delete = 46,
        Divide = 111,
        Down = 40,
        E = 69,
        End = 35,
        Enter = 13,
        EraseEof = 249,
        Escape = 27,
        Execute = 43,
        Exsel = 248,
        F = 70,
        F1 = 112,
        F10 = 121,
        F11,
        F12,
        F13,
        F14,
        F15,
        F16,
        F17,
        F18,
        F19,
        F2 = 113,
        F20 = 131,
        F21,
        F22,
        F23,
        F24,
        F3 = 114,
        F4,
        F5,
        F6,
        F7,
        F8,
        F9,
        G = 71,
        H,
        Help = 47,
        Home = 36,
        I = 73,
        ImeConvert = 28,
        ImeNoConvert,
        Insert = 45,
        J = 74,
        K,
        Kana = 21,
        Kanji = 25,
        L = 76,
        LaunchApplication1 = 182,
        LaunchApplication2,
        LaunchMail = 180,
        LeftControl = 162,
        Left = 37,
        LeftAlt = 164,
        LeftShift = 160,
        LeftWindows = 91,
        M = 77,
        MediaNextTrack = 176,
        MediaPlayPause = 179,
        MediaPreviousTrack = 177,
        MediaStop,
        Multiply = 106,
        N = 78,
        None = 0,
        NumLock = 144,
        NumPad0 = 96,
        NumPad1,
        NumPad2,
        NumPad3,
        NumPad4,
        NumPad5,
        NumPad6,
        NumPad7,
        NumPad8,
        NumPad9,
        O = 79,
        OemAuto = 243,
        OemCopy = 242,
        OemEnlW = 244,
        OemSemicolon = 186,
        OemBackslash = 226,
        OemQuestion = 191,
        OemTilde,
        OemOpenBrackets = 219,
        OemPipe,
        OemCloseBrackets,
        OemQuotes,
        Oem8,
        OemClear = 254,
        OemComma = 188,
        OemMinus,
        OemPeriod,
        OemPlus = 187,
        P = 80,
        Pa1 = 253,
        PageDown = 34,
        PageUp = 33,
        Pause = 19,
        Play = 250,
        Print = 42,
        PrintScreen = 44,
        ProcessKey = 229,
        Q = 81,
        R,
        RightControl = 163,
        Right = 39,
        RightAlt = 165,
        RightShift = 161,
        RightWindows = 92,
        S = 83,
        Scroll = 145,
        Select = 41,
        SelectMedia = 181,
        Separator = 108,
        Sleep = 95,
        Space = 32,
        Subtract = 109,
        T = 84,
        Tab = 9,
        U = 85,
        Up = 38,
        V = 86,
        VolumeDown = 174,
        VolumeMute = 173,
        VolumeUp = 175,
        W = 87,
        X,
        Y,
        Z,
        Zoom = 251,
        ChatPadGreen = 202,
        ChatPadOrange
    }
    public interface IInputState
    {
        /// <summary>
        /// 是否点击
        /// </summary>
        /// <param name="key">0:左键 / 1:右键 / 2:中键 / 自定义按键</param>
        /// <returns>是否点击</returns>
        bool IsClick(int key);
    }
    //public interface IMultipleInputState : IInputState
    //{
    //    /// <summary>
    //    /// 用于按下顺序排序
    //    /// </summary>
    //    int ID { get; }
    //}
    public interface IPointerState : IInputState
    {
        /// <summary>
        /// 坐标
        /// </summary>
        VECTOR2 Position { get; set; }
    }
    public interface IMouseState : IPointerState
    {
        [Code(ECode.Value)]
        float ScrollWheelValue { get; }
    }
    public interface ITouchState : IPointerState
    {
        [Code(ECode.Value)]
        float Pressure { get; }
    }
    public interface IKeyboardState : IInputState
    {
        bool HasPressedAnyKey { get; }
        /// <summary>
        /// 按下的所有键
        /// </summary>
        int[] GetPressedKey();
    }
    public abstract class Input<T> where T : IInputState
    {
        private T current;
        private T previous;
        private Dictionary<int, ComboClick> comboClicks = new Dictionary<int, ComboClick>();

        public T Current
        {
            get { return current; }
            protected set { current = value; }
        }
        public T Previous
        {
            get { return previous; }
        }
        protected Dictionary<int, ComboClick> ComboClicks
        {
            get { return comboClicks; }
        }
        public ComboClick ComboClick
        {
            get { return GetComboClick(DefaultKey); }
        }
        /// <summary>默认键</summary>
        public virtual int DefaultKey { get { return 0; } }

        public ComboClick GetComboClick(int key)
        {
            ComboClick combo;
            comboClicks.TryGetValue(key, out combo);
            return combo;
        }
        public void AddMultipleClick(params int[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                comboClicks[keys[i]] = new ComboClick();
            }
        }
        public virtual bool IsClick(int key)
        {
            return (current != null && current.IsClick(key)) && (previous == null || !previous.IsClick(key));
            //return current.IsClick(key) && !previous.IsClick(key);
        }
        public virtual bool IsRelease(int key)
        {
            return (previous != null && previous.IsClick(key)) && (current == null || !current.IsClick(key));
            //return !current.IsClick(key) && previous.IsClick(key);
        }
        public virtual bool IsPressed(int key)
        {
            return current != null && current.IsClick(key);
            //return current.IsClick(key);
        }
        public virtual void Update(Entry entry)
        {
            float elapsed = entry.GameTime.Elapsed;
            StateUpdate();
            if (comboClicks.Count > 0)
            {
                foreach (KeyValuePair<int, ComboClick> item in comboClicks)
                {
                    item.Value.Update(IsPressed(item.Key), elapsed);
                }
            }
        }
        protected virtual void StateUpdate()
        {
            previous = current;
            current = GetState();
        }
        protected abstract T GetState();
    }
    public abstract class ActionKeyboard<T> : KEYBOARD
    {
        public Dictionary<T, int> ActionMap = new Dictionary<T, int>();

        public bool IsActionClick(T action)
        {
            return IsClick(ActionMap[action]);
        }
        public bool IsActionRelease(T action)
        {
            return IsRelease(ActionMap[action]);
        }
        public bool IsActionPressed(T action)
        {
            return IsPressed(ActionMap[action]);
        }
    }
    public abstract class KEYBOARD : Input<IKeyboardState>
    {
        /// <summary>ComboClick.IsKeyActive(ms)</summary>
        public static float KeyInputInterval = 50;

        public bool Focused
        {
            get { return Current.HasPressedAnyKey || (Previous != null && Previous.HasPressedAnyKey); }
        }
        public bool Ctrl
        {
            get
            {
                return IsPressed(PCKeys.LeftControl) || IsPressed(PCKeys.RightControl);
            }
        }
        public bool Alt
        {
            get
            {
                return IsPressed(PCKeys.LeftAlt) || IsPressed(PCKeys.RightAlt);
            }
        }
        public bool Shift
        {
            get
            {
                return IsPressed(PCKeys.LeftShift) || IsPressed(PCKeys.RightShift);
            }
        }

        public ComboClick GetComboClick(PCKeys key)
        {
            return GetComboClick((int)key);
        }
        public bool IsClick(PCKeys key)
        {
            return IsClick((int)key);
        }
        public bool IsRelease(PCKeys key)
        {
            return IsRelease((int)key);
        }
        public bool IsPressed(PCKeys key)
        {
            return IsPressed((int)key);
        }
        public bool IsInputKeyPressed(int key)
        {
            bool isClick = IsClick(key);
            ComboClick click;
            if (ComboClicks.TryGetValue(key, out click))
            {
                return click.IsKeyActive(KeyInputInterval);
            }
            else
            {
                if (isClick)
                {
                    AddMultipleClick(key);
                    ComboClicks[key].Update(true, 0);
                }
                return isClick;
            }
        }
        public bool IsInputKeyPressed(PCKeys key)
        {
            return IsInputKeyPressed((int)key);
        }

        //protected override void StateUpdate()
        //{
        //    base.StateUpdate();

        //    foreach (int key in Current.PressedKey)
        //    {
        //        if (!ComboClicks.ContainsKey(key))
        //        {
        //            AddMultipleClick(key);
        //        }
        //    }
        //}
    }
    public class ComboClick
    {
        /// <summary>
        /// 连续点击有效时间
        /// </summary>
        public static ushort ComboTime = 250;

        private float doubleClickTime = ComboTime;
        private float firstClickedTime;
        private bool isFirstClicked;
        private bool isDoubleClick;
        private int clickCount;
        private float pressedTime;
        private float _pressedTime;

        /// <summary>
        /// 连续点击次数
        /// </summary>
        public int ClickCount { get { return clickCount; } }
        public bool IsDoubleClick { get { return isDoubleClick; } }
        /// <summary>
        /// 按键按下时间（ms）
        /// </summary>
        public float PressedTime { get { return pressedTime; } }
        /// <summary>
        /// 是否处于连续点击的有效时间内
        /// </summary>
        public bool IsComboClickActive { get { return clickCount > 0 && firstClickedTime < doubleClickTime; } }

        public ComboClick() { }
        /// <summary>
        /// 多次点击
        /// </summary>
        /// <param name="doubleClickInternal">双击判定时间</param>
        public ComboClick(float doubleClickInternal)
        {
            this.doubleClickTime = doubleClickInternal;
        }

        public void Update(bool click, float time)
        {
            // mouse double click
            if (isDoubleClick)
            {
                isDoubleClick = false;
            }

            if (click)
            {
                if (!isFirstClicked)
                {
                    isFirstClicked = true;
                    firstClickedTime = 0;
                    clickCount++;
                    if (clickCount > 1)
                        isDoubleClick = true;
                }
                else
                {
                    _pressedTime = pressedTime;
                    pressedTime += time;
                }
            }
            else
            {
                if (!isFirstClicked)
                {
                    _pressedTime = 0;
                    pressedTime = 0;
                }
                isFirstClicked = false;
            }

            firstClickedTime += time;
            if (firstClickedTime > doubleClickTime)
            {
                clickCount = 0;
                isFirstClicked = false;
                firstClickedTime = 0;
            }
        }
        /// <summary>
        /// 按键是否有效，首次按下有效 / 持续按下超过双击时间后持续有效
        /// </summary>
        public bool IsKeyActive(float interval)
        {
            if (isFirstClicked && pressedTime == 0)
                return true;
            float over = pressedTime - doubleClickTime;
            if (over < 0)
                return false;
            if (interval == 0)
                return true;
            return (int)(over / interval) != (int)((_pressedTime - doubleClickTime) / interval);
        }
        public bool IsPressedTimeOver(float time)
        {
            return _pressedTime <= time && time < pressedTime;
        }
    }
    // IPointer : IInputState继承后会导致Pointer<T>继承的Input<T>的IsClick方法不能确定是Input<T>.IsClick还是IInputState.IsClick
    public interface IPointer
    {
        VECTOR2 Position { get; set; }
        VECTOR2 PositionPrevious { get; }
        VECTOR2 DeltaPosition { get; }
        VECTOR2 ClickPosition { get; }
        ComboClick ComboClick { get; }
        int DefaultKey { get; }

        void Update(Entry entry);
        ComboClick GetComboClick(int key);
        bool IsClick(int key);
        bool IsTap();
        bool IsTap(int key);
        bool IsRelease(int key);
        bool IsPressed(int key);
        bool EnterArea(RECT area);
        bool EnterArea(CIRCLE area);
        bool LeaveArea(RECT area);
        bool LeaveArea(CIRCLE area);
    }
    /// <summary>
    /// 升级到.net4.0就可以对泛型T使用out关键字，Pointer`IPointerState就可以等于MOUSE或TOUCH实例了，目前则使用IPointer接口
    /// </summary>
    public abstract class Pointer<T> : Input<T>, IPointer where T : IPointerState
    {
        /// <summary>
        /// 轻击按下最长时间(ms)
        /// </summary>
        public static byte TapTime = 250;
        /// <summary>
        /// 轻击拖动最长距离(px)
        /// </summary>
        public static byte TapDistance = 20;

        protected VECTOR2 clickPosition = VECTOR2.NaN;
        protected VECTOR2 position = VECTOR2.NaN;
        protected VECTOR2 positionPrevious = VECTOR2.NaN;

        /// <summary>Position in Graphcis</summary>
        public VECTOR2 Position
        {
            get { return position; }
            set
            {
                position = value;
                if (Current != null)
                    Current.Position = Entry._GRAPHICS.PointToScreen(value);
            }
        }
        public VECTOR2 DeltaPosition
        {
            get
            {
                if (Previous == null || Current == null)
                    return VECTOR2.Zero;
                return VECTOR2.Subtract(ref position, ref positionPrevious);
            }
        }
        public VECTOR2 PositionPrevious
        {
            get { return positionPrevious; }
        }
        public VECTOR2 ClickPosition
        {
            get { return clickPosition; }
        }
        public VECTOR2 ClickPositionRelative
        {
            get { return VECTOR2.Subtract(ref position, ref clickPosition); }
        }

        public bool IsTap()
        {
            return IsTap(DefaultKey);
        }
        public bool IsTap(int key)
        {
            var combo = GetComboClick(key);
            if (combo == null)
                return false;
            return IsRelease(key) && ClickPositionRelative.LengthSquared() < TapDistance * TapDistance && combo.PressedTime < TapTime;
        }
        protected override void StateUpdate()
        {
            base.StateUpdate();
            positionPrevious = position;
            if (Current == null)
                if (Previous != null)
                    position = Entry._GRAPHICS.PointToGraphics(Previous.Position);
                else
                    position = VECTOR2.NaN;
            else
                position = Entry._GRAPHICS.PointToGraphics(Current.Position);

            // 最后点下的按键作为点击坐标
            foreach (var item in ComboClicks)
            {
                if (IsClick(item.Key))
                {
                    clickPosition = position;
                    break;
                }
            }
        }
        public bool EnterArea(RECT area)
        {
            return !area.Contains(PositionPrevious) && area.Contains(Position);
        }
        public bool EnterArea(CIRCLE area)
        {
            return !area.Contains(PositionPrevious) && area.Contains(Position);
        }
        public bool LeaveArea(RECT area)
        {
            return !area.Contains(Position) && area.Contains(PositionPrevious);
        }
        public bool LeaveArea(CIRCLE area)
        {
            return !area.Contains(Position) && area.Contains(PositionPrevious);
        }

        // 防止Input<T>的实现与IPointer的定义不一致
        ComboClick IPointer.ComboClick
        {
            get { return this.ComboClick; }
        }
        int IPointer.DefaultKey
        {
            get { return this.DefaultKey; }
        }
        ComboClick IPointer.GetComboClick(int key)
        {
            return this.GetComboClick(key);
        }
        void IPointer.Update(Entry entry)
        {
            this.Update(entry);
        }
        bool IPointer.IsClick(int key)
        {
            return this.IsClick(key);
        }
        bool IPointer.IsRelease(int key)
        {
            return this.IsRelease(key);
        }
        bool IPointer.IsPressed(int key)
        {
            return this.IsPressed(key);
        }
    }
    /// <summary>
    /// 0: Left
    /// 1: Right
    /// 2: Middle
    /// </summary>
    public abstract class MOUSE : Pointer<IMouseState>
    {
        public float ScrollWheelValue
        {
            get { return Current.ScrollWheelValue - Previous.ScrollWheelValue; }
        }

        public MOUSE()
        {
            AddMultipleClick(0, 1, 2);
        }
    }
    /// <summary>
    /// 可以用IMouseState模拟单个Touch
    /// </summary>
    /// <typeparam name="T">IMouseState或ITouchState</typeparam>
    public class SingleTouch<T> : Pointer<T> where T : IPointerState
    {
        protected T currentState;
        public SingleTouch()
        {
            AddMultipleClick(0);
        }
        public void SetCurrent(T state)
        {
            this.currentState = state;
        }
        protected override T GetState()
        {
            T temp = currentState;
            currentState = default(T);
            return temp;
        }
    }
    public abstract class TOUCH : Pointer<ITouchState>
    {
        public static byte TOUCH_COUNT = 5;

        private SingleTouch<ITouchState>[] inputs = new SingleTouch<ITouchState>[TOUCH_COUNT];
        private int psize;
        private int size;
        private ITouchState[] states = new ITouchState[TOUCH_COUNT];

        public int Count
        {
            get { return size; }
        }
        public Pointer<ITouchState> First
        {
            get
            {
                if (size == 0)
                    return null;
                return inputs[0];
            }
        }
        public Pointer<ITouchState> Last
        {
            get
            {
                if (size == 0)
                    return null;
                return inputs[size - 1];
            }
        }
        public override int DefaultKey
        {
            get { return size == 0 ? 0 : size - 1; }
        }
        public Pointer<ITouchState> this[int index] { get { return GetTouch(index); } }

        #region 手势

        /// <summary>
        /// 扩大
        /// </summary>
        public bool TouchExpand
        {
            get { return Scale > 0; }
        }
        /// <summary>
        /// 缩小
        /// </summary>
        public bool TouchReduce
        {
            get { return Scale < 0; }
        }
        /// <summary>
        /// 缩放
        /// </summary>
        public float Scale
        {
            get
            {
                if (inputs.Length > 1)
                {
                    Pointer<ITouchState> t1 = First;
                    Pointer<ITouchState> t2 = Last;

                    if (t1.DeltaPosition.Equals(VECTOR2.Zero) && t2.DeltaPosition.Equals(VECTOR2.Zero))
                    {
                        return 0;
                    }
                    else
                    {
                        return VECTOR2.Distance(t1.Position, t2.Position) -
                               VECTOR2.Distance(VECTOR2.Subtract(t1.Position, t1.DeltaPosition), VECTOR2.Subtract(t2.Position, t2.DeltaPosition));
                    }
                }
                else
                {
                    return 0;
                }
            }
        }
        /// <summary>
        /// 旋转
        /// </summary>
        public float Rotate
        {
            get
            {
                if (inputs.Length > 1)
                {
                    Pointer<ITouchState> t1 = First;
                    Pointer<ITouchState> t2 = Last;

                    if (t1.DeltaPosition.Equals(VECTOR2.Zero) && t2.DeltaPosition.Equals(VECTOR2.Zero))
                    {
                        return 0;
                    }
                    else
                    {
                        return VECTOR2.Degree(t1.Position, t2.Position) -
                            VECTOR2.Degree(VECTOR2.Subtract(t1.Position, t1.DeltaPosition), VECTOR2.Subtract(t2.Position, t2.DeltaPosition));
                    }
                }
                else
                {
                    return 0;
                }
            }
        }

        #endregion

        public TOUCH()
        {
            AddMultipleClick(0);
            for (int i = 0; i < inputs.Length; i++)
                inputs[i] = new SingleTouch<ITouchState>();
        }

        public Pointer<ITouchState> GetTouch(int index)
        {
            if (index < 0 || index >= size)
                return null;
            return inputs[index];
        }
        public sealed override bool IsClick(int key)
        {
            return size > psize;
        }
        public sealed override bool IsPressed(int key)
        {
            return size > 0;
        }
        public sealed override bool IsRelease(int key)
        {
            return size < psize;
        }
        protected override void StateUpdate()
        {
            // refresh states
            psize = size;
            size = GetTouches(states);
            if (size > states.Length)
                throw new ArgumentOutOfRangeException("size");
            for (int i = 0; i < size; i++)
                inputs[i].SetCurrent(states[i]);
            for (int i = 0; i < inputs.Length; i++)
                inputs[i].Update(Entry.Instance);
            base.StateUpdate();
        }
        protected override ITouchState GetState()
        {
            if (size == 0)
                return null;
            else
                return inputs[size - 1].Current;
        }
        /// <summary>
        /// 刷新TouchState到缓存
        /// </summary>
        /// <param name="states">前一帧的states缓存</param>
        /// <returns>Touch的数量</returns>
        protected abstract int GetTouches(ITouchState[] states);
    }

    /// <summary>
    /// 所有要实现的文本操作
    /// </summary>
    public enum EInputText
    {
        /// <summary>
        /// 输入字符
        /// </summary>
        Char = 0,
        /// <summary>
        /// 删除字符（前）
        /// </summary>
        BackSpace = 8,
        /// <summary>
        /// Tab \t
        /// </summary>
        Tab = 9,
        /// <summary>
        /// 确认 Ctrl + Enter \n
        /// </summary>
        Enter = 10,
        /// <summary>
        /// 换行 Enter \r
        /// </summary>
        Newline = 13,
        /// <summary>
        /// 取消
        /// </summary>
        Esc = 27,
        /// <summary>
        /// 删除字符（后）
        /// </summary>
        Delete = 46,
        /// <summary>
        /// 选择 Shift L:160 R:161
        /// </summary>
        Select = 160,
        /// <summary>
        /// 同类字符跳过 Ctrl L:162 R:163
        /// </summary>
        Skip = 162,

        /// <summary>
        /// 移位到行尾
        /// </summary>
        End = 35,
        /// <summary>
        /// 移位到行首
        /// </summary>
        Home = 36,
        /// <summary>
        /// 左移位
        /// </summary>
        Left = 37,
        /// <summary>
        /// 上移位
        /// </summary>
        Up = 38,
        /// <summary>
        /// 右移位
        /// </summary>
        Right = 39,
        /// <summary>
        /// 下移位
        /// </summary>
        Down = 40,

        // Ctrl + Alphabet = 1 ~ 26

        /// <summary>
        /// 全选 Ctrl + A
        /// </summary>
        SelectAll = 1,
        /// <summary>
        /// 复制 Ctrl + C
        /// </summary>
        Copy = 3,
        /// <summary>
        /// 粘贴 Ctrl + V
        /// </summary>
        Paste = 22,
        /// <summary>
        /// 剪切 Ctrl + X
        /// </summary>
        Cut = 24,
        /// <summary>
        /// 重做 Ctrl + Y
        /// </summary>
        Redo = 25,
        /// <summary>
        /// 撤销 Ctrl + Z
        /// </summary>
        Undo = 26,
    }
    public abstract class InputText
    {
        protected enum EInput : byte
        {
            Input,
            Replace,
            Done,
            Canceled,
        }

        public static COLOR CursorColor = COLOR.Black;
        public static COLOR CursorAreaColor = new COLOR(51, 153, 255);
        public static int BlickInterval = 1000;
        protected class OTextInput : ORecord
        {
            public string Text;
            public int Index;
            public int From;
            public int To;
            private InputText device;

            public OTextInput(InputText device)
            {
                this.device = device;
                this.Text = device.text;
                this.Index = device.index;
                this.From = device.from;
                this.To = device.to;
            }

            protected override void Operate(Operation operation)
            {
                OTextInput target = operation as OTextInput;
                device.Text = target.Text;
                device.index = target.Index;
                device.from = target.From;
                device.to = target.To;
            }
        }

        private TIME blick;
        private ITypist typist;
        private bool stopping;
        private string previous;
        private string current;
        private string text;
        private string[] lines;
        private int index = -1;
        private int from = -1;
        private int to = -1;
        private string copied;
        private bool focusCtrl;
        private OperationLog operations = new OperationLog();
        private bool shift;
        private bool ctrl;
        private Queue<char> inputCache = new Queue<char>();

        public string Text
        {
            get { return text; }
            set
            {
                if (value == null)
                    value = string.Empty;
                current = value;
                if (typist.BreakLine)
                {
                    text = typist.Font.BreakLine(value, typist.TextArea.Width, out lines);
                }
                else
                {
                    text = value;
                    lines = value.Split(FONT.LINE_BREAK);
                }
                blick.Reset();

                if (index >= value.Length)
                    index = value.Length;
            }
        }
        public int Index
        {
            get { return index; }
        }
        public int LastIndex
        {
            get { return text.Length; }
        }
        public ITypist Typist
        {
            get { return typist; }
        }
        public bool IsActive
        {
            get { return typist != null; }
        }
        public bool SelectOrder
        {
            get { return from <= to; }
        }
        public bool HasSelected
        {
            get { return from != to; }
        }
        public int SelectedFrom
        {
            get { return from; }
        }
        public int SelectedTo
        {
            get { return to; }
        }
        public string SelectedText
        {
            get
            {
                if (HasSelected)
                {
                    int p1 = FONT.ChangeIndex(text, from, current);
                    int p2 = FONT.ChangeIndex(text, to, current);
                    if (p1 > p2)
                        Utility.Swap(ref p1, ref p2);
                    int len = p2 - p1;
                    return new string(current.ToCharArray(p1, len));
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        public bool CursorShow
        {
            get
            {
                blick.Interval = BlickInterval;
                return blick.Current <= (BlickInterval >> 1);
            }
        }
        public VECTOR2 CursorLocation
        {
            get
            {
                return typist.Font.Cursor(text, index);
            }
        }
        public IEnumerable<RECT> SelectedAreas
        {
            get
            {
                if (!HasSelected)
                    yield break;

                RECT area = typist.TextArea;

                Range<int> selected = new Range<int>(
                    _MATH.Max(from, to),
                    _MATH.Min(from, to));
                VECTOR2 start, end;

                int index = selected.Min;
                while (true)
                {
                    Range<int> range = FONT.IndexLineRange(text, index);
                    range.Min = _MATH.Max(selected.Min, range.Min);
                    if (range.Max >= selected.Max)
                        range.Max = selected.Max;
                    start = typist.Font.Cursor(text, range.Min);
                    end = typist.Font.Cursor(text, range.Max);
                    yield return new RECT(start, new VECTOR2(end.X - start.X, typist.Font.LineHeight));
                    if (range.Max == selected.Max)
                        yield break;
                    else
                        index = range.Max + 1;
                }
            }
        }
        public string Copied
        {
            get { return copied; }
        }
        /// <summary>
        /// 正在输入法里打字，此时的键盘快捷键操作将无效
        /// </summary>
        public virtual bool ImmCapturing
        {
            get { return false; }
        }

        public void Active(ITypist user)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            if (typist != null)
                if (user != typist)
                    Stop();
                else
                    return;
            if (!user.Readonly)
                OnActive(user);
            typist = user;

            previous = typist.Text;
            Text = previous;
            if (typist.ActiveSelect)
            {
                SelectAll();
            }
            else
            {
                int index = GetClickPositionIndex(Entry.Instance.INPUT.Pointer.Position);
                //if (index == -1)
                //{
                //    index = LastIndex;
                //}
                focusCtrl = !HasSelected && ctrl;
                Focus(index, focusCtrl || shift);
            }
            operations.Clear();
        }
        public void Stop()
        {
            if (stopping)
                return;
            if (typist == null)
                throw new InvalidOperationException();
            this.stopping = true;
            typist.OnStop(current);
            var temp = typist;
            typist = null;
            OnStop(typist);
            operations.Clear();
            this.index = -1;
            this.from = -1;
            this.to = -1;
            this.stopping = false;
        }
        protected virtual void OnActive(ITypist typist)
        {
        }
        protected virtual void OnStop(ITypist typist)
        {
        }
        private int GetClickPositionIndex(VECTOR2 cursor)
        {
            return typist.Font.CursorIndex(text, cursor - typist.TextArea.Location);
        }
        internal void Update(Entry e)
        {
            if (!IsActive)
                return;

            // && e.Input.Keyboard.Focused
            if (e.INPUT.Keyboard != null && !ImmCapturing)
            {
                shift = e.INPUT.Keyboard.Shift;
                ctrl = e.INPUT.Keyboard.Ctrl;

                // shortcut
                if (ctrl)
                {
                    if (e.INPUT.Keyboard.IsClick(PCKeys.A))
                    {
                        SelectAll();
                    }
                    else if (e.INPUT.Keyboard.IsClick(PCKeys.C))
                    {
                        Copy();
                    }
                    else if (e.INPUT.Keyboard.IsClick(PCKeys.V))
                    {
                        Paste();
                    }
                    else if (e.INPUT.Keyboard.IsClick(PCKeys.X))
                    {
                        Cut();
                    }
                    else if (e.INPUT.Keyboard.IsClick(PCKeys.Y))
                    {
                        Redo();
                    }
                    else if (e.INPUT.Keyboard.IsClick(PCKeys.Z))
                    {
                        Undo();
                    }
                }

                if (e.INPUT.Keyboard.IsInputKeyPressed(PCKeys.Left))
                {
                    Left();
                }
                else if (e.INPUT.Keyboard.IsInputKeyPressed(PCKeys.Right))
                {
                    Right();
                }

                if (e.INPUT.Keyboard.IsInputKeyPressed(PCKeys.Up))
                {
                    Up();
                }
                else if (e.INPUT.Keyboard.IsInputKeyPressed(PCKeys.Down))
                {
                    Down();
                }

                if (e.INPUT.Keyboard.IsClick(PCKeys.Home))
                {
                    Home();
                }
                else if (e.INPUT.Keyboard.IsClick(PCKeys.End))
                {
                    End();
                }

                if (e.INPUT.Keyboard.IsInputKeyPressed(PCKeys.Back))
                {
                    BackSpace();
                    UpdateEnd(e);
                    return;
                }
                else if (e.INPUT.Keyboard.IsInputKeyPressed(PCKeys.Delete))
                {
                    Delete();
                }

                if (e.INPUT.Keyboard.IsClick(PCKeys.Escape))
                {
                    if (HasSelected)
                    {
                        SelectCancel();
                        UpdateEnd(e);
                        return;
                    }
                    else
                    {
                        Escape();
                    }
                }

                if (e.INPUT.Keyboard.IsClick(PCKeys.Enter))
                {
                    if (ctrl || !typist.Multiple)
                    {
                        Enter();
                    }
                    else
                    {
                        NewLine();
                    }
                }
            }

            if (!IsActive)
                return;

            if (e.INPUT.Pointer != null && !e.INPUT.Pointer.Position.IsNaN())
            {
                // implement:
                // mouse click to focus or blur
                // mouse pressed to selected
                if (e.INPUT.Pointer.IsPressed(0))
                {
                    bool click = e.INPUT.Pointer.IsClick(0);
                    int mouse = GetClickPositionIndex(e.INPUT.Pointer.Position);
                    if (click)
                    {
                        if (!typist.ViewArea.Contains(e.INPUT.Pointer.Position))
                        {
                            Stop();
                            return;
                        }
                        if (e.INPUT.Pointer.ComboClick.IsDoubleClick)
                        {
                            ctrl = true;
                        }
                        focusCtrl = !HasSelected && ctrl;
                        if (focusCtrl)
                        {
                            index = -1;
                        }
                        Focus(mouse, focusCtrl || shift);
                    }
                    else
                    {
                        // ctrl + 点击选择 / 双击选择
                        // 则此按下选择无效
                        if (!focusCtrl)
                        {
                            Focus(mouse, true);
                        }
                    }
                }
                else
                {
                    focusCtrl = false;
                }
            }
            else
            {
                focusCtrl = false;
            }

            if (!IsActive)
                return;

            // input text
            string input;
            EInput result = InputCapture(out input);
            if (!string.IsNullOrEmpty(input))
            {
                switch (result)
                {
                    case EInput.Input:
                        Input(input);
                        break;

                    case EInput.Replace:
                        index += input.Length - current.Length;
                        //current = input;
                        Text = input;
                        break;

                    case EInput.Done:
                        Enter();
                        return;

                    case EInput.Canceled:
                        Escape();
                        return;
                }
            }

            UpdateEnd(e);
        }
        private void UpdateEnd(Entry e)
        {
            // set text to typist
            typist.Text = current;

            // cursor blick
            blick.Update(e.GameTime);
            if (blick.IsEnd)
            {
                blick.NextTurn();
            }
        }
        protected abstract EInput InputCapture(out string text);

        public void Input(string input)
        {
            if (typist.Readonly)
                return;
            int count = input.Length;
            char[] chars = input.ToCharArray();
            for (int i = 0; i < count; i++)
                if (!typist.Filter(ref chars[i]))
                    inputCache.Enqueue(chars[i]);
            input = new string(inputCache.ToArray());
            inputCache.Clear();

            OTextInput operation = new OTextInput(this);

            if (HasSelected)
                InternalDeleteSelect();
            int cursor = FONT.ChangeIndex(text, index, current);
            if (typist.MaxLength > 0)
            {
                if (text.Length > typist.MaxLength)
                    return;
                if (text.Length + input.Length > typist.MaxLength)
                {
                    // over max text length
                    input = input.Substring(0, typist.MaxLength - text.Length);
                    if (input.Length == 0)
                        return;
                }
            }
            Text = current.Insert(cursor, input);
            cursor += input.Length;
            if (typist.BreakLine)
            {
                cursor = FONT.ChangeIndexToBreakLine(current, cursor, text);
            }
            this.index = cursor;

            operation.Operation = new OTextInput(this);
            operations.Operate(operation);
        }
        public void Tab()
        {
            // 找出上一行中光标所在当前行的索引字符
            // 若为字符，则找到下个空格位置
            // 若为空格，则找到下个字符位置
            // 添加空格
            // 若字符数量过多，默认为4个空格
            // Shift: 向前退格
            Input("    ");
        }
        public void NewLine()
        {
            if (typist.Multiple)
                Input("\n");
            else
                Enter();
        }
        public void Paste()
        {
            string paste = copied;
            Paste(ref paste);
            if (!string.IsNullOrEmpty(paste))
                Input(paste);
        }
        protected virtual void Paste(ref string paste)
        {
        }

        private void InternalDeleteSelect()
        {
            if (typist.Readonly)
                return;
            int start, end;
            if (SelectOrder)
            {
                start = from;
                end = to;
            }
            else
            {
                start = to;
                end = from;
            }
            start = FONT.ChangeIndex(text, start, current);
            end = FONT.ChangeIndex(text, end, current);

            Text = current.Remove(start, end - start);
            ctrl = false;
            Focus(_MATH.Min(from, to), false);
        }
        protected void DeleteSelect()
        {
            OTextInput operation = new OTextInput(this);
            InternalDeleteSelect();
            operation.Operation = new OTextInput(this);
            operations.Operate(operation);
        }
        public void BackSpace()
        {
            if (!HasSelected)
                Select(index - 1, index);
            DeleteSelect();
        }
        public void Delete()
        {
            // shift + delete: 删除整行，光标坐标不变，同ctrl + x
            if (!HasSelected)
                Select(index + 1, index);
            DeleteSelect();
        }

        public void Select(int from, int to)
        {
            this.from = _MATH.Clamp(from, 0, text.Length);
            this.to = _MATH.Clamp(to, 0, text.Length);
        }
        public void SelectAll()
        {
            this.index = 0;
            from = 0;
            to = 0;
            Focus(LastIndex, true);
        }
        public void SelectCancel()
        {
            from = index;
            to = index;
        }
        public void Copy()
        {
            if (!typist.IsMask && HasSelected)
            {
                copied = SelectedText;
                Copy(copied);
            }
        }
        public virtual void Copy(string copy)
        {
        }
        public void Cut()
        {
            if (!HasSelected)
            {
                // select current line contains \r\n
                Range<int> range = FONT.IndexLineRange(text, index);
                Select(range.Min, range.Max);
            }
            Copy();
            DeleteSelect();
        }

        /// <summary>
        /// 设置索引
        /// </summary>
        /// <param name="index">当前索引</param>
        /// <param name="select">是否为选中索引</param>
        protected void Focus(int index, bool select)
        {
            select |= focusCtrl;
            if (index == -1)
            {
                Stop();
                return;
            }
            index = _MATH.Clamp(index, 0, LastIndex);

            // ctrl: 选中当前字符周边的相近字符
            // shift: 从上个索引出选择到当前索引
            // 包括ctrl和shift的组合

            int start, end;
            if (HasSelected && select)
            {
                start = from;
                end = to;
            }
            else
            {
                start = this.index;
                end = this.index;
            }
            if (focusCtrl || ctrl)
            {
                Range<int> range = FONT.SimilarChar(text, index);
                if (this.index == -1)
                {
                    start = range.Min;
                    end = range.Max;
                }
                else
                {
                    // 选择顺序切换时
                    // 逆序 => 顺序：from计算相近词，start=min，end为range.Max
                    // 顺序 => 逆序：from计算相近词，start=max，end为range.Min
                    if (start < end)
                    {
                        if (range.Max < start)
                        {
                            // 顺序 => 逆序
                            Range<int> original = FONT.SimilarChar(text, from);
                            start = original.Max;
                            end = range.Min;
                        }
                    }
                    if (start > end)
                    {
                        if (range.Min > start)
                        {
                            // 逆序 => 顺序
                            Range<int> original = FONT.SimilarChar(text, from);
                            start = original.Min;
                            end = range.Max;
                        }
                    }
                    if (start <= end)
                    {
                        start = _MATH.Min(start, range.Min);
                        end = range.Max;
                    }
                    else
                    {
                        start = _MATH.Max(start, range.Max);
                        end = range.Min;
                    }
                }
                index = end;
            }
            if (select)
            {
                if (start != end)
                {
                    if (index <= this.index)
                    {
                        end = _MATH.Min(index, end);
                    }
                    else
                    {
                        end = _MATH.Max(index, end);
                    }
                }
                else
                {
                    start = this.index;
                    end = index;
                }
            }
            else
            {
                start = end;
            }
            if (start != end)
            {
                Select(start, end);
                index = end;
            }
            else
            {
                SelectCancel();
            }
            if (this.index != index)
            {
                this.index = index;
                blick.Reset();
            }
        }
        public void Left()
        {
            if (HasSelected && !shift)
            {
                Focus(_MATH.Min(from, to), false);
            }
            else
            {
                int target = _MATH.Max(index - 1, 0);
                if (ctrl)
                {
                    Range<int> range = FONT.SimilarChar(text, target);
                    target = range.Min;
                    ctrl = false;
                }
                Focus(target, shift);
            }
        }
        public void Home()
        {
            Range<int> range = FONT.IndexLineRange(text, index);
            Focus(range.Min, shift);
        }
        public void Right()
        {
            if (HasSelected && !shift)
            {
                Focus(_MATH.Max(from, to), false);
            }
            else
            {
                int target = _MATH.Min(index + 1, LastIndex);
                if (ctrl)
                {
                    Range<int> range = FONT.SimilarChar(text, target);
                    target = range.Max;
                    ctrl = false;
                }
                Focus(target, shift);
            }
        }
        public void End()
        {
            Range<int> range = FONT.IndexLineRange(text, index);
            Focus(range.Max, shift);
        }
        public void Up()
        {
            if (index <= lines[0].Length)
            {
                if (!shift)
                {
                    SelectCancel();
                }
            }
            else
            {
                VECTOR2 cursor = typist.Font.Cursor(text, index);
                cursor.Y -= typist.Font.LineHeight;
                // 没选中，ctrl + shift，直接选中了上面的单词，向下是对的，参照向下
                Focus(typist.Font.CursorIndex(text, cursor), shift);
            }
        }
        public void Down()
        {
            if (index >= LastIndex - lines[lines.Length - 1].Length)
            {
                if (!shift)
                {
                    SelectCancel();
                }
            }
            else
            {
                VECTOR2 cursor = typist.Font.Cursor(text, index);
                cursor.Y += typist.Font.LineHeight;
                Focus(typist.Font.CursorIndex(text, cursor), shift);
            }
        }
        // ITypist.TextArea PageUp PageDown

        public void Undo()
        {
            operations.Undo();
        }
        public void Redo()
        {
            operations.Redo();
        }
        public void Enter()
        {
            Stop();
        }
        public void Escape()
        {
            current = previous;
            typist.Text = previous;
            Stop();
        }
    }
    /// <summary>
    /// 只有实现此接口的对象允许使用文本输入设备
    /// </summary>
    public interface ITypist
    {
        /// <summary>激活设备时是否要选中所有文字</summary>
        bool ActiveSelect { get; }
        /// <summary>计算光标位置</summary>
        FONT Font { get; }
        /// <summary>未经处理的源文字</summary>
        string Text { get; set; }
        /// <summary>只读则不允许输入和删除操作，不过可以复制</summary>
        bool Readonly { get; }
        /// <summary>控制是否超出文字区域自动换行，用于计算光标位置</summary>
        bool BreakLine { get; }
        /// <summary>用于判断回车是换行还是确定</summary>
        bool Multiple { get; }
        /// <summary>遮挡(密码)模式时不可复制选中</summary>
        bool IsMask { get; }
        /// <summary>文字区域，用于计算光标位置</summary>
        RECT TextArea { get; }
        /// <summary>显示区域，用于点击取消编辑</summary>
        RECT ViewArea { get; }
        /// <summary>限制输入最大长度</summary>
        int MaxLength { get; }
        /// <summary>使用输入设备的控件是否激活，用于控制设备自动关闭</summary>
        bool IsActive { get; }
        /// <summary>文字筛选，用于例如只能输入数字</summary>
        /// <param name="c">允许替换的单个字符</param>
        /// <returns>true: 被筛选掉的非法字符 / false: 合法字符</returns>
        bool Filter(ref char c);
        /// <summary>设备关闭时回调源的处理程序</summary>
        /// <param name="result">最终文本属性</param>
        void OnStop(string result);
    }


    #endregion


    #region Content


    public abstract class ContentPipeline
    {
        /// <summary>
        /// 可处理的源文件类型
        /// <para>null: 可以处理所有类型</para>
        /// </summary>
        public abstract IEnumerable<string> SuffixProcessable { get; }
        /// <summary>
        /// 源文件输出以及最终能加载的文件类型，以源文件类型加载则值应为null
        /// </summary>
        public virtual string FileType { get { return null; } }
        protected internal ContentManager Manager
        {
            get;
            internal set;
        }
        protected _IO.iO IO
        {
            get
            {
                if (Manager == null)
                    return _IO._iO;
                return Manager.IODevice;
            }
        }

        //protected ContentPipeline GetPipelineExceptSelf(string file)
        //{
        //    return Manager.FindPipeline(this, ref file);
        //}
        public virtual void LoadMetadata(string file)
        {
            throw new NotImplementedException();
        }
        public virtual bool Processable(ref string file)
        {
            string suffix = file.Substring(file.LastIndexOf(".") + 1);

            if (FileType == suffix)
                return true;

            if (FileType == null)
            {
                // 所有类型都允许处理
                if (SuffixProcessable == null)
                    return true;

                if (SuffixProcessable.Contains(suffix))
                    return true;
            }
            else
            {
                if (SuffixProcessable != null && SuffixProcessable.Contains(suffix))
                {
                    file = Path.ChangeExtension(file, FileType);
                    return true;
                }
            }

            return false;
        }
        public virtual byte[] Process(string file)
        {
            throw new NotImplementedException();
        }
        protected internal abstract Content Load(string file);
        protected internal abstract void LoadAsync(AsyncLoadContent async);

        public static void Wait<T>(AsyncLoadContent load, T async, Func<T, Content> call) where T : Async
        {
            if (load == null)
                throw new ArgumentNullException("load");

            if (async == null)
                throw new ArgumentNullException("async");

            if (call == null)
                throw new ArgumentNullException("call");

            if (async.IsEnd)
            {
                if (async.IsSuccess)
                {
                    load.SetData(call(async));
                }
                else if (async.IsFaulted)
                {
                    load.Error(async.FaultedReason);
                }
                else
                {
                    load.Cancel();
                }
                return;
            }

            EntryService.Instance.SetCoroutine(
                new CorDelegate(
                    (time) =>
                    {
                        if (load.IsEnd)
                        {
                            async.Cancel();
                            return true;
                        }

                        if (async.IsEnd)
                        {
                            if (async.IsSuccess)
                            {
                                load.SetData(call(async));
                            }
                            else if (async.IsFaulted)
                            {
                                load.Error(async.FaultedReason);
                            }
                            else
                            {
                                load.Cancel();
                            }
                            return true;
                        }

                        return false;
                    }));
        }
        public static void Wait<T>(AsyncLoadContent load, IEnumerable<T> asyncs, Func<Content> result) where T : Async
        {
            Wait(load, asyncs, 0, result);
        }
        /// <summary>
        /// 等待其它异步队列完成
        /// </summary>
        /// <typeparam name="T">等待异步类型</typeparam>
        /// <param name="load">异步加载结果</param>
        /// <param name="asyncs">等待的异步队列</param>
        /// <param name="executeCount">队列有完成时继续执行的个数，等于0则全部同步完成</param>
        /// <param name="result">完成时加载的结果</param>
        public static void Wait<T>(AsyncLoadContent load, IEnumerable<T> asyncs, byte executeCount, Func<Content> result) where T : Async
        {
            if (load == null)
                throw new ArgumentNullException("load");

            if (asyncs == null)
                throw new ArgumentNullException("asyncs");

            if (result == null)
                throw new ArgumentNullException("call");

            Queue<T> queue = new Queue<T>(asyncs);

            EntryService.Instance.SetCoroutine(
                new CorDelegate(
                    (time) =>
                    {
                        var async = queue.Peek();
                        if (async == null)
                        {
                            load.Error(new ArgumentNullException("async"));
                        }

                        if (load.IsEnd)
                        {
                            while (queue.Count > 0)
                            {
                                async = queue.Dequeue();
                                async.Cancel();
                            }
                            return true;
                        }

                        int temp = 0;
                        while (true)
                        {
                            if (async.IsEnd)
                            {
                                queue.Dequeue();
                                if (async.IsSuccess)
                                {
                                    if (queue.Count > 0)
                                    {
                                        if (executeCount > 0 && ++temp >= executeCount)
                                        {
                                            return false;
                                        }
                                        else
                                        {
                                            async = queue.Peek();
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        var content = result();
                                        if (content == null)
                                        {
                                            load.Error(new InvalidOperationException("Wait for the async queue must have a content return."));
                                        }
                                        else
                                        {
                                            load.SetData(content);
                                        }
                                    }
                                }
                                else if (async.IsFaulted)
                                {
                                    load.Error(async.FaultedReason);
                                }
                                else
                                {
                                    load.Cancel();
                                }
                                return true;
                            }
                            else
                            {
                                break;
                            }
                        }

                        return false;
                    }));
        }
        public static void Wait<T>(AsyncLoadContent load, T async, Action<T> next) where T : Async
        {
            if (load == null)
                throw new ArgumentNullException("load");

            if (async == null)
                throw new ArgumentNullException("async");

            if (next == null)
                throw new ArgumentNullException("call");

            if (async.IsEnd)
            {
                if (async.IsSuccess)
                {
                    next(async);
                }
                else if (async.IsFaulted)
                {
                    load.Error(async.FaultedReason);
                }
                else
                {
                    load.Cancel();
                }
                return;
            }

            EntryService.Instance.SetCoroutine(
                new CorDelegate(
                    (time) =>
                    {
                        if (load.IsEnd)
                        {
                            async.Cancel();
                            return true;
                        }

                        if (async.IsEnd)
                        {
                            if (async.IsSuccess)
                            {
                                next(async);
                            }
                            else if (async.IsFaulted)
                            {
                                load.Error(async.FaultedReason);
                            }
                            else
                            {
                                load.Cancel();
                            }
                            return true;
                        }

                        return false;
                    }));
        }
    }
    public abstract class ContentPipelineBinary : ContentPipeline
    {
        protected internal sealed override Content Load(string file)
        {
            return LoadFromBytes(Manager.IODevice.ReadByte(file));
        }
        protected internal sealed override void LoadAsync(AsyncLoadContent async)
        {
            var read = Manager.IODevice.ReadAsync(async.File);
            if (read.IsEnd)
            {
                async.SetData(LoadFromBytes(read.Data));
            }
            else
            {
                Wait(async, read,
                    (result) =>
                    {
                        return LoadFromBytes(result.Data);
                    });
            }
        }
        public abstract Content LoadFromBytes(byte[] bytes);
    }
    public abstract class ContentPipelineText : ContentPipelineBinary
    {
        public sealed override Content LoadFromBytes(byte[] bytes)
        {
            return InternalLoad(Manager.IODevice.ReadPreambleText(bytes));
        }
        protected abstract Content InternalLoad(string text);
    }

    /*
     * Content加载方式
     * 1. 完全同步加载
     *  黑屏卡住加载，适用于短时间加载
     * 2. 协程同步加载
     *  进度条加载，适用于长时间且需要保证加载项完成才能继续的加载
     * 3. 完全异步加载
     *  网页式局部刷新，未加载不影响程序继续
     * 
     * 跨平台通用方式：协程同步加载
     */
    public abstract class Content : IDisposable
    {
        internal ContentManager ContentManager;
        protected internal string _Key;
        /// <summary>Cache不会被Dispose</summary>
        internal bool IsMain;

        public string Key
        {
            get { return _Key; }
        }
        // || ContentManager[Key] != this: Cache
        public abstract bool IsDisposed { get; }

        protected virtual void CacheDispose() { }
        protected internal abstract void InternalDispose();
        protected internal virtual Content Cache()
        {
            return this;
        }
        public void Dispose()
        {
            if (!IsMain)
            {
                return;
            }
            if (ContentManager == null)
            {
                if (!IsDisposed)
                {
                    InternalDispose();
                }
            }
            else
            {
                ContentManager.Dispose(_Key);
            }
        }

        public static bool Serializer(ByteRefWriter writer, object value, Type type)
        {
            bool nil = value == null;
            Content content = null;
            if (!nil)
            {
                content = value as Content;
                if (content == null)
                    return false;
            }
            else if (!type.Is(typeof(Content)))
                return false;

            if (content == null || string.IsNullOrEmpty(content.Key))
                writer.Write((string)null);
            else
                writer.Write(content.Key);

            return true;
        }
        public static Func<Type, Func<ByteRefReader, object>> Deserializer(ContentManager content)
        {
            return (type) =>
            {
                if (type.Is(typeof(Content)))
                    return (reader) =>
                    {
                        string key;
                        reader.Read(out key);
                        if (key == null)
                            return null;
                        else
                            // 同步加载
                            return content.Load<TEXTURE>(key);
                    };
                else
                    return null;
            };
        }
    }
    public sealed class AsyncLoadContent : AsyncData<Content>
    {
        private Action<Content> callback;
        private Action<Exception> exCallback;
        private ContentPipeline pipeline;
        private Queue<AsyncLoadContent> queues;

        private ContentManager Manager
        {
            get { return pipeline.Manager; }
        }
        public string Key
        {
            get;
            private set;
        }
        public string File
        {
            get;
            private set;
        }

        internal AsyncLoadContent(ContentPipeline pipeline, string key, string file)
        {
            this.pipeline = pipeline;
            this.Key = key;
            this.File = file;
        }

        internal AsyncLoadContent Load<T>(Action<T> callback, Action<Exception> exCallback) where T : Content
        {
            CheckCompleted();

            if (State == EAsyncState.Running)
            {
                AsyncLoadContent async = new AsyncLoadContent(pipeline, Key, File);
                async.Load(callback, exCallback);
                if (queues == null)
                    queues = new Queue<AsyncLoadContent>();
                queues.Enqueue(async);
                return async;
            }

            if (callback != null)
            {
                Action<Content> _callback = callback as Action<Content>;
                if (_callback == null)
                {
                    _callback = (c) =>
                    {
                        // 强制类型转换不触发explicit/implicit转换操作符
                        //callback(c as T);
                        // HACK: JS的as关键字暂未实现对explicit的调用，所以暂时使用强制类型转换
                        callback((T)c.Cache());
                    };
                }
                this.callback = _callback;
            }
            if (exCallback != null)
            {
                this.exCallback = exCallback;
            }
            Run();
            return null;
        }
        /// <summary>
        /// Queue加载的项也会重复的设置
        /// 像PIECE这样的内容，重复设置的Content将前后不相等，导致Manager[Key]里Dispose之前的Content
        /// </summary>
        [Code(ECode.BUG)]
        protected override void OnSetData(ref Content data)
        {
            if (data != null
                // 临时解决BUG
                && !Manager.IsLoaded(Key))
            {
                Manager[Key] = data;
                //data = data.Cache();
            }
        }
        protected sealed override void InternalComplete()
        {
            Manager.RemoveAsync(this);

            if (State == EAsyncState.Success)
            {
                if (callback != null)
                {
                    callback(Data);
                    callback = null;
                }
                if (queues != null)
                {
                    while (queues.Count > 0)
                    {
                        var queue = queues.Dequeue();
                        if (!queue.IsEnd)
                        {
                            queue.SetData(Data);
                        }
                    }
                }
            }
            else if (State == EAsyncState.Faulted)
            {
                if (exCallback != null)
                {
                    exCallback(FaultedReason);
                }
                if (queues != null)
                {
                    while (queues.Count > 0)
                    {
                        var queue = queues.Dequeue();
                        if (!queue.IsEnd)
                        {
                            queue.Error(FaultedReason);
                        }
                    }
                }
            }
            else if (State == EAsyncState.Canceled)
            {
                if (queues != null)
                {
                    while (queues.Count > 0)
                    {
                        var queue = queues.Dequeue();
                        if (!queue.IsEnd)
                        {
                            queue.Cancel();
                        }
                    }
                }
            }
        }
    }
    [ADevice]
    public class ContentManager
    {
        private _IO.iO ioDevice;
        private List<ContentPipeline> contentPipelines = new List<ContentPipeline>();
        private Dictionary<string, Content> contents = new Dictionary<string, Content>();
        private Dictionary<string, AsyncLoadContent> asyncs = new Dictionary<string, AsyncLoadContent>();

        public _IO.iO IODevice
        {
            get { return ioDevice; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("IODevice");

                if (!IsDisposed)
                    throw new InvalidOperationException("Can't change the root directory with not null ContentManager.");

                ioDevice = value;
            }
        }
        public string RootDirectory
        {
            get { return IODevice.RootDirectory; }
            set
            {
                if (IODevice.RootDirectory == value)
                    return;

                if (!IsDisposed)
                    throw new InvalidOperationException("Can't change the root directory with not null ContentManager.");

                IODevice.RootDirectory = value;
            }
        }
        public IEnumerable<ContentPipeline> ContentPipelines
        {
            get
            {
                for (int i = 0; i < contentPipelines.Count; i++)
                {
                    yield return contentPipelines[i];
                }
            }
        }
        public IEnumerable<string> LoadableContentFileSuffix
        {
            get
            {
                HashSet<string> set = new HashSet<string>();
                foreach (ContentPipeline item in ContentPipelines)
                {
                    if (item.FileType != null)
                    {
                        if (set.Add(item.FileType))
                        {
                            yield return item.FileType;
                            continue;
                        }
                    }

                    var suffix = item.SuffixProcessable;
                    if (suffix == null)
                    {
                        if (set.Add(null))
                        {
                            yield return null;
                        }
                    }
                    else
                    {
                        foreach (var s in suffix)
                        {
                            if (set.Add(s))
                            {
                                yield return s;
                            }
                        }
                    }
                }
            }
        }
        public Content this[string key]
        {
            get
            {
                if (IsLoaded(key))
                    return contents[key].Cache();
                else
                    return null;
            }
            set
            {
                if (value == null)
                    return;

                if (value.ContentManager != null && value.ContentManager != this)
                {
                    value.ContentManager.contents.Remove(value._Key);
                }

                Content content;
                if (contents.TryGetValue(key, out content))
                {
                    if (content != value)
                    {
                        content.Dispose();
                    }
                }
                AddContent(key, value);
            }
        }
        public bool IsDisposed
        {
            get { return contents.Count == 0 && asyncs.Count == 0; }
        }

        [ADeviceNew]
        public ContentManager()
            : this(_IO._iO)
        {
        }
        public ContentManager(_IO.iO io)
        {
            this.IODevice = io;
        }

        public void AddFirstPipeline(ContentPipeline pipeline)
        {
            if (pipeline.Manager != null)
                throw new InvalidOperationException("Pipeline has been set by other manager.");
            pipeline.Manager = this;
            this.contentPipelines.Insert(0, pipeline);
        }
        public void AddPipeline(params ContentPipeline[] pipelines)
        {
            foreach (var pipeline in pipelines)
            {
                if (pipeline.Manager != null)
                    throw new InvalidOperationException("Pipeline has been set by other manager.");
                pipeline.Manager = this;
                this.contentPipelines.Add(pipeline);
            }
        }
        public void RemovePipeline(params ContentPipeline[] pipelines)
        {
            foreach (ContentPipeline pipeline in pipelines)
            {
                if (this.contentPipelines.Remove(pipeline))
                {
                    pipeline.Manager = null;
                }
            }
        }
        //internal ContentPipeline FindPipeline(ContentPipeline expect, ref string file)
        //{
        //    //for (int i = contentPipelines.Count - 1; i >= 0; i--)
        //    //{
        //    //    if (contentPipelines[i] == expect)
        //    //        break;
        //    //    if (contentPipelines[i].Processable(ref file))
        //    //        return contentPipelines[i];
        //    //}
        //    bool flag = false;
        //    foreach (var pipeline in contentPipelines)
        //    {
        //        if (flag)
        //        {
        //            if (pipeline.Processable(ref file))
        //                return pipeline;
        //        }
        //        else
        //        {
        //            flag = pipeline == expect;
        //        }
        //    }
        //    return null;
        //}
        private ContentPipeline FindPipeline(ref string file)
        {
            // '\' or '/', use '\' till IO.BuildPath
            //file = file.Replace('/', _IO.SPLIT);
            foreach (var pipeline in contentPipelines)
                if (pipeline.Processable(ref file))
                    return pipeline;
            throw new NotImplementedException(string.Format("Not found the fixed ContentPipeline for \"{0}\".", file));
        }
        public bool IsLoaded(string key)
        {
            Content value;
            if (contents.TryGetValue(key, out value))
                return !value.IsDisposed;
            return false;
        }
        public bool IsLoaded(string key, out Content content)
        {
            content = null;
            if (contents.TryGetValue(key, out content))
                return !content.IsDisposed;
            return false;
        }
        public void AddContent(string key, Content content)
        {
            content.IsMain = true;
            content._Key = key;
            content.ContentManager = this;
            contents[key] = content;
        }
        private void InternalDisposeContent(Content content)
        {
            content.InternalDispose();
            content.ContentManager = null;
            content._Key = null;
        }
        public void Dispose()
        {
            StopAsyncLoading();
            foreach (Content content in contents.Values)
                InternalDisposeContent(content);
            contents.Clear();
        }
        public void Dispose(string key)
        {
            Content content;
            if (contents.TryGetValue(key, out content))
            {
                InternalDisposeContent(content);
                contents.Remove(key);
            }
        }
        internal void Dispose(Content content)
        {
            if (content != null && content.ContentManager == this)
            {
                Dispose(content._Key);
            }
        }
        public Content Load(string file)
        {
            file = FilePathUnify(file);
            return InternalLoad(file, file);
        }
        public Content Load(string key, string file)
        {
            //if (asyncs.ContainsKey(key))
            //    throw new InvalidOperationException("Content is be loading by async.");
            return InternalLoad(key, FilePathUnify(file));
        }
        private Content InternalLoad(string key, string file)
        {
            Content content;
            if (!IsLoaded(key, out content))
            {
                var pipeline = FindPipeline(ref file);
                content = pipeline.Load(file);
                AddContent(key, content);
            }

            content = content.Cache();
            AsyncLoadContent async;
            if (asyncs.TryGetValue(key, out async))
                async.SetData(content);

            return content;
        }
        public T Load<T>(string file) where T : Content
        {
            return (T)Load(file, file);
        }
        public T Load<T>(string key, string file) where T : Content
        {
            return (T)Load(key, file);
        }
        public void StopAsyncLoading()
        {
            foreach (AsyncLoadContent loading in asyncs.Values.ToArray())
                loading.Cancel();
            asyncs.Clear();
        }
        internal void RemoveAsync(AsyncLoadContent async)
        {
            asyncs.Remove(async.Key);
        }
        private AsyncLoadContent InternalLoadAsync<T>(string key, string file, Action<T> callback, Action<Exception> exCallback) where T : Content
        {
            AsyncLoadContent async;
            if (asyncs.TryGetValue(key, out async))
            {
                // 正在加载时，异步加载内部会生成加载队列
                async = async.Load(callback, exCallback);
                if (async == null)
                    throw new ArgumentNullException("AsyncLoadContentQueue");
                return async;
            }

            ContentPipeline pipeline = FindPipeline(ref file);
            async = new AsyncLoadContent(pipeline, key, file);
            async.Load(callback, exCallback);

            Content content;
            if (IsLoaded(key, out content))
                // 已加载则直接完成
                async.SetData(content);
            else
                // 未加载则交由管道加载
                pipeline.LoadAsync(async);

            // 已加载完成则不进入队列
            if (!async.IsEnd)
                asyncs.Add(key, async);
            return async;
        }
        public AsyncLoadContent LoadAsync<T>(string key, string file, Action<T> callback, Action<Exception> exCallback) where T : Content
        {
            return InternalLoadAsync<T>(key, FilePathUnify(file), callback, exCallback);
        }
        public AsyncLoadContent LoadAsync<T>(string file, Action<T> callback) where T : Content
        {
            file = FilePathUnify(file);
            return InternalLoadAsync(file, file, callback, null);
        }
        public AsyncLoadContent LoadAsync<T>(string file, Action<T> callback, Action<Exception> exCallback) where T : Content
        {
            file = FilePathUnify(file);
            return InternalLoadAsync(file, file, callback, exCallback);
        }
        public AsyncLoadContent LoadAsync<T>(string key, string file, Action<T> callback) where T : Content
        {
            return InternalLoadAsync(key, FilePathUnify(file), callback, null);
        }
        private AsyncLoadContent InternalReplaceAsync<T>(string key, string file, Action<T> callback, Action<Exception> exCallback) where T : Content
        {
            AsyncLoadContent async;
            if (asyncs.TryGetValue(key, out async))
            {
                // 正在加载时，异步加载内部会生成加载队列
                async = async.Load(callback, exCallback);
                if (async == null)
                    throw new ArgumentNullException("AsyncLoadContentQueue");
                return async;
            }

            ContentPipeline pipeline = FindPipeline(ref file);
            async = new AsyncLoadContent(pipeline, key, file);
            async.Load(callback, exCallback);

            Content content;
            if (IsLoaded(key, out content))
            {
                // 已加载则卸载掉之前的资源
                content.Dispose();
            }
            //else
            // 未加载则交由管道加载
            pipeline.LoadAsync(async);

            // 已加载完成则不进入队列
            if (!async.IsEnd)
                asyncs.Add(key, async);
            return async;
        }
        public AsyncLoadContent ReplaceAsync<T>(string key, string file, Action<T> callback, Action<Exception> exCallback) where T : Content
        {
            return InternalReplaceAsync<T>(key, FilePathUnify(file), callback, exCallback);
        }
        public AsyncLoadContent ReplaceAsync<T>(string file, Action<T> callback) where T : Content
        {
            file = FilePathUnify(file);
            return InternalReplaceAsync(file, file, callback, null);
        }
        public AsyncLoadContent ReplaceAsync<T>(string file, Action<T> callback, Action<Exception> exCallback) where T : Content
        {
            file = FilePathUnify(file);
            return InternalReplaceAsync(file, file, callback, exCallback);
        }
        public AsyncLoadContent ReplaceAsync<T>(string key, string file, Action<T> callback) where T : Content
        {
            return InternalReplaceAsync(key, FilePathUnify(file), callback, null);
        }
        public T New<T>(string key, string file, out string _key) where T : Content
        {
            _key = IdentityKey(key);
            return Load<T>(_key, file);
        }
        //public void Add(string key, Content target)
        //{
        //    if (!IsLoaded(key) && target != null)
        //    {
        //        AddContent(key, target);
        //    }
        //}
        //public void Modify(string key, Content target)
        //{
        //    Dispose(key);
        //    Add(key, target);
        //}
        public string IdentityKey(string key)
        {
            int index = 0;
            bool flag = false;
            while (contents.ContainsKey(key))
            {
                flag = true;
                index++;
            }
            if (flag)
                key = key + index;
            return key;
        }

        public static string PathNonSuffix(string fileName)
        {
            int index = fileName.LastIndexOf('.');
            if (index != -1)
                fileName = fileName.Substring(0, index);
            fileName = fileName.Replace('\\', '/');
            return fileName;
        }
        public static string FilePathUnify(string filePath)
        {
            return filePath.Replace('\\', '/');
        }
    }


    #endregion


    #region Audio


    public enum ESoundState
    {
        Stopped = 0,
        Playing = 1,
        Paused = 2,
    }
    public abstract class SOUND : Content
    {
        public static string[] FileTypes =
		{
			"wav",
			"ogg",
			"mp3",
		};
        internal SoundSource Source;
        public ESoundState State
        {
            get
            {
                if (Source == null)
                    return ESoundState.Stopped;
                return Source.State;
            }
        }
    }
    /// <summary>可延迟加载的声音</summary>
    //internal sealed class SOUND_DELAY : SOUND
    //{
    //    public AsyncLoadContent Async;
    //    public SOUND Sound;

    //    public override bool IsDisposed
    //    {
    //        get { return Async == null || (Sound == null || Sound.IsDisposed); }
    //    }
    //    protected internal override void InternalDispose()
    //    {
    //        if (Async != null && !Async.IsEnd)
    //            Async.Cancel();
    //        Async = null;
    //        if (Sound != null)
    //            Sound.InternalDispose();
    //    }
    //}
    public abstract class SoundSource
    {
        /// <summary>播放状态</summary>
        protected internal abstract ESoundState State { get; }
        /// <summary>音量(0 ~ 1)</summary>
        protected internal abstract float Volume { get; set; }
        /// <summary>声道(-1左 ~ 1右)</summary>
        protected internal abstract float Channel { get; set; }
        /// <summary>设置是否循环</summary>
        protected internal abstract void SetLoop(bool loop);
    }
    public interface IAudioSource
    {
        float SourceX { get; }
        float SourceY { get; }
    }
    public class PAudioSource : IAudioSource
    {
        public VECTOR2 Position;
        public float SourceX
        {
            get { return Position.X; }
        }
        public float SourceY
        {
            get { return Position.Y; }
        }
    }
    [ADevice]
    public abstract class AUDIO
    {
        /*
         * 音效播放的情况
         * 1. 无效果播放：无论位置，所有人听到的效果是一样的
         * 2. 自动效果播放：游戏中根据播放源的声音与摄像机的距离调整音量
         * *3. 半自动效果播放：音效第一次播放时自动，之后音源和摄像机的关系不再影响音效
         * 
         * 1. 总队列播放：任何下一个总对列播放的音效都将打断当前总队列的音效
         * 2. 对象队列播放：任何来自同一对象的下一个音效都将打断这个对象队列的音效
         * 3. 无队列播放：无论怎样都不会打断音效
         */

        class Sound : PoolItem
        {
            public IAudioSource AudioSource;
            public SoundSource SoundSource;
        }

        private float volume = 1;
        private Sound sound = new Sound();
        private Dictionary<object, Sound> sources = new Dictionary<object, Sound>();
        private Pool<Sound> sounds = new Pool<Sound>();
        private Pool<Sound> freeSounds = new Pool<Sound>();
        private ContentManager content;
        private bool isInternalContent;
        public IAudioSource Listener;
        /// <summary>3D音效能听到声音的最远距离</summary>
        public float MaxDistance = 750;
        private float _maxDistanceSquared;
        private float _maxDistanceSquaredD;

        /// <summary>0 ~ 1</summary>
        public virtual float Volume
        {
            get { return volume; }
            set { volume = value; }
        }
        /// <summary>是否静音</summary>
        public virtual bool Mute { get; set; }
        /// <summary>侦听坐标</summary>
        public VECTOR2 ListenerLocation
        {
            get
            {
                if (Listener == null)
                    return Entry._GRAPHICS.GraphicsSize / 2;
                return new VECTOR2(Listener.SourceX, Listener.SourceY);
            }
        }
        public bool IsDisposed
        {
            get
            {
                return sound == null ||
                    sources.Count == 0 ||
                    sounds.Count == 0;
            }
        }
        public ContentManager Content
        {
            get
            {
                if (content == null)
                {
                    content = Entry.Instance.NewContentManager();
                    isInternalContent = true;
                }
                return content;
            }
            set
            {
                if (content != null && !content.IsDisposed && isInternalContent)
                    content.Dispose();
                content = value;
                isInternalContent = false;
            }
        }

        protected internal virtual void Update(GameTime time)
        {
            VECTOR2 listener = ListenerLocation;
            _maxDistanceSquared = MaxDistance * MaxDistance;
            _maxDistanceSquaredD = 1 / _maxDistanceSquared;
            // main
            Update(sound, ref listener);
            foreach (var item in freeSounds)
            {
                if (item.SoundSource == null || item.SoundSource.State == ESoundState.Stopped)
                    freeSounds.RemoveAt(item);
            }
            foreach (var item in sounds)
            {
                if (item.SoundSource == null || item.SoundSource.State == ESoundState.Stopped)
                    // 播放完毕
                    sounds.RemoveAt(item);
                else
                    Update(item, ref listener);
            }
        }
        private void Update(Sound sound, ref VECTOR2 listener)
        {
            if (sound.AudioSource != null)
            {
                sound.SoundSource.Volume = Volume * GetVolume(ref listener, sound.AudioSource);
                sound.SoundSource.Channel = GetChannel(listener.X, sound.AudioSource.SourceX);
            }
        }
        public void StopMusic()
        {
            Stop(sound);
        }
        public void StopVoice(object key)
        {
            Sound sound;
            if (sources.TryGetValue(key, out sound) && sound.SoundSource != null && sound.SoundSource.State == ESoundState.Playing)
            {
                Stop(sound);
                //sounds.Remove(key);
            }
        }
        private void Stop(Sound sound)
        {
            if (sound.SoundSource != null)
            {
                if (freeSounds[sound.PoolIndex] == sound)
                    freeSounds.RemoveAt(sound);
                else if (sounds[sound.PoolIndex] == sound)
                    sounds.RemoveAt(sound);
                Stop(sound.SoundSource);
            }
            //sound.SoundEffect.Unload();
        }
        private float GetVolume(ref VECTOR2 reference, IAudioSource source)
        {
            VECTOR2 sourceP = new VECTOR2(source.SourceX, source.SourceY);
            if (reference.X == sourceP.X && reference.Y == sourceP.Y)
                return 1;
            float d;
            VECTOR2.DistanceSquared(ref reference, ref sourceP, out d);
            if (d == 0)
                return 1.0f;
            else if (d > _maxDistanceSquared)
                return 0f;
            else
                return (_maxDistanceSquared - d) * _maxDistanceSquaredD;
        }
        private float GetChannel(float x, float targetX)
        {
            if (MaxDistance == 0)
                return 0;
            return _MATH.Clamp((targetX - x) / MaxDistance, -1, 1);
        }
        private void Play(Sound sound, SOUND wave, float volume, float channel, bool loop)
        {
            if (Volume == 0 || volume == 0 || Mute)
                return;
            Play(ref sound.SoundSource, wave);
            if (sound.SoundSource == null)
                return;
            wave.Source = sound.SoundSource;
            sound.SoundSource.SetLoop(loop);
            sound.SoundSource.Volume = volume * this.Volume;
            sound.SoundSource.Channel = channel;
        }
        protected abstract void Play(ref SoundSource source, SOUND wave);
        public void PauseMusic()
        {
            if (this.sound.SoundSource != null)
                Pause(this.sound.SoundSource);
        }
        public void Pause(SOUND sound)
        {
            if (sound.Source != null)
                Pause(sound.Source);
        }
        public void ResumeMusic()
        {
            if (this.sound.SoundSource != null)
                Resume(this.sound.SoundSource);
        }
        public void Resume(SOUND sound)
        {
            if (sound.Source != null)
                Resume(sound.Source);
        }
        protected virtual void Pause(SoundSource source) { }
        protected virtual void Resume(SoundSource source) { }
        protected abstract void Stop(SoundSource source);
        public SOUND PlayMusic(string name)
        {
            return PlayMusic(name, null);
        }
        public SOUND PlayMusic(string name, IAudioSource source)
        {
            SOUND load = Content.Load<SOUND>(name);
            PlayMusic(load, source);
            return load;
        }
        public SOUND PlayMusic(string name, float volume, float channel)
        {
            SOUND load = Content.Load<SOUND>(name);
            PlayMusic(load, volume, channel);
            return load;
        }
        public void PlayMusic(SOUND sound, IAudioSource source)
        {
            StopMusic();

            if (sound == null)
                return;

            this.sound.AudioSource = source;

            float volume = 1, channel = 0;
            if (source != null)
            {
                VECTOR2 listener = ListenerLocation;
                volume = GetVolume(ref listener, source);
                channel = GetChannel(listener.X, source.SourceX);
            }
            Play(this.sound, sound, volume, channel, true);
        }
        public void PlayMusic(SOUND sound, float volume, float channel)
        {
            StopMusic();

            if (sound == null)
                return;

            this.sound.AudioSource = null;

            Play(this.sound, sound, volume, channel, true);
        }
        public SOUND PlayVoice(object obj, string name)
        {
            return PlayVoice(obj, name, 1, 0);
        }
        public SOUND PlayVoice(object obj, string name, IAudioSource source)
        {
            SOUND load = Content.Load<SOUND>(name);
            PlayVoice(obj, load, source);
            return load;
        }
        public SOUND PlayVoice(object obj, string name, float volume, float channel)
        {
            SOUND load = Content.Load<SOUND>(name);
            PlayVoice(obj, load, volume, channel);
            return load;
        }
        public void PlayVoice(object obj, SOUND sound, IAudioSource source)
        {
            InternalPlayVoice(obj, sound, source, 1, 0);
        }
        public void PlayVoice(object obj, SOUND sound, float volume, float channel)
        {
            InternalPlayVoice(obj, sound, null, volume, channel);
        }
        private void InternalPlayVoice(object obj, SOUND sound, IAudioSource source, float volume, float channel)
        {
            StopVoice(obj);

            if (sound == null)
                return;

            Sound newSound;
            if (source == null)
                newSound = GetFreeSound();
            else
                newSound = GetSourcedSound();
            newSound.AudioSource = source;
            sources[obj] = newSound;

            if (source != null)
            {
                VECTOR2 listener = ListenerLocation;
                volume = GetVolume(ref listener, source);
                channel = GetChannel(listener.X, source.SourceX);
            }

            Play(newSound, sound, volume, channel, false);
        }
        public void ClearVoiceStack()
        {
            sources.Clear();
        }
        public SOUND PlaySound(string name)
        {
            return PlaySound(name, 1, 0);
        }
        public SOUND PlaySound(string name, IAudioSource source)
        {
            SOUND load = Content.Load<SOUND>(name);
            PlaySound(load, source);
            return load;
        }
        public SOUND PlaySound(string name, float volume, float channel)
        {
            SOUND load = Content.Load<SOUND>(name);
            PlaySound(load, volume, channel);
            return load;
        }
        public void PlaySound(SOUND sound, IAudioSource source)
        {
            if (sound == null)
                return;

            Sound newSound = GetSourcedSound();
            newSound.AudioSource = source;

            float volume = 1, channel = 0;
            if (source != null)
            {
                VECTOR2 listener = ListenerLocation;
                volume = GetVolume(ref listener, source);
                channel = GetChannel(listener.X, source.SourceX);
            }
            Play(newSound, sound, volume, channel, false);
        }
        public void PlaySound(SOUND sound, float volume, float channel)
        {
            if (sound == null)
                return;
            Play(GetFreeSound(), sound, volume, channel, false);
        }
        private Sound GetSourcedSound()
        {
            Sound newSound = sounds.Allot();
            if (newSound == null)
            {
                newSound = new Sound();
                sounds.Add(newSound);
            }
            newSound.AudioSource = null;
            return newSound;
        }
        private Sound GetFreeSound()
        {
            Sound newSound = freeSounds.Allot();
            if (newSound == null)
            {
                newSound = new Sound();
                freeSounds.Add(newSound);
            }
            newSound.AudioSource = null;
            return newSound;
        }
        public virtual void Dispose()
        {
            Stop(sound);
            if (content != null && isInternalContent)
                content.Dispose();
            //foreach (Sound item in sources.Values)
            //    item.SoundEffect.Dispose();
            //foreach (Sound item in sounds)
            //    item.SoundEffect.Dispose();
            //foreach (var item in freeSounds)
            //    item.SoundEffect.Dispose();
            sources.Clear();
            sounds.Clear();
            freeSounds.Clear();
        }
    }
    public class AudioEmpty : AUDIO
    {
        protected override void Play(ref SoundSource source, SOUND wave)
        {
        }
        protected override void Stop(SoundSource source)
        {
        }
    }


    #endregion


    #region Graphics


    /*
	 * Texture种类
	 * 1. 普通Texture
	 * 2. Piece: 自带SourceRectangle的组合大图
	 * 3. Patch: 九宫格图
	 * -4. Tile: 平铺图
	 * -5. Map: 超过2048的超大图
	 * 6. Animation: 序列帧动画
	 */
    [ADevice]
    public abstract class TEXTURE : Content, ICoroutine
    {
        /// <summary>只是更换了后缀的png文件，由组合大图工具生成</summary>
        public const string SPECIAL_TEXTURE_TYPE = "t2d";
        public static string[] TextureFileType =
		{
			"png",
			"jpg",
			"jpeg",
			"bmp",
            SPECIAL_TEXTURE_TYPE,
		};
        private const string KEY_PIXEL = "*PIXEL";

        internal static TEXTURE _pixel;
        public static TEXTURE Pixel
        {
            get
            {
                if (_pixel == null)
                {
                    _pixel = Entry.Instance.NewTEXTURE(1, 1);
                    _pixel.SetData(new COLOR[] { COLOR.Default });
                    _pixel = new TEXTURE_SYSTEM(_pixel);
                    _pixel._Key = KEY_PIXEL;
                }
                return _pixel;
            }
        }

        public abstract int Width { get; }
        public abstract int Height { get; }
        public VECTOR2 Size
        {
            get { return new VECTOR2(Width, Height); }
        }
        public VECTOR2 Center
        {
            get { return new VECTOR2(Width * 0.5f, Height * 0.5f); }
        }
        public virtual bool IsEnd
        {
            get { return false; }
        }
        internal virtual bool IsLinked { get { return false; } }

        protected TEXTURE()
        {
        }
        [ADeviceNew]
        public TEXTURE(int width, int height)
        {
        }

        public COLOR[] GetData()
        {
            return GetData(new RECT(0, 0, Width, Height));
        }
        public virtual COLOR[] GetData(RECT area)
        {
            throw new NotImplementedException();
        }
        public void SetData(COLOR[] buffer)
        {
            SetData(buffer, new RECT(0, 0, Width, Height));
        }
        public virtual void SetData(COLOR[] buffer, RECT area)
        {
            throw new NotImplementedException();
        }
        public virtual void Save(string file)
        {
            throw new NotImplementedException();
        }
        public virtual void Update(GameTime time)
        {
        }
        protected internal virtual bool Draw(GRAPHICS graphics, ref SpriteVertex vertex) { return false; }

        public static TEXTURE GetDrawableTexture(TEXTURE texture)
        {
            //if (texture == null)
            //    return null;
            //if (texture.IsLinked)
            //    return GetDrawableTexture(((TEXTURE_Link)texture).Base);
            //else
            //    return texture;

            while (true)
            {
                if (texture == null) return null;
                if (texture.IsLinked)
                    texture = ((TEXTURE_Link)texture).Base;
                else
                    return texture;
            }
        }
        public static new bool Serializer(ByteRefWriter writer, object value, Type type)
        {
            bool nil = value == null;
            TEXTURE texture = null;
            if (!nil)
            {
                texture = value as TEXTURE;
                if (texture == null)
                    return false;
                if (value is ParticleEmitter || value is ParticleSystem)
                    return false;
            }
            else if (!type.Is(typeof(TEXTURE)))
                return false;

            if (texture == null || string.IsNullOrEmpty(texture.Key))
                writer.Write((string)null);
            else
            {
                //if (string.IsNullOrEmpty(texture.Key))
                //    throw new ArgumentNullException("Texture key is that be serialized can't be null.");
                if (texture.Key.StartsWith(PATCH.KEY_PATCH))
                {
                    PATCH patch = (PATCH)texture;
                    writer.Write(string.Format("{0}{1},{2},{3},{4},{5},{6}", PATCH.KEY_PATCH,
                        patch.Anchor.X, patch.Anchor.Y, patch.Anchor.Width, patch.Anchor.Height,
                        patch.ColorBody.ToRGBAComma(),
                        patch.ColorBorder.ToRGBAComma()));
                }
                else
                    writer.Write(texture.Key);
            }

            return true;
        }
        public static Func<Type, VariableObject, Func<ByteRefReader, object>> Deserializer(ContentManager content, List<AsyncLoadContent> list)
        {
            return (type, field) =>
            {
                if (type.Is(typeof(ParticleEmitter)) || type.Is(typeof(ParticleSystem)))
                    return null;
                if (type.Is(typeof(TEXTURE)))
                    return (reader) =>
                    {
                        string key;
                        reader.Read(out key);
                        if (key == null)
                            return null;
                        else if (key == KEY_PIXEL)
                            return Pixel;
                        else if (key.StartsWith(PATCH.KEY_PATCH))
                        {
                            key = key.Substring(PATCH.KEY_PATCH.Length);
                            string[] data = key.Split(',');
                            RECT area;
                            area.X = float.Parse(data[0]);
                            area.Y = float.Parse(data[1]);
                            area.Width = float.Parse(data[2]);
                            area.Height = float.Parse(data[3]);
                            COLOR body;
                            body.R = byte.Parse(data[4]);
                            body.G = byte.Parse(data[5]);
                            body.B = byte.Parse(data[6]);
                            body.A = byte.Parse(data[7]);
                            COLOR border;
                            border.R = byte.Parse(data[8]);
                            border.G = byte.Parse(data[9]);
                            border.B = byte.Parse(data[10]);
                            border.A = byte.Parse(data[11]);
                            PATCH patch = PATCH.GetNinePatch(body, border, 1);
                            patch.Anchor = area;
                            return patch;
                        }
                        else
                        {
                            if (list == null)
                            {
                                // 同步加载
                                return content.Load<TEXTURE>(key);
                            }
                            else
                            {
                                // 异步加载
                                TEXTURE_DELAY delay = new TEXTURE_DELAY();
                                var async = content.LoadAsync<TEXTURE>(key, (t) => delay.Base = t);
                                delay.Async = async;
                                list.Add(async);
                                return delay;
                            }
                        }
                    };
                else
                    return null;
            };
        }
    }
    public abstract class TEXTURE_Link : TEXTURE
    {
        public virtual EntryEngine.TEXTURE Base { get; set; }
        public override int Width
        {
            get { return Base.Width; }
        }
        public override int Height
        {
            get { return Base.Height; }
        }
        public override bool IsEnd
        {
            get { return Base.IsEnd; }
        }
        public override bool IsDisposed
        {
            get { return Base.IsDisposed; }
        }
        internal sealed override bool IsLinked
        {
            get { return true; }
        }

        public TEXTURE_Link() { }
        public TEXTURE_Link(EntryEngine.TEXTURE Base) { this.Base = Base; }

        public override EntryEngine.COLOR[] GetData(EntryEngine.RECT area)
        {
            return Base.GetData(area);
        }
        public override void SetData(EntryEngine.COLOR[] buffer, EntryEngine.RECT area)
        {
            Base.SetData(buffer, area);
        }
        public override void Save(string file)
        {
            Base.Save(file);
        }
        protected internal override void InternalDispose()
        {
            Base.Dispose();
        }
        protected internal override bool Draw(GRAPHICS graphics, ref SpriteVertex vertex)
        {
            if (Base == null)
                return true;
            graphics.Draw(Base, ref vertex);
            return true;
        }
        public override void Update(GameTime time)
        {
            if (Base != null)
                Base.Update(time);
        }
    }
    /// <summary>系统图片素材不会被释放掉</summary>
    internal sealed class TEXTURE_SYSTEM : TEXTURE_Link
    {
        public TEXTURE_SYSTEM() { }
        public TEXTURE_SYSTEM(TEXTURE texture) : base(texture) { }

        protected internal override void InternalDispose()
        {
        }
    }
    /// <summary>可延迟加载的图片</summary>
    public sealed class TEXTURE_DELAY : TEXTURE_Link
    {
        public Async Async;

        protected internal override void InternalDispose()
        {
            base.InternalDispose();
            if (Async != null && !Async.IsEnd)
                Async.Cancel();
            Async = null;
        }
    }

    public sealed class PIECE : TEXTURE_Link
    {
        /// <summary>宽高就是图像的尺寸</summary>
        public RECT SourceRectangle;
        /// <summary>宽高分别是右边和下边的尺寸</summary>
        public RECT Padding;

        public override int Width
        {
            get { return (int)(SourceRectangle.Width + Padding.X + Padding.Width); }
        }
        public override int Height
        {
            get { return (int)(SourceRectangle.Height + Padding.Y + Padding.Height); }
        }

        public PIECE()
        {
        }
        public PIECE(TEXTURE texture, RECT source)
        {
            if (texture == null)
                throw new ArgumentNullException("texture");
            this.Base = texture;
            this.SourceRectangle = source;
        }
        public PIECE(TEXTURE texture, RECT source, RECT padding) : this(texture, source)
        {
            this.Padding = padding;
        }

        protected internal override bool Draw(GRAPHICS graphics, ref SpriteVertex vertex)
        {
            if (Base == null)
                return true;

            float width = Width;
            float height = Height;
            if (vertex.Source.Width > width || vertex.Source.Height > height)
                throw new InvalidOperationException("Piece texture can't tile. SourceRectangle must be contained in piece's SourceRectangle.");

            if (!Padding.IsEmpty)
            {
                float width2 = vertex.Destination.Width;
                float height2 = vertex.Destination.Height;
                float scaleX = width2 / vertex.Source.Width;
                float scaleY = height2 / vertex.Source.Height;
                float whiteX1 = 0, whiteX2;
                float whiteY1 = 0, whiteY2;
                bool oxFlag = false, oyFlag = false;
                whiteX1 = Padding.X - vertex.Source.X;
                if (whiteX1 >= 0)
                {
                    vertex.Source.Width -= whiteX1;
                    whiteX1 *= scaleX;
                    vertex.Destination.Width -= whiteX1;
                    vertex.Source.X = 0;
                    oxFlag = true;
                }
                else
                {
                    vertex.Source.X -= Padding.X;
                    whiteX1 = 0;
                }
                whiteX2 = vertex.Source.X + vertex.Source.Width;
                if (whiteX2 > SourceRectangle.Width)
                {
                    whiteX2 -= SourceRectangle.Width;
                    vertex.Source.Width -= whiteX2;
                    whiteX2 *= scaleX;
                    vertex.Destination.Width -= whiteX2;
                    oxFlag = true;
                }
                if (oxFlag)
                {
                    vertex.Origin.X = (vertex.Origin.X * width2 - whiteX1) / vertex.Destination.Width;
                }

                whiteY1 = Padding.Y - vertex.Source.Y;
                if (whiteY1 >= 0)
                {
                    vertex.Source.Height -= whiteY1;
                    whiteY1 *= scaleY;
                    vertex.Destination.Height -= whiteY1;
                    vertex.Source.Y = 0;
                    oyFlag = true;
                }
                else
                {
                    vertex.Source.Y -= Padding.Y;
                    whiteY1 = 0;
                }
                whiteY2 = vertex.Source.Y + vertex.Source.Height;
                if (whiteY2 > SourceRectangle.Height)
                {
                    whiteY2 -= SourceRectangle.Height;
                    vertex.Source.Height -= whiteY2;
                    whiteY2 *= scaleY;
                    vertex.Destination.Height -= whiteY2;
                    oyFlag = true;
                }
                if (oyFlag)
                {
                    vertex.Origin.Y = (vertex.Origin.Y * height2 - whiteY1) / vertex.Destination.Height;
                }
            }
            vertex.Source.X += SourceRectangle.X;
            vertex.Source.Y += SourceRectangle.Y;
            
            graphics.Draw(Base, ref vertex);
            return true;
        }
        protected internal override Content Cache()
        {
            // Base.Cache()?
            var cache = new PIECE();
            cache.Base = this.Base;
            cache.SourceRectangle = this.SourceRectangle;
            cache.Padding = this.Padding;
            cache._Key = this._Key;
            return cache;
        }
        protected internal override void InternalDispose()
        {
            // dispose with all of the piece disposed

            // 由于Queue的加载，ContentManager[Key]里会Dispose掉PIECE
        }
    }
    public sealed class PipelinePiece : ContentPipeline
    {
        [AReflexible]public class Piece
        {
            public string File;
            public string Map;
            public RECT Source;
            public RECT Padding;
        }

        static PipelinePiece()
        {
            // HACK: 防止构造函数被优化掉
            new Piece();
        }

        private Dictionary<string, Piece> Pieces;

        public override IEnumerable<string> SuffixProcessable
        {
            get { return TEXTURE.TextureFileType.Enumerable(); }
        }

        public override void LoadMetadata(string content)
        {
            CSVReader reader = new CSVReader(content);
            var read = reader.ReadObject<Piece[]>();

            if (Pieces == null || Pieces.Count == 0)
                Pieces = read.ToDictionary(p => p.File);
            else
            {
                Piece temp;
                foreach (var piece in read)
                {
                    if (Pieces.TryGetValue(piece.File, out temp))
                    {
                        if (piece.Map != temp.Map || piece.Source != temp.Source)
                            throw new ArgumentException("Same file can't be has different map or source.");
                    }
                    else
                    {
                        Pieces.Add(piece.File, piece);
                    }
                }
            }
        }
        public override byte[] Process(string file)
        {
            throw new NotImplementedException("Metadata processed by EntryBuilder.TexFreeTiledMap");
        }
        private Piece FindPiece(string file)
        {
            Piece piece;
            if (string.IsNullOrEmpty(IO.RootDirectory))
                Pieces.TryGetValue(file, out piece);
            else
                Pieces.TryGetValue(IO.BuildPath(file), out piece);
            return piece;
        }
        private string FindMap(Piece piece)
        {
            string map = piece.Map;
            if (string.IsNullOrEmpty(IO.RootDirectory))
                return map;
            else
            {
                //if (map.StartsWith(ContentManager.FilePathUnify(IO.RootDirectory)))
                //{
                //}
                int len = IO.RootDirectory.Length;
                char end = IO.RootDirectory[len - 1];
                if (end == '/' || end == '\\')
                    return map.Substring(len);
                else
                    return map.Substring(len + 1);
            }
        }
        public override bool Processable(ref string file)
        {
            if (Pieces == null || Pieces.Count == 0)
                return false;
            return FindPiece(file) != null;
        }
        protected internal override Content Load(string file)
        {
            Piece result = FindPiece(file);

            TEXTURE texture = Manager.Load<TEXTURE>(FindMap(result));

            PIECE piece = new PIECE();
            piece.Base = texture;
            piece.SourceRectangle = result.Source;
            piece.Padding = result.Padding;
            return piece;
        }
        protected internal override void LoadAsync(AsyncLoadContent async)
        {
            Piece result = FindPiece(async.File);

            PIECE piece = new PIECE();
            piece.SourceRectangle = result.Source;
            piece.Padding = result.Padding;

            AsyncLoadContent wait = Manager.LoadAsync<TEXTURE>(FindMap(result),
                (texture) =>
                {
                    piece.Base = texture;
                });

            Wait(async, wait, t => piece);
        }

        public PipelinePiece()
        {
            if (defaultPipeline != null)
            {
                this.Pieces = defaultPipeline.Pieces;
            }
        }
        public PipelinePiece(PipelinePiece clone)
        {
            if (clone.Pieces == null || clone.Pieces.Count == 0)
                throw new ArgumentNullException("Metadata");
            this.Pieces = clone.Pieces;
        }

        private static PipelinePiece defaultPipeline;
        public static PipelinePiece GetDefaultPipeline()
        {
            if (defaultPipeline == null)
            {
                defaultPipeline = new PipelinePiece();
                defaultPipeline.Pieces = new Dictionary<string, Piece>();
            }
            return defaultPipeline;
        }
    }

    public sealed class PATCH : TEXTURE_Link
    {
        internal const string KEY_PATCH = "*PATCH";
        internal const string COLOR_NULL = "255,255,255,0";
        struct PatchPiece
        {
            public float X;
            public float Y;
            public float W;
            public float H;
            public float SX;
            public float SY;
            public float SW;
            public float SH;
        }
        private static PatchPiece[][] _grid = new PatchPiece[3][];
        public static COLOR NullColor = new COLOR(255, 255, 255, 0);

        static PATCH()
        {
            for (int i = 0; i < _grid.Length; i++)
                _grid[i] = new PatchPiece[3];
        }

        internal static TEXTURE _patch;
        public static TEXTURE _PATCH
        {
            get
            {
                if (_patch == null)
                {
                    _patch = Entry.Instance.NewTEXTURE(8, 8);

                    COLOR[] colors = new COLOR[8 * 8];
                    for (int i = 0; i < colors.Length; i++)
                        colors[i] = COLOR.Default;

                    _patch.SetData(colors);

                    _patch = new TEXTURE_SYSTEM(_patch);
                }
                return _patch;
            }
        }

        public static PATCH GetNinePatch(COLOR body, COLOR border, byte bold)
        {
            if (bold > 3)
                throw new ArgumentException("Bold can't be over 3.");
            PATCH patch = new PATCH();
            patch.Base = _PATCH;
            patch.ColorBody = body;
            patch.ColorBorder = border;
            patch.Anchor = new RECT(bold, bold, _patch.Width - bold * 2, _patch.Height - bold * 2);
            patch._Key = KEY_PATCH;
            return patch;
        }

        /// <summary>左上 Body宽高[不是右下]</summary>
        public RECT Anchor;
        public COLOR ColorBody;
        public COLOR ColorBorder;

        public float Left
        {
            get { return Anchor.X; }
            set { Anchor.X = value; }
        }
        public float Top
        {
            get { return Anchor.Y; }
            set { Anchor.Y = value; }
        }
        public float Right
        {
            get { return Anchor.Right; }
            set { Anchor.Width = value - Anchor.X; }
        }
        public float Bottom
        {
            get { return Anchor.Bottom; }
            set { Anchor.Height = value - Anchor.Y; }
        }
        public float RightWidth { get { return Base.Width - Anchor.X - Anchor.Width; } }
        public float BottomHeight { get { return Base.Height - Anchor.Y - Anchor.Height; } }
        public bool IsDefaultPatch
        {
            get { return Key != null && Key.StartsWith(KEY_PATCH); }
        }

        public PATCH()
        {
        }
        public PATCH(TEXTURE texture, RECT patch)
        {
            if (texture == null)
                throw new ArgumentNullException("texture");
            this.Base = texture;
            this.Anchor = patch;
        }
        public PATCH(TEXTURE texture, RECT patch, COLOR body, COLOR border)
            : this(texture, patch)
        {
            this.ColorBody = body;
            this.ColorBorder = border;
        }

        protected internal override bool Draw(GRAPHICS graphics, ref SpriteVertex vertex)
        {
            if (Base == null)
                return true;

            if ((int)vertex.Source.Width != Base.Width ||
                (int)vertex.Source.Height != Base.Height)
                throw new ArgumentException("Patch's source must be all of the base texture.");

            float bwidth = Base.Width;
            float bheight = Base.Height;
            float width = vertex.Destination.Width;
            float height = vertex.Destination.Height;
            float right_sx = Anchor.X + Anchor.Width;
            float bottom_sy = Anchor.Y + Anchor.Height;
            float center_width = width - (bwidth - Anchor.Width);
            float middle_height = height - (bheight - Anchor.Height);
            float right_width = bwidth - Anchor.X - Anchor.Width;
            float bottom_height = bheight - Anchor.Y - Anchor.Height;
            float right_x = width - right_width;
            float bottom_y = height - bottom_height;
            vertex.Origin.X *= width;
            vertex.Origin.Y *= height;
            VECTOR2 origin = vertex.Origin;

            // 上左
            //Draw(graphics, vertex, 0, 0, Anchor.X, Anchor.Y, Anchor.X, Anchor.Y);
            _grid[0][0].X = 0;
            _grid[0][0].Y = 0;
            _grid[0][0].SX = 0;
            _grid[0][0].SY = 0;
            _grid[0][0].W = Anchor.X;
            _grid[0][0].H = Anchor.Y;
            _grid[0][0].SW = Anchor.X;
            _grid[0][0].SH = Anchor.Y;
            // 上中
            //Draw(graphics, vertex, Anchor.X, 0, center_width, Anchor.Y, Anchor.Width, Anchor.Y);
            _grid[0][1].X = Anchor.X;
            _grid[0][1].Y = 0;
            _grid[0][1].SX = Anchor.X;
            _grid[0][1].SY = 0;
            _grid[0][1].W = center_width;
            _grid[0][1].H = Anchor.Y;
            _grid[0][1].SW = Anchor.Width;
            _grid[0][1].SH = Anchor.Y;
            // 上右
            //Draw(graphics, vertex, right_x, 0, right_width, Anchor.Y, right_width, Anchor.Y);
            _grid[0][2].X = right_x;
            _grid[0][2].Y = 0;
            _grid[0][2].SX = right_sx;
            _grid[0][2].SY = 0;
            _grid[0][2].W = right_width;
            _grid[0][2].H = Anchor.Y;
            _grid[0][2].SW = right_width;
            _grid[0][2].SH = Anchor.Y;
            // 中左
            //Draw(graphics, vertex, 0, Anchor.Y, Anchor.X, middle_height, Anchor.X, Anchor.Height);
            _grid[1][0].X = 0;
            _grid[1][0].Y = Anchor.Y;
            _grid[1][0].SX = 0;
            _grid[1][0].SY = Anchor.Y;
            _grid[1][0].W = Anchor.X;
            _grid[1][0].H = middle_height;
            _grid[1][0].SW = Anchor.X;
            _grid[1][0].SH = Anchor.Height;
            // 中中
            //Draw(graphics, vertex, Anchor.X, Anchor.Y, center_width, middle_height, Anchor.Width, Anchor.Height);
            _grid[1][1].X = Anchor.X;
            _grid[1][1].Y = Anchor.Y;
            _grid[1][1].SX = Anchor.X;
            _grid[1][1].SY = Anchor.Y;
            _grid[1][1].W = center_width;
            _grid[1][1].H = middle_height;
            _grid[1][1].SW = Anchor.Width;
            _grid[1][1].SH = Anchor.Height;
            // 中右
            //Draw(graphics, vertex, right_x, Anchor.Y, right_width, middle_height, right_width, Anchor.Height);
            _grid[1][2].X = right_x;
            _grid[1][2].Y = Anchor.Y;
            _grid[1][2].SX = right_sx;
            _grid[1][2].SY = Anchor.Y;
            _grid[1][2].W = right_width;
            _grid[1][2].H = middle_height;
            _grid[1][2].SW = right_width;
            _grid[1][2].SH = Anchor.Height;
            // 下左
            //Draw(graphics, vertex, 0, bottom_y, Anchor.X, bottom_height, Anchor.X, bottom_height);
            _grid[2][0].X = 0;
            _grid[2][0].Y = bottom_y;
            _grid[2][0].SX = 0;
            _grid[2][0].SY = bottom_sy;
            _grid[2][0].W = Anchor.X;
            _grid[2][0].H = bottom_height;
            _grid[2][0].SW = Anchor.X;
            _grid[2][0].SH = bottom_height;
            // 下中
            //Draw(graphics, vertex, Anchor.X, bottom_y, center_width, bottom_height, Anchor.Width, bottom_height);
            _grid[2][1].X = Anchor.X;
            _grid[2][1].Y = bottom_y;
            _grid[2][1].SX = Anchor.X;
            _grid[2][1].SY = bottom_sy;
            _grid[2][1].W = center_width;
            _grid[2][1].H = bottom_height;
            _grid[2][1].SW = Anchor.Width;
            _grid[2][1].SH = bottom_height;
            // 下右
            //Draw(graphics, vertex, right_x, bottom_y, right_width, bottom_height, right_width, bottom_height);
            _grid[2][2].X = right_x;
            _grid[2][2].Y = bottom_y;
            _grid[2][2].SX = right_sx;
            _grid[2][2].SY = bottom_sy;
            _grid[2][2].W = right_width;
            _grid[2][2].H = bottom_height;
            _grid[2][2].SW = right_width;
            _grid[2][2].SH = bottom_height;

            // 镜像
            if ((vertex.Flip & EFlip.FlipHorizontally) != EFlip.None)
            {
                for (int i = 0; i < 3; i++)
                {
                    _grid[i][2].X = 0;
                    _grid[i][1].X = _grid[i][2].W;
                    _grid[i][0].X = _grid[i][1].X + _grid[i][1].W;
                }
            }
            if ((vertex.Flip & EFlip.FlipVertically) != EFlip.None)
            {
                for (int i = 0; i < 3; i++)
                {
                    _grid[2][i].Y = 0;
                    _grid[1][i].Y = _grid[2][i].H;
                    _grid[0][i].Y = _grid[1][i].Y + _grid[1][i].H;
                }
            }
            
            // 先画Body
            if (!(ColorBody.A == 0 && ColorBody.R == 255 && ColorBody.G == 255 && ColorBody.B == 255))
            {
                COLOR color = vertex.Color;
                vertex.Color.R = ColorBody.R;
                vertex.Color.G = ColorBody.G;
                vertex.Color.B = ColorBody.B;
                vertex.Color.A = ColorBody.A;
                Draw(graphics, ref vertex, ref _grid[1][1]);
                // 还原颜色，防止边框比改变颜色时，边框也和Body颜色一样
                vertex.Color = color;
            }
            else
                Draw(graphics, ref vertex, ref _grid[1][1]);

            // 统一画border
            if (!(ColorBorder.A == 0 && ColorBorder.R == 255 && ColorBorder.G == 255 && ColorBorder.B == 255))
            {
                vertex.Color.R = ColorBorder.R;
                vertex.Color.G = ColorBorder.G;
                vertex.Color.B = ColorBorder.B;
                vertex.Color.A = ColorBorder.A;
            }
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (i == 1 && j == 1) continue;
                    // Origin会被修改
                    vertex.Origin.X = origin.X;
                    vertex.Origin.Y = origin.Y;
                    Draw(graphics, ref vertex, ref _grid[i][j]);
                }
            }
            return true;
        }
        void Draw(GRAPHICS graphics, ref SpriteVertex vertex, ref PatchPiece param)
        {
            vertex.Destination.Width = param.W;
            vertex.Destination.Height = param.H;
            vertex.Source.X = param.SX;
            vertex.Source.Y = param.SY;
            vertex.Source.Width = param.SW;
            vertex.Source.Height = param.SH;
            // vertex.Origin.X * width在外面已经乘过了，这里直接用
            vertex.Origin.X = __GRAPHICS.CalcOrigin(param.X, param.W, vertex.Origin.X);
            vertex.Origin.Y = __GRAPHICS.CalcOrigin(param.Y, param.H, vertex.Origin.Y);
            graphics.Draw(Base, ref vertex);
        }
        protected internal override Content Cache()
        {
            var cache = new PATCH();
            cache._Key = this._Key;
            cache.Base = this.Base;
            cache.Anchor = this.Anchor;
            cache.ColorBody = this.ColorBody;
            cache.ColorBorder = this.ColorBorder;
            return cache;
        }
    }
    public sealed class PipelinePatch : ContentPipeline
    {
        [AReflexible]public class Patch
        {
            public RECT Anchor;
            public COLOR ColorBody;
            public COLOR ColorBorder;
            public string Source;
        }

        static PipelinePatch()
        {
            // HACK: 防止构造函数被优化掉
            new PATCH();
        }

        public override IEnumerable<string> SuffixProcessable
        {
            get { yield break; }
        }
        /// <summary>
        /// mtpatch: Metadata of Texture Patch
        /// </summary>
        public override string FileType
        {
            get { return "mtpatch"; }
        }

        protected internal override Content Load(string file)
        {
            string metadata = IO.ReadText(file);
            Patch patch = new XmlReader(metadata).ReadObject<Patch>();

            TEXTURE texture;
            if (string.IsNullOrEmpty(patch.Source))
                texture = PATCH._PATCH;
            else
                texture = Manager.Load<TEXTURE>(patch.Source);

            PATCH tp = new PATCH();
            tp.Base = texture;
            tp.Anchor = patch.Anchor;
            tp.ColorBody = patch.ColorBody;
            tp.ColorBorder = patch.ColorBorder;
            return tp;
        }
        protected internal override void LoadAsync(AsyncLoadContent async)
        {
            Wait(async, IO.ReadAsync(async.File),
                wait =>
                {
                    string metadata = IO.ReadPreambleText(wait.Data);
                    Patch patch = new XmlReader(metadata).ReadObject<Patch>();

                    PATCH tp = new PATCH();
                    tp.Anchor = patch.Anchor;
                    tp.ColorBody = patch.ColorBody;
                    tp.ColorBorder = patch.ColorBorder;

                    if (string.IsNullOrEmpty(patch.Source))
                    {
                        tp.Base = PATCH._PATCH;
                        async.SetData(tp);
                    }
                    else
                    {
                        Wait(async,
                            Manager.LoadAsync<TEXTURE>(patch.Source, t => tp.Base = t),
                            result => tp);
                    }
                });
        }
    }

    public sealed class ANIMATION : TEXTURE_Link
    {
        private List<Sequence> sequences;
        private Dictionary<string, TEXTURE> textures;
        private int current;
        private float elapsedTime;
        private int currentFrame;
        private int loop;
        private int updated;

        public float FrameElapsedTime
        {
            get { return elapsedTime; }
        }
        public Sequence Sequence
        {
            get { return sequences[current]; }
        }
        public float FullSequenceTime
        {
            get { return GetSequenceTime(Sequence, new HashSet<string>()); }
        }
        public float Progress
        {
            get
            {
                Sequence sequence = Sequence;
                int loop = this.loop;
                if (sequence.Loop < 0)
                    loop = 0;
                float time = sequence.Time;
                float current = elapsedTime;
                for (int i = 0; i < currentFrame; i++)
                    current += sequence[i].Interval;
                current += time * loop;
                return _MATH.InOne(current * 1.0f / (time * (sequence.Loop < 0 ? 1 : sequence.Loop + 1)));
            }
            set
            {
                Sequence sequence = Sequence;
                float time = sequence.TotalTime;
                value = _MATH.InOne(value);
                float target = time * value;
                float current = 0;
                loop = 0;
                for (int j = _MATH.Max(0, sequence.Loop); j >= 0; j--)
                {
                    for (int i = 0; i < sequence.FrameCount; i++)
                    {
                        current += sequence[i].Interval;
                        if (current >= target)
                        {
                            currentFrame = i;
                            elapsedTime = sequence[i].Interval - (current - target);
                            return;
                        }
                    }
                    loop++;
                }
                currentFrame = sequence.FrameCount - 1;
                elapsedTime = Frame.Interval;
            }
        }
        public Frame Frame
        {
            get { return Sequence.Frames[currentFrame]; }
        }
        public bool IsSequenceOver
        {
            get
            {
                Sequence sequence = Sequence;
                if (sequence.Loop < 0)
                    return false;
                return currentFrame == sequence.FrameCount - 1 && loop >= sequence.Loop && IsFrameOver;
            }
        }
        public bool IsFrameOver
        {
            get { return elapsedTime >= Frame.Interval; }
        }
        public TEXTURE Texture
        {
            get { return textures[Frame.Texture]; }
        }
        public Sequence this[string name]
        {
            get { return sequences.FirstOrDefault(s => s.Name == name); }
        }
        public override TEXTURE Base
        {
            get
            {
                if (base.Base == null)
                    base.Base = Texture;
                return base.Base;
            }
            set
            {
                base.Base = value;
            }
        }
        public override bool IsEnd
        {
            get { return IsSequenceOver; }
        }
        public override bool IsDisposed
        {
            get
            {
                if (textures == null || textures.Count == 0)
                    return true;

                foreach (TEXTURE texture in textures.Values)
                    if (texture.IsDisposed)
                        return true;

                return false;
            }
        }

        public ANIMATION()
        {
            sequences = new List<Sequence>();
        }
        public ANIMATION(ANIMATION clone)
            : this(clone.sequences, clone.textures)
        {
        }
        internal ANIMATION(IEnumerable<Sequence> sequences, Dictionary<string, TEXTURE> textures)
        {
            if (sequences == null)
                throw new ArgumentNullException("sequences");
            if (textures == null)
                throw new ArgumentNullException("textures");
            this.sequences = new List<Sequence>(sequences);
            this.textures = textures;
        }

        private float GetSequenceTime(Sequence sequence, HashSet<string> list)
        {
            // loop
            if (!list.Add(sequence.Name))
                return -1;

            if (sequence.Loop < 0)
                return -1;
            else
            {
                float time = 0;
                if (!string.IsNullOrEmpty(sequence.Next))
                {
                    // loop
                    //if (!list.Add(sequence.Next))
                    //    return -1;

                    var next = this[sequence.Next];
                    if (next != null)
                    {
                        // loop
                        if (next.Next == sequence.Name)
                        {
                            return -1;
                        }
                        else
                        {
                            // recursion
                            float temp = GetSequenceTime(next, list);
                            if (temp == -1)
                                return -1;
                            else
                                time = temp;
                        }
                    }
                }
                time += sequence.TotalTime;
                return time;
            }
        }
        public void AddSequence(Sequence sequence)
        {
            if (this[sequence.Name] != null)
                throw new InvalidOperationException("Sequence name has been existed.");
            sequences.Add(sequence);
        }
        public void AddSequence(string name, short loop, string next, int interval, params string[] textures)
        {
            Sequence sequence = new Sequence();
            sequence.Name = name;
            sequence.Next = next;
            sequence.Loop = loop;
            int count = textures.Length;
            sequence.Frames = new Frame[count];
            for (int i = 0; i < count; i++)
            {
                Frame frame = new Frame();
                frame.Texture = textures[i];
                frame.Interval = interval;
                sequence.Frames[i] = frame;
            }
            AddSequence(sequence);
        }
        public bool RemoveSequence(string name)
        {
            return this.sequences.Remove(this[name]);
        }
        public void Load(ContentManager content)
        {
            if (!IsDisposed)
                Dispose();

            textures = new Dictionary<string, TEXTURE>();
            foreach (var frame in sequences.SelectMany(s => s.Frames))
                if (!textures.ContainsKey(frame.Texture))
                    textures.Add(frame.Texture, content.Load<TEXTURE>(frame.Texture));
        }
        public void Reset()
        {
            current = 0;
            ResetSequence();
        }
        public void ResetSequence()
        {
            loop = 0;
            currentFrame = 0;
            elapsedTime = 0;
            Base = Texture;
        }
        public void NextSequence()
        {
            if (++current >= sequences.Count)
                current = 0;
            ResetSequence();
        }
        /// <summary>
        /// 下一帧
        /// </summary>
        /// <returns>序列动画是否播放完毕</returns>
        public bool NextFrame()
        {
            bool over = false;
            var sequence = Sequence;
            if (++currentFrame >= sequence.FrameCount)
            {
                if (sequence.Loop >= 0 && ++loop > sequence.Loop)
                {
                    if (!Play(sequence.Next))
                    {
                        // keep the last frame
                        currentFrame = sequence.FrameCount - 1;
                        loop = sequence.Loop;
                        over = true;
                    }
                }
                else
                {
                    currentFrame = 0;
                }
            }
            Base = Texture;
            return over;
        }
        /// <returns>成功更换动画</returns>
        public bool Play(string name)
        {
            Sequence sequence = Sequence;
            if (sequence.Name != name)
            {
                int index = sequences.IndexOf(s => s.Name == name);
                if (index == -1)
                {
                    return false;
                }
                else
                {
                    this.current = index;
                    ResetSequence();
                    return true;
                }
            }
            return false;
        }
        public void Replay(string name)
        {
            if (Sequence.Name == name)
                ResetSequence();
            else
                Play(name);
        }
        /// <returns>动画播放完毕</returns>
        public bool Update(float elapsed)
        {
            bool over = false;
            var frame = Frame;
            while (elapsedTime >= frame.Interval)
            {
                over = NextFrame();
                //this.elapsedTime = 0;
                this.elapsedTime = elapsedTime - frame.Interval;
                frame = Frame;
            }
            this.elapsedTime += elapsed;

            return over;
        }
        public override void Update(GameTime time)
        {
            updated = GameTime.Time.FrameID;
            Update(time.ElapsedSecond);
        }
        protected internal override bool Draw(GRAPHICS graphics, ref SpriteVertex vertex)
        {
            if (updated != GameTime.Time.FrameID)
            {
                updated = GameTime.Time.FrameID;
                Update(GameTime.Time);
            }
            var frame = Frame;
            if (frame != null)
            {
                vertex.Origin.X += frame.PivotX;
                vertex.Origin.Y += frame.PivotY;
                base.Draw(graphics, ref vertex);
            }
            return true;
        }
        protected internal override void InternalDispose()
        {
            if (textures != null)
            {
                //foreach (TEXTURE item in textures.Values)
                //    item.Dispose();
                //textures.Clear();
                textures = null;
            }
        }
        protected internal override Content Cache()
        {
            var cache = new ANIMATION(sequences, textures);
            cache._Key = this._Key;
            return cache;
        }
    }
    [AReflexible]public class Sequence
    {
        public string Name;
        public short Loop;
        public string Next;
        public Frame[] Frames;

        public int FrameCount
        {
            get
            {
                if (Frames == null)
                    return 0;
                return Frames.Length;
            }
        }
        public float Time
        {
            get
            {
                int count = FrameCount;
                float time = 0;
                for (int i = 0; i < count; i++)
                    time += Frames[i].Interval;
                return time;
            }
        }
        public float TotalTime
        {
            get
            {
                return Time * (Loop < 0 ? 1 : Loop + 1);
            }
        }
        public Frame this[int index]
        {
            get { return Frames[index]; }
        }

        public void SetInterval(float interval)
        {
            if (Frames != null)
            {
                foreach (var item in Frames)
                {
                    item.Interval = interval;
                }
            }
        }
    }
    [AReflexible]public class Frame
    {
        public string Texture;
        /// <summary>秒</summary>
        public float Interval;
        public float PivotX;
        public float PivotY;
    }
    public class PipelineAnimation : ContentPipeline
    {
        static PipelineAnimation()
        {
            // HACK: 防止构造函数被优化掉
            new Sequence();
            new Frame();
        }

        public override IEnumerable<string> SuffixProcessable
        {
            get { yield break; }
        }
        /// <summary>mtseq: Metadata of Sequence</summary>
        public override string FileType
        {
            get { return "mtseq"; }
        }

        protected internal override Content Load(string file)
        {
            string metadata = IO.ReadText(file);
            Sequence[] sequences = new XmlReader(metadata).ReadObject<Sequence[]>();

            Dictionary<string, TEXTURE> textures = new Dictionary<string, TEXTURE>();
            foreach (var sequence in sequences)
            {
                foreach (var frame in sequence.Frames)
                {
                    if (!textures.ContainsKey(frame.Texture))
                    {
                        textures.Add(frame.Texture, Manager.Load<TEXTURE>(frame.Texture));
                    }
                }
            }

            return new ANIMATION(sequences, textures);
        }
        protected internal override void LoadAsync(AsyncLoadContent async)
        {
            Wait(async, IO.ReadAsync(async.File),
                wait =>
                {
                    string metadata = IO.ReadPreambleText(wait.Data);
                    Sequence[] sequences = new XmlReader(metadata).ReadObject<Sequence[]>();

                    Dictionary<string, TEXTURE> textures = new Dictionary<string, TEXTURE>();

                    Wait(async,
                        sequences.SelectMany(s => s.Frames).Distinct(f => f.Texture).
                            Select(f => Manager.LoadAsync<TEXTURE>(f.Texture, t => textures[f.Texture] = t)),
                        () => new ANIMATION(sequences, textures));
                });
        }
    }


    [ADevice("FONT.Default")]
    public abstract class FONT : Content
    {
        public enum ECharType
        {
            Special,
            Alphabet,
            Symbol,
            WhiteSpace,
            Unicode,
        }
        internal const string KEY_DEFAULT = "*DEFAULT";

        private static FONT _default;
        public static FONT Default
        {
            get
            {
                if (_default == null)
                    return null;
                FONT font = (FONT)_default.Cache();
                font._Key = KEY_DEFAULT;
                return font;
            }
            set
            {
                _default = value;
            }
        }

        /// <summary>
        /// 字体尺寸（单位：像素）
        /// </summary>
        public abstract float FontSize { get; set; }
        public abstract float LineHeight { get; }
        public bool IsDefault { get { return _Key == KEY_DEFAULT; } }
        public virtual bool IsDynamic { get { return false; } }

        protected FONT()
        {
        }
        [ADeviceNew]
        public FONT(string name, float fontSize)
        {
        }

        public virtual VECTOR2 MeasureString(string text)
        {
            return MeasureString(CharWidth, LineHeight, text);
        }
        public string BreakLine(string text, float width, out string[] lines)
        {
            return BreakLine(CharWidth, LineHeight, text, width, out lines);
        }
        public string BreakLine(string text, float width)
        {
            string[] lines;
            return BreakLine(CharWidth, LineHeight, text, width, out lines);
        }
        public VECTOR2 Cursor(string text, int index)
        {
            return Cursor(CharWidth, LineHeight, text, index);
        }
        public int CursorIndex(string text, VECTOR2 mouse)
        {
            return CursorIndex(CharWidth, LineHeight, text, mouse);
        }
        protected internal abstract float CharWidth(char c);
        protected internal abstract void Draw(GRAPHICS spriteBatch, string text, VECTOR2 location, COLOR color, float scale);

        public static Func<Type, VariableObject, Func<ByteRefReader, object>> Deserializer(ContentManager content, List<AsyncLoadContent> list)
        {
            return (type, field) =>
            {
                if (type.Is(typeof(FONT)))
                    return (reader) =>
                    {
                        string key;
                        reader.Read(out key);
                        if (key == null)
                            return null;
                        else if (key == KEY_DEFAULT)
                            return Default;
                        else
                        {
                            if (list == null)
                            {
                                // 同步加载
                                return content.Load<FONT>(key);
                            }
                            else
                            {
                                // 异步加载
                                FONT_DELAY delay = new FONT_DELAY();
                                delay.Async = content.LoadAsync<FONT>(key, (t) => delay.Base = t);
                                list.Add(delay.Async);
                                return delay;
                            }
                        }
                    };
                else
                    return null;
            };
        }

        public const char LINE_BREAK_2 = '\r';
        public const char LINE_BREAK = '\n';
        public const string SYMBOL_STRING = ")!@#$%^&*(`~-_=+\\|[{]};:\'\",<.>/?";

        public static bool IsHalfWidthChar(char c)
        {
            return c < 127;
        }
        /// <summary>
        /// 测量等宽字体字符串的尺寸
        /// </summary>
        /// <param name="calcWidth">计算字符宽度</param>
        /// <param name="height">行高</param>
        /// <param name="text">要测量的文字</param>
        /// <returns>文字的宽高</returns>
        public static VECTOR2 MeasureString(Func<char, float> calcWidth, float height, string text)
        {
            VECTOR2 result;

            if (string.IsNullOrEmpty(text))
            {
                // result.X = 1;
                result.X = 0;
                result.Y = height;
                return result;
            }

            int count = text.Length;
            float width = 0;
            // 最大宽度
            float max = 0;
            int line = 1;
            for (int i = 0; i < count; i++)
            {
                char c = text[i];
                if (c == LINE_BREAK)
                {
                    line++;
                    if (width > max)
                    {
                        max = width;
                    }
                    width = 0;
                }
                else
                {
                    width += calcWidth(c);
                }
                if (width > max)
                {
                    max = width;
                }
            }

            result.X = max;
            // 高度 = 换行符个数 * 尺寸
            result.Y = line * height;
            return result;
        }
        /// <summary>
        /// 自动换行
        /// </summary>
        /// <param name="calcWidth">计算字符宽度</param>
        /// <param name="height">行高</param>
        /// <param name="text">字符串</param>
        /// <param name="width">行宽</param>
        /// <param name="lines">拆分后的每行字符串</param>
        /// <returns>换行后的字符串</returns>
        public static string BreakLine(Func<char, float> calcWidth, float height, string text, float width, out string[] lines)
        {
            if (string.IsNullOrEmpty(text))
            {
                lines = new string[] { text };
                return text;
            }

            lines = text.Split(LINE_BREAK);
            if (text.Length == 1)
                return text;

            List<string> list = new List<string>(lines.Length * 2);

            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    list.Add(line);
                    continue;
                }

                int count = line.Length;
                float _width = 0;
                float alphabetWidth = 0;
                int? alphabet = null;
                int start = 0;
                for (int i = 0; i < count; i++)
                {
                    char c = line[i];

                    bool isSpace = char.IsWhiteSpace(c);
                    bool isHalf = IsHalfWidthChar(c);
                    bool isBreak = isSpace || !isHalf;
                    float charWidth = calcWidth(c);
                    _width += charWidth;

                    if (_width > width)
                    {
                        if (
                            // 如果是英文或字符，单词整体不能打断，空格除外
                            isBreak ||
                            // 超过一行的字符连续时还是要换行
                            alphabet == start)
                        {
                            list.Add(line.Substring(start, i - start));
                            _width = charWidth;
                            alphabetWidth = 0;
                            start = i;
                            alphabet = i;
                        }
                        else
                        {
                            // 连续英文换行
                            int value = alphabet.HasValue ? alphabet.Value : i;
                            list.Add(line.Substring(start, value - start));
                            _width = alphabetWidth + charWidth;
                            start = value;
                        }
                    }
                    if (isBreak)
                    {
                        alphabet = null;
                        alphabetWidth = 0;
                    }
                    else
                    {
                        alphabetWidth += charWidth;
                        if (alphabet == null)
                            alphabet = i;
                    }
                }
                list.Add(line.Substring(start));
            }

            lines = list.ToArray();
            return string.Join(LINE_BREAK.ToString(), lines);
        }
        /// <summary>
        /// 光标在一段文字中的坐标
        /// </summary>
        /// <param name="calcWidth">计算字符宽度</param>
        /// <param name="height">行高</param>
        /// <param name="text">字符串</param>
        /// <param name="index">索引</param>
        /// <returns>坐标</returns>
        public static VECTOR2 Cursor(Func<char, float> calcWidth, float height, string text, int index)
        {
            VECTOR2 result = new VECTOR2();

            if (string.IsNullOrEmpty(text) || index <= 0)
                return result;

            int count = text.Length;
            if (index > count)
                index = count;

            string[] lines = text.Split(LINE_BREAK);

            for (int i = 0; i < lines.Length; i++)
            {
                text = lines[i];
                if (index > text.Length)
                {
                    result.Y += height;
                    index -= text.Length + 1;
                }
                else
                {
                    float width = 0;
                    for (int j = 0; j < index; j++)
                    {
                        // calcWidth(text[j])结果没错，可是width+=的结果有误
                        // 错误特点是不管怎么累加width的结构都是calcWidth的两倍，相当于width=calcWidth;width+=width;，以下是calcWidth-width的结果
                        // 错误 width += calcWidth(text[j]): 7-14
                        // 正确 temp = calcWidth(text[j]); width += temp: 7-7
                        // MeasureString同样有类似用法，不过是正确的
                        float temp = calcWidth(text[j]);
                        width += temp;
                    }
                    result.X = width;
                    break;
                }

                if (index <= 0)
                    break;
            }

            return result;
        }
        public static VECTOR2 CursorAtLast(Func<char, float> calcWidth, float height, string text)
        {
            return Cursor(calcWidth, height, text, text.Length);
        }
        /// <summary>
        /// 获得鼠标在文字内容中的索引
        /// </summary>
        /// <param name="calcWidth">计算字符宽度</param>
        /// <param name="height">行高</param>
        /// <param name="text">内容</param>
        /// <param name="mouse">鼠标坐标</param>
        /// <returns>光标在文字内容中的索引</returns>
        public static int CursorIndex(Func<char, float> calcWidth, float height, string text, VECTOR2 mouse)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            string[] lines = text.Split(LINE_BREAK);
            int row = -1;
            int col = -1;
            // 行
            if (lines.Length == 1)
            {
                row = 0;
            }
            else
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    if (mouse.Y < (i + 1) * height)
                    {
                        row = i;
                        break;
                    }
                }
                if (row == -1)
                {
                    row = lines.Length - 1;
                }
            }
            // 列
            string chars = lines[row];
            float width = 0;
            float add;
            for (int i = 0; i < chars.Length; i++)
            {
                add = calcWidth(chars[i]);
                width += add;
                if (mouse.X < width - add * 0.5f)
                {
                    col = i;
                    break;
                }
            }
            if (col == -1)
            {
                col = chars.Length;
            }
            // 计算索引
            int index = 0;
            for (int i = 0; i < row; i++)
            {
                index += lines[i].Length;
                // \n
                col++;
            }
            return index + col;
        }
        /// <summary>
        /// 查找索引处字符相似的字符的连续字符串
        /// </summary>
        /// <param name="text">要查找的字符串</param>
        /// <param name="index">相似字符的索引位置</param>
        /// <returns>相似字符的索引范围</returns>
        public static Range<int> SimilarChar(string text, int index)
        {
            Range<int> range = new Range<int>();
            if (string.IsNullOrEmpty(text))
                return range;

            int len = text.Length;
            index = _MATH.Clamp(index, 0, len - 1);

            ECharType type = GetCharType(text[index]);
            if (type == ECharType.WhiteSpace && index != 0)
            {
                index--;
                type = GetCharType(text[index]);
            }

            int cursor = index - 1;
            while (true)
            {
                if (cursor == -1 || GetCharType(text[cursor]) != type)
                {
                    range.Min = cursor + 1;
                    break;
                }
                cursor--;
            }

            cursor = _MATH.Min(index + 1, len);
            while (true)
            {
                if (cursor == len || GetCharType(text[cursor]) != type)
                {
                    range.Max = cursor;
                    break;
                }
                cursor++;
            }

            return range;
        }
        public static ECharType GetCharType(char c)
        {
            if ((c >= 65 && c <= 91) || (c >= 97 && c <= 123))
                return ECharType.Alphabet;

            if (SYMBOL_STRING.Contains(c))
                return ECharType.Symbol;

            if (char.IsWhiteSpace(c))
                return ECharType.WhiteSpace;

            if (c <= 255)
                return ECharType.Special;

            return ECharType.Unicode;
        }
        /// <summary>
        /// 将自动换行的字符串中的索引映射到没换行前的字符串内
        /// </summary>
        /// <param name="current">自动换行后的文字</param>
        /// <param name="index">换行后文字内的索引</param>
        /// <param name="previous">没换行前的文字</param>
        /// <returns>没换行前文字内的索引</returns>
        public static int ChangeIndex(string current, int index, string previous)
        {
            int offset = 0;
            for (int i = 0; i < index; i++)
            {
                if (current[i] == LINE_BREAK && previous[i - offset] != LINE_BREAK)
                {
                    offset++;
                }
            }
            return index - offset;
        }
        public static int ChangeIndexToBreakLine(string current, int index, string breakline)
        {
            int offset = 0;
            for (int i = 0; i < index; i++)
            {
                if (breakline[i + offset] == LINE_BREAK && current[i] != LINE_BREAK)
                {
                    offset++;
                }
            }
            return index + offset;
        }
        public static Range<int> IndexLineRange(string text, int index)
        {
            Range<int> range = new Range<int>();
            int len = text.Length;
            int cursor = index - 1;
            while (true)
            {
                if (cursor == -1 || text[cursor] == LINE_BREAK)
                {
                    range.Min = cursor + 1;
                    break;
                }
                cursor--;
            }
            cursor = index;
            while (true)
            {
                if (cursor == len || text[cursor] == LINE_BREAK)
                {
                    range.Max = cursor;
                    break;
                }
                cursor++;
            }
            return range;
        }

        public static string GetLastLine(string text)
        {
            return text.Substring(text.LastIndexOf(LINE_BREAK) + 1);
        }
        /// <summary>
        /// 文字中索引所在的行的文字
        /// </summary>
        /// <param name="text">文字</param>
        /// <param name="index">索引</param>
        /// <returns>索引所在行的文字</returns>
        public static string IndexLineText(string text, int index)
        {
            string[] lines = text.Split(LINE_BREAK);
            int total = 0;
            foreach (string line in lines)
            {
                total += line.Length + 1;
                if (index <= total)
                {
                    return text;
                }
            }
            return string.Empty;
        }
        public static int IndexForCol(string text, int index)
        {
            int row, col;
            IndexForRowCol(text, index, 0, text.Length, out row, out col);
            return row;
        }
        /// <summary>
        /// 文字中索引所在的行与列索引
        /// </summary>
        /// <param name="text">文字</param>
        /// <param name="index">索引</param>
        /// <param name="start">文字开始索引</param>
        /// <param name="count">文字数量</param>
        /// <param name="row">行索引</param>
        /// <param name="col">列索引</param>
        public static void IndexForRowCol(string text, int index, int start, int count, out int row, out int col)
        {
            row = 0;
            col = 0;
            int end = start + count;
            for (int i = start; i < count; i++)
            {
                if (text[i] == LINE_BREAK)
                {
                    col = i;
                    row++;
                }
            }
            col = index - col;
        }
    }
    public abstract class FONT_Link : EntryEngine.FONT
    {
        public virtual EntryEngine.FONT Base { get; set; }
        public override float FontSize
        {
            get { return Base.FontSize; }
            set { Base.FontSize = value; }
        }
        public override float LineHeight
        {
            get { return Base.LineHeight; }
        }
        public override bool IsDisposed
        {
            get { return Base.IsDisposed; }
        }

        public FONT_Link() { }
        public FONT_Link(EntryEngine.FONT Base) { this.Base = Base; }

        public override EntryEngine.VECTOR2 MeasureString(string text)
        {
            return Base.MeasureString(text);
        }
        protected internal override float CharWidth(char c)
        {
            return Base.CharWidth(c);
        }
        protected internal override void Draw(EntryEngine.GRAPHICS spriteBatch, string text, EntryEngine.VECTOR2 location, EntryEngine.COLOR color, float scale)
        {
            Base.Draw(spriteBatch, text, location, color, scale);
        }
        protected internal override void InternalDispose()
        {
            Base.InternalDispose();
        }
        protected internal override EntryEngine.Content Cache()
        {
            return Base.Cache();
        }
    }
    internal sealed class FONT_DELAY : FONT_Link
    {
        public AsyncLoadContent Async;

        protected internal override void InternalDispose()
        {
            base.InternalDispose();
            if (Async != null && !Async.IsEnd)
                Async.Cancel();
            Async = null;
        }
    }
    public class TextShader
    {
        public VECTOR2 Offset;
        public COLOR Color;
        public TextShader() { this.Color.A = 128; }
        public TextShader(float offsetX, float offsetY, COLOR color)
        {
            this.Offset.X = offsetX;
            this.Offset.Y = offsetY;
            this.Color = color;
        }
        public TextShader(TextShader copy)
        {
            if (copy == null)
                throw new ArgumentNullException("copy");
            this.Offset = copy.Offset;
            this.Color = copy.Color;
        }
        public bool IsStroke { get { return Offset.X == 0 && Offset.Y == 0; } }
        public bool IsShader { get { return Offset.X != 0 || Offset.Y != 0; } }
        public static TextShader CreateStroke(COLOR color)
        {
            return new TextShader(0, 0, color);
        }
        public static TextShader CreateShader()
        {
            return new TextShader(2, 2, new COLOR(128, 128, 128, 128));
        }
        public static TextShader CreateShader(COLOR color)
        {
            return new TextShader(2, 2, color);
        }
        internal static float _Stroke = 2;
        internal static float _Stroke2 = _Stroke * 2;
        public static float Stroke
        {
            get { return _Stroke; }
            set
            {
                if (value < 1 || value > 4)
                    throw new ArgumentException("Stroke must be 1~4.");
                _Stroke = value;
                _Stroke2 = value * 2;
            }
        }
    }
    public abstract class FontTexture : FONT
    {
        public const ushort BUFFER_SIZE = 1024;

        public class Buffer
        {
            public byte Index;
            internal ushort x;
            public ushort X { get { return x; } }
            internal ushort y;
            public ushort Y { get { return y; } }
            public byte W;
            public byte H;
            /// <summary>字的间隔，默认等于W</summary>
            public byte Space;
        }
        protected class CacheInfo
        {
            public TEXTURE[] Textures;
            public Dictionary<char, Buffer> Maps;
        }

        protected float fontSize;
        protected float lineHeight;
        protected CacheInfo cache;
        public VECTOR2 Spacing;
        public TextShader Effect;

        public override float FontSize
        {
            get { return fontSize; }
            set { fontSize = value; }
        }
        public override float LineHeight
        {
            get { return lineHeight; }
        }
        public override bool IsDisposed
        {
            get { return cache == null || cache.Maps == null || cache.Maps.Count == 0 || cache.Textures.IsEmpty(); }
        }

        public IEnumerable<TEXTURE> GetTextures()
        {
            foreach (var item in cache.Textures)
                if (item != null)
                    yield return item;
        }
        protected internal override float CharWidth(char c)
        {
            if (c == '\r') return 0;
            Buffer buffer;
            if (cache.Maps.TryGetValue(c, out buffer))
            {
                return buffer.W + Spacing.X;
            }
            else
            {
                //_LOG.Warning("字体不包含文字:{0}", c);
                return 0;
            }
        }
        public bool GetTextTexture(char c, out TEXTURE texture, out RECT source)
        {
            var buffer = GetBuffer(c);
            if (buffer == null)
            {
                texture = null;
                source.X = source.Y = source.Width = source.Height = 0;
                return false;
            }
            else
            {
                texture = cache.Textures[buffer.Index];
                source.X = buffer.x;
                source.Y = buffer.y;
                source.Width = buffer.W;
                source.Height = buffer.H;
                return true;
            }
        }
        protected virtual Buffer GetBuffer(char c)
        {
            Buffer buffer;
            cache.Maps.TryGetValue(c, out buffer);
            return buffer;
        }
        protected internal override void Draw(GRAPHICS spriteBatch, string text, VECTOR2 location, COLOR color, float scale)
        {
            RECT area;
            area.X = location.X;
            area.Y = location.Y;
            float y = location.Y;
            float height = lineHeight;
            //RECT uv = RECT.Empty;
            int count = text.Length;
            for (int i = 0; i < count; i++)
            {
                char c = text[i];
                if (c == LINE_BREAK)
                {
                    area.X = location.X;
                    area.Y += height + Spacing.Y * scale;
                    y = area.Y;
                }
                else
                {
                    var buffer = GetBuffer(c);
                    if (buffer == null)
                        continue;

                    area.Y = y;
                    //area.Y = y + (height - uv.Height) / 2;
                    //area.Y = y + height - uv.Height;
                    area.Width = buffer.W * scale;
                    area.Height = buffer.H * scale;
                    if (Effect != null)
                    {
                        if (Effect.IsStroke)
                        {
                            // 假描边，字的尺寸不大的话效果还可以
                            spriteBatch.BaseDraw(cache.Textures[buffer.Index], area.X - TextShader.Stroke, area.Y - TextShader.Stroke, area.Width + TextShader._Stroke2, area.Height + TextShader._Stroke2, false, buffer.x, buffer.y, buffer.W, buffer.H, true, Effect.Color.R, Effect.Color.G, Effect.Color.B, Effect.Color.A, 0, 0, 0, EFlip.None);
                        }
                        else
                        {
                            // 阴影
                            spriteBatch.BaseDraw(cache.Textures[buffer.Index], area.X + Effect.Offset.X, area.Y + Effect.Offset.Y, area.Width, area.Height, false, buffer.x, buffer.y, buffer.W, buffer.H, true, Effect.Color.R, Effect.Color.G, Effect.Color.B, Effect.Color.A, 0, 0, 0, EFlip.None);
                        }
                    }
                    spriteBatch.BaseDraw(cache.Textures[buffer.Index], area.X, area.Y, area.Width, area.Height, false, buffer.x, buffer.y, buffer.W, buffer.H, true, color.R, color.G, color.B, color.A, 0, 0, 0, EFlip.None);
                    //spriteBatch.Draw(texture, area, uv, color);
                    area.X += buffer.Space * scale + Spacing.X;
                }
            }
        }
        protected internal override void InternalDispose()
        {
            if (cache != null)
            {
                for (int i = 0; i < cache.Textures.Length; i++)
                    if (cache.Textures[i] != null)
                        cache.Textures[i].Dispose();
                cache.Maps.Clear();
            }
        }
        protected virtual void CopyTo(FontTexture target)
        {
            target._Key = this._Key;
            target.fontSize = this.fontSize;
            target.lineHeight = this.lineHeight;
            target.Spacing = this.Spacing;
            target.cache = this.cache;
            if (this.Effect != null)
                target.Effect = new TextShader(this.Effect);
            else
                target.Effect = null;
        }
    }
    public class FontStatic : FontTexture
    {
        private float scale = 1;

        protected float Scale
        {
            get { return scale; }
        }
        public override float FontSize
        {
            get { return fontSize * scale; }
            set
            {
                if (fontSize == 0)
                    this.fontSize = value;
                else
                    scale = _MATH.Near(value / fontSize, 1);
            }
        }
        public override float LineHeight
        {
            get
            {
                return base.LineHeight * scale;
            }
        }

        protected FontStatic()
        {
        }
        internal FontStatic(float fontSize, float lineHeight, Dictionary<char, Buffer> maps, TEXTURE[] textures)
        {
            if (fontSize <= 1)
                throw new ArgumentException("FontSize must greater than 1");
            if (textures == null)
                throw new ArgumentNullException("textures");
            if (maps == null)
                throw new ArgumentNullException("buffers");
            this.fontSize = fontSize;
            this.lineHeight = lineHeight;
            this.cache = new CacheInfo();
            this.cache.Textures = textures;
            this.cache.Maps = maps;
        }

        protected internal override float CharWidth(char c)
        {
            return base.CharWidth(c) * scale;
        }
        //protected internal override void Draw(GRAPHICS spriteBatch, string text, VECTOR2 location, COLOR color, float scale)
        //{
        //    //bool scaled = scale != 1;
        //    //if (scaled)
        //    //{
        //    //    spriteBatch.BeginFromPrevious(MATRIX2x3.CreateScale(scale, scale) * MATRIX2x3.CreateTranslation(location.X, location.Y));
        //    //    location.X = 0;
        //    //    location.Y = 0;
        //    //}

        //    base.Draw(spriteBatch, text, location, color, scale * this.scale);

        //    //if (scaled)
        //    //    spriteBatch.End();
        //}
        protected internal override void Draw(GRAPHICS spriteBatch, string text, VECTOR2 location, COLOR color, float scale)
        {
            scale *= this.scale;
            base.Draw(spriteBatch, text, location, color, scale);
        }
        protected override void CopyTo(FontTexture target)
        {
            base.CopyTo(target);
            FontStatic font = (FontStatic)target;
            font.scale = this.scale;
        }
        protected internal override Content Cache()
        {
            FontStatic clone = new FontStatic();
            CopyTo(clone);
            return clone;
        }
    }
    public class PipelineFontStatic : ContentPipeline
    {
        public override IEnumerable<string> SuffixProcessable
        {
            get { return null; }
        }
        /// <summary>
        /// mtsfont: Metadata of static font
        /// </summary>
        public override string FileType
        {
            get { return "mtsfont"; }
        }

        protected internal override Content Load(string file)
        {
            ByteReader reader = new ByteReader(IO.ReadByte(file));
            string name;
            reader.Read(out name);

            float size;
            float height;
            reader.Read(out size);
            reader.Read(out height);

            Dictionary<char, FontStatic.Buffer> maps = new Dictionary<char, FontStatic.Buffer>();
            int count;
            reader.Read(out count);
            int index = 0;
            char c;
            for (int i = 0; i < count; i++)
            {
                reader.Read(out c);
                FontStatic.Buffer buffer = new FontStatic.Buffer();
                reader.Read(out buffer.Index);
                reader.Read(out buffer.x);
                reader.Read(out buffer.y);
                reader.Read(out buffer.W);
                reader.Read(out buffer.H);
                maps.Add(c, buffer);
                if (buffer.Index != index)
                    index++;
            }

            string directory = Path.GetDirectoryName(file);
            if (!string.IsNullOrEmpty(directory))
                directory += "\\";
            TEXTURE[] textures = new TEXTURE[index + 1];
            for (int i = 0; i <= index; i++)
                textures[i] = Manager.Load<TEXTURE>(string.Format("{0}{1}_{2}_{3}.png", directory, name, size, index));

            return new FontStatic(size, height, maps, textures);
        }
        protected internal override void LoadAsync(AsyncLoadContent async)
        {
            Wait(async, IO.ReadAsync(async.File),
                wait =>
                {
                    ByteReader reader = new ByteReader(wait.Data);
                    string name;
                    reader.Read(out name);

                    float size;
                    float height;
                    reader.Read(out size);
                    reader.Read(out height);

                    Dictionary<char, FontStatic.Buffer> maps = new Dictionary<char, FontStatic.Buffer>();
                    int count;
                    reader.Read(out count);
                    int index = 0;
                    char c;
                    for (int i = 0; i < count; i++)
                    {
                        reader.Read(out c);
                        FontStatic.Buffer buffer = new FontStatic.Buffer();
                        reader.Read(out buffer.Index);
                        reader.Read(out buffer.x);
                        reader.Read(out buffer.y);
                        reader.Read(out buffer.W);
                        reader.Read(out buffer.H);
                        maps.Add(c, buffer);
                        if (buffer.Index != index)
                            index++;
                    }

                    string directory = Path.GetDirectoryName(async.File);
                    if (!string.IsNullOrEmpty(directory))
                        directory += "\\";
                    TEXTURE[] textures = new TEXTURE[index + 1];
                    Wait(async,
                        Enumerable.Range(0, textures.Length).
                            Select(i =>
                                Manager.LoadAsync<TEXTURE>(
                                    string.Format("{0}{1}_{2}_{3}.png", directory, name, size, index),
                                    result => textures[i] = result)),
                        () => new FontStatic(size, height, maps, textures));
                });
        }
        //private string[] Read(string file, byte[] bytes, out float size, out float height, out Dictionary<char, FontStatic.Buffer> maps)
        //{
        //    ByteReader reader = new ByteReader(bytes);
        //    string name;
        //    reader.Read(out name);

        //    reader.Read(out size);
        //    reader.Read(out height);

        //    maps = new Dictionary<char, FontStatic.Buffer>();
        //    int count;
        //    reader.Read(out count);
        //    int index = 0;
        //    char c;
        //    for (int i = 0; i < count; i++)
        //    {
        //        reader.Read(out c);
        //        FontStatic.Buffer buffer = new FontStatic.Buffer();
        //        reader.Read(out buffer.Index);
        //        reader.Read(out buffer.X);
        //        reader.Read(out buffer.Y);
        //        reader.Read(out buffer.W);
        //        reader.Read(out buffer.H);
        //        maps.Add(c, buffer);
        //        if (buffer.Index != index)
        //            index++;
        //    }

        //    string directory = Path.GetDirectoryName(file);
        //    if (!string.IsNullOrEmpty(directory))
        //        directory += "\\";
        //    string[] textures = new string[index + 1];
        //    for (int i = 0; i <= index; i++)
        //        textures[i] = string.Format("{0}{1}_{2}_{3}.png", directory, name, size, index);

        //    return textures;
        //}
    }
    public class AsyncDrawDynamicChar : AsyncData<COLOR[]>
    {
        internal TEXTURE texture;
        internal FontTexture.Buffer buffer;
        protected override void OnSetData(ref COLOR[] data)
        {
            texture.SetData(data, new RECT(buffer.x, buffer.y, buffer.W, buffer.H));
        }
    }
    public abstract class FontDynamic : FontStatic
    {
        /// <summary>字体大小相差不大时，采用静态字体缩放</summary>
        public static byte StaticStep = 0;
        private static Dictionary<int, FontDynamic> fonts = new Dictionary<int, FontDynamic>();
        private static AsyncDrawDynamicChar async1 = new AsyncDrawDynamicChar();

        protected class CacheInfo2 : FontTexture.CacheInfo
        {
            internal int id;
            internal byte index;
            internal ushort u;
            internal ushort v;
        }

        protected CacheInfo2 cacheP
        {
            get { return (CacheInfo2)cache; }
            private set { cache = value; }
        }
        public override float FontSize
        {
            get
            {
                return base.FontSize;
            }
            set
            {
                float size = base.FontSize;
                if (size != value)
                {
                    size = GetDynamicSize(value);
                    if (size != this.fontSize)
                    {
                        string key = this._Key;
                        var changed = OnSizeChanged(value);
                        if (changed != null && changed != this)
                            changed.CopyTo(this);
                        this._Key = key;
                    }
                    if (base.FontSize != value)
                        base.FontSize = value;
                }
            }
        }
        public sealed override bool IsDynamic { get { return true; } }

        protected FontDynamic(int id)
        {
            FontDynamic font;
            if (fonts.TryGetValue(id, out font))
            {
                font.CopyTo(this);
                return;
            }

            cacheP = BuildGraphicsInfo();
            cacheP.Textures = new TEXTURE[2];
            cacheP.Maps = new Dictionary<char, Buffer>();
            cacheP.id = id;
            fonts.Add(id, this);
        }

        protected virtual CacheInfo2 BuildGraphicsInfo()
        {
            return new CacheInfo2();
        }
        protected abstract FontDynamic OnSizeChanged(float fontSize);
        protected internal override float CharWidth(char c)
        {
            GetBuffer(c);
            return base.CharWidth(c);
        }
        public void RequestString(string text)
        {
            if (text == null) return;
            foreach (char c in text)
                GetBuffer(c);
        }
        public override VECTOR2 MeasureString(string text)
        {
            RequestString(text);
            return base.MeasureString(text);
        }
        protected sealed override Buffer GetBuffer(char c)
        {
            Buffer buffer = base.GetBuffer(c);
            if (buffer != null)
                return buffer;
            if (c == LINE_BREAK || c == LINE_BREAK_2)
                return null;

            var cache = this.cacheP;
            byte index = cache.index;
            ushort u = cache.u;
            ushort v = cache.v;

            bool newFlag = false;
            VECTOR2 size = MeasureBufferSize(c);
            byte width = (byte)_MATH.Ceiling(size.X);
            byte height = (byte)_MATH.Ceiling(size.Y);
            //if (width == 0 || height == 0)
            //    if (char.IsWhiteSpace(c))
            //        return null;
            //    else
            //        throw new ArgumentOutOfRangeException("FontSize too large over the dynamic buffer size");
            TEXTURE texture = cache.Textures[index];
            // new font texture
            if (texture != null)
            {
                if (width > texture.Width || height > texture.Height)
                    throw new ArgumentOutOfRangeException("FontSize too large over the dynamic buffer size");
                if (u + width > texture.Width)
                {
                    u = 0;
                    v += (ushort)_MATH.Ceiling(lineHeight);
                    if (v + lineHeight > texture.Height)
                        newFlag = true;
                }
            }
            else
                newFlag = true;

            if (newFlag)
            {
                u = 0;
                v = 0;
                texture = CreateTextureBuffer();
                if (cache.Textures[index] != null)
                    index++;
                if (index == cache.Textures.Length)
                    Array.Resize(ref cache.Textures, index + 2);
                cache.Textures[index] = texture;
            }

            buffer = new Buffer();
            buffer.Index = index;
            buffer.x = u;
            buffer.y = v;
            buffer.W = width;
            buffer.H = height;
            
            cache.Maps.Add(c, buffer);

            AsyncDrawDynamicChar async;
            if (async1.IsEnd)
            {
                async = async1;
                async1.Run();
            }
            else
                async = new AsyncDrawDynamicChar();
            async.texture = texture;
            async.buffer = buffer;
            DrawChar(async, c, buffer);
            u += buffer.W;
            //u++;
            if (buffer.Space == 0)
                buffer.Space = buffer.W;
            
            cache.index = index;
            cache.u = u;
            cache.v = v;

            return buffer;
        }
        protected virtual VECTOR2 MeasureBufferSize(char c)
        {
            return MeasureString(CalcBufferWidth, lineHeight, c.ToString());
        }
        protected float CalcBufferWidth(char c)
        {
            return IsHalfWidthChar(c) ? fontSize * 0.5f : fontSize;
        }
        protected virtual TEXTURE CreateTextureBuffer()
        {
            return Entry.Instance.NewTEXTURE(BUFFER_SIZE, BUFFER_SIZE);
        }
        //protected abstract AsyncData<COLOR[]> DrawChar(char c, Buffer uv);
        //protected abstract COLOR[] DrawChar(char c, Buffer uv);
        protected abstract void DrawChar(AsyncDrawDynamicChar result, char c, Buffer uv);
        public void Clear()
        {
            InternalDispose();
        }
        protected internal override void InternalDispose()
        {
            base.InternalDispose();
            Array.Resize(ref cacheP.Textures, 2);
            cacheP.index = 0;
            cacheP.u = 0;
            cacheP.v = 0;
        }

        public static float GetDynamicSize(float staticSize)
        {
            if (StaticStep == 0)
                return staticSize;
            return _MATH.Max(_MATH.Multiple(staticSize, StaticStep), StaticStep);
        }
        public static int GetCacheID(string name, float size)
        {
            return name.GetHashCode() + ((int)(GetDynamicSize(size) * 100.0f) / 100);
        }
        public static FontDynamic GetCache(int id)
        {
            FontDynamic cache;
            fonts.TryGetValue(id, out cache);
            return cache;
        }
        public static void ClearDynamicFonts()
        {
            for (int i = 0; i < fonts.Count; i++)
                fonts[i].Dispose();
            fonts.Clear();
        }
    }


    [Code(ECode.LessUseful | ECode.MayBeReform)]
    public interface SHADER
    {
        int PassCount { get; }
        void LoadFromCode(string code);
        bool SetPass(int pass);
        bool HasProperty(string name);
        bool GetValueBoolean(string property);
        int GetValueInt32(string property);
        MATRIX GetValueMatrix(string property);
        float GetValueSingle(string property);
        TEXTURE GetValueTexture(string property);
        VECTOR2 GetValueVector2(string property);
        VECTOR3 GetValueVector3(string property);
        VECTOR4 GetValueVector4(string property);
        void SetValue(string property, bool value);
        void SetValue(string property, float value);
        void SetValue(string property, int value);
        void SetValue(string property, MATRIX value);
        void SetValue(string property, TEXTURE value);
        void SetValue(string property, VECTOR2 value);
        void SetValue(string property, VECTOR3 value);
        void SetValue(string property, VECTOR4 value);
    }
    //public class SHADER : ShaderBase
    //{
    //    private List<ShaderVertex> vs;
    //    private List<ShaderPixel> ps;

    //    public List<ShaderVertex> VS
    //    {
    //        get { return vs; }
    //    }
    //    public List<ShaderPixel> PS
    //    {
    //        get { return ps; }
    //    }

    //    public SHADER()
    //    {
    //        vs = new List<ShaderVertex>();
    //        ps = new List<ShaderPixel>();
    //    }
    //    public SHADER(ShaderVertex[] vs, ShaderPixel[] ps)
    //    {
    //        this.vs = new List<ShaderVertex>(vs);
    //        this.ps = new List<ShaderPixel>(ps);
    //    }
    //    public SHADER(params ShaderVertex[] vs)
    //    {
    //        this.vs = new List<ShaderVertex>(vs);
    //        this.ps = new List<ShaderPixel>();
    //    }
    //    public SHADER(params ShaderPixel[] ps)
    //    {
    //        this.vs = new List<ShaderVertex>();
    //        this.ps = new List<ShaderPixel>(ps);
    //    }
    //}


    [Flags]public enum EFlip : byte
    {
        None = 0,
        FlipHorizontally = 1,
        FlipVertically = 2,
    }
    public enum EPrimitiveType : byte
    {
        Point,
        Line,
        Triangle,
    }
    public struct SpriteVertex
    {
        public RECT Source;
        public RECT Destination;
        public VECTOR2 Origin;
        public float Rotation;
        public EFlip Flip;
        public COLOR Color;
    }
    public struct TextureVertex
    {
        public VECTOR3 Position;
        public COLOR Color;
        public VECTOR2 TextureCoordinate;
    }
    /// <summary>屏幕适配</summary>
    public enum EViewport
    {
        /// <summary>画布尺寸保持与屏幕尺寸一样</summary>
        None,
        /// <summary>保持画布分辨率比例拉升自动适配屏幕</summary>
        Adapt,
        /// <summary>拉伸画布分辨率到屏幕分辨率</summary>
        Strength,
        /// <summary>画布尺寸始终保持不变，当屏幕尺寸大于画布尺寸时，画布居中</summary>
        Keep
    }
    public struct BoundingBox
    {
        public float Left;
        public float Top;
        public float Right;
        public float Bottom;

        public bool Intersects(ref BoundingBox other)
        {
            return other.Left < this.Right && this.Left < other.Right && other.Top < this.Bottom && this.Top < other.Bottom;
        }
    }
    internal class RenderBuffer
    {
        internal int TextureIndex;
        internal SpriteVertex[] spriteQueue = new SpriteVertex[128];
        internal BoundingBox[] spriteBoundingBox = new BoundingBox[128];
        internal int spriteQueueCount;
    }
    /// <summary>
    /// OriginX, OriginY是相对于SourceRectangle的百分比数字，例如0.5,0.5就是图像居中绘制
    /// </summary>
    [ADevice]
    public abstract class GRAPHICS
    {
        public const int MAX_BATCH_COUNT = 2048;
        public static readonly RECT NullSource = new RECT(float.NaN, 0, 0, 0);

        protected class RenderState
        {
            public MATRIX Transform;
            public bool ThreeD;
            public RECT Graphics;
            public SHADER Shader;
        }

        private EViewport viewportMode = EViewport.Adapt;
        private MATRIX2x3 view = MATRIX2x3.Identity;
        protected MATRIX2x3 graphicsToScreen;
        private MATRIX2x3 screenToGraphics;
        private RECT graphicsViewport = new RECT(0, 0, 1280, 720);
        private RenderState nullRenderState;
        private PoolStack<RenderState> renderStates = new PoolStack<RenderState>();
        public COLOR DefaultColor = COLOR.White;
        public readonly float[] XCornerOffsets = { 0, 1, 1, 0 };
        public readonly float[] YCornerOffsets = { 0, 0, 1, 1 };
        private TEXTURE currentTexture;
        private TEXTURE[] textures = new TEXTURE[8];
        private int textureCount;
        private RenderBuffer[] buffers = new RenderBuffer[16];
        private int bufferCount;
        private TextureVertex[] outputVertices = new TextureVertex[MAX_BATCH_COUNT * 4];
        private short[] indices;
        /// <summary>绘制前检测对象是否在视口内，不在视口内则跳过绘制，若绘制性能高则不建议开启此检测</summary>
        public bool Culling;

        public EViewport ViewportMode
        {
            get { return viewportMode; }
            set
            {
                if (viewportMode == value)
                    return;
                viewportMode = value;
                GraphicsAdaptScreen();
            }
        }
        public VECTOR2 ScreenSize
        {
            get { return InternalScreenSize; }
            set
            {
                InternalScreenSize = value;
                GraphicsAdaptScreen();
            }
        }
        public RECT FullScreenArea
        {
            get
            {
                return new RECT(VECTOR2.Zero, ScreenSize);
            }
        }
        public VECTOR2 GraphicsSize
        {
            get { return graphicsViewport.Size; }
            set
            {
                if (value.X == graphicsViewport.Width && value.Y == graphicsViewport.Height)
                    return;
                graphicsViewport.Width = value.X;
                graphicsViewport.Height = value.Y;
                GraphicsAdaptScreen();
            }
        }
        public RECT FullGraphicsArea
        {
            get { return new RECT(0, 0, graphicsViewport.Width, graphicsViewport.Height); }
        }
        public VECTOR2 OnePixel
        {
            get { return ToPixelCeiling(VECTOR2.One); }
        }
        public MATRIX2x3 View
        {
            get { return view; }
            protected set { view = value; }
        }
        public RECT Viewport
        {
            get { return graphicsViewport; }
        }

        public abstract bool IsFullScreen { get; set; }
        protected abstract VECTOR2 InternalScreenSize { get; set; }

        protected RenderState CurrentRenderState
        {
            get
            {
                if (renderStates.Count > 0)
                    return renderStates.Peek();
                else
                    return nullRenderState;
            }
        }
        public int RenderTargetCount
        {
            get { return renderStates.Count; }
        }
        public MATRIX2x3 CurrentTransform
        {
            get { return (MATRIX2x3)CurrentRenderState.Transform; }
        }
        public RECT CurrentGraphics
        {
            get { return CurrentRenderState.Graphics; }
        }
        protected IEnumerable<SHADER> Shaders
        {
            get
            {
                foreach (var state in renderStates)
                    if (state.Shader != null)
                        yield return state.Shader;
            }
        }
        protected bool HasRenderTarget
        {
            get { return renderStates.Count > 0; }
        }
        protected TEXTURE Texture { get { return currentTexture; } }

        protected GRAPHICS()
        {
            nullRenderState = new RenderState();
            nullRenderState.Transform = MATRIX.Identity;
            nullRenderState.Graphics = graphicsViewport;
            indices = CreateTriangleIndexData(MAX_BATCH_COUNT << 2);
        }

        internal void GraphicsAdaptScreen()
        {
            var screen = ScreenSize;
            var graphics = GraphicsSize;
            view = MATRIX2x3.Identity;
            switch (viewportMode)
            {
                case EViewport.None:
                    graphicsViewport.Width = screen.X;
                    graphicsViewport.Height = screen.Y;
                    break;

                case EViewport.Adapt:
                    float scale;
                    VECTOR2 offset;
                    __GRAPHICS.ViewAdapt(graphics, screen, out scale, out offset);

                    view.M11 = scale;
                    view.M22 = scale;
                    view.M31 = offset.X;
                    view.M32 = offset.Y;
                    break;

                case EViewport.Keep:
                    view.M31 = _MATH.Nature((screen.X - graphics.X) * 0.5f);
                    view.M32 = _MATH.Nature((screen.Y - graphics.Y) * 0.5f);
                    break;

                case EViewport.Strength:
                    VECTOR2 strength = VECTOR2.Divide(ref screen, ref graphics);
                    view.M11 = strength.X;
                    view.M22 = strength.Y;
                    break;
            }

            // set viewport
            //RECT screenViewport;
            //screenViewport.X = view.M31;
            //screenViewport.Y = view.M32;
            //screenViewport.Width = screen.X - view.M31 * 2;
            //screenViewport.Height = screen.Y - view.M32 * 2;

            //graphicsViewport.X = view.M31 / view.M11;
            //graphicsViewport.Y = view.M32 / view.M22;

            graphicsToScreen = view;
            MATRIX2x3.Invert(ref graphicsToScreen, out screenToGraphics);
            view.M31 = 0;
            view.M32 = 0;
            SetViewport(view, graphicsViewport);

            // viewport set over, change to graphics
            nullRenderState.Graphics = graphicsViewport;
            nullRenderState.Graphics.X = 0;
            nullRenderState.Graphics.Y = 0;
            //graphicsViewport.X = 0;
            //graphicsViewport.Y = 0;
        }
        protected abstract void SetViewport(MATRIX2x3 view, RECT viewport);
        public VECTOR2 PointToGraphics(VECTOR2 point)
        {
            VECTOR2.Transform(ref point, ref screenToGraphics);
            return point;
        }
        public VECTOR2 PointToScreen(VECTOR2 point)
        {
            VECTOR2.Transform(ref point, ref graphicsToScreen);
            return point;
        }
        public VECTOR2 PointToViewport(VECTOR2 point)
        {
            VECTOR2.Transform(ref point, ref view);
            return point;
        }
        public RECT AreaToGraphics(RECT rect)
        {
            RECT.Transform(ref rect, ref screenToGraphics);
            return rect;
        }
        public RECT AreaToScreen(RECT rect)
        {
            RECT.Transform(ref rect, ref graphicsToScreen);
            return rect;
        }
        public RECT AreaToViewport(RECT rect)
        {
            RECT.Transform(ref rect, ref view);
            return rect;
        }
        /// <summary>
        /// 计算需要的像素在分辨率下缩放的值
        /// 例如原分辨率下1px的对象，在缩放后可能不足1px而导致不显示
        /// </summary>
        /// <param name="pixel">原分辨率下的像素值</param>
        /// <returns>固定分辨率里显示的值</returns>
        public VECTOR2 ToPixel(VECTOR2 pixel)
        {
            pixel.X *= screenToGraphics.M11;
            pixel.Y *= screenToGraphics.M22;
            return pixel;
        }
        public VECTOR2 ToPixelCeiling(VECTOR2 pixel)
        {
            pixel = ToPixel(pixel);
            pixel.X = _MATH.Ceiling(pixel.X);
            pixel.X = _MATH.Ceiling(pixel.Y);
            return pixel;
        }
        private void Begin(ref MATRIX2x3 matrix, ref RECT graphics, SHADER shader)
        {
            MATRIX matrix4x4 = (MATRIX)matrix;
            Begin(false, ref matrix4x4, ref graphics, shader);
        }
        private void Begin(bool threeD, ref MATRIX matrix, ref RECT graphics, SHADER shader)
        {
            RenderState renderState;
            if (!renderStates.Allot(out renderState))
            {
                renderState = new RenderState();
                renderStates.Push(renderState);
            }
            renderState.ThreeD = threeD;
            renderState.Transform = matrix;
            renderState.Graphics = graphics;
            renderState.Shader = shader;
            Begin(renderState);
        }
        private void Begin(RenderState state)
        {
            Flush();
            MATRIX result = state.Transform;
            if (!state.ThreeD)
                result *= (MATRIX)view;
            RECT scissor = state.Graphics;
            //scissor.Offset(graphicsViewport.X, graphicsViewport.Y);
            //RECT.Intersect(ref scissor, ref graphicsViewport, out scissor);
            InternalBegin(state.ThreeD, ref result, ref scissor, state.Shader);
        }
        protected abstract void InternalBegin(bool threeD, ref MATRIX matrix, ref RECT graphics, SHADER shader);
        public void Begin(MATRIX transform, RECT graphics, SHADER shader)
        {
            Begin(true, ref transform, ref graphics, shader);
        }
        public void Begin()
        {
            RenderState rs = CurrentRenderState;
            Begin(rs.ThreeD, ref rs.Transform, ref rs.Graphics, null);
        }
        public void Begin(MATRIX2x3 transform)
        {
            Begin(ref transform, ref CurrentRenderState.Graphics, null);
        }
        public void Begin(RECT graphics)
        {
            RenderState rs = CurrentRenderState;
            Begin(rs.ThreeD, ref rs.Transform, ref graphics, null);
        }
        public void Begin(MATRIX2x3 transform, RECT graphics)
        {
            Begin(ref transform, ref graphics, null);
        }
        public void Begin(MATRIX2x3 transform, RECT graphics, SHADER shader)
        {
            Begin(ref transform, ref graphics, shader);
        }
        public void Begin(SHADER shader)
        {
            RenderState rs = CurrentRenderState;
            Begin(rs.ThreeD, ref rs.Transform, ref rs.Graphics, shader);
        }
        public void BeginFromPrevious(MATRIX2x3 matrix)
        {
            matrix = FromPrevious(matrix);
            Begin(ref matrix, ref CurrentRenderState.Graphics, null);
        }
        public void BeginFromPrevious(RECT rect)
        {
            rect = FromPrevious(rect);
            Begin(CurrentRenderState.ThreeD, ref CurrentRenderState.Transform, ref rect, null);
        }
        /// <summary>在之前的矩阵的基础上开始绘制</summary>
        public void BeginFromPrevious(MATRIX2x3 matrix, RECT rect)
        {
            matrix = FromPrevious(matrix);
            rect = FromPrevious(rect);
            Begin(ref matrix, ref rect, null);
        }
        public MATRIX2x3 FromPrevious(MATRIX2x3 matrix)
        {
            return matrix * (MATRIX2x3)CurrentRenderState.Transform;
        }
        public RECT FromPrevious(RECT rect)
        {
            RECT _preRect = CurrentRenderState.Graphics;
            rect.X += _preRect.X;
            rect.Y += _preRect.Y;
            RECT.Intersect(ref rect, ref _preRect, out rect);
            rect.X -= _preRect.X;
            rect.Y -= _preRect.Y;
            return rect;
        }
        public RECT FromPreviousNonOffset(RECT rect)
        {
            return RECT.Intersect(CurrentRenderState.Graphics, rect);
        }
        public virtual void Clear()
        {
        }
        public void End()
        {
            if (!HasRenderTarget)
                throw new InvalidOperationException("sprite batch not begin");

            Flush();
            RenderState state = renderStates.Peek();
            renderStates.Pop();
            Ending(state);

            if (HasRenderTarget)
                Begin(CurrentRenderState);
        }
        protected virtual void Ending(RenderState render)
        {
        }
        public TEXTURE Screenshot()
        {
            return Screenshot(FullGraphicsArea);
        }
        public virtual TEXTURE Screenshot(RECT graphics)
        {
            throw new NotImplementedException();
        }
        /// <summary>截屏只能截取到屏幕尺寸而不是画布尺寸</summary>
        public virtual void Screenshot(RECT graphics, Action<TEXTURE> callback)
        {
            if (callback == null)
                throw new ArgumentNullException();
            callback(Screenshot(graphics));
        }
        /// <summary>视框架的支持情况，可能可以截取到指定画布尺寸</summary>
        public virtual void BeginScreenshot(RECT graphics)
        {
            throw new NotImplementedException();
        }
        public virtual TEXTURE EndScreenshot()
        {
            throw new NotImplementedException();
        }
        public void Draw(TEXTURE texture, RECT rect)
        {
            BaseDraw(texture, rect.X, rect.Y, rect.Width, rect.Height, false, float.NaN, 0, 0, 0, false, 0, 0, 0, 0, 0, 0, 0, EFlip.None);
        }
        public void Draw(TEXTURE texture, RECT rect, COLOR color)
        {
            BaseDraw(texture, rect.X, rect.Y, rect.Width, rect.Height, false, float.NaN, 0, 0, 0, true, color.R, color.G, color.B, color.A, 0, 0, 0, EFlip.None);
        }
        public void Draw(TEXTURE texture, RECT rect, RECT source, COLOR color)
        {
            BaseDraw(texture, rect.X, rect.Y, rect.Width, rect.Height, false, source.X, source.Y, source.Width, source.Height, true, color.R, color.G, color.B, color.A, 0, 0, 0, EFlip.None);
        }
        public void Draw(TEXTURE texture, RECT rect, float rotation, float originX, float originY)
        {
            BaseDraw(texture, rect.X, rect.Y, rect.Width, rect.Height, false, float.NaN, 0, 0, 0, false, 0, 0, 0, 0, rotation, originX, originY, EFlip.None);
        }
        public void Draw(TEXTURE texture, RECT rect, float rotation, float originX, float originY, EFlip flip)
        {
            BaseDraw(texture, rect.X, rect.Y, rect.Width, rect.Height, false, float.NaN, 0, 0, 0, false, 0, 0, 0, 0, rotation, originX, originY, flip);
        }
        public void Draw(TEXTURE texture, RECT rect, COLOR color, float rotation, float originX, float originY, EFlip flip)
        {
            BaseDraw(texture, rect.X, rect.Y, rect.Width, rect.Height, false, float.NaN, 0, 0, 0, true, color.R, color.G, color.B, color.A, rotation, originX, originY, flip);
        }
        public void Draw(TEXTURE texture, RECT rect, RECT source, COLOR color, float rotation, float originX, float originY, EFlip flip)
        {
            BaseDraw(texture, rect.X, rect.Y, rect.Width, rect.Height, false, source.X, source.Y, source.Width, source.Height, true, color.R, color.G, color.B, color.A, rotation, originX, originY, flip);
        }
        public void Draw(TEXTURE texture, VECTOR2 location)
        {
            BaseDraw(texture, location.X, location.Y, 1, 1, true, float.NaN, 0, 0, 0, false, 0, 0, 0, 0, 0, 0, 0, EFlip.None);
        }
        public void Draw(TEXTURE texture, VECTOR2 location, COLOR color)
        {
            BaseDraw(texture, location.X, location.Y, 1, 1, true, float.NaN, 0, 0, 0, true, color.R, color.G, color.B, color.A, 0, 0, 0, EFlip.None);
        }
        public void Draw(TEXTURE texture, VECTOR2 location, float rotation, float originX, float originY)
        {
            BaseDraw(texture, location.X, location.Y, 1, 1, true, float.NaN, 0, 0, 0, false, 0, 0, 0, 0, rotation, originX, originY, EFlip.None);
        }
        public void Draw(TEXTURE texture, VECTOR2 location, float rotation, float originX, float originY, float scaleX, float scaleY)
        {
            BaseDraw(texture, location.X, location.Y, scaleX, scaleY, true, float.NaN, 0, 0, 0, false, 0, 0, 0, 0, rotation, originX, originY, EFlip.None);
        }
        public void Draw(TEXTURE texture, VECTOR2 location, float rotation, float originX, float originY, float scaleX, float scaleY, EFlip flip)
        {
            BaseDraw(texture, location.X, location.Y, scaleX, scaleY, true, float.NaN, 0, 0, 0, false, 0, 0, 0, 0, rotation, originX, originY, flip);
        }
        public void Draw(TEXTURE texture, VECTOR2 location, COLOR color, float rotation, float originX, float originY, float scaleX, float scaleY)
        {
            BaseDraw(texture, location.X, location.Y, scaleX, scaleY, true, float.NaN, 0, 0, 0, true, color.R, color.G, color.B, color.A, rotation, originX, originY, EFlip.None);
        }
        public void Draw(TEXTURE texture, VECTOR2 location, COLOR color, float rotation, float originX, float originY, float scaleX, float scaleY, EFlip flip)
        {
            BaseDraw(texture, location.X, location.Y, scaleX, scaleY, true, float.NaN, 0, 0, 0, true, color.R, color.G, color.B, color.A, rotation, originX, originY, flip);
        }
        public void Draw(TEXTURE texture, VECTOR2 location, RECT source, COLOR color, float rotation, VECTOR2 origin, VECTOR2 scale, EFlip flip)
        {
            BaseDraw(texture, location.X, location.Y, scale.X, scale.Y, true, source.X, source.Y, source.Width, source.Height, true, color.R, color.G, color.B, color.A, rotation, origin.X, origin.Y, flip);
        }
        public void BaseDraw(TEXTURE texture,
            float x, float y, float w, float h, bool scale, 
            float sx, float sy, float sw, float sh,
            bool color, byte r, byte g, byte b, byte a, 
            float rotation, float ox, float oy, EFlip flip)
        {
            if (texture == null)
            {
                throw new ArgumentNullException("texture");
            }
            if (texture.IsDisposed)
            {
                _LOG.Warning("Draw disposed texture!");
                return;
            }
            if (float.IsNaN(sx))
            {
                sx = 0;
                sy = 0;
                sw = texture.Width;
                sh = texture.Height;
            }
            if (sw == 0 || sh == 0 || w == 0 || h == 0)
            {
                return;
            }
            if (scale)
            {
                w *= sw;
                h *= sh;
            }
            if (!color)
            {
                r = DefaultColor.R;
                g = DefaultColor.G;
                b = DefaultColor.B;
                a = DefaultColor.A;
            }

            //spriteQueue[spriteQueueCount].Destination.X = x;
            //spriteQueue[spriteQueueCount].Destination.Y = y;
            //spriteQueue[spriteQueueCount].Destination.Width = w;
            //spriteQueue[spriteQueueCount].Destination.Height = h;
            //spriteQueue[spriteQueueCount].Source.X = sx;
            //spriteQueue[spriteQueueCount].Source.Y = sy;
            //spriteQueue[spriteQueueCount].Source.Width = sw;
            //spriteQueue[spriteQueueCount].Source.Height = sh;
            //spriteQueue[spriteQueueCount].Color.R = r;
            //spriteQueue[spriteQueueCount].Color.G = g;
            //spriteQueue[spriteQueueCount].Color.B = b;
            //spriteQueue[spriteQueueCount].Color.A = a;
            //spriteQueue[spriteQueueCount].Rotation = rotation;
            //spriteQueue[spriteQueueCount].Origin.X = ox;
            //spriteQueue[spriteQueueCount].Origin.Y = oy;
            //spriteQueue[spriteQueueCount].Flip = flip;
            //Draw(texture, ref spriteQueue[spriteQueueCount]);

            SpriteVertex vertex;
            vertex.Destination.X = x;
            vertex.Destination.Y = y;
            vertex.Destination.Width = w;
            vertex.Destination.Height = h;
            vertex.Source.X = sx;
            vertex.Source.Y = sy;
            vertex.Source.Width = sw;
            vertex.Source.Height = sh;
            vertex.Color.R = r;
            vertex.Color.G = g;
            vertex.Color.B = b;
            vertex.Color.A = a;
            vertex.Rotation = rotation;
            vertex.Origin.X = ox;
            vertex.Origin.Y = oy;
            vertex.Flip = flip;
            Draw(texture, ref vertex);
        }
        public void Draw(TEXTURE texture, ref SpriteVertex vertex)
        {
            if (texture.Draw(this, ref vertex))
                return;

            if (vertex.Source.Width <= 0f || vertex.Source.Height <= 0f)
                return;

            // 计算BoundingBox，一方面用于Culling，一方面用于合批时的遮挡判断
            BoundingBox box;

            MATRIX2x3 matrix = CurrentTransform;
            if (vertex.Rotation == 0 && vertex.Flip == EFlip.None)
            {
                RECT desc = vertex.Destination;
                if (vertex.Origin.X != 0)
                {
                    desc.X -= desc.Width * (vertex.Origin.X / vertex.Source.Width);
                    //vertex.Origin.X = 0;
                }
                if (vertex.Origin.Y != 0)
                {
                    desc.Y -= desc.Height * (vertex.Origin.Y / vertex.Source.Height);
                    //vertex.Origin.Y = 0;
                }

                if (matrix.IsIdentity())
                {
                    box.Left = desc.X;
                    box.Top = desc.Y;
                    box.Right = desc.Right;
                    box.Bottom = desc.Bottom;
                }
                else
                {
                    RECT result;
                    RECT.CreateBoundingBox(ref desc, ref matrix, out result);
                    box.Left = result.X;
                    box.Top = result.Y;
                    box.Right = result.Right;
                    box.Bottom = result.Bottom;
                }
            }
            else
            {
                // 超出视口部分不绘制
                MATRIX2x3 current;
                __GRAPHICS.DrawMatrix(ref vertex.Destination, ref vertex.Source, vertex.Rotation, ref vertex.Origin, vertex.Flip, out current);

                if (!matrix.IsIdentity())
                    current = current * matrix;

                RECT result = vertex.Destination;
                result.X = 0;
                result.Y = 0;
                RECT.CreateBoundingBox(ref result, ref current, out result);

                box.Left = result.X;
                box.Top = result.Y;
                box.Right = result.Right;
                box.Bottom = result.Bottom;
            }
            
            RECT scissor = CurrentGraphics;
            BoundingBox graphics;
            graphics.Left = scissor.X;
            graphics.Top = scissor.Y;
            graphics.Right = scissor.Right;
            graphics.Bottom = scissor.Bottom;
            if (!Culling || box.Intersects(ref graphics))
                InternalDraw(texture, ref vertex, ref box);
        }
        //protected bool CheckCulling(ref SpriteVertex vertex)
        //{
        //    MATRIX2x3 matrix = CurrentTransform;
        //    RECT viewport = CurrentGraphics;

        //    if (vertex.Rotation == 0 && vertex.Flip == EFlip.None)
        //    {
        //        if (vertex.Origin.X != 0)
        //        {
        //            vertex.Destination.X -= vertex.Destination.Width * (vertex.Origin.X / vertex.Source.Width);
        //            vertex.Origin.X = 0;
        //        }
        //        if (vertex.Origin.Y != 0)
        //        {
        //            vertex.Destination.Y -= vertex.Destination.Height * (vertex.Origin.Y / vertex.Source.Height);
        //            vertex.Origin.Y = 0;
        //        }

        //        bool draw = true;
        //        if (matrix.IsIdentity())
        //        {
        //            draw = viewport.Intersects(vertex.Destination);
        //        }
        //        else
        //        {
        //            RECT result;
        //            RECT.CreateBoundingBox(ref vertex.Destination, ref matrix, out result);
        //            draw = viewport.Intersects(result);
        //        }

        //        return draw;
        //    }
        //    else
        //    {
        //        // 超出视口部分不绘制
        //        MATRIX2x3 current;
        //        __GRAPHICS.DrawMatrix(ref vertex.Destination, ref vertex.Source, vertex.Rotation, ref vertex.Origin, vertex.Flip, out current);

        //        if (!matrix.IsIdentity())
        //            current = current * matrix;

        //        RECT result = vertex.Destination;
        //        result.X = 0;
        //        result.Y = 0;
        //        RECT.CreateBoundingBox(ref result, ref current, out result);

        //        return viewport.Intersects(result);
        //    }
        //}
        protected virtual void InternalDraw(TEXTURE texture, ref SpriteVertex vertex, ref BoundingBox box)
        {
            //if (texture != currentTexture)
            //{
            //    if (texture != null && currentTexture != null)
            //    {
            //        this.Flush();
            //    }
            //    this.currentTexture = texture;
            //}

            //if (this.spriteQueueCount >= this.spriteQueue.Length)
            //    Array.Resize<SpriteVertex>(ref this.spriteQueue, this.spriteQueue.Length * 2);

            ////spriteQueue[spriteQueueCount++] = vertex;
            //int index = spriteQueueCount;
            //spriteQueue[index].Destination.X = vertex.Destination.X;
            //spriteQueue[index].Destination.Y = vertex.Destination.Y;
            //spriteQueue[index].Destination.Width = vertex.Destination.Width;
            //spriteQueue[index].Destination.Height = vertex.Destination.Height;
            //spriteQueue[index].Source.X = vertex.Source.X;
            //spriteQueue[index].Source.Y = vertex.Source.Y;
            //spriteQueue[index].Source.Width = vertex.Source.Width;
            //spriteQueue[index].Source.Height = vertex.Source.Height;
            //spriteQueue[index].Color.R = vertex.Color.R;
            //spriteQueue[index].Color.G = vertex.Color.G;
            //spriteQueue[index].Color.B = vertex.Color.B;
            //spriteQueue[index].Color.A = vertex.Color.A;
            //spriteQueue[index].Rotation = vertex.Rotation;
            //spriteQueue[index].Origin.X = vertex.Origin.X;
            //spriteQueue[index].Origin.Y = vertex.Origin.Y;
            //spriteQueue[index].Flip = vertex.Flip;
            //spriteQueueCount++;

            RenderBuffer buffer = null;
            TEXTURE drawable = TEXTURE.GetDrawableTexture(texture);
            if (drawable == currentTexture)
            {
                // 不更换图片和原来一样缓存参数
                buffer = buffers[bufferCount - 1];
            }
            else
            {
                //Flush();
                //if (buffers[0] == null) buffers[0] = new RenderBuffer();
                //buffer = buffers[0];
                //bufferCount = 1;
                //textures[textureCount] = drawable;
                //currentTexture = drawable;
                int drawableIndex = -1;
                for (int i = 0; i < textureCount; i++)
                {
                    if (textures[i] == drawable)
                    {
                        drawableIndex = i;
                        break;
                    }
                }

                bool newBuffer = false;
                if (drawableIndex == -1)
                {
                    // 新图片，新缓存
                    if (textureCount == textures.Length)
                        Array.Resize(ref textures, textureCount << 1);
                    textures[textureCount] = drawable;
                    drawableIndex = textureCount;
                    textureCount++;
                    newBuffer = true;
                }
                else
                {
                    for (int i = bufferCount - 1; i >= 0; i--)
                    {
                        if (buffers[i].TextureIndex == drawableIndex)
                        {
                            // 检查缓存图片之后的所有缓存中的渲染包围盒是否会被此次渲染遮挡
                            // 只要渲染之间没有遮挡关系，渲染顺序现后就无所谓
                            // 此时将本次渲染提到之前相同图片的渲染位置，减少更换图片的批次
                            for (int j = bufferCount - 1; j > i; j--)
                            {
                                for (int s = buffers[j].spriteQueueCount - 1; s >= 0; s--)
                                {
                                    // 遮挡了其它对象，不能提前合批渲染
                                    if (buffers[j].spriteBoundingBox[s].Intersects(ref box))
                                    {
                                        newBuffer = true;
                                        break;
                                    }
                                }
                                // 已经存在了遮挡关系，需要新建Buffer
                                if (newBuffer)
                                    break;
                            }
                            // 采用之前的Buffer
                            if (!newBuffer)
                            {
                                buffer = buffers[i];
                            }
                            break;
                        }
                    }
                }

                // 新图片或者本次渲染遮挡了之前的其它渲染才导致需要新建Buffer
                if (newBuffer)
                {
                    if (bufferCount == buffers.Length)
                        Array.Resize(ref buffers, bufferCount << 1);

                    if (buffers[bufferCount] == null)
                        buffers[bufferCount] = new RenderBuffer();

                    buffer = buffers[bufferCount];
                    buffer.TextureIndex = drawableIndex;
                    bufferCount++;

                    currentTexture = drawable;
                }
            }

            // 像缓存后面追加渲染顶点
            int index = buffer.spriteQueueCount;
            if (index >= buffer.spriteQueue.Length)
            {
                int newSize = index << 1;
                Array.Resize(ref buffer.spriteQueue, newSize);
                Array.Resize(ref buffer.spriteBoundingBox, newSize);
            }
            buffer.spriteQueue[index].Destination.X = (int)vertex.Destination.X;
            buffer.spriteQueue[index].Destination.Y = (int)vertex.Destination.Y;
            buffer.spriteQueue[index].Destination.Width = (int)vertex.Destination.Width;
            buffer.spriteQueue[index].Destination.Height = (int)vertex.Destination.Height;
            buffer.spriteQueue[index].Source.X = vertex.Source.X;
            buffer.spriteQueue[index].Source.Y = vertex.Source.Y;
            buffer.spriteQueue[index].Source.Width = vertex.Source.Width;
            buffer.spriteQueue[index].Source.Height = vertex.Source.Height;
            buffer.spriteQueue[index].Color.R = vertex.Color.R;
            buffer.spriteQueue[index].Color.G = vertex.Color.G;
            buffer.spriteQueue[index].Color.B = vertex.Color.B;
            buffer.spriteQueue[index].Color.A = vertex.Color.A;
            buffer.spriteQueue[index].Rotation = vertex.Rotation;
            buffer.spriteQueue[index].Origin.X = vertex.Origin.X;
            buffer.spriteQueue[index].Origin.Y = vertex.Origin.Y;
            buffer.spriteQueue[index].Flip = vertex.Flip;

            buffer.spriteBoundingBox[index] = box;

            buffer.spriteQueueCount++;
        }
        public void Draw(FONT font, string text, VECTOR2 location, COLOR color)
        {
            Draw(font, text, location, color, 1);
        }
        public void Draw(FONT font, string text, VECTOR2 location, COLOR color, float scale)
        {
            if (Culling)
            {
                VECTOR2 size = font.MeasureString(text);
                RECT desc;
                desc.X = location.X;
                desc.Y = location.Y;
                desc.Width = size.X * scale;
                desc.Height = size.Y * scale;

                // 超出视口部分不绘制
                MATRIX2x3 matrix = CurrentTransform;
                RECT viewport = CurrentGraphics;
                if (!matrix.IsIdentity())
                    RECT.CreateBoundingBox(ref desc, ref matrix, out desc);
                if (viewport.Intersects(desc))
                {
                    BaseDrawFont(font, text, location, color, scale);
                }
            }
            else
            {
                BaseDrawFont(font, text, location, color, scale);
            }
        }
        public void Draw(FONT font, string text, RECT bound, COLOR color, UI.EPivot alignment)
        {
            VECTOR2 location = UI.UIElement.TextAlign(bound, font.MeasureString(text), alignment);
            Draw(font, text, location, color, 1);
        }
        public void Draw(FONT font, string text, RECT bound, COLOR color, UI.EPivot alignment, float scale)
        {
            VECTOR2 location = UI.UIElement.TextAlign(bound, font.MeasureString(text) * scale, alignment);
            Draw(font, text, location, color, scale);
        }
        protected virtual void BaseDrawFont(FONT font, string text, VECTOR2 location, COLOR color, float scale)
        {
            font.Draw(this, text, location, color, scale);
        }
        protected internal virtual void Render()
        {
            if (renderStates.Count > 0)
                throw new InvalidOperationException("Has render state not End.");
            //_LOG.Debug("Render Count: {0}", TestFlushCount);
            //TestFlushCount = 0;
        }

        //int TestFlushCount;
        protected void Flush()
        {
            if (bufferCount == 0)
                return;

            //_LOG.Debug("buffer count: {0} stack: {1}", bufferCount, Environment.StackTrace);
            for (int r = 0; r < bufferCount; r++)
            {
                RenderBuffer buffer = buffers[r];

                currentTexture = textures[buffer.TextureIndex];
                DrawPrimitivesBegin(currentTexture, EPrimitiveType.Triangle);

                int offset = 0;
                int count = buffer.spriteQueueCount;
                //_LOG.Debug("{0}: tname:{1} count:{2}", TestFlushCount, currentTexture.Key, count);
                buffer.spriteQueueCount = 0;
                while (count > 0)
                {
                    // 批绘制顶点缓存最多只有8192个，所以最多一次处理2048个Sprite
                    int num = count;
                    if (num > MAX_BATCH_COUNT)
                        num = MAX_BATCH_COUNT;

                    for (int i = 0; i < num; i++)
                        InputVertexToOutputVertex(ref buffer.spriteQueue[offset + i], i * 4);

                    DrawPrimitives(EPrimitiveType.Triangle, outputVertices, 0, num * 4, indices, 0, num * 2);
                    offset += num;
                    count -= num;
                }

                DrawPrimitivesEnd();

                //TestFlushCount++;
            }

            textureCount = 0;
            bufferCount = 0;
            currentTexture = null;
        }
        private void InputVertexToOutputVertex(ref SpriteVertex vertex, int outputIndex)
        {
            float rotation = vertex.Rotation;
            float cos, sin;
            if (rotation != 0f)
            {
                cos = (float)Math.Cos(rotation);
                sin = (float)Math.Sin(rotation);
            }
            else
            {
                cos = 1f;
                sin = 0f;
            }

            for (int i = 0; i < 4; i++)
            {
                float xc = XCornerOffsets[i];
                float yc = YCornerOffsets[i];
                float xOffset = (xc - vertex.Origin.X) * vertex.Destination.Width;
                float yOffset = (yc - vertex.Origin.Y) * vertex.Destination.Height;
                //float x = vertex.Destination.X + xOffset * cos - yOffset * sin;
                //float y = vertex.Destination.Y + xOffset * sin + yOffset * cos;
                if ((vertex.Flip & EFlip.FlipHorizontally) != EFlip.None)
                    xc = 1f - xc;
                if ((vertex.Flip & EFlip.FlipVertically) != EFlip.None)
                    yc = 1f - yc;

                int index = outputIndex + i;
                outputVertices[index].Position.X = vertex.Destination.X + xOffset * cos - yOffset * sin;
                outputVertices[index].Position.Y = vertex.Destination.Y + xOffset * sin + yOffset * cos;
                outputVertices[index].TextureCoordinate.X = vertex.Source.X + xc * vertex.Source.Width;
                outputVertices[index].TextureCoordinate.Y = vertex.Source.Y + yc * vertex.Source.Height;
                outputVertices[index].Color.R = vertex.Color.R;
                outputVertices[index].Color.G = vertex.Color.G;
                outputVertices[index].Color.B = vertex.Color.B;
                outputVertices[index].Color.A = vertex.Color.A;
            }
        }
        /// <summary>三角形绘制</summary>
        public void DrawPrimitives(TEXTURE texture, TextureVertex[] vertices, int offset, int count, short[] indexes, int indexOffset, int primitiveCount)
        {
            DrawPrimitives(texture, EPrimitiveType.Triangle, vertices, offset, count, indexes, indexOffset, primitiveCount);
        }
        public void DrawPrimitives(TEXTURE texture, EPrimitiveType ptype, TextureVertex[] vertices, int offset, int count, short[] indexes, int indexOffset, int primitiveCount)
        {
            //var drawable = TEXTURE.GetDrawableTexture(texture);
            //bool changeTex = currentTexture != drawable;
            Flush();
            currentTexture = texture;
            //DrawPrimitivesBegin(changeTex ? drawable : null, ptype);
            DrawPrimitivesBegin(texture, ptype);
            DrawPrimitives(ptype, vertices, offset, count, indexes, indexOffset, primitiveCount);
            DrawPrimitivesEnd();
        }
        protected abstract void DrawPrimitives(EPrimitiveType ptype, TextureVertex[] vertices, int offset, int count, short[] indexes, int indexOffset, int primitiveCount);
        protected abstract void DrawPrimitivesBegin(TEXTURE texture, EPrimitiveType ptype);
        protected virtual void DrawPrimitivesEnd()
        {
        }

        /// <summary>每6个为一组按的索引0,1,2,0,2,3</summary>
        public static short[] CreateTriangleIndexData(int vertexCount)
        {
            short[] array = new short[(vertexCount >> 2) * 6];
            for (int i = 0, m = vertexCount >> 2; i < m; i++)
            {
                array[i * 6] = (short)(i * 4);
                array[i * 6 + 1] = (short)(i * 4 + 1);
                array[i * 6 + 2] = (short)(i * 4 + 2);
                array[i * 6 + 3] = (short)(i * 4);
                array[i * 6 + 4] = (short)(i * 4 + 2);
                array[i * 6 + 5] = (short)(i * 4 + 3);
            }
            return array;
        }
        /// <summary>每3个为一组按顺序的索引0,1,2,1,2,3,2,3,4</summary>
        public static short[] CreateOrderIndexData(int vertexCount)
        {
            if (vertexCount < 2)
                throw new ArgumentOutOfRangeException();
            vertexCount -= 2;
            short[] array = new short[vertexCount * 3];
            for (short i = 0; i < vertexCount; i++)
            {
                array[i * 3] = i;
                array[i * 3 + 1] = (short)(i + 1);
                array[i * 3 + 2] = (short)(i + 2);
            }
            return array;
        }
    }
    [Code(ECode.LessUseful | ECode.BeNotTest)]
    public class VertexPool
    {
        private TEXTURE texture;
        private TextureVertex[] vertices;
        private int index;
        private int triangle;
        private short[] indices;

        public VertexPool(int capcity, short[] indices)
        {
            if (capcity <= 0)
                throw new ArgumentException("capcity");

            if (indices == null)
                throw new ArgumentNullException("indices");

            if (indices.Length <= capcity)
                throw new ArgumentException("Indices count is not enough.");

            this.indices = indices;
            vertices = new TextureVertex[capcity];
        }

        public bool IsNeedFlush(TEXTURE tex, int count)
        {
            if (tex == null)
                return false;

            if (tex != texture)
                return true;
            this.texture = tex;

            if (index + count >= vertices.Length)
                return true;

            return false;
        }
        public void Flush(GRAPHICS g)
        {
            if (texture == null)
                return;

            if (index == 0)
            {
                texture = null;
                return;
            }

            g.DrawPrimitives(texture, vertices, 0, index, indices, 0, triangle);

            texture = null;
            index = 0;
            triangle = 0;
        }
        public void AllocVertex(int vertexCount, int triangleCount, ActionRef<TextureVertex> action)
        {
            for (int i = 0; i < vertexCount; i++)
                action(ref vertices[index + i]);
            index += vertexCount;
            triangleCount += triangleCount;
        }
    }


    #region cpu render

    public class TextureCPU : TEXTURE
    {
        internal int width;
        private int height;
        internal COLOR[] datas;

        public override int Width
        {
            get { return width; }
        }
        public override int Height
        {
            get { return height; }
        }
        public override bool IsDisposed
        {
            get { return datas == null; }
        }

        public TextureCPU(COLOR[] color, int width)
        {
            if (color.Length % width != 0)
                throw new ArgumentException();
            this.datas = color;
            this.width = width;
            this.height = color.Length / width;
        }
        public TextureCPU(TEXTURE texture)
        {
            this.width = texture.Width;
            this.height = texture.Height;
            this.datas = texture.GetData();
        }

        public COLOR GetColor(ushort x, ushort y)
        {
            return datas[y * width + x];
        }
        public override COLOR[] GetData(RECT area)
        {
            if (area.X == 0 && area.Y == 0 && area.Width == width && area.Height == height)
                return datas;
            return Utility.GetArray(datas, (int)area.X, (int)area.Y, (int)area.Width, (int)area.Height, width);
        }
        public override void SetData(COLOR[] buffer, RECT area)
        {
            Utility.SetArray(buffer, datas, (int)area.X, (int)area.Y, (int)area.Width, (int)area.Height, width, (int)area.Width, 0);
        }
        protected internal override void InternalDispose()
        {
            this.datas = null;
        }
    }
    public abstract class CPUShaderBase : IDisposable
    {
        public bool Enable = true;
        protected internal abstract void main();
        public virtual void Dispose() { }
    }
    public abstract class CPUShaderVertex : CPUShaderBase
    {
        protected internal static VECTOR3 Position;
    }
    public class VSDefault : CPUShaderVertex
    {
        public static VSDefault Default { get; private set; }
        static VSDefault()
        {
            Default = new VSDefault();
        }
        private VSDefault() { }
        internal MATRIX view;
        internal VECTOR3 vpos;
        internal COLOR vcolor;
        internal VECTOR2 vcoord;
        protected internal override void main()
        {
            VECTOR3.Transform(ref vpos, ref view, out Position);
        }
    }
    public abstract class CPUShaderPixel : CPUShaderBase
    {
        public const float R255 = 1f / 255f;
        protected internal static COLOR Color;
        protected internal static TextureCPU Sampler
        {
            get;
            internal set;
        }
    }
    public class PSDefault : CPUShaderPixel
    {
        public static PSDefault Default { get; private set; }
        static PSDefault()
        {
            Default = new PSDefault();
        }
        private PSDefault() { }
        internal COLOR color;
        internal VECTOR2 coord;
        protected internal override void main()
        {
            if (coord.Y >= Sampler.Height) coord.Y %= Sampler.Height;
            if (coord.X >= Sampler.width) coord.X %= Sampler.width;
            COLOR scolor = Sampler.datas[(ushort)coord.Y * Sampler.width + (ushort)coord.X];
            Color.R = (byte)(color.R * scolor.R * R255);
            Color.G = (byte)(color.G * scolor.G * R255);
            Color.B = (byte)(color.B * scolor.B * R255);
            Color.A = (byte)(color.A * scolor.A * R255);
        }
    }
    public sealed class GraphicsDeviceCPU : GRAPHICS
    {
        private enum EColorFlag
        {
            /// <summary>
            /// 线性计算每个像素点的值
            /// </summary>
            Linear,
            /// <summary>
            /// 固定计算像素点的值
            /// </summary>
            Fixed,
            /// <summary>
            /// 不计算颜色的值
            /// </summary>
            Ignore,
        }

        private CPUShaderVertex[] currentVS = new CPUShaderVertex[4];
        private CPUShaderPixel[] currentPS = new CPUShaderPixel[4];
        private int vsIndex;
        private int psIndex;
        private VECTOR3[] triangleVertex = new VECTOR3[3];
        private byte[] triangleVertexIndex = new byte[3];
        private TextureVertex _four;
        private VECTOR3[] lineVertex = new VECTOR3[2];
        private Dictionary<TEXTURE, TextureCPU> textureDataCache = new Dictionary<TEXTURE, TextureCPU>();

        private COLOR[] buffers;
        private ushort bufferWidth;
        private ushort bufferHeight;
        private TEXTURE screenGraphics;
        private BoundingBox scissor;
        private GRAPHICS baseDevice;

        public override bool IsFullScreen
        {
            get { return baseDevice.IsFullScreen; }
            set { baseDevice.IsFullScreen = value; }
        }
        protected override VECTOR2 InternalScreenSize
        {
            get { return baseDevice.ScreenSize; }
            set { baseDevice.ScreenSize = value; }
        }

        public GraphicsDeviceCPU(GRAPHICS device)
        {
            if (device == null)
                throw new ArgumentNullException("GraphicsDevice");
            this.baseDevice = device;
            this.GraphicsSize = device.GraphicsSize;
        }

        private void SetVS()
        {
            vsIndex = 0;
            //foreach (var shader in Shaders)
            //{
            //    foreach (var vs in shader.VS)
            //    {
            //        if (vs.Enable)
            //        {
            //            if (currentVS.Length == vsIndex)
            //            {
            //                Array.Resize(ref currentVS, vsIndex * 2);
            //            }
            //            currentVS[vsIndex++] = vs;
            //        }
            //    }
            //}
        }
        private void SetPS()
        {
            psIndex = 0;
            //foreach (var shader in Shaders)
            //{
            //    foreach (var ps in shader.PS)
            //    {
            //        if (ps.Enable)
            //        {
            //            if (currentPS.Length == vsIndex)
            //            {
            //                Array.Resize(ref currentPS, psIndex * 2);
            //            }
            //            currentPS[psIndex++] = ps;
            //        }
            //    }
            //}
        }
        protected override void SetViewport(MATRIX2x3 view, RECT viewport)
        {
            if (baseDevice.ViewportMode != this.ViewportMode)
                baseDevice.ViewportMode = this.ViewportMode;
            if (baseDevice.GraphicsSize != this.GraphicsSize)
                baseDevice.GraphicsSize = this.GraphicsSize;
            if (baseDevice.ScreenSize != this.ScreenSize)
                baseDevice.ScreenSize = this.ScreenSize;
            viewport = AreaToScreen(viewport);
            bufferWidth = (ushort)viewport.Width;
            bufferHeight = (ushort)viewport.Height;
            buffers = new COLOR[bufferHeight * bufferWidth];
            screenGraphics = Entry.Instance.NewTEXTURE(bufferWidth, bufferHeight);
            view.M31 = 0;
            view.M32 = 0;
        }
        protected override void InternalBegin(bool threeD, ref MATRIX matrix, ref RECT graphics, SHADER shader)
        {
            VSDefault.Default.view = matrix;
            this.scissor.Left = graphics.X;
            this.scissor.Top = graphics.Y;
            this.scissor.Right = graphics.Right;
            this.scissor.Bottom = graphics.Bottom;
        }
        protected override void DrawPrimitivesBegin(TEXTURE texture, EPrimitiveType ptype)
        {
            TEXTURE drawable = TEXTURE.GetDrawableTexture(texture);
            TextureCPU cpu;
            if (!textureDataCache.TryGetValue(drawable, out cpu))
            {
                cpu = new TextureCPU(drawable);
                textureDataCache.Add(drawable, cpu);
            }
            PSDefault.Sampler = cpu;
        }
        protected override void DrawPrimitives(EPrimitiveType ptype, TextureVertex[] vertices, int offset, int count, short[] indexes, int indexOffset, int primitiveCount)
        {
            TIMER timer = TIMER.StartNew();

            SetVS();
            SetPS();

            switch (ptype)
            {
                case EPrimitiveType.Point:
                    #region Point
                    for (int i = 0; i < primitiveCount; i++)
                    {
                        int index = offset + indexes[indexOffset + i];
                        // vertex shader
                        VSDefault.Default.vpos = vertices[index].Position;
                        //VSDefault.Default.vcolor = vertices[index].Color;
                        //VSDefault.Default.vcoord = vertices[index].TextureCoordinate;
                        VSDefault.Default.main();
                        for (int s = 0; s < vsIndex; s++)
                            currentVS[s].main();
                        // pixel shader
                        PSDefault.Default.color = VSDefault.Default.vcolor;
                        PSDefault.Default.coord = VSDefault.Default.vcoord;
                        PSDefault.Default.main();
                        for (int s = 0; s < psIndex; s++)
                            currentPS[s].main();
                        // scissor
                        if (CPUShaderVertex.Position.X >= scissor.Left &&
                            CPUShaderVertex.Position.X < scissor.Right &&
                            CPUShaderVertex.Position.Y >= scissor.Top &&
                            CPUShaderVertex.Position.Y <= scissor.Bottom)
                        {
                            // render
                            buffers[(int)CPUShaderVertex.Position.Y * bufferWidth + (int)CPUShaderVertex.Position.X] = CPUShaderPixel.Color;
                        }
                    }
                    #endregion
                    break;

                case EPrimitiveType.Line:
                    #region Line

                    #endregion
                    break;

                case EPrimitiveType.Triangle:
                    //vertices[0].Color = COLOR.Red;
                    //vertices[1].Color = COLOR.Lime;
                    //vertices[2].Color = COLOR.Blue;
                    //vertices[3].Color = COLOR.White;
                    //primitiveCount = 1;
                    #region Triangle
                    for (int i = 0; i < primitiveCount; i++)
                    {
                        int index = indexOffset + i * 3;
                        for (int p = 0; p < 3; p++)
                        {
                            // vertex shader
                            VSDefault.Default.vpos = vertices[offset + indexes[index + p]].Position;
                            //VSDefault.Default.vcolor = vertices[index].Color;
                            //VSDefault.Default.vcoord = vertices[index].TextureCoordinate;
                            VSDefault.Default.main();
                            for (int s = 0; s < vsIndex; s++)
                                currentVS[s].main();
                            triangleVertex[p] = CPUShaderVertex.Position;
                        }

                        // sort triangle vertex
                        triangleVertexIndex[0] = 0;
                        triangleVertexIndex[1] = 1;
                        triangleVertexIndex[2] = 2;
                        SortVertex(0, 1);
                        SortVertex(1, 2);
                        SortVertex(0, 1);

                        // render
                        int index1 = offset + indexes[index + triangleVertexIndex[0]];
                        int index2 = offset + indexes[index + triangleVertexIndex[1]];
                        int index3 = offset + indexes[index + triangleVertexIndex[2]];
                        if (triangleVertex[triangleVertexIndex[0]].Y == triangleVertex[triangleVertexIndex[1]].Y)
                        {
                            // 上底三角形
                            DrawTriangle(
                                ref vertices[index3], ref vertices[index1], ref vertices[index2],
                                ref triangleVertex[triangleVertexIndex[2]], ref triangleVertex[triangleVertexIndex[0]], ref triangleVertex[triangleVertexIndex[1]]);
                        }
                        else if (triangleVertex[triangleVertexIndex[1]].Y == triangleVertex[triangleVertexIndex[2]].Y)
                        {
                            // 下底三角形
                            DrawTriangle(
                                ref vertices[index1], ref vertices[index2], ref vertices[index3],
                                ref triangleVertex[triangleVertexIndex[0]], ref triangleVertex[triangleVertexIndex[1]], ref triangleVertex[triangleVertexIndex[2]]);
                        }
                        else
                        {
                            // 拆分成上底和下底的两个三角形
                            VECTOR3 four;
                            four.Y = triangleVertex[triangleVertexIndex[1]].Y;
                            four.Z = 0;

                            float v = (triangleVertex[triangleVertexIndex[1]].Y - triangleVertex[triangleVertexIndex[0]].Y) / (triangleVertex[triangleVertexIndex[2]].Y - triangleVertex[triangleVertexIndex[0]].Y);
                            float w = 1 - v;

                            four.X = triangleVertex[triangleVertexIndex[0]].X + v * (triangleVertex[triangleVertexIndex[2]].X - triangleVertex[triangleVertexIndex[0]].X);

                            // color差值
                            _four.Color.R = (byte)(vertices[index1].Color.R * w + vertices[index3].Color.R * v);
                            _four.Color.G = (byte)(vertices[index1].Color.G * w + vertices[index3].Color.G * v);
                            _four.Color.B = (byte)(vertices[index1].Color.B * w + vertices[index3].Color.B * v);
                            _four.Color.A = (byte)(vertices[index1].Color.A * w + vertices[index3].Color.A * v);

                            // uv差值
                            _four.TextureCoordinate.X = vertices[index1].TextureCoordinate.X * w + vertices[index3].TextureCoordinate.X * v;
                            _four.TextureCoordinate.Y = vertices[index1].TextureCoordinate.Y * w + vertices[index3].TextureCoordinate.Y * v;

                            if (four.X > triangleVertex[triangleVertexIndex[1]].X)
                            {
                                // 上底三角形
                                DrawTriangle(
                                    ref vertices[index1], ref vertices[index2], ref _four,
                                    ref triangleVertex[triangleVertexIndex[0]], ref triangleVertex[triangleVertexIndex[1]], ref four);
                                // 下底三角形
                                DrawTriangle(
                                    ref vertices[index3], ref vertices[index2], ref _four,
                                    ref triangleVertex[triangleVertexIndex[2]], ref triangleVertex[triangleVertexIndex[1]], ref four);
                            }
                            else
                            {
                                // 上底三角形
                                DrawTriangle(
                                    ref vertices[index1], ref _four, ref vertices[index2],
                                    ref triangleVertex[triangleVertexIndex[0]], ref four, ref triangleVertex[triangleVertexIndex[1]]);
                                // 下底三角形
                                DrawTriangle(
                                    ref vertices[index3], ref _four, ref vertices[index2],
                                    ref triangleVertex[triangleVertexIndex[2]], ref four, ref triangleVertex[triangleVertexIndex[1]]);
                            }
                        }
                    }
                    #endregion
                    break;

                default: throw new NotImplementedException();
            }
        }
        private void SortVertex(byte index1, byte index2)
        {
            bool swap = false;
            // 从上到下，从左到右排列顶点
            float y = triangleVertex[triangleVertexIndex[index1]].Y - triangleVertex[triangleVertexIndex[index2]].Y;
            if (y > 0)
                swap = true;
            else if (y == 0)
                swap = triangleVertex[triangleVertexIndex[index1]].X > triangleVertex[triangleVertexIndex[index2]].X;
            if (swap)
            {
                byte index = triangleVertexIndex[index1];
                triangleVertexIndex[index1] = triangleVertexIndex[index2];
                triangleVertexIndex[index2] = index;
            }
        }
        private void DrawTriangle(
            ref TextureVertex _pos, ref TextureVertex _lef, ref TextureVertex _rig,
            ref VECTOR3 pos, ref VECTOR3 lef, ref VECTOR3 rig)
        {
            bool downBottom = lef.Y > pos.Y;
            short ytop;
            short ybot;
            if (downBottom)
            {
                ytop = (short)pos.Y;
                ybot = (short)lef.Y;
            }
            else
            {
                ytop = (short)lef.Y;
                ybot = (short)pos.Y;
            }

            // scissor
            if (ybot < scissor.Top || ytop >= scissor.Bottom)
                return;

            // 颜色差值类型
            EColorFlag colorFlag;
            if ((_pos.Color.R != _lef.Color.R || _pos.Color.G != _lef.Color.G || _pos.Color.B != _lef.Color.B || _pos.Color.A != _lef.Color.A) ||
                (_pos.Color.R != _rig.Color.R || _pos.Color.G != _rig.Color.G || _pos.Color.B != _rig.Color.B || _pos.Color.A != _rig.Color.A) ||
                (_rig.Color.R != _lef.Color.R || _rig.Color.G != _lef.Color.G || _rig.Color.B != _lef.Color.B || _rig.Color.A != _lef.Color.A))
            {
                colorFlag = EColorFlag.Linear;
            }
            else
            {
                colorFlag = EColorFlag.Fixed;
                PSDefault.Default.color = _pos.Color;
            }

            // 三角形两条边的反斜率
            float bottomY = lef.Y;
            float yxlef = (pos.X - lef.X) / (pos.Y - bottomY);
            float yxrig = (pos.X - rig.X) / (pos.Y - bottomY);
            // uv差值
            float v;
            float _v = 1f / (ybot - ytop);
            if (downBottom)
            {
                v = 1;
                _v = -_v;
            }
            else
            {
                v = 0;
            }
            float u;
            float _u = 1f / (rig.X - lef.X);
            for (short y = ytop; y < ybot; y++, v += _v)
            {
                // scissor
                if (y < scissor.Top)
                    continue;
                if (y >= scissor.Bottom)
                    break;

                short xlef = (short)((y - bottomY) * yxlef + lef.X);
                short xrig = (short)((y - bottomY) * yxrig + rig.X);
                if (xlef == xrig)
                    continue;
                // scissor
                if (xrig < scissor.Left || xlef >= scissor.Right)
                    continue;

                u = 0;
                for (short x = xlef; x < xrig; x++, u += _u)
                {
                    // scissor
                    if (x < scissor.Left)
                        continue;
                    if (x >= scissor.Right)
                        break;

                    float w = 1 - u - v;

                    // 颜色线性差值
                    if (colorFlag == EColorFlag.Linear)
                    {
                        PSDefault.Default.color.R = (byte)(_lef.Color.R * w + _rig.Color.R * u + _pos.Color.R * v);
                        PSDefault.Default.color.G = (byte)(_lef.Color.G * w + _rig.Color.G * u + _pos.Color.G * v);
                        PSDefault.Default.color.B = (byte)(_lef.Color.B * w + _rig.Color.B * u + _pos.Color.B * v);
                        PSDefault.Default.color.A = (byte)(_lef.Color.A * w + _rig.Color.A * u + _pos.Color.A * v);
                    }
                    // uv线性差值
                    PSDefault.Default.coord.X = _lef.TextureCoordinate.X * w + _rig.TextureCoordinate.X * u + _pos.TextureCoordinate.X * v;
                    PSDefault.Default.coord.Y = _lef.TextureCoordinate.Y * w + _rig.TextureCoordinate.Y * u + _pos.TextureCoordinate.Y * v;
                    PSDefault.Default.main();
                    for (int s = 0; s < psIndex; s++)
                        currentPS[s].main();

                    int index = y * bufferWidth + x;
                    if (buffers[index].A == 0)
                    {
                        buffers[index].R = (byte)(PSDefault.Color.R * PSDefault.Color.A * PSDefault.R255);
                        buffers[index].G = (byte)(PSDefault.Color.G * PSDefault.Color.A * PSDefault.R255);
                        buffers[index].B = (byte)(PSDefault.Color.B * PSDefault.Color.A * PSDefault.R255);
                        buffers[index].A = 255;
                    }
                    else
                    {
                        // 根据混合模式混合画布上已有的颜色和当前像素的颜色
                        buffers[index].R = (byte)(PSDefault.Color.R * PSDefault.Color.A * PSDefault.R255 + buffers[index].R - buffers[index].R * PSDefault.Color.A * PSDefault.R255);
                        buffers[index].G = (byte)(PSDefault.Color.G * PSDefault.Color.A * PSDefault.R255 + buffers[index].G - buffers[index].G * PSDefault.Color.A * PSDefault.R255);
                        buffers[index].B = (byte)(PSDefault.Color.B * PSDefault.Color.A * PSDefault.R255 + buffers[index].B - buffers[index].B * PSDefault.Color.A * PSDefault.R255);
                    }
                    //buffers[y * bufferWidth + x] = PSDefault.Color;
                }
            }
        }
        protected internal override void Render()
        {
            screenGraphics.SetData(buffers);
            baseDevice.Begin();
            baseDevice.Draw(screenGraphics, FullGraphicsArea);
            baseDevice.End();
            baseDevice.Render();
        }
        public override void Clear()
        {
            for (int i = 0, len = buffers.Length; i < len; i++)
                buffers[i].A = 0;
        }
    }

    #endregion

    #endregion
}

#endif