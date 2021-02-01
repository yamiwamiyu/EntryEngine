using System.Collections;

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

            props["Version"] = reader.ReadInt32();
            props["IsInverted"] = reader.ReadBoolean();
            props["IsNotLinked"] = reader.ReadBoolean();
            props["IsDisabled"] = reader.ReadBoolean();
            reader.ReadByte();

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
                        reader.ReadBytes(24);
                        break;

                    case 1:
                    case 2:
                    case 4:
                    case 5:
                        // 顶点信息
                        ArrayList pointList = new ArrayList();
                        for (int j = 0; j < 3; j++)
                        {
                            int x = reader.ReadInt32();
                            int y = reader.ReadInt32();
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
