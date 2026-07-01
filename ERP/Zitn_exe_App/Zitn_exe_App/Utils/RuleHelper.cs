using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Zitn_exe_App.Models;
using static Zitn_exe_App.Forms.IndexForm;

namespace Zitn_exe_App.Utils
{
    public static class RuleHelper
    {
        /// <summary>
        ///  插入行
        /// </summary>
        /// <param name="list"></param>
        /// <param name="selectedRule"></param>
        /// <param name="suppliers"></param>
        /// <param name="supplierIndex"></param>
        /// <param name="hasVisibleRow"></param>
        /// <param name="insertIndex"></param>
        public static void InsertRow(
            BindingList<RuleDto> list,
            RuleDto selectedRule,
            List<SupplierMaterial> suppliers,
            int supplierIndex,
            bool hasVisibleRow,
            int insertIndex = -1)
        {
            RuleDto newRule = new RuleDto();

            // ==================== 当前页有数据 ====================
            if (hasVisibleRow && selectedRule != null)
            {
                newRule.MaterialId = selectedRule.MaterialId ?? "";
                newRule.MaterialCode = selectedRule.MaterialCode ?? "";
                newRule.MaterialName = selectedRule.MaterialName ?? "";
                newRule.MaterialSpec = selectedRule.MaterialSpec ?? "";
                newRule.Supplier = selectedRule.Supplier ?? "";
                newRule.Unit = selectedRule.Unit ?? "";

                newRule.Source = "插入行";

                if (insertIndex >= 0)
                {
                    list.Insert(insertIndex, newRule);
                }
                else
                {
                    list.Add(newRule);
                }
            }
            // ==================== 当前页已经删空 ====================
            else
            {
                if (suppliers != null &&
                    suppliers.Count > 0 &&
                    supplierIndex >= 0 &&
                    supplierIndex < suppliers.Count)
                {
                    var currentSupplier = suppliers[supplierIndex];

                    newRule.Supplier = currentSupplier.Supplier ?? "";
                    newRule.MaterialId = currentSupplier.MaterialId ?? "";
                    newRule.MaterialCode = currentSupplier.MaterialCode ?? "";
                    newRule.MaterialName = currentSupplier.MaterialName ?? "";
                    newRule.MaterialSpec = currentSupplier.MaterialSpec ?? "";
                    newRule.Unit = currentSupplier.Unit ?? "";
                }

                newRule.Source = "插入行";

                list.Add(newRule);
            }
        }

        /// <summary>
        /// 根据 GuidSN 删除行
        /// </summary>
        /// <param name="list"></param>
        /// <param name="guidSN"></param>
        /// <returns></returns>
        public static bool DeleteRow(
        BindingList<RuleDto> list,
        string guidSN)
        {
            if (string.IsNullOrWhiteSpace(guidSN))
            {
                return false;
            }

            RuleDto deleteItem = list
                .FirstOrDefault(x => x.GuidSN == guidSN);

            if (deleteItem != null)
            {
                list.Remove(deleteItem);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取当前页数据
        /// </summary>
        /// <param name="list"></param>
        /// <param name="supplier"></param>
        /// <returns></returns>
        public static List<RuleDto> GetCurrentPageData(
        BindingList<RuleDto> list,
        SupplierMaterial supplier)
        {
            return list
                .Where(p =>
                    p.Supplier == supplier.Supplier
                    && p.MaterialCode == supplier.MaterialCode)
                .ToList();
        }

        /// <summary>
        /// 同步页面数据
        /// </summary>
        public static void SyncPageData(
            BindingList<RuleDto> list,
            List<RuleDto> finalData)
        {
            foreach (var item in finalData)
            {
                var target = list.FirstOrDefault(x =>
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
        }

        /// <summary>
        /// 更新原始数据
        /// </summary>
        public static void UpdateOriginalData(
            List<RuleDto> originalData,
            List<RuleDto> finalData,
            SupplierMaterial supplier)
        {
            originalData.RemoveAll(x =>
                x.Supplier == supplier.Supplier
                && x.MaterialCode == supplier.MaterialCode);

            originalData.AddRange(finalData.Select(item => new RuleDto
            {
                MaterialId = item.MaterialId,
                MaterialCode = item.MaterialCode,
                MaterialName = item.MaterialName,
                MaterialSpec = item.MaterialSpec,
                Supplier = item.Supplier,
                Down = item.Down,
                Up = 0,
                Qty = item.Up,
                Price = item.Price,
                Unit = item.Unit,
                Source = item.Source,
                BillNo = item.BillNo
            }));
        }
    }
}