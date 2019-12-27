using SuperSocket.ClientEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocket4Net;

namespace ProGM.Business.SocketServer
{
    public class WebSocketManager
    {
        private AutoResetEvent messageReceiveEvent = new AutoResetEvent(false);
        private string lastMessageReceived;
        private WebSocket webSocket;

        public WebSocketManager(string webSocketUri)
        {
            Console.WriteLine("Initializing websocket. Uri: " + webSocketUri);
            webSocket = new WebSocket(webSocketUri);
            webSocket.Opened += new EventHandler(websocket_Opened);
            webSocket.Closed += new EventHandler(websocket_Closed);
            webSocket.Error += new EventHandler<ErrorEventArgs>(websocket_Error);
            webSocket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(websocket_MessageReceived);

            webSocket.Open();
            while (webSocket.State == WebSocketState.Connecting) { };   // by default webSocket4Net has AutoSendPing=true, 
                                                                        // so we need to wait until connection established
            if (webSocket.State != WebSocketState.Open)
            {
                throw new Exception("Connection is not opened.");
            }
        }

        public string Send(string data)
        {
            Console.WriteLine("Client wants to send data:");
            Console.WriteLine(data);

            webSocket.Send(data);
            if (!messageReceiveEvent.WaitOne(5000))                         // waiting for the response with 5 secs timeout
                Console.WriteLine("Cannot receive the response. Timeout.");

            return lastMessageReceived;
        }

        public void Close()
        {
            Console.WriteLine("Closing websocket...");
            webSocket.Close();
        }

        private void websocket_Opened(object sender, EventArgs e)
        {
            Console.WriteLine("Websocket is opened.");

            string json1 = @"
            {
            ""event"": ""registration-user"",
            ""data"": {
                 ""idUser"": ""5f543a6d-2560-11ea-b536-005056b97a5d"",
                 ""userName"": ""admin""
            } }";

            string json2 = @"
            {
            ""method"": ""registration-pc"",
            ""params"": {
                 ""mac"": ""30:85:a9:1d:a7:51"",
                 ""userName"": ""admin""
            } }";
            Send("xin chào");
            //Send("\"registration-user\", {\"idUser\": \"5f543a6d-2560-11ea-b536-005056b97a5d\", \"userName\": \"admin\"}");
            //Send("\"registration-pc\", {\"mac\": \"30:85:a9:1d:a7:51\", \"userName\": \"admin\"}");
        }
        private void websocket_Error(object sender, ErrorEventArgs e)
        {
            Console.WriteLine(e.Exception.Message);
        }
        private void websocket_Closed(object sender, EventArgs e)
        {
            Console.WriteLine("Websocket is closed.");
        }

        private void websocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Console.WriteLine("Message received: " + e.Message);
            lastMessageReceived = e.Message;
            messageReceiveEvent.Set();
        }
    }
}

