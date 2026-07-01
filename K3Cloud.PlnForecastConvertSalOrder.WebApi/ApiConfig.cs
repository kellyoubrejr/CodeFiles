using System.Configuration;

namespace K3Cloud.PlnForecastConvertSalOrder.WebApi
{
    /// <summary>
    /// K3Cloud API 配置（读取web.config的appSettings，部署后可配置）
    /// </summary>
    internal static class ApiConfig
    {
        public static string K3CloudUrl => Get("K3CloudApiUrl", "http://10.0.32.18/k3cloud/");
        public static string AppId => Get("K3CloudApiAppId", "688399bec6449e");
        public static string UserName => Get("K3CloudApiUserName", "admin");
        public static string Password => Get("K3CloudApiPassword", "Flzx3qc!");
        public static int Lcid => int.TryParse(Get("K3CloudApiLcid", "2052"), out var v) ? v : 2052;

        private static string Get(string key, string defaultValue)
        {
            return ConfigurationManager.AppSettings[key] ?? defaultValue;
        }
    }
}
