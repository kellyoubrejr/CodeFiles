using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Zitn_DBSQD_Audit_To_PRD_FeedMtrl_Operation_Plugin
{
    [Description("【服务插件】调拨申请单审核，自动生成直接调拨单"), HotUpdate]
    public class AuditOperation : AbstractOperationServicePlugIn
    {
        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);

            string dbsqBillno = string.Empty;
            List<long> entryIdList = new List<long>();            
            List<string> cangWeiList = new List<string>();
            string ziduan = string.Empty;

            foreach (DynamicObject billObj in e.DataEntitys)
            {
                dbsqBillno = Convert.ToString(billObj["BillNo"]);

                // 查询调拨单明细
                string reasonQuery = $@"/*dialect*/
                    SELECT B.FENTRYID,FSTOCKLOCINID
                    FROM T_STK_STKTRANSFERAPP A
                    JOIN T_STK_STKTRANSFERAPPENTRY B ON A.FID = B.FID
                    WHERE FBILLNO = '{dbsqBillno}'";
                var reasonList = DBUtils.ExecuteDynamicObject(this.Context, reasonQuery);
                if (reasonList == null || reasonList.Count == 0) continue;
                for(int i = 0; i < reasonList.Count; i++)
                {
                    entryIdList.Add(Convert.ToInt64(reasonList[i]["FENTRYID"]));
                    cangWeiList.Add(Convert.ToString(reasonList[i]["FSTOCKLOCINID"]));
                }

                var reasonQuery1 = string.Format(@"/*dialect*/SELECT DISTINCT
                                                              T1.FDATAVALUE 
                                                            FROM
                                                              T_STK_STKTRANSFERAPP A
                                                              JOIN T_STK_STKTRANSFERAPPENTRY B ON A.FID = B.FID
                                                              JOIN T_BD_MATERIAL C ON B.FMATERIALID = C.FMATERIALID
                                                              JOIN T_BAS_ASSISTANTDATAENTRY_L T1 ON A.F_PAEZ_DBYY = T1.FENTRYID 
                                                            WHERE
                                                              FBILLNO = '{0}'", dbsqBillno);
                DynamicObjectCollection reasonList1 = DBUtils.ExecuteDynamicObject(this.Context, reasonQuery1);
                if (reasonList1 != null && reasonList1.Count > 0)
                {
                    for (int ij = 0; ij < reasonList1.Count; ij++)
                    {
                        if (Convert.ToString(reasonList1[ij]["FDATAVALUE"]) == "生产借用原材料及半成品")
                        {
                            ziduan = "生产借用原材料及半成品";
                        }
                    }
                }
            }

            if(ziduan == "生产借用原材料及半成品")
            {
                if (!entryIdList.Any())
                    return;

                // =====  登录 WebApi =====
                //dev
                K3CloudApiClient client = new K3CloudApiClient("http://10.0.32.18/k3cloud/");
                var loginResult = client.ValidateLogin(
                    "688399bec6449e",
                    "admin",
                    "Flzx3qc!",
                    2052
                );
                //test
                /*K3CloudApiClient client = new K3CloudApiClient("http://127.0.0.1/k3cloud/");
                var loginResult = client.ValidateLogin(
                    "6940c27ae377d5",
                    "刘总",
                    "yangwei11",
                    2052
                );*/

                if (JObject.Parse(loginResult)["LoginResultType"].Value<int>() != 1)
                    return;

                string entryIds = string.Join(",", entryIdList.Distinct());

                JObject jObj = new JObject();

                jObj.Add("Ids", "");
                jObj.Add("Numbers", new JArray());
                jObj.Add("EntryIds", entryIds);
                jObj.Add("RuleId", "StkTransferApply-StkTransferDirect");
                jObj.Add("TargetBillTypeId", "");
                jObj.Add("TargetOrgId", 0);
                jObj.Add("TargetFormId", "STK_TransferDirect");
                jObj.Add("IsEnableDefaultRule", false);
                jObj.Add("IsDraftWhenSaveFail", true);
                jObj.Add("CustomParams", new JObject());

                var resultJson = client.Push("STK_TRANSFERAPPLY", jObj.ToString());
                var pushResult = JObject.Parse(resultJson);
                if (!pushResult["Result"]["ResponseStatus"]["IsSuccess"].Value<bool>()) return;

                var successList = pushResult["Result"]["ResponseStatus"]["SuccessEntitys"] as JArray;
                foreach (var item in successList)
                {
                    long fid = item["Id"].Value<long>();
                    var FentryIds = item["EntryIds"]["FBillEntry"] as JArray;

                    JObject modelobject = new JObject();
                    modelobject.Add("FID", fid);

                    //ZT20170002
                    JObject cgyObj = new JObject();
                    cgyObj.Add("FNumber", "ZT20170002");
                    modelobject.Add("FStockerId", cgyObj);

                    JArray entryarray = new JArray();
                    for (int i = 0; i < FentryIds.Count; i++)
                    {
                        long entryId = FentryIds[i].Value<long>();

                        JObject entryobject = new JObject();
                        entryobject.Add("FEntryID", entryId);

                        //var query1 = string.Format(@"/*dialect*/
                        //                SELECT B.FENTRYID,FZJDBBILLNO,T2.FLOT,T3.FNUMBER
                        //                                    FROM T_STK_STKTRANSFERAPP A
                        //                                    JOIN T_STK_STKTRANSFERAPPENTRY B ON A.FID = B.FID
                        //                                    JOIN T_BD_MATERIAL M1 ON B.FMATERIALID = M1.FMATERIALID
                        //                                    JOIN T_STK_STKTRANSFERIN T1 ON B.FZJDBBILLNO = T1.FBILLNO
                        //                                    JOIN T_STK_STKTRANSFERINENTRY T2 ON T1.FID = T2.FID
                        //                                    JOIN T_BD_LOTMASTER T3 ON T2.FLOT = T3.FLOTID
                        //                                    WHERE A.FBILLNO = '{0}' AND B.FENTRYID = {1}", dbsqBillno, entryIdList[i]);
                        //var queryResult = DBUtils.ExecuteDynamicObject(this.Context, query1);
                        //if (queryResult == null || queryResult.Count == 0) continue;
                        //var lotFnumber = queryResult[0]["FNUMBER"].ToString();

                        //批号
                        /*JObject lotObj = new JObject();
                        lotObj.Add("FNumber", lotFnumber);
                        entryobject.Add("FLot", lotObj);*/

                        //仓位 
                        /*JObject stockLocObj = new JObject();
                        JObject shelf = new JObject();
                        shelf.Add("FNumber", "100062");
                        stockLocObj.Add("FDESTSTOCKLOCID__FF100001", shelf);
                        JObject layer = new JObject();
                        layer.Add("FNumber", "100034");
                        stockLocObj.Add("FDESTSTOCKLOCID__FF100002", layer);
                        JObject pos = new JObject();
                        pos.Add("FNumber", "100013");
                        stockLocObj.Add("FDESTSTOCKLOCID__FF100003", pos);*/
                        //entryobject.Add("FDestStockLocId", cangWeiList[i]);
                        var sql = string.Format(@"/*dialect*/UPDATE B  SET B.FDESTSTOCKLOCID = '{0}'  FROM T_STK_STKTRANSFERIN A JOIN T_STK_STKTRANSFERINENTRY B ON A.FID = B.FID  WHERE a.fid = '{1}' 
 AND B.fentryid = '{2}'", cangWeiList[i], fid, entryId);
                        DBUtils.Execute(this.Context, sql);

                        entryarray.Add(entryobject);
                    }
                    modelobject.Add("FBillEntry", entryarray);

                    JObject saveObj = new JObject
                    {
                        ["Model"] = modelobject,
                        ["IsDeleteEntry"] = false
                    };

                    var saveResult = client.Save("STK_TransferDirect", saveObj.ToString());
                    var saveResultObj = JObject.Parse(saveResult);
                    if (saveResultObj["Result"]["ResponseStatus"]["IsSuccess"].Value<bool>())
                    {
                        client.Submit("STK_TransferDirect", new JObject { ["Ids"] = fid.ToString() }.ToString());
                    }
                }
            }
            
        }
    }
}
