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
        public static CompilerResults Compile(string output, string version, string platform, string refferenceDllDir, params string[] codes)
        {
            CSharpCodeProvider.SetCompileVersion(version);
            CompilerParameters param = new CompilerParameters();
            param.CompilerOptions = "/unsafe /optimize";
            if (!string.IsNullOrEmpty(platform))
                param.CompilerOptions += string.Format(" /platform:{0}", platform);
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

		public static class UNICODE
		{
			public class UnicodeRange
			{
				public string Name;
				public ushort Start;
				public ushort End;

				public UnicodeRange(string name, ushort start, ushort end)
				{
					this.Name = name;
					this.Start = start;
					this.End = end;
				}
			}
			public static readonly UnicodeRange[] Unicodes =
			{
				new UnicodeRange("C0控制符及基本拉丁文 (C0 Control and Basic Latin)", 0x0000, 0x007F),
				new UnicodeRange("C1控制符及拉丁文补充-1 (C1 Control and Latin 1 Supplement)", 0x0080, 0x00FF),
				new UnicodeRange("拉丁文扩展-A (Latin Extended-A)", 0x0100, 0x017F),
				new UnicodeRange("拉丁文扩展-B (Latin Extended-B)", 0x0180, 0x024F),
				new UnicodeRange("国际音标扩展 (IPA Extensions)", 0x0250, 0x02AF),
				new UnicodeRange("空白修饰字母 (Spacing Modifiers)", 0x02B0, 0x02FF),
				new UnicodeRange("结合用读音符号 (Combining Diacritics Marks)", 0x0300, 0x036F),
				new UnicodeRange("希腊文及科普特文 (Greek and Coptic)", 0x0370, 0x03FF),
				new UnicodeRange("西里尔字母 (Cyrillic)", 0x0400, 0x04FF),
				new UnicodeRange("西里尔字母补充 (Cyrillic Supplement)", 0x0500, 0x052F),
				new UnicodeRange("亚美尼亚语 (Armenian)", 0x0530, 0x058F),
				new UnicodeRange("希伯来文 (Hebrew)", 0x0590, 0x05FF),
				new UnicodeRange("阿拉伯文 (Arabic)", 0x0600, 0x06FF),
				new UnicodeRange("叙利亚文 (Syriac)", 0x0700, 0x074F),
				new UnicodeRange("阿拉伯文补充 (Arabic Supplement)", 0x0750, 0x077F),
				new UnicodeRange("马尔代夫语 (Thaana)", 0x0780, 0x07BF),
				new UnicodeRange("西非书面语言 (N'Ko)", 0x07C0, 0x07FF),
				new UnicodeRange("阿维斯塔语及巴列维语 (Avestan and Pahlavi)", 0x0800, 0x085F),
				new UnicodeRange("Mandaic", 0x0860, 0x087F),
				new UnicodeRange("撒马利亚语 (Samaritan)", 0x0880, 0x08AF),
				new UnicodeRange("天城文书 (Devanagari)", 0x0900, 0x097F),
				new UnicodeRange("孟加拉语 (Bengali)", 0x0980, 0x09FF),
				new UnicodeRange("锡克教文 (Gurmukhi)", 0x0A00, 0x0A7F),
				new UnicodeRange("古吉拉特文 (Gujarati)", 0x0A80, 0x0AFF),
				new UnicodeRange("奥里亚文 (Oriya)", 0x0B00, 0x0B7F),
				new UnicodeRange("泰米尔文 (Tamil)", 0x0B80, 0x0BFF),
				new UnicodeRange("泰卢固文 (Telugu)", 0x0C00, 0x0C7F),
				new UnicodeRange("卡纳达文 (Kannada)", 0x0C80, 0x0CFF),
				new UnicodeRange("德拉维族语 (Malayalam)", 0x0D00, 0x0D7F),
				new UnicodeRange("僧伽罗语 (Sinhala)", 0x0D80, 0x0DFF),
				new UnicodeRange("泰文 (Thai)", 0x0E00, 0x0E7F),
				new UnicodeRange("老挝文 (Lao)", 0x0E80, 0x0EFF),
				new UnicodeRange("藏文 (Tibetan)", 0x0F00, 0x0FFF),
				new UnicodeRange("缅甸语 (Myanmar)", 0x1000, 0x109F),
				new UnicodeRange("格鲁吉亚语 (Georgian)", 0x10A0, 0x10FF),
				new UnicodeRange("朝鲜文 (Hangul Jamo)", 0x1100, 0x11FF),
				new UnicodeRange("埃塞俄比亚语 (Ethiopic)", 0x1200, 0x137F),
				new UnicodeRange("埃塞俄比亚语补充 (Ethiopic Supplement)", 0x1380, 0x139F),
				new UnicodeRange("切罗基语 (Cherokee)", 0x13A0, 0x13FF),
				new UnicodeRange("统一加拿大土著语音节 (Unified Canadian Aboriginal Syllabics)", 0x1400, 0x167F),
				new UnicodeRange("欧甘字母 (Ogham)", 0x1680, 0x169F),
				new UnicodeRange("如尼文 (Runic)", 0x16A0, 0x16FF),
				new UnicodeRange("塔加拉语 (Tagalog)", 0x1700, 0x171F),
				new UnicodeRange("Hanunóo", 0x1720, 0x173F),
				new UnicodeRange("Buhid", 0x1740, 0x175F),
				new UnicodeRange("Tagbanwa", 0x1760, 0x177F),
				new UnicodeRange("高棉语 (Khmer)", 0x1780, 0x17FF),
				new UnicodeRange("蒙古文 (Mongolian)", 0x1800, 0x18AF),
				new UnicodeRange("Cham", 0x18B0, 0x18FF),
				new UnicodeRange("Limbu", 0x1900, 0x194F),
				new UnicodeRange("德宏泰语 (Tai Le)", 0x1950, 0x197F),
				new UnicodeRange("新傣仂语 (New Tai Lue)", 0x1980, 0x19DF),
				new UnicodeRange("高棉语记号 (Kmer Symbols)", 0x19E0, 0x19FF),
				new UnicodeRange("Buginese", 0x1A00, 0x1A1F),
				new UnicodeRange("Batak", 0x1A20, 0x1A5F),
				new UnicodeRange("Lanna", 0x1A80, 0x1AEF),
				new UnicodeRange("巴厘语 (Balinese)", 0x1B00, 0x1B7F),
				new UnicodeRange("巽他语 (Sundanese)", 0x1B80, 0x1BB0),
				new UnicodeRange("Pahawh Hmong", 0x1BC0, 0x1BFF),
				new UnicodeRange("雷布查语(Lepcha)", 0x1C00, 0x1C4F),
				new UnicodeRange("Ol Chiki", 0x1C50, 0x1C7F),
				new UnicodeRange("曼尼普尔语 (Meithei/Manipuri)", 0x1C80, 0x1CDF),
				new UnicodeRange("语音学扩展 (Phonetic Extensions)", 0x1D00, 0x1D7F),
				new UnicodeRange("语音学扩展补充 (Phonetic Extensions Supplement)", 0x1D80, 0x1DBF),
				new UnicodeRange("结合用读音符号补充 (Combining Diacritics Marks Supplement)", 0x1DC0, 0x1DFF),
				new UnicodeRange("拉丁文扩充附加 (Latin Extended Additional)", 0x1E00, 0x1EFF),
				new UnicodeRange("希腊语扩充 (Greek Extended)", 0x1F00, 0x1FFF),
				new UnicodeRange("常用标点 (General Punctuation)", 0x2000, 0x206F),
				new UnicodeRange("上标及下标 (Superscripts and Subscripts)", 0x2070, 0x209F),
				new UnicodeRange("货币符号 (Currency Symbols)", 0x20A0, 0x20CF),
				new UnicodeRange("组合用记号 (Combining Diacritics Marks for Symbols)", 0x20D0, 0x20FF),
				new UnicodeRange("字母式符号 (Letterlike Symbols)", 0x2100, 0x214F),
				new UnicodeRange("数字形式 (Number Form)", 0x2150, 0x218F),
				new UnicodeRange("箭头 (Arrows)", 0x2190, 0x21FF),
				new UnicodeRange("数学运算符 (Mathematical Operator)", 0x2200, 0x22FF),
				new UnicodeRange("杂项工业符号 (Miscellaneous Technical)", 0x2300, 0x23FF),
				new UnicodeRange("控制图片 (Control Pictures)", 0x2400, 0x243F),
				new UnicodeRange("光学识别符 (Optical Character Recognition)", 0x2440, 0x245F),
				new UnicodeRange("封闭式字母数字 (Enclosed Alphanumerics)", 0x2460, 0x24FF),
				new UnicodeRange("制表符 (Box Drawing)", 0x2500, 0x257F),
				new UnicodeRange("方块元素 (Block Element)", 0x2580, 0x259F),
				new UnicodeRange("几何图形 (Geometric Shapes)", 0x25A0, 0x25FF),
				new UnicodeRange("杂项符号 (Miscellaneous Symbols)", 0x2600, 0x26FF),
				new UnicodeRange("印刷符号 (Dingbats)", 0x2700, 0x27BF),
				new UnicodeRange("杂项数学符号-A (Miscellaneous Mathematical Symbols-A)", 0x27C0, 0x27EF),
				new UnicodeRange("追加箭头-A (Supplemental Arrows-A)", 0x27F0, 0x27FF),
				new UnicodeRange("盲文点字模型 (Braille Patterns)", 0x2800, 0x28FF),
				new UnicodeRange("追加箭头-B (Supplemental Arrows-B)", 0x2900, 0x297F),
				new UnicodeRange("杂项数学符号-B (Miscellaneous Mathematical Symbols-B)", 0x2980, 0x29FF),
				new UnicodeRange("追加数学运算符 (Supplemental Mathematical Operator)", 0x2A00, 0x2AFF),
				new UnicodeRange("杂项符号和箭头 (Miscellaneous Symbols and Arrows)", 0x2B00, 0x2BFF),
				new UnicodeRange("格拉哥里字母 (Glagolitic)", 0x2C00, 0x2C5F),
				new UnicodeRange("拉丁文扩展-C (Latin Extended-C)", 0x2C60, 0x2C7F),
				new UnicodeRange("古埃及语 (Coptic)", 0x2C80, 0x2CFF),
				new UnicodeRange("格鲁吉亚语补充 (Georgian Supplement)", 0x2D00, 0x2D2F),
				new UnicodeRange("提非纳文 (Tifinagh)", 0x2D30, 0x2D7F),
				new UnicodeRange("埃塞俄比亚语扩展 (Ethiopic Extended)", 0x2D80, 0x2DDF),
				new UnicodeRange("追加标点 (Supplemental Punctuation)", 0x2E00, 0x2E7F),
				new UnicodeRange("CJK 部首补充 (CJK Radicals Supplement)", 0x2E80, 0x2EFF),
				new UnicodeRange("康熙字典部首 (Kangxi Radicals)", 0x2F00, 0x2FDF),
				new UnicodeRange("表意文字描述符 (Ideographic Description Characters)", 0x2FF0, 0x2FFF),
				new UnicodeRange("CJK 符号和标点 (CJK Symbols and Punctuation)", 0x3000, 0x303F),
				new UnicodeRange("日文平假名 (Hiragana)", 0x3040, 0x309F),
				new UnicodeRange("日文片假名 (Katakana)", 0x30A0, 0x30FF),
				new UnicodeRange("注音字母 (Bopomofo)", 0x3100, 0x312F),
				new UnicodeRange("朝鲜文兼容字母 (Hangul Compatibility Jamo)", 0x3130, 0x318F),
				new UnicodeRange("象形字注释标志 (Kanbun)", 0x3190, 0x319F),
				new UnicodeRange("注音字母扩展 (Bopomofo Extended)", 0x31A0, 0x31BF),
				new UnicodeRange("CJK 笔画 (CJK Strokes)", 0x31C0, 0x31EF),
				new UnicodeRange("日文片假名语音扩展 (Katakana Phonetic Extensions)", 0x31F0, 0x31FF),
				new UnicodeRange("封闭式 CJK 文字和月份 (Enclosed CJK Letters and Months)", 0x3200, 0x32FF),
				new UnicodeRange("CJK 兼容 (CJK Compatibility)", 0x3300, 0x33FF),
				new UnicodeRange("CJK 统一表意符号扩展 A (CJK Unified Ideographs Extension A)", 0x3400, 0x4DBF),
				new UnicodeRange("易经六十四卦符号 (Yijing Hexagrams Symbols)", 0x4DC0, 0x4DFF),
				new UnicodeRange("CJK 统一表意符号 (CJK Unified Ideographs)", 0x4E00, 0x9FBF),
				new UnicodeRange("彝文音节 (Yi Syllables)", 0xA000, 0xA48F),
				new UnicodeRange("彝文字根 (Yi Radicals)", 0xA490, 0xA4CF),
				new UnicodeRange("Vai", 0xA500, 0xA61F),
				new UnicodeRange("统一加拿大土著语音节补充 (Unified Canadian Aboriginal Syllabics Supplement)", 0xA660, 0xA6FF),
				new UnicodeRange("声调修饰字母 (Modifier Tone Letters)", 0xA700, 0xA71F),
				new UnicodeRange("拉丁文扩展-D (Latin Extended-D)", 0xA720, 0xA7FF),
				new UnicodeRange("Syloti Nagri", 0xA800, 0xA82F),
				new UnicodeRange("八思巴字 (Phags-pa)", 0xA840, 0xA87F),
				new UnicodeRange("Saurashtra", 0xA880, 0xA8DF),
				new UnicodeRange("爪哇语 (Javanese)", 0xA900, 0xA97F),
				new UnicodeRange("Chakma", 0xA980, 0xA9DF),
				new UnicodeRange("Varang Kshiti", 0xAA00, 0xAA3F),
				new UnicodeRange("Sorang Sompeng", 0xAA40, 0xAA6F),
				new UnicodeRange("Newari", 0xAA80, 0xAADF),
				new UnicodeRange("越南傣语 (Vi?t Thái)", 0xAB00, 0xAB5F),
				new UnicodeRange("Kayah Li", 0xAB80, 0xABA0),
				new UnicodeRange("朝鲜文音节 (Hangul Syllables)", 0xAC00, 0xD7AF),
				new UnicodeRange("High-half zone of UTF-16", 0xD800, 0xDBFF),
				new UnicodeRange("Low-half zone of UTF-16", 0xDC00, 0xDFFF),
				new UnicodeRange("自行使用区域 (Private Use Zone)", 0xE000, 0xF8FF),
				new UnicodeRange("CJK 兼容象形文字 (CJK Compatibility Ideographs)", 0xF900, 0xFAFF),
				new UnicodeRange("字母表达形式 (Alphabetic Presentation Form)", 0xFB00, 0xFB4F),
				new UnicodeRange("阿拉伯表达形式A (Arabic Presentation Form-A)", 0xFB50, 0xFDFF),
				new UnicodeRange("变量选择符 (Variation Selector)", 0xFE00, 0xFE0F),
				new UnicodeRange("竖排形式 (Vertical Forms)", 0xFE10, 0xFE1F),
				new UnicodeRange("组合用半符号 (Combining Half Marks)", 0xFE20, 0xFE2F),
				new UnicodeRange("CJK 兼容形式 (CJK Compatibility Forms)", 0xFE30, 0xFE4F),
				new UnicodeRange("小型变体形式 (Small Form Variants)", 0xFE50, 0xFE6F),
				new UnicodeRange("阿拉伯表达形式B (Arabic Presentation Form-B)", 0xFE70, 0xFEFF),
				new UnicodeRange("半型及全型形式 (Halfwidth and Fullwidth Form)", 0xFF00, 0xFFEF),
				new UnicodeRange("特殊 (Specials)", 0xFFF0, 0xFFFF),
			};
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