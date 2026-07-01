using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Zitn_exe_App.Models;
using static Zitn_exe_App.Forms.IndexForm;

namespace Zitn_exe_App.Utils
{
    public static class PageHelper
    {
        // ==================== 根据当前分页过滤 ====================
        public static void FilterSupplier(
            DataGridView dgv,
            List<SupplierMaterial> suppliers,
            int index)
        {
            if (suppliers == null || suppliers.Count == 0)
            {
                dgv.ClearSelection();
                return;
            }

            if (index < 0) index = 0;
            if (index >= suppliers.Count) index = suppliers.Count - 1;

            CurrencyManager cm =
                (CurrencyManager)dgv.BindingContext[dgv.DataSource];

            cm.SuspendBinding();

            dgv.ClearSelection();

            var current = suppliers[index];

            foreach (DataGridViewRow row in dgv.Rows)
            {
                string supplier = row.Cells["Supplier"].Value?.ToString();
                string material = row.Cells["MaterialId"].Value?.ToString();

                row.Visible =
                    supplier == current.Supplier &&
                    material == current.MaterialCode;
            }

            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.Visible)
                {
                    row.Selected = true;
                    dgv.CurrentCell = row.Cells.Cast<DataGridViewCell>()
                        .FirstOrDefault(c => c.Visible);
                    break;
                }
            }

            cm.ResumeBinding();
        }
    }
}