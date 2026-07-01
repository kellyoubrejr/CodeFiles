using System;
using System.Windows.Forms;

namespace Zitn_exe_App.Utils
{
    public class ExitHelper
    {
        /// <summary>
        /// 退出系统方法
        /// </summary>
        public static void ExitSystem()
        {
            DialogResult result = MessageBox.Show(
                "是否退出系统？",
                "退出确认",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        /// <summary>
        /// 窗体关闭拦截
        /// </summary>
        public static void HandleFormClosing(FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "是否退出系统？",
                "退出确认",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
        }
    }
}
