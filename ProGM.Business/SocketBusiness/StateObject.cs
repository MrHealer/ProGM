
using System;
using System.Net.Sockets;
using System.Text;

namespace ProGM.Business.SocketBusiness
{
    public sealed class StateObject : IStateObject
    {
        /* Contains the state information. */

        private const int Buffer_Size = 1024;
        private readonly byte[] buffer = new byte[Buffer_Size];
        private readonly Socket listener;
        private readonly string ipAddress;
        private readonly DateTime timeAccept;
        private StringBuilder sb;

        public StateObject(Socket listener,string ipAddress = "")
        {
            this.listener = listener;
            this.timeAccept = DateTime.Now;
            this.ipAddress = ipAddress;
            this.Close = false;
            this.Reset();
        }

        public DateTime TimeAccept
        {
            get
            {
                return this.timeAccept;
            }
        }
        public string IpAddress
        {
            get
            {
                return this.ipAddress;
            }
        }

        public bool Close { get; set; }

        public int BufferSize
        {
            get
            {
                return Buffer_Size;
            }
        }

        public byte[] Buffer
        {
            get
            {
                return this.buffer;
            }
        }

        public Socket Listener
        {
            get
            {
                return this.listener;
            }
        }

        public string Text
        {
            get
            {
                return this.sb.ToString();
            }
        }

        public void Append(string text)
        {
            this.sb.Append(text);
        }

        public void Reset()
        {
            this.sb = new StringBuilder();
        }
    }
}
