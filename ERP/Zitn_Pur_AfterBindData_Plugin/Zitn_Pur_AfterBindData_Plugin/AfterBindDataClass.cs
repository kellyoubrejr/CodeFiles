using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.ComponentModel;
using System.Linq;

namespace Zitn_Pur_AfterBindData_Plugin
{
    [Description("【表单插件】采购订单打开AfterBindData【染色】")]
    public class AfterBindDataClass : AbstractBillPlugIn
    {
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);

            string userName = this.Context.UserName;

            // 🔹 用户权限控制
            if (!userName.Equals("刘军"))
            {
                this.View.GetControl<Button>("F_ZMER_Button_qtr").Visible = false;
                this.View.GetControl("F_ZMER_TEXT_APV").Visible = false;
                this.View.GetControl("F_ZMER_TEXT_6OQ").Visible = false;

                if (userName.Equals("王晓伟") || userName.Equals("刘欣") || userName.Equals("采购总监")||
                    userName.Equals("朱培康") || userName.Equals("郭庆") || userName.Equals("何俊义")||
                    userName.Equals("牟少容") || userName.Equals("王舒丙") || userName.Equals("于宝阳") || userName.Equals("王若歆")
                    )
                {
                    this.View.GetControl("F_ZMER_TEXT_6OQ").Visible = true;
                }


            }

            string billNo = string.Empty;
            DynamicObject headObj = this.View.Model.DataObject as DynamicObject;
            if (headObj != null)
            {
                billNo = Convert.ToString(headObj["BillNo"]);
            }

            if (string.IsNullOrEmpty(billNo))
                return; // 防止空单据号

            var entitySql = string.Format("/*dialect*/SELECT FSUPPLIERID,FMATERIALID,FQTY,FTAXPRICE,B.FENTRYID FROM T_PUR_POORDER A JOIN T_PUR_POORDERENTRY B ON A.FID = B.FID JOIN T_PUR_POORDERENTRY_F B1 ON B.FENTRYID = B1.FENTRYID WHERE FBILLNO = '{0}'", billNo);
            DynamicObjectCollection result = DBUtils.ExecuteDynamicObject(this.Context, entitySql);
            if (result != null && result.Count > 0)
            {
                for (int i = 0; i < result.Count; i++)
                {
                    string supplierId = Convert.ToString(result[i]["FSUPPLIERID"]);
                    string materialId = Convert.ToString(result[i]["FMATERIALID"]);
                    long qty = Convert.ToInt64(result[i]["FQTY"]);
                    decimal taxPrice = Convert.ToDecimal(result[i]["FTAXPRICE"]);
                    long entryId = Convert.ToInt64(result[i]["FENTRYID"]);

                    var sql = string.Format(@"/*dialect*/SELECT * FROM ZMER_t_Cust_Entry100101 A JOIN T_BD_SUPPLIER_L B ON A.FSUPPLIER = B.FNAME 
WHERE B.FSUPPLIERID = '{0}' AND FMATERIALID = '{1}' AND {2} <= F_ZMER_TEXT_83G AND {2} > F_ZMER_TEXT_QTR AND {3}>= CONVERT(DECIMAL(18,2), FTAXPRICE)", supplierId, materialId, qty, taxPrice);
                    DynamicObjectCollection coll = DBUtils.ExecuteDynamicObject(this.Context, sql);
                    if (coll != null && coll.Count > 0)
                    {
                        this.View.GetControl<Button>("F_ZMER_Button_qtr").Enabled = false;
                    }
                }
            }


            string gysName = GetSupplierName();

            var entity = this.View.BillBusinessInfo.GetEntity("FPOOrderEntry");
            var entityObjs = this.View.Model.GetEntityDataObject(entity);

            // 🔹 调用存储过程，一次返回整单明细免审结果
            var spResult = DBUtils.ExecuteDynamicObject(this.Context,
                $"EXEC ZMER_JudgeIsAudit_Entry @BillNo='{billNo}'");

            for (int i = 0; i < entityObjs.Count; i++)
            {
                int entryId = Convert.ToInt32(entityObjs[i]["ID"]);

                // 🔹 查找当前行的存储过程返回结果
                var rowResult = spResult.FirstOrDefault(r => Convert.ToInt32(r["EntryID"]) == entryId);
                if (rowResult != null && Convert.ToInt32(rowResult["Result"]) == 1)
                {
                    SetEntryBackColor("FPOOrderEntry", entryId, "#C6EFCE"); // ✅ 绿色标记免审
                }
            }
        }

        // 🔹 行染色方法
        private void SetEntryBackColor(string entryKey, long entryId, string color)
        {
            var grid = this.View.GetControl<EntryGrid>(entryKey);
            if (grid == null) return;

            var entity = this.View.BusinessInfo.GetEntity(entryKey);
            if (entity == null) return;

            int rowCount = this.Model.GetEntryRowCount(entryKey);

            for (int i = 0; i < rowCount; i++)
            {
                var rowObj = this.Model.GetEntityDataObject(entity, i);
                if (rowObj == null) continue;

                long currentEntryId = Convert.ToInt64(rowObj["Id"]);
                if (currentEntryId == entryId)
                {
                    grid.SetRowBackcolor(color, i);
                    break;
                }
            }
        }

        // 🔹 获取供应商名称
        private string GetSupplierName()
        {
            DynamicObject gysObj = this.View.Model.GetValue("FSupplierId") as DynamicObject;
            if (gysObj == null) return string.Empty;

            string gysId = Convert.ToString(gysObj["Id"]);
            if (string.IsNullOrWhiteSpace(gysId)) return string.Empty;

            string sql = $"/*dialect*/SELECT FNAME FROM T_BD_SUPPLIER_L WHERE FSUPPLIERID = '{gysId}'";
            DynamicObjectCollection result = DBUtils.ExecuteDynamicObject(this.Context, sql);

            return (result != null && result.Count > 0) ? Convert.ToString(result[0]["FNAME"]) : string.Empty;
        }

        // 🔹 获取物料编号
        private string GetMaterialNumber(int rowIndex)
        {
            DynamicObject materialObj = this.View.Model.GetValue("FMaterialId", rowIndex) as DynamicObject;
            if (materialObj == null) return string.Empty;

            string materialId = Convert.ToString(materialObj["Id"]);
            if (string.IsNullOrWhiteSpace(materialId)) return string.Empty;

            string sql = $"/*dialect*/SELECT FNUMBER FROM T_BD_MATERIAL WHERE FMATERIALID = '{materialId}'";
            DynamicObjectCollection result = DBUtils.ExecuteDynamicObject(this.Context, sql);

            return (result != null && result.Count > 0) ? Convert.ToString(result[0]["FNUMBER"]) : string.Empty;
        }

        // 🔹 获取物料ID
        private string GetMaterialId(int i)
        {
            DynamicObject materialObj = this.View.Model.GetValue("FMaterialId", i) as DynamicObject;
            return (materialObj != null) ? Convert.ToString(materialObj["Id"]) : string.Empty;
        }
    }
}