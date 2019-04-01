using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine;
using EntryEngine.Network;

namespace Server
{
    partial class Service : ProxyTcpAsync
    {
        protected override IEnumerator<LoginResult> Login(Link link)
        {
            throw new NotImplementedException();
        }

        protected override void OnUpdate(GameTime time)
        {
            throw new NotImplementedException();
        }
    }
}
