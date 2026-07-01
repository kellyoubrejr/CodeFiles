using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Zitn_exe_App.Data;
using Zitn_exe_App.Utils;
using Zitn_exe_App.Models;
using Zitn_exe_App.Services;

namespace Zitn_exe_App.Forms
{
    public partial class IndexForm : Form
    {
        private RuleService _ruleService = new RuleService();

        private string _param;

        private BindingList<RuleDto> _list;

        private readonly DataService _service = new DataService();

        private List<RuleDto> _originalData;

        private List<RuleDto> _incomingOriginalData;

        public class SupplierMaterial
        {
            public string Supplier { get; set; }

            public string MaterialId { get; set; }

            public string MaterialCode { get; set; }

            public string MaterialName { get; set; }

            public string MaterialSpec { get; set; }

            public string Unit { get; set; }

            public bool IsModified { get; set; }

            public bool SkipValidate { get; set; }
            public bool IsSaved { get; set; }
        }
        public List<SupplierMaterial> Suppliers = new List<SupplierMaterial>();
        private int index = 0;

        public IndexForm(string param)
        {
            InitializeComponent();
            _param = param;
        }

        private void IndexForm_Load(object sender, EventArgs e)
        {
            InitGrid();

            LoadData();

            _originalData = _list.Select(item => new RuleDto
            {
                MaterialId = item.MaterialCode,
                MaterialName = item.MaterialName,
                MaterialSpec = item.MaterialSpec,
                Supplier = item.Supplier,
                Down = item.Down,
                Up = item.Up,
                Qty = item.Qty,
                Price = item.Price,
                Unit = item.Unit,
                Source = item.Source
            }).ToList();

        }

        /// <summary>
        /// 初始化表格
        /// </summary>
        private void InitGrid()
        {
            dgvMain.AutoGenerateColumns = false;

            dgvMain.Columns["MaterialId"].DataPropertyName = "MaterialCode";
            dgvMain.Columns["MaterialName"].DataPropertyName = "MaterialName";
            dgvMain.Columns["MaterialSpec"].DataPropertyName = "MaterialSpec";
            dgvMain.Columns["Supplier"].DataPropertyName = "Supplier";
            //dgvMain.Columns["Down"].DataPropertyName = "Down";
            dgvMain.Columns["Up"].DataPropertyName = "Up";
            dgvMain.Columns["Qty"].DataPropertyName = "Qty";
            dgvMain.Columns["Price"].DataPropertyName = "Price";
            dgvMain.Columns["Unit"].DataPropertyName = "Unit";
            dgvMain.Columns["Source"].DataPropertyName = "Source";

            // dgvMain.Columns["Down"].Visible = false;
            // dgvMain.Columns["GuidSN"].Visible = false;

            // 处理清空数值单元格时的异常，空值默认转为 0
            dgvMain.CellParsing += (s, e) =>
            {
                if (string.IsNullOrEmpty(e.Value?.ToString()))
                {
                    if (e.DesiredType == typeof(int) || e.DesiredType == typeof(decimal))
                    {
                        e.Value = 0;
                        e.ParsingApplied = true;
                    }
                }
            };

            SetDataGridViewStyle();
        }

        /// <summary>
        /// 设置DataGridView样式
        /// </summary>
        private void SetDataGridViewStyle()
        {
            // 设置整体外观
            dgvMain.BackgroundColor = System.Drawing.Color.White;
            dgvMain.BorderStyle = BorderStyle.FixedSingle;

            // 设置网格线
            dgvMain.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            dgvMain.GridColor = System.Drawing.Color.FromArgb(220, 220, 220);

            // 设置列标题样式
            dgvMain.EnableHeadersVisualStyles = false;
            dgvMain.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(70, 130, 180);
            dgvMain.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dgvMain.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            dgvMain.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

            // 设置数据行样式
            dgvMain.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            dgvMain.DefaultCellStyle.ForeColor = System.Drawing.Color.Black;
            dgvMain.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(220, 230, 240);
            dgvMain.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;
            dgvMain.DefaultCellStyle.Font = new System.Drawing.Font("微软雅黑", 9F);

            // 设置交替行颜色
            dgvMain.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(248, 248, 248);

            // 设置行高
            dgvMain.RowTemplate.Height = 25;

            // 设置选择模式
            dgvMain.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // 其他设置
            dgvMain.AllowUserToAddRows = false;
            //dgvMain.ReadOnly = true;
            dgvMain.AllowUserToResizeRows = false;
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        private void LoadData()
        {
            //_param = "[{\"MaterialId\":\"4300322\",\"MaterialCode\":\"806014736\",\"MaterialName\":\"8CW1-1-3外壳\",\"MaterialSpec\":\"QZT1-0151-0-01\",\"Supplier\":\"河北烁展仪器仪表制造有限公司\",\"Down\":0,\"Up\":59,\"Price\":95.00,\"Unit\":\"Pcs\",\"Source\":\"当前数据\",\"Flag\":\"Flag=1\",\"BillNo\":\"CGDD023599\",\"Qty\":59,\"FentryId\":\"169170\"},{\"MaterialId\":\"2893542\",\"MaterialCode\":\"104019864\",\"MaterialName\":\"PCB板-STND9MLZ(集成变换器国产化芯片验证)\",\"MaterialSpec\":\"STND9MLZ(集成变换器国产化芯片验证)\",\"Supplier\":\"深圳嘉立创科技集团股份有限公司\",\"Down\":0,\"Up\":5,\"Price\":39.77,\"Unit\":\"Pcs\",\"Source\":\"当前数据\",\"Flag\":\"Flag=1\",\"BillNo\":\"CGDD023593\",\"Qty\":5,\"FentryId\":\"169161\"},{\"MaterialId\":\"4686803\",\"MaterialCode\":\"107011486\",\"MaterialName\":\"高温有源晶振\",\"MaterialSpec\":\"XH41L-42GX-16.000MHz\",\"Supplier\":\"芯睿尚宁（北京）有限公司\",\"Down\":0,\"Up\":12,\"Price\":3820.00,\"Unit\":\"Pcs\",\"Source\":\"当前数据\",\"Flag\":\"Flag=1\",\"BillNo\":\"CGDD023600\",\"Qty\":12,\"FentryId\":\"169171\"}]";
            var data = _service.ParseFromParam(_param);

            // 将 Up 的值搬到 Qty，仅当 Source 为"当前数据"时 Up 置 0
            data.ForEach(x => { x.Qty = x.Up; if (x.Source == "当前数据") x.Up = 0; });

            _incomingOriginalData = data.Select(item => new RuleDto
            {
                GuidSN = item.GuidSN,
                MaterialId = item.MaterialId,
                MaterialCode = item.MaterialCode,
                MaterialName = item.MaterialName,
                MaterialSpec = item.MaterialSpec,
                Supplier = item.Supplier,
                Down = item.Down,
                Up = item.Up,
                Qty = item.Qty,
                Price = item.Price,
                Unit = item.Unit,
                Source = item.Source,
                BillNo = item.BillNo,
                FentryId = item.FentryId

            }).ToList();

            _list = new BindingList<RuleDto>(data);
            dgvMain.DataSource = _list;

            //
            foreach (var item in _incomingOriginalData)
            {
                SupplierMaterial supplierMaterial = new SupplierMaterial
                {
                    Supplier = item.Supplier,
                    MaterialCode = item.MaterialCode,
                    MaterialName = item.MaterialName,
                    MaterialSpec = item.MaterialSpec,
                    Unit = item.Unit
                };

                bool found = false;
                foreach (var sm in Suppliers)
                {
                    if (sm.Supplier == supplierMaterial.Supplier && sm.MaterialCode == supplierMaterial.MaterialCode)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found) Suppliers.Add(supplierMaterial);
            }

            FilterSupplier();

            if (Suppliers.Count > 0)
                MarkAsViewed(Suppliers[0]);
        }
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            ExitHelper.ExitSystem();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = System.DateTime.Now.ToString();
        }

        // 删除行功能
        private void btn_del_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvMain.SelectedRows.Count == 0)
                {
                    MessageBox.Show(
                        "请先选择要删除的行！",
                        "提示",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                DialogResult result = MessageBox.Show(
                    "确定要删除选中的行吗？",
                    "确认删除",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                {
                    return;
                }

                string guidSN = dgvMain.SelectedRows[0]
                    .Cells["GuidSN"]
                    .Value?
                    .ToString();

                bool success = RuleHelper.DeleteRow(_list, guidSN);

                if (!success)
                {
                    MessageBox.Show(
                        "删除失败，未找到对应数据！",
                        "提示",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                FilterSupplier();

                // 当前页是否已经删空
                bool exists = _list.Any(x =>
                    x.Supplier == Suppliers[index].Supplier
                    && x.MaterialCode == Suppliers[index].MaterialCode);

            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"删除失败：{ex.Message}",
                    "错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // 插入行功能
        private void btn_ins_Click(object sender, EventArgs e)
        {
            try
            {
                bool hasVisibleRow = dgvMain.Rows
                    .Cast<DataGridViewRow>()
                    .Any(r => r.Visible);

                RuleDto selectedRule = dgvMain.CurrentRow?.DataBoundItem as RuleDto;

                int insertIndex = dgvMain.CurrentRow != null
                    ? dgvMain.CurrentRow.Index + 1
                    : -1;

                RuleHelper.InsertRow(
                    _list,
                    selectedRule,
                    Suppliers,
                    index,
                    hasVisibleRow,
                    insertIndex);

                FilterSupplier();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"插入行失败：{ex.Message}",
                    "错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // 保存功能
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (Suppliers == null || Suppliers.Count == 0)
            {
                MessageBox.Show(
                    "当前没有分页数据！",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            try
            {
                dgvMain.EndEdit();
                dgvMain.CommitEdit(DataGridViewDataErrorContexts.Commit);

                BindingManagerBase bm = BindingContext[dgvMain.DataSource];
                if (bm != null)
                {
                    bm.EndCurrentEdit();
                }

                // ==================== 数据是否发生改变 ====================
                //if (!IsDataChanged())
                //{
                //    DialogResult result = MessageBox.Show(
                //    "当前数据未做任何修改操作，是否仍要保存？",
                //    "提示",
                //    MessageBoxButtons.YesNo,
                //    MessageBoxIcon.Question);

                //    if (result == DialogResult.No)
                //    {
                //        return;
                //    }
                //}

                // ==================== 没有数据 ====================
                if (_list == null || _list.Count == 0)
                {
                    MessageBox.Show("没有数据可以保存！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // ==================== 当前页数据 ====================
                var currentData = RuleHelper.GetCurrentPageData(_list, Suppliers[index]);

                // 给每行打上网格原始行号
                for (int i = 0; i < currentData.Count; i++)
                {
                    currentData[i].RowIndex = i + 1;
                }

                // ==================== 校验链 ====================
                if (!ValidateHelper.ValidateRequiredFields(
                currentData,
                msg => MessageBox.Show(msg, "提示"),
                msg => MessageBox.Show(msg, "确认", MessageBoxButtons.YesNo) == DialogResult.Yes))
                {
                    return;
                }

                if (!ValidateHelper.ValidateUpUnique(currentData,
                msg => MessageBox.Show(msg)))
                {
                    return;
                }

                var sortedByUp = currentData.OrderBy(x => x.Up).ToList();

                if (!ValidateHelper.ValidatePriceDescendingAfterSort(sortedByUp,
                         msg => MessageBox.Show(msg)))
                {
                    return;
                }

                var finalData = ValidateHelper.GenerateDownAndCheckConflict(
                                sortedByUp,
                                msg => MessageBox.Show(msg));
                if (finalData == null) return;

                //if (_ruleService.CheckDataAlreadyExists(finalData))
                //{
                //    MessageBox.Show("当前信息已保存！", "提示",
                //        MessageBoxButtons.OK, MessageBoxIcon.Information);

                //    return;
                //}

                var missing = _ruleService.GetMissingHistoryData(finalData);

                /*if (missing.Count > 0)
                {
                    string message = $"【温馨提示】\n\n共检测到 {missing.Count} 行数据未找到历史价格信息：\n\n";

                    int displayCount = Math.Min(missing.Count, 10);

                    for (int i = 0; i < displayCount; i++)
                    {
                        message += $"• {missing[i]}\n";
                    }

                    if (missing.Count > 10)
                    {
                        message += $"\n... 还有 {missing.Count - 10} 行未列出";
                    }

                    message += $"\n\n是否继续保存？";

                    DialogResult result = MessageBox.Show(
                        message,
                        "历史数据提醒",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result != DialogResult.Yes)
                    {
                        return;
                    }
                }*/

                // ==================== 保存数据库 ====================
                _ruleService.SaveToDatabase(
                    finalData,
                    Suppliers[index].Supplier,
                    finalData[0].MaterialId,
                    _originalData);

                if (_incomingOriginalData != null && _incomingOriginalData.Count > 0)
                {
                    var updateGroups = _incomingOriginalData
                        .Where(x => !string.IsNullOrEmpty(x.BillNo)
                            && !string.IsNullOrEmpty(x.MaterialId)
                            && x.Supplier == Suppliers[index].Supplier
                            && x.MaterialCode == Suppliers[index].MaterialCode)
                        .GroupBy(x => new { x.BillNo, x.MaterialId });

                    string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    foreach (var group in updateGroups)
                    {
                        string billNo = group.Key.BillNo;
                        string materialId = group.Key.MaterialId;

                        string updateSql = $@"
                        UPDATE T_PUR_POORDERENTRY 
                        SET FTIME = '已设置' 
                        WHERE FMATERIALID = '{materialId}' 
                        AND FID IN (SELECT DISTINCT FID FROM T_PUR_POORDER WHERE FBILLNO = '{billNo}')";

                        int affectedRows = DbHelper.ExecuteNonQuery(updateSql);

                        // 查询采购订单的含税单价、数量、供应商名称
                        string poQuery = $@"
                        SELECT A.FENTRYID, B.FTAXPRICE, A.FQTY, S.FNAME
                        FROM T_PUR_POORDER P
                        JOIN T_PUR_POORDERENTRY A ON P.FID = A.FID
                        JOIN T_PUR_POORDERENTRY_F B ON A.FENTRYID = B.FENTRYID
                        JOIN T_BD_SUPPLIER_L S ON P.FSUPPLIERID = S.FSUPPLIERID
                        WHERE A.FMATERIALID = '{materialId}' AND P.FBILLNO = '{billNo}'";

                        DataTable poData = DbHelper.ExecuteQuery(poQuery);

                        if (poData != null && poData.Rows.Count > 0)
                        {
                            string supplierName = poData.Rows[0]["FNAME"].ToString();

                            // 查询免审规则表获取上限和价格
                            string ruleQuery = $@"
                            SELECT F_ZMER_TEXT_QTR, F_ZMER_TEXT_83G, FTAXPRICE
                            FROM ZMER_t_Cust_Entry100101
                            WHERE FMATERIALID = '{materialId}' AND FSUPPLIER = '{supplierName}'";

                            DataTable ruleData = DbHelper.ExecuteQuery(ruleQuery);

                            if (ruleData != null && ruleData.Rows.Count > 0)
                            {
                                decimal ruleLower = Convert.ToDecimal(ruleData.Rows[0]["F_ZMER_TEXT_QTR"]);
                                decimal ruleUpper = Convert.ToDecimal(ruleData.Rows[0]["F_ZMER_TEXT_83G"]);
                                decimal rulePrice = Convert.ToDecimal(ruleData.Rows[0]["FTAXPRICE"]);

                                foreach (DataRow poRow in poData.Rows)
                                {
                                    decimal poQty = Convert.ToDecimal(poRow["FQTY"]);
                                    decimal poPrice = Convert.ToDecimal(poRow["FTAXPRICE"]);
                                    string fentryId = poRow["FENTRYID"].ToString();

                                    // 判断采购数量是否在规则范围内
                                    if (poQty > ruleLower && poQty <= ruleUpper)
                                    {
                                        string colorFlag;
                                        if (poPrice > rulePrice)
                                        {
                                            colorFlag = "命中不符合规则";
                                        }
                                        else if (poPrice <= rulePrice)
                                        {
                                            colorFlag = "符合规则";
                                        }
                                        else
                                        {
                                            continue;
                                        }

                                        string updateColorSql = $@"
                                        UPDATE T_PUR_POORDERENTRY
                                        SET FCOLORFLAG = '{colorFlag}'
                                        WHERE FENTRYID = '{fentryId}'";

                                        DbHelper.ExecuteNonQuery(updateColorSql);
                                    }
                                }
                            }
                        }
                    }
                }

                // ==================== 同步页面数据 ====================
                for (int i = 0; i < finalData.Count; i++)
                {
                    var item = finalData[i];

                    var target = _list.FirstOrDefault(x =>
                        x.Supplier == item.Supplier
                        && x.MaterialCode == item.MaterialCode
                        && x.Up == item.Up);

                    if (target != null)
                    {
                        target.Down = item.Down;
                        target.Up = item.Up;
                        target.Price = item.Price;
                        target.Unit = item.Unit;
                        target.Source = item.Source;
                        target.Qty = item.Qty;
                    }
                }

                // ==================== 更新_originalData ====================
                _originalData.RemoveAll(x =>
                    x.Supplier == Suppliers[index].Supplier
                    && x.MaterialId == Suppliers[index].MaterialCode);

                _originalData.AddRange(finalData.Select(item => new RuleDto
                {
                    MaterialId = item.MaterialId,
                    MaterialCode = item.MaterialCode,
                    MaterialName = item.MaterialName,
                    MaterialSpec = item.MaterialSpec,
                    Supplier = item.Supplier,
                    Down = item.Down,
                    Up = item.Up,
                    Qty = item.Qty,
                    Price = item.Price,
                    Unit = item.Unit,
                    Source = item.Source,
                    BillNo = item.BillNo
                }));

                if (index >= 0 && index < Suppliers.Count)
                {
                    Suppliers[index].IsSaved = true;
                }


                ColorSavedRows();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 给当前保存的供应商数据染色
        /// </summary>
        private void ColorSavedRows()
        {
            // 获取当前供应商和物料编码
            string currentSupplier = Suppliers[index].Supplier;
            string currentMaterialCode = Suppliers[index].MaterialCode;

            // 浅绿色
            Color savedColor = Color.FromArgb(198, 239, 206);

            foreach (DataGridViewRow row in dgvMain.Rows)
            {
                if (row.IsNewRow) continue;

                string rowSupplier = row.Cells["Supplier"].Value?.ToString();
                string rowMaterialCode = row.Cells["MaterialId"].Value?.ToString();

                // 匹配当前保存数据
                if (rowSupplier == currentSupplier &&
                    rowMaterialCode == currentMaterialCode)
                {
                    // 普通状态颜色
                    row.DefaultCellStyle.BackColor = savedColor;
                    row.DefaultCellStyle.ForeColor = Color.Black;

                    // 选中状态颜色（关键）
                    row.DefaultCellStyle.SelectionBackColor = savedColor;
                    row.DefaultCellStyle.SelectionForeColor = Color.Black;
                }
            }
        }

        /// <summary>
        /// 校验数据是否发生改变
        /// 不按索引比较，按业务主键比较
        /// </summary>
        private bool IsDataChanged()
        {
            // 数量不同
            if (_originalData.Count != _list.Count)
            {
                return true;
            }

            foreach (var current in _list)
            {
                // 按业务唯一键查找
                var original = _originalData.FirstOrDefault(x =>
                    x.Supplier == current.Supplier
                    && x.MaterialId == current.MaterialCode
                    && x.Up == current.Up);

                // 找不到
                if (original == null)
                {
                    return true;
                }

                // 比较字段
                if (current.MaterialCode != original.MaterialId ||
                    current.MaterialName != original.MaterialName ||
                    current.MaterialSpec != original.MaterialSpec ||
                    current.Supplier != original.Supplier ||
                    current.Down != original.Down ||
                    current.Up != original.Up ||
                    current.Price != original.Price ||
                    current.Unit != original.Unit ||
                    current.Source != original.Source)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 校验所有分页数据是否全部已保存到数据库
        /// </summary>
        private bool ValidateAllPagesSaved()
        {
            if (_list == null || _list.Count == 0)
            {
                return true;
            }

            List<string> notSavedList = new List<string>();

            var skipList = Suppliers
    .Where(x => x.SkipValidate)
    .ToList();

            foreach (var item in _list)
            {
                bool skip = skipList.Any(x =>
    x.Supplier == item.Supplier
    && x.MaterialCode == item.MaterialCode);

                if (skip)
                {
                    continue;
                }

                string sql = $@"
SELECT COUNT(1)
FROM ZMER_t_Cust_Entry100101 A
INNER JOIN T_BD_MATERIAL B ON A.FMATERIALID = B.FMATERIALID
WHERE A.FSUPPLIER = N'{item.Supplier?.Replace("'", "''")}'
AND B.FNUMBER = N'{item.MaterialCode?.Replace("'", "''")}'
AND A.F_ZMER_TEXT_83G = {item.Up}
AND A.FTAXPRICE = {item.Price}
";

                object result = DbHelper.ExecuteScalar(sql);
                int count = Convert.ToInt32(result);

                if (count == 0)
                {
                    notSavedList.Add(
                        $"供应商:{item.Supplier} | 物料:{item.MaterialCode} | 上限:{item.Up} | 价格:{item.Price}"
                    );
                }
            }

            if (notSavedList.Count > 0)
            {
                return false;
            }

            return true;
        }

        private void tsmi_history_Click(object sender, EventArgs e)
        {
            dgvMain.EndEdit();
            // ==================== 先检查全量是否已保存 ====================
            if (!CheckCanContinue())
            {
                return;
            }

            if (dgvMain.CurrentRow == null)
            {
                MessageBox.Show("请先选中一条供应商数据！",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            string supplierName = dgvMain.CurrentRow.Cells["Supplier"].Value?.ToString();

            if (string.IsNullOrEmpty(supplierName))
            {
                MessageBox.Show("当前选中行的供应商名称为空！",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            List<string> selectedMaterialIds = GetSelectedMaterialIds(supplierName);

            foreach (Form form in Application.OpenForms)
            {
                if (form is HistoryForms)
                {
                    form.Activate();
                    return;
                }
            }

            HistoryForms historyForm = new HistoryForms(supplierName, selectedMaterialIds);

            historyForm.Owner = this;

            historyForm.Show();
        }

        /// <summary>
        /// 获取当前供应商已经选择过的物料编码列表
        /// </summary>
        private List<string> GetSelectedMaterialIds(string supplierName)
        {
            List<string> selectedIds = new List<string>();

            // 遍历首页表格中当前供应商的所有行
            foreach (var item in _list)
            {
                if (item.Supplier == supplierName && !string.IsNullOrEmpty(item.MaterialId))
                {
                    selectedIds.Add(item.MaterialId);
                }
            }
            selectedIds = selectedIds.Distinct().ToList();
            return selectedIds;
        }

        // 接收从 HistoryForms 返回的数据
        public void AddSelectedData(DataTable selectedData, string supplierName)
        {
            try
            {
                // ==================== 1. 清空旧数据 ====================

                _list?.Clear();

                Suppliers?.Clear();

                index = 0;

                // ==================== 2. 重新组装数据 ====================

                List<RuleDto> newData = new List<RuleDto>();

                foreach (DataRow row in selectedData.Rows)
                {
                    RuleDto newRule = new RuleDto
                    {
                        Supplier = supplierName,

                        MaterialId = row["物料编码"].ToString(),

                        MaterialCode = row["物料编码"].ToString(),

                        MaterialName = row["物料名称"].ToString(),

                        MaterialSpec = row["规格"].ToString(),

                        Qty = Convert.ToInt32(row["采购数量"]),

                        Up = 0,

                        Price = Convert.ToDecimal(row["含税单价"]),

                        Source = "供应商历史采购"
                    };

                    newData.Add(newRule);
                }

                // ==================== 3. 重新绑定数据 ====================

                _list = new BindingList<RuleDto>(newData);

                dgvMain.DataSource = _list;

                // ==================== 4. 重新生成分页（供应商+物料） ====================

                foreach (var item in newData)
                {
                    SupplierMaterial supplierMaterial = new SupplierMaterial
                    {
                        Supplier = item.Supplier,
                        MaterialCode = item.MaterialCode,
                        SkipValidate = true
                    };

                    bool exists = Suppliers.Any(x =>
                        x.Supplier == supplierMaterial.Supplier
                        && x.MaterialCode == supplierMaterial.MaterialCode);

                    if (!exists)
                    {
                        Suppliers.Add(supplierMaterial);
                    }
                }

                // ==================== 5. 重新生成原始数据（非常关键） ====================

                _originalData = newData.Select(item => new RuleDto
                {
                    MaterialId = item.MaterialId,
                    MaterialCode = item.MaterialCode,
                    MaterialName = item.MaterialName,
                    MaterialSpec = item.MaterialSpec,
                    Supplier = item.Supplier,
                    Down = item.Down,
                    Up = item.Up,
                    Qty = item.Qty,
                    Price = item.Price,
                    Unit = item.Unit,
                    Source = item.Source
                }).ToList();

                // ==================== 6. 刷新分页显示 ====================

                if (Suppliers.Count > 0)
                {
                    FilterSupplier();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"加载历史数据失败：{ex.Message}",
                    "错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        public void AddSelectedData1(DataTable selectedData, string supplierName)
        {
            List<RuleDto> newItems = new List<RuleDto>();

            foreach (DataRow row in selectedData.Rows)
            {
                RuleDto newRule = new RuleDto
                {
                    Supplier = supplierName,
                    MaterialCode = row["物料编码"].ToString(),
                    MaterialName = row["物料名称"].ToString(),
                    MaterialSpec = row["规格"].ToString(),
                    Qty = Convert.ToInt32(row["采购数量"]),
                    Up = 0,
                    Price = Convert.ToDecimal(row["含税单价"]),
                    Unit = row["单位"]?.ToString() ?? "",
                    Source = "历史免审采购信息"

                };

                _list.Add(newRule);
                newItems.Add(newRule);
            }

            // 同步 Suppliers 分页结构
            foreach (var item in newItems)
            {
                bool exists = Suppliers.Any(x =>
                    x.Supplier == item.Supplier
                    && x.MaterialCode == item.MaterialCode);

                if (!exists)
                {
                    Suppliers.Add(new SupplierMaterial
                    {
                        Supplier = item.Supplier,
                        MaterialCode = item.MaterialCode,
                        MaterialName = item.MaterialName,
                        MaterialSpec = item.MaterialSpec
                    });
                }
            }

            // 同步 _originalData
            if (_originalData != null)
            {
                _originalData.AddRange(newItems.Select(item => new RuleDto
                {
                    MaterialId = item.MaterialId,
                    MaterialCode = item.MaterialCode,
                    MaterialName = item.MaterialName,
                    MaterialSpec = item.MaterialSpec,
                    Supplier = item.Supplier,
                    Down = item.Down,
                    Up = item.Up,
                    Qty = item.Qty,
                    Price = item.Price,
                    Unit = item.Unit,
                    Source = item.Source
                }));
            }

            // 切换到新加数据所在的分页
            if (newItems.Count > 0)
            {
                var firstItem = newItems[0];
                int newIndex = Suppliers.FindIndex(x =>
                    x.Supplier == firstItem.Supplier
                    && x.MaterialCode == firstItem.MaterialCode);
                if (newIndex >= 0) index = newIndex;
            }

            // 刷新分页显示
            FilterSupplier();

            if (_list.Count > 0)
            {
                //dgvMain.CurrentCell = dgvMain.Rows[_list.Count - 1].Cells[0];
                var firstVisibleColumn = dgvMain.Columns.Cast<DataGridViewColumn>()
        .FirstOrDefault(c => c.Visible);

                if (firstVisibleColumn != null)
                {
                    dgvMain.CurrentCell = dgvMain.Rows[_list.Count - 1].Cells[firstVisibleColumn.Index];
                }
                else
                {
                    dgvMain.CurrentCell = dgvMain.Rows[_list.Count - 1].Cells[0];
                }
                dgvMain.FirstDisplayedScrollingRowIndex = _list.Count - 1;
            }
        }


        private void tsmi_unaudit_gys_Click(object sender, EventArgs e)
        {
            dgvMain.EndEdit();
            // ==================== 先检查全量是否已保存 ====================
            if (!CheckCanContinue())
            {
                return;
            }

            if (dgvMain.CurrentRow == null)
            {
                MessageBox.Show("请先选中一条供应商数据！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string supplierName = dgvMain.CurrentRow.Cells["Supplier"].Value?.ToString();

            if (string.IsNullOrEmpty(supplierName))
            {
                MessageBox.Show("当前选中行的供应商名称为空！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<string> selectedMaterialCodes = GetSelectedUnAuditMaterialCodes(supplierName);

            foreach (Form form in Application.OpenForms)
            {
                if (form is SupplierUnAuditInfo)
                {
                    form.Activate();
                    return;
                }
            }

            SupplierUnAuditInfo historyForm = new SupplierUnAuditInfo(supplierName, selectedMaterialCodes);
            historyForm.Owner = this;
            historyForm.Show();
        }

        private void tsmi_unaudit_all_Click(object sender, EventArgs e)
        {
            dgvMain.EndEdit();
            // ==================== 先检查全量是否已保存 ====================
            if (!CheckCanContinue())
            {
                return;
            }

            List<string> selectedMaterialCodes = GetSelectedAllUnAuditMaterialCodes();

            foreach (Form form in Application.OpenForms)
            {
                if (form is AllUnAuditInfo)
                {
                    form.Activate();
                    return;
                }
            }

            AllUnAuditInfo form1 = new AllUnAuditInfo(selectedMaterialCodes);
            form1.Owner = this;
            form1.Show();
        }

        private void tsmi_history_purunaudit_info_Click(object sender, EventArgs e)
        {
            dgvMain.EndEdit();
            // ==================== 先检查全量是否已保存 ====================
            if (!CheckCanContinue())
            {
                return;
            }

            PurHistoryUnAuditInfo purHistoryUnAuditInfo = new PurHistoryUnAuditInfo();
            purHistoryUnAuditInfo.Owner = this;
            purHistoryUnAuditInfo.Show();
        }

        /// <summary>
        /// 获取当前供应商已经选择过的免审物料编码列表
        /// </summary>
        private List<string> GetSelectedUnAuditMaterialCodes(string supplierName)
        {
            List<string> selectedCodes = new List<string>();

            foreach (var item in _list)
            {
                if (item.Supplier == supplierName && item.Source == "供应商免审" && !string.IsNullOrEmpty(item.MaterialId))
                {
                    selectedCodes.Add(item.MaterialId);
                }
            }

            return selectedCodes.Distinct().ToList();
        }

        /// <summary>
        /// 接收从 SupplierUnAuditInfo 返回的免审数据
        /// </summary>
        public void AddSelectedUnAuditData(DataTable selectedData, string supplierName)
        {
            // ==================== 1. 清空旧状态 ====================
            _list?.Clear();
            _originalData?.Clear();
            Suppliers?.Clear();
            index = 0;

            // ==================== 2. 重建数据 ====================
            List<RuleDto> newData = new List<RuleDto>();

            foreach (DataRow row in selectedData.Rows)
            {
                newData.Add(new RuleDto
                {
                    Supplier = supplierName,
                    MaterialCode = row["物料编码"].ToString(),
                    MaterialName = row["物料名称"].ToString(),
                    MaterialSpec = row["规格"].ToString(),
                    Qty = Convert.ToInt32(row["采购数量"]),
                    Up = 0,
                    Price = Convert.ToDecimal(row["含税单价"]),
                    Unit = row["单位"].ToString(),
                    Source = "供应商免审"
                });
            }

            // ==================== 3. 重新绑定 ====================
            _list = new BindingList<RuleDto>(newData);
            dgvMain.DataSource = _list;

            // ==================== 4. 重建分页结构 ====================
            foreach (var item in newData)
            {
                Suppliers.Add(new SupplierMaterial
                {
                    Supplier = item.Supplier,
                    MaterialCode = item.MaterialCode,
                    MaterialName = item.MaterialName,
                    MaterialSpec = item.MaterialSpec,
                    Unit =item.Unit,
                    SkipValidate = true
                });
            }

            Suppliers = Suppliers
                .GroupBy(x => new { x.Supplier, x.MaterialCode })
                .Select(g => g.First())
                .ToList();

            // ==================== 5. 重建原始数据 ====================
            _originalData = newData.Select(item => new RuleDto
            {
                MaterialId = item.MaterialId,
                MaterialCode = item.MaterialCode,
                MaterialName = item.MaterialName,
                MaterialSpec = item.MaterialSpec,
                Supplier = item.Supplier,
                Down = item.Down,
                Up = item.Up,
                Qty = item.Qty,
                Price = item.Price,
                Unit = item.Unit,
                Source = item.Source
            }).ToList();

            // ==================== 6. 刷新UI ====================
            if (Suppliers.Count > 0)
            {
                FilterSupplier();
            }
        }

        /// <summary>
        /// 获取已经选择过的全部免审物料编码列表（用于排除）
        /// </summary>
        private List<string> GetSelectedAllUnAuditMaterialCodes()
        {
            List<string> selectedCodes = new List<string>();

            // 遍历首页表格中所有免审来源的行
            foreach (var item in _list)
            {
                if (item.Source == "全部免审" && !string.IsNullOrEmpty(item.MaterialId))
                {
                    selectedCodes.Add(item.MaterialId);
                }
            }

            return selectedCodes.Distinct().ToList();
        }

        /// <summary>
        /// 接收从 AllUnAuditInfo 返回的全部免审数据
        /// </summary>
        public void AddSelectedAllUnAuditData(DataTable selectedData)
        {
            _list?.Clear();
            _originalData?.Clear();
            Suppliers?.Clear();
            index = 0;

            List<RuleDto> newData = new List<RuleDto>();

            foreach (DataRow row in selectedData.Rows)
            {
                newData.Add(new RuleDto
                {
                    Supplier = row["供应商"].ToString(),
                    MaterialCode = row["物料编码"].ToString(),
                    MaterialName = row["物料名称"].ToString(),
                    MaterialSpec = row["规格"].ToString(),
                    Qty = Convert.ToInt32(row["上限"]),
                    Up = 0,
                    Price = Convert.ToDecimal(row["含税单价"]),
                    Unit = row["单位"].ToString(),
                    Source = "全部免审"
                });
            }

            _list = new BindingList<RuleDto>(newData);
            dgvMain.DataSource = _list;

            Suppliers = newData
                .Select(x => new SupplierMaterial
                {
                    Supplier = x.Supplier,
                    MaterialCode = x.MaterialCode,
                    SkipValidate = true
                })
                .GroupBy(x => new { x.Supplier, x.MaterialCode })
                .Select(g => g.First())
                .ToList();

            _originalData = newData;

            if (Suppliers.Count > 0)
            {
                FilterSupplier();
            }
        }

        private void btn_clear_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show("确定要清空所有数据吗？", "确认清空",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                {
                    return;
                }

                _list?.Clear();

                _originalData?.Clear();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"清空数据失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (index <= Suppliers.Count - 2)
            {
                index += 1;
                FilterSupplier();
                MarkAsViewed(Suppliers[index]);
            }
            if (index == Suppliers.Count - 1) toolStripButNext.Enabled = false;
            toolStripButPre.Enabled = true;
        }

        private void toolStripButPre_Click(object sender, EventArgs e)
        {
            if (index > 0)
            {
                index -= 1;
                FilterSupplier();
            }
            if (index == 0) toolStripButPre.Enabled = false;
            toolStripButNext.Enabled = true;
        }

        
        private void FilterSupplier()
        {
            PageHelper.FilterSupplier(dgvMain, Suppliers, index);
        }

        /// <summary>
        /// 标记当前供应商为已浏览
        /// </summary>
        private void MarkAsViewed(SupplierMaterial sm)
        {
            if (_incomingOriginalData == null || _incomingOriginalData.Count == 0)
                return;

            var fentryIds = _incomingOriginalData
                .Where(x => x.Supplier == sm.Supplier && x.MaterialCode == sm.MaterialCode
                    && !string.IsNullOrEmpty(x.FentryId))
                .Select(x => x.FentryId)
                .Distinct();

            foreach (var fentryId in fentryIds)
            {
                string sql = $@"UPDATE T_PUR_POORDERENTRY
                                SET FTIME = '已浏览'
                                WHERE FENTRYID = '{fentryId}' AND FTIME <> '已设置'";
                DbHelper.ExecuteNonQuery(sql);
            }
        }

        /// <summary>
        /// 检查是否允许继续进入其它页面
        /// </summary>
        private bool CheckCanContinue()
        {
            //if (ValidateAllPagesSaved())
            //    return true;

            if (index >= 0 && index < Suppliers.Count && Suppliers[index].IsSaved)
            {
                // 当前页已保存，直接放行
                return true;
            }


            //if (!IsDataChanged())
            //{
            //    MessageBox.Show("当前数据尚未保存。",
            //        "提示",
            //        MessageBoxButtons.OK,
            //        MessageBoxIcon.Information);

            //    return true;
            //}

            var modifiedList =
                ValidateHelper.GetModifiedDataDetails(_list.ToList(), _originalData);

            if (modifiedList.Count == 0)
                return true;

            string msg = "以下数据已修改但未保存：\n\n";

            int showCount = Math.Min(modifiedList.Count, 10);

            for (int i = 0; i < showCount; i++)
                msg += $"• {modifiedList[i]}\n";

            msg += "\n继续操作可能造成当前操作数据丢失，是否继续？";

            DialogResult result = MessageBox.Show(
                msg,
                "未保存数据提醒",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return false;

            index = 0;

            dgvMain.DataSource = _list;

            return true;
        }

        /// <summary>
        /// 只检查当前页数据是否发生变化
        /// </summary>
        private bool IsDataChangedForCurrentPage()
        {
            if (Suppliers == null || Suppliers.Count == 0 || index < 0 || index >= Suppliers.Count)
                return false;

            string currentSupplier = Suppliers[index].Supplier;
            string currentMaterialCode = Suppliers[index].MaterialCode;

            // 获取当前页的数据
            var currentPageData = _list.Where(x =>
                x.Supplier == currentSupplier &&
                x.MaterialCode == currentMaterialCode
            ).ToList();

            // 获取当前页的原始数据
            var originalPageData = _originalData.Where(x =>
                x.Supplier == currentSupplier &&
                x.MaterialId == currentMaterialCode
            ).ToList();

            // 数量不同说明有变化
            if (currentPageData.Count != originalPageData.Count)
                return true;

            // 逐条比较
            foreach (var current in currentPageData)
            {
                var original = originalPageData.FirstOrDefault(x => x.Up == current.Up);

                if (original == null)
                    return true;

                if (current.Price != original.Price ||
                    current.MaterialName != original.MaterialName ||
                    current.MaterialSpec != original.MaterialSpec ||
                    current.Unit != original.Unit ||
                    current.Source != original.Source)
                {
                    return true;
                }
            }

            return false;
        }
    }
}