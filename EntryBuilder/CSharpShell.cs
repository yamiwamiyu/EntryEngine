using System;
using System.Text;
using System.Linq;
using EntryEngine;
using EntryEngine.Serialize;

namespace EntryBuilder
{
    abstract class Shell
    {
        public abstract void Protect(ref byte[] bytes);
        /// <summary>
        /// <para>构建脱壳代码</para>
        /// <para>传入参数：byte[] bytes</para>
        /// <para>代码目标：将bytes变回调用Protect之前的样子</para>
        /// </summary>
        /// <param name="builder">代码构建器</param>
        public abstract void BuildLoadCode(StringBuilder builder);
    }
    internal abstract class ShellShell : Shell
    {
        public virtual Shell Base { get; set; }

        public ShellShell() { }
        public ShellShell(Shell Base) { this.Base = Base; }

        public override void Protect(ref byte[] bytes)
        {
            Base.Protect(ref bytes);
        }
        public override void BuildLoadCode(StringBuilder builder)
        {
            Base.BuildLoadCode(builder);
        }
    }
    class ShellReverse : Shell
    {
        public override void Protect(ref byte[] bytes)
        {
            Array.Reverse(bytes);
        }
        public override void BuildLoadCode(StringBuilder builder)
        {
            builder.AppendLine("Array.Reverse(bytes);");
        }
    }
    class ShellPlus : Shell
    {
        private byte plus;
        public override void Protect(ref byte[] bytes)
        {
            plus = _RANDOM.NextByte();
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = (byte)(bytes[i] + plus);
        }
        public override void BuildLoadCode(StringBuilder builder)
        {
            builder.AppendLine("for (int i = 0; i < bytes.Length; i++)");
            builder.AppendBlock(() =>
            {
                builder.AppendLine("bytes[i] = (byte)(bytes[i] - {0});", plus);
            });
        }
    }
    class ShellShuffle : Shell
    {
        int[] indexes;
        public override void Protect(ref byte[] bytes)
        {
            Random random = new Random(_RANDOM.Next());
            int length = bytes.Length;
            int count = _MATH.Min(random.Next(100, 1000), length);
            indexes = new int[count];
            for (int i = 0; i < count; i++)
            {
                indexes[i] = random.Next(length);
            }
            byte temp;
            for (int i = count - 1; i >= 0; i--)
            {
                int index = indexes[i];
                temp = bytes[index];
                bytes[index] = bytes[i];
                bytes[i] = temp;
            }
        }
        public override void BuildLoadCode(StringBuilder builder)
        {
            builder.AppendLine("int[] indexes = new int[] {{ {0} }};", string.Join(",", indexes.Select(i => i.ToString()).ToArray()));
            builder.AppendLine("byte temp;");
            builder.AppendLine("for (int i = 0; i < indexes.Length; i++)");
            builder.AppendBlock(() =>
            {
                builder.AppendLine("int index = indexes[i];");
                builder.AppendLine("temp = bytes[index];");
                builder.AppendLine("bytes[index] = bytes[i];");
                builder.AppendLine("bytes[i] = temp;");
            });
        }
    }
    class ShellCheck : Shell
    {
        private int key;
        public override void Protect(ref byte[] bytes)
        {
            key = _RANDOM.Next();
            int value = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                value += bytes[i];
            }
            value += key;
            bytes = bytes.Insert(0, BitConverter.GetBytes(value));
        }
        public override void BuildLoadCode(StringBuilder builder)
        {
            builder.AppendLine("int value = BitConverter.ToInt32(bytes, 0);");
            builder.AppendLine("for (int i = 4; i < bytes.Length; i++)");
            builder.AppendBlock(() =>
            {
                builder.AppendLine("value -= bytes[i];");
            });
            builder.AppendLine("if (value != {0})", key);
            builder.AppendBlock(() =>
            {
                builder.AppendLine("throw new Exception(\"Don't reflect me!\");");
            });
            builder.AppendLine("byte[] temp = new byte[bytes.Length - 4];");
            builder.AppendLine("Array.Copy(bytes, 4, temp, 0, temp.Length);");
            builder.AppendLine("bytes = temp;");
        }
    }
}
