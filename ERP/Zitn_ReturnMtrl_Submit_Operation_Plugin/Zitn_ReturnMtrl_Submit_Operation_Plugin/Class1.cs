using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.ComponentModel;

namespace Zitn_ReturnMtrl_Submit_Operation_Plugin
{
    [Description("【服务插件】：生产退料单提交时，根据对应物料的采购价格更新价值分类字段")]
    [Kingdee.BOS.Util.HotUpdate]
    public class Class1 : AbstractOperationServicePlugIn
    {
        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);

            string returnBillno = string.Empty;

            string ids = string.Empty;

            foreach (DynamicObject obj in e.DataEntitys)
            {
                long fid = Convert.ToInt64(obj["Id"]);

                if (!string.IsNullOrWhiteSpace(ids))
                    ids += ",";

                ids += fid.ToString();
            }

            var reasonQuery = string.Format($@"/*dialect*/SELECT A.FBILLNO,FPRICE,Fpricesort,FNUMBER,B.FENTRYID FROM T_PRD_RETURNMTRL A JOIN
                                                            T_PRD_RETURNMTRLENTRY B ON A.FID = B.FID JOIN 
                                                            T_BD_MATERIAL M1 ON B.FMATERIALID = M1.FMATERIALID
                                                            WHERE
                                                              A.FID IN ({ids})");
            DynamicObjectCollection reasonList = DBUtils.ExecuteDynamicObject(this.Context, reasonQuery);
            if (reasonList != null && reasonList.Count > 0)
            {
                for (int i = 0; i < reasonList.Count; i++)
                {
                    returnBillno = Convert.ToString(reasonList[i]["FBILLNO"]);
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
                    DynamicObjectCollection coll1 = DBUtils.ExecuteDynamicObject(this.Context, query1);
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
                            DBUtils.Execute(this.Context, upSql);
                        }
                    }
                }
            }

        }
        private string GetPriceFlag(double price)
        {
            if (price > 10)
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
