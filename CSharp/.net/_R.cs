using __System.Reflection;
using EntryBuilder.CodeAnalysis.Semantics;
using __System;
using System.Collections.Generic;

[ANonOptimize][AInvariant]static class _R
{
    private static Dictionary<string, CSharpType> _t = new Dictionary<string, CSharpType>();
    private static Dictionary<CSharpType, RuntimeType> _rt = new Dictionary<CSharpType, RuntimeType>();
    private static Dictionary<string, CSharpAssembly> _a = new Dictionary<string, CSharpAssembly>();
    private static Dictionary<CSharpAssembly, Assembly> _ra = new Dictionary<CSharpAssembly, Assembly>();
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
        // 缓存
        CSharpType result;
        if (_t.TryGetValue(name, out result))
            return result;
        int index = name.IndexOf('[', 0);
        if (index == -1)
        {
            index = name.IndexOf(',', 0);
            if (index != -1)
                // 去除程序集信息，因为JS代码全在一个程序集中
                name = name.Substring(0, index);
            return BuildType(name);
        }
        else
        {
            string typeName = name.Substring(0, index);
            CSharpType gTypeDefinition = AllocType(typeName);
            int index2 = typeName.IndexOf('`', 0);
            if (index2 != -1)
            {
                int typeParameterCount = int.Parse(name.Substring(index2 + 1, index - index2 - 1));
                CSharpType[] typeArguments = new CSharpType[typeParameterCount];
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
                    typeArguments[tai++] = AllocType(typeName);
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
                gTypeDefinition = CSharpType.CreateConstructedType(gTypeDefinition, gTypeDefinition.ContainingType, typeArguments);
            }
            // name[index] == '[' 用于处理 System.Int32[], mscorlib
            while (index < name.Length && name[index] == '[')
            {
                gTypeDefinition = CSharpType.CreateArray(1, gTypeDefinition);
                index += 2;
            }
            // 缓存
            _t.Add(name, gTypeDefinition);
            return gTypeDefinition;
        }
    }
    [ANonOptimize][AInvariant]public static RuntimeType FromType(CSharpType key)
    {
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
}
