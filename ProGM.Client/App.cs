using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using ProGM.Business.SocketBusiness;
using ProGM.Client.View.Chat;
using ProGM.Client.View.Login;
using ProGM.Client.View.GoiDo;
using ProGM.Business.Model;
using ProGM.Business.ApiBusiness;

namespace ProGM.Client
{
    public partial class App : DevExpress.XtraEditors.XtraForm
    {
        public IAsyncClient asyncClient;
        public frmChat frmChat;
        public frmDangNhap frmDangNhap;
        public frmLock frmLock;

        bool isVerifyAccount = false;
        bool isConnectServer = false;
        public string ComputerDetail = "";
        string IpManager = "";
        public App()
        {
            string mac = PCExtention.GetMacId();
            var detail = RestshapCommand.ComputerDetail(mac);
            if (detail != null && detail != null && detail.computeDetail.Count() > 0)
            {
                this.ComputerDetail = JsonConvert.SerializeObject(detail);
                this.IpManager = detail.computeDetail[0].strManagerPcIP;
            }

            InitializeComponent();
            this.Location = new Point(Screen.PrimaryScreen.Bounds.Right - this.Width, //should be (0,0)
                          Screen.PrimaryScreen.Bounds.Y);
            this.TopMost = true;
            this.StartPosition = FormStartPosition.Manual;
            // kết nối tới máy trạm
            asyncClient = new AsyncClient();
            asyncClient.Connected += AsyncClient_Connected;
            asyncClient.MessageReceived += AsyncClient_MessageReceived;
            asyncClient.MessageSubmitted += AsyncClient_MessageSubmitted;
            asyncClient.Disconnected += AsyncClient_Disconnected;
            new Thread(new ThreadStart(asyncClient.StartClient)).Start();

        }


        #region event socket
        private void AsyncClient_MessageSubmitted(IAsyncClient a, bool close)
        {
            //throw new NotImplementedException();
        }
        private void AsyncClient_Connected(IAsyncClient a)
        {
            isConnectServer = true;
            resgisterMac();
            asyncClient.Receive();
        }
        private void AsyncClient_Disconnected()
        {

            isConnectServer = false;
            this.Invoke((Action)delegate
            {
                this.Hide();
                if (this.frmDangNhap != null)
                {
                    this.frmLock.Show();
                    this.frmDangNhap.ShowDialog();
            
                    MessageBox.Show("Không thể sử dụng dịch vụ vào lúc này!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            });
        }
        private void AsyncClient_MessageReceived(IAsyncClient a, string msg)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<SocketReceivedData>(msg);

                switch (obj.type)
                {
                    #region AUTHORIZE
                    case SocketCommandType.AUTHORIZE:

                        break;
                    #endregion

                    #region CHAT
                    case SocketCommandType.CHAT:
                        if (this.frmChat == null || (this.frmChat != null && this.frmChat.Disposing))
                        {
                            this.frmChat = new frmChat(this);

                        }
                        this.Invoke((Action)delegate
                        {
                            this.frmChat.UpdateHistory(obj.msgFrom + " Say: " + obj.msg + DateTime.Now.ToString("     HH:ss dd/MM/yyyy"));
                            this.frmChat.Show();
                        });
                        break;
                    #endregion

                    #region OPENCLIENT
                    case SocketCommandType.OPENCLIENT:
                        this.Invoke((Action)delegate
                        {
                            this.Show();
                            if (this.frmDangNhap != null)
                            {
                                this.frmDangNhap.Hide();
                            }
                            if (this.frmLock != null)
                            {
                                this.frmLock.Hide();
                            }
                        });
                        break;
                    #endregion

                    #region CLOSECLIENT
                    case SocketCommandType.CLOSECLIENT:

                        this.Invoke((Action)delegate
                        {
                            this.Hide();
                            if (this.frmLock != null)
                            {
                                this.frmLock.Show();
                            }
                            if (this.frmDangNhap != null)
                            {
                                this.frmDangNhap.TopMost = true;
                                this.frmDangNhap.btnLogin.Enabled = true;
                                this.frmDangNhap.Show();
                            }
                           
                        });


                        break;
                    #endregion

                    #region LOGIN_SUCCESS
                    case SocketCommandType.LOGIN_SUCCESS:
                        this.Invoke((Action)delegate
                        {
                            this.Show();
                            if (this.frmDangNhap != null)
                            {
                                this.frmDangNhap.Hide();
                            }
                            if (this.frmLock != null)
                            {
                                this.frmLock.Hide();
                            }
                        });
                        break;
                    #endregion

                    #region LOGIN_FALSED
                    case SocketCommandType.LOGIN_FALSED:
                        MessageBox.Show(obj.msg, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        break;
                    #endregion

                    default:
                        break;

                }

            }
            catch (Exception)
            {


            }
        }


        #endregion

        #region form event

        private void App_Load(object sender, EventArgs e)
        {

            this.frmLock = new frmLock(this);
            this.Hide();
            this.frmLock.ShowDialog();
        }
        private void btnOpenChat_Click(object sender, EventArgs e)
        {
            this.frmChat = (frmChat)Application.OpenForms["frmChat"];
            if (this.frmChat == null)
            {
                this.frmChat = new frmChat(this);
            }
            this.frmChat.TopMost = true;
            this.frmChat.Show();
        }

        #endregion

        #region  other method

        public void resgisterMac()
        {
            string macaddress = PCExtention.GetMacId();
            SocketReceivedData ms = new SocketReceivedData();
            ms.msgFrom = "Linh";
            ms.msgTo = "SERVER";
            ms.macAddressFrom = macaddress;
            ms.type = SocketCommandType.AUTHORIZE;
            this.asyncClient.Send(JsonConvert.SerializeObject(ms), false);
        }

        #endregion

        private void btnOrder_Click(object sender, EventArgs e)
        {
            frmGoiDo _frmGoiDo = new frmGoiDo();
            _frmGoiDo.Show();
        }
    }
}
