using System;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace Zitn_exe_App.Utils
{
    public class MacAddressHelper
    {
        /// <summary>
        /// 允许使用的MAC地址列表（白名单）
        /// "70-20-84-00-20-26" "72-1C-E7-7E-97-C1"-->刘军
        /// </summary>
        private static readonly string[] AllowedMacAddresses = new string[]
        {
            "70-20-84-00-20-26",
            "72-1C-E7-7E-97-C1"
        };

        /// <summary>
        /// 获取本机第一块物理网卡的MAC地址
        /// </summary>
        /// <returns>MAC地址（格式：XX-XX-XX-XX-XX-XX）</returns>
        public static string GetLocalMacAddress()
        {
            try
            {
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface adapter in nics)
                {
                    // 过滤掉虚拟网卡、回环网卡、非活动网卡
                    if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                        adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                    {
                        if (adapter.OperationalStatus == OperationalStatus.Up)
                        {
                            string mac = adapter.GetPhysicalAddress().ToString();
                            if (!string.IsNullOrEmpty(mac) && mac.Length == 12)
                            {
                                // 格式化为 XX-XX-XX-XX-XX-XX
                                return FormatMacAddress(mac);
                            }
                        }
                    }
                }

                // 如果没找到活动的网卡，获取第一个可用的
                foreach (NetworkInterface adapter in nics)
                {
                    string mac = adapter.GetPhysicalAddress().ToString();
                    if (!string.IsNullOrEmpty(mac) && mac.Length == 12)
                    {
                        return FormatMacAddress(mac);
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取MAC地址失败：{ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 格式化MAC地址（将1234567890AB转换为12-34-56-78-90-AB）
        /// </summary>
        private static string FormatMacAddress(string mac)
        {
            if (string.IsNullOrEmpty(mac) || mac.Length != 12)
                return mac;

            return $"{mac.Substring(0, 2)}-{mac.Substring(2, 2)}-{mac.Substring(4, 2)}-{mac.Substring(6, 2)}-{mac.Substring(8, 2)}-{mac.Substring(10, 2)}";
        }

        /// <summary>
        /// 校验MAC地址是否在白名单中
        /// </summary>
        /// <returns>true: 允许访问；false: 禁止访问</returns>
        public static bool ValidateMacAddress()
        {
            string currentMac = GetLocalMacAddress();

            if (string.IsNullOrEmpty(currentMac))
            {
                return false;
            }

            foreach (string allowedMac in AllowedMacAddresses)
            {
                // 不区分大小写比较
                if (string.Equals(currentMac, allowedMac, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                // 比较无分隔符的格式
                string currentMacNoDash = currentMac.Replace("-", "");
                string allowedMacNoDash = allowedMac.Replace("-", "");
                if (string.Equals(currentMacNoDash, allowedMacNoDash, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 校验MAC地址，如果不通过则显示错误并退出
        /// </summary>
        /// <returns>true: 校验通过；false: 校验不通过</returns>
        public static bool CheckMacAddressAndExit()
        {
            if (!ValidateMacAddress())
            {
                string currentMac = GetLocalMacAddress();
                MessageBox.Show(
                    $"当前设备未授权使用本软件！\n\n" +
                    $"您的MAC地址：{currentMac}\n" +
                    $"请联系管理员授权。",
                    "授权验证失败",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                System.Diagnostics.Process.GetCurrentProcess().Kill();
                Environment.Exit(0);
                return false;
            }
            return true;
        }
    }
}