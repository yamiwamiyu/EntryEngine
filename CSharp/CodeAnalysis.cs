using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using EntryBuilder.CodeAnalysis.Semantics;
using EntryBuilder.CodeAnalysis.Syntax;
using EntryEngine;
using EntryEngine.Serialize;

namespace EntryBuilder.CodeAnalysis.Syntax
{
    /*
 * 编码注意事项
 * 1. 可空类型int?，类型和?之间必须不能有空格，否则会和?:运算符冲突。正确例：int? b = d ? (int?)a : null;
 * 
 * NotSupported: 不予以支持的书写格式（搜索注释可找到）
 */
    public class ParseSourceException : Exception
    {
        const int MAX_FOCUS = 300;
        internal int SourceIndex
        {
            get;
            private set;
        }
        public string SourceText
        {
            get;
            private set;
        }
        public string ErrorFocus
        {
            get;
            private set;
        }
        private ParseSourceException(int index, string source, string message, Exception innerEx)
            : base(message, innerEx)
        {
            this.SourceIndex = index;
            this.SourceText = source;
            this.ErrorFocus = message;
        }
        public static ParseSourceException Throw(int index, string source, int pos, Exception innerEx)
        {
            if (string.IsNullOrEmpty(source))
                throw new ArgumentNullException("source");
            if (pos > source.Length || pos < 0)
                throw new IndexOutOfRangeException();
            int focus = source.Length - pos;
            focus = focus > MAX_FOCUS ? MAX_FOCUS : focus;
            string message = source.Substring(pos, focus);
            throw new ParseSourceException(index, source, message, innerEx);
        }
    }
    public class ParseFileException : Exception
    {
        public string File
        {
            get;
            private set;
        }
        public ParseFileException(string file, ParseSourceException ex)
            : base(file, ex)
        {
            this.File = file;
        }
    }

    //public class Solution
    //{
    //    public List<Project> Projects
    //    {
    //        get;
    //        private set;
    //    }

    //    public Solution()
    //    {
    //        Projects = new List<Project>();
    //    }
    //}
    public class Project
    {
        PrimitiveValue AssemblyTitle;
        PrimitiveValue AssemblyDescription;
        PrimitiveValue AssemblyCompany;
        PrimitiveValue AssemblyCopyright;
        PrimitiveValue guid;
        PrimitiveValue AssemblyVersion;
        PrimitiveValue AssemblyFileVersion;

        public string Title
        {
            get { return AssemblyTitle == null ? null : AssemblyTitle.Value.ToString(); }
            set { AssemblyTitle.Value = value; }
        }
        public string Description
        {
            get { return AssemblyDescription == null ? null : AssemblyDescription.Value.ToString(); }
            set { AssemblyDescription.Value = value; }
        }
        public string Company
        {
            get { return AssemblyCompany == null ? null : AssemblyCompany.Value.ToString(); }
            set { AssemblyCompany.Value = value; }
        }
        public string Copyright
        {
            get { return AssemblyCopyright == null ? null : AssemblyCopyright.Value.ToString(); }
            set { AssemblyCopyright.Value = value; }
        }
        public string Guid
        {
            get { return guid == null ? null : guid.Value.ToString(); }
            set { guid.Value = value; }
        }
        public string Version
        {
            get { return AssemblyVersion == null ? null : AssemblyVersion.Value.ToString(); }
            set { AssemblyVersion.Value = value; }
        }
        public string FileVersion
        {
            get { return AssemblyFileVersion == null ? null : AssemblyFileVersion.Value.ToString(); }
            set { AssemblyFileVersion.Value = value; }
        }
        public HashSet<string> Symbols
        {
            get;
            private set;
        }
        public DefineFile[] Files
        {
            get { return files; }
        }

        public Project()
        {
            Symbols = new HashSet<string>();
        }

        StringStreamReader r;
        DefineFile[] files;
        public void ParseFromFile(params string[] files)
        {
            int len = files.Length;
            string[] sources = new string[len];
            for (int i = 0; i < len; i++)
                sources[i] = File.ReadAllText(files[i]);
            try
            {
                ParseFromSource(sources);
            }
            catch (ParseSourceException ex)
            {
                throw new ParseFileException(files[ex.SourceIndex], ex);
            }
            for (int i = 0; i < len; i++)
                this.files[i].Name = files[i];
        }
        public void ParseFromSource(params string[] sources)
        {
            Parse(sources);
            // AutoParseProjectName();
            foreach (var file in files)
            {
                if (file.Attributes.Count > 0)
                {
                    var attributes = file.Attributes;
                    foreach (var item in attributes)
                    {
                        ReferenceMember rm = item.Target as ReferenceMember;
                        if (rm != null)
                        {
                            switch (rm.Name.Name)
                            {
                                case "AssemblyTitle": AssemblyTitle = (PrimitiveValue)item.Arguments[0].Expression; break;
                                case "AssemblyDescription": AssemblyDescription = (PrimitiveValue)item.Arguments[0].Expression; break;
                                case "AssemblyCompany": AssemblyCompany = (PrimitiveValue)item.Arguments[0].Expression; break;
                                case "AssemblyCopyright": AssemblyCopyright = (PrimitiveValue)item.Arguments[0].Expression; break;
                                case "Guid": guid = (PrimitiveValue)item.Arguments[0].Expression; break;
                                case "AssemblyVersion": AssemblyVersion = (PrimitiveValue)item.Arguments[0].Expression; break;
                                case "AssemblyFileVersion": AssemblyFileVersion = (PrimitiveValue)item.Arguments[0].Expression; break;
                                default: continue;
                            }
                        }
                    }
                }
            }
        }
        private void Parse(string[] sources)
        {
            int len = sources.Length;
            files = new DefineFile[len];
            for (int i = 0; i < len; i++)
            {
                r = new StringStreamReader(sources[i]);
                r.WORD_BREAK = " \t\n\r{(),:;\"";
                try
                {
                    ParseCodeFile(out files[i]);
                }
                catch (Exception ex)
                {
                    ParseSourceException.Throw(i, sources[i], r.Pos, ex);
                }
            }
            r = null;
        }
        public void AddSymbols(IEnumerable<string> symbols)
        {
            foreach (var item in symbols)
            {
                if (string.IsNullOrEmpty(item))
                    continue;
                Symbols.Add(item);
            }
        }
        public void AddSymbols(string symbolStr)
        {
            if (string.IsNullOrEmpty(symbolStr))
                return;
            AddSymbols(symbolStr.Split(';'));
        }


        class Condition : Tree<Condition>
        {
            public bool IsNo;
            public bool Result;
            public bool IsAnd;
            private bool sorted;

            protected override void OnAdded(Condition node, int index)
            {
                base.OnAdded(node, index);
                sorted = false;
                node.sorted = false;
            }
            protected override void OnRemoved(Condition node)
            {
                base.OnRemoved(node);
                sorted = false;
                node.sorted = false;
            }
            public bool CalcResult()
            {
                if (!sorted)
                    Sort();
                bool result;
                if (ChildCount == 0)
                {
                    result = this.Result;
                }
                else
                {
                    result = false;
                    bool and = false;
                    foreach (var item in Childs)
                    {
                        bool temp = item.CalcResult();
                        if (and)
                            result &= temp;
                        else
                            result |= temp;
                        and = item.IsAnd;
                    }
                }
                if (IsNo)
                    result = !result;
                return result;
            }
            public void SortAll()
            {
                if (ChildCount > 0)
                {
                    ForeachChildPriority(null, Sort);
                }
            }
            public void Sort()
            {
                if (ChildCount > 0 && !sorted)
                    this.Childs.SortOrder(Sort);
                sorted = true;
            }
            public static bool Sort(Condition c1, Condition c2)
            {
                return !c1.IsAnd;
            }
            private static void Sort(Condition c)
            {
                c.Sort();
            }
        }
        Stack<bool> pre = new Stack<bool>();

        void ParseCodeFile(out DefineFile code)
        {
            code = new DefineFile();
            while (!r.IsEnd)
            {
                ParseComment();

                // using System;
                var s1 = ParseUsingNamespace();
                if (s1 != null)
                {
                    code.UsingNamespaces.Add(s1);
                    continue;
                }

                // namespace EntryEngine
                var s2 = ParseDefineNamespace();
            NAMESPACE:
                if (s2 != null)
                {
                    ParseDefineNamespace(code, s2);
                    continue;
                }

                // [assembly: AssemblyTitle("EntryEngine")]
                if (r.IsNextSign('['))
                {
                    if (r.IsNextSign("assembly", 1) || r.IsNextSign("module", 1))
                    {
                        var s3 = ParseInvokeAttribute();
                        code.Attributes.Add(s3);
                        continue;
                    }
                }

                if (r.IsEnd)
                    break;
                s2 = new DefineNamespace();
                s2.Name = new Named("");
                goto NAMESPACE;
            }
        }
        FixedText ParseComment()
        {
            StringBuilder builder = null;
            while (!r.IsEnd)
            {
                if (r.IsNextSign("//"))
                {
                    r.EatLine();
                }
                else if (r.EatAfterSignIfIs("/*"))
                {
                    r.EatAfterSign("*/");
                }
                else if (r.EatAfterSignIfIs("#"))
                {
                    if (r.EatAfterSignIfIs("endif"))
                    {
                        pre.Pop();
                    }
                    else if (r.EatAfterSignIfIs("elif") || r.EatAfterSignIfIs("else"))
                    {
                        // 已经进入某个预编译选项，那么这个预编译选项的其它条件都将不成立
                        int count = pre.Count;
                        while (true)
                        {
                            if (r.EatAfterSignIfIs("#if"))
                                pre.Push(false);
                            else if (r.EatAfterSignIfIs("#endif"))
                            {
                                pre.Pop();
                                if (pre.Count < count)
                                    break;
                            }
                            r.EatLine();
                        }
                    }
                    else if (r.EatAfterSignIfIs("if"))
                    {
                    PRE:
                        Condition condition = new Condition();
                        int line = r.NextPosition("\n");
                        while (r.Pos < line)
                        {
                            if (r.IsNextSign("!"))
                            {
                                if (r.IsNextSign("(", 1))
                                {
                                    condition.IsNo = true;
                                    // eat the '!'
                                    r.Read();
                                    continue;
                                }
                            }

                            if (r.EatAfterSignIfIs("||"))
                                continue;

                            if (r.EatAfterSignIfIs("&&"))
                            {
                                if (condition.ChildCount > 0)
                                    condition.Last.IsAnd = true;
                                else
                                    condition.IsAnd = true;
                                continue;
                            }

                            if (r.EatAfterSignIfIs("("))
                            {
                                var child = new Condition();
                                condition.Add(child);
                                condition = child;
                                continue;
                            }

                            if (r.EatAfterSignIfIs(")"))
                            {
                                condition = condition.Parent;
                                continue;
                            }

                            Condition symbol = new Condition();
                            symbol.IsNo = r.EatAfterSignIfIs("!");
                            var content = r.Next(" \r\n|&()").Trim();
                            if (content == FixedText.PRE)
                            {
                                // FixedText
                                builder = new StringBuilder();
                            }
                            symbol.Result = Symbols.Contains(content);
                            symbol.IsAnd = r.EatAfterSignIfIs("&&");
                            r.EatAfterSignIfIs("||");
                            condition.Add(symbol);
                        }

                        if (condition.Parent != null)
                            throw new ArgumentException("预编译条件最后应该回到跟节点");

                        condition.SortAll();
                        bool result = condition.CalcResult();
                        if (result)
                        {
                            // 后面遇到elif和else都跳过，直到endif
                            pre.Push(true);
                        }
                        else
                        {
                            // 后面遇到elif和else要再次进行判断，直到result=true或endif，这之前的文字全部当作注释吃掉
                            int count = pre.Count;
                            while (true)
                            {
                                if (r.EatAfterSignIfIs("#if"))
                                    pre.Push(false);
                                else if (r.EatAfterSignIfIs("#elif"))
                                {
                                    if (pre.Count == count)
                                        goto PRE;
                                }
                                else if (r.EatAfterSignIfIs("#else"))
                                {
                                    if (pre.Count == count)
                                    {
                                        pre.Push(true);
                                        break;
                                    }
                                }
                                else if (r.EatAfterSignIfIs("#endif"))
                                {
                                    if (pre.Count > count)
                                        pre.Pop();
                                    else
                                        break;
                                }
                                r.EatLine();
                            }
                        }
                    }
                    else
                    {
                        string line = r.EatLine();
                        if (builder != null)
                            builder.AppendLine(line);
                    }
                }
                else
                {
                    break;
                }
            }
            if (builder == null)
                return null;
            else
                return new FixedText(builder.ToString());
        }
        UsingNamespace ParseUsingNamespace()
        {
            if (r.EatAfterSignIfIs("using"))
            {
                UsingNamespace obj = new UsingNamespace();
                int p = r.NextPosition("=;");
                if (r.GetChar(p) == '=')
                {
                    obj.Alias = new Named(r.NextToSignAfter("=").Trim());
                    obj.Reference = ParseReference();
                }
                else
                {
                    obj.Reference = ParseReference();
                }
                r.EatAfterSign(";");
                return obj;
            }
            return null;
        }
        List<InvokeAttribute> ParseAttributes()
        {
            List<InvokeAttribute> attributes = new List<InvokeAttribute>();
            while (true)
            {
                InvokeAttribute attribute = ParseInvokeAttribute();
                if (attribute == null)
                    break;
                attributes.Add(attribute);
            }
            return attributes;
        }
        InvokeAttribute ParseInvokeAttribute()
        {
            if (r.EatAfterSignIfIs("["))
            {
                InvokeAttribute attribute = new InvokeAttribute();
                if (r.GetChar(r.NextPosition(":(]")) == ':')
                {
                    attribute.IsAssembly = r.EatAfterSignIfIs("assembly");
                    attribute.IsModule = r.EatAfterSignIfIs("module");
                    attribute.IsReturnValue = r.EatAfterSignIfIs("return");
                    r.EatAfterSign(":");
                }
                attribute.Target = ParseReference();
                // NotSupported: 可能是","隔开的多个特性 [特性1(param), 特性2]
                // 可能是无参的特性类名
                if (!r.IsNextSign("]"))
                    attribute.Arguments = ParseMethodActualArgument();
                r.EatAfterSignIfIs("]");
                return attribute;
            }
            return null;
        }
        PrimitiveValue ParsePrimitiveValue()
        {
            bool primitive;
            object value = ParsePrimitiveValue(out primitive);
            if (primitive)
                return new PrimitiveValue(value);
            else
                return null;
        }
        object ParsePrimitiveValue(out bool isPrimitive)
        {
            isPrimitive = true;

            if (r.IsNextSign("null") && r.IsNext(" \r\n\t,;.])", 4, false))
            {
                r.EatAfterSign("null");
                return null;
            }

            if (r.EatAfterSignIfIs("@\""))
            {
                // @开头的字符串：双引号在字符串内写作两个双引号
                int skip = 0;
                while (true)
                {
                    int pos = r.NextPosition("\"", skip);
                    if (r.GetChar(pos + 1) == '"')
                        skip = pos - r.Pos + 2;
                    else
                    {
                        string str = r.ToPosition(pos);
                        // eat the right '"'
                        r.Read();
                        return str.Replace("\"\"", "\"");
                    }
                }
            }
            if (r.EatAfterSignIfIs("\""))
            {
                // 字符串：双引号需要转意\"，左斜杠则需要将两个左斜杠变回一个左斜杠
                int skip = 0;
                while (true)
                {
                    int pos = r.NextPosition("\"", skip);
                    // 转意的 '\' 为双数时，则没有对引号进行转意，而是左斜杠自身的转意
                    bool convert = false;
                    for (int i = pos - 1, p = r.Pos + skip; i >= p; i--)
                    {
                        if (r.GetChar(i) == '\\')
                            convert = !convert;
                        else
                            break;
                    }

                    if (convert)
                    {
                        skip = pos - r.Pos + 1;
                    }
                    else
                    {
                        string str = r.ToPosition(pos);
                        // eat the right '"'
                        r.Read();
                        return _SERIALIZE.CodeToString(str);
                    }
                }
            }

            if (r.EatAfterSignIfIs("\'"))
            {
                // 字符
                bool translation = false;
                if (r.IsNext("\\", 0, false))
                {
                    if (r.EatAfterSignIfIs("\\u"))
                        return (char)Convert.ToInt32(r.NextToSignAfter("'"), 16);
                    else
                    {
                        r.EatAfterSign("\\");
                        translation = true;
                    }
                }
                char result = r.Read();
                // 转译 t => \t
                if (translation)
                    result = _SERIALIZE.GetEscapeChar(result);
                // eat the right "'"
                r.Read();
                return result;
            }

            // bool
            if (r.IsNextSign("true") && r.IsNext(" \r\n\t,;.])", 4, false))
            {
                r.EatAfterSign("true");
                return true;
            }
            if (r.IsNextSign("false") && r.IsNext(" \r\n\t,;.])", 5, false))
            {
                r.EatAfterSign("false");
                return false;
            }

            // 数字
            char c = r.PeekChar;
            if (IsPrimitiveFlag(c))
            {
                //string number = r.Next("),; }]:<>!+-*/%^&|=");
                string number = r.Next("),; }]:<>!*/%^&|=").Trim();
                if (number.Contains("0x"))
                    if (c == '-')
                    {
                        // 采用minus一元运算符
                        isPrimitive = false;
                        //return -Convert.ToInt32(number, 16);
                    }
                    else
                    {
                        long value = Convert.ToInt64(number, 16);
                        if (value <= int.MaxValue && value >= int.MinValue)
                            return (int)value;
                        if (value >= int.MaxValue && value <= uint.MaxValue)
                            return (uint)value;
                        return value;
                        //return Convert.ToInt32(number, 16);
                    }
                else if (number.EndsWith("m", true, null))
                    return decimal.Parse(number.Substring(0, number.Length - 1));
                else if (number.EndsWith("f", true, null))
                    return float.Parse(number.Substring(0, number.Length - 1));
                else if (number.EndsWith("u", true, null))
                    return uint.Parse(number.Substring(0, number.Length - 1));
                else if (number.EndsWith("ul", true, null))
                    return ulong.Parse(number.Substring(0, number.Length - 2));
                else if (number.EndsWith("l", true, null))
                    return long.Parse(number.Substring(0, number.Length - 1));
                else if (number.Contains("E-") || number.Contains("e-"))
                    return double.Parse(number);
                else if (number.EndsWith("d", true, null))
                    return double.Parse(number.Substring(0, number.Length - 1));
                else if (number.Contains("."))
                    return double.Parse(number);
                else
                    return int.Parse(number);
            }

            isPrimitive = false;
            return null;
        }
        bool IsPrimitiveFlag(char c)
        {
            if (c == '.')
                return true;
            if (c >= '0' && c <= '9')
                return true;
            if (c == '-')
            {
                c = r.GetChar(r.Pos + 1);
                if (c == '.')
                    return true;
                if (c >= '0' && c <= '9')
                    return true;
            }
            return false;
        }
        DefineNamespace ParseDefineNamespace()
        {
            if (r.EatAfterSignIfIs("namespace"))
            {
                DefineNamespace obj = new DefineNamespace();
                obj.Name = new Named(r.Next("{").Trim());
                return obj;
            }
            return null;
        }
        void ParseDefineNamespace(DefineFile code, DefineNamespace obj)
        {
            obj.File = code;
            code.DefineNamespaces.Add(obj);
            // 不定义namespace时为默认namespace
            //if (r.IsNextSign('{'))
            //    r.Read();
            //else
            //    throw new ArgumentException("定义Namespace没有'{'");
            bool hasNamespace = r.EatAfterSignIfIs("{");
            while (!r.IsEnd)
            {
                ParseComment();
                if (r.IsEnd || r.IsNextSign("namespace"))
                    break;
                if (hasNamespace && r.IsNextSign('}'))
                {
                    r.Read();
                    break;
                }

                // using System;
                var s1 = ParseUsingNamespace();
                if (s1 != null)
                {
                    obj.UsingNamespaces.Add(s1);
                    continue;
                }

                // namespace EntryEngine
                var s2 = ParseDefineNamespace();
                if (s2 != null)
                {
                    ParseDefineNamespace(code, s2);
                    continue;
                }

                // class|struct|interface|enum|delegate
                var s3 = ParseDefineType();
                if (s3 != null)
                {
                    DefineType dtype = s3 as DefineType;
                    if (dtype != null)
                    {
                        dtype.Namespace = obj;
                    }
                    obj.DefineTypes.Add(s3);
                    continue;
                }
            }
        }
        DefineMember ParseDefineType()
        {
            var attributes = ParseAttributes();
            ParseComment();
            EModifier modifier = ParseModifier();
            DefineMember member;
            ParseDefineType(out member);
            member.Attributes = attributes;
            member.Modifier = modifier;
            return member;
        }
        void ParseDefineType(out DefineMember member)
        {
            string type = r.Next(r.WHITE_SPACE);
            switch (type)
            {
                case "class":
                case "struct":
                case "interface":
                    var s1 = new DefineType();
                    s1.Type = (EType)Enum.Parse(typeof(EType), type, true);
                    ParseDefineType(s1);
                    member = s1;
                    break;

                case "enum":
                    var s2 = new DefineEnum();
                    s2.Name = new Named(r.Next("{:").Trim());
                    if (r.EatAfterSignIfIs(":"))
                        s2.UnderlyingTypeName = new Named(r.NextToSign("{").Trim());
                    r.EatAfterSign("{");
                    while (true)
                    {
                        ParseComment();
                        if (r.EatAfterSignIfIs("}"))
                            break;

                        FieldEnum field = new FieldEnum();
                        field.Attributes = ParseAttributes();
                        field.Name = new Named(r.Next("=,}/").Trim());
                        if (r.EatAfterSignIfIs("="))
                            field.Value = ParseExpression();
                        r.EatAfterSignIfIs(",");
                        s2.Fields.Add(field);
                    }
                    member = s2;
                    break;

                case "delegate":
                    var s3 = new DefineDelegate();
                    s3.ReturnType = ParseReference();
                    s3.Name = new Named(r.Next(" \r\n\t<(").Trim());
                    ParseDefineGenericArgument(s3.Generic);
                    s3.Arguments = ParseMethodFormalArgument();
                    ParseConstraint(s3.Generic);
                    member = s3;
                    break;

                default: throw new NotImplementedException();
            }
            // class Name { };是可以允许有一个';'结尾的
            r.EatAfterSignIfIs(";");
        }
        EModifier ParseModifier()
        {
            EModifier modifier = EModifier.None;
            while (true)
            {
                string word = r.PeekNext(" \r\n\t");
                switch (word)
                {
                    case "private": modifier |= EModifier.Private; break;
                    case "internal": modifier |= EModifier.Internal; break;
                    case "protected": modifier |= EModifier.Protected; break;
                    case "public": modifier |= EModifier.Public; break;
                    case "abstract": modifier |= EModifier.Abstract; break;
                    case "virtual": modifier |= EModifier.Virtual; break;
                    case "override": modifier |= EModifier.Override; break;
                    case "sealed": modifier |= EModifier.Sealed; break;
                    case "static": modifier |= EModifier.Static; break;
                    case "readonly": modifier |= EModifier.Readonly; break;
                    case "const": modifier |= EModifier.Const; break;
                    case "new": modifier |= EModifier.New; break;
                    case "partial": modifier |= EModifier.Partial; break;
                    case "extern": modifier |= EModifier.Extern; break;
                    case "unsafe": modifier |= EModifier.Unsafe; break;
                    case "explicit": modifier |= EModifier.Explicit; break;
                    case "implicit": modifier |= EModifier.Implicit; break;
                    case "event": modifier |= EModifier.Event; break;
                    //case "operator": modifier |= EModifier.Operator; break;
                    default: return modifier;
                }
                r.Next(" \r\n\t");
                r.EatWhitespace();
            }
        }
        void ParseDefineType(DefineType type)
        {
            type.Name = new Named(r.Next(" :<{\r\n").Trim());
            // 泛型
            ParseDefineGenericArgument(type.Generic);
            // 继承
            if (r.EatAfterSignIfIs(":"))
            {
                while (true)
                {
                    type.Inherits.Add(ParseReference());
                    ParseComment();
                    if (!r.EatAfterSignIfIs(","))
                        break;
                }
            }
            ParseConstraint(type.Generic);

            r.EatAfterSign("{");
            while (!r.IsEnd)
            {
                ParseComment();
                if (r.IsNextSign('}'))
                {
                    r.Read();
                    break;
                }

                ParseDefineTypeMember(type);
            }
        }
        void ParseDefineTypeMember(DefineType define)
        {
            /*
             * 字段，属性，构造函数，普通函数
             * ',': 定义多个字段
             * '=': 有初始值字段
             * ';': 无初始值字段
             * '{': 属性
             * '[': 索引器
             * '(': 方法
             * '<': 泛型方法
             */
            var attributes = ParseAttributes();
            EModifier modifier = ParseModifier();

            string word = r.PeekNext(" \r\n\t");
            if (word == "class" || word == "struct" || word == "interface" ||
                word == "enum" || word == "delegate")
            {
                DefineMember member;
                ParseDefineType(out member);
                member.Attributes = attributes;
                member.Modifier = modifier;
                define.NestedType.Add(member);
            }
            else
            {
                ReferenceMember type = ParseReference();
                // 构造函数
                if (type.Name.Name == define.Name.Name && r.IsNextSign("("))
                {
                    DefineConstructor member = new DefineConstructor();
                    //member.Name = type.Name;
                    // 构造函数和类型分开使用不同的实例方便JS改名
                    member.Name = new Named(type.Name.Name);
                    member.Arguments = ParseMethodFormalArgument();
                    if (r.EatAfterSignIfIs(":"))
                        member.Base = (InvokeMethod)ParseExpression();
                    member.Body = ParseBody();

                    member.Attributes = attributes;
                    member.Modifier = modifier;
                    define.Methods.Add(member);
                }
                else
                {
                    r.EatWhitespace();
                    int position = r.NextPosition(" \r\n\t,=;{[<(");
                    int skip = position - r.Pos;
                    int explicitImplement = -1;
                    // '<'可能是显示实现泛型接口的部分
                    if (r.IsNext("<", skip))
                    {
                        int temp = r.Pos;
                        if (ParseExplicitImplement() != null)
                        {
                            explicitImplement = temp;
                            position = r.NextPosition(" \r\n\t,=;{[<(");
                            skip = position - r.Pos;
                        }
                    }
                    if (r.IsNextSign("operator ") || r.IsNext("<(", skip))
                    {
                        if (explicitImplement != -1) r.Pos = explicitImplement;

                        DefineMethod member = new DefineMethod();
                        member.ReturnType = type;
                        ParseDefineMethod(member);

                        member.Attributes = attributes;
                        member.Modifier = modifier;
                        define.Methods.Add(member);
                    }
                    else if (r.IsNext(",=;", skip))
                    {
                        DefineField member = new DefineField();
                        member.Type = type;
                        ParseDefineField(member);

                        member.Attributes = attributes;
                        member.Modifier = modifier;
                        define.Fields.Add(member);
                    }
                    else if (r.IsNext("{[", skip))
                    {
                        if (explicitImplement != -1) r.Pos = explicitImplement;

                        DefineProperty member = new DefineProperty();
                        member.Type = type;
                        ParseDefineProperty(member);

                        member.Attributes = attributes;
                        member.Modifier = modifier;
                        define.Properties.Add(member);
                    }
                    else
                        throw new NotImplementedException();
                }
            }
        }
        void ParseDefineGenericArgument(DefineGeneric generic)
        {
            ParseComment();
            if (r.EatAfterSignIfIs("<"))
            {
                while (true)
                {
                    DefineGenericArgument arg = new DefineGenericArgument();

                    arg.Attributes = ParseAttributes();

                    if (r.EatAfterSignIfIs("in "))
                        arg.Variance = EVariance.Contravariant;
                    else if (r.EatAfterSignIfIs("out "))
                        arg.Variance = EVariance.Covariant;

                    string name = r.Next(" ,>").Trim();
                    arg.Name = new Named(name);
                    generic.GenericTypes.Add(arg);

                    ParseComment();
                    if (r.EatAfterSignIfIs(">"))
                        break;
                    else
                        r.EatAfterSign(",");
                }
            }
        }
        void ParseConstraint(DefineGeneric generic)
        {
            while (true)
            {
                ParseComment();
                if (r.EatAfterSignIfIs("where"))
                {
                    Constraint constraint = new Constraint();
                    string name = r.NextToSignAfter(":").Trim();
                    constraint.Type = new ReferenceMember();
                    constraint.Type.Name = new Named(name);
                    // 继承引用的类型可能是泛型，有class, struct, new()三中特殊用法
                    while (true)
                    {
                        string peek = r.PeekNext(", \r\n\t{");
                        if (peek == "class" || peek == "struct")
                        {
                            constraint.Constraints.Add(new ReferenceMember(new Named(peek)));
                            r.NextWord();
                        }
                        else if (peek == "new()" ||
                            (peek.StartsWith("new") && r.IsNextSign("(", 3) && r.IsNextSign(")", 4)))
                        {
                            constraint.Constraints.Add(new ReferenceMember(new Named("new()")));
                            r.EatAfterSignIfIs("new");
                            r.EatAfterSignIfIs("(");
                            r.EatAfterSignIfIs(")");
                        }
                        else
                        {
                            // 解析引用类型
                            constraint.Constraints.Add(ParseReference());
                        }

                        ParseComment();
                        if (!r.EatAfterSignIfIs(","))
                            break;
                    }
                    generic.Constraints.Add(constraint);
                }
                else
                    break;
            }
        }
        void ParseDefineField(FieldLocal field)
        {
            bool first = true;
            while (true)
            {
                Field current = field;
                string name = r.Next(" \r\n\t,=;");
                if (!first)
                {
                    current = new Field();
                    field.Multiple.Add(current);
                }
                current.Name = new Named(name);
                ParseComment();
                if (r.EatAfterSignIfIs(";"))
                    break;
                if (r.EatAfterSignIfIs("="))
                    current.Value = ParseExpression();
                ParseComment();
                if (r.EatAfterSignIfIs(","))
                    first = false;
                else if (r.EatAfterSignIfIs(";")
                    // fixed | using
                    || r.IsNextSign(")"))
                    break;
            }
        }
        void ParseDefineProperty(DefineProperty property)
        {
            property.ExplicitImplement = ParseExplicitImplement();
            string name = r.Next(" \r\n\t[{");
            property.Name = new Named(name);
            property.Arguments = ParseMethodFormalArgument();
            // Accessor
            r.EatAfterSign("{");
            while (true)
            {
                ParseComment();

                Accessor accessor = new Accessor();
                accessor.Attributes = ParseAttributes();

                ParseComment();

                accessor.Modifier = ParseModifier();

                string key = r.Next("{;").Trim();
                accessor.Name = new Named(key);
                accessor.AccessorType = (EAccessor)Enum.Parse(typeof(EAccessor), key, true);
                accessor.Body = ParseBody();

                if (accessor.AccessorType == EAccessor.GET || accessor.AccessorType == EAccessor.ADD)
                    property.Getter = accessor;
                else
                    property.Setter = accessor;

                ParseComment();
                if (r.EatAfterSignIfIs("}"))
                    break;
            }
        }
        void ParseDefineMethod(DefineMethod method)
        {
            method.ExplicitImplement = ParseExplicitImplement();
            string name;
            // explicit或implicit
            if (method.ReturnType.Name.Name == "operator")
            {
                method.IsCast = true;
                // Nullable<T>: 结果返回类型只剩下Nullable导致错误
                method.ReturnType = ParseReference();
                name = string.Empty;
            }
            else
            {
                name = r.Next(" \r\n\t<(").Trim();
                if (name == "operator")
                {
                    method._IsOperator = true;
                    name = r.Next("(").Trim();
                }
            }
            method.Name = new Named(name);
            ParseDefineGenericArgument(method.Generic);
            method.Arguments = ParseMethodFormalArgument();
            ParseConstraint(method.Generic);
            method.Body = ParseBody();
        }
        ReferenceMember ParseExplicitImplement()
        {
            ReferenceMember explicitImplement = null;
            string temp = r.PeekNext("([{");
            while (temp.Contains('.'))
            {
                ReferenceMember impl = ParseReference(false);
                if (explicitImplement == null)
                    explicitImplement = impl;
                else
                {
                    impl.Target = explicitImplement;
                    explicitImplement = impl;
                }
                r.Read();
                temp = r.PeekNext("([{");
            }
            return explicitImplement;
        }
        List<FormalArgument> ParseMethodFormalArgument()
        {
            if (r.IsNextSign("("))
                r.EatAfterSign("(");
            else if (r.IsNextSign("["))
                r.EatAfterSign("[");
            else
                return null;

            List<FormalArgument> args = new List<FormalArgument>();
            while (!r.EatAfterSignIfIs(")") && !r.EatAfterSignIfIs("]"))
            {
                FormalArgument arg = new FormalArgument();
                arg.Attributes = ParseAttributes();
                if (r.EatAfterSignIfIs("ref "))
                    arg.Direction = EDirection.REF;
                else if (r.EatAfterSignIfIs("out "))
                    arg.Direction = EDirection.OUT;
                else if (r.EatAfterSignIfIs("params "))
                    arg.Direction = EDirection.PARAMS;
                else if (r.EatAfterSignIfIs("this "))
                    arg.Direction = EDirection.THIS;
                arg.Type = ParseReference();
                arg.Name = new Named(r.Next(" \r\n\t,)]").Trim());
                if (r.EatAfterSignIfIs("="))
                    arg.Value = ParseExpression();
                args.Add(arg);
                r.EatAfterSignIfIs(",");
            }
            return args;
        }
        List<ActualArgument> ParseMethodActualArgument()
        {
            bool isIndexer;
            return ParseMethodActualArgument(out isIndexer);
        }
        List<ActualArgument> ParseMethodActualArgument(out bool isIndexer)
        {
            if (r.IsNextSign("("))
            {
                r.EatAfterSign("(");
                isIndexer = false;
            }
            else if (r.IsNextSign("["))
            {
                r.EatAfterSign("[");
                isIndexer = true;
            }
            else
                throw new NotImplementedException();

            List<ActualArgument> args = new List<ActualArgument>();
            while (!r.EatAfterSignIfIs(")") && !r.EatAfterSignIfIs("]"))
            {
                ActualArgument arg = new ActualArgument();
                if (r.EatAfterSignIfIs("ref "))
                    arg.Direction = EDirection.REF;
                else if (r.EatAfterSignIfIs("out "))
                    arg.Direction = EDirection.OUT;
                arg.Expression = ParseExpression();
                args.Add(arg);
                r.EatAfterSignIfIs(",");
                ParseComment();
            }
            return args;
        }

        ReferenceMember ParseReference(bool full = true)
        {
            const string filter = ". \t\r\n,);:([]{<>?*+-*/%^&|=!";
            /*
             * 命名空间.类名<类型1, 类型2>
             * 类型后面可能跟随的字符情况
             * ' ': 定义变量 | 约束
             * ',': 多个约束 | 多个泛型 | 多个继承
             * ')': 强制类型转换 | typeof | default | sizeof
             * ';': as | is
             * '(': 调用构造函数
             * ']': 特性引用完毕
             * '[': 数组类型 | 索引器
             * '{': 类型定义
             * '<': 泛型类型
             * '>': 泛型引用完毕
             * ':': case | label
             * '?': Nullable类型 | ?:运算符
             * '*': 指针类型
             * '+' | '-': i++ | i--
             * '.': 调用其它
             */
            string name = r.Next(filter).Trim();
            if (string.IsNullOrEmpty(name))
                return null;

            //if (name == "this")
            //    return new CodeThis();
            //else if (name == "base")
            //    return new CodeBase();

            ReferenceMember reference = new ReferenceMember();
            char c = r.PeekChar;
            if (c == '?')
            {
                r.Read();
                name += "?";
            }
            else if (c == '*')
            {
                r.Read();
                name += "*";
            }
            // 有可能是小于符号 i < n
            else
            {

                if (c == '<')
                {
                    r.EatAfterSign("<");
                    do
                    {
                        reference.GenericTypes.Add(ParseReference());
                        r.EatAfterSignIfIs(",");
                    }
                    while (!r.EatAfterSignIfIs(">"));

                    //while (!r.EatAfterSignIfIs(">"))
                    //{
                    //    reference.GenericTypes.Add(ParseReference());
                    //    r.EatAfterSignIfIs(",");
                    //}
                    //// typeof(Nullable<>)
                    //if (reference.GenericTypes.Count == 0)
                    //{
                    //    reference.GenericTypes.Add(new ReferenceMember());
                    //}
                }
            }

            reference.Name = new Named(name);
            while (r.IsNextSign("[") && r.IsNextSign("]", 1))
            {
                r.EatAfterSign("[");
                r.EatAfterSign("]");
                //name += "[]";
                reference.ArrayDimension++;
            }

            if (full && r.EatAfterSignIfIs("."))
            {
                var child = ParseReference();
                child.Root.Target = reference;
                reference = child;
            }

            return reference;
        }
        SyntaxNode ParseExpression()
        {
            SyntaxNode expression = ParseExpressionFront();
            if (expression == null)
                return null;
            // !(a + 5).ToString() 或者 (int)new List<int>().Count 等一元运算符的
            WithExpressionExpression cast = null;
            if (expression is UnaryOperator || expression is Cast)
                cast = (WithExpressionExpression)expression;
            while (true)
            {
                var expression2 = ParseExpressionBack(expression);
                if (expression == expression2)
                {
                    if (cast != null && cast.Expression == null)
                    {
                        cast.Expression = expression;
                        return cast;
                    }
                    return expression;
                }
                // 整理二元运算符顺序
                if (expression2 is BinaryOperator)
                {
                    BinaryOperator o1 = (BinaryOperator)expression2;
                    BinaryOperator o2 = o1.Right as BinaryOperator;
                    if (o2 != null && (o1.Operator < o2.Operator ||
                        // 赋值号"="应该先执行右边的再执行左边的
                        // HACK: 在不允许a=b=c表达式时，对于属性的调用和赋值则可省略一部分复杂闭包
                        (o1.Operator == o2.Operator && o1.Operator < EBinaryOperator.Assign)))
                    {
                        o1.Right = o2.Left;
                        o2.Left = o1;
                        expression2 = o2;
                    }
                }
                // 整理一元运算符顺序（方法重载的特殊情况-VECTOR2.Transform()
                if (cast != null && cast.Expression != null)
                {
                    if (expression2 is InvokeMethod)
                    {
                        ((InvokeMethod)expression2).Target = cast.Expression;
                        cast.Expression = null;
                    }
                    else if (expression2 is ReferenceMember)
                    {
                        ((ReferenceMember)expression2).Target = cast.Expression;
                        cast.Expression = null;
                    }
                }
                expression = expression2;
            }
        }
        SyntaxNode ParseExpressionFront()
        {
            ParseComment();

            r.EatWhitespace();
            // 原始值
            var s2 = ParsePrimitiveValue();
            if (s2 != null) return s2;

            #region unary
            EUnaryOperator o = 0;
            if (r.EatAfterSignIfIs("++"))
                o = EUnaryOperator.Increment;
            else if (r.EatAfterSignIfIs("--"))
                o = EUnaryOperator.Decrement;
            else if (r.EatAfterSignIfIs("!"))
                o = EUnaryOperator.Not;
            else if (r.EatAfterSignIfIs("~"))
                o = EUnaryOperator.BitNot;
            else if (r.EatAfterSignIfIs("-"))
                o = EUnaryOperator.Minus;
            else if (r.EatAfterSignIfIs("+"))
                o = EUnaryOperator.Plus;
            else if (r.EatAfterSignIfIs("*"))
                o = EUnaryOperator.Dereference;
            else if (r.EatAfterSignIfIs("&"))
                o = EUnaryOperator.AddressOf;
            if (o != 0)
            {
                UnaryOperator unary = new UnaryOperator();
                unary.Operator = (EUnaryOperator)o;
                // 假设是-VECTOR2.Transform得出的结果采用一元运算符
                unary.Expression = ParseExpressionFront();
                return unary;
            }
            #endregion

            #region typeof | sizeof | default
            if (r.IsNextSign("typeof") && r.IsNextSign("(", 6))
            {
                r.EatAfterSign("typeof");
                r.EatAfterSign("(");
                TypeOf s1 = new TypeOf();
                //s1.Type.Name = node.AddReference(r.NextToSignAfter(")").Trim());
                s1.Reference = ParseReference();
                r.EatAfterSign(")");
                return s1;
            }
            if (r.IsNextSign("sizeof") && r.IsNextSign("(", 6))
            {
                r.EatAfterSign("sizeof");
                r.EatAfterSign("(");
                SizeOf s1 = new SizeOf();
                //s1.Type.Name = node.AddReference(r.NextToSignAfter(")").Trim());
                s1.Reference = ParseReference();
                r.EatAfterSign(")");
                return s1;
            }
            if (r.IsNextSign("default") && r.IsNextSign("(", 7))
            {
                r.EatAfterSign("default");
                r.EatAfterSign("(");
                DefaultValue s1 = new DefaultValue();
                //s1.Type.Name = node.AddReference(r.NextToSignAfter(")").Trim());
                s1.Reference = ParseReference();
                r.EatAfterSign(")");
                return s1;
            }
            #endregion

            #region checked | unchecked
            if (r.IsNextSign("checked") && r.IsNextSign("(", 7))
            {
                r.EatAfterSign("checked");
                r.EatAfterSign("(");
                Checked s1 = new Checked();
                //s1.Type.Name = node.AddReference(r.NextToSignAfter(")").Trim());
                s1.Expression = ParseExpression();
                r.EatAfterSign(")");
                return s1;
            }
            if (r.IsNextSign("unchecked") && r.IsNextSign("(", 9))
            {
                r.EatAfterSign("unchecked");
                r.EatAfterSign("(");
                Unchecked s1 = new Unchecked();
                //s1.Type.Name = node.AddReference(r.NextToSignAfter(")").Trim());
                s1.Expression = ParseExpression();
                r.EatAfterSign(")");
                return s1;
            }
            #endregion

            #region 括号 | Lambda表达式
            if (r.IsNextSign('('))
            {
                bool isLambda = false;
                ParseComment();
                if (r.IsNextSign(")") || r.IsNextSign("ref") || r.IsNextSign("out"))
                {
                    isLambda = true;
                }
                else
                {
                    // Lambda表达式定义关键字期间不允许插入注释
                    int p = r.NextPosition(",)&|+-*/%^<>([{}", 1);
                    if (r.GetChar(p) == ',')
                        isLambda = true;
                    else if (r.GetChar(p) == ')')
                        isLambda = r.IsNextSign("=>", p - r.Pos + 1);
                    else
                        isLambda = false;
                }

                if (isLambda)
                {
                    return ParseLambdaExpression();
                }
                else
                {
                    r.EatAfterSign("(");
                    var expression = ParseExpression();
                    r.EatAfterSign(")");
                    if (expression is ReferenceMember)
                    {
                        var expression2 = ParseExpressionFront();
                        // 可能只是(Member)
                        if (expression2 != null)
                        {
                            Cast cast = new Cast();
                            cast.Type = (ReferenceMember)expression;
                            cast.Expression = expression2;
                            return cast;
                        }
                    }
                    Parenthesized parenthesized = new Parenthesized();
                    parenthesized.Expression = expression;
                    return parenthesized;
                }
            }
            #endregion

            // 匿名方法 delegate(Type arg1) { }
            if (r.IsNextSign("delegate") && r.IsNextSign("(", 8))
            {
                DefineMethod method = new DefineMethod();
                ParseDefineMethod(method);
                return method;
            }

            // 构造函数创建对象
            if (r.EatAfterSignIfIs("new "))
            {
                New method = new New();
                // 表达式：new object(param).method()
                var referenceMethod = ParseReference();
                if (referenceMethod.ArrayDimension > 0)
                {
                    InvokeMethod invoke = null;
                    SyntaxNode target = referenceMethod;
                    for (int i = 0; i < referenceMethod.ArrayDimension; i++)
                    {
                        invoke = new InvokeMethod();
                        invoke.IsIndexer = true;
                        invoke.Target = target;
                        target = invoke;
                    }
                    referenceMethod.ArrayDimension = 0;
                    method.Method = invoke;
                }
                else
                {
                    method.Method = (InvokeMethod)ParseExpressionBack(referenceMethod);
                }
                if (r.EatAfterSignIfIs("{"))
                {
                    while (!r.EatAfterSignIfIs("}"))
                    {
                        method.Initializer.Add(ParseExpression());
                        r.EatAfterSignIfIs(",");
                    }
                }
                return method;
            }

            // 数组值 { 1, 2, 3 }
            if (r.EatAfterSignIfIs("{"))
            {
                ArrayValue array = new ArrayValue();
                ParseComment();
                while (!r.EatAfterSignIfIs("}"))
                {
                    array.Values.Add(ParseExpression());
                    r.EatAfterSignIfIs(",");
                }
                return array;
            }

            // 引用对象的定义：字段，方法
            // 可能是定义变量的Lambda表达式 item => { }
            int pos = r.Pos;
            ReferenceMember reference = ParseReference();
            if (r.IsNextSign("=>"))
            {
                r.Pos = pos;
                return ParseLambdaExpression();
            }
            else
                return reference;
        }
        SyntaxNode ParseExpressionBack(SyntaxNode front)
        {
            ParseComment();

            #region operator
            if (r.EatAfterSignIfIs("++"))
            {
                UnaryOperator unary = new UnaryOperator();
                unary.Operator = EUnaryOperator.PostIncrement;
                unary.Expression = front;
                return unary;
            }
            if (r.EatAfterSignIfIs("--"))
            {
                UnaryOperator unary = new UnaryOperator();
                unary.Operator = EUnaryOperator.PostDecrement;
                unary.Expression = front;
                return unary;
            }

            EBinaryOperator o = 0;
            if (r.EatAfterSignIfIs("&="))
                o = EBinaryOperator.AssignBitwiseAnd;
            else if (r.EatAfterSignIfIs("|="))
                o = EBinaryOperator.AssignBitwiseOr;
            else if (r.EatAfterSignIfIs("^="))
                o = EBinaryOperator.AssignExclusiveOr;
            else if (r.EatAfterSignIfIs("<<="))
                o = EBinaryOperator.AssignShiftLeft;
            else if (r.EatAfterSignIfIs(">>="))
                o = EBinaryOperator.AssignShiftRight;
            else if (r.EatAfterSignIfIs("+="))
                o = EBinaryOperator.AssignAdd;
            else if (r.EatAfterSignIfIs("-="))
                o = EBinaryOperator.AssignSubtract;
            else if (r.EatAfterSignIfIs("*="))
                o = EBinaryOperator.AssignMultiply;
            else if (r.EatAfterSignIfIs("/="))
                o = EBinaryOperator.AssignDivide;
            else if (r.EatAfterSignIfIs("%="))
                o = EBinaryOperator.AssignModulus;
            else if (r.EatAfterSignIfIs("&&"))
                o = EBinaryOperator.ConditionalAnd;
            else if (r.EatAfterSignIfIs("||"))
                o = EBinaryOperator.ConditionalOr;
            else if (r.EatAfterSignIfIs("&"))
                o = EBinaryOperator.BitwiseAnd;
            else if (r.EatAfterSignIfIs("|"))
                o = EBinaryOperator.BitwiseOr;
            else if (r.EatAfterSignIfIs("^"))
                o = EBinaryOperator.ExclusiveOr;
            else if (r.EatAfterSignIfIs("<<"))
                o = EBinaryOperator.ShiftLeft;
            else if (r.EatAfterSignIfIs(">>"))
                o = EBinaryOperator.ShiftRight;
            else if (r.EatAfterSignIfIs("??"))
                o = EBinaryOperator.NullCoalescing;
            else if (r.EatAfterSignIfIs("+"))
                o = EBinaryOperator.Addition;
            else if (r.EatAfterSignIfIs("-"))
                o = EBinaryOperator.Subtraction;
            else if (r.EatAfterSignIfIs("*"))
                o = EBinaryOperator.Multiply;
            else if (r.EatAfterSignIfIs("/"))
                o = EBinaryOperator.Division;
            else if (r.EatAfterSignIfIs("%"))
                o = EBinaryOperator.Modulus;
            else if (r.EatAfterSignIfIs("<="))
                o = EBinaryOperator.LessThanOrEqual;
            else if (r.EatAfterSignIfIs("<"))
                o = EBinaryOperator.LessThan;
            else if (r.EatAfterSignIfIs(">="))
                o = EBinaryOperator.GreaterThanOrEqual;
            else if (r.EatAfterSignIfIs(">"))
                o = EBinaryOperator.GreaterThan;
            else if (r.EatAfterSignIfIs("=="))
                o = EBinaryOperator.Equality;
            else if (r.EatAfterSignIfIs("!="))
                o = EBinaryOperator.Inequality;
            else if (r.EatAfterSignIfIs("="))
                o = EBinaryOperator.Assign;
            if (o != 0)
            {
                BinaryOperator binary = new BinaryOperator();
                binary.Left = front;
                binary.Operator = o;
                // 比较运算符时(a == b) ? 0 : 1
                // 赋值运算符时a |= (b ? 0 : 1)
                binary.Right = ParseExpression();
                if (o < EBinaryOperator.Assign && binary.Right is ConditionalOperator)
                {
                    ConditionalOperator condition = (ConditionalOperator)binary.Right;
                    var temp = condition.Condition;
                    condition.Condition = binary;
                    binary.Right = temp;
                    return condition;
                }
                return binary;
            }

            if (r.EatAfterSignIfIs("?"))
            {
                ConditionalOperator condition = new ConditionalOperator();
                condition.Condition = front;
                condition.True = ParseExpression();
                r.EatAfterSign(":");
                condition.False = ParseExpression();
                return condition;
            }
            #endregion

            // as | is | cast
            if (r.EatAfterSignIfIs("as "))
            {
                As as1 = new As();
                as1.Expression = front;
                as1.Reference = ParseReference();
                return as1;
            }
            if (r.EatAfterSignIfIs("is "))
            {
                Is as1 = new Is();
                as1.Expression = front;
                as1.Reference = ParseReference();
                return as1;
            }

            // 调用方法 | 调用索引
            Parenthesized parenthesized = front as Parenthesized;
            //bool isCast = (parenthesized != null && parenthesized.Expression is ReferenceMember);

            //if ((r.IsNextSign("(") && !isCast) || r.IsNextSign("["))
            if (r.IsNextSign("(") || r.IsNextSign("["))
            {
                InvokeMethod method = new InvokeMethod();
                method.Target = front;
                method.Arguments = ParseMethodActualArgument(out method.IsIndexer);
                return method;
            }

            // 目标引用成员：obj.Member
            if (r.EatAfterSignIfIs("."))
            {
                ReferenceMember reference = ParseReference();
                //reference.Target.Add(front);
                reference.Root.Target = front;
                return reference;
            }

            //if (isCast)
            //{
            //    var expression = ParseExpressionFront();
            //    if (expression != null)
            //    {
            //        // 强制类型转换
            //        Cast cast = new Cast();
            //        cast.Reference = (ReferenceMember)parenthesized.Expression;
            //        cast.Expression = expression;
            //        return cast;
            //    }
            //}

            return front;
        }
        Body ParseBody()
        {
            ParseComment();
            if (r.EatAfterSignIfIs(";"))
            {
                // 未实现的方法
                return null;
            }
            else
            {
                // body
                Body body = new Body();
                r.EatAfterSign("{");
                FixedText text = ParseComment();
                if (text != null)
                    body.Statements.Add(text);
                while (!r.EatAfterSignIfIs("}"))
                {
                    body.Statements.Add(ParseStatement());
                    text = ParseComment();
                    if (text != null)
                        body.Statements.Add(text);
                }
                return body;
            }
        }
        Body ParseStatementBody()
        {
            if (r.IsNextSign("{"))
            {
                return ParseBody();
            }
            else
            {
                Body body = new Body();
                body.Statements.Add(ParseStatement());
                return body;
            }
        }
        SyntaxNode ParseStatement()
        {
            /*
            * // Statement种类
            * *定义变量: Type name [= value];
            * *块: { ... }
            * *空: ;
             * 
             * 表达式
             * 赋值: name = value 包括&=,|=,++,--,+=,-=,*=,/=,%=等等
             * 调用方法: new Class() | target.Method()
            * 
            * // 关键字
            * *if
            * *switch
            * *case
            * goto
            * label
            * 
            * *for
            * *foreach
            * *while
            * *do.while
            * *continue
            * *break
            * *return
            * *yield return
            * *yield break
            * 
            * *throw
            * *try.catch.finally
            * 
            * *using
            * *fixed
            * *checked
            * *unchecked
            * *lock
            * 
            * *unsafe
            */

            ParseComment();

            #region if
            if (r.IsNextSign("if") && r.IsNextSign("(", 2))
            {
                r.EatAfterSign("if");
                IfStatement if1 = new IfStatement();
                r.EatAfterSign("(");
                if1.Condition = ParseExpression();
                r.EatAfterSign(")");
                if1.Body = ParseStatementBody();

                ParseComment();
                while (r.EatAfterSignIfIs("else"))
                {
                    if (r.IsNextSign("if"))
                    {
                        r.EatAfterSignIfIs("if");
                        // else if
                        IfStatement if2 = new IfStatement();
                        if2.IsElseIf = true;
                        r.EatAfterSign("(");
                        if2.Condition = ParseExpression();
                        r.EatAfterSign(")");
                        if2.Body = ParseStatementBody();
                        if1.Else.Add(if2);
                    }
                    else
                    {
                        // else
                        IfStatement if3 = new IfStatement();
                        if3.Body = ParseStatementBody();
                        if1.Else.Add(if3);
                        break;
                    }
                    ParseComment();
                }
                return if1;
            }
            #endregion

            #region switch
            if (r.IsNextSign("switch") && r.IsNextSign("(", 6))
            {
                r.EatAfterSign("switch");
                SwitchStatement switch1 = new SwitchStatement();
                r.EatAfterSign("(");
                switch1.Condition = ParseExpression();
                r.EatAfterSign(")");
                r.EatAfterSign("{");
                while (!r.EatAfterSignIfIs("}"))
                {
                    CaseLabel case1 = new CaseLabel();
                    while (true)
                    {
                        if (!r.EatAfterSignIfIs("default"))
                        {
                            r.EatAfterSign("case");
                            case1.Labels.Add(ParseExpression());
                        }
                        r.EatAfterSignIfIs(":");
                        if (!case1.IsDefault)
                        {
                            ParseComment();
                            if (!r.IsNextSign("case"))
                                break;
                        }
                        else
                            break;
                    }

                    while (!r.IsNextSign("case") && !r.IsNextSign("}") && !r.IsNextSign("default"))
                        case1.Statements.Add(ParseStatement());
                    switch1.Cases.Add(case1);
                }
                return switch1;
            }
            #endregion

            #region foreach
            if (r.IsNextSign("foreach") && r.IsNextSign("(", 7))
            {
                r.EatAfterSign("foreach");
                ForeachStatement foreach1 = new ForeachStatement();
                r.EatAfterSign("(");
                foreach1.Type = ParseReference();
                foreach1.Name = new Named(r.Next(" \r\n\t"));
                r.EatAfterSign("in");
                foreach1.In = ParseExpression();
                r.EatAfterSign(")");
                foreach1.Body = ParseStatementBody();
                return foreach1;
            }
            #endregion

            #region for
            if (r.IsNextSign("for") && r.IsNextSign("(", 3))
            {
                r.EatAfterSign("for");
                ForStatement for1 = new ForStatement();
                r.EatAfterSignIfIs("(");
                while (!r.EatAfterSignIfIs(";"))
                {
                    for1.Initializers.Add(ParseStatement());
                    if (!r.EatAfterSignIfIs(","))
                        break;
                }
                for1.Condition = ParseExpression();
                r.EatAfterSign(";");
                while (!r.EatAfterSignIfIs(")"))
                {
                    for1.Statements.Add(ParseStatement());
                    r.EatAfterSignIfIs(",");
                }
                for1.Body = ParseStatementBody();
                return for1;
            }
            #endregion

            #region while & do-while
            if (r.IsNextSign("while") && r.IsNextSign("(", 5))
            {
                r.EatAfterSign("while");
                WhileStatement while1 = new WhileStatement();
                r.EatAfterSignIfIs("(");
                while1.Condition = ParseExpression();
                r.EatAfterSignIfIs(")");
                while1.Body = ParseStatementBody();
                return while1;
            }
            if (r.IsNextSign("do") && r.IsNext(" \r\n\t{", 2, false))
            {
                r.EatAfterSign("do");
                DoWhileStatement dowhile1 = new DoWhileStatement();
                dowhile1.Body = ParseStatementBody();
                r.EatAfterSignIfIs("while");
                r.EatAfterSignIfIs("(");
                dowhile1.Condition = ParseExpression();
                r.EatAfterSignIfIs(")");
                r.EatAfterSignIfIs(";");
                return dowhile1;
            }
            #endregion

            #region 控制跳转语句
            if (r.IsNextSign("continue") && r.IsNextSign(";", 8))
            {
                r.EatAfterSign("continue");
                r.EatAfterSign(";");
                return new ContinueStatement();
            }
            if (r.IsNextSign("break") && r.IsNextSign(";", 5))
            {
                r.EatAfterSign("break");
                r.EatAfterSign(";");
                return new BreakStatement();
            }
            if (r.IsNextSign("return") && r.IsNext(" \r\n\t;", 6, false))
            {
                r.EatAfterSign("return");
                ReturnStatement return1 = new ReturnStatement();
                if (!r.IsNextSign(";"))
                    return1.Value = ParseExpression();
                r.EatAfterSign(";");
                return return1;
            }
            if (r.EatAfterSignIfIs("yield "))
            {
                if (r.EatAfterSignIfIs("break"))
                {
                    r.EatAfterSign(";");
                    return new YieldBreakStatement();
                }
                else
                {
                    r.EatAfterSign("return");
                    YieldReturnStatement return1 = new YieldReturnStatement();
                    return1.Value = ParseExpression();
                    r.EatAfterSign(";");
                    return return1;
                }
            }
            // 禁止使用goto语句
            if (r.EatAfterSignIfIs("goto "))
            {
                //throw new InvalidCastException("不支持goto语句");
                GotoStatement goto1 = new GotoStatement();
                goto1.HasCase = r.EatAfterSignIfIs("case");
                goto1.Value = ParseExpression();
                r.EatAfterSign(";");
                return goto1;
            }
            #endregion

            #region try catch finally
            if (r.IsNextSign("try") && r.IsNextSign("{", 3))
            {
                r.EatAfterSign("try");
                TryCatchFinallyStatement ex = new TryCatchFinallyStatement();
                ex.Try = ParseBody();
                while (r.EatAfterSignIfIs("catch"))
                {
                    CatchStatement ex1 = new CatchStatement();
                    if (r.EatAfterSignIfIs("("))
                    {
                        ex1.Type = ParseReference();
                        if (!r.EatAfterSignIfIs(")"))
                            ex1.Name = new Named(r.NextToSignAfter(")").Trim());
                    }
                    ex1.Body = ParseBody();
                    ex.Catch.Add(ex1);
                }
                if (r.EatAfterSignIfIs("finally"))
                {
                    ex.Finally = ParseBody();
                }
                return ex;
            }
            if (r.IsNextSign("throw") && r.IsNext(" \r\n\t;", 5, false))
            {
                r.EatAfterSign("throw");
                ThrowStatement throw1 = new ThrowStatement();
                if (!r.EatAfterSignIfIs(";"))
                    throw1.Value = ParseExpression();
                r.EatAfterSignIfIs(";");
                return throw1;
            }
            #endregion

            #region using fixed lock unsafe
            WithFieldStatement withField = null;
            if (r.IsNextSign("using") && r.IsNextSign("(", 5))
            {
                r.EatAfterSign("using");
                withField = new UsingStatement();
            }
            else if (r.IsNextSign("fixed") && r.IsNextSign("(", 5))
            {
                r.EatAfterSign("fixed");
                withField = new FixedStatement();
            }
            if (withField != null)
            {
                r.EatAfterSign("(");
                while (!r.EatAfterSignIfIs(")"))
                {
                    // 可以是 Expression: using(value) | Statement: using(Type obj = value)
                    if (r.PeekNextLine.IndexOf('=') == -1)
                        withField.Fields.Add(ParseReference());
                    else
                        withField.Fields.Add(ParseStatement());
                    r.EatAfterSignIfIs(",");
                }
                withField.Body = ParseStatementBody();
                return withField;
            }

            if (r.IsNextSign("lock") && r.IsNextSign("(", 4))
            {
                r.EatAfterSign("lock");
                r.EatAfterSign("(");
                LockStatement lock1 = new LockStatement();
                lock1.Refenrence = ParseReference();
                r.EatAfterSign(")");
                lock1.Body = ParseStatementBody();
                return lock1;
            }
            if (r.IsNextSign("unsafe") && r.IsNextSign("{", 6))
            {
                r.EatAfterSign("unsafe");
                UnsafeStatement unsafe1 = new UnsafeStatement();
                unsafe1.Body = ParseBody();
                return unsafe1;
            }
            if (r.IsNextSign("checked") && r.IsNextSign("{", 7))
            {
                r.EatAfterSign("checked");
                CheckedStatement checked1 = new CheckedStatement();
                checked1.Body = ParseBody();
                return checked1;
            }
            if (r.IsNextSign("unchecked") && r.IsNextSign("{", 9))
            {
                r.EatAfterSign("unchecked");
                UncheckedStatement unckecked1 = new UncheckedStatement();
                unckecked1.Body = ParseBody();
                return unckecked1;
            }
            #endregion

            if (r.EatAfterSignIfIs(";"))
                return new EmptyStatement();

            if (r.IsNextSign("{"))
                return ParseBody();

            // NotSupported: 会把new 类型()当做Modifier读掉，虽然还有volatile等，目前只能用const
            //var modifier = ParseModifier();
            EModifier modifier = EModifier.None;
            if (r.EatAfterSignIfIs("const"))
                modifier = EModifier.Const;

            var expression = ParseExpression();
            if (expression is ReferenceMember)
            {
                if (r.EatAfterSignIfIs(":"))
                {
                    LabelStatement label = new LabelStatement();
                    label.Name = ((ReferenceMember)expression).Name;
                    return label;
                }

                // 定义变量
                FieldLocal field = new FieldLocal();
                field.Modifier = modifier;  // const
                field.Type = (ReferenceMember)expression;
                ParseDefineField(field);
                return field;
            }
            else
            {
                // 表达式
                //ExpressionStatement statement = new ExpressionStatement();
                //statement.Expression = expression;
                //return statement;
                r.EatAfterSignIfIs(";");
                return expression;
            }
        }
        SyntaxNode ParseLambdaExpression()
        {
            Lambda lambda = new Lambda();

            if (r.IsNextSign("("))
            {
                // (a  )
                int p = r.NextPosition(" \r\n\t,)");
                if (r.IsNext(",)", p - r.Pos))
                {
                    // 隐式
                    r.EatAfterSignIfIs("(");
                    while (!r.EatAfterSignIfIs(")"))
                    {
                        FormalArgument argument = new FormalArgument();
                        argument.Name = new Named(r.Next(",)").Trim());
                        lambda.Parameters.Add(argument);
                        r.EatAfterSignIfIs(",");
                    }
                }
                else
                {
                    // 显示
                    lambda.Parameters = ParseMethodFormalArgument();
                }
            }
            else
            {
                //ActionRef<int> action = (ref int a) => Console.WriteLine(a);
                //Action<int> action3 = (int a) => Console.WriteLine(a);
                //Action<int, int> action4 = (a,b) => Console.WriteLine(a);
                //TestRef action2 = (ref int a, int b) => b = a;
                // 唯一一个参数，不可能有ref|out，不可能显示定义参数
                FormalArgument argument = new FormalArgument();
                argument.Name = new Named(r.Next("=").Trim());
                lambda.Parameters.Add(argument);
            }

            r.EatAfterSignIfIs("=>");

            if (r.IsNextSign("{"))
            {
                lambda.Body = ParseBody();
            }
            else
            {
                //Func<int, int> func = (a) => a;
                //new Func<int, string>((a) => a.ToString())(5);
                //new Action<int>((a) => a.ToString())(5);
                // 一句Expression：Resolver时不好区分是Expression还是Statement，调用方法时需要知道方法的返回类型是void才是Statement
                lambda.IsSingleBody = true;
                lambda.Body = new Body();
                lambda.Body.Statements.Add(ParseExpression());
            }

            return lambda;
        }
    }

    // 对象模型
    public abstract class SyntaxNode { }

    #region Define

    public class DefineFile : SyntaxNode
    {
        public string Name;
        public List<UsingNamespace> UsingNamespaces = new List<UsingNamespace>();
        public List<InvokeAttribute> Attributes = new List<InvokeAttribute>();
        public List<DefineNamespace> DefineNamespaces = new List<DefineNamespace>();
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
                return Name;
            if (DefineNamespaces.Count > 0)
                return DefineNamespaces[0].Name.Name;
            return base.ToString();
        }
    }
    public abstract class Define : SyntaxNode
    {
        public Named Name;
        public bool HasName
        {
            get { return Name != null && !string.IsNullOrEmpty(Name.Name); }
        }
    }

    /*
     * 1. Indexer: [特性] 类型 名字 [值](不起作用，写了只是警告)
     * 2. Constructor: [特性] 类型 名字 [值]
     * 3. Method: [特性] [方向] 类型 名字 [值]
     * 4. Enum: [特性] 名字 [值]
     * 5. Field: [特性] [修饰符] 类型 名字 [值] [,名字 [值]]
     * 6. Local Field: [修饰符] 类型 名字 [值] [,名字 [值]]
     */
    public class Field : Define
    {
        public SyntaxNode Value;
    }
    public class FieldEnum : Field
    {
        public List<InvokeAttribute> Attributes;
    }
    public class FormalArgument : FieldEnum
    {
        public ReferenceMember Type;
        public EDirection Direction;
    }
    public class FieldLocal : Field
    {
        public EModifier Modifier;
        public ReferenceMember Type;
        public List<Field> Multiple = new List<Field>();
        public bool IsMultiple
        {
            get { return Multiple.Count > 0; }
        }
        public bool IsAutoType
        {
            get { return Type.Name.Name == "var"; }
        }
        public virtual bool IsMember
        {
            get { return false; }
        }
    }
    public class DefineField : FieldLocal
    {
        public List<InvokeAttribute> Attributes;
        public override bool IsMember
        {
            get { return true; }
        }
    }

    public class DefineNamespace : Define
    {
        internal DefineFile File;
        public List<UsingNamespace> UsingNamespaces = new List<UsingNamespace>();
        public List<DefineNamespace> DefineNamespaces = new List<DefineNamespace>();
        public List<DefineMember> DefineTypes = new List<DefineMember>();
    }
    public abstract class DefineMember : Define
    {
        public List<InvokeAttribute> Attributes;
        public EModifier Modifier;
    }
    public class DefineGeneric
    {
        public List<DefineGenericArgument> GenericTypes = new List<DefineGenericArgument>();
        public List<Constraint> Constraints = new List<Constraint>();
        public bool IsGeneric
        {
            get { return GenericTypes.Count > 0; }
        }
    }
    [Flags]
    public enum EModifier
    {
        None = 0,
        Private = 1,
        Internal = 2,
        Protected = 4,
        Public = 8,
        //VisibilityMask = 15,
        Abstract = 16,
        Virtual = 32,
        Override = 64,
        Sealed = 128,
        Static = 256,
        Readonly = 512,
        Const = 1024,
        New = 2048,
        Partial = 4096,
        Extern = 8192,
        //Volatile = 16384,
        Unsafe = 32768,
        Explicit = 65536,
        Implicit = 131072,
        Event = 262144,
        //Operator = 524288,
        //Async = 65536,
        //Final
    }
    public abstract class CodeModifier : SyntaxNode
    {
        public EModifier Modifier;
    }
    public enum EType
    {
        CLASS,
        STRUCT,
        INTERFACE,
        //ENUM,
    }
    public class DefineType : DefineMember
    {
        internal DefineNamespace Namespace;
        public DefineGeneric Generic = new DefineGeneric();
        public List<ReferenceMember> Inherits = new List<ReferenceMember>();
        public List<DefineField> Fields = new List<DefineField>();
        public List<DefineProperty> Properties = new List<DefineProperty>();
        public List<DefineMemberMethod> Methods = new List<DefineMemberMethod>();
        public List<DefineMember> NestedType = new List<DefineMember>();
        public EType Type;
    }

    public class DefineGenericArgument : Define
    {
        public List<InvokeAttribute> Attributes;
        public EVariance Variance;
    }
    public class Constraint : SyntaxNode
    {
        public ReferenceMember Type;
        public List<ReferenceMember> Constraints = new List<ReferenceMember>();
    }
    public enum EUnderlyingType
    {
        NONE,
        BYTE,
        SBYTE,
        SHORT,
        USHORT,
        INT,
        UINT,
        LONG,
        ULONG,
    }
    public class DefineEnum : DefineMember
    {
        public List<FieldEnum> Fields = new List<FieldEnum>();
        public Named UnderlyingTypeName;
        public EUnderlyingType UnderlyingType
        {
            get
            {
                if (UnderlyingTypeName == null)
                    return EUnderlyingType.NONE;
                return (EUnderlyingType)Enum.Parse(typeof(EUnderlyingType), UnderlyingTypeName.Name, true);
            }
        }
    }
    public class DefineProperty : DefineMember
    {
        /// <summary>显式实现接口</summary>
        public ReferenceMember ExplicitImplement;
        public ReferenceMember Type;
        public Accessor Getter;
        public Accessor Setter;
        public List<FormalArgument> Arguments;
        public bool IsIndexer
        {
            get { return Arguments != null && Arguments.Count > 0; }
        }
        public bool IsAuto
        {
            get
            {
                return Getter != null && Setter != null &&
                    Getter.Body == null && Setter.Body == null;
            }
        }
    }
    public enum EAccessor
    {
        GET,
        SET,
        ADD,
        REMOVE,
    }
    public class Accessor : DefineMember
    {
        public EAccessor AccessorType;
        public Body Body;
    }
    public abstract class DefineMemberMethod : DefineMember
    {
        public List<FormalArgument> Arguments;
        public Body Body;
    }
    public class DefineConstructor : DefineMemberMethod
    {
        /// <summary>调用其它构造函数，例如this(false)</summary>
        public InvokeMethod Base;
    }
    public class DefineMethod : DefineMemberMethod
    {
        /// <summary>显式实现接口</summary>
        public ReferenceMember ExplicitImplement;
        public DefineGeneric Generic = new DefineGeneric();
        public ReferenceMember ReturnType;
        internal bool _IsOperator;
        public bool Anonymous
        {
            get { return Name == null || Name.Name == "delegate"; }
        }
        public bool HasReturnType
        {
            get { return ReturnType != null && ReturnType.Name != null && ReturnType.Name.Name != "void"; }
        }
        public bool IsOperator
        {
            get { return IsCast || _IsOperator; }
        }
        public bool IsCast
        {
            get;
            set;
        }
    }
    public class DefineDelegate : DefineMember
    {
        public List<FormalArgument> Arguments;
        public DefineGeneric Generic = new DefineGeneric();
        public ReferenceMember ReturnType;
        public bool HasReturnType
        {
            get { return ReturnType != null && ReturnType.Name != null && ReturnType.Name.Name != "void"; }
        }
    }
    public class Body : SyntaxNode
    {
        public List<SyntaxNode> Statements = new List<SyntaxNode>();
    }

    public class ActualArgument : SyntaxNode
    {
        public EDirection Direction;
        public SyntaxNode Expression;
    }

    public class FixedText : SyntaxNode
    {
        public const string PRE = "___";
        public string Text;
        public FixedText() { }
        public FixedText(string text) { this.Text = text; }
    }

    #endregion

    #region Statement

    public class IfStatement : SyntaxNode
    {
        public SyntaxNode Condition;
        public List<IfStatement> Else = new List<IfStatement>();
        public Body Body;
        public bool IsElse
        {
            get { return Condition == null; }
        }
        public bool IsElseIf
        {
            get;
            set;
        }
    }
    public class SwitchStatement : SyntaxNode
    {
        public SyntaxNode Condition;
        public List<CaseLabel> Cases = new List<CaseLabel>();
    }
    public class CaseLabel : SyntaxNode
    {
        public List<SyntaxNode> Labels = new List<SyntaxNode>();
        public Body Body = new Body();
        public List<SyntaxNode> Statements
        {
            get { return Body.Statements; }
        }
        public bool IsCase
        {
            get { return Labels.Count > 0; }
        }
        public bool IsDefault
        {
            get { return Labels.Count == 0; }
        }
    }
    public class LabelStatement : Define { }

    public class ForStatement : SyntaxNode
    {
        public List<SyntaxNode> Initializers = new List<SyntaxNode>();
        public SyntaxNode Condition;
        public List<SyntaxNode> Statements = new List<SyntaxNode>();
        public Body Body;
    }
    public class WhileStatement : SyntaxNode
    {
        public SyntaxNode Condition;
        public Body Body;
    }
    public class DoWhileStatement : WhileStatement { }
    public class ForeachStatement : Define
    {
        public ReferenceMember Type;
        public SyntaxNode In;
        public Body Body;
    }
    public class ContinueStatement : SyntaxNode { }
    public class BreakStatement : SyntaxNode { }
    public class ReturnStatement : SyntaxNode
    {
        public SyntaxNode Value;
        public bool HasValue
        {
            get { return Value != null; }
        }
    }
    public class YieldBreakStatement : SyntaxNode { }
    public class YieldReturnStatement : ReturnStatement { }
    public class GotoStatement : ReturnStatement
    {
        public bool HasCase
        {
            get;
            set;
        }
    }

    public class TryCatchFinallyStatement : SyntaxNode
    {
        public Body Try;
        public List<CatchStatement> Catch = new List<CatchStatement>();
        public Body Finally;
    }
    public class CatchStatement : Define
    {
        public ReferenceMember Type;
        public Body Body;
        public bool HasArgument
        {
            get { return this.Type != null; }
        }
    }
    public class ThrowStatement : ReturnStatement { }

    public abstract class WithBodyStatement : SyntaxNode
    {
        public Body Body;
    }
    public class UnsafeStatement : WithBodyStatement { }
    public class CheckedStatement : WithBodyStatement { }
    public class UncheckedStatement : WithBodyStatement { }
    public class LockStatement : WithBodyStatement
    {
        public ReferenceMember Refenrence;
    }
    public abstract class WithFieldStatement : WithBodyStatement
    {
        public List<SyntaxNode> Fields = new List<SyntaxNode>();
    }
    public class UsingStatement : WithFieldStatement { }
    public class FixedStatement : WithFieldStatement { }

    public class EmptyStatement : SyntaxNode { }

    //public class ExpressionStatement : CodeModel
    //{
    //    public CodeModel Expression;
    //}

    #endregion

    #region Expression

    public abstract class WithExpressionExpression : SyntaxNode
    {
        public SyntaxNode Expression;
    }

    public enum EUnaryOperator
    {
        /// <summary>!a</summary>
        Not = 1,
        /// <summary>~a</summary>
        BitNot,
        /// <summary>-a</summary>
        Minus,
        /// <summary>+a</summary>
        Plus,
        /// <summary>*a 不可重写</summary>
        Dereference,
        /// <summary>&a 不可重写</summary>
        AddressOf,
        /// <summary>++a</summary>
        Increment,
        /// <summary>--a</summary>
        Decrement,
        /// <summary>a++</summary>
        PostIncrement,
        /// <summary>a--</summary>
        PostDecrement,
    }
    public class UnaryOperator : WithExpressionExpression
    {
        public EUnaryOperator Operator;
    }
    public class Cast : WithExpressionExpression
    {
        public ReferenceMember Type;
    }
    public enum EBinaryOperator
    {
        /// <summary>left * right</summary>
        Multiply = 1,
        /// <summary>left / right</summary>
        Division = 2,
        /// <summary>left % right</summary>
        Modulus = 3,

        /// <summary>left + right</summary>
        Addition = 4,
        /// <summary>left - right</summary>
        Subtraction = 5,

        // left << right
        ShiftLeft = 6,
        /// <summary>left >> right</summary>
        ShiftRight = 7,

        // left & right
        BitwiseAnd = 8,
        /// <summary>left ^ right</summary>
        ExclusiveOr = 9,
        /// <summary>left | right</summary>
        BitwiseOr = 10,

        /// <summary>left > right</summary>
        GreaterThan,
        /// <summary>left >= right</summary>
        GreaterThanOrEqual,
        // left < right
        LessThan,
        // left <= right
        LessThanOrEqual,
        /// <summary>left == right</summary>
        Equality,
        /// <summary>left != right</summary>
        Inequality,

        // left && right
        ConditionalAnd,
        /// <summary>left || right</summary>
        ConditionalOr,

        /// <summary>left ?? right</summary>
        NullCoalescing,

        /// <summary>left = right，后面的赋值运算符-100就是其对应的计算运算符</summary>
        Assign = 100,
        /// <summary>left *= right</summary>
        AssignMultiply = 101,
        /// <summary>left /= right</summary>
        AssignDivide = 102,
        /// <summary>left %= right</summary>
        AssignModulus = 103,
        /// <summary>left += right</summary>
        AssignAdd = 104,
        /// <summary>left -= right</summary>
        AssignSubtract = 105,
        // left <<= right
        AssignShiftLeft = 106,
        /// <summary>left >>= right</summary>
        AssignShiftRight = 107,
        // left &= right
        AssignBitwiseAnd = 108,
        /// <summary>left ^= right</summary>
        AssignExclusiveOr = 109,
        /// <summary>left |= right</summary>
        AssignBitwiseOr = 110,
    }
    public class BinaryOperator : SyntaxNode
    {
        public EBinaryOperator Operator;
        public SyntaxNode Left;
        public SyntaxNode Right;
    }
    public class ConditionalOperator : SyntaxNode
    {
        public SyntaxNode Condition;
        public SyntaxNode True;
        public SyntaxNode False;
    }

    public enum ESystemPrimitiveType
    {
        REFERENCE,
        STRING,
        CHAR,
        BOOL,
        DECIMAL,
        FLOAT,
        DOUBLE,
        INT,
        UINT,
        LONG,
        ULONG,
    }
    public class PrimitiveValue : SyntaxNode
    {
        public object Value;
        public ESystemPrimitiveType Type
        {
            get
            {
                if (Value == null)
                    return ESystemPrimitiveType.REFERENCE;
                if (Value is string)
                    return ESystemPrimitiveType.STRING;
                if (Value is char)
                    return ESystemPrimitiveType.CHAR;
                if (Value is bool)
                    return ESystemPrimitiveType.BOOL;
                if (Value is float)
                    return ESystemPrimitiveType.FLOAT;
                if (Value is double)
                    return ESystemPrimitiveType.DOUBLE;
                if (Value is decimal)
                    return ESystemPrimitiveType.DECIMAL;
                if (Value is int)
                    return ESystemPrimitiveType.INT;
                if (Value is uint)
                    return ESystemPrimitiveType.UINT;
                if (Value is long)
                    return ESystemPrimitiveType.LONG;
                if (Value is ulong)
                    return ESystemPrimitiveType.ULONG;
                throw new InvalidCastException();
            }
        }
        public PrimitiveValue() { }
        public PrimitiveValue(object value) { this.Value = value; }
    }
    public class ArrayValue : SyntaxNode
    {
        public List<SyntaxNode> Values = new List<SyntaxNode>();
    }

    public class Parenthesized : WithExpressionExpression
    {
        public Parenthesized() { }
        public Parenthesized(SyntaxNode expression) { this.Expression = expression; }
    }
    /// <summary>checked和unchecked既可以是Expression，也可以是Statement，例如checked(a+b)或者checked{a*=b;}</summary>
    public class Checked : WithExpressionExpression { }
    public class Unchecked : WithExpressionExpression { }

    public class Lambda : SyntaxNode
    {
        public List<FormalArgument> Parameters = new List<FormalArgument>();
        public Body Body;
        public bool IsSingleBody
        {
            get;
            set;
        }
        public bool IsImplicitParameter
        {
            get
            {
                foreach (var item in Parameters)
                    if (item.Type != null)
                        return false;
                return true;
            }
        }
    }

    public class ReferenceMember : SyntaxNode
    {
        /// <summary>A.B.C的引用没有涵盖在名字里，所以这里Target可能是方法或其它引用：array[0].Reference或new object().Reference或List^int.Enumerator或int.MaxValue</summary>
        public SyntaxNode Target;
        public Named Name;
        public List<ReferenceMember> GenericTypes = new List<ReferenceMember>();
        public byte ArrayDimension;
        public ReferenceMember() { }
        public ReferenceMember(Named name) { this.Name = name; }
        public bool IsGeneric
        {
            get { return GenericTypes.Count > 0; }
        }
        public bool IsArray
        {
            get { return ArrayDimension > 0; }
        }
        public ReferenceMember Base
        {
            get { return Target as ReferenceMember; }
        }
        public ReferenceMember Root
        {
            get
            {
                var parent = this;
                while (true)
                {
                    if (parent.Target != null)
                    {
                        parent = parent.Base;
                        if (parent == null)
                            return null;
                    }
                    else
                        return parent;
                }
            }
        }
        public bool HasParent
        {
            get { return Target != null; }
        }
        public Stack<ReferenceMember> References
        {
            get
            {
                Stack<ReferenceMember> stack = new Stack<ReferenceMember>(4);
                var r = this;
                while (true)
                {
                    stack.Push(r);
                    if (r.Target == null)
                        break;
                    r = r.Base;
                    if (r == null)
                        //throw new InvalidCastException();
                        break;
                }
                return stack;
            }
        }
        public bool IsGenericDefinition
        {
            get { return GenericTypes.Count > 0 && GenericTypes.All(g => g == null); }
        }

        public override string ToString()
        {
            if (!IsGeneric)
                return Name.Name;
            StringBuilder builder = new StringBuilder();
            builder.Append(Name.Name);
            builder.Append("<");
            for (int i = 0, n = GenericTypes.Count - 1; i <= n; i++)
            {
                builder.Append(GenericTypes[i].ToString());
                if (i != n)
                    builder.Append(", ");
            }
            builder.Append(">");
            return builder.ToString();
        }
        public string ToAvailableName()
        {
            if (!IsGeneric)
                return Name.Name;
            StringBuilder builder = new StringBuilder();
            InternalToAvailableName(builder);
            return builder.ToString();
        }
        void InternalToAvailableName(StringBuilder builder)
        {
            builder.Append(Name.Name);
            if (IsGeneric)
            {
                for (int i = 0, n = GenericTypes.Count - 1; i <= n; i++)
                {
                    builder.Append('_');
                    GenericTypes[i].InternalToAvailableName(builder);
                }
            }
        }
        public string GetNamespaceName()
        {
            StringBuilder builder = new StringBuilder();
            Stack<string> stack = new Stack<string>();
            var parent = this;
            while (true)
            {
                stack.Push(parent.Name.Name);
                if (parent.Target != null)
                {
                    parent = parent.Base;
                    if (parent == null)
                        break;
                }
                else
                    break;
            }
            while (stack.Count > 0)
            {
                builder.Append(stack.Pop());
                if (stack.Count > 0)
                    builder.Append('.');
            }
            return builder.ToString();
        }
    }
    //public class CodeArray : ReferenceMember { }
    //public class CodeNullable : ReferenceMember { }
    //public class CodePointer : ReferenceMember { }
    //public class CodeThis : ReferenceMember { }
    //public class CodeBase : ReferenceMember { }
    public abstract class ReferenceMemberMember : SyntaxNode
    {
        public ReferenceMember Reference;
    }
    public class UsingNamespace : ReferenceMemberMember
    {
        public Named Alias;
    }
    public class TypeOf : ReferenceMemberMember { }
    public class SizeOf : ReferenceMemberMember { }
    public class DefaultValue : ReferenceMemberMember { }
    // NotSupported: 方法能指定参数名从而打乱参数顺序调用方法ForEach(null, end: null, begin: null);
    public class InvokeMethod : SyntaxNode
    {
        /// <summary>ReferenceMember: Method() | New or InvokeMethod: new Delegate(Method)() / new int[3][] | Other Expression: a++.ToString()</summary>
        public SyntaxNode Target;
        public List<ActualArgument> Arguments;
        public bool IsIndexer;

        public bool TargetIsNewDelegate
        {
            get { return Target is New && !IsIndexer; }
        }
        public bool TargetIsNewArray
        {
            get { return Target is New && IsIndexer; }
        }
    }
    // NotSupported: 特性除了构造函数的参数外，还能指定特性内可赋值的字段或属性进行指定名称赋值[Summary(note:"abc",Value=3)]
    public class InvokeAttribute : InvokeMethod
    {
        /// <summary>[assembly: Attribute]</summary>
        public bool IsAssembly
        {
            get;
            set;
        }
        /// <summary>[module: Attribute]</summary>
        public bool IsModule
        {
            get;
            set;
        }
        /// <summary>[return: Attribute]</summary>
        public bool IsReturnValue
        {
            get;
            set;
        }
    }

    public class New : SyntaxNode
    {
        /// <summary>可能是构造函数或者数组构造函数</summary>
        public InvokeMethod Method;
        public List<SyntaxNode> Initializer = new List<SyntaxNode>();

        public ReferenceMember Type
        {
            get
            {
                InvokeMethod method = Method;
                // todo: 应该不会有两层
                while (!(method.Target is ReferenceMember))
                    method = (InvokeMethod)method.Target;
                return (ReferenceMember)method.Target;
            }
        }
        public bool IsNewArray
        {
            get { return Method.IsIndexer; }
        }
    }

    public abstract class ReferenceChange : ReferenceMemberMember
    {
        public SyntaxNode Expression;
    }
    public class As : ReferenceChange { }
    public class Is : ReferenceChange { }

    #endregion

    public abstract class SyntaxVisitor
    {
        static Type[] VisitType = new Type[1];
        static object[] VisitArgument = new object[1];

        Dictionary<Type, MethodInfo> methods = new Dictionary<Type, MethodInfo>();

        public virtual void Visit(SyntaxNode model)
        {
            if (model == null)
                return;
            //Type type = model.GetType();
            //MethodInfo info;
            //if (!methods.TryGetValue(type, out info))
            //{
            //    VisitType[0] = type;
            //    info = this.GetType().GetMethod("Visit", VisitType);
            //    methods.Add(type, info);
            //}
            //VisitArgument[0] = model;
            //info.Invoke(this, VisitArgument);

            if (model is DefineFile) Visit((DefineFile)model);
            else if (model is DefineNamespace) Visit((DefineNamespace)model);
            else if (model is DefineType) Visit((DefineType)model);
            else if (model is Constraint) Visit((Constraint)model);
            else if (model is DefineEnum) Visit((DefineEnum)model);
            else if (model is DefineField) Visit((DefineField)model);
            else if (model is DefineProperty) Visit((DefineProperty)model);
            else if (model is Accessor) Visit((Accessor)model);
            else if (model is DefineConstructor) Visit((DefineConstructor)model);
            else if (model is DefineMethod) Visit((DefineMethod)model);
            else if (model is DefineDelegate) Visit((DefineDelegate)model);
            else if (model is Body) Visit((Body)model);
            else if (model is FieldLocal) Visit((FieldLocal)model);
            else if (model is Field) Visit((Field)model);
            else if (model is FixedText) Visit((FixedText)model);

            else if (model is IfStatement) Visit((IfStatement)model);
            else if (model is SwitchStatement) Visit((SwitchStatement)model);
            else if (model is CaseLabel) Visit((CaseLabel)model);
            else if (model is LabelStatement) Visit((LabelStatement)model);
            else if (model is ForStatement) Visit((ForStatement)model);
            else if (model is DoWhileStatement) Visit((DoWhileStatement)model);
            else if (model is WhileStatement) Visit((WhileStatement)model);
            else if (model is ForeachStatement) Visit((ForeachStatement)model);
            else if (model is ContinueStatement) Visit((ContinueStatement)model);
            else if (model is YieldBreakStatement) Visit((YieldBreakStatement)model);
            else if (model is YieldReturnStatement) Visit((YieldReturnStatement)model);
            else if (model is BreakStatement) Visit((BreakStatement)model);
            else if (model is ThrowStatement) Visit((ThrowStatement)model);
            else if (model is ReturnStatement) Visit((ReturnStatement)model);
            else if (model is GotoStatement) Visit((GotoStatement)model);
            else if (model is TryCatchFinallyStatement) Visit((TryCatchFinallyStatement)model);
            else if (model is UnsafeStatement) Visit((UnsafeStatement)model);
            else if (model is CheckedStatement) Visit((CheckedStatement)model);
            else if (model is UncheckedStatement) Visit((UncheckedStatement)model);
            else if (model is LockStatement) Visit((LockStatement)model);
            else if (model is UsingStatement) Visit((UsingStatement)model);
            else if (model is FixedStatement) Visit((FixedStatement)model);
            else if (model is EmptyStatement) Visit((EmptyStatement)model);

            else if (model is UnaryOperator) Visit((UnaryOperator)model);
            else if (model is BinaryOperator) Visit((BinaryOperator)model);
            else if (model is ConditionalOperator) Visit((ConditionalOperator)model);
            else if (model is PrimitiveValue) Visit((PrimitiveValue)model);
            else if (model is ArrayValue) Visit((ArrayValue)model);
            else if (model is Parenthesized) Visit((Parenthesized)model);
            else if (model is Checked) Visit((Checked)model);
            else if (model is Unchecked) Visit((Unchecked)model);
            else if (model is Lambda) Visit((Lambda)model);
            else if (model is ReferenceMember) Visit((ReferenceMember)model);
            else if (model is UsingNamespace) Visit((UsingNamespace)model);
            else if (model is TypeOf) Visit((TypeOf)model);
            else if (model is SizeOf) Visit((SizeOf)model);
            else if (model is DefaultValue) Visit((DefaultValue)model);
            else if (model is InvokeMethod) Visit((InvokeMethod)model);
            else if (model is New) Visit((New)model);
            else if (model is As) Visit((As)model);
            else if (model is Is) Visit((Is)model);
            else if (model is Cast) Visit((Cast)model);
        }

        public virtual void Visit(DefineFile node) { }
        public virtual void Visit(DefineNamespace node) { }
        public virtual void Visit(DefineType node) { }
        public virtual void Visit(List<DefineGenericArgument> node) { }
        public virtual void Visit(Constraint node) { }
        public virtual void Visit(DefineEnum node) { }
        public virtual void Visit(DefineField node) { }
        public virtual void Visit(DefineProperty node) { }
        public virtual void Visit(Accessor node) { }
        public virtual void Visit(DefineConstructor node) { }
        public virtual void Visit(DefineMethod node) { }
        public virtual void Visit(DefineDelegate node) { }
        public virtual void Visit(List<FormalArgument> node) { }
        public virtual void Visit(Body node) { }
        public virtual void Visit(List<ActualArgument> node) { }
        public virtual void Visit(FieldLocal node) { }
        public virtual void Visit(Field node) { }
        public virtual void Visit(FixedText node) { }

        public virtual void Visit(IfStatement node) { }
        public virtual void Visit(SwitchStatement node) { }
        public virtual void Visit(CaseLabel node) { }
        public virtual void Visit(LabelStatement node) { }
        public virtual void Visit(ForStatement node) { }
        public virtual void Visit(WhileStatement node) { }
        public virtual void Visit(DoWhileStatement node) { }
        public virtual void Visit(ForeachStatement node) { }
        public virtual void Visit(ContinueStatement node) { }
        public virtual void Visit(BreakStatement node) { }
        public virtual void Visit(ReturnStatement node) { }
        public virtual void Visit(YieldBreakStatement node) { }
        public virtual void Visit(YieldReturnStatement node) { }
        public virtual void Visit(GotoStatement node) { }
        public virtual void Visit(TryCatchFinallyStatement node) { }
        public virtual void Visit(List<CatchStatement> node) { }
        public virtual void Visit(ThrowStatement node) { }
        public virtual void Visit(UnsafeStatement node) { }
        public virtual void Visit(CheckedStatement node) { }
        public virtual void Visit(UncheckedStatement node) { }
        public virtual void Visit(LockStatement node) { }
        public virtual void Visit(UsingStatement node) { }
        public virtual void Visit(FixedStatement node) { }
        public virtual void Visit(EmptyStatement node) { }

        public virtual void Visit(UnaryOperator node) { }
        public virtual void Visit(BinaryOperator node) { }
        public virtual void Visit(ConditionalOperator node) { }
        public virtual void Visit(PrimitiveValue node) { }
        public virtual void Visit(ArrayValue node) { }
        public virtual void Visit(Parenthesized node) { }
        public virtual void Visit(Checked node) { }
        public virtual void Visit(Unchecked node) { }
        public virtual void Visit(Lambda node) { }
        public virtual void Visit(ReferenceMember node) { }
        //public virtual void Visit(CodeThis node) { }
        //public virtual void Visit(CodeBase node) { }
        //public virtual void Visit(CodeNullable node) { }
        //public virtual void Visit(CodePointer node) { }
        public virtual void Visit(UsingNamespace node) { }
        public virtual void Visit(TypeOf node) { }
        public virtual void Visit(SizeOf node) { }
        public virtual void Visit(DefaultValue node) { }
        public virtual void Visit(InvokeMethod node) { }
        public virtual void Visit(List<InvokeAttribute> node) { }
        public virtual void Visit(New node) { }
        public virtual void Visit(As node) { }
        public virtual void Visit(Is node) { }
        public virtual void Visit(Cast node) { }
    }
    public abstract class SyntaxWalker : SyntaxVisitor
    {
        public override void Visit(DefineFile node)
        {
            foreach (var item in node.UsingNamespaces) Visit(item);
            Visit(node.Attributes);
            foreach (var item in node.DefineNamespaces) Visit(item);
        }
        public override void Visit(DefineNamespace node)
        {
            foreach (var item in node.UsingNamespaces) Visit(item);
            foreach (var item in node.DefineTypes) Visit(item);
            foreach (var item in node.DefineNamespaces) Visit(item);
        }
        public override void Visit(List<DefineGenericArgument> node)
        {
            foreach (var item in node)
                Visit(item.Attributes);
        }
        public override void Visit(Constraint node)
        {
            Visit(node.Type);
            foreach (var item in node.Constraints) Visit(item);
        }
        public override void Visit(DefineType node)
        {
            Visit(node.Attributes);
            if (node.Generic.IsGeneric)
                Visit(node.Generic.GenericTypes);
            foreach (var item in node.Inherits) Visit(item);
            if (node.Generic.IsGeneric && node.Generic.Constraints.Count > 0)
                foreach (var item in node.Generic.Constraints) Visit(item);
            foreach (var item in node.Fields) Visit(item);
            foreach (var item in node.Properties) Visit(item);
            foreach (var item in node.Methods) Visit(item);
            foreach (var item in node.NestedType) Visit(item);
        }
        public override void Visit(DefineEnum node)
        {
            Visit(node.Attributes);
            foreach (var item in node.Fields)
            {
                Visit(item.Attributes);
                Visit(item);
            }
        }
        public override void Visit(DefineField node)
        {
            Visit(node.Attributes);
            Visit((FieldLocal)node);
        }
        public override void Visit(DefineProperty node)
        {
            Visit(node.Attributes);
            Visit(node.Type);
            if (node.IsIndexer)
                Visit(node.Arguments);
            if (node.Getter != null) Visit(node.Getter);
            if (node.Setter != null) Visit(node.Setter);
        }
        public override void Visit(Accessor node)
        {
            Visit(node.Attributes);
            Visit(node.Body);
        }
        public override void Visit(DefineConstructor node)
        {
            Visit(node.Attributes);
            Visit(node.Arguments);
            if (node.Base != null)
                Visit(node.Base);
            Visit(node.Body);
        }
        public override void Visit(DefineMethod node)
        {
            Visit(node.Attributes);
            Visit(node.ReturnType);
            if (node.ExplicitImplement != null)
                Visit(node.ExplicitImplement);
            if (node.Generic.IsGeneric)
                Visit(node.Generic.GenericTypes);
            Visit(node.Arguments);
            if (node.Generic.IsGeneric && node.Generic.Constraints.Count > 0)
                foreach (var item in node.Generic.Constraints) Visit(item);
            Visit(node.Body);
        }
        public override void Visit(DefineDelegate node)
        {
            Visit(node.Attributes);
            Visit(node.ReturnType);
            if (node.Generic.IsGeneric)
                Visit(node.Generic.GenericTypes);
            Visit(node.Arguments);
            if (node.Generic.IsGeneric && node.Generic.Constraints.Count > 0)
                foreach (var item in node.Generic.Constraints) Visit(item);
        }
        public override void Visit(List<FormalArgument> node)
        {
            foreach (var item in node)
            {
                Visit(item.Attributes);
                if (item.Type != null) Visit(item.Type);
                Visit(item);
            }
        }
        public override void Visit(Body node)
        {
            if (node != null)
            {
                foreach (var item in node.Statements)
                    Visit(item);
            }
        }
        public override void Visit(List<ActualArgument> node)
        {
            if (node == null)
                return;
            foreach (var item in node) Visit(item.Expression);
        }
        public override void Visit(Field node)
        {
            if (node.Value != null) Visit(node.Value);
        }

        public override void Visit(FieldLocal node)
        {
            Visit(node.Type);
            Visit((Field)node);
            if (node.IsMultiple)
                foreach (var item in node.Multiple)
                    Visit(item);
        }
        public override void Visit(IfStatement node)
        {
            if (!node.IsElse)
                Visit(node.Condition);
            Visit(node.Body);
            foreach (var item in node.Else) Visit(item);
        }
        public override void Visit(SwitchStatement node)
        {
            Visit(node.Condition);
            foreach (var item in node.Cases) Visit(item);
        }
        public override void Visit(CaseLabel node)
        {
            foreach (var item in node.Labels) Visit(item);
            foreach (var item in node.Statements) Visit(item);
        }
        public override void Visit(LabelStatement node)
        {
        }
        public override void Visit(ForStatement node)
        {
            foreach (var item in node.Initializers) Visit(item);
            Visit(node.Condition);
            foreach (var item in node.Statements) Visit(item);
            Visit(node.Body);
        }
        public override void Visit(WhileStatement node)
        {
            Visit(node.Condition);
            Visit(node.Body);
        }
        public override void Visit(DoWhileStatement node)
        {
            Visit(node.Body);
            Visit(node.Condition);
        }
        public override void Visit(ForeachStatement node)
        {
            Visit(node.Type);
            Visit(node.In);
            Visit(node.Body);
        }
        public override void Visit(ContinueStatement node)
        {
        }
        public override void Visit(BreakStatement node)
        {
        }
        public override void Visit(ReturnStatement node)
        {
            if (node.Value != null)
                Visit(node.Value);
        }
        public override void Visit(YieldBreakStatement node)
        {
        }
        public override void Visit(YieldReturnStatement node)
        {
            Visit((ReturnStatement)node);
        }
        public override void Visit(GotoStatement node)
        {
            Visit(node.Value);
        }
        public override void Visit(TryCatchFinallyStatement node)
        {
            Visit(node.Try);
            Visit(node.Catch);
            if (node.Finally != null)
            {
                Visit(node.Finally);
            }
        }
        public override void Visit(List<CatchStatement> node)
        {
            foreach (var item in node)
            {
                if (item.HasArgument)
                    Visit(item.Type);
                Visit(item.Body);
            }
        }
        public override void Visit(ThrowStatement node)
        {
            if (node.Value != null)
                Visit(node.Value);
        }
        public override void Visit(UnsafeStatement node)
        {
            Visit(node.Body);
        }
        public override void Visit(CheckedStatement node)
        {
            Visit(node.Body);
        }
        public override void Visit(UncheckedStatement node)
        {
            Visit(node.Body);
        }
        public override void Visit(LockStatement node)
        {
            Visit(node.Refenrence);
            Visit(node.Body);
        }
        public override void Visit(UsingStatement node)
        {
            foreach (var item in node.Fields) Visit(item);
            Visit(node.Body);
        }
        public override void Visit(FixedStatement node)
        {
            foreach (var item in node.Fields) Visit(item);
            Visit(node.Body);
        }
        public override void Visit(EmptyStatement node)
        {
        }

        public override void Visit(UnaryOperator node)
        {
            Visit(node.Expression);
        }
        public override void Visit(BinaryOperator node)
        {
            Visit(node.Left);
            Visit(node.Right);
        }
        public override void Visit(ConditionalOperator node)
        {
            Visit(node.Condition);
            Visit(node.True);
            Visit(node.False);
        }
        public override void Visit(PrimitiveValue node)
        {
        }
        public override void Visit(ArrayValue node)
        {
            foreach (var item in node.Values) Visit(item);
        }
        public override void Visit(Parenthesized node)
        {
            Visit(node.Expression);
        }
        public override void Visit(Checked node)
        {
            Visit(node.Expression);
        }
        public override void Visit(Unchecked node)
        {
            Visit(node.Expression);
        }
        public override void Visit(Lambda node)
        {
            Visit(node.Parameters);
            Visit(node.Body);
        }
        public override void Visit(ReferenceMember node)
        {
            //foreach (var item in node.Target) Visit(item);
            if (node.Target != null) Visit(node.Target);
            foreach (var item in node.GenericTypes) Visit(item);
        }
        public override void Visit(UsingNamespace node)
        {
            Visit(node.Reference);
        }
        public override void Visit(TypeOf node)
        {
            Visit(node.Reference);
        }
        public override void Visit(SizeOf node)
        {
            Visit(node.Reference);
        }
        public override void Visit(DefaultValue node)
        {
            Visit(node.Reference);
        }
        public override void Visit(InvokeMethod node)
        {
            Visit(node.Target);
            Visit(node.Arguments);
        }
        public override void Visit(List<InvokeAttribute> node)
        {
            if (node == null)
                return;
            foreach (var item in node)
            {
                if (item.Arguments == null || item.Arguments.Count == 0)
                    Visit(item.Target);
                else
                    Visit((InvokeMethod)item);
            }
        }
        public override void Visit(New node)
        {
            Visit(node.Method);
            if (node.Initializer != null && node.Initializer.Count > 0)
                foreach (var item in node.Initializer) Visit(item);
        }
        public override void Visit(As node)
        {
            Visit(node.Expression);
            Visit(node.Reference);
        }
        public override void Visit(Is node)
        {
            Visit(node.Expression);
            Visit(node.Reference);
        }
        public override void Visit(Cast node)
        {
            Visit(node.Type);
            Visit(node.Expression);
        }
    }
    public abstract class CodeBuilder : SyntaxVisitor
    {
        protected static EModifier[] Modifier;
        public static Dictionary<EUnaryOperator, string> UOP = new Dictionary<EUnaryOperator, string>();
        public static Dictionary<EBinaryOperator, string> BOP = new Dictionary<EBinaryOperator, string>();

        static CodeBuilder()
        {
            Array array = Enum.GetValues(typeof(EModifier));
            Modifier = new EModifier[array.Length - 1];
            for (int i = 1, n = array.Length; i < n; i++)
                Modifier[i - 1] = (EModifier)array.GetValue(i);

            UOP[EUnaryOperator.Not] = "!";
            UOP[EUnaryOperator.BitNot] = "~";
            UOP[EUnaryOperator.Minus] = "-";
            UOP[EUnaryOperator.Plus] = "+";
            UOP[EUnaryOperator.Increment] = "++";
            UOP[EUnaryOperator.Decrement] = "--";
            UOP[EUnaryOperator.PostIncrement] = "++";
            UOP[EUnaryOperator.PostDecrement] = "--";
            UOP[EUnaryOperator.Dereference] = "*";
            UOP[EUnaryOperator.AddressOf] = "&";

            BOP[EBinaryOperator.BitwiseAnd] = "&";
            BOP[EBinaryOperator.BitwiseOr] = "|";
            BOP[EBinaryOperator.ExclusiveOr] = "^";
            BOP[EBinaryOperator.ConditionalAnd] = "&&";
            BOP[EBinaryOperator.ConditionalOr] = "||";
            BOP[EBinaryOperator.GreaterThan] = ">";
            BOP[EBinaryOperator.GreaterThanOrEqual] = ">=";
            BOP[EBinaryOperator.Equality] = "==";
            BOP[EBinaryOperator.Inequality] = "!=";
            BOP[EBinaryOperator.LessThan] = "<";
            BOP[EBinaryOperator.LessThanOrEqual] = "<=";
            BOP[EBinaryOperator.Addition] = "+";
            BOP[EBinaryOperator.Subtraction] = "-";
            BOP[EBinaryOperator.Multiply] = "*";
            BOP[EBinaryOperator.Division] = "/";
            BOP[EBinaryOperator.Modulus] = "%";
            BOP[EBinaryOperator.ShiftLeft] = "<<";
            BOP[EBinaryOperator.ShiftRight] = ">>";
            BOP[EBinaryOperator.NullCoalescing] = "??";
            BOP[EBinaryOperator.Assign] = "=";
            BOP[EBinaryOperator.AssignAdd] = "+=";
            BOP[EBinaryOperator.AssignSubtract] = "-=";
            BOP[EBinaryOperator.AssignMultiply] = "*=";
            BOP[EBinaryOperator.AssignDivide] = "/=";
            BOP[EBinaryOperator.AssignModulus] = "%=";
            BOP[EBinaryOperator.AssignShiftLeft] = "<<=";
            BOP[EBinaryOperator.AssignShiftRight] = ">>=";
            BOP[EBinaryOperator.AssignBitwiseAnd] = "&=";
            BOP[EBinaryOperator.AssignBitwiseOr] = "|=";
            BOP[EBinaryOperator.AssignExclusiveOr] = "^=";
        }
        public static EBinaryOperator FindBinaryOperator(string name)
        {
            foreach (var item in BOP)
                if (item.Value == name)
                    return item.Key;
            return 0;
        }
        public static EUnaryOperator FindUnaryOperator(string name)
        {
            foreach (var item in UOP)
                if (item.Value == name)
                    return item.Key;
            return 0;
        }

        protected StringBuilder builder = new StringBuilder();

        public virtual string Result
        {
            get { return builder.ToString(); }
        }

        protected void WriteSplitByComma<T>(List<T> nodes) where T : SyntaxNode
        {
            for (int i = 0, n = nodes.Count - 1; i <= n; i++)
            {
                Visit(nodes[i]);
                if (i != n)
                    builder.Append(",");
            }
        }
        protected void WriteTypes(List<ReferenceMember> types)
        {
            for (int i = 0, n = types.Count - 1; i <= n; i++)
            {
                Visit(types[i]);
                if (i != n)
                    builder.Append(",");
            }
        }
        protected void VisitStatement(SyntaxNode statement)
        {
            Visit(statement);
            if (IsNeedEndStatement(statement))
                builder.AppendLine(";");
        }
        protected virtual bool IsNeedEndStatement(SyntaxNode statement)
        {
            return statement is FieldLocal ||
                statement is New ||
                statement is InvokeMethod ||
                (statement is BinaryOperator && ((BinaryOperator)statement).Operator >= EBinaryOperator.Assign) ||
                (statement is UnaryOperator && ((UnaryOperator)statement).Operator >= EUnaryOperator.Increment);
        }

        public virtual void Visit(EModifier node) { }
    }
    public class CSharpCodeBuilder : CodeBuilder
    {
        public override void Visit(DefineFile node)
        {
            foreach (var item in node.UsingNamespaces) Visit(item);
            Visit(node.Attributes);
            foreach (var item in node.DefineNamespaces) Visit(item);
        }
        public override void Visit(DefineNamespace node)
        {
            bool hasName = !string.IsNullOrEmpty(node.Name.Name);
            if (hasName)
            {
                builder.AppendLine("namespace {0}", node.Name);
                builder.AppendLine("{");
            }
            foreach (var item in node.UsingNamespaces) Visit(item);
            foreach (var item in node.DefineTypes) Visit(item);
            foreach (var item in node.DefineNamespaces) Visit(item);
            if (hasName)
            {
                builder.AppendLine("}");
            }
        }
        public override void Visit(List<DefineGenericArgument> node)
        {
            builder.Append("<");
            bool notFirst = false;
            foreach (var item in node)
            {
                if (notFirst)
                    builder.Append(", ");

                Visit(item.Attributes);

                if (item.Variance == EVariance.Covariant)
                    builder.Append("out ");
                else if (item.Variance == EVariance.Contravariant)
                    builder.Append("in ");

                builder.Append(item.Name);
                notFirst = true;
            }
            builder.Append(">");
        }
        public override void Visit(Constraint node)
        {
            builder.Append(" where ");
            Visit(node.Type);
            builder.Append(" : ");
            WriteTypes(node.Constraints);
        }
        public override void Visit(DefineType node)
        {
            Visit(node.Attributes);
            Visit(node.Modifier);
            switch (node.Type)
            {
                case EType.CLASS: builder.Append("class"); break;
                case EType.STRUCT: builder.Append("struct"); break;
                case EType.INTERFACE: builder.Append("interface"); break;
            }
            builder.Append(" {0}", node.Name);
            if (node.Generic.IsGeneric)
                Visit(node.Generic.GenericTypes);
            if (node.Inherits.Count > 0)
            {
                builder.Append(" : ");
                for (int i = 0, n = node.Inherits.Count - 1; i <= n; i++)
                {
                    Visit(node.Inherits[i]);
                    if (i != n)
                        builder.Append(", ");
                }
            }
            if (node.Generic.IsGeneric && node.Generic.Constraints.Count > 0)
                foreach (var item in node.Generic.Constraints) Visit(item);
            builder.AppendLine();
            builder.AppendLine("{");
            foreach (var item in node.Fields) Visit(item);
            foreach (var item in node.Properties) Visit(item);
            foreach (var item in node.Methods) Visit(item);
            foreach (var item in node.NestedType) Visit(item);
            builder.AppendLine("}");
        }
        public override void Visit(DefineEnum node)
        {
            Visit(node.Attributes);
            Visit(node.Modifier);
            builder.Append("enum {0}", node.Name);
            if (node.UnderlyingType != EUnderlyingType.NONE)
                builder.Append(" : {0}", node.UnderlyingType.ToString().ToLower());
            builder.AppendLine();
            builder.AppendLine("{");
            foreach (var item in node.Fields)
            {
                Visit(item.Attributes);
                Visit(item);
                builder.AppendLine(",");
            }
            builder.AppendLine("}");
        }
        public override void Visit(DefineField node)
        {
            Visit(node.Attributes);
            Visit((FieldLocal)node);
        }
        public override void Visit(DefineProperty node)
        {
            Visit(node.Attributes);
            Visit(node.Modifier);
            Visit(node.Type);
            builder.Append(" ");
            if (node.ExplicitImplement != null)
            {
                Visit(node.ExplicitImplement);
                builder.Append(".");
            }
            builder.Append("{0}", node.Name);
            if (node.IsIndexer)
            {
                builder.Append("[");
                Visit(node.Arguments);
                builder.Append("]");
            }
            builder.AppendLine();
            builder.AppendLine("{");
            if (node.Getter != null) Visit(node.Getter);
            if (node.Setter != null) Visit(node.Setter);
            builder.AppendLine("}");
        }
        public override void Visit(Accessor node)
        {
            Visit(node.Attributes);
            Visit(node.Modifier);
            builder.Append(node.AccessorType.ToString().ToLower());
            if (node.Body == null)
                builder.AppendLine(";");
            else
            {
                builder.AppendLine();
                Visit(node.Body);
            }
        }
        public override void Visit(DefineConstructor node)
        {
            Visit(node.Attributes);
            Visit(node.Modifier);
            builder.Append("{0}", node.Name);
            builder.Append("(");
            Visit(node.Arguments);
            builder.Append(")");
            if (node.Base != null)
            {
                builder.Append(" : ");
                Visit(node.Base);
            }
            builder.AppendLine();
            Visit(node.Body);
        }
        public override void Visit(DefineMethod node)
        {
            Visit(node.Attributes);
            Visit(node.Modifier);
            if (node.IsCast)
                builder.Append("operator ");
            Visit(node.ReturnType);
            builder.Append(" ");
            if (node._IsOperator)
                builder.Append("operator ");
            if (node.ExplicitImplement != null)
            {
                Visit(node.ExplicitImplement);
                builder.Append(".");
            }
            builder.Append("{0}", node.Name);
            if (node.Generic.IsGeneric)
                Visit(node.Generic.GenericTypes);
            builder.Append("(");
            Visit(node.Arguments);
            builder.Append(")");
            if (node.Generic.IsGeneric && node.Generic.Constraints.Count > 0)
                foreach (var item in node.Generic.Constraints) Visit(item);
            if (node.Body == null)
                builder.AppendLine(";");
            else
            {
                builder.AppendLine();
                Visit(node.Body);
            }
        }
        public override void Visit(DefineDelegate node)
        {
            Visit(node.Attributes);
            Visit(node.Modifier);
            builder.Append("delegate ");
            Visit(node.ReturnType);
            builder.Append(" ");
            builder.Append("{0}", node.Name);
            if (node.Generic.IsGeneric)
                Visit(node.Generic.GenericTypes);
            builder.Append("(");
            Visit(node.Arguments);
            builder.Append(")");
            if (node.Generic.IsGeneric && node.Generic.Constraints.Count > 0)
                foreach (var item in node.Generic.Constraints) Visit(item);
            builder.AppendLine(";");
        }
        public override void Visit(List<FormalArgument> node)
        {
            if (node == null)
                return;
            for (int i = 0, n = node.Count - 1; i <= n; i++)
            {
                var item = node[i];
                Visit(item.Attributes);
                if (item.Direction != EDirection.NONE)
                    builder.Append("{0} ", item.Direction.ToString().ToLower());
                if (item.Type != null)
                {
                    Visit(item.Type);
                    builder.Append(" ");
                }
                Visit(item);
                if (i != n)
                    builder.Append(", ");
            }
        }
        public override void Visit(Body node)
        {
            if (node == null)
                builder.AppendLine(";");
            else
            {
                //builder.AppendLine();
                builder.AppendLine("{");
                foreach (var item in node.Statements)
                    VisitStatement(item);
                builder.AppendLine("}");
            }
        }
        public override void Visit(List<ActualArgument> node)
        {
            if (node == null)
                return;
            for (int i = 0, n = node.Count - 1; i <= n; i++)
            {
                var item = node[i];
                if (item.Direction != EDirection.NONE)
                    builder.Append("{0} ", item.Direction.ToString().ToLower());
                Visit(item.Expression);
                if (i != n)
                    builder.Append(", ");
            }
        }
        public override void Visit(Field node)
        {
            builder.Append(node.Name);
            if (node.Value != null)
            {
                builder.Append(" = ");
                Visit(node.Value);
            }
        }
        public override void Visit(FieldLocal node)
        {
            Visit(node.Modifier);
            Visit(node.Type);
            builder.Append(" ");
            Visit((Field)node);
            if (node.IsMultiple)
            {
                foreach (var item in node.Multiple)
                {
                    builder.Append(", ");
                    Visit(item);
                }
            }
            if (node.IsMember)
                builder.AppendLine(";");
        }
        public override void Visit(EModifier node)
        {
            if (node == EModifier.None)
                return;
            for (int i = 0, n = Modifier.Length; i < n; i++)
                if ((node & Modifier[i]) != EModifier.None)
                    builder.Append("{0} ", Modifier[i].ToString().ToLower());
        }
        public override void Visit(FixedText node)
        {
            builder.AppendLine(node.Text);
        }

        public override void Visit(IfStatement node)
        {
            if (node.IsElse)
            {
                builder.Append("else");
            }
            else
            {
                if (node.IsElseIf)
                    builder.Append("else if");
                else
                    builder.Append("if");
                builder.Append(" (");
                Visit(node.Condition);
                builder.Append(")");
            }
            builder.AppendLine();
            Visit(node.Body);
            foreach (var item in node.Else) Visit(item);
        }
        public override void Visit(SwitchStatement node)
        {
            builder.Append("switch (");
            Visit(node.Condition);
            builder.AppendLine(")");
            builder.AppendLine("{");
            foreach (var item in node.Cases) Visit(item);
            builder.AppendLine("}");
        }
        public override void Visit(CaseLabel node)
        {
            if (node.IsDefault)
                builder.AppendLine("default:");
            else
            {
                foreach (var item in node.Labels)
                {
                    builder.Append("case ");
                    Visit(item);
                    builder.AppendLine(":");
                }
            }
            foreach (var item in node.Statements) VisitStatement(item);
        }
        public override void Visit(LabelStatement node)
        {
            builder.Append("{0}:", node.Name);
        }
        public override void Visit(ForStatement node)
        {
            builder.Append("for (");
            for (int i = 0, n = node.Initializers.Count - 1; i <= n; i++)
            {
                Visit(node.Initializers[i]);
                if (i != n)
                    builder.Append(", ");
            }
            builder.Append("; ");
            Visit(node.Condition);
            builder.Append("; ");
            for (int i = 0, n = node.Statements.Count - 1; i <= n; i++)
            {
                Visit(node.Statements[i]);
                if (i != n)
                    builder.Append(", ");
            }
            builder.AppendLine(")");
            Visit(node.Body);
        }
        public override void Visit(WhileStatement node)
        {
            builder.Append("while (");
            Visit(node.Condition);
            builder.AppendLine(")");
            Visit(node.Body);
        }
        public override void Visit(DoWhileStatement node)
        {
            builder.AppendLine("do");
            Visit(node.Body);
            builder.Append("while (");
            Visit(node.Condition);
            builder.AppendLine(");");
        }
        public override void Visit(ForeachStatement node)
        {
            builder.Append("foreach (");
            Visit(node.Type);
            builder.Append(" {0} in ", node.Name);
            Visit(node.In);
            builder.AppendLine(")");
            Visit(node.Body);
        }
        public override void Visit(ContinueStatement node)
        {
            builder.AppendLine("continue;");
        }
        public override void Visit(BreakStatement node)
        {
            builder.AppendLine("break;");
        }
        public override void Visit(ReturnStatement node)
        {
            builder.Append("return");
            if (node.Value != null)
            {
                builder.Append(" ");
                Visit(node.Value);
            }
            builder.AppendLine(";");
        }
        public override void Visit(YieldBreakStatement node)
        {
            builder.AppendLine("yield break;");
        }
        public override void Visit(YieldReturnStatement node)
        {
            builder.Append("yield ");
            Visit((ReturnStatement)node);
        }
        public override void Visit(GotoStatement node)
        {
            builder.Append("goto ");
            if (node.HasCase)
                builder.Append("case ");
            Visit(node.Value);
            builder.AppendLine(";");
        }
        public override void Visit(TryCatchFinallyStatement node)
        {
            builder.AppendLine("try");
            Visit(node.Try);
            Visit(node.Catch);
            if (node.Finally != null)
            {
                builder.AppendLine("finally");
                Visit(node.Finally);
            }
        }
        public override void Visit(List<CatchStatement> node)
        {
            foreach (var item in node)
            {
                builder.Append("catch");
                if (item.HasArgument)
                {
                    builder.Append(" (");
                    Visit(item.Type);
                    if (item.HasName)
                        builder.Append(" {0}", item.Name);
                    builder.Append(")");
                }
                builder.AppendLine();
                Visit(item.Body);
            }
        }
        public override void Visit(ThrowStatement node)
        {
            builder.Append("throw");
            if (node.Value != null)
            {
                builder.Append(" ");
                Visit(node.Value);
            }
            builder.AppendLine(";");
        }
        public override void Visit(UnsafeStatement node)
        {
            builder.AppendLine("unsafe");
            Visit(node.Body);
        }
        public override void Visit(CheckedStatement node)
        {
            builder.AppendLine("checked");
            Visit(node.Body);
        }
        public override void Visit(UncheckedStatement node)
        {
            builder.AppendLine("unchecked");
            Visit(node.Body);
        }
        public override void Visit(LockStatement node)
        {
            builder.Append("lock (");
            Visit(node.Refenrence);
            builder.AppendLine(")");
            Visit(node.Body);
        }
        public override void Visit(UsingStatement node)
        {
            builder.Append("using (");
            WriteSplitByComma(node.Fields);
            builder.AppendLine(")");
            Visit(node.Body);
        }
        public override void Visit(FixedStatement node)
        {
            builder.Append("fixed (");
            WriteSplitByComma(node.Fields);
            builder.AppendLine(")");
            Visit(node.Body);
        }
        public override void Visit(EmptyStatement node)
        {
            builder.Append(";");
        }

        public override void Visit(UnaryOperator node)
        {
            if (node.Operator == EUnaryOperator.PostDecrement ||
                node.Operator == EUnaryOperator.PostIncrement)
            {
                Visit(node.Expression);
                builder.Append(UOP[node.Operator]);
            }
            else
            {
                builder.Append(UOP[node.Operator]);
                Visit(node.Expression);
            }
        }
        public override void Visit(BinaryOperator node)
        {
            Visit(node.Left);
            builder.Append(" {0} ", BOP[node.Operator]);
            Visit(node.Right);
        }
        public override void Visit(ConditionalOperator node)
        {
            Visit(node.Condition);
            builder.Append(" ? ");
            Visit(node.True);
            builder.Append(" : ");
            Visit(node.False);
        }
        public override void Visit(PrimitiveValue node)
        {
            object value = node.Value;

            if (value == null)
                builder.Append("null");
            else if (value is bool)
                if ((bool)value)
                    builder.Append("true");
                else
                    builder.Append("false");
            else if (value is string)
            {
                builder.Append('\"');
                string str = value.ToString();
                foreach (char c in str)
                {
                    switch (c)
                    {
                        case '"': builder.Append("\\\""); break;
                        case '\r': builder.Append("\\r"); break;
                        case '\n': builder.Append("\\n"); break;
                        case '\0': builder.Append("\\0"); break;
                        case '\t': builder.Append("\\t"); break;
                        case '\\': builder.Append("\\\\"); break;
                        default: builder.Append(c); break;
                    }
                }
                builder.Append('\"');
            }
            else if (value is char)
            {
                char c = (char)value;
                if (c == '\'')
                    builder.Append("'\\''");
                else if (c == '\\')
                    builder.Append("'\\\\'");
                else
                    builder.Append("'{0}'", _SERIALIZE.CharToCodeChar(c));
            }
            else if (value is decimal)
                builder.Append("{0}m", value);
            else if (value is float)
                builder.Append("{0}f", value);
            else if (value is uint)
                builder.Append("{0}u", value);
            else if (value is long)
                builder.Append("{0}L", value);
            else if (value is ulong)
                builder.Append("{0}uL", value);
            else
                builder.Append(value);
        }
        public override void Visit(ArrayValue node)
        {
            builder.Append("{");
            for (int i = 0, n = node.Values.Count - 1; i <= n; i++)
            {
                Visit(node.Values[i]);
                if (i != n)
                    builder.Append(",");
            }
            builder.Append("}");
        }
        public override void Visit(Parenthesized node)
        {
            builder.Append("(");
            Visit(node.Expression);
            builder.Append(")");
        }
        public override void Visit(Checked node)
        {
            builder.Append("checked(");
            Visit(node.Expression);
            builder.Append(")");
        }
        public override void Visit(Unchecked node)
        {
            builder.Append("unchecked(");
            Visit(node.Expression);
            builder.Append(")");
        }
        public override void Visit(Lambda node)
        {
            builder.Append("(");
            Visit(node.Parameters);
            builder.Append(") => ");
            if (node.IsSingleBody)
                Visit(node.Body.Statements[0]);
            else
            {
                builder.AppendLine();
                Visit(node.Body);
            }
        }
        public override void Visit(ReferenceMember node)
        {
            if (node.Target != null)
            {
                Visit(node.Target);
                builder.Append(".");
            }
            //foreach (var item in node.Target)
            //{
            //    Visit(item);
            //    builder.Append(".");
            //}
            builder.Append(node.Name);
            if (node.GenericTypes.Count > 0)
            {
                builder.Append("<");
                for (int i = 0, n = node.GenericTypes.Count - 1; i <= n; i++)
                {
                    Visit(node.GenericTypes[i]);
                    if (i != n)
                        builder.Append(", ");
                }
                builder.Append(">");
            }
            for (int i = 0; i < node.ArrayDimension; i++)
                builder.Append("[]");
        }
        //public override void Visit(CodeThis node)
        //{
        //    builder.Append("this");
        //}
        //public override void Visit(CodeBase node)
        //{
        //    builder.Append("base");
        //}
        //public override void Visit(CodeNullable node)
        //{
        //    Visit((ReferenceMember)node);
        //    builder.Append("?");
        //}
        //public override void Visit(CodePointer node)
        //{
        //    Visit((ReferenceMember)node);
        //    builder.Append("*");
        //}
        public override void Visit(UsingNamespace node)
        {
            builder.Append("using ");
            if (node.Alias != null)
                builder.Append("{0} = ", node.Alias);
            Visit(node.Reference);
            builder.AppendLine(";");
        }
        public override void Visit(TypeOf node)
        {
            builder.Append("typeof(");
            Visit(node.Reference);
            builder.Append(")");
        }
        public override void Visit(SizeOf node)
        {
            builder.Append("sizeof(");
            Visit(node.Reference);
            builder.Append(")");
        }
        public override void Visit(DefaultValue node)
        {
            builder.Append("default(");
            Visit(node.Reference);
            builder.Append(")");
        }
        public override void Visit(InvokeMethod node)
        {
            Visit(node.Target);
            if (node.IsIndexer)
            {
                builder.Append("[");
                Visit(node.Arguments);
                builder.Append("]");
            }
            else
            {
                builder.Append("(");
                Visit(node.Arguments);
                builder.Append(")");
            }
        }
        public override void Visit(List<InvokeAttribute> node)
        {
            if (node == null)
                return;
            foreach (var item in node)
            {
                builder.Append("[");
                if (item.IsAssembly)
                    builder.Append("assembly: ");
                else if (item.IsModule)
                    builder.Append("module: ");
                else if (item.IsReturnValue)
                    builder.Append("return: ");
                if (item.Arguments == null || item.Arguments.Count == 0)
                    Visit(item.Target);
                else
                    Visit((InvokeMethod)item);
                builder.AppendLine("]");
            }
        }
        public override void Visit(New node)
        {
            builder.Append("new ");
            Visit(node.Method);
            if (node.Initializer != null && node.Initializer.Count > 0)
            {
                builder.Append("{");
                WriteSplitByComma(node.Initializer);
                builder.Append("}");
            }
        }
        public override void Visit(As node)
        {
            Visit(node.Expression);
            builder.Append(" as ");
            Visit(node.Reference);
        }
        public override void Visit(Is node)
        {
            Visit(node.Expression);
            builder.Append(" is ");
            Visit(node.Reference);
        }
        public override void Visit(Cast node)
        {
            builder.Append("(");
            Visit(node.Type);
            builder.Append(")");
            Visit(node.Expression);
            //builder.Append(")");
        }
    }
    public class CSharpDummyCodeBuilder : CSharpCodeBuilder
    {
        public override void Visit(Body node)
        {
            if (node != null)
                builder.AppendLine("{throw new global::System.NotImplementedException();}");
            else
                builder.AppendLine(";");
        }
    }
}


namespace EntryBuilder.CodeAnalysis.Refactoring
{
    using EntryBuilder.CodeAnalysis.Semantics;

    internal static class _Reflection
    {
        public static Dictionary<Assembly, CSharpAssembly> AssemblyMaps = new Dictionary<Assembly, CSharpAssembly>();
        public static Dictionary<Type, CSharpType> TypeMaps = new Dictionary<Type, CSharpType>();
    }
    internal class RefactoringHelper
    {
        Dictionary<SyntaxNode, DefinedType> _types;
        Dictionary<SyntaxNode, MemberDefinitionInfo> _members;
        Dictionary<object, BEREF> _refs;
        Dictionary<SyntaxNode, REF> _syntax;
        public Dictionary<SyntaxNode, DefinedType> types
        {
            get
            {
                if (_types == null)
                    _types = _BuildReference.types;
                return _types;
            }
        }
        public Dictionary<SyntaxNode, MemberDefinitionInfo> members
        {
            get
            {
                if (_members == null)
                    _members = _BuildReference.members;
                return _members;
            }
        }
        public Dictionary<object, BEREF> refs
        {
            get
            {
                if (_refs == null)
                    _refs = _BuildReference.objectReferences;
                return _refs;
            }
        }
        public Dictionary<SyntaxNode, REF> syntax
        {
            get
            {
                if (_syntax == null)
                    _syntax = _BuildReference.syntaxReferences;
                return _syntax;
            }
        }
        public bool HasReference(object obj)
        {
            BEREF beref;
            if (refs.TryGetValue(obj, out beref))
                return beref.Define != null;
            return false;
        }
    }
    public class Rewriter : CSharpCodeBuilder
    {
        private static readonly string[] SerializeAttribute = { "Serializable", "NonSerialized", "ANonSerializedP" };
        public static bool IsSerializableAttribute(string name)
        {
            for (int i = 0; i < SerializeAttribute.Length; i++)
                if (name.EndsWith(SerializeAttribute[i]))
                    return true;
            return false;
        }

        internal static Named THIS = new Named("this");
        Dictionary<SyntaxNode, DefinedType> _types;
        Dictionary<SyntaxNode, MemberDefinitionInfo> _members;
        Dictionary<object, BEREF> _refs;
        Dictionary<SyntaxNode, REF> _syntax;
        private Stack<CSharpType> _definingTypes = new Stack<CSharpType>();
        private Stack<CSharpMember> _definingMembers = new Stack<CSharpMember>();
        internal Dictionary<SyntaxNode, DefinedType> types
        {
            get
            {
                if (_types == null)
                    _types = _BuildReference.types;
                return _types;
            }
        }
        internal Dictionary<SyntaxNode, MemberDefinitionInfo> members
        {
            get
            {
                if (_members == null)
                    _members = _BuildReference.members;
                return _members;
            }
        }
        public Dictionary<object, BEREF> refs
        {
            get
            {
                if (_refs == null)
                    _refs = _BuildReference.objectReferences;
                return _refs;
            }
        }
        public Dictionary<SyntaxNode, REF> syntax
        {
            get
            {
                if (_syntax == null)
                    _syntax = _BuildReference.syntaxReferences;
                return _syntax;
            }
        }
        protected CSharpType DefiningType
        {
            get { return _definingTypes.Peek(); }
        }
        protected CSharpMember DefiningMember
        {
            get { return _definingMembers.Peek(); }
        }
        protected bool SetMember(SyntaxNode node)
        {
            MemberDefinitionInfo member;
            if (members.TryGetValue(node, out member) && HasMember(member))
            {
                _definingMembers.Clear();
                _definingMembers.Push(member);
                OnSetMember(member);
                return true;
            }
            return false;
        }
        /// <summary>除了定义方法外，lambda表达式，闭包等都会生成临时方法；会影响实参，return表达式</summary>
        public void PushMember(CSharpMember member)
        {
            _definingMembers.Push(member);
        }
        public void PopMember() { _definingMembers.Pop(); }
        public bool HasMember(CSharpMember member)
        {
            return !member.IsExtern && (HasReference(member) || member.Attributes.Any(a => a.Type.Name.Name == ANonOptimize.Name));
        }
        public bool HasMember(CSharpMember member, out BEREF beref)
        {
            return (HasReference(member, out beref) || member.Attributes.Any(a => a.Type.Name.Name == ANonOptimize.Name)) && !member.IsExtern;
        }
        protected virtual void OnSetMember(CSharpMember member) { }
        internal bool HasReference(object obj)
        {
            BEREF beref;
            return HasReference(obj, out beref);
        }
        internal bool HasReference(object obj, out BEREF beref)
        {
            if (refs.TryGetValue(obj, out beref))
                return beref.Define != null;
            return false;
        }
        public bool HasType(CSharpType type)
        {
            BEREF beref;
            return HasType(type, out beref);
        }
        public bool HasType(CSharpType type, out BEREF beref)
        {
            return HasReference(type, out beref) || type.Attributes.Any(a => a.Type.Name.Name == ANonOptimize.Name);
        }
        public bool IsDefineOnly(CSharpType type)
        {
            if (type == null)
                return false;
            while (type.DefiningType != null)
                type = type.DefiningType;
            var find = types.FirstOrDefault(p => p.Value.Type == type);
            if (find.Value != null)
                return find.Value.DefineFile == EFileType.Define;
            return false;
        }
        public CSharpType GetSyntaxType(SyntaxNode node)
        {
            CSharpType type;
            CSharpMember member;
            VAR _var;
            return GetSyntaxType(node, out type, out member, out _var);
        }
        public CSharpType GetSyntaxType(SyntaxNode node, out CSharpType type, out CSharpMember member, out VAR _var)
        {
            type = null;
            member = null;
            _var = null;

            REF _ref;
            if (syntax.TryGetValue(node, out _ref))
            {
                type = _ref.Definition.Define as CSharpType;
                if (type != null) return type;

                member = _ref.Definition.Define as CSharpMember;
                if (member != null)
                {
                    //if (member.IsConstructor && !member.IsStatic)
                    //    return member.ContainingType;
                    //else
                    return member.ReturnType;
                }

                _var = _ref.Definition.Define as VAR;
                if (_var != null) return _var.Type;
            }
            return null;
        }
        public sealed override void Visit(DefineType node)
        {
            DefinedType define;
            if (types.TryGetValue(node, out define))
            {
                var type = define.Type;
                if (!HasType(type))
                    return;
                _definingTypes.Push(type);
                if (type == CSharpType.OBJECT)
                    VisitTypeObject(node);
                else if (type == CSharpType.STRING)
                    VisitTypeString(node);
                else if (type == CSharpType.ARRAY)
                    VisitTypeArray(node);
                else if (type == CSharpType.MATH)
                    VisitTypeMath(node);    
                else if (type == CSharpType.BOOL)
                    VisitTypeBool(node);
                else if (type == CSharpType.BYTE)
                    VisitTypeByte(node);
                else if (type == CSharpType.SBYTE)
                    VisitTypeSByte(node);
                else if (type == CSharpType.USHORT)
                    VisitTypeUShort(node);
                else if (type == CSharpType.SHORT)
                    VisitTypeShort(node);
                else if (type == CSharpType.CHAR)
                    VisitTypeChar(node);
                else if (type == CSharpType.UINT)
                    VisitTypeUInt(node);
                else if (type == CSharpType.INT)
                    VisitTypeInt(node);
                else if (type == CSharpType.FLOAT)
                    VisitTypeFloat(node);
                else if (type == CSharpType.ULONG)
                    VisitTypeULong(node);
                else if (type == CSharpType.LONG)
                    VisitTypeLong(node);
                else if (type == CSharpType.DOUBLE)
                    VisitTypeDouble(node);
                else
                    Write(node);
                _definingTypes.Pop();
            }
            else
                Write(node);
        }
        public sealed override void Visit(DefineEnum node)
        {
            DefinedType define;
            if (types.TryGetValue(node, out define))
            {
                var type = define.Type;
                if (!HasType(type))
                    return;
                _definingTypes.Push(type);
                Write(node);
                _definingTypes.Pop();
            }
            else
                Write(node);
        }
        public sealed override void Visit(DefineDelegate node)
        {
            DefinedType define;
            if (types.TryGetValue(node, out define))
            {
                var type = define.Type;
                if (!HasType(type))
                    return;
                _definingTypes.Push(type);
                Write(node);
                _definingTypes.Pop();
            }
            else
                Write(node);
        }
        protected virtual void VisitTypeObject(DefineType type)
        {
            Write(type);
        }
        protected virtual void VisitTypeString(DefineType type)
        {
            Write(type);
        }
        protected virtual void VisitTypeArray(DefineType type)
        {
            Write(type);
        }
        protected virtual void VisitTypeMath(DefineType type)
        {
            Write(type);
        }
        protected virtual void VisitTypeBool(DefineType type)
        {
            Write(type);
        }
        protected virtual void VisitTypeByte(DefineType type)
        {
            Write(type);
        }
        protected virtual void VisitTypeSByte(DefineType type)
        {
            Write(type);
        }
        protected virtual void VisitTypeUShort(DefineType type)
        {
            Write(type);
        }
        protected virtual void VisitTypeShort(DefineType type)
        {
            Write(type);
        }
        protected virtual void VisitTypeChar(DefineType type)
        {
            Write(type);
        }
        protected virtual void VisitTypeUInt(DefineType type)
        {
            Write(type);
        }
        protected virtual void VisitTypeInt(DefineType type)
        {
            Write(type);
        }
        protected virtual void VisitTypeFloat(DefineType type)
        {
            Write(type);
        }
        protected virtual void VisitTypeULong(DefineType type)
        {
            Write(type);
        }
        protected virtual void VisitTypeLong(DefineType type)
        {
            Write(type);
        }
        protected virtual void VisitTypeDouble(DefineType type)
        {
            Write(type);
        }
        protected virtual void Write(DefineType node)
        {
            base.Visit(node);
        }
        protected virtual void Write(DefineEnum node)
        {
            base.Visit(node);
        }
        protected virtual void Write(DefineDelegate node)
        {
            base.Visit(node);
        }
        public sealed override void Visit(DefineField node)
        {
            if (SetMember(node))
                Write(node);
        }
        public sealed override void Visit(DefineProperty node)
        {
            if (SetMember(node))
                Write(node);
        }
        public sealed override void Visit(DefineMethod node)
        {
            if (SetMember(node))
                Write(node);
        }
        public sealed override void Visit(DefineConstructor node)
        {
            if (SetMember(node))
                Write(node);
        }
        protected virtual void Write(DefineField node)
        {
            base.Visit(node);
        }
        protected virtual void Write(DefineProperty node)
        {
            base.Visit(node);
        }
        protected virtual void Write(DefineMethod node)
        {
            base.Visit(node);
        }
        protected virtual void Write(DefineConstructor node)
        {
            base.Visit(node);
        }

        internal virtual void WriteBegin(IEnumerable<DefineFile> files)
        {
        }
        internal virtual void WriteEnd(IEnumerable<DefineFile> files)
        {
        }
    }
    internal enum EEnumerator
    {
        None,
        Enumerator,
        Enumerable,
    }
    internal class IEnumeratorTester : SyntaxWalker
    {
        static IEnumeratorTester Tester;
        /// <summary>0:不包含/1:包含/2:不包含而包含Return，快速结束检测</summary>
        private byte HasYield;
        private IEnumeratorTester() { }
        public static EEnumerator EnumeratorType(CSharpType type)
        {
            if (type == null)
                return EEnumerator.None;

            CSharpType temp;
            if (type.IsType(CSharpType.IENUMERABLE, out temp))
                return EEnumerator.Enumerable;
            else if (type.IsType(CSharpType.IENUMERATOR, out temp))
                return EEnumerator.Enumerator;

            if (CSharpType.IENUMERABLE != null && type == CSharpType.IENUMERABLE)
                return EEnumerator.Enumerable;
            if (CSharpType.IENUMERATOR != null && type == CSharpType.IENUMERATOR)
                return EEnumerator.Enumerator;
            if (type.ContainingNamespace == null)
                return EEnumerator.None;
            string ns = type.ContainingNamespace.ToString();
            if (ns == "System.Collections.Generic")
            {
                if (type.TypeParameters.Count != 1)
                    return EEnumerator.None;
            }
            else if (ns == "System.Collections")
            {
                if (type.TypeParameters.Count != 0)
                    return EEnumerator.None;
            }
            else
                return EEnumerator.None;
            string name = type.Name.Name;
            if (name == "IEnumerator")
                return EEnumerator.Enumerator;
            else if (name == "IEnumerable")
                return EEnumerator.Enumerable;
            else
                return EEnumerator.None;
        }
        /// <summary>测试是否包含yield，需要重构语法逻辑</summary>
        public static bool TestYield(Body node)
        {
            if (node == null)
                return false;
            return TestYield(node.Statements);
        }
        public static bool TestYield(IEnumerable<SyntaxNode> statements)
        {
            if (statements == null)
                return false;
            if (Tester == null)
                Tester = new IEnumeratorTester();
            Tester.HasYield = 0;
            Tester.Test(statements);
            return Tester.HasYield == 1;
        }
        private void Test(IEnumerable<SyntaxNode> statements)
        {
            foreach (var item in statements)
            {
                if (HasYield == 2)
                    return;
                int temp = HasYield;
                Visit(item);
                // 既有yield return又有return
                if (temp != 0 && temp != HasYield)
                    throw new InvalidCastException();
            }
        }
        public override void Visit(Body node)
        {
            if (node == null)
                return;
            Test(node.Statements);
        }
        public override void Visit(Lambda node)
        {
        }
        public override void Visit(ReturnStatement node)
        {
            HasYield = 2;
        }
        public override void Visit(YieldBreakStatement node)
        {
            HasYield = 1;
        }
        public override void Visit(YieldReturnStatement node)
        {
            HasYield = 1;
        }
    }
    internal class IEnumeratorRebuilder : SyntaxWalker
    {
        internal static Named ENUMERATOR_STATE = new Named("this._s");
        internal static Named GET_ENUMERATOR = new Named("GetEnumerator");
        internal static Named CURRENT = new Named("this.$current");
        internal static Named MOVE_NEXT = new Named("MoveNext");
        internal static Named RESET = new Named("Reset");
        internal static Named DISPOSE = new Named("Dispose");

        class ContinueToReturn : SyntaxWalker
        {
            private static ContinueToReturn instance;
            private ContinueToReturn() { }
            public static void Change(Body body)
            {
                if (instance == null) instance = new ContinueToReturn();
                instance.Visit(body);
            }
            public override void Visit(Body node)
            {
                if (node != null)
                {
                    //if (!IEnumeratorTester.TestYield(node))
                    //    return;
                    for (int i = node.Statements.Count - 1; i >= 0; i--)
                    {
                        var item = node.Statements[i];
                        if (item is ContinueStatement)
                        {
                            // ++那个表达式
                            node.Statements[i] = INS.ReturnUnresolvedSymbol();
                        }
                        else
                            base.Visit(item);
                    }
                }
            }
        }
        class BreakToReturn : SyntaxWalker
        {
            private static BreakToReturn instance;
            private BreakToReturn() { }
            public static void Change(Body body)
            {
                if (instance == null) instance = new BreakToReturn();
                instance.Visit(body);
            }
            public override void Visit(Body node)
            {
                if (node != null)
                {
                    //if (!IEnumeratorTester.TestYield(node))
                    //    return;
                    for (int i = node.Statements.Count - 1; i >= 0; i--)
                    {
                        var item = node.Statements[i];
                        if (item is BreakStatement)
                        {
                            // ++后的一个表达式
                            node.Statements[i] = INS.ReturnUnresolvedSymbol();
                        }
                        else
                            base.Visit(item);
                    }
                }
            }
        }
        class BreakToEmpty : SyntaxWalker
        {
            private static BreakToEmpty instance;
            private BreakToEmpty() { }
            public static void Change(Body body)
            {
                if (instance == null) instance = new BreakToEmpty();
                instance.Visit(body);
            }
            public override void Visit(Body node)
            {
                if (node != null)
                {
                    for (int i = node.Statements.Count - 1; i >= 0; i--)
                    {
                        var item = node.Statements[i];
                        if (item is BreakStatement)
                        {
                            //node.Statements[i] = INS.ReturnState(-3);
                            node.Statements.RemoveAt(i);
                        }
                        else
                            base.Visit(item);
                    }
                }
            }
        }
        class PlusThis : SyntaxWalker
        {
            private static ReferenceMember THIS = new ReferenceMember(JSRewriter.THIS);
            private static PlusThis instance;
            private PlusThis() { }
            public static void Plus(Body body)
            {
                if (instance == null) instance = new PlusThis();
                instance.Visit(body);
            }
            public override void Visit(ReferenceMember node)
            {
                if (node.Target != null)
                {
                    Visit(node.Target);
                }
                else
                {
                    if (INS.define.Fields.Any(f => f.Name.Name == node.Name.Name))
                    {
                        node.Target = THIS;
                    }
                }
            }
        }

        internal static IEnumeratorRebuilder INS = new IEnumeratorRebuilder();
        List<List<SyntaxNode>> stateList = new List<List<SyntaxNode>>();
        List<ReturnStatement> unresolved = new List<ReturnStatement>();
        int depth = -1;
        DefineType define;
        Stack<ReturnStatement> recordLoopBeginStates = new Stack<ReturnStatement>();
        HashSet<List<SyntaxNode>> breakOrContinues = new HashSet<List<SyntaxNode>>();

        List<SyntaxNode> States { get { return stateList[depth]; } }
        ReturnStatement recordLoopBeginState
        {
            get { return recordLoopBeginStates.Peek(); }
            set
            {
                if (value == null)
                    recordLoopBeginStates.Pop();
                else
                    recordLoopBeginStates.Push(value);
            }
        }

        protected IEnumeratorRebuilder() { }
        public static List<List<SyntaxNode>> Build(List<FormalArgument> arguments, CSharpMember method, Body body, out DefineType define)
        {
            INS.Initialize();
            INS.define.Name = new Named("___" + method.Name.Name);
            if (arguments != null && arguments.Count > 0)
                foreach (var item in arguments)
                    INS.AddFieldOnly(item);
            INS.OpenNewBlock();
            INS.Visit(body);
            INS.ResolveReturnSymbol(INS.GetNextState());
            define = INS.define;
            //PlusThis.Plus(body);
            return INS.stateList;
        }

        void Initialize()
        {
            stateList.Clear();
            depth = -1;
            define = new DefineType();
            define.Type = EType.STRUCT;
        }
        void OpenNewBlock()
        {
            List<SyntaxNode> block = new List<SyntaxNode>();
            stateList.Add(block);
            depth++;
        }
        ReturnStatement RecordLoopBeginState()
        {
            recordLoopBeginState = new ReturnStatement();
            recordLoopBeginState.Value = new PrimitiveValue(GetCurrentState());
            return recordLoopBeginState;
        }
        ReturnStatement PeekRecordLoopBeginState()
        {
            if (recordLoopBeginState == null)
                throw new InvalidOperationException();
            return recordLoopBeginState;
        }
        ReturnStatement PopRecordLoopBeginState()
        {
            var temp = PeekRecordLoopBeginState();
            recordLoopBeginState = null;
            return temp;
        }
        ReturnStatement ReturnNextState()
        {
            return ReturnState(GetNextState());
        }
        ReturnStatement ReturnUnresolvedSymbol()
        {
            var state = new ReturnStatement();
            unresolved.Add(state);
            return state;
        }
        void ResolveReturnSymbol(int state)
        {
            foreach (var item in unresolved)
                item.Value = new PrimitiveValue(state);
            unresolved.Clear();
        }
        ReturnStatement ReturnState(int value)
        {
            ReturnStatement rs = new ReturnStatement();
            rs.Value = new PrimitiveValue(value);
            return rs;
        }
        int GetCurrentState()
        {
            return depth;
        }
        int GetNextState()
        {
            return GetCurrentState() + 1;
        }
        void SetState(SyntaxNode value)
        {
            BinaryOperator bop = new BinaryOperator();
            bop.Left = new ReferenceMember(ENUMERATOR_STATE);
            bop.Operator = EBinaryOperator.Assign;
            bop.Right = new PrimitiveValue(GetNextState());
            States.Add(bop);

            bop = new BinaryOperator();
            bop.Left = new ReferenceMember(CURRENT);
            bop.Operator = EBinaryOperator.Assign;
            bop.Right = value;
            States.Add(bop);
        }
        SyntaxNode WithParenthesized(SyntaxNode node)
        {
            if (node is BinaryOperator || node is UnaryOperator)
                return new Parenthesized(node);
            else
                return node;
        }
        void ChangeBreak()
        {
            foreach (var item in breakOrContinues)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    if (item[i] is BreakStatement)
                    {
                        item[i] = ReturnUnresolvedSymbol();
                    }
                }
            }
        }
        void ChangeContinue()
        {
            foreach (var item in breakOrContinues)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    if (item[i] is ContinueStatement)
                    {
                        item[i] = ReturnUnresolvedSymbol();
                    }
                }
            }
        }
        void YieldWhile(Body body, SyntaxNode condition, Action exp3)
        {
            RecordLoopBeginState();

            // 第二表达式决定跳转的Body
            IfStatement ifs = new IfStatement();
            ifs.Condition = condition;
            ifs.Body = new Body();
            ifs.Body.Statements.Add(ReturnNextState());

            var unresolved = new ReturnStatement();
            IfStatement els = new IfStatement();
            els.Body = new Body();
            els.Body.Statements.Add(unresolved);
            ifs.Else.Add(els);

            States.Add(ifs);

            // for循环体生成Body块
            OpenNewBlock();
            Visit(body);
            ContinueToReturn.Change(body);
            ChangeContinue();
            ResolveReturnSymbol(GetNextState());

            // 第三表达式生成块
            OpenNewBlock();
            if (exp3 != null)
                exp3();
            States.Add(PopRecordLoopBeginState());
            BreakToReturn.Change(body);
            ChangeBreak();
            ResolveReturnSymbol(GetNextState());

            unresolved.Value = new PrimitiveValue(GetNextState());
            OpenNewBlock();

            breakOrContinues.Clear();
        }
        void YieldDowhile(Body body, SyntaxNode condition)
        {
            RecordLoopBeginState();

            // for循环体生成Body块
            Visit(body);
            ContinueToReturn.Change(body);
            ChangeContinue();
            ResolveReturnSymbol(GetNextState());

            OpenNewBlock();
            BreakToReturn.Change(body);
            ChangeBreak();
            ResolveReturnSymbol(GetNextState());

            // 第二表达式决定跳转的Body
            IfStatement ifs = new IfStatement();
            ifs.Condition = condition;
            ifs.Body = new Body();
            ifs.Body.Statements.Add(PopRecordLoopBeginState());

            IfStatement els = new IfStatement();
            els.Body = new Body();
            els.Body.Statements.Add(ReturnNextState());
            ifs.Else.Add(els);

            States.Add(ifs);
            OpenNewBlock();

            breakOrContinues.Clear();
        }
        protected virtual DefineField AddFieldOnly(FormalArgument arg)
        {
            DefineField field = new DefineField();
            field.Type = arg.Type;
            field.Name = arg.Name;
            this.define.Fields.Add(field);
            return field;
        }
        DefineField AddField(ReferenceMember type, Named name, SyntaxNode value)
        {
            DefineField f = new DefineField();
            f.Type = type;
            f.Name = name;
            this.define.Fields.Add(f);

            // 赋值
            if (value != null)
            {
                BinaryOperator bop = new BinaryOperator();
                bop.Operator = EBinaryOperator.Assign;
                bop.Left = WithThis(f.Name);
                bop.Right = value;
                States.Add(bop);
            }

            return f;
        }
        protected virtual DefineField AddField(FieldLocal f)
        {
            DefineField field = AddField(f.Type, f.Name, f.Value);

            foreach (var mul in f.Multiple)
            {
                if (mul.Value != null)
                {
                    var bop = new BinaryOperator();
                    bop.Operator = EBinaryOperator.Assign;
                    bop.Left = WithThis(mul.Name);
                    bop.Right = mul.Value;
                    States.Add(bop);
                }

                Field field2 = new Field();
                field2.Name = mul.Name;
                field.Multiple.Add(field2);
            }

            return field;
        }
        ReferenceMember WithThis(Named name)
        {
            if (name.Name.StartsWith("this."))
                return new ReferenceMember(name);
            return new ReferenceMember(new Named(string.Format("{0}.{1}", JSRewriter.THIS, name.Name)));
        }

        public override void Visit(Body node)
        {
            int bodyYieldFlag = GetCurrentState();
            bool yieldFlag = false;
            for (int i = 0, n = node.Statements.Count - 1; i <= n; i++)
            {
                var item = node.Statements[i];
                // 后续由BreakToReturn和ContinueToReturn更换成return语句
                //if (item is BreakStatement || item is ContinueStatement)
                //    continue;

                if (item is FieldLocal && IEnumeratorTester.TestYield(node.Statements.Skip(i + 1)))
                    // 临时变量声明后面有Yield相关内容，此变量需要提升为类型字段
                    AddField((FieldLocal)item);
                else
                {
                    // 后续更换成return语句
                    if (item is BreakStatement || item is ContinueStatement)
                        breakOrContinues.Add(States);
                    Visit(item);
                }
                // 根据yield表达式分块
                //if (i != n && (item is YieldReturnStatement || item is YieldBreakStatement))
                if (item is YieldReturnStatement)
                {
                    yieldFlag = true;
                    //if (i != n)
                    OpenNewBlock();
                }
                else if (i < n && item is YieldBreakStatement)
                {
                    // 跳过yield break后的不可到达的表达式
                    return;
                }
            }
            if (yieldFlag || bodyYieldFlag != GetCurrentState())
            {
                if (States.Count > 0)
                {
                    var last = States[States.Count - 1];
                    if (last is ReturnStatement || last is ContinueStatement || last is BreakStatement)
                        return;
                }
                // 块中最后一个yield到块末尾的部分
                States.Add(ReturnUnresolvedSymbol());
            }

            // 仅1条跳转语句的状态机还是有必要的
            //if (a == 0)
            //{
            //    yield return a;
            //    // 仅有一条跳转语句，跳转到整个if块的结尾
            //}
            //else if (a == 1)
            //{
            //    a++;
            //}
        }
        public override void Visit(ForStatement node)
        {
            bool hasYield = IEnumeratorTester.TestYield(node.Body);
            if (hasYield)
            {
                // 第一表达式
                foreach (var item in node.Initializers)
                {
                    if (item is FieldLocal)
                    {
                        AddField((FieldLocal)item);
                    }
                    else if (item is BinaryOperator)
                    {
                        States.Add(item);
                    }
                    else
                        throw new NotImplementedException();
                }
                States.Add(ReturnNextState());
                // 第一表达式结束
                OpenNewBlock();
                YieldWhile(node.Body, node.Condition,
                    () =>
                    {
                        foreach (var item in node.Statements)
                            States.Add(item);
                    });
            }
            else
            {
                States.Add(node);
            }
        }
        public override void Visit(ForeachStatement node)
        {
            bool hasYield = IEnumeratorTester.TestYield(node.Body);
            if (hasYield)
            {
                var leftName = new Named("__" + GetCurrentState());

                // 第一表达式
                AddField(null, leftName, new InvokeMethod() { Target = new ReferenceMember(new Named("GetEnumerator")) { Target = node.In } });
                //BinaryOperator bop = new BinaryOperator();
                //bop.Operator = EBinaryOperator.Assign;
                //bop.Left = new ReferenceMember(field.Name);
                //bop.Right = new InvokeMethod() { Target = new ReferenceMember(new Named("GetEnumerator")) { Target = node.In } };
                //States.Add(bop);
                var bop = (BinaryOperator)States.Last();
                States.Add(ReturnNextState());

                InvokeMethod condition = new InvokeMethod();
                condition.Target = new ReferenceMember(new Named("MoveNext")) { Target = bop.Left };


                OpenNewBlock();
                // item => this.item = this.__0.Current
                bop = new BinaryOperator();
                bop.Operator = EBinaryOperator.Assign;
                bop.Left = WithThis(node.Name);
                // BUG: 如果此类型需要通用的，不可能写死调用__getCurrent()
                bop.Right = new ReferenceMember(new Named(string.Format("this.{0}.{1}Current()", leftName.Name, JSRewriter.GET)));
                node.Body.Statements.Insert(0, bop);
                YieldWhile(node.Body, condition, null);
            }
            else
            {
                States.Add(node);
            }
        }
        public override void Visit(WhileStatement node)
        {
            bool hasYield = IEnumeratorTester.TestYield(node.Body);
            if (hasYield)
            {
                OpenNewBlock();
                YieldWhile(node.Body, node.Condition, null);
            }
            else
            {
                States.Add(node);
            }
        }
        public override void Visit(DoWhileStatement node)
        {
            bool hasYield = IEnumeratorTester.TestYield(node.Body);
            if (hasYield)
            {
                OpenNewBlock();
                YieldDowhile(node.Body, node.Condition);
            }
            else
            {
                States.Add(node);
            }
        }
        public override void Visit(IfStatement node)
        {
            // 判断错误
            bool hasYield = IEnumeratorTester.TestYield(node.Body);
            bool isIf = !(node.IsElseIf || node.IsElse);
            if (hasYield)
            {
                // if.Body => 跳转到if.Body
                var body = node.Body;

                // 生成if.Body
                node.Body = new Body();
                node.Body.Statements.Add(ReturnNextState());

                // 普通else和else if涵盖在了普通if块中
                ReturnStatement unresolved = null;
                if (isIf)
                {
                    States.Add(node);
                    unresolved = new ReturnStatement();
                    States.Add(unresolved);
                    //States.Add(ReturnUnresolvedSymbol());
                }

                OpenNewBlock();
                // 可能if在循环中，但又把body弄丢了，导致循环无法改变控制语句的语法
                Visit(body);

                // else结束后跳转到整个if段的结束
                if (isIf)
                {
                    foreach (var item in node.Else) Visit(item);
                    ResolveReturnSymbol(GetNextState());
                    if (unresolved != null)
                        unresolved.Value = new PrimitiveValue(GetNextState());
                    OpenNewBlock();
                }
            }
            else
            {
                // 没有yield的普通if块
                if (isIf)
                {
                    States.Add(node);
                    States.Add(ReturnUnresolvedSymbol());
                    foreach (var item in node.Else) Visit(item);
                    ResolveReturnSymbol(GetNextState());
                    OpenNewBlock();
                }
            }
        }
        public override void Visit(SwitchStatement node)
        {
            bool hasYield = false;
            foreach (var item in node.Cases)
            {
                hasYield = IEnumeratorTester.TestYield(item.Body);
                if (hasYield)
                    break;
            }
            if (hasYield)
            {
                if (node.Cases.Count == 1 && node.Cases[0].IsDefault)
                {
                    // 仅有一个default标签的switch
                    this.Visit(node.Cases[0].Body);
                    return;
                }

                FieldLocal tempSwitch = new FieldLocal();
                tempSwitch.Type = new ReferenceMember(new Named("var"));
                tempSwitch.Name = new Named("__" + GetCurrentState());
                tempSwitch.Value = node.Condition;
                States.Add(tempSwitch);
                ReferenceMember reference = new ReferenceMember(tempSwitch.Name);

                // 将case块全部修改为if块的实现方式
                IfStatement _if = null;
                foreach (var item in node.Cases)
                {
                    IfStatement temp = new IfStatement();
                    if (item.IsCase)
                    {
                        for (int i = 0, n = item.Labels.Count - 1; i <= n; i++)
                        {
                            BinaryOperator condition = new BinaryOperator();
                            condition.Left = reference;
                            condition.Operator = EBinaryOperator.Equality;
                            condition.Right = WithParenthesized(item.Labels[i]);
                            if (temp.Condition == null) { temp.Condition = condition; }
                            else
                            {
                                BinaryOperator or = new BinaryOperator();
                                if (i == 1)
                                {
                                    // 为第一个没加括号的条件也追加上括号
                                    temp.Condition = new Parenthesized(temp.Condition);
                                }
                                or.Left = temp.Condition;
                                or.Operator = EBinaryOperator.ConditionalOr;
                                or.Right = new Parenthesized(condition);
                                temp.Condition = or;
                            }
                        }
                        if (_if == null) { _if = temp; }
                        else
                        {
                            temp.IsElseIf = true;
                            _if.Else.Add(temp);
                        }
                    }
                    else
                    {
                        _if.Else.Add(temp);
                    }
                    // break表达式全部要改为return到当前if块结束
                    BreakToEmpty.Change(item.Body);
                    temp.Body = item.Body;
                }
                // 采用if块形式实现switch
                this.Visit(_if);
            }
            else
            {
                States.Add(node);
            }
        }
        //public override void Visit(CaseLabel node)
        //{
        //}
        public override void Visit(BreakStatement node) { States.Add(node); }
        public override void Visit(ContinueStatement node) { States.Add(node); }
        public override void Visit(ReturnStatement node) { throw new NotImplementedException(); }
        public override void Visit(YieldBreakStatement node)
        {
            //SetState(new DefaultValue() { Reference = new ReferenceMember(new Named("undifined")) });
            States.Add(ReturnState(-2));
        }
        public override void Visit(YieldReturnStatement node)
        {
            SetState(node.Value);
            States.Add(ReturnState(-1));
        }
        public override void Visit(GotoStatement node) { throw new NotImplementedException(); }
        public override void Visit(ThrowStatement node) { States.Add(node); }
        public override void Visit(UnsafeStatement node) { throw new NotImplementedException(); }
        public override void Visit(LockStatement node) { throw new NotImplementedException(); }
        public override void Visit(UsingStatement node) { throw new NotImplementedException(); }
        public override void Visit(FixedStatement node) { throw new NotImplementedException(); }
        public override void Visit(TryCatchFinallyStatement node) { throw new NotImplementedException("暂不支持在迭代器中使用try关键字"); }

        public override void Visit(InvokeMethod node) { States.Add(node); }
        public override void Visit(New node) { States.Add(node); }
        public override void Visit(UnaryOperator node) { States.Add(node); }
        public override void Visit(BinaryOperator node) { States.Add(node); }
        public override void Visit(FieldLocal node)
        {
            BinaryOperator bop;
            if (node.Value != null)
            {
                bop = new BinaryOperator();
                //bop.Left = new ReferenceMember(node.Name);
                bop.Left = WithThis(node.Name);
                bop.Operator = EBinaryOperator.Assign;
                bop.Right = node.Value;
                States.Add(bop);
            }

            foreach (var item in node.Multiple)
            {
                if (item.Value != null)
                {
                    bop = new BinaryOperator();
                    //bop.Left = new ReferenceMember(item.Name);
                    bop.Left = WithThis(item.Name);
                    bop.Operator = EBinaryOperator.Assign;
                    bop.Right = item.Value;
                    States.Add(bop);
                }
            }
        }
    }
    public class ShortNameProvider
    {
        static char[] EMPTY = new char[0];
        static ShortNameProvider codeNameProvider;
        public static ShortNameProvider CodeNameProvider
        {
            get
            {
                if (codeNameProvider == null)
                {
                    var chars = GetChars('A', 'Z');
                    var chars2 = GetChars('a', 'z'); ;
                    codeNameProvider = new ShortNameProvider();
                    codeNameProvider.chars = chars;
                    codeNameProvider.Concat(chars2);
                }
                return codeNameProvider;
            }
        }
        internal static HashSet<string> KEYWORD = new HashSet<string>();
        static ShortNameProvider()
        {
            KEYWORD.Add("do");
            KEYWORD.Add("in");
            KEYWORD.Add("if");
            KEYWORD.Add("is");
            KEYWORD.Add("as");
            KEYWORD.Add("var");
            KEYWORD.Add("for");
            KEYWORD.Add("new");
            KEYWORD.Add("out");
            KEYWORD.Add("try");
            KEYWORD.Add("void");
            KEYWORD.Add("base");
            KEYWORD.Add("this");
            KEYWORD.Add("case");
            KEYWORD.Add("else");
            KEYWORD.Add("goto");
            KEYWORD.Add("null");
            KEYWORD.Add("true");
            KEYWORD.Add("false");
            KEYWORD.Add("while");
            KEYWORD.Add("break");
            KEYWORD.Add("catch");

            KEYWORD.Add("bool");
            KEYWORD.Add("byte");
            KEYWORD.Add("sbyte");
            KEYWORD.Add("char");
            KEYWORD.Add("ushort");
            KEYWORD.Add("short");
            KEYWORD.Add("uint");
            KEYWORD.Add("int");
            KEYWORD.Add("float");
            KEYWORD.Add("ulong");
            KEYWORD.Add("long");
            KEYWORD.Add("double");
        }
        // 命名用的字符在chars中的索引
        private int[] nameIndex = new int[4];
        // 命名用的字符
        private char[] nameChar = new char[4];
        // 当前命名的名字长度，从0开始
        private int length;
        // 正在命名的字符在nameChar中的索引
        private int renameIndex;
        // 用于命名可用的字符
        private char[] chars;
        private ShortNameProvider() { }
        public ShortNameProvider(char[] chars)
        {
            if (chars == null)
                throw new ArgumentNullException("chars");
            this.chars = chars;
        }
        public ShortNameProvider(char asciiS, char asciiE)
        {
            this.chars = GetChars(asciiS, asciiE);
        }
        public ShortNameProvider(ShortNameProvider copy)
        {
            this.chars = copy.chars;
        }
        public void Concat(char[] chars)
        {
            if (chars == null)
                throw new ArgumentNullException();
            if (chars.Length == 0)
            {
                this.chars = chars;
                return;
            }
            int length = this.chars.Length;
            Array.Resize(ref this.chars, length + chars.Length);
            Array.Copy(chars, 0, this.chars, length, chars.Length);
        }
        public void CheckRepeat()
        {
            HashSet<char> set = new HashSet<char>();
            for (int i = 0; i < chars.Length; i++)
                if (!set.Add(chars[i]))
                    throw new ArgumentException("After repeat: " + chars[i]);
        }
        public void Clear()
        {
            Clear(nameChar.Length);
        }
        public void Clear(int minCharCount)
        {
            if (minCharCount < 1)
                minCharCount = 1;
            if (nameChar.Length != minCharCount)
            {
                nameChar = new char[minCharCount];
                nameIndex = new int[minCharCount];
            }
            else
            {
                Array.Clear(nameChar, 0, minCharCount);
                Array.Clear(nameIndex, 0, minCharCount);
            }
            renameIndex = 0;
            length = 0;
        }
        public string Provide()
        {
            if (chars.Length == 0)
                throw new InvalidOperationException("No name chars.");
            while (true)
            {
                if (nameIndex[renameIndex] == chars.Length)
                {
                    Array.Clear(nameChar, renameIndex, length - renameIndex + 1);
                    Array.Clear(nameIndex, renameIndex, length - renameIndex + 1);
                    // 已经命名到最后的字母
                    renameIndex--;
                    if (renameIndex == -1)
                    {
                        // 字符数已经用完，增加字符个数
                        length++;
                        if (length >= nameChar.Length)
                        {
                            Array.Resize(ref nameChar, length + 2);
                            Array.Resize(ref nameIndex, length + 2);
                        }
                        renameIndex = 0;
                    }
                    continue;
                }
                else
                {
                    nameChar[renameIndex] = chars[nameIndex[renameIndex]++];
                    if (renameIndex < length)
                    {
                        // AX, AY, AZ => BA 此时命名A=>B，++代表紧接着是Z=>B
                        renameIndex++;
                    }
                    else
                    {
                        string result = new string(nameChar, 0, length + 1);
                        if (KEYWORD.Contains(result))
                        {
                            continue;
                        }
                        return result;
                    }
                }
            }
        }
        public static char[] GetChars(char asciiS, char asciiE)
        {
            if (asciiS == asciiE)
                return EMPTY;
            if (asciiS > asciiE)
                throw new ArgumentOutOfRangeException("");
            int count = asciiE - asciiS + 1;
            char[] result = new char[count];
            for (int i = 0; i < count; i++)
                result[i] = (char)(asciiS + i);
            return result;
        }
    }
    /// <summary>
    /// todo: 按类型，成员区分作用域，进一步让类型，成员，临时变量的名字长度更短
    /// </summary>
    internal class Renamer : SyntaxWalker
    {
        private Renamer() { }
        Rewriter processor;
        ShortNameProvider tprovider = new ShortNameProvider('A','Z');
        ShortNameProvider mprovider = ShortNameProvider.CodeNameProvider;
        Dictionary<CSharpType, string> renamedTypes = new Dictionary<CSharpType,string>();
        Dictionary<CSharpMember, string> renamedMembers = new Dictionary<CSharpMember,string>();
        private void Check(SyntaxNode node)
        {
            BEREF beref;
            DefinedType define;
            if (processor.types.TryGetValue(node, out define))
            {
                var type = define.Type;
                if (type.Assembly.IsProject && processor.HasType(type, out beref))
                {
                    if (!type.Attributes.Any(a => a.Type.Name.Name == AInvariant.Name))
                    {
                        // 重命名类型
                        renamedTypes.Add(type, type.Name.Name);
                        //string name = CodeNameProvider.GetName();
                        string name = GetName(type.Name.Name, true);
                        type.Name.Name = name;
                        foreach (var refName in beref.References)
                            if (refName.Reference != null)
                                refName.Reference.Name = name;
                    }

                    // 重命名成员
                    foreach (var member in type.MemberDefinitions)
                    {
                        if (member.IsOverride || member.ExplicitInterfaceImplementation != null || member.IsExtern)
                            continue;

                        BEREF beref2;
                        if (!processor.HasMember(member, out beref2))
                            continue;

                        // 隐式实现接口的成员也应该不修改名字
                        if (CSharpType.GetAllBaseInterfaces(type.BaseInterfaces).SelectMany(i => i.MemberDefinitions).Any(m => 
                            m.Name == member.Name && CSharpParameter.Equals(m.Parameters, member.Parameters)))
                            continue;

                        if (member.Attributes.Any(a => a.Type.Name.Name == AInvariant.Name))
                            continue;

                        // 构造函数，索引器，方法：可重载，所以反射时必须使用修改过后的名字
                        //if (member.IsField || member.IsProperty)
                        renamedMembers.Add(member, member.Name.Name);
                        //string name = CodeNameProvider.GetName();
                        string name = GetName(member.Name.Name, false);
                        member.Name.Name = name;
                        foreach (var refName in beref2.References)
                            if (refName.Reference != null)
                                refName.Reference.Name = name;
                    }
                }
            }
        }
        public override void Visit(DefineType node)
        {
            Check(node);
            foreach (var item in node.NestedType) Visit(item);
        }
        public override void Visit(DefineEnum node)
        {
            Check(node);
        }
        public static void Rename(IEnumerable<DefineFile> files, Rewriter processor, out Dictionary<CSharpType, string> renamedTypes, out Dictionary<CSharpMember, string> renamedMembers)
        {
            if (processor == null || files == null)
                throw new InvalidOperationException();
            Renamer renamer = new Renamer();
            renamer.processor = processor;
            renamer.tprovider.Concat(ShortNameProvider.GetChars('a', 'q'));
            foreach (var item in files)
                if (CodeFileHelper.ParseFileType(item.Name) != EFileType.Define)
                    renamer.Visit(item);
            renamedTypes = renamer.renamedTypes;
            renamedMembers = renamer.renamedMembers;
        }
        public static void Rename(object obj, Rewriter processor, string name)
        {
            BEREF beref;
            if (processor.HasReference(obj, out beref))
            {
                foreach (var item in beref.References)
                {
                    if (item.Reference != null)
                        item.Reference.Name = name;
                }
            }
            CSharpType type = obj as CSharpType;
            if (type != null)
            {
                type.Name.Name = name;
            }
            else
            {
                CSharpMember member = obj as CSharpMember;
                if (member != null)
                {
                    member.Name.Name = name;
                }
                else
                {
                    VAR _var = obj as VAR;
                    if (_var != null)
                    {
                        _var.Name.Name = name;
                    }
                    else
                    {
                        CSharpNamespace _namespace = obj as CSharpNamespace;
                        if (_namespace != null)
                        {
                            _namespace.Name = name;
                        }
                        else
                        {
                            throw new InvalidCastException();
                        }
                    }
                }
            }
        }

        string GetName(string name, bool type)
        {
            char c = name[0];
            if (c == '_' || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
            {
                int count;
                if (!testRename.TryGetValue(name, out count))
                    count = 0;
                count++;
                testRename[name] = count;
                return name + count;
            }
            else
            {
                if (type)
                    return tprovider.Provide();
                else
                    return mprovider.Provide();
            }
        }
        static Dictionary<string, int> testRename = new Dictionary<string, int>();
    }
    internal class ReflexibleTypes : SyntaxWalker
    {
        private ReflexibleTypes() { }
        List<CSharpType> reflexibleTypes = new List<CSharpType>();
        Rewriter processor;
        bool all;
        private void Check(SyntaxNode node)
        {
            DefinedType define;
            if (processor.types.TryGetValue(node, out define))
            {
                CSharpType type = define.Type;
                if (processor.HasType(type) && (all || (type.Assembly.IsProject && type.Attributes.Any(a => a.Type.Name.Name == AReflexible.Name || a.Type.Name.Name == "Serializable"))))
                    reflexibleTypes.Add(type);
            }
        }
        public override void Visit(DefineFile node)
        {
            // 文件里的所有类型都生成程序集信息
            all = node.Name.EndsWith(".r.cs");
            //all = true;
            base.Visit(node);
        }
        public override void Visit(DefineType node)
        {
            Check(node);
            foreach (var item in node.NestedType) Visit(item);
        }
        public override void Visit(DefineEnum node)
        {
            Check(node);
        }
        public static List<CSharpType> Reflexible(IEnumerable<DefineFile> files, Rewriter processor)
        {
            if (processor == null || files == null)
                throw new InvalidOperationException();
            ReflexibleTypes obj = new ReflexibleTypes();
            obj.processor = processor;
            foreach (var item in files)
                obj.Visit(item);
            return obj.reflexibleTypes;
        }
    }
    public class JSRewriter : Rewriter
    {
        enum ES
        {
            function,
            prototype,
            _this,
        }
        static Dictionary<ES, string> S = new Dictionary<ES, string>();

        static JSRewriter()
        {
            S[ES.function] = "function";
            S[ES.prototype] = ".prototype.";
            S[ES._this] = "this";

            //BOP[EBinaryOperator.Equality] = "===";
            //BOP[EBinaryOperator.Inequality] = "!==";
        }

        // List<int> => (List(int))
        const string GENERIC_NAME = "__g";
        //const string SUPER = "__base";
        const string AUTO_PROPERTY = "__";
        internal const string GET = "__get";
        internal const string SET = "__set";
        const string ADDRESS_VALUE = "v";
        // foreach (var item in enumrable) => var __ = enumerable.GetEnumerator(); while(__.MoveNext()) { var item = __.Current; ...body... }
        const string ENUMERABLE = "_e";
        // catch => catch(__ex)
        const string ANONYMOUS_EX = "__ex";
        // new A() { a = 5 } => (function(){var __obj=new A(); __obj.a=5; return __obj;})()
        const string ANONYMOUS_OBJ = "__obj";
        const string ANONYMOUS_RET = "__ret";
        // temp = n++;s => temp = (function() { var __temp = n; n = Type.op_Increment(n); return __temp; })()
        const string ANONYMOUS_OP = "__temp";
        const string ANONYMOUS_OP_TARGET = "$t1";
        // ref a => var __a = new ref(a)
        const string REF = "_r";
        static ReferenceMember REF_P = new ReferenceMember(new Named(null));
        const string TYPE_NAME = "_TN";
        const string INHERIT = "_TI";
        static ReferenceMember CONSTRUCTOR = new ReferenceMember(new Named("this.constructor"));
        const string STRUCT_COPY = "$copy2";
        const string STRUCT_CLONE = "$clone";
        const string TEMP_THIS = "$this";
        const string DEFAULT = "$dft";
        const string ENUM = "$enum";
        const string IS = "$is";
        const string AS = "$as";
        const string TYPE_OF = "$tof";
        const string ARRAY_TYPE = "$array";
        const string REF_OUT = "$ref";

        class FinderBaseMember : SyntaxWalker
        {
            private JSRewriter writer;
            private HashSet<CSharpMember> members = new HashSet<CSharpMember>();
            private FinderBaseMember() { }
            /// <returns>被显示base.调用的父类成员</returns>
            public static CSharpMember[] FindBaseMembers(DefineType type, JSRewriter writer)
            {
                FinderBaseMember finder = new FinderBaseMember();
                finder.writer = writer;
                finder.Visit(type);
                return finder.members.ToArray();
            }
            public override void Visit(DefineType node)
            {
                foreach (var item in node.Properties) Visit(item);
                foreach (var item in node.Methods) Visit(item);
            }
            public override void Visit(DefineProperty node)
            {
                if (node.Getter != null) Visit(node.Getter);
                if (node.Setter != null) Visit(node.Setter);
            }
            public override void Visit(Accessor node)
            {
                if (node.Body != null) Visit(node.Body);
            }
            public override void Visit(DefineMethod node)
            {
                if (node.Body != null) Visit(node.Body);
            }
            public override void Visit(ReferenceMember node)
            {
                var parent = node.Base;
                if (parent != null && parent.Name.Name == "base")
                {
                    var member = (CSharpMember)writer.syntax[node].Definition.Define;
                    // 可能member已经被优化掉了
                    if (member != null)
                        members.Add(member);
                }
                else
                {
                    if (node.Target != null)
                    {
                        Visit(node.Target);
                    }
                }
            }
        }
        /// <summary>寻找body中是否使用有this的内容，关系着委托方法是否需要bind(this)</summary>
        class ThisReference : SyntaxWalker
        {
            public static ThisReference ins;
            JSRewriter writer;
            bool result;
            public override void Visit(ReferenceMember node)
            {
                if (node.Name.Name == "this")
                {
                    result = true;
                    return;
                }

                REF _ref;
                writer.syntax.TryGetValue(node, out _ref);
                if (_ref == null)
                    return;

                if (node.Target == null)
                {
                    CSharpMember member = _ref.Definition.Define as CSharpMember;
                    if (member != null && !member.IsStatic && !member.IsConstant)
                    {
                        // 隐式调用类型自身包含的成员
                        result = true;
                    }
                }
                else
                {
                    Visit(node.Target);
                }
            }
            public override void Visit(Body node)
            {
                if (result)
                    return;
                if (node != null)
                {
                    foreach (var item in node.Statements)
                    {
                        Visit(item);
                        if (result)
                            return;
                    }
                }
            }
            public static bool Test(JSRewriter writer, Body body)
            {
                if (ins == null) ins = new ThisReference();
                ins.writer = writer;
                ins.result = false;
                ins.Visit(body);
                return ins.result;
            }
        }
        class YieldJS : IEnumeratorRebuilder
        {
            static JSRewriter rewriter;
            public static void Initialize(JSRewriter rewriter)
            {
                YieldJS.rewriter = rewriter;
                INS = new YieldJS();
            }
            private void FieldWithThis(FieldLocal node)
            {
                WithThis(node);
                foreach (var item in node.Multiple)
                    WithThis(item);
            }
            private void WithThis(SyntaxNode node)
            {
                REF _ref;
                if (rewriter.syntax.TryGetValue(node, out _ref))
                {
                    VAR _var = (VAR)_ref.Definition.Define;
                    if (!_var.Name.Name.StartsWith("this."))
                        Renamer.Rename(_var, rewriter, "this." + _var.Name.Name);
                }
            }
            protected override DefineField AddField(FieldLocal f)
            {
                FieldWithThis(f);
                return base.AddField(f);
            }
            public override void Visit(FieldLocal node)
            {
                FieldWithThis(node);
                base.Visit(node);
            }
            public override void Visit(ForeachStatement node)
            {
                bool hasYield = IEnumeratorTester.TestYield(node.Body);
                if (hasYield)
                    WithThis(node);
                base.Visit(node);
            }
            public override void Visit(ForStatement node)
            {
                bool hasYield = IEnumeratorTester.TestYield(node.Body);
                if (hasYield)
                    foreach (var item in node.Initializers)
                        if (item is FieldLocal)
                            FieldWithThis((FieldLocal)item);
                base.Visit(node);
            }
        }
        StringBuilder beginWriter;
        StringBuilder swapWriter;
        StringBuilder endWriter = new StringBuilder();
        StringBuilder endWriter2 = new StringBuilder();
        string paramArgument;
        int paramArgumentOffset;
        Stack<string> typeNames = new Stack<string>();
        /// <summary>作用域深度，也就是{}的数量</summary>
        int scopeDepth = 0;
        /// <summary>body块开始时追加的文本</summary>
        StringBuilder inBody = new StringBuilder();
        /// <summary>对代码生成设置临时索引，以便通过调用GetLastWrittenExpression获取临时索引以后生成的代码</summary>
        int tempIndex = -1;
        /// <summary>new VECTOR2() { X = 5 }这样构造时，X不默认增加this.</summary>
        bool initializing = false;
        List<CSharpType> reflexibleTypes = new List<CSharpType>();
        Dictionary<CSharpType, string> renamedTypes = new Dictionary<CSharpType, string>();
        Dictionary<CSharpMember, string> renamedMembers = new Dictionary<CSharpMember, string>();
        ShortNameProvider nameProvider = new ShortNameProvider('r','z');
        New refNewNode;
        /// <summary>constructor() : this()，记录这个this，因为只有它能找到相应的引用</summary>
        SyntaxNode constructorTarget;
        /// <summary>扩展方法this参数的节点</summary>
        SyntaxNode exThisParam;
        bool inYieldEnumerable = false;
        bool isStaticYield = false;
        int assignOperator;

        string TypeName
        {
            get { return typeNames.Count == 0 ? null : typeNames.Peek(); }
        }
        string ParentTypeName
        {
            get
            {
                if (typeNames.Count < 2)
                    return null;
                else
                {
                    string temp = typeNames.Pop();
                    string result = typeNames.Peek();
                    typeNames.Push(temp);
                    return result;
                }
            }
        }
        /// <summary>泛型类内部的成员当场生成代码，顶级类型的静态字段和继承信息放在最后生成</summary>
        StringBuilder Builder
        {
            get
            {
                var type = DefiningType;
                while (type != null)
                {
                    if (type.TypeParametersCount > 0)
                        return swapWriter;
                    type = type.ContainingType;
                }
                return endWriter;
            }
        }
        bool MultiAssign { get { return assignOperator > 1; } }
        public override string Result
        {
            get
            {
                var result = new StringBuilder();
                result.Append(beginWriter);
                result.Append(builder);
                result.Append(endWriter);
                result.Append(endWriter2);
                return result.ToString();
            }
        }

        void WriteInvokeBaseDefaultConstructor(string name, bool isExplicit)
        {
            if (isExplicit)
                builder.AppendLine("this.constructor = {0};", name);
            else
                builder.AppendLine("this.constructor = {0}.$;", name);
            builder.AppendLine("this.constructor();");
            //builder.AppendLine("delete this.{0};", SUPER);
        }
        void WriteValueOrDefault(SyntaxNode node)
        {
            if (node == null)
                // 没有赋值需要手动赋上默认值
                builder.Append(GetDefaultValueCode(DefiningMember.ReturnType));
            else
                Visit(node);
            builder.AppendLine(";");
        }
        void WriteParamsArgument()
        {
            if (!string.IsNullOrEmpty(paramArgument))
            {
                builder.AppendLine("var {0} = arguments[{1}] instanceof Array ? arguments[{1}] : Array.prototype.slice.apply(arguments).slice({1});", paramArgument, paramArgumentOffset);
                paramArgument = null;
            }
            paramArgumentOffset = 0;
        }
        void SetTempIndex()
        {
            tempIndex = builder.Length;
        }
        string GetLastWrittenExpression()
        {
            if (tempIndex == -1)
                throw new InvalidOperationException();
            char[] expBuffer = new char[builder.Length - tempIndex];
            builder.CopyTo(tempIndex, expBuffer, 0, expBuffer.Length);
            string expression = new string(expBuffer);
            tempIndex = -1;
            return expression;
        }
        string GetLastWrittenExpression(Action write)
        {
            SetTempIndex();
            write();
            return GetLastWrittenExpression();
        }
        string PeekLastWrittenExpression(Action write)
        {
            SetTempIndex();
            int temp = builder.Length;
            write();
            int temp2 = builder.Length;
            // 有可能write内改变了tempIndex
            if (tempIndex == -1)
                tempIndex = temp;
            string result = GetLastWrittenExpression();
            builder.Remove(temp, temp2 - temp);
            return result;
        }
        bool HasRefParam(InvokeMethod node)
        {
            return node.Arguments != null && node.Arguments.Count > 0 && node.Arguments.Any(n => n.Direction == EDirection.REF || n.Direction == EDirection.OUT);
        }
        void RenameLocalField(SyntaxNode node)
        {
            REF _ref;
            if (syntax.TryGetValue(node, out _ref))
            {
                // BUG: 各种
                Renamer.Rename(_ref.Definition.Define, this, nameProvider.Provide());
            }
        }
        /// <summary>builder替换成临时的StringBuilder进行写入操作</summary>
        void FixedBuilder(Action action, StringBuilder fix)
        {
            var temp2 = builder;
            builder = fix;
            action();
            builder = temp2;
        }
        bool IsJSArray(CSharpMember member)
        {
            return member.IsIndexer && member.IsExtern;
            //return member.ContainingType.Name.Name.EndsWith("ClampedArray") 
            //    || (member.ContainingType.Name.Name == "ArrayBuffer" 
            //    || (member.ContainingType.BaseClass != null 
            //        && (member.ContainingType.BaseClass.Name.Name == "ArrayBuffer"
            //            || member.ContainingType.BaseClass.Name.Name == "Array")));
        }

        // BUG todo
        // 1. ref或out参数 spriteQueue[this.spriteQueueCount] = __1.Release()的部分，在调用方法内部，改变了spriteQueueCount的值导致对象覆盖
        // 2. array.GetType().GetElementType()无法获得类型
        // 3. DateTime内部改用int，相对于1970/01/01的时间
        // OK 5. yield块的普通for循环，int i = 0; i < 100 => var i = 0; [this.]i < 100导致错误
        // 6. uint颜色值右移位运算会是int计算，不过&255后值还算正确，右移最好用>>>而非>>
        // 7. JS关键字用1个字节标识，集压缩和加密于一体
        // 8. 将反射生成代码需要用到的类型和成员在改名前记录下来，取消所有AInvariant特性，进一步缩减代码

        // 可能存在的问题
        // 1. string str = null; str += "Dir" => "nullDir"，C#里是"Dir"
        // 2. string str = 'a' + "abc" => "97abc"   应写成'a'.ToString()
        // 3. if (base.Base == null) Base = Texture; 此时Base = Texture在闭包内生成的var temp = this.__getBase()将导致死循环   应写成base.Base = Texture

        public override void Visit(DefineFile node) { }
        public override void Visit(DefineNamespace node) { }
        public override void Visit(List<DefineGenericArgument> node)
        {
            /*
             * 泛型类特殊处理
             * List<int>._emptyList和List<float>._emptyList会是不同的类型和实例
             */
        }
        public override void Visit(Constraint node) { }
        void AppendPrototype(DefineType node, string targetType)
        {
            var temp = builder;
            builder = beginWriter;
            typeNames.Push(targetType);
            WriteReflectionInfo(null, false);
            foreach (var item in node.Fields.Where(f => (f.Modifier & EModifier.Static) != EModifier.None || (f.Modifier & EModifier.Const) != EModifier.None)) Visit(item);
            foreach (var item in node.Properties) Visit(item);
            foreach (var item in node.Methods.Where(m => m is DefineMethod)) Visit(item);
            foreach (var item in node.NestedType) Visit(item);
            typeNames.Pop();
            builder = temp;
        }
        void WriteReflectionInfo(string specialTypeName, bool constructed)
        {
            //if (reflexibleTypes.Contains(DefiningType))
            {
                if (constructed && DefiningType.TypeParametersCount > 0)
                {
                    builder.Append("{0}.{1}2 = function(){{ return {2}.{1} + ", TypeName, TYPE_NAME, ParentTypeName);
                    //builder.Append("{0}.{1} = {2}.{1} + ", TypeName, TYPE_NAME, ParentTypeName);
                    // IList^1[----]
                    builder.Append("\"[[\" + ");
                    var types = DefiningType.TypeParameters;
                    for (int i = 0, n = types.Count - 1; i <= n; i++)
                    {
                        builder.Append("CSharpType.GetRuntimeTypeName({1}({0}).type)", types[i].Name.Name, TYPE_OF);
                        if (i != n)
                        {
                            builder.Append(" + \"],[\" + ");
                        }
                    }
                    builder.AppendLine(" + \"]]\"; };");
                }
                else
                {
                    builder.AppendLine("{0}.{1} = \"{2}{3}\";", TypeName, TYPE_NAME, GetRenamedRuntimeTypeName(DefiningType), specialTypeName);
                }
            }
        }
        protected override void VisitTypeObject(DefineType type)
        {
            AppendPrototype(type, "Object");
            builder.AppendLine("Object.prototype.GetType = function() { return $tof(this.constructor); };");
            //builder.AppendLine("Object.prototype.ToString = function() { return this.toString(); };");
            builder.AppendLine("Object.prototype.GetHashCode = function() { if (!this.$hash) this.$hash = new Date().getTime() & $H32; return this.$hash; };");
        }
        protected override void VisitTypeString(DefineType type)
        {
            AppendPrototype(type, "String");
            builder.AppendLine("String.Create = function(chars)");
            builder.AppendBlockWithEnd(() =>
            {
                builder.AppendLine("var n = chars.length;");
                builder.AppendLine("var c0 = (n / 5)|0;");
                builder.AppendLine("var c1 = n % 5;");
                builder.AppendLine("var strs = new Array(c0 + c1);");
                builder.AppendLine("var i0 = 0;");
                builder.AppendLine("for (var i = 4; i < n; i += 5) strs[i0++] = String.fromCharCode(chars[i - 4], chars[i - 3], chars[i - 2], chars[i - 1], chars[i]);");
                builder.AppendLine("for (var i = c0 * 5; i < n; i++) strs[i0++] = String.fromCharCode(chars[i]);");
                builder.AppendLine("return strs.join(\"\");");
            });
        }
        protected override void VisitTypeArray(DefineType type)
        {
            AppendPrototype(type, "Array");
            builder.AppendLine("Array.CreateInstance = function(et, len) { return new Array(len); };");
            // Array.Clear
            //builder.AppendLine("Array.Clear(array, index, length)");
            //builder.AppendBlock(() =>
            //{
            //    builder.AppendLine("for (var i = index, n = index + length; i < n; i++)");
            //    builder.AppendLine("array[i] = $dtf();");
            //});
        }
        protected override void VisitTypeMath(DefineType type)
        {
            AppendPrototype(type, "Math");
        }
        bool IsCanInherit(CSharpType type)
        {
            if (type == null || type == CSharpType.OBJECT || type == CSharpType.ARRAY || type.IsPrivate || IsDefineOnly(type))
                return false;
            return true;
        }
        protected override void OnSetMember(CSharpMember member)
        {
            nameProvider.Clear(1);
        }
        protected override void Write(DefineType node)
        {
            /*
             * [Type] = function(){} // 定义类型
             * [Type].[Constructor] = function(){}
             * [Type].prototype.[Member] = [Value] // 如果这里是数组这样的引用类型，所以对字段的赋值还是放在构造函数里比较好
             * [Type].[Constructor].prototype = [Type].prototype;
             * 
             * [ChildType].prototype = new [Type];
             * [ChildType].prototype.[Member] = [Value];
             */

            CSharpType type = DefiningType;
            bool isPrimitive = CSharpType.IsPrimitive(type);

            typeNames.Push(type.Name.Name);

            FixedBuilder(() =>
            {
                #region 泛型外包类型
                // 通过定义一个方法传入泛型类型来动态创建具体类型的泛型类实例
                if (node.Generic.IsGeneric)
                {
                    // 泛型类没必要真假原型
                    builder.Append("function {0}(", TypeName);
                    for (int i = 0, n = node.Generic.GenericTypes.Count - 1; i <= n; i++)
                    {
                        var item = node.Generic.GenericTypes[i];
                        builder.Append(item.Name.Name);
                        if (i != n)
                            builder.Append(", ");
                    }
                    builder.AppendLine(")");
                    builder.AppendLine("{");
                    // 类型缓存
                    builder.AppendLine("if (!{0}.types) {0}.types=[];", TypeName);
                    builder.AppendLine("var __i;");
                    builder.AppendLine("for (__i = 0; __i < {0}.types.length; __i++)", TypeName);
                    builder.AppendBlock(() =>
                    {
                        builder.AppendLine("var __array = {0}.types[__i];", TypeName);
                        for (int i = 0, n = node.Generic.GenericTypes.Count - 1; i <= n; i++)
                        {
                            var item = node.Generic.GenericTypes[i];
                            builder.AppendLine("if (__array.keys[{0}] != {1}) continue;", i, item.Name.Name);
                        }
                        builder.AppendLine("break;");
                    });
                    builder.AppendLine("if (__i < {0}.types.length) return {0}.types[__i].value;", TypeName);
                    // 根据泛型创建一个带入实际类型的类，这个类名可以是固定的
                    //typeNames.Push(GENERIC_NAME + type.Depth);
                    typeNames.Push(GENERIC_NAME);
                }
                #endregion

                // 默认构造函数来定义类型
                var baseClass = type.BaseClass;
                if (baseClass == CSharpType.OBJECT || baseClass == CSharpType.VALUE_TYPE)
                    baseClass = null;

                CSharpMember[] baseMembers = FinderBaseMember.FindBaseMembers(node, this);
                // 真原型：不带任何字段，但是prototype指向这个类型，继承时也new这个原型类型
                builder.AppendLine("function {0}(){{}}", TypeName);
                // 反射的程序集信息
                //if (!node.Generic.IsGeneric)
                WriteReflectionInfo(null, true);
                //WriteInheritRelation();

                // 接口：KEYBOARD : Input<IKeyboardState>，保留接口定义使其不报错即可
                if (node.Type != EType.INTERFACE)
                {
                    if (!type.IsStatic && !isPrimitive)
                    {
                        // 假原型：用于定义类型有哪些字段；没有显示声明构造函数时也是类型的默认构造函数
                        builder.AppendLine("{0}.$ = function()", TypeName);
                        builder.AppendBlockWithEnd(() =>
                        {
                            // 针对构造函数内有调用抽象方法的（例如_RANDOM.Random），需要先构造相应的方法，所以将调用构造函数放在方法的下方
                            CSharpMember constructorThis = DefiningType.MemberDefinitions.FirstOrDefault(m => m.IsConstructor && !m.IsStatic);
                            if (baseClass != null)
                            {
                                string baseClassName = GetTypeName(baseClass);
                                // 自身没有显示声明的构造函数时作为类型默认的构造函数
                                if (constructorThis == null && IsCanInherit(baseClass))
                                {
                                    CSharpMember constructor = baseClass.MemberDefinitions.FirstOrDefault(m => m.IsConstructor && !m.IsStatic && m.Parameters.Count == 0);
                                    // 有继承类型且继承类型有显示声明的构造函数，则应该默认调用那个构造函数
                                    if (constructor != null)
                                        WriteInvokeBaseDefaultConstructor(string.Format("{0}.{1}", baseClassName, constructor.Name.Name), true);
                                    else
                                        WriteInvokeBaseDefaultConstructor(baseClassName, false);
                                }
                                // base.调用的成员需要生成[base_成员]作为类型的成员保留
                                for (int i = 0; i < baseMembers.Length; i++)
                                    if (baseMembers[i].IsField)
                                        throw new InvalidCastException("不应与base的字段重名");
                                    else if (baseMembers[i].IsProperty || baseMembers[i].IsIndexer)
                                    {
                                        builder.AppendLine("this._{0}{1}{2} = {3}.prototype.{1}{2};", GetInheritDepth(baseClass), GET, baseMembers[i].Name.Name, baseClassName);
                                        builder.AppendLine("this._{0}{1}{2} = {3}.prototype.{1}{2};", GetInheritDepth(baseClass), SET, baseMembers[i].Name.Name, baseClassName);
                                    }
                                    else
                                        builder.AppendLine("this._{0}{2} = {1}.prototype.{2};", GetInheritDepth(baseClass), baseClassName, baseMembers[i].Name.Name);
                            }
                            if (constructorThis == null)
                                // constructor一定指向真原型
                                builder.AppendLine("this.constructor = {0};", TypeName);
                            // 实例字段
                            foreach (var item in node.Fields.Where(f => (f.Modifier & EModifier.Static) == EModifier.None && (f.Modifier & EModifier.Const) == EModifier.None))
                                Visit(item);
                            // 自动属性的隐式字段
                            foreach (var item in node.Properties.Where(n => n.IsAuto && (n.Modifier & EModifier.Static) == EModifier.None))
                            {
                                if (!SetMember(item))
                                    continue;
                                builder.Append("this.{0}{1} = ", AUTO_PROPERTY, item.Name.Name);
                                builder.Append(GetDefaultValueCode(DefiningMember.ReturnType));
                                builder.AppendLine(";");
                            }
                            // todo:考虑将委托右边，方法参数加上.bind(this)，就不需要在构造函数里bind所有方法了
                            //foreach (var item in node.Properties.Where(m => (m.Modifier & EModifier.Static) == EModifier.None))
                            //{
                            //    if (!SetMember(item))
                            //        continue;
                            //    // 有可能abstract override
                            //    if (!DefiningMember.IsAbstract && DefiningMember.IsOverride)
                            //        continue;
                            //    if (item.Getter != null)
                            //        builder.AppendLine("this.{0}{1} = this.{0}{1}.bind(this);", GET, DefiningMember.Name.Name);
                            //    if (item.Setter != null)
                            //        builder.AppendLine("this.{0}{1} = this.{0}{1}.bind(this);", SET, DefiningMember.Name.Name);
                            //}
                            //foreach (DefineMethod item in node.Methods.Where(m => m is DefineMethod && (m.Modifier & EModifier.Static) == EModifier.None))
                            //{
                            //    if (!SetMember(item))
                            //        continue;
                            //    // 有可能abstract override
                            //    if (!DefiningMember.IsAbstract && DefiningMember.IsOverride)
                            //        continue;
                            //    builder.AppendLine("this.{0} = this.{0}.bind(this);", DefiningMember.Name.Name);
                            //}
                        });
                        if (baseClass != null)
                        {
                            // 继承父类型 [new 父类型()]
                            if (node.Generic.IsGeneric)
                            {
                                builder.AppendLine("{0}.prototype = new {1}();", TypeName, GetTypeName(baseClass));
                            }
                            else
                            {
                                //var tempBuilder = builder;
                                //builder = beginWriter;

                                //builder.AppendLine("window.$sp{0} = function()", TypeName);
                                //builder.AppendBlockWithEnd(() =>
                                //{
                                //    builder.AppendLine("if ($sp{0}.$initd) return;", TypeName);
                                //    builder.AppendLine("$sp{0}.$initd = true;", TypeName);
                                //    if (baseClass.TypeArguments.Count == 0)
                                //    {
                                //        // A:B B:C 若生成顺序为A>B>C，那么A.prototype=new B(); B.prototype=new C();，最终A将不继承C
                                //        // 调用父类型的设置prototype函数来先设置父类型的继承关系
                                //        builder.AppendLine("if (window.$sp{0}) $sp{0}();", baseClass.Name.Name);
                                //        //builder.AppendLine("delete $sp{0};", baseClass.Name.Name);
                                //        builder.AppendLine("window.$sp{0} = undefined;", baseClass.Name.Name);
                                //    }
                                //    builder.AppendLine("{0}.prototype = new {1}();", TypeName, GetTypeName(baseClass));
                                //});

                                //builder = tempBuilder;

                                //builder.AppendLine("if (window.$sp{0}) $sp{0}();", TypeName);

                                // 类型的生成顺序已经根据引用关系排序了，直接指定prototype即可
                                builder.AppendLine("{0}.prototype = new {1}();", TypeName, GetTypeName(baseClass));
                            }
                        }
                        builder.AppendLine("{0}.$.prototype = {0}.prototype;", TypeName);
                    }

                    if (type.ContainingType != null)
                        builder.AppendLine("{0}.{1} = {1};", ParentTypeName, TypeName);

                    // 结构体生成copy和clone，可以进行深度克隆
                    if (type.IsStruct && !isPrimitive)
                    {
                        // copy
                        builder.AppendLine("{0}.prototype.{1} = function(t)", TypeName, STRUCT_COPY);
                        builder.AppendBlockWithEnd(() =>
                        {
                            // 实例字段
                            foreach (var item in node.Fields.Where(f => (f.Modifier & EModifier.Static) == EModifier.None && (f.Modifier & EModifier.Const) == EModifier.None))
                            {
                                if (!SetMember(item))
                                    continue;
                                if (DefiningMember.ReturnType.IsStruct && !CSharpType.IsPrimitive(DefiningMember.ReturnType))
                                    builder.AppendLine("this.{0}.{1}(t.{0});", item.Name.Name, STRUCT_COPY);
                                else
                                    builder.AppendLine("t.{0} = this.{0};", item.Name.Name);
                            }
                            // 自动属性的隐式字段
                            foreach (var item in node.Properties.Where(n => n.IsAuto && (n.Modifier & EModifier.Static) == EModifier.None))
                            {
                                if (!SetMember(item))
                                    continue;
                                if (DefiningMember.ReturnType.IsStruct && !CSharpType.IsPrimitive(DefiningMember.ReturnType))
                                    builder.AppendLine("this.{0}{1}.{2}(t.{0}{1});", AUTO_PROPERTY, item.Name.Name, STRUCT_COPY);
                                else
                                    builder.Append("t.{0}{1} = this.{0}{1};", AUTO_PROPERTY, item.Name.Name);
                            }
                        });
                        // clone
                        builder.AppendLine("{0}.prototype.{1} = function()", TypeName, STRUCT_CLONE);
                        builder.AppendBlockWithEnd(() =>
                        {
                            builder.AppendLine("var t = new (({0}).$)();", TypeName);
                            builder.AppendLine("this.{0}(t);", STRUCT_COPY);
                            builder.AppendLine("return t;");
                        });
                    }

                    // 属性
                    foreach (var item in node.Properties)
                    {
                        bool isStatic = (item.Modifier & EModifier.Static) != EModifier.None;
                        //if (isStatic || !((item.Getter != null && item.Getter.Body != null && ThisReference.Test(this, item.Getter.Body)) ||
                        //    (item.Setter != null && item.Setter.Body != null && ThisReference.Test(this, item.Setter.Body))))
                        {
                            Visit(item);
                            // 静态自动属性的隐式字段
                            if (item.IsAuto && isStatic)
                            {
                                builder.Append("{0}.{1}{2} = ", TypeName, AUTO_PROPERTY, item.Name.Name);
                                builder.Append(GetDefaultValueCode(DefiningMember.ReturnType));
                                builder.AppendLine(";");
                            }
                        }
                    }
                    // 构造函数
                    foreach (DefineConstructor item in node.Methods.Where(m => m is DefineConstructor && (m.Modifier & EModifier.Static) == EModifier.None)) Visit(item);
                    // 记录提供一个默认构造函数以便Activator.CreateInstance调用
                    if (((type.IsStruct && !isPrimitive) || (type.IsClass && !type.IsStatic && !type.IsAbstract)))
                    {
                        var constructor = DefiningType.MemberDefinitions.FirstOrDefault(m => m.IsConstructor && !m.IsStatic && m.Parameters.Count == 0);
                        if (constructor != null)
                        {
                            //builder.AppendLine("{0}.$ctor = {0}.{1};", TypeName, constructor.Name.Name);
                            builder.AppendLine("_R.$TC[\"{0}\"] = {1}.{2};", GetRenamedRuntimeTypeName(type), TypeName, constructor.Name.Name);
                        }
                        else
                        {
                            // 默认无参构造函数
                            //builder.AppendLine("{0}.$ctor = {0}.$;", TypeName);
                            builder.AppendLine("_R.$TC[\"{0}\"] = {1}.$;", GetRenamedRuntimeTypeName(type), TypeName);
                        }
                    }
                    // 静态方法
                    foreach (DefineMethod item in node.Methods.Where(m => m is DefineMethod && (m.Modifier & EModifier.Static) != EModifier.None)) Visit(item);
                    // 实例方法
                    foreach (DefineMethod item in node.Methods.Where(m => m is DefineMethod && (m.Modifier & EModifier.Static) == EModifier.None)) Visit(item);
                    // 内部类
                    foreach (var item in node.NestedType) Visit(item);

                    // 赋值的静态字段 & 静态构造函数 应该放在最后面执行，但仍然有先后顺序

                    FixedBuilder(() =>
                    {
                        // 静态字段
                        foreach (var item in node.Fields.Where(f => (f.Modifier & EModifier.Static) != EModifier.None || (f.Modifier & EModifier.Const) != EModifier.None))
                        {
                            Visit(item);
                        }
                        // 静态构造函
                        foreach (var item in node.Methods.Where(m => m is DefineConstructor && (m.Modifier & EModifier.Static) != EModifier.None))
                        {
                            builder.AppendLine("(function()");
                            Visit(item.Body);
                            // 直接调用此构造函数
                            builder.AppendLine(")();");
                        }
                    }, Builder);

                }// end of isInterface

                if (node.Generic.IsGeneric)
                {
                    builder.AppendLine("var __array = new Array({0});", node.Generic.GenericTypes.Count);
                    for (int i = 0, n = node.Generic.GenericTypes.Count - 1; i <= n; i++)
                    {
                        var item = node.Generic.GenericTypes[i];
                        builder.AppendLine("__array[{0}] = {1};", i, item.Name.Name);
                    }
                    // keys=泛型实参类型数组 value=类型
                    builder.AppendLine("{0}.types[__i] = {{}};", ParentTypeName);
                    builder.AppendLine("{0}.types[__i].keys = __array;", ParentTypeName);
                    builder.AppendLine("{0}.types[__i].value = {1};", ParentTypeName, TypeName);
                    builder.AppendLine("return {0};", TypeName);
                    builder.AppendLine("}");
                    typeNames.Pop();
                    WriteReflectionInfo(null, false);
                }

            }, swapWriter); // end of FixedBuilder(swapWriter)

            // 定义类型结束，开始定义其它类型
            typeNames.Pop();
        }
        protected override void Write(DefineEnum node)
        {
            typeNames.Push(GetTypeName(DefiningType));

            //if (DefiningType.ContainingType == null)
            //    builder.Append("window.");
            builder.AppendLine("{0} = function(v,n){{this.v=v;this.n=n;}};", TypeName);
            WriteReflectionInfo(null, false);
            builder.AppendLine("{0}.{2} = {1};", TypeName, GetStructSize(DefiningType.UnderlyingType), ENUM);
            int value = 0;
            foreach (var item in node.Fields)
            {
                builder.Append("{0}.{1} = ", TypeName, item.Name.Name);
                if (item.Value != null)
                {
                    // 如果value是引用一个const，还需要知道那个const的值
                    if (item.Value is PrimitiveValue)
                    {
                        PrimitiveValue primitive = (PrimitiveValue)item.Value;
                        builder.Append(primitive.Value);
                        value = (int)Convert.ChangeType(primitive.Value, typeof(int)) + 1;
                    }
                    else
                    {
                        throw new NotImplementedException("不支持枚举值引用其它常量");
                        //Visit(item.Value);
                    }
                }
                else
                {
                    builder.Append(value);
                    value++;
                }
                builder.AppendLine(";");
            }

            typeNames.Pop();
        }
        protected override void Write(DefineDelegate node) { }
        protected override void Write(DefineField node)
        {
            if (DefiningMember.IsStatic || DefiningMember.IsConstant)
                builder.Append("{0}.{1} = ", TypeName, node.Name.Name);
            else
                builder.Append("this.{0} = ", node.Name.Name);
            WriteValueOrDefault(node.Value);
        }
        protected override void Write(DefineProperty node)
        {
            /* 可以实现和C#属性一样的效果，定义和执行效率未考证
             * Object.defineProperty(A.prototype, "PropertyName", {
                get:function() { return this._a; },
                set:function(value) { return this._a = value; },
                });
             */
            // 抽象属性不生成
            var member = DefiningMember;
            if (member.IsAbstract)
                return;

            string name = member.Name.Name;
            // virtual属性且有被重写时，视为字段可能会出现错误
            //if (node.IsAuto)
            //{
            //    // 自动属性直接视为字段
            //    builder.AppendLine("{0}.prototype.{1} = {2};", TypeName, name, GetDefaultValueCode(member.ReturnType));
            //    return;
            //}

            // 统一采用get|set方法的实现方式
            string isStatic = member.IsStatic ? string.Empty : "prototype.";
            string isThis = member.IsStatic ? TypeName : "this";
            if (node.Getter != null)
            {
                // 防止显示实现的成员覆盖掉隐式实现的成员
                if (!member.IsStatic && node.ExplicitImplement != null)
                    builder.AppendLine("if (!{0}.{3}{1}{2})", TypeName, GET, name, isStatic);
                builder.Append("{0}.{3}{1}{2} = function(", TypeName, GET, name, isStatic);
                Visit(node.Arguments);
                builder.Append(")");
                if (node.Getter.Body == null)
                    builder.AppendLine("{{return {2}.{0}{1};}};", AUTO_PROPERTY, name, isThis);
                else
                {
                    builder.AppendLine();
                    WriteIEnumerableBody(node.Arguments, member, node.Getter.Body);
                    builder.Append(";");
                }
            }
            if (node.Setter != null)
            {
                // 防止显示实现的成员覆盖掉隐式实现的成员
                if (!member.IsStatic && node.ExplicitImplement != null)
                    builder.AppendLine("if (!{0}.{3}{1}{2})", TypeName, SET, name, isStatic);
                builder.Append("{0}.{3}{1}{2} = function(value", TypeName, SET, name, isStatic);
                paramArgumentOffset = 1;
                if (node.Arguments != null && node.Arguments.Count > 0)
                    builder.Append(", ");
                Visit(node.Arguments);
                builder.Append(")");
                if (node.Setter.Body == null)
                    builder.AppendLine("{{{2}.{0}{1}=value;}};", AUTO_PROPERTY, name, isThis);
                else
                {
                    builder.AppendLine();
                    Visit(node.Setter.Body);
                    builder.Append(";");
                }
            }

            /* JS类似C#属性的实现，索引器可使用属性的实现方式，用到的参数改为实例变量 */
            //if (node.IsIndexer)
            //{
            //    /* 普通的get|set方法的实现 */
            //    builder.Append("{0}.{1}__{2} = function(", TypeName, isStatic, name);
            //    Visit(node.Arguments);
            //    builder.AppendLine(")");
            //    builder.AppendBlockWithEnd(() =>
            //    {
            //        foreach (var item in node.Arguments)
            //            builder.AppendLine("{0}.___{1} = {1};", isThis, item.Name);

            //        // 参数作为实例参数，每次调用时赋值
            //        foreach (var item in node.Arguments)
            //            item.Name.Name = string.Format("{0}.___{1}", isThis, item.Name.Name);
            //    });
            //}
            //if (member.IsStatic)
            //    builder.AppendLine("Object.defineProperty({0}, \"{1}\",", TypeName, name);
            //else
            //    builder.AppendLine("Object.defineProperty({0}.prototype, \"{1}\",", TypeName, name);
            //builder.AppendBlock(() =>
            //{
            //    if (node.Getter != null)
            //    {
            //        builder.Append("get:function()");
            //        if (node.Getter.Body == null)
            //            builder.AppendLine("{{return {2}.{0}{1};}}", AUTO_PROPERTY, name, isThis);
            //        else
            //        {
            //            builder.AppendLine();
            //            WriteIEnumerableBody(node.Arguments, member, node.Getter.Body);
            //        }
            //    }
            //    if (node.Setter != null)
            //    {
            //        if (node.Getter != null)
            //            builder.Append(',');
            //        builder.Append("set:function(value)");
            //        if (node.Setter.Body == null)
            //            builder.AppendLine("{{{2}.{0}{1}=value;}}", AUTO_PROPERTY, name, isThis);
            //        else
            //        {
            //            builder.AppendLine();
            //            Visit(node.Setter.Body);
            //        }
            //    }
            //});
            //builder.AppendLine(");");
        }
        public override void Visit(Accessor node)
        {
            Visit(node.Body);
        }
        protected override void Write(DefineConstructor node)
        {
            // 构造函数作为类型的静态成员来调用
            builder.Append("{0}.{1} = function(", TypeName, node.Name.Name);
            Visit(node.Arguments);
            builder.AppendLine(")");
            builder.AppendBlockWithEnd(() =>
            {
                CSharpMember constructor;

                // 若是继承this的构造函数，则继承的构造函数已经调用了原型构造函数了，不重复调用
                //if (node.Base == null || this.GetSyntaxType(node.Base) != DefiningType)
                    // 调用原型构造函数
                    WriteInvokeBaseDefaultConstructor(TypeName, false);

                if (node.Base == null)
                {
                    // 显示声明的无参构造函数
                    var type = DefiningMember.ContainingType.BaseClass;
                    if (IsCanInherit(type))
                    {
                        constructor = type.MemberDefinitions.FirstOrDefault(m => m.IsConstructor && !m.IsStatic && m.Parameters.Count == 0);
                        if (constructor == null)
                        {
                            // 继承类型隐式默认构造函数$
                            WriteInvokeBaseDefaultConstructor(GetTypeName(type), false);
                        }
                        else
                        {
                            // 继承类型显示默认构造函数
                            WriteInvokeBaseDefaultConstructor(string.Format("{0}.{1}", GetTypeName(type), constructor.Name.Name), true);
                        }
                    }
                }
                else
                {
                    // base() | this()
                    constructor = (CSharpMember)syntax[node.Base].Definition.Define;
                    if (constructor.ContainingType == DefiningType)
                        builder.AppendLine("this.constructor = {0}.{1};", TypeName, constructor.Name.Name);
                    else
                        builder.AppendLine("this.constructor = {0}.{1};", GetTypeName(constructor.ContainingType), constructor.Name.Name);

                    // 构造函数允许使用ref和out
                    constructorTarget = node.Base.Target;
                    node.Base.Target = CONSTRUCTOR;
                    Visit(node.Base);
                    constructorTarget = null;
                    builder.AppendLine(";");
                    //builder.AppendLine("delete this.{0};", SUPER);
                }
                // constructor一定指向真原型
                builder.AppendLine("this.constructor = {0};", TypeName);
                if (node.Body != null)
                    Visit(node.Body);
            });
            builder.AppendLine("{0}.{1}.prototype = {0}.prototype;", TypeName, node.Name.Name);
        }
        protected override void Write(DefineMethod node)
        {
            if ((node.Modifier & EModifier.Abstract) == EModifier.Abstract)
                return;
            var member = DefiningMember;
            string methodName = member.Name.Name;
            //if (member.ExplicitInterfaceImplementation != null &&
            //    methodName == "GetEnumerator" &&
            //    member.ExplicitInterfaceImplementation.ContainingNamespace != null &&
            //    member.ExplicitInterfaceImplementation.ContainingNamespace.ToString() == "System.Collections")
            //{
            //    // IEnumerable.GetEnumerator 会覆盖掉 IEnumerable<T>.GetEnumerator，所以这里不生成此方法
            //    return;
            //}
            // 没有重命名再进行名字修改
            if (member.IsOperator && !renamedMembers.ContainsKey(member))
            {
                Named name = _BuildReference.GetBinaryOperator(CodeBuilder.FindBinaryOperator(methodName));
                if (name == null)
                    name = _BuildReference.GetUnaryOperator(CodeBuilder.FindUnaryOperator(methodName));
                // 为null可能是Explicit或Implicit
                if (name != null)
                    methodName = name.Name;
            }
            if (member.IsStatic)
                builder.Append("{0}.{1} = function(", TypeName, methodName);
            else
            {
                // 防止显示实现的成员覆盖掉隐式实现的成员
                if (!member.IsStatic && node.ExplicitImplement != null)
                    builder.AppendLine("if (!{0}.prototype.{1})", TypeName, methodName);
                builder.Append("{0}.prototype.{1} = function(", TypeName, methodName);
            }
            // <>泛型: 采用跟泛型类同样的方式生成外部泛型闭包，内部返回实际方法
            if (node.Generic.IsGeneric)
            {
                for (int i = 0, n = node.Generic.GenericTypes.Count - 1; i <= n; i++)
                {
                    var item = node.Generic.GenericTypes[i];
                    builder.Append(item.Name.Name);
                    if (i != n)
                        builder.Append(", ");
                }
                builder.AppendLine(")");
                builder.AppendLine("{");
                // 闭包内的临时变量
                //builder.AppendLine("var {0} = this;", TEMP_THIS);
                builder.Append("return function(");
            }
            Visit(node.Arguments);
            builder.AppendLine(")");
            WriteIEnumerableBody(node.Arguments, member, node.Body);
            if (node.Generic.IsGeneric)
            {
                builder.AppendLine(".bind(this);");
                builder.AppendLine("};");
            }
            else
                builder.AppendLine(";");
        }
        void WriteIEnumerableBody(List<FormalArgument> arguments, CSharpMember method, Body body)
        {
            if (body == null)
            {
                Visit(body);
            }
            else
            {
                var e = IEnumeratorTester.EnumeratorType(method.ReturnType);
                if (e != EEnumerator.None && IEnumeratorTester.TestYield(body))
                {
                    inYieldEnumerable = true;
                    isStaticYield = DefiningMember.IsStatic;
                    DefineType define;
                    var stateList = IEnumeratorRebuilder.Build(arguments, method, body, out define);
                    builder.AppendBlock(() =>
                    {
                        builder.AppendLine("function $create()");
                        builder.AppendBlock(() =>
                        {
                            // 方法体使用此类型
                            builder.AppendLine("var _ = new {0}.{1}();", TypeName, define.Name.Name);
                            if (arguments != null)
                                foreach (var item in arguments)
                                    builder.AppendLine("_.{0} = {0};", item.Name.Name);
                            // $this
                            if (!DefiningMember.IsStatic)
                                builder.AppendLine("_.{0} = this;", TEMP_THIS);
                            builder.AppendLine("return _;");
                        });

                        // yield方法块做缓存
                        builder.AppendLine("if (!{0}.{1})", TypeName, define.Name.Name);
                        // 声明此迭代器类型
                        builder.AppendBlock(() =>
                        {
                            // Constructor
                            builder.AppendLine("{0}.{1} = function()", TypeName, define.Name.Name);
                            builder.AppendBlockWithEnd(() =>
                            {
                                //WriteReflectionInfo(define.Name.Name, true);
                                //builder.AppendLine("this.{0}Current = function(){{return {0};", GetDefaultValueCode(method.ReturnType.TypeArguments[0]));
                                builder.AppendLine("{0} = 0;", IEnumeratorRebuilder.ENUMERATOR_STATE);
                                //if (!DefiningMember.IsStatic)
                                //    builder.AppendLine("$bind($this, [this.GetEnumerator,this.MoveNext]);");
                            });
                            // TypeName
                            //builder.AppendLine("{0}.{1}.{2} = \"0_{0}_{1}\";", TypeName, define.Name.Name, TYPE_NAME);
                            // GetEnumerator
                            //builder.AppendLine("{0}.{1}.prototype.GetEnumerator = function() {{ return this; }}", TypeName, define.Name.Name);
                            builder.AppendLine("{0}.{1}.prototype.GetEnumerator = function()", TypeName, define.Name.Name);
                            builder.AppendBlockWithEnd(() =>
                            {
                                // 第一次就使用本身，后面再创建新实例
                                builder.AppendLine("if (!this.$get) { this.$get = true; return this; }");
                                builder.AppendLine("var __ = new {0}.{1}();", TypeName, define.Name.Name);
                                if (arguments != null)
                                    foreach (var item in arguments)
                                        builder.AppendLine("__.{0} = this.{0};", item.Name.Name);
                                builder.AppendLine("return __;");
                            });
                            // Current
                            builder.AppendLine("{0}.{1}.prototype.{2}Current = function() {{ return this.$current; }};", TypeName, define.Name.Name, GET);
                            builder.AppendLine("{0}.{1}.prototype.{2}Current = function(v) {{ return this.$current = v; }};", TypeName, define.Name.Name, SET);
                            // Reset
                            builder.AppendLine("{0}.{1}.prototype.Reset = function(){{}};", TypeName, define.Name.Name);
                            // Dispose
                            builder.AppendLine("{0}.{1}.prototype.Dispose = function(){{}};", TypeName, define.Name.Name);
                            // MoveNext
                            builder.AppendLine("{0}.{1}.prototype.MoveNext = function()", TypeName, define.Name.Name);
                            builder.AppendBlockWithEnd(() =>
                            {
                                builder.AppendLine("var temp = {0};", IEnumeratorRebuilder.ENUMERATOR_STATE);
                                builder.AppendLine("do { temp = this.__Move(temp); } while (temp >= 0);");
                                builder.AppendLine("return temp == -1");
                            });
                            // 生成Yield的核心代码__Move
                            builder.AppendLine("{0}.{1}.prototype.__Move = function(__s)", TypeName, define.Name.Name);
                            builder.AppendBlockWithEnd(() =>
                            {
                                // 调用方法传入的参数全部追加this
                                if (arguments != null)
                                    foreach (var item in arguments)
                                        Renamer.Rename(syntax[item].Definition.Define, this, "this." + item.Name.Name);
                                // $this
                                if (!DefiningMember.IsStatic)
                                    builder.AppendLine("var {0} = this.{0};", TEMP_THIS);
                                builder.AppendLine("switch (__s)");
                                builder.AppendBlock(() =>
                                {
                                    for (int i = 0; i < stateList.Count; i++)
                                    {
                                        builder.AppendLine("case {0}:", i);
                                        foreach (var item in stateList[i])
                                        {
                                            VisitStatement(item);
                                        }
                                        builder.AppendLine();
                                    }
                                });
                                builder.AppendLine("return -2;");
                            });
                        });

                        builder.AppendLine("return $create.bind(this)();");
                    });// end of enumerator body
                    inYieldEnumerable = false;
                }
                else
                    Visit(body);
            }
        }
        public override void Visit(List<FormalArgument> node)
        {
            if (node == null || node.Count == 0)
                return;
            for (int i = 0, n = node.Count - 1; i <= n; i++)
            {
                var argument = node[i];
                RenameLocalField(argument);
                Visit(argument);
                if (argument.Direction == EDirection.REF || argument.Direction == EDirection.OUT)
                {

                    // ref int a => ref a
                    var _ref = syntax[argument];
                    foreach (var item in _ref.Definition.References)
                        if (item.Reference != null)
                            item.Reference.Name += ".v";
                }
                if (i != n)
                {
                    builder.Append(", ");
                }
                else
                {
                    if (argument.Direction == EDirection.PARAMS)
                    {
                        paramArgument = node[n].Name.Name;
                        paramArgumentOffset = n;
                    }
                }
            }
        }
        public override void Visit(List<ActualArgument> node)
        {
            VisitActualArgument(node, DefiningMember);
        }
        /// <summary>调用方法时传的实参，传入方法以确定每个参数的类型</summary>
        void VisitActualArgument(List<ActualArgument> node, CSharpMember member)
        {
            bool hasArguments = node != null && node.Count > 0;
            // 扩展方法的this参数
            int offset = 0;
            if (exThisParam != null)
            {
                offset++;
                var temp = exThisParam;
                exThisParam = null;
                Visit(temp);
                if (hasArguments)
                    builder.Append(", ");
            }
            if (hasArguments)
            {
                var parameters = member == null ? null : member.Parameters;
                for (int i = 0, n = node.Count - 1; i <= n; i++, offset++)
                {
                    var item = node[i];
                    // ref && out
                    if (item.Direction == EDirection.REF || item.Direction == EDirection.OUT)
                    {
                        REF_P.Name.Name = REF + i;
                        Visit(REF_P);
                    }
                    else
                    {
                        CSharpType delegateType = null;
                        if (parameters != null)
                        {
                            if (parameters.Count > 0 && offset >= parameters.Count && parameters.Last().IsParams)
                                delegateType = parameters.Last().Type;
                            else
                                delegateType = parameters[offset].Type;
                        }
                        WriteStructClone(item.Expression, delegateType);
                    }
                    if (i != n)
                        builder.Append(", ");
                }
            }
        }
        public override void Visit(Body node)
        {
            scopeDepth++;
            //builder.AppendLine();
            builder.AppendLine("{");
            WriteParamsArgument();
            if (inBody.Length > 0)
            {
                builder.Append(inBody);
                inBody.Clear();
            }
            foreach (var item in node.Statements)
                VisitStatement(item);
            builder.AppendLine("}");
            scopeDepth--;
        }
        public override void Visit(Field node)
        {
            builder.Append(node.Name.Name);
            if (node.Value != null)
            {
                builder.Append(" = ");
                //Visit(node.Value);
                WriteStructClone(node.Value, null);
            }
        }
        public override void Visit(FieldLocal node)
        {
            RenameLocalField(node);

            builder.Append("var ");
            VAR _var = (VAR)syntax[node].Definition.Define;
            if (node.Value == null && _var.Type.IsStruct && !Refactor.IsNumberType(_var.Type))
                // 定义一个结构体，后面直接对其字段进行赋值的情况(例如 VECTOR2 vec;vec.x=0;vec.y=0)，应该要new构造此结构体
                builder.Append("{0} = {1}", node.Name.Name, GetDefaultValueCode(_var.Type));
            else
                Visit((Field)node);
            if (node.IsMultiple)
            {
                foreach (var item in node.Multiple)
                {
                    RenameLocalField(item);
                    builder.Append(", ");
                    _var = (VAR)syntax[item].Definition.Define;
                    if (item.Value == null && _var.Type.IsStruct && !Refactor.IsNumberType(_var.Type))
                        builder.Append("{0} = {1}", item.Name.Name, GetDefaultValueCode(_var.Type));
                    else
                        Visit(item);
                }
            }
            if (node.IsMember)
                builder.AppendLine(";");
        }
        public override void Visit(EModifier node) { }

        public override void Visit(ForeachStatement node)
        {
            // HACK: 同一域内两个相同的foreach则可能导致重名，不过在JS里重名只是后一个覆盖前一个所以暂时没问题
            string name = ENUMERABLE + scopeDepth;
            builder.Append("var {0} = ", name);
            Visit(node.In);
            CSharpType etype;
            var obj = syntax[node.In].Definition.Define;
            if (obj is CSharpMember)
                etype = ((CSharpMember)obj).ReturnType;
            else if (obj is VAR)
                etype = ((VAR)obj).Type;
            else
                etype = (CSharpType)obj;
            if (IEnumeratorTester.EnumeratorType(etype) == EEnumerator.Enumerable)
                builder.Append(".GetEnumerator()");
            builder.AppendLine(";");

            builder.AppendLine("while ({0}.MoveNext())", name);
            builder.AppendBlock(() =>
            {
                builder.AppendLine("var {0} = {1}.{2}Current();", node.Name.Name, name, GET);
                Visit(node.Body);
            });
        }
        public override void Visit(TryCatchFinallyStatement node)
        {
            builder.AppendLine("try");
            Visit(node.Try);
            Visit(node.Catch);
            if (node.Finally != null)
            {
                builder.AppendLine("finally");
                Visit(node.Finally);
            }
        }
        public override void Visit(List<CatchStatement> node)
        {
            if (node.Count == 0)
            {
                // JS没有catch捕获不到异常，finally会执行，但之后的代码将不被执行
                builder.AppendLine("catch({0}){{}}", ANONYMOUS_EX);
            }
            else
            {
                builder.AppendLine("catch({0})", ANONYMOUS_EX);
                builder.AppendBlock(() =>
                {
                    // 让网页抛出异常，可以看到行号等，调试用
                    builder.AppendLine("if ({0}.message) {{ throw {0}; }}", ANONYMOUS_EX);
                    // todo: BUG $is(exception) 判断不出异常类型
                    for (int i = 0, n = node.Count - 1; i <= n; i++)
                    {
                        var item = node[i];
                        if (item.HasName)
                        {
                            inBody.AppendLine("{0} = {1};", item.Name.Name, ANONYMOUS_EX);
                        }
                        if (item.HasArgument)
                        {
                            if (i > 0)
                                builder.Append("else ");
                            //builder.Append("if ({0} instanceof ", ANONYMOUS_EX);
                            builder.Append("if ($is({0}, ", ANONYMOUS_EX);
                            Visit(item.Type);
                            builder.AppendLine("))");
                        }
                        Visit(item.Body);
                    }
                });
            }
        }
        public override void Visit(ThrowStatement node)
        {
            builder.Append("throw ");
            if (node.Value != null)
                Visit(node.Value);
            else
                builder.Append(ANONYMOUS_EX);
            builder.AppendLine(";");
        }
        public override void Visit(CheckedStatement node)
        {
            throw new NotImplementedException();
        }
        public override void Visit(UncheckedStatement node)
        {
            throw new NotImplementedException();
        }
        public override void Visit(UsingStatement node)
        {
            List<VAR> _vars = new List<VAR>();
            foreach (var item in node.Fields)
            {
                REF _ref;
                if (syntax.TryGetValue(item, out _ref))
                {
                    VAR _var = _ref.Definition.Define as VAR;
                    if (_var != null)
                        _vars.Add(_var);
                }
                Visit(item);
                builder.AppendLine(";");
            }
            Visit(node.Body);
            // todo: 目前数组类型没有Dispose方法，还需调试
            //foreach (var item in _vars)
            //{
            //    builder.AppendLine("{0}.Dispose();", item.Name.Name);
            //}
        }
        public override void Visit(LockStatement node)
        {
            Visit(node.Body);
        }
        public override void Visit(ReturnStatement node)
        {
            builder.Append("return");
            if (node.Value != null)
            {
                builder.Append(" ");
                WriteStructClone(node.Value, DefiningMember.ReturnType);
            }
            builder.AppendLine(";");
        }
        /// <summary><para>事件 += obj; return obj; 方法(obj); obj都有可能是一个方法，此时需要bind(当时的指针对象)</para>
        /// <para>结构体在调用方法，return时，需要克隆结构体</para></summary>
        /// <param name="node">obj表达式</param>
        /// <param name="delegateType">表达式的类型</param>
        void WriteStructClone(SyntaxNode node, CSharpType delegateType)
        {
            if (node == null)
                return;

            // expression ? expression.bind(bind) : expression;
            string expression = null;
            string bind = null;
            // isNull为false时，例如引用this.方法时，肯定不为null，此时不需要判断null，直接bind(this)即可
            bool isNull = true;

            REF _ref;
            if (syntax.TryGetValue(node, out _ref))
            {
                CSharpType structType = null;
                VAR _var = _ref.Definition.Define as VAR;
                if (_var != null)
                {
                    structType = _var.Type;
                    if ((delegateType != null && (delegateType.IsDelegate || delegateType == CSharpType.DELEGATE))
                        //&& structType.Equals(delegateType)
                        )
                    {
                        // 委托方法作为参数，需要bind(this)
                        //builder.Append("(");
                        //Visit(node);
                        //builder.Append(")");
                        if (!DefiningMember.IsStatic)
                            bind = "this";
                    }
                }
                else
                {
                    CSharpMember member = _ref.Definition.Define as CSharpMember;
                    if (member != null)
                    {
                        // 属性或方法在return时已经clone了，所以并不需要再clone
                        if (member.IsField || member.IsConstant)
                        {
                            structType = member.ReturnType;
                        }
                        if ((delegateType != null && (delegateType.IsDelegate || delegateType == CSharpType.DELEGATE)) && 
                            (member.IsMethod || (structType != null && structType.IsDelegate)))
                        {
                            isNull = !member.IsMethod;
                            // 委托方法作为参数，需要bind(this)
                            //builder.Append("(");
                            //Visit(node);
                            //builder.Append(")");
                            if (node is ReferenceMember)
                            {
                                ReferenceMember r = (ReferenceMember)node;
                                if (r.Target != null)
                                {
                                    bind = PeekLastWrittenExpression(() => Visit(r.Target));
                                }
                            }
                            if (bind == null && !DefiningMember.IsStatic)
                                bind = "this";
                        }
                    }
                    else
                    {
                        CSharpType type = _ref.Definition.Define as CSharpType;
                        if (type != null)
                        {
                            // array[i] | 临时委托
                            if (type.IsArray)
                                structType = type.ElementType;
                            else
                            {
                                if (!(node is PrimitiveValue))
                                    structType = type;
                            }
                            // 临时委托
                            if (delegateType != null && (delegateType.IsDelegate || delegateType == CSharpType.DELEGATE))
                            {
                                isNull = type.IsArray;
                                if (!DefiningMember.IsStatic)
                                    bind = "this";
                            }
                        }
                    }
                }

                if (structType != null && structType.IsValueType && !CSharpType.IsPrimitive(structType) && !structType.IsEnum)
                {
                    //builder.Append("(");
                    Visit(node);
                    builder.Append(".{0}()", STRUCT_CLONE);
                }
                else
                {
                    //Visit(node);
                    expression = PeekLastWrittenExpression(() => Visit(node));
                    if (!(node is PrimitiveValue) && bind != null)
                        if (isNull)
                            builder.Append("{0} ? {0}.bind({1}) : {0}", expression, bind);
                        else
                            builder.Append("{0}.bind({1})", expression, bind);
                    else
                        builder.Append(expression);
                }
            }
            else
                Visit(node);
            // null除外
            //if (!(node is PrimitiveValue) && delegateType != null && delegateType.IsDelegate)
            //    builder.Append(".bind(this)");
        }

        void VisitOperatorParameter(SyntaxNode memberNode, SyntaxNode opNode, out VAR refVar, out CSharpMember refMember, out CSharpMember refOperator)
        {
            refMember = null;
            refOperator = null;
            refVar = null;
            REF refExp, refOp;
            if (syntax.TryGetValue(memberNode, out refExp))
            {
                CSharpMember member = refExp.Definition.Define as CSharpMember;
                if (member != null)
                {
                    refMember = member;
                }
                else
                {
                    // 除了临时变量外，还可能是数组(arg[0]++)
                    refVar = refExp.Definition.Define as VAR;
                    if (refVar == null)
                    {
                        CSharpType type = refExp.Definition.Define as CSharpType;
                        if (type.IsArray)
                        {
                            refVar = new VAR();
                            refVar.Type = type.ElementType;
                        }
                        // else (a==b) && (c==d) 此时引用的是表达式的类型
                    }
                }
            }
            if (syntax.TryGetValue(opNode, out refOp))
            {
                CSharpMember member = refOp.Definition.Define as CSharpMember;
                if (member != null && member.IsOperator)
                    refOperator = member;
            }
        }
        string CheckOverflow(VAR refVar, CSharpMember refMember, string arg)
        {
            // 自动算数溢出
            // byte: b & 255
            // sbyte: (b > 127) ? (b | -256) : b
            string overflow = null;
            CSharpType type;
            if (refVar != null) type = refVar.Type;
            else type = refMember.ReturnType;
            if (type == CSharpType.BYTE)
                overflow = "$toB";
            else if (type == CSharpType.SBYTE)
                overflow = "$toSB";
            // else: 对其它类型暂不进行自动的算数溢出检测，而在强制类型转换时进行溢出检测
            if (overflow != null)
                arg = string.Format("({0}({1}))", overflow, arg);
            return arg;
        }
        string CheckDivide(BinaryOperator op, string arg)
        {
            if (op.Operator == EBinaryOperator.Division || op.Operator == EBinaryOperator.AssignDivide)
            {
                bool needInt = false;
                CSharpType type = GetSyntaxType(op);
                if (type == null)
                {
                    CSharpType left = GetSyntaxType(op.Left);
                    CSharpType right = GetSyntaxType(op.Right);
                    needInt = IsInteger(left) && IsInteger(right);
                }
                else
                {
                    needInt = IsInteger(type);
                }
                if (needInt)
                    return string.Format("(({0})|0)", arg);
            }
            return arg;
        }
        bool IsInteger(CSharpType type)
        {
            return type == CSharpType.BYTE || type == CSharpType.SBYTE ||
                type == CSharpType.USHORT || type == CSharpType.SHORT || type == CSharpType.CHAR ||
                type == CSharpType.UINT || type == CSharpType.INT ||
                type == CSharpType.ULONG || type == CSharpType.LONG;
        }
        string GetBinaryOperatorLeftTarget(SyntaxNode expression, VAR refVar, CSharpMember refMember)
        {
            SyntaxNode target;
            if (refVar == null)
            {
                if (refMember.IsField || refMember.IsProperty)
                    target = ((ReferenceMember)expression).Target;
                else
                    target = ((InvokeMethod)expression).Target;
                // 类型内部成员
                //needThis &= target == null;
            }
            else
            {
                // 引用的为临时变量refVar
                target = null;
            }
            // 可以直接get和set的字段或临时变量
            bool isField = refVar != null || refMember.IsField;

            string __t1;
            if (target == null)
            {
                // 引用的是类型内部的成员，但由于闭包的"this"关键字指向的是window，所以要将this传入方法内，此时__t1指向传入的this参数
                if (refVar == null)
                {
                    if (refMember.IsStatic)
                        __t1 = GetTypeName(refMember.ContainingType);
                    else
                        __t1 = GetThis();
                    __t1 += ".";
                }
                else
                    __t1 = null;
            }
            else
            {
                if (target is ReferenceMember && ((ReferenceMember)target).Name.Name == "base")
                {
                    // [base.成员]的情况
                    __t1 = string.Format("{0}._{1}", GetThis(), GetInheritDepth(refMember.ContainingType));
                    //isBase = true;
                }
                else
                {
                    __t1 = string.Format("{0}.", PeekLastWrittenExpression(() => Visit(target)));
                }
            }
            return __t1;
        }
        /// <summary>写入赋值运算符代码</summary>
        /// <param name="expression">被赋值的表达式，一元运算符为引用的表达式，二元运算符为左表达式</param>
        /// <param name="refVar">被引用的临时变量，由VisitOperatorPhase1提供</param>
        /// <param name="refMember">被引用的成员，应该是字段，属性或者索引器，由VisitOperatorPhase1提供</param>
        /// <param name="getValue">可对调用属性的get的值进行自定义，这也是闭包最后传出的值</param>
        /// <param name="setValue">可对调用属性的set的值(getValue的最终值)进行自定义</param>
        void WriteAssignOperator(SyntaxNode expression, VAR refVar, CSharpMember refMember, Func<string, string> getValue, Func<string, string> setValue)
        {
            SyntaxNode target;
            if (refVar == null)
            {
                if (refMember.IsField || refMember.IsProperty)
                    target = ((ReferenceMember)expression).Target;
                else
                    target = ((InvokeMethod)expression).Target;
                // 类型内部成员
                //needThis &= target == null;
            }
            else
            {
                // 引用的为临时变量refVar
                target = null;
            }
            // 可以直接get和set的字段或临时变量
            bool isField = refVar != null || refMember.IsField;

            builder.Append("(function(");
            builder.Append("){");

            string __t1;
            if (target == null)
            {
                // 引用的是类型内部的成员，但由于闭包的"this"关键字指向的是window，所以要将this传入方法内，此时__t1指向传入的this参数
                if (refVar == null)
                {
                    if (refMember.IsStatic)
                        __t1 = GetTypeName(refMember.ContainingType);
                    else
                        __t1 = GetThis();
                    __t1 += ".";
                }
                else
                    __t1 = null;
            }
            else
            {
                if (target is ReferenceMember && ((ReferenceMember)target).Name.Name == "base")
                {
                    // [base.成员]的情况
                    __t1 = string.Format("{0}._{1}", GetThis(), GetInheritDepth(refMember.ContainingType));
                    //isBase = true;
                }
                else
                {
                    if (isField)
                    {
                        __t1 = PeekLastWrittenExpression(() => Visit(target));
                    }
                    else
                    {
                        // 属性或者索引器
                        __t1 = ANONYMOUS_OP_TARGET;
                        builder.Append("var {0}={1};", __t1, PeekLastWrittenExpression(() => Visit(target)));
                    }
                    __t1 += ".";
                }
            }

            // indexer参数
            string indexerArg = null;
            // 声明结果变量，这个变量将为闭包的返回结果
            builder.Append("var {0}=", ANONYMOUS_OP);
            StringBuilder getBuilder = new StringBuilder();
            // 临时变量则__t1为null
            if (__t1 != null) getBuilder.Append(__t1);
            if (isField)
            {
                if (refVar != null)
                    getBuilder.Append(refVar.Name.Name);
                else
                    getBuilder.Append(refMember.Name.Name);
            }
            else
            {
                getBuilder.Append("{0}{1}(", GET, refMember.Name.Name);
                if (refMember.IsIndexer)
                {
                    // 索引器不能使用带ref和out的参数，但可以使用params
                    indexerArg = PeekLastWrittenExpression(() => VisitActualArgument(((InvokeMethod)expression).Arguments, refMember));
                    getBuilder.Append(indexerArg);
                }
                getBuilder.Append(")");
            }

            // 结果变量的值
            string getParam = getBuilder.ToString();
            if (getValue != null)
                getParam = getValue(getParam);
            builder.Append(getParam);
            builder.Append(";");

            // 赋值
            // set参数
            string setParamValue;
            if (setValue == null)
                setParamValue = ANONYMOUS_OP;
            else
                setParamValue = setValue(ANONYMOUS_OP);
            if (__t1 != null) builder.Append(__t1);
            if (isField)
            {
                if (refVar != null)
                    builder.Append(refVar.Name.Name);
                else
                    builder.Append(refMember.Name.Name);
                builder.Append(" = ");
                builder.Append(setParamValue);
            }
            else
            {
                builder.Append("{0}{1}(", SET, refMember.Name.Name);
                builder.Append(setParamValue);
                if (refMember.IsIndexer)
                    builder.Append(", {0}", indexerArg);
                builder.Append(")");
            }
            // 赋值完毕
            builder.Append(";");
            // 返回最终结果
            builder.Append("return {0};", ANONYMOUS_OP);
            // 声明闭包函数完成并调用闭包函数，首次调用则传入this，闭包内调用闭包则传入闭包参数
            builder.Append("}).bind(this)(");
            builder.Append(")");
        }
        // 赋值号左边必须是字段，属性或索引器
        public override void Visit(UnaryOperator node)
        {
            // JS不允许指针操作
            if (node.Operator == EUnaryOperator.AddressOf ||
                node.Operator == EUnaryOperator.Dereference)
                throw new NotImplementedException();

            VAR refVar;
            CSharpMember refMember;
            CSharpMember refOperator;
            VisitOperatorParameter(node.Expression, node, out refVar, out refMember, out refOperator);

            #region 注释
            /*
                 * 例: this[0]++; 不引用重载运算符
                 * var __t1 = this;
                 * var __t2 = __t1.getThis(0);
                 * __t1.setThis(__t2 + 1, 0);
                 * return __t2;
                 * 例: ++this[0]; 不引用重载运算符
                 * var __t1 = this;
                 * var __t2 = __t1.getThis(0) + 1;
                 * __t1.setThis(__t2, 0);
                 * return __t2; // 虽然赋值后，__t1.getThis(0)的值和__t2值可能不同，但也不重复调用__t1.getThis(0)而直接选用__t2
                 * 
                 * 例: this[0]++; 引用重载运算符
                 * var __t1 = this;
                 * var __t2 = __t1.getThis(0);
                 * __t1.setThis(T.op_Increment(__t2), 0);
                 * return __t2;
                 * 例: ++this[0]; 引用重载运算符
                 * var __t1 = this;
                 * var __t2 = T.op_Increment(__t1.getThis(0));
                 * __t1.setThis(__t2, 0);
                 * return __t2;
                 */
            #endregion

            bool assignLeft = node.Operator == EUnaryOperator.Decrement || node.Operator == EUnaryOperator.Increment;
            bool assignRight = node.Operator == EUnaryOperator.PostDecrement || node.Operator == EUnaryOperator.PostIncrement;
            bool isProperty = refMember != null && (refMember.IsProperty || refMember.IsIndexer);
            bool isOperator = refOperator != null;
            string opName = null;
            if (isOperator)
            {
                if (renamedMembers.ContainsKey(refOperator))
                    opName = refOperator.Name.Name;
                else
                    opName = _BuildTypeSystem.UnaryOperator[node.Operator].Name;
            }
            if ((assignLeft || assignRight) && (isProperty || isOperator) && (refMember == null || !IsJSArray(refMember)))
            {
                // 赋值运算符
                Func<string, string> resultValue;
                if (isOperator)
                {
                    if (assignLeft && !isProperty)
                    {
                        // temp = ++n; => temp = n = Type.op_Increment(n)
                        resultValue = null;
                        builder.Append("(");
                        string expression = GetLastWrittenExpression(() => Visit(node.Expression));
                        builder.Append("={0}.{1}({2}))", GetTypeName(refOperator.ContainingType), opName, expression);
                    }
                    else
                    {
                        resultValue = (p) => string.Format("{0}.{1}({2})", GetTypeName(refOperator.ReturnType), opName, p);
                    }
                }
                else
                {
                    resultValue = (p) => CheckOverflow(refVar, refMember, string.Format("{0} {1} 1", p, (node.Operator == EUnaryOperator.Decrement || node.Operator == EUnaryOperator.PostDecrement) ? "-" : "+"));
                }

                assignOperator++;
                if (assignLeft)
                    WriteAssignOperator(node.Expression, refVar, refMember, resultValue, null);
                else
                    WriteAssignOperator(node.Expression, refVar, refMember, null, resultValue);
                assignOperator--;
            }
            else
            {
                // 非赋值运算符
                if (isOperator)
                {
                    // !n => Type.op_Not(n)
                    builder.Append("{0}.{1}(", GetTypeName(refOperator.ContainingType), opName);
                    Visit(node.Expression);
                    builder.Append(")");
                }
                else
                {
                    if (assignRight)
                    {
                        // n++
                        Visit(node.Expression);
                        builder.Append(UOP[node.Operator]);
                    }
                    else
                    {
                        // !n
                        builder.Append(UOP[node.Operator]);
                        Visit(node.Expression);
                    }
                }
            }
        }
        public override void Visit(BinaryOperator node)
        {
            if (node.Operator == EBinaryOperator.NullCoalescing)
                throw new NotImplementedException();

            /* 优化方案
             * 未连续的运算符部分，不采用闭包 (连续运算符：a = b = c * 5;)
             */

            VAR refVar;
            CSharpMember refMember;
            CSharpMember refOperator;
            VisitOperatorParameter(node.Left, node, out refVar, out refMember, out refOperator);

            bool assign = node.Operator >= EBinaryOperator.Assign;
            EBinaryOperator bop = node.Operator;
            if (assign)
            {
                bop -= EBinaryOperator.Assign;
                assignOperator++;
            }
            bool isProperty = refMember != null && (refMember.IsProperty || refMember.IsIndexer);
            bool isOperator = refOperator != null;
            string opName = null;
            if (isOperator)
            {
                if (renamedMembers.ContainsKey(refOperator))
                    opName = refOperator.Name.Name;
                else
                    opName = _BuildTypeSystem.BinaryOperator[bop].Name;
            }

            CSharpType delegateType = null;
            if (refVar != null)
                delegateType = refVar.Type;
            else if (refMember != null)
                delegateType = refMember.ReturnType;
            string right = PeekLastWrittenExpression(() => WriteStructClone(node.Right, delegateType));
            // event = a; event += a; event -= a;
            if ((node.Operator == EBinaryOperator.Assign || node.Operator == EBinaryOperator.AssignSubtract || node.Operator == EBinaryOperator.AssignAdd) &&
                //(refVar != null && refVar.Type.IsDelegate) || (refMember != null && refMember.ReturnType.IsDelegate) && !(node.Right is PrimitiveValue))
                (delegateType != null && delegateType.IsDelegate))
            {
                //if (node.Right is ReferenceMember)
                //{
                //    int targetPoint = right.LastIndexOf('.');
                //    if (targetPoint != -1)
                //        right = string.Format("{0}.bind({1})", right, right.Substring(0, targetPoint));
                //    else
                //        right = string.Format("{0}.bind(this)", right);
                //}
                if (node.Operator == EBinaryOperator.Assign)
                {
                }
                else
                {
                    right = string.Format("new MulticastDelegate.MulticastDelegate({0})", right);
                }
            }

            if (assign && (refMember == null || !IsJSArray(refMember)))
            {
                // 需要检测类型，需要自动溢出
                if (isProperty && isOperator)
                {
                    // a.Property += value => a.setProperty(Type.op_Addtion(a.getProperty(), value))
                    Func<string, string> resultValue = (p) => string.Format("{0}.{1}({2}, {3})", GetTypeName(refOperator.ContainingType), opName, p, right);
                    WriteAssignOperator(node.Left, refVar, refMember, null, resultValue);
                }
                else if (isProperty)
                {
                    if (node.Operator == EBinaryOperator.Assign)
                    {
                        if (MultiAssign)
                        {
                            // n = a.Property = value => a = (var temp = a.getP(); a.setP(value); return temp)
                            Func<string, string> resultValue = (p) => right;
                            WriteAssignOperator(node.Left, refVar, refMember, null, resultValue);
                        }
                        else
                        {
                            // a.Property = value => a.setProperty(value)
                            builder.Append("{0}{1}{2}(", GetBinaryOperatorLeftTarget(node.Left, refVar, refMember), SET, refMember.Name.Name);
                            Visit(node.Right);
                            if (refMember.IsIndexer)
                            {
                                builder.Append(", ");
                                VisitActualArgument(((InvokeMethod)node.Left).Arguments, refMember);
                            }
                            builder.Append(")");
                        }
                    }
                    else
                    {
                        // a.Property += value => a.setProperty($toB(a.getProperty() + value))
                        Func<string, string> resultValue = (p) => CheckOverflow(refVar, refMember, CheckDivide(node, string.Format("{0} {1} ({2})", p, BOP[bop], right)));
                        WriteAssignOperator(node.Left, refVar, refMember, null, resultValue);
                    }
                }
                else if (isOperator)
                {
                    // a += value => (a = Type.op_Addtion(a, value))
                    string left = PeekLastWrittenExpression(() => Visit(node.Left));
                    builder.Append("({0}=", left);
                    builder.Append("{0}.{1}(", GetTypeName(refOperator.ContainingType), opName);
                    builder.Append("{0}, {1}))", left, right);
                }
                else
                {
                    string left = PeekLastWrittenExpression(() => Visit(node.Left));
                    if (node.Operator == EBinaryOperator.Assign)
                    {
                        // a = value
                        builder.Append("{0} = {1}", left, right);
                    }
                    else
                    {
                        // a += value => (a = $toB(a + value))
                        builder.Append("({0} = {1})", left, CheckDivide(node, CheckOverflow(refVar, refMember, string.Format("{0} {1} ({2})", left, BOP[bop], right))));
                    }
                }
            }
            else
            {
                if (bop == 0)
                    bop = EBinaryOperator.Assign;
                if (isOperator)
                {
                    // a + value => Type.op_Addtion(a, value)
                    // a.Property + value => Type.op_Addtion(a.getProperty(), value)
                    builder.Append("{0}.{1}(", GetTypeName(refOperator.ContainingType), opName);
                    Visit(node.Left);
                    builder.Append(", ");
                    Visit(node.Right);
                    builder.Append(")");
                }
                else
                {
                    // a + value
                    // a.Property + value => a.getProperty() + value
                    builder.Append(CheckDivide(node, string.Format("{0} {1} {2}", PeekLastWrittenExpression(() => Visit(node.Left)), BOP[bop], right)));
                }
            }

            if (assign)
                assignOperator--;
        }
        public override void Visit(PrimitiveValue node)
        {
            object value = node.Value;

            if (value == null)
                builder.Append("null");
            else if (value is bool)
                if ((bool)value)
                    builder.Append("true");
                else
                    builder.Append("false");
            else if (value is string)
            {
                builder.Append('\"');
                string str = value.ToString();
                foreach (char c in str)
                {
                    switch (c)
                    {
                        case '"': builder.Append("\\\""); break;
                        case '\r': builder.Append("\\r"); break;
                        case '\n': builder.Append("\\n"); break;
                        case '\0': builder.Append("\\0"); break;
                        case '\t': builder.Append("\\t"); break;
                        case '\\': builder.Append("\\\\"); break;
                        default: builder.Append(c); break;
                    }
                }
                builder.Append('\"');
            }
            else if (value is char)
            {
                char c = (char)value;
                // Char类型可能还需要特殊处理，平时可当做数字进行计算，ToString可变为字符串
                builder.Append((int)c);
            }
            else if (value is float || value is double)
            {
                string svalue = value.ToString();
                builder.Append(svalue);
                if (svalue.IndexOf('.') == -1)
                    builder.Append(".0");
            }
            else
                builder.Append(value);
        }
        public override void Visit(ArrayValue node)
        {
            builder.Append("[");
            for (int i = 0, n = node.Values.Count - 1; i <= n; i++)
            {
                Visit(node.Values[i]);
                if (i != n)
                    builder.Append(",");
            }
            builder.Append("]");
        }
        public override void Visit(Parenthesized node)
        {
            builder.Append("(");
            Visit(node.Expression);
            builder.Append(")");
        }
        public override void Visit(Checked node)
        {
            throw new NotImplementedException();
        }
        public override void Visit(Unchecked node)
        {
            throw new NotImplementedException();
        }
        public override void Visit(Lambda node)
        {
            REF lambda = syntax[node];
            CSharpType delegateType = (CSharpType)lambda.Definition.Define;
            CSharpMember invoker = delegateType.DelegateInvokeMethod;

            PushMember(invoker);

            builder.Append("function(");
            Visit(node.Parameters);
            builder.Append(")");
            if (node.IsSingleBody)
            {
                builder.Append("{");
                if (!CSharpType.IsVoid(invoker.ReturnType))
                    builder.Append(" return ");
                WriteStructClone(node.Body.Statements[0], invoker.ReturnType);
                //builder.Append("; }.bind(this)");
                builder.Append("; }");
            }
            else
            {
                builder.AppendLine();
                Visit(node.Body);
                //builder.AppendLine(".bind(this)");
            }

            PopMember();
        }
        void WriteReferenceValue(ReferenceMember node)
        {
            if (inYieldEnumerable && !isStaticYield && node.Name.Name == "this")
                builder.Append(TEMP_THIS);
            else
                builder.Append(node.Name.Name);
        }
        public override void Visit(ReferenceMember node)
        {
            REF _ref;
            syntax.TryGetValue(node, out _ref);

            CSharpMember member = null;
            CSharpType type = null;
            if (_ref != null)
            {
                member = _ref.Definition.Define as CSharpMember;
                type = _ref.Definition.Define as CSharpType;
            }

            // 泛型：new List<int>() => new (List(int)).$()
            if (node.GenericTypes.Count > 0)
                builder.Append("(");

            CSharpType invokeType = null;
            // 指定有类型或实例调用的静态或实例成员 a.b | byte.MaxValue
            if (node.Target != null)
            {
                invokeType = GetSyntaxType(node.Target);
                // 显示调用从父类型继承的静态成员，此时应该调用父类型而非继承类型 ChildType.StaticMember => ParentType.StaticMember
                // 若值为true，则是调用父类继承的成员，将不执行Visit(node.Target)
                bool parentFlag = false;

                REF ref2;
                // 结果返回false可能是生成代码时产生的语法树
                if (syntax.TryGetValue(node.Target, out ref2))
                {
                    var t2 = ref2.Definition.Define as CSharpType;
                    if (member == null)
                    {
                        if (type != null && type.ContainingType != null)
                        {
                            // 显示调用从父类型继承的类型
                            if (t2 != null && !type.ContainingType.Equals(t2))
                            {
                                builder.Append(GetTypeName(t2));
                                parentFlag = true;
                            }
                        }
                        //else: 局部变量(VAR)
                    }
                    else
                    {
                        if (member.IsStatic || member.IsConstant)
                        {
                            if (t2 != null && !(node.Target is New) 
                                && (!(node.Target is ReferenceMember) 
                                // 例如this继承IEnumerable<T>，this会使t2 != null，此时应该调用的是扩展方法
                                    || (((ReferenceMember)node.Target).Name.Name != "this" && ((ReferenceMember)node.Target).Name.Name != "base")))
                            {
                                // 显示调用从父类型继承的静态成员
                                builder.Append(GetTypeName(member.ContainingType));
                                parentFlag = true;
                            }
                            else
                            {
                                // 调用扩展方法： list.First(lambda) => Enumerable.First(list, lambda);
                                builder.Append("{0}", GetTypeName(member.ContainingType));
                                exThisParam = node.Target;
                                parentFlag = true;
                            }
                        }
                    }
                }// end of TryGetValue
                // 生成代码时的特殊调用，例如this.constructor
                //else
                //    throw new NotImplementedException();

                // 忽略完全限定名的命名空间
                if (ref2 == null || !(ref2.Definition.Define is CSharpNamespace))
                {
                    // base成员会生成b_Member，此时不需要'.'
                    if (node.Target is ReferenceMember && ((ReferenceMember)node.Target).Name.Name == "base")
                    {
                        CSharpType baseType = ref2.Definition.Define as CSharpType;
                        // base.A => _1A (_[继承深度])
                        //if (!InOperator)
                        WriteThis();
                        builder.Append("._{0}", GetInheritDepth(baseType));
                    }
                    else
                    {
                        if (!parentFlag)
                            Visit(node.Target);
                        //if (!(node.Target is ReferenceMember) || ((ReferenceMember)node.Target).Name.Name != "base")
                        builder.Append(".");
                    }
                }
            }

            bool defineOnly = false;

            // base.Member => this.base_Member
            //if (node.Name.Name == "base")
            //{
            //    builder.Append("this.{0}", BASE);
            //}
            if (node.Name.Name == "this")
            {
                WriteThis();
            }
            else
            {
                if (_ref != null)
                {
                    if (member != null)
                    {
                        if (node.Target == null && node.Name.Name != "this")
                        {
                            // 第一个引用不带任何前缀时，需要加上引用域前缀
                            if (!member.IsConstructor)
                            {
                                if (member.IsStatic || member.IsConstant)
                                {
                                    // 静态成员
                                    builder.Append("{0}.", GetTypeName(member.ContainingType));
                                }
                                else
                                {
                                    // 实例成员
                                    if (!initializing)
                                    {
                                        WriteThis();
                                        builder.Append(".");
                                    }
                                }
                            }
                        }

                        if (member.IsConstructor)
                        {
                            if (node.IsGeneric)
                            {
                                // (List(int)).constructor
                                builder.Append("(");
                            }
                            // 例如JS定义的类型Date，调用其无参构造函数将变成new Date.Date()导致报错
                            defineOnly = IsDefineOnly(member.ContainingType);
                            if (!defineOnly)
                                // Type.constructor时，node引用member，这里添加类型名
                                builder.Append(member.ContainingType.Name.Name);
                        }
                        // Indexer应该已经不存在了，改由InvokeMethod处理
                        else if (member.IsProperty || member.IsIndexer)
                        {
                            // 默认为get，set的部分由运算符自行解决
                            builder.Append("{0}{1}", GET, member.Name.Name);
                            if (member.IsProperty)
                                builder.Append("()");
                        }
                        else if (member.IsMethod && invokeType == CSharpType.CHAR && member.Name.Name == "toString")
                        {
                            // char|enum.ToString等的特殊处理
                            builder.Append("$c2s");
                        }
                        else
                        {
                            builder.Append(member.Name.Name);
                        }
                    }
                    else if (type != null)
                    {
                        if (type.IsDelegate)
                        {
                            builder.Append("MulticastDelegate");
                        }
                        else
                        {
                            //if (type.IsArray)
                            //    builder.Append(GetTypeName(type));
                            builder.Append(GetTypeDefinitionName(type));
                        }
                    }
                    else
                    {
                        var _var = _ref.Definition.Define as VAR;
                        if (_var != null)
                        {
                            // HACK：局部变量 在闭包里也可能调用的是局部变量，可能还是需要对作用域进行判定
                            //if (inYieldEnumerable && !InClosure && !_var.Name.Name.StartsWith("this."))
                            //{
                            //    builder.Append("this.");
                            //}
                        }
                        // 引用类型或局部变量
                        WriteReferenceValue(node);
                    }
                }
                else
                {
                    // 构造的IEnumerator新增的语法，原样输出即可
                    WriteReferenceValue(node);
                }
            }

            if (node.GenericTypes.Count > 0)
            {
                //if (type == null || !type.IsDelegate)
                if (type == null)
                {
                    builder.Append("(");
                    for (int i = 0, n = node.GenericTypes.Count - 1; i <= n; i++)
                    {
                        Visit(node.GenericTypes[i]);
                        if (i != n)
                            builder.Append(", ");
                    }
                    builder.Append(")");
                }

                builder.Append(")");
            }
            //else
            //{
            //    // 在WriteInvoke中已实现
            //    if (member != null)
            //    {
            //        // 隐式决定泛型类型调用的方法
            //        builder.Append("(");
            //        var arguments = member.TypeArguments;
            //        for (int i = 0, n = arguments.Count - 1; i <= n; i++)
            //        {
            //            builder.Append(GetTypeName(arguments[i]));
            //            if (i != n)
            //                builder.Append(", ");
            //        }
            //        builder.Append("))");
            //    }
            //}

            // 第一个引用不带任何前缀时，需要加上引用域前缀
            if (member != null && member.IsConstructor)
            {
                // (List(int)).constructor
                if (node.IsGeneric)
                    builder.Append(")");
                // 防止new Date();变成new Date.Date();
                if (!defineOnly)
                    builder.Append(".");
                builder.Append(member.Name.Name);
            }

            if (node.ArrayDimension > 0)
                //throw new ArgumentException("声明类型变成了var，数组引用应该已经没有");
                ;
            //for (int i = 0; i < node.ArrayDimension; i++)
            //    builder.Append("[]");
        }

        private string GetThis()
        {
            if (inYieldEnumerable && !isStaticYield)
                return TEMP_THIS;
            else
                return "this";
        }
        private void WriteThis()
        {
            builder.Append(GetThis());
        }
        public override void Visit(UsingNamespace node)
        {
            throw new NotImplementedException();
        }
        public override void Visit(TypeOf node)
        {
            builder.Append("$tof");
            builder.Append("(");
            if (node.Reference.GenericTypes.Any(g => g == null))
            {
                // typeof(List<>)
                throw new NotImplementedException("不支持typeof(泛型原型)");
            }
            else
            {
                Visit(node.Reference);
            }
            builder.Append(")");
        }
        public override void Visit(SizeOf node)
        {
            // 转换成常量
            CSharpType type = (CSharpType)syntax[node.Reference].Definition.Define;
            builder.Append(GetStructSize(type));
        }
        public override void Visit(DefaultValue node)
        {
            builder.Append(DEFAULT);
            builder.Append("(");
            Visit(node.Reference);
            builder.Append(")");
        }
        public override void Visit(InvokeMethod node)
        {
            /*
            * return Method(ref a, out b) =>
            * (function()
            * {
            *   var __a = new ref(a);
            *   var __b = new ref(b);
            *   var __temp = Method(__a, __b);
            *   a = __a.v;
            *   b = __b.v;
             *   return __temp;
            * })()
             * 
             * new Method(ref a, out b) =>
             * {
            *   var __a = new ref(a);
            *   var __b = new ref(b);
            *  var __temp = new Method(__a, __b);
            *   a = __a.v;
            *   b = __b.v;
             *   return __temp;
            * })()
            */

            var refNew = this.refNewNode;
            this.refNewNode = null;

            // 泛型构造函数调用 new (List(int))();
            CSharpType returnType;
            object define;
            if (constructorTarget == null)
            {
                REF temp;
                if (syntax.TryGetValue(node, out temp))
                    define = temp.Definition.Define;
                else
                    // 可能是yield中生成的语法节点
                    define = null;
            }
            else
                define = syntax[constructorTarget].Definition.Define;
            if (define is CSharpMember)
            {
                // 方法
                CSharpMember member = (CSharpMember)define;
                if (member.IsConstructor)
                    returnType = member.ContainingType;
                else
                    returnType = member.ReturnType;
            }
            else if (define is CSharpType)
            {
                // 类型：默认构造函数
                returnType = (CSharpType)define;
            }
            else if (define is VAR)
            {
                // VAR：数组类型变量或委托类型变量
                VAR _var = (VAR)define;
                if (_var.Type.IsArray)
                {
                    returnType = _var.Type.ElementType;
                }
                else if (_var.Type.IsDelegate)
                {
                    returnType = _var.Type.DelegateInvokeMethod.ReturnType;
                }
                else
                    throw new InvalidCastException();
            }
            else
            {
                // yield中生成的语法节点
                returnType = null;
            }

            bool hasRefParam = HasRefParam(node);
            bool hasReturnType = returnType != null && returnType != CSharpType.VOID;
            if (hasRefParam)
            {
                //inClosure++;
                // 方法参数带ref|out时，调用前后分别追加新建临时变量和赋值最终变量
                // 没有返回值不需要闭包
                if (hasReturnType)
                {
                    builder.AppendLine("(function()");
                    builder.AppendLine("{");
                }
                for (int i = 0; i < node.Arguments.Count; i++)
                {
                    var item = node.Arguments[i];
                    if (item.Direction == EDirection.REF || item.Direction == EDirection.OUT)
                    {
                        builder.Append("var {0}{1} = {2}.Alloc(", REF, i, REF_OUT);
                        Visit(item.Expression);
                        builder.AppendLine(");");
                    }
                }
            }
            if (hasRefParam && hasReturnType)
                builder.Append("var {0} = ", ANONYMOUS_RET);
            if (refNew != null)
            {
                WriteConstroctur(refNew);
            }
            else
            {
                WriteInvoke(node);
            }

            // 引用参数的结尾
            if (hasRefParam)
            {
                builder.AppendLine(";");
                for (int i = 0; i < node.Arguments.Count; i++)
                {
                    var item = node.Arguments[i];
                    if (item.Direction == EDirection.REF || item.Direction == EDirection.OUT)
                    {
                        // 结构体内部的某方法内调用ref this时
                        // 1. 对字段赋值：因为JS本来就是引用类型，所以可以省略this = __0;
                        // 2. 对this赋值：内部改变对象，外部对象实际是所有字段的值也发生了改变，应该拷贝所有的字段
                        if (item.Expression is ReferenceMember && ((ReferenceMember)item.Expression).Name.Name == "this")
                        {
                            builder.AppendLine("{0}{1}.Release().{2}(this);", REF, i, STRUCT_COPY);
                        }
                        else
                        {
                            Visit(item.Expression);
                            builder.AppendLine(" = {0}{1}.Release();", REF, i);
                        }
                        // 调用方法的表达式可能导致后面多出一个";"
                    }
                }
                if (hasReturnType)
                {
                    builder.AppendLine("return {0};", ANONYMOUS_RET);
                    builder.AppendLine("}");
                    builder.Append(").bind(this)(");
                    //inClosure--;
                    //WriteThis();
                    builder.Append(")");
                }
            }
        }
        void WriteInvoke(InvokeMethod node)
        {
            exThisParam = null;
            Visit(node.Target);
            REF _ref;
            if (constructorTarget == null && syntax.TryGetValue(node.Target, out _ref))
            {
                if (node.IsIndexer)
                {
                    // ?
                }
                else
                {
                    var type = _ref.Definition.Define as CSharpType;
                    if (type != null && !IsDefineOnly(type))
                    {
                        // 调用类型未显示声明的构造函数
                        builder.Append(".$");
                    }
                }
            }
            syntax.TryGetValue(node, out _ref);
            bool indexerFlag = true;
            CSharpMember _member = null;
            if (_ref != null && node.IsIndexer)
            {
                _member = _ref.Definition.Define as CSharpMember;
                if (_member != null && _member.IsIndexer &&
                    // ArrayBuffer的数组形式调用变成了调用索引器导致语法错误
                    !IsJSArray(_member))
                {
                    builder.Append(".{0}{1}", GET, _member.Name.Name);
                    indexerFlag = false;
                }
            }
            // indexer只有是调用索引器时才会被改为()
            if (node.IsIndexer && indexerFlag)
            {
                // 数组array[index] | 交错数组定义new int[length]([])
                if (node.Arguments != null && node.Arguments.Count > 0)
                {
                    builder.Append("[");
                    VisitActualArgument(node.Arguments, _member);
                    builder.Append("]");
                }
            }
            else
            {
                if (_ref != null)
                {
                    _member = _ref.Definition.Define as CSharpMember;
                    //if (member != null && (member.IsMethod || member.IsIndexer))
                    if (_member != null && _member.IsMethod)
                    {
                        // 若为泛型方法，node引用的则是泛型方法的原型，用于防止被优化掉的
                        _member = (CSharpMember)syntax[node.Target].Definition.Define;
                        if (!((ReferenceMember)node.Target).IsGeneric)
                        {
                            var typeArguments = _member.TypeArguments;
                            if (typeArguments.Count > 0)
                            {
                                builder.Append("(");
                                // 调用泛型方法时将泛型参数的部分作为方法参数调用 声明:Test<T>(T a) => 调用:Test(int)(a)
                                for (int i = 0, n = typeArguments.Count - 1; i <= n; i++)
                                {
                                    builder.Append(GetTypeName(typeArguments[i]));
                                    if (i != n)
                                        builder.Append(", ");
                                }
                                builder.Append(")");
                            }
                        }
                    }

                    // 委托类型调用Invoke方法，Function.prototype也追加了这个方法
                    CSharpType dtype = null;
                    if (_member != null)
                    {
                        if (!_member.IsConstructor && !_member.IsIndexer && _member.ReturnType.IsDelegate)
                            dtype = _member.ReturnType;
                    }
                    else if (_ref.Definition.Define is CSharpType)
                    {
                        dtype = (CSharpType)_ref.Definition.Define;
                        if (!dtype.IsDelegate)
                            dtype = null;
                    }
                    else
                    {
                        VAR _var = _ref.Definition.Define as VAR;
                        if (_var != null && _var.Type.IsDelegate)
                            dtype = _var.Type;
                    }
                    if (dtype != null)
                    {
                        builder.Append(".Invoke");
                    }
                }
                builder.Append("(");
                VisitActualArgument(node.Arguments, _member);
                builder.Append(")");
            }
        }
        public override void Visit(List<InvokeAttribute> node)
        {
            // 特性信息已经在生成反射代码时包含进去了
        }
        public override void Visit(New node)
        {
            // BUG: where T : new()内部使用new T()时，没法正确指向构造函数

            // new Action<int>(Method) => new MultipleDelegate(Method)
            REF _ref;
            if (syntax.TryGetValue(node.Method, out _ref))
            {
                CSharpType dConstructor = _ref.Definition.Define as CSharpType;
                if (dConstructor != null && dConstructor.IsDelegate)
                {
                    VisitActualArgument(node.Method.Arguments, dConstructor.Members.First(d => d.IsConstructor));
                    return;
                }
            }
            if (HasRefParam(node.Method))
            {
                refNewNode = node;
                Visit(node.Method);
            }
            else
            {
                WriteNew(node);
            }
        }
        void WriteNew(New node)
        {
            if (node.Initializer != null && node.Initializer.Count > 0)
            {
                if (node.IsNewArray)
                {
                    ArrayValue array = new ArrayValue();
                    array.Values = node.Initializer;
                    Visit(array);
                }
                else
                {
                    /*
                    * C#:
                    * VECTOR2 v = new VECTOR2() { X = 5, Y = 10 };
                    * 
                    * JS:
                    * var v = (function() { var __obj = new VECTOR2(); __obj.X = 5; __obj.Y = 5; return __obj; })();
                    */
                    // 生成临时方法并调用方法得到返回的对象
                    builder.AppendLine("(function()");
                    builder.AppendLine("{");
                    builder.Append("var {0} = ", ANONYMOUS_OBJ);
                    WriteConstroctur(node);
                    builder.AppendLine(";");
                    initializing = true;
                    foreach (var item in node.Initializer)
                    {
                        builder.Append("{0}.", ANONYMOUS_OBJ);
                        Visit(item);
                        builder.AppendLine(";");
                    }
                    initializing = false;
                    builder.AppendLine("return {0};", ANONYMOUS_OBJ);
                    builder.AppendLine("}");
                    builder.Append(").bind(this)()");
                }
            }
            else
            {
                if (node.IsNewArray)
                {
                    // 一定是有数组长度的数组构造函数
                    // 构架定长数组改用_API.CreateArray<T>(int size)以对创建的数组的元素赋值默认值
                    builder.Append("_API.CreateArray(");
                    Visit(node.Type);
                    builder.Append(")(");
                    Visit(node.Method.Arguments[0].Expression);
                    builder.Append(")");
                }
                else
                {
                    WriteConstroctur(node);
                }
            }
        }
        static ReferenceMember STRING_CREATE = new ReferenceMember(new Named("String.Create"));
        static InvokeMethod STRING_CREATE2 = new InvokeMethod() { Target = STRING_CREATE };
        void WriteConstroctur(New node)
        {
            refNewNode = null;
            if (node.Method.Target is ReferenceMember)
            {
                ReferenceMember rm = (ReferenceMember)node.Method.Target;
                // new string(char[] chars) => String.Create
                if (rm.Name.Name == "string" || rm.Name.Name == "String")
                {
                    STRING_CREATE2.Arguments = node.Method.Arguments;
                    WriteInvoke(STRING_CREATE2);
                    return;
                }
            }
            builder.Append("new ");
            //ReferenceMember rt = node.Type;
            //REF r = syntax[rt];
            //if (r.Definition.Define is CSharpMember)
            //{
            //    // 显示声明的构造函数是类型的静态成员 例如A的默认构造函数调用为new A.A0()
            //    CSharpMember member = (CSharpMember)r.Definition.Define;
            //    builder.Append("{0}.", GetTypeName(member.ContainingType));
            //}
            // else: 未显示声明的默认无参构造函数
            WriteInvoke(node.Method);
        }
        public override void Visit(As node)
        {
            builder.Append("{0}(", AS);
            Visit(node.Expression);
            builder.Append(',');
            Visit(node.Reference);
            builder.Append(")");
        }
        public override void Visit(Is node)
        {
            builder.Append("{0}(", IS);
            Visit(node.Expression);
            builder.Append(',');
            Visit(node.Reference);
            builder.Append(')');
        }
        public override void Visit(Cast node)
        {
            // (sbyte)T.op(exp) | (sbyte)exp => (T.op(exp) & 255) | ((exp) & 255)
            // 对于不需要检测溢出的情况
            // 1. cast大于等于type 2. 非数字类型 3. 表达式类型为null
            CSharpType cast = (CSharpType)syntax[node.Type].Definition.Define;
            CSharpType type = GetSyntaxType(node.Expression);

            REF _ref;
            CSharpMember member = null;
            if (syntax.TryGetValue(node, out _ref) && (member = _ref.Definition.Define as CSharpMember) != null)
                // explicit 对于返回类型是int的，用(sbyte)也可以调用运算符
                type = member.ReturnType;

            // (cast)type: 若对于int转byte，byte转sbyte，sbyte转byte都需要进行类型溢出或转换
            bool isNumber1 = Refactor.GetNumberType(cast, out cast);
            bool isNumber2 = Refactor.GetNumberType(type, out type);
            bool isOverflow = false;
            bool f2i = false;
            if (isNumber1 && isNumber2)
            {
                int size1 = Refactor.GetPrimitiveTypeSize(cast);
                int size2 = Refactor.GetPrimitiveTypeSize(type);

                // 小数转整数
                if ((type == CSharpType.FLOAT || type == CSharpType.DOUBLE) && (cast != CSharpType.FLOAT && cast != CSharpType.DOUBLE))
                {
                    f2i = true;
                }

                if (size1 < size2 ||
                    (size1 == size2 &&
                    !(cast.Equals(type)
                        || cast == CSharpType.FLOAT
                        || cast == CSharpType.DOUBLE)))
                {
                    // 检测类型溢出
                    isOverflow = true;
                }
            }

            if (f2i)
                builder.Append("((");
            if (isOverflow)
                builder.Append("{0}(", Overflow(cast));

            // 字符转换，枚举转换：一般情况下为数字，ToString时特殊处理
            if (member != null)
            {
                // explicit: ContainingType.op_Explicit[ReturnType](expression)
                //builder.Append("{0}.{1}{2}(", GetTypeName(member.ContainingType), member.Name.Name, member.ReturnType.Name.Name);
                builder.Append("{0}.{1}(", GetTypeName(member.ContainingType), member.Name.Name);
                Visit(node.Expression);
                builder.Append(")");
            }
            else
            {
                Visit(node.Expression);
            }

            if (isOverflow)
                builder.Append(")");
            if (f2i)
                builder.Append(")|0)");
        }

        internal static string GetDefaultValueCode(CSharpType type)
        {
            if (type.IsArray)
                return "null";
            else if (type.IsEnum)
            {
                var def = type.MemberDefinitions.FirstOrDefault();
                if (def == null)
                    return "0";
                else
                    return string.Format("{0}.{1}", GetTypeName(type), def.Name.Name);
            }
            else if (type == CSharpType.BOOL) return "false";
            else if (type == CSharpType.SBYTE
                || type == CSharpType.BYTE
                || type == CSharpType.SHORT
                || type == CSharpType.USHORT
                || type == CSharpType.INT
                || type == CSharpType.UINT
                || type == CSharpType.FLOAT
                || type == CSharpType.DOUBLE
                || type == CSharpType.LONG
                || type == CSharpType.ULONG)
                return "0";
            else if (type == CSharpType.CHAR)
            {
                // 可能需要的特殊处理
                return "0";
            }
            else if (type == CSharpType.STRING) return "\"\"";
            else if (type.IsValueType)
                return string.Format("new {0}.$()", GetTypeName(type));
            else if (type.IsEnum)
                return string.Format("{0}.{1}", GetTypeName(type), type.Members.MaxMin(m => (int)Convert.ChangeType(m.ConstantValue, typeof(int)), true).Name.Name);
            else if (type.IsTypeParameter)
                //return string.Format("$dft({0})", DEFAULT, GetTypeName(type));
                return string.Format("$dft({0})", type.Name.Name);
            else
                return "null";
        }
        internal static string GetTypeDefinitionName(CSharpType t)
        {
            if (t == null)
                return null;

            if (t.ContainingType == null && !t.IsConstructed)
            {
                // 数组类型 reader.ReadObject<Piece[]>() => reader.ReadObject(___Array(Piece))()
                if (t.IsArray)
                {
                    return string.Format("$array({0})", GetTypeName(t.ElementType));
                    //return string.Format("___Array({0})", GetTypeName(t.ElementType));
                }
                else if (t.TypeParametersCount > 0)
                {
                    // 泛型定义类型
                    return GENERIC_NAME;
                }
                return t.Name.Name;
            }

            StringBuilder builder = new StringBuilder();
            var type = t;
            // 泛型
            var typeArguments = type.TypeArguments;
            int n = typeArguments.Count - 1;
            if (n >= 0)
                builder.Append("(");
            builder.Append(type.Name.Name);
            if (n >= 0)
            {
                builder.Append("(");
                for (int i = 0; i <= n; i++)
                {
                    builder.Append(GetTypeName(typeArguments[i]));
                    if (i != n)
                        builder.Append(", ");
                }
                builder.Append("))");
            }
            return builder.ToString();

            //if (t.IsArray)
            //    return GetTypeDefinitionName(t.ElementType);
            //return t.Name.Name;
        }
        internal static string GetTypeName(CSharpType t)
        {
            if (t == null)
                return null;

            if (t.ContainingType == null && !t.IsConstructed)
            {
                // 数组类型 reader.ReadObject<Piece[]>() => reader.ReadObject(___Array(Piece))()
                if (t.IsArray)
                {
                    return string.Format("$array({0})", GetTypeName(t.ElementType));
                    //return string.Format("___Array({0})", GetTypeName(t.ElementType));
                }
                else if (t.TypeParametersCount > 0)
                {
                    // 泛型定义类型
                    return GENERIC_NAME;
                }
                return t.Name.Name;
            }

            StringBuilder builder = new StringBuilder();
            Stack<string> names = new Stack<string>();
            var type = t;
            while (type != null)
            {
                // 父类型 | 泛型
                var typeArguments = type.TypeArguments;
                int n = typeArguments.Count - 1;
                if (n >= 0)
                    builder.Append("(");
                builder.Append(type.Name.Name);
                if (n >= 0)
                {
                    builder.Append("(");
                    for (int i = 0; i <= n; i++)
                    {
                        builder.Append(GetTypeName(typeArguments[i]));
                        if (i != n)
                            builder.Append(", ");
                    }
                    builder.Append("))");
                }
                names.Push(builder.ToString());
                builder.Clear();
                type = type.ContainingType;
                //break;
            }
            while (names.Count > 0)
            {
                builder.Append(names.Pop());
                if (names.Count != 0)
                    builder.Append('.');
            }
            return builder.ToString();
        }
        internal static int GetStructSize(CSharpType t)
        {
            if (!t.IsStruct)
                return 4;
            var members = t.Members;
            // 基础类型
            int size = Refactor.GetPrimitiveTypeSize(t);
            if (size == -1)
            {
                // 其它自定义结构类型
                size = 0;
                foreach (var item in members)
                {
                    if (!item.IsField)
                        continue;
                    var type = item.ReturnType;
                    int s = Refactor.GetPrimitiveTypeSize(type);
                    if (s != -1) size += s;
                    else size += GetStructSize(type);
                }
            }
            return size;
        }
        internal static string Overflow(CSharpType type)
        {
            if (type == CSharpType.BYTE)
                return "$toB";
            else if (type == CSharpType.SBYTE)
                return "$toSB";
            else if (type == CSharpType.USHORT)
                return "$toUS";
            else if (type == CSharpType.SHORT)
                return "$toS";
            else if (type == CSharpType.UINT)
                return "$toUI";
            else if (type == CSharpType.SBYTE)
                return "$toI";
            else if (type == CSharpType.ULONG)
                return "$toUL";
            else if (type == CSharpType.LONG)
                return "$toL";
            else
                return null;
        }
        internal static int GetInheritDepth(CSharpType t)
        {
            int depth = 0;
            while ((t = t.BaseClass) != null)
                depth++;
            return depth;
        }

        /// <summary>定义的所有类型，按照被引用的类型优先排序</summary>
        List<DefineMember> DefineTypes = new List<DefineMember>();
        /// <summary>还未添加的有继承的类型，需要用于排序后逐步生成类型</summary>
        Dictionary<CSharpType, DefineMember> DefineTypes2 = new Dictionary<CSharpType, DefineMember>();
        void AddDefineType(DefineMember type)
        {
            if (type is DefineType)
            {
                var dt = ((DefineType)type);
                if (dt.Inherits.Count == 0)
                    DefineTypes.Add(type);
                else
                    DefineTypes2.Add(types[type].Type, type);
                //foreach (var item in dt.NestedType)
                //    AddDefineType(item);
            }
            else
                // enum | delegate
                DefineTypes.Add(type);
        }
        /// <summary>获取类型引用了的所有类型</summary>
        /// <param name="type">类型</param>
        /// <returns>引用了的所有类型</returns>
        IEnumerable<CSharpType> TypeReferences(CSharpType type)
        {
            if (type.BaseClass != null)
                yield return type.BaseClass;
            foreach (var item in type.BaseInterfaces)
                yield return item;
        }
        internal override void WriteBegin(IEnumerable<DefineFile> files)
        {
            // 添加全部的类型然后排序
            foreach (var item in files)
                foreach (var n in item.DefineNamespaces)
                    foreach (var t in n.DefineTypes)
                        AddDefineType(t);
            // 没有继承的类型已经加入DefineTypes，需要排序加入的类型在DefineTypes2
            while (DefineTypes2.Count > 0)
            {
                int count = DefineTypes2.Count;
                // 本次被添入的类型
                List<DefineMember> addTypes = new List<DefineMember>();
                foreach (var item in DefineTypes2)
                {
                    if (TypeReferences(item.Key).Any(i => DefineTypes2.ContainsKey(i)))
                        continue;
                    addTypes.Add(item.Value);
                }
                if (addTypes.Count == 0)
                    // 所有类型都互相有引用，陷入了死循环
                    throw new Exception("所有类型都互相有引用，陷入了死循环");
                foreach (var item in addTypes)
                {
                    DefineTypes2.Remove(types[item].Type);
                    DefineTypes.Add(item);
                }
            }

            YieldJS.Initialize(this);
            builder = new StringBuilder();
            // 需要生成反射程序集信息的类型
            this.reflexibleTypes = ReflexibleTypes.Reflexible(files, this);
            // 重命名类型和成员名称，达到减少代码量和混淆的作用
            Renamer.Rename(files, this, out this.renamedTypes, out this.renamedMembers);
            Renamer.Rename(CSharpType.STRING, this, "String");
            foreach (var item in CSharpType.STRING.MemberDefinitions)
            {
                string name = GetRenamedMemberOriginName(item);
                switch (name)
                {
                    case "Length":
                        // 把Length属性直接改为JS字符串的length字段
                        ((MemberDefinitionInfo)item)._Kind = MemberDefinitionKind.Field;
                        Renamer.Rename(item, this, "length");
                        break;

                    case "IndexOf":
                        if (item.Parameters[0].Type == CSharpType.STRING)
                            Renamer.Rename(item, this, "indexOf");
                        break;
                }
            }
            Renamer.Rename(CSharpType.OBJECT, this, "Object");
            Renamer.Rename(CSharpType.OBJECT.MemberDefinitions.First(m => m.Name.Name == "ToString"), this, "toString");
            Renamer.Rename(CSharpType.ARRAY, this, "Array");
            foreach (var item in CSharpType.ARRAY.MemberDefinitions)
            {
                string name = GetRenamedMemberOriginName(item);
                switch (name)
                {
                    case "Length":
                        // 把Length属性直接改为JS数组的length字段
                        ((MemberDefinitionInfo)item)._Kind = MemberDefinitionKind.Field;
                        Renamer.Rename(item, this, "length");
                        break;
                }
            }
            Renamer.Rename(CSharpType.MATH, this, "Math");
            foreach (var item in CSharpType.MATH.MemberDefinitions)
            {
                string oname = GetRenamedMemberOriginName(item);
                Renamer.Rename(item, this, oname.ToLower());
            }
            //StringBuilder sb = new StringBuilder();
            //foreach (var item in renamedTypes)
            //    sb.AppendLine(item.ToString());
            //System.IO.File.WriteAllText("index.txt", sb.ToString());

            builder.AppendLine("Number.prototype.GetHashCode = function() { return this; };");
            builder.AppendLine("Number.prototype.$c2s = function() { return String.fromCharCode(this); };");
            builder.AppendLine("Number.prototype.Equals = function(v) { return this.valueOf() === v; };");
            // todo: enum.ToString
            // builder.AppendLine("Number.prototype.$e2s = function() { this.GetType() }");
            builder.AppendLine("Function.prototype.Invoke = function() { return this.apply(null, arguments); };");

            /*
             * 关键字
             * 1. default
             * 2. is
             * 3. as
             * 4. typeof
             * 
             * 反射
             * 程序集类型信息
             */
            //builder.AppendLine(top_js);
            // default
            builder.AppendLine("function {0}(t)", DEFAULT);
            builder.AppendBlockWithEnd(() =>
            {
                builder.AppendLine("if (t == bool) return false;");
                builder.AppendLine("if (t == byte || t == sbyte || t == ushort || t == short || t == char || t == uint || t == int || t == float || t == ulong || t == long || t == double) return 0;");
                builder.AppendLine("if (t.prototype.{0}) return new t.$();", STRUCT_CLONE);
                builder.AppendLine("if (t.{0}) return 0;", ENUM);
                builder.AppendLine("return null;");
            });
            // is
            //builder.AppendLine("function {0}(v, t)", IS);
            //builder.AppendBlockWithEnd(() =>
            //{
            //    builder.AppendLine("if (v == null) return false;");
            //    builder.AppendLine("return v.GetType().{0}.indexOf(t.{1}) != -1;", INHERIT, TYPE_NAME);
            //});
            builder.AppendLine("function {0}(v, t)", IS);
            builder.AppendBlockWithEnd(() =>
            {
                builder.AppendLine("if (v == null) return false;");
                // Number特殊类型
                builder.AppendLine("if (v.constructor.name && v.constructor.name == \"Number\" && (t == byte || t == sbyte || t == ushort || t == short || t == char || t == uint || t == int || t == float || t == ulong || t == long || t == double)) return true;");
                // 一层继承关系可直接拿到
                builder.AppendLine("if (v.constructor.prototype.constructor == t) return true;");
                builder.AppendLine("var t1 = v.GetType();");
                builder.AppendLine("if (t1 == null) return false;");
                //builder.AppendLine("if (t1.{0} && t.{1}) return t1.{0}.indexOf(t.{1}) != -1;", INHERIT, TYPE_NAME);
                builder.AppendLine("var t2 = $tof(t);");
                builder.AppendLine("if (t2 == null) return false;");
                builder.AppendLine("if (t1.name && t2.name) return t1.name == t2.name;");
                builder.AppendLine("return t2.IsAssignableFrom(t1);");
            });
            // as
            builder.AppendLine("function {0}(v, t)", AS);
            builder.AppendBlockWithEnd(() => builder.AppendLine("return $is(v, t) ? v : null;"));
            // typeof
            builder.AppendLine("function {0}(t)", TYPE_OF);
            builder.AppendBlockWithEnd(() =>
            {
                builder.AppendLine("if (t.$type) return t.$type;");
                builder.AppendLine("if (t == {0}) return {1}(t.T).MakeArrayType();", ARRAY_TYPE, TYPE_OF);
                builder.AppendLine("if (!t.{0} && t.{0}2) t.{0} = t.{0}2();", TYPE_NAME);
                // BUG: Yield块生成的类型没有_TN
                // BUG: function没有_TN
                builder.AppendLine("if (!t.{0}) return null;", TYPE_NAME);
                builder.AppendLine("var type = _R.AllocType(t.{0});", TYPE_NAME);
                builder.AppendLine("var ret;");
                builder.AppendLine("if (type == null) ret = new SimpleType.SimpleType(t.{0});", TYPE_NAME);
                builder.AppendLine("else ret = _R.FromType(type);");
                builder.AppendLine("t.$type = ret;");
                builder.AppendLine("return ret;");
            });
            builder.AppendLine("function {0}(T)", ARRAY_TYPE);
            builder.AppendBlockWithEnd(() =>
            {
                builder.AppendLine("{0}.T = T;", ARRAY_TYPE);
                builder.AppendLine("return {0};", ARRAY_TYPE);
            });
            // ref
            builder.AppendLine("function {0}(v) {{ this.v = v; this.free = false; }}", REF_OUT);
            builder.AppendLine("{0}.queue = [];", REF_OUT);
            builder.AppendLine("{0}.Alloc = function(v)", REF_OUT);
            builder.AppendBlockWithEnd(() =>
            {
                builder.AppendLine("var queue = {0}.queue;", REF_OUT);
                builder.AppendLine("for (var i = 0; i < queue.length; i++)");
                builder.AppendBlock(() =>
                {
                    builder.AppendLine("if (queue[i].free)");
                    builder.AppendBlock(() =>
                    {
                        builder.AppendLine("queue[i].free = false;");
                        builder.AppendLine("queue[i].v = v;");
                        builder.AppendLine("return queue[i];");
                    });
                });
                builder.AppendLine("var result = new {0}(v);", REF_OUT);
                builder.AppendLine("queue.push(result);");
                builder.AppendLine("return result;");
            });
            builder.AppendLine("{0}.prototype.Release = function()", REF_OUT);
            builder.AppendBlockWithEnd(() =>
            {
                builder.AppendLine("this.free = true;");
                builder.AppendLine("return this.v;");
            });
            // Array.Create
            builder.AppendLine("function _API(){}");
            builder.AppendLine("_API.CreateArray = function(T)");
            builder.AppendBlockWithEnd(() =>
            {
                builder.AppendLine("return function(length)");
                builder.AppendBlock(() =>
                {
                    builder.AppendLine("var array = new Array(length);");
                    builder.AppendLine("if (T.prototype.{0})", STRUCT_CLONE);
                    builder.AppendBlock(() =>
                    {
                        builder.AppendLine("for (var i = 0; i < length; i++)");
                        builder.AppendLine("array[i] = new T.$();");
                    });
                    builder.AppendLine("else");
                    builder.AppendBlock(() =>
                    {
                        builder.AppendLine("var value = {0}(T);", DEFAULT);
                        builder.AppendLine("for (var i = 0; i < length; i++)");
                        builder.AppendLine("array[i] = value;");
                    });
                    builder.AppendLine("return array;");
                });
            });
            // const
            builder.AppendLine("window.$B8 = 255;");
            builder.AppendLine("window.$H8 = ($B8/2)|0;");
            builder.AppendLine("window.$R8 = -$B8;");
            builder.AppendLine("window.$B16 = 65535;");
            builder.AppendLine("window.$H16 = ($B16/2)|0;");
            builder.AppendLine("window.$R16 = -$B16 - 1;");
            builder.AppendLine("window.$B32 = 4294967295;");
            builder.AppendLine("window.$H32 = ($B32/2)|0;");
            builder.AppendLine("window.$R32 = -$B32 - 1;");
            builder.AppendLine("window.$B64 = 9007199254740991;");
            builder.AppendLine("window.$H64 = ($B64/2)|0;");
            builder.AppendLine("window.$R64 = -$B64 - 1;");
            builder.AppendLine("function $toB(v) { return v & $B8; }");
            builder.AppendLine("function $toSB(v) { return ((v & $B8) > $H8) ? (v | $R8) : (v & $B8); }");
            builder.AppendLine("function $toUS(v) { return v & $B16; }");
            builder.AppendLine("function $toS(v) { return ((v & $B16) > $H16) ? (v | $R16) : (v & $B16); }");
            builder.AppendLine("function $toUI(v) { return v & $B32; }");
            builder.AppendLine("function $toI(v) { return ((v & $B32) > $H32) ? (v | $R32) : (v & $B32); }");
            builder.AppendLine("function $toUL(v) { return v & $B64; }");
            builder.AppendLine("function $toL(v) { return (v > $H64) ? (v | $R64) : v; }");

            #region 反射
            /*
            * case "[Name]": 此名字应该是字段原名
            * 类似数据库或者Texture文件此类开发时生成的代码或文件
            * 其序列化时的信息包含的是类型原始名字
            * 
            * 类型名的话应该需要包含短名和全名两种
            */
            builder.AppendLine("function _R(){}");
            builder.AppendLine("_R.$TC = [];"); // CSharpType Cache
            builder.AppendLine("_R.RTC = [];"); // RuntimeType Cache
            builder.AppendLine("_R.AC = [];");  // Assembly Cache
            builder.AppendLine("_R.CAC = [];"); // CSharpAssembly Cache
            builder.AppendLine("_R.NC = [];");  // Namespace Cache

            #region CSharpType AllocType(string name)
            // 命名空间
            //builder.AppendLine("_R.AllocNS = function(name)");
            //builder.AppendBlock(() =>
            //{
            //    builder.AppendLine("var ns = _R.NC[name];");
            //    builder.AppendLine("if (!ns)");
            //    builder.AppendBlock(() =>
            //    {
            //        builder.AppendLine("var nss = name.Split(46);");
            //        builder.AppendLine("var last = nss.length - 1;");
            //        builder.AppendLine("for (var i = 0; i < last; i++)");
            //        builder.AppendLine("ns = _R.AllocNS(nss[i]);");
            //        builder.AppendLine("var nns = new CSharpNamespace.CSharpNamespace();");
            //        builder.AppendLine("nns.{0}Name(nss[last]);", SET);
            //        builder.AppendLine("if (ns) ns.AddNamespace(nns);");
            //        builder.AppendLine("ns = nns;");
            //    });
            //    builder.AppendLine("return ns;");
            //});
            // CSharpType
            // todo: 生成代码太多，考虑生成序列化信息，然后反序列化来创建
            builder.AppendLine("_R.BuildType = function(name)");
            builder.AppendBlockWithEnd(() =>
            {
                builder.AppendLine("switch (name)");
                builder.AppendBlock(() =>
                {
                    //foreach (var type in reflexibleTypes)
                    foreach (var type in types.Values.Select(t => t.Type))
                    {
                        if (!HasType(type) || IsDefineOnly(type))
                            continue;
                        var type2 = type as TypeDefinitionInfo;
                        if (type2 == null)
                            continue;
                        bool reflexible = reflexibleTypes.Contains(type);
                        // 实际生成的类型名称
                        string typeName = GetRenamedRuntimeTypeName(type);
                        // 只针对case中的字符串
                        var fullName = GetRenamedRuntimeTypeName(type);
                        //if (type.TypeParametersCount > 0)
                        //{
                        //    // todo: 暂不支持泛型类型的反射
                        //    Console.WriteLine("暂不支持泛型类型的反射 type:{0}", fullName);
                        //    continue;
                        //}
                        builder.AppendLine("case \"{0}\":", fullName);
                        builder.Append("var type = _R.CreateType(name,");
                        builder.Append("\"{0}\",", type.Assembly.Name);
                        builder.Append("{0},", (int)type2._TypeAttributes);
                        if (type2._UnderlyingType != null)
                            builder.Append("\"{0}\",", GetRenamedRuntimeTypeName(type.UnderlyingType));
                        else
                            builder.Append("null,");
                        //if (type.ContainingNamespace != null)
                        //{
                        //    builder.AppendLine("_R.AllocNS(\"{0}\").AddType(type);", type.ContainingNamespace.ToString());
                        //}
                        // 跟Namespace一样，类型完全名直接生成了，但最终还是需要考虑和C#一样，名字只是短名字，需要Namespace和ContainingType
                        //if (type.ContainingType != null)
                        //{
                        //    //if (!reflexibleTypes.Contains(type.ContainingType))
                        //    //    throw new InvalidCastException("父类型也应标记为可反射的");
                        //    builder.AppendLine("_R.AllocType(\"{0}\").Add(type);", GetRenamedRuntimeTypeName(type.ContainingType));
                        //}
                        if (type.TypeParametersCount > 0)
                        {
                            var tparameters = type.TypeParameters;
                            //builder.AppendLine("type._TypeParameters = new Array({0});", tparameters.Count);
                            //for (int i = 0; i < tparameters.Count; i++)
                            //{
                            //    var parameter = tparameters[i];
                            //    builder.AppendLine("type._TypeParameters[{0}] = new TypeParameterData.$();", i);
                            //    builder.AppendLine("type._TypeParameters[{0}].Name = new Named.Named(\"{1}\");", i, parameter.Name.Name);
                            //}
                            builder.Append("[");
                            for (int i = 0, n = tparameters.Count - 1; i <= n; i++)
                            {
                                builder.Append("\"{0}\"", tparameters[i].Name.Name);
                                if (i != n)
                                    builder.Append(",");
                            }
                            builder.Append("],");
                        }
                        else
                            builder.Append("null,");
                        if (type2._BaseTypes.Count > 0)
                        {
                            //foreach (var item in type2._BaseTypes)
                            //{
                            //    //if (HasType(item) && reflexibleTypes.Contains(CSharpType.GetDefinitionType(item)))
                            //    if (HasType(item))
                            //    {
                            //        builder.AppendLine("var _baseType = _R.AllocType(\"{0}\");", GetRenamedRuntimeTypeName(item));
                            //        builder.AppendLine("if (_baseType != null) type._BaseTypes.Add(_baseType);");
                            //    }
                            //}
                            builder.Append("[");
                            for (int i = 0, n = type2._BaseTypes.Count - 1; i <= n; i++)
                            {
                                builder.Append("\"{0}\"", GetRenamedRuntimeTypeName(type2._BaseTypes[i]));
                                if (i != n)
                                    builder.Append(",");
                            }
                            builder.Append("]");
                        }
                        else
                            builder.Append("null");
                        builder.AppendLine(");");
                        var members = type.MemberDefinitions;
                        int memberIndex = 0;
                        if (reflexible && members.Count > 0)
                        {
                            //builder.AppendLine("var member;");
                            foreach (var member in members)
                            {
                                if (!member.IsPublic)
                                    continue;
                                // 暂时不生成方法的程序集信息
                                if (member.IsMethod || member.IsOperator || member.IsDestructor)
                                    continue;
                                var parameters = member.Parameters;
                                if (member.IsConstructor && !member.IsStatic && parameters.Count > 0)
                                    continue;
                                if (member.IsProperty && member.Accessors.Count != 2)
                                    continue;
                                MemberDefinitionInfo member2 = (MemberDefinitionInfo)member;
                                var memberName = GetRenamedMemberOriginName(member);
                                builder.Append("member = _R.CreateMember(type,{0},", memberIndex++);
                                builder.Append("\"{0}\",", memberName);
                                builder.Append("{0},", (int)member2._MemberAttributes);
                                builder.Append("{0},", (int)member2._Kind);
                                // ReturnType
                                if (member.ReturnType != null)
                                    builder.Append("\"{0}\",", GetRenamedRuntimeTypeName(member.ReturnType));
                                else
                                    builder.Append("null,");
                                // Parameters
                                //if (parameters.Count > 0)
                                //{
                                //    builder.AppendLine("member._Parameters = new Array({0});", parameters.Count);
                                //    for (int i = 0; i < parameters.Count; i++)
                                //    {
                                //        var parameter = parameters[i];
                                //        builder.AppendLine("var arg = new MemberDefinitionInfo.ParameterData.$();");
                                //        //builder.AppendLine("data.Direction = {0};", parameter.);
                                //        builder.AppendLine("arg.Name = new Named.Named(\"{0}\");", parameter.Name.Name);
                                //        builder.AppendLine("arg.Type = _R.AllocType(\"{0}\");", GetRenamedRuntimeTypeName(parameter.Type));
                                //        builder.AppendLine("member._Parameters[{0}] = arg;", i);
                                //    }
                                //}
                                // Accessors
                                if (member.IsProperty || member.IsIndexer)
                                {
                                    var acs = member.Accessors;
                                    var get = acs.FirstOrDefault(a => a.IsGetAccessor);
                                    //if (get != null)
                                    //    builder.AppendLine("member._Accessors.Add(new MemberAccessor.MemberAccessor(member, true));");
                                    var set = acs.FirstOrDefault(a => a.IsSetAccessor);
                                    //if (set != null)
                                    //    builder.AppendLine("member._Accessors.Add(new MemberAccessor.MemberAccessor(member, false));");
                                    int value;
                                    if (get != null && set != null)
                                        value = 3;
                                    else if (get != null)
                                        value = 1;
                                    else
                                        value = 2;
                                    builder.Append("{0},", value);
                                }
                                else
                                    builder.Append("0,");
                                // Attribute
                                //foreach (var item in member.Attributes)
                                //{
                                //    string name = GetRenamedRuntimeTypeName(item._AttributeType);
                                //    if (IsSerializableAttribute(name))
                                //        builder.AppendLine("member._Attributes.Add(_R.AllocType(\"{0}\"), null);", name);
                                //}
                                var atts = member.Attributes;
                                if (atts.Count > 0)
                                {
                                    builder.Append("[");
                                    for (int i = 0, n = atts.Count - 1; i <= n; i++)
                                    {
                                        string name = GetRenamedRuntimeTypeName(atts[i]._AttributeType);
                                        if (IsSerializableAttribute(name))
                                            builder.Append("\"{0}\"", name);
                                    }
                                    builder.Append("]");
                                }
                                else
                                    builder.Append("null");
                                builder.AppendLine(");");
                            }
                        }
                        // 写一个能让JS直接调用的默认的构造函数
                        //if (!CSharpType.IsConstructedType(type) && !members.Any(m => m.IsConstructor && !m.IsStatic && m.Parameters.Count == 0))
                        //    builder.AppendLine("type.$ctor = {0}.$;", type.Name.Name);
                        builder.AppendLine("return type;");
                        builder.AppendLine();
                    }// end of reflexibleTypes
                    builder.AppendLine("default: return null;");
                });// end of switch(name)
            });// end of _R.BuildType
            #endregion
            #region object Invoke(CSharpMember member, object obj, object[] args)
            builder.AppendLine("_R.Invoke = function(member, obj, args)");
            builder.AppendBlockWithEnd(() =>
            {
                builder.AppendLine("var ctype = member.{0}ContainingType();", GET);
                builder.AppendLine("var typename = CSharpType.GetRuntimeTypeName(ctype);", GET);
                // 调用父类型的成员时将typename赋值成父类型名称
                builder.AppendLine("while (true)");
                builder.AppendBlock(() =>
                {
                    builder.AppendLine("switch (typename)");
                    builder.AppendBlock(() =>
                    {
                        foreach (var type in reflexibleTypes)
                        {
                            // todo:（需要考虑父类成员调用的情况） 没有成员被引用的类型不生成
                            if (!type.Members.Any(m => HasReference(m)))
                                continue;
                            // 只针对case中的字符串
                            string typeName = GetRenamedRuntimeTypeName(type);
                            // 反射调用静态成员时的类型名称
                            string typeNameInvoke = GetTypeName(type);
                            builder.AppendLine("case \"{0}\":", typeName);
                            // 生成Members的调用
                            if (type.Members.Any(m => (m.IsField && !m.IsConstant) && HasMember(m)))
                            {
                                builder.AppendLine("if (member.{0}IsField())", GET);
                                builder.AppendBlock(() =>
                                {
                                    BuildInvokeReflexibleMember(type, (m) => m.IsField && !m.IsConstant,
                                        (member) =>
                                        {
                                            if (!member.IsConstant)
                                                builder.Append("if (args.length == 0)");
                                            if (member.IsStatic || member.IsConstant)
                                                builder.Append(" return {1}.{0};", member.Name.Name, typeNameInvoke);
                                            else
                                                builder.Append(" return obj.{0};", member.Name.Name);
                                            if (!(member.IsConstant || member.IsReadonly))
                                            {
                                                if (member.IsConstant || member.IsStatic)
                                                    builder.Append(" else {{ {1}.{0} = args[0]; return null; }}", member.Name.Name, typeNameInvoke);
                                                else
                                                    builder.Append(" else {{ obj.{0} = args[0]; return null; }}", member.Name.Name);
                                            }
                                            builder.AppendLine();
                                        });
                                });
                            }
                            if (type.Members.Any(m => m.IsProperty && HasMember(m)))
                            {
                                builder.AppendLine("if (member.{0}IsProperty())", GET);
                                builder.AppendBlock(() =>
                                {
                                    BuildInvokeReflexibleMember(type, (m) => m.IsProperty,
                                        (member) =>
                                        {
                                            var accessors = member.Accessors;
                                            bool hasGet = accessors.Any(a => a.IsGetAccessor);
                                            bool hasSet = accessors.Any(a => a.IsSetAccessor);
                                            if (hasGet)
                                            {
                                                if (hasSet)
                                                    builder.Append("if (args.length == 0)");
                                                if (member.IsStatic)
                                                    builder.Append(" return {1}.{2}{0}();", member.Name.Name, typeNameInvoke, GET);
                                                else
                                                    builder.Append(" return obj.{1}{0}();", member.Name.Name, GET);
                                            }
                                            if (hasSet)
                                            {
                                                if (hasGet)
                                                    builder.Append(" else {");
                                                if (member.IsStatic)
                                                    builder.Append("{1}.{2}{0}(args[0]); return null;", member.Name.Name, typeNameInvoke, SET);
                                                else
                                                    builder.Append("obj.{1}{0}(args[0]); return null;", member.Name.Name, SET);
                                                if (hasGet)
                                                    builder.Append("}");
                                            }
                                            builder.AppendLine();
                                        });
                                });
                            }
                            // 至少构造无参默认构造函数，能够调用Activator.CreateInstance
                            if (((type.IsStruct && !CSharpType.IsPrimitive(type))) || (type.IsClass && !type.IsStatic && !type.IsAbstract))
                            {
                                var constructor = type.Members.FirstOrDefault(m => m.IsConstructor && m.Parameters.Count == 0);
                                if (constructor != null)
                                    builder.AppendLine("return new {0}.{1}();", typeNameInvoke, constructor.Name.Name);
                                else
                                    // 默认无参构造函数
                                    builder.AppendLine("return new {0}.$();", typeNameInvoke);
                            }
                            //if (type.Members.Any(m => m.IsConstructor && HasMember(m)))
                            //{
                            //    builder.AppendLine("if (member.IsConstructor)");
                            //    builder.AppendBlock(() =>
                            //    {
                            //        BuildInvokeReflexibleMember(type, (m) => m.IsConstructor,
                            //            (member) =>
                            //            {
                            //                var parameters = member.Parameters;
                            //                builder.Append("return new {0}.{1}", typeNameInvoke, member.Name.Name);
                            //                builder.Append('(');
                            //                for (int i = 0, n = parameters.Count - 1; i <= n; i++)
                            //                {
                            //                    builder.Append("args[{0}]", i);
                            //                    if (i != n)
                            //                        builder.Append(", ");
                            //                }
                            //                builder.AppendLine(");");
                            //            });
                            //    });
                            //}
                            //if (type.Members.Any(m => m.IsIndexer && HasReference(m)))
                            //{
                            //    builder.AppendLine("if (member.IsIndexer)");
                            //    builder.AppendBlock(() =>
                            //    {
                            //        BuildInvokeReflexibleMember(type, (m) => m.IsIndexer,
                            //            (member) =>
                            //            {
                            //                var accessors = member.Accessors;
                            //                bool hasGet = accessors.Any(a => a.IsGetAccessor);
                            //                bool hasSet = accessors.Any(a => a.IsSetAccessor);
                            //                if (hasGet)
                            //                {
                            //                    var parameters = accessors.First(a => a.IsGetAccessor).Parameters;
                            //                    if (hasSet)
                            //                        builder.Append("if (args.length == 0 && args.length == {0})", parameters.Count);
                            //                    builder.Append(" return obj.{1}{0}(", member.Name.Name, GET);
                            //                    for (int i = 0, n = parameters.Count - 1; i <= n; i++)
                            //                    {
                            //                        builder.Append("args[{0}]", i);
                            //                        if (i != n)
                            //                            builder.Append(", ");
                            //                    }
                            //                    builder.Append(");");
                            //                }
                            //                if (hasSet)
                            //                {
                            //                    if (hasGet)
                            //                        builder.Append("else {");
                            //                    builder.Append("obj.{1}{0}(", member.Name.Name, SET);
                            //                    var parameters = accessors.First(a => a.IsSetAccessor).Parameters;
                            //                    for (int i = 0, n = parameters.Count - 1; i <= n; i++)
                            //                    {
                            //                        builder.Append("args[{0}]", i);
                            //                        if (i != n)
                            //                            builder.Append(", ");
                            //                    }
                            //                    builder.Append("); return null;");
                            //                    if (hasGet)
                            //                        builder.Append("}");
                            //                }
                            //                builder.AppendLine();
                            //            });
                            //    });
                            //}
                            //if (type.Members.Any(m => m.IsMethod && HasReference(m)))
                            //{
                            //    builder.AppendLine("if (member.IsMethod)");
                            //    builder.AppendBlock(() =>
                            //    {
                            //        BuildInvokeReflexibleMember(type, (m) => m.IsMethod && m.TypeParameters.Count == 0,
                            //            (member) =>
                            //            {
                            //                var parameters = member.Parameters;
                            //                if (!CSharpType.IsVoid(member.ReturnType))
                            //                    builder.Append("return ");
                            //                if (member.IsStatic)
                            //                    builder.Append("{1}.{0}", member.Name.Name, typeNameInvoke);
                            //                else
                            //                    builder.Append("obj.{0}", member.Name.Name);
                            //                builder.Append('(');
                            //                for (int i = 0, n = parameters.Count - 1; i <= n; i++)
                            //                {
                            //                    builder.Append("args[{0}]", i);
                            //                    if (i != n)
                            //                        builder.Append(", ");
                            //                }
                            //                builder.Append(");");
                            //                if (CSharpType.IsVoid(member.ReturnType))
                            //                    builder.AppendLine("return null;");
                            //                else
                            //                    builder.AppendLine();
                            //            });

                            //        // todo:泛型方法尚未处理
                            //        // todo:ref,out,params的方法调用尚未处理
                            //        builder.AppendLine("throw -2;");
                            //    });
                            //}
                            // todo:Event尚未处理
                        }// end of foreach reflexibleTypes
                    });// end of switch(typename)
                    builder.AppendLine("throw -1;");
                });// end of while(true)
            });// end of _R.Invoke
            #endregion
            #endregion

            // default: 数字为0，结构体需要使用默认构造函数创建对象，对象类型则为null
            // is: 无论变量类型，值只要为null，is都会返回false
            // typeof: 若未能找到Type，应针对类型生成一个仅带有Name的SimpleType

            beginWriter = builder;
            builder = new StringBuilder();
            swapWriter = builder;

            foreach (var item in DefineTypes)
                Visit(item);
        }
        internal override void WriteEnd(IEnumerable<DefineFile> files)
        {
            // BUG: 泛型类型的构造函数暂未解决，也就是没法反射构建List<T>
            endWriter.AppendLine("Activator.CreateDefault = function(t)");
            endWriter.AppendBlockWithEnd(() =>
            {
                endWriter.AppendLine("var name = CSharpType.GetRuntimeTypeName(t.type);");
                endWriter.AppendLine("var $ctor = _R.$TC[name];");
                endWriter.AppendLine("return new $ctor();");
            });

            builder.AppendLine("MulticastDelegate.prototype.Invoke = function()");
            builder.AppendBlockWithEnd(() =>
            {
                builder.AppendLine("var args2 = Array.prototype.slice.apply(arguments).slice(0);");
                builder.AppendLine("function invoke(func) { func.Invoke.apply(func, args2); }");
                builder.AppendLine("this.InvokeAll(invoke);");
            });
        }
        private static Queue<string> renamedQueue = new Queue<string>();
        private string GetRenamedRuntimeTypeName(CSharpType type)
        {
            renamedQueue.Clear();
            CSharpType typeDefinition = CSharpType.GetDefinitionType(type);
            CSharpType tempType = typeDefinition;
            while (tempType != null)
            {
                renamedQueue.Enqueue(tempType.Name.Name);
                tempType.Name.Name = GetRenamedTypeOriginName(tempType);
                tempType = tempType.ContainingType;
            }
            string result = CSharpType.GetRuntimeTypeName(type);
            tempType = typeDefinition;
            while (tempType != null)
            {
                tempType.Name.Name = renamedQueue.Dequeue();
                tempType = tempType.ContainingType;
            }
            return result;
        }
        private string GetRenamedTypeOriginName(CSharpType type)
        {
            string name;
            if (!renamedTypes.TryGetValue(type, out name))
            {
                string typeName;
                if (type.IsArray)
                {
                    do
                    {
                        typeName = GetRenamedTypeOriginName(type.ElementType);
                        //typeName += "[]";
                        type = type.ElementType;
                    } while (type.IsArray);
                }
                else
                    typeName = type.Name.Name;
                switch (typeName)
                {
                    case "Object": name = "System.Object"; break;
                    case "bool": name = "System.Boolean"; break;
                    case "sbyte": name = "System.SByte"; break;
                    case "byte": name = "System.Byte"; break;
                    case "short": name = "System.Int16"; break;
                    case "ushort": name = "System.UInt16"; break;
                    case "char": name = "System.Char"; break;
                    case "int": name = "System.Int32"; break;
                    case "uint": name = "System.UInt32"; break;
                    case "float": name = "System.Single"; break;
                    case "long": name = "System.Int64"; break;
                    case "ulong": name = "System.UInt64"; break;
                    case "double": name = "System.Double"; break;
                    case "String": if (type == CSharpType.STRING) name = "System.String"; else name = type.Name.Name; break;
                    default: name = type.Name.Name; break;
                }
            }
            return name;
        }
        private string GetRenamedMemberOriginName(CSharpMember member)
        {
            string name;
            if (!renamedMembers.TryGetValue(member, out name))
                name = member.Name.Name;
            return name;
        }
        private void BuildInvokeReflexibleMember(CSharpType type, Func<CSharpMember, bool> filter, Action<CSharpMember> invoke)
        {
            string memberName;
            if (!type.Members.Any(m => filter(m) && ReflexibleMember(m, type, out memberName) != EReflexibleMember.None))
                return;

            builder.AppendLine("switch (member.{0}Name().Name)", GET);
            builder.AppendBlock(() =>
            {
                foreach (var member in type.Members.Where(m => filter(m)))
                {
                    // BUG: 貌似memberName不正确
                    var status = ReflexibleMember(member, type, out memberName);
                    if (status == EReflexibleMember.None)
                        continue;
                    if (status == EReflexibleMember.OK)
                    {
                        builder.Append("case \"{0}\":", memberName);
                        invoke(member);
                    }
                    else if (status == EReflexibleMember.Base)
                    {
                        // 生成跳转到父类反射成员的部分
                        if (!reflexibleTypes.Contains(member.ContainingType))
                            continue;
                        builder.Append("case \"{0}\":", memberName);
                        string typeName;
                        if (!renamedTypes.TryGetValue(member.ContainingType, out typeName))
                            typeName = type.Name.Name;
                        builder.AppendLine("typename = \"{0}\"; continue;", typeName);
                    }
                }

            });
        }
        private EReflexibleMember ReflexibleMember(CSharpMember member, CSharpType parent, out string oname)
        {
            oname = null;
            if (!HasReference(member))
                return EReflexibleMember.None;
            //if (!renamedMembers.TryGetValue(member, out oname) || (member.IsProperty || member.IsMethod || member.IsConstructor || member.IsOperator))
            if (!renamedMembers.TryGetValue(member, out oname) || (member.IsIndexer || member.IsMethod || member.IsConstructor || member.IsOperator))
                oname = member.Name.Name;

            if (member.ContainingType == parent)
                return EReflexibleMember.OK;
            else
                return EReflexibleMember.Base;
        }
        private enum EReflexibleMember
        {
            /// <summary>不能反射此成员</summary>
            None,
            /// <summary>可以反射此成员</summary>
            OK,
            /// <summary>此成员为父类型的成员</summary>
            Base,
        }
    }
    public enum ECodeLanguage
    {
        CSharp,
        JavaScript,
    }
    public static class Refactor
    {
        internal static HashSet<CSharpAssembly> assemblies = new HashSet<CSharpAssembly>();
        public static IList<CSharpAssembly> Assemblies
        {
            get { return assemblies.ToArray(); }
        }

        public static void AddAssembly(CSharpAssembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");
            assemblies.Add(assembly);
        }

        public static CSharpAssembly LoadFromAssembly(Assembly assembly)
        {
            CSharpAssembly csa;
            if (_Reflection.AssemblyMaps.TryGetValue(assembly, out csa))
                return csa;
            else
            {
                csa = new BinaryAssembly();
                _Reflection.AssemblyMaps.Add(assembly, csa);
            }
            csa.Name = assembly.FullName;
            Type[] types = assembly.GetExportedTypes();
            foreach (var item in types)
                LoadFromType(item);
            return csa;
        }
        internal static CSharpType LoadFromType(Type type)
        {
            // 考虑Assembly
            CSharpAssembly assembly = LoadFromAssembly(type.Assembly);

            if (type.IsArray)
            {
                var elementType = LoadFromType(type.GetElementType());
                return new ArrayTypeReference(elementType, 1);
            }

            CSharpType ct;
            if (_Reflection.TypeMaps.TryGetValue(type, out ct))
                return ct;

            bool result = type != null && type.IsPublic && !type.Name.StartsWith("<>");
            if (result)
            {
                if (type.IsGenericType && !type.IsGenericTypeDefinition)
                {
                    var genericType = LoadFromType(type.GetGenericTypeDefinition());
                    return CSharpType.CreateConstructedType(genericType, genericType.ContainingType, type.GetGenericArguments().Select(g => LoadFromType(g)).ToList());
                }
            }
            else
                return null;

            TypeDefinitionInfo cst = new TypeDefinitionInfo();
            assembly.Add(cst);
            _Reflection.TypeMaps.Add(type, cst);
            string name = type.Name;
            if (name.StartsWith("@"))
                name = name.Substring(1);
            int genericNameIndex = name.IndexOf('`');
            if (genericNameIndex != -1)
                name = name.Substring(0, genericNameIndex);
            // 关键字不需要命名空间
            string __namespace = string.Empty;
            if (name == "Boolean")
                name = "bool";
            else if (name == "SByte")
                name = "sbyte";
            else if (name == "Byte")
                name = "byte";
            else if (name == "Int16")
                name = "short";
            else if (name == "UInt16")
                name = "ushort";
            else if (name == "Char")
                name = "char";
            else if (name == "Int32")
                name = "int";
            else if (name == "UInt32")
                name = "uint";
            else if (name == "Single")
                name = "float";
            else if (name == "Int64")
                name = "long";
            else if (name == "UInt64")
                name = "ulong";
            else if (name == "Double")
                name = "double";
            else if (name == "Decimal")
                name = "decimal";
            else if (name == "Object")
                name = "object";
            else if (name == "String")
                name = "string";
            else
                __namespace = type.Namespace;
            cst._Name = new Named(name);

            if (type.IsInterface)
                cst._TypeAttributes |= TypeAttributes.Interface;
            else if (type.IsEnum)
            {
                cst._TypeAttributes |= TypeAttributes.Enum;
                cst._UnderlyingType = LoadFromType(Enum.GetUnderlyingType(type));
            }
            else if (type.IsValueType)
                cst._TypeAttributes |= TypeAttributes.ValueType;
            else if (type.IsClass)
                if (type.Is(typeof(Delegate)))
                    cst._TypeAttributes |= TypeAttributes.Delegate;
                else
                    cst._TypeAttributes |= TypeAttributes.Class;

            if (type.IsPublic)
                cst._TypeAttributes |= TypeAttributes.Public;
            if (type.IsNestedPublic)
                cst._TypeAttributes |= TypeAttributes.NestedPublic;
            if (!cst.IsPublic)
                cst._TypeAttributes |= TypeAttributes.Internal;
            if (type.IsNestedAssembly)
                cst._TypeAttributes |= TypeAttributes.NestedInternal;
            if (type.IsNestedFamORAssem)
                cst._TypeAttributes |= TypeAttributes.NestedProtectedOrInternal;
            if (type.IsNestedFamily)
                cst._TypeAttributes |= TypeAttributes.NestedProtected;
            if (type.IsNestedPrivate)
                cst._TypeAttributes |= TypeAttributes.NestedPrivate;
            if (cst.IsSealed)
                cst._TypeAttributes |= TypeAttributes.Sealed;
            if (type.IsAbstract)
                cst._TypeAttributes |= TypeAttributes.Abstract;
            if (type.IsSerializable)
                cst._TypeAttributes |= TypeAttributes.Serializable;

            string[] namespaces = __namespace.Split('.');
            IEnumerable<CSharpNamespace> find = assembly.Namespaces;
            foreach (var item in namespaces)
            {
                var temp = find == null ? null : find.FirstOrDefault(n => n.Name == item);
                if (temp == null)
                {
                    temp = new CSharpNamespace();
                    temp.Name = item;
                }
                if (cst._ContainingNamespace != null)
                    cst._ContainingNamespace.AddNamespace(temp);
                cst._ContainingNamespace = temp;
                find = cst._ContainingNamespace.Namespaces;
            }
            cst._ContainingNamespace.AddType(cst);

            // _ContainingType
            if (type.DeclaringType != null)
                cst._ContainingType = LoadFromType(type.DeclaringType);
            // _NestedType
            Type[] nestedTypes = type.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public);
            //Type[] nestedTypes = type.GetNestedTypes();
            foreach (var item in nestedTypes)
            {
                // 过滤private|internal类
                if (item.IsNestedPrivate || item.IsNestedAssembly)
                    continue;
                var temp = LoadFromType(item);
                if (temp != null)
                    cst.NestedTypes.Add(temp);
            }
            // _TypeParameters
            if (type.IsGenericTypeDefinition)
            {
                var generics = type.GetGenericArguments();
                for (int i = 0; i < generics.Length; i++)
                {
                    var generic = generics[i];
                    TypeParameterData tpd = new TypeParameterData();
                    tpd.Name = new Named(generic.Name);
                    tpd.Variance = (EVariance)((generic.GenericParameterAttributes & GenericParameterAttributes.Covariant) | (generic.GenericParameterAttributes & GenericParameterAttributes.Contravariant));
                    tpd.HasConstructorConstraint = (generic.GenericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) != GenericParameterAttributes.None;
                    tpd.HasValueTypeConstraint = (generic.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != GenericParameterAttributes.None;
                    tpd.HasReferenceTypeConstraint = (generic.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != GenericParameterAttributes.None;
                    cst._TypeParameters.Add(tpd);
                    _Reflection.TypeMaps.Add(generic, cst.TypeParameters[i]);
                    var constraints = generic.GetGenericParameterConstraints();
                    foreach (var item in constraints)
                    {
                        var temp = LoadFromType(item);
                        if (temp != null)
                            tpd.AddConstraint(temp);
                    }
                }
            }
            // _BaseTypes
            if (type.BaseType != null)
            {
                var temp = LoadFromType(type.BaseType);
                if (temp != null)
                    cst._BaseTypes.Add(temp);
            }
            foreach (var item in type.GetInterfaces())
            {
                var temp = LoadFromType(item);
                if (temp != null)
                    cst._BaseTypes.Add(temp);
            }
            // Attributes
            object[] attributes = type.GetCustomAttributes(true);
            foreach (var item in attributes)
            {
                var temp = LoadFromType(item.GetType());
                if (temp != null)
                    cst._Attributes.Add(temp, null);
            }
            // Members
            BindingFlags flag = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            // Fields
            var fields = type.GetFields(flag);
            int memberIndex = 0;
            foreach (var item in fields)
            {
                if (item.IsPrivate || item.IsAssembly)
                    continue;
                var _type = LoadFromType(item.FieldType);
                if (_type == null)
                    continue;
                MemberDefinitionInfo member = cst.CreateMemberDefinition(memberIndex++);
                member._ReturnType = _type;
                member._Name = new Named(item.Name);
                if (type.IsEnum)
                {
                    member._Kind = MemberDefinitionKind.EnumMember;
                }
                else
                {
                    if (item.IsLiteral)
                    {
                        member._Kind = MemberDefinitionKind.Constant;
                        member._ConstantValue = item.GetRawConstantValue();
                    }
                    else
                        member._Kind = MemberDefinitionKind.Field;
                }
                if (item.IsPublic)
                    member._MemberAttributes |= MemberAttributes.Public;
                else if (item.IsFamilyOrAssembly)
                    member._MemberAttributes |= MemberAttributes.ProtectedOrInternal;
                else if (item.IsFamily)
                    member._MemberAttributes |= MemberAttributes.Protected;
                else if (item.IsAssembly)
                    member._MemberAttributes |= MemberAttributes.Internal;
                else
                    member._MemberAttributes |= MemberAttributes.Private;
                if (item.IsInitOnly)
                    member._MemberAttributes |= MemberAttributes.ReadOnly;
                if (item.IsStatic)
                    member._MemberAttributes |= MemberAttributes.Static;
            }
            var properties = type.GetProperties(flag);
            foreach (var item in properties)
            {
                var _type = LoadFromType(item.PropertyType);
                if (_type == null)
                    continue;
                MemberDefinitionInfo member = cst.CreateMemberDefinition(memberIndex++);
                member._ReturnType = _type;
                member._Name = new Named(item.Name);
                var parameters = item.GetIndexParameters();
                if (parameters.Length == 0)
                    member._Kind = MemberDefinitionKind.Property;
                else
                    member._Kind = MemberDefinitionKind.Indexer;
                MemberAccessor accessor = null;
                if (item.CanRead)
                {
                    var get = item.GetGetMethod(true);
                    MemberDefinitionInfo method = new MemberDefinitionInfo(cst, memberIndex);
                    SetMethodAttributes(method, get);
                    accessor = new MemberAccessor(member, true);
                }
                if (item.CanWrite)
                {
                    var set = item.GetSetMethod(true);
                    MemberDefinitionInfo method = new MemberDefinitionInfo(cst, memberIndex);
                    SetMethodAttributes(method, set);
                    var setAccessor = new MemberAccessor(member, false);
                    if (accessor != null)
                        if ((accessor._MemberAttributes & ~MemberAttributes.VisibilityMask) > (method._MemberAttributes & ~MemberAttributes.VisibilityMask))
                            accessor = setAccessor;
                        else
                            // get和set可见性一样，清空set访问器的可见性
                            setAccessor._MemberAttributes = MemberAttributes.None;
                    else
                        accessor = setAccessor;
                }
                member._MemberAttributes = accessor._MemberAttributes;
                accessor._MemberAttributes = MemberAttributes.None;
                if (item.CanRead)
                    member._MemberAttributes |= MemberAttributes.PropertyGetter;
                if (item.CanWrite)
                    member._MemberAttributes |= MemberAttributes.PropertySetter;
                // 参数 None | PARAMS
                if (parameters.Length > 0)
                {
                    member._Parameters = new MemberDefinitionInfo.ParameterData[parameters.Length];
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var param = parameters[i];
                        MemberDefinitionInfo.ParameterData data = new MemberDefinitionInfo.ParameterData();
                        data.Name = new Named(param.Name);
                        data.Type = LoadFromType(param.ParameterType);
                        if (param.IsOptional)
                            data.DefaultValueString = param.DefaultValue == null ? null : param.DefaultValue.ToString();
                        if (param.HasAttribute<ParamArrayAttribute>())
                            data.Direction = EDirection.PARAMS;
                        member._Parameters[i] = data;
                    }
                }
            }
            var methods = type.GetMethods(flag);
            foreach (var item in methods)
            {
                if (item.IsPrivate || item.IsAssembly)
                    continue;
                if (item.IsSpecialName)
                {
                    // 过滤掉属性方法
                    if (item.Name.StartsWith("get_")
                        || item.Name.StartsWith("set_")
                        || item.Name.StartsWith("add_")
                        || item.Name.StartsWith("remove_"))
                    {
                        continue;
                    }
                }
                var _type = LoadFromType(item.ReturnType);
                if (_type == null)
                    continue;
                MemberDefinitionInfo member = cst.CreateMemberDefinition(memberIndex++);
                SetMethodAttributes(member, item);
                member._ReturnType = _type;
                member._Name = new Named(item.Name);
                if (item.IsSpecialName && item.IsPublic && item.IsStatic && item.IsHideBySig && item.Name.StartsWith("op_"))
                    member._Kind = MemberDefinitionKind.Operator;
                else
                    member._Kind = MemberDefinitionKind.Method;
            }
            var constructors = type.GetConstructors(flag);
            foreach (var item in constructors)
            {
                if (item.IsPrivate || item.IsAssembly)
                    continue;
                MemberDefinitionInfo member = cst.CreateMemberDefinition(memberIndex++);
                SetMethodAttributes(member, item);
                member._Name = new Named(type.Name);
                member._Kind = MemberDefinitionKind.Constructor;
            }
            var events = type.GetEvents(flag);
            foreach (var item in events)
            {
                var _type = LoadFromType(item.EventHandlerType);
                if (_type == null)
                    continue;
                MemberDefinitionInfo member = new MemberDefinitionInfo(cst, memberIndex);
                member._ReturnType = _type;
                member._Name = new Named(item.Name);
                member._Kind = MemberDefinitionKind.Event;
                MemberAccessor accessor = null;
                var accm = item.GetAddMethod(true);
                MemberDefinitionInfo method = new MemberDefinitionInfo(cst, memberIndex);
                SetMethodAttributes(method, accm);
                accessor = new MemberAccessor(member, true);
                member._MemberAttributes = accessor._MemberAttributes;
                member._MemberAttributes |= MemberAttributes.PropertyGetter;
                member._MemberAttributes |= MemberAttributes.PropertySetter;
                accessor = new MemberAccessor(member, false);
            }

            return cst;
        }
        private static void SetMethodAttributes(MemberDefinitionInfo member, MethodBase method)
        {
            if (method.IsPublic)
                member._MemberAttributes |= MemberAttributes.Public;
            else if (method.IsFamilyOrAssembly)
                member._MemberAttributes |= MemberAttributes.ProtectedOrInternal;
            else if (method.IsFamily)
                member._MemberAttributes |= MemberAttributes.Protected;
            else if (method.IsAssembly)
                member._MemberAttributes |= MemberAttributes.Internal;
            else
                member._MemberAttributes |= MemberAttributes.Private;
            if (method.IsVirtual)
                member._MemberAttributes |= MemberAttributes.Abstract;
            else if (method.IsAbstract)
                member._MemberAttributes |= MemberAttributes.Abstract;
            if (method.IsFinal)
                member._MemberAttributes |= MemberAttributes.Sealed;
            if (method.IsStatic)
                member._MemberAttributes |= MemberAttributes.Static;

            // 泛型
            if (method.IsGenericMethodDefinition)
            {
                Type[] types = method.GetGenericArguments();
                member._TypeParameters = new TypeParameterData[types.Length];
                for (int i = 0; i < types.Length; i++)
                {
                    var generic = types[i];
                    TypeParameterData tpd = new TypeParameterData();
                    tpd.Name = new Named(generic.Name);
                    tpd.Variance = (EVariance)((generic.GenericParameterAttributes & GenericParameterAttributes.Covariant) | (generic.GenericParameterAttributes & GenericParameterAttributes.Contravariant));
                    tpd.HasConstructorConstraint = (generic.GenericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) != GenericParameterAttributes.None;
                    tpd.HasValueTypeConstraint = (generic.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != GenericParameterAttributes.None;
                    tpd.HasReferenceTypeConstraint = (generic.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != GenericParameterAttributes.None;
                    member._TypeParameters[i] = tpd;
                    _Reflection.TypeMaps[generic] = member.TypeParameters[i];
                    var constraints = generic.GetGenericParameterConstraints();
                    foreach (var item in constraints)
                    {
                        var temp = LoadFromType(item);
                        if (temp != null)
                            tpd.AddConstraint(temp);
                    }
                }
            }
            // 参数
            var parameters = method.GetParameters();
            if (parameters.Length > 0)
            {
                member._Parameters = new MemberDefinitionInfo.ParameterData[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    var param = parameters[i];
                    MemberDefinitionInfo.ParameterData data = new MemberDefinitionInfo.ParameterData();
                    data.Name = new Named(param.Name);
                    data.Type = LoadFromType(param.ParameterType);
                    if (param.IsOptional)
                        data.DefaultValueString = param.DefaultValue == null ? null : param.DefaultValue.ToString();
                    if (param.IsOut)
                        data.Direction = EDirection.OUT;
                    else if (param.HasAttribute<ParamArrayAttribute>())
                        data.Direction = EDirection.PARAMS;
                    else if (param.ParameterType.IsByRef)
                        data.Direction = EDirection.REF;
                    // 没有代码判断不到this
                    member._Parameters[i] = data;
                }
            }
        }

        public static void Resolve(params Project[] projects)
        {
            for (int i = 0; i < projects.Length; i++)
                Resolve(projects[i], true);
        }
        public static CSharpAssembly Resolve(Project project, bool isProject)
        {
            CSharpAssembly assembly;
            if (isProject)
                assembly = new ProjectAssembly();
            else
                assembly = new BinaryAssembly();
            assembly.Name = project.Title;
            assemblies.Add(assembly);

            _BuildTypeSystem build = null;
            build = BuildTypeSystem<_BuildNamespace>(assembly, build, project);
            build = BuildTypeSystem<_BuildType>(assembly, build, project);
            build = BuildTypeSystem<_BuildMember>(assembly, build, project);
            build = BuildTypeSystem<_BuildReference>(assembly, build, project);

            // 解析完成后如果有入口方法，默认添加入口方法的引用
            if (isProject)
            {
                var refs = _BuildReference.objectReferences;
                HashSet<object> rootRefs = new HashSet<object>();
                // 寻找Main方法作为根引用
                foreach (var type in assembly.Types)
                {
                    if (type.IsClass)
                    {
                        var entryPoint = type.Members.FirstOrDefault(m => m.IsMethod && m.IsStatic && m.Name.Name == "Main");
                        if (entryPoint != null)
                        {
                            rootRefs.Add(type);
                            rootRefs.Add(entryPoint);
                            break;
                        }
                    }
                }
                if (rootRefs.Count > 0)
                    // 添加上引用以免在生成代码时被忽略
                    foreach (var item in rootRefs)
                        if (!refs.ContainsKey(item))
                            refs.Add(item, new BEREF() { Define = item });
            }

            return assembly;
        }
        /// <param name="rootRefObj">被根对象(类型会成员)引用则视为有引用，默认会找到Main方法视为根引用</param>
        public static void Optimize(params object[] rootRefObj)
        {
            CSharpAssembly[] targets = assemblies.Where(a => a is ProjectAssembly).ToArray();
            var refs = _BuildReference.objectReferences;

            HashSet<object> rootRefs = new HashSet<object>(rootRefObj);
            // 寻找Main方法作为根引用
            foreach (var assembly in targets)
            {
                foreach (var type in assembly.Types)
                {
                    if (type.IsClass)
                    {
                        var entryPoint = type.Members.FirstOrDefault(m => m.IsMethod && m.IsStatic && m.Name.Name == "Main");
                        if (entryPoint != null)
                        {
                            rootRefs.Add(type);
                            rootRefs.Add(entryPoint);
                            break;
                        }
                    }
                }
            }
            if (rootRefs.Count == 0)
                throw new InvalidOperationException("优化必须拥有至少一个根引用对象");
            // 添加上引用以免在生成代码时被忽略
            //foreach (var item in rootRefs)
            //    if (!refs.ContainsKey(item))
            //        refs.Add(item, new BEREF() { Define = item });

            // 正在优化递归中的对象，防止递归中再次递归
            HashSet<object> recursion = new HashSet<object>();
            Func<object, sbyte> HasBeReferenced = null;
            BEREF _ref;
            HasBeReferenced = (obj) =>
            {
                if (rootRefs.Contains(obj))
                    return 1;

                if (!recursion.Add(obj))
                    return 0;

                CSharpType type = obj as CSharpType;
                if (type != null)
                {
                    if (type.Attributes.Any(a => a.Type.Name.Name == ANonOptimize.Name))
                    {
                        rootRefs.Add(type);
                        return 1;
                    }
                    // 可序列化的类型成员也全都不能优化掉
                    else if (type.Attributes.Any(a => a.Type.Name.Name == AReflexible.Name || a.Type.Name.Name == "Serializable"))
                    {
                        rootRefs.Add(type);
                        foreach (var item in type.MemberDefinitions)
                        {
                            rootRefs.Add(item);
                        }
                        return 1;
                    }
                }
                else
                {
                    CSharpMember member = obj as CSharpMember;
                    if (member.Attributes.Any(a => a.Type.Name.Name == ANonOptimize.Name))
                    {
                        rootRefs.Add(member);
                        return 1;
                    }
                    // 可序列化的类型成员也全都不能优化掉
                    else if (member.ContainingType.Attributes.Any(a => a.Type.Name.Name == AReflexible.Name || a.Type.Name.Name == "Serializable"))
                    {
                        rootRefs.Add(member.ContainingType);
                        foreach (var item in member.ContainingType.MemberDefinitions)
                        {
                            rootRefs.Add(item);
                        }
                        return 1;
                    }
                    // 静态构造函数只要类型被引用就算被引用
                    if (member.IsConstructor && member.IsStatic)
                    {
                        return HasBeReferenced(CSharpType.GetDefinitionType(member.ContainingType));
                    }
                }

                BEREF beref;
                if (refs.TryGetValue(obj, out beref))
                {
                    foreach (var item in beref.References)
                    {
                        // 已经被删除的引用
                        if (item.Referencer.Define == null)
                            continue;

                        sbyte result = HasBeReferenced(item.Referencer.Define);
                        if (result == 1)
                        {
                            // 类型是否真的被引用，还需确认引用自己的类型是否真的被引用
                            if (type != null)
                            {
                                CSharpType type2 = item.Referencer.Define as CSharpType;
                                if (type2 != null)
                                {
                                    if (!type2.Equals(type) && HasBeReferenced(CSharpType.GetDefinitionType(type2)) == 1)
                                    {
                                        rootRefs.Add(obj);
                                        return 1;
                                    }
                                }
                                else
                                {
                                    CSharpMember member2 = item.Referencer.Define as CSharpMember;
                                    if (!member2.ContainingType.Equals(type) && HasBeReferenced(CSharpType.GetDefinitionType(member2.ContainingType)) == 1)
                                    {
                                        rootRefs.Add(obj);
                                        return 1;
                                    }
                                }
                            }
                            else
                            {
                                rootRefs.Add(obj);
                                return 1;
                            }
                        }
                    }
                }
                return -1;
            };
            
            List<CSharpType> optimizedTypes = new List<CSharpType>();
            List<CSharpMember> optimizedMembers = new List<CSharpMember>();

            Action<CSharpType> OptimizeType = null;
            OptimizeType = (type) =>
            {
                if (HasBeReferenced(type) == 1)
                {
                    rootRefs.Add(type);
                }
                else
                {
                    if (refs.TryGetValue(type, out _ref)) _ref.Define = null;
                    optimizedTypes.Add(type);
                    foreach (var member in type.MemberDefinitions)
                    {
                        optimizedMembers.Add(member);
                        if (refs.TryGetValue(member, out _ref)) _ref.Define = null;
                    }
                    foreach (var nested in type.NestedTypes)
                    {
                        // 父类里的内部类必须由父类来清理
                        if (nested.ContainingType != type)
                            continue;
                        if (refs.TryGetValue(nested, out _ref)) _ref.Define = null;
                        optimizedTypes.Add(nested);
                    }
                    recursion.Clear();
                    return;
                }
                recursion.Clear();

                foreach (var member in type.MemberDefinitions)
                {
                    if (HasBeReferenced(member) == 1)
                    {
                        rootRefs.Add(member);
                    }
                    else
                    {
                        if (refs.TryGetValue(member, out _ref)) _ref.Define = null;
                        optimizedMembers.Add(member);
                    }
                    recursion.Clear();
                }
                
                foreach (var nested in type.NestedTypes)
                {
                    // 防止内部类实现外部类的死循环
                    if (nested == type)
                        continue;
                    OptimizeType(nested);
                }
            };

            // 循环所有类型及其所有成员，找出无引用的对象
            foreach (var assembly in targets)
                foreach (var type in assembly.Types)
                    OptimizeType(type);

            //StringBuilder builder = new StringBuilder();
            //foreach (var item in optimizedTypes)
            //    builder.AppendLine(item.ToString());
            //builder.AppendLine();
            //foreach (var item in optimizedMembers)
            //    builder.AppendLine(item.ToString());
            //File.WriteAllText("optimized.txt", builder.ToString());
        }
        public static string RebuildCode(ECodeLanguage language, params DefineFile[] files)
        {
            Rewriter rewriter;
            switch (language)
            {
                case ECodeLanguage.CSharp:
                    rewriter = new Rewriter();
                    break;
                case ECodeLanguage.JavaScript:
                    rewriter = new JSRewriter();
                    break;
                default: throw new NotImplementedException();
            }

            var defineFiles = files.Where(f => CodeFileHelper.ParseFileType(f.Name) != EFileType.Define);
            rewriter.WriteBegin(defineFiles);
            foreach (var file in defineFiles)
                rewriter.Visit(file);
            rewriter.WriteEnd(defineFiles);
            return rewriter.Result;
        }
        private static void BuildTypeSystem(_BuildTypeSystem target, CSharpAssembly assembly, _BuildTypeSystem previous, Project project)
        {
            target.assembly = assembly;
            foreach (var item in project.Files)
                target.Visit(item);
        }
        private static T BuildTypeSystem<T>(CSharpAssembly assembly, _BuildTypeSystem previous, Project project) where T : _BuildTypeSystem, new()
        {
            _LOG.Info("{0}解析中", typeof(T).Name);
            T target = new T();
            BuildTypeSystem(target, assembly, previous, project);
            return target;
        }
        public static void UnloadDomain()
        {
            assemblies.Clear();
            _BuildReference.members.Clear();
            _BuildReference.types.Clear();
            _BuildReference.objectReferences.Clear();
            _BuildReference.syntaxReferences.Clear();
        }

        public static bool IsNumberType(CSharpType t)
        {
            return t != null &&
                (t == CSharpType.SBYTE
                || t == CSharpType.BYTE
                || t == CSharpType.CHAR
                || t == CSharpType.SHORT
                || t == CSharpType.USHORT
                || t == CSharpType.INT
                || t == CSharpType.UINT
                || t == CSharpType.FLOAT
                || t == CSharpType.DOUBLE
                || t == CSharpType.LONG
                || t == CSharpType.ULONG
                || t.IsEnum);
        }
        internal static int GetPrimitiveTypeSize(CSharpType t)
        {
            if (t == null) return -1;
            else if (t == CSharpType.BOOL) return 1;
            else if (t == CSharpType.SBYTE) return 1;
            else if (t == CSharpType.BYTE) return 1;
            else if (t == CSharpType.CHAR) return 2;
            else if (t == CSharpType.SHORT) return 2;
            else if (t == CSharpType.USHORT) return 2;
            else if (t == CSharpType.INT) return 4;
            else if (t == CSharpType.UINT) return 4;
            else if (t == CSharpType.FLOAT) return 4;
            else if (t == CSharpType.DOUBLE) return 8;
            else if (t == CSharpType.LONG) return 8;
            else if (t == CSharpType.ULONG) return 8;
            else if (t.IsEnum) return GetPrimitiveTypeSize(t.UnderlyingType);
            else return -1;
        }
        internal static bool GetNumberType(CSharpType t, out CSharpType resultType)
        {
            if (t != null)
            {
                if (t.IsEnum)
                {
                    resultType = t.UnderlyingType;
                    return true;
                }
                else
                {
                    if (t == CSharpType.BOOL
                        || t == CSharpType.SBYTE
                        || t == CSharpType.BYTE

                        || t == CSharpType.SHORT
                        || t == CSharpType.USHORT
                        || t == CSharpType.INT
                        || t == CSharpType.UINT
                        || t == CSharpType.FLOAT
                        || t == CSharpType.DOUBLE
                        || t == CSharpType.LONG
                        || t == CSharpType.ULONG)
                    {
                        resultType = t;
                        return true;
                    }
                    else if (t == CSharpType.CHAR)
                    {
                        resultType = CSharpType.USHORT;
                        return true;
                    }
                }
            }
            resultType = null;
            return false;
        }
    }
    internal enum EFileType
    {
        /// <summary>[.cs]:普通的代码文件，参与语义分析，生成代码</summary>
        Normal,
        /// <summary>[.d.cs]:类型和成员的定义，参与语义分析，不生成代码</summary>
        Define,
    }
    internal static class CodeFileHelper
    {
        public static EFileType ParseFileType(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                if (fileName.EndsWith(".d.cs"))
                {
                    return EFileType.Define;
                }
            }
            return EFileType.Normal;
        }
    }

    // 语义解析，引用分析
    public class REF
    {
        public SyntaxNode RefSyntax { get; internal set; }
        /// <summary>CSharpType | CSharpMember</summary>
        public BEREF Referencer { get; internal set; }
        /// <summary>为null时，可能为调用重载的运算符</summary>
        public Named Reference { get; internal set; }
        /// <summary>CSharpType | CSharpMember | CSharpParameterInfo</summary>
        public BEREF Definition { get; internal set; }
        public override string ToString()
        {
            return string.Format("{0} invoke by {1}", Definition.ToString(), Referencer);
        }
    }
    public class BEREF
    {
        public List<REF> References { get; private set; }
        /// <summary>null表示此引用已被删除</summary>
        public object Define { get; internal set; }
        internal BEREF()
        {
            References = new List<REF>();
        }
        public override string ToString()
        {
            return Define == null ? "null" : Define.ToString();
        }
    }
    public class VAR
    {
        public CSharpMember DeclaringMember { get; internal set; }
        public CSharpType Type { get; internal set; }
        public Named Name { get; internal set; }
        public override string ToString()
        {
            return string.Format("{0} {1}", Type.ToString(), Name);
        }
    }
    internal abstract class _BuildTypeSystem : SyntaxWalker
    {
        internal static Named Nullable = new Named("Nullable");
        //internal static Named DELEGATE = new Named("Delegate");
        //internal static Named ARRAY = new Named("Array");
        //internal static Named ENUM = new Named("Enum");
        //internal static Named TYPE = new Named("System.Type");
        internal static ReferenceMember TYPE = new ReferenceMember(new Named("Type")) { Target = new ReferenceMember(new Named("System")) };

        internal static Dictionary<EUnaryOperator, Named> UnaryOperator = new Dictionary<EUnaryOperator, Named>();
        internal static Dictionary<EBinaryOperator, Named> BinaryOperator = new Dictionary<EBinaryOperator, Named>();
        internal static Named IMPLICIT = new Named("op_Implicit");
        internal static Named EXPLICIT = new Named("op_Explicit");
        internal static Named INVOKE = new Named("Invoke");
        internal static Named VALUE = new Named("value");

        static _BuildTypeSystem()
        {
            UnaryOperator[EUnaryOperator.Not] = new Named("op_LogicalNot");
            UnaryOperator[EUnaryOperator.BitNot] = new Named("op_OnesComplement");
            UnaryOperator[EUnaryOperator.Minus] = new Named("op_Negation");
            UnaryOperator[EUnaryOperator.Plus] = new Named("op_UnaryPlus");
            UnaryOperator[EUnaryOperator.Increment] = new Named("op_Increment");
            UnaryOperator[EUnaryOperator.Decrement] = new Named("op_Decrement");
            UnaryOperator[EUnaryOperator.PostIncrement] = new Named("op_Increment");
            UnaryOperator[EUnaryOperator.PostDecrement] = new Named("op_Decrement");

            BinaryOperator[EBinaryOperator.BitwiseAnd] = new Named("op_BitwiseAnd");
            BinaryOperator[EBinaryOperator.BitwiseOr] = new Named("op_BitwiseOr");
            BinaryOperator[EBinaryOperator.ExclusiveOr] = new Named("op_ExclusiveOr");
            BinaryOperator[EBinaryOperator.GreaterThan] = new Named("op_GreaterThan");
            BinaryOperator[EBinaryOperator.GreaterThanOrEqual] = new Named("op_GreaterThanOrEqual");
            BinaryOperator[EBinaryOperator.Equality] = new Named("op_Equality");
            BinaryOperator[EBinaryOperator.Inequality] = new Named("op_Inequality");
            BinaryOperator[EBinaryOperator.LessThan] = new Named("op_LessThan");
            BinaryOperator[EBinaryOperator.LessThanOrEqual] = new Named("op_LessThanOrEqual");
            BinaryOperator[EBinaryOperator.Addition] = new Named("op_Addition");
            BinaryOperator[EBinaryOperator.Subtraction] = new Named("op_Subtraction");
            BinaryOperator[EBinaryOperator.Multiply] = new Named("op_Multiply");
            BinaryOperator[EBinaryOperator.Division] = new Named("op_Division");
            BinaryOperator[EBinaryOperator.Modulus] = new Named("op_Modulus");
            BinaryOperator[EBinaryOperator.ShiftLeft] = new Named("op_LeftShift");
            BinaryOperator[EBinaryOperator.ShiftRight] = new Named("op_RightShift");
        }
        internal static Named GetUnaryOperator(EUnaryOperator unary)
        {
            Named op;
            UnaryOperator.TryGetValue(unary, out op);
            return op;
        }
        internal static Named GetBinaryOperator(EBinaryOperator binary)
        {
            Named op;
            BinaryOperator.TryGetValue(binary, out op);
            return op;
        }

        internal CSharpAssembly assembly;
        private HashSet<CSharpNamespace> fileUsing = new HashSet<CSharpNamespace>();
        private HashSet<CSharpNamespace> namespaceUsing = new HashSet<CSharpNamespace>();
        private CSharpNamespace definingNamespace;

        internal IEnumerable<CSharpNamespace> DefinedNamespaces
        {
            get
            {
                return Refactor.assemblies.SelectMany(a => a.Namespaces);
            }
        }
        protected CSharpNamespace DefiningNamespace
        {
            get { return definingNamespace; }
        }
        protected IEnumerable<CSharpNamespace> AssemblyAccessableNamespaces
        {
            get
            {
                //return namespaceUsing.Concat(fileUsing).Distinct();
                return fileUsing.Concat(namespaceUsing).Distinct();
            }
        }
        protected IEnumerable<CSharpType> AssemblyAccessableTypes
        {
            get
            {
                // 引用的命名空间顺序应该倒过来，继承相同名字的类型时，越后加载的程序集的类型越先被继承
                // 但是WebGL的Array会覆盖掉System.Array
                return AssemblyAccessableNamespaces.Reverse().SelectMany(a => a.Types);
            }
        }
        protected internal EFileType FileType { get; private set; }

        /// <summary>从NestedType寻找类型时，protected和private时是否可见</summary>
        protected virtual bool IsTypeVisible(CSharpType t)
        {
            return (t.Assembly == assembly && t.IsInternal) || t.IsPublic;
        }
        /// <returns>可访问的根类型，泛型内的内部就应该是其泛型参数</returns>
        protected virtual IEnumerable<CSharpType> GetAccessableRootTypes()
        {
            return null;
        }
        public CSharpType FindType(ReferenceMember node)
        {
            Stack<ReferenceMember> stack;
            CSharpType result = FindType(node, null, out stack);
            //if (stack.Count > 0)
            //    return null;
            return result;
        }
        public CSharpType FindType(ReferenceMember node, CSharpType defaultType, out Stack<ReferenceMember> surplus)
        {
            surplus = node.References;
            CSharpNamespace ns = null;
            CSharpType result = defaultType;
            CSharpType tempResult = result;     // 泛型类在构建类型之后，可能NestedType找不到类型，所以临时记录没构造的泛型类
            while (surplus.Count > 0)
            {
                var current = surplus.Peek();

                // 泛型实参
                IList<CSharpType> typeArguments = CSharpType.EmptyList;
                if (current.IsGeneric)
                {
                    CSharpType[] arguments = new CSharpType[current.GenericTypes.Count];
                    for (int i = 0; i < arguments.Length; i++)
                        // typeof(Nullable<>)
                        if (current.GenericTypes[i] != null)
                            arguments[i] = FindType(current.GenericTypes[i]);
                    typeArguments = arguments;
                }

                // Nullable转换
                var name = current.Name;
                if (name.Name.EndsWith("?"))
                {
                    // 转换成Nullable
                    name.Name = name.Name.Substring(0, name.Name.Length - 1);
                    typeArguments = new CSharpType[1] { FindType(current) };
                    name = Nullable;
                }

                // 内部查找类型
                CSharpType found = null;
                CSharpNamespace foundNS = null;
                if (name == Nullable)
                {
                    found = DefinedNamespaces.First(n => n.Name == "System").Types.FirstOrDefault(t => t.Name.Name == name.Name && t.TypeParametersCount == 1);
                }
                else if (result != null)
                {
                    // 从内部类中查找
                    found = tempResult.NestedTypes.FirstOrDefault(t => t.Name.Name == name.Name && t.TypeParameters.Count == typeArguments.Count && IsTypeVisible(t));
                }
                else
                {
                    if (ns != null)
                    {
                        // 从命名空间中查找
                        found = ns.Types.FirstOrDefault(t => t.Name.Name == name.Name && t.TypeParameters.Count == typeArguments.Count && IsTypeVisible(t));
                        if (found == null)
                            foundNS = ns.Namespaces.FirstOrDefault(n => n.Name == name.Name);
                    }
                    else
                    {
                        // 从可访问的根中查找
                        var accessableRootTypes = GetAccessableRootTypes();
                        if (accessableRootTypes != null)
                            found = accessableRootTypes.FirstOrDefault(t => t.Name.Name == name.Name && t.TypeParameters.Count == typeArguments.Count && IsTypeVisible(t));
                        if (found == null)
                            found = AssemblyAccessableTypes.FirstOrDefault(t => t.Name.Name == name.Name && t.TypeParameters.Count == typeArguments.Count && IsTypeVisible(t));
                        if (found == null)
                            foundNS = DefinedNamespaces.FirstOrDefault(n => n.Name == name.Name);
                    }
                }

                // Nullable转换完成将Syntax改回原样
                if (name == Nullable)
                    current.Name.Name += "?";

                if (found == null && foundNS == null)
                    break;

                if (foundNS != null)
                {
                    ns = foundNS;
                    OnFindNamespace(ns, current);
                }
                else
                {
                    // 构造泛型类型
                    tempResult = found;
                    result = found;
                    if (typeArguments.Count > 0 && !current.IsGenericDefinition)
                        result = CSharpType.CreateConstructedType(result, typeArguments);

                    // 构造数组类型
                    byte dimension = current.ArrayDimension;
                    while (dimension > 0)
                    {
                        result = CSharpType.CreateArray(1, result);
                        dimension--;
                    }

                    OnFindType(found, result, current);
                }

                surplus.Pop();
            }
            return result;
        }
        protected virtual void OnFindNamespace(CSharpNamespace target, ReferenceMember node)
        {
        }
        protected virtual void OnFindType(CSharpType found, CSharpType result, ReferenceMember node)
        {
        }
        protected virtual void AddType(TypeDefinitionInfo type)
        {
            assembly.Add(type);
            if (type.ContainingNamespace == null && DefiningNamespace != null)
            {
                DefiningNamespace.AddType(type);
                // 刷新存在的命名空间
                //if (!DefinedNamespaces.Any(d => d == definingNamespace))
                //    definedNamespaces = assembly.Namespaces;
            }
        }

        public override void Visit(DefineFile node)
        {
            //_LOG.Debug(node.Name);
            FileType = CodeFileHelper.ParseFileType(node.Name);
            foreach (var item in node.DefineNamespaces) CreateOrFindNamespace(item);
            fileUsing.Clear();
            foreach (var item in node.UsingNamespaces) Visit(item);
            foreach (var item in node.DefineNamespaces) Visit(item);
        }
        public override void Visit(DefineNamespace node)
        {
            CreateOrFindNamespace(node);
        }
        private void CreateOrFindNamespace(DefineNamespace node)
        {
            namespaceUsing.Clear();
            definingNamespace = null;
            foreach (var name in node.Name.GetNames())
            {
                string name2 = GetNamespaceName(name);
                CSharpNamespace find = DefinedNamespaces.FirstOrDefault(n => n.Name == name2);
                if (find == null)
                {
                    if (definingNamespace != null)
                        find = definingNamespace.Namespaces.FirstOrDefault(n => n.Name == name);
                    if (find == null)
                    {
                        find = new CSharpNamespace();
                        find.Name = name2;
                    }
                    if (definingNamespace != null)
                        definingNamespace.AddNamespace(find);
                }
                namespaceUsing.Add(find);
                definingNamespace = find;
            }
            // 空命名空间默认被引用
            foreach (var item in DefinedNamespaces.Where(n => string.IsNullOrEmpty(n.Name)))
                namespaceUsing.Add(item);
            foreach (var item in node.UsingNamespaces) Visit(node);
        }
        public override void Visit(UsingNamespace node)
        {
            if (node.Alias != null)
                throw new NotImplementedException();
            IEnumerable<CSharpNamespace> finder = DefinedNamespaces;
            foreach (var item in node.Reference.References)
            {
                string name = GetNamespaceName(item.Name.Name);
                var temp = finder.Where(n => n.Name == name);
                foreach (var ns in temp)
                    fileUsing.Add(ns);
                finder = temp.SelectMany(t => t.Namespaces);
            }
        }
        internal static string GetNamespaceName(string nsName)
        {
            // __开头的命名空间是为了和.net原本命名空间的类型区分而追加的前缀
            if (nsName.StartsWith("__"))
                return nsName.Substring(2);
            return nsName;
        }
    }
    /// <summary>构建命名空间，合并分布类</summary>
    internal class _BuildNamespace : _BuildTypeSystem
    {
        private Dictionary<string, DefineType> partialMap = new Dictionary<string, DefineType>();
        private Stack<DefineType> stack = new Stack<DefineType>();

        string ResolveFullName(DefineType node)
        {
            StringBuilder builder = new StringBuilder();
            string ns = DefiningNamespace.ToString();
            if (!string.IsNullOrEmpty(ns))
            {
                builder.Append(ns);
                builder.Append('.');
            }
            if (stack.Count > 0)
            {
                bool first = true;
                foreach (var item in stack)
                {
                    if (first)
                        first = false;
                    else
                        builder.Append('+');
                    ResolveTypeName(node, builder);
                }
            }
            ResolveTypeName(node, builder);
            return builder.ToString();
        }
        static void ResolveTypeName(DefineType node, StringBuilder builder)
        {
            builder.Append(node.Name.Name);
            var generic = node.Generic.GenericTypes;
            if (generic != null && generic.Count > 0)
            {
                builder.Append("<");
                for (int i = 0, n = generic.Count - 1; i <= n; i++)
                {
                    builder.Append(generic[i].Name);
                    if (i != n)
                        builder.Append(",");
                }
                builder.Append(">");
            }
        }
        static string ResolveTypeReferenceName(ReferenceMember reference)
        {
            StringBuilder builder = new StringBuilder(reference.Name.Name);
            if (reference.GenericTypes != null && reference.GenericTypes.Count > 0)
            {
                builder.Append("<");
                for (int i = 0, n = reference.GenericTypes.Count - 1; i <= n; i++)
                {
                    builder.Append(ResolveTypeReferenceName(reference.GenericTypes[i]));
                    if (i != n)
                        builder.Append(",");
                }
                builder.Append(">");
            }
            return builder.ToString();
        }

        void DeleteCombinedPartialType(List<DefineMember> types)
        {
            // 删除被合并的partial类型
            for (int i = types.Count - 1; i >= 0; i--)
            {
                if (types[i].Name == null)
                {
                    types.RemoveAt(i);
                }
                else
                {
                    DefineType type = types[i] as DefineType;
                    if (type != null)
                    {
                        DeleteCombinedPartialType(type.NestedType);
                    }
                }
            }
        }
        public override void Visit(DefineNamespace node)
        {
            base.Visit(node);
            foreach (var item in node.DefineTypes) if (item is DefineType) Visit(item);
            DeleteCombinedPartialType(node.DefineTypes);
            foreach (var item in node.DefineNamespaces) Visit(item);
        }
        public override void Visit(DefineType node)
        {
            bool isPartial = (node.Modifier & EModifier.Partial) != EModifier.None;
            bool isReplace = node.Name.Name.StartsWith("@_");
            if (isReplace)
                node.Name.Name = node.Name.Name.Substring(2);
            if (node.Name.Name.StartsWith("@"))
                node.Name.Name = node.Name.Name.Substring(1);
            if (node.Name.Name.EndsWith("Attribute") && node.Name.Name.Length > 9)
                node.Name.Name = node.Name.Name.Substring(0, node.Name.Name.Length - 9);
            if (isPartial || isReplace)
            {
                // 例如JS，string类型继承JS的String类型，方便重写Length{get{return this.length;}}，所以对string类型进行特殊命名
                TypeDefinitionInfo csharpType = new TypeDefinitionInfo();

                DefineType t;
                string fullname = ResolveFullName(node);
                if (partialMap.TryGetValue(fullname, out t))
                {
                    /*
                     * 合并partial类
                     * 1. 合并引用的命名空间（文件 | 命名空间）
                     * 2. 合并特性
                     * 3. 合并继承，注意继承和实现接口的顺序
                     * 4. 合并泛型约束
                     * 5. 合并内部定义的成员
                     */
                    foreach (var item in node.Namespace.File.UsingNamespaces)
                    {
                        string name = item.Reference.GetNamespaceName();
                        if (t.Namespace.File.UsingNamespaces.Any(n => n.Reference.GetNamespaceName() == name))
                            continue;
                        t.Namespace.File.UsingNamespaces.Add(item);
                    }
                    foreach (var item in node.Namespace.UsingNamespaces)
                    {
                        string name = item.Reference.GetNamespaceName();
                        if (t.Namespace.UsingNamespaces.Any(n => n.Reference.GetNamespaceName() == name))
                            continue;
                        t.Namespace.UsingNamespaces.Add(item);
                    }
                    t.Attributes.AddRange(node.Attributes);
                    t.Generic.Constraints.AddRange(node.Generic.Constraints);
                    //if (!isReplace && node.Inherits.Count > 0)
                    if (node.Inherits.Count > 0)
                    {
                        var names = t.Inherits.Select(a => ResolveTypeReferenceName(a)).ToList();
                        foreach (var inherit in node.Inherits)
                        {
                            string name = ResolveTypeReferenceName(inherit);
                            if (!names.Contains(name))
                            {
                                if (isReplace)
                                    t.Inherits.Insert(0, inherit);
                                else
                                    t.Inherits.Add(inherit);
                            }
                        }
                    }
                    t.NestedType.AddRange(node.NestedType);
                    //if (isPartial)
                    //{
                    //    // partial类则直接添加
                    //    t.Fields.AddRange(node.Fields);
                    //    t.Properties.AddRange(node.Properties);
                    //    t.Methods.AddRange(node.Methods);
                    //}
                    //else
                    {
                        // 特殊类遇到同样的成员则替换该成员 Modifier不替换
                        int index;
                        foreach (var item in node.Fields)
                        {
                            index = t.Fields.IndexOf(d => d.Name.Name == item.Name.Name);
                            if (index == -1)
                            {
                                t.Fields.Add(item);
                            }
                            else
                            {
                                var temp = t.Fields[index];
                                //item.Modifier = temp.Modifier;
                                item.Attributes = temp.Attributes;
                                t.Fields[index] = item;
                            }
                        }
                        foreach (var item in node.Properties)
                        {
                            index = t.Properties.IndexOf(d =>
                            {
                                int pcount = item.Arguments == null ? 0 : item.Arguments.Count;
                                if (d.Name.Name == item.Name.Name && (d.Arguments == null ? 0 : d.Arguments.Count) == pcount)
                                {
                                    bool flag = true;
                                    for (int i = 0; i < pcount; i++)
                                    {
                                        // 参数不同则不是同一个索引器
                                        if (d.Arguments[i].Direction != item.Arguments[i].Direction ||
                                            ResolveTypeReferenceName(d.Arguments[i].Type) != ResolveTypeReferenceName(item.Arguments[i].Type))
                                        {
                                            flag = false;
                                            break;
                                        }
                                    }
                                    if (flag)
                                        return true;
                                }
                                return false;
                            });
                            if (index == -1)
                            {
                                t.Properties.Add(item);
                            }
                            else
                            {
                                var temp = t.Properties[index];
                                //item.Modifier = temp.Modifier;
                                item.Attributes = temp.Attributes;
                                t.Properties[index] = item;
                            }
                        }
                        foreach (var item in node.Methods)
                        {
                            index = t.Methods.IndexOf(d =>
                            {
                                int pcount = item.Arguments == null ? 0 : item.Arguments.Count;
                                if (d.Name.Name == item.Name.Name && (d.Arguments == null ? 0 : d.Arguments.Count) == pcount)
                                {
                                    bool flag = true;
                                    for (int i = 0; i < pcount; i++)
                                    {
                                        // 参数不同则不是同一个方法
                                        if (d.Arguments[i].Direction != item.Arguments[i].Direction ||
                                            ResolveTypeReferenceName(d.Arguments[i].Type) != ResolveTypeReferenceName(item.Arguments[i].Type))
                                        {
                                            flag = false;
                                            break;
                                        }
                                    }
                                    if (flag)
                                        return true;
                                }
                                return false;
                            });
                            if (index == -1)
                            {
                                t.Methods.Add(item);
                            }
                            else
                            {
                                var temp = t.Methods[index];
                                //item.Modifier = temp.Modifier;
                                item.Attributes = temp.Attributes;
                                t.Methods[index] = item;
                            }
                        }
                    }

                    // 移除掉被合并的partial类
                    node.Name = null;
                }
                else
                {
                    if (!isPartial && isReplace)
                    {
                        _LOG.Warning("替换类型应该在源类型后被解析[{0}]", fullname);
                        return;
                    }
                    partialMap.Add(fullname, node);
                }
            }

            stack.Push(node);
            foreach (var item in node.NestedType) Visit(item);
            stack.Pop();
        }
    }
    internal class DefinedType
    {
        public SyntaxNode Define;
        public EFileType DefineFile;
        public CSharpType Type;
    }
    internal abstract class _BuildTypeBase : _BuildTypeSystem
    {
        internal static Dictionary<SyntaxNode, DefinedType> types;
        internal static CSharpType GetType(SyntaxNode node)
        {
            DefinedType define;
            if (types.TryGetValue(node, out define))
                return define.Type;
            return null;
        }
        private Stack<TypeDefinitionInfo> definingTypes = new Stack<TypeDefinitionInfo>();
        protected TypeDefinitionInfo DefiningType
        {
            get { return definingTypes.Count == 0 ? null : definingTypes.Peek(); }
        }
        protected void AddType(SyntaxNode node, CSharpType type)
        {
            if (types == null)
                types = new Dictionary<SyntaxNode, DefinedType>();
            DefinedType define = new DefinedType();
            define.Define = node;
            define.DefineFile = FileType;
            define.Type = type;
            types.Add(node, define);
            if (type is TypeDefinitionInfo)
                AddType((TypeDefinitionInfo)type);
        }
        protected override void AddType(TypeDefinitionInfo type)
        {
            if (DefiningType != null)
                DefiningType.Add(type);
            base.AddType(type);
        }
        protected override bool IsTypeVisible(CSharpType nestedType)
        {
            bool result = base.IsTypeVisible(nestedType);
            if (DefiningType == null || result)
                return result;
            if (nestedType.IsTypeParameter)
                return true;
            CSharpType type = DefiningType;
            while (type != null)
            {
                if (type.NestedTypes.Contains(nestedType) &&
                    // 当前外部类的NestedType或者其继承的非私有的NestedType
                    (type == nestedType.ContainingType || !nestedType.IsPrivate))
                {
                    return true;
                }
                type = type.ContainingType;
            }
            return false;
        }
        protected override IEnumerable<CSharpType> GetAccessableRootTypes()
        {
            CSharpType parent = DefiningType;
            while (parent != null)
            {
                yield return parent;
                foreach (var item in parent.TypeParameters)
                    yield return item;
                foreach (var item in parent.NestedTypes)
                    yield return item;
                parent = parent.ContainingType;
            }
        }
        public override void Visit(DefineNamespace node)
        {
            base.Visit(node);
            foreach (var item in node.DefineTypes) Visit(item);
            foreach (var item in node.DefineNamespaces) Visit(item);
        }
        public override void Visit(DefineType node)
        {
            definingTypes.Push((TypeDefinitionInfo)types[node].Type);
            InternalVisitType(node);
            definingTypes.Pop();
        }
        protected virtual void InternalVisitType(DefineType node)
        {
            base.Visit(node);
        }
        public override void Visit(DefineEnum node)
        {
            definingTypes.Push((TypeDefinitionInfo)types[node].Type);
            InternalVisitEnum(node);
            definingTypes.Pop();
        }
        protected virtual void InternalVisitEnum(DefineEnum node)
        {
            base.Visit(node);
        }
        public override void Visit(DefineDelegate node)
        {
            definingTypes.Push((TypeDefinitionInfo)types[node].Type);
            InternalVisitDelegate(node);
            definingTypes.Pop();
        }
        protected virtual void InternalVisitDelegate(DefineDelegate node)
        {
            base.Visit(node);
        }
    }
    internal class _BuildType : _BuildTypeBase
    {
        private TypeAttributes GetTypeAttributes(EModifier modifier)
        {
            TypeAttributes result = TypeAttributes.None;

            CSharpType parent = DefiningType;
            if (parent == null)
            {
                if ((modifier & EModifier.Public) != EModifier.None)
                    result |= TypeAttributes.Public;
                else
                    result |= TypeAttributes.Internal;
            }
            else
            {
                if ((modifier & EModifier.Public) != EModifier.None)
                    result |= TypeAttributes.NestedPublic;
                else
                {
                    bool isProtected = (modifier & EModifier.Protected) != EModifier.None;
                    bool isInternal = (modifier & EModifier.Internal) != EModifier.None;
                    if (isProtected && isInternal)
                        result |= TypeAttributes.NestedProtectedOrInternal;
                    else if (isProtected)
                        result |= TypeAttributes.NestedProtected;
                    else if (isInternal)
                        result |= TypeAttributes.NestedInternal;
                    else
                        result |= TypeAttributes.NestedPrivate;
                }
            }

            if ((modifier & EModifier.Static) != EModifier.None)
                result |= TypeAttributes.Abstract | TypeAttributes.Sealed;
            else
            {
                if ((modifier & EModifier.Abstract) != EModifier.None)
                    result |= TypeAttributes.Abstract;
                if ((modifier & EModifier.Sealed) != EModifier.None)
                    result |= TypeAttributes.Sealed;
            }

            return result;
        }
        private void BuildTypeParameters(TypeDefinitionInfo type, DefineGeneric generic)
        {
            int pcount = generic.GenericTypes.Count;
            type._TypeParameters = new TypeParameterData[pcount];
            for (int i = 0; i < pcount; i++)
            {
                var g = generic.GenericTypes[i];
                TypeParameterData tpi = new TypeParameterData();
                //TypeParameterInfo tpi = new TypeParameterInfo(type, i, type);
                tpi.Name = g.Name;
                tpi.Variance = g.Variance;
                type._TypeParameters[i] = tpi;
                //AddType(g, tpi);
            }
            var typeParameters = type.TypeParameters;
            for (int i = 0; i < generic.GenericTypes.Count; i++)
            {
                var g = generic.GenericTypes[i];
                var tpi = typeParameters[i];
                AddType(g, tpi);
            }
        }
        public override void Visit(DefineType node)
        {
            TypeDefinitionInfo type = new TypeDefinitionInfo();

            type._Name = node.Name;
            switch (type._Name.Name)
            {
                case "object": CSharpType.OBJECT = type; break;
                case "Delegate": CSharpType.DELEGATE = type; break;
                case "MulticastDelegate": CSharpType.MULTICAST_DELEGATE = type; break;
                case "Array": if (DefiningNamespace.Name == "System") CSharpType.ARRAY = type; break;
                case "Enum": CSharpType.ENUM = type; break;
                case "Math": if (DefiningNamespace.Name == "System") CSharpType.MATH = type; break;
                case "ValueType": CSharpType.VALUE_TYPE = type; break;
                case "void": CSharpType.VOID = type; break;
                case "bool": CSharpType.BOOL = type; break;
                case "byte": CSharpType.BYTE = type; break;
                case "sbyte": CSharpType.SBYTE = type; break;
                case "ushort": CSharpType.USHORT = type; break;
                case "short": CSharpType.SHORT = type; break;
                case "char": CSharpType.CHAR = type; break;
                case "uint": CSharpType.UINT = type; break;
                case "int": CSharpType.INT = type; break;
                case "float": CSharpType.FLOAT = type; break;
                case "ulong": CSharpType.ULONG = type; break;
                case "long": CSharpType.LONG = type; break;
                case "double": CSharpType.DOUBLE = type; break;
                case "string": CSharpType.STRING = type; break;
                case "IEnumerable": if (DefiningNamespace.Name == "Generic") CSharpType.IENUMERABLE = type; break;
                case "IEnumerator": if (DefiningNamespace.Name == "Generic") CSharpType.IENUMERATOR = type; break;
                case "Nullable": if (node.Generic.GenericTypes.Count == 1) CSharpType.NULLABLE = type; break;

                case "___Array": ArrayTypeReference.___Array = type; break;
            }

            if (node.Type == EType.CLASS)
                type._TypeAttributes |= TypeAttributes.Class;
            else if (node.Type == EType.STRUCT)
                type._TypeAttributes |= TypeAttributes.ValueType;
            else if (node.Type == EType.INTERFACE)
                type._TypeAttributes |= TypeAttributes.Interface;
            type._TypeAttributes |= GetTypeAttributes(node.Modifier);

            BuildTypeParameters(type, node.Generic);

            AddType(node, type);

            base.Visit(node);
        }
        protected override void InternalVisitType(DefineType node)
        {
            foreach (var item in node.NestedType) Visit(item);
        }
        public override void Visit(DefineEnum node)
        {
            TypeDefinitionInfo type = new TypeDefinitionInfo();
            type._Name = node.Name;
            type._TypeAttributes |= TypeAttributes.Enum;
            type._TypeAttributes |= GetTypeAttributes(node.Modifier);
            AddType(node, type);
        }
        public override void Visit(DefineDelegate node)
        {
            TypeDefinitionInfo type = new TypeDefinitionInfo();
            type._Name = node.Name;
            type._TypeAttributes |= TypeAttributes.Delegate;
            type._TypeAttributes |= GetTypeAttributes(node.Modifier);
            BuildTypeParameters(type, node.Generic);
            AddType(node, type);
        }
    }
    internal abstract class _BuildMemberBase : _BuildTypeBase
    {
        internal static Dictionary<SyntaxNode, MemberDefinitionInfo> members = new Dictionary<SyntaxNode, MemberDefinitionInfo>();
        private MemberDefinitionInfo definingMember;
        private int memberIndex;
        protected MemberDefinitionInfo DefiningMember
        {
            get { return definingMember; }
        }
        protected override IEnumerable<CSharpType> GetAccessableRootTypes()
        {
            if (DefiningMember != null)
                return DefiningMember.TypeParameters.Concat(base.GetAccessableRootTypes());
            return base.GetAccessableRootTypes();
        }
        public override void Visit(DefineType node)
        {
            memberIndex = 0;
            definingMember = null;
            base.Visit(node);
            memberIndex = 0;
            definingMember = null;
        }
        protected virtual MemberDefinitionInfo CreateMemberDefinition(SyntaxNode node)
        {
            MemberDefinitionInfo member = DefiningType.CreateMemberDefinition(memberIndex++);
            members.Add(node, member);
            return member;
        }
        protected void SetModifier(MemberDefinitionInfo info, EModifier modifier)
        {
            if ((modifier & EModifier.Public) != EModifier.None)
                info._MemberAttributes |= MemberAttributes.Public;
            else if ((modifier & EModifier.Private) != EModifier.None)
                info._MemberAttributes |= MemberAttributes.Private;
            else
            {
                bool isProtected = (modifier & EModifier.Protected) != EModifier.None;
                bool isInternal = (modifier & EModifier.Internal) != EModifier.None;
                if (isProtected && isInternal)
                    info._MemberAttributes |= MemberAttributes.ProtectedOrInternal;
                else if (isProtected)
                    info._MemberAttributes |= MemberAttributes.Protected;
                else if (isInternal)
                    info._MemberAttributes |= MemberAttributes.Internal;
                else
                    info._MemberAttributes |= DefiningType.IsInterface ? MemberAttributes.Public : MemberAttributes.Private;
            }

            if ((modifier & EModifier.Abstract) != EModifier.None)
                info._MemberAttributes |= MemberAttributes.Abstract;
            if ((modifier & EModifier.Virtual) != EModifier.None)
                info._MemberAttributes |= MemberAttributes.Virtual;
            if ((modifier & EModifier.Override) != EModifier.None)
                info._MemberAttributes |= MemberAttributes.Override;
            if ((modifier & EModifier.Sealed) != EModifier.None)
                info._MemberAttributes |= MemberAttributes.Sealed;

            if ((modifier & EModifier.New) != EModifier.None)
                info._MemberAttributes |= MemberAttributes.New;
            if ((modifier & EModifier.Readonly) != EModifier.None)
                info._MemberAttributes |= MemberAttributes.ReadOnly;
            if ((modifier & EModifier.Static) != EModifier.None)
                info._MemberAttributes |= MemberAttributes.Static;

            if ((modifier & EModifier.Extern) != EModifier.None)
                info._MemberAttributes |= MemberAttributes.Extern;
        }
        public override void Visit(DefineField node)
        {
            definingMember = members[node];
            var field = definingMember;
            InternalVisitField(node);
            if (node.IsMultiple)
            {
                foreach (var item in node.Multiple)
                {
                    members.TryGetValue(item, out definingMember);
                    InternalVisitField(field, item);
                }
            }
        }
        protected virtual void InternalVisitField(DefineField node)
        {
            base.Visit(node);
        }
        protected virtual void InternalVisitField(MemberDefinitionInfo member, Field node)
        {
            base.Visit(node);
        }
        public override void Visit(DefineProperty node)
        {
            definingMember = members[node];
        }
        public override void Visit(DefineConstructor node)
        {
            definingMember = members[node];
        }
        public override void Visit(DefineMethod node)
        {
            definingMember = members[node];
        }
        protected override void InternalVisitDelegate(DefineDelegate node)
        {
            // 会调用Visit(DefineMethod)导致找不到引用
            //base.Visit(node);
        }
    }
    internal class _BuildMember : _BuildMemberBase
    {
        internal static Named GET_ENUMERATOR;

        private void BuildTypeParameterConstraint(TypeParameterData[] datas, DefineGeneric generic)
        {
            if (datas == null)
                return;
            for (int i = 0; i < datas.Length; i++)
            {
                var constraint = generic.Constraints.FirstOrDefault(c => c.Type.Name.Name == datas[i].Name.Name);
                if (constraint != null)
                {
                    foreach (var item in constraint.Constraints)
                    {
                        if (item.Name.Name == "new()")
                            datas[i].HasConstructorConstraint = true;
                        else if (item.Name.Name == "struct")
                            datas[i].HasValueTypeConstraint = true;
                        else if (item.Name.Name == "class")
                            datas[i].HasReferenceTypeConstraint = true;
                        else
                            datas[i].AddConstraint(FindType(item));
                    }
                }
            }
        }
        private void BuildMemberTypeParameters(MemberDefinitionInfo member, DefineGeneric generic)
        {
            int pcount = generic.GenericTypes.Count;
            member._TypeParameters = new TypeParameterData[pcount];
            for (int i = 0; i < pcount; i++)
            {
                var g = generic.GenericTypes[i];
                TypeParameterData tpi = new TypeParameterData();
                //TypeParameterInfo tpi = new TypeParameterInfo(type, i, type);
                tpi.Name = g.Name;
                tpi.Variance = g.Variance;
                member._TypeParameters[i] = tpi;
                //AddType(g, tpi);
            }
            var typeParameters = member.TypeParameters;
            for (int i = 0; i < generic.GenericTypes.Count; i++)
            {
                var g = generic.GenericTypes[i];
                var tpi = typeParameters[i];
                AddType(g, tpi);
            }
        }
        private void BuildMemberParameters(MemberDefinitionInfo member, List<FormalArgument> arguments)
        {
            if (arguments == null || arguments.Count == 0)
                return;
            int count = arguments.Count;
            member._Parameters = new MemberDefinitionInfo.ParameterData[count];
            for (int i = 0; i < count; i++)
            {
                var argument = arguments[i];
                var data = new MemberDefinitionInfo.ParameterData();
                data.Direction = argument.Direction;
                data.Name = argument.Name;
                data.Type = FindType(argument.Type);
                if (argument.Value != null)
                    _LOG.Warning("暂时不应支持方法参数带默认值，方法匹配时会匹配不上 Type={0} Method={1}", DefiningType.Name.Name, member.Name);
                member._Parameters[i] = data;
            }
        }

        protected override void InternalVisitType(DefineType node)
        {
            Visit(node.Attributes);
            BuildTypeParameterConstraint(DefiningType._TypeParameters, node.Generic);
            foreach (var item in node.Inherits)
            {
                var inherit = FindType(item);
                if (inherit == null)
                    throw new KeyNotFoundException("inherit");
                DefiningType._BaseTypes.Add(inherit);
            }
            //base.InternalVisitType(node);
            foreach (var item in node.Fields) Visit(item);
            foreach (var item in node.Properties) Visit(item);
            foreach (var item in node.Methods) Visit(item);
            foreach (var item in node.NestedType) Visit(item);
        }
        public override void Visit(DefineField node)
        {
            var member = CreateMemberDefinition(node);
            if ((node.Modifier & EModifier.Const) != EModifier.None)
                member._Kind = MemberDefinitionKind.Constant;
            else if ((node.Modifier & EModifier.Event) != EModifier.None)
                // 可能不需要这里
                member._Kind = MemberDefinitionKind.Event;
            else
                member._Kind = MemberDefinitionKind.Field;
            SetModifier(member, node.Modifier);
            member._ReturnType = FindType(node.Type);
            member._Name = node.Name;

            base.Visit(node);

            Visit(node.Attributes);
        }
        protected override void InternalVisitField(DefineField node)
        {
        }
        protected override void InternalVisitField(MemberDefinitionInfo member, Field node)
        {
            var multiple = CreateMemberDefinition(node);
            multiple._Kind = member._Kind;
            multiple._MemberAttributes = member._MemberAttributes;
            multiple._ReturnType = member._ReturnType;
            multiple._Name = node.Name;
        }
        public override void Visit(DefineProperty node)
        {
            var member = CreateMemberDefinition(node);
            base.Visit(node);
            Visit(node.Attributes);

            if ((node.Modifier & EModifier.Event) != EModifier.None)
                // 可能不需要这里
                member._Kind = MemberDefinitionKind.Event;
            else if (node.IsIndexer)
                member._Kind = MemberDefinitionKind.Indexer;
            else
                member._Kind = MemberDefinitionKind.Property;
            SetModifier(member, node.Modifier);
            member._ReturnType = FindType(node.Type);
            if (node.ExplicitImplement != null)
                member._ExplicitInterfaceImplementation = FindType(node.ExplicitImplement);
            member._Name = node.Name;

            //Visit(node.Arguments);
            BuildMemberParameters(member, node.Arguments);

            if (node.Getter != null) Visit(node.Getter);
            if (node.Setter != null) Visit(node.Setter);
        }
        public override void Visit(Accessor node)
        {
            Visit(node.Attributes);
            new MemberAccessor(DefiningMember, node.AccessorType == EAccessor.GET);
        }
        public override void Visit(DefineConstructor node)
        {
            var member = CreateMemberDefinition(node);
            base.Visit(node);
            Visit(node.Attributes);
            member._Kind = MemberDefinitionKind.Constructor;
            SetModifier(member, node.Modifier);
            if (node.Name.Name.StartsWith("@"))
                node.Name.Name = node.Name.Name.Substring(1);
            member._Name = node.Name;

            //Visit(node.Arguments);
            BuildMemberParameters(member, node.Arguments);
        }
        public override void Visit(DefineMethod node)
        {
            var member = CreateMemberDefinition(node);
            base.Visit(node);
            Visit(node.Attributes);
            // node.Name.Name.StartsWith("implicit")
            if (node.IsCast && (node.Modifier & EModifier.Implicit) == EModifier.Implicit)
                throw new NotImplementedException();
            if (node.IsOperator || node.IsCast)
                member._Kind = MemberDefinitionKind.Operator;
            else
                member._Kind = MemberDefinitionKind.Method;
            SetModifier(member, node.Modifier);
            if (node.ExplicitImplement != null)
                member._ExplicitInterfaceImplementation = FindType(node.ExplicitImplement);
            if (node.IsCast)
                // 暂不支持隐式类型转换
                member._Name = new Named(EXPLICIT.Name + node.ReturnType.ToAvailableName());
            else
                member._Name = node.Name;
            BuildMemberTypeParameters(member, node.Generic);
            BuildTypeParameterConstraint(member._TypeParameters, node.Generic);

            //if (node.HasReturnType)
            member._ReturnType = FindType(node.ReturnType);
            //Visit(node.Arguments);
            BuildMemberParameters(member, node.Arguments);

            if (member.ContainingType == CSharpType.IENUMERABLE && member.Name.Name == "GetEnumerator")
            {
                GET_ENUMERATOR = node.Name;
            }
        }
        protected override void InternalVisitEnum(DefineEnum node)
        {
            Visit(node.Attributes);

            foreach (var item in node.Fields)
            {
                var member = CreateMemberDefinition(item);
                member._Kind = MemberDefinitionKind.EnumMember;
                member._Name = item.Name;
                member._ReturnType = DefiningType;
                member._MemberAttributes |= MemberAttributes.Public | MemberAttributes.Static;
                Visit(item.Attributes);
            }
        }
        protected override void InternalVisitDelegate(DefineDelegate node)
        {
            Visit(node.Attributes);
            BuildTypeParameterConstraint(DefiningType._TypeParameters, node.Generic);
            //DefiningType._BaseTypes.Add(CSharpType.DELEGATE);
            // ?默认构造函数 ?默认调用方法
            var member = DefiningType.CreateMemberDefinition(0);
            member._Name = INVOKE;
            member._Kind = MemberDefinitionKind.Method;
            member._ReturnType = FindType(node.ReturnType);
            member._MemberAttributes = MemberAttributes.Public;
            BuildMemberParameters(member, node.Arguments);
        }
        //public override void Visit(List<FormalArgument> node)
        //{
        //    if (node == null || node.Count == 0)
        //        return;
        //    int count = node.Count;
        //    var member = DefiningMember;
        //    member._Parameters = new MemberDefinitionInfo.ParameterData[count];
        //    for (int i = 0; i < count; i++)
        //    {
        //        var item = node[i];
        //        MemberDefinitionInfo.ParameterData data = new MemberDefinitionInfo.ParameterData();
        //        data.Direction = item.Direction;
        //        data.Name = item.Name;
        //        data.Type = FindType(item.Type);
        //        member._Parameters[i] = data;
        //    }
        //}
        public override void Visit(List<InvokeAttribute> node)
        {
            if (node == null) return;

            Dictionary<CSharpType, object> attributes;
            if (DefiningMember == null)
                attributes = DefiningType._Attributes;
            else
                attributes = DefiningMember._Attributes;

            foreach (var item in node)
            {
                CSharpType atype = FindType((ReferenceMember)item.Target);
                if (!attributes.ContainsKey(atype))
                    attributes.Add(atype, null);
            }
        }
    }
    internal class _BuildReference : _BuildMemberBase
    {
        class _CalcExpressionType : SyntaxWalker
        {
            public static CSharpType SearchTypeParameter(CSharpType type)
            {
                if (type.IsTypeParameter)
                {
                    return type;
                }
                else if (type.IsConstructed)
                {
                    CSharpType result = null;
                    var arguments = type.TypeArguments;
                    foreach (var item in arguments)
                    {
                        CSharpType temp = SearchTypeParameter(item);
                        if (temp != null)
                        {
                            if (result == null)
                                result = temp;
                            else
                                // 有多个泛型类型的Lambda不能用作确定泛型参数
                                return null;
                        }
                    }
                    return result;
                }
                else if (type.IsArray)
                {
                    return SearchTypeParameter(type.ElementType);
                }
                return null;
            }
            public static bool MatchNumericType(CSharpType origin, CSharpType target)
            {
                if (origin == CSharpType.BYTE
                    || origin == CSharpType.SBYTE
                    || origin == CSharpType.USHORT
                    || origin == CSharpType.SHORT
                    || origin == CSharpType.CHAR
                    || origin == CSharpType.UINT
                    || origin == CSharpType.INT
                    || origin == CSharpType.ULONG
                    || origin == CSharpType.LONG
                    || origin == CSharpType.FLOAT
                    || origin == CSharpType.DOUBLE)
                {
                    return target == CSharpType.BYTE
                        || target == CSharpType.SBYTE
                        || target == CSharpType.USHORT
                        || target == CSharpType.SHORT
                        || target == CSharpType.CHAR
                        || target == CSharpType.UINT
                        || target == CSharpType.INT
                        || target == CSharpType.ULONG
                        || target == CSharpType.LONG
                        || target == CSharpType.FLOAT
                        || target == CSharpType.DOUBLE;
                }
                return false;
            }
            public static bool MatchedTargetType(CSharpType origin, CSharpType target)
            {
                if (origin != target)
                {
                    CSharpType result = ImplicitTypeChangeResult(origin, target);
                    if (result == target)
                    {
                        return true;
                    }
                }
                return false;
            }

            protected internal _BuildReference Builder;
            protected internal CSharpType Type;
            CSharpType lambdaType;
            BodyTree tree;
            bool isTest;
            /// <summary>作用域</summary>
            class BodyTree : Tree<BodyTree>
            {
                public VAR Variable;
                public BodyTree() { }
                public BodyTree(VAR variable)
                {
                    this.Variable = variable;
                }
                public override string ToString()
                {
                    return Variable.ToString();
                }
            }
            class LambdaTester : _CalcExpressionType
            {
                private List<CSharpType> returnTypes = new List<CSharpType>();

                private LambdaTester() { }
                public static bool TestLambda(_CalcExpressionType _base, CSharpType lambdaType, SyntaxNode lambda, bool testOnly, out CSharpType lambdaReturnType)
                {
                    if (!(lambda is Lambda))
                        throw new NotImplementedException();

                    lambdaReturnType = null;

                    LambdaTester tester = new LambdaTester();
                    _base.CopyTo(tester);
                    tester.lambdaType = lambdaType;
                    // 在test块中可能因为确定了某个泛型实参而内部的lambda不采用test，导致语法节点提前引用了不该引用的对象
                    tester.isTest = tester.isTest || testOnly;
                    // 记录树，后面用于还原
                    BodyTree leaf = _base.tree;
                    int count = _base.tree.ChildCount;
                    tester.tree = _base.tree;
                    try
                    {
                        tester.Visit((Lambda)lambda);
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                    if (testOnly)
                    {
                        // 还原树
                        for (int i = leaf.ChildCount - 1; i >= count; i--)
                            leaf.Remove(i);
                    }

                    if (tester.Type == null)
                        return false;
                    if (!testOnly)
                        _base.AddRef(lambda, lambdaType, null);
                    // 若Lambda返回值类型为null，应将Type赋值为lambdaType
                    if (tester.Type == lambdaType)
                        return true;
                    // 求出返回值类型
                    lambdaReturnType = tester.Type;
                    return true;
                }

                protected override void InternalVisitBodyStatement(SyntaxNode node)
                {
                    if (node is ReturnStatement)
                        Visit((ReturnStatement)node);
                    else
                        base.InternalVisitBodyStatement(node);
                }
                public override void Visit(ReturnStatement node)
                {
                    if (node.Value != null)
                    {
                        CSharpType returnType = Calc(node);
                        //if (returnType == null)
                        //    throw new NotImplementedException();
                        returnTypes.Add(returnType);
                    }
                }
                public override void Visit(Lambda node)
                {
                    CSharpMember lambdaInvoke = lambdaType.DelegateInvokeMethod;
                    var parameters = lambdaInvoke.Parameters;
                    // 执行body
                    int count = node.Parameters.Count;
                    if (count != parameters.Count)
                    {
                        // 不匹配
                        return;
                    }
                    for (int i = 0; i < count; i++)
                    {
                        FormalArgument arg = node.Parameters[i];
                        if (arg.Type != null)
                        {
                            CSharpType ptype = FindTypeAndRef(arg.Type);
                            if (ptype != parameters[i].Type)
                                return;
                        }
                        NewVariableAndRef(arg, parameters[i].Type, arg.Name);
                    }

                    if (node.IsSingleBody)
                        // 单个语句可能没有return也可能代表返回类型
                        returnTypes.Add(Calc(node.Body.Statements[0]));
                    else
                        // VisitBody应该计算出Body内的值或者错误
                        Visit(node.Body);

                    CSharpType testReturnType = lambdaInvoke.ReturnType;
                    if (returnTypes.Count == 0)
                    {
                        if (!CSharpType.IsVoid(testReturnType))
                        {
                            // 非void返回值，不匹配
                            Type = null;
                            return;
                        }
                    }
                    else
                    {
                        // 根据可能不同的返回类型确定最终返回类型
                        CSharpType result = returnTypes[0];
                        for (int i = 1; i < returnTypes.Count; i++)
                        {
                            CSharpType temp = returnTypes[i];
                            result = ImplicitTypeChangeResult(result, temp);
                        }
                        if (testReturnType != result)
                        {
                            if (testReturnType == null && result != null && node.IsSingleBody)
                            {
                                // void返回类型的lambda只有一句方法调用且该方法有返回值时可以视为返回void处理
                                var invoke = node.Body.Statements[0] as InvokeMethod;
                                if (invoke == null || invoke.IsIndexer)
                                    return;
                            }
                            else
                            {
                                if (ImplicitTypeChangeResult(result, testReturnType) != testReturnType)
                                    return;
                            }
                        }
                        // 泛型则已返回类型作为最终类型
                        if (SearchTypeParameter(testReturnType) != null)
                            testReturnType = result;
                    }
                    Type = testReturnType;
                    if (Type == null)
                        // 表示没有返回值
                        Type = lambdaType;
                }
            }
            class MethodInvoker : _CalcExpressionType
            {
                struct Argument
                {
                    /// <summary>匹配参数类型时用</summary>
                    public CSharpType Type;
                    public EDirection Direction;
                    public SyntaxNode Node;
                    /// <summary>定义参数时的类型，参数为Lambda表达式将为null，Primitive.null也是null</summary>
                    public CSharpType ArgumentType;
                    /// <summary>方法直接作为参数时找到但尚未确定的成员</summary>
                    public List<CSharpMember> Functions;

                    public CSharpType MatchedLambdaType;
                    public CSharpType MatchedLambdaReturnType;
                    public CSharpType TempLambdaType;
                    public CSharpMember MatchedDelegateMethod;

                    /// <summary>Lambda || ReferenceMember(Lambda的实现)</summary>
                    public bool IsLambda
                    {
                        get { return ArgumentType == null && (Node is Lambda || Node is ReferenceMember); }
                    }
                    public bool IsRef
                    {
                        get { return (Direction & EDirection.REF) != EDirection.NONE; }
                    }
                    public bool IsOut
                    {
                        get { return (Direction & EDirection.OUT) != EDirection.NONE; }
                    }
                    public CSharpType ResultReturnType
                    {
                        get
                        {
                            if (ArgumentType != null)
                                return ArgumentType;
                            return MatchedLambdaReturnType;
                        }
                    }
                    public bool IsUnresolvedDelegateMethod
                    {
                        get { return this.Functions != null && this.Functions.Count > 0; }
                    }

                    public bool ResolveDelegateMethod(CSharpType dtype)
                    {
                        //if (dtype == CSharpType.MULTICAST_DELEGATE || dtype == CSharpType.DELEGATE)
                        //    return true;
                        if (!dtype.IsDelegate)
                            //throw new InvalidCastException();
                            return false;
                        var method = dtype.DelegateInvokeMethod;
                        var ptypes = method.Parameters;
                        var rtype = method.ReturnType;
                        bool resolved = false;
                        foreach (var item in Functions)
                        {
                            var ptypes2 = item.Parameters;
                            if (ptypes.Count != ptypes2.Count)
                                continue;

                            bool flag = true;
                            for (int i = 0; i < ptypes.Count; i++)
                            {
                                if (ptypes[i].IsRef != ptypes2[i].IsRef ||
                                    ptypes[i].IsOut != ptypes2[i].IsOut ||
                                    !ptypes[i].Type.Equals(ptypes2[i].Type))
                                {
                                    flag = false;
                                    break;
                                }
                            }
                            if (!flag)
                                continue;

                            var rtype2 = item.ReturnType;
                            if (!rtype.Equals(rtype2))
                                continue;

                            MatchedDelegateMethod = item;
                            resolved = true;
                            break;
                        }
                        if (!resolved)
                        {
                            // 例如Action<T>和ActionRef<T>，单纯只是不匹配而已
                            return false;
                        }
                        // 最终委托的返回值
                        this.ArgumentType = dtype;
                        return true;
                    }
                }
                struct MemberFinder
                {
                    public enum EFind
                    {
                        Both,
                        /// <summary>实例成员</summary>
                        Instance,
                        /// <summary>静态成员</summary>
                        Static
                    }
                    public EFind Find;
                    internal _CalcExpressionType Calculator;
                    public MemberFinder(_CalcExpressionType _calc)
                    {
                        this.Find = EFind.Both;
                        this.Calculator = _calc;
                    }
                    /// <param name="definingMember">是否检查静态方法内不能引用实例成员，若不需检查，传入null，若有[类型.]作为前提也应传入null</param>
                    public bool CanVisitMember(CSharpMember _member)
                    {
                        CSharpType type = Calculator.Type == null ? Calculator.Builder.DefiningType : Calculator.Type;
                        // 处理这种情况IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
                        if (_member.ExplicitInterfaceImplementation != null && !_member.ExplicitInterfaceImplementation.Equals(type))
                            return false;
                        if (Find == EFind.Instance)
                        {
                            if (_member.IsStatic)
                                return false;
                            CSharpMember definingMember = Calculator.Builder.DefiningMember;
                            // 静态成员不能访问直接实例成员
                            if (definingMember.IsStatic)
                                return false;
                        }
                        // 静态构造函数不可显示访问
                        if (_member.IsConstructor && _member.IsStatic)
                            return false;
                        // 静态成员不能访问直接实例成员
                        if (Calculator.Type == null && Calculator.Builder.DefiningMember != null &&
                            Calculator.Builder.DefiningMember.IsStatic && !_member.IsStatic && !_member.IsConstant)
                            return false;
                        //if (Calculator.Type == null)
                        //    Calculator.Type = Calculator.Builder.DefiningType;
                        if (_member.IsPublic)
                            return true;
                        //if ((_member.IsProtectedOrInternal || _member.IsInternal) && _member.Assembly != type.Assembly)
                        if (_member.IsInternal && _member.Assembly != type.Assembly)
                            return false;
                        if (_member.IsPrivate)
                        {
                            //CSharpType _type = Calculator.Type;
                            CSharpType _type = type;
                            while (_type != null)
                            {
                                if (_member.ContainingType == _type)
                                    return true;
                                // Graph<T> where T : Graph<T> 此时T可以调用Graph<T>的Private成员
                                else if (_type.IsTypeParameter && _type.DefiningType == Calculator.Builder.DefiningType)
                                    return true;
                                _type = _type.ContainingType;
                            }
                            return false;
                        }
                        if (_member.IsProtectedOrInternal || _member.IsProtected)
                        {
                            //CSharpType _type = Calculator.Type;
                            CSharpType _type = type;
                            while (_type != null)
                            {
                                if (_member.ContainingType.Equals(_type))
                                    return true;
                                _type = _type.BaseClass;
                            }
                            return false;
                        }
                        return true;
                    }
                }
                enum EMatchMember
                {
                    /// <summary>不匹配</summary>
                    NOT_MATCH,
                    /// <summary>泛型匹配</summary>
                    GENERIC,
                    /// <summary>params参数匹配</summary>
                    PARAMS,
                    /// <summary>lambda参数匹配</summary>
                    LAMBDA,
                    /// <summary>参数类型不完全匹配</summary>
                    PTYPE,
                    /// <summary>完全匹配</summary>
                    ALL,
                }
                class NullMember : CSharpMember
                {
                    public override CSharpType ContainingType
                    {
                        get { throw new NotImplementedException(); }
                    }
                    public override CSharpAssembly Assembly
                    {
                        get { throw new NotImplementedException(); }
                    }
                    public override int GetHashCode()
                    {
                        throw new NotImplementedException();
                    }
                    public override bool Equals(CSharpMember other)
                    {
                        throw new NotImplementedException();
                    }
                }

                static CSharpMember EmptyMember = new NullMember();
                static IList<CSharpMember> SingleMember = new CSharpMember[1];
                static Argument[] EmptyList = new Argument[0];
                static List<CSharpMember> functions
                {
                    get { return _functions.Count == 0 ? null : _functions.Peek(); }
                }
                /// <summary>null: 不是调用方法 / EmptyList: 方法没有参数</summary>
                Argument[] methodArguments;
                bool isConstructor;
                CSharpMember matchedMember;

                bool HasMethodArgument
                {
                    get { return methodArguments != null; }
                }

                protected override void CopyTo(_CalcExpressionType _calc)
                {
                    base.CopyTo(_calc);
                    MethodInvoker invoker = _calc as MethodInvoker;
                    if (invoker != null)
                    {
                        invoker.isConstructor = this.isConstructor;
                        invoker.Type = this.Type;
                    }
                }
                static bool MatchMethod(CSharpMember m1, CSharpMember m2)
                {
                    var ptypes = m1.Parameters;

                    var ptypes2 = m2.Parameters;
                    if (ptypes.Count != ptypes2.Count)
                        return false;

                    var rtype = m1.ReturnType;
                    bool flag = true;
                    for (int i = 0; i < ptypes.Count; i++)
                    {
                        if (ptypes[i].IsRef != ptypes2[i].IsRef ||
                            ptypes[i].IsOut != ptypes2[i].IsOut ||
                            !ptypes[i].Type.Equals(ptypes2[i].Type))
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (!flag)
                        return false;

                    var rtype2 = m2.ReturnType;
                    if (!rtype.Equals(rtype2))
                        return false;

                    return true;
                }
                CSharpType SearchReturnGenericType(CSharpType type, CSharpType[] resolved)
                {
                    if (type.IsTypeParameter)
                    {
                        return type;
                    }
                    else if (type.IsConstructed)
                    {
                        CSharpType result = null;
                        var arguments = type.TypeArguments;
                        foreach (var item in arguments)
                        {
                            CSharpType temp = SearchReturnGenericType(item, resolved);
                            if (temp != null)
                            {
                                if (temp.IsTypeParameter && resolved[temp.TypeParameterPosition] != null)
                                    continue;
                                if (result == null)
                                    result = temp;
                                else
                                    // 有多个泛型类型的Lambda不能用作确定泛型参数
                                    return null;
                            }
                        }
                        return result;
                    }
                    else if (type.IsArray)
                    {
                        return SearchReturnGenericType(type.ElementType, resolved);
                    }
                    return null;
                }
                bool ResolveGenericType(ref CSharpMember member, CSharpType exType)
                {
                    int pindex = exType == null ? 0 : 1;
                    var typeParameters = member.TypeParameters;
                    var parameters = member.Parameters;
                    var resolvedGTs = new CSharpType[typeParameters.Count];
                    TypeParameterBinder binder = new TypeParameterBinder(typeParameters, resolvedGTs);
                    for (int i = 0, n = parameters.Count - 1; i <= n; i++)
                    {
                        var parameter = parameters[i];
                        var parameterType = parameter.Type;
                        var argumentType = exType;
                        if (!(i == 0 && pindex > 0))
                        {
                            var argument = methodArguments[i - pindex];
                            // params
                            if (parameter.IsParams && argument.ArgumentType != null && !argument.ArgumentType.IsArray)
                                parameterType = parameterType.ElementType;
                            argumentType = argument.ArgumentType;
                            if (argument.IsLambda)
                            {
                                // BUG: (Func<T, T2>, T[])确定后面参数的类型后，是可以带入T，确定T2类型的
                                // 根据Lambda的返回值可能可以确定泛型类型
                                if (!parameterType.IsDelegate)
                                    return false;
                                var invoke = parameterType.DelegateInvokeMethod;
                                if (invoke.ReturnType == null)
                                    continue;
                                else
                                {
                                    var typeParameter = SearchReturnGenericType(invoke.ReturnType, resolvedGTs);
                                    if (typeParameter == null)
                                        continue;
                                }
                                // 将已确定的泛型带入，已决定有两个泛型的其中一个，让其能够顺利解决类型
                                if (!LambdaTester.TestLambda(this, binder.ProcessType(parameterType), argument.Node, true, out argumentType))
                                    return false;
                                if (argumentType == null)
                                    return false;
                                parameterType = invoke.ReturnType;
                            }
                            else if (argument.IsUnresolvedDelegateMethod)
                            {
                                argument.ResolveDelegateMethod(parameterType);
                            }
                        }
                        if (!ResolveGenericType(parameterType, argumentType, resolvedGTs))
                            return false;
                    }
                    // todo: 解决上面的BUG，由于目前很少有委托在前，数组在后的情况，所以暂未解决
                    //bool lambdaFlag = true;
                    //while (lambdaFlag)
                    //{
                    //}
                    for (int i = 0; i < typeParameters.Count; i++)
                        if (resolvedGTs[i] == null)
                            return false;
                    member = new MemberWithTypeArguments(member, resolvedGTs);
                    return true;
                }
                bool ResolveGenericType(CSharpType parameterType, CSharpType argumentType, CSharpType[] output)
                {
                    // lambda或者primitive.null的表示暂未解决的类型
                    if (argumentType == null)
                        return true;

                    // 最终获得泛型参数对应的泛型类型
                    if (parameterType.IsTypeParameter && argumentType.Is(parameterType))
                    {
                        CSharpType temp = output[parameterType.TypeParameterPosition];
                        if (temp != null)
                        {
                            output[parameterType.TypeParameterPosition] = ImplicitTypeChangeResult(temp, argumentType);
                        }
                        else
                        {
                            output[parameterType.TypeParameterPosition] = argumentType;
                        }
                        return true;
                    }

                    /* 实参类型必定是形参类型或者其实现类 */
                    if (parameterType.IsArray)
                    {
                        if (!argumentType.IsArray)
                            return false;
                        return ResolveGenericType(parameterType.ElementType, argumentType.ElementType, output);
                    }

                    if (parameterType.IsConstructed)
                    {
                        if (!argumentType.IsType(parameterType.DefiningType, out argumentType))
                            return false;
                        var parameters = parameterType.TypeArguments;
                        var arguments = argumentType.TypeArguments;
                        if (parameters.Count != arguments.Count)
                            ResolveGenericType(parameterType, argumentType, output);
                        else
                            for (int i = 0; i < parameters.Count; i++)
                                if (!ResolveGenericType(parameters[i], arguments[i], output))
                                    return false;
                        return true;
                    }

                    return true;
                }
                CSharpMember ResolveMember(IList<CSharpMember> members, IList<CSharpType> generics)
                {
                    if (members.Count == 0)
                        throw new InvalidOperationException();

                    if (members.Count >= 1)
                    {
                        CSharpMember member = members[0];
                        if (!member.IsMethod && !member.IsConstructor && !member.IsIndexer)
                            return member;
                    }

                    /*
                     * 方法重载
                     * 方法参数个数大于1个时不会出现最佳匹配问题，因为满足两个方法的条件将编译不通过
                     * case 1:
                     *  static void Test(int a, short b) { }
                        static void Test(short a, float b) { }
                     * Test((byte)5, (byte)5);  // 编译错误，两个方法间调用不明确
                     * 
                     * case 2:
                     * 泛型方法和普通方法都匹配的情况下，普通方法优先
                     */
                    EMatchMember matchedMode = EMatchMember.NOT_MATCH;
                    CSharpMember matchedMember = null;
                    IList<CSharpParameter> matchedParameters = null;
                    bool gFlag = generics != null && generics.Count > 0;
                    for (int i = 0; i < members.Count; i++)
                    {
                        CSharpMember member = members[i];
                        if (!(member.IsIndexer || member.IsMethod) && methodArguments == null)
                            return null;

                        // 方法参数是委托类型，直接指定方法作为该参数
                        if (methodArguments == null)
                        {
                            if (lambdaType != null)
                            {
                                // 能确定委托类型
                                if (!MatchMethod(lambdaType.DelegateInvokeMethod, member))
                                    continue;
                                return member;
                            }
                            else
                            {
                                return EmptyMember;
                            }
                        }

                        var parameters = member.Parameters;
                        // 参数个数不匹配
                        if (methodArguments.Length != parameters.Count && !parameters.Any(p => p.IsThis || p.IsParams))
                            continue;

                        // 匹配参数的开始索引，this扩展方法可能跳过第一个参数
                        int pindex = 0;
                        if (parameters.Count > 0 && parameters[0].IsThis)
                        {
                            // this
                            if (Type != null)
                            {
                                if (Type.Equals(member.ContainingType))
                                {
                                    // 手动调用静态扩展方法
                                    pindex = 0;
                                }
                                else
                                {
                                    if (Type.Is(parameters[0].Type))
                                        pindex = 1;
                                    else
                                        continue;
                                }
                            }
                            // 可能静态方法的方式调用扩展方法
                            //else
                            //    continue;
                        }
                        // 实参与形参个数的差值
                        int paramCountDiff = methodArguments.Length - (parameters.Count - pindex);
                        if (paramCountDiff < -1)
                        {
                            // 少两个则肯定不对
                            continue;
                        }
                        else if (paramCountDiff > 0 || paramCountDiff == -1)
                        {
                            // 参数多出或者仅少一个必须是params
                            if (!parameters[parameters.Count - 1].IsParams)
                                continue;
                        }

                        var typeParameters = member.TypeParameters;
                        if (generics != null && generics.Count != typeParameters.Count)
                            continue;
                        bool implicitGFlag = false;
                        if (typeParameters.Count > 0)
                        {
                            if (generics == null)
                            {
                                // 隐式决定泛型参数
                                if (!ResolveGenericType(ref member, pindex > 0 ? Type : null))
                                    continue;
                                implicitGFlag = true;
                            }
                            else
                                member = new MemberWithTypeArguments(member, generics);
                            parameters = member.Parameters;
                        }

                        // 决定参数进行匹配 P.S.参数只有一个是有最佳匹配，大于一个时有普通匹配和params匹配
                        EMatchMember match = ParameterMatch(pindex, methodArguments, parameters);
                        UnresolveMethodArgumentLambdaType();
                        if (match != EMatchMember.NOT_MATCH)
                        {
                            if (match == EMatchMember.ALL)
                            {
                                if (implicitGFlag)
                                {
                                    match = EMatchMember.GENERIC;
                                }
                                else
                                {
                                    //return member;// 不能跳过确定lambda类型的部分
                                    matchedMember = member;
                                    break;
                                }
                            }
                            if (match > matchedMode || (match == matchedMode && matchedMode == EMatchMember.LAMBDA))
                            {
                                matchedMode = match;
                                matchedMember = member;
                                matchedParameters = parameters;
                            }
                            else if (match == matchedMode)
                            {
                                if (match == EMatchMember.PTYPE && !gFlag)
                                {
                                    // 取参数类型最合适的方法
                                    for (int j = pindex; j < parameters.Count; j++)
                                    {
                                        if (MatchedTargetType(matchedParameters[j].Type, parameters[j].Type))
                                        {
                                            matchedMember = member;
                                            matchedParameters = parameters;
                                            break;
                                        }
                                    }
                                }
                                //else
                                //    throw new NotImplementedException();
                            }
                        }
                    }
                    if (matchedMember == null)
                    {
                        return null;
                    }
                    // 确定lambda类型
                    for (int i = 0; i < methodArguments.Length; i++)
                    {
                        var argument = methodArguments[i];
                        if (argument.IsLambda && !argument.IsUnresolvedDelegateMethod)
                            LambdaTester.TestLambda(this, argument.MatchedLambdaType, argument.Node, false, out argument.MatchedLambdaReturnType);
                    }
                    return matchedMember;
                }
                EMatchMember ParameterMatch(int pindex, Argument[] arguments, IList<CSharpParameter> parameters)
                {
                    EMatchMember match = EMatchMember.ALL;
                    // 确定赋值lambda表达式的类型
                    if (arguments.Any(a => a.ArgumentType == null))
                    {
                        // 参数有未确定类型的lambda表达式时带入方法查看匹配
                        for (int j = 0; j < arguments.Length; j++)
                        {
                            int jindex = j + pindex;
                            jindex = jindex < parameters.Count - 1 ? jindex : parameters.Count - 1;
                            //CSharpType ptype = parameters[j + pindex].Type;
                            if (arguments[j].IsUnresolvedDelegateMethod)
                            {
                                CSharpType testType;
                                if (this.lambdaType != null)
                                    testType = this.lambdaType;
                                else
                                    testType = parameters[jindex].Type;
                                if (!arguments[j].ResolveDelegateMethod(testType))
                                    return EMatchMember.NOT_MATCH;
                            }
                            else if (arguments[j].IsLambda)
                            {
                                CSharpType lambdaType = parameters[jindex].Type;
                                if (!lambdaType.IsDelegate)
                                    return EMatchMember.NOT_MATCH;
                                CSharpType returnType;
                                if (!LambdaTester.TestLambda(this, lambdaType, arguments[j].Node, true, out returnType))
                                    return EMatchMember.NOT_MATCH;
                                var lambdaTypeParameters = lambdaType.TypeParameters;
                                if (lambdaTypeParameters.Count > 0)
                                {
                                    // 泛型委托
                                    // 隐式决定泛型参数 应在外部决定此委托参数的类型
                                }
                                CSharpType matchedReturnType = arguments[j].MatchedLambdaReturnType;
                                if (matchedReturnType != null)
                                {
                                    // 取返回值类型小的，例如int和float，取int，但是int和void取int
                                    if (returnType == null || MatchedTargetType(matchedReturnType, returnType))
                                        return EMatchMember.NOT_MATCH;
                                }
                                arguments[j].TempLambdaType = lambdaType;
                                arguments[j].MatchedLambdaReturnType = returnType;
                                arguments[j].Type = lambdaType;
                                match = EMatchMember.LAMBDA;
                            }
                            else if (arguments[j].ArgumentType == null)
                            {
                                // method(null)，此时method参数必须是引用类型
                                var parameter = parameters[jindex];
                                if ((!parameter.Type.IsConstructed || parameter.Type.DefiningType != CSharpType.NULLABLE) && parameter.Type.IsValueType)
                                    return EMatchMember.NOT_MATCH;
                                arguments[j].Type = parameter.Type;
                            }
                            else
                                arguments[j].Type = arguments[j].ArgumentType;
                        }
                    }

                    CSharpType ptype = null;
                    CSharpType paramsType = null;
                    // 决定参数进行匹配 P.S.参数只有一个时有最佳匹配，大于一个时有普通匹配和params匹配
                    for (int j = 0; j < arguments.Length; j++)
                    {
                        var argument = arguments[j];
                        if (j + pindex < parameters.Count)
                        {
                            var parameter = parameters[j + pindex];
                            if (parameter.IsRef != argument.IsRef)
                                return EMatchMember.NOT_MATCH;
                            if (parameter.IsOut != argument.IsOut)
                                return EMatchMember.NOT_MATCH;
                            ptype = parameter.Type;
                            if (parameter.IsParams)
                            {
                                if (ptype.Equals(argument.Type))
                                    if (arguments.Length == parameters.Count - pindex)
                                        break;
                                    else
                                        return EMatchMember.NOT_MATCH;
                                ptype = ptype.ElementType;
                                paramsType = argument.Type;
                                match = EMatchMember.PARAMS;
                            }
                        }
                        if (paramsType != null && ptype.IsTypeParameter && !(argument.Type.Is(paramsType) || paramsType.Is(argument.Type)))
                        {
                            match = EMatchMember.NOT_MATCH;
                            break;
                        }
                        if (ptype.Equals(argument.Type) ||
                            // byte参数的方法，直接写0会认为是byte，可是在解析时该类型为int
                            (argument.Node is PrimitiveValue && MatchNumericType(argument.Type, ptype)))
                            continue;
                        if (ptype.IsTypeParameter)
                        {
                            // 泛型参数及其约束
                            if (ptype.HasValueTypeConstraint && !ptype.IsValueType)
                                return EMatchMember.NOT_MATCH;
                            // 接口也算ReferenceType
                            if (ptype.HasReferenceTypeConstraint && ptype.IsValueType)
                                return EMatchMember.NOT_MATCH;
                            if (ptype.HasConstructorConstraint)
                            {
                                if (argument.Type.IsInterface)
                                    return EMatchMember.NOT_MATCH;
                                if (argument.Type.IsClass)
                                {
                                    var constructors = ptype.Members.Where(m => m.IsConstructor && !m.IsStatic).ToList();
                                    if (constructors.Count > 0 && !constructors.Any(c => c.IsPublic && c.TypeArguments.Count == 0))
                                        return EMatchMember.NOT_MATCH;
                                }
                                // enum | struct都默认包含new()
                            }
                            var constraints = ptype.TypeConstraints;
                            if (!constraints.All(c => ptype.Is(c)))
                                return EMatchMember.NOT_MATCH;
                            match = EMatchMember.GENERIC;
                        }
                        //else if (MatchedTargetType(argument.Type, ptype) || argument.Type.Is(ptype))
                        else if (MatchedTargetType(argument.Type, ptype))
                        {
                            if (match > EMatchMember.PTYPE)
                                match = EMatchMember.PTYPE;
                        }
                        else
                            return EMatchMember.NOT_MATCH;
                    }

                    for (int j = 0; j < arguments.Length; j++)
                    {
                        if (arguments[j].MatchedDelegateMethod != null)
                        {
                            AddRef(arguments[j].Node, arguments[j].MatchedDelegateMethod, ((ReferenceMember)arguments[j].Node).Name);
                            arguments[j].MatchedDelegateMethod = null;
                        }
                        if (arguments[j].IsLambda)
                        {
                            // 确定lambda返回类型
                            arguments[j].MatchedLambdaType = arguments[j].TempLambdaType;
                            AddRef(arguments[j].Node, arguments[j].MatchedLambdaType, null);
                            //arguments[j].Type = null;
                        }
                        else if (arguments[j].ArgumentType == null)
                        {
                            // null参数引用方法参数的引用类型
                            AddRef(arguments[j].Node, arguments[j].Type, null);
                            //arguments[j].Type = null;
                        }
                    }
                    return match;
                }
                void UnresolveMethodArgumentLambdaType()
                {
                    for (int j = 0; j < methodArguments.Length; j++)
                    {
                        if (methodArguments[j].IsLambda)
                        {
                            // 确定lambda返回类型
                            methodArguments[j].Type = null;
                        }
                        else if (methodArguments[j].ArgumentType == null)
                        {
                            // null参数引用方法参数的引用类型
                            methodArguments[j].Type = null;
                        }
                    }
                }
                bool ResolveMemberType(IList<CSharpMember> members, IList<CSharpType> generics, out CSharpType type, ReferenceMember node)
                {
                    CSharpMember member;
                    return ResolveMemberType(members, generics, out member, out type, node);
                }
                bool ResolveMemberType(IList<CSharpMember> members, IList<CSharpType> generics, out CSharpMember member, out CSharpType type, ReferenceMember node)
                {
                    member = ResolveMember(members, generics);
                    type = null;
                    if (member == null)
                        return false;
                    if (member == EmptyMember)
                    {
                        // event += Method
                        if (functions == null)
                            // BUG: return Method
                            throw new InvalidOperationException();
                        functions.AddRange(members);
                        return true;
                    }
                    AddRef(node, member, node.Name);
                    type = member.ReturnType;
                    matchedMember = member;
                    return true;
                }

                public override void Visit(ReturnStatement node)
                {
                    if (node.Value != null)
                    {
                        if (Builder.DefiningMember.ReturnType.IsDelegate && node.Value is ReferenceMember)
                        {
                            // 可能return委托方法
                            List<CSharpMember> members = new List<CSharpMember>();
                            _functions.Push(members);
                            Type = Calc(node.Value);
                            if (Type == null)
                                Type = Builder.DefiningMember.ReturnType;
                            _functions.Pop();
                        }
                        else
                        {
                            Type = Calc(node.Value);
                        }
                    }
                }
                public override void Visit(ReferenceMember node)
                {
                    // 1. 局部变量
                    // 2. 内部成员
                    // 3. 外部类静态成员
                    // 4. 类型
                    MemberFinder finder = new MemberFinder(this);
                    CSharpType definingType = Builder.DefiningType;
                    CSharpMember definingMember = Builder.DefiningMember;

                    Stack<ReferenceMember> parents = node.References;
                    if (parents.Peek().Target != null)
                    {
                        // new Method().Member
                        Type = _Calc<MethodInvoker>(parents.Peek().Target);
                        //parents.Pop();
                    }
                    else
                    {
                        if (isConstructor)
                        {
                            int refFlag = parents.Count;
                            Type = Builder.FindType(node, null, out parents);
                            //var member = Type.MemberDefinitions.FirstOrDefault(m => m.IsConstructor && m.Parameters.Count == methodArguments.Length);
                            var members = Type.MemberDefinitions.Where(m => m.IsConstructor && finder.CanVisitMember(m)).ToList();
                            CSharpType tempType;
                            if (members.Count > 0 && ResolveMemberType(members, CSharpType.EmptyList, out tempType, parents.Count == 0 ? node : (ReferenceMember)parents.Peek().Target))
                            {
                                // 引用了显示声明的构造函数
                                // 也引用其类型防止类型被优化掉
                                AddRef(new ReferenceMember(node.Name), Type, null);
                            }
                            else
                            {
                                if (methodArguments != null)
                                {
                                    if (lambdaType.IsDelegate && methodArguments[0].IsUnresolvedDelegateMethod)
                                    {
                                        // 委托类型默认自带的构造函数
                                        if (!methodArguments[0].ResolveDelegateMethod(lambdaType))
                                            throw new InvalidCastException();
                                        AddRef(methodArguments[0].Node, methodArguments[0].MatchedDelegateMethod, ((ReferenceMember)methodArguments[0].Node).Name);
                                    }
                                    else if (methodArguments.Length > 0)
                                        throw new InvalidCastException("未能正确找到构造函数");
                                }
                                // 如果类型没有默认显示声明的构造函数，则语法节点引用其类型
                                AddRef(node, Type, node.Name);
                            }
                            // FindType导致a.b中的a没有引用，在这里补上
                            if (refFlag != parents.Count)
                                FindTypeAndRef(node);
                        }
                        // PSRandom有成员_RANDOM.Random Random，Update方法中调用的Random.Next，这里导致只剩下Next，Type被设置成了System.Random
                        //else
                        //    Builder.Ref(() => Type = Builder.FindType(node, null, out parents));
                    }

                    // new Type() { [A] = value }, A是Type的成员
                    if (Type == null && isConstructor && lambdaType != null)
                    {
                        Type = lambdaType;
                    }


                    //ReferenceMember typeNameName = null;
                    //IList<CSharpType> typeNameTypeArguments = null;
                    //List<CSharpMember> typeNameMembers = null;
                    while (parents.Count > 0)
                    {
                        var current = parents.Pop();
                        // todo: test
                        if (current.Name.Name == "Sqrt" && definingMember != null && definingMember.Name.Name == "IsPrime")
                        {
                            CSharpMember member2 = definingType.Members.FirstOrDefault(f => f.Name.Name == current.Name.Name);
                        }

                        // 泛型实参
                        IList<CSharpType> typeArguments = null;
                        if (current.IsGeneric)
                        {
                            CSharpType[] arguments = new CSharpType[current.GenericTypes.Count];
                            for (int i = 0; i < arguments.Length; i++)
                                if (current.GenericTypes[i] != null)
                                    arguments[i] = FindTypeAndRef(current.GenericTypes[i]);
                            typeArguments = arguments;
                        }

                        string cname = current.Name.Name;
                        if (Type == null)
                        {
                            if (cname == "this")
                            {
                                // 构造函数调用的构造函数this()
                                if (parents.Count == 0 && Builder.invokeConstructor && definingMember != null && definingMember.IsConstructor)
                                {
                                    cname = definingType.Name.Name;
                                }
                                else
                                {
                                    //finder.Find = MemberFinder.EFind.Instance;
                                    if (definingType.TypeParametersCount > 0)
                                    {
                                        // 泛型类内部直接指向由声明的泛型构造的泛型实例类型
                                        Type = CSharpType.CreateConstructedType(definingType, definingType.TypeParameters);
                                    }
                                    else
                                    {
                                        Type = definingType;
                                    }
                                    // 索引器？
                                    AddRef(current, Type, null);
                                    continue;
                                }
                            }

                            if (cname == "base")
                            {
                                // 构造函数调用的构造函数this()
                                if (parents.Count == 0 && definingMember != null && definingMember.IsConstructor)
                                {
                                    //cname = definingType.Name.Name;
                                    cname = definingType.BaseClass.Name.Name;
                                }
                                else
                                {
                                    //finder.Find = MemberFinder.EFind.Instance;
                                    Type = definingType.BaseClass;
                                    AddRef(current, Type, null);
                                    continue;
                                }
                            }

                            VAR _variable = FindVariableAndRef(current);
                            if (_variable != null)
                            {
                                // 委托类型的变量当做方法被调用
                                //if (parents.Count == 0 && _variable.Type.IsDelegate && methodArguments != null)
                                //{
                                //    SingleMember[0] = _variable.Type.DelegateInvokeMethod;
                                //    if (!ResolveMemberType(SingleMember, typeArguments, out Type, current))
                                //        throw new InvalidCastException();
                                //}
                                // 自定义索引器
                                //else if (parents.Count == 0 && methodArguments != null)
                                //{
                                //    var members = _variable.Type.Members.Where(m => m.Name.Name == "this" && finder.CanVisitMember(m)).ToList();
                                //    if (members.Count > 0)
                                //    {
                                //        if (typeArguments != null)
                                //            throw new InvalidCastException();
                                //        if (ResolveMemberType(members, typeArguments, out Type, node))
                                //            continue;
                                //        throw new InvalidCastException();
                                //    }
                                //}
                                //else
                                Type = _variable.Type;
                                continue;
                            }

                            // 连续.表达式，只有最后一个才是方法，否则是字段，属性或类型
                            bool findMethodFlag = !HasMethodArgument || parents.Count == 0;
                            //if (!HasMethodArgument || parents.Count == 0)
                            {
                                // HACK: (暂不解决，修改成员名称不与类名冲突即可)成员和类型名相同时，两种都有可能，不过作为返回类型一定是一致的；引用类型变成引用成员，可能导致生成的代码有错误
                                // BUG: 类型成员名称和命名空间重名，例如Stub类的Protocol
                                var members = definingMember.ContainingType.Members.Where(m => m.Name.Name == cname && (findMethodFlag || (m.IsField || m.IsConstant || m.IsProperty)) && finder.CanVisitMember(m)).ToList();
                                if (members.Count > 0)
                                {
                                    //if (parents.Count > 0 && members.Count == 1 && members[0].IsField || members[0].IsProperty || members[0].IsConstant)
                                    //{
                                    //    CSharpType sameNameType = Builder.FindType(current);
                                    //    if (sameNameType != null)
                                    //    {
                                    //        typeNameName = current;
                                    //        typeNameTypeArguments = typeArguments;
                                    //        typeNameMembers = members;
                                    //        Type = sameNameType;
                                    //        continue;
                                    //    }
                                    //}
                                    if (ResolveMemberType(members, typeArguments, out Type, current))
                                        continue;
                                }

                                // 外部类静态方法
                                var outter = definingMember.ContainingType.ContainingType;
                                bool flag = false;
                                while (outter != null)
                                {
                                    members = outter.Members.Where(m => (m.IsStatic || m.IsConstant) && m.Name.Name == cname && finder.CanVisitMember(m)).ToList();
                                    if (members.Count > 0)
                                    {
                                        if (ResolveMemberType(members, typeArguments, out Type, current))
                                        {
                                            flag = true;
                                            break;
                                        }
                                    }
                                    outter = outter.ContainingType;
                                }
                                // 找到外部类方法
                                if (flag)
                                    continue;
                            }

                            Builder.Ref(() => Type = Builder.FindType(node, null, out parents));
                            if (Type == null)
                                throw new InvalidOperationException();
                            continue;
                        }
                        else
                        {
                            var members = Type.Members.Where(m => m.Name.Name == cname && finder.CanVisitMember(m)).ToList();
                            if (members.Count > 0)
                            {
                                CSharpType temp;
                                CSharpMember member;
                                if (ResolveMemberType(members, typeArguments, out member, out temp, current))
                                {
                                    // 解决同名成员和类型
                                    //if (typeNameName != null)
                                    //{
                                    //    if (member.IsStatic)
                                    //    {
                                    //        // 之前的是类型
                                    //        FindTypeAndRef(typeNameName);
                                    //    }
                                    //    else
                                    //    {
                                    //        // 之前的是成员
                                    //        if (!ResolveMemberType(typeNameMembers, typeNameTypeArguments, out Type, typeNameName))
                                    //            throw new InvalidCastException();
                                    //    }
                                    //    typeNameName = null;
                                    //}
                                    // 放置把原有的Type冲掉，后续还要执行扩展方法
                                    Type = temp;
                                    continue;
                                }
                            }
                            else
                            {
                                // 默认无参构造函数
                                if (isConstructor)
                                {
                                    AddRef(current, Type, current.Name);
                                    continue;
                                }
                            }

                            // 扩展方法
                            members = Builder.AssemblyAccessableTypes.Where(t => t.IsStatic).SelectMany(t => t.Members.Where(m => m.Name.Name == cname && finder.CanVisitMember(m))).ToList();
                            if (members.Count > 0)
                            {
                                CSharpMember exMember;
                                if (ResolveMemberType(members, typeArguments, out exMember, out Type, current))
                                {
                                    // 引用其类型防止类型被优化掉
                                    AddRef(new ReferenceMember(current.Name), exMember.ContainingType, null);
                                    continue;
                                }
                            }
                        }

                        throw new InvalidOperationException();
                    }
                }
                public override void Visit(InvokeMethod node)
                {
                    if (!this.isConstructor)
                        this.lambdaType = null;

                    int count = node.Arguments == null ? 0 : node.Arguments.Count;
                    if (count == 0)
                    {
                        methodArguments = EmptyList;
                    }
                    else
                    {
                        var tempLambdaType = this.lambdaType;
                        this.lambdaType = null;
                        methodArguments = new Argument[count];
                        for (int i = 0; i < count; i++)
                        {
                            var arg = new Argument();
                            arg.Direction = node.Arguments[i].Direction;
                            arg.Node = node.Arguments[i].Expression;
                            // 保存方法直接作为委托类型参数情况下的所有成员
                            arg.Functions = new List<CSharpMember>();
                            _functions.Push(arg.Functions);
                            // Primitive.null时类型为null，只能确定为引用类型
                            arg.ArgumentType = Calc(node.Arguments[i].Expression);
                            _functions.Pop();
                            arg.Type = arg.ArgumentType;
                            methodArguments[i] = arg;
                        }
                        this.lambdaType = tempLambdaType;
                    }

                    if (node.Target is New)
                    {
                        Type = _Calc<MethodInvoker>(node.Target);
                    }
                    else if (node.Target is InvokeMethod)
                    {
                        Type = _Calc<MethodInvoker>(node.Target);
                    }
                    else if (node.Target is ReferenceMember)
                    {
                        Visit(node.Target);
                    }
                    else
                    {
                        Type = _Calc<MethodInvoker>(node.Target);
                    }

                    if (node.IsIndexer)
                    {
                        if (Type.IsArray || Type == CSharpType.ARRAY)
                        {
                            if (count == 0)
                            {
                                // new int[3][]
                                if (!node.TargetIsNewArray)
                                    throw new NotImplementedException();
                                Type = CSharpType.CreateArray(1, Type);
                                AddRef(node, Type, null);
                            }
                            else
                            {
                                AddRef(node, Type, null);
                                // array[index]: 返回类型为数组子类型
                                if (Type.IsArray)
                                    Type = Type.ElementType;
                                else
                                    Type = CSharpType.OBJECT;
                            }
                        }
                        else
                        {
                            // 类型自定义索引器
                            MemberFinder finder = new MemberFinder(this);
                            var indices = Type.Members.Where(m => m.Name.Name == "this" && finder.CanVisitMember(m)).ToList();
                            CSharpMember indexer = ResolveMember(indices, null);
                            AddRef(node, indexer, null);
                            Type = indexer.ReturnType;
                        }
                    }
                    else
                    {
                        REF tref;
                        if (syntaxReferences.TryGetValue(node.Target, out tref))
                        {
                            CSharpMember member = tref.Definition.Define as CSharpMember;
                            if (member != null && member.IsMethod)
                            {
                                // 由于调用的方法可能是MemberWithTypeArgument，所以这里引用一下其定义防止优化时被删掉，也防止Name在改名时被忽略
                                while (member.DefiningMember != null)
                                    member = member.DefiningMember;
                                AddRef(node, member, ((ReferenceMember)node.Target).Name);
                            }
                            else
                            {
                                if (Type != null && Type.IsDelegate
                                    && ((member != null && !member.IsMethod) || tref.Definition.Define is VAR)
                                    && !isConstructor)
                                {
                                    //if (isConstructor)
                                    //{
                                    //    // Action<int> += Method; 决定Method的引用
                                    //    for (int i = 0; i < count; i++)
                                    //    {
                                    //        if (methodArguments[i].IsUnresolvedDelegateMethod)
                                    //        {
                                    //            AddRef(node.Arguments[i], methodArguments[i].Functions[0], ((ReferenceMember)node.Arguments[i].Expression).Name);
                                    //        }
                                    //    }
                                    //}
                                    //else
                                    //{

                                    //}

                                    // 委托类型的变量或成员
                                    SingleMember[0] = Type.DelegateInvokeMethod;
                                    CSharpMember invoke = ResolveMember(SingleMember, null);
                                    //AddRef(node, invoke, null);
                                    AddRef(node, Type, null);
                                    Type = invoke.ReturnType;
                                }
                                else
                                {
                                    AddRef(node, tref.Definition.Define, null);
                                }
                            }
                        }
                        else
                        {
                            if (Type != null && Type.IsDelegate
                                && node.Target is ReferenceMember && (matchedMember == null || !matchedMember.IsMethod))
                            {
                                // 委托类型的变量或成员
                                SingleMember[0] = Type.DelegateInvokeMethod;
                                CSharpMember invoke = ResolveMember(SingleMember, null);
                                Type = invoke.ReturnType;
                            }

                            // 可能是在isTest里面
                            if (!isTest)
                            {
                                CSharpMember m2 = Builder.DefiningMember;
                                CSharpType t2 = Builder.DefiningType;
                                //throw new InvalidCastException();
                            }
                            else
                            {
                            }
                        }

                        //if (node.TargetIsNewDelegate)
                        //{
                        //    // 一定是new了个委托类型来调用
                        //    ResolveMember(Type.Members.Where(m => m.Name == INVOKE).ToList(), null);
                        //}
                        //// else: ReferenceMember
                        //else
                        //{

                        //}
                    }
                }
                public override void Visit(New node)
                {
                    if (node.IsNewArray)
                    {
                        // 调用数组构造函数new int[]
                        Type = FindTypeAndRef(node.Type);
                        Type = CSharpType.CreateArray(1, Type);
                        AddRef(node, CSharpType.ARRAY, null);
                        // 参数引用
                        if (node.Method.Arguments != null)
                            foreach (var item in node.Method.Arguments)
                                Calc(item.Expression);
                        foreach (var item in node.Initializer) Calc(item);
                    }
                    else
                    {
                        Type = Builder.FindType(node.Type);
                        lambdaType = Type;
                        this.isConstructor = true;
                        _Calc<MethodInvoker>(node.Method);
                        this.isConstructor = false;
                        REF _ref;
                        if (syntaxReferences.TryGetValue(node.Method, out _ref))
                            AddRef(node, _ref.Definition.Define, null);
                        // new Class() { a = 5 }，a是Class的变量，Type应该默认设置为Class
                        foreach (var item in node.Initializer)
                        {
                            BinaryOperator assign = item as BinaryOperator;
                            if (assign != null)
                            {
                                _Calc<MethodInvoker>(assign.Left);
                                Calc(assign.Right);
                            }
                            else
                            {
                                //Calc(item);
                                throw new NotImplementedException();
                            }
                        }
                        lambdaType = null;
                    }
                }
            }

            private CSharpType Calc(SyntaxNode node)
            {
                return _Calc<_CalcExpressionType>(node);
            }
            /// <summary>由_CalcBody内部自调用，调用时继承变量作用域树</summary>
            private CSharpType _Calc<T>(SyntaxNode node) where T : _CalcExpressionType, new()
            {
                T _calc = new T();
                CopyTo(_calc);
                _calc.Visit(node);
                return _calc.Type;
            }
            protected virtual void CopyTo(_CalcExpressionType _calc)
            {
                _calc.Builder = this.Builder;
                _calc.tree = this.tree;
                _calc.lambdaType = this.lambdaType;
                _calc.isTest = this.isTest;
            }
            protected static CSharpType ImplicitTypeChangeResult(CSharpType t1, CSharpType t2)
            {
                if (t1 == null && t2 == null)
                    return null;
                if (t1 == null && t2 != null)
                    return t2;
                if (t2 == null && t1 != null)
                    return t1;
                if (t1.Equals(t2))
                    return t2;

                if (t1.Is(t2))
                    return t2;
                if (t2.Is(t1))
                    return t1;

                if (SupportImplicitTypeChange(t1) && SupportImplicitTypeChange(t2))
                {
                    //if (t1 == CSharpType.OBJECT || t2 == CSharpType.OBJECT)
                    //    return CSharpType.OBJECT;
                    if (t1 == CSharpType.STRING || t2 == CSharpType.STRING)
                        return CSharpType.STRING;
                    if (t1 == CSharpType.DOUBLE || t2 == CSharpType.DOUBLE)
                        return CSharpType.DOUBLE;
                    if (t1 == CSharpType.FLOAT || t2 == CSharpType.FLOAT)
                        return CSharpType.FLOAT;
                    if (t1 == CSharpType.ULONG || t2 == CSharpType.ULONG)
                        return CSharpType.ULONG;
                    if (t1 == CSharpType.LONG || t2 == CSharpType.LONG)
                        return CSharpType.LONG;
                    if (t1 == CSharpType.UINT || t2 == CSharpType.UINT)
                        return CSharpType.UINT;
                    if (t1 == CSharpType.INT || t2 == CSharpType.INT)
                        return CSharpType.INT;
                    if (t1 == CSharpType.CHAR || t2 == CSharpType.CHAR)
                        return CSharpType.CHAR;
                    if (t1 == CSharpType.USHORT || t2 == CSharpType.USHORT)
                        return CSharpType.USHORT;
                    if (t1 == CSharpType.SHORT || t2 == CSharpType.SHORT)
                        return CSharpType.SHORT;
                    if (t1 == CSharpType.BYTE || t2 == CSharpType.BYTE)
                        return CSharpType.BYTE;
                    if (t1 == CSharpType.SBYTE || t2 == CSharpType.SBYTE)
                        return CSharpType.SBYTE;
                }

                //throw new NotImplementedException();
                return null;
            }
            public static bool SupportImplicitTypeChange(CSharpType type)
            {
                return type == CSharpType.STRING
                    || type == CSharpType.DOUBLE
                    || type == CSharpType.FLOAT
                    || type == CSharpType.ULONG
                    || type == CSharpType.LONG
                    || type == CSharpType.UINT
                    || type == CSharpType.INT
                    || type == CSharpType.CHAR
                    || type == CSharpType.USHORT
                    || type == CSharpType.SHORT
                    || type == CSharpType.BYTE
                    || type == CSharpType.SBYTE;
            }
            protected CSharpType FindTypeAndRef(ReferenceMember node)
            {
                if (isTest)
                    return Builder.FindType(node);
                return Builder.FindTypeAndRef(node);
            }
            protected void AddRef(SyntaxNode refSyntax, object beRefObj, Named reference)
            {
                if (isTest)
                    return;
                Builder.AddRef(refSyntax, beRefObj, reference, null);
            }
            internal void ParseLambdaType(CSharpType type, Action action)
            {
                // 可以确定LambdaType的表达式
                // FieldLocal BinaryOperator(Assign) New ReturnStatement InvokeMethod
                if (type == null || !type.IsDelegate)
                {
                    action();
                }
                else
                {
                    var temp = this.lambdaType;
                    this.lambdaType = type;
                    action();
                    this.lambdaType = temp;
                }
            }
            /// <summary>此方法被引用的地方就是可能从外层被调用的地方，默认加上DefiningMember.Parameters</summary>
            public static CSharpType Calc(_BuildReference builder, SyntaxNode node, List<FormalArgument> arguments)
            {
                if (node == null)
                    return null;
                _CalcExpressionType _calc = new _CalcExpressionType();
                _calc.Builder = builder;
                _calc.tree = new BodyTree();
                if (arguments != null)
                {
                    REF arg;
                    foreach (var item in arguments)
                        // indexer时get和set使用同一个参数实例
                        if (syntaxReferences.TryGetValue(item, out arg))
                        {
                            _calc.AddRef(item, arg.Definition.Define, item.Name);
                            _calc.tree.Add(new BodyTree((VAR)arg.Definition.Define));
                        }
                        else
                            _calc.NewVariableAndRef(item, builder.FindTypeAndRef(item.Type), item.Name);
                }
                // 正在定义字段，可能访问的是node.Value
                if (builder.DefiningMember != null && builder.DefiningMember.IsField)
                    _calc.lambdaType = builder.DefiningMember.ReturnType;
                _calc.Visit(node);
                _calc.lambdaType = null;
                return _calc.Type;
            }

            void PushScope()
            {
                BodyTree newTree = new BodyTree();
                this.tree.Add(newTree);
                this.tree = newTree;
            }
            void PopScope()
            {
                this.tree = tree.Parent;
                if (this.tree == null)
                    throw new InvalidOperationException("作用域栈已经到底");
            }
            VAR NewVariableAndRef(SyntaxNode node, CSharpType type, Named name)
            {
                VAR variable = new VAR();
                variable.DeclaringMember = Builder.DefiningMember;
                variable.Name = name;
                variable.Type = type;
                if (!isTest)
                    Builder.AddRef(node, variable, name, null);
                tree.Add(new BodyTree(variable));
                return variable;
            }
            VAR FindVariableAndRef(ReferenceMember node)
            {
                // 向上一层循环走之后，只循环到自己为止，自己以下的兄弟节点不循环
                BodyTree pscope = null;
                BodyTree temp = this.tree;
                while (temp != null)
                {
                    int index = temp.IndexOf(pscope);
                    if (index == -1)
                        index = temp.ChildCount;
                    index--;
                    // 倒查最近的一个变量
                    for (int i = index; i >= 0; i--)
                    {
                        var item = temp[i];
                        if (item.Variable == null)
                            continue;
                        if (item.Variable.Name.Name == node.Name.Name)
                        {
                            AddRef(node, item.Variable, node.Name);
                            return item.Variable;
                        }
                    }
                    pscope = temp;
                    temp = temp.Parent;
                }
                return null;
            }

            public override void Visit(SwitchStatement node)
            {
                PushScope();
                base.Visit(node);
                PopScope();
            }
            public override void Visit(Body node)
            {
                if (node != null)
                {
                    PushScope();
                    foreach (var item in node.Statements) InternalVisitBodyStatement(item);
                    PopScope();
                }
            }
            protected virtual void InternalVisitBodyStatement(SyntaxNode node)
            {
                //Calc(node);
                Visit(node);
            }
            public override void Visit(Accessor node)
            {
                PushScope();
                if (node.AccessorType != EAccessor.GET)
                {
                    var value = new ReferenceMember(new Named("value"));
                    NewVariableAndRef(value, Builder.DefiningMember.ReturnType, value.Name);
                }
                base.Visit(node);
                PopScope();
            }
            public override void Visit(ForStatement node)
            {
                PushScope();
                base.Visit(node);
                PopScope();
            }
            public override void Visit(ForeachStatement node)
            {
                PushScope();
                CSharpType enumerable = Calc(node.In);
                CSharpType elementType;
                //if (enumerable.IsArray)
                //    elementType = enumerable.ElementType;
                //else
                {
                    CSharpType enumerableType;
                    if (!enumerable.IsType(CSharpType.IENUMERABLE, out enumerableType))
                    {
                        // HACK: 针对IEnumerable的应急措施
                        elementType = CSharpType.OBJECT;
                        enumerableType = enumerable;
                    }
                    else
                    {
                        elementType = enumerableType.TypeArguments[0];
                    }
                    // node.In除了引用对象或类型外，还应该引用其GetEnumerator函数
                    var getEnumerator = enumerableType.Members.FirstOrDefault(m => m.Name.Name == _BuildMember.GET_ENUMERATOR.Name);
                    if (getEnumerator == null)
                    {
                        throw new InvalidCastException();
                    }
                    AddRef(new ReferenceMember(_BuildMember.GET_ENUMERATOR), getEnumerator, getEnumerator.Name);
                }
                if (node.Type.Name.Name == "var")
                {
                    NewVariableAndRef(node, elementType, node.Name);
                    AddRef(node.Type, elementType, node.Type.Name);
                }
                else
                {
                    NewVariableAndRef(node, FindTypeAndRef(node.Type), node.Name);
                }
                Visit(node.Body);
                PopScope();
            }
            public override void Visit(FieldLocal node)
            {
                CSharpType type;
                if (node.IsAutoType)
                {
                    // var: 以表达式值的类型作为其类型
                    type = Calc(node.Value);
                    AddRef(node.Type, type, node.Type.Name);
                }
                else
                {
                    type = FindTypeAndRef(node.Type);
                    if (node.Value != null)
                    {
                        if (node.Value is Lambda)
                        {
                            if (!type.IsDelegate)
                                throw new InvalidCastException();
                            // 针对可能是Lambda做特殊处理
                            lambdaType = type;
                        }
                        Calc(node.Value);
                        lambdaType = null;
                        //Visit(node.Value);
                    }
                }
                NewVariableAndRef(node, type, node.Name);

                foreach (var item in node.Multiple)
                {
                    NewVariableAndRef(item, type, item.Name);
                    if (item.Value != null)
                    {
                        if (item.Value is Lambda)
                        {
                            if (!type.IsDelegate)
                                throw new InvalidCastException();
                            // 针对可能是Lambda做特殊处理
                            lambdaType = type;
                        }
                        Calc(item.Value);
                        lambdaType = null;
                    }
                }
            }

            public override void Visit(List<CatchStatement> node)
            {
                foreach (var item in node)
                {
                    if (item.HasArgument)
                    {
                        CSharpType t = FindTypeAndRef(item.Type);
                        if (item.HasName)
                        {
                            NewVariableAndRef(item, t, item.Name);
                        }
                    }
                    Visit(item.Body);
                }
            }

            public override void Visit(ReturnStatement node)
            {
                if (node.Value is Lambda)
                {
                    // Return可确定Lambda返回类型
                    ParseLambdaType(lambdaType == null ? Builder.DefiningMember.ReturnType : lambdaType.DelegateInvokeMethod.ReturnType, () => base.Visit(node));
                    //var temp = this.lambdaType;
                    //if (temp == null)
                    //    lambdaType = Builder.DefiningMember.ReturnType;
                    //else
                    //    lambdaType = temp.DelegateInvokeMethod.ReturnType;
                    //base.Visit(node);
                    //lambdaType = temp;
                }
                else if (node.Value is ReferenceMember)
                {
                    // 可能return委托方法
                    Type = _Calc<MethodInvoker>(node);
                }
                else
                {
                    base.Visit(node);
                }
            }
            public override void Visit(UnaryOperator node)
            {
                Type = Calc(node.Expression);

                Named name = GetUnaryOperator(node.Operator);
                if (name != null)
                {
                    string name1 = name.Name;
                    string name2 = CodeBuilder.UOP[node.Operator];
                    // 可能是重写的运算符
                    var operators = Type.Members.Where(m => m.IsOperator && (m.Name.Name == name1 || m.Name.Name == name2)).ToList();
                    if (operators.Count > 0)
                    {
                        CSharpMember op = operators.FirstOrDefault(m =>
                        {
                            var parameters = m.Parameters;
                            if (parameters.Count == 1)
                                return parameters[0].Type == Type;
                            return false;
                        });
                        if (op != null)
                        {
                            AddRef(node, op, null);
                            Type = op.ReturnType;
                            return;
                        }
                    }
                }

                // 基础类型默认运算符 & 自动转换类型
                if (node.Operator == EUnaryOperator.Plus || node.Operator == EUnaryOperator.Minus)
                {
                    if (Type == CSharpType.BYTE
                        || Type == CSharpType.SBYTE
                        || Type == CSharpType.SHORT
                        || Type == CSharpType.USHORT
                        || Type == CSharpType.CHAR)
                    {
                        Type = CSharpType.INT;
                    }
                    else if (Type == CSharpType.UINT)
                    {
                        if (node.Operator == EUnaryOperator.Minus)
                        {
                            Type = CSharpType.LONG;
                        }
                    }
                }
                if (Type != null) AddRef(node, Type, null);
            }
            public override void Visit(BinaryOperator node)
            {
                CSharpType left = Calc(node.Left);
                List<CSharpMember> members = null;
                if (left != null && left.IsDelegate 
                    // 委托可能是调用判断，例如action == null
                    && (node.Operator >= EBinaryOperator.Assign))
                {
                    members = new List<CSharpMember>();
                    _functions.Push(members);
                }
                CSharpType right = null;
                ParseLambdaType(left, () => right = Calc(node.Right));
                if (members != null)
                {
                    right = left;
                    _functions.Pop();
                }

                if (left == null && right == null)
                    throw new NotImplementedException();
                if (left != null)
                {
                    //AddRef(node, left, null);
                    if (left.IsConstructed && left.DefiningType == CSharpType.NULLABLE)
                        left = left.TypeArguments[0];
                }
                if (right != null)
                {
                    //AddRef(node, right, null);
                    if (right.IsConstructed && right.DefiningType == CSharpType.NULLABLE)
                        right = right.TypeArguments[0];
                }

                EBinaryOperator o = node.Operator;

                //if (o == EBinaryOperator.Assign)
                //{
                //    Type = null;
                //    return;
                //}
                //else if (o > EBinaryOperator.Assign)
                //{
                //    o -= 100;
                //}
                switch (o)
                {
                    case EBinaryOperator.Assign: if (members == null) { Type = null; return; } break;
                    case EBinaryOperator.AssignAdd: o = EBinaryOperator.Addition; break;
                    case EBinaryOperator.AssignSubtract: o = EBinaryOperator.Subtraction; break;
                    case EBinaryOperator.AssignMultiply: o = EBinaryOperator.Multiply; break;
                    case EBinaryOperator.AssignDivide: o = EBinaryOperator.Division; break;
                    case EBinaryOperator.AssignModulus: o = EBinaryOperator.Modulus; break;
                    case EBinaryOperator.AssignShiftLeft: o = EBinaryOperator.ShiftLeft; break;
                    case EBinaryOperator.AssignShiftRight: o = EBinaryOperator.ShiftRight; break;
                    case EBinaryOperator.AssignBitwiseAnd: o = EBinaryOperator.BitwiseAnd; break;
                    case EBinaryOperator.AssignBitwiseOr: o = EBinaryOperator.BitwiseOr; break;
                    case EBinaryOperator.AssignExclusiveOr: o = EBinaryOperator.ExclusiveOr; break;
                }

                Named name = GetBinaryOperator(o);
                if (name != null)
                {
                    string name1 = name.Name;
                    string name2 = CodeBuilder.BOP[o];

                    CSharpMember op = null;
                    List<CSharpMember> operators;
                    if (left != null)
                    {
                        operators = left.Members.Where(m => m.IsOperator && (m.Name.Name == name1 || m.Name.Name == name2)).ToList();
                        if (operators.Count > 0)
                        {
                            op = operators.FirstOrDefault(m =>
                            {
                                var parameters = m.Parameters;
                                if (parameters.Count == 2)
                                    return left.Is(parameters[0].Type) && 
                                        ((right == null && !parameters[1].Type.IsValueType) ||
                                        (right != null && ((MatchNumericType(right, parameters[1].Type) && MatchedTargetType(right, parameters[1].Type)) || right.Is(parameters[1].Type))));
                                return false;
                            });
                        }
                    }
                    if (right != null)
                    {
                        if (op == null)
                        {
                            operators = right.Members.Where(m => m.IsOperator && (m.Name.Name == name1 || m.Name.Name == name2)).ToList();
                            if (operators.Count > 0)
                            {
                                op = operators.FirstOrDefault(m =>
                                {
                                    var parameters = m.Parameters;
                                    if (parameters.Count == 2)
                                        return ((left == null && !parameters[0].Type.IsValueType) ||
                                            (left != null && ((MatchNumericType(left, parameters[0].Type) && MatchedTargetType(left, parameters[0].Type)) || left.Is(parameters[0].Type)))
                                            && right.Is(parameters[1].Type));
                                    return false;
                                });
                            }
                        }
                    }
                    if (op != null)
                    {
                        AddRef(node, op, null);
                        Type = op.ReturnType;
                        return;
                    }
                }

                if (members != null) return;

                switch (o)
                {
                    case EBinaryOperator.Multiply:
                    case EBinaryOperator.Division:
                    case EBinaryOperator.Modulus:
                    case EBinaryOperator.Addition:
                    case EBinaryOperator.Subtraction:
                    case EBinaryOperator.ShiftLeft:
                    case EBinaryOperator.ShiftRight:
                    case EBinaryOperator.BitwiseAnd:
                    case EBinaryOperator.ExclusiveOr:
                    case EBinaryOperator.BitwiseOr:
                        // 数学运算符
                        if (left == CSharpType.STRING || right == CSharpType.STRING)
                            Type = CSharpType.STRING;
                        else if (left == CSharpType.DOUBLE || right == CSharpType.DOUBLE)
                            Type = CSharpType.DOUBLE;
                        else if (left == CSharpType.FLOAT || right == CSharpType.FLOAT)
                            Type = CSharpType.FLOAT;
                        else if (left != null && left.IsEnum)
                            Type = left;
                        else if (right != null && right.IsEnum)
                            Type = right;
                        else if (left == CSharpType.ULONG || right == CSharpType.ULONG)
                            Type = CSharpType.ULONG;
                        else if (left == CSharpType.LONG || right == CSharpType.LONG)
                            Type = CSharpType.LONG;
                        else if (left == CSharpType.UINT || right == CSharpType.UINT)
                            Type = CSharpType.UINT;
                        else if (left == CSharpType.INT || right == CSharpType.INT
                            || left == CSharpType.USHORT || right == CSharpType.USHORT
                            || left == CSharpType.SHORT || right == CSharpType.SHORT
                            || left == CSharpType.BYTE || right == CSharpType.BYTE
                            || left == CSharpType.SBYTE || right == CSharpType.SBYTE)
                            Type = CSharpType.INT;
                        else if (left == CSharpType.CHAR || right == CSharpType.CHAR)
                            Type = CSharpType.CHAR;
                        else if (left == CSharpType.BOOL || right == CSharpType.BOOL)
                            Type = CSharpType.BOOL;
                        else
                            throw new InvalidCastException();
                        AddRef(node, Type, null);
                        return;

                    case EBinaryOperator.GreaterThan:
                    case EBinaryOperator.GreaterThanOrEqual:
                    case EBinaryOperator.LessThan:
                    case EBinaryOperator.LessThanOrEqual:
                    case EBinaryOperator.Equality:
                    case EBinaryOperator.Inequality:
                    case EBinaryOperator.ConditionalAnd:
                    case EBinaryOperator.ConditionalOr:
                        Type = CSharpType.BOOL;
                        AddRef(node, Type, null);
                        return;

                    case EBinaryOperator.NullCoalescing:
                        Type = left;
                        AddRef(node, Type, null);
                        return;

                    case EBinaryOperator.Assign:
                    case EBinaryOperator.AssignAdd:
                    case EBinaryOperator.AssignSubtract:
                    case EBinaryOperator.AssignMultiply:
                    case EBinaryOperator.AssignDivide:
                    case EBinaryOperator.AssignModulus:
                    case EBinaryOperator.AssignShiftLeft:
                    case EBinaryOperator.AssignShiftRight:
                    case EBinaryOperator.AssignBitwiseAnd:
                    case EBinaryOperator.AssignBitwiseOr:
                    case EBinaryOperator.AssignExclusiveOr:
                        Type = null;
                        return;
                }
                throw new NotImplementedException();
            }
            public override void Visit(ConditionalOperator node)
            {
                Calc(node.Condition);
                CSharpType _true = Calc(node.True);
                CSharpType _false = Calc(node.False);
                if (_true != _false)
                {
                    //CSharpType numeric = NumericResult(_true, _false);
                    //if (numeric == null)
                    //{
                    //    if (_true.Is(_false))
                    //        _true = _false;
                    //    else if (!_false.Is(_true))
                    //        throw new NotImplementedException();
                    //}
                    //else
                    //    _true = numeric;
                    _true = ImplicitTypeChangeResult(_true, _false);
                }
                Type = _true;
                if (Type != null) AddRef(node, Type, null);
            }
            public override void Visit(PrimitiveValue node)
            {
                ESystemPrimitiveType type = node.Type;
                if (type != ESystemPrimitiveType.REFERENCE)
                {
                    switch (type)
                    {
                        case ESystemPrimitiveType.REFERENCE: Type = null; break;
                        case ESystemPrimitiveType.STRING: Type = CSharpType.STRING; break;
                        case ESystemPrimitiveType.CHAR: Type = CSharpType.CHAR; break;
                        case ESystemPrimitiveType.BOOL: Type = CSharpType.BOOL; break;
                        case ESystemPrimitiveType.DECIMAL: throw new InvalidCastException();
                        case ESystemPrimitiveType.FLOAT: Type = CSharpType.FLOAT; break;
                        case ESystemPrimitiveType.DOUBLE: Type = CSharpType.DOUBLE; break;
                        case ESystemPrimitiveType.INT: Type = CSharpType.INT; break;
                        case ESystemPrimitiveType.UINT: Type = CSharpType.UINT; break;
                        case ESystemPrimitiveType.LONG: Type = CSharpType.LONG; break;
                        case ESystemPrimitiveType.ULONG: Type = CSharpType.ULONG; break;
                    }
                }
                if (Type != null) AddRef(node, Type, null);
            }
            public override void Visit(Lambda node)
            {
                // 定义委托类型字段时赋值为一个Lambda表达式
                // static int TestReturnInt(int a) { return a; }
                // Action action1 = () => TestReturnInt(5); // 仅一句TestReturnInt(5);可被当做return void处理
                // Func<float> action1 = () => TestReturnInt(5); // 仅一句TestReturnInt(5);可被当做return int处理

                // Lambda出现位置
                // 1. new Delegate([])
                // 2. d = [];
                // 3. Method([]);
                // 4. return [];

                if (lambdaType == null)
                {
                    // InvokeMethod的参数类型
                    Type = null;
                    return;
                }
                CSharpType lambdaReturnType;
                if (LambdaTester.TestLambda(this, lambdaType, node, false, out lambdaReturnType))
                {
                    Type = lambdaType;
                    //if (Type != null) AddRef(node, Type, null);
                }
            }
            public override void Visit(ReferenceMember node)
            {
                Type = _Calc<MethodInvoker>(node);
            }
            public override void Visit(TypeOf node)
            {
                FindTypeAndRef(node.Reference);
                Type = Builder.FindType(TYPE);
                AddRef(node, Type, null);
            }
            public override void Visit(SizeOf node)
            {
                FindTypeAndRef(node.Reference);
                Type = CSharpType.INT;
                AddRef(node, Type, null);
            }
            public override void Visit(DefaultValue node)
            {
                Type = FindTypeAndRef(node.Reference);
                AddRef(node, Type, null);
            }
            public override void Visit(InvokeMethod node)
            {
                Type = _Calc<MethodInvoker>(node);
            }
            public override void Visit(New node)
            {
                Type = _Calc<MethodInvoker>(node);
            }
            public override void Visit(As node)
            {
                Calc(node.Expression);
                Type = FindTypeAndRef(node.Reference);
                AddRef(node, Type, null);
            }
            public override void Visit(Is node)
            {
                Calc(node.Expression);
                FindTypeAndRef(node.Reference);
                Type = CSharpType.BOOL;
                AddRef(node, Type, null);
            }
            public override void Visit(Cast node)
            {
                var etype = Calc(node.Expression);
                var ctype = FindTypeAndRef(node.Type);
                // explicit 对于返回类型是int的，用(sbyte)也可以调用运算符
                bool isNumber = Refactor.IsNumberType(ctype);
                List<CSharpMember> operators = null;
                if (etype != null)
                    operators = etype.Members.Where(m => m.IsOperator && m.Name.Name.StartsWith(EXPLICIT.Name)).ToList();
                CSharpMember _explicit = null;
                if (operators != null && operators.Count > 0)
                {
                    _explicit = operators.FirstOrDefault(m => (isNumber && Refactor.IsNumberType(m.ReturnType)) || ctype.Is(m.ReturnType) || m.ReturnType.Is(ctype));
                    if (_explicit != null)
                        AddRef(node, _explicit, null);
                }
                if (_explicit == null)
                {
                    if (ctype == null)
                        throw new InvalidCastException();
                    operators = ctype.Members.Where(m => m.IsOperator && m.Name.Name.StartsWith(EXPLICIT.Name)).ToList();
                    if (operators.Count > 0)
                    {
                        _explicit = operators.FirstOrDefault(m => (isNumber && Refactor.IsNumberType(m.ReturnType)) || ctype.Is(m.ReturnType) || m.ReturnType.Is(ctype));
                        if (_explicit != null)
                            AddRef(node, _explicit, null);
                    }
                }
                Type = ctype;
                AddRef(node, Type, null);
            }
            public override void Visit(Parenthesized node)
            {
                Type = Calc(node.Expression);
                if (Type != null) AddRef(node, Type, null);
            }
        }

        static Stack<List<CSharpMember>> _functions = new Stack<List<CSharpMember>>();
        internal static Dictionary<object, BEREF> objectReferences = new Dictionary<object, BEREF>();
        internal static Dictionary<SyntaxNode, REF> syntaxReferences = new Dictionary<SyntaxNode, REF>();

        private bool findAndRef;
        private bool invokeConstructor;
        public void Ref(Action action)
        {
            findAndRef = true;
            action();
            findAndRef = false;
        }
        /// <param name="beRefObj">被引用的对象，可以是局部变量，成员，类型</param>
        /// <param name="reference">引用此对象时的名字引用</param>
        private void AddRef(SyntaxNode refSyntax, object beRefObj, Named reference, object refScope)
        {
            if (beRefObj == null || refSyntax == null)
                throw new InvalidOperationException();

            if (syntaxReferences.ContainsKey(refSyntax))
                return;

            // 泛型的构造对象应引用泛型的定义对象
            if (beRefObj is CSharpType)
            {
                CSharpType type = (CSharpType)beRefObj;
                while (type.IsArray || type.IsConstructed)
                {
                    if (type.IsArray)
                        type = type.ElementType;
                    else if (type.IsConstructed)
                        type = type.DefiningType;
                    // 新增一个引用去引用定义
                    AddRef(new ReferenceMember(reference), type, reference, null);
                }
                //beRefKey = type;
            }
            else if (beRefObj is CSharpMember)
            {
                CSharpMember member = (CSharpMember)beRefObj;
                while (member.DefiningMember != null)
                {
                    member = member.DefiningMember;
                    // 新增一个引用去引用定义
                    AddRef(new ReferenceMember(reference), member, reference, null);
                }
                //beRefKey = member;
            }

            // 创建被引用的对象
            BEREF defined;
            if (!objectReferences.TryGetValue(beRefObj, out defined))
            {
                defined = new BEREF();
                defined.Define = beRefObj;
                // 针对泛型实例，beRefKey为泛型，beRefObj是构造后的类型
                objectReferences.Add(beRefObj, defined);
            }

            // 创建引用
            REF _ref = new REF();
            _ref.RefSyntax = refSyntax;
            _ref.Definition = defined;
            _ref.Reference = reference;
            defined.References.Add(_ref);
            syntaxReferences.Add(refSyntax, _ref);

            // 创建引用者
            object referencer = refScope;
            if (refScope == null)
            {
                if (DefiningMember != null)
                    referencer = DefiningMember;
                else if (DefiningType != null)
                    referencer = DefiningType;
                else
                {
                    // Attribute可能没有引用域，此时应先定义类型后成员，再引用其特性
                    throw new InvalidOperationException("必须有引用域");
                }
            }
            BEREF defined2;
            if (!objectReferences.TryGetValue(referencer, out defined2))
            {
                defined2 = new BEREF();
                defined2.Define = referencer;
                objectReferences.Add(referencer, defined2);
            }
            _ref.Referencer = defined2;
        }
        private VAR NewVariableAndRef(SyntaxNode node, CSharpType type, Named name)
        {
            VAR variable = new VAR();
            variable.DeclaringMember = DefiningMember;
            variable.Name = name;
            variable.Type = type;
            AddRef(node, variable, name, null);
            return variable;
        }
        internal CSharpType FindTypeAndRef(ReferenceMember node)
        {
            findAndRef = true;
            var type = FindType(node);
            if (type == null)
                throw new ArgumentNullException();
            //AddRef(node, type, node.Name);
            findAndRef = false;
            return type;
        }
        protected override void OnFindType(CSharpType found, CSharpType result, ReferenceMember node)
        {
            if (findAndRef && result != null)
            {
                // 防止漏掉泛型参数类型的引用
                AddRef(node, result, node.Name, null);
                //AddRef(node, found, node.Name);
            }
        }
        protected override void OnFindNamespace(CSharpNamespace target, ReferenceMember node)
        {
            if (findAndRef && target != null)
            {
                // 完全限定名时的命名空间引用
                AddRef(node, target, node.Name, null);
            }
        }

        public override void Visit(Constraint node)
        {
            FindTypeAndRef(node.Type);
            foreach (var item in node.Constraints)
            {
                switch (item.Name.Name)
                {
                    case "new()":
                    case "struct":
                    case "class":
                        break;

                    default:
                        FindTypeAndRef(item);
                        break;
                }
            }
        }
        protected override void InternalVisitType(DefineType node)
        {
            var inherits = DefiningType._BaseTypes;
            for (int i = 0; i < node.Inherits.Count; i++)
            {
                //AddRef(node.Inherits[i], inherits[i], node.Inherits[i].Name);
                CSharpType type = FindTypeAndRef(node.Inherits[i]);
                // 父类型也循环引用子类型，保证子类型不被优化掉
                //AddRef(new ReferenceMember(node.Inherits[i].Name), DefiningType, node.Inherits[i].Name, type);
            }
            CSharpType inherit = DefiningType.BaseClass;
            while (inherit != null)
            {
                CSharpMember constructor = inherit.MemberDefinitions.FirstOrDefault(m => m.IsConstructor && m.Parameters.Count == 0);
                if (constructor != null)
                {
                    // 引用显示声明的无参构造函数
                    AddRef(node, constructor, null, null);
                    break;
                }
                inherit = inherit.BaseClass;
            }
            if (node.Generic.IsGeneric)
            {
                Visit(node.Generic.GenericTypes);
                foreach (var item in node.Generic.Constraints) Visit(item);
            }
            //Visit(node.Attributes);
            foreach (var item in node.Fields) Visit(item);
            foreach (var item in node.Properties) Visit(item);
            foreach (var item in node.Methods) Visit(item);
            // HACK: 父类型的成员可能在此子类型中实现了某接口
            foreach (var item in node.NestedType) Visit(item);
        }
        protected override void InternalVisitEnum(DefineEnum node)
        {
            CSharpType underlyingType = null;
            switch (node.UnderlyingType)
            {
                case EUnderlyingType.BYTE: underlyingType = CSharpType.BYTE; break;
                case EUnderlyingType.SBYTE: underlyingType = CSharpType.SBYTE; break;
                case EUnderlyingType.SHORT: underlyingType = CSharpType.SHORT; break;
                case EUnderlyingType.USHORT: underlyingType = CSharpType.USHORT; break;
                case EUnderlyingType.NONE:
                case EUnderlyingType.INT: underlyingType = CSharpType.INT; break;
                case EUnderlyingType.UINT: underlyingType = CSharpType.UINT; break;
                case EUnderlyingType.LONG: underlyingType = CSharpType.LONG; break;
                case EUnderlyingType.ULONG: underlyingType = CSharpType.ULONG; break;
            }
            AddRef(new ReferenceMember(node.UnderlyingTypeName), underlyingType, node.UnderlyingTypeName, null);
            DefiningType._UnderlyingType = underlyingType;

            decimal value = 0;
            foreach (var item in node.Fields)
            {
                var member = members[item];
                if (item.Value != null)
                {
                    if (!(item.Value is PrimitiveValue))
                        throw new NotImplementedException("不支持枚举值引用其它常量");
                    value = (decimal)Convert.ChangeType(((PrimitiveValue)item.Value).Value, typeof(decimal));
                }
                member._ConstantValue = value;
                value++;
            }
        }
        protected override void InternalVisitDelegate(DefineDelegate node)
        {
            //Visit(node.Attributes);
            if (node.HasReturnType) Visit(node.ReturnType);
            if (node.Generic.IsGeneric)
            {
                Visit(node.Generic.GenericTypes);
                foreach (var item in node.Generic.Constraints) Visit(item);
            }
            Visit(node.Arguments);
        }
        protected override void InternalVisitField(DefineField node)
        {
            //Visit(node.Attributes);
            Visit(node.Type);
            if (node.Value != null) _CalcExpressionType.Calc(this, node.Value, null);
        }
        protected override void InternalVisitField(MemberDefinitionInfo member, Field node)
        {
            if (node.Value != null) _CalcExpressionType.Calc(this, node.Value, null);
        }
        public override void Visit(DefineProperty node)
        {
            base.Visit(node);
            Visit(node.Type);
            //if (node.IsIndexer) Visit(node.Arguments);
            if (node.Getter != null) _CalcExpressionType.Calc(this, node.Getter, node.Arguments);
            if (node.Setter != null) _CalcExpressionType.Calc(this, node.Setter, node.Arguments);
            // 重写的成员或接口的实现成员应该引用base成员
            ReferenceBaseMember(node, 0, node.Arguments);
        }
        public override void Visit(DefineConstructor node)
        {
            base.Visit(node);
            Visit(node.Arguments);
            if (node.Base != null)
            {
                invokeConstructor = true;
                //Visit(node.Base);
                _CalcExpressionType.Calc(this, node, node.Arguments);
                invokeConstructor = false;
            }
            _CalcExpressionType.Calc(this, node.Body, node.Arguments);
            // 静态构造函数 & 默认无参构造函数：类型被调用就算被调用
            if (DefiningMember.IsStatic || node.Arguments.Count == 0)
                AddRef(new ReferenceMember(node.Name), DefiningMember, null, DefiningType);
            // 任何构造函数被调用类型都算被调用
            AddRef(new ReferenceMember(node.Name), DefiningType, null, DefiningMember);
        }
        public override void Visit(DefineMethod node)
        {
            base.Visit(node);
            if (node.HasReturnType)
                Visit(node.ReturnType);
            if (node.ExplicitImplement != null)
                Visit(node.ExplicitImplement);
            if (node.Generic.IsGeneric)
                Visit(node.Generic.GenericTypes);
            Visit(node.Arguments);
            if (node.Generic.IsGeneric && node.Generic.Constraints.Count > 0)
                foreach (var item in node.Generic.Constraints) Visit(item);
            _CalcExpressionType.Calc(this, node.Body, node.Arguments);
            // 重写的成员或接口的实现成员应该引用base成员
            ReferenceBaseMember(node, node.Generic.GenericTypes.Count, node.Arguments);
        }
        void ReferenceBaseMember(DefineMember define, int gcount, List<FormalArgument> arguments)
        {
            int count = arguments == null ? 0 : arguments.Count;
            MemberDefinitionInfo.ParameterData[] argTypes;
            //CSharpType[] argTypes;
            if (count == 0) argTypes = MemberDefinitionInfo.EmptyList;
            else argTypes = new MemberDefinitionInfo.ParameterData[count];
            for (int i = 0; i < argTypes.Length; i++)
            {
                argTypes[i].Type = FindType(arguments[i].Type);
                argTypes[i].Direction = arguments[i].Direction;
            }

            var member = MatchOverridedOrImplementedMember(DefiningMember, gcount, argTypes);
            if (member != null)
            {
                // override引用父类型的方法，接口实现引用接口定义的方法
                AddRef(define, member, define.Name, null);
                // 父类型也循环引用子类型重写成员，保证子类型成员不被优化掉
                AddRef(new ReferenceMember(define.Name), DefiningMember, define.Name, member);

                // 引用到根：有实现接口则引用接口方法，没实现接口则引用到父类型定义的方法
                CSharpMember temp = member;
                while (true)
                {
                    CSharpMember temp2 = MatchOverridedOrImplementedMember(member, gcount, argTypes);
                    if (temp2 == null)
                        break;
                    member = temp2;
                    AddRef(new ReferenceMember(define.Name), DefiningMember, define.Name, member);
                }
                while (member.DefiningMember != null)
                {
                    member = member.DefiningMember;
                    AddRef(new ReferenceMember(define.Name), DefiningMember, define.Name, member);
                }
                DefiningMember._Attributes = ((MemberDefinitionInfo)member)._Attributes;
                DefiningMember._Name = member.Name;
            }
        }
        /// <summary>重写的成员或接口的实现成员应该引用base成员</summary>
        CSharpMember MatchOverridedOrImplementedMember(CSharpMember member, int gcount, MemberDefinitionInfo.ParameterData[] argTypes)
        {
            // 显示实现的接口成员引用接口成员
            // 若没有显示实现的接口成员，但有隐式实现的接口成员，也引用接口成员
            // 若两者都有，则显示的引用接口成员，隐式的不引用
            IEnumerable<CSharpMember> members;
            if (member.IsOverride)
                // override
                members = member.ContainingType.Members;
            else
                // 显式 | 隐式实现接口
                members = CSharpType.GetAllBaseInterfaces(member.ContainingType.BaseInterfaces).SelectMany(b => b.MemberDefinitions);
            CSharpMember result = null;
            foreach (var item in members.Where(m => m.Name.Name == member.Name.Name && m.TypeParameters.Count == gcount))
            {
                //if (item.ExplicitInterfaceImplementation != null)
                //    continue;
                var args = item.Parameters;
                if (args.Count != argTypes.Length)
                    continue;
                int i = 0;
                if (argTypes.All(a => a.MatchParameter(args[i++])))
                    result = item;
            }
            if (result == null || result == member) return null;
            //while (result.DefiningMember != null)
            //    result = result.DefiningMember;
            return result;
        }

        //public override void Visit(List<FormalArgument> node)
        //{
        //    if (node == null)
        //        return;
        //    foreach (var item in node)
        //        NewVariableAndRef(item, FindTypeAndRef(item.Type), item.Name);
        //}
        //public override void Visit(Accessor node)
        //{
        //    _CalcExpressionType.Calc(this, node);
        //}
        //public override void Visit(Body node)
        //{
        //    if (node == null) return;
        //    _CalcExpressionType.Calc(this, node);
        //}

        public override void Visit(ReferenceMember node)
        {
            FindTypeAndRef(node);
        }
        public override void Visit(InvokeMethod node)
        {
            // Constructor : this()
            _CalcExpressionType.Calc(this, node, null);
        }
        // InvokeAttribute
        public override void Visit(List<InvokeAttribute> node)
        {
            if (node == null) return;
            foreach (var item in node)
                FindTypeAndRef((ReferenceMember)item.Target);
        }
    }
}
