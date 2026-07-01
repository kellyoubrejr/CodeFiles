using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Util;
using Newtonsoft.Json;

namespace zitn_prd_instock_button_tobpm_pluginDev
{
    [Description("【表单插件】Zitn-yw】生产入库单：按钮点击后调用BpmApi")]
    [HotUpdate]
    public class Class1 : AbstractDynamicFormPlugIn
    {
        public override void BarItemClick(BarItemClickEventArgs e)
        {
            base.BarItemClick(e);

            if (!e.BarItemKey.Equals("bpmapi_btn", StringComparison.OrdinalIgnoreCase))
                return;

            var requestList = new List<object>();
            var uniqueKeys = new HashSet<string>();

            try
            {
                string sequenceNo = this.View.Model.GetValue("Fysdjh").ToString();

                if (sequenceNo == null)
                {
                    this.View.ShowErrMessage("未获取到有效的云枢单据号，操作已中止。");
                    return;
                }

                var dataToSend = new { sequenceNo = sequenceNo };
                string jsonBody = JsonConvert.SerializeObject(dataToSend);

                string apiUrl = "http://10.0.32.10:8769/api/public/aftersale/noticeSend";

                CallAndLog(apiUrl, jsonBody, sequenceNo);

                bool success = CallPostApi(apiUrl, jsonBody, out string response);

                if (!success)
                {
                    this.View.ShowErrMessage($"接口调用失败！\n返回信息：{response}");
                }
                else
                {
                    this.View.ShowMessage("接口调用成功！");
                }
            }
            catch (Exception ex)
            {
                this.View.ShowErrMessage($"系统异常：{ex.Message}");
            }
        }

        private void CallAndLog(string url, string jsonData, string sequenceNo)
        {
            bool success = false;
            string response = "未调用或调用前发生异常";

            try
            {
                success = CallPostApi(url, jsonData, out response);
            }
            catch (Exception apiCallEx)
            {
                success = false;
                response = $"调用API时发生异常: {apiCallEx.Message}";
            }

            string logPath = @"D:\金蝶自定义日志文件\生产入库单审核调用BpmApi.txt";
            try
            {
                using (StreamWriter sw = File.AppendText(logPath))
                {
                    if (success)
                    {
                        LogWithSeparator(sw, $"{DateTime.Now} 生产入库单审核调用BpmApi成功，单据号：{sequenceNo}");
                    }
                    else
                    {
                        LogWithSeparator(sw, $"{DateTime.Now} 生产入库单审核调用BpmApi失败，单据号：{sequenceNo}，错误信息：{response}");
                    }
                }
            }
            catch (Exception logEx)
            {
                Console.WriteLine($"向文件 {logPath} 写入日志时失败: {logEx.Message}");

            }
        }


        private bool CallPostApi(string url, string jsonData, out string responseText)
        {
            responseText = "";
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.Headers[HttpRequestHeader.ContentType] = "application/json";

                    byte[] postBytes = Encoding.UTF8.GetBytes(jsonData);
                    byte[] responseBytes = webClient.UploadData(url, "POST", postBytes);
                    responseText = Encoding.UTF8.GetString(responseBytes);

                    //return responseText.Contains("success") || responseText.Contains("\"code\":200");
                    return responseText.Contains("success") || responseText.Contains("\"code\":200") || responseText.Contains(@"""errcode"":0") || responseText.Contains("操作成功");
                }
            }
            catch (WebException webEx)
            {
                responseText = $"WebException: {webEx.Status} - {webEx.Message}";
                if (webEx.Response != null)
                {
                    using (var errorResponse = (HttpWebResponse)webEx.Response)
                    {
                        responseText += $"; HTTP Status Code: {errorResponse.StatusCode}; ";
                        try
                        {
                            using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                            {
                                string errorBody = reader.ReadToEnd();
                                responseText += $"Error Body: {errorBody}";
                            }
                        }
                        catch { }
                    }
                }
                return false;
            }
            catch (System.Net.Sockets.SocketException sockEx)
            {
                responseText = $"SocketException (网络连接错误): {sockEx.Message}";
                return false;
            }
            catch (TimeoutException timeoutEx)
            {
                responseText = $"TimeoutException: 请求超时 - {timeoutEx.Message}";
                return false;
            }
            catch (Exception ex)
            {
                responseText = $"General Exception in CallPostApi: {ex.Message}";
                return false;
            }
        }

        private void LogWithSeparator(string filePath, string message)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(filePath))
                {
                    LogWithSeparator(sw, message);
                }
            }
            catch
            {
                try
                {
                    using (StreamWriter sw = File.AppendText(filePath))
                    {
                        sw.WriteLine(message);
                    }
                }
                catch { }
            }
        }

        private void LogWithSeparator(StreamWriter streamWriter, string message)
        {
            streamWriter.WriteLine("==================================================");
            streamWriter.WriteLine(message);
            streamWriter.WriteLine("==================================================");
        }
    }
}
