/*
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Zitn_PUR_ShowFrom_Submit2_Plugin
{
    [Description("【单据插件】相似物料行背景色高亮")]
    [HotUpdate]
    public class SimilarMaterialRowColorPlugin : AbstractBillPlugIn
    {
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);

            // 采购价目信息
            SetRowBackColorByFirstColumn(
                "F_ZMER_Entity_qhl",
                "F_ZMER_TEXT_ACP",
                "相似物料采购价目信息",
                "#DCEBFF"
            );

            // 历史采购信息
            SetRowBackColorByFirstColumn(
                "F_ZMER_Entity_r2z",
                "F_ZMER_TEXT_1LD",
                "相似物料历史采购信息",
                "#DCEBFF"
            );

            // 询价记录
            SetRowBackColorByFirstColumn(
                "F_ZMER_Entity_9ra",
                "F_ZMER_TEXT_N1V",
                "相似物料采购询价记录",
                "#DCEBFF"
            );
        }

        /// <summary>
        /// 根据单据体第一列字段的值，设置匹配行的背景色
        /// </summary>
        /// <param name="entityKey">单据体标识</param>
        /// <param name="firstColumnFieldKey">第一列字段标识</param>
        /// <param name="matchText">需要匹配的文本内容</param>
        /// <param name="color">颜色（如 #DCEBFF / #0000FF）</param>
        private void SetRowBackColorByFirstColumn(
            string entityKey,
            string firstColumnFieldKey,
            string matchText,
            string color)
        {
            var grid = this.View.GetControl<EntryGrid>(entityKey);
            if (grid == null)
                return;

            int rowCount = this.Model.GetEntryRowCount(entityKey);
            if (rowCount <= 0)
                return;

            var colors = new List<KeyValuePair<int, string>>();

            for (int row = 0; row < rowCount; row++)
            {
                var value = this.Model.GetValue(firstColumnFieldKey, row);
                string text = value == null ? string.Empty : value.ToString();

                if (text == matchText)
                {
                    colors.Add(new KeyValuePair<int, string>(row, color));
                }
            }

            if (colors.Count > 0)
            {
                grid.SetRowBackcolor(colors);
            }
        }
    }
}
*/