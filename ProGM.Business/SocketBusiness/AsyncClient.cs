﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ProGM.Business.SocketBusiness
{
    public delegate void ConnectedHandler(IAsyncClient a);
    public delegate void ClientMessageReceivedHandler(IAsyncClient a, string msg);
    public delegate void ClientMessageSubmittedHandler(IAsyncClient a, bool close);
    public delegate void ServerDisconnectedHandler();


    public sealed class AsyncClient : IAsyncClient
    {
        private const ushort Port = 8000;

        private Socket listener;
        private bool close;

        private readonly ManualResetEvent connected = new ManualResetEvent(false);
        private readonly ManualResetEvent sent = new ManualResetEvent(false);
        private readonly ManualResetEvent received = new ManualResetEvent(false);

        public event ConnectedHandler Connected;
        public event ServerDisconnectedHandler Disconnected;
        public event ClientMessageReceivedHandler MessageReceived;
        public event ClientMessageSubmittedHandler MessageSubmitted;

        public void StartClient()
        {
            var host = Dns.GetHostEntry(string.Empty);
            var ip = IPAddress.Parse("127.0.0.1");
            var endpoint = new IPEndPoint(ip, Port);

            try
            {
                this.listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.listener.BeginConnect(endpoint, this.OnConnectCallback, this.listener);
                this.connected.WaitOne();

                var connectedHandler = this.Connected;

                if (connectedHandler != null)
                {
                    connectedHandler(this);
                }
            }
            catch (SocketException)
            {
                // TODO:
            }
        }

        public bool IsConnected()
        {
            return !(this.listener.Poll(1000, SelectMode.SelectRead) && this.listener.Available == 0);
        }

        private void OnConnectCallback(IAsyncResult result)
        {
            var server = (Socket)result.AsyncState;

            try
            {
                server.EndConnect(result);
                this.connected.Set();
            }
            catch (SocketException)
            {
            }
        }

        #region Receive data
        public void Receive()
        {
            var state = new StateObject(this.listener);

            state.Listener.BeginReceive(state.Buffer, 0, state.BufferSize, SocketFlags.None, this.ReceiveCallback, state);
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            var state = (IStateObject)result.AsyncState;
            try
            {
                var receive = state.Listener.EndReceive(result);

                if (receive > 0)
                {
                    state.Append(Encoding.UTF8.GetString(state.Buffer, 0, receive));
                }


                state.Listener.BeginReceive(state.Buffer, 0, state.BufferSize, SocketFlags.None, this.ReceiveCallback, state);

                var messageReceived = this.MessageReceived;

                if (messageReceived != null)
                {
                    messageReceived(this, state.Text);
                }

                state.Reset();
                this.received.Set();
            }
            catch (SocketException socketException)
            {
                //WSAECONNRESET, the other side closed impolitely
                if (socketException.ErrorCode == 10054 || ((socketException.ErrorCode != 10004) && (socketException.ErrorCode != 10053)))
                {
                    // Complete the disconnect request.
                    var disconnected = this.Disconnected;

                    if (disconnected != null)
                    {
                        disconnected();
                    }
                    state.Listener.Close();

                }
            }
            catch (NullReferenceException e)
            {
                // Do something with e, please.
            }
            catch (Exception ex)
            {

            }
            


        }
        #endregion

        #region Send data
        public void Send(string msg, bool close)
        {
            if (!this.IsConnected())
            {
                throw new Exception("Destination socket is not connected.");
            }

            var response = Encoding.UTF8.GetBytes(msg);

            this.close = close;
            this.listener.BeginSend(response, 0, response.Length, SocketFlags.None, this.SendCallback, this.listener);
        }

        private void SendCallback(IAsyncResult result)
        {
            try
            {
                var resceiver = (Socket)result.AsyncState;

                resceiver.EndSend(result);
            }
            catch (SocketException)
            {
                // TODO:
            }
            catch (ObjectDisposedException)
            {
                // TODO;
            }

            var messageSubmitted = this.MessageSubmitted;

            if (messageSubmitted != null)
            {
                messageSubmitted(this, this.close);
            }

            this.sent.Set();
        }
        #endregion

        private void Close()
        {
            try
            {
                if (!this.IsConnected())
                {
                    return;
                }

                this.listener.Shutdown(SocketShutdown.Both);
                this.listener.Close();
            }
            catch (SocketException)
            {
                // TODO:
            }
        }

        public void Dispose()
        {
            this.sent.Dispose();
            this.received.Dispose();
            this.connected.Dispose();
           
            
            this.Close();
        }
    }
}
