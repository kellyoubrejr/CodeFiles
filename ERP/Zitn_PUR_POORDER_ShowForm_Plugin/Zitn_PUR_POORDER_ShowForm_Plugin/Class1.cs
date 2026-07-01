using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.ComponentModel;

namespace Zitn_PUR_POORDER_ShowForm_Plugin
{
    [Description("【单据插件】点击字段打开采购需求查看模版,并填充数据信息")]
    [HotUpdate]
    public class Class1 : AbstractBillPlugIn
    {
        public override void EntryButtonCellClick(EntryButtonCellClickEventArgs e)
        {
            base.EntryButtonCellClick(e);

            if (!e.FieldKey.EqualsIgnoreCase("F_ZMER_TEXT_6OQ"))
                return;

            if (e.Row < 0)
                return;

            string billNo = Convert.ToString(this.Model.GetValue("FBillNo"));

            DynamicObject materialObj = this.Model.GetValue("FMaterialId", e.Row) as DynamicObject;
            
            string materialId1 = materialObj != null ? materialObj["Id"].ToString() : "";


            DynamicObject supplierObj = this.Model.GetValue("FSupplierId", e.Row) as DynamicObject;

            string supplierId = supplierObj != null ? supplierObj["Id"].ToString() : "";


            var materialId2 = string.Format(@"SELECT TOP 1 FNUMBER FROM T_BD_MATERIAL WHERE FMATERIALID = '{0}'", materialId1);
            DynamicObjectCollection material = DBUtils.ExecuteDynamicObject(this.Context, materialId2);

            string materialId = "";

            if (material.Count > 0)
            {
                materialId = material[0]["FNUMBER"].ToString();
            }

            var nameQuery = string.Format(@"SELECT TOP 1 FNAME,FSPECIFICATION FROM T_BD_MATERIAL_L WHERE FMATERIALID = '{0}'", materialId1);
            DynamicObjectCollection nameResult = DBUtils.ExecuteDynamicObject(this.Context, nameQuery);

            string wlname = "";
            string wlguige = "";
            if (nameResult.Count > 0)
            {
                wlname = nameResult[0]["FNAME"].ToString();
                wlguige = nameResult[0]["FSPECIFICATION"].ToString();
            }



            var showParam = new BillShowParameter
            {
                FormId = "ZMER_PUR_DEMO",
                Status = OperationStatus.ADDNEW
            };

            showParam.CustomParams.Add("SrcBillNo", billNo);
            showParam.CustomParams.Add("SrcMaterialId", materialId);
            showParam.CustomParams.Add("SrcSupplierId", supplierId);
            showParam.CustomParams.Add("SrcMaterialName", wlname);
            showParam.CustomParams.Add("SrcMaterialSpecification", wlguige);

            this.View.ShowForm(showParam);
        }
    }

    [Description("ZMER_PUR_DEMO 单据插件：填充展示数据")]
    [HotUpdate]
    public class ZmerPurDemoFillPlugIn : AbstractBillPlugIn
    {
        public override void AfterCreateNewData(EventArgs e)
        {
            base.AfterCreateNewData(e);

            string billNo = this.View.OpenParameter.GetCustomParameter("SrcBillNo")?.ToString();

            string materialId = this.View.OpenParameter.GetCustomParameter("SrcMaterialId")?.ToString();

            string supplierId = this.View.OpenParameter.GetCustomParameter("SrcSupplierId")?.ToString();

            string wlname = this.View.OpenParameter.GetCustomParameter("SrcMaterialName")?.ToString();

            string wlguige = this.View.OpenParameter.GetCustomParameter("SrcMaterialSpecification")?.ToString();

            if (string.IsNullOrEmpty(materialId))
                return;

            this.Model.SetValue("F_ZMER_TEXT_2BG", materialId);
            this.Model.SetValue("F_ZMER_Text_uky", wlname);
            this.Model.SetValue("F_ZMER_TEXT_DVN", wlguige);

            FillPriceList(materialId, supplierId);

            FillInquiry(materialId);

            FillHistory(materialId, billNo);
        }

        /// <summary>
        /// 日期时间格式化【年月日时分秒→年月日】
        /// </summary>
        /// <param name="value">字段值</param>
        /// <returns></returns>
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

        /// <summary>
        /// 金额格式化【保留两位小数】
        /// </summary>
        /// <param name="value">字段值</param>
        /// <param name="scale">位数</param>
        /// <returns></returns>
        private string FormatDecimal(object value, int scale = 2)
        {
            if (value == null || value == DBNull.Value)
                return string.Empty;

            decimal d;
            if (!decimal.TryParse(value.ToString(), out d))
                return string.Empty;

            return d.ToString("F" + scale);
        }

        /// <summary>
        /// 填充采购价目信息
        /// </summary>
        /// <param name="materialNumber">物料编码</param>
        private void FillPriceList(string materialNumber, string supplierId)
        {
            string entityKey = "F_ZMER_Entity_qhl";
            this.Model.DeleteEntryData(entityKey);

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
                            FROM T_PUR_POORDER A 
                            LEFT JOIN T_PUR_POORDERENTRY B ON A.FID = B.FID
                            LEFT JOIN t_PUR_PriceListEntry C ON B.FMATERIALID = C.FMATERIALID
                            LEFT JOIN t_PUR_PriceList D ON C.FID = D.FID
                            LEFT JOIN T_BD_SUPPLIER_L E ON D.FSUPPLIERID = E.FSUPPLIERID
                            LEFT JOIN T_BD_MATERIAL F ON C.FMATERIALID = F.FMATERIALID
                            LEFT JOIN T_BD_MATERIAL_L G ON F.FMATERIALID = G.FMATERIALID
                            WHERE F.FNUMBER = '" + materialNumber + "'" +
                            "AND A.FSUPPLIERID = '" + supplierId + "'" +
                            "AND A.FDOCUMENTSTATUS = 'C'";

            DynamicObjectCollection rows = DBUtils.ExecuteDynamicObject(this.Context, sql);

            decimal minPrice = decimal.MaxValue;
            decimal maxPrice = decimal.MinValue;

            for (int i = 0; i < rows.Count; i++)
            {
                this.Model.CreateNewEntryRow(entityKey);

/*                this.Model.SetValue(
                    "F_ZMER_TEXT_ACP",
                    "采购价目信息",
                    i);*/
                this.Model.SetValue(
                    "F_ZMER_TEXT_APV",
                    rows[i]["FNUMBER"]?.ToString(),
                    i);
                this.Model.SetValue(
                    "F_ZMER_TEXT_TNE", 
                    rows[i]["MaterialNumber"]?.ToString(), 
                    i);
                this.Model.SetValue(
                    "F_ZMER_TEXT_CX3", 
                    rows[i]["MaterialName"]?.ToString(),
                    i);
                this.Model.SetValue(
                    "F_ZMER_TEXT_V8S", 
                    rows[i]["FSPECIFICATION"]?.ToString(), 
                    i);
                this.Model.SetValue(
                    "F_ZMER_TEXT_DJH", 
                    FormatDecimal(rows[i]["FPRICE"], 2),
                    i);
                this.Model.SetValue(
                    "F_ZMER_TEXT_WT6", 
                    FormatDecimal(rows[i]["FTAXPRICE"], 2), 
                    i);
                this.Model.SetValue(
                    "F_ZMER_TEXT_F4W", 
                    FormatDecimal(rows[i]["FTAXRATE"],2),
                    i);
                this.Model.SetValue(
                    "F_ZMER_TEXT_YEL", 
                    FormatDecimal(rows[i]["FUPPRICE"], 2), 
                    i);
                this.Model.SetValue(
                    "F_ZMER_TEXT_HPA", 
                    FormatDecimal(rows[i]["FDOWNPRICE"], 2),
                    i);
                this.Model.SetValue(
                    "F_ZMER_TEXT_ZZZ",
                    FormatDate(rows[i]["FEFFECTIVEDATE"]),
                    i);
                this.Model.SetValue(
                    "F_ZMER_TEXT_IAO",
                    FormatDate(rows[i]["FEXPIRYDATE"]),
                    i);

                decimal price;
                if (decimal.TryParse(rows[i]["FPRICE"]?.ToString(), out price)) 
                {
                    if(price < minPrice) minPrice = price;
                    if(price > maxPrice) maxPrice = price;
                }
            }

            ///<summary>
            /// 填充小计、最低价和最高价
            /// </summary>
            int lastRow = rows.Count;
            this.Model.CreateNewEntryRow(entityKey);
            this.Model.SetValue(
                "F_ZMER_TEXT_ACP",
                "小计",
                lastRow);
            this.Model.SetValue(
                "F_ZMER_TEXT_APV",
                "最低价格",
                lastRow);
            this.Model.SetValue(
                "F_ZMER_TEXT_TNE",
                FormatDecimal(minPrice, 2),
                lastRow);
            this.Model.SetValue(
                "F_ZMER_TEXT_CX3",
                "最高价格",
                lastRow);
            this.Model.SetValue(
                "F_ZMER_TEXT_V8S",
                FormatDecimal(maxPrice, 2),
                lastRow);            
        }

        
        /// <summary>
        /// 填充采购询价信息
        /// </summary>
        /// <param name="materialNumber">物料编码</param>
        private void FillInquiry(string materialNumber)
        {
            string entityKey = "F_ZMER_Entity_9ra";
            this.Model.DeleteEntryData(entityKey);

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
                                G.FNAME                  AS Buyer,
                                C.FNUMBER                AS MaterialCode,
                                D.FNAME                  AS MaterialName,
                                D.FSPECIFICATION         AS Specification,
                                F.FNAME                  AS Unit,
                                B.F_ZMER_Qty_yrr         AS Qty,
                                B.F_ZMER_Datetime_h1g    AS ArrivalDate,
                                H.FNAME                  AS DeliveryMethod,
                                B.F_ZMER_Remarks_imu     AS DeliveryAddress,
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
                            JOIN V_BAS_ASSISTANTDATAENTRY_L H ON B.F_ZMER_Assistant_zc5 = H.FID
                            WHERE C.FNUMBER = '" + materialNumber + @"'
                              AND A.FDOCUMENTSTATUS = 'C'
                                AND H.FLOCALEID = 2052"
                                ;

            DynamicObjectCollection rows = DBUtils.ExecuteDynamicObject(this.Context, sql);

            decimal minPrice = decimal.MaxValue;
            decimal maxPrice = decimal.MinValue;
            int inquiryCount = 0;
            decimal totalQty = 0;

            for (int i = 0; i < rows.Count; i++)
            {
                this.Model.CreateNewEntryRow(entityKey);

                /*this.Model.SetValue(
                    "F_ZMER_TEXT_N1V",
                    "采购询价记录", 
                    i);*/
                this.Model.SetValue("F_ZMER_TEXT_83G", rows[i]["FBillno"], i);
                this.Model.SetValue(
                    "F_ZMER_TEXT_6CK", 
                    FormatDate(rows[i]["InquiryDate"]), 
                    i);
                this.Model.SetValue(
                    "F_ZMER_TEXT_PMA",
                    rows[i]["PurchaseOrg"]?.ToString(),
                    i);
                this.Model.SetValue(
                    "F_ZMER_TEXT_7XZ", 
                    rows[i]["SupplierName"]?.ToString(),
                    i);
                this.Model.SetValue(
                    "F_ZMER_TEXT_Q8O", 
                    rows[i]["Buyer"]?.ToString(), 
                    i);
                this.Model.SetValue(
                    "F_ZMER_TEXT_9ID", 
                    rows[i]["MaterialCode"]?.ToString(),
                    i);
                this.Model.SetValue(
                    "F_ZMER_TEXT_ST2", 
                    rows[i]["MaterialName"]?.ToString(), 
                    i);
                this.Model.SetValue(
                    "F_ZMER_TEXT_B3R", 
                    rows[i]["Specification"]?.ToString(), 
                    i);
                this.Model.SetValue(
                    "F_ZMER_TEXT_UEH", 
                    rows[i]["Unit"]?.ToString(), 
                    i);
                this.Model.SetValue(
                    "F_ZMER_TEXT_CO6", 
                    FormatDecimal(rows[i]["Qty"], 2),
                    i);
                this.Model.SetValue(
                    "F_ZMER_TEXT_VZV", 
                    FormatDate(rows[i]["ArrivalDate"]), 
                    i);
                this.Model.SetValue(
                    "F_ZMER_TEXT_EAK", 
                    rows[i]["DeliveryMethod"]?.ToString(),
                    i);
                this.Model.SetValue(
                    "F_ZMER_TEXT_XK9", 
                    rows[i]["DeliveryAddress"]?.ToString(), 
                    i);
                this.Model.SetValue(
                    "F_ZMER_TEXT_GVY", 
                    FormatDecimal(rows[i]["Price"], 2), 
                    i);
                this.Model.SetValue(
                    "F_ZMER_TEXT_Y5O", 
                    rows[i]["Remark"]?.ToString(), 
                    i);

                inquiryCount++;
                //totalQty += FormatDecimal(rows[i]["Qty"], 2);
                decimal price;
                if(decimal.TryParse(rows[i]["Price"]?.ToString(), out price))
                {
                    if (price < minPrice) minPrice = price;
                    if (price > maxPrice) maxPrice = price;
                }

            }

            int lastRow = rows.Count;
            this.Model.CreateNewEntryRow(entityKey);
            this.Model.SetValue(
                "F_ZMER_TEXT_N1V",
                "小计",
                lastRow);
            /*this.Model.SetValue(
                "F_ZMER_TEXT_6CK", 
                "询价次数", 
                lastRow);
            this.Model.SetValue(
                "F_ZMER_TEXT_PMA", 
                inquiryCount, 
                lastRow);*/
            this.Model.SetValue(
                "F_ZMER_Text_83g", 
                "最低报价", 
                lastRow);
            this.Model.SetValue(
                "F_ZMER_Text_9id",
                FormatDecimal(minPrice, 2), 
                lastRow);
            this.Model.SetValue(
                "F_ZMER_Text_st2", 
                "最高报价", 
                lastRow);
            this.Model.SetValue(
                "F_ZMER_Text_b3r", 
                FormatDecimal(maxPrice, 2), 
                lastRow);

        }

        /// <summary>
        /// 填充历史采购信息
        /// </summary>
        /// <param name="materialNumber">物料编码</param>
        /// <param name="billNo">采购订单号</param>
        private void FillHistory(string materialNumber, string billNo)
        {
            string entityKey = "F_ZMER_Entity_r2z";
            this.Model.DeleteEntryData(entityKey);

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
                            WHERE C.FNUMBER = '" + materialNumber + @"'
                              AND A.FDOCUMENTSTATUS = 'C'
                              AND A.FBILLNO <> '" + billNo + "'";

            DynamicObjectCollection rows = DBUtils.ExecuteDynamicObject(this.Context, sql);

            decimal totalQty = 0;
            decimal totalAmount = 0;
            int count = 0;

            for (int i = 0; i < rows.Count; i++)
            {
                this.Model.CreateNewEntryRow(entityKey);

                /*this.Model.SetValue(
                    "F_ZMER_TEXT_1LD",
                    "历史采购信息", 
                    i);*/
                this.Model.SetValue(
                    "F_ZMER_TEXT_KV3",
                    rows[i]["FBillno"]?.ToString(), 
                    i);
                this.Model.SetValue(
                    "F_ZMER_TEXT_36S",
                    FormatDate(rows[i]["FCreateDate"]),
                    i);
                this.Model.SetValue(
                    "F_ZMER_TEXT_LGH", 
                    rows[i]["SupplierName"]?.ToString(),
                    i);
                this.Model.SetValue(
                    "F_ZMER_TEXT_4R6", 
                    FormatDecimal(rows[i]["Qty"], 2), 
                    i);
                this.Model.SetValue(
                    "F_ZMER_TEXT_HGD", 
                    FormatDecimal(rows[i]["Price"], 2), 
                    i);
                this.Model.SetValue(
                    "F_ZMER_TEXT_RE5",
                    FormatDecimal(rows[i]["FALLAMOUNT"], 2),
                    i);

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
            int summaryRow = rows.Count;
            this.Model.CreateNewEntryRow(entityKey);

            this.Model.SetValue("F_ZMER_TEXT_1LD", "小计", summaryRow);
            this.Model.SetValue("F_ZMER_TEXT_KV3", "总采购数量", summaryRow);
            this.Model.SetValue("F_ZMER_TEXT_36S", FormatDecimal(totalQty, 2), summaryRow);

            this.Model.SetValue("F_ZMER_TEXT_LGH", "平均采购价格", summaryRow);

            decimal avgPrice = totalQty > 0 ? totalAmount / totalQty : 0m;
            this.Model.SetValue("F_ZMER_TEXT_4R6", FormatDecimal(avgPrice, 2), summaryRow);
        }

    }
}
