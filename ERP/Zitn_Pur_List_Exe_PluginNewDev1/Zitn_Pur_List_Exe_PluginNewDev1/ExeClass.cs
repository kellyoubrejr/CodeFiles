using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using System.Runtime.Remoting.Contexts;

namespace Zitn_Pur_List_Exe_PluginNewDev1
{
    /// <summary>
    /// 【列表插件】打开本地电脑上的免审规则exe程序
    /// </summary>  
    [Description("【列表插件】打开本地电脑上的exe程序"), HotUpdate]
    public class ExeClass : AbstractListPlugIn
    {
        public override void EntryButtonCellClick(EntryButtonCellClickEventArgs e)
        {
            base.EntryButtonCellClick(e);
            var linkBtnKey = "F_ZMER_TEXT_APV";
            if (e.FieldKey.EqualsIgnoreCase(linkBtnKey))
            {
                // 获取所有选中的行数据
                var selectedRows = this.ListView.SelectedRowsInfo;
                if (selectedRows == null || !selectedRows.Any())
                {
                    this.View.ShowMessage("请先选择要处理的行！");
                    return;
                }

                // 获取点击行的fentryid，用于排序：点击行优先展示
                var clickedRow = this.ListView.CurrentPageRowsInfo
                    .FirstOrDefault(p => p.RowKey == e.Row);
                string clickedFentryId = clickedRow?.EntryPrimaryKeyValue;

                List<RuleDto> sendList = new List<RuleDto>();

                // 构建有序列表：从点击行开始，按原序往后，到底后绕回开头
                var selectedList = selectedRows.ToList();
                var orderedRows = new List<dynamic>();

                int clickedIndex = selectedList.FindIndex(r => r.EntryPrimaryKeyValue == clickedFentryId);
                if (clickedIndex >= 0)
                {
                    // 从点击行到末尾
                    for (int i = clickedIndex; i < selectedList.Count; i++)
                        orderedRows.Add(selectedList[i]);
                    // 从开头到点击行前一行的补齐
                    for (int i = 0; i < clickedIndex; i++)
                        orderedRows.Add(selectedList[i]);
                }
                else
                {
                    orderedRows.AddRange(selectedList);
                }

                // 遍历处理
                foreach (var rowData in orderedRows)
                {
                    string billNo = rowData.BillNo;
                    string fid = rowData.PrimaryKeyValue;
                    string fentryid = rowData.EntryPrimaryKeyValue;

                    string wlId = string.Empty;
                    string gysName = string.Empty;
                    decimal qty = 0;
                    decimal price = 0;
                    string danwei = string.Empty;

                    string sql = $@"/*dialect*/
                                            SELECT B.FMATERIALID AS wlid,M1.FNUMBER,
                                                   S1.FNAME AS GYSNAME,
                                                   B.FQTY,
                                                   B1.FTAXPRICE,
                                                   T.FNAME,
                                                    BDN.FNAME AS WLNAME,
                                                    BDN.FSPECIFICATION AS WLGG
                                            FROM T_PUR_POORDER A
                                            JOIN T_PUR_POORDERENTRY B ON A.FID = B.FID
                                            JOIN T_BD_MATERIAL_L BDN ON B.FMATERIALID = BDN.FMATERIALID
                                            JOIN T_BD_MATERIAL M1 ON BDN.FMATERIALID = M1.FMATERIALID
                                            JOIN T_BD_SUPPLIER_L S1 ON A.FSUPPLIERID = S1.FSUPPLIERID
                                            JOIN T_PUR_POORDERENTRY_F B1 ON B.FENTRYID = B1.FENTRYID
                                            JOIN T_BD_UNIT_L T ON B.FUNITID = T.FUNITID
                                            WHERE FBILLNO = '{billNo}'
                                            AND A.FID = '{fid}'
                                            AND B.FENTRYID = '{fentryid}'";

                    var result = DBUtils.ExecuteDynamicObject(this.Context, sql);

                    if (result != null && result.Count > 0)
                    {
                        wlId = result[0]["wlid"].ToString();
                        gysName = result[0]["GYSNAME"].ToString();
                        qty = Convert.ToDecimal(result[0]["FQTY"]);
                        price = Math.Round(Convert.ToDecimal(result[0]["FTAXPRICE"]), 2);
                        danwei = result[0]["FNAME"].ToString();

                        //先检查是否存在免审信息，存在直接传输exe免审信息，不存在传输当前选中行的信息
                        string ruleSql = $@"/*dialect*/
                                                    SELECT DISTINCT F_ZMER_CREATEDATE_83G,A.FMATERIALID,m1.fnumber,A.FSUPPLIER,A.F_ZMER_TEXT_QTR,A.F_ZMER_TEXT_83G,A.FTAXPRICE,T.FNAME AS FDANWEI,BDN.FNAME AS WLNAME,BDN.FSPECIFICATION AS WLGG,A.FQTY
                                                    FROM ZMER_t_Cust100025 A2 JOIN
                                                    ZMER_t_Cust_Entry100101 A ON A2.FID = A.FID
                                                    JOIN T_BD_MATERIAL_L BDN ON A.FMATERIALID = BDN.FMATERIALID
                                                    JOIN T_BD_MATERIAL M1 ON BDN.FMATERIALID = M1.FMATERIALID
                                                    JOIN T_BD_SUPPLIER_L B ON A.FSUPPLIER = B.FNAME
                                                    JOIN T_BD_SUPPLIER A1 ON A1.FSUPPLIERID = B.FSUPPLIERID
                                                    JOIN T_PUR_POORDERENTRY PUREN ON A.FMATERIALID = PUREN.FMATERIALID
                                                    JOIN T_PUR_POORDER PUR ON PUR.FID = PUREN.FID AND PUR.FSUPPLIERID = A1.FSUPPLIERID
                                                    JOIN T_BD_UNIT_L T ON PUREN.FUNITID = T.FUNITID
                                                    WHERE B.FNAME = '{gysName}'
                                                    AND A.FMATERIALID = '{wlId}' AND A1.FUSEORGID=101006";

                        var ruleList = DBUtils.ExecuteDynamicObject(this.Context, ruleSql);

                        if (ruleList != null && ruleList.Count > 0)
                        {
                            // 存在免审信息
                            for (int i = 0; i < ruleList.Count; i++)
                            {
                                var rule = ruleList[i];
                                sendList.Add(new RuleDto
                                {
                                    MaterialId = rule["FMATERIALID"].ToString(),
                                    MaterialCode = rule["FNUMBER"].ToString(),
                                    MaterialName = rule["WLNAME"].ToString(),
                                    MaterialSpec = rule["WLGG"].ToString(),
                                    Supplier = rule["FSUPPLIER"].ToString(),
                                    Down = Convert.ToInt32(rule["F_ZMER_TEXT_QTR"]),
                                    Up = Convert.ToInt32(rule["F_ZMER_TEXT_83G"]),
                                    Price = Math.Round(Convert.ToDecimal(rule["FTAXPRICE"]), 2),
                                    Unit = rule["FDANWEI"].ToString(),
                                    Source = "历史免审信息",
                                    Flag = "Flag=1",
                                    Qty = Convert.ToInt32(qty),
                                    BillNo = billNo,
                                    FentryId = fentryid
                                });
                            }
                        }
                        else
                        {
                            // 不存在免审信息，使用当前行数据
                            sendList.Add(new RuleDto
                            {
                                MaterialId = result[0]["wlid"].ToString(),
                                MaterialCode = result[0]["FNUMBER"].ToString(),
                                MaterialName = result[0]["WLNAME"].ToString(),
                                MaterialSpec = result[0]["WLGG"].ToString(),
                                Supplier = gysName,
                                Down = 0,
                                Up = Convert.ToInt32(qty),
                                Price = price,
                                Unit = danwei,
                                Source = "当前数据",
                                Flag = "Flag=1",
                                Qty = Convert.ToInt32(qty),
                                BillNo = billNo,
                                FentryId = fentryid
                            });
                        }
                    }
                }

                if (sendList.Count == 0)
                {
                    this.View.ShowMessage("没有获取到有效数据！");
                    return;
                }

                // 序列化并传输数据
                string json = JsonConvert.SerializeObject(sendList);
                string base64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));

                if (Context.ClientType != ClientType.Silverlight && Context.ClientType != ClientType.WPF)
                {
                    return;
                }

                var url = @"D:\UnAudit_exe\Zitn_exe_App.exe";
                this.View.GetControl("F_Jac_Link").InvokeControlMethod("SetClickFromServerOfParameter", url, base64);
            }
        }

        public class RuleDto
        {
            public string MaterialId { get; set; }
            public string MaterialCode { get; set; }
            public string MaterialName { get; set; }
            public string MaterialSpec { get; set; }
            public string Supplier { get; set; }
            public int Down { get; set; }
            public int Up { get; set; }
            public decimal Price { get; set; }
            public string Unit { get; set; }
            public string Source { get; set; }
            public string Flag { get; set; }
            public string BillNo { get; set; }
            public int Qty { get; set; }
            public string FentryId { get; set; }
        }
    }
}