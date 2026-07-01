using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.IO;

namespace Zitn_DBSQFORZJDB_Schldue_Plugin
{
    [Description("【定时任务】：调拨申请单、直接调拨单批量脚本")]
    [Kingdee.BOS.Util.HotUpdate]
    public class Class1 : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            #region WebApi 登录

            /*K3CloudApiClient client = new K3CloudApiClient("http://10.0.32.18/k3cloud/");
            var loginResult = client.ValidateLogin(
                "688399bec6449e",
                "admin",
                "Flzx3qc!",
                2052
            );*/

            K3CloudApiClient client = new K3CloudApiClient("http://127.0.0.1/k3cloud/");
            var loginResult = client.ValidateLogin(
                "6940c27ae377d5",
                "刘总",
                "yangwei11",
                2052
            );

            if (JObject.Parse(loginResult)["LoginResultType"].Value<int>() != 1)
                return;

            #endregion

            #region 读取文件
            string filePath = @"C:\Users\Administrator\Desktop\dbsqBillno.txt";

            if (!File.Exists(filePath))
                return;

            string[] billNos = File.ReadAllLines(filePath);

            foreach (string billNo in billNos)
            {
                if (string.IsNullOrWhiteSpace(billNo))
                    continue;

                ProcessBill(ctx, billNo.Trim());
            }
            #endregion

            /*ProcessBill(ctx, "DBSQ005392");*/
        }

        /// <summary>
        /// 处理单个调拨申请单
        /// </summary>
        private void ProcessBill(Context ctx, string dbsqBillno)
        {
            var dbsqQuery = string.Format(@"/*dialect*/SELECT B.FMATERIALID,FNUMBER,FSTOCKID,FQTY,FSTOCKLOCINID
                                            FROM T_STK_STKTRANSFERAPP A 
                                            JOIN T_STK_STKTRANSFERAPPENTRY B ON A.FID = B.FID
                                            JOIN T_BD_MATERIAL C ON B.FMATERIALID = C.FMATERIALID
                                            WHERE FBILLNO = '{0}'", dbsqBillno);

            DynamicObjectCollection dbsqList = DBUtils.ExecuteDynamicObject(ctx, dbsqQuery);

            if (dbsqList == null || dbsqList.Count == 0)
                return;

            for (int i = 0; i < dbsqList.Count; i++)
            {
                string wlid = dbsqList[i]["FMATERIALID"].ToString();
                string cckid = dbsqList[i]["FSTOCKID"].ToString();
                int qty = Convert.ToInt32(dbsqList[i]["FQTY"]);

                var zjdbQuery = string.Format(@"/*dialect*/SELECT B.FMATERIALID,FNUMBER,FDESTSTOCKID,
                                                B.FQTY,FSRCSTOCKLOCID,A.FDATE,FBILLNO
                                                FROM T_STK_STKTRANSFERIN A 
                                                JOIN T_STK_STKTRANSFERINENTRY B ON A.FID = B.FID
                                                JOIN T_BD_MATERIAL C ON B.FMATERIALID = C.FMATERIALID
                                                WHERE B.FMATERIALID = '{0}' 
                                                AND FDESTSTOCKID = '{1}'",
                                                wlid, cckid);

                DynamicObjectCollection zjdbList = DBUtils.ExecuteDynamicObject(ctx, zjdbQuery);

                if (zjdbList == null || zjdbList.Count == 0)
                    continue;

                for (int j = 0; j < zjdbList.Count; j++)
                {
                    int zjdbqty = Convert.ToInt32(zjdbList[j]["FQTY"]);
                    string zjdbdccw = zjdbList[j]["FSRCSTOCKLOCID"].ToString();

                    if (qty == zjdbqty)
                    {
                        UpdateStockLoc(ctx, dbsqBillno, wlid, zjdbdccw);
                        break;
                    }
                    else if (qty < zjdbqty)
                    {
                        NOeQUAL(wlid, cckid, qty, ctx, dbsqBillno);
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 数量不等时处理
        /// </summary>
        private void NOeQUAL(string wlid, string cckid, int qty, Context ctx, string dbsqBillno)
        {
            var zjdbQuery1 = string.Format(@"/*dialect*/SELECT TOP 1 B.FMATERIALID,FNUMBER,
                                            FDESTSTOCKID,FQTY,FSRCSTOCKLOCID,A.FDATE,FBILLNO
                                            FROM T_STK_STKTRANSFERIN A 
                                            JOIN T_STK_STKTRANSFERINENTRY B ON A.FID = B.FID
                                            JOIN T_BD_MATERIAL C ON B.FMATERIALID = C.FMATERIALID
                                            WHERE B.FMATERIALID = '{0}' 
                                            AND FDESTSTOCKID = '{1}' 
                                            AND FQTY > {2}
                                            ORDER BY A.FDATE DESC",
                                            wlid, cckid, qty);

            DynamicObjectCollection zjdbList1 = DBUtils.ExecuteDynamicObject(ctx, zjdbQuery1);

            if (zjdbList1 == null || zjdbList1.Count == 0)
                return;

            string zjdbwlid = zjdbList1[0]["FMATERIALID"].ToString();
            string zjdbdccw = zjdbList1[0]["FSRCSTOCKLOCID"].ToString();

            UpdateStockLoc(ctx, dbsqBillno, zjdbwlid, zjdbdccw);
        }

        /// <summary>
        /// 更新库存位置
        /// </summary>
        private void UpdateStockLoc(Context ctx, string billNo, string materialId, string stockLocId)
        {
            var updSql = string.Format(@"/*dialect*/UPDATE B 
                                        SET B.FSTOCKLOCINID = '{0}'
                                        FROM T_STK_STKTRANSFERAPP A 
                                        JOIN T_STK_STKTRANSFERAPPENTRY B ON A.FID = B.FID 
                                        WHERE FBILLNO = '{1}' 
                                        AND B.FMATERIALID = '{2}'",
                                        stockLocId, billNo, materialId);

            DBUtils.Execute(ctx, updSql);
        }
    }
}