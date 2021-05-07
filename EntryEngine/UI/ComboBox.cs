#if CLIENT

using System;
using System.Collections.Generic;
using System.Linq;

namespace EntryEngine.UI
{
    /// <summary>标签分页导航</summary>
    public class TabPage : CheckBox
    {
        private UIElement page;

        /// <summary>标签页被选中时需要显示的控件</summary>
        public UIElement Page
        {
            get { return page; }
            set
            {
                if (page == value)
                    return;

                if (page != null && page.Parent == this)
                    Remove(page);

                page = value;
                if (value != null)
                {
                    if (page.Parent == null)
                        Add(page);
                    page.Visible = base.Checked;
                    //page.IsClip = false;
                }
            }
        }
        public override EUIType UIType
        {
            get { return EUIType.TabPage; }
        }

        public TabPage()
        {
            this.IsClip = false;
            this.IsRadioButton = true;
            this.UIText.TextAlignment = EPivot.MiddleCenter;
        }

        protected override void InternalSetChecked(bool value)
        {
            base.InternalSetChecked(value);
            if (page != null)
                page.Visible = base.Checked;
        }
        public override IEnumerator<UIElement> GetEnumerator()
        {
            foreach (var item in Childs.ToArray())
                if (item != page)
                    yield return item;
        }
    }
    /// <summary>下拉框</summary>
    public class DropDown : CheckBox
    {
        private Button dropDownText;
        private Selectable dropDownList;
        /// <summary>下拉框展开时触发</summary>
        public event Action<DropDown> Expanded;
        /// <summary>下拉框折叠时触发</summary>
        public event Action<DropDown> Collapsed;

        /// <summary>下拉框是否被展开</summary>
        public override bool Checked
        {
            get
            {
                if (dropDownList != null)
                    return dropDownList.Visible;

                //if (dropDownText != null)
                //    return dropDownText.Checked;

                return false;
            }
            set
            {
                if (dropDownList != null)
                    dropDownList.Visible = value;

                if (dropDownText != null)
                    dropDownText.Checked = value;

                base.Checked = value;
                if (value)
                {
                    //ToFront();
                    if (dropDownList != null)
                    {
                        Entry.Instance.ShowDialogScene(dropDownList, EState.None);
                        dropDownList.ToFront();
                    }
                    if (Expanded != null)
                        Expanded(this);
                }
                else
                {
                    if (Collapsed != null)
                        Collapsed(this);
                }
            }
        }
        /// <summary>下拉框显示的文字</summary>
        public Button DropDownText
        {
            get { return dropDownText; }
            set
            {
                if (dropDownText == value)
                    return;

                if (dropDownText != null)
                {
                    if (dropDownList.Parent == this)
                        Remove(dropDownText);
                    if (value == null)
                        base.Text = dropDownText.Text;
                }

                dropDownText = value;
                if (value != null && value.Parent == null)
                    Add(dropDownText);
            }
        }
        /// <summary>下拉选项列表</summary>
        public Selectable DropDownList
        {
            get { return dropDownList; }
            set
            {
                if (dropDownList == value)
                    return;

                if (dropDownList != null)
                {
                    dropDownList.SelectHandle -= OnSelectHandle;
                    dropDownList.Select -= OnSelect;
                    dropDownList.UnHover -= OnCancelHandle;
                    if (dropDownList.Parent == this)
                        Remove(dropDownList);
                }

                dropDownList = value;
                if (value != null)
                {
                    if (dropDownList.Parent == null)
                        Add(dropDownList);
                    //dropDownList.IsClip = false;
                    dropDownList.DragMode = EDragMode.Drag;
                    dropDownList.SelectHandle += OnSelectHandle;
                    dropDownList.Select += OnSelect;
                    dropDownList.UnHover += OnCancelHandle;
                    if (dropDownText != null)
                        dropDownList.Visible = dropDownText.Checked;
                    else
                        dropDownList.Visible = base.Checked;
                }
            }
        }
        /// <summary>下拉项的数量</summary>
        public int ItemCount
        {
            get
            {
                if (dropDownList == null)
                    return 0;
                return dropDownList.ChildCount
                    - (dropDownList.ScrollBarVertical == null ? 0 : 1)
                    - (dropDownList.ScrollBarHorizontal == null ? 0 : 1);
            }
        }
        /// <summary>显示选中项的文字</summary>
        public override string Text
        {
            get
            {
                if (dropDownText == null)
                    return base.Text;
                return dropDownText.Text;
            }
            set
            {
                if (dropDownText != null)
                {
                    dropDownText.Text = value;
                    base.Text = string.Empty;
                }
                else
                    base.Text = value;
            }
        }
        public override EUIType UIType
        {
            get { return EUIType.DropDown; }
        }

        public DropDown()
        {
            this.IsClip = false;
            Clicked -= OnClicked;
            IsRadioButton = true;
            UIText.TextAlignment = EPivot.MiddleCenter;
        }

        private void OnCancelHandle(UIElement sender, Entry e)
        {
            if (Checked && !dropDownList.IsHover && e.INPUT.Pointer.IsClick(0))
            {
                // 点击面板以外收起面板
                Checked = false;
                Handle();
            }
        }
        private bool OnSelectHandle()
        {
            return Checked && dropDownList.IsHover
                    && __INPUT.PointerIsRelease(0)
                    && (dropDownList.IsClick || this.IsClick || (dropDownText != null && dropDownText.IsClick));
        }
        private void OnSelect(Selectable selectable, int index)
        {
            var item = selectable.Selected;
            if (item != null)
            {
                Text = item.Text;
            }
            else
            {
                Text = null;
            }
            Checked = false;
        }
        public void SetDefaultControl(float width, float height)
        {
            this.Width = width;
            this.Height = height;

            DropDownText = new Button();
            dropDownText.Width = width;
            dropDownText.Height = height;

            DropDownList = new Selectable();
            dropDownList.Width = width;
            dropDownList.Height = 0;
            dropDownList.Anchor = EAnchor.Left | EAnchor.Right | EAnchor.Top;
            dropDownList.Y = height;
            dropDownList.DragMode = EDragMode.Drag;
        }
        protected override void InternalEvent(Entry e)
        {
            base.InternalEvent(e);

            /*
             * 没有DropDownList：不响应事件
             * 只有DropDownList：响应选择
             * 两个都有：响应下拉/收起，响应选择
             */
            //if (dropDownList == null || (dropDownText == null && !dropDownList.Visible) || Handled)
            if (dropDownList == null || Handled)
                return;

            if (e.INPUT.Pointer.IsClick(0) &&
                ((dropDownText != null && dropDownText.IsHover) || IsHover))
            {
                Checked = !Checked;
                Handle();
            }
        }
        public override IEnumerator<UIElement> GetEnumerator()
        {
            foreach (var item in Childs.ToArray())
                if (item != dropDownText && item != dropDownList)
                    yield return item;
        }
    }
    //public class MenuStrip : DropDown
    //{
    //    public MenuStrip()
    //    {
    //        this.Hover += MenuStrip_Hover;
    //    }

    //    void MenuStrip_Hover(UIElement sender, Entry e)
    //    {
    //        if (!this.Checked && this.Group.Any(c => c.Checked))
    //        {
    //            this.Checked = true;
    //        }
    //    }
    //}
    public interface ISelectable
    {
        bool Selected { get; set; }
    }
    /// <summary>可选择的列表</summary>
    public class Selectable : UIScene
    {
        private int selectedIndex = -1;
        public event Action<Selectable, int> Select;
        public event Action<Selectable> SelectedIndexChanged;
        public event Action<UIElement, float> ResetLayout;
        /// <summary>返回true时会选中Hover的项</summary>
        public event Func<bool> SelectHandle;
        public event Func<Selectable, string, Button> CreateItem;
        private float maxHeight;

        public override float Height
        {
            get
            {
                return base.Height;
            }
            set
            {
                base.Height = value;
                this.maxHeight = value;
            }
        }
        public int ItemCount
        {
            get
            {
                return ChildCount
                       - (ScrollBarVertical == null ? 0 : 1)
                       - (ScrollBarHorizontal == null ? 0 : 1);
            }
        }
        /// <summary>选中项的索引</summary>
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                value = _MATH.Clamp(value, -1, ItemCount - 1);

                bool changed = value != selectedIndex;
                if (changed)
                {
                    // unchecked previous item
                    var previous = GetItem<Button>(selectedIndex);
                    if (previous != null)
                        previous.Checked = false;

                    if (value != -1)
                    {
                        // check new item
                        var item = GetItem<Button>(value);
                        if (item != null)
                            item.Checked = true;
                    }

                    selectedIndex = value;
                }

                if (Select != null)
                    Select(this, value);

                if (changed && SelectedIndexChanged != null)
                    SelectedIndexChanged(this);
            }
        }
        /// <summary>选中项</summary>
        public Button Selected
        {
            get
            {
                if (selectedIndex == -1)
                    return null;
                return GetItem<Button>(selectedIndex);
            }
        }
        public override EUIType UIType
        {
            get { return EUIType.Selectable; }
        }

        public Selectable()
        {
            RegistEvent(DoSelectHandle);
        }

        public void DoSelect(int index)
        {
            this.selectedIndex = -1;
            this.SelectedIndex = index;
        }
        public Button AddItem(string text)
        {
            if (CreateItem != null)
            {
                Button create = CreateItem(this, text);
                if (create != null)
                {
                    Add(create);
                    return create;
                }
            }
            Button button = new Button();
            button.Width = this.Width;
            button.Text = text;
            button.Height = button.UIText.Font.LineHeight;
            button.UIText.TextAlignment = EPivot.MiddleLeft;
            //button.Clicked -= button.OnClicked;
            button.Eventable = false;
            Add(button);
            return button;
        }
        public bool RemoveItem(int index)
        {
            if (index < 0 || index >= ChildCount) return false;
            Remove(index);
            return true;
        }
        public bool RemoveItem(string text)
        {
            foreach (var item in GetItems<Button>())
            {
                if (item.Text == text)
                {
                    Remove(item);
                    return true;
                }
            }
            return false;
        }
        public void RemoveCurrent()
        {
            var selected = Selected;
            if (selected != null)
                Remove(selected);
        }
        protected override void OnAdded(UIElement node, int index)
        {
            base.OnAdded(node, index);
            if (node == ScrollBarHorizontal && node == ScrollBarVertical)
                return;
            ResetItemLayout();
        }
        protected override void OnRemoved(UIElement node)
        {
            base.OnRemoved(node);

            if (node == ScrollBarHorizontal && node == ScrollBarVertical)
                return;

            ResetItemLayout();
            int count = ItemCount;
            if (selectedIndex >= count)
            {
                selectedIndex = -1;
                SelectedIndex = count - 1;
            }
            else
            {
                // 重新选中当前项
                int current = selectedIndex;
                selectedIndex = -1;
                SelectedIndex = current;
            }
        }
        public void ResetItemLayout()
        {
            float y = 0;
            foreach (var item in this)
            {
                item.X = 0;
                item.Y = y;
                if (ResetLayout != null)
                    ResetLayout(item, y);
                y += item.Height;
            }
        }
        public T GetItem<T>(int index) where T : UIElement
        {
            foreach (var child in this)
            {
                T item = child as T;
                if (item != null && index-- == 0)
                    return item;
            }
            return null;
        }
        public IEnumerable<T> GetItems<T>() where T : UIElement
        {
            return this.Where(i => i is T).Select(i => i as T);
        }
        private void DoSelectHandle(Entry e)
        {
            if (isHover)
            {
                // block
                bool handle = false;
                if (SelectHandle != null)
                    handle = SelectHandle();
                if (e.INPUT.Pointer.IsClick(0))
                {
                    Handle();
                    return;
                }
                else if (e.INPUT.Pointer.IsTap() || handle)
                {
                    // select
                    int index = 0;
                    foreach (var item in this)
                    {
                        if (item.IsHover)
                        {
                            Handle();
                            SelectedIndex = index;
                            return;
                        }
                        index++;
                    }
                }
            }
        }
        //protected override void InternalEvent(Entry e)
        //{
        //    base.InternalEvent(e);

        //    if (isHover)
        //    {
        //        // block
        //        bool handle = false;
        //        if (SelectHandle != null)
        //            handle = SelectHandle();
        //        if (e.INPUT.Pointer.IsClick(0))
        //        {
        //            Handled = true;
        //            return;
        //        }
        //        else if (e.INPUT.Pointer.IsTap() || handle)
        //        {
        //            // select
        //            int index = 0;
        //            foreach (var item in this)
        //            {
        //                if (item.IsHover)
        //                {
        //                    Handled = true;
        //                    SelectedIndex = index;
        //                    return;
        //                }
        //                index++;
        //            }
        //        }
        //    }
        //}
        protected override void InternalUpdate(Entry e)
        {
            base.InternalUpdate(e);
            if (ItemCount > 0)
            {
                // 当项高不足时，只显示项高度
                float height = 0;
                foreach (var item in this)
                    height += item.Height;
                if (maxHeight > 0 && height < maxHeight)
                    base.Height = height;
                else
                    base.Height = maxHeight;
            }
        }

        /// <summary>Panel的子控件必须实现ISelectable</summary>
        public static void ListViewSelect(Panel panel)
        {
            // 最后一次是选中还是取消选中，Ctrl+Shift选择时会范围进行选中/取消选中
            bool lastIsSelect = true;
            // 最后一次操作的项的索引，Shift时，从这个索引网目标索引进行选中/取消选中
            int lastSelectIndex = -1;
            VECTOR2 clickOffsetPosition = VECTOR2.Zero;
            bool areaSelect = false;
            Action cancelSelect = () =>
            {
                foreach (ISelectable item in panel)
                {
                    if (item.Selected)
                        item.Selected = false;
                }
            };
            Action<Entry> clickSelect = (e) =>
            {
                if (areaSelect) return;
                int current = -1;
                for (int i = 0; i < panel.ChildCount; i++)
                    if (panel[i].IsHover)
                    {
                        current = i;
                        break;
                    }
                if (current == -1)
                {
                    // 左键点到面板内的空白处
                    if (!e.INPUT.Keyboard.Ctrl && !e.INPUT.Keyboard.Shift)
                    {
                        // 取消所有选择
                        cancelSelect();
                    }
                }
                else
                {
                    bool ctrl = e.INPUT.Keyboard.Ctrl;
                    bool shift = e.INPUT.Keyboard.Shift;
                    if (shift && ctrl)
                    {
                        // 按住Shift时，选择/取消选择连续项，选择还是取消选择取决于Ctrl的最后一次操作
                        bool isSelect = lastIsSelect;
                        if (lastSelectIndex > current)
                            Utility.Swap(ref lastSelectIndex, ref current);
                        for (int i = _MATH.Max(lastSelectIndex, 0); i <= current; i++)
                        {
                            ISelectable selectable = panel[i] as ISelectable;
                            if (selectable != null && selectable.Selected != isSelect)
                            {
                                selectable.Selected = isSelect;
                            }
                        }
                    }
                    else if (shift)
                    {
                        // 没有按Ctrl时，无论最后是选择还是取消选择都是选择
                        lastIsSelect = true;
                        // 按住Shift时，选择/取消选择连续项
                        if (lastSelectIndex > current)
                            Utility.Swap(ref lastSelectIndex, ref current);
                        for (int i = 0; i < panel.ChildCount; i++)
                        {
                            // 范围内的选中，否则取消选中
                            bool isSelect = i >= lastSelectIndex && i <= current;
                            ISelectable selectable = panel[i] as ISelectable;
                            if (selectable != null && selectable.Selected != isSelect)
                            {
                                selectable.Selected = isSelect;
                            }
                        }
                    }
                    else if (ctrl)
                    {
                        // 加选/取消选择目标项
                        ISelectable selectable = panel[current] as ISelectable;
                        selectable.Selected = !selectable.Selected;
                        lastIsSelect = selectable.Selected;
                        lastSelectIndex = current;
                    }
                    else
                    {
                        if (panel.Any(c => c != panel[current] && ((ISelectable)c).Selected))
                        {
                            cancelSelect();
                            ISelectable selectable = panel[current] as ISelectable;
                            if (!selectable.Selected)
                                selectable.Selected = true;
                            lastIsSelect = true;
                        }
                        else
                        {
                            ISelectable selectable = panel[current] as ISelectable;
                            selectable.Selected = !selectable.Selected;
                            lastIsSelect = selectable.Selected;
                        }
                        lastSelectIndex = current;
                    }
                }
            };
            panel.EventBegin = (sender, e) =>
            {
                // 点击右键清空选中
                if (panel.IsHover && e.INPUT.Pointer.IsClick(1))
                {
                    cancelSelect();
                }
                // 点击清空选中项，选中点击的当前项
                if (panel.IsHover)
                {
                    if (e.INPUT.Pointer.IsClick(0))
                    {
                        clickOffsetPosition = panel.ConvertGraphicsToLocal(e.INPUT.Pointer.ClickPosition);
                        areaSelect = false;
                    }
                    else if (e.INPUT.Pointer.IsRelease(0))
                    {
                        clickSelect(e);
                    }
                }
                // 拖拽连续选中项
                if (panel.IsClick && e.INPUT.Pointer.IsPressed(0) && e.INPUT.Pointer.Position != e.INPUT.Pointer.ClickPosition)
                {
                    areaSelect = true;
                    var p1 = panel.ConvertGraphicsToLocal(e.INPUT.Pointer.Position);
                    RECT clip = RECT.CreateRectangle(p1, clickOffsetPosition);
                    //clip.X += panel.OffsetX;
                    //clip.Y += panel.OffsetY;
                    for (int i = 0; i < panel.ChildCount; i++)
                    {
                        bool isSelect = panel[i].Clip.Intersects(ref clip);
                        ISelectable selectable = panel[i] as ISelectable;
                        if (selectable != null && selectable.Selected != isSelect)
                        {
                            selectable.Selected = isSelect;
                        }
                    }
                    // 拖拽到外部时滚动面板
                    p1.X -= panel.OffsetX;
                    p1.Y -= panel.OffsetY;
                    const float MULTIPLE = 3.0f;
                    if (p1.X < 0) panel.OffsetX += p1.X * MULTIPLE * e.GameTime.ElapsedSecond;
                    else if (p1.X > panel.Width) panel.OffsetX += (p1.X - panel.Width) * MULTIPLE * e.GameTime.ElapsedSecond;
                    if (p1.Y < 0) panel.OffsetY += p1.Y * MULTIPLE * e.GameTime.ElapsedSecond;
                    else if (p1.Y > panel.Height) panel.OffsetY += (p1.Y - panel.Height) * MULTIPLE * e.GameTime.ElapsedSecond;
                }
            };
            PATCH patch = PATCH.GetNinePatch(new COLOR(0, 120, 215, 64), new COLOR(0, 120, 215, 128), 1);
            panel.DrawBeforeEnd = (sender, sb, e) =>
            {
                if (panel.IsClick && e.INPUT.Pointer.IsPressed(0))
                {
                    var p2 = panel.ConvertLocalToGraphics(clickOffsetPosition);
                    sb.Draw(patch, RECT.CreateRectangle(e.INPUT.Pointer.Position, p2));
                }
            };
        }
    }
}

#endif