using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using EntryEngine.Serialize;
using EntryEngine;

namespace EntryEngine
{
    public static class _RH
    {
        public static readonly string[] Modifier =
        {
            "public",
            "protected internal",
            "internal",
            "protected",
            "private",
        };
        
        private static bool IsCodeType(string want, string name)
        {
            return name == want ||
                name == want + "[]" ||  // array
                name == want + "&" ||   // ref
                name == want + "*";		// pointer
        }
        public static string CodeName(this Type type)
        {
            return CodeName(type, true);
        }
        public static string CodeName(this Type type, bool full)
        {
            if (type.IsByRef)
                type = type.GetElementType();
            string name = type.Name;
            if (IsCodeType("Int16", name) || IsCodeType("UInt16", name))
                return name.Replace("Int16", "short").ToLower();
            else if (IsCodeType("Int32", name) || IsCodeType("UInt32", name))
                return name.Replace("Int32", "int").ToLower();
            else if (IsCodeType("Int64", name) || IsCodeType("UInt64", name))
                return name.Replace("Int64", "long").ToLower();
            else if (IsCodeType("Single", name))
                return name.Replace("Single", "float");
            else if (IsCodeType("Boolean", name))
                return name.Replace("Boolean", "bool");
            else if (IsCodeType("Char", name) ||
                IsCodeType("SByte", name) ||
                IsCodeType("Byte", name) ||
                IsCodeType("Object", name) ||
                IsCodeType("Double", name) ||
                IsCodeType("String", name) ||
                IsCodeType("Void", name))
                return name.ToLower();
            else if (type.IsArray)
                return type.GetElementType().CodeName() + "[]";
            else if (type.IsGenericType)
            {
                Type[] generic = type.GetGenericArguments();

                if (name.StartsWith("Nullable`1"))
                    return generic[0].CodeName(full) + "?";

                int glen = generic.Length;
                Stack<string> names = new Stack<string>();
                while (true)
                {
                    string tname = type.Name;
                    int index = tname.IndexOf('`');
                    if (index != -1)
                    {
                        int count = Utility.GetNumber(tname, index + 1);
                        tname = tname.Remove(index);
                        tname += "<";
                        for (int i = glen - count; i < glen; i++)
                        {
                            Type t = generic[i];
                            tname += t.CodeName(full);
                            if (i != glen - 1)
                                tname += ", ";
                        }
                        tname += ">";
                        glen -= count;
                    }
                    names.Push(tname);
                    if (type.DeclaringType == null)
                        break;
                    type = type.DeclaringType;
                }

                name = string.Join(".", names.ToArray());
                if (full && !string.IsNullOrEmpty(type.Namespace))
                    name = string.Format("{0}.{1}", type.Namespace, name);
            }
            else
            {
                // nested type
                if (type.IsNested)
                {
                    name = type.FullName;
                    // <T>
                    if (name == null)
                        name = type.Name;
                    else
                    {
                        name = name.Replace('+', '.');
                        if (!full && !string.IsNullOrEmpty(type.Namespace))
                            name = name.Substring(type.Namespace.Length + 1);
                    }
                }
                else
                    if (full)
                        name = type.FullName;
            }
            return name;
        }
        public static string CodeValue(object obj)
        {
            return CodeValue(obj, true);
        }
        /// <summary>
        /// 获得一个对象生成的代码表示方式
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="top">当对象为数组时，标识对象是否为顶级对象</param>
        /// <returns>对象的C#代码表达方式</returns>
        public static string CodeValue(object obj, bool top)
        {
            return CodeValue(obj, top, null);
        }
        public static string CodeValue(object obj, bool top, EntryEngine.Serialize.SerializeSetting? setting)
        {
            if (obj == null)
                return "null";
            else
            {
                Type type = obj.GetType();
                string value;
                if (type.IsEnum)
                {
                    Type utype = Enum.GetUnderlyingType(type);
                    //value = string.Format("{0}.{1}", type.Name, obj);
                    value = string.Format("({0})({1})", type.DeclaringType == null ? type.Name : type.FullName.Replace('+', '.'), Convert.ChangeType(obj, utype));
                }
                else if (type == typeof(string))
                    //value = string.Format("\"{0}\"", _SERIALIZE.StringToCode(obj.ToString()));
                    value = _SERIALIZE.StringToCode(obj.ToString());
                else if (type == typeof(bool))
                    value = obj.ToString().ToLower();
                else if (type == typeof(float))
                    value = obj.ToString() + "f";
                else if (type.IsArray)
                {
                    StringBuilder str = new StringBuilder();
                    str.AppendLine(string.Format("new {0}", type.Name));
                    str.AppendLine("{");
                    var array = (Array)obj;
                    for (int i = 0; i < array.Length; i++)
                    {
                        str.Append(CodeValue(array.GetValue(i), false));
                        str.Append(',');
                    }
                    str.AppendLine();
                    str.Append("}");
                    if (!top)
                        str.Append(',');
                    value = str.ToString();
                }
                else if (type.IsClass || (!type.IsInterface && !type.IsPrimitive))
                {
                    StringBuilder str = new StringBuilder();
                    str.AppendLine(string.Format("new {0}()", type.Name));
                    str.AppendLine("{");
                    if (setting.HasValue)
                    {
                        setting.Value.Serialize(type, obj,
                            v =>
                            {
                                str.AppendLine(string.Format("{0} = {1},", v.VariableName, CodeValue(v.GetValue(), false, setting)));
                            });
                    }
                    else
                    {
                        var fields = type.GetFields().Where(f => !f.IsStatic).ToArray();
                        foreach (var field in fields)
                        {
                            str.AppendLine(string.Format("{0} = {1},", field.Name, CodeValue(field.GetValue(obj), false)));
                        }
                        var properties = type.GetProperties().Where(p => p.GetIndexParameters().Length == 0 &&
                            p.CanWrite && p.CanRead).ToArray();
                        foreach (var property in properties)
                        {
                            str.AppendLine(string.Format("{0} = {1},", property.Name, CodeValue(property.GetValue(obj, null), false)));
                        }
                    }
                    str.Append("}");
                    value = str.ToString();
                }
                else
                    value = obj.ToString();
                return value;
            }
        }
        public static bool CanOverride(this PropertyInfo property)
        {
            if (property.CanRead)
                return property.GetGetMethod(true).CanOverride();
            else
                return property.GetSetMethod(true).CanOverride();
        }
        public static bool CanOverride(this MethodInfo method)
        {
            return !method.IsStatic && (method.IsAbstract || (method.IsVirtual && !method.IsFinal));
        }
        public static object DefaultValue(this Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
        public static IEnumerable<T> DeclareTypeOnly<T>(this IEnumerable<T> members, Type declareType) where T : MemberInfo
        {
            return members.Where(m => m.DeclaringType == declareType);
        }
        public static IEnumerable<T> DeclareTypeNotObject<T>(this IEnumerable<T> members) where T : MemberInfo
        {
            Type OBJECT = typeof(Object);
            return members.Where(m => m.DeclaringType != OBJECT);
        }
        public static T GetAttribute<T>(this ParameterInfo param) where T : Attribute
        {
            object[] attributes = param.GetCustomAttributes(typeof(T), true);
            if (attributes.Length == 0)
                return null;
            else
                return (T)attributes[0];
        }
        public static bool HasAttribute<T>(this ParameterInfo param) where T : Attribute
        {
            return param.GetAttribute<T>() != null;
        }
        public static T GetAttribute<T>(this MemberInfo type) where T : Attribute
        {
            return GetAttribute<T>(type, false);
        }
        public static T GetAttribute<T>(this MemberInfo type, bool inherit) where T : Attribute
        {
            object[] attributes = type.GetCustomAttributes(typeof(T), inherit);
            if (attributes.Length == 0)
                return null;
            else
                return (T)attributes[0];
        }
        public static T GetAttribute<T>(this Enum value) where T : Attribute
        {
            return value.GetType().GetField(value.ToString()).GetAttribute<T>();
        }
        public static bool HasAttribute<T>(this MemberInfo type) where T : Attribute
        {
            return type.GetAttribute<T>() != null;
        }
        public static bool HasAttribute<T>(this MemberInfo type, bool inherit) where T : Attribute
        {
            return type.GetAttribute<T>(inherit) != null;
        }
        public static bool HasReturnType(this MethodInfo method)
        {
            return method.ReturnType.FullName != "System.Void";
            //return method.ReturnType.TypeInitializer != null;
        }
        public static string GetModifier(this Type member)
        {
            if (member.IsNested)
            {
                if (member.IsNestedPublic)
                    return Modifier[0];
                else if (member.IsNestedFamORAssem)
                    return Modifier[1];
                else if (member.IsNestedAssembly)
                    return Modifier[2];
                else if (member.IsNestedFamily)
                    return Modifier[3];
                else if (member.IsNestedPrivate)
                    return Modifier[4];
                else
                    throw new NotImplementedException();
            }
            else if (member.IsPublic)
            {
                return Modifier[0];
            }
            else
            {
                return Modifier[2];
            }
        }
        public static string GetModifier(this FieldInfo member)
        {
            if (member.IsPublic)
                return Modifier[0];
            else if (member.IsFamilyOrAssembly)
                return Modifier[1];
            else if (member.IsAssembly)
                return Modifier[2];
            else if (member.IsFamily)
                return Modifier[3];
            else if (member.IsPrivate)
                return Modifier[4];
            else
                throw new NotImplementedException();
        }
        public static string GetModifier(this PropertyInfo member)
        {
            int index = Modifier.Length;
            if (member.CanRead)
            {
                string modifier = member.GetGetMethod(true).GetModifier();
                index = Modifier.IndexOf(modifier);
            }
            if (member.CanWrite)
            {
                string modifier = member.GetSetMethod(true).GetModifier();
                int temp = Modifier.IndexOf(modifier);
                if (temp < index)
                    index = temp;
            }
            return Modifier[index];
        }
        public static string GetModifier(this MethodBase member)
        {
            if (member.IsPublic)
                return Modifier[0];
            else if (member.IsFamilyOrAssembly)
                return Modifier[1];
            else if (member.IsAssembly)
                return Modifier[2];
            else if (member.IsFamily)
                return Modifier[3];
            else if (member.IsPrivate)
                return Modifier[4];
            else
                throw new NotImplementedException();
        }
        public static IEnumerable<FieldInfo> GetPrivateStaticFields(this Type type, bool hasPublic)
        {
            BindingFlags flag = BindingFlags.NonPublic | BindingFlags.Static;
            if (hasPublic)
                flag |= BindingFlags.Public;
            return type.GetFields(flag).Where(f => f.GetAttribute<System.Runtime.CompilerServices.CompilerGeneratedAttribute>() == null);
        }
        public static IEnumerable<PropertyInfo> WithoutIndex(this IEnumerable<PropertyInfo> properties)
        {
            return properties.Where(p => p.GetIndexParameters().Length == 0);
        }
        public static IEnumerable<PropertyInfo> GetAllProperties(this Type type)
        {
            return GetAllProperties(type, BindingFlags.Instance | BindingFlags.Public);
        }
        public static IEnumerable<PropertyInfo> GetAllProperties(this Type type, BindingFlags flag)
        {
            if (type.IsInterface)
                return type.GetInterfaceProperties(flag);
            else
                return type.GetProperties(flag);
        }
        public static IEnumerable<PropertyInfo> GetInterfaceProperties(this Type type)
        {
            return GetInterfaceProperties(type, BindingFlags.Instance | BindingFlags.Public);
        }
        public static IEnumerable<PropertyInfo> GetInterfaceProperties(this Type type, BindingFlags flag)
        {
            List<PropertyInfo> properties = new List<PropertyInfo>();
            Type[] interfaces = type.GetInterfaces();
            foreach (Type item in interfaces)
                properties.AddRange(item.GetInterfaceProperties(flag));
            properties.AddRange(type.GetProperties(flag));
            return properties.Distinct();
        }
        public static IEnumerable<MethodInfo> GetPropertyMethods(this IEnumerable<MethodInfo> methods)
        {
            return methods.Where(m => m.IsSpecialName);
        }
        public static IEnumerable<MethodInfo> GetInterfaceMethods(this Type type)
        {
            return GetInterfaceMethods(type, BindingFlags.Instance | BindingFlags.Public);
        }
        public static IEnumerable<MethodInfo> GetInterfaceMethods(this Type type, BindingFlags flag)
        {
            List<MethodInfo> methods = new List<MethodInfo>();
            Type[] interfaces = type.GetInterfaces();
            foreach (Type item in interfaces)
                methods.AddRange(item.GetInterfaceMethods(flag));
            methods.AddRange(type.GetMethods(flag));
            return methods.Distinct();
        }
        public static IEnumerable<MethodInfo> GetAllMethods(this Type type)
        {
            return GetAllMethods(type, BindingFlags.Instance | BindingFlags.Public);
        }
        public static IEnumerable<MethodInfo> GetAllMethods(this Type type, BindingFlags flag)
        {
            if (type.IsInterface)
                return type.GetInterfaceMethods(flag);
            else
                return type.GetMethods(flag);
        }
        public static IEnumerable<MethodInfo> MethodOnly(this IEnumerable<MethodInfo> methods)
        {
            return methods.Where(m => !m.IsSpecialName || m.Name.StartsWith("op_"));
        }
        public static IEnumerable<MethodInfo> MethodOfProperty(this IEnumerable<MethodInfo> methods)
        {
            return methods.Where(m => m.IsSpecialName && !m.Name.StartsWith("op_"));
        }
        public static IEnumerable<Type> GetInnerTypes(this Type type)
        {
            return type.GetMembers().Where(m => m.MemberType == MemberTypes.NestedType).Select(m => (Type)m);
        }
        public static Type[] GetTypes(this Assembly assembly, Type baseType)
        {
            return assembly.GetTypes().Where(t => !t.IsAbstract && t.Is(baseType)).ToArray();
        }
        public static List<Type> GetTypes(this IEnumerable<Assembly> assemblies, Type baseType)
        {
            List<Type> types = new List<Type>();
            foreach (Assembly assembly in assemblies)
            {
                types.AddRange(GetTypes(assembly, baseType));
            }
            return types;
        }
        public static List<Type> GetTypes(this IEnumerable<Assembly> assemblies, Func<Type, bool> func)
        {
            List<Type> types = new List<Type>();
            foreach (Assembly assembly in assemblies)
            {
                types.AddRange(assembly.GetTypes().Where(func));
            }
            return types;
        }
        public static IEnumerable<Type> GetTypesWithAttribute<T>(this Assembly assembly) where T : Attribute
        {
            return assembly.GetTypes().Where(t => t.HasAttribute<T>());
        }
        public static IEnumerable<Type> GetTypesWithAttribute<T>(this Assembly assembly, bool inherit) where T : Attribute
        {
            return assembly.GetTypes().Where(t => t.HasAttribute<T>(inherit));
        }
        public static List<Type> GetTypesWithAttribute<T>(this IEnumerable<Assembly> assemblies) where T : Attribute
        {
            return GetTypes(assemblies, t => t.HasAttribute<T>());
        }
        public static List<Type> GetTypesWithAttribute<T>(this IEnumerable<Assembly> assemblies, bool inherit) where T : Attribute
        {
            return GetTypes(assemblies, t => t.HasAttribute<T>(inherit));
        }
        public static T[] GetEnum<T>()
        {
            Array array = Enum.GetValues(typeof(T));
            int count = array.Length;
            T[] result = new T[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = (T)array.GetValue(i);
            }
            return result;
        }
        public static object DefaultValue(this FieldInfo member)
        {
            object value = null;
            if (member.DeclaringType != null)
            {
                try
                {
                    var obj = Activator.CreateInstance(member.DeclaringType);
                    value = member.DeclaringType.GetField(member.Name).GetValue(obj);
                }
                catch { }
            }
            else
            {
                try
                {
                    value = Activator.CreateInstance(member.FieldType);
                }
                catch { }
            }
            return value;
        }
        public static object DefaultValue(this PropertyInfo member)
        {
            object value = null;
            if (member.DeclaringType != null)
            {
                try
                {
                    var obj = Activator.CreateInstance(member.DeclaringType);
                    value = member.DeclaringType.GetProperty(member.Name).GetValue(obj, null);
                }
                catch { }
            }
            else
            {
                try
                {
                    value = Activator.CreateInstance(member.PropertyType);
                }
                catch { }
            }
            return value;
        }
        public static string Indent(int indent)
        {
            //const string TAB = "    ";
            //string builder = "";
            //for (int i = 0; i < indent; i++)
            //{
            //    builder += TAB;
            //}
            //return builder;
            const char INDENT = ' ';
            int count = indent * 4;
            char[] space = new char[count];
            for (int i = 0; i < count; i++)
                space[i] = INDENT;
            return new string(space);
        }
        public static string Indent(string content)
        {
            string[] TAB_OPEN = { "{" };
            // { {
            string[] TAB_CLOSE = { "}", "},", "};" };
            string[] BREAKER = new string[1] { "\r\n" };

            string[] lines = content.Split(BREAKER, StringSplitOptions.None);
            lines = Indent(lines, TAB_OPEN, TAB_CLOSE);
            return string.Join(BREAKER[0], lines);
        }
        public static string[] Indent(string[] contents, string[] tabStart, string[] tabEnd)
        {
            int count = contents.Length;
            string[] result = new string[count];

            int indent = 0;
            for (int i = 0; i < count; i++)
            {
                try
                {
                    string line = contents[i].Trim();

                    foreach (string temp in tabEnd)
                    {
                        if (temp == line)
                        {
                            indent--;
                            break;
                        }
                    }

                    result[i] = Indent(indent) + line;

                    foreach (string temp in tabStart)
                    {
                        if (temp == line)
                        {
                            indent++;
                            break;
                        }
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }

            return result;
        }
    }
}
