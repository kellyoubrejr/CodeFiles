using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using Zitn_exe_App.Data;

namespace Zitn_exe_App.Forms
{
    public partial class HistoryForms : Form
    {
        private List<string> _excludedMaterialIds;
        public HistoryForms(string supplierName, List<string> excludedMaterialIds = null)
        {
            InitializeComponent();
            dgv_gys.Columns["wlnum"].ReadOnly = true;
            dgv_gys.Columns["wlname"].ReadOnly = true;
            dgv_gys.Columns["wlspec"].ReadOnly = true;
            dgv_gys.Columns["qty"].ReadOnly = true;
            dgv_gys.Columns["taxprice"].ReadOnly = true;
            _excludedMaterialIds = excludedMaterialIds ?? new List<string>();
            lblSupplierName.Text = supplierName;
            LoadMaterialData(supplierName);
        }

        /// <summary>
        /// 根据供应商名称加载历史采购数据（排除已选择的物料）
        /// </summary>
        private void LoadMaterialData(string supplierName)
        {
            try
            {
                string sql = @"
            SELECT DISTINCT
                B.FMATERIALID AS wlid,M.FNUMBER AS MaterialCode,
                M1.FNAME AS WLNAME,
                M1.FSPECIFICATION AS WLSPEC,
                B.FQTY,
                B1.FTAXPRICE 
            FROM T_PUR_POORDER A
            JOIN T_PUR_POORDERENTRY B ON A.FID = B.FID
            JOIN T_BD_SUPPLIER_L S1 ON A.FSUPPLIERID = S1.FSUPPLIERID
            JOIN T_PUR_POORDERENTRY_F B1 ON B.FENTRYID = B1.FENTRYID 
            JOIN T_BD_MATERIAL_L M1 ON B.FMATERIALID = M1.FMATERIALID
            JOIN T_BD_MATERIAL M ON M.FMATERIALID = M1.FMATERIALID
            WHERE S1.FNAME = @supplierName";

                // ========== 新增：排除已选择的物料编码 ==========
                if (_excludedMaterialIds != null && _excludedMaterialIds.Count > 0)
                {
                    // 构建 IN 查询的参数列表
                    var excludedParams = new Dictionary<string, object>();
                    excludedParams.Add("@supplierName", supplierName);

                    string excludedCondition = "";
                    for (int i = 0; i < _excludedMaterialIds.Count; i++)
                    {
                        string paramName = $"@excludedId{i}";
                        excludedCondition += $", {paramName}";
                        excludedParams.Add(paramName, _excludedMaterialIds[i]);
                    }

                    // 去掉开头的逗号
                    excludedCondition = excludedCondition.TrimStart(',');
                    sql += $" AND M.FNUMBER NOT IN ({excludedCondition})";

                    // 执行查询
                    DataTable dt = DbHelper.ExecuteQuery(sql, excludedParams);

                    // 填充数据
                    FillDataGridView(dt, supplierName);
                }
                else
                {
                    // 没有需要排除的数据，直接查询
                    var parameters = new Dictionary<string, object>
            {
                { "@supplierName", supplierName }
            };
                    DataTable dt = DbHelper.ExecuteQuery(sql, parameters);
                    FillDataGridView(dt, supplierName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"查询历史数据失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 填充 DataGridView
        /// </summary>
        private void FillDataGridView(DataTable dt, string supplierName)
        {
            // 清空表格现有数据
            dgv_gys.Rows.Clear();

            // 循环遍历查询结果，添加到 dgv_gys
            foreach (DataRow row in dt.Rows)
            {
                int rowIndex = dgv_gys.Rows.Add();

                // 复选框默认不选中
                dgv_gys.Rows[rowIndex].Cells["chkSelect"].Value = false;
                // 物料编码
                dgv_gys.Rows[rowIndex].Cells["wlnum"].Value = row["MaterialCode"]?.ToString() ?? "";
                // 物料名称
                dgv_gys.Rows[rowIndex].Cells["wlname"].Value = row["WLNAME"]?.ToString() ?? "";
                // 规格
                dgv_gys.Rows[rowIndex].Cells["wlspec"].Value = row["WLSPEC"]?.ToString() ?? "";
                // 采购数量
                dgv_gys.Rows[rowIndex].Cells["qty"].Value = Convert.ToDecimal(row["FQTY"] ?? 0).ToString("F0");
                // 含税单价
                dgv_gys.Rows[rowIndex].Cells["taxprice"].Value = Convert.ToDecimal(row["FTAXPRICE"] ?? 0).ToString("F2");
            }

            // 如果没有查询到数据，提示用户
            if (dt.Rows.Count == 0)
            {
                string message = _excludedMaterialIds.Count > 0
                    ? $"供应商【{supplierName}】的所有历史物料都已选择完毕！"
                    : $"未找到供应商【{supplierName}】的历史采购记录！";
                MessageBox.Show(message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            DataTable selectedData = new DataTable();
            selectedData.Columns.Add("物料编码", typeof(string));
            selectedData.Columns.Add("物料名称", typeof(string));
            selectedData.Columns.Add("规格", typeof(string));
            selectedData.Columns.Add("采购数量", typeof(decimal));
            selectedData.Columns.Add("含税单价", typeof(decimal));

            foreach (DataGridViewRow row in dgv_gys.Rows)
            {
                if (row.IsNewRow) continue;

                DataGridViewCheckBoxCell checkCell = row.Cells["chkSelect"] as DataGridViewCheckBoxCell;
                if (checkCell != null && Convert.ToBoolean(checkCell.Value) == true)
                {
                    DataRow newRow = selectedData.NewRow();
                    newRow["物料编码"] = row.Cells["wlnum"].Value?.ToString() ?? "";
                    newRow["物料名称"] = row.Cells["wlname"].Value?.ToString() ?? "";
                    newRow["规格"] = row.Cells["wlspec"].Value?.ToString() ?? "";
                    newRow["采购数量"] = Convert.ToDecimal(row.Cells["qty"].Value ?? 0);
                    newRow["含税单价"] = Convert.ToDecimal(row.Cells["taxprice"].Value ?? 0);
                    selectedData.Rows.Add(newRow);
                }
            }

            if (selectedData.Rows.Count == 0)
            {
                MessageBox.Show("请至少选择一条物料数据！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            IndexForm mainForm = this.Owner as IndexForm;
            if (mainForm != null)
            {
                string supplierName = lblSupplierName.Text;  
                mainForm.AddSelectedData(selectedData, supplierName);
            }

            //MessageBox.Show($"已返回 {selectedData.Rows.Count} 条数据！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();

        }

        private void HistoryForms_Load(object sender, EventArgs e)
        {

        }

        private void dgv_gys_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
