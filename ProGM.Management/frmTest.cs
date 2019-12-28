using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Quobject.SocketIoClientDotNet.Client;

namespace ProGM.Management
{
    public partial class frmTest : Form
    {
        public frmTest()
        {
            InitializeComponent();
        }
        Socket socket;

        private void frmTest_Load(object sender, EventArgs e)
        {
            this.socket = IO.Socket("http://40.74.77.139:8888");
            this.socket.On(Socket.EVENT_CONNECT, () =>
            {
                Console.WriteLine("Connect OK");
                var use2r = new test1();
                use2r.idUser = "984f2670-2561-11ea-b536-005056b97a5d";
                use2r.userName = "gammer02";
                string jsonRequest = JsonConvert.SerializeObject(use2r);
                this.socket.Emit("registration-user", jsonRequest);
            });
            this.socket.On("registration-user-status", (data) =>
            {
                Console.WriteLine("registration-user-status: " + data);
            });
        }
    }
    public class test1
    {
        public string idUser { set; get; }
        public string userName { set; get; }
    }
}
