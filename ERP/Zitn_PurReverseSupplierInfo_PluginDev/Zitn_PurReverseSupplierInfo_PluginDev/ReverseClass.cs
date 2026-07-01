using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using DynamicObject = Kingdee.BOS.Orm.DataEntity.DynamicObject;

namespace Zitn_PurReverseSupplierInfo_PluginDev
{
    [Description("采购订单审核-反写供应商主数据付款方式字段"), HotUpdate]
    public class ReverseClass : AbstractOperationServicePlugIn
    {
        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);

            if (e.DataEntitys == null || e.DataEntitys.Length == 0)
                return;

            /* ================== 登录 K3Cloud ================== */
            K3CloudApiClient client = new K3CloudApiClient("http://10.0.32.18/k3cloud/");
            var loginResult = client.ValidateLogin(
                "688399bec6449e",
                "admin",
                "Flzx3qc!",
                2052
            );
            //test
            /*K3CloudApiClient client = new K3CloudApiClient("http://127.0.0.1/k3cloud/");
            var loginResult = client.ValidateLogin(
                "6940c27ae377d5",
                "刘总",
                "yangwei11",
                2052
            );*/

            if (JObject.Parse(loginResult)["LoginResultType"].Value<int>() != 1)
            {
                throw new KDBusinessException("LOGIN_FAIL", "K3Cloud 登录失败");
            }

            foreach (DynamicObject purBill in e.DataEntitys)
            {
                /* ================== 获取采购订单信息 ================== */
                string billNo = Convert.ToString(purBill["BillNo"]);

                DynamicObject supplier = purBill["SUPPLIERID"] as DynamicObject;
                string supplierName = supplier?["Name"]?.ToString();

                var fkjhSql = string.Format("/*dialect*/SELECT F_ZMER_COMBO_QTR FROM T_PUR_POORDER A JOIN T_PUR_POORDERINSTALLMENT B1 ON A.FID = B1.FID WHERE FBILLNO = '{0}'", billNo);
                DynamicObjectCollection rows = DBUtils.ExecuteDynamicObject(this.Context, fkjhSql);

                if (rows != null && rows.Count > 0)
                {
                    var rawFkfs = rows[0]["F_ZMER_COMBO_QTR"];
                    if (rawFkfs == null || rawFkfs == DBNull.Value || string.IsNullOrEmpty(rawFkfs.ToString()))
                    {
                        return;
                    }
                    if (!int.TryParse(rawFkfs.ToString(), out int fkfs))
                    {
                        return;
                    }

                    var upSql = string.Format("/*dialect*/UPDATE A SET F_UNW_COMBO_ZC5 = {0}  FROM T_BD_SUPPLIER A JOIN T_BD_SUPPLIER_L B ON A.FSUPPLIERID = B.FSUPPLIERID WHERE FNAME = '{1}'", fkfs, supplierName);
                    DBUtils.Execute(this.Context, upSql);
                }
            }
        }
    }
}
