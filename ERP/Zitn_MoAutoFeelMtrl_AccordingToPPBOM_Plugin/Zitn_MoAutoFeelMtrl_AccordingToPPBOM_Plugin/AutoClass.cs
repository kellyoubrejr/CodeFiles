using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Util;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Dynamic;

namespace Zitn_MoAutoFeelMtrl_AccordingToPPBOM_Plugin
{
    [Description("【服务插件】生产订单业务状态【开工】，根据PPBOM备注中的量程信息，自动生成生产补料单。"), HotUpdate]
    public class Class1 : AbstractOperationServicePlugIn
    {
        private static readonly string LogPath = @"D:\金蝶自定义日志文件\放大组件量程PPBOM.txt";
        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);

            K3CloudApiClient client = new K3CloudApiClient("http://10.0.32.18/k3cloud/");
            var loginResult = client.ValidateLogin(
                "688399bec6449e",
                "admin",
                "Flzx3qc!",
                2052
            );
            if (JObject.Parse(loginResult)["LoginResultType"].Value<int>() != 1)
            {
                WriteLog($"登录失败: {loginResult}");
                return;
            }
            WriteLog("登录成功");

            string llcj1 = string.Empty;
            string llcj = string.Empty;
            string llr = string.Empty;
            string bmfzr = string.Empty;
            string yfzg = string.Empty;

            var allIds = e.SelectedRows
                            .Select(row => row.DataEntity["Id"]?.ToString())
                            .ToList();
            var ids = string.Join(",", allIds);
            var billSql = string.Format($@"/*dialect*/SELECT
                                                      A.FBILLNO,
                                                      A.FID,
                                                      B.FQTY AS MOQTY,
                                                      B.FENTRYID,
                                                      B.FMATERIALID AS WLID,
                                                      M.FNUMBER AS WLNUM,
                                                      B1.FSTATUS,
                                                      B.F_ZMER_COMBO_TZK AS SYB,
                                                  M.FUSEORGID AS ORGID,FSEQ
                                                    FROM
                                                      T_PRD_MO A
                                                      JOIN T_PRD_MOENTRY B ON A.FID = B.FID
                                                      JOIN T_PRD_MOENTRY_A B1 ON B.FENTRYID = B1.FENTRYID
                                                      JOIN T_BD_MATERIAL M ON B.FMATERIALID = M.FMATERIALID
                                                    WHERE A.FID IN ({ids})");//生产事业部 F_UNW_COMBO_RE5
            var billDt = DBUtils.ExecuteDynamicObject(this.Context, billSql);
            if (billDt != null && billDt.Count > 0)
            {
                for (int i = 0; i < billDt.Count; i++)
                {
                    var billInfo = billDt[i];
                    
                    if (billInfo["WLNUM"] != null && billInfo["WLNUM"].ToString().StartsWith("90"))
                    {
                        var moWlnum = billInfo["WLNUM"].ToString();
                        var status = Convert.ToInt64(billInfo["FSTATUS"]);
                        var billNo = billInfo["FBILLNO"].ToString();
                        var wlId = billInfo["WLID"].ToString();
                        var moEntryId = billInfo["FENTRYID"].ToString();
                        var moId = billInfo["FID"].ToString();
                        var syb = billInfo["SYB"]?.ToString() ?? "";
                        var moSeq = Convert.ToInt32(billInfo["FSEQ"]);
                        var orgId = Convert.ToInt64(billInfo["ORGID"]);
                        var ownerNumber = GetOwner(orgId);
                        WriteLog($"开始处理 生产订单:{billNo} 物料内码:{wlId} 事业部:{syb}");
                        switch (syb)
                        {
                            case "1": bmfzr = "110791"; yfzg = "110791"; break;
                            case "2": bmfzr = "110862"; yfzg = "110861"; break;
                            case "3": bmfzr = "986184"; yfzg = "110861"; break;
                            case "4": bmfzr = "3475725"; yfzg = "110861"; break;
                            case "5": bmfzr = "3466500"; yfzg = "110791"; break;
                        }

                        // 查询车间 → 领料车间编码、领料人（按MO分录取车间）
                        var llcjList = DBUtils.ExecuteDynamicObject(this.Context, $@"/*dialect*/SELECT FWORKSHOPID FROM T_PRD_MOENTRY WHERE FENTRYID = {moEntryId}");
                        if (llcjList != null && llcjList.Count > 0) llcj1 = Convert.ToString(llcjList[0]["FWORKSHOPID"]);
                        var llcjList2 = DBUtils.ExecuteDynamicObject(this.Context, $@"/*dialect*/SELECT FNUMBER FROM T_BD_DEPARTMENT WHERE FDEPTID ={llcj1}");
                        if (llcjList2 != null && llcjList2.Count > 0) llcj = Convert.ToString(llcjList2[0]["FNUMBER"]);

                        var llrDict = new Dictionary<string, string>
                        {
                            {"SM11", "ZT20160001"},    // 电装车间：高靖
                            {"WD11", "ZT20190014"},    // 微电子车间：王茂桢
                            {"CG11", "ZT20130009"},    // 传感器车间：邓昌雪
                            {"JS11", "ZT20130009"},    // 加速度计车间：邓昌雪
                            {"JJ11", "ZT20130003"},    // 机加工车间：薛冰坤
                            {"DCJC01", "ZT20190014"},  // 电测车间：王茂桢
                        };
                        llr = llrDict.TryGetValue(llcj, out var l) ? l : "";

                        // 去重：检查该MO分录是否已生成过补料单，避免重复生成
                        var existFeedSql = $@"/*dialect*/SELECT COUNT(1) FROM T_PRD_FEEDMTRLDATA WHERE FMOENTRYID = {moEntryId}";
                        var existFeedDt = DBUtils.ExecuteDynamicObject(this.Context, existFeedSql);
                        if (existFeedDt != null && existFeedDt.Count > 0 && Convert.ToInt32(existFeedDt[0][0]) > 0)
                        {
                            WriteLog($"  MO分录{moEntryId}已存在补料单，跳过");
                            continue;
                        }

                        // 查询生产用料清单，收集所有量程编号 + 分录ID
                        var ppBomSql = string.Format($@"/*dialect*/SELECT A.FBILLNO AS PPBOMBILLNO,A.FID AS PPBOMID,A.FMOBILLNO,B.FENTRYID,B1.FMEMO,B.FSEQ FROM
                            T_PRD_PPBOM A JOIN T_PRD_PPBOMENTRY B ON A.FID = B.FID
                            LEFT JOIN T_PRD_PPBOMENTRY_L B1 ON B.FENTRYID = B1.FENTRYID
                            WHERE B.FMOENTRYID = '{moEntryId}' AND A.FMOBILLNO = '{billNo}'");
                        var ppBomDt = DBUtils.ExecuteDynamicObject(this.Context, ppBomSql);
                        if (ppBomDt == null || ppBomDt.Count == 0)
                        {
                            WriteLog($"  未找到用料清单 物料:{wlId} 订单:{billNo}");
                            continue;
                        }
                        WriteLog($"  用料清单分录数:{ppBomDt.Count}");
                        var ppBomBillNo = ppBomDt[0]["PPBOMBILLNO"]?.ToString() ?? "";
                        var ppBomId = ppBomDt[0]["PPBOMID"]?.ToString() ?? "";

                        var lcSet = new HashSet<int>();
                        var ppBomEntryIds = new List<string>();
                        var lcToPpbomSeq = new Dictionary<int, int>();
                        for (int j = 0; j < ppBomDt.Count; j++)
                        {
                            var ppBomInfo = ppBomDt[j];
                            if (ppBomInfo["FMEMO"] == null || !ppBomInfo["FMEMO"].ToString().Contains("量程")) continue;

                            var lcMatch = System.Text.RegularExpressions.Regex.Match(ppBomInfo["FMEMO"].ToString(), @"量程(1[0-2]|[1-9])");
                            if (!lcMatch.Success) continue;

                            var lcNum = GetLcNum(lcMatch.Value);
                            lcSet.Add(lcNum);
                            ppBomEntryIds.Add(ppBomInfo["FENTRYID"]?.ToString());
                            if (!lcToPpbomSeq.ContainsKey(lcNum))
                                lcToPpbomSeq[lcNum] = Convert.ToInt32(ppBomInfo["FSEQ"]);
                        }

                        if (lcSet.Count == 0)
                        {
                            WriteLog("  用料清单备注未包含量程信息");
                            continue;
                        }
                        WriteLog($"  检测到量程: [{string.Join(",", lcSet)}]");

                        // 遍历所有量程，查询量程配置表，汇总补料明细
                        var moQty = Convert.ToDecimal(billInfo["MOQTY"]);
                        var feedList = new List<FeedEntry>();
                        foreach (var lcNum in lcSet)
                        {
                            var lcSql = string.Format($@"/*dialect*/SELECT FMATERIAL1,ffzyl,ffmyl FROM ZMER_t_Cust100034 A JOIN ZMER_t_Cust_Entry100129 B ON A.FID = B.FID WHERE flc = '{lcNum}'");
                            var lcDt = DBUtils.ExecuteDynamicObject(this.Context, lcSql);
                            if (lcDt == null || lcDt.Count == 0) continue;

                            for (int m = 0; m < lcDt.Count; m++)
                            {
                                var lcRow = lcDt[m];
                                var ffzyl = Convert.ToDecimal(lcRow["ffzyl"]);
                                var ffmyl = Convert.ToDecimal(lcRow["ffmyl"]);
                                var feedQty = ffmyl != 0 ? ffzyl / ffmyl * moQty : 0;

                                feedList.Add(new FeedEntry
                                {
                                    MaterialId = lcRow["FMATERIAL1"]?.ToString(),
                                    FeedQty = feedQty,
                                    PpbomEntrySeq = lcToPpbomSeq.TryGetValue(lcNum, out var ppbomSeq) ? ppbomSeq : 0
                                });
                            }
                        }

                        if (feedList.Count == 0)
                        {
                            WriteLog("  量程配置表未查询到物料数据");
                            continue;
                        }
                        WriteLog($"  量程配置汇总明细行数:{feedList.Count}");

                        // 下推：PPBOM → 补料单（只推一个分录ID，生成一张补料单）
                        JObject jObj = new JObject
                        {
                            ["Ids"] = "",
                            ["Numbers"] = new JArray(),
                            ["EntryIds"] = ppBomEntryIds.FirstOrDefault() ?? "",
                            ["RuleId"] = "PRD_PPBOM2FEEDMTRL",
                            ["TargetBillTypeId"] = "",
                            ["TargetOrgId"] = 0,
                            ["TargetFormId"] = "PRD_FeedMtrl",
                            ["IsEnableDefaultRule"] = false,
                            ["IsDraftWhenSaveFail"] = true,
                            ["CustomParams"] = new JObject()
                        };
                        WriteLog($"  下推请求: {jObj}");

                        var resultJson = client.Push("PRD_PPBOM", jObj.ToString());
                        WriteLog($"  下推响应: {resultJson}");
                        var pushResult = JObject.Parse(resultJson);
                        if (!pushResult["Result"]["ResponseStatus"]["IsSuccess"].Value<bool>())
                        {
                            WriteLog($"  ★ 下推失败: {resultJson}");
                            continue;
                        }

                        var successList = pushResult["Result"]["ResponseStatus"]["SuccessEntitys"] as JArray;
                        if (successList == null || successList.Count == 0)
                        {
                            WriteLog("  ★ 下推成功但SuccessEntitys为空");
                            continue;
                        }
                        WriteLog($"  下推成功, 生成补料单数:{successList.Count}");

                        // 只处理第一张补料单，所有量程明细汇总到这一张单上
                        var item = successList[0];
                        {
                            long fid = item["Id"].Value<long>();
                            var FentryIds = item["EntryIds"]["FEntity"] as JArray;
                            if (FentryIds == null || FentryIds.Count == 0) { WriteLog("  ★ FentryIds为空"); continue; }

                            JObject modelobject = new JObject();
                            modelobject.Add("FID", fid);
                            modelobject.Add("FSrcBillType", "PRD_PPBOM");
                            modelobject.Add("FSrcBillNo", ppBomBillNo);
                            modelobject.Add("FSrcBillId", Convert.ToInt64(ppBomId));

                            JObject llyyObj = new JObject();
                            llyyObj.Add("FNumber", "LLYY031");
                            modelobject.Add("F_PAEZ_LLYY", llyyObj);

                            JObject wsObj = new JObject();
                            wsObj.Add("FNUMBER", llcj);
                            modelobject.Add("FWorkShopId", wsObj);

                            modelobject.Add("F_PAEZ_SYB", syb);

                            JObject lrrObj = new JObject();
                            lrrObj.Add("FNUMBER", llr);
                            modelobject.Add("FPickerId", lrrObj);

                            JObject bmfzrObj = new JObject();
                            bmfzrObj.Add("FID", bmfzr);
                            modelobject.Add("F_PAEZ_BMFZR", bmfzrObj);

                            JObject yfzgObj = new JObject();
                            yfzgObj.Add("FID", yfzg);
                            modelobject.Add("F_PAEZ_YFBBZ", yfzgObj);

                            JArray entryarray = new JArray();
                            for (int k = 0; k < feedList.Count; k++)
                            {
                                var feedData = feedList[k];

                                JObject entryobject = new JObject();
                                entryobject.Add("FEntryID", 0);

                                string materialCode = GetMemberBinder(feedData.MaterialId);

                                var kcSql = string.Format($@"/*dialect*/SELECT
                                                                          m.FNUMBER AS WLNUM,
                                                                          lotStock.FNUMBER AS FLOT,
                                                                        CASE
    
                                                                            WHEN TSUB.FBASELOCKQTY IS NULL THEN
                                                                            a.FBASEQTY ELSE a.FBASEQTY - TSUB.FBASELOCKQTY 
                                                                          END AS KYL,
                                                                          stockL.FName AS STOCK 
                                                                        FROM
                                                                          T_STK_INVENTORY a
                                                                          LEFT JOIN T_BD_LOTMASTER lotStock ON lotStock.FLOTID = a.FLOT 
                                                                          AND lotStock.FMATERIALID = a.FMATERIALID 
                                                                          AND a.FSTOCKORGID = lotStock.FUSEORGID
                                                                          LEFT JOIN (
                                                                          SELECT
                                                                            TLKE.FSUPPLYINTERID AS FINVENTRYID,
                                                                            SUM ( TLKE.FBASEQTY ) AS FBASELOCKQTY,
                                                                            SUM ( TLKE.FSECQTY ) AS FSECLOCKQTY 
                                                                          FROM
                                                                            T_PLN_RESERVELINKENTRY TLKE
                                                                            INNER JOIN T_PLN_RESERVELINK TLKH ON TLKE.FID = TLKH.FID 
                                                                          WHERE
                                                                            TLKE.FSUPPLYFORMID = 'STK_Inventory' 
                                                                            AND TLKE.FLINKTYPE = '4' 
                                                                          GROUP BY
                                                                            TLKE.FSUPPLYINTERID 
                                                                          ) TSUB ON a.FID = TSUB.FINVENTRYID
                                                                          INNER JOIN T_BD_MATERIAL m ON m.FMATERIALID = a.FMATERIALID
                                                                          INNER JOIN T_BD_MATERIAL_L ml ON ml.FMATERIALID = m.FMATERIALID 
                                                                          AND ml.FLOCALEID = 2052
                                                                          INNER JOIN t_BD_StockStatus kczt ON kczt.FSTOCKSTATUSID = a.FSTOCKSTATUSID
                                                                          INNER JOIN T_BD_STOCKSTATUS_L kcztL ON kcztL.FSTOCKSTATUSID = kczt.FSTOCKSTATUSID 
                                                                          AND kcztL.FLOCALEID = 2052
                                                                          INNER JOIN T_BD_UNIT_L baseUnit ON baseUnit.FUNITID = a.FBASEUNITID 
                                                                          AND baseUnit.FLOCALEID = 2052
                                                                          INNER JOIN T_BD_Stock_L stockL ON stockL.FSTOCKID = a.FSTOCKID 
                                                                          AND stockL.FLOCALEID = 2052 
                                                                        WHERE
                                                                          a.FBASEQTY > 0 
                                                                          AND A.FSTOCKID = 189314 
                                                                          AND M.FNUMBER = '{materialCode}'");
                                var kcDt = DBUtils.ExecuteDynamicObject(this.Context, kcSql);
                                if (kcDt == null || kcDt.Count == 0)
                                {
                                    WriteLog($"  ★ 库存查询不到物料:{materialCode}");
                                    continue;
                                }
                                var flot = kcDt[0]["FLOT"]?.ToString() ?? "";

                                //产品编码
                                JObject productObj = new JObject();
                                productObj.Add("FNumber", moWlnum);
                                entryobject.Add("FParentMaterialId", productObj);


                                // 物料编码（来自量程配置表）
                                JObject materialObj = new JObject();
                                materialObj.Add("FNumber", materialCode);
                                entryobject.Add("FMaterialId", materialObj);

                                // 数量 = 分子/分母 × 生产订单数量
                                entryobject.Add("FAppQty", feedData.FeedQty);
                                entryobject.Add("FActualQty", feedData.FeedQty);

                                JObject fstockidObject = new JObject();
                                fstockidObject.Add("FNUMBER", "1201");
                                entryobject.Add("FStockId", fstockidObject);

                                JObject lotObj = new JObject();
                                lotObj.Add("FNumber", flot);
                                entryobject.Add("FLot", lotObj);

                                JObject ffeedreasonidObject = new JObject();
                                ffeedreasonidObject.Add("FNumber", "BLYY09_SYS");
                                entryobject.Add("FFeedReasonId", ffeedreasonidObject);

                                // 生产订单号
                                entryobject.Add("FMoBillNo", billNo);

                                // 单据体车间
                                JObject entryWsObj = new JObject();
                                entryWsObj.Add("FNumber", llcj);
                                entryobject.Add("FEntryWorkShopId", entryWsObj);

                                // 产品货主类型 + 产品货主
                                entryobject.Add("FParentOwnerTypeId", "BD_OwnerOrg");
                                JObject parentOwnerObj = new JObject();
                                parentOwnerObj.Add("FNumber", ownerNumber);
                                entryobject.Add("FParentOwnerId", parentOwnerObj);

                                // 系统源单编号（PPBOM单号）
                                entryobject.Add("FEntrySrcBillNo", ppBomBillNo);
                                entryobject.Add("FEntrySrcBillType", "PRD_PPBOM");
                                entryobject.Add("FMoEntrySeq", moSeq);
                                entryobject.Add("FEntrySrcEntrySeq", feedData.PpbomEntrySeq);

                                // 仓位
                                JObject stockLocObj = new JObject();
                                JObject shelfObj = new JObject { { "FNumber", "01" } };
                                stockLocObj.Add("FSTOCKLOCID__FF100001", shelfObj);
                                stockLocObj.Add("FSTOCKLOCID__FF100002", shelfObj);
                                stockLocObj.Add("FSTOCKLOCID__FF100003", shelfObj);
                                entryobject.Add("FStockLocId", stockLocObj);

                                // 生产订单内码 + 分录内码
                                entryobject.Add("FMoId", Convert.ToInt64(moId));
                                entryobject.Add("FMoEntryId", Convert.ToInt64(moEntryId));

                                // 关联关系表（指向PPBOM分录，维持业务流程链路）
                                JArray linkArray = new JArray();
                                JObject linkObj = new JObject();
                                linkObj.Add("FEntity_Link_FRuleId", "453ea75e-a523-4e2a-bf33-cd019f707d1e");
                                linkObj.Add("FEntity_Link_FSTableName", "T_PRD_PPBOMENTRY");
                                linkObj.Add("FEntity_Link_FSBillId", Convert.ToInt64(ppBomId));
                                linkObj.Add("FEntity_Link_FSId", Convert.ToInt64(ppBomEntryIds.FirstOrDefault() ?? "0"));
                                linkObj.Add("FEntity_Link_FBaseActualQtyOld", feedData.FeedQty);
                                linkObj.Add("FEntity_Link_FBaseActualQty", feedData.FeedQty);
                                linkArray.Add(linkObj);
                                entryobject.Add("FEntity_Link", linkArray);

                                entryarray.Add(entryobject);
                            }

                            modelobject.Add("FEntity", entryarray);

                            JObject saveObj = new JObject
                            {
                                ["Model"] = modelobject,
                                ["IsDeleteEntry"] = false
                            };

                            WriteLog($"  保存请求 FID:{fid} 分录数:{entryarray.Count} IsDeleteEntry:true");
                            WriteLog($"  保存JSON: {saveObj}");
                            var saveResult = client.Save("PRD_FeedMtrl", saveObj.ToString());
                            WriteLog($"  保存响应: {saveResult}");
                            var saveResultJson = JObject.Parse(saveResult);
                            if (!saveResultJson["Result"]["ResponseStatus"]["IsSuccess"].Value<bool>())
                            {
                                var errMsg = saveResultJson["Result"]["ResponseStatus"]["Errors"]?[0]?["Message"]?.ToString() ?? "未知错误";
                                throw new KDBusinessException("", $"补料单保存失败 FID:{fid}，错误:{errMsg}");
                            }
                            else
                            {
                                WriteLog($"  √ 保存成功 FID:{fid} 明细:{entryarray.Count}行");
                            }
                        }
                    }
                }
            }
        }

        private string GetMemberBinder(string materialId)
        {
            var sql = $@"/*dialect*/SELECT FNUMBER FROM T_BD_MATERIAL WHERE FMATERIALID = '{materialId}' AND FUSEORGID = 101006";
            var result = DBUtils.ExecuteDynamicObject(this.Context, sql);
            if (result != null && result.Count > 0)
            {
                return result[0]["FNUMBER"]?.ToString() ?? "";
            }
            return "";
        }

        private void WriteLog(string msg)
        {
            try
            {
                File.AppendAllText(LogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}{Environment.NewLine}");
            }
            catch { }
        }

        private string GetOwner(long orgId)
        {
            var dic = new Dictionary<long, string>
            {
                { 1, "100" },
                { 101006, "101" }
            };
            return dic.ContainsKey(orgId) ? dic[orgId] : "";
        }

        private class FeedEntry
        {
            public string MaterialId { get; set; }
            public decimal FeedQty { get; set; }
            public int PpbomEntrySeq { get; set; }
        }

        /// <summary>
        /// 量程文字 → 枚举值映射
        /// </summary>
        private static readonly Dictionary<string, int> LcMap = new Dictionary<string, int>()
        {
            { "量程1",  1  },
            { "量程2",  2  },
            { "量程3",  3  },
            { "量程4",  4  },
            { "量程5",  5  },
            { "量程6",  6  },
            { "量程7",  7  },
            { "量程8",  8  },
            { "量程9",  9  },
            { "量程10", 10 },
            { "量程11", 11 },
            { "量程12", 12 },
        };

        private static int GetLcNum(string lcValue)
        {
            return LcMap.TryGetValue(lcValue, out var num) ? num : 0;
        }
    }
}
