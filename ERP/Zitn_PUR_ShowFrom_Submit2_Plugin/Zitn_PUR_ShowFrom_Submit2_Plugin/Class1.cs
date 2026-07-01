using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Zitn_PUR_ShowFrom_Submit2_Plugin
{
    [Description("【单据插件】点击【相似物料情况】按钮,重新计算并填充数据信息")]
    [HotUpdate]
    public class Class1 : AbstractBillPlugIn
    {

        public override void ButtonClick(ButtonClickEventArgs e)
        {
            base.ButtonClick(e);

            if (!string.Equals(e.Key, "F_ZMER_Button_0q1", System.StringComparison.OrdinalIgnoreCase))
                return;

            string flag = this.Model.GetValue("F_ZMER_SIMILAR_FLAG")?.ToString();

            if (flag == "Y")
            {
                ClearSimilarMaterial();

                ReCalcPriceListSummary();
                ReCalcInquirySummary();
                ReCalcHistorySummary();

                this.Model.SetValue("F_ZMER_SIMILAR_FLAG", "N");
                this.View.GetControl<Button>("F_ZMER_Button_0q1").Text = "考虑相似物料";
                return;
            }

            var wlnum = this.Model.GetValue("F_ZMER_TEXT_2BG")?.ToString();

            ReCalculateAndFill(wlnum);

            ReCalcPriceListSummary();
            ReCalcInquirySummary();
            ReCalcHistorySummary();


            this.Model.SetValue("F_ZMER_SIMILAR_FLAG", "Y");
            this.View.GetControl<Button>("F_ZMER_Button_0q1").Text = "已考虑相似物料";


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
                "#FFFF00"
            );
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
                "#FFFF00"
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
                "#FFFF00"
            );
        }


        private void ClearSimilarMaterial()
        {
            ClearEntryRowsByFirstColumn(
                "F_ZMER_Entity_r2z",
                "F_ZMER_TEXT_1LD",
                "相似物料历史采购信息"
            );
            

            ClearEntryRowsByFirstColumn(
                "F_ZMER_Entity_9ra",
                "F_ZMER_TEXT_N1V",
                "相似物料采购询价记录"
            );


            ClearEntryRowsByFirstColumn(
                "F_ZMER_Entity_qhl",
                "F_ZMER_TEXT_ACP",
                "相似物料采购价目信息"
            );
        }

        private void ClearEntryRowsByFirstColumn(string entityKey,string firstColumnFieldKey,string matchText)
        {
            int rowCount = this.Model.GetEntryRowCount(entityKey);
            int lastRowIndex = rowCount - 1;
            this.Model.DeleteEntryRow(entityKey, lastRowIndex);
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

        #region 侏罗纪
        private void ReCalculateAndFill(string wlnum)
        {
            if (wlnum.IsNullOrEmptyOrWhiteSpace())
            {
                this.View.ShowErrMessage("物料编码不能为空！");
                return;
            }
            else
            {
                var namequery = string.Format(@"
                                               SELECT 
                                                    DISTINCT FNAME
                                                FROM
                                                    T_BD_MATERIAL A
                                                    JOIN T_BD_MATERIAL_L B ON A.FMATERIALID = B.FMATERIALID
                                                WHERE
                                                    FNUMBER = '" + wlnum + "'");
                DynamicObjectCollection nameresult = DBUtils.ExecuteDynamicObject(this.Context, namequery);
                if (nameresult != null && nameresult.Count > 0)
                {
                    var wlname = nameresult[0]["FNAME"].ToString();

                    var query = string.Format(@"
                                        SELECT
	                                        DISTINCT FNUMBER 
                                        FROM
	                                        T_BD_MATERIAL A
	                                        JOIN T_BD_MATERIAL_L B ON A.FMATERIALID = B.FMATERIALID 
                                        WHERE
	                                        FNAME = '" + wlname + "' AND FNUMBER <> '" + wlnum + "'");

                    DynamicObjectCollection result = DBUtils.ExecuteDynamicObject(this.Context, query);
                    if (result != null && result.Count > 0)
                    {
                        for (int i = 0; i < result.Count; i++)
                        {
                            var materialCode = result[i]["FNUMBER"].ToString();

                            FillPriceList(materialCode);

                            FillInquiry(materialCode);

                            FillHistory(materialCode);
                        }
                    }
                }
            }
        }
        #endregion

        #region 历史采购
        private void FillHistory(string materialCode)
        {
            string entityKey = "F_ZMER_Entity_r2z";
            string titleField = "F_ZMER_TEXT_1LD";

            RemoveSummaryRow(entityKey, titleField);

            int startRowIndex = this.Model.GetEntryRowCount(entityKey);

            string sql = @"
                            SELECT 
                                A.FBillno,
                                A.FCreateDate,
                                B.Fqty          AS Qty,
                                E.FNAME         AS SupplierName,
                                D.FTAXPRICE     AS Price,
                                FALLAMOUNT              
                            FROM T_PUR_POORDER A
                            JOIN T_PUR_POORDERENTRY B ON A.FID = B.FID
                            JOIN T_BD_MATERIAL C ON B.FMATERIALID = C.FMATERIALID
                            JOIN T_PUR_POORDERENTRY_F D ON D.FENTRYID = B.FENTRYID
                            JOIN T_BD_SUPPLIER_L E ON A.FSUPPLIERID = E.FSUPPLIERID
                            WHERE C.FNUMBER = '" + materialCode + @"'
                              AND A.FDOCUMENTSTATUS = 'C'";

            DynamicObjectCollection rows = DBUtils.ExecuteDynamicObject(this.Context, sql);

            decimal totalQty = 0;
            decimal totalAmount = 0;
            int count = 0;

            for (int i = 0; i < rows.Count; i++)
            {
                int rowIndex = startRowIndex + i;

                this.Model.CreateNewEntryRow(entityKey);

                this.Model.SetValue(
                    "F_ZMER_TEXT_1LD",
                    "相似物料历史采购信息",
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_KV3",
                    rows[i]["FBillno"]?.ToString(),
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_36S",
                    FormatDate(rows[i]["FCreateDate"]),
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_LGH",
                    rows[i]["SupplierName"]?.ToString(),
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_4R6",
                    FormatDecimal(rows[i]["Qty"], 2),
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_HGD",
                    FormatDecimal(rows[i]["Price"], 2),
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_RE5",
                    FormatDecimal(rows[i]["FALLAMOUNT"], 2),
                    rowIndex);

                decimal qty = 0m;
                decimal amount = 0m;

                if (decimal.TryParse(rows[i]["Qty"]?.ToString(), out qty))
                {
                    totalQty += qty;
                }

                if (decimal.TryParse(rows[i]["FALLAMOUNT"]?.ToString(), out amount))
                {
                    totalAmount += amount;
                }

                count++;
            }
            SetRowBackColorByFirstColumn(
                "F_ZMER_Entity_r2z",
                "F_ZMER_TEXT_1LD",
                "相似物料历史采购信息",
                "#DCEBFF"
            );
        }
        #endregion

        #region 采购询价
        private void FillInquiry(string materialCode)
        {
            string entityKey = "F_ZMER_Entity_9ra";
            string titleField = "F_ZMER_TEXT_N1V";

            RemoveSummaryRow(entityKey, titleField);
            int startRowIndex = this.Model.GetEntryRowCount(entityKey);

            string sql = @"
                            SELECT A.FBILLNO,
                                A.F_ZMER_DATE_EMQ        AS InquiryDate,
                                CASE
                                    WHEN A.F_ZMER_ORGID_VB1 = '1' THEN '青岛智腾科技有限公司'
                                    WHEN A.F_ZMER_ORGID_VB1 = '101006' THEN '青岛智腾微电子有限公司'
                                    WHEN A.F_ZMER_ORGID_VB1 = '101007' THEN '青岛智腾电源有限公司'
                                    WHEN A.F_ZMER_ORGID_VB1 = '101050' THEN 'test'
                                    WHEN A.F_ZMER_ORGID_VB1 = '1404303' THEN '青岛智腾烽行能源有限公司'
                                    WHEN A.F_ZMER_ORGID_VB1 = '1516310' THEN '青岛晶英电子科技有限公司'
                                    WHEN A.F_ZMER_ORGID_VB1 = '3149866' THEN '青岛智腾微电子有限公司北京分公司'
                                    WHEN A.F_ZMER_ORGID_VB1 = '3241152' THEN '青岛加速度智能科技有限公司'
                                    WHEN A.F_ZMER_ORGID_VB1 = '4032930' THEN '青岛智腾微电子有限公司西安分公司'
                                    WHEN A.F_ZMER_ORGID_VB1 = '4665868' THEN '青岛智导电子有限公司'
                                    WHEN A.F_ZMER_ORGID_VB1 = '4665869' THEN '青岛深科睿探技术有限公司'
                                    WHEN A.F_ZMER_ORGID_VB1 = '4852744' THEN '青岛智导电子有限公司北京分公司'
                                END                       AS PurchaseOrg,
                                C.FNUMBER                AS MaterialCode,
                                D.FNAME                  AS MaterialName,
                                D.FSPECIFICATION         AS Specification,
                                F.FNAME                  AS Unit,
                                B.F_ZMER_Qty_yrr         AS Qty,
                                B.F_ZMER_Datetime_h1g    AS ArrivalDate,
                                B.F_ZMER_Price_k79       AS Price,
                                B.Fnotes                 AS Remark,
                                E.FNAME                  AS SupplierName
                            FROM ZMER_t_Cust100019 A
                            JOIN ZMER_t_Cust_Entry100080 B ON A.FID = B.FID
                            JOIN T_BD_MATERIAL C ON B.FMATERIALID = C.FMATERIALID
                            JOIN T_BD_MATERIAL_L D ON C.FMATERIALID = D.FMATERIALID  
                            JOIN T_BD_SUPPLIER_L E ON B.FSUPPLIERID = E.FSUPPLIERID
                            JOIN T_BD_UNIT_L F ON B.F_ZMER_UnitID_fg2 = F.FUNITID
                            JOIN V_BD_BUYER_L G ON A.F_ZMER_Base_xwf = G.FID
                            WHERE C.FNUMBER = '" + materialCode + @"'
                              AND A.FDOCUMENTSTATUS = 'C'";
            DynamicObjectCollection rows = DBUtils.ExecuteDynamicObject(this.Context, sql);

            if (rows == null || rows.Count == 0)
                return;

            decimal minPrice = decimal.MaxValue;
            decimal maxPrice = decimal.MinValue;
            int inquiryCount = 0;

            for (int j = 0; j < rows.Count; j++)
            {
                int rowIndex = startRowIndex + j;

                this.Model.CreateNewEntryRow(entityKey);

                this.Model.SetValue(
                    "F_ZMER_TEXT_N1V",
                    "相似物料采购询价记录",
                    rowIndex);
                this.Model.SetValue("F_ZMER_TEXT_83G", rows[j]["FBillno"], rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_6CK",
                    FormatDate(rows[j]["InquiryDate"]),
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_PMA",
                    rows[j]["PurchaseOrg"]?.ToString(),
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_7XZ",
                    rows[j]["SupplierName"]?.ToString(),
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_9ID",
                    rows[j]["MaterialCode"]?.ToString(),
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_ST2",
                    rows[j]["MaterialName"]?.ToString(),
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_B3R",
                    rows[j]["Specification"]?.ToString(),
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_UEH",
                    rows[j]["Unit"]?.ToString(),
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_CO6",
                    FormatDecimal(rows[j]["Qty"], 2),
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_VZV",
                    FormatDate(rows[j]["ArrivalDate"]),
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_GVY",
                    FormatDecimal(rows[j]["Price"], 2),
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_Y5O",
                    rows[j]["Remark"]?.ToString(),
                    rowIndex);

                inquiryCount++;
                decimal price;
                if (decimal.TryParse(rows[j]["Price"]?.ToString(), out price))
                {
                    if (price < minPrice) minPrice = price;
                    if (price > maxPrice) maxPrice = price;
                }
            }
            SetRowBackColorByFirstColumn(
                "F_ZMER_Entity_9ra",
                "F_ZMER_TEXT_N1V",
                "相似物料采购询价记录",
                "#DCEBFF"
            );
        }
        #endregion

        #region 采购价目
        private void FillPriceList(string materialCode)
        {
            string entityKey = "F_ZMER_Entity_qhl";
            string titleField = "F_ZMER_TEXT_ACP";

            RemoveSummaryRow(entityKey, titleField);

            int startRowIndex = this.Model.GetEntryRowCount(entityKey);

            string sql = @"
                            SELECT
                              D.FNUMBER,
                              E.FNAME,
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
                              LEFT JOIN t_PUR_PriceList D ON C.FID = D.FID
                              LEFT JOIN T_BD_SUPPLIER_L E ON D.FSUPPLIERID = E.FSUPPLIERID
                              LEFT JOIN T_BD_MATERIAL F ON C.FMATERIALID = F.FMATERIALID
                              LEFT JOIN T_BD_MATERIAL_L G ON F.FMATERIALID = G.FMATERIALID 
                            WHERE F.FNUMBER = '" + materialCode + "'";

            DynamicObjectCollection rows = DBUtils.ExecuteDynamicObject(this.Context, sql);

            if (rows == null || rows.Count == 0)
                return;

            decimal minPrice = decimal.MaxValue;
            decimal maxPrice = decimal.MinValue;


            for (int j = 0; j < rows.Count; j++)
            {
                int rowIndex = startRowIndex + j;

                this.Model.CreateNewEntryRow(entityKey);

                this.Model.SetValue(
                    "F_ZMER_TEXT_ACP", 
                    "相似物料采购价目信息", 
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_APV", 
                    rows[j]["FNUMBER"]?.ToString(),
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_TNE", 
                    rows[j]["MaterialNumber"], 
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_CX3", 
                    rows[j]["MaterialName"], 
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_V8S", 
                    rows[j]["FSPECIFICATION"], 
                    rowIndex);
                
                this.Model.SetValue(
                    "F_ZMER_TEXT_WT6", 
                    FormatDecimal(rows[j]["FTAXPRICE"], 2), 
                    rowIndex);
                
                this.Model.SetValue(
                    "F_ZMER_TEXT_YEL", 
                    FormatDecimal(rows[j]["FUPPRICE"], 2),
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_HPA", 
                    FormatDecimal(rows[j]["FDOWNPRICE"], 2), 
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_ZZZ", 
                    FormatDate(rows[j]["FEFFECTIVEDATE"]), 
                    rowIndex);
                this.Model.SetValue(
                    "F_ZMER_TEXT_IAO", 
                    FormatDate(rows[j]["FEXPIRYDATE"]), 
                    rowIndex);

                decimal price;
                if (decimal.TryParse(rows[j]["FPRICE"]?.ToString(), out price))
                {
                    if (price < minPrice) minPrice = price;
                    if (price > maxPrice) maxPrice = price;
                }
            }
            SetRowBackColorByFirstColumn(
                "F_ZMER_Entity_qhl",
                "F_ZMER_TEXT_ACP",
                "相似物料采购价目信息",
                "#DCEBFF"
            );
        }

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


        #endregion

        #region 公共方法

        /// <summary>
        /// 根据单据体第一列字段的值，设置匹配行的背景色
        /// </summary>
        /// <param name="entityKey">单据体标识</param>
        /// <param name="firstColumnFieldKey">第一列字段标识</param>
        /// <param name="matchText">需要匹配的文本内容</param>
        /// <param name="color">颜色（如 #DCEBFF / #0000FF）</param>
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

        private string FormatDate(object value)
        {
            if (value == null)
                return string.Empty;

            DateTime dt;
            if (DateTime.TryParse(value.ToString(), out dt))
            {
                return dt.ToString("yyyy-MM-dd");
            }

            return string.Empty;
        }

        private string FormatDecimal(object value, int scale = 2)
        {
            if (value == null || value == DBNull.Value)
                return string.Empty;

            decimal d;
            if (!decimal.TryParse(value.ToString(), out d))
                return string.Empty;

            return d.ToString("F" + scale);
        }

        #endregion
    }
}
