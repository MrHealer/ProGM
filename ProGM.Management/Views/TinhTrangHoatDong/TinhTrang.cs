using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using ProGM.Management.Model;
using ProGM.Management.Controller;
using ProGM.Management.Views.TaiKhoan;
using Newtonsoft.Json;
using DevExpress.XtraGrid.Views.Tile;
using DevExpress.XtraGrid.Views.Tile.ViewInfo;
using DevExpress.XtraGrid.Views.Grid;
using ProGM.Business.Model;
using ProGM.Business.ApiBusiness;
using Timer = System.Timers.Timer;

namespace ProGM.Management.Views.TinhTrangHoatDong
{
    public partial class TinhTrang : DevExpress.XtraEditors.XtraUserControl
    {
        App app_controller;
        public List<gridViewDataItem> datasource = new List<gridViewDataItem>();
        public TinhTrang(MenuObject obj, App app)
        {
            this.app_controller = app;
            InitializeComponent();
            ConfigLayout.UpdateLayout(this, panelTinhTrang, grdTinhTrang, obj);
            InitData();

        }
        public void InitData()
        {
            responseListPC responseData = RestshapCommand.GetAllComputerByCompany(this.app_controller.CompanyId);
            if (responseData != null && responseData.computerList!=null)
            {
                int countOffline = 0;
                int countOnline = 0;
                int countReady = 0;
                foreach (var item in responseData.computerList)
                {
                    var pc = this.app_controller.clients.Where(n => n.macaddress.Equals(item.strMacAddress)).SingleOrDefault();
                    int status = 0;
                    if (pc != null)
                    {
                        if (pc.timerStart != null && pc.timerStart != DateTime.MinValue)
                        {
                            status = 2;
                            countOnline++;
                        }
                        else
                        {
                            status = 1;
                            countReady++;
                        }
                    }
                    else
                    {
                        countOffline++;
                    }

                    datasource.Add(new gridViewDataItem()
                    {
                        NamePC = item.strName,
                        Old = item.strName,
                        Group = item.strGroupName,
                        Price = item.iPrice,
                        MacID = item.strMacAddress,
                        Status = status,
                        timeLogin = "00:00:00"
                    });
                }
                lbCountOffline.Text = countOffline.ToString();
                lbCountOnline.Text = countOnline.ToString();
                lbCountReady.Text = countReady.ToString();


            }
            grdTinhTrang.DataSource = datasource;
        }

        private void pictureEdit1_EditValueChanged(object sender, EventArgs e)
        {
            MessageBox.Show("111");
        }

        private void PopupMenuShowing(object sender, EventArgs e)
        {


        }

        private void tileView1_ShowingPopupEditForm(object sender, DevExpress.XtraGrid.Views.Grid.ShowingPopupEditFormEventArgs e)
        {
            MessageBox.Show("22");
        }

        private void tileView1_ItemRightClick(object sender, DevExpress.XtraGrid.Views.Tile.TileViewItemClickEventArgs e)
        {
            TileView view = sender as TileView;
            Point pt = view.GridControl.PointToClient(Control.MousePosition);
            System.Drawing.Point p2 = Control.MousePosition;
            int rowStatus = int.Parse(string.Format("{0}", tileView1.GetFocusedRowCellValue("Status")));
            bool isShowpopupmenu = true;
            switch (rowStatus)
            {
                case PCStatus.OFFLINE:
                    isShowpopupmenu = false;
                    MessageBox.Show("Vui lòng bật máy trước!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case PCStatus.READY:
                    btnOpenPC.Enabled = true;
                    menuAddMoney.Enabled = false;
                    menuBlockPC.Enabled = false;
                    menuShowChat.Enabled = false;
                    break;
                case PCStatus.ONLINE:
                    btnOpenPC.Enabled = false;
                    menuAddMoney.Enabled = true;
                    menuBlockPC.Enabled = true;
                    menuShowChat.Enabled = true;
                    break;
                default:
                    break;
            }
            if (isShowpopupmenu)
            {
                popupMenuPC.ShowPopup(p2);
            }


        }

        private void barButtonItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            frmNapTien napTien = new frmNapTien();
            napTien.Show();
        }

        private void menuShowChat_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var id = tileView1.GetFocusedRowCellValue("MacID");
        }


        private void btnOpenPC_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            string mac = tileView1.GetFocusedRowCellValue("MacID").ToString();
            var client = this.app_controller.clients.Where(n => n.macaddress.Equals(mac)).SingleOrDefault();
            if (client != null)
            {
                client.timerStart = DateTime.Now;
                client.Price = decimal.Parse(tileView1.GetFocusedRowCellValue("Price").ToString()); 
                this.app_controller.CreateJobPay(client.ipaddress);
                SocketReceivedData ms = new SocketReceivedData();
                ms.type = SocketCommandType.OPENCLIENT;
                ms.timeStart = client.timerStart;
                ms.price = client.Price;
                this.app_controller.asyncSocketListener.Send(client.ipaddress, JsonConvert.SerializeObject(ms), false);
                this.UpdateStatusPC(mac, 2, DateTime.Now.ToString("HH:mm:ss"));
                client.status = PCStatus.ONLINE;
            }

        }

        private void menuBlockPC_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            string mac = tileView1.GetFocusedRowCellValue("MacID").ToString();
            var client = this.app_controller.clients.Where(n => n.macaddress.Equals(mac)).SingleOrDefault();
            if (client != null)
            {
                //sop timer 
                var timerPay = this.app_controller.lsTimerPay.Where(n => n.Key == client.ipaddress).SingleOrDefault();
                if (timerPay.Value!=null && timerPay.Value.Enabled)
                {
                    timerPay.Value.Enabled = false;
                    timerPay.Value.Dispose();
                    this.app_controller.lsTimerPay.Remove(client.ipaddress);
                }
                SocketReceivedData ms = new SocketReceivedData();
                ms.type = SocketCommandType.CLOSECLIENT;
                this.app_controller.asyncSocketListener.Send(client.ipaddress, JsonConvert.SerializeObject(ms), false);
                this.UpdateStatusPC(mac, 1, "00:00:00");
            }
        }

        private void tileView1_ItemCustomize(object sender, TileViewItemCustomizeEventArgs e)
        {
            TileView view = sender as TileView;
            int status = int.Parse(string.Format("{0}", view.GetRowCellValue(e.RowHandle, "Status")));
            switch (status)
            {
                case PCStatus.OFFLINE:
                    e.Item.Elements[1].Image = Properties.Resources.offile;
                    break;
                case PCStatus.READY:
                    e.Item.Elements[1].Image = Properties.Resources.vang;
                    break;
                case PCStatus.ONLINE:
                    e.Item.Elements[1].Image = Properties.Resources.online;
                    break;
                default:
                    break;
            }
        }

        #region orther method
        public void UpdateStatusPC(string mac, int status, string time)
        {
            var item = datasource.Where(n => n.MacID == mac).SingleOrDefault();
            if (item != null)
            {
                item.Status = status;
                item.timeLogin = time;
            }
            this.Invoke((Action)delegate
            {
                string cOnline = datasource.Where(n => n.Status == 2).Count().ToString();
                string cReady = datasource.Where(n => n.Status == 1).Count().ToString();
                string cOffline = datasource.Where(n => n.Status == 0).Count().ToString();
                lbCountOffline.Text = cOffline;
                lbCountReady.Text = cReady;
                lbCountOnline.Text = cOnline;
                grdTinhTrang.RefreshDataSource();
            });

            // Application.DoEvents();


        }
        #endregion

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string text = txtSearch.Text;
                if (!string.IsNullOrEmpty(text))
                {
                    var data = datasource.Where(n => n.NamePC.Contains(text)).ToList();
                    grdTinhTrang.DataSource = data;
                }
                else
                {
                    grdTinhTrang.DataSource = datasource;
                }
                grdTinhTrang.RefreshDataSource();
            }
        }
    }
}
