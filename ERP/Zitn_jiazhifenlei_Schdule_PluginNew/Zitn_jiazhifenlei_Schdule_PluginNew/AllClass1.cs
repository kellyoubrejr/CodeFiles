using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.BusinessEntity.BusinessFlow;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;

namespace Zitn_jiazhifenlei_Schdule_PluginNew
{
    [Description("【跑批】：生产领料、退料、补料，出库单更新审核中的价值分类")]
    [Kingdee.BOS.Util.HotUpdate]
    public class AllClass1 : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            #region WebApi 登录

            K3CloudApiClient client = new K3CloudApiClient("http://10.0.32.18/k3cloud/");
            var loginResult = client.ValidateLogin(
                "688399bec6449e",
                "admin",
                "Flzx3qc!",
                2052
            );

            if (JObject.Parse(loginResult)["LoginResultType"].Value<int>() != 1)
                return;

            #endregion

            OutStock(ctx);
            FeedMtrl(ctx);
            ReturnMtrl(ctx);
            PickMtrl(ctx);
        }

        private void PickMtrl(Context ctx)
        {
            var reasonQuery = string.Format($@"/*dialect*/SELECT A.FBILLNO,Fpricesort,FNUMBER,B.FENTRYID FROM T_PRD_PICKMTRL A JOIN
                                                            T_PRD_PICKMTRLDATA B ON A.FID = B.FID JOIN 
                                                            T_BD_MATERIAL M1 ON B.FMATERIALID = M1.FMATERIALID
                                                            WHERE
                                                               A.FDOCUMENTSTATUS ='B'");
            DynamicObjectCollection reasonList = DBUtils.ExecuteDynamicObject(ctx, reasonQuery);
            if (reasonList != null && reasonList.Count > 0)
            {
                for (int i = 0; i < reasonList.Count; i++)
                {
                    string pickBillno = Convert.ToString(reasonList[i]["FBILLNO"]);
                    string wlnum = Convert.ToString(reasonList[i]["FNUMBER"]);
                    string entryid = Convert.ToString(reasonList[i]["FENTRYID"]);

                    string query1 = string.Format($@"SELECT TOP
                                                              1 A.FBillno,
                                                              A.FSUPPLIERID,
                                                              C.FTAXPRICE,
                                                              B.Fqty from T_PUR_POORDER A
                                                              JOIN T_PUR_POORDEREntry B ON B.Fid= A.FID
                                                              JOIN T_PUR_POORDERENTRY_F C ON C.FENTRYID= B.FENTRYID
                                                              JOIN T_BD_MATERIAL M1 ON B.FMATERIALID = M1.FMATERIALID
                                                            WHERE
                                                              A.FDOCUMENTSTATUS= 'C' 
                                                              AND M1.FNUMBER= '{wlnum}' 
                                                            ORDER BY
                                                              A.Fdate DESC");
                    DynamicObjectCollection coll1 = DBUtils.ExecuteDynamicObject(ctx, query1);
                    if (coll1 != null && coll1.Count > 0)
                    {
                        for (int j = 0; j < coll1.Count; j++)
                        {
                            double ftaxprice = Convert.ToDouble(coll1[j]["FTAXPRICE"]);

                            string flag = GetPriceFlag(ftaxprice);

                            var upSql = string.Format(@"/*dialect*/UPDATE B SET B.Fpricesort = '{0}' 
                                                            FROM T_PRD_PICKMTRL A JOIN
                                                            T_PRD_PICKMTRLDATA B ON A.FID = B.FID JOIN 
                                                            T_BD_MATERIAL M1 ON B.FMATERIALID = M1.FMATERIALID
                                                            WHERE FBILLNO='{1}' AND M1.FNUMBER = '{2}' AND B.FENTRYID = '{3}'", flag, pickBillno, wlnum, entryid);
                            DBUtils.Execute(ctx, upSql);
                        }
                    }
                }
            }
        }

        private void ReturnMtrl(Context ctx)
        {
            var reasonQuery = string.Format($@"/*dialect*/SELECT A.FBILLNO,FPRICE,Fpricesort,FNUMBER,B.FENTRYID FROM T_PRD_RETURNMTRL A JOIN
                                                            T_PRD_RETURNMTRLENTRY B ON A.FID = B.FID JOIN 
                                                            T_BD_MATERIAL M1 ON B.FMATERIALID = M1.FMATERIALID
                                                            WHERE
                                                               A.FDOCUMENTSTATUS ='B'");
            DynamicObjectCollection reasonList = DBUtils.ExecuteDynamicObject(ctx, reasonQuery);
            if (reasonList != null && reasonList.Count > 0)
            {
                for (int i = 0; i < reasonList.Count; i++)
                {
                    string returnBillno = Convert.ToString(reasonList[i]["FBILLNO"]);
                    string wlnum = Convert.ToString(reasonList[i]["FNUMBER"]);
                    string entryid = Convert.ToString(reasonList[i]["FENTRYID"]);

                    string query1 = string.Format($@"SELECT TOP
                                                              1 A.FBillno,
                                                              A.FSUPPLIERID,
                                                              C.FTAXPRICE,
                                                              B.Fqty from T_PUR_POORDER A
                                                              JOIN T_PUR_POORDEREntry B ON B.Fid= A.FID
                                                              JOIN T_PUR_POORDERENTRY_F C ON C.FENTRYID= B.FENTRYID
                                                              JOIN T_BD_MATERIAL M1 ON B.FMATERIALID = M1.FMATERIALID
                                                            WHERE
                                                              A.FDOCUMENTSTATUS= 'C' 
                                                              AND M1.FNUMBER= '{wlnum}' 
                                                            ORDER BY
                                                              A.Fdate DESC");
                    DynamicObjectCollection coll1 = DBUtils.ExecuteDynamicObject(ctx, query1);
                    if (coll1 != null && coll1.Count > 0)
                    {
                        for (int j = 0; j < coll1.Count; j++)
                        {
                            double ftaxprice = Convert.ToDouble(coll1[j]["FTAXPRICE"]);

                            string flag = GetPriceFlag(ftaxprice);

                            var upSql = string.Format(@"/*dialect*/UPDATE B SET B.Fpricesort = '{0}' 
                                                            FROM T_PRD_RETURNMTRL A JOIN
                                                                T_PRD_RETURNMTRLENTRY B ON A.FID = B.FID JOIN 
                                                                T_BD_MATERIAL M1 ON B.FMATERIALID = M1.FMATERIALID
                                                            WHERE FBILLNO='{1}' AND M1.FNUMBER = '{2}' AND B.FENTRYID = '{3}'", flag, returnBillno, wlnum, entryid);
                            DBUtils.Execute(ctx, upSql);
                        }
                    }
                }
            }
        }

        private void FeedMtrl(Context ctx)
        {
            var reasonQuery = string.Format($@"/*dialect*/SELECT A.FBILLNO,fprice,Fpricesort,FNUMBER,B.FENTRYID FROM T_PRD_FEEDMTRL A JOIN
T_PRD_FEEDMTRLDATA B ON A.FID = B.FID JOIN 
T_BD_MATERIAL M1 ON B.FMATERIALID = M1.FMATERIALID
                                                            WHERE
                                                              A.FDOCUMENTSTATUS ='B'");
            DynamicObjectCollection reasonList = DBUtils.ExecuteDynamicObject(ctx, reasonQuery);
            if (reasonList != null && reasonList.Count > 0)
            {
                for (int i = 0; i < reasonList.Count; i++)
                {
                    string feedBillno = Convert.ToString(reasonList[i]["FBILLNO"]);
                    string wlnum = Convert.ToString(reasonList[i]["FNUMBER"]);
                    string entryid = Convert.ToString(reasonList[i]["FENTRYID"]);

                    string query1 = string.Format($@"SELECT TOP
                                                              1 A.FBillno,
                                                              A.FSUPPLIERID,
                                                              C.FTAXPRICE,
                                                              B.Fqty from T_PUR_POORDER A
                                                              JOIN T_PUR_POORDEREntry B ON B.Fid= A.FID
                                                              JOIN T_PUR_POORDERENTRY_F C ON C.FENTRYID= B.FENTRYID
                                                              JOIN T_BD_MATERIAL M1 ON B.FMATERIALID = M1.FMATERIALID
                                                            WHERE
                                                              A.FDOCUMENTSTATUS= 'C' 
                                                              AND M1.FNUMBER= '{wlnum}' 
                                                            ORDER BY
                                                              A.Fdate DESC");
                    DynamicObjectCollection coll1 = DBUtils.ExecuteDynamicObject(ctx, query1);
                    if (coll1 != null && coll1.Count > 0)
                    {
                        for (int j = 0; j < coll1.Count; j++)
                        {
                            double ftaxprice = Convert.ToDouble(coll1[j]["FTAXPRICE"]);

                            string flag = GetPriceFlag(ftaxprice);

                            var upSql = string.Format(@"/*dialect*/UPDATE B SET B.Fpricesort = '{0}' 
                                                             FROM T_PRD_FEEDMTRL A JOIN
                                                            T_PRD_FEEDMTRLDATA B ON A.FID = B.FID JOIN 
                                                            T_BD_MATERIAL M1 ON B.FMATERIALID = M1.FMATERIALID
                                                            WHERE FBILLNO='{1}' AND M1.FNUMBER = '{2}' AND B.FENTRYID = '{3}'", flag, feedBillno, wlnum, entryid);
                            DBUtils.Execute(ctx, upSql);
                        }
                    }
                }
            }
        }

        private void OutStock(Context ctx)
        {
            var reasonQuery = string.Format($@"/*dialect*/SELECT A.FBILLNO,Fpricesort,FNUMBER,B.FENTRYID FROM T_STK_OUTSTOCKAPPLY A JOIN
                                                            T_STK_OUTSTOCKAPPLYENTRY B ON A.FID = B.FID JOIN
                                                            T_BD_MATERIAL M1 ON B.FMATERIALID = M1.FMATERIALID
                                                            WHERE
                                                              A.FDOCUMENTSTATUS ='B'");
            DynamicObjectCollection reasonList = DBUtils.ExecuteDynamicObject(ctx, reasonQuery);
            if (reasonList != null && reasonList.Count > 0)
            {
                for (int i = 0; i < reasonList.Count; i++)
                {
                    string outBillno = Convert.ToString(reasonList[i]["FBILLNO"]);
                    string wlnum = Convert.ToString(reasonList[i]["FNUMBER"]);
                    string entryid = Convert.ToString(reasonList[i]["FENTRYID"]);

                    string query1 = string.Format($@"SELECT TOP
                                                              1 A.FBillno,
                                                              A.FSUPPLIERID,
                                                              C.FTAXPRICE,
                                                              B.Fqty from T_PUR_POORDER A
                                                              JOIN T_PUR_POORDEREntry B ON B.Fid= A.FID
                                                              JOIN T_PUR_POORDERENTRY_F C ON C.FENTRYID= B.FENTRYID
                                                              JOIN T_BD_MATERIAL M1 ON B.FMATERIALID = M1.FMATERIALID
                                                            WHERE
                                                              A.FDOCUMENTSTATUS= 'C' 
                                                              AND M1.FNUMBER= '{wlnum}' 
                                                            ORDER BY
                                                              A.Fdate DESC");
                    DynamicObjectCollection coll1 = DBUtils.ExecuteDynamicObject(ctx, query1);
                    if (coll1 != null && coll1.Count > 0)
                    {
                        for (int j = 0; j < coll1.Count; j++)
                        {
                            double ftaxprice = Convert.ToDouble(coll1[j]["FTAXPRICE"]);

                            string flag = GetPriceFlag(ftaxprice);

                            var upSql = string.Format(@"/*dialect*/UPDATE B SET B.Fpricesort = '{0}' 
                                                            FROM T_STK_OUTSTOCKAPPLY A JOIN
                                                            T_STK_OUTSTOCKAPPLYENTRY B ON A.FID = B.FID JOIN
                                                            T_BD_MATERIAL M1 ON B.FMATERIALID = M1.FMATERIALID
                                                            WHERE FBILLNO='{1}' AND M1.FNUMBER = '{2}' AND B.FENTRYID = '{3}'", flag, outBillno, wlnum, entryid);
                            DBUtils.Execute(ctx, upSql);
                        }
                    }
                }
            }
        }

        private string GetPriceFlag(double price)
        {
            if (price < 10)
                return "D";
            if (price >= 10 && price < 50)
                return "C";
            if (price >= 50 && price < 100)
                return "B";
            if (price >= 100)
                return "A";
            return string.Empty;
        }
    }
}
