#if CLIENT

using System;
using System.Collections.Generic;
using System.Linq;

namespace EntryEngine.UI
{
    [Flags]
    public enum EAnchor
    {
        Left = 1,
        Top = 2,
        Right = 4,
        Bottom = 8,
        Center = 16,
        Middle = 32,
    }
    public enum EPivot
    {
        TopLeft = 0x00,
        TopCenter = 0x01,
        TopRight = 0x02,
        MiddleLeft = 0x10,
        MiddleCenter = 0x11,
        MiddleRight = 0x12,
        BottomLeft = 0x20,
        BottomCenter = 0x21,
        BottomRight = 0x22,
    }
    public delegate void DUpdate<T>(T sender, Entry e) where T : UIElement;
    public delegate void DDraw<T>(T sender, GRAPHICS spriteBatch, Entry e) where T : UIElement;
    public delegate void DUpdateGlobal(UIElement sender, bool senderEventInvoke, Entry e);

    /// <summary>
    /// 简单，高性能绘制
    /// 1. 不用支持旋转、镜像
    /// 2. 非特殊情况下，不使用Transform、Shader和Scissor
    /// 
    /// 4种状态的Rectangle
    /// 1. 相对于 父容器 / 屏幕
    /// 2. 完整 / 裁剪
    /// 
    /// 更新顺序，绘制顺序，绘制覆盖顺序为逆序，所以可能需要将更新改为逆序
    /// 
    /// 每帧动作顺序
    /// 1. Childs从后往前Event, Parent.Event
    /// 2. Parent.Update, Childs从后往前Update
    /// 3. Parent.Draw, Childs从前往后Draw
    /// 
    /// IsClip及其相关参数的有不明确的地方，需要修改
    /// </summary>
    [Code(ECode.ToBeContinue)]
    public abstract class UIElement : Tree<UIElement>, IDisposable
    {
        private static bool __handled;
        public static bool Handled
        {
            get { return __handled; }
            protected internal set
            {
                __handled = value;
                // DEBUG
                if (__handled)
                {
                }
            }
        }
        protected internal static UIElement FocusedElement { get; private set; }
        public static DUpdateGlobal GlobalEnter;
        public static DUpdateGlobal GlobalHover;
        public static DUpdateGlobal GlobalUnHover;
        public static DUpdateGlobal GlobalClick;
        public static DUpdateGlobal GlobalClicked;

        public string Name;
        private RECT clip;
		private MATRIX2x3 model = MATRIX2x3.Identity;
        public SHADER Shader;
        public bool Enable = true;
        public bool Eventable = true;
        public bool Visible = true;
        public EAnchor Anchor = EAnchor.Left | EAnchor.Top;
        public COLOR Color = COLOR.Default;
        private bool isClip = true;
        private EPivot pivot;
        public object Tag;
        public event Action<VECTOR2> ContentSizeChanged;
        public DUpdate<UIElement> UpdateBegin;
        public DUpdate<UIElement> UpdateEnd;
        public DUpdate<UIElement> EventBegin;
        public DUpdate<UIElement> EventEnd;
        public DDraw<UIElement> DrawBeforeBegin;
        public DDraw<UIElement> DrawAfterBegin;
        public DDraw<UIElement> DrawBeforeChilds;
        public DDraw<UIElement> DrawBeforeEnd;
        public DDraw<UIElement> DrawFocus;
        public DDraw<UIElement> DrawAfterEnd;
        private List<Action<Entry>> events = new List<Action<Entry>>();
        /// <summary>鼠标进入区域内</summary>
        public event DUpdate<UIElement> Enter;
        /// <summary>鼠标在区域内移动</summary>
        public event DUpdate<UIElement> Move;
        /// <summary>鼠标离开区域</summary>
        public event DUpdate<UIElement> Exit;
        /// <summary>获得焦点</summary>
        public event DUpdate<UIElement> Focus;
        /// <summary>失去焦点</summary>
        public event DUpdate<UIElement> Blur;
        /// <summary>鼠标在区域内（不包含进入区域的一次）</summary>
        public event DUpdate<UIElement> Hover;
        /// <summary>鼠标不在区域内</summary>
        public event DUpdate<UIElement> UnHover;
        /// <summary>鼠标左键按下</summary>
        public event DUpdate<UIElement> Click;
        /// <summary>鼠标左键按住拖拽，并指针在目标范围内</summary>
        public event DUpdate<UIElement> Pressed;
        /// <summary>鼠标左键按住拖拽</summary>
        public event DUpdate<UIElement> Drag;
        /// <summary>鼠标左键抬起，需要触发过点击</summary>
        public event DUpdate<UIElement> Clicked;
        /// <summary>鼠标左键抬起</summary>
        public event DUpdate<UIElement> Released;
        /// <summary>鼠标左键双击</summary>
        public event DUpdate<UIElement> DoubleClick;
        /// <summary>键盘按键状态改变</summary>
        public event DUpdate<UIElement> Keyboard;

        private bool needUpdateLocalToWorld = true;
        /// <summary>
        /// 当一个子场景在主场景中时
        /// Entry对场景的更新是从后往前的，即子场景会先更新
        /// 对于Touch来说，前一帧没有按下时，自场景和父场景的Hover状态都是false
        /// 当前帧按下时，父场景由于Hover是false，会导致子场景的Hover也是false
        /// 所以此时应该像needUpdateLocalToWorld时先去更新父场景的Hover状态
        /// </summary>
        private bool needUpdateHover = true;
		private MATRIX2x3 world = MATRIX2x3.Identity;
		private MATRIX2x3 worldInvert = MATRIX2x3.Identity;
        protected VECTOR2 contentSize;
        private int sortZ = -1;
        private bool needSort;
        protected bool isHover;
        private bool isClick;
        /// <summary>viewport in Parent</summary>
        private RECT finalClip;
        /// <summary>graphics viewport in screen</summary>
        private RECT finalViewClip;
        private UIElement[] drawOrder;
        private bool isTopMost;

        internal virtual bool IsScene
        {
            get { return false; }
        }
        protected internal bool NeedUpdateLocalToWorld
        {
            get { return needUpdateLocalToWorld; }
            set
            {
                if (!needUpdateLocalToWorld && value)
                {
                    //foreach (UIElement child in this)
                    for (int i = 0; i < Childs.Count; i++)
                    {
                        Childs[i].NeedUpdateLocalToWorld = value;
                    }
                    needUpdateLocalToWorld = value;
                }
                needUpdateLocalToWorld = value;
            }
        }
        public MATRIX2x3 Model
        {
            get { return model; }
            set
            {
                model = value;
                NeedUpdateLocalToWorld = true;
            }
        }
		public MATRIX2x3 World
        {
            get { return world; }
        }
        public UIScene Scene
        {
            get
            {
                UIElement i = this;
                while (true)
                {
                    if (i.Parent == null)
                    {
                        if (i.IsScene)
                            return (UIScene)i;
                        else
                            return null;
                    }
                    else
                    {
                        if (i.Parent.IsScene)
                            return (UIScene)i.Parent;
                        else
                            i = i.Parent;
                    }
                }
            }
        }
        public UIScene SceneIsRunning
        {
            get
            {
                UIElement i = this;
                while (true)
                {
                    if (i.Parent == null)
                        if (i.IsScene)
                            return (UIScene)i;
                        else
                            return null;
                    i = i.Parent;
                }
            }
        }
        public virtual bool CanFocused
        {
            get { return false; }
        }
        public bool Focused
        {
            get { return FocusedElement == this; }
            set { SetFocus(value); }
        }
        public UIElement NextFocusedElement
        {
            get
            {
                // 检查子对象是否可以设置焦点
                UIElement result = this.CanFocusChild;
                if (result != null)
                {
                    return result;
                }

                for (UIElement parent = Parent, child = this; parent != null; child = parent, parent = parent.Parent)
                {
                    int index = parent.Childs.IndexOf(child);
                    for (int i = index + 1; i < parent.Childs.Count; i++)
                    {
                        // 检查父对象是否可以设置焦点
                        if (parent.Childs[i].CanFocused && parent.Childs[i].IsEventable)
                        {
                            return parent.Childs[i];
                        }
                        // 检查父对象的所有子对象是否可以设置焦点
                        result = parent.Childs[i].CanFocusChild;
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }

                // 最后的焦点控件到头则为无焦点
                return null;
            }
        }
        private UIElement CanFocusChild
        {
            get
            {
                if (!IsEventable)
                    return null;

                UIElement result;
                for (int i = 0; i < Childs.Count; i++)
                {
                    result = Childs[i];
                    if (result.CanFocused && result.IsEventable)
                    {
                        return result;
                    }

                    result = result.CanFocusChild;
                    if (result != null)
                    {
                        return result;
                    }
                }
                return null;
            }
        }

        public float X
        {
            get { return clip.X; }
            set
            {
                if (X != value)
                {
                    clip.X = value;
                    NeedUpdateLocalToWorld = true;
                }
            }
        }
        public float Y
        {
            get { return clip.Y; }
            set
            {
                if (Y != value)
                {
                    clip.Y = value;
                    NeedUpdateLocalToWorld = true;
                }
            }
        }
        public VECTOR2 Location
        {
            get { return new VECTOR2(X, Y); }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }
        public virtual float Width
        {
            get { return Clip.Width; }
            set
            {
                if (value == 0)
                    clip.Width = 0;
                UpdateWidth(Width, value);
                clip.Width = value;
                NeedUpdateLocalToWorld = true;
            }
        }
        public virtual float Height
        {
            get { return Clip.Height; }
            set
            {
                if (value == 0)
                    clip.Height = 0;
                UpdateHeight(Height, value);
                clip.Height = value;
                NeedUpdateLocalToWorld = true;
            }
        }
        public VECTOR2 Size
        {
            get { return new VECTOR2(Width, Height); }
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }
        public RECT Clip
        {
            get
            {
                if (!IsAutoClip)
                {
                    return clip;
                }
                else
                {
                    RECT autoClip = new RECT();
                    autoClip.X = X;
                    autoClip.Y = Y;

                    if (clip.Width == 0)
                        autoClip.Width = contentSize.X;
                    else
                        autoClip.Width = clip.Width;

                    if (clip.Height == 0)
                        autoClip.Height = contentSize.Y;
                    else
                        autoClip.Height = clip.Height;

                    return autoClip;
                }
            }
            set
            {
                X = value.X;
                Y = value.Y;
                Width = value.Width;
                Height = value.Height;
            }
        }
        public RECT InParentClip
        {
            get { return InParent(Clip); }
        }
        public virtual VECTOR2 ContentSize
        {
            get
            {
                RECT clip = ChildClip;
                return new VECTOR2(clip.Right, clip.Bottom);
                //return ChildClip.Size;
            }
        }
        public virtual RECT ChildClip
        {
            get
            {
                return CalcChildClip(this, DefaultChildClip);
            }
        }
        public RECT InParentChildClip
        {
            get { return InParent(ChildClip); }
        }
        public EPivot Pivot
        {
            get { return pivot; }
            set
            {
                if (pivot != value)
                {
                    pivot = value;
                    NeedUpdateLocalToWorld = true;
                }
            }
        }
        public VECTOR2 PivotPoint
        {
            get
            {
                VECTOR2 size = Size;
                return new VECTOR2(PivotAlignmentX * size.X * 0.5f, PivotAlignmentY * size.Y * 0.5f);
            }
        }
        /// <summary>左0/中1/右2</summary>
        public int PivotAlignmentX
        {
            get { return (int)pivot & 0x0f; }
            set { Pivot = (EPivot)(value + (PivotAlignmentY >> 4)); }
        }
        /// <summary>上0/中1/下2</summary>
        public int PivotAlignmentY
        {
            get { return ((int)pivot & 0xf0) >> 4; }
            set { Pivot = (EPivot)(PivotAlignmentX + (value >> 4)); }
        }
        public bool IsAutoClip
        {
            get { return clip.Width == 0 || clip.Height == 0; }
        }
        public bool IsAutoWidth
        {
            get { return clip.Width == 0; }
        }
        public bool IsAutoHeight
        {
            get { return clip.Height == 0; }
        }
        /// <summary>约束子控件是否在自己的可视范围内才让有效</summary>
        public bool IsClip
        {
            get { return isClip; }
            set
            {
                if (isClip != value)
                {
                    isClip = value;
                    NeedUpdateLocalToWorld = true;
                }
            }
        }
        public bool IsHover
        {
            get { return isHover; }
        }
        public bool IsClick
        {
            get { return isClick; }
        }
        public int SortZ
        {
            get { return sortZ; }
            set
            {
                if (sortZ != value)
                {
                    sortZ = value;
                    if (Parent != null)
                    {
                        Parent.needSort = true;
                    }
                }
            }
        }
        public bool FinalHover
        {
            get
            {
                for (int i = 0; i < Childs.Count; i++)
                {
                    if (Childs[i].FinalHover)
                    {
                        return true;
                    }
                }
                return isHover;
            }
        }
        public bool FinalEnable
        {
            get
            {
                for (UIElement i = this; i != null; i = i.Parent)
                {
                    if (!i.IsEnable)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public bool FinalEventable
        {
            get
            {
                for (UIElement i = this; i != null; i = i.Parent)
                {
                    if (!i.IsEventable)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public bool FinalVisible
        {
            get
            {
                for (UIElement i = this; i != null; i = i.Parent)
                {
                    if (!i.IsVisible)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public bool IsEnable
        {
            get { return Enable && IsVisible; }
        }
        public bool IsEventable
        {
            get { return Eventable && IsVisible; }
        }
        public bool IsVisible
        {
            //get { return Visible && Color.A > 0; }
            get { return Visible; }
        }
        protected bool NeedDrawChild
        {
            get
            {
                if (drawOrder == null || drawOrder.Length == 0)
                    return false;

                if (!IsVisible)
                    return false;

                //for (int i = 0; i < drawOrder.Length; i++)
                //{
                //    if (drawOrder[i].IsVisible && !drawOrder[i].drawTopMost)
                //    {
                //        return true;
                //    }
                //}
                //return false;

                return true;
            }
        }
        public virtual RECT ViewClip
        {
            get
            {
                if (needUpdateLocalToWorld)
                {
                    var temp = finalClip;
                    temp.Width = Width;
                    temp.Height = Height;
                    return temp;
                }
                return finalClip;
            }
        }
        public RECT FinalViewClip
        {
            get { return finalViewClip; }
        }

        public UIElement()
        {
            RegistEvent(DoEnter);
            RegistEvent(DoMove);
            RegistEvent(DoExit);
            RegistEvent(DoHover);
            RegistEvent(DoUnHover);
            RegistEvent(DoClick);
            RegistEvent(DoPressed);
            RegistEvent(DoDrag);
            RegistEvent(DoClicked);
            RegistEvent(DoReleased);
            RegistEvent(DoDoubleClick);
            RegistEvent(DoKeyboard);
        }

        public void Update(Entry e)
        {
            UpdateLocalToWorld();

            if (!IsEnable)
                return;

            OnUpdateBegin(e);

            InternalUpdate(e);
            for (int i = Childs.Count - 1; i >= 0 && i < Childs.Count; i--)
            {
                //if (Handled)
                //    break;
                if (!Childs[i].IsScene || ((UIScene)Childs[i]).Entry == null)
                    Childs[i].Update(e);
            }

            OnUpdateEnd(e);
        }
        public void Event(Entry e)
        {
            UpdateLocalToWorld();

            var pointer = e.INPUT.Pointer;
            UpdateHoverState(pointer);

            if (!isClick)
            {
                isClick = isHover && pointer.IsClick(pointer.DefaultKey);
            }

            OnEventBegin(e);

            for (int i = Childs.Count - 1; i >= 0 && i < Childs.Count; i--)
            {
                //if (Handled)
                //    break;
                if (!Childs[i].IsScene || ((UIScene)Childs[i]).Entry == null)
                    Childs[i].Event(e);
            }

            if (FinalEventable && !Handled)
            {
                for (int i = 0; i < events.Count; i++)
                {
                    if (Handled)
                        break;
                    events[i](e);
                }
                if (!Handled)
                    InternalEvent(e);
            }

            OnEventEnd(e);

            if (isClick)
            {
                isClick = pointer.IsPressed(pointer.DefaultKey) ||
                    // Invoke "IsClick" is true in parent InternalEvent
                    pointer.IsRelease(pointer.DefaultKey);
            }

            needUpdateHover = true;
        }
        public void Draw(GRAPHICS spriteBatch, Entry e)
        {
            UpdateLocalToWorld();
            isTopMost = false;

            if (!IsVisible)
                return;

            UpdateSort();
            UpdateContent();

            if (DrawBeforeBegin != null)
            {
                DrawBeforeBegin(this, spriteBatch, e);
            }

            DrawBegin(spriteBatch, ref model, ref finalViewClip, Shader);

            if (DrawAfterBegin != null)
            {
                DrawAfterBegin(this, spriteBatch, e);
            }

            InternalDraw(spriteBatch, e);
            if (DrawBeforeChilds != null)
            {
                DrawBeforeChilds(this, spriteBatch, e);
            }
            if (NeedDrawChild)
            {
                for (int i = 0; i < drawOrder.Length; i++)
                {
                    if (drawOrder[i].isTopMost)
                        continue;
                    if (drawOrder[i].IsScene)
                    {
                        var scene = (UIScene)drawOrder[i];
                        if (!(scene.Entry == null || (!scene.DrawState && scene.IsDrawable)))
                            continue;
                    }
                    drawOrder[i].Draw(spriteBatch, e);
                    //var scene = drawOrder[i] as UIScene;
                    //if (scene == null || scene.Entry == null || (!scene.DrawState && scene.IsDrawable))
                    //    drawOrder[i].Draw(spriteBatch, e);
                }
            }
            InternalDrawAfter(spriteBatch, e);
			if (DrawBeforeEnd != null)
			{
				DrawBeforeEnd(this, spriteBatch, e);
			}

            DrawEnd(spriteBatch, ref model, ref finalViewClip, Shader);

            if (DrawAfterEnd != null)
            {
                DrawAfterEnd(this, spriteBatch, e);
				if (DrawFocus != null)
				{
					DrawFocus(this, spriteBatch, e);
				}
            }
        }
        protected virtual void OnUpdateBegin(Entry e)
        {
            if (UpdateBegin != null)
            {
                UpdateBegin(this, e);
            }
        }
        protected virtual void OnUpdateEnd(Entry e)
        {
            if (UpdateEnd != null)
            {
                UpdateEnd(this, e);
            }
        }
        protected virtual void OnEventBegin(Entry e)
        {
            if (EventBegin != null)
            {
                EventBegin(this, e);
            }
        }
        protected virtual void OnEventEnd(Entry e)
        {
            if (EventEnd != null && !Handled)
            {
                EventEnd(this, e);
            }
        }
		protected virtual void DrawBegin(GRAPHICS spriteBatch, ref MATRIX2x3 transform, ref RECT view, SHADER shader)
        {
        }
		protected virtual void DrawEnd(GRAPHICS spriteBatch, ref MATRIX2x3 transform, ref RECT view, SHADER shader)
        {
        }
        private void UpdateSort()
        {
            if (needSort)
            {
                drawOrder = Childs.ToArray();
				Utility.SortOrderAsc(drawOrder, e => e.sortZ);
                needSort = false;
                OnUpdateSort(drawOrder);
            }
        }
        protected virtual void OnUpdateSort(UIElement[] drawOrder)
        {
        }
        internal void UpdateHoverState(IPointer pointer)
        {
            if (needUpdateHover)
            {
                if (!pointer.Position.IsNaN())
                {
                    isHover = IsContains(pointer.Position);
                }
                else if (!pointer.PositionPrevious.IsNaN())
                {
                    isHover = IsContains(pointer.PositionPrevious);
                }
                else
                {
                    isHover = false;
                }
                needUpdateHover = false;
            }
        }
        private void UpdateLocalToWorld()
        {
            UpdateContent();
            if (NeedUpdateLocalToWorld)
            {
                if (Parent != null)
                    Parent.UpdateLocalToWorld();

                UpdateTranslation(ref model);

                if (Parent != null)
                    world = model * Parent.world;
                else
                    world = model;

                UpdateTransformEnd(ref model, ref world);

                UpdateClip(ref finalClip, ref finalViewClip, ref model, ref world);

				MATRIX2x3.Invert(ref world, out worldInvert);
                needUpdateLocalToWorld = false;
            }
        }
		protected virtual void UpdateTranslation(ref MATRIX2x3 transform)
        {
            transform.M31 = X;
            transform.M32 = Y;

            float pivotX = PivotAlignmentX * Width * 0.5f;
            float pivotY = PivotAlignmentY * Height * 0.5f;
            transform.M31 -= transform.M11 * pivotX + transform.M21 * pivotY;
            transform.M32 -= transform.M12 * pivotX + transform.M22 * pivotY;
        }
		protected virtual void UpdateClip(ref RECT finalClip, ref RECT finalViewClip, ref MATRIX2x3 transform, ref MATRIX2x3 localToWorld)
        {
            RECT clip = Clip;
            finalClip = clip;
            finalClip.X = localToWorld.M31;
            finalClip.Y = localToWorld.M32;

            bool scissor = Parent != null && Parent.isClip;
            finalViewClip = finalClip;
            if (scissor)
            {
				RECT.Intersect(ref finalViewClip, ref Parent.finalViewClip, out finalViewClip);
            }
        }
		protected virtual void UpdateTransformEnd(ref MATRIX2x3 transform, ref MATRIX2x3 localToWorld)
        {
        }
        public void ResetContentSize()
        {
            contentSize.X = 0;
            contentSize.Y = 0;
        }
        private void UpdateContent()
        {
            if (IsNeedUpdateContent())
            {
                VECTOR2 size = ContentSize;
                if (contentSize.X != size.X || contentSize.Y != size.Y)
                {
                    NeedUpdateLocalToWorld = true;
                    contentSize.X = size.X;
                    contentSize.Y = size.Y;
                    if (ContentSizeChanged != null)
                    {
                        ContentSizeChanged(size);
                    }
                }
            }
        }
        protected virtual bool IsNeedUpdateContent()
        {
            return IsAutoClip;
        }
        protected virtual void InternalUpdate(Entry e)
        {
        }
        protected virtual void InternalEvent(Entry e)
        {
        }
        protected virtual void InternalDraw(GRAPHICS spriteBatch, Entry e)
        {
        }
        protected virtual void InternalDrawAfter(GRAPHICS spriteBatch, Entry e)
        {
        }
        public void DrawTopMost()
        {
            DrawTopMost(Scene);
        }
        public void DrawTopMost(UIScene scene)
        {
            if (isTopMost || scene == this || scene == null)
                return;
            isTopMost = true;
            scene.TopMost.Enqueue(this);
        }
        public bool SkipDraw()
        {
            UIScene top = Scene;
            if (top == this || top == null)
                return false;

            if (isTopMost && top.TopMost.Count > 0)
            {
                UIElement peek = top.TopMost.Peek();
                if (peek == this)
                {
                    top.TopMost.Dequeue();
                }
                else
                {
                    throw new InvalidOperationException("SkipDraw should be called before DrawTopMost and can't invoke repeated");
                }
            }
            else
            {
                isTopMost = true;
            }
            return isTopMost;
        }
        public virtual void ToFront()
        {
            if (Parent == null)
                return;

            if (Parent.Childs.Remove(this))
            {
                Parent.Childs.Add(this);
                needSort = true;
            }
        }
        public virtual void ToBack()
        {
            if (Parent == null)
                return;

            if (Parent.Childs.Remove(this))
            {
                Parent.Childs.Insert(0, this);
                needSort = true;
            }
        }

        public void AddChildFirst(UIElement node)
        {
            Insert(node, 0);
        }
        protected override bool CheckAdd(UIElement node)
        {
            return Childs.IndexOf(node) == -1;
        }
        protected override void OnAdded(UIElement node, int index)
        {
            //child.UpdateLocalToWorld();
            needSort = true;
            NeedUpdateLocalToWorld = true;
        }
        protected override void OnRemoved(UIElement node)
        {
            needSort = true;
            NeedUpdateLocalToWorld = true;
        }
        protected void InsertChildBefore(UIElement element, UIElement target)
        {
            int index = Childs.IndexOf(target);
            if (index == -1)
                Insert(element, 0);
            else
                Insert(element, index);
        }
        protected void InsertChildAfter(UIElement element, UIElement target)
        {
            int index = Childs.IndexOf(target);
            if (index == -1)
                Add(element);
            else
                Insert(element, index + 1);
        }
        private void UpdateWidth(float srcWidth, float dstWidth)
        {
            // UI编辑器 -> 读取TabPage.Page -> 读取TabPage.Parent.Clip -> 导致Page尺寸拉大
            if (srcWidth == 0)
                return;

            float add = dstWidth - srcWidth;
            float mul = srcWidth == 0 ? 0 : dstWidth / srcWidth;
            for (int i = 0; i < Childs.Count; i++)
            {
                var child = Childs[i];
                bool left = (child.Anchor & EAnchor.Left) == EAnchor.Left;
                bool right = (child.Anchor & EAnchor.Right) == EAnchor.Right;
                bool center = (child.Anchor & EAnchor.Center) == EAnchor.Center;

                if (center)
                {
                    if (left && right)
                    {
                        child.X *= mul;
                        child.Width *= mul;
                    }
                    else if (right)
                    {
                        child.X = (child.X + child.Width) * mul - child.Width;
                        //child.X = child.X * mul + child.Width * (mul - 1);
                    }
                    else
                    {
                        child.X *= mul;
                    }
                }
                else
                {
                    if (left && right)
                    {
                        child.Width += add;
                    }
                    else if (right)
                    {
                        child.X += add;
                    }
                }
            }
        }
        private void UpdateHeight(float srcHeight, float dstHeight)
        {
            if (srcHeight == 0)
                return;

            float add = dstHeight - srcHeight;
            float mul = srcHeight == 0 ? 0 : dstHeight / srcHeight;
            for (int i = 0; i < Childs.Count; i++)
            {
                var child = Childs[i];
                bool top = (child.Anchor & EAnchor.Top) == EAnchor.Top;
                bool bottom = (child.Anchor & EAnchor.Bottom) == EAnchor.Bottom;
                bool middle = (child.Anchor & EAnchor.Middle) == EAnchor.Middle;

                if (middle)
                {
                    if (top && bottom)
                    {
                        child.Y *= mul;
                        child.Height *= mul;
                    }
                    else if (bottom)
                    {
                        child.Y = (child.Y + child.Height) * mul - child.Height;
                        //child.Y = child.Y * mul + child.Height * (mul - 1);
                    }
                    else
                    {
                        child.Y *= mul;
                    }
                }
                else
                {
                    if (top && bottom)
                    {
                        child.Height += add;
                    }
                    else if (bottom)
                    {
                        child.Y += add;
                    }
                }
            }
        }
        public virtual bool IsContains(VECTOR2 graphicsPosition)
        {
            return InParentClip.Contains(ConvertGraphicsToLocalView(graphicsPosition));
        }
        public RECT InParent(RECT clip)
        {
            clip.X -= PivotAlignmentX * Width * 0.5f;
            clip.Y -= PivotAlignmentY * Height * 0.5f;
            if (!needUpdateLocalToWorld && (clip.X != model.M31 || clip.Y != model.M32))
            {
                clip.X = model.M31;
                clip.Y = model.M32;
            }
            return clip;
        }
        public VECTOR2 ConvertGraphicsToLocalView(VECTOR2 point)
        {
            if (Parent != null)
            {
                if (Parent.isClip && !Parent.isHover)
                {
                    point = VECTOR2.NaN;
                }
                else
                {
                    point = Parent.ConvertGraphicsToLocal(point);
                }
            }
            return point;
        }
        public VECTOR2 ConvertGraphicsToLocal(VECTOR2 point)
        {
            VECTOR2.Transform(ref point, ref worldInvert);
			return point;
        }
        public VECTOR2 ConvertLocalToGraphics(VECTOR2 point)
        {
            VECTOR2.Transform(ref point, ref world);
			return point;
        }
        public VECTOR2 ConvertLocalToOther(VECTOR2 point, UIElement other)
        {
            VECTOR2 result;
            result = ConvertLocalToGraphics(point);
            if (other != null)
                result = other.ConvertGraphicsToLocal(result);
            return result;
        }
        public bool SetFocus(bool focus)
        {
            if (focus)
            {
                if (FocusedElement != null && FocusedElement != this)
                {
                    //focusedElement.OnBlur();
                    FocusedElement.SetFocus(false);
                }
                if (!CanFocused)
                {
                    return false;
                }
                FocusedElement = this;
                this.OnFocus();
                return true;
            }
            else
            {
                if (Focused)
                {
                    FocusedElement = null;
                    OnBlur();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public virtual void Dispose()
        {
            for (int i = 0; i < Childs.Count; i++)
            {
                Childs[i].Dispose();
            }
        }

        protected void RegistEvent(Action<Entry> e)
        {
            if (e == null)
                throw new ArgumentNullException("Event");
            events.Add(e);
        }
        protected bool OnEnter(Entry e)
        {
            return isHover && !IsContains(e.INPUT.Pointer.PositionPrevious);
        }
        private void DoEnter(Entry e)
        {
            if (GlobalEnter == null && Enter == null) return;
            bool enter = OnEnter(e);
            if (Enter != null && enter)
            {
                Enter(this, e);
            }
            if (GlobalEnter != null && enter)
            {
                GlobalEnter(this, Enter != null, e);
            }
        }
        protected bool OnMove(Entry e)
        {
            if (isHover)
            {
                VECTOR2 moved = e.INPUT.Pointer.DeltaPosition;
                if (!moved.IsNaN() && moved.X != 0 && moved.Y != 0)
                {
                    return true;
                }
            }
            return false;
        }
        private void DoMove(Entry e)
        {
            if (Move != null && OnMove(e))
            {
                Move(this, e);
            }
        }
        protected bool OnExit(Entry e)
        {
            return !isHover && IsContains(e.INPUT.Pointer.PositionPrevious);
        }
        private void DoExit(Entry e)
        {
            if (Exit != null && OnExit(e))
            {
                Exit(this, e);
            }
        }
        protected virtual void OnFocus()
        {
            if (Focus != null)
            {
                Focus(this, Entry.Instance);
            }
        }
        protected virtual void OnBlur()
        {
            if (Blur != null)
            {
                Blur(this, Entry.Instance);
            }
        }
        private void DoHover(Entry e)
        {
            if (Hover != null && isHover)
            {
                Hover(this, e);
            }
            if (GlobalHover != null && isHover)
            {
                GlobalHover(this, Hover != null, e);
            }
        }
        private void DoUnHover(Entry e)
        {
            if (UnHover != null && !isHover)
            {
                UnHover(this, e);
            }
            if (GlobalUnHover != null && !isHover)
            {
                GlobalUnHover(this, UnHover != null, e);
            }
        }
        protected bool OnClick(Entry e)
        {
            return isHover && e.INPUT.Pointer.IsClick(e.INPUT.Pointer.DefaultKey);
        }
        private void DoClick(Entry e)
        {
            if (Focused && e.INPUT.Pointer.IsClick(e.INPUT.Pointer.DefaultKey) && !isHover)
            {
                SetFocus(false);
            }
            if (Click == null && GlobalClick == null) return;
            bool flag = OnClick(e);
            if (Click != null && flag)
            {
                Click(this, e);
            }
            if (GlobalClick != null && flag)
            {
                GlobalClick(this, Click != null, e);
            }
        }
        protected bool OnPressed(Entry e)
        {
            return isClick && isHover && e.INPUT.Pointer.IsPressed(e.INPUT.Pointer.DefaultKey);
        }
        private void DoPressed(Entry e)
        {
            if (Pressed != null && OnPressed(e))
            {
                Pressed(this, e);
            }
        }
        protected bool OnDrag(Entry e)
        {
            return isClick && e.INPUT.Pointer.IsPressed(e.INPUT.Pointer.DefaultKey);
        }
        private void DoDrag(Entry e)
        {
            if (Drag != null && OnDrag(e))
            {
                Drag(this, e);
            }
        }
        protected bool OnClicked(Entry e)
        {
            return isHover && isClick && e.INPUT.Pointer.IsRelease(e.INPUT.Pointer.DefaultKey);
        }
        private void DoClicked(Entry e)
        {
            if (Clicked == null && GlobalClicked == null) return;
            bool flag = OnClicked(e);
            if (Clicked != null && flag)
            {
                Clicked(this, e);
            }
            if (GlobalClicked != null && flag)
            {
                GlobalClicked(this, Clicked != null, e);
            }
        }
        protected bool OnReleased(Entry e)
        {
            return isHover && e.INPUT.Pointer.IsRelease(e.INPUT.Pointer.DefaultKey);
        }
        private void DoReleased(Entry e)
        {
            if (Released != null && OnReleased(e))
            {
                Released(this, e);
            }
        }
        protected bool OnDoubleClick(Entry e)
        {
            return isHover && e.INPUT.Pointer.ComboClick.IsDoubleClick;
        }
        private void DoDoubleClick(Entry e)
        {
            if (DoubleClick != null && OnDoubleClick(e))
            {
                DoubleClick(this, e);
            }
        }
        protected bool OnKeyboard(Entry e)
        {
            return e.INPUT.Keyboard != null && e.INPUT.Keyboard.Focused;
        }
        private void DoKeyboard(Entry e)
        {
            if (Keyboard != null && OnKeyboard(e))
            {
                Keyboard(this, e);
            }
        }

        public static RECT CalcChildClip(UIElement parent, Func<UIElement, RECT> clipGenerator)
        {
            if (parent.Childs.Count == 0)
                return RECT.Empty;
            RECT clip = new RECT(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);
            foreach (UIElement child in parent.Childs)
            {
                RECT temp = clipGenerator(child);
                if (temp.Width == 0 || temp.Height == 0)
                    continue;
                clip.X = _MATH.Min(temp.X, clip.X);
                clip.Y = _MATH.Min(temp.Y, clip.Y);
                clip.Width = _MATH.Max(temp.Right, clip.Width);
                clip.Height = _MATH.Max(temp.Bottom, clip.Height);
            }
            clip.Width = clip.Width - clip.X;
            clip.Height = clip.Height - clip.Y;
            return clip;
        }
        protected static RECT DefaultChildClip(UIElement child)
        {
            if (!child.Visible)
                return RECT.Empty;
            return child.InParentClip;
            //return RECT.Union(child.Clip, child.InParentChildClip);
        }
        public static VECTOR2 CalcPivotPoint(VECTOR2 size, EPivot pivot)
        {
            return new VECTOR2(
                Utility.EnumLow4((int)pivot) * size.X * 0.5f,
                Utility.EnumHigh4((int)pivot) * size.Y * 0.5f);
        }
        public static int PivotX(EPivot pivot)
        {
            return Utility.EnumLow4((int)pivot);
        }
        public static int PivotY(EPivot pivot)
        {
            return Utility.EnumHigh4((int)pivot);
        }
        public static VECTOR2 TextAlign(RECT bound, VECTOR2 textSize, EPivot alignment)
        {
            VECTOR2 location = bound.Location;
            location = VECTOR2.Add(location, CalcPivotPoint(bound.Size, alignment));
            location = VECTOR2.Subtract(location, CalcPivotPoint(textSize, alignment));
            return location;
        }
        public static UIElement FindElementByPosition(UIElement Parent, VECTOR2 screenPosition)
        {
            return FindChildPriority(Parent, e => !e.IsVisible, e => e.IsContains(screenPosition));
        }
        public static bool FindSkipInvisible(UIElement target)
        {
            return !target.Visible;
        }
        public static bool FindSkipUnhover(UIElement target)
        {
            return !target.isHover;
        }
    }

    /// <summary>
    /// 流程状态
    /// 1. Ending & Loading同时进行
    /// 2. 所有Ending结束，Loading完成的菜单率先进入Preparing，Preparing需要进行绘制，但不进行更新
    /// 3. 所有Preparing结束，进入Showing
    /// 4. 所有Beginning结束，进入Running
    /// 
    /// 流程状态2
    /// Ending: Update, Draw
    /// Loading:
    /// Preparing: Draw
    /// Showing: Update, Draw
    /// Running: Event, Update, Draw
    /// </summary>
    public enum EPhase
    {
        None,
        Ending,
        Loading,
        Preparing,
        Prepared,
        Showing,
        Running,
    }
    /// <summary>
    /// 场景更新的状态
    /// 
    /// 参数
    ///     None:
    ///         继续更新
    ///         
    ///     Dialog:
    ///         对话框，不更新其它场景事件
    ///         
    ///     Block:
    ///         对话框，完全跳过其它场景更新
    ///         
    ///     Cover:
    ///         遮罩，覆盖除主菜单外的所有自己下面的对话框，使其不绘制也不更新
    ///         
    ///     CoverAll:
    ///         完全跳过其它菜单的绘制和更新
    ///         
    ///     Break:
    ///         可以移除此场景
    ///         
    ///		Dispose:
    ///			移除场景并释放资源
    ///			
    ///		Release:
    ///			移除场景并释放资源及其在Entry内的缓存
    /// </summary>
    [Code(ECode.ToBeContinue)]
    public enum EState
    {
        None,
        Dialog,
        Block,
        Break,
        Dispose,
        Release,
        Cover,
        CoverAll,
    }
    public enum EContent
    {
        New,
        Inherit,
        System
    }
    /// <summary>
    /// 只保留居中和自定义
    /// </summary>
    [Code(ECode.MayBeReform)]
    public enum EShowPosition
    {
        Default,
        ParentCenter,
        [Obsolete]
        GraphicsCenter,
    }
    public class UIScene : Panel, IDisposable
    {
        internal EPhase Phase;
        internal COROUTINE Phasing;
        public EState State = EState.None;
        public EContent ContentType = EContent.Inherit;
        public EShowPosition ShowPosition;
        public PCKeys FocusNextKey = PCKeys.Tab;
        public event Action<UIScene, ContentManager> PhaseLoading;
        public event Action<UIScene> PhasePreparing;
        public event Action<UIScene> PhasePrepared;
        public event Action<UIScene> PhaseShowing;
        public event Action<UIScene> PhaseShown;
        public event Action<UIScene> PhaseEnding;
        public event Action<UIScene> PhaseEnded;
        public event Action<UIScene, ContentManager> LoadCompleted;
        internal Queue<UIElement> TopMost = new Queue<UIElement>();
        private List<AsyncLoadContent> loadings = new List<AsyncLoadContent>();
        internal bool IsDrawable;

        internal override bool IsScene
        {
            get { return true; }
        }
        internal bool DrawState
        {
            get { return State != EState.Break && State != EState.Dispose && State != EState.Release; }
        }
        public Entry Entry
        {
            get;
            internal set;
        }
        public ContentManager Content
        {
            get;
            protected set;
        }
        public bool IsDisposed
        {
            get
            {
                if (Content == null || Content.IsDisposed)
                {
					return true;
                }
                else
                {
					if (ContentType == EContent.Inherit && Parent != null && Parent.Scene != null)
                    {
                        return Parent.Scene.IsDisposed;
                    }
                    return false;
                }
            }
        }
        protected IEnumerable<AsyncLoadContent> Loadings
        {
            get { return loadings.Enumerable(); }
        }
        public EPhase RunningState
        {
            get { return Phase; }
        }
        public bool IsInStage
        {
            get { return Entry != null; }
        }

        public UIScene()
        {
            if (Entry._GRAPHICS == null)
                this.Size = new VECTOR2(1280, 720);
            else
                this.Size = Entry._GRAPHICS.GraphicsSize;
            this.Keyboard += DoKeyboard;
        }
        public UIScene(string name)
            : this()
        {
            this.Name = name;
        }

        internal void SetPhase(IEnumerable<ICoroutine> coroutine)
        {
            if (Phasing != null)
                Phasing.Dispose();
            if (coroutine == null)
                Phasing = null;
            else
                Phasing = new COROUTINE(coroutine);
        }
        internal void OnPhaseLoading()
        {
            Phase = EPhase.Loading;
            SetPhase(Loading());
            if (PhaseLoading != null)
                PhaseLoading(this, Content);
        }
        internal void OnLoadCompleted()
        {
            if (loadings.Count == 0)
                if (LoadCompleted != null)
                    LoadCompleted(this, Content);
        }
        internal void OnPhasePreparing()
        {
            OnLoadCompleted();
            Phase = EPhase.Preparing;
            SetPhase(Preparing());
            if (PhasePreparing != null)
                PhasePreparing(this);
        }
        internal void OnPhasePrepared()
        {
            Phase = EPhase.Prepared;
            if (PhasePrepared != null)
                PhasePrepared(this);
        }
        /// <summary>
        /// Scene进入到Entry
        /// </summary>
        /// <param name="previous">切换菜单则是前一个主菜单，二级菜单则为当前主菜单</param>
        internal void OnPhaseShowing()
        {
            Phase = EPhase.Showing;
            SetPhase(Showing());
            if (PhaseShowing != null)
                PhaseShowing(this);
        }
        internal void OnPhaseShown()
        {
            Phase = EPhase.Running;
            SetPhase(Running());
            if (PhaseShown != null)
                PhaseShown(this);
        }
        internal void OnPhaseEnding()
        {
            Phase = EPhase.Ending;
            SetPhase(Ending());
            if (PhaseEnding != null)
                PhaseEnding(this);
        }
        /// <summary>
        /// Scene从Entry移除
        /// </summary>
        /// <param name="next">换菜单则是即将切换到的菜单，否则为null</param>
        internal void OnPhaseEnded()
        {
            Phase = EPhase.None;
            if (PhaseEnded != null)
                PhaseEnded(this);
            Entry = null;
        }

        protected internal virtual IEnumerable<ICoroutine> Ending()
        {
            return null;
        }
        protected internal virtual IEnumerable<ICoroutine> Loading()
        {
            return null;
        }
        protected internal virtual IEnumerable<ICoroutine> Preparing()
        {
            return null;
        }
        protected internal virtual IEnumerable<ICoroutine> Showing()
        {
            return null;
        }
        protected internal virtual IEnumerable<ICoroutine> Running()
        {
            return null;
        }
        /// <summary>
        /// <para>异步加载协程，在Load中调用</para>
        /// <para>1. LoadAsync 完全不阻断协程</para>
        /// <para>2. yield return LoadAsync 阻断协程直到异步加载完成，可以自定义ICoroutine来实现加载条</para>
        /// </summary>
        /// <param name="async">异步加载状态</param>
        /// <returns>阻断协程</returns>
        protected virtual ICoroutine LoadAsync(AsyncLoadContent async)
        {
            if (!async.IsEnd)
            {
                loadings.Add(async);
                return async;
            }
            return null;
        }
        internal void Show(Entry entry)
        {
            this.Entry = entry;

            SetPhase(null);

            switch (ShowPosition)
            {
                case EShowPosition.ParentCenter:
                    Pivot = EPivot.MiddleCenter;
                    if (Parent != null)
                        Location = Parent.Size * 0.5f;
                    else
                    {
                        //goto case EShowPosition.GraphicsCenter;
                        Pivot = EPivot.MiddleCenter;
                        Location = Entry.GRAPHICS.GraphicsSize * 0.5f;
                    }
                    break;

                case EShowPosition.GraphicsCenter:
                    Pivot = EPivot.MiddleCenter;
                    Location = Entry.GRAPHICS.GraphicsSize * 0.5f;
                    break;
            }

            if (IsDisposed)
            {
                if (ContentType == EContent.Inherit)
                {
                    // inherit from parent scene
                    if (Parent != null && Parent.Scene != null)
                    {
                        Content = Parent.Scene.Content;
                    }

                    // inherit from current main scene
                    if (Content == null && Entry.Scene != null)
                    {
                        Content = Entry.Scene.Content;
                    }
                }
                else if (ContentType == EContent.System)
                {
                    if (entry.ContentManager != null)
                    {
                        Content = entry.ContentManager;
                    }
                }

                if (Content == null)
                {
                    Content = entry.NewContentManager();
                }
            }
        }
        public void Close(bool immediately)
        {
            Close(State, immediately);
        }
        public void Close(EState state, bool immediately)
        {
            if (Entry == null)
                return;

            if (immediately)
                Entry.CloseImmediately(this, state);
            else
                Entry.Close(this, state);
        }
        /// <summary>
        /// 场景在其它场景里时，被Remove或Clear时需要关闭此场景
        /// </summary>
        protected override void OnRemovedBy(UIElement parent)
        {
            base.OnRemovedBy(parent);
            Close(true);
        }
        public override void ToFront()
        {
            if (Entry == null)
                base.ToFront();
            else
                Entry.ToFront(this);
        }
        public override void ToBack()
        {
            if (Entry == null)
                base.ToBack();
            else
                Entry.ToBack(this);
        }
        protected override void InternalUpdate(Entry e)
        {
            if (loadings.Count > 0)
            {
                loadings = loadings.Where(l => !l.IsEnd).ToList();
                OnLoadCompleted();
            }
            base.InternalUpdate(e);
        }
        private void DoKeyboard(UIElement sender, Entry e)
        {
            if (Parent == null && e.INPUT.Keyboard.IsClick(FocusNextKey))
            {
                UIElement next = FocusedElement;
                if (next == null)
                    // 第一个可以设置焦点的控件
                    next = NextFocusedElement;
                else
                    // 当前焦点的下一个焦点控件
                    next = next.NextFocusedElement;

                if (next != null)
                    next.SetFocus(true);
                else if (FocusedElement != null)
                    // 最后的焦点控件后设置为无焦点
                    FocusedElement.SetFocus(false);
            }
        }
		protected override void DrawEnd(GRAPHICS spriteBatch, ref MATRIX2x3 transform, ref RECT view, SHADER shader)
        {
            while (TopMost.Count > 0)
                TopMost.Dequeue().Draw(spriteBatch, Entry.Instance);

            base.DrawEnd(spriteBatch, ref transform, ref view, shader);
        }
        public override void Dispose()
        {
            foreach (var loading in loadings)
                if (!loading.IsEnd)
                    loading.Cancel();
            loadings.Clear();
            base.Dispose();
            if (Content != null && Content != Entry.Instance.ContentManager)
            {
                Content.Dispose();
                Content = null;
            }
            State = EState.Dispose;
            SetPhase(null);
            TopMost.Clear();
        }
    }

    public class UIText
    {
        public string Text = "";
        public FONT Font = FONT.Default;
        public COLOR FontColor = COLOR.Default;
        public EPivot TextAlignment;
        public TextShader TextShader;
        public VECTOR2 Padding;
        public float Scale = 1f;

        public float FontSize
        {
            get { return Font == null ? 0 : Font.FontSize; }
            set { if (Font != null) Font.FontSize = value; }
        }

        public void GetPaddingClip(ref RECT rect)
        {
            int x = UIElement.PivotX(TextAlignment);
            int y = UIElement.PivotY(TextAlignment);
            rect.X += Padding.X * 0.5f;
            rect.Width -= Padding.X;
            rect.Y += Padding.Y * 0.5f;
            rect.Height -= Padding.Y;
        }
        public void GetAlignmentClip(ref RECT rect, out float offsetX, out float offsetY)
        {
            int x = UIElement.PivotX(TextAlignment);
            int y = UIElement.PivotY(TextAlignment);
            VECTOR2 size = Font.MeasureString(Text);
            offsetX = (rect.Width - size.X) * 0.5f * x;
            offsetY = (rect.Height - size.Y) * 0.5f * y;
            rect.X += offsetX;
            rect.Y += offsetY;
            if (offsetX < 0)
                rect.Width += -offsetX * 2;
            if (offsetY < 0)
                rect.Height += -offsetY * 2;
        }
        public RECT GetTextClip(RECT rect)
        {
            int x = UIElement.PivotX(TextAlignment);
            int y = UIElement.PivotY(TextAlignment);
            rect.X += Padding.X * 0.5f;
            rect.Width -= Padding.X;
            rect.Y += Padding.Y * 0.5f;
            rect.Height -= Padding.Y;

            VECTOR2 size = Font.MeasureString(Text) * Scale;
            float offsetX = (rect.Width - size.X) * 0.5f * x;
            float offsetY = (rect.Height - size.Y) * 0.5f * y;
            rect.X += offsetX;
            rect.Y += offsetY;
            if (offsetX < 0)
                rect.Width += -offsetX * 2;
            if (offsetY < 0)
                rect.Height += -offsetY * 2;
            return rect;
        }
        public void Draw(GRAPHICS spriteBatch, RECT rect)
        {
            if (Font != null && !string.IsNullOrEmpty(Text))
            {
                VECTOR2 location = GetTextClip(rect).Location;
                bool effect = false;
                if (TextShader != null)
                {
                    FontTexture ft = Font as FontTexture;
                    if (ft == null)
                    {
                        if (TextShader.IsShader)
                        {
                            spriteBatch.Draw(Font, Text, VECTOR2.Add(location, TextShader.Offset), TextShader.Color, Scale);
                        }
                        // 不支持描边
                    }
                    else
                    {
                        ft.Effect = TextShader;
                    }
                }
                spriteBatch.Draw(Font, Text, location, FontColor, Scale);
                if (effect)
                {
                    ((FontTexture)Font).Effect = null;
                }
            }
        }
    }
}

#endif