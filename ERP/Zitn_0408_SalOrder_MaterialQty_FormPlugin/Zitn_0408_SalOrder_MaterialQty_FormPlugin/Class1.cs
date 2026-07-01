using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.ComponentModel;

namespace Zitn_0408_SalOrder_MaterialQty_FormPlugin
{
    [Description("【表单插件】0408测试，调拨申请单订单根据物料带出数量")]
    [HotUpdate]
    public class Class1 : AbstractBillPlugIn
    {
        public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
        {
            base.AfterBarItemClick(e);

            if (e.BarItemKey.Equals("tbSplitSave", StringComparison.OrdinalIgnoreCase))
            {
                var entity = this.View.BillBusinessInfo.GetEntity("FEntity");
                var entityObjs = this.View.Model.GetEntityDataObject(entity);

                for (int i = 0; i < entityObjs.Count; i++)
                {
                    DynamicObject materialObj = this.Model.GetValue("FMaterialId", i) as DynamicObject;
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

                            this.Model.SetValue("F_KYS", kysl, i);
                        }
                    }
                }
                this.View.Model.Save();
            }
        }
    }
}