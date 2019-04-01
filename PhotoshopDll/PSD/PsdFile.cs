using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace PhotoshopFile
{
	public class PsdFile
	{
		private class DecompressChannelContext
		{
			private Channel ch;
			public DecompressChannelContext(Channel ch)
			{
				this.ch = ch;
			}
			public void DecompressChannel(object context)
			{
				this.ch.DecompressImageData();
			}
		}
		private short channelCount;
		private int bitDepth;
		public byte[] ColorModeData = new byte[0];
		private byte[] GlobalLayerMaskData = new byte[0];
		public Layer BaseLayer
		{
			get;
			set;
		}
		public ImageCompression ImageCompression
		{
			get;
			set;
		}
		public short Version
		{
			get;
			private set;
		}
		public short ChannelCount
		{
			get
			{
				return this.channelCount;
			}
			set
			{
				if (value < 1 || value > 56)
				{
					throw new ArgumentException("Number of channels must be from 1 to 56.");
				}
				this.channelCount = value;
			}
		}
		public int RowCount
		{
			get
			{
				return this.BaseLayer.Rect.Height;
			}
			set
			{
				if (value < 0 || value > 30000)
				{
					throw new ArgumentException("Number of rows must be from 1 to 30000.");
				}
				this.BaseLayer.Rect = new Rectangle(0, 0, this.BaseLayer.Rect.Width, value);
			}
		}
		public int ColumnCount
		{
			get
			{
				return this.BaseLayer.Rect.Width;
			}
			set
			{
				if (value < 0 || value > 30000)
				{
					throw new ArgumentException("Number of columns must be from 1 to 30000.");
				}
				this.BaseLayer.Rect = new Rectangle(0, 0, value, this.BaseLayer.Rect.Height);
			}
		}
		public int BitDepth
		{
			get
			{
				return this.bitDepth;
			}
			set
			{
				if (value <= 8)
				{
					if (value != 1 && value != 8)
					{
						goto IL_22;
					}
				}
				else
				{
					if (value != 16 && value != 32)
					{
						goto IL_22;
					}
				}
				this.bitDepth = value;
				return;
				IL_22:
				throw new NotImplementedException("Invalid bit depth.");
			}
		}
		public PsdColorMode ColorMode
		{
			get;
			set;
		}
		public ImageResources ImageResources
		{
			get;
			set;
		}
		public ResolutionInfo Resolution
		{
			get
			{
				return (ResolutionInfo)this.ImageResources.Get(ResourceID.ResolutionInfo);
			}
			set
			{
				this.ImageResources.Set(value);
			}
		}
		public List<Layer> Layers
		{
			get;
			private set;
		}
		public List<LayerInfo> AdditionalInfo
		{
			get;
			private set;
		}
		public bool AbsoluteAlpha
		{
			get;
			set;
		}
		public PsdFile()
		{
			this.Version = 1;
			this.BaseLayer = new Layer(this);
			this.BaseLayer.Rect = new Rectangle(0, 0, 0, 0);
			this.ImageResources = new ImageResources();
			this.Layers = new List<Layer>();
			this.AdditionalInfo = new List<LayerInfo>();
		}
		public PsdFile(string fileName) : this()
		{
			Load(fileName);
		}
		public void Load(string fileName)
		{
			Load(fileName, Encoding.Default);
		}
		public void Load(string fileName, Encoding encoding)
		{
			using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
			{
				this.Load(fileStream, encoding);
			}
		}
		public void Load(Stream stream, Encoding encoding)
		{
			PsdBinaryReader reader = new PsdBinaryReader(stream, encoding);
			this.LoadHeader(reader);
			this.LoadColorModeData(reader);
			this.LoadImageResources(reader);
			this.LoadLayerAndMaskInfo(reader);
			this.LoadImage(reader);
			this.DecompressImages();
		}
		public void Save(string fileName, Encoding encoding)
		{
			using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
			{
				this.Save(fileStream, encoding);
			}
		}
		public void Save(Stream stream, Encoding encoding)
		{
			if (this.BitDepth != 8)
			{
				throw new NotImplementedException("Only 8-bit color has been implemented for saving.");
			}
			PsdBinaryWriter psdBinaryWriter = new PsdBinaryWriter(stream, encoding);
			psdBinaryWriter.AutoFlush = true;
			this.PrepareSave();
			this.SaveHeader(psdBinaryWriter);
			this.SaveColorModeData(psdBinaryWriter);
			this.SaveImageResources(psdBinaryWriter);
			this.SaveLayerAndMaskInfo(psdBinaryWriter);
			this.SaveImage(psdBinaryWriter);
		}
		private void LoadHeader(PsdBinaryReader reader)
		{
			string a = reader.ReadAsciiChars(4);
			if (a != "8BPS")
			{
				throw new PsdInvalidException("The given stream is not a valid PSD file");
			}
			this.Version = reader.ReadInt16();
			if (this.Version != 1)
			{
				throw new PsdInvalidException("The PSD file has an unknown version");
			}
			reader.BaseStream.Position += 6L;
			this.ChannelCount = reader.ReadInt16();
			this.RowCount = reader.ReadInt32();
			this.ColumnCount = reader.ReadInt32();
			this.BitDepth = (int)reader.ReadInt16();
			this.ColorMode = (PsdColorMode)reader.ReadInt16();
		}
		private void SaveHeader(PsdBinaryWriter writer)
		{
			string s = "8BPS";
			writer.WriteAsciiChars(s);
			writer.Write(this.Version);
			byte[] value = new byte[6];
			writer.Write(value);
			writer.Write(this.ChannelCount);
			writer.Write(this.RowCount);
			writer.Write(this.ColumnCount);
			writer.Write((short)this.BitDepth);
			writer.Write((short)this.ColorMode);
		}
		private void LoadColorModeData(PsdBinaryReader reader)
		{
			uint num = reader.ReadUInt32();
			if (num > 0u)
			{
				this.ColorModeData = reader.ReadBytes((int)num);
			}
		}
		private void SaveColorModeData(PsdBinaryWriter writer)
		{
			writer.Write((uint)this.ColorModeData.Length);
			writer.Write(this.ColorModeData);
		}
		private void LoadImageResources(PsdBinaryReader reader)
		{
			this.ImageResources.Clear();
			uint num = reader.ReadUInt32();
			if (num <= 0u)
			{
				return;
			}
			long position = reader.BaseStream.Position;
			long num2 = position + (long)((ulong)num);
			while (reader.BaseStream.Position < num2)
			{
				ImageResource item = ImageResourceFactory.CreateImageResource(reader);
				this.ImageResources.Add(item);
			}
			reader.BaseStream.Position = position + (long)((ulong)num);
		}
		private void SaveImageResources(PsdBinaryWriter writer)
		{
			using (new PsdBlockLengthWriter(writer))
			{
				foreach (ImageResource current in this.ImageResources)
				{
					current.Save(writer);
				}
			}
		}
		private void LoadLayerAndMaskInfo(PsdBinaryReader reader)
		{
			uint num = reader.ReadUInt32();
			if (num <= 0u)
			{
				return;
			}
			long position = reader.BaseStream.Position;
			long num2 = position + (long)((ulong)num);
			this.LoadLayers(reader, true);
			this.LoadGlobalLayerMask(reader);
			while (reader.BaseStream.Position < num2)
			{
				LayerInfo layerInfo = LayerInfoFactory.Load(reader);
				this.AdditionalInfo.Add(layerInfo);
				if (layerInfo is RawLayerInfo)
				{
					RawLayerInfo rawLayerInfo = (RawLayerInfo)layerInfo;
					string key;
					if ((key = layerInfo.Key) != null)
					{
						if (!(key == "Layr") && !(key == "Lr16") && !(key == "Lr32"))
						{
							if (!(key == "LMsk"))
							{
								continue;
							}
						}
						else
						{
							using (MemoryStream memoryStream = new MemoryStream(rawLayerInfo.Data))
							{
								using (PsdBinaryReader psdBinaryReader = new PsdBinaryReader(memoryStream, reader))
								{
									this.LoadLayers(psdBinaryReader, false);
								}
								continue;
							}
						}
						this.GlobalLayerMaskData = rawLayerInfo.Data;
					}
				}
			}
			reader.BaseStream.Position = position + (long)((ulong)num);
		}
		private void SaveLayerAndMaskInfo(PsdBinaryWriter writer)
		{
			using (new PsdBlockLengthWriter(writer))
			{
				long position = writer.BaseStream.Position;
				this.SaveLayers(writer);
				this.SaveGlobalLayerMask(writer);
				foreach (LayerInfo current in this.AdditionalInfo)
				{
					current.Save(writer);
				}
				writer.WritePadding(position, 2);
			}
		}
		private void LoadLayers(PsdBinaryReader reader, bool hasHeader)
		{
			uint num = 0u;
			if (hasHeader)
			{
				num = reader.ReadUInt32();
				if (num <= 0u)
				{
					return;
				}
			}
			long position = reader.BaseStream.Position;
			short num2 = reader.ReadInt16();
			if (num2 < 0)
			{
				this.AbsoluteAlpha = true;
				num2 = Math.Abs(num2);
			}
			this.Layers.Clear();
			if (num2 == 0)
			{
				return;
			}
			for (int i = 0; i < (int)num2; i++)
			{
				Layer item = new Layer(reader, this);
				this.Layers.Add(item);
			}
			foreach (Layer current in this.Layers)
			{
				foreach (Channel current2 in current.Channels)
				{
					current2.LoadPixelData(reader);
				}
			}
			if (num > 0u)
			{
				long num3 = position + (long)((ulong)num);
				long arg_E7_0 = reader.BaseStream.Position;
				if (reader.BaseStream.Position < num3)
				{
					reader.BaseStream.Position = num3;
				}
			}
		}
		private void DecompressImages()
		{
			IEnumerable<Layer> enumerable = this.Layers.Concat(new List<Layer>
			{
				this.BaseLayer
			});
			foreach (Layer current in enumerable)
			{
				foreach (Channel current2 in current.Channels)
				{
					PsdFile.DecompressChannelContext @object = new PsdFile.DecompressChannelContext(current2);
					@object.DecompressChannel(null);
				}
			}
			foreach (Layer current3 in this.Layers)
			{
				foreach (Channel current4 in current3.Channels)
				{
					if (current4.ID == -2)
					{
						current3.Masks.LayerMask.ImageData = current4.ImageData;
					}
					else
					{
						if (current4.ID == -3)
						{
							current3.Masks.UserMask.ImageData = current4.ImageData;
						}
					}
				}
			}
		}
		public void PrepareSave()
		{
			List<Layer> list = this.Layers.Concat(new List<Layer>
			{
				this.BaseLayer
			}).ToList<Layer>();
			foreach (Layer current in list)
			{
				current.PrepareSave();
			}
			this.SetVersionInfo();
			this.VerifyLayerSections();
		}
		internal void VerifyLayerSections()
		{
			int num = 0;
			foreach (Layer current in this.Layers.Reverse<Layer>())
			{
				LayerInfo layerInfo = current.AdditionalInfo.SingleOrDefault((LayerInfo x) => x is LayerSectionInfo);
				if (layerInfo != null)
				{
					LayerSectionInfo layerSectionInfo = (LayerSectionInfo)layerInfo;
					switch (layerSectionInfo.SectionType)
					{
					case LayerSectionType.OpenFolder:
					case LayerSectionType.ClosedFolder:
						num++;
						break;
					case LayerSectionType.SectionDivider:
						num--;
						if (num < 0)
						{
							throw new PsdInvalidException("Layer section ended without matching start marker.");
						}
						break;
					default:
						throw new PsdInvalidException("Unrecognized layer section type.");
					}
				}
			}
			if (num != 0)
			{
				throw new PsdInvalidException("Layer section not closed by end marker.");
			}
		}
		public void SetVersionInfo()
		{
			if ((VersionInfo)this.ImageResources.Get(ResourceID.VersionInfo) == null)
			{
				VersionInfo versionInfo = new VersionInfo();
				this.ImageResources.Set(versionInfo);
				Assembly executingAssembly = Assembly.GetExecutingAssembly();
				Version version = executingAssembly.GetName().Version;
				string str = string.Concat(new object[]
				{
					version.Major,
					".",
					version.Minor,
					".",
					version.Build
				});
				versionInfo.Version = 1u;
				versionInfo.HasRealMergedData = true;
				versionInfo.ReaderName = "Paint.NET PSD Plugin";
				versionInfo.WriterName = "Paint.NET PSD Plugin " + str;
				versionInfo.FileVersion = 1u;
			}
		}
		private void SaveLayers(PsdBinaryWriter writer)
		{
			using (new PsdBlockLengthWriter(writer))
			{
				short num = (short)this.Layers.Count;
				if (this.AbsoluteAlpha)
				{
					num = (short)-num;
				}
				if (num != 0)
				{
					long position = writer.BaseStream.Position;
					writer.Write(num);
					foreach (Layer current in this.Layers)
					{
						current.Save(writer);
					}
					foreach (Layer current2 in this.Layers)
					{
						foreach (Channel current3 in current2.Channels)
						{
							current3.SavePixelData(writer);
						}
					}
					writer.WritePadding(position, 4);
				}
			}
		}
		private void LoadGlobalLayerMask(PsdBinaryReader reader)
		{
			uint num = reader.ReadUInt32();
			if (num <= 0u)
			{
				return;
			}
			this.GlobalLayerMaskData = reader.ReadBytes((int)num);
		}
		private void SaveGlobalLayerMask(PsdBinaryWriter writer)
		{
			writer.Write((uint)this.GlobalLayerMaskData.Length);
			writer.Write(this.GlobalLayerMaskData);
		}
		private void LoadImage(PsdBinaryReader reader)
		{
			this.ImageCompression = (ImageCompression)reader.ReadInt16();
			for (short num = 0; num < this.ChannelCount; num += 1)
			{
				Channel channel = new Channel(num, this.BaseLayer);
				channel.ImageCompression = this.ImageCompression;
				channel.Length = this.RowCount * Util.BytesPerRow(this.BaseLayer.Rect, this.BitDepth);
				if (this.ImageCompression == ImageCompression.Rle)
				{
					channel.RleHeader = reader.ReadBytes(2 * this.RowCount);
					int num2 = 0;
					using (MemoryStream memoryStream = new MemoryStream(channel.RleHeader))
					{
						using (PsdBinaryReader psdBinaryReader = new PsdBinaryReader(memoryStream, Encoding.ASCII))
						{
							for (int i = 0; i < this.RowCount; i++)
							{
								num2 += (int)psdBinaryReader.ReadUInt16();
							}
						}
					}
					channel.Length = num2;
				}
				this.BaseLayer.Channels.Add(channel);
			}
			foreach (Channel current in this.BaseLayer.Channels)
			{
				current.Data = reader.ReadBytes(current.Length);
			}
			if (this.ColorMode != PsdColorMode.Multichannel && this.ChannelCount == this.ColorMode.MinChannelCount() + 1)
			{
				Channel channel2 = this.BaseLayer.Channels.Last<Channel>();
				channel2.ID = -1;
			}
		}
		private void SaveImage(PsdBinaryWriter writer)
		{
			writer.Write((short)this.ImageCompression);
			if (this.ImageCompression == ImageCompression.Rle)
			{
				foreach (Channel current in this.BaseLayer.Channels)
				{
					writer.Write(current.RleHeader);
				}
			}
			foreach (Channel current2 in this.BaseLayer.Channels)
			{
				writer.Write(current2.Data);
			}
		}
	}
}
