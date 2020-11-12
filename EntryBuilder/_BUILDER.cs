using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine.Serialize;
using System.Reflection;
using EntryEngine;
using System.Drawing;

namespace EntryBuilder
{
    static class _BUILDER
    {
        public static void AppendSummary(this StringBuilder builder, MemberInfo member)
        {
            ASummary summary = member.GetAttribute<ASummary>();
            if (summary == null)
                return;
            AppendSummary(builder, summary);
        }
        public static void AppendSummary(this StringBuilder builder, ASummary summary)
        {
            AppendSummary(builder, summary.Note);
        }
        public static void AppendSummary(this StringBuilder builder, string summary)
        {
            string[] lines = summary.Split('\n');
            if (lines.Length > 1)
            {
                builder.AppendLine("/// <summary>");
                foreach (var line in lines)
                    builder.AppendLine("/// <para>{0}</para>", line);
                builder.AppendLine("/// </summary>");
            }
            else
                builder.AppendLine("/// <summary>{0}</summary>", summary);
        }
        public static string GetSummary(this ASummary note)
        {
            StringBuilder builder = new StringBuilder();
            AppendSummary(builder, note);
            return builder.ToString();
        }
        public static string GetSummary(this MemberInfo member)
        {
            ASummary note = member.GetAttribute<ASummary>();
            if (note == null)
                return null;
            else
                return GetSummary(note);
        }
        public static void AppendSharpIfElseCompile(this StringBuilder builder, string pre, Action _true, Action _false)
        {
            if (_true == null && _false == null)
                throw new ArgumentNullException("append");
            if (string.IsNullOrEmpty(pre))
                throw new ArgumentNullException("pre");
            builder.AppendLine("#if {0}", pre);
            if (_true != null)
                _true();
            builder.AppendLine("#else");
            if (_false != null)
                _false();
            builder.AppendLine("#endif");
        }
        public static void AppendSharpIfCompile(this StringBuilder builder, string pre, Action append)
        {
            if (append == null)
                throw new ArgumentNullException("append");
            if (!string.IsNullOrEmpty(pre))
            {
                builder.AppendLine("#if {0}", pre);
                builder.AppendLine();
                append();
                builder.AppendLine();
                builder.AppendLine("#endif");
            }
            else
                append();
        }
        public static void AppendGenericDefine(this StringBuilder builder, MethodBase method)
        {
            if (method.IsGenericMethod)
            {
                Type[] generic = method.GetGenericArguments();
                builder.Append("<");
                for (int i = 0, len = generic.Length - 1; i <= len; i++)
                {
                    builder.Append(generic[i].CodeName(true));
                    if (i != len)
                        builder.Append(", ");
                }
                builder.Append(">");
            }
        }
        public static void AppendMethodDefine(this StringBuilder builder, MethodInfo method)
        {
            AppendMethodDefine(builder, method, null);
        }
        public static void AppendMethodDefine(this StringBuilder builder, MethodInfo method, string modifier)
        {
            builder.Append("{0} ", method.GetModifier());
            if (!string.IsNullOrEmpty(modifier))
                builder.Append("{0} ", modifier);
            builder.Append("{0} ", method.ReturnType.CodeName());
            builder.Append(method.Name);
            builder.AppendGenericDefine(method);
            builder.AppendMethodParametersWithBracket(method);
        }
        public static void AppendMethodParametersWithBracket(this StringBuilder builder, MethodBase method)
        {
            builder.Append("(");
            AppendMethodParametersOnly(builder, method);
            builder.AppendLine(")");
        }
        public static void AppendMethodParametersOnly(this StringBuilder builder, MethodBase method)
        {
            // BUG: ParamArrayAttribute找不到，这一批扩展方法考虑移出EntryEngine到EntryBuilder
#if DEBUG
            var parameters = method.GetParameters();
            for (int j = 0, n = parameters.Length - 1; j <= n; j++)
            {
                ParameterInfo parameter = parameters[j];
                if (parameter.HasAttribute<ParamArrayAttribute>())
                    builder.Append("params ");
                else if (parameter.IsOut)
                    builder.Append("out ");
                else if (parameter.ParameterType.IsByRef)
                    builder.Append("ref ");
                builder.AppendFormat("{0} {1}", parameter.ParameterType.CodeName(true), parameter.Name);
                if (j != n)
                    builder.Append(", ");
            }
#endif
        }
        public static void AppendMethodInvoke(this StringBuilder builder, MethodBase method)
        {
            AppendMethodInvoke(builder, method, null, null, null, null);
        }
        public static void AppendMethodInvoke(this StringBuilder builder, MethodBase method, string instance)
        {
            AppendMethodInvoke(builder, method, instance, null, null, null);
        }
        public static void AppendMethodInvoke(this StringBuilder builder, MethodBase method, string startParam, string endParam)
        {
            AppendMethodInvoke(builder, method, null, null, startParam, endParam);
        }
        public static void AppendMethodInvoke(this StringBuilder builder, MethodBase method, string instance, string startParam, string endParam)
        {
            AppendMethodInvoke(builder, method, instance, null, startParam, endParam);
        }
        public static void AppendMethodInvoke(this StringBuilder builder, MethodBase method, string instance, string methodName, string startParam, string endParam)
        {
            if (!string.IsNullOrEmpty(instance))
                builder.Append("{0}.", instance);
            if (string.IsNullOrEmpty(methodName))
                if (!method.IsConstructor)
                    methodName = method.Name;
            builder.Append(methodName);
            builder.AppendGenericDefine(method);
            builder.Append("(");
            var parameters = method.GetParameters();
            if (!string.IsNullOrEmpty(startParam))
            {
                builder.Append(startParam);
                if (parameters.Length > 0)
                    builder.Append(", ");
            }
            int last = parameters.Length - 1;
            bool hasEndParam = !string.IsNullOrEmpty(endParam);
            for (int j = 0; j <= last; j++)
            {
                ParameterInfo parameter = parameters[j];
                if (parameter.IsOut)
                    builder.Append("out ");
                else if (parameter.ParameterType.IsByRef)
                    builder.Append("ref ");
                builder.AppendFormat("{0}", parameter.Name);
                if (j != last || hasEndParam)
                    builder.Append(", ");
            }
            if (hasEndParam)
                builder.Append(endParam);
            builder.AppendLine(");");
        }
        public static void AppendMethodInvoke(this StringBuilder builder, MethodInfo method)
        {
            AppendMethodInvoke(builder, method, null, null, null, null);
        }
        public static void AppendMethodInvoke(this StringBuilder builder, MethodInfo method, string instance)
        {
            AppendMethodInvoke(builder, method, instance, null, null, null);
        }
        public static void AppendMethodInvoke(this StringBuilder builder, MethodInfo method, string startParam, string endParam)
        {
            AppendMethodInvoke(builder, method, null, null, startParam, endParam);
        }
        public static void AppendMethodInvoke(this StringBuilder builder, MethodInfo method, string instance, string startParam, string endParam)
        {
            AppendMethodInvoke(builder, method, instance, null, startParam, endParam);
        }
        public static void AppendMethodInvoke(this StringBuilder builder, MethodInfo method, string instance, string methodName, string startParam, string endParam)
        {
            if (method.HasReturnType())
                builder.Append("return ");
            AppendMethodInvoke(builder, (MethodBase)method, instance, methodName, startParam, endParam);
        }

        public static void Draw<T>(this AVLTreeBase<T> tree, string output)
        {
            int depth = tree.Height + 1;
            int w = (int)Math.Pow(2, depth);
            int height = 50;
            int y = 10;
            using (Bitmap bitmap = new Bitmap(w * height, depth * height + y * 2))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    Font font = new Font("黑体", 12);

                    Action<int, int, int, AVLTreeBase<T>.AVLTreeNode> draw = null;
                    draw = (_x, _y, _w, node) =>
                    {
                        if (node == null) return;
                        if (node.Left != null)
                            g.DrawLine(Pens.Red, _x, _y, _x - (_w >> 1), _y + height);
                        if (node.Right != null)
                            g.DrawLine(Pens.Red, _x, _y, _x + (_w >> 1), _y + height);
                        int __x = _x - (height >> 1);
                        int __y = _y - (height >> 1);
                        g.FillEllipse(Brushes.Black, __x, __y, height, height);
                        string text = node == null ? "Null" : node.ToString();
                        var size = g.MeasureString(text, font);
                        g.DrawString(text, font, Brushes.White,
                            _x - ((int)size.Width >> 1),
                            _y - ((int)size.Height >> 1));
                        g.DrawString(node.Height.ToString(), font, Brushes.Red, _x, _y);

                        draw(_x - (_w >> 1), _y + height, _w >> 1, node.Left);
                        draw(_x + (_w >> 1), _y + height, _w >> 1, node.Right);
                    };

                    draw((bitmap.Width >> 1), y + (height >> 1), (bitmap.Width >> 1), tree.Root);
                }
                bitmap.Save(output);
            }
        }
    }

    public static class _UNICODE
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
            // 中文
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

        public static string GetString(ushort start, ushort end)
        {
            char[] chars = new char[end - start + 1];
            for (int i = start, index = 0; i <= end; i++, index++)
                chars[index] = (char)i;
            return new string(chars);
        }
        public static string GetString(UnicodeRange range)
        {
            return GetString(range.Start, range.End);
        }
        public static string GetChinese()
        {
            //return GetString(0xF900, 0xFAFF);
            return GetString(0xF900, 0xFA6D);
        }
        public static string GetEnglish()
        {
            return GetString(0, 127);
        }
        public static string GetJapanese()
        {
            return GetString(0x0800, 0x4DFF);
        }
    }
}
