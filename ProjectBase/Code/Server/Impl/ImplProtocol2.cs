using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine;

namespace Server.Impl
{
    class ImplProtocol2 : StubBase, _Protocol2
    {
        void _Protocol2.TestLogin(CBProtocol2_TestLogin callback)
        {
            callback.Callback(Player);
        }
    }
}
