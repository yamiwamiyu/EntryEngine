using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine;
using DragonBones;
using System.IO;
using EntryEngine.Serialize;

namespace EntryEngine.DragonBones
{
    class DBTextureAtlasData : TextureAtlasData
    {
        internal TEXTURE Texture;
    }
    class DBSlot : Slot
    {
        protected override void _InitDisplay(object value, bool isRetain)
        {
        }
        protected override void _DisposeDisplay(object value, bool isRelease)
        {
        }
        protected override void _OnUpdateDisplay()
        {
        }
        protected override void _AddDisplay()
        {
        }
        protected override void _ReplaceDisplay(object value)
        {
        }
        protected override void _RemoveDisplay()
        {
        }
        protected override void _UpdateZOrder()
        {
            // todo: 排序
        }
        internal override void _UpdateVisible()
        {
        }
        internal override void _UpdateBlendMode()
        {
        }
        protected override void _UpdateColor()
        {
        }
        protected override void _UpdateFrame()
        {
        }
        protected override void _UpdateMesh()
        {
        }
        protected override void _UpdateTransform()
        {
        }
        protected override void _IdentityTransform()
        {
        }
    }
    class DBFactory : BaseFactory
    {
        internal static global::DragonBones.DragonBones _dragonBonesInstance = new global::DragonBones.DragonBones(null);

        protected override TextureAtlasData _BuildTextureAtlasData(TextureAtlasData textureAtlasData, object textureAtlas)
        {
            // 首次进入先构建抽象类的实现类
            if (textureAtlasData == null)
            {
                return new DBTextureAtlasData();
            }
            // 二次进入时已经将大图切片都已经加载好了
            ((DBTextureAtlasData)textureAtlasData).Texture = (TEXTURE)textureAtlas;
            return textureAtlasData;
        }
        protected override Armature _BuildArmature(BuildArmaturePackage dataPackage)
        {
            DRAGONBONES display = new DRAGONBONES();
            display.Proxy = new IArmatureProxy();

            Armature armature = new Armature();
            armature.Init(dataPackage.armature, display.Proxy, display, _dragonBonesInstance);

            display.DragonBonesData = dataPackage.data;
            display.TextureData = (DBTextureAtlasData)dataPackage.texture;
            return armature;
        }
        protected override Slot _BuildSlot(BuildArmaturePackage dataPackage, SlotData slotData, Armature armature)
        {
            DBSlot slot = new DBSlot();
            slot.Init(slotData, armature, null, null);
            return slot;
        }
    }

    /// <summary>龙骨骼</summary>
    public class DRAGONBONES : TEXTURE
    {
        private const int TL = 0;
        private const int TR = 1;
        private const int BL = 2;
        private const int BR = 3;
        private static short[] quadTriangles = { 0, 1, 2, 1, 3, 2 };
        private static TextureVertex[] vertices = new TextureVertex[128];

        internal DragonBonesData DragonBonesData;
        internal DBTextureAtlasData TextureData;
        int updated;

        public override int Width
        {
            get { return (int)Proxy.Armature._armatureData.aabb.width; }
        }
        public override int Height
        {
            get { return (int)Proxy.Armature._armatureData.aabb.height; }
        }
        public override bool IsDisposed
        {
            get { return Proxy.Armature == null; }
        }
        /// <summary>龙骨信息</summary>
        public IArmatureProxy Proxy
        {
            get;
            internal set;
        }
        public TEXTURE Texture { get { return TextureData.Texture; } }

        internal DRAGONBONES() { }

        public override void Update(GameTime time)
        {
            updated = GameTime.Time.FrameID;
            if (Proxy.Armature != null)
                Proxy.Armature.AdvanceTime(time.ElapsedSecond);
        }
        protected override void InternalDispose()
        {
            if (Proxy != null)
            {
                Proxy.Armature.Dispose();
                Proxy = null;
            }
            if (TextureData != null)
            {
                if (TextureData.Texture != null)
                {
                    TextureData.Texture.Dispose();
                    TextureData.Texture = null;
                }
                TextureData = null;
            }
        }
        protected override bool Draw(GRAPHICS graphics, ref SpriteVertex vertex)
        {
            if (Proxy.Armature == null) return true;

            if (updated != GameTime.Time.FrameID)
            {
                updated = GameTime.Time.FrameID;
                Update(GameTime.Time);
            }

            var slots = Proxy.Armature.GetSlots();
            if (slots.Count == 0) return true;

            //armature.flipX = true;
            //armature.flipY = true;
            //vertex.Flip = EFlip.FlipHorizontally;

            MATRIX2x3 matrix;
            __GRAPHICS.DrawMatrix(ref vertex.Destination, ref vertex.Source, vertex.Rotation, ref vertex.Origin, vertex.Flip, out matrix);
            graphics.BeginFromPrevious(matrix);

            SpriteVertex temp = new SpriteVertex();
            int vertexCount = 0;
            int primitiveCount = 0;
            var vertices = graphics.GetVertexBuffer();

            VECTOR2 p;
            int count = 0;
            for (int i = 0; i < slots.Count; i++)
            {
                Slot slot = slots[i];

                //if (!slot.visible) continue;

                if (slot.displayIndex == -1)
                {
                    // 子骨架
                }
                else
                {
                    var display = slot.displayList[slot.displayIndex];
                    if (display is ImageDisplayData)
                    {
                        var data = (ImageDisplayData)display;
                        var currentTextureData = data.texture;

                        graphics.ToSpriteVertex(TextureData.Texture,
                            slot.global.x,
                            slot.global.y,
                            slot.global.scaleX, slot.global.scaleY,
                            true,
                            data.texture.region.x, data.texture.region.y, data.texture.region.width, data.texture.region.height,
                            true,
                            (byte)(slot._colorTransform.redMultiplier * vertex.Color.R), (byte)(slot._colorTransform.greenMultiplier * vertex.Color.G),
                            (byte)(slot._colorTransform.blueMultiplier * vertex.Color.B), (byte)(slot._colorTransform.alphaMultiplier * vertex.Color.A),
                            slot.global.rotation,
                            data.pivot.x, data.pivot.y,
                            EFlip.None, ref temp);
                        graphics.InputVertexToOutputVertex(ref temp, vertexCount);
                        vertexCount += 4;
                        primitiveCount += 2;
                    }
                    else if (display is MeshDisplayData)
                    {
                        //var data = (MeshDisplayData)display;
                        //var intArray = data.vertices.data.intArray;
                        //var floatArray = data.vertices.data.floatArray;
                        //var vc = intArray[data.vertices.offset + (int)BinaryOffset.MeshVertexCount];
                        //var tc = intArray[data.vertices.offset + (int)BinaryOffset.MeshTriangleCount];
                        //int vertexOffset = intArray[data.vertices.offset + (int)BinaryOffset.MeshFloatOffset];
                        //var textureScale = slot._armature.armatureData.scale;

                        //if (vertexOffset < 0)
                        //    vertexOffset += 65536; // Fixed out of bounds bug. 

                        //var uvOffset = vertexOffset + vertexCount * 2;
                        //var scale = slot._armature._armatureData.scale;

                        //for (int j = 0, iV = vertexOffset, iU = uvOffset, l = vertexCount; j < l; ++j)
                        //{
                        //    vertices[vertexCount].UV.X = (data.texture.region.x + floatArray[iU++] * data.texture.region.width);
                        //    vertices[vertexCount].UV.Y = 1.0f - (data.texture.region.y + floatArray[iU++] * data.texture.region.height);

                        //    vertices[vertexCount].Position.X = floatArray[iV++] * textureScale;
                        //    vertices[vertexCount].Position.Y = floatArray[iV++] * textureScale;
                        //    vertices[vertexCount].Position.Z = 0;

                        //    vertices[vertexCount].Color.R = (byte)(slot._colorTransform.redMultiplier * vertex.Color.R);
                        //    vertices[vertexCount].Color.G = (byte)(slot._colorTransform.greenMultiplier * vertex.Color.G);
                        //    vertices[vertexCount].Color.B = (byte)(slot._colorTransform.blueMultiplier * vertex.Color.B);
                        //    vertices[vertexCount].Color.A = (byte)(slot._colorTransform.alphaMultiplier * vertex.Color.A);

                        //    vertexCount++;
                        //}

                        //primitiveCount += tc;
                    }
                    else if (display is Armature)
                    {
                        var data = (Armature)display;
                        ((DRAGONBONES)data.display).Draw(graphics, ref vertex);
                    }
                    else
                        throw new NotImplementedException();
                }
            }

            graphics.DrawPrimitives(TextureData.Texture, EPrimitiveType.Triangle,
                vertices, 0, vertexCount,
                graphics.GetIndicesBuffer(), 0, primitiveCount);

            graphics.End();

            return true;
        }
        protected override Content Cache()
        {
            var armature = PipelineDragonBones.factory.BuildArmature(null, this.DragonBonesData, this.TextureData);
            var cache = (DRAGONBONES)armature.display;
            cache._Key = this._Key;
            cache.Proxy.Animation.Play();
            return cache;
        }
    }
    public class PipelineDragonBones : ContentPipeline
    {
        /// <summary>龙骨骼文件加载的后缀名dbs</summary>
        public const string SUFFIX = "dbs";
        internal static DBFactory factory = new DBFactory();
        static PipelineDragonBones()
        {
            global::DragonBones.DragonBones.yDown = true;
        }

        public override IEnumerable<string> SuffixProcessable
        {
            get { yield return SUFFIX; }
        }

        protected override Content Load(string file)
        {
            string temp = file.Substring(0, file.LastIndexOf('.'));
            string filename = Path.GetFileNameWithoutExtension(file);

            string loadfile = temp + "_ske.json";
            string stringdata = Manager.IODevice.ReadText(loadfile);
            var dragonbonesData = BaseFactory._objectParser.ParseDragonBonesData(new JsonReader(stringdata).ReadDictionary());

            loadfile = temp + "_tex.png";
            var texture = Manager.Load<TEXTURE>(loadfile);

            loadfile = temp + "_tex.json";
            stringdata = Manager.IODevice.ReadText(loadfile);
            var textureData = factory.ParseTextureAtlasData(new JsonReader(stringdata).ReadDictionary(), texture);

            var armature = factory.BuildArmature(null, dragonbonesData, textureData);
            var result = (DRAGONBONES)armature.display;
            return result;
        }
        protected override void LoadAsync(AsyncLoadContent async)
        {
            string file = async.File;
            string temp = file.Substring(0, file.LastIndexOf('.'));
            string filename = Path.GetFileNameWithoutExtension(file);

            string loadfile = temp + "_ske.json";
            Wait(async, Manager.IODevice.ReadAsync(loadfile), r1 =>
            {
                string stringdata = Manager.IODevice.ReadPreambleText(r1.Data);
                var dragonbonesData = BaseFactory._objectParser.ParseDragonBonesData(new JsonReader(stringdata).ReadDictionary());

                loadfile = temp + "_tex.png";
                Manager.LoadAsync<TEXTURE>(loadfile, r2 =>
                {
                    loadfile = temp + "_tex.json";
                    Wait(async, Manager.IODevice.ReadAsync(loadfile), r3 =>
                    {
                        stringdata = Manager.IODevice.ReadPreambleText(r3.Data);
                        var textureData = factory.ParseTextureAtlasData(new JsonReader(stringdata).ReadDictionary(), r2);

                        var armature = factory.BuildArmature(null, dragonbonesData, textureData);
                        var result = (DRAGONBONES)armature.display;
                        async.SetData(result);
                    });
                });
            });
        }
    }
}
