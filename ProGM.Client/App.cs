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
using ProGM.Business.Extention;
using System.Media;
using System.Globalization;


namespace ProGM.Client
{
    public partial class App : DevExpress.XtraEditors.XtraForm
    {
        public IAsyncClient asyncClient;
        public frmChat frmChat;
        public frmDangNhap frmDangNhap;
        public frmLock frmLock;
        public frmGoiDo frmGoiDo;

        bool isVerifyAccount = false;
        public bool isConnectServer = false;
        public string ComputerDetail = "";
        int timeWarning = 0;
        public string ComputerName = "";
        Thread threadListen;

        public string ManagerPcIP = "";

        string IdUserLogin = "";

        public App()
        {
            string mac = PCExtention.GetMacId();
            Logger.WriteLog(Logger.LogType.Error, ManagerPcIP);
            var detail = RestshapCommand.ComputerDetail(mac);
            if (detail != null && detail != null && detail.computeDetail.Count() > 0)
            {
                this.ComputerDetail = JsonConvert.SerializeObject(detail);
                this.ManagerPcIP = detail.computeDetail[0].strManagerPcIP;
                this.ComputerName = detail.computeDetail[0].strName;
                lbComputerName.Text = ComputerName;
            }

            InitializeComponent();
            this.Location = new Point(Screen.PrimaryScreen.Bounds.Right - this.Width, //should be (0,0)
                          Screen.PrimaryScreen.Bounds.Y);
            //this.TopMost = true;
            this.StartPosition = FormStartPosition.Manual;
            // kết nối tới máy trạm
            regiterClientConnect(ManagerPcIP);

        }

        #region event socket

        private void regiterClientConnect(string ManagerPcIP)
        {
            asyncClient = new AsyncClient();
            asyncClient.Connected += AsyncClient_Connected;
            asyncClient.MessageReceived += AsyncClient_MessageReceived;
            asyncClient.Disconnected += AsyncClient_Disconnected;
            threadListen = new Thread(() => asyncClient.StartClient(ManagerPcIP));
            threadListen.Start();
        }
        private void AsyncClient_Connected(IAsyncClient a)
        {
            isConnectServer = true;
            resgisterMac();
            asyncClient.Receive();
            if (frmDangNhap != null)
            {

                if (frmDangNhap.InvokeRequired)
                {
                    frmDangNhap.Invoke((Action)delegate
                    {
                        frmDangNhap.lbMesseage.Text = "";
                        frmDangNhap.btnLogin.Enabled = true;
                    });
                }
                else
                {
                    frmDangNhap.lbMesseage.Text = "";
                    frmDangNhap.btnLogin.Enabled = true;
                }
            }

        }
        private void AsyncClient_Disconnected()
        {

            isConnectServer = false;
            if (this.InvokeRequired)
            {
                this.Invoke((Action)delegate
                {
                    this.Hide();
                });
            }
            else
            {
                this.Hide();
            }

            if (this.frmDangNhap != null)
            {
                asyncClient.Dispose();
                threadListen.Abort();

                if (frmDangNhap.InvokeRequired)
                {
                    frmDangNhap.Invoke((Action)delegate
                    {
                        frmDangNhap.lbMesseage.Text = "Không thể sử dụng dịch vụ vào lúc này";
                        frmDangNhap.btnLogin.Enabled = false;
                    });
                }
                else
                {
                    frmDangNhap.lbMesseage.Text = "Không thể sử dụng dịch vụ vào lúc này";
                    frmDangNhap.btnLogin.Enabled = false;
                }


                regiterClientConnect(this.ManagerPcIP);
                if (this.frmLock.InvokeRequired)
                {
                    this.frmLock.Invoke((Action)delegate
                    {
                        this.frmLock.Show();
                    });
                }
                else
                {
                    this.frmLock.Show();
                }
                

                if (!this.frmDangNhap.Modal)
                {
                    if (this.frmDangNhap.InvokeRequired)
                    {
                        this.frmDangNhap.Invoke((Action)delegate
                        {
                            this.frmDangNhap.ShowDialog();
                        });
                    }
                    else
                    {
                        this.frmDangNhap.ShowDialog();
                    }
                }

            }

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
                        });

                        if (this.frmDangNhap.InvokeRequired)
                        {
                            frmDangNhap.Invoke((Action)delegate { this.frmDangNhap.Hide(); });
                        }
                        else
                        {
                            this.frmDangNhap.Hide();
                        }
                        if (this.frmLock.InvokeRequired)
                        {
                            frmLock.Invoke((Action)delegate { this.frmLock.Hide(); });
                        }
                        else
                        {
                            this.frmLock.Hide();
                        }
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

                                this.frmDangNhap.btnLogin.Enabled = true;
                                this.frmDangNhap.ShowDialog();
                            }

                        });


                        break;
                    #endregion

                    #region LOGIN_SUCCESS
                    case SocketCommandType.LOGIN_SUCCESS:


                        this.Invoke((Action)delegate
                        {
                            var UserDetail = RestshapCommand.AccountDetail(obj.idUser).accountDetails[0];
                            lbUserName.Text = UserDetail.strFullName + "(" + UserDetail.strName + ")";
                            this.IdUserLogin = UserDetail.strId;

                            lbAccountBlance.Text = FormatExtention.Money(obj.accountBlance.ToString());
                            lbTimeStart.Text = obj.timeStart.ToString("HH:mm:ss");
                            lbTimeUser.Text = FormatExtention.FormartMinute(obj.timeUsed);
                            lbTimeRemaining.Text = FormatExtention.FormartMinute(obj.timeRemaining);
                            lbPrice.Text = FormatExtention.Money(obj.price.ToString());lbUserName.Text = obj.username ?? this.ComputerName;
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
                        if (this.frmDangNhap.btnLogin.InvokeRequired)
                        {
                            this.frmDangNhap.btnLogin.Invoke((Action)delegate { this.frmDangNhap.btnLogin.Enabled = true; });
                        }
                        else
                        {
                            this.frmDangNhap.btnLogin.Enabled = true;
                        }
                       
                        break;
                    #endregion

                    #region UPDATE_INFO_USED
                    case SocketCommandType.UPDATE_INFO_USED:
                        this.Invoke((Action)delegate
                        {

                            lbTimeStart.Text = obj.timeStart.ToString("HH:mm:ss");
                            lbTimeUser.Text = FormatExtention.FormartMinute(obj.timeUsed);
                            lbPrice.Text = FormatExtention.Money(obj.price.ToString());

                            if (string.IsNullOrEmpty(obj.username))
                            {
                                var thanhtien = obj.price / 60 * obj.timeUsed;
                                lbTimeRemaining.Text = FormatExtention.Money(thanhtien.ToString());
                                lbTimeRemainingTitle.Text = "Thành tiền";
                            }
                            else
                            {

                                lbTimeRemaining.Text = FormatExtention.FormartMinute(obj.timeRemaining);
                                lbAccountBlance.Text = FormatExtention.Money(obj.accountBlance.ToString());
                                if (obj.timeRemaining == 6 || obj.timeRemaining == 7)
                                {
                                    timeWarning = obj.timeRemaining;
                                    timerWarning.Interval = 5000;
                                    timerWarning.Enabled = true;
                                }
                            }
                        });

                        break;
                    #endregion

                    #region OUT_OF_MONEY
                    case SocketCommandType.OUT_OF_MONEY:
                        this.Invoke((Action)delegate
                        {
                            //lbAccountBlance.Text = obj.accountBlance.ToString();
                            //lbTimeStart.Text = obj.timeStart.ToString("HH:mm:ss");
                            //lbTimeUser.Text = obj.timeUsed.ToString();
                            //lbTimeRemaining.Text = obj.timeRemaining.ToString();
                            //lbPrice.Text = obj.price.ToString();
                            this.Hide();
                            if (this.frmLock != null)
                            {
                                //this.frmLock.TopMost = true;
                                this.frmLock.Show();
                            }
                            if (this.frmDangNhap != null)
                            {
                                this.frmDangNhap.btnLogin.Enabled = true;
                                this.frmDangNhap.ShowDialog();
                            }

                        });
                        break;
                    #endregion

                    #region UPDATE_TOTAL_MONEY
                    case SocketCommandType.UPDATE_TOTAL_MONEY:
                        this.lbAccountBlance.Text = FormatExtention.Money(obj.accountBlance.ToString());
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
            this.frmLock.Show();
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

        private void btnOrder_Click(object sender, EventArgs e)
        {
            if (this.frmGoiDo == null || this.frmGoiDo.IsDisposed)
            {
                this.frmGoiDo = new frmGoiDo();

            }
            this.frmGoiDo.Show();
        }


        private void btnLogout_Click(object sender, EventArgs e)
        {

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon.Visible = false;
        }

        private void App_Resize(object sender, EventArgs e)
        {

            if (this.WindowState == FormWindowState.Minimized)
            {
                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(3000);
                this.ShowInTaskbar = false;
            }
        }

        private void App_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            e.Cancel = true;
        }
        System.Media.SoundPlayer player = new System.Media.SoundPlayer();
        private void timerWarning_Tick(object sender, EventArgs e)
        {
            if (timeWarning <= 5)
            {
                switch (timeWarning)
                {
                    case 5:
                        SoundPlayer audio5 = new SoundPlayer(Properties.Resources._5phut);
                        audio5.Play();
                        break;
                    case 4:
                        SoundPlayer audio4 = new SoundPlayer(Properties.Resources._4phut);
                        audio4.Play();
                        break;
                    case 3:
                        SoundPlayer audio3 = new SoundPlayer(Properties.Resources._3phut);
                        audio3.Play();
                        break;
                    case 2:
                        SoundPlayer audio2 = new SoundPlayer(Properties.Resources._2phut);
                        audio2.Play();
                        break;
                    case 1:
                        SoundPlayer audio1 = new SoundPlayer(Properties.Resources._1phut);
                        audio1.Play();
                        break;
                    case 0:
                        timerWarning.Enabled = false;
                        break;
                    default:
                        break;
                }
            }
            timeWarning--;
        }
        #endregion

        #region  other method

        public void resgisterMac()
        {
            string macaddress = PCExtention.GetMacId();
            SocketReceivedData ms = new SocketReceivedData();
            ms.msgFrom = this.ComputerName;
            ms.msgTo = "SERVER";
            ms.macAddressFrom = macaddress;
            ms.type = SocketCommandType.AUTHORIZE;
            this.asyncClient.Send(JsonConvert.SerializeObject(ms), false);
        }

        #endregion

    }
}
