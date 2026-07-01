using Kingdee.BOS.App.Data;
using Kingdee.BOS.BusinessEntity.BillTrack;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.ComponentModel;

namespace Zitn_ReceiveBill_Plugin
{
    [Description("【服务插件】：收料单保存提交时，设置超收信息审批流")]
    [Kingdee.BOS.Util.HotUpdate]
    public class ReceiveClass1 : AbstractOperationServicePlugIn
    {
        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);

            string SLbillno = string.Empty;
            foreach (DynamicObject obj in e.DataEntitys)
            {
                SLbillno = obj["BillNo"].ToString();

                var SLsql = string.Format("/*dialect*/SELECT FACTRECEIVEQTY,FSRCBILLNO,M.FNUMBER,B.FENTRYID FROM T_PUR_Receive A JOIN T_PUR_ReceiveEntry B ON A.FID = B.FID JOIN T_BD_MATERIAL M ON B.FMATERIALID = M.FMATERIALID WHERE FBILLNO = '{0}'", SLbillno);
                DynamicObjectCollection col = DBUtils.ExecuteDynamicObject(this.Context, SLsql);
                if (col != null && col.Count > 0)
                {
                    for (int i = 0; i < col.Count; i++)
                    {
                        int mustqty = Convert.ToInt32(col[i]["FACTRECEIVEQTY"]);
                        string purno = col[i]["FSRCBILLNO"].ToString();
                        string wlnum = col[i]["FNUMBER"].ToString();
                        int entryid = Convert.ToInt32(col[i]["FENTRYID"]);

                        string POsql = $@"SELECT FRECEIVEQTY,FQTY FROM 
                                    T_PUR_POORDER C JOIN T_PUR_POORDERENTRY D ON C.FID = D.FID 
                                                                    JOIN T_PUR_POORDERENTRY_r E ON D.FENTRYID=E.FENTRYID
                                                                    JOIN T_BD_MATERIAL M ON D.FMATERIALID = M.FMATERIALID
                                                                    WHERE C.FBILLNO = '{purno}' AND M.FNUMBER = '{wlnum}'";
                        DynamicObjectCollection POcol
                            = DBUtils.ExecuteDynamicObject(this.Context, POsql);
                        if (POcol != null && POcol.Count > 0)
                        {
                            bool hasFlag = false;
                            for (int j = 0; j < POcol.Count; j++)
                            {
                                int receiveqty = Convert.ToInt32(POcol[j]["FRECEIVEQTY"]);
                                int qty = Convert.ToInt32(POcol[j]["FQTY"]);

                                decimal result = (mustqty + receiveqty - qty) / (decimal)qty;
                                result = Math.Round(result, 2);

                                if (result > 0)
                                {
                                    var upSql = $@"UPDATE T_PUR_RECEIVEENTRY SET FCFBL = {result.ToString("0.00")} WHERE FENTRYID = {entryid} ";
                                    DBUtils.Execute(this.Context, upSql);
                                }
                                if (result > 0.30m)
                                {
                                    /*var upSql = $@"UPDATE T_PUR_Receive SET FFLAG = '是' WHERE FBILLNO = {billno} ";
                                    DBUtils.Execute(this.Context, upSql);*/

                                    hasFlag = true;
                                }
                            }
                            if (hasFlag)
                            {
                                string upSql = $@"UPDATE T_PUR_Receive SET FFLAG = '是' WHERE FBILLNO = '{SLbillno}'";
                                DBUtils.Execute(this.Context, upSql);
                            }
                        }
                    }
                }
            }
        }
    }
}
