using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.BOS.WebApi.ServicesStub;
using System;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using Kingdee.BOS.WebApi.Client;
using K3Cloud.PlnForecastConvertSalOrder.WebApi.Models;

namespace K3Cloud.PlnForecastConvertSalOrder.WebApi
{
    [Description("【预测-->销售MRP接口】查询销售上预测分录id，方法入参接口传入进来的json数据包，" +
        "调用预测下推销售接口保存操作"), HotUpdate]
    public class ConvertService : AbstractWebApiBusinessService
    {
        public ConvertService(KDServiceContext context)
            : base(context)
        {
        }

        /// <summary>
        /// 接收BPM保存销售订单JSON，处理预测单下推销售订单逻辑
        /// </summary>
        /// <param name="request">BPM传入的原始JSON数据包（JObject保证所有字段不丢失）</param>
        /// <returns>返回销售订单编码、内码、分录内码</returns>
        public ProcessResult ProcessBpmSaveOrder(JObject request)
        {
            var ctx = KDContext.Session.AppContext;
            if (ctx == null)
            {
                return new ProcessResult
                {
                    StatusCode = 401,
                    Message = "超时，请重新登录"
                };
            }

            if (request == null || request["model"] == null)
            {
                return new ProcessResult
                {
                    StatusCode = 400,
                    Message = "参数无效：缺少model数据"
                };
            }

            try
            {
                // 1.查询销售订单上的预测单分录id
                var sql = @"SELECT F_UNW_Integer_tzk FROM T_SAL_ORDERENTRY WHERE FENTRYID = 001";
                var data = DBUtils.ExecuteDataSet(ctx, sql);

                var forecastEntryId = string.Empty;
                if (data != null && data.Tables.Count > 0 && data.Tables[0].Rows.Count > 0)
                {
                    forecastEntryId = data.Tables[0].Rows[0]["F_UNW_Integer_tzk"]?.ToString() ?? string.Empty;
                }

                // 2.接收BPM保存JSON：model是原始完整的销售订单数据，后续Save直接用它
                var modelJson = request["model"] as JObject;
                var requestDto = request.ToObject<BpmSaveOrderRequest>();

                // 登录K3Cloud API
                var client = new K3CloudApiClient(ApiConfig.K3CloudUrl);
                var loginResult = client.ValidateLogin(
                    ApiConfig.AppId,
                    ApiConfig.UserName,
                    ApiConfig.Password,
                    ApiConfig.Lcid);
                if (JObject.Parse(loginResult)["LoginResultType"].Value<int>() != 1)
                {
                    return new ProcessResult
                    {
                        StatusCode = 500,
                        Message = "K3Cloud登录失败: " + loginResult
                    };
                }

                // 3.保存销售订单（使用BPM传入的原始完整JSON）
                var saveReqObj = new JObject
                {
                    ["Model"] = modelJson,
                    ["IsDeleteEntry"] = false
                };

                var saveResultStr = client.Save("SAL_SaleOrder", saveReqObj.ToString());
                var saveResultObj = JObject.Parse(saveResultStr);

                if (!saveResultObj["Result"]["ResponseStatus"]["IsSuccess"].Value<bool>())
                {
                    var errMsg = saveResultObj["Result"]["ResponseStatus"]["Errors"]?[0]?["Message"]?.ToString() ?? "未知错误";
                    return new ProcessResult
                    {
                        StatusCode = 500,
                        Message = "销售订单保存失败: " + errMsg
                    };
                }

                var saveSuccessList = saveResultObj["Result"]["ResponseStatus"]["SuccessEntitys"] as JArray;
                var saveItem = saveSuccessList?[0];
                var fid = saveItem?["Id"]?.Value<long>() ?? 0;
                var fBillNo = saveItem?["Number"]?.ToString() ?? "";
                var entryIds = saveItem?["EntryIds"]?["FEntity"] as JArray;
                var fEntryId = entryIds?[0]?.Value<long>() ?? 0;

                // 4.预测单下推销售订单
                var pushObj = new JObject
                {
                    ["Ids"] = "",
                    ["Numbers"] = new JArray(),
                    ["EntryIds"] = forecastEntryId,
                    ["RuleId"] = "",
                    ["TargetBillTypeId"] = "",
                    ["TargetOrgId"] = 0,
                    ["TargetFormId"] = "SAL_SaleOrder",
                    ["IsEnableDefaultRule"] = "false",
                    ["IsDraftWhenSaveFail"] = "false",
                    ["CustomParams"] = new JObject()
                };

                var pushResultStr = client.Push("PLN_FORECAST", pushObj.ToString());
                var pushResultObj = JObject.Parse(pushResultStr);

                if (!pushResultObj["Result"]["ResponseStatus"]["IsSuccess"].Value<bool>())
                {
                    var errMsg = pushResultObj["Result"]["ResponseStatus"]["Errors"]?[0]?["Message"]?.ToString() ?? "未知错误";
                    return new ProcessResult
                    {
                        StatusCode = 500,
                        Message = "预测单下推销售订单失败: " + errMsg
                    };
                }

                // 5.保存销售订单（下推后字段修改）
                var pushSuccessList = pushResultObj["Result"]["ResponseStatus"]["SuccessEntitys"] as JArray;
                if (pushSuccessList != null && pushSuccessList.Count > 0)
                {
                    var pushItem = pushSuccessList[0];
                    long pushFid = pushItem["Id"].Value<long>();
                    var pushEntryIds = pushItem["EntryIds"]["FEntity"] as JArray;

                    var saveModel = modelJson;
                    saveModel["FID"] = pushFid;

                    if (pushEntryIds != null && saveModel["FSaleOrderEntry"] is JArray entryArr)
                    {
                        for (int i = 0; i < entryArr.Count && i < pushEntryIds.Count; i++)
                        {
                            entryArr[i]["FEntryID"] = pushEntryIds[i].Value<long>();
                        }
                    }

                    var finalSaveObj = new JObject
                    {
                        ["Model"] = saveModel,
                        ["IsDeleteEntry"] = false
                    };

                    client.Save("SAL_SaleOrder", finalSaveObj.ToString());
                }

                return new ProcessResult
                {
                    StatusCode = 200,
                    Message = "成功",
                    FBillNo = fBillNo,
                    FId = (int)fid,
                    FEntryId = (int)fEntryId
                };
            }
            catch (Exception ex)
            {
                return new ProcessResult
                {
                    StatusCode = 500,
                    Message = "服务器错误: " + ex.Message
                };
            }
        }
    }
}
