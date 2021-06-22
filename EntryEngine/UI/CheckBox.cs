#if CLIENT

using System;
using System.Collections.Generic;
using System.Linq;

namespace EntryEngine.UI
{
    /// <summary>选中框覆盖类型</summary>
    public enum ECheckedOverlay : sbyte
    {
        普通覆盖选中 = -1,
        不覆盖,
        选中覆盖普通 = 1,
    }
    /// <summary>单选 / 多选</summary>
	public class CheckBox : Button
	{
        /// <summary>true: 单选框 / false: 多选框</summary>
		public bool IsRadioButton;
        /// <summary>不为0: 选中状态下，显示普通和选中两个状态的图片</summary>
        public ECheckedOverlay CheckedOverlayNormal;
        public event Action<CheckBox> GroupSelectionChanged;

        /// <summary>在相同父级中的框</summary>
		public IEnumerable<CheckBox> Group
		{
			get
			{
				if (Parent == null)
					yield break;

				foreach (UIElement e in Parent)
				{
					CheckBox group = e as CheckBox;
					if (group != null && group.IsRadioButton == this.IsRadioButton)
					{
						yield return group;
					}
				}
			}
		}
        /// <summary>在相同父级中被选中的框</summary>
		public IEnumerable<CheckBox> GroupSelection
		{
			get
			{
                return Group.Where(g => g.Checked);
			}
		}
        /// <summary>在相同父级中被选中的框在父级中的索引</summary>
        public IEnumerable<int> GroupSelectionIndexes
        {
            get
            {
                int index = 0;
                foreach (var item in Group)
                {
                    if (item.Checked)
                        yield return index;
                    index++;
                }
            }
        }
        /// <summary>在相同父级中首个被选中的框，没有选中时返回null</summary>
        public CheckBox GroupSelected
        {
            get
            {
                //if (!IsRadioButton)
                //    return null;
                return Group.FirstOrDefault(g => g.Checked);
            }
        }
        /// <summary>在相同父级中首个被选中的框的索引，没有选中时返回-1</summary>
        public int GroupSelectedIndex
        {
            get
            {
                //if (!IsRadioButton)
                //    return -1;

                int index = 0;
                foreach (var item in Group)
                {
                    if (item.Checked)
                        return index;
                    index++;
                }

                return -1;
            }
        }
        /// <summary>是否选中的状态</summary>
		public override bool Checked
		{
			get
			{
				return base.Checked;
			}
			set
			{
				if (base.Checked == value)
				{
					return;
				}

				if (Parent != null)
				{
					if (IsRadioButton && value)
					{
						foreach (CheckBox e in Group)
						{
                            if (e.Checked)
                            {
                                e.InternalSetChecked(false);
                            }
						}
					}
				}

                //base.Checked = value;
                InternalSetChecked(value);

                foreach (var item in Group)
                {
                    if (item.GroupSelectionChanged != null)
                    {
                        item.GroupSelectionChanged(item);
                    }
                }
			}
		}
        public override RECT ChildClip
        {
            get
            {
                if (Childs.Count != 0)
                    return base.ChildClip;

                var textClip = CalcTextClip();
                var clip = Clip;

                textClip.X += clip.X;
                textClip.Y += clip.Y;

                RECT.Union(ref textClip, ref clip, out clip);
                return clip;
            }
        }
        public RECT ViewTextClip
        {
            get
            {
                var clip = CalcTextClip();

                var view = ViewClip;
                clip.X += view.X;
                clip.Y += view.Y;

                return clip;
            }
        }
        public override EUIType UIType
        {
            get { return EUIType.CheckBox; }
        }

		public CheckBox()
		{
			this.UIText.TextAlignment = EPivot.MiddleRight;
		}

        protected RECT CalcTextClip()
        {
            if (UIText.Font == null || string.IsNullOrEmpty(UIText.Text))
                return RECT.Empty;

            RECT textClip;
            RECT clip = Clip;

            VECTOR2 fontSize = UIText.Font.MeasureString(UIText.Text);
            int x = PivotX(UIText.TextAlignment);
            int y = PivotY(UIText.TextAlignment);
            if (x == 1)
                textClip.X = (clip.Width - fontSize.X) / 2;
            else
                textClip.X = fontSize.X * (x - 1);
            if (y == 1)
                textClip.Y = (clip.Height - fontSize.Y) / 2;
            else
                textClip.Y = clip.Height * y + fontSize.Y * (y - 1);
            textClip.Width = fontSize.X;
            textClip.Height = fontSize.Y;

            if (x != 1 && y == 1)
            {
                textClip.X = x == 0 ? -fontSize.X : clip.Width;
                textClip.Y = (clip.Height - fontSize.Y) / 2;
            }
            else
            {
                textClip.X = (clip.Width - fontSize.X) / 2 * x;
                textClip.Y = clip.Height * (y / 2.0f) + fontSize.Y * (y - 2) / 2.0f;
            }

            return textClip;
        }
        public override bool IsContains(VECTOR2 screenPosition)
        {
            return ViewTextClip.Contains(screenPosition) || base.IsContains(screenPosition);
        }
        protected virtual void InternalSetChecked(bool value)
        {
            base.Checked = value;
        }
		protected override bool OnCheckedChanging()
		{
			return IsRadioButton || !Checked;
		}
        protected override void InternalDraw(GRAPHICS spriteBatch, Entry e)
        {
            EButtonState state = ButtonState;

            bool drawChecked = SourceClicked != null && Checked;
            if (drawChecked && CheckedOverlayNormal == ECheckedOverlay.普通覆盖选中)
                spriteBatch.Draw(SourceClicked, ViewClip, Color);

            TEXTURE current = this[drawChecked && CheckedOverlayNormal != ECheckedOverlay.不覆盖 ? EButtonState.Normal : state];
            if (current != null)
				spriteBatch.Draw(current, ViewClip, Color);

            if (drawChecked && CheckedOverlayNormal == ECheckedOverlay.选中覆盖普通)
                spriteBatch.Draw(SourceClicked, ViewClip, Color);

            DrawFont(spriteBatch, e);
        }
        protected override void DrawFont(GRAPHICS spriteBatch, Entry e)
        {
            if (UIText.Font != null && !string.IsNullOrEmpty(Text))
                UIText.Draw(spriteBatch, ViewTextClip);
        }
	}
}

#endif