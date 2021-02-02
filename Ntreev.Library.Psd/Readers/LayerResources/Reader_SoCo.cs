using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ntreev.Library.Psd.Readers.LayerResources
{
    [ResourceID("SoCo")]
    class Reader_SoCo : ResourceReaderBase
    {
        public Reader_SoCo(PsdReader reader, long length)
            : base(reader, length)
        {

        }
        protected override void ReadValue(PsdReader reader, object userData, out IProperties value)
        {
            value = new DescriptorStructure(reader, true);
        }
    }

    [ResourceID("vscg")]
    class Reader_vscg : Reader_SoCo
    {
        public Reader_vscg(PsdReader reader, long length)
            : base(reader, length)
        {
        }

        protected override void ReadValue(PsdReader reader, object userData, out IProperties value)
        {
            reader.ReadType();
            base.ReadValue(reader, userData, out value);
        }
    }
}
