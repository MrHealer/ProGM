
using ProGM.Business.Extention;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ProGM.Business.SocketBusiness
{
    public delegate void MessageReceivedHandler(string ipAddress, string msg);
    public delegate void MessageSubmittedHandler(string ipAddress, bool close);
    public delegate void DisconnectedHandler(string ipAddress);

    public sealed class AsyncSocketListener : IAsyncSocketListener
    {
        private const ushort Port = 8000;
        private const ushort Limit = 250;

        private static readonly IAsyncSocketListener instance = new AsyncSocketListener();

        private readonly ManualResetEvent mre = new ManualResetEvent(false);
        private readonly IDictionary<string, IStateObject> clients = new Dictionary<string, IStateObject>();

        public event MessageReceivedHandler MessageReceived;
        public event MessageSubmittedHandler MessageSubmitted;
        public event DisconnectedHandler Disconnected;

        private AsyncSocketListener()
        {
        }

        public static IAsyncSocketListener Instance
        {
            get
            {
                return instance;
            }
        }

        /* Starts the AsyncSocketListener */
        public void StartListening()
        {
            string IP = PCExtention.GetLocalIPAddress();
            var endpoint = new IPEndPoint(IPAddress.Parse(IP), Port);
            try
            {
                using (var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    listener.Bind(endpoint);
                    listener.Listen(Limit);
                    Console.WriteLine("=>> Server start listen in  " + IP + ":" + Port);
                    while (true)
                    {
                        this.mre.Reset();
                        listener.BeginAccept(this.OnClientConnect, listener);
                        this.mre.WaitOne();
                    }
                }
            }
            catch (SocketException)
            {
                // TODO:
            }
        }
        /* Gets a socket from the clients dictionary by his Id. */
        private IStateObject GetClient(string ipaddress)
        {
            IStateObject state;

            return this.clients.TryGetValue(ipaddress, out state) ? state : null;
        }

        /* Checks if the socket is connected. */
        public bool IsConnected(string ipaddress)
        {
            var state = this.GetClient(ipaddress);

            return !(state.Listener.Poll(1000, SelectMode.SelectRead) && state.Listener.Available == 0);
        }

        /* Add a socket to the clients dictionary. Lock clients temporary to handle multiple access.
         * ReceiveCallback raise a event, after the message receive complete. */
        #region Receive data
        public void OnClientConnect(IAsyncResult result)
        {
            try
            {
                this.mre.Set();

                IStateObject state;

                lock (this.clients)
                {
                    var socket = ((Socket)result.AsyncState).EndAccept(result);

                    string IP = ((IPEndPoint)socket.RemoteEndPoint).Address.ToString();
                    var checkExit = this.clients.Where(n => n.Key == IP).SingleOrDefault();
                    if (this.clients.ContainsKey(IP))
                    {
                        this.clients[IP].Listener.Close();
                        this.clients[IP].Listener.Dispose();
                        state = new StateObject(socket, IP);
                        this.clients[IP] = state;
                    }
                    else
                    {
                        state = new StateObject(socket, IP);
                        this.clients.Add(IP, state);
                    }
                    Console.WriteLine("==> Client connected. IP  " + IP);
                }

                state.Listener.BeginReceive(state.Buffer, 0, state.BufferSize, SocketFlags.None, new AsyncCallback(this.ReceiveCallback), state);
            }
            catch (SocketException ex)
            {
                // TODO:
            }
            catch (Exception x)
            {

            }
        }
        public void ReceiveCallback(IAsyncResult result)
        {
            var state = (IStateObject)result.AsyncState;

            try
            {
                var receive = state.Listener.EndReceive(result);

                if (receive > 0)
                {
                    state.Append(Encoding.UTF8.GetString(state.Buffer, 0, receive));
                }


                state.Listener.BeginReceive(state.Buffer, 0, state.BufferSize, SocketFlags.None, new AsyncCallback(this.ReceiveCallback), state);

                var messageReceived = this.MessageReceived;

                if (messageReceived != null)
                {
                    messageReceived(state.IpAddress, state.Text);
                }

                state.Reset();

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
                        disconnected(state.IpAddress);
                    }

                    state.Listener.Close();

                }
            }
            catch (Exception)
            {

            }
        }
        #endregion

        /* Send(int id, String msg, bool close) use bool to close the connection after the message sent. */
        #region Send data
        public void Send(string ipaddress , string msg, bool close)
        {
            var state = this.GetClient(ipaddress);

            if (state == null)
            {
                throw new Exception("Client does not exist.");
            }

            if (!this.IsConnected(state.IpAddress))
            {
                throw new Exception("Destination socket is not connected.");
            }

            try
            {
                var send = Encoding.UTF8.GetBytes(msg);

                state.Close = close;
                state.Listener.BeginSend(send, 0, send.Length, SocketFlags.None, this.SendCallback, state);
            }
            catch (SocketException)
            {
                // TODO:
            }
            catch (ArgumentException)
            {
                // TODO:
            }
        }

        private void SendCallback(IAsyncResult result)
        {
            var state = (IStateObject)result.AsyncState;

            try
            {
                state.Listener.EndSend(result);
            }
            catch (SocketException)
            {
                // TODO:
            }
            catch (ObjectDisposedException)
            {
                // TODO:
            }
            finally
            {
                var messageSubmitted = this.MessageSubmitted;

                if (messageSubmitted != null)
                {
                    messageSubmitted(state.IpAddress, state.Close);
                }
            }
        }
        #endregion
        public void Close(string ipaddress)
        {
            var state = this.GetClient(ipaddress);

            if (state == null)
            {
                throw new Exception("Client does not exist.");
            }

            try
            {
                state.Listener.Shutdown(SocketShutdown.Both);
                state.Listener.Close();
            }
            catch (SocketException)
            {
                // TODO:
            }
            finally
            {
                lock (this.clients)
                {
                    this.clients.Remove(state.IpAddress);
                    Console.WriteLine("==> Client disconnected with IP {0}", state.IpAddress);
                }
            }
        }
        public void Dispose()
        {
            var lsClient = this.clients.ToList();
            lock(lsClient)
            {
                if (lsClient!=null && lsClient.Count>0)
                {
                    foreach (var id in lsClient)
                    {
                        this.Close(id.Key);
                    }
                }
               
            }
            this.mre.Dispose();
        }
    }
}
