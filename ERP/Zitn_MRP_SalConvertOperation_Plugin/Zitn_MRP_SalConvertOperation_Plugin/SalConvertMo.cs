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
    [Description("【已审核销售订单保存】，检测分录“评审状态”通过按照实际投产数量生成生产订单"), HotUpdate]
    public class SalConvertMo : AbstractOperationServicePlugIn
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

            WriteLog("========== 已审核销售订单保存-生成生产订单 开始 ==========");

            var allIds = e.SelectedRows
                            .Select(row => row.DataEntity["Id"]?.ToString())
                            .ToList();
            WriteLog($"选中行数: {allIds.Count}, FIDs: {string.Join(",", allIds)}");

            // 按单据类型分组，支持批量混合处理
            var idsByBillType = GetBillTypeGroups(allIds);
            idsByBillType.TryGetValue("68d0fdc2ec8d84", out var ytList);
            idsByBillType.TryGetValue("6902b7bb51f609", out var nbszList);
            WriteLog($"按单据类型分组: 预投销售订单={ytList?.Count ?? 0}条, 内部试制销售订单={nbszList?.Count ?? 0}条");

            foreach (var kv in idsByBillType)
            {
                var billType = kv.Key;
                var groupIds = string.Join(",", kv.Value);
                WriteLog($"处理单据类型={billType}, FIDs={groupIds}");

                if (billType == "685101070dae60")
                {
                    YTSalOrder(groupIds, client);
                }
                else if (billType == "68d0fda8ec895c")
                {
                    NBSZSalOrder(groupIds, client);
                }
                else
                {
                    WriteLog($"未知单据类型={billType}, 跳过");
                }
            }
        }

        private void NBSZSalOrder(string ids, K3CloudApiClient client)
        {
            var salSql = $@"SELECT  A.FBILLNO AS XSDDNO,
                A.FID AS SALFID,
                B.FENTRYID AS SALENTRYID,
                C.FNUMBER AS KHNUM,
                M.FNUMBER AS WLNUM,
                A.F_PAEZ_TEXT AS BPMNO,
                T.FNUMBER AS DW,                
                B.FQTY AS SL,
                B1.FDELIVERYDATE AS SJ,
                CU.FNUMBER AS BB
                FROM T_SAL_ORDER A
                JOIN T_SAL_ORDERENTRY B ON A.FID = B.FID
                JOIN T_SAL_ORDERENTRY_D B1 ON B.FENTRYID = B1.FENTRYID
                JOIN T_SAL_ORDERFIN B3 ON A.FID = B3.FID
                JOIN T_BD_CURRENCY CU ON B3.FSETTLECURRID = CU.FCURRENCYID
                LEFT JOIN T_BD_CUSTOMER C ON A.FCUSTID = C.FCUSTID
                JOIN T_BD_MATERIAL M ON B.FMATERIALID = M.FMATERIALID
                JOIN T_BD_UNIT T ON B.FUNITID = T.FUNITID
                WHERE A.FBILLTYPEID = '6902b7bb51f609' AND A.FID IN ({ids})";
            WriteLog($"内部试制销售订单执行SQL: {salSql}");
        }

        private void YTSalOrder(string ids, K3CloudApiClient client)
        {
            var salSql = $@"SELECT  A.FBILLNO AS XSDDNO,
                A.FID AS SALFID,
                B.FENTRYID AS SALENTRYID,
                C.FNUMBER AS KHNUM,
                M.FNUMBER AS WLNUM,
                A.F_PAEZ_TEXT AS BPMNO,
                T.FNUMBER AS DW,                
                B.F_PAEZ_SJTCQTY AS SL,
                B1.FDELIVERYDATE AS SJ,
                CU.FNUMBER AS BB
                FROM T_SAL_ORDER A
                JOIN T_SAL_ORDERENTRY B ON A.FID = B.FID
                JOIN T_SAL_ORDERENTRY_D B1 ON B.FENTRYID = B1.FENTRYID
                JOIN T_SAL_ORDERFIN B3 ON A.FID = B3.FID
                JOIN T_BD_CURRENCY CU ON B3.FSETTLECURRID = CU.FCURRENCYID
                LEFT JOIN T_BD_CUSTOMER C ON A.FCUSTID = C.FCUSTID
                JOIN T_BD_MATERIAL M ON B.FMATERIALID = M.FMATERIALID
                JOIN T_BD_UNIT T ON B.FUNITID = T.FUNITID
                WHERE A.FBILLTYPEID = '68d0fdc2ec8d84' AND A.FID IN ({ids}) AND B.F_PAEZ_STATUS ='1'";
            WriteLog($"预投销售订单执行SQL: {salSql}");

            var salDt = DBUtils.ExecuteDynamicObject(this.Context, salSql);
            if (salDt == null || salDt.Count == 0)
            {
                WriteLog("SQL查询结果为空，退出");
                return;
            }
            WriteLog($"SQL查询到 {salDt.Count} 行数据");

            // 按销售订单FID分组，一个销售订单生成一张生产单
            var orderDict = new Dictionary<string, List<DynamicObject>>();
            foreach (DynamicObject row in salDt)
            {
                var salFid = row["SALFID"]?.ToString();
                if (string.IsNullOrEmpty(salFid)) continue;
                if (!orderDict.ContainsKey(salFid))
                    orderDict[salFid] = new List<DynamicObject>();
                orderDict[salFid].Add(row);
            }
            WriteLog($"按销售订单分组后共 {orderDict.Count} 张生产单待创建");

            foreach (var kv in orderDict)
            {
                var salFid = kv.Key;
                var rows = kv.Value;
                var bpmNo = rows[0]["BPMNO"]?.ToString() ?? "";
                var bibie = rows[0]["BB"]?.ToString() ?? "";
                var salNo = rows[0]["XSDDNO"]?.ToString() ?? "";
                var today = DateTime.Now.ToString("yyyy-MM-dd");

                WriteLog($"---------- 开始创建预测单, 销售订单FID={salFid}, 分录行数={rows.Count} ----------");

                // ---- 单据体 ----
                JArray entryArray = new JArray();
                foreach (var row in rows)
                {
                    var wlNum = row["WLNUM"]?.ToString() ?? "";
                    var dw = row["DW"]?.ToString() ?? "";
                    var sl = Convert.ToDecimal(row["SL"]);
                    var sj = Convert.ToDateTime(row["SJ"]).ToString("yyyy-MM-dd");
                    var khNum = row["KHNUM"]?.ToString() ?? "";

                    WriteLog($"分录: 物料={wlNum}, 客户={khNum}, 数量={sl}, 单位={dw}, 日期={sj}");
                }
            }
        }

        private Dictionary<string, List<string>> GetBillTypeGroups(List<string> allIds)
        {
            var result = new Dictionary<string, List<string>>();
            if (allIds == null || allIds.Count == 0) return result;

            var ids = string.Join(",", allIds);
            var sql = $@"/*dialect*/SELECT A.FID, A.FBILLTYPEID, A.FBILLNO, A.FDOCUMENTSTATUS FROM T_SAL_ORDER A WHERE A.FID IN ({ids})";
            var dt = DBUtils.ExecuteDynamicObject(this.Context, sql);
            var skippedNos = new List<string>();
            foreach (DynamicObject row in dt)
            {
                var fid = row["FID"]?.ToString();
                var billType = row["FBILLTYPEID"]?.ToString();
                var billNo = row["FBILLNO"]?.ToString();
                var docStatus = row["FDOCUMENTSTATUS"]?.ToString();
                if (string.IsNullOrEmpty(fid) || string.IsNullOrEmpty(billType)) continue;
                if (docStatus != "C")
                {
                    skippedNos.Add(billNo ?? fid);
                    continue;
                }
                if (!result.ContainsKey(billType))
                    result[billType] = new List<string>();
                result[billType].Add(fid);
            }
            if (skippedNos.Count > 0)
                WriteLog($"跳过{skippedNos.Count}条未审核的销售订单，单号: {string.Join(",", skippedNos)}");
            return result;
        }
    }
}
