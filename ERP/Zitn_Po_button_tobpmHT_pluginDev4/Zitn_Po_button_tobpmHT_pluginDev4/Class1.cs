using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Util;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using Kingdee.BOS.Orm.DataEntity;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Kingdee.BOS.App.Data;

namespace Zitn_Po_button_tobpmHT_pluginDev4
{
    [Description("【表单插件】采购订单：按钮点击后调用合同BpmApi")]
    [HotUpdate]
    public class Class1 : AbstractDynamicFormPlugIn
    {

        private static readonly string LogPath = @"D:\金蝶自定义日志文件\采购订单审核推送BPM合同.txt";
        public override void BarItemClick(BarItemClickEventArgs e)
        {
            base.BarItemClick(e);

            if (!e.BarItemKey.Equals("bpmHTapi_btn", StringComparison.OrdinalIgnoreCase))
                return;

            string poBillno = this.View.Model.GetValue("FBillNo").ToString();

            var requestList = GetPurchaseOrderData(poBillno);

            string jsonBody = JsonConvert.SerializeObject(requestList);
            //string apiUrl = "http://10.0.128.10:8081/api/public/contractAuditSeal/startWorkflowInstance";
            string apiUrl = "http://10.0.32.10:8769/api/public/contractAuditSeal/generatePurchaseOrder";

            WriteLog("========== 开始手动推送新版 ==========");
            WriteLog($"单据: {poBillno}");
            WriteLog($"请求URL: {apiUrl}");
            WriteLog($"请求数据: {jsonBody}");
            LogEmptyFields(requestList);

            bool success = CallPostApi(apiUrl, jsonBody, out string response);

            if (!success)
            {
                WriteLog($"推送完成！返回信息: {response}");
                WriteLog("========== 推送结束 ==========");
                //throw new KDBusinessException("调用合同审核签章接口失败:", $"错误信息：{response}，请处理");
            }
            else
            {
                WriteLog($"推送完成！返回结果: {response}");
                WriteLog("========== 推送结束 ==========");
            }
        }

        /// <summary>
        /// 安全获取字符串值，null/DBNull 返回空字符串
        /// </summary>
        private static string SafeStr(DynamicObject obj, string field)
        {
            var val = obj[field];
            if (val == null || val == DBNull.Value) return "";
            return val.ToString();
        }

        /// <summary>
        /// 查询采购订单数据
        /// </summary>
        private List<Dictionary<string, object>> GetPurchaseOrderData(string poBillno)
        {
            // 主查询：PO + 物料（不JOIN付款计划，避免笛卡尔积）
            string poSql = $@"/*dialect*/SELECT DISTINCT
                                            A.FBILLNO AS PONO,
                                            A.FID AS POID,
                                            V.FNUMBER AS BUYER,
                                            B3.FDELIVERYDATE AS APPLYDATE,
                                            FTAXPRICE AS TAXPRICE,
                                            M.FNUMBER AS WLNUM,
                                            M1.FNAME AS WLNAME,
                                            M1.FSPECIFICATION AS WLSPEC,
                                            B.FNOTE,
                                            S.FNAME AS SUPPLIER,
                                        CASE

                                            WHEN M.FUSEORGID = '1' THEN
                                            '青岛智腾科技有限公司'
                                            WHEN M.FUSEORGID = '101006' THEN
                                            '青岛智腾微电子有限公司'
                                            WHEN M.FUSEORGID = '101007' THEN
                                            '青岛智腾电源有限公司'
                                            WHEN M.FUSEORGID = '101050' THEN
                                            'test'
                                            WHEN M.FUSEORGID = '1404303' THEN
                                            '青岛智腾烽行能源有限公司'
                                            WHEN M.FUSEORGID = '1516310' THEN
                                            '青岛晶英电子科技有限公司'
                                            WHEN M.FUSEORGID = '3149866' THEN
                                            '青岛智腾微电子有限公司北京分公司'
                                            WHEN M.FUSEORGID = '3241152' THEN
                                            '青岛加速度智能科技有限公司'
                                            WHEN M.FUSEORGID = '4032930' THEN
                                            '青岛智腾微电子有限公司西安分公司'
                                            WHEN M.FUSEORGID = '4665868' THEN
                                            '青岛智导电子有限公司'
                                            WHEN M.FUSEORGID = '4665869' THEN
                                            '青岛深科睿探技术有限公司'
                                            WHEN M.FUSEORGID = '4852744' THEN
                                            '青岛智导电子有限公司北京分公司'
                                            END AS POORG,
                                            FALLAMOUNT AS ALLPRICE,
                                            FQTY AS QTY,
                                            U.FNAME AS UNIT,
                                            S2.FOPENBANKNAME AS BANK,
                                            FBANKCODE AS ZH,
                                            TT.FSOCIALCRECODE AS SH,
                                            S4.FADDRESS AS ADDRESS,
                                            U1.FNAME AS WLDJ,
                                            FLEGALPERSON AS FR,
                                            TT1.FCONTACT AS WR,
                                            B1.FTAXRATE AS SL
                                        FROM
                                            T_PUR_POORDER A
                                            LEFT JOIN T_PUR_POORDERFIN A1 ON A.FID = A1.FID
                                            LEFT JOIN T_PUR_POORDERENTRY B ON A.FID = B.FID
                                            LEFT JOIN T_PUR_POORDERENTRY_F B1 ON B1.FENTRYID = B.FENTRYID
                                            LEFT JOIN T_PUR_POORDERENTRY_D B3 ON B3.FENTRYID = B.FENTRYID
                                            LEFT JOIN V_BD_BUYER V ON A.FPURCHASERID = V.FID
                                            LEFT JOIN T_BD_MATERIAL M ON B.FMATERIALID = M.FMATERIALID
                                            LEFT JOIN T_BD_MATERIAL_L M1 ON M.FMATERIALID = M1.FMATERIALID
                                            LEFT JOIN T_BD_SUPPLIER_L S ON A.FSUPPLIERID = S.FSUPPLIERID
                                            LEFT JOIN t_BD_SupplierBase TT ON S.FSUPPLIERID = TT.FSUPPLIERID
                                            LEFT JOIN t_BD_SupplierContact TT1 ON S.FSUPPLIERID = TT1.FSUPPLIERID
                                            LEFT JOIN T_BD_SUPPLIERBANK S1 ON S.FSUPPLIERID = S1.FSUPPLIERID
                                            LEFT JOIN T_BD_SUPPLIERBANK_L S2 ON S1.FBANKID = S2.FBANKID
                                            LEFT JOIN t_BD_SupplierFinance S3 ON S.FSUPPLIERID = S3.FSUPPLIERID
                                            LEFT JOIN t_BD_SupplierLocation S4 ON S.FSUPPLIERID = S4.FSUPPLIERID
                                            LEFT JOIN T_BD_UNIT_L U ON B.FUNITID = U.FUNITID
                                            LEFT JOIN UNW_t_Cust100015_L U1 ON M.F_PAEZ_ERPWLDJ = U1.FID
                            WHERE
                              A.FBILLNO = '{poBillno}'";

            // 付款计划独立查询：避免与物料表产生笛卡尔积
            string paySql = $@"/*dialect*/SELECT
                                            A.FID AS POID,
                                        CASE

                                            WHEN B2.F_ZMER_COMBO_QTR = '1' THEN
                                            '定金'
                                            WHEN B2.F_ZMER_COMBO_QTR = '2' THEN
                                            '款到发货'
                                            WHEN B2.F_ZMER_COMBO_QTR = '3' THEN
                                            '入库后账期付款'
                                            WHEN B2.F_ZMER_COMBO_QTR = '4' THEN
                                            '票后账期付款'
                                            WHEN B2.F_ZMER_COMBO_QTR = '5' THEN
                                            '质保金'
                                            ELSE '无'
                                            END AS FKFS,
                                            B2.FYFRATIO AS YFBL
                                        FROM
                                            T_PUR_POORDER A
                                            LEFT JOIN T_PUR_POORDERINSTALLMENT B2 ON A.FID = B2.FID
                                        WHERE
                                            A.FBILLNO = '{poBillno}'";

            DynamicObjectCollection POcol = DBUtils.ExecuteDynamicObject(this.Context, poSql);
            DynamicObjectCollection PAYcol = DBUtils.ExecuteDynamicObject(this.Context, paySql);

            // 按照POID分组
            var poGroups = new Dictionary<string, Dictionary<string, object>>();
            var materialsGroups = new Dictionary<string, List<Dictionary<string, object>>>();
            var paymentGroups = new Dictionary<string, List<Dictionary<string, object>>>();

            foreach (DynamicObject obj in POcol)
            {
                string poId = obj["POID"]?.ToString();
                if (string.IsNullOrEmpty(poId))
                {
                    continue;
                }

                if (!poGroups.ContainsKey(poId))
                {
                    var purDict = new Dictionary<string, object>();

                    purDict["po"] = SafeStr(obj, "PONO");
                    purDict["poId"] = SafeStr(obj, "POID");
                    purDict["buyer"] = SafeStr(obj, "BUYER");
                    purDict["applyDate"] = "";
                    if (obj["APPLYDATE"] != null && obj["APPLYDATE"] != DBNull.Value)
                    {
                        DateTime applyDate;
                        if (DateTime.TryParse(obj["APPLYDATE"].ToString(), out applyDate))
                        {
                            purDict["applyDate"] = applyDate.ToString("yyyy-MM-dd");
                        }
                        else
                        {
                            purDict["applyDate"] = obj["APPLYDATE"].ToString().Substring(0, 10);
                        }
                    }
                    purDict["supplier"] = SafeStr(obj, "SUPPLIER");
                    purDict["poOrg"] = SafeStr(obj, "POORG");
                    purDict["bank"] = SafeStr(obj, "BANK");
                    purDict["zh"] = SafeStr(obj, "ZH");
                    purDict["sh"] = SafeStr(obj, "SH");
                    purDict["address"] = SafeStr(obj, "ADDRESS");
                    purDict["fr"] = SafeStr(obj, "FR");
                    purDict["wr"] = SafeStr(obj, "WR");
                    purDict["sl"] = SafeStr(obj, "SL");

                    poGroups[poId] = purDict;
                    materialsGroups[poId] = new List<Dictionary<string, object>>();
                    paymentGroups[poId] = new List<Dictionary<string, object>>();
                }

                // 物料去重：相同物料编号只添加一次（避免供应商银行/联系人等1对多表产生的重复行）
                string wlnum = SafeStr(obj, "WLNUM");
                bool matExists = false;
                foreach (var existing in materialsGroups[poId])
                {
                    if (existing.TryGetValue("materialCode", out var eCode)
                        && (eCode?.ToString() ?? "") == wlnum)
                    {
                        matExists = true;
                        break;
                    }
                }
                if (!matExists)
                {
                    var materialDict = new Dictionary<string, object>();
                    materialDict["materialCode"] = wlnum;
                    materialDict["materialName"] = SafeStr(obj, "WLNAME");
                    materialDict["materialSpec"] = SafeStr(obj, "WLSPEC");
                    materialDict["materialLevel"] = SafeStr(obj, "WLDJ");
                    materialDict["qty"] = SafeStr(obj, "QTY");
                    materialDict["unit"] = SafeStr(obj, "UNIT");
                    materialDict["taxPrice"] = SafeStr(obj, "TAXPRICE");
                    materialDict["note"] = SafeStr(obj, "FNOTE");
                    materialDict["sl"] = SafeStr(obj, "SL");

                    decimal allPrice = 0;
                    if (obj["ALLPRICE"] != null && obj["ALLPRICE"] != DBNull.Value)
                    {
                        decimal.TryParse(obj["ALLPRICE"].ToString(), out allPrice);
                    }
                    materialDict["allPrice"] = allPrice;
                    materialDict["priceLower"] = allPrice.ToString("F2");

                    materialsGroups[poId].Add(materialDict);
                }
            }

            // 付款计划独立处理：从独立查询结果中读取，去重
            foreach (DynamicObject obj in PAYcol)
            {
                string poId = obj["POID"]?.ToString();
                if (string.IsNullOrEmpty(poId))
                {
                    continue;
                }

                if (!paymentGroups.ContainsKey(poId))
                {
                    paymentGroups[poId] = new List<Dictionary<string, object>>();
                }

                string fkfs = SafeStr(obj, "FKFS");
                string yfbl = SafeStr(obj, "YFBL");

                bool exists = false;
                foreach (var existing in paymentGroups[poId])
                {
                    if (existing.TryGetValue("fkfs", out var efkfs) && existing.TryGetValue("yfbl", out var eyfbl))
                    {
                        if ((efkfs?.ToString() ?? "") == fkfs && (eyfbl?.ToString() ?? "") == yfbl)
                        {
                            exists = true;
                            break;
                        }
                    }
                }
                if (!exists)
                {
                    var paymentDict = new Dictionary<string, object>();
                    paymentDict["fkfs"] = fkfs;
                    paymentDict["yfbl"] = yfbl;
                    paymentGroups[poId].Add(paymentDict);
                }
            }

            var requestList = new List<Dictionary<string, object>>();

            foreach (var poId in poGroups.Keys)
            {
                var orderDict = new Dictionary<string, object>();

                var purData = poGroups[poId];

                var materials = materialsGroups[poId];
                decimal totalAllPrice = 0;
                foreach (var material in materials)
                {
                    totalAllPrice += Convert.ToDecimal(material["allPrice"]);
                }

                string totalPriceUpper = ConvertToChineseAmountFallback(totalAllPrice);
                string totalPriceLower = totalAllPrice.ToString("F2");

                purData["priceUp"] = totalPriceUpper;
                purData["priceLower"] = totalPriceLower;

                orderDict["pur"] = purData;

                orderDict["materials"] = materials;

                orderDict["payment"] = paymentGroups[poId];

                requestList.Add(orderDict);
            }

            return requestList;
        }

        private string ConvertToChineseAmountFallback(decimal amount)
        {
            if (amount == 0) return "零元整";

            string[] cnNum = { "零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖" };
            string[] cnUnit = { "", "拾", "佰", "仟" };
            string[] cnBigUnit = { "", "万", "亿" };

            string amountStr = Math.Round(amount, 2).ToString("F2");
            string integerPart = amountStr.Split('.')[0];
            string decimalPart = amountStr.Split('.')[1];

            StringBuilder result = new StringBuilder();

            int len = integerPart.Length;
            int bigUnitIndex = 0;
            bool isZero = true;

            for (int i = len - 1; i >= 0; i--)
            {
                int digit = int.Parse(integerPart[i].ToString());
                int pos = len - 1 - i;
                int unitPos = pos % 4;

                if (pos % 4 == 0 && pos > 0)
                {
                    bigUnitIndex++;
                    if (digit != 0 || (i + 1 < len && integerPart[i + 1] != '0'))
                    {
                        result.Insert(0, cnBigUnit[bigUnitIndex]);
                    }
                }

                if (digit != 0)
                {
                    if (unitPos == 0)
                    {
                        result.Insert(0, cnNum[digit]);
                    }
                    else
                    {
                        result.Insert(0, cnUnit[unitPos]);
                        result.Insert(0, cnNum[digit]);
                    }
                    isZero = false;
                }
                else
                {
                    if (!isZero && i > 0 && integerPart[i - 1] != '0')
                    {
                        result.Insert(0, "零");
                    }
                }
            }

            if (isZero)
            {
                result.Append("零");
            }
            result.Append("元");

            int jiao = int.Parse(decimalPart[0].ToString());
            int fen = int.Parse(decimalPart[1].ToString());

            if (jiao == 0 && fen == 0)
            {
                result.Append("整");
            }
            else
            {
                if (jiao != 0)
                {
                    result.Append(cnNum[jiao]);
                    result.Append("角");
                }
                if (fen != 0)
                {
                    result.Append(cnNum[fen]);
                    result.Append("分");
                }
            }

            return result.ToString();
        }


        /// <summary>
        /// 记录请求数据中各字段的空值情况，方便定位"参数不能为空"是哪个字段
        /// </summary>
        private static void LogEmptyFields(List<Dictionary<string, object>> requestList)
        {
            for (int i = 0; i < requestList.Count; i++)
            {
                var order = requestList[i];
                WriteLog($"--- 第{i + 1}条订单字段检查 ---");

                if (order.TryGetValue("pur", out var purObj) && purObj is Dictionary<string, object> pur)
                {
                    foreach (var kv in pur)
                    {
                        var val = kv.Value;
                        bool isEmpty = val == null || (val is string s && string.IsNullOrEmpty(s));
                        if (isEmpty)
                        {
                            WriteLog($"  [空] pur.{kv.Key} = {(val == null ? "null" : "\"\"")}");
                        }
                    }
                }

                if (order.TryGetValue("materials", out var matsObj) && matsObj is List<Dictionary<string, object>> materials)
                {
                    for (int m = 0; m < materials.Count; m++)
                    {
                        foreach (var kv in materials[m])
                        {
                            var val = kv.Value;
                            bool isEmpty = val == null || (val is string s && string.IsNullOrEmpty(s));
                            if (isEmpty)
                            {
                                WriteLog($"  [空] materials[{m}].{kv.Key} = {(val == null ? "null" : "\"\"")}");
                            }
                        }
                    }
                }

                if (order.TryGetValue("payment", out var payObj) && payObj is List<Dictionary<string, object>> payments)
                {
                    for (int p = 0; p < payments.Count; p++)
                    {
                        foreach (var kv in payments[p])
                        {
                            var val = kv.Value;
                            bool isEmpty = val == null || (val is string s && string.IsNullOrEmpty(s));
                            if (isEmpty)
                            {
                                WriteLog($"  [空] payment[{p}].{kv.Key} = {(val == null ? "null" : "\"\"")}");
                            }
                        }
                    }
                }
            }
            WriteLog("--- 字段检查完毕 ---");
        }

        private static void WriteLog(string message)
        {
            try
            {
                string dir = Path.GetDirectoryName(LogPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.AppendAllText(LogPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  {message}{Environment.NewLine}");
            }
            catch
            {
                // 日志写入失败不抛异常，避免影响主流程
            }
        }

        /// <summary>
        /// 调用POST接口
        /// </summary>
        private bool CallPostApi(string url, string jsonData, out string responseText)
        {
            responseText = "";
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Timeout = 30000;

                byte[] postBytes = Encoding.UTF8.GetBytes(jsonData);
                request.ContentLength = postBytes.Length;
                using (Stream reqStream = request.GetRequestStream())
                {
                    reqStream.Write(postBytes, 0, postBytes.Length);
                }

                HttpWebResponse response;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException webEx)
                {
                    response = (HttpWebResponse)webEx.Response;
                }

                if (response == null)
                {
                    responseText = "接口无响应";
                    WriteLog($"接口返回异常: 无响应");
                    return false;
                }

                int statusCode = (int)response.StatusCode;
                WriteLog($"HTTP状态码: {statusCode}");

                string body;
                using (Stream respStream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(respStream, Encoding.UTF8))
                {
                    body = reader.ReadToEnd();
                }
                response.Dispose();

                WriteLog($"接口返回内容: {body}");

                responseText = body;
                return statusCode == 200 && (body.Contains("success") || body.Contains("\"code\":200") || body.Contains("\"code\":0"));
            }
            catch (Exception ex)
            {
                responseText = ex.Message;
                WriteLog($"接口调用异常: {ex.Message}");
                return false;
            }
        }
    }
}
