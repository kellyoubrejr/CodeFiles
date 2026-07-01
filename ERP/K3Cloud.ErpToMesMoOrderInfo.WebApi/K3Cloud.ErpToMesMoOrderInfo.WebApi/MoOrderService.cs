using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.BOS.WebApi.ServicesStub;
using System;
using System.Collections.Generic;
using System.Data;
using Kingdee.BOS.App.Data;

namespace K3Cloud.ErpToMesMoOrderInfo.WebApi
{
    public class MoOrderService : AbstractWebApiBusinessService
    {
        public MoOrderService(KDServiceContext context)
            : base(context)
        {
        }

        /// <summary>
        /// 获取生产订单等信息
        /// </summary>
        /// <param name="queryDate">查询日期</param>
        /// <param name="onlyStart">是否只查询计划状态：1-只查计划状态，0-查所有状态</param>
        /// <returns></returns>
        public object GetTodoInfo(DateTime queryDate, int onlyStart)
        {
            var ctx = KDContext.Session.AppContext;
            if (ctx == null)
            {
                return new
                {
                    StatusCode = 401,
                    Message = "超时，请重新登录"
                };
            }

            if (queryDate == default(DateTime))
            {
                return new
                {
                    StatusCode = 400,
                    Message = "查询日期参数无效"
                };
            }

            if (onlyStart != 0 && onlyStart != 1)
            {
                return new
                {
                    StatusCode = 400,
                    Message = "onlyStart 参数无效，只能为 0 或 1"
                };
            }

            try
            {
                string dateParam = queryDate.ToString("yyyy-MM-dd");

                var result = DBUtils.ExecuteDataSet(ctx, $"EXEC sp_GetProductionOrderInfo '{dateParam}',{onlyStart}");

                var dataList = new List<Dictionary<string, object>>();

                if (result.Tables.Count > 0 && result.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in result.Tables[0].Rows)
                    {
                        var rowDict = new Dictionary<string, object>();
                        foreach (DataColumn column in result.Tables[0].Columns)
                        {
                            rowDict[column.ColumnName] = row[column];
                        }
                        dataList.Add(rowDict);
                    }
                }

                if (dataList.Count == 0)
                {
                    return new
                    {
                        StatusCode = 404,
                        Message = "未找到相关单据信息"
                    };
                }

                return new
                {
                    StatusCode = 200,
                    Data = dataList,
                    ApiName = System.Reflection.MethodBase.GetCurrentMethod().Name,
                    OnlyStart = onlyStart
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    StatusCode = 500,
                    Message = $"服务器错误: {ex.Message}"
                };
            }
        }
    }
}