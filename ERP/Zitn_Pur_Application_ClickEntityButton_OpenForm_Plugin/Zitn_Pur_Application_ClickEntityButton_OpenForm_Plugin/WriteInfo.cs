using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using static Kingdee.K3.MFG.App.AppServiceContext;

namespace Zitn_Pur_Application_ClickMenuButton_OpenForm_Plugin
{
    [Description("流程进度单据插件：接收数据")]
    [HotUpdate]
    public class WriteInfo : AbstractDynamicFormPlugIn
    {
        public override void EntryButtonCellClick(EntryButtonCellClickEventArgs e)
        {
            base.EntryButtonCellClick(e);

            if (!e.EntryKey.EqualsIgnoreCase("F_ZMER_Entity_tzk"))
                return;

            switch (e.FieldKey.ToUpper())
            {
                case "F_ZMER_TEXT_UKY":
                case "F_ZMER_TEXT_W5C":
                case "F_ZMER_TEXT_YRR":
                case "F_ZMER_TEXT_ZC5":
                    OpenFlowForm(e.Row, e.FieldKey);
                    break;
            }
        }

        private void OpenFlowForm(int row, string fieldKey)
        {
            var showParam = new BillShowParameter
            {
                FormId = "kfaf85e0e1e73450d921d38718db91ab7",
                Status = OperationStatus.ADDNEW,
                Width = 900,
                Height = 600
            };


            string reqBillNo = this.Model.GetValue("F_ZMER_TEXT_CA9", row).ToString();
            showParam.CustomParams.Add("ReqBillNo", reqBillNo);

            this.View.ShowForm(showParam);
        }
    }

    [Description("流程进度单据插件：填充数据")]
    [HotUpdate]
    public class WriteInfoClass1 : AbstractBillPlugIn
    {
        public override void AfterCreateNewData(EventArgs e)
        {
            base.AfterCreateNewData(e);

            var param = this.View.OpenParameter;

            string reqBillNo = param.GetCustomParameter("ReqBillNo").ToString();

            string entryKey = "F_ZMER_Entity_apv";

            var reqList = QueryReqEntry(reqBillNo);
            if (reqList.Count == 0)
            {
                this.View.ShowMessage("未查询到采购申请明细！");
                return;
            }

            this.View.Model.DeleteEntryData(entryKey);

            foreach (var req in reqList)
            {
                // 查询 PO / 检验 / 入库（按物料对齐）
                var poList = QueryPO(req.BillNo, req.WLNUM);
                var qcList = QueryQC(req.BillNo, req.WLNUM);
                var rkList = QueryRK(req.BillNo, req.WLNUM);

                /*int maxCount = Math.Max(Math.Max(poList.Count, qcList.Count), rkList.Count);*/

                int maxCount = Math.Max(
                    1,
                    Math.Max(
                        Math.Max(poList.Count, qcList.Count),
                        rkList.Count
                    )
                );

                for (int i = 0; i < maxCount; i++)
                {
                    this.View.Model.CreateNewEntryRow(entryKey);
                    int row = this.View.Model.GetEntryRowCount(entryKey) - 1;

                    // 采购申请明细
                    this.View.Model.SetValue("FCGSQDNO", req.BillNo, row);


                  
                    if (req.DHSJ < req.AVGDATE)
                    {
                        this.View.Model.SetValue("FWEIXIAN", "存在逾期风险", row);
                        SetCellBackColor("F_ZMER_Entity_apv", row, "FWEIXIAN", "#e91010");
                    }
                    else
                    {
                        this.View.Model.SetValue("FWEIXIAN", "正常", row);
                        //SetCellBackColor("F_ZMER_Entity_apv", row, "FWEIXIAN", "#008000");
                    }




                    this.View.Model.SetValue("FCGSQDSTATUS", FormatStatus(req.Status), row);
                    this.View.Model.SetValue("FSYB", req.SYB, row);
                    this.View.Model.SetValue("FXM", req.XM, row);
                    this.View.Model.SetValue("FSQR", req.SQR, row);
                    this.View.Model.SetValue("FSQSJ", req.SQSJ, row);
                    this.View.Model.SetValue("FWLNUM", req.WLNUM, row);
                    this.View.Model.SetValue("FWLNAME", req.WLNAME, row);
                    this.View.Model.SetValue("FGG", req.GG, row);
                    this.View.Model.SetValue("Fsyjhrq", req.DHSJ, row);
                    this.View.Model.SetValue("Fsycgrq", req.PJSJ, row);

                    // ===== 采购订单 =====
                    if (i < poList.Count)
                    {
                        var po = poList[i];
                        this.View.Model.SetValue("FCGDDNO", po.BillNo, row);
                        this.View.Model.SetValue("FCGDDSTATUS", FormatStatus(po.Status), row);
                    }

                    // ===== 检验单 =====
                    if (i < qcList.Count)
                    {
                        var qc = qcList[i];
                        this.View.Model.SetValue("FQCNO", qc.BillNo, row);
                        this.View.Model.SetValue("FQCSTATUS", FormatStatus(qc.Status), row);
                    }

                    // ===== 入库单 =====
                    if (i < rkList.Count)
                    {
                        var rk = rkList[i];
                        this.View.Model.SetValue("FINSTKNO", rk.BillNo, row);
                        this.View.Model.SetValue("FINSTKSTATUS", FormatStatus(rk.Status), row);
                    }
                }
            }

            this.View.UpdateView(entryKey);
        }

        private string FormatStatus(string status)
        {
            switch (status)
            {
                case "C": return "已完成";
                case "A":
                case "B":
                case "D":
                case "Z":
                default:
                    return "进行中";
            }
        }

        private void SetCellBackColor(string entryKey, int rowIndex, string fieldName, string color)
        {
            SetCellBackColor(entryKey, rowIndex, new[] { fieldName }, color);
        }

        private void SetCellBackColor(string entryKey, int rowIndex, IEnumerable<string> fieldNames, string color)
        {
            var grid = this.View.GetControl<EntryGrid>(entryKey);
            if (grid == null || fieldNames == null)
                return;

            // 行号合法性校验（建议加上，避免越界）
            int rowCount = this.Model.GetEntryRowCount(entryKey);
            if (rowIndex < 0 || rowIndex >= rowCount)
                return;

            foreach (string fieldName in fieldNames)
            {
                if (!string.IsNullOrEmpty(fieldName))
                {
                    List<KeyValuePair<int, string>> colorList = new List<KeyValuePair<int, string>>
            {
                new KeyValuePair<int, string>(rowIndex, color)
            };

                    grid.SetCellsBackcolor(fieldName, colorList);
                }
            }
        }


        private List<BillInfo> QueryRK(string reqBillNo, string materialNumber)
        {
            string sql = $@"
                /*dialect*/
                SELECT DISTINCT A.FBILLNO, A.FDOCUMENTSTATUS
                FROM T_STK_INSTOCK A
                JOIN T_STK_INSTOCKENTRY B ON A.FID = B.FID
                JOIN T_BD_MATERIAL M ON B.FMATERIALID = M.FMATERIALID
                WHERE B.FPOORDERNO IN (
                    SELECT A.FBILLNO
                    FROM T_PUR_POORDER A
                    JOIN T_PUR_POORDERENTRY B ON A.FID = B.FID
                    JOIN T_PUR_POORDERENTRY_R C ON B.FENTRYID = C.FENTRYID
                    WHERE C.FSRCBILLNO = '{reqBillNo}'
                )
                AND M.FNUMBER = '{materialNumber}'
            ";
            return ExecuteQuery(sql);
        }

        private List<BillInfo> QueryQC(string reqBillNo, string materialNumber)
        {
            string sql = $@"
                /*dialect*/
                SELECT DISTINCT A.FBILLNO, A.FDOCUMENTSTATUS
                FROM T_QM_INSPECTBILL A
                JOIN T_QM_INSPECTBILLENTRY B ON A.FID = B.FID
                JOIN T_QM_IBREFERDETAIL C ON B.FENTRYID = C.FENTRYID
                JOIN T_QM_INSPECTBILLENTRY_A T1 ON B.FENTRYID = T1.FENTRYID
                JOIN T_BD_MATERIAL M ON T1.FMATERIALID = M.FMATERIALID
                WHERE C.FORDERBILLNO IN (
                    SELECT A.FBILLNO
                    FROM T_PUR_POORDER A
                    JOIN T_PUR_POORDERENTRY B ON A.FID = B.FID
                    JOIN T_PUR_POORDERENTRY_R C ON B.FENTRYID = C.FENTRYID
                    WHERE C.FSRCBILLNO = '{reqBillNo}'
                )
                AND M.FNUMBER = '{materialNumber}'
            ";
            return ExecuteQuery(sql);
        }

        private List<BillInfo> QueryPO(string reqBillNo, string materialNumber)
        {
            string sql = $@"
                /*dialect*/
                SELECT DISTINCT A.FBILLNO, A.FDOCUMENTSTATUS
                FROM T_PUR_POORDER A
                JOIN T_PUR_POORDERENTRY B ON A.FID = B.FID
                JOIN T_PUR_POORDERENTRY_R C ON B.FENTRYID = C.FENTRYID
                JOIN T_BD_MATERIAL M ON B.FMATERIALID = M.FMATERIALID
                WHERE C.FSRCBILLNO = '{reqBillNo}' AND M.FNUMBER = '{materialNumber}'
            ";
            return ExecuteQuery(sql);
        }

        private List<BillInfo> ExecuteQuery(string sql)
        {
            var list = new List<BillInfo>();
            var data = DbUtils.ExecuteDynamicObject(this.Context, sql);
            if (data == null) return list;

            foreach (var row in data)
            {
                list.Add(new BillInfo
                {
                    BillNo = Convert.ToString(row["FBILLNO"]),
                    Status = Convert.ToString(row["FDOCUMENTSTATUS"])
                });
            }
            return list;
        }

        //获取当前日期
        DateTime nowTime = DateTime.Now;
        private List<ReqEntryInfo> QueryReqEntry(string billNo)
        {
            // F_ZMER_ASSISTANT_RE5 更改 F_UNW_Assistant_qtr_83g
            string sql = $@"
                /*dialect*/
                SELECT
                    A.FBILLNO,
                    A.FDOCUMENTSTATUS,
                    B.FENTRYID,
                    C.FNUMBER        AS FWLNUM,
                    D.FNAME          AS FWLNAME,
                    D.FSPECIFICATION AS FGG,
                    T2.FCAPTION      AS FSYB,
                    A.F_UNW_Assistant_qtr_83g AS FXM,
                    T3.FNAME         AS FSQR,
                    A.FAPPLICATIONDATE AS FSQSJ,
                    FARRIVALDATE
                FROM T_PUR_Requisition A
                JOIN T_PUR_REQENTRY B ON A.FID = B.FID
                JOIN T_BD_MATERIAL C ON B.FMATERIALID = C.FMATERIALID
                JOIN T_BD_MATERIAL_L D ON C.FMATERIALID = D.FMATERIALID
                JOIN T_META_FORMENUMITEM T1 ON A.F_PAEZ_SYB = T1.FVALUE
                JOIN T_META_FORMENUMITEM_L T2 ON T1.FENUMID = T2.FENUMID
                JOIN T_BD_STAFF_L T3 ON A.FAPPLICANTID = T3.FSTAFFID
                WHERE A.FBILLNO = '{billNo}' AND T1.FID ='342ceac5-f321-44d3-bce2-06da551840db'
            ";

            var list = new List<ReqEntryInfo>();
            var data = DbUtils.ExecuteDynamicObject(this.Context, sql);
            if (data == null) return list;

            foreach (var row in data)
            {
                string materialNumber = Convert.ToString(row["FWLNUM"]);

                string avgCycleQuery = string.Format(@"
                                                /*dialect*/
                                                SELECT 
                                                            CEILING(AVG(DATEDIFF(DAY, A.FAPPLICATIONDATE, B.FARRIVALDATE))) AS AVGDATE
                                                        FROM
                                                            T_PUR_Requisition A 
                                                        JOIN 
                                                            T_PUR_ReqEntry B ON A.FID = B.FID
                                                        JOIN
                                                            T_BD_MATERIAL M1 ON B.FMATERIALID = M1.FMATERIALID
                                                        WHERE 
                                                            M1.FNUMBER = '{0}'
                                                            AND A.FAPPLICATIONDATE >= DATEADD(YEAR, -1, GETDATE())", materialNumber);

                DynamicObjectCollection avgResult = DBUtils.ExecuteDynamicObject(this.Context, avgCycleQuery);
                long averageCycleInDays = 0;
                if (avgResult != null && avgResult.Count > 0 && avgResult[0]["AVGDATE"] != null)
                {
                    
                    averageCycleInDays = Convert.ToInt64(avgResult[0]["AVGDATE"]);
                }


                list.Add(new ReqEntryInfo
                {
                    BillNo = Convert.ToString(row["FBILLNO"]),
                    Status = Convert.ToString(row["FDOCUMENTSTATUS"]),
                    EntryId = Convert.ToInt64(row["FENTRYID"]),
                    WLNUM = Convert.ToString(row["FWLNUM"]),
                    WLNAME = Convert.ToString(row["FWLNAME"]),
                    GG = Convert.ToString(row["FGG"]),
                    SYB = Convert.ToString(row["FSYB"]),
                    XM = Convert.ToString(row["FXM"]),
                    SQR = Convert.ToString(row["FSQR"]),
                    SQSJ = row["FSQSJ"] == null ? (DateTime?)null : Convert.ToDateTime(row["FSQSJ"]),
                    DHSJ = (long)(row["FARRIVALDATE"] == DBNull.Value ? (int?)null : (Convert.ToDateTime(row["FARRIVALDATE"]).Date - DateTime.Now.Date).Days),
                    PJSJ = ((long)(row["FARRIVALDATE"] == DBNull.Value ? (int?)null : (Convert.ToDateTime(row["FARRIVALDATE"]).Date - DateTime.Now.Date).Days) - averageCycleInDays) ,
                    AVGDATE = averageCycleInDays,





                });
            }

            return list;
        }
    }

    #region DTO

    public class BillInfo
    {
        public string BillNo { get; set; }
        public string Status { get; set; }
    }

    public class ReqEntryInfo : BillInfo
    {
        public long EntryId { get; set; }
        public string SYB { get; set; }
        public string XM { get; set; }
        public string SQR { get; set; }
        public DateTime? SQSJ { get; set; }
        public Int64 DHSJ { get; set; }
        public Int64 PJSJ { get; set; }
        public string WLNUM { get; set; }
        public string WLNAME { get; set; }
        public string GG { get; set; }
        public long AVGDATE { get; internal set; }
    }

    #endregion
}
