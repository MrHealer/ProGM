using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProGM.Business.SocketBusiness
{
    public interface IAsyncSocketListener : IDisposable
    {
        event MessageReceivedHandler MessageReceived;

        event MessageSubmittedHandler MessageSubmitted; 
        event DisconnectedHandler Disconnected;

        
        void StartListening();

        bool IsConnected(string ipaddress);

        void OnClientConnect(IAsyncResult result);

        void ReceiveCallback(IAsyncResult result);

        void Send(string ipaddress, string msg, bool close);

        void Close(string ipaddress);
    }
}
