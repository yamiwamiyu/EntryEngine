using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntryBuilder.CodeAnalysis.Semantics
{
    public enum EDirection
    {
        NONE,
        REF,
        OUT,
        PARAMS,
        THIS,
    }
    public enum EVariance
    {
        None,
        /// <summary>out</summary>
        Covariant,
        /// <summary>in</summary>
        Contravariant,
    }
    public class Named
    {
        static string[] _empty = new string[1];
        [AInvariant]public string Name;
        public Named() { }
        public Named(string name)
        {
            this.Name = name;
        }
        public string[] GetNames()
        {
            if (string.IsNullOrEmpty(Name))
            {
                _empty[0] = Name;
                return _empty;
            }
            return Name.Split('.');
        }
        public override string ToString()
        {
            return Name;
        }
    }

    /*
     * 引用查找需求
     * 1. 重载的运算符：operator +(T t1, T t2)转换为T __op1(T t2)的实例方法，a+b变为a.__op1(b)，进而将BinaryOperator变为InvokeMethod
     * 2. 类型转换：explicit方法在Cast时，(T)obj转换为T.__opc(obj)，Cast变为InvokeMethod
     * 3. 类型转换：implicit方法在赋值时，int a=obj转换为T.__opc(obj)，ReferenceMember变为InvokeMethod
     * 4. 索引器/属性：采用闭包形式，new Action(() => o.P = o.GetP(param); ... ; o.SetP(param, o.P);)();，Statement变为InvokeMethod
     * 5. 删除内容：删除没有被引用的内容，删除顺序为 成员 -> 类型 -> 成员参数
     * 
     * !. 需要可选生成动态dll信息，使其它语言平台可以使用反射
     * !. 通过dll可生成用于解析的CSharpAssembly，生成代码时不生成该dll的代码
     */
    public abstract class CSharpAssembly
    {
        public string Name;
        public List<CSharpType> Types = new List<CSharpType>();
        public IEnumerable<CSharpNamespace> Namespaces
        {
            get
            {
                HashSet<CSharpNamespace> result = new HashSet<CSharpNamespace>();
                foreach (var item in Types)
                {
                    if (item.ContainingNamespace != null)
                    {
                        CSharpNamespace temp = item.ContainingNamespace;
                        while (temp != null)
                        {
                            result.Add(temp);
                            temp = temp.ContainingNamespace;
                        }
                    }
                }
                return result;
            }
        }
        public virtual bool IsProject { get { return false; } }

        [ANonOptimize]internal void Add(TypeDefinitionInfo target)
        {
            if (target == null)
                throw new ArgumentNullException();
            if (target.Assembly != null && target.Assembly != this)
                throw new InvalidOperationException();
            if (Types.Contains(target))
                return;
            Types.Add(target);
            target._Assembly = this;
        }
        public override string ToString()
        {
            return Name;
        }
    }
    [ANonOptimize]public class BinaryAssembly : CSharpAssembly { }
    public class ProjectAssembly : CSharpAssembly
    {
        public override bool IsProject { get { return true; } }
    }
    public class CSharpNamespace
    {
        public string Name;
        public bool IsRoot
        {
            get { return ContainingNamespace == null; }
        }
        public CSharpNamespace ContainingNamespace;
        public List<CSharpNamespace> Namespaces = new List<CSharpNamespace>();
        public List<CSharpType> Types = new List<CSharpType>();

        internal CSharpNamespace()
        {
            Namespaces = new List<CSharpNamespace>();
            Types = new List<CSharpType>();
        }

        internal void AddNamespace(CSharpNamespace target)
        {
            if (target == null)
                throw new ArgumentNullException();
            if (Namespaces.Contains(target))
                return;
            if (target.ContainingNamespace != null && target.ContainingNamespace != this)
                target.Namespaces.Remove(target);
            Namespaces.Add(target);
            target.ContainingNamespace = this;
        }
        internal void AddType(TypeDefinitionInfo target)
        {
            if (target == null)
                throw new ArgumentNullException();
            if (Types.Contains(target))
                return;
            if (target.ContainingNamespace != null && target.ContainingNamespace != this)
                target.ContainingNamespace.Types.Remove(target);
            Types.Add(target);
            target._ContainingNamespace = this;
        }
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Name);

            CSharpNamespace temp = this.ContainingNamespace;
            while (temp != null)
            {
                // BUG: Insert(int, char)并没有，但是进行了自动类型转换调用
                builder.Insert(0, ".");
                //builder.Insert(0, '.');
                builder.Insert(0, temp.Name);
                temp = temp.ContainingNamespace;
            }

            return builder.ToString();
        }
    }

    [AInvariant]public abstract class CSharpType : IEquatable<CSharpType>
    {
        internal static readonly CSharpType[] EmptyList = new CSharpType[0];
        /// <summary>类型默认继承</summary>
        internal static TypeDefinitionInfo OBJECT;
        /// <summary>[]默认继承</summary>
        internal static TypeDefinitionInfo ARRAY;
        /// <summary>delegate默认继承(MulticastDelegate)</summary>
        internal static TypeDefinitionInfo DELEGATE;
        internal static TypeDefinitionInfo MULTICAST_DELEGATE;
        /// <summary>enum默认继承</summary>
        internal static TypeDefinitionInfo ENUM;
        /// <summary>struct默认继承</summary>
        internal static TypeDefinitionInfo VALUE_TYPE;
        /// <summary>数学类</summary>
        internal static TypeDefinitionInfo MATH;
        internal static TypeDefinitionInfo VOID;
        internal static TypeDefinitionInfo BOOL;
        internal static TypeDefinitionInfo BYTE;
        internal static TypeDefinitionInfo SBYTE;
        internal static TypeDefinitionInfo USHORT;
        internal static TypeDefinitionInfo SHORT;
        internal static TypeDefinitionInfo CHAR;
        internal static TypeDefinitionInfo UINT;
        internal static TypeDefinitionInfo INT;
        internal static TypeDefinitionInfo FLOAT;
        internal static TypeDefinitionInfo ULONG;
        internal static TypeDefinitionInfo LONG;
        internal static TypeDefinitionInfo DOUBLE;
        internal static TypeDefinitionInfo STRING;
        internal static TypeDefinitionInfo IENUMERABLE;
        internal static TypeDefinitionInfo IENUMERATOR;
        internal static TypeDefinitionInfo NULLABLE;
        internal static TypeDefinitionInfo ___Array;

        public virtual Named Name
        {
            get
            {
                return null;
            }
        }
        public string FullName
        {
            get
            {
                Stack<string> names = new Stack<string>();
                var type = this;
                while (type != null)
                {
                    names.Push(type.Name.Name);
                    type = type.ContainingType;
                }
                int depth = names.Count;
                var ns = this.ContainingNamespace;
                while (ns != null)
                {
                    if (!string.IsNullOrEmpty(ns.Name))
                        names.Push(ns.Name);
                    ns = ns.ContainingNamespace;
                }
                StringBuilder builder = new StringBuilder();
                while (names.Count > 0)
                {
                    builder.Append(names.Pop());
                    if (names.Count != 0)
                    {
                        if (names.Count < depth)
                            builder.Append('+');
                        else
                            builder.Append('.');
                    }
                }
                var generic = TypeParameters;
                if (generic.Count > 0)
                {
                    builder.Append('<');
                    for (int i = 0, n = generic.Count - 1; i <= n; i++)
                    {
                        builder.Append(generic[i].Name.Name);
                        if (i != n)
                            builder.Append(", ");
                    }
                    builder.Append('>');
                }
                return builder.ToString();
            }
        }
        public virtual bool IsClass
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsStruct
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsInterface
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsEnum
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsValueType
        {
            get
            {
                return this.IsStruct || this.IsEnum;
            }
        }
        public virtual bool IsReferenceType
        {
            get
            {
                return this.IsClass || this.IsInterface || this.IsDelegate || this.IsAnonymousType || this.IsArray;
            }
        }
        public virtual bool IsDelegate
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsSealed
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsAbstract
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsPublic
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsInternal
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsProtected
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsProtectedOrInternal
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsPrivate
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsArray
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsPointer
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsTypeParameter
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsUnknown
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsNested
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsConstructed
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsStatic
        {
            get
            {
                return this.IsAbstract && this.IsSealed;
            }
        }
        public virtual bool IsAnonymousType
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsDynamic
        {
            get
            {
                return false;
            }
        }
        /// <summary>泛型类型的定义类，例如List^1[[int]]，这个类型就是List^1</summary>
        public virtual CSharpType DefiningType { get { return null; } }
        /// <summary>泛型类型的成员类，例如List^1[[int]]的ToArray(返回int[])，这个类型就是List^1的ToArray(返回T[])</summary>
        public virtual CSharpMember DefiningMember { get { return null; } }
        /// <summary>类型所在的命名空间</summary>
        public virtual CSharpNamespace ContainingNamespace { get { return null; } }
        /// <summary>类型的外部类</summary>
        public virtual CSharpType ContainingType { get { return null; } }
        public virtual CSharpType UnderlyingType { get { return null; } }
        /// <summary>内部类</summary>
        public virtual IList<CSharpType> NestedTypes { get { return CSharpType.EmptyList; } }
        /// <summary>泛型形参</summary>
        public virtual IList<CSharpType> TypeParameters { get { return CSharpType.EmptyList; } }
        public virtual int TypeParametersCount { get { return TypeParameters.Count; } }
        /// <summary>继承的父类</summary>
        public virtual CSharpType BaseClass { get { return null; } }
        public virtual IList<CSharpType> BaseInterfaces { get { return CSharpType.EmptyList; } }
        public virtual CSharpMember DelegateInvokeMethod
        {
            get
            {
                if (this.IsDelegate)
                {
                    return this.Members.FirstOrDefault(m => m.Name.Name == "Invoke" && m.IsPublic);
                }
                return null;
            }
        }
        /// <summary>类型定义的成员，包含父类继承的成员</summary>
        public virtual IList<CSharpMember> Members { get { return CSharpMember.EmptyList; } }
        /// <summary>类型定义的成员，不包含父类继承的成员</summary>
        internal virtual IList<CSharpMember> MemberDefinitions { get { return CSharpMember.EmptyList; } }
        /// <summary>泛型实参</summary>
        public virtual IList<CSharpType> TypeArguments { get { return CSharpType.EmptyList; } }
        /// <summary>泛型参数的位置，例如Dictionary^2[[T],[U]]，T就是0，U就是1</summary>
        public virtual int TypeParameterPosition { get { return -1; } }
        /// <summary>where T : new()</summary>
        public virtual bool HasConstructorConstraint { get { return false; } }
        /// <summary>where T : class</summary>
        public virtual bool HasValueTypeConstraint { get { return false; } }
        /// <summary>where T : class</summary>
        public virtual bool HasReferenceTypeConstraint { get { return false; } }
        /// <summary>泛型类型的约束类型，定义泛型时例如where T : 约束类型</summary>
        public virtual IList<CSharpType> TypeConstraints { get { return CSharpType.EmptyList; } }
        /// <summary>协变</summary>
        public virtual bool IsCovariant { get { return false; } }
        /// <summary>逆变</summary>
        public virtual bool IsContravariant { get { return false; } }
        /// <summary>数组类型</summary>
        public virtual CSharpType ElementType { get { return null; } }
        /// <summary>数组维度</summary>
        public virtual int Rank { get { return -1; } }
        public virtual IList<CSharpAttribute0> Attributes { get { return CSharpAttribute0.EmptyList; } }
        [AInvariant]public abstract CSharpAssembly Assembly
        {
            get;
        }
        internal int Depth
        {
            get
            {
                int depth = 0;
                CSharpType type = this;
                while (type.ContainingType != null)
                {
                    depth++;
                    type = type.ContainingType;
                }
                return depth;
            }
        }

        [AInvariant]public bool Is(CSharpType other)
        {
            return WhtherIsType(other) != null;
        }
        public bool IsType(CSharpType other, out CSharpType result)
        {
            result = WhtherIsType(other);
            return result != null;
        }
        public CSharpType WhtherIsType(CSharpType other)
        {
            if (other == null)
                return null;
            if (this.Equals(other))
                return this;
            if (other.IsArray)
            {
                // 不考虑协变逆变的情况下，数组类型必须完全一致
                // 父类[] array = new 字类[]，允许赋值
                if (this.IsArray 
                    && (other.ElementType.IsTypeParameter || other.ElementType.IsClass)
                    && this.ElementType.Is(other.ElementType))
                    return this;
                return null;
            }
            // 不考虑协变逆变
            else if (other.IsConstructed)
            {
                if (other.DefiningType == NULLABLE && this.Is(other.TypeArguments[0]))
                    return this;
                // 继续往下走
            }
            else if (other.IsTypeParameter)
            {
                if (other.HasValueTypeConstraint && !this.IsValueType)
                    return null;

                if (other.HasConstructorConstraint)
                {
                    if (this.IsAbstract)
                        return null;
                    var members = this.Members;
                    // 是否有显示定义构造函数
                    bool hasConstructor = false;
                    foreach (var item in members)
                    {
                        if (item.IsConstructor && !item.IsStatic)
                        {
                            if (item.Parameters.Count == 0)
                            {
                                if (item.IsPublic)
                                    break;
                                return null;
                            }
                            hasConstructor = true;
                        }
                    }
                    if (hasConstructor && !this.IsStruct)
                        return null;
                }

                if (other.HasReferenceTypeConstraint && !(this.IsClass || this.IsInterface))
                    return null;

                var constraints = other.TypeConstraints;
                foreach (var item in constraints)
                    if (!this.Is(item))
                        return null;

                return this;
            }

            // 泛型类型比较
            CSharpType target = other;
            if (other.IsConstructed)
                target = other.DefiningType;
            if (this.IsConstructed && this.DefiningType.Equals(target))
            {
                if (!other.IsConstructed || CompareTypeArgumentsIs(other))
                    // 泛型类的情况，需要匹配泛型实参是否符合
                    return this;
                else
                    return null;
            }

            // 继承类型比较
            if (target.IsInterface)
            {
                foreach (var item in BaseInterfaces)
                    if (item.IsType(other, out target))
                        return target;
            }
            target = BaseClass;
            if (target != null && target.IsType(other, out target))
                return target;
            
            return null;
        }
        internal bool ImplementInterface(CSharpType ifaceType)
        {
            CSharpType type = this;
            while (type != null)
            {
                var interfaces = type.BaseInterfaces;
                if (interfaces != null)
                {
                    for (int i = 0; i < interfaces.Count; i++)
                    {
                        if (interfaces[i] == ifaceType || (interfaces[i] != null && interfaces[i].ImplementInterface(ifaceType)))
                        {
                            return true;
                        }
                    }
                }
                type = type.BaseClass;
            }
            return false;
        }
        public virtual bool IsSubclassOf(CSharpType c)
        {
            CSharpType type = this;
            if (type == c)
            {
                return false;
            }
            while (type != null)
            {
                if (type == c)
                {
                    return true;
                }
                type = type.BaseClass;
            }
            return false;
        }
        public virtual bool IsAssignableFrom(CSharpType c)
        {
            if (c == null)
            {
                return false;
            }
            if (this == c)
            {
                return true;
            }
            if (c.IsSubclassOf(this))
            {
                return true;
            }
            if (this.IsInterface)
            {
                return c.ImplementInterface(this);
            }
            if (this.IsTypeParameter)
            {
                var genericParameterConstraints = this.TypeConstraints;
                for (int i = 0; i < genericParameterConstraints.Count; i++)
                {
                    if (!genericParameterConstraints[i].IsAssignableFrom(c))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        internal bool CompareTypeArguments(CSharpType other)
        {
            IList<CSharpType> typeArguments = this.TypeArguments;
            IList<CSharpType> typeArguments2 = other.TypeArguments;
            if (typeArguments.Count != typeArguments2.Count)
            {
                return false;
            }
            for (int i = 0; i < typeArguments.Count; i++)
            {
                if (!typeArguments[i].Equals(typeArguments2[i]))
                {
                    return false;
                }
            }
            return true;
        }
        internal bool CompareTypeArgumentsIs(CSharpType other)
        {
            IList<CSharpType> typeArguments = this.TypeArguments;
            IList<CSharpType> typeArguments2 = other.TypeArguments;
            if (typeArguments.Count != typeArguments2.Count)
            {
                return false;
            }
            for (int i = 0; i < typeArguments.Count; i++)
            {
                if (typeArguments2[i].IsTypeParameter)
                {
                    if (!typeArguments[i].Is(typeArguments2[i]))
                    {
                        return false;
                    }
                }
                else
                {
                    if (this.DefiningType.TypeParameters[i].IsCovariant)
                    {
                        if (!typeArguments[i].Is(typeArguments2[i]))
                            return false;
                    }
                    else if (this.DefiningType.TypeParameters[i].IsContravariant)
                    {
                        if (!typeArguments2[i].Is(typeArguments[i]))
                            return false;
                    }
                    else if (!typeArguments[i].Equals(typeArguments2[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        internal int GetTypeArgumentsHashCode()
        {
            int num = 0;
            foreach (CSharpType current in this.TypeArguments)
            {
                num += current.GetHashCode();
            }
            return num;
        }
        internal void AppendTypeArguments(StringBuilder sb)
        {
            if (this.TypeArguments.Count > 0)
            {
                sb.Append("<");
                for (int i = 0; i < this.TypeArguments.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(",");
                    }
                    sb.Append(this.TypeArguments[i].ToString());
                }
                sb.Append(">");
            }
        }
        public abstract override string ToString();
        public virtual bool Equals(CSharpType other)
        {
            return other == this;
        }

        public static bool IsVoid(CSharpType type)
        {
            return type == null || type == VOID;
        }
        public static bool IsPrimitive(CSharpType type)
        {
            return type == CSharpType.BOOL ||
                type == CSharpType.SBYTE ||
                type == CSharpType.BYTE ||
                type == CSharpType.CHAR ||
                type == CSharpType.SHORT ||
                type == CSharpType.USHORT ||
                type == CSharpType.INT ||
                type == CSharpType.UINT ||
                type == CSharpType.FLOAT ||
                type == CSharpType.LONG ||
                type == CSharpType.ULONG ||
                type == CSharpType.DOUBLE ||
                type == CSharpType.STRING;
        }
        internal static CSharpType CreateArray(int rank, CSharpType elementType)
        {
            //return new ArrayTypeReference(elementType, rank);
            while (rank-- > 0)
                elementType = new ArrayTypeReference(elementType, 1);
            return elementType;
        }
        [AInvariant]internal static CSharpType CreateConstructedType(CSharpType definingType, IList<CSharpType> typeArguments)
        {
            return CreateConstructedType(definingType, definingType.ContainingType, typeArguments);
        }
        internal static CSharpType CreateConstructedType(CSharpType definingType, CSharpType containingType, IList<CSharpType> typeArguments)
        {
            if (containingType == definingType.ContainingType && typeArguments.Count == 0)
            {
                return definingType;
            }
            return new ConstructedTypeReference(definingType, containingType, typeArguments);
        }
        internal static CSharpType CreatePointer(CSharpType elementType)
        {
            return new PointerTypeReference(elementType);
        }
        internal static bool TypesEquals(IList<CSharpType> types1, IList<CSharpType> types2)
        {
            if (types1.Count < types2.Count)
            {
                return false;
            }
            for (int i = 0; i < types1.Count; i++)
            {
                if (!types1[i].Equals(types2[i]))
                {
                    return false;
                }
            }
            return true;
        }
        internal static int TypesGetHashCode(IList<CSharpType> types)
        {
            int num = 0;
            foreach (CSharpType current in types)
            {
                num += current.GetHashCode();
            }
            return num;
        }
        [AInvariant]public static string GetRuntimeTypeName(CSharpType type)
        {
            CSharpType tempType = type;
            type = GetDefinitionType(type);

            StringBuilder builder = new StringBuilder();

            // 命名空间
            CSharpNamespace ns = type.ContainingNamespace;
            if (ns != null)
            {
                string name = ns.ToString();
                if (!string.IsNullOrEmpty(name))
                {
                    builder.Append(name);
                    builder.Append(".");
                }
            }

            // 泛型形参的总数量
            int gcount = 0;
            int count = 0;
            CSharpType parent = type.ContainingType;
            for (; parent != null; count++)
                parent = parent.ContainingType;
            // 栈方式追加内部类的外层类
            // 例如TimeLine<int>.TimeKeyFrame
            // 结果是TimeLine^1+TimeKeyFrame[[int]]
            for (int i = count; i >= 0; i--)
            {
                parent = type;
                for (int j = 0; j < i; j++)
                    parent = parent.ContainingType;
                builder.Append(parent.Name.Name);
                while (parent.IsConstructed)
                    parent = parent.DefiningType;
                // 泛型形参数量
                int pcount = parent.TypeParametersCount;
                if (pcount > 0)
                {
                    builder.Append("^" + pcount);
                    gcount += pcount;
                }
                if (i != 0)
                    builder.Append("+");
            }
            // 泛型实参
            // class T1<T> { class T2<U> { } }
            // T1<int>.T2<int>的名字是T1^1+T2^1[[int],[int]]
            // 会将多个单个泛型的类型，汇总到最终的一个泛型类型数组中
            if (gcount > 0)
            {
                builder.Append("[");
                // 栈方式追加泛型实参类
                for (int i = count; i >= 0; i--)
                {
                    parent = type;
                    for (int j = 0; j < i; j++)
                        parent = parent.ContainingType;
                    if (!type.IsConstructed) continue;
                    var typeArguments = parent.TypeArguments;
                    for (int k = 0; k < typeArguments.Count; k++)
                    {
                        builder.Append("[");
                        builder.Append(GetRuntimeTypeName(typeArguments[k]));
                        builder.Append("]");
                        gcount--;
                        if (count > 0)
                            builder.Append(", ");
                    }
                }
                builder.Append("]");
            }
            if (tempType.IsArray)
            {
                CSharpType element = tempType;
                while (element != null && element.IsArray)
                {
                    builder.Append("[]");
                    element = element.ElementType;
                }
            }
            //if (withAssembly)
            //{
            //    builder.Append(", ");
            //    builder.Append(type.Assembly.Name);
            //}
            return builder.ToString();
        }
        public static CSharpType GetDefinitionType(CSharpType type)
        {
            if (type is TypeDefinitionInfo)
                return type;

            if (type.IsArray)
                return GetDefinitionType(type.ElementType);

            while (type.DefiningType != null)
                type = type.DefiningType;
            return type;
        }
        internal static IList<CSharpType> GetAllBaseInterfaces(IEnumerable<CSharpType> types)
        {
            HashSet<CSharpType> set = new HashSet<CSharpType>();
            GetAllBaseInterfaces(types, set);
            return new List<CSharpType>(set);
        }
        internal static IList<CSharpType> GetAllBaseInterfaces(CSharpType type)
        {
            HashSet<CSharpType> set = new HashSet<CSharpType>();
            GetAllBaseInterfaces(type, set);
            return new List<CSharpType>(set);
        }
        private static void GetAllBaseInterfaces(CSharpType type, HashSet<CSharpType> set)
        {
            if (type.IsInterface)
            {
                set.Add(type);
            }
            else
            {
                if (type.IsClass)
                {
                    CSharpType _base = type.BaseClass;
                    if (_base != null)
                        GetAllBaseInterfaces(_base.BaseInterfaces, set);
                }
            }
            GetAllBaseInterfaces(type.BaseInterfaces, set);
        }
        private static void GetAllBaseInterfaces(IEnumerable<CSharpType> types, HashSet<CSharpType> set)
        {
            foreach (var item in types)
            {
                GetAllBaseInterfaces(item, set);
            }
        }
        public static bool IsConstructedType(CSharpType type)
        {
            while (!type.IsConstructed)
            {
                type = type.ContainingType;
                if (type == null)
                    return false;
            }
            return true;
        }
    }
    [Flags]
    internal enum TypeAttributes
    {
        Public = 1,
        Internal = 2,
        NestedPublic = 4,
        NestedPrivate = 8,
        NestedProtected = 16,
        NestedInternal = 32,
        NestedProtectedOrInternal = 64,
        //NestedProtectedAndInternal = 128,
        //InferredAccessibility = 512,
        Sealed = 1024,
        Abstract = 2048,
        Delegate = 5120,
        Enum = 9216,
        ValueType = 17408,
        Pointer = 33792,
        //Variable = 65536,
        Class = 131072,
        Unknown = 394240,
        Anonymous = 656384,
        Interface = 1048576,
        //Unsafe = 2097152,
        //CoClass = 4194304,
        //Attribute = 8388608,
        //Hidden = 16777216,
        //Advanced = 33554432,
        //Obsolete = 67108864,
        Serializable = 134217728,
        FlagsAttribute = 274989056,
        VisibilityMask = 255,
        None = 0
    }
    internal class TypeDefinitionInfo : CSharpType, TypeParameterInfo.IProvider
    {
        public CSharpAssembly _Assembly;
        public Named _Name;
        public TypeAttributes _TypeAttributes;
        public CSharpNamespace _ContainingNamespace;
        public CSharpType _ContainingType;
        public List<CSharpType> _NestedTypes = new List<CSharpType>();
        public TypeParameterData[] _TypeParameters;
        public CSharpType[] _typeParameters;
        public List<CSharpType> _BaseTypes = new List<CSharpType>();
        public List<MemberDefinitionInfo> _Members = new List<MemberDefinitionInfo>();
        public Dictionary<CSharpType, object> _Attributes = new Dictionary<CSharpType, object>();
        public CSharpType _UnderlyingType;

        public override CSharpAssembly Assembly
        {
            get { return _Assembly; }
        }
        public override IList<CSharpAttribute0> Attributes
        {
            get
            {
                HashSet<CSharpAttribute0> types = new HashSet<CSharpAttribute0>();
                foreach (var item in _Attributes)
                    types.Add(new CSharpAttribute0(this, item.Key));
                foreach (var item in _BaseTypes)
                    foreach (var att in item.Attributes)
                        types.Add(att);
                return types.ToArray();
            }
        }
        internal virtual TypeAttributes TypeAttributes
        {
            get { return _TypeAttributes; }
        }
        public override Named Name
        {
            get { return _Name; }
        }
        public override IList<CSharpType> TypeParameters
        {
            get
            {
                if (_TypeParameters == null || _TypeParameters.Length == 0)
                    return EmptyList;
                if (_typeParameters == null)
                {
                    int count = _TypeParameters.Length;
                    _typeParameters = new CSharpType[count];
                    for (int i = 0; i < count; i++)
                        _typeParameters[i] = new TypeParameterInfo(this, i, this);
                }
                return _typeParameters;
            }
        }
        //public override int TypeParametersCount
        //{
        //    get
        //    {
        //        return _TypeParameters == null ? 0 : _TypeParameters.Length;
        //    }
        //}
        public override CSharpNamespace ContainingNamespace
        {
            get { return _ContainingNamespace != null ? _ContainingNamespace : (_ContainingType != null ? _ContainingType.ContainingNamespace : null); }
        }
        public override CSharpType ContainingType
        {
            get { return _ContainingType; }
        }
        public override CSharpType UnderlyingType
        {
            get
            {
                if (this.IsEnum)
                {
                    return _UnderlyingType;
                }
                return null;
            }
        }
        public override CSharpType BaseClass
        {
            get
            {
                CSharpType _base = _BaseTypes.FirstOrDefault(b => b.IsClass);
                if (_base == null)
                {
                    if (IsDelegate)
                        _base = MULTICAST_DELEGATE;
                    else if (IsStruct)
                        _base = VALUE_TYPE;
                    else if (IsEnum)
                        _base = ENUM;
                    // 在运行时环境以上可能都为null
                    if (_base == null && this != OBJECT
#if DEBUG
                        && (OBJECT._BaseTypes.Count == 0 || OBJECT._BaseTypes[0] != this)
#endif
                        )
                        _base = OBJECT;
                }
                return _base;
            }
        }
        public override IList<CSharpType> BaseInterfaces
        {
            get
            {
                if (_BaseTypes.Count == 0)
                    return EmptyList;
                return _BaseTypes.Where(t => t.IsInterface).ToList();
                //return GetBaseInterfaces(_BaseTypes);
            }
        }
        internal override IList<CSharpMember> MemberDefinitions
        {
            get { return _Members.ToArray(); }
        }
        public override IList<CSharpMember> Members
        {
            get
            {
                if (IsInterface)
                {
                    // 接口要包含其继承的所有接口成员
                    return _Members.Concat(GetAllBaseInterfaces(_BaseTypes).SelectMany(i => i.MemberDefinitions)).ToList();
                }

                var _base = BaseClass;
                if (_base != null)
                {
                    // 对于显示实现的成员，Members应该不能包含在内
                    return _Members.Where(m => m._ExplicitInterfaceImplementation == null).Concat(_base.Members).ToList();
                    //return FilterOverridedMember(_Members.Concat(_base.Members));
                }
                else
                {
                    return _Members.Select(m => (CSharpMember)m).ToList();
                }
            }
        }
        public override IList<CSharpType> NestedTypes
        {
            get
            {
                var _base = BaseClass;
                // 防止object继承其它框架的object类型时陷入死循环
                //if (this == OBJECT && _base != null)
                //{
                //    List<CSharpType> temp = new List<CSharpType>(1);
                //    temp.Add(_base);
                //    return temp;
                //}
                if (_base != null)
                    return _NestedTypes.Concat(_base.NestedTypes).ToList();
                else
                    return _NestedTypes;
            }
        }
        public override bool IsClass
        {
            get
            {
                return this.HasAttribute(TypeAttributes.Class);
            }
        }
        public override bool IsStruct
        {
            get
            {
                return this.HasAttribute(TypeAttributes.ValueType);
            }
        }
        public override bool IsInterface
        {
            get
            {
                return this.HasAttribute(TypeAttributes.Interface);
            }
        }
        public override bool IsEnum
        {
            get
            {
                return this.HasAttribute(TypeAttributes.Enum);
            }
        }
        public override bool IsDelegate
        {
            get
            {
                return this.HasAttribute(TypeAttributes.Delegate);
            }
        }
        public override bool IsSealed
        {
            get
            {
                return this.HasAttribute(TypeAttributes.Sealed);
            }
        }
        public override bool IsAbstract
        {
            get
            {
                return this.HasAttribute(TypeAttributes.Abstract);
            }
        }
        public override bool IsNested
        {
            get
            {
                return this.ContainingType != null;
            }
        }
        public override bool IsPublic
        {
            get
            {
                return this.HasVisibility(TypeAttributes.Public) || this.HasVisibility(TypeAttributes.NestedPublic);
            }
        }
        public override bool IsInternal
        {
            get
            {
                return this.HasVisibility(TypeAttributes.Internal) || this.HasVisibility(TypeAttributes.NestedInternal);
            }
        }
        public override bool IsProtected
        {
            get
            {
                return this.HasVisibility(TypeAttributes.NestedProtected);
            }
        }
        public override bool IsProtectedOrInternal
        {
            get
            {
                return this.HasVisibility(TypeAttributes.NestedProtectedOrInternal);
            }
        }
        public override bool IsPrivate
        {
            get
            {
                return this.HasVisibility(TypeAttributes.NestedPrivate);
            }
        }
        [ANonOptimize]public TypeDefinitionInfo()
        {
        }
        [ANonOptimize]internal virtual MemberDefinitionInfo CreateMemberDefinition(int memberIndex)
        {
            var member = new MemberDefinitionInfo(this, memberIndex);
            _Members.Add(member);
            return member;
        }
        private bool HasAttribute(TypeAttributes attribute)
        {
            return (this.TypeAttributes & attribute) == attribute;
        }
        private bool HasVisibility(TypeAttributes visibility)
        {
            return (this.TypeAttributes & TypeAttributes.VisibilityMask) == visibility;
        }
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (this.ContainingType != null)
            {
                stringBuilder.Append(this.ContainingType.ToString());
                stringBuilder.Append(".");
            }
            else
            {
                if (!this.ContainingNamespace.IsRoot)
                {
                    stringBuilder.Append(this.ContainingNamespace.ToString());
                    stringBuilder.Append(".");
                }
            }
            stringBuilder.Append(this.Name);
            if (this.TypeParameters.Count > 0)
            {
                stringBuilder.Append('<');
                for (int i = 0; i < this.TypeParameters.Count; i++)
                {
                    if (i > 0)
                    {
                        stringBuilder.Append(',');
                    }
                    stringBuilder.Append(this.TypeParameters[i].Name);
                }
                stringBuilder.Append('>');
            }
            return stringBuilder.ToString();
        }
        Named TypeParameterInfo.IProvider.GetName(int index)
        {
            return this._TypeParameters[index].Name;
        }
        bool TypeParameterInfo.IProvider.IsCovariant(int index)
        {
            return this._TypeParameters[index].IsCovariant;
        }
        bool TypeParameterInfo.IProvider.IsContravariant(int index)
        {
            return this._TypeParameters[index].IsContravariant;
        }
        IList<CSharpType> TypeParameterInfo.IProvider.GetTypeConstraints(int index)
        {
            if (_TypeParameters == null || _TypeParameters.Length == 0)
                return EmptyList;
            if (this._TypeParameters[index].TypeConstraints == null)
                return EmptyList;
            return this._TypeParameters[index].TypeConstraints;
        }
        bool TypeParameterInfo.IProvider.HasReferenceTypeConstraint(int index)
        {
            return this._TypeParameters[index].HasReferenceTypeConstraint;
        }
        bool TypeParameterInfo.IProvider.HasValueTypeConstraint(int index)
        {
            return this._TypeParameters[index].HasValueTypeConstraint;
        }
        bool TypeParameterInfo.IProvider.HasConstructorConstraint(int index)
        {
            return this._TypeParameters[index].HasConstructorConstraint;
        }

        internal void Add(TypeDefinitionInfo target)
        {
            if (target == null)
                throw new ArgumentNullException();
            if (target.Assembly != null && target.Assembly != this.Assembly)
                throw new InvalidOperationException();
            if (target.ContainingType == this)
                return;
            _NestedTypes.Add(target);
            if (target.ContainingType != null)
            {
                var oldParent = target.ContainingType as TypeDefinitionInfo;
                if (oldParent != null)
                    oldParent._NestedTypes.Remove(target);
            }
            target._ContainingType = this;
            target._ContainingNamespace = this.ContainingNamespace;
            target._Assembly = this.Assembly;
        }

        internal static List<CSharpMember> FilterOverridedMember(IEnumerable<CSharpMember> members)
        {
            List<CSharpMember> overrided = new List<CSharpMember>(16);
            List<CSharpMember> result = new List<CSharpMember>(32);
            foreach (var item in members)
            {
                if (item.IsOverride)
                    overrided.Add(item);

                if (item.IsOverride || item.IsVirtual || item.IsAbstract)
                {
                    // 筛选
                    // BUG: 带有泛型时，类型比较不能单纯相等，可能是Is
                    if (overrided.Any(m =>
                        // 名字一样
                        m.Name.Name == item.Name.Name
                        // 父类型不一致
                        && m.ContainingType != item.ContainingType
                        // 返回类型一样
                        && m.ReturnType == item.ReturnType
                        // 参数类型一样
                        && m.Parameters.Count == item.Parameters.Count && CSharpType.TypesEquals(m.Parameters.Select(p => p.Type).ToList(), item.Parameters.Select(t => t.Type).ToList())))
                    {
                        continue;
                    }
                }

                result.Add(item);
            }
            return result;
        }
    }
    internal class ArrayTypeReference : CSharpType, IEquatable<ArrayTypeReference>
    {
        public CSharpType gArrayType;
        public CSharpType elementType;
        public int rank;
        public Named name;
        public override CSharpAssembly Assembly
        {
            get
            {
                return this.elementType.Assembly;
            }
        }
        public override CSharpType BaseClass
        {
            get { return gArrayType; }
        }
        public override Named Name
        {
            get { return name; }
        }
        public override bool IsArray
        {
            get { return true; }
        }
        public override int Rank
        {
            get { return this.rank; }
        }
        public override CSharpType ElementType
        {
            get { return this.elementType; }
        }
        public override IList<CSharpType> BaseInterfaces
        {
            get { return gArrayType.BaseInterfaces; }
        }
        public override IList<CSharpMember> Members
        {
            get { return gArrayType.Members; }
        }
        internal override IList<CSharpMember> MemberDefinitions
        {
            get { return gArrayType.MemberDefinitions; }
        }
        public ArrayTypeReference() { }
        internal ArrayTypeReference(CSharpType elementType, int rank)
        {
            if (rank != 1)
                throw new InvalidCastException();
            this.elementType = elementType;
            this.rank = rank;
            if (___Array != null)
                this.gArrayType = CSharpType.CreateConstructedType(___Array, new CSharpType[] { elementType });
            this.name = new Named(elementType.Name.Name + "[]");
        }
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            CSharpType cSharpType = this;
            while (cSharpType.IsArray)
            {
                stringBuilder.Append('[');
                for (int i = 1; i < cSharpType.Rank; i++)
                    stringBuilder.Append(',');
                stringBuilder.Append(']');
                cSharpType = cSharpType.ElementType;
            }
            stringBuilder.Insert(0, cSharpType.ToString(), 1);
            return stringBuilder.ToString();
        }
        public override int GetHashCode()
        {
            return 354117 + this.rank.GetHashCode() + this.elementType.GetHashCode();
        }
        public override bool Equals(CSharpType other)
        {
            return this.Equals(other as ArrayTypeReference);
        }
        public bool Equals(ArrayTypeReference other)
        {
            return other != null && this.rank.Equals(other.rank) && this.elementType.Equals(other.elementType);
        }
    }
    internal class PointerTypeReference : CSharpType, IEquatable<PointerTypeReference>
    {
        public CSharpType elementType;
        public override CSharpAssembly Assembly
        {
            get
            {
                return this.elementType.Assembly;
            }
        }
        public override CSharpType ElementType
        {
            get
            {
                return this.elementType;
            }
        }
        public override bool IsPointer
        {
            get
            {
                return true;
            }
        }
        public PointerTypeReference() { }
        internal PointerTypeReference(CSharpType elementType)
        {
            this.elementType = elementType;
        }
        public override string ToString()
        {
            return string.Format("{0}*", this.elementType);
        }
        public override int GetHashCode()
        {
            return 268789573 + this.elementType.GetHashCode();
        }
        public override bool Equals(CSharpType other)
        {
            return this.Equals(other as PointerTypeReference);
        }
        public bool Equals(PointerTypeReference other)
        {
            return other != null && this.elementType.Equals(other.elementType);
        }
    }
    internal class ConstructedTypeReference : CSharpType, IEquatable<ConstructedTypeReference>
    {
        public CSharpType definingType;
        public CSharpType containingType;
        public IList<CSharpType> typeArguments;
        public List<CSharpType> typeParameters = new List<CSharpType>();
        public override bool IsConstructed { get { return true; } }
        public override Named Name { get { return this.definingType.Name; } }
        public override CSharpType DefiningType { get { return this.definingType; } }
        public override CSharpType ContainingType { get { return this.containingType; } }
        public override IList<CSharpType> TypeArguments { get { return this.typeArguments; } }


        public override IList<CSharpMember> Members
        {
            get { return this.definingType.Members.Select(m => (CSharpMember)new MemberWithType(this, m)).ToList(); }
        }
        internal override IList<CSharpMember> MemberDefinitions
        {
            get { return this.definingType.MemberDefinitions.Select(m => (CSharpMember)new MemberWithType(this, m)).ToList(); }
        }
        public override CSharpAssembly Assembly
        {
            get
            {
                return this.DefiningType.Assembly;
            }
        }
        public override IList<CSharpAttribute0> Attributes
        {
            get
            {
                return this.DefiningType.Attributes;
            }
        }
        public override CSharpType BaseClass
        {
            get
            {
                return this.CreateTypeParameterBinder().ProcessType(this.DefiningType.BaseClass);
            }
        }
        public override IList<CSharpType> BaseInterfaces
        {
            get
            {
                return this.CreateTypeParameterBinder().ProcessTypes(this.DefiningType.BaseInterfaces);
            }
        }
        public override CSharpNamespace ContainingNamespace
        {
            get
            {
                return this.DefiningType.ContainingNamespace;
            }
        }
        public override bool IsAbstract
        {
            get
            {
                return this.DefiningType.IsAbstract;
            }
        }
        public override bool IsAnonymousType
        {
            get
            {
                return this.DefiningType.IsAnonymousType;
            }
        }
        public override bool IsClass
        {
            get
            {
                return this.DefiningType.IsClass;
            }
        }
        public override bool IsDelegate
        {
            get
            {
                return this.DefiningType.IsDelegate;
            }
        }
        public override bool IsEnum
        {
            get
            {
                return this.DefiningType.IsEnum;
            }
        }
        public override bool IsInterface
        {
            get
            {
                return this.DefiningType.IsInterface;
            }
        }
        public override bool IsInternal
        {
            get
            {
                return this.DefiningType.IsInternal;
            }
        }
        public override bool IsNested
        {
            get
            {
                return this.DefiningType.IsNested;
            }
        }
        public override bool IsPrivate
        {
            get
            {
                return this.DefiningType.IsPrivate;
            }
        }
        public override bool IsProtected
        {
            get
            {
                return this.DefiningType.IsProtected;
            }
        }
        public override bool IsProtectedOrInternal
        {
            get
            {
                return this.DefiningType.IsProtectedOrInternal;
            }
        }
        public override bool IsPublic
        {
            get
            {
                return this.DefiningType.IsPublic;
            }
        }
        public override bool IsSealed
        {
            get
            {
                return this.DefiningType.IsSealed;
            }
        }
        public override bool IsStatic
        {
            get
            {
                return this.DefiningType.IsStatic;
            }
        }
        public override bool IsStruct
        {
            get
            {
                return this.DefiningType.IsStruct;
            }
        }
        public override IList<CSharpType> NestedTypes
        {
            get
            {
                return this.CreateTypeParameterBinder().ProcessTypes(this.DefiningType.NestedTypes);
            }
        }
        public override IList<CSharpType> TypeParameters
        {
            get
            {
                if (typeParameters == null)
                {
                    // class MyDictionary<T> : Dictionary<int, T>
                    // 以上情况typeof(MyDictionary<>).BaseType只有1个形参
                    var p = definingType.TypeParameters;
                    typeParameters = new List<CSharpType>(p.Count);
                    for (int i = 0; i < p.Count; i++)
                        if (typeArguments[i].IsTypeParameter)
                            typeParameters.Add(p[i]);
                }
                return typeParameters;
            }
        }
        public ConstructedTypeReference() { }
        internal ConstructedTypeReference(CSharpType definingType, CSharpType containingType, IList<CSharpType> typeArguments)
        {
            this.definingType = definingType;
            this.containingType = containingType;
            this.typeArguments = typeArguments;
        }
        public override bool Equals(CSharpType other)
        {
            return this.Equals(other as ConstructedTypeReference);
        }
        public bool Equals(ConstructedTypeReference other)
        {
            return other != null && ((this.containingType == null) ? this.definingType.Equals(other.definingType) : this.containingType.Equals(other.containingType)) && base.CompareTypeArguments(other);
        }
        public override int GetHashCode()
        {
            return ((this.containingType == null) ? this.definingType.GetHashCode() : this.containingType.GetHashCode()) + base.GetTypeArgumentsHashCode();
        }
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (this.ContainingType != null)
            {
                stringBuilder.Append(this.ContainingType.ToString());
                stringBuilder.Append(".");
            }
            else
            {
                if (!this.DefiningType.ContainingNamespace.IsRoot)
                {
                    stringBuilder.Append(this.DefiningType.ContainingNamespace.ToString());
                    stringBuilder.Append(".");
                }
            }
            stringBuilder.Append(this.DefiningType.Name);
            base.AppendTypeArguments(stringBuilder);
            return stringBuilder.ToString();
        }
        internal IList<CSharpType> CollectAllTypeParameters()
        {
            List<CSharpType> result = new List<CSharpType>();
            ConstructedTypeReference.CollectAllTypeParameters(this.definingType, result);
            return result;
        }
        internal static void CollectAllTypeParameters(CSharpType type, List<CSharpType> result)
        {
            CSharpType cSharpType = type.ContainingType;
            if (cSharpType != null)
            {
                ConstructedTypeReference.CollectAllTypeParameters(cSharpType, result);
            }
            foreach (CSharpType current in type.TypeParameters)
            {
                result.Add(current);
            }
        }
        internal IList<CSharpType> CollectAllTypeArguments()
        {
            List<CSharpType> result = new List<CSharpType>();
            ConstructedTypeReference.CollectAllTypeArguments(this, result);
            return result;
        }
        internal static void CollectAllTypeArguments(CSharpType type, List<CSharpType> result)
        {
            CSharpType cSharpType = type.ContainingType;
            if (cSharpType != null)
            {
                ConstructedTypeReference.CollectAllTypeArguments(cSharpType, result);
            }
            foreach (CSharpType current in type.TypeArguments)
            {
                result.Add(current);
            }
        }
        internal TypeParameterBinder CreateTypeParameterBinder()
        {
            IList<CSharpType> typeParameters = this.CollectAllTypeParameters();
            IList<CSharpType> list = this.CollectAllTypeArguments();
            return new TypeParameterBinder(typeParameters, list);
        }
    }
    internal class TypeParameterInfo : CSharpType
    {
        internal interface IProvider
        {
            Named GetName(int index);
            bool IsCovariant(int index);
            bool IsContravariant(int index);
            IList<CSharpType> GetTypeConstraints(int index);
            bool HasReferenceTypeConstraint(int index);
            bool HasValueTypeConstraint(int index);
            bool HasConstructorConstraint(int index);
        }
        public CSharpType definingType;
        public CSharpMember definingMember;
        public int position;
        public TypeParameterInfo.IProvider infoProvider;
        public override CSharpAssembly Assembly
        {
            get
            {
                return this.definingType.Assembly;
            }
        }
        public override Named Name
        {
            get { return infoProvider.GetName(position); }
        }
        public override bool IsCovariant
        {
            get { return infoProvider.IsCovariant(position); }
        }
        public override bool IsContravariant
        {
            get { return infoProvider.IsContravariant(position); }
        }
        public override CSharpType DefiningType
        {
            get
            {
                return this.definingType;
            }
        }
        public override CSharpMember DefiningMember
        {
            get
            {
                return this.definingMember;
            }
        }
        public override bool HasConstructorConstraint
        {
            get { return infoProvider.HasConstructorConstraint(position); }
        }
        public override bool HasValueTypeConstraint
        {
            get { return infoProvider.HasValueTypeConstraint(position); }
        }
        public override bool HasReferenceTypeConstraint
        {
            get { return infoProvider.HasReferenceTypeConstraint(position); }
        }
        public override IList<CSharpType> TypeConstraints
        {
            get { return infoProvider.GetTypeConstraints(position); }
        }
        public override bool IsTypeParameter { get { return true; } }
        public override int TypeParameterPosition
        {
            get
            {
                return this.position;
            }
        }
        public override CSharpType BaseClass
        {
            get
            {
                if (HasValueTypeConstraint)
                    return VALUE_TYPE;
                if (HasReferenceTypeConstraint)
                    return OBJECT;
                var constraints = TypeConstraints;
                if (constraints.Count == 0)
                    return OBJECT;
                CSharpType _class = constraints.FirstOrDefault(c => c.IsClass);
                if (_class == null)
                    return OBJECT;
                return _class;
            }
        }
        public override IList<CSharpType> BaseInterfaces
        {
            get
            {
                var constraints = TypeConstraints;
                if (constraints.Count == 0)
                    return EmptyList;
                return constraints.Where(t => t.IsInterface).ToList();
                //return TypeDefinitionInfo.GetAllBaseInterfaces(constraints);
            }
        }
        public override IList<CSharpMember> Members
        {
            get { return BaseClass.Members.Concat(CSharpType.GetAllBaseInterfaces(TypeConstraints).SelectMany(i => i.MemberDefinitions)).ToList(); }
        }
        internal override IList<CSharpMember> MemberDefinitions
        {
            get { return BaseClass.MemberDefinitions; }
        }
        public TypeParameterInfo() { }
        internal TypeParameterInfo(CSharpType definingType, int position, TypeParameterInfo.IProvider infoProvider)
        {
            this.definingType = definingType;
            this.position = position;
            this.infoProvider = infoProvider;
        }
        internal TypeParameterInfo(CSharpMember definingMember, int position, TypeParameterInfo.IProvider infoProvider)
        {
            this.definingType = definingMember.ContainingType;
            this.definingMember = definingMember;
            this.position = position;
            this.infoProvider = infoProvider;
        }
        public override int GetHashCode()
        {
            if (this.definingMember == null)
            {
                return this.definingType.GetHashCode() + this.position.GetHashCode();
            }
            return this.definingMember.GetHashCode() + this.position.GetHashCode();
        }
        public override bool Equals(CSharpType other)
        {
            return this.Equals(other as TypeParameterInfo);
        }
        public bool Equals(TypeParameterInfo other)
        {
            if (other == null)
            {
                return false;
            }
            CSharpMember memberDefinitionInfo = this.definingMember;
            CSharpMember memberDefinitionInfo2 = other.definingMember;
            if (memberDefinitionInfo == null && memberDefinitionInfo2 == null)
            {
                return this.definingType.Equals(other.definingType) && this.position.Equals(other.position);
            }
            return !(memberDefinitionInfo == null) && !(memberDefinitionInfo2 == null) && memberDefinitionInfo.Equals(memberDefinitionInfo2) && this.position.Equals(other.position);
        }
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if (this.definingMember == null)
            {
                builder.Append(this.definingType.ToString());
                builder.Append('`');
                builder.Append(this.Name.Name);
            }
            else
            {
                builder.Append(this.definingMember.ToString());
                builder.Append('`');
                builder.Append(this.Name.Name);
            }
            return builder.ToString();
        }
    }
    internal class CSharpTypeVisitor<TResult>
    {
        public virtual TResult Visit(CSharpType type)
        {
            if (type == null)
            {
                return default(TResult);
            }
            if (type.IsArray)
            {
                return this.VisitArray(type);
            }
            if (type.IsPointer)
            {
                return this.VisitPointer(type);
            }
            if (type.IsAnonymousType)
            {
                return this.VisitAnonymousType(type);
            }
            if (type.IsTypeParameter)
            {
                return this.VisitTypeParameter(type);
            }
            if (type.DefiningType == null)
            {
                return this.VisitTypeDefinition(type);
            }
            return this.VisitConstructedType(type);
        }
        protected virtual TResult VisitConstructedType(CSharpType type)
        {
            return default(TResult);
        }
        protected virtual TResult VisitTypeDefinition(CSharpType type)
        {
            return default(TResult);
        }
        protected virtual TResult VisitTypeParameter(CSharpType type)
        {
            return default(TResult);
        }
        protected virtual TResult VisitAnonymousType(CSharpType type)
        {
            return default(TResult);
        }
        protected virtual TResult VisitPointer(CSharpType type)
        {
            return default(TResult);
        }
        protected virtual TResult VisitArray(CSharpType type)
        {
            return default(TResult);
        }
    }
    internal class TypeParameterBinder : CSharpTypeVisitor<CSharpType>
    {
        private readonly IList<CSharpType> typeParameters;
        private readonly IList<CSharpType> typeArguments;
        public IList<CSharpType> TypeArguments
        {
            get
            {
                return this.typeArguments;
            }
        }
        public IList<CSharpType> TypeParameters
        {
            get
            {
                return this.typeParameters;
            }
        }
        public TypeParameterBinder(IList<CSharpType> typeParameters, IList<CSharpType> typeArguments)
        {
            //if (typeParameters.Count != typeArguments.Count)
            //{
            //    throw new ArgumentException("Partial binding of type parameters is not allowed");
            //}
            this.typeParameters = typeParameters;
            this.typeArguments = typeArguments;
        }
        public CSharpType ProcessType(CSharpType type)
        {
            return this.Visit(type);
        }
        public IList<CSharpType> ProcessTypes(IList<CSharpType> types)
        {
            return types.Select(type => this.ProcessType(type)).ToList();
        }
        protected override CSharpType VisitArray(CSharpType type)
        {
            CSharpType elementType = this.ProcessType(type.ElementType);
            return CSharpType.CreateArray(type.Rank, elementType);
        }
        protected override CSharpType VisitPointer(CSharpType type)
        {
            CSharpType elementType = this.ProcessType(type.ElementType);
            return CSharpType.CreatePointer(elementType);
        }
        protected override CSharpType VisitTypeDefinition(CSharpType type)
        {
            CSharpType containingType = this.ProcessType(type.ContainingType);
            IList<CSharpType> list = this.ProcessTypes(type.TypeParameters);
            return CSharpType.CreateConstructedType(type, containingType, list);
        }
        protected override CSharpType VisitConstructedType(CSharpType type)
        {
            CSharpType containingType = this.ProcessType(type.ContainingType);
            CSharpType definingType = type.DefiningType;
            IList<CSharpType> list = this.ProcessTypes(type.TypeArguments);
            return CSharpType.CreateConstructedType(definingType, containingType, list);
        }
        protected override CSharpType VisitAnonymousType(CSharpType type)
        {
            return type;
        }
        protected override CSharpType VisitTypeParameter(CSharpType type)
        {
            // hack: 不知为何部分会找不到，也不进类型的Equals 具体类型FirstOrDefault.TSource
            //int num = this.typeParameters.IndexOf(type);
            int num = IndexOf(type);
            if (num < 0)
                return type;
            if (num >= this.typeArguments.Count || this.typeArguments[num] == null)
                return type;
            return this.typeArguments[num];
        }
        int IndexOf(CSharpType type)
        {
            int count = this.typeParameters.Count;
            if (type == null)
            {
                for (int i = 0; i < count; i++)
                {
                    if (this.typeParameters[i] == null)
                        return i;
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    if (type.Equals(this.typeParameters[i]))
                        return i;
                }
            }
            return -1;
        }
    }
    internal class TypeIs : CSharpTypeVisitor<CSharpType>
    {
        internal CSharpType tester;
        public TypeIs(CSharpType type)
        {
            this.tester = type;
        }
        public bool Is(CSharpType target)
        {
            return Is(target, out target);
        }
        public bool Is(CSharpType target, out CSharpType result)
        {
            if (target == null)
                throw new ArgumentNullException();
            result = Visit(target);
            return result != null;
        }
        private CSharpType InternalIs(CSharpType target)
        {
            if (Is(target, out target))
                return target;
            else
                return null;
        }
        public override CSharpType Visit(CSharpType type)
        {
            if (tester == null)
                return null;
            if (tester.Equals(type))
                return tester;
            return base.Visit(type);
        }
        protected override CSharpType VisitArray(CSharpType type)
        {
            // 不考虑协变逆变的情况下，数组类型必须完全一致
            if (tester.IsArray && type.ElementType.IsTypeParameter)
            {
                var temp = tester;
                this.tester = temp.ElementType;
                if (VisitTypeParameter(type) != null)
                    return temp;
            }
            return null;
        }
        protected override CSharpType VisitConstructedType(CSharpType type)
        {
            // 嵌套可空Nullable<Nullable<int>>是不允许的，编译会报错
            if (type.DefiningType == CSharpType.NULLABLE)
                return InternalIs(type.TypeArguments[0]);
            return VisitTypeDefinition(type);
        }
        protected override CSharpType VisitTypeParameter(CSharpType type)
        {
            if (type.HasValueTypeConstraint && !tester.IsValueType)
                return null;

            if (type.HasConstructorConstraint)
            {
                if (tester.IsAbstract)
                    return null;
                var members = tester.Members;
                // 是否有显示定义构造函数
                bool hasConstructor = false;
                foreach (var item in members)
                {
                    if (item.IsConstructor && !item.IsStatic)
                    {
                        if (item.Parameters.Count == 0)
                        {
                            if (item.IsPublic)
                                break;
                            return null;
                        }
                        hasConstructor = true;
                    }
                }
                if (hasConstructor && !tester.IsStruct)
                    return null;
            }

            if (type.HasReferenceTypeConstraint && !(tester.IsClass || tester.IsInterface))
                return null;

            var constraints = type.TypeConstraints;
            foreach (var item in constraints)
                if (!this.Is(item))
                    return null;

            return tester;
        }
        protected override CSharpType VisitTypeDefinition(CSharpType type)
        {
            var temp = tester;

            CSharpType target = type;
            if (type.IsConstructed)
                target = type.DefiningType;
            if (temp.IsConstructed && temp.DefiningType.Equals(target))
            {
                if (!type.IsConstructed || temp.CompareTypeArgumentsIs(type))
                    // 泛型类的情况，需要匹配泛型实参是否符合
                    return temp;
                else
                    return null;
            }

            tester = temp.BaseClass;
            if (tester != null)
            {
                // 对object类型的判断加速
                if (tester != CSharpType.OBJECT)
                {
                    if (Is(type))
                        return tester;
                }
                else
                {
                    if (tester.Equals(type))
                        return tester;
                }
            }
            foreach (var item in temp.BaseInterfaces)
            {
                tester = item;
                if (Is(type))
                    return tester;
            }
            return null;
        }
        protected override CSharpType VisitAnonymousType(CSharpType type)
        {
            throw new NotImplementedException();
        }
        protected override CSharpType VisitPointer(CSharpType type)
        {
            throw new NotImplementedException();
        }
        public static bool Is(CSharpType type, CSharpType target, out CSharpType result)
        {
            return new TypeIs(type).Is(target, out result);
        }
    }

    [Flags]
    internal enum MemberAttributes
    {
        None = 0,
        VisibilityMask = 7,
        Public = 5,
        ProtectedOrInternal = 4,
        Protected = 3,
        Internal = 2,
        Private = 1,
        PartialMethodSignature = 16,
        PartialMethodImplementation = 32,
        PartialMask = 48,
        StartupMainMethod = 64,
        Abstract = 128,
        Extern = 256,
        New = 512,
        Override = 1024,
        ReadOnly = 2048,
        Sealed = 4096,
        Static = 8192,
        Virtual = 16384,
        InterfaceMember = 32768,
        PropertyGetter = 65536,
        PropertySetter = 131072,
        AdvancedInconsistent = 262144,
        HiddenInconsistent = 524288,
        Advanced = 1048576,
        Hidden = 2097152,
        Unsafe = 4194304,
        Volatile = 8388608,
        Obsolete = 67108864,
        Synthesized = 134217728,
        PropertySetterDiffers = 16777216,
        PropertyGetterDiffers = 33554432,
        PropertyAccessibilityShift = 28,
        PropertyPublic = 1342177280,
        PropertyPrivate = 268435456,
        PropertyProtected = 805306368,
        PropertyInternal = 536870912,
        PropertyProtectedOrInternal = 1073741824,
        PropertyVisibilityMask = 1879048192
    }
    public abstract class CSharpMember : IEquatable<CSharpMember>
    {
        public static readonly CSharpMember[] EmptyList = new CSharpMember[0];
        [AInvariant]public virtual Named Name
        {
            get { return null; }
        }
        [AInvariant]public virtual bool IsMethod
        {
            get
            {
                return false;
            }
        }
        [AInvariant]public virtual bool IsConstructor
        {
            get
            {
                return false;
            }
        }
        [AInvariant]public virtual bool IsDestructor
        {
            get
            {
                return false;
            }
        }
        [AInvariant]public virtual bool IsOperator
        {
            get
            {
                return false;
            }
        }
        [AInvariant]public virtual bool IsIndexer
        {
            get
            {
                return false;
            }
        }
        [AInvariant]public virtual bool IsProperty
        {
            get
            {
                return false;
            }
        }
        [AInvariant]public virtual bool IsField
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsFixedSizeBuffer
        {
            get
            {
                return false;
            }
        }
        [AInvariant]public virtual bool IsConstant
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsEvent
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsEnumMember
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsGetAccessor
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsSetAccessor
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsAddAccessor
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsRemoveAccessor
        {
            get
            {
                return false;
            }
        }
        public virtual int FixedBufferSize
        {
            get
            {
                return -1;
            }
        }
        public virtual object ConstantValue
        {
            get
            {
                return false;
            }
        }
        public virtual IList<CSharpMember> Accessors
        {
            get
            {
                return CSharpMember.EmptyList;
            }
        }
        [AInvariant]public abstract CSharpType ContainingType
        {
            get;
        }
        public virtual CSharpMember DefiningMember
        {
            get
            {
                return null;
            }
        }
        public virtual CSharpType ReturnType
        {
            get
            {
                return null;
            }
        }
        public virtual IList<CSharpParameter> Parameters
        {
            get
            {
                return CSharpParameter.EmptyList;
            }
        }
        public virtual CSharpType ExplicitInterfaceImplementation
        {
            get
            {
                return null;
            }
        }
        public virtual IList<CSharpAttribute0> Attributes
        {
            get
            {
                return CSharpAttribute0.EmptyList;
            }
        }
        public abstract CSharpAssembly Assembly
        {
            get;
        }
        public virtual bool IsPublic
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsPrivate
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsProtected
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsProtectedOrInternal
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsInternal
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsNew
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsVirtual
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsAbstract
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsStatic
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsReadonly
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsOverride
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsPartialImplementation
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsPartialSignature
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsVolatile
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsExtern
        {
            get
            {
                return false;
            }
        }
        public virtual bool IsSealed
        {
            get
            {
                return false;
            }
        }
        public virtual IList<CSharpType> TypeParameters
        {
            get
            {
                return CSharpType.EmptyList;
            }
        }
        public virtual IList<CSharpType> TypeArguments
        {
            get
            {
                return CSharpType.EmptyList;
            }
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as CSharpMember);
        }
        public abstract override int GetHashCode();
        public abstract bool Equals(CSharpMember other);
    }
    internal enum MemberDefinitionKind
    {
        Method,
        Constructor,
        Destructor,
        Operator,
        Indexer,
        Property,
        Field,
        Constant,
        Event,
        EnumMember,
        Unknown
    }
    [AInvariant]internal class MemberDefinitionInfo : CSharpMember, CSharpParameter.IProvider, TypeParameterInfo.IProvider, IEquatable<MemberDefinitionInfo>
    {
        [AInvariant]internal struct ParameterData
        {
            public Named Name;
            public CSharpType Type;
            public EDirection Direction;
            public bool IsThis { get { return Direction == EDirection.THIS; } }
            public bool IsParams { get { return Direction == EDirection.PARAMS; } }
            public bool IsRef { get { return Direction == EDirection.REF; } }
            public bool IsOut { get { return Direction == EDirection.OUT; } }
            public string DefaultValueString;
            public bool MatchParameter(CSharpParameter parameter)
            {
                if (IsRef != parameter.IsRef
                    || IsOut != parameter.IsOut
                    || IsParams != parameter.IsParams
                    || IsThis != parameter.IsThis)
                    return false;
                return Type.Equals(parameter.Type);
            }
        }
        internal static ParameterData[] EmptyList = new ParameterData[0];

        public TypeDefinitionInfo definingType;
        public int memberIndex;
        [AInvariant]public Named _Name;
        [AInvariant]public MemberAttributes _MemberAttributes;
        [AInvariant]public MemberDefinitionKind _Kind;
        [AInvariant]public CSharpType _ReturnType;
        public CSharpType _ExplicitInterfaceImplementation;
        [AInvariant]public TypeParameterData[] _TypeParameters;
        [AInvariant]public ParameterData[] _Parameters;
        [AInvariant]public Dictionary<CSharpType, object> _Attributes = new Dictionary<CSharpType, object>();
        [AInvariant]public List<CSharpMember> _Accessors = new List<CSharpMember>();
        public object _ConstantValue;

        public int MemberIndex
        {
            get
            {
                return this.memberIndex;
            }
        }
        public TypeDefinitionInfo DefiningType
        {
            get
            {
                return this.definingType;
            }
        }
        public override CSharpType ContainingType
        {
            get
            {
                return this.DefiningType;
            }
        }
        public override CSharpAssembly Assembly
        {
            get
            {
                return this.definingType.Assembly;
            }
        }
        internal virtual MemberAttributes MemberAttributes
        {
            get { return _MemberAttributes; }
        }
        internal virtual MemberDefinitionKind Kind
        {
            get { return _Kind; }
        }
        public override CSharpType ReturnType
        {
            get { return _ReturnType; }
        }
        public override CSharpType ExplicitInterfaceImplementation
        {
            get { return _ExplicitInterfaceImplementation; }
        }
        public override Named Name
        {
            get { return _Name; }
        }
        public override bool IsMethod
        {
            get
            {
                return this.Kind == MemberDefinitionKind.Method;
            }
        }
        public override bool IsConstructor
        {
            get
            {
                return this.Kind == MemberDefinitionKind.Constructor;
            }
        }
        public override bool IsDestructor
        {
            get
            {
                return this.Kind == MemberDefinitionKind.Destructor;
            }
        }
        public override bool IsOperator
        {
            get
            {
                return this.Kind == MemberDefinitionKind.Operator;
            }
        }
        public override bool IsIndexer
        {
            get
            {
                return this.Kind == MemberDefinitionKind.Indexer;
            }
        }
        public override bool IsProperty
        {
            get
            {
                return this.Kind == MemberDefinitionKind.Property;
            }
        }
        public override bool IsField
        {
            get
            {
                return this.Kind == MemberDefinitionKind.Field;
            }
        }
        public override bool IsConstant
        {
            get
            {
                return this.Kind == MemberDefinitionKind.Constant;
            }
        }
        public override bool IsEvent
        {
            get
            {
                return this.Kind == MemberDefinitionKind.Event;
            }
        }
        public override bool IsEnumMember
        {
            get
            {
                return this.Kind == MemberDefinitionKind.EnumMember;
            }
        }
        public override bool IsPublic
        {
            get
            {
                return this.HasVisibility(MemberAttributes.Public);
            }
        }
        public override bool IsInternal
        {
            get
            {
                return this.HasVisibility(MemberAttributes.Internal);
            }
        }
        public override bool IsProtected
        {
            get
            {
                return this.HasVisibility(MemberAttributes.Protected);
            }
        }
        public override bool IsProtectedOrInternal
        {
            get
            {
                return this.HasVisibility(MemberAttributes.ProtectedOrInternal);
            }
        }
        public override bool IsPrivate
        {
            get
            {
                return this.HasVisibility(MemberAttributes.Private);
            }
        }
        public override bool IsNew
        {
            get
            {
                return this.HasAttribute(MemberAttributes.New);
            }
        }
        public override bool IsVirtual
        {
            get
            {
                return this.HasAttribute(MemberAttributes.Virtual);
            }
        }
        public override bool IsAbstract
        {
            get
            {
                return this.HasAttribute(MemberAttributes.Abstract);
            }
        }
        public override bool IsStatic
        {
            get
            {
                return this.HasAttribute(MemberAttributes.Static);
            }
        }
        public override bool IsReadonly
        {
            get
            {
                if (this.IsProperty)
                {
                    return this.HasAttribute(MemberAttributes.PropertyGetter) && !this.HasAttribute(MemberAttributes.PropertySetter);
                }
                return this.HasAttribute(MemberAttributes.ReadOnly);
            }
        }
        public override bool IsOverride
        {
            get
            {
                return this.HasAttribute(MemberAttributes.Override);
            }
        }
        public override bool IsPartialImplementation
        {
            get
            {
                return this.HasAttribute(MemberAttributes.PartialMethodImplementation);
            }
        }
        public override bool IsPartialSignature
        {
            get
            {
                return this.HasAttribute(MemberAttributes.PartialMethodSignature);
            }
        }
        public override bool IsVolatile
        {
            get
            {
                return this.HasAttribute(MemberAttributes.Volatile);
            }
        }
        public override bool IsExtern
        {
            get
            {
                return this.HasAttribute(MemberAttributes.Extern);
            }
        }
        public override bool IsSealed
        {
            get
            {
                return this.HasAttribute(MemberAttributes.Sealed);
            }
        }
        public override IList<CSharpMember> Accessors
        {
            get { return _Accessors; }
        }
        public override IList<CSharpType> TypeParameters
        {
            get
            {
                if (_TypeParameters == null || _TypeParameters.Length == 0)
                    return CSharpType.EmptyList;
                int count = _TypeParameters.Length;
                CSharpType[] types = new CSharpType[count];
                for (int i = 0; i < count; i++)
                    types[i] = new TypeParameterInfo(this, i, this);
                return types;
            }
        }
        public override IList<CSharpParameter> Parameters
        {
            get
            {
                if (_Parameters == null || _Parameters.Length == 0)
                    return CSharpParameter.EmptyList;
                int count = _Parameters.Length;
                CSharpParameter[] parameters = new CSharpParameter[count];
                for (int i = 0; i < count; i++)
                    parameters[i] = new CSharpParameter(this, i, this);
                return parameters;
            }
        }
        public override IList<CSharpAttribute0> Attributes
        {
            get
            {
                List<CSharpAttribute0> types = new List<CSharpAttribute0>();
                foreach (var item in _Attributes)
                    types.Add(new CSharpAttribute0(this, item.Key));
                return types;
            }
        }
        internal MemberDefinitionInfo(TypeDefinitionInfo definingType, int memberIndex)
        {
            this.definingType = definingType;
            this.memberIndex = memberIndex;
        }
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(ContainingType.Name.Name);
            builder.Append('.');
            builder.Append(this.Name.Name);
            return builder.ToString();
        }
        public override bool Equals(CSharpMember other)
        {
            return this.Equals(other as MemberDefinitionInfo);
        }
        public bool Equals(MemberDefinitionInfo other)
        {
            return other != null && this.definingType.Equals(other.definingType) && this.memberIndex.Equals(other.memberIndex);
        }
        public override int GetHashCode()
        {
            return this.definingType.GetHashCode() + this.memberIndex.GetHashCode();
        }
        private bool HasAttribute(MemberAttributes attribute)
        {
            return (this.MemberAttributes & attribute) == attribute;
        }
        private bool HasVisibility(MemberAttributes visibility)
        {
            return (this.MemberAttributes & MemberAttributes.VisibilityMask) == visibility;
        }
        Named CSharpParameter.IProvider.GetName(int index)
        {
            return this._Parameters[index].Name;
        }
        CSharpType CSharpParameter.IProvider.GetType(int index)
        {
            return this._Parameters[index].Type;
        }
        bool CSharpParameter.IProvider.IsThis(int index)
        {
            return this._Parameters[index].IsThis;
        }
        bool CSharpParameter.IProvider.IsParams(int index)
        {
            return this._Parameters[index].IsParams;
        }
        bool CSharpParameter.IProvider.IsRef(int index)
        {
            return this._Parameters[index].IsRef;
        }
        bool CSharpParameter.IProvider.IsOut(int index)
        {
            return this._Parameters[index].IsOut;
        }
        CSharpDefaultParameterValue CSharpParameter.IProvider.GetDefaultValue(int index)
        {
            return new CSharpDefaultParameterValue(this._Parameters[index].DefaultValueString);
        }
        Named TypeParameterInfo.IProvider.GetName(int index)
        {
            return this._TypeParameters[index].Name;
        }
        bool TypeParameterInfo.IProvider.IsCovariant(int index)
        {
            return this._TypeParameters[index].IsCovariant;
        }
        bool TypeParameterInfo.IProvider.IsContravariant(int index)
        {
            return this._TypeParameters[index].IsContravariant;
        }
        IList<CSharpType> TypeParameterInfo.IProvider.GetTypeConstraints(int index)
        {
            if (_TypeParameters == null || _TypeParameters.Length == 0)
                return CSharpType.EmptyList;
            if (this._TypeParameters[index].TypeConstraints == null)
                return CSharpType.EmptyList;
            return this._TypeParameters[index].TypeConstraints;
        }
        bool TypeParameterInfo.IProvider.HasReferenceTypeConstraint(int index)
        {
            return this._TypeParameters[index].HasReferenceTypeConstraint;
        }
        bool TypeParameterInfo.IProvider.HasValueTypeConstraint(int index)
        {
            return this._TypeParameters[index].HasValueTypeConstraint;
        }
        bool TypeParameterInfo.IProvider.HasConstructorConstraint(int index)
        {
            return this._TypeParameters[index].HasConstructorConstraint;
        }
    }
    internal class MemberWithType : CSharpMember, CSharpParameter.IProvider, TypeParameterInfo.IProvider, IEquatable<MemberWithType>
    {
        public ConstructedTypeReference containingType;
        public CSharpMember definingMember;
        public IList<CSharpParameter> parameters;
        public IList<CSharpType> typeParameters;
        public override CSharpType ContainingType
        {
            get
            {
                return this.containingType;
            }
        }
        public override CSharpMember DefiningMember
        {
            get { return this.definingMember; }
        }
        public override CSharpType ExplicitInterfaceImplementation
        {
            get { return DefiningMember.ExplicitInterfaceImplementation; }
        }
        public override CSharpAssembly Assembly
        {
            get
            {
                return this.DefiningMember.Assembly;
            }
        }
        public override Named Name
        {
            get
            {
                return this.DefiningMember.Name;
            }
        }
        public override bool IsConstant
        {
            get
            {
                return this.DefiningMember.IsConstant;
            }
        }
        public override bool IsAddAccessor
        {
            get
            {
                return this.DefiningMember.IsAddAccessor;
            }
        }
        public override bool IsConstructor
        {
            get
            {
                return this.DefiningMember.IsConstructor;
            }
        }
        public override bool IsDestructor
        {
            get
            {
                return this.DefiningMember.IsDestructor;
            }
        }
        public override bool IsEnumMember
        {
            get
            {
                return this.DefiningMember.IsEnumMember;
            }
        }
        public override bool IsEvent
        {
            get
            {
                return this.DefiningMember.IsEvent;
            }
        }
        public override bool IsField
        {
            get
            {
                return this.DefiningMember.IsField;
            }
        }
        public override bool IsFixedSizeBuffer
        {
            get
            {
                return this.DefiningMember.IsFixedSizeBuffer;
            }
        }
        public override bool IsGetAccessor
        {
            get
            {
                return this.DefiningMember.IsGetAccessor;
            }
        }
        public override bool IsIndexer
        {
            get
            {
                return this.DefiningMember.IsIndexer;
            }
        }
        public override bool IsAbstract
        {
            get
            {
                return this.DefiningMember.IsAbstract;
            }
        }
        public override bool IsExtern
        {
            get
            {
                return this.DefiningMember.IsExtern;
            }
        }
        public override bool IsInternal
        {
            get
            {
                return this.DefiningMember.IsInternal;
            }
        }
        public override bool IsMethod
        {
            get
            {
                return this.DefiningMember.IsMethod;
            }
        }
        public override bool IsNew
        {
            get
            {
                return this.DefiningMember.IsNew;
            }
        }
        public override bool IsOperator
        {
            get
            {
                return this.DefiningMember.IsOperator;
            }
        }
        public override bool IsOverride
        {
            get
            {
                return this.DefiningMember.IsOverride;
            }
        }
        public override bool IsPrivate
        {
            get
            {
                return this.DefiningMember.IsPrivate;
            }
        }
        public override bool IsPartialImplementation
        {
            get
            {
                return this.DefiningMember.IsPartialImplementation;
            }
        }
        public override bool IsPartialSignature
        {
            get
            {
                return this.DefiningMember.IsPartialSignature;
            }
        }
        public override bool IsProperty
        {
            get
            {
                return this.DefiningMember.IsProperty;
            }
        }
        public override bool IsProtected
        {
            get
            {
                return this.DefiningMember.IsProtected;
            }
        }
        public override bool IsProtectedOrInternal
        {
            get
            {
                return this.DefiningMember.IsProtectedOrInternal;
            }
        }
        public override bool IsPublic
        {
            get
            {
                return this.DefiningMember.IsPublic;
            }
        }
        public override bool IsReadonly
        {
            get
            {
                return this.DefiningMember.IsReadonly;
            }
        }
        public override bool IsRemoveAccessor
        {
            get
            {
                return this.DefiningMember.IsRemoveAccessor;
            }
        }
        public override bool IsSealed
        {
            get
            {
                return this.DefiningMember.IsSealed;
            }
        }
        public override bool IsSetAccessor
        {
            get
            {
                return this.DefiningMember.IsSetAccessor;
            }
        }
        public override bool IsStatic
        {
            get
            {
                return this.DefiningMember.IsStatic;
            }
        }
        public override bool IsVirtual
        {
            get
            {
                return this.DefiningMember.IsVirtual;
            }
        }
        public override bool IsVolatile
        {
            get
            {
                return this.DefiningMember.IsVolatile;
            }
        }
        public override CSharpType ReturnType
        {
            get
            {
                return this.CreateTypeParameterBinder().ProcessType(this.DefiningMember.ReturnType);
            }
        }
        public override IList<CSharpMember> Accessors
        {
            get
            {
                return this.DefiningMember.Accessors;
            }
        }
        public override IList<CSharpParameter> Parameters
        {
            get
            {
                if (this.parameters == null)
                {
                    int count = this.DefiningMember.Parameters.Count;
                    List<CSharpParameter> array = new List<CSharpParameter>(count);
                    for (int i = 0; i < count; i++)
                    {
                        array.Add(new CSharpParameter(this, i, this));
                    }
                    this.parameters = array;
                }
                return this.parameters;
            }
        }
        public override IList<CSharpType> TypeParameters
        {
            get
            {
                if (this.typeParameters == null)
                {
                    int count = this.DefiningMember.TypeParameters.Count;
                    List<CSharpType> array = new List<CSharpType>(count);
                    for (int i = 0; i < count; i++)
                    {
                        array.Add(new TypeParameterInfo(this.DefiningMember, i, this));
                    }
                    this.typeParameters = array;
                }
                return this.typeParameters;
            }
        }
        public MemberWithType() { }
        public MemberWithType(ConstructedTypeReference containingType, CSharpMember member)
        {
            this.containingType = containingType;
            this.definingMember = member;
        }
        private TypeParameterBinder CreateTypeParameterBinder()
        {
            return this.containingType.CreateTypeParameterBinder();
        }
        public override bool Equals(CSharpMember other)
        {
            return this.Equals(other as MemberWithType);
        }
        public bool Equals(MemberWithType other)
        {
            return other != null && this.containingType.Equals(other.containingType) && this.definingMember.Equals(other.definingMember);
        }
        public override int GetHashCode()
        {
            return this.definingMember.GetHashCode();
        }
        Named CSharpParameter.IProvider.GetName(int index)
        {
            CSharpParameter cSharpParameter = this.DefiningMember.Parameters[index];
            return cSharpParameter.Name;
        }
        CSharpType CSharpParameter.IProvider.GetType(int index)
        {
            CSharpParameter cSharpParameter = this.DefiningMember.Parameters[index];
            return this.CreateTypeParameterBinder().ProcessType(cSharpParameter.Type);
        }
        bool CSharpParameter.IProvider.IsThis(int index)
        {
            CSharpParameter cSharpParameter = this.DefiningMember.Parameters[index];
            return cSharpParameter.IsThis;
        }
        bool CSharpParameter.IProvider.IsParams(int index)
        {
            CSharpParameter cSharpParameter = this.DefiningMember.Parameters[index];
            return cSharpParameter.IsParams;
        }
        bool CSharpParameter.IProvider.IsRef(int index)
        {
            CSharpParameter cSharpParameter = this.DefiningMember.Parameters[index];
            return cSharpParameter.IsRef;
        }
        bool CSharpParameter.IProvider.IsOut(int index)
        {
            CSharpParameter cSharpParameter = this.DefiningMember.Parameters[index];
            return cSharpParameter.IsOut;
        }
        CSharpDefaultParameterValue CSharpParameter.IProvider.GetDefaultValue(int index)
        {
            CSharpParameter cSharpParameter = this.DefiningMember.Parameters[index];
            return cSharpParameter.DefaultValue;
        }
        Named TypeParameterInfo.IProvider.GetName(int index)
        {
            CSharpType cSharpType = this.DefiningMember.TypeParameters[index];
            return cSharpType.Name;
        }
        bool TypeParameterInfo.IProvider.IsCovariant(int index)
        {
            CSharpType cSharpType = this.DefiningMember.TypeParameters[index];
            return cSharpType.IsCovariant;
        }
        bool TypeParameterInfo.IProvider.IsContravariant(int index)
        {
            CSharpType cSharpType = this.DefiningMember.TypeParameters[index];
            return cSharpType.IsContravariant;
        }
        IList<CSharpType> TypeParameterInfo.IProvider.GetTypeConstraints(int index)
        {
            CSharpType cSharpType = this.DefiningMember.TypeParameters[index];
            return this.CreateTypeParameterBinder().ProcessTypes(cSharpType.TypeConstraints);
        }
        bool TypeParameterInfo.IProvider.HasReferenceTypeConstraint(int index)
        {
            CSharpType cSharpType = this.DefiningMember.TypeParameters[index];
            return cSharpType.HasReferenceTypeConstraint;
        }
        bool TypeParameterInfo.IProvider.HasValueTypeConstraint(int index)
        {
            CSharpType cSharpType = this.DefiningMember.TypeParameters[index];
            return cSharpType.HasValueTypeConstraint;
        }
        bool TypeParameterInfo.IProvider.HasConstructorConstraint(int index)
        {
            CSharpType cSharpType = this.DefiningMember.TypeParameters[index];
            return cSharpType.HasConstructorConstraint;
        }
    }
    internal class MemberWithTypeArguments : CSharpMember, CSharpParameter.IProvider, TypeParameterInfo.IProvider, IEquatable<MemberWithTypeArguments>
    {
        public CSharpMember member;
        public IList<CSharpType> methodTypeArguments;
        public IList<CSharpParameter> parameters;
        public IList<CSharpType> typeParameters;
        public override CSharpType ContainingType
        {
            get
            {
                return this.member.ContainingType;
            }
        }
        public override CSharpMember DefiningMember
        {
            get
            {
                return this.member;
            }
        }
        public override CSharpType ExplicitInterfaceImplementation
        {
            get { return DefiningMember.ExplicitInterfaceImplementation; }
        }
        public override CSharpAssembly Assembly
        {
            get
            {
                return this.member.Assembly;
            }
        }
        public override Named Name
        {
            get
            {
                return this.member.Name;
            }
        }
        public override bool IsConstant
        {
            get
            {
                return this.DefiningMember.IsConstant;
            }
        }
        public override bool IsAddAccessor
        {
            get
            {
                return this.DefiningMember.IsAddAccessor;
            }
        }
        public override bool IsConstructor
        {
            get
            {
                return this.DefiningMember.IsConstructor;
            }
        }
        public override bool IsDestructor
        {
            get
            {
                return this.DefiningMember.IsDestructor;
            }
        }
        public override bool IsEnumMember
        {
            get
            {
                return this.DefiningMember.IsEnumMember;
            }
        }
        public override bool IsEvent
        {
            get
            {
                return this.DefiningMember.IsEvent;
            }
        }
        public override bool IsField
        {
            get
            {
                return this.DefiningMember.IsField;
            }
        }
        public override bool IsFixedSizeBuffer
        {
            get
            {
                return this.DefiningMember.IsFixedSizeBuffer;
            }
        }
        public override bool IsGetAccessor
        {
            get
            {
                return this.DefiningMember.IsGetAccessor;
            }
        }
        public override bool IsIndexer
        {
            get
            {
                return this.DefiningMember.IsIndexer;
            }
        }
        public override bool IsAbstract
        {
            get
            {
                return this.DefiningMember.IsAbstract;
            }
        }
        public override bool IsExtern
        {
            get
            {
                return this.DefiningMember.IsExtern;
            }
        }
        public override bool IsInternal
        {
            get
            {
                return this.DefiningMember.IsInternal;
            }
        }
        public override bool IsMethod
        {
            get
            {
                return this.DefiningMember.IsMethod;
            }
        }
        public override bool IsNew
        {
            get
            {
                return this.DefiningMember.IsNew;
            }
        }
        public override bool IsOperator
        {
            get
            {
                return this.DefiningMember.IsOperator;
            }
        }
        public override bool IsOverride
        {
            get
            {
                return this.DefiningMember.IsOverride;
            }
        }
        public override bool IsPartialImplementation
        {
            get
            {
                return this.DefiningMember.IsPartialImplementation;
            }
        }
        public override bool IsPartialSignature
        {
            get
            {
                return this.DefiningMember.IsPartialSignature;
            }
        }
        public override bool IsPrivate
        {
            get
            {
                return this.DefiningMember.IsPrivate;
            }
        }
        public override bool IsProperty
        {
            get
            {
                return this.DefiningMember.IsProperty;
            }
        }
        public override bool IsProtected
        {
            get
            {
                return this.DefiningMember.IsProtected;
            }
        }
        public override bool IsProtectedOrInternal
        {
            get
            {
                return this.DefiningMember.IsProtectedOrInternal;
            }
        }
        public override bool IsPublic
        {
            get
            {
                return this.DefiningMember.IsPublic;
            }
        }
        public override bool IsReadonly
        {
            get
            {
                return this.DefiningMember.IsReadonly;
            }
        }
        public override bool IsRemoveAccessor
        {
            get
            {
                return this.DefiningMember.IsRemoveAccessor;
            }
        }
        public override bool IsSealed
        {
            get
            {
                return this.DefiningMember.IsSealed;
            }
        }
        public override bool IsSetAccessor
        {
            get
            {
                return this.DefiningMember.IsSetAccessor;
            }
        }
        public override bool IsStatic
        {
            get
            {
                return this.DefiningMember.IsStatic;
            }
        }
        public override bool IsVirtual
        {
            get
            {
                return this.DefiningMember.IsVirtual;
            }
        }
        public override bool IsVolatile
        {
            get
            {
                return this.DefiningMember.IsVolatile;
            }
        }
        public override CSharpType ReturnType
        {
            get
            {
                return this.CreateTypeParameterBinder().ProcessType(this.member.ReturnType);
            }
        }
        public override IList<CSharpMember> Accessors
        {
            get
            {
                return this.DefiningMember.Accessors;
            }
        }
        public override IList<CSharpParameter> Parameters
        {
            get
            {
                if (this.parameters == null)
                {
                    int count = this.member.Parameters.Count;
                    CSharpParameter[] array = new CSharpParameter[count];
                    for (int i = 0; i < array.Length; i++)
                    {
                        array[i] = new CSharpParameter(this, i, this);
                    }
                    this.parameters = array;
                }
                return this.parameters;
            }
        }
        public override IList<CSharpType> TypeParameters
        {
            get
            {
                if (this.typeParameters == null)
                {
                    int count = this.member.TypeParameters.Count;
                    CSharpType[] array = new CSharpType[count];
                    for (int i = 0; i < count; i++)
                    {
                        array[i] = new TypeParameterInfo(MemberWithTypeArguments.GetMemberDefinition(this.member), i, this);
                    }
                    this.typeParameters = array;
                }
                return this.typeParameters;
            }
        }
        public override IList<CSharpType> TypeArguments
        {
            get
            {
                return this.methodTypeArguments;
            }
        }
        public MemberWithTypeArguments() { }
        internal MemberWithTypeArguments(CSharpMember member, IList<CSharpType> methodTypeArguments)
        {
            this.member = member;
            this.methodTypeArguments = methodTypeArguments;
        }
        public override bool Equals(CSharpMember other)
        {
            return this.Equals(other as MemberWithTypeArguments);
        }
        public bool Equals(MemberWithTypeArguments other)
        {
            return other != null && this.member.Equals(other.member) && CSharpType.TypesEquals(this.methodTypeArguments, other.methodTypeArguments);
        }
        public override int GetHashCode()
        {
            return this.member.GetHashCode() + CSharpType.TypesGetHashCode(this.methodTypeArguments);
        }
        private TypeParameterBinder CreateTypeParameterBinder()
        {
            return new TypeParameterBinder(this.member.TypeParameters, this.methodTypeArguments);
        }
        private static MemberDefinitionInfo GetMemberDefinition(CSharpMember member)
        {
            if (member is MemberDefinitionInfo)
            {
                return member as MemberDefinitionInfo;
            }
            return MemberWithTypeArguments.GetMemberDefinition(member.DefiningMember);
        }
        Named CSharpParameter.IProvider.GetName(int index)
        {
            CSharpParameter cSharpParameter = this.member.Parameters[index];
            return cSharpParameter.Name;
        }
        CSharpType CSharpParameter.IProvider.GetType(int index)
        {
            CSharpParameter cSharpParameter = this.member.Parameters[index];
            return this.CreateTypeParameterBinder().ProcessType(cSharpParameter.Type);
        }
        bool CSharpParameter.IProvider.IsThis(int index)
        {
            CSharpParameter cSharpParameter = this.member.Parameters[index];
            return cSharpParameter.IsThis;
        }
        bool CSharpParameter.IProvider.IsParams(int index)
        {
            CSharpParameter cSharpParameter = this.member.Parameters[index];
            return cSharpParameter.IsParams;
        }
        bool CSharpParameter.IProvider.IsRef(int index)
        {
            CSharpParameter cSharpParameter = this.member.Parameters[index];
            return cSharpParameter.IsRef;
        }
        bool CSharpParameter.IProvider.IsOut(int index)
        {
            CSharpParameter cSharpParameter = this.member.Parameters[index];
            return cSharpParameter.IsOut;
        }
        CSharpDefaultParameterValue CSharpParameter.IProvider.GetDefaultValue(int index)
        {
            CSharpParameter cSharpParameter = this.member.Parameters[index];
            return cSharpParameter.DefaultValue;
        }
        Named TypeParameterInfo.IProvider.GetName(int index)
        {
            CSharpType cSharpType = this.member.TypeParameters[index];
            return cSharpType.Name;
        }
        bool TypeParameterInfo.IProvider.IsCovariant(int index)
        {
            CSharpType cSharpType = this.member.TypeParameters[index];
            return cSharpType.IsCovariant;
        }
        bool TypeParameterInfo.IProvider.IsContravariant(int index)
        {
            CSharpType cSharpType = this.member.TypeParameters[index];
            return cSharpType.IsContravariant;
        }
        IList<CSharpType> TypeParameterInfo.IProvider.GetTypeConstraints(int index)
        {
            CSharpType cSharpType = this.member.TypeParameters[index];
            return this.CreateTypeParameterBinder().ProcessTypes(cSharpType.TypeConstraints);
        }
        bool TypeParameterInfo.IProvider.HasReferenceTypeConstraint(int index)
        {
            CSharpType cSharpType = this.member.TypeParameters[index];
            return cSharpType.HasReferenceTypeConstraint;
        }
        bool TypeParameterInfo.IProvider.HasValueTypeConstraint(int index)
        {
            CSharpType cSharpType = this.member.TypeParameters[index];
            return cSharpType.HasValueTypeConstraint;
        }
        bool TypeParameterInfo.IProvider.HasConstructorConstraint(int index)
        {
            CSharpType cSharpType = this.member.TypeParameters[index];
            return cSharpType.HasConstructorConstraint;
        }
    }
    // 访问器未找到提供，自己实现访问器
    [ANonOptimize][AInvariant]internal class MemberAccessor : CSharpMember, CSharpParameter.IProvider
    {
        private static Named SpecialName = new Named("value");

        public MemberDefinitionInfo definingMember;
        public Named _Name;
        public List<CSharpParameter> _Parameters;
        public MemberAttributes _MemberAttributes;

        public override CSharpMember DefiningMember
        {
            get
            {
                return definingMember;
            }
        }
        public override CSharpType ContainingType
        {
            get { return DefiningMember.ContainingType; }
        }
        public override CSharpAssembly Assembly
        {
            get { return ContainingType.Assembly; }
        }
        public override Named Name
        {
            get
            {
                return _Name;
            }
        }
        public override bool IsMethod
        {
            get
            {
                return true;
            }
        }
        public override bool IsGetAccessor
        {
            get
            {
                return this.HasAttribute(MemberAttributes.PropertyGetter) && !DefiningMember.IsEvent;
            }
        }
        public override bool IsSetAccessor
        {
            get
            {
                return this.HasAttribute(MemberAttributes.PropertySetter) && !DefiningMember.IsEvent;
            }
        }
        public override bool IsAddAccessor
        {
            get
            {
                return this.HasAttribute(MemberAttributes.PropertyGetter) && DefiningMember.IsEvent;
            }
        }
        public override bool IsRemoveAccessor
        {
            get
            {
                return this.HasAttribute(MemberAttributes.PropertySetter) && DefiningMember.IsEvent;
            }
        }
        public override bool IsPublic
        {
            get
            {
                return DefiningMember.IsPublic;
            }
        }
        public override bool IsInternal
        {
            get
            {
                bool result = this.HasVisibility(MemberAttributes.Internal);
                if (result)
                    return result;
                else
                    return DefiningMember.IsPrivate;
            }
        }
        public override bool IsProtected
        {
            get
            {
                bool result = this.HasVisibility(MemberAttributes.Protected);
                if (result)
                    return result;
                else
                    return DefiningMember.IsPrivate;
            }
        }
        public override bool IsProtectedOrInternal
        {
            get
            {
                bool result = this.HasVisibility(MemberAttributes.ProtectedOrInternal);
                if (result)
                    return result;
                else
                    return DefiningMember.IsPrivate;
            }
        }
        public override bool IsPrivate
        {
            get
            {
                bool result = this.HasVisibility(MemberAttributes.Private);
                if (result)
                    return result;
                else
                    return DefiningMember.IsPrivate;
            }
        }
        public override bool IsStatic
        {
            get
            {
                return DefiningMember.IsStatic;
            }
        }
        public override IList<CSharpParameter> Parameters
        {
            get
            {
                if (DefiningMember.IsIndexer)
                {
                    if (_Parameters == null)
                    {
                        _Parameters = new List<CSharpParameter>(DefiningMember.Parameters);
                        if (!IsGetAccessor)
                            _Parameters.Add(new CSharpParameter(this, _Parameters.Count, this));
                    }
                    return _Parameters;
                }
                return DefiningMember.Parameters;
            }
        }

        public MemberAccessor() { }
        [AInvariant][ANonOptimize]internal MemberAccessor(MemberDefinitionInfo member, bool get)
        {
            member._Accessors.Add(this);
            this.definingMember = member;
            this._MemberAttributes |= get ? MemberAttributes.PropertyGetter : MemberAttributes.PropertySetter;

            StringBuilder builder = new StringBuilder();
            if (member.IsEvent)
            {
                if (IsAddAccessor)
                    builder.Append("add");
                else
                    builder.Append("remove");
            }
            else
            {
                if (get)
                    builder.Append("get");
                else
                    builder.Append("set");
            }
            builder.Append('_');
            builder.Append(DefiningMember.Name.Name);
            _Name = new Named(builder.ToString());
        }

        private bool HasAttribute(MemberAttributes attribute)
        {
            return (this._MemberAttributes & attribute) == attribute;
        }
        private bool HasVisibility(MemberAttributes visibility)
        {
            return (this._MemberAttributes & MemberAttributes.VisibilityMask) == visibility;
        }
        public override int GetHashCode()
        {
            return DefiningMember.GetHashCode() & Name.Name.GetHashCode();
        }
        public override bool Equals(CSharpMember other)
        {
            if (other == null) return false;
            return DefiningMember == other.DefiningMember && Name == other.Name;
        }
        Named CSharpParameter.IProvider.GetName(int index)
        {
            return SpecialName;
        }
        CSharpType CSharpParameter.IProvider.GetType(int index)
        {
            return DefiningMember.ReturnType;
        }
        bool CSharpParameter.IProvider.IsThis(int index)
        {
            return false;
        }
        bool CSharpParameter.IProvider.IsParams(int index)
        {
            return false;
        }
        bool CSharpParameter.IProvider.IsRef(int index)
        {
            return false;
        }
        bool CSharpParameter.IProvider.IsOut(int index)
        {
            return false;
        }
        CSharpDefaultParameterValue CSharpParameter.IProvider.GetDefaultValue(int index)
        {
            return null;
        }
    }

    [AInvariant]internal struct TypeParameterData
    {
        [AInvariant]public Named Name;
        public EVariance Variance;
        public bool IsCovariant { get { return (Variance & EVariance.Covariant) != EVariance.None; } }
        public bool IsContravariant { get { return (Variance & EVariance.Contravariant) != EVariance.None; } }
        public List<CSharpType> TypeConstraints;
        public bool HasReferenceTypeConstraint;
        public bool HasValueTypeConstraint;
        public bool HasConstructorConstraint;
        public void AddConstraint(CSharpType type)
        {
            if (TypeConstraints == null)
                TypeConstraints = new List<CSharpType>();
            TypeConstraints.Add(type);
        }
    }
    public class CSharpParameter : IEquatable<CSharpParameter>
    {
        public interface IProvider
        {
            Named GetName(int index);
            CSharpType GetType(int index);
            bool IsThis(int index);
            bool IsParams(int index);
            bool IsRef(int index);
            bool IsOut(int index);
            CSharpDefaultParameterValue GetDefaultValue(int index);
        }
        internal static readonly IList<CSharpParameter> EmptyList = new CSharpParameter[0];
        public CSharpMember containingMember;
        public int position;
        public CSharpParameter.IProvider infoProvider;
        public CSharpMember ContainingMember
        {
            get
            {
                return this.containingMember;
            }
        }
        public Named Name
        {
            get
            {
                return this.infoProvider.GetName(this.position);
            }
        }
        public bool IsThis
        {
            get
            {
                return this.infoProvider.IsThis(this.position);
            }
        }
        public bool IsRef
        {
            get
            {
                return this.infoProvider.IsRef(this.position);
            }
        }
        public bool IsOut
        {
            get
            {
                return this.infoProvider.IsOut(this.position);
            }
        }
        public bool IsParams
        {
            get
            {
                return this.infoProvider.IsParams(this.position);
            }
        }
        public CSharpDefaultParameterValue DefaultValue
        {
            get
            {
                return this.infoProvider.GetDefaultValue(this.position);
            }
        }
        public CSharpType Type
        {
            get
            {
                return this.infoProvider.GetType(this.position);
            }
        }
        public int Position
        {
            get
            {
                return this.position;
            }
        }
        public CSharpParameter() { }
        internal CSharpParameter(CSharpMember containingMember, int position, CSharpParameter.IProvider infoProvider)
        {
            this.containingMember = containingMember;
            this.position = position;
            this.infoProvider = infoProvider;
        }
        public override int GetHashCode()
        {
            return this.containingMember.GetHashCode() + this.position.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return this.Equals(obj as CSharpParameter);
        }
        public bool Equals(CSharpParameter other)
        {
            return other != null && this.containingMember.Equals(other.containingMember) && this.position.Equals(other.position);
        }
        public static bool operator ==(CSharpParameter lhs, CSharpParameter rhs)
        {
            return object.Equals(lhs, rhs);
        }
        public static bool operator !=(CSharpParameter lhs, CSharpParameter rhs)
        {
            return !(lhs == rhs);
        }
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            this.AppendSignature(stringBuilder, true);
            return stringBuilder.ToString();
        }
        public void AppendSignature(StringBuilder sb, bool includeParameterName)
        {
            if (this.IsRef)
            {
                sb.Append("ref ");
            }
            if (this.IsOut)
            {
                sb.Append("out ");
            }
            if (this.IsParams)
            {
                sb.Append("params ");
            }
            if (this.IsThis)
            {
                sb.Append("this ");
            }
            sb.Append(this.Type.ToString());
            if (includeParameterName)
            {
                sb.Append(' ');
                sb.Append(this.Name);
            }
        }
        public static bool Equals(IList<CSharpParameter> p1, IList<CSharpParameter> p2)
        {
            if (p1.Count != p2.Count)
                return false;
            for (int i = 0; i < p1.Count; i++)
            {
                if (p1[i].Type != p2[i].Type
                    || p1[i].IsOut != p2[i].IsOut
                    || p1[i].IsParams != p2[i].IsParams
                    || p1[i].IsRef != p2[i].IsRef
                    || p1[i].IsThis != p2[i].IsThis)
                    return false;
            }
            return true;
        }
    }
    public class CSharpDefaultParameterValue
    {
        public string displayString;
        public string DisplayString
        {
            get
            {
                return this.displayString;
            }
        }
        public CSharpDefaultParameterValue() { }
        internal CSharpDefaultParameterValue(string displayString)
        {
            this.displayString = displayString;
        }
        public override string ToString()
        {
            return this.DisplayString;
        }
    }

    public class CSharpAttribute0
    {
        internal static CSharpAttribute0[] EmptyList = new CSharpAttribute0[0];

        public object _DefiningObject;
        public CSharpType _AttributeType;

        public CSharpType DefiningType { get { return _DefiningObject as CSharpType; } }
        public CSharpMember DefiningMember { get { return _DefiningObject as CSharpMember; } }
        public object DefiningObject { get { return _DefiningObject; } }
        public CSharpType Type { get { return _AttributeType; } }

        public CSharpAttribute0() { }
        internal CSharpAttribute0(CSharpType type, CSharpType atype)
        {
            this._DefiningObject = type;
            this._AttributeType = atype;
        }
        internal CSharpAttribute0(CSharpMember member, CSharpType atype)
        {
            this._DefiningObject = member;
            this._AttributeType = atype;
        }

        public override bool Equals(object obj)
        {
            CSharpAttribute0 att = obj as CSharpAttribute0;
            if (att != null)
            {
                return att._DefiningObject == this._DefiningObject && _AttributeType == att._AttributeType;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return _DefiningObject.GetHashCode() & _AttributeType.GetHashCode();
        }
    }
}
