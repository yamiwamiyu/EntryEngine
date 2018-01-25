using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntryEngine.HTML5
{
    public static class _HTML5
    {
        //public static Image
        //public static WebGLTexture GetTextureGL(this TEXTURE texture)
        //{
        //    if (texture == null)
        //        return null;
        //    var t = texture as TextureJS;
        //    if (t == null)
        //    {
        //        TEXTURE_Link tl = texture as TEXTURE_Link;
        //        if (tl == null)
        //            //throw new System.ArgumentException("Not supported texture type.");
        //            return null;
        //        return GetTextureGL(tl.Base);
        //    }
        //    else
        //        return t.Texture;
        //}
        public static byte[] GetBytes(this Uint8ClampedArray array)
        {
            int len = array.byteLength;
            byte[] bytes = new byte[len];
            for (int i = 0; i < len; i++)
                bytes[i] = array[i];
            return bytes;
        }
        public static void ToUint8ClampedArray(this byte[] bytes, Uint8ClampedArray array)
        {
            int len = bytes.Length;
            for (int i = 0; i < len; i++)
                array[i] = bytes[i];
        }
    }
}
