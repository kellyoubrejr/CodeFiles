using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.ComponentModel;

namespace Zitn_Pur_SubmitJudgeFcolorflag_Plugin
{
    [Description("【提交服务operation】:采购订单，查询是否在免审单据中，如果在，写标志Fcolorflag - 存在免审记录 否则不写"), HotUpdate]
    public class SubmitJudgeFcolorflag : AbstractOperationServicePlugIn
    {
        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);

            foreach (var billObj in e.DataEntitys)
            {
                string billNo = Convert.ToString(billObj["BillNo"]);

                string entitySql = $@"
                                    /*dialect*/
                                    SELECT FSUPPLIERID,FMATERIALID,FQTY,FTAXPRICE,B.FENTRYID 
                                    FROM T_PUR_POORDER A 
                                    JOIN T_PUR_POORDERENTRY B ON A.FID = B.FID 
                                    JOIN T_PUR_POORDERENTRY_F B1 ON B.FENTRYID = B1.FENTRYID 
                                    WHERE FBILLNO = '{billNo}'";

                var result = DBUtils.ExecuteDynamicObject(this.Context, entitySql);

                if (result == null || result.Count == 0) continue;

                foreach (var entity in result)
                {
                    string supplierId = Convert.ToString(entity["FSUPPLIERID"]);
                    string materialId = Convert.ToString(entity["FMATERIALID"]);
                    decimal qty = Convert.ToDecimal(entity["FQTY"]);
                    decimal taxPrice = Convert.ToDecimal(entity["FTAXPRICE"]);
                    long entryId = Convert.ToInt64(entity["FENTRYID"]);

                    // ✅ 1. 是否存在免审配置（只判断供应商+物料）
                    string existSql = $@"
                                        /*dialect*/
                                        SELECT 1 
                                        FROM ZMER_t_Cust_Entry100101 A 
                                        JOIN T_BD_SUPPLIER_L B ON A.FSUPPLIER = B.FNAME 
                                        WHERE B.FSUPPLIERID = '{supplierId}' 
                                        AND FMATERIALID = '{materialId}'";

                    var existColl = DBUtils.ExecuteDynamicObject(this.Context, existSql);

                    if (existColl == null || existColl.Count == 0)
                    {
                        // ❌ 连配置都没有 → 不处理
                        continue;
                    }

                    // ✅ 2. 判断是否满足免审条件
                    string matchSql = $@"
                                    /*dialect*/
                                    SELECT 1 
                                    FROM ZMER_t_Cust_Entry100101 A 
                                    JOIN T_BD_SUPPLIER_L B ON A.FSUPPLIER = B.FNAME 
                                    WHERE B.FSUPPLIERID = '{supplierId}' 
                                    AND FMATERIALID = '{materialId}'
                                    AND {qty} <= F_ZMER_TEXT_83G 
                                    AND {qty} > F_ZMER_TEXT_QTR 
                                    AND {taxPrice} >= FTAXPRICE";

                    var matchColl = DBUtils.ExecuteDynamicObject(this.Context, matchSql);

                    string updateSql = "";

                    if (matchColl != null && matchColl.Count > 0)
                    {
                        // 🟢 满足免审
                        updateSql = $"/*dialect*/UPDATE T_PUR_POORDERENTRY SET FCOLORFLAG = N'存在免审记录' WHERE FENTRYID = {entryId}";
                    }
                    else
                    {
                        // 🔵 有配置但不满足
                        updateSql = $"/*dialect*/UPDATE T_PUR_POORDERENTRY SET FCOLORFLAG = N'存在免审但不满足条件' WHERE FENTRYID = {entryId}";
                    }

                    DBUtils.Execute(this.Context, updateSql);
                }
            }
        }
    }
}
