//Released under the MIT License.
//
//Copyright (c) 2015 Ntreev Soft co., Ltd.
//
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
//documentation files (the "Software"), to deal in the Software without restriction, including without limitation the 
//rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit 
//persons to whom the Software is furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
//Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
//WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
//COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
//OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ntreev.Library.Psd
{
    public static class IPropertiesExtension
    {
        public static bool Contains(this IProperties props, string property, params string[] properties)
        {
            return props.Contains(GeneratePropertyName(property, properties));
        }

        public static T ToValue<T>(this IProperties props, string property, params string[] properties)
        {
            return (T)props[GeneratePropertyName(property, properties)];
        }

        public static Guid ToGuid(this IProperties props, string property, params string[] properties)
        {
            return new Guid(props.ToString(property, properties));
        }

        public static string ToString(this IProperties props, string property, params string[] properties)
        {
            return ToValue<string>(props, property, properties);
        }

        public static byte ToByte(this IProperties props, string property, params string[] properties)
        {
            return ToValue<byte>(props, property, properties);
        }

        public static int ToInt32(this IProperties props, string property, params string[] properties)
        {
            return ToValue<int>(props, property, properties);
        }

        public static float ToSingle(this IProperties props, string property, params string[] properties)
        {
            return ToValue<float>(props, property, properties);
        }

        public static double ToDouble(this IProperties props, string property, params string[] properties)
        {
            return ToValue<double>(props, property, properties);
        }

        public static bool ToBoolean(this IProperties props, string property, params string[] properties)
        {
            return ToValue<bool>(props, property, properties);
        }

        public static bool TryGetValue<T>(this IProperties props, ref T value, string property, params string[] properties)
        {
            string propertyName = GeneratePropertyName(property, properties);
            if (props.Contains(propertyName) == false)
                return false;
            value = props.ToValue<T>(propertyName);
            return true;
        }

        private static string GeneratePropertyName(string property, params string[] properties)
        {
            if (properties.Length == 0)
                return property;

            return property + "." + string.Join(".", properties);
        }


        /// <summary>通过Resource中获取属性值</summary>
        /// <param name="property">key.key.key...</param>
        public static T Value<T>(this IProperties p, string property)
        {
            object value = p[property];
            if (value == null) return default(T);
            if (value is T) return (T)value;
            return (T)Convert.ChangeType(value, typeof(T));
        }
        /// <summary>通过Resource中获取属性值</summary>
        /// <param name="properties">可能由于ps版本原因，同一个属性值保存在不同的属性中</param>
        public static T Value<T>(this IProperties p, params string[] properties)
        {
            object value = null;
            for (int i = 0; i < properties.Length; i++)
                if (p.Contains(properties[i]))
                {
                    value = p[properties[i]];
                    break;
                }
            if (value is T) return (T)value;
            return (T)Convert.ChangeType(value, typeof(T));
        }
        public static string PrintProperties(this IProperties properties)
        {
            StringBuilder builder = new StringBuilder();
            PrintProperties(builder, properties);
            return builder.ToString();
        }
        static void PrintProperties(StringBuilder builder, object value)
        {
            if (value is string)
                builder.AppendFormat("\"{0}\"", value);
            else if (value is IProperties)
            {
                var properties = (IProperties)value;
                bool flag = false;
                builder.Append("{");
                foreach (var item in properties)
                {
                    flag = true;
                    builder.AppendFormat("\"{0}\":", item.Key);
                    PrintProperties(builder, item.Value);
                    builder.Append(",");
                }
                if (flag)
                    builder = builder.Remove(builder.Length - 1, 1);
                builder.Append("}");
            }
            else if (value is System.Collections.IList)
            {
                builder.Append("[");
                var array = (System.Collections.IList)value;
                for (int i = 0, e = array.Count - 1; i <= e; i++)
                {
                    PrintProperties(builder, array[i]);
                    if (i != e)
                        builder.Append(",");
                }
                builder.Append("]");
            }
            else
                builder.Append(value);
        }
    }
}
