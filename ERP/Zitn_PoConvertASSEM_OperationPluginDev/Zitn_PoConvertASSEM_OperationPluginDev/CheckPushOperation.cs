using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;

namespace Zitn_PoConvertASSEM_OperationPluginDev
{
    [Description("【服务operation】:采购入库单审核，自动下推组装拆卸单"), HotUpdate]
    public class CheckPushOperation : AbstractOperationServicePlugIn
    {
        private bool _hasSplit = false;
        private List<string> _extraMessages = new List<string>();
        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);

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

            foreach (Kingdee.BOS.Orm.DataEntity.DynamicObject billObj in e.DataEntitys)
            {
                string billNo = Convert.ToString(billObj["BillNo"]);
                long srcFid = Convert.ToInt64(billObj["Id"]);

                // 查询采购入库单中的费用物料
                string cgrkSql = $@"
                /*dialect*/
                SELECT
                  M.FNUMBER AS WLNUM,
                  A.FSTOCKORGID,
                  B.FENTRYID,
                  B.FREALQTY AS QTY,
                  S.FNUMBER AS FSTOCK,
                  L.FNUMBER AS FLOT,
                  F1.FNUMBER AS SHELF,
                  F2.FNUMBER AS LAYER,
                  F3.FNUMBER AS POS
                FROM
                  t_STK_InStock A
                  JOIN T_STK_INSTOCKENTRY B ON A.FID = B.FID
                  JOIN T_BD_MATERIAL M ON B.FMATERIALID = M.FMATERIALID
                  JOIN T_BD_MATERIALGROUP G ON M.FMATERIALGROUP = G.FID
                  JOIN T_BD_MATERIALGROUP_L G1 ON G.FID = G1.FID 
                  AND G1.FNAME LIKE '%费用%' 
                  JOIN T_BD_STOCK S ON B.FSTOCKID = S.FSTOCKID
                  JOIN T_BD_LOTMASTER L ON B.FLOT = L.FLOTID
                  JOIN T_BAS_FLEXVALUESDETAIL F ON B.FSTOCKLOCID = F.FID
                  JOIN T_BAS_FLEXVALUESENTRY F1 ON F.FF100001 = F1.FENTRYID 
                 JOIN T_BAS_FLEXVALUESENTRY F2  ON F.FF100002 = F2.FENTRYID 
                 JOIN T_BAS_FLEXVALUESENTRY F3  ON F.FF100003 = F3.FENTRYID
                WHERE A.FBILLNO = '{billNo}'";

                DynamicObjectCollection cgrkResult =
                    DBUtils.ExecuteDynamicObject(this.Context, cgrkSql);

                if (cgrkResult == null || cgrkResult.Count == 0)
                {
                    continue;
                }

                // ========== 收集所有分录ID，一次性下推 ==========
                List<string> entryIds = new List<string>();
                for (int i = 0; i < cgrkResult.Count; i++)
                {
                    string entryId = Convert.ToString(cgrkResult[i]["FENTRYID"]);
                    if (!entryIds.Contains(entryId))
                    {
                        entryIds.Add(entryId);
                    }
                }

                JObject pushObj = new JObject();
                pushObj.Add("Ids", "");
                pushObj.Add("Numbers", new JArray());
                pushObj.Add("EntryIds", string.Join(",", entryIds));
                pushObj.Add("RuleId", "38bf9dde-c38f-414b-b756-79931101f168");
                pushObj.Add("TargetBillTypeId", "");
                pushObj.Add("TargetOrgId", 0);
                pushObj.Add("TargetFormId", "STK_AssembledApp");
                pushObj.Add("IsEnableDefaultRule", false);
                pushObj.Add("IsDraftWhenSaveFail", true);
                pushObj.Add("CustomParams", new JObject());

                var pushJson = client.Push("STK_InStock", pushObj.ToString());
                JObject pushResult = JObject.Parse(pushJson);
                bool isSuccess =
                    pushResult["Result"]["ResponseStatus"]["IsSuccess"]
                    .Value<bool>();

                if (!isSuccess)
                {
                    JArray pushErrors =
                        pushResult["Result"]["ResponseStatus"]["Errors"] as JArray;

                    string pushErrorMsg =
                        pushErrors != null && pushErrors.Count > 0
                        ? pushErrors[0]["Message"].ToString()
                        : "下推失败";

                    var failResult = new OperateResult();

                    failResult.SuccessStatus = false;

                    failResult.Number =
                        ObjectUtils.Object2String(
                            this.BusinessInfo.GetBillNoField()
                            .DynamicProperty
                            .GetValueFast(e.DataEntitys[0]));

                    failResult.Message = string.Format(
                        "单据【{0}】自动生成组装拆卸单失败，原因：{1}",
                        failResult.Number,
                        pushErrorMsg);

                    this.OperationResult.OperateResult.Add(failResult);

                    // 第三条消息
                    _extraMessages.Add(
                        string.Format(
                            "单据【{0}】自动生成组装拆卸单执行失败，请检查后重试！原因：{1}",
                            failResult.Number, pushErrorMsg));


                    continue;
                }

                JArray successList =
                    pushResult["Result"]["ResponseStatus"]["SuccessEntitys"]
                    as JArray;

                if (successList == null || successList.Count == 0)
                {
                    continue;
                }

                long targetFid = Convert.ToInt64(successList[0]["Id"]);

                // ========== 循环构建数据 ==========
                JArray entityArray = new JArray();           // 成品表体
                bool hasSplit = false; // 拆分补录标识

                for (int i = 0; i < cgrkResult.Count; i++)
                {                    
                    HashSet<string> materialSet = new HashSet<string>();
                    // 当前采购入库物料信息
                    string wlnum = Convert.ToString(cgrkResult[i]["WLNUM"]);
                    string entryId = Convert.ToString(cgrkResult[i]["FENTRYID"]);
                    long orgId = Convert.ToInt64(cgrkResult[i]["FSTOCKORGID"]);
                    long qty = Convert.ToInt64(cgrkResult[i]["QTY"]);
                    string stockCode = Convert.ToString(cgrkResult[i]["FSTOCK"]);
                    string lot = Convert.ToString(cgrkResult[i]["FLOT"]);
                    string shelfCode = Convert.ToString(cgrkResult[i]["SHELF"]);
                    string layerCode = Convert.ToString(cgrkResult[i]["LAYER"]);
                    string posCode = Convert.ToString(cgrkResult[i]["POS"]);

                    // 查询BOM
                    string bomSql = $@"
                    /*dialect*/
                    SELECT top 1
                        A.FNUMBER AS BOM,
                        M.FNUMBER AS BOMFATHER,
                        M1.FNUMBER AS BOMSON
                    FROM T_ENG_BOM A
                    JOIN T_ENG_BOMCHILD B ON A.FID = B.FID
                    JOIN T_BD_MATERIAL M ON A.FMATERIALID = M.FMATERIALID
                    JOIN T_BD_MATERIAL M1 ON B.FMATERIALID = M1.FMATERIALID
                    WHERE M1.FNUMBER = '{wlnum}'
                    AND M1.FUSEORGID = {orgId}
                    order by A.FNUMBER desc";

                    DynamicObjectCollection bomResult =
                        DBUtils.ExecuteDynamicObject(this.Context, bomSql);

                    if (bomResult == null || bomResult.Count == 0)
                    {
                        continue;
                    }

                    string bom = Convert.ToString(bomResult[0]["BOM"]);
                    string bomFather = Convert.ToString(bomResult[0]["BOMFATHER"]);

                    // ========== 构建成品行 ==========
                    JObject entityObj = new JObject();

                    JObject materialObj = new JObject();
                    materialObj.Add("FNumber", bomFather);
                    entityObj.Add("FMaterialID", materialObj);

                    JObject bomObj = new JObject();
                    bomObj.Add("FNumber", bom);
                    entityObj.Add("FBomID", bomObj);
                    entityObj.Add("FQty", qty);

                    JObject stockObj = new JObject();
                    stockObj.Add("FNumber", stockCode);
                    entityObj.Add("FStockID", stockObj);

                    JObject stockLocObj = new JObject();
                    JObject shelf = new JObject();
                    shelf.Add("FNumber", shelfCode);
                    stockLocObj.Add("FSTOCKLOCID__FF100001", shelf);
                    JObject layer = new JObject();
                    layer.Add("FNumber", layerCode);
                    stockLocObj.Add("FSTOCKLOCID__FF100002", layer);
                    JObject pos = new JObject();
                    pos.Add("FNumber", posCode);
                    stockLocObj.Add("FSTOCKLOCID__FF100003", pos);
                    entityObj.Add("FStockLocId", stockLocObj);

                    JObject lotObj = new JObject();
                    lotObj.Add("FNumber", lot);
                    entityObj.Add("FLot", lotObj);

                    entityObj.Add("FOwnerTypeID", "BD_OwnerOrg");
                    JObject ownerValueObj = new JObject();
                    ownerValueObj.Add("FNumber", GetOwner(orgId));
                    entityObj.Add("FOwnerID", ownerValueObj);

                    entityObj.Add("FSrcBillTypeId", "STK_InStock");
                    entityObj.Add("FSrcBillNo", billNo);

                    // ========== 构建关联关系Link表 ==========
                    JArray linkArray = new JArray();
                    JObject linkObj = new JObject();

                    // 转换规则
                    linkObj.Add("FEntity_Link_FRuleId", "38bf9dde-c38f-414b-b756-79931101f168");
                    // 源单表名（采购入库单单据体）
                    linkObj.Add("FEntity_Link_FSTableName", "T_STK_INSTOCKENTRY");
                    // 源单单据内码
                    linkObj.Add("FEntity_Link_FSBillId", srcFid);
                    // 源单分录内码
                    linkObj.Add("FEntity_Link_FSId", Convert.ToInt64(entryId));
                    // 原始携带量
                    linkObj.Add("FEntity_Link_FBaseQtyOld", qty);
                    // 修改携带量
                    linkObj.Add("FEntity_Link_FBaseQty", qty);

                    linkArray.Add(linkObj);
                    entityObj.Add("FEntity_Link", linkArray);

                    // ========== 构建子件表（挂在成品行下） ==========
                    JArray subEntityArray = new JArray();

                    // 1. 当前费用物料作为子件
                    if (!materialSet.Contains(wlnum))
                    {
                        materialSet.Add(wlnum);

                        JObject subObj = new JObject();
                        JObject subMaterialObj = new JObject();
                        subMaterialObj.Add("FNumber", wlnum);
                        subObj.Add("FMaterialIDSETY", subMaterialObj);
                        subObj.Add("FQtySETY", qty);

                        JObject subStockObj = new JObject();
                        subStockObj.Add("FNumber", stockCode);
                        subObj.Add("FStockIDSETY", subStockObj);

                        JObject subStockLocObj = new JObject();
                        JObject subShelf = new JObject();
                        subShelf.Add("FNumber", shelfCode);
                        subStockLocObj.Add("FSTOCKLOCIDSETY__FF100001", subShelf);
                        JObject subLayer = new JObject();
                        subLayer.Add("FNumber", layerCode);
                        subStockLocObj.Add("FSTOCKLOCIDSETY__FF100002", subLayer);
                        JObject subPos = new JObject();
                        subPos.Add("FNumber", posCode);
                        subStockLocObj.Add("FSTOCKLOCIDSETY__FF100003", subPos);
                        subObj.Add("FStockLocIdSETY", subStockLocObj);

                        JObject subLotObj = new JObject();
                        subLotObj.Add("FNumber", lot);
                        subObj.Add("FLotSETY", subLotObj);

                        subObj.Add("FOwnerTypeIDSETY", "BD_OwnerOrg");
                        JObject subOwnerValueObj = new JObject();
                        subOwnerValueObj.Add("FNumber", GetOwner(orgId));
                        subObj.Add("FOwnerIDSETY", subOwnerValueObj);

                        subEntityArray.Add(subObj);
                    }

                    // 2. 查询BOM其他子项
                    string bomSonSql = $@"
                    /*dialect*/
                    SELECT 
                        M1.FNUMBER AS BOMSON, FNUMERATOR AS FZ, FDENOMINATOR AS FM
                    FROM T_ENG_BOM A
                    JOIN T_ENG_BOMCHILD B ON A.FID = B.FID
                    JOIN T_BD_MATERIAL M ON A.FMATERIALID = M.FMATERIALID
                    JOIN T_BD_MATERIAL M1 ON B.FMATERIALID = M1.FMATERIALID
                    WHERE A.FNUMBER = '{bom}'
                    AND M1.FUSEORGID = {orgId}
                    AND M1.FNUMBER <> '{wlnum}'";

                    DynamicObjectCollection bomSonResult =
                        DBUtils.ExecuteDynamicObject(this.Context, bomSonSql);

                    // 3. 处理其他BOM子项
                    if (bomSonResult != null && bomSonResult.Count > 0)
                    {
                        for (int k = 0; k < bomSonResult.Count; k++)
                        {
                            string otherBomSon = Convert.ToString(bomSonResult[k]["BOMSON"]);
                            decimal fz = Convert.ToDecimal(bomSonResult[k]["FZ"]);
                            decimal fzEnd = Math.Round(fz, 3, MidpointRounding.AwayFromZero);
                            decimal fm = Convert.ToDecimal(bomSonResult[k]["FM"]);
                            decimal fmEnd = Math.Round(fm, 3, MidpointRounding.AwayFromZero);
                            //long otherQty = Convert.ToInt64(Math.Round(qty * (fzEnd / fmEnd), MidpointRounding.AwayFromZero));
                            decimal otherQty = qty * (fzEnd / fmEnd);

                            // 查询库存
                            string stockinventorySql = $@"
                            /*dialect*/
                            SELECT 
                                m.FNUMBER AS WLNUM,
                                lotStock.FNUMBER AS FLOT,
                                CASE 
                                    WHEN TSUB.FBASELOCKQTY IS NULL THEN a.FBASEQTY 
                                    ELSE a.FBASEQTY - TSUB.FBASELOCKQTY 
                                END AS KYL,
                                stockL.FName AS STOCK
                            FROM T_STK_INVENTORY a 
                            LEFT JOIN T_BD_LOTMASTER lotStock 
                                ON lotStock.FLOTID = a.FLOT 
                                AND lotStock.FMATERIALID = a.FMATERIALID 
                                AND a.FSTOCKORGID = lotStock.FUSEORGID 
                            LEFT JOIN (
                                SELECT 
                                    TLKE.FSUPPLYINTERID AS FINVENTRYID, 
                                    SUM(TLKE.FBASEQTY) AS FBASELOCKQTY,
                                    SUM(TLKE.FSECQTY) AS FSECLOCKQTY 
                                FROM T_PLN_RESERVELINKENTRY TLKE 
                                INNER JOIN T_PLN_RESERVELINK TLKH ON TLKE.FID = TLKH.FID
                                WHERE TLKE.FSUPPLYFORMID = 'STK_Inventory'  
                                    AND TLKE.FLINKTYPE = '4' 
                                GROUP BY TLKE.FSUPPLYINTERID
                            ) TSUB ON a.FID = TSUB.FINVENTRYID
                            INNER JOIN T_BD_MATERIAL m ON m.FMATERIALID = a.FMATERIALID
                            INNER JOIN T_BD_MATERIAL_L ml ON ml.FMATERIALID = m.FMATERIALID AND ml.FLOCALEID = 2052
                            INNER JOIN t_BD_StockStatus kczt ON kczt.FSTOCKSTATUSID = a.FSTOCKSTATUSID
                            INNER JOIN T_BD_STOCKSTATUS_L kcztL ON kcztL.FSTOCKSTATUSID = kczt.FSTOCKSTATUSID AND kcztL.FLOCALEID = 2052
                            INNER JOIN T_BD_UNIT_L baseUnit ON baseUnit.FUNITID = a.FBASEUNITID AND baseUnit.FLOCALEID = 2052
                            INNER JOIN T_BD_Stock_L stockL ON stockL.FSTOCKID = a.FSTOCKID AND stockL.FLOCALEID = 2052
                            WHERE a.FBASEQTY > 0 
                             AND (A.FSTOCKID = 165499 OR A.FSTOCKID = 180622)
                            AND M.FNUMBER = '{otherBomSon}'";

                            DynamicObjectCollection stockinventoryResult =
                                DBUtils.ExecuteDynamicObject(this.Context, stockinventorySql);

                            if (stockinventoryResult == null || stockinventoryResult.Count == 0)
                            {
                                continue;
                            }

                            // 批号匹配逻辑
                            bool lotHasBracket = false;
                            string matchLotContent = "";

                            if (lot.Contains("(") && lot.Contains(")"))
                            {
                                lotHasBracket = true;
                                int startIdx = lot.IndexOf("(");
                                int endIdx = lot.IndexOf(")");
                                if (startIdx < endIdx)
                                {
                                    matchLotContent = lot.Substring(startIdx + 1, endIdx - startIdx - 1);
                                }
                            }

                            decimal remainingQty = otherQty;

                            for (int m = 0; m < stockinventoryResult.Count; m++)
                            {
                                if (remainingQty <= 0) break;

                                string inventoryLot = Convert.ToString(stockinventoryResult[m]["FLOT"]);
                                long kyl = Convert.ToInt64(stockinventoryResult[m]["KYL"]);

                                // 判断批号是否匹配
                                bool isMatch = false;
                                if (lotHasBracket)
                                {
                                    if (inventoryLot.Contains("(") && inventoryLot.Contains(")"))
                                    {
                                        int invStart = inventoryLot.IndexOf("(");
                                        int invEnd = inventoryLot.IndexOf(")");
                                        if (invStart < invEnd)
                                        {
                                            string invBracketContent = inventoryLot.Substring(invStart + 1, invEnd - invStart - 1);
                                            if (invBracketContent == matchLotContent)
                                            {
                                                isMatch = true;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (inventoryLot == lot)
                                    {
                                        isMatch = true;
                                    }
                                }

                                if (!isMatch)
                                {
                                    hasSplit = true;
                                }

                                decimal fillQty = 0.00m;
                                if (kyl >= remainingQty)
                                {
                                    fillQty = remainingQty;
                                    remainingQty = 0;
                                }
                                else
                                {
                                    fillQty = kyl;
                                    remainingQty -= kyl;
                                }

                                if (fillQty > 0)
                                {
                                    string uniqueKey = otherBomSon + "|" + inventoryLot;
                                    if (!materialSet.Contains(uniqueKey))
                                    {
                                        materialSet.Add(uniqueKey);

                                        JObject subObj = new JObject();
                                        JObject subMaterialObj = new JObject();
                                        subMaterialObj.Add("FNumber", otherBomSon);
                                        subObj.Add("FMaterialIDSETY", subMaterialObj);
                                        subObj.Add("FQtySETY", fillQty);

                                        JObject subStockObj = new JObject();
                                        subStockObj.Add("FNumber", "7101");
                                        subObj.Add("FStockIDSETY", subStockObj);

                                        JObject subLotObj = new JObject();
                                        subLotObj.Add("FNumber", inventoryLot);
                                        subObj.Add("FLotSETY", subLotObj);

                                        subObj.Add("FOwnerTypeIDSETY", "BD_OwnerOrg");
                                        JObject subOwnerValueObj = new JObject();
                                        subOwnerValueObj.Add("FNumber", GetOwner(orgId));
                                        subObj.Add("FOwnerIDSETY", subOwnerValueObj);

                                        if (!isMatch)
                                        {
                                            decimal diffQty = otherQty - fillQty;

                                            string notes = $"拆分补录-物料:{otherBomSon}," +
                                                           $"需求数量:{otherQty.ToString("F3")}," +
                                                           $"补录批号:{inventoryLot}," +
                                                           $"补录数量:{fillQty.ToString("F3")}," +
                                                           $"源单批号:{lot}," +
                                                           $"源单物料:{wlnum}";
                                            subObj.Add("FNOTES", notes);
                                        }

                                        subEntityArray.Add(subObj);
                                    }
                                }
                            }
                        }
                    }

                    // 子件挂到当前成品行
                    entityObj.Add("FSubEntity", subEntityArray);
                    // 成品行加入数组
                    entityArray.Add(entityObj);
                }

                JObject saveObj = new JObject();
                JObject modelObj = new JObject();
                modelObj.Add("FID", targetFid);
                modelObj.Add("FEntity", entityArray);
                saveObj.Add("Model", modelObj);

                string saveJson = saveObj.ToString();
                var saveResult = client.Save("STK_AssembledApp", saveJson);

                JObject saveResultObj = JObject.Parse(saveResult);
                bool saveIsSuccess = saveResultObj["Result"]["ResponseStatus"]["IsSuccess"].Value<bool>();

                var result = new OperateResult();

                if (saveIsSuccess)
                {
                    string newBillNo =
                        saveResultObj["Result"]["Number"].ToString();

                    result.SuccessStatus = true;

                    result.Number =
                        ObjectUtils.Object2String(
                            this.BusinessInfo.GetBillNoField()
                            .DynamicProperty
                            .GetValueFast(e.DataEntitys[0]));

                    result.Message = string.Format(
                        "单据【{0}】自动生成组装拆卸单成功，单号：{1}",
                        result.Number,
                        newBillNo);

                    // 第三条消息
                    string extraMsg =
                        string.Format(
                            "单据【{0}】自动生成组装拆卸单执行完成,单号：{1}",
                            result.Number, newBillNo);

                    if (hasSplit)
                    {
                        _hasSplit = true;
                        extraMsg += "，批号存在拆分，请核实！";
                    }

                    _extraMessages.Add(extraMsg);
                }
                else
                {
                    JArray errors =
                        saveResultObj["Result"]["ResponseStatus"]["Errors"]
                        as JArray;

                    string errorMsg =
                        errors != null && errors.Count > 0
                        ? errors[0]["Message"].ToString()
                        : "未知错误";

                    result.SuccessStatus = false;

                    result.Message = string.Format(
                        "单据【{0}】自动生成组装拆卸单失败，原因：{1}",
                        result.Number,
                        errorMsg);

                    // 第三条消息
                    _extraMessages.Add(
                        string.Format(
                            "单据【{0}】自动生成组装拆卸单执行失败，请检查后重试！原因：{1}",
                            result.Number, errorMsg));
                }

                this.OperationResult.IsShowMessage = true;
                this.OperationResult.OperateResult.Add(result);

            }

            // 所有业务处理完成后，统一追加第三条消息
            foreach (string msg in _extraMessages)
            {
                var extraResult = new OperateResult();

                extraResult.SuccessStatus = true;

                extraResult.PKValue = e.DataEntitys[0]["Id"];

                extraResult.Number =
                    ObjectUtils.Object2String(
                        this.BusinessInfo.GetBillNoField()
                        .DynamicProperty
                        .GetValueFast(e.DataEntitys[0]));

                extraResult.Message = msg;

                this.OperationResult.OperateResult.Add(extraResult);
            }
        }

        private string GetOwner(long orgId)
        {
            Dictionary<long, string> dic = new Dictionary<long, string>()
            {
                { 1, "100" },
                { 101006, "101" }
            };
            return dic.ContainsKey(orgId) ? dic[orgId] : "";
        }
    }
}