using System;
using System.Collections.Specialized;
using System.Drawing;
namespace PhotoshopFile
{
	public class Mask
	{
		private byte backgroundColor;
		private static int positionVsLayerBit = BitVector32.CreateMask();
		private static int disabledBit = BitVector32.CreateMask(Mask.positionVsLayerBit);
		private static int invertOnBlendBit = BitVector32.CreateMask(Mask.disabledBit);
		private BitVector32 flags;
		public Layer Layer
		{
			get;
			private set;
		}
		public Rectangle Rect
		{
			get;
			set;
		}
		public byte BackgroundColor
		{
			get
			{
				return this.backgroundColor;
			}
			set
			{
				if (value != 0 && value != 255)
				{
					throw new PsdInvalidException("Mask background must be fully-opaque or fully-transparent.");
				}
				this.backgroundColor = value;
			}
		}
		public BitVector32 Flags
		{
			get
			{
				return this.flags;
			}
		}
		public bool PositionVsLayer
		{
			get
			{
				return this.flags[Mask.positionVsLayerBit];
			}
			set
			{
				this.flags[Mask.positionVsLayerBit] = value;
			}
		}
		public bool Disabled
		{
			get
			{
				return this.flags[Mask.disabledBit];
			}
			set
			{
				this.flags[Mask.disabledBit] = value;
			}
		}
		public bool InvertOnBlend
		{
			get
			{
				return this.flags[Mask.invertOnBlendBit];
			}
			set
			{
				this.flags[Mask.invertOnBlendBit] = value;
			}
		}
		public byte[] ImageData
		{
			get;
			set;
		}
		public Mask(Layer layer)
		{
			this.Layer = layer;
			this.flags = default(BitVector32);
		}
		public Mask(Layer layer, Rectangle rect, byte color, BitVector32 flags)
		{
			this.Layer = layer;
			this.Rect = rect;
			this.BackgroundColor = color;
			this.flags = flags;
		}
	}
}
