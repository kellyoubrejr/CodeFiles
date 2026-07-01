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

namespace Zitn_DBSQD_Audit_To_ZJDB_Operation_Plugin
{
    [Description("【服务插件】调拨申请单审核，差异数量小于0，自动生成生产补料单"), HotUpdate]
    public class AuditOperation : AbstractOperationServicePlugIn
    {
        public class FeedMtrlEntry
        {
            public long FEntryId { get; set; }       // 新增：WebApi FEntryID
            public string MaterialNumber { get; set; }
            public long Qty { get; set; }
            public string StockId { get; set; }
            public string Lot { get; set; }
            public string CpNum { get; set; }
            public string StockLocId { get; set; }
        }

        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);

            string dbsqBillno = string.Empty;
            string fmobillno = string.Empty;
            string llcj = string.Empty;
            string syb = string.Empty;
            string llr = string.Empty;
            string yfzg = string.Empty;
            string bmfzr = string.Empty;

            List<long> fentryids = new List<long>();
            List<string> materialNumbers = new List<string>();
            List<FeedMtrlEntry> entryDatas = new List<FeedMtrlEntry>();

            foreach (DynamicObject billObj in e.DataEntitys)
            {
                dbsqBillno = Convert.ToString(billObj["BillNo"]);

                // 查询调拨单明细
                string reasonQuery = $@"/*dialect*/
                    SELECT B.FENTRYID,FCYQTY, FCPNUM, FMOBILLNO, M1.FNUMBER, B.FSTOCKID, FPICKMTRLBILLNO, FZJDBBILLNO, FSTOCKLOCINID
                    FROM T_STK_STKTRANSFERAPP A
                    JOIN T_STK_STKTRANSFERAPPENTRY B ON A.FID = B.FID
                    JOIN T_BD_MATERIAL M1 ON B.FMATERIALID = M1.FMATERIALID
                    WHERE FBILLNO = '{dbsqBillno}'";
                var reasonList = DBUtils.ExecuteDynamicObject(this.Context, reasonQuery);
                if (reasonList == null || reasonList.Count == 0) continue;

                for (int i = 0; i < reasonList.Count; i++)
                {
                    string wlnumber = Convert.ToString(reasonList[i]["FNUMBER"]);
                    string cw = Convert.ToString(reasonList[i]["FSTOCKLOCINID"]);
                    long fcyqty = Convert.ToInt64(reasonList[i]["FCYQTY"]);
                    string fcpnum = Convert.ToString(reasonList[i]["FCPNUM"]);
                    string fstockid = Convert.ToString(reasonList[i]["FSTOCKID"]);
                    string fpickmtrlbillno = Convert.ToString(reasonList[i]["FPICKMTRLBILLNO"]);
                    string zjdbBillno = Convert.ToString(reasonList[i]["FZJDBBILLNO"]);
                    long fentryid = Convert.ToInt64(reasonList[i]["FENTRYID"]);

                    // 批号
                    //string ph = string.Empty;
                    //var zjList = DBUtils.ExecuteDynamicObject(this.Context, $@"/*dialect*/
                    //    SELECT FLOT FROM T_STK_STKTRANSFERIN A 
                    //    JOIN T_STK_STKTRANSFERINENTRY B ON A.FID = B.FID
                    //    JOIN T_BD_MATERIAL M1 ON B.FMATERIALID = M1.FMATERIALID
                    //    WHERE FBILLNO='{zjdbBillno}' AND M1.FNUMBER='{wlnumber}'");
                    //if (zjList != null && zjList.Count > 0)
                    //{
                    //    string phid = Convert.ToString(zjList[0]["FLOT"]);
                    //    var phList = DBUtils.ExecuteDynamicObject(this.Context, $@"/*dialect*/SELECT FNUMBER FROM T_BD_LOTMASTER WHERE FLOTID={phid}");
                    //    if (phList != null && phList.Count > 0) ph = Convert.ToString(phList[0]["FNUMBER"]);
                    //}

                    var query1 = string.Format(@"/*dialect*/
                                        SELECT B.FENTRYID,FZJDBBILLNO,T2.FLOT,T3.FNUMBER
                                                            FROM T_STK_STKTRANSFERAPP A
                                                            JOIN T_STK_STKTRANSFERAPPENTRY B ON A.FID = B.FID
                                                            JOIN T_BD_MATERIAL M1 ON B.FMATERIALID = M1.FMATERIALID
                                                            JOIN T_STK_STKTRANSFERIN T1 ON B.FZJDBBILLNO = T1.FBILLNO
                                                            JOIN T_STK_STKTRANSFERINENTRY T2 ON T1.FID = T2.FID
                                                            JOIN T_BD_LOTMASTER T3 ON T2.FLOT = T3.FLOTID
                                                            WHERE A.FBILLNO = '{0}' AND B.FENTRYID = {1}", dbsqBillno, fentryid);
                    var queryResult = DBUtils.ExecuteDynamicObject(this.Context, query1);
                    if (queryResult == null || queryResult.Count == 0) continue;
                    var ph = queryResult[0]["FNUMBER"].ToString();




                    // 领料人/部门负责人
                    string llr1 = string.Empty;
                    var llList = DBUtils.ExecuteDynamicObject(this.Context, $@"/*dialect*/SELECT F_PAEZ_SYB, FPICKERID FROM T_PRD_PICKMTRL WHERE FBILLNO='{fpickmtrlbillno}'");
                    if (llList != null && llList.Count > 0)
                    {
                        syb = Convert.ToString(llList[0]["F_PAEZ_SYB"]);
                        llr1 = Convert.ToString(llList[0]["FPICKERID"]);
                        var sql2 = string.Format("/*dialect*/SELECT FNUMBER FROM T_BD_STAFF WHERE FSTAFFID={0}", llr1);
                        var llrList = DBUtils.ExecuteDynamicObject(this.Context, sql2);
                        if (llrList != null && llrList.Count > 0) llr = Convert.ToString(llrList[0]["FNUMBER"]);
                        switch (syb)
                        {
                            case "1": bmfzr = "110791"; yfzg = "110791"; break;
                            case "2": bmfzr = "110862"; yfzg = "110861"; break;
                            case "3": bmfzr = "986184"; yfzg = "110861"; break;
                            case "4": bmfzr = "3475725"; yfzg = "110861"; break;
                            case "5": bmfzr = "3466500"; yfzg = "110791"; break;
                        }
                    }

                    if (fcyqty < 0)
                    {
                        if (!materialNumbers.Contains(wlnumber)) materialNumbers.Add(wlnumber);

                        string llcj1 = string.Empty;
                        if (string.IsNullOrWhiteSpace(fmobillno))
                        {

                            fmobillno = Convert.ToString(reasonList[i]["FMOBILLNO"]);
                            var llcjList = DBUtils.ExecuteDynamicObject(this.Context, $@"/*dialect*/SELECT FWORKSHOPID FROM T_PRD_MO WHERE FBILLNO='{fmobillno}'");
                            if (llcjList != null && llcjList.Count > 0) llcj1 = Convert.ToString(llcjList[0]["FWORKSHOPID"]);
                            var llcjList2 = DBUtils.ExecuteDynamicObject(this.Context, $@"/*dialect*/SELECT FNUMBER FROM T_BD_DEPARTMENT WHERE FDEPTID ={llcj1}");
                            if (llcjList2 != null && llcjList2.Count > 0) llcj = Convert.ToString(llcjList2[0]["FNUMBER"]);
                        }

                        entryDatas.Add(new FeedMtrlEntry
                        {
                            MaterialNumber = wlnumber,
                            Qty = fcyqty,
                            StockId = fstockid,
                            Lot = ph,
                            CpNum = fcpnum,
                            StockLocId = cw
                        });
                    }
                }

                if (materialNumbers.Count == 0) continue;

                string inCondition = string.Join("','", entryDatas.Select(p => p.MaterialNumber).Distinct());
                var result = DBUtils.ExecuteDynamicObject(this.Context, $@"/*dialect*/
                    SELECT DISTINCT a.FBILLNO, a.FID, m1.FNUMBER, m2.FNUMBER, b.FENTRYID
                    FROM T_PRD_PPBOM A
                    JOIN T_PRD_PPBOMENTRY B ON A.FID=B.FID
                    JOIN T_BD_MATERIAL M1 ON B.FMATERIALID=M1.FMATERIALID
                    JOIN T_BD_MATERIAL M2 ON A.FMATERIALID=M2.FMATERIALID
                    JOIN T_PRD_PPBOMENTRY_C B1 ON B.FENTRYID=B1.FENTRYID
                    JOIN T_PRD_PPBOMENTRY_Q B2 ON B.FENTRYID=B2.FENTRYID
                    JOIN T_PRD_MO MO1 ON A.FMOBILLNO=MO1.FBILLNO
                    JOIN T_PRD_MOENTRY MO2 ON MO1.FID=MO2.FID
                    JOIN T_PRD_MOENTRY_A MO3 ON MO2.FENTRYID=MO3.FENTRYID
                    WHERE A.FMOBILLNO='{fmobillno}' AND B1.FISSUETYPE=1 AND B2.FPICKEDQTY>0
                    AND M1.FNUMBER IN('{inCondition}') AND M2.FNUMBER='{entryDatas[0].CpNum}' AND MO3.FSTATUS=4");

                if (result != null && result.Count > 0)
                {
                    for (int i = 0; i < result.Count; i++)
                        entryDatas[i].FEntryId = Convert.ToInt64(result[i]["FENTRYID"]);
                }
            }

            // 登录 WebApi
            //K3CloudApiClient client = new K3CloudApiClient("http://127.0.0.1/k3cloud/");
            //var loginResult = client.ValidateLogin("6940c27ae377d5", "刘总", "yangwei11", 2052);
            K3CloudApiClient client = new K3CloudApiClient("http://10.0.32.18/k3cloud/");
            var loginResult = client.ValidateLogin(
                "688399bec6449e",
                "admin",
                "Flzx3qc!",
                2052
            );
            if (JObject.Parse(loginResult)["LoginResultType"].Value<int>() != 1) return;

            JObject jObj = new JObject
            {
                ["Ids"] = "",
                ["Numbers"] = new JArray(),
                ["EntryIds"] = string.Join(",", entryDatas.Select(p => p.FEntryId).Distinct()),
                ["RuleId"] = "PRD_PPBOM2FEEDMTRL",
                ["TargetBillTypeId"] = "",
                ["TargetOrgId"] = 0,
                ["TargetFormId"] = "PRD_FeedMtrl",
                ["IsEnableDefaultRule"] = false,
                ["IsDraftWhenSaveFail"] = true,
                ["CustomParams"] = new JObject()
            };

            var resultJson = client.Push("PRD_PPBOM", jObj.ToString());
            var pushResult = JObject.Parse(resultJson);
            if (!pushResult["Result"]["ResponseStatus"]["IsSuccess"].Value<bool>()) return;

            var successList = pushResult["Result"]["ResponseStatus"]["SuccessEntitys"] as JArray;
            foreach (var item in successList)
            {
                long fid = item["Id"].Value<long>();
                var FentryIds = item["EntryIds"]["FEntity"] as JArray;

                JObject modelobject = new JObject();
                modelobject.Add("FID", fid);

                // 领料原因
                JObject llyyObj = new JObject();
                llyyObj.Add("FNumber", "LLYY030");
                modelobject.Add("F_PAEZ_LLYY", llyyObj);

                // 领料车间
                JObject wsObj = new JObject();
                wsObj.Add("FNUMBER", llcj);
                modelobject.Add("FWorkShopId", wsObj);

                //事业部
                modelobject.Add("F_PAEZ_SYB", syb);

                // 领料人
                JObject lrrObj = new JObject();
                lrrObj.Add("FNUMBER", llr);
                modelobject.Add("FPickerId", lrrObj);

                //部门负责人
                JObject bmfzrObj = new JObject();
                //bmfzrObj.Add("FSTAFFNUMBER", "001");
                bmfzrObj.Add("fid", bmfzr);
                modelobject.Add("F_PAEZ_BMFZR", bmfzrObj);

                //研发总工
                JObject yfzgObj = new JObject();
                //yfzgObj.Add("FSTAFFNUMBER", "001");
                yfzgObj.Add("FID", yfzg);
                modelobject.Add("F_PAEZ_YFBBZ", yfzgObj);

                // ---- 单据体 ----
                JArray entryarray = new JArray();
                for (int i = 0; i < FentryIds.Count; i++)
                {
                    long entryId = FentryIds[i].Value<long>();
                    var entryData = i < entryDatas.Count ? entryDatas[i] : entryDatas[0];
                    if (entryData == null) continue;

                    JObject entryobject = new JObject();
                    entryobject.Add("FEntryID", entryId);

                    //申请数量
                    entryobject.Add("FAppQty", Math.Abs(entryData.Qty));

                    //实发数量
                    entryobject.Add("FActualQty", Math.Abs(entryData.Qty));//Math.Abs(fcyqty)


                    var cangku = DBUtils.ExecuteDynamicObject(this.Context, $@"/*dialect*/SELECT FNUMBER FROM T_BD_STOCK WHERE FSTOCKID ='{entryData.StockId}'");
                    if (cangku == null || cangku.Count == 0) continue;
                     var cangkubum = Convert.ToString(cangku[0]["FNUMBER"]);



                    // 仓库
                    JObject fstockidObject = new JObject();
                    fstockidObject.Add("FNUMBER", cangkubum);
                    entryobject.Add("FStockId", fstockidObject);

                    //批号
                    JObject lotObj = new JObject();
                    lotObj.Add("FNumber", entryData.Lot);
                    //lotObj.Add("FNumber", "R20250412-6（N903D-06）");
                    entryobject.Add("FLot", lotObj);

                    // 补料原因
                    JObject ffeedreasonidObject = new JObject();
                    ffeedreasonidObject.Add("FNumber", "BLYY02_SYS");
                    entryobject.Add("FFeedReasonId", ffeedreasonidObject);


                    /*JObject stockLocObj = new JObject();
                    JObject shelf = new JObject { ["FID"] = 100036 };
                    stockLocObj.Add("FSTOCKLOCID__FF100001", shelf);
                    entryobject.Add("FStockLocId", stockLocObj);*/

                    var sql = string.Format(@"/*dialect*/UPDATE B  SET B.FSTOCKLOCID = '{0}'  FROM T_PRD_FEEDMTRL A JOIN T_PRD_FEEDMTRLDATA B ON A.FID = B.FID  WHERE a.fid = '{1}' 
 AND B.fentryid = '{2}'", entryData.StockLocId, fid, entryId);
                    DBUtils.Execute(this.Context, sql);

                    entryarray.Add(entryobject);
                }

                modelobject.Add("FEntity", entryarray);

                JObject saveObj = new JObject
                {
                    ["Model"] = modelobject,
                    ["IsDeleteEntry"] = false
                };

                var saveResult = client.Save("PRD_FeedMtrl", saveObj.ToString());
                var saveResultObj = JObject.Parse(saveResult);
                if (saveResultObj["Result"]["ResponseStatus"]["IsSuccess"].Value<bool>())
                {
                    client.Submit("PRD_FeedMtrl", new JObject { ["Ids"] = fid.ToString() }.ToString());
                }
            }
        }
    }
}