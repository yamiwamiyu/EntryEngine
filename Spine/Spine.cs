using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine;
using System.IO;

namespace Spine
{
    public class SPINE : TEXTURE
    {
        private const int TL = 0;
        private const int TR = 1;
        private const int BL = 2;
        private const int BR = 3;
        private const int INIT_VERTEX_BUFFER = 128;
        private static short[] quadTriangles = { 0, 1, 2, 1, 3, 2 };
        private static float[] _vertices = new float[INIT_VERTEX_BUFFER * 2];
        private static TextureVertex[] vertices = new TextureVertex[INIT_VERTEX_BUFFER];
        static void AllocateBuffer(int vertexCount)
        {
            if (vertexCount <= vertices.Length)
                return;
            vertices = new TextureVertex[vertexCount];
            _vertices = new float[vertexCount * 2];
        }

        SkeletonClipping clipper = new SkeletonClipping();

        public override int Width
        {
            get { return (int)Skeleton.Data.Width; }
        }
        public override int Height
        {
            get { return (int)Skeleton.Data.Height; }
        }
        public override bool IsDisposed
        {
            get { return Skeleton == null; }
        }
        public AnimationState Animation
        {
            get;
            private set;
        }
        public Skeleton Skeleton
        {
            get;
            private set;
        }

        internal SPINE(SkeletonData skeletonData)
        {
            this.Skeleton = new Skeleton(skeletonData);
            AnimationStateData stateData = new AnimationStateData(skeletonData);
            this.Animation = new AnimationState(stateData);
        }

        public override void Update(GameTime time)
        {
            if (IsDisposed)
                return;
            Animation.Update(time.ElapsedSecond);
            Animation.Apply(Skeleton);
            Skeleton.UpdateWorldTransform();
        }

        /// <summary>
        /// 除了坐标和颜色需要每次计算外，uv是固定的，顶点数量也是固定的，所以可以固定顶点数组和索引数组
        /// </summary>
        [Code(ECode.Optimize)]
        protected override bool Draw(GRAPHICS graphics, ref SpriteVertex vertex)
        {
            if (IsDisposed)
                return true;

            var drawOrder = Skeleton.DrawOrder;
            if (drawOrder.Count == 0)
                return true;

            MATRIX2x3 matrix;
            __GRAPHICS.DrawMatrix(ref vertex.Destination, ref vertex.Source, vertex.Rotation, ref vertex.Origin, vertex.Flip, out matrix);
            graphics.BeginFromPrevious(matrix);

            var drawOrderItems = Skeleton.DrawOrder.Items;
            float skeletonR = Skeleton.R, skeletonG = Skeleton.G, skeletonB = Skeleton.B, skeletonA = Skeleton.A;
            for (int i = 0, n = drawOrder.Count; i < n; i++)
            {
                Slot slot = drawOrderItems[i];
                Attachment attachment = slot.Attachment;

                #region render
                if (attachment is RegionAttachment)
                {
                    RegionAttachment regionAttachment = (RegionAttachment)attachment;
                    //BlendState blend = slot.Data.BlendMode == BlendMode.additive ? BlendState.Additive : defaultBlendState;
                    //if (device.BlendState != blend)
                    //{
                    //    End();
                    //    device.BlendState = blend;
                    //}

                    //MeshItem item = batcher.NextItem(4, 6);
                    //item.triangles = quadTriangles;
                    TextureVertex[] itemVertices = vertices;

                    AtlasRegion region = (AtlasRegion)regionAttachment.RendererObject;
                    //item.texture = (TEXTURE)region.page.rendererObject;
                    TEXTURE texture = (TEXTURE)region.page.rendererObject;

                    COLOR color;
                    float a = skeletonA * slot.A * regionAttachment.A;
                    bool premultipliedAlpha = false;
                    if (premultipliedAlpha)
                    {
                        color = new COLOR(
                                skeletonR * slot.R * regionAttachment.R * a,
                                skeletonG * slot.G * regionAttachment.G * a,
                                skeletonB * slot.B * regionAttachment.B * a, a);
                    }
                    else
                    {
                        color = new COLOR(
                                skeletonR * slot.R * regionAttachment.R,
                                skeletonG * slot.G * regionAttachment.G,
                                skeletonB * slot.B * regionAttachment.B, a);
                    }
                    itemVertices[TL].Color = color;
                    itemVertices[BL].Color = color;
                    itemVertices[BR].Color = color;
                    itemVertices[TR].Color = color;

                    regionAttachment.ComputeWorldVertices(slot.Bone, _vertices, 0, 2);
                    itemVertices[TL].Position.X = _vertices[RegionAttachment.ULX];
                    itemVertices[TL].Position.Y = _vertices[RegionAttachment.ULY];
                    itemVertices[TL].Position.Z = 0;
                    itemVertices[BL].Position.X = _vertices[RegionAttachment.BLX];
                    itemVertices[BL].Position.Y = _vertices[RegionAttachment.BLY];
                    itemVertices[BL].Position.Z = 0;
                    itemVertices[BR].Position.X = _vertices[RegionAttachment.BRX];
                    itemVertices[BR].Position.Y = _vertices[RegionAttachment.BRY];
                    itemVertices[BR].Position.Z = 0;
                    itemVertices[TR].Position.X = _vertices[RegionAttachment.URX];
                    itemVertices[TR].Position.Y = _vertices[RegionAttachment.URY];
                    itemVertices[TR].Position.Z = 0;

                    float[] uvs = regionAttachment.UVs;
                    itemVertices[TL].TextureCoordinate.X = uvs[RegionAttachment.ULX];
                    itemVertices[TL].TextureCoordinate.Y = uvs[RegionAttachment.ULY];
                    itemVertices[BL].TextureCoordinate.X = uvs[RegionAttachment.BLX];
                    itemVertices[BL].TextureCoordinate.Y = uvs[RegionAttachment.BLY];
                    itemVertices[BR].TextureCoordinate.X = uvs[RegionAttachment.BRX];
                    itemVertices[BR].TextureCoordinate.Y = uvs[RegionAttachment.BRY];
                    itemVertices[TR].TextureCoordinate.X = uvs[RegionAttachment.URX];
                    itemVertices[TR].TextureCoordinate.Y = uvs[RegionAttachment.URY];

                    graphics.DrawPrimitives(texture, vertices, 0, 4, quadTriangles, 0, 2);
                }
                else if (attachment is MeshAttachment)
                {
                    MeshAttachment mesh = (MeshAttachment)attachment;
                    int vertexCount = mesh.WorldVerticesLength;
                    if (vertices.Length < vertexCount) AllocateBuffer(vertexCount);
                    mesh.ComputeWorldVertices(slot, _vertices);

                    short[] triangles = mesh.Triangles;
                    //MeshItem item = batcher.NextItem(vertexCount, triangles.Length);
                    //item.triangles = triangles;

                    AtlasRegion region = (AtlasRegion)mesh.RendererObject;
                    //item.texture = (TEXTURE)region.page.rendererObject;
                    TEXTURE texture = (TEXTURE)region.page.rendererObject;

                    COLOR color;
                    float a = skeletonA * slot.A * mesh.A;
                    bool premultipliedAlpha = false;
                    if (premultipliedAlpha)
                    {
                        color = new COLOR(
                                skeletonR * slot.R * mesh.R * a,
                                skeletonG * slot.G * mesh.G * a,
                                skeletonB * slot.B * mesh.B * a, a);
                    }
                    else
                    {
                        color = new COLOR(
                                skeletonR * slot.R * mesh.R,
                                skeletonG * slot.G * mesh.G,
                                skeletonB * slot.B * mesh.B, a);
                    }

                    float[] uvs = mesh.UVs;
                    TextureVertex[] itemVertices = vertices;
                    for (int ii = 0, v = 0; v < vertexCount; ii++, v += 2)
                    {
                        itemVertices[ii].Color = color;
                        itemVertices[ii].Position.X = _vertices[v];
                        itemVertices[ii].Position.Y = _vertices[v + 1];
                        itemVertices[ii].Position.Z = 0;
                        itemVertices[ii].TextureCoordinate.X = uvs[v];
                        itemVertices[ii].TextureCoordinate.Y = uvs[v + 1];
                    }

                    graphics.DrawPrimitives(texture, vertices, 0, vertexCount / 2, triangles, 0, triangles.Length / 3);
                }
                #endregion
            }

            graphics.End();

            return true;
        }
        protected override void InternalDispose()
        {
            if (Skeleton != null)
            {
                for (int i = 0, n = Skeleton.Slots.Count; i < n; i++)
                {
                    var slot = Skeleton.Slots.Items[i];
                    Attachment attachment = slot.Attachment;
                    if (attachment is RegionAttachment)
                    {
                        RegionAttachment regionAttachment = (RegionAttachment)attachment;
                        AtlasRegion region = (AtlasRegion)regionAttachment.RendererObject;
                        TEXTURE texture = (TEXTURE)region.page.rendererObject;
                        if (texture != null)
                            texture.Dispose();
                    }
                    else if (attachment is MeshAttachment)
                    {
                        MeshAttachment mesh = (MeshAttachment)attachment;
                        AtlasRegion region = (AtlasRegion)mesh.RendererObject;
                        TEXTURE texture = (TEXTURE)region.page.rendererObject;
                        if (texture != null)
                            texture.Dispose();
                    }
                }
                Skeleton = null;
                Animation = null;
            }
        }
        protected override Content Cache()
        {
            SPINE spine = new SPINE(Skeleton.Data);
            spine._Key = this._Key;
            return spine;
        }
    }
    public class PipelineSpine : ContentPipeline
    {
        static PipelineSpine()
        {
            Bone.yDown = true;
        }

        class SpineAtlasLoader : TextureLoader
        {
            internal PipelineSpine Pipeline;
            internal List<AsyncLoadContent> Asyncs;

            public SpineAtlasLoader(PipelineSpine pipeline, bool async)
            {
                this.Pipeline = pipeline;
                if (async)
                    Asyncs = new List<AsyncLoadContent>();
            }

            void TextureLoader.Load(AtlasPage page, string path)
            {
                if (Asyncs == null)
                {
                    TEXTURE texture = Pipeline.Manager.Load<TEXTURE>(path);
                    page.rendererObject = texture;
                    page.width = texture.Width;
                    page.height = texture.Height;
                }
                else
                {
                    var async = Pipeline.Manager.LoadAsync<TEXTURE>(path,
                        texture =>
                        {
                            page.rendererObject = texture;
                            page.width = texture.Width;
                            page.height = texture.Height;
                        });
                    if (!async.IsSuccess)
                        Asyncs.Add(async);
                }
            }
            void TextureLoader.Unload(object texture)
            {
                ((TEXTURE)texture).Dispose();
            }
        }

        public override IEnumerable<string> SuffixProcessable
        {
            get { yield return "spine"; }
        }

        protected override Content Load(string file)
        {
            SpineAtlasLoader loader = new SpineAtlasLoader(this, false);

            string temp = file.Substring(0, file.LastIndexOf('.'));
            string dir = Path.GetDirectoryName(temp);

            string text = IO.ReadText(temp + ".atlas");
            TextReader reader = new StringReader(text);
            Atlas atlas = new Atlas(reader, dir, loader);

            byte[] data = IO.ReadByte(temp + ".skel.bytes");
            SkeletonBinary binary = new SkeletonBinary(atlas);
            binary.Scale = 0.097f;
            //binary.Scale = 1f;
            SkeletonData skeletonData = binary.ReadSkeletonData(new MemoryStream(data));

            //string text = IO.ReadText(temp + ".atlas");
            //TextReader reader = new StringReader(text);
            //Atlas atlas = new Atlas(reader, dir, loader);

            //string data = IO.ReadText(temp + ".json");
            //SkeletonJson json = new SkeletonJson(atlas);
            //SkeletonData skeletonData = json.ReadSkeletonData(new StringReader(data));

            SPINE spine = new SPINE(skeletonData);
            return spine;
        }
        protected override void LoadAsync(AsyncLoadContent async)
        {
            SpineAtlasLoader loader = new SpineAtlasLoader(this, true);

            string file = async.File;
            string temp = file.Substring(0, file.LastIndexOf('.'));
            string dir = Path.GetDirectoryName(temp);

            Wait(async, IO.ReadAsync(temp + ".atlas"),
                wait =>
                {
                    string text = IO.ReadPreambleText(wait.Data);
                    TextReader reader = new StringReader(text);
                    Atlas atlas = new Atlas(reader, dir, loader);

                    Wait(async, IO.ReadAsync(temp + ".skel"),
                        wait2 =>
                        {
                            SkeletonBinary binary = new SkeletonBinary(atlas);
                            SkeletonData skeletonData = binary.ReadSkeletonData(new MemoryStream(wait2.Data));

                            SPINE spine = new SPINE(skeletonData);
                            if (loader.Asyncs.Count == 0)
                                async.SetData(spine);
                            else
                                Wait(async, loader.Asyncs, () => spine);
                        });
                });
        }
    }
}
