using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Zitn_PUR_ShowFrom_Submit_Plugin
{
    [Description("【单据插件】点击【替代料情况】按钮,重新计算并填充数据信息")]
    [HotUpdate]
    public class SubstituteMaterialPlugin : AbstractBillPlugIn
    {
        public override void ButtonClick(ButtonClickEventArgs e)
        {
            base.ButtonClick(e);

            if (!string.Equals(e.Key, "F_ZMER_Button_0q2", StringComparison.OrdinalIgnoreCase))
                return;
            string flag = this.Model.GetValue("F_ZMER_SIMILAR_FLAG")?.ToString();

            if (flag == "YY")
            {
                ClearSimilarMaterial();

                ReCalcPriceListSummary();
                ReCalcInquirySummary();
                ReCalcHistorySummary();

                this.Model.SetValue("F_ZMER_SIMILAR_FLAG", "NN");
                this.View.GetControl<Button>("F_ZMER_Button_0q2").Text = "考虑替代物料";
                return;
            }

            //var materialCode = this.Model.GetValue("F_ZMER_TEXT_2BG")?.ToString();
            var materialCode = "105001259";
            ReCalculateAndFill(materialCode);

            ReCalcPriceListSummary();
            ReCalcInquirySummary();
            ReCalcHistorySummary();

            this.Model.SetValue("F_ZMER_SIMILAR_FLAG", "YY");
            this.View.GetControl<Button>("F_ZMER_Button_0q2").Text = "已考虑替代物料";

        }

        private void ReCalcPriceListSummary()
        {
            string entityKey = "F_ZMER_Entity_qhl";

            int rowCount = this.Model.GetEntryRowCount(entityKey);
            if (rowCount <= 0) return;

            decimal minPrice = decimal.MaxValue;
            decimal maxPrice = decimal.MinValue;
            bool hasValue = false;

            for (int i = 0; i < rowCount; i++)
            {
                var priceObj = this.Model.GetValue("F_ZMER_TEXT_WT6", i); // 含税单价
                decimal price;
                if (decimal.TryParse(priceObj?.ToString(), out price))
                {
                    hasValue = true;
                    if (price < minPrice) minPrice = price;
                    if (price > maxPrice) maxPrice = price;
                }
            }

            if (!hasValue) return;

            int summaryRow = this.Model.GetEntryRowCount(entityKey);
            this.Model.CreateNewEntryRow(entityKey);

            this.Model.SetValue("F_ZMER_TEXT_ACP", "小计", summaryRow);
            this.Model.SetValue("F_ZMER_TEXT_APV", "最低价格", summaryRow);
            this.Model.SetValue("F_ZMER_TEXT_TNE", FormatDecimal(minPrice, 2), summaryRow);
            this.Model.SetValue("F_ZMER_TEXT_CX3", "最高价格", summaryRow);
            this.Model.SetValue("F_ZMER_TEXT_V8S", FormatDecimal(maxPrice, 2), summaryRow);
            SetRowBackColorByFirstColumn(
                "F_ZMER_Entity_qhl",
                "F_ZMER_TEXT_ACP",
                "小计",
                "#FFE699"
            );
        }

        private void SetRowBackColorByFirstColumn(
            string entityKey,
            string firstColumnFieldKey,
            string matchText,
            string color)
        {
            var grid = this.View.GetControl<EntryGrid>(entityKey);
            if (grid == null)
                return;

            int rowCount = this.Model.GetEntryRowCount(entityKey);
            if (rowCount <= 0)
                return;

            var colors = new List<KeyValuePair<int, string>>();

            for (int row = 0; row < rowCount; row++)
            {
                var value = this.Model.GetValue(firstColumnFieldKey, row);
                string text = value == null ? string.Empty : value.ToString();

                if (text == matchText)
                {
                    colors.Add(new KeyValuePair<int, string>(row, color));
                }
            }

            if (colors.Count > 0)
            {
                grid.SetRowBackcolor(colors);
            }
        }

        private void ReCalcInquirySummary()
        {
            string entityKey = "F_ZMER_Entity_9ra";
            int rowCount = this.Model.GetEntryRowCount(entityKey);
            if (rowCount <= 0) return;

            decimal minPrice = decimal.MaxValue;
            decimal maxPrice = decimal.MinValue;
            bool hasValue = false;

            for (int i = 0; i < rowCount; i++)
            {
                var priceObj = this.Model.GetValue("F_ZMER_TEXT_GVY", i);
                decimal price;
                if (decimal.TryParse(priceObj?.ToString(), out price))
                {
                    hasValue = true;
                    if (price < minPrice) minPrice = price;
                    if (price > maxPrice) maxPrice = price;
                }
            }

            if (!hasValue) return;

            int summaryRow = this.Model.GetEntryRowCount(entityKey);
            this.Model.CreateNewEntryRow(entityKey);

            this.Model.SetValue("F_ZMER_TEXT_N1V", "小计", summaryRow);
            this.Model.SetValue("F_ZMER_Text_83g", "最低报价", summaryRow);
            this.Model.SetValue("F_ZMER_Text_9id", FormatDecimal(minPrice, 2), summaryRow);
            this.Model.SetValue("F_ZMER_Text_st2", "最高报价", summaryRow);
            this.Model.SetValue("F_ZMER_Text_b3r", FormatDecimal(maxPrice, 2), summaryRow);
            SetRowBackColorByFirstColumn(
                "F_ZMER_Entity_9ra",
                "F_ZMER_TEXT_N1V",
                "小计",
                "#FFE699"
            );
        }

        private void ReCalcHistorySummary()
        {
            string entityKey = "F_ZMER_Entity_r2z";
            int rowCount = this.Model.GetEntryRowCount(entityKey);
            if (rowCount <= 0) return;

            decimal totalQty = 0m;
            decimal totalAmount = 0m;

            for (int i = 0; i < rowCount; i++)
            {
                decimal qty, amount;

                if (decimal.TryParse(this.Model.GetValue("F_ZMER_TEXT_4R6", i)?.ToString(), out qty))
                    totalQty += qty;

                if (decimal.TryParse(this.Model.GetValue("F_ZMER_TEXT_RE5", i)?.ToString(), out amount))
                    totalAmount += amount;
            }

            if (totalQty <= 0) return;

            decimal avgPrice = totalAmount / totalQty;

            int summaryRow = this.Model.GetEntryRowCount(entityKey);
            this.Model.CreateNewEntryRow(entityKey);

            this.Model.SetValue("F_ZMER_TEXT_1LD", "小计", summaryRow);
            this.Model.SetValue("F_ZMER_TEXT_KV3", "总采购数量", summaryRow);
            this.Model.SetValue("F_ZMER_TEXT_36S", FormatDecimal(totalQty, 2), summaryRow);
            this.Model.SetValue("F_ZMER_TEXT_LGH", "平均采购价格", summaryRow);
            this.Model.SetValue("F_ZMER_TEXT_4R6", FormatDecimal(avgPrice, 2), summaryRow);
            SetRowBackColorByFirstColumn(
                "F_ZMER_Entity_r2z",
                "F_ZMER_TEXT_1LD",
                "小计",
                "#FFE699"
            );
        }

        private void ClearSimilarMaterial()
        {
            ClearEntryRowsByFirstColumn(
                "F_ZMER_Entity_r2z",
                "F_ZMER_TEXT_1LD",
                "替代料历史采购信息"
            );

            ClearEntryRowsByFirstColumn(
                "F_ZMER_Entity_9ra",
                "F_ZMER_TEXT_N1V",
                "替代料采购询价记录"
            );

            ClearEntryRowsByFirstColumn(
                "F_ZMER_Entity_qhl",
                "F_ZMER_TEXT_ACP",
                "替代料采购价目信息"
            );
        }

        private void ClearEntryRowsByFirstColumn(string entityKey, string firstColumnFieldKey, string matchText)
        {
            int rowCount = this.Model.GetEntryRowCount(entityKey);

            /*int lastRowIndex = rowCount - 1;
            this.Model.DeleteEntryRow(entityKey, lastRowIndex);*/
            if (rowCount <= 0)
                return;

            for (int row = rowCount - 1; row >= 0; row--)
            {
                var value = this.Model.GetValue(firstColumnFieldKey, row);
                string text = value == null ? string.Empty : value.ToString();

                if (text == matchText || text == "小计")
                {
                    this.Model.DeleteEntryRow(entityKey, row);
                }
            }
        }

        #region 主逻辑

        private void ReCalculateAndFill(string materialCode)
        {
            if (materialCode.IsNullOrEmptyOrWhiteSpace())
            {
                this.View.ShowErrMessage("物料编码不能为空！");
                return;
            }

            //WHERE 子项物料编码 <> '{0}'
            string sql = string.Format(
                "/*dialect*/SELECT DISTINCT 子项物料编码 FROM dbo.FN_BOM_ReplaceGroup_BySubMaterial('{0}') ",
                materialCode);

            DynamicObjectCollection result = DBUtils.ExecuteDynamicObject(this.Context, sql);
            if (result == null || result.Count == 0)
                return;

            foreach (DynamicObject row in result)
            {
                string subMaterialCode = row["子项物料编码"]?.ToString();
                if (subMaterialCode.IsNullOrEmptyOrWhiteSpace())
                    continue;

                FillPriceList(subMaterialCode);
                FillInquiry(subMaterialCode);
                FillHistory(subMaterialCode);
            }
        }

        #endregion

        #region 采购价目

        private void FillPriceList(string materialCode)
        {
            string entityKey = "F_ZMER_Entity_qhl";
            string titleField = "F_ZMER_TEXT_ACP";

            RemoveSummaryRow(entityKey, titleField);
            int startRow = this.Model.GetEntryRowCount(entityKey);

            string sql = $@"
                SELECT
                    D.FNUMBER,
                    F.FNUMBER AS MaterialNumber,
                    G.FNAME AS MaterialName,
                    G.FSPECIFICATION,
                    C.FPRICE,
                    C.FTAXPRICE,
                    C.FTAXRATE,
                    C.FUPPRICE,
                    C.FDOWNPRICE,
                    C.FEFFECTIVEDATE,
                    C.FEXPIRYDATE 
                FROM t_PUR_PriceListEntry C
                JOIN t_PUR_PriceList D ON C.FID = D.FID
                JOIN T_BD_MATERIAL F ON C.FMATERIALID = F.FMATERIALID
                JOIN T_BD_MATERIAL_L G ON F.FMATERIALID = G.FMATERIALID
                WHERE F.FNUMBER = '{materialCode}'";

            var rows = DBUtils.ExecuteDynamicObject(this.Context, sql);
            if (rows == null || rows.Count == 0) return;

            for (int i = 0; i < rows.Count; i++)
            {
                int rowIndex = startRow + i;
                this.Model.CreateNewEntryRow(entityKey);

                this.Model.SetValue(titleField, "替代料采购价目信息", rowIndex);
                this.Model.SetValue("F_ZMER_TEXT_APV", rows[i]["FNUMBER"], rowIndex);
                this.Model.SetValue("F_ZMER_TEXT_TNE", rows[i]["MaterialNumber"], rowIndex);
                this.Model.SetValue("F_ZMER_TEXT_CX3", rows[i]["MaterialName"], rowIndex);
                this.Model.SetValue("F_ZMER_TEXT_V8S", rows[i]["FSPECIFICATION"], rowIndex);
                this.Model.SetValue("F_ZMER_TEXT_WT6", FormatDecimal(rows[i]["FTAXPRICE"]), rowIndex);
                this.Model.SetValue("F_ZMER_TEXT_YEL", FormatDecimal(rows[i]["FUPPRICE"]), rowIndex);
                this.Model.SetValue("F_ZMER_TEXT_HPA", FormatDecimal(rows[i]["FDOWNPRICE"]), rowIndex);
                this.Model.SetValue("F_ZMER_TEXT_ZZZ", FormatDate(rows[i]["FEFFECTIVEDATE"]), rowIndex);
                this.Model.SetValue("F_ZMER_TEXT_IAO", FormatDate(rows[i]["FEXPIRYDATE"]), rowIndex);
            }

            SetRowBackColor(entityKey, titleField, "替代料采购价目信息");
        }

        #endregion

        #region 采购询价

        private void FillInquiry(string materialCode)
        {
            string entityKey = "F_ZMER_Entity_9ra";
            string titleField = "F_ZMER_TEXT_N1V";

            RemoveSummaryRow(entityKey, titleField);
            int startRow = this.Model.GetEntryRowCount(entityKey);

            string sql = $@"
                                SELECT
                                      A.FBILLNO,
                                      A.F_ZMER_DATE_EMQ AS InquiryDate,
                                    CASE
                                        WHEN A.F_ZMER_ORGID_VB1 = '1' THEN
                                        '青岛智腾科技有限公司' 
                                        WHEN A.F_ZMER_ORGID_VB1 = '101006' THEN
                                        '青岛智腾微电子有限公司' 
                                        WHEN A.F_ZMER_ORGID_VB1 = '101007' THEN
                                        '青岛智腾电源有限公司' 
                                        WHEN A.F_ZMER_ORGID_VB1 = '101050' THEN
                                        'test' 
                                        WHEN A.F_ZMER_ORGID_VB1 = '1404303' THEN
                                        '青岛智腾烽行能源有限公司' 
                                        WHEN A.F_ZMER_ORGID_VB1 = '1516310' THEN
                                        '青岛晶英电子科技有限公司' 
                                        WHEN A.F_ZMER_ORGID_VB1 = '3149866' THEN
                                        '青岛智腾微电子有限公司北京分公司' 
                                        WHEN A.F_ZMER_ORGID_VB1 = '3241152' THEN
                                        '青岛加速度智能科技有限公司' 
                                        WHEN A.F_ZMER_ORGID_VB1 = '4032930' THEN
                                        '青岛智腾微电子有限公司西安分公司' 
                                        WHEN A.F_ZMER_ORGID_VB1 = '4665868' THEN
                                        '青岛智导电子有限公司' 
                                        WHEN A.F_ZMER_ORGID_VB1 = '4665869' THEN
                                        '青岛深科睿探技术有限公司' 
                                        WHEN A.F_ZMER_ORGID_VB1 = '4852744' THEN
                                        '青岛智导电子有限公司北京分公司' 
                                      END AS PurchaseOrg,
                                      C.FNUMBER AS MaterialCode,
                                      D.FNAME AS MaterialName,
                                      D.FSPECIFICATION AS Specification,
                                      F.FNAME AS Unit,
                                      B.F_ZMER_Qty_yrr AS Qty,
                                      B.F_ZMER_Datetime_h1g AS ArrivalDate,
                                      B.F_ZMER_Price_k79 AS Price,
                                      B.Fnotes AS Remark,
                                      E.FNAME AS SupplierName 
                                    FROM
                                      ZMER_t_Cust100019 A
                                      JOIN ZMER_t_Cust_Entry100080 B ON A.FID = B.FID
                                      JOIN T_BD_MATERIAL C ON B.FMATERIALID = C.FMATERIALID
                                      JOIN T_BD_MATERIAL_L D ON C.FMATERIALID = D.FMATERIALID
                                      JOIN T_BD_SUPPLIER_L E ON B.FSUPPLIERID = E.FSUPPLIERID
                                      JOIN T_BD_UNIT_L F ON B.F_ZMER_UnitID_fg2 = F.FUNITID
                                      JOIN V_BD_BUYER_L G ON A.F_ZMER_Base_xwf = G.FID 
                                    WHERE
                                      C.FNUMBER = '{materialCode}'
                                      AND A.FDOCUMENTSTATUS = 'C'";

            var rows = DBUtils.ExecuteDynamicObject(this.Context, sql);
            if (rows == null || rows.Count == 0) return;

            for (int i = 0; i < rows.Count; i++)
            {
                int rowIndex = startRow + i;
                this.Model.CreateNewEntryRow(entityKey);

                this.Model.SetValue(titleField, "替代料采购询价记录", rowIndex);
                this.Model.SetValue("F_ZMER_TEXT_83G", rows[i]["FBillno"], rowIndex);
                this.Model.SetValue("F_ZMER_TEXT_6CK", FormatDate(rows[i]["InquiryDate"]), rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_PMA",
                    rows[i]["PurchaseOrg"]?.ToString(),
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_7XZ",
                    rows[i]["SupplierName"]?.ToString(),
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_9ID",
                    rows[i]["MaterialCode"]?.ToString(),
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_ST2",
                    rows[i]["MaterialName"]?.ToString(),
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_B3R",
                    rows[i]["Specification"]?.ToString(),
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_UEH",
                    rows[i]["Unit"]?.ToString(),
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_CO6",
                    FormatDecimal(rows[i]["Qty"], 2),
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_VZV",
                    FormatDate(rows[i]["ArrivalDate"]),
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_GVY",
                    FormatDecimal(rows[i]["Price"], 2),
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_Y5O",
                    rows[i]["Remark"]?.ToString(),
                    rowIndex);
            }

            SetRowBackColor(entityKey, titleField, "替代料采购询价记录");
        }

        #endregion

        #region 历史采购

        private void FillHistory(string materialCode)
        {
            string entityKey = "F_ZMER_Entity_r2z";
            string titleField = "F_ZMER_TEXT_1LD";

            RemoveSummaryRow(entityKey, titleField);
            int startRow = this.Model.GetEntryRowCount(entityKey);

            string sql = $@"
                SELECT 
                    A.FBillno,
                    A.FCreateDate,
                    B.Fqty,
                    D.FTAXPRICE,
                    E.FNAME AS SupplierName,FALLAMOUNT 
                FROM T_PUR_POORDER A
                JOIN T_PUR_POORDERENTRY B ON A.FID = B.FID
                JOIN T_BD_MATERIAL C ON B.FMATERIALID = C.FMATERIALID
                JOIN T_PUR_POORDERENTRY_F D ON D.FENTRYID = B.FENTRYID
                JOIN T_BD_SUPPLIER_L E ON A.FSUPPLIERID = E.FSUPPLIERID
                WHERE C.FNUMBER = '{materialCode}'
                  AND A.FDOCUMENTSTATUS = 'C'";

            var rows = DBUtils.ExecuteDynamicObject(this.Context, sql);
            if (rows == null || rows.Count == 0) return;

            for (int i = 0; i < rows.Count; i++)
            {
                int rowIndex = startRow + i;
                this.Model.CreateNewEntryRow(entityKey);

                this.Model.SetValue(titleField, "替代料历史采购信息", rowIndex);
                this.Model.SetValue("F_ZMER_TEXT_KV3", rows[i]["FBillno"], rowIndex);
                this.Model.SetValue("F_ZMER_TEXT_36S", FormatDate(rows[i]["FCreateDate"]), rowIndex);
                this.Model.SetValue("F_ZMER_TEXT_LGH", rows[i]["SupplierName"], rowIndex);
                this.Model.SetValue("F_ZMER_TEXT_4R6", FormatDecimal(rows[i]["Fqty"]), rowIndex);
                this.Model.SetValue("F_ZMER_TEXT_HGD", FormatDecimal(rows[i]["FTAXPRICE"]), rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_RE5",
                    FormatDecimal(rows[i]["FALLAMOUNT"], 2),
                    rowIndex);
            }

            SetRowBackColor(entityKey, titleField, "替代料历史采购信息");
        }

        #endregion

        #region 公共方法

        private void RemoveSummaryRow(string entityKey, string titleField)
        {
            int rowCount = this.Model.GetEntryRowCount(entityKey);
            if (rowCount <= 0) return;

            var lastValue = this.Model.GetValue(titleField, rowCount - 1)?.ToString();
            if (lastValue == "小计")
            {
                this.Model.DeleteEntryRow(entityKey, rowCount - 1);
            }
        }

        private void SetRowBackColor(string entityKey, string fieldKey, string matchText)
        {
            var grid = this.View.GetControl<EntryGrid>(entityKey);
            if (grid == null) return;

            var colors = new List<KeyValuePair<int, string>>();
            int rowCount = this.Model.GetEntryRowCount(entityKey);

            for (int i = 0; i < rowCount; i++)
            {
                var value = this.Model.GetValue(fieldKey, i)?.ToString();
                if (value == matchText)
                {
                    colors.Add(new KeyValuePair<int, string>(i, "#8CB4FF"));
                }
            }

            if (colors.Count > 0)
                grid.SetRowBackcolor(colors);
        }

        private string FormatDate(object value)
        {
            DateTime dt;
            return value != null && DateTime.TryParse(value.ToString(), out dt)
                ? dt.ToString("yyyy-MM-dd")
                : string.Empty;
        }

        private string FormatDecimal(object value, int scale = 2)
        {
            decimal d;
            return value != null && decimal.TryParse(value.ToString(), out d)
                ? d.ToString("F" + scale)
                : string.Empty;
        }

        #endregion
    }
}
