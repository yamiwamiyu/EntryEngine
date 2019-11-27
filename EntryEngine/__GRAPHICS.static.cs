#if CLIENT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntryEngine
{
    public static partial class __GRAPHICS
    {
        private static EntryEngine.GRAPHICS __instance { get { return Entry._GRAPHICS; } }
        public static EntryEngine.COLOR DefaultColor
        {
            get { return __instance.DefaultColor; }
            set { __instance.DefaultColor = value; }
        }
        public static float[] XCornerOffsets
        {
            get { return __instance.XCornerOffsets; }
        }
        public static float[] YCornerOffsets
        {
            get { return __instance.YCornerOffsets; }
        }
        public static bool Culling
        {
            get { return __instance.Culling; }
            set { __instance.Culling = value; }
        }
        public static EntryEngine.EViewport ViewportMode
        {
            get { return __instance.ViewportMode; }
            set { __instance.ViewportMode = value; }
        }
        public static EntryEngine.VECTOR2 ScreenSize
        {
            get { return __instance.ScreenSize; }
            set { __instance.ScreenSize = value; }
        }
        public static EntryEngine.RECT FullScreenArea
        {
            get { return __instance.FullScreenArea; }
        }
        public static EntryEngine.VECTOR2 GraphicsSize
        {
            get { return __instance.GraphicsSize; }
            set { __instance.GraphicsSize = value; }
        }
        public static EntryEngine.RECT FullGraphicsArea
        {
            get { return __instance.FullGraphicsArea; }
        }
        public static EntryEngine.VECTOR2 OnePixel
        {
            get { return __instance.OnePixel; }
        }
        public static EntryEngine.MATRIX2x3 View
        {
            get { return __instance.View; }
        }
        public static EntryEngine.RECT Viewport
        {
            get { return __instance.Viewport; }
        }
        public static bool IsFullScreen
        {
            get { return __instance.IsFullScreen; }
            set { __instance.IsFullScreen = value; }
        }
        public static int RenderTargetCount
        {
            get { return __instance.RenderTargetCount; }
        }
        public static EntryEngine.MATRIX2x3 CurrentTransform
        {
            get { return __instance.CurrentTransform; }
        }
        public static EntryEngine.RECT CurrentGraphics
        {
            get { return __instance.CurrentGraphics; }
        }
        public static EntryEngine.VECTOR2 PointToGraphics(EntryEngine.VECTOR2 point)
        {
            return __instance.PointToGraphics(point);
        }
        public static EntryEngine.VECTOR2 PointToScreen(EntryEngine.VECTOR2 point)
        {
            return __instance.PointToScreen(point);
        }
        public static EntryEngine.VECTOR2 PointToViewport(EntryEngine.VECTOR2 point)
        {
            return __instance.PointToViewport(point);
        }
        public static EntryEngine.RECT AreaToGraphics(EntryEngine.RECT rect)
        {
            return __instance.AreaToGraphics(rect);
        }
        public static EntryEngine.RECT AreaToScreen(EntryEngine.RECT rect)
        {
            return __instance.AreaToScreen(rect);
        }
        public static EntryEngine.RECT AreaToViewport(EntryEngine.RECT rect)
        {
            return __instance.AreaToViewport(rect);
        }
        public static EntryEngine.VECTOR2 ToPixel(EntryEngine.VECTOR2 pixel)
        {
            return __instance.ToPixel(pixel);
        }
        public static EntryEngine.VECTOR2 ToPixelCeiling(EntryEngine.VECTOR2 pixel)
        {
            return __instance.ToPixelCeiling(pixel);
        }
        public static void Begin(EntryEngine.MATRIX transform, EntryEngine.RECT graphics, EntryEngine.SHADER shader)
        {
            __instance.Begin(transform, graphics, shader);
        }
        public static void Begin()
        {
            __instance.Begin();
        }
        public static void Begin(EntryEngine.MATRIX2x3 transform)
        {
            __instance.Begin(transform);
        }
        public static void Begin(EntryEngine.RECT graphics)
        {
            __instance.Begin(graphics);
        }
        public static void Begin(EntryEngine.MATRIX2x3 transform, EntryEngine.RECT graphics)
        {
            __instance.Begin(transform, graphics);
        }
        public static void Begin(EntryEngine.MATRIX2x3 transform, EntryEngine.RECT graphics, EntryEngine.SHADER shader)
        {
            __instance.Begin(transform, graphics, shader);
        }
        public static void Begin(EntryEngine.SHADER shader)
        {
            __instance.Begin(shader);
        }
        public static void BeginFromPrevious(EntryEngine.MATRIX2x3 matrix)
        {
            __instance.BeginFromPrevious(matrix);
        }
        public static void BeginFromPrevious(EntryEngine.RECT rect)
        {
            __instance.BeginFromPrevious(rect);
        }
        public static void BeginFromPrevious(EntryEngine.MATRIX2x3 matrix, EntryEngine.RECT rect)
        {
            __instance.BeginFromPrevious(matrix, rect);
        }
        public static EntryEngine.MATRIX2x3 FromPrevious(EntryEngine.MATRIX2x3 matrix)
        {
            return __instance.FromPrevious(matrix);
        }
        public static EntryEngine.RECT FromPrevious(EntryEngine.RECT rect)
        {
            return __instance.FromPrevious(rect);
        }
        public static EntryEngine.RECT FromPreviousNonOffset(EntryEngine.RECT rect)
        {
            return __instance.FromPreviousNonOffset(rect);
        }
        public static void Clear()
        {
            __instance.Clear();
        }
        public static void End()
        {
            __instance.End();
        }
        public static EntryEngine.TEXTURE Screenshot()
        {
            return __instance.Screenshot();
        }
        public static EntryEngine.TEXTURE Screenshot(EntryEngine.RECT graphics)
        {
            return __instance.Screenshot(graphics);
        }
        public static void Screenshot(EntryEngine.RECT graphics, System.Action<EntryEngine.TEXTURE> callback)
        {
            __instance.Screenshot(graphics, callback);
        }
        public static void BeginScreenshot(EntryEngine.RECT graphics)
        {
            __instance.BeginScreenshot(graphics);
        }
        public static EntryEngine.TEXTURE EndScreenshot()
        {
            return __instance.EndScreenshot();
        }
        public static void Draw(EntryEngine.TEXTURE texture, EntryEngine.RECT rect)
        {
            __instance.Draw(texture, rect);
        }
        public static void Draw(EntryEngine.TEXTURE texture, EntryEngine.RECT rect, EntryEngine.COLOR color)
        {
            __instance.Draw(texture, rect, color);
        }
        public static void Draw(EntryEngine.TEXTURE texture, EntryEngine.RECT rect, EntryEngine.RECT source, EntryEngine.COLOR color)
        {
            __instance.Draw(texture, rect, source, color);
        }
        public static void Draw(EntryEngine.TEXTURE texture, EntryEngine.RECT rect, float rotation, float originX, float originY)
        {
            __instance.Draw(texture, rect, rotation, originX, originY);
        }
        public static void Draw(EntryEngine.TEXTURE texture, EntryEngine.RECT rect, float rotation, float originX, float originY, EntryEngine.EFlip flip)
        {
            __instance.Draw(texture, rect, rotation, originX, originY, flip);
        }
        public static void Draw(EntryEngine.TEXTURE texture, EntryEngine.RECT rect, EntryEngine.COLOR color, float rotation, float originX, float originY, EntryEngine.EFlip flip)
        {
            __instance.Draw(texture, rect, color, rotation, originX, originY, flip);
        }
        public static void Draw(EntryEngine.TEXTURE texture, EntryEngine.RECT rect, EntryEngine.RECT source, EntryEngine.COLOR color, float rotation, float originX, float originY, EntryEngine.EFlip flip)
        {
            __instance.Draw(texture, rect, source, color, rotation, originX, originY, flip);
        }
        public static void Draw(EntryEngine.TEXTURE texture, EntryEngine.VECTOR2 location)
        {
            __instance.Draw(texture, location);
        }
        public static void Draw(EntryEngine.TEXTURE texture, EntryEngine.VECTOR2 location, EntryEngine.COLOR color)
        {
            __instance.Draw(texture, location, color);
        }
        public static void Draw(EntryEngine.TEXTURE texture, EntryEngine.VECTOR2 location, float rotation, float originX, float originY)
        {
            __instance.Draw(texture, location, rotation, originX, originY);
        }
        public static void Draw(EntryEngine.TEXTURE texture, EntryEngine.VECTOR2 location, float rotation, float originX, float originY, float scaleX, float scaleY)
        {
            __instance.Draw(texture, location, rotation, originX, originY, scaleX, scaleY);
        }
        public static void Draw(EntryEngine.TEXTURE texture, EntryEngine.VECTOR2 location, float rotation, float originX, float originY, float scaleX, float scaleY, EntryEngine.EFlip flip)
        {
            __instance.Draw(texture, location, rotation, originX, originY, scaleX, scaleY, flip);
        }
        public static void Draw(EntryEngine.TEXTURE texture, EntryEngine.VECTOR2 location, EntryEngine.COLOR color, float rotation, float originX, float originY, float scaleX, float scaleY)
        {
            __instance.Draw(texture, location, color, rotation, originX, originY, scaleX, scaleY);
        }
        public static void Draw(EntryEngine.TEXTURE texture, EntryEngine.VECTOR2 location, EntryEngine.COLOR color, float rotation, float originX, float originY, float scaleX, float scaleY, EntryEngine.EFlip flip)
        {
            __instance.Draw(texture, location, color, rotation, originX, originY, scaleX, scaleY, flip);
        }
        public static void Draw(EntryEngine.TEXTURE texture, EntryEngine.VECTOR2 location, EntryEngine.RECT source, EntryEngine.COLOR color, float rotation, EntryEngine.VECTOR2 origin, EntryEngine.VECTOR2 scale, EntryEngine.EFlip flip)
        {
            __instance.Draw(texture, location, source, color, rotation, origin, scale, flip);
        }
        public static void BaseDraw(EntryEngine.TEXTURE texture, float x, float y, float w, float h, bool scale, float sx, float sy, float sw, float sh, bool color, byte r, byte g, byte b, byte a, float rotation, float ox, float oy, EntryEngine.EFlip flip)
        {
            __instance.BaseDraw(texture, x, y, w, h, scale, sx, sy, sw, sh, color, r, g, b, a, rotation, ox, oy, flip);
        }
        public static void Draw(EntryEngine.TEXTURE texture, ref EntryEngine.SpriteVertex vertex)
        {
            __instance.Draw(texture, ref vertex);
        }
        public static void Draw(EntryEngine.FONT font, string text, EntryEngine.VECTOR2 location, EntryEngine.COLOR color)
        {
            __instance.Draw(font, text, location, color);
        }
        public static void Draw(EntryEngine.FONT font, string text, EntryEngine.VECTOR2 location, EntryEngine.COLOR color, float scale)
        {
            __instance.Draw(font, text, location, color, scale);
        }
        public static void Draw(EntryEngine.FONT font, string text, EntryEngine.RECT bound, EntryEngine.COLOR color, EntryEngine.UI.EPivot alignment)
        {
            __instance.Draw(font, text, bound, color, alignment);
        }
        public static void Draw(EntryEngine.FONT font, string text, EntryEngine.RECT bound, EntryEngine.COLOR color, EntryEngine.UI.EPivot alignment, float scale)
        {
            __instance.Draw(font, text, bound, color, alignment, scale);
        }
        public static void DrawPrimitives(EntryEngine.TEXTURE texture, EntryEngine.TextureVertex[] vertices, int offset, int count, short[] indexes, int indexOffset, int primitiveCount)
        {
            __instance.DrawPrimitives(texture, vertices, offset, count, indexes, indexOffset, primitiveCount);
        }
        public static void DrawPrimitives(EntryEngine.TEXTURE texture, EntryEngine.EPrimitiveType ptype, EntryEngine.TextureVertex[] vertices, int offset, int count, short[] indexes, int indexOffset, int primitiveCount)
        {
            __instance.DrawPrimitives(texture, ptype, vertices, offset, count, indexes, indexOffset, primitiveCount);
        }
    }
}

#endif
