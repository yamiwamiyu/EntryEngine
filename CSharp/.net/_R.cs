using __System.Reflection;
using EntryBuilder.CodeAnalysis.Semantics;
using __System;
using System.Collections.Generic;

/// <summary>程序集反射相关内容</summary>
[ANonOptimize][AInvariant]static class _R
{
    internal static Dictionary<string, CSharpType> _t = new Dictionary<string, CSharpType>();
    internal static Dictionary<CSharpType, RuntimeType> _rt = new Dictionary<CSharpType, RuntimeType>();
    private static Dictionary<string, CSharpAssembly> _a = new Dictionary<string, CSharpAssembly>();
    private static Dictionary<CSharpAssembly, Assembly> _ra = new Dictionary<CSharpAssembly, Assembly>();
    internal static Dictionary<string, SimpleType> _st = new Dictionary<string, SimpleType>();
    /* 程序集代码又Rewriter生成
    switch (name)
    {
        case "int":
            TypeDefinitionInfo type = new TypeDefinitionInfo();
            type._Name = new Named("int");
            return type;
    }
    return null;*/
    [ANonOptimize][AInvariant]private extern static CSharpType BuildType(string name);
    [ANonOptimize][AInvariant]private static TypeDefinitionInfo CreateType(string name, string aname, TypeAttributes att, string underlyingType, string[] typeParameters, string[] baseTypes)
    {
        var type = new TypeDefinitionInfo();
        _t.Add(name, type);

        type._Name = new Named(name);
        type._Assembly = AllocA(aname);
        type._Assembly.Add(type);
        type._TypeAttributes = att;
        if (underlyingType != null)
            type._UnderlyingType = AllocType(underlyingType);
        if (typeParameters != null)
        {
            type._TypeParameters = new TypeParameterData[typeParameters.Length];
            for (int i = 0; i < typeParameters.Length; i++)
            {
                type._TypeParameters[0] = new TypeParameterData();
                type._TypeParameters[0].Name = new Named(typeParameters[i]);
            }
        }
        if (baseTypes != null)
        {
            for (int i = 0; i < baseTypes.Length; i++)
            {
                var bt = AllocType(baseTypes[i]);
                if (bt != null)
                    type._BaseTypes.Add(bt);
            }
        }

        return type;
    }
    [ANonOptimize][AInvariant]public static void CreateMember(TypeDefinitionInfo type, int index, string name, MemberAttributes att, MemberDefinitionKind kind, string returnType, int accessor, string[] atts)
    {
        MemberDefinitionInfo member = type.CreateMemberDefinition(index);
        member._Name = new Named(name);
        member._MemberAttributes = att;
        member._Kind = kind;
        if (returnType != null)
            member._ReturnType = AllocType(returnType);
        if (accessor > 0)
        {
            if (accessor == 1 || accessor == 3)
                member._Accessors.Add(new MemberAccessor(member, true));
            if (accessor == 2 || accessor == 3)
                member._Accessors.Add(new MemberAccessor(member, false));
        }
        if (atts != null)
        {
            for (int i = 0; i < atts.Length; i++)
                member._Attributes.Add(AllocType(atts[i]), null);
        }
    }
    [ANonOptimize][AInvariant]public static CSharpType AllocType(string name)
    {
        var type = ParseTypeName<CSharpType>(name,
            (typeName, assemblyName) =>
            {
                CSharpType result;
                if (!_t.TryGetValue(typeName, out result))
                {
                    result = BuildType(typeName);
                }
                return result;
            },
            (t1, tArray) =>
            {
                if (t1 == null) return t1;
                return CSharpType.CreateConstructedType(t1, t1.ContainingType, tArray);
            },
            (t) =>
            {
                if (t == null) return t;
                return CSharpType.CreateArray(1, t);
            });
        if (type != null && !_t.ContainsKey(name))
        {
            _t.Add(name, type);
        }
        return type;
    }
    [ANonOptimize][AInvariant]public static RuntimeType FromType(CSharpType key)
    {
        if (key == null)
        {
            throw new ArgumentNullException();
        }
        RuntimeType result;
        if (!_rt.TryGetValue(key, out result))
        {
            result = new RuntimeType(key);
            _rt.Add(key, result);
        }
        return result;
    }
    [ANonOptimize][AInvariant]public static CSharpAssembly AllocA(string key)
    {
        CSharpAssembly result;
        if (!_a.TryGetValue(key, out result))
        {
            result = new BinaryAssembly();
            result.Name = key;
            _a.Add(key, result);
        }
        return result;
    }
    [ANonOptimize][AInvariant]public static Assembly AllocAssembly(CSharpType key)
    {
        Assembly result;
        if (!_ra.TryGetValue(key.Assembly, out result))
        {
            result = new RuntimeAssembly(key.Assembly);
            _ra.Add(key.Assembly, result);
        }
        return result;
    }
    /*
     * 调用实现
     * 1. 穷举所有类型所有成员的调用
     * 优：各语言通用，不依赖语言支持反射的变式实现，执行速度快
     * 劣：生成代码多（引用优化和手动添加反射标签，应该也不会太多）
     */
    [ANonOptimize][AInvariant]public extern static object Invoke(CSharpMember member, object obj, object[] args);

    // EntryEngine.Serialize._SERIALIZE
    public static T ParseTypeName<T>(string name,
        Func<string, string, T> onParseType,
        Func<T, T[], T> onGeneric,
        Func<T, T> onArray)
    {
        int index = name.IndexOf('[', 0);
        if (index == -1)
        {
            string[] names = name.Split(',');
            if (names.Length > 1)
                return onParseType(names[0], names[1].Substring(1, names[1].Length - 1));
            else
                return onParseType(names[0], null);
        }
        else
        {
            string typeName = name.Substring(0, index);
            // 追加最后的程序集信息
            int lastIndex = name.LastIndexOf(']');
            lastIndex = name.IndexOf(',', lastIndex);
            string aname = null;
            if (lastIndex != -1)
                aname = name.Substring(lastIndex + 2);
            T type = onParseType(typeName, aname);
            int index2 = typeName.IndexOf('`', 0);
            if (index2 != -1)
            {
                int typeParameterCount = int.Parse(name.Substring(index2 + 1, index - index2 - 1));
                T[] typeArguments = new T[typeParameterCount];
                int gcount = 1;
                index += 2;
                index2 = index;
                int tai = 0;
                while (true)
                {
                    int end = name.IndexOf(']', index2);
                    int start = name.IndexOf('[', index2);
                    if (start != -1 && start < end)
                    {
                        // 有其它的泛型类型
                        gcount++;
                        index2 = start + 1;
                        continue;
                    }
                    if (gcount > 0)
                    {
                        // 其它泛型类型的结束
                        gcount--;
                        if (gcount > 0)
                        {
                            index2 = end + 1;
                            continue;
                        }
                    }
                    typeName = name.Substring(index, end - index);
                    typeArguments[tai++] = ParseTypeName(typeName, onParseType, onGeneric, onArray);
                    if (tai == typeArguments.Length)
                    {
                        // 跳过字符]]
                        index = end + 2;
                        break;
                    }
                    // 跳过字符：],[
                    index = end + 3;
                    index2 = index;
                    gcount = 1;
                }

                type = onGeneric(type, typeArguments);
            }
            // name[index] == '[' 用于处理 System.Int32[], mscorlib
            while (index < name.Length && name[index] == '[')
            {
                type = onArray(type);
                index += 2;
            }
            return type;
        }
    }

    /// <summary>根据指定类型创建实例</summary>
    [ASystemAPI]public extern static object CreateObject(Type type);
}
