using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Zitn_MO_Selection_Plugin
{
    [Description("生产订单根据单据类型动态渲染下拉列表"), HotUpdate]
    public class FillComboFieldPlugIn : AbstractBillPlugIn
    {
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);

            BindComboField();

        }

        private void BindComboField()
        {
            var levelEnumObjects = (DynamicObjectCollection)((ComboField)this.View.BillBusinessInfo.GetField("F_ZMER_Combo_qtr")).EnumObject["Items"];

            var enumList = new List<EnumItem>();

            var billTypeId = string.Empty;

            var billTypeObj = this.Model.GetValue("FBILLTYPE") as DynamicObject;

            if (billTypeObj != null)
            {
                billTypeId = billTypeObj[0].ToString();
            }

            //0e74146732c24bec90178b6fe16a2d1c 返工
            //5db962f0046f0a 测试

            //0e74146732c24bec90178b6fe16a2d1c

            //68be9d106f2df5

            if (billTypeId == "0e74146732c24bec90178b6fe16a2d1c") // 汇报入库-返工生产
            {
                var enumObjects = levelEnumObjects.Where(o => Convert.ToInt32(o["Value"]) < 5).Select(o => (EnumItem)o).ToArray();

                enumList.AddRange(enumObjects);
            }
            else if (billTypeId == "68be9d106f2df5") // 测试-升级
            {
                var enumObjects = levelEnumObjects.Where(o => Convert.ToInt32(o["Value"]) > 4).Select(o => (EnumItem)o).ToArray();

                enumList.AddRange(enumObjects);
            }

            var comboList = this.View.GetFieldEditor<ComboFieldEditor>("F_ZMER_Combo_qtr", 0);

            if (comboList != null)
            {
                comboList.SetComboItems(enumList);
            }
        }
    }
}
