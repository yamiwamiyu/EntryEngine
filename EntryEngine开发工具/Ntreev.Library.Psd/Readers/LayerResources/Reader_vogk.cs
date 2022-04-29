using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ntreev.Library.Psd.Readers.LayerResources
{
    /// <summary>形状图层形状信息</summary>
    [ResourceID("vogk")]
    class Reader_vogk : ResourceReaderBase
    {
        public Reader_vogk(PsdReader reader, long length)
            : base(reader, length)
        {

        }
        protected override void ReadValue(PsdReader reader, object userData, out IProperties value)
        {
            reader.Skip(8);
            value = new DescriptorStructure(reader, false);
        }
    }
}
