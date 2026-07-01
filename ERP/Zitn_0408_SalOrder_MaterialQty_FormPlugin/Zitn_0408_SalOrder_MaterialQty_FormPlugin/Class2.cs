using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.ComponentModel;

namespace Zitn_0408_SalOrder_MaterialQty_FormPlugin
{
    [Description("【表单插件datachanged】0408测试，调拨申请单订单根据物料带出数量")]
    [HotUpdate]
    public class Class2 : AbstractBillPlugIn
    {
        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);

            if (e.Field.Key.EqualsIgnoreCase("FMaterialId"))
            {
                int rowIndex = e.Row;

                if (rowIndex < 0) return;

                DynamicObject materialObj = this.View.Model.GetValue("FMaterialId", rowIndex) as DynamicObject;
                string wlid = materialObj != null ? materialObj["Id"].ToString() : "";

                if (!string.IsNullOrEmpty(wlid))
                {
                    var result = DBUtils.ExecuteDataSet(this.Context, $"EXEC qlcx {wlid}");
                    if (result.Tables.Count > 0 && result.Tables[0].Rows.Count > 0)
                    {
                        decimal kysl = 0;
                        if (result.Tables[0].Rows[0]["kysl"] != null && result.Tables[0].Rows[0]["kysl"] != DBNull.Value)
                        {
                            kysl = Convert.ToDecimal(result.Tables[0].Rows[0]["kysl"]);
                        }

                        this.View.Model.SetValue("F_KYS", kysl, rowIndex);
                    }
                }
                this.View.Model.Save();
            }
        }
    }
}