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
            Armature armature = new Armature();
            armature.Init(dataPackage.armature, display, display, _dragonBonesInstance);

            display.DragonBonesData = dataPackage.data;
            display.Texture = (DBTextureAtlasData)dataPackage.texture;
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
    public class DRAGONBONES : TEXTURE, IArmatureProxy
    {
        private const int TL = 0;
        private const int TR = 1;
        private const int BL = 2;
        private const int BR = 3;
        private static short[] quadTriangles = { 0, 1, 2, 1, 3, 2 };
        private static TextureVertex[] vertices = new TextureVertex[128];

        internal DragonBonesData DragonBonesData;
        internal Armature Armature;
        internal DBTextureAtlasData Texture;
        int updated;

        public override int Width
        {
            get { return (int)Armature._armatureData.aabb.width; }
        }
        public override int Height
        {
            get { return (int)Armature._armatureData.aabb.height; }
        }
        public override bool IsDisposed
        {
            get { return Armature == null; }
        }

        internal DRAGONBONES() { }

        public override void Update(GameTime time)
        {
            updated = GameTime.Time.FrameID;
            if (Armature != null)
                Armature.AdvanceTime(time.ElapsedSecond);
        }
        protected override void InternalDispose()
        {
            if (Armature != null)
            {
                Armature.Dispose();
                Armature = null;
            }
            if (Texture != null)
            {
                if (Texture.Texture != null)
                {
                    Texture.Texture.Dispose();
                    Texture.Texture = null;
                }
                Texture = null;
            }
        }
        protected override bool Draw(GRAPHICS graphics, ref SpriteVertex vertex)
        {
            if (Armature == null) return true;

            if (updated != GameTime.Time.FrameID)
            {
                updated = GameTime.Time.FrameID;
                Update(GameTime.Time);
            }

            var slots = Armature.GetSlots();
            if (slots.Count == 0) return true;

            //armature.flipX = true;
            //armature.flipY = true;
            //vertex.Flip = EFlip.FlipHorizontally;

            MATRIX2x3 matrix;
            __GRAPHICS.DrawMatrix(ref vertex.Destination, ref vertex.Source, vertex.Rotation, ref vertex.Origin, vertex.Flip, out matrix);
            graphics.BeginFromPrevious(matrix);

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

                        //var textureScale = slot._armature.armatureData.scale * currentTextureData.parent.scale;
                        //var sourceX = currentTextureData.region.x;
                        //var sourceY = currentTextureData.region.y;
                        //var sourceWidth = currentTextureData.region.width;
                        //var sourceHeight = currentTextureData.region.height;

                        //var scaleWidth = sourceWidth * textureScale;
                        //var scaleHeight = sourceHeight * textureScale;
                        //var pivotX = slot._pivotX;
                        //var pivotY = slot._pivotY;

                        //{
                        //    byte r = (byte)(vertex.Color.R * slot._colorTransform.redMultiplier);
                        //    byte g = (byte)(vertex.Color.G * slot._colorTransform.greenMultiplier);
                        //    byte b = (byte)(vertex.Color.B * slot._colorTransform.blueMultiplier);
                        //    byte a = (byte)(vertex.Color.A * slot._colorTransform.alphaMultiplier);
                        //    for (int j = 0; j < 4; j++)
                        //    {
                        //        vertices[j].Color.R = r;
                        //        vertices[j].Color.G = g;
                        //        vertices[j].Color.B = b;
                        //        vertices[j].Color.A = a;
                        //    }
                        //}

                        //{
                        //    var sm = slot.parent.globalTransformMatrix;
                        //    var bm = slot.globalTransformMatrix;
                        //    float[] vertexOffset = 
                        //    {
                        //        sm.tx, sm.ty,
                        //        sm.tx + sourceWidth, sm.ty,
                        //        sm.tx, sm.ty + sourceHeight,
                        //        sm.tx + sourceWidth, sm.ty + sourceHeight,
                        //    };
                        //    float bwx = bm.tx + vertex.Destination.X, bwy = bm.ty + vertex.Destination.X;
                        //    float a = bm.a, b = bm.b, c = bm.c, d = bm.d;
                        //    float offsetX, offsetY;

                        //    offsetX = vertexOffset[0]; // 0
                        //    offsetY = vertexOffset[1]; // 1
                        //    vertices[0].Position.X = offsetX * a + offsetY * b + bwx; // bl
                        //    vertices[0].Position.Y = offsetX * c + offsetY * d + bwy;

                        //    offsetX = vertexOffset[2]; // 2
                        //    offsetY = vertexOffset[3]; // 3
                        //    vertices[1].Position.X = offsetX * a + offsetY * b + bwx; // ul
                        //    vertices[1].Position.Y = offsetX * c + offsetY * d + bwy;

                        //    offsetX = vertexOffset[4]; // 4
                        //    offsetY = vertexOffset[5]; // 5
                        //    vertices[2].Position.X = offsetX * a + offsetY * b + bwx; // ur
                        //    vertices[2].Position.Y = offsetX * c + offsetY * d + bwy;

                        //    offsetX = vertexOffset[6]; // 6
                        //    offsetY = vertexOffset[7]; // 7
                        //    vertices[3].Position.X = offsetX * a + offsetY * b + bwx; // br
                        //    vertices[3].Position.Y = offsetX * c + offsetY * d + bwy;

                        //    //vertices
                        //    //vertices[0].Position.X = 0 * scaleWidth - pivotX * sourceWidth;
                        //    //vertices[0].Position.Y = 0 * scaleHeight - pivotY * sourceHeight;

                        //    //vertices[1].Position.X = 1 * scaleWidth - pivotX * sourceWidth;
                        //    //vertices[1].Position.Y = 0 * scaleHeight - pivotY * sourceHeight;

                        //    //vertices[2].Position.X = 0 * scaleWidth - pivotX * sourceWidth;
                        //    //vertices[2].Position.Y = 1 * scaleHeight - pivotY * sourceHeight;

                        //    //vertices[3].Position.X = 1 * scaleWidth - pivotX * sourceWidth;
                        //    //vertices[3].Position.Y = 1 * scaleHeight - pivotY * sourceHeight;
                        //}

                        //{
                        //    vertices[0].TextureCoordinate.X = data.texture.region.x;
                        //    vertices[0].TextureCoordinate.Y = data.texture.region.y;
                        //    vertices[1].TextureCoordinate.X = data.texture.region.x + data.texture.region.width;
                        //    vertices[1].TextureCoordinate.Y = data.texture.region.y;
                        //    vertices[2].TextureCoordinate.X = data.texture.region.x;
                        //    vertices[2].TextureCoordinate.Y = data.texture.region.y + data.texture.region.height;
                        //    vertices[3].TextureCoordinate.X = data.texture.region.x + data.texture.region.width;
                        //    vertices[3].TextureCoordinate.Y = data.texture.region.y + data.texture.region.height;
                        //}

                        //graphics.DrawPrimitives(Texture.Texture, vertices, 0, 4, quadTriangles, 0, 2);


                        graphics.BaseDraw(Texture.Texture,
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
                            EFlip.None);
                    }
                    else if (display is MeshDisplayData)
                    {
                        //var data = (MeshDisplayData)display;
                        //graphics.BaseDraw(Texture.Texture, data.transform.x, data.transform.y,
                        //    data.texture.region.width, data.texture.region.height,
                        //    false,
                        //    data.texture.region.x, data.texture.region.y, data.texture.region.width, data.texture.region.height,
                        //    true,
                        //    (byte)(slot._colorTransform.redMultiplier * 255), (byte)(slot._colorTransform.greenMultiplier * 255),
                        //    (byte)(slot._colorTransform.blueMultiplier * 255), (byte)(slot._colorTransform.alphaMultiplier * 255),
                        //    data.transform.rotation,
                        //    slot._pivotX, slot._pivotY,
                        //    EFlip.None);
                    }
                    else if (display is Armature)
                    {
                        var data = (Armature)display;
                        ((DRAGONBONES)data.proxy).Draw(graphics, ref vertex);
                    }
                    else
                        throw new NotImplementedException();
                }
            }

            graphics.End();

            return true;
        }
        protected override Content Cache()
        {
            var armature = PipelineDragonBones.factory.BuildArmature(null, this.DragonBonesData, this.Texture);
            var cache = (DRAGONBONES)armature.proxy;
            cache._Key = this._Key;
            cache.animation.Play();
            return cache;
        }

        #region IArmatureProxy
        public void DBInit(Armature armature)
        {
            this.Armature = armature;
        }
        public void DBClear()
        {
        }
        public void DBUpdate()
        {
            //this._armature.AdvanceTime(GameTime.Time.ElapsedSecond);
        }
        public void Dispose(bool disposeProxy)
        {
        }
        public Armature armature
        {
            get { return Armature; }
        }
        /// <summary>龙骨骼动画</summary>
        public Animation animation
        {
            get { return Armature.animation; }
        }
        public bool HasDBEventListener(string type)
        {
            return false;
        }
        public void DispatchDBEvent(string type, EventObject eventObject)
        {
        }
        public void AddDBEventListener(string type, ListenerDelegate<EventObject> listener)
        {
        }
        public void RemoveDBEventListener(string type, ListenerDelegate<EventObject> listener)
        {
        }
        #endregion
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
            var result = (DRAGONBONES)armature.proxy;
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
                        var result = (DRAGONBONES)armature.proxy;
                        async.SetData(result);
                    });
                });
            });
        }
    }
}
