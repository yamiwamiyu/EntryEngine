using UnityEngine;
using EntryEngine;
using System.Collections.Generic;

namespace EntryEngine.Unity
{
    public static class _UNITY
    {
        // left hand coordinate -> zx 2d coordinate
        public static Vector3 ToZXY(this Vector3 vector)
        {
            return new Vector3(vector.z, vector.x, vector.y);
        }
        public static Vector3 ToZXY(this VECTOR2 vector)
        {
            return new Vector3(vector.Y, 0, vector.X);
        }
        public static VECTOR2 ToZX(this Vector3 vector)
        {
            return new VECTOR2(vector.z, vector.x);
        }
        public static VECTOR2 ToCartesian(this VECTOR2 vector, float height)
        {
            return new VECTOR2(vector.X, height - vector.Y);
        }
        public static VECTOR2 ToCartesian(this VECTOR2 vector)
        {
            return new VECTOR2(vector.X, Screen.height - vector.Y);
        }
        public static Vector2 ToCartesian(this Vector2 vector, float height)
        {
            return new Vector2(vector.x, height - vector.y);
        }
        public static Rect ToCartesian(this RECT rect, float screenHeight)
        {
            return new Rect(rect.X, screenHeight - rect.Y - rect.Height, rect.Width, rect.Height);
        }
        public static Rect ToCartesian(this RECT rect)
        {
            return new Rect(rect.X, Screen.height - rect.Y - rect.Height, rect.Width, rect.Height);
        }
        public static Rect GetFrameSource(this TEXTURE frame, RECT value)
        {
            return new Rect(value.X / frame.Width,
                1 - value.Height / frame.Height - value.Y / frame.Height,
                value.Width / frame.Width,
                value.Height / frame.Height);
            //return new Rect(value.X * _MATH.DIVIDE_BY_1[frame.Width],
            //    1 - value.Height * _MATH.DIVIDE_BY_1[frame.Height] - value.Y *_MATH.DIVIDE_BY_1[frame.Height],
            //    value.Width * _MATH.DIVIDE_BY_1[frame.Width],
            //    value.Height * _MATH.DIVIDE_BY_1[frame.Height]);
        }
        public static Rect GetFrameSource(this RECT rect, TEXTURE frame)
        {
            return GetFrameSource(frame, rect);
        }
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
        public static Rect GetRect(this RECT rect)
        {
            return new Rect(rect.X, rect.Y, rect.Width, rect.Height);
        }
        public static Color32 GetColor32(this COLOR color)
        {
            return new Color32(color.R, color.G, color.B, color.A);
        }
        public static Color GetColor(this COLOR color)
        {
            Color result;
            result.r = color.R * COLOR.BYTE_TO_FLOAT;
            result.g = color.G * COLOR.BYTE_TO_FLOAT;
            result.b = color.B * COLOR.BYTE_TO_FLOAT;
            result.a = color.A * COLOR.BYTE_TO_FLOAT;
            return result;
        }
        public static Matrix4x4 GetMatrix(this MATRIX matrix)
        {
            Matrix4x4 m = new Matrix4x4();
            m.m00 = matrix.M11;
            m.m01 = matrix.M21;
            m.m02 = matrix.M31;
            m.m03 = matrix.M41;
            m.m10 = matrix.M12;
            m.m11 = matrix.M22;
            m.m12 = matrix.M32;
            m.m13 = matrix.M42;
            m.m20 = matrix.M13;
            m.m21 = matrix.M23;
            m.m22 = matrix.M33;
            m.m23 = matrix.M43;
            m.m30 = matrix.M14;
            m.m31 = matrix.M24;
            m.m32 = matrix.M34;
            m.m33 = matrix.M44;
            return m;
        }
        public static Matrix4x4 GetMatrix(this MATRIX2x3 matrix)
        {
            Matrix4x4 result;
            result.m00 = matrix.M11;
            result.m01 = matrix.M21;
            result.m02 = 0;
            result.m03 = matrix.M31;
            result.m10 = matrix.M12;
            result.m11 = matrix.M22;
            result.m12 = 0;
            result.m13 = matrix.M32;
            result.m20 = 0;
            result.m21 = 0;
            result.m22 = 1;
            result.m23 = 0;
            result.m30 = 0;
            result.m31 = 0;
            result.m32 = 0;
            result.m33 = 1;
            return result;
        }
        public static VECTOR2 GetVector2(this Vector2 vector)
        {
            return new VECTOR2(vector.x, vector.y);
        }
        public static VECTOR2 GetVector2(this Vector3 vector)
        {
            return new VECTOR2(vector.x, vector.y);
        }
        public static VECTOR3 GetVector3(this Vector3 vector)
        {
            return new VECTOR3(vector.x, vector.y, vector.z);
        }
        public static VECTOR4 GetVector4(this Vector4 vector)
        {
            return new VECTOR4(vector.x, vector.y, vector.z, vector.w);
        }
        public static RECT GetRect(this Rect rect)
        {
            return new RECT(rect.x, rect.y, rect.width, rect.height);
        }
        public static COLOR GetColor(this Color color)
        {
            return new COLOR(color.r, color.g, color.b, color.a);
        }
        /// <summary>左下角坐标转换左上角坐标</summary>
        public static COLOR[] GetColor(this Color[] buffer, int width)
        {
            int count = buffer.Length;
            COLOR[] colors = new COLOR[count];
            int start = count - width;
            for (int i = 0; i < count; i++)
            {
                colors[start].R = (byte)(buffer[i].r * byte.MaxValue);
                colors[start].G = (byte)(buffer[i].g * byte.MaxValue);
                colors[start].B = (byte)(buffer[i].b * byte.MaxValue);
                colors[start].A = (byte)(buffer[i].a * byte.MaxValue);
                start++;
                if (start % width == 0)
                    start = start - 2 * width;
            }
            return colors;
        }
        /// <summary>左上角坐标转换左下角坐标</summary>
        public static Color32[] GetColor(this COLOR[] buffer, int width)
        {
            int count = buffer.Length;
            Color32[] colors = new Color32[count];
            int start = count - width;
            for (int i = 0; i < count; i++)
            {
                colors[start].r = buffer[i].R;
                colors[start].g = buffer[i].G;
                colors[start].b = buffer[i].B;
                colors[start].a = buffer[i].A;
                start++;
                if (start % width == 0)
                    start = start - 2 * width;
            }
            return colors;
        }
        public static COLOR GetColor(this Color32 color)
        {
            return new COLOR(color.r, color.g, color.b, color.a);
        }
        public static MATRIX GetMatrix(this Matrix4x4 matrix)
        {
            MATRIX m = new MATRIX();
            m.M11 = matrix.m00;
            m.M21 = matrix.m01;
            m.M31 = matrix.m02;
            m.M41 = matrix.m03;
            m.M12 = matrix.m10;
            m.M22 = matrix.m11;
            m.M32 = matrix.m12;
            m.M42 = matrix.m13;
            m.M13 = matrix.m20;
            m.M23 = matrix.m21;
            m.M33 = matrix.m22;
            m.M43 = matrix.m23;
            m.M14 = matrix.m30;
            m.M24 = matrix.m31;
            m.M34 = matrix.m32;
            m.M44 = matrix.m33;
            return m;
        }
        public static MATRIX2x3 GetMatrix2x3(this Matrix4x4 matrix)
        {
            MATRIX2x3 result;
            result.M11 = matrix.m00;
            result.M21 = matrix.m01;
            result.M31 = matrix.m03;
            result.M12 = matrix.m10;
            result.M22 = matrix.m11;
            result.M32 = matrix.m13;
            return result;
        }
        public static Texture GetTexture(this TEXTURE texture)
        {
            if (texture == null)
                //throw new System.ArgumentNullException("texture");
                return null;
            TextureUnity t = texture as TextureUnity;
            if (t == null)
            {
                TEXTURE_Link tl = texture as TEXTURE_Link;
                if (tl == null)
                    //throw new System.ArgumentException("Not supported texture type.");
                    return null;
                return GetTexture(tl.Base);
            }
            else
                return t.Texture;
        }
        public static Material GetMaterial(this SHADER shader)
        {
            ShaderUnity material = shader as ShaderUnity;
            if (material == null)
                return null;
            else
                return material.Material;
        }
        public static Vector3 MoveStayY(this Camera camera, Vector2 moved)
        {
            var dx = camera.transform.up;
            if (dx.y != 0)
            {
                moved.y /= dx.y;
                dx.y = 0;
            }
            return dx * moved.y + camera.transform.right * -moved.x;
        }
        public static Vector3 OrthoRayToPlaneXZ(this Camera camera, Vector3 screenPoint)
        {
            var ray = camera.ScreenPointToRay(screenPoint);
            var point = ray.GetPoint(ray.origin.y / Mathf.Cos(Vector3.Angle(ray.direction, Vector3.down) * Mathf.Deg2Rad));
            return point;
        }
        public static bool IsClick(this KeyboardUnity keyboard, KeyCode key)
        {
            return keyboard.IsClick((int)key);
        }
        public static bool IsRelease(this KeyboardUnity keyboard, KeyCode key)
        {
            return keyboard.IsRelease((int)key);
        }
        public static bool IsPressed(this KeyboardUnity keyboard, KeyCode key)
        {
            return keyboard.IsPressed((int)key);
        }
        public static void RemoveComponent<T>(this GameObject obj)
        {
            Remove(obj.GetComponent(typeof(T)));
        }
        public static void Remove(this Component component)
        {
            GameObject.Destroy(component);
        }
        public static void Unload(this Object obj)
        {
            Resources.UnloadAsset(obj);
        }
        public static void Destroy(this Object obj)
        {
            Object.Destroy(obj);
        }
        public static void Dispose(this Object obj)
        {
            obj.Destroy();
            obj.Unload();
        }
#if DEBUG
        public static T ToEnum<T>(this int value)
        {
            return (T)System.Enum.Parse(typeof(T), value.ToString());
        }
#endif
    }
}
