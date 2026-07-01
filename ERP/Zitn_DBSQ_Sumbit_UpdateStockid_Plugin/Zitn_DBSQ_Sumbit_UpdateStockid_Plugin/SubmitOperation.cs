using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.ComponentModel;
namespace Zitn_DBSQ_Sumbit_UpdateStockid_Plugin
{
    [Description("【服务插件】：调拨申请单提交时、wlnum+stockid获取直接调拨单更新...")]
    [Kingdee.BOS.Util.HotUpdate]
    public class SubmitOperation : AbstractOperationServicePlugIn
    {
        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);

            string dbsqBillno = string.Empty;

            foreach (DynamicObject billObj in e.DataEntitys)
            {
                dbsqBillno = Convert.ToString(billObj["BillNo"]);

                var reasonQuery = string.Format(@"/*dialect*/SELECT DISTINCT
                                                              T1.FDATAVALUE 
                                                            FROM
                                                              T_STK_STKTRANSFERAPP A
                                                              JOIN T_STK_STKTRANSFERAPPENTRY B ON A.FID = B.FID
                                                              JOIN T_BD_MATERIAL C ON B.FMATERIALID = C.FMATERIALID
                                                              JOIN T_BAS_ASSISTANTDATAENTRY_L T1 ON A.F_PAEZ_DBYY = T1.FENTRYID 
                                                            WHERE
                                                              FBILLNO = '{0}'",dbsqBillno);
                DynamicObjectCollection reasonList = DBUtils.ExecuteDynamicObject(this.Context, reasonQuery);
                if (reasonList != null && reasonList.Count > 0)
                {
                    for(int i = 0; i < reasonList.Count; i++)
                    {
                        if (Convert.ToString(reasonList[i]["FDATAVALUE"]) == "生产借用原材料及半成品")
                        {
                            ProcessBill(this.Context, dbsqBillno);
                        }
                    }
                }
            }
        }

        private void ProcessBill(Context context, string dbsqBillno)
        {
            //调拨申请单调入仓位等信息
            var dbsqQuery = string.Format(@"/*dialect*/SELECT B.FMATERIALID,FNUMBER,FSTOCKID,FQTY,FSTOCKLOCINID,B.FENTRYID
                                            FROM T_STK_STKTRANSFERAPP A 
                                            JOIN T_STK_STKTRANSFERAPPENTRY B ON A.FID = B.FID
                                            JOIN T_BD_MATERIAL C ON B.FMATERIALID = C.FMATERIALID
                                            WHERE FBILLNO = '{0}'", dbsqBillno);

            DynamicObjectCollection dbsqList = DBUtils.ExecuteDynamicObject(context, dbsqQuery);
            if (dbsqList == null || dbsqList.Count == 0)
                return;

            for (int i = 0; i < dbsqList.Count; i++)
            {
                string wlid = dbsqList[i]["FMATERIALID"].ToString();
                string cckid = dbsqList[i]["FSTOCKID"].ToString();
                int qty = Convert.ToInt32(dbsqList[i]["FQTY"]);
                long entryid = Convert.ToInt64(dbsqList[i]["FENTRYID"]);

                //直接调拨单调出仓位
                var zjdbQuery = string.Format(@"/*dialect*/SELECT B.FMATERIALID,C.FNUMBER,FDESTSTOCKID,
                                                B.FQTY,FSRCSTOCKLOCID,A.FDATE,FBILLNO,S.FNUMBER AS DRCKBM
                                                FROM T_STK_STKTRANSFERIN A 
                                                JOIN T_STK_STKTRANSFERINENTRY B ON A.FID = B.FID
                                                JOIN T_BD_MATERIAL C ON B.FMATERIALID = C.FMATERIALID
                                                JOIN T_BD_STOCK S ON B.FDESTSTOCKID = S.FSTOCKID AND FISOPENLOCATION =1
                                                WHERE B.FMATERIALID = '{0}' 
                                                AND FDESTSTOCKID = '{1}' AND A.FDOCUMENTSTATUS = 'C'",
                                                wlid, cckid);

                DynamicObjectCollection zjdbList = DBUtils.ExecuteDynamicObject(context, zjdbQuery);

                if (zjdbList == null || zjdbList.Count == 0)
                    continue;

                for (int j = 0; j < zjdbList.Count; j++)
                {
                    int zjdbqty = Convert.ToInt32(zjdbList[j]["FQTY"]);
                    string zjdbdccw = zjdbList[j]["FSRCSTOCKLOCID"].ToString();
                    string zjdbBillno = zjdbList[j]["FBILLNO"].ToString();


                    if (qty == zjdbqty)
                    {
                        UpdateStockLoc(context, dbsqBillno, wlid, zjdbdccw,zjdbBillno, cckid,entryid);
                        break;
                    }
                    else if (qty < zjdbqty)
                    {
                        NOeQUAL(wlid, cckid, qty, context, dbsqBillno,zjdbBillno,entryid);
                        break;
                    }
                }

            }
        }

        private void NOeQUAL(string wlid, string cckid, int qty, Context context, string dbsqBillno, string zjdbBillno, long entryid)
        {
            var zjdbQuery1 = string.Format(@"/*dialect*/SELECT TOP 1 B.FMATERIALID,FNUMBER,
                                            FDESTSTOCKID,FQTY,FSRCSTOCKLOCID,A.FDATE,FBILLNO
                                            FROM T_STK_STKTRANSFERIN A 
                                            JOIN T_STK_STKTRANSFERINENTRY B ON A.FID = B.FID
                                            JOIN T_BD_MATERIAL C ON B.FMATERIALID = C.FMATERIALID
                                            WHERE B.FMATERIALID = '{0}' 
                                            AND FDESTSTOCKID = '{1}' 
                                            AND FQTY > {2} AND A.FDOCUMENTSTATUS = 'C'
                                            ORDER BY A.FDATE DESC",
                                            wlid, cckid, qty);

            DynamicObjectCollection zjdbList1 = DBUtils.ExecuteDynamicObject(context, zjdbQuery1);

            if (zjdbList1 == null || zjdbList1.Count == 0)
                return;

            string zjdbwlid = zjdbList1[0]["FMATERIALID"].ToString();
            string zjdbdccw = zjdbList1[0]["FSRCSTOCKLOCID"].ToString();

            UpdateStockLoc(context, dbsqBillno, zjdbwlid, zjdbdccw,zjdbBillno, cckid,entryid);
        }

        private void UpdateStockLoc(Context context, string billNo, string materialId, string stockLocId, string zjdbBillno, string cckid, long entryid)
        {
            var updSql = string.Format(@"/*dialect*/UPDATE B 
                                        SET B.FSTOCKLOCINID = '{0}'
                                        FROM T_STK_STKTRANSFERAPP A 
                                        JOIN T_STK_STKTRANSFERAPPENTRY B ON A.FID = B.FID 
                                        WHERE FBILLNO = '{1}' 
                                        AND B.FMATERIALID = '{2}'",
                                        stockLocId, billNo, materialId);

            DBUtils.Execute(context, updSql);

            JudgeFlag(context, zjdbBillno, materialId, cckid, billNo, entryid);
        }

        /// <summary>
        /// 判断单据是否是超额发料【超额发料自动生成有标识】
        /// </summary>
        /// <param name="context"></param>
        /// <param name="billNo"></param>
        private void JudgeFlag(Context context, string zjdbBillno, string materialId, string stockLocId, string billNo,long entryid)
        {
            var query = string.Format(@"/*dialect*/SELECT FISFLAG,FMOBILLNO,fpickmtrlbillno,fcpnum FROM T_STK_STKTRANSFERIN A JOIN T_STK_STKTRANSFERINENTRY B ON A.FID = B.FID WHERE FBILLNO = '{0}' AND A.FDOCUMENTSTATUS = 'C'", zjdbBillno);
            DynamicObjectCollection list = DBUtils.ExecuteDynamicObject(context, query);
            if (list != null && list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    string flag = list[i]["FISFLAG"].ToString();
                    string fpickmtrlbillno = list[i]["fpickmtrlbillno"].ToString();
                    string fMOBILLNO = list[i]["FMOBILLNO"].ToString();
                    string cpnum = list[i]["fcpnum"].ToString();

                    string wlnum = string.Empty;
                    var wlQuery = string.Format("/*dialect*/SELECT DISTINCT FNUMBER FROM T_BD_MATERIAL WHERE FMATERIALID = '{0}'",materialId);
                    DynamicObjectCollection wlRows = DBUtils.ExecuteDynamicObject(context, wlQuery);
                    if (wlRows != null && wlRows.Count > 0)
                    {
                        wlnum = wlRows[0]["FNUMBER"].ToString();
                    }

                    if (flag == "1")
                    {
                        var updSql = string.Format(@"/*dialect*/UPDATE B SET B.fzjdbbillno = '{0}' ,fmobillno = '{1}' ,fpickmtrlbillno = '{2}',FCPNUM = '{3}'
                                                                    FROM
                                                                      T_STK_STKTRANSFERAPP A
                                                                      JOIN T_STK_STKTRANSFERAPPENTRY B ON A.FID = B.FID
                                                                      JOIN T_BD_MATERIAL C ON B.FMATERIALID = C.FMATERIALID 
                                                                    WHERE FBILLNO = '{4}' AND FSTOCKID ='{5}' AND FNUMBER ='{6}'  AND B.FENTRYID = {7}", zjdbBillno,fMOBILLNO,fpickmtrlbillno,cpnum,billNo, stockLocId,wlnum,entryid);
                        DBUtils.Execute(context, updSql);
                    }
                }
            }
        }
    }
}
