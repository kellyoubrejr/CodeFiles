using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Contexts;
using System.Text.RegularExpressions;

namespace K3_Test
{
    [Description("【列表插件】点击字段打开免审规则设置新增模版, 并填充数据信息")]
    [HotUpdate]
    public class Test : AbstractListPlugIn
    {
        public override void EntryButtonCellClick(EntryButtonCellClickEventArgs e)
        {
            base.EntryButtonCellClick(e);

            if (!e.FieldKey.EqualsIgnoreCase("F_ZMER_TEXT_APV"))
                return;

            /*string clientMac = GetClientMac();
            string allowMac = "70-20-84-00-20-26";
            string allowMac1 = "BC-24-11-27-B8-1F";

            if (!clientMac.Equals(allowMac, StringComparison.OrdinalIgnoreCase) ||
                !clientMac.Equals(allowMac1, StringComparison.OrdinalIgnoreCase))
            {
                this.View.ShowErrMessage($"无权限打开该页面！");
                return;
            }*/

            var rowData = this.ListView.CurrentPageRowsInfo
                .FirstOrDefault(p => p.RowKey == e.Row);

            if (rowData == null)
                return;

            string billNo = rowData.BillNo;
            string fid = rowData.PrimaryKeyValue;
            string fentryid = rowData.EntryPrimaryKeyValue;
            string wlId = string.Empty;
            string gysName = string.Empty;
            int qty = 0;
            decimal price = 0;
            string danwei = string.Empty;

            var Query = string.Format("/*dialect*/SELECT B.FMATERIALID AS wlid,S1.FNAME AS GYSNAME,B.FQTY,B1.FTAXPRICE,T.FNAME FROM T_PUR_POORDER A JOIN T_PUR_POORDERENTRY B ON A.FID = B.FID JOIN T_BD_SUPPLIER_L S1 ON A.FSUPPLIERID = S1.FSUPPLIERID JOIN T_PUR_POORDERENTRY_F B1 ON B.FENTRYID = B1.FENTRYID JOIN T_BD_UNIT_L T ON B.FUNITID = T.FUNITID WHERE FBILLNO = '{0}'  AND A.FID = '{1}' AND B.FENTRYID = '{2}'", billNo, fid, fentryid);
            DynamicObjectCollection Result = DBUtils.ExecuteDynamicObject(this.Context, Query);
            if (Result.Count > 0)
            {
                wlId = Result[0]["wlid"].ToString();
                gysName = Result[0]["GYSNAME"].ToString();
                qty = Convert.ToInt32(Result[0]["FQTY"]);
                price = Convert.ToDecimal(Result[0]["FTAXPRICE"]);
                danwei = Result[0]["FNAME"].ToString();

            }

            var showParam = new BillShowParameter
            {
                FormId = "k7078466f851f47eca6565bdb352045d5",
                Status = OperationStatus.ADDNEW,

                Width = 1200,
                Height = 400
            };

            showParam.CustomParams.Add("wlId", wlId);
            showParam.CustomParams.Add("gysName", gysName);
            showParam.CustomParams.Add("qty", qty.ToString());
            showParam.CustomParams.Add("price", price.ToString());
            showParam.CustomParams.Add("billNo", billNo);
            showParam.CustomParams.Add("danwei", danwei);

            this.View.ShowForm(showParam);
        }
        private string GetClientMac()
        {
            var mac = Context.ClientInfo?.MacAddress;
            return FormatMac(mac);
        }


        private string FormatMac(string mac)
        {
            if (string.IsNullOrWhiteSpace(mac))
                return string.Empty;

            mac = Regex.Replace(mac, "[:\\-\\s]", "").ToUpper();

            if (mac.Length < 12)
                return mac;

            return string.Join("-", Enumerable.Range(0, 6)
                .Select(i => mac.Substring(i * 2, 2)));
        }
    }

    [Description("【填充展示数据】接收Zitn_UnAudit_Set_List_PluginNew")]
    [HotUpdate]
    public class UnAuditListClass2 : AbstractBillPlugIn
    {
        public override void AfterCreateNewData(EventArgs e)
        {
            base.AfterCreateNewData(e);

            string billNo = this.View.OpenParameter.GetCustomParameter("billNo")?.ToString();
            string materialId = this.View.OpenParameter.GetCustomParameter("wlId")?.ToString();
            string supName = this.View.OpenParameter.GetCustomParameter("gysName")?.ToString();
            /*int Qty = Convert.ToInt32(this.View.OpenParameter.GetCustomParameter("qty"));
            decimal Price = Convert.ToDecimal(this.View.OpenParameter.GetCustomParameter("price"));*/
            string Qty = this.View.OpenParameter.GetCustomParameter("qty")?.ToString();
            string Price = this.View.OpenParameter.GetCustomParameter("price")?.ToString();
            string danwei = this.View.OpenParameter.GetCustomParameter("danwei")?.ToString();

            this.Model.SetValue("FPURNO", billNo);
            this.Model.SetValue("FSUPPLIER", supName);
            this.Model.SetValue("FQTY", FormatDecimal(Qty, 0));
            this.Model.SetValue("FTAXPRICE", FormatDecimal(Price, 2));
            this.Model.SetValue("FDANWEI", danwei);

            var materialMeta = MetaDataServiceHelper.Load(this.Context, "BD_MATERIAL") as FormMetadata;

            DynamicObjectType materialType = materialMeta.BusinessInfo.GetDynamicObjectType();

            DynamicObject materialObj = BusinessDataServiceHelper.Load(
                this.Context,
                new object[] { materialId },
                materialType
            ).FirstOrDefault();

            this.Model.SetValue("FMATERIALID", materialObj);
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
    }

}
