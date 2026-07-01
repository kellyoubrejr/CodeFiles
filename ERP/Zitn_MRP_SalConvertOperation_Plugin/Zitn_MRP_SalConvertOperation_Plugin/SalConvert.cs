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
    [Description("【销售订单审核】，意向/预测销售订单自动生成意向销售订单/预测备料预测单"), HotUpdate]
    public class SalConvert : AbstractOperationServicePlugIn
    {
        private static readonly string LogPath = @"D:\金蝶自定义日志文件\MRP.txt";
        // 预测单单据类型，YCD03_ZT002-预测销售订单，YCD03_ZT001-预测备料
        private const string PLN_FORECAST_TYPE1 = "YCD03_ZT002";

        private const string PLN_FORECAST_TYPE2 = "YCD03_ZT001";

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

            WriteLog("========== 销售订单审核-预测单生成 开始 ==========");

            var allIds = e.SelectedRows
                            .Select(row => row.DataEntity["Id"]?.ToString())
                            .ToList();
            WriteLog($"选中行数: {allIds.Count}, FIDs: {string.Join(",", allIds)}");

            // 按单据类型分组，支持批量混合处理
            var idsByBillType = GetBillTypeGroups(allIds);
            idsByBillType.TryGetValue("685101070dae60", out var yxList);
            idsByBillType.TryGetValue("68d0fda8ec895c", out var ycList);
            WriteLog($"按单据类型分组: 意向销售订单={yxList?.Count ?? 0}条, 预测销售订单={ycList?.Count ?? 0}条");

            foreach (var kv in idsByBillType)
            {
                var billType = kv.Key;
                var groupIds = string.Join(",", kv.Value);
                WriteLog($"处理单据类型={billType}, FIDs={groupIds}");

                if (billType == "685101070dae60")
                {
                    YXSalOrder(groupIds, client);
                }
                else if (billType == "68d0fda8ec895c")
                {
                    YCSalOrder(groupIds, client);
                }
                else
                {
                    WriteLog($"未知单据类型={billType}, 跳过");
                }
            }



        }

        private void YCSalOrder(string ids, K3CloudApiClient client)
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
                WHERE A.FBILLTYPEID = '68d0fda8ec895c' AND A.FID IN ({ids})";
            WriteLog($"预测销售订单执行SQL: {salSql}");

            var salDt = DBUtils.ExecuteDynamicObject(this.Context, salSql);
            if (salDt == null || salDt.Count == 0)
            {
                WriteLog("SQL查询结果为空，退出");
                return;
            }
            WriteLog($"SQL查询到 {salDt.Count} 行数据");

            // 按销售订单FID分组，一个销售订单生成一张预测单
            var orderDict = new Dictionary<string, List<DynamicObject>>();
            foreach (DynamicObject row in salDt)
            {
                var salFid = row["SALFID"]?.ToString();
                if (string.IsNullOrEmpty(salFid)) continue;
                if (!orderDict.ContainsKey(salFid))
                    orderDict[salFid] = new List<DynamicObject>();
                orderDict[salFid].Add(row);
            }
            WriteLog($"按销售订单分组后共 {orderDict.Count} 张预测单待创建");

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

                    JObject entryObject = new JObject();
                    entryObject.Add("FEntryID", 0);

                    JObject supplyOrgObj = new JObject();
                    supplyOrgObj.Add("FNumber", "101");
                    entryObject.Add("FSupplyOrgId", supplyOrgObj);

                    JObject custObj = new JObject();
                    custObj.Add("FNumber", khNum);
                    entryObject.Add("FCustID", custObj);

                    entryObject.Add("FProductType", "0");

                    JObject materialObj = new JObject();
                    materialObj.Add("FNumber", wlNum);
                    entryObject.Add("FMaterialID", materialObj);

                    JObject bomObj = new JObject();
                    bomObj.Add("FNumber", "");
                    entryObject.Add("FBomID", bomObj);

                    JObject unitObj = new JObject();
                    unitObj.Add("FNumber", dw);
                    entryObject.Add("FUnitID", unitObj);

                    JObject baseUnitObj = new JObject();
                    baseUnitObj.Add("FNumber", dw);
                    entryObject.Add("FBaseUnitID", baseUnitObj);

                    entryObject.Add("FQty", sl);
                    entryObject.Add("FBaseQty", sl);
                    entryObject.Add("FStartDate", sj);
                    entryObject.Add("FEndDate", sj);
                    entryObject.Add("FReserveType", "1");
                    entryObject.Add("FAVERATYPE", "N");

                    JObject stockOrgObj = new JObject();
                    stockOrgObj.Add("FNumber", "101");
                    entryObject.Add("FStockOrgId", stockOrgObj);

                    entryObject.Add("FCloseStatus", "N");

                    entryObject.Add("F_XQ_YSL", sl);

                    entryArray.Add(entryObject);
                }

                // ---- 主表Model ----
                JObject modelObject = new JObject();
                modelObject.Add("FID", 0);

                JObject foreOrgObj = new JObject();
                foreOrgObj.Add("FNumber", "101");
                modelObject.Add("FForeOrgId", foreOrgObj);

                JObject billTypeObj = new JObject();
                billTypeObj.Add("FNUMBER", PLN_FORECAST_TYPE2);
                modelObject.Add("FBillTypeID", billTypeObj);

                modelObject.Add("FDate", today);
                modelObject.Add("F_PAEZ_NBYC", "false");
                modelObject.Add("F_PAEZ_YSDJH", bpmNo);
                JObject bibieObj = new JObject();
                bibieObj.Add("FNumber", bibie);
                modelObject.Add("F_XQ_BB", bibieObj);
                modelObject.Add("FEntity", entryArray);
                //单据编号FBillNo 
                modelObject.Add("FBillNo", salNo);

                // ---- Save ----
                JObject saveObj = new JObject
                {
                    ["Model"] = modelObject,
                    ["IsDeleteEntry"] = true
                };

                var jsonData = saveObj.ToString();
                WriteLog($"调用Save接口, JSON数据: {jsonData}");

                try
                {
                    var result = client.Save("PLN_FORECAST", jsonData);
                    WriteLog($"Save返回结果: {result}");

                    var saveResult = JObject.Parse(result);
                    var responseStatus = saveResult["Result"]?["ResponseStatus"];
                    if (responseStatus != null)
                    {
                        var isSuccess = responseStatus["IsSuccess"]?.Value<bool>() ?? false;
                        if (isSuccess)
                        {
                            var forecastId = saveResult["Result"]?["Id"]?.ToString() ?? "";
                            var forecastNumber = saveResult["Result"]?["Number"]?.ToString() ?? "";
                            WriteLog($"预测单创建成功: Id={forecastId}, Number={forecastNumber}");

                            // 反查预测单的FID和FENTRYID
                            var forecastSql = $@"SELECT A.FID, B.FENTRYID
                                      FROM T_PLN_FORECAST A
                                      JOIN T_PLN_FORECASTENTRY B ON A.FID = B.FID
                                      WHERE A.FBILLNO = '{forecastNumber}'";
                            WriteLog($"反查预测单SQL: {forecastSql}");

                            var forecastDt = DBUtils.ExecuteDynamicObject(this.Context, forecastSql);
                            if (forecastDt != null && forecastDt.Count > 0)
                            {
                                WriteLog($"反查预测单结果行数: {forecastDt.Count}");

                                // 逐行更新销售订单分录，按顺序匹配
                                for (int i = 0; i < forecastDt.Count && i < rows.Count; i++)
                                {
                                    var forecastFid = forecastDt[i]["FID"]?.ToString();
                                    var forecastEntryId = forecastDt[i]["FENTRYID"]?.ToString();
                                    var salEntryId = rows[i]["SALENTRYID"]?.ToString();

                                    var updateSql = $@"UPDATE T_SAL_ORDERENTRY
                                           SET F_UNW_Integer_re5 = {forecastFid},
                                               F_UNW_Integer_tzk = {forecastEntryId}
                                           WHERE FENTRYID = {salEntryId}";
                                    WriteLog($"更新销售订单分录: {updateSql}");
                                    DBUtils.Execute(this.Context, updateSql);
                                }
                                WriteLog("销售订单分录更新完成");
                            }
                            else
                            {
                                WriteLog($"反查预测单无结果, 预测单号={forecastNumber}");
                            }
                        }
                        else
                        {
                            var errors = responseStatus["Errors"] as JArray;
                            var errorMsg = errors != null && errors.Count > 0
                                ? errors[0]["Message"]?.ToString()
                                : "未知错误";
                            WriteLog($"预测单创建失败: {errorMsg}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteLog($"调用Save接口异常: {ex.Message}");
                }
            }

            WriteLog("========== 销售订单审核-预测单生成 结束 ==========");
        }

        private void YXSalOrder(string ids, K3CloudApiClient client)
        {
            var salSql = $@"SELECT  A.FBILLNO AS XSDDNO,
                A.FID AS SALFID,
                B.FENTRYID AS SALENTRYID,
                C.FNUMBER AS KHNUM,
                M.FNUMBER AS WLNUM,
                A.F_PAEZ_TEXT AS BPMNO,
                T.FNUMBER AS DW,
                B2.FTAXRATE AS SLV,
                B2.FTAXAMOUNT AS SE,
                B2.FPRICE AS DJ,
                B2.FTAXPRICE AS HSDJ,
                B2.FAMOUNT AS JE,
                B2.FALLAMOUNT AS JSHJ,
                B.FQTY AS SL,
                B1.FDELIVERYDATE AS SJ,
                CU.FNUMBER AS BB
                FROM T_SAL_ORDER A
                JOIN T_SAL_ORDERENTRY B ON A.FID = B.FID
                JOIN T_SAL_ORDERENTRY_D B1 ON B.FENTRYID = B1.FENTRYID
                JOIN T_SAL_ORDERENTRY_F B2 ON B.FENTRYID = B2.FENTRYID
                JOIN T_SAL_ORDERFIN B3 ON A.FID = B3.FID
                JOIN T_BD_CURRENCY CU ON B3.FSETTLECURRID = CU.FCURRENCYID
                JOIN T_BD_CUSTOMER C ON A.FCUSTID = C.FCUSTID
                JOIN T_BD_MATERIAL M ON B.FMATERIALID = M.FMATERIALID
                JOIN T_BD_UNIT T ON B.FUNITID = T.FUNITID
                WHERE A.FBILLTYPEID = '685101070dae60' AND A.FID IN ({ids})";
            WriteLog($"执行SQL: {salSql}");

            var salDt = DBUtils.ExecuteDynamicObject(this.Context, salSql);
            if (salDt == null || salDt.Count == 0)
            {
                WriteLog("SQL查询结果为空，退出");
                return;
            }
            WriteLog($"SQL查询到 {salDt.Count} 行数据");

            // 按销售订单FID分组，一个销售订单生成一张预测单
            var orderDict = new Dictionary<string, List<DynamicObject>>();
            foreach (DynamicObject row in salDt)
            {
                var salFid = row["SALFID"]?.ToString();
                if (string.IsNullOrEmpty(salFid)) continue;
                if (!orderDict.ContainsKey(salFid))
                    orderDict[salFid] = new List<DynamicObject>();
                orderDict[salFid].Add(row);
            }
            WriteLog($"按销售订单分组后共 {orderDict.Count} 张预测单待创建");

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
                    var slv = Convert.ToDecimal(row["SLV"]);
                    var se = Convert.ToDecimal(row["SE"]);
                    var dj = Convert.ToDecimal(row["DJ"]);
                    var hsdj = Convert.ToDecimal(row["HSDJ"]);
                    var je = Convert.ToDecimal(row["JE"]);
                    var jshj = Convert.ToDecimal(row["JSHJ"]);

                    WriteLog($"分录: 物料={wlNum}, 客户={khNum}, 数量={sl}, 单位={dw}, 日期={sj}, 单价={dj}, 含税单价={hsdj}, 税率={slv}, 税额={se}, 金额={je}, 价税合计={jshj}");

                    JObject entryObject = new JObject();
                    entryObject.Add("FEntryID", 0);

                    JObject supplyOrgObj = new JObject();
                    supplyOrgObj.Add("FNumber", "101");
                    entryObject.Add("FSupplyOrgId", supplyOrgObj);

                    JObject custObj = new JObject();
                    custObj.Add("FNumber", khNum);
                    entryObject.Add("FCustID", custObj);

                    entryObject.Add("FProductType", "0");

                    JObject materialObj = new JObject();
                    materialObj.Add("FNumber", wlNum);
                    entryObject.Add("FMaterialID", materialObj);

                    JObject bomObj = new JObject();
                    bomObj.Add("FNumber", "");
                    entryObject.Add("FBomID", bomObj);

                    JObject unitObj = new JObject();
                    unitObj.Add("FNumber", dw);
                    entryObject.Add("FUnitID", unitObj);

                    JObject baseUnitObj = new JObject();
                    baseUnitObj.Add("FNumber", dw);
                    entryObject.Add("FBaseUnitID", baseUnitObj);

                    entryObject.Add("FQty", sl);
                    entryObject.Add("FBaseQty", sl);
                    entryObject.Add("FStartDate", sj);
                    entryObject.Add("FEndDate", sj);
                    entryObject.Add("FReserveType", "1");
                    entryObject.Add("FAVERATYPE", "N");

                    JObject stockOrgObj = new JObject();
                    stockOrgObj.Add("FNumber", "101");
                    entryObject.Add("FStockOrgId", stockOrgObj);

                    entryObject.Add("FCloseStatus", "N");

                    // 销售订单价格信息
                    entryObject.Add("F_XQ_YSL", sl);
                    entryObject.Add("F_XQ_DJ", dj);
                    entryObject.Add("F_XQ_HSDJ", hsdj);
                    entryObject.Add("F_XQ_SL", slv);
                    entryObject.Add("F_XQ_SE", se);
                    entryObject.Add("F_XQ_JE", je);
                    entryObject.Add("F_XQ_JSHJ", jshj);

                    entryArray.Add(entryObject);
                }

                // ---- 主表Model ----
                JObject modelObject = new JObject();
                modelObject.Add("FID", 0);

                JObject foreOrgObj = new JObject();
                foreOrgObj.Add("FNumber", "101");
                modelObject.Add("FForeOrgId", foreOrgObj);

                JObject billTypeObj = new JObject();
                billTypeObj.Add("FNUMBER", PLN_FORECAST_TYPE1);
                modelObject.Add("FBillTypeID", billTypeObj);

                modelObject.Add("FDate", today);
                modelObject.Add("F_PAEZ_NBYC", "false");
                modelObject.Add("F_PAEZ_YSDJH", bpmNo);
                JObject bibieObj = new JObject();
                bibieObj.Add("FNumber", bibie);
                modelObject.Add("F_XQ_BB", bibieObj);
                modelObject.Add("FEntity", entryArray);
                //单据编号FBillNo 
                modelObject.Add("FBillNo", salNo);

                // ---- Save ----
                JObject saveObj = new JObject
                {
                    ["Model"] = modelObject,
                    ["IsDeleteEntry"] = true
                };

                var jsonData = saveObj.ToString();
                WriteLog($"调用Save接口, JSON数据: {jsonData}");

                try
                {
                    var result = client.Save("PLN_FORECAST", jsonData);
                    WriteLog($"Save返回结果: {result}");

                    var saveResult = JObject.Parse(result);
                    var responseStatus = saveResult["Result"]?["ResponseStatus"];
                    if (responseStatus != null)
                    {
                        var isSuccess = responseStatus["IsSuccess"]?.Value<bool>() ?? false;
                        if (isSuccess)
                        {
                            var forecastId = saveResult["Result"]?["Id"]?.ToString() ?? "";
                            var forecastNumber = saveResult["Result"]?["Number"]?.ToString() ?? "";
                            WriteLog($"预测单创建成功: Id={forecastId}, Number={forecastNumber}");

                            // 反查预测单的FID和FENTRYID
                            var forecastSql = $@"SELECT A.FID, B.FENTRYID
                                      FROM T_PLN_FORECAST A
                                      JOIN T_PLN_FORECASTENTRY B ON A.FID = B.FID
                                      WHERE A.FBILLNO = '{forecastNumber}'";
                            WriteLog($"反查预测单SQL: {forecastSql}");

                            var forecastDt = DBUtils.ExecuteDynamicObject(this.Context, forecastSql);
                            if (forecastDt != null && forecastDt.Count > 0)
                            {
                                WriteLog($"反查预测单结果行数: {forecastDt.Count}");

                                // 逐行更新销售订单分录，按顺序匹配
                                for (int i = 0; i < forecastDt.Count && i < rows.Count; i++)
                                {
                                    var forecastFid = forecastDt[i]["FID"]?.ToString();
                                    var forecastEntryId = forecastDt[i]["FENTRYID"]?.ToString();
                                    var salEntryId = rows[i]["SALENTRYID"]?.ToString();

                                    var updateSql = $@"UPDATE T_SAL_ORDERENTRY
                                           SET F_UNW_Integer_re5 = {forecastFid},
                                               F_UNW_Integer_tzk = {forecastEntryId}
                                           WHERE FENTRYID = {salEntryId}";
                                    WriteLog($"更新销售订单分录: {updateSql}");
                                    DBUtils.Execute(this.Context, updateSql);
                                }
                                WriteLog("销售订单分录更新完成");
                            }
                            else
                            {
                                WriteLog($"反查预测单无结果, 预测单号={forecastNumber}");
                            }
                        }
                        else
                        {
                            var errors = responseStatus["Errors"] as JArray;
                            var errorMsg = errors != null && errors.Count > 0
                                ? errors[0]["Message"]?.ToString()
                                : "未知错误";
                            WriteLog($"预测单创建失败: {errorMsg}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteLog($"调用Save接口异常: {ex.Message}");
                }
            }

            WriteLog("========== 销售订单审核-预测单生成 结束 ==========");
        }

        private Dictionary<string, List<string>> GetBillTypeGroups(List<string> allIds)
        {
            var result = new Dictionary<string, List<string>>();
            if (allIds == null || allIds.Count == 0) return result;

            var ids = string.Join(",", allIds);
            var sql = $@"/*dialect*/SELECT A.FID, A.FBILLTYPEID FROM T_SAL_ORDER A WHERE A.FID IN ({ids})";
            var dt = DBUtils.ExecuteDynamicObject(this.Context, sql);
            foreach (DynamicObject row in dt)
            {
                var fid = row["FID"]?.ToString();
                var billType = row["FBILLTYPEID"]?.ToString();
                if (string.IsNullOrEmpty(fid) || string.IsNullOrEmpty(billType)) continue;
                if (!result.ContainsKey(billType))
                    result[billType] = new List<string>();
                result[billType].Add(fid);
            }
            return result;
        }
    }
}
