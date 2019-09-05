using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using EntryEngine.Network;
using EntryEngine.Serialize;
using System.Data;
using System.Reflection;
using EntryEngine;

namespace EntryBuilder
{
	static class UtilityCode
	{
		public static string EscapeChars = "rn\'\"\\b";
		public static string LineBreak = "\n";

        /// <summary>
        /// <para>#if EntryBuilder</para>
        /// <para>throw new System.NotImplementedException();</para>
        /// <para>#else</para>
        /// ...
        /// <para>#endif</para>
        /// </summary>
        public static void AppendSharpIfThrow(this StringBuilder builder, Action append)
        {
			const string PRE = "EntryBuilder";
			const string NOT_IMPLEMENT = "throw new System.NotImplementedException();";
			builder.AppendSharpIfElseCompile(PRE,
				() => builder.AppendLine(NOT_IMPLEMENT),
				append);
        }
		public static string BuildCode(string code, string _namespace = "", params string[] usings)
		{
			List<string> builder = new List<string>();
			string format;
			// using
			format = "using {0};";
			foreach (string u in usings)
			{
				builder.Add(Format(format, u));
			}
			if (builder.Count > 0)
				builder.Add("");
			// namespace
			int tab = 0;
			bool hasNamespace = !string.IsNullOrEmpty(_namespace);
			if (hasNamespace)
			{
				format = "namespace {0}";
				builder.Add(Format(format, _namespace));
				builder.Add("{");
				tab = 1;
			}
			string[] codes = code.Replace('\r', ' ').Replace('\t', ' ').Split('\n');
			for (int i = 0; i < codes.Length; i++)
			{
				string temp = codes[i].Trim();
				if (temp == "}")
					tab--;
                codes[i] = _RH.Indent(tab) + temp;
				if (temp == "{")
					tab++;
			}
			builder.AddRange(codes);
			if (hasNamespace)
				builder.Add("}");
			return string.Join("\r\n", builder.ToArray());
		}
		public static string Format(string format, params object[] param)
		{
			return string.Format(format, param);
		}
		public static string StrInSymbol(string text, int pos, string symbol)
		{
			return StrInSymbol(text, pos, symbol, symbol);
		}
		/// <summary>
		/// 字符串中某一字符在某个标识段内的字符串
		/// </summary>
		/// <param name="text">字符串</param>
		/// <param name="pos">字符索引</param>
		/// <param name="symbolS">开始标识</param>
		/// <param name="symbolE">结束标识</param>
		/// <param name="ss">遇到开始标识之前遇到此标识则return null</param>
		/// <param name="se">遇到结束标识之前遇到此标识则return null</param>
		/// <returns>没有处于标识段内则为null</returns>
		public static string StrInSymbol(string text, int pos, string symbolS, string symbolE, string ss = null, string se = null)
		{
			int lenS = symbolS.Length, lenE = symbolE.Length;
			int lenSS = ss == null ? 0 : ss.Length, lenSE = se == null ? 0 : se.Length;
			int start, end;
			int len = text.Length;
			int temp = pos;
			while (true)
			{
				if (temp < 0)
				{
					start = 0;
					return null;
				}

				if (lenSS > 0 && text.Substring(temp, lenSS) == ss)
				{
					return null;
				}

				if ((temp > 0 && lenS == 1 && text[temp - 1] != '\\' && EscapeChars.Contains(text[temp])) &&
					temp + lenS <= len && text.Substring(temp, lenS) == symbolS)
				{
					start = temp + lenS;
					break;
				}
				else
				{
					temp--;
				}
			}
			temp = pos;
			while (true)
			{
				if (temp == len)
				{
					end = len;
					return null;
				}

				if (lenSE > 0 && text.Substring(temp, lenSE) == se)
				{
					return null;
				}

				if ((temp > 0 && lenE == 1 && text[temp - 1] != '\\' && EscapeChars.Contains(text[temp])) &&
					temp + lenE <= len && text.Substring(temp, lenE) == symbolE)
				{
					end = temp;
					break;
				}
				else
				{
					temp++;
				}
			}
			return text.Substring(start, end - start);
		}
		public static bool IsStrInSymbol(string text, int pos, string symbol)
		{
			return StrInSymbol(text, pos, symbol) != null;
		}
		public static bool IsStrInSymbol(string text, int pos, string symbolS, string symbolE, string ss = null, string se = null)
		{
			return StrInSymbol(text, pos, symbolS, symbolE, ss, se) != null;
		}
		public static bool IsStrInLineSymbol(string text, int pos, string symbolS, string symbolE)
		{
			return StrInSymbol(text, pos, symbolS, symbolE, LineBreak, LineBreak) != null;
		}
		public static bool IsStrInLineString(string text, int pos)
		{
			return IsStrInLineString(text, pos, LineBreak, LineBreak);
		}
		public static bool IsStrInLineString(string text, int pos, string ss = null, string se = null)
		{
			if (pos > 0 && pos < text.Length - 1)
			{
				string symbol = "\"";
				bool result = IsStrInSymbol(text, pos, symbol, symbol, ss, se);
				if (!result)
				{
					char c = '\'';
					result = text[pos - 1] == c && text[pos + 1] == c;
				}
				return result;
			}
			else
			{
				return false;
			}
		}
        public static void Push<T>(this Stack<T> stack, IEnumerable<T> source)
        {
            foreach (var item in source)
            {
                stack.Push(item);
            }
        }

        public static bool CheckCompileError(CompilerResults result)
        {
            if (result.Errors.HasErrors)
            {
                for (int i = 0; i < result.Errors.Count; i++)
                {
                    Console.WriteLine(result.Errors[i].ErrorText);
                }
                return true;
            }
            return false;
        }
        public static CompilerResults Compile(string output, string version, string platform, string icon, string refferenceDllDir, params string[] codes)
        {
            CSharpCodeProvider.SetCompileVersion(version);
            CompilerParameters param = new CompilerParameters();
            param.CompilerOptions = "/unsafe /optimize";
            if (!string.IsNullOrEmpty(platform))
                param.CompilerOptions += string.Format(" /platform:{0}", platform);
            if (!string.IsNullOrEmpty(icon))
                param.CompilerOptions += string.Format(" /win32icon:{0}", icon);
            param.ReferencedAssemblies.Add("System.dll");
            param.ReferencedAssemblies.Add("System.Data.dll");
            param.ReferencedAssemblies.Add("System.Core.dll");
            if (!string.IsNullOrEmpty(refferenceDllDir))
            {
                var dlls = Directory.GetFiles(refferenceDllDir, "*.dll", SearchOption.TopDirectoryOnly);
                foreach (string dll in dlls)
                    param.ReferencedAssemblies.Add(Path.GetFileName(dll));
            }
            param.GenerateExecutable = Path.GetExtension(output) == ".exe";
            param.OutputAssembly = output;
            param.GenerateInMemory = false;
            param.TreatWarningsAsErrors = false;

            CSharpCodeProvider compiler = new CSharpCodeProvider();
            return compiler.CompileAssemblyFromSource(param, codes);
        }

        private static MD5 md5;
        private static MD5 MD5
        {
            get
            {
                if (md5 == null)
                    md5 = new MD5CryptoServiceProvider();
                return md5;
            }
        }

        public static string GetFileMD5(string file)
        {
            byte[] buffer;
            using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read))
                buffer = MD5.ComputeHash(stream);

            /* 
             * 获取文件的MD5值(16位byte[])
             * Windows计算出的是32位的MD5值
             * 需要将16位换算成32位字符
             * 换算原理: 32位 = 16位高位4位 + 16位低位4位
             */
            StringBuilder md5 = new StringBuilder();
            int i, j;
            foreach (byte b in buffer)
            {
                i = (int)b;
                j = i >> 4;     // 获得高4位
                md5.Append(Convert.ToString(j, 16));
                j = (i << 4 & 0x00ff) >> 4;     // 升4位，去掉高位，降回原来，获得低4位
                md5.Append(Convert.ToString(j, 16));
            }

            return md5.ToString();
        }
	}
    class CSharpCodeProvider : Microsoft.CSharp.CSharpCodeProvider
    {
        private static Dictionary<string, string> CompileVersion = new Dictionary<string, string>();
        private const string COMPILE_VERSION = "CompilerVersion";

        public static void SetCompileVersion(string version)
        {
            //if (version != 2.0 &&
            //    version != 3.0 &&
            //    version != 3.5 &&
            //    version != 4.0 &&
            //    version != 4.5)
            //{
            //    //throw new NotImplementedException("not support .net framework compiler version " + version);
            //    CompileVersion.Remove(COMPILE_VERSION);
            //    Console.WriteLine("use default compile version");
            //    return;
            //}
            if (string.IsNullOrEmpty(version))
                return;
            if (!version.StartsWith("v"))
                version = "v" + version;
            CompileVersion[COMPILE_VERSION] = version;
        }

        public CSharpCodeProvider()
            : base(CompileVersion)
        {
        }
    }

    partial class Program
    {
    }
}