using System;
using System.Windows.Forms;
using System.Linq;
using Newtonsoft.Json.Linq; 
using Zitn_exe_App.Forms;
using Zitn_exe_App.Utils;
using System.Text;

namespace Zitn_exe_App
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
      {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            

            string param = args.Length > 0 ? args[0] : null;
            bool isFlagOne = false;

            if (!string.IsNullOrEmpty(param))
            {
                try
                {
                    byte[] base64Bytes = Convert.FromBase64String(param);
                    string jsonString = Encoding.UTF8.GetString(base64Bytes);

                    //MessageBox.Show($"解码后的JSON:\n{jsonString}", "调试信息");

                    //System.IO.File.WriteAllText(@"d:\b.txt", param); // 将JSON写入文件以便调试

                    JArray jsonArray = JArray.Parse(jsonString);

                    isFlagOne = jsonArray.Any(item => item["Flag"]?.ToString() == "Flag=1");

                    //param = jsonString;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"JSON解析错误: {ex.Message}");
                }
            }

            if (!MacAddressHelper.ValidateMacAddress())
            {
                string currentMac = MacAddressHelper.GetLocalMacAddress();
                MessageBox.Show(
                    $"当前设备未授权使用本软件！\n\n" +
                    $"您的MAC地址：{currentMac}\n" +
                    $"请联系管理员授权。",
                    "授权验证失败",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            if (isFlagOne)
            {
                Application.Run(new IndexForm(param));
            }
            else
            {
                LoginForm form = new LoginForm();
                if (form.ShowDialog() == DialogResult.OK)
                {
                    Application.Run(new IndexForm(param));
                }
                else
                {
                    Application.Exit();
                }
            }
        }
    }
}