using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System.ComponentModel;

namespace Zitn_PoSupplierFKFSDataChanged_Plugin
{
    [Description("【表单插件datachanged】采购根据供应商携带付款方式到付款计划中字段")]
    [HotUpdate]
    public class Class1 : AbstractBillPlugIn
    {
        const string fieldKey = "F_ZMER_Combo_qtr";

        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);

            if (e.Field.Key.EqualsIgnoreCase("FSUPPLIERID"))
            {
                if (e.NewValue != null)
                {
                    string supplierId = string.Empty;

                    if (e.NewValue is DynamicObject)
                    {
                        var supplier = e.NewValue as DynamicObject;
                        supplierId = supplier["Id"]?.ToString();
                    }
                    else
                    {
                        supplierId = e.NewValue.ToString();
                    }

                    if (!string.IsNullOrEmpty(supplierId))
                    {
                        var sql = string.Format($"/*dialect*/select F_UNW_Combo_zc5 from T_BD_SUPPLIER where fsupplierid = {supplierId} AND FUSEORGID = 101006");
                        var result = DBUtils.ExecuteDynamicObject(this.Context, sql);
                        if (result != null && result.Count > 0)
                        {
                            var fkfs = result[0]["F_UNW_Combo_zc5"]?.ToString();

                            if (!string.IsNullOrEmpty(fkfs))
                            {
                                this.View.Model.SetValue(fieldKey, fkfs);
                                this.View.UpdateView(fieldKey);
                            }
                        }
                    }
                }
            }
        }
    }
}