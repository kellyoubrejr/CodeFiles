using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;

namespace Zitn_PurCloseTObpmHT_Plugin
{
    [Description("【服务插件】：采购订单关闭，调用bpm接口反po")]
    [Kingdee.BOS.Util.HotUpdate]
    public class PurClose : AbstractOperationServicePlugIn
    {
        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);

            var ids = string.Join(",", e.SelectedRows
                             .Select(row => row.DataEntity["Id"]?.ToString()));

            var requestList = GetPurchaseOrderData(ids);

            string jsonBody = JsonConvert.SerializeObject(requestList);
            //string apiUrl = "http://10.0.128.10:8081/api/public/contractAuditSeal/startWorkflowInstance";
            string apiUrl = "http://10.0.32.10:8769/api/public/contractAuditSeal/closePurchaseOrder";

            bool success = CallPostApi(apiUrl, jsonBody, out string response);

            if (!success)
            {
                //throw new KDBusinessException("调用合同审核签章接口失败:", $"错误信息：{response}，请处理");
            }
            else
            {
                // 接口调用成功，显示返回结果
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
                using (WebClient client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.Encoding = Encoding.UTF8;

                    byte[] postBytes = Encoding.UTF8.GetBytes(jsonData);
                    byte[] responseBytes = client.UploadData(url, "POST", postBytes);
                    responseText = Encoding.UTF8.GetString(responseBytes);

                    return responseText.Contains("success") || responseText.Contains("\"code\":200") || responseText.Contains("\"code\":0");
                }
            }
            catch (Exception ex)
            {
                responseText = ex.Message;
                return false;
            }
        }

        private object GetPurchaseOrderData(string ids)
        {
            var purDict = new List<string>();
            string poSql = string.Format($"/*dialect*/SELECT FBILLNO FROM T_PUR_POORDER WHERE FID IN ({ids})");
            DynamicObjectCollection poCol = DBUtils.ExecuteDynamicObject(this.Context, poSql);
            foreach (var po in poCol)
            {
                string billNo = po["FBILLNO"]?.ToString();
                purDict.Add(billNo);
            }
            return purDict;
        }
    }
}
