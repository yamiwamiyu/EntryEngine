#if CLIENT

namespace EntryEngine.UI
{
    public class TextureBox : UIElement
    {
		private TEXTURE texture;
        public EViewport DisplayMode = EViewport.Strength;
        public event DUpdate<TextureBox> TextureChanged;

        public TEXTURE Texture
        {
			get { return texture; }
            set
            {
				if (texture != value)
				{
					texture = value;
					if (TextureChanged != null)
					{
						TextureChanged(this, Entry.Instance);
					}
				}
            }
        }
        public override VECTOR2 ContentSize
        {
            get
            {
                if (Texture == null)
                    return VECTOR2.Zero;
                else
                    return texture.Size;
            }
        }

        protected override void InternalDraw(GRAPHICS spriteBatch, Entry e)
        {
			if (texture == null)
                return;

            RECT view = ViewClip;
            VECTOR2 cs = ContentSize;
            VECTOR2 size = view.Size;
            EViewport mode = DisplayMode;

            if (mode == EViewport.None)
            {
                RECT source = new RECT();
                if (cs.X < size.X)
                {
                    view.Width = cs.X;
                    source.Width = cs.X;
                }
                else
                    source.Width = size.X;
                if (cs.Y < size.Y)
                {
                    view.Height = cs.Y;
                    source.Height = cs.Y;
                }
                else
                    source.Height = size.Y;
                spriteBatch.Draw(Texture, view, source, Color);
                //spriteBatch.Draw(Texture, view.Location, null, Color);
            }
            else if (mode == EViewport.Keep ||
                (mode == EViewport.Adapt && IsAutoClip))
            {
                RECT source = new RECT();
                if (cs.X < size.X)
                {
                    view.X += (size.X - cs.X) * 0.5f;
                    view.Width = cs.X;
                    source.Width = cs.X;
                }
                else
                    source.Width = size.X;
                if (cs.Y < size.Y)
                {
                    view.Y += (size.Y - cs.Y) * 0.5f;
                    view.Height = cs.Y;
                    source.Height = cs.Y;
                }
                else
                    source.Height = size.Y;
                spriteBatch.Draw(Texture, view, source, Color);
            }
            else if (mode == EViewport.Adapt)
            {
                float scale;
                VECTOR2 offset;
                __GRAPHICS.ViewAdapt(cs, size, out scale, out offset);

                view.Width = cs.X * scale;
                view.Height = cs.Y * scale;
                view.X += offset.X;
                view.Y += offset.Y;

                spriteBatch.Draw(Texture, view, Color);
            }
            else
            {
                // strength
                spriteBatch.Draw(Texture, view, Color);
            }
        }
        public override void Dispose()
        {
            base.Dispose();
            texture = null;
        }
    }
	public class AnimationBox : TextureBox
	{
		public ANIMATION Animation
		{
			get
			{
				return (ANIMATION)Texture;
			}
			set
			{
				if (value != null)
				{
					value.AutoPlay = false;
				}
				Texture = value;
			}
		}
		public event DUpdate<AnimationBox> FrameChanged;
		public event DUpdate<AnimationBox> SequenceOver;

		protected override void InternalUpdate(Entry e)
		{
			ANIMATION animation = Animation;
			if (animation != null)
			{
				animation.AutoPlay = false;
				if (FrameChanged != null && animation.IsFrameOver)
				{
					FrameChanged(this, e);
				}
				if (animation.Update(e.GameTime.Elapsed))
				{
					if (SequenceOver != null)
					{
						SequenceOver(this, e);
					}
				}
			}
		}
	}
}

#endif