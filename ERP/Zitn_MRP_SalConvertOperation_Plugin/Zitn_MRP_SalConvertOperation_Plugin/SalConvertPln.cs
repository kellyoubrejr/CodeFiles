using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Zitn_MRP_SalConvertOperation_Plugin
{
    [Description("保存时针对评审通过的分录触发实际投产数量超销售数量正差额（多投数量）部分生成计划订单（MRP销售预投），并在分录行记录生成的单据信息"), HotUpdate]
    internal class SalConvertPln : AbstractOperationServicePlugIn
    {
        private static readonly string LogPath = @"D:\金蝶自定义日志文件\MRP.txt";
        private static void WriteLog(string msg)
        {
            try
            {
                var dir = Path.GetDirectoryName(LogPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.AppendAllText(LogPath,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}{Environment.NewLine}",
                    Encoding.UTF8);
            }
            catch { /* 写日志失败不影响主流程 */ }
        }

        public override void BeforeExecuteOperationTransaction(BeforeExecuteOperationTransaction e)
        {
            base.BeforeExecuteOperationTransaction(e);

            var client = new K3CloudApiClient("http://10.0.128.18/k3cloud/");
            WriteLog("开始登录K3Cloud...");

            var loginResult = client.ValidateLogin(
                "6a1f9ac10098f8",
                "2",
                "1qaz@WSX",
                2052
            );
            WriteLog($"登录返回: {loginResult}");

            if (JObject.Parse(loginResult)["LoginResultType"].Value<int>() != 1)
            {
                WriteLog("登录失败，退出");
                return;
            }
            WriteLog("登录成功");

            WriteLog("========== 已审核销售订单保存-生成计划订单 开始 ==========");

            var allIds = e.SelectedRows
                            .Select(row => row.DataEntity["Id"]?.ToString())
                            .ToList();
            WriteLog($"选中行数: {allIds.Count}, FIDs: {string.Join(",", allIds)}");
        }
    }
}
