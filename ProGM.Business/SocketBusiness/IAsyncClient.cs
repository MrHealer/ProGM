﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProGM.Business.SocketBusiness
{ 
    public interface IAsyncClient : IDisposable
    {
        event ConnectedHandler Connected;

        event ClientMessageReceivedHandler MessageReceived;

        event ClientMessageSubmittedHandler MessageSubmitted;

        void StartClient();

        bool IsConnected();

        void Receive();

        void Send(string msg, bool close);
    }
}