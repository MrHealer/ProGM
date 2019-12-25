using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using ProGM.Business.Model;
using ProGM.Business.ApiBusiness;

namespace ProGM.Management.Views.DangNhap
{
    public partial class DangNhap : DevExpress.XtraEditors.XtraForm
    {
        App app_controller;
        public DangNhap(App app)
        {
            this.app_controller = app;
            InitializeComponent();
        }

        private void DangNhap_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            string userName = txtTaiKhoan.Text;
            string passWord = txtMatKhau.Text;
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(passWord))
            {
                MessageBox.Show("Vui lòng nhập thông tin tài khoản", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                string messeage = "";
                LoginResponse loginResponse = RestshapCommand.Login(userName, passWord, ref messeage);
                SocketReceivedData ms = new SocketReceivedData();
                if (loginResponse != null)
                {

                    if (loginResponse.result[0].status == "SUCCESS")
                    {
                        if (loginResponse.result[0].strRoleName == "MANAGER")
                        {
                            this.Hide();
                            this.app_controller.UpdateGui();
                            this.app_controller.Show();
                        }
                        else
                        {
                            MessageBox.Show("Tài khoản không được phép đăng nhập", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else if (loginResponse.result[0].status == "VERIFY")
                    {
                        XtraMessageBox.Show("Chỗ này hiển thị form đổi mật khẩu");
                        //MessageBox.Show("Tài khoản hoặc mật khẩu không đúng", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Đăng nhập không thành công", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
    }
}