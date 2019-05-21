#if CLIENT

using System;
using System.Collections.Generic;
using System.Linq;

namespace EntryEngine.UI
{
    public class TabPage : CheckBox
    {
        private UIElement page;

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
    public class DropDown : Button
    {
        private Button dropDownText;
        private Selectable dropDownList;
        public event Action<DropDown> Expanded;
        public event Action<DropDown> Collapsed;
        
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
                    dropDownText.Text = value;
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
                Button button = item as Button;
                if (button != null)
                    Text = button.Text;
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
            dropDownList.Height = height;
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

            if (IsHover && e.INPUT.Pointer.IsClick(0))
            {
                Checked = !Checked;
                Handle();
            }
            else if (dropDownText != null && dropDownText.IsHover && e.INPUT.Pointer.IsClick(0))
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
    public class Selectable : UIScene
    {
        private int selectedIndex = -1;
        public event Action<Selectable, int> Select;
        public event Action<Selectable> SelectedIndexChanged;
        public event Action<UIElement, float> ResetLayout;
        /// <summary>返回true时会选中Hover的项</summary>
        public event Func<bool> SelectHandle;
        public event Func<string, Button> CreateItem;
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
        public UIElement Selected
        {
            get
            {
                if (selectedIndex == -1)
                    return null;
                return GetItem<UIElement>(selectedIndex);
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

        public Button AddItem(string text)
        {
            if (CreateItem != null)
            {
                Button create = CreateItem(text);
                if (create != null) return create;
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
            var item = GetItem<UIElement>(index);
            if (item == null)
                return false;
            return Remove(item);
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
    }
}

#endif