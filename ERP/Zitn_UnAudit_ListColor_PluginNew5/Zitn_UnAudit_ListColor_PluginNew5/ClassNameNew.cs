using System.ComponentModel;
using System.Drawing;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;

namespace Zitn_UnAudit_ListColor_PluginNew5
{
    [Description("列表颜色 + 排序【新0625】")]
    [Kingdee.BOS.Util.HotUpdate]
    public class ClassNameNew : AbstractListPlugIn
    {
        /// <summary>
        /// 排序：让标记行排在最前
        /// </summary>
        public override void PrepareFilterParameter(FilterArgs e)
        {
            base.PrepareFilterParameter(e);

            string userName = this.Context.UserName;
            if (userName == "刘军")
            {
                if (string.IsNullOrWhiteSpace(e.SortString))
                {
                    e.SortString = "FCOLORFLAG DESC";
                }
                else
                {
                    e.SortString = "FCOLORFLAG DESC," + e.SortString;
                }
            }
        }

        /// <summary>
        /// 行颜色控制
        /// </summary>
        public override void OnFormatRowConditions(ListFormatConditionArgs args)
        {
            base.OnFormatRowConditions(args);

            string userName = this.Context.UserName;
            if (userName == "刘军" || userName == "admin" || userName == "采购总监")
            {

                string docStatus = args.DataRow["FDOCUMENTSTATUS"].ToString();

                if (docStatus == "B" || docStatus == "b")
                {

                    string entryId = args.DataRow["t3_FENTRYID"].ToString();

                    var noSql = string.Format(@"
                                        /*dialect*/
                                        SELECT FCOLORFLAG ,FTIME
                                        FROM T_PUR_POORDERENTRY 
                                        WHERE FENTRYID = '{0}'", entryId);

                    DynamicObjectCollection result = DBUtils.ExecuteDynamicObject(this.Context, noSql);

                    if (result != null && result.Count > 0)
                    {
                        string flag = null;
                        if (result[0]["FCOLORFLAG"] != null)
                            flag = result[0]["FCOLORFLAG"].ToString();

                        string time = null;
                        if (result[0]["FTIME"] != null)
                            time = result[0]["FTIME"].ToString();

                        if (!string.IsNullOrEmpty(time) || !string.IsNullOrEmpty(flag))
                        {

                            if (flag == "存在免审记录")
                            {
                                FormatCondition fc = new FormatCondition
                                {
                                    ApplayRow = true,
                                    BackColor = ColorTranslator.ToHtml(Color.LightGreen)
                                };

                                args.FormatConditions.Add(fc);
                            }
                            if (flag == "存在免审但不满足条件")
                            {
                                FormatCondition fc = new FormatCondition
                                {
                                    ApplayRow = true,
                                    BackColor = ColorTranslator.ToHtml(Color.LightBlue)
                                };

                                args.FormatConditions.Add(fc);
                            }
                            if (flag == "命中不符合规则")
                            {
                                FormatCondition fc = new FormatCondition
                                {
                                    ApplayRow = true,
                                    BackColor = ColorTranslator.ToHtml(Color.LightBlue)
                                };

                                args.FormatConditions.Add(fc);
                            }
                            if (flag == "符合规则")
                            {
                                FormatCondition fc = new FormatCondition
                                {
                                    ApplayRow = true,
                                    BackColor = ColorTranslator.ToHtml(Color.LightGreen)
                                };

                                args.FormatConditions.Add(fc);
                            }

                        }
                    }
                }
            }
        }
    }
}