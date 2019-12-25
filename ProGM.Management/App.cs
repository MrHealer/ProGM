using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ProGM.Management.Views.DangNhap;
using ProGM.Business.SocketBusiness;
using ProGM.Business.Model;
using Newtonsoft.Json;
using ProGM.Management.Views;
using System.Threading;
using DevExpress.XtraEditors;
using DevExpress.XtraBars.Navigation;
using ProGM.Management.FormState;
using DevExpress.Utils.Svg;
using ProGM.Management.Model;
using ProGM.Management.Views.TinhTrangHoatDong;
using ProGM.Management.Views.TaiKhoan;
using ProGM.Management.Views.NhatKyHeThong;
using ProGM.Management.Views.NhatKySuDung;
using ProGM.Management.Views.NhomNguoiDung;
using ProGM.Management.Views.NhomMay;
using System.Web.UI.WebControls;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using ProGM.Management.Controller;
using ProGM.Management.Views.Chat;
using ProGM.Business.ApiBusiness;
using Quobject.SocketIoClientDotNet.Client;
using Timer = System.Timers.Timer;

namespace ProGM.Management
{
    public partial class App : DevExpress.XtraEditors.XtraForm
    {
        MenuObject objMenu = new MenuObject();
        TinhTrang userTinhTrang;


        public IAsyncSocketListener asyncSocketListener;
        public List<SocketClients> clients = new List<SocketClients>();
        public IDictionary<int, Timer> lsTimerPay = new Dictionary<int, Timer>();
        Thread threadListen;
        public bool isVerifyAccount = false;
        public string ManagerLoginName = "";
        public string CompanyId = "";
        public App()
        {
            InitializeComponent();
            SocketEventRegistration();
        }
        #region socket.io Server
        Quobject.SocketIoClientDotNet.Client.Socket _socket;
        public void ConnectSocketToServer()
        {
            //_socket = IO.Socket("http://40.74.77.139:8888/");
            _socket = IO.Socket("http://125.212.225.24:8000");

            _socket.On(Quobject.SocketIoClientDotNet.Client.Socket.EVENT_CONNECT, () =>
            {
                //socket.Emit("hi");
                MessageBox.Show("Connect node js Ok");
            });
            _socket.On("hi", (data) =>
            {
                Console.WriteLine(data);
                _socket.Disconnect();
            });
        }
        #endregion
        #region socket event  

        private void SocketEventRegistration()
        {
            asyncSocketListener = AsyncSocketListener.Instance;
            asyncSocketListener.MessageReceived += AsyncSocketListener_MessageReceived;
            asyncSocketListener.Disconnected += AsyncSocketListener_Disconnected;
            threadListen = new Thread(new ThreadStart(asyncSocketListener.StartListening));
            threadListen.Start();
        }
        private void AsyncSocketListener_Disconnected(int id)
        {
            var _cl = this.clients.Where(n => n.id == id).SingleOrDefault();
            if (_cl != null)
            {
                this.userTinhTrang.UpdateStatusPC(_cl.macaddress, 0, "00:00:00");
                asyncSocketListener.Close(id);
                var tm = (Timer)lsTimerPay.Where(n => n.Key == id).SingleOrDefault().Value;
                tm.Enabled = false;
                tm.Dispose();
                lsTimerPay.Remove(id);
                this.clients = this.clients.Where(n => n.id != id).ToList();
            }
        }
        private void AsyncSocketListener_MessageReceived(int id, string msg)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<SocketReceivedData>(msg);
                switch (obj.type)
                {
                    #region AUTHORIZE
                    case SocketCommandType.AUTHORIZE:
                        SocketClients client = new SocketClients();
                        client.id = id;
                        client.macaddress = obj.macAddressFrom;
                        clients.Add(client);
                        if (this.userTinhTrang != null)
                        {
                            this.userTinhTrang.UpdateStatusPC(obj.macAddressFrom, 1, "00:00:00");
                        }
                        break;
                    #endregion

                    #region CHAT
                    case SocketCommandType.CHAT:
                        var _client = clients.Where(c => c.macaddress == obj.macAddressFrom).SingleOrDefault();
                        if (_client != null)
                        {
                            if (_client.frmChat == null || (_client.frmChat != null && _client.frmChat.Disposing))
                            {
                                _client.frmChat = new frmChat(id, this);

                            }
                            this.Invoke((Action)delegate
                            {
                                _client.frmChat.Text = obj.msgFrom;
                                _client.frmChat.UpdateHistory(obj.msgFrom + " Say: " + obj.msg + DateTime.Now.ToString("     HH:ss dd/MM/yyyy"));
                                _client.frmChat.Show();
                            });
                        }
                        break;
                    #endregion

                    #region LOGIN
                    case SocketCommandType.LOGIN:
                        string messeage = "";
                        LoginResponse loginResponse = RestshapCommand.Login(obj.username, obj.password, ref messeage);
                        SocketReceivedData ms = new SocketReceivedData();
                        if (loginResponse != null)
                        {

                            if (loginResponse.result[0].status == "SUCCESS")
                            {
                                #region đăng nhập thánh  công
                                var _clientsk = clients.Where(c => c.macaddress == obj.macAddressFrom).SingleOrDefault();
                                if (_clientsk != null)
                                {
                                    _clientsk.userLogin = obj.username;
                                    _clientsk.timerStart = DateTime.Now;
                                    _clientsk.accountBlance = loginResponse.result[0].dBalance;
                                    _clientsk.macaddress = obj.macAddressFrom;
                                    _clientsk.Price = decimal.Parse(this.userTinhTrang.datasource.Where(n => n.MacID == obj.macAddressFrom).SingleOrDefault().Price);

                                }

                                CreateJobPay(id, true);

                                var thoigianconlai = _clientsk.accountBlance / _clientsk.Price * 60;
                                ms.accountBlance = _clientsk.accountBlance;
                                ms.timeStart = _clientsk.timerStart;
                                ms.timeUpdate = DateTime.Now;
                                ms.timeUsed = _clientsk.timeUsed;
                                ms.timeRemaining = Decimal.ToInt32(thoigianconlai);
                                ms.price = _clientsk.Price;
                                ms.type = SocketCommandType.LOGIN_SUCCESS;

                                this.userTinhTrang.UpdateStatusPC(obj.macAddressFrom, 2, string.Format("{0:HH:mm:ss}", _clientsk.timerStart));


                                #endregion

                            }
                            else if (loginResponse.result[0].status == "FALSED")
                            {
                                ms.type = SocketCommandType.LOGIN_FALSED;
                                ms.msg = "Đăng nhập thất bại";
                            }
                        }
                        else
                        {
                            ms.type = SocketCommandType.LOGIN_FALSED;
                            ms.msg = "Đăng nhập thất bại";
                        }
                        this.asyncSocketListener.Send(id, JsonConvert.SerializeObject(ms), false);
                        break;
                    #endregion

                    default:
                        break;
                }

            }
            catch (Exception exx)
            {


            }
        }

        private void Timerpay_Tick(object sender, EventArgs e, int id)
        {
            Timer tt = sender as Timer;
            var _clientItem = clients.Where(n => n.id == id).SingleOrDefault();
            if (_clientItem != null)
            {
                _clientItem.timeUsed += 2;

                // trường hợp đăng nhập bằng tài khoản
                if (!string.IsNullOrEmpty(_clientItem.userLogin))
                {
                    _clientItem.accountBlance = _clientItem.accountBlance - (_clientItem.Price / 60 * 2);
                    var thoigianconlai = _clientItem.accountBlance / _clientItem.Price * 60;
                    if (thoigianconlai <= 0)
                    {
                        _clientItem.timerStart = DateTime.MinValue;
                        _clientItem.userLogin = "";
                        _clientItem.accountBlance = 0;
                        _clientItem.timeUsed = 0;
                        _clientItem.frmChat = null;
                        tt.Enabled = false;
                        tt.Dispose();
                        //hiển thị tiền ở client
                        SocketReceivedData ms = new SocketReceivedData();
                        ms.type = SocketCommandType.OUT_OF_MONEY;
                        this.asyncSocketListener.Send(id, JsonConvert.SerializeObject(ms), false);
                        this.userTinhTrang.UpdateStatusPC(_clientItem.macaddress, 1, "00:00:00");
                    }
                    else
                    {
                        SocketReceivedData ms = new SocketReceivedData();
                        ms.username = _clientItem.userLogin;
                        ms.accountBlance = _clientItem.accountBlance;
                        ms.timeStart = _clientItem.timerStart;
                        ms.timeUpdate = DateTime.Now;
                        ms.timeUsed = _clientItem.timeUsed;
                        ms.timeRemaining = Decimal.ToInt32(thoigianconlai);
                        ms.price = _clientItem.Price;
                        ms.type = SocketCommandType.UPDATE_INFO_USED;
                        this.asyncSocketListener.Send(id, JsonConvert.SerializeObject(ms), false);
                    }
                }
                //trường hợp mở máy
                else
                {

                    SocketReceivedData ms = new SocketReceivedData();
                    ms.msgFrom = "SERVER";
                    ms.timeStart = _clientItem.timerStart;
                    ms.timeUpdate = DateTime.Now;
                    ms.timeUsed = _clientItem.timeUsed;
                    ms.price = _clientItem.Price;
                    ms.type = SocketCommandType.UPDATE_INFO_USED;
                    this.asyncSocketListener.Send(id, JsonConvert.SerializeObject(ms), false);
                }
            }
        }
        #endregion

        #region form event
        private void App_Load(object sender, EventArgs e)
        {
            this.Hide();
            DangNhap frmlogin = new DangNhap(this);
            frmlogin.ShowDialog();
        }
        private void App_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.asyncSocketListener.Dispose();
            this.threadListen.Abort();
        }

        private void tileNavPaneMenu_ElementClick(object sender, NavElementEventArgs e)
        {
            if (e.Element.Name == "mTinhtrang")
            {

            }
        }

        #endregion

        #region Orther Method

        public void UpdateGui()
        {
            ngFrameMenu.Width = this.Width;
            ngFrameMenu.Height = this.Height;
            ngPage1.Width = this.Width;
            ngPage1.Height = this.Height;
            objMenu = new MenuObject();
            objMenu.frmHeight = this.Height;
            objMenu.frmWidth = this.Width;
            objMenu.frmMenuHeight = tileNavPaneMenu.Height;
            objMenu.frmMenuWidth = tileNavPaneMenu.Width;
            userTinhTrang = new TinhTrang(objMenu, this);
            ngPage1.Controls.Add(userTinhTrang);

        }

        public void CreateJobPay(int id, bool login = false)
        {
            Timer timerpay = new Timer();
            timerpay.Elapsed += (sender, eventArgs) => Timerpay_Tick(sender, eventArgs, id);
            timerpay.Interval = 5000;
            timerpay.Enabled = true;
            lsTimerPay.Add(id, timerpay);
        }


        #endregion


    }

    public class SocketClients
    {
        public int id { set; get; }
        public string macaddress { set; get; }
        public frmChat frmChat { set; get; }
        public DateTime timerStart { set; get; }
        public string userLogin { set; get; }
        public decimal accountBlance { set; get; }
        public decimal Price { set; get; }

        public int timeUsed { set; get; }
    }
}
