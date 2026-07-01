using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Zitn_PurApplication_AfterBindData_PluginNew
{
    [Description("【表单插件】采购申请单打开AfterBindData【染色】")]
    public class Class1 : AbstractBillPlugIn
    {
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);


            string userName = this.Context.UserName;

            if (!userName.Equals("刘军"))
            {
                //隐藏单据体行内字段
                this.View.GetControl("F_ZMER_TEXT_6OQ").Visible = false;

                //隐藏单据体菜单按钮
                this.View.GetBarItem("FEntity", "ZMER_tbButton").Visible = false;

                if (userName.Equals("朱艳玲") || userName.Equals("陈传昀") || userName.Equals("孙祥飞") || 
                    userName.Equals("魏红华") || userName.Equals("朱东婷") || userName.Equals("訾豫熙") ) {
                    this.View.GetBarItem("FEntity", "ZMER_tbButton").Visible = true;
                }

                if (userName.Equals("王晓伟") || userName.Equals("刘欣") || userName.Equals("采购总监"))
                {
                    this.View.GetControl("F_ZMER_TEXT_6OQ").Visible = true;
                }
            }

            var currentDate = DateTime.Now.ToString("yyyy-MM-dd");

            var sqDateStr = this.View.Model.GetValue("FAPPLICATIONDATE");

            var sqDate = Convert.ToDateTime(sqDateStr).ToString("yyyy-MM-dd");

            var entityObjs = this.View.Model.GetEntityDataObject(this.View.BusinessInfo.GetEntryEntity("FEntity"));

            for (int i = 0; i < entityObjs.Count; i++)
            {
                var entityObj = entityObjs[i];
                var dhDateStr = entityObj["ARRIVALDATE"];
                var dhDate = Convert.ToDateTime(dhDateStr).ToString("yyyy-MM-dd");
                long entryId = Convert.ToInt64(entityObj["Id"]);
                string wlnum = GetMaterialNumber(i);

                DateTime currentDateTime = Convert.ToDateTime(currentDate);
                DateTime arrivalDateTime = Convert.ToDateTime(dhDate);

                TimeSpan timeSpan = arrivalDateTime - currentDateTime;

                int dayDifference = timeSpan.Days;


                var query = string.Format(@"/*dialect*/SELECT 
                                                            CEILING(AVG(DATEDIFF(DAY, A.FAPPLICATIONDATE, B.FARRIVALDATE))) AS AVGDATE
                                                        FROM
                                                            T_PUR_Requisition A 
                                                        JOIN 
                                                            T_PUR_ReqEntry B ON A.FID = B.FID
                                                        JOIN
                                                            T_BD_MATERIAL M1 ON B.FMATERIALID = M1.FMATERIALID
                                                        WHERE 
                                                            M1.FNUMBER = '{0}'
                                                            AND A.FAPPLICATIONDATE >= DATEADD(YEAR, -1, GETDATE())", wlnum);
                DynamicObjectCollection result = DBUtils.ExecuteDynamicObject(this.Context, query);
                if (result != null && result.Count > 0)
                {
                    var avgDate = Convert.ToInt32(result[0]["AVGDATE"]);

                    //if (DateTime.Compare(Convert.ToDateTime(dayDifference), Convert.ToDateTime(avgDate)) < 0)
                    //{

                    if (dayDifference < avgDate)
                    {
                        //SetEntryBackColor("FEntity", entryId, "#e91010");
                        this.View.Model.SetValue("ffengxian", "存在逾期风险", i);
                        SetCellBackColor("FEntity", entryId, "ffengxian", "#e91010");
                    }
                    else
                    {
                        this.View.Model.SetValue("ffengxian", "正常", i);
                    }

                    //}
                }

            }
        }

        private void SetEntryBackColor(string entryKey, long entryId, string color)
        {
            var grid = this.View.GetControl<EntryGrid>(entryKey);
            if (grid == null)
                return;

            var entity = this.View.BusinessInfo.GetEntity(entryKey);
            if (entity == null)
                return;

            int rowCount = this.Model.GetEntryRowCount(entryKey);

            for (int i = 0; i < rowCount; i++)
            {
                var rowObj = this.Model.GetEntityDataObject(entity, i);
                if (rowObj == null)
                    continue;

                long currentEntryId = Convert.ToInt64(rowObj["Id"]);

                if (currentEntryId == entryId)
                {
                    grid.SetRowBackcolor(color, i);
                    break;
                }
            }
        }

        private string GetMaterialNumber(int rowIndex)
        {
            DynamicObject materialObj = this.View.Model.GetValue("FMaterialId", rowIndex) as DynamicObject;
            if (materialObj == null)
                return string.Empty;

            string materialId = Convert.ToString(materialObj["Id"]);
            if (string.IsNullOrWhiteSpace(materialId))
                return string.Empty;

            string sql = string.Format(
                "/*dialect*/SELECT FNUMBER FROM T_BD_MATERIAL WHERE FMATERIALID = '{0}'",
                materialId);

            DynamicObjectCollection result = DBUtils.ExecuteDynamicObject(this.Context, sql);

            if (result == null || result.Count == 0)
                return string.Empty;

            return Convert.ToString(result[0]["FNUMBER"]);
        }

        private void SetCellBackColor(string entryKey, long entryId, string fieldName, string color)
        {
            SetCellBackColor(entryKey, entryId, new[] { fieldName }, color);
        }
        private void SetCellBackColor(string entryKey, long entryId, IEnumerable<string> fieldNames, string color)
        {
            var grid = this.View.GetControl<EntryGrid>(entryKey);
            if (grid == null || fieldNames == null)
                return;

            var entity = this.View.BusinessInfo.GetEntity(entryKey);
            if (entity == null)
                return;

            int rowCount = this.Model.GetEntryRowCount(entryKey);

            for (int i = 0; i < rowCount; i++)
            {
                var rowObj = this.Model.GetEntityDataObject(entity, i);
                if (rowObj == null)
                    continue;

                long currentEntryId = Convert.ToInt64(rowObj["Id"]);
                if (currentEntryId == entryId)
                {
                    foreach (string fieldName in fieldNames)
                    {
                        if (!string.IsNullOrEmpty(fieldName))
                        {
                            List<KeyValuePair<int, string>> colorList = new List<KeyValuePair<int, string>>();
                            colorList.Add(new KeyValuePair<int, string>(i, color));

                            grid.SetCellsBackcolor(fieldName, colorList);
                        }
                    }
                    break;
                }
            }
        }
    }
}
