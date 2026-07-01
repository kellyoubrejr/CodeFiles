using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Zitn_Pur_Application_ClickEntityButton_OpenForm_Plugin
{
    [Description("【单据插件】点击单据体进度按钮事件")]
        [HotUpdate]
    public class EntityButton : AbstractBillPlugIn
    {
        public override void AfterEntryBarItemClick(AfterBarItemClickEventArgs e)
        {
            base.AfterEntryBarItemClick(e);

            if (!e.BarItemKey.Equals("ZMER_tbButton"))
                return;

            string reqBillNo = Convert.ToString(this.Model.GetValue("FBillNo"));
            string reqDisplayStatus = Convert.ToString(this.Model.GetValue("FDocumentStatus"));

            string reqStatus = CalcStatus(new[] { reqDisplayStatus });


            if (string.IsNullOrWhiteSpace(reqBillNo))
            {
                this.View.ShowMessage("当前单据无单据编号！");
                return;
            }

            var poList = QueryPoOrders(reqBillNo);
            string poStatus = CalcStatus(poList.Select(p => p.Status));

            var inspectStatuses = new List<string>();
            var instockStatuses = new List<string>();

            foreach (var po in poList)
            {
                inspectStatuses.AddRange(QueryInspectBills(po.BillNo));
                instockStatuses.AddRange(QueryInstockBills(po.BillNo));
            }

            string inspectStatus = CalcStatus(inspectStatuses);
            string instockStatus = CalcStatus(instockStatuses);

            var showParam = new BillShowParameter
            {
                FormId = "kbfd33122f695426096e7f25a70c3c76f",
                Status = OperationStatus.ADDNEW,

                Width = 900,
                /*Height = 250*/
                Height = 400

            };

            showParam.CustomParams.Add("ReqBillNo", reqBillNo);
            showParam.CustomParams.Add("ReqStatus", reqStatus);
            showParam.CustomParams.Add("POStatus", poStatus);
            showParam.CustomParams.Add("InspectStatus", inspectStatus);
            showParam.CustomParams.Add("InstockStatus", instockStatus);

            this.View.ShowForm(showParam);
        }
        #region ===== 查询方法 =====

        private List<(string BillNo, string Status)> QueryPoOrders(string reqBillNo)
        {
            string sql = $@"
                            SELECT DISTINCT A.FBILLNO, A.FDOCUMENTSTATUS
                            FROM T_PUR_POORDER A
                            JOIN T_PUR_POORDERENTRY B ON A.FID = B.FID
                            JOIN T_PUR_POORDERENTRY_R C ON B.FENTRYID = C.FENTRYID
                            WHERE C.FSRCBILLNO = '{reqBillNo}'";

            return DBUtils.ExecuteDynamicObject(this.Context, sql)
                .Select(r => (
                    r["FBILLNO"].ToString(),
                    r["FDOCUMENTSTATUS"].ToString()
                )).ToList();
        }

        private List<string> QueryInspectBills(string poBillNo)
        {
            string sql = $@"
                        SELECT DISTINCT A.FDOCUMENTSTATUS
                        FROM T_QM_INSPECTBILL A
                        JOIN T_QM_INSPECTBILLENTRY B ON A.FID = B.FID
                        JOIN T_QM_IBREFERDETAIL C ON B.FENTRYID = C.FENTRYID
                        WHERE C.FORDERBILLNO = '{poBillNo}'";

            return DBUtils.ExecuteDynamicObject(this.Context, sql)
                .Select(r => r["FDOCUMENTSTATUS"].ToString())
                .ToList();
        }

        private List<string> QueryInstockBills(string poBillNo)
        {
            string sql = $@"
                        SELECT DISTINCT A.FDOCUMENTSTATUS
                        FROM T_STK_INSTOCK A
                        JOIN T_STK_INSTOCKENTRY B ON A.FID = B.FID
                        WHERE B.FPOORDERNO = '{poBillNo}'";

            return DBUtils.ExecuteDynamicObject(this.Context, sql)
                .Select(r => r["FDOCUMENTSTATUS"].ToString())
                .ToList();
        }

        #endregion

        #region ===== 状态计算 =====

        private string CalcStatus(IEnumerable<string> statuses)
        {
            if (statuses == null || !statuses.Any())
                return "未生成";

            return statuses.All(s => s == "C") ? "已完成" : "进行中";
        }

        #endregion

    }

    [Description("订单进度单据插件：接收并填充数据")]
    [HotUpdate]
    public class Class2 : AbstractBillPlugIn
    {
        public override void AfterCreateNewData(EventArgs e)
        {
            base.AfterCreateNewData(e);

            var param = this.View.OpenParameter;

            this.Model.SetValue("F_ZMER_TEXT_CA9", param.GetCustomParameter("ReqBillNo"));
            this.Model.SetValue("F_ZMER_TEXT_UKY", param.GetCustomParameter("ReqStatus"));
            this.Model.SetValue("F_ZMER_TEXT_W5C", param.GetCustomParameter("POStatus"));
            this.Model.SetValue("F_ZMER_TEXT_YRR", param.GetCustomParameter("InspectStatus"));
            this.Model.SetValue("F_ZMER_TEXT_ZC5", param.GetCustomParameter("InstockStatus"));
        }

    }


}
