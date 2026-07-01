using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Linq;
using static Kingdee.K3.MFG.App.AppServiceContext;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Kingdee.BOS.Util;
using System.Data;

namespace zitn_prd_instock_audit_tobpm_pluginDevNew
{
    [Description("【服务插件】生产入库单：审核调用BpmApi")]
    [HotUpdate]
    public class Class1 : AbstractOperationServicePlugIn
    {
        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            try
            {
                base.AfterExecuteOperationTransaction(e);

                K3CloudApiClient client =
                    new K3CloudApiClient("http://10.0.32.18/K3Cloud/");

                var loginResult =
                    client.ValidateLogin(
                        "688399bec6449e ",
                        "admin",
                        "Flzx3qc!",
                        2052);
                var resultType =
                    JObject.Parse(loginResult)["LoginResultType"]
                    .Value<int>();

                if (resultType != 1)
                {
                    return;
                }

                var ids = string.Join(",",
                    e.DataEntitys.Select(o => o[0]));

                var query = string.Format(@"
SELECT Fysdjh
FROM T_PRD_INSTOCK
WHERE FID IN ({0})", ids);

                DynamicObjectCollection collection =
                    DbUtils.ExecuteDynamicObject(
                        this.Context,
                        query);

                if (collection == null || collection.Count == 0)
                {
                    return;
                }

                for (int i = 0; i < collection.Count; i++)
                {
                    string sequenceNo =
                        collection[i]["Fysdjh"]?.ToString();

                    var dataToSend = new
                    {
                        sequenceNo = sequenceNo
                    };

                    string jsonBody =
                        JsonConvert.SerializeObject(dataToSend);

                    string apiUrl =
                        "http://10.0.32.10:8769/api/public/aftersale/noticeSend";

                    CallApi(apiUrl, jsonBody);
                }
            }
            catch
            {
                // 不影响审核流程
            }
        }

        /// <summary>
        /// 调用API
        /// </summary>
        private void CallApi(string url, string jsonData)
        {
            try
            {
                string response;
                CallPostApi(url, jsonData, out response);
            }
            catch
            {
                // 不影响审核流程
            }
        }

        private bool CallPostApi(
            string url,
            string jsonData,
            out string responseText)
        {
            responseText = "";

            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.Headers[
                        HttpRequestHeader.ContentType] =
                        "application/json";

                    byte[] postBytes =
                        Encoding.UTF8.GetBytes(jsonData);

                    byte[] responseBytes =
                        webClient.UploadData(
                            url,
                            "POST",
                            postBytes);

                    responseText =
                        Encoding.UTF8.GetString(responseBytes);

                    return responseText.Contains("success")
                           || responseText.Contains("\"code\":200")
                           || responseText.Contains(@"""errcode"":0")
                           || responseText.Contains("操作成功");
                }
            }
            catch
            {
                return false;
            }
        }
    }
}