#if CLIENT

using System;
using System.Collections.Generic;

namespace EntryEngine.UI
{
    /// <summary>滚动模式</summary>
	[Flags]public enum EScrollOrientation
	{
        /// <summary>没有滚动</summary>
		None = 0x00,
		Horizontal = 0x01,
		HorizontalAuto = 0x02,
		Vertical = 0x04,
		VerticalAuto = 0x08,
	}
    /// <summary>拖拽模式</summary>
	public enum EDragMode
	{
        /// <summary>不能拖拽</summary>
		None,
        /// <summary>可拖拽</summary>
		Drag,
		//DragInertia,
        /// <summary>移动控件的位置</summary>
		Move
	}
    /// <summary>面板</summary>
	public class Panel : UIElement
	{
        private const float INERTIA_STOP = 0.05f;

		private EScrollOrientation scrollOrientation = EScrollOrientation.VerticalAuto | EScrollOrientation.HorizontalAuto;
		private ScrollBarBase scrollBarHorizontal;
		private ScrollBarBase scrollBarVertical;
		private VECTOR2 offset;
		private VECTOR2 offsetScope;
		private VECTOR2 contentScope;
		private bool scrollBarSizeFixed;
		private bool needUpdateScrollBar;
		private bool needBeginEnd;
        /// <summary>无论可否滚动，覆盖控件大小的背景图片</summary>
		public TEXTURE Background;
        /// <summary>内容较多可滚动时，显示在完整内容区域的背景图片</summary>
        public TEXTURE BackgroundFull;
        /// <summary>拖拽模式</summary>
		public EDragMode DragMode;
        /// <summary>拖拽时的惯性缩放值，例如鼠标移动5px，乘以这个缩放值来施加给惯性</summary>
        public float DragInertia;
        /// <summary>惯性拖动碰撞到边缘时的反弹力度的缩放，0时不反弹，1时原力度反弹</summary>
        public float Rebound;
        /// <summary>惯性移动量，拖拽时会自动维护，也可以自定义设置这个值</summary>
        public VECTOR2 Inertia;
        private float dragFriction = 0.95f;
        /// <summary>鼠标滑轮的滑动像素，0则不能使用鼠标滑轮</summary>
        public float ScrollWheelSpeed = 100;
        /// <summary>滚动事件</summary>
		public event DUpdate<Panel> Scroll;
		public event DUpdate<Panel> ScrollBarChanged;

        /// <summary>滚动条显示</summary>
		public EScrollOrientation ScrollOrientation
		{
			get { return scrollOrientation; }
			set
			{
				if (scrollOrientation != value)
				{
					scrollOrientation = value;
					UpdateScrollBar();
				}
			}
		}
        /// <summary>横向滚动条</summary>
		public ScrollBarBase ScrollBarHorizontal
		{
			get { return scrollBarHorizontal; }
			set
			{
				if (scrollBarHorizontal != null)
				{
					scrollBarHorizontal.ValueChanged -= DoScrollX;
                    scrollBarHorizontal.Panel = null;
				}
				if (value != null)
				{
					value.ValueChanged += DoScrollX;
                    value.Panel = this;
				}
				scrollBarHorizontal = value;
				UpdateScrollBar();
			}
		}
        /// <summary>纵向滚动条</summary>
		public ScrollBarBase ScrollBarVertical
		{
			get { return scrollBarVertical; }
			set
			{
				if (scrollBarVertical != null)
				{
					scrollBarVertical.ValueChanged -= DoScrollY;
                    scrollBarVertical.Panel = null;
				}
				if (value != null)
				{
					value.ValueChanged += DoScrollY;
                    value.Panel = this;
				}
				scrollBarVertical = value;
				UpdateScrollBar();
			}
		}
		public bool ScrollBarSizeFixed
		{
			get { return scrollBarSizeFixed; }
			set
			{
				if (scrollBarSizeFixed != value)
				{
					scrollBarSizeFixed = value;
					UpdateScrollBar();
				}
			}
		}
        /// <summary>拖拽惯性摩擦，惯性力*=摩擦力；0则立刻停下，1则停不下来</summary>
        public float DragFriction
        {
            get { return dragFriction; }
            set { dragFriction = _MATH.Clamp(value, 0, 1); }
        }
        /// <summary>内容偏移</summary>
		public float OffsetX
		{
			get { return offset.X; }
			set
			{
				float x = offset.X;
				offset.X = _MATH.Clamp(value, 0, offsetScope.X);
				if (x != offset.X)
				{
					OnScroll();
				}
			}
		}
        /// <summary>内容偏移</summary>
		public float OffsetY
		{
			get { return offset.Y; }
			set
			{
				float y = offset.Y;
				offset.Y = _MATH.Clamp(value, 0, offsetScope.Y);
				if (y != offset.Y)
				{
					OnScroll();
				}
			}
		}
        /// <summary>内容偏移</summary>
		public VECTOR2 Offset
		{
			get { return offset; }
			set
			{
				VECTOR2 temp = offset;
				offset.X = _MATH.Clamp(value.X, 0, offsetScope.X);
				offset.Y = _MATH.Clamp(value.Y, 0, offsetScope.Y);
				if (temp.X != offset.X || temp.Y != offset.Y)
				{
					OnScroll();
				}
			}
		}
        /// <summary>内容可偏移区间</summary>
		public VECTOR2 OffsetScope
		{
			get { return offsetScope; }
		}
        /// <summary>内容区间</summary>
		public VECTOR2 ContentScope
		{
			get
			{
				return contentScope;
			}
			set
			{
				if (contentScope.X == value.X && contentScope.Y == value.Y)
					return;
				contentScope = value;
                UpdateScrollScope(true);
			}
		}
        /// <summary>横向内容能否滚动</summary>
		public bool CanScrollHorizontal
		{
			get { return offsetScope.X > 0; }
		}
        /// <summary>纵向内容能否滚动</summary>
		public bool CanScrollVertical
		{
			get { return offsetScope.Y > 0; }
		}
		public bool ShowScrollHorizontal
		{
			get
			{
				return scrollBarHorizontal != null &&
					(Utility.EnumContains((int)scrollOrientation, (int)EScrollOrientation.Horizontal) ||
					(Utility.EnumContains((int)scrollOrientation, (int)EScrollOrientation.HorizontalAuto) && CanScrollHorizontal));
			}
		}
		public bool ShowScrollVertical
		{
			get
			{
				return scrollBarVertical != null &&
				(Utility.EnumContains((int)scrollOrientation, (int)EScrollOrientation.Vertical) ||
				(Utility.EnumContains((int)scrollOrientation, (int)EScrollOrientation.VerticalAuto) && CanScrollVertical));
			}
		}
		public override float Width
		{
			get
			{
				return base.Width;
			}
			set
			{
				base.Width = value;
                UpdateScrollScope(true);
			}
		}
		public override float Height
		{
			get
			{
				return base.Height;
			}
			set
			{
				base.Height = value;
				UpdateScrollScope(true);
			}
		}
		public override VECTOR2 ContentSize
		{
			get
			{
				if (contentScope.X != 0 && contentScope.Y != 0)
					return contentScope;
				VECTOR2 size = base.ContentSize;
				if (contentScope.X != 0)
					size.X = contentScope.X;
				if (contentScope.Y != 0)
					size.Y = contentScope.Y;
				return size;
			}
		}
        //public override RECT ViewClip
        //{
        //    get { return GetOffsetView(base.ViewClip); }
        //}
        public RECT FullViewClip
        {
            get { return GetOffsetView(base.ViewClip); }
        }
		protected virtual bool NeedBeginEnd
		{
			get
			{
				if (CanScrollHorizontal || CanScrollVertical)
					return true;

				RECT clip = ChildClip;
				return clip.X < 0 || clip.Y < 0;
			}
		}
        public override EUIType UIType
        {
            get { return EUIType.Panel; }
        }

		public Panel()
		{
			ContentSizeChanged += InternalContentSizeChanged;
			Drag += DoDrag;
            Hover += new DUpdate<UIElement>(PanelMouseScroll);
            //IsClip = true;
		}

        protected override void OnAdd(UIElement node, int index)
        {
            base.OnAdd(node, index);
            UpdateScrollScope(true);
        }
        protected override void OnRemoved(UIElement node)
        {
            base.OnRemoved(node);
            UpdateScrollScope(true);
        }
        protected void PanelMouseScroll(UIElement sender, Entry e)
        {
            if (e.INPUT.Mouse != null && ScrollWheelSpeed != 0)
            {
                float value = e.INPUT.Mouse.ScrollWheelValue;
                if (value != 0)
                {
                    // 鼠标悬浮在某个可滑动的窗口时，滑倒边界也不触发其它面板的滑动
                    if (offsetScope.Y != 0)
                    {
                        this.OffsetY += value * ScrollWheelSpeed;
                        Handle();
                    }
                    else if (offsetScope.X != 0)
                    {
                        this.OffsetX += value * ScrollWheelSpeed;
                        Handle();
                    }
                }
            }
        }
		public RECT GetOffsetView(RECT baseView)
		{
			baseView.X -= offset.X;
			baseView.Y -= offset.Y;
			baseView.Width += offsetScope.X;
			baseView.Height += offsetScope.Y;
			return baseView;
		}
        protected override bool IsNeedUpdateContent()
        {
            return base.IsNeedUpdateContent() || (contentScope.X == 0 && contentScope.Y == 0);
        }
		protected virtual void InternalContentSizeChanged(VECTOR2 size)
		{
			this.UpdateScrollScope(false);
			this.InternalUpdateScrollBar();
		}
		internal void UpdateScrollBar()
		{
			if (needUpdateScrollBar)
				return;

			needUpdateScrollBar = true;
			NeedUpdateLocalToWorld = true;
		}
		protected void InternalUpdateScrollBar()
		{
			if (!needUpdateScrollBar)
				return;

			if (scrollBarHorizontal != null)
			{
				scrollBarHorizontal.Visible = false;
			}
			if (scrollBarVertical != null)
			{
				scrollBarVertical.Visible = false;
			}

			bool fixedH = ShowScrollHorizontal;
			bool fixedV = ShowScrollVertical;

			if (fixedH)
			{
				scrollBarHorizontal.Visible = true;
				scrollBarHorizontal.MinValue = 0;
				scrollBarHorizontal.MaxValue = offsetScope.X;
				if (!scrollBarSizeFixed)
				{
					float percent = Width / (offsetScope.X + Width);
					scrollBarHorizontal.Body.Width = scrollBarHorizontal.BarViewClip.Width * percent;
				}
				scrollBarHorizontal.Value = OffsetX;
                //scrollBarHorizontal.IsClip = false;
				scrollBarHorizontal.ValueChanged -= DoScrollX;
				scrollBarHorizontal.ValueChanged += DoScrollX;
				scrollBarHorizontal.Body.Visible = CanScrollHorizontal;
				scrollBarHorizontal.NeedUpdateLocalToWorld = true;
			}

			if (fixedV)
			{
				scrollBarVertical.Visible = true;
				scrollBarVertical.MinValue = 0;
				scrollBarVertical.MaxValue = offsetScope.Y;
				if (!scrollBarSizeFixed)
				{
					float percent = Height / (offsetScope.Y + Height);
					scrollBarVertical.Body.Height = scrollBarVertical.BarViewClip.Height * percent;
				}
				scrollBarVertical.Value = OffsetY;
                //scrollBarVertical.IsClip = false;
				scrollBarVertical.ValueChanged -= DoScrollY;
				scrollBarVertical.ValueChanged += DoScrollY;
				scrollBarVertical.Body.Visible = CanScrollVertical;
				scrollBarVertical.NeedUpdateLocalToWorld = true;
			}

			if (ScrollBarChanged != null)
			{
				ScrollBarChanged(this, Entry.Instance);
			}

			needUpdateScrollBar = false;
		}
		private void UpdateScrollScope(bool stayValue)
		{
            VECTOR2 content = ContentSize;
			VECTOR2 temp = offsetScope;
			offsetScope.X = _MATH.Max(content.X - Width, 0);
			offsetScope.Y = _MATH.Max(content.Y - Height, 0);
			if (offsetScope.X > 0 && ShowScrollVertical)
				offsetScope.X += scrollBarVertical.Width;
			if (offsetScope.Y > 0 && ShowScrollHorizontal)
				offsetScope.Y += scrollBarHorizontal.Height;
			UpdateScrollBar();
            //if (!offsetScope.Equals(ref temp))
            if (offsetScope != temp)
            {
                if (stayValue)
                    Offset = VECTOR2.Multiply(new VECTOR2(
                        temp.X == 0 ? 0 : offset.X / temp.X,
                        temp.Y == 0 ? 0 : offset.Y / temp.Y)
                        , offsetScope);
                else
                    Offset = offset;
            }
		}
		private void OnScroll()
		{
            //Handle();
			NeedUpdateLocalToWorld = true;
			if (scrollBarHorizontal != null && CanScrollHorizontal)
			{
				scrollBarHorizontal.Percent = OffsetX / offsetScope.X;
			}
			if (scrollBarVertical != null && CanScrollVertical)
			{
				scrollBarVertical.Percent = OffsetY / offsetScope.Y;
			}
			if (Scroll != null)
			{
				Scroll(this, Entry.Instance);
			}
		}
		public virtual void DoDrag(UIElement sender, Entry e)
		{
			if (DragMode != EDragMode.None)
			{
                Inertia.X = 0;
                Inertia.Y = 0;
                VECTOR2 delta = e.INPUT.Pointer.DeltaPosition;
                if (!delta.IsNaN() && (delta.X != 0 || delta.Y != 0))
                {
                    switch (DragMode)
                    {
                        case EDragMode.Drag:
                            if (offsetScope.X == 0 && offsetScope.Y == 0)
                                return;
                            Offset = VECTOR2.Subtract(Offset, delta);
                            Handle();
                            break;

                        case EDragMode.Move:
                            bool bx, by;
                            DoMove(delta.X, delta.Y, out bx, out by);
                            Handle();
                            break;
                    }
                    if (DragInertia != 0)
                    {
                        Inertia = delta * DragInertia;
                    }
                }
			}
		}
        /// <summary>移动面板</summary>
        /// <param name="moveX">X移动量</param>
        /// <param name="moveY">Y移动量</param>
        /// <param name="boundaryX">X是否已到边界</param>
        /// <param name="boundaryY">Y是否已到边界</param>
        protected void DoMove(float moveX, float moveY, out bool boundaryX, out bool boundaryY)
        {
            VECTOR2 limit;
            if (Parent == null)
                limit = Entry._GRAPHICS.GraphicsSize;
            else
                limit = Parent.Size;
            VECTOR2 center = Size * 0.5f;
            center.X *= 1 - PivotAlignmentX;
            center.Y *= 1 - PivotAlignmentY;
            limit.X -= center.X;
            limit.Y -= center.Y;

            VECTOR2 temp = Location;
            temp.X = _MATH.Clamp(temp.X + moveX, -center.X, limit.X);
            temp.Y = _MATH.Clamp(temp.Y + moveY, -center.Y, limit.Y);
            Location = temp;

            boundaryX = temp.X == -center.X || temp.X == limit.X;
            boundaryY = temp.Y == -center.Y || temp.Y == limit.Y;
        }
        public void SetInertiaTarget(VECTOR2 localPositionTarget)
        {
            if (dragFriction == 0 || dragFriction == 1) return;

            if (DragMode == EDragMode.None) return;
            VECTOR2 start;
            if (DragMode == EDragMode.Drag)
            {
                start = localPositionTarget - Size * 0.5f;
                if (start.X < 0) start.X = 0;
                if (start.Y < 0) start.Y = 0;
                if (start.X > offsetScope.X) start.X = offsetScope.X;
                if (start.Y > offsetScope.Y) start.Y = offsetScope.Y;
                localPositionTarget = Offset;
            }
            else
                start = Location;

            float distance = VECTOR2.Distance(localPositionTarget, start);
            // 等比数列求和公式S = (a1 - an * q) / (1 - q)，这里反求a1
            float endSpeed = distance * (1 - dragFriction) + (INERTIA_STOP * dragFriction);

            float radian;
            VECTOR2.Radian(ref start, ref localPositionTarget, out radian);
            CIRCLE.ParametricEquation(endSpeed, radian, out this.Inertia);
        }
		//protected override void UpdateChilds(IEnumerable<UIElement> childs)
		//{
		//	UpdateScrollScope();
		//}
		private void DoScrollX(Slider sender, Entry e)
		{
			OffsetX = offsetScope.X * sender.Percent;
		}
		private void DoScrollY(Slider sender, Entry e)
		{
			OffsetY = offsetScope.Y * sender.Percent;
		}
		protected override void UpdateClip(ref RECT finalClip, ref RECT finalViewClip, ref MATRIX2x3 transform, ref MATRIX2x3 localToWorld)
		{
			base.UpdateClip(ref finalClip, ref finalViewClip, ref transform, ref localToWorld);
			localToWorld.M31 -= OffsetX;
			localToWorld.M32 -= OffsetY;
            UIScene scene = SceneIsRunning;
            if (scene != null && scene.UseFlowLayout)
            {
                finalClip.Height += Flow.Bottom - finalClip.Bottom;
            }
		}
        protected override void InternalUpdate(Entry e)
        {
            base.InternalUpdate(e);
            InternalUpdateScrollBar();
            if (DragMode != EDragMode.None && !IsClick && (Inertia.X != 0 || Inertia.Y != 0))
            {
                VECTOR2 temp;
                // 移动惯性，碰撞到边界则反弹
                switch (DragMode)
                {
                    case EDragMode.Drag:
                        temp = Offset;
                        temp.X -= Inertia.X;
                        temp.Y -= Inertia.Y;
                        if (temp.X < 0 || temp.X > offsetScope.X)
                            Inertia.X = -Inertia.X * Rebound;
                        if (temp.Y < 0 || temp.Y > offsetScope.Y)
                            Inertia.Y = -Inertia.Y * Rebound;
                        Offset = temp;
                        break;

                    case EDragMode.Move:
                        bool bx, by;
                        DoMove(Inertia.X, Inertia.Y, out bx, out by);
                        if (bx)
                            Inertia.X = -Inertia.X * Rebound;
                        if (by)
                            Inertia.Y = -Inertia.Y * Rebound;
                        break;
                }
                Inertia.X *= dragFriction;
                if (_MATH.Abs(Inertia.X) <= INERTIA_STOP)
                    Inertia.X = 0;
                Inertia.Y *= dragFriction;
                if (_MATH.Abs(Inertia.Y) <= INERTIA_STOP)
                    Inertia.Y = 0;
            }
        }
		protected override void DrawBegin(GRAPHICS spriteBatch, ref MATRIX2x3 transform, ref RECT view, SHADER shader)
		{
			needBeginEnd = NeedBeginEnd;
			if (needBeginEnd)
			{
				spriteBatch.Begin(spriteBatch.FromPreviousNonOffset(view));
			}
			base.DrawBegin(spriteBatch, ref transform, ref view, shader);
		}
		protected override void DrawEnd(GRAPHICS spriteBatch, ref MATRIX2x3 transform, ref RECT view, SHADER shader)
		{
			base.DrawEnd(spriteBatch, ref transform, ref view, shader);
			if (needBeginEnd)
			{
				spriteBatch.End();
			}
		}
		protected override void InternalDraw(GRAPHICS spriteBatch, Entry e)
		{
            if (Background != null)
            {
                spriteBatch.Draw(Background, ViewClip, Color);
            }
            if (BackgroundFull != null)
            {
                spriteBatch.Draw(BackgroundFull, FullViewClip, Color);
            }
		}
		public override void Dispose()
		{
			base.Dispose();
            Background = null;
            BackgroundFull = null;
		}
        public override IEnumerator<UIElement> GetEnumerator()
        {
            foreach (var item in Childs.ToArray())
                if (item != scrollBarHorizontal && item != scrollBarVertical)
                    yield return item;
        }

        //protected static RECT PanelChildClip(UIElement child)
        //{
        //    if (!child.Visible)
        //        return RECT.Empty;
        //    if (child.IsAutoClip)
        //        return child.ClipInParent;
        //    return RECT.Union(child.ClipInParent, child.ChildClip);
        //}
	}

    public class ListView<DataType, ElementType> : IDisposable where ElementType : UIElement
    {
        private Panel panel;
        private Pool<ElementType> pools = new Pool<ElementType>();
        public event Func<ElementType> OnCreateElement;
        public event Action<ElementType, int, DataType> OnSetData;
        //public event Action<ElementType> OnRemoveElement;

        public List<DataType> Datas
        {
            get;
            private set;
        }
        public IEnumerable<ElementType> Elements
        {
            get
            {
                for (int i = 0; i < panel.ChildCount; i++)
                    yield return (ElementType)panel[i];
            }
        }
        public Panel Panel
        {
            get { return panel; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("panel");

                if (value == panel)
                    return;

                if (panel != null)
                {
                    panel.Scroll -= Scroll;
                    //panel.Hover -= HoverScroll;
                    Dispose();
                }

                this.panel = value;

                panel.Scroll += Scroll;
                //panel.Hover += HoverScroll;
                var datas = Datas;
                Datas = null;
                SetDataSource(datas);
            }
        }

        public ListView(Panel panel)
        {
            Datas = new List<DataType>();
            this.Panel = panel;
        }

        private void Scroll(Panel sender, Entry e)
        {
            float offset = panel.OffsetY;
            int count = panel.ChildCount;
            int i;
            for (i = 0; i < count; i++)
            {
                float bottom = panel[i].Y + panel[i].Height;
                //y += Panel[i].Height;
                if (bottom > offset)
                {
                    break;
                }
                panel[i].Visible = false;
            }
            offset += panel.Height;
            for (; i < count; i++)
            {
                panel[i].Visible = panel[i].Y < offset;
            }
        }
        private void ResetContentScope()
        {
            // 上面的Scroll方法会自动隐藏掉看不见的元素，导致Panel.ContentSize变小，自动适配滚动区域则会出错
            if (panel.ChildCount == 0)
                panel.ContentScope = VECTOR2.Zero;
            else
                panel.ContentScope = new VECTOR2(0, panel.Last.Clip.Bottom);
        }
        public void SetDataSource(List<DataType> value)
        {
            //if (Datas == value)
            //    return;

            if (value == null) value = new List<DataType>();
            Close();

            //Panel.Offset = VECTOR2.Zero;
            Panel.ContentScope = VECTOR2.Zero;

            int count = value.Count;
            for (int i = 0; i < count; i++)
                AddData(value[i]);
            Datas = value;
            //Panel.ContentScope = new VECTOR2(0, y);
        }
        public ElementType AddData(DataType data)
        {
            var element = pools.Allot();
            if (element == null)
            {
                if (OnCreateElement == null)
                    element = Activator.CreateInstance<ElementType>();
                else
                {
                    element = OnCreateElement();
                    if (element == null)
                        throw new ArgumentNullException("element");
                }
            }
            element.Y = element.Height * Panel.ChildCount;
            if (OnSetData != null)
                OnSetData(element, Panel.ChildCount, data);
            Panel.Add(element);
            ResetContentScope();

            Datas.Add(data);

            return element;
        }
        public bool Remove(int index)
        {
            if (index < 0 || index >= Datas.Count)
                return false;
            Datas.RemoveAt(index);
            var temp = panel[index];
            pools.Remove((ElementType)temp);
            panel.Remove(index);
            if (index < Datas.Count)
            {
                for (int i = index; i < Datas.Count; i++)
                {
                    // 如果元素高度各不相同时，这样会导致坐标不正确
                    panel[i].Y -= panel[i].Height;
                    if (OnSetData != null)
                        OnSetData((ElementType)panel[i], i, Datas[i]);
                }
            }
            ResetContentScope();
            Scroll(null, null);
            return true;
        }
        public void Close()
        {
            //if (OnRemoveElement != null)
            //    foreach (var item in pools)
            //        OnRemoveElement(item);
            pools.ClearToFree();
            if (panel != null)
                Panel.Clear();
        }
        public void Dispose()
        {
            Close();
            pools.Clear();
        }
    }
}

#endif