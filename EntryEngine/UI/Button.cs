#if CLIENT

namespace EntryEngine.UI
{
	public enum EButtonState
	{
		Normal,
		Hover,
		Click,
		Clicked,
		Unable = 0xf0,
	}
	public class Button : UIElement
	{
		public TEXTURE SourceNormal;
		public TEXTURE SourceHover;
		public TEXTURE SourceClick;
		public TEXTURE SourceClicked;
		public TEXTURE SourceUnable;
		private bool hasClicked;
		private UIText uitext = new UIText();
        public event DUpdate<Button> OnChecked;
		public event DUpdate<Button> CheckedChanged;

		public UIText UIText
		{
			get { return uitext; }
			set
			{
				if (value != null && value != uitext)
				{
					uitext = value;
					NeedUpdateLocalToWorld = true;
				}
			}
		}
		public virtual string Text
		{
			get { return uitext.Text; }
			set { uitext.Text = value; }
		}
        //public override bool CanFocused
        //{
        //    get
        //    {
        //        return true;
        //    }
        //}
		public virtual bool Checked
		{
			get { return hasClicked; }
            set
            {
                if (hasClicked != value)
                {
                    hasClicked = value;
                    if (CheckedChanged != null)
                    {
                        CheckedChanged(this, Entry.Instance);
                    }
                    if (value && OnChecked != null)
                    {
                        OnChecked(this, Entry.Instance);
                    }
                }
            }
		}
		public TEXTURE Source
		{
			get
			{
				if (!FinalEventable && SourceUnable != null)
					return SourceUnable;

				TEXTURE source;
				if (Checked && SourceClicked != null)
				{
					return SourceClicked;
				}
				else
				{
					source = SourceNormal;
				}

				if (isHover)
				{
                    if (IsClick)
					{
						if (SourceClick != null)
						{
							source = SourceClick;
						}
					}
					else if (SourceHover != null)
					{
						source = SourceHover;
					}
				}

				return source;
			}
		}
		public EButtonState ButtonState
		{
			get
			{
				if (!FinalEventable && SourceUnable != null)
					return EButtonState.Unable;

				EButtonState state;

				if (Checked && SourceClicked != null)
				{
					state = EButtonState.Clicked;
				}
				else
				{
					state = EButtonState.Normal;
				}

				if (isHover)
				{
					if (IsClick)
					{
						if (SourceClick != null)
						{
							state = EButtonState.Click;
						}
					}
					else if (SourceHover != null)
					{
						state = EButtonState.Hover;
					}
				}

				return state;
			}
		}
		public override VECTOR2 ContentSize
		{
			get
			{
				TEXTURE texture = Source;
				if (texture == null)
					return TextContentSize;
				else
					return texture.Size;
			}
		}
        public virtual VECTOR2 TextContentSize
        {
            get { return CalcTextContentSize(Text); }
        }
		public virtual RECT TextArea
		{
            get { return UIText.GetTextClip(ViewClip); }
            //get { return ViewClip; }
		}
        public override EUIType UIType
        {
            get { return EUIType.Button; }
        }

		protected TEXTURE this[EButtonState state]
		{
			get
			{
				switch (state)
				{
					case EButtonState.Normal:
						return SourceNormal;
					case EButtonState.Hover:
						return SourceHover;
					case EButtonState.Click:
						return SourceClick;
					case EButtonState.Clicked:
						return SourceClicked;
                    case EButtonState.Unable:
                        return SourceUnable;
				}
				return null;
			}
			set
			{
				switch (state)
				{
					case EButtonState.Normal:
						SourceNormal = value;
						break;
					case EButtonState.Hover:
						SourceHover = value;
						break;
					case EButtonState.Click:
						SourceClick = value;
						break;
					case EButtonState.Clicked:
						SourceClicked = value;
						break;
                    case EButtonState.Unable:
                        SourceUnable = value;
                        break;
				}
			}
		}
		public Button()
		{
			this.uitext.TextAlignment = EPivot.MiddleCenter;
			this.Clicked += OnClicked;
		}

        public void DoCheck()
        {
            if (!Checked)
            {
                Checked = true;
            }
            else
            {
                if (OnChecked != null)
                {
                    OnChecked(this, Entry.Instance);
                }
            }
        }
        public VECTOR2 CalcTextContentSize(string text)
        {
            VECTOR2 size;
            if (uitext.Font == null)
                size = VECTOR2.Zero;
            else
                if (string.IsNullOrEmpty(text))
                    size = new VECTOR2(0, uitext.Font.LineHeight);
                else
                    size = uitext.Font.MeasureString(text);
            if (PivotX(uitext.TextAlignment) != 1)
                size.X += uitext.Padding.X;
            if (PivotY(uitext.TextAlignment) != 1)
                size.Y += uitext.Padding.Y;
            return size;
            //return VECTOR2.Add(ref size, ref uitext.Padding);
        }
		public void OnClicked(UIElement sender, Entry e)
		{
			Checked = OnCheckedChanging();
		}
		protected virtual bool OnCheckedChanging()
		{
			return true;
		}
		protected override void InternalDraw(GRAPHICS spriteBatch, Entry e)
		{
			TEXTURE texture = Source;
			if (texture != null)
			{
				spriteBatch.Draw(texture, ViewClip, Color);
			}
			DrawFont(spriteBatch, e);
		}
		protected virtual void DrawFont(GRAPHICS spriteBatch, Entry e)
		{
			if (uitext.Font != null && !string.IsNullOrEmpty(uitext.Text))
			{
				uitext.Draw(spriteBatch, ViewClip);
			}
		}
		public override void Dispose()
		{
			base.Dispose();
            SourceNormal = null;
            SourceHover = null;
            SourceClick = null;
            SourceClicked = null;
            SourceUnable = null;
		}
	}
}

#endif