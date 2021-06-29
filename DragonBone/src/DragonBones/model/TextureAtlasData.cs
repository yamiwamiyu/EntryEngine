using System.Collections.Generic;

namespace EntryEngine.DragonBone.DBCore
{
    /// <summary>
    /// - The texture atlas data.
    /// </summary>
    /// <version>DragonBones 3.0</version>
    /// <language>en_US</language>

    /// <summary>
    /// - 贴图集数据。
    /// </summary>
    /// <version>DragonBones 3.0</version>
    /// <language>zh_CN</language>
    public class TextureAtlasData : BaseObject
    {
        /// <private/>
        public bool autoSearch;
        /// <private/>
        public uint width;
        /// <private/>
        public uint height;
        /// <private/>
        public float scale;
        /// <summary>
        /// - The texture atlas name.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>

        /// <summary>
        /// - 贴图集名称。
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>zh_CN</language>
        public string name;
        /// <summary>
        /// - The image path of the texture atlas.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>

        /// <summary>
        /// - 贴图集图片路径。
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>zh_CN</language>
        public string imagePath;
        /// <private/>
        public readonly Dictionary<string, TextureData> textures = new Dictionary<string, TextureData>();
        public TextureAtlasData()
        {
        }
        /// <inheritDoc/>
        protected override void _OnClear()
        {
            foreach (var value in this.textures.Values)
            {
                value.ReturnToPool();
            }

            this.autoSearch = false;
            this.width = 0;
            this.height = 0;
            this.scale = 1.0f;
            this.textures.Clear();
            this.name = "";
            this.imagePath = "";
        }
        /// <private/>
        public void CopyFrom(TextureAtlasData value)
        {
            this.autoSearch = value.autoSearch;
            this.scale = value.scale;
            this.width = value.width;
            this.height = value.height;
            this.name = value.name;
            this.imagePath = value.imagePath;

            foreach (var texture in this.textures.Values)
            {
                texture.ReturnToPool();
            }

            this.textures.Clear();

            foreach (var pair in value.textures)
            {
                var texture = CreateTexture();
                texture.CopyFrom(pair.Value);
                textures[pair.Key] = texture;
            }
        }
        /// <internal/>
        /// <private/>
        public virtual TextureData CreateTexture() { return new TextureData(); }
        /// <internal/>
        /// <private/>
        public void AddTexture(TextureData value)
        {
            if (value != null)
            {
                TextureData data;
                if (this.textures.TryGetValue(value.name, out data))
                {
                    data.ReturnToPool();
                }

                value.parent = this;
                this.textures[value.name] = value;
            }
        }
        /// <private/>
        public TextureData GetTexture(string name)
        {
            TextureData texture;
            textures.TryGetValue(name, out texture);
            return texture;
        }

    }
    /// <internal/>
    /// <private/>
    public class TextureData : BaseObject
    {
        public static Rectangle CreateRectangle()
        {
            return new Rectangle();
        }

        public bool rotated;
        public string name;
        public readonly Rectangle region = new Rectangle();
        public TextureAtlasData parent;
        public Rectangle frame = null; // Initial value.

        protected override void _OnClear()
        {
            this.rotated = false;
            this.name = "";
            this.region.Clear();
            this.parent = null; //
            this.frame = null;
        }

        public virtual void CopyFrom(TextureData value)
        {
            this.rotated = value.rotated;
            this.name = value.name;
            this.region.CopyFrom(value.region);
            this.parent = value.parent;

            if (this.frame == null && value.frame != null)
            {
                this.frame = TextureData.CreateRectangle();
            }
            else if (this.frame != null && value.frame == null)
            {
                this.frame = null;
            }

            if (this.frame != null && value.frame != null)
            {
                this.frame.CopyFrom(value.frame);
            }
        }
    }
}