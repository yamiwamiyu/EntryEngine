#if CLIENT

using System;
namespace EntryEngine.UI
{
	public class Label : Button, ITypist
	{
		private string text = string.Empty;
		private int maxLength = -1;
		private bool breakLine;
        public COLOR CursorColor = InputText.CursorColor;
        public COLOR CursorAreaColor = InputText.CursorAreaColor;
        public event Action<Label> FontChanged;
        public event Action<Label> TextChanged;
        public event Action<Label> TextEditOver;
        public event Action<Label> DisplayChanged;
        private VECTOR2 offset;

		public FONT Font
		{
			get { return UIText.Font; }
			set
			{
				bool changed = Font != value;
				UIText.Font = value;
				ResetDisplay();
				if (changed && FontChanged != null)
					FontChanged(this);
			}
		}
        public float FontSize
        {
            get
            {
                if (Font != null)
                    return Font.FontSize;
                else
                    return 0;
            }
            set
            {
                if (Font == null)
                    return;
                Font.FontSize = value;
                NeedUpdateLocalToWorld = true;
            }
        }
		public override string Text
		{
			get { return text; }
			set
			{
				if (value == null)
					value = string.Empty;
				if (value == text)
					return;
                text = LengthLimit(value);
                if (!Checked && TextChanged != null)
                    TextChanged(this);
                ResetDisplay();
			}
		}
		public bool BreakLine
		{
			get { return breakLine && Font != null && !IsAutoWidth; }
			set
			{
				if (breakLine != value)
				{
					breakLine = value;
					ResetDisplay();
				}
			}
		}
		public string DisplayText
        {
            get { return UIText.Text; }
            protected set
            {
                if (UIText.Text != value)
                {
                    UIText.Text = value;
                    if (DisplayChanged != null)
                    {
                        DisplayChanged(this);
                    }
                }
            }
        }
		public int MaxLength
		{
			get { return maxLength; }
			set
			{
				maxLength = _MATH.Max(value, -1);
				Text = LengthLimit(Text);
			}
		}
		public bool ActiveSelect
		{
			get;
			set;
		}
        string ITypist.Text
        {
            get { return this.Text; }
            set { this.Text = value; }
        }
        bool ITypist.Readonly
        {
            get { return true; }
        }
        bool ITypist.Multiple
        {
            get { return false; }
        }
        bool ITypist.IsMask
        {
            get { return false; }
        }
        RECT ITypist.TextArea
        {
            get { return this.TextArea; }
        }
        public bool IsActive
        {
            get { return FinalEventable && Focused; }
        }
		protected InputText InputDevice
		{
			get { return Entry.Instance.INPUT.InputDevice; }
		}
        public override VECTOR2 TextContentSize
        {
            get { return CalcTextContentSize(DisplayText); }
        }
        public override VECTOR2 ContentSize
        {
            get
            {
                if (Font != null)
                    return TextContentSize;
                return base.ContentSize;
            }
        }
        public override bool CanFocused
        {
            get { return CanSelect; }
        }
        public bool CanSelect
        {
            get;
            set;
        }
        //public override bool Checked
        //{
        //    get { return Focused; }
        //    set { SetFocus(value); }
        //}
        public override RECT TextArea
        {
            get { return base.TextArea - offset; }
        }
        public RECT ViewArea
        {
            get
            {
                RECT clip = ViewClip;
                UIText.GetPaddingClip(ref clip);
                RECT view = FinalViewClip;
                RECT.Intersect(ref clip, ref view, out clip);
                return clip;
            }
        }

		public Label()
		{
			UIText.TextAlignment = EPivot.TopLeft;
            Clicked -= OnClicked;
            Click += ClickFocus;
		}

        public void ClickFocus(UIElement sender, Entry e)
        {
            if (!Focused && CanSelect)
            {
                SetFocus(true);
                Handled = true;
            }
        }
		public bool OverLength(int length)
		{
			return maxLength >= 0 && length > maxLength;
		}
		public string LengthLimit(string text)
		{
			if (OverLength(text.Length))
			{
				return text.Substring(0, maxLength);
			}
			else
			{
				return text;
			}
		}
		public virtual void ResetDisplay()
		{
            InternalResetDisplay(Text);
		}
        protected virtual void InternalResetDisplay(string text)
        {
            if (BreakLine)
                DisplayText = Font.BreakLine(text, TextArea.Width);
            else
                DisplayText = text;
        }
        protected override void OnFocus()
        {
            // active the input device
			if (InputDevice != null)
            {
                offset.X = 0;
                offset.Y = 0;
				InputDevice.Active(this);
            }

            base.OnFocus();
        }
        protected override void OnBlur()
        {
            if (InputDevice != null && InputDevice.Typist == this)
            {
                offset.X = 0;
                offset.Y = 0;
                InputDevice.Stop();
            }

            base.OnBlur();
        }
        private bool NeedBeginEnd(ref RECT view)
        {
            if (IsAutoHeight)
                return false;
            if (InputDevice == null)
                return false;
            var size = TextContentSize;
            // 各平台相同字体尺寸有误差，不要因为细小误差导致BeginEnd
            size.X -= 1;
            size.Y -= 1;
            //size.X -= UIText.Padding.X;
            //size.Y -= UIText.Padding.Y;
            if (InputDevice.Typist != this)
                return size.X > view.Width || size.Y > view.Height;
            RECT alignmentClip = view;
            float offsetX, offsetY;
            UIText.GetAlignmentClip(ref alignmentClip, out offsetX, out offsetY);
            var cursor = InputDevice.CursorLocation;
            cursor.X += offsetX;
            cursor.Y += offsetY;
            bool flag = false;
            if (size.Y > view.Height)
            {
                if (cursor.Y < offset.Y)
                {
                    offset.Y = cursor.Y;
                    flag = true;
                }
                else if (cursor.Y + Font.LineHeight > offset.Y + view.Height)
                {
                    offset.Y = cursor.Y + Font.LineHeight - view.Height;
                    flag = true;
                }
            }
            if (size.X > view.Width)
            {
                if (cursor.X < offset.X)
                {
                    offset.X = cursor.X;
                    flag = true;
                }
                else if (cursor.X > offset.X + view.Width)
                {
                    offset.X = cursor.X - view.Width;
                    flag = true;
                }
            }

            if (flag)
                return true;
            else
                return size.X > view.Width || size.Y > view.Height;
        }
        protected override sealed void DrawFont(GRAPHICS spriteBatch, Entry e)
        {
            RECT textArea = base.TextArea;
            RECT viewArea = ViewArea;
            bool needBeginEnd = NeedBeginEnd(ref viewArea);
            if (needBeginEnd)
            {
                MATRIX2x3 temp = MATRIX2x3.Identity;
                temp.M31 -= offset.X;
                temp.M32 -= offset.Y;
                viewArea.Width++;
                spriteBatch.Begin(temp, viewArea);
            }
            // selected text area
            if (Focused && InputDevice != null && InputDevice.HasSelected)
            {
                foreach (RECT area in InputDevice.SelectedAreas)
                {
                    var area2 = area;
                    area2.X += textArea.X;
                    area2.Y += textArea.Y;
                    spriteBatch.Draw(TEXTURE.Pixel, area2, CursorAreaColor);
                }
            }
            BaseDrawFont(spriteBatch, e);
            if (Focused && InputDevice != null && InputDevice.IsActive && InputDevice.CursorShow)
            {
                RECT cursor = new RECT();
                cursor.Location = textArea.Location + InputDevice.CursorLocation;
                cursor.Width = spriteBatch.OnePixel.X;
                cursor.Height = UIText.Font.LineHeight;
                spriteBatch.Draw(TEXTURE.Pixel, cursor, CursorColor);
            }
            if (needBeginEnd)
            {
                spriteBatch.End();
            }
        }
        protected virtual void BaseDrawFont(GRAPHICS spriteBatch, Entry e)
        {
            base.DrawFont(spriteBatch, e);
        }
        public virtual bool Filter(ref char c)
        {
            return false;
        }
        public virtual void OnStop(string result)
        {
            SetFocus(false);
            text = null;
            Text = result;
            if (TextEditOver != null)
                TextEditOver(this);
        }

        public static bool ValidLength(ref string text, int limit)
        {
            if (string.IsNullOrEmpty(null))
                return true;
            if (text.Length > limit) return false;
            int length = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] > 255)
                    length += 2;
                else
                    length++;
                if (length > limit)
                {
                    text = text.Substring(0, i);
                    return false;
                }
            }
            return true;
        }
    }
}

#endif