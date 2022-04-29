using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntryEngine.WebGL
{
    public static class _WebGL
    {
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
