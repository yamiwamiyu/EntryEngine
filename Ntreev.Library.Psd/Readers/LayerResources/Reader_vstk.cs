using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ntreev.Library.Psd.Readers.LayerResources
{
    /// <summary>形状图层描边信息</summary>
    [ResourceID("vstk")]
    class Reader_vstk : ResourceReaderBase
    {
        public Reader_vstk(PsdReader reader, long length)
            : base(reader, length)
        {

        }

        protected override void ReadValue(PsdReader reader, object userData, out IProperties value)
        {
            value = new DescriptorStructure(reader, true);
        }
    }
}
