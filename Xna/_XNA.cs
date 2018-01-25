using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EntryEngine.Xna
{
    public static class _XNA
    {
        public static Vector2 GetVector2(this VECTOR2 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }
        public static Vector3 GetVector3(this VECTOR3 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }
        public static Vector4 GetVector4(this VECTOR4 vector)
        {
            return new Vector4(vector.X, vector.Y, vector.Z, vector.W);
        }
        public static Rectangle GetRect(this RECT rect)
        {
            return new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
        }
        public static Color GetColor(this COLOR color)
        {
            return new Color(color.R, color.G, color.B, color.A);
        }
        public static Matrix GetMatrix(this MATRIX matrix)
        {
            return new Matrix(
                matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                matrix.M41, matrix.M42, matrix.M43, matrix.M44);
        }
        public static Texture2D GetTexture(this TEXTURE texture)
        {
            if (texture == null)
                throw new System.ArgumentNullException("texture");
            TextureXna t = texture as TextureXna;
            if (t == null)
            {
                TEXTURE_Link tl = texture as TEXTURE_Link;
                if (tl == null)
                    throw new System.ArgumentException("Not supported texture type.");
                return GetTexture(tl.Base);
            }
            else
                return t.Texture2D;
        }
        public static VECTOR2 GetVector2(this Vector2 vector)
        {
            return new VECTOR2(vector.X, vector.Y);
        }
        public static VECTOR3 GetVector3(this Vector3 vector)
        {
            return new VECTOR3(vector.X, vector.Y, vector.Z);
        }
        public static VECTOR4 GetVector4(this Vector4 vector)
        {
            return new VECTOR4(vector.X, vector.Y, vector.Z, vector.W);
        }
        public static RECT GetRect(this Rectangle rect)
        {
            return new RECT(rect.X, rect.Y, rect.Width, rect.Height);
        }
        public static COLOR GetColor(this Color color)
        {
            return new COLOR(color.R, color.G, color.B, color.A);
        }
        public static MATRIX GetMatrix(this Matrix matrix)
        {
            return new MATRIX(
                matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                matrix.M41, matrix.M42, matrix.M43, matrix.M44);
        }
		public static Matrix GetMatrix(this MATRIX2x3 matrix)
		{
			return new Matrix(
				matrix.M11, matrix.M12, 0, 0,
				matrix.M21, matrix.M22, 0, 0,
				0, 0, 1, 0,
				matrix.M31, matrix.M32, 0, 1);
		}
        public static COLOR[] GetColor(this Texture2D texture)
        {
            COLOR[] color = new COLOR[texture.Width * texture.Height];
            texture.GetData(color);
            return color;
        }
        public static COLOR[] GetColor(this Texture2D texture, RECT? source)
        {
            Rectangle temp = source == null ? new Rectangle(0, 0, texture.Width, texture.Height) : source.Value.GetRect();
            COLOR[] color = new COLOR[temp.Width * temp.Height];
            texture.GetData(0, temp, color, 0, color.Length);
            return color;
        }
		public static byte[] GetBuffer(this Texture2D texture)
		{
			byte[] buffer = new byte[texture.Width * texture.Height * 4];
			texture.GetData(buffer);
			return buffer;
		}
		public static byte[] GetBuffer(this Texture2D texture, RECT? source)
		{
			Rectangle temp = source == null ? new Rectangle(0, 0, texture.Width, texture.Height) : source.Value.GetRect();
			byte[] buffer = new byte[temp.Width * temp.Height * 4];
			texture.GetData(0, temp, buffer, 0, buffer.Length);
			return buffer;
		}
        public static Color GetPixel(this Texture2D texture, int x, int y)
        {
            Color[] color = new Color[1];
            Rectangle rec = new Rectangle(x - 1, y - 1, 1, 1);
            texture.GetData(0, rec, color, 0, 1);
            return color[0];
        }
        public static void SetColor(this Texture2D texture, Color[] color)
        {
            texture.SetData(color);
        }
        /// <summary>
        /// 设置图片颜色
        /// </summary>
        /// <param name="texture">要设置颜色的图片</param>
        /// <param name="color">要设置颜色的区域的颜色</param>
        /// <param name="rec">要设置颜色的区域</param>
        public static void SetColor(this Texture2D texture, Color[] color, RECT rec)
        {
            texture.SetData(0, rec.GetRect(), color, 0, color.Length, SetDataOptions.None);
        }
		public static void SetColor(this Texture2D texture, byte[] buffer, RECT rect)
		{
			texture.SetData(0, rect.GetRect(), buffer, 0, buffer.Length, SetDataOptions.None);
		}
        /// <summary>
        /// 设置图片颜色
        /// </summary>
        /// <param name="texture">要设置颜色的图片</param>
        /// <param name="color">整张图片的颜色</param>
        /// <param name="source">图片上的像素区域</param>
        /// <param name="rec">要设置颜色的区域</param>
        public static void SetColor(this Texture2D texture, Color[] color, RECT source, RECT rec)
        {
            Color[] array = color.GetArray((int)source.X, (int)source.Y, (int)source.Width, (int)source.Height, texture.Width);
            texture.SetData(0, rec.GetRect(), array, 0, array.Length, SetDataOptions.None);
        }
        public static void SetPixel(this Texture2D texture, Color color, int x, int y)
        {
            Color[] data = { color };
            Rectangle rec = new Rectangle(x - 1, y - 1, 1, 1);
            texture.SetData(data);
        }
    }
}
