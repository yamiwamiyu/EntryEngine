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
        public void ReleaseUnusableScene()
        {
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
            return ShowMainScene<T>(null);
        }
        public T ShowMainScene<T>(UIScene parent) where T : UIScene, new()
        {
            T scene = CacheScene<T>();
            ShowMainScene(scene, parent);
            return scene;
        }
        public void ShowMainScene(UIScene scene)
        {
            ShowMainScene(scene, null);
        }
        public void ShowMainScene(UIScene scene, UIScene parent)
        {
            if (scenes.Count > 0)
                scenes.ForFirstToLast((item) => item.OnPhaseEnding(scene));
            InternalShowScene(scene, parent, EState.None, true);
            phase = EPhase.Ending;
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
            return ShowDialogScene<T>(null, dialogState);
        }
        public T ShowDialogScene<T>(UIScene dialogParent, EState dialogState) where T : UIScene, new()
        {
            T scene = CacheScene<T>();
            // new scene loading
            ShowDialogScene(scene, dialogParent, dialogState);
            return (T)scene;
        }
        public void ShowDialogScene(UIScene scene)
        {
            InternalShowScene(scene, null, EState.None, false);
        }
        public void ShowDialogScene(UIScene scene, EState state)
        {
            InternalShowScene(scene, null, state, false);
        }
        public void ShowDialogScene(UIScene scene, UIScene dialogParent, EState dialogState)
        {
            InternalShowScene(scene, dialogParent, dialogState, false);
        }
        private void InternalShowScene(UIScene scene, UIScene dialogParent, EState dialogState, bool isMain)
        {
            if (scene == null)
                throw new ArgumentNullException("scene");

            if (OnShowScene != null)
                OnShowScene(scene, dialogState, isMain);

            // 已显示的场景重新进入Showing阶段
            if (scenes.Contains(scene))
            {
                scene.State = dialogState;
                scene.OnPhaseShowing(Scene);
                if (phase == EPhase.Running)
                    phase = EPhase.Showing;
                ToFront(scene);
                return;
            }

            // new scene loading
            if (isMain)
                scenes.AddFirst(scene);
            else
                scenes.AddLast(scene);
            scene.Show(this, dialogParent);
            scene.OnPhaseLoading();
            scene.State = dialogState;
            //if (phase == EPhase.Running)
                //phase = EPhase.Loading;
                phase = EPhase.Ending;
        }
        public void ShowDesktop(UIScene next, bool dispose)
        {
            foreach (var item in Dialogs)
                item.OnPhaseEnding(next);
            phase = EPhase.Ending;
        }
        public void ShowDesktopImmediately(EState state)
        {
            foreach (var scene in Dialogs)
                Close(scene, state);
        }
        internal void Close(UIScene scene, EState state)
        {
            scene.State = state;
            scene.OnPhaseEnding(null);
            // 关闭子场景
            foreach (var dialog in GetChildScene(scene))
            {
                dialog.State = state;
                dialog.OnPhaseEnding(null);
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
                scene.OnPhaseEnding(null);
                scene.OnPhaseEnded(null);
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
            UIElement.Handled = false;
            if (IPlatform.IsActive)
                InputUpdate();
            if (AUDIO != null)
                AUDIO.Update(GameTime);
            PhaseUpdate();
            SceneUpdate();
        }
        private bool NeedUpdateScene(UIScene scene)
        {
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

            if (scene.IsDrawable && scene.DrawState)
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
            foreach (var item in scenes.Where(s => s.Phase == EPhase.Ending))
                if (item.Phasing != null && !item.Phasing.IsEnd)
                    end = false;

            if (end)
                phase = EPhase.Loading;

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
                phase = EPhase.Preparing;

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

                var previous = Scene;
                UIScene next = null;
                foreach (var item in scenes)
                {
                    if (item.Phase > EPhase.Ending)
                    {
                        next = item;
                        break;
                    }
                }
                scenes.ForFirstToLast(item =>
                {
                    if (item.Phase == EPhase.Ending)
                    {
                        scenes.Remove(item);
                        item.OnPhaseEnded(next);
                        InternalCloseScene(item, ref item.State);
                    }
                    else if (item.Phase == EPhase.Prepared)
                    {
                        item.OnPhaseShowing(previous);
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
                            _event = false;
                            break;

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
            TimeSpan time = IPlatform.FrameRate;
            gameTime.ElapsedTime = time;
            gameTime.Elapsed = (float)time.TotalMilliseconds;
            gameTime.ElapsedSecond = (float)time.TotalSeconds;
        }
        public void Draw()
        {
            GRAPHICS.Clear();

            GRAPHICS.Begin(MATRIX2x3.Identity, GRAPHICS.FullGraphicsArea);

            var node = scenes.First;
            while (node != null)
            {
                var scene = node.Value;
                if (NeedDrawScene(scene))
                    scene.Draw(GRAPHICS, this);
                node = node.Next;
            }

            if (OnDrawMouse != null && IPlatform.IsMouseVisible)
                OnDrawMouse(GRAPHICS, INPUT);

            GRAPHICS.End();

            GRAPHICS.Render();
        }
        public override void Dispose()
        {
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
                    UIElement.Handled = true;
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
        /// <summary>
        /// 默认键
        /// </summary>
        public virtual int DefaultKey { get { return 0; } }

        public ComboClick GetComboClick(int key)
        {
            ComboClick combo;
            comboClicks.TryGetValue(key, out combo);
            return combo;
        }
        protected void AddMultipleClick(params int[] keys)
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
                return blick.Current <= BlickInterval / 2;
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
            OnStop(typist);
            typist = null;
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
            return typist.Font.CursorIndex(text, VECTOR2.Subtract(cursor, typist.TextArea.Location));
        }
        internal void Update(Entry e)
        {
            // && e.Input.Keyboard.Focused
            if (e.INPUT.Keyboard != null && !ImmCapturing)
            {
                shift = e.INPUT.Keyboard.Shift;
                ctrl = e.INPUT.Keyboard.Ctrl;

                if (!IsActive)
                    return;

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
                        current = input;
                        //index = current.Length;
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
        protected virtual void Copy(string copy)
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
        /// <summary>
        /// 激活设备时是否要选中所有文字
        /// </summary>
        bool ActiveSelect { get; }
        /// <summary>
        /// 计算光标位置
        /// </summary>
        FONT Font { get; }
        /// <summary>
        /// 未经处理的源文字
        /// </summary>
        string Text { get; set; }
        /// <summary>
        /// 只读则不允许输入和删除操作，不过可以复制
        /// </summary>
        bool Readonly { get; }
        /// <summary>
        /// 控制是否超出文字区域自动换行，用于计算光标位置
        /// </summary>
        bool BreakLine { get; }
        /// <summary>
        /// 用于判断回车是换行还是确定
        /// </summary>
        bool Multiple { get; }
        /// <summary>
        /// 遮挡模式时不可复制选中
        /// </summary>
        bool IsMask { get; }
        /// <summary>
        /// 文字区域，用于计算光标位置
        /// </summary>
        RECT TextArea { get; }
        /// <summary>
        /// 显示区域，用于点击取消编辑
        /// </summary>
        RECT ViewArea { get; }
        /// <summary>
        /// 限制输入最大长度
        /// </summary>
        int MaxLength { get; }
        /// <summary>
        /// 使用输入设备的控件是否激活，用于控制设备自动关闭
        /// </summary>
        bool IsActive { get; }
        /// <summary>
        /// 文字筛选，用于例如只能输入数字
        /// </summary>
        /// <param name="c">允许替换的单个字符</param>
        /// <returns>true: 被筛选掉的非法字符 / false: 合法字符</returns>
        bool Filter(ref char c);
        /// <summary>
        /// 设备关闭时回调源的处理程序
        /// </summary>
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
        protected internal virtual bool Processable(ref string file)
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
        protected internal override Content Load(string file)
        {
            return InternalLoad(Manager.IODevice.ReadByte(file));
        }
        protected internal override void LoadAsync(AsyncLoadContent async)
        {
            var read = Manager.IODevice.ReadAsync(async.File);
            if (read.IsEnd)
            {
                async.SetData(InternalLoad(read.Data));
            }
            else
            {
                Wait(async, read,
                    (result) =>
                    {
                        return InternalLoad(result.Data);
                    });
            }
        }
        protected internal abstract Content InternalLoad(byte[] bytes);
    }
    public abstract class ContentPipelineText : ContentPipelineBinary
    {
        protected internal sealed override Content InternalLoad(byte[] bytes)
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
        internal bool IsLoaded(string key)
        {
            Content value;
            if (contents.TryGetValue(key, out value))
                return !value.IsDisposed;
            return false;
        }
        internal bool IsLoaded(string key, out Content content)
        {
            content = null;
            if (contents.TryGetValue(key, out content))
                return !content.IsDisposed;
            return false;
        }
        internal void AddContent(string key, Content content)
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

        private static TEXTURE _pixel;
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
            if (texture == null)
                return null;
            if (texture.IsLinked)
                return GetDrawableTexture(((TEXTURE_Link)texture).Base);
            else
                return texture;
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
        public static Func<Type, Func<ByteRefReader, object>> Deserializer(ContentManager content, List<AsyncLoadContent> list)
        {
            return (type) =>
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
                                delay.Async = content.LoadAsync<TEXTURE>(key, (t) => delay.Base = t);
                                list.Add(delay.Async);
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
    internal sealed class TEXTURE_DELAY : TEXTURE_Link
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

    public sealed class PIECE : TEXTURE_Link
    {
        public RECT SourceRectangle;
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

            if (vertex.Source.Width > Width ||
                vertex.Source.Height > Height)
                throw new InvalidOperationException("Piece texture can't tile. SourceRectangle must be contained in piece's SourceRectangle.");

            if (!Padding.IsEmpty)
            {
                float whiteW = Padding.X + Padding.Width;
                float whiteH = Padding.Y + Padding.Height;

                vertex.Destination.Width *= SourceRectangle.Width / (SourceRectangle.Width + whiteW);
                vertex.Destination.Height *= SourceRectangle.Height / (SourceRectangle.Height + whiteH);

                vertex.Origin.X -= Padding.X;
                vertex.Origin.Y -= Padding.Y;

                vertex.Source.Width -= whiteW;
                vertex.Source.Height -= whiteH;
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
        protected internal override bool Processable(ref string file)
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
        private static float[][] _position = new float[4][];
        private static float[][] _size = new float[4][];
        private static TEXTURE _patch;
        public static COLOR NullColor = new COLOR(255, 255, 255, 0);

        static PATCH()
        {
            for (int i = 0, n = _position.Length; i < n; i++)
                _position[i] = new float[3];
            for (int i = 0, n = _size.Length; i < n; i++)
                _size[i] = new float[3];
        }

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

            VECTOR2 scale;
            scale.X = 1;
            scale.Y = 1;
            //if (source.Width < Base.Width)
            //    scale.X = Base.Width / source.Width;
            //else
            //    scale.X = 1;
            //if (source.Height < Base.Height)
            //    scale.Y = Base.Height / source.Height;
            //else
            //    scale.Y = 1;

            _position[0][1] = Anchor.X;
            _position[0][2] = Anchor.Right;
            _position[1][1] = Anchor.Y;
            _position[1][2] = Anchor.Bottom;

            _size[0][0] = Anchor.X;
            _size[0][1] = Anchor.Width;
            _size[0][2] = Base.Width - _position[0][2];
            _size[1][0] = Anchor.Y;
            _size[1][1] = Anchor.Height;
            _size[1][2] = Base.Height - _position[1][2];

            _size[2][0] = _size[0][0];
            _size[2][1] = vertex.Destination.Width - _size[0][2] - Anchor.X;
            _size[2][2] = _size[0][2];
            _size[3][0] = _size[1][0];
            _size[3][1] = vertex.Destination.Height - _size[1][2] - Anchor.Y;
            _size[3][2] = _size[1][2];

            _position[2][1] = Anchor.X;
            _position[2][2] = _position[2][1] + _size[2][1];
            _position[3][1] = Anchor.Y;
            _position[3][2] = _position[3][1] + _size[3][1];

            MATRIX2x3 matrix;
            __GRAPHICS.DrawMatrix(ref vertex.Destination, ref vertex.Source, vertex.Rotation, ref vertex.Origin, vertex.Flip, out matrix);
            graphics.BeginFromPrevious(matrix);
            vertex.Rotation = 0;
            vertex.Origin.X = 0;
            vertex.Origin.Y = 0;

            COLOR cborder = ColorBorder.A == 0 && ColorBorder.R == 255 && ColorBorder.G == 255 && ColorBorder.B == 255 ? vertex.Color : ColorBorder;
            COLOR cbody = ColorBody.A == 0 && ColorBody.R == 255 && ColorBody.G == 255 && ColorBody.B == 255 ? vertex.Color : ColorBody;
            int y, x;
            for (int i = 0; i < 3; i++)
            {
                if ((vertex.Flip & EFlip.FlipHorizontally) != EFlip.None)
                    x = 2 - i;
                else
                    x = i;

                vertex.Source.X = _position[0][x];
                vertex.Source.Width = _size[0][x];

                vertex.Destination.X = _position[2][i];
                vertex.Destination.Width = _size[2][i];

                for (int j = 0; j < 3; j++)
                {
                    if ((vertex.Flip & EFlip.FlipVertically) != EFlip.None)
                        y = 2 - j;
                    else
                        y = j;

                    vertex.Source.Y = _position[1][y];
                    vertex.Source.Height = _size[1][y];

                    vertex.Destination.Y = _position[3][j];
                    vertex.Destination.Height = _size[3][j];

                    // Unity使用了__GRAPHICS.DrawMatrix将view.X和view.Y赋值为0
                    if (x == 1 && y == 1)
                    {
                        //graphics._InternalDraw(Base, ref temp, ref patch, ref content, 0, ref zero, flip);
                        vertex.Color.R = cbody.R;
                        vertex.Color.G = cbody.G;
                        vertex.Color.B = cbody.B;
                        vertex.Color.A = cbody.A;
                        graphics.Draw(Base, ref vertex);
                    }
                    else
                    {
                        //graphics._InternalDraw(Base, ref temp, ref patch, ref c, 0, ref zero, flip);
                        vertex.Color.R = cborder.R;
                        vertex.Color.G = cborder.G;
                        vertex.Color.B = cborder.B;
                        vertex.Color.A = cborder.A;
                        graphics.Draw(Base, ref vertex);
                    }
                }
            }

            graphics.End();
            return true;
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
        public bool AutoPlay = true;
        private bool updated;

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
            if (updated)
                return IsSequenceOver;

            bool over = false;
            var frame = Frame;
            if (elapsedTime >= frame.Interval)
            {
                float result = elapsedTime - frame.Interval;
                // 这里不一定只换一帧，但只做1帧解决
                over = NextFrame();
                //this.elapsedTime = 0;
                this.elapsedTime = result;
            }
            this.elapsedTime += elapsed;

            this.updated = true;
            return over;
        }
        public override void Update(GameTime time)
        {
            Update(time.Elapsed);
        }
        protected internal override bool Draw(GRAPHICS graphics, ref SpriteVertex vertex)
        {
            float elapsed = EntryService.Instance.GameTime.Elapsed;
            if (AutoPlay)
            {
                Update(elapsed);
            }
            updated = false;
            var frame = Frame;
            if (frame != null)
            {
                vertex.Origin.X += frame.PivotX;
                vertex.Origin.Y += frame.PivotY;
                base.Draw(graphics, ref vertex);
            }
            if (AutoPlay)
            {
                Update(elapsed);
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
        public float Interval;
        public float PivotX;
        public float PivotY;
    }
    public class PipelineAnimation : ContentPipeline
    {
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
        protected internal abstract void Draw(GRAPHICS spriteBatch, string text, VECTOR2 location, COLOR color);

        public static Func<Type, Func<ByteRefReader, object>> Deserializer(ContentManager content, List<AsyncLoadContent> list)
        {
            return (type) =>
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
                            _width = alphabetWidth;
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
        protected internal override void Draw(EntryEngine.GRAPHICS spriteBatch, string text, EntryEngine.VECTOR2 location, EntryEngine.COLOR color)
        {
            Base.Draw(spriteBatch, text, location, color);
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
    public abstract class FontTexture : FONT
    {
        public const ushort BUFFER_SIZE = 256;

        public class Buffer
        {
            public byte Index;
            public ushort X;
            public ushort Y;
            public byte W;
            public byte H;
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

        protected internal override float CharWidth(char c)
        {
            return cache.Maps[c].W + Spacing.X;
        }
        protected virtual Buffer GetBuffer(char c)
        {
            Buffer buffer;
            cache.Maps.TryGetValue(c, out buffer);
            return buffer;
        }
        protected internal override void Draw(GRAPHICS spriteBatch, string text, VECTOR2 location, COLOR color)
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
                    area.Y += height + Spacing.Y;
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
                    area.Width = buffer.W;
                    area.Height = buffer.H;
                    spriteBatch.BaseDraw(cache.Textures[buffer.Index], area.X, area.Y, area.Width, area.Height, false, buffer.X, buffer.Y, buffer.W, buffer.H, true, color.R, color.G, color.B, color.A, 0, 0, 0, EFlip.None);
                    //spriteBatch.Draw(texture, area, uv, color);
                    area.X += buffer.W + Spacing.X;
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
        protected internal override void Draw(GRAPHICS spriteBatch, string text, VECTOR2 location, COLOR color)
        {
            bool scaled = scale != 1;
            if (scaled)
            {
                spriteBatch.BeginFromPrevious(MATRIX2x3.CreateScale(scale, scale) * MATRIX2x3.CreateTranslation(location.X, location.Y));
                location.X = 0;
                location.Y = 0;
            }

            base.Draw(spriteBatch, text, location, color);

            if (scaled)
                spriteBatch.End();
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
                reader.Read(out buffer.X);
                reader.Read(out buffer.Y);
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
                        reader.Read(out buffer.X);
                        reader.Read(out buffer.Y);
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
    public abstract class FontDynamic : FontStatic
    {
        /// <summary>字体大小相差不大时，采用静态字体缩放</summary>
        public static byte StaticStep = 8;
        private static Dictionary<int, FontDynamic> fonts = new Dictionary<int, FontDynamic>();

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
            foreach (char c in text)
                GetBuffer(c);
        }
        public override VECTOR2 MeasureString(string text)
        {
            RequestString(text);
            return base.MeasureString(text);
        }
        protected override Buffer GetBuffer(char c)
        {
            Buffer buffer = base.GetBuffer(c);
            if (buffer != null)
                return buffer;
            if (c == LINE_BREAK)
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
                    v += (byte)lineHeight;
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
            buffer.X = u;
            buffer.Y = v;
            buffer.W = width;
            buffer.H = height;
            cache.Maps.Add(c, buffer);

            RECT uv = new RECT(u, v, width, height);

            u += width;

            var colors = DrawChar(c, ref uv);
            if (colors != null)
                texture.SetData(colors, uv);

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
        protected abstract COLOR[] DrawChar(char c, ref RECT uv);
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


    [Flags]
    public enum EFlip : byte
    {
        None = 0,
        FlipHorizontally = 1,
        FlipVertically = 2,
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
    /// <summary>
    /// 屏幕适配
    /// </summary>
    public enum EViewport
    {
        /// <summary>
        /// 画布尺寸保持与屏幕尺寸一样
        /// </summary>
        None,
        /// <summary>
        /// 保持画布分辨率比例拉升自动适配屏幕
        /// </summary>
        Adapt,
        /// <summary>
        /// 拉伸画布分辨率到屏幕分辨率
        /// </summary>
        Strength,
        /// <summary>
        /// 画布尺寸始终保持不变，当屏幕尺寸大于画布尺寸时，画布居中
        /// </summary>
        Keep
    }
    [ADevice]
    public abstract class GRAPHICS
    {
        public const int MAX_BATCH_COUNT = 2048;
        public static readonly RECT NullSource = new RECT(float.NaN, 0, 0, 0);

        protected class RenderState
        {
            public MATRIX2x3 Transform;
            public RECT Graphics;
            public SHADER Shader;

            public RenderState()
            {
            }
            public RenderState(MATRIX2x3 transform, RECT graphics)
            {
                this.Transform = transform;
                this.Graphics = graphics;
            }
        }

        private EViewport viewportMode = EViewport.Adapt;
        private MATRIX2x3 view = MATRIX2x3.Identity;
        private MATRIX2x3 graphicsToScreen;
        private MATRIX2x3 screenToGraphics;
        private RECT graphicsViewport = new RECT(0, 0, 1280, 720);
        private RenderState nullRenderState;
        private PoolStack<RenderState> renderStates = new PoolStack<RenderState>();
        public COLOR DefaultColor = COLOR.White;
        public readonly float[] XCornerOffsets = { 0, 1, 1, 0 };
        public readonly float[] YCornerOffsets = { 0, 0, 1, 1 };
        private TEXTURE currentTexture;
        private SpriteVertex[] spriteQueue = new SpriteVertex[128];
        private int spriteQueueCount;
        private TextureVertex[] outputVertices = new TextureVertex[MAX_BATCH_COUNT * 4];
        protected short[] indices;
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
            get { return CurrentRenderState.Transform; }
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

        protected GRAPHICS()
        {
            nullRenderState = new RenderState(MATRIX2x3.Identity, graphicsViewport);
            indices = CreateIndexData();
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
            RenderState renderState;
            if (!renderStates.Allot(out renderState))
            {
                renderState = new RenderState();
                renderStates.Push(renderState);
            }
            renderState.Transform = matrix;
            renderState.Graphics = graphics;
            renderState.Shader = shader;
            Begin(renderState);
        }
        private void Begin(RenderState state)
        {
            Flush();
            MATRIX2x3 result = state.Transform * view;
            RECT scissor = state.Graphics;
            //scissor.Offset(graphicsViewport.X, graphicsViewport.Y);
            //RECT.Intersect(ref scissor, ref graphicsViewport, out scissor);
            InternalBegin(ref result, ref scissor, state.Shader);
        }
        protected abstract void InternalBegin(ref MATRIX2x3 matrix, ref RECT graphics, SHADER shader);
        public void Begin()
        {
            RenderState rs = CurrentRenderState;
            Begin(ref rs.Transform, ref rs.Graphics, null);
        }
        public void Begin(MATRIX2x3 transform)
        {
            Begin(ref transform, ref CurrentRenderState.Graphics, null);
        }
        public void Begin(RECT graphics)
        {
            Begin(ref CurrentRenderState.Transform, ref graphics, null);
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
            Begin(ref rs.Transform, ref rs.Graphics, shader);
        }
        public void BeginFromPrevious(MATRIX2x3 matrix)
        {
            matrix *= CurrentRenderState.Transform;
            Begin(ref matrix, ref CurrentRenderState.Graphics, null);
        }
        public void BeginFromPrevious(RECT rect)
        {
            RECT _preRect = CurrentRenderState.Graphics;
            rect.X += _preRect.X;
            rect.Y += _preRect.Y;
            RECT.Intersect(ref rect, ref _preRect, out rect);
            rect.X -= _preRect.X;
            rect.Y -= _preRect.Y;
            Begin(ref CurrentRenderState.Transform, ref rect, null);
        }
        /// <summary>在之前的矩阵的基础上开始绘制</summary>
        public void BeginFromPrevious(MATRIX2x3 matrix, RECT rect)
        {
            matrix *= CurrentRenderState.Transform;
            RECT _preRect = CurrentRenderState.Graphics;
            rect.X += _preRect.X;
            rect.Y += _preRect.Y;
            RECT.Intersect(ref rect, ref _preRect, out rect);
            rect.X -= _preRect.X;
            rect.Y -= _preRect.Y;
            Begin(ref matrix, ref rect, null);
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
            Ending(state);
            renderStates.Pop();

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
        public void Draw(TEXTURE texture, RECT rect, RECT source)
        {
            BaseDraw(texture, rect.X, rect.Y, rect.Width, rect.Height, false, source.X, source.Y, source.Width, source.Height, false, 0, 0, 0, 0, 0, 0, 0, EFlip.None);
        }
        public void Draw(TEXTURE texture, RECT rect, RECT source, COLOR color)
        {
            BaseDraw(texture, rect.X, rect.Y, rect.Width, rect.Height, false, source.X, source.Y, source.Width, source.Height, true, color.R, color.G, color.B, color.A, 0, 0, 0, EFlip.None);
        }
        public void Draw(TEXTURE texture, RECT rect, float rotation, float originX, float originY)
        {
            BaseDraw(texture, rect.X, rect.Y, rect.Width, rect.Height, false, float.NaN, 0, 0, 0, false, 0, 0, 0, 0, rotation, originX, originY, EFlip.None);
        }
        public void Draw(TEXTURE texture, RECT rect, float rotation, VECTOR2 origin, EFlip flip)
        {
            BaseDraw(texture, rect.X, rect.Y, rect.Width, rect.Height, false, float.NaN, 0, 0, 0, false, 0, 0, 0, 0, rotation, origin.X, origin.Y, flip);
        }
        public void Draw(TEXTURE texture, RECT rect, RECT source, COLOR color, float rotation, float originX, float originY, EFlip flip)
        {
            BaseDraw(texture, rect.X, rect.Y, rect.Width, rect.Height, false, source.X, source.Y, source.Width, source.Height, true, color.R, color.G, color.B, color.A, rotation, originX, originY, flip);
        }
        public void Draw(TEXTURE texture, VECTOR2 location)
        {
            BaseDraw(texture, location.X, location.Y, 1, 1, true, float.NaN, 0, 0, 0, false, 0, 0, 0, 0, 0, 0, 0, EFlip.None);
        }
        public void Draw(TEXTURE texture, VECTOR2 location, float scaleX, float scaleY)
        {
            BaseDraw(texture, location.X, location.Y, scaleX, scaleY, true, float.NaN, 0, 0, 0, false, 0, 0, 0, 0, 0, 0, 0, EFlip.None);
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
        public void Draw(TEXTURE texture, VECTOR2 location, float rotation, VECTOR2 origin, float scaleX, float scaleY)
        {
            BaseDraw(texture, location.X, location.Y, scaleX, scaleY, true, float.NaN, 0, 0, 0, false, 0, 0, 0, 0, rotation, origin.X, origin.Y, EFlip.None);
        }
        public void Draw(TEXTURE texture, VECTOR2 location, COLOR color, float rotation, float originX, float originY, float scaleX, float scaleY)
        {
            BaseDraw(texture, location.X, location.Y, scaleX, scaleY, true, float.NaN, 0, 0, 0, true, color.R, color.G, color.B, color.A, rotation, originX, originY, EFlip.None);
        }
        public void Draw(TEXTURE texture, VECTOR2 location, RECT source, COLOR color, float rotation, VECTOR2 origin, VECTOR2 scale, EFlip flip)
        {
            BaseDraw(texture, location.X, location.Y, scale.X, scale.Y, true, source.X, source.Y, source.Width, source.Height, true, color.R, color.G, color.B, color.A, rotation, origin.X, origin.Y, flip);
        }
        public virtual void BaseDraw(TEXTURE texture,
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

            if (this.spriteQueueCount >= this.spriteQueue.Length)
            {
                Array.Resize<SpriteVertex>(ref this.spriteQueue, this.spriteQueue.Length * 2);
                indices = CreateIndexData();
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

            if (Culling)
            {
                MATRIX2x3 matrix = CurrentTransform;
                RECT viewport = CurrentGraphics;

                if (vertex.Rotation == 0 && vertex.Flip == EFlip.None)
                {
                    if (vertex.Origin.X != 0)
                    {
                        vertex.Destination.X -= vertex.Destination.Width * (vertex.Origin.X / vertex.Source.Width);
                        vertex.Origin.X = 0;
                    }
                    if (vertex.Origin.Y != 0)
                    {
                        vertex.Destination.Y -= vertex.Destination.Height * (vertex.Origin.Y / vertex.Source.Height);
                        vertex.Origin.Y = 0;
                    }

                    bool draw = true;
                    if (matrix.IsIdentity())
                    {
                        draw = viewport.Intersects(vertex.Destination);
                    }
                    else
                    {
                        RECT result;
                        RECT.CreateBoundingBox(ref vertex.Destination, ref matrix, out result);
                        draw = viewport.Intersects(result);
                    }
                    if (draw)
                    {
                        InternalDraw(texture, ref vertex);
                        //InternalDrawFast(texture, ref vertex.Destination, ref vertex.Source, ref vertex.Color);
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

                    if (viewport.Intersects(result))
                    {
                        InternalDraw(texture, ref vertex);
                    }
                }
            }
            else
            {
                InternalDraw(texture, ref vertex);
            }
        }
        protected virtual void InternalDraw(TEXTURE texture, ref SpriteVertex vertex)
        {
            if (vertex.Source.Width == 0f || vertex.Source.Height == 0f)
                return;

            if (texture != currentTexture)
            {
                if (texture != null && currentTexture != null)
                {
                    this.Flush();
                }
                this.currentTexture = texture;
            }

            //spriteQueue[spriteQueueCount++] = vertex;
            int index = spriteQueueCount;
            spriteQueue[index].Destination.X = vertex.Destination.X;
            spriteQueue[index].Destination.Y = vertex.Destination.Y;
            spriteQueue[index].Destination.Width = vertex.Destination.Width;
            spriteQueue[index].Destination.Height = vertex.Destination.Height;
            spriteQueue[index].Source.X = vertex.Source.X;
            spriteQueue[index].Source.Y = vertex.Source.Y;
            spriteQueue[index].Source.Width = vertex.Source.Width;
            spriteQueue[index].Source.Height = vertex.Source.Height;
            spriteQueue[index].Color.R = vertex.Color.R;
            spriteQueue[index].Color.G = vertex.Color.G;
            spriteQueue[index].Color.B = vertex.Color.B;
            spriteQueue[index].Color.A = vertex.Color.A;
            spriteQueue[index].Rotation = vertex.Rotation;
            spriteQueue[index].Origin.X = vertex.Origin.X;
            spriteQueue[index].Origin.Y = vertex.Origin.Y;
            spriteQueue[index].Flip = vertex.Flip;
            spriteQueueCount++;
        }
        public void Draw(FONT font, string text, VECTOR2 location, COLOR color)
        {
            Draw(font, text, ref location, ref color);
        }
        public void Draw(FONT font, string text, RECT bound, COLOR color, UI.EPivot alignment)
        {
            VECTOR2 location = UI.UIElement.TextAlign(bound, font.MeasureString(text), alignment);
            Draw(font, text, ref location, ref color);
        }
        protected virtual void Draw(FONT font, string text, ref VECTOR2 location, ref COLOR color)
        {
            font.Draw(this, text, location, color);
        }
        protected internal virtual void Render()
        {
        }

        private void Flush()
        {
            if (currentTexture == null)
                return;

            if (spriteQueueCount == 0)
            {
                currentTexture = null;
                return;
            }

            DrawPrimitivesBegin(currentTexture);

            int offset = 0;
            int count = spriteQueueCount;
            while (count > 0)
            {
                // 批绘制顶点缓存最多只有8192个，所以最多一次处理2048个Sprite
                int num = count;
                if (num > MAX_BATCH_COUNT)
                    num = MAX_BATCH_COUNT;

                for (int i = 0; i < num; i++)
                    InputVertexToOutputVertex(ref spriteQueue[offset + i], i * 4);

                DrawPrimitives(outputVertices, 0, num * 4, indices, 0, num * 2);
                offset += num;
                count -= num;
            }

            DrawPrimitivesEnd();
            spriteQueueCount = 0;
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

            float u;
            //if (vertex.Source.Width == 0f)
            //    u = vertex.Origin.X * 2E+32f;
            //else
            u = vertex.Origin.X / vertex.Source.Width;
            float v;
            //if (vertex.Source.Height == 0f)
            //    v = vertex.Origin.Y * 2E+32f;
            //else
            v = vertex.Origin.Y / vertex.Source.Height;

            for (int i = 0; i < 4; i++)
            {
                float xc = XCornerOffsets[i];
                float yc = YCornerOffsets[i];
                float xOffset = (xc - u) * vertex.Destination.Width;
                float yOffset = (yc - v) * vertex.Destination.Height;
                float x = vertex.Destination.X + xOffset * cos - yOffset * sin;
                float y = vertex.Destination.Y + xOffset * sin + yOffset * cos;
                if ((vertex.Flip & EFlip.FlipHorizontally) != EFlip.None)
                    xc = 1f - xc;
                if ((vertex.Flip & EFlip.FlipVertically) != EFlip.None)
                    yc = 1f - yc;

                int index = outputIndex + i;
                outputVertices[index].Position.X = x;
                outputVertices[index].Position.Y = y;
                outputVertices[index].TextureCoordinate.X = vertex.Source.X + xc * vertex.Source.Width;
                outputVertices[index].TextureCoordinate.Y = vertex.Source.Y + yc * vertex.Source.Height;
                outputVertices[index].Color.R = vertex.Color.R;
                outputVertices[index].Color.G = vertex.Color.G;
                outputVertices[index].Color.B = vertex.Color.B;
                outputVertices[index].Color.A = vertex.Color.A;
                OutputVertex(ref outputVertices[index]);
            }
        }
        /// <summary>三角形绘制</summary>
        public void DrawPrimitives(TEXTURE texture, TextureVertex[] vertices, int offset, int count, short[] indexes, int indexOffset, int primitiveCount)
        {
            bool changeTex = currentTexture != texture;
            Flush();
            DrawPrimitivesBegin(changeTex ? texture : null);
            DrawPrimitives(vertices, offset, count, indexes, indexOffset, primitiveCount);
            DrawPrimitivesEnd();
            currentTexture = texture;
        }
        protected abstract void DrawPrimitives(TextureVertex[] vertices, int offset, int count, short[] indexes, int indexOffset, int primitiveCount);
        protected abstract void DrawPrimitivesBegin(TEXTURE texture);
        protected virtual void DrawPrimitivesEnd()
        {
        }
        protected virtual void OutputVertex(ref TextureVertex output)
        {
        }

        protected virtual short[] CreateIndexData()
        {
            short[] array = new short[MAX_BATCH_COUNT * 6];
            for (int i = 0, m = MAX_BATCH_COUNT; i < m; i++)
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
        /// <summary>每6个为一组按的索引0,1,2,0,2,3</summary>
        public static short[] CreateIndexData(int vertexCount)
        {
            short[] array = new short[vertexCount / 4 * 6];
            for (int i = 0, m = vertexCount / 4; i < m; i++)
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


    // cpu render
    //public class TextureCPU : TEXTURE
    //{
    //    private int width;
    //    private int height;
    //    private COLOR[] datas;

    //    public override int Width
    //    {
    //        get { return width; }
    //    }
    //    public override int Height
    //    {
    //        get { return height; }
    //    }

    //    public TextureCPU(TEXTURE texture)
    //    {
    //        this.width = texture.Width;
    //        this.height = texture.Height;
    //        this.datas = texture.GetData();
    //    }

    //    public override COLOR[] GetData(RECT area)
    //    {
    //        if (area.X == 0 && area.Y == 0 && area.Width == width && area.Height == height)
    //            return datas;
    //        return Utility.GetArray(datas, area, Width);
    //    }
    //    public override void SetData(COLOR[] buffer, RECT area)
    //    {
    //        Utility.SetArray(buffer, datas, area, width, 0);
    //    }
    //    protected internal override void InternalDispose()
    //    {
    //        this.datas = null;
    //    }
    //}
    //public struct VertexInput
    //{
    //    public VECTOR2 Position;
    //    public COLOR Color;
    //    public VECTOR2 TextureCoordinate;
    //}
    //public struct VertexOutput
    //{
    //    public short X;
    //    public short Y;
    //    public COLOR Color;
    //    public ushort U;
    //    public ushort V;
    //}
    //public abstract class ShaderBase : IDisposable
    //{
    //    public bool Enable = true;
    //    public virtual void Dispose()
    //    {
    //    }
    //}
    //public abstract class ShaderVertex : ShaderBase
    //{
    //    protected internal abstract unsafe void VS(ref VertexInput[] vertex);
    //}
    //public abstract class ShaderPixel : ShaderBase
    //{
    //    protected internal static TEXTURE Sampler
    //    {
    //        get;
    //        internal set;
    //    }

    //    protected internal abstract unsafe void PS(ref VertexOutput* vertex);
    //}
    ///// <summary>
    ///// Error
    ///// warning 1. Flip数组越界
    ///// tick 2. 旋转是索引越界
    ///// 应该是float精度问题：左端点x=0.9，右端点x=10.1，目标光栅化应该是1-10，直接(int)则为0-10；若改为(int)+0.5，那么0.1和9.6期望为0-9，可是变为0-10，导致越界
    ///// 2.5. 旋转不越界，不过两个三角形接缝处在特定情况会出现裂缝
    ///// </summary>
    //[Code(ECode.ToBeContinue)]
    //public sealed class GraphicsDeviceCPU : GRAPHICS
    //{
    //    private enum EColorFlag
    //    {
    //        /// <summary>
    //        /// 线性计算每个像素点的值
    //        /// </summary>
    //        Linear,
    //        /// <summary>
    //        /// 固定计算像素点的值
    //        /// </summary>
    //        Fixed,
    //        /// <summary>
    //        /// 不计算颜色的值
    //        /// </summary>
    //        Ignore,
    //    }

    //    private static int[] xCornerOffsets = { 0, 1, 0, 1 };
    //    private static int[] yCornerOffsets = { 0, 0, 1, 1 };
    //    private static ShaderVertex[] currentVS = new ShaderVertex[4];
    //    private static ShaderPixel[] currentPS = new ShaderPixel[4];
    //    private static int vsIndex;
    //    private static int psIndex;
    //    private static COLOR[] datas;
    //    private static int textureWidth;

    //    private GRAPHICS baseDevice;
    //    private SpriteVertex[] sprites = new SpriteVertex[4];
    //    private TEXTURE[] textures = new TEXTURE[4];
    //    private Stack<int> spriteCount = new Stack<int>();
    //    private int spriteQueue;
    //    private COLOR[] buffers;
    //    private ushort bufferWidth;
    //    private ushort bufferHeight;
    //    private TEXTURE screenGraphics;
    //    private VertexInput[] vertices = new VertexInput[4];
    //    private VertexOutput[] outputs = new VertexOutput[1280 * 720];
    //    private MATRIX2x3 transform;
    //    private RECT scissor;

    //    public override bool IsFullScreen
    //    {
    //        get { return baseDevice.IsFullScreen; }
    //        set { baseDevice.IsFullScreen = value; }
    //    }
    //    protected override VECTOR2 InternalScreenSize
    //    {
    //        get { return baseDevice.ScreenSize; }
    //        set { baseDevice.ScreenSize = value; }
    //    }

    //    public GraphicsDeviceCPU(GRAPHICS device)
    //    {
    //        if (device == null)
    //            throw new ArgumentNullException("GraphicsDevice");
    //        this.baseDevice = device;
    //        this.GraphicsSize = device.GraphicsSize;
    //    }

    //    private void SetVS()
    //    {
    //        vsIndex = 0;
    //        foreach (var shader in Shaders)
    //        {
    //            foreach (var vs in shader.VS)
    //            {
    //                if (vs.Enable)
    //                {
    //                    if (currentVS.Length == vsIndex)
    //                    {
    //                        Array.Resize(ref currentVS, vsIndex * 2);
    //                    }
    //                    currentVS[vsIndex++] = vs;
    //                }
    //            }
    //        }
    //    }
    //    private void SetPS()
    //    {
    //        psIndex = 0;
    //        foreach (var shader in Shaders)
    //        {
    //            foreach (var ps in shader.PS)
    //            {
    //                if (ps.Enable)
    //                {
    //                    if (currentPS.Length == vsIndex)
    //                    {
    //                        Array.Resize(ref currentPS, psIndex * 2);
    //                    }
    //                    currentPS[psIndex++] = ps;
    //                }
    //            }
    //        }
    //    }
    //    protected override void SetViewport(MATRIX2x3 view, RECT viewport)
    //    {
    //        if (baseDevice.ViewportMode != this.ViewportMode)
    //            baseDevice.ViewportMode = this.ViewportMode;
    //        if (baseDevice.GraphicsSize != this.GraphicsSize)
    //            baseDevice.GraphicsSize = this.GraphicsSize;
    //        if (baseDevice.ScreenSize != this.ScreenSize)
    //            baseDevice.ScreenSize = this.ScreenSize;
    //        viewport = AreaToScreen(viewport);
    //        bufferWidth = (ushort)viewport.Width;
    //        bufferHeight = (ushort)viewport.Height;
    //        buffers = new COLOR[bufferHeight * bufferWidth];
    //        screenGraphics = Entry.Instance.NewTEXTURE(bufferWidth, bufferHeight);
    //        view.M31 = 0;
    //        view.M32 = 0;
    //    }
    //    protected override void InternalDrawFast(TEXTURE texture, ref RECT rect, ref RECT source, ref COLOR color)
    //    {
    //        InternalDraw(texture, ref rect, ref source, ref color, 0, ref nullPivot, EFlip.None);
    //    }
    //    protected unsafe override void InternalDraw(TEXTURE texture, ref RECT rect, ref RECT source, ref COLOR color, float rotation, ref VECTOR2 origin, EFlip flip)
    //    {
    //        if (texture == null)
    //            throw new ArgumentNullException("texture");
    //        if (!HasRenderTarget)
    //            throw new InvalidOperationException("Begin must be called before draw.");

    //        if (spriteQueue == sprites.Length)
    //            Array.Resize(ref sprites, spriteQueue * 2);

    //        fixed (SpriteVertex* ptr = &sprites[spriteQueue])
    //        {
    //            ptr->Destination.X = rect.X;
    //            ptr->Destination.Y = rect.Y;
    //            ptr->Destination.Width = rect.Width;
    //            ptr->Destination.Height = rect.Height;
    //            ptr->Source.X = source.X;
    //            ptr->Source.Y = source.Y;
    //            ptr->Source.Width = source.Width;
    //            ptr->Source.Height = source.Height;
    //            ptr->Color.R = color.R;
    //            ptr->Color.G = color.G;
    //            ptr->Color.B = color.B;
    //            ptr->Color.A = color.A;
    //            ptr->Rotation = rotation;
    //            ptr->Origin.X = origin.X;
    //            ptr->Origin.Y = origin.Y;
    //            ptr->Flip = flip;
    //        }

    //        if (textures.Length != sprites.Length)
    //            Array.Resize(ref textures, sprites.Length);

    //        textures[spriteQueue] = texture;
    //        spriteQueue++;
    //    }
    //    protected override void InternalBegin(ref MATRIX2x3 matrix, ref RECT graphics, SHADER shader)
    //    {
    //        spriteCount.Push(spriteQueue);
    //        this.transform = matrix;
    //        this.scissor = graphics;
    //    }
    //    protected override unsafe void Ending(GRAPHICS.RenderState render)
    //    {
    //        TIMER timer = TIMER.StartNew();
    //        int start = spriteCount.Pop();

    //        RECT scissor = this.scissor.ToLocation();
    //        SetVS();
    //        SetPS();
    //        bool isIdentity = transform.IsIdentity();

    //        fixed (SpriteVertex* ptrsv = &sprites[start])
    //        {
    //            SpriteVertex* ptr = ptrsv;
    //            // draw the sprites
    //            for (int i = start; i < spriteQueue; i++)
    //            {
    //                TEXTURE texture = textures[i];
    //                datas = texture.GetData();
    //                textureWidth = texture.Width;
    //                ShaderPixel.Sampler = textures[i];

    //                #region calc vertex infomation
    //                fixed (VertexInput* vertexptr = &vertices[0])
    //                {
    //                    float rotationX;
    //                    float rotationY;
    //                    if (ptr->Rotation != 0f)
    //                    {
    //                        rotationX = (float)_MATH.Cos(ptr->Rotation);
    //                        rotationY = (float)_MATH.Sin(ptr->Rotation);
    //                    }
    //                    else
    //                    {
    //                        rotationX = 1f;
    //                        rotationY = 0f;
    //                    }
    //                    float pivotOffsetX;
    //                    float pivotOffsetY;
    //                    if (ptr->Origin.X == ptr->Source.Width)
    //                        pivotOffsetX = 1;
    //                    else
    //                        pivotOffsetX = ptr->Origin.X / ptr->Source.Width;
    //                    if (ptr->Origin.Y == ptr->Source.Height)
    //                        pivotOffsetY = 1;
    //                    else
    //                        pivotOffsetY = ptr->Origin.Y / ptr->Source.Height;
    //                    VertexInput* ptr2 = vertexptr;
    //                    for (int j = 0; j < 4; j++)
    //                    {
    //                        float x = xCornerOffsets[j];
    //                        float y = yCornerOffsets[j];
    //                        float offsetX = (x - pivotOffsetX) * ptr->Destination.Width;
    //                        float offsetY = (y - pivotOffsetY) * ptr->Destination.Height;
    //                        ptr2->Position.X = ptr->Destination.X + offsetX * rotationX - offsetY * rotationY;
    //                        ptr2->Position.Y = ptr->Destination.Y + offsetX * rotationY + offsetY * rotationX;
    //                        if ((ptr->Flip & EFlip.FlipHorizontally) != EFlip.None)
    //                            x = 1 - x;
    //                        if ((ptr->Flip & EFlip.FlipVertically) != EFlip.None)
    //                            y = 1 - y;
    //                        // fixed: 数组越界 width - 1, height - 1
    //                        ptr2->TextureCoordinate.X = ptr->Source.X + x * (ptr->Source.Width - 1);
    //                        ptr2->TextureCoordinate.Y = ptr->Source.Y + y * (ptr->Source.Height - 1);
    //                        ptr2->Color = ptr->Color;
    //                        ptr2++;
    //                    }
    //                }
    //                #endregion

    //                #region vertex infomation
    //                // vertex shader
    //                if (vsIndex > 0)
    //                {
    //                    for (int j = 0; j < vsIndex; j++)
    //                    {
    //                        currentVS[j].VS(ref vertices);
    //                    }
    //                }
    //                int count = vertices.Length;
    //                if (isIdentity)
    //                {
    //                    // transform the vertex
    //                    fixed (VertexInput* vertexptr = &vertices[0])
    //                    {
    //                        VertexInput* ptr2 = vertexptr;
    //                        for (int j = 0; j < count; j++)
    //                        {
    //                            VECTOR2.Transform(ref ptr2->Position, ref transform);
    //                            ptr2++;
    //                        }
    //                    }
    //                }
    //                #endregion

    //                #region rasterize, clip, pixel shader and render to buffer
    //                // rasterize
    //                const int COUNT = 3;

    //                if (vertices.Length < COUNT)
    //                    throw new ArgumentOutOfRangeException("Trangle rasterizer need the vertex count bigger than 3.");

    //                int index = 0;
    //                int end = vertices.Length - COUNT;
    //                count = 0;
    //                while (index <= end)
    //                {
    //                    fixed (VertexInput* temp = &vertices[index++])
    //                    {
    //                        VertexInput* ptr1 = temp;
    //                        VertexInput* ptr2 = temp + 1;
    //                        VertexInput* ptr3 = temp + 2;

    //                        EColorFlag flag;
    //                        if (psIndex != 0)
    //                        {
    //                            flag = EColorFlag.Ignore;
    //                        }
    //                        else
    //                        {
    //                            if (ptr1->Color.Equals(ref ptr2->Color) & ptr1->Color.Equals(ref ptr3->Color))
    //                            {
    //                                if (ptr1->Color.A == 0)
    //                                    continue;
    //                                if (ptr1->Color.R == byte.MaxValue
    //                                    && ptr1->Color.G == byte.MaxValue
    //                                    && ptr1->Color.B == byte.MaxValue
    //                                    && ptr1->Color.A == byte.MaxValue)
    //                                {
    //                                    flag = EColorFlag.Ignore;
    //                                }
    //                                else
    //                                {
    //                                    flag = EColorFlag.Fixed;
    //                                }
    //                            }
    //                            else
    //                            {
    //                                flag = EColorFlag.Linear;
    //                            }
    //                        }

    //                        TriangleVerticesSort(ref ptr1, ref ptr2);
    //                        TriangleVerticesSort(ref ptr2, ref ptr3);
    //                        TriangleVerticesSort(ref ptr1, ref ptr2);

    //                        /*
    //                         * 优化
    //                         * 1. u,v不要每个点都计算，计算x起始和x结束两点的u,v，中间的点按步增长
    //                         * tick 2. 三个顶点颜色值一样时，不计算uv，直接对颜色进行赋值；当Alpha=0时，直接跳过整个光栅化
    //                         * tick 3. 裁切时，ybottom < scissor.y && ytop >= scissor.height && xlef < scissor.x && xright >= scissor.width时直接跳过
    //                         * 4. 裁切时，ytop < scissor.y时，ytop = scissor.y；ybottom > scissor.height时，break；x同样
    //                         */
    //                        if (ptr1->Position.Y == ptr2->Position.Y)
    //                        {
    //                            DrawTriangle(ptr3, ptr1, ptr2, flag, ref scissor);
    //                        }
    //                        else if (ptr2->Position.Y == ptr3->Position.Y)
    //                        {
    //                            DrawTriangle(ptr1, ptr2, ptr3, flag, ref scissor);
    //                        }
    //                        else
    //                        {
    //                            VertexInput mid;
    //                            VertexInput* ptr4 = &mid;
    //                            // 计算1->3直线y=2.y的x值
    //                            ptr4->Position.Y = ptr2->Position.Y;
    //                            ptr4->Position.X = (ptr2->Position.Y - ptr1->Position.Y) * (ptr3->Position.X - ptr1->Position.X) / (ptr3->Position.Y - ptr1->Position.Y) + ptr1->Position.X;
    //                            //ptr4->Position.ToRoundInt();

    //                            // 计算uvw
    //                            float u, v, w;
    //                            VECTOR2.Barycentric(ref ptr4->Position,
    //                                ref ptr1->Position,
    //                                ref ptr2->Position,
    //                                ref ptr3->Position,
    //                                out u, out v);
    //                            w = 1 - u - v;

    //                            // 根据uv计算Color
    //                            if (flag != EColorFlag.Linear)
    //                            {
    //                                ptr4->Color = ptr1->Color;
    //                            }
    //                            else
    //                            {
    //                                ptr4->Color.R = (byte)(ptr1->Color.R * w + ptr2->Color.R * u + ptr3->Color.R * v);
    //                                ptr4->Color.G = (byte)(ptr1->Color.G * w + ptr2->Color.G * u + ptr3->Color.G * v);
    //                                ptr4->Color.B = (byte)(ptr1->Color.B * w + ptr2->Color.B * u + ptr3->Color.B * v);
    //                                ptr4->Color.A = (byte)(ptr1->Color.A * w + ptr2->Color.A * u + ptr3->Color.A * v);
    //                            }
    //                            // 根据uv计算TextureCoordinate
    //                            ptr4->TextureCoordinate.X = ptr1->TextureCoordinate.X * w + ptr2->TextureCoordinate.X * u + ptr3->TextureCoordinate.X * v;
    //                            ptr4->TextureCoordinate.Y = ptr1->TextureCoordinate.Y * w + ptr2->TextureCoordinate.Y * u + ptr3->TextureCoordinate.Y * v;
    //                            ptr4->TextureCoordinate.X = (int)ptr4->TextureCoordinate.X;
    //                            ptr4->TextureCoordinate.Y = (int)ptr4->TextureCoordinate.Y;

    //                            // 将左右点排序后分别绘制上三角和下三角
    //                            if (ptr4->Position.X > ptr2->Position.X)
    //                            {
    //                                VertexInput* swap = ptr2;
    //                                ptr2 = ptr4;
    //                                ptr4 = swap;
    //                            }
    //                            DrawTriangle(ptr1, ptr4, ptr2, flag, ref scissor);
    //                            DrawTriangle(ptr3, ptr4, ptr2, flag, ref scissor);
    //                        }
    //                    }
    //                }
    //                #endregion
    //                // next sprite
    //                ptr++;
    //            }
    //        }

    //        spriteQueue = start;
    //    }
    //    /// <summary>
    //    /// 三角形 平顶 / 平底 光栅化
    //    /// </summary>
    //    /// <param name="vertex">v = 1</param>
    //    /// <param name="lef">w = 1</param>
    //    /// <param name="rig">u = 1</param>
    //    private unsafe void DrawTriangle(VertexInput* vertex, VertexInput* lef, VertexInput* rig, EColorFlag colorFlag,
    //        ref RECT scissor)
    //    {
    //        bool bottomFixed = lef->Position.Y > vertex->Position.Y;

    //        int result1, result2;
    //        short ytop;
    //        short ybot;
    //        if (bottomFixed)
    //        {
    //            _MATH.Floor(ref vertex->Position.Y, ref lef->Position.Y);
    //            rig->Position.Y = lef->Position.Y;
    //            ytop = (short)vertex->Position.Y;
    //            ybot = (short)lef->Position.Y;
    //        }
    //        else
    //        {
    //            _MATH.Floor(ref lef->Position.Y, ref vertex->Position.Y);
    //            rig->Position.Y = lef->Position.Y;
    //            ytop = (short)lef->Position.Y;
    //            ybot = (short)vertex->Position.Y;
    //        }
    //        _MATH.Floor(ref lef->Position.X, ref rig->Position.X);
    //        //vertex->Position.X = _MATH.Round(vertex->Position.X);

    //        if (ybot < scissor.Y || ytop >= scissor.Height)
    //            return;

    //        float k12 = (vertex->Position.X - lef->Position.X) / (vertex->Position.Y - lef->Position.Y);
    //        float k13 = (vertex->Position.X - rig->Position.X) / (vertex->Position.Y - rig->Position.Y);

    //        float dividerV;
    //        float _v;
    //        float vadd;
    //        if (bottomFixed)
    //        {
    //            int height = ybot - ytop;
    //            dividerV = _MATH.DIVIDE_BY_2048[height];
    //            _v = _MATH.DIVIDE_2048 * height;
    //            vadd = -_MATH.DIVIDE_2048;
    //        }
    //        else
    //        {
    //            dividerV = _MATH.DIVIDE_BY_2048[ybot - ytop];
    //            _v = 0;
    //            vadd = _MATH.DIVIDE_2048;
    //        }

    //        float dividerU = _MATH.DIVIDE_BY_2048[(int)(rig->Position.X - lef->Position.X + 0.5f)];
    //        float _u = 0;

    //        float u, v, w;
    //        VertexOutput ptroutput;
    //        VertexOutput* output = &ptroutput;
    //        VECTOR4 color = VECTOR4.Zero;
    //        float divide255 = _MATH.DIVIDE_BY_1[255];
    //        if (colorFlag == EColorFlag.Fixed)
    //        {
    //            color.X = vertex->Color.R * divide255;
    //            color.Y = vertex->Color.G * divide255;
    //            color.W = vertex->Color.B * divide255;
    //            color.Z = vertex->Color.A * divide255;
    //        }

    //        for (short y = ytop; y < ybot; y++, _v += vadd)
    //        {
    //            if (y < scissor.Y)
    //                continue;
    //            if (y >= scissor.Height)
    //                break;

    //            _MATH.Floor(
    //                (y - lef->Position.Y) * k12 + lef->Position.X,
    //                (y - rig->Position.Y) * k13 + rig->Position.X,
    //                out result1, out result2);
    //            short xlef = (short)result1;
    //            short xrig = (short)result2;
    //            if (xlef == xrig)
    //                continue;
    //            if (xrig < scissor.X || xlef >= scissor.Width)
    //                continue;

    //            v = _v * dividerV;
    //            _u = 0;

    //            for (short x = xlef; x <= xrig; x++, _u += _MATH.DIVIDE_2048)
    //            {
    //                if (x < scissor.X)
    //                    continue;
    //                if (x >= scissor.Width)
    //                    break;

    //                u = _u * dividerU;
    //                w = 1 - u - v;

    //                #region Vertex Output

    //                output->Y = y;
    //                output->X = x;

    //                output->U = (ushort)(lef->TextureCoordinate.X * w + rig->TextureCoordinate.X * u + vertex->TextureCoordinate.X * v);
    //                output->V = (ushort)(lef->TextureCoordinate.Y * w + rig->TextureCoordinate.Y * u + vertex->TextureCoordinate.Y * v);

    //                if (psIndex == 0)
    //                {
    //                    output->Color = datas[output->V * textureWidth + output->U];
    //                }
    //                else
    //                {
    //                    // pixel shader
    //                    for (int i = 0; i < psIndex; i++)
    //                    {
    //                        currentPS[i].PS(ref output);
    //                    }
    //                }

    //                // calc color
    //                if (colorFlag != EColorFlag.Ignore)
    //                {
    //                    if (colorFlag == EColorFlag.Linear)
    //                    {
    //                        color.X = (lef->Color.R * w + rig->Color.R * u + vertex->Color.R * v) * divide255;
    //                        color.Y = (lef->Color.G * w + rig->Color.G * u + vertex->Color.G * v) * divide255;
    //                        color.Z = (lef->Color.B * w + rig->Color.B * u + vertex->Color.B * v) * divide255;
    //                        color.W = (lef->Color.A * w + rig->Color.A * u + vertex->Color.A * v) * divide255;
    //                    }
    //                    output->Color.R = (byte)(output->Color.R * color.X);
    //                    output->Color.G = (byte)(output->Color.G * color.Y);
    //                    output->Color.B = (byte)(output->Color.B * color.Z);
    //                    output->Color.A = (byte)(output->Color.A * color.W);
    //                }

    //                // draw to buffer
    //                buffers[output->Y * bufferWidth + output->X] = output->Color;

    //                #endregion
    //            }
    //        }
    //    }
    //    protected internal override void Render()
    //    {
    //        screenGraphics.SetData(buffers);
    //        baseDevice.Begin();
    //        baseDevice.Draw(screenGraphics, FullGraphicsArea);
    //        baseDevice.End();
    //        baseDevice.Render();
    //    }
    //    public unsafe override void Clear()
    //    {
    //        fixed (COLOR* ptr = buffers)
    //        {
    //            COLOR* color = ptr;
    //            int count = buffers.Length;
    //            for (int i = 0; i < count; i++)
    //            {
    //                color->A = 0;
    //                color++;
    //            }
    //        }
    //    }

    //    private static unsafe void TriangleVerticesSort(ref VertexInput* ptr1, ref VertexInput* ptr2)
    //    {
    //        bool swap = false;
    //        float y = ptr1->Position.Y - ptr2->Position.Y;
    //        if (y > 0)
    //            swap = true;
    //        else if (y == 0)
    //            swap = ptr1->Position.X > ptr2->Position.X;

    //        if (swap)
    //        {
    //            VertexInput* temp = ptr1;
    //            ptr1 = ptr2;
    //            ptr2 = temp;
    //        }
    //    }
    //}


    #endregion
}

#endif