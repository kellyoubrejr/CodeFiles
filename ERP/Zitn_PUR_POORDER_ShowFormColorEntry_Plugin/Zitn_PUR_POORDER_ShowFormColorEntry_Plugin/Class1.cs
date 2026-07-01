using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Zitn_PUR_POORDER_ShowFormColorEntry_Plugin
{

    [Description("【表单插件】单据体最后一行背景色")]
    public class Class1 : AbstractBillPlugIn
    {
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);

            string[] entityKeys = new string[]
            {
            "F_ZMER_Entity_qhl",
            "F_ZMER_Entity_9ra",
            "F_ZMER_Entity_r2z"
            };

            string backColor = "#FFE699";

            foreach (var entityKey in entityKeys)
            {
                SetLastRowBackColor(entityKey, backColor);
            }
        }

        /// <summary>
        /// 给指定单据体的最后一行设置背景色
        /// </summary>
        private void SetLastRowBackColor(string entityKey, string color)
        {
            var grid = this.View.GetControl<EntryGrid>(entityKey);
            if (grid == null)
                return;

            int rowCount = this.Model.GetEntryRowCount(entityKey);
            if (rowCount <= 0)
                return;

            int lastRowIndex = rowCount - 1;

            var colors = new List<KeyValuePair<int, string>>
        {
            new KeyValuePair<int, string>(lastRowIndex, color)
        };

            grid.SetRowBackcolor(colors);
        }
    }
}
