using EntryBuilder.CodeAnalysis.Semantics;
using __System.Collections.Generic;

namespace __System.Reflection
{
    public enum BindingFlags
    {
        Default = 0,
        Instance = 4,
        Static = 8,
        Public = 16,
        NonPublic = 32,
    }
    public abstract class Assembly
    {
        public abstract string FullName { get; }
        public abstract AssemblyName GetName();
    }
    public sealed class AssemblyName
    {
        public string Name { get; set; }
        public AssemblyName() { }
        public AssemblyName(string name) { this.Name = name; }
    }
    public abstract class MemberInfo
    {
        public static readonly object[] _Empty = new object[0];

        public abstract Type DeclaringType { get; }
        public abstract string Name { get; }
        public abstract object[] GetCustomAttributes(bool inherit);
        public abstract object[] GetCustomAttributes(Type attributeType, bool inherit);
    }
    public abstract class FieldInfo : MemberInfo
    {
        public abstract Type FieldType { get; }
        public abstract bool IsAssembly { get; }
        public abstract bool IsFamily { get; }
        public abstract bool IsFamilyOrAssembly { get; }
        public abstract bool IsInitOnly { get; }
        public abstract bool IsLiteral { get; }
        public virtual bool IsNotSerialized
        {
            get
            {
#if !DEBUG
                return GetCustomAttributes(typeof(NonSerialized), true).Length > 0;
#else
                throw new NotImplementedException();
#endif
            }
        }
        public abstract bool IsPrivate { get; }
        public abstract bool IsPublic { get; }
        public abstract bool IsSpecialName { get; }
        public abstract bool IsStatic { get; }

        public abstract object GetRawConstantValue();
        public abstract object GetValue(object obj);
        public abstract void SetValue(object obj, object value);
    }
    public abstract class PropertyInfo : MemberInfo
    {
        public abstract bool CanRead { get; }
        public abstract bool CanWrite { get; }
        public abstract Type PropertyType { get; }

        public MethodInfo[] GetAccessors()
        {
            return GetAccessors(false);
        }
        public abstract MethodInfo[] GetAccessors(bool nonPublic);
        public MethodInfo GetGetMethod()
        {
            return GetGetMethod(false);
        }
        public abstract MethodInfo GetGetMethod(bool nonPublic);
        public abstract ParameterInfo[] GetIndexParameters();
        public MethodInfo GetSetMethod()
        {
            return GetSetMethod(false);
        }
        public abstract MethodInfo GetSetMethod(bool nonPublic);
        public abstract object GetValue(object obj, object[] index);
        public abstract void SetValue(object obj, object value, object[] index);
    }
    public abstract class MethodBase : MemberInfo
    {
        public abstract bool IsAbstract { get; }
        public abstract bool IsAssembly { get; }
        public abstract bool IsConstructor { get; }
        public abstract bool IsFamily { get; }
        public abstract bool IsFamilyOrAssembly { get; }
        public abstract bool IsFinal { get; }
        public virtual bool IsGenericMethod { get { return false; } }
        public virtual bool IsGenericMethodDefinition { get { return false; } }
        public abstract bool IsPrivate { get; }
        public abstract bool IsPublic { get; }
        public abstract bool IsSpecialName { get; }
        public abstract bool IsStatic { get; }
        public abstract bool IsVirtual { get; }

        public virtual Type[] GetGenericArguments() { return Type.EmptyTypes; }
        public abstract ParameterInfo[] GetParameters();
        public abstract object Invoke(object obj, object[] parameters);
    }
    public abstract class ParameterInfo
    {
        public static readonly ParameterInfo[] _Empty = new ParameterInfo[0];
        public abstract bool IsIn { get; }
        public abstract bool IsOut { get; }
        public abstract string Name { get; }
        public abstract Type ParameterType { get; }
        public abstract int Position { get; }
        public abstract object[] GetCustomAttributes(bool inherit);
        public abstract object[] GetCustomAttributes(Type attributeType, bool inherit);
    }
    public abstract class ConstructorInfo : MethodBase
    {
        public abstract object Invoke(object[] parameters);
    }
    public abstract class MethodInfo : MethodBase
    {
        public abstract bool ContainsGenericParameters { get; }
        public abstract Type ReturnType { get; }

        public abstract MethodInfo GetBaseDefinition();
        public abstract MethodInfo GetGenericMethodDefinition();
        public abstract MethodInfo MakeGenericMethod(params Type[] typeArguments);
    }

    [AInvariant]internal class RuntimeAssembly : Assembly
    {
        internal CSharpAssembly assembly;
        public override string FullName
        {
            get { return assembly.Name; }
        }
        [AInvariant]internal RuntimeAssembly(CSharpAssembly assembly)
        {
            this.assembly = assembly;
        }
        public override AssemblyName GetName()
        {
            return new AssemblyName(assembly.Name);
        }
        public override int GetHashCode()
        {
            return assembly.GetHashCode();
        }
    }
    internal class RuntimeFieldInfo : FieldInfo
    {
        static object[] _setParameter = new object[1];
        internal CSharpMember member;

        public override string Name { get { return member.Name.Name; } }
        public override Type DeclaringType { get { return _R.FromType(member.ContainingType); } }
        public override Type FieldType { get { return _R.FromType(member.ReturnType); } }
        public override bool IsAssembly { get { return member.IsInternal; } }
        public override bool IsFamily { get { return member.IsProtected; } }
        public override bool IsFamilyOrAssembly { get { return member.IsProtectedOrInternal; } }
        public override bool IsInitOnly { get { return member.IsReadonly; } }
        public override bool IsLiteral { get { return member.IsConstant; } }
        public override bool IsPrivate { get { return member.IsPrivate; } }
        public override bool IsPublic { get { return member.IsPublic; } }
        //todo:实现
        public override bool IsSpecialName { get { return false; } }
        public override bool IsStatic { get { return member.IsStatic; } }

        internal RuntimeFieldInfo(CSharpMember member)
        {
            this.member = member;
        }

        public override object GetRawConstantValue()
        {
            if (!IsLiteral)
                return null;
            return member.ConstantValue;
        }
        public override object GetValue(object obj)
        {
            return _R.Invoke(member, obj, _Empty);
        }
        public override void SetValue(object obj, object value)
        {
            _setParameter[0] = value;
            _R.Invoke(member, obj, _setParameter);
        }
        public override object[] GetCustomAttributes(bool inherit)
        {
            List<object> list = new List<object>();
            foreach (var item in member.Attributes)
                if (inherit || item.DefiningObject == member)
                    list.Add(item);
            return list.ToArray();
        }
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            CSharpType attType = ((RuntimeType)attributeType).type;
            List<object> list = new List<object>();
            foreach (var item in member.Attributes)
                if (item.Type == attType && (inherit || item.DefiningObject == member))
                    list.Add(item);
            return list.ToArray();
        }
    }
    internal class RuntimePropertyInfo : PropertyInfo
    {
        internal CSharpMember member;

        public override string Name
        {
            get { return member.Name.Name; }
        }
        public override Type DeclaringType { get { return _R.FromType(member.ContainingType); } }
        public override bool CanRead
        {
            get
            {
                var accessors = member.Accessors;
                for (int i = 0; i < accessors.Count; i++)
                    if (accessors[i].IsAddAccessor || accessors[i].IsGetAccessor)
                        return true;
                return true;
            }
        }
        public override bool CanWrite
        {
            get
            {
                var accessors = member.Accessors;
                for (int i = 0; i < accessors.Count; i++)
                    if (accessors[i].IsRemoveAccessor || accessors[i].IsSetAccessor)
                        return true;
                return true;
            }
        }
        public override Type PropertyType { get { return _R.FromType(member.ReturnType); } }

        internal RuntimePropertyInfo(CSharpMember member) { this.member = member; }

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            var accessors = member.Accessors;
            int count = 0;
            for (int i = 0; i < accessors.Count; i++)
                if (accessors[i].IsPublic != nonPublic)
                    count++;
            RuntimeMethodInfo[] methods = new RuntimeMethodInfo[count];
            for (int i = 0; i < accessors.Count; i++)
                if (accessors[i].IsPublic != nonPublic)
                    methods[i] = new RuntimeMethodInfo(accessors[i]);
            return methods;
        }
        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            var accessors = member.Accessors;
            for (int i = 0; i < accessors.Count; i++)
                if ((accessors[i].IsGetAccessor || accessors[i].IsAddAccessor) &&
                    accessors[i].IsPublic != nonPublic)
                    return new RuntimeMethodInfo(accessors[i]);
            return null;
        }
        public override ParameterInfo[] GetIndexParameters()
        {
            var ps = member.Parameters;
            if (ps.Count == 0)
                return ParameterInfo._Empty;
            ParameterInfo[] parameters = new ParameterInfo[ps.Count];
            for (int i = 0; i < parameters.Length; i++)
                parameters[i] = new RuntimeParameterInfo(ps[i]);
            return parameters;
        }
        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            var accessors = member.Accessors;
            for (int i = 0; i < accessors.Count; i++)
                if ((accessors[i].IsSetAccessor || accessors[i].IsRemoveAccessor) &&
                    accessors[i].IsPublic != nonPublic)
                    return new RuntimeMethodInfo(accessors[i]);
            return null;
        }
        public override object GetValue(object obj, object[] index)
        {
            return _R.Invoke(member, obj, index);
        }
        public override void SetValue(object obj, object value, object[] index)
        {
            int len = index == null ? 0 : index.Length;
            object[] args = new object[len + 1];
            args[0] = value;
            for (int i = 0; i < len; i++)
                args[i + 1] = index[i];
            _R.Invoke(member, obj, args);
        }
        public override object[] GetCustomAttributes(bool inherit)
        {
            List<object> list = new List<object>();
            foreach (var item in member.Attributes)
                if (inherit || item.DefiningObject == member)
                    list.Add(item);
            return list.ToArray();
        }
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            CSharpType attType = ((RuntimeType)attributeType).type;
            List<object> list = new List<object>();
            foreach (var item in member.Attributes)
                if (item.Type == attType && (inherit || item.DefiningObject == member))
                    list.Add(item);
            return list.ToArray();
        }
    }
    internal class RuntimeParameterInfo : ParameterInfo
    {
        internal CSharpParameter parameter;

        public override bool IsIn { get { return parameter.IsRef; } }
        public override bool IsOut { get { return parameter.IsOut; } }
        public override string Name { get { return parameter.Name.Name; } }
        public override Type ParameterType { get { return _R.FromType(parameter.Type); } }
        public override int Position { get { return parameter.Position; } }

        internal RuntimeParameterInfo(CSharpParameter parameter) { this.parameter = parameter; }

        //todo:实现
        public override object[] GetCustomAttributes(bool inherit) { return null; }
        public override object[] GetCustomAttributes(Type attributeType, bool inherit) { return null; }
    }
    internal class RuntimeConstructorInfo : ConstructorInfo
    {
        internal CSharpMember member;

        public override string Name { get { return member.Name.Name; } }
        public override Type DeclaringType { get { return _R.FromType(member.ContainingType); } }
        public override bool IsAbstract { get { return member.IsAbstract; } }
        public override bool IsAssembly { get { return member.IsInternal; } }
        public override bool IsConstructor { get { return true; } }
        public override bool IsFamily { get { return member.IsProtected; } }
        public override bool IsFamilyOrAssembly { get { return member.IsProtectedOrInternal; } }
        public override bool IsFinal { get { return member.IsSealed; } }
        public override bool IsPrivate { get { return member.IsPrivate; } }
        public override bool IsPublic { get { return member.IsPublic; } }
        public override bool IsSpecialName { get { return member.DefiningMember != null && member.DefiningMember.IsProperty; } }
        public override bool IsStatic { get { return member.IsStatic; } }
        public override bool IsVirtual { get { return member.IsVirtual; } }

        internal RuntimeConstructorInfo(CSharpMember member) { this.member = member; }

        public override ParameterInfo[] GetParameters()
        {
            var ps = member.Parameters;
            if (ps.Count == 0)
                return ParameterInfo._Empty;
            ParameterInfo[] parameters = new ParameterInfo[ps.Count];
            for (int i = 0; i < parameters.Length; i++)
                parameters[i] = new RuntimeParameterInfo(ps[i]);
            return parameters;
        }
        public override object Invoke(object[] parameters)
        {
            return _R.Invoke(member, null, parameters);
        }
        public override object Invoke(object obj, object[] parameters)
        {
            return _R.Invoke(member, obj, parameters);
        }
        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotImplementedException();
        }
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }
    }
    internal class RuntimeMethodInfo : MethodInfo
    {
        internal CSharpMember member;

        public override string Name { get { return member.Name.Name; } }
        public override Type DeclaringType { get { return _R.FromType(member.ContainingType); } }
        public override bool IsAbstract { get { return member.IsAbstract; } }
        public override bool IsAssembly { get { return member.IsInternal; } }
        public override bool IsConstructor { get { return false; } }
        public override bool IsFamily { get { return member.IsProtected; } }
        public override bool IsFamilyOrAssembly { get { return member.IsProtectedOrInternal; } }
        public override bool IsFinal { get { return member.IsSealed; } }
        public override bool IsGenericMethod { get { return member.TypeArguments.Count > 0; } }
        public override bool IsGenericMethodDefinition { get { return member.TypeParameters.Count > 0; } }
        public override bool IsPrivate { get { return member.IsPrivate; } }
        public override bool IsPublic { get { return member.IsPublic; } }
        public override bool IsSpecialName { get { return member.DefiningMember != null && member.DefiningMember.IsProperty; } }
        public override bool IsStatic { get { return member.IsStatic; } }
        public override bool IsVirtual { get { return member.IsVirtual; } }
        public override Type ReturnType { get { return _R.FromType(member.ReturnType); } }
        public override bool ContainsGenericParameters { get { return member.TypeParameters.Count > 0; } }

        internal RuntimeMethodInfo(CSharpMember member) { this.member = member; }

        public override ParameterInfo[] GetParameters()
        {
            var ps = member.Parameters;
            if (ps.Count == 0)
                return ParameterInfo._Empty;
            ParameterInfo[] parameters = new ParameterInfo[ps.Count];
            for (int i = 0; i < parameters.Length; i++)
                parameters[i] = new RuntimeParameterInfo(ps[i]);
            return parameters;
        }
        public override object Invoke(object obj, object[] parameters)
        {
            return _R.Invoke(member, obj, parameters);
        }
        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotImplementedException();
        }
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }
        public override MethodInfo GetBaseDefinition()
        {
            if (!member.IsOverride)
                return null;
            var _base = member.ContainingType.BaseClass;
            if (_base == null)
                return null;
            for (int i = 0; i < _base.Members.Count; i++)
            {
                var m = _base.Members[i];
                // todo: 用同样的名字实例来确定是同一个方法
                if (m.Name == member.Name)
                    return new RuntimeMethodInfo(m);
            }
            return null;
        }
        public override Type[] GetGenericArguments()
        {
            var args = member.TypeArguments;
            if (args.Count == 0)
                return Type.EmptyTypes;
            Type[] types = new Type[args.Count];
            for (int i = 0; i < types.Length; i++)
                types[i] = _R.FromType(args[i]);
            return types;
        }
        public override MethodInfo GetGenericMethodDefinition()
        {
            if (member.DefiningMember == null)
                return null;
            return new RuntimeMethodInfo(member.DefiningMember);
        }
        public override MethodInfo MakeGenericMethod(params Type[] typeArguments)
        {
            var typeParameters = member.TypeParameters;
            if (typeParameters.Count != typeArguments.Length || typeParameters.Count == 0)
                return null;
            CSharpType[] types = new CSharpType[typeArguments.Length];
            for (int i = 0; i < types.Length; i++)
                types[i] = ((RuntimeType)typeArguments[i]).type;
            return new RuntimeMethodInfo(new MemberWithTypeArguments(member, types));
        }
    }
}
