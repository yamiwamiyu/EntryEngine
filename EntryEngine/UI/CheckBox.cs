#if CLIENT

using System;
using System.Collections.Generic;
using System.Linq;

namespace EntryEngine.UI
{
	public class CheckBox : Button
	{
		public bool IsRadioButton;
		public bool CheckedOverlayNormal;
        public event Action<CheckBox> GroupSelectionChanged;

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
		public IEnumerable<CheckBox> GroupSelection
		{
			get
			{
                return Group.Where(g => g.Checked);
			}
		}
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
        public CheckBox GroupSelected
        {
            get
            {
                //if (!IsRadioButton)
                //    return null;
                return Group.FirstOrDefault(g => g.Checked);
            }
        }
        public int GroupSelectedIndex
        {
            get
            {
                if (!IsRadioButton)
                    return -1;

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
            if (UIText.Font == null || string.IsNullOrEmpty(Text))
                return RECT.Empty;

            RECT textClip;
            RECT clip = Clip;

            VECTOR2 fontSize = UIText.Font.MeasureString(Text);
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

            TEXTURE current = this[state];
            if (current != null)
            {
				spriteBatch.Draw(current, ViewClip, Color);
            }
            if (SourceClicked != null && Checked && state != EButtonState.Clicked && CheckedOverlayNormal)
			{
                spriteBatch.Draw(SourceClicked, ViewClip, Color);
            }

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