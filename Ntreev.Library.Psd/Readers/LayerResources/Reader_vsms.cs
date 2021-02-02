using System.Collections;
using System;

namespace Ntreev.Library.Psd.Readers.LayerResources
{
    abstract class Reader_VectorPathDataResource : ResourceReaderBase
    {
        public Reader_VectorPathDataResource(PsdReader reader, long length)
            : base(reader, length)
        {

        }
        protected override void ReadValue(PsdReader reader, object userData, out IProperties value)
        {
            Properties props = new Properties(7);
            reader.Skip(8);

            int num = (int)(this.Length - 8) / 26;
            ArrayList path = new ArrayList(num);
            for (int i = 0; i < num; i++)
            {
                switch (reader.ReadInt16())
                {
                    case 0:
                    case 3:
                    case 6:
                    case 7:
                    case 8:
                        // 忽略掉其它信息
                        reader.Skip(24);
                        break;

                    case 1:
                    case 2:
                    case 4:
                    case 5:
                        // 顶点信息
                        ArrayList pointList = new ArrayList();
                        for (int j = 0; j < 3; j++)
                        {
                            double y = ReadBezierPoint(reader);
                            double x = ReadBezierPoint(reader);
                            pointList.Add(x);
                            pointList.Add(y);
                        }
                        path.Add(pointList);
                        break;
                }
            }
            props["Path"] = path;

            value = props;
        }
        /// <summary>获取点在psd文档中的绝对坐标，单位为文档的宽高的百分比</summary>
        private double ReadBezierPoint(PsdReader reader)
        {
            var byte1 = reader.ReadByte();
            bool isNegativ = (byte1 & 0xF0) != 0;
            byte intPart = (byte)(byte1 & 0x0F);
            int intVal;

            if (isNegativ)
            {
                intVal = intPart - 16;
            }
            else
            {
                intVal = intPart;
            }

            byte[] bytes = new byte[4];

            bytes[3] = 0;
            bytes[2] = reader.ReadByte();
            bytes[1] = reader.ReadByte();
            bytes[0] = reader.ReadByte();

            var fraction = BitConverter.ToInt32(bytes, 0);

            double ret = intVal + (double)fraction / 16777216.0;

            return ret;
        }
    }
    [ResourceID("vsms")]
    class Reader_vsms : Reader_VectorPathDataResource
    {
        public Reader_vsms(PsdReader reader, long length)
            : base(reader, length)
        {

        }
    }
    [ResourceID("vmsk")]
    class Reader_vmsk : Reader_VectorPathDataResource
    {
        public Reader_vmsk(PsdReader reader, long length)
            : base(reader, length)
        {

        }
    }
}
