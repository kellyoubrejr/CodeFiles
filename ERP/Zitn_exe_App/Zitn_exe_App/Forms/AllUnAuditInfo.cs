using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using Zitn_exe_App.Data;

namespace Zitn_exe_App.Forms
{
    public partial class AllUnAuditInfo : Form
    {
        private List<string> _excludedMaterialCodes;

        public AllUnAuditInfo(List<string> excludedMaterialCodes = null)
        {
            InitializeComponent();
            SetGridViewReadOnly();

            _excludedMaterialCodes = excludedMaterialCodes ?? new List<string>();

            btnSearch.Click += new EventHandler(btnSearch_Click);

            // 1. 先加载全部供应商和物料到下拉列表（只执行一次）
            LoadAllSuppliers();
            LoadAllMaterials();

            // 2. 再加载全部数据到表格
            LoadAllUnAuditData();
        }

        /// <summary>
        /// 首次加载全部供应商到下拉列表（只调用一次）
        /// </summary>
        private void LoadAllSuppliers()
        {
            try
            {
                string sql = @"
                    SELECT DISTINCT A.FSUPPLIER AS SUPPLIER
                    FROM ZMER_t_Cust_Entry100101 A
                    ORDER BY A.FSUPPLIER";

                DataTable dt = DbHelper.ExecuteQuery(sql, null);

                comboBox1.Items.Clear();
                //comboBox1.Items.Add("全部");
                comboBox1.Items.Add("");

                foreach (DataRow row in dt.Rows)
                {
                    string supplier = row["SUPPLIER"]?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(supplier))
                    {
                        comboBox1.Items.Add(supplier);
                    }
                }

                comboBox1.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载供应商列表失败：{ex.Message}", "错误");
            }
        }

        /// <summary>
        /// 首次加载全部物料到下拉列表（只调用一次）
        /// </summary>
        private void LoadAllMaterials()
        {
            try
            {
                string sql = @"
                    SELECT DISTINCT M.FNUMBER AS WLNUM
                    FROM ZMER_t_Cust_Entry100101 A
                    JOIN T_BD_MATERIAL M ON A.FMATERIALID = M.FMATERIALID
                    ORDER BY M.FNUMBER";

                DataTable dt = DbHelper.ExecuteQuery(sql, null);

                comboBox2.Items.Clear();
                //comboBox2.Items.Add("全部");
                comboBox2.Items.Add("");

                foreach (DataRow row in dt.Rows)
                {
                    string material = row["WLNUM"]?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(material))
                    {
                        comboBox2.Items.Add(material);
                    }
                }

                comboBox2.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载物料列表失败：{ex.Message}", "错误");
            }
        }

        /// <summary>
        /// 加载免审记录数据
        /// </summary>
        private void LoadAllUnAuditData(string supplierFilter = null, string materialFilter = null)
        {
            try
            {
                string sql = @"
                    SELECT 
                        A.FSUPPLIER AS SUPPLIER,
                        M.FNUMBER AS WLNUM,
                        M1.FNAME AS WLNAME,
                        M1.FSPECIFICATION AS WLSPEC,
                        A.F_ZMER_TEXT_83G AS UP,
                        A.FTAXPRICE,
                        A.FDANWEI AS UNIT
                    FROM ZMER_t_Cust_Entry100101 A
                    JOIN T_BD_MATERIAL M ON A.FMATERIALID = M.FMATERIALID
                    JOIN T_BD_MATERIAL_L M1 ON M.FMATERIALID = M1.FMATERIALID
                    WHERE 1=1";

                var parameters = new Dictionary<string, object>();

                // 排除已选择的物料
                if (_excludedMaterialCodes != null && _excludedMaterialCodes.Count > 0)
                {
                    for (int i = 0; i < _excludedMaterialCodes.Count; i++)
                    {
                        string paramName = $"@excludedCode{i}";
                        sql += $" AND M.FNUMBER != {paramName}";
                        parameters.Add(paramName, _excludedMaterialCodes[i]);
                    }
                }

                // 供应商模糊筛选
                if (!string.IsNullOrWhiteSpace(supplierFilter))
                {
                    sql += " AND A.FSUPPLIER LIKE @supplier";
                    parameters.Add("@supplier", $"%{supplierFilter}%");
                }

                // 物料编码模糊筛选
                if (!string.IsNullOrWhiteSpace(materialFilter))
                {
                    sql += " AND M.FNUMBER LIKE @material";
                    parameters.Add("@material", $"%{materialFilter}%");
                }

                DataTable dt = DbHelper.ExecuteQuery(sql, parameters);
                FillDataGridView(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"查询免审记录失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 填充表格
        /// </summary>
        private void FillDataGridView(DataTable dt)
        {
            dgv_all.Rows.Clear();

            foreach (DataRow row in dt.Rows)
            {
                int rowIndex = dgv_all.Rows.Add();
                dgv_all.Rows[rowIndex].Cells["chkSelect"].Value = false;
                dgv_all.Rows[rowIndex].Cells["supplier"].Value = row["SUPPLIER"]?.ToString() ?? "";
                dgv_all.Rows[rowIndex].Cells["wlnum"].Value = row["WLNUM"]?.ToString() ?? "";
                dgv_all.Rows[rowIndex].Cells["wlname"].Value = row["WLNAME"]?.ToString() ?? "";
                dgv_all.Rows[rowIndex].Cells["wlspec"].Value = row["WLSPEC"]?.ToString() ?? "";
                dgv_all.Rows[rowIndex].Cells["up"].Value = row["UP"]?.ToString() ?? "0";
                dgv_all.Rows[rowIndex].Cells["taxprice"].Value = row["FTAXPRICE"]?.ToString() ?? "0";
                dgv_all.Rows[rowIndex].Cells["unit"].Value = row["UNIT"]?.ToString() ?? "";
            }

            if (dt.Rows.Count == 0)
            {
                string message = _excludedMaterialCodes.Count > 0
                    ? "所有免审物料都已选择完毕！"
                    : "未找到任何免审记录！";
                MessageBox.Show(message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// 搜索按钮 - 根据两个下拉列表筛选
        /// </summary>
        private void btnSearch_Click(object sender, EventArgs e)
        {
            // 获取下拉列表选中的供应商
            string comboSupplier = null;
            if (comboBox1.SelectedIndex > 0)
            {
                comboSupplier = comboBox1.SelectedItem?.ToString();
            }

            // 获取下拉列表选中的物料
            string comboMaterial = null;
            if (comboBox2.SelectedIndex > 0)
            {
                comboMaterial = comboBox2.SelectedItem?.ToString();
            }

            // 如果两个都选"全部"，加载全部数据
            if (string.IsNullOrEmpty(comboSupplier) && string.IsNullOrEmpty(comboMaterial))
            {
                LoadAllUnAuditData();
                this.Text = "全部免审信息";
                return;
            }

            // 执行组合条件查询
            LoadAllUnAuditData(comboSupplier, comboMaterial);
        }

        /// <summary>
        /// 确认按钮 - 返回勾选的数据
        /// </summary>
        private void btnConfirm_Click(object sender, EventArgs e)
        {
            DataTable selectedData = new DataTable();
            selectedData.Columns.Add("供应商", typeof(string));
            selectedData.Columns.Add("物料编码", typeof(string));
            selectedData.Columns.Add("物料名称", typeof(string));
            selectedData.Columns.Add("规格", typeof(string));
            selectedData.Columns.Add("上限", typeof(decimal));
            selectedData.Columns.Add("含税单价", typeof(decimal));
            selectedData.Columns.Add("单位", typeof(string));

            foreach (DataGridViewRow row in dgv_all.Rows)
            {
                if (row.IsNewRow) continue;

                DataGridViewCheckBoxCell checkCell = row.Cells["chkSelect"] as DataGridViewCheckBoxCell;
                if (checkCell != null && Convert.ToBoolean(checkCell.Value) == true)
                {
                    DataRow newRow = selectedData.NewRow();
                    newRow["供应商"] = row.Cells["supplier"].Value?.ToString() ?? "";
                    newRow["物料编码"] = row.Cells["wlnum"].Value?.ToString() ?? "";
                    newRow["物料名称"] = row.Cells["wlname"].Value?.ToString() ?? "";
                    newRow["规格"] = row.Cells["wlspec"].Value?.ToString() ?? "";
                    newRow["上限"] = Convert.ToDecimal(row.Cells["up"].Value ?? 0);
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
                mainForm.AddSelectedAllUnAuditData(selectedData);
            }

            this.Close();
        }

        private void SetGridViewReadOnly()
        {
            dgv_all.ReadOnly = false;
        }
    }
}