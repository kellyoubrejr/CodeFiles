using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.BOS.WebApi.ServicesStub;
using System;
using System.Collections.Generic;
using System.Data;
using Kingdee.BOS.App.Data;

namespace K3Cloud.K3cloudPurTOBPM.WebApi
{
    /// <summary>
    /// 采购需求N表同步BPM
    /// 
    /// </summary>
    public class PurTOBPMService : AbstractWebApiBusinessService
    {
        public PurTOBPMService(KDServiceContext context)
            : base(context)
        {
        }
        /// <summary>
        /// 查询采购需求数据（调用存储过程 pr_purapplicationV1_0）
        /// </summary>
        /// <param name="startDate">开始时间（格式：yyyy-MM）</param>
        /// <param name="endDate">结束时间（格式：yyyy-MM）</param>
        /// <returns>列表数据</returns>
        public object GetPurChaseInfo(string startDate, string endDate)
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

            try
            {
                if (string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(endDate))
                {
                    return new
                    {
                        StatusCode = 400,
                        Message = "开始时间和结束时间不能为空，格式：yyyy-MM"
                    };
                }

                string sql = $"EXEC pr_purapplicationV1_0 '{startDate}','{endDate}'";
                var result = DBUtils.ExecuteDataSet(ctx, sql);

                var dataList = new List<Dictionary<string, object>>();

                if (result.Tables.Count > 0 && result.Tables[0].Rows.Count > 0)
                {
                    DataTable table = result.Tables[0];

                    foreach (DataRow row in table.Rows)
                    {
                        var rowDict = new Dictionary<string, object>();
                        rowDict["buyer"] = row["采购员"] == DBNull.Value ? null : row["采购员"];

                        rowDict["demandTotal"] = row["需求总条数"] == DBNull.Value ? 0 : row["需求总条数"];
                        rowDict["demandOk"] = row["需求满足条数"] == DBNull.Value ? 0 : row["需求满足条数"];
                        rowDict["demandNg"] = row["需求不满足条数"] == DBNull.Value ? 0 : row["需求不满足条数"];
                        rowDict["demandRate"] = row["需求满足率"] == DBNull.Value ? 0 : row["需求满足率"];

                        rowDict["orderOnTime"] = row["下单及时数"] == DBNull.Value ? 0 : row["下单及时数"];
                        rowDict["orderLate"] = row["下单不及时数"] == DBNull.Value ? 0 : row["下单不及时数"];
                        rowDict["orderRate"] = row["下单及时率"] == DBNull.Value ? 0 : row["下单及时率"];

                        rowDict["deliveryOnTime"] = row["交付及时数"] == DBNull.Value ? 0 : row["交付及时数"];
                        rowDict["deliveryLate"] = row["交付不及时数"] == DBNull.Value ? 0 : row["交付不及时数"];
                        rowDict["deliveryRate"] = row["交付及时率"] == DBNull.Value ? 0 : row["交付及时率"];


                        rowDict["purchaseAmountTotal"] = row["采购总金额"] == DBNull.Value ? 0 : row["采购总金额"];
                        rowDict["purchaseCount"] = row["采购条数"] == DBNull.Value ? 0 : row["采购条数"];
                        rowDict["totalAmountAll"] = row["全部总金额"] == DBNull.Value ? 0 : row["全部总金额"];
                        rowDict["totalCountAll"] = row["全部总条数"] == DBNull.Value ? 0 : row["全部总条数"];
                        rowDict["purchaseCountRatio"] = row["条数占比"] == DBNull.Value ? 0 : row["条数占比"];
                        rowDict["purchaseAmountRatio"] = row["金额占比"] == DBNull.Value ? 0 : row["金额占比"];
                        rowDict["returnTotalCount"] = row["采购退料总条数"] == DBNull.Value ? 0 : row["采购退料总条数"];
                        rowDict["returnOnTimeCount"] = row["采购退料及时条数"] == DBNull.Value ? 0 : row["采购退料及时条数"];
                        rowDict["returnDelayCount"] = row["采购退料不及时条数"] == DBNull.Value ? 0 : row["采购退料不及时条数"];
                        rowDict["returnOnTimeRate"] = row["采购退料及时率"] == DBNull.Value ? 0 : row["采购退料及时率"];
                        rowDict["prodReturnTotal"] = row["生产退料总单数"] == DBNull.Value ? 0 : row["生产退料总单数"];
                        rowDict["prodReturnHandled"] = row["生产退料已处理"] == DBNull.Value ? 0 : row["生产退料已处理"];
                        rowDict["prodReturnUnhandled"] = row["生产退料未处理"] == DBNull.Value ? 0 : row["生产退料未处理"];
                        rowDict["prodReturnRate"] = row["生产退料及时率"] == DBNull.Value ? 0 : row["生产退料及时率"];

                        dataList.Add(rowDict);
                    }
                }
                else
                {
                    return new
                    {
                        StatusCode = 200,
                        Data = new List<Dictionary<string, object>>(),
                        ApiName = System.Reflection.MethodBase.GetCurrentMethod().Name
                    };
                }

                return new
                {
                    StatusCode = 200,
                    Data = dataList,
                    ApiName = System.Reflection.MethodBase.GetCurrentMethod().Name
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

        /// <summary>
        /// 查询供应商付款数据
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public object SupplierFKTOBPMService(string startDate, string endDate)
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

            try
            {
                if (string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(endDate))
                {
                    return new
                    {
                        StatusCode = 400,
                        Message = "开始时间和结束时间不能为空，格式：yyyy-MM"
                    };
                }

                string sql = string.Format($@"/*dialect*/SELECT     S.FNAME as 供应商名称,
                                                SUM(B.FAPPLYAMOUNTFOR)as 申请付款总金额, 
                                                SUM(SUM(B.FAPPLYAMOUNTFOR)) OVER()  as 全部申请付款金额
                                            FROM T_CN_PAYAPPLY A
                                            JOIN T_CN_PAYAPPLYENTRY B ON A.FID = B.FID
                                            JOIN T_BD_SUPPLIER_L S ON A.FRECTUNIT = S.FSUPPLIERID
                                            WHERE
                                                A.FRECTUNITTYPE = 'BD_Supplier'
                                                AND A.FCREATEDATE >= '{startDate}'
                                                AND A.FCREATEDATE < '{endDate}'
                                            GROUP BY
                                                S.FNAME
                                            ORDER BY
                                                申请付款总金额 DESC; ");
                var result = DBUtils.ExecuteDataSet(ctx, sql);

                var dataList = new List<Dictionary<string, object>>();

                if (result.Tables.Count > 0 && result.Tables[0].Rows.Count > 0)
                {
                    DataTable table = result.Tables[0];

                    foreach (DataRow row in table.Rows)
                    {
                        var rowDict = new Dictionary<string, object>();
                        rowDict["supplierName"] = row["供应商名称"] == DBNull.Value ? null : row["供应商名称"];

                        rowDict["amount"] = row["申请付款总金额"] == DBNull.Value ? 0 : row["申请付款总金额"];
                        rowDict["totalAmount"] = row["全部申请付款金额"] == DBNull.Value ? 0 : row["全部申请付款金额"];

                        dataList.Add(rowDict);
                    }
                }
                else
                {
                    return new
                    {
                        StatusCode = 200,
                        Data = new List<Dictionary<string,object>>(),   
                        ApiName = System.Reflection.MethodBase.GetCurrentMethod().Name
                    };
                }

                return new
                {
                    StatusCode = 200,
                    Data = dataList,
                    ApiName = System.Reflection.MethodBase.GetCurrentMethod().Name
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

        /// <summary>
        /// 查询费用项目数据
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public object CostItemsTOBPMService(string startDate, string endDate)
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

            try
            {
                if (string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(endDate))
                {
                    return new
                    {
                        StatusCode = 400,
                        Message = "开始时间和结束时间不能为空，格式：yyyy-MM"
                    };
                }

                string sql = string.Format($@"/*dialect*/SELECT
                                                  E.FNAME AS 费用项目,
                                                  SUM(CASE WHEN B.FSOURCETYPE = 'PUR_PurchaseOrder' THEN B.FAPPLYAMOUNTFOR ELSE 0 END) AS 采购订单,
                                                  SUM(CASE WHEN B.FSOURCETYPE = 'AP_Payable' THEN B.FAPPLYAMOUNTFOR ELSE 0 END) AS 应付单,
                                                  SUM(B.FAPPLYAMOUNTFOR) AS 合计,
                                                  ROUND(
                                                    CASE
                                                      WHEN SUM(B.FAPPLYAMOUNTFOR) = 0 THEN 0
                                                      ELSE(SUM(CASE WHEN B.FSOURCETYPE = 'PUR_PurchaseOrder' THEN B.FAPPLYAMOUNTFOR ELSE 0 END) * 100.0 / SUM(B.FAPPLYAMOUNTFOR))
                                                    END, 2
                                                  ) AS 比例
                                                FROM T_CN_PAYAPPLY A
                                                JOIN T_CN_PAYAPPLYENTRY B ON A.FID = B.FID
                                                JOIN T_BD_EXPENSE_L E ON B.FCOSTID = E.FEXPID
                                                WHERE
                                                  A.FRECTUNITTYPE = 'BD_Supplier'
                                                  AND B.FCOSTID IN(20045, 117490)
                                                  AND B.FSOURCETYPE IN('PUR_PurchaseOrder', 'AP_Payable')
                                                  AND A.FCREATEDATE >= '{startDate}'
                                                  AND A.FCREATEDATE < '{endDate}'
                                                  AND A.FDOCUMENTSTATUS = 'C'
                                                GROUP BY E.FNAME

                                                UNION ALL
                                                SELECT
                                                  '总计' AS 费用项目,
                                                  SUM(CASE WHEN B.FSOURCETYPE = 'PUR_PurchaseOrder' THEN B.FAPPLYAMOUNTFOR ELSE 0 END) AS 采购订单,
                                                  SUM(CASE WHEN B.FSOURCETYPE = 'AP_Payable' THEN B.FAPPLYAMOUNTFOR ELSE 0 END) AS 应付单,
                                                  SUM(B.FAPPLYAMOUNTFOR) AS 合计,
                                                  ROUND(
                                                    CASE
                                                      WHEN SUM(B.FAPPLYAMOUNTFOR) = 0 THEN 0
                                                      ELSE(SUM(CASE WHEN B.FSOURCETYPE = 'PUR_PurchaseOrder' THEN B.FAPPLYAMOUNTFOR ELSE 0 END) * 100.0 / SUM(B.FAPPLYAMOUNTFOR))
                                                    END, 2
                                                  ) AS 比例
                                                FROM T_CN_PAYAPPLY A
                                                JOIN T_CN_PAYAPPLYENTRY B ON A.FID = B.FID
                                                JOIN T_BD_EXPENSE_L E ON B.FCOSTID = E.FEXPID
                                                WHERE
                                                  A.FRECTUNITTYPE = 'BD_Supplier'
                                                  AND B.FCOSTID IN(20045, 117490)
                                                  AND B.FSOURCETYPE IN('PUR_PurchaseOrder', 'AP_Payable')
                                                  AND A.FCREATEDATE >= '{startDate}'
                                                  AND A.FCREATEDATE < '{endDate}'
                                                  AND A.FDOCUMENTSTATUS = 'C'   ");
                var result = DBUtils.ExecuteDataSet(ctx, sql);

                var dataList = new List<Dictionary<string, object>>();

                if (result.Tables.Count > 0 && result.Tables[0].Rows.Count > 0)
                {
                    DataTable table = result.Tables[0];

                    foreach (DataRow row in table.Rows)
                    {
                        var rowDict = new Dictionary<string, object>();
                        rowDict["feeItem"] = row["费用项目"] == DBNull.Value ? null : row["费用项目"];

                        rowDict["purchaseOrderAmount"] = row["采购订单"] == DBNull.Value ? 0 : row["采购订单"];
                        rowDict["apAmount"] = row["应付单"] == DBNull.Value ? 0 : row["应付单"];
                        rowDict["totalAmount"] = row["合计"] == DBNull.Value ? 0 : row["合计"];
                        rowDict["ratio"] = row["比例"] == DBNull.Value ? 0 : row["比例"];

                        dataList.Add(rowDict);
                    }
                }
                else
                {
                    return new
                    {
                        StatusCode = 200,
                        Data = new List<Dictionary<string, object>>(),
                        ApiName = System.Reflection.MethodBase.GetCurrentMethod().Name
                    };
                }

                return new
                {
                    StatusCode = 200,
                    Data = dataList,
                    ApiName = System.Reflection.MethodBase.GetCurrentMethod().Name
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

        /// <summary>
        /// 采购检验
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public object WXLLTOBPMService(string startDate, string endDate)
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

            try
            {
                if (string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(endDate))
                {
                    return new
                    {
                        StatusCode = 400,
                        Message = "开始时间和结束时间不能为空，格式：yyyy-MM"
                    };
                }

                string sql = string.Format($@"/*dialect*/SELECT
                                                          S.FNAME AS 供应商,
                                                          SUM(B.FINSPECTQTY) AS 检验数量,
                                                          SUM(B.FQUALIFIEDQTY) AS 合格数,
                                                          SUM(B.FUNQUALIFIEDQTY) AS 不合格数,
                                                          (SUM(B.FQUALIFIEDQTY) / SUM(B.FINSPECTQTY)) * 100 AS 合格率,
                                                          (SUM(B.FQUALIFIEDQTY) + SUM(CASE WHEN T1.FDEFPROCESS = 'B' THEN T1.FDEFECTIVEQTY ELSE 0 END)) AS 接收,
                                                          SUM(CASE WHEN T1.FDEFPROCESS = 'B' THEN T1.FDEFECTIVEQTY ELSE 0 END) AS 让步接收数量,
                                                          SUM(CASE WHEN T1.FDEFPROCESS = 'F' THEN T1.FDEFECTIVEQTY ELSE 0 END) AS 判退数量,
                                                          (SUM(B.FQUALIFIEDQTY) + SUM(CASE WHEN T1.FDEFPROCESS = 'B' THEN T1.FDEFECTIVEQTY ELSE 0 END)) / SUM(B.FINSPECTQTY) * 100 AS 接受率
                                                        FROM
                                                          T_QM_INSPECTBILL A
                                                          JOIN T_QM_INSPECTBILLENTRY B ON A.FID = B.FID
                                                          JOIN T_BD_SUPPLIER_L S ON B.FSUPPLIERID = S.FSUPPLIERID
                                                          LEFT JOIN T_QM_DEFECTPROCESSENTRY T1 ON A.FBILLNO = T1.FSOURCEBILLNO
                                                          LEFT JOIN T_QM_DEFECTPROCESS T ON T.FID = T1.FID
                                                            WHERE
                                                         A.FCREATEDATE >= '{startDate}'
                                                  AND A.FCREATEDATE < '{endDate}'
                                                        GROUP BY
                                                          S.FNAME");
                var result = DBUtils.ExecuteDataSet(ctx, sql);

                var dataList = new List<Dictionary<string, object>>();

                if (result.Tables.Count > 0 && result.Tables[0].Rows.Count > 0)
                {
                    DataTable table = result.Tables[0];

                    foreach (DataRow row in table.Rows)
                    {
                        var rowDict = new Dictionary<string, object>();
                        rowDict["supplier"] = row["供应商"] == DBNull.Value ? null : row["供应商"];

                        rowDict["inspectionQty"] = row["检验数量"] == DBNull.Value ? 0 : row["检验数量"];
                        rowDict["qualifiedQty"] = row["合格数"] == DBNull.Value ? 0 : row["合格数"];
                        rowDict["unqualifiedQty"] = row["不合格数"] == DBNull.Value ? 0 : row["不合格数"];
                        rowDict["passRate"] = row["合格率"] == DBNull.Value ? 0 : row["合格率"];
                        rowDict["received"] = row["接收"] == DBNull.Value ? 0 : row["接收"];
                        rowDict["concession"] = row["让步接收数量"] == DBNull.Value ? 0 : row["让步接收数量"];
                        rowDict["rejected"] = row["判退数量"] == DBNull.Value ? 0 : row["判退数量"];
                        rowDict["acceptRate"] = row["接受率"] == DBNull.Value ? 0 : row["接受率"];

                        dataList.Add(rowDict);
                    }
                }
                else
                {
                    return new
                    {
                        StatusCode = 200,
                        Data = new List<Dictionary<string, object>>(),
                        ApiName = System.Reflection.MethodBase.GetCurrentMethod().Name
                    };
                }

                return new
                {
                    StatusCode = 200,
                    Data = dataList,
                    ApiName = System.Reflection.MethodBase.GetCurrentMethod().Name
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

        /// <summary>
        /// 未下单：采购申请数量-已下推采购订单数量
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public object NoDownOrderTOBPMService(string startDate, string endDate)
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

            try
            {
                if (string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(endDate))
                {
                    return new
                    {
                        StatusCode = 400,
                        Message = "开始时间和结束时间不能为空，格式：yyyy-MM"
                    };
                }

                int reqQty = 0;
                int purQty = 0;

                // ================== 采购申请数量 ==================
                string sql = $@"/*dialect*/
            SELECT COUNT(*) AS QTY 
            FROM T_PUR_Requisition 
            WHERE FAPPLICATIONDATE >= '{startDate}'
              AND FAPPLICATIONDATE < '{endDate}'AND FDOCUMENTSTATUS = 'C'";

                var result = DBUtils.ExecuteDataSet(ctx, sql);

                if (result.Tables.Count > 0 && result.Tables[0].Rows.Count > 0)
                {
                    reqQty = result.Tables[0].Rows[0]["QTY"] == DBNull.Value
                        ? 0
                        : Convert.ToInt32(result.Tables[0].Rows[0]["QTY"]);
                }

                // ================== 已下推采购订单数量 ==================
                string sql2 = $@"/*dialect*/
            SELECT COUNT(*) AS cyqty
            FROM T_PUR_Requisition A
            JOIN T_PUR_POORDER P3 ON P3.FID = A.FID
            WHERE A.FAPPLICATIONDATE >= '{startDate}'
              AND A.FAPPLICATIONDATE < '{endDate}'";

                var result2 = DBUtils.ExecuteDataSet(ctx, sql2);

                if (result2.Tables.Count > 0 && result2.Tables[0].Rows.Count > 0)
                {
                    purQty = result2.Tables[0].Rows[0]["cyqty"] == DBNull.Value
                        ? 0
                        : Convert.ToInt32(result2.Tables[0].Rows[0]["cyqty"]);
                }

                // ================== 计算差值 ==================
                int diff = reqQty - purQty;

                if (diff < 0)
                {
                    diff = 0;
                }

                // ================== 统一列表结构返回 ==================
                var dataList = new List<Dictionary<string, object>>();

                var rowDict = new Dictionary<string, object>();

                rowDict["diffQty"] = diff;

                dataList.Add(rowDict);

                return new
                {
                    StatusCode = 200,
                    Data = dataList,   
                    ApiName = System.Reflection.MethodBase.GetCurrentMethod().Name
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

        /// <summary>
        /// 未到货：采购订单数量-已下推收料单数量
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public object NoArriveOrderTOBPMService(string startDate, string endDate)
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

            try
            {
                if (string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(endDate))
                {
                    return new
                    {
                        StatusCode = 400,
                        Message = "开始时间和结束时间不能为空，格式：yyyy-MM"
                    };
                }

                int cgQty = 0;
                int slQty = 0;

                // ================== 采购订单数量 ==================
                string sql = $@"/*dialect*/
            SELECT COUNT(*) AS QTY 
            FROM T_PUR_POORDER 
            WHERE FDATE >= '{startDate}'
              AND FDATE < '{endDate}'AND FDOCUMENTSTATUS = 'C'";

                var result = DBUtils.ExecuteDataSet(ctx, sql);

                if (result.Tables.Count > 0 && result.Tables[0].Rows.Count > 0)
                {
                    cgQty = result.Tables[0].Rows[0]["QTY"] == DBNull.Value
                        ? 0
                        : Convert.ToInt32(result.Tables[0].Rows[0]["QTY"]);
                }

                string sql2 = $@"/*dialect*/
                                        SELECT COUNT(DISTINCT a.fbillno) AS cyqty
                            FROM T_PUR_Receive a
                            JOIN T_PUR_ReceiveEntry b ON a.fid = b.FID
                            JOIN T_PUR_POORDER C ON B.FSRCBILLNO = C.FBILLNO
                            WHERE a.FDATE >= '{startDate}'
                              AND a.FDATE < '{endDate}'
                              AND C.FDATE >= '{startDate}'
                              AND C.FDATE < '{endDate}'
                              AND a.FDOCUMENTSTATUS <> 'C'
                              AND b.FSRCBILLNO IS NOT NULL
                              AND b.FSRCBILLNO <> ''";

                var result2 = DBUtils.ExecuteDataSet(ctx, sql2);

                if (result2.Tables.Count > 0 && result2.Tables[0].Rows.Count > 0)
                {
                    slQty = result2.Tables[0].Rows[0]["cyqty"] == DBNull.Value
                        ? 0
                        : Convert.ToInt32(result2.Tables[0].Rows[0]["cyqty"]);
                }

                // ================== 计算差值 ==================
                int diff = cgQty - slQty;

                if (diff < 0)
                {
                    diff = 0;
                }

                // ================== 统一列表结构返回 ==================
                var dataList = new List<Dictionary<string, object>>();

                var rowDict = new Dictionary<string, object>();

                rowDict["diffQty"] = diff;

                dataList.Add(rowDict);

                return new
                {
                    StatusCode = 200,
                    Data = dataList,
                    ApiName = System.Reflection.MethodBase.GetCurrentMethod().Name
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

        /// <summary>
        /// 未检验数量：采购订单数量-已下推检验审核数量
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public object ArriveNoQCTOBPMService(string startDate, string endDate)
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

            try
            {
                if (string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(endDate))
                {
                    return new
                    {
                        StatusCode = 400,
                        Message = "开始时间和结束时间不能为空，格式：yyyy-MM"
                    };
                }

                int cgQty = 0;
                int slQty = 0;

                // ================== 收料数量 ==================
                string sql = $@"/*dialect*/
            SELECT COUNT(*) AS QTY 
            FROM T_PUR_Receive 
            WHERE FDATE >= '{startDate}'
              AND FDATE < '{endDate}'AND FDOCUMENTSTATUS = 'C'";

                var result = DBUtils.ExecuteDataSet(ctx, sql);

                if (result.Tables.Count > 0 && result.Tables[0].Rows.Count > 0)
                {
                    cgQty = result.Tables[0].Rows[0]["QTY"] == DBNull.Value
                        ? 0
                        : Convert.ToInt32(result.Tables[0].Rows[0]["QTY"]);
                }

                // ================== 已下推检验审核数量 ==================
                string sql2 = $@"/*dialect*/
                                        SELECT COUNT(DISTINCT a.fbillno) AS cyqty
                            FROM T_PUR_Receive a
                            JOIN T_PUR_ReceiveEntry b ON a.fid = b.FID
                            JOIN T_PUR_POORDER C ON B.FSRCBILLNO = C.FBILLNO
                            WHERE a.FDATE >= '{startDate}'
                              AND a.FDATE < '{endDate}'
                              AND C.FDATE >= '{startDate}'
                              AND C.FDATE < '{endDate}'
                              AND a.FDOCUMENTSTATUS = 'C'
                              AND b.FSRCBILLNO IS NOT NULL
                              AND b.FSRCBILLNO <> ''";

                var result2 = DBUtils.ExecuteDataSet(ctx, sql2);

                if (result2.Tables.Count > 0 && result2.Tables[0].Rows.Count > 0)
                {
                    slQty = result2.Tables[0].Rows[0]["cyqty"] == DBNull.Value
                        ? 0
                        : Convert.ToInt32(result2.Tables[0].Rows[0]["cyqty"]);
                }

                // ================== 计算差值 ==================
                int diff = cgQty - slQty;

                if (diff < 0)
                {
                    diff = 0;
                }

                // ================== 统一列表结构返回 ==================
                var dataList = new List<Dictionary<string, object>>();

                var rowDict = new Dictionary<string, object>();

                rowDict["diffQty"] = diff;

                dataList.Add(rowDict);

                return new
                {
                    StatusCode = 200,
                    Data = dataList,
                    ApiName = System.Reflection.MethodBase.GetCurrentMethod().Name
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

        /// <summary>
        /// 未入库数量：采购订单数量-已下推入库审核数量
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public object QCNoInStockTOBPMService(string startDate, string endDate)
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

            try
            {
                if (string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(endDate))
                {
                    return new
                    {
                        StatusCode = 400,
                        Message = "开始时间和结束时间不能为空，格式：yyyy-MM"
                    };
                }

                int cgQty = 0;
                int slQty = 0;

                // ================== 检验数量 ==================
                string sql = $@"/*dialect*/
            SELECT COUNT(*) AS QTY 
            FROM T_PUR_Receive 
            WHERE FDATE >= '{startDate}'
              AND FDATE < '{endDate}'AND FDOCUMENTSTATUS = 'C'";

                var result = DBUtils.ExecuteDataSet(ctx, sql);

                if (result.Tables.Count > 0 && result.Tables[0].Rows.Count > 0)
                {
                    cgQty = result.Tables[0].Rows[0]["QTY"] == DBNull.Value
                        ? 0
                        : Convert.ToInt32(result.Tables[0].Rows[0]["QTY"]);
                }

                // ================== 已下推入库审核数量 ==================
                string sql2 = $@"/*dialect*/
                                             SELECT COUNT(DISTINCT a.fbillno) as cyqty
                                            FROM t_STK_InStock a
                                            JOIN T_STK_INSTOCKENTRY b ON a.fid = b.FID
                                            JOIN T_PUR_Receive C ON B.FSRCBILLNO = C.FBILLNO
                                            WHERE a.FDATE >= '{startDate}'
                                              AND a.FDATE < '{endDate}'
                                              AND C.FDATE >= '{startDate}'
                                              AND C.FDATE < '{endDate}'
                                              AND a.FDOCUMENTSTATUS = 'C'
                                              AND b.FSRCBILLNO IS NOT NULL
                                              AND b.FSRCBILLNO <> ''";

                var result2 = DBUtils.ExecuteDataSet(ctx, sql2);

                if (result2.Tables.Count > 0 && result2.Tables[0].Rows.Count > 0)
                {
                    slQty = result2.Tables[0].Rows[0]["cyqty"] == DBNull.Value
                        ? 0
                        : Convert.ToInt32(result2.Tables[0].Rows[0]["cyqty"]);
                }

                // ================== 计算差值 ==================
                int diff = cgQty - slQty;

                if (diff < 0)
                {
                    diff = 0;
                }

                // ================== 统一列表结构返回 ==================
                var dataList = new List<Dictionary<string, object>>();

                var rowDict = new Dictionary<string, object>();
                
                rowDict["diffQty"] = diff;

                dataList.Add(rowDict);


                return new
                {
                    StatusCode = 200,
                    Data = dataList,
                    ApiName = System.Reflection.MethodBase.GetCurrentMethod().Name
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
