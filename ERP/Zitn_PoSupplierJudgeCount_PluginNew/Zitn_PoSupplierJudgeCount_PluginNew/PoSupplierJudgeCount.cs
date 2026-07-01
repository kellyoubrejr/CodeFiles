using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.ComponentModel;

namespace Zitn_PoSupplierJudgeCount_PluginNew
{
    [Description("【服务插件】：采购单保存提交时，检查供应商")]
    [Kingdee.BOS.Util.HotUpdate]
    public class PoSupplierJudgeCount : AbstractOperationServicePlugIn
    {
        public override void BeforeExecuteOperationTransaction(BeforeExecuteOperationTransaction e)
        {
            base.BeforeExecuteOperationTransaction(e);

            CheckSupplier(e);
        }
        /// <summary>
        /// 采购订单保存提交是，检查供应商是否满足条件，当年小于等于五次，采购总额含税的小于等于五万
        /// </summary>
        /// <param name="e"></param>
        private void CheckSupplier(BeforeExecuteOperationTransaction e)
        {
            string po = string.Empty;
            foreach (var row in e.SelectedRows)
            {
                po = row.DataEntity["BillNo"]?.ToString();

                var suSql = string.Format("/*dialect*/SELECT DISTINCT FSUPPLIERID FROM T_PUR_POORDER WHERE FBILLNO = '{0}'",po);
                var suData = DBUtils.ExecuteDynamicObject(this.Context, suSql);
                if (suData !=null && suData.Count>0)
                {
                    string supplierId = suData[0]["FSUPPLIERID"].ToString();

                    var gysSql = string.Format($"/*dialect*/SELECT FPRIMARYGROUP FROM t_BD_Supplier WHERE FSUPPLIERID = {supplierId}");
                    var gysData = DBUtils.ExecuteDynamicObject(this.Context, gysSql);
                    if (gysData != null && gysData.Count > 0)
                    {
                        for (int i = 0; i < gysData.Count; i++)
                        {
                            int groupId = Convert.ToInt32(gysData[i]["FPRIMARYGROUP"]);
                            // dev:5237578     test :4739570
                            if (groupId == 5237578)
                            {
                                var poAllSql = string.Format($"/*dialect*/SELECT SUM(FALLAMOUNT_LC) AS FALLAMOUNT \r\nFROM\r\nT_PUR_POORDER A\r\nJOIN T_PUR_POORDERENTRY B ON A.FID = B.FID\r\nJOIN T_PUR_POORDERENTRY_F B1 ON B1.FENTRYID = B.FENTRYID \r\nWHERE FSUPPLIERID = {supplierId}\r\nAND FDATE >= DATEADD(YEAR, -1, GETDATE())\r\nAND FDATE <= GETDATE()");
                                var poAllData = DBUtils.ExecuteDynamicObject(this.Context, poAllSql);
                                if (poAllData != null && poAllData.Count > 0)
                                {
                                    decimal poAllAmount = Convert.ToDecimal(poAllData[0]["FALLAMOUNT"]);
                                    if (poAllAmount > 50000)
                                    {
                                        throw new KDBusinessException("检验不通过", "您已触发转为正式供应商的条件");
                                    }
                                }
                                var poAllSql1 = string.Format($"/*dialect*/SELECT COUNT(DISTINCT A.FBILLNO) AS ORDER_COUNT\r\nFROM T_PUR_POORDER A\r\nJOIN T_PUR_POORDERENTRY B ON A.FID = B.FID\r\nJOIN T_PUR_POORDERENTRY_F B1 ON B1.FENTRYID = B.FENTRYID\r\nWHERE A.FSUPPLIERID = {supplierId}\r\nAND A.FDATE >= DATEADD(YEAR, -1, GETDATE())\r\nAND A.FDATE <= GETDATE()");
                                var poAllData1 = DBUtils.ExecuteDynamicObject(this.Context, poAllSql1);
                                if (poAllData1 != null && poAllData1.Count > 0)
                                {
                                    int poAllCount = Convert.ToInt32(poAllData1[0]["ORDER_COUNT"]);
                                    if (poAllCount > 12)
                                    {
                                        throw new KDBusinessException("检验不通过", "您已触发转为正式供应商的条件");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
