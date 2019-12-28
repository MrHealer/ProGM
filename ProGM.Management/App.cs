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
using DevExpress.XtraBars.Navigation;
using ProGM.Management.Model;
using ProGM.Management.Views.TinhTrangHoatDong;
using ProGM.Management.Views.TaiKhoan;
using ProGM.Management.Views.NhatKyHeThong;
using ProGM.Management.Views.NhatKySuDung;
using ProGM.Management.Views.NhomNguoiDung;
using ProGM.Management.Views.NhomMay;
using ProGM.Management.Views.Chat;
using ProGM.Business.ApiBusiness;
using Timer = System.Timers.Timer;
using Quobject.SocketIoClientDotNet.Client;
using Newtonsoft.Json.Linq;
using ProGM.Business.Extention;

namespace ProGM.Management
{
    public partial class App : DevExpress.XtraEditors.XtraForm
    {
        #region private param 
        MenuObject objMenu = new MenuObject();
        TinhTrang userTinhTrang;
        Thread threadListen;
        Socket socket;
        #endregion

        #region public param 
        public bool isVerifyAccount = false;
        public string ManagerLoginName = "";
        public string ManagerDisplayName = "";
        public string ManagerLoginId = "";
        public string CompanyId = "";
        public IAsyncSocketListener asyncSocketListener;
        public List<SocketClients> clients = new List<SocketClients>();
        public IDictionary<string, Timer> lsTimerPay = new Dictionary<string, Timer>();

        public List<mobileChat> mobileChats = new List<mobileChat>();
        #endregion
        public App()
        {
            InitializeComponent();

        }
        #region socket.io Server
        bool resterUserOk = false;


        public void ConnectSocketToServer()
        {
            this.socket = IO.Socket("http://40.74.77.139:8888");
            this.socket.On(Socket.EVENT_CONNECT, () =>
            {
                Console.WriteLine("Connect OK");
                var user = new JObject();
                user["idUser"] = ManagerLoginId;
                user["userName"] = ManagerLoginName;
                var jsonRequest = JsonConvert.SerializeObject(user);
                Thread.Sleep(500);
                this.socket.Emit("registration-user", jsonRequest);
            });
            this.socket.On("registration-user-status", (data) =>
            {
                resterUserOk = true;
                string mac = PCExtention.GetMacId();
                var pc = new JObject();
                pc["mac"] = mac;
                this.socket.Emit("registration-pc", JsonConvert.SerializeObject(pc));
                Console.WriteLine("registration-user-status: " + data);
            });
            this.socket.On("register-pc-status", (data) =>
            {
                Console.WriteLine("register-pc-status: " + data);
            });
            //sự kiện yêu cầu mở máy từ QR
            this.socket.On("login-pc", (data) =>
            {
                Console.WriteLine("login-pc: " + data);
                JObject jsonData = JObject.Parse(data.ToString());
                string mac = jsonData.GetValue("mac").ToString();
                string idUser = jsonData.GetValue("idUser").ToString();
                string userName = jsonData.GetValue("userName").ToString();


                // xử lý mở máy ở đây
                // thông tin: Mac,idUser,userName
                //-- lấy thông tin tiền của user
                // kiểm tra có máy tính đang kết nối bắng địa chỉ mác ở trên k
                //1: lấy thông tin của tài khoản
                var acountDetail = RestshapCommand.AccountDetail(idUser);
                if (acountDetail != null && acountDetail.accountDetails.Length == 1)
                {
                    if (acountDetail.accountDetails[0].iActive == 1)
                    {
                        var client = this.clients.Where(n => n.macaddress == mac).SingleOrDefault();
                        if (client != null)
                        {
                            if (client.status == PCStatus.READY)
                            {
                                if (OpenComputerByAccount(mac, userName, acountDetail.accountDetails[0].dBalance))
                                {
                                    var status = new JObject();
                                    status["idUser"] = acountDetail.accountDetails[0].strId;
                                    status["userName"] = userName;
                                    status["mac"] = mac;
                                    status["status"] = "SUCCESS";
                                    status["messeage"] = "LOGIN THÀNH CÔNG";
                                    this.socket.Emit("login-pc-status", JsonConvert.SerializeObject(status));
                                    client.status = PCStatus.ONLINE;
                                    return;
                                }
                            }
                            {
                                var status = new JObject();
                                status["idUser"] = acountDetail.accountDetails[0].strId;
                                status["userName"] = userName;
                                status["mac"] = mac;
                                status["status"] = "ERROR";
                                status["messeage"] = "Login thất bại";
                                this.socket.Emit("login-pc-status", JsonConvert.SerializeObject(status));
                            }
                        }
                    }
                }
            });
            this.socket.On("chat-receive", (data) =>
            {
                JObject meseage = JObject.Parse(data.ToString());

                string mac = meseage.GetValue("mac").ToString();
                string idUserSend = meseage.GetValue("idUserSend").ToString();
                string userSend = meseage.GetValue("userSend").ToString();
                string idUserReceive = meseage.GetValue("idUserReceive").ToString();
                string userReceive = meseage.GetValue("userReceive").ToString();
                string content = meseage.GetValue("content").ToString();

                mobileChat objCheck = mobileChats.Where(n => n.IdUser == idUserSend).FirstOrDefault();
                if (objCheck == null)
                {
                    objCheck = new mobileChat();
                    objCheck.FormChat = new frmChat(idUserSend, this, true);
                    objCheck.FormChat.Text = userSend + " (mobile)";
                    #region save messeage history
                    Messeage ms = new Messeage();
                    ms.idUserSend = idUserSend;
                    ms.userSend = userSend;
                    ms.idUserReceive = this.ManagerLoginId;
                    ms.userReceive = this.ManagerLoginName;
                    ms.content = content;
                    ms.mac = mac;
                    objCheck.mac = mac;
                    objCheck.IdUser = idUserSend;
                    objCheck.UserName = userSend;
                    objCheck.Messeages.Add(ms);
                    mobileChats.Add(objCheck);
                    #endregion
                }
                else
                {
                    if (objCheck.FormChat == null || objCheck.FormChat.IsDisposed)
                    {
                        objCheck.FormChat = new frmChat(idUserSend, this, true);
                        objCheck.FormChat.Text = userSend + " (mobile)";
                        #region save messeage history
                        Messeage ms = new Messeage();
                        ms.idUserSend = idUserSend;
                        ms.userSend = userSend;
                        ms.idUserReceive = this.ManagerLoginId;
                        ms.userReceive = this.ManagerLoginName;
                        ms.content = content;
                        ms.mac = mac;
                        objCheck.mac = mac;
                        objCheck.Messeages.Add(ms);
                        #endregion


                    }


                }
                this.Invoke((Action)delegate
                {
                    objCheck.FormChat.UpdateHistory("==> " + userSend + " : " + content);
                    objCheck.FormChat.Show();
                });


                Console.WriteLine("chat-receive: " + data);
            });
        }
        private IO.Options CreateOptions()
        {
            Quobject.SocketIoClientDotNet.Client.IO.Options op = new Quobject.SocketIoClientDotNet.Client.IO.Options();
            op.AutoConnect = true;
            op.Reconnection = true;
            op.ReconnectionAttempts = 5;
            op.ReconnectionDelay = 5;
            op.Timeout = 1000;
            op.Secure = true;
            op.ForceNew = true;
            op.Multiplex = true;
            return op;
        }

        public void ChatMobile(string idClientMoBile, string messeage)
        {
            var clientMoblie = mobileChats.Where(n => n.IdUser == idClientMoBile).FirstOrDefault();
            if (clientMoblie != null)
            {
                var obj_messeage = new Messeage();
                Messeage ms = new Messeage();
                ms.idUserSend = this.ManagerLoginId;
                ms.userSend = this.ManagerLoginName;
                ms.idUserReceive = clientMoblie.IdUser;
                ms.userReceive = clientMoblie.UserName;
                ms.content = messeage;
                ms.mac = PCExtention.GetMacId();
                this.Invoke((Action)delegate
                {
                    clientMoblie.FormChat.UpdateHistory("==> Bạn: " + messeage);
                });
                string _mess = JsonConvert.SerializeObject(ms);
                this.socket.Emit("chat-send", _mess);
            }
        }
        private void ReadyClient(string mac)
        {
            while (!resterUserOk) { };
            var pc = new JObject();
            pc["mac"] = mac;
            this.socket.Emit("registration-pc", JsonConvert.SerializeObject(pc));
        }

        private void OfflineClient(string mac)
        {
            var pc = new JObject();
            pc["mac"] = mac;
            this.socket.Emit("logout-pc", pc);
        }
        #endregion

        #region socket event  

        public void SocketEventRegistration()
        {
            asyncSocketListener = AsyncSocketListener.Instance;
            asyncSocketListener.MessageReceived += AsyncSocketListener_MessageReceived;
            asyncSocketListener.Disconnected += AsyncSocketListener_Disconnected;
            threadListen = new Thread(new ThreadStart(asyncSocketListener.StartListening));
            threadListen.Start();
        }
        private void AsyncSocketListener_Disconnected(string ipaddress)
        {
            var _cl = this.clients.Where(n => n.ipaddress == ipaddress).SingleOrDefault();
            if (_cl != null)
            {
                OfflineClient(_cl.macaddress);
                this.userTinhTrang.UpdateStatusPC(_cl.macaddress, 0, "00:00:00");
                asyncSocketListener.Close(ipaddress);
                var tm = (Timer)lsTimerPay.Where(n => n.Key == ipaddress).SingleOrDefault().Value;
                if (tm != null)
                {
                    tm.Enabled = false;
                    tm.Dispose();
                    lsTimerPay.Remove(ipaddress);
                }

                this.clients = this.clients.Where(n => n.ipaddress != ipaddress).ToList();
            }
        }
        private void AsyncSocketListener_MessageReceived(string ipaddress, string msg)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<SocketReceivedData>(msg);
                switch (obj.type)
                {
                    #region AUTHORIZE
                    case SocketCommandType.AUTHORIZE:
                        SocketClients client = new SocketClients();
                        client.ipaddress = ipaddress;
                        client.status = PCStatus.READY;
                        client.macaddress = obj.macAddressFrom;
                        clients.Add(client);
                        if (this.userTinhTrang != null)
                        {
                            this.userTinhTrang.UpdateStatusPC(obj.macAddressFrom, 1, "00:00:00");
                        }
                        ReadyClient(obj.macAddressFrom);
                        // đăng ký online
                        break;
                    #endregion

                    #region CHAT
                    case SocketCommandType.CHAT:
                        var _client = clients.Where(c => c.macaddress == obj.macAddressFrom).SingleOrDefault();
                        if (_client != null)
                        {
                            if (_client.frmChat == null || (_client.frmChat != null && _client.frmChat.IsDisposed))
                            {
                                _client.frmChat = new frmChat(ipaddress, this);

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
                                OpenComputerByAccount(obj.macAddressFrom, obj.username, loginResponse.result[0].dBalance);
                                #endregion

                            }
                            else if (loginResponse.result[0].status == "FALSED")
                            {
                                ms.type = SocketCommandType.LOGIN_FALSED;
                                ms.msg = "Đăng nhập thất bại";
                                this.asyncSocketListener.Send(ipaddress, JsonConvert.SerializeObject(ms), false);
                            }
                        }
                        else
                        {
                            ms.type = SocketCommandType.LOGIN_FALSED;
                            ms.msg = "Đăng nhập thất bại";
                            this.asyncSocketListener.Send(ipaddress, JsonConvert.SerializeObject(ms), false);
                        }

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
        /// <summary>
        /// trừ tiền
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="ipaddress"></param>
        private void Timerpay_Tick(object sender, EventArgs e, string ipaddress)
        {
            Timer tt = sender as Timer;
            var _clientItem = clients.Where(n => n.ipaddress == ipaddress).FirstOrDefault();
            if (_clientItem != null && _clientItem.status == PCStatus.ONLINE)
            {
                //tăng thời gian len 2 phút
                _clientItem.timeUsed += 2;
                // trường hợp đăng nhập bằng tài khoản
                if (!string.IsNullOrEmpty(_clientItem.userLogin))
                {
                    //tiền còn lại
                    var amount = (_clientItem.Price / 60 * 2);

                    _clientItem.accountBlance = _clientItem.accountBlance - amount;

                    // số dư khả dụng cho lần trừ tiếp theo
                    if (_clientItem.accountBlance >= amount)
                    {
                        var thoigianconlai = _clientItem.accountBlance / _clientItem.Price * 60;
                        if (RestshapCommand.walletWithdrawal(_clientItem.IdUser, ManagerLoginId, amount, "Phí sử dụng dịch vụ"))
                        {
                            if (thoigianconlai <= 0)
                            {
                                _clientItem.timerStart = DateTime.MinValue;
                                _clientItem.userLogin = "";
                                _clientItem.accountBlance = 0;
                                _clientItem.timeUsed = 0;
                                _clientItem.frmChat = null;
                                _clientItem.status = 1;
                                tt.Enabled = false;
                                tt.Dispose();
                                lsTimerPay.Remove(ipaddress);
                                //hiển thị tiền ở client
                                SocketReceivedData ms = new SocketReceivedData();
                                ms.type = SocketCommandType.OUT_OF_MONEY;
                                this.asyncSocketListener.Send(ipaddress, JsonConvert.SerializeObject(ms), false);
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
                                this.asyncSocketListener.Send(ipaddress, JsonConvert.SerializeObject(ms), false);
                            }
                        }

                    }
                    else
                    {
                        _clientItem.timerStart = DateTime.MinValue;
                        _clientItem.userLogin = "";
                        _clientItem.accountBlance = 0;
                        _clientItem.timeUsed = 0;
                        _clientItem.frmChat = null;
                        _clientItem.status = 1;
                        tt.Enabled = false;
                        tt.Dispose();
                        lsTimerPay.Remove(ipaddress);
                        //hiển thị tiền ở client
                        SocketReceivedData ms = new SocketReceivedData();
                        ms.type = SocketCommandType.OUT_OF_MONEY;
                        this.asyncSocketListener.Send(ipaddress, JsonConvert.SerializeObject(ms), false);
                        this.userTinhTrang.UpdateStatusPC(_clientItem.macaddress, 1, "00:00:00");
                    }
                    //thời gian còn lại

                }
                //trường hợp mở máy
                else
                {
                    // reset thông tin cũ 
                    SocketReceivedData ms = new SocketReceivedData();
                    ms.msgFrom = "SERVER";
                    ms.timeStart = _clientItem.timerStart;
                    ms.timeUpdate = DateTime.Now;
                    ms.timeUsed = _clientItem.timeUsed;
                    ms.price = _clientItem.Price;
                    ms.type = SocketCommandType.UPDATE_INFO_USED;
                    this.asyncSocketListener.Send(ipaddress, JsonConvert.SerializeObject(ms), false);
                }
            }
            else
            {
                tt.Enabled = false;
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
            if (!string.IsNullOrEmpty(ManagerLoginName))
            {
                this.WindowState = FormWindowState.Minimized;
                e.Cancel = true;
            }
            else
            {
                if (this.asyncSocketListener != null)
                {
                    this.asyncSocketListener.Dispose();
                    this.threadListen.Abort();
                }
            }

        }

        private void tileNavPaneMenu_ElementClick(object sender, NavElementEventArgs e)
        {
            string itemTag = e.Element.Name;
            switch (itemTag)
            {
                //Tình trạng hoạt động
                case "mDanhSachMayKhach":
                    ngPage1.Controls.Add(userTinhTrang);
                    ngFrameMenu.SelectedPage = ngPage1;
                    break;
                //Tài khoản
                case "mTaiKhoan":
                    TaiKhoan userTaiKhoan = new TaiKhoan(objMenu);
                    ngPage2.Controls.Add(userTaiKhoan);
                    ngFrameMenu.SelectedPage = ngPage2;
                    break;
                //Nhật ký hệ thống
                case "mNhatKyHeThong":
                    NhatKyHeThong userNKHeThong = new NhatKyHeThong(objMenu);
                    ngPage3.Controls.Add(userNKHeThong);
                    ngFrameMenu.SelectedPage = ngPage3;
                    break;
                //Nhật ký giao dịch
                case "mNhatKyGiaoDich":
                    NhatKySuDung userNhatKySuDung = new NhatKySuDung(objMenu);
                    ngPage4.Controls.Add(userNhatKySuDung);
                    ngFrameMenu.SelectedPage = ngPage4;
                    break;
                //Nhóm người dùng
                case "mNhomNguoiDung":
                    NhomNguoiDung userNhomNguoiDung = new NhomNguoiDung(objMenu);
                    ngPage5.Controls.Add(userNhomNguoiDung);
                    ngFrameMenu.SelectedPage = ngPage5;
                    break;
                //Nhóm máy
                case "mNhomMay":
                    NhomMay userNhomMay = new NhomMay(objMenu);
                    ngPage6.Controls.Add(userNhomMay);
                    ngFrameMenu.SelectedPage = ngPage6;
                    break;
                //Dịch vụ
                case "mDichVu":
                    break;
                default:
                    break;
            }

        }
        private void navChiTiet_ElementClick(object sender, NavElementEventArgs e)
        {
            if (navChiTiet.Tag.ToString() == "detail")
            {
                ngPage1.Controls.Clear();
                TinhTrangChiTiet userTinhTrangCT = new TinhTrangChiTiet(objMenu);
                ngPage1.Controls.Add(userTinhTrangCT);
                ngFrameMenu.SelectedPage = ngPage1;
                navChiTiet.SuperTip.Items.Clear();
                navChiTiet.SuperTip.Items.AddTitle("Hiển thị thông tin theo định dạng lưới");
                navChiTiet.Tag = "big";
            }
            else
            {
                if (navChiTiet.Tag.ToString() == "big")
                {
                    ngPage1.Controls.Clear();
                    TinhTrang userTinhTrang = new TinhTrang(objMenu, this);
                    ngPage1.Controls.Add(userTinhTrang);
                    ngFrameMenu.SelectedPage = ngPage1;
                    navChiTiet.SuperTip.Items.Clear();
                    navChiTiet.SuperTip.Items.AddTitle("Hiển thị thông tin theo định dạng lưới");
                    navChiTiet.Tag = "detail";
                }
            }
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

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Maximized;
            this.ShowInTaskbar = true;
            notifyIcon.Visible = false;
        }
        #endregion

        #region Orther Method

        public void UpdateGui()
        {
            navButtonUser.Caption = "[Administrator - " + ManagerDisplayName + "]";
            ngFrameMenu.Width = this.Width;
            ngFrameMenu.Height = this.Height;
            ngPage1.Width = this.Width;
            ngPage1.Height = this.Height;
            objMenu = new MenuObject();
            objMenu.frmHeight = this.Height;
            objMenu.frmWidth = this.Width;
            objMenu.frmMenuHeight = tileNavPaneMenu.Height;
            objMenu.frmMenuWidth = tileNavPaneMenu.Width;
            this.tileNavPaneMenu.SelectedElement = mDanhSachMayKhach;
            userTinhTrang = new TinhTrang(objMenu, this);
            ngPage1.Controls.Add(userTinhTrang);

        }
        /// <summary>
        /// Trừ tiền sử dụng
        /// </summary>
        /// <param name="id"></param>
        /// <param name="login"></param>
        public void CreateJobPay(string ipaddress, bool login = false)
        {
            Timer timerpay = new Timer();
            timerpay.Elapsed += (sender, eventArgs) => Timerpay_Tick(sender, eventArgs, ipaddress);
            timerpay.Interval = 120000;
            timerpay.Enabled = true;
            lsTimerPay.Add(ipaddress, timerpay);
        }

        /// <summary>
        /// Mở máy bằng tài khoản
        /// </summary>
        /// <param name="clientID"></param>
        /// <param name="mac"></param>
        /// <param name="userName"></param>
        /// <param name="dBalance"></param>
        /// <returns></returns>
        public bool OpenComputerByAccount(string mac, string userName, decimal dBalance)
        {
            SocketReceivedData ms = new SocketReceivedData();
            var _clientsk = clients.Where(c => c.macaddress == mac).SingleOrDefault();
            var computer = RestshapCommand.ComputerDetail(mac);
            var amount = (computer.computeDetail[0].iPrice / 60 * 2);
            if (_clientsk != null && _clientsk.status == PCStatus.READY )
            {
                // tièn trong tài khoản  khả dụng
                if ((computer.computeDetail[0].iPrice / 60 * 2) < dBalance)
                {
                    _clientsk.userLogin = userName;
                    _clientsk.timerStart = DateTime.Now;
                    _clientsk.accountBlance = dBalance;
                    _clientsk.macaddress = mac;
                    _clientsk.status = 2;
                    _clientsk.Price = decimal.Parse(this.userTinhTrang.datasource.Where(n => n.MacID == mac).SingleOrDefault().Price);
                    CreateJobPay(_clientsk.ipaddress, true);
                    var thoigianconlai = _clientsk.accountBlance / _clientsk.Price * 60;
                    ms.accountBlance = _clientsk.accountBlance;
                    ms.timeStart = _clientsk.timerStart;
                    ms.timeUpdate = DateTime.Now;
                    ms.timeUsed = _clientsk.timeUsed;
                    ms.timeRemaining = Decimal.ToInt32(thoigianconlai);
                    ms.price = _clientsk.Price;
                    ms.type = SocketCommandType.LOGIN_SUCCESS;
                    this.asyncSocketListener.Send(_clientsk.ipaddress, JsonConvert.SerializeObject(ms), false);
                    this.userTinhTrang.UpdateStatusPC(mac, 2, string.Format("{0:HH:mm:ss}", _clientsk.timerStart));
                    return true;
                }
                else
                {
                    ms.type = SocketCommandType.LOGIN_FALSED;
                    ms.msg = "Tài khoản không đủ vui lòng nạp thêm để xử dụng dịch vụ";
                    this.asyncSocketListener.Send(_clientsk.ipaddress, JsonConvert.SerializeObject(ms), false);
                }
              
            }
            return false;
        }


        #endregion


    }

    public class SocketClients
    {
        public string ipaddress { set; get; }
        public string macaddress { set; get; }
        public frmChat frmChat { set; get; }
        public DateTime timerStart { set; get; }
        public string IdUser { set; get; }
        public string userLogin { set; get; }
        public decimal accountBlance { set; get; }
        public decimal Price { set; get; }
        public int timeUsed { set; get; }
        public int status { set; get; }
    }

    public class mobileChat
    {
        public mobileChat()
        {
            this.Messeages = new List<Messeage>();
        }
        public frmChat FormChat { set; get; }
        public string IdUser { set; get; }
        public string UserName { set; get; }
        public string mac { set; get; }

        public List<Messeage> Messeages { set; get; }
    }
}
