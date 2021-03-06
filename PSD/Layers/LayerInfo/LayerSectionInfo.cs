﻿using System;

namespace PSDFile
{
    public enum LayerSectionType
    {
        /// <summary>普通图层</summary>
        Layer = 0,
        /// <summary>第一个图层组开始标记</summary>
        OpenFolder = 1,
        /// <summary>非第一个图层组开始标记</summary>
        ClosedFolder = 2,
        /// <summary>图层组结束的标记</summary>
        SectionDivider = 3
    }

    public enum LayerSectionSubtype
    {
        Normal = 0,
        SceneGroup = 1
    }

    /// <summary>
    /// Layer sections are known as Groups in the Photoshop UI.
    /// </summary>
    public class LayerSectionInfo : LayerInfo
    {
        public override string Signature { get { return "8BIM"; } }

        private string key = "lsct";
        public override string Key { get { return key; } }

        public LayerSectionType SectionType { get; set; }

        private LayerSectionSubtype? _subtype;
        public LayerSectionSubtype Subtype
        {
            get { return _subtype ?? LayerSectionSubtype.Normal; }
            set { _subtype = value; }
        }

        private string _blendModeKey = "pass";
        public string BlendModeKey
        {
            get { return _blendModeKey; }
            set
            {
                if (value.Length != 4)
                {
                    throw new ArgumentException(
                      "BlendModeKey must be 4 characters in length.");
                }
                _blendModeKey = value;
            }
        }

        public LayerSectionInfo(PsdBinaryReader reader, string key, int dataLength)
        {
            // The key for layer section info is documented to be "lsct".  However,
            // some Photoshop files use the undocumented key "lsdk", with apparently
            // the same data format.
            this.key = key;

            SectionType = (LayerSectionType)reader.ReadInt32();
            if (dataLength >= 12)
            {
                var signature = reader.ReadAsciiChars(4);
                if (signature != "8BIM")
                    throw new PsdInvalidException("Invalid section divider signature.");

                BlendModeKey = reader.ReadAsciiChars(4);
                if (dataLength >= 16)
                {
                    Subtype = (LayerSectionSubtype)reader.ReadInt32();
                }
            }
        }

        public LayerSectionInfo(string key = null)
        {
            if (!string.IsNullOrEmpty(key))
            {
                this.key = key;
            }
        }

        protected override void WriteData(PsdBinaryWriter writer)
        {
            writer.Write((Int32)SectionType);
            if (BlendModeKey != null)
            {
                writer.WriteAsciiChars("8BIM");
                writer.WriteAsciiChars(BlendModeKey);
                if (_subtype != null)
                    writer.Write((Int32)Subtype);
            }
        }
    }
}
