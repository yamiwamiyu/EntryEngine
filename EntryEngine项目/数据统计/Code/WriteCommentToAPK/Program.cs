using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WriteCommentToAPK
{
    class Program
    {
        public class ApkFile
        {
            public const int CHANNEL_ID = 0x012FCF81;
            static readonly byte[] MAGIC_DIR_END = { 0x50, 0x4b, 0x05, 0x06 };
            //static readonly byte[] MAGIC_DIR_END = { 0x06, 0x05, 0x4b, 0x50, };

            public byte[] Bytes;
            public Encoding Encoding = Encoding.UTF8;

            public string Comment
            {
                get
                {
                    int offset, length;
                    if (CheckComment(out offset, out length))
                        return Encoding.GetString(Bytes, offset, length);
                    else
                        throw new InvalidOperationException("无效的ZIP格式，不能读取Comment");
                }
                set
                {
                    int offset, length;
                    if (CheckComment(out offset, out length))
                    {
                        byte[] commentBytes = Encoding.GetBytes(value);
                        int newLength = commentBytes.Length;
                        if (newLength != length)
                        {
                            Bytes[offset - 2] = (byte)(newLength & 255);
                            Bytes[offset - 1] = (byte)(newLength >> 8);
                            byte[] newBytes = new byte[Bytes.Length - length + commentBytes.Length];
                            Array.Copy(Bytes, 0, newBytes, 0, offset);
                            Array.Copy(Bytes, 0, newBytes, offset + length, Bytes.Length - (offset + length));
                            Bytes = newBytes;
                        }
                        Array.Copy(commentBytes, 0, Bytes, offset, newLength);
                    }
                    else
                        throw new InvalidOperationException("无效的ZIP格式，不能写入Comment");
                }
            }
            public bool HasV2
            {
                get
                {
                    int offset;
                    return CheckHasV2(out offset);
                }
            }

            public ApkFile(byte[] bytes)
            {
                this.Bytes = bytes;
            }
            public ApkFile(byte[] bytes, Encoding encoding)
            {
                this.Bytes = bytes;
                this.Encoding = encoding;
            }

            private void Insert(long index, byte[] data)
            {
                byte[] newBytes = new byte[Bytes.Length + data.Length];
                Array.Copy(Bytes, 0, newBytes, 0, index);
                Array.Copy(data, 0, newBytes, index, data.Length);
                Array.Copy(Bytes, index, newBytes, (index + data.Length), newBytes.Length - (index + data.Length));
                Bytes = newBytes;
            }

            public bool InsertData(int key, byte[] value)
            {
                int offset;
                if (!CheckHasV2(out offset))
                    return false;
                // Size-ID-Value
                byte[] commentBytes = new byte[12 + value.Length];
                byte[] idValueSizeBytes = BitConverter.GetBytes((long)(4 + value.Length));
                Array.Copy(idValueSizeBytes, 0, commentBytes, 0, 8);
                byte[] idBytes = BitConverter.GetBytes(key);
                Array.Copy(idBytes, 0, commentBytes, 8, 4);
                Array.Copy(value, 0, commentBytes, 12, value.Length);

                //long blockSize = BitConverter.ToInt64(bytes, offset - 24);
                //long testSize = BitConverter.ToInt64(bytes, (int)(offset - 8 - blockSize));
                //Console.WriteLine("Block Data Size: {0} {1}", blockSize, testSize);
                //// 改变数据内容尺寸
                //byte[] contentLengthBytes = BitConverter.GetBytes(blockSize + commentBytes.Length);
                //Array.Copy(contentLengthBytes, 0, bytes, offset - 24, 8);
                //Array.Copy(contentLengthBytes, 0, bytes, offset - 8 - blockSize, 8);
                //blockSize = BitConverter.ToInt64(bytes, offset - 24);

                // 插入新数据块
                //Insert(offset - 24, commentBytes);
                // 插入新数据会导致后面offset修改
                // 改为修改v2签名块键值对中的内容，具体如下
                // 1. 签名块的内容尺寸一般在4096的倍数
                // 2. 最后一个键值对的值部分很多0空着为了凑够4096的倍数
                // 3. 将最后一个签名的尺寸改小到非0的部分
                // 4. 后面全0的部分用自己新建的渠道键值对去覆盖
                int blockEnd = offset - 24;
                int blockSize = (int)BitConverter.ToInt64(Bytes, blockEnd);
                int index = offset - blockSize;
                while (index < blockEnd)
                {
                    long size = BitConverter.ToInt64(Bytes, index);
                    int readID = BitConverter.ToInt32(Bytes, index + 8);
                    if (readID == key)
                    {
                        throw new InvalidOperationException(string.Format("已经包含Key:{0} Value:{1}", readID, Encoding.UTF8.GetString(Bytes, index + 12, (int)size - 4)));
                    }
                    if (index + 8 + size == blockEnd)
                    {
                        // 找到全0并且足够容纳的索引
                        int start = index + 12;
                        int nullIndex = start;
                        for (int i = blockEnd - 1; i >= start; i--)
                        {
                            if (Bytes[i] == 0)
                            {
                                nullIndex = i + 1;
                                if (nullIndex + commentBytes.Length == blockEnd)
                                    break;
                            }
                            else
                                break;
                        }
                        if (nullIndex + commentBytes.Length > blockEnd)
                        {
                            throw new NotImplementedException(string.Format("剩余空间不够写入渠道，剩余{0}字节，需要{1}字节",
                                nullIndex == -1 ? 0 : blockEnd - nullIndex, commentBytes.Length));
                        }
                        long newSize = 4 + (nullIndex - start);
                        // 更改上一个块的尺寸
                        byte[] newSizeBytes = BitConverter.GetBytes(newSize);
                        Array.Copy(newSizeBytes, 0, Bytes, index, 8);
                        // 新增渠道块内容
                        Array.Copy(commentBytes, 0, Bytes, nullIndex, commentBytes.Length);
                    }
                    index += 8 + (int)size;
                }

                // 以下更改了非签名块的部分，会导致签名验证不通过
                // 更新offset
                //byte[] offsetBytes = BitConverter.GetBytes(offset + commentBytes.Length);
                //Array.Copy(offsetBytes, 0, bytes, bytes.Length - 6, 4);

                return true;
            }
            public byte[] ReadData(int id)
            {
                int offset;
                if (!CheckHasV2(out offset))
                    return null;

                int blockEnd = offset - 24;
                int blockSize = (int)BitConverter.ToInt64(Bytes, blockEnd);
                int index = offset - blockSize;
                while (index < blockEnd)
                {
                    long size = BitConverter.ToInt64(Bytes, index);
                    int readID = BitConverter.ToInt32(Bytes, index + 8);
                    if (readID == id)
                    {
                        byte[] data = new byte[size - 4];
                        Array.Copy(Bytes, index + 12, data, 0, size - 4);
                        return data;
                    }
                    else
                        index += 8 + (int)size;
                }
                return null;
            }
            private bool CheckComment(out int commentOffset, out int commentLength)
            {
                for (int i = Bytes.Length - 22; i >= 0; i--)
                {
                    if (Bytes[i] == MAGIC_DIR_END[0] &&
                        Bytes[i + 1] == MAGIC_DIR_END[1] &&
                        Bytes[i + 2] == MAGIC_DIR_END[2] &&
                        Bytes[i + 3] == MAGIC_DIR_END[3])
                    {
                        commentOffset = i + 22;
                        commentLength = Bytes[i + 20] + (Bytes[i + 21] << 8);
                        return true;
                    }
                }
                commentOffset = -1;
                commentLength = 0;
                return false;
            }
            private byte[] GetCommentBytes()
            {
                int offset, length;
                CheckComment(out offset, out length);
                byte[] result = new byte[length];
                Array.Copy(Bytes, offset, result, 0, length);
                return result;
            }
            public bool CheckHasV2(out int offset)
            {
                offset = BitConverter.ToInt32(Bytes, Bytes.Length - 6);
                if (offset < 0 || offset >= Bytes.Length)
                    return false;
                return Encoding.ASCII.GetString(Bytes, offset - 16, 16) == "APK Sig Block 42";
            }

            public static string ReadChannel(byte[] buffer)
            {
                ApkFile apk = new ApkFile(buffer);
                int offset;
                if (apk.CheckHasV2(out offset))
                {
                    byte[] data = apk.ReadData(ApkFile.CHANNEL_ID);
                    if (data == null)
                        return null;
                    else
                        return apk.Encoding.GetString(data);
                }
                else
                {
                    return apk.Comment;
                }
            }
        }

        public static void WriteChannelToAPK(string channel, string apkFile, string outputApkFile)
        {
            ApkFile apk = new ApkFile(File.ReadAllBytes(apkFile));
            int offset;
            if (apk.CheckHasV2(out offset))
            {
                apk.InsertData(ApkFile.CHANNEL_ID, apk.Encoding.GetBytes(channel));
                File.WriteAllBytes(outputApkFile, apk.Bytes);
            }
            else
            {
                // 当前安装包不具有 V2 签名
                // 使用Ionic.Zip.dll的方法
                //using (Ionic.Zip.ZipFile file = Ionic.Zip.ZipFile.Read(apkFile, new Ionic.Zip.ReadOptions()
                //{
                //    Encoding = Encoding.UTF8
                //}))
                //{
                //    file.Comment = channel;
                //    file.Save(outputApkFile);
                //}

                apk.Comment = channel;
                //string test = apk.Comment;
                File.WriteAllBytes(outputApkFile, apk.Bytes);
                Console.WriteLine("写入渠道：当前安装包不具有 V2 签名");
            }
        }
        public static string ReadAPKChannel(string apkFile)
        {
            ApkFile apk = new ApkFile(File.ReadAllBytes(apkFile));
            int offset;
            if (apk.CheckHasV2(out offset))
            {
                byte[] data = apk.ReadData(ApkFile.CHANNEL_ID);
                if (data == null)
                    return string.Empty;
                else
                    return apk.Encoding.GetString(data);
            }
            else
            {
                // 当前安装包不具有 V2 签名
                //using (Ionic.Zip.ZipFile file = Ionic.Zip.ZipFile.Read(apkFile, new Ionic.Zip.ReadOptions()
                //{
                //    Encoding = Encoding.UTF8
                //}))
                //{
                //    return file.Comment;
                //}
                return apk.Comment;
            }
        }
        static void Main(string[] args)
        {
            //Console.WriteLine(BitConverter.IsLittleEndian);
            //WriteChannelToAPK("测试渠道号123", "朕的江山.apk", "朕的江山_test.apk");
            //string channelTest = ReadAPKChannel("朕的江山_test.apk");
            //WriteChannelToAPK("测试TestAndroid长长长长长长长长长长长长长长长长长长长长的渠道号", "TestAndroid.apk", "TestAndroid_test.apk");
            //string channelTest = ReadAPKChannel("TestAndroid_test.apk");
            //Console.WriteLine("渠道号：" + channelTest);
            //Console.WriteLine("渠道号长度：" + channelTest.Length);
            //Console.ReadKey();
            //return;

            Console.WriteLine("使用说明：");
            Console.WriteLine("1. 不传参数：读取当前文件夹下所有apk的渠道号");
            Console.WriteLine("2. 1个参数(指定apk包)：读取指定apk的渠道号");
            Console.WriteLine("3. 2个参数(指定apk包，渠道号)：为指定apk写入渠道号，若渠道号为空，则为当前文件夹下所有apk包重新写入已有的渠道号(母包更新)");

            if (args.Length == 0)
            {
                foreach (var item in Directory.GetFiles(Environment.CurrentDirectory, "*.apk"))
                    Console.WriteLine("渠道包[{0}]的渠道号：{1}", Path.GetFileNameWithoutExtension(item), ReadAPKChannel(item));
                Console.ReadKey();
                return;
            }
            string package = args[0];
            string packageName = Path.GetFileNameWithoutExtension(package);
            if (args.Length == 1)
            {
                Console.WriteLine("渠道包[{0}]的渠道号：{1}", package, ReadAPKChannel(package));
                return;
            }
            string channel = args[1];
            if (string.IsNullOrEmpty(channel))
            {
                Console.WriteLine("母包有修改，重新打包所有当前文件夹内已有的子包");
                foreach (var item in Directory.GetFiles(Environment.CurrentDirectory, "*.apk", SearchOption.AllDirectories))
                {
                    if (package == Path.GetFileName(item))
                        continue;
                    // hack: 若根目录有两个母包和其渠道包，这里会把母包覆盖掉其它的母包
                    WriteChannelToAPK(ReadAPKChannel(item), package, item);
                    Console.WriteLine("重新生成渠道包【{0}】成功", item);
                }
            }
            else
            {
                string output = string.Format("{0}_{1}.apk", packageName, channel);
                WriteChannelToAPK(channel, package, output);
                Console.WriteLine("生成渠道包【{0}】成功", output);
            }
        }
    }
}
