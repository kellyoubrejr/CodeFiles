using System;
using System.Windows.Forms;
using Zitn_exe_App.Forms;
using Zitn_exe_App.Services;
using Zitn_exe_App.Utils;

namespace Zitn_exe_App
{
    public partial class LoginForm : Form
    {
        private readonly AuthService _authService = new AuthService();
        public LoginForm()
        {
            InitializeComponent();
            this.AcceptButton = btn_qd;
            this.FormClosing += LoginForm_FormClosing;
        }

        //登录
        private void btn_qd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtb_user.Text))
            {
                MessageBox.Show("请输入用户名");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtb_passw.Text))
            {
                MessageBox.Show("请输入密码");
                return;
            }

            string result = _authService.Login(
                txtb_user.Text.Trim(),
                txtb_passw.Text.Trim()
            );

            if (result == "OK")
            {
                //MessageBox.Show("登录成功");
                DialogResult = DialogResult.OK;
                this.FormClosing -= LoginForm_FormClosing;
                //Close();
            }
            else
            {
                MessageBox.Show(result);
            }


        }

        //重置
        private void btn_rej_Click(object sender, EventArgs e)
        {
            string username = txtb_user.Text.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("请先输入用户名！");
                return;
            }

            ChangePasswordForm form = new ChangePasswordForm(username);
            form.ShowDialog();
        }




        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //ExitHelper.HandleFormClosing(e);
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            MacAddressHelper.CheckMacAddressAndExit();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            toolStripStatusLabel1.Text = System.DateTime.Now.ToString();
        }
    }
}
