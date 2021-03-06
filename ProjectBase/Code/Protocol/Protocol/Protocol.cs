﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine.Network;

//[ProtocolStub(1, typeof(DemoProtocolCallback))]
//public interface DemoProtocol
//{
//    void GameRequest1(int param1, string[] param2, TimeSpan param3, Action<DateTime> callback);
//    void GameRequest2();
//}
//public interface DemoProtocolCallback
//{
//    void OnGameRequest2(DateTime now);
//}

[ProtocolStub(1, null)]
public interface Protocol1
{
    void PlayerExists(string name, Action<bool> callback);
    void Register(string name, string password, Action<T_PLAYER> callback);
    void Login(string name, string password, Action<T_PLAYER> callback);
}

[ProtocolStub(2, null)]
public interface Protocol2
{
    void TestLogin(Action<T_PLAYER> callback);
}