using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zitn_exe_App.Data;

namespace Zitn_exe_App.Forms
{
    public partial class SupplierUnAuditInfo : Form
    {
        private List<string> _excludedMaterialCodes;
        public SupplierUnAuditInfo(string supplierName, List<string> excludedMaterialCodes = null)
        {
            InitializeComponent();
            dgv_unAudit.Columns["wlnum"].ReadOnly = true;
            dgv_unAudit.Columns["wlname"].ReadOnly = true;
            dgv_unAudit.Columns["wlspec"].ReadOnly = true;
            dgv_unAudit.Columns["up"].ReadOnly = true;
            dgv_unAudit.Columns["taxprice"].ReadOnly = true;
            dgv_unAudit.Columns["unit"].ReadOnly = true;
            _excludedMaterialCodes = excludedMaterialCodes ?? new List<string>();
            lblSupplierName.Text = supplierName;
            this.txtb_wl.KeyPress += new KeyPressEventHandler(txtb_wl_KeyPress);
            this.btnSearch.Click += new EventHandler(btnSearch_Click);
            LoadUnAuditData(supplierName);
        }

        /// <summary>
        /// 根据供应商名称加载免审记录数据（排除已选择的物料）
        /// </summary>
        private void LoadUnAuditData(string supplierName)
        {
            try
            {
                string sql = @"
                    SELECT 
                        M.FNUMBER AS WLNUM,
                        M1.FNAME AS WLNAME,
                        M1.FSPECIFICATION AS WLSPEC,
                        A.F_ZMER_TEXT_83G AS QTY,
                        A.FTAXPRICE,
                        A.FDANWEI AS UNIT
                    FROM ZMER_t_Cust_Entry100101 A
                    JOIN T_BD_MATERIAL M ON A.FMATERIALID = M.FMATERIALID
                    JOIN T_BD_MATERIAL_L M1 ON M.FMATERIALID = M1.FMATERIALID
                    WHERE A.FSUPPLIER = @supplierName";

                var parameters = new Dictionary<string, object>();
                parameters.Add("@supplierName", supplierName);

                // 如果有需要排除的物料编码，添加排除条件
                if (_excludedMaterialCodes != null && _excludedMaterialCodes.Count > 0)
                {
                    for (int i = 0; i < _excludedMaterialCodes.Count; i++)
                    {
                        string paramName = $"@excludedCode{i}";
                        sql += $" AND M.FNUMBER != {paramName}";
                        parameters.Add(paramName, _excludedMaterialCodes[i]);
                    }
                }

                DataTable dt = DbHelper.ExecuteQuery(sql, parameters);
                FillDataGridView(dt, supplierName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"查询免审记录失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 填充 DataGridView
        /// </summary>
        private void FillDataGridView(DataTable dt, string supplierName)
        {
            // 清空表格现有数据
            dgv_unAudit.Rows.Clear();

            // 循环遍历查询结果，添加到 dgv_unAudit
            foreach (DataRow row in dt.Rows)
            {
                int rowIndex = dgv_unAudit.Rows.Add();

                // 复选框默认不选中
                dgv_unAudit.Rows[rowIndex].Cells["chkSelect"].Value = false;
                // 物料编码
                dgv_unAudit.Rows[rowIndex].Cells["wlnum"].Value = row["WLNUM"]?.ToString() ?? "";
                // 物料名称
                dgv_unAudit.Rows[rowIndex].Cells["wlname"].Value = row["WLNAME"]?.ToString() ?? "";
                // 规格
                dgv_unAudit.Rows[rowIndex].Cells["wlspec"].Value = row["WLSPEC"]?.ToString() ?? "";
                // 采购数量
                dgv_unAudit.Rows[rowIndex].Cells["up"].Value = row["QTY"]?.ToString() ?? "0";
                // 含税单价
                dgv_unAudit.Rows[rowIndex].Cells["taxprice"].Value = row["FTAXPRICE"]?.ToString() ?? "0";
                // 单位
                dgv_unAudit.Rows[rowIndex].Cells["unit"].Value = row["UNIT"]?.ToString() ?? "";
            }

            // 如果没有查询到数据，提示用户
            if (dt.Rows.Count == 0)
            {
                string message = _excludedMaterialCodes.Count > 0
                    ? $"供应商【{supplierName}】的所有免审物料都已选择完毕！"
                    : $"未找到供应商【{supplierName}】的免审记录！";
                MessageBox.Show(message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// 确认按钮点击事件
        /// </summary>
        private void btnConfirm_Click(object sender, EventArgs e)
        {
            DataTable selectedData = new DataTable();
            selectedData.Columns.Add("物料编码", typeof(string));
            selectedData.Columns.Add("物料名称", typeof(string));
            selectedData.Columns.Add("规格", typeof(string));
            selectedData.Columns.Add("采购数量", typeof(decimal));
            selectedData.Columns.Add("含税单价", typeof(decimal));
            selectedData.Columns.Add("单位", typeof(string));

            foreach (DataGridViewRow row in dgv_unAudit.Rows)
            {
                if (row.IsNewRow) continue;

                DataGridViewCheckBoxCell checkCell = row.Cells["chkSelect"] as DataGridViewCheckBoxCell;
                if (checkCell != null && Convert.ToBoolean(checkCell.Value) == true)
                {
                    DataRow newRow = selectedData.NewRow();
                    newRow["物料编码"] = row.Cells["wlnum"].Value?.ToString() ?? "";
                    newRow["物料名称"] = row.Cells["wlname"].Value?.ToString() ?? "";
                    newRow["规格"] = row.Cells["wlspec"].Value?.ToString() ?? "";
                    newRow["采购数量"] = Convert.ToDecimal(row.Cells["up"].Value ?? 0);
                    newRow["含税单价"] = Convert.ToDecimal(row.Cells["taxprice"].Value ?? 0);
                    newRow["单位"] = row.Cells["unit"].Value?.ToString() ?? "";
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
                mainForm.AddSelectedUnAuditData(selectedData, supplierName);
            }

            this.Close();
        }

        /// <summary>
        /// 搜索按钮点击事件 - 模糊筛选物料编码
        /// </summary>
        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchText = txtb_wl.Text.Trim();

            if (string.IsNullOrEmpty(searchText))
            {
                
            }
            else
            {
                FilterByMaterialCode(searchText);
            }
        }

        /// <summary>
        /// 根据物料编码模糊筛选 DataGridView
        /// </summary>
        /// <param name="searchText">搜索关键词</param>
        private void FilterByMaterialCode(string searchText)
        {
            int visibleCount = 0;
            int totalCount = 0;

            foreach (DataGridViewRow row in dgv_unAudit.Rows)
            {
                if (row.IsNewRow) continue;

                totalCount++;
                string wlnum = row.Cells["wlnum"].Value?.ToString() ?? "";

                bool isMatch = wlnum.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;

                row.Visible = isMatch;

                if (isMatch)
                {
                    visibleCount++;
                }
            }

            /*if (visibleCount == 0)
            {
                MessageBox.Show($"未找到包含【{searchText}】的物料编码！", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                this.Text = $"供应商免审信息 - 找到 {visibleCount} 条记录（共 {totalCount} 条）";
            }*/
        }

        /// <summary>
        /// 可选：在 txtb_wl 文本框中按回车键触发搜索
        /// </summary>
        private void txtb_wl_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                btnSearch_Click(sender, e);
                e.Handled = true;
            }
        }
    }
}
