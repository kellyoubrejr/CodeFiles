using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Zitn_exe_App.Data;

namespace Zitn_exe_App.Forms
{
    public partial class PurHistoryUnAuditInfo : Form
    {
        public PurHistoryUnAuditInfo()
        {
            InitializeComponent();

            // 设置表格为只读
            SetGridViewReadOnly();

            dgv_purall.CellClick += dgv_purall_CellClick;

            // 初始化日期控件（默认为空，表示使用近两个月）
            InitializeDateTimePicker();

            // 加载完成后自动查询近两个月数据
            this.Load += PurHistoryUnAuditInfo_Load;
        }

        /// <summary>
        /// 处理单元格点击事件
        /// </summary>
        private void dgv_purall_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // 检查是否点击了有效行和按钮列
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // 判断点击的是否是 setBtn 列
                if (dgv_purall.Columns[e.ColumnIndex].Name == "setBtn")
                {
                    // 获取当前行的数据
                    DataGridViewRow row = dgv_purall.Rows[e.RowIndex];

                    if (row.Cells["setBtn"].Value?.ToString() == "已点击")
                    {
                        MessageBox.Show("该条记录已经返回免审信息，无需点击！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // 提取数据
                    string supplierName = row.Cells["supplier"].Value?.ToString() ?? "";
                    string materialCode = row.Cells["wlnum"].Value?.ToString() ?? "";
                    string materialName = row.Cells["wlname"].Value?.ToString() ?? "";
                    string specification = row.Cells["wlspec"].Value?.ToString() ?? "";
                    decimal qty = Convert.ToDecimal(row.Cells["qty"].Value ?? 0);
                    decimal taxPrice = Convert.ToDecimal(row.Cells["taxprice"].Value ?? 0);
                    string danWei = (row.Tag as RowTagInfo)?.DanWei ?? "";

                    // 检查是否有打开的 IndexForm
                    IndexForm existingForm = null;
                    foreach (Form form in Application.OpenForms)
                    {
                        if (form is IndexForm)
                        {
                            existingForm = form as IndexForm;
                            existingForm.Activate();
                            break;
                        }
                    }

                    // 如果没有打开的 IndexForm，创建新的
                    if (existingForm == null)
                    {
                        // 注意：IndexForm 需要参数，这里传入空字符串或默认值
                        existingForm = new IndexForm("");
                        existingForm.Show();
                    }

                    // 调用 IndexForm 的方法添加数据
                    DataTable selectedData = new DataTable();
                    selectedData.Columns.Add("物料编码", typeof(string));
                    selectedData.Columns.Add("物料名称", typeof(string));
                    selectedData.Columns.Add("规格", typeof(string));
                    selectedData.Columns.Add("采购数量", typeof(decimal));
                    selectedData.Columns.Add("含税单价", typeof(decimal));
                    selectedData.Columns.Add("单位", typeof(string));

                    DataRow newRow = selectedData.NewRow();
                    newRow["物料编码"] = materialCode;
                    newRow["物料名称"] = materialName;
                    newRow["规格"] = specification;
                    newRow["采购数量"] = qty;
                    newRow["含税单价"] = taxPrice;
                    newRow["单位"] = danWei;
                    selectedData.Rows.Add(newRow);

                    // 调用 AddSelectedData 方法
                    existingForm.AddSelectedData1(selectedData, supplierName);

                    // 修改按钮文本为"已点击"
                    row.Cells["setBtn"].Value = "已点击";

                    row.Cells["setBtn"].Style.BackColor = System.Drawing.Color.LightGray;
                    row.Cells["setBtn"].Style.ForeColor = System.Drawing.Color.Gray;

                    // 可选：关闭当前窗体
                    //this.Close();
                }
            }
        }

        /// <summary>
        /// 窗体加载时自动查询近两个月数据
        /// </summary>
        private void PurHistoryUnAuditInfo_Load(object sender, EventArgs e)
        {
            // 自动加载近两个月数据
            LoadReportData();
        }

        /// <summary>
        /// 初始化日期控件
        /// </summary>
        private void InitializeDateTimePicker()
        {
            // 设置日期控件默认为空白显示
            //dtpStart.Format = DateTimePickerFormat.Custom;
            dtpStart.Format = DateTimePickerFormat.Short;
            dtpStart.CustomFormat = " ";
            //dtpStart.Value = DateTime.Now;
            // 修改这里：设置为当前日期减2个月
            dtpStart.Value = DateTime.Now.AddMonths(-2);
            dtpStart.CloseUp += dtpStart_CloseUp;
            dtpStart.ValueChanged += dtpStart_ValueChanged;
        }

        private void dtpStart_CloseUp(object sender, EventArgs e)
        {
            // 用户选择了日期后，恢复正常显示
            if (dtpStart.Format == DateTimePickerFormat.Custom && dtpStart.CustomFormat == " ")
            {
                dtpStart.Format = DateTimePickerFormat.Short;
                dtpStart.CustomFormat = null;
            }
        }

        /// <summary>
        /// 设置表格只读
        /// </summary>
        private void SetGridViewReadOnly()
        {
            dgv_purall.ReadOnly = true;
        }

        /// <summary>
        /// 获取查询的开始日期
        /// </summary>
        private object GetStartDate()
        {
            // 如果日期控件有选中的值（不是空白）
            if (dtpStart.Checked && dtpStart.Format != DateTimePickerFormat.Custom)
            {
                return dtpStart.Value.Date;
            }
            // 否则返回 DBNull，存储过程会使用默认值（近两个月）
            return DBNull.Value;
        }

        /// <summary>
        /// 日期控件选择值改变时，恢复正常显示格式
        /// </summary>
        private void dtpStart_ValueChanged(object sender, EventArgs e)
        {
            if (dtpStart.Format == DateTimePickerFormat.Custom && dtpStart.CustomFormat == " ")
            {
                // 恢复正常显示
                dtpStart.Format = DateTimePickerFormat.Short;
                dtpStart.CustomFormat = null;
            }
        }

        /// <summary>
        /// 调用存储过程加载数据
        /// </summary>
        private void LoadReportData()
        {
            try
            {
                object beginDate = GetStartDate();

                // 调用存储过程
                string sql = "EXEC ZMER_GetMatchedPO @BeginDate";

                var parameters = new Dictionary<string, object>
                {
                    { "@BeginDate", beginDate }
                };

                DataTable dt = DbHelper.ExecuteQuery(sql, parameters);
                FillDataGridView(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"查询报表数据失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 填充 DataGridView（带分组排序和渲染）
        /// </summary>
        private void FillDataGridView(DataTable dt)
        {
            // 清空表格现有数据
            dgv_purall.Rows.Clear();

            if (dt.Rows.Count == 0)
            {
                MessageBox.Show($"未查询到数据！\n", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 处理数据：分组、排序
            var processedData = ProcessAndSortData(dt);

            // 填充数据到 DataGridView
            foreach (var row in processedData)
            {
                int rowIndex = dgv_purall.Rows.Add();

                dgv_purall.Rows[rowIndex].Cells["pur"].Value = row["采购订单"]?.ToString() ?? "";
                dgv_purall.Rows[rowIndex].Cells["purdate"].Value = row["采购日期"] != DBNull.Value
                    ? Convert.ToDateTime(row["采购日期"]).ToString("yyyy-MM-dd") : "";
                dgv_purall.Rows[rowIndex].Cells["sqr"].Value = row["申请人"]?.ToString() ?? "";
                dgv_purall.Rows[rowIndex].Cells["syb"].Value = row["事业部"]?.ToString() ?? "";
                dgv_purall.Rows[rowIndex].Cells["buyer"].Value = row["采购员"]?.ToString() ?? "";
                dgv_purall.Rows[rowIndex].Cells["supplier"].Value = row["供应商"]?.ToString() ?? "";
                dgv_purall.Rows[rowIndex].Cells["wlnum"].Value = row["物料编码"]?.ToString() ?? "";
                dgv_purall.Rows[rowIndex].Cells["wlname"].Value = row["物料名称"]?.ToString() ?? "";
                dgv_purall.Rows[rowIndex].Cells["wlspec"].Value = row["规格型号"]?.ToString() ?? "";
                dgv_purall.Rows[rowIndex].Cells["qty"].Value = row["采购数量"]?.ToString() ?? "0";
                dgv_purall.Rows[rowIndex].Cells["taxprice"].Value = row["含税单价"]?.ToString() ?? "0";
                dgv_purall.Rows[rowIndex].Cells["note"].Value = row["备注"]?.ToString() ?? "";
                dgv_purall.Rows[rowIndex].Cells["fdate"].Value = row["规则日期"] != DBNull.Value
                    ? Convert.ToDateTime(row["规则日期"]).ToString("yyyy-MM-dd") : "";
                dgv_purall.Rows[rowIndex].Cells["setBtn"].Value = "设置免审";

                // 存储分组信息和价格，用于后续渲染
                string groupKey = $"{row["物料编码"]}_{row["供应商"]}";
                decimal price = Convert.ToDecimal(row["含税单价"] ?? 0);
                DateTime purDate = row["采购日期"] != DBNull.Value
                    ? Convert.ToDateTime(row["采购日期"])
                    : DateTime.MinValue;

                dgv_purall.Rows[rowIndex].Tag = new RowTagInfo
                {
                    GroupKey = groupKey,
                    Price = price,
                    PurDate = purDate,
                    DanWei = row["DANWEI"]?.ToString() ?? ""
                };
            }

            // 应用颜色渲染（检查价格是否逐行下降）
            HighlightPriceTrendRows();
        }

        /// <summary>
        /// 处理数据：按物料+供应商分组，组内按采购日期降序排序
        /// </summary>
        private List<DataRow> ProcessAndSortData(DataTable dt)
        {
            // 添加分组Key列
            dt.Columns.Add("GroupKey", typeof(string));

            // 为每一行生成分组Key（物料编码 + 供应商）
            foreach (DataRow row in dt.Rows)
            {
                string materialCode = row["物料编码"]?.ToString() ?? "";
                string supplier = row["供应商"]?.ToString() ?? "";
                string groupKey = $"{materialCode}_{supplier}";
                row["GroupKey"] = groupKey;
            }

            // 按分组Key分组，组内按采购日期降序排序（由近及远）
            var sortedRows = dt.AsEnumerable()
                .OrderBy(r => r["GroupKey"])                          // 先按分组排序
                .ThenByDescending(r => Convert.ToDateTime(r["采购日期"] ?? DateTime.MinValue)) // 组内按日期降序
                .ToList();

            return sortedRows;
        }

        /// <summary>
        /// 高亮显示价格趋势行（同一物料+供应商，价格随日期由近及远逐行比较）
        /// 当前价格 <= 上一行价格（持平或上涨）：绿色
        /// 当前价格 > 上一行价格（价格下降）：红色异常
        /// </summary>
        private void HighlightPriceTrendRows()
        {
            string currentGroupKey = "";
            decimal? previousPrice = null;

            foreach (DataGridViewRow row in dgv_purall.Rows)
            {
                if (row.IsNewRow) continue;

                RowTagInfo tagInfo = row.Tag as RowTagInfo;
                if (tagInfo == null) continue;

                // 如果分组发生变化，重置前一价格
                if (currentGroupKey != tagInfo.GroupKey)
                {
                    currentGroupKey = tagInfo.GroupKey;
                    previousPrice = null;
                    // 更新前一价格
                    previousPrice = tagInfo.Price;
                    continue;
                }

                // 有上一行价格，进行比较
                if (previousPrice.HasValue)
                {
                    // 当前价格 > 上一行价格（价格下降）-> 红色异常
                    if (tagInfo.Price > previousPrice.Value)
                    {
                        row.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(198, 239, 206);
                        row.DefaultCellStyle.ForeColor = System.Drawing.Color.Red;
                    }
                    // 当前价格 <= 上一行价格（价格持平或上涨）-> 绿色正常
                    else
                    {
                        row.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(255, 199, 206);
                        row.DefaultCellStyle.ForeColor = System.Drawing.Color.Black;
                    }
                }

                // 更新前一价格
                previousPrice = tagInfo.Price;
            }
        }

        /// <summary>
        /// 行标签信息类
        /// </summary>
        public class RowTagInfo
        {
            public string GroupKey { get; set; }
            public decimal Price { get; set; }
            public DateTime PurDate { get; set; }
            public string DanWei { get; set; }
        }

        /// <summary>
        /// 确认按钮点击事件（根据选中的日期重新查询数据）
        /// </summary>
        private void btnConfirm_Click(object sender, EventArgs e)
        {
            // 根据用户选择的日期重新查询
            LoadReportData();
        }
    }
}