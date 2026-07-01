using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zitn_exe_App.Data;
using Zitn_exe_App.Services;
using Zitn_exe_App.Utils;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Zitn_exe_App.Forms
{
    public partial class ChangePasswordForm : Form
    {
        private readonly string _username;

        private readonly PasswordService _service = new PasswordService();


        public ChangePasswordForm( string username)
        {
            InitializeComponent();

            _username = username;

            txt_user.Text = username;
            txt_user.ReadOnly = true;
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void btn_cancal_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btn_ok_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txt_user.Text) ||
                string.IsNullOrWhiteSpace(txt_oldPwd.Text) ||
                string.IsNullOrWhiteSpace(txt_newPwd.Text) ||
                string.IsNullOrWhiteSpace(txt_confirmPwd.Text))
            {
                MessageBox.Show("请完整填写信息！");
                return;
            }

            if (txt_newPwd.Text != txt_confirmPwd.Text)
            {
                MessageBox.Show("两次新密码不一致！");
                return;
            }

            string result = _service.ChangePassword(
                                                     txt_user.Text.Trim(),
                                                     txt_oldPwd.Text.Trim(),
                                                     txt_newPwd.Text.Trim()
                                                 );

            if (result == "OK")
            {
                MessageBox.Show("修改成功！");
                this.Close();
            }
            else
            {
                MessageBox.Show(result);
            }
        }
    }
}
