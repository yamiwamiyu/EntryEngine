#if CLIENT

using System;

namespace EntryEngine.UI
{
	public enum EDirection4
	{
		Right = 0,
		Down = 1,
		Left = 2,
		Up = 3
	}
	public enum EDirection8
	{
		Right = 0,
		RightDown = 1,
		Down = 2,
		DownLeft = 3,
		Left = 4,
		LeftUp = 5,
		Up = 6,
		UpRight = 7
	}
	public abstract class Slider : UIElement
	{
		private float curValue;
		private float minValue;
		private float maxValue;
		public float Step = 1;
		public event DUpdate<Slider> ValueChanging;
		public event DUpdate<Slider> ValueChanged;

		public float Value
		{
			get { return curValue; }
			set
			{
				value = _MATH.Clamp(value, minValue, maxValue);
				if (curValue != value)
				{
					OnValueChanging(ref value);
					curValue = value;
					OnValueChanged(curValue);
				}
			}
		}
		public float MinValue
		{
			get { return minValue; }
			set
			{
				minValue = value;

				if (minValue > maxValue)
					maxValue = minValue;

				if (minValue > curValue)
					Value = minValue;
			}
		}
		public float MaxValue
		{
			get { return maxValue; }
			set
			{
				maxValue = value;

				if (maxValue < minValue)
					minValue = maxValue;

				if (maxValue < curValue)
					Value = maxValue;
			}
		}
		public float ValueScope
		{
			get { return maxValue - minValue; }
		}
		public float Percent
		{
			get
			{
				if (maxValue == minValue)
					return 0.0f;
				else
					return (curValue - minValue) / (maxValue - minValue);
			}
			set
			{
				Value = value * (maxValue - minValue) + minValue;
			}
		}

		protected virtual void OnValueChanging(ref float value)
		{
			if (ValueChanging != null)
			{
				ValueChanging(this, null);
			}
		}
		protected virtual void OnValueChanged(float value)
		{
			if (ValueChanged != null)
			{
				ValueChanged(this, null);
			}
		}
	}
	public abstract class ScrollBarBase : Slider
	{
        private Panel panel;
		private Button body;
		public TEXTURE Bar;
		public TEXTURE BarL;
		public TEXTURE BarR;
		private Button buttonL;
		private Button buttonR;
		protected RECT barClip;
		protected RECT barViewClip;
		private bool isDeltaPressed = true;
		private bool isDeltaDrag = true;
		protected float dragOffset;
		public float StepClickBar = 10;
		public float StepScroll = 1;
		public event DUpdate<ScrollBarBase> Scroll;

        public Panel Panel
        {
            get { return panel; }
            internal set { panel = value; }
        }
        public bool IsPanelContent
        {
            get { return panel != null && panel == Parent; }
        }
		public Button Body
		{
			get { return body; }
			set
			{
				if (body == value)
				{
					return;
				}
				if (body != null)
				{
                    Remove(body);
				}
				this.body = value;
				if (value != null)
				{
                    Add(value);
					SetBody(value);
				}
			}
		}
		public Button ButtonL
		{
			get { return buttonL; }
			set
			{
				if (buttonL == value)
				{
					return;
				}
				if (buttonL != null)
				{
                    Remove(buttonL);
				}
				this.buttonL = value;
				if (value != null)
				{
                    Insert(value, 0);
					SetButtonL(value);
				}
			}
		}
		public Button ButtonR
		{
			get { return buttonR; }
			set
			{
				if (buttonR == value)
				{
					return;
				}
				if (buttonR != null)
				{
                    Remove(buttonR);
				}
				this.buttonR = value;
				if (value != null)
				{
                    Insert(value, 0);
					SetButtonR(value);
				}
			}
		}
		public bool IsDeltaPressed
		{
			get { return isDeltaPressed; }
			set
			{
				if (isDeltaPressed == value)
					return;

				this.isDeltaPressed = value;

				if (buttonL != null)
				{
					SetButtonL(buttonL);
				}

				if (buttonR != null)
				{
					SetButtonR(buttonR);
				}
			}
		}
		public bool IsDeltaDrag
		{
			get { return isDeltaDrag; }
			set
			{
				if (isDeltaDrag == value)
					return;

				this.isDeltaDrag = value;

				if (body != null)
					SetBody(body);
			}
		}
		public bool IsDeltaClick
		{
			get { return StepClickBar > 0; }
		}
		public RECT BarViewClip
		{
			get { return barViewClip; }
		}

		public ScrollBarBase()
		{
			SortZ = int.MaxValue;
            //IsClip = false;
			RegistEvent(OnScroll);
			this.Drag += DoDrag;
		}

        //protected override void OnRemoved(UIElement node)
        //{
        //    //base.OnRemoved(node);
        //    if (node == buttonL)
        //        buttonL = null;
        //    else if (node == buttonR)
        //        buttonR = null;
        //}
		protected override void UpdateTranslation(ref MATRIX2x3 transform)
        {
            base.UpdateTranslation(ref transform);
            if (panel != null && panel == Parent)
            {
                transform.M31 += panel.OffsetX;
                transform.M32 += panel.OffsetY;
            }
        }
		protected override void UpdateClip(ref RECT finalClip, ref RECT finalViewClip, ref MATRIX2x3 transform, ref MATRIX2x3 localToWorld)
		{
			base.UpdateClip(ref finalClip, ref finalViewClip, ref transform, ref localToWorld);

			UpdateBarClip(ref finalClip, ref barClip, ref barViewClip);

			if (body != null)
			{
				SetBody(body);
			}
			if (buttonL != null)
			{
				SetButtonL(buttonL);
			}
			if (buttonR != null)
			{
				SetButtonR(buttonR);
			}
		}
		protected abstract void UpdateBarClip(ref RECT finalClip, ref RECT barClip, ref RECT barViewClip);
		protected virtual void SetBody(Button body)
		{
			body.Click -= ValueDragBegin;
			body.Drag -= ValueDrag;
			if (isDeltaDrag)
			{
				body.Click += ValueDragBegin;
				body.Drag += ValueDrag;
			}
		}
		protected virtual void SetButtonL(Button buttonL)
		{
			buttonL.Click -= ValueMinus;
			buttonL.Pressed -= ValueMinus;
			if (isDeltaPressed)
			{
				buttonL.Pressed += ValueMinus;
			}
			else
			{
				buttonL.Click += ValueMinus;
			}
		}
		protected virtual void SetButtonR(Button buttonR)
		{
			buttonR.Click -= ValuePlus;
			buttonR.Pressed -= ValuePlus;
			if (isDeltaPressed)
			{
				buttonR.Pressed += ValuePlus;
			}
			else
			{
				buttonR.Click += ValuePlus;
			}
		}
		private void ValueMinus(UIElement sender, Entry e)
		{
			Value -= Step;
		}
		private void ValuePlus(UIElement sender, Entry e)
		{
			Value += Step;
		}
		protected virtual void ValueDragBegin(UIElement sender, Entry e)
		{
		}
		protected virtual void ValueDrag(UIElement sender, Entry e)
		{
		}
		protected virtual void DoDrag(UIElement sender, Entry e)
		{
		}
		protected override void OnValueChanged(float value)
		{
			if (body != null)
			{
				SetBody(body);
			}
			base.OnValueChanged(value);
		}
		private void OnScroll(Entry e)
		{
			if (e.INPUT.Mouse != null && (isHover || (Parent != null && Parent.IsHover)))
			{
				float scroll = e.INPUT.Mouse.ScrollWheelValue;
				if (scroll != 0)
				{
					if (StepScroll != 0)
					{
						Value += StepScroll * scroll;
					}
					if (Scroll != null)
					{
						Scroll(this, e);
					}
				}
			}
		}
		protected override void InternalDraw(GRAPHICS spriteBatch, Entry e)
		{
			if (IsAutoClip)
			{
				throw new NotImplementedException();
			}
		}
		public override void Dispose()
		{
			base.Dispose();
            Bar = null;
            BarL = null;
            BarR = null;
		}
        public override System.Collections.Generic.IEnumerator<UIElement> GetEnumerator()
        {
            foreach (var item in Childs.ToArray())
                if (item != buttonL && item != buttonR)
                    yield return item;
        }

        public static float OnScroll(UIElement sender, Entry e)
        {
            if (e.INPUT.Mouse != null && (sender.IsHover || (sender.Parent != null && sender.Parent.IsHover)))
                return e.INPUT.Mouse.ScrollWheelValue;
            return 0;
        }
	}
	public class ScrollBarHorizontal : ScrollBarBase
	{
		protected override void UpdateBarClip(ref RECT finalClip, ref RECT barClip, ref RECT barViewClip)
		{
			barClip = finalClip;
            //if (ButtonL != null)
            //{
            //    barClip.X += ButtonL.Width;
            //    barClip.Width -= ButtonL.Width;
            //}
            //if (ButtonR != null)
            //{
            //    barClip.Width -= ButtonR.Width;
            //}

			finalClip = barClip;
			barViewClip = finalClip;

			if (Body != null)
			{
				barClip.X += Body.Width / 2;
				barClip.Width -= Body.Width;
			}
		}
		protected override void SetBody(Button body)
		{
			body.Pivot = EPivot.MiddleCenter;
			body.X = barClip.X + barClip.Width * Percent - barViewClip.X;
			body.Y = Height / 2;
			base.SetBody(body);
		}
		protected override void SetButtonL(Button buttonL)
		{
            //buttonL.IsClip = false;
            //buttonL.Pivot = EPivot.MiddleLeft;
            //buttonL.X = 0;
            //buttonL.Y = Height / 2;
			base.SetButtonL(buttonL);
		}
		protected override void SetButtonR(Button buttonR)
		{
            //buttonR.IsClip = false;
            //buttonR.Pivot = EPivot.MiddleRight;
            //buttonR.X = Width;
            //buttonR.Y = Height / 2;
			base.SetButtonR(buttonR);
		}
		protected override void ValueDragBegin(UIElement sender, Entry e)
		{
			dragOffset = sender.ConvertGraphicsToLocal(e.INPUT.Pointer.Position).X - sender.Width / 2;
		}
		protected override void ValueDrag(UIElement sender, Entry e)
		{
            Handled = true;
			if (barClip.Width != 0)
			{
                float position = ConvertGraphicsToLocal(e.INPUT.Pointer.Position).X - dragOffset;
				Percent = (position - barClip.X + barViewClip.X) / barClip.Width;
			}
		}
		protected override void DoDrag(UIElement sender, Entry e)
		{
            VECTOR2 position = ConvertGraphicsToLocal(e.INPUT.Pointer.Position);
			float diff = (position.X - barClip.X) / barClip.Width - Percent;
			Value += _MATH.Sign(diff) * _MATH.Min(_MATH.Abs(StepClickBar), _MATH.Abs(diff) * ValueScope);
		}
		protected override void InternalDraw(GRAPHICS spriteBatch, Entry e)
		{
			base.InternalDraw(spriteBatch, e);

			RECT body = barViewClip;
			// bar
			if (Bar != null)
			{
				spriteBatch.Draw(Bar, body, Color);
			}
			// barL
			if (BarL != null)
			{
				body.Width = barClip.Width * Percent - (barClip.Width - barViewClip.Width) / 2;
				//if (BarL.NinePatch.IsEmpty && !BarL.NinePatch.Bound.IsEmpty)
				//{
				//    RECT temp = BarL.NinePatch.Bound;
				//    BarL.NinePatch.Bound.Width *= Percent;
				//    BarL.Draw(spriteBatch, body);
				//    BarL.NinePatch.Bound = temp;
				//}
				//else
				//{
				//    BarL.Draw(spriteBatch, body);
				//}
				spriteBatch.Draw(BarL, body, Color);
			}
			// barR
			if (BarR != null)
			{
				body.X += body.Width;
				body.Width = _MATH.Ceiling(barViewClip.Width - body.Width);
				//if (BarR.NinePatch.IsEmpty && !BarR.NinePatch.Bound.IsEmpty)
				//{
				//    RECT temp = BarR.NinePatch.Bound;
				//    float inflate = BarR.NinePatch.Bound.Width * (1 - Percent);
				//    BarR.NinePatch.Bound.Width = _MATH.Ceiling(BarR.NinePatch.Bound.Width * (1 - Percent));
				//    BarR.NinePatch.Bound.X += temp.Width - BarR.NinePatch.Bound.Width;
				//    BarR.Draw(spriteBatch, body);
				//    BarR.NinePatch.Bound = temp;
				//}
				//else
				//{
				//    BarR.Draw(spriteBatch, body);
				//}
				spriteBatch.Draw(BarR, body, Color);
			}
		}
	}
	public class ScrollBarVertical : ScrollBarBase
	{
		protected override void UpdateBarClip(ref RECT finalClip, ref RECT barClip, ref RECT barViewClip)
		{
			barClip = finalClip;
            //if (ButtonL != null)
            //{
            //    barClip.Y += ButtonL.Height;
            //    barClip.Height -= ButtonL.Height;
            //}
            //if (ButtonR != null)
            //{
            //    barClip.Height -= ButtonR.Height;
            //}

			finalClip = barClip;
			barViewClip = finalClip;

			if (Body != null)
			{
				barClip.Y += Body.Height / 2;
				barClip.Height -= Body.Height;
			}
		}
		protected override void SetBody(Button body)
		{
			body.Pivot = EPivot.MiddleCenter;
			body.X = Width / 2;
			body.Y = barClip.Y + barClip.Height * Percent - barViewClip.Y;
			base.SetBody(body);
		}
		protected override void SetButtonL(Button buttonL)
		{
            //buttonL.IsClip = false;
			buttonL.Pivot = EPivot.MiddleLeft;
			buttonL.X = Width / 2;
			buttonL.Y = 0;
			base.SetButtonL(buttonL);
		}
		protected override void SetButtonR(Button buttonR)
		{
            //buttonR.IsClip = false;
			buttonR.Pivot = EPivot.MiddleRight;
			buttonR.X = Width / 2;
			buttonR.Y = Height;
			base.SetButtonR(buttonR);
		}
		protected override void ValueDragBegin(UIElement sender, Entry e)
		{
            dragOffset = sender.ConvertGraphicsToLocal(e.INPUT.Pointer.Position).Y - sender.Height / 2;
		}
		protected override void ValueDrag(UIElement sender, Entry e)
		{
            Handled = true;
			if (barClip.Height != 0)
			{
                float position = ConvertGraphicsToLocal(e.INPUT.Pointer.Position).Y - dragOffset;
				Percent = (position - barClip.Y + barViewClip.Y) / barClip.Height;
			}
		}
		protected override void DoDrag(UIElement sender, Entry e)
		{
            VECTOR2 position = ConvertGraphicsToLocal(e.INPUT.Pointer.Position);
			float diff = (position.Y - barClip.Y) / barClip.Height - Percent;
			Value += _MATH.Sign(diff) * _MATH.Min(_MATH.Abs(StepClickBar), _MATH.Abs(diff) * ValueScope);
		}
		protected override void InternalDraw(GRAPHICS spriteBatch, Entry e)
		{
			base.InternalDraw(spriteBatch, e);

			RECT body = barViewClip;
			// bar
			if (Bar != null)
			{
				spriteBatch.Draw(Bar, body, Color);
			}
			// barL
			if (BarL != null)
			{
				body.Height = barClip.Height * Percent - (barClip.Height - barViewClip.Height) / 2;
				//if (BarL.NinePatch.IsEmpty && !BarL.NinePatch.Bound.IsEmpty)
				//{
				//    RECT temp = BarL.NinePatch.Bound;
				//    BarL.NinePatch.Bound.Height *= Percent;
				//    BarL.Draw(spriteBatch, body);
				//    BarL.NinePatch.Bound = temp;
				//}
				//else
				//{
				//    BarL.Draw(spriteBatch, body);
				//}
				spriteBatch.Draw(BarL, body, Color);
			}
			// barR
			if (BarR != null)
			{
				body.Y += body.Height;
				body.Height = _MATH.Ceiling(barViewClip.Height - body.Height);
				//if (BarR.NinePatch.IsEmpty && !BarR.NinePatch.Bound.IsEmpty)
				//{
				//    RECT temp = BarR.NinePatch.Bound;
				//    float inflate = BarR.NinePatch.Bound.Height * (1 - Percent);
				//    BarR.NinePatch.Bound.Height = _MATH.Ceiling(BarR.NinePatch.Bound.Height * (1 - Percent));
				//    BarR.NinePatch.Bound.Y += temp.Height - BarR.NinePatch.Bound.Height;
				//    BarR.Draw(spriteBatch, body);
				//    BarR.NinePatch.Bound = temp;
				//}
				//else
				//{
				//    BarR.Draw(spriteBatch, body);
				//}
				spriteBatch.Draw(BarR, body, Color);
			}
		}
	}
}

#endif